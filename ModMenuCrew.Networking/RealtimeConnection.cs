using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InnerNet;
using ModMenuCrew.UI.Managers;
using ModMenuCrew.UI.Menus;
using UnityEngine;

namespace ModMenuCrew.Networking;

public static class RealtimeConnection
{
	private static ClientWebSocket _webSocket;

	private static CancellationTokenSource _cancellationTokenSource;

	private static bool _isConnected = false;

	private static bool _isAuthenticated = false;

	private static bool _shouldReconnect = true;

	private static volatile bool _isConnecting = false;

	private static int _reconnectAttempts = 0;

	private static readonly int MAX_RECONNECT_ATTEMPTS = 15;

	private static readonly int BASE_RECONNECT_DELAY_MS = 3000;

	private static long _lastConnectAttemptMs = 0L;

	private const long MIN_RECONNECT_INTERVAL_MS = 3000L;

	private static readonly Random _reconnectJitter = new Random();

	private static CancellationTokenSource _keepaliveCts = null;

	private static string _cachedKey = "";

	private static string _cachedToken = "";

	private static string _cachedHwid = "";

	private static string _wsUrl = "";

	private static byte[] _channelKey = null;

	private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

	private static AesGcm _cachedAesGcm = null;

	private static readonly object _aesLock = new object();

	private static readonly JsonSerializerOptions _wsJsonOpts = new JsonSerializerOptions();

	private static string _clientSessionId = Guid.NewGuid().ToString("N");

	private static int _lastKnownGameId = 0;

	private static int _channelEncryptedXor = 1850449314;

	private const int CHANNEL_SENTINEL = 1850449315;

	private static long _lastForceReconnectMs = 0L;

	private const long FORCE_RECONNECT_COOLDOWN_MS = 30000L;

	internal static string ClientSessionId => _clientSessionId;

	private static bool IsChannelEncrypted => (_channelEncryptedXor ^ 0x6E4B9DA3) == 1850449315;

	internal static bool IsConnected
	{
		get
		{
			if (_isConnected)
			{
				return _isAuthenticated;
			}
			return false;
		}
	}

	private static int GetKeepaliveIntervalMs()
	{
		try
		{
			if (GetIsInGame())
			{
				return 20000;
			}
		}
		catch
		{
		}
		return 45000;
	}

	private static void SetChannelEncrypted(bool value)
	{
		_channelEncryptedXor = ((!value) ? 1850449314 : 0);
	}

	internal static async void Connect(string baseUrl, string key, string token)
	{
		if (!_isConnected)
		{
			if (!baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			{
				ServerData.TriggerSilentDenial();
				return;
			}
			_cachedKey = key;
			_cachedToken = token;
			_cachedHwid = ModKeyValidator.CachedHwid;
			_shouldReconnect = true;
			_reconnectAttempts = 0;
			_wsUrl = baseUrl.Replace("https://", "wss://").TrimEnd('/') + "/ws/realtime";
			await ConnectInternal();
		}
	}

	internal static void Disconnect()
	{
		_shouldReconnect = false;
		_isConnected = false;
		_isAuthenticated = false;
		_isConnecting = false;
		SetChannelEncrypted(value: false);
		_channelKey = null;
		DisposeAesGcm();
		try
		{
			_keepaliveCts?.Cancel();
			_keepaliveCts?.Dispose();
		}
		catch
		{
		}
		_keepaliveCts = null;
		try
		{
			_cancellationTokenSource?.Cancel();
			ClientWebSocket webSocket = _webSocket;
			if (webSocket != null && webSocket.State == WebSocketState.Open)
			{
				_webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None).Wait(1000);
			}
			_webSocket?.Dispose();
		}
		catch
		{
		}
		finally
		{
			_webSocket = null;
			try
			{
				_cancellationTokenSource?.Dispose();
			}
			catch
			{
			}
			_cancellationTokenSource = null;
		}
	}

	internal static void ForceReconnect()
	{
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		if (num - _lastForceReconnectMs < 30000)
		{
			return;
		}
		_lastForceReconnectMs = num;
		_isConnected = false;
		_isAuthenticated = false;
		_isConnecting = false;
		SetChannelEncrypted(value: false);
		_channelKey = null;
		DisposeAesGcm();
		_reconnectAttempts = 0;
		try
		{
			_keepaliveCts?.Cancel();
			_keepaliveCts?.Dispose();
		}
		catch
		{
		}
		_keepaliveCts = null;
		try
		{
			_cancellationTokenSource?.Cancel();
			_webSocket?.Dispose();
		}
		catch
		{
		}
		finally
		{
			_webSocket = null;
			try
			{
				_cancellationTokenSource?.Dispose();
			}
			catch
			{
			}
			_cancellationTokenSource = null;
		}
		ScheduleReconnect();
	}

	internal static async void SendUpdate(object playerData, object uiState, object banMenuState, object banMenuUiState, object cheatsState = null, object cheatsUiState = null, object alivePlayers = null, object spoofingState = null, object settingsState = null, object sabotageState = null, object impersonatePlayers = null, bool priority = false)
	{
		if (!IsConnected)
		{
			return;
		}
		try
		{
			RefreshSessionIdIfLobbyChanged();
			string attestation_proof = ActionPermitSystem.ComputeAttestationProof();
			int localPlayerId = GetLocalPlayerId();
			await SendMessage(new
			{
				type = "update",
				players = playerData,
				isHost = GetIsHost(),
				isInGame = GetIsInGame(),
				localPlayerId = ((localPlayerId == 255) ? (-1) : localPlayerId),
				uiState = uiState,
				banMenuState = banMenuState,
				banMenuUiState = banMenuUiState,
				cheatsState = cheatsState,
				cheatsUiState = cheatsUiState,
				alivePlayers = alivePlayers,
				spoofingState = spoofingState,
				settingsState = settingsState,
				sabotageState = sabotageState,
				impersonatePlayers = impersonatePlayers,
				animationsUiState = AnimationsTab.GetUIState(),
				menuWidth = GhostUI.CurrentWindowWidth,
				menuHeight = GhostUI.CurrentWindowHeight,
				isInMeeting = ((Object)(object)MeetingHud.Instance != (Object)null),
				currentRole = GetCurrentRole(),
				attestation_proof = attestation_proof,
				clientSessionId = _clientSessionId,
				pri = (priority ? 1 : 0)
			});
		}
		catch (Exception)
		{
		}
	}

	internal static async void SendActionRequest(string requestId, string actionId, string permitToken, string clientChallenge)
	{
		if (!IsConnected)
		{
			return;
		}
		try
		{
			string attestation_proof = ActionPermitSystem.ComputeAttestationProof();
			await SendMessage(new
			{
				type = "action_request",
				requestId = requestId,
				actionId = actionId,
				permitToken = permitToken,
				clientChallenge = clientChallenge,
				attestation_proof = attestation_proof
			});
		}
		catch
		{
		}
	}

	private static string GetCurrentRole()
	{
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			object obj;
			if (localPlayer == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo data = localPlayer.Data;
				obj = ((data != null) ? data.Role : null);
			}
			RoleBehaviour val = (RoleBehaviour)obj;
			if ((Object)(object)val != (Object)null)
			{
				return ((object)val).GetType().Name;
			}
		}
		catch
		{
		}
		return "";
	}

	internal static string GetCurrentRoleForHeartbeat()
	{
		return GetCurrentRole();
	}

	private static async Task ConnectInternal()
	{
		if (_isConnecting || _isConnected)
		{
			return;
		}
		_isConnecting = true;
		try
		{
			_webSocket = new ClientWebSocket();
			try
			{
				_cancellationTokenSource?.Dispose();
			}
			catch
			{
			}
			_cancellationTokenSource = new CancellationTokenSource();
			_webSocket.Options.RemoteCertificateValidationCallback = delegate(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors errors)
			{
				bool flag = CertificatePinner.ValidateServerCertificate(sender, cert, chain, errors);
				return (!flag && ModKeyValidator.IsRunningUnderWine && errors == SslPolicyErrors.RemoteCertificateChainErrors) || flag;
			};
			await _webSocket.ConnectAsync(new Uri(_wsUrl), _cancellationTokenSource.Token);
			_isConnected = true;
			string encryptedChannelKey = "";
			if (ModKeyValidator.IsRunningUnderWine)
			{
				_channelKey = null;
				SetChannelEncrypted(value: false);
			}
			else
			{
				_channelKey = new byte[32];
				SetChannelEncrypted(value: false);
				using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
				{
					randomNumberGenerator.GetBytes(_channelKey);
				}
				byte[] array = GhostUI.RsaEncrypt(_channelKey);
				encryptedChannelKey = ((array != null) ? Convert.ToBase64String(array) : "");
			}
			await SendMessageRaw(new
			{
				type = "auth",
				key = _cachedKey,
				token = _cachedToken,
				hwid = _cachedHwid,
				encryptedChannelKey = encryptedChannelKey
			});
			ReceiveLoop();
		}
		catch (Exception)
		{
			_isConnected = false;
			ScheduleReconnect();
		}
		finally
		{
			_isConnecting = false;
		}
	}

	private static async Task ReceiveLoop()
	{
		byte[] buffer = new byte[65536];
		MemoryStream messageStream = new MemoryStream(65536);
		try
		{
			while (true)
			{
				ClientWebSocket webSocket = _webSocket;
				if (webSocket == null || webSocket.State != WebSocketState.Open || _cancellationTokenSource.Token.IsCancellationRequested)
				{
					break;
				}
				WebSocketReceiveResult webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
				if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
				{
					break;
				}
				messageStream.Write(buffer, 0, webSocketReceiveResult.Count);
				if (!webSocketReceiveResult.EndOfMessage)
				{
					continue;
				}
				string @string = Encoding.UTF8.GetString(messageStream.GetBuffer(), 0, (int)messageStream.Position);
				messageStream.Position = 0L;
				messageStream.SetLength(0L);
				string json;
				if (IsChannelEncrypted && _channelKey != null)
				{
					if (!@string.StartsWith("E:"))
					{
						continue;
					}
					byte[] array = ChannelDecrypt(Convert.FromBase64String(@string.Substring(2)));
					if (array == null)
					{
						continue;
					}
					json = Encoding.UTF8.GetString(array);
				}
				else
				{
					json = @string;
				}
				ProcessMessage(json);
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception)
		{
		}
		finally
		{
			messageStream.Dispose();
		}
		_isConnected = false;
		_isAuthenticated = false;
		SetChannelEncrypted(value: false);
		_channelKey = null;
		ScheduleReconnect();
	}

	private static void ProcessMessage(string json)
	{
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(json);
			JsonElement rootElement = jsonDocument.RootElement;
			if (!rootElement.TryGetProperty("type", out var value))
			{
				return;
			}
			switch (value.GetString())
			{
			case "auth_success":
				_isAuthenticated = true;
				_reconnectAttempts = 0;
				if (_channelKey != null)
				{
					SetChannelEncrypted(value: true);
				}
				try
				{
					if (rootElement.TryGetProperty("spki_pins", out var value3) && value3.ValueKind == JsonValueKind.Array)
					{
						List<string> list = new List<string>();
						foreach (JsonElement item in value3.EnumerateArray())
						{
							if (item.ValueKind == JsonValueKind.String)
							{
								list.Add(item.GetString());
							}
						}
						if (list.Count > 0)
						{
							CertificatePinner.UpdatePinsFromServer(list.ToArray());
						}
					}
				}
				catch
				{
				}
				try
				{
					PlayerPickMenu.ResetHashForReconnect();
				}
				catch
				{
				}
				TriggerInitialUpdate();
				StartKeepaliveLoop();
				break;
			case "auth_error":
				_reconnectAttempts = MAX_RECONNECT_ATTEMPTS;
				_isAuthenticated = false;
				try
				{
					_cancellationTokenSource?.Cancel();
					break;
				}
				catch
				{
					break;
				}
			case "bytecode_update":
				ProcessBytecodeUpdate(rootElement);
				break;
			case "action_approved":
			{
				JsonElement value4;
				string text = (rootElement.TryGetProperty("requestId", out value4) ? value4.GetString() : null);
				JsonElement value5;
				string serverNonce = (rootElement.TryGetProperty("serverNonce", out value5) ? value5.GetString() : null);
				if (!string.IsNullOrEmpty(text))
				{
					ActionPermitSystem.OnServerApproval(text, serverNonce);
				}
				break;
			}
			case "session_revoked":
			{
				ModKeyValidator.RequestResetFromRealtime((rootElement.TryGetProperty("reason", out var value2) && value2.ValueKind == JsonValueKind.String) ? value2.GetString() : "");
				Disconnect();
				break;
			}
			case "action_denied":
				break;
			}
		}
		catch
		{
		}
	}

	private static void ProcessBytecodeUpdate(JsonElement root)
	{
		try
		{
			JsonElement value;
			long num = (root.TryGetProperty("session_token", out value) ? value.GetInt64() : 0);
			if (root.TryGetProperty("action_map", out var value2) && value2.ValueKind == JsonValueKind.Object)
			{
				ServerData.UpdateActionIdMap(value2);
			}
			if (root.TryGetProperty("security_config", out var value3) && value3.ValueKind == JsonValueKind.Object)
			{
				ServerData.ParseSecurityConfig(value3);
			}
			byte[] array = null;
			byte[] array2 = null;
			byte[] array3 = null;
			byte[] array4 = null;
			if (root.TryGetProperty("game_bytecode", out var value4) && value4.ValueKind == JsonValueKind.String)
			{
				string @string = value4.GetString();
				if (!string.IsNullOrEmpty(@string))
				{
					array = Convert.FromBase64String(@string);
				}
			}
			if (root.TryGetProperty("lobby_bytecode", out var value5) && value5.ValueKind == JsonValueKind.String)
			{
				string string2 = value5.GetString();
				if (!string.IsNullOrEmpty(string2))
				{
					array2 = Convert.FromBase64String(string2);
				}
			}
			if (root.TryGetProperty("playerpick_bytecode", out var value6) && value6.ValueKind == JsonValueKind.String)
			{
				string string3 = value6.GetString();
				if (!string.IsNullOrEmpty(string3))
				{
					array3 = Convert.FromBase64String(string3);
				}
			}
			if (root.TryGetProperty("banmenu_bytecode", out var value7) && value7.ValueKind == JsonValueKind.String)
			{
				string string4 = value7.GetString();
				if (!string.IsNullOrEmpty(string4))
				{
					array4 = Convert.FromBase64String(string4);
				}
			}
			byte[] array5 = null;
			if (root.TryGetProperty("cheats_bytecode", out var value8) && value8.ValueKind == JsonValueKind.String)
			{
				string string5 = value8.GetString();
				if (!string.IsNullOrEmpty(string5))
				{
					array5 = Convert.FromBase64String(string5);
				}
			}
			byte[] array6 = null;
			if (root.TryGetProperty("spoofing_bytecode", out var value9) && value9.ValueKind == JsonValueKind.String)
			{
				string string6 = value9.GetString();
				if (!string.IsNullOrEmpty(string6))
				{
					array6 = Convert.FromBase64String(string6);
				}
			}
			byte[] array7 = null;
			if (root.TryGetProperty("settings_bytecode", out var value10) && value10.ValueKind == JsonValueKind.String)
			{
				string string7 = value10.GetString();
				if (!string.IsNullOrEmpty(string7))
				{
					array7 = Convert.FromBase64String(string7);
				}
			}
			byte[] array8 = null;
			if (root.TryGetProperty("sabotage_bytecode", out var value11) && value11.ValueKind == JsonValueKind.String)
			{
				string string8 = value11.GetString();
				if (!string.IsNullOrEmpty(string8))
				{
					array8 = Convert.FromBase64String(string8);
				}
			}
			byte[] array9 = null;
			if (root.TryGetProperty("hostcontrols_bytecode", out var value12) && value12.ValueKind == JsonValueKind.String)
			{
				string string9 = value12.GetString();
				if (!string.IsNullOrEmpty(string9))
				{
					array9 = Convert.FromBase64String(string9);
				}
			}
			byte[] array10 = null;
			if (root.TryGetProperty("extras_bytecode", out var value13) && value13.ValueKind == JsonValueKind.String)
			{
				string string10 = value13.GetString();
				if (!string.IsNullOrEmpty(string10))
				{
					array10 = Convert.FromBase64String(string10);
				}
			}
			byte[] array11 = null;
			if (root.TryGetProperty("animations_bytecode", out var value14) && value14.ValueKind == JsonValueKind.String)
			{
				string string11 = value14.GetString();
				if (!string.IsNullOrEmpty(string11))
				{
					array11 = Convert.FromBase64String(string11);
				}
			}
			if (array == null && array2 == null && array3 == null && array4 == null && array5 == null && array6 == null && array7 == null && array8 == null && array9 == null && array10 == null && array11 == null && num > 0)
			{
				long lastRealtimeUpdateMs = ServerData.LastRealtimeUpdateMs;
				long num2 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				if (lastRealtimeUpdateMs > 0 && num2 - lastRealtimeUpdateMs > 20000)
				{
					ActionPermitSystem.EnqueueMainThread(delegate
					{
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					});
				}
			}
			else
			{
				ServerData.UpdateFromRealtime(array2, array, array3, array4, array5, array6, array7, num, array8, array9, array10, array11);
			}
			if (root.TryGetProperty("attestation_seed", out var value15) && value15.ValueKind == JsonValueKind.String)
			{
				string string12 = value15.GetString();
				int[] array12 = null;
				if (root.TryGetProperty("attestation_methods", out var value16) && value16.ValueKind == JsonValueKind.Array)
				{
					List<int> list = new List<int>();
					foreach (JsonElement item in value16.EnumerateArray())
					{
						if (item.ValueKind == JsonValueKind.Number)
						{
							list.Add(item.GetInt32());
						}
					}
					array12 = list.ToArray();
				}
				if (!string.IsNullOrEmpty(string12) && array12 != null && array12.Length != 0)
				{
					ActionPermitSystem.UpdateAttestation(string12, array12);
				}
			}
			if (root.TryGetProperty("session_proof", out var value17) && value17.ValueKind == JsonValueKind.String)
			{
				string string13 = value17.GetString();
				JsonElement value18;
				long num3 = ((root.TryGetProperty("proof_seed", out value18) && value18.ValueKind == JsonValueKind.Number) ? value18.GetInt64() : 0);
				JsonElement value19;
				long num4 = ((root.TryGetProperty("proof_expires", out value19) && value19.ValueKind == JsonValueKind.Number) ? value19.GetInt64() : 0);
				JsonElement value20;
				long ttlSeconds = ((root.TryGetProperty("proof_ttl_seconds", out value20) && value20.ValueKind == JsonValueKind.Number) ? value20.GetInt64() : 0);
				if (!string.IsNullOrEmpty(string13) && num3 > 0 && num4 > 0)
				{
					ModKeyValidator.UpdateProof(string13, num3, num4, ttlSeconds);
				}
			}
			if (root.TryGetProperty("render_key", out var value21) && value21.ValueKind == JsonValueKind.String)
			{
				string string14 = value21.GetString();
				JsonElement value22;
				long num5 = ((root.TryGetProperty("render_expires", out value22) && value22.ValueKind == JsonValueKind.Number) ? value22.GetInt64() : 0);
				JsonElement value23;
				long num6 = ((root.TryGetProperty("render_nonce", out value23) && value23.ValueKind == JsonValueKind.Number) ? value23.GetInt64() : 0);
				JsonElement value24;
				long ttlSeconds2 = ((root.TryGetProperty("render_ttl_seconds", out value24) && value24.ValueKind == JsonValueKind.Number) ? value24.GetInt64() : 0);
				if (!string.IsNullOrEmpty(string14) && num5 > 0 && num6 > 0)
				{
					ServerGate.UpdateRenderPermission(string14, num5, num6, ttlSeconds2);
				}
			}
		}
		catch
		{
		}
	}

	private static async Task SendMessage(object message)
	{
		ClientWebSocket webSocket = _webSocket;
		if (webSocket == null || webSocket.State != WebSocketState.Open)
		{
			return;
		}
		string s = JsonSerializer.Serialize(message, _wsJsonOpts);
		if (IsChannelEncrypted && _channelKey != null)
		{
			byte[] array = ChannelEncrypt(Encoding.UTF8.GetBytes(s));
			if (array != null)
			{
				string s2 = "E:" + Convert.ToBase64String(array);
				byte[] bytes = Encoding.UTF8.GetBytes(s2);
				await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, _cancellationTokenSource.Token);
			}
		}
		else
		{
			byte[] bytes2 = Encoding.UTF8.GetBytes(s);
			await _webSocket.SendAsync(new ArraySegment<byte>(bytes2), WebSocketMessageType.Text, endOfMessage: true, _cancellationTokenSource.Token);
		}
	}

	private static async Task SendMessageRaw(object message)
	{
		ClientWebSocket webSocket = _webSocket;
		if (webSocket != null && webSocket.State == WebSocketState.Open)
		{
			string s = JsonSerializer.Serialize(message, _wsJsonOpts);
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, _cancellationTokenSource.Token);
		}
	}

	private static AesGcm GetOrCreateAesGcm()
	{
		if (_cachedAesGcm != null)
		{
			return _cachedAesGcm;
		}
		lock (_aesLock)
		{
			if (_cachedAesGcm == null && _channelKey != null)
			{
				_cachedAesGcm = new AesGcm(_channelKey);
			}
			return _cachedAesGcm;
		}
	}

	private static void DisposeAesGcm()
	{
		lock (_aesLock)
		{
			_cachedAesGcm?.Dispose();
			_cachedAesGcm = null;
		}
	}

	private static void RefreshSessionIdIfLobbyChanged()
	{
		try
		{
			AmongUsClient instance = AmongUsClient.Instance;
			int num = ((instance != null) ? ((InnerNetClient)instance).GameId : 0);
			if (num != 0 && num != _lastKnownGameId)
			{
				_lastKnownGameId = num;
				_clientSessionId = Guid.NewGuid().ToString("N");
			}
		}
		catch
		{
		}
	}

	private static byte[] ChannelEncrypt(byte[] plaintext)
	{
		try
		{
			byte[] array = new byte[12 + plaintext.Length + 16];
			_rng.GetBytes(array, 0, 12);
			AesGcm orCreateAesGcm = GetOrCreateAesGcm();
			if (orCreateAesGcm == null)
			{
				return null;
			}
			orCreateAesGcm.Encrypt(new ReadOnlySpan<byte>(array, 0, 12), plaintext, new Span<byte>(array, 12, plaintext.Length), new Span<byte>(array, 12 + plaintext.Length, 16));
			return array;
		}
		catch
		{
			return null;
		}
	}

	private static byte[] ChannelDecrypt(byte[] encrypted)
	{
		try
		{
			if (encrypted.Length < 28)
			{
				return null;
			}
			int num = encrypted.Length - 12 - 16;
			byte[] array = new byte[num];
			AesGcm orCreateAesGcm = GetOrCreateAesGcm();
			if (orCreateAesGcm == null)
			{
				return null;
			}
			orCreateAesGcm.Decrypt(new ReadOnlySpan<byte>(encrypted, 0, 12), new ReadOnlySpan<byte>(encrypted, 12, num), new ReadOnlySpan<byte>(encrypted, 12 + num, 16), array);
			return array;
		}
		catch
		{
			return null;
		}
	}

	private static void ScheduleReconnect()
	{
		if (!_shouldReconnect)
		{
			return;
		}
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastConnectAttemptMs;
		int val = (int)((num < 3000) ? (3000 - num) : 0);
		if (_reconnectAttempts >= MAX_RECONNECT_ATTEMPTS)
		{
			Task.Delay(120000).ContinueWith(delegate(Task _)
			{
				if (_shouldReconnect && !_isConnected)
				{
					_reconnectAttempts = MAX_RECONNECT_ATTEMPTS - 3;
					_lastConnectAttemptMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
					_ = ConnectInternal();
				}
			});
			return;
		}
		_reconnectAttempts++;
		int num2 = Math.Max(BASE_RECONNECT_DELAY_MS * (int)Math.Pow(2.0, Math.Min(_reconnectAttempts - 1, 5)), val);
		Task.Delay(num2 + (int)((double)num2 * 0.25 * _reconnectJitter.NextDouble())).ContinueWith(delegate(Task _)
		{
			if (_shouldReconnect && !_isConnected)
			{
				_lastConnectAttemptMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				_ = ConnectInternal();
			}
		});
	}

	internal static void ResetReconnectAttempts()
	{
		if (!_isConnected && _shouldReconnect && _reconnectAttempts >= MAX_RECONNECT_ATTEMPTS)
		{
			_reconnectAttempts = 0;
			ConnectInternal();
		}
	}

	private static void StartKeepaliveLoop()
	{
		try
		{
			_keepaliveCts?.Cancel();
			_keepaliveCts?.Dispose();
		}
		catch
		{
		}
		_keepaliveCts = new CancellationTokenSource();
		CancellationToken token = _keepaliveCts.Token;
		Task.Run(async delegate
		{
			try
			{
				while (!token.IsCancellationRequested && _isConnected && _isAuthenticated)
				{
					await Task.Delay(GetKeepaliveIntervalMs(), token);
					if (!_isConnected || !_isAuthenticated)
					{
						break;
					}
					TriggerInitialUpdate();
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch
			{
			}
		});
	}

	private static void TriggerInitialUpdate()
	{
		ActionPermitSystem.EnqueueMainThread(delegate
		{
			try
			{
				object[] playerData = PlayerPickMenu.CollectPlayerDataForServer();
				object uIState = PlayerPickMenu.GetUIState();
				object banMenuState = BanMenu.GetBanMenuState();
				object uIState2 = BanMenu.GetUIState();
				object cheatsState = CheatManager.GetCheatsState();
				object cheatsUiState = CheatManager.GetCheatsUiState();
				List<object> alivePlayersForServer = CheatManager.GetAlivePlayersForServer();
				object spoofingState = SpoofingMenu.GetSpoofingState();
				object settingsState = SettingsTab.GetSettingsState();
				object sabotageState = null;
				try
				{
					sabotageState = CheatManager.GetSabotageState();
				}
				catch
				{
				}
				SendUpdate(playerData, uIState, banMenuState, uIState2, cheatsState, cheatsUiState, alivePlayersForServer, spoofingState, settingsState, sabotageState);
			}
			catch
			{
			}
		});
	}

	private static bool GetIsHost()
	{
		try
		{
			AmongUsClient instance = AmongUsClient.Instance;
			return instance != null && ((InnerNetClient)instance).AmHost;
		}
		catch
		{
			return false;
		}
	}

	private static bool GetIsInGame()
	{
		try
		{
			return (Object)(object)ShipStatus.Instance != (Object)null;
		}
		catch
		{
			return false;
		}
	}

	private static int GetLocalPlayerId()
	{
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			return (localPlayer != null) ? localPlayer.PlayerId : byte.MaxValue;
		}
		catch
		{
			return 255;
		}
	}
}
