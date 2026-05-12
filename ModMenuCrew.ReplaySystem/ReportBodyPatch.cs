using HarmonyLib;
using ModMenuCrew.Features;
using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

[HarmonyPatch(typeof(PlayerControl), "ReportDeadBody")]
public static class ReportBodyPatch
{
	public static void Prefix(PlayerControl __instance, NetworkedPlayerInfo target)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ReplayRecorder.Instance == (Object)null) && ReplayRecorder.Instance.IsRecording)
		{
			MeetingStartPatch._bodyJustReported = true;
			byte targetId = (((Object)(object)target != (Object)null) ? target.PlayerId : byte.MaxValue);
			NetworkedPlayerInfo data = __instance.Data;
			string text = ((data != null) ? data.PlayerName : null) ?? "?";
			string text2 = ((target != null) ? target.PlayerName : null) ?? "unknown";
			ReplayRecorder.Instance.LogEvent(ReplayEventType.Report, __instance.PlayerId, targetId, Vector2.op_Implicit(((Component)__instance).transform.position), text + " reported " + text2 + "'s body");
		}
	}
}
