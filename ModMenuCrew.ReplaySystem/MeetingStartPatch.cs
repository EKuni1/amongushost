using HarmonyLib;
using ModMenuCrew.Features;
using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

[HarmonyPatch(typeof(MeetingHud), "Start")]
public static class MeetingStartPatch
{
	internal static bool _bodyJustReported;

	public static void Postfix(MeetingHud __instance)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ReplayRecorder.Instance == (Object)null) && ReplayRecorder.Instance.IsRecording)
		{
			if (_bodyJustReported)
			{
				_bodyJustReported = false;
				ReplayRecorder.Instance.LogEvent(ReplayEventType.Meeting, byte.MaxValue, byte.MaxValue, Vector2.zero, "Meeting started (body reported)");
			}
			else
			{
				ReplayRecorder.Instance.LogEvent(ReplayEventType.EmergencyButton, byte.MaxValue, byte.MaxValue, Vector2.zero, "Emergency meeting called");
			}
		}
	}
}
