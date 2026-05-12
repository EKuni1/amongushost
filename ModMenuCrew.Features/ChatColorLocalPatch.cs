using HarmonyLib;
using UnityEngine;

namespace ModMenuCrew.Features;

[HarmonyPatch(typeof(ChatController), "AddChat")]
internal static class ChatColorLocalPatch
{
	internal static void Prefix(PlayerControl sourcePlayer, ref string chatText)
	{
		if (string.IsNullOrEmpty(chatText) || MiscConfig.ChatColorEnabled == null || !MiscConfig.ChatColorEnabled.Value)
		{
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (!((Object)(object)localPlayer == (Object)null) && !((Object)(object)sourcePlayer == (Object)null) && sourcePlayer.PlayerId == localPlayer.PlayerId && (chatText.Length < 8 || !chatText.StartsWith("<color=#")))
		{
			string text = ValidateHex(MiscConfig.ChatColorHex?.Value);
			if (text != null)
			{
				chatText = "<color=#" + text + ">" + chatText + "</color>";
			}
		}
	}

	private static string ValidateHex(string hex)
	{
		if (string.IsNullOrEmpty(hex))
		{
			return null;
		}
		if (hex[0] == '#')
		{
			hex = hex.Substring(1);
		}
		if (hex.Length != 6)
		{
			return null;
		}
		for (int i = 0; i < 6; i++)
		{
			char c = hex[i];
			if ((c < '0' || c > '9') && (c < 'a' || c > 'f') && (c < 'A' || c > 'F'))
			{
				return null;
			}
		}
		return hex.ToUpperInvariant();
	}
}
