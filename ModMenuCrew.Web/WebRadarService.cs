using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace ModMenuCrew.Web;

public static class WebRadarService
{
	private sealed class ClientSlot
	{
		public WebSocket Socket;

		public SemaphoreSlim SendGate;

		public string Ip;
	}

	private enum TokenSource
	{
		None,
		Query,
		Header,
		Cookie
	}

	private const int PORT = 9222;

	private const int WS_MAX_FRAME_SIZE = 4096;

	private const int MAX_HTTP_REQUEST_SIZE = 8192;

	private const int MAX_CLIENTS = 5;

	private const int MAX_CONCURRENT_TCP = 32;

	private const int HTTP_READ_TIMEOUT_MS = 5000;

	private const int WS_SEND_TIMEOUT_MS = 2000;

	private const int WS_KEEPALIVE_INTERVAL_MS = 30000;

	private const long BROADCAST_INTERVAL_MS = 66L;

	private const int MAX_AUTH_FAILS = 5;

	private const long BAN_DURATION_MS = 300000L;

	private const int TOKEN_LENGTH = 10;

	private const int IP_SECURITY_MAX_ENTRIES = 5000;

	private const long IP_SECURITY_SWEEP_INTERVAL_MS = 60000L;

	private static readonly Regex _safeResourcePath = new Regex("^/radar(?:/[A-Za-z0-9_\\-]+(?:\\.[A-Za-z0-9]+)?)*/?$", RegexOptions.Compiled);

	private static TcpListener _listener;

	private static CancellationTokenSource _cts;

	private static readonly ConcurrentDictionary<WebSocket, ClientSlot> _clients = new ConcurrentDictionary<WebSocket, ClientSlot>();

	private static readonly ConcurrentDictionary<string, byte[]> _resourceCache = new ConcurrentDictionary<string, byte[]>();

	private static readonly SemaphoreSlim _acceptGate = new SemaphoreSlim(32, 32);

	private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		IncludeFields = true,
		WriteIndented = false
	};

	private static long _lastBroadcastMs;

	private static long _lastIpSweepMs;

	private static bool _isRunning;

	private static bool _allowRemote;

	private static byte[] _tokenBytes;

	private static string _tokenString;

	private static byte[] _lastBroadcastData;

	private static string _lastMapName;

	private static int _lastPlayerHash;

	private static volatile HashSet<string> _allowedHosts;

	private static readonly ConcurrentDictionary<string, (int fails, long banUntil)> _ipSecurity = new ConcurrentDictionary<string, (int, long)>();

	public static bool IsRunning => _isRunning;

	public static string AccessToken => _tokenString;

	public static void Start()
	{
		Start(allowRemote: true);
	}

	public static void Start(bool allowRemote)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Expected O, but got Unknown
		if (_isRunning)
		{
			return;
		}
		_tokenString = GenerateToken(10);
		_tokenBytes = Encoding.ASCII.GetBytes(_tokenString);
		_allowRemote = allowRemote;
		_allowedHosts = BuildAllowedHosts();
		_ipSecurity.Clear();
		_lastIpSweepMs = 0L;
		_cts = new CancellationTokenSource();
		bool flag = default(bool);
		try
		{
			_listener = new TcpListener(allowRemote ? IPAddress.Any : IPAddress.Loopback, 9222);
			_listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: false);
			_listener.Start();
		}
		catch (SocketException ex)
		{
			try
			{
				ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
				if (instance != null)
				{
					ManualLogSource log = ((BasePlugin)instance).Log;
					if (log != null)
					{
						BepInExErrorLogInterpolatedStringHandler val = new BepInExErrorLogInterpolatedStringHandler(90, 2, ref flag);
						if (flag)
						{
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[WebRadar] Bind failed on port ");
							((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>(9222);
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral(": ");
							((BepInExLogInterpolatedStringHandler)val).AppendFormatted<SocketError>(ex.SocketErrorCode);
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" — another process may own it (Chrome DevTools uses 9222)");
						}
						log.LogError(val);
					}
				}
			}
			catch
			{
			}
			_tokenString = null;
			_tokenBytes = null;
			try
			{
				_cts.Dispose();
			}
			catch
			{
			}
			_cts = null;
			return;
		}
		_isRunning = true;
		AcceptLoop(_cts.Token);
		try
		{
			ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
			if (instance == null)
			{
				return;
			}
			ManualLogSource log = ((BasePlugin)instance).Log;
			if (log != null)
			{
				BepInExInfoLogInterpolatedStringHandler val2 = new BepInExInfoLogInterpolatedStringHandler(39, 3, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[WebRadar] Started on http://");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(allowRemote ? "0.0.0.0" : "127.0.0.1");
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(":");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<int>(9222);
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(" — token ");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(MaskToken(_tokenString));
				}
				log.LogInfo(val2);
			}
		}
		catch
		{
		}
	}

	private static string MaskToken(string t)
	{
		if (string.IsNullOrEmpty(t) || t.Length < 4)
		{
			return "***";
		}
		return t.Substring(0, 2) + new string('*', t.Length - 4) + t.Substring(t.Length - 2);
	}

	private static string GenerateToken(int length)
	{
		int length2 = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".Length;
		int num = 256 - 256 % length2;
		StringBuilder stringBuilder = new StringBuilder(length);
		Span<byte> data = stackalloc byte[1];
		while (stringBuilder.Length < length)
		{
			RandomNumberGenerator.Fill(data);
			if (data[0] < num)
			{
				stringBuilder.Append("ABCDEFGHJKLMNPQRSTUVWXYZ23456789"[data[0] % length2]);
			}
		}
		return stringBuilder.ToString();
	}

	private static HashSet<string> BuildAllowedHosts()
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			$"127.0.0.1:{9222}",
			$"localhost:{9222}",
			$"[::1]:{9222}"
		};
		try
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				if (networkInterface.OperationalStatus != OperationalStatus.Up || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
				{
					continue;
				}
				foreach (UnicastIPAddressInformation unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
				{
					if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
					{
						string text = unicastAddress.Address.ToString();
						if (!text.StartsWith("169.254."))
						{
							hashSet.Add($"{text}:{9222}");
						}
					}
				}
			}
		}
		catch
		{
		}
		return hashSet;
	}

	private static bool NicLooksVirtual(NetworkInterface nic)
	{
		string text = nic.Name ?? "";
		string text2 = nic.Description ?? "";
		string[] array = new string[16]
		{
			"virtual", "vmware", "vbox", "virtualbox", "hyper-v", "hyperv", "vethernet", "docker", "wsl", "tap-",
			"tun", "loopback", "npcap", "bluetooth", "teredo", "isatap"
		};
		foreach (string value in array)
		{
			if (text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
			if (text2.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
		}
		return false;
	}

	public static void Stop()
	{
		if (!_isRunning)
		{
			return;
		}
		_isRunning = false;
		try
		{
			_cts?.Cancel();
		}
		catch
		{
		}
		foreach (ClientSlot value in _clients.Values)
		{
			CloseSlotAsync(value);
		}
		_clients.Clear();
		try
		{
			_listener?.Stop();
		}
		catch
		{
		}
		try
		{
			_cts?.Dispose();
		}
		catch
		{
		}
		_cts = null;
		_listener = null;
		_resourceCache.Clear();
		_lastBroadcastData = null;
		_lastMapName = null;
		_lastPlayerHash = 0;
		_tokenString = null;
		_tokenBytes = null;
		_allowRemote = false;
		_allowedHosts = null;
		_lastIpSweepMs = 0L;
		_ipSecurity.Clear();
		try
		{
			ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
			if (instance != null)
			{
				ManualLogSource log = ((BasePlugin)instance).Log;
				if (log != null)
				{
					log.LogInfo((object)"[WebRadar] Stopped");
				}
			}
		}
		catch
		{
		}
	}

	private static async Task CloseSlotAsync(ClientSlot slot)
	{
		try
		{
			using CancellationTokenSource timeout = new CancellationTokenSource(500);
			await slot.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "shutdown", timeout.Token);
		}
		catch
		{
		}
		try
		{
			slot.Socket.Dispose();
		}
		catch
		{
		}
		try
		{
			slot.SendGate.Dispose();
		}
		catch
		{
		}
	}

	public static void Broadcast(RadarState state)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		if (!_isRunning || _clients.IsEmpty)
		{
			return;
		}
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		long num2 = Interlocked.Read(ref _lastBroadcastMs);
		if (num - num2 < 66 || Interlocked.CompareExchange(ref _lastBroadcastMs, num, num2) != num2)
		{
			return;
		}
		int num3 = ComputeStateHash(state);
		if (num3 == _lastPlayerHash && state.MapName == _lastMapName && _lastBroadcastData != null)
		{
			return;
		}
		_lastPlayerHash = num3;
		_lastMapName = state.MapName;
		try
		{
			string s = JsonSerializer.Serialize(state, _jsonOpts);
			_lastBroadcastData = Encoding.UTF8.GetBytes(s);
		}
		catch (Exception ex)
		{
			try
			{
				ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
				if (instance == null)
				{
					return;
				}
				ManualLogSource log = ((BasePlugin)instance).Log;
				if (log != null)
				{
					bool flag = default(bool);
					BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(29, 1, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[WebRadar] Serialize failed: ");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(ex.GetType().Name);
					}
					log.LogWarning(val);
				}
				return;
			}
			catch
			{
				return;
			}
		}
		byte[] lastBroadcastData = _lastBroadcastData;
		foreach (KeyValuePair<WebSocket, ClientSlot> client in _clients)
		{
			ClientSlot value = client.Value;
			if (value.Socket.State != WebSocketState.Open)
			{
				if (_clients.TryRemove(client.Key, out var value2))
				{
					CloseSlotAsync(value2);
				}
			}
			else
			{
				SendFrameAsync(value, lastBroadcastData);
			}
		}
	}

	private static async Task SendFrameAsync(ClientSlot slot, byte[] payload)
	{
		if (!(await slot.SendGate.WaitAsync(0).ConfigureAwait(continueOnCapturedContext: false)))
		{
			return;
		}
		try
		{
			using CancellationTokenSource timeout = new CancellationTokenSource(2000);
			await slot.Socket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, endOfMessage: true, timeout.Token).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch
		{
			if (_clients.TryRemove(slot.Socket, out var value))
			{
				CloseSlotAsync(value);
			}
		}
		finally
		{
			try
			{
				slot.SendGate.Release();
			}
			catch
			{
			}
		}
	}

	private static int ComputeStateHash(RadarState state)
	{
		int num = 17;
		num = num * 31 + state.LocalPlayerId;
		if (state.Players != null)
		{
			for (int i = 0; i < state.Players.Length; i++)
			{
				PlayerData playerData = state.Players[i];
				num = num * 31 + (int)(playerData.X * 100f);
				num = num * 31 + (int)(playerData.Y * 100f);
				num = num * 31 + (playerData.IsDead ? 1 : 0);
				num = num * 31 + (playerData.InVent ? 1 : 0);
				num = num * 31 + (playerData.IsImpostor ? 1 : 0);
				num = num * 31 + playerData.ColorId;
				num = num * 31 + playerData.Id;
				num = num * 31 + ((playerData.Name != null) ? playerData.Name.GetHashCode() : 0);
			}
		}
		return num;
	}

	private static async Task AcceptLoop(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
		{
			TcpListener listener = _listener;
			if (listener == null)
			{
				break;
			}
			TcpClient client = null;
			try
			{
				client = await listener.AcceptTcpClientAsync().ConfigureAwait(continueOnCapturedContext: false);
				if (ct.IsCancellationRequested)
				{
					try
					{
						client.Dispose();
						break;
					}
					catch
					{
						break;
					}
				}
				SweepIpSecurityIfDue();
				if (!(await _acceptGate.WaitAsync(0).ConfigureAwait(continueOnCapturedContext: false)))
				{
					try
					{
						client.Dispose();
					}
					catch
					{
					}
					continue;
				}
				TcpClient tcp = client;
				Task.Run(async delegate
				{
					try
					{
						await HandleTcpClient(tcp, ct).ConfigureAwait(continueOnCapturedContext: false);
					}
					catch
					{
					}
					finally
					{
						try
						{
							_acceptGate.Release();
						}
						catch
						{
						}
					}
				});
			}
			catch (ObjectDisposedException)
			{
				break;
			}
			catch (SocketException)
			{
				break;
			}
			catch
			{
				try
				{
					client?.Dispose();
				}
				catch
				{
				}
			}
		}
	}

	private static void SweepIpSecurityIfDue()
	{
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		long num2 = Interlocked.Read(ref _lastIpSweepMs);
		if ((num - num2 < 60000 && _ipSecurity.Count < 5000) || Interlocked.CompareExchange(ref _lastIpSweepMs, num, num2) != num2)
		{
			return;
		}
		(int, long) value;
		foreach (KeyValuePair<string, (int, long)> item in _ipSecurity)
		{
			if (item.Value.Item2 > 0 && item.Value.Item2 <= num)
			{
				_ipSecurity.TryRemove(item.Key, out value);
			}
		}
		if (_ipSecurity.Count < 5000)
		{
			return;
		}
		int num3 = _ipSecurity.Count - 3750;
		foreach (KeyValuePair<string, (int, long)> item2 in _ipSecurity)
		{
			if (num3 <= 0)
			{
				break;
			}
			if (item2.Value.Item2 == 0L && _ipSecurity.TryRemove(item2.Key, out value))
			{
				num3--;
			}
		}
	}

	private static async Task HandleTcpClient(TcpClient tcp, CancellationToken ct)
	{
		string clientIp = null;
		bool handedOffToWs = false;
		NetworkStream stream = null;
		try
		{
			tcp.ReceiveTimeout = 5000;
			tcp.SendTimeout = 5000;
			stream = tcp.GetStream();
			try
			{
				clientIp = ((IPEndPoint)tcp.Client.RemoteEndPoint)?.Address.ToString();
			}
			catch
			{
			}
			bool isLocal = IsLoopback(clientIp);
			if (!isLocal && !_allowRemote)
			{
				await SendHttpResponse(stream, "403 Forbidden", DefaultSecurityHeaders(), "Remote disabled", ct);
				return;
			}
			(int, long) value2;
			if (!isLocal && clientIp != null && _ipSecurity.TryGetValue(clientIp, out (int, long) value))
			{
				long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				if (value.Item2 > num)
				{
					await SendHttpResponse(stream, "429 Too Many Requests", DefaultSecurityHeaders(), "", ct);
					return;
				}
				if (value.Item2 > 0 && value.Item2 <= num)
				{
					_ipSecurity.TryRemove(clientIp, out value2);
				}
			}
			using CancellationTokenSource readCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			readCts.CancelAfter(5000);
			string text = await ReadHttpRequest(stream, readCts.Token).ConfigureAwait(continueOnCapturedContext: false);
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			(string method, string path, Dictionary<string, string> headers) tuple = ParseHttpRequest(text);
			string item = tuple.method;
			string item2 = tuple.path;
			Dictionary<string, string> item3 = tuple.headers;
			string text2 = StripQuery(item2);
			if (text2 == "/favicon.ico")
			{
				await SendHttpResponse(stream, "204 No Content", DefaultSecurityHeaders(), "", ct);
				return;
			}
			if (!_safeResourcePath.IsMatch(text2) && text2 != "/" && text2 != "")
			{
				await SendHttpResponse(stream, "400 Bad Request", DefaultSecurityHeaders(), "", ct);
				return;
			}
			var (flag, tokenSource) = ValidateToken(item2, item3);
			if (!isLocal && !flag)
			{
				if (clientIp != null && tokenSource != 0)
				{
					RecordAuthFail(clientIp);
				}
				await SendHttpResponse(stream, "401 Unauthorized", DefaultSecurityHeaders(), "", ct);
				return;
			}
			if (!isLocal && clientIp != null && flag)
			{
				_ipSecurity.TryRemove(clientIp, out value2);
			}
			bool setCookie = flag && tokenSource == TokenSource.Query;
			if (!IsHostAllowed(item3))
			{
				await SendHttpResponse(stream, "421 Misdirected Request", DefaultSecurityHeaders(), "", ct);
				return;
			}
			if (IsWebSocketUpgrade(item, text2, item3))
			{
				if (_clients.Count >= 5)
				{
					await SendHttpResponse(stream, "503 Service Unavailable", DefaultSecurityHeaders(), "Max clients", ct);
					return;
				}
				if (!IsOriginAllowed(item3))
				{
					await SendHttpResponse(stream, "403 Forbidden", DefaultSecurityHeaders(), "", ct);
					return;
				}
				tcp.ReceiveTimeout = 0;
				tcp.SendTimeout = 0;
				WebSocket ws = null;
				ClientSlot slot = null;
				try
				{
					ws = await HandleWebSocketUpgrade(stream, item3, ct).ConfigureAwait(continueOnCapturedContext: false);
					if (ws == null)
					{
						return;
					}
					slot = new ClientSlot
					{
						Socket = ws,
						SendGate = new SemaphoreSlim(1, 1),
						Ip = (clientIp ?? "unknown")
					};
					if (!_clients.TryAdd(ws, slot))
					{
						try
						{
							slot.SendGate.Dispose();
						}
						catch
						{
						}
						try
						{
							ws.Dispose();
							return;
						}
						catch
						{
							return;
						}
					}
					handedOffToWs = true;
					byte[] lastBroadcastData = _lastBroadcastData;
					if (lastBroadcastData != null)
					{
						SendFrameAsync(slot, lastBroadcastData);
					}
				}
				catch
				{
					try
					{
						slot?.SendGate.Dispose();
					}
					catch
					{
					}
					try
					{
						ws?.Dispose();
					}
					catch
					{
					}
					throw;
				}
				await ReceiveLoop(slot, ct).ConfigureAwait(continueOnCapturedContext: false);
				if (_clients.TryRemove(ws, out var value3))
				{
					await CloseSlotAsync(value3).ConfigureAwait(continueOnCapturedContext: false);
				}
				return;
			}
			if (item == "GET")
			{
				await HandleHttpGet(stream, text2, isLocal, setCookie, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			else if (!(item == "OPTIONS"))
			{
				await SendHttpResponse(stream, "405 Method Not Allowed", DefaultSecurityHeaders(), "", ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				await SendHttpResponse(stream, "204 No Content", DefaultSecurityHeaders(), "", ct).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch
		{
		}
		finally
		{
			if (!handedOffToWs)
			{
				try
				{
					stream?.Dispose();
				}
				catch
				{
				}
				try
				{
					tcp.Dispose();
				}
				catch
				{
				}
			}
		}
	}

	private static void RecordAuthFail(string ip)
	{
		_ipSecurity.AddOrUpdate(ip, (string _) => (fails: 1, banUntil: 0L), delegate(string _, (int fails, long banUntil) cur)
		{
			int num = cur.fails + 1;
			long item = ((num >= 5) ? (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 300000) : 0);
			return (fails: num, banUntil: item);
		});
	}

	private static async Task<string> ReadHttpRequest(NetworkStream stream, CancellationToken ct)
	{
		byte[] buf = new byte[4096];
		using MemoryStream ms = new MemoryStream();
		int total = 0;
		int num;
		while ((num = await stream.ReadAsync(buf.AsMemory(0, buf.Length), ct).ConfigureAwait(continueOnCapturedContext: false)) > 0)
		{
			total += num;
			if (total > 8192)
			{
				return null;
			}
			ms.Write(buf, 0, num);
			if (ContainsCrlfCrlf(ms.GetBuffer(), (int)ms.Length))
			{
				break;
			}
		}
		return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
	}

	private static bool ContainsCrlfCrlf(byte[] data, int len)
	{
		for (int i = 3; i < len; i++)
		{
			if (data[i - 3] == 13 && data[i - 2] == 10 && data[i - 1] == 13 && data[i] == 10)
			{
				return true;
			}
		}
		return false;
	}

	private static (string method, string path, Dictionary<string, string> headers) ParseHttpRequest(string raw)
	{
		string[] array = raw.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.None);
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		string[] array2 = array[0].Split(' ');
		string item = array2[0];
		string item2 = ((array2.Length > 1) ? array2[1] : "/");
		for (int i = 1; i < array.Length; i++)
		{
			string text = array[i];
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			int num = text.IndexOf(':');
			if (num > 0)
			{
				dictionary[text.Substring(0, num).Trim()] = text.Substring(num + 1).Trim();
			}
		}
		return (method: item, path: item2, headers: dictionary);
	}

	private static string StripQuery(string path)
	{
		int num = path.IndexOf('?');
		if (num < 0)
		{
			return path;
		}
		return path.Substring(0, num);
	}

	private static bool IsWebSocketUpgrade(string method, string cleanPath, Dictionary<string, string> headers)
	{
		if (method == "GET" && cleanPath == "/radar/ws" && headers.TryGetValue("Upgrade", out var value))
		{
			return value.IndexOf("websocket", StringComparison.OrdinalIgnoreCase) >= 0;
		}
		return false;
	}

	private static bool IsLoopback(string ip)
	{
		if (!(ip == "127.0.0.1") && !(ip == "::1"))
		{
			return ip == "::ffff:127.0.0.1";
		}
		return true;
	}

	private static bool IsHostAllowed(Dictionary<string, string> headers)
	{
		if (!headers.TryGetValue("Host", out var value))
		{
			return false;
		}
		HashSet<string> allowedHosts = _allowedHosts;
		if (allowedHosts == null)
		{
			return false;
		}
		if (allowedHosts.Contains(value))
		{
			return true;
		}
		HashSet<string> hashSet = BuildAllowedHosts();
		if (hashSet.Contains(value))
		{
			_allowedHosts = hashSet;
			return true;
		}
		return false;
	}

	private static bool IsOriginAllowed(Dictionary<string, string> headers)
	{
		if (!headers.TryGetValue("Origin", out var value))
		{
			return true;
		}
		if (!headers.TryGetValue("Host", out var value2))
		{
			return false;
		}
		string value3 = "http://" + value2;
		return value.Equals(value3, StringComparison.OrdinalIgnoreCase);
	}

	private static (bool ok, TokenSource src) ValidateToken(string path, Dictionary<string, string> headers)
	{
		if (_tokenBytes == null)
		{
			return (ok: false, src: TokenSource.None);
		}
		string text = null;
		TokenSource item = TokenSource.None;
		int num = path.IndexOf('?');
		if (num >= 0)
		{
			string[] array = path.Substring(num + 1).Split('&');
			foreach (string text2 in array)
			{
				int num2 = text2.IndexOf('=');
				if (num2 > 0 && text2.Substring(0, num2).Equals("token", StringComparison.OrdinalIgnoreCase))
				{
					text = text2.Substring(num2 + 1);
					int num3 = text.IndexOfAny(new char[2] { ' ', '#' });
					if (num3 >= 0)
					{
						text = text.Substring(0, num3);
					}
					item = TokenSource.Query;
					break;
				}
			}
		}
		if (text == null && headers.TryGetValue("X-Radar-Token", out var value))
		{
			text = value;
			item = TokenSource.Header;
		}
		if (text == null && headers.TryGetValue("Cookie", out var value2))
		{
			string[] array = value2.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string text3 = array[i].Trim();
				if (text3.StartsWith("mm_radar=", StringComparison.Ordinal))
				{
					text = text3.Substring("mm_radar=".Length);
					item = TokenSource.Cookie;
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			return (ok: false, src: TokenSource.None);
		}
		byte[] bytes;
		try
		{
			bytes = Encoding.ASCII.GetBytes(text);
		}
		catch
		{
			return (ok: false, src: TokenSource.None);
		}
		return (ok: CryptographicOperations.FixedTimeEquals(bytes, _tokenBytes), src: item);
	}

	private static async Task<WebSocket> HandleWebSocketUpgrade(NetworkStream stream, Dictionary<string, string> headers, CancellationToken ct)
	{
		if (!headers.TryGetValue("Sec-WebSocket-Key", out var value))
		{
			return null;
		}
		string text = ComputeAcceptKey(value);
		string s = "HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: " + text + "\r\n\r\n";
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		await stream.WriteAsync(bytes.AsMemory(0, bytes.Length), ct).ConfigureAwait(continueOnCapturedContext: false);
		return WebSocket.CreateFromStream(stream, isServer: true, null, TimeSpan.FromMilliseconds(30000.0));
	}

	private static string ComputeAcceptKey(string key)
	{
		string s = key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
		using SHA1 sHA = SHA1.Create();
		return Convert.ToBase64String(sHA.ComputeHash(Encoding.UTF8.GetBytes(s)));
	}

	private static async Task ReceiveLoop(ClientSlot slot, CancellationToken ct)
	{
		byte[] buffer = new byte[4096];
		WebSocket ws = slot.Socket;
		while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
		{
			try
			{
				if ((await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(continueOnCapturedContext: false)).MessageType == WebSocketMessageType.Close)
				{
					try
					{
						await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
						break;
					}
					catch
					{
						break;
					}
				}
			}
			catch
			{
				break;
			}
		}
	}

	private static async Task HandleHttpGet(NetworkStream stream, string cleanPath, bool isLocal, bool setCookie, CancellationToken ct)
	{
		string text = cleanPath.TrimEnd('/');
		if (text == "" || text == "/radar")
		{
			text = "/radar/index.html";
		}
		if (text == "/radar/share-info")
		{
			if (!isLocal)
			{
				await SendHttpResponse(stream, "403 Forbidden", DefaultSecurityHeaders(), "", ct);
				return;
			}
			string value = ResolveLocalIp();
			_allowedHosts = BuildAllowedHosts();
			string body = $"{{\"token\":\"{_tokenString}\",\"ip\":\"{value}\",\"port\":{9222}}}";
			Dictionary<string, string> dictionary = DefaultSecurityHeaders();
			dictionary["Content-Type"] = "application/json; charset=utf-8";
			dictionary["Cache-Control"] = "no-store";
			if (setCookie && _tokenString != null)
			{
				dictionary["Set-Cookie"] = "mm_radar=" + _tokenString + "; Path=/radar; HttpOnly; SameSite=Strict; Max-Age=86400";
			}
			await SendHttpResponse(stream, "200 OK", dictionary, body, ct);
			return;
		}
		string text2 = PathToResourceName(text);
		if (text2 == null)
		{
			await SendHttpResponse(stream, "404 Not Found", DefaultSecurityHeaders(), "", ct);
			return;
		}
		string mimeType = GetMimeType(text);
		byte[] array = LoadEmbeddedResource(text2);
		if (array == null)
		{
			await SendHttpResponse(stream, "404 Not Found", DefaultSecurityHeaders(), "", ct);
			return;
		}
		Dictionary<string, string> dictionary2 = DefaultSecurityHeaders();
		dictionary2["Content-Type"] = mimeType;
		dictionary2["Cache-Control"] = "no-cache";
		if (setCookie && _tokenString != null)
		{
			dictionary2["Set-Cookie"] = "mm_radar=" + _tokenString + "; Path=/radar; HttpOnly; SameSite=Strict; Max-Age=86400";
		}
		if (mimeType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
		{
			dictionary2["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; img-src 'self' data:; connect-src 'self'; font-src 'self' data: https://fonts.gstatic.com; base-uri 'none'; form-action 'none'; frame-ancestors 'none'";
		}
		await SendHttpResponse(stream, "200 OK", dictionary2, array, ct);
	}

	private static string PathToResourceName(string path)
	{
		string text = path.TrimStart('/');
		if (text.Contains("..") || text.Contains("//"))
		{
			return null;
		}
		string text2 = text;
		foreach (char c in text2)
		{
			if (!char.IsLetterOrDigit(c) && c != '.' && c != '_' && c != '-' && c != '/')
			{
				return null;
			}
		}
		return "ModMenuCrew." + text.Replace('/', '.');
	}

	private static string GetMimeType(string path)
	{
		switch (Path.GetExtension(path).ToLowerInvariant())
		{
		case ".html":
			return "text/html; charset=utf-8";
		case ".css":
			return "text/css; charset=utf-8";
		case ".js":
			return "application/javascript; charset=utf-8";
		case ".json":
			return "application/json; charset=utf-8";
		case ".png":
			return "image/png";
		case ".jpeg":
		case ".jpg":
			return "image/jpeg";
		case ".svg":
			return "image/svg+xml";
		case ".ico":
			return "image/x-icon";
		default:
			return "application/octet-stream";
		}
	}

	private static byte[] LoadEmbeddedResource(string name)
	{
		if (_resourceCache.TryGetValue(name, out var value))
		{
			return value;
		}
		using Stream stream = typeof(WebRadarService).Assembly.GetManifestResourceStream(name);
		if (stream == null)
		{
			return null;
		}
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		byte[] array = memoryStream.ToArray();
		_resourceCache.TryAdd(name, array);
		return array;
	}

	private static Dictionary<string, string> DefaultSecurityHeaders()
	{
		return new Dictionary<string, string>
		{
			{ "X-Content-Type-Options", "nosniff" },
			{ "X-Frame-Options", "DENY" },
			{ "Referrer-Policy", "no-referrer" },
			{ "Permissions-Policy", "geolocation=(), microphone=(), camera=()" },
			{ "Cross-Origin-Resource-Policy", "same-origin" }
		};
	}

	private static async Task SendHttpResponse(NetworkStream stream, string status, Dictionary<string, string> headers, string body, CancellationToken ct)
	{
		await SendHttpResponse(stream, status, headers, Encoding.UTF8.GetBytes(body ?? ""), ct).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static async Task SendHttpResponse(NetworkStream stream, string status, Dictionary<string, string> headers, byte[] body, CancellationToken ct)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder2);
		handler.AppendLiteral("HTTP/1.1 ");
		handler.AppendFormatted(status);
		handler.AppendLiteral("\r\n");
		stringBuilder3.Append(ref handler);
		foreach (KeyValuePair<string, string> header in headers)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(4, 2, stringBuilder2);
			handler.AppendFormatted(header.Key);
			handler.AppendLiteral(": ");
			handler.AppendFormatted(header.Value);
			handler.AppendLiteral("\r\n");
			stringBuilder4.Append(ref handler);
		}
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(18, 1, stringBuilder2);
		handler.AppendLiteral("Content-Length: ");
		handler.AppendFormatted((body != null) ? body.Length : 0);
		handler.AppendLiteral("\r\n");
		stringBuilder5.Append(ref handler);
		stringBuilder.Append("Connection: close\r\n");
		stringBuilder.Append("\r\n");
		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
			await stream.WriteAsync(bytes.AsMemory(0, bytes.Length), ct).ConfigureAwait(continueOnCapturedContext: false);
			if (body != null && body.Length != 0)
			{
				await stream.WriteAsync(body.AsMemory(0, body.Length), ct).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		catch
		{
		}
	}

	private static string ResolveLocalIp()
	{
		string text = null;
		try
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				if (networkInterface.OperationalStatus != OperationalStatus.Up || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
				{
					continue;
				}
				IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
				bool flag = false;
				try
				{
					foreach (GatewayIPAddressInformation gatewayAddress in iPProperties.GatewayAddresses)
					{
						if (gatewayAddress.Address != null && !gatewayAddress.Address.Equals(IPAddress.Any) && !gatewayAddress.Address.Equals(IPAddress.IPv6Any))
						{
							flag = true;
							break;
						}
					}
				}
				catch
				{
				}
				bool flag2 = NicLooksVirtual(networkInterface);
				foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
				{
					if (unicastAddress.Address.AddressFamily != AddressFamily.InterNetwork)
					{
						continue;
					}
					string text2 = unicastAddress.Address.ToString();
					if (!text2.StartsWith("169.254."))
					{
						if (flag && !flag2)
						{
							return text2;
						}
						if (text == null)
						{
							text = text2;
						}
					}
				}
			}
		}
		catch
		{
		}
		return text ?? "127.0.0.1";
	}
}
