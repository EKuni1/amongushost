using System;
using HarmonyLib;
using Il2CppSystem;
using UnityEngine;

namespace ModMenuCrew.Features;

[HarmonyPatch(typeof(AmongUsClient), "OnDisconnected")]
public static class DisconnectPatch
{
	public static void Postfix()
	{
		MMCIdentification.ClearMMCPlayers();
		GameCheats.IsRevealSusActive = false;
		try
		{
			GameCheats.CleanupRadarTextures();
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[DisconnectPatch] FIX 2026: Radar cleanup error: " + ex.Message));
		}
		try
		{
			SabotageService.ResetDoorLocks();
		}
		catch
		{
		}
	}
}
