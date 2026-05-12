using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using UnityEngine;

namespace ModMenuCrew.UI;

public static class IconLoader
{
	private static readonly string[] TAB_IDS = new string[16]
	{
		"dashboard", "ban_menu", "host_controls", "spoofing", "lobbies", "settings", "game", "cheats", "playerpick", "movement",
		"impostor", "sabotage", "replay", "misc", "extras", "animations"
	};

	private static readonly Dictionary<string, string> _displayNameToId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Dashboard"] = "dashboard",
		["Account"] = "ban_menu",
		["Host"] = "host_controls",
		["Spoofing"] = "spoofing",
		["Lobbies"] = "lobbies",
		["Settings"] = "settings",
		["Game"] = "game",
		["Cheats"] = "cheats",
		["Players"] = "playerpick",
		["Movement"] = "movement",
		["Impostor"] = "impostor",
		["Sabotage"] = "sabotage",
		["Replay"] = "replay",
		["Cosmetics"] = "misc",
		["Extras"] = "extras",
		["Animations"] = "animations",
		["Misc"] = "misc",
		["Ban Menu"] = "ban_menu",
		["Host Controls"] = "host_controls",
		["PlayerPick"] = "playerpick",
		["Playerpick"] = "playerpick"
	};

	private static readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

	private static bool _initialized;

	public static void Initialize()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		try
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] tAB_IDS = TAB_IDS;
			foreach (string text in tAB_IDS)
			{
				Texture2D val = LoadEmbedded(executingAssembly, text);
				if ((Object)(object)val != (Object)null)
				{
					_cache[text] = val;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[IconLoader] Initialize failed: " + ex.Message));
		}
	}

	public static void PreloadAll()
	{
		Initialize();
	}

	public static Texture2D GetIcon(string nameOrId)
	{
		if (string.IsNullOrEmpty(nameOrId))
		{
			return null;
		}
		if (!_initialized)
		{
			Initialize();
		}
		if (_cache.TryGetValue(nameOrId, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		if (_displayNameToId.TryGetValue(nameOrId, out var value2) && _cache.TryGetValue(value2, out var value3) && (Object)(object)value3 != (Object)null)
		{
			return value3;
		}
		return null;
	}

	public static Texture2D GetIconForTab(string tabName)
	{
		return GetIcon(tabName);
	}

	public static void ClearCache()
	{
		foreach (Texture2D value in _cache.Values)
		{
			if ((Object)(object)value != (Object)null)
			{
				Object.Destroy((Object)(object)value);
			}
		}
		_cache.Clear();
		_initialized = false;
	}

	private static Texture2D LoadEmbedded(Assembly asm, string tabId)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		string text = "ModMenuCrew.TabIcons." + tabId + ".png";
		try
		{
			using Stream stream = asm.GetManifestResourceStream(text);
			if (stream == null)
			{
				Debug.LogWarning(Object.op_Implicit("[IconLoader] Embedded resource missing: " + text));
				return null;
			}
			using MemoryStream memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			byte[] array = memoryStream.ToArray();
			Texture2D val = new Texture2D(2, 2, (TextureFormat)4, false)
			{
				filterMode = (FilterMode)1,
				wrapMode = (TextureWrapMode)1,
				hideFlags = (HideFlags)61
			};
			if (!ImageConversion.LoadImage(val, Il2CppStructArray<byte>.op_Implicit(array), false))
			{
				Debug.LogWarning(Object.op_Implicit("[IconLoader] LoadImage failed: " + text));
				Object.Destroy((Object)(object)val);
				return null;
			}
			RemapToWhiteForTinting(val);
			return val;
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[IconLoader] Load " + text + ": " + ex.Message));
			return null;
		}
	}

	private static void RemapToWhiteForTinting(Texture2D tex)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!((Texture)tex).isReadable)
			{
				return;
			}
			Il2CppStructArray<Color32> pixels = tex.GetPixels32();
			bool flag = false;
			for (int i = 0; i < ((Il2CppArrayBase<Color32>)(object)pixels).Length; i++)
			{
				Color32 val = ((Il2CppArrayBase<Color32>)(object)pixels)[i];
				if (val.a > 20)
				{
					float num = ((float)(int)val.r * 0.299f + (float)(int)val.g * 0.587f + (float)(int)val.b * 0.114f) / 255f;
					if (num < 0.98f)
					{
						byte b = (byte)(Mathf.Clamp01(1f - (1f - num) * 0.12f) * 255f);
						((Il2CppArrayBase<Color32>)(object)pixels)[i] = new Color32(b, b, b, val.a);
						flag = true;
					}
				}
			}
			if (flag)
			{
				tex.SetPixels32(pixels);
			}
			tex.Apply(false, true);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(Object.op_Implicit("[IconLoader] Remap failed: " + ex.Message));
		}
	}
}
