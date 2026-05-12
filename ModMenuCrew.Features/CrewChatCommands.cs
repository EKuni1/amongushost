using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using Hazel;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using ModMenuCrew.UI.Menus;
using UnityEngine;

namespace ModMenuCrew.Features;

[HarmonyPatch]
public static class CrewChatCommands
{
	private const int K_PING = 1;

	private const int K_COLOR = 2;

	private const int K_HELP = 3;

	private const int K_START = 4;

	private const int K_MOD = 5;

	private const int K_KICK = 6;

	private const int K_BAN = 7;

	private const int K_DM = 8;

	private const int K_VIP = 9;

	private const int K_REMOVE = 10;

	private const int K_RESTORE = 11;

	private const int K_PUBLIC = 12;

	private const int K_PRIVATE = 13;

	private const int K_RAINBOW = 14;

	private const int K_IDS = 15;

	private static object _cachedConfigRef;

	private static Dictionary<string, int> _cmdMap;

	private static DateTime _lastCommandTime = DateTime.MinValue;

	private static readonly Dictionary<string, byte> _colorMap = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase)
	{
		{ "red", 0 },
		{ "blue", 1 },
		{ "green", 2 },
		{ "darkgreen", 2 },
		{ "pink", 3 },
		{ "orange", 4 },
		{ "yellow", 5 },
		{ "black", 6 },
		{ "white", 7 },
		{ "purple", 8 },
		{ "brown", 9 },
		{ "cyan", 10 },
		{ "lime", 11 },
		{ "maroon", 12 },
		{ "rose", 13 },
		{ "banana", 14 },
		{ "gray", 15 },
		{ "grey", 15 },
		{ "tan", 16 },
		{ "coral", 17 }
	};

	private static readonly string[] _colorNames = new string[18]
	{
		"Red", "Blue", "Green", "Pink", "Orange", "Yellow", "Black", "White", "Purple", "Brown",
		"Cyan", "Lime", "Maroon", "Rose", "Banana", "Gray", "Tan", "Coral"
	};

	private static Dictionary<string, int> GetDispatchMap()
	{
		ServerData.SecurityConfig config = ServerData.Config;
		if (config == null)
		{
			return null;
		}
		if (_cachedConfigRef != config)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			if (!string.IsNullOrEmpty(config.CmdPing))
			{
				dictionary[config.CmdPing] = 1;
			}
			if (!string.IsNullOrEmpty(config.CmdColor))
			{
				dictionary[config.CmdColor] = 2;
			}
			if (!string.IsNullOrEmpty(config.CmdColour))
			{
				dictionary[config.CmdColour] = 2;
			}
			if (!string.IsNullOrEmpty(config.CmdHelp))
			{
				dictionary[config.CmdHelp] = 3;
			}
			if (!string.IsNullOrEmpty(config.CmdStart))
			{
				dictionary[config.CmdStart] = 4;
			}
			if (!string.IsNullOrEmpty(config.CmdRemove))
			{
				dictionary[config.CmdRemove] = 10;
			}
			if (!string.IsNullOrEmpty(config.CmdRestore))
			{
				dictionary[config.CmdRestore] = 11;
			}
			if (!string.IsNullOrEmpty(config.CmdPublic))
			{
				dictionary[config.CmdPublic] = 12;
			}
			if (!string.IsNullOrEmpty(config.CmdPrivate))
			{
				dictionary[config.CmdPrivate] = 13;
			}
			if (!string.IsNullOrEmpty(config.CmdRainbow))
			{
				dictionary[config.CmdRainbow] = 14;
			}
			if (!string.IsNullOrEmpty(config.CmdIds))
			{
				dictionary[config.CmdIds] = 15;
			}
			if (!string.IsNullOrEmpty(config.CmdMod))
			{
				dictionary[config.CmdMod] = 5;
			}
			if (!string.IsNullOrEmpty(config.CmdVip))
			{
				dictionary[config.CmdVip] = 9;
			}
			if (!string.IsNullOrEmpty(config.CmdKick))
			{
				dictionary[config.CmdKick] = 6;
			}
			if (!string.IsNullOrEmpty(config.CmdBan))
			{
				dictionary[config.CmdBan] = 7;
			}
			if (!string.IsNullOrEmpty(config.CmdDm))
			{
				dictionary[config.CmdDm] = 8;
			}
			if (!string.IsNullOrEmpty(config.CmdPm))
			{
				dictionary[config.CmdPm] = 8;
			}
			if (!string.IsNullOrEmpty(config.CmdWhisper))
			{
				dictionary[config.CmdWhisper] = 8;
			}
			_cmdMap = dictionary;
			_cachedConfigRef = config;
		}
		return _cmdMap;
	}

	private static float Cooldown()
	{
		float num = ServerData.Config?.CmdCooldownSec ?? 0f;
		if (!(num > 0f))
		{
			return 3f;
		}
		return num;
	}

	[HarmonyPatch(typeof(GameManager), "StartGame")]
	[HarmonyPostfix]
	public static void Postfix_StartGame()
	{
		_lastCommandTime = DateTime.MinValue;
	}

	[HarmonyPatch(typeof(ChatController), "SendChat")]
	[HarmonyPrefix]
	public static bool Prefix_SendChat(ChatController __instance)
	{
		if (string.IsNullOrWhiteSpace(__instance.freeChatField.Text) || ((AbstractChatInputField)__instance.quickChatField).Visible)
		{
			return true;
		}
		string text = __instance.freeChatField.Text.Trim();
		if (!text.StartsWith("/"))
		{
			return true;
		}
		string[] array = text.Split(' ');
		if (array[0].Equals("/bw", StringComparison.OrdinalIgnoreCase))
		{
			if ((Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				HandleBannedWord(array);
			}
			try
			{
				((AbstractChatInputField)__instance.freeChatField).Clear();
			}
			catch
			{
			}
			return false;
		}
		if (array[0].Equals("/banlist", StringComparison.OrdinalIgnoreCase))
		{
			HandleBanList();
			try
			{
				((AbstractChatInputField)__instance.freeChatField).Clear();
			}
			catch
			{
			}
			return false;
		}
		if (array[0].Equals("/unban", StringComparison.OrdinalIgnoreCase))
		{
			if ((Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				HandleUnban(array);
			}
			else
			{
				Local("<color=#ff5555>Host-only.</color>");
			}
			try
			{
				((AbstractChatInputField)__instance.freeChatField).Clear();
			}
			catch
			{
			}
			return false;
		}
		Dictionary<string, int> dispatchMap = GetDispatchMap();
		if (dispatchMap == null || dispatchMap.Count == 0)
		{
			return true;
		}
		if (!dispatchMap.TryGetValue(array[0], out var value))
		{
			return true;
		}
		bool flag = (Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmHost;
		float num = Cooldown();
		switch (value)
		{
		case 1:
			HandlePing(num);
			break;
		case 5:
			HandleMod(array);
			break;
		case 9:
			HandleVip(array);
			break;
		case 3:
			HandleHelp();
			break;
		case 2:
			if (!flag)
			{
				return true;
			}
			HandleColor(array, PlayerControl.LocalPlayer);
			break;
		case 4:
			if (!flag)
			{
				return true;
			}
			HandleHostStart();
			break;
		case 10:
			if (!flag)
			{
				return true;
			}
			HandleHostRemove();
			break;
		case 11:
			if (!flag)
			{
				return true;
			}
			HandleHostRestore(array);
			break;
		case 12:
			if (!flag)
			{
				return true;
			}
			HandleHostVisibility(makePublic: true, "Host");
			break;
		case 13:
			if (!flag)
			{
				return true;
			}
			HandleHostVisibility(makePublic: false, "Host");
			break;
		case 14:
			if (!flag)
			{
				return true;
			}
			HandleRainbow(array);
			break;
		case 15:
			HandleIds();
			break;
		case 6:
			if (!flag)
			{
				return true;
			}
			HandleKick(array);
			break;
		case 7:
			if (!flag)
			{
				return true;
			}
			HandleBan(array);
			break;
		case 8:
			HandleDm(array);
			break;
		default:
			return true;
		}
		((AbstractChatInputField)__instance.freeChatField).Clear();
		__instance.timeSinceLastMessage = num;
		return false;
	}

	private static bool TryParseColor(string arg, out byte colorId, out string colorName)
	{
		colorId = 0;
		colorName = null;
		if (string.IsNullOrWhiteSpace(arg))
		{
			return false;
		}
		string text = arg.Trim();
		if (byte.TryParse(text, out var result))
		{
			if (result >= _colorNames.Length)
			{
				return false;
			}
			colorId = result;
			colorName = _colorNames[result];
			return true;
		}
		if (_colorMap.TryGetValue(text, out var value))
		{
			colorId = value;
			colorName = _colorNames[value];
			return true;
		}
		return false;
	}

	private static ClientData FindClientByColor(byte colorId)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return null;
		}
		try
		{
			Enumerator<ClientData> enumerator = ((InnerNetClient)AmongUsClient.Instance).allClients.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ClientData current = enumerator.Current;
				if (current == null || (Object)(object)current.Character == (Object)null || (Object)(object)current.Character.Data == (Object)null)
				{
					continue;
				}
				try
				{
					PlayerOutfit defaultOutfit = current.Character.Data.DefaultOutfit;
					if (defaultOutfit != null && defaultOutfit.ColorId == colorId)
					{
						return current;
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
		return null;
	}

	private static string ColorListHint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<size=80%><color=#B0B5BA>Colors: ");
		for (int i = 0; i < _colorNames.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(_colorNames[i].ToLowerInvariant());
		}
		stringBuilder.Append("</color></size>");
		return stringBuilder.ToString();
	}

	private static bool IsColorKeyword(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			return string.Equals(s, "color", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static void HandleColor(string[] args, PlayerControl target)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
			return;
		}
		if ((Object)(object)target == (Object)null || (Object)(object)target.Data == (Object)null)
		{
			Local("<color=#ff5555>Player not ready.</color>");
			return;
		}
		string value = args[0];
		bool flag = (Object)(object)target == (Object)(object)PlayerControl.LocalPlayer;
		if (args.Length < 2)
		{
			if (flag)
			{
				Local($"<b><color=#00ffff>{value} usage:</color></b> {value} [name|id]");
				Local($"<color=#B0B5BA>Examples: {value} red | {value} 5 | {value} list</color>");
			}
			return;
		}
		string text = args[1].Trim().ToLowerInvariant();
		switch (text)
		{
		case "list":
		case "help":
		case "?":
			if (flag)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("<b><color=#00ffff>Available colors:</color></b>");
				for (int i = 0; i < _colorNames.Length; i++)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(24, 2, stringBuilder2);
					handler.AppendLiteral("<color=#B0B5BA>");
					handler.AppendFormatted(i, "00");
					handler.AppendLiteral("</color> ");
					handler.AppendFormatted(_colorNames[i]);
					stringBuilder2.Append(ref handler);
					stringBuilder.Append((i % 3 == 2) ? "\n" : "   ");
				}
				Local(stringBuilder.ToString());
			}
			return;
		}
		byte b;
		if (byte.TryParse(text, out var result))
		{
			if (result >= _colorNames.Length)
			{
				if (flag)
				{
					Local($"<color=#ff5555>Color id must be 0-{_colorNames.Length - 1}. Try {value} list.</color>");
				}
				return;
			}
			b = result;
		}
		else
		{
			if (!_colorMap.TryGetValue(text, out var value2))
			{
				if (flag)
				{
					Local($"<color=#ff5555>Unknown color '{args[1]}'. Try {value} list.</color>");
				}
				return;
			}
			b = value2;
		}
		try
		{
			target.RpcSetColor(b);
			string value3 = target.Data.PlayerName ?? "Player";
			if (flag)
			{
				Local("<color=#00ff88>Color changed to <b>" + _colorNames[b] + "</b>.</color>");
				return;
			}
			Local($"<color=#00ff88><b>{value3}</b> changed color to <b>{_colorNames[b]}</b>.</color>");
		}
		catch (Exception value4)
		{
			if (flag)
			{
				Local("<color=#ff5555>Failed to change color.</color>");
			}
			Debug.LogError(Object.op_Implicit($"[CrewMod] color error: {value4}"));
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "HandleRpc")]
	[HarmonyPrefix]
	public static void Prefix_HandleRpc(PlayerControl __instance, byte callId, MessageReader reader)
	{
		try
		{
			ServerData.SecurityConfig config = ServerData.Config;
			if (config == null || config.RpcSendChatId == 0 || callId != config.RpcSendChatId || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost || (Object)(object)__instance == (Object)null || reader == null || ((Object)(object)PlayerControl.LocalPlayer != (Object)null && (Object)(object)__instance == (Object)(object)PlayerControl.LocalPlayer))
			{
				return;
			}
			int position = reader.Position;
			string text = null;
			try
			{
				text = reader.ReadString();
			}
			catch
			{
			}
			try
			{
				reader.Position = position;
			}
			catch
			{
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}
			string text2 = text.Trim();
			if (!text2.StartsWith("/"))
			{
				return;
			}
			Dictionary<string, int> dispatchMap = GetDispatchMap();
			if (dispatchMap == null)
			{
				return;
			}
			string[] array = text2.Split(' ');
			if (!dispatchMap.TryGetValue(array[0], out var value) || (value != 4 && value != 6 && value != 7 && value != 2 && value != 10 && value != 11 && value != 12 && value != 13))
			{
				return;
			}
			string authoritativeFriendCode = GetAuthoritativeFriendCode(__instance);
			string text3 = "Staff";
			try
			{
				if ((Object)(object)__instance.Data != (Object)null)
				{
					text3 = __instance.Data.PlayerName;
				}
			}
			catch
			{
			}
			if (string.IsNullOrEmpty(authoritativeFriendCode))
			{
				return;
			}
			bool flag = CrewModeratorService.IsModerator(authoritativeFriendCode);
			bool flag2 = !flag && CrewVipService.IsVip(authoritativeFriendCode);
			if ((!flag && !flag2) || (value != 2 && !flag) || ((value == 10 || value == 11) && !flag) || ((value == 12 || value == 13) && !flag))
			{
				return;
			}
			switch (value)
			{
			case 4:
				if (!((Object)(object)LobbyBehaviour.Instance == (Object)null) && !((Object)(object)ShipStatus.Instance != (Object)null))
				{
					try
					{
						NotifyUtils.Info(text3 + " started the match via chat");
					}
					catch
					{
					}
					try
					{
						Local("<color=#00ff88><b>" + text3 + "</b> started the match via chat.</color>");
					}
					catch
					{
					}
					TriggerCountdownStart();
				}
				break;
			case 10:
				if (!IsRoundActive())
				{
					try
					{
						NotifyUtils.Info(text3 + " removed the lobby map");
					}
					catch
					{
					}
					try
					{
						Local("<color=#00ff88><b>" + text3 + "</b> removed the lobby map via chat.</color>");
					}
					catch
					{
					}
					ExecuteHostDestroy();
				}
				break;
			case 11:
				if (IsRoundActive())
				{
					break;
				}
				switch ((array.Length >= 2) ? array[1].Trim().ToLowerInvariant() : "")
				{
				case "":
				case "lobby":
				case "lb":
					try
					{
						NotifyUtils.Info(text3 + " restored the lobby");
					}
					catch
					{
					}
					try
					{
						Local("<color=#00ff88><b>" + text3 + "</b> restored the lobby via chat.</color>");
					}
					catch
					{
					}
					ExecuteHostRestoreLobby();
					break;
				default:
				{
					if (TryResolveMapArg(array[1], out var spawnId, out var label))
					{
						try
						{
							NotifyUtils.Info(text3 + " spawned " + label);
						}
						catch
						{
						}
						try
						{
							Local($"<color=#00ff88><b>{text3}</b> spawned <b>{label}</b> via chat.</color>");
						}
						catch
						{
						}
						ExecuteHostSpawn(spawnId);
					}
					break;
				}
				}
				break;
			case 6:
				if (array.Length >= 2)
				{
					if (array.Length >= 3 && IsColorKeyword(array[1]))
					{
						ExecuteKickBanByColor(string.Join(" ", array, 2, array.Length - 2).Trim(), ban: false, text3);
					}
					else
					{
						ExecuteKickBan(string.Join(" ", array, 1, array.Length - 1).Trim(), ban: false, text3);
					}
				}
				break;
			case 7:
				if (array.Length >= 2)
				{
					if (array.Length >= 3 && IsColorKeyword(array[1]))
					{
						ExecuteKickBanByColor(string.Join(" ", array, 2, array.Length - 2).Trim(), ban: true, text3);
					}
					else
					{
						ExecuteKickBan(string.Join(" ", array, 1, array.Length - 1).Trim(), ban: true, text3);
					}
				}
				break;
			case 2:
				HandleColor(array, __instance);
				break;
			case 12:
				HandleHostVisibility(makePublic: true, text3);
				break;
			case 13:
				HandleHostVisibility(makePublic: false, text3);
				break;
			case 3:
			case 5:
			case 8:
			case 9:
				break;
			}
		}
		catch (Exception value2)
		{
			Debug.LogError(Object.op_Implicit($"[CrewMod] HandleRpc dispatcher error: {value2}"));
		}
	}

	private static void HandleHostVisibility(bool makePublic, string actorName)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
			return;
		}
		if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Local("<color=#ff5555>Host-only.</color>");
			return;
		}
		if ((Object)(object)LobbyBehaviour.Instance == (Object)null || (Object)(object)ShipStatus.Instance != (Object)null)
		{
			Local("<color=#ff5555>Lobby-only.</color>");
			return;
		}
		try
		{
			if (((InnerNetClient)AmongUsClient.Instance).IsGamePublic == makePublic)
			{
				Local("<color=#ffa500>Lobby is already <b>" + (makePublic ? "public" : "private") + "</b>.</color>");
				return;
			}
			((InnerNetClient)AmongUsClient.Instance).ChangeGamePublic(makePublic);
			string text = (makePublic ? "public" : "private");
			try
			{
				NotifyUtils.Info(actorName + " set lobby " + text);
			}
			catch
			{
			}
			Local($"<color=#00ff88><b>{actorName}</b> set lobby <b>{text}</b>.</color>");
		}
		catch (Exception value)
		{
			Local("<color=#ff5555>Failed to change lobby visibility.</color>");
			Debug.LogError(Object.op_Implicit($"[CrewMod] visibility error: {value}"));
		}
	}

	private static void HandleHostStart()
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
		}
		else if ((Object)(object)ShipStatus.Instance != (Object)null)
		{
			Local("<color=#ffa500>Already in game.</color>");
		}
		else
		{
			TriggerCountdownStart();
		}
	}

	private static PlayerControl FindPlayerById(byte pid)
	{
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && current.PlayerId == pid)
			{
				return current;
			}
		}
		return null;
	}

	private static void HandleBanList()
	{
		List<KeyValuePair<string, string>> list = CrewBanListService.ListDetailed();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("<color=#ff5555><b>━━━ Persistent Ban List ━━━</b></color>");
		if (list.Count == 0)
		{
			stringBuilder.AppendLine("<color=#B0B5BA>No banned players.</color>");
		}
		else
		{
			StringBuilder stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler;
			foreach (KeyValuePair<string, string> item in list)
			{
				string value = (string.IsNullOrEmpty(item.Value) ? "?" : item.Value);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(56, 2, stringBuilder2);
				handler.AppendLiteral("<color=#FFD86A><b>");
				handler.AppendFormatted(item.Key);
				handler.AppendLiteral("</b></color> <color=#8a8f99>—</color> ");
				handler.AppendFormatted(value);
				stringBuilder3.AppendLine(ref handler);
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(95, 1, stringBuilder2);
			handler.AppendLiteral("<size=80%><color=#B0B5BA>");
			handler.AppendFormatted(list.Count);
			handler.AppendLiteral(" banned. Use <b>/unban [friendCode|name]</b> to remove.</color></size>");
			stringBuilder4.AppendLine(ref handler);
		}
		Local(stringBuilder.ToString());
	}

	private static void HandleUnban(string[] args)
	{
		if (args.Length < 2)
		{
			Local("<color=#ff5555>Usage: <b>/unban</b> <color=#FFD86A>[friendCode|name]</color></color>");
			return;
		}
		string text = string.Join(" ", args, 1, args.Length - 1).Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			Local("<color=#ff5555>Missing argument.</color>");
			return;
		}
		if (text.Contains("#") && CrewBanListService.Remove(text))
		{
			Local("<color=#00ff88>Removed <b>" + text + "</b> from banlist.</color>");
			return;
		}
		int num = CrewBanListService.RemoveByName(text);
		if (num > 0)
		{
			Local($"<color=#00ff88>Removed <b>{num}</b> entry/entries matching <b>{text}</b>.</color>");
		}
		else if (CrewBanListService.Remove(text))
		{
			Local("<color=#00ff88>Removed <b>" + text + "</b> from banlist.</color>");
		}
		else
		{
			Local("<color=#ffa500>No banlist entry matched <b>" + text + "</b>.</color>");
		}
	}

	private static void HandleIds()
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("<color=#00ffff><b>━━━ Player IDs ━━━</b></color>");
		List<PlayerControl> list = new List<PlayerControl>();
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)((current != null) ? current.Data : null) != (Object)null && !current.Data.Disconnected)
			{
				list.Add(current);
			}
		}
		list.Sort((PlayerControl a, PlayerControl b) => a.PlayerId.CompareTo(b.PlayerId));
		if (list.Count == 0)
		{
			stringBuilder.AppendLine("<color=#ffa500>No players found.</color>");
		}
		else
		{
			StringBuilder stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler;
			foreach (PlayerControl item in list)
			{
				string value = item.Data.PlayerName ?? "?";
				bool num = GameCheats.IsPlayerRainbowActive(item.PlayerId);
				string value2 = (((Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetObject)item).OwnerId == ((InnerNetClient)AmongUsClient.Instance).HostId) ? " <size=80%><color=#5865F2>[host]</color></size>" : "");
				string value3 = (num ? " <size=80%><color=#ff88ff>[rainbow]</color></size>" : "");
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(64, 4, stringBuilder2);
				handler.AppendLiteral("<b><color=#FFD86A>#");
				handler.AppendFormatted(item.PlayerId);
				handler.AppendLiteral("</color></b> <color=#8a8f99>—</color> <b>");
				handler.AppendFormatted(value);
				handler.AppendLiteral("</b>");
				handler.AppendFormatted(value2);
				handler.AppendFormatted(value3);
				stringBuilder3.AppendLine(ref handler);
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(111, 1, stringBuilder2);
			handler.AppendLiteral("<size=80%><color=#B0B5BA>");
			handler.AppendFormatted(list.Count);
			handler.AppendLiteral(" player(s). Use the id with <b>/rainbow &lt;id&gt;</b> to rainbow them.</color></size>");
			stringBuilder4.AppendLine(ref handler);
		}
		Local(stringBuilder.ToString());
	}

	private static void HandleRainbow(string[] args)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
			return;
		}
		if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Local("<color=#ff5555>Host-only.</color>");
			return;
		}
		string text = args[0];
		if (args.Length < 2)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(38, 1, stringBuilder2);
			handler.AppendLiteral("<color=#ff88ff><b>━━━ ");
			handler.AppendFormatted(text);
			handler.AppendLiteral(" ━━━</b></color>");
			stringBuilder3.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(89, 1, stringBuilder2);
			handler.AppendLiteral("<b>");
			handler.AppendFormatted(text);
			handler.AppendLiteral("</b> <color=#FFD86A>[id]</color> <color=#8a8f99>—</color> start rainbow on that player");
			stringBuilder4.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(93, 1, stringBuilder2);
			handler.AppendLiteral("<b>");
			handler.AppendFormatted(text);
			handler.AppendLiteral(" stop</b> <color=#FFD86A>[id]</color> <color=#8a8f99>—</color> stop rainbow on that player");
			stringBuilder5.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(70, 1, stringBuilder2);
			handler.AppendLiteral("<b>");
			handler.AppendFormatted(text);
			handler.AppendLiteral(" stop all</b> <color=#8a8f99>—</color> stop rainbow on every player");
			stringBuilder6.AppendLine(ref handler);
			List<string> list = new List<string>();
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if (!((Object)(object)((current != null) ? current.Data : null) == (Object)null) && !current.Data.Disconnected)
				{
					string value = (GameCheats.IsPlayerRainbowActive(current.PlayerId) ? "<color=#ff88ff>●</color> " : "");
					list.Add($"{value}<color=#FFD86A><b>{current.PlayerId}</b></color>={current.Data.PlayerName}");
				}
			}
			if (list.Count > 0)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder7 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(49, 1, stringBuilder2);
				handler.AppendLiteral("<size=80%><color=#B0B5BA>Players: ");
				handler.AppendFormatted(string.Join(" <color=#5a5f6a>|</color> ", list));
				handler.AppendLiteral("</color></size>");
				stringBuilder7.AppendLine(ref handler);
			}
			Local(stringBuilder.ToString());
			return;
		}
		string text2 = args[1].Trim().ToLowerInvariant();
		byte result2;
		if (text2 == "stop")
		{
			if (args.Length < 3)
			{
				Local("<color=#ff5555>Usage:</color> <b>" + text + " stop</b> <color=#FFD86A>[id|all]</color>");
				return;
			}
			string text3 = args[2].Trim().ToLowerInvariant();
			if (text3 == "all")
			{
				int num = GameCheats.StopAllPlayerRainbows();
				if (num > 0)
				{
					Local($"<color=#00ff88>Stopped rainbow on <b>{num}</b> player(s).</color>");
				}
				else
				{
					Local("<color=#ffa500>No players were in rainbow.</color>");
				}
				return;
			}
			if (!byte.TryParse(text3, out var result))
			{
				Local("<color=#ff5555>Invalid id '<b>" + args[2] + "</b>'. Use <b>/ids</b> to list.</color>");
				return;
			}
			PlayerControl obj = FindPlayerById(result);
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				NetworkedPlayerInfo data = obj.Data;
				obj2 = ((data != null) ? data.PlayerName : null);
			}
			if (obj2 == null)
			{
				obj2 = $"id {result}";
			}
			string text4 = (string)obj2;
			if (GameCheats.StopPlayerRainbow(result))
			{
				Local("<color=#00ff88>Stopped rainbow on <b>" + text4 + "</b>.</color>");
			}
			else
			{
				Local("<color=#ffa500><b>" + text4 + "</b> is not in rainbow.</color>");
			}
		}
		else if (!byte.TryParse(text2, out result2))
		{
			Local("<color=#ff5555>Invalid id '<b>" + args[1] + "</b>'. Use <b>/ids</b> to list players.</color>");
		}
		else
		{
			PlayerControl val = FindPlayerById(result2);
			if ((Object)(object)val == (Object)null || (Object)(object)val.Data == (Object)null || val.Data.Disconnected)
			{
				Local($"<color=#ff5555>No player with id <b>{result2}</b>. Use <b>/ids</b> to list.</color>");
			}
			else if (GameCheats.IsPlayerRainbowActive(result2))
			{
				Local($"<color=#ffa500><b>{val.Data.PlayerName}</b> is already in rainbow. Use <b>{text} stop {result2}</b> to stop.</color>");
			}
			else if (GameCheats.StartPlayerRainbow(result2))
			{
				Local($"<color=#ff88ff>●</color> <color=#00ff88>Rainbow started on <b>{val.Data.PlayerName}</b> <size=80%><color=#B0B5BA>(id {result2})</color></size>.</color>");
			}
			else
			{
				Local("<color=#ff5555>Failed to start rainbow.</color>");
			}
		}
	}

	private static bool IsRoundActive()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		if ((Object)(object)AmongUsClient.Instance != (Object)null)
		{
			return (int)((InnerNetClient)AmongUsClient.Instance).GameState == 2;
		}
		return false;
	}

	private static void HandleHostRemove()
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
			return;
		}
		if (IsRoundActive())
		{
			Local("<color=#ffa500>Cannot remove during an active round.</color>");
			return;
		}
		ExecuteHostDestroy();
		try
		{
			Local("<color=#00ff88>Lobby map removed.</color>");
		}
		catch
		{
		}
	}

	private static void HandleHostRestore(string[] args)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
			return;
		}
		if (IsRoundActive())
		{
			Local("<color=#ffa500>Cannot restore during an active round.</color>");
			return;
		}
		if (args == null || args.Length < 2)
		{
			ExecuteHostRestoreLobby();
			try
			{
				Local("<color=#00ff88>Lobby restored.</color>");
				return;
			}
			catch
			{
				return;
			}
		}
		string text = args[1].Trim().ToLowerInvariant();
		if (text == "lobby" || text == "lb")
		{
			ExecuteHostRestoreLobby();
			try
			{
				Local("<color=#00ff88>Lobby restored.</color>");
				return;
			}
			catch
			{
				return;
			}
		}
		if (!TryResolveMapArg(args[1], out var spawnId, out var label))
		{
			Local("<color=#ff5555>Unknown map '" + args[1] + "'. Try skeld, mira, polus, airship, lobby.</color>");
			return;
		}
		ExecuteHostSpawn(spawnId);
		try
		{
			Local("<color=#00ff88>Spawned <b>" + label + "</b>.</color>");
		}
		catch
		{
		}
	}

	private static bool TryResolveMapArg(string arg, out uint spawnId, out string label)
	{
		spawnId = 0u;
		label = "";
		if (string.IsNullOrWhiteSpace(arg))
		{
			return false;
		}
		ServerData.SecurityConfig config = ServerData.Config;
		if (config == null)
		{
			return false;
		}
		switch (arg.Trim().ToLowerInvariant())
		{
		case "sk":
		case "skeld":
		case "theskeld":
			spawnId = config.M0;
			label = "The Skeld";
			return true;
		case "mr":
		case "mira_hq":
		case "mira":
		case "mirahq":
			spawnId = config.M1;
			label = "MIRA HQ";
			return true;
		case "pl":
		case "polus":
			spawnId = config.M2;
			label = "Polus";
			return true;
		case "as":
		case "airship":
			spawnId = config.M3;
			label = "Airship";
			return true;
		default:
			return false;
		}
	}

	private static void ExecuteHostDestroy()
	{
		try
		{
			GameCheats.MapCheats.FullTeardown();
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[CrewMod] destroy error: {value}"));
		}
	}

	private static void ExecuteHostSpawn(uint spawnId)
	{
		try
		{
			GameCheats.SelectMapBySpawnId(spawnId, 1);
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[CrewMod] spawn error: {value}"));
		}
	}

	private static void ExecuteHostRestoreLobby()
	{
		try
		{
			GameCheats.ClearSelMap();
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[CrewMod] restore lobby error: {value}"));
		}
	}

	internal static void TriggerCountdownStart()
	{
		try
		{
			if (DestroyableSingleton<GameStartManager>.InstanceExists)
			{
				GameStartManager instance = DestroyableSingleton<GameStartManager>.Instance;
				if (!((Object)(object)instance == (Object)null))
				{
					instance.BeginGame();
				}
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[CrewMod] TriggerCountdownStart error: {value}"));
		}
	}

	private static void HandleMod(string[] args)
	{
		ServerData.SecurityConfig config = ServerData.Config;
		if (config == null)
		{
			return;
		}
		string text = args[0];
		string text2 = config.CmdModAdd ?? "";
		string text3 = config.CmdModRemove ?? "";
		string text4 = config.CmdModList ?? "";
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
			return;
		}
		if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Local("<color=#ff5555>" + text + " is host-only.</color>");
			return;
		}
		if (args.Length < 2)
		{
			Local($"Usage: <b>{text} {text2} [friendCode]</b> | <b>{text} {text3} [friendCode]</b> | <b>{text} {text4}</b>");
			return;
		}
		string a = args[1];
		if (!string.IsNullOrEmpty(text4) && string.Equals(a, text4, StringComparison.OrdinalIgnoreCase))
		{
			List<string> list = CrewModeratorService.List();
			if (list.Count == 0)
			{
				Local("<color=#ffa500>No moderators.</color>");
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(44, 1, stringBuilder2);
			handler.AppendLiteral("<b><color=#00ffff>Moderators (");
			handler.AppendFormatted(list.Count);
			handler.AppendLiteral("):</color></b>");
			stringBuilder2.AppendLine(ref handler);
			foreach (string item in list)
			{
				stringBuilder.AppendLine("• " + item);
			}
			Local(stringBuilder.ToString());
		}
		else if (!string.IsNullOrEmpty(text2) && string.Equals(a, text2, StringComparison.OrdinalIgnoreCase))
		{
			if (args.Length < 3)
			{
				Local($"Usage: {text} {text2} [playerName|friendCode] | {text} {text2} color [colorName|id]");
			}
			else if (args.Length >= 4 && IsColorKeyword(args[2]))
			{
				StaffAddByColor(string.Join(" ", args, 3, args.Length - 3).Trim(), isVip: false);
			}
			else
			{
				string text5 = string.Join(" ", args, 2, args.Length - 2).Trim();
				string resolvedName;
				string text6 = ResolveFriendCode(text5, out resolvedName);
				if (string.IsNullOrEmpty(text6))
				{
					Local("<color=#ff5555>Could not find online player '<b>" + text5 + "</b>'. Use the exact in-lobby name, or pass a full FriendCode (contains '#').</color>");
				}
				else if (CrewModeratorService.Add(text6, resolvedName))
				{
					string text7 = ((!string.IsNullOrEmpty(resolvedName)) ? (resolvedName + " <size=80%><color=#B0B5BA>(" + text6 + ")</color></size>") : text6);
					Local("<color=#00ff88>Added <b>" + text7 + "</b> as moderator.</color>");
				}
				else
				{
					Local("<color=#ffa500>" + (resolvedName ?? text6) + " is already a moderator.</color>");
				}
			}
		}
		else if (!string.IsNullOrEmpty(text3) && string.Equals(a, text3, StringComparison.OrdinalIgnoreCase))
		{
			if (args.Length < 3)
			{
				Local($"Usage: {text} {text3} [playerName|friendCode] | {text} {text3} color [colorName|id]");
			}
			else if (args.Length >= 4 && IsColorKeyword(args[2]))
			{
				StaffRemoveByColor(string.Join(" ", args, 3, args.Length - 3).Trim(), isVip: false);
			}
			else
			{
				string text8 = string.Join(" ", args, 2, args.Length - 2).Trim();
				string resolvedName2;
				string text9 = ResolveFriendCode(text8, out resolvedName2);
				if (string.IsNullOrEmpty(text9))
				{
					text9 = text8;
				}
				if (CrewModeratorService.Remove(text9))
				{
					Local("<color=#00ff88>Removed <b>" + (resolvedName2 ?? text9) + "</b>.</color>");
				}
				else
				{
					Local("<color=#ffa500>" + (resolvedName2 ?? text9) + " is not a moderator.</color>");
				}
			}
		}
		else
		{
			Local($"Usage: <b>{text} {text2}</b> | <b>{text} {text3}</b> | <b>{text} {text4}</b>");
		}
	}

	private static void HandleVip(string[] args)
	{
		ServerData.SecurityConfig config = ServerData.Config;
		if (config == null)
		{
			return;
		}
		string text = args[0];
		string text2 = config.CmdVipAdd ?? "add";
		string text3 = config.CmdVipRemove ?? "remove";
		string text4 = config.CmdVipList ?? "list";
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
			return;
		}
		if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Local("<color=#ff5555>" + text + " is host-only.</color>");
			return;
		}
		if (args.Length < 2)
		{
			Local($"Usage: <b>{text} {text2} [playerName|friendCode]</b> | <b>{text} {text3} [playerName|friendCode]</b> | <b>{text} {text4}</b>");
			Local("<color=#B0B5BA>VIPs can use <b>/color</b> only.</color>");
			return;
		}
		string a = args[1];
		if (!string.IsNullOrEmpty(text4) && string.Equals(a, text4, StringComparison.OrdinalIgnoreCase))
		{
			List<KeyValuePair<string, string>> list = CrewVipService.ListDetailed();
			if (list.Count == 0)
			{
				Local("<color=#ffa500>No VIPs.</color>");
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(38, 1, stringBuilder2);
			handler.AppendLiteral("<b><color=#FFD86A>VIPs (");
			handler.AppendFormatted(list.Count);
			handler.AppendLiteral("):</color></b>");
			stringBuilder2.AppendLine(ref handler);
			foreach (KeyValuePair<string, string> item in list)
			{
				string text5 = ((!string.IsNullOrEmpty(item.Value)) ? (item.Value + " <size=80%><color=#B0B5BA>(" + item.Key + ")</color></size>") : item.Key);
				stringBuilder.AppendLine("• " + text5);
			}
			Local(stringBuilder.ToString());
		}
		else if (!string.IsNullOrEmpty(text2) && string.Equals(a, text2, StringComparison.OrdinalIgnoreCase))
		{
			if (args.Length < 3)
			{
				Local($"Usage: {text} {text2} [playerName|friendCode] | {text} {text2} color [colorName|id]");
			}
			else if (args.Length >= 4 && IsColorKeyword(args[2]))
			{
				StaffAddByColor(string.Join(" ", args, 3, args.Length - 3).Trim(), isVip: true);
			}
			else
			{
				string text6 = string.Join(" ", args, 2, args.Length - 2).Trim();
				string resolvedName;
				string text7 = ResolveFriendCode(text6, out resolvedName);
				if (string.IsNullOrEmpty(text7))
				{
					Local("<color=#ff5555>Could not find online player '<b>" + text6 + "</b>'. Use exact in-lobby name or FriendCode (contains '#').</color>");
				}
				else if (CrewModeratorService.IsModerator(text7))
				{
					Local("<color=#ffa500>" + (resolvedName ?? text7) + " is already a moderator (has full staff access).</color>");
				}
				else if (CrewVipService.Add(text7, resolvedName))
				{
					string text8 = ((!string.IsNullOrEmpty(resolvedName)) ? (resolvedName + " <size=80%><color=#B0B5BA>(" + text7 + ")</color></size>") : text7);
					Local("<color=#FFD86A>Added <b>" + text8 + "</b> as VIP.</color> <size=80%><color=#B0B5BA>(can use /color only)</color></size>");
				}
				else
				{
					Local("<color=#ffa500>" + (resolvedName ?? text7) + " is already a VIP.</color>");
				}
			}
		}
		else if (!string.IsNullOrEmpty(text3) && string.Equals(a, text3, StringComparison.OrdinalIgnoreCase))
		{
			if (args.Length < 3)
			{
				Local($"Usage: {text} {text3} [playerName|friendCode] | {text} {text3} color [colorName|id]");
			}
			else if (args.Length >= 4 && IsColorKeyword(args[2]))
			{
				StaffRemoveByColor(string.Join(" ", args, 3, args.Length - 3).Trim(), isVip: true);
			}
			else
			{
				string text9 = string.Join(" ", args, 2, args.Length - 2).Trim();
				string resolvedName2;
				string text10 = ResolveFriendCode(text9, out resolvedName2);
				if (string.IsNullOrEmpty(text10))
				{
					text10 = text9;
				}
				if (CrewVipService.Remove(text10))
				{
					Local("<color=#00ff88>Removed VIP <b>" + (resolvedName2 ?? text10) + "</b>.</color>");
				}
				else
				{
					Local("<color=#ffa500>" + (resolvedName2 ?? text10) + " is not a VIP.</color>");
				}
			}
		}
		else
		{
			Local($"Usage: <b>{text} {text2}</b> | <b>{text} {text3}</b> | <b>{text} {text4}</b>");
		}
	}

	private static void HandleBannedWord(string[] args)
	{
		if (args.Length < 2)
		{
			Local("<color=#FFD86A>Usage:</color> <b>/bw add [word]</b> | <b>/bw remove [word]</b> | <b>/bw clear</b> | <b>/bw list</b>");
			return;
		}
		string text = args[1].ToLowerInvariant();
		ConfigEntry<string> autoKickChatWordList = CheatConfig.AutoKickChatWordList;
		if (autoKickChatWordList == null)
		{
			Local("<color=#ff5555>Config not initialized.</color>");
			return;
		}
		List<string> list = new List<string>();
		string[] array = (autoKickChatWordList.Value ?? "").Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string text2 = array[i].Trim();
			if (!string.IsNullOrEmpty(text2))
			{
				list.Add(text2);
			}
		}
		if (text == "list")
		{
			if (list.Count == 0)
			{
				Local("<color=#B0B5BA>Banned word list is empty.</color>");
			}
			else
			{
				Local("<b>Banned words:</b> " + string.Join(", ", list));
			}
			return;
		}
		if (text == "clear")
		{
			autoKickChatWordList.Value = "";
			CheatConfig.Save();
			try
			{
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
			catch
			{
			}
			Local("<color=#4ade80>Banned word list cleared.</color>");
			return;
		}
		if (args.Length < 3)
		{
			Local("<color=#FFD86A>Usage:</color> <b>/bw " + text + " [word]</b>");
			return;
		}
		string word = string.Join(" ", args, 2, args.Length - 2).Trim();
		if (string.IsNullOrEmpty(word))
		{
			return;
		}
		switch (text)
		{
		case "add":
			if ((word.StartsWith("*") ? word.Substring(1) : word).Length > 32)
			{
				Local("<color=#ff5555>Word too long (max 32 chars).</color>");
				break;
			}
			if (list.Exists((string x) => string.Equals(x, word, StringComparison.OrdinalIgnoreCase)))
			{
				Local("<color=#B0B5BA>'" + word + "' already blacklisted.</color>");
				break;
			}
			list.Add(word);
			autoKickChatWordList.Value = string.Join(",", list);
			CheatConfig.Save();
			try
			{
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
			catch
			{
			}
			Local("<color=#4ade80>Added banned word:</color> <b>" + word + "</b>");
			break;
		case "remove":
		case "rm":
			if (list.RemoveAll((string x) => string.Equals(x, word, StringComparison.OrdinalIgnoreCase)) == 0)
			{
				Local("<color=#B0B5BA>'" + word + "' not in list.</color>");
				break;
			}
			autoKickChatWordList.Value = string.Join(",", list);
			CheatConfig.Save();
			try
			{
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
			catch
			{
			}
			Local("<color=#4ade80>Removed banned word:</color> <b>" + word + "</b>");
			break;
		default:
			Local("<color=#FFD86A>Unknown subcommand. Use:</color> add | remove | clear | list");
			break;
		}
	}

	private static void HandleKick(string[] args)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
		}
		else if (args.Length < 2)
		{
			Local($"Usage: <b>{args[0]} [playerName|id]</b> | <b>{args[0]} color [colorName|id]</b>");
		}
		else if (args.Length >= 3 && IsColorKeyword(args[1]))
		{
			ExecuteKickBanByColor(string.Join(" ", args, 2, args.Length - 2).Trim(), ban: false, "You");
		}
		else
		{
			ExecuteKickBan(string.Join(" ", args, 1, args.Length - 1).Trim(), ban: false, "You");
		}
	}

	private static void HandleBan(string[] args)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
		}
		else if (args.Length < 2)
		{
			Local($"Usage: <b>{args[0]} [playerName|id]</b> | <b>{args[0]} color [colorName|id]</b>");
		}
		else if (args.Length >= 3 && IsColorKeyword(args[1]))
		{
			ExecuteKickBanByColor(string.Join(" ", args, 2, args.Length - 2).Trim(), ban: true, "You");
		}
		else
		{
			ExecuteKickBan(string.Join(" ", args, 1, args.Length - 1).Trim(), ban: true, "You");
		}
	}

	private static void HandleDm(string[] args)
	{
		ServerData.SecurityConfig config = ServerData.Config;
		if (config == null)
		{
			return;
		}
		string text = args[0];
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff5555>Not connected.</color>");
			return;
		}
		if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Local("<color=#ff5555>" + text + " is host-only (the Among Us server only routes targeted messages from the host).</color>");
			return;
		}
		if (args.Length < 3)
		{
			Local($"<b><color=#a866ff>{text} usage:</color></b> {text} [playerName|id] [message]");
			Local("<color=#B0B5BA>Only the target player will see the message.</color>");
			return;
		}
		string text2 = args[1].Trim();
		string text3 = string.Join(" ", args, 2, args.Length - 2).Trim();
		if (string.IsNullOrWhiteSpace(text3))
		{
			Local("<color=#ff5555>Empty message.</color>");
			return;
		}
		int num = ((config.DmMaxChars > 0) ? config.DmMaxChars : 500);
		if (text3.Length > num)
		{
			text3 = text3.Substring(0, num);
		}
		ClientData val = ResolveClient(text2);
		if (val == null)
		{
			Local("<color=#ff5555>Could not find player '<b>" + text2 + "</b>'.</color>");
			return;
		}
		if (val.Id == ((InnerNetClient)AmongUsClient.Instance).ClientId)
		{
			Local("<color=#ffa500>Cannot DM yourself.</color>");
			return;
		}
		if ((Object)(object)val.Character == (Object)null)
		{
			Local("<color=#ff5555>" + val.PlayerName + " isn't ready to receive messages yet.</color>");
			return;
		}
		try
		{
			SendPrivateMessage(val.Id, text3);
			Local("<color=#a866ff>[DM → <b>" + val.PlayerName + "</b>]</color> " + text3);
		}
		catch (Exception value)
		{
			Local("<color=#ff5555>Failed to send DM.</color>");
			Debug.LogError(Object.op_Implicit($"[CrewMod] dm error: {value}"));
		}
	}

	private static void SendPrivateMessage(int targetClientId, string message)
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null || (Object)(object)localPlayer.Data == (Object)null)
		{
			return;
		}
		AmongUsClient instance = AmongUsClient.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		ServerData.SecurityConfig config = ServerData.Config;
		if (config != null)
		{
			byte rpcSetNameId = config.RpcSetNameId;
			byte rpcSendChatId = config.RpcSendChatId;
			if (rpcSetNameId != 0 && rpcSendChatId != 0 && !string.IsNullOrEmpty(config.DmTitleTemplate))
			{
				string text = localPlayer.Data.PlayerName ?? "";
				string text2 = config.DmTitleTemplate.Replace("{sender}", text);
				uint netId = ((InnerNetObject)localPlayer.Data).NetId;
				MessageWriter val = ((InnerNetClient)instance).StartRpcImmediately(((InnerNetObject)localPlayer).NetId, rpcSetNameId, (SendOption)1, targetClientId);
				val.Write(netId);
				val.Write(text2);
				val.EndMessage();
				val.StartMessage((byte)2);
				val.WritePacked(((InnerNetObject)localPlayer).NetId);
				val.Write(rpcSendChatId);
				val.Write(message);
				val.EndMessage();
				val.StartMessage((byte)2);
				val.WritePacked(((InnerNetObject)localPlayer).NetId);
				val.Write(rpcSetNameId);
				val.Write(netId);
				val.Write(text);
				((InnerNetClient)instance).FinishRpcImmediately(val);
			}
		}
	}

	private static void HandleHelp()
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Local("<color=#ff5555>Host-only.</color>");
			return;
		}
		ServerData.SecurityConfig config = ServerData.Config;
		if (config != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("<color=#00ffff><b>━━━ Chat Commands ━━━</b></color>");
			if (!string.IsNullOrEmpty(config.CmdPing))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(105, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdPing);
				handler.AppendLiteral("</b> <color=#8a8f99>—</color> room status <size=80%><color=#B0B5BA>(ping, FPS, players)</color></size>");
				stringBuilder3.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdHelp))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(100, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdHelp);
				handler.AppendLiteral("</b> <color=#8a8f99>—</color> show this list <size=80%><color=#B0B5BA>(local only)</color></size>");
				stringBuilder4.AppendLine(ref handler);
			}
			stringBuilder.AppendLine("<color=#5865F2><b>── Staff ──</b></color>");
			if (!string.IsNullOrEmpty(config.CmdColor))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder5 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(149, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdColor);
				handler.AppendLiteral("</b> <color=#FFD86A>[name|id|list]</color> <color=#8a8f99>—</color> change your color <size=80%><color=#B0B5BA>(host + mods + vips)</color></size>");
				stringBuilder5.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdStart))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder6 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(102, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdStart);
				handler.AppendLiteral("</b> <color=#8a8f99>—</color> start the match <size=80%><color=#B0B5BA>(host + mods)</color></size>");
				stringBuilder6.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdRestore))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder7 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(196, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdRestore);
				handler.AppendLiteral("</b> <color=#FFD86A>[skeld|mira|polus|airship|lobby]</color> <color=#8a8f99>—</color> restore lobby or spawn a map (no arg = restore lobby) <size=80%><color=#B0B5BA>(host + mods)</color></size>");
				stringBuilder7.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdRemove))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder8 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(107, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdRemove);
				handler.AppendLiteral("</b> <color=#8a8f99>—</color> remove the lobby map <size=80%><color=#B0B5BA>(host + mods)</color></size>");
				stringBuilder8.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdPublic))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder9 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(103, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdPublic);
				handler.AppendLiteral("</b> <color=#8a8f99>—</color> set lobby public <size=80%><color=#B0B5BA>(host + mods)</color></size>");
				stringBuilder9.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdPrivate))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder10 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(104, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdPrivate);
				handler.AppendLiteral("</b> <color=#8a8f99>—</color> set lobby private <size=80%><color=#B0B5BA>(host + mods)</color></size>");
				stringBuilder10.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdIds) || !string.IsNullOrEmpty(config.CmdRainbow))
			{
				stringBuilder.AppendLine("<color=#ff88ff><b>── Rainbow ──</b></color>");
			}
			if (!string.IsNullOrEmpty(config.CmdIds))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder11 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(140, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdIds);
				handler.AppendLiteral("</b> <color=#8a8f99>—</color> list every player with their <b>id</b> and <b>name</b> <size=80%><color=#B0B5BA>(local only)</color></size>");
				stringBuilder11.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdRainbow))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder12 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(145, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdRainbow);
				handler.AppendLiteral("</b> <color=#FFD86A>[id]</color> <color=#8a8f99>—</color> start rainbow on a single player <size=80%><color=#B0B5BA>(host only)</color></size>");
				stringBuilder12.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder13 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(120, 1, stringBuilder2);
				handler.AppendLiteral("  <color=#a0a4ad>↳</color> <b>");
				handler.AppendFormatted(config.CmdRainbow);
				handler.AppendLiteral(" stop</b> <color=#FFD86A>[id]</color> <color=#8a8f99>—</color> stop rainbow on that player");
				stringBuilder13.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder14 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(97, 1, stringBuilder2);
				handler.AppendLiteral("  <color=#a0a4ad>↳</color> <b>");
				handler.AppendFormatted(config.CmdRainbow);
				handler.AppendLiteral(" stop all</b> <color=#8a8f99>—</color> stop rainbow on every player");
				stringBuilder14.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder15 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(80, 2, stringBuilder2);
				handler.AppendLiteral("  <size=80%><color=#B0B5BA>Tip: run <b>");
				handler.AppendFormatted(config.CmdIds);
				handler.AppendLiteral("</b> first, then <b>");
				handler.AppendFormatted(config.CmdRainbow);
				handler.AppendLiteral(" 3</b></color></size>");
				stringBuilder15.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdKick))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder16 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(133, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdKick);
				handler.AppendLiteral("</b> <color=#FFD86A>[name|id]</color> <color=#8a8f99>—</color> kick a player <size=80%><color=#B0B5BA>(host + mods)</color></size>");
				stringBuilder16.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder17 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(125, 1, stringBuilder2);
				handler.AppendLiteral("  <color=#a0a4ad>↳</color> <b>");
				handler.AppendFormatted(config.CmdKick);
				handler.AppendLiteral(" color</b> <color=#FFD86A>[colorName|id]</color> <color=#8a8f99>—</color> kick by in-game color");
				stringBuilder17.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdBan))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder18 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(174, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdBan);
				handler.AppendLiteral("</b> <color=#FFD86A>[name|id]</color> <color=#8a8f99>—</color> ban a player (persistent: FriendCode saved to banlist) <size=80%><color=#B0B5BA>(host + mods)</color></size>");
				stringBuilder18.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder19 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(124, 1, stringBuilder2);
				handler.AppendLiteral("  <color=#a0a4ad>↳</color> <b>");
				handler.AppendFormatted(config.CmdBan);
				handler.AppendLiteral(" color</b> <color=#FFD86A>[colorName|id]</color> <color=#8a8f99>—</color> ban by in-game color");
				stringBuilder19.AppendLine(ref handler);
			}
			stringBuilder.AppendLine("<b>/banlist</b> <color=#8a8f99>—</color> show every saved banned FriendCode <size=80%><color=#B0B5BA>(local only)</color></size>");
			stringBuilder.AppendLine("<b>/unban</b> <color=#FFD86A>[friendCode|name]</color> <color=#8a8f99>—</color> remove from banlist <size=80%><color=#B0B5BA>(host only)</color></size>");
			if (!string.IsNullOrEmpty(config.CmdDm))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder20 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(143, 1, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdDm);
				handler.AppendLiteral("</b> <color=#FFD86A>[name|id] [message]</color> <color=#8a8f99>—</color> private message <size=80%><color=#B0B5BA>(host only)</color></size>");
				stringBuilder20.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdMod))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder21 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(128, 4, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdMod);
				handler.AppendLiteral("</b> <color=#FFD86A>");
				handler.AppendFormatted(config.CmdModAdd);
				handler.AppendLiteral("|");
				handler.AppendFormatted(config.CmdModRemove);
				handler.AppendLiteral("|");
				handler.AppendFormatted(config.CmdModList);
				handler.AppendLiteral("</color> <color=#8a8f99>—</color> manage moderators <size=80%><color=#B0B5BA>(host only)</color></size>");
				stringBuilder21.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder22 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(127, 3, stringBuilder2);
				handler.AppendLiteral("  <color=#a0a4ad>↳</color> <b>");
				handler.AppendFormatted(config.CmdMod);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(config.CmdModAdd);
				handler.AppendLiteral("/");
				handler.AppendFormatted(config.CmdModRemove);
				handler.AppendLiteral(" color</b> <color=#FFD86A>[colorName|id]</color> <color=#8a8f99>—</color> pick by in-game color");
				stringBuilder22.AppendLine(ref handler);
			}
			if (!string.IsNullOrEmpty(config.CmdVip))
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder23 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(142, 4, stringBuilder2);
				handler.AppendLiteral("<b>");
				handler.AppendFormatted(config.CmdVip);
				handler.AppendLiteral("</b> <color=#FFD86A>");
				handler.AppendFormatted(config.CmdVipAdd);
				handler.AppendLiteral("|");
				handler.AppendFormatted(config.CmdVipRemove);
				handler.AppendLiteral("|");
				handler.AppendFormatted(config.CmdVipList);
				handler.AppendLiteral("</color> <color=#8a8f99>—</color> manage VIPs (color-only access) <size=80%><color=#B0B5BA>(host only)</color></size>");
				stringBuilder23.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder24 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(127, 3, stringBuilder2);
				handler.AppendLiteral("  <color=#a0a4ad>↳</color> <b>");
				handler.AppendFormatted(config.CmdVip);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(config.CmdVipAdd);
				handler.AppendLiteral("/");
				handler.AppendFormatted(config.CmdVipRemove);
				handler.AppendLiteral(" color</b> <color=#FFD86A>[colorName|id]</color> <color=#8a8f99>—</color> pick by in-game color");
				stringBuilder24.AppendLine(ref handler);
			}
			Local(stringBuilder.ToString());
		}
	}

	private static ClientData ResolveClient(string arg)
	{
		if (string.IsNullOrWhiteSpace(arg) || (Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return null;
		}
		string text = arg.Trim();
		try
		{
			Enumerator<ClientData> enumerator;
			if (int.TryParse(text, out var result))
			{
				enumerator = ((InnerNetClient)AmongUsClient.Instance).allClients.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ClientData current = enumerator.Current;
					if (current != null && current.Id == result)
					{
						return current;
					}
				}
			}
			enumerator = ((InnerNetClient)AmongUsClient.Instance).allClients.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ClientData current2 = enumerator.Current;
				if (current2 != null && !string.IsNullOrEmpty(current2.PlayerName) && string.Equals(current2.PlayerName, text, StringComparison.OrdinalIgnoreCase))
				{
					return current2;
				}
			}
			enumerator = ((InnerNetClient)AmongUsClient.Instance).allClients.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ClientData current3 = enumerator.Current;
				if (current3 != null && !string.IsNullOrEmpty(current3.PlayerName) && current3.PlayerName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return current3;
				}
			}
			enumerator = ((InnerNetClient)AmongUsClient.Instance).allClients.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ClientData current4 = enumerator.Current;
				if (current4 != null && !string.IsNullOrEmpty(current4.FriendCode) && string.Equals(current4.FriendCode, text, StringComparison.OrdinalIgnoreCase))
				{
					return current4;
				}
			}
		}
		catch
		{
		}
		return null;
	}

	private static void ExecuteKickBan(string targetArg, bool ban, string actorName)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			return;
		}
		string text = (ban ? "ban" : "kick");
		string value = (ban ? "banned" : "kicked");
		ClientData val = ResolveClient(targetArg);
		if (val == null)
		{
			Local($"<color=#ff5555>Could not find player '<b>{targetArg}</b>' to {text}.</color>");
			return;
		}
		if (val.Id == ((InnerNetClient)AmongUsClient.Instance).HostId)
		{
			Local("<color=#ff5555>Cannot " + text + " the host.</color>");
			return;
		}
		try
		{
			string text2 = val.PlayerName ?? $"id {val.Id}";
			((InnerNetClient)AmongUsClient.Instance).KickPlayer(val.Id, ban);
			if (ban && !string.IsNullOrEmpty(val.FriendCode))
			{
				CrewBanListService.Add(val.FriendCode, text2, "by " + actorName);
			}
			try
			{
				NotifyUtils.Info($"{actorName} {value} {text2}");
			}
			catch
			{
			}
			Local($"<color=#00ff88><b>{actorName}</b> {value} <b>{text2}</b>.</color>");
		}
		catch (Exception value2)
		{
			Local($"<color=#ff5555>Failed to {text} {val.PlayerName}.</color>");
			Debug.LogError(Object.op_Implicit($"[CrewMod] {text} error: {value2}"));
		}
	}

	private static void ExecuteKickBanByColor(string colorArg, bool ban, string actorName)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			return;
		}
		string text = (ban ? "ban" : "kick");
		string value = (ban ? "banned" : "kicked");
		if (string.IsNullOrWhiteSpace(colorArg))
		{
			Local("<color=#ff5555>Usage: " + (ban ? "ban" : "kick") + " color [colorName|id]</color>");
			Local(ColorListHint());
			return;
		}
		if (!TryParseColor(colorArg, out var colorId, out var colorName))
		{
			Local("<color=#ff5555>Unknown color '<b>" + colorArg + "</b>'.</color>");
			Local(ColorListHint());
			return;
		}
		ClientData val = FindClientByColor(colorId);
		if (val == null)
		{
			Local("<color=#ff5555>No player with color <b>" + colorName + "</b> in lobby.</color>");
			return;
		}
		if (val.Id == ((InnerNetClient)AmongUsClient.Instance).HostId)
		{
			Local("<color=#ff5555>Cannot " + text + " the host.</color>");
			return;
		}
		try
		{
			string text2 = val.PlayerName ?? $"id {val.Id}";
			((InnerNetClient)AmongUsClient.Instance).KickPlayer(val.Id, ban);
			if (ban && !string.IsNullOrEmpty(val.FriendCode))
			{
				CrewBanListService.Add(val.FriendCode, text2, $"by {actorName} ({colorName})");
			}
			try
			{
				NotifyUtils.Info($"{actorName} {value} {text2} ({colorName})");
			}
			catch
			{
			}
			Local($"<color=#00ff88><b>{actorName}</b> {value} <b>{text2}</b> <size=80%><color=#B0B5BA>(color: {colorName})</color></size>.</color>");
		}
		catch (Exception value2)
		{
			Local($"<color=#ff5555>Failed to {text} player with color {colorName}.</color>");
			Debug.LogError(Object.op_Implicit($"[CrewMod] {text} by color error: {value2}"));
		}
	}

	private static void StaffAddByColor(string colorArg, bool isVip)
	{
		if (string.IsNullOrWhiteSpace(colorArg))
		{
			Local("<color=#ff5555>Usage: color [colorName|id]</color>");
			Local(ColorListHint());
			return;
		}
		if (!TryParseColor(colorArg, out var colorId, out var colorName))
		{
			Local("<color=#ff5555>Unknown color '<b>" + colorArg + "</b>'.</color>");
			Local(ColorListHint());
			return;
		}
		ClientData val = FindClientByColor(colorId);
		if (val == null)
		{
			Local("<color=#ff5555>No player with color <b>" + colorName + "</b> in lobby.</color>");
			return;
		}
		string text = val.PlayerName ?? $"id {val.Id}";
		string text2 = (string.IsNullOrEmpty(val.FriendCode) ? null : val.FriendCode.Trim());
		if (string.IsNullOrEmpty(text2))
		{
			Local($"<color=#ff5555>Cannot add <b>{text}</b> <size=80%><color=#B0B5BA>(color: {colorName})</color></size>: player has no friend code (guest).</color>");
		}
		else if (isVip)
		{
			if (CrewModeratorService.IsModerator(text2))
			{
				Local($"<color=#ffa500><b>{text}</b> <size=80%><color=#B0B5BA>(color: {colorName})</color></size> is already a moderator (has full staff access).</color>");
			}
			else if (CrewVipService.Add(text2, text))
			{
				Local($"<color=#FFD86A>Added <b>{text}</b> <size=80%><color=#B0B5BA>(color: {colorName}, {text2})</color></size> as VIP.</color> <size=80%><color=#B0B5BA>(can use /color only)</color></size>");
			}
			else
			{
				Local($"<color=#ffa500><b>{text}</b> <size=80%><color=#B0B5BA>(color: {colorName})</color></size> is already a VIP.</color>");
			}
		}
		else if (CrewModeratorService.Add(text2, text))
		{
			Local($"<color=#00ff88>Added <b>{text}</b> <size=80%><color=#B0B5BA>(color: {colorName}, {text2})</color></size> as moderator.</color>");
		}
		else
		{
			Local($"<color=#ffa500><b>{text}</b> <size=80%><color=#B0B5BA>(color: {colorName})</color></size> is already a moderator.</color>");
		}
	}

	private static void StaffRemoveByColor(string colorArg, bool isVip)
	{
		if (string.IsNullOrWhiteSpace(colorArg))
		{
			Local("<color=#ff5555>Usage: color [colorName|id]</color>");
			Local(ColorListHint());
			return;
		}
		if (!TryParseColor(colorArg, out var colorId, out var colorName))
		{
			Local("<color=#ff5555>Unknown color '<b>" + colorArg + "</b>'.</color>");
			Local(ColorListHint());
			return;
		}
		ClientData val = FindClientByColor(colorId);
		if (val == null)
		{
			Local("<color=#ff5555>No player with color <b>" + colorName + "</b> in lobby.</color>");
			return;
		}
		string value = val.PlayerName ?? $"id {val.Id}";
		string text = (string.IsNullOrEmpty(val.FriendCode) ? null : val.FriendCode.Trim());
		if (string.IsNullOrEmpty(text))
		{
			Local($"<color=#ff5555><b>{value}</b> <size=80%><color=#B0B5BA>(color: {colorName})</color></size> has no friend code.</color>");
		}
		else if (isVip ? CrewVipService.Remove(text) : CrewModeratorService.Remove(text))
		{
			Local($"<color=#00ff88>Removed <b>{value}</b> <size=80%><color=#B0B5BA>(color: {colorName}, {text})</color></size>{(isVip ? " from VIPs" : "")}.</color>");
		}
		else
		{
			Local($"<color=#ffa500><b>{value}</b> <size=80%><color=#B0B5BA>(color: {colorName})</color></size> is not a {(isVip ? "VIP" : "moderator")}.</color>");
		}
	}

	private static string ResolveFriendCode(string arg, out string resolvedName)
	{
		resolvedName = null;
		if (string.IsNullOrWhiteSpace(arg))
		{
			return null;
		}
		string text = arg.Trim();
		try
		{
			if ((Object)(object)AmongUsClient.Instance != (Object)null)
			{
				Enumerator<ClientData> enumerator = ((InnerNetClient)AmongUsClient.Instance).allClients.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ClientData current = enumerator.Current;
					if (current != null && !string.IsNullOrEmpty(current.PlayerName) && string.Equals(current.PlayerName, text, StringComparison.OrdinalIgnoreCase))
					{
						resolvedName = current.PlayerName;
						if (!string.IsNullOrEmpty(current.FriendCode))
						{
							return current.FriendCode.Trim();
						}
					}
				}
				enumerator = ((InnerNetClient)AmongUsClient.Instance).allClients.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ClientData current2 = enumerator.Current;
					if (current2 != null && !string.IsNullOrEmpty(current2.FriendCode) && string.Equals(current2.FriendCode, text, StringComparison.OrdinalIgnoreCase))
					{
						resolvedName = current2.PlayerName;
						return current2.FriendCode.Trim();
					}
				}
			}
		}
		catch
		{
		}
		if (text.Contains("#"))
		{
			return text;
		}
		return null;
	}

	private static string GetAuthoritativeFriendCode(PlayerControl pc)
	{
		if ((Object)(object)pc == (Object)null)
		{
			return null;
		}
		string friendCode = pc.FriendCode;
		if (!string.IsNullOrEmpty(friendCode))
		{
			return friendCode.Trim();
		}
		try
		{
			if ((Object)(object)AmongUsClient.Instance != (Object)null)
			{
				Enumerator<ClientData> enumerator = ((InnerNetClient)AmongUsClient.Instance).allClients.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ClientData current = enumerator.Current;
					if (current != null && (Object)(object)current.Character != (Object)null && current.Character.PlayerId == pc.PlayerId)
					{
						return string.IsNullOrEmpty(current.FriendCode) ? null : current.FriendCode.Trim();
					}
				}
			}
		}
		catch
		{
		}
		return null;
	}

	private static void HandlePing(float cooldown)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			Local("<color=#ff0000>Error: You are not connected to a lobby.</color>");
		}
		else if ((DateTime.UtcNow - _lastCommandTime).TotalSeconds < (double)cooldown)
		{
			int value = (int)Math.Ceiling((double)cooldown - (DateTime.UtcNow - _lastCommandTime).TotalSeconds);
			Local($"<color=#ffff00>Please wait {value}s before using this command again.</color>");
		}
		else
		{
			_lastCommandTime = DateTime.UtcNow;
			Local(BuildInfoMessage());
		}
	}

	private static string BuildInfoMessage()
	{
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("<b><color=#00ffff>--- Room Status ---</color></b>");
		int ping = ((InnerNetClient)AmongUsClient.Instance).Ping;
		string pingColor = GetPingColor(ping);
		float value = 1f / Time.smoothDeltaTime;
		string regionName = GetRegionName();
		string value2 = "N/A";
		if (((InnerNetClient)AmongUsClient.Instance).GameId != 0)
		{
			value2 = GameCode.IntToGameName(((InnerNetClient)AmongUsClient.Instance).GameId);
		}
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(65, 4, stringBuilder2);
		handler.AppendLiteral("<b>Ping:</b> <color=#");
		handler.AppendFormatted(pingColor);
		handler.AppendLiteral(">");
		handler.AppendFormatted(ping);
		handler.AppendLiteral("ms</color> | <b>FPS:</b> ");
		handler.AppendFormatted(value, "0");
		handler.AppendLiteral(" | <b>Region:</b> ");
		handler.AppendFormatted(regionName);
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(41, 1, stringBuilder2);
		handler.AppendLiteral("<b>Room Code:</b> <color=#ffa500>");
		handler.AppendFormatted(value2);
		handler.AppendLiteral("</color>");
		stringBuilder4.AppendLine(ref handler);
		stringBuilder.AppendLine("");
		if (PlayerControl.AllPlayerControls.Count > 0)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(41, 1, stringBuilder2);
			handler.AppendLiteral("<b><color=#ffff00>Players (");
			handler.AppendFormatted(PlayerControl.AllPlayerControls.Count);
			handler.AppendLiteral("):</color></b>");
			stringBuilder5.AppendLine(ref handler);
			Dictionary<byte, ClientData> dictionary = new Dictionary<byte, ClientData>();
			Enumerator<ClientData> enumerator = ((InnerNetClient)AmongUsClient.Instance).allClients.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ClientData current = enumerator.Current;
				if (current != null && (Object)(object)current.Character != (Object)null)
				{
					dictionary[current.Character.PlayerId] = current;
				}
			}
			List<PlayerControl> list = new List<PlayerControl>();
			Enumerator<PlayerControl> enumerator2 = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				PlayerControl current2 = enumerator2.Current;
				list.Add(current2);
			}
			list.Sort((PlayerControl p1, PlayerControl p2) => p1.PlayerId.CompareTo(p2.PlayerId));
			foreach (PlayerControl item in list)
			{
				if ((Object)(object)item.Data == (Object)null)
				{
					continue;
				}
				string playerName = item.Data.PlayerName;
				bool num = (Object)(object)item == (Object)(object)PlayerControl.LocalPlayer;
				bool flag = false;
				string value3 = "Unknown";
				string value4 = "";
				if (dictionary.TryGetValue(item.PlayerId, out var value5))
				{
					flag = value5.Id == ((InnerNetClient)AmongUsClient.Instance).HostId;
					value3 = GetPlatformName(value5.PlatformData.Platform);
					if (!string.IsNullOrEmpty(value5.FriendCode) && CrewModeratorService.IsModerator(value5.FriendCode))
					{
						value4 = " <color=#ffbf00>[MOD]</color>";
					}
				}
				string value6 = (flag ? "<color=#ff0000>[HOST]</color> " : "");
				string value7 = (num ? " <color=#00ff00>(You)</color>" : "");
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder6 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(43, 5, stringBuilder2);
				handler.AppendFormatted(value6);
				handler.AppendFormatted(playerName);
				handler.AppendFormatted(value7);
				handler.AppendFormatted(value4);
				handler.AppendLiteral(" <size=80%><color=#B0B5BA>[");
				handler.AppendFormatted(value3);
				handler.AppendLiteral("]</color></size>");
				stringBuilder6.AppendLine(ref handler);
			}
		}
		else
		{
			stringBuilder.AppendLine("<i>No players found (empty lobby or error).</i>");
		}
		return stringBuilder.ToString();
	}

	private static string GetPingColor(int ping)
	{
		if (ping < 100)
		{
			return "00ff00";
		}
		if (ping < 200)
		{
			return "ffff00";
		}
		return "ff0000";
	}

	private static string GetRegionName()
	{
		if (DestroyableSingleton<ServerManager>.InstanceExists)
		{
			return DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name;
		}
		return "Unknown";
	}

	private static string GetPlatformName(Platforms platform)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected I4, but got Unknown
		switch (platform - 1)
		{
		case 5:
		case 6:
			return "Mobile";
		case 7:
			return "Switch";
		case 8:
			return "Xbox";
		case 9:
			return "PlayStation";
		case 1:
			return "PC (Steam)";
		case 0:
			return "PC (Epic)";
		case 4:
			return "PC (Itch)";
		case 3:
			return "PC (MS Store)";
		default:
			return "PC";
		}
	}

	internal static void Local(string message)
	{
		if (DestroyableSingleton<HudManager>.InstanceExists && (Object)(object)DestroyableSingleton<HudManager>.Instance.Chat != (Object)null && (Object)(object)PlayerControl.LocalPlayer != (Object)null)
		{
			DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, message, false);
		}
	}
}
