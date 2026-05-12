using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using Il2CppSystem;
using UnityEngine;

namespace ModMenuCrew.Features;

public static class CrewBanListService
{
	private static readonly object _lock = new object();

	private static readonly HashSet<string> _bans = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, string> _names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, string> _reasons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static bool _loaded;

	private const string FILE_NAME = "banlist.txt";

	public static string FilePath => Path.Combine(GetGameRootSafe(), "ModMenuCrew", "banlist.txt");

	public static int Count
	{
		get
		{
			EnsureLoaded();
			lock (_lock)
			{
				return _bans.Count;
			}
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
		if (_loaded)
		{
			return;
		}
		lock (_lock)
		{
			if (_loaded)
			{
				return;
			}
			string filePath = FilePath;
			try
			{
				string directoryName = Path.GetDirectoryName(filePath);
				if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				_bans.Clear();
				_names.Clear();
				_reasons.Clear();
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
							_bans.Add(text2);
							if (array2.Length >= 2)
							{
								_names[text2] = array2[1].Trim();
							}
							if (array2.Length >= 3)
							{
								_reasons[text2] = array2[2].Trim();
							}
						}
					}
				}
				else
				{
					File.WriteAllText(filePath, "# CrewBanList — persistent friend-code ban list" + Environment.NewLine + "# Format: friendCode, playerName, reason" + Environment.NewLine);
				}
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[CrewBanList] Load error: {value}"));
			}
			_loaded = true;
		}
	}

	public static bool IsBanned(string friendCode)
	{
		if (string.IsNullOrWhiteSpace(friendCode))
		{
			return false;
		}
		EnsureLoaded();
		lock (_lock)
		{
			return _bans.Contains(friendCode.Trim());
		}
	}

	public static bool Add(string friendCode, string playerName = null, string reason = null)
	{
		if (string.IsNullOrWhiteSpace(friendCode))
		{
			return false;
		}
		EnsureLoaded();
		lock (_lock)
		{
			string text = friendCode.Trim();
			if (!_bans.Add(text))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(playerName))
			{
				_names[text] = playerName.Trim();
			}
			if (!string.IsNullOrEmpty(reason))
			{
				_reasons[text] = reason.Trim();
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
			if (!_bans.Remove(text))
			{
				return false;
			}
			_names.Remove(text);
			_reasons.Remove(text);
			PersistUnlocked();
			return true;
		}
	}

	public static int RemoveByName(string playerName)
	{
		if (string.IsNullOrWhiteSpace(playerName))
		{
			return 0;
		}
		EnsureLoaded();
		lock (_lock)
		{
			string b = playerName.Trim();
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, string> name in _names)
			{
				if (string.Equals(name.Value, b, StringComparison.OrdinalIgnoreCase))
				{
					list.Add(name.Key);
				}
			}
			foreach (string item in list)
			{
				_bans.Remove(item);
				_names.Remove(item);
				_reasons.Remove(item);
			}
			if (list.Count > 0)
			{
				PersistUnlocked();
			}
			return list.Count;
		}
	}

	public static List<KeyValuePair<string, string>> ListDetailed()
	{
		EnsureLoaded();
		lock (_lock)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(_bans.Count);
			foreach (string ban in _bans)
			{
				_names.TryGetValue(ban, out var value);
				list.Add(new KeyValuePair<string, string>(ban, value ?? ""));
			}
			list.Sort((KeyValuePair<string, string> a, KeyValuePair<string, string> b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
			return list;
		}
	}

	private static void PersistUnlocked()
	{
		string filePath = FilePath;
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("# CrewBanList — persistent friend-code ban list");
			stringBuilder.AppendLine("# Format: friendCode, playerName, reason");
			foreach (string ban in _bans)
			{
				_names.TryGetValue(ban, out var value);
				_reasons.TryGetValue(ban, out var value2);
				stringBuilder.Append(ban);
				stringBuilder.Append(',');
				stringBuilder.Append(value ?? "");
				stringBuilder.Append(',');
				stringBuilder.Append(value2 ?? "");
				stringBuilder.AppendLine();
			}
			AtomicWrite(filePath, stringBuilder.ToString());
		}
		catch (Exception value3)
		{
			Debug.LogError(Object.op_Implicit($"[CrewBanList] Persist error: {value3}"));
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
