using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using Il2CppSystem;
using ModMenuCrew.Features;
using ModMenuCrew.Networking;
using UnityEngine;

namespace ModMenuCrew;

public static class ServerData
{
	public sealed class UISnapshot
	{
		public readonly byte[] LobbyBytecode;

		public readonly byte[] GameBytecode;

		public readonly byte[] PlayerPickBytecode;

		public readonly byte[] BanMenuBytecode;

		public readonly byte[] CheatsBytecode;

		public readonly byte[] SpoofingBytecode;

		public readonly byte[] SettingsBytecode;

		public readonly byte[] SabotageBytecode;

		public readonly byte[] HostControlsBytecode;

		public readonly byte[] ExtrasBytecode;

		public readonly byte[] AnimationsBytecode;

		public readonly long SessionToken;

		public readonly int BytecodeVersion;

		public readonly bool IsValid;

		public UISnapshot(byte[] lobby, byte[] game, byte[] playerPick, byte[] banMenu, byte[] cheats, byte[] spoofing, byte[] settings, byte[] sabotage, long token, int version, byte[] hostControls = null, byte[] extras = null, byte[] animations = null)
		{
			LobbyBytecode = lobby;
			GameBytecode = game;
			PlayerPickBytecode = playerPick;
			BanMenuBytecode = banMenu;
			CheatsBytecode = cheats;
			SpoofingBytecode = spoofing;
			SettingsBytecode = settings;
			SabotageBytecode = sabotage;
			HostControlsBytecode = hostControls;
			ExtrasBytecode = extras;
			AnimationsBytecode = animations;
			SessionToken = token;
			BytecodeVersion = version;
			IsValid = (lobby != null || game != null) && token > 0;
		}
	}

	internal class EncryptedPayload
	{
		public string Ciphertext { get; set; }

		public string Iv { get; set; }

		public string Tag { get; set; }
	}

	internal sealed class SecurityConfig
	{
		private const ulong MASK_SENTINEL = 8809796824969168901uL;

		private ulong _maskB = 8809796824969168901uL;

		public HashSet<byte> RpcFilterIds { get; set; } = new HashSet<byte>();


		public int RpcPlayerLimit { get; set; }

		public int RpcGlobalLimit { get; set; }

		public float RpcPlayerBanSec { get; set; }

		public float RpcGlobalBanSec { get; set; }

		public HashSet<byte> RpcFilterIdsB { get; set; } = new HashSet<byte>();


		public int RpcPlayerLimitB { get; set; }

		public int RpcGlobalLimitB { get; set; }

		public float RpcPlayerBanSecB { get; set; }

		public float RpcGlobalBanSecB { get; set; }

		public int RpcFramePlayerB { get; set; }

		public int RpcFrameGlobalB { get; set; }

		public byte R0 { get; set; }

		public byte RpcTriggerSpores { get; set; }

		public byte RpcSetScanner { get; set; }

		public byte RpcSetInvisibility { get; set; }

		public byte RpcPhantomPoof { get; set; }

		public byte RpcCheckColor { get; set; }

		public byte RpcCastVote { get; set; }

		public byte RpcSetRoleLegacy { get; set; }

		public byte RpcCustomBypass { get; set; }

		public byte RpcTeleport { get; set; }

		public byte RpcTeleportSync { get; set; }

		public byte RpcBroadcastMsg { get; set; }

		public byte RpcMmcHandshake { get; set; }

		public byte RpcEnterVent { get; set; }

		public byte RpcExitVent { get; set; }

		public byte SabotageTrigger { get; set; }

		public byte SabotageRepair { get; set; }

		public byte CommsRepair2 { get; set; }

		public byte MushroomTrigger { get; set; }

		public byte DoorOpenMask { get; set; }

		public byte ConsoleOpenMask { get; set; }

		public byte ConsoleCloseMask { get; set; }

		public byte ConsoleCompleteMask { get; set; }

		public int LightSwitchCount { get; set; }

		public byte SecurityCamOn { get; set; }

		public byte SecurityCamOff { get; set; }

		public float DoorRecloseThrottle { get; set; }

		public float ScannerAutoDisable { get; set; }

		public int TaskBatchSize { get; set; }

		public float TaskDelayMin { get; set; }

		public float TaskDelayMax { get; set; }

		public float BatchPauseDelay { get; set; }

		public float GodModeInterval { get; set; }

		public float KillAlertDuration { get; set; }

		public float ImpostorAttemptCooldown { get; set; }

		public float TeleportMinInterval { get; set; }

		public float TeleportMaxDistance { get; set; }

		public int TeleportMaxAttempts { get; set; }

		public float MmcHandshakeInterval { get; set; }

		public int ModDetectMaxPlayers { get; set; }

		public int ModDetectMaxImpostors { get; set; }

		public float ModDetectMaxSpeed { get; set; }

		public float ModDetectMinKillCd { get; set; }

		public float LobbyTimerDuration { get; set; }

		public string MmcSignature { get; set; } = "";


		public byte X0 { get; set; }

		public byte X1 { get; set; }

		public byte X2 { get; set; }

		public byte X3 { get; set; }

		public byte X4 { get; set; }

		public byte X5 { get; set; }

		public float W0 { get; set; }

		public float W1 { get; set; }

		public float W2 { get; set; }

		public float W3 { get; set; }

		public int W4 { get; set; }

		public int W5 { get; set; }

		public int W6 { get; set; }

		public float W7 { get; set; }

		public float W8 { get; set; }

		public float W9 { get; set; }

		public string Z0 { get; set; } = "";


		public string Z1 { get; set; } = "";


		public float Z2 { get; set; }

		public float W10 { get; set; }

		public float Z3 { get; set; }

		public float W11 { get; set; }

		public float W12 { get; set; }

		public float W13 { get; set; }

		public float W14 { get; set; }

		public float W15 { get; set; }

		public float W16 { get; set; }

		public float W17 { get; set; }

		public float W18 { get; set; }

		public float W19 { get; set; }

		public int W20 { get; set; }

		public int W21 { get; set; }

		public float W22 { get; set; }

		public float W23 { get; set; }

		public int W24 { get; set; }

		public int W25 { get; set; }

		public int W26 { get; set; }

		public int W27 { get; set; }

		public float W28 { get; set; }

		public int W29 { get; set; }

		public int W30 { get; set; }

		public float W33 { get; set; }

		public int W31 { get; set; }

		public int W32 { get; set; }

		public float W34 { get; set; }

		public int W35 { get; set; }

		public int W36 { get; set; }

		public int W37 { get; set; }

		public float W38 { get; set; }

		public float W39 { get; set; }

		public float W40 { get; set; }

		public string Z4 { get; set; } = "";


		public string Z5 { get; set; } = "";


		public string Z6 { get; set; } = "";


		public string Z7 { get; set; } = "";


		public string Z8 { get; set; } = "";


		public string Z9 { get; set; } = "";


		public string Z10 { get; set; } = "";


		public string Za { get; set; } = "";


		public string ZaN { get; set; } = "";


		public long ZaT { get; set; }

		public uint M0 { get; set; }

		public uint M1 { get; set; }

		public uint M2 { get; set; }

		public uint M3 { get; set; }

		public uint M4 { get; set; }

		public int GoKcd { get; set; }

		public int GoSpd { get; set; }

		public int GoIlg { get; set; }

		public int GoClg { get; set; }

		public int GoVtm { get; set; }

		public int GoShfCd { get; set; }

		public int GoShfDur { get; set; }

		public int GoGaDur { get; set; }

		public int GoGaCd { get; set; }

		public int GoSciCd { get; set; }

		public int GoSciBat { get; set; }

		public int GoEngCd { get; set; }

		public int GoEngVt { get; set; }

		public int GoPhCd { get; set; }

		public int GoPhDur { get; set; }

		public int GoTrkCd { get; set; }

		public int GoTrkDur { get; set; }

		public int GoTrkDly { get; set; }

		public int GoNsDur { get; set; }

		public int GoVprDis { get; set; }

		public int GoDetLim { get; set; }

		public int GoImp { get; set; }

		public int GoKDist { get; set; }

		public int GoEmg { get; set; }

		public int GoEmgCd { get; set; }

		public int GoDisc { get; set; }

		public int GoVote { get; set; }

		public int GoComm { get; set; }

		public int GoShrt { get; set; }

		public int GoLng { get; set; }

		public int GoTbar { get; set; }

		public int GoBVis { get; set; }

		public int GoBGst { get; set; }

		public int GoBCfm { get; set; }

		public int GoBAnon { get; set; }

		public int GoBShfSk { get; set; }

		public int GoBImpRo { get; set; }

		public int GoBNsAlr { get; set; }

		public string CmdPing { get; set; } = "";


		public string CmdColor { get; set; } = "";


		public string CmdColour { get; set; } = "";


		public string CmdHelp { get; set; } = "";


		public string CmdStart { get; set; } = "";


		public string CmdRemove { get; set; } = "";


		public string CmdRestore { get; set; } = "";


		public string CmdPublic { get; set; } = "";


		public string CmdPrivate { get; set; } = "";


		public string CmdRainbow { get; set; } = "";


		public string CmdIds { get; set; } = "";


		public string CmdMod { get; set; } = "";


		public string CmdKick { get; set; } = "";


		public string CmdBan { get; set; } = "";


		public string CmdDm { get; set; } = "";


		public string CmdPm { get; set; } = "";


		public string CmdWhisper { get; set; } = "";


		public string CmdModAdd { get; set; } = "";


		public string CmdModRemove { get; set; } = "";


		public string CmdModList { get; set; } = "";


		public string CmdVip { get; set; } = "";


		public string CmdVipAdd { get; set; } = "";


		public string CmdVipRemove { get; set; } = "";


		public string CmdVipList { get; set; } = "";


		public string VipFileName { get; set; } = "";


		public byte RpcSendChatId { get; set; }

		public byte RpcSetNameId { get; set; }

		public float CmdCooldownSec { get; set; }

		public int DmMaxChars { get; set; }

		public string ModFileName { get; set; } = "";


		public string DmTitleTemplate { get; set; } = "";


		public bool IsLoaded { get; set; }

		internal void SetFilterMaskB(HashSet<byte> set)
		{
			ulong num = 0uL;
			if (set != null)
			{
				foreach (byte item in set)
				{
					if (item < 64)
					{
						num |= (ulong)(1L << (int)item);
					}
				}
			}
			_maskB = num ^ 0x7A42AF8937A6C805uL;
		}

		internal bool IsInFilterB(byte callId)
		{
			if (callId >= 64)
			{
				return false;
			}
			return ((_maskB ^ 0x7A42AF8937A6C805L) & (ulong)(1L << (int)callId)) != 0;
		}
	}

	private static readonly int _mainThreadId = Thread.CurrentThread.ManagedThreadId;

	private static long _integrityHashXor = -7328062225247170456L;

	private const long INTEGRITY_HASH_SENTINEL = -7328062225247170456L;

	private static long _lastRealtimeUpdateMs = 0L;

	private const long REALTIME_PROTECTION_WINDOW_MS = 5000L;

	private static long _lastDenialTimestamp = 0L;

	private static int _isLoadedXor = -559038737;

	private static List<TabDefinition> _tabsInternal = new List<TabDefinition>();

	private static List<string> _premiumFeaturesInternal = new List<string>();

	private static HashSet<string> _validControlIds = new HashSet<string>();

	private static Dictionary<string, bool> _toggleStates = new Dictionary<string, bool>();

	private static Dictionary<string, float> _sliderValues = new Dictionary<string, float>();

	private static Dictionary<string, string> _actionIdReverseMap = new Dictionary<string, string>();

	private static long _delayedDenialTimeMs = -1L;

	internal static string SessionDecryptKey { get; private set; }

	public static long LastUpdate { get; private set; }

	public static long LastRealtimeUpdateMs => _lastRealtimeUpdateMs;

	public static bool IsLoaded => (_isLoadedXor ^ -559038737) == 1;

	internal static IReadOnlyList<TabDefinition> Tabs => _tabsInternal.AsReadOnly();

	public static IReadOnlyList<string> PremiumFeatures => _premiumFeaturesInternal.AsReadOnly();

	public static UISnapshot CurrentSnapshot { get; private set; }

	public static byte[] LobbyBytecode => CurrentSnapshot?.LobbyBytecode;

	public static byte[] GameBytecode => CurrentSnapshot?.GameBytecode;

	public static int BytecodeVersion => CurrentSnapshot?.BytecodeVersion ?? 0;

	public static Vector2 RenderOffset { get; private set; } = Vector2.zero;


	internal static SecurityConfig Config { get; private set; } = new SecurityConfig();


	internal static void SetLoaded(bool loaded)
	{
		_isLoadedXor = (loaded ? 1 : 0) ^ -559038737;
		LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}

	internal static void ParseSecurityConfig(JsonElement el)
	{
		try
		{
			SecurityConfig securityConfig = new SecurityConfig();
			if (el.TryGetProperty("rpcValidator", out var value))
			{
				if (value.TryGetProperty("filterIds", out var value2) && value2.ValueKind == JsonValueKind.Array)
				{
					foreach (JsonElement item in value2.EnumerateArray())
					{
						securityConfig.RpcFilterIds.Add((byte)item.GetInt32());
					}
				}
				securityConfig.RpcPlayerLimit = (value.TryGetProperty("playerLimit", out var value3) ? value3.GetInt32() : 0);
				securityConfig.RpcGlobalLimit = (value.TryGetProperty("globalLimit", out value3) ? value3.GetInt32() : 0);
				securityConfig.RpcPlayerBanSec = (value.TryGetProperty("playerBanSec", out value3) ? ((float)value3.GetDouble()) : 0f);
				securityConfig.RpcGlobalBanSec = (value.TryGetProperty("globalBanSec", out value3) ? ((float)value3.GetDouble()) : 0f);
				if (value.TryGetProperty("attackFilterIds", out var value4) && value4.ValueKind == JsonValueKind.Array)
				{
					foreach (JsonElement item2 in value4.EnumerateArray())
					{
						securityConfig.RpcFilterIdsB.Add((byte)item2.GetInt32());
					}
				}
				securityConfig.SetFilterMaskB(securityConfig.RpcFilterIdsB);
				securityConfig.RpcPlayerLimitB = (value.TryGetProperty("attackPlayerLimit", out value3) ? value3.GetInt32() : 0);
				securityConfig.RpcGlobalLimitB = (value.TryGetProperty("attackGlobalLimit", out value3) ? value3.GetInt32() : 0);
				securityConfig.RpcPlayerBanSecB = (value.TryGetProperty("attackPlayerBanSec", out value3) ? ((float)value3.GetDouble()) : 0f);
				securityConfig.RpcGlobalBanSecB = (value.TryGetProperty("attackGlobalBanSec", out value3) ? ((float)value3.GetDouble()) : 0f);
				securityConfig.RpcFramePlayerB = (value.TryGetProperty("attackFramePlayer", out value3) ? value3.GetInt32() : 0);
				securityConfig.RpcFrameGlobalB = (value.TryGetProperty("attackFrameGlobal", out value3) ? value3.GetInt32() : 0);
			}
			if (el.TryGetProperty("rpcIds", out var value5))
			{
				securityConfig.R0 = (byte)(value5.TryGetProperty("a", out var value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcTriggerSpores = (byte)(value5.TryGetProperty("triggerSpores", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcSetScanner = (byte)(value5.TryGetProperty("setScanner", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcSetInvisibility = (byte)(value5.TryGetProperty("setInvisibility", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcPhantomPoof = (byte)(value5.TryGetProperty("phantomPoof", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcCheckColor = (byte)(value5.TryGetProperty("checkColor", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcCastVote = (byte)(value5.TryGetProperty("castVote", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcSetRoleLegacy = (byte)(value5.TryGetProperty("setRoleLegacy", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcCustomBypass = (byte)(value5.TryGetProperty("customBypass", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcTeleport = (byte)(value5.TryGetProperty("teleport", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcTeleportSync = (byte)(value5.TryGetProperty("teleportSync", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcBroadcastMsg = (byte)(value5.TryGetProperty("broadcastMsg", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcMmcHandshake = (byte)(value5.TryGetProperty("mmcHandshake", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcEnterVent = (byte)(value5.TryGetProperty("enterVent", out value6) ? ((byte)value6.GetInt32()) : 0);
				securityConfig.RpcExitVent = (byte)(value5.TryGetProperty("exitVent", out value6) ? ((byte)value6.GetInt32()) : 0);
			}
			if (el.TryGetProperty("sabotage", out var value7))
			{
				securityConfig.SabotageTrigger = (byte)(value7.TryGetProperty("triggerByte", out var value8) ? ((byte)value8.GetInt32()) : 0);
				securityConfig.SabotageRepair = (byte)(value7.TryGetProperty("repairByte", out value8) ? ((byte)value8.GetInt32()) : 0);
				securityConfig.CommsRepair2 = (byte)(value7.TryGetProperty("commsRepair2", out value8) ? ((byte)value8.GetInt32()) : 0);
				securityConfig.MushroomTrigger = (byte)(value7.TryGetProperty("mushroomTrigger", out value8) ? ((byte)value8.GetInt32()) : 0);
				securityConfig.DoorOpenMask = (byte)(value7.TryGetProperty("doorOpenMask", out value8) ? ((byte)value8.GetInt32()) : 0);
				securityConfig.ConsoleOpenMask = (byte)(value7.TryGetProperty("consoleOpenMask", out value8) ? ((byte)value8.GetInt32()) : 0);
				securityConfig.ConsoleCloseMask = (byte)(value7.TryGetProperty("consoleCloseMask", out value8) ? ((byte)value8.GetInt32()) : 0);
				securityConfig.ConsoleCompleteMask = (byte)(value7.TryGetProperty("consoleCompleteMask", out value8) ? ((byte)value8.GetInt32()) : 0);
				securityConfig.LightSwitchCount = (value7.TryGetProperty("lightSwitchCount", out value8) ? value8.GetInt32() : 0);
				securityConfig.SecurityCamOn = (byte)(value7.TryGetProperty("securityCamOn", out value8) ? ((byte)value8.GetInt32()) : 0);
				securityConfig.SecurityCamOff = (byte)(value7.TryGetProperty("securityCamOff", out value8) ? ((byte)value8.GetInt32()) : 0);
				securityConfig.DoorRecloseThrottle = (value7.TryGetProperty("doorRecloseThrottle", out value8) ? ((float)value8.GetDouble()) : 0f);
			}
			if (el.TryGetProperty("timing", out var value9))
			{
				securityConfig.ScannerAutoDisable = (value9.TryGetProperty("scannerAutoDisable", out var value10) ? ((float)value10.GetDouble()) : 0f);
				securityConfig.TaskBatchSize = (value9.TryGetProperty("taskBatchSize", out value10) ? value10.GetInt32() : 0);
				securityConfig.TaskDelayMin = (value9.TryGetProperty("taskDelayMin", out value10) ? ((float)value10.GetDouble()) : 0f);
				securityConfig.TaskDelayMax = (value9.TryGetProperty("taskDelayMax", out value10) ? ((float)value10.GetDouble()) : 0f);
				securityConfig.BatchPauseDelay = (value9.TryGetProperty("batchPauseDelay", out value10) ? ((float)value10.GetDouble()) : 0f);
				securityConfig.GodModeInterval = (value9.TryGetProperty("godModeInterval", out value10) ? ((float)value10.GetDouble()) : 0f);
				securityConfig.KillAlertDuration = (value9.TryGetProperty("killAlertDuration", out value10) ? ((float)value10.GetDouble()) : 0f);
				securityConfig.ImpostorAttemptCooldown = (value9.TryGetProperty("impostorAttemptCooldown", out value10) ? ((float)value10.GetDouble()) : 0f);
				securityConfig.TeleportMinInterval = (value9.TryGetProperty("teleportMinInterval", out value10) ? ((float)value10.GetDouble()) : 0f);
				securityConfig.TeleportMaxDistance = (value9.TryGetProperty("teleportMaxDistance", out value10) ? ((float)value10.GetDouble()) : 0f);
				securityConfig.TeleportMaxAttempts = (value9.TryGetProperty("teleportMaxAttempts", out value10) ? value10.GetInt32() : 0);
				securityConfig.MmcHandshakeInterval = (value9.TryGetProperty("mmcHandshakeInterval", out value10) ? ((float)value10.GetDouble()) : 0f);
			}
			if (el.TryGetProperty("modDetection", out var value11))
			{
				securityConfig.ModDetectMaxPlayers = (value11.TryGetProperty("maxPlayers", out var value12) ? value12.GetInt32() : 0);
				securityConfig.ModDetectMaxImpostors = (value11.TryGetProperty("maxImpostors", out value12) ? value12.GetInt32() : 0);
				securityConfig.ModDetectMaxSpeed = (value11.TryGetProperty("maxSpeed", out value12) ? ((float)value12.GetDouble()) : 0f);
				securityConfig.ModDetectMinKillCd = (value11.TryGetProperty("minKillCd", out value12) ? ((float)value12.GetDouble()) : 0f);
				securityConfig.LobbyTimerDuration = (value11.TryGetProperty("lobbyTimerDuration", out value12) ? ((float)value12.GetDouble()) : 0f);
			}
			if (el.TryGetProperty("mmcSignature", out var value13))
			{
				securityConfig.MmcSignature = value13.GetString() ?? "";
			}
			if (el.TryGetProperty("nx", out var value14))
			{
				securityConfig.X0 = (byte)(value14.TryGetProperty("x0", out var value15) ? ((byte)value15.GetInt32()) : 0);
				securityConfig.X1 = (byte)(value14.TryGetProperty("x1", out value15) ? ((byte)value15.GetInt32()) : 0);
				securityConfig.X2 = (byte)(value14.TryGetProperty("x2", out value15) ? ((byte)value15.GetInt32()) : 0);
				securityConfig.X3 = (byte)(value14.TryGetProperty("x3", out value15) ? ((byte)value15.GetInt32()) : 0);
				securityConfig.X4 = (byte)(value14.TryGetProperty("x4", out value15) ? ((byte)value15.GetInt32()) : 0);
				securityConfig.X5 = (byte)(value14.TryGetProperty("x5", out value15) ? ((byte)value15.GetInt32()) : 0);
				securityConfig.W0 = (value14.TryGetProperty("w0", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W1 = (value14.TryGetProperty("w1", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W2 = (value14.TryGetProperty("w2", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W3 = (value14.TryGetProperty("w3", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W4 = (value14.TryGetProperty("w4", out value15) ? value15.GetInt32() : 0);
				securityConfig.W5 = (value14.TryGetProperty("w5", out value15) ? value15.GetInt32() : 0);
				securityConfig.W6 = (value14.TryGetProperty("w6", out value15) ? value15.GetInt32() : 0);
				securityConfig.W7 = (value14.TryGetProperty("w7", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W8 = (value14.TryGetProperty("w8", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W9 = (value14.TryGetProperty("w9", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.Z0 = (value14.TryGetProperty("z0", out value15) ? (value15.GetString() ?? "") : "");
				securityConfig.Z1 = (value14.TryGetProperty("z1", out value15) ? (value15.GetString() ?? "") : "");
				securityConfig.Z2 = (value14.TryGetProperty("z2", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W10 = (value14.TryGetProperty("w10", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.Z3 = (value14.TryGetProperty("z3", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W11 = (value14.TryGetProperty("w11", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W12 = (value14.TryGetProperty("w12", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W13 = (value14.TryGetProperty("w13", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W14 = (value14.TryGetProperty("w14", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W15 = (value14.TryGetProperty("w15", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W16 = (value14.TryGetProperty("w16", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W17 = (value14.TryGetProperty("w17", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W18 = (value14.TryGetProperty("w18", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W19 = (value14.TryGetProperty("w19", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W20 = (value14.TryGetProperty("w20", out value15) ? value15.GetInt32() : 0);
				securityConfig.W21 = (value14.TryGetProperty("w21", out value15) ? value15.GetInt32() : 0);
				securityConfig.W22 = (value14.TryGetProperty("w22", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W23 = (value14.TryGetProperty("w23", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W24 = (value14.TryGetProperty("w24", out value15) ? value15.GetInt32() : 0);
				securityConfig.W25 = (value14.TryGetProperty("w25", out value15) ? value15.GetInt32() : 0);
				securityConfig.W26 = (value14.TryGetProperty("w26", out value15) ? value15.GetInt32() : 0);
				securityConfig.W27 = (value14.TryGetProperty("w27", out value15) ? value15.GetInt32() : 0);
				securityConfig.W28 = (value14.TryGetProperty("w28", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W29 = (value14.TryGetProperty("w29", out value15) ? value15.GetInt32() : 0);
				securityConfig.W30 = (value14.TryGetProperty("w30", out value15) ? value15.GetInt32() : 0);
				securityConfig.W31 = (value14.TryGetProperty("w31", out value15) ? value15.GetInt32() : 0);
				securityConfig.W32 = (value14.TryGetProperty("w32", out value15) ? value15.GetInt32() : 0);
				securityConfig.W33 = (value14.TryGetProperty("w33", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W34 = (value14.TryGetProperty("w34", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W35 = (value14.TryGetProperty("w35", out value15) ? value15.GetInt32() : 0);
				securityConfig.W36 = (value14.TryGetProperty("w36", out value15) ? value15.GetInt32() : 0);
				securityConfig.W37 = (value14.TryGetProperty("w37", out value15) ? value15.GetInt32() : 0);
				securityConfig.W38 = (value14.TryGetProperty("w38", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W39 = (value14.TryGetProperty("w39", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.W40 = (value14.TryGetProperty("w40", out value15) ? ((float)value15.GetDouble()) : 0f);
				securityConfig.Z4 = (value14.TryGetProperty("z4", out value15) ? (value15.GetString() ?? "") : "");
				securityConfig.Z5 = (value14.TryGetProperty("z5", out value15) ? (value15.GetString() ?? "") : "");
				securityConfig.Z6 = (value14.TryGetProperty("z6", out value15) ? (value15.GetString() ?? "") : "");
				securityConfig.Z7 = (value14.TryGetProperty("z7", out value15) ? (value15.GetString() ?? "") : "");
				securityConfig.Z8 = (value14.TryGetProperty("z8", out value15) ? (value15.GetString() ?? "") : "");
				securityConfig.Z9 = (value14.TryGetProperty("z9", out value15) ? (value15.GetString() ?? "") : "");
				securityConfig.Z10 = (value14.TryGetProperty("z10", out value15) ? (value15.GetString() ?? "") : "");
				securityConfig.Za = (value14.TryGetProperty("za", out value15) ? (value15.GetString() ?? "") : "");
				securityConfig.ZaN = (value14.TryGetProperty("zaN", out value15) ? (value15.GetString() ?? "") : "");
				securityConfig.ZaT = (value14.TryGetProperty("zaT", out value15) ? value15.GetInt64() : 0);
				if (value14.TryGetProperty("m0", out value15))
				{
					securityConfig.M0 = (uint)value15.GetInt32();
				}
				if (value14.TryGetProperty("m1", out value15))
				{
					securityConfig.M1 = (uint)value15.GetInt32();
				}
				if (value14.TryGetProperty("m2", out value15))
				{
					securityConfig.M2 = (uint)value15.GetInt32();
				}
				if (value14.TryGetProperty("m3", out value15))
				{
					securityConfig.M3 = (uint)value15.GetInt32();
				}
				if (value14.TryGetProperty("m4", out value15))
				{
					securityConfig.M4 = (uint)value15.GetInt32();
				}
			}
			if (el.TryGetProperty("go", out var value16))
			{
				securityConfig.GoKcd = (value16.TryGetProperty("kcd", out var value17) ? value17.GetInt32() : 0);
				securityConfig.GoSpd = (value16.TryGetProperty("spd", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoIlg = (value16.TryGetProperty("ilg", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoClg = (value16.TryGetProperty("clg", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoVtm = (value16.TryGetProperty("vtm", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoShfCd = (value16.TryGetProperty("shfcd", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoShfDur = (value16.TryGetProperty("shfdur", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoGaDur = (value16.TryGetProperty("gadur", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoGaCd = (value16.TryGetProperty("gacd", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoSciCd = (value16.TryGetProperty("scicd", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoSciBat = (value16.TryGetProperty("scibat", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoEngCd = (value16.TryGetProperty("engcd", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoEngVt = (value16.TryGetProperty("engvt", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoPhCd = (value16.TryGetProperty("phcd", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoPhDur = (value16.TryGetProperty("phdur", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoTrkCd = (value16.TryGetProperty("trkcd", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoTrkDur = (value16.TryGetProperty("trkdur", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoTrkDly = (value16.TryGetProperty("trkdly", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoNsDur = (value16.TryGetProperty("nsdur", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoVprDis = (value16.TryGetProperty("vprdis", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoDetLim = (value16.TryGetProperty("detlim", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoImp = (value16.TryGetProperty("imp", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoKDist = (value16.TryGetProperty("kdist", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoEmg = (value16.TryGetProperty("emg", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoEmgCd = (value16.TryGetProperty("emgcd", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoDisc = (value16.TryGetProperty("disc", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoVote = (value16.TryGetProperty("vote", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoComm = (value16.TryGetProperty("comm", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoShrt = (value16.TryGetProperty("shrt", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoLng = (value16.TryGetProperty("lng", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoTbar = (value16.TryGetProperty("tbar", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoBVis = (value16.TryGetProperty("bvis", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoBGst = (value16.TryGetProperty("bgst", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoBCfm = (value16.TryGetProperty("bcfm", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoBAnon = (value16.TryGetProperty("banon", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoBShfSk = (value16.TryGetProperty("bshfsk", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoBImpRo = (value16.TryGetProperty("bimpro", out value17) ? value17.GetInt32() : 0);
				securityConfig.GoBNsAlr = (value16.TryGetProperty("bnsalr", out value17) ? value17.GetInt32() : 0);
			}
			if (el.TryGetProperty("chatCommands", out var value18))
			{
				securityConfig.CmdPing = (value18.TryGetProperty("ping", out var value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdColor = (value18.TryGetProperty("color", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdColour = (value18.TryGetProperty("colour", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdHelp = (value18.TryGetProperty("help", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdStart = (value18.TryGetProperty("start", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdRemove = (value18.TryGetProperty("remove", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdRestore = (value18.TryGetProperty("restore", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdPublic = (value18.TryGetProperty("public", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdPrivate = (value18.TryGetProperty("private", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdRainbow = (value18.TryGetProperty("rainbow", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdIds = (value18.TryGetProperty("ids", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdMod = (value18.TryGetProperty("mod", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdKick = (value18.TryGetProperty("kick", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdBan = (value18.TryGetProperty("ban", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdDm = (value18.TryGetProperty("dm", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdPm = (value18.TryGetProperty("pm", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdWhisper = (value18.TryGetProperty("whisper", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdModAdd = (value18.TryGetProperty("modAdd", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdModRemove = (value18.TryGetProperty("modRemove", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdModList = (value18.TryGetProperty("modList", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.CmdVip = (value18.TryGetProperty("vip", out value19) ? (value19.GetString() ?? "/vip") : "/vip");
				securityConfig.CmdVipAdd = (value18.TryGetProperty("vipAdd", out value19) ? (value19.GetString() ?? "add") : "add");
				securityConfig.CmdVipRemove = (value18.TryGetProperty("vipRemove", out value19) ? (value19.GetString() ?? "remove") : "remove");
				securityConfig.CmdVipList = (value18.TryGetProperty("vipList", out value19) ? (value19.GetString() ?? "list") : "list");
				securityConfig.VipFileName = (value18.TryGetProperty("vipFileName", out value19) ? (value19.GetString() ?? "vips.txt") : "vips.txt");
				securityConfig.RpcSendChatId = (byte)(value18.TryGetProperty("sendChat", out value19) ? ((byte)value19.GetInt32()) : 0);
				securityConfig.RpcSetNameId = (byte)(value18.TryGetProperty("setName", out value19) ? ((byte)value19.GetInt32()) : 0);
				securityConfig.CmdCooldownSec = (value18.TryGetProperty("cooldownSec", out value19) ? ((float)value19.GetDouble()) : 0f);
				securityConfig.DmMaxChars = (value18.TryGetProperty("dmMaxChars", out value19) ? value19.GetInt32() : 0);
				securityConfig.ModFileName = (value18.TryGetProperty("modFileName", out value19) ? (value19.GetString() ?? "") : "");
				securityConfig.DmTitleTemplate = (value18.TryGetProperty("dmTitleTemplate", out value19) ? (value19.GetString() ?? "") : "");
			}
			securityConfig.IsLoaded = true;
			Config = securityConfig;
			try
			{
				CrewModeratorService.EnsureLoaded();
			}
			catch
			{
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(Object.op_Implicit("[ServerData] ParseSecurityConfig failed: " + ex.Message));
		}
	}

	private static byte[] HexToBytes(string hex)
	{
		if (string.IsNullOrEmpty(hex))
		{
			return null;
		}
		int length = hex.Length;
		byte[] array = new byte[length / 2];
		for (int i = 0; i < length; i += 2)
		{
			array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
		}
		return array;
	}

	private static string DecryptPayload(EncryptedPayload payload, string sessionKeyHex)
	{
		if (payload == null || string.IsNullOrEmpty(payload.Ciphertext) || string.IsNullOrEmpty(payload.Iv) || string.IsNullOrEmpty(payload.Tag) || string.IsNullOrEmpty(sessionKeyHex))
		{
			return null;
		}
		try
		{
			byte[] key = HexToBytes(sessionKeyHex);
			byte[] nonce = HexToBytes(payload.Iv);
			byte[] tag = HexToBytes(payload.Tag);
			byte[] array = Convert.FromBase64String(payload.Ciphertext);
			byte[] array2 = new byte[array.Length];
			using (AesGcm aesGcm = new AesGcm(key))
			{
				aesGcm.Decrypt(nonce, array, tag, array2);
			}
			return Encoding.UTF8.GetString(array2);
		}
		catch
		{
			return null;
		}
	}

	internal static void ParseFromEncryptedPayload(EncryptedPayload encryptedPayload, string sessionKeyHex, long newSessionToken = 0L, bool isHeartbeat = false)
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string text = DecryptPayload(encryptedPayload, sessionKeyHex);
			if (string.IsNullOrEmpty(text))
			{
				if (isHeartbeat)
				{
					SetLoaded(loaded: false);
					return;
				}
				Debug.LogError(Object.op_Implicit("d0"));
				TriggerSilentDenial();
				return;
			}
			SessionDecryptKey = sessionKeyHex;
			LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			using JsonDocument jsonDocument = JsonDocument.Parse(text);
			JsonElement rootElement = jsonDocument.RootElement;
			List<TabDefinition> list = new List<TabDefinition>();
			List<string> list2 = new List<string>();
			HashSet<string> hashSet = new HashSet<string>();
			byte[] array = null;
			byte[] array2 = null;
			int version = 0;
			if (rootElement.TryGetProperty("render_offset", out var value))
			{
				float num = 0f;
				float num2 = 0f;
				if (value.TryGetProperty("x", out var value2))
				{
					num = (float)value2.GetDouble();
				}
				if (value.TryGetProperty("y", out var value3))
				{
					num2 = (float)value3.GetDouble();
				}
				RenderOffset = new Vector2(num, num2);
			}
			else
			{
				RenderOffset = Vector2.zero;
			}
			if (rootElement.TryGetProperty("security_config", out var value4) && value4.ValueKind == JsonValueKind.Object)
			{
				ParseSecurityConfig(value4);
			}
			rootElement.TryGetProperty("payload_signature", out var _);
			if (rootElement.TryGetProperty("premium_features", out var value6) && value6.ValueKind == JsonValueKind.Array)
			{
				foreach (JsonElement item in value6.EnumerateArray())
				{
					list2.Add(item.GetString() ?? "");
				}
				list2.Sort(StringComparer.Ordinal);
			}
			if (!rootElement.TryGetProperty("tabs", out var value7) || value7.ValueKind != JsonValueKind.Array)
			{
				return;
			}
			foreach (JsonElement item2 in value7.EnumerateArray())
			{
				JsonElement value8;
				JsonElement value9;
				JsonElement value10;
				JsonElement value11;
				JsonElement value12;
				TabDefinition tabDefinition = new TabDefinition
				{
					Id = (item2.TryGetProperty("id", out value8) ? (value8.GetString() ?? "") : ""),
					Name = (item2.TryGetProperty("name", out value9) ? (value9.GetString() ?? "") : ""),
					Icon = (item2.TryGetProperty("icon", out value10) ? (value10.GetString() ?? "") : ""),
					Context = (item2.TryGetProperty("context", out value11) ? (value11.GetString() ?? "both") : "both"),
					Enabled = (item2.TryGetProperty("enabled", out value12) && value12.GetBoolean())
				};
				if (item2.TryGetProperty("sections", out var value13) && value13.ValueKind == JsonValueKind.Array)
				{
					foreach (JsonElement item3 in value13.EnumerateArray())
					{
						JsonElement value14;
						JsonElement value15;
						JsonElement value16;
						JsonElement value17;
						SectionDefinition sectionDefinition = new SectionDefinition
						{
							Id = (item3.TryGetProperty("id", out value14) ? (value14.GetString() ?? "") : ""),
							Name = (item3.TryGetProperty("name", out value15) ? (value15.GetString() ?? "") : ""),
							Visible = (!item3.TryGetProperty("visible", out value16) || value16.GetBoolean()),
							VisibleWhen = (item3.TryGetProperty("visible_when", out value17) ? (value17.GetString() ?? "") : "")
						};
						if (item3.TryGetProperty("buttons", out var value18) && value18.ValueKind == JsonValueKind.Array)
						{
							foreach (JsonElement item4 in value18.EnumerateArray())
							{
								sectionDefinition.Buttons.Add(new ButtonDefinition
								{
									Id = (item4.TryGetProperty("id", out var value19) ? (value19.GetString() ?? "") : ""),
									Label = (item4.TryGetProperty("label", out var value20) ? (value20.GetString() ?? "") : ""),
									Type = (item4.TryGetProperty("type", out var value21) ? (value21.GetString() ?? "") : ""),
									Enabled = (!item4.TryGetProperty("enabled", out var value22) || value22.GetBoolean())
								});
							}
						}
						if (item3.TryGetProperty("sliders", out var value23) && value23.ValueKind == JsonValueKind.Array)
						{
							foreach (JsonElement item5 in value23.EnumerateArray())
							{
								JsonElement value24;
								JsonElement value25;
								JsonElement value26;
								JsonElement value27;
								JsonElement value28;
								JsonElement value29;
								SliderDefinition sliderDefinition = new SliderDefinition
								{
									Id = (item5.TryGetProperty("id", out value24) ? (value24.GetString() ?? "") : ""),
									Label = (item5.TryGetProperty("label", out value25) ? (value25.GetString() ?? "") : ""),
									Min = (item5.TryGetProperty("min", out value26) ? ((float)value26.GetDouble()) : 0f),
									Max = (item5.TryGetProperty("max", out value27) ? ((float)value27.GetDouble()) : 1f),
									Default = (item5.TryGetProperty("default", out value28) ? ((float)value28.GetDouble()) : 0f),
									Step = (item5.TryGetProperty("step", out value29) ? ((float)value29.GetDouble()) : 0.1f)
								};
								sectionDefinition.Sliders.Add(sliderDefinition);
								if (!_sliderValues.ContainsKey(sliderDefinition.Id))
								{
									_sliderValues[sliderDefinition.Id] = sliderDefinition.Default;
								}
							}
						}
						tabDefinition.Sections.Add(sectionDefinition);
					}
				}
				if (item2.TryGetProperty("locations", out var value30) && value30.ValueKind == JsonValueKind.Array)
				{
					foreach (JsonElement item6 in value30.EnumerateArray())
					{
						tabDefinition.Locations.Add(new TeleportLocation
						{
							Id = (item6.TryGetProperty("id", out var value31) ? (value31.GetString() ?? "") : ""),
							Name = (item6.TryGetProperty("name", out var value32) ? (value32.GetString() ?? "") : ""),
							X = (item6.TryGetProperty("x", out var value33) ? ((float)value33.GetDouble()) : 0f),
							Y = (item6.TryGetProperty("y", out var value34) ? ((float)value34.GetDouble()) : 0f)
						});
					}
				}
				if (item2.TryGetProperty("buttons", out var value35) && value35.ValueKind == JsonValueKind.Array)
				{
					SectionDefinition sectionDefinition2 = new SectionDefinition
					{
						Id = "default",
						Name = "",
						Visible = true
					};
					foreach (JsonElement item7 in value35.EnumerateArray())
					{
						sectionDefinition2.Buttons.Add(new ButtonDefinition
						{
							Id = (item7.TryGetProperty("id", out var value36) ? (value36.GetString() ?? "") : ""),
							Label = (item7.TryGetProperty("label", out var value37) ? (value37.GetString() ?? "") : ""),
							Type = (item7.TryGetProperty("type", out var value38) ? (value38.GetString() ?? "") : ""),
							Enabled = (!item7.TryGetProperty("enabled", out var value39) || value39.GetBoolean())
						});
					}
					if (sectionDefinition2.Buttons.Count > 0)
					{
						tabDefinition.Sections.Add(sectionDefinition2);
					}
				}
				if (item2.TryGetProperty("sliders", out var value40) && value40.ValueKind == JsonValueKind.Array)
				{
					SectionDefinition sectionDefinition3 = ((tabDefinition.Sections.Count > 0) ? tabDefinition.Sections[tabDefinition.Sections.Count - 1] : new SectionDefinition
					{
						Id = "default",
						Visible = true
					});
					foreach (JsonElement item8 in value40.EnumerateArray())
					{
						JsonElement value41;
						JsonElement value42;
						JsonElement value43;
						JsonElement value44;
						JsonElement value45;
						JsonElement value46;
						SliderDefinition sliderDefinition2 = new SliderDefinition
						{
							Id = (item8.TryGetProperty("id", out value41) ? (value41.GetString() ?? "") : ""),
							Label = (item8.TryGetProperty("label", out value42) ? (value42.GetString() ?? "") : ""),
							Min = (item8.TryGetProperty("min", out value43) ? ((float)value43.GetDouble()) : 0f),
							Max = (item8.TryGetProperty("max", out value44) ? ((float)value44.GetDouble()) : 1f),
							Default = (item8.TryGetProperty("default", out value45) ? ((float)value45.GetDouble()) : 0f),
							Step = (item8.TryGetProperty("step", out value46) ? ((float)value46.GetDouble()) : 0.1f)
						};
						sectionDefinition3.Sliders.Add(sliderDefinition2);
						if (!_sliderValues.ContainsKey(sliderDefinition2.Id))
						{
							_sliderValues[sliderDefinition2.Id] = sliderDefinition2.Default;
						}
					}
					if (!tabDefinition.Sections.Contains(sectionDefinition3))
					{
						tabDefinition.Sections.Add(sectionDefinition3);
					}
				}
				if (tabDefinition.Sections != null)
				{
					foreach (SectionDefinition section in tabDefinition.Sections)
					{
						if (section.Buttons != null)
						{
							foreach (ButtonDefinition button in section.Buttons)
							{
								if (!string.IsNullOrEmpty(button.Id))
								{
									hashSet.Add(button.Id);
								}
							}
						}
						if (section.Sliders == null)
						{
							continue;
						}
						foreach (SliderDefinition slider in section.Sliders)
						{
							if (!string.IsNullOrEmpty(slider.Id))
							{
								hashSet.Add(slider.Id);
							}
						}
					}
				}
				list.Add(tabDefinition);
			}
			if (rootElement.TryGetProperty("bytecode_version", out var value47))
			{
				version = value47.GetInt32();
			}
			if (rootElement.TryGetProperty("lobby_bytecode", out var value48))
			{
				string @string = value48.GetString();
				if (!string.IsNullOrEmpty(@string))
				{
					try
					{
						array = Convert.FromBase64String(@string);
					}
					catch
					{
						array = null;
					}
				}
			}
			if (rootElement.TryGetProperty("game_bytecode", out var value49))
			{
				string string2 = value49.GetString();
				if (!string.IsNullOrEmpty(string2))
				{
					try
					{
						array2 = Convert.FromBase64String(string2);
					}
					catch
					{
						array2 = null;
					}
				}
			}
			byte[] array3 = null;
			if (rootElement.TryGetProperty("playerpick_bytecode", out var value50))
			{
				string string3 = value50.GetString();
				if (!string.IsNullOrEmpty(string3))
				{
					try
					{
						array3 = Convert.FromBase64String(string3);
					}
					catch
					{
						array3 = null;
					}
				}
			}
			byte[] array4 = null;
			if (rootElement.TryGetProperty("banmenu_bytecode", out var value51))
			{
				string string4 = value51.GetString();
				if (!string.IsNullOrEmpty(string4))
				{
					try
					{
						array4 = Convert.FromBase64String(string4);
					}
					catch
					{
						array4 = null;
					}
				}
			}
			long num3 = 0L;
			num3 = ExtractV5Token(array);
			if (num3 == 0L)
			{
				num3 = ExtractV5Token(array2);
			}
			if (num3 == 0L && newSessionToken > 0)
			{
				num3 = newSessionToken;
				if (array == null && array2 == null)
				{
					Debug.LogWarning(Object.op_Implicit("d1"));
				}
			}
			if (num3 == 0L)
			{
				num3 = ModKeyValidator.CurrentSessionToken;
				Debug.LogWarning(Object.op_Implicit("d2"));
			}
			byte[] array5 = null;
			if (rootElement.TryGetProperty("cheats_bytecode", out var value52))
			{
				string string5 = value52.GetString();
				if (!string.IsNullOrEmpty(string5))
				{
					try
					{
						array5 = Convert.FromBase64String(string5);
					}
					catch
					{
						array5 = null;
					}
				}
			}
			byte[] array6 = null;
			if (rootElement.TryGetProperty("spoofing_bytecode", out var value53))
			{
				string string6 = value53.GetString();
				if (!string.IsNullOrEmpty(string6))
				{
					try
					{
						array6 = Convert.FromBase64String(string6);
					}
					catch
					{
						array6 = null;
					}
				}
			}
			byte[] array7 = null;
			if (rootElement.TryGetProperty("settings_bytecode", out var value54))
			{
				string string7 = value54.GetString();
				if (!string.IsNullOrEmpty(string7))
				{
					try
					{
						array7 = Convert.FromBase64String(string7);
					}
					catch
					{
						array7 = null;
					}
				}
			}
			byte[] sabotage = null;
			if (rootElement.TryGetProperty("sabotage_bytecode", out var value55))
			{
				string string8 = value55.GetString();
				if (!string.IsNullOrEmpty(string8))
				{
					try
					{
						sabotage = Convert.FromBase64String(string8);
					}
					catch
					{
						sabotage = null;
					}
				}
			}
			byte[] hostControls = null;
			if (rootElement.TryGetProperty("hostcontrols_bytecode", out var value56))
			{
				string string9 = value56.GetString();
				if (!string.IsNullOrEmpty(string9))
				{
					try
					{
						hostControls = Convert.FromBase64String(string9);
					}
					catch
					{
						hostControls = null;
					}
				}
			}
			byte[] array8 = null;
			if (rootElement.TryGetProperty("extras_bytecode", out var value57))
			{
				string string10 = value57.GetString();
				if (!string.IsNullOrEmpty(string10))
				{
					try
					{
						array8 = Convert.FromBase64String(string10);
					}
					catch
					{
						array8 = null;
					}
				}
			}
			byte[] array9 = null;
			if (rootElement.TryGetProperty("animations_bytecode", out var value58))
			{
				string string11 = value58.GetString();
				if (!string.IsNullOrEmpty(string11))
				{
					try
					{
						array9 = Convert.FromBase64String(string11);
					}
					catch
					{
						array9 = null;
					}
				}
			}
			UISnapshot currentSnapshot = CurrentSnapshot;
			long num4 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			if (isHeartbeat && currentSnapshot != null)
			{
				byte[] array10 = currentSnapshot.GameBytecode ?? currentSnapshot.LobbyBytecode;
				byte[] array11 = array2 ?? array;
				if (array10 != null && array10.Length >= 536 && array11 != null && array11.Length >= 536 && array10[0] == 80 && array10[1] == 79 && array10[2] == 76 && array10[3] == 53 && array11[0] == 80 && array11[1] == 79 && array11[2] == 76 && array11[3] == 53)
				{
					long num5 = BitConverter.ToInt64(array10, 528);
					long num6 = BitConverter.ToInt64(array11, 528);
					if (num6 > 0 && num5 > 0 && num6 < num5)
					{
						return;
					}
				}
			}
			long serverNowMs;
			if (_lastRealtimeUpdateMs > 0 && num4 - _lastRealtimeUpdateMs < 5000 && currentSnapshot != null)
			{
				serverNowMs = num4 + ModKeyValidator.ServerTimeOffsetMs;
				if (array3 != null && currentSnapshot.PlayerPickBytecode != null && IsBytecodesFresh(currentSnapshot.PlayerPickBytecode))
				{
					array3 = currentSnapshot.PlayerPickBytecode;
				}
				if (array4 != null && currentSnapshot.BanMenuBytecode != null && IsBytecodesFresh(currentSnapshot.BanMenuBytecode))
				{
					array4 = currentSnapshot.BanMenuBytecode;
				}
				if (array5 != null && currentSnapshot.CheatsBytecode != null && IsBytecodesFresh(currentSnapshot.CheatsBytecode))
				{
					array5 = currentSnapshot.CheatsBytecode;
				}
				if (array6 != null && currentSnapshot.SpoofingBytecode != null && IsBytecodesFresh(currentSnapshot.SpoofingBytecode))
				{
					array6 = currentSnapshot.SpoofingBytecode;
				}
				if (array7 != null && currentSnapshot.SettingsBytecode != null && IsBytecodesFresh(currentSnapshot.SettingsBytecode))
				{
					array7 = currentSnapshot.SettingsBytecode;
				}
				if (array8 != null && currentSnapshot.ExtrasBytecode != null && IsBytecodesFresh(currentSnapshot.ExtrasBytecode))
				{
					array8 = currentSnapshot.ExtrasBytecode;
				}
				if (array9 != null && currentSnapshot.AnimationsBytecode != null && IsBytecodesFresh(currentSnapshot.AnimationsBytecode))
				{
					array9 = currentSnapshot.AnimationsBytecode;
				}
			}
			UISnapshot currentSnapshot2 = new UISnapshot(array, array2, array3, array4, array5, array6, array7, sabotage, num3, version, hostControls, array8, array9);
			_tabsInternal = list;
			_premiumFeaturesInternal = list2;
			_validControlIds = hashSet;
			CurrentSnapshot = currentSnapshot2;
			if (newSessionToken > 0)
			{
				ModKeyValidator.SetSessionTokenInternal(newSessionToken);
				GhostUI.InvalidateHMACCache();
			}
			_integrityHashXor = CalculateIntegrity() ^ -7328062225247170456L;
			SetLoaded(loaded: true);
			bool IsBytecodesFresh(byte[] bc)
			{
				if (bc != null && bc.Length >= 536 && bc[0] == 80 && bc[1] == 79 && bc[2] == 76 && bc[3] == 53)
				{
					long num7 = BitConverter.ToInt64(bc, 528);
					if (num7 > 0)
					{
						return serverNowMs - num7 < 300000;
					}
					return false;
				}
				return false;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(Object.op_Implicit("[ServerData] ParseFromEncryptedPayload failed: " + ex.Message));
		}
		static long ExtractV5Token(byte[] bytes)
		{
			if (bytes != null && bytes.Length >= 268 && bytes[0] == 80 && bytes[1] == 79 && bytes[2] == 76 && bytes[3] == 53)
			{
				return BitConverter.ToInt64(bytes, 260);
			}
			return 0L;
		}
	}

	internal static void UpdateFromRealtime(byte[] lobbyBytecode, byte[] gameBytecode, byte[] playerPickBytecode, byte[] banMenuBytecode, byte[] cheatsBytecode, byte[] spoofingBytecode, byte[] settingsBytecode, long sessionToken, byte[] sabotageBytecode = null, byte[] hostControlsBytecode = null, byte[] extrasBytecode = null, byte[] animationsBytecode = null)
	{
		try
		{
			if (lobbyBytecode != null || gameBytecode != null || playerPickBytecode != null || banMenuBytecode != null || cheatsBytecode != null || spoofingBytecode != null || settingsBytecode != null || sabotageBytecode != null || hostControlsBytecode != null || extrasBytecode != null || animationsBytecode != null)
			{
				UISnapshot currentSnapshot = CurrentSnapshot;
				byte[] obj = lobbyBytecode ?? currentSnapshot?.LobbyBytecode;
				byte[] array = gameBytecode ?? currentSnapshot?.GameBytecode;
				byte[] playerPick = playerPickBytecode ?? currentSnapshot?.PlayerPickBytecode;
				byte[] banMenu = banMenuBytecode ?? currentSnapshot?.BanMenuBytecode;
				byte[] cheats = cheatsBytecode ?? currentSnapshot?.CheatsBytecode;
				byte[] spoofing = spoofingBytecode ?? currentSnapshot?.SpoofingBytecode;
				byte[] settings = settingsBytecode ?? currentSnapshot?.SettingsBytecode;
				byte[] sabotage = sabotageBytecode ?? currentSnapshot?.SabotageBytecode;
				byte[] hostControls = hostControlsBytecode ?? currentSnapshot?.HostControlsBytecode;
				byte[] extras = extrasBytecode ?? currentSnapshot?.ExtrasBytecode;
				byte[] animations = animationsBytecode ?? currentSnapshot?.AnimationsBytecode;
				long num = 0L;
				if (lobbyBytecode != null)
				{
					num = ExtractV5Token(lobbyBytecode);
				}
				if (num == 0L && gameBytecode != null)
				{
					num = ExtractV5Token(gameBytecode);
				}
				long num2 = ExtractV5Token(obj);
				if (num2 == 0L)
				{
					num2 = ExtractV5Token(array);
				}
				long token = ((num > 0) ? ((sessionToken > 0) ? sessionToken : num) : ((num2 <= 0) ? ((sessionToken > 0) ? sessionToken : (currentSnapshot?.SessionToken ?? 0)) : num2));
				int version = currentSnapshot?.BytecodeVersion ?? 5;
				CurrentSnapshot = new UISnapshot(obj, array, playerPick, banMenu, cheats, spoofing, settings, sabotage, token, version, hostControls, extras, animations);
				_lastRealtimeUpdateMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				if (sessionToken > 0)
				{
					ModKeyValidator.SetSessionTokenInternal(sessionToken);
					GhostUI.InvalidateHMACCache();
				}
				_integrityHashXor = CalculateIntegrity() ^ -7328062225247170456L;
				if (!IsLoaded)
				{
					SetLoaded(loaded: true);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(Object.op_Implicit(ex.Message));
		}
		static long ExtractV5Token(byte[] bc)
		{
			if (bc != null && bc.Length >= 268 && bc[0] == 80 && bc[1] == 79 && bc[2] == 76 && bc[3] == 53)
			{
				return BitConverter.ToInt64(bc, 260);
			}
			return 0L;
		}
	}

	internal static void RegisterControlId(string id)
	{
		if (!string.IsNullOrEmpty(id))
		{
			_validControlIds.Add(id);
		}
	}

	internal static bool GetToggleState(string id)
	{
		bool value;
		return _toggleStates.TryGetValue(id, out value) && value;
	}

	internal static void SetToggleState(string id, bool value)
	{
		if (!_validControlIds.Contains(id))
		{
			if (IsLoaded)
			{
				TriggerSilentDenial();
			}
		}
		else
		{
			_toggleStates[id] = value;
		}
	}

	internal static void ToggleState(string id)
	{
		if (!_validControlIds.Contains(id))
		{
			if (IsLoaded)
			{
				TriggerSilentDenial();
			}
		}
		else
		{
			_toggleStates[id] = !GetToggleState(id);
		}
	}

	internal static float GetSliderValue(string id)
	{
		if (!_sliderValues.TryGetValue(id, out var value))
		{
			return 0f;
		}
		return value;
	}

	internal static void SetSliderValue(string id, float value)
	{
		if (!_validControlIds.Contains(id))
		{
			if (IsLoaded)
			{
				TriggerSilentDenial();
			}
		}
		else
		{
			_sliderValues[id] = value;
		}
	}

	internal static void SetSliderValueInternal(string id, float value)
	{
		_sliderValues[id] = value;
	}

	internal static void SetToggleStateInternal(string id, bool value)
	{
		_toggleStates[id] = value;
	}

	internal static void Clear()
	{
		SetLoaded(loaded: false);
		_tabsInternal.Clear();
		_premiumFeaturesInternal.Clear();
		_validControlIds.Clear();
		_toggleStates.Clear();
		_sliderValues.Clear();
		SessionDecryptKey = null;
		LastUpdate = 0L;
		_integrityHashXor = -7328062225247170456L;
		CurrentSnapshot = null;
	}

	private static long GetStableHash(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return 0L;
		}
		long num = 23L;
		foreach (char c in str)
		{
			num = num * 31 + c;
		}
		return num;
	}

	internal static long GetStoredIntegrityHash()
	{
		return _integrityHashXor ^ -7328062225247170456L;
	}

	internal static long CalculateIntegrity()
	{
		long num = 3735928559L;
		if (PremiumFeatures != null)
		{
			foreach (string premiumFeature in PremiumFeatures)
			{
				num = (num * 397) ^ GetStableHash(premiumFeature);
			}
		}
		return num;
	}

	internal static void TriggerSilentDenial()
	{
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		long num2 = Interlocked.Read(ref _lastDenialTimestamp);
		if (num - num2 < 2000)
		{
			return;
		}
		Interlocked.Exchange(ref _lastDenialTimestamp, num);
		if (Thread.CurrentThread.ManagedThreadId != _mainThreadId)
		{
			SessionDecryptKey = "CORRUPTED_SESSION_" + Guid.NewGuid();
			_isLoadedXor = 0;
			CurrentSnapshot = null;
			_integrityHashXor = -7328062225247170456L;
			ActionPermitSystem.EnqueueMainThread(delegate
			{
				TriggerSilentDenial();
			});
			return;
		}
		try
		{
			ModKeyValidator.ResetValidationState();
		}
		catch
		{
		}
		try
		{
			ActionPermitSystem.ClearRegistry();
		}
		catch
		{
		}
		try
		{
			RealtimeConnection.Disconnect();
		}
		catch
		{
		}
		SessionDecryptKey = "CORRUPTED_SESSION_" + Guid.NewGuid();
		_isLoadedXor = 0;
		_tabsInternal?.Clear();
		CurrentSnapshot = null;
		_integrityHashXor = -7328062225247170456L;
	}

	internal static void UpdateActionIdMap(JsonElement mapElement)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(_actionIdReverseMap);
		foreach (JsonProperty item in mapElement.EnumerateObject())
		{
			if (item.Value.ValueKind == JsonValueKind.String)
			{
				dictionary[item.Name] = item.Value.GetString();
			}
		}
		_actionIdReverseMap = dictionary;
	}

	internal static string DeobfuscateActionId(string obfuscatedId)
	{
		if (string.IsNullOrEmpty(obfuscatedId))
		{
			return obfuscatedId;
		}
		if (!_actionIdReverseMap.TryGetValue(obfuscatedId, out var value))
		{
			return obfuscatedId;
		}
		return value;
	}

	internal static void ScheduleDelayedDenial(float delaySeconds)
	{
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)(delaySeconds * 1000f);
		if (_delayedDenialTimeMs < 0 || num < _delayedDenialTimeMs)
		{
			_delayedDenialTimeMs = num;
		}
	}

	internal static void CheckDelayedDenial()
	{
		if (_delayedDenialTimeMs > 0 && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() >= _delayedDenialTimeMs)
		{
			_delayedDenialTimeMs = -1L;
			TriggerSilentDenial();
		}
	}

	internal static void CancelDelayedDenial()
	{
		_delayedDenialTimeMs = -1L;
	}

	internal static bool IsTabEnabled(string tabId)
	{
		if (!IsLoaded || Tabs == null || Tabs.Count == 0)
		{
			return false;
		}
		foreach (TabDefinition tab in Tabs)
		{
			if (tab.Id.Equals(tabId, StringComparison.OrdinalIgnoreCase) && tab.Enabled)
			{
				return true;
			}
		}
		return false;
	}
}
