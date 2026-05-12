using HarmonyLib;
using ModMenuCrew.Features;
using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

[HarmonyPatch(typeof(PlayerPhysics), "RpcEnterVent")]
public static class VentEnterPatch
{
	public static void Postfix(PlayerPhysics __instance, int ventId)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ReplayRecorder.Instance == (Object)null) && ReplayRecorder.Instance.IsRecording)
		{
			PlayerControl myPlayer = __instance.myPlayer;
			object obj;
			if (myPlayer == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo data = myPlayer.Data;
				obj = ((data != null) ? data.PlayerName : null);
			}
			if (obj == null)
			{
				obj = "?";
			}
			string text = (string)obj;
			ReplayRecorder.Instance.LogEvent(ReplayEventType.Vent, __instance.myPlayer.PlayerId, (byte)ventId, Vector2.op_Implicit(((Component)__instance).transform.position), text + " entered vent");
			ReplayRecorder.Instance.SetAnimState(__instance.myPlayer.PlayerId, AnimState.VentEnter);
		}
	}
}
