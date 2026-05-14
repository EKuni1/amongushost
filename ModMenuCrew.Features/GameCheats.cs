using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using ModMenuCrew.UI.Managers;
using ModMenuCrew.UI.Menus;
using ModMenuCrew.UI.Styles;
using ModMenuCrew.Web;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ModMenuCrew.Features
{
    public static class GameCheats
    {

	[HarmonyPatch(typeof(CustomNetworkTransform), "FixedUpdate")]
	public static class FakeLagPatch
	{
		public static bool Prefix(CustomNetworkTransform __instance)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			if (!FakeLagEnabled || !((InnerNetObject)__instance).AmOwner)
			{
				return true;
			}
			if (_fakeLagBypass)
			{
				return true;
			}
			if (!IsFakeLagContextValid())
			{
				return true;
			}
			if ((Object)(object)__instance.body != (Object)null)
			{
				Vector2 position = __instance.body.position;
				if (!_fakeLagPosInit)
				{
					_fakeLagLastPos = position;
					_fakeLagPosInit = true;
				}
				else if (Vector2.Distance(position, _fakeLagLastPos) > 2f)
				{
					__instance.body.position = _fakeLagLastPos;
					((Component)__instance).transform.position = new Vector3(_fakeLagLastPos.x, _fakeLagLastPos.y, ((Component)__instance).transform.position.z);
				}
				else
				{
					_fakeLagLastPos = position;
				}
			}
			if (Time.frameCount % 15 == 0)
			{
				((InnerNetObject)__instance).SetDirtyBit(2u);
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(CustomNetworkTransform), "SnapTo", new System.Type[]
	{
		typeof(Vector2),
		typeof(ushort)
	})]
	public static class FakeLagSnapToPatch
	{
		public static bool Prefix(CustomNetworkTransform __instance)
		{
			if (_fakeLagBypass)
			{
				return true;
			}
			if (FakeLagEnabled && ((InnerNetObject)__instance).AmOwner)
			{
				return !IsFakeLagContextValid();
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(CustomNetworkTransform), "RpcSnapTo")]
	public static class FakeLagRpcSnapToPatch
	{
		public static bool Prefix(CustomNetworkTransform __instance)
		{
			if (_fakeLagBypass)
			{
				return true;
			}
			if (FakeLagEnabled && ((InnerNetObject)__instance).AmOwner)
			{
				return !IsFakeLagContextValid();
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(CustomNetworkTransform), "Halt")]
	public static class FakeLagHaltPatch
	{
		public static bool Prefix(CustomNetworkTransform __instance)
		{
			if (_fakeLagBypass)
			{
				return true;
			}
			if (FakeLagEnabled && ((InnerNetObject)__instance).AmOwner)
			{
				return !IsFakeLagContextValid();
			}
			return true;
		}
	}

	internal struct DummyEntry
	{
		public uint[] NetIds;

		public Vector2 Pos;

		public byte SourcePlayerId;
	}

	[HarmonyPatch(typeof(AmongUsClient), "StartGame")]
	public static class AutoClearLabOnStartGamePatch
	{
		public static void Prefix()
		{
			try
			{
				AmongUsClient instance = AmongUsClient.Instance;
				if (!((Object)(object)instance == (Object)null) && ((InnerNetClient)instance).AmHost && (_dummyActive || _cfActive))
				{
					_ImmediateDespawnAllGhosts();
					_dummyActive = false;
					_dummyNetIdsList.Clear();
					_dummySynced.Clear();
					_cfActive = false;
					_cfStates.Clear();
					_cfSynced.Clear();
					_cfSpawnQueue.Clear();
					_cfReconcilePending.Clear();
					LabRpcArbiter.Unregister(LabRpcArbiter.Consumer.GhostTwins);
					LabRpcArbiter.Unregister(LabRpcArbiter.Consumer.ShadowClones);
					LabRpcArbiter.FlushAll();
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Object.op_Implicit("[LAB] AutoClearLabOnStartGame prefix threw: " + ex.Message));
			}
		}
	}

	[HarmonyPatch(typeof(HudManager), "Update")]
	public static class FakeCountdownHudPatch
	{
		public static Exception Finalizer(Exception __exception)
		{
			if (__exception is Il2CppException)
			{
				return null;
			}
			return __exception;
		}

		public static void Postfix()
		{
			try
			{
				TickFakeCountdown();
			}
			catch
			{
			}
			try
			{
				TickRainbow();
			}
			catch
			{
			}
			try
			{
				TickPlayerRainbows();
			}
			catch
			{
			}
			try
			{
				TickColorBurstQueue();
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(LobbyBehaviour), "Update")]
	public static class FakeCountdownLobbyPatch
	{
		public static void Postfix()
		{
			try
			{
				TickFakeCountdown();
			}
			catch
			{
			}
			try
			{
				TickRainbow();
			}
			catch
			{
			}
			try
			{
				TickPlayerRainbows();
			}
			catch
			{
			}
			try
			{
				TickColorBurstQueue();
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(ShipStatus), "Start")]
	public static class MapSwapInitPatch
	{
		public static void Postfix(ShipStatus __instance)
		{
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			_endGameActive = false;
			try
			{
				if ((Object)(object)AmongUsClient.Instance == (Object)null)
				{
					return;
				}
				bool amHost = ((InnerNetClient)AmongUsClient.Instance).AmHost;
				if (!(amHost ? _mapSwapInProgress : ((InnerNetClient)AmongUsClient.Instance).IsGameStarted) || (Object)(object)__instance != (Object)(object)ShipStatus.Instance)
				{
					return;
				}
				_mapSwapInProgress = false;
				if (_savedCosmeticsCache != null)
				{
					__instance.CosmeticsCache = _savedCosmeticsCache;
					_savedCosmeticsCache = null;
				}
				else
				{
					try
					{
						IEnumerator enumerator = (IEnumerator)__instance.CosmeticsCache.PopulateFromPlayers();
						((MonoBehaviour)__instance).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(enumerator));
					}
					catch
					{
					}
				}
				if (amHost)
				{
					try
					{
						__instance.Begin();
					}
					catch
					{
					}
				}
				Debug.Log(Object.op_Implicit($"[Cheat] MapSwapInit: Begin={amHost} CacheTransfer={_savedCosmeticsCache == null} map={__instance.Type}"));
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(Vent), "SetOutline")]
	public static class VentSetOutlineNullGuardPatch
	{
		public static bool Prefix(Vent __instance)
		{
			try
			{
				if ((Object)(object)__instance == (Object)null)
				{
					return false;
				}
				if ((Object)(object)((Component)__instance).GetComponent<Renderer>() == (Object)null)
				{
					return false;
				}
				return true;
			}
			catch
			{
				return false;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerVoteArea), "SetCosmetics")]
	public static class SetCosmeticsNullGuardPatch
	{
		public static Exception Finalizer(Exception __exception)
		{
			if (__exception is Il2CppException)
			{
				return null;
			}
			return __exception;
		}
	}

	[HarmonyPatch(typeof(MeetingHud), "ServerStart")]
	public static class MeetingHudAutoRestorePatch
	{
		public static void Prefix()
		{
			if (!_isImpersonating)
			{
				return;
			}
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if ((Object)(object)((localPlayer != null) ? localPlayer.Data : null) == (Object)null)
			{
				return;
			}
			try
			{
				localPlayer.Data.DefaultOutfit.ColorId = _savedColorId;
				localPlayer.Data.DefaultOutfit.HatId = _savedHatId;
				localPlayer.Data.DefaultOutfit.SkinId = _savedSkinId;
				localPlayer.Data.DefaultOutfit.PetId = _savedPetId;
				localPlayer.Data.DefaultOutfit.VisorId = _savedVisorId;
				localPlayer.Data.DefaultOutfit.NamePlateId = _savedNamePlateId;
				localPlayer.Data.MarkDirty();
				localPlayer.RpcSetColor(_savedColorId);
				localPlayer.RpcSetHat(_savedHatId);
				localPlayer.RpcSetSkin(_savedSkinId);
				localPlayer.RpcSetPet(_savedPetId);
				localPlayer.RpcSetVisor(_savedVisorId);
				_isImpersonating = false;
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(MeetingHud), "Start")]
	public static class MeetingHudStartNullGuardPatch
	{
		public static Exception Finalizer(Exception __exception)
		{
			if (__exception is Il2CppException)
			{
				return null;
			}
			return __exception;
		}
	}

	private class _CFState
	{
		public uint[] netIds;

		public int cntIdx;

		public ushort sid;

		public float orbitPhase;

		public int orbitBroadcastStep;
	}

	[HarmonyPatch(typeof(PhantomRole), "FixedUpdate")]
	public static class PhantomInfiniteDurationPatch
	{
		public static void Postfix(PhantomRole __instance)
		{
			if (CheatConfig.PhantomMode.Value && !((Object)(object)((__instance != null) ? ((RoleBehaviour)__instance).Player : null) == (Object)null) && ((InnerNetObject)((RoleBehaviour)__instance).Player).AmOwner && __instance.IsInvisible)
			{
				__instance.durationSecondsRemaining = 9999f;
			}
		}
	}

	[HarmonyPatch(typeof(PhantomRole), "IsValidTarget")]
	public static class PhantomKillWhileInvisiblePatch
	{
		public static bool Prefix(PhantomRole __instance, ref bool __result, NetworkedPlayerInfo target)
		{
			if (!CheatConfig.PhantomMode.Value)
			{
				return true;
			}
			if ((Object)(object)((__instance != null) ? ((RoleBehaviour)__instance).Player : null) == (Object)null || !((InnerNetObject)((RoleBehaviour)__instance).Player).AmOwner)
			{
				return true;
			}
			if (!__instance.IsInvisible)
			{
				return true;
			}
			if (!((Object)(object)target == (Object)null) && !target.Disconnected && !target.IsDead && target.PlayerId != ((RoleBehaviour)__instance).Player.PlayerId)
			{
				RoleBehaviour role = target.Role;
				if (role == null || !role.IsImpostor)
				{
					__result = true;
					return false;
				}
			}
			__result = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "HandleRpc")]
	public static class ImpostorForcer1Patch
	{
		public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
		{
			if (callId != 44 || !((Object)(object)AmongUsClient.Instance != (Object)null) || ((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				return;
			}
			byte b = reader.ReadByte();
			if (reader.ReadByte() != 1)
			{
				return;
			}
			PlayerControl val = null;
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current != (Object)null && current.PlayerId == b)
				{
					val = current;
					break;
				}
			}
			if ((Object)(object)val != (Object)null)
			{
				val.RpcSetRole((RoleTypes)1, false);
				LogCheat($"Player {b} forced to impostor.");
			}
		}
	}

	public static class Impostor1Forcer
	{
		internal static void RequestImpostorRole()
		{
			if ((Object)(object)PlayerControl.LocalPlayer == (Object)null)
			{
				Debug.LogWarning(Object.op_Implicit("Local player not found."));
				return;
			}
			if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				Debug.LogWarning(Object.op_Implicit("You must be host to use this cheat."));
				return;
			}
			try
			{
				MessageWriter val = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)PlayerControl.LocalPlayer).NetId, (byte)44, (SendOption)1, -1);
				val.Write(PlayerControl.LocalPlayer.PlayerId);
				val.Write((byte)1);
				val.Write(GenerateFakeToken(PlayerControl.LocalPlayer.PlayerId));
				((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val);
				LogCheat("Impostor role requested with bypass.");
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"Error in RequestImpostorRole: {value}"));
			}
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "HandleRpc")]
	public static class MurderBypassPatch
	{
		public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader, out PlayerControl __state)
		{
			__state = null;
			if (callId != 12)
			{
				return true;
			}
			try
			{
				int position = reader.Position;
				try
				{
					uint num = reader.ReadPackedUInt32();
					Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
					while (enumerator.MoveNext())
					{
						PlayerControl current = enumerator.Current;
						if ((Object)(object)current != (Object)null && ((InnerNetObject)current).NetId == num)
						{
							__state = current;
							break;
						}
					}
				}
				catch
				{
				}
				finally
				{
					reader.Position = position;
				}
			}
			catch
			{
			}
			return true;
		}

		public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, PlayerControl __state)
		{
			if (callId != 12 || (Object)(object)__state == (Object)null)
			{
				return;
			}
			if ((Object)(object)__state.Data != (Object)null && __state.Data.IsDead)
			{
				if ((Object)(object)__state.cosmetics != (Object)null)
				{
					((Component)__state).gameObject.layer = LayerMask.NameToLayer("Ghost");
				}
				if (((InnerNetObject)__state).AmOwner && (Object)(object)DestroyableSingleton<HudManager>.Instance != (Object)null)
				{
					if (Object.op_Implicit((Object)(object)Minigame.Instance))
					{
						try
						{
							Minigame.Instance.Close();
						}
						catch
						{
						}
					}
					try
					{
						__state.cosmetics.SetNameMask(false);
					}
					catch
					{
					}
				}
				Debug.Log(Object.op_Implicit("[KillBypassPatch] Visuals applied for " + __state.Data.PlayerName + " (Confirmed Dead)."));
			}
			else
			{
				NetworkedPlayerInfo data = __state.Data;
				Debug.Log(Object.op_Implicit("[KillBypassPatch] Target " + ((data != null) ? data.PlayerName : null) + " did not die (Protected/Failed). Visuals skipped."));
			}
		}
	}

	[HarmonyPatch(typeof(HudManager), "Start")]
	public static class ForceAspectUpdateAlwaysPatch
	{
		public static Exception Finalizer(Exception __exception)
		{
			if (__exception is Il2CppException)
			{
				return null;
			}
			return __exception;
		}

		private static void Postfix(HudManager __instance)
		{
			if ((Object)(object)__instance == (Object)null)
			{
				return;
			}
			try
			{
				EnableUpdateAlways((MonoBehaviour)(object)__instance.UseButton);
				EnableUpdateAlways((MonoBehaviour)(object)__instance.KillButton);
				EnableUpdateAlways((MonoBehaviour)(object)__instance.ReportButton);
				EnableUpdateAlways((MonoBehaviour)(object)__instance.SabotageButton);
				EnableUpdateAlways((MonoBehaviour)(object)__instance.ImpostorVentButton);
				EnableUpdateAlways((MonoBehaviour)(object)__instance.AdminButton);
				EnableUpdateAlways((MonoBehaviour)(object)__instance.AbilityButton);
				EnableUpdateAlways((MonoBehaviour)(object)__instance.SecondaryAbilityButton);
				EnableUpdateAlways((MonoBehaviour)(object)__instance.PetButton);
				EnableUpdateAlways((MonoBehaviour)(object)__instance.TaskPanel);
				EnableUpdateAlways((MonoBehaviour)(object)__instance.CrewmatesKilled);
				if ((Object)(object)__instance.TaskStuff != (Object)null)
				{
					EnableUpdateAlwaysOnGO(__instance.TaskStuff);
					EnableUpdateAlwaysOnAllDescendants(__instance.TaskStuff);
				}
				if ((Object)(object)__instance.TaskPanel != (Object)null)
				{
					EnableUpdateAlwaysOnAllDescendants(((Component)__instance.TaskPanel).gameObject);
				}
				if ((Object)(object)__instance.MapButton != (Object)null)
				{
					EnableUpdateAlwaysOnGO(((Component)__instance.MapButton).gameObject);
				}
				if ((Object)(object)__instance.SettingsButton != (Object)null)
				{
					EnableUpdateAlwaysOnGO(__instance.SettingsButton);
				}
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[Cheat] ForceAspectUpdateAlways error: {value}"));
			}
		}

		private static void EnableUpdateAlways(MonoBehaviour target)
		{
			if (!((Object)(object)target == (Object)null))
			{
				EnableUpdateAlwaysOnGO(((Component)target).gameObject);
			}
		}

		private static void EnableUpdateAlwaysOnGO(GameObject go)
		{
			if ((Object)(object)go == (Object)null)
			{
				return;
			}
			try
			{
				AspectPosition component = go.GetComponent<AspectPosition>();
				if ((Object)(object)component != (Object)null)
				{
					component.updateAlways = true;
					return;
				}
				Transform parent = go.transform.parent;
				while ((Object)(object)parent != (Object)null)
				{
					AspectPosition component2 = ((Component)parent).GetComponent<AspectPosition>();
					if ((Object)(object)component2 != (Object)null)
					{
						component2.updateAlways = true;
						break;
					}
					parent = parent.parent;
				}
			}
			catch
			{
			}
		}

		private static void EnableUpdateAlwaysOnAllDescendants(GameObject root)
		{
			if ((Object)(object)root == (Object)null)
			{
				return;
			}
			try
			{
				Il2CppArrayBase<AspectPosition> componentsInChildren = root.GetComponentsInChildren<AspectPosition>(true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					AspectPosition val = componentsInChildren[i];
					if ((Object)(object)val != (Object)null)
					{
						val.updateAlways = true;
					}
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(FollowerCamera), "Update")]
	public static class ZoomOutPatch
	{
		private static void Postfix(FollowerCamera __instance)
		{
			try
			{
				if ((Object)(object)__instance == (Object)null)
				{
					return;
				}
				Camera component = ((Component)__instance).GetComponent<Camera>();
				if ((Object)(object)component == (Object)null)
				{
					return;
				}
				float visionMultiplier = CheatConfig.VisionMultiplier;
				bool flag = IsOnActiveMap();
				if (visionMultiplier > 3.01f && !FreeCamEnabled && !_endGameActive && flag)
				{
					if (component.orthographicSize < visionMultiplier && component.orthographicSize < _defaultOrthoSize + 0.01f)
					{
						_defaultOrthoSize = component.orthographicSize;
					}
					component.orthographicSize = visionMultiplier;
					try
					{
						HudManager instance = DestroyableSingleton<HudManager>.Instance;
						Camera val = ((instance != null) ? instance.UICamera : null);
						if ((Object)(object)val != (Object)null)
						{
							val.orthographicSize = visionMultiplier;
						}
					}
					catch
					{
					}
					HudManager instance2 = DestroyableSingleton<HudManager>.Instance;
					if ((Object)(object)((instance2 != null) ? instance2.ShadowQuad : null) != (Object)null && ((Component)instance2.ShadowQuad).gameObject.activeSelf)
					{
						((Component)instance2.ShadowQuad).gameObject.SetActive(false);
					}
					if ((Object)(object)((instance2 != null) ? instance2.FullScreen : null) != (Object)null && ((Renderer)instance2.FullScreen).enabled)
					{
						((Renderer)instance2.FullScreen).enabled = false;
					}
					return;
				}
				if (component.orthographicSize > _defaultOrthoSize + 0.01f)
				{
					component.orthographicSize = _defaultOrthoSize;
				}
				else if (component.orthographicSize < _defaultOrthoSize - 0.01f)
				{
					_defaultOrthoSize = component.orthographicSize;
				}
				try
				{
					HudManager instance3 = DestroyableSingleton<HudManager>.Instance;
					Camera val2 = ((instance3 != null) ? instance3.UICamera : null);
					if ((Object)(object)val2 != (Object)null)
					{
						val2.orthographicSize = component.orthographicSize;
					}
				}
				catch
				{
				}
				CheatManager instance4 = CheatManager.Instance;
				if ((instance4 == null || !instance4.NoShadowsEnabled) && !FreeCamEnabled)
				{
					HudManager instance5 = DestroyableSingleton<HudManager>.Instance;
					if ((Object)(object)((instance5 != null) ? instance5.ShadowQuad : null) != (Object)null && !((Component)instance5.ShadowQuad).gameObject.activeSelf)
					{
						((Component)instance5.ShadowQuad).gameObject.SetActive(true);
					}
					if ((Object)(object)((instance5 != null) ? instance5.FullScreen : null) != (Object)null && !((Renderer)instance5.FullScreen).enabled)
					{
						((Renderer)instance5.FullScreen).enabled = true;
					}
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(Controller), "Update")]
	public static class ControllerCameraFixPatch
	{
		private static void Prefix(Controller __instance)
		{
			try
			{
				bool flag = IsOnActiveMap();
				if ((CheatConfig.VisionMultiplier > 3.01f || FreeCamEnabled) && !_endGameActive && flag)
				{
					if (DestroyableSingleton<HudManager>.InstanceExists)
					{
						HudManager instance = DestroyableSingleton<HudManager>.Instance;
						Camera val = ((instance != null) ? instance.UICamera : null);
						if ((Object)(object)val != (Object)null)
						{
							__instance.mainCam = val;
						}
					}
				}
				else if ((Object)(object)__instance.mainCam != (Object)(object)Camera.main && (Object)(object)Camera.main != (Object)null)
				{
					__instance.mainCam = Camera.main;
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(ShipStatus), "CalculateLightRadius")]
	public static class SatelliteLightRadiusPatch
	{
		private static void Postfix(ref float __result)
		{
			try
			{
				CheatManager instance = CheatManager.Instance;
				if (instance != null && instance.NoShadowsEnabled)
				{
					__result = 50f;
					return;
				}
				float visionMultiplier = CheatConfig.VisionMultiplier;
				if (visionMultiplier > 3.01f && !FreeCamEnabled)
				{
					__result = Mathf.Max(__result, visionMultiplier);
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(AspectPosition), "AdjustPosition", new System.Type[] { })]
	public static class AspectPositionParentFixPatch
	{
		private static void Prefix(AspectPosition __instance)
		{
			try
			{
				if (((Object)(object)__instance.parentCam == (Object)null || (Object)(object)__instance.parentCam == (Object)(object)Camera.main) && DestroyableSingleton<HudManager>.InstanceExists)
				{
					HudManager instance = DestroyableSingleton<HudManager>.Instance;
					Camera val = ((instance != null) ? instance.UICamera : null);
					if ((Object)(object)val != (Object)null)
					{
						__instance.parentCam = val;
					}
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(AspectPosition), "AdjustPosition", new System.Type[] { typeof(float) })]
	public static class AspectPositionParentFixPatchAspect
	{
		private static void Prefix(AspectPosition __instance)
		{
			try
			{
				if (((Object)(object)__instance.parentCam == (Object)null || (Object)(object)__instance.parentCam == (Object)(object)Camera.main) && DestroyableSingleton<HudManager>.InstanceExists)
				{
					HudManager instance = DestroyableSingleton<HudManager>.Instance;
					Camera val = ((instance != null) ? instance.UICamera : null);
					if ((Object)(object)val != (Object)null)
					{
						__instance.parentCam = val;
					}
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(Minigame), "Begin")]
	public static class MinigameBeginZoomFix
	{
		private static void Prefix()
		{
			if ((Object)(object)Camera.main != (Object)null && (CheatConfig.VisionMultiplier > 3.01f || freecamActive))
			{
				Camera.main.orthographicSize = _defaultOrthoSize;
			}
		}
	}

	[HarmonyPatch(typeof(MeetingHud), "Start")]
	public static class MeetingHudStartZoomFix
	{
		private static void Prefix()
		{
			if (CheatConfig.VisionMultiplier > 3.01f)
			{
				ResetSatelliteState();
			}
			else if ((Object)(object)Camera.main != (Object)null && freecamActive)
			{
				Camera.main.orthographicSize = _defaultOrthoSize;
			}
		}
	}

	[HarmonyPatch(typeof(AmongUsClient), "OnGameEnd")]
	public static class ResetZoomOnGameEndPatch
	{
		private static void Postfix()
		{
			_endGameActive = true;
			ResetSatelliteState();
			if (FreeCamEnabled)
			{
				FreeCamEnabled = false;
			}
			CheatConfig.GodMode = false;
			CheatConfig.GodModeAll = false;
			try
			{
				if (_dummyActive)
				{
					ToggleDummy();
				}
			}
			catch
			{
			}
			try
			{
				if (_cfActive)
				{
					ToggleCloneFollow();
				}
			}
			catch
			{
			}
			try
			{
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
			catch
			{
			}
			DisableFakeLagOnGameEnd();
			RpcSyncValidator.Reset();
			try
			{
				AutoKickChatWordPatch.Reset();
			}
			catch
			{
			}
			IsRevealSusActive = CheatConfig.RevealSus?.Value ?? false;
			try
			{
				CleanupTracers();
			}
			catch
			{
			}
			try
			{
				CleanupRadarTextures();
			}
			catch
			{
			}
			try
			{
				ClearAutoKickState();
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "ReportClosest")]
	public static class InfiniteReportPatch
	{
		private static bool Prefix(PlayerControl __instance)
		{
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				ConfigEntry<bool> infiniteReportRange = CheatConfig.InfiniteReportRange;
				if (infiniteReportRange == null || !infiniteReportRange.Value)
				{
					return true;
				}
				if ((Object)(object)__instance != (Object)(object)PlayerControl.LocalPlayer)
				{
					return true;
				}
				if (((InnerNetClient)AmongUsClient.Instance).IsGameOver || (Object)(object)__instance.Data == (Object)null || __instance.Data.IsDead)
				{
					return true;
				}
				Il2CppArrayBase<DeadBody> val = Object.FindObjectsOfType<DeadBody>();
				if (val == null || val.Length == 0)
				{
					return true;
				}
				DeadBody val2 = null;
				float num = float.MaxValue;
				Vector2 truePosition = __instance.GetTruePosition();
				foreach (DeadBody item in val)
				{
					if (!((Object)(object)item == (Object)null) && ((Behaviour)item).enabled && !item.Reported && !((Object)(object)item.myCollider == (Object)null) && ((Behaviour)item.myCollider).enabled)
					{
						float num2 = Vector2.Distance(truePosition, item.TruePosition);
						if (num2 < num)
						{
							num = num2;
							val2 = item;
						}
					}
				}
				if ((Object)(object)val2 == (Object)null)
				{
					return true;
				}
				GameData instance = GameData.Instance;
				NetworkedPlayerInfo val3 = ((instance != null) ? instance.GetPlayerById(val2.ParentId) : null);
				if ((Object)(object)val3 == (Object)null)
				{
					return true;
				}
				val2.Reported = true;
				__instance.CmdReportDeadBody(val3);
				return false;
			}
			catch
			{
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(Ladder), "SetOutline")]
	public static class LadderSetOutlineSafePatch
	{
		private static bool Prefix(Ladder __instance, bool on, bool mainTarget)
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)__instance == (Object)null)
				{
					return false;
				}
				SpriteRenderer image = __instance.Image;
				if ((Object)(object)image == (Object)null)
				{
					return false;
				}
				Material material = ((Renderer)image).material;
				if ((Object)(object)material == (Object)null)
				{
					return false;
				}
				material.SetFloat("_Outline", on ? 1f : 0f);
				material.SetColor("_OutlineColor", Color.white);
				material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
			}
			catch
			{
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(ZiplineConsole), "SetOutline")]
	public static class ZiplineConsoleSetOutlineSafePatch
	{
		private static bool Prefix(ZiplineConsole __instance, bool on, bool mainTarget)
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)__instance == (Object)null)
				{
					return false;
				}
				SpriteRenderer image = __instance.image;
				if ((Object)(object)image == (Object)null)
				{
					return false;
				}
				Material material = ((Renderer)image).material;
				if ((Object)(object)material == (Object)null)
				{
					return false;
				}
				material.SetFloat("_Outline", on ? 1f : 0f);
				material.SetColor("_OutlineColor", Color.white);
				material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
			}
			catch
			{
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "FixedUpdate")]
	public static class InfiniteReportButtonPatch
	{
		public static void Postfix(PlayerControl __instance)
		{
			try
			{
				if ((Object)(object)__instance != (Object)(object)PlayerControl.LocalPlayer)
				{
					return;
				}
				ConfigEntry<bool> infiniteReportRange = CheatConfig.InfiniteReportRange;
				if (infiniteReportRange == null || !infiniteReportRange.Value || (Object)(object)__instance.Data == (Object)null || __instance.Data.IsDead || !DestroyableSingleton<HudManager>.InstanceExists)
				{
					return;
				}
				Il2CppArrayBase<DeadBody> val = Object.FindObjectsOfType<DeadBody>();
				if (val == null)
				{
					return;
				}
				foreach (DeadBody item in val)
				{
					if (!((Object)(object)item == (Object)null) && ((Behaviour)item).enabled && !item.Reported && !((Object)(object)item.myCollider == (Object)null) && ((Behaviour)item.myCollider).enabled)
					{
						DestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(true);
						break;
					}
				}
			}
			catch
			{
			}
		}
	}

	private struct MapInfo
	{
		public float xOffset;

		public float yOffset;

		public float scale;

		public float minX;

		public float maxX;

		public float minY;

		public float maxY;

		public float imgMinX;

		public float imgMaxX;

		public float imgMinY;

		public float imgMaxY;

		public MapInfo(float xOff, float yOff, float sc, float mnX, float mxX, float mnY, float mxY)
		{
			xOffset = xOff;
			yOffset = yOff;
			scale = sc;
			minX = mnX;
			maxX = mxX;
			minY = mnY;
			maxY = mxY;
			imgMinX = mnX;
			imgMaxX = mxX;
			imgMinY = mnY;
			imgMaxY = mxY;
		}

		public MapInfo(float xOff, float yOff, float sc, float mnX, float mxX, float mnY, float mxY, float imnX, float imxX, float imnY, float imxY)
		{
			xOffset = xOff;
			yOffset = yOff;
			scale = sc;
			minX = mnX;
			maxX = mxX;
			minY = mnY;
			maxY = mxY;
			imgMinX = imnX;
			imgMaxX = imxX;
			imgMinY = imnY;
			imgMaxY = imxY;
		}
	}

	private struct RadarPlayerData
	{
		public float x;

		public float y;

		public bool isImpostor;

		public bool isDead;

		public int colorId;

		public string name;

		public byte playerId;
	}

	private struct RadarDeadBodyData
	{
		public float x;

		public float y;

		public int colorId;

		public string name;
	}

	private struct RoomData
	{
		public float x;

		public float y;

		public float w;

		public float h;

		public int shapeType;

		public RoomData(float x, float y, float w, float h, int shape)
		{
			this.x = x;
			this.y = y;
			this.w = w;
			this.h = h;
			shapeType = shape;
		}

		public RoomData(float x, float y, float w, float h, bool octagon = false, bool corridor = false)
		{
			this.x = x;
			this.y = y;
			this.w = w;
			this.h = h;
			shapeType = (corridor ? 5 : (octagon ? 1 : 0));
		}
	}

	[HarmonyPatch(typeof(LogicOptions), "GetKillDistance")]
	public static class NoKillDistancePatch
	{
		public static bool Prefix(ref float __result)
		{
			if (!NoKillDistanceLimitEnabled)
			{
				return true;
			}
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
				return true;
			}
			if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
			{
				return true;
			}
			__result = float.MaxValue;
			return false;
		}
	}

	[HarmonyPatch(typeof(ImpostorRole), "FindClosestTarget")]
	public static class NoKillDistanceTargetPatch
	{
		public static bool Prefix(ImpostorRole __instance, ref PlayerControl __result)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			if (!NoKillDistanceLimitEnabled)
			{
				return true;
			}
			try
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
				if ((Object)obj == (Object)null || !localPlayer.Data.Role.IsImpostor)
				{
					return true;
				}
				Vector2 truePosition = localPlayer.GetTruePosition();
				PlayerControl val = null;
				float num = float.MaxValue;
				Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PlayerControl current = enumerator.Current;
					if (!((Object)(object)current == (Object)null) && !((Object)(object)current.Data == (Object)null) && !current.Data.IsDead && !current.Data.Disconnected && current.PlayerId != localPlayer.PlayerId && !((Object)(object)current.Data.Role == (Object)null) && current.Data.Role.CanBeKilled && !current.inVent && !current.inMovingPlat && current.Visible && ((Behaviour)current.Collider).enabled)
					{
						Vector2 truePosition2 = current.GetTruePosition();
						float num2 = Vector2.Distance(truePosition, truePosition2);
						if (num2 < num)
						{
							num = num2;
							val = current;
						}
					}
				}
				__result = val;
				return false;
			}
			catch
			{
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "TurnOnProtection")]
	public static class ShieldVisPatch
	{
		public static void Prefix(ref bool visible)
		{
			if (ShieldVisEnabled)
			{
				visible = true;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "FixedUpdate")]
	public static class SeeGhostsPatch
	{
		public static void Postfix(PlayerControl __instance)
		{
			if ((Object)(object)__instance == (Object)(object)PlayerControl.LocalPlayer)
			{
				if (Input.GetKeyDown((KeyCode)290))
				{
					ToggleRadarDebug();
				}
				if (Input.GetKeyDown((KeyCode)291))
				{
					DumpRoomBounds();
				}
				UpdateRadarDebug();
				CheckLobbyChange();
				_RB();
				_TCH();
				_TCL();
				_TA();
				_TB();
			}
			if (!SeeGhostsEnabled || (Object)(object)__instance == (Object)null || (Object)(object)__instance.Data == (Object)null || !__instance.Data.IsDead || ((InnerNetObject)__instance).AmOwner)
			{
				return;
			}
			try
			{
				__instance.Visible = true;
				((Component)__instance).gameObject.layer = LayerMask.NameToLayer("Players");
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "HandleRpc")]
	public static class GhostChatRpcPatch
	{
		public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
		{
			if (callId == RPC_SET_SCANNER)
			{
				HandleScannerRPC(__instance, callId, reader);
			}
			else
			{
				if (callId != 13 || !SeeDeadChatEnabled)
				{
					return;
				}
				try
				{
					if (!((Object)(object)PlayerControl.LocalPlayer == (Object)null) && !((Object)(object)PlayerControl.LocalPlayer.Data == (Object)null) && !PlayerControl.LocalPlayer.Data.IsDead && !((Object)(object)__instance == (Object)null) && !((Object)(object)__instance.Data == (Object)null) && __instance.Data.IsDead)
					{
						int position = reader.Position;
						reader.Position = 0;
						string chatText = reader.ReadString();
						reader.Position = position;
						ForceAddGhostChat(__instance, chatText);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError(Object.op_Implicit("[GhostChat] Erro ao processar RPC: " + ex.Message));
				}
			}
		}
	}

	[HarmonyPatch(typeof(ChatController), "AddChat")]
	public static class ForceShowDeadChatPatch
	{
		private static bool _originalDeadState;

		private static bool _stateModified;

		public static void Prefix(ChatController __instance, PlayerControl sourcePlayer, string chatText, bool censor)
		{
			_stateModified = false;
			if (SeeDeadChatEnabled && !((Object)(object)((sourcePlayer != null) ? sourcePlayer.Data : null) == (Object)null) && sourcePlayer.Data.IsDead)
			{
				PlayerControl localPlayer = PlayerControl.LocalPlayer;
				if (!((Object)(object)((localPlayer != null) ? localPlayer.Data : null) == (Object)null) && !PlayerControl.LocalPlayer.Data.IsDead)
				{
					_originalDeadState = PlayerControl.LocalPlayer.Data.IsDead;
					PlayerControl.LocalPlayer.Data.IsDead = true;
					_stateModified = true;
				}
			}
		}

		public static void Postfix(ChatController __instance, PlayerControl sourcePlayer)
		{
			if (_stateModified)
			{
				PlayerControl localPlayer = PlayerControl.LocalPlayer;
				if ((Object)(object)((localPlayer != null) ? localPlayer.Data : null) != (Object)null)
				{
					PlayerControl.LocalPlayer.Data.IsDead = _originalDeadState;
					_stateModified = false;
				}
			}
		}
	}

	[HarmonyPatch(typeof(ChatController), "AddChat")]
	public static class AutoKickChatWordPatch
	{
		private static string _cachedRaw;

		private static string[] _wholeWords;

		private static string[] _substringWords;

		private static readonly HashSet<int> _recentKicked = new HashSet<int>();

		private static float _lastCleanup;

		private static string Normalize(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return "";
			}
			string text;
			try
			{
				text = s.Normalize(NormalizationForm.FormKD);
			}
			catch
			{
				text = s;
			}
			StringBuilder stringBuilder = new StringBuilder(text.Length);
			char c = '\0';
			for (int i = 0; i < text.Length; i++)
			{
				char c2 = text[i];
				if (c2 != '\u200b' && c2 != '\ufeff' && c2 != '\u200c' && c2 != '\u200d' && CharUnicodeInfo.GetUnicodeCategory(c2) != UnicodeCategory.NonSpacingMark)
				{
					if (c2 >= 'A' && c2 <= 'Z')
					{
						c2 = (char)(c2 + 32);
					}
					switch (c2)
					{
					case '@':
						c2 = 'a';
						break;
					case '$':
						c2 = 's';
						break;
					case '0':
						c2 = 'o';
						break;
					case '1':
						c2 = 'i';
						break;
					case '3':
						c2 = 'e';
						break;
					case '4':
						c2 = 'a';
						break;
					case '5':
						c2 = 's';
						break;
					case '7':
						c2 = 't';
						break;
					case '!':
						c2 = 'i';
						break;
					}
					if (c2 != c || c2 < 'a' || c2 > 'z')
					{
						stringBuilder.Append(c2);
						c = c2;
					}
				}
			}
			return stringBuilder.ToString();
		}

		private static bool IsWordChar(char c)
		{
			if (c < 'a' || c > 'z')
			{
				if (c >= '0')
				{
					return c <= '9';
				}
				return false;
			}
			return true;
		}

		public static void Prefix(PlayerControl sourcePlayer, string chatText)
		{
			try
			{
				ConfigEntry<bool> autoKickByChatEnabled = CheatConfig.AutoKickByChatEnabled;
				if (autoKickByChatEnabled == null || !autoKickByChatEnabled.Value || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost || (Object)(object)sourcePlayer == (Object)null || string.IsNullOrEmpty(chatText))
				{
					return;
				}
				PlayerControl localPlayer = PlayerControl.LocalPlayer;
				if (((Object)(object)localPlayer != (Object)null && sourcePlayer.PlayerId == localPlayer.PlayerId) || IsAutoKickImmune(sourcePlayer.PlayerId))
				{
					return;
				}
				string text = CheatConfig.AutoKickChatWordList?.Value ?? "";
				if (!string.Equals(text, _cachedRaw, StringComparison.Ordinal))
				{
					_cachedRaw = text;
					List<string> list = new List<string>();
					List<string> list2 = new List<string>();
					string[] array = text.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					for (int i = 0; i < array.Length; i++)
					{
						string text2 = array[i].Trim();
						if (text2.Length == 0)
						{
							continue;
						}
						bool flag = text2[0] == '*';
						string text3 = (flag ? text2.Substring(1).Trim() : text2);
						string text4 = Normalize(text3);
						if (!string.IsNullOrEmpty(text4))
						{
							if (flag || text3.IndexOf(' ') >= 0)
							{
								list2.Add(text4);
							}
							else
							{
								list.Add(text4);
							}
						}
					}
					_wholeWords = ((list.Count == 0) ? null : list.ToArray());
					_substringWords = ((list2.Count == 0) ? null : list2.ToArray());
				}
				if (_wholeWords == null && _substringWords == null)
				{
					return;
				}
				if (Time.time - _lastCleanup > 30f)
				{
					_recentKicked.Clear();
					_lastCleanup = Time.time;
				}
				ClientData client = ((InnerNetClient)AmongUsClient.Instance).GetClient(((InnerNetObject)sourcePlayer).OwnerId);
				if (client == null || _recentKicked.Contains(client.Id))
				{
					return;
				}
				string text5 = Normalize(chatText);
				if (string.IsNullOrEmpty(text5))
				{
					return;
				}
				string text6 = null;
				if (_substringWords != null)
				{
					for (int j = 0; j < _substringWords.Length; j++)
					{
						if (text5.IndexOf(_substringWords[j], StringComparison.Ordinal) >= 0)
						{
							text6 = "*" + _substringWords[j];
							break;
						}
					}
				}
				if (text6 == null && _wholeWords != null)
				{
					int length = text5.Length;
					int k = 0;
					while (k < length)
					{
						for (; k < length && !IsWordChar(text5[k]); k++)
						{
						}
						int l;
						for (l = k; l < length && IsWordChar(text5[l]); l++)
						{
						}
						if (l > k)
						{
							int num = l - k;
							for (int m = 0; m < _wholeWords.Length; m++)
							{
								string text7 = _wholeWords[m];
								if (text7.Length == num && string.CompareOrdinal(text5, k, text7, 0, num) == 0)
								{
									text6 = text7;
									break;
								}
							}
							if (text6 != null)
							{
								break;
							}
						}
						k = l;
					}
				}
				if (text6 != null)
				{
					_recentKicked.Add(client.Id);
					((InnerNetClient)AmongUsClient.Instance).KickPlayer(client.Id, false);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[AutoKick] Kicked ");
					object obj = client.PlayerName;
					if (obj == null)
					{
						NetworkedPlayerInfo data = sourcePlayer.Data;
						obj = ((data != null) ? data.PlayerName : null) ?? "???";
					}
					defaultInterpolatedStringHandler.AppendFormatted((string?)obj);
					defaultInterpolatedStringHandler.AppendLiteral(" (chat word '");
					defaultInterpolatedStringHandler.AppendFormatted(text6);
					defaultInterpolatedStringHandler.AppendLiteral("')");
					LogCheat(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			catch
			{
			}
		}

		internal static void Reset()
		{
			_recentKicked.Clear();
			_cachedRaw = null;
			_wholeWords = null;
			_substringWords = null;
			_lastCleanup = 0f;
		}
	}

	[HarmonyPatch(typeof(ChatBubble), "SetName")]
	public static class RevealSusChatPatch
	{
		public static void Prefix(ref string playerName, ref Color color)
		{
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Invalid comparison between Unknown and I4
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Invalid comparison between Unknown and I4
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Invalid comparison between Unknown and I4
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Invalid comparison between Unknown and I4
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Invalid comparison between Unknown and I4
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			if (!IsRevealSusActive || string.IsNullOrEmpty(playerName))
			{
				return;
			}
			try
			{
				Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PlayerControl current = enumerator.Current;
					if (!((Object)(object)((current != null) ? current.Data : null) == (Object)null) && playerName.Contains(current.Data.PlayerName))
					{
						bool flag;
						string value;
						if ((Object)(object)current.Data.Role != (Object)null)
						{
							flag = current.Data.Role.IsImpostor;
							value = GetSpecificRoleName(current.Data.Role);
						}
						else
						{
							RoleTypes roleType = current.Data.RoleType;
							flag = (int)roleType == 1 || (int)roleType == 5 || (int)roleType == 9 || (int)roleType == 18 || (int)roleType == 7;
							value = roleType.ToString().ToUpper();
						}
						string value2 = (flag ? "#FF3333" : "#99CCFF");
						playerName = $"{playerName} <color={value2}>[{value}]</color>";
						if (flag)
						{
							color = new Color(1f, 0.2f, 0.2f, 1f);
						}
						break;
					}
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(HudManager), "Update")]
	public static class AlwaysShowChatPatch
	{
		public static Exception Finalizer(Exception __exception)
		{
			if (__exception is Il2CppException)
			{
				return null;
			}
			return __exception;
		}

		public static void Postfix(HudManager __instance)
		{
			try
			{
				if (++_hudUpdateFrameCounter >= 60)
				{
					_hudUpdateFrameCounter = 0;
					PlayerPickMenu.CheckRealtimeUpdate();
				}
			}
			catch
			{
			}
			bool flag = (Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmHost;
			if (!AlwaysShowChatEnabled && !flag)
			{
				return;
			}
			try
			{
				if (!Object.op_Implicit((Object)(object)__instance))
				{
					return;
				}
				ChatController chat = __instance.Chat;
				if (Object.op_Implicit((Object)(object)chat) && Object.op_Implicit((Object)(object)ShipStatus.Instance) && Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
				{
					GameObject gameObject = ((Component)chat).gameObject;
					if (!gameObject.activeSelf)
					{
						gameObject.SetActive(true);
					}
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(ChatController), "SetVisible")]
	public static class PreventChatHidePatch
	{
		public static bool Prefix(bool visible)
		{
			bool flag = (Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmHost;
			if (!AlwaysShowChatEnabled && !flag)
			{
				return true;
			}
			return visible;
		}
	}

	[HarmonyPatch(typeof(ChatController), "SendChat")]
	public static class BlockChatSendPatch
	{
		public static bool Prefix(ChatController __instance)
		{
			if ((Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				return true;
			}
			if (!AlwaysShowChatEnabled)
			{
				return true;
			}
			bool num = (Object)(object)MeetingHud.Instance != (Object)null;
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			bool? obj;
			if (localPlayer == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo data = localPlayer.Data;
				obj = ((data != null) ? new bool?(data.IsDead) : null);
			}
			bool? flag = obj;
			bool valueOrDefault = flag.GetValueOrDefault();
			bool flag2 = (Object)(object)LobbyBehaviour.Instance != (Object)null;
			if (num || flag2 || valueOrDefault)
			{
				return true;
			}
			if ((Object)(object)__instance != (Object)null)
			{
				FreeChatInputField freeChatField = __instance.freeChatField;
				if (freeChatField != null)
				{
					((AbstractChatInputField)freeChatField).Clear();
				}
				__instance.AddChat(PlayerControl.LocalPlayer, "<color=#FF4444>[MMC] Chat is READ-ONLY outside meetings to prevent kicks!</color>", false);
			}
			return false;
		}
	}

	public static class MapCheats
	{
		private static void _SweepZombies()
		{
			try
			{
				Il2CppArrayBase<ShipStatus> val = Object.FindObjectsOfType<ShipStatus>();
				if (val != null)
				{
					for (int i = 0; i < val.Length; i++)
					{
						ShipStatus val2 = val[i];
						if (!((Object)(object)val2 == (Object)null))
						{
							try
							{
								Object.Destroy((Object)(object)((Component)val2).gameObject);
							}
							catch
							{
							}
						}
					}
				}
			}
			catch
			{
			}
			try
			{
				Il2CppArrayBase<LobbyBehaviour> val3 = Object.FindObjectsOfType<LobbyBehaviour>();
				if (val3 == null)
				{
					return;
				}
				for (int j = 0; j < val3.Length; j++)
				{
					LobbyBehaviour val4 = val3[j];
					if (!((Object)(object)val4 == (Object)null))
					{
						try
						{
							Object.Destroy((Object)(object)((Component)val4).gameObject);
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
		}

		internal static void FullTeardown()
		{
			if ((Object)(object)AmongUsClient.Instance == (Object)null)
			{
				_selMapNetIds = null;
				_selMapSpawnId = -1;
				_mapActive = false;
				_mapNetIds = null;
				_allMapsActive = false;
				_allMapNetIds.Clear();
				return;
			}
			if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				Debug.LogWarning(Object.op_Implicit("[MapCheats] Only the host can teardown map/lobby."));
				return;
			}
			try
			{
				if (_selMapNetIds != null)
				{
					uint[] selMapNetIds = _selMapNetIds;
					for (int i = 0; i < selMapNetIds.Length; i++)
					{
						TrackDestroyedNetId(selMapNetIds[i]);
					}
				}
				if (_mapNetIds != null)
				{
					uint[] selMapNetIds = _mapNetIds;
					for (int i = 0; i < selMapNetIds.Length; i++)
					{
						TrackDestroyedNetId(selMapNetIds[i]);
					}
				}
				foreach (uint[] allMapNetId in _allMapNetIds)
				{
					if (allMapNetId != null)
					{
						uint[] selMapNetIds = allMapNetId;
						for (int i = 0; i < selMapNetIds.Length; i++)
						{
							TrackDestroyedNetId(selMapNetIds[i]);
						}
					}
				}
			}
			catch
			{
			}
			LobbyBehaviour instance = LobbyBehaviour.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				try
				{
					if (instance != null)
					{
						((InnerNetObject)instance).Despawn();
					}
				}
				catch
				{
				}
				try
				{
					Object.Destroy((Object)(object)((Component)instance).gameObject);
				}
				catch
				{
				}
				LobbyBehaviour.Instance = null;
			}
			ShipStatus instance2 = ShipStatus.Instance;
			if ((Object)(object)instance2 != (Object)null)
			{
				try
				{
					if (instance2 != null)
					{
						((InnerNetObject)instance2).Despawn();
					}
				}
				catch
				{
				}
				try
				{
					Object.Destroy((Object)(object)((Component)instance2).gameObject);
				}
				catch
				{
				}
				ShipStatus.Instance = null;
			}
			_SweepZombies();
			_selMapNetIds = null;
			_selMapSpawnId = -1;
			_mapActive = false;
			_mapNetIds = null;
			_allMapsActive = false;
			_allMapNetIds.Clear();
			Debug.Log(Object.op_Implicit("[MapCheats] FullTeardown complete (lobby/ship despawned, zombies swept, trackers flushed)"));
		}

		internal static void DestroyMap()
		{
			FullTeardown();
		}

		internal static void SpawnLobby()
		{
			if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				Debug.LogWarning(Object.op_Implicit("[MapCheats] Only the host can create the lobby."));
				return;
			}
			_SweepZombies();
			LobbyBehaviour.Instance = null;
			if ((Object)(object)LobbyBehaviour.Instance == (Object)null)
			{
				LobbyBehaviour lobbyPrefab = DestroyableSingleton<GameStartManager>.Instance.LobbyPrefab;
				if ((Object)(object)lobbyPrefab != (Object)null)
				{
					LobbyBehaviour.Instance = Object.Instantiate<LobbyBehaviour>(lobbyPrefab);
					((InnerNetClient)AmongUsClient.Instance).Spawn((InnerNetObject)(object)LobbyBehaviour.Instance, -2, (SpawnFlags)0);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
					defaultInterpolatedStringHandler.AppendLiteral("[MapCheats] LobbyBehaviour spawned via prefab NetId=");
					LobbyBehaviour instance = LobbyBehaviour.Instance;
					defaultInterpolatedStringHandler.AppendFormatted((instance != null) ? new uint?(((InnerNetObject)instance).NetId) : null);
					Debug.Log(Object.op_Implicit(defaultInterpolatedStringHandler.ToStringAndClear()));
				}
				else
				{
					Debug.LogWarning(Object.op_Implicit("[MapCheats] LobbyPrefab not found in GameStartManager."));
				}
			}
			else
			{
				Debug.LogWarning(Object.op_Implicit("[MapCheats] LobbyBehaviour already exists."));
			}
		}
	}

	[HarmonyPatch(typeof(MapBehaviour), "Show")]
	public static class LiveMapPatch
	{
		public static void Prefix(MapBehaviour __instance, MapOptions opts)
		{
			try
			{
				opts.ShowLivePlayerPosition = true;
				opts.IncludeDeadBodies = false;
				opts.AllowMovementWhileMapOpen = true;
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(MapBehaviour), "FixedUpdate")]
	public static class MapPlayerDotsPatch
	{
		public static void Postfix(MapBehaviour __instance)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Expected O, but got Unknown
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			ConfigEntry<bool> godMapEnabled = CheatConfig.GodMapEnabled;
			if (godMapEnabled == null || !godMapEnabled.Value || !__instance.IsOpen)
			{
				return;
			}
			try
			{
				ShipStatus instance = ShipStatus.Instance;
				if ((Object)(object)instance == (Object)null)
				{
					return;
				}
				float mapScale = instance.MapScale;
				float num = Mathf.Sign(((Component)instance).transform.localScale.x);
				List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
				if (allPlayerControls == null)
				{
					return;
				}
				int num2 = 0;
				for (int i = 0; i < allPlayerControls.Count; i++)
				{
					PlayerControl val = allPlayerControls[i];
					if ((Object)(object)val == (Object)null || (Object)(object)val.Data == (Object)null || val.Data.IsDead || val.Data.Disconnected || ((InnerNetObject)val).AmOwner)
					{
						continue;
					}
					SpriteRenderer val2;
					if (num2 < _mapPlayerDots.Count)
					{
						val2 = _mapPlayerDots[num2];
						if ((Object)(object)val2 == (Object)null)
						{
							val2 = CreateDot(__instance);
							_mapPlayerDots[num2] = val2;
						}
					}
					else
					{
						val2 = CreateDot(__instance);
						_mapPlayerDots.Add(val2);
					}
					Vector3 position = ((Component)val).transform.position;
					position /= mapScale;
					position.x *= num;
					position.z = -1f;
					((Component)val2).transform.localPosition = position;
					((Component)val2).gameObject.SetActive(true);
					PlayerMaterial.SetColors(val.Data.DefaultOutfit.ColorId, (Renderer)val2);
					bool flag = (Object)(object)val.Data.Role != (Object)null && val.Data.Role.IsImpostor;
					((Renderer)val2).material.SetFloat("_Outline", flag ? 1f : 0f);
					if (flag)
					{
						((Renderer)val2).material.SetColor("_OutlineColor", Color.red);
					}
					num2++;
				}
				for (int j = num2; j < _mapPlayerDots.Count; j++)
				{
					if ((Object)(object)_mapPlayerDots[j] != (Object)null)
					{
						((Component)_mapPlayerDots[j]).gameObject.SetActive(false);
					}
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(MapBehaviour), "Close")]
	public static class MapCloseClearDotsPatch
	{
		public static void Postfix()
		{
			for (int i = 0; i < _mapPlayerDots.Count; i++)
			{
				if ((Object)(object)_mapPlayerDots[i] != (Object)null)
				{
					((Component)_mapPlayerDots[i]).gameObject.SetActive(false);
				}
			}
		}
	}

	[HarmonyPatch(typeof(SabotageSystemType), "Deteriorate")]
	public static class NoSabotageCooldownPatch
	{
		public static void Prefix(SabotageSystemType __instance)
		{
			try
			{
				ConfigEntry<bool> noSabotageCooldown = CheatConfig.NoSabotageCooldown;
				if (noSabotageCooldown != null && noSabotageCooldown.Value)
				{
					__instance.Timer = 0f;
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(MapBehaviour), "Show")]
	public static class CrewmateSabotagePatch
	{
		public static void Prefix(MapBehaviour __instance, MapOptions opts)
		{
			ConfigEntry<bool> crewmateSabotage = CheatConfig.CrewmateSabotage;
			if (crewmateSabotage != null && crewmateSabotage.Value && (Object)(object)PlayerControl.LocalPlayer != (Object)null && (Object)(object)PlayerControl.LocalPlayer.Data.Role != (Object)null && !PlayerControl.LocalPlayer.Data.Role.IsImpostor)
			{
				opts.Mode = (Modes)3;
			}
		}
	}

	[HarmonyPatch(typeof(SabotageSystemType), "get_AnyActive")]
	public static class MultiSabotagePatch
	{
		public static bool Prefix(ref bool __result)
		{
			ConfigEntry<bool> multiSabotage = CheatConfig.MultiSabotage;
			if (multiSabotage != null && multiSabotage.Value)
			{
				__result = false;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(SabotageSystemType), "UpdateSystem")]
	public static class MultiSabotageBypassPatch
	{
		public static void Prefix(SabotageSystemType __instance)
		{
			try
			{
				ConfigEntry<bool> multiSabotage = CheatConfig.MultiSabotage;
				if (multiSabotage != null && multiSabotage.Value && __instance != null)
				{
					__instance.Timer = 0f;
				}
			}
			catch
			{
			}
		}
	}

	internal static class BodyTypeCheats
	{
		internal static readonly PlayerBodyTypes[] AvailableTypes;

		internal static int CurrentTypeIndex;

		internal static void SetAllPlayersBodyType(PlayerBodyTypes bodyType)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PlayerControl current = enumerator.Current;
					if ((Object)(object)((current != null) ? current.MyPhysics : null) != (Object)null)
					{
						current.MyPhysics.SetBodyType(bodyType);
					}
				}
				LogCheat($"Body type set to {bodyType} for all players");
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[BodyTypeCheats] Error: {value}"));
			}
		}

		internal static void SetLocalBodyType(PlayerBodyTypes bodyType)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				PlayerControl localPlayer = PlayerControl.LocalPlayer;
				if ((Object)(object)((localPlayer != null) ? localPlayer.MyPhysics : null) != (Object)null)
				{
					localPlayer.MyPhysics.SetBodyType(bodyType);
					LogCheat($"Local body type set to {bodyType}");
				}
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[BodyTypeCheats] Error: {value}"));
			}
		}

		internal static PlayerBodyTypes CycleBodyType()
		{
			CurrentTypeIndex = (CurrentTypeIndex + 1) % AvailableTypes.Length;
			return AvailableTypes[CurrentTypeIndex];
		}

		public static string GetBodyTypeName(PlayerBodyTypes type)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected I4, but got Unknown
			return (int)type switch
			{
				0 => "Normal", 
				3 => "Long", 
				1 => "Horse", 
				2 => "Seeker", 
				4 => "Long Seeker", 
				_ => "Unknown", 
			};
		}

		public static PlayerBodyTypes GetCurrentType()
		{
			return AvailableTypes[CurrentTypeIndex];
		}

		static BodyTypeCheats()
		{
			PlayerBodyTypes[] array = new PlayerBodyTypes[5];
			RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
			AvailableTypes = (PlayerBodyTypes[])(object)array;
			CurrentTypeIndex = 0;
		}
	}

	[HarmonyPatch(typeof(MeetingHud), "Start")]
	public static class RevealVotesStartPatch
	{
		public static void Postfix()
		{
			_revealedVoters.Clear();
		}
	}

	[HarmonyPatch(typeof(MeetingHud), "Close")]
	public static class RevealVotesClosePatch
	{
		public static void Prefix()
		{
			_revealedVoters.Clear();
		}
	}

	[HarmonyPatch(typeof(MeetingHud), "PopulateResults")]
	public static class ClearVotesBeforeResultsPatch
	{
		public static void Prefix(MeetingHud __instance)
		{
			try
			{
				if (((__instance != null) ? __instance.playerStates : null) == null)
				{
					return;
				}
				Enumerator<SpriteRenderer> enumerator2;
				foreach (PlayerVoteArea item in (Il2CppArrayBase<PlayerVoteArea>)(object)__instance.playerStates)
				{
					if ((Object)(object)item == (Object)null)
					{
						continue;
					}
					VoteSpreader componentInChildren = ((Component)item).GetComponentInChildren<VoteSpreader>();
					if (((componentInChildren != null) ? componentInChildren.Votes : null) == null)
					{
						continue;
					}
					enumerator2 = componentInChildren.Votes.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						SpriteRenderer current2 = enumerator2.Current;
						if ((Object)(object)current2 != (Object)null)
						{
							Object.Destroy((Object)(object)((Component)current2).gameObject);
						}
					}
					componentInChildren.Votes.Clear();
				}
				if (!((Object)(object)__instance.SkippedVoting != (Object)null))
				{
					return;
				}
				VoteSpreader componentInChildren2 = __instance.SkippedVoting.GetComponentInChildren<VoteSpreader>();
				if (((componentInChildren2 != null) ? componentInChildren2.Votes : null) == null)
				{
					return;
				}
				enumerator2 = componentInChildren2.Votes.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					SpriteRenderer current3 = enumerator2.Current;
					if ((Object)(object)current3 != (Object)null)
					{
						Object.Destroy((Object)(object)((Component)current3).gameObject);
					}
				}
				componentInChildren2.Votes.Clear();
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(MeetingHud), "Update")]
	public static class RevealVotesUpdatePatch
	{
		public static bool Prefix(MeetingHud __instance)
		{
			try
			{
				if ((Object)(object)__instance == (Object)null)
				{
					return false;
				}
				if (__instance.playerStates == null)
				{
					return false;
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static void Postfix(MeetingHud __instance)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Invalid comparison between Unknown and I4
			try
			{
				ConfigEntry<bool> revealVotes = CheatConfig.RevealVotes;
				if (revealVotes == null || !revealVotes.Value || (Object)(object)__instance == (Object)null || __instance.playerStates == null || (int)__instance.state >= 4)
				{
					return;
				}
				foreach (PlayerVoteArea item in (Il2CppArrayBase<PlayerVoteArea>)(object)__instance.playerStates)
				{
					if ((Object)(object)item == (Object)null)
					{
						continue;
					}
					byte targetPlayerId = item.TargetPlayerId;
					byte votedFor = item.VotedFor;
					if (votedFor == byte.MaxValue || votedFor == 254 || votedFor == 252 || _revealedVoters.Contains(targetPlayerId))
					{
						continue;
					}
					NetworkedPlayerInfo val = null;
					int voterColorId = 0;
					Enumerator<PlayerControl> enumerator2 = PlayerControl.AllPlayerControls.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						PlayerControl current2 = enumerator2.Current;
						if ((Object)(object)current2 != (Object)null && current2.PlayerId == targetPlayerId)
						{
							val = current2.Data;
							if ((Object)(object)current2.cosmetics != (Object)null)
							{
								voterColorId = current2.cosmetics.ColorId;
							}
							break;
						}
					}
					if ((Object)(object)val == (Object)null)
					{
						continue;
					}
					_revealedVoters.Add(targetPlayerId);
					Transform val2 = null;
					if (votedFor == 253)
					{
						if ((Object)(object)__instance.SkippedVoting != (Object)null)
						{
							val2 = __instance.SkippedVoting.transform;
							__instance.BloopAVoteIcon(val, 0, val2);
							__instance.SkippedVoting.SetActive(true);
						}
					}
					else
					{
						foreach (PlayerVoteArea item2 in (Il2CppArrayBase<PlayerVoteArea>)(object)__instance.playerStates)
						{
							if ((Object)(object)item2 != (Object)null && item2.TargetPlayerId == votedFor)
							{
								val2 = ((Component)item2).transform;
								__instance.BloopAVoteIcon(val, 0, val2);
								break;
							}
						}
					}
					if ((Object)(object)val2 != (Object)null)
					{
						RevealAnonymousVoteColors(val2, voterColorId);
					}
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(KillButton), "DoClick")]
	public static class NoKillCooldownDoClickPatch
	{
		public static bool Prefix(KillButton __instance)
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			ConfigEntry<bool> noKillCooldown = CheatConfig.NoKillCooldown;
			if (noKillCooldown == null || !noKillCooldown.Value)
			{
				return true;
			}
			if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				return true;
			}
			if (!LocalPlayerCanKill())
			{
				return true;
			}
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if ((Object)(object)localPlayer == (Object)null || (Object)(object)localPlayer.Data == (Object)null || localPlayer.Data.IsDead)
			{
				return true;
			}
			PlayerControl currentTarget = __instance.currentTarget;
			if ((Object)(object)currentTarget == (Object)null || (Object)(object)currentTarget.Data == (Object)null || currentTarget.Data.IsDead)
			{
				return true;
			}
			Vector2 killPos = Vector2.op_Implicit(((Component)currentTarget).transform.position);
			localPlayer.killTimer = 0f;
			HostForceKillPlayer(currentTarget);
			localPlayer.killTimer = 0f;
			if (KillAndVentEnabled)
			{
				try
				{
					SmartVentAfterKill(killPos);
				}
				catch
				{
				}
			}
			try
			{
				((ActionButton)__instance).SetCoolDown(0f, 1f);
				((ActionButton)__instance).canInteract = true;
			}
			catch
			{
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "SetKillTimer")]
	public static class NoKillCooldownSetTimerPatch
	{
		public static void Prefix(PlayerControl __instance, ref float time)
		{
			ConfigEntry<bool> noKillCooldown = CheatConfig.NoKillCooldown;
			if (noKillCooldown != null && noKillCooldown.Value && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost && (Object)(object)__instance == (Object)(object)PlayerControl.LocalPlayer && LocalPlayerCanKill())
			{
				time = 0f;
			}
		}

		public static void Postfix(PlayerControl __instance)
		{
			ConfigEntry<bool> noKillCooldown = CheatConfig.NoKillCooldown;
			if (noKillCooldown != null && noKillCooldown.Value && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost && (Object)(object)__instance == (Object)(object)PlayerControl.LocalPlayer && LocalPlayerCanKill())
			{
				__instance.killTimer = 0f;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "FixedUpdate")]
	public static class NoKillCooldownFixedUpdatePatch
	{
		public static void Postfix(PlayerControl __instance)
		{
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)__instance != (Object)(object)PlayerControl.LocalPlayer)
			{
				return;
			}
			ConfigEntry<bool> noKillCooldown = CheatConfig.NoKillCooldown;
			if (noKillCooldown == null || !noKillCooldown.Value || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost || !LocalPlayerCanKill())
			{
				return;
			}
			__instance.killTimer = 0f;
			try
			{
				HudManager instance = DestroyableSingleton<HudManager>.Instance;
				KillButton val = ((instance != null) ? instance.KillButton : null);
				if ((Object)(object)val != (Object)null)
				{
					((ActionButton)val).SetCoolDown(0f, 1f);
					((ActionButton)val).canInteract = true;
					if ((Object)(object)((ActionButton)val).graphic != (Object)null)
					{
						((ActionButton)val).graphic.color = Color.white;
					}
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "MurderPlayer")]
	public static class NoKillCooldownMurderPatch
	{
		public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
		{
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			if (KillAndVentEnabled && (Object)(object)__instance == (Object)(object)PlayerControl.LocalPlayer && (Object)(object)target != (Object)null && (Object)(object)target.Data != (Object)null && target.Data.IsDead)
			{
				ConfigEntry<bool> noKillCooldown = CheatConfig.NoKillCooldown;
				if (noKillCooldown != null && noKillCooldown.Value)
				{
					AmongUsClient instance = AmongUsClient.Instance;
					if (instance != null && ((InnerNetClient)instance).AmHost)
					{
						goto IL_007b;
					}
				}
				try
				{
					SmartVentAfterKill(Vector2.op_Implicit(((Component)target).transform.position));
				}
				catch
				{
				}
			}
			goto IL_007b;
			IL_007b:
			ConfigEntry<bool> noKillCooldown2 = CheatConfig.NoKillCooldown;
			if (noKillCooldown2 != null && noKillCooldown2.Value && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost && (Object)(object)__instance == (Object)(object)PlayerControl.LocalPlayer)
			{
				__instance.killTimer = 0f;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "MurderPlayer")]
	public static class RemoteCooldownResetPatch
	{
		public static void Postfix(PlayerControl __instance)
		{
			if ((Object)(object)__instance == (Object)(object)PlayerControl.LocalPlayer)
			{
				ConfigEntry<bool> noKillCooldown = CheatConfig.NoKillCooldown;
				if (noKillCooldown != null && noKillCooldown.Value)
				{
					return;
				}
			}
			if ((Object)(object)__instance.Data != (Object)null && (Object)(object)__instance.Data.Role != (Object)null && __instance.Data.Role.IsImpostor)
			{
				float @float = GameOptionsManager.Instance.CurrentGameOptions.GetFloat((FloatOptionNames)1);
				__instance.killTimer = @float;
			}
		}
	}

	[HarmonyPatch(typeof(ExileController), "ReEnableGameplay")]
	public static class ResetCooldownsPostMeetingPatch
	{
		public static void Postfix()
		{
			float @float = GameOptionsManager.Instance.CurrentGameOptions.GetFloat((FloatOptionNames)1);
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current.Data != (Object)null && (Object)(object)current.Data.Role != (Object)null && current.Data.Role.IsImpostor && !current.Data.IsDead)
				{
					current.killTimer = @float;
				}
			}
			LogCheat("[Sync] Kill cooldowns reset after meeting.");
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "OnGameStart")]
	public static class ResetCooldownsOnStartPatch
	{
		public static void Postfix()
		{
			CheatConfig.GodMode = false;
			CheatConfig.GodModeAll = false;
			float killTimer = 10f;
			if (GameOptionsManager.Instance != null && GameOptionsManager.Instance.CurrentGameOptions != null)
			{
				killTimer = GameOptionsManager.Instance.CurrentGameOptions.GetFloat((FloatOptionNames)1);
			}
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current.Data != (Object)null && (Object)(object)current.Data.Role != (Object)null && current.Data.Role.IsImpostor)
				{
					current.killTimer = killTimer;
				}
			}
		}
	}

	[HarmonyPatch(typeof(InnerNetClient), "Spawn", new System.Type[]
	{
		typeof(InnerNetObject),
		typeof(int),
		typeof(SpawnFlags)
	})]
	public static class TrackVanillaSpawnNetIdPatch
	{
		public static void Postfix(InnerNetObject netObjParent)
		{
			try
			{
				if ((Object)(object)netObjParent == (Object)null)
				{
					return;
				}
				TrackVanillaNetId(netObjParent.NetId);
				Il2CppArrayBase<InnerNetObject> componentsInChildren = ((Component)netObjParent).GetComponentsInChildren<InnerNetObject>();
				if (componentsInChildren == null)
				{
					return;
				}
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if ((Object)(object)componentsInChildren[i] != (Object)null)
					{
						TrackVanillaNetId(componentsInChildren[i].NetId);
					}
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(InnerNetClient), "AddNetObject")]
	public static class TrackAddNetObjectPatch
	{
		public static void Postfix(InnerNetObject obj)
		{
			try
			{
				if ((Object)(object)obj != (Object)null)
				{
					TrackVanillaNetId(obj.NetId);
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(InnerNetClient), "Despawn", new System.Type[] { typeof(InnerNetObject) })]
	public static class TrackDespawnNetIdPatch
	{
		public static void Postfix(InnerNetObject objToDespawn)
		{
			try
			{
				if ((Object)(object)objToDespawn != (Object)null)
				{
					TrackDestroyedNetId(objToDespawn.NetId);
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(GameManager), "StartGame")]
	public static class DisableLabTogglesOnHostStartPatch
	{
		public static void Prefix()
		{
			try
			{
				AmongUsClient instance = AmongUsClient.Instance;
				if ((Object)(object)instance == (Object)null || !((InnerNetClient)instance).AmHost)
				{
					return;
				}
				bool flag = false;
				if (_dummyActive)
				{
					ToggleDummy();
					flag = true;
					LogCheat("[LAB] Ghost Twins force-disabled on host game start.");
				}
				if (_cfActive)
				{
					ToggleCloneFollow();
					flag = true;
					LogCheat("[LAB] Shadow Clones force-disabled on host game start.");
				}
				if (!flag)
				{
					return;
				}
				try
				{
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
				catch
				{
				}
			}
			catch (Exception ex)
			{
				LogCheat("[LAB] Force-disable failed: " + ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(GameStartManager), "ReallyBegin")]
	public static class CleanupLabOnCountdownStartPatch
	{
		public static void Postfix()
		{
			try
			{
				AmongUsClient instance = AmongUsClient.Instance;
				if ((Object)(object)instance == (Object)null || !((InnerNetClient)instance).AmHost)
				{
					return;
				}
				List<uint> list = new List<uint>(256);
				if (_cfActive)
				{
					foreach (KeyValuePair<byte, List<_CFState>> cfState in _cfStates)
					{
						if (cfState.Value == null)
						{
							continue;
						}
						foreach (_CFState item3 in cfState.Value)
						{
							if (item3?.netIds != null)
							{
								uint[] netIds = item3.netIds;
								foreach (uint item in netIds)
								{
									list.Add(item);
								}
							}
						}
					}
				}
				if (_dummyActive)
				{
					foreach (DummyEntry dummyNetIds in _dummyNetIdsList)
					{
						if (dummyNetIds.NetIds != null)
						{
							uint[] netIds = dummyNetIds.NetIds;
							foreach (uint item2 in netIds)
							{
								list.Add(item2);
							}
						}
					}
				}
				bool flag = false;
				if (_cfActive)
				{
					_cfActive = false;
					_cfActivatedByOrbit = false;
					_cfStates.Clear();
					_cfSynced.Clear();
					_cfReconcilePending.Clear();
					_orbitRoundRobinCursor = 0;
					_cfSpawnQueue.Clear();
					LabRpcArbiter.Unregister(LabRpcArbiter.Consumer.ShadowClones);
					flag = true;
					LogCheat("[LAB] Shadow Clones / Orbit toggle flipped OFF at countdown.");
				}
				if (_dummyActive)
				{
					_dummyActive = false;
					_dummyNetIdsList.Clear();
					_dummySynced.Clear();
					LabRpcArbiter.Unregister(LabRpcArbiter.Consumer.GhostTwins);
					flag = true;
					LogCheat("[LAB] Ghost Twins toggle flipped OFF at countdown.");
				}
				if (_rainbowEnabled || _playerRainbowPhases.Count > 0 || _colorBurstQ.Count > 0)
				{
					_rainbowEnabled = false;
					_rainbowPhase = 0;
					_rainbowPlayerIdx = 0;
					_playerRainbowPhases.Clear();
					_colorBurstQ.Clear();
					flag = true;
					LogCheat("[LAB] Rainbow halted at countdown.");
				}
				if (flag)
				{
					try
					{
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					}
					catch
					{
					}
				}
				if (list.Count > 0 && DestroyableSingleton<HudManager>.InstanceExists)
				{
					MonoBehaviourExtensions.StartCoroutine((MonoBehaviour)(object)DestroyableSingleton<HudManager>.Instance, _StaggeredDespawnCoroutine(list));
					LogCheat($"[LAB] Staggered despawn started: {list.Count} NetIds at safe rate.");
				}
			}
			catch (Exception ex)
			{
				LogCheat("[LAB] Countdown deactivation failed: " + ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(LogicGameFlowNormal), "CheckEndCriteria")]
	public static class DisableGameEndNormalPatch
	{
		public static bool Prefix(LogicGameFlowNormal __instance)
		{
			if (!ModMenuCrewPlugin.DisableGameEndFallback)
			{
				ConfigEntry<bool> disableGameEnd = CheatConfig.DisableGameEnd;
				if (disableGameEnd == null || !disableGameEnd.Value)
				{
					return true;
				}
			}
			if (ShouldAllowGameEnd())
			{
				return true;
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(LogicGameFlowHnS), "CheckEndCriteria")]
	public static class DisableGameEndHnSPatch
	{
		public static bool Prefix(LogicGameFlowHnS __instance)
		{
			if (!ModMenuCrewPlugin.DisableGameEndFallback)
			{
				ConfigEntry<bool> disableGameEnd = CheatConfig.DisableGameEnd;
				if (disableGameEnd == null || !disableGameEnd.Value)
				{
					return true;
				}
			}
			if (ShouldAllowGameEnd())
			{
				return true;
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(GameManager), "CheckEndGameViaTasks")]
	public static class DisableGameEndTaskPatch
	{
		public static bool Prefix(GameManager __instance)
		{
			if (!ModMenuCrewPlugin.DisableGameEndFallback)
			{
				ConfigEntry<bool> disableGameEnd = CheatConfig.DisableGameEnd;
				if (disableGameEnd == null || !disableGameEnd.Value)
				{
					return true;
				}
			}
			if (ShouldAllowGameEnd())
			{
				return true;
			}
			return false;
		}
	}

	public struct ForceVotesPlayerInfo
	{
		public byte PlayerId;

		public string Name;
	}

	[HarmonyPatch(typeof(MeetingHud), "Start")]
	public static class ForceVotesMeetingStartPatch
	{
		public static void Postfix()
		{
			try
			{
				ForceVotesRefreshPlayerList();
				ForceVotesSelectedTargetIndex = 0;
				Debug.Log(Object.op_Implicit("[ForceVotes] Meeting started, player list refreshed"));
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[ForceVotes] Error in MeetingStartPatch: {value}"));
			}
		}
	}

	[HarmonyPatch(typeof(MeetingHud), "Close")]
	public static class ForceVotesMeetingClosePatch
	{
		public static void Prefix()
		{
			ForceVotesAlivePlayers.Clear();
			ForceVotesSelectedTargetIndex = 0;
		}
	}

	[HarmonyPatch(typeof(PlayerControl), "HandleRpc")]
	public static class RpcSyncValidator
	{
		private static readonly Dictionary<byte, int> _c0 = new Dictionary<byte, int>();

		private static readonly Dictionary<byte, float> _t0 = new Dictionary<byte, float>();

		private static readonly HashSet<byte> _b0 = new HashSet<byte>();

		private static readonly Dictionary<byte, float> _e0 = new Dictionary<byte, float>();

		private static int _gc = 0;

		private static float _gt = 0f;

		private static bool _gf = false;

		private static float _ge = 0f;

		private static readonly Dictionary<byte, int> _c1 = new Dictionary<byte, int>();

		private static readonly Dictionary<byte, float> _t1 = new Dictionary<byte, float>();

		private static readonly HashSet<byte> _b1 = new HashSet<byte>();

		private static readonly Dictionary<byte, float> _e1 = new Dictionary<byte, float>();

		private static int _gc1 = 0;

		private static float _gt1 = 0f;

		private static bool _gf1 = false;

		private static float _ge1 = 0f;

		private static int _fr1 = -1;

		private static int _frg1 = 0;

		private static readonly Dictionary<byte, int> _frp1 = new Dictionary<byte, int>();

		internal static bool FilterAttackRpc(byte p, byte callId)
		{
			try
			{
				if (CheatConfig.Nf7 == null || !CheatConfig.Nf7.Value)
				{
					return true;
				}
				ServerData.SecurityConfig config = ServerData.Config;
				if (config == null)
				{
					return true;
				}
				if (!config.IsInFilterB(callId))
				{
					return true;
				}
				PlayerControl localPlayer = PlayerControl.LocalPlayer;
				if ((Object)(object)localPlayer != (Object)null && p == localPlayer.PlayerId)
				{
					return true;
				}
				float unscaledTime = Time.unscaledTime;
				int frameCount = Time.frameCount;
				if (frameCount != _fr1)
				{
					_fr1 = frameCount;
					_frg1 = 0;
					_frp1.Clear();
				}
				if (_b1.Contains(p))
				{
					if (!_e1.TryGetValue(p, out var value) || !(unscaledTime >= value))
					{
						return false;
					}
					_b1.Remove(p);
					_e1.Remove(p);
					_c1.Remove(p);
					_t1.Remove(p);
				}
				if (_gf1)
				{
					if (!(unscaledTime >= _ge1))
					{
						return false;
					}
					_gf1 = false;
					_gc1 = 0;
				}
				int num = ((config.RpcFrameGlobalB > 0) ? config.RpcFrameGlobalB : 20);
				if (_frg1 >= num)
				{
					return false;
				}
				int num2 = ((config.RpcFramePlayerB > 0) ? config.RpcFramePlayerB : 8);
				if (_frp1.TryGetValue(p, out var value2) && value2 >= num2)
				{
					return false;
				}
				if (unscaledTime - _gt1 >= 1f)
				{
					_gt1 = unscaledTime;
					_gc1 = 1;
				}
				else
				{
					_gc1++;
					int num3 = ((config.RpcGlobalLimitB > 0) ? config.RpcGlobalLimitB : 40);
					if (_gc1 > num3)
					{
						float num4 = ((config.RpcGlobalBanSecB > 0f) ? config.RpcGlobalBanSecB : 10f);
						_gf1 = true;
						_ge1 = unscaledTime + num4;
						return false;
					}
				}
				if (!_t1.TryGetValue(p, out var value3))
				{
					_t1[p] = unscaledTime;
					_c1[p] = 1;
					_frg1++;
					_frp1[p] = (_frp1.TryGetValue(p, out var value4) ? value4 : 0) + 1;
					return true;
				}
				if (unscaledTime - value3 >= 1f)
				{
					_t1[p] = unscaledTime;
					_c1[p] = 1;
					_frg1++;
					_frp1[p] = (_frp1.TryGetValue(p, out var value5) ? value5 : 0) + 1;
					return true;
				}
				int value6;
				int num5 = (_c1.TryGetValue(p, out value6) ? value6 : 0) + 1;
				_c1[p] = num5;
				int num6 = ((config.RpcPlayerLimitB > 0) ? config.RpcPlayerLimitB : 12);
				if (num5 > num6)
				{
					float num7 = ((config.RpcPlayerBanSecB > 0f) ? config.RpcPlayerBanSecB : 20f);
					_b1.Add(p);
					_e1[p] = unscaledTime + num7;
					return false;
				}
				_frg1++;
				_frp1[p] = (_frp1.TryGetValue(p, out var value7) ? value7 : 0) + 1;
				return true;
			}
			catch
			{
				return true;
			}
		}

		public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
		{
			try
			{
				return _Pi(__instance, callId);
			}
			catch
			{
				return true;
			}
		}

		private static bool _Pi(PlayerControl __instance, byte callId)
		{
			if ((Object)(object)__instance == (Object)null)
			{
				return true;
			}
			byte playerId = __instance.PlayerId;
			float unscaledTime = Time.unscaledTime;
			ServerData.SecurityConfig config = ServerData.Config;
			if (config == null)
			{
				return true;
			}
			bool flag = CheatConfig.Nf4 != null && CheatConfig.Nf4.Value;
			if (CheatConfig.Nf7 != null && CheatConfig.Nf7.Value)
			{
				if (!FilterAttackRpc(playerId, callId))
				{
					return false;
				}
				if (config.IsInFilterB(callId))
				{
					return true;
				}
			}
			if (!flag)
			{
				return true;
			}
			if (_b0.Contains(playerId))
			{
				if (!_e0.TryGetValue(playerId, out var value) || !(unscaledTime >= value))
				{
					return false;
				}
				_b0.Remove(playerId);
				_e0.Remove(playerId);
				_c0.Remove(playerId);
				_t0.Remove(playerId);
			}
			HashSet<byte> rpcFilterIds = config.RpcFilterIds;
			if (rpcFilterIds == null || rpcFilterIds.Count == 0 || !rpcFilterIds.Contains(callId))
			{
				return true;
			}
			if (_gf)
			{
				if (!(unscaledTime >= _ge))
				{
					return false;
				}
				_gf = false;
				_gc = 0;
			}
			if (unscaledTime - _gt >= 1f)
			{
				_gt = unscaledTime;
				_gc = 1;
			}
			else
			{
				_gc++;
				if (_gc > ((config.RpcGlobalLimit > 0) ? config.RpcGlobalLimit : 10))
				{
					_gf = true;
					_ge = unscaledTime + ((config.RpcGlobalBanSec > 0f) ? config.RpcGlobalBanSec : 5f);
					return false;
				}
			}
			if (!_t0.TryGetValue(playerId, out var value2))
			{
				_t0[playerId] = unscaledTime;
				_c0[playerId] = 1;
				return true;
			}
			if (unscaledTime - value2 >= 1f)
			{
				_t0[playerId] = unscaledTime;
				_c0[playerId] = 1;
				return true;
			}
			int value3;
			int num = (_c0.TryGetValue(playerId, out value3) ? value3 : 0) + 1;
			_c0[playerId] = num;
			if (num > ((config.RpcPlayerLimit > 0) ? config.RpcPlayerLimit : 5))
			{
				_b0.Add(playerId);
				_e0[playerId] = unscaledTime + ((config.RpcPlayerBanSec > 0f) ? config.RpcPlayerBanSec : 10f);
				return false;
			}
			return true;
		}

		internal static void Reset()
		{
			_c0.Clear();
			_t0.Clear();
			_b0.Clear();
			_e0.Clear();
			_gc = 0;
			_c1.Clear();
			_t1.Clear();
			_b1.Clear();
			_e1.Clear();
			_gc1 = 0;
			_gt1 = 0f;
			_gf1 = false;
			_ge1 = 0f;
			_fr1 = -1;
			_frg1 = 0;
			_frp1.Clear();
			_gt = 0f;
			_gf = false;
			_ge = 0f;
		}
	}

	[HarmonyPatch(typeof(PlayerPhysics), "HandleRpc")]
	public static class RpcPhysicsValidator
	{
		public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId)
		{
			try
			{
				if ((Object)(object)__instance == (Object)null)
				{
					return true;
				}
				if (CheatConfig.Nf7 == null || !CheatConfig.Nf7.Value)
				{
					return true;
				}
				ServerData.SecurityConfig config = ServerData.Config;
				if (config == null)
				{
					return true;
				}
				if (!config.IsInFilterB(callId))
				{
					return true;
				}
				byte b = byte.MaxValue;
				try
				{
					uint netId = ((InnerNetObject)__instance).NetId;
					List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
					if (allPlayerControls != null)
					{
						for (int i = 0; i < allPlayerControls.Count; i++)
						{
							PlayerControl val = allPlayerControls[i];
							if ((Object)(object)val != (Object)null && (Object)(object)val.MyPhysics != (Object)null && ((InnerNetObject)val.MyPhysics).NetId == netId)
							{
								b = val.PlayerId;
								break;
							}
						}
					}
				}
				catch
				{
				}
				if (b == byte.MaxValue)
				{
					return true;
				}
				return RpcSyncValidator.FilterAttackRpc(b, callId);
			}
			catch
			{
				return true;
			}
		}
	}

	private struct MapBounds
	{
		public float minX;

		public float maxX;

		public float minY;

		public float maxY;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass192_0
	{
		public PlayerTask task;

		internal bool _003CCompleteAllTasksWithDelay_003Eb__0(PlayerTask t)
		{
			return t.TaskId == task.Id;
		}
	}

	[CompilerGenerated]
	private sealed class _003CBypassScannerWithTimeout_003Ed__200 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CBypassScannerWithTimeout_003Ed__200(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				BypassScanner(value: true);
				_003C_003E2__current = (object)new WaitForSeconds(duration);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				BypassScanner(value: false);
				return false;
			}
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCoSmartVentAfterKill_003Ed__576 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Vector2 killPos;

		private PlayerControl _003ClocalPlayer_003E5__2;

		private float _003Ctimeout_003E5__3;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoSmartVentAfterKill_003Ed__576(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003ClocalPlayer_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_020f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_036b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0370: Unknown result type (might be due to invalid IL or missing references)
			//IL_0375: Unknown result type (might be due to invalid IL or missing references)
			//IL_0382: Unknown result type (might be due to invalid IL or missing references)
			//IL_0389: Unknown result type (might be due to invalid IL or missing references)
			//IL_039b: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0402: Unknown result type (might be due to invalid IL or missing references)
			//IL_040c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_025b: Unknown result type (might be due to invalid IL or missing references)
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003ClocalPlayer_003E5__2 = PlayerControl.LocalPlayer;
				if ((Object)(object)_003ClocalPlayer_003E5__2 == (Object)null || (Object)(object)_003ClocalPlayer_003E5__2.Data == (Object)null)
				{
					return false;
				}
				if ((Object)(object)ShipStatus.Instance == (Object)null)
				{
					return false;
				}
				_003Ctimeout_003E5__3 = Time.realtimeSinceStartup + 0.7f;
				goto IL_00a7;
			case 1:
				_003C_003E1__state = -1;
				goto IL_00a7;
			case 2:
				{
					_003C_003E1__state = -1;
					if ((Object)(object)_003ClocalPlayer_003E5__2 == (Object)null || (Object)(object)_003ClocalPlayer_003E5__2.Data == (Object)null || _003ClocalPlayer_003E5__2.Data.IsDead)
					{
						return false;
					}
					if ((Object)(object)ShipStatus.Instance == (Object)null)
					{
						return false;
					}
					Il2CppReferenceArray<Vent> allVents = ShipStatus.Instance.AllVents;
					if (allVents == null || ((Il2CppArrayBase<Vent>)(object)allVents).Length == 0)
					{
						Debug.LogWarning(Object.op_Implicit("[KillAndVent] No vents on this map."));
						return false;
					}
					List<Vector2> list = new List<Vector2>(15);
					Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
					while (enumerator.MoveNext())
					{
						PlayerControl current = enumerator.Current;
						if (!((Object)(object)current == (Object)null) && !((Object)(object)current.Data == (Object)null) && !current.Data.IsDead && !current.Data.Disconnected && current.PlayerId != _003ClocalPlayer_003E5__2.PlayerId)
						{
							try
							{
								list.Add(Vector2.op_Implicit(((Component)current).transform.position));
							}
							catch
							{
							}
						}
					}
					Vent val = null;
					float num = float.MinValue;
					Vent val2 = null;
					float num2 = float.MinValue;
					foreach (Vent item in (Il2CppArrayBase<Vent>)(object)allVents)
					{
						if ((Object)(object)item == (Object)null)
						{
							continue;
						}
						Vector2 val3;
						try
						{
							val3 = Vector2.op_Implicit(((Component)item).transform.position);
						}
						catch
						{
							continue;
						}
						if (Vector2.Distance(val3, killPos) < 2f)
						{
							continue;
						}
						float num3 = float.MaxValue;
						float num4 = 0f;
						if (list.Count > 0)
						{
							for (int i = 0; i < list.Count; i++)
							{
								float num5 = Vector2.Distance(val3, list[i]);
								if (num5 < num3)
								{
									num3 = num5;
								}
								num4 += num5;
							}
						}
						else
						{
							num3 = 999f;
							num4 = 999f;
						}
						float num6 = ((list.Count > 0) ? (num4 / (float)list.Count) : 999f);
						if (num3 > num2)
						{
							num2 = num3;
							val2 = item;
						}
						if (!(num3 < 4f))
						{
							float num7 = num3 * 2f + num6 * 1f;
							if ((Object)(object)item.Left != (Object)null || (Object)(object)item.Right != (Object)null || (Object)(object)item.Center != (Object)null)
							{
								num7 += 3f;
							}
							if (num7 > num)
							{
								num = num7;
								val = item;
							}
						}
					}
					if ((Object)(object)val == (Object)null)
					{
						val = val2;
					}
					if ((Object)(object)val == (Object)null)
					{
						Debug.LogWarning(Object.op_Implicit("[KillAndVent] Could not find any valid vent."));
						return false;
					}
					Vector2 val4 = Vector2.op_Implicit(((Component)val).transform.position);
					((Component)_003ClocalPlayer_003E5__2).transform.position = new Vector3(val4.x, val4.y, ((Component)_003ClocalPlayer_003E5__2).transform.position.z);
					if ((Object)(object)_003ClocalPlayer_003E5__2.NetTransform != (Object)null)
					{
						_003ClocalPlayer_003E5__2.NetTransform.SnapTo(val4);
					}
					try
					{
						Camera main = Camera.main;
						if ((Object)(object)main != (Object)null)
						{
							((Component)main).transform.position = new Vector3(val4.x, val4.y, ((Component)main).transform.position.z);
						}
					}
					catch
					{
					}
					if ((Object)(object)_003ClocalPlayer_003E5__2.MyPhysics != (Object)null)
					{
						_003ClocalPlayer_003E5__2.MyPhysics.RpcEnterVent(val.Id);
					}
					LogCheat($"[KillAndVent] Escaped to vent #{val.Id} (score: {num:F1})");
					return false;
				}
				IL_00a7:
				if (_003ClocalPlayer_003E5__2.isKilling || !_003ClocalPlayer_003E5__2.moveable)
				{
					if (!(Time.realtimeSinceStartup > _003Ctimeout_003E5__3))
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 1;
						return true;
					}
					Debug.LogWarning(Object.op_Implicit("[KillAndVent] Timeout waiting for kill animation. Forcing escape."));
				}
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCompleteAllTasksOptimized_003Ed__191 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public int totalPending;

		private bool _003CisHost_003E5__2;

		private int _003CcompletedCount_003E5__3;

		private int _003CbatchCount_003E5__4;

		private float _003CdynamicDelay_003E5__5;

		private IEnumerator<PlayerTask> _003C_003E7__wrap5;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCompleteAllTasksOptimized_003Ed__191(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || (uint)(num - 1) <= 2u)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap5 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0298: Expected O, but got Unknown
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0245: Expected O, but got Unknown
			bool result;
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					result = false;
					goto end_IL_0000;
				case 0:
					_003C_003E1__state = -1;
					if (!((Object)(object)PlayerControl.LocalPlayer == (Object)null) && !((Object)(object)PlayerControl.LocalPlayer.Data == (Object)null) && !((Object)(object)AmongUsClient.Instance == (Object)null))
					{
						_003CisHost_003E5__2 = ((InnerNetClient)AmongUsClient.Instance).AmHost;
						_003CcompletedCount_003E5__3 = 0;
						_003CbatchCount_003E5__4 = 0;
						Il2CppArrayBase<PlayerTask> val = PlayerControl.LocalPlayer.myTasks.ToArray();
						_003CdynamicDelay_003E5__5 = Mathf.Lerp(MAX_TASK_DELAY, MIN_TASK_DELAY, Mathf.Clamp01((float)totalPending / 15f));
						_003C_003E7__wrap5 = val.GetEnumerator();
						_003C_003E1__state = -3;
						break;
					}
					result = false;
					goto end_IL_0000;
				case 1:
					_003C_003E1__state = -3;
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					result = true;
					goto end_IL_0000;
				case 2:
					_003C_003E1__state = -3;
					break;
				case 3:
					_003C_003E1__state = -3;
					break;
				}
				while (true)
				{
					if (_003C_003E7__wrap5.MoveNext())
					{
						PlayerTask current = _003C_003E7__wrap5.Current;
						if ((Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null)
						{
							result = false;
							_003C_003Em__Finally1();
							break;
						}
						if ((Object)(object)current == (Object)null || current.IsComplete)
						{
							continue;
						}
						try
						{
							MessageWriter val2 = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)PlayerControl.LocalPlayer).NetId, (byte)1, (SendOption)1, -1);
							val2.WritePacked(current.Id);
							((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val2);
						}
						catch (Exception value)
						{
							Debug.LogError(Object.op_Implicit($"Error sending RPC for task {current.Id}: {value}"));
							continue;
						}
						NormalPlayerTask val3 = (NormalPlayerTask)(object)((current is NormalPlayerTask) ? current : null);
						if (val3 != null)
						{
							val3.taskStep = val3.MaxStep;
						}
						current.Complete();
						try
						{
							if ((Object)(object)GameData.Instance != (Object)null)
							{
								GameData.Instance.CompleteTask(PlayerControl.LocalPlayer, current.Id);
							}
						}
						catch
						{
						}
						_003CcompletedCount_003E5__3++;
						_003CbatchCount_003E5__4++;
						float num = (float)(random.NextDouble() * 0.02 - 0.01);
						if (_003CbatchCount_003E5__4 >= TASK_BATCH_SIZE)
						{
							_003CbatchCount_003E5__4 = 0;
							_003C_003E2__current = (object)new WaitForSeconds(BATCH_PAUSE_DELAY);
							_003C_003E1__state = 1;
							result = true;
						}
						else
						{
							_003C_003E2__current = (object)new WaitForSeconds(Mathf.Max(0.01f, _003CdynamicDelay_003E5__5 + num));
							_003C_003E1__state = 3;
							result = true;
						}
						break;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap5 = null;
					try
					{
						PlayerControl.LocalPlayer.Data.MarkDirty();
					}
					catch
					{
					}
					LogCheat($"All {_003CcompletedCount_003E5__3} tasks completed. (Host: {_003CisHost_003E5__2})");
					result = false;
					break;
				}
				end_IL_0000:;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
			return result;
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap5 != null)
			{
				_003C_003E7__wrap5.Dispose();
			}
		}

		[DebuggerHidden]
		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCompleteAllTasksWithDelay_003Ed__192 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float perTaskDelay;

		private bool _003CisHost_003E5__2;

		private IEnumerator<PlayerTask> _003C_003E7__wrap2;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCompleteAllTasksWithDelay_003Ed__192(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || (uint)(num - 1) <= 1u)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Expected O, but got Unknown
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					if ((Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)PlayerControl.LocalPlayer.Data == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null)
					{
						return false;
					}
					_003CisHost_003E5__2 = ((InnerNetClient)AmongUsClient.Instance).AmHost;
					Il2CppArrayBase<PlayerTask> val = PlayerControl.LocalPlayer.myTasks.ToArray();
					_003C_003E7__wrap2 = val.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				}
				case 1:
					_003C_003E1__state = -3;
					break;
				case 2:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap2.MoveNext())
				{
					_003C_003Ec__DisplayClass192_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass192_0
					{
						task = _003C_003E7__wrap2.Current
					};
					if ((Object)(object)CS_0024_003C_003E8__locals0.task == (Object)null || CS_0024_003C_003E8__locals0.task.IsComplete)
					{
						continue;
					}
					MessageWriter val2 = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)PlayerControl.LocalPlayer).NetId, (byte)1, (SendOption)1, -1);
					val2.WritePacked(CS_0024_003C_003E8__locals0.task.Id);
					((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val2);
					PlayerTask task = CS_0024_003C_003E8__locals0.task;
					NormalPlayerTask val3 = (NormalPlayerTask)(object)((task is NormalPlayerTask) ? task : null);
					if (val3 != null)
					{
						val3.taskStep = val3.MaxStep;
					}
					CS_0024_003C_003E8__locals0.task.Complete();
					try
					{
						PlayerTask val4 = PlayerControl.LocalPlayer.Data.Tasks.ToArray().FirstOrDefault((Func<PlayerTask, bool>)((PlayerTask t) => t.TaskId == CS_0024_003C_003E8__locals0.task.Id));
						if (val4 != null)
						{
							val4.Complete = true;
						}
					}
					catch
					{
					}
					LogCheat($"Task {CS_0024_003C_003E8__locals0.task.Id} completed. (Host: {_003CisHost_003E5__2})");
					if (perTaskDelay > 0f)
					{
						_003C_003E2__current = (object)new WaitForSeconds(perTaskDelay);
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = null;
				PlayerControl.LocalPlayer.Data.MarkDirty();
				LogCheat(_003CisHost_003E5__2 ? "All tasks completed and synced." : "All tasks completed locally. Sync may vary.");
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CDelayedKillBypass_003Ed__220 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float delay;

		public PlayerControl killer;

		public PlayerControl target;

		public bool broadcast;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CDelayedKillBypass_003Ed__220(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = (object)new WaitForSeconds(delay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if ((Object)(object)killer == (Object)null || (Object)(object)target == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null)
				{
					Debug.LogWarning(Object.op_Implicit("killer/target/AmongUsClient not found."));
					return false;
				}
				if (broadcast)
				{
					BroadcastKillBypass(killer, target, 0f);
				}
				else
				{
					ExecuteKillBypass(killer, target, 0f);
				}
				LogCheat($"[Bypass] Kill of {target.PlayerId} broadcast after {delay}s delay.");
				return false;
			}
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGhostCleanupCoroutine_003Ed__104 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float delaySeconds;

		public uint[] netIds;

		public int selfClientId;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGhostCleanupCoroutine_003Ed__104(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = (object)new WaitForSeconds(delaySeconds);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				if ((Object)(object)AmongUsClient.Instance == (Object)null || netIds == null)
				{
					return false;
				}
				for (int i = 0; i < netIds.Length; i++)
				{
					uint num = netIds[i];
					try
					{
						MessageWriter val = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(uint.MaxValue, (byte)0, (SendOption)1, -1);
						val.EndMessage();
						val.StartMessage((byte)5);
						val.WritePacked(num);
						((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val);
						if (selfClientId >= 0)
						{
							MessageWriter val2 = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(uint.MaxValue, (byte)0, (SendOption)1, selfClientId);
							val2.EndMessage();
							val2.StartMessage((byte)5);
							val2.WritePacked(num);
							((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogError(Object.op_Implicit($"[F4 Test] cleanup despawn {num}: {ex.Message}"));
					}
				}
				LogCheat("F4: ghost cleanup done (" + netIds.Length + " NetIds)");
				return false;
			}
			}
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CKillAllCoroutine_003Ed__251 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public bool crewOnly;

		public bool impostorsOnly;

		private float _003CbaseDelay_003E5__2;

		private List<PlayerControl>.Enumerator _003C_003E7__wrap2;

		private PlayerControl _003Ctarget_003E5__4;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CKillAllCoroutine_003Ed__251(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap2 = default(List<PlayerControl>.Enumerator);
			_003Ctarget_003E5__4 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Expected O, but got Unknown
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					if ((Object)(object)ShipStatus.Instance == (Object)null || (Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).IsGameStarted || !((InnerNetClient)AmongUsClient.Instance).AmHost)
					{
						return false;
					}
					List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
					List<PlayerControl> list = new List<PlayerControl>(allPlayerControls.Count);
					for (int i = 0; i < allPlayerControls.Count; i++)
					{
						PlayerControl val = allPlayerControls[i];
						if ((Object)(object)val != (Object)null && (Object)(object)val != (Object)(object)PlayerControl.LocalPlayer && IsValidKillTarget(val, crewOnly, impostorsOnly))
						{
							list.Add(val);
						}
					}
					_003CbaseDelay_003E5__2 = 0f;
					_003C_003E7__wrap2 = list.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				}
				case 1:
					_003C_003E1__state = -3;
					BroadcastKillBypass(PlayerControl.LocalPlayer, _003Ctarget_003E5__4, 0f);
					_003CbaseDelay_003E5__2 += (float)random.Next(100, 300) / 1000f;
					_003Ctarget_003E5__4 = null;
					break;
				}
				if (_003C_003E7__wrap2.MoveNext())
				{
					_003Ctarget_003E5__4 = _003C_003E7__wrap2.Current;
					_003C_003E2__current = (object)new WaitForSeconds(_003CbaseDelay_003E5__2);
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = default(List<PlayerControl>.Enumerator);
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap2).Dispose();
		}

		[DebuggerHidden]
		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CLocalDisableScansAfterDelay_003Ed__214 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float delay;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CLocalDisableScansAfterDelay_003Ed__214(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = (object)new WaitForSeconds(delay);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator.MoveNext())
				{
					LocalToggleScanner(enumerator.Current, on: false);
				}
				return false;
			}
			}
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSendFakePositionCoroutine_003Ed__233 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSendFakePositionCoroutine_003Ed__233(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected O, but got Unknown
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
			}
			else
			{
				_003C_003E1__state = -1;
			}
			Vector2 val = default(Vector2);
			(val)._002Ector(1000f, 1000f);
			PlayerControl.LocalPlayer.NetTransform.SnapTo(val);
			_003C_003E2__current = (object)new WaitForSeconds(0.2f);
			_003C_003E1__state = 1;
			return true;
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CWalkScanRoutine_003Ed__210 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private PlayerControl _003Clp_003E5__2;

		private ShipStatus _003Cship_003E5__3;

		private byte _003Cpid_003E5__4;

		private Vector2 _003CpadPos_003E5__5;

		private Collider2D _003Ccol_003E5__6;

		private Vector2 _003CcolliderOffset_003E5__7;

		private Vector2 _003CwalkDest_003E5__8;

		private float _003Ctimeout_003E5__9;

		private float _003Ctimer_003E5__10;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CWalkScanRoutine_003Ed__210(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Clp_003E5__2 = null;
			_003Cship_003E5__3 = null;
			_003Ccol_003E5__6 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_031d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02af: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				_003Clp_003E5__2 = null;
				_003Cship_003E5__3 = null;
				try
				{
					_003Clp_003E5__2 = PlayerControl.LocalPlayer;
				}
				catch
				{
				}
				if ((Object)(object)_003Clp_003E5__2 == (Object)null)
				{
					Debug.LogError(Object.op_Implicit("[WalkScan] LocalPlayer null"));
					MedbayScanEnabled = false;
					return false;
				}
				try
				{
					_003Cship_003E5__3 = ShipStatus.Instance;
				}
				catch
				{
				}
				if ((Object)(object)_003Cship_003E5__3 == (Object)null)
				{
					Debug.LogError(Object.op_Implicit("[WalkScan] ShipStatus null"));
					MedbayScanEnabled = false;
					return false;
				}
				MedScannerBehaviour val = null;
				try
				{
					val = _003Cship_003E5__3.MedScanner;
				}
				catch
				{
				}
				if ((Object)(object)val == (Object)null)
				{
					Debug.LogError(Object.op_Implicit("[WalkScan] MedScanner null"));
					MedbayScanEnabled = false;
					return false;
				}
				_003Cpid_003E5__4 = _003Clp_003E5__2.PlayerId;
				Vector3 position = val.Position;
				_003CpadPos_003E5__5 = new Vector2(position.x, position.y);
				_003Clp_003E5__2.moveable = false;
				_003Ccol_003E5__6 = _003Clp_003E5__2.Collider;
				_003CcolliderOffset_003E5__7 = (((Object)(object)_003Ccol_003E5__6 != (Object)null) ? _003Ccol_003E5__6.offset : Vector2.zero);
				if ((Object)(object)_003Ccol_003E5__6 != (Object)null)
				{
					((Behaviour)_003Ccol_003E5__6).enabled = false;
				}
				try
				{
					((Component)Camera.main).GetComponent<FollowerCamera>().Locked = false;
				}
				catch
				{
				}
				_003Clp_003E5__2.NetTransform.RpcSnapTo(_003CpadPos_003E5__5 + new Vector2(0f, 2f));
				try
				{
					_003Cship_003E5__3.RpcUpdateSystem((SystemTypes)10, (byte)(_003Cpid_003E5__4 | 0x80u));
				}
				catch
				{
				}
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				if (!MedbayScanEnabled)
				{
					return false;
				}
				_003CwalkDest_003E5__8 = _003CpadPos_003E5__5 - _003CcolliderOffset_003E5__7;
				if (_walkPhysicsCoroutine != null)
				{
					try
					{
						((MonoBehaviour)_003Clp_003E5__2).StopCoroutine(_walkPhysicsCoroutine);
					}
					catch
					{
					}
				}
				_walkPhysicsCoroutine = ((MonoBehaviour)_003Clp_003E5__2).StartCoroutine(_003Clp_003E5__2.MyPhysics.WalkPlayerTo(_003CpadPos_003E5__5, 0.001f, 2f, false));
				_003Ctimeout_003E5__9 = 0f;
				goto IL_02d7;
			case 3:
				_003C_003E1__state = -1;
				goto IL_02d7;
			case 4:
				_003C_003E1__state = -1;
				try
				{
					((Component)Camera.main).GetComponent<FollowerCamera>().Locked = true;
				}
				catch
				{
				}
				BroadcastScannerDirect(_003Clp_003E5__2, on: true);
				_003Ctimer_003E5__10 = 0f;
				break;
			case 5:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_02d7:
				if (MedbayScanEnabled && _003Ctimeout_003E5__9 < 3f)
				{
					_003Ctimeout_003E5__9 += Time.deltaTime;
					if (!(Vector2.Distance(new Vector2(((Component)_003Clp_003E5__2).transform.position.x, ((Component)_003Clp_003E5__2).transform.position.y), _003CwalkDest_003E5__8) < 0.05f))
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 3;
						return true;
					}
				}
				if (_walkPhysicsCoroutine != null)
				{
					try
					{
						((MonoBehaviour)_003Clp_003E5__2).StopCoroutine(_walkPhysicsCoroutine);
					}
					catch
					{
					}
					_walkPhysicsCoroutine = null;
				}
				_003Clp_003E5__2.MyPhysics.body.velocity = Vector2.zero;
				if ((Object)(object)_003Ccol_003E5__6 != (Object)null)
				{
					((Behaviour)_003Ccol_003E5__6).enabled = true;
				}
				if (!MedbayScanEnabled)
				{
					return false;
				}
				_003C_003E2__current = null;
				_003C_003E1__state = 4;
				return true;
			}
			if (_003Ctimer_003E5__10 < 10f && MedbayScanEnabled)
			{
				_003Ctimer_003E5__10 += Time.deltaTime;
				_003C_003E2__current = null;
				_003C_003E1__state = 5;
				return true;
			}
			try
			{
				BroadcastScannerDirect(_003Clp_003E5__2, on: false);
			}
			catch
			{
			}
			try
			{
				_003Cship_003E5__3.RpcUpdateSystem((SystemTypes)10, (byte)(_003Cpid_003E5__4 | 0x40u));
			}
			catch
			{
			}
			try
			{
				((Component)Camera.main).GetComponent<FollowerCamera>().Locked = false;
			}
			catch
			{
			}
			_003Clp_003E5__2.moveable = true;
			MedbayScanEnabled = false;
			_walkScanCoroutine = null;
			Debug.Log(Object.op_Implicit("[WalkScan] Complete"));
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			return false;
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003C_StaggeredDespawnCoroutine_003Ed__671 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public List<uint> netIds;

		private int _003CselfId_003E5__2;

		private int _003Ci_003E5__3;

		object System.Collections.Generic.IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003C_StaggeredDespawnCoroutine_003Ed__671(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Expected O, but got Unknown
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				AmongUsClient instance = AmongUsClient.Instance;
				if ((Object)(object)instance == (Object)null)
				{
					return false;
				}
				_003CselfId_003E5__2 = ((InnerNetClient)instance).ClientId;
				_003Ci_003E5__3 = 0;
				break;
			}
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__3 += 4;
				break;
			}
			if (_003Ci_003E5__3 < netIds.Count)
			{
				AmongUsClient instance2 = AmongUsClient.Instance;
				if ((Object)(object)instance2 == (Object)null || !((InnerNetClient)instance2).AmConnected)
				{
					return false;
				}
				int num = Math.Min(_003Ci_003E5__3 + 4, netIds.Count);
				for (int i = _003Ci_003E5__3; i < num; i++)
				{
					uint netId = netIds[i];
					try
					{
						_SendDespawnDirect(instance2, netId, -1);
					}
					catch
					{
					}
					try
					{
						_SendDespawnDirect(instance2, netId, _003CselfId_003E5__2);
					}
					catch
					{
					}
				}
				_003C_003E2__current = (object)new WaitForSeconds(0.18f);
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool System.Collections.IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void System.Collections.IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private static readonly System.Random random = new System.Random();

	public static readonly byte[] HOST_COLOR_IDS = new byte[19]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11, 12, 13, 14, 15, 16, 17, 18
	};

	internal static bool ForceColorForEveryone = false;

	private static bool _hasUsedCamsCheatBefore = false;

	private static bool zoomOutEnabled = false;

	private static float customZoomValue = 10f;

	private static float _defaultOrthoSize = 3f;

	private static bool _endGameActive = false;

	internal static bool TeleportToCursorEnabled = false;

	internal static bool ScannerBypassEnabled = false;

	private static float _scannerAutoDisableTime = 0f;

	internal static bool MedbayScanEnabled = false;

	internal static bool FakeLagEnabled = false;

	private static Vector2 _fakeLagLastPos;

	private static bool _fakeLagPosInit = false;

	private static bool _fakeLagBypass = false;

	private static uint _ghostNetIdCounter;

	internal static uint _trackedVanillaNetIdCnt = 0u;

	internal static readonly HashSet<uint> _localDestroyedNetIds = new HashSet<uint>();

	private static bool _ghostCeilingWarned = false;

	internal static byte _specTarget = byte.MaxValue;

	private static bool _specDisabledShadow = false;

	private static Vector3 _specOriginalLightLocal = Vector3.zero;

	private static bool _specCachedLightLocal = false;

	internal static bool _dummyActive;

	private static readonly List<DummyEntry> _dummyNetIdsList = new List<DummyEntry>();

	private static readonly HashSet<int> _dummySynced = new HashSet<int>();

	private static float _dummyReconcileNext = 0f;

	private const int FAKE_CD_MAX = 127;

	private const float FAKE_CD_TICK_INTERVAL = 1f;

	private const float FAKE_CD_TOGGLE_COOLDOWN = 0.3f;

	private static bool _fakeCdActive = false;

	private static int _fakeCdCurrent = 127;

	private static float _fakeCdLastTick = 0f;

	private static float _fakeCdLastFire = 0f;

	private static uint _capturedShipSpawnId = uint.MaxValue;

	private static int _capturedShipChildCount = -1;

	internal static int _selMapSpawnId = -1;

	private static uint[] _selMapNetIds = null;

	private static float _lastSelMapTime = 0f;

	private const float SEL_MAP_COOLDOWN_SEC = 4f;

	internal static bool _mapSwapInProgress;

	private static CosmeticsCache _savedCosmeticsCache;

	internal static bool _mapActive;

	private static uint[] _mapNetIds;

	internal static bool _allMapsActive;

	private static readonly List<uint[]> _allMapNetIds = new List<uint[]>();

	private static readonly Dictionary<byte, List<_CFState>> _cfStates = new Dictionary<byte, List<_CFState>>();

	internal static bool _cfActive;

	private static float _cfNextSend;

	private static readonly HashSet<int> _cfSynced = new HashSet<int>();

	private static float _cfReconcileNext = 0f;

	private static readonly Queue<byte> _cfSpawnQueue = new Queue<byte>();

	private static readonly Dictionary<int, List<byte>> _cfReconcilePending = new Dictionary<int, List<byte>>();

	private static bool _cfActivatedByOrbit;

	private static int _cfCachedCntIdx = -1;

	private static int _cfCachedNumChildren = 0;

	private const float ORBIT_ANGULAR_STEP = 0.4f;

	private const int ORBIT_BASE_PER_TICK_BUDGET = 8;

	private const float ORBIT_BROADCAST_MIN_INTERVAL = 0.5f;

	private static int _orbitRoundRobinCursor = 0;

	private static Coroutine _walkScanCoroutine;

	private static Coroutine _walkPhysicsCoroutine;

	internal static bool IsRevealSusActive = false;

	private static readonly StringBuilder _revealSusSb = new StringBuilder(256);

	private static readonly StringBuilder _revealSusMeetingSb = new StringBuilder(128);

	private static Coroutine invisibilityCoroutine;

	private static float _lastSliderSyncTime = 0f;

	private static bool _sliderSyncPending = false;

	private const float SLIDER_SYNC_DEBOUNCE = 0.15f;

	private static Dictionary<string, float> _gsOriginalFloats;

	private static Dictionary<string, int> _gsOriginalInts;

	private static Dictionary<string, bool> _gsOriginalBools;

	private static float _lastInstantStartTime = 0f;

	private static float _lastAutoKickTime = 0f;

	private static float _autoKickRecentCleanTime = 0f;

	private static readonly HashSet<int> _autoKickRecentIds = new HashSet<int>();

	private static string[] _cachedNameBlacklist;

	private static string _cachedNameBlacklistRaw;

	private const float KICK_NOTIFY_DELAY = 2.5f;

	private static string _pendingKickNotify;

	private static float _pendingKickNotifyTime;

	private static int _pendingKickCount;

	private static readonly Dictionary<byte, float> _pendingLevelCheck = new Dictionary<byte, float>();

	private const float LEVEL_SYNC_DELAY = 2.5f;

	internal static bool DisableGameVent = false;

	private static float _lastVentCheckTime = 0f;

	private const float VENT_CHECK_INTERVAL = 0.25f;

	private static byte? _protectedPlayerId = null;

	private static float _protectionStartTime = 0f;

	private static float _protectionDuration = 10f;

	private static readonly List<(byte PlayerId, string Name, bool IsImpostor)> _alivePlayersBuffer = new List<(byte, string, bool)>(15);

	private static readonly List<(byte PlayerId, string Name, bool IsImpostor, bool IsDead)> _allPlayersBuffer = new List<(byte, string, bool, bool)>(16);

	private static float _lastGodModeProtectionTime = 0f;

	private const int GOD_MODE_MAX_STACKS = 5;

	private static float _lastGodModeAllTime = 0f;

	private const float RAINBOW_INTERVAL = 2f;

	private const float RAINBOW_STEP_NORMAL = 0.35f;

	private const float RAINBOW_STEP_LAB_ACTIVE = 1f;

	private static bool _rainbowEnabled;

	private static int _rainbowPhase;

	private static float _rainbowLastApply;

	private static int _rainbowPlayerIdx;

	private static readonly Queue<KeyValuePair<byte, int>> _colorBurstQ = new Queue<KeyValuePair<byte, int>>();

	private static float _colorBurstLastSend;

	private static readonly Dictionary<byte, int> _playerRainbowPhases = new Dictionary<byte, int>();

	private static float _playerRainbowLastApply;

	private static bool _isImpersonating = false;

	private static byte _savedColorId = 0;

	private static string _savedHatId = "";

	private static string _savedSkinId = "";

	private static string _savedPetId = "";

	private static string _savedVisorId = "";

	private static string _savedNamePlateId = "";

	private static float _impTimer = 0f;

	private static float _impDuration = 0f;

	internal static bool FreeCamEnabled = false;

	private static bool freecamActive = false;

	private static float freeCamSpeed = 10f;

	private const float SCROLL_ZOOM_STEP = 0.5f;

	private const float SCROLL_ZOOM_MIN = 3f;

	private const float SCROLL_ZOOM_MAX = 15f;

	private static int _hudUpdateFrameCounter = 0;

	internal static bool TracersEnabled = false;

	private static Dictionary<byte, LineRenderer> _tracerLines = new Dictionary<byte, LineRenderer>();

	private static readonly HashSet<byte> _tracerActiveIdsBuffer = new HashSet<byte>(20);

	private static readonly List<byte> _tracerRemoveBuffer = new List<byte>(20);

	private static Material _tracerMaterial;

	private static float _lastTracerUpdateTime = 0f;

	private const float TRACER_UPDATE_INTERVAL = 0.033f;

	private const float TRACER_Z = -2f;

	private const float TRACER_MIN_DISTANCE = 0.3f;

	private const float TRACER_DIST_SCALE_MAX = 30f;

	private const float TRACER_CREW_WIDTH = 0.045f;

	private const float TRACER_CREW_ALPHA = 0.75f;

	private const float TRACER_IMP_WIDTH = 0.07f;

	private const float TRACER_IMP_ALPHA = 0.92f;

	internal static bool NoClipSmoothEnabled = false;

	private static float _noClipSpeed = 5f;

	internal static bool RadarEnabled = false;

	internal static bool RadarShowGhosts = true;

	internal static bool RadarShowDeadBodies = true;

	internal static bool RadarDrawIcons = true;

	internal static bool RadarRightClickTP = true;

	internal static bool RadarShowBorder = true;

	internal static bool RadarLocked = false;

	internal static bool RadarRevealRoles = true;

	internal static bool RadarMinimized = false;

	internal static bool RadarShowVents = true;

	internal static bool RadarShowTracers = false;

	internal static bool RadarClickPlayerTP = true;

	internal static bool RadarShowMapImage = true;

	internal static float RadarMapZoom = 1f;

	private static Rect _radarRect = new Rect(10f, 80f, 280f, 280f);

	private static bool _isDragging = false;

	private static bool _isResizing = false;

	private static bool _radarBlockedMovement = false;

	private static Vector2 _dragOffset;

	private static Vector2 _resizeStart;

	private static Rect _resizeStartRect;

	private static float _lastRadarUpdate = 0f;

	private const float RADAR_UPDATE_INTERVAL = 0.1f;

	private static int _radarFrame = 0;

	private static float _pulseTime = 0f;

	private const float RADAR_MIN_SIZE = 200f;

	private const float RADAR_MAX_SIZE = 500f;

	private const float HEADER_HEIGHT = 24f;

	private const float FOOTER_HEIGHT = 18f;

	private const float RADAR_MAP_ZOOM_MIN = 0.5f;

	private const float RADAR_MAP_ZOOM_MAX = 3f;

	private static Dictionary<int, Texture2D> _mapTextures = new Dictionary<int, Texture2D>();

	private static bool _mapTextureLoaded = false;

	private static float _sonarAngle = 0f;

	private static Texture2D _texSonarLine;

	private static float _nearestImpostorDist = float.MaxValue;

	private static bool _sonarInitialized = false;

	private static string _cachedMapName = "MAPA";

	private static int _cachedAliveCount = 0;

	private static Vector2 _localPos;

	private static int _currentMapType;

	private static bool _isAirship = false;

	private static readonly Dictionary<int, MapInfo> MapInfos = new Dictionary<int, MapInfo>
	{
		{
			0,
			new MapInfo(0f, 0f, 4.4f, -25.5f, 21.3f, -19.4f, 8.7f, -23.56f, 19.56f, -17.34f, 7.34f)
		},
		{
			1,
			new MapInfo(0f, 0f, 4.4f, -13.1f, 30.9f, -5.3f, 27.2f)
		},
		{
			2,
			new MapInfo(0f, 0f, 4.4f, -1.4f, 42.7f, -28f, 3.7f)
		},
		{
			3,
			new MapInfo(0f, 0f, 4.4f, -25f, 27.5f, -13.5f, 16f)
		}
	};

	private static readonly MapInfo AirshipMapInfo = new MapInfo(0f, 0f, 4.4f, -26.7f, 41.4f, -19.5f, 19.1f);

	private static Texture2D _airshipMapTexture;

	private static RadarPlayerData[] _playerCache = new RadarPlayerData[16];

	private static int _playerCacheCount = 0;

	private static RadarDeadBodyData[] _deadBodyCache = new RadarDeadBodyData[16];

	private static int _deadBodyCacheCount = 0;

	private const int SHAPE_RECT = 0;

	private const int SHAPE_OCTAGON = 1;

	private const int SHAPE_PENTAGON_L = 2;

	private const int SHAPE_PENTAGON_R = 3;

	private const int SHAPE_HEXAGON = 4;

	private const int SHAPE_CORRIDOR = 5;

	private static readonly RoomData[] SkeldRooms = new RoomData[21]
	{
		new RoomData(-0.66f, 1.25f, 11.69f, 11.95f, 4),
		new RoomData(9.62f, 2.44f, 5.63f, 7.13f, 2),
		new RoomData(5.88f, -3.4f, 4.23f, 4.01f, 3),
		new RoomData(17.51f, -4.53f, 4.6f, 5.7f, 2),
		new RoomData(9.47f, -12.72f, 5.29f, 4.87f, 0),
		new RoomData(4.06f, -15.92f, 5.58f, 3.89f, 0),
		new RoomData(4.63f, -8.08f, 5.59f, 5.13f, 0),
		new RoomData(-2.07f, -13.4f, 6.4f, 8.83f, 0),
		new RoomData(-7.51f, -10.13f, 6.54f, 7.06f, 0),
		new RoomData(-8.12f, -3.47f, 6.55f, 5.45f, 0),
		new RoomData(-13.34f, -4.39f, 3.25f, 6.28f, 0),
		new RoomData(-17.31f, 1f, 5.43f, 5.73f, 0),
		new RoomData(-17.17f, -11.85f, 5.11f, 5.23f, 0),
		new RoomData(-21.37f, -5f, 4.97f, 8.68f, 0),
		new RoomData(-0.1f, -6.84f, 3.84f, 4.24f, octagon: false, corridor: true),
		new RoomData(6.05f, 1.01f, 1.82f, 1.62f, octagon: false, corridor: true),
		new RoomData(11.51f, -6.11f, 7.23f, 8.44f, octagon: false, corridor: true),
		new RoomData(-9.98f, -12.78f, 9.37f, 5.44f, octagon: false, corridor: true),
		new RoomData(3.92f, -12.78f, 5.67f, 2.43f, octagon: false, corridor: true),
		new RoomData(-10.56f, 0.55f, 8.39f, 2.6f, octagon: false, corridor: true),
		new RoomData(-16.92f, -5.55f, 4.28f, 7.4f, octagon: false, corridor: true)
	};

	private static readonly RoomData[] MiraRooms = new RoomData[18]
	{
		new RoomData(-5.73f, 4.12f, 11.8f, 8.41f, 0),
		new RoomData(2.45f, 13.21f, 4.89f, 7.2f, 0),
		new RoomData(9.78f, 12.49f, 4.97f, 5.75f, 0),
		new RoomData(14.55f, 18.78f, 3.66f, 4.53f, 0),
		new RoomData(20.99f, 18.53f, 3.72f, 5.13f, 0),
		new RoomData(17.8f, 23.4f, 10.61f, 4.66f, 0),
		new RoomData(15.41f, -0.28f, 3.52f, 3.97f, 0),
		new RoomData(15.41f, 3.98f, 3.5f, 3.12f, 0),
		new RoomData(7.46f, 2.87f, 7.45f, 5.59f, 0),
		new RoomData(6.09f, 5.75f, 2.4f, 6.98f, 0),
		new RoomData(23.64f, 2.49f, 11.49f, 6.29f, 0),
		new RoomData(19.47f, 3.47f, 3.3f, 4.66f, 0),
		new RoomData(23.61f, -2.56f, 11.53f, 2.46f, 0),
		new RoomData(11.76f, 1.78f, 4f, 2f, octagon: false, corridor: true),
		new RoomData(17.88f, 13.69f, 10.63f, 14.85f, octagon: false, corridor: true),
		new RoomData(12.39f, 1.68f, 2.33f, 7.86f, octagon: false, corridor: true),
		new RoomData(2.43f, -1.48f, 17.51f, 2.67f, octagon: false, corridor: true),
		new RoomData(17.85f, 6.75f, 13.76f, 2.21f, octagon: false, corridor: true)
	};

	private static readonly RoomData[] PolusRooms = new RoomData[14]
	{
		new RoomData(16.62f, -2.13f, 5.81f, 8.65f, 0),
		new RoomData(22.21f, -17.27f, 12.74f, 2.7f, 0),
		new RoomData(22.52f, -23.54f, 6.03f, 5.99f, 0),
		new RoomData(11.71f, -17.48f, 3.11f, 4.05f, 0),
		new RoomData(11.92f, -23.41f, 4.63f, 4.87f, 0),
		new RoomData(6.37f, -11.19f, 9.86f, 5.36f, 0),
		new RoomData(19.93f, -12.08f, 5.51f, 2.96f, 0),
		new RoomData(3.57f, -19.24f, 6.91f, 7.74f, 0),
		new RoomData(32.94f, -8.12f, 16.4f, 6.87f, 0),
		new RoomData(36.66f, -20.96f, 5.96f, 4.98f, 0),
		new RoomData(2.33f, -24.04f, 4.01f, 1.89f, 0),
		new RoomData(2.99f, -12.06f, 2.87f, 1.93f, 0),
		new RoomData(28.21f, -22.71f, 10.94f, 6.67f, 0),
		new RoomData(39.56f, -14.87f, 3.25f, 11.31f, 0)
	};

	private static readonly RoomData[] FungleRooms = new RoomData[15]
	{
		new RoomData(-16.43f, 6.04f, 6.79f, 4.47f, 0),
		new RoomData(22.1f, 13.68f, 7.09f, 3.85f, 0),
		new RoomData(-21.6f, -7.53f, 5.46f, 1.65f, 0),
		new RoomData(-7.81f, 9.57f, 8.42f, 8.28f, 0),
		new RoomData(9.24f, -11.43f, 4.44f, 4f, 0),
		new RoomData(-15.62f, -8.08f, 6.72f, 4.87f, 0),
		new RoomData(-4.27f, -9.78f, 4.67f, 3.42f, 0),
		new RoomData(8.25f, 2.14f, 5.16f, 5.89f, 0),
		new RoomData(-3.04f, -1.84f, 5.78f, 4.85f, 0),
		new RoomData(1.86f, -1.47f, 4.13f, 3.01f, 0),
		new RoomData(12.86f, 8.65f, 5.68f, 4.64f, 0),
		new RoomData(22.25f, -7.72f, 5.62f, 4.93f, 0),
		new RoomData(-17.14f, -0.88f, 6.03f, 4.12f, 0),
		new RoomData(1.12f, 5.27f, 5.64f, 4.94f, 0),
		new RoomData(23.09f, 2.83f, 7.44f, 8.33f, 0)
	};

	private static readonly RoomData[] AirshipRooms = new RoomData[21]
	{
		new RoomData(-20.76f, -0.62f, 8.85f, 7.02f, 0),
		new RoomData(-12.36f, -6.53f, 6.87f, 7.45f, 0),
		new RoomData(-13.26f, 1.88f, 3.94f, 4.1f, 0),
		new RoomData(-2.22f, 1.65f, 12.53f, 10.07f, 0),
		new RoomData(-0.48f, 10.05f, 6f, 6.13f, 0),
		new RoomData(-9.11f, 8.87f, 10.44f, 10.44f, 0),
		new RoomData(-5.15f, -9.09f, 6.76f, 8.7f, 0),
		new RoomData(-13.08f, -13.88f, 8.17f, 6.39f, 0),
		new RoomData(9.52f, 9.07f, 13.22f, 7.21f, 0),
		new RoomData(10.88f, 0.12f, 12.79f, 8.75f, 0),
		new RoomData(20.84f, 1.25f, 8.15f, 8.2f, 0),
		new RoomData(20.16f, 9.73f, 7.08f, 7.56f, 0),
		new RoomData(29.87f, 7.42f, 11.32f, 6.8f, 0),
		new RoomData(27.44f, -0.63f, 4.29f, 7.07f, 0),
		new RoomData(8.05f, -13.37f, 6.68f, 9.2f, 0),
		new RoomData(34.99f, -0.55f, 9.86f, 8.23f, 0),
		new RoomData(15.04f, -7.46f, 12.2f, 9.02f, 0),
		new RoomData(27.51f, -7.92f, 11.91f, 5.8f, 0),
		new RoomData(10.54f, 15.55f, 15.26f, 4.1f, 0),
		new RoomData(1.49f, -12.07f, 5.49f, 2.53f, 0),
		new RoomData(-12.41f, -1.53f, 7.29f, 2.05f, octagon: false, corridor: true)
	};

	private static Texture2D _texRoomFill;

	private static Texture2D _texRoomBorder;

	private static Texture2D _texCorridorFill;

	private static GUIStyle _sRoomFill;

	private static GUIStyle _sRoomBorder;

	private static GUIStyle _sCorridorFill;

	private static bool _radarInitialized = false;

	private static Texture2D _texWindowBg;

	private static Texture2D _texHeaderBg;

	private static Texture2D _texMapBg;

	private static Texture2D _texBorderAccent;

	private static Texture2D _texBorderDim;

	private static Texture2D _texImpostorGlow;

	private static Texture2D _texImpostorDot;

	private static Texture2D _texGhostDot;

	private static Texture2D _texDeadBodyBg;

	private static Texture2D _texVisor;

	private static Texture2D _texShadow;

	private static Texture2D _texMapOverlay;

	private static Texture2D _texVent;

	private static Texture2D _texTracerLine;

	private static Texture2D _texGridDim;

	private static Texture2D _texGridBright;

	private static Texture2D _texVentStyle;

	private static Texture2D _texScanLine;

	private static Texture2D[] _texPlayers;

	private static GUIStyle _sWindowBg;

	private static GUIStyle _sHeaderBg;

	private static GUIStyle _sMapBg;

	private static GUIStyle _sBorderAccent;

	private static GUIStyle _sBorderDim;

	private static GUIStyle _sImpostorGlow;

	private static GUIStyle _sImpostorDot;

	private static GUIStyle _sGhostDot;

	private static GUIStyle _sDeadBodyBg;

	private static GUIStyle _sVisor;

	private static GUIStyle _sShadow;

	private static GUIStyle _sMapOverlay;

	private static GUIStyle _sGridDim;

	private static GUIStyle _sGridBright;

	private static GUIStyle _sVentStyle;

	private static GUIStyle _sScanLine;

	private static GUIStyle[] _sPlayers;

	private static GUIStyle _sLabel;

	private static GUIStyle _sTitle;

	private static GUIStyle _sSmall;

	private static GUIStyle _sButton;

	private static readonly Color ThemeAccentSoft = new Color(1f, 0.25f, 0.4f, 1f);

	private static readonly Color ThemeAccentDim = new Color(0.5f, 0.02f, 0.1f, 1f);

	private static readonly Color ThemeVisor = new Color(0f, 0.9f, 1f, 1f);

	private static readonly Color ThemeImpostorGlow = new Color(1f, 0f, 0.15f, 0.35f);

	private static readonly Color ThemeMapBg = new Color(0.03f, 0.03f, 0.05f, 0.98f);

	private static readonly Color ThemeMapOverlay = new Color(1f, 0.09f, 0.27f, 0.06f);

	private static readonly Color[] PlayerColors = (Color[])(object)new Color[18]
	{
		new Color(0.77f, 0.07f, 0.07f),
		new Color(0.07f, 0.18f, 0.82f),
		new Color(0.07f, 0.5f, 0.18f),
		new Color(0.93f, 0.33f, 0.73f),
		new Color(0.94f, 0.49f, 0.05f),
		new Color(0.96f, 0.93f, 0.15f),
		new Color(0.24f, 0.24f, 0.24f),
		new Color(0.84f, 0.88f, 0.94f),
		new Color(0.42f, 0.18f, 0.73f),
		new Color(0.44f, 0.29f, 0.12f),
		new Color(0.22f, 0.99f, 0.86f),
		new Color(0.31f, 0.94f, 0.22f),
		new Color(0.43f, 0.15f, 0.15f),
		new Color(0.93f, 0.8f, 0.8f),
		new Color(1f, 0.72f, 0.4f),
		new Color(0.51f, 0.51f, 0.51f),
		new Color(0.56f, 0.44f, 0.34f),
		new Color(0.93f, 0.46f, 0.47f)
	};

	private static readonly Dictionary<int, Texture2D> _texCache = new Dictionary<int, Texture2D>();

	private static GUIStyle _cachedMapStyle;

	private static PlayerData[] _webRadarPool = new PlayerData[16];

	private static float _lastCacheUpdateTime = 0f;

	private const float CACHE_UPDATE_INTERVAL = 0.1f;

	internal static bool NoKillDistanceLimitEnabled = false;

	internal static bool KillAndVentEnabled = false;

	internal static bool SeeGhostsEnabled = false;

	internal static bool SeeDeadChatEnabled = false;

	internal static bool ShieldVisEnabled = false;

	internal static bool AlwaysShowChatEnabled = false;

	private static bool _killCooldownUIInitialized = false;

	private static GUIStyle _cooldownAboveHeadStyle;

	private static GUIStyle _cooldownShadowStyle;

	private static Texture2D _cooldownBgTex;

	private static readonly StringBuilder _killCooldownSb = new StringBuilder(64);

	private static readonly List<SpriteRenderer> _mapPlayerDots = new List<SpriteRenderer>(16);

	private static float _killAlertEndTime = 0f;

	private static float _killAlertStartTime = 0f;

	private const float KILL_ALERT_SLIDE_IN = 0.3f;

	private const float KILL_ALERT_FADE_OUT = 1.5f;

	private static string _killAlertTitle = "";

	private static string _killAlertDetail = "";

	private static GUIStyle _killAlertTitleStyle;

	private static GUIStyle _killAlertDetailStyle;

	private static GUIStyle _killAlertBgStyle;

	private static GUIStyle _killAlertAccentStyle;

	private static GUIStyle _killAlertShadowStyle;

	private static Texture2D _killAlertBgTex;

	private static Texture2D _killAlertAccentTex;

	private static Texture2D _killAlertShadowTex;

	private static Vector2 _eventLogScrollPos = Vector2.zero;

	private static Rect _eventLogRect = new Rect(10f, 250f, 380f, 300f);

	private static bool _eventLogDragging = false;

	private static Vector2 _eventLogDragOffset;

	private static GUIStyle _eventLogHeaderStyle;

	private static GUIStyle _eventLogEventStyle;

	private static GUIStyle _eventLogTimeStyle;

	private static Texture2D _eventLogBgTex;

	private static Texture2D _eventLogHeaderTex;

	private static HashSet<byte> _revealedVoters = new HashSet<byte>();

	internal static int ForceVotesSelectedTargetIndex = 0;

	internal static List<ForceVotesPlayerInfo> ForceVotesAlivePlayers = new List<ForceVotesPlayerInfo>();

	public const byte VOTE_HAS_NOT_VOTED = byte.MaxValue;

	public const byte VOTE_MISSED = 254;

	public const byte VOTE_SKIPPED = 253;

	private static bool _radarDebugEnabled = false;

	private static float _radarDebugTimer = 0f;

	private static readonly float RADAR_DEBUG_INTERVAL = 0.5f;

	private static Dictionary<SystemTypes, (Vector3 pos, Vector2 size)> _roomBoundsCache = new Dictionary<SystemTypes, (Vector3, Vector2)>();

	private static float _roomCacheTime = 0f;

	private static readonly string _D0 = "";

	private static readonly string _D1 = "";

	private static int _lastKnownGameId = 0;

	private static bool _wasConnected = false;

	private static float _v0 = 0f;

	private static float _v1 = 0f;

	private static float _v2 = 0f;

	private static float _v3 = 0f;

	private static int _v4 = 0;

	private static byte _v5 = 0;

	private static float _vF = 0f;

	private static float _vG = 0f;

	private static float _vH = 0f;

	private static float _vI = 0f;

	private static float _vJ = 0f;

	private static float _vK = 0f;

	private static float _vM = 0f;

	private static bool _vN = false;

	private static float _vO = 0f;

	private static readonly Dictionary<byte, byte> _mH = new Dictionary<byte, byte>();

	private static readonly Dictionary<byte, byte> _mK = new Dictionary<byte, byte>();

	private static readonly Dictionary<byte, byte> _mV = new Dictionary<byte, byte>();

	private static readonly Dictionary<byte, byte> _mN = new Dictionary<byte, byte>();

	private static readonly byte[] _r0 = new byte[4];

	private static readonly Dictionary<byte, byte>[] _s0 = new Dictionary<byte, byte>[4];

	private static readonly string[][] _gsC = new string[4][]
	{
		new string[2],
		new string[2],
		new string[2],
		new string[2]
	};

	private static int _gsV = -1;

	private static bool _chActive = false;

	private static float _chStart = 0f;

	private static int _chOwnerId = -1;

	private static byte _chHostPid = byte.MaxValue;

	private static float _v7 = 0f;

	private static int _v8 = 0;

	private static float _v6 = 0f;

	private static bool _clActive = false;

	private static float _clStart = 0f;

	private static float _vA = 0f;

	private static int _vB = 0;

	private static float _vC = 0.5f;

	private static float _v9 = 0f;

	private static int _clPC = 0;

	private static readonly Dictionary<byte, float> _aT = new Dictionary<byte, float>();

	private static readonly List<byte> _aE = new List<byte>();

	private static readonly Dictionary<byte, int> _aS = new Dictionary<byte, int>();

	private static readonly Dictionary<byte, float> _aP = new Dictionary<byte, float>();

	private static float _aU = 0f;

	private static float _vD = 0f;

	private static readonly HashSet<byte> _bT = new HashSet<byte>();

	private static readonly Dictionary<byte, int> _bS = new Dictionary<byte, int>();

	private static float _bU = 0f;

	private static float _vE = 0f;

	public static byte RPC_SET_SCANNER => ServerData.Config.RpcSetScanner;

	public static byte RPC_SET_INVISIBILITY => ServerData.Config.RpcSetInvisibility;

	public static byte CUSTOM_RPC_PHANTOM_POOF => ServerData.Config.RpcPhantomPoof;

	public static byte CHECK_COLOR_RPC => ServerData.Config.RpcCheckColor;

	internal static bool AnimShields { get; set; } = false;


	internal static bool AnimAsteroids { get; set; } = false;


	internal static bool AnimEmptyGarbage { get; set; } = false;


	internal static bool AnimCamsInUse { get; set; } = false;


	internal static bool IsZoomOutActive => zoomOutEnabled;

	private static float SCANNER_AUTO_DISABLE_DURATION
	{
		get
		{
			if (!(ServerData.Config.ScannerAutoDisable > 0f))
			{
				return 12f;
			}
			return ServerData.Config.ScannerAutoDisable;
		}
	}

	private static uint _GhostNetIdBase
	{
		get
		{
			if (!(ServerData.Config.W2 > 0f))
			{
				return 0u;
			}
			return (uint)ServerData.Config.W2;
		}
	}

	private static uint _GhostNetIdCeiling
	{
		get
		{
			if (!(ServerData.Config.W3 > 0f))
			{
				return 0u;
			}
			return (uint)ServerData.Config.W3;
		}
	}

	private static float _DummyReconcileHz
	{
		get
		{
			if (ServerData.Config.W6 <= 0)
			{
				return 0f;
			}
			return (float)ServerData.Config.W6 / 1000f;
		}
	}

	private static (uint spawnId, int children, string label)[] _SEL_MAP_DEFS => new(uint, int, string)[4]
	{
		(ServerData.Config.M0, 1, "Skeld"),
		(ServerData.Config.M1, 1, "MIRA"),
		(ServerData.Config.M2, 1, "Polus"),
		(ServerData.Config.M3, 1, "Airship")
	};

	private static (uint spawnId, int children, string label)[] _MAP_DEFS => new(uint, int, string)[5]
	{
		(ServerData.Config.M0, 1, "Skeld"),
		(ServerData.Config.M1, 1, "MIRA"),
		(ServerData.Config.M2, 1, "Polus"),
		(ServerData.Config.M3, 1, "Airship"),
		(ServerData.Config.M4, 1, "Fungle")
	};

	private static float _CfBaseHz
	{
		get
		{
			if (!(ServerData.Config.W8 > 0f))
			{
				return 0f;
			}
			return 1000f / ServerData.Config.W8;
		}
	}

	private static float _CfReconcileHz
	{
		get
		{
			if (!(ServerData.Config.W9 > 0f))
			{
				return 0f;
			}
			return ServerData.Config.W9 / 1000f;
		}
	}

	private static int TASK_BATCH_SIZE
	{
		get
		{
			if (ServerData.Config.TaskBatchSize <= 0)
			{
				return 3;
			}
			return ServerData.Config.TaskBatchSize;
		}
	}

	private static float MIN_TASK_DELAY
	{
		get
		{
			if (!(ServerData.Config.TaskDelayMin > 0f))
			{
				return 0.02f;
			}
			return ServerData.Config.TaskDelayMin;
		}
	}

	private static float MAX_TASK_DELAY
	{
		get
		{
			if (!(ServerData.Config.TaskDelayMax > 0f))
			{
				return 0.08f;
			}
			return ServerData.Config.TaskDelayMax;
		}
	}

	private static float BATCH_PAUSE_DELAY
	{
		get
		{
			if (!(ServerData.Config.BatchPauseDelay > 0f))
			{
				return 0.05f;
			}
			return ServerData.Config.BatchPauseDelay;
		}
	}

	public static bool HasProtectedPlayer => _protectedPlayerId.HasValue;

	public static bool IsGodModeActive
	{
		get
		{
			if (CheatConfig.GodMode)
			{
				AmongUsClient instance = AmongUsClient.Instance;
				if (instance == null)
				{
					return false;
				}
				return ((InnerNetClient)instance).AmHost;
			}
			return false;
		}
	}

	internal static bool RainbowEnabled => _rainbowEnabled;

	internal static bool IsImpersonating => _isImpersonating;

	internal static bool IsKillCooldownsActive
	{
		get
		{
			ConfigEntry<bool> showKillCooldowns = CheatConfig.ShowKillCooldowns;
			if (showKillCooldowns != null && showKillCooldowns.Value)
			{
				AmongUsClient instance = AmongUsClient.Instance;
				if (instance != null && ((InnerNetClient)instance).IsGameStarted)
				{
					return !IsRevealSusActive;
				}
			}
			return false;
		}
	}

	private static float KILL_ALERT_DURATION
	{
		get
		{
			if (!(ServerData.Config.KillAlertDuration > 0f))
			{
				return 5f;
			}
			return ServerData.Config.KillAlertDuration;
		}
	}

	private static float ROOM_CACHE_DURATION => 5f;

	private static float _AR_CEIL => ServerData.Config.W11;

	private static float _AR_MAXDIV => ServerData.Config.W12;

	private static float _AR_MINDIV => ServerData.Config.W13;

	private static float _AR_PAUSE => ServerData.Config.W14;

	private static float _AR_RECOVERY => ServerData.Config.W38;

	private static float _AR_RECOVERY_HOLD => ServerData.Config.W39;

	private static float _AR_GRACE => ServerData.Config.W16;

	private static float _AR_RAMP_START => ServerData.Config.W40;

	private static float _AR_CLDUR => ServerData.Config.W17;

	private static float _AR_CHDUR => ServerData.Config.W18;

	private static float _AR_PRIMER => ServerData.Config.W19;

	private static float _CH_DUR => _AR_CHDUR;

	private static float _CL_DUR => _AR_CLDUR;

	private static float _C0
	{
		get
		{
			if (!(ServerData.Config.W7 > 0f))
			{
				return 0f;
			}
			return ServerData.Config.W7;
		}
	}

	private static float _C2
	{
		get
		{
			if (!(ServerData.Config.W0 > 0f))
			{
				return 0f;
			}
			return ServerData.Config.W0;
		}
	}

	private static byte _C3
	{
		get
		{
			if (ServerData.Config.X0 <= 0)
			{
				return 0;
			}
			return ServerData.Config.X0;
		}
	}

	private static int _L2
	{
		get
		{
			if (ServerData.Config.W5 <= 0)
			{
				return 0;
			}
			return ServerData.Config.W5;
		}
	}

	private static string _L3
	{
		get
		{
			if (string.IsNullOrEmpty(ServerData.Config.Z0))
			{
				return "";
			}
			return ServerData.Config.Z0;
		}
	}

	private static byte _L4
	{
		get
		{
			if (ServerData.Config.X1 <= 0)
			{
				return 0;
			}
			return ServerData.Config.X1;
		}
	}

	private static byte _L5
	{
		get
		{
			if (ServerData.Config.X2 <= 0)
			{
				return 0;
			}
			return ServerData.Config.X2;
		}
	}

	private static byte _L6
	{
		get
		{
			if (ServerData.Config.X3 <= 0)
			{
				return 0;
			}
			return ServerData.Config.X3;
		}
	}

	private static byte _L7
	{
		get
		{
			if (ServerData.Config.X4 <= 0)
			{
				return 0;
			}
			return ServerData.Config.X4;
		}
	}

	private static bool IsOnActiveMap()
	{
		try
		{
			return (Object)(object)ShipStatus.Instance != (Object)null && !_endGameActive;
		}
		catch
		{
			return false;
		}
	}

	private static bool IsFakeLagContextValid()
	{
		try
		{
			return (Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).IsGameStarted && (Object)(object)ShipStatus.Instance != (Object)null;
		}
		catch
		{
			return false;
		}
	}

	internal static void SetFakeLag(bool enabled)
	{
		if ((!enabled || IsFakeLagContextValid()) && FakeLagEnabled != enabled)
		{
			FakeLagEnabled = enabled;
			_fakeLagPosInit = false;
			LogCheat("Fake Lag " + (enabled ? "ON" : "OFF"));
		}
	}

	internal static void DisableFakeLagOnGameEnd()
	{
		if (FakeLagEnabled)
		{
			FakeLagEnabled = false;
			_fakeLagPosInit = false;
			LogCheat("Fake Lag OFF (game end)");
		}
	}

	internal static void TrackVanillaNetId(uint netId)
	{
		if (netId != 0 && netId < 100000)
		{
			uint num = netId + 1;
			if (num > _trackedVanillaNetIdCnt)
			{
				_trackedVanillaNetIdCnt = num;
			}
		}
	}

	internal static void TrackDestroyedNetId(uint netId)
	{
		if (netId == 0)
		{
			return;
		}
		try
		{
			_localDestroyedNetIds.Add(netId);
		}
		catch
		{
		}
	}

	private static uint NextGhostNetIdBase(int slotsNeeded)
	{
		int num = Math.Max(slotsNeeded, 1);
		uint num2 = _trackedVanillaNetIdCnt + 10;
		if (num2 < _GhostNetIdBase)
		{
			num2 = _GhostNetIdBase;
		}
		if (_ghostNetIdCounter < num2)
		{
			_ghostNetIdCounter = num2;
		}
		int num3 = 5000;
		while (num3-- > 0)
		{
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				if (_localDestroyedNetIds.Contains(_ghostNetIdCounter + (uint)i))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
			_ghostNetIdCounter++;
		}
		uint ghostNetIdCounter = _ghostNetIdCounter;
		_ghostNetIdCounter += (uint)num;
		if (_ghostNetIdCounter >= _GhostNetIdCeiling && !_ghostCeilingWarned)
		{
			_ghostCeilingWarned = true;
			Debug.LogWarning(Object.op_Implicit($"[MapCheats] Ghost NetId counter near MinServerID ceiling: {_ghostNetIdCounter}"));
		}
		return ghostNetIdCounter;
	}

	private static void _SpecActivate(PlayerControl target)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		_specTarget = target.PlayerId;
		try
		{
			HudManager instance = DestroyableSingleton<HudManager>.Instance;
			FollowerCamera val = ((instance != null) ? instance.PlayerCam : null);
			if ((Object)(object)val != (Object)null)
			{
				val.SetTarget(((Il2CppObjectBase)target).TryCast<MonoBehaviour>());
			}
		}
		catch
		{
		}
		_specDisabledShadow = false;
		try
		{
			HudManager instance2 = DestroyableSingleton<HudManager>.Instance;
			MeshRenderer val2 = ((instance2 != null) ? instance2.ShadowQuad : null);
			if ((Object)(object)val2 != (Object)null && ((Component)val2).gameObject.activeSelf)
			{
				((Component)val2).gameObject.SetActive(false);
				_specDisabledShadow = true;
			}
		}
		catch
		{
		}
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if ((Object)(object)localPlayer != (Object)null)
			{
				LightSource componentInChildren = ((Component)localPlayer).GetComponentInChildren<LightSource>();
				if ((Object)(object)componentInChildren != (Object)null && !_specCachedLightLocal)
				{
					_specOriginalLightLocal = ((Component)componentInChildren).transform.localPosition;
					_specCachedLightLocal = true;
				}
			}
		}
		catch
		{
		}
	}

	private static void _SpecDeactivate()
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		_specTarget = byte.MaxValue;
		try
		{
			HudManager instance = DestroyableSingleton<HudManager>.Instance;
			FollowerCamera val = ((instance != null) ? instance.PlayerCam : null);
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if ((Object)(object)val != (Object)null && (Object)(object)localPlayer != (Object)null)
			{
				val.SetTarget(((Il2CppObjectBase)localPlayer).TryCast<MonoBehaviour>());
			}
		}
		catch
		{
		}
		if (_specDisabledShadow)
		{
			_specDisabledShadow = false;
			try
			{
				ConfigEntry<bool> noShadows = CheatConfig.NoShadows;
				if (noShadows == null || !noShadows.Value)
				{
					HudManager instance2 = DestroyableSingleton<HudManager>.Instance;
					MeshRenderer val2 = ((instance2 != null) ? instance2.ShadowQuad : null);
					if ((Object)(object)val2 != (Object)null)
					{
						((Component)val2).gameObject.SetActive(true);
					}
				}
			}
			catch
			{
			}
		}
		if (!_specCachedLightLocal)
		{
			return;
		}
		_specCachedLightLocal = false;
		try
		{
			PlayerControl localPlayer2 = PlayerControl.LocalPlayer;
			if ((Object)(object)localPlayer2 != (Object)null)
			{
				LightSource componentInChildren = ((Component)localPlayer2).GetComponentInChildren<LightSource>();
				if ((Object)(object)componentInChildren != (Object)null)
				{
					((Component)componentInChildren).transform.localPosition = _specOriginalLightLocal;
				}
			}
		}
		catch
		{
		}
	}

	internal static void ToggleSpec(byte playerId)
	{
		if (_specTarget == playerId)
		{
			_SpecDeactivate();
		}
		else
		{
			GameData instance = GameData.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo playerById = instance.GetPlayerById(playerId);
				obj = ((playerById != null) ? playerById.Object : null);
			}
			PlayerControl val = (PlayerControl)obj;
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			_SpecActivate(val);
		}
		PlayerPickMenu.TriggerRealtimeUpdate(force: true);
	}

	internal static void TickSpec()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (_specTarget == byte.MaxValue)
		{
			return;
		}
		GameData instance = GameData.Instance;
		NetworkedPlayerInfo val = ((instance != null) ? instance.GetPlayerById(_specTarget) : null);
		if ((Object)(object)val == (Object)null || val.Disconnected)
		{
			_SpecDeactivate();
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			return;
		}
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			PlayerControl @object = val.Object;
			if (!((Object)(object)localPlayer == (Object)null) && !((Object)(object)@object == (Object)null))
			{
				LightSource componentInChildren = ((Component)localPlayer).GetComponentInChildren<LightSource>();
				if ((Object)(object)componentInChildren != (Object)null)
				{
					((Component)componentInChildren).transform.position = ((Component)@object).transform.position;
				}
			}
		}
		catch
		{
		}
	}

	private static void _WriteDummySpawnSub(MessageWriter w, uint spawnId, InnerNetObject[] children, int numChildren, uint[] netIds, byte sourcePlayerId, Vector2 pos)
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if (children == null || netIds == null || netIds.Length != numChildren)
		{
			Debug.LogWarning(Object.op_Implicit($"[Cheat] dummy write skipped: children={children?.Length} netIds={netIds?.Length} num={numChildren}"));
			return;
		}
		w.StartMessage((byte)4);
		w.WritePacked(spawnId);
		w.WritePacked(-2);
		w.Write((byte)0);
		w.WritePacked(numChildren);
		for (int i = 0; i < numChildren; i++)
		{
			InnerNetObject val = children[i];
			w.WritePacked(netIds[i]);
			w.StartMessage((byte)1);
			if ((Object)(object)((Il2CppObjectBase)val).TryCast<PlayerControl>() != (Object)null)
			{
				w.Write(true);
				w.Write(sourcePlayerId);
			}
			else if ((Object)(object)((Il2CppObjectBase)val).TryCast<CustomNetworkTransform>() != (Object)null)
			{
				w.Write((ushort)0);
				NetHelpers.WriteVector2(pos, w);
			}
			w.EndMessage();
		}
		w.EndMessage();
	}

	private static void _WriteDespawnSub(MessageWriter w, uint netId)
	{
		w.StartMessage((byte)5);
		w.WritePacked(netId);
		w.EndMessage();
	}

	private static DummyEntry? _SpawnOneDummy()
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null || (Object)(object)localPlayer.MyPhysics == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return null;
		}
		Il2CppArrayBase<InnerNetObject> componentsInChildren = ((Component)localPlayer).GetComponentsInChildren<InnerNetObject>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return null;
		}
		int length = componentsInChildren.Length;
		uint num = NextGhostNetIdBase(length);
		uint[] array = new uint[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = num + (uint)i;
		}
		Vector2 val = (((Object)(object)((Component)localPlayer).transform != (Object)null) ? Vector2.op_Implicit(((Component)localPlayer).transform.position) : Vector2.zero);
		DummyEntry dummyEntry = default(DummyEntry);
		dummyEntry.NetIds = array;
		dummyEntry.Pos = val;
		dummyEntry.SourcePlayerId = localPlayer.PlayerId;
		DummyEntry value = dummyEntry;
		uint spawnId = ((InnerNetObject)localPlayer).SpawnId;
		byte playerId = localPlayer.PlayerId;
		int clientId = ((InnerNetClient)AmongUsClient.Instance).ClientId;
		int estimatedBytes = ServerData.Config.W36 + length * ServerData.Config.W37;
		int[] array2 = new int[2] { -1, clientId };
		foreach (int targetClientId in array2)
		{
			Il2CppArrayBase<InnerNetObject> capturedChildren = componentsInChildren;
			int capturedNum = length;
			uint capturedSpawnId = spawnId;
			uint[] capturedNetIds = array;
			byte capturedSource = playerId;
			Vector2 capturedPos = val;
			LabRpcArbiter.Enqueue(new LabRpcArbiter.LabOp
			{
				Consumer = LabRpcArbiter.Consumer.GhostTwins,
				Priority = LabRpcArbiter.Priority.High,
				TargetClientId = targetClientId,
				SendOption = (SendOption)1,
				EstimatedBytes = estimatedBytes,
				WriteSubMessage = delegate(MessageWriter w)
				{
					//IL_0025: Unknown result type (might be due to invalid IL or missing references)
					_WriteDummySpawnSub(w, capturedSpawnId, Il2CppArrayBase<InnerNetObject>.op_Implicit(capturedChildren), capturedNum, capturedNetIds, capturedSource, capturedPos);
				}
			});
		}
		LogCheat("dummy enqueued NetIds " + num + "-" + (num + length - 1));
		return value;
	}

	internal static void ToggleDummy()
	{
		if (!_dummyActive && (Object)(object)ShipStatus.Instance != (Object)null)
		{
			LogCheat("Ghost Twins: activation blocked — not in lobby");
			return;
		}
		_dummyActive = !_dummyActive;
		_dummySynced.Clear();
		if (_dummyActive)
		{
			LabRpcArbiter.Register(LabRpcArbiter.Consumer.GhostTwins);
			_dummyNetIdsList.Clear();
			DummyEntry? dummyEntry = _SpawnOneDummy();
			DummyEntry? dummyEntry2 = _SpawnOneDummy();
			if (dummyEntry.HasValue)
			{
				_dummyNetIdsList.Add(dummyEntry.Value);
			}
			if (dummyEntry2.HasValue)
			{
				_dummyNetIdsList.Add(dummyEntry2.Value);
			}
			_dummyReconcileNext = Time.time + _DummyReconcileHz;
		}
		else
		{
			_EnqueueDummyDespawnAll();
			_dummyNetIdsList.Clear();
			LabRpcArbiter.Unregister(LabRpcArbiter.Consumer.GhostTwins);
			LogCheat("dummies despawn enqueued");
		}
	}

	private static void _EnqueueDummyDespawnAll()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)AmongUsClient.Instance == (Object)null || _dummyNetIdsList.Count == 0)
		{
			return;
		}
		int clientId = ((InnerNetClient)AmongUsClient.Instance).ClientId;
		LabRpcArbiter.Register(LabRpcArbiter.Consumer.Cleanup);
		foreach (DummyEntry dummyNetIds in _dummyNetIdsList)
		{
			uint[] netIds = dummyNetIds.NetIds;
			foreach (uint num in netIds)
			{
				uint capturedNetId = num;
				int[] array = new int[2] { -1, clientId };
				foreach (int targetClientId in array)
				{
					LabRpcArbiter.Enqueue(new LabRpcArbiter.LabOp
					{
						Consumer = LabRpcArbiter.Consumer.Cleanup,
						Priority = LabRpcArbiter.Priority.Critical,
						TargetClientId = targetClientId,
						SendOption = (SendOption)1,
						EstimatedBytes = 8,
						WriteSubMessage = delegate(MessageWriter w)
						{
							_WriteDespawnSub(w, capturedNetId);
						}
					});
				}
			}
		}
	}

	internal static void ForceClearDummies()
	{
		_EnqueueDummyDespawnAll();
		_dummyNetIdsList.Clear();
		_dummySynced.Clear();
		_dummyActive = false;
		LabRpcArbiter.Unregister(LabRpcArbiter.Consumer.GhostTwins);
		LogCheat("dummies force-cleared");
		NotifyUtils.Info("Ghost Twins cleared");
	}

	private static void _ImmediateDespawnAllGhosts()
	{
		AmongUsClient instance = AmongUsClient.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		int clientId = ((InnerNetClient)instance).ClientId;
		List<uint> list = new List<uint>(32);
		foreach (DummyEntry dummyNetIds in _dummyNetIdsList)
		{
			if (dummyNetIds.NetIds != null)
			{
				uint[] netIds = dummyNetIds.NetIds;
				foreach (uint item in netIds)
				{
					list.Add(item);
				}
			}
		}
		foreach (KeyValuePair<byte, List<_CFState>> cfState in _cfStates)
		{
			List<_CFState> value = cfState.Value;
			if (value == null)
			{
				continue;
			}
			foreach (_CFState item3 in value)
			{
				if (item3 != null && item3.netIds != null)
				{
					uint[] netIds = item3.netIds;
					foreach (uint item2 in netIds)
					{
						list.Add(item2);
					}
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		int[] array = new int[2] { -1, clientId };
		foreach (int num in array)
		{
			try
			{
				MessageWriter val = ((InnerNetClient)instance).StartRpcImmediately(uint.MaxValue, (byte)0, (SendOption)1, num);
				val.EndMessage();
				for (int j = 0; j < list.Count; j++)
				{
					val.StartMessage((byte)5);
					val.WritePacked(list[j]);
					val.EndMessage();
				}
				val.StartMessage((byte)2);
				val.WritePacked(uint.MaxValue);
				val.Write((byte)0);
				((InnerNetClient)instance).FinishRpcImmediately(val);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Object.op_Implicit($"[LAB] immediate despawn tgt={num} threw: {ex.Message}"));
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			TrackDestroyedNetId(list[k]);
		}
		LogCheat($"Immediate despawn sent for {list.Count} ghost NetIds (GT+SC)");
	}

	private static void _EnqueueRebroadcastDummyEntryTo(int targetClientId, DummyEntry entry)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null || (Object)(object)localPlayer.MyPhysics == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return;
		}
		Il2CppArrayBase<InnerNetObject> componentsInChildren = ((Component)localPlayer).GetComponentsInChildren<InnerNetObject>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return;
		}
		int length = componentsInChildren.Length;
		if (entry.NetIds != null && entry.NetIds.Length == length)
		{
			Il2CppArrayBase<InnerNetObject> capturedChildren = componentsInChildren;
			int capturedNum = length;
			uint capturedSpawnId = ((InnerNetObject)localPlayer).SpawnId;
			uint[] capturedNetIds = entry.NetIds;
			byte capturedSource = entry.SourcePlayerId;
			Vector2 capturedPos = entry.Pos;
			int estimatedBytes = ServerData.Config.W36 + length * ServerData.Config.W37;
			LabRpcArbiter.Enqueue(new LabRpcArbiter.LabOp
			{
				Consumer = LabRpcArbiter.Consumer.GhostTwins,
				Priority = LabRpcArbiter.Priority.High,
				TargetClientId = targetClientId,
				SendOption = (SendOption)1,
				EstimatedBytes = estimatedBytes,
				WriteSubMessage = delegate(MessageWriter w)
				{
					//IL_0025: Unknown result type (might be due to invalid IL or missing references)
					_WriteDummySpawnSub(w, capturedSpawnId, Il2CppArrayBase<InnerNetObject>.op_Implicit(capturedChildren), capturedNum, capturedNetIds, capturedSource, capturedPos);
				}
			});
		}
	}

	internal static void TickDummyReconcile()
	{
		if (!_dummyActive || _dummyNetIdsList.Count == 0 || (Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return;
		}
		float time = Time.time;
		if (time < _dummyReconcileNext)
		{
			return;
		}
		_dummyReconcileNext = time + _DummyReconcileHz;
		int clientId = ((InnerNetClient)AmongUsClient.Instance).ClientId;
		List<ClientData> allClients = ((InnerNetClient)AmongUsClient.Instance).allClients;
		if (allClients == null)
		{
			return;
		}
		for (int i = 0; i < allClients.Count; i++)
		{
			ClientData val = allClients[i];
			if (val == null)
			{
				continue;
			}
			int id = val.Id;
			if (id == clientId || _dummySynced.Contains(id))
			{
				continue;
			}
			foreach (DummyEntry dummyNetIds in _dummyNetIdsList)
			{
				_EnqueueRebroadcastDummyEntryTo(id, dummyNetIds);
			}
			_dummySynced.Add(id);
			LogCheat($"dummies sync enqueued for late joiner cid={id}");
			break;
		}
	}

	internal static void ForceClearClones()
	{
		try
		{
			_CFDespawnAll();
		}
		catch
		{
		}
		_cfActive = false;
		_cfStates.Clear();
		_cfSynced.Clear();
		_cfSpawnQueue.Clear();
		_cfReconcilePending.Clear();
		LabRpcArbiter.Unregister(LabRpcArbiter.Consumer.ShadowClones);
		LogCheat("clones force-cleared");
		NotifyUtils.Info("Shadow Clones cleared");
	}

	private static void SendFakeCdSilent(int value)
	{
		try
		{
			int num = value;
			if (num > 127)
			{
				num = 127;
			}
			if (num < -128)
			{
				num = -128;
			}
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if ((Object)(object)localPlayer != (Object)null)
			{
				localPlayer.RpcSetStartCounter(num);
			}
			if (DestroyableSingleton<GameStartManager>.InstanceExists)
			{
				DestroyableSingleton<GameStartManager>.Instance.SetStartCounter((sbyte)num);
			}
		}
		catch
		{
		}
	}

	private static void SendFakeCdWithSound(int value)
	{
		SendFakeCdSilent(-1);
		SendFakeCdSilent(value);
	}

	internal static bool IsFakeCdActive()
	{
		return _fakeCdActive;
	}

	internal static int GetFakeCdCurrent()
	{
		return _fakeCdCurrent;
	}

	internal static void ToggleFakeCountdown()
	{
		if (Time.unscaledTime - _fakeCdLastFire < 0.3f)
		{
			return;
		}
		_fakeCdLastFire = Time.unscaledTime;
		if (_fakeCdActive)
		{
			_fakeCdActive = false;
			SendFakeCdSilent(-1);
			try
			{
				NotifyUtils.Warning("Fake countdown STOPPED");
				return;
			}
			catch
			{
				return;
			}
		}
		if (!DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			try
			{
				NotifyUtils.Error("Not in lobby");
				return;
			}
			catch
			{
				return;
			}
		}
		_fakeCdActive = true;
		_fakeCdCurrent = 127;
		_fakeCdLastTick = Time.unscaledTime;
		SendFakeCdWithSound(_fakeCdCurrent);
		try
		{
			NotifyUtils.Info("Fake countdown STARTED");
		}
		catch
		{
		}
	}

	internal static void TickFakeCountdown()
	{
		if (!_fakeCdActive)
		{
			return;
		}
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime - _fakeCdLastTick < 1f)
		{
			return;
		}
		_fakeCdLastTick = unscaledTime;
		if (!DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			_fakeCdActive = false;
			return;
		}
		_fakeCdCurrent--;
		if (_fakeCdCurrent < 0)
		{
			_fakeCdCurrent = 127;
		}
		SendFakeCdWithSound(_fakeCdCurrent);
	}

	[IteratorStateMachine(typeof(_003CGhostCleanupCoroutine_003Ed__104))]
	private static IEnumerator GhostCleanupCoroutine(uint[] netIds, float delaySeconds, int selfClientId = -1)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGhostCleanupCoroutine_003Ed__104(0)
		{
			netIds = netIds,
			delaySeconds = delaySeconds,
			selfClientId = selfClientId
		};
	}

	internal static void CaptureShipSpawnId()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		ShipStatus instance = ShipStatus.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			uint spawnId = ((InnerNetObject)instance).SpawnId;
			if (_capturedShipSpawnId != spawnId)
			{
				int length = ((Component)instance).GetComponentsInChildren<InnerNetObject>().Length;
				_capturedShipSpawnId = spawnId;
				_capturedShipChildCount = length;
				LogCheat($"F4: captured SpawnId={spawnId} children={length} Type={instance.Type}");
			}
		}
	}

	internal static void DiagnoseMapSpawnIds()
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			LogCheat("[DiagMaps] no client");
			return;
		}
		try
		{
			Il2CppReferenceArray<InnerNetObject> nonAddressableSpawnableObjects = ((InnerNetClient)AmongUsClient.Instance).NonAddressableSpawnableObjects;
			LogCheat($"[DiagMaps] NonAddressable count={((Il2CppArrayBase<InnerNetObject>)(object)nonAddressableSpawnableObjects)?.Length ?? 0}");
			if (nonAddressableSpawnableObjects != null)
			{
				for (int i = 0; i < ((Il2CppArrayBase<InnerNetObject>)(object)nonAddressableSpawnableObjects).Length; i++)
				{
					if ((Object)(object)((Il2CppArrayBase<InnerNetObject>)(object)nonAddressableSpawnableObjects)[i] != (Object)null)
					{
						int length = ((Component)((Il2CppArrayBase<InnerNetObject>)(object)nonAddressableSpawnableObjects)[i]).GetComponentsInChildren<InnerNetObject>().Length;
						LogCheat($"  NA[{i}] SpawnId={((Il2CppArrayBase<InnerNetObject>)(object)nonAddressableSpawnableObjects)[i].SpawnId} children={length} type={((object)((Il2CppArrayBase<InnerNetObject>)(object)nonAddressableSpawnableObjects)[i]).GetType().Name} name={((Object)((Component)((Il2CppArrayBase<InnerNetObject>)(object)nonAddressableSpawnableObjects)[i]).gameObject).name}");
					}
				}
			}
			Il2CppReferenceArray<AssetReference> spawnableObjects = ((InnerNetClient)AmongUsClient.Instance).SpawnableObjects;
			LogCheat($"[DiagMaps] SpawnableObjects (Addressable) count={((Il2CppArrayBase<AssetReference>)(object)spawnableObjects)?.Length ?? 0}");
			List<AssetReference> shipPrefabs = AmongUsClient.Instance.ShipPrefabs;
			LogCheat($"[DiagMaps] ShipPrefabs count={shipPrefabs?.Count ?? 0}");
			string[] array = new string[6] { "Skeld", "MIRA", "Polus", "dlekS", "Airship", "Fungle" };
			if (shipPrefabs != null && spawnableObjects != null)
			{
				for (int j = 0; j < shipPrefabs.Count; j++)
				{
					try
					{
						AssetReference val = shipPrefabs[j];
						if (val == null)
						{
							LogCheat($"  Ship[{j}] null");
							continue;
						}
						Object runtimeKey = val.RuntimeKey;
						string text = ((runtimeKey != null) ? runtimeKey.ToString() : null) ?? "?";
						int num = -1;
						for (int k = 0; k < ((Il2CppArrayBase<AssetReference>)(object)spawnableObjects).Length; k++)
						{
							if (((Il2CppArrayBase<AssetReference>)(object)spawnableObjects)[k] != null)
							{
								Object runtimeKey2 = ((Il2CppArrayBase<AssetReference>)(object)spawnableObjects)[k].RuntimeKey;
								if ((((runtimeKey2 != null) ? runtimeKey2.ToString() : null) ?? "") == text)
								{
									num = k;
									break;
								}
							}
						}
						string value = ((j < array.Length) ? array[j] : $"Map{j}");
						LogCheat($"  {value}: key={text} → SpawnId={((num >= 0) ? num.ToString() : "NOT FOUND")}");
					}
					catch (Exception ex)
					{
						LogCheat($"  Ship[{j}] error: {ex.Message}");
					}
				}
			}
			if ((Object)(object)ShipStatus.Instance != (Object)null)
			{
				int length2 = ((Component)ShipStatus.Instance).GetComponentsInChildren<InnerNetObject>().Length;
				LogCheat($"[DiagMaps] Current map: {((object)ShipStatus.Instance).GetType().Name} SpawnId={((InnerNetObject)ShipStatus.Instance).SpawnId} children={length2}");
			}
			else
			{
				LogCheat("[DiagMaps] No ShipStatus.Instance (in lobby)");
			}
		}
		catch (Exception ex2)
		{
			LogCheat("[DiagMaps] error: " + ex2.Message);
		}
	}

	private static void WriteSpawnSubMessage(MessageWriter w, uint spawnId, uint[] netIds)
	{
		w.StartMessage((byte)4);
		w.WritePacked(spawnId);
		w.WritePacked(-2);
		w.Write((byte)0);
		w.WritePacked(netIds.Length);
		for (int i = 0; i < netIds.Length; i++)
		{
			w.WritePacked(netIds[i]);
			w.StartMessage((byte)1);
			w.EndMessage();
		}
		w.EndMessage();
	}

	private static void _EnqueueMapSpawn(uint spawnId, uint[] netIds)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return;
		}
		int clientId = ((InnerNetClient)AmongUsClient.Instance).ClientId;
		int estimatedBytes = 20 + netIds.Length * 6;
		int[] array = new int[2] { -1, clientId };
		foreach (int targetClientId in array)
		{
			uint[] capturedNetIds = netIds;
			LabRpcArbiter.Enqueue(new LabRpcArbiter.LabOp
			{
				Consumer = LabRpcArbiter.Consumer.MapOps,
				Priority = LabRpcArbiter.Priority.High,
				TargetClientId = targetClientId,
				SendOption = (SendOption)1,
				EstimatedBytes = estimatedBytes,
				WriteSubMessage = delegate(MessageWriter w)
				{
					WriteSpawnSubMessage(w, spawnId, capturedNetIds);
				}
			});
		}
	}

	private static void _EnqueueMapDespawn(uint netId)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return;
		}
		int clientId = ((InnerNetClient)AmongUsClient.Instance).ClientId;
		int[] array = new int[2] { -1, clientId };
		foreach (int targetClientId in array)
		{
			LabRpcArbiter.Enqueue(new LabRpcArbiter.LabOp
			{
				Consumer = LabRpcArbiter.Consumer.MapOps,
				Priority = LabRpcArbiter.Priority.Critical,
				TargetClientId = targetClientId,
				SendOption = (SendOption)1,
				EstimatedBytes = 8,
				WriteSubMessage = delegate(MessageWriter w)
				{
					_WriteDespawnSub(w, netId);
				}
			});
		}
	}

	private static void _DespawnSelMap(int selfId)
	{
		if (_selMapNetIds == null)
		{
			return;
		}
		LabRpcArbiter.Register(LabRpcArbiter.Consumer.MapOps);
		uint[] selMapNetIds = _selMapNetIds;
		for (int i = 0; i < selMapNetIds.Length; i++)
		{
			_EnqueueMapDespawn(selMapNetIds[i]);
		}
		_selMapNetIds = null;
		try
		{
			if ((Object)(object)ShipStatus.Instance != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)ShipStatus.Instance).gameObject);
				ShipStatus.Instance = null;
			}
		}
		catch
		{
		}
		_selMapSpawnId = -1;
	}

	internal static void SelectMapBySpawnId(uint spawnId, int children)
	{
		if (!ServerData.Config.IsLoaded || (Object)(object)AmongUsClient.Instance == (Object)null || Time.time - _lastSelMapTime < 4f)
		{
			return;
		}
		_lastSelMapTime = Time.time;
		int clientId = ((InnerNetClient)AmongUsClient.Instance).ClientId;
		LabRpcArbiter.Register(LabRpcArbiter.Consumer.MapOps);
		if (_selMapSpawnId == (int)spawnId)
		{
			_DespawnSelMap(clientId);
			try
			{
				MapCheats.SpawnLobby();
				return;
			}
			catch
			{
				return;
			}
		}
		_DespawnSelMap(clientId);
		LobbyBehaviour instance = LobbyBehaviour.Instance;
		if ((Object)(object)instance != (Object)null)
		{
			try
			{
				if (instance != null)
				{
					((InnerNetObject)instance).Despawn();
				}
			}
			catch
			{
			}
			Object.Destroy((Object)(object)((Component)instance).gameObject);
		}
		uint num = NextGhostNetIdBase(children);
		uint[] array = new uint[children];
		for (int i = 0; i < children; i++)
		{
			array[i] = num + (uint)i;
		}
		_mapSwapInProgress = true;
		try
		{
			ShipStatus instance2 = ShipStatus.Instance;
			_savedCosmeticsCache = ((instance2 != null) ? instance2.CosmeticsCache : null);
		}
		catch
		{
		}
		_EnqueueMapSpawn(spawnId, array);
		_selMapNetIds = array;
		_selMapSpawnId = (int)spawnId;
		LogCheat($"SelMap: enqueued SpawnId={spawnId} NetId={num}");
	}

	internal static void ClearSelMap()
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return;
		}
		_DespawnSelMap(((InnerNetClient)AmongUsClient.Instance).ClientId);
		try
		{
			MapCheats.SpawnLobby();
		}
		catch
		{
		}
	}

	internal static string TestSpawnMap()
	{
		try
		{
			if ((Object)(object)AmongUsClient.Instance == (Object)null)
			{
				return "no client";
			}
			if ((Object)(object)ShipStatus.Instance != (Object)null && !_mapActive)
			{
				CaptureShipSpawnId();
				return $"captured SpawnId={_capturedShipSpawnId} children={_capturedShipChildCount}";
			}
			_ = ((InnerNetClient)AmongUsClient.Instance).ClientId;
			LabRpcArbiter.Register(LabRpcArbiter.Consumer.MapOps);
			if (_mapActive)
			{
				if (_mapNetIds != null)
				{
					uint[] mapNetIds = _mapNetIds;
					for (int i = 0; i < mapNetIds.Length; i++)
					{
						_EnqueueMapDespawn(mapNetIds[i]);
					}
				}
				MapCheats.FullTeardown();
				LogCheat("F4: map teardown via FullTeardown");
				return "map removed";
			}
			uint num = NextGhostNetIdBase(1);
			uint[] array = new uint[1];
			for (int j = 0; j < 1; j++)
			{
				array[j] = num + (uint)j;
			}
			_mapSwapInProgress = true;
			_EnqueueMapSpawn(0u, array);
			LogCheat($"F4: Skeld enqueued NetId={num}");
			LobbyBehaviour instance = LobbyBehaviour.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				uint netId = ((InnerNetObject)instance).NetId;
				try
				{
					if (instance != null)
					{
						((InnerNetObject)instance).Despawn();
					}
				}
				catch
				{
				}
				Object.Destroy((Object)(object)((Component)instance).gameObject);
				LogCheat($"F4: LobbyBehaviour despawned NetId={netId}");
			}
			_mapActive = true;
			_mapNetIds = array;
			return "map ON";
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[F4 Test] TestSpawnMap: " + ex.Message));
			return "error: " + ex.Message;
		}
	}

	internal static void ToggleAllMaps()
	{
		if (!ServerData.Config.IsLoaded || (Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return;
		}
		LabRpcArbiter.Register(LabRpcArbiter.Consumer.MapOps);
		if (_allMapsActive)
		{
			foreach (uint[] allMapNetId in _allMapNetIds)
			{
				for (int i = 0; i < allMapNetId.Length; i++)
				{
					_EnqueueMapDespawn(allMapNetId[i]);
				}
			}
			MapCheats.FullTeardown();
			LogCheat("AllMaps: teardown via FullTeardown");
			return;
		}
		LobbyBehaviour instance = LobbyBehaviour.Instance;
		if ((Object)(object)instance != (Object)null)
		{
			_EnqueueMapDespawn(((InnerNetObject)instance).NetId);
			Object.Destroy((Object)(object)((Component)instance).gameObject);
		}
		(uint, int, string)[] mAP_DEFS = _MAP_DEFS;
		for (int i = 0; i < mAP_DEFS.Length; i++)
		{
			(uint, int, string) tuple = mAP_DEFS[i];
			uint item = tuple.Item1;
			int item2 = tuple.Item2;
			string item3 = tuple.Item3;
			uint num = NextGhostNetIdBase(item2);
			uint[] array = new uint[item2];
			for (int j = 0; j < item2; j++)
			{
				array[j] = num + (uint)j;
			}
			_mapSwapInProgress = true;
			_EnqueueMapSpawn(item, array);
			_allMapNetIds.Add(array);
			LogCheat($"AllMaps: enqueued {item3} SpawnId={item} NetId={num}");
		}
		_allMapsActive = true;
	}

	internal static void ToggleCloneFollow()
	{
		if (!_cfActive && (Object)(object)ShipStatus.Instance != (Object)null)
		{
			LogCheat("Shadow Clones: activation blocked — not in lobby");
			return;
		}
		_cfActive = !_cfActive;
		_cfSynced.Clear();
		if (_cfActive)
		{
			_cfActivatedByOrbit = false;
			if (CheatConfig.ShadowClonesOrbit != null && CheatConfig.ShadowClonesOrbit.Value)
			{
				CheatConfig.ShadowClonesOrbit.Value = false;
				CheatConfig.Save();
			}
			LabRpcArbiter.Register(LabRpcArbiter.Consumer.ShadowClones);
			_CFSpawnAll();
			_cfReconcileNext = Time.time + _CfReconcileHz;
		}
		else
		{
			_cfActivatedByOrbit = false;
			if (CheatConfig.ShadowClonesOrbit != null && CheatConfig.ShadowClonesOrbit.Value)
			{
				CheatConfig.ShadowClonesOrbit.Value = false;
				CheatConfig.Save();
			}
			_CFDespawnAll();
			LabRpcArbiter.Unregister(LabRpcArbiter.Consumer.ShadowClones);
		}
	}

	internal static void ToggleCloneOrbit()
	{
		ConfigEntry<bool> shadowClonesOrbit = CheatConfig.ShadowClonesOrbit;
		if (shadowClonesOrbit == null)
		{
			return;
		}
		bool flag = !shadowClonesOrbit.Value;
		if (flag && !_cfActive && (Object)(object)ShipStatus.Instance != (Object)null)
		{
			LogCheat("Orbit Clones: activation blocked — not in lobby");
			return;
		}
		shadowClonesOrbit.Value = flag;
		CheatConfig.Save();
		if (flag)
		{
			if (!_cfActive)
			{
				_cfActivatedByOrbit = true;
				_cfActive = true;
				_cfSynced.Clear();
				LabRpcArbiter.Register(LabRpcArbiter.Consumer.ShadowClones);
				_CFSpawnAll();
				_cfReconcileNext = Time.time + _CfReconcileHz;
			}
		}
		else if (_cfActive && _cfActivatedByOrbit)
		{
			_cfActivatedByOrbit = false;
			_cfActive = false;
			_cfSynced.Clear();
			_CFDespawnAll();
			LabRpcArbiter.Unregister(LabRpcArbiter.Consumer.ShadowClones);
		}
	}

	internal static void ResyncClones()
	{
		if (_cfActive && !((Object)(object)ShipStatus.Instance != (Object)null))
		{
			_CFDespawnAll();
			_CFSpawnAll();
			_cfReconcileNext = Time.time + _CfReconcileHz;
			LogCheat("Shadow Clones: resynced (count/config change)");
		}
	}

	private static void _WriteCloneSpawnSub(MessageWriter w, uint spawnId, InnerNetObject[] children, int numChildren, uint[] ghostNetIds, byte sourcePlayerId, Vector2 pos)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		w.StartMessage((byte)4);
		w.WritePacked(spawnId);
		w.WritePacked(-2);
		w.Write((byte)0);
		w.WritePacked(numChildren);
		for (int i = 0; i < numChildren; i++)
		{
			InnerNetObject val = children[i];
			w.WritePacked(ghostNetIds[i]);
			w.StartMessage((byte)1);
			if ((Object)(object)((Il2CppObjectBase)val).TryCast<PlayerControl>() != (Object)null)
			{
				w.Write(true);
				w.Write(sourcePlayerId);
			}
			else if ((Object)(object)((Il2CppObjectBase)val).TryCast<CustomNetworkTransform>() != (Object)null)
			{
				w.Write((ushort)0);
				NetHelpers.WriteVector2(pos, w);
			}
			w.EndMessage();
		}
		w.EndMessage();
	}

	private static bool _CFSpawnForPlayer(PlayerControl player)
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null || (Object)(object)player == (Object)null || player.PlayerId == localPlayer.PlayerId)
		{
			return false;
		}
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return false;
		}
		if (_cfStates.ContainsKey(player.PlayerId))
		{
			return false;
		}
		if (!_CFIsPlayerValidForClone(player.PlayerId))
		{
			LogCheat($"CF: spawn refused PlayerId={player.PlayerId} (disconnected/no GameData)");
			return false;
		}
		Il2CppArrayBase<InnerNetObject> componentsInChildren = ((Component)localPlayer).GetComponentsInChildren<InnerNetObject>();
		int length = componentsInChildren.Length;
		if (length == 0)
		{
			return false;
		}
		int num = -1;
		for (int i = 0; i < length; i++)
		{
			if ((Object)(object)((Il2CppObjectBase)componentsInChildren[i]).TryCast<CustomNetworkTransform>() != (Object)null)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			LogCheat("CF: CNT not found");
			return false;
		}
		_cfCachedCntIdx = num;
		_cfCachedNumChildren = length;
		int w2 = ServerData.Config.W35;
		if (w2 <= 0)
		{
			LogCheat("CF: spawn aborted — server config unavailable");
			return false;
		}
		int num2 = Mathf.Clamp(CheatConfig.ShadowClonesCount?.Value ?? 1, 1, w2);
		Vector2 val = (((Object)(object)((Component)player).transform != (Object)null) ? Vector2.op_Implicit(((Component)player).transform.position) : Vector2.zero);
		uint spawnId = ((InnerNetObject)localPlayer).SpawnId;
		int clientId = ((InnerNetClient)AmongUsClient.Instance).ClientId;
		byte playerId = player.PlayerId;
		int estimatedBytes = ServerData.Config.W36 + length * ServerData.Config.W37;
		List<_CFState> list = new List<_CFState>(num2);
		for (int j = 0; j < num2; j++)
		{
			uint num3 = NextGhostNetIdBase(length);
			uint[] array = new uint[length];
			for (int k = 0; k < length; k++)
			{
				array[k] = num3 + (uint)k;
			}
			int[] array2 = new int[2] { -1, clientId };
			foreach (int targetClientId in array2)
			{
				uint[] capturedNetIds = array;
				Il2CppArrayBase<InnerNetObject> capturedChildren = componentsInChildren;
				int capturedNum = length;
				uint capturedSpawnId = spawnId;
				byte capturedSource = playerId;
				Vector2 capturedPos = val;
				LabRpcArbiter.Enqueue(new LabRpcArbiter.LabOp
				{
					Consumer = LabRpcArbiter.Consumer.ShadowClones,
					Priority = LabRpcArbiter.Priority.High,
					TargetClientId = targetClientId,
					SendOption = (SendOption)1,
					EstimatedBytes = estimatedBytes,
					WriteSubMessage = delegate(MessageWriter w)
					{
						//IL_0025: Unknown result type (might be due to invalid IL or missing references)
						_WriteCloneSpawnSub(w, capturedSpawnId, Il2CppArrayBase<InnerNetObject>.op_Implicit(capturedChildren), capturedNum, capturedNetIds, capturedSource, capturedPos);
					}
				});
			}
			float orbitPhase = ((num2 > 1) ? ((float)Math.PI * 2f * (float)j / (float)num2) : 0f);
			list.Add(new _CFState
			{
				netIds = array,
				cntIdx = num,
				sid = 1,
				orbitPhase = orbitPhase
			});
		}
		_cfStates[player.PlayerId] = list;
		LogCheat($"CF: {num2} clone(s) enqueued PlayerId={playerId}");
		return true;
	}

	private static void _CFSpawnAll()
	{
		_cfStates.Clear();
		_cfSynced.Clear();
		_cfSpawnQueue.Clear();
		_cfReconcilePending.Clear();
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return;
		}
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if (!((Object)(object)current == (Object)null) && current.PlayerId != localPlayer.PlayerId && _CFIsPlayerValidForClone(current.PlayerId))
			{
				_cfSpawnQueue.Enqueue(current.PlayerId);
			}
		}
		AmongUsClient instance = AmongUsClient.Instance;
		List<ClientData> val = ((instance != null) ? ((InnerNetClient)instance).allClients : null);
		if (val == null)
		{
			return;
		}
		int clientId = ((InnerNetClient)AmongUsClient.Instance).ClientId;
		for (int i = 0; i < val.Count; i++)
		{
			ClientData val2 = val[i];
			if (val2 != null && val2.Id != clientId)
			{
				_cfSynced.Add(val2.Id);
			}
		}
	}

	private static bool _CFIsPlayerValidForClone(byte pid)
	{
		try
		{
			GameData instance = GameData.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				return false;
			}
			NetworkedPlayerInfo playerById = instance.GetPlayerById(pid);
			if ((Object)(object)playerById == (Object)null)
			{
				return false;
			}
			if (playerById.Disconnected)
			{
				return false;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static void _CFDrainSpawnQueue()
	{
		if (_cfSpawnQueue.Count == 0)
		{
			return;
		}
		byte b = _cfSpawnQueue.Dequeue();
		if (_cfStates.ContainsKey(b))
		{
			return;
		}
		if (!_CFIsPlayerValidForClone(b))
		{
			LogCheat($"CF: drain skipped PlayerId={b} (disconnected)");
			return;
		}
		PlayerControl val = null;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && current.PlayerId == b)
			{
				val = current;
				break;
			}
		}
		if (!((Object)(object)val == (Object)null))
		{
			_CFSpawnForPlayer(val);
		}
	}

	private static void _CFEnqueueReconcileOne(int targetClientId, byte sourcePlayerId, List<_CFState> states)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null || states == null || states.Count == 0)
		{
			return;
		}
		Il2CppArrayBase<InnerNetObject> componentsInChildren = ((Component)localPlayer).GetComponentsInChildren<InnerNetObject>();
		int length = componentsInChildren.Length;
		if (length == 0)
		{
			return;
		}
		PlayerControl val = null;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && current.PlayerId == sourcePlayerId)
			{
				val = current;
				break;
			}
		}
		Vector2 val2 = (((Object)(object)val != (Object)null && (Object)(object)((Component)val).transform != (Object)null) ? Vector2.op_Implicit(((Component)val).transform.position) : Vector2.zero);
		foreach (_CFState state in states)
		{
			if (state != null && state.netIds != null && state.netIds.Length == length)
			{
				uint[] capturedNetIds = state.netIds;
				Il2CppArrayBase<InnerNetObject> capturedChildren = componentsInChildren;
				int capturedNum = length;
				uint capturedSpawnId = ((InnerNetObject)localPlayer).SpawnId;
				byte capturedSource = sourcePlayerId;
				Vector2 capturedPos = val2;
				int estimatedBytes = ServerData.Config.W36 + length * ServerData.Config.W37;
				LabRpcArbiter.Enqueue(new LabRpcArbiter.LabOp
				{
					Consumer = LabRpcArbiter.Consumer.ShadowClones,
					Priority = LabRpcArbiter.Priority.High,
					TargetClientId = targetClientId,
					SendOption = (SendOption)1,
					EstimatedBytes = estimatedBytes,
					WriteSubMessage = delegate(MessageWriter w)
					{
						//IL_0025: Unknown result type (might be due to invalid IL or missing references)
						_WriteCloneSpawnSub(w, capturedSpawnId, Il2CppArrayBase<InnerNetObject>.op_Implicit(capturedChildren), capturedNum, capturedNetIds, capturedSource, capturedPos);
					}
				});
			}
		}
	}

	private static void _CFReconcile()
	{
		if (!_cfActive || (Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return;
		}
		float time = Time.time;
		if (time < _cfReconcileNext)
		{
			return;
		}
		_cfReconcileNext = time + _CfReconcileHz;
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return;
		}
		List<byte> list = null;
		foreach (KeyValuePair<byte, List<_CFState>> cfState in _cfStates)
		{
			if (!_CFIsPlayerValidForClone(cfState.Key))
			{
				if (list == null)
				{
					list = new List<byte>();
				}
				list.Add(cfState.Key);
			}
		}
		if (list != null)
		{
			foreach (byte item in list)
			{
				LogCheat($"CF: pruning stale clone PlayerId={item} (disconnected)");
				try
				{
					_CFDespawnPlayer(item);
				}
				catch
				{
				}
			}
		}
		Enumerator<PlayerControl> enumerator3 = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator3.MoveNext())
		{
			PlayerControl current3 = enumerator3.Current;
			if (!((Object)(object)current3 == (Object)null) && current3.PlayerId != localPlayer.PlayerId && !_cfStates.ContainsKey(current3.PlayerId) && !_cfSpawnQueue.Contains(current3.PlayerId) && _CFIsPlayerValidForClone(current3.PlayerId))
			{
				_cfSpawnQueue.Enqueue(current3.PlayerId);
			}
		}
		int clientId = ((InnerNetClient)AmongUsClient.Instance).ClientId;
		List<ClientData> allClients = ((InnerNetClient)AmongUsClient.Instance).allClients;
		if (allClients != null)
		{
			for (int i = 0; i < allClients.Count; i++)
			{
				ClientData val = allClients[i];
				if (val == null)
				{
					continue;
				}
				int id = val.Id;
				if (id == clientId || _cfSynced.Contains(id))
				{
					continue;
				}
				_cfSynced.Add(id);
				List<byte> list2 = new List<byte>(_cfStates.Count);
				foreach (KeyValuePair<byte, List<_CFState>> cfState2 in _cfStates)
				{
					list2.Add(cfState2.Key);
				}
				if (list2.Count > 0)
				{
					_cfReconcilePending[id] = list2;
				}
				LogCheat($"CF: reconcile pending init cid={id} count={list2.Count}");
			}
		}
		int num = -1;
		byte b = 0;
		foreach (KeyValuePair<int, List<byte>> item2 in _cfReconcilePending)
		{
			List<byte> value = item2.Value;
			if (value != null && value.Count != 0)
			{
				num = item2.Key;
				b = value[0];
				value.RemoveAt(0);
				break;
			}
		}
		if (num >= 0 && _cfStates.TryGetValue(b, out var value2))
		{
			_CFEnqueueReconcileOne(num, b, value2);
		}
		if (_cfReconcilePending.Count <= 0)
		{
			return;
		}
		List<int> list3 = null;
		foreach (KeyValuePair<int, List<byte>> item3 in _cfReconcilePending)
		{
			if (item3.Value == null || item3.Value.Count == 0)
			{
				if (list3 == null)
				{
					list3 = new List<int>();
				}
				list3.Add(item3.Key);
			}
		}
		if (list3 == null)
		{
			return;
		}
		foreach (int item4 in list3)
		{
			_cfReconcilePending.Remove(item4);
		}
	}

	private static Vector2 _CFComputeClonePos(Vector2 playerPos, _CFState state, bool orbit, float radius, float speed, float t)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!orbit)
		{
			return playerPos;
		}
		float num = (orbit ? (state.orbitPhase + (float)state.orbitBroadcastStep * 0.4f) : (state.orbitPhase + t * speed));
		return new Vector2(playerPos.x + Mathf.Cos(num) * radius, playerPos.y + Mathf.Sin(num) * radius);
	}

	internal static void TickCloneFollow()
	{
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		if (!_cfActive)
		{
			return;
		}
		try
		{
			_CFDrainSpawnQueue();
		}
		catch
		{
		}
		try
		{
			_CFReconcile();
		}
		catch
		{
		}
		if (_cfStates.Count == 0 || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			return;
		}
		bool flag = CheatConfig.ShadowClonesOrbit?.Value ?? false;
		float num = (flag ? Math.Max(0.5f, 1f / _CfBaseHz) : (1f / _CfBaseHz));
		if (Time.time < _cfNextSend)
		{
			return;
		}
		_cfNextSend = Time.time + num;
		float radius = CheatConfig.ShadowCloneOrbitRadius?.Value ?? 1.5f;
		float speed = CheatConfig.ShadowCloneOrbitSpeed?.Value ?? 3f;
		float time = Time.time;
		int num2 = 0;
		foreach (KeyValuePair<byte, List<_CFState>> cfState in _cfStates)
		{
			List<_CFState> value = cfState.Value;
			if (value == null || value.Count == 0)
			{
				continue;
			}
			num2 += value.Count;
			PlayerControl val = null;
			Enumerator<PlayerControl> enumerator2 = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				PlayerControl current2 = enumerator2.Current;
				if ((Object)(object)current2 != (Object)null && current2.PlayerId == cfState.Key)
				{
					val = current2;
					break;
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			Vector2 playerPos = Vector2.op_Implicit(((Component)val).transform.position);
			foreach (_CFState item in value)
			{
				Vector2 val2 = _CFComputeClonePos(playerPos, item, flag, radius, speed, time);
				uint num3 = item.netIds[item.cntIdx];
				try
				{
					CustomNetworkTransform val3 = ((InnerNetClient)AmongUsClient.Instance).FindObjectByNetId<CustomNetworkTransform>(num3);
					if ((Object)(object)val3 != (Object)null)
					{
						((Component)val3).transform.position = new Vector3(val2.x, val2.y, val2.y / 1000f);
					}
				}
				catch
				{
				}
			}
		}
		int num4;
		if (flag)
		{
			int val4 = HostOutboundBudget.Remaining();
			num4 = Math.Min(8, val4);
			if (num4 <= 0)
			{
				return;
			}
		}
		else
		{
			num4 = num2;
		}
		int estimatedBytes = num4 * ServerData.Config.W37;
		LabRpcArbiter.Enqueue(new LabRpcArbiter.LabOp
		{
			Consumer = LabRpcArbiter.Consumer.ShadowClones,
			Priority = LabRpcArbiter.Priority.Normal,
			TargetClientId = -1,
			SendOption = (SendOption)0,
			EstimatedBytes = estimatedBytes,
			WriteSubMessage = _WriteAllCloneDataFlags
		});
	}

	private static void _WriteAllCloneDataFlags(MessageWriter w)
	{
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		bool flag = CheatConfig.ShadowClonesOrbit?.Value ?? false;
		float radius = CheatConfig.ShadowCloneOrbitRadius?.Value ?? 1.5f;
		float speed = CheatConfig.ShadowCloneOrbitSpeed?.Value ?? 3f;
		float time = Time.time;
		List<KeyValuePair<byte, _CFState>> list = new List<KeyValuePair<byte, _CFState>>(32);
		foreach (KeyValuePair<byte, List<_CFState>> cfState in _cfStates)
		{
			List<_CFState> value = cfState.Value;
			if (value == null || value.Count == 0)
			{
				continue;
			}
			foreach (_CFState item in value)
			{
				if (item != null && item.netIds != null)
				{
					list.Add(new KeyValuePair<byte, _CFState>(cfState.Key, item));
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		list.Sort((KeyValuePair<byte, _CFState> a, KeyValuePair<byte, _CFState> b) => a.Key.CompareTo(b.Key));
		int num;
		if (flag)
		{
			int val = HostOutboundBudget.Remaining();
			num = Math.Min(Math.Min(8, val), list.Count);
			if (num <= 0)
			{
				return;
			}
		}
		else
		{
			num = list.Count;
		}
		if (_orbitRoundRobinCursor >= list.Count)
		{
			_orbitRoundRobinCursor = 0;
		}
		int num2 = 0;
		int orbitRoundRobinCursor = _orbitRoundRobinCursor;
		for (int i = 0; i < list.Count; i++)
		{
			if (num2 >= num)
			{
				break;
			}
			int index = (orbitRoundRobinCursor + i) % list.Count;
			KeyValuePair<byte, _CFState> keyValuePair = list[index];
			byte key = keyValuePair.Key;
			_CFState value2 = keyValuePair.Value;
			PlayerControl val2 = null;
			Enumerator<PlayerControl> enumerator3 = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				PlayerControl current3 = enumerator3.Current;
				if ((Object)(object)current3 != (Object)null && current3.PlayerId == key)
				{
					val2 = current3;
					break;
				}
			}
			if (!((Object)(object)val2 == (Object)null))
			{
				if (flag)
				{
					value2.orbitBroadcastStep++;
				}
				Vector2 val3 = _CFComputeClonePos(Vector2.op_Implicit(((Component)val2).transform.position), value2, flag, radius, speed, time);
				uint num3 = value2.netIds[value2.cntIdx];
				ushort num4 = ++value2.sid;
				w.StartMessage((byte)1);
				w.WritePacked(num3);
				w.Write(num4);
				w.WritePacked(1);
				NetHelpers.WriteVector2(val3, w);
				w.EndMessage();
				num2++;
			}
		}
		if (flag && list.Count > 0)
		{
			_orbitRoundRobinCursor = (orbitRoundRobinCursor + num2) % list.Count;
		}
	}

	private static void _CFDespawnAll()
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			_cfStates.Clear();
			_cfSpawnQueue.Clear();
			return;
		}
		int clientId = ((InnerNetClient)AmongUsClient.Instance).ClientId;
		LabRpcArbiter.Register(LabRpcArbiter.Consumer.Cleanup);
		foreach (KeyValuePair<byte, List<_CFState>> cfState in _cfStates)
		{
			List<_CFState> value = cfState.Value;
			if (value == null)
			{
				continue;
			}
			foreach (_CFState item in value)
			{
				if (item == null || item.netIds == null)
				{
					continue;
				}
				uint[] netIds = item.netIds;
				foreach (uint num in netIds)
				{
					uint capturedNetId = num;
					int[] array = new int[2] { -1, clientId };
					foreach (int targetClientId in array)
					{
						LabRpcArbiter.Enqueue(new LabRpcArbiter.LabOp
						{
							Consumer = LabRpcArbiter.Consumer.Cleanup,
							Priority = LabRpcArbiter.Priority.Critical,
							TargetClientId = targetClientId,
							SendOption = (SendOption)1,
							EstimatedBytes = 8,
							WriteSubMessage = delegate(MessageWriter w)
							{
								_WriteDespawnSub(w, capturedNetId);
							}
						});
					}
				}
			}
		}
		_cfStates.Clear();
		_cfSynced.Clear();
		_cfSpawnQueue.Clear();
		_cfReconcilePending.Clear();
		LogCheat("CF: all clones despawn enqueued");
	}

	private static void _CFDespawnPlayer(byte playerId)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !_cfStates.TryGetValue(playerId, out var value))
		{
			return;
		}
		int clientId = ((InnerNetClient)AmongUsClient.Instance).ClientId;
		if (value != null)
		{
			foreach (_CFState item in value)
			{
				if (item == null || item.netIds == null)
				{
					continue;
				}
				uint[] netIds = item.netIds;
				foreach (uint num in netIds)
				{
					uint capturedNetId = num;
					int[] array = new int[2] { -1, clientId };
					foreach (int targetClientId in array)
					{
						LabRpcArbiter.Enqueue(new LabRpcArbiter.LabOp
						{
							Consumer = LabRpcArbiter.Consumer.Cleanup,
							Priority = LabRpcArbiter.Priority.Critical,
							TargetClientId = targetClientId,
							SendOption = (SendOption)1,
							EstimatedBytes = 8,
							WriteSubMessage = delegate(MessageWriter w)
							{
								_WriteDespawnSub(w, capturedNetId);
							}
						});
					}
				}
			}
		}
		_cfStates.Remove(playerId);
		LogCheat($"CF: clones for PlayerId={playerId} despawn enqueued (target left)");
	}

	internal static void OnClientLeft(int clientId, byte playerId)
	{
		try
		{
			_dummySynced.Remove(clientId);
		}
		catch
		{
		}
		try
		{
			_cfSynced.Remove(clientId);
		}
		catch
		{
		}
		try
		{
			_cfReconcilePending.Remove(clientId);
		}
		catch
		{
		}
		if (playerId != byte.MaxValue && _cfSpawnQueue.Count > 0)
		{
			try
			{
				List<byte> list = new List<byte>(_cfSpawnQueue.Count);
				foreach (byte item in _cfSpawnQueue)
				{
					if (item != playerId)
					{
						list.Add(item);
					}
				}
				_cfSpawnQueue.Clear();
				foreach (byte item2 in list)
				{
					_cfSpawnQueue.Enqueue(item2);
				}
			}
			catch
			{
			}
		}
		if (_cfActive && playerId != byte.MaxValue)
		{
			try
			{
				_CFDespawnPlayer(playerId);
			}
			catch
			{
			}
		}
	}

	internal static void ResetCloneFollow()
	{
		_cfActive = false;
		_cfActivatedByOrbit = false;
		_cfStates.Clear();
		_cfSynced.Clear();
		_cfReconcilePending.Clear();
		_orbitRoundRobinCursor = 0;
	}

	private static void LogCheat(string message)
	{
		Debug.Log(Object.op_Implicit("[Cheat] " + message));
	}

	private static string GenerateFakeToken(byte playerId = 0)
	{
		long ticks = DateTime.UtcNow.Ticks;
		AmongUsClient instance = AmongUsClient.Instance;
		string value = ((instance != null) ? ((InnerNetClient)instance).GameId.ToString() : null) ?? "0";
		string s = $"{playerId}-{ticks}-{value}";
		using SHA256 sHA = SHA256.Create();
		return Convert.ToBase64String(sHA.ComputeHash(Encoding.UTF8.GetBytes(s)));
	}

	private static string GenerateVerificationToken(byte playerId, byte scanCount, long timestamp)
	{
		return $"{playerId}-{scanCount}-{timestamp}-MODMENUCREW";
	}

	private static bool ValidateToken(string token, byte playerId, byte scanCount, long timestamp)
	{
		string text = GenerateVerificationToken(playerId, scanCount, timestamp);
		if (token == text)
		{
			return Math.Abs(DateTime.UtcNow.Ticks - timestamp) < 50000000;
		}
		return false;
	}

	internal static void CloseMeeting()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (!ModKeyValidator.V() || !IntegrityGuard.IsIntact)
		{
			ServerData.TriggerSilentDenial();
			return;
		}
		try
		{
			if (Object.op_Implicit((Object)(object)MeetingHud.Instance))
			{
				if ((Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmHost)
				{
					MeetingHud.Instance.RpcClose();
					LogCheat("Meeting closed for all clients (host, RpcClose).");
					return;
				}
				((InnerNetObject)MeetingHud.Instance).DespawnOnDestroy = false;
				Object.Destroy((Object)(object)((Component)MeetingHud.Instance).gameObject);
				((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f, false));
				PlayerControl.LocalPlayer.SetKillTimer(GameManager.Instance.LogicOptions.GetKillCooldown());
				ShipStatus.Instance.EmergencyCooldown = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
				((Component)Camera.main).GetComponent<FollowerCamera>().Locked = false;
				DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
				LogCheat("Meeting exited locally (non-host).");
			}
			else if ((Object)(object)ExileController.Instance != (Object)null)
			{
				ExileController.Instance.ReEnableGameplay();
				ExileController.Instance.WrapUp();
				LogCheat("Exile ended.");
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"Error in CloseMeeting: {value}"));
		}
	}

	private static void ForcePlayAnimation(byte animationType)
	{
		if ((Object)(object)PlayerControl.LocalPlayer == (Object)null)
		{
			return;
		}
		try
		{
			PlayerControl.LocalPlayer.PlayAnimation(animationType);
			PlayerControl.LocalPlayer.RpcPlayAnimation(animationType);
		}
		catch (Exception ex)
		{
			LogCheat($"[Animation] Error playing anim {animationType}: {ex.Message}");
		}
	}

	internal static void ProcessAnimationCheats()
	{
		if ((Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).IsGameStarted)
		{
			return;
		}
		try
		{
			GameOptionsManager instance = GameOptionsManager.Instance;
			byte? obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				IGameOptions currentGameOptions = instance.CurrentGameOptions;
				obj = ((currentGameOptions != null) ? new byte?(currentGameOptions.MapId) : null);
			}
			byte? b = obj;
			byte valueOrDefault = b.GetValueOrDefault();
			if (AnimShields)
			{
				if (valueOrDefault == 0 || valueOrDefault == 3)
				{
					ForcePlayAnimation(1);
				}
				AnimShields = false;
			}
			if (AnimAsteroids)
			{
				if (valueOrDefault == 0 || valueOrDefault == 2 || valueOrDefault == 3)
				{
					ForcePlayAnimation(6);
				}
				AnimAsteroids = false;
			}
			if (AnimEmptyGarbage)
			{
				if (valueOrDefault == 0 || valueOrDefault == 3)
				{
					ForcePlayAnimation(10);
				}
				AnimEmptyGarbage = false;
			}
			if (AnimCamsInUse && !_hasUsedCamsCheatBefore)
			{
				if (valueOrDefault != 1 && valueOrDefault != 5)
				{
					ShipStatus instance2 = ShipStatus.Instance;
					if (instance2 != null)
					{
						instance2.RpcUpdateSystem((SystemTypes)11, (byte)1);
					}
					_hasUsedCamsCheatBefore = true;
				}
				else
				{
					AnimCamsInUse = false;
				}
			}
			else if (!AnimCamsInUse && _hasUsedCamsCheatBefore)
			{
				ShipStatus instance3 = ShipStatus.Instance;
				if (instance3 != null)
				{
					instance3.RpcUpdateSystem((SystemTypes)11, (byte)0);
				}
				_hasUsedCamsCheatBefore = false;
			}
		}
		catch
		{
		}
	}

	internal static void RunCoroutine(IEnumerator routine)
	{
		HudManager instance = DestroyableSingleton<HudManager>.Instance;
		if (instance != null)
		{
			((MonoBehaviour)instance).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(routine));
		}
	}

	internal static void CompleteAllTasks()
	{
		if (!IntegrityGuard.IsIntact || !ModKeyValidator.V())
		{
			ServerData.TriggerSilentDenial();
		}
		else
		{
			if ((Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)PlayerControl.LocalPlayer.Data == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null)
			{
				return;
			}
			try
			{
				List<PlayerTask> myTasks = PlayerControl.LocalPlayer.myTasks;
				if (myTasks != null && myTasks.Count != 0)
				{
					int num = ((IEnumerable<PlayerTask>)myTasks.ToArray()).Count((PlayerTask t) => (Object)(object)t != (Object)null && !t.IsComplete);
					if (num != 0)
					{
						((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(CompleteAllTasksOptimized(num)));
					}
				}
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"Error completing tasks: {value}"));
			}
		}
	}

	[IteratorStateMachine(typeof(_003CCompleteAllTasksOptimized_003Ed__191))]
	internal static IEnumerator CompleteAllTasksOptimized(int totalPending)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCompleteAllTasksOptimized_003Ed__191(0)
		{
			totalPending = totalPending
		};
	}

	[IteratorStateMachine(typeof(_003CCompleteAllTasksWithDelay_003Ed__192))]
	internal static IEnumerator CompleteAllTasksWithDelay(float perTaskDelay = 0.1f)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCompleteAllTasksWithDelay_003Ed__192(0)
		{
			perTaskDelay = perTaskDelay
		};
	}

	internal static void TeleportToCursor()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		_fakeLagBypass = true;
		try
		{
			if ((Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)Camera.main == (Object)null)
			{
				return;
			}
			Vector3 mousePosition = Input.mousePosition;
			Vector3 val = Camera.main.ScreenToWorldPoint(mousePosition);
			val.z = ((Component)PlayerControl.LocalPlayer).transform.position.z;
			if (PlayerControl.LocalPlayer.inVent)
			{
				try
				{
					PlayerPhysics myPhysics = PlayerControl.LocalPlayer.MyPhysics;
					if (myPhysics != null)
					{
						myPhysics.ExitAllVents();
					}
				}
				catch
				{
				}
			}
			if (PlayerControl.LocalPlayer.onLadder)
			{
				try
				{
					PlayerPhysics myPhysics2 = PlayerControl.LocalPlayer.MyPhysics;
					if (myPhysics2 != null)
					{
						myPhysics2.ResetMoveState(true);
					}
				}
				catch
				{
				}
			}
			Collider2D collider = PlayerControl.LocalPlayer.Collider;
			bool enabled = collider != null && ((Behaviour)collider).enabled;
			if ((Object)(object)collider != (Object)null)
			{
				((Behaviour)collider).enabled = false;
			}
			((Component)PlayerControl.LocalPlayer).transform.position = val;
			if ((Object)(object)PlayerControl.LocalPlayer.NetTransform != (Object)null)
			{
				PlayerControl.LocalPlayer.NetTransform.SnapTo(Vector2.op_Implicit(val));
			}
			if ((Object)(object)collider != (Object)null)
			{
				((Behaviour)collider).enabled = enabled;
			}
			if ((Object)(object)AmongUsClient.Instance != (Object)null)
			{
				MessageWriter val2 = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)PlayerControl.LocalPlayer).NetId, (byte)21, (SendOption)1, -1);
				val2.Write(val.x);
				val2.Write(val.y);
				CustomNetworkTransform netTransform = PlayerControl.LocalPlayer.NetTransform;
				val2.Write((ushort)((netTransform != null) ? netTransform.lastSequenceId : 0));
				((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val2);
			}
			if (FakeLagEnabled)
			{
				_fakeLagLastPos = Vector2.op_Implicit(val);
				_fakeLagPosInit = true;
			}
			LogCheat($"Teleported to ({val.x:F1}, {val.y:F1})");
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[TeleportToCursor] Error: " + ex.Message));
		}
		finally
		{
			_fakeLagBypass = false;
		}
	}

	internal static void CheckTeleportInput()
	{
		if (!TeleportToCursorEnabled || !Input.GetMouseButtonDown(1))
		{
			return;
		}
		try
		{
			if ((Object)(object)MeetingHud.Instance != (Object)null)
			{
				return;
			}
		}
		catch
		{
		}
		TeleportToCursor();
	}

	internal static void KickAllFromVents()
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			AmongUsClient instance = AmongUsClient.Instance;
			if (instance == null || !((InnerNetClient)instance).AmHost)
			{
				return;
			}
			int num = 0;
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current == (Object)null || (Object)(object)current.Data == (Object)null || current.Data.IsDead || !current.inVent)
				{
					continue;
				}
				int num2 = 0;
				if ((Object)(object)ShipStatus.Instance != (Object)null && ShipStatus.Instance.AllVents != null)
				{
					float num3 = float.MaxValue;
					foreach (Vent item in (Il2CppArrayBase<Vent>)(object)ShipStatus.Instance.AllVents)
					{
						if (!((Object)(object)item == (Object)null))
						{
							float num4 = Vector2.Distance(Vector2.op_Implicit(((Component)current).transform.position), Vector2.op_Implicit(((Component)item).transform.position));
							if (num4 < num3)
							{
								num3 = num4;
								num2 = item.Id;
							}
						}
					}
				}
				current.MyPhysics.RpcBootFromVent(num2);
				num++;
			}
			if (num > 0)
			{
				NotifyUtils.Success($"Kicked {num} player(s) from vents");
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(Object.op_Implicit("[ModMenuCrew] KickAllFromVents: " + ex.Message));
		}
	}

	internal static void BypassScanner(bool value)
	{
		try
		{
			if (!((Object)(object)PlayerControl.LocalPlayer == (Object)null) && !((Object)(object)AmongUsClient.Instance == (Object)null))
			{
				byte b = (byte)(PlayerControl.LocalPlayer.scannerCount + 1);
				PlayerControl.LocalPlayer.scannerCount = b;
				PlayerControl.LocalPlayer.SetScanner(value, b);
				Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PlayerControl current = enumerator.Current;
					MessageWriter val = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)PlayerControl.LocalPlayer).NetId, (byte)15, (SendOption)1, ((InnerNetObject)current).OwnerId);
					val.Write(value);
					val.Write(b);
					((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("BypassScanner error: " + ex.Message));
		}
	}

	internal static void HandleScannerRPC(PlayerControl instance, byte callId, MessageReader reader)
	{
		if (callId != 15)
		{
			return;
		}
		try
		{
			bool flag = reader.ReadBoolean();
			byte b = reader.ReadByte();
			instance.SetScanner(flag, b);
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("HandleScannerRPC error: " + ex.Message));
		}
	}

	[IteratorStateMachine(typeof(_003CBypassScannerWithTimeout_003Ed__200))]
	internal static IEnumerator BypassScannerWithTimeout(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBypassScannerWithTimeout_003Ed__200(0)
		{
			duration = duration
		};
	}

	internal static void UpdateScannerBypass()
	{
		if (ScannerBypassEnabled && _scannerAutoDisableTime > 0f && Time.time >= _scannerAutoDisableTime)
		{
			SetScannerBypass(enabled: false);
		}
	}

	internal static void SetScannerBypass(bool enabled)
	{
		if (ScannerBypassEnabled != enabled)
		{
			ScannerBypassEnabled = enabled;
			BypassScanner(enabled);
			if (enabled)
			{
				_scannerAutoDisableTime = Time.time + SCANNER_AUTO_DISABLE_DURATION;
				LogCheat($"Scanner Bypass ON (auto-off in {SCANNER_AUTO_DISABLE_DURATION}s)");
			}
			else
			{
				_scannerAutoDisableTime = 0f;
				LogCheat("Scanner Bypass OFF");
			}
		}
	}

	internal static float GetScannerBypassRemainingTime()
	{
		if (!ScannerBypassEnabled || _scannerAutoDisableTime <= 0f)
		{
			return 0f;
		}
		return Mathf.Max(0f, _scannerAutoDisableTime - Time.time);
	}

	internal static void SetMedbayScan(bool enabled)
	{
		if (MedbayScanEnabled == enabled)
		{
			return;
		}
		MedbayScanEnabled = enabled;
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null)
		{
			MedbayScanEnabled = false;
			return;
		}
		if (enabled)
		{
			MonoBehaviour val = (MonoBehaviour)(((object)DestroyableSingleton<HudManager>.Instance) ?? ((object)localPlayer));
			if (_walkScanCoroutine != null)
			{
				try
				{
					val.StopCoroutine(_walkScanCoroutine);
				}
				catch
				{
				}
			}
			_walkScanCoroutine = val.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(WalkScanRoutine()));
			Debug.Log(Object.op_Implicit("[WalkScan] Coroutine started on " + ((object)val).GetType().Name));
			return;
		}
		MonoBehaviour val2 = (MonoBehaviour)(((object)DestroyableSingleton<HudManager>.Instance) ?? ((object)localPlayer));
		if (_walkScanCoroutine != null)
		{
			try
			{
				val2.StopCoroutine(_walkScanCoroutine);
			}
			catch
			{
			}
			_walkScanCoroutine = null;
		}
		WalkScanCleanup();
	}

	private static void WalkScanCleanup()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if ((Object)(object)localPlayer == (Object)null)
			{
				return;
			}
			if (_walkPhysicsCoroutine != null)
			{
				try
				{
					((MonoBehaviour)localPlayer).StopCoroutine(_walkPhysicsCoroutine);
				}
				catch
				{
				}
				_walkPhysicsCoroutine = null;
			}
			try
			{
				localPlayer.MyPhysics.body.velocity = Vector2.zero;
			}
			catch
			{
			}
			BroadcastScannerDirect(localPlayer, on: false);
			byte playerId = localPlayer.PlayerId;
			if ((Object)(object)ShipStatus.Instance != (Object)null)
			{
				ShipStatus.Instance.RpcUpdateSystem((SystemTypes)10, (byte)(playerId | 0x40u));
			}
			try
			{
				((Component)Camera.main).GetComponent<FollowerCamera>().Locked = false;
			}
			catch
			{
			}
			if ((Object)(object)localPlayer.Collider != (Object)null)
			{
				((Behaviour)localPlayer.Collider).enabled = true;
			}
			localPlayer.moveable = true;
		}
		catch
		{
		}
		LogCheat("Walk Scan OFF");
	}

	private static void BroadcastScannerDirect(PlayerControl lp, bool on)
	{
		byte b2 = (lp.scannerCount += 1);
		lp.MyPhysics.Animations.PlayScanner(on, false, lp.cosmetics.FlipX);
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			try
			{
				MessageWriter val = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)lp).NetId, (byte)15, (SendOption)1, ((InnerNetObject)current).OwnerId);
				val.Write(on);
				val.Write(b2);
				((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val);
			}
			catch
			{
			}
		}
	}

	private static void StartWalkTo(PlayerControl lp, Vector2 target)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (_walkPhysicsCoroutine != null)
		{
			try
			{
				((MonoBehaviour)lp).StopCoroutine(_walkPhysicsCoroutine);
			}
			catch
			{
			}
		}
		_walkPhysicsCoroutine = ((MonoBehaviour)lp).StartCoroutine(lp.MyPhysics.WalkPlayerTo(target, 0.001f, 1f, false));
	}

	[IteratorStateMachine(typeof(_003CWalkScanRoutine_003Ed__210))]
	private static IEnumerator WalkScanRoutine()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWalkScanRoutine_003Ed__210(0);
	}

	internal static void UpdateWalkInVent()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		ConfigEntry<bool> walkInVent = CheatConfig.WalkInVent;
		if (walkInVent == null || !walkInVent.Value)
		{
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (!((Object)(object)localPlayer == (Object)null) && localPlayer.inVent)
		{
			localPlayer.moveable = true;
			if ((Object)(object)localPlayer.MyPhysics != (Object)null)
			{
				localPlayer.MyPhysics.body.velocity = Vector2.zero;
			}
		}
	}

	internal static void LocalVisualScanForEveryone(float duration = 2f)
	{
		if (!((Object)(object)PlayerControl.LocalPlayer == (Object)null))
		{
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				LocalToggleScanner(enumerator.Current, on: true);
			}
			((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(LocalDisableScansAfterDelay(duration)));
		}
	}

	private static void LocalToggleScanner(PlayerControl player, bool on)
	{
		try
		{
			byte b2 = (player.scannerCount += 1);
			player.SetScanner(on, b2);
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"LocalToggleScanner error ({((player != null) ? ((Object)player).name : null)}): {value}"));
		}
	}

	[IteratorStateMachine(typeof(_003CLocalDisableScansAfterDelay_003Ed__214))]
	private static IEnumerator LocalDisableScansAfterDelay(float delay)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CLocalDisableScansAfterDelay_003Ed__214(0)
		{
			delay = delay
		};
	}

	internal static void KillAll(bool crewOnly = false, bool impostorsOnly = false)
	{
		if (!IntegrityGuard.IsIntact || !ModKeyValidator.V())
		{
			ServerData.TriggerSilentDenial();
			return;
		}
		if ((Object)(object)ShipStatus.Instance == (Object)null || (Object)(object)PlayerControl.LocalPlayer == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("ShipStatus or local player not found."));
			return;
		}
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("AmongUsClient not found."));
			return;
		}
		try
		{
			bool flag = !((InnerNetClient)AmongUsClient.Instance).IsGameStarted;
			List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
			float num = 0f;
			for (int i = 0; i < allPlayerControls.Count; i++)
			{
				PlayerControl val = allPlayerControls[i];
				if (!((Object)(object)val == (Object)null) && !((Object)(object)val == (Object)(object)PlayerControl.LocalPlayer) && IsValidKillTarget(val, crewOnly, impostorsOnly))
				{
					if (flag)
					{
						ExecuteKillBypass(PlayerControl.LocalPlayer, val, num);
					}
					else
					{
						BroadcastKillBypass(PlayerControl.LocalPlayer, val, num);
					}
					num += (float)random.Next(100, 300) / 1000f;
				}
			}
			LogCheat("Mass kill completed with bypass.");
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"Error in KillAll: {value}"));
		}
	}

	private static bool IsValidKillTarget(PlayerControl target, bool crewOnly, bool impostorsOnly)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		if (!((Object)(object)target == (Object)null))
		{
			NetworkedPlayerInfo data = target.Data;
			if (!((Object)(object)((data != null) ? data.Role : null) == (Object)null) && !target.Data.IsDead)
			{
				if (crewOnly)
				{
					return (int)target.Data.Role.TeamType == 0;
				}
				if (impostorsOnly)
				{
					return (int)target.Data.Role.TeamType == 1;
				}
				return true;
			}
		}
		return false;
	}

	internal static void HostForceKillPlayer(PlayerControl target)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected I4, but got Unknown
		if (!IntegrityGuard.IsIntact || !ModKeyValidator.V())
		{
			ServerData.TriggerSilentDenial();
			return;
		}
		if ((Object)(object)target == (Object)null || (Object)(object)target.Data == (Object)null || target.Data.IsDead)
		{
			Debug.LogWarning(Object.op_Implicit("[HostKill] Invalid target or already dead."));
			return;
		}
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[HostKill] Only the host can use this method."));
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return;
		}
		try
		{
			MurderResultFlags val = (MurderResultFlags)9;
			localPlayer.MurderPlayer(target, val);
			if ((Object)(object)target.Data != (Object)null && !target.Data.IsDead)
			{
				target.Data.IsDead = true;
			}
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && !((Object)(object)current == (Object)(object)PlayerControl.LocalPlayer))
				{
					MessageWriter val2 = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)localPlayer).NetId, (byte)12, (SendOption)1, ((InnerNetObject)current).OwnerId);
					MessageExtensions.WriteNetObject(val2, (InnerNetObject)(object)target);
					val2.Write((int)val);
					((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val2);
				}
			}
			if ((Object)(object)target.cosmetics != (Object)null)
			{
				((Component)target).gameObject.layer = LayerMask.NameToLayer("Ghost");
			}
			NetworkedPlayerInfo data = target.Data;
			LogCheat("[HostKill] " + ((data != null) ? data.PlayerName : null) + " killed by host successfully.");
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[HostKill] Error killing player: {value}"));
		}
	}

	private static void ExecuteKillBypass(PlayerControl killer, PlayerControl target, float delay)
	{
		if ((Object)(object)target == (Object)null || (Object)(object)killer == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("Target or killer not found."));
			return;
		}
		if (delay > 0f)
		{
			((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(DelayedKillBypass(killer, target, delay)));
			return;
		}
		killer.MurderPlayer(target, (MurderResultFlags)9);
		LogCheat($"[Bypass] {target.PlayerId} local kill attempt (awaiting server validation).");
	}

	private static void BroadcastKillBypass(PlayerControl killer, PlayerControl target, float delay)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || (Object)(object)killer == (Object)null || (Object)(object)target == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("AmongUsClient/killer/target not found."));
			return;
		}
		if (delay > 0f)
		{
			((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(DelayedKillBypass(killer, target, delay, broadcast: true)));
			return;
		}
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if (!((Object)(object)current == (Object)(object)PlayerControl.LocalPlayer))
			{
				MessageWriter val = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)killer).NetId, (byte)12, (SendOption)1, ((InnerNetObject)current).OwnerId);
				MessageExtensions.WriteNetObject(val, (InnerNetObject)(object)target);
				val.Write(9);
				((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val);
			}
		}
		LogCheat($"[Bypass] Kill of {target.PlayerId} broadcast to all (awaiting server validation).");
	}

	[IteratorStateMachine(typeof(_003CDelayedKillBypass_003Ed__220))]
	private static IEnumerator DelayedKillBypass(PlayerControl killer, PlayerControl target, float delay, bool broadcast = false)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDelayedKillBypass_003Ed__220(0)
		{
			killer = killer,
			target = target,
			delay = delay,
			broadcast = broadcast
		};
	}

	internal static void RevealImpostors()
	{
		try
		{
			AmongUsClient instance = AmongUsClient.Instance;
			if (instance == null || !((InnerNetClient)instance).IsGameStarted)
			{
				LogCheat("Reveal Sus: Only works during a game!");
				return;
			}
			IsRevealSusActive = !IsRevealSusActive;
			if (CheatConfig.RevealSus != null)
			{
				CheatConfig.RevealSus.Value = IsRevealSusActive;
			}
			if (IsRevealSusActive)
			{
				LogCheat("Reveal Sus ON! (Names will update constantly)");
				return;
			}
			LogCheat("Reveal Sus OFF!");
			try
			{
				Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PlayerControl current = enumerator.Current;
					if ((Object)(object)((current != null) ? current.Data : null) != (Object)null)
					{
						CosmeticsLayer cosmetics = current.cosmetics;
						if ((Object)(object)((cosmetics != null) ? cosmetics.nameText : null) != (Object)null)
						{
							((TMP_Text)current.cosmetics.nameText).text = current.Data.PlayerName;
						}
					}
				}
			}
			catch
			{
			}
			try
			{
				MMCIdentification.ReapplyAllMMCTags();
			}
			catch
			{
			}
			try
			{
				PlayerInfoDisplay.UpdatePlayerInfoTags();
			}
			catch
			{
			}
			try
			{
				if (!((Object)(object)MeetingHud.Instance != (Object)null) || MeetingHud.Instance.playerStates == null)
				{
					return;
				}
				foreach (PlayerVoteArea item in (Il2CppArrayBase<PlayerVoteArea>)(object)MeetingHud.Instance.playerStates)
				{
					if (!((Object)(object)item == (Object)null))
					{
						GameData instance2 = GameData.Instance;
						NetworkedPlayerInfo val = ((instance2 != null) ? instance2.GetPlayerById(item.TargetPlayerId) : null);
						if ((Object)(object)val != (Object)null && (Object)(object)item.NameText != (Object)null)
						{
							((TMP_Text)item.NameText).text = val.PlayerName;
						}
					}
				}
			}
			catch
			{
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"RevealImpostors Error: {value}"));
		}
	}

	internal static string GetSpecificRoleName(RoleBehaviour role)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected I4, but got Unknown
		if ((Object)(object)role == (Object)null)
		{
			return "UNKNOWN";
		}
		try
		{
			RoleTypes role2 = role.Role;
			return (int)role2 switch
			{
				1 => "IMPOSTOR", 
				5 => "SHAPESHIFTER", 
				9 => "PHANTOM", 
				18 => "VIPER", 
				0 => "CREWMATE", 
				3 => "ENGINEER", 
				2 => "SCIENTIST", 
				8 => "NOISEMAKER", 
				10 => "TRACKER", 
				4 => "GUARDIAN ANGEL", 
				6 => "CREWMATE GHOST", 
				7 => "IMPOSTOR GHOST", 
				_ => ((object)(RoleTypes)role2).ToString().ToUpper(), 
			};
		}
		catch
		{
			try
			{
				return role.NiceName?.ToUpper() ?? "UNKNOWN";
			}
			catch
			{
				return "UNKNOWN";
			}
		}
	}

	private static string GetSpecificRoleName(RoleTypes roleType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected I4, but got Unknown
		return (int)roleType switch
		{
			1 => "IMPOSTOR", 
			5 => "SHAPESHIFTER", 
			9 => "PHANTOM", 
			18 => "VIPER", 
			0 => "CREWMATE", 
			3 => "ENGINEER", 
			2 => "SCIENTIST", 
			8 => "NOISEMAKER", 
			10 => "TRACKER", 
			4 => "GUARDIAN ANGEL", 
			6 => "CREWMATE GHOST", 
			7 => "IMPOSTOR GHOST", 
			_ => roleType.ToString().ToUpper(), 
		};
	}

	internal static void UpdateRevealSus()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Invalid comparison between Unknown and I4
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Invalid comparison between Unknown and I4
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Invalid comparison between Unknown and I4
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Invalid comparison between Unknown and I4
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Invalid comparison between Unknown and I4
		if (!IsRevealSusActive || PlayerControl.AllPlayerControls == null)
		{
			return;
		}
		bool flag = !(CheatConfig.HideMMCStar?.Value ?? false);
		bool flag2 = CheatConfig.ShowKillCooldowns?.Value ?? false;
		bool flag3 = CheatConfig.ShowPlayerInfo?.Value ?? false;
		string value = null;
		if (flag)
		{
			value = ColorUtility.ToHtmlStringRGB(Color.HSVToRGB(Time.time * 0.5f % 1f, 0.7f, 1f));
		}
		try
		{
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)((current != null) ? current.Data : null) == (Object)null)
				{
					continue;
				}
				CosmeticsLayer cosmetics = current.cosmetics;
				if ((Object)(object)((cosmetics != null) ? cosmetics.nameText : null) == (Object)null)
				{
					continue;
				}
				bool flag4;
				string specificRoleName;
				if ((Object)(object)current.Data.Role != (Object)null)
				{
					flag4 = current.Data.Role.IsImpostor;
					specificRoleName = GetSpecificRoleName(current.Data.Role);
				}
				else
				{
					RoleTypes roleType = current.Data.RoleType;
					flag4 = (int)roleType == 1 || (int)roleType == 5 || (int)roleType == 9 || (int)roleType == 18 || (int)roleType == 7;
					specificRoleName = GetSpecificRoleName(roleType);
				}
				string playerName = current.Data.PlayerName;
				bool num = flag && MMCIdentification.IsMMCPlayer(current.PlayerId);
				byte playerId = current.PlayerId;
				PlayerControl localPlayer = PlayerControl.LocalPlayer;
				bool flag5 = playerId == ((localPlayer != null) ? new byte?(localPlayer.PlayerId) : null);
				string value2 = (flag4 ? "#FF3333" : "#99CCFF");
				_revealSusSb.Clear();
				if (num)
				{
					if (flag5)
					{
						_revealSusSb.Append("<color=#").Append(value).Append(">★</color> ");
					}
					else
					{
						_revealSusSb.Append("<color=#").Append(value).Append("><b>★ MMC ★</b></color> ");
					}
				}
				_revealSusSb.Append("<color=").Append(value2).Append(">")
					.Append(playerName)
					.Append("</color>");
				_revealSusSb.Append("\n<size=60%><color=").Append(value2).Append("><b>")
					.Append(specificRoleName)
					.Append("</b></color></size>");
				if (flag2 && flag4 && !current.Data.IsDead)
				{
					float killTimer = current.killTimer;
					string value3 = ((killTimer <= 0.1f) ? "FF2222" : ((killTimer < 2f) ? "FF4444" : ((!(killTimer < 5f)) ? "AAAAAA" : "FF9922")));
					_revealSusSb.Append("\n<size=50%><color=#").Append(value3).Append("><b>");
					if (killTimer <= 0.1f)
					{
						_revealSusSb.Append("⚠ CAN KILL!");
					}
					else
					{
						_revealSusSb.Append(killTimer.ToString("F1")).Append('s');
					}
					_revealSusSb.Append("</b></color></size>");
				}
				if (flag3)
				{
					string text = PlayerInfoDisplay.BuildInfoSuffix(current);
					if (text != null)
					{
						_revealSusSb.Append(text);
					}
				}
				string text2 = _revealSusSb.ToString();
				if (((TMP_Text)current.cosmetics.nameText).text != text2)
				{
					((TMP_Text)current.cosmetics.nameText).text = text2;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(Object.op_Implicit("[RevealSus] Frame error: " + ex.Message));
		}
	}

	internal static void UpdateRevealSusMeeting()
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Invalid comparison between Unknown and I4
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Invalid comparison between Unknown and I4
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Invalid comparison between Unknown and I4
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Invalid comparison between Unknown and I4
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Invalid comparison between Unknown and I4
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		if (!IsRevealSusActive)
		{
			return;
		}
		MeetingHud instance = MeetingHud.Instance;
		if ((Object)(object)instance == (Object)null || instance.playerStates == null)
		{
			return;
		}
		bool flag = !(CheatConfig.HideMMCStar?.Value ?? false);
		try
		{
			foreach (PlayerVoteArea item in (Il2CppArrayBase<PlayerVoteArea>)(object)instance.playerStates)
			{
				if ((Object)(object)item == (Object)null || (Object)(object)item.NameText == (Object)null)
				{
					continue;
				}
				byte targetPlayerId = item.TargetPlayerId;
				PlayerControl val = null;
				Enumerator<PlayerControl> enumerator2 = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					PlayerControl current2 = enumerator2.Current;
					if ((Object)(object)current2 != (Object)null && current2.PlayerId == targetPlayerId)
					{
						val = current2;
						break;
					}
				}
				if ((Object)(object)((val != null) ? val.Data : null) == (Object)null)
				{
					continue;
				}
				bool flag2;
				string specificRoleName;
				if ((Object)(object)val.Data.Role != (Object)null)
				{
					flag2 = val.Data.Role.IsImpostor;
					specificRoleName = GetSpecificRoleName(val.Data.Role);
				}
				else
				{
					RoleTypes roleType = val.Data.RoleType;
					flag2 = (int)roleType == 1 || (int)roleType == 5 || (int)roleType == 9 || (int)roleType == 18 || (int)roleType == 7;
					specificRoleName = GetSpecificRoleName(roleType);
				}
				string playerName = val.Data.PlayerName;
				string value = (flag2 ? "#FF3333" : "#99CCFF");
				_revealSusMeetingSb.Clear();
				if (flag && MMCIdentification.IsMMCPlayer(targetPlayerId))
				{
					PlayerControl localPlayer = PlayerControl.LocalPlayer;
					bool num = targetPlayerId == ((localPlayer != null) ? new byte?(localPlayer.PlayerId) : null);
					string value2 = ColorUtility.ToHtmlStringRGB(Color.HSVToRGB(Time.time * 0.5f % 1f, 0.7f, 1f));
					if (num)
					{
						_revealSusMeetingSb.Append("<color=#").Append(value2).Append(">★</color> ");
					}
					else
					{
						_revealSusMeetingSb.Append("<color=#").Append(value2).Append("><b>★ MMC ★</b></color> ");
					}
				}
				_revealSusMeetingSb.Append("<color=").Append(value).Append(">")
					.Append(playerName)
					.Append("</color>");
				_revealSusMeetingSb.Append(" <size=70%><color=").Append(value).Append(">[")
					.Append(specificRoleName)
					.Append("]</color></size>");
				string text = _revealSusMeetingSb.ToString();
				if (((TMP_Text)item.NameText).text != text)
				{
					((TMP_Text)item.NameText).text = text;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(Object.op_Implicit("[RevealSusMeeting] Frame error: " + ex.Message));
		}
	}

	internal static void ChangePlayerName(PlayerControl player, string newName)
	{
		if ((Object)(object)player == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("Player not found."));
			return;
		}
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("You must be host to use this cheat."));
			return;
		}
		try
		{
			player.Data.PlayerName = newName;
			player.Data.MarkDirty();
			MessageWriter val = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)player).NetId, (byte)6, (SendOption)1, -1);
			val.Write(newName);
			val.Write(GenerateFakeToken(player.PlayerId));
			((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val);
			LogCheat($"Player {player.PlayerId} name changed to {newName} and synced to all.");
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"Error changing name: {value}"));
		}
	}

	internal static void ToggleInvisibility(bool enable)
	{
		if ((Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("Invisibility not enabled: local player not found."));
			return;
		}
		try
		{
			if (enable)
			{
				if (invisibilityCoroutine != null)
				{
					((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StopCoroutine(invisibilityCoroutine);
				}
				invisibilityCoroutine = ((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(SendFakePositionCoroutine()));
				((Component)PlayerControl.LocalPlayer.cosmetics.nameText).gameObject.SetActive(false);
				((Component)PlayerControl.LocalPlayer.cosmetics.hat).gameObject.SetActive(false);
				((Component)PlayerControl.LocalPlayer.cosmetics.skin).gameObject.SetActive(false);
				Debug.Log(Object.op_Implicit("Invisibility enabled (local only, no host)."));
			}
			else
			{
				if (invisibilityCoroutine != null)
				{
					((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StopCoroutine(invisibilityCoroutine);
					invisibilityCoroutine = null;
				}
				((Component)PlayerControl.LocalPlayer.cosmetics.nameText).gameObject.SetActive(true);
				((Component)PlayerControl.LocalPlayer.cosmetics.hat).gameObject.SetActive(true);
				((Component)PlayerControl.LocalPlayer.cosmetics.skin).gameObject.SetActive(true);
				Debug.Log(Object.op_Implicit("Invisibility disabled (local only, no host)."));
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"Error toggling invisibility: {value}"));
		}
	}

	[IteratorStateMachine(typeof(_003CSendFakePositionCoroutine_003Ed__233))]
	private static IEnumerator SendFakePositionCoroutine()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSendFakePositionCoroutine_003Ed__233(0);
	}

	internal static void IncreaseVision(float multiplier = 3f)
	{
		zoomOutEnabled = true;
		customZoomValue = multiplier;
		Debug.Log(Object.op_Implicit($"[Cheat] Zoom Out enabled: {customZoomValue}"));
	}

	internal static void ResetSatelliteIfNotInGame()
	{
		try
		{
			if (!IsOnActiveMap() && (Object)(object)Camera.main != (Object)null && Camera.main.orthographicSize > 4f)
			{
				Camera.main.orthographicSize = _defaultOrthoSize;
			}
		}
		catch
		{
		}
	}

	internal static void ResetSatelliteState()
	{
		try
		{
			CheatConfig.VisionMultiplier = 3f;
			try
			{
				ServerData.SetSliderValueInternal("cheat_vision", 3f);
			}
			catch
			{
			}
			if ((Object)(object)Camera.main != (Object)null && Camera.main.orthographicSize > 4f)
			{
				Camera.main.orthographicSize = _defaultOrthoSize;
			}
			try
			{
				HudManager instance = DestroyableSingleton<HudManager>.Instance;
				Camera val = ((instance != null) ? instance.UICamera : null);
				if ((Object)(object)val != (Object)null)
				{
					val.orthographicSize = _defaultOrthoSize;
				}
			}
			catch
			{
			}
			try
			{
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
			catch
			{
			}
		}
		catch
		{
		}
	}

	internal static void ClearEndGameState()
	{
		_endGameActive = false;
	}

	internal static void ResetVision()
	{
		zoomOutEnabled = false;
		customZoomValue = _defaultOrthoSize;
		if ((Object)(object)Camera.main != (Object)null)
		{
			Camera.main.orthographicSize = _defaultOrthoSize;
		}
		Debug.Log(Object.op_Implicit("[Cheat] Zoom Out disabled"));
	}

	internal static void ForceUIRecalculation()
	{
		try
		{
			if ((Delegate)(object)ResolutionManager.ResolutionChanged != (Delegate)null)
			{
				ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / (float)Screen.height, Screen.width, Screen.height, Screen.fullScreen);
			}
		}
		catch
		{
		}
	}

	private static bool IsInBlockingUI()
	{
		try
		{
			if ((Object)(object)MeetingHud.Instance != (Object)null)
			{
				return true;
			}
			if ((Object)(object)ExileController.Instance != (Object)null)
			{
				return true;
			}
			if ((Object)(object)Minigame.Instance != (Object)null)
			{
				return true;
			}
			if ((Object)(object)MapBehaviour.Instance != (Object)null && MapBehaviour.Instance.IsOpen)
			{
				return true;
			}
			if ((Object)(object)PlayerCustomizationMenu.Instance != (Object)null)
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	internal static void SafeMassKill()
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("You must be host to use this cheat."));
			return;
		}
		((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(KillAllCoroutine(crewOnly: true)));
		LogCheat("Safe mass kill of crewmates started.");
	}

	[IteratorStateMachine(typeof(_003CKillAllCoroutine_003Ed__251))]
	private static IEnumerator KillAllCoroutine(bool crewOnly = false, bool impostorsOnly = false)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CKillAllCoroutine_003Ed__251(0)
		{
			crewOnly = crewOnly,
			impostorsOnly = impostorsOnly
		};
	}

	private static bool ValidatePlayer(byte playerId)
	{
		List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
		if (allPlayerControls == null)
		{
			return false;
		}
		for (int i = 0; i < allPlayerControls.Count; i++)
		{
			if ((Object)(object)allPlayerControls[i] != (Object)null && allPlayerControls[i].PlayerId == playerId)
			{
				return true;
			}
		}
		return false;
	}

	internal static void ForceStartGame()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I4
		if (!IntegrityGuard.IsIntact || !ModKeyValidator.V())
		{
			ServerData.TriggerSilentDenial();
			return;
		}
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("[ForceStart] AmongUsClient not available."));
			return;
		}
		if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[ForceStart] Only the host can force start the game."));
			return;
		}
		if ((int)((InnerNetClient)AmongUsClient.Instance).GameState == 2)
		{
			Debug.LogWarning(Object.op_Implicit("[ForceStart] Game already in progress."));
			return;
		}
		try
		{
			int count = PlayerControl.AllPlayerControls.Count;
			if (count < 1)
			{
				Debug.LogWarning(Object.op_Implicit("[ForceStart] Not enough players."));
				return;
			}
			GameStartManager instance = DestroyableSingleton<GameStartManager>.Instance;
			if (instance != null)
			{
				PassiveButton startButton = instance.StartButton;
				if (startButton != null)
				{
					ButtonClickedEvent onClick = startButton.OnClick;
					if (onClick != null)
					{
						((UnityEvent)onClick).Invoke();
					}
				}
			}
			LogCheat($"Game force-started with {count} players!");
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[ForceStart] Error: {value}"));
		}
	}

	internal static void SetGameFloat(int optionName, float value)
	{
		if (!IntegrityGuard.IsIntact)
		{
			return;
		}
		AmongUsClient instance = AmongUsClient.Instance;
		if (instance == null || !((InnerNetClient)instance).AmHost)
		{
			return;
		}
		try
		{
			GameOptionsManager instance2 = GameOptionsManager.Instance;
			IGameOptions val = ((instance2 != null) ? instance2.CurrentGameOptions : null);
			if (val != null)
			{
				val.SetFloat((FloatOptionNames)optionName, value);
				DebouncedSyncGameSettings();
			}
		}
		catch
		{
		}
	}

	internal static void SetGameInt(int optionName, int value)
	{
		if (!IntegrityGuard.IsIntact)
		{
			return;
		}
		AmongUsClient instance = AmongUsClient.Instance;
		if (instance == null || !((InnerNetClient)instance).AmHost)
		{
			return;
		}
		try
		{
			GameOptionsManager instance2 = GameOptionsManager.Instance;
			IGameOptions val = ((instance2 != null) ? instance2.CurrentGameOptions : null);
			if (val != null)
			{
				val.SetInt((Int32OptionNames)optionName, value);
				DebouncedSyncGameSettings();
			}
		}
		catch
		{
		}
	}

	private static void DebouncedSyncGameSettings()
	{
		_sliderSyncPending = true;
		if (Time.realtimeSinceStartup - _lastSliderSyncTime >= 0.15f)
		{
			FlushSliderSync();
		}
	}

	internal static void FlushSliderSync()
	{
		if (_sliderSyncPending)
		{
			_sliderSyncPending = false;
			_lastSliderSyncTime = Time.realtimeSinceStartup;
			SyncGameSettings();
		}
	}

	internal static void SetGameBool(int optionName, bool value)
	{
		if (!IntegrityGuard.IsIntact)
		{
			return;
		}
		AmongUsClient instance = AmongUsClient.Instance;
		if (instance == null || !((InnerNetClient)instance).AmHost)
		{
			return;
		}
		try
		{
			GameOptionsManager instance2 = GameOptionsManager.Instance;
			IGameOptions val = ((instance2 != null) ? instance2.CurrentGameOptions : null);
			if (val != null)
			{
				val.SetBool((BoolOptionNames)optionName, value);
				SyncGameSettings();
			}
		}
		catch
		{
		}
	}

	internal static bool GetGameBool(int optionName)
	{
		try
		{
			GameOptionsManager instance = GameOptionsManager.Instance;
			IGameOptions val = ((instance != null) ? instance.CurrentGameOptions : null);
			if (val == null)
			{
				return false;
			}
			bool flag = default(bool);
			return val.TryGetBool((BoolOptionNames)optionName, ref flag) && flag;
		}
		catch
		{
			return false;
		}
	}

	internal static float GetGameFloat(int optionName)
	{
		try
		{
			GameOptionsManager instance = GameOptionsManager.Instance;
			IGameOptions val = ((instance != null) ? instance.CurrentGameOptions : null);
			if (val == null)
			{
				return 0f;
			}
			float num = default(float);
			return val.TryGetFloat((FloatOptionNames)optionName, ref num) ? num : 0f;
		}
		catch
		{
			return 0f;
		}
	}

	internal static int GetGameInt(int optionName)
	{
		try
		{
			GameOptionsManager instance = GameOptionsManager.Instance;
			IGameOptions val = ((instance != null) ? instance.CurrentGameOptions : null);
			if (val == null)
			{
				return 0;
			}
			int num = default(int);
			return val.TryGetInt((Int32OptionNames)optionName, ref num) ? num : 0;
		}
		catch
		{
			return 0;
		}
	}

	internal static void SyncGameSettings()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected I4, but got Unknown
		try
		{
			AmongUsClient instance = AmongUsClient.Instance;
			if (instance == null || !((InnerNetClient)instance).AmHost || (Object)(object)PlayerControl.LocalPlayer == (Object)null)
			{
				return;
			}
			GameOptionsManager instance2 = GameOptionsManager.Instance;
			if (instance2 == null)
			{
				return;
			}
			uint num = 0u;
			bool flag = false;
			try
			{
				num = (uint)(int)instance2.GameHostOptions.Keywords;
				flag = true;
			}
			catch
			{
			}
			instance2.GameHostOptions = instance2.CurrentGameOptions;
			if (flag)
			{
				try
				{
					instance2.GameHostOptions.SetUInt((UInt32OptionNames)1, num);
				}
				catch
				{
				}
			}
			GameManager instance3 = GameManager.Instance;
			if (((instance3 != null) ? instance3.LogicOptions : null) != null)
			{
				GameManager.Instance.LogicOptions.SyncOptions();
				return;
			}
			GameOptionsFactory gameOptionsFactory = instance2.gameOptionsFactory;
			if (gameOptionsFactory != null)
			{
				byte[] array = Il2CppArrayBase<byte>.op_Implicit((Il2CppArrayBase<byte>)(object)gameOptionsFactory.ToBytes(instance2.CurrentGameOptions, AprilFoolsMode.IsAprilFoolsModeToggledOn));
				PlayerControl.LocalPlayer.RpcSyncSettings(Il2CppStructArray<byte>.op_Implicit(array));
			}
		}
		catch
		{
		}
	}

	internal static void SaveOriginalGameSettings()
	{
		try
		{
			GameOptionsManager instance = GameOptionsManager.Instance;
			IGameOptions val = ((instance != null) ? instance.CurrentGameOptions : null);
			if (val != null && _gsOriginalFloats == null)
			{
				_gsOriginalFloats = new Dictionary<string, float>();
				_gsOriginalInts = new Dictionary<string, int>();
				_gsOriginalBools = new Dictionary<string, bool>();
				float value = default(float);
				if (val.TryGetFloat((FloatOptionNames)1, ref value))
				{
					_gsOriginalFloats["kc"] = value;
				}
				float value2 = default(float);
				if (val.TryGetFloat((FloatOptionNames)2, ref value2))
				{
					_gsOriginalFloats["sp"] = value2;
				}
				float value3 = default(float);
				if (val.TryGetFloat((FloatOptionNames)4, ref value3))
				{
					_gsOriginalFloats["cv"] = value3;
				}
				float value4 = default(float);
				if (val.TryGetFloat((FloatOptionNames)3, ref value4))
				{
					_gsOriginalFloats["iv"] = value4;
				}
				int value5 = default(int);
				if (val.TryGetInt((Int32OptionNames)3, ref value5))
				{
					_gsOriginalInts["em"] = value5;
				}
				int value6 = default(int);
				if (val.TryGetInt((Int32OptionNames)4, ref value6))
				{
					_gsOriginalInts["ec"] = value6;
				}
				int value7 = default(int);
				if (val.TryGetInt((Int32OptionNames)5, ref value7))
				{
					_gsOriginalInts["dt"] = value7;
				}
				int value8 = default(int);
				if (val.TryGetInt((Int32OptionNames)6, ref value8))
				{
					_gsOriginalInts["vt"] = value8;
				}
				int value9 = default(int);
				if (val.TryGetInt((Int32OptionNames)10, ref value9))
				{
					_gsOriginalInts["ct"] = value9;
				}
				int value10 = default(int);
				if (val.TryGetInt((Int32OptionNames)12, ref value10))
				{
					_gsOriginalInts["lt"] = value10;
				}
				int value11 = default(int);
				if (val.TryGetInt((Int32OptionNames)11, ref value11))
				{
					_gsOriginalInts["st"] = value11;
				}
				bool value12 = default(bool);
				if (val.TryGetBool((BoolOptionNames)3, ref value12))
				{
					_gsOriginalBools["ce"] = value12;
				}
				bool value13 = default(bool);
				if (val.TryGetBool((BoolOptionNames)4, ref value13))
				{
					_gsOriginalBools["av"] = value13;
				}
				bool value14 = default(bool);
				if (val.TryGetBool((BoolOptionNames)1, ref value14))
				{
					_gsOriginalBools["vt2"] = value14;
				}
			}
		}
		catch
		{
		}
	}

	internal static void SyncSlidersFromGameOptions()
	{
		try
		{
			GameOptionsManager instance = GameOptionsManager.Instance;
			IGameOptions val = ((instance != null) ? instance.CurrentGameOptions : null);
			if (val != null)
			{
				ServerData.SecurityConfig config = ServerData.Config;
				int num = default(int);
				if (config.GoImp > 0 && val.TryGetInt((Int32OptionNames)config.GoImp, ref num))
				{
					ServerData.SetSliderValueInternal("h_gs_imp", num);
				}
				float num2 = default(float);
				if (config.GoKcd > 0 && val.TryGetFloat((FloatOptionNames)config.GoKcd, ref num2))
				{
					ServerData.SetSliderValueInternal("h_gs_killcd", (num2 <= 0.001f) ? 0f : num2);
				}
				float value = default(float);
				if (config.GoSpd > 0 && val.TryGetFloat((FloatOptionNames)config.GoSpd, ref value))
				{
					ServerData.SetSliderValueInternal("h_gs_speed", value);
				}
				float value2 = default(float);
				if (config.GoClg > 0 && val.TryGetFloat((FloatOptionNames)config.GoClg, ref value2))
				{
					ServerData.SetSliderValueInternal("h_gs_crewvis", value2);
				}
				float value3 = default(float);
				if (config.GoIlg > 0 && val.TryGetFloat((FloatOptionNames)config.GoIlg, ref value3))
				{
					ServerData.SetSliderValueInternal("h_gs_impvis", value3);
				}
				int num3 = default(int);
				if (config.GoEmg > 0 && val.TryGetInt((Int32OptionNames)config.GoEmg, ref num3))
				{
					ServerData.SetSliderValueInternal("h_gs_emg", num3);
				}
				int num4 = default(int);
				if (config.GoEmgCd > 0 && val.TryGetInt((Int32OptionNames)config.GoEmgCd, ref num4))
				{
					ServerData.SetSliderValueInternal("h_gs_emgcd", num4);
				}
				int num5 = default(int);
				if (config.GoDisc > 0 && val.TryGetInt((Int32OptionNames)config.GoDisc, ref num5))
				{
					ServerData.SetSliderValueInternal("h_gs_disc", num5);
				}
				int num6 = default(int);
				if (config.GoVote > 0 && val.TryGetInt((Int32OptionNames)config.GoVote, ref num6))
				{
					ServerData.SetSliderValueInternal("h_gs_vote", num6);
				}
				int num7 = default(int);
				if (config.GoComm > 0 && val.TryGetInt((Int32OptionNames)config.GoComm, ref num7))
				{
					ServerData.SetSliderValueInternal("h_gs_common", num7);
				}
				int num8 = default(int);
				if (config.GoLng > 0 && val.TryGetInt((Int32OptionNames)config.GoLng, ref num8))
				{
					ServerData.SetSliderValueInternal("h_gs_long", num8);
				}
				int num9 = default(int);
				if (config.GoShrt > 0 && val.TryGetInt((Int32OptionNames)config.GoShrt, ref num9))
				{
					ServerData.SetSliderValueInternal("h_gs_short", num9);
				}
				int num10 = default(int);
				if (config.GoKDist > 0 && val.TryGetInt((Int32OptionNames)config.GoKDist, ref num10))
				{
					ServerData.SetSliderValueInternal("h_gs_killdist", num10);
				}
				int num11 = default(int);
				if (config.GoTbar > 0 && val.TryGetInt((Int32OptionNames)config.GoTbar, ref num11))
				{
					ServerData.SetSliderValueInternal("h_gs_taskbar", num11);
				}
				float value4 = default(float);
				if (config.GoVtm > 0 && val.TryGetFloat((FloatOptionNames)config.GoVtm, ref value4))
				{
					ServerData.SetSliderValueInternal("h_gs_venttime", value4);
				}
				float value5 = default(float);
				if (config.GoGaDur > 0 && val.TryGetFloat((FloatOptionNames)config.GoGaDur, ref value5))
				{
					ServerData.SetSliderValueInternal("h_gs_ga_dur", value5);
				}
				float value6 = default(float);
				if (config.GoGaCd > 0 && val.TryGetFloat((FloatOptionNames)config.GoGaCd, ref value6))
				{
					ServerData.SetSliderValueInternal("h_gs_ga_cd", value6);
				}
				float value7 = default(float);
				if (config.GoEngCd > 0 && val.TryGetFloat((FloatOptionNames)config.GoEngCd, ref value7))
				{
					ServerData.SetSliderValueInternal("h_gs_eng_cd", value7);
				}
				float value8 = default(float);
				if (config.GoEngVt > 0 && val.TryGetFloat((FloatOptionNames)config.GoEngVt, ref value8))
				{
					ServerData.SetSliderValueInternal("h_gs_eng_venttime", value8);
				}
				float value9 = default(float);
				if (config.GoSciCd > 0 && val.TryGetFloat((FloatOptionNames)config.GoSciCd, ref value9))
				{
					ServerData.SetSliderValueInternal("h_gs_sci_cd", value9);
				}
				float value10 = default(float);
				if (config.GoSciBat > 0 && val.TryGetFloat((FloatOptionNames)config.GoSciBat, ref value10))
				{
					ServerData.SetSliderValueInternal("h_gs_sci_bat", value10);
				}
				float value11 = default(float);
				if (config.GoTrkCd > 0 && val.TryGetFloat((FloatOptionNames)config.GoTrkCd, ref value11))
				{
					ServerData.SetSliderValueInternal("h_gs_track_cd", value11);
				}
				float value12 = default(float);
				if (config.GoTrkDur > 0 && val.TryGetFloat((FloatOptionNames)config.GoTrkDur, ref value12))
				{
					ServerData.SetSliderValueInternal("h_gs_track_dur", value12);
				}
				float value13 = default(float);
				if (config.GoTrkDly > 0 && val.TryGetFloat((FloatOptionNames)config.GoTrkDly, ref value13))
				{
					ServerData.SetSliderValueInternal("h_gs_track_dly", value13);
				}
				float value14 = default(float);
				if (config.GoNsDur > 0 && val.TryGetFloat((FloatOptionNames)config.GoNsDur, ref value14))
				{
					ServerData.SetSliderValueInternal("h_gs_noise_dur", value14);
				}
				float value15 = default(float);
				if (config.GoDetLim > 0 && val.TryGetFloat((FloatOptionNames)config.GoDetLim, ref value15))
				{
					ServerData.SetSliderValueInternal("h_gs_detec_limit", value15);
				}
				float value16 = default(float);
				if (config.GoShfCd > 0 && val.TryGetFloat((FloatOptionNames)config.GoShfCd, ref value16))
				{
					ServerData.SetSliderValueInternal("h_gs_shift_cd", value16);
				}
				float value17 = default(float);
				if (config.GoShfDur > 0 && val.TryGetFloat((FloatOptionNames)config.GoShfDur, ref value17))
				{
					ServerData.SetSliderValueInternal("h_gs_shift_dur", value17);
				}
				float value18 = default(float);
				if (config.GoPhCd > 0 && val.TryGetFloat((FloatOptionNames)config.GoPhCd, ref value18))
				{
					ServerData.SetSliderValueInternal("h_gs_phan_cd", value18);
				}
				float value19 = default(float);
				if (config.GoPhDur > 0 && val.TryGetFloat((FloatOptionNames)config.GoPhDur, ref value19))
				{
					ServerData.SetSliderValueInternal("h_gs_phan_dur", value19);
				}
				float value20 = default(float);
				if (config.GoVprDis > 0 && val.TryGetFloat((FloatOptionNames)config.GoVprDis, ref value20))
				{
					ServerData.SetSliderValueInternal("h_gs_viper_dis", value20);
				}
			}
		}
		catch
		{
		}
	}

	internal static void ResetGameSettings()
	{
		if (!IntegrityGuard.IsIntact)
		{
			return;
		}
		AmongUsClient instance = AmongUsClient.Instance;
		if (instance == null || !((InnerNetClient)instance).AmHost)
		{
			return;
		}
		try
		{
			GameOptionsManager instance2 = GameOptionsManager.Instance;
			IGameOptions val = ((instance2 != null) ? instance2.CurrentGameOptions : null);
			if (val == null)
			{
				return;
			}
			if (_gsOriginalFloats != null)
			{
				if (_gsOriginalFloats.TryGetValue("kc", out var value))
				{
					val.SetFloat((FloatOptionNames)1, value);
				}
				if (_gsOriginalFloats.TryGetValue("sp", out var value2))
				{
					val.SetFloat((FloatOptionNames)2, value2);
				}
				if (_gsOriginalFloats.TryGetValue("cv", out var value3))
				{
					val.SetFloat((FloatOptionNames)4, value3);
				}
				if (_gsOriginalFloats.TryGetValue("iv", out var value4))
				{
					val.SetFloat((FloatOptionNames)3, value4);
				}
				if (_gsOriginalInts.TryGetValue("em", out var value5))
				{
					val.SetInt((Int32OptionNames)3, value5);
				}
				if (_gsOriginalInts.TryGetValue("ec", out var value6))
				{
					val.SetInt((Int32OptionNames)4, value6);
				}
				if (_gsOriginalInts.TryGetValue("dt", out var value7))
				{
					val.SetInt((Int32OptionNames)5, value7);
				}
				if (_gsOriginalInts.TryGetValue("vt", out var value8))
				{
					val.SetInt((Int32OptionNames)6, value8);
				}
				if (_gsOriginalInts.TryGetValue("ct", out var value9))
				{
					val.SetInt((Int32OptionNames)10, value9);
				}
				if (_gsOriginalInts.TryGetValue("lt", out var value10))
				{
					val.SetInt((Int32OptionNames)12, value10);
				}
				if (_gsOriginalInts.TryGetValue("st", out var value11))
				{
					val.SetInt((Int32OptionNames)11, value11);
				}
				if (_gsOriginalBools.TryGetValue("ce", out var value12))
				{
					val.SetBool((BoolOptionNames)3, value12);
				}
				if (_gsOriginalBools.TryGetValue("av", out var value13))
				{
					val.SetBool((BoolOptionNames)4, value13);
				}
				if (_gsOriginalBools.TryGetValue("vt2", out var value14))
				{
					val.SetBool((BoolOptionNames)1, value14);
				}
			}
			SyncGameSettings();
			SyncSlidersFromGameOptions();
			_gsOriginalFloats = null;
			_gsOriginalInts = null;
			_gsOriginalBools = null;
		}
		catch
		{
		}
	}

	internal static void SaveSettingsPreset(int slot)
	{
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Expected O, but got Unknown
		bool @bool = default(bool);
		try
		{
			GameOptionsManager instance = GameOptionsManager.Instance;
			IGameOptions val = ((instance != null) ? instance.CurrentGameOptions : null);
			if (val == null)
			{
				return;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				dictionary["kc"] = val.GetFloat((FloatOptionNames)1).ToString(CultureInfo.InvariantCulture);
			}
			catch
			{
			}
			try
			{
				dictionary["sp"] = val.GetFloat((FloatOptionNames)2).ToString(CultureInfo.InvariantCulture);
			}
			catch
			{
			}
			try
			{
				dictionary["cv"] = val.GetFloat((FloatOptionNames)4).ToString(CultureInfo.InvariantCulture);
			}
			catch
			{
			}
			try
			{
				dictionary["iv"] = val.GetFloat((FloatOptionNames)3).ToString(CultureInfo.InvariantCulture);
			}
			catch
			{
			}
			try
			{
				dictionary["em"] = val.GetInt((Int32OptionNames)3).ToString();
			}
			catch
			{
			}
			try
			{
				dictionary["ec"] = val.GetInt((Int32OptionNames)4).ToString();
			}
			catch
			{
			}
			try
			{
				dictionary["dt"] = val.GetInt((Int32OptionNames)5).ToString();
			}
			catch
			{
			}
			try
			{
				dictionary["vt"] = val.GetInt((Int32OptionNames)6).ToString();
			}
			catch
			{
			}
			try
			{
				dictionary["ct"] = val.GetInt((Int32OptionNames)10).ToString();
			}
			catch
			{
			}
			try
			{
				dictionary["lt"] = val.GetInt((Int32OptionNames)12).ToString();
			}
			catch
			{
			}
			try
			{
				dictionary["st"] = val.GetInt((Int32OptionNames)11).ToString();
			}
			catch
			{
			}
			try
			{
				@bool = val.GetBool((BoolOptionNames)3);
				dictionary["ce"] = @bool.ToString();
			}
			catch
			{
			}
			try
			{
				@bool = val.GetBool((BoolOptionNames)4);
				dictionary["av"] = @bool.ToString();
			}
			catch
			{
			}
			try
			{
				@bool = val.GetBool((BoolOptionNames)1);
				dictionary["vt2"] = @bool.ToString();
			}
			catch
			{
			}
			string text = Path.Combine(Paths.ConfigPath, "ModMenuCrew");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string path = Path.Combine(text, $"preset_{slot}.txt");
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				list.Add(item.Key + "=" + item.Value);
			}
			File.WriteAllLines(path, list.ToArray());
			NotifyUtils.Info($"Preset {slot} saved");
		}
		catch (Exception ex)
		{
			ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(22, 2, ref @bool);
			if (@bool)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("SaveSettingsPreset(");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<int>(slot);
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("): ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(ex.Message);
			}
			log.LogError(val2);
		}
	}

	internal static void LoadSettingsPreset(int slot)
	{
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Expected O, but got Unknown
		try
		{
			AmongUsClient instance = AmongUsClient.Instance;
			if (instance == null || !((InnerNetClient)instance).AmHost)
			{
				return;
			}
			string path = Path.Combine(Paths.ConfigPath, "ModMenuCrew", $"preset_{slot}.txt");
			if (!File.Exists(path))
			{
				NotifyUtils.Warning($"Preset {slot} not found");
				return;
			}
			GameOptionsManager instance2 = GameOptionsManager.Instance;
			IGameOptions val = ((instance2 != null) ? instance2.CurrentGameOptions : null);
			if (val == null)
			{
				return;
			}
			SaveOriginalGameSettings();
			string[] array = File.ReadAllLines(path);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('=');
				if (array2.Length != 2)
				{
					continue;
				}
				string text = array2[0].Trim();
				string text2 = array2[1].Trim();
				try
				{
					if (text == null)
					{
						continue;
					}
					switch (text.Length)
					{
					case 2:
						switch (text[0])
						{
						case 'k':
							if (text == "kc")
							{
								val.SetFloat((FloatOptionNames)1, float.Parse(text2, CultureInfo.InvariantCulture));
							}
							break;
						case 's':
							if (!(text == "sp"))
							{
								if (text == "st")
								{
									val.SetInt((Int32OptionNames)11, int.Parse(text2));
								}
							}
							else
							{
								val.SetFloat((FloatOptionNames)2, float.Parse(text2, CultureInfo.InvariantCulture));
							}
							break;
						case 'c':
							switch (text)
							{
							case "cv":
								val.SetFloat((FloatOptionNames)4, float.Parse(text2, CultureInfo.InvariantCulture));
								break;
							case "ct":
								val.SetInt((Int32OptionNames)10, int.Parse(text2));
								break;
							case "ce":
								val.SetBool((BoolOptionNames)3, bool.Parse(text2));
								break;
							}
							break;
						case 'i':
							if (text == "iv")
							{
								val.SetFloat((FloatOptionNames)3, float.Parse(text2, CultureInfo.InvariantCulture));
							}
							break;
						case 'e':
							if (!(text == "em"))
							{
								if (text == "ec")
								{
									val.SetInt((Int32OptionNames)4, int.Parse(text2));
								}
							}
							else
							{
								val.SetInt((Int32OptionNames)3, int.Parse(text2));
							}
							break;
						case 'd':
							if (text == "dt")
							{
								val.SetInt((Int32OptionNames)5, int.Parse(text2));
							}
							break;
						case 'v':
							if (text == "vt")
							{
								val.SetInt((Int32OptionNames)6, int.Parse(text2));
							}
							break;
						case 'l':
							if (text == "lt")
							{
								val.SetInt((Int32OptionNames)12, int.Parse(text2));
							}
							break;
						case 'a':
							if (text == "av")
							{
								val.SetBool((BoolOptionNames)4, bool.Parse(text2));
							}
							break;
						}
						break;
					case 3:
						if (text == "vt2")
						{
							val.SetBool((BoolOptionNames)1, bool.Parse(text2));
						}
						break;
					}
				}
				catch
				{
				}
			}
			SyncGameSettings();
			SyncSlidersFromGameOptions();
			NotifyUtils.Success($"Preset {slot} loaded");
		}
		catch (Exception ex)
		{
			ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			bool flag = default(bool);
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(22, 2, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("LoadSettingsPreset(");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<int>(slot);
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("): ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(ex.Message);
			}
			log.LogError(val2);
		}
	}

	internal static void InstantStartGame()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		if (!IntegrityGuard.IsIntact || Time.time - _lastInstantStartTime < 3f || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost || (int)((InnerNetClient)AmongUsClient.Instance).GameState == 2)
		{
			return;
		}
		try
		{
			if (_dummyActive || _cfActive)
			{
				NotifyUtils.Error("Remove LAB entities first (Ghost Twins / Shadow Clones)");
				return;
			}
			_lastInstantStartTime = Time.time;
			try
			{
				if (_mapActive)
				{
					TestSpawnMap();
				}
			}
			catch
			{
			}
			try
			{
				if (_selMapSpawnId != -1)
				{
					_DespawnSelMap(((InnerNetClient)AmongUsClient.Instance).ClientId);
					MapCheats.SpawnLobby();
				}
			}
			catch
			{
			}
			AmongUsClient.Instance.KickNotJoinedPlayers();
			HudManager instance = DestroyableSingleton<HudManager>.Instance;
			if ((Object)(object)((instance != null) ? instance.GameMenu : null) != (Object)null && DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen)
			{
				DestroyableSingleton<HudManager>.Instance.GameMenu.Close();
			}
			AmongUsClient.Instance.StartGame();
			GameStartManager instance2 = DestroyableSingleton<GameStartManager>.Instance;
			if ((Object)(object)instance2 != (Object)null)
			{
				AmongUsClient.Instance.DisconnectHandlers.Remove(((Il2CppObjectBase)instance2).Cast<IDisconnectHandler>());
				Object.Destroy((Object)(object)((Component)instance2).gameObject);
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[InstantStart] Error: {value}"));
		}
	}

	internal static void KickPlayer(byte playerId, bool ban = false)
	{
		if (!IntegrityGuard.IsIntact)
		{
			return;
		}
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[Kick] Only the host can kick players."));
			return;
		}
		try
		{
			PlayerControl val = null;
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current != (Object)null && current.PlayerId == playerId)
				{
					val = current;
					break;
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				Debug.LogWarning(Object.op_Implicit($"[Kick] Player {playerId} not found."));
				return;
			}
			if ((Object)(object)val == (Object)(object)PlayerControl.LocalPlayer)
			{
				Debug.LogWarning(Object.op_Implicit("[Kick] You cannot kick yourself."));
				return;
			}
			int ownerId = ((InnerNetObject)val).OwnerId;
			string text = null;
			NetworkedPlayerInfo data = val.Data;
			string text2 = ((data != null) ? data.PlayerName : null);
			if (ban)
			{
				try
				{
					Enumerator<ClientData> enumerator2 = ((InnerNetClient)AmongUsClient.Instance).allClients.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						ClientData current2 = enumerator2.Current;
						if (current2 != null && current2.Id == ownerId)
						{
							text = current2.FriendCode;
							break;
						}
					}
				}
				catch
				{
				}
			}
			((InnerNetClient)AmongUsClient.Instance).KickPlayer(ownerId, ban);
			if (ban && !string.IsNullOrEmpty(text))
			{
				try
				{
					CrewBanListService.Add(text, text2, "UI ban");
				}
				catch
				{
				}
			}
			LogCheat($"Player {text2 ?? playerId.ToString()} was {(ban ? "banned" : "kicked")}!");
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[Kick] Error: {value}"));
		}
	}

	internal static void KickPlayerByName(string playerName, bool ban = false)
	{
		PlayerControl val = null;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if (current != null)
			{
				NetworkedPlayerInfo data = current.Data;
				if (((data == null) ? null : data.PlayerName?.Equals(playerName, StringComparison.OrdinalIgnoreCase)).GetValueOrDefault())
				{
					val = current;
					break;
				}
			}
		}
		if ((Object)(object)val != (Object)null)
		{
			KickPlayer(val.PlayerId, ban);
		}
		else
		{
			Debug.LogWarning(Object.op_Implicit("[Kick] Player '" + playerName + "' not found."));
		}
	}

	internal static void KickPlayer(PlayerControl player, bool ban = false)
	{
		if (!((Object)(object)player == (Object)null))
		{
			KickPlayer(player.PlayerId, ban);
		}
	}

	internal static void FlushKickNotifications()
	{
		if (_pendingKickNotify != null && !(Time.realtimeSinceStartup - _pendingKickNotifyTime < 2.5f))
		{
			NotifyUtils.Warning(_pendingKickNotify);
			_pendingKickNotify = null;
			_pendingKickCount = 0;
		}
	}

	private static bool IsAutoKickImmune(byte playerId)
	{
		try
		{
			GameData instance = GameData.Instance;
			NetworkedPlayerInfo obj = ((instance != null) ? instance.GetPlayerById(playerId) : null);
			string text = ((obj != null) ? obj.FriendCode : null);
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			return CrewModeratorService.IsModerator(text) || CrewVipService.IsVip(text);
		}
		catch
		{
			return false;
		}
	}

	internal static void ScheduleAutoKickCheck()
	{
		if (!CheatConfig.AutoKickByLevelEnabled.Value && !CheatConfig.AutoKickByNameEnabled.Value)
		{
			return;
		}
		_lastAutoKickTime = Mathf.Min(_lastAutoKickTime, Time.time - 2f);
		if (!CheatConfig.AutoKickByLevelEnabled.Value)
		{
			return;
		}
		try
		{
			List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
			if (allPlayerControls == null)
			{
				return;
			}
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			byte b = ((localPlayer != null) ? localPlayer.PlayerId : byte.MaxValue);
			for (int i = 0; i < allPlayerControls.Count; i++)
			{
				PlayerControl val = allPlayerControls[i];
				if ((Object)(object)val != (Object)null && val.PlayerId != b && !_pendingLevelCheck.ContainsKey(val.PlayerId))
				{
					_pendingLevelCheck[val.PlayerId] = Time.time;
				}
			}
		}
		catch
		{
		}
	}

	internal static void RunAutoKick()
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost || (!CheatConfig.AutoKickByLevelEnabled.Value && !CheatConfig.AutoKickByNameEnabled.Value) || !Object.op_Implicit((Object)(object)LobbyBehaviour.Instance))
		{
			return;
		}
		if (_pendingLevelCheck.Count > 0 && CheatConfig.AutoKickByLevelEnabled.Value)
		{
			int value = CheatConfig.AutoKickMinLevel.Value;
			List<byte> list = new List<byte>();
			foreach (KeyValuePair<byte, float> item in _pendingLevelCheck)
			{
				if (Time.time - item.Value < 2.5f)
				{
					continue;
				}
				list.Add(item.Key);
				try
				{
					if (IsAutoKickImmune(item.Key))
					{
						continue;
					}
					GameData instance = GameData.Instance;
					NetworkedPlayerInfo val = ((instance != null) ? instance.GetPlayerById(item.Key) : null);
					if ((Object)(object)val == (Object)null)
					{
						continue;
					}
					uint playerLevel = val.PlayerLevel;
					if (playerLevel == uint.MaxValue || string.IsNullOrEmpty(val.PlayerName))
					{
						continue;
					}
					uint num = playerLevel + 1;
					if (num >= (uint)value)
					{
						continue;
					}
					PlayerControl val2 = null;
					List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
					if (allPlayerControls != null)
					{
						for (int i = 0; i < allPlayerControls.Count; i++)
						{
							if ((Object)(object)allPlayerControls[i] != (Object)null && allPlayerControls[i].PlayerId == item.Key)
							{
								val2 = allPlayerControls[i];
								break;
							}
						}
					}
					if (!((Object)(object)val2 == (Object)null) && !((InnerNetObject)val2).AmOwner)
					{
						ClientData client = ((InnerNetClient)AmongUsClient.Instance).GetClient(((InnerNetObject)val2).OwnerId);
						if (client != null && !_autoKickRecentIds.Contains(client.Id))
						{
							_autoKickRecentIds.Add(client.Id);
							((InnerNetClient)AmongUsClient.Instance).KickPlayer(client.Id, false);
							string value2 = client.PlayerName ?? "???";
							LogCheat($"[AutoKick] Kicked {value2} (Lv{num} < Min {value})");
							_pendingKickCount++;
							_pendingKickNotifyTime = Time.realtimeSinceStartup;
							_pendingKickNotify = ((_pendingKickCount == 1) ? $"Auto-kicked {value2} (Lv{num} < Min {value})" : $"Auto-kicked {_pendingKickCount} players");
						}
					}
				}
				catch
				{
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				_pendingLevelCheck.Remove(list[j]);
			}
		}
		if (Time.time - _lastAutoKickTime < 3f)
		{
			return;
		}
		_lastAutoKickTime = Time.time;
		if (Time.time - _autoKickRecentCleanTime > 30f)
		{
			_autoKickRecentCleanTime = Time.time;
			_autoKickRecentIds.Clear();
		}
		bool value3 = CheatConfig.AutoKickByLevelEnabled.Value;
		int value4 = CheatConfig.AutoKickMinLevel.Value;
		bool value5 = CheatConfig.AutoKickByNameEnabled.Value;
		if (value5)
		{
			string text = CheatConfig.AutoKickNameList.Value ?? "";
			if (!string.Equals(text, _cachedNameBlacklistRaw, StringComparison.Ordinal))
			{
				_cachedNameBlacklistRaw = text;
				if (string.IsNullOrEmpty(text))
				{
					_cachedNameBlacklist = null;
				}
				else
				{
					_cachedNameBlacklist = text.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					for (int k = 0; k < _cachedNameBlacklist.Length; k++)
					{
						_cachedNameBlacklist[k] = _cachedNameBlacklist[k].Trim();
					}
				}
			}
		}
		List<PlayerControl> allPlayerControls2 = PlayerControl.AllPlayerControls;
		if (allPlayerControls2 == null)
		{
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		byte b = ((localPlayer != null) ? localPlayer.PlayerId : byte.MaxValue);
		for (int l = 0; l < allPlayerControls2.Count; l++)
		{
			PlayerControl val3 = allPlayerControls2[l];
			if ((Object)(object)val3 == (Object)null || val3.PlayerId == b || IsAutoKickImmune(val3.PlayerId))
			{
				continue;
			}
			ClientData client2 = ((InnerNetClient)AmongUsClient.Instance).GetClient(((InnerNetObject)val3).OwnerId);
			if (client2 == null || _autoKickRecentIds.Contains(client2.Id))
			{
				continue;
			}
			string text2 = null;
			if (value3)
			{
				uint num2 = uint.MaxValue;
				string value6 = null;
				try
				{
					GameData instance2 = GameData.Instance;
					NetworkedPlayerInfo val4 = ((instance2 != null) ? instance2.GetPlayerById(val3.PlayerId) : null);
					if ((Object)(object)val4 != (Object)null)
					{
						num2 = val4.PlayerLevel;
						value6 = val4.PlayerName;
					}
				}
				catch
				{
				}
				if (num2 != uint.MaxValue && !string.IsNullOrEmpty(value6))
				{
					uint num3 = num2 + 1;
					if (num3 < (uint)value4)
					{
						text2 = $"Lv{num3} < Min {value4}";
					}
				}
			}
			if (text2 == null && value5 && _cachedNameBlacklist != null)
			{
				NetworkedPlayerInfo data = val3.Data;
				string text3 = ((data != null) ? data.PlayerName : null) ?? client2.PlayerName;
				if (!string.IsNullOrEmpty(text3))
				{
					for (int m = 0; m < _cachedNameBlacklist.Length; m++)
					{
						if (_cachedNameBlacklist[m].Length > 0 && string.Equals(text3, _cachedNameBlacklist[m], StringComparison.OrdinalIgnoreCase))
						{
							text2 = "Blacklisted name";
							break;
						}
					}
				}
			}
			if (text2 != null)
			{
				_autoKickRecentIds.Add(client2.Id);
				((InnerNetClient)AmongUsClient.Instance).KickPlayer(client2.Id, false);
				string value7 = client2.PlayerName ?? "???";
				LogCheat($"[AutoKick] Kicked {value7} ({text2})");
				_pendingKickCount++;
				_pendingKickNotifyTime = Time.realtimeSinceStartup;
				_pendingKickNotify = ((_pendingKickCount == 1) ? $"Auto-kicked {value7} ({text2})" : $"Auto-kicked {_pendingKickCount} players");
			}
		}
	}

	internal static void ClearAutoKickState()
	{
		_autoKickRecentIds.Clear();
		_autoKickRecentCleanTime = Time.time;
		_cachedNameBlacklist = null;
		_cachedNameBlacklistRaw = null;
		_pendingLevelCheck.Clear();
	}

	internal static void RunDisableVent()
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		if (!DisableGameVent || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost || !((InnerNetClient)AmongUsClient.Instance).IsGameStarted || Time.time - _lastVentCheckTime < 0.25f)
		{
			return;
		}
		_lastVentCheckTime = Time.time;
		try
		{
			List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
			if (allPlayerControls == null)
			{
				return;
			}
			for (int i = 0; i < allPlayerControls.Count; i++)
			{
				PlayerControl val = allPlayerControls[i];
				if ((Object)(object)val == (Object)null || (Object)(object)val.Data == (Object)null || val.Data.IsDead || ((InnerNetObject)val).AmOwner || !val.inVent)
				{
					continue;
				}
				int num = 0;
				try
				{
					ShipStatus instance = ShipStatus.Instance;
					if (((instance != null) ? instance.AllVents : null) != null)
					{
						Il2CppReferenceArray<Vent> allVents = ShipStatus.Instance.AllVents;
						float num2 = float.MaxValue;
						for (int j = 0; j < ((Il2CppArrayBase<Vent>)(object)allVents).Length; j++)
						{
							float num3 = Vector2.Distance(Vector2.op_Implicit(((Component)val).transform.position), Vector2.op_Implicit(((Component)((Il2CppArrayBase<Vent>)(object)allVents)[j]).transform.position));
							if (num3 < num2)
							{
								num2 = num3;
								num = ((Il2CppArrayBase<Vent>)(object)allVents)[j].Id;
							}
						}
					}
				}
				catch
				{
				}
				try
				{
					val.MyPhysics.RpcBootFromVent(num);
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	internal static void ProtectPlayer(byte playerId, float duration = 10f)
	{
		if (!((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[ProtectPlayer] Only the HOST can protect players."));
			return;
		}
		if (!((Object)(object)ShipStatus.Instance != (Object)null) || Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			Debug.LogWarning(Object.op_Implicit("[ProtectPlayer] Game must be in progress (not in lobby or meeting)."));
			return;
		}
		PlayerControl val = null;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && current.PlayerId == playerId)
			{
				val = current;
				break;
			}
		}
		if ((Object)(object)val == (Object)null)
		{
			NotifyUtils.Error("Player not found.");
			return;
		}
		NetworkedPlayerInfo data = val.Data;
		if (data != null && data.IsDead)
		{
			Debug.LogWarning(Object.op_Implicit("[ProtectPlayer] Cannot protect dead players."));
			return;
		}
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			int? obj;
			if (localPlayer == null)
			{
				obj = null;
			}
			else
			{
				CosmeticsLayer cosmetics = localPlayer.cosmetics;
				obj = ((cosmetics != null) ? new int?(cosmetics.ColorId) : null);
			}
			int? num = obj;
			int valueOrDefault = num.GetValueOrDefault();
			PlayerControl.LocalPlayer.RpcProtectPlayer(val, valueOrDefault);
			_protectedPlayerId = playerId;
			_protectionStartTime = Time.time;
			_protectionDuration = duration;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 2);
			defaultInterpolatedStringHandler.AppendLiteral(" PROTECTION ON: ");
			NetworkedPlayerInfo data2 = val.Data;
			defaultInterpolatedStringHandler.AppendFormatted(((data2 != null) ? data2.PlayerName : null) ?? playerId.ToString());
			defaultInterpolatedStringHandler.AppendLiteral(" is protected for ");
			defaultInterpolatedStringHandler.AppendFormatted((duration > 0f) ? $"{duration}s" : "infinite");
			defaultInterpolatedStringHandler.AppendLiteral("!");
			LogCheat(defaultInterpolatedStringHandler.ToStringAndClear());
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[ProtectPlayer] Error: {value}"));
		}
	}

	internal static void ProtectPlayerByName(string playerName, float duration = 10f)
	{
		PlayerControl val = null;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if (current != null)
			{
				NetworkedPlayerInfo data = current.Data;
				if (((data == null) ? null : data.PlayerName?.Equals(playerName, StringComparison.OrdinalIgnoreCase)).GetValueOrDefault())
				{
					val = current;
					break;
				}
			}
		}
		if ((Object)(object)val != (Object)null)
		{
			ProtectPlayer(val.PlayerId, duration);
		}
		else
		{
			Debug.LogWarning(Object.op_Implicit("[ProtectPlayer] Player '" + playerName + "' not found."));
		}
	}

	internal static void RemoveProtection()
	{
		if (!_protectedPlayerId.HasValue)
		{
			Debug.Log(Object.op_Implicit("[ProtectPlayer] No player is currently protected."));
			return;
		}
		PlayerControl val = null;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && current.PlayerId == _protectedPlayerId.Value)
			{
				val = current;
				break;
			}
		}
		if ((Object)(object)val != (Object)null)
		{
			val.protectedByGuardianId = -1;
			NetworkedPlayerInfo data = val.Data;
			LogCheat("\ud83d\udee1\ufe0f PROTECTION REMOVED from " + ((data != null) ? data.PlayerName : null));
		}
		_protectedPlayerId = null;
	}

	internal static void UpdateProtection()
	{
		if (_protectedPlayerId.HasValue && !(_protectionDuration <= 0f) && Time.time - _protectionStartTime >= _protectionDuration)
		{
			RemoveProtection();
		}
	}

	internal static void ProtectSelf(float duration = 10f)
	{
		if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
		{
			ProtectPlayer(PlayerControl.LocalPlayer.PlayerId, duration);
		}
	}

	internal static PlayerControl GetProtectedPlayer()
	{
		if (!_protectedPlayerId.HasValue)
		{
			return null;
		}
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && current.PlayerId == _protectedPlayerId.Value)
			{
				return current;
			}
		}
		return null;
	}

	internal static List<(byte PlayerId, string Name, bool IsImpostor)> GetAlivePlayersForProtection()
	{
		_alivePlayersBuffer.Clear();
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if (!((Object)(object)current == (Object)null) && !((Object)(object)current.Data == (Object)null) && !current.Data.IsDead)
			{
				List<(byte PlayerId, string Name, bool IsImpostor)> alivePlayersBuffer = _alivePlayersBuffer;
				byte playerId = current.PlayerId;
				string item = current.Data.PlayerName ?? $"Player {current.PlayerId}";
				RoleBehaviour role = current.Data.Role;
				alivePlayersBuffer.Add((playerId, item, role != null && role.IsImpostor));
			}
		}
		return _alivePlayersBuffer;
	}

	internal static List<(byte PlayerId, string Name, bool IsImpostor, bool IsDead)> GetAllPlayersForImpersonate()
	{
		_allPlayersBuffer.Clear();
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if (!((Object)(object)current == (Object)null) && !((Object)(object)current.Data == (Object)null) && !current.Data.Disconnected && (!((Object)(object)localPlayer != (Object)null) || current.PlayerId != localPlayer.PlayerId))
			{
				List<(byte PlayerId, string Name, bool IsImpostor, bool IsDead)> allPlayersBuffer = _allPlayersBuffer;
				byte playerId = current.PlayerId;
				string item = current.Data.PlayerName ?? $"Player {current.PlayerId}";
				RoleBehaviour role = current.Data.Role;
				allPlayersBuffer.Add((playerId, item, role != null && role.IsImpostor, current.Data.IsDead));
			}
		}
		return _allPlayersBuffer;
	}

	private static float _ComputeReprotectInterval()
	{
		float num = 10f;
		try
		{
			GameOptionsManager instance = GameOptionsManager.Instance;
			IGameOptions val = ((instance != null) ? instance.CurrentGameOptions : null);
			if (val != null)
			{
				num = val.GetFloat((FloatOptionNames)1100);
			}
		}
		catch
		{
		}
		return Mathf.Clamp(num / 5f, 0.5f, 6f);
	}

	internal static void UpdateGodMode()
	{
		if (CheatConfig.GodMode && !((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost && !((Object)(object)ShipStatus.Instance == (Object)null) && !((Object)(object)MeetingHud.Instance != (Object)null))
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if (!((Object)(object)localPlayer == (Object)null) && !((Object)(object)localPlayer.Data == (Object)null) && !localPlayer.Data.IsDead && Time.time - _lastGodModeProtectionTime >= _ComputeReprotectInterval())
			{
				_lastGodModeProtectionTime = Time.time;
				ApplyGodModeProtection(localPlayer);
			}
		}
	}

	private static void ApplyGodModeProtection(PlayerControl target)
	{
		try
		{
			bool num = target.protectedByGuardianId >= 0;
			CosmeticsLayer cosmetics = target.cosmetics;
			int num2 = ((cosmetics != null) ? cosmetics.ColorId : 0);
			PlayerControl.LocalPlayer.RpcProtectPlayer(target, num2);
			if (!num)
			{
				NetworkedPlayerInfo data = target.Data;
				LogCheat("\ud83d\udd31 GOD MODE: Protection applied to " + ((data != null) ? data.PlayerName : null) + "!");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[GodMode] Error applying protection: " + ex.Message));
		}
	}

	internal static void UpdateGodModeAll()
	{
		if (!CheatConfig.GodModeAll || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost || (Object)(object)ShipStatus.Instance == (Object)null || (Object)(object)MeetingHud.Instance != (Object)null)
		{
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null || (Object)(object)localPlayer.Data == (Object)null || Time.time - _lastGodModeAllTime < _ComputeReprotectInterval())
		{
			return;
		}
		_lastGodModeAllTime = Time.time;
		try
		{
			List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
			if (allPlayerControls == null)
			{
				return;
			}
			int count = allPlayerControls.Count;
			for (int i = 0; i < count; i++)
			{
				PlayerControl val = allPlayerControls[i];
				if (!((Object)(object)val == (Object)null) && !((Object)(object)val.Data == (Object)null) && !val.Data.IsDead && !val.Data.Disconnected)
				{
					CosmeticsLayer cosmetics = val.cosmetics;
					int num = ((cosmetics != null) ? cosmetics.ColorId : 0);
					localPlayer.RpcProtectPlayer(val, num);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[GodModeAll] Error: " + ex.Message));
		}
	}

	internal static void HostSetPlayerColor(PlayerControl target, int colorId)
	{
		if (!IntegrityGuard.IsIntact)
		{
			return;
		}
		if (!ModKeyValidator.IsPremium || !ModKeyValidator.V())
		{
			NotifyUtils.Warning("★ Set Player Color is PREMIUM ONLY!");
			return;
		}
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[HostSetColor] Only the HOST can use this cheat."));
			return;
		}
		if ((Object)(object)((target != null) ? target.Data : null) == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("[HostSetColor] Invalid player."));
			return;
		}
		try
		{
			byte b = (byte)Math.Max(0, Math.Min(colorId, ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length - 1));
			target.RpcSetColor(b);
			LogCheat($"\ud83c\udfa8 Cor de {target.Data.PlayerName} mudada para {b} (sync broadcast)");
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[HostSetColor] Erro: {value}"));
		}
	}

	internal static void HostSetAllPlayersColor(int colorId)
	{
		if (!IntegrityGuard.IsIntact)
		{
			return;
		}
		if (!ModKeyValidator.IsPremium || !ModKeyValidator.V())
		{
			NotifyUtils.Warning("★ Set All Colors is PREMIUM ONLY!");
			return;
		}
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[HostSetAllColors] Only the HOST can use this."));
			return;
		}
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)((current != null) ? current.Data : null) != (Object)null && !current.Data.Disconnected)
			{
				EnqueueColorRpc(current.PlayerId, colorId);
			}
		}
		LogCheat($"\ud83c\udfa8 All players now have color {colorId} (staggered)!");
	}

	internal static void HostSetPlayerColorInternal(PlayerControl target, int colorId)
	{
		if ((Object)(object)((target != null) ? target.Data : null) == (Object)null)
		{
			return;
		}
		try
		{
			byte b = (byte)Math.Max(0, Math.Min(colorId, ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length - 1));
			target.RpcSetColor(b);
		}
		catch
		{
		}
	}

	private static void EnqueueColorRpc(byte playerId, int colorId)
	{
		_colorBurstQ.Enqueue(new KeyValuePair<byte, int>(playerId, colorId));
	}

	internal static void TickColorBurstQueue()
	{
		if (_colorBurstQ.Count == 0)
		{
			return;
		}
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost || !((InnerNetClient)AmongUsClient.Instance).AmConnected)
		{
			_colorBurstQ.Clear();
			return;
		}
		float num = HostOutboundBudget.Utilization();
		float num2 = ((num >= 0.85f) ? 2f : ((num >= 0.6f) ? 1f : ((LabRpcArbiter.ActiveCount() <= 0) ? 0.35f : 1f)));
		float time = Time.time;
		if (time - _colorBurstLastSend < num2 || !HostOutboundBudget.CanSend(1))
		{
			return;
		}
		_colorBurstLastSend = time;
		KeyValuePair<byte, int> keyValuePair = _colorBurstQ.Dequeue();
		PlayerControl val = null;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && current.PlayerId == keyValuePair.Key)
			{
				val = current;
				break;
			}
		}
		if ((Object)(object)((val != null) ? val.Data : null) != (Object)null && !val.Data.Disconnected)
		{
			HostSetPlayerColorInternal(val, keyValuePair.Value);
			HostOutboundBudget.Record(1);
		}
	}

	internal static void ToggleRainbow()
	{
		if (!IntegrityGuard.IsIntact)
		{
			_rainbowEnabled = false;
			return;
		}
		if (!ModKeyValidator.IsPremium || !ModKeyValidator.V())
		{
			_rainbowEnabled = false;
			return;
		}
		_rainbowEnabled = !_rainbowEnabled;
		_rainbowLastApply = 0f;
		_rainbowPlayerIdx = 0;
		if (!_rainbowEnabled)
		{
			_rainbowPhase = 0;
		}
	}

	internal static void ResetRainbow()
	{
		_rainbowEnabled = false;
		_rainbowPhase = 0;
		_rainbowLastApply = 0f;
		_rainbowPlayerIdx = 0;
		_playerRainbowPhases.Clear();
		_playerRainbowLastApply = 0f;
		_colorBurstQ.Clear();
		_colorBurstLastSend = 0f;
	}

	internal static bool IsPlayerRainbowActive(byte playerId)
	{
		return _playerRainbowPhases.ContainsKey(playerId);
	}

	internal static bool StartPlayerRainbow(byte playerId)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			return false;
		}
		if (_playerRainbowPhases.ContainsKey(playerId))
		{
			return false;
		}
		_playerRainbowPhases[playerId] = 0;
		return true;
	}

	internal static bool StopPlayerRainbow(byte playerId)
	{
		return _playerRainbowPhases.Remove(playerId);
	}

	internal static int StopAllPlayerRainbows()
	{
		int count = _playerRainbowPhases.Count;
		_playerRainbowPhases.Clear();
		return count;
	}

	internal static void TickPlayerRainbows()
	{
		if (_playerRainbowPhases.Count == 0 || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			return;
		}
		float num = ((LabRpcArbiter.ActiveCount() > 0) ? 4f : 2f);
		float time = Time.time;
		if (time - _playerRainbowLastApply < num)
		{
			return;
		}
		_playerRainbowLastApply = time;
		int length = ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length;
		if (length <= 0)
		{
			return;
		}
		List<byte> list = null;
		foreach (byte item in new List<byte>(_playerRainbowPhases.Keys))
		{
			PlayerControl val = null;
			Enumerator<PlayerControl> enumerator2 = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				PlayerControl current2 = enumerator2.Current;
				if ((Object)(object)current2 != (Object)null && current2.PlayerId == item)
				{
					val = current2;
					break;
				}
			}
			if ((Object)(object)val == (Object)null || (Object)(object)val.Data == (Object)null || val.Data.Disconnected)
			{
				if (list == null)
				{
					list = new List<byte>();
				}
				list.Add(item);
			}
			else
			{
				int num2 = _playerRainbowPhases[item];
				int colorId = num2 % length;
				EnqueueColorRpc(item, colorId);
				_playerRainbowPhases[item] = (num2 + 1) % length;
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (byte item2 in list)
		{
			_playerRainbowPhases.Remove(item2);
		}
	}

	internal static void TickRainbow()
	{
		if (!_rainbowEnabled)
		{
			return;
		}
		if (!IntegrityGuard.IsIntact)
		{
			_rainbowEnabled = false;
		}
		else if (!ModKeyValidator.IsPremium || !ModKeyValidator.V())
		{
			_rainbowEnabled = false;
		}
		else
		{
			if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				return;
			}
			float num = ((LabRpcArbiter.ActiveCount() > 0) ? 1f : 0.35f);
			float time = Time.time;
			if (time - _rainbowLastApply < num)
			{
				return;
			}
			_rainbowLastApply = time;
			int length = ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length;
			if (length <= 0)
			{
				return;
			}
			List<PlayerControl> list = new List<PlayerControl>(16);
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)((current != null) ? current.Data : null) != (Object)null && !current.Data.Disconnected)
				{
					list.Add(current);
				}
			}
			if (list.Count != 0)
			{
				if (_rainbowPlayerIdx >= list.Count)
				{
					_rainbowPlayerIdx = 0;
				}
				int rainbowPlayerIdx = _rainbowPlayerIdx;
				int colorId = (rainbowPlayerIdx + _rainbowPhase) % length;
				EnqueueColorRpc(list[rainbowPlayerIdx].PlayerId, colorId);
				_rainbowPlayerIdx++;
				if (_rainbowPlayerIdx >= list.Count)
				{
					_rainbowPlayerIdx = 0;
					_rainbowPhase = (_rainbowPhase + 1) % length;
				}
			}
		}
	}

	internal static void HostRandomizeAllColors()
	{
		if (!IntegrityGuard.IsIntact)
		{
			return;
		}
		if (!ModKeyValidator.IsPremium || !ModKeyValidator.V())
		{
			NotifyUtils.Warning("★ Randomize Colors is PREMIUM ONLY!");
			return;
		}
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[HostRandomizeColors] Only the HOST can use this."));
			return;
		}
		int length = ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)((current != null) ? current.Data : null) != (Object)null && !current.Data.Disconnected)
			{
				EnqueueColorRpc(current.PlayerId, random.Next(0, length));
			}
		}
		LogCheat("\ud83c\udfb2 Cores randomizadas (staggered)!");
	}

	internal static void HostSetPlayerName(PlayerControl target, string newName)
	{
		Debug.LogWarning(Object.op_Implicit("[HostSetName] DISABLED - server validates names!"));
		LogCheat("⚠\ufe0f Set Name disabled - server reverts!");
	}

	internal static void HostSetAllNamesHacked()
	{
		Debug.LogWarning(Object.op_Implicit("[HostSetAllNames] DISABLED - server validates names!"));
		LogCheat("⚠\ufe0f Set All Names disabled - server reverts!");
	}

	internal static void HostSetPlayerHat(PlayerControl target, string hatId)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[HostSetHat] Only the HOST can use this."));
		}
		else if (!((Object)(object)((target != null) ? target.Data : null) == (Object)null))
		{
			try
			{
				target.RpcSetHat(hatId);
				LogCheat($"\ud83c\udfa9 Hat de {target.Data.PlayerName} mudado para {hatId} (sync broadcast)");
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[HostSetHat] Erro: {value}"));
			}
		}
	}

	internal static void HostSetPlayerSkin(PlayerControl target, string skinId)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[HostSetSkin] Only the HOST can use this."));
		}
		else if (!((Object)(object)((target != null) ? target.Data : null) == (Object)null))
		{
			try
			{
				target.RpcSetSkin(skinId);
				LogCheat($"\ud83d\udc55 Skin de {target.Data.PlayerName} mudada para {skinId} (sync broadcast)");
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[HostSetSkin] Erro: {value}"));
			}
		}
	}

	internal static void HostSetPlayerPet(PlayerControl target, string petId)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[HostSetPet] Only the HOST can use this."));
		}
		else if (!((Object)(object)((target != null) ? target.Data : null) == (Object)null))
		{
			try
			{
				target.RpcSetPet(petId);
				LogCheat($"\ud83d\udc3e Pet de {target.Data.PlayerName} mudado para {petId} (sync broadcast)");
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[HostSetPet] Erro: {value}"));
			}
		}
	}

	internal static void HostSetPlayerVisor(PlayerControl target, string visorId)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[HostSetVisor] Only the HOST can use this."));
		}
		else if (!((Object)(object)((target != null) ? target.Data : null) == (Object)null))
		{
			try
			{
				target.RpcSetVisor(visorId);
				LogCheat($"\ud83d\udc53 Visor de {target.Data.PlayerName} mudado para {visorId} (sync broadcast)");
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[HostSetVisor] Erro: {value}"));
			}
		}
	}

	internal static void HostSetPlayerNameplate(PlayerControl target, string nameplateId)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.LogWarning(Object.op_Implicit("[HostSetNameplate] Only the HOST can use this."));
		}
		else if (!((Object)(object)((target != null) ? target.Data : null) == (Object)null))
		{
			try
			{
				target.Data.DefaultOutfit.NamePlateId = nameplateId;
				PlayerOutfit defaultOutfit = target.Data.DefaultOutfit;
				byte namePlateSequenceId = defaultOutfit.NamePlateSequenceId;
				defaultOutfit.NamePlateSequenceId = (byte)(namePlateSequenceId + 1);
				target.Data.MarkDirty();
				LogCheat("\ud83c\udff7\ufe0f Nameplate de " + target.Data.PlayerName + " mudado para " + nameplateId);
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[HostSetNameplate] Erro: {value}"));
			}
		}
	}

	internal static float GetImpRemaining()
	{
		if (!_isImpersonating || !(_impDuration > 0f))
		{
			return 0f;
		}
		return Mathf.Max(0f, _impTimer - Time.time);
	}

	internal static void UpdateImpersonate()
	{
		if (_isImpersonating && _impDuration > 0f && Time.time >= _impTimer)
		{
			HostRestoreOutfit();
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
	}

	internal static void SetImpDuration(float d)
	{
		_impDuration = d;
	}

	internal static void HostImpersonatePlayer(byte sourcePlayerId)
	{
		if (!IntegrityGuard.IsIntact || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			return;
		}
		PlayerControl val = null;
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)((localPlayer != null) ? localPlayer.Data : null) == (Object)null)
		{
			return;
		}
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && current.PlayerId == sourcePlayerId)
			{
				val = current;
				break;
			}
		}
		if ((Object)(object)((val != null) ? val.Data : null) == (Object)null || val.PlayerId == localPlayer.PlayerId)
		{
			return;
		}
		try
		{
			if (!_isImpersonating)
			{
				PlayerOutfit defaultOutfit = localPlayer.Data.DefaultOutfit;
				_savedColorId = (byte)defaultOutfit.ColorId;
				_savedHatId = defaultOutfit.HatId ?? "";
				_savedSkinId = defaultOutfit.SkinId ?? "";
				_savedPetId = defaultOutfit.PetId ?? "";
				_savedVisorId = defaultOutfit.VisorId ?? "";
				_savedNamePlateId = defaultOutfit.NamePlateId ?? "";
			}
			PlayerOutfit defaultOutfit2 = val.Data.DefaultOutfit;
			byte b = (byte)Math.Max(0, Math.Min(defaultOutfit2.ColorId, ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length - 1));
			localPlayer.RpcSetColor(b);
			localPlayer.RpcSetHat(defaultOutfit2.HatId);
			localPlayer.RpcSetSkin(defaultOutfit2.SkinId);
			localPlayer.RpcSetPet(defaultOutfit2.PetId);
			localPlayer.RpcSetVisor(defaultOutfit2.VisorId);
			localPlayer.Data.DefaultOutfit.NamePlateId = defaultOutfit2.NamePlateId;
			PlayerOutfit defaultOutfit3 = localPlayer.Data.DefaultOutfit;
			byte namePlateSequenceId = defaultOutfit3.NamePlateSequenceId;
			defaultOutfit3.NamePlateSequenceId = (byte)(namePlateSequenceId + 1);
			localPlayer.Data.MarkDirty();
			_isImpersonating = true;
			_impTimer = ((_impDuration > 0f) ? (Time.time + _impDuration) : 0f);
			LogCheat($"Impersonating {val.Data.PlayerName} (color={b}, hat={defaultOutfit2.HatId})");
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[HostImpersonate] Error: {value}"));
		}
	}

	internal static void HostRestoreOutfit()
	{
		if (!_isImpersonating || (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)((localPlayer != null) ? localPlayer.Data : null) == (Object)null)
		{
			return;
		}
		try
		{
			localPlayer.RpcSetColor(_savedColorId);
			localPlayer.RpcSetHat(_savedHatId);
			localPlayer.RpcSetSkin(_savedSkinId);
			localPlayer.RpcSetPet(_savedPetId);
			localPlayer.RpcSetVisor(_savedVisorId);
			localPlayer.Data.DefaultOutfit.NamePlateId = _savedNamePlateId;
			PlayerOutfit defaultOutfit = localPlayer.Data.DefaultOutfit;
			byte namePlateSequenceId = defaultOutfit.NamePlateSequenceId;
			defaultOutfit.NamePlateSequenceId = (byte)(namePlateSequenceId + 1);
			localPlayer.Data.MarkDirty();
			_isImpersonating = false;
			LogCheat("Outfit restored");
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[HostRestoreOutfit] Error: {value}"));
		}
	}

	internal static void HostStripPlayerCosmetics(PlayerControl target)
	{
		Debug.LogWarning(Object.op_Implicit("[HostStripCosmetics] DISABLED - causes kick on online server!"));
		LogCheat("⚠\ufe0f Strip Cosmetics disabled - causes kick!");
	}

	internal static void HostStripAllCosmetics()
	{
		Debug.LogWarning(Object.op_Implicit("[HostStripAllCosmetics] DISABLED - causes kick on online server!"));
		LogCheat("⚠\ufe0f Strip All Cosmetics disabled - causes kick!");
	}

	internal static List<(int Id, string Name)> GetAvailableColors()
	{
		List<(int, string)> list = new List<(int, string)>();
		string[] array = new string[18]
		{
			"Red", "Blue", "Green", "Pink", "Orange", "Yellow", "Black", "White", "Purple", "Brown",
			"Cyan", "Lime", "Maroon", "Rose", "Banana", "Gray", "Tan", "Coral"
		};
		for (int i = 0; i < Math.Min(((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length, array.Length); i++)
		{
			list.Add((i, array[i]));
		}
		return list;
	}

	internal static void ToggleFreeCam(bool enable)
	{
		FreeCamEnabled = enable;
		LogCheat(enable ? "FreeCam ATIVADO - WASD Mover, Q/E Zoom" : "FreeCam DESATIVADO");
	}

	internal static void UpdateFreeCam()
	{
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Camera.main == (Object)null)
		{
			return;
		}
		FollowerCamera component = ((Component)Camera.main).gameObject.GetComponent<FollowerCamera>();
		if (FreeCamEnabled)
		{
			bool flag = false;
			try
			{
				flag = (Object)(object)MeetingHud.Instance != (Object)null || (Object)(object)ExileController.Instance != (Object)null;
			}
			catch
			{
			}
			if (flag)
			{
				if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
				{
					PlayerControl.LocalPlayer.moveable = true;
				}
				return;
			}
			if (!freecamActive)
			{
				if ((Object)(object)component != (Object)null)
				{
					((Behaviour)component).enabled = false;
					component.Target = null;
				}
				freecamActive = true;
				Debug.Log(Object.op_Implicit("[FreeCam] Ativado"));
			}
			HudManager instance = DestroyableSingleton<HudManager>.Instance;
			if ((Object)(object)((instance != null) ? instance.ShadowQuad : null) != (Object)null && ((Component)DestroyableSingleton<HudManager>.Instance.ShadowQuad).gameObject.activeSelf)
			{
				((Component)DestroyableSingleton<HudManager>.Instance.ShadowQuad).gameObject.SetActive(false);
			}
			bool flag2 = false;
			try
			{
				HudManager instance2 = DestroyableSingleton<HudManager>.Instance;
				flag2 = (Object)(object)((instance2 != null) ? instance2.Chat : null) != (Object)null && DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening;
			}
			catch
			{
			}
			if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
			{
				PlayerControl.LocalPlayer.moveable = flag2;
			}
			if (!flag2)
			{
				Vector3 val = default(Vector3);
				(val)._002Ector(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);
				float num = freeCamSpeed;
				if (Input.GetKey((KeyCode)304))
				{
					num *= 2.5f;
				}
				if (Input.GetKey((KeyCode)306))
				{
					num *= 0.3f;
				}
				((Component)Camera.main).transform.position = ((Component)Camera.main).transform.position + val * num * Time.deltaTime;
				if (Input.GetKey((KeyCode)113))
				{
					Camera main = Camera.main;
					main.orthographicSize += 5f * Time.deltaTime;
				}
				if (Input.GetKey((KeyCode)101))
				{
					Camera main2 = Camera.main;
					main2.orthographicSize -= 5f * Time.deltaTime;
				}
				float y = Input.mouseScrollDelta.y;
				if (y != 0f)
				{
					Camera main3 = Camera.main;
					main3.orthographicSize -= y * 2f;
				}
				Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1f, 30f);
			}
		}
		else
		{
			if (!freecamActive)
			{
				return;
			}
			if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
			{
				PlayerControl.LocalPlayer.moveable = true;
			}
			if ((Object)(object)component != (Object)null)
			{
				((Behaviour)component).enabled = true;
				if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
				{
					component.SetTarget((MonoBehaviour)(object)PlayerControl.LocalPlayer);
				}
			}
			if ((Object)(object)Camera.main != (Object)null)
			{
				Camera.main.orthographicSize = 3f;
			}
			HudManager instance3 = DestroyableSingleton<HudManager>.Instance;
			if ((Object)(object)((instance3 != null) ? instance3.UICamera : null) != (Object)null)
			{
				DestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = 3f;
			}
			HudManager instance4 = DestroyableSingleton<HudManager>.Instance;
			if ((Object)(object)((instance4 != null) ? instance4.ShadowQuad : null) != (Object)null)
			{
				((Component)DestroyableSingleton<HudManager>.Instance.ShadowQuad).gameObject.SetActive(true);
			}
			freecamActive = false;
			Debug.Log(Object.op_Implicit("[FreeCam] Disabled - camera, movement and vision restored"));
		}
	}

	internal static void UpdateSatelliteScroll()
	{
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Camera.main == (Object)null)
		{
			return;
		}
		ConfigEntry<bool> lockScrollZoom = CheatConfig.LockScrollZoom;
		if ((lockScrollZoom != null && lockScrollZoom.Value) || FreeCamEnabled || ModMenuCrewPlugin.DebuggerComponent.IsMenuOpen || !IsOnActiveMap())
		{
			return;
		}
		try
		{
			if ((Object)(object)Minigame.Instance != (Object)null)
			{
				return;
			}
		}
		catch
		{
		}
		try
		{
			if ((Object)(object)MeetingHud.Instance != (Object)null)
			{
				return;
			}
		}
		catch
		{
		}
		try
		{
			if ((Object)(object)ExileController.Instance != (Object)null)
			{
				return;
			}
		}
		catch
		{
		}
		try
		{
			HudManager instance = DestroyableSingleton<HudManager>.Instance;
			if ((Object)(object)instance == (Object)null || ((Object)(object)instance.Chat != (Object)null && instance.Chat.IsOpenOrOpening))
			{
				return;
			}
		}
		catch
		{
		}
		try
		{
			if ((Object)(object)PlayerCustomizationMenu.Instance != (Object)null)
			{
				return;
			}
		}
		catch
		{
		}
		try
		{
			if ((Object)(object)MapBehaviour.Instance != (Object)null && MapBehaviour.Instance.IsOpen)
			{
				return;
			}
		}
		catch
		{
		}
		if (Input.GetMouseButtonDown(2))
		{
			CheatConfig.VisionMultiplier = 3f;
			ServerData.SetSliderValueInternal("cheat_vision", 3f);
			if ((Object)(object)Camera.main != (Object)null && Camera.main.orthographicSize > 4f)
			{
				Camera.main.orthographicSize = 3f;
			}
			try
			{
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				return;
			}
			catch
			{
				return;
			}
		}
		float y = Input.mouseScrollDelta.y;
		if (y == 0f)
		{
			return;
		}
		float visionMultiplier = CheatConfig.VisionMultiplier;
		float num = visionMultiplier - y * 0.5f;
		num = Mathf.Clamp(num, 3f, 15f);
		if (num <= 3.01f)
		{
			num = 3f;
		}
		if (Mathf.Abs(num - visionMultiplier) < 0.01f)
		{
			return;
		}
		CheatConfig.VisionMultiplier = num;
		ServerData.SetSliderValueInternal("cheat_vision", num);
		if (num <= 3.01f && (Object)(object)Camera.main != (Object)null && Camera.main.orthographicSize > 4f)
		{
			Camera.main.orthographicSize = 3f;
		}
		try
		{
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
		catch
		{
		}
	}

	private static void AdjustResolution()
	{
		try
		{
			ResolutionChangedHandler resolutionChanged = ResolutionManager.ResolutionChanged;
			if (resolutionChanged != null)
			{
				resolutionChanged.Invoke((float)Screen.width / (float)Screen.height, Screen.width, Screen.height, Screen.fullScreen);
			}
		}
		catch
		{
		}
	}

	internal static void ToggleTracers(bool enable)
	{
		TracersEnabled = enable;
		if (!enable)
		{
			CleanupTracers();
		}
		LogCheat(enable ? "Tracers ENABLED" : "Tracers DISABLED");
	}

	private static void CleanupTracers()
	{
		foreach (KeyValuePair<byte, LineRenderer> tracerLine in _tracerLines)
		{
			if ((Object)(object)tracerLine.Value != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)tracerLine.Value).gameObject);
			}
		}
		_tracerLines.Clear();
		if ((Object)(object)_tracerMaterial != (Object)null)
		{
			Object.Destroy((Object)(object)_tracerMaterial);
		}
		_tracerMaterial = null;
	}

	internal static void UpdateTracers()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (Object)(object)ShipStatus.Instance != (Object)null || (Object)(object)LobbyBehaviour.Instance != (Object)null;
		if (!TracersEnabled || !flag || (Object)(object)PlayerControl.LocalPlayer == (Object)null)
		{
			if (_tracerLines.Count > 0)
			{
				CleanupTracers();
			}
		}
		else
		{
			if (Time.time - _lastTracerUpdateTime < 0.033f)
			{
				return;
			}
			_lastTracerUpdateTime = Time.time;
			if ((Object)(object)_tracerMaterial == (Object)null)
			{
				_tracerMaterial = new Material(Shader.Find("Sprites/Default"));
			}
			Vector3 position = ((Component)PlayerControl.LocalPlayer).transform.position;
			position.z = -2f;
			_tracerActiveIdsBuffer.Clear();
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current == (Object)null || (Object)(object)current == (Object)(object)PlayerControl.LocalPlayer || (Object)(object)current.Data == (Object)null || current.Data.IsDead)
				{
					continue;
				}
				byte playerId = current.PlayerId;
				_tracerActiveIdsBuffer.Add(playerId);
				Vector3 position2 = ((Component)current).transform.position;
				if (Vector2.Distance(new Vector2(position.x, position.y), new Vector2(position2.x, position2.y)) < 0.3f)
				{
					if (_tracerLines.TryGetValue(playerId, out var value) && (Object)(object)value != (Object)null)
					{
						((Renderer)value).enabled = false;
					}
					continue;
				}
				position2.z = -2f;
				CosmeticsLayer cosmetics = current.cosmetics;
				int num = ((cosmetics != null) ? cosmetics.ColorId : 0);
				num = Mathf.Clamp(num, 0, ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length - 1);
				RoleBehaviour role = current.Data.Role;
				bool num2 = role != null && role.IsImpostor;
				Color val = Color32.op_Implicit(((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors)[num]);
				float num3;
				float num4;
				float num5;
				float num6;
				if (num2)
				{
					num3 = 0.92f;
					num4 = 0.07f;
					num5 = 0.85f;
					num6 = 0.7f;
				}
				else
				{
					num3 = 0.75f;
					num4 = 0.045f;
					num5 = 0.45f;
					num6 = 0.2f;
				}
				Color startColor = val;
				startColor.a = num3;
				Color endColor = val;
				endColor.a = num3 * num6;
				if (!_tracerLines.TryGetValue(playerId, out var value2) || (Object)(object)value2 == (Object)null)
				{
					GameObject val2 = new GameObject("MMC_Tracer");
					val2.transform.SetParent(((Component)current).transform);
					value2 = val2.AddComponent<LineRenderer>();
					value2.positionCount = 2;
					((Renderer)value2).sharedMaterial = _tracerMaterial;
					value2.numCapVertices = 3;
					value2.useWorldSpace = true;
					((Renderer)value2).sortingOrder = 5;
					((Renderer)value2).receiveShadows = false;
					((Renderer)value2).shadowCastingMode = (ShadowCastingMode)0;
					_tracerLines[playerId] = value2;
				}
				((Renderer)value2).enabled = true;
				value2.SetPosition(0, position);
				value2.SetPosition(1, position2);
				value2.startWidth = num4;
				value2.endWidth = num4 * num5;
				value2.startColor = startColor;
				value2.endColor = endColor;
			}
			_tracerRemoveBuffer.Clear();
			foreach (byte key in _tracerLines.Keys)
			{
				if (!_tracerActiveIdsBuffer.Contains(key))
				{
					_tracerRemoveBuffer.Add(key);
				}
			}
			foreach (byte item in _tracerRemoveBuffer)
			{
				if (_tracerLines.TryGetValue(item, out var value3) && (Object)(object)value3 != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)value3).gameObject);
				}
				_tracerLines.Remove(item);
			}
		}
	}

	internal static void ToggleNoClipSmooth(bool enable)
	{
		NoClipSmoothEnabled = enable;
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)((localPlayer != null) ? localPlayer.Collider : null) != (Object)null)
		{
			((Behaviour)PlayerControl.LocalPlayer.Collider).enabled = !enable;
		}
		LogCheat(enable ? "NoClip Smooth ATIVADO - Use WASD para atravessar paredes" : "NoClip Smooth DESATIVADO");
	}

	internal static void UpdateNoClipSmooth()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		if (!NoClipSmoothEnabled || (Object)(object)PlayerControl.LocalPlayer == (Object)null)
		{
			return;
		}
		try
		{
			if ((Object)(object)MeetingHud.Instance != (Object)null)
			{
				return;
			}
		}
		catch
		{
		}
		if ((Object)(object)PlayerControl.LocalPlayer.Collider != (Object)null && ((Behaviour)PlayerControl.LocalPlayer.Collider).enabled)
		{
			((Behaviour)PlayerControl.LocalPlayer.Collider).enabled = false;
		}
		Vector2 zero = Vector2.zero;
		if (Input.GetKey((KeyCode)119))
		{
			zero.y += 1f;
		}
		if (Input.GetKey((KeyCode)115))
		{
			zero.y -= 1f;
		}
		if (Input.GetKey((KeyCode)97))
		{
			zero.x -= 1f;
		}
		if (Input.GetKey((KeyCode)100))
		{
			zero.x += 1f;
		}
		if (!(zero == Vector2.zero))
		{
			float num = (Input.GetKey((KeyCode)304) ? (_noClipSpeed * 2f) : _noClipSpeed);
			Vector2 val = Vector2.op_Implicit(((Component)PlayerControl.LocalPlayer).transform.position) + (zero).normalized * num * Time.deltaTime;
			((Component)PlayerControl.LocalPlayer).transform.position = new Vector3(val.x, val.y, ((Component)PlayerControl.LocalPlayer).transform.position.z);
			PlayerControl.LocalPlayer.NetTransform.SnapTo(val);
		}
	}

	private static void DestroyTex(ref Texture2D tex)
	{
		if ((Object)(object)tex != (Object)null && (Object)(object)tex != (Object)(object)Texture2D.whiteTexture)
		{
			Object.Destroy((Object)(object)tex);
			tex = null;
		}
	}

	public static void CleanupRadarTextures()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			RadarEnabled = false;
			_radarInitialized = false;
			_mapTextureLoaded = false;
			_sonarInitialized = false;
			foreach (int item in new List<int>(_mapTextures.Keys))
			{
				Texture2D tex = _mapTextures[item];
				DestroyTex(ref tex);
				_mapTextures[item] = null;
			}
			_mapTextures.Clear();
			DestroyTex(ref _airshipMapTexture);
			DestroyTex(ref _texSonarLine);
			DestroyTex(ref _texWindowBg);
			DestroyTex(ref _texHeaderBg);
			DestroyTex(ref _texMapBg);
			DestroyTex(ref _texBorderAccent);
			DestroyTex(ref _texBorderDim);
			DestroyTex(ref _texMapOverlay);
			DestroyTex(ref _texImpostorGlow);
			DestroyTex(ref _texImpostorDot);
			DestroyTex(ref _texGhostDot);
			DestroyTex(ref _texDeadBodyBg);
			DestroyTex(ref _texVisor);
			DestroyTex(ref _texShadow);
			DestroyTex(ref _texVent);
			DestroyTex(ref _texTracerLine);
			DestroyTex(ref _texGridDim);
			DestroyTex(ref _texGridBright);
			DestroyTex(ref _texVentStyle);
			DestroyTex(ref _texScanLine);
			DestroyTex(ref _texRoomFill);
			DestroyTex(ref _texRoomBorder);
			DestroyTex(ref _texCorridorFill);
			if (_texPlayers != null)
			{
				for (int i = 0; i < _texPlayers.Length; i++)
				{
					DestroyTex(ref _texPlayers[i]);
				}
			}
			if (_sButton != null)
			{
				GUIStyleState normal = _sButton.normal;
				if ((Object)(object)((normal != null) ? normal.background : null) != (Object)null)
				{
					Object.Destroy((Object)(object)_sButton.normal.background);
					_sButton.normal.background = null;
				}
				GUIStyleState hover = _sButton.hover;
				if ((Object)(object)((hover != null) ? hover.background : null) != (Object)null)
				{
					Object.Destroy((Object)(object)_sButton.hover.background);
					_sButton.hover.background = null;
				}
			}
			DestroyTex(ref _killAlertBgTex);
			DestroyTex(ref _killAlertAccentTex);
			DestroyTex(ref _killAlertShadowTex);
			_killAlertBgStyle = (_killAlertAccentStyle = (_killAlertShadowStyle = null));
			_killAlertTitleStyle = (_killAlertDetailStyle = null);
			DestroyTex(ref _cooldownBgTex);
			_cooldownAboveHeadStyle = null;
			_cooldownShadowStyle = null;
			DestroyTex(ref _eventLogBgTex);
			DestroyTex(ref _eventLogHeaderTex);
			_eventLogHeaderStyle = null;
			_eventLogEventStyle = null;
			_eventLogTimeStyle = null;
			_sWindowBg = (_sHeaderBg = (_sMapBg = null));
			_sBorderAccent = (_sBorderDim = (_sMapOverlay = null));
			_sImpostorGlow = (_sImpostorDot = (_sGhostDot = (_sDeadBodyBg = null)));
			_sVisor = (_sShadow = null);
			_sGridDim = (_sGridBright = (_sVentStyle = (_sScanLine = null)));
			_sRoomFill = (_sRoomBorder = (_sCorridorFill = null));
			_sLabel = (_sTitle = (_sSmall = (_sButton = null)));
			_cachedMapStyle = null;
			if (_sPlayers != null)
			{
				for (int j = 0; j < _sPlayers.Length; j++)
				{
					_sPlayers[j] = null;
				}
			}
			foreach (KeyValuePair<int, Texture2D> item2 in _texCache)
			{
				if ((Object)(object)item2.Value != (Object)null && (Object)(object)item2.Value != (Object)(object)Texture2D.whiteTexture)
				{
					Object.Destroy((Object)(object)item2.Value);
				}
			}
			_texCache.Clear();
			_playerCacheCount = 0;
			_deadBodyCacheCount = 0;
			try
			{
				GuiStyles.ClearCache();
			}
			catch
			{
			}
			try
			{
				GhostUI.ClearCachedTextures();
			}
			catch
			{
			}
			Debug.Log(Object.op_Implicit("[RADAR] Full cleanup — textures + styles + texCache + GuiStyles destroyed."));
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[RADAR] Error during cleanup: " + ex.Message));
		}
	}

	private static bool TryGetMapInfo(out MapInfo info)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (_isAirship)
		{
			info = AirshipMapInfo;
			return true;
		}
		return MapInfos.TryGetValue(_currentMapType, out info);
	}

	private static void LoadAirshipTexture()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		try
		{
			if (!string.IsNullOrEmpty(RadarMapData.AirshipMapBase64))
			{
				byte[] array = Convert.FromBase64String(RadarMapData.AirshipMapBase64);
				Texture2D val = new Texture2D(2, 2, (TextureFormat)4, false);
				((Texture)val).filterMode = (FilterMode)1;
				if (ImageConversion.LoadImage(val, Il2CppStructArray<byte>.op_Implicit(array)))
				{
					((Object)val).hideFlags = (HideFlags)61;
					_airshipMapTexture = val;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[RADAR] Airship texture error: " + ex.Message));
		}
	}

	internal static void ToggleRadar(bool enable)
	{
		RadarEnabled = enable;
		LogCheat(enable ? "RADAR ATIVADO!" : "Radar desativado");
	}

	internal static void SetRadarScale(float scale)
	{
		SetRadarSize(Mathf.Lerp(200f, 500f, Mathf.InverseLerp(0.03f, 0.15f, scale)));
		LogCheat($"Radar scale: {scale:F2}");
	}

	internal static void SetRadarSize(float size)
	{
		size = Mathf.Clamp(size, 200f, 500f);
		(_radarRect).width = size;
		(_radarRect).height = size;
	}

	private static Texture2D MakeTex(Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		int key = ((int)(c.r * 255f) << 24) | ((int)(c.g * 255f) << 16) | ((int)(c.b * 255f) << 8) | (int)(c.a * 255f);
		if (_texCache.TryGetValue(key, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		Texture2D val = new Texture2D(1, 1);
		val.SetPixel(0, 0, c);
		val.Apply();
		((Object)val).hideFlags = (HideFlags)61;
		_texCache[key] = val;
		return val;
	}

	private static GUIStyle MakeStyle(Texture2D tex)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		GUIStyle val = new GUIStyle();
		val.normal.background = tex;
		return val;
	}

	private static Texture2D MakeGradientV(int h, Color top, Color bot)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(1, h);
		for (int i = 0; i < h; i++)
		{
			val.SetPixel(0, i, Color.Lerp(top, bot, (float)i / (float)(h - 1)));
		}
		val.Apply();
		((Texture)val).wrapMode = (TextureWrapMode)1;
		((Object)val).hideFlags = (HideFlags)61;
		return val;
	}

	private static Texture2D MakeFrameTex(int w, int h, Color inner, Color border, int bw)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(w, h);
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				val.SetPixel(j, i, (j < bw || j >= w - bw || i < bw || i >= h - bw) ? border : inner);
			}
		}
		val.Apply();
		((Object)val).hideFlags = (HideFlags)61;
		return val;
	}

	private static void InitRadar()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Expected O, but got Unknown
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Expected O, but got Unknown
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Expected O, but got Unknown
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Expected O, but got Unknown
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_053e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Expected O, but got Unknown
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_058d: Unknown result type (might be due to invalid IL or missing references)
		if (_radarInitialized)
		{
			return;
		}
		LoadMapTexture();
		_texWindowBg = MakeTex(GuiStyles.Theme.BgDarkA);
		_texHeaderBg = MakeGradientV(24, GuiStyles.Theme.HeaderTop, GuiStyles.Theme.HeaderBottom);
		_texMapBg = MakeTex(ThemeMapBg);
		_texBorderAccent = MakeTex(GuiStyles.Theme.Accent);
		_texBorderDim = MakeTex(GuiStyles.Theme.AccentDim);
		_texMapOverlay = MakeTex(ThemeMapOverlay);
		_texImpostorGlow = MakeTex(ThemeImpostorGlow);
		_texImpostorDot = MakeTex(GuiStyles.Theme.Error);
		_texGhostDot = MakeTex(new Color(0.6f, 0.6f, 0.7f, 0.35f));
		_texDeadBodyBg = MakeTex(new Color(0.6f, 0.08f, 0.12f, 0.85f));
		_texVisor = MakeTex(ThemeVisor);
		_texShadow = MakeTex(new Color(0.01f, 0.01f, 0.02f, 0.92f));
		_texPlayers = (Texture2D[])(object)new Texture2D[18];
		for (int i = 0; i < 18; i++)
		{
			_texPlayers[i] = MakeTex(PlayerColors[i]);
		}
		_sWindowBg = MakeStyle(_texWindowBg);
		_sHeaderBg = MakeStyle(_texHeaderBg);
		_sMapBg = MakeStyle(_texMapBg);
		_sBorderAccent = MakeStyle(_texBorderAccent);
		_sBorderDim = MakeStyle(_texBorderDim);
		_sMapOverlay = MakeStyle(_texMapOverlay);
		_sImpostorGlow = MakeStyle(_texImpostorGlow);
		_sImpostorDot = MakeStyle(_texImpostorDot);
		_sGhostDot = MakeStyle(_texGhostDot);
		_sDeadBodyBg = MakeStyle(_texDeadBodyBg);
		_sVisor = MakeStyle(_texVisor);
		_sShadow = MakeStyle(_texShadow);
		_sPlayers = (GUIStyle[])(object)new GUIStyle[18];
		for (int j = 0; j < 18; j++)
		{
			_sPlayers[j] = MakeStyle(_texPlayers[j]);
		}
		_sLabel = new GUIStyle(GUI.skin.label)
		{
			alignment = (TextAnchor)4,
			fontStyle = (FontStyle)1,
			fontSize = 10
		};
		_sLabel.normal.textColor = GuiStyles.Theme.TextPrimary;
		_sTitle = new GUIStyle(_sLabel)
		{
			fontSize = 12,
			fontStyle = (FontStyle)1
		};
		_sTitle.normal.textColor = GuiStyles.Theme.Accent;
		_sSmall = new GUIStyle(_sLabel)
		{
			fontSize = 9,
			fontStyle = (FontStyle)0
		};
		_sSmall.normal.textColor = GuiStyles.Theme.TextMuted;
		_sButton = new GUIStyle(GUI.skin.button)
		{
			fontSize = 11,
			alignment = (TextAnchor)4,
			fixedWidth = 20f,
			fixedHeight = 18f
		};
		_sButton.normal.background = MakeFrameTex(20, 18, new Color(0.06f, 0.06f, 0.08f, 0.98f), GuiStyles.Theme.AccentDim, 1);
		_sButton.hover.background = MakeFrameTex(20, 18, new Color(0.12f, 0.03f, 0.06f, 0.98f), GuiStyles.Theme.Accent, 1);
		_sButton.normal.textColor = GuiStyles.Theme.TextPrimary;
		_sButton.hover.textColor = GuiStyles.Theme.Accent;
		_texVent = MakeTex(new Color(0.2f, 0.8f, 0.6f, 0.9f));
		_texTracerLine = MakeTex(new Color(1f, 0.09f, 0.27f, 0.5f));
		_texGridDim = MakeTex(new Color(1f, 0.09f, 0.27f, 0.08f));
		_texGridBright = MakeTex(new Color(1f, 0.09f, 0.27f, 0.2f));
		_texVentStyle = MakeTex(new Color(0.2f, 0.8f, 0.6f, 0.9f));
		_texScanLine = MakeTex(new Color(1f, 0.09f, 0.27f, 0.18f));
		_sGridDim = MakeStyle(_texGridDim);
		_sGridBright = MakeStyle(_texGridBright);
		_sVentStyle = MakeStyle(_texVentStyle);
		_sScanLine = MakeStyle(_texScanLine);
		_texRoomFill = MakeTex(new Color(0.08f, 0.09f, 0.125f, 0.75f));
		_texRoomBorder = MakeTex(new Color(0.25f, 0.28f, 0.35f, 0.85f));
		_texCorridorFill = MakeTex(new Color(0.06f, 0.07f, 0.1f, 0.6f));
		_sRoomFill = MakeStyle(_texRoomFill);
		_sRoomBorder = MakeStyle(_texRoomBorder);
		_sCorridorFill = MakeStyle(_texCorridorFill);
		if ((Object)(object)_texSonarLine == (Object)null)
		{
			_texSonarLine = new Texture2D(2, 64, (TextureFormat)4, false);
			Color val = default(Color);
			for (int k = 0; k < 64; k++)
			{
				float num = 1f - (float)k / 64f;
				(val)._002Ector(0.2f, 1f, 0.4f, num * 0.6f);
				_texSonarLine.SetPixel(0, k, val);
				_texSonarLine.SetPixel(1, k, val);
			}
			_texSonarLine.Apply();
			((Texture)_texSonarLine).wrapMode = (TextureWrapMode)1;
			((Object)_texSonarLine).hideFlags = (HideFlags)61;
		}
		_sonarInitialized = true;
		_radarInitialized = true;
	}

	private static void LoadMapTexture()
	{
		if (!_mapTextureLoaded)
		{
			try
			{
				LoadAndStoreMapTexture(0, RadarMapData.SkeldMapBase64);
				LoadAndStoreMapTexture(1, RadarMapData.MiraHqMapBase64);
				LoadAndStoreMapTexture(2, RadarMapData.PolusMapBase64);
				LoadAndStoreMapTexture(3, RadarMapData.FungleMapBase64);
				LoadAirshipTexture();
				Debug.Log(Object.op_Implicit($"[RADAR] All map textures loaded. Count: {_mapTextures.Count}"));
			}
			catch (Exception ex)
			{
				Debug.LogError(Object.op_Implicit("[RADAR] Critical error in LoadMapTexture: " + ex.GetType().Name + ": " + ex.Message));
			}
			_mapTextureLoaded = true;
		}
	}

	private static void LoadAndStoreMapTexture(int mapType, string base64Data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Texture2D value = LoadSingleMapTexture(mapType, base64Data);
			_mapTextures[mapType] = value;
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit($"[RADAR] Exception loading {GetMapName(mapType)} map: {ex.GetType().Name}: {ex.Message}"));
			_mapTextures[mapType] = CreateFallbackTexture();
		}
	}

	private static Texture2D LoadSingleMapTexture(int mapType, string base64Data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		string mapName = GetMapName(mapType);
		if (string.IsNullOrEmpty(base64Data))
		{
			Debug.LogWarning(Object.op_Implicit("[RADAR] No map data for " + mapName));
			return CreateFallbackTexture();
		}
		base64Data = base64Data.Trim().Replace("\r", "").Replace("\n", "");
		if (base64Data.Length < 1000)
		{
			Debug.LogWarning(Object.op_Implicit($"[RADAR] Base64 too short for {mapName} ({base64Data.Length} chars)"));
			return CreateFallbackTexture();
		}
		byte[] array = Convert.FromBase64String(base64Data);
		if (array.Length < 100)
		{
			Debug.LogWarning(Object.op_Implicit("[RADAR] Map data too small for " + mapName));
			return CreateFallbackTexture();
		}
		Texture2D val = new Texture2D(2, 2, (TextureFormat)4, false);
		if (!ImageConversion.LoadImage(val, Il2CppStructArray<byte>.op_Implicit(array)) || ((Texture)val).width <= 4 || ((Texture)val).height <= 4)
		{
			Debug.LogWarning(Object.op_Implicit($"[RADAR] Texture load failed for {mapName}: {((Texture)val).width}x{((Texture)val).height}"));
			Object.Destroy((Object)(object)val);
			return CreateFallbackTexture();
		}
		((Object)val).hideFlags = (HideFlags)61;
		((Texture)val).filterMode = (FilterMode)1;
		((Texture)val).wrapMode = (TextureWrapMode)1;
		Debug.Log(Object.op_Implicit($"[RADAR] ✓ {mapName} map texture loaded: {((Texture)val).width}x{((Texture)val).height}"));
		return val;
	}

	private static Texture2D CreateFallbackTexture()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Texture2D val = new Texture2D(64, 64, (TextureFormat)4, false);
			Color32 val2 = default(Color32);
			_002Ector((byte)40, (byte)40, (byte)50, (byte)220);
			Color32 val3 = default(Color32);
			_002Ector((byte)180, (byte)60, (byte)180, (byte)220);
			Color32[] array = (Color32[])(object)new Color32[4096];
			for (int i = 0; i < 64; i++)
			{
				for (int j = 0; j < 64; j++)
				{
					bool flag = (j / 8 + i / 8) % 2 == 0;
					array[i * 64 + j] = (flag ? val2 : val3);
				}
			}
			val.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
			val.Apply();
			((Object)val).hideFlags = (HideFlags)61;
			((Texture)val).filterMode = (FilterMode)0;
			Debug.LogWarning(Object.op_Implicit("[RADAR] BLACKHAT FIX: Using checkerboard fallback texture - map asset corrupted or missing!"));
			return val;
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[RADAR] CreateFallbackTexture failed: " + ex.Message));
			return Texture2D.whiteTexture;
		}
	}

	private static void DrawMapImage(Rect mapArea)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		Texture2D value;
		Texture2D val = (_isAirship ? _airshipMapTexture : (_mapTextures.TryGetValue(_currentMapType, out value) ? value : null));
		if (!((Object)(object)val == (Object)null) && TryGetMapInfo(out var info))
		{
			if (_cachedMapStyle == null || (Object)(object)_cachedMapStyle.normal.background != (Object)(object)val)
			{
				_cachedMapStyle = new GUIStyle();
				_cachedMapStyle.normal.background = val;
			}
			Vector2 val2 = WorldToRadar(info.imgMinX, info.imgMaxY, mapArea);
			Vector2 val3 = WorldToRadar(info.imgMaxX, info.imgMinY, mapArea);
			Rect val4 = default(Rect);
			(val4)._002Ector(val2.x, val2.y, val3.x - val2.x, val3.y - val2.y);
			Color color = GUI.color;
			GUI.color = Color.white;
			try
			{
				GUI.Box(val4, GUIContent.none, _cachedMapStyle);
			}
			catch
			{
			}
			GUI.color = color;
		}
	}

	private static void UpdateCache()
	{
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Invalid comparison between Unknown and I4
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		if (time - _lastRadarUpdate < 0.1f)
		{
			return;
		}
		_lastRadarUpdate = time;
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return;
		}
		Vector2 val = (_localPos = Vector2.op_Implicit(((Component)localPlayer).transform.position));
		UpdateMapInfo();
		_cachedAliveCount = 0;
		_playerCacheCount = 0;
		_nearestImpostorDist = float.MaxValue;
		try
		{
			List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
			int count = allPlayerControls.Count;
			for (int i = 0; i < count; i++)
			{
				PlayerControl val2 = null;
				try
				{
					val2 = allPlayerControls[i];
				}
				catch
				{
					continue;
				}
				if ((Object)(object)val2 == (Object)null || (Object)(object)val2.Data == (Object)null || val2.PlayerId == localPlayer.PlayerId)
				{
					continue;
				}
				if (_playerCacheCount >= 15)
				{
					break;
				}
				bool flag = false;
				try
				{
					flag = val2.Data.IsDead;
				}
				catch
				{
					continue;
				}
				if (!flag)
				{
					_cachedAliveCount++;
				}
				if (flag && !RadarShowGhosts)
				{
					continue;
				}
				bool flag2 = false;
				bool flag3 = false;
				try
				{
					if ((Object)(object)val2.Data.Role != (Object)null)
					{
						flag2 = val2.Data.Role.IsImpostor;
						flag3 = true;
					}
				}
				catch
				{
				}
				if (!flag3)
				{
					try
					{
						if ((Object)(object)val2.Data.Role != (Object)null)
						{
							flag2 = (int)val2.Data.Role.TeamType == 1;
							flag3 = true;
						}
					}
					catch
					{
					}
				}
				if (!flag3)
				{
					try
					{
						flag2 = RoleManager.IsImpostorRole(val2.Data.RoleType);
					}
					catch
					{
					}
				}
				int num = 0;
				try
				{
					num = val2.Data.DefaultOutfit.ColorId;
				}
				catch
				{
				}
				Vector2 zero = Vector2.zero;
				try
				{
					zero = Vector2.op_Implicit(((Component)val2).transform.position);
				}
				catch
				{
					continue;
				}
				if (flag2 && !flag)
				{
					float num2 = zero.x - val.x;
					float num3 = zero.y - val.y;
					float num4 = Mathf.Sqrt(num2 * num2 + num3 * num3);
					if (num4 < _nearestImpostorDist)
					{
						_nearestImpostorDist = num4;
					}
				}
				string name = "";
				try
				{
					name = ((val2.Data.PlayerName.Length > 6) ? val2.Data.PlayerName.Substring(0, 6) : val2.Data.PlayerName);
				}
				catch
				{
				}
				_playerCache[_playerCacheCount++] = new RadarPlayerData
				{
					x = zero.x,
					y = zero.y,
					isImpostor = flag2,
					isDead = flag,
					colorId = Mathf.Clamp(num, 0, 17),
					name = name,
					playerId = val2.PlayerId
				};
			}
			try
			{
				if (!localPlayer.Data.IsDead)
				{
					_cachedAliveCount++;
				}
			}
			catch
			{
			}
		}
		catch
		{
		}
		_deadBodyCacheCount = 0;
		if (RadarShowDeadBodies)
		{
			try
			{
				List<PlayerControl> allPlayerControls2 = PlayerControl.AllPlayerControls;
				int count2 = allPlayerControls2.Count;
				for (int j = 0; j < count2; j++)
				{
					PlayerControl val3 = null;
					try
					{
						val3 = allPlayerControls2[j];
					}
					catch
					{
						continue;
					}
					if ((Object)(object)val3 == (Object)null || (Object)(object)val3.Data == (Object)null)
					{
						continue;
					}
					bool flag4 = false;
					try
					{
						flag4 = val3.Data.IsDead;
					}
					catch
					{
						continue;
					}
					if (flag4)
					{
						if (_deadBodyCacheCount >= 15)
						{
							break;
						}
						int num5 = 0;
						try
						{
							num5 = val3.Data.DefaultOutfit.ColorId;
						}
						catch
						{
						}
						string name2 = "";
						try
						{
							name2 = ((val3.Data.PlayerName.Length > 5) ? val3.Data.PlayerName.Substring(0, 5) : val3.Data.PlayerName);
						}
						catch
						{
						}
						Vector2 zero2 = Vector2.zero;
						try
						{
							zero2 = Vector2.op_Implicit(((Component)val3).transform.position);
						}
						catch
						{
							continue;
						}
						_deadBodyCache[_deadBodyCacheCount++] = new RadarDeadBodyData
						{
							x = zero2.x,
							y = zero2.y,
							colorId = Mathf.Clamp(num5, 0, 17),
							name = name2
						};
					}
				}
			}
			catch
			{
			}
		}
		try
		{
			if (WebRadarService.IsRunning)
			{
				RadarState state = default(RadarState);
				state.MapName = GetWebRadarMapKey(_currentMapType);
				state.Players = BuildWebRadarPlayerDataArray();
				PlayerControl localPlayer2 = PlayerControl.LocalPlayer;
				state.LocalPlayerId = (byte)((localPlayer2 != null) ? localPlayer2.PlayerId : 0);
				state.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				WebRadarService.Broadcast(state);
			}
		}
		catch
		{
		}
	}

	private static PlayerData[] BuildWebRadarPlayerDataArray()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
		int count = allPlayerControls.Count;
		int num = 0;
		if (_webRadarPool.Length < count)
		{
			_webRadarPool = new PlayerData[count];
		}
		for (int i = 0; i < count; i++)
		{
			PlayerControl val = null;
			try
			{
				val = allPlayerControls[i];
			}
			catch
			{
				continue;
			}
			if ((Object)(object)val == (Object)null || (Object)(object)val.Data == (Object)null)
			{
				continue;
			}
			bool isImpostor = false;
			try
			{
				if ((Object)(object)val.Data.Role != (Object)null)
				{
					isImpostor = val.Data.Role.IsImpostor;
				}
			}
			catch
			{
			}
			int num2 = 0;
			try
			{
				num2 = val.Data.DefaultOutfit.ColorId;
			}
			catch
			{
			}
			Vector2 zero = Vector2.zero;
			try
			{
				zero = Vector2.op_Implicit(((Component)val).transform.position);
			}
			catch
			{
				continue;
			}
			string name = "";
			try
			{
				name = ((val.Data.PlayerName.Length > 6) ? val.Data.PlayerName.Substring(0, 6) : val.Data.PlayerName);
			}
			catch
			{
			}
			bool isDead = false;
			try
			{
				isDead = val.Data.IsDead;
			}
			catch
			{
			}
			bool inVent = false;
			try
			{
				inVent = val.inVent;
			}
			catch
			{
			}
			_webRadarPool[num++] = new PlayerData
			{
				Id = val.PlayerId,
				Name = name,
				X = zero.x,
				Y = zero.y,
				ColorId = Mathf.Clamp(num2, 0, 17),
				IsImpostor = isImpostor,
				IsDead = isDead,
				InVent = inVent
			};
		}
		if (num == _webRadarPool.Length)
		{
			return _webRadarPool;
		}
		PlayerData[] array = new PlayerData[num];
		Array.Copy(_webRadarPool, array, num);
		return array;
	}

	private static void UpdateMapInfo()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if ((Object)(object)ShipStatus.Instance != (Object)null)
			{
				_currentMapType = ShipStatus.Instance.Type;
				_isAirship = ShipStatus.Instance is AirshipStatus;
				_cachedMapName = (_isAirship ? "AIRSHIP" : GetMapName(_currentMapType));
				return;
			}
			GameOptionsManager instance = GameOptionsManager.Instance;
			if (((instance != null) ? instance.CurrentGameOptions : null) != null)
			{
				_currentMapType = intGameOptionsManager.Instance.CurrentGameOptions.MapId;
				_cachedMapName = GetMapName(_currentMapType);
			}
			else if ((Object)(object)LobbyBehaviour.Instance != (Object)null)
			{
				_cachedMapName = "LOBBY";
			}
		}
		catch
		{
		}
	}

	private static string GetMapName(int mapType)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		if (_isAirship)
		{
			return "AIRSHIP";
		}
		if ((int)mapType == 0)
		{
			return "THE SKELD";
		}
		if ((int)mapType == 1)
		{
			return "MIRA HQ";
		}
		if ((int)mapType == 2)
		{
			return "POLUS";
		}
		if ((int)mapType == 3)
		{
			return "THE FUNGLE";
		}
		return "UNKNOWN";
	}

	private static string GetWebRadarMapKey(int mapType)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		if (_isAirship)
		{
			return "Airship";
		}
		if ((int)mapType == 0)
		{
			return "Ship";
		}
		if ((int)mapType == 1)
		{
			return "Hq";
		}
		if ((int)mapType == 2)
		{
			return "Pb";
		}
		if ((int)mapType == 3)
		{
			return "Fungle";
		}
		return "Ship";
	}

	private static Vector2 WorldToRadar(float worldX, float worldY, Rect mapArea)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetMapInfo(out var info))
		{
			info = new MapInfo(0f, 0f, 4f, -20f, 20f, -15f, 10f);
		}
		float num = info.maxX - info.minX;
		float num2 = info.maxY - info.minY;
		float num3 = (mapArea).width / num;
		float num4 = (mapArea).height / num2;
		float num5 = Mathf.Min(num3, num4);
		float num6 = ((mapArea).width - num * num5) * 0.5f;
		float num7 = ((mapArea).height - num2 * num5) * 0.5f;
		float num8 = (mapArea).x + num6 + (worldX - info.minX) * num5;
		float num9 = (mapArea).y + num7 + (info.maxY - worldY) * num5;
		return new Vector2(num8, num9);
	}

	private static void HandleInput(float rx, float ry, float rw, float rh)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(Input.mousePosition);
		val.y = (float)Screen.height - val.y;
		Rect val2 = default(Rect);
		(val2)._002Ector(rx, ry, rw, rh);
		Rect val3 = default(Rect);
		(val3)._002Ector(rx, ry, rw - 44f, 24f);
		Rect val4 = default(Rect);
		(val4)._002Ector(rx + rw - 20f, ry + rh - 20f, 20f, 20f);
		Rect val5 = default(Rect);
		(val5)._002Ector(rx + 4f, ry + 24f + 2f, rw - 8f, rh - 24f - 18f - 4f);
		bool flag = false;
		try
		{
			flag = (Object)(object)MeetingHud.Instance != (Object)null;
		}
		catch
		{
		}
		try
		{
			if (!flag && (val2).Contains(val) && (Object)(object)PlayerControl.LocalPlayer != (Object)null)
			{
				if (PlayerControl.LocalPlayer.moveable)
				{
					PlayerControl.LocalPlayer.moveable = false;
					_radarBlockedMovement = true;
				}
			}
			else if (_radarBlockedMovement && !_isDragging && !_isResizing && (Object)(object)PlayerControl.LocalPlayer != (Object)null)
			{
				PlayerControl.LocalPlayer.moveable = true;
				_radarBlockedMovement = false;
			}
		}
		catch
		{
			_radarBlockedMovement = false;
		}
		if (!RadarLocked && !RadarMinimized && (val2).Contains(val))
		{
			float y = Input.mouseScrollDelta.y;
			if (y != 0f)
			{
				float num = 20f;
				float num2 = Mathf.Clamp((_radarRect).width + y * num, 200f, 500f);
				float width = (_radarRect).width;
				float height = (_radarRect).height;
				(_radarRect).width = num2;
				(_radarRect).height = num2;
				float num3 = (val.x - rx) / width;
				float num4 = (val.y - ry) / height;
				(_radarRect).x = Mathf.Clamp(val.x - num3 * num2, 0f, (float)Screen.width - num2);
				(_radarRect).y = Mathf.Clamp(val.y - num4 * num2, 0f, (float)Screen.height - num2);
			}
		}
		if (!RadarLocked)
		{
			if (Input.GetMouseButtonDown(0) && (val4).Contains(val))
			{
				_isResizing = true;
				_resizeStart = val;
				_resizeStartRect = _radarRect;
			}
			if (_isResizing && Input.GetMouseButton(0))
			{
				Vector2 val6 = val - _resizeStart;
				float height2 = ((_radarRect).width = Mathf.Clamp((_resizeStartRect).width + Mathf.Max(val6.x, val6.y), 200f, 500f));
				(_radarRect).height = height2;
			}
			if (Input.GetMouseButtonUp(0))
			{
				_isResizing = false;
			}
		}
		if (!RadarLocked && !_isResizing)
		{
			if (Input.GetMouseButtonDown(0) && (val3).Contains(val))
			{
				_isDragging = true;
				_dragOffset = val - new Vector2(rx, ry);
			}
			if (_isDragging && Input.GetMouseButton(0))
			{
				(_radarRect).x = Mathf.Clamp(val.x - _dragOffset.x, 0f, (float)Screen.width - rw);
				(_radarRect).y = Mathf.Clamp(val.y - _dragOffset.y, 0f, (float)Screen.height - rh);
			}
			if (Input.GetMouseButtonUp(0))
			{
				_isDragging = false;
			}
		}
		if (!RadarRightClickTP || RadarMinimized || flag || !Input.GetMouseButtonDown(1) || !(val5).Contains(val) || Input.GetKey((KeyCode)304) || Input.GetKey((KeyCode)306) || !TryGetMapInfo(out var info))
		{
			return;
		}
		float num6 = (val.x - (val5).x) / (val5).width;
		float num7 = 1f - (val.y - (val5).y) / (val5).height;
		Vector2 val7 = default(Vector2);
		(val7)._002Ector(info.minX + num6 * (info.maxX - info.minX), info.minY + num7 * (info.maxY - info.minY));
		try
		{
			PlayerControl.LocalPlayer.NetTransform.SnapTo(val7);
			if ((Object)(object)AmongUsClient.Instance != (Object)null)
			{
				MessageWriter val8 = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)PlayerControl.LocalPlayer).NetId, (byte)21, (SendOption)1, -1);
				NetHelpers.WriteVector2(val7, val8);
				val8.Write(PlayerControl.LocalPlayer.NetTransform.lastSequenceId);
				((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val8);
			}
		}
		catch
		{
		}
	}

	internal static void UpdateRadarLogic()
	{
		try
		{
			bool isRunning = WebRadarService.IsRunning;
			if ((RadarEnabled || isRunning) && !((Object)(object)PlayerControl.LocalPlayer == (Object)null))
			{
				if (RadarEnabled)
				{
					_radarFrame = (_radarFrame + 1) % 180;
					_pulseTime += Time.deltaTime;
				}
				if (Time.time - _lastCacheUpdateTime >= 0.1f)
				{
					_lastCacheUpdateTime = Time.time;
					UpdateCache();
				}
			}
		}
		catch
		{
		}
	}

	internal static void DrawRadar()
	{
		//IL_08ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_090a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0921: Unknown result type (might be due to invalid IL or missing references)
		//IL_0947: Unknown result type (might be due to invalid IL or missing references)
		//IL_0951: Unknown result type (might be due to invalid IL or missing references)
		//IL_095e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0975: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_09cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d27: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d29: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d32: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d8a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d91: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dd5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ddf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e03: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e17: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e2b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e41: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e4e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e65: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e8f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ebb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ec8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0edf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f47: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f94: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fb5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fd2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fff: Unknown result type (might be due to invalid IL or missing references)
		//IL_101d: Unknown result type (might be due to invalid IL or missing references)
		//IL_103a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1067: Unknown result type (might be due to invalid IL or missing references)
		//IL_1085: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_10f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1129: Unknown result type (might be due to invalid IL or missing references)
		//IL_1146: Unknown result type (might be due to invalid IL or missing references)
		//IL_1164: Unknown result type (might be due to invalid IL or missing references)
		//IL_1181: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1209: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a31: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a55: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a62: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a79: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a95: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ade: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aeb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b02: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b6b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b78: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b8f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1289: Unknown result type (might be due to invalid IL or missing references)
		//IL_12d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_12f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1338: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ced: Unknown result type (might be due to invalid IL or missing references)
		//IL_1378: Unknown result type (might be due to invalid IL or missing references)
		//IL_13aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_13d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bdf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c11: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c42: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c63: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c70: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ca2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cb9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd0: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0600: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Unknown result type (might be due to invalid IL or missing references)
		//IL_062c: Unknown result type (might be due to invalid IL or missing references)
		//IL_063c: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0811: Unknown result type (might be due to invalid IL or missing references)
		//IL_0833: Unknown result type (might be due to invalid IL or missing references)
		//IL_0856: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!RadarEnabled)
			{
				return;
			}
			if (_radarInitialized && _sWindowBg == null)
			{
				_radarInitialized = false;
			}
			if ((Object)(object)PlayerControl.LocalPlayer == (Object)null)
			{
				RadarEnabled = false;
				Debug.LogWarning(Object.op_Implicit("[RADAR] FIX 2026: Auto-disabled - LocalPlayer is null"));
				return;
			}
			if ((Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).IsGameStarted && (Object)(object)ShipStatus.Instance == (Object)null)
			{
				RadarEnabled = false;
				Debug.LogWarning(Object.op_Implicit("[RADAR] FIX 2026: Auto-disabled - ShipStatus is null during game"));
				return;
			}
			InitRadar();
			float x = (_radarRect).x;
			float y = (_radarRect).y;
			float width = (_radarRect).width;
			float num = (RadarMinimized ? 28f : (_radarRect).height);
			HandleInput(x, y, width, (_radarRect).height);
			GUI.Box(new Rect(x + 3f, y + 3f, width, num), GUIContent.none, _sShadow);
			GUI.Box(new Rect(x, y, width, num), GUIContent.none, _sWindowBg);
			if (RadarShowBorder)
			{
				float num2 = 0.6f + 0.4f * Mathf.Sin(_pulseTime * 3f);
				GUI.color = new Color(1f, 1f, 1f, num2);
				GUI.Box(new Rect(x, y, width, 2f), GUIContent.none, _sBorderAccent);
				GUI.Box(new Rect(x, y + num - 2f, width, 2f), GUIContent.none, _sBorderAccent);
				GUI.Box(new Rect(x, y, 2f, num), GUIContent.none, _sBorderAccent);
				GUI.Box(new Rect(x + width - 2f, y, 2f, num), GUIContent.none, _sBorderAccent);
				GUI.color = Color.white;
			}
			if (_nearestImpostorDist < 5f && RadarRevealRoles)
			{
				float num3 = Mathf.PingPong(Time.time * 4f, 1f);
				GUI.color = new Color(1f, 0.05f, 0.05f, 0.5f + num3 * 0.5f);
				GUI.Box(new Rect(x - 2f, y - 2f, width + 4f, 4f), GUIContent.none, _sBorderAccent);
				GUI.Box(new Rect(x - 2f, y + num - 2f, width + 4f, 4f), GUIContent.none, _sBorderAccent);
				GUI.Box(new Rect(x - 2f, y - 2f, 4f, num + 4f), GUIContent.none, _sBorderAccent);
				GUI.Box(new Rect(x + width - 2f, y - 2f, 4f, num + 4f), GUIContent.none, _sBorderAccent);
				GUI.color = Color.white;
			}
			GUI.Box(new Rect(x + 2f, y + 2f, width - 4f, 24f), GUIContent.none, _sHeaderBg);
			float num4 = 0.7f + 0.3f * Mathf.Sin(_pulseTime * 4f);
			GUI.color = new Color(1f, 1f, 1f, num4);
			GUI.Box(new Rect(x + 2f, y + 2f, width - 4f, 1f), GUIContent.none, _sBorderAccent);
			GUI.color = Color.white;
			_sTitle.alignment = (TextAnchor)3;
			_sTitle.normal.textColor = GuiStyles.Theme.Accent;
			string text = ((int)(_pulseTime * 4f) % 4) switch
			{
				0 => "●", 
				1 => "◔", 
				2 => "◑", 
				3 => "◕", 
				_ => "●", 
			};
			GUI.Label(new Rect(x + 6f, y + 3f, 16f, 18f), text, _sTitle);
			_sTitle.normal.textColor = GuiStyles.Theme.TextPrimary;
			_sTitle.fontSize = 12;
			GUI.Label(new Rect(x + 22f, y + 3f, 80f, 18f), "RADAR", _sTitle);
			_sSmall.alignment = (TextAnchor)5;
			_sSmall.normal.textColor = GuiStyles.Theme.AccentSoft;
			_sSmall.fontSize = 9;
			GUI.Label(new Rect(x + width - 140f, y + 4f, 90f, 16f), _cachedMapName, _sSmall);
			if (GUI.Button(new Rect(x + width - 46f, y + 4f, 20f, 18f), RadarMinimized ? "□" : "−", _sButton))
			{
				RadarMinimized = !RadarMinimized;
			}
			if (GUI.Button(new Rect(x + width - 24f, y + 4f, 20f, 18f), "×", _sButton))
			{
				RadarEnabled = false;
			}
			GUI.Box(new Rect(x + 4f, y + 24f, width - 8f, 1f), GUIContent.none, _sBorderDim);
			if (RadarMinimized)
			{
				return;
			}
			Rect val = default(Rect);
			(val)._002Ector(x + 4f, y + 24f + 2f, width - 8f, num - 24f - 18f - 4f);
			GUI.Box(val, GUIContent.none, _sMapBg);
			bool flag = (_isAirship ? ((Object)(object)_airshipMapTexture != (Object)null) : (_mapTextures.ContainsKey(_currentMapType) && (Object)(object)_mapTextures[_currentMapType] != (Object)null));
			if (RadarShowMapImage && flag)
			{
				DrawMapImage(val);
			}
			else
			{
				DrawMapRooms(val);
			}
			DrawMapGridSafe(val);
			float num5 = Time.time * 0.4f % 1f;
			GUI.Box(new Rect((val).x + num5 * (val).width, (val).y, 2f, (val).height), GUIContent.none, _sScanLine);
			GUI.color = new Color(1f, 1f, 1f, 0.04f);
			for (int i = 0; (float)i < (val).height; i += 20)
			{
				GUI.Box(new Rect((val).x, (val).y + (float)i, (val).width, 1f), GUIContent.none, _sScanLine);
			}
			GUI.color = Color.white;
			if (_sonarInitialized && (Object)(object)_texSonarLine != (Object)null)
			{
				_sonarAngle += Time.deltaTime * 180f;
				if (_sonarAngle >= 360f)
				{
					_sonarAngle -= 360f;
				}
				float num6 = (val).x + (val).width / 2f;
				float num7 = (val).y + (val).height / 2f;
				float num8 = Mathf.Min((val).width, (val).height) / 2f - 4f;
				float num9 = _sonarAngle * ((float)Math.PI / 180f);
				float num10 = num6 + Mathf.Sin(num9) * num8;
				float num11 = num7 - Mathf.Cos(num9) * num8;
				int num12 = 8;
				for (int j = 0; j < num12; j++)
				{
					float num13 = (float)j / (float)num12;
					float num14 = Mathf.Lerp(num6, num10, num13);
					float num15 = Mathf.Lerp(num7, num11, num13);
					float num16 = 0.6f * (1f - num13);
					GUI.color = new Color(0.2f, 1f, 0.4f, num16);
					GUI.Box(new Rect(num14 - 1f, num15 - 1f, 3f, 3f), GUIContent.none, _sScanLine);
				}
				GUI.color = Color.white;
			}
			int num17 = 0;
			try
			{
				num17 = PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId;
			}
			catch
			{
			}
			num17 = Mathf.Clamp(num17, 0, 17);
			for (int k = 0; k < _deadBodyCacheCount; k++)
			{
				ref RadarDeadBodyData reference = ref _deadBodyCache[k];
				Vector2 val2 = WorldToRadar(reference.x, reference.y, val);
				if ((val).Contains(val2))
				{
					GUI.Box(new Rect(val2.x - 7f, val2.y - 7f, 14f, 14f), GUIContent.none, _sDeadBodyBg);
					GUI.Box(new Rect(val2.x - 5f, val2.y - 5f, 10f, 10f), GUIContent.none, _sPlayers[reference.colorId]);
					_sLabel.normal.textColor = Color.white;
					GUI.Label(new Rect(val2.x - 7f, val2.y - 8f, 14f, 16f), "X", _sLabel);
				}
			}
			int num18 = 0;
			for (int l = 0; l < _playerCacheCount; l++)
			{
				ref RadarPlayerData reference2 = ref _playerCache[l];
				Vector2 val3 = WorldToRadar(reference2.x, reference2.y, val);
				val3.x = Mathf.Clamp(val3.x, (val).xMin + 8f, (val).xMax - 8f);
				val3.y = Mathf.Clamp(val3.y, (val).yMin + 8f, (val).yMax - 8f);
				if (reference2.isDead)
				{
					_ = GUI.color;
					GUI.color = new Color(1f, 1f, 1f, 0.35f);
					GUI.Box(new Rect(val3.x - 6f, val3.y - 6f, 12f, 12f), GUIContent.none, _sPlayers[reference2.colorId]);
					GUI.color = Color.white;
					_sLabel.normal.textColor = new Color(1f, 0.2f, 0.2f, 0.9f);
					_sLabel.fontSize = 14;
					_sLabel.fontStyle = (FontStyle)1;
					GUI.Label(new Rect(val3.x - 7f, val3.y - 9f, 14f, 18f), "✖", _sLabel);
					_sLabel.fontSize = 10;
					_sLabel.fontStyle = (FontStyle)0;
					_sSmall.normal.textColor = new Color(0.65f, 0.65f, 0.7f, 0.55f);
					_sSmall.alignment = (TextAnchor)4;
					_sSmall.fontSize = 8;
					GUI.Label(new Rect(val3.x - 25f, val3.y + 8f, 50f, 10f), reference2.name, _sSmall);
					_sSmall.fontSize = 9;
				}
				else if (reference2.isImpostor && RadarRevealRoles)
				{
					num18++;
					GUI.Box(new Rect(val3.x - 12f, val3.y - 12f, 24f, 24f), GUIContent.none, _sImpostorGlow);
					DrawPlayerDotSafe(val3.x, val3.y, reference2.colorId, isImp: true);
					string content = "[IMP] " + reference2.name;
					_sSmall.normal.textColor = Color.red;
					_sSmall.alignment = (TextAnchor)4;
					_sSmall.fontSize = 9;
					GuiStyles.DrawOutlinedLabel(new Rect(val3.x - 35f, val3.y + 8f, 70f, 12f), content, _sSmall);
					_sLabel.normal.textColor = Color.yellow;
					GUI.Label(new Rect(val3.x + 4f, val3.y - 12f, 12f, 12f), "!", _sLabel);
				}
				else
				{
					DrawPlayerDotSafe(val3.x, val3.y, reference2.colorId, isImp: false);
				}
			}
			Vector2 val4 = WorldToRadar(_localPos.x, _localPos.y, val);
			val4.x = Mathf.Clamp(val4.x, (val).xMin + 8f, (val).xMax - 8f);
			val4.y = Mathf.Clamp(val4.y, (val).yMin + 8f, (val).yMax - 8f);
			DrawPlayerDotSafe(val4.x, val4.y, num17, isImp: false);
			float num19 = 0.7f + 0.3f * Mathf.Sin(_pulseTime * 5f);
			GUI.color = new Color(1f, 1f, 1f, num19 * 0.5f);
			GUI.Box(new Rect(val4.x - 12f, val4.y - 12f, 24f, 24f), GUIContent.none, _sVisor);
			GUI.color = Color.white;
			_sLabel.normal.textColor = ThemeVisor;
			_sLabel.fontSize = 12;
			GUI.Label(new Rect(val4.x - 12f, val4.y - 12f, 24f, 24f), "◎", _sLabel);
			_sLabel.fontSize = 10;
			_sSmall.normal.textColor = ThemeVisor;
			_sSmall.alignment = (TextAnchor)4;
			_sSmall.fontSize = 9;
			_sSmall.fontStyle = (FontStyle)1;
			GUI.Label(new Rect(val4.x - 14f, val4.y + 12f, 28f, 12f), "YOU", _sSmall);
			_sSmall.fontStyle = (FontStyle)0;
			float num20 = y + num - 18f;
			GUI.Box(new Rect(x + 2f, num20, width - 4f, 16f), GUIContent.none, _sHeaderBg);
			GUI.Box(new Rect(x + 4f, num20, width - 8f, 1f), GUIContent.none, _sBorderDim);
			float num21 = num20 + 2f;
			_sSmall.alignment = (TextAnchor)3;
			_sSmall.fontSize = 8;
			GUI.Box(new Rect(x + 6f, num21 + 3f, 7f, 7f), GUIContent.none, _sPlayers[num17]);
			_sSmall.normal.textColor = GuiStyles.Theme.Accent;
			GUI.Label(new Rect(x + 15f, num21, 18f, 12f), "EU", _sSmall);
			GUI.Box(new Rect(x + 38f, num21 + 3f, 7f, 7f), GUIContent.none, _sImpostorDot);
			_sSmall.normal.textColor = GuiStyles.Theme.Error;
			GUI.Label(new Rect(x + 47f, num21, 20f, 12f), "IMP", _sSmall);
			GUI.Box(new Rect(x + 72f, num21 + 3f, 7f, 7f), GUIContent.none, _sVisor);
			_sSmall.normal.textColor = ThemeVisor;
			GUI.Label(new Rect(x + 81f, num21, 30f, 12f), "CREW", _sSmall);
			_sSmall.normal.textColor = new Color(1f, 0.2f, 0.2f, 0.9f);
			GUI.Label(new Rect(x + 114f, num21 - 1f, 18f, 14f), "✖", _sSmall);
			_sSmall.normal.textColor = new Color(0.6f, 0.6f, 0.65f, 0.8f);
			GUI.Label(new Rect(x + 126f, num21, 25f, 12f), "\ud83d\udc80", _sSmall);
			_sSmall.normal.textColor = GuiStyles.Theme.TextMuted;
			GUI.Label(new Rect(x + 150f, num21, 35f, 12f), $"[{_cachedAliveCount}]", _sSmall);
			_sSmall.alignment = (TextAnchor)4;
			_sSmall.normal.textColor = ThemeVisor;
			_sSmall.fontSize = 8;
			GUI.Label(new Rect(x + width / 2f - 50f, num21, 100f, 12f), $"X:{_localPos.x:F1} Y:{_localPos.y:F1}", _sSmall);
			if (num18 > 0)
			{
				_sSmall.alignment = (TextAnchor)5;
				_sSmall.normal.textColor = GuiStyles.Theme.Error;
				_sSmall.fontSize = 9;
				_sSmall.fontStyle = (FontStyle)1;
				float num22 = 0.6f + 0.4f * Mathf.Sin(_pulseTime * 6f);
				GUI.color = new Color(1f, 1f, 1f, num22);
				GUI.Label(new Rect(x + width - 75f, num21, 65f, 12f), $"⚠ {num18} IMP", _sSmall);
				GUI.color = Color.white;
				_sSmall.fontStyle = (FontStyle)0;
			}
			_sSmall.fontSize = 9;
			if (!RadarLocked)
			{
				_sSmall.alignment = (TextAnchor)8;
				_sSmall.normal.textColor = GuiStyles.Theme.AccentDim;
				_sSmall.fontSize = 14;
				GUI.Label(new Rect(x + width - 18f, y + num - 17f, 16f, 14f), "◢", _sSmall);
				_sSmall.alignment = (TextAnchor)6;
				_sSmall.normal.textColor = GuiStyles.Theme.TextMuted;
				_sSmall.fontSize = 8;
				GUI.Label(new Rect(x + 4f, y + num - 14f, 90f, 12f), "↕ Scroll=Zoom", _sSmall);
			}
			_sSmall.fontSize = 9;
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[Radar] Error: " + ex.GetType().Name + ": " + ex.Message));
			if (ex.StackTrace != null && ex.StackTrace.Length > 0)
			{
				Debug.LogError(Object.op_Implicit("[Radar] Stack: " + ex.StackTrace.Substring(0, Math.Min(500, ex.StackTrace.Length))));
			}
		}
	}

	private static void DrawPlayerDotSafe(float px, float py, int colorId, bool isImp)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		colorId = Mathf.Clamp(colorId, 0, 17);
		if (RadarDrawIcons)
		{
			GUI.Box(new Rect(px - 6f, py - 7f, 12f, 14f), GUIContent.none, _sShadow);
			GUI.Box(new Rect(px - 5f, py - 6f, 10f, 12f), GUIContent.none, _sPlayers[colorId]);
			GUI.Box(new Rect(px - 1f, py - 5f, 4f, 3f), GUIContent.none, _sVisor);
			GUI.Box(new Rect(px - 7f, py - 1f, 3f, 5f), GUIContent.none, _sPlayers[colorId]);
		}
		else
		{
			GUI.Box(new Rect(px - 6f, py - 6f, 12f, 12f), GUIContent.none, _sShadow);
			GUI.Box(new Rect(px - 5f, py - 5f, 10f, 10f), GUIContent.none, _sPlayers[colorId]);
		}
	}

	private static void DrawMapGridSafe(Rect mapArea)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		int num = 8;
		float num2 = (mapArea).width / (float)num;
		float num3 = (mapArea).height / (float)num;
		for (int i = 1; i < num; i++)
		{
			float num4 = (mapArea).x + (float)i * num2;
			bool flag = i == num / 2;
			GUI.Box(new Rect(num4, (mapArea).y, (float)((!flag) ? 1 : 2), (mapArea).height), GUIContent.none, flag ? _sGridBright : _sGridDim);
		}
		for (int j = 1; j < num; j++)
		{
			float num5 = (mapArea).y + (float)j * num3;
			bool flag2 = j == num / 2;
			GUI.Box(new Rect((mapArea).x, num5, (mapArea).width, (float)((!flag2) ? 1 : 2)), GUIContent.none, flag2 ? _sGridBright : _sGridDim);
		}
		GUI.Box(new Rect((mapArea).x, (mapArea).y, (mapArea).width, 1f), GUIContent.none, _sGridBright);
		GUI.Box(new Rect((mapArea).x, (mapArea).yMax - 1f, (mapArea).width, 1f), GUIContent.none, _sGridBright);
		GUI.Box(new Rect((mapArea).x, (mapArea).y, 1f, (mapArea).height), GUIContent.none, _sGridBright);
		GUI.Box(new Rect((mapArea).xMax - 1f, (mapArea).y, 1f, (mapArea).height), GUIContent.none, _sGridBright);
		_sSmall.alignment = (TextAnchor)4;
		_sSmall.normal.textColor = new Color(0.4f, 0.7f, 0.9f, 0.6f);
		_sSmall.fontSize = 9;
		float num6 = (mapArea).x + (mapArea).width * 0.5f;
		float num7 = (mapArea).y + (mapArea).height * 0.5f;
		GUI.Label(new Rect(num6 - 8f, (mapArea).y + 2f, 16f, 12f), "N", _sSmall);
		GUI.Label(new Rect(num6 - 8f, (mapArea).yMax - 14f, 16f, 12f), "S", _sSmall);
		GUI.Label(new Rect((mapArea).x + 2f, num7 - 6f, 12f, 12f), "W", _sSmall);
		GUI.Label(new Rect((mapArea).xMax - 14f, num7 - 6f, 12f, 12f), "E", _sSmall);
		_sSmall.fontSize = 8;
	}

	private static void DrawMapRooms(Rect mapArea)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		RoomData[] currentMapRooms = GetCurrentMapRooms();
		if (currentMapRooms == null || currentMapRooms.Length == 0)
		{
			return;
		}
		if (!TryGetMapInfo(out var info))
		{
			info = new MapInfo(0f, 0f, 4f, -20f, 20f, -15f, 10f);
		}
		RoomData[] array = currentMapRooms;
		Rect val3 = default(Rect);
		for (int i = 0; i < array.Length; i++)
		{
			RoomData roomData = array[i];
			float worldX = roomData.x - roomData.w / 2f;
			float worldY = roomData.y + roomData.h / 2f;
			float worldX2 = roomData.x + roomData.w / 2f;
			float worldY2 = roomData.y - roomData.h / 2f;
			Vector2 val = WorldToRadar(worldX, worldY, mapArea);
			Vector2 val2 = WorldToRadar(worldX2, worldY2, mapArea);
			(val3)._002Ector(val.x, val.y, val2.x - val.x, val2.y - val.y);
			if (!((val3).xMax < (mapArea).x) && !((val3).x > (mapArea).xMax) && !((val3).yMax < (mapArea).y) && !((val3).y > (mapArea).yMax))
			{
				switch (roomData.shapeType)
				{
				case 5:
					GUI.Box(val3, GUIContent.none, _sCorridorFill);
					break;
				case 4:
					DrawHexagonRoom(val3);
					break;
				case 2:
					DrawPentagonRoom(val3, pointLeft: true);
					break;
				case 3:
					DrawPentagonRoom(val3, pointLeft: false);
					break;
				case 1:
					DrawOctagonRoom(val3);
					break;
				default:
					DrawRectRoom(val3);
					break;
				}
			}
		}
	}

	private static void DrawOctagonRoom(Rect rect)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Min((rect).width, (rect).height) * 0.15f;
		GUI.Box(new Rect((rect).x + num, (rect).y, (rect).width - num * 2f, (rect).height), GUIContent.none, _sRoomFill);
		GUI.Box(new Rect((rect).x, (rect).y + num, (rect).width, (rect).height - num * 2f), GUIContent.none, _sRoomFill);
		GUI.Box(new Rect((rect).x + num, (rect).y, (rect).width - num * 2f, 2f), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).x + num, (rect).yMax - 2f, (rect).width - num * 2f, 2f), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).x, (rect).y + num, 2f, (rect).height - num * 2f), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).xMax - 2f, (rect).y + num, 2f, (rect).height - num * 2f), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).x, (rect).y + num - 2f, num, 2f), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).x + num - 2f, (rect).y, 2f, num), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).xMax - num, (rect).y + num - 2f, num, 2f), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).xMax - num, (rect).y, 2f, num), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).x, (rect).yMax - num, num, 2f), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).x + num - 2f, (rect).yMax - num, 2f, num), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).xMax - num, (rect).yMax - num, num, 2f), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).xMax - num, (rect).yMax - num, 2f, num), GUIContent.none, _sRoomBorder);
	}

	private static void DrawRectRoom(Rect rect)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		GUI.Box(rect, GUIContent.none, _sRoomFill);
		GUI.Box(new Rect((rect).x, (rect).y, (rect).width, 2f), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).x, (rect).yMax - 2f, (rect).width, 2f), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).x, (rect).y, 2f, (rect).height), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).xMax - 2f, (rect).y, 2f, (rect).height), GUIContent.none, _sRoomBorder);
	}

	private static void DrawHexagonRoom(Rect rect)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		float num = (rect).width * 0.25f;
		GUI.Box(new Rect((rect).x, (rect).y + (rect).height * 0.3f, (rect).width, (rect).height * 0.7f), GUIContent.none, _sRoomFill);
		GUI.Box(new Rect((rect).x + num * 0.5f, (rect).y, (rect).width - num, (rect).height * 0.4f), GUIContent.none, _sRoomFill);
		GUI.Box(new Rect((rect).x + num, (rect).y, (rect).width - num * 2f, 2f), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).x, (rect).y + num, 2f, (rect).height - num), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).xMax - 2f, (rect).y + num, 2f, (rect).height - num), GUIContent.none, _sRoomBorder);
		GUI.Box(new Rect((rect).x, (rect).yMax - 2f, (rect).width, 2f), GUIContent.none, _sRoomBorder);
	}

	private static void DrawPentagonRoom(Rect rect, bool pointLeft)
	{
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		float num = (rect).width * 0.3f;
		if (pointLeft)
		{
			GUI.Box(new Rect((rect).x + num * 0.5f, (rect).y, (rect).width - num * 0.5f, (rect).height), GUIContent.none, _sRoomFill);
			float num2 = (rect).y + (rect).height * 0.5f;
			GUI.Box(new Rect((rect).x, num2 - (rect).height * 0.25f, num, (rect).height * 0.5f), GUIContent.none, _sRoomFill);
			GUI.Box(new Rect((rect).x + num, (rect).y, (rect).width - num, 2f), GUIContent.none, _sRoomBorder);
			GUI.Box(new Rect((rect).x + num, (rect).yMax - 2f, (rect).width - num, 2f), GUIContent.none, _sRoomBorder);
			GUI.Box(new Rect((rect).xMax - 2f, (rect).y, 2f, (rect).height), GUIContent.none, _sRoomBorder);
			GUI.Box(new Rect((rect).x, num2 - 1f, num, 2f), GUIContent.none, _sRoomBorder);
		}
		else
		{
			GUI.Box(new Rect((rect).x, (rect).y, (rect).width - num * 0.5f, (rect).height), GUIContent.none, _sRoomFill);
			float num3 = (rect).y + (rect).height * 0.5f;
			GUI.Box(new Rect((rect).xMax - num, num3 - (rect).height * 0.25f, num, (rect).height * 0.5f), GUIContent.none, _sRoomFill);
			GUI.Box(new Rect((rect).x, (rect).y, (rect).width - num, 2f), GUIContent.none, _sRoomBorder);
			GUI.Box(new Rect((rect).x, (rect).yMax - 2f, (rect).width - num, 2f), GUIContent.none, _sRoomBorder);
			GUI.Box(new Rect((rect).x, (rect).y, 2f, (rect).height), GUIContent.none, _sRoomBorder);
			GUI.Box(new Rect((rect).xMax - num, num3 - 1f, num, 2f), GUIContent.none, _sRoomBorder);
		}
	}

	private static RoomData[] GetCurrentMapRooms()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		if (_isAirship)
		{
			return AirshipRooms;
		}
		if ((int)_currentMapType == 0)
		{
			return SkeldRooms;
		}
		if ((int)_currentMapType == 1)
		{
			return MiraRooms;
		}
		if ((int)_currentMapType == 2)
		{
			return PolusRooms;
		}
		if ((int)_currentMapType == 3)
		{
			return FungleRooms;
		}
		return SkeldRooms;
	}

	internal static void ToggleNoKillDistanceLimit(bool enable)
	{
		NoKillDistanceLimitEnabled = enable;
		if (CheatConfig.NoKillDistanceLimit != null)
		{
			CheatConfig.NoKillDistanceLimit.Value = enable;
		}
	}

	internal static void ToggleKillAndVent(bool enable)
	{
		KillAndVentEnabled = enable;
		LogCheat(enable ? "Kill & Vent ENABLED (Smart Escape)" : "Kill & Vent DISABLED");
	}

	internal static void SmartVentAfterKill(Vector2 killPos)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if ((Object)(object)DestroyableSingleton<HudManager>.Instance != (Object)null)
			{
				((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(CoSmartVentAfterKill(killPos)));
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[KillAndVent] Error starting coroutine: {value}"));
		}
	}

	[IteratorStateMachine(typeof(_003CCoSmartVentAfterKill_003Ed__576))]
	private static IEnumerator CoSmartVentAfterKill(Vector2 killPos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoSmartVentAfterKill_003Ed__576(0)
		{
			killPos = killPos
		};
	}

	internal static void ToggleSeeGhosts(bool enable)
	{
		SeeGhostsEnabled = enable;
		Enumerator<PlayerControl> enumerator;
		if (enable)
		{
			enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current == (Object)null || (Object)(object)current.Data == (Object)null || !current.Data.IsDead || ((InnerNetObject)current).AmOwner)
				{
					continue;
				}
				try
				{
					current.Visible = true;
					((Component)current).gameObject.layer = LayerMask.NameToLayer("Players");
					CosmeticsLayer cosmetics = current.cosmetics;
					if ((Object)(object)((cosmetics != null) ? cosmetics.nameText : null) != (Object)null)
					{
						((Component)current.cosmetics.nameText).gameObject.SetActive(true);
					}
				}
				catch
				{
				}
			}
			LogCheat("See Ghosts ON - You can see dead players!");
			return;
		}
		enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current2 = enumerator.Current;
			if ((Object)(object)current2 == (Object)null || (Object)(object)current2.Data == (Object)null || !current2.Data.IsDead || !((Object)(object)current2 != (Object)(object)PlayerControl.LocalPlayer))
			{
				continue;
			}
			try
			{
				if (!PlayerControl.LocalPlayer.Data.IsDead)
				{
					current2.Visible = false;
					((Component)current2).gameObject.layer = LayerMask.NameToLayer("Ghost");
				}
			}
			catch
			{
			}
		}
		LogCheat("Ver Fantasmas DESATIVADO");
	}

	internal static void ToggleSeeDeadChat(bool enable)
	{
		SeeDeadChatEnabled = enable;
		LogCheat(enable ? "Ver Chat dos Mortos ATIVADO" : "Ver Chat dos Mortos DESATIVADO");
	}

	internal static void ToggleShieldVis(bool enable)
	{
		ShieldVisEnabled = enable;
	}

	private static void ForceAddGhostChat(PlayerControl sourcePlayer, string chatText)
	{
		HudManager instance = DestroyableSingleton<HudManager>.Instance;
		if ((Object)(object)((instance != null) ? instance.Chat : null) == (Object)null)
		{
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if ((Object)(object)((localPlayer != null) ? localPlayer.Data : null) == (Object)null)
		{
			return;
		}
		try
		{
			bool num = !PlayerControl.LocalPlayer.Data.IsDead;
			if (num)
			{
				PlayerControl.LocalPlayer.Data.IsDead = true;
			}
			string text = "<color=#949EAD>[GHOST]</color> " + chatText;
			DestroyableSingleton<HudManager>.Instance.Chat.AddChat(sourcePlayer, text, false);
			if (num)
			{
				PlayerControl.LocalPlayer.Data.IsDead = false;
			}
			LogCheat("[GhostChat] " + sourcePlayer.Data.PlayerName + ": " + chatText);
		}
		catch (Exception ex)
		{
			PlayerControl localPlayer2 = PlayerControl.LocalPlayer;
			if ((Object)(object)((localPlayer2 != null) ? localPlayer2.Data : null) != (Object)null)
			{
				PlayerControl.LocalPlayer.Data.IsDead = false;
			}
			Debug.LogError(Object.op_Implicit("[GhostChat] Erro: " + ex.Message));
		}
	}

	internal static void ToggleAlwaysShowChat(bool enable)
	{
		AlwaysShowChatEnabled = enable;
		LogCheat(enable ? "Always Show Chat ENABLED (Read-Only)" : "Always Show Chat DISABLED");
	}

	private static void InitKillCooldownUI()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		if (!_killCooldownUIInitialized)
		{
			_cooldownBgTex = new Texture2D(1, 1);
			_cooldownBgTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.75f));
			_cooldownBgTex.Apply();
			GUIStyle val = new GUIStyle
			{
				fontSize = 13,
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4
			};
			val.normal.textColor = Color.white;
			_cooldownAboveHeadStyle = val;
			GUIStyle val2 = new GUIStyle
			{
				fontSize = 13,
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4
			};
			val2.normal.textColor = Color.black;
			_cooldownShadowStyle = val2;
			_killCooldownUIInitialized = true;
		}
	}

	private static Color GetKillCooldownColor(float killTimer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (killTimer < 2f)
		{
			return new Color(1f, 0.2f, 0.2f);
		}
		if (killTimer < 5f)
		{
			return new Color(1f, 0.6f, 0.2f);
		}
		return Color.white;
	}

	internal static bool AppendKillCooldownSuffix(PlayerControl player, StringBuilder sb)
	{
		object obj;
		if (player == null)
		{
			obj = null;
		}
		else
		{
			NetworkedPlayerInfo data = player.Data;
			obj = ((data != null) ? data.Role : null);
		}
		if ((Object)obj == (Object)null || !player.Data.Role.IsImpostor || player.Data.IsDead)
		{
			return false;
		}
		ConfigEntry<bool> showKillCooldowns = CheatConfig.ShowKillCooldowns;
		if (showKillCooldowns == null || !showKillCooldowns.Value)
		{
			return false;
		}
		AmongUsClient instance = AmongUsClient.Instance;
		if (instance == null || !((InnerNetClient)instance).IsGameStarted)
		{
			return false;
		}
		try
		{
			float killTimer = player.killTimer;
			string value = ((killTimer <= 0.1f) ? "FF2222" : ((killTimer < 2f) ? "FF4444" : ((!(killTimer < 5f)) ? "AAAAAA" : "FF9922")));
			sb.Append("\n<size=60%><color=#").Append(value).Append(">");
			if (killTimer <= 0.1f)
			{
				sb.Append("⚠ CAN KILL!");
			}
			else
			{
				sb.Append(killTimer.ToString("F1")).Append('s');
			}
			sb.Append("</color></size>");
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static string BuildKillCooldownSuffix(PlayerControl player)
	{
		StringBuilder killCooldownSb = _killCooldownSb;
		killCooldownSb.Clear();
		if (!AppendKillCooldownSuffix(player, killCooldownSb))
		{
			return null;
		}
		return killCooldownSb.ToString();
	}

	internal static void DrawKillCooldowns()
	{
		ConfigEntry<bool> showKillCooldowns = CheatConfig.ShowKillCooldowns;
		if (showKillCooldowns == null || !showKillCooldowns.Value || (Object)(object)PlayerControl.LocalPlayer == (Object)null)
		{
			return;
		}
		AmongUsClient instance = AmongUsClient.Instance;
		if (instance == null || !((InnerNetClient)instance).IsGameStarted || IsRevealSusActive)
		{
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		byte b = ((localPlayer != null) ? localPlayer.PlayerId : byte.MaxValue);
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			object obj;
			if (current == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo data = current.Data;
				obj = ((data != null) ? data.Role : null);
			}
			if ((Object)obj == (Object)null || !current.Data.Role.IsImpostor || current.Data.IsDead)
			{
				continue;
			}
			CosmeticsLayer cosmetics = current.cosmetics;
			if ((Object)(object)((cosmetics != null) ? cosmetics.nameText : null) == (Object)null)
			{
				continue;
			}
			if (current.PlayerId == b)
			{
				ConfigEntry<bool> customNameEnabled = MiscConfig.CustomNameEnabled;
				if (customNameEnabled != null && customNameEnabled.Value)
				{
					continue;
				}
			}
			if (MMCIdentification.IsMMCPlayer(current.PlayerId))
			{
				continue;
			}
			try
			{
				_killCooldownSb.Clear();
				_killCooldownSb.Append(current.Data.PlayerName);
				if (AppendKillCooldownSuffix(current, _killCooldownSb))
				{
					string text = _killCooldownSb.ToString();
					if (((TMP_Text)current.cosmetics.nameText).text != text)
					{
						((TMP_Text)current.cosmetics.nameText).text = text;
					}
				}
			}
			catch
			{
			}
		}
	}

	private static SpriteRenderer CreateDot(MapBehaviour map)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		SpriteRenderer herePoint = map.HerePoint;
		GameObject obj = Object.Instantiate<GameObject>(((Component)herePoint).gameObject, ((Component)herePoint).transform.parent);
		((Object)obj).name = "ModPlayerDot";
		obj.transform.localScale = Vector3.one;
		SpriteRenderer component = obj.GetComponent<SpriteRenderer>();
		((Renderer)component).material = new Material(((Renderer)herePoint).material);
		((Renderer)component).material.SetInt(PlayerMaterial.MaskLayer, 255);
		((Renderer)component).sortingOrder = 50;
		return component;
	}

	private static void EnsureKillAlertStyles()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		if (_killAlertTitleStyle == null || !((Object)(object)_killAlertBgTex != (Object)null))
		{
			_killAlertBgTex = MakeKillAlertTex(2, 2, new Color(0.05f, 0.02f, 0.02f, 0.92f));
			_killAlertAccentTex = MakeKillAlertTex(2, 2, new Color(1f, 0.05f, 0.05f, 1f));
			_killAlertShadowTex = MakeKillAlertTex(2, 2, new Color(0f, 0f, 0f, 0.5f));
			GUIStyle val = new GUIStyle
			{
				fontSize = 20,
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4,
				richText = true
			};
			val.normal.textColor = Color.white;
			_killAlertTitleStyle = val;
			GUIStyle val2 = new GUIStyle
			{
				fontSize = 13,
				alignment = (TextAnchor)4,
				richText = true
			};
			val2.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);
			_killAlertDetailStyle = val2;
			GUIStyle val3 = new GUIStyle();
			val3.normal.background = _killAlertBgTex;
			_killAlertBgStyle = val3;
			GUIStyle val4 = new GUIStyle();
			val4.normal.background = _killAlertAccentTex;
			_killAlertAccentStyle = val4;
			GUIStyle val5 = new GUIStyle();
			val5.normal.background = _killAlertShadowTex;
			_killAlertShadowStyle = val5;
		}
	}

	private static Texture2D MakeKillAlertTex(int w, int h, Color col)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(w, h);
		Color[] array = (Color[])(object)new Color[w * h];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = col;
		}
		val.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
		val.Apply();
		((Object)val).hideFlags = (HideFlags)61;
		return val;
	}

	public static void ShowKillAlert(PlayerControl killer, PlayerControl victim, string location)
	{
		ConfigEntry<bool> killAlertsEnabled = CheatConfig.KillAlertsEnabled;
		if (killAlertsEnabled != null && killAlertsEnabled.Value)
		{
			object obj;
			if (killer == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo data = killer.Data;
				obj = ((data != null) ? data.PlayerName : null);
			}
			if (obj == null)
			{
				obj = "???";
			}
			string text = (string)obj;
			object obj2;
			if (victim == null)
			{
				obj2 = null;
			}
			else
			{
				NetworkedPlayerInfo data2 = victim.Data;
				obj2 = ((data2 != null) ? data2.PlayerName : null);
			}
			if (obj2 == null)
			{
				obj2 = "???";
			}
			string text2 = (string)obj2;
			_killAlertStartTime = Time.time;
			_killAlertEndTime = Time.time + KILL_ALERT_DURATION;
			_killAlertTitle = "<color=#FF1A1A><size=24>☠</size></color>  KILL DETECTED  <color=#FF1A1A><size=24>☠</size></color>";
			_killAlertDetail = (string.IsNullOrEmpty(location) ? $"<color=#FF6666><b>{text}</b></color> <color=#949EAD>→</color> <color=#AAFFAA><b>{text2}</b></color>" : $"<color=#FF6666><b>{text}</b></color> <color=#949EAD>→</color> <color=#AAFFAA><b>{text2}</b></color>  <color=#6B7280>({location})</color>");
			LogCheat("☠ " + text + " KILLED " + text2 + (string.IsNullOrEmpty(location) ? "" : (" in " + location)));
		}
	}

	internal static void DrawKillAlert()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		if (!(Time.time > _killAlertEndTime) && !string.IsNullOrEmpty(_killAlertTitle))
		{
			EnsureKillAlertStyles();
			float num = Time.time - _killAlertStartTime;
			float num2 = _killAlertEndTime - Time.time;
			float num3 = Mathf.Clamp01(num / 0.3f);
			float num4 = 1f - Mathf.Pow(1f - num3, 3f);
			float num5 = ((num2 < 1.5f) ? Mathf.Clamp01(num2 / 1.5f) : 1f);
			float num6 = 0.85f + 0.15f * Mathf.Sin(Time.time * 4f);
			float scale = GuiStyles.Spacing.Scale;
			float num7 = (float)Screen.width / scale;
			Matrix4x4 matrix = GUI.matrix;
			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
			float num8 = 420f;
			float num9 = 64f;
			float num10 = (num7 - num8) / 2f;
			float num11 = Mathf.Lerp(0f - num9 - 10f, 60f, num4);
			Color color = GUI.color;
			GUI.color = new Color(1f, 1f, 1f, num5 * 0.5f);
			GUI.Box(new Rect(num10 + 3f, num11 + 3f, num8, num9), GUIContent.none, _killAlertShadowStyle);
			GUI.color = new Color(1f, 1f, 1f, num5);
			GUI.Box(new Rect(num10, num11, num8, num9), GUIContent.none, _killAlertBgStyle);
			GUI.color = new Color(1f, 1f, 1f, num5 * num6);
			GUI.Box(new Rect(num10, num11, num8, 3f), GUIContent.none, _killAlertAccentStyle);
			GUI.Box(new Rect(num10, num11 + num9 - 2f, num8, 2f), GUIContent.none, _killAlertAccentStyle);
			GUI.color = new Color(1f, 0.05f, 0.05f, num5 * num6 * 0.6f);
			GUI.Box(new Rect(num10, num11 + 3f, 3f, num9 - 5f), GUIContent.none, _killAlertAccentStyle);
			GUI.Box(new Rect(num10 + num8 - 3f, num11 + 3f, 3f, num9 - 5f), GUIContent.none, _killAlertAccentStyle);
			_killAlertTitleStyle.normal.textColor = new Color(1f, 1f, 1f, num5);
			GUI.Label(new Rect(num10, num11 + 4f, num8, 32f), _killAlertTitle, _killAlertTitleStyle);
			_killAlertDetailStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f, num5);
			GUI.Label(new Rect(num10, num11 + 34f, num8, 24f), _killAlertDetail, _killAlertDetailStyle);
			GUI.color = color;
			GUI.matrix = matrix;
		}
	}

	private static void InitEventLoggerStyles()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected O, but got Unknown
		if ((Object)(object)_eventLogBgTex == (Object)null)
		{
			_eventLogBgTex = new Texture2D(1, 1);
			_eventLogBgTex.SetPixel(0, 0, new Color(0.08f, 0.08f, 0.12f, 0.95f));
			_eventLogBgTex.Apply();
			_eventLogHeaderTex = new Texture2D(1, 1);
			_eventLogHeaderTex.SetPixel(0, 0, new Color(0.15f, 0.1f, 0.18f, 1f));
			_eventLogHeaderTex.Apply();
		}
		if (_eventLogHeaderStyle == null)
		{
			GUIStyle val = new GUIStyle
			{
				fontSize = 14,
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)3
			};
			val.normal.textColor = new Color(0.9f, 0.6f, 0.9f);
			_eventLogHeaderStyle = val;
			_eventLogHeaderStyle.padding.left = 10;
		}
		if (_eventLogEventStyle == null)
		{
			_eventLogEventStyle = new GUIStyle
			{
				fontSize = 12,
				alignment = (TextAnchor)3,
				wordWrap = false,
				clipping = (TextClipping)1
			};
			_eventLogEventStyle.padding.left = 5;
			_eventLogEventStyle.padding.right = 5;
			_eventLogEventStyle.padding.top = 2;
			_eventLogEventStyle.padding.bottom = 2;
		}
		if (_eventLogTimeStyle == null)
		{
			GUIStyle val2 = new GUIStyle
			{
				fontSize = 10,
				alignment = (TextAnchor)3
			};
			val2.normal.textColor = new Color(0.5f, 0.5f, 0.6f);
			_eventLogTimeStyle = val2;
			_eventLogTimeStyle.padding.left = 5;
		}
	}

	internal static void DrawEventLoggerUI()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Invalid comparison between Unknown and I4
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		ConfigEntry<bool> eventLoggerEnabled = CheatConfig.EventLoggerEnabled;
		if (eventLoggerEnabled == null || !eventLoggerEnabled.Value || !EventLogger.ShowUI)
		{
			return;
		}
		InitEventLoggerStyles();
		Event current = Event.current;
		Vector2 mousePosition = current.mousePosition;
		Rect val = default(Rect);
		(val)._002Ector((_eventLogRect).x, (_eventLogRect).y, (_eventLogRect).width - 70f, 28f);
		if ((int)current.type == 0 && (val).Contains(mousePosition))
		{
			_eventLogDragging = true;
			_eventLogDragOffset = mousePosition - new Vector2((_eventLogRect).x, (_eventLogRect).y);
		}
		else if ((int)current.type == 1)
		{
			_eventLogDragging = false;
		}
		else if ((int)current.type == 3 && _eventLogDragging)
		{
			(_eventLogRect).x = mousePosition.x - _eventLogDragOffset.x;
			(_eventLogRect).y = mousePosition.y - _eventLogDragOffset.y;
		}
		GUIStyle val2 = new GUIStyle();
		val2.normal.background = _eventLogBgTex;
		GUI.Box(_eventLogRect, GUIContent.none, val2);
		Rect val3 = new Rect((_eventLogRect).x, (_eventLogRect).y, (_eventLogRect).width, 28f);
		GUIStyle val4 = new GUIStyle();
		val4.normal.background = _eventLogHeaderTex;
		GUI.Box(val3, GUIContent.none, val4);
		GUI.Label(new Rect((_eventLogRect).x + 5f, (_eventLogRect).y + 2f, 150f, 24f), "EVENT LOG", _eventLogHeaderStyle);
		List<GameEvent> filteredEvents = EventLogger.GetFilteredEvents();
		int count = filteredEvents.Count;
		_eventLogTimeStyle.normal.textColor = new Color(0.5f, 0.5f, 0.6f);
		GUI.Label(new Rect((_eventLogRect).x + 120f, (_eventLogRect).y + 6f, 100f, 16f), count.ToString(), _eventLogTimeStyle);
		GUI.backgroundColor = new Color(0.6f, 0.2f, 0.2f);
		if (GUI.Button(new Rect((_eventLogRect).xMax - 28f, (_eventLogRect).y + 4f, 22f, 20f), "X"))
		{
			EventLogger.ShowUI = false;
		}
		GUI.backgroundColor = Color.white;
		GUI.backgroundColor = new Color(0.3f, 0.3f, 0.4f);
		if (GUI.Button(new Rect((_eventLogRect).xMax - 70f, (_eventLogRect).y + 4f, 38f, 20f), "CLR"))
		{
			EventLogger.Clear();
		}
		GUI.backgroundColor = Color.white;
		float num = (_eventLogRect).x + 3f;
		float num2 = (_eventLogRect).y + 32f;
		float num3 = (_eventLogRect).width - 6f;
		float num4 = (_eventLogRect).height - 38f;
		float num5 = Mathf.Max((float)count * 24f, num4);
		float num6 = num3 - 18f;
		_eventLogScrollPos = GUI.BeginScrollView(new Rect(num, num2, num3, num4), _eventLogScrollPos, new Rect(0f, 0f, num6, num5));
		int num7 = Mathf.Max(0, Mathf.FloorToInt(_eventLogScrollPos.y / 24f) - 1);
		int num8 = Mathf.Min(count, num7 + Mathf.CeilToInt(num4 / 24f) + 2);
		for (int i = num7; i < num8; i++)
		{
			int num9 = count - 1 - i;
			if (num9 >= 0 && num9 < count)
			{
				GameEvent gameEvent = filteredEvents[num9];
				float num10 = (float)i * 24f;
				if (i % 2 == 0)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.03f);
					GUI.Box(new Rect(0f, num10, num6, 24f), GUIContent.none);
					GUI.color = Color.white;
				}
				GUI.Label(new Rect(2f, num10, 40f, 24f), gameEvent.Time.ToString("HH:mm"), _eventLogTimeStyle);
				_eventLogEventStyle.normal.textColor = gameEvent.Color;
				GUI.Label(new Rect(45f, num10, num6 - 50f, 24f), GetEventIcon(gameEvent.Type) + gameEvent.Message, _eventLogEventStyle);
			}
		}
		GUI.EndScrollView();
	}

	private static string GetEventIcon(GameEventType type)
	{
		return type switch
		{
			GameEventType.Kill => "☠ ", 
			GameEventType.Vent => "\ud83d\udd32 ", 
			GameEventType.Report => "\ud83d\udce2 ", 
			GameEventType.Vote => "✋ ", 
			GameEventType.Sabotage => "⚠ ", 
			_ => "• ", 
		};
	}

	internal static void ToggleEventLoggerUI()
	{
		EventLogger.ShowUI = !EventLogger.ShowUI;
		LogCheat(EventLogger.ShowUI ? "Event Logger UI opened" : "Event Logger UI closed");
	}

	internal static void OpenSabotageMap()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		try
		{
			if ((Object)(object)DestroyableSingleton<HudManager>.Instance == (Object)null)
			{
				Debug.LogWarning(Object.op_Implicit("[OpenSabotageMap] HudManager.Instance is null"));
				return;
			}
			if ((Object)(object)ShipStatus.Instance == (Object)null)
			{
				Debug.LogWarning(Object.op_Implicit("[OpenSabotageMap] ShipStatus.Instance is null - not in game"));
				return;
			}
			DestroyableSingleton<HudManager>.Instance.InitMap();
			MapOptions val = new MapOptions
			{
				Mode = (Modes)3
			};
			DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(val);
			LogCheat("Sabotage map opened!");
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[OpenSabotageMap] Error: {value}"));
		}
	}

	private static void RevealAnonymousVoteColors(Transform targetTransform, int voterColorId)
	{
		try
		{
			VoteSpreader componentInChildren = ((Component)targetTransform).GetComponentInChildren<VoteSpreader>();
			if (((componentInChildren != null) ? componentInChildren.Votes : null) != null && componentInChildren.Votes.Count != 0)
			{
				SpriteRenderer val = componentInChildren.Votes[componentInChildren.Votes.Count - 1];
				if ((Object)(object)val != (Object)null)
				{
					PlayerMaterial.SetColors(voterColorId, (Renderer)(object)val);
				}
			}
		}
		catch
		{
		}
	}

	private static bool LocalPlayerCanKill()
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
			return false;
		}
		if (localPlayer.Data.Role.IsImpostor)
		{
			return true;
		}
		try
		{
			HudManager instance = DestroyableSingleton<HudManager>.Instance;
			KillButton val = ((instance != null) ? instance.KillButton : null);
			if ((Object)(object)val != (Object)null && ((Behaviour)val).isActiveAndEnabled)
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	[IteratorStateMachine(typeof(_003C_StaggeredDespawnCoroutine_003Ed__671))]
	private static IEnumerator _StaggeredDespawnCoroutine(List<uint> netIds)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003C_StaggeredDespawnCoroutine_003Ed__671(0)
		{
			netIds = netIds
		};
	}

	private static void _SendDespawnDirect(AmongUsClient client, uint netId, int target)
	{
		MessageWriter val = ((InnerNetClient)client).StartRpcImmediately(uint.MaxValue, (byte)0, (SendOption)1, target);
		val.EndMessage();
		val.StartMessage((byte)5);
		val.WritePacked(netId);
		val.EndMessage();
		val.StartMessage((byte)2);
		val.WritePacked(uint.MaxValue);
		val.Write((byte)0);
		((InnerNetClient)client).FinishRpcImmediately(val);
	}

	private static bool ShouldAllowGameEnd()
	{
		try
		{
			return (Object)(object)MeetingHud.Instance != (Object)null || (Object)(object)ExileController.Instance != (Object)null;
		}
		catch
		{
			return false;
		}
	}

	internal static void ForceVotesRefreshPlayerList()
	{
		ForceVotesAlivePlayers.Clear();
		MeetingHud instance = MeetingHud.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		foreach (PlayerVoteArea item in (Il2CppArrayBase<PlayerVoteArea>)(object)instance.playerStates)
		{
			if ((Object)(object)item == (Object)null || item.AmDead)
			{
				continue;
			}
			string name = "Player " + item.TargetPlayerId;
			Enumerator<PlayerControl> enumerator2 = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				PlayerControl current2 = enumerator2.Current;
				if ((Object)(object)current2 != (Object)null && current2.PlayerId == item.TargetPlayerId && (Object)(object)current2.Data != (Object)null)
				{
					name = current2.Data.PlayerName;
					break;
				}
			}
			ForceVotesAlivePlayers.Add(new ForceVotesPlayerInfo
			{
				PlayerId = item.TargetPlayerId,
				Name = name
			});
		}
		Debug.Log(Object.op_Implicit($"[ForceVotes] RefreshPlayerList: Found {ForceVotesAlivePlayers.Count} alive players"));
	}

	internal static void ForceAllVotesTo(byte targetPlayerId)
	{
		if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
		{
			Debug.Log(Object.op_Implicit("[ForceVotes] Must be host to force votes"));
			return;
		}
		MeetingHud instance = MeetingHud.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			Debug.Log(Object.op_Implicit("[ForceVotes] No active meeting"));
			return;
		}
		Debug.Log(Object.op_Implicit($"[ForceVotes] Attempting to force votes to target {targetPlayerId}"));
		int num = 0;
		foreach (PlayerVoteArea item in (Il2CppArrayBase<PlayerVoteArea>)(object)instance.playerStates)
		{
			if ((Object)(object)item == (Object)null)
			{
				continue;
			}
			byte targetPlayerId2 = item.TargetPlayerId;
			bool amDead = item.AmDead;
			sbyte b = (sbyte)item.VotedFor;
			byte votedFor = item.VotedFor;
			Debug.Log(Object.op_Implicit($"[ForceVotes] Player {targetPlayerId2}: VotedFor(sbyte)={b}, VotedFor(byte)={votedFor}, AmDead={amDead}, DidVote={item.DidVote}"));
			bool flag = !item.DidVote || b == -1 || votedFor == byte.MaxValue;
			if (!(!amDead && flag))
			{
				continue;
			}
			Debug.Log(Object.op_Implicit($"[ForceVotes] Forcing vote from {targetPlayerId2} to {targetPlayerId}"));
			try
			{
				instance.CmdCastVote(targetPlayerId2, targetPlayerId);
				num++;
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[ForceVotes] CmdCastVote failed: {value}"));
				try
				{
					ForceVoteViaRPC(instance, targetPlayerId2, targetPlayerId);
					num++;
				}
				catch (Exception value2)
				{
					Debug.LogError(Object.op_Implicit($"[ForceVotes] RPC fallback also failed: {value2}"));
				}
			}
		}
		Debug.Log(Object.op_Implicit($"[ForceVotes] Forced {num} votes to target {targetPlayerId}"));
	}

	internal static void ForceVoteViaRPC(MeetingHud meeting, byte voterId, byte targetPlayerId)
	{
		MessageWriter val = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)meeting).NetId, (byte)((ServerData.Config.RpcCastVote > 0) ? ServerData.Config.RpcCastVote : 24), (SendOption)1, -1);
		val.Write(voterId);
		val.Write(targetPlayerId);
		((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val);
	}

	internal static void ForceAllVotesToSkip()
	{
		ForceAllVotesTo(253);
	}

	internal static int ForceVotesCountRemaining()
	{
		MeetingHud instance = MeetingHud.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return 0;
		}
		int num = 0;
		foreach (PlayerVoteArea item in (Il2CppArrayBase<PlayerVoteArea>)(object)instance.playerStates)
		{
			if (!((Object)(object)item == (Object)null) && !item.AmDead && !item.DidVote)
			{
				num++;
			}
		}
		return num;
	}

	internal static string ForceVotesGetCurrentTargetName()
	{
		if (ForceVotesSelectedTargetIndex == 0)
		{
			return "Skip Vote";
		}
		if (ForceVotesSelectedTargetIndex - 1 < ForceVotesAlivePlayers.Count)
		{
			return ForceVotesAlivePlayers[ForceVotesSelectedTargetIndex - 1].Name;
		}
		ForceVotesSelectedTargetIndex = 0;
		return "Skip Vote";
	}

	internal static void ForceVotesExecute()
	{
		if (ForceVotesSelectedTargetIndex == 0)
		{
			ForceAllVotesToSkip();
			return;
		}
		int num = ForceVotesSelectedTargetIndex - 1;
		if (num < ForceVotesAlivePlayers.Count)
		{
			byte playerId = ForceVotesAlivePlayers[num].PlayerId;
			Debug.Log(Object.op_Implicit($"[ForceVotes] Forcing to player ID: {playerId}"));
			ForceAllVotesTo(playerId);
		}
	}

	internal static void ForceVotesSelectPrevious()
	{
		ForceVotesSelectedTargetIndex--;
		if (ForceVotesSelectedTargetIndex < 0)
		{
			ForceVotesSelectedTargetIndex = ForceVotesAlivePlayers.Count;
		}
	}

	internal static void ForceVotesSelectNext()
	{
		ForceVotesSelectedTargetIndex++;
		if (ForceVotesSelectedTargetIndex > ForceVotesAlivePlayers.Count)
		{
			ForceVotesSelectedTargetIndex = 0;
		}
	}

	internal static void ToggleRadarDebug()
	{
		_radarDebugEnabled = !_radarDebugEnabled;
		if (_radarDebugEnabled)
		{
			_roomBoundsCache.Clear();
			Debug.Log(Object.op_Implicit("[RadarDebug] ENABLED - Printing coordinates every 0.5s. Type DISABLERADARDEBUG to stop."));
		}
		else
		{
			Debug.Log(Object.op_Implicit("[RadarDebug] DISABLED"));
		}
	}

	internal static void DisableRadarDebug()
	{
		_radarDebugEnabled = false;
		Debug.Log(Object.op_Implicit("[RadarDebug] DISABLED"));
	}

	internal static void UpdateRadarDebug()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (!_radarDebugEnabled || (Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)ShipStatus.Instance == (Object)null)
		{
			return;
		}
		_radarDebugTimer += Time.deltaTime;
		if (!(_radarDebugTimer < RADAR_DEBUG_INTERVAL))
		{
			_radarDebugTimer = 0f;
			_roomCacheTime += Time.deltaTime;
			if (_roomCacheTime >= ROOM_CACHE_DURATION || _roomBoundsCache.Count == 0)
			{
				_roomCacheTime = 0f;
				RefreshRoomBoundsCache();
			}
			Vector3 position = ((Component)PlayerControl.LocalPlayer).transform.position;
			float value = (float)Math.Round(position.x, 2);
			float value2 = (float)Math.Round(position.y, 2);
			string playerRoomName = GetPlayerRoomName(position);
			string currentMapName = GetCurrentMapName();
			MapBounds currentMapBounds = GetCurrentMapBounds();
			Debug.Log(Object.op_Implicit($"[RadarDebug] {currentMapName} | X: {value:F2} Y: {value2:F2} | ROOM: {playerRoomName} | BOUNDS: minX={currentMapBounds.minX:F1} maxX={currentMapBounds.maxX:F1} minY={currentMapBounds.minY:F1} maxY={currentMapBounds.maxY:F1}"));
		}
	}

	private static void RefreshRoomBoundsCache()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		_roomBoundsCache.Clear();
		ShipStatus instance = ShipStatus.Instance;
		if (((instance != null) ? instance.AllRooms : null) == null)
		{
			return;
		}
		foreach (PlainShipRoom item in (Il2CppArrayBase<PlainShipRoom>)(object)ShipStatus.Instance.AllRooms)
		{
			if (!((Object)(object)item == (Object)null) && (int)item.RoomId != 0 && (int)item.RoomId != 41)
			{
				Vector3 position = ((Component)item).transform.position;
				Vector2 val = Vector2.zero;
				if ((Object)(object)item.roomArea != (Object)null)
				{
					Bounds bounds = item.roomArea.bounds;
					val = Vector2.op_Implicit((bounds).size);
				}
				_roomBoundsCache[item.RoomId] = (position, val);
				Debug.Log(Object.op_Implicit($"[RadarDebug] Room cached: {item.RoomId} at ({position.x:F2}, {position.y:F2}) size ({val.x:F2}, {val.y:F2})"));
			}
		}
	}

	private static string GetPlayerRoomName(Vector3 playerPos)
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		ShipStatus instance = ShipStatus.Instance;
		if (((instance != null) ? instance.AllRooms : null) == null)
		{
			return "NO_ROOMS";
		}
		SystemTypes val;
		foreach (PlainShipRoom item3 in (Il2CppArrayBase<PlainShipRoom>)(object)ShipStatus.Instance.AllRooms)
		{
			if ((Object)(object)item3 != (Object)null && (Object)(object)item3.roomArea != (Object)null && item3.roomArea.OverlapPoint(Vector2.op_Implicit(playerPos)))
			{
				val = item3.RoomId;
				return val.ToString();
			}
		}
		try
		{
			HudManager instance2 = DestroyableSingleton<HudManager>.Instance;
			object obj;
			if (instance2 == null)
			{
				obj = null;
			}
			else
			{
				RoomTracker roomTracker = instance2.roomTracker;
				obj = ((roomTracker != null) ? roomTracker.LastRoom : null);
			}
			if ((Object)obj != (Object)null)
			{
				val = DestroyableSingleton<HudManager>.Instance.roomTracker.LastRoom.RoomId;
				string text = val.ToString();
				if (!string.IsNullOrEmpty(text) && text != "Invalid")
				{
					return text;
				}
			}
		}
		catch
		{
		}
		foreach (KeyValuePair<SystemTypes, (Vector3, Vector2)> item4 in _roomBoundsCache)
		{
			(Vector3, Vector2) value = item4.Value;
			Vector3 item = value.Item1;
			Vector2 item2 = value.Item2;
			float num = item2.x / 2f;
			float num2 = item2.y / 2f;
			if (playerPos.x >= item.x - num && playerPos.x <= item.x + num && playerPos.y >= item.y - num2 && playerPos.y <= item.y + num2)
			{
				val = item4.Key;
				return val.ToString();
			}
		}
		return "CORRIDOR/OUTSIDE";
	}

	private static string GetCurrentMapName()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ShipStatus.Instance == (Object)null)
		{
			return "UNKNOWN";
		}
		int type = (int)ShipStatus.Instance.Type;
		if ((int)type == 0)
		{
			return "SKELD";
		}
		if ((int)type == 1)
		{
			return "MIRA_HQ";
		}
		if ((int)type == 2)
		{
			return "POLUS";
		}
		if ((int)type == 3)
		{
			return "FUNGLE";
		}
		ShipStatus instance = ShipStatus.Instance;
		if (((instance != null) ? instance.AllRooms : null) != null)
		{
			foreach (PlainShipRoom item in (Il2CppArrayBase<PlainShipRoom>)(object)ShipStatus.Instance.AllRooms)
			{
				if (!((Object)(object)item == (Object)null))
				{
					SystemTypes roomId = item.RoomId;
					switch (roomId.ToString())
					{
					case "Cockpit":
					case "Armory":
					case "VaultRoom":
					case "GapRoom":
					case "MainHall":
					case "Showers":
					case "Records":
					case "Lounge":
					case "Ventilation":
					case "CargoBay":
					case "Electrical":
					case "Medical":
					case "HallOfPortraits":
					case "ViewingDeck":
						return "AIRSHIP";
					}
				}
			}
		}
		return type.ToString();
	}

	private static MapBounds GetCurrentMapBounds()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ShipStatus.Instance == (Object)null)
		{
			return default(MapBounds);
		}
		if (MapInfos.TryGetValue(ShipStatus.Instance.Type, out var value))
		{
			MapBounds result = default(MapBounds);
			result.minX = value.minX;
			result.maxX = value.maxX;
			result.minY = value.minY;
			result.maxY = value.maxY;
			return result;
		}
		return default(MapBounds);
	}

	internal static void DumpRoomBounds()
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_059a: Unknown result type (might be due to invalid IL or missing references)
		//IL_059f: Unknown result type (might be due to invalid IL or missing references)
		//IL_078f: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_0631: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_0679: Unknown result type (might be due to invalid IL or missing references)
		//IL_069d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0709: Unknown result type (might be due to invalid IL or missing references)
		//IL_072d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ShipStatus.Instance == (Object)null)
		{
			Debug.Log(Object.op_Implicit("[RoomDump] ERROR: Not in a game (ShipStatus is null)"));
			return;
		}
		Il2CppReferenceArray<PlainShipRoom> allRooms = ShipStatus.Instance.AllRooms;
		if (allRooms == null || ((Il2CppArrayBase<PlainShipRoom>)(object)allRooms).Length == 0)
		{
			Debug.Log(Object.op_Implicit("[RoomDump] ERROR: No rooms found"));
			return;
		}
		string currentMapName = GetCurrentMapName();
		Debug.Log(Object.op_Implicit("[RoomDump] ═══════════════════════════════════════════════════════════"));
		Debug.Log(Object.op_Implicit($"[RoomDump] MAP: {currentMapName} — {((Il2CppArrayBase<PlainShipRoom>)(object)allRooms).Length} rooms found"));
		Debug.Log(Object.op_Implicit("[RoomDump] Format: new RoomData(centerX, centerY, width, height, SHAPE)"));
		Debug.Log(Object.op_Implicit("[RoomDump] ═══════════════════════════════════════════════════════════"));
		Debug.Log(Object.op_Implicit("[RoomDump] // ===== " + currentMapName + " — Auto-dumped from game engine ====="));
		int num = 0;
		int num2 = 0;
		SystemTypes roomId;
		for (int i = 0; i < ((Il2CppArrayBase<PlainShipRoom>)(object)allRooms).Length; i++)
		{
			PlainShipRoom val = ((Il2CppArrayBase<PlainShipRoom>)(object)allRooms)[i];
			if (!((Object)(object)val == (Object)null) && (int)val.RoomId != 0)
			{
				roomId = val.RoomId;
				string value = roomId.ToString();
				Vector3 position = ((Component)val).transform.position;
				float x = position.x;
				float y = position.y;
				float value2 = 5f;
				float value3 = 5f;
				if ((Object)(object)val.roomArea != (Object)null)
				{
					Bounds bounds = val.roomArea.bounds;
					x = (bounds).center.x;
					y = (bounds).center.y;
					value2 = (bounds).size.x;
					value3 = (bounds).size.y;
				}
				Debug.Log(Object.op_Implicit($"[RoomDump] // {value} — center ({x:F2}, {y:F2}) size ({value2:F2} x {value3:F2})"));
				Debug.Log(Object.op_Implicit($"[RoomDump] new RoomData({x:F2}f, {y:F2}f, {value2:F2}f, {value3:F2}f, SHAPE_RECT),"));
				num++;
			}
		}
		for (int j = 0; j < ((Il2CppArrayBase<PlainShipRoom>)(object)allRooms).Length; j++)
		{
			PlainShipRoom val2 = ((Il2CppArrayBase<PlainShipRoom>)(object)allRooms)[j];
			if (!((Object)(object)val2 == (Object)null) && (int)val2.RoomId == 0)
			{
				Vector3 position2 = ((Component)val2).transform.position;
				float x2 = position2.x;
				float y2 = position2.y;
				float num3 = 4f;
				float num4 = 2f;
				if ((Object)(object)val2.roomArea != (Object)null)
				{
					Bounds bounds2 = val2.roomArea.bounds;
					x2 = (bounds2).center.x;
					y2 = (bounds2).center.y;
					num3 = (bounds2).size.x;
					num4 = (bounds2).size.y;
				}
				Debug.Log(Object.op_Implicit($"[RoomDump] // Hallway #{num2} — ({x2:F2}, {y2:F2}) size ({num3:F2} x {num4:F2}) {((num4 > num3) ? "VERT" : "HORIZ")}"));
				Debug.Log(Object.op_Implicit($"[RoomDump] new RoomData({x2:F2}f, {y2:F2}f, {num3:F2}f, {num4:F2}f, false, true),"));
				num2++;
			}
		}
		Debug.Log(Object.op_Implicit($"[RoomDump] TOTAL: {num} rooms + {num2} hallways"));
		Debug.Log(Object.op_Implicit("[RoomDump] ── RAW COLLIDER DATA ──"));
		for (int k = 0; k < ((Il2CppArrayBase<PlainShipRoom>)(object)allRooms).Length; k++)
		{
			PlainShipRoom val3 = ((Il2CppArrayBase<PlainShipRoom>)(object)allRooms)[k];
			if (!((Object)(object)val3 == (Object)null))
			{
				roomId = val3.RoomId;
				string value4 = roomId.ToString();
				Vector3 position3 = ((Component)val3).transform.position;
				if ((Object)(object)val3.roomArea != (Object)null)
				{
					Bounds bounds3 = val3.roomArea.bounds;
					Debug.Log(Object.op_Implicit($"[RoomDump] {value4,-20} pos=({position3.x:F3},{position3.y:F3}) center=({(bounds3).center.x:F3},{(bounds3).center.y:F3}) min=({(bounds3).min.x:F3},{(bounds3).min.y:F3}) max=({(bounds3).max.x:F3},{(bounds3).max.y:F3}) size=({(bounds3).size.x:F3},{(bounds3).size.y:F3})"));
				}
				else
				{
					Debug.Log(Object.op_Implicit($"[RoomDump] {value4,-20} pos=({position3.x:F3},{position3.y:F3}) NO_COLLIDER"));
				}
			}
		}
		Debug.Log(Object.op_Implicit("[RoomDump] ═══════════════════════════════════════════════════════════"));
	}

	private static void CheckLobbyChange()
	{
		int num = 0;
		bool flag = false;
		try
		{
			AmongUsClient instance = AmongUsClient.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				num = ((InnerNetClient)instance).GameId;
				flag = ((InnerNetClient)instance).AmConnected;
			}
		}
		catch
		{
		}
		if (_wasConnected && !flag)
		{
			ResetSyncState();
			_lastKnownGameId = 0;
			_aU = 0f;
			_bU = 0f;
		}
		else if (num != _lastKnownGameId)
		{
			if (_lastKnownGameId != 0)
			{
				ResetSyncState();
				_aU = 0f;
				_bU = 0f;
			}
			_lastKnownGameId = num;
		}
		_wasConnected = flag;
	}

	internal static void ResetSyncState()
	{
		try
		{
			RpcSyncValidator.Reset();
		}
		catch
		{
		}
		try
		{
			AutoKickChatWordPatch.Reset();
		}
		catch
		{
		}
		try
		{
			GhostUI.ResetTextInputState();
		}
		catch
		{
		}
		_chActive = false;
		_chStart = 0f;
		_chOwnerId = -1;
		_chHostPid = byte.MaxValue;
		_v7 = 0f;
		_v8 = 0;
		_v6 = 0f;
		_clActive = false;
		_clStart = 0f;
		_vA = 0f;
		_vB = 0;
		_v9 = 0f;
		_clPC = 0;
		_fakeCdActive = false;
		_fakeCdCurrent = 127;
		_fakeCdLastTick = 0f;
		ResetRainbow();
		_isImpersonating = false;
		_impTimer = 0f;
		_aT.Clear();
		_aS.Clear();
		_aP.Clear();
		_bT.Clear();
		_bS.Clear();
		_allMapNetIds.Clear();
		_allMapsActive = false;
		_selMapNetIds = null;
		_selMapSpawnId = -1;
		_dummyActive = false;
		_dummyNetIdsList.Clear();
		_dummySynced.Clear();
		_cfActive = false;
		_cfStates.Clear();
		_cfSynced.Clear();
		_cfSpawnQueue.Clear();
		_cfReconcilePending.Clear();
		_mapActive = false;
		_mapNetIds = null;
		LabRpcArbiter.ResetState();
		_localDestroyedNetIds.Clear();
		_trackedVanillaNetIdCnt = 0u;
		_ghostNetIdCounter = _GhostNetIdBase;
		_ghostCeilingWarned = false;
		_RE();
		if (_specTarget != byte.MaxValue)
		{
			_SpecDeactivate();
		}
		try
		{
			PlayerPickMenu.ResetHashForReconnect();
		}
		catch
		{
		}
		PlayerPickMenu.TriggerRealtimeUpdate(force: true);
	}

	internal static void _RB()
	{
		float num = Time.deltaTime;
		if (num <= 0f)
		{
			num = 0.005f;
		}
		float num2 = 0f;
		try
		{
			AmongUsClient instance = AmongUsClient.Instance;
			if ((Object)(object)instance != (Object)null && ((InnerNetClient)instance).AmConnected)
			{
				num2 = ((InnerNetClient)instance).Ping;
			}
		}
		catch
		{
		}
		if (_v0 <= 0f && num2 > 0f)
		{
			_v0 = num2;
		}
		else
		{
			_v0 = ((num2 > _v0) ? (_v0 * 0.4f + num2 * 0.6f) : (_v0 * 0.5f + num2 * 0.5f));
		}
		bool flag = _v1 > 0f;
		if (!flag && _AR_PAUSE > 0f && _v0 > _AR_PAUSE && Time.time > _v2)
		{
			_v1 = 1f;
			_vO = 0f;
			flag = true;
		}
		if (flag)
		{
			_v3 += num;
			if (_v0 < _AR_RECOVERY)
			{
				_vO += num;
			}
			else
			{
				_vO = 0f;
			}
			if (_vO >= _AR_RECOVERY_HOLD)
			{
				_v1 = 0f;
				_vO = 0f;
				_v2 = Time.time + _AR_GRACE;
				_vN = true;
				flag = false;
			}
		}
		else
		{
			_vN = false;
		}
		bool chActive = _chActive;
		bool clActive = _clActive;
		bool flag2 = _aT.Count > 0;
		bool flag3 = _bT.Count > 0;
		int num3 = (chActive ? 1 : 0) + (clActive ? 1 : 0) + (flag2 ? 1 : 0) + (flag3 ? 1 : 0);
		if (num3 == 0 || flag)
		{
			_vF = 0f;
			_vG = 0f;
			_vH = 0f;
			_vI = 0f;
			return;
		}
		float num4 = num2 - _vJ;
		_vK = _vK * 0.7f + num4 * 0.3f;
		_vJ = num2;
		if (_vK > 200f && num2 > 1700f && num3 >= 2 && Time.time >= _vM)
		{
			_vM = Time.time + 2f;
		}
		float num5 = ((Time.time < _vM) ? 0.8f : 1f);
		float num6 = 1f;
		float aR_PAUSE = _AR_PAUSE;
		float aR_RAMP_START = _AR_RAMP_START;
		if (aR_PAUSE > 0f && aR_RAMP_START > 0f && aR_RAMP_START < aR_PAUSE)
		{
			float num7 = 0.15f;
			if (_v0 > aR_RAMP_START && _v0 < aR_PAUSE)
			{
				float num8 = (_v0 - aR_RAMP_START) / (aR_PAUSE - aR_RAMP_START);
				if (num8 < 0f)
				{
					num8 = 0f;
				}
				else if (num8 > 1f)
				{
					num8 = 1f;
				}
				num6 = 1f - num8 * (1f - num7);
			}
			else if (_v0 >= aR_PAUSE)
			{
				num6 = num7;
			}
		}
		float num9 = _AR_CEIL * num5 * num6;
		int num10 = 1;
		if (clActive)
		{
			int num11 = 0;
			try
			{
				Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PlayerControl current = enumerator.Current;
					if ((Object)(object)current != (Object)null && !((InnerNetObject)current).AmOwner && (Object)(object)current.Data != (Object)null && !current.Data.Disconnected)
					{
						num11++;
					}
				}
			}
			catch
			{
			}
			num10 = Math.Max(1, num11);
		}
		int num12 = (clActive ? num10 : 0);
		int num13 = (chActive ? 1 : 0);
		int num14 = (flag2 ? 1 : 0);
		int num15 = (flag3 ? 1 : 0);
		int num16 = num12 + num13 + num14 + num15;
		if (num16 == 0)
		{
			_vF = 0f;
			_vG = 0f;
			_vH = 0f;
			_vI = 0f;
			return;
		}
		float num17 = (clActive ? (num9 * (float)num12 / (float)num16) : 0f);
		float num18 = (chActive ? (num9 * (float)num13 / (float)num16) : 0f);
		float num19 = (flag2 ? (num9 * (float)num14 / (float)num16) : 0f);
		float num20 = (flag3 ? (num9 * (float)num15 / (float)num16) : 0f);
		float num21 = Math.Min(_AR_CEIL, _AR_MAXDIV / (float)num10);
		if (num17 > num21)
		{
			float num22 = num17 - num21;
			num17 = num21;
			int num23 = (chActive ? 1 : 0) + (flag2 ? 1 : 0) + (flag3 ? 1 : 0);
			if (num23 > 0)
			{
				float num24 = num22 / (float)num23;
				if (chActive)
				{
					num18 += num24;
				}
				if (flag2)
				{
					num19 += num24;
				}
				if (flag3)
				{
					num20 += num24;
				}
			}
		}
		_vG = num17 * num;
		_vF = num18 * num;
		_vH = num19 * num;
		_vI = num20 * num;
	}

	private static void _GU()
	{
		ServerData.SecurityConfig config = ServerData.Config;
		int num = (config.Z4 ?? "").Length + (config.Z6 ?? "").Length * 31 + (config.Z8 ?? "").Length * 97 + (config.Z10 ?? "").Length * 127;
		if (num != _gsV)
		{
			_gsV = num;
			_gsC[0][0] = config.Z4;
			_gsC[0][1] = config.Z5;
			_gsC[1][0] = config.Z6;
			_gsC[1][1] = config.Z7;
			_gsC[2][0] = config.Z8;
			_gsC[2][1] = config.Z9;
			_gsC[3][0] = config.Z10;
			_gsC[3][1] = config.Z10;
		}
	}

	internal static void _RE()
	{
		if (!_chActive && !_clActive && _aT.Count == 0 && _bT.Count == 0)
		{
			_v0 = 0f;
			_v1 = 0f;
			_v2 = 0f;
			_v3 = 0f;
			_vK = 0f;
			_vM = 0f;
			_vJ = 0f;
			_vO = 0f;
		}
		_v4 = 0;
		_mH.Clear();
		_mK.Clear();
		_mV.Clear();
		_mN.Clear();
	}

	internal static void _PS(AmongUsClient client, uint controlNetId, int targetOwnerId)
	{
		_v5++;
		MessageWriter val = ((InnerNetClient)client).StartRpcImmediately(controlNetId, ServerData.Config.X5, (SendOption)1, targetOwnerId);
		val.Write(ServerData.Config.Z1);
		val.Write(_v5);
		((InnerNetClient)client).FinishRpcImmediately(val);
	}

	internal static int _AU(int targetCount)
	{
		if (_v1 > 0f)
		{
			return 0;
		}
		int num = Math.Max(1, targetCount);
		if (_vN)
		{
			return (int)Math.Min(500f, 3000f / (float)num);
		}
		float num2 = Math.Min(_AR_CEIL, _AR_MAXDIV / (float)num);
		float num3 = Math.Min(800f, _AR_MINDIV / (float)num);
		float num4 = ((_v0 < 200f) ? num2 : ((_v0 < 400f) ? (num2 * 0.9f) : ((_v0 < 600f) ? (num2 * 0.75f) : ((!(_v0 < _AR_PAUSE)) ? num3 : Math.Max(num3, num2 * 0.5f)))));
		return Math.Max((int)num3, (int)num4);
	}

	internal static int _QM(AmongUsClient client, uint controlNetId, uint physicsNetId, Vector2 pos, int targetOwnerId, byte sidKey, int packetCount, int f1 = 0, int f2 = 0, bool f3 = false)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		byte x = ServerData.Config.X0;
		int num2 = 0;
		if (_L4 > 0)
		{
			_r0[num2] = _L4;
			_s0[num2] = _mH;
			num2++;
		}
		if (_L5 > 0)
		{
			_r0[num2] = _L5;
			_s0[num2] = _mK;
			num2++;
		}
		if (_L6 > 0)
		{
			_r0[num2] = _L6;
			_s0[num2] = _mV;
			num2++;
		}
		if (_L7 > 0)
		{
			_r0[num2] = _L7;
			_s0[num2] = _mN;
			num2++;
		}
		if (num2 == 0)
		{
			return 0;
		}
		_GU();
		Vector2 val = default(Vector2);
		(val)._002Ector(pos.x + 0.5f, pos.y);
		float z = ServerData.Config.Z2;
		float z2 = ServerData.Config.Z3;
		int num3 = Math.Min(10, Math.Max(2, f1 + f2));
		int num4 = Math.Max(1, f1);
		for (int i = 0; i < packetCount; i++)
		{
			int num5 = i % num2;
			byte b = NextSid(_s0[num5], sidKey);
			MessageWriter val2 = ((InnerNetClient)client).StartRpcImmediately(controlNetId, _r0[num5], (SendOption)0, targetOwnerId);
			val2.Write(_gsC[num5 % _gsC.Length][(i + num5) % 2]);
			val2.Write(b);
			val2.EndMessage();
			for (int j = 1; j < num3; j++)
			{
				if (j >= num4)
				{
					float num6 = (f3 ? z2 : ((_v4 % 4 == 0) ? z : z2));
					float num7 = (((j + i) % 4 < 2) ? num6 : (0f - num6)) + (float)((j * 7 + i) % 11) * 0.35f;
					float num8 = (((j + i) % 3 == 0) ? num6 : (0f - num6)) + (float)((j * 5 + i) % 9) * 0.35f;
					_v4++;
					val2.StartMessage((byte)2);
					val2.WritePacked(physicsNetId);
					val2.Write(x);
					NetHelpers.WriteVector2(new Vector2(pos.x + num7, pos.y + num8), val2);
					NetHelpers.WriteVector2(val, val2);
				}
				else
				{
					int num9 = j % num2;
					byte b2 = NextSid(_s0[num9], sidKey);
					val2.StartMessage((byte)2);
					val2.WritePacked(controlNetId);
					val2.Write(_r0[num9]);
					val2.Write(_gsC[num9 % _gsC.Length][(i + j) % 2]);
					val2.Write(b2);
				}
				if (j < num3 - 1)
				{
					val2.EndMessage();
				}
			}
			((InnerNetClient)client).FinishRpcImmediately(val2);
			num += num3;
		}
		return num;
	}

	internal static float _GP()
	{
		return _v0;
	}

	internal static float _GT()
	{
		return _v3;
	}

	internal static bool _IP()
	{
		return _v1 > 0f;
	}

	internal static float _GR()
	{
		if (!(_v1 > 0f))
		{
			return 0f;
		}
		return Math.Max(0f, _AR_RECOVERY_HOLD - _vO);
	}

	internal static bool IsCHActive()
	{
		return _chActive;
	}

	internal static float GetCHRemaining()
	{
		if (!_chActive)
		{
			return 0f;
		}
		float num = Time.time - _chStart - _v6;
		return Mathf.Max(0f, _CH_DUR - num);
	}

	internal static byte GetAREngineState()
	{
		if (!_chActive && !_clActive && _aT.Count <= 0 && _bT.Count <= 0)
		{
			return 0;
		}
		if (_v1 > 0f)
		{
			return 4;
		}
		if (Time.time < _v2)
		{
			return 5;
		}
		float aR_PAUSE = _AR_PAUSE;
		float aR_RAMP_START = _AR_RAMP_START;
		if (aR_PAUSE <= 0f || aR_RAMP_START <= 0f || aR_RAMP_START >= aR_PAUSE)
		{
			return 1;
		}
		float v = _v0;
		float num = (aR_RAMP_START + aR_PAUSE) * 0.5f;
		if (v >= num)
		{
			return 3;
		}
		if (v >= aR_RAMP_START)
		{
			return 2;
		}
		return 1;
	}

	internal static int GetAREnginePingBucket()
	{
		int num = (int)_v0;
		if (num < 0)
		{
			num = 0;
		}
		return num / 200 * 200;
	}

	internal static void ToggleCH()
	{
		if (!IntegrityGuard.IsIntact)
		{
			_chActive = false;
		}
		else if (!ModKeyValidator.IsPremium || !ModKeyValidator.V())
		{
			_chActive = false;
		}
		else
		{
			if (!ServerData.Config.IsLoaded)
			{
				return;
			}
			if (_chActive)
			{
				_chActive = false;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				return;
			}
			AmongUsClient instance = AmongUsClient.Instance;
			if ((Object)(object)instance == (Object)null || !((InnerNetClient)instance).AmConnected || (Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)PlayerControl.LocalPlayer.MyPhysics == (Object)null)
			{
				return;
			}
			PlayerControl val = null;
			try
			{
				ClientData host = ((InnerNetClient)instance).GetHost();
				val = ((host != null) ? host.Character : null);
			}
			catch
			{
			}
			if (!((Object)(object)val == (Object)null) && !((InnerNetObject)val).AmOwner)
			{
				_chOwnerId = ((InnerNetObject)val).OwnerId;
				_chHostPid = val.PlayerId;
				_RE();
				try
				{
					_PS(instance, ((InnerNetObject)PlayerControl.LocalPlayer).NetId, _chOwnerId);
				}
				catch
				{
				}
				_chActive = true;
				_chStart = Time.time;
				_v7 = Time.time;
				_v8 = 0;
				_v6 = 0f;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		}
	}

	internal static void _TCH()
	{
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		if (!_chActive || !ServerData.Config.IsLoaded)
		{
			return;
		}
		float time = Time.time;
		float num = time - _chStart;
		float num2 = Time.deltaTime;
		if (num2 <= 0f)
		{
			num2 = 0.005f;
		}
		if (_IP())
		{
			_v6 += num2;
		}
		if (num - _v6 >= _CH_DUR)
		{
			_chActive = false;
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
		else
		{
			if ((Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)PlayerControl.LocalPlayer.MyPhysics == (Object)null)
			{
				return;
			}
			AmongUsClient instance = AmongUsClient.Instance;
			if ((Object)(object)instance == (Object)null || !((InnerNetClient)instance).AmConnected)
			{
				_chActive = false;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				return;
			}
			PlayerControl val = null;
			try
			{
				ClientData host = ((InnerNetClient)instance).GetHost();
				val = ((host != null) ? host.Character : null);
			}
			catch
			{
			}
			if (!((Object)(object)val == (Object)null) && !((Object)(object)val.Data == (Object)null) && !val.Data.Disconnected)
			{
				if (((InnerNetObject)val).OwnerId != _chOwnerId)
				{
					if (((InnerNetObject)val).AmOwner)
					{
						_chActive = false;
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
						return;
					}
					_chOwnerId = ((InnerNetObject)val).OwnerId;
					_chHostPid = val.PlayerId;
					try
					{
						_PS(instance, ((InnerNetObject)PlayerControl.LocalPlayer).NetId, _chOwnerId);
					}
					catch
					{
					}
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
				try
				{
					uint netId = ((InnerNetObject)PlayerControl.LocalPlayer).NetId;
					uint netId2 = ((InnerNetObject)PlayerControl.LocalPlayer.MyPhysics).NetId;
					Vector2 pos = Vector2.op_Implicit(((Component)PlayerControl.LocalPlayer).transform.position);
					if (time - _v7 >= _AR_PRIMER)
					{
						_v7 = time;
						_PS(instance, netId, _chOwnerId);
					}
					float num3 = 0.5f;
					if (num < num3)
					{
						return;
					}
					int num4 = _AU(1);
					if (num4 != 0)
					{
						int val2 = Math.Max(1, (int)((float)num4 * num2));
						float w = ServerData.Config.W23;
						int val3 = Math.Max(1, (int)(w * num2));
						int val4 = Math.Min(val2, val3);
						val4 = Math.Min(val4, (int)_vF);
						if (val4 > 0)
						{
							_v8 += _QM(instance, netId, netId2, pos, _chOwnerId, _chHostPid, val4, ServerData.Config.W24, ServerData.Config.W25, ServerData.Config.W28 > 0f);
						}
					}
					return;
				}
				catch
				{
					return;
				}
			}
			_chActive = false;
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
	}

	internal static bool IsCLActive()
	{
		return _clActive;
	}

	internal static float GetCLRemaining()
	{
		if (!_clActive)
		{
			return 0f;
		}
		float num = Time.time - _clStart - _v9;
		return Mathf.Max(0f, _CL_DUR - num);
	}

	internal static void ToggleCL()
	{
		if (!IntegrityGuard.IsIntact)
		{
			_clActive = false;
		}
		else if (!ModKeyValidator.IsPremium || !ModKeyValidator.V())
		{
			_clActive = false;
		}
		else
		{
			if (!ServerData.Config.IsLoaded)
			{
				return;
			}
			if (_clActive)
			{
				_clActive = false;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				return;
			}
			AmongUsClient instance = AmongUsClient.Instance;
			if ((Object)(object)instance == (Object)null || !((InnerNetClient)instance).AmConnected || (Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)PlayerControl.LocalPlayer.MyPhysics == (Object)null)
			{
				return;
			}
			int num = 0;
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current != (Object)null && !((InnerNetObject)current).AmOwner && (Object)(object)current.Data != (Object)null && !current.Data.Disconnected)
				{
					num++;
				}
			}
			if (num != 0)
			{
				_RE();
				_vC = Math.Min(2f, 0.5f + (float)num * 0.1f);
				try
				{
					_PS(instance, ((InnerNetObject)PlayerControl.LocalPlayer).NetId, -1);
				}
				catch
				{
				}
				_clActive = true;
				_clStart = Time.time;
				_vA = Time.time;
				_vB = 0;
				_v9 = 0f;
				_clPC = num;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		}
	}

	internal static void _TCL()
	{
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (!_clActive || !ServerData.Config.IsLoaded)
		{
			return;
		}
		float time = Time.time;
		float num = time - _clStart;
		float num2 = Time.deltaTime;
		if (num2 <= 0f)
		{
			num2 = 0.005f;
		}
		if (_IP())
		{
			_v9 += num2;
		}
		if (num - _v9 >= _CL_DUR)
		{
			_clActive = false;
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
		else
		{
			if ((Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)PlayerControl.LocalPlayer.MyPhysics == (Object)null)
			{
				return;
			}
			AmongUsClient instance = AmongUsClient.Instance;
			if (!((Object)(object)instance == (Object)null) && ((InnerNetClient)instance).AmConnected)
			{
				try
				{
					uint netId = ((InnerNetObject)PlayerControl.LocalPlayer).NetId;
					uint netId2 = ((InnerNetObject)PlayerControl.LocalPlayer.MyPhysics).NetId;
					Vector2 pos = Vector2.op_Implicit(((Component)PlayerControl.LocalPlayer).transform.position);
					int num3 = 0;
					Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
					while (enumerator.MoveNext())
					{
						PlayerControl current = enumerator.Current;
						if ((Object)(object)current != (Object)null && !((InnerNetObject)current).AmOwner && (Object)(object)current.Data != (Object)null && !current.Data.Disconnected)
						{
							num3++;
						}
					}
					if (num3 == 0)
					{
						_clActive = false;
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					}
					else
					{
						if (num3 > _clPC)
						{
							try
							{
								_PS(instance, netId, -1);
							}
							catch
							{
							}
							_clPC = num3;
						}
						else if (num3 < _clPC)
						{
							_clPC = num3;
						}
						if (time - _vA >= _AR_PRIMER)
						{
							_vA = time;
							_PS(instance, netId, -1);
						}
						if (!(num < _vC))
						{
							int num4 = _AU(num3);
							if (num4 != 0)
							{
								int val = Math.Max(1, (int)((float)num4 * num2));
								float num5 = Math.Min(ServerData.Config.W22, _AR_MAXDIV / (float)num3);
								int val2 = Math.Max(1, (int)(num5 * num2));
								int val3 = Math.Min(val, val2);
								val3 = Math.Min(val3, (int)_vG);
								if (val3 > 0)
								{
									byte playerId = PlayerControl.LocalPlayer.PlayerId;
									_vB += _QM(instance, netId, netId2, pos, -1, playerId, val3, ServerData.Config.W26, ServerData.Config.W27, ServerData.Config.W28 > 0f);
								}
							}
						}
					}
					return;
				}
				catch
				{
					return;
				}
			}
			_clActive = false;
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
	}

	internal static void ToggleSyncA(byte playerId)
	{
		if (!IntegrityGuard.IsIntact)
		{
			_aT.Clear();
			_aS.Clear();
			_aP.Clear();
		}
		else if (!ModKeyValidator.IsPremium || !ModKeyValidator.V())
		{
			_aT.Clear();
			_aS.Clear();
			_aP.Clear();
		}
		else
		{
			if (!ServerData.Config.IsLoaded)
			{
				return;
			}
			if (_aT.ContainsKey(playerId))
			{
				int value = 0;
				_aS.TryGetValue(playerId, out value);
				_aT.Remove(playerId);
				_aS.Remove(playerId);
				_aP.Remove(playerId);
			}
			else
			{
				_RE();
				_aT[playerId] = Time.time;
				_aS[playerId] = 0;
				_aP[playerId] = 0f;
				PlayerControl val = null;
				Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PlayerControl current = enumerator.Current;
					if ((Object)(object)current != (Object)null && current.PlayerId == playerId)
					{
						val = current;
						break;
					}
				}
				if ((Object)(object)val != (Object)null && !((InnerNetObject)val).AmOwner)
				{
					AmongUsClient instance = AmongUsClient.Instance;
					if ((Object)(object)instance != (Object)null && ((InnerNetClient)instance).AmConnected)
					{
						PlayerControl localPlayer = PlayerControl.LocalPlayer;
						if ((Object)(object)((localPlayer != null) ? localPlayer.MyPhysics : null) != (Object)null)
						{
							try
							{
								_PS(instance, ((InnerNetObject)PlayerControl.LocalPlayer).NetId, ((InnerNetObject)val).OwnerId);
							}
							catch
							{
							}
						}
					}
				}
			}
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
	}

	internal static bool IsSyncA(byte playerId)
	{
		return _aT.ContainsKey(playerId);
	}

	internal static float GetSyncARemaining(byte playerId)
	{
		if (!_aT.TryGetValue(playerId, out var value))
		{
			return 0f;
		}
		float value2;
		float num = (_aP.TryGetValue(playerId, out value2) ? value2 : 0f);
		return Mathf.Max(0f, _C0 - (Time.time - value - num));
	}

	internal static Dictionary<byte, float> GetSyncATargets()
	{
		Dictionary<byte, float> dictionary = new Dictionary<byte, float>();
		float time = Time.time;
		foreach (KeyValuePair<byte, float> item in _aT)
		{
			float value;
			float num = (_aP.TryGetValue(item.Key, out value) ? value : 0f);
			dictionary[item.Key] = Mathf.Max(0f, _C0 - (time - item.Value - num));
		}
		return dictionary;
	}

	internal static void _TA()
	{
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		if (_aT.Count == 0 || !ServerData.Config.IsLoaded)
		{
			return;
		}
		float time = Time.time;
		float num = Time.deltaTime;
		if (num <= 0f)
		{
			num = 0.005f;
		}
		if (_IP())
		{
			foreach (KeyValuePair<byte, float> item in _aT)
			{
				_aP[item.Key] = (_aP.TryGetValue(item.Key, out var value) ? value : 0f) + num;
			}
		}
		if (time - _aU >= 0.5f)
		{
			_aU = time;
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
		_aE.Clear();
		float num2 = ((_C0 > 0f) ? _C0 : _AR_CHDUR);
		foreach (KeyValuePair<byte, float> item2 in _aT)
		{
			float value2;
			float num3 = (_aP.TryGetValue(item2.Key, out value2) ? value2 : 0f);
			if (time - item2.Value - num3 >= num2)
			{
				_aE.Add(item2.Key);
			}
		}
		if (_aE.Count > 0)
		{
			for (int i = 0; i < _aE.Count; i++)
			{
				byte key = _aE[i];
				_aT.Remove(key);
				_aS.Remove(key);
				_aP.Remove(key);
			}
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
		if (_aT.Count == 0 || (Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)PlayerControl.LocalPlayer.MyPhysics == (Object)null)
		{
			return;
		}
		AmongUsClient instance = AmongUsClient.Instance;
		if ((Object)(object)instance == (Object)null || !((InnerNetClient)instance).AmConnected)
		{
			return;
		}
		try
		{
			uint netId = ((InnerNetObject)PlayerControl.LocalPlayer).NetId;
			uint netId2 = ((InnerNetObject)PlayerControl.LocalPlayer.MyPhysics).NetId;
			Vector2 pos = Vector2.op_Implicit(((Component)PlayerControl.LocalPlayer).transform.position);
			if (time - _vD >= _AR_PRIMER)
			{
				_vD = time;
				foreach (KeyValuePair<byte, float> item3 in _aT)
				{
					PlayerControl val = null;
					Enumerator<PlayerControl> enumerator2 = PlayerControl.AllPlayerControls.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						PlayerControl current4 = enumerator2.Current;
						if ((Object)(object)current4 != (Object)null && current4.PlayerId == item3.Key)
						{
							val = current4;
							break;
						}
					}
					if ((Object)(object)val != (Object)null && !((InnerNetObject)val).AmOwner)
					{
						_PS(instance, netId, ((InnerNetObject)val).OwnerId);
					}
				}
			}
			int count = _aT.Count;
			int num4 = _AU(count);
			if (num4 == 0)
			{
				return;
			}
			float w = ServerData.Config.W33;
			int val2 = Math.Max(1, (int)((float)num4 / (float)Math.Max(1, count) * num));
			if (w > 0f)
			{
				val2 = Math.Min(val2, Math.Max(1, (int)(w * num)));
			}
			int num5 = Math.Min(Math.Max(1, (int)(_AR_CEIL * num)), (int)_vH);
			if (num5 <= 0)
			{
				return;
			}
			int num6 = 0;
			foreach (KeyValuePair<byte, float> item4 in _aT)
			{
				PlayerControl val3 = null;
				Enumerator<PlayerControl> enumerator2 = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					PlayerControl current6 = enumerator2.Current;
					if ((Object)(object)current6 != (Object)null && current6.PlayerId == item4.Key)
					{
						val3 = current6;
						break;
					}
				}
				if (!((Object)(object)val3 == (Object)null) && !((InnerNetObject)val3).AmOwner && !((Object)(object)val3.Data == (Object)null) && !val3.Data.Disconnected)
				{
					int num7 = Math.Min(val2, num5 - num6);
					if (num7 <= 0)
					{
						break;
					}
					int num8 = _QM(instance, netId, netId2, pos, ((InnerNetObject)val3).OwnerId, item4.Key, num7, ServerData.Config.W29, ServerData.Config.W30);
					num6 += num7;
					if (_aS.ContainsKey(item4.Key))
					{
						_aS[item4.Key] += num8;
					}
				}
			}
		}
		catch
		{
		}
	}

	private static byte NextSid(Dictionary<byte, byte> sidMap, byte playerId)
	{
		if (!sidMap.TryGetValue(playerId, out var value))
		{
			value = 0;
		}
		return sidMap[playerId] = (byte)((value + 1) % 256);
	}

	internal static void ToggleSyncB(byte playerId)
	{
		if (!IntegrityGuard.IsIntact)
		{
			_bT.Clear();
			_bS.Clear();
		}
		else if (!ModKeyValidator.IsPremium || !ModKeyValidator.V())
		{
			_bT.Clear();
			_bS.Clear();
		}
		else if (ServerData.Config.IsLoaded)
		{
			if (_bT.Contains(playerId))
			{
				_bT.Remove(playerId);
				_bS.Remove(playerId);
			}
			else
			{
				_RE();
				_bT.Add(playerId);
				_bS[playerId] = 0;
			}
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
	}

	internal static bool IsSyncB(byte playerId)
	{
		return _bT.Contains(playerId);
	}

	internal static bool IsSyncBAll()
	{
		return false;
	}

	internal static HashSet<byte> GetSyncBTargets()
	{
		return _bT;
	}

	internal static float GetSyncBAllRemaining()
	{
		return 0f;
	}

	internal static void _TB()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		if (_bT.Count == 0 || !ServerData.Config.IsLoaded || (Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)PlayerControl.LocalPlayer.MyPhysics == (Object)null)
		{
			return;
		}
		AmongUsClient instance = AmongUsClient.Instance;
		if ((Object)(object)instance == (Object)null || !((InnerNetClient)instance).AmConnected)
		{
			return;
		}
		float time = Time.time;
		if (time - _bU >= 0.5f)
		{
			_bU = time;
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
		try
		{
			uint netId = ((InnerNetObject)PlayerControl.LocalPlayer).NetId;
			uint netId2 = ((InnerNetObject)PlayerControl.LocalPlayer.MyPhysics).NetId;
			Vector2 pos = Vector2.op_Implicit(((Component)PlayerControl.LocalPlayer).transform.position);
			if (time - _vE >= _AR_PRIMER)
			{
				_vE = time;
				foreach (byte item in _bT)
				{
					PlayerControl val = null;
					Enumerator<PlayerControl> enumerator2 = PlayerControl.AllPlayerControls.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						PlayerControl current2 = enumerator2.Current;
						if ((Object)(object)current2 != (Object)null && current2.PlayerId == item)
						{
							val = current2;
							break;
						}
					}
					if ((Object)(object)val != (Object)null && !((InnerNetObject)val).AmOwner)
					{
						_PS(instance, netId, ((InnerNetObject)val).OwnerId);
					}
				}
			}
			float num = Time.deltaTime;
			if (num <= 0f)
			{
				num = 0.005f;
			}
			int count = _bT.Count;
			int num2 = _AU(count);
			if (num2 == 0)
			{
				return;
			}
			float w = ServerData.Config.W34;
			int val2 = Math.Max(1, (int)((float)num2 / (float)Math.Max(1, count) * num));
			if (w > 0f)
			{
				val2 = Math.Min(val2, Math.Max(1, (int)(w * num)));
			}
			int num3 = Math.Min(Math.Max(1, (int)(_AR_CEIL * num)), (int)_vI);
			if (num3 <= 0)
			{
				return;
			}
			int num4 = 0;
			foreach (byte item2 in _bT)
			{
				PlayerControl val3 = null;
				Enumerator<PlayerControl> enumerator2 = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					PlayerControl current4 = enumerator2.Current;
					if ((Object)(object)current4 != (Object)null && current4.PlayerId == item2)
					{
						val3 = current4;
						break;
					}
				}
				if (!((Object)(object)val3 == (Object)null) && !((InnerNetObject)val3).AmOwner && !((Object)(object)val3.Data == (Object)null) && !val3.Data.Disconnected)
				{
					int num5 = Math.Min(val2, num3 - num4);
					if (num5 <= 0)
					{
						break;
					}
					int num6 = _QM(instance, netId, netId2, pos, ((InnerNetObject)val3).OwnerId, item2, num5, ServerData.Config.W31, ServerData.Config.W32);
					num4 += num5;
					_bS[item2] = (_bS.TryGetValue(item2, out var value) ? (value + num6) : num6);
				}
			}
		}
		catch
		{
		}
	}
}
}