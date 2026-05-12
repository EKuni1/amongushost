using AmongUs.Data;
using HarmonyLib;
using Il2CppSystem;
using UnityEngine;

namespace ModMenuCrew.Features;

[HarmonyPatch(typeof(InventoryManager), "CheckExpAndLevel")]
public static class InventoryManager_CheckExpAndLevel_Patch
{
	public static void Postfix()
	{
		if (SpoofingService.EnableLevelSpoof)
		{
			uint effectiveLevel = SpoofingService.GetEffectiveLevel();
			if (DataManager.Player.Stats.Level != effectiveLevel)
			{
				DataManager.Player.Stats.Level = effectiveLevel;
				((AbstractSaveData)DataManager.Player).Save();
				Debug.Log(Object.op_Implicit($"[SpoofingService] CheckExpAndLevel override: Level {effectiveLevel} salvo no perfil."));
			}
		}
	}
}
