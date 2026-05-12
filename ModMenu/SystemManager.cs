using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using UnityEngine;

namespace ModMenuCrew;

public static class SystemManager
{
	internal static void CloseDoorsOfType(SystemTypes type)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			NotifyUtils.Error("ShipStatus not available!");
			return;
		}
		if (!IsDoorSystem(type))
		{
			NotifyUtils.Warning($"{type} does not have doors to close.");
			return;
		}
		try
		{
			ShipStatus.Instance.RpcCloseDoorsOfType(type);
			NotifyUtils.Door($"{type} doors closed!");
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[SystemManager] Error closing {type} doors: {value}"));
			NotifyUtils.Error($"Failed to close {type} doors!");
		}
	}

	private static bool IsDoorSystem(SystemTypes type)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ShipStatus.Instance == (Object)null)
		{
			return false;
		}
		Il2CppReferenceArray<OpenableDoor> allDoors = ShipStatus.Instance.AllDoors;
		if (allDoors == null)
		{
			return false;
		}
		for (int i = 0; i < ((Il2CppArrayBase<OpenableDoor>)(object)allDoors).Length; i++)
		{
			if ((Object)(object)((Il2CppArrayBase<OpenableDoor>)(object)allDoors)[i] != (Object)null && ((Il2CppArrayBase<OpenableDoor>)(object)allDoors)[i].Room == type)
			{
				return true;
			}
		}
		return false;
	}
}
