using HarmonyLib;
using ModMenuCrew.Features;
using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

[HarmonyPatch(typeof(PlayerControl), "CompleteTask")]
public static class TaskCompletePatch
{
	public static void Postfix(PlayerControl __instance, uint idx)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ReplayRecorder.Instance == (Object)null || !ReplayRecorder.Instance.IsRecording || (Object)(object)__instance == (Object)null)
		{
			return;
		}
		try
		{
			NetworkedPlayerInfo data = __instance.Data;
			string text = ((data != null) ? data.PlayerName : null) ?? "?";
			ReplayRecorder.Instance.LogEvent(ReplayEventType.TaskComplete, __instance.PlayerId, (byte)(idx & 0xFFu), Vector2.op_Implicit(((Component)__instance).transform.position), text + " completed a task");
		}
		catch
		{
		}
	}
}
