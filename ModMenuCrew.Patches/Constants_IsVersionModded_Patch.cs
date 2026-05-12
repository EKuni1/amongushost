using HarmonyLib;

namespace ModMenuCrew.Patches;

[HarmonyPatch(typeof(Constants), "IsVersionModded")]
public static class Constants_IsVersionModded_Patch
{
	[HarmonyPrefix]
	public static bool ForceReturnFalse(ref bool __result)
	{
		__result = false;
		return false;
	}
}
