using HarmonyLib;
using ModMenuCrew.Features;
using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

[HarmonyPatch(typeof(ChatController), "AddChat")]
public static class ChatPatch
{
	public static void Postfix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ReplayRecorder.Instance == (Object)null || !ReplayRecorder.Instance.IsRecording || (Object)(object)sourcePlayer == (Object)null || string.IsNullOrEmpty(chatText))
		{
			return;
		}
		try
		{
			NetworkedPlayerInfo data = sourcePlayer.Data;
			string text = ((data != null) ? data.PlayerName : null) ?? "?";
			ReplayRecorder.Instance.LogEvent(ReplayEventType.Chat, sourcePlayer.PlayerId, 0, Vector2.op_Implicit(((Component)sourcePlayer).transform.position), text + ": " + chatText);
		}
		catch
		{
		}
	}
}
