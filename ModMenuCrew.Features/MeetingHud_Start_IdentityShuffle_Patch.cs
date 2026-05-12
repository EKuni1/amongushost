using HarmonyLib;

namespace ModMenuCrew.Features;

[HarmonyPatch(typeof(MeetingHud), "Start")]
public static class MeetingHud_Start_IdentityShuffle_Patch
{
	public static void Postfix()
	{
		if (SpoofingService.IsAnyShuffleEnabled())
		{
			SpoofingService.ApplyIdentityShuffle();
		}
	}
}
