using System;
using System.Collections.Generic;
using System.Text;
using AmongUs.Data;
using AmongUs.Data.Player;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using ModMenuCrew.Features;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew.UI.Menus;

public class BanMenu
{
	[Serializable]
	internal class BanMenuStateDto
	{
		public bool h { get; set; }

		public bool g { get; set; }

		public float bp { get; set; }

		public int bm { get; set; }

		public int pc { get; set; }

		public int st { get; set; }

		public int tz { get; set; }

		public int th { get; set; }

		public int tm { get; set; }

		public int gs { get; set; }

		public int gi { get; set; }

		public int gc { get; set; }

		public int ik { get; set; }

		public int td { get; set; }

		public int te { get; set; }

		public int cs { get; set; }

		public int tc { get; set; }

		public int sf { get; set; }

		public string pn { get; set; } = "";


		public string fc { get; set; } = "";


		public string rg { get; set; } = "";


		public bool gu { get; set; }

		public bool ar { get; set; }

		public string rc { get; set; } = "";


		public string mn { get; set; } = "";


		public int ic { get; set; }

		public int gm { get; set; }
	}

	[Serializable]
	internal class BanMenuUiDto
	{
		public bool da { get; set; }

		public bool db { get; set; }

		public int sa { get; set; }

		public int sb { get; set; }

		public bool ro { get; set; }

		public string rs { get; set; } = "Impostor";


		public bool al { get; set; }

		public bool an { get; set; }

		public int av { get; set; }

		public string nl { get; set; } = "";


		public int ns { get; set; }

		public string np { get; set; } = "";


		public bool ac { get; set; }

		public string cwl { get; set; } = "";


		public bool gs { get; set; }

		public bool hd { get; set; }

		public bool hm { get; set; }

		public bool hc { get; set; }

		public bool ha { get; set; }

		public int hms { get; set; }

		public int mpi { get; set; }

		public string mpn { get; set; } = "";


		public string mls { get; set; } = "";


		public int vpi { get; set; }

		public string vpn { get; set; } = "";


		public string vls { get; set; } = "";


		public bool hr { get; set; }

		public int scc { get; set; }

		public bool sco { get; set; }

		public int hst { get; set; }

		public string rls { get; set; } = "";

	}

	private ServerData.UISnapshot _safeSnapshot;

	private byte[] _cachedBMBytecode;

	private byte[] _cachedBMInverseMap;

	private long _cachedBMToken;

	private static bool _dropdownA = false;

	private static bool _dropdownB = false;

	private static int _selectorA = 0;

	private static int _selectorB = 0;

	private static int _autoKickNameSelector = 0;

	private static int _modPickerSelector = 0;

	private static int _vipPickerSelector = 0;

	private static bool _showSettings = false;

	private static int _hostSubTab = 0;

	private static Dictionary<string, Action<long>> _actionRegistry;

	private static readonly BanMenuStateDto _banMenuStateDto = new BanMenuStateDto();

	private static readonly BanMenuUiDto _banMenuUiDto = new BanMenuUiDto();

	public static bool DropdownA
	{
		get
		{
			return _dropdownA;
		}
		set
		{
			_dropdownA = value;
		}
	}

	public static bool DropdownB
	{
		get
		{
			return _dropdownB;
		}
		set
		{
			_dropdownB = value;
		}
	}

	public static int SelectorA
	{
		get
		{
			return _selectorA;
		}
		set
		{
			_selectorA = value;
		}
	}

	public static int SelectorB
	{
		get
		{
			return _selectorB;
		}
		set
		{
			_selectorB = value;
		}
	}

	public static int AutoKickNameSelector
	{
		get
		{
			return _autoKickNameSelector;
		}
		set
		{
			_autoKickNameSelector = value;
		}
	}

	public static int ModPickerSelector
	{
		get
		{
			return _modPickerSelector;
		}
		set
		{
			_modPickerSelector = value;
		}
	}

	public static int VipPickerSelector
	{
		get
		{
			return _vipPickerSelector;
		}
		set
		{
			_vipPickerSelector = value;
		}
	}

	public static bool ShowSettings
	{
		get
		{
			return _showSettings;
		}
		set
		{
			_showSettings = value;
		}
	}

	public static int HostSubTab
	{
		get
		{
			return _hostSubTab;
		}
		set
		{
			_hostSubTab = value;
		}
	}

	public static void RegisterActions(Dictionary<string, Action<long>> registry)
	{
		if (registry == null)
		{
			return;
		}
		for (int i = 0; i <= 200; i += 5)
		{
			int amt = i;
			registry[$"bp_add_{i}"] = guard(delegate(long t)
			{
				//IL_0065: Unknown result type (might be due to invalid IL or missing references)
				if (GhostUI.CheckToken(t))
				{
					PlayerData player6 = DataManager.Player;
					PlayerBanData val11 = ((player6 != null) ? player6.ban : null);
					if (val11 != null)
					{
						if (val11.BanPoints < 0f)
						{
							val11.BanPoints = 0f;
						}
						val11.BanPoints += (float)amt;
						if (val11.BanPoints > 0f)
						{
							val11.PreviousGameStartDate = new DateTime(DateTime.UtcNow.Ticks);
						}
						Action onBanPointsChanged4 = val11.OnBanPointsChanged;
						if (onBanPointsChanged4 != null)
						{
							onBanPointsChanged4.Invoke();
						}
						try
						{
							((AbstractSaveData)DataManager.Player).Save();
						}
						catch
						{
						}
					}
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			});
			registry[$"bp_sub_{i}"] = guard(delegate(long t)
			{
				//IL_0047: Unknown result type (might be due to invalid IL or missing references)
				if (GhostUI.CheckToken(t))
				{
					PlayerData player5 = DataManager.Player;
					PlayerBanData val10 = ((player5 != null) ? player5.ban : null);
					if (val10 != null)
					{
						val10.BanPoints = Mathf.Max(0f, val10.BanPoints - (float)amt);
						val10.PreviousGameStartDate = new DateTime(DateTime.MinValue.Ticks);
						Action onBanPointsChanged3 = val10.OnBanPointsChanged;
						if (onBanPointsChanged3 != null)
						{
							onBanPointsChanged3.Invoke();
						}
						try
						{
							((AbstractSaveData)DataManager.Player).Save();
						}
						catch
						{
						}
					}
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			});
		}
		registry["bp_clear"] = guard(delegate(long t)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			if (GhostUI.CheckToken(t))
			{
				PlayerData player4 = DataManager.Player;
				PlayerBanData val9 = ((player4 != null) ? player4.ban : null);
				if (val9 != null)
				{
					val9.BanPoints = 0f;
					val9.PreviousGameStartDate = new DateTime(DateTime.MinValue.Ticks);
					Action onBanPointsChanged2 = val9.OnBanPointsChanged;
					if (onBanPointsChanged2 != null)
					{
						onBanPointsChanged2.Invoke();
					}
					try
					{
						((AbstractSaveData)DataManager.Player).Save();
					}
					catch
					{
					}
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["bp_fix"] = guard(delegate(long t)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if (GhostUI.CheckToken(t))
			{
				PlayerData player3 = DataManager.Player;
				PlayerBanData val8 = ((player3 != null) ? player3.ban : null);
				if (val8 != null)
				{
					if (val8.BanPoints < 0f)
					{
						val8.BanPoints = 0f;
					}
					val8.BanPoints = 0f;
					val8.PreviousGameStartDate = new DateTime(DateTime.MinValue.Ticks);
					Action onBanPointsChanged = val8.OnBanPointsChanged;
					if (onBanPointsChanged != null)
					{
						onBanPointsChanged.Invoke();
					}
					try
					{
						((AbstractSaveData)DataManager.Player).Save();
					}
					catch
					{
					}
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["acc_guest"] = guard(delegate(long t)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Invalid comparison between Unknown and I4
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Invalid comparison between Unknown and I4
			if (GhostUI.CheckToken(t))
			{
				try
				{
					PlayerData player2 = DataManager.Player;
					PlayerAccountData val7 = ((player2 != null) ? player2.Account : null);
					if (val7 == null)
					{
						return;
					}
					val7.LoginStatus = (AccountLoginStatus)(((int)val7.LoginStatus == 3) ? 1 : 3);
					try
					{
						((AbstractSaveData)DataManager.Player).Save();
					}
					catch
					{
					}
					NotifyUtils.Info(((int)val7.LoginStatus == 3) ? "Guest Mode ON" : "Guest Mode OFF");
				}
				catch
				{
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["acc_ads"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				try
				{
					PlayerData player = DataManager.Player;
					PlayerAdsData val6 = ((player != null) ? player.Ads : null);
					if (val6 != null)
					{
						val6.HasPurchasedAdRemoval = true;
						try
						{
							((AbstractSaveData)DataManager.Player).Save();
						}
						catch
						{
						}
						NotifyUtils.Info("Ad removal applied!");
					}
				}
				catch
				{
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_start"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance30 = AmongUsClient.Instance;
				if (instance30 != null && ((InnerNetClient)instance30).AmHost && !((Object)(object)LobbyBehaviour.Instance == (Object)null) && !((Object)(object)ShipStatus.Instance != (Object)null))
				{
					GameCheats.InstantStartGame();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_rnd_col"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t) && ModKeyValidator.IsPremium && ModKeyValidator.V())
			{
				AmongUsClient instance29 = AmongUsClient.Instance;
				if (instance29 != null && ((InnerNetClient)instance29).AmHost)
				{
					GameCheats.HostRandomizeAllColors();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_rainbow_all"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t) && ModKeyValidator.IsPremium && ModKeyValidator.V())
			{
				AmongUsClient instance28 = AmongUsClient.Instance;
				if (instance28 != null && ((InnerNetClient)instance28).AmHost)
				{
					GameCheats.ToggleRainbow();
					NotifyUtils.Info(GameCheats.RainbowEnabled ? "Rainbow ON" : "Rainbow OFF");
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_col_p"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t) && ModKeyValidator.IsPremium && ModKeyValidator.V())
			{
				_selectorA = Math.Max(0, _selectorA - 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_col_n"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t) && ModKeyValidator.IsPremium && ModKeyValidator.V())
			{
				_selectorA = Math.Min((((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors)?.Length ?? 18) - 1, _selectorA + 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_col_apply"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t) && ModKeyValidator.IsPremium && ModKeyValidator.V())
			{
				AmongUsClient instance27 = AmongUsClient.Instance;
				if (instance27 != null && ((InnerNetClient)instance27).AmHost)
				{
					GameCheats.HostSetAllPlayersColor(_selectorA);
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_map_rm"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance26 = AmongUsClient.Instance;
				if (instance26 != null && ((InnerNetClient)instance26).AmHost)
				{
					GameCheats.MapCheats.FullTeardown();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_map_lobby"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance25 = AmongUsClient.Instance;
				if (instance25 != null && ((InnerNetClient)instance25).AmHost)
				{
					GameCheats.MapCheats.SpawnLobby();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_dummy"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.ToggleDummy();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_dummy_clr"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.ForceClearDummies();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_mapov"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.TestSpawnMap();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_clone"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.ToggleCloneFollow();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_clone_clr"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.ForceClearClones();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_clone_cnt_p"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.ShadowClonesCount != null)
				{
					int value3 = CheatConfig.ShadowClonesCount.Value;
					int num18 = Math.Max(1, value3 - 1);
					CheatConfig.ShadowClonesCount.Value = num18;
					if (num18 != value3)
					{
						GameCheats.ResyncClones();
					}
				}
				CheatConfig.Save();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_clone_cnt_n"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (CheatConfig.ShadowClonesCount != null)
				{
					int w = ServerData.Config.W35;
					if (w <= 0)
					{
						return;
					}
					int value2 = CheatConfig.ShadowClonesCount.Value;
					int num17 = Math.Min(w, value2 + 1);
					CheatConfig.ShadowClonesCount.Value = num17;
					if (num17 != value2)
					{
						GameCheats.ResyncClones();
					}
				}
				CheatConfig.Save();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_clone_orbit"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.ToggleCloneOrbit();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_maps_all"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.ToggleAllMaps();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_msel_lb"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.ClearSelMap();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_msel_sk"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SelectMapBySpawnId(ServerData.Config.M0, 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_msel_mr"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SelectMapBySpawnId(ServerData.Config.M1, 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_msel_pl"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SelectMapBySpawnId(ServerData.Config.M2, 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_msel_as"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SelectMapBySpawnId(ServerData.Config.M3, 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["ro_toggle"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				if (ImpostorForcer.IsAnyOverrideActive)
				{
					ImpostorForcer.SetRoleOverrideEnabled(enabled: false);
					ImpostorForcer.ClearAllPreGameRoleAssignments();
				}
				else
				{
					ImpostorForcer.SetRoleOverrideEnabled(enabled: true);
				}
				_dropdownA = false;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["ro_dd"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_dropdownA = !_dropdownA;
				_dropdownB = false;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		for (int j = 0; j <= 15; j++)
		{
			int idx7 = j;
			registry[$"ro_sel_{j}"] = guard(delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					AmongUsClient instance24 = AmongUsClient.Instance;
					if (instance24 != null && ((InnerNetClient)instance24).AmHost)
					{
						RoleTypes[] supportedRoles = ImpostorForcer.GetSupportedRoles();
						if (idx7 >= 0 && idx7 < supportedRoles.Length)
						{
							ImpostorForcer.SetSelectedRoleForHost(supportedRoles[idx7]);
						}
						_dropdownA = false;
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					}
				}
			});
		}
		registry["bt_dd"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_dropdownB = !_dropdownB;
				_dropdownA = false;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		for (int k = 0; k <= 5; k++)
		{
			int idx6 = k;
			registry[$"bt_sel_{k}"] = guard(delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					AmongUsClient instance23 = AmongUsClient.Instance;
					if (instance23 != null && ((InnerNetClient)instance23).AmHost)
					{
						_selectorB = idx6;
						PlayerBodyTypes[] availableTypes = GameCheats.BodyTypeCheats.AvailableTypes;
						if (idx6 >= 0 && idx6 < availableTypes.Length)
						{
							GameCheats.BodyTypeCheats.CurrentTypeIndex = idx6;
							GameCheats.BodyTypeCheats.SetAllPlayersBodyType(availableTypes[idx6]);
						}
						_dropdownB = false;
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					}
				}
			});
		}
		registry["h_ak_level"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance22 = AmongUsClient.Instance;
				if (instance22 != null && ((InnerNetClient)instance22).AmHost)
				{
					CheatConfig.AutoKickByLevelEnabled.Value = !CheatConfig.AutoKickByLevelEnabled.Value;
					CheatConfig.Save();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_ak_lvl_up"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance21 = AmongUsClient.Instance;
				if (instance21 != null && ((InnerNetClient)instance21).AmHost)
				{
					CheatConfig.AutoKickMinLevel.Value = Mathf.Min(100, CheatConfig.AutoKickMinLevel.Value + 1);
					CheatConfig.Save();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_ak_lvl_dn"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance20 = AmongUsClient.Instance;
				if (instance20 != null && ((InnerNetClient)instance20).AmHost)
				{
					CheatConfig.AutoKickMinLevel.Value = Mathf.Max(1, CheatConfig.AutoKickMinLevel.Value - 1);
					CheatConfig.Save();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_ak_lvl_u5"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance19 = AmongUsClient.Instance;
				if (instance19 != null && ((InnerNetClient)instance19).AmHost)
				{
					CheatConfig.AutoKickMinLevel.Value = Mathf.Min(100, CheatConfig.AutoKickMinLevel.Value + 5);
					CheatConfig.Save();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_ak_lvl_d5"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance18 = AmongUsClient.Instance;
				if (instance18 != null && ((InnerNetClient)instance18).AmHost)
				{
					CheatConfig.AutoKickMinLevel.Value = Mathf.Max(1, CheatConfig.AutoKickMinLevel.Value - 5);
					CheatConfig.Save();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_ak_name"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance17 = AmongUsClient.Instance;
				if (instance17 != null && ((InnerNetClient)instance17).AmHost)
				{
					CheatConfig.AutoKickByNameEnabled.Value = !CheatConfig.AutoKickByNameEnabled.Value;
					CheatConfig.Save();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_ak_ns_p"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_autoKickNameSelector = Math.Max(0, _autoKickNameSelector - 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_ak_ns_n"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_autoKickNameSelector = Math.Min((PlayerControl.AllPlayerControls?.Count ?? 1) - 1, _autoKickNameSelector + 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_ak_name_add"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance16 = AmongUsClient.Instance;
				if (instance16 != null && ((InnerNetClient)instance16).AmHost)
				{
					try
					{
						List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
						if (allPlayerControls == null || _autoKickNameSelector >= allPlayerControls.Count)
						{
							return;
						}
						PlayerControl val5 = allPlayerControls[_autoKickNameSelector];
						if ((Object)(object)val5 == (Object)null || (Object)(object)val5 == (Object)(object)PlayerControl.LocalPlayer)
						{
							return;
						}
						NetworkedPlayerInfo data = val5.Data;
						string name = ((data != null) ? data.PlayerName : null);
						if (string.IsNullOrEmpty(name))
						{
							return;
						}
						List<string> list6 = new List<string>((CheatConfig.AutoKickNameList.Value ?? "").Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries));
						for (int num16 = 0; num16 < list6.Count; num16++)
						{
							list6[num16] = list6[num16].Trim();
						}
						if (!list6.Exists((string n) => string.Equals(n, name, StringComparison.OrdinalIgnoreCase)))
						{
							list6.Add(name);
							CheatConfig.AutoKickNameList.Value = string.Join(",", list6);
							CheatConfig.Save();
						}
					}
					catch
					{
					}
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_ak_name_clr"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance15 = AmongUsClient.Instance;
				if (instance15 != null && ((InnerNetClient)instance15).AmHost)
				{
					CheatConfig.AutoKickNameList.Value = "";
					CheatConfig.Save();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_ak_chat"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance14 = AmongUsClient.Instance;
				if (instance14 != null && ((InnerNetClient)instance14).AmHost)
				{
					if (CheatConfig.AutoKickByChatEnabled != null)
					{
						CheatConfig.AutoKickByChatEnabled.Value = !CheatConfig.AutoKickByChatEnabled.Value;
					}
					CheatConfig.Save();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_ak_cw_clr"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance13 = AmongUsClient.Instance;
				if (instance13 != null && ((InnerNetClient)instance13).AmHost)
				{
					if (CheatConfig.AutoKickChatWordList != null)
					{
						CheatConfig.AutoKickChatWordList.Value = "";
					}
					CheatConfig.Save();
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		registry["h_ak_cw_input"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance12 = AmongUsClient.Instance;
				if (instance12 != null && ((InnerNetClient)instance12).AmHost && CheatConfig.AutoKickChatWordList != null)
				{
					string typed = GhostUI.ConsumeStagedString("h_ak_cw_input");
					if (!string.IsNullOrWhiteSpace(typed))
					{
						typed = typed.Trim();
						if (typed.Length != 0 && typed.Length <= 32)
						{
							try
							{
								List<string> list5 = new List<string>((CheatConfig.AutoKickChatWordList.Value ?? "").Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries));
								for (int num15 = 0; num15 < list5.Count; num15++)
								{
									list5[num15] = list5[num15].Trim();
								}
								list5.RemoveAll(string.IsNullOrEmpty);
								if (list5.Exists((string x) => string.Equals(x, typed, StringComparison.OrdinalIgnoreCase)))
								{
									NotifyUtils.Info("Already blacklisted: " + typed);
								}
								else
								{
									list5.Add(typed);
									CheatConfig.AutoKickChatWordList.Value = string.Join(",", list5);
									CheatConfig.Save();
									NotifyUtils.Success("Added banned word: " + typed);
								}
							}
							catch
							{
							}
							PlayerPickMenu.TriggerRealtimeUpdate(force: true);
						}
					}
				}
			}
		});
		for (int l = 0; l < 20; l++)
		{
			int idx5 = l;
			registry[$"h_ak_cw_rm_{l}"] = guard(delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					AmongUsClient instance11 = AmongUsClient.Instance;
					if (instance11 != null && ((InnerNetClient)instance11).AmHost)
					{
						try
						{
							List<string> list4 = new List<string>((CheatConfig.AutoKickChatWordList?.Value ?? "").Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries));
							for (int num14 = 0; num14 < list4.Count; num14++)
							{
								list4[num14] = list4[num14].Trim();
							}
							list4.RemoveAll(string.IsNullOrEmpty);
							if (idx5 < 0 || idx5 >= list4.Count)
							{
								return;
							}
							string text6 = list4[idx5];
							list4.RemoveAt(idx5);
							if (CheatConfig.AutoKickChatWordList != null)
							{
								CheatConfig.AutoKickChatWordList.Value = string.Join(",", list4);
							}
							CheatConfig.Save();
							NotifyUtils.Info("Removed chat word: " + text6);
						}
						catch
						{
						}
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					}
				}
			});
		}
		for (int m = 0; m < 20; m++)
		{
			int idx4 = m;
			registry[$"h_ak_name_rm_{m}"] = guard(delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					AmongUsClient instance10 = AmongUsClient.Instance;
					if (instance10 != null && ((InnerNetClient)instance10).AmHost)
					{
						try
						{
							List<string> list3 = new List<string>((CheatConfig.AutoKickNameList.Value ?? "").Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries));
							for (int num13 = 0; num13 < list3.Count; num13++)
							{
								list3[num13] = list3[num13].Trim();
							}
							list3.RemoveAll(string.IsNullOrEmpty);
							if (idx4 < 0 || idx4 >= list3.Count)
							{
								return;
							}
							string text5 = list3[idx4];
							list3.RemoveAt(idx4);
							CheatConfig.AutoKickNameList.Value = string.Join(",", list3);
							CheatConfig.Save();
							NotifyUtils.Info("Removed from blacklist: " + text5);
						}
						catch
						{
						}
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					}
				}
			});
		}
		registry["h_mod_p"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_modPickerSelector = Math.Max(0, _modPickerSelector - 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_mod_n"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				int num12 = 0;
				try
				{
					AmongUsClient instance9 = AmongUsClient.Instance;
					num12 = ((instance9 == null) ? null : ((InnerNetClient)instance9).allClients?.Count).GetValueOrDefault();
				}
				catch
				{
				}
				_modPickerSelector = Math.Min(Math.Max(0, num12 - 1), _modPickerSelector + 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_mod_add"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance7 = AmongUsClient.Instance;
				if (instance7 != null && ((InnerNetClient)instance7).AmHost)
				{
					try
					{
						AmongUsClient instance8 = AmongUsClient.Instance;
						List<ClientData> val3 = ((instance8 != null) ? ((InnerNetClient)instance8).allClients : null);
						if (val3 == null || _modPickerSelector >= val3.Count)
						{
							return;
						}
						ClientData val4 = val3[_modPickerSelector];
						if (val4 == null)
						{
							return;
						}
						string text3 = val4.FriendCode ?? "";
						string text4 = val4.PlayerName ?? "";
						if (string.IsNullOrWhiteSpace(text3))
						{
							NotifyUtils.Warning("No FriendCode for that player");
							return;
						}
						CrewModeratorService.Add(text3, text4);
						NotifyUtils.Info("Moderator added: " + text4);
					}
					catch
					{
					}
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		for (int num = 0; num < 20; num++)
		{
			int idx3 = num;
			registry[$"h_mod_rm_{num}"] = guard(delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					AmongUsClient instance6 = AmongUsClient.Instance;
					if (instance6 != null && ((InnerNetClient)instance6).AmHost)
					{
						try
						{
							List<KeyValuePair<string, string>> list2 = CrewModeratorService.ListDetailed();
							if (idx3 < 0 || idx3 >= list2.Count)
							{
								return;
							}
							KeyValuePair<string, string> keyValuePair2 = list2[idx3];
							CrewModeratorService.Remove(keyValuePair2.Key);
							NotifyUtils.Info("Moderator removed: " + (string.IsNullOrEmpty(keyValuePair2.Value) ? keyValuePair2.Key : keyValuePair2.Value));
						}
						catch
						{
						}
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					}
				}
			});
		}
		registry["h_vip_p"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_vipPickerSelector = Math.Max(0, _vipPickerSelector - 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_vip_n"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				int num11 = 0;
				try
				{
					AmongUsClient instance5 = AmongUsClient.Instance;
					num11 = ((instance5 == null) ? null : ((InnerNetClient)instance5).allClients?.Count).GetValueOrDefault();
				}
				catch
				{
				}
				_vipPickerSelector = Math.Min(Math.Max(0, num11 - 1), _vipPickerSelector + 1);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_vip_add"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				AmongUsClient instance3 = AmongUsClient.Instance;
				if (instance3 != null && ((InnerNetClient)instance3).AmHost)
				{
					try
					{
						AmongUsClient instance4 = AmongUsClient.Instance;
						List<ClientData> val = ((instance4 != null) ? ((InnerNetClient)instance4).allClients : null);
						if (val == null || _vipPickerSelector >= val.Count)
						{
							return;
						}
						ClientData val2 = val[_vipPickerSelector];
						if (val2 == null)
						{
							return;
						}
						string text = val2.FriendCode ?? "";
						string text2 = val2.PlayerName ?? "";
						if (string.IsNullOrWhiteSpace(text))
						{
							NotifyUtils.Warning("No FriendCode for that player");
							return;
						}
						CrewVipService.Add(text, text2);
						NotifyUtils.Info("VIP added: " + text2);
					}
					catch
					{
					}
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			}
		});
		for (int num2 = 0; num2 < 20; num2++)
		{
			int idx2 = num2;
			registry[$"h_vip_rm_{num2}"] = guard(delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					AmongUsClient instance2 = AmongUsClient.Instance;
					if (instance2 != null && ((InnerNetClient)instance2).AmHost)
					{
						try
						{
							List<KeyValuePair<string, string>> list = CrewVipService.ListDetailed();
							if (idx2 < 0 || idx2 >= list.Count)
							{
								return;
							}
							KeyValuePair<string, string> keyValuePair = list[idx2];
							CrewVipService.Remove(keyValuePair.Key);
							NotifyUtils.Info("VIP removed: " + (string.IsNullOrEmpty(keyValuePair.Value) ? keyValuePair.Key : keyValuePair.Value));
						}
						catch
						{
						}
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					}
				}
			});
		}
		for (int num3 = 0; num3 < 20; num3++)
		{
			int idx = num3;
			registry[$"h_rl_ban_{num3}"] = guard(delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					AmongUsClient instance = AmongUsClient.Instance;
					if (instance == null || !((InnerNetClient)instance).AmHost)
					{
						NotifyUtils.Warning("Only the host can ban.");
					}
					else
					{
						try
						{
							RecentlyLeftService.Entry at = RecentlyLeftService.GetAt(idx);
							if (at == null)
							{
								NotifyUtils.Warning("Entry not found");
								return;
							}
							bool num10 = CrewBanListService.Add(at.FriendCode, at.PlayerName ?? "", "Banned after leaving");
							RecentlyLeftService.RemoveAt(idx);
							if (num10)
							{
								NotifyUtils.Info("Banned " + (string.IsNullOrEmpty(at.PlayerName) ? at.FriendCode : at.PlayerName));
							}
							else
							{
								NotifyUtils.Info("Already on banlist: " + (string.IsNullOrEmpty(at.PlayerName) ? at.FriendCode : at.PlayerName));
							}
						}
						catch
						{
						}
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					}
				}
			});
			registry[$"h_rl_rm_{num3}"] = guard(delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					try
					{
						RecentlyLeftService.RemoveAt(idx);
					}
					catch
					{
					}
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			});
		}
		registry["h_rl_clr"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				try
				{
					RecentlyLeftService.Clear();
				}
				catch
				{
				}
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_sec_settings"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_showSettings = !_showSettings;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_sub_role"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_hostSubTab = 0;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_sub_lobby"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_hostSubTab = 1;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_sub_map"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_hostSubTab = 2;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_sub_lab"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_hostSubTab = 3;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_sub_rules"] = delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				_hostSubTab = 4;
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		};
		registry["h_gs_imp"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				int num9 = (int)ServerData.GetSliderValue("h_gs_imp");
				if (num9 < 1)
				{
					num9 = 1;
				}
				if (num9 > 3)
				{
					num9 = 3;
				}
				GameCheats.SetGameInt(ServerData.Config.GoImp, num9);
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_killcd"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				float num8 = ServerData.GetSliderValue("h_gs_killcd");
				if (num8 < 0f)
				{
					num8 = 0f;
				}
				float value = ((num8 <= 0f) ? 0.0001f : num8);
				GameCheats.SetGameFloat(ServerData.Config.GoKcd, value);
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_speed"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoSpd, ServerData.GetSliderValue("h_gs_speed"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_crewvis"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoClg, ServerData.GetSliderValue("h_gs_crewvis"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_impvis"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoIlg, ServerData.GetSliderValue("h_gs_impvis"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_emg"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameInt(ServerData.Config.GoEmg, (int)ServerData.GetSliderValue("h_gs_emg"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_emgcd"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameInt(ServerData.Config.GoEmgCd, (int)ServerData.GetSliderValue("h_gs_emgcd"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_disc"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameInt(ServerData.Config.GoDisc, (int)ServerData.GetSliderValue("h_gs_disc"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_vote"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameInt(ServerData.Config.GoVote, (int)ServerData.GetSliderValue("h_gs_vote"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_common"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameInt(ServerData.Config.GoComm, (int)ServerData.GetSliderValue("h_gs_common"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_long"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameInt(ServerData.Config.GoLng, (int)ServerData.GetSliderValue("h_gs_long"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_short"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameInt(ServerData.Config.GoShrt, (int)ServerData.GetSliderValue("h_gs_short"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_killdist"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				int num7 = (int)ServerData.GetSliderValue("h_gs_killdist");
				if (num7 < 0)
				{
					num7 = 0;
				}
				if (num7 > 2)
				{
					num7 = 2;
				}
				GameCheats.SetGameInt(ServerData.Config.GoKDist, num7);
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_venttime"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoVtm, ServerData.GetSliderValue("h_gs_venttime"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_taskbar"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				int num6 = (int)ServerData.GetSliderValue("h_gs_taskbar");
				if (num6 < 0)
				{
					num6 = 0;
				}
				if (num6 > 2)
				{
					num6 = 2;
				}
				GameCheats.SetGameInt(ServerData.Config.GoTbar, num6);
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_ga_dur"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoGaDur, ServerData.GetSliderValue("h_gs_ga_dur"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_ga_cd"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoGaCd, ServerData.GetSliderValue("h_gs_ga_cd"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_eng_cd"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoEngCd, ServerData.GetSliderValue("h_gs_eng_cd"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_eng_venttime"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoEngVt, ServerData.GetSliderValue("h_gs_eng_venttime"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_sci_cd"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoSciCd, ServerData.GetSliderValue("h_gs_sci_cd"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_sci_bat"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoSciBat, ServerData.GetSliderValue("h_gs_sci_bat"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_track_cd"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoTrkCd, ServerData.GetSliderValue("h_gs_track_cd"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_track_dur"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoTrkDur, ServerData.GetSliderValue("h_gs_track_dur"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_track_dly"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoTrkDly, ServerData.GetSliderValue("h_gs_track_dly"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_noise_dur"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoNsDur, ServerData.GetSliderValue("h_gs_noise_dur"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_detec_limit"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				float num5 = ServerData.GetSliderValue("h_gs_detec_limit");
				if (num5 < 1f)
				{
					num5 = 1f;
				}
				if (num5 > 10f)
				{
					num5 = 10f;
				}
				GameCheats.SetGameFloat(ServerData.Config.GoDetLim, num5);
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_shift_cd"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoShfCd, ServerData.GetSliderValue("h_gs_shift_cd"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_shift_dur"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoShfDur, ServerData.GetSliderValue("h_gs_shift_dur"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_phan_cd"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoPhCd, ServerData.GetSliderValue("h_gs_phan_cd"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_phan_dur"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoPhDur, ServerData.GetSliderValue("h_gs_phan_dur"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_viper_dis"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.SetGameFloat(ServerData.Config.GoVprDis, ServerData.GetSliderValue("h_gs_viper_dis"));
				PlayerPickMenu.TriggerRealtimeUpdate();
			}
		});
		registry["h_gs_confirm"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				bool gameBool7 = GameCheats.GetGameBool(ServerData.Config.GoBCfm);
				GameCheats.SetGameBool(ServerData.Config.GoBCfm, !gameBool7);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_gs_anon"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				bool gameBool6 = GameCheats.GetGameBool(ServerData.Config.GoBAnon);
				GameCheats.SetGameBool(ServerData.Config.GoBAnon, !gameBool6);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_gs_visual"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				bool gameBool5 = GameCheats.GetGameBool(ServerData.Config.GoBVis);
				GameCheats.SetGameBool(ServerData.Config.GoBVis, !gameBool5);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_gs_ghosttask"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				bool gameBool4 = GameCheats.GetGameBool(ServerData.Config.GoBGst);
				GameCheats.SetGameBool(ServerData.Config.GoBGst, !gameBool4);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_gs_impprot"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				bool gameBool3 = GameCheats.GetGameBool(ServerData.Config.GoBImpRo);
				GameCheats.SetGameBool(ServerData.Config.GoBImpRo, !gameBool3);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_gs_noisealert"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				bool gameBool2 = GameCheats.GetGameBool(ServerData.Config.GoBNsAlr);
				GameCheats.SetGameBool(ServerData.Config.GoBNsAlr, !gameBool2);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_gs_shiftskin"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				bool gameBool = GameCheats.GetGameBool(ServerData.Config.GoBShfSk);
				GameCheats.SetGameBool(ServerData.Config.GoBShfSk, !gameBool);
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		registry["h_gs_reset"] = guard(delegate(long t)
		{
			if (GhostUI.CheckToken(t))
			{
				GameCheats.ResetGameSettings();
				PlayerPickMenu.TriggerRealtimeUpdate(force: true);
			}
		});
		for (int num4 = 1; num4 <= 3; num4++)
		{
			int s = num4;
			registry[$"h_gs_save{s}"] = guard(delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					GameCheats.SaveSettingsPreset(s);
				}
			});
			registry[$"h_gs_load{s}"] = guard(delegate(long t)
			{
				if (GhostUI.CheckToken(t))
				{
					GameCheats.LoadSettingsPreset(s);
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
			});
		}
		static Action<long> guard(Action<long> inner)
		{
			return delegate(long t)
			{
				if (IntegrityGuard.IsIntact)
				{
					inner(t);
				}
			};
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
		if (safeSnapshot?.BanMenuBytecode != null && safeSnapshot.BanMenuBytecode.Length != 0)
		{
			if (_cachedBMBytecode != safeSnapshot.BanMenuBytecode)
			{
				_cachedBMBytecode = safeSnapshot.BanMenuBytecode;
				_cachedBMInverseMap = ((safeSnapshot.BanMenuBytecode.Length >= 524) ? new byte[256] : null);
				if (_cachedBMInverseMap != null)
				{
					Array.Copy(safeSnapshot.BanMenuBytecode, 268, _cachedBMInverseMap, 0, 256);
				}
				_cachedBMToken = ((safeSnapshot.BanMenuBytecode.Length >= 268) ? BitConverter.ToInt64(safeSnapshot.BanMenuBytecode, 260) : safeSnapshot.SessionToken);
			}
			GhostUI.Execute(safeSnapshot.BanMenuBytecode, _cachedBMToken, GetActionRegistry(), _cachedBMInverseMap);
		}
		else
		{
			GUILayout.Label("<color=#949EAD>Loading from server...</color>", GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(50f) });
		}
	}

	public static int GetStateHash()
	{
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Invalid comparison between Unknown and I4
		int num = 17;
		num = num * 31 + (_dropdownA ? 1 : 0);
		num = num * 31 + (_dropdownB ? 1 : 0);
		num = num * 31 + _selectorA;
		num = num * 31 + _selectorB;
		num = num * 31 + _autoKickNameSelector;
		num = num * 31 + _modPickerSelector;
		try
		{
			num = num * 31 + CrewModeratorService.List().Count;
		}
		catch
		{
		}
		num = num * 31 + (GameCheats.RainbowEnabled ? 1 : 0);
		num = num * 31 + (CheatConfig.AutoKickByLevelEnabled.Value ? 1 : 0);
		num = num * 31 + CheatConfig.AutoKickMinLevel.Value;
		num = num * 31 + (CheatConfig.AutoKickByNameEnabled.Value ? 1 : 0);
		num = num * 31 + (CheatConfig.AutoKickNameList.Value?.GetHashCode() ?? 0);
		num = num * 31 + ((CheatConfig.AutoKickByChatEnabled?.Value ?? false) ? 1 : 0);
		num = num * 31 + (CheatConfig.AutoKickChatWordList?.Value?.GetHashCode()).GetValueOrDefault();
		num = num * 31 + (CheatConfig.ShadowClonesCount?.Value ?? 1);
		num = num * 31 + ((CheatConfig.ShadowClonesOrbit?.Value ?? false) ? 1 : 0);
		num = num * 31 + _hostSubTab;
		try
		{
			PlayerData player = DataManager.Player;
			PlayerBanData val = ((player != null) ? player.ban : null);
			num = num * 31 + (int)((val != null) ? val.BanPoints : 0f);
			num = num * 31 + ((val != null) ? val.BanMinutesLeft : 0);
			int num2 = num * 31;
			AmongUsClient instance = AmongUsClient.Instance;
			int num3;
			if (instance != null)
			{
				_ = ((InnerNetClient)instance).GameState;
				if (0 == 0)
				{
					num3 = (int)((InnerNetClient)AmongUsClient.Instance).GameState;
					goto IL_01b7;
				}
			}
			num3 = 0;
			goto IL_01b7;
			IL_01b7:
			num = num2 + num3;
			num = num * 31 + DateTime.Now.Minute;
			int num4 = num * 31;
			PlayerData player2 = DataManager.Player;
			int num5;
			if (player2 == null)
			{
				num5 = 0;
			}
			else
			{
				PlayerAccountData account = player2.Account;
				num5 = (((int)((account != null) ? new AccountLoginStatus?(account.LoginStatus) : null).GetValueOrDefault() == 3) ? 1 : 0);
			}
			num = num4 + ((num5 != 0) ? 1 : 0);
			int num6 = num * 31;
			PlayerData player3 = DataManager.Player;
			int num7;
			if (player3 == null)
			{
				num7 = 0;
			}
			else
			{
				PlayerAdsData ads = player3.Ads;
				num7 = (((ads != null) ? new bool?(ads.HasPurchasedAdRemoval) : null).GetValueOrDefault() ? 1 : 0);
			}
			num = num6 + ((num7 != 0) ? 1 : 0);
		}
		catch
		{
		}
		return num;
	}

	public static object GetBanMenuState()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Expected I4, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Invalid comparison between Unknown and I4
		try
		{
			PlayerData player = DataManager.Player;
			PlayerBanData val = ((player != null) ? player.ban : null);
			AmongUsClient instance = AmongUsClient.Instance;
			GameStates? val2 = ((instance != null) ? new GameStates?(((InnerNetClient)instance).GameState) : null);
			BanMenuStateDto banMenuStateDto = _banMenuStateDto;
			AmongUsClient instance2 = AmongUsClient.Instance;
			banMenuStateDto.h = instance2 != null && ((InnerNetClient)instance2).AmHost;
			_banMenuStateDto.g = (Object)(object)ShipStatus.Instance != (Object)null;
			_banMenuStateDto.bp = ((val != null) ? val.BanPoints : 0f);
			_banMenuStateDto.bm = ((val != null) ? val.BanMinutesLeft : 0);
			_banMenuStateDto.pc = PlayerControl.AllPlayerControls?.Count ?? 0;
			_banMenuStateDto.st = (val2.HasValue ? ((int)val2.Value) : 0);
			_banMenuStateDto.tz = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;
			_banMenuStateDto.th = DateTime.Now.Hour;
			_banMenuStateDto.tm = DateTime.Now.Minute;
			try
			{
				PlayerData player2 = DataManager.Player;
				PlayerStatsData val3 = ((player2 != null) ? player2.Stats : null);
				if (val3 != null)
				{
					_banMenuStateDto.gs = (int)val3.GetStat((StatID)1);
					_banMenuStateDto.gi = (int)val3.GetStat((StatID)3);
					_banMenuStateDto.gc = (int)val3.GetStat((StatID)4);
					_banMenuStateDto.ik = (int)val3.GetStat((StatID)11);
					_banMenuStateDto.td = (int)val3.GetStat((StatID)12);
					_banMenuStateDto.te = (int)val3.GetStat((StatID)13);
					_banMenuStateDto.cs = (int)val3.GetStat((StatID)10);
					_banMenuStateDto.tc = (int)val3.GetStat((StatID)8);
					_banMenuStateDto.sf = (int)val3.GetStat((StatID)7);
				}
			}
			catch
			{
			}
			try
			{
				BanMenuStateDto banMenuStateDto2 = _banMenuStateDto;
				PlayerData player3 = DataManager.Player;
				object obj2;
				if (player3 == null)
				{
					obj2 = null;
				}
				else
				{
					PlayerCustomizationData customization = player3.Customization;
					obj2 = ((customization != null) ? customization.Name : null);
				}
				if (obj2 == null)
				{
					obj2 = "";
				}
				banMenuStateDto2.pn = (string)obj2;
				BanMenuStateDto banMenuStateDto3 = _banMenuStateDto;
				EOSManager instance3 = DestroyableSingleton<EOSManager>.Instance;
				banMenuStateDto3.fc = ((instance3 != null) ? instance3.FriendCode : null) ?? "";
				BanMenuStateDto banMenuStateDto4 = _banMenuStateDto;
				ServerManager instance4 = DestroyableSingleton<ServerManager>.Instance;
				object obj3;
				if (instance4 == null)
				{
					obj3 = null;
				}
				else
				{
					IRegionInfo currentRegion = instance4.CurrentRegion;
					obj3 = ((currentRegion != null) ? currentRegion.Name : null);
				}
				if (obj3 == null)
				{
					obj3 = "";
				}
				banMenuStateDto4.rg = (string)obj3;
				BanMenuStateDto banMenuStateDto5 = _banMenuStateDto;
				PlayerData player4 = DataManager.Player;
				int gu;
				if (player4 == null)
				{
					gu = 0;
				}
				else
				{
					PlayerAccountData account = player4.Account;
					gu = (((int)((account != null) ? new AccountLoginStatus?(account.LoginStatus) : null).GetValueOrDefault() == 3) ? 1 : 0);
				}
				banMenuStateDto5.gu = (byte)gu != 0;
				BanMenuStateDto banMenuStateDto6 = _banMenuStateDto;
				PlayerData player5 = DataManager.Player;
				bool? obj4;
				if (player5 == null)
				{
					obj4 = null;
				}
				else
				{
					PlayerAdsData ads = player5.Ads;
					obj4 = ((ads != null) ? new bool?(ads.HasPurchasedAdRemoval) : null);
				}
				bool? flag = obj4;
				banMenuStateDto6.ar = flag.GetValueOrDefault();
			}
			catch
			{
			}
			try
			{
				AmongUsClient instance5 = AmongUsClient.Instance;
				if ((Object)(object)instance5 != (Object)null && ((InnerNetClient)instance5).GameId != 0)
				{
					_banMenuStateDto.rc = GameCode.IntToGameName(((InnerNetClient)instance5).GameId) ?? "";
				}
				else
				{
					_banMenuStateDto.rc = "";
				}
				GameOptionsManager instance6 = GameOptionsManager.Instance;
				IGameOptions val4 = ((instance6 != null) ? instance6.CurrentGameOptions : null);
				if (val4 != null)
				{
					int num = Mathf.Clamp((int)val4.MapId, 0, ((Il2CppArrayBase<string>)(object)Constants.MapNames).Length - 1);
					_banMenuStateDto.mn = ((Il2CppArrayBase<string>)(object)Constants.MapNames)[num] ?? "";
					_banMenuStateDto.ic = val4.NumImpostors;
					_banMenuStateDto.gm = (int)val4.GameMode;
				}
			}
			catch
			{
			}
			return _banMenuStateDto;
		}
		catch
		{
			_banMenuStateDto.h = false;
			_banMenuStateDto.g = false;
			_banMenuStateDto.bp = 0f;
			_banMenuStateDto.bm = 0;
			_banMenuStateDto.pc = 0;
			_banMenuStateDto.st = 0;
			_banMenuStateDto.tz = 0;
			_banMenuStateDto.th = 0;
			_banMenuStateDto.tm = 0;
			return _banMenuStateDto;
		}
	}

	public static object GetUIState()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			_banMenuUiDto.da = _dropdownA;
			_banMenuUiDto.db = _dropdownB;
			_banMenuUiDto.sa = _selectorA;
			_banMenuUiDto.sb = _selectorB;
			_banMenuUiDto.ro = ImpostorForcer.IsAnyOverrideActive;
			BanMenuUiDto banMenuUiDto = _banMenuUiDto;
			RoleTypes selectedRoleForHost = ImpostorForcer.SelectedRoleForHost;
			banMenuUiDto.rs = selectedRoleForHost.ToString();
			_banMenuUiDto.al = CheatConfig.AutoKickByLevelEnabled.Value;
			_banMenuUiDto.an = CheatConfig.AutoKickByNameEnabled.Value;
			_banMenuUiDto.av = CheatConfig.AutoKickMinLevel.Value;
			_banMenuUiDto.nl = CheatConfig.AutoKickNameList.Value ?? "";
			_banMenuUiDto.ns = _autoKickNameSelector;
			_banMenuUiDto.ac = CheatConfig.AutoKickByChatEnabled?.Value ?? false;
			_banMenuUiDto.cwl = CheatConfig.AutoKickChatWordList?.Value ?? "";
			_banMenuUiDto.gs = _showSettings;
			_banMenuUiDto.hd = GameCheats._dummyActive;
			_banMenuUiDto.hm = GameCheats._mapActive;
			_banMenuUiDto.hc = GameCheats._cfActive;
			_banMenuUiDto.hst = _hostSubTab;
			_banMenuUiDto.ha = GameCheats._allMapsActive;
			_banMenuUiDto.hms = GameCheats._selMapSpawnId;
			_banMenuUiDto.hr = GameCheats.RainbowEnabled;
			_banMenuUiDto.scc = CheatConfig.ShadowClonesCount?.Value ?? 1;
			_banMenuUiDto.sco = CheatConfig.ShadowClonesOrbit?.Value ?? false;
			try
			{
				List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
				if (allPlayerControls != null && _autoKickNameSelector < allPlayerControls.Count && (Object)(object)allPlayerControls[_autoKickNameSelector] != (Object)null)
				{
					BanMenuUiDto banMenuUiDto2 = _banMenuUiDto;
					NetworkedPlayerInfo data = allPlayerControls[_autoKickNameSelector].Data;
					banMenuUiDto2.np = ((data != null) ? data.PlayerName : null) ?? "";
				}
				else
				{
					_banMenuUiDto.np = "";
				}
			}
			catch
			{
				_banMenuUiDto.np = "";
			}
			try
			{
				AmongUsClient instance = AmongUsClient.Instance;
				List<ClientData> val = ((instance != null) ? ((InnerNetClient)instance).allClients : null);
				int num = val?.Count ?? 0;
				if (num == 0)
				{
					_banMenuUiDto.mpi = 0;
					_banMenuUiDto.mpn = "";
				}
				else
				{
					if (_modPickerSelector >= num)
					{
						_modPickerSelector = num - 1;
					}
					if (_modPickerSelector < 0)
					{
						_modPickerSelector = 0;
					}
					_banMenuUiDto.mpi = _modPickerSelector;
					ClientData val2 = val[_modPickerSelector];
					_banMenuUiDto.mpn = ((val2 != null) ? val2.PlayerName : null) ?? "";
				}
			}
			catch
			{
				_banMenuUiDto.mpi = 0;
				_banMenuUiDto.mpn = "";
			}
			try
			{
				AmongUsClient instance2 = AmongUsClient.Instance;
				List<ClientData> val3 = ((instance2 != null) ? ((InnerNetClient)instance2).allClients : null);
				if (val3 == null || val3.Count == 0)
				{
					_banMenuUiDto.vpi = 0;
					_banMenuUiDto.vpn = "";
				}
				else
				{
					int count = val3.Count;
					if (_vipPickerSelector >= count)
					{
						_vipPickerSelector = count - 1;
					}
					if (_vipPickerSelector < 0)
					{
						_vipPickerSelector = 0;
					}
					_banMenuUiDto.vpi = _vipPickerSelector;
					ClientData val4 = val3[_vipPickerSelector];
					_banMenuUiDto.vpn = ((val4 != null) ? val4.PlayerName : null) ?? "";
				}
			}
			catch
			{
				_banMenuUiDto.vpi = 0;
				_banMenuUiDto.vpn = "";
			}
			try
			{
				List<KeyValuePair<string, string>> list = CrewModeratorService.ListDetailed();
				if (list.Count == 0)
				{
					_banMenuUiDto.mls = "";
				}
				else
				{
					StringBuilder stringBuilder = new StringBuilder(list.Count * 24);
					for (int i = 0; i < list.Count; i++)
					{
						if (i > 0)
						{
							stringBuilder.Append(';');
						}
						KeyValuePair<string, string> keyValuePair = list[i];
						stringBuilder.Append((keyValuePair.Key ?? "").Replace(';', ' ').Replace('|', ' '));
						stringBuilder.Append('|');
						stringBuilder.Append((keyValuePair.Value ?? "").Replace(';', ' ').Replace('|', ' '));
					}
					_banMenuUiDto.mls = stringBuilder.ToString();
				}
			}
			catch
			{
				_banMenuUiDto.mls = "";
			}
			try
			{
				List<RecentlyLeftService.Entry> list2 = RecentlyLeftService.ListDetailed();
				if (list2.Count == 0)
				{
					_banMenuUiDto.rls = "";
				}
				else
				{
					StringBuilder stringBuilder2 = new StringBuilder(list2.Count * 24);
					for (int j = 0; j < list2.Count; j++)
					{
						if (j > 0)
						{
							stringBuilder2.Append(';');
						}
						RecentlyLeftService.Entry entry = list2[j];
						stringBuilder2.Append((entry.FriendCode ?? "").Replace(';', ' ').Replace('|', ' '));
						stringBuilder2.Append('|');
						stringBuilder2.Append((entry.PlayerName ?? "").Replace(';', ' ').Replace('|', ' '));
					}
					_banMenuUiDto.rls = stringBuilder2.ToString();
				}
			}
			catch
			{
				_banMenuUiDto.rls = "";
			}
			try
			{
				List<KeyValuePair<string, string>> list3 = CrewVipService.ListDetailed();
				if (list3.Count == 0)
				{
					_banMenuUiDto.vls = "";
				}
				else
				{
					StringBuilder stringBuilder3 = new StringBuilder(list3.Count * 24);
					for (int k = 0; k < list3.Count; k++)
					{
						if (k > 0)
						{
							stringBuilder3.Append(';');
						}
						KeyValuePair<string, string> keyValuePair2 = list3[k];
						stringBuilder3.Append((keyValuePair2.Key ?? "").Replace(';', ' ').Replace('|', ' '));
						stringBuilder3.Append('|');
						stringBuilder3.Append((keyValuePair2.Value ?? "").Replace(';', ' ').Replace('|', ' '));
					}
					_banMenuUiDto.vls = stringBuilder3.ToString();
				}
			}
			catch
			{
				_banMenuUiDto.vls = "";
			}
			return _banMenuUiDto;
		}
		catch
		{
			_banMenuUiDto.da = false;
			_banMenuUiDto.db = false;
			_banMenuUiDto.sa = 0;
			_banMenuUiDto.sb = 0;
			_banMenuUiDto.ro = false;
			_banMenuUiDto.rs = "Impostor";
			_banMenuUiDto.al = false;
			_banMenuUiDto.an = false;
			_banMenuUiDto.av = 5;
			_banMenuUiDto.nl = "";
			_banMenuUiDto.ns = 0;
			_banMenuUiDto.np = "";
			_banMenuUiDto.ac = false;
			_banMenuUiDto.cwl = "";
			_banMenuUiDto.scc = 1;
			_banMenuUiDto.sco = false;
			_banMenuUiDto.rls = "";
			return _banMenuUiDto;
		}
	}
}
