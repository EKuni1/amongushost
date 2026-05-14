using HarmonyLib;
using UnityEngine;

namespace ModMenuCrew.Features;

[HarmonyPatch(typeof(PlayerControl), "RpcSetLevel")]
public static class PlayerControl_RpcSetLevel_Patch
{
	public static void Prefix(ref uint level)
	{
		if (SpoofingService.EnableLevelSpoof)
		{
			uint effectiveLevel = SpoofingService.GetEffectiveLevel();
			Debug.Log(Object.op_Implicit($"[SpoofingService] RpcSetLevel intercepted: {level} -> {effectiveLevel}"));
			level = effectiveLevel;
		}
	}
}
