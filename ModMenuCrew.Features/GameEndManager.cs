using System;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using Il2CppSystem;
using InnerNet;
using UnityEngine;

namespace ModMenuCrew.Features;

public static class GameEndManager
{
	public static void ForceGameEnd(GameOverReason endReason)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Invalid comparison between Unknown and I4
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (IntegrityGuard.IsIntact)
		{
			if ((Object)(object)AmongUsClient.Instance == (Object)null)
			{
				Debug.LogWarning(Object.op_Implicit("[GameEndManager] AmongUsClient not available."));
				return;
			}
			if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				Debug.LogWarning(Object.op_Implicit("[GameEndManager] Only the host can force end the game."));
				return;
			}
			if ((Object)(object)GameManager.Instance == (Object)null)
			{
				Debug.LogWarning(Object.op_Implicit("[GameEndManager] GameManager not available."));
				return;
			}
			if ((int)((InnerNetClient)AmongUsClient.Instance).GameState == 3)
			{
				Debug.LogWarning(Object.op_Implicit("[GameEndManager] Game already ended."));
				return;
			}
			if ((Object)(object)ShipStatus.Instance == (Object)null)
			{
				Debug.LogWarning(Object.op_Implicit("[GameEndManager] Not in a game (ShipStatus null)."));
				return;
			}
			GameManager.Instance.RpcEndGame(endReason, false);
			Debug.Log(Object.op_Implicit($"[GameEndManager] Game ended: {endReason}"));
		}
	}

	public static void ForceImpostorWinByKill()
	{
		ForceGameEnd((GameOverReason)3);
	}

	public static void ForceImpostorWinBySabotage()
	{
		ForceGameEnd((GameOverReason)4);
	}

	public static void ForceImpostorWinByVote()
	{
		ForceGameEnd((GameOverReason)2);
	}

	public static void ForceCrewmateWinByVote()
	{
		ForceGameEnd((GameOverReason)0);
	}

	public static void ForceCrewmateWinByTask()
	{
		ForceGameEnd((GameOverReason)1);
	}

	public static void ForceWinByImpostorDisconnect()
	{
		ForceGameEnd((GameOverReason)5);
	}

	public static void ForceWinByCrewmateDisconnect()
	{
		ForceGameEnd((GameOverReason)6);
	}

	public static void ForceHnSCrewmateWin()
	{
		ForceGameEnd((GameOverReason)7);
	}

	public static void ForceHnSImpostorWin()
	{
		ForceGameEnd((GameOverReason)8);
	}

	public static void ForceMyTeamWin()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		object obj;
		if (localPlayer == null)
		{
			obj = null;
		}
		else
		{
			NetworkedPlayerInfo data = localPlayer.Data;
			obj = ((data != null) ? data.Role : null);
		}
		if ((Object)obj == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("[GameEndManager] Role not available."));
		}
		else
		{
			ForceSmartWin(PlayerControl.LocalPlayer.Data.Role.IsImpostor);
		}
	}

	public static void ForceEnemyTeamWin()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		object obj;
		if (localPlayer == null)
		{
			obj = null;
		}
		else
		{
			NetworkedPlayerInfo data = localPlayer.Data;
			obj = ((data != null) ? data.Role : null);
		}
		if ((Object)obj == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("[GameEndManager] Role not available."));
		}
		else
		{
			ForceSmartWin(!PlayerControl.LocalPlayer.Data.Role.IsImpostor);
		}
	}

	public static void ForceSmartWin(bool forImpostors)
	{
		bool flag = IsHideAndSeekMode();
		if (forImpostors)
		{
			if (flag)
			{
				ForceHnSImpostorWin();
			}
			else
			{
				ForceImpostorWinByKill();
			}
		}
		else if (flag)
		{
			ForceHnSCrewmateWin();
		}
		else
		{
			ForceCrewmateWinByTask();
		}
	}

	public static bool IsHideAndSeekMode()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		GameOptionsManager instance = GameOptionsManager.Instance;
		if (((instance != null) ? instance.CurrentGameOptions : null) == null)
		{
			return false;
		}
		GameModes gameMode = GameOptionsManager.Instance.CurrentGameOptions.GameMode;
		if ((int)gameMode != 2)
		{
			return (int)gameMode == 4;
		}
		return true;
	}

	public static bool CanEndGame()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return false;
		}
		if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			return false;
		}
		if ((Object)(object)GameManager.Instance == (Object)null)
		{
			return false;
		}
		if ((Object)(object)ShipStatus.Instance == (Object)null)
		{
			return false;
		}
		if ((int)((InnerNetClient)AmongUsClient.Instance).GameState == 3)
		{
			return false;
		}
		return true;
	}

	public static string GetCannotEndReason()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return "Client null";
		}
		if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			return "Not host";
		}
		if ((Object)(object)GameManager.Instance == (Object)null)
		{
			return "GameManager null";
		}
		if ((Object)(object)ShipStatus.Instance == (Object)null)
		{
			return "Not in match";
		}
		if ((int)((InnerNetClient)AmongUsClient.Instance).GameState == 3)
		{
			return "Game ended";
		}
		return "Ready";
	}

	public static string GetReasonName(GameOverReason reason)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected I4, but got Unknown
		return (int)reason switch
		{
			0 => "Crewmates (Vote)", 
			1 => "Crewmates (Tasks)", 
			2 => "Impostors (Vote)", 
			3 => "Impostors (Kill)", 
			4 => "Impostors (Sabotage)", 
			5 => "Impostor Disconnected", 
			6 => "Crewmate Disconnected", 
			7 => "Hiders Survived (Timer)", 
			8 => "Seeker Found All", 
			_ => ((object)(GameOverReason)(ref reason)).ToString(), 
		};
	}

	public static GameOverReason[] GetAvailableReasons()
	{
		if (IsHideAndSeekMode())
		{
			GameOverReason[] array = new GameOverReason[4];
			RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
			return (GameOverReason[])(object)array;
		}
		GameOverReason[] array2 = new GameOverReason[7];
		RuntimeHelpers.InitializeArray(array2, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		return (GameOverReason[])(object)array2;
	}

	public static bool DidHumansWin(GameOverReason reason)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		if ((int)reason != 1 && (int)reason != 0 && (int)reason != 5)
		{
			return (int)reason == 7;
		}
		return true;
	}

	public static bool DidImpostorsWin(GameOverReason reason)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if ((int)reason != 3 && (int)reason != 4 && (int)reason != 2 && (int)reason != 6)
		{
			return (int)reason == 8;
		}
		return true;
	}
}
