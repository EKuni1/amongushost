// ═══════════════════════════════════════════════════════════════════
// Among Us Web Radar v2.0 — High-Performance Canvas Renderer
// ═══════════════════════════════════════════════════════════════════
// PERF 2026: Dirty-flag rendering, offscreen map cache, position
// interpolation, single-pass draw, zero per-frame allocations.
// ═══════════════════════════════════════════════════════════════════

(function () {
    'use strict';

    // ── CONFIG ──────────────────────────────────────────────────────
    // Auto-detect host + pass token from URL query params for remote access
    const _params = new URLSearchParams(window.location.search);
    const _token = _params.get('token');
    const _wsHost = window.location.host || 'localhost:9222';
    const WS_URL = 'ws://' + _wsHost + '/radar/ws' + (_token ? '?token=' + _token : '');
    const RECONNECT_DELAY_MS = 2000;
    const PLAYER_RADIUS = 7;
    const INTERP_SPEED = 12; // Interpolation smoothing (higher = faster snap)
    const NAME_FONT_SIZE = 11;
    const STATUS_FONT_SIZE = 9;
    const TITLE_FONT_SIZE = 13;
    const FONT_FAMILY = '"Syne Mono", "Courier New", monospace';
    const TEXT_STROKE = 'rgba(3, 6, 10, 0.92)';
    const TEXT_PRIMARY = 'rgba(255, 255, 255, 0.96)';
    const TEXT_SECONDARY = 'rgba(255, 255, 255, 0.84)';
    const TEXT_DEAD = 'rgba(255, 255, 255, 0.58)';
    const TEXT_MUTED = 'rgba(255, 255, 255, 0.64)';
    const VENT_TEXT = 'rgba(255, 196, 64, 0.96)';

    // Among Us color palette (indices 0-17)
    const PLAYER_COLORS = [
        '#c51111', '#132ed1', '#117f2d', '#ed54ba',
        '#ef7d0e', '#f5f557', '#3f474e', '#d6e0f0',
        '#6b2fbb', '#71491e', '#38fedc', '#50ef39',
        '#6b2833', '#ecc0d3', '#fffebe', '#708496',
        '#928776', '#ec7578',
    ];

    // Ring colors — no glow, pure geometry
    const IMPOSTOR_RING = '#ff3232';
    const LOCAL_RING    = 'rgba(255, 255, 255, 0.7)';

    // Game world bounds
    // Calibrated from F10 room collider dumps (2026-03-30)
    const MAP_BOUNDS = {
        Ship:    { minX: -25.5, maxX: 21.3, minY: -19.4, maxY: 8.7 },
        Hq:      { minX: -13.1, maxX: 30.9, minY: -5.3, maxY: 27.2 },
        Pb:      { minX: -1.4, maxX: 42.7, minY: -28.0, maxY: 3.7 },
        Airship: { minX: -26.7, maxX: 41.4, minY: -19.5, maxY: 19.1 },
        Fungle:  { minX: -25.5, maxX: 28.0, minY: -14.5, maxY: 18.0 },
    };

    // Image world bounds (map background positioning)
    const IMAGE_BOUNDS = {
        Ship: { minX: -23.56, maxX: 19.56, minY: -17.34, maxY: 7.34 },
    };

    // ── STATE ───────────────────────────────────────────────────────
    let canvas, ctx;
    let ws = null;
    let lastState = null;
    let prevState = null;       // Previous state for interpolation
    let lastStateTime = 0;      // When lastState was received (performance.now)
    let mapImages = {};
    let mapImagesLoaded = {};
    let reconnectTimer = null;
    let dirty = true;           // PERF: Only render when data changes

    // Offscreen map cache
    let mapCache = null;        // OffscreenCanvas or regular Canvas
    let mapCacheKey = '';       // "mapName:canvasW:canvasH" — invalidate on change
    let mapCacheCtx = null;

    // Interpolated player positions
    let interpPositions = {};   // playerId -> { x, y }

    // FPS tracking
    let frameCount = 0;
    let fpsTime = 0;
    let currentFps = 0;

    // UI elements
    let elStatus, elMapName, elPlayerCount, elFps;

    // ── INIT ────────────────────────────────────────────────────────
    function init() {
        canvas = document.getElementById('radar-canvas');
        ctx = canvas.getContext('2d');
        elStatus      = document.getElementById('connection-status');
        elMapName     = document.getElementById('map-name');
        elPlayerCount = document.getElementById('player-count');
        elFps         = document.getElementById('fps-counter');

        loadMapImages();
        resizeCanvas();
        window.addEventListener('resize', function () {
            resizeCanvas();
            invalidateMapCache();
            dirty = true;
        });
        connectWs();
        requestAnimationFrame(renderLoop);
    }

    function loadMapImages() {
        if (typeof RADAR_MAP_DATA === 'undefined') return;
        var keys = Object.keys(RADAR_MAP_DATA);
        for (var i = 0; i < keys.length; i++) {
            (function (key) {
                var img = new Image();
                img.onload = function () {
                    mapImagesLoaded[key] = true;
                    invalidateMapCache();
                    dirty = true;
                };
                img.onerror = function () { mapImagesLoaded[key] = false; };
                img.src = RADAR_MAP_DATA[key];
                mapImages[key] = img;
            })(keys[i]);
        }
    }

    function resizeCanvas() {
        canvas.width = canvas.clientWidth;
        canvas.height = canvas.clientHeight;
    }

    function invalidateMapCache() {
        mapCacheKey = '';
    }

    // ── WEBSOCKET ───────────────────────────────────────────────────
    function connectWs() {
        if (ws && (ws.readyState === WebSocket.OPEN || ws.readyState === WebSocket.CONNECTING)) return;

        try {
            ws = new WebSocket(WS_URL);
        } catch (e) {
            scheduleReconnect();
            return;
        }

        ws.onopen = function () {
            elStatus.textContent = 'online';
            elStatus.classList.add('connected');
            if (reconnectTimer) { clearTimeout(reconnectTimer); reconnectTimer = null; }
            dirty = true;
        };

        ws.onmessage = function (event) {
            try {
                prevState = lastState;
                lastState = JSON.parse(event.data);
                lastStateTime = performance.now();

                // Seed interpolation targets
                if (lastState && lastState.players) {
                    for (var i = 0; i < lastState.players.length; i++) {
                        var p = lastState.players[i];
                        if (!interpPositions[p.id]) {
                            interpPositions[p.id] = { x: p.x, y: p.y };
                        }
                    }
                }

                dirty = true;
            } catch (e) { /* ignore bad JSON */ }
        };

        ws.onclose = function () {
            elStatus.textContent = 'offline';
            elStatus.classList.remove('connected');
            scheduleReconnect();
            dirty = true;
        };

        ws.onerror = function () {
            try { ws.close(); } catch (e) { /* ignore */ }
        };
    }

    function scheduleReconnect() {
        if (reconnectTimer) return;
        reconnectTimer = setTimeout(function () {
            reconnectTimer = null;
            connectWs();
        }, RECONNECT_DELAY_MS);
    }

    // ── COORDINATE MAPPING ──────────────────────────────────────────
    function getMapBounds(mapName) {
        return MAP_BOUNDS[mapName] || MAP_BOUNDS.Ship;
    }

    function worldToCanvas(wx, wy, bounds, canvasW, canvasH) {
        var mapW = bounds.maxX - bounds.minX;
        var mapH = bounds.maxY - bounds.minY;
        var h = canvasH; // HUD is floating overlay — full height available
        var scaleX = canvasW / mapW;
        var scaleY = h / mapH;
        var scale = Math.min(scaleX, scaleY);
        var offsetX = (canvasW - mapW * scale) / 2;
        var offsetY = (h - mapH * scale) / 2;
        return {
            x: offsetX + (wx - bounds.minX) * scale,
            y: offsetY + (bounds.maxY - wy) * scale,
            scale: scale
        };
    }

    function clamp(value, min, max) {
        return Math.min(max, Math.max(min, value));
    }

    function getUiScale(cw, ch) {
        return clamp(Math.min(cw, ch) / 720, 0.95, 1.35);
    }

    function setCanvasFont(baseSize, cw, ch, weight) {
        var scaled = Math.round(baseSize * getUiScale(cw, ch));
        ctx.font = (weight || 500) + ' ' + scaled + 'px ' + FONT_FAMILY;
    }

    function drawReadableText(text, x, y, fillStyle, strokeWidth) {
        ctx.save();
        ctx.lineJoin = 'round';
        ctx.miterLimit = 2;
        ctx.strokeStyle = TEXT_STROKE;
        ctx.lineWidth = strokeWidth;
        ctx.shadowColor = 'rgba(0, 0, 0, 0.78)';
        ctx.shadowBlur = 8;
        ctx.shadowOffsetX = 0;
        ctx.shadowOffsetY = 1;
        ctx.strokeText(text, x, y);
        ctx.fillStyle = fillStyle;
        ctx.fillText(text, x, y);
        ctx.restore();
    }

    // ── INTERPOLATION ───────────────────────────────────────────────
    function updateInterpolation(dt) {
        if (!lastState || !lastState.players) return;
        var speed = INTERP_SPEED * dt;
        if (speed > 1) speed = 1; // Cap at instant snap

        for (var i = 0; i < lastState.players.length; i++) {
            var p = lastState.players[i];
            var ip = interpPositions[p.id];
            if (!ip) {
                interpPositions[p.id] = { x: p.x, y: p.y };
                continue;
            }
            // Lerp toward target
            ip.x += (p.x - ip.x) * speed;
            ip.y += (p.y - ip.y) * speed;

            // Snap if very close (avoid floating drift)
            if (Math.abs(p.x - ip.x) < 0.01) ip.x = p.x;
            if (Math.abs(p.y - ip.y) < 0.01) ip.y = p.y;

            // If still moving, keep rendering
            if (ip.x !== p.x || ip.y !== p.y) dirty = true;
        }

        // Clean up disconnected players
        for (var id in interpPositions) {
            var found = false;
            for (var i = 0; i < lastState.players.length; i++) {
                if (lastState.players[i].id == id) { found = true; break; }
            }
            if (!found) delete interpPositions[id];
        }
    }

    // ── MAP CACHE ───────────────────────────────────────────────────
    function getMapCacheCanvas(mapKey, bounds, cw, ch) {
        var cacheKey = mapKey + ':' + cw + ':' + ch;
        if (cacheKey === mapCacheKey && mapCache) return mapCache;

        // Create or resize offscreen canvas
        if (!mapCache) {
            mapCache = document.createElement('canvas');
            mapCacheCtx = mapCache.getContext('2d');
        }
        mapCache.width = cw;
        mapCache.height = ch;

        // Draw map image to cache
        if (mapKey && mapImagesLoaded[mapKey] && mapImages[mapKey]) {
            var imgB = IMAGE_BOUNDS[mapKey] || bounds;
            var tl = worldToCanvas(imgB.minX, imgB.maxY, bounds, cw, ch);
            var br = worldToCanvas(imgB.maxX, imgB.minY, bounds, cw, ch);
            mapCacheCtx.globalAlpha = 0.18;
            mapCacheCtx.drawImage(mapImages[mapKey], tl.x, tl.y, br.x - tl.x, br.y - tl.y);
            mapCacheCtx.globalAlpha = 1.0;
        }

        // Draw hairline grid — barely visible reference
        mapCacheCtx.strokeStyle = 'rgba(255, 255, 255, 0.016)';
        mapCacheCtx.lineWidth = 1;
        var gridSize = 40;
        for (var x = 0; x < cw; x += gridSize) {
            mapCacheCtx.beginPath();
            mapCacheCtx.moveTo(x, 0);
            mapCacheCtx.lineTo(x, ch);
            mapCacheCtx.stroke();
        }
        for (var y = 0; y < ch; y += gridSize) {
            mapCacheCtx.beginPath();
            mapCacheCtx.moveTo(0, y);
            mapCacheCtx.lineTo(cw, y);
            mapCacheCtx.stroke();
        }

        mapCacheKey = cacheKey;
        return mapCache;
    }

    // ── RENDER ──────────────────────────────────────────────────────
    var lastFrameTime = 0;

    function renderLoop(timestamp) {
        // FPS counter
        frameCount++;
        if (timestamp - fpsTime >= 1000) {
            currentFps = frameCount;
            frameCount = 0;
            fpsTime = timestamp;
            if (elFps) elFps.textContent = currentFps + ' fps';
        }

        // Delta time for interpolation
        var dt = lastFrameTime ? (timestamp - lastFrameTime) / 1000 : 0.016;
        lastFrameTime = timestamp;

        // Interpolate positions (may set dirty=true)
        updateInterpolation(dt);

        // Only render when needed
        if (dirty) {
            render();
            dirty = false;
        }

        requestAnimationFrame(renderLoop);
    }

    function render() {
        var cw = canvas.width;
        var ch = canvas.height;
        ctx.clearRect(0, 0, cw, ch);

        if (!lastState) {
            drawNoData(cw, ch);
            return;
        }

        var bounds = getMapBounds(lastState.mapName);

        // PERF: Blit cached map background (drawn once)
        var cached = getMapCacheCanvas(lastState.mapName, bounds, cw, ch);
        ctx.drawImage(cached, 0, 0);

        // Update status bar
        if (elMapName) elMapName.textContent = lastState.mapName || '--';
        var playerCount = lastState.players ? lastState.players.length : 0;
        if (elPlayerCount) elPlayerCount.textContent = playerCount + (playerCount === 1 ? ' player' : ' players');

        if (!lastState.players || lastState.players.length === 0) return;

        // PERF: Single-pass render — sort dead first (underneath), alive on top
        var sorted = lastState.players.slice().sort(function (a, b) {
            if (a.isDead && !b.isDead) return -1;
            if (!a.isDead && b.isDead) return 1;
            return 0;
        });

        for (var i = 0; i < sorted.length; i++) {
            drawPlayer(sorted[i], bounds, cw, ch);
        }
    }

    function drawNoData(cw, ch) {
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        var uiScale = getUiScale(cw, ch);
        setCanvasFont(TITLE_FONT_SIZE, cw, ch, 700);
        drawReadableText('waiting for signal', cw / 2, ch / 2 - 8 * uiScale, TEXT_SECONDARY, Math.max(2, Math.round(uiScale * 2)));
        setCanvasFont(STATUS_FONT_SIZE, cw, ch, 500);
        drawReadableText('game must be running with web radar enabled', cw / 2, ch / 2 + 14 * uiScale, TEXT_MUTED, Math.max(2, Math.round(uiScale * 1.6)));
    }

    function drawPlayer(p, bounds, cw, ch) {
        // Use interpolated position for smooth movement
        var ip = interpPositions[p.id] || { x: p.x, y: p.y };
        var pos = worldToCanvas(ip.x, ip.y, bounds, cw, ch);
        var color = PLAYER_COLORS[p.colorId] || '#ffffff';
        var isLocal = (p.id === lastState.localPlayerId);
        var radius = PLAYER_RADIUS;
        var uiScale = getUiScale(cw, ch);
        var textStroke = Math.max(2, Math.round(uiScale * 1.8));

        if (p.isDead) {
            // Dead: small × — ghost opacity
            ctx.globalAlpha = 0.28;
            ctx.strokeStyle = color;
            ctx.lineWidth = 1.5;
            var half = radius * 0.65;
            ctx.beginPath();
            ctx.moveTo(pos.x - half, pos.y - half);
            ctx.lineTo(pos.x + half, pos.y + half);
            ctx.moveTo(pos.x + half, pos.y - half);
            ctx.lineTo(pos.x - half, pos.y + half);
            ctx.stroke();
            ctx.globalAlpha = 1.0;
        } else {
            // Alive: filled circle — no glow, pure shape
            ctx.beginPath();
            ctx.arc(pos.x, pos.y, radius, 0, Math.PI * 2);
            ctx.fillStyle = color;
            ctx.globalAlpha = p.inVent ? 0.35 : 1.0;
            ctx.fill();
            ctx.globalAlpha = 1.0;

            // Impostor ring — thin red, no glow
            if (p.isImpostor) {
                ctx.beginPath();
                ctx.arc(pos.x, pos.y, radius + 3, 0, Math.PI * 2);
                ctx.strokeStyle = IMPOSTOR_RING;
                ctx.lineWidth = 1;
                ctx.stroke();
            }

            // Local player ring — hairline dashed white
            if (isLocal) {
                ctx.beginPath();
                ctx.arc(pos.x, pos.y, radius + 4, 0, Math.PI * 2);
                ctx.strokeStyle = LOCAL_RING;
                ctx.lineWidth = 1;
                ctx.setLineDash([2, 3]);
                ctx.stroke();
                ctx.setLineDash([]);
            }

            // Vent indicator — minimal tag
            if (p.inVent) {
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                setCanvasFont(STATUS_FONT_SIZE, cw, ch, 700);
                drawReadableText('vent', pos.x, pos.y + radius + Math.round(16 * uiScale), VENT_TEXT, Math.max(2, Math.round(uiScale * 1.5)));
            }
        }

        // Name label — dim, monospace, no shadow
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        setCanvasFont(NAME_FONT_SIZE, cw, ch, p.isDead ? 500 : 700);
        drawReadableText(
            p.name || '?',
            pos.x,
            pos.y - radius - Math.round(7 * uiScale),
            p.isDead ? TEXT_DEAD : (isLocal ? TEXT_PRIMARY : TEXT_SECONDARY),
            textStroke
        );
    }

    // ── START ───────────────────────────────────────────────────────
    // ── SHARE SYSTEM ──
    function initShare() {
        var btn = document.getElementById('share-btn');
        var modal = document.getElementById('share-modal');
        var urlInput = document.getElementById('share-url');
        var copyBtn = document.getElementById('share-copy');
        var closeBtn = document.getElementById('share-close');
        if (!btn || !modal) return;

        var isLocal = location.hostname === 'localhost' || location.hostname === '127.0.0.1' || location.hostname === '::1';
        if (!isLocal) { btn.style.display = 'none'; return; }

        var shareUrl = '';

        btn.addEventListener('click', function() {
            var shareInfoUrl = '/radar/share-info' + (_token ? '?token=' + encodeURIComponent(_token) : '');
            fetch(shareInfoUrl, { headers: _token ? { 'X-Radar-Token': _token } : {} })
                .then(function(r) { return r.json(); })
                .then(function(data) {
                    shareUrl = 'http://' + data.ip + ':' + data.port + '/radar/?token=' + data.token;
                    urlInput.value = shareUrl;

                    // Try Web Share API first (mobile native share sheet)
                    if (navigator.share) {
                        navigator.share({ title: 'Among Us Radar', url: shareUrl }).catch(function() {
                            modal.style.display = 'flex';
                        });
                    } else {
                        modal.style.display = 'flex';
                    }
                })
                .catch(function() { urlInput.value = 'Error'; modal.style.display = 'flex'; });
        });

        copyBtn.addEventListener('click', function() {
            var text = urlInput.value;
            var onSuccess = function() {
                copyBtn.textContent = 'COPIED!';
                copyBtn.style.background = 'rgba(25, 242, 192, 0.3)';
                setTimeout(function() {
                    copyBtn.textContent = 'COPY';
                    copyBtn.style.background = '';
                }, 2000);
            };

            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(text).then(onSuccess).catch(function() {
                    legacyCopy(text) && onSuccess();
                });
            } else {
                legacyCopy(text) && onSuccess();
            }
        });

        closeBtn.addEventListener('click', function() { modal.style.display = 'none'; });
        modal.addEventListener('click', function(e) { if (e.target === modal) modal.style.display = 'none'; });
        document.addEventListener('keydown', function(e) { if (e.key === 'Escape') modal.style.display = 'none'; });
    }

    function legacyCopy(text) {
        var ta = document.createElement('textarea');
        ta.value = text;
        ta.style.cssText = 'position:fixed;left:-9999px;opacity:0';
        document.body.appendChild(ta);
        ta.select();
        var ok = false;
        try { ok = document.execCommand('copy'); } catch(e) {}
        ta.remove();
        return ok;
    }

    function initAll() { init(); initShare(); }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAll);
    } else {
        initAll();
    }
})();
