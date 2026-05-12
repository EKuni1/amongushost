using System;
using BepInEx.Configuration;
using HarmonyLib;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace ModMenuCrew.Features;

[HarmonyPatch(typeof(PlayerControl), "MurderPlayer")]
public static class KillLogPatch
{
	public static void Postfix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((Enum)resultFlags).HasFlag((Enum)(object)(MurderResultFlags)1) && !((Object)(object)((target != null) ? target.Data : null) == (Object)null) && !((Object)(object)((__instance != null) ? __instance.Data : null) == (Object)null))
			{
				string roomName = GetRoomName(target.GetTruePosition());
				EventLogger.LogKill(__instance, target, roomName);
				ConfigEntry<bool> killAlertsEnabled = CheatConfig.KillAlertsEnabled;
				if (killAlertsEnabled != null && killAlertsEnabled.Value)
				{
					GameCheats.ShowKillAlert(__instance, target, roomName);
				}
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[KillLogPatch] Error: {value}"));
		}
	}

	private static string GetRoomName(Vector2 position)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ShipStatus.Instance == (Object)null)
		{
			return "Unknown";
		}
		try
		{
			Enumerator<SystemTypes, PlainShipRoom> enumerator = ShipStatus.Instance.FastRooms.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<SystemTypes, PlainShipRoom> current = enumerator.Current;
				if ((Object)(object)current.Value != (Object)null && (Object)(object)current.Value.roomArea != (Object)null && current.Value.roomArea.OverlapPoint(position))
				{
					SystemTypes roomId = current.Value.RoomId;
					return roomId.ToString();
				}
			}
		}
		catch
		{
		}
		return "Hallway";
	}
}
