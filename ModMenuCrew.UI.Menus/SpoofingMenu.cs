using System;
using System.Collections.Generic;
using ModMenuCrew.Features;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew.UI.Menus;

public class SpoofingMenu
{
	[Serializable]
	internal class SpoofingStateDto
	{
		public bool le { get; set; }

		public bool pe { get; set; }

		public bool fe { get; set; }

		public int cl { get; set; }

		public string cp { get; set; } = "Steam";


		public string fc { get; set; } = "";


		public int ac { get; set; }

		public bool sn { get; set; }

		public bool sc { get; set; }

		public bool sco { get; set; }
	}

	private static Dictionary<string, Action<long>> _actionRegistry;

	private ServerData.UISnapshot _safeSnapshot;

	private byte[] _cachedSPBytecode;

	private byte[] _cachedSPInverseMap;

	private long _cachedSPToken;

	private static readonly SpoofingStateDto _spoofingDto = new SpoofingStateDto();

	internal static void RegisterActions(Dictionary<string, Action<long>> registry)
	{
		if (registry == null)
		{
			return;
		}
		registry["sp_level_toggle"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.EnableLevelSpoof = !SpoofingService.EnableLevelSpoof;
				if (SpoofingService.EnableLevelSpoof)
				{
					SpoofingService.MarkForReapplication();
					SpoofingService.ApplyLevelSpoof();
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_level_slider"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SetLevel((uint)Mathf.Clamp(Mathf.RoundToInt(ServerData.GetSliderValue("sp_level_slider")), 1, 10000));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		};
		int[] array = new int[8] { 1, 10, 50, 100, 500, 1000, 5000, 10000 };
		foreach (int num in array)
		{
			uint lvl = (uint)num;
			registry[$"sp_level_{num}"] = delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					SpoofingService.SetLevel(lvl);
					ServerData.SetSliderValueInternal("sp_level_slider", lvl);
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			};
		}
		registry["sp_platform_toggle"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.EnablePlatformSpoof = !SpoofingService.EnablePlatformSpoof;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_plat_steam"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SetPlatform((Platforms)2);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_plat_epic"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SetPlatform((Platforms)1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_plat_msstore"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SetPlatform((Platforms)4);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_plat_playstation"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SetPlatform((Platforms)10);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_plat_xbox"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SetPlatform((Platforms)9);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_plat_switch"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SetPlatform((Platforms)8);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_plat_android"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SetPlatform((Platforms)7);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_plat_iphone"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SetPlatform((Platforms)6);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_plat_mac"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SetPlatform((Platforms)3);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_plat_itch"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SetPlatform((Platforms)5);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_fc_toggle"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.EnableFriendCodeSpoof = !SpoofingService.EnableFriendCodeSpoof;
				if (SpoofingService.EnableFriendCodeSpoof)
				{
					SpoofingService.ApplyFriendCodeSpoof();
				}
				else
				{
					SpoofingService.RestoreFriendCode();
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_fc_randomize"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.SpoofedFriendCode = SpoofingService.GenerateRandomFriendCode();
				if (SpoofingService.EnableFriendCodeSpoof)
				{
					SpoofingService.ApplyFriendCodeSpoof();
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_shuffle_name"] = delegate
		{
		};
		registry["sp_shuffle_color"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.EnableShuffleColor = !SpoofingService.EnableShuffleColor;
				if (!SpoofingService.EnableShuffleColor && !SpoofingService.IsAnyShuffleEnabled())
				{
					SpoofingService.RestoreIdentity();
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_shuffle_cosmetics"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.EnableShuffleCosmetics = !SpoofingService.EnableShuffleCosmetics;
				if (!SpoofingService.EnableShuffleCosmetics && !SpoofingService.IsAnyShuffleEnabled())
				{
					SpoofingService.RestoreIdentity();
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["sp_disable_all"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				SpoofingService.DisableAll();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
	}

	private static Dictionary<string, Action<long>> GetActionRegistry()
	{
		if (_actionRegistry == null)
		{
			_actionRegistry = new Dictionary<string, Action<long>>();
			RegisterActions(_actionRegistry);
			ServerData.SetSliderValueInternal("sp_level_slider", SpoofingService.GetEffectiveLevel());
		}
		return _actionRegistry;
	}

	public void Draw()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)Event.current.type == 8)
		{
			PlayerPickMenu.CheckRealtimeUpdate();
			_safeSnapshot = ServerData.CurrentSnapshot;
		}
		ServerData.UISnapshot safeSnapshot = _safeSnapshot;
		if (safeSnapshot?.SpoofingBytecode != null && safeSnapshot.SpoofingBytecode.Length != 0)
		{
			if (_cachedSPBytecode != safeSnapshot.SpoofingBytecode)
			{
				_cachedSPBytecode = safeSnapshot.SpoofingBytecode;
				_cachedSPInverseMap = ((safeSnapshot.SpoofingBytecode.Length >= 524) ? new byte[256] : null);
				if (_cachedSPInverseMap != null)
				{
					Array.Copy(safeSnapshot.SpoofingBytecode, 268, _cachedSPInverseMap, 0, 256);
				}
				_cachedSPToken = ((safeSnapshot.SpoofingBytecode.Length >= 268) ? BitConverter.ToInt64(safeSnapshot.SpoofingBytecode, 260) : safeSnapshot.SessionToken);
			}
			GhostUI.Execute(safeSnapshot.SpoofingBytecode, _cachedSPToken, GetActionRegistry(), _cachedSPInverseMap);
		}
		else
		{
			GUILayout.Label("<color=#949EAD>Loading from server...</color>", GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(50f) });
		}
	}

	internal static object GetSpoofingState()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			_spoofingDto.le = SpoofingService.EnableLevelSpoof;
			_spoofingDto.pe = SpoofingService.EnablePlatformSpoof;
			_spoofingDto.fe = SpoofingService.EnableFriendCodeSpoof;
			_spoofingDto.cl = (int)SpoofingService.GetEffectiveLevel();
			_spoofingDto.cp = SpoofingService.PlatformToString(SpoofingService.SpoofedPlatform);
			_spoofingDto.fc = SpoofingService.SpoofedFriendCode;
			_spoofingDto.ac = SpoofingService.GetActiveCount();
			_spoofingDto.sn = SpoofingService.EnableShuffleName;
			_spoofingDto.sc = SpoofingService.EnableShuffleColor;
			_spoofingDto.sco = SpoofingService.EnableShuffleCosmetics;
			return _spoofingDto;
		}
		catch
		{
			_spoofingDto.le = false;
			_spoofingDto.pe = false;
			_spoofingDto.fe = false;
			_spoofingDto.cl = 100;
			_spoofingDto.cp = "Steam";
			_spoofingDto.fc = "";
			_spoofingDto.ac = 0;
			_spoofingDto.sn = false;
			_spoofingDto.sc = false;
			_spoofingDto.sco = false;
			return _spoofingDto;
		}
	}
}
