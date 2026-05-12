using HarmonyLib;
using ModMenuCrew.Features;
using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

[HarmonyPatch(typeof(PlayerPhysics), "StartClimb")]
public static class ClimbPatch
{
	public static void Postfix(PlayerPhysics __instance, bool down)
	{
		if (!((Object)(object)ReplayRecorder.Instance == (Object)null) && ReplayRecorder.Instance.IsRecording && !((Object)(object)((__instance != null) ? __instance.myPlayer : null) == (Object)null))
		{
			ReplayRecorder.Instance.SetAnimState(__instance.myPlayer.PlayerId, down ? AnimState.ClimbDown : AnimState.Climb);
		}
	}
}
