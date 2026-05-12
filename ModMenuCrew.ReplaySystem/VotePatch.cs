using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using ModMenuCrew.Features;
using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

[HarmonyPatch(typeof(MeetingHud), "CastVote")]
public static class VotePatch
{
	public static void Postfix(MeetingHud __instance, byte srcPlayerId, byte suspectPlayerId)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ReplayRecorder.Instance == (Object)null || !ReplayRecorder.Instance.IsRecording)
		{
			return;
		}
		string text = "?";
		string text2 = ((suspectPlayerId == 253) ? "SKIP" : "?");
		try
		{
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if (!((Object)(object)((current != null) ? current.Data : null) == (Object)null))
				{
					if (current.PlayerId == srcPlayerId)
					{
						text = current.Data.PlayerName;
					}
					if (current.PlayerId == suspectPlayerId)
					{
						text2 = current.Data.PlayerName;
					}
				}
			}
		}
		catch
		{
		}
		ReplayRecorder.Instance.LogEvent(ReplayEventType.Vote, srcPlayerId, suspectPlayerId, Vector2.zero, text + " voted " + text2);
	}
}
