using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using Hazel;
using InnerNet;
using ModMenuCrew.Messages;
using TMPro;
using UnityEngine;

namespace ModMenuCrew.Features;

public static class MMCIdentification
{
	private const float TAG_OFFSET_Y = 0.4f;

	private static HashSet<byte> _mmcPlayers = new HashSet<byte>();

	private static HashSet<byte> _taggedPlayers = new HashSet<byte>();

	private static float _lastHandshakeTime = 0f;

	private static bool _handshakeSent = false;

	private static float _hue = 0f;

	private static StringBuilder _sharedSb = new StringBuilder(256);

	private static readonly char STAR_CHAR = '★';

	private static readonly string MMC_TAG = "MMC";

	private static readonly string NAME_COLOR_START = "<color=#FFFFFF>";

	private static readonly string NAME_COLOR_END = "</color>";

	private static string MMC_SIGNATURE
	{
		get
		{
			if (string.IsNullOrEmpty(ServerData.Config.MmcSignature))
			{
				return "MMC_v5";
			}
			return ServerData.Config.MmcSignature;
		}
	}

	private static float HANDSHAKE_INTERVAL
	{
		get
		{
			if (!(ServerData.Config.MmcHandshakeInterval > 0f))
			{
				return 5f;
			}
			return ServerData.Config.MmcHandshakeInterval;
		}
	}

	public static bool Enabled { get; set; } = true;


	public static int MMCPlayerCount => _mmcPlayers.Count;

	private static bool IsSystemDisabled => CheatConfig.HideMMCStar?.Value ?? false;

	public static bool IsMMCPlayer(byte playerId)
	{
		return _mmcPlayers.Contains(playerId);
	}

	public static List<string> GetMMCPlayerNames()
	{
		List<string> list = new List<string>();
		if (PlayerControl.AllPlayerControls == null)
		{
			return list;
		}
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && _mmcPlayers.Contains(current.PlayerId))
			{
				NetworkedPlayerInfo data = current.Data;
				list.Add(((data != null) ? data.PlayerName : null) ?? "Unknown");
			}
		}
		return list;
	}

	public static void ClearMMCPlayers()
	{
		_mmcPlayers.Clear();
		_taggedPlayers.Clear();
		_handshakeSent = false;
		_lastHandshakeTime = 0f;
	}

	public static void ClearAppliedTags()
	{
		_taggedPlayers.Clear();
	}

	public static void ReapplyAllMMCTags()
	{
		if (!Enabled || IsSystemDisabled || PlayerControl.AllPlayerControls == null)
		{
			return;
		}
		_taggedPlayers.Clear();
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && _mmcPlayers.Contains(current.PlayerId))
			{
				ApplyMMCTagToPlayer(current);
			}
		}
	}

	public static void SendHandshake()
	{
		if (!Enabled || IsSystemDisabled || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected || (Object)(object)PlayerControl.LocalPlayer == (Object)null)
		{
			return;
		}
		try
		{
			MessageWriter val = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)PlayerControl.LocalPlayer).NetId, CustomRpcCalls.MMCHandshake, (SendOption)1, -1);
			val.Write(MMC_SIGNATURE);
			val.Write(PlayerControl.LocalPlayer.PlayerId);
			val.Write("6.1.4b");
			((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val);
			_handshakeSent = true;
			_lastHandshakeTime = Time.time;
			if (!_mmcPlayers.Contains(PlayerControl.LocalPlayer.PlayerId))
			{
				_mmcPlayers.Add(PlayerControl.LocalPlayer.PlayerId);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[MMCIdentification] Error sending handshake: " + ex.Message));
		}
	}

	public static void HandleHandshake(MessageReader reader, byte senderId)
	{
		if (!Enabled || IsSystemDisabled)
		{
			return;
		}
		try
		{
			string text = reader.ReadString();
			byte b = reader.ReadByte();
			string value = reader.ReadString();
			if (text != MMC_SIGNATURE)
			{
				Debug.LogWarning(Object.op_Implicit($"[MMCIdentification] Invalid signature from {b}"));
			}
			else
			{
				if (_mmcPlayers.Contains(b))
				{
					return;
				}
				_mmcPlayers.Add(b);
				string value2 = "Unknown";
				PlayerControl val = null;
				if (PlayerControl.AllPlayerControls != null)
				{
					Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
					while (enumerator.MoveNext())
					{
						PlayerControl current = enumerator.Current;
						if ((Object)(object)current != (Object)null && current.PlayerId == b)
						{
							NetworkedPlayerInfo data = current.Data;
							value2 = ((data != null) ? data.PlayerName : null) ?? "Unknown";
							val = current;
							break;
						}
					}
				}
				Debug.Log(Object.op_Implicit($"[MMCIdentification] New MMC player identified: {value2} (ID: {b}, v{value})"));
				if ((Object)(object)val != (Object)null && !GameCheats.IsRevealSusActive)
				{
					ApplyMMCTagToPlayer(val);
				}
				if (!_handshakeSent)
				{
					SendHandshake();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[MMCIdentification] Error processing handshake: " + ex.Message));
		}
	}

	public static void Update()
	{
		if (Enabled && !IsSystemDisabled && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmConnected && !((Object)(object)PlayerControl.LocalPlayer == (Object)null))
		{
			LobbyStartPatch.CheckPendingHandshake();
			if (Time.time - _lastHandshakeTime > HANDSHAKE_INTERVAL)
			{
				SendHandshake();
			}
		}
	}

	private static string GetRainbowColorHex()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_hue += Time.deltaTime * 0.5f;
		if (_hue > 1f)
		{
			_hue -= 1f;
		}
		return ColorUtility.ToHtmlStringRGB(Color.HSVToRGB(_hue, 1f, 1f));
	}

	private static int GetPulsingSize()
	{
		return (int)((0.9f + Mathf.PingPong(Time.time * 2f, 0.2f)) * 100f);
	}

	public static void ApplyMMCTagToPlayer(PlayerControl player)
	{
		if ((Object)(object)((player != null) ? player.Data : null) == (Object)null)
		{
			return;
		}
		CosmeticsLayer cosmetics = player.cosmetics;
		if ((Object)(object)((cosmetics != null) ? cosmetics.nameText : null) == (Object)null || IsSystemDisabled)
		{
			return;
		}
		ConfigEntry<bool> hideMMCStar = CheatConfig.HideMMCStar;
		if ((hideMMCStar != null && hideMMCStar.Value) || _taggedPlayers.Contains(player.PlayerId))
		{
			return;
		}
		try
		{
			byte playerId = player.PlayerId;
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			bool flag = playerId == ((localPlayer != null) ? localPlayer.PlayerId : byte.MaxValue);
			if (flag)
			{
				ConfigEntry<bool> customNameEnabled = MiscConfig.CustomNameEnabled;
				if (customNameEnabled != null && customNameEnabled.Value)
				{
					_taggedPlayers.Add(player.PlayerId);
					Debug.Log(Object.op_Implicit("[MMCIdentification] Local player MMC tag delegated to MiscNameService"));
					return;
				}
			}
			string playerName = player.Data.PlayerName;
			string text = "FFD700";
			string text2 = (flag ? ("<color=#" + text + ">★</color> ") : ("<color=#" + text + "><b>★ MMC ★</b></color> ")) + "<color=#FFFFFF>" + playerName + "</color>";
			((TMP_Text)player.cosmetics.nameText).text = text2;
			_taggedPlayers.Add(player.PlayerId);
			Debug.Log(Object.op_Implicit($"[MMCIdentification] Tag applied to {playerName} (ID: {player.PlayerId})"));
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[MMCIdentification] Error applying tag: " + ex.Message));
		}
	}

	public static void UpdateMMCNameTags()
	{
		if (!Enabled || GameCheats.IsRevealSusActive)
		{
			return;
		}
		List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
		if (allPlayerControls == null || allPlayerControls.Count == 0)
		{
			return;
		}
		ConfigEntry<bool> hideMMCStar = CheatConfig.HideMMCStar;
		if ((hideMMCStar != null && hideMMCStar.Value) || IsSystemDisabled)
		{
			for (int i = 0; i < allPlayerControls.Count; i++)
			{
				PlayerControl val = allPlayerControls[i];
				if ((Object)(object)((val != null) ? val.Data : null) == (Object)null)
				{
					continue;
				}
				CosmeticsLayer cosmetics = val.cosmetics;
				if (!((Object)(object)((cosmetics != null) ? cosmetics.nameText : null) == (Object)null))
				{
					string text = ((TMP_Text)val.cosmetics.nameText).text;
					if (text != null && (text.IndexOf(STAR_CHAR) >= 0 || text.IndexOf(MMC_TAG, StringComparison.Ordinal) >= 0))
					{
						((TMP_Text)val.cosmetics.nameText).text = val.Data.PlayerName;
					}
				}
			}
		}
		else
		{
			if (_mmcPlayers.Count == 0)
			{
				return;
			}
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			byte b = ((localPlayer != null) ? localPlayer.PlayerId : byte.MaxValue);
			bool isKillCooldownsActive = GameCheats.IsKillCooldownsActive;
			string rainbowColorHex = GetRainbowColorHex();
			int pulsingSize = GetPulsingSize();
			for (int j = 0; j < allPlayerControls.Count; j++)
			{
				PlayerControl val2 = allPlayerControls[j];
				if ((Object)(object)((val2 != null) ? val2.Data : null) == (Object)null)
				{
					continue;
				}
				CosmeticsLayer cosmetics2 = val2.cosmetics;
				if ((Object)(object)((cosmetics2 != null) ? cosmetics2.nameText : null) == (Object)null)
				{
					continue;
				}
				byte playerId = val2.PlayerId;
				int num;
				if (isKillCooldownsActive)
				{
					RoleBehaviour role = val2.Data.Role;
					if (role != null && role.IsImpostor)
					{
						num = ((!val2.Data.IsDead) ? 1 : 0);
						goto IL_01a0;
					}
				}
				num = 0;
				goto IL_01a0;
				IL_01a0:
				bool flag = (byte)num != 0;
				if (!_mmcPlayers.Contains(playerId))
				{
					if (!flag)
					{
						string text2 = ((TMP_Text)val2.cosmetics.nameText).text;
						if (text2 != null && (text2.IndexOf(STAR_CHAR) >= 0 || text2.IndexOf(MMC_TAG, StringComparison.Ordinal) >= 0 || text2.IndexOf('\n') >= 0))
						{
							((TMP_Text)val2.cosmetics.nameText).text = val2.Data.PlayerName;
						}
					}
					continue;
				}
				string playerName = val2.Data.PlayerName;
				bool flag2 = playerId == b;
				if (flag2)
				{
					ConfigEntry<bool> customNameEnabled = MiscConfig.CustomNameEnabled;
					if (customNameEnabled != null && customNameEnabled.Value)
					{
						continue;
					}
				}
				_sharedSb.Clear();
				_sharedSb.Append("<size=").Append(pulsingSize).Append("%><color=#")
					.Append(rainbowColorHex)
					.Append(">");
				if (flag2)
				{
					_sharedSb.Append("★");
				}
				else
				{
					_sharedSb.Append("<b>★ MMC USER ★</b>");
				}
				_sharedSb.Append("</color></size> ");
				_sharedSb.Append(NAME_COLOR_START).Append(playerName).Append(NAME_COLOR_END);
				if (isKillCooldownsActive)
				{
					RoleBehaviour role2 = val2.Data.Role;
					if (role2 != null && role2.IsImpostor && !val2.Data.IsDead)
					{
						try
						{
							GameCheats.AppendKillCooldownSuffix(val2, _sharedSb);
						}
						catch
						{
						}
					}
				}
				ConfigEntry<bool> showPlayerInfo = CheatConfig.ShowPlayerInfo;
				if (showPlayerInfo != null && showPlayerInfo.Value)
				{
					try
					{
						string text3 = PlayerInfoDisplay.BuildInfoSuffix(val2);
						if (text3 != null)
						{
							_sharedSb.Append(text3);
						}
					}
					catch
					{
					}
				}
				string text4 = _sharedSb.ToString();
				if (((TMP_Text)val2.cosmetics.nameText).text != text4)
				{
					((TMP_Text)val2.cosmetics.nameText).text = text4;
				}
			}
		}
	}

	public static void RenderMMCTags()
	{
	}

	public static void RemovePlayer(byte playerId)
	{
		_mmcPlayers.Remove(playerId);
		_taggedPlayers.Remove(playerId);
	}
}
