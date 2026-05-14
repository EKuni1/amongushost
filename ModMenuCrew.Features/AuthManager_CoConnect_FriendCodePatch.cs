using HarmonyLib;
using UnityEngine;

namespace ModMenuCrew.Features;

[HarmonyPatch(typeof(AuthManager), "CoConnect")]
public static class AuthManager_CoConnect_FriendCodePatch
{
	public static void Prefix()
	{
		if (SpoofingService.EnableFriendCodeSpoof)
		{
			SpoofingService.ApplyFriendCodeSpoof();
			Debug.Log(Object.op_Implicit("[SpoofingService] FriendCode spoof applied before AuthManager.CoConnect/BuildData"));
		}
	}
}
