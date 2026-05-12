using System;
using System.Collections.Generic;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace ModMenuCrew.Features;

internal static class AnimationToggleService
{
	private class LoopEntry
	{
		public Action Play;

		public float Interval;

		public float NextFire;
	}

	[HarmonyPatch(typeof(HudManager), "Update")]
	public static class TickPatch
	{
		public static void Postfix()
		{
			try
			{
				Tick();
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(PlayerPhysics), "HandleAnimation")]
	public static class ForceAnimPatch
	{
		public static void Postfix(PlayerPhysics __instance)
		{
			try
			{
				if ((Object)(object)__instance == (Object)null || (Object)(object)__instance.myPlayer == (Object)null || !((InnerNetObject)__instance).AmOwner)
				{
					return;
				}
				PlayerAnimations animations = __instance.Animations;
				if ((Object)(object)animations == (Object)null)
				{
					return;
				}
				if (_active.ContainsKey("anim_idle"))
				{
					if (animations.IsPlayingRunAnimation() || animations.IsPlayingClimbAnimation())
					{
						animations.PlayIdleAnimation();
					}
				}
				else if (_active.ContainsKey("anim_run"))
				{
					if (!animations.IsPlayingRunAnimation() && !animations.IsPlayingClimbAnimation() && !animations.IsPlayingEnterVentAnimation() && !animations.IsPlayingSpawnAnimation())
					{
						animations.PlayRunAnimation();
					}
				}
				else if (_active.ContainsKey("anim_climb_up"))
				{
					if (!animations.IsPlayingClimbAnimation())
					{
						animations.PlayClimbAnimation(false);
					}
				}
				else if (_active.ContainsKey("anim_climb_down"))
				{
					if (!animations.IsPlayingClimbAnimation())
					{
						animations.PlayClimbAnimation(true);
					}
				}
				else if (_active.ContainsKey("anim_ghost_idle"))
				{
					if (!animations.IsPlayingGhostIdleAnimation())
					{
						animations.PlayGhostIdleAnimation();
					}
				}
				else if (_active.ContainsKey("anim_ga_idle") && !animations.IsPlayingGuardianAngelIdleAnimation())
				{
					animations.PlayGuardianAngelIdleAnimation();
				}
			}
			catch
			{
			}
		}
	}

	private static readonly Dictionary<string, LoopEntry> _active = new Dictionary<string, LoopEntry>();

	private static readonly Dictionary<string, float> _intervals = BuildIntervals();

	private static Dictionary<string, float> BuildIntervals()
	{
		return new Dictionary<string, float>
		{
			{ "anim_idle", 2f },
			{ "anim_run", 2f },
			{ "anim_climb_up", 2f },
			{ "anim_climb_down", 2f },
			{ "anim_enter_vent", 1.5f },
			{ "anim_exit_vent", 1.5f },
			{ "anim_jump", 1f },
			{ "anim_spawn", 1.5f },
			{ "anim_scanner_on", 0.8f },
			{ "anim_ghost_idle", 2f },
			{ "anim_ga_idle", 2f },
			{ "anim_shapeshift", 1.5f },
			{ "anim_vanish", 1.5f },
			{ "anim_vanish_poof", 1.5f },
			{ "anim_appear", 1.5f },
			{ "anim_protect_flash", 1.5f },
			{ "anim_protect_loop", 5.2f },
			{ "anim_pet_sequence", 3f },
			{ "anim_pet_idle", 2.5f },
			{ "anim_pet_walk", 2.5f },
			{ "anim_pet_scared", 2.5f },
			{ "anim_pet_mourn", 2.5f },
			{ "anim_skin_idle", 2f },
			{ "anim_skin_jump", 1f },
			{ "anim_skin_climb", 2f },
			{ "anim_skin_climb_down", 2f },
			{ "anim_skin_spawn", 2f },
			{ "anim_skin_ghost", 2f },
			{ "anim_hat_climb", 2.5f },
			{ "anim_hat_floor", 2.5f },
			{ "anim_alert_flash", 1.2f },
			{ "anim_kill_blur", 1f },
			{ "anim_particles_burst", 1f }
		};
	}

	internal static bool IsOneShot(string id)
	{
		switch (id)
		{
		default:
			return id == "anim_eject_text_sfx";
		case "anim_scanner_off":
		case "anim_mushroom_tint_in":
		case "anim_mushroom_tint_out":
		case "anim_meeting_slam":
			return true;
		}
	}

	internal static bool IsActive(string id)
	{
		if (!string.IsNullOrEmpty(id))
		{
			return _active.ContainsKey(id);
		}
		return false;
	}

	internal static void Toggle(string id, Action play)
	{
		if (!string.IsNullOrEmpty(id) && play != null)
		{
			if (_active.ContainsKey(id))
			{
				_active.Remove(id);
				return;
			}
			float value;
			float interval = (_intervals.TryGetValue(id, out value) ? value : 1.5f);
			_active[id] = new LoopEntry
			{
				Play = play,
				Interval = interval,
				NextFire = Time.time
			};
		}
	}

	internal static void Clear()
	{
		_active.Clear();
	}

	internal static int GetStateHash()
	{
		int num = 17;
		foreach (KeyValuePair<string, LoopEntry> item in _active)
		{
			num = num * 31 + item.Key.GetHashCode();
		}
		return num * 31 + _active.Count;
	}

	internal static string[] GetActiveIds()
	{
		if (_active.Count == 0)
		{
			return Array.Empty<string>();
		}
		string[] array = new string[_active.Count];
		int num = 0;
		foreach (string key in _active.Keys)
		{
			array[num++] = key;
		}
		return array;
	}

	internal static void Tick()
	{
		if (_active.Count == 0)
		{
			return;
		}
		float time = Time.time;
		List<string> list = null;
		foreach (KeyValuePair<string, LoopEntry> item in _active)
		{
			if (time >= item.Value.NextFire)
			{
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(item.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			string key = list[i];
			if (_active.TryGetValue(key, out var value))
			{
				try
				{
					value.Play?.Invoke();
				}
				catch
				{
				}
				value.NextFire = time + value.Interval;
			}
		}
	}
}
