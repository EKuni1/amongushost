using HarmonyLib;
using ModMenuCrew.Features;
using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

[HarmonyPatch(typeof(ShipStatus), "RpcCloseDoorsOfType")]
public static class DoorClosePatch
{
	public static void Postfix(ShipStatus __instance, SystemTypes type)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected I4, but got Unknown
		if ((Object)(object)ReplayRecorder.Instance == (Object)null || !ReplayRecorder.Instance.IsRecording || (Object)(object)__instance == (Object)null)
		{
			return;
		}
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			byte playerId = ((localPlayer != null) ? localPlayer.PlayerId : byte.MaxValue);
			ReplayRecorder.Instance.LogEvent(ReplayEventType.DoorClose, playerId, (byte)(int)type, Vector2.zero, $"Doors closed in {type}");
		}
		catch
		{
		}
	}
}
