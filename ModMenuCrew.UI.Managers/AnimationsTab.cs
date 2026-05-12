using System;
using System.Collections.Generic;
using ModMenuCrew.Features;
using ModMenuCrew.UI.Menus;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew.UI.Managers;

public class AnimationsTab
{
	private ServerData.UISnapshot _safeSnapshot;

	private byte[] _snapshotBytecode;

	private byte[] _snapshotInverseMap;

	private long _snapshotToken;

	internal static bool _showMovement = true;

	internal static bool _showJumpSpawn = true;

	internal static bool _showGhost = false;

	internal static bool _showRole = true;

	internal static bool _showPet = false;

	internal static bool _showSkin = false;

	internal static bool _showHat = false;

	internal static bool _showVfx = false;

	internal static bool _showMeeting = false;

	private static Dictionary<string, Action<long>> _registry;

	internal static Dictionary<string, Action<long>> GetOrBuildActionRegistry()
	{
		if (_registry != null)
		{
			return _registry;
		}
		_registry = new Dictionary<string, Action<long>>();
		Section("anim_sec_movement", delegate(bool v)
		{
			_showMovement = v;
		}, () => _showMovement);
		Section("anim_sec_jumpspawn", delegate(bool v)
		{
			_showJumpSpawn = v;
		}, () => _showJumpSpawn);
		Section("anim_sec_ghost", delegate(bool v)
		{
			_showGhost = v;
		}, () => _showGhost);
		Section("anim_sec_role", delegate(bool v)
		{
			_showRole = v;
		}, () => _showRole);
		Section("anim_sec_pet", delegate(bool v)
		{
			_showPet = v;
		}, () => _showPet);
		Section("anim_sec_skin", delegate(bool v)
		{
			_showSkin = v;
		}, () => _showSkin);
		Section("anim_sec_hat", delegate(bool v)
		{
			_showHat = v;
		}, () => _showHat);
		Section("anim_sec_vfx", delegate(bool v)
		{
			_showVfx = v;
		}, () => _showVfx);
		Section("anim_sec_meeting", delegate(bool v)
		{
			_showMeeting = v;
		}, () => _showMeeting);
		ToggleLoop("anim_idle", AnimationService.PlayIdle);
		ToggleLoop("anim_run", AnimationService.PlayRun);
		ToggleLoop("anim_climb_up", AnimationService.PlayClimbUp);
		ToggleLoop("anim_climb_down", AnimationService.PlayClimbDown);
		ToggleLoop("anim_enter_vent", AnimationService.PlayEnterVent);
		ToggleLoop("anim_exit_vent", AnimationService.PlayExitVent);
		ToggleLoop("anim_jump", AnimationService.PlayJump);
		ToggleLoop("anim_spawn", AnimationService.PlaySpawn);
		ToggleLoop("anim_scanner_on", AnimationService.PlayScannerOn);
		OneShot("anim_scanner_off", AnimationService.PlayScannerOff);
		ToggleLoop("anim_ghost_idle", AnimationService.PlayGhostIdle);
		ToggleLoop("anim_ga_idle", AnimationService.PlayGuardianAngelIdle);
		ToggleLoop("anim_shapeshift", AnimationService.PlayShapeshift);
		ToggleLoop("anim_vanish", AnimationService.PlayVanishCharge);
		ToggleLoop("anim_vanish_poof", AnimationService.PlayVanishPoof);
		ToggleLoop("anim_appear", AnimationService.PlayAppearPoof);
		ToggleLoop("anim_protect_flash", AnimationService.PlayProtectFlash);
		ToggleLoop("anim_protect_loop", AnimationService.PlayProtectLoop);
		ToggleLoop("anim_pet_sequence", AnimationService.PlayPetSequence);
		ToggleLoop("anim_pet_idle", AnimationService.PlayPetIdle);
		ToggleLoop("anim_pet_walk", AnimationService.PlayPetWalk);
		ToggleLoop("anim_pet_scared", AnimationService.PlayPetScared);
		ToggleLoop("anim_pet_mourn", AnimationService.PlayPetMourn);
		ToggleLoop("anim_skin_idle", AnimationService.PlaySkinIdle);
		ToggleLoop("anim_skin_jump", AnimationService.PlaySkinJump);
		ToggleLoop("anim_skin_climb", AnimationService.PlaySkinClimbUp);
		ToggleLoop("anim_skin_climb_down", AnimationService.PlaySkinClimbDown);
		ToggleLoop("anim_skin_spawn", AnimationService.PlaySkinSpawn);
		ToggleLoop("anim_skin_ghost", AnimationService.PlaySkinGhost);
		ToggleLoop("anim_hat_climb", AnimationService.PlayHatClimb);
		ToggleLoop("anim_hat_floor", AnimationService.PlayHatFloor);
		ToggleLoop("anim_alert_flash", AnimationService.PlayAlertFlash);
		ToggleLoop("anim_kill_blur", AnimationService.PlayKillBlur);
		ToggleLoop("anim_particles_burst", AnimationService.PlayParticleBurst);
		OneShot("anim_mushroom_tint_in", AnimationService.PlayMushroomTintIn);
		OneShot("anim_mushroom_tint_out", AnimationService.PlayMushroomTintOut);
		OneShot("anim_meeting_slam", AnimationService.PlayMeetingSlam);
		OneShot("anim_eject_text_sfx", AnimationService.PlayEjectTextSfx);
		return _registry;
		static void OneShot(string id, Action fn)
		{
			ServerData.RegisterControlId(id);
			_registry[id] = delegate(long t)
			{
				if (!GhostUI.CheckToken(t))
				{
					return;
				}
				try
				{
					fn();
				}
				catch
				{
				}
			};
		}
		static void Section(string id, Action<bool> setter, Func<bool> getter)
		{
			ServerData.RegisterControlId(id);
			_registry[id] = delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					setter(!getter());
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			};
		}
		static void ToggleLoop(string id, Action fn)
		{
			ServerData.RegisterControlId(id);
			_registry[id] = delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					AnimationToggleService.Toggle(id, fn);
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			};
		}
	}

	internal static int GetStateHash()
	{
		return (((((((((17 * 31 + (_showMovement ? 1 : 0)) * 31 + (_showJumpSpawn ? 1 : 0)) * 31 + (_showGhost ? 1 : 0)) * 31 + (_showRole ? 1 : 0)) * 31 + (_showPet ? 1 : 0)) * 31 + (_showSkin ? 1 : 0)) * 31 + (_showHat ? 1 : 0)) * 31 + (_showVfx ? 1 : 0)) * 31 + (_showMeeting ? 1 : 0)) * 31 + AnimationToggleService.GetStateHash();
	}

	internal static object GetUIState()
	{
		return new
		{
			mv = _showMovement,
			js = _showJumpSpawn,
			gh = _showGhost,
			ro = _showRole,
			pt = _showPet,
			sk = _showSkin,
			ht = _showHat,
			vx = _showVfx,
			mt = _showMeeting,
			active = AnimationToggleService.GetActiveIds()
		};
	}

	public void Draw()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)Event.current.type == 8)
		{
			PlayerPickMenu.CheckRealtimeUpdate();
			_safeSnapshot = ServerData.CurrentSnapshot;
			byte[] array = _safeSnapshot?.AnimationsBytecode;
			if (array != null && array.Length >= 536 && array != _snapshotBytecode)
			{
				_snapshotBytecode = array;
				_snapshotInverseMap = new byte[256];
				Array.Copy(array, 268, _snapshotInverseMap, 0, 256);
				_snapshotToken = BitConverter.ToInt64(array, 260);
			}
		}
		if (_snapshotBytecode != null && _snapshotBytecode.Length != 0)
		{
			GhostUI.Execute(_snapshotBytecode, _snapshotToken, GetOrBuildActionRegistry(), _snapshotInverseMap);
			return;
		}
		GUILayout.Label("<color=#949EAD>Loading animations...</color>", GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedHeight(50f) });
	}
}
