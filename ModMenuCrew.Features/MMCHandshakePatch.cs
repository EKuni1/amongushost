using HarmonyLib;
using Hazel;
using ModMenuCrew.Messages;

namespace ModMenuCrew.Features;

[HarmonyPatch(typeof(PlayerControl), "HandleRpc")]
public static class MMCHandshakePatch
{
	public static void Postfix(PlayerControl __instance, byte callId, MessageReader reader)
	{
		if (callId == CustomRpcCalls.MMCHandshake)
		{
			MMCIdentification.HandleHandshake(reader, __instance.PlayerId);
		}
	}
}
