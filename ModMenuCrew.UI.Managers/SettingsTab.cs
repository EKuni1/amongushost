using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using BepInEx.Configuration;
using ModMenuCrew.Features;
using ModMenuCrew.Patches;
using ModMenuCrew.UI.Menus;
using ModMenuCrew.UI.Styles;
using ModMenuCrew.Web;
using UnityEngine;

namespace ModMenuCrew.UI.Managers;

public class SettingsTab
{
	private byte[] _cachedSTGBytecode;

	private byte[] _cachedSTGInverseMap;

	private long _cachedSTGToken;

	private static bool _showAppearance = true;

	private static bool _showRadarVision = false;

	private static bool _showGameplay = false;

	private static bool _showHostLobby = false;

	private static bool _showRoles = false;

	private static bool _showKeybinds = false;

	private static bool _showCosmetics = false;

	private static bool _pinMode = false;

	private static readonly string[] PINNABLE_KEYS = new string[22]
	{
		"radar", "tracers", "webradar", "ghosts", "deadchat", "freecam", "noclip", "nokillcd", "forceimp", "blockend",
		"revealvotes", "infreport", "tpcursor", "endlessvent", "crewsab", "multisab", "lobbytimer", "streamermode", "venting", "walkvent",
		"chatpaste", "playerinfo"
	};

	private static string _pendingKeybind = null;

	private static float _pendingSaveTime = 0f;

	private static bool _savePending = false;

	private static Dictionary<string, Action<long>> _actionRegistry;

	internal static void RegisterActions(Dictionary<string, Action<long>> registry)
	{
		if (registry == null)
		{
			return;
		}
		registry["stg_save"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				ConfigFile config2 = LobbyHarmonyPatches.Config;
				if (config2 != null)
				{
					config2.Save();
				}
				NotifyUtils.Info("Settings saved!");
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_reload"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				ConfigFile config = LobbyHarmonyPatches.Config;
				if (config != null)
				{
					config.Reload();
				}
				NotifyUtils.Info("Settings reloaded!");
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_reset"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				ResetAllToDefaults();
				NotifyUtils.Info("Settings reset to defaults!");
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_sec_appearance"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_showAppearance = !_showAppearance;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_sec_radarvision"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_showRadarVision = !_showRadarVision;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_sec_gameplay"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_showGameplay = !_showGameplay;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_sec_hostlobby"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_showHostLobby = !_showHostLobby;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_sec_roles"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_showRoles = !_showRoles;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_sec_keybinds"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_showKeybinds = !_showKeybinds;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_sec_cosmetics"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_showCosmetics = !_showCosmetics;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_pin_mode"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_pinMode = !_pinMode;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		string[] pINNABLE_KEYS = PINNABLE_KEYS;
		foreach (string text in pINNABLE_KEYS)
		{
			string key = text;
			registry["stg_pin_" + key] = delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					TogglePin(key);
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					ScheduleSave();
				}
			};
		}
		registry["stg_preset_streamer"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				ApplyPresetStreamer();
			}
		};
		registry["stg_preset_observer"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				ApplyPresetObserver();
			}
		};
		registry["stg_preset_competitive"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				ApplyPresetCompetitive();
			}
		};
		registry["stg_preset_chaos"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				ApplyPresetChaos();
			}
		};
		registry["stg_preset_host"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				ApplyPresetHost();
			}
		};
		registry["stg_preset_roles"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				ApplyPresetRoles();
			}
		};
		for (int j = 0; j < 6; j++)
		{
			int idx = j;
			registry[$"stg_theme_{j}"] = delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					ThemePresets.Apply(idx);
					if (CheatConfig.ThemeIndex != null)
					{
						CheatConfig.ThemeIndex.Value = idx;
					}
					NotifyUtils.Info("Theme: " + ThemePresets.Names[idx]);
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					ScheduleSave();
				}
			};
		}
		registry["stg_opacity"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.MenuOpacity != null)
				{
					CheatConfig.MenuOpacity.Value = ServerData.GetSliderValue("stg_opacity");
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_width"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.MenuWidth != null)
				{
					CheatConfig.MenuWidth.Value = ServerData.GetSliderValue("stg_width");
					GhostUI.SetWindowSize(CheatConfig.MenuWidth.Value, CheatConfig.MenuHeight?.Value ?? 600f);
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_height"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.MenuHeight != null)
				{
					CheatConfig.MenuHeight.Value = ServerData.GetSliderValue("stg_height");
					GhostUI.SetWindowSize(CheatConfig.MenuWidth?.Value ?? 500f, CheatConfig.MenuHeight.Value);
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_vision"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				CheatConfig.VisionMultiplier = ServerData.GetSliderValue("stg_vision");
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_radar_zoom"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.RadarScale != null)
				{
					CheatConfig.RadarScale.Value = ServerData.GetSliderValue("stg_radar_zoom");
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_menu_key"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_pendingKeybind = "menukey";
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_rgb_lobby"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (LobbyHarmonyPatches.cfgRgbLobbyCode != null)
				{
					LobbyHarmonyPatches.cfgRgbLobbyCode.Value = !LobbyHarmonyPatches.cfgRgbLobbyCode.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_lobby_music"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (LobbyHarmonyPatches.cfgDisableLobbyMusic != null)
				{
					LobbyHarmonyPatches.cfgDisableLobbyMusic.Value = !LobbyHarmonyPatches.cfgDisableLobbyMusic.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_hide_star"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.HideMMCStar != null)
				{
					CheatConfig.HideMMCStar.Value = !CheatConfig.HideMMCStar.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_size_450_450"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SetInterfaceSize(450f, 450f);
			}
		};
		registry["stg_size_500_600"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SetInterfaceSize(500f, 600f);
			}
		};
		registry["stg_size_600_700"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SetInterfaceSize(600f, 700f);
			}
		};
		registry["stg_size_800_800"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SetInterfaceSize(800f, 800f);
			}
		};
		registry["stg_size_900_600"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SetInterfaceSize(900f, 600f);
			}
		};
		registry["stg_size_500_800"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SetInterfaceSize(500f, 800f);
			}
		};
		registry["stg_auto_size"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				ApplySmartSize();
			}
		};
		registry["stg_size_720p"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SetInterfaceSize(500f, 550f);
			}
		};
		registry["stg_size_1080p"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SetInterfaceSize(550f, 650f);
			}
		};
		registry["stg_size_1440p"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SetInterfaceSize(650f, 750f);
			}
		};
		registry["stg_size_4k"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SetInterfaceSize(750f, 800f);
			}
		};
		registry["stg_size_uw"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SetInterfaceSize(700f, 600f);
			}
		};
		registry["stg_streamer_mode"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (LobbyHarmonyPatches.cfgStreamerMode != null)
				{
					LobbyHarmonyPatches.cfgStreamerMode.Value = !LobbyHarmonyPatches.cfgStreamerMode.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_lobby_timer"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (LobbyHarmonyPatches.cfgShowLobbyTimer != null)
				{
					LobbyHarmonyPatches.cfgShowLobbyTimer.Value = !LobbyHarmonyPatches.cfgShowLobbyTimer.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_auto_extend"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (LobbyHarmonyPatches.cfgAutoExtendTimer != null)
				{
					LobbyHarmonyPatches.cfgAutoExtendTimer.Value = !LobbyHarmonyPatches.cfgAutoExtendTimer.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_extra_info"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (LobbyHarmonyPatches.cfgShowLobbyInfo != null)
				{
					LobbyHarmonyPatches.cfgShowLobbyInfo.Value = !LobbyHarmonyPatches.cfgShowLobbyInfo.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_player_info"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.ShowPlayerInfo != null)
				{
					CheatConfig.ShowPlayerInfo.Value = !CheatConfig.ShowPlayerInfo.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_see_ghosts"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.SeeGhosts != null)
				{
					CheatConfig.SeeGhosts.Value = !CheatConfig.SeeGhosts.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_dead_chat"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.SeeDeadChat != null)
				{
					CheatConfig.SeeDeadChat.Value = !CheatConfig.SeeDeadChat.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_freecam"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.FreeCamEnabled != null)
				{
					CheatConfig.FreeCamEnabled.Value = !CheatConfig.FreeCamEnabled.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_noclip"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.NoClipSmoothEnabled != null)
				{
					CheatConfig.NoClipSmoothEnabled.Value = !CheatConfig.NoClipSmoothEnabled.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_lock_scroll"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.LockScrollZoom != null)
				{
					CheatConfig.LockScrollZoom.Value = !CheatConfig.LockScrollZoom.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_radar"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.RadarEnabled != null)
				{
					CheatConfig.RadarEnabled.Value = !CheatConfig.RadarEnabled.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_tracers"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.TracersEnabled != null)
				{
					CheatConfig.TracersEnabled.Value = !CheatConfig.TracersEnabled.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_radar_map"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.RadarShowMapImage != null)
				{
					CheatConfig.RadarShowMapImage.Value = !CheatConfig.RadarShowMapImage.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_webradar"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.WebRadarEnabled != null)
				{
					CheatConfig.WebRadarEnabled.Value = !CheatConfig.WebRadarEnabled.Value;
					if (CheatConfig.WebRadarEnabled.Value)
					{
						if (!WebRadarService.IsRunning)
						{
							WebRadarService.Start();
						}
					}
					else if (WebRadarService.IsRunning)
					{
						WebRadarService.Stop();
					}
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_venting"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.AllowVenting != null)
				{
					CheatConfig.AllowVenting.Value = !CheatConfig.AllowVenting.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_walk_vent"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.WalkInVent != null)
				{
					CheatConfig.WalkInVent.Value = !CheatConfig.WalkInVent.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_tp_cursor"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.TeleportWithCursor != null)
				{
					CheatConfig.TeleportWithCursor.Value = !CheatConfig.TeleportWithCursor.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_no_kill_cd"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.NoKillCooldown != null)
				{
					CheatConfig.NoKillCooldown.Value = !CheatConfig.NoKillCooldown.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_force_imp"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.ForceImpostor != null)
				{
					CheatConfig.ForceImpostor.Value = !CheatConfig.ForceImpostor.Value;
					ImpostorForcer.SetRoleOverrideEnabled(CheatConfig.ForceImpostor.Value);
					if (CheatConfig.ForceImpostor.Value)
					{
						ImpostorForcer.SetSelectedRoleForHost((RoleTypes)1);
					}
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["stg_block_end"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.DisableGameEnd != null)
				{
					CheatConfig.DisableGameEnd.Value = !CheatConfig.DisableGameEnd.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_reveal_votes"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.RevealVotes != null)
				{
					CheatConfig.RevealVotes.Value = !CheatConfig.RevealVotes.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_crew_sab"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.CrewmateSabotage != null)
				{
					CheatConfig.CrewmateSabotage.Value = !CheatConfig.CrewmateSabotage.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_multi_sab"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.MultiSabotage != null)
				{
					CheatConfig.MultiSabotage.Value = !CheatConfig.MultiSabotage.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_nf4"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.Nf4 != null)
				{
					CheatConfig.Nf4.Value = !CheatConfig.Nf4.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_nf7"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.Nf7 != null)
				{
					CheatConfig.Nf7.Value = !CheatConfig.Nf7.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_inf_report"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.InfiniteReportRange != null)
				{
					CheatConfig.InfiniteReportRange.Value = !CheatConfig.InfiniteReportRange.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_chat_paste"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.ChatAllowPaste != null)
				{
					CheatConfig.ChatAllowPaste.Value = !CheatConfig.ChatAllowPaste.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_endless_vent"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.EndlessVentTime != null)
				{
					CheatConfig.EndlessVentTime.Value = !CheatConfig.EndlessVentTime.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_no_vent_cd"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.NoVentCooldown != null)
				{
					CheatConfig.NoVentCooldown.Value = !CheatConfig.NoVentCooldown.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_endless_ss"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.EndlessShapeshiftDuration != null)
				{
					CheatConfig.EndlessShapeshiftDuration.Value = !CheatConfig.EndlessShapeshiftDuration.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_no_ss_cd"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.NoShapeshiftCooldown != null)
				{
					CheatConfig.NoShapeshiftCooldown.Value = !CheatConfig.NoShapeshiftCooldown.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_endless_bat"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.EndlessBattery != null)
				{
					CheatConfig.EndlessBattery.Value = !CheatConfig.EndlessBattery.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_no_vitals_cd"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.NoVitalsCooldown != null)
				{
					CheatConfig.NoVitalsCooldown.Value = !CheatConfig.NoVitalsCooldown.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_endless_track"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.EndlessTracking != null)
				{
					CheatConfig.EndlessTracking.Value = !CheatConfig.EndlessTracking.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_no_track_cd"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.NoTrackingCooldown != null)
				{
					CheatConfig.NoTrackingCooldown.Value = !CheatConfig.NoTrackingCooldown.Value;
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_kb_radar"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_pendingKeybind = "radar";
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_kb_freecam"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_pendingKeybind = "freecam";
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_kb_noclip"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_pendingKeybind = "noclip";
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_kb_tracers"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_pendingKeybind = "tracers";
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_kb_ghosts"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_pendingKeybind = "ghosts";
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_kb_nokillcd"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_pendingKeybind = "nokillcd";
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_unlock_hats"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.UnlockHats != null)
				{
					CheatConfig.UnlockHats.Value = !CheatConfig.UnlockHats.Value;
				}
				CosmeticsUnlockPatch.TriggerRefresh();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_unlock_skins"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.UnlockSkins != null)
				{
					CheatConfig.UnlockSkins.Value = !CheatConfig.UnlockSkins.Value;
				}
				CosmeticsUnlockPatch.TriggerRefresh();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_unlock_pets"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.UnlockPets != null)
				{
					CheatConfig.UnlockPets.Value = !CheatConfig.UnlockPets.Value;
				}
				CosmeticsUnlockPatch.TriggerRefresh();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_unlock_visors"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.UnlockVisors != null)
				{
					CheatConfig.UnlockVisors.Value = !CheatConfig.UnlockVisors.Value;
				}
				CosmeticsUnlockPatch.TriggerRefresh();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_unlock_nameplates"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.UnlockNameplates != null)
				{
					CheatConfig.UnlockNameplates.Value = !CheatConfig.UnlockNameplates.Value;
				}
				CosmeticsUnlockPatch.TriggerRefresh();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_unlock_bundles"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.UnlockBundles != null)
				{
					CheatConfig.UnlockBundles.Value = !CheatConfig.UnlockBundles.Value;
				}
				CosmeticsUnlockPatch.TriggerRefresh();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
		registry["stg_unlock_stars"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.UnlockStarsBeans != null)
				{
					CheatConfig.UnlockStarsBeans.Value = !CheatConfig.UnlockStarsBeans.Value;
				}
				CosmeticsUnlockPatch.TriggerRefresh();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				ScheduleSave();
			}
		};
	}

	private static void ScheduleSave()
	{
		_savePending = true;
		_pendingSaveTime = Time.time + 1.5f;
	}

	internal static void UpdateAutoSave()
	{
		if (!_savePending || !(Time.time >= _pendingSaveTime))
		{
			return;
		}
		_savePending = false;
		try
		{
			ConfigFile config = LobbyHarmonyPatches.Config;
			if (config != null)
			{
				config.Save();
			}
		}
		catch
		{
		}
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

	public void DrawSettingsTab()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!ServerData.IsTabEnabled("settings"))
		{
			return;
		}
		if (_pendingKeybind != null)
		{
			Event current = Event.current;
			if (current != null && current.isKey && (int)current.type == 4)
			{
				if ((int)current.keyCode == 27)
				{
					_pendingKeybind = null;
				}
				else if ((int)current.keyCode == 8)
				{
					SetKeybind(_pendingKeybind, (KeyCode)0);
					_pendingKeybind = null;
				}
				else if ((int)current.keyCode != 0)
				{
					SetKeybind(_pendingKeybind, current.keyCode);
					_pendingKeybind = null;
				}
				current.Use();
				return;
			}
		}
		ServerData.UISnapshot currentSnapshot = ServerData.CurrentSnapshot;
		if (currentSnapshot == null)
		{
			return;
		}
		byte[] settingsBytecode = currentSnapshot.SettingsBytecode;
		if (settingsBytecode == null || settingsBytecode.Length < 10)
		{
			return;
		}
		ServerData.SetSliderValueInternal("stg_opacity", CheatConfig.MenuOpacity?.Value ?? 0.95f);
		ServerData.SetSliderValueInternal("stg_width", CheatConfig.MenuWidth?.Value ?? 500f);
		ServerData.SetSliderValueInternal("stg_height", CheatConfig.MenuHeight?.Value ?? 600f);
		ServerData.SetSliderValueInternal("stg_vision", CheatConfig.VisionMultiplier);
		ServerData.SetSliderValueInternal("stg_radar_zoom", CheatConfig.RadarScale?.Value ?? 0.08f);
		ServerData.SetToggleStateInternal("stg_webradar", CheatConfig.WebRadarEnabled?.Value ?? false);
		long sessionToken = currentSnapshot.SessionToken;
		if (settingsBytecode != _cachedSTGBytecode)
		{
			_cachedSTGBytecode = settingsBytecode;
			_cachedSTGInverseMap = ((settingsBytecode.Length >= 524) ? new byte[256] : null);
			if (_cachedSTGInverseMap != null)
			{
				Array.Copy(settingsBytecode, 268, _cachedSTGInverseMap, 0, 256);
			}
			_cachedSTGToken = ((settingsBytecode.Length >= 268) ? BitConverter.ToInt64(settingsBytecode, 260) : sessionToken);
		}
		GhostUI.Execute(settingsBytecode, _cachedSTGToken, GetActionRegistry(), _cachedSTGInverseMap);
	}

	internal static object GetSettingsState()
	{
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			bool rgbLobbyCode = LobbyHarmonyPatches.cfgRgbLobbyCode?.Value ?? false;
			bool lobbyMusic = !(LobbyHarmonyPatches.cfgDisableLobbyMusic?.Value ?? false);
			float menuOpacity = CheatConfig.MenuOpacity?.Value ?? 0.95f;
			bool hideMMCStar = CheatConfig.HideMMCStar?.Value ?? false;
			float menuWidth = CheatConfig.MenuWidth?.Value ?? 500f;
			float menuHeight = CheatConfig.MenuHeight?.Value ?? 600f;
			bool streamerMode = LobbyHarmonyPatches.cfgStreamerMode?.Value ?? false;
			string customCode = LobbyHarmonyPatches.cfgCustomCode?.Value ?? "SECRET";
			bool lobbyTimer = LobbyHarmonyPatches.cfgShowLobbyTimer?.Value ?? false;
			bool autoExtend = LobbyHarmonyPatches.cfgAutoExtendTimer?.Value ?? false;
			bool extraInfo = LobbyHarmonyPatches.cfgShowLobbyInfo?.Value ?? false;
			bool showPlayerInfo = CheatConfig.ShowPlayerInfo?.Value ?? false;
			bool seeGhosts = CheatConfig.SeeGhosts?.Value ?? false;
			bool seeDeadChat = CheatConfig.SeeDeadChat?.Value ?? false;
			bool freeCam = CheatConfig.FreeCamEnabled?.Value ?? false;
			bool noClip = CheatConfig.NoClipSmoothEnabled?.Value ?? false;
			float visionMultiplier = CheatConfig.VisionMultiplier;
			bool lockScrollZoom = CheatConfig.LockScrollZoom?.Value ?? false;
			bool radarEnabled = CheatConfig.RadarEnabled?.Value ?? false;
			bool webRadarEnabled = CheatConfig.WebRadarEnabled?.Value ?? false;
			bool tracersEnabled = CheatConfig.TracersEnabled?.Value ?? false;
			float radarScale = CheatConfig.RadarScale?.Value ?? 0.08f;
			bool radarShowMap = CheatConfig.RadarShowMapImage?.Value ?? true;
			bool allowVenting = CheatConfig.AllowVenting?.Value ?? false;
			bool walkInVent = CheatConfig.WalkInVent?.Value ?? false;
			bool teleportCursor = CheatConfig.TeleportWithCursor?.Value ?? false;
			bool noKillCooldown = CheatConfig.NoKillCooldown?.Value ?? false;
			bool forceImpostor = CheatConfig.ForceImpostor?.Value ?? false;
			bool disableGameEnd = CheatConfig.DisableGameEnd?.Value ?? false;
			bool revealVotes = CheatConfig.RevealVotes?.Value ?? false;
			bool crewSabotage = CheatConfig.CrewmateSabotage?.Value ?? false;
			bool multiSabotage = CheatConfig.MultiSabotage?.Value ?? false;
			bool nf4V = CheatConfig.Nf4?.Value ?? true;
			bool nf7V = CheatConfig.Nf7?.Value ?? true;
			bool infiniteReportRange = CheatConfig.InfiniteReportRange?.Value ?? false;
			bool chatAllowPaste = CheatConfig.ChatAllowPaste?.Value ?? false;
			bool endlessVentTime = CheatConfig.EndlessVentTime?.Value ?? false;
			bool noVentCooldown = CheatConfig.NoVentCooldown?.Value ?? false;
			bool endlessShapeshift = CheatConfig.EndlessShapeshiftDuration?.Value ?? false;
			bool noShapeshiftCD = CheatConfig.NoShapeshiftCooldown?.Value ?? false;
			bool endlessBattery = CheatConfig.EndlessBattery?.Value ?? false;
			bool noVitalsCD = CheatConfig.NoVitalsCooldown?.Value ?? false;
			bool endlessTracking = CheatConfig.EndlessTracking?.Value ?? false;
			bool noTrackingCD = CheatConfig.NoTrackingCooldown?.Value ?? false;
			bool showAppearance = _showAppearance;
			bool showRadarVision = _showRadarVision;
			bool showGameplay = _showGameplay;
			bool showHostLobby = _showHostLobby;
			bool showRoles = _showRoles;
			bool showKeybinds = _showKeybinds;
			bool showCosmetics = _showCosmetics;
			bool pinMode = _pinMode;
			string pinnedKeys = CheatConfig.PinnedSettings?.Value ?? "";
			int activeIndex = ThemePresets.ActiveIndex;
			bool unlockHats = CheatConfig.UnlockHats?.Value ?? true;
			bool unlockSkins = CheatConfig.UnlockSkins?.Value ?? true;
			bool unlockPets = CheatConfig.UnlockPets?.Value ?? true;
			bool unlockVisors = CheatConfig.UnlockVisors?.Value ?? true;
			bool unlockNameplates = CheatConfig.UnlockNameplates?.Value ?? true;
			bool unlockBundles = CheatConfig.UnlockBundles?.Value ?? true;
			bool unlockStarsBeans = CheatConfig.UnlockStarsBeans?.Value ?? true;
			ConfigEntry<KeyCode> keybindRadar = CheatConfig.KeybindRadar;
			object obj;
			KeyCode value;
			if (keybindRadar == null)
			{
				obj = null;
			}
			else
			{
				value = keybindRadar.Value;
				obj = ((object)(KeyCode)(ref value)).ToString();
			}
			if (obj == null)
			{
				obj = "None";
			}
			ConfigEntry<KeyCode> keybindFreeCam = CheatConfig.KeybindFreeCam;
			object obj2;
			if (keybindFreeCam == null)
			{
				obj2 = null;
			}
			else
			{
				value = keybindFreeCam.Value;
				obj2 = ((object)(KeyCode)(ref value)).ToString();
			}
			if (obj2 == null)
			{
				obj2 = "None";
			}
			ConfigEntry<KeyCode> keybindNoClip = CheatConfig.KeybindNoClip;
			object obj3;
			if (keybindNoClip == null)
			{
				obj3 = null;
			}
			else
			{
				value = keybindNoClip.Value;
				obj3 = ((object)(KeyCode)(ref value)).ToString();
			}
			if (obj3 == null)
			{
				obj3 = "None";
			}
			ConfigEntry<KeyCode> keybindTracers = CheatConfig.KeybindTracers;
			object obj4;
			if (keybindTracers == null)
			{
				obj4 = null;
			}
			else
			{
				value = keybindTracers.Value;
				obj4 = ((object)(KeyCode)(ref value)).ToString();
			}
			if (obj4 == null)
			{
				obj4 = "None";
			}
			ConfigEntry<KeyCode> keybindSeeGhosts = CheatConfig.KeybindSeeGhosts;
			object obj5;
			if (keybindSeeGhosts == null)
			{
				obj5 = null;
			}
			else
			{
				value = keybindSeeGhosts.Value;
				obj5 = ((object)(KeyCode)(ref value)).ToString();
			}
			if (obj5 == null)
			{
				obj5 = "None";
			}
			ConfigEntry<KeyCode> keybindNoKillCooldown = CheatConfig.KeybindNoKillCooldown;
			object obj6;
			if (keybindNoKillCooldown == null)
			{
				obj6 = null;
			}
			else
			{
				value = keybindNoKillCooldown.Value;
				obj6 = ((object)(KeyCode)(ref value)).ToString();
			}
			if (obj6 == null)
			{
				obj6 = "None";
			}
			ConfigEntry<KeyCode> menuToggleKey = CheatConfig.MenuToggleKey;
			object obj7;
			if (menuToggleKey == null)
			{
				obj7 = null;
			}
			else
			{
				value = menuToggleKey.Value;
				obj7 = ((object)(KeyCode)(ref value)).ToString();
			}
			if (obj7 == null)
			{
				obj7 = "Delete";
			}
			return new
			{
				rgbLobbyCode = rgbLobbyCode,
				lobbyMusic = lobbyMusic,
				menuOpacity = menuOpacity,
				hideMMCStar = hideMMCStar,
				menuWidth = menuWidth,
				menuHeight = menuHeight,
				streamerMode = streamerMode,
				customCode = customCode,
				lobbyTimer = lobbyTimer,
				autoExtend = autoExtend,
				extraInfo = extraInfo,
				showPlayerInfo = showPlayerInfo,
				seeGhosts = seeGhosts,
				seeDeadChat = seeDeadChat,
				freeCam = freeCam,
				noClip = noClip,
				visionMultiplier = visionMultiplier,
				lockScrollZoom = lockScrollZoom,
				radarEnabled = radarEnabled,
				webRadarEnabled = webRadarEnabled,
				tracersEnabled = tracersEnabled,
				radarScale = radarScale,
				radarShowMap = radarShowMap,
				allowVenting = allowVenting,
				walkInVent = walkInVent,
				teleportCursor = teleportCursor,
				noKillCooldown = noKillCooldown,
				forceImpostor = forceImpostor,
				disableGameEnd = disableGameEnd,
				revealVotes = revealVotes,
				crewSabotage = crewSabotage,
				multiSabotage = multiSabotage,
				_nf4V = nf4V,
				_nf7V = nf7V,
				infiniteReportRange = infiniteReportRange,
				chatAllowPaste = chatAllowPaste,
				endlessVentTime = endlessVentTime,
				noVentCooldown = noVentCooldown,
				endlessShapeshift = endlessShapeshift,
				noShapeshiftCD = noShapeshiftCD,
				endlessBattery = endlessBattery,
				noVitalsCD = noVitalsCD,
				endlessTracking = endlessTracking,
				noTrackingCD = noTrackingCD,
				showAppearance = showAppearance,
				showRadarVision = showRadarVision,
				showGameplay = showGameplay,
				showHostLobby = showHostLobby,
				showRoles = showRoles,
				showKeybinds = showKeybinds,
				showCosmetics = showCosmetics,
				pinMode = pinMode,
				pinnedKeys = pinnedKeys,
				themeIndex = activeIndex,
				unlockHats = unlockHats,
				unlockSkins = unlockSkins,
				unlockPets = unlockPets,
				unlockVisors = unlockVisors,
				unlockNameplates = unlockNameplates,
				unlockBundles = unlockBundles,
				unlockStarsBeans = unlockStarsBeans,
				kbRadar = (string)obj,
				kbFreeCam = (string)obj2,
				kbNoClip = (string)obj3,
				kbTracers = (string)obj4,
				kbGhosts = (string)obj5,
				kbNoKillCD = (string)obj6,
				menuToggleKey = (string)obj7,
				pendingKeybind = (_pendingKeybind ?? ""),
				screenWidth = Screen.width,
				screenHeight = Screen.height,
				isFullscreen = Screen.fullScreen,
				detectedScale = GuiStyles.Spacing.Scale,
				confirmEjects = GameCheats.GetGameBool(3),
				anonVotes = GameCheats.GetGameBool(4),
				visualTasks = GameCheats.GetGameBool(1),
				ghostsDoTasks = GameCheats.GetGameBool(2),
				impsCanSeeProtect = GameCheats.GetGameBool(1100),
				noiseImpostorAlert = GameCheats.GetGameBool(1300),
				shapeLeavesSkin = GameCheats.GetGameBool(1000)
			};
		}
		catch
		{
			return new { };
		}
	}

	private static void ApplySmartSize()
	{
		SetInterfaceSize(550f, 650f);
		GhostUI.CenterWindow();
		NotifyUtils.Info($"Auto-size: {550f:0}x{650f:0} (GUI units) for {Screen.width}x{Screen.height}");
	}

	private static void SetInterfaceSize(float width, float height)
	{
		if (CheatConfig.MenuWidth != null)
		{
			CheatConfig.MenuWidth.Value = width;
		}
		if (CheatConfig.MenuHeight != null)
		{
			CheatConfig.MenuHeight.Value = height;
		}
		GhostUI.SetWindowSize(width, height);
		NotifyUtils.Info($"Interface size: {width}x{height}");
		PlayerPickMenu.TriggerRealtimeUpdate(force: true);
	}

	private static void SetKeybind(string id, KeyCode key)
	{
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case "radar":
			if (CheatConfig.KeybindRadar != null)
			{
				CheatConfig.KeybindRadar.Value = key;
			}
			break;
		case "freecam":
			if (CheatConfig.KeybindFreeCam != null)
			{
				CheatConfig.KeybindFreeCam.Value = key;
			}
			break;
		case "noclip":
			if (CheatConfig.KeybindNoClip != null)
			{
				CheatConfig.KeybindNoClip.Value = key;
			}
			break;
		case "tracers":
			if (CheatConfig.KeybindTracers != null)
			{
				CheatConfig.KeybindTracers.Value = key;
			}
			break;
		case "ghosts":
			if (CheatConfig.KeybindSeeGhosts != null)
			{
				CheatConfig.KeybindSeeGhosts.Value = key;
			}
			break;
		case "nokillcd":
			if (CheatConfig.KeybindNoKillCooldown != null)
			{
				CheatConfig.KeybindNoKillCooldown.Value = key;
			}
			break;
		case "menukey":
			if (CheatConfig.MenuToggleKey != null)
			{
				CheatConfig.MenuToggleKey.Value = key;
			}
			break;
		}
		CheatConfig.Save();
		NotifyUtils.Info($"Keybind set: {key}");
		PlayerPickMenu.TriggerRealtimeUpdate(force: true);
	}

	private static void ResetAllToDefaults()
	{
		if (LobbyHarmonyPatches.Config == null)
		{
			return;
		}
		foreach (ConfigDefinition key in LobbyHarmonyPatches.Config.Keys)
		{
			LobbyHarmonyPatches.Config[key].BoxedValue = LobbyHarmonyPatches.Config[key].DefaultValue;
		}
		LobbyHarmonyPatches.Config.Save();
	}

	private static HashSet<string> GetPinnedSet()
	{
		HashSet<string> hashSet = new HashSet<string>();
		string[] array = (CheatConfig.PinnedSettings?.Value ?? "").Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (!string.IsNullOrEmpty(text))
			{
				hashSet.Add(text);
			}
		}
		return hashSet;
	}

	private static void TogglePin(string key)
	{
		if (CheatConfig.PinnedSettings == null)
		{
			return;
		}
		HashSet<string> pinnedSet = GetPinnedSet();
		if (pinnedSet.Contains(key))
		{
			pinnedSet.Remove(key);
		}
		else
		{
			pinnedSet.Add(key);
		}
		List<string> list = new List<string>();
		string[] pINNABLE_KEYS = PINNABLE_KEYS;
		foreach (string item in pINNABLE_KEYS)
		{
			if (pinnedSet.Contains(item))
			{
				list.Add(item);
			}
		}
		CheatConfig.PinnedSettings.Value = string.Join(",", list);
	}

	private static void SetBool(ConfigEntry<bool> entry, bool value)
	{
		if (entry != null)
		{
			entry.Value = value;
		}
	}

	private static void ApplyPresetStreamer()
	{
		SetBool(LobbyHarmonyPatches.cfgStreamerMode, value: true);
		SetBool(CheatConfig.HideMMCStar, value: true);
		SetBool(LobbyHarmonyPatches.cfgRgbLobbyCode, value: false);
		SetBool(CheatConfig.ShowPlayerInfo, value: false);
		NotifyUtils.Info("Preset applied: \ud83c\udfa5 Streamer");
		PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		ScheduleSave();
	}

	private static void ApplyPresetObserver()
	{
		SetBool(CheatConfig.RadarEnabled, value: true);
		SetBool(CheatConfig.TracersEnabled, value: true);
		SetBool(CheatConfig.WebRadarEnabled, value: true);
		SetBool(CheatConfig.SeeGhosts, value: true);
		SetBool(CheatConfig.SeeDeadChat, value: true);
		SetBool(CheatConfig.RevealVotes, value: true);
		SetBool(CheatConfig.ShowPlayerInfo, value: true);
		SetBool(CheatConfig.RadarShowMapImage, value: true);
		ConfigEntry<bool> webRadarEnabled = CheatConfig.WebRadarEnabled;
		if (webRadarEnabled != null && webRadarEnabled.Value && !WebRadarService.IsRunning)
		{
			WebRadarService.Start();
		}
		NotifyUtils.Info("Preset applied: \ud83d\udc41 Observer");
		PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		ScheduleSave();
	}

	private static void ApplyPresetCompetitive()
	{
		SetBool(CheatConfig.NoKillCooldown, value: false);
		SetBool(CheatConfig.ForceImpostor, value: false);
		SetBool(CheatConfig.DisableGameEnd, value: false);
		SetBool(CheatConfig.RadarEnabled, value: false);
		SetBool(CheatConfig.TracersEnabled, value: false);
		SetBool(CheatConfig.WebRadarEnabled, value: false);
		SetBool(CheatConfig.NoClipSmoothEnabled, value: false);
		SetBool(CheatConfig.TeleportWithCursor, value: false);
		SetBool(CheatConfig.MultiSabotage, value: false);
		SetBool(CheatConfig.CrewmateSabotage, value: false);
		SetBool(CheatConfig.InfiniteReportRange, value: false);
		SetBool(CheatConfig.SeeGhosts, value: false);
		SetBool(CheatConfig.SeeDeadChat, value: false);
		ImpostorForcer.SetRoleOverrideEnabled(enabled: false);
		ConfigEntry<bool> webRadarEnabled = CheatConfig.WebRadarEnabled;
		if (webRadarEnabled != null && !webRadarEnabled.Value && WebRadarService.IsRunning)
		{
			WebRadarService.Stop();
		}
		NotifyUtils.Info("Preset applied: \ud83c\udfc6 Competitive");
		PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		ScheduleSave();
	}

	private static void ApplyPresetChaos()
	{
		SetBool(CheatConfig.NoKillCooldown, value: true);
		SetBool(CheatConfig.NoKillDistanceLimit, value: true);
		GameCheats.NoKillDistanceLimitEnabled = true;
		SetBool(CheatConfig.MultiSabotage, value: true);
		SetBool(CheatConfig.CrewmateSabotage, value: true);
		SetBool(CheatConfig.InfiniteReportRange, value: true);
		SetBool(CheatConfig.TeleportWithCursor, value: true);
		SetBool(CheatConfig.AllowVenting, value: true);
		SetBool(CheatConfig.WalkInVent, value: true);
		NotifyUtils.Info("Preset applied: \ud83d\udc80 Chaos");
		PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		ScheduleSave();
	}

	private static void ApplyPresetHost()
	{
		SetBool(CheatConfig.ForceImpostor, value: true);
		ImpostorForcer.SetRoleOverrideEnabled(enabled: true);
		ImpostorForcer.SetSelectedRoleForHost((RoleTypes)1);
		SetBool(CheatConfig.DisableGameEnd, value: true);
		SetBool(LobbyHarmonyPatches.cfgShowLobbyTimer, value: true);
		SetBool(LobbyHarmonyPatches.cfgAutoExtendTimer, value: true);
		SetBool(LobbyHarmonyPatches.cfgShowLobbyInfo, value: true);
		SetBool(CheatConfig.ShowPlayerInfo, value: true);
		SetBool(CheatConfig.RevealVotes, value: true);
		NotifyUtils.Info("Preset applied: \ud83d\udc51 Host Power");
		PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		ScheduleSave();
	}

	private static void ApplyPresetRoles()
	{
		SetBool(CheatConfig.EndlessVentTime, value: true);
		SetBool(CheatConfig.NoVentCooldown, value: true);
		SetBool(CheatConfig.EndlessShapeshiftDuration, value: true);
		SetBool(CheatConfig.NoShapeshiftCooldown, value: true);
		SetBool(CheatConfig.EndlessBattery, value: true);
		SetBool(CheatConfig.NoVitalsCooldown, value: true);
		SetBool(CheatConfig.EndlessTracking, value: true);
		SetBool(CheatConfig.NoTrackingCooldown, value: true);
		NotifyUtils.Info("Preset applied: \ud83d\udd27 Role Cheats");
		PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		ScheduleSave();
	}
}
