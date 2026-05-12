namespace ModMenuCrew.Messages;

public static class CustomRpcCalls
{
	public static byte BroadcastMessage
	{
		get
		{
			if (ServerData.Config.RpcBroadcastMsg <= 0)
			{
				return 201;
			}
			return ServerData.Config.RpcBroadcastMsg;
		}
	}

	public static byte MMCHandshake
	{
		get
		{
			if (ServerData.Config.RpcMmcHandshake <= 0)
			{
				return 202;
			}
			return ServerData.Config.RpcMmcHandshake;
		}
	}
}
