using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using ModMenuCrew.Features;
using ModMenuCrew.Networking;
using ModMenuCrew.UI.Managers;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew.UI.Menus;

public class PlayerPickMenu
{
	private static bool _showFilters = false;

	private static bool _filterImpostors = true;

	private static bool _filterCrewmates = true;

	private static byte? _openRoleDropdown = null;

	private static byte? _openPreAssignDropdown = null;

	private static Vector2 _roleDropdownScroll = Vector2.zero;

	private static readonly string[] ALL_ROLES = new string[10] { "Crewmate", "Engineer", "Scientist", "Noisemaker", "Tracker", "Detective", "Impostor", "Shapeshifter", "Phantom", "Viper" };

	private static readonly HashSet<string> PREMIUM_ROLES = new HashSet<string> { "Shapeshifter", "Phantom", "Viper" };

	private static readonly int[] _vanillaMaxImpByCount = new int[16]
	{
		0, 0, 0, 0, 1, 1, 1, 2, 2, 3,
		3, 3, 3, 3, 3, 3
	};

	private static readonly RoleTypes[] _crewRoleCandidates;

	private static readonly RoleTypes[] _impRoleCandidates;

	private static readonly List<RoleTypes> _rndPool;

	private static readonly List<PlayerControl> _rndPlayers;

	private static readonly List<string> _rndImpNames;

	private static readonly Random _rng;

	private static readonly Dictionary<byte, RoleTypes> _finalizePreserveBuf;

	private static readonly Dictionary<RoleTypes, int> _rndInvBuf;

	private static Dictionary<string, Action<long>> _actionRegistry;

	private ServerData.UISnapshot _safeSnapshot;

	private byte[] _cachedPPBytecode;

	private byte[] _cachedPPInverseMap;

	private long _cachedPPToken;

	private static readonly List<object> _playerDataBuffer;

	private static readonly Dictionary<int, string> _preAssignBuffer;

	private static int _lastPlayerHash;

	private static int _lastCheckedFrame;

	private static float _lastSendTime;

	private static float _lastWakeupTime;

	private static float _lastHashCheckTime;

	private static float _lastHashChangeSendTime;

	private const float HASH_CHANGE_DEBOUNCE = 0.15f;

	private const float SEND_THROTTLE_SECONDS = 0.05f;

	private const float HASH_CHECK_INTERVAL_WS_DEAD = 0.25f;

	private const float HEARTBEAT_WAKEUP_THROTTLE = 3f;

	private const float HEARTBEAT_WAKEUP_THROTTLE_PRIORITY = 0.5f;

	internal static volatile bool PendingImmediateHeartbeat;

	internal static void RegisterActions(Dictionary<string, Action<long>> registry)
	{
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		if (registry == null)
		{
			return;
		}
		RegisterPlayerAction(registry, "tp_", delegate(long token, byte playerId)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact)
			{
				PlayerControl playerById6 = GetPlayerById(playerId);
				if ((Object)(object)playerById6 != (Object)null && !playerById6.Data.IsDead && (Object)(object)PlayerControl.LocalPlayer != (Object)null)
				{
					PlayerControl.LocalPlayer.NetTransform.SnapTo(Vector2.op_Implicit(((Component)playerById6).transform.position));
				}
			}
		});
		RegisterPlayerAction(registry, "kill_", delegate(long token, byte playerId)
		{
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost && !((Object)(object)ShipStatus.Instance == (Object)null))
			{
				PlayerControl playerById5 = GetPlayerById(playerId);
				if ((Object)(object)playerById5 != (Object)null && !playerById5.Data.IsDead)
				{
					GameCheats.HostForceKillPlayer(playerById5);
				}
			}
		});
		RegisterPlayerAction(registry, "kick_", delegate(long token, byte playerId)
		{
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				PlayerControl playerById4 = GetPlayerById(playerId);
				if ((Object)(object)playerById4 != (Object)null && (Object)(object)playerById4 != (Object)(object)PlayerControl.LocalPlayer)
				{
					GameCheats.KickPlayer(playerById4);
				}
			}
		});
		RegisterPlayerAction(registry, "ban_", delegate(long token, byte playerId)
		{
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost && !((Object)(object)ShipStatus.Instance != (Object)null))
			{
				PlayerControl playerById3 = GetPlayerById(playerId);
				if ((Object)(object)playerById3 != (Object)null && (Object)(object)playerById3 != (Object)(object)PlayerControl.LocalPlayer)
				{
					GameCheats.KickPlayer(playerById3, ban: true);
				}
			}
		});
		RegisterPlayerAction(registry, "ol_", delegate(long token, byte playerId)
		{
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact)
			{
				GameCheats.ToggleSyncA(playerId);
			}
		});
		RegisterPlayerAction(registry, "lag_", delegate(long token, byte playerId)
		{
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact)
			{
				GameCheats.ToggleSyncB(playerId);
			}
		});
		RegisterPlayerAction(registry, "spec_", delegate(long token, byte playerId)
		{
			if (GhostUI.CheckToken(token))
			{
				GameCheats.ToggleSpec(playerId);
			}
		});
		registry["ch_host"] = delegate(long token)
		{
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact)
			{
				GameCheats.ToggleCH();
			}
		};
		registry["cl_all"] = delegate(long token)
		{
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact)
			{
				GameCheats.ToggleCL();
			}
		};
		registry["ht_troll"] = delegate
		{
			GameCheats.ToggleFakeCountdown();
			TriggerRealtimeUpdate(force: true);
		};
		registry["toggle_filters"] = delegate
		{
			_showFilters = !_showFilters;
			TriggerRealtimeUpdate(force: true);
		};
		registry["filter_impostors"] = delegate
		{
			_filterImpostors = !_filterImpostors;
			TriggerRealtimeUpdate(force: true);
		};
		registry["filter_crewmates"] = delegate
		{
			_filterCrewmates = !_filterCrewmates;
			TriggerRealtimeUpdate(force: true);
		};
		RegisterPlayerAction(registry, "role_open_", delegate(long token, byte playerId)
		{
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				_openRoleDropdown = ((_openRoleDropdown == playerId) ? null : new byte?(playerId));
				_openPreAssignDropdown = null;
				_roleDropdownScroll = Vector2.zero;
				TriggerRealtimeUpdate(force: true);
			}
		});
		registry["role_close"] = delegate
		{
			_openRoleDropdown = null;
			TriggerRealtimeUpdate(force: true);
		};
		string[] aLL_ROLES = ALL_ROLES;
		foreach (string text in aLL_ROLES)
		{
			string capturedRole = text;
			for (byte b = 0; b <= 15; b++)
			{
				byte playerId4 = b;
				registry[$"role_set_{capturedRole}_{playerId4}"] = delegate(long token)
				{
					//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
					//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
					//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
					//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
					//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
					if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost)
					{
						PlayerControl playerById2 = GetPlayerById(playerId4);
						if (!((Object)(object)playerById2 == (Object)null))
						{
							string value = capturedRole;
							if (PREMIUM_ROLES.Contains(capturedRole) && capturedRole != ServerData.Config.Za && (!ModKeyValidator.IsPremium || !ModKeyValidator.V()))
							{
								NotifyUtils.Warning("★ " + capturedRole + " is PREMIUM ONLY! Using Impostor.");
								value = "Impostor";
							}
							try
							{
								RoleTypes val2 = (RoleTypes)Enum.Parse(typeof(RoleTypes), value);
								playerById2.RpcSetRole(val2, true);
								if (IsImpostorRole(val2))
								{
									playerById2.Data.RpcSetTasks(Il2CppStructArray<byte>.op_Implicit(Array.Empty<byte>()));
								}
								ImpostorForcer.UpdateRoleLocally(playerById2, val2);
							}
							catch
							{
							}
							_openRoleDropdown = null;
							TriggerRealtimeUpdate(force: true);
						}
					}
				};
			}
		}
		RegisterPlayerAction(registry, "preassign_open_", delegate(long token, byte playerId)
		{
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost && !((Object)(object)ShipStatus.Instance != (Object)null))
			{
				_openPreAssignDropdown = ((_openPreAssignDropdown == playerId) ? null : new byte?(playerId));
				_openRoleDropdown = null;
				TriggerRealtimeUpdate(force: true);
			}
		});
		registry["preassign_close"] = delegate
		{
			_openPreAssignDropdown = null;
			TriggerRealtimeUpdate(force: true);
		};
		for (byte b2 = 0; b2 <= 15; b2++)
		{
			byte playerId3 = b2;
			registry[$"preassign_clear_{playerId3}"] = delegate(long token)
			{
				if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost)
				{
					ImpostorForcer.ClearPreGameRoleForPlayer(playerId3);
					_openPreAssignDropdown = null;
					TriggerRealtimeUpdate(force: true);
				}
			};
		}
		registry["preassign_clear_all"] = delegate(long token)
		{
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				ImpostorForcer.ClearAllPreGameRoleAssignments();
				_openPreAssignDropdown = null;
				TriggerRealtimeUpdate(force: true);
			}
		};
		registry["randomize_roles"] = delegate(long token)
		{
			if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost && !((Object)(object)ShipStatus.Instance != (Object)null))
			{
				RandomizeAllPlayerRoles();
				_openPreAssignDropdown = null;
				TriggerRealtimeUpdate(force: true);
			}
		};
		RoleTypes[] supportedRoles = ImpostorForcer.GetSupportedRoles();
		foreach (RoleTypes val in supportedRoles)
		{
			RoleTypes capturedRoleType = val;
			for (byte b3 = 0; b3 <= 15; b3++)
			{
				byte playerId2 = b3;
				registry[$"preassign_set_{capturedRoleType}_{playerId2}"] = delegate(long token)
				{
					//IL_003f: Unknown result type (might be due to invalid IL or missing references)
					//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
					//IL_0077: Unknown result type (might be due to invalid IL or missing references)
					if (GhostUI.CheckToken(token) && IntegrityGuard.IsIntact && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost && !((Object)(object)ShipStatus.Instance != (Object)null))
					{
						if (ImpostorForcer.IsRolePremiumLocked(capturedRoleType) && (!ModKeyValidator.IsPremium || !ModKeyValidator.V()))
						{
							NotifyUtils.Warning($"★ {capturedRoleType} is PREMIUM ONLY!");
						}
						else
						{
							PlayerControl playerById = GetPlayerById(playerId2);
							if ((Object)(object)playerById != (Object)null)
							{
								ImpostorForcer.SetPreGameRoleForPlayer(playerById, capturedRoleType, isManual: true);
							}
							_openPreAssignDropdown = null;
							TriggerRealtimeUpdate(force: true);
						}
					}
				};
			}
		}
	}

	private static bool IsImpostorRole(RoleTypes role)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)role != 1 && (int)role != 5 && (int)role != 9)
		{
			return (int)role == 18;
		}
		return true;
	}

	internal static void RandomizeAllPlayerRolesSilent()
	{
		try
		{
			if (!((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost && !((Object)(object)ShipStatus.Instance != (Object)null))
			{
				RandomizeAllPlayerRoles(silent: true);
			}
		}
		catch
		{
		}
	}

	internal static void RebalanceAfterManualSet(byte newManualId, RoleTypes newRole)
	{
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Invalid comparison between Unknown and I4
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost || (Object)(object)ShipStatus.Instance != (Object)null || PlayerControl.AllPlayerControls == null)
			{
				return;
			}
			GameOptionsManager instance = GameOptionsManager.Instance;
			IGameOptions val = ((instance != null) ? instance.CurrentGameOptions : null);
			if (val == null)
			{
				return;
			}
			bool flag = false;
			if ((int)newRole != 0 && (int)newRole != 1)
			{
				int num = -1;
				try
				{
					num = val.RoleOptions.GetNumPerGame(newRole);
				}
				catch
				{
					num = -1;
				}
				if (num >= 0)
				{
					int num2 = 0;
					foreach (KeyValuePair<byte, RoleTypes> preGameRoleAssignment in ImpostorForcer.PreGameRoleAssignments)
					{
						if (preGameRoleAssignment.Value == newRole)
						{
							num2++;
						}
					}
					int num3 = num2 - num;
					if (num3 > 0)
					{
						List<byte> list = new List<byte>(ImpostorForcer.PreGameRoleAssignments.Keys);
						for (int i = 0; i < list.Count; i++)
						{
							if (num3 <= 0)
							{
								break;
							}
							byte b = list[i];
							if (b != newManualId && !ImpostorForcer.ManuallyAssigned.Contains(b) && ImpostorForcer.PreGameRoleAssignments.TryGetValue(b, out var value) && value == newRole)
							{
								ImpostorForcer.ClearPreGameRoleForPlayer(b);
								num3--;
								flag = true;
							}
						}
					}
				}
			}
			if (IsImpostorRole(newRole))
			{
				int num4 = 0;
				Enumerator<PlayerControl> enumerator2 = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					PlayerControl current = enumerator2.Current;
					if (!((Object)(object)current == (Object)null) && !((Object)(object)current.Data == (Object)null) && !current.Data.Disconnected)
					{
						num4++;
					}
				}
				int num5 = _vanillaMaxImpByCount[Mathf.Clamp(num4, 0, _vanillaMaxImpByCount.Length - 1)];
				int num6 = Mathf.Clamp(val.NumImpostors, 1, num5);
				int num7 = 0;
				foreach (KeyValuePair<byte, RoleTypes> preGameRoleAssignment2 in ImpostorForcer.PreGameRoleAssignments)
				{
					if (IsImpostorRole(preGameRoleAssignment2.Value))
					{
						num7++;
					}
				}
				int num8 = num7 - num6;
				if (num8 > 0)
				{
					List<byte> list2 = new List<byte>(ImpostorForcer.PreGameRoleAssignments.Keys);
					for (int j = 0; j < list2.Count; j++)
					{
						if (num8 <= 0)
						{
							break;
						}
						byte b2 = list2[j];
						if (b2 != newManualId && !ImpostorForcer.ManuallyAssigned.Contains(b2) && ImpostorForcer.PreGameRoleAssignments.TryGetValue(b2, out var value2) && IsImpostorRole(value2))
						{
							ImpostorForcer.ClearPreGameRoleForPlayer(b2);
							num8--;
							flag = true;
						}
					}
				}
				int num9 = 0;
				foreach (KeyValuePair<byte, RoleTypes> preGameRoleAssignment3 in ImpostorForcer.PreGameRoleAssignments)
				{
					if (IsImpostorRole(preGameRoleAssignment3.Value))
					{
						num9++;
					}
				}
				if (num9 > num5)
				{
					try
					{
						NotifyUtils.Warning($"Imp manual exceeds vanilla cap ({num9}/{num5}) — extras dropped at game start.");
					}
					catch
					{
					}
				}
			}
			if (!flag)
			{
				return;
			}
			AssignRolesForNewPlayersSilent();
			try
			{
				TriggerRealtimeUpdate(force: true);
			}
			catch
			{
			}
		}
		catch (Exception value3)
		{
			Debug.LogError(Object.op_Implicit($"[PlayerPickMenu] RebalanceAfterManualSet error: {value3}"));
		}
	}

	internal static void FinalizeRolesBeforeGameStart()
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost || PlayerControl.AllPlayerControls == null)
			{
				return;
			}
			_rndPlayers.Clear();
			int num = 0;
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && !((Object)(object)current.Data == (Object)null) && !current.Data.Disconnected)
				{
					num++;
					_rndPlayers.Add(current);
				}
			}
			if (num < 4)
			{
				return;
			}
			GameOptionsManager instance = GameOptionsManager.Instance;
			IGameOptions val = ((instance != null) ? instance.CurrentGameOptions : null);
			if (val == null)
			{
				return;
			}
			_finalizePreserveBuf.Clear();
			foreach (KeyValuePair<byte, RoleTypes> preGameRoleAssignment in ImpostorForcer.PreGameRoleAssignments)
			{
				if (ImpostorForcer.ManuallyAssigned.Contains(preGameRoleAssignment.Key))
				{
					_finalizePreserveBuf[preGameRoleAssignment.Key] = preGameRoleAssignment.Value;
				}
			}
			if (ImpostorForcer.RoleOverrideEnabled && (Object)(object)PlayerControl.LocalPlayer != (Object)null && !_finalizePreserveBuf.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
			{
				_finalizePreserveBuf[PlayerControl.LocalPlayer.PlayerId] = ImpostorForcer.SelectedRoleForHost;
			}
			int num2 = _vanillaMaxImpByCount[Mathf.Clamp(num, 0, _vanillaMaxImpByCount.Length - 1)];
			int num3 = Mathf.Clamp(val.NumImpostors, 1, num2);
			int num4 = 0;
			foreach (KeyValuePair<byte, RoleTypes> item in _finalizePreserveBuf)
			{
				if (IsImpostorRole(item.Value))
				{
					num4++;
				}
			}
			if (num4 > num2)
			{
				int num5 = num4 - num2;
				List<byte> list = new List<byte>(_finalizePreserveBuf.Keys);
				byte localId = (((Object)(object)PlayerControl.LocalPlayer != (Object)null) ? PlayerControl.LocalPlayer.PlayerId : byte.MaxValue);
				list.Sort(delegate(byte a, byte b)
				{
					if (a == localId)
					{
						return 1;
					}
					return (b == localId) ? (-1) : a.CompareTo(b);
				});
				list.Reverse();
				for (int i = 0; i < list.Count; i++)
				{
					if (num5 <= 0)
					{
						break;
					}
					byte b2 = list[i];
					if ((b2 != localId || !ImpostorForcer.RoleOverrideEnabled) && IsImpostorRole(_finalizePreserveBuf[b2]))
					{
						_finalizePreserveBuf[b2] = (RoleTypes)0;
						num5--;
					}
				}
			}
			ImpostorForcer.ClearAllPreGameRoleAssignments();
			foreach (KeyValuePair<byte, RoleTypes> item2 in _finalizePreserveBuf)
			{
				ImpostorForcer.PreGameRoleAssignments[item2.Key] = item2.Value;
				if (ImpostorForcer.ManuallyAssigned != null)
				{
					ImpostorForcer.ManuallyAssigned.Add(item2.Key);
				}
			}
			_rndPlayers.RemoveAll((PlayerControl p) => (Object)(object)p == (Object)null || _finalizePreserveBuf.ContainsKey(p.PlayerId));
			if (_rndPlayers.Count == 0)
			{
				return;
			}
			int num6 = 0;
			foreach (KeyValuePair<byte, RoleTypes> item3 in _finalizePreserveBuf)
			{
				if (IsImpostorRole(item3.Value))
				{
					num6++;
				}
			}
			int num7 = Mathf.Max(0, num3 - num6);
			num7 = Mathf.Min(num7, _rndPlayers.Count);
			bool premium = ModKeyValidator.IsPremium && ModKeyValidator.V();
			for (int num8 = _rndPlayers.Count - 1; num8 > 0; num8--)
			{
				int index = _rng.Next(num8 + 1);
				PlayerControl value = _rndPlayers[num8];
				_rndPlayers[num8] = _rndPlayers[index];
				_rndPlayers[index] = value;
			}
			Dictionary<RoleTypes, int> remaining = new Dictionary<RoleTypes, int>(BuildRemainingInventory(val.RoleOptions, _impRoleCandidates));
			Dictionary<RoleTypes, int> remaining2 = new Dictionary<RoleTypes, int>(BuildRemainingInventory(val.RoleOptions, _crewRoleCandidates));
			AssignTeamByGameRules(val.RoleOptions, _impRoleCandidates, (RoleTypes)1, 0, num7, premium, remaining);
			AssignTeamByGameRules(val.RoleOptions, _crewRoleCandidates, (RoleTypes)0, num7, _rndPlayers.Count, premium, remaining2);
			if (num4 <= num2)
			{
				return;
			}
			try
			{
				NotifyUtils.Warning($"Imp manual cap: {num4}>{num2}, trimmed.");
			}
			catch
			{
			}
		}
		catch (Exception value2)
		{
			Debug.LogError(Object.op_Implicit($"[PlayerPickMenu] FinalizeRolesBeforeGameStart error: {value2}"));
		}
	}

	internal static void AssignRolesForNewPlayersSilent()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost || (Object)(object)ShipStatus.Instance != (Object)null || PlayerControl.AllPlayerControls == null)
			{
				return;
			}
			if (ImpostorForcer.RoleOverrideEnabled && (Object)(object)PlayerControl.LocalPlayer != (Object)null && !ImpostorForcer.PreGameRoleAssignments.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
			{
				byte playerId = PlayerControl.LocalPlayer.PlayerId;
				ImpostorForcer.PreGameRoleAssignments[playerId] = ImpostorForcer.SelectedRoleForHost;
				ImpostorForcer.ManuallyAssigned.Add(playerId);
			}
			_rndPlayers.Clear();
			int num = 0;
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && !((Object)(object)current.Data == (Object)null) && !current.Data.Disconnected)
				{
					num++;
					if (!ImpostorForcer.PreGameRoleAssignments.ContainsKey(current.PlayerId))
					{
						_rndPlayers.Add(current);
					}
				}
			}
			if (_rndPlayers.Count == 0 || num < 4)
			{
				return;
			}
			GameOptionsManager instance = GameOptionsManager.Instance;
			IGameOptions val = ((instance != null) ? instance.CurrentGameOptions : null);
			if (val == null)
			{
				return;
			}
			int num2 = _vanillaMaxImpByCount[Mathf.Clamp(num, 0, _vanillaMaxImpByCount.Length - 1)];
			int num3 = Mathf.Clamp(val.NumImpostors, 1, num2);
			int num4 = 0;
			foreach (KeyValuePair<byte, RoleTypes> preGameRoleAssignment in ImpostorForcer.PreGameRoleAssignments)
			{
				if (IsImpostorRole(preGameRoleAssignment.Value))
				{
					num4++;
				}
			}
			int num5 = Mathf.Max(0, num3 - num4);
			num5 = Mathf.Min(num5, _rndPlayers.Count);
			bool premium = ModKeyValidator.IsPremium && ModKeyValidator.V();
			for (int num6 = _rndPlayers.Count - 1; num6 > 0; num6--)
			{
				int index = _rng.Next(num6 + 1);
				PlayerControl value = _rndPlayers[num6];
				_rndPlayers[num6] = _rndPlayers[index];
				_rndPlayers[index] = value;
			}
			Dictionary<RoleTypes, int> remaining = new Dictionary<RoleTypes, int>(BuildRemainingInventory(val.RoleOptions, _impRoleCandidates));
			Dictionary<RoleTypes, int> remaining2 = new Dictionary<RoleTypes, int>(BuildRemainingInventory(val.RoleOptions, _crewRoleCandidates));
			AssignTeamByGameRules(val.RoleOptions, _impRoleCandidates, (RoleTypes)1, 0, num5, premium, remaining);
			AssignTeamByGameRules(val.RoleOptions, _crewRoleCandidates, (RoleTypes)0, num5, _rndPlayers.Count, premium, remaining2);
		}
		catch (Exception value2)
		{
			Debug.LogError(Object.op_Implicit($"[PlayerPickMenu] AssignRolesForNewPlayersSilent error: {value2}"));
		}
	}

	private static void RandomizeAllPlayerRoles(bool silent = false)
	{
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (PlayerControl.AllPlayerControls == null)
			{
				return;
			}
			_rndPlayers.Clear();
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current != (Object)null && (Object)(object)current.Data != (Object)null && !current.Data.Disconnected)
				{
					_rndPlayers.Add(current);
				}
			}
			int count = _rndPlayers.Count;
			if (count < 4)
			{
				if (!silent)
				{
					NotifyUtils.Warning("Need at least 4 players to randomize roles.");
				}
				return;
			}
			GameOptionsManager instance = GameOptionsManager.Instance;
			IGameOptions val = ((instance != null) ? instance.CurrentGameOptions : null);
			if (val == null)
			{
				if (!silent)
				{
					NotifyUtils.Warning("Game options not loaded — cannot match game rules.");
				}
				return;
			}
			ImpostorForcer.ClearAllPreGameRoleAssignments();
			int num = _vanillaMaxImpByCount[Mathf.Clamp(count, 0, _vanillaMaxImpByCount.Length - 1)];
			int num2 = Mathf.Clamp(val.NumImpostors, 1, num);
			bool premium = ModKeyValidator.IsPremium && ModKeyValidator.V();
			for (int num3 = count - 1; num3 > 0; num3--)
			{
				int index = _rng.Next(num3 + 1);
				PlayerControl value = _rndPlayers[num3];
				_rndPlayers[num3] = _rndPlayers[index];
				_rndPlayers[index] = value;
			}
			Dictionary<RoleTypes, int> remaining = new Dictionary<RoleTypes, int>(BuildRemainingInventory(val.RoleOptions, _impRoleCandidates));
			Dictionary<RoleTypes, int> remaining2 = new Dictionary<RoleTypes, int>(BuildRemainingInventory(val.RoleOptions, _crewRoleCandidates));
			AssignTeamByGameRules(val.RoleOptions, _impRoleCandidates, (RoleTypes)1, 0, num2, premium, remaining);
			AssignTeamByGameRules(val.RoleOptions, _crewRoleCandidates, (RoleTypes)0, num2, count, premium, remaining2);
			_rndImpNames.Clear();
			foreach (KeyValuePair<byte, RoleTypes> preGameRoleAssignment in ImpostorForcer.PreGameRoleAssignments)
			{
				PlayerControl playerById = GetPlayerById(preGameRoleAssignment.Key);
				if ((Object)(object)playerById != (Object)null && IsImpostorRole(preGameRoleAssignment.Value))
				{
					_rndImpNames.Add(playerById.Data.PlayerName ?? "???");
				}
			}
			if (!silent)
			{
				NotifyUtils.Success($"Randomized (game rules)! {num2} imp(s): {string.Join(", ", _rndImpNames)}");
			}
		}
		catch (Exception value2)
		{
			Debug.LogError(Object.op_Implicit($"[PlayerPickMenu] RandomizeAllPlayerRoles error: {value2}"));
		}
	}

	private static Dictionary<RoleTypes, int> BuildRemainingInventory(IRoleOptionsCollection roleOpts, RoleTypes[] candidates)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		_rndInvBuf.Clear();
		foreach (RoleTypes val in candidates)
		{
			int numPerGame;
			try
			{
				numPerGame = roleOpts.GetNumPerGame(val);
			}
			catch
			{
				continue;
			}
			if (numPerGame > 0)
			{
				_rndInvBuf[val] = numPerGame;
			}
		}
		foreach (KeyValuePair<byte, RoleTypes> preGameRoleAssignment in ImpostorForcer.PreGameRoleAssignments)
		{
			if (_rndInvBuf.TryGetValue(preGameRoleAssignment.Value, out var value) && value > 0)
			{
				_rndInvBuf[preGameRoleAssignment.Value] = value - 1;
			}
		}
		return _rndInvBuf;
	}

	private static void AssignTeamByGameRules(IRoleOptionsCollection roleOpts, RoleTypes[] candidates, RoleTypes defaultRole, int startIdx, int endIdx, bool premium, Dictionary<RoleTypes, int> remaining)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		int num = endIdx - startIdx;
		if (num <= 0)
		{
			return;
		}
		_rndPool.Clear();
		RoleTypes[] array = candidates;
		foreach (RoleTypes val in array)
		{
			int chancePerGame;
			try
			{
				chancePerGame = roleOpts.GetChancePerGame(val);
			}
			catch
			{
				continue;
			}
			if (chancePerGame == 100)
			{
				int value;
				int num2 = (remaining.TryGetValue(val, out value) ? value : 0);
				for (int j = 0; j < num2; j++)
				{
					_rndPool.Add(val);
				}
			}
		}
		array = candidates;
		foreach (RoleTypes val2 in array)
		{
			int chancePerGame2;
			try
			{
				chancePerGame2 = roleOpts.GetChancePerGame(val2);
			}
			catch
			{
				continue;
			}
			if (chancePerGame2 <= 0 || chancePerGame2 >= 100)
			{
				continue;
			}
			int value2;
			int num3 = (remaining.TryGetValue(val2, out value2) ? value2 : 0);
			for (int k = 0; k < num3; k++)
			{
				if (_rng.Next(101) < chancePerGame2)
				{
					_rndPool.Add(val2);
				}
			}
		}
		while (_rndPool.Count > num)
		{
			_rndPool.RemoveAt(_rng.Next(_rndPool.Count));
		}
		while (_rndPool.Count < num)
		{
			_rndPool.Add(defaultRole);
		}
		for (int l = startIdx; l < endIdx; l++)
		{
			int index = _rng.Next(_rndPool.Count);
			RoleTypes val3 = _rndPool[index];
			_rndPool.RemoveAt(index);
			if (!premium && ImpostorForcer.IsRolePremiumLocked(val3))
			{
				val3 = defaultRole;
			}
			ImpostorForcer.SetPreGameRoleForPlayer(_rndPlayers[l], val3);
			if (remaining.TryGetValue(val3, out var value3) && value3 > 0)
			{
				remaining[val3] = value3 - 1;
			}
		}
	}

	private static void RegisterPlayerAction(Dictionary<string, Action<long>> registry, string prefix, Action<long, byte> handler)
	{
		for (byte b = 0; b <= 15; b++)
		{
			string key = prefix + b;
			byte playerId = b;
			registry[key] = delegate(long token)
			{
				handler(token, playerId);
			};
		}
	}

	private static PlayerControl GetPlayerById(byte playerId)
	{
		if (PlayerControl.AllPlayerControls == null)
		{
			return null;
		}
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && (Object)(object)current.Data != (Object)null && current.PlayerId == playerId && !current.Data.Disconnected)
			{
				return current;
			}
		}
		return null;
	}

	private static Dictionary<string, Action<long>> GetActionRegistry()
	{
		if (_actionRegistry == null)
		{
			_actionRegistry = new Dictionary<string, Action<long>>();
			RegisterActions(_actionRegistry);
		}
		return _actionRegistry;
	}

	public void Draw()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)Event.current.type == 8)
		{
			CheckRealtimeUpdate();
			_safeSnapshot = ServerData.CurrentSnapshot;
		}
		ServerData.UISnapshot safeSnapshot = _safeSnapshot;
		if (safeSnapshot != null && safeSnapshot.PlayerPickBytecode != null && safeSnapshot.PlayerPickBytecode.Length != 0)
		{
			if (_cachedPPBytecode != safeSnapshot.PlayerPickBytecode)
			{
				_cachedPPBytecode = safeSnapshot.PlayerPickBytecode;
				if (safeSnapshot.PlayerPickBytecode.Length >= 524)
				{
					if (_cachedPPInverseMap == null)
					{
						_cachedPPInverseMap = new byte[256];
					}
					Array.Copy(safeSnapshot.PlayerPickBytecode, 268, _cachedPPInverseMap, 0, 256);
				}
				else
				{
					_cachedPPInverseMap = null;
				}
				_cachedPPToken = ((safeSnapshot.PlayerPickBytecode.Length >= 268) ? BitConverter.ToInt64(safeSnapshot.PlayerPickBytecode, 260) : safeSnapshot.SessionToken);
			}
			GhostUI.Execute(safeSnapshot.PlayerPickBytecode, _cachedPPToken, GetActionRegistry(), _cachedPPInverseMap);
		}
		else
		{
			GUILayout.BeginVertical(GuiStyles.SectionStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GuiStyles.CachedExpandWidth,
				GuiStyles.CachedExpandHeight
			});
			GUILayout.Label("<size=14><b>PLAYER SELECTION</b></size>", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Space(16f);
			GUILayout.Label("<color=#949EAD>Loading from server...</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.EndVertical();
		}
	}

	internal static object[] CollectPlayerDataForServer()
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected I4, but got Unknown
		try
		{
			if (PlayerControl.AllPlayerControls == null)
			{
				return Array.Empty<object>();
			}
			_playerDataBuffer.Clear();
			List<object> playerDataBuffer = _playerDataBuffer;
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current == (Object)null || (Object)(object)current.Data == (Object)null)
				{
					continue;
				}
				int num = 0;
				int num2 = 0;
				List<TaskInfo> tasks = current.Data.Tasks;
				if (tasks != null)
				{
					num2 = tasks.Count;
					for (int i = 0; i < num2; i++)
					{
						if (tasks[i].Complete)
						{
							num++;
						}
					}
				}
				int platformId = 0;
				string platformName = "";
				uint level = 0u;
				int clientId = ((InnerNetObject)current).OwnerId;
				string friendCode = "";
				try
				{
					AmongUsClient instance = AmongUsClient.Instance;
					ClientData val = ((instance != null) ? ((InnerNetClient)instance).GetClient(((InnerNetObject)current).OwnerId) : null);
					if (val != null)
					{
						clientId = val.Id;
						friendCode = val.FriendCode ?? "";
						if (val.PlatformData != null)
						{
							platformId = (int)val.PlatformData.Platform;
							platformName = val.PlatformData.PlatformName ?? "";
						}
					}
					uint num3 = uint.MaxValue;
					try
					{
						GameData instance2 = GameData.Instance;
						NetworkedPlayerInfo val2 = ((instance2 != null) ? instance2.GetPlayerById(current.PlayerId) : null);
						if ((Object)(object)val2 != (Object)null)
						{
							num3 = val2.PlayerLevel;
						}
					}
					catch
					{
					}
					level = ((num3 != 0 && num3 != uint.MaxValue) ? (num3 + 1) : 0u);
				}
				catch
				{
				}
				byte playerId = current.PlayerId;
				string name = current.Data.PlayerName ?? "???";
				PlayerOutfit defaultOutfit = current.Data.DefaultOutfit;
				int colorId = ((defaultOutfit != null) ? defaultOutfit.ColorId : 0);
				RoleBehaviour role = current.Data.Role;
				playerDataBuffer.Add(new
				{
					id = playerId,
					name = name,
					colorId = colorId,
					isImpostor = (role != null && role.IsImpostor),
					isDead = current.Data.IsDead,
					isDisconnected = current.Data.Disconnected,
					tasksCompleted = num,
					tasksTotal = num2,
					isHost = ((Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetObject)current).OwnerId == ((InnerNetClient)AmongUsClient.Instance).HostId),
					platformId = platformId,
					platformName = platformName,
					level = level,
					clientId = clientId,
					friendCode = friendCode
				});
			}
			return playerDataBuffer.ToArray();
		}
		catch
		{
			return Array.Empty<object>();
		}
	}

	internal static (bool isHost, bool isInGame, byte localPlayerId) GetGameContext()
	{
		try
		{
			bool item = (Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmHost;
			bool item2 = (Object)(object)ShipStatus.Instance != (Object)null;
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			byte item3 = ((localPlayer != null) ? localPlayer.PlayerId : byte.MaxValue);
			return (isHost: item, isInGame: item2, localPlayerId: item3);
		}
		catch
		{
			return (isHost: false, isInGame: false, localPlayerId: byte.MaxValue);
		}
	}

	internal static object GetUIState()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			_preAssignBuffer.Clear();
			RoleTypes val;
			foreach (KeyValuePair<byte, RoleTypes> preGameRoleAssignment in ImpostorForcer.PreGameRoleAssignments)
			{
				Dictionary<int, string> preAssignBuffer = _preAssignBuffer;
				byte key = preGameRoleAssignment.Key;
				val = preGameRoleAssignment.Value;
				preAssignBuffer[key] = val.ToString();
			}
			Dictionary<int, string> preAssignBuffer2 = _preAssignBuffer;
			Dictionary<byte, float> syncATargets = GameCheats.GetSyncATargets();
			Dictionary<int, float> dictionary = new Dictionary<int, float>();
			foreach (KeyValuePair<byte, float> item in syncATargets)
			{
				dictionary[item.Key] = item.Value;
			}
			HashSet<byte> syncBTargets = GameCheats.GetSyncBTargets();
			List<int> list = new List<int>();
			foreach (byte item2 in syncBTargets)
			{
				list.Add(item2);
			}
			bool showFilters = _showFilters;
			bool filterImpostors = _filterImpostors;
			bool filterCrewmates = _filterCrewmates;
			int? openRoleDropdown = (_openRoleDropdown.HasValue ? new int?(_openRoleDropdown.Value) : null);
			int? openPreAssignDropdown = (_openPreAssignDropdown.HasValue ? new int?(_openPreAssignDropdown.Value) : null);
			bool isAnyOverrideActive = ImpostorForcer.IsAnyOverrideActive;
			val = ImpostorForcer.SelectedRoleForHost;
			return new
			{
				showFilters = showFilters,
				filterImpostors = filterImpostors,
				filterCrewmates = filterCrewmates,
				openRoleDropdown = openRoleDropdown,
				openPreAssignDropdown = openPreAssignDropdown,
				hostRoleOverrideEnabled = isAnyOverrideActive,
				hostRoleOverrideRole = val.ToString(),
				preAssignments = preAssignBuffer2,
				isPremium = ModKeyValidator.IsPremium,
				sA = dictionary,
				sBA = GameCheats.IsSyncBAll(),
				sBR = GameCheats.GetSyncBAllRemaining(),
				sB = list,
				chA = GameCheats.IsCHActive(),
				chR = GameCheats.GetCHRemaining(),
				clA = GameCheats.IsCLActive(),
				clR = GameCheats.GetCLRemaining(),
				htA = GameCheats.IsFakeCdActive(),
				htR = GameCheats.GetFakeCdCurrent(),
				sSpec = (int)GameCheats._specTarget,
				arS = GameCheats.GetAREngineState(),
				arP = GameCheats.GetAREnginePingBucket()
			};
		}
		catch
		{
			return new
			{
				showFilters = _showFilters,
				filterImpostors = _filterImpostors,
				filterCrewmates = _filterCrewmates,
				openRoleDropdown = (int?)null,
				openPreAssignDropdown = (int?)null,
				hostRoleOverrideEnabled = false,
				hostRoleOverrideRole = "Impostor",
				preAssignments = new Dictionary<int, string>(),
				isPremium = false,
				sA = new Dictionary<int, float>(),
				sBA = false,
				sBR = 0f,
				sB = new List<int>(),
				chA = false,
				chR = 0f,
				clA = false,
				clR = 0f,
				htA = false,
				htR = 0,
				sSpec = 255,
				arS = (byte)0,
				arP = 0
			};
		}
	}

	internal static void ResetHashForReconnect()
	{
		_lastPlayerHash = 0;
	}

	internal static void CheckRealtimeUpdate()
	{
		int frameCount = Time.frameCount;
		if (frameCount == _lastCheckedFrame)
		{
			return;
		}
		_lastCheckedFrame = frameCount;
		if (!RealtimeConnection.IsConnected)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (realtimeSinceStartup - _lastHashCheckTime < 0.25f)
			{
				return;
			}
			_lastHashCheckTime = realtimeSinceStartup;
		}
		int num = CalculatePlayerHash();
		if (num != _lastPlayerHash)
		{
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
			if (!(realtimeSinceStartup2 - _lastHashChangeSendTime < 0.15f))
			{
				_lastHashChangeSendTime = realtimeSinceStartup2;
				_lastPlayerHash = num;
				TriggerRealtimeUpdate();
			}
		}
	}

	internal static void TriggerRealtimeUpdate(bool force = false)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!force && realtimeSinceStartup - _lastSendTime < 0.05f)
		{
			_lastPlayerHash = 0;
			return;
		}
		if (force)
		{
			_lastPlayerHash = CalculatePlayerHash();
		}
		_lastSendTime = realtimeSinceStartup;
		try
		{
			if (!RealtimeConnection.IsConnected)
			{
				PendingImmediateHeartbeat = true;
				float num = (force ? 0.5f : 3f);
				if (realtimeSinceStartup - _lastWakeupTime >= num)
				{
					_lastWakeupTime = realtimeSinceStartup;
					ModKeyValidator.ForceHeartbeatWakeup();
				}
				return;
			}
			object[] playerData = CollectPlayerDataForServer();
			object uIState = GetUIState();
			object banMenuState = BanMenu.GetBanMenuState();
			object uIState2 = BanMenu.GetUIState();
			object cheatsState = CheatManager.GetCheatsState();
			object cheatsUiState = CheatManager.GetCheatsUiState();
			List<object> alivePlayersForServer = CheatManager.GetAlivePlayersForServer();
			List<object> allPlayersForImpersonateServer = CheatManager.GetAllPlayersForImpersonateServer();
			object spoofingState = SpoofingMenu.GetSpoofingState();
			object settingsState = SettingsTab.GetSettingsState();
			object sabotageState = null;
			try
			{
				sabotageState = CheatManager.GetSabotageState();
			}
			catch
			{
			}
			RealtimeConnection.SendUpdate(playerData, uIState, banMenuState, uIState2, cheatsState, cheatsUiState, alivePlayersForServer, spoofingState, settingsState, sabotageState, allPlayersForImpersonateServer, force);
		}
		catch
		{
		}
	}

	private static int CalculatePlayerHash()
	{
		//IL_062e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0633: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Expected I4, but got Unknown
		//IL_0718: Unknown result type (might be due to invalid IL or missing references)
		//IL_071d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected I4, but got Unknown
		try
		{
			if (PlayerControl.AllPlayerControls == null)
			{
				return 0;
			}
			int num = 17;
			int num2 = 0;
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current == (Object)null || (Object)(object)current.Data == (Object)null)
				{
					continue;
				}
				num2++;
				num = num * 31 + current.PlayerId;
				num = num * 31 + (current.Data.IsDead ? 1 : 0);
				num = num * 31 + (current.Data.Disconnected ? 1 : 0);
				int num3 = num * 31;
				RoleBehaviour role = current.Data.Role;
				num = num3 + ((role != null && role.IsImpostor) ? 1 : 0);
				int num4 = num * 31;
				PlayerOutfit defaultOutfit = current.Data.DefaultOutfit;
				num = num4 + ((defaultOutfit != null) ? defaultOutfit.ColorId : 0);
				if (current.Data.PlayerName != null)
				{
					num = num * 31 + current.Data.PlayerName.GetHashCode();
				}
				if (current.Data.Tasks != null)
				{
					int num5 = 0;
					for (int i = 0; i < current.Data.Tasks.Count; i++)
					{
						if (current.Data.Tasks[i].Complete)
						{
							num5++;
						}
					}
					num = num * 31 + num5;
				}
				try
				{
					AmongUsClient instance = AmongUsClient.Instance;
					ClientData val = ((instance != null) ? ((InnerNetClient)instance).GetClient(((InnerNetObject)current).OwnerId) : null);
					if (val != null && val.PlatformData != null)
					{
						num = num * 31 + val.PlatformData.Platform;
					}
					try
					{
						GameData instance2 = GameData.Instance;
						NetworkedPlayerInfo val2 = ((instance2 != null) ? instance2.GetPlayerById(current.PlayerId) : null);
						if ((Object)(object)val2 != (Object)null)
						{
							num = num * 31 + (int)val2.PlayerLevel;
						}
					}
					catch
					{
					}
				}
				catch
				{
				}
			}
			num = num * 31 + num2;
			int num6 = num * 31;
			AmongUsClient instance3 = AmongUsClient.Instance;
			num = num6 + ((instance3 != null && ((InnerNetClient)instance3).AmHost) ? 1 : 0);
			num = num * 31 + (((Object)(object)ShipStatus.Instance != (Object)null) ? 1 : 0);
			num = num * 31 + (_openRoleDropdown.HasValue ? (_openRoleDropdown.Value + 1) : 0);
			num = num * 31 + (_openPreAssignDropdown.HasValue ? (_openPreAssignDropdown.Value + 1) : 0);
			num = num * 31 + (_showFilters ? 1 : 0);
			num = num * 31 + (_filterImpostors ? 1 : 0);
			num = num * 31 + (_filterCrewmates ? 1 : 0);
			try
			{
				num = num * 31 + (CheatConfig.GodMode ? 1 : 0);
				num = num * 31 + (CheatConfig.GodModeAll ? 1 : 0);
				num = num * 31 + ((CheatConfig.SeeGhosts?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.SeeDeadChat?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.FreeCamEnabled?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.NoClipSmoothEnabled?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.RadarEnabled?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.TracersEnabled?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.AllowVenting?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.WalkInVent?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.TeleportWithCursor?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.ShowKillCooldowns?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.NoShadows?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.KillAlertsEnabled?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.EventLoggerEnabled?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.NoSabotageCooldown?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.PhantomMode?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.EndlessVentTime?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.NoVentCooldown?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.EndlessShapeshiftDuration?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.NoShapeshiftCooldown?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.EndlessBattery?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.NoVitalsCooldown?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.EndlessTracking?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.NoTrackingCooldown?.Value ?? false) ? 1 : 0);
				num = num * 31 + (int)(CheatConfig.VisionMultiplier * 10f);
				num = num * 31 + (int)((CheatConfig.RadarScale?.Value ?? 0.08f) * 100f);
				num = num * 31 + ((CheatConfig.UnlockHats?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.UnlockSkins?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.UnlockPets?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.UnlockVisors?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.UnlockNameplates?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.UnlockBundles?.Value ?? false) ? 1 : 0);
				num = num * 31 + ((CheatConfig.UnlockStarsBeans?.Value ?? false) ? 1 : 0);
				num = num * 31 + (GameCheats.ScannerBypassEnabled ? 1 : 0);
				if (GameCheats.ScannerBypassEnabled)
				{
					num = num * 31 + (int)GameCheats.GetScannerBypassRemainingTime();
				}
				num = num * 31 + (GameCheats.IsRevealSusActive ? 1 : 0);
			}
			catch
			{
			}
			try
			{
				num = num * 31 + (SpoofingService.EnableLevelSpoof ? 1 : 0);
				num = num * 31 + (SpoofingService.EnablePlatformSpoof ? 1 : 0);
				num = num * 31 + (SpoofingService.EnableFriendCodeSpoof ? 1 : 0);
				num = num * 31 + (int)SpoofingService.GetEffectiveLevel();
				num = num * 31 + SpoofingService.SpoofedPlatform;
				num = num * 31 + (SpoofingService.SpoofedFriendCode?.GetHashCode() ?? 0);
			}
			catch
			{
			}
			try
			{
				num = num * 31 + BanMenu.GetStateHash();
			}
			catch
			{
			}
			try
			{
				num = num * 31 + RecentlyLeftService.GetStateHash();
			}
			catch
			{
			}
			try
			{
				num = num * 31 + AnimationsTab.GetStateHash();
			}
			catch
			{
			}
			RoleTypes val3;
			try
			{
				Dictionary<byte, RoleTypes> preGameRoleAssignments = ImpostorForcer.PreGameRoleAssignments;
				num = num * 31 + preGameRoleAssignments.Count;
				foreach (KeyValuePair<byte, RoleTypes> item in preGameRoleAssignments)
				{
					num = num * 31 + item.Key;
					int num7 = num * 31;
					val3 = item.Value;
					num = num7 + val3.GetHashCode();
				}
			}
			catch
			{
			}
			try
			{
				num = num * 31 + CheatManager.GetUiStateHash();
			}
			catch
			{
			}
			try
			{
				num = num * 31 + (ImpostorForcer.IsAnyOverrideActive ? 1 : 0);
				int num8 = num * 31;
				val3 = ImpostorForcer.SelectedRoleForHost;
				num = num8 + val3.GetHashCode();
			}
			catch
			{
			}
			try
			{
				num = num * 31 + (((Object)(object)MeetingHud.Instance != (Object)null) ? 1 : 0);
			}
			catch
			{
			}
			try
			{
				num = num * 31 + (GameCheats._dummyActive ? 1 : 0);
				num = num * 31 + (GameCheats._mapActive ? 1 : 0);
				num = num * 31 + (GameCheats._cfActive ? 1 : 0);
				num = num * 31 + GameCheats._specTarget;
				num = num * 31 + (GameCheats._allMapsActive ? 1 : 0);
				num = num * 31 + (GameCheats._selMapSpawnId + 2);
			}
			catch
			{
			}
			try
			{
				Dictionary<byte, float> syncATargets = GameCheats.GetSyncATargets();
				num = num * 31 + syncATargets.Count;
				foreach (KeyValuePair<byte, float> item2 in syncATargets)
				{
					num = num * 31 + item2.Key + (int)Math.Ceiling(item2.Value);
				}
				num = num * 31 + (GameCheats.IsSyncBAll() ? 7 : 0);
				num = num * 31 + (int)Math.Ceiling(GameCheats.GetSyncBAllRemaining());
				HashSet<byte> syncBTargets = GameCheats.GetSyncBTargets();
				num = num * 31 + syncBTargets.Count;
				foreach (byte item3 in syncBTargets)
				{
					num = num * 31 + item3;
				}
				num = num * 31 + (GameCheats.IsCHActive() ? 13 : 0);
				num = num * 31 + (int)Math.Ceiling(GameCheats.GetCHRemaining());
				num = num * 31 + (GameCheats.IsCLActive() ? 17 : 0);
				num = num * 31 + (int)Math.Ceiling(GameCheats.GetCLRemaining());
				num = num * 31 + (GameCheats.IsFakeCdActive() ? 19 : 0);
				num = num * 31 + GameCheats.GetFakeCdCurrent();
			}
			catch
			{
			}
			return num;
		}
		catch
		{
			return 0;
		}
	}

	static PlayerPickMenu()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		RoleTypes[] array = new RoleTypes[5];
		RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		_crewRoleCandidates = (RoleTypes[])(object)array;
		RoleTypes[] array2 = new RoleTypes[3];
		RuntimeHelpers.InitializeArray(array2, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		_impRoleCandidates = (RoleTypes[])(object)array2;
		_rndPool = new List<RoleTypes>(16);
		_rndPlayers = new List<PlayerControl>(16);
		_rndImpNames = new List<string>(4);
		_rng = new Random();
		_finalizePreserveBuf = new Dictionary<byte, RoleTypes>(16);
		_rndInvBuf = new Dictionary<RoleTypes, int>(16);
		_playerDataBuffer = new List<object>(16);
		_preAssignBuffer = new Dictionary<int, string>(16);
		_lastPlayerHash = 0;
		_lastCheckedFrame = -1;
		_lastSendTime = 0f;
		_lastWakeupTime = 0f;
		_lastHashCheckTime = 0f;
		_lastHashChangeSendTime = 0f;
		PendingImmediateHeartbeat = false;
	}
}
