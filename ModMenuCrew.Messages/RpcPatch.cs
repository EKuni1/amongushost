using HarmonyLib;
using Hazel;

namespace ModMenuCrew.Messages;

[HarmonyPatch(typeof(PlayerControl), "HandleRpc")]
public static class RpcPatch
{
	public static void Postfix(PlayerControl __instance, byte callId, MessageReader reader)
	{
		if (callId == CustomRpcCalls.BroadcastMessage)
		{
			CustomMessage.HandleBypass(reader, __instance.PlayerId);
		}
	}
}
