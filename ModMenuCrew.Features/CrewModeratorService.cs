using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using Il2CppSystem;
using UnityEngine;

namespace ModMenuCrew.Features;

public static class CrewModeratorService
{
	private static readonly object _lock = new object();

	private static readonly HashSet<string> _mods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, string> _names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static bool _loaded;

	private static object _loadedForConfigRef;

	public static string FilePath
	{
		get
		{
			string text = ServerData.Config?.ModFileName;
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			return Path.Combine(GetGameRootSafe(), "ModMenuCrew", text);
		}
	}

	private static string LegacyPath
	{
		get
		{
			string text = ServerData.Config?.ModFileName;
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			return Path.Combine(GetGameRootSafe(), text);
		}
	}

	private static string GetGameRootSafe()
	{
		try
		{
			string gameRootPath = Paths.GameRootPath;
			if (!string.IsNullOrWhiteSpace(gameRootPath))
			{
				return gameRootPath;
			}
		}
		catch
		{
		}
		try
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			if (!string.IsNullOrWhiteSpace(baseDirectory))
			{
				return baseDirectory;
			}
		}
		catch
		{
		}
		try
		{
			string currentDirectory = Environment.CurrentDirectory;
			if (!string.IsNullOrWhiteSpace(currentDirectory))
			{
				return currentDirectory;
			}
		}
		catch
		{
		}
		return ".";
	}

	public static void EnsureLoaded()
	{
		ServerData.SecurityConfig config = ServerData.Config;
		if (config == null || (_loaded && _loadedForConfigRef == config))
		{
			return;
		}
		lock (_lock)
		{
			if (_loaded && _loadedForConfigRef == config)
			{
				return;
			}
			string filePath = FilePath;
			if (string.IsNullOrEmpty(filePath))
			{
				_loaded = false;
				return;
			}
			try
			{
				string directoryName = Path.GetDirectoryName(filePath);
				if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				try
				{
					string legacyPath = LegacyPath;
					if (!string.IsNullOrEmpty(legacyPath) && File.Exists(legacyPath) && !File.Exists(filePath))
					{
						File.Move(legacyPath, filePath);
					}
				}
				catch
				{
				}
				_mods.Clear();
				_names.Clear();
				if (File.Exists(filePath))
				{
					string[] array = File.ReadAllLines(filePath);
					for (int i = 0; i < array.Length; i++)
					{
						string text = array[i]?.Trim();
						if (string.IsNullOrEmpty(text) || text.StartsWith("#"))
						{
							continue;
						}
						string[] array2 = text.Split(',');
						string text2 = array2[0].Trim();
						if (!string.IsNullOrEmpty(text2))
						{
							_mods.Add(text2);
							if (array2.Length >= 2)
							{
								_names[text2] = array2[1].Trim();
							}
						}
					}
				}
				else
				{
					File.WriteAllText(filePath, "# CrewModerator list" + Environment.NewLine + "# One per line. Format: friendCode  OR  friendCode, playerName" + Environment.NewLine);
				}
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[CrewMod] Load error: {value}"));
			}
			_loaded = true;
			_loadedForConfigRef = config;
		}
	}

	public static bool IsModerator(string friendCode)
	{
		if (string.IsNullOrWhiteSpace(friendCode))
		{
			return false;
		}
		EnsureLoaded();
		lock (_lock)
		{
			return _mods.Contains(friendCode.Trim());
		}
	}

	public static bool Add(string friendCode, string playerName = null)
	{
		if (string.IsNullOrWhiteSpace(friendCode))
		{
			return false;
		}
		EnsureLoaded();
		lock (_lock)
		{
			string text = friendCode.Trim();
			if (!_mods.Add(text))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(playerName))
			{
				_names[text] = playerName.Trim();
			}
			PersistUnlocked();
			return true;
		}
	}

	public static bool Remove(string friendCode)
	{
		if (string.IsNullOrWhiteSpace(friendCode))
		{
			return false;
		}
		EnsureLoaded();
		lock (_lock)
		{
			string text = friendCode.Trim();
			if (!_mods.Remove(text))
			{
				return false;
			}
			_names.Remove(text);
			PersistUnlocked();
			return true;
		}
	}

	public static List<string> List()
	{
		EnsureLoaded();
		lock (_lock)
		{
			List<string> list = new List<string>(_mods.Count);
			foreach (string mod in _mods)
			{
				if (_names.TryGetValue(mod, out var value) && !string.IsNullOrEmpty(value))
				{
					list.Add(mod + " (" + value + ")");
				}
				else
				{
					list.Add(mod);
				}
			}
			list.Sort(StringComparer.OrdinalIgnoreCase);
			return list;
		}
	}

	public static List<KeyValuePair<string, string>> ListDetailed()
	{
		EnsureLoaded();
		lock (_lock)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(_mods.Count);
			foreach (string mod in _mods)
			{
				_names.TryGetValue(mod, out var value);
				list.Add(new KeyValuePair<string, string>(mod, value ?? ""));
			}
			list.Sort((KeyValuePair<string, string> a, KeyValuePair<string, string> b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
			return list;
		}
	}

	private static void PersistUnlocked()
	{
		string filePath = FilePath;
		if (string.IsNullOrEmpty(filePath))
		{
			return;
		}
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("# CrewModerator list");
			stringBuilder.AppendLine("# One per line. Format: friendCode  OR  friendCode, playerName");
			foreach (string mod in _mods)
			{
				if (_names.TryGetValue(mod, out var value) && !string.IsNullOrEmpty(value))
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
					handler.AppendFormatted(mod);
					handler.AppendLiteral(", ");
					handler.AppendFormatted(value);
					stringBuilder2.AppendLine(ref handler);
				}
				else
				{
					stringBuilder.AppendLine(mod);
				}
			}
			AtomicWrite(filePath, stringBuilder.ToString());
		}
		catch (Exception value2)
		{
			Debug.LogError(Object.op_Implicit($"[CrewMod] Persist error: {value2}"));
		}
	}

	private static void AtomicWrite(string path, string content)
	{
		string text = path + ".tmp." + Guid.NewGuid().ToString("N").Substring(0, 8);
		try
		{
			File.WriteAllText(text, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			if (File.Exists(path))
			{
				try
				{
					File.Replace(text, path, null);
					return;
				}
				catch
				{
				}
				try
				{
					File.Delete(path);
				}
				catch
				{
				}
			}
			File.Move(text, path);
		}
		finally
		{
			try
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
			}
			catch
			{
			}
		}
	}
}
