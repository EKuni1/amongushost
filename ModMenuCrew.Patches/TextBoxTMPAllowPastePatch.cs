using HarmonyLib;
using ModMenuCrew.Features;
using UnityEngine;

namespace ModMenuCrew.Patches;

[HarmonyPatch(typeof(TextBoxTMP), "Update")]
public static class TextBoxTMPAllowPastePatch
{
	public static void Prefix(TextBoxTMP __instance)
	{
		try
		{
			if (!((Object)(object)__instance == (Object)null) && !__instance.AllowPaste && CheatConfig.ChatAllowPaste != null && CheatConfig.ChatAllowPaste.Value)
			{
				__instance.AllowPaste = true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError($"[TextBoxTMPAllowPastePatch] Error: {ex.Message}");
		}
	}
}
