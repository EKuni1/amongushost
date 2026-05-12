using System;
using System.Collections.Generic;
using ModMenuCrew.Features;
using ModMenuCrew.UI.Menus;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew.UI.Managers;

public class HostControlsTab
{
	private ServerData.UISnapshot _safeSnapshot;

	private byte[] _snapshotBytecode;

	private byte[] _snapshotInverseMap;

	private long _snapshotToken;

	private static Dictionary<string, Action<long>> _actionRegistry;

	public static bool DropdownA
	{
		get
		{
			return BanMenu.DropdownA;
		}
		set
		{
			BanMenu.DropdownA = value;
		}
	}

	public static bool DropdownB
	{
		get
		{
			return BanMenu.DropdownB;
		}
		set
		{
			BanMenu.DropdownB = value;
		}
	}

	public static int SelectorA
	{
		get
		{
			return BanMenu.SelectorA;
		}
		set
		{
			BanMenu.SelectorA = value;
		}
	}

	public static int SelectorB
	{
		get
		{
			return BanMenu.SelectorB;
		}
		set
		{
			BanMenu.SelectorB = value;
		}
	}

	public static void RegisterActions(Dictionary<string, Action<long>> registry)
	{
		BanMenu.RegisterActions(registry);
	}

	internal static Dictionary<string, Action<long>> GetOrBuildActionRegistry()
	{
		return GetActionRegistry();
	}

	private static Dictionary<string, Action<long>> GetActionRegistry()
	{
		if (_actionRegistry == null)
		{
			_actionRegistry = new Dictionary<string, Action<long>>();
			BanMenu.RegisterActions(_actionRegistry);
			WrapAction("h_ak_name_clr", delegate(Action<long> orig, long t)
			{
				orig(t);
				ExtrasTab.ClearCustomNameBuffer();
			});
			WrapAction("h_ak_name_add", delegate(Action<long> orig, long t)
			{
				orig(t);
				ExtrasTab.ClearCustomNameBuffer();
			});
			InitGameSettingsSliders();
		}
		return _actionRegistry;
	}

	private static void WrapAction(string id, Action<Action<long>, long> wrapper)
	{
		if (_actionRegistry.TryGetValue(id, out var original))
		{
			_actionRegistry[id] = delegate(long t)
			{
				wrapper(original, t);
			};
		}
	}

	internal static void InitGameSettingsSliders()
	{
		try
		{
			GameCheats.SaveOriginalGameSettings();
			GameCheats.SyncSlidersFromGameOptions();
		}
		catch
		{
		}
	}

	public void Draw()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)Event.current.type == 8)
		{
			PlayerPickMenu.CheckRealtimeUpdate();
			_safeSnapshot = ServerData.CurrentSnapshot;
			byte[] array = _safeSnapshot?.HostControlsBytecode;
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
			GhostUI.Execute(_snapshotBytecode, _snapshotToken, GetActionRegistry(), _snapshotInverseMap);
			return;
		}
		GUILayout.Label("<color=#949EAD>Loading host controls...</color>", GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedHeight(50f) });
	}
}
