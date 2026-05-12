using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using Il2CppSystem;
using UnityEngine;

namespace ModMenuCrew.Features;

public static class CrewVipService
{
	private static readonly object _lock = new object();

	private static readonly HashSet<string> _vips = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, string> _names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static bool _loaded;

	private static object _loadedForConfigRef;

	public static string FilePath
	{
		get
		{
			string text = ServerData.Config?.VipFileName;
			if (string.IsNullOrEmpty(text))
			{
				text = "vips.txt";
			}
			return Path.Combine(GetGameRootSafe(), "ModMenuCrew", text);
		}
	}

	private static string LegacyPath
	{
		get
		{
			string text = ServerData.Config?.VipFileName;
			if (string.IsNullOrEmpty(text))
			{
				text = "vips.txt";
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
				_vips.Clear();
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
							_vips.Add(text2);
							if (array2.Length >= 2)
							{
								_names[text2] = array2[1].Trim();
							}
						}
					}
				}
				else
				{
					File.WriteAllText(filePath, "# CrewVip list (can use /color only)" + Environment.NewLine + "# One per line. Format: friendCode  OR  friendCode, playerName" + Environment.NewLine);
				}
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[CrewVip] Load error: {value}"));
			}
			_loaded = true;
			_loadedForConfigRef = config;
		}
	}

	public static bool IsVip(string friendCode)
	{
		if (string.IsNullOrWhiteSpace(friendCode))
		{
			return false;
		}
		EnsureLoaded();
		lock (_lock)
		{
			return _vips.Contains(friendCode.Trim());
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
			if (!_vips.Add(text))
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
			if (!_vips.Remove(text))
			{
				return false;
			}
			_names.Remove(text);
			PersistUnlocked();
			return true;
		}
	}

	public static List<KeyValuePair<string, string>> ListDetailed()
	{
		EnsureLoaded();
		lock (_lock)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(_vips.Count);
			foreach (string vip in _vips)
			{
				_names.TryGetValue(vip, out var value);
				list.Add(new KeyValuePair<string, string>(vip, value ?? ""));
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
			stringBuilder.AppendLine("# CrewVip list (can use /color only)");
			stringBuilder.AppendLine("# One per line. Format: friendCode  OR  friendCode, playerName");
			foreach (string vip in _vips)
			{
				if (_names.TryGetValue(vip, out var value) && !string.IsNullOrEmpty(value))
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
					handler.AppendFormatted(vip);
					handler.AppendLiteral(", ");
					handler.AppendFormatted(value);
					stringBuilder2.AppendLine(ref handler);
				}
				else
				{
					stringBuilder.AppendLine(vip);
				}
			}
			AtomicWrite(filePath, stringBuilder.ToString());
		}
		catch (Exception value2)
		{
			Debug.LogError(Object.op_Implicit($"[CrewVip] Persist error: {value2}"));
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
