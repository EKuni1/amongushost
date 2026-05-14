using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace ModMenuCrew.Features;

internal static class ChatBlacklistFileService
{
	private const string FILE_NAME = "WordsBlackList.txt";

	private const string SUBFOLDER = "ModMenuCrew";

	private const int MAX_FILE_BYTES = 1000000;

	private const int MAX_WORD_COUNT = 500;

	private const int MAX_WORD_LENGTH = 32;

	private static readonly object _lock = new object();

	private static bool _loaded;

	private static bool _persisting;

	private static bool _errorLogged;

	internal static string FilePath
	{
		get
		{
			try
			{
				return Path.Combine(GetGameRootSafe(), "ModMenuCrew", "WordsBlackList.txt");
			}
			catch
			{
				return null;
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

	private static void LogErrorOnce(string prefix, Exception e)
	{
		if (_errorLogged)
		{
			return;
		}
		_errorLogged = true;
		try
		{
			Debug.LogError(Object.op_Implicit($"[ChatBlacklistFile] {prefix}: {e.GetType().Name}: {e.Message}"));
		}
		catch
		{
		}
	}

	internal static void EnsureLoaded()
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
			try
			{
				string filePath = FilePath;
				if (string.IsNullOrEmpty(filePath))
				{
					_loaded = true;
					return;
				}
				string directoryName = Path.GetDirectoryName(filePath);
				if (!string.IsNullOrEmpty(directoryName))
				{
					try
					{
						if (!Directory.Exists(directoryName))
						{
							Directory.CreateDirectory(directoryName);
						}
					}
					catch (Exception e)
					{
						LogErrorOnce("CreateDirectory", e);
						_loaded = true;
						return;
					}
				}
				ConfigEntry<string> autoKickChatWordList = CheatConfig.AutoKickChatWordList;
				if (autoKickChatWordList == null)
				{
					_loaded = true;
					return;
				}
				bool flag = false;
				try
				{
					flag = File.Exists(filePath);
				}
				catch
				{
				}
				_persisting = true;
				try
				{
					if (flag)
					{
						string text = null;
						try
						{
							FileInfo fileInfo = new FileInfo(filePath);
							if (fileInfo.Length > 1000000)
							{
								LogErrorOnce("File too large, truncating", new IOException($"{fileInfo.Length} > {1000000}"));
								using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
								byte[] array = new byte[1000000];
								int count = fileStream.Read(array, 0, 1000000);
								text = Encoding.UTF8.GetString(array, 0, count);
							}
							else
							{
								text = File.ReadAllText(filePath);
							}
						}
						catch (Exception e2)
						{
							LogErrorOnce("ReadAllText", e2);
							text = null;
						}
						if (!string.IsNullOrEmpty(text))
						{
							List<string> list = ParseFile(text);
							if (list.Count > 0)
							{
								try
								{
									autoKickChatWordList.Value = string.Join(",", list);
								}
								catch (Exception e3)
								{
									LogErrorOnce("cfg.Value set", e3);
								}
							}
						}
					}
					else
					{
						WriteFileSafe(filePath, ParseCsv(autoKickChatWordList.Value ?? ""));
					}
				}
				finally
				{
					_persisting = false;
				}
				try
				{
					autoKickChatWordList.SettingChanged -= OnConfigChanged;
					autoKickChatWordList.SettingChanged += OnConfigChanged;
				}
				catch (Exception e4)
				{
					LogErrorOnce("SettingChanged wire", e4);
				}
			}
			catch (Exception e5)
			{
				LogErrorOnce("EnsureLoaded", e5);
			}
			_loaded = true;
		}
	}

	private static void OnConfigChanged(object sender, EventArgs e)
	{
		if (_persisting)
		{
			return;
		}
		try
		{
			SaveFromConfig();
		}
		catch (Exception e2)
		{
			LogErrorOnce("OnConfigChanged", e2);
		}
	}

	internal static void SaveFromConfig()
	{
		lock (_lock)
		{
			if (_persisting)
			{
				return;
			}
			_persisting = true;
			try
			{
				string filePath = FilePath;
				if (string.IsNullOrEmpty(filePath))
				{
					return;
				}
				string directoryName = Path.GetDirectoryName(filePath);
				if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
				{
					try
					{
						Directory.CreateDirectory(directoryName);
					}
					catch (Exception e)
					{
						LogErrorOnce("CreateDirectory on save", e);
						return;
					}
				}
				List<string> words = ParseCsv(CheatConfig.AutoKickChatWordList?.Value ?? "");
				WriteFileSafe(filePath, words);
			}
			catch (Exception e2)
			{
				LogErrorOnce("SaveFromConfig", e2);
			}
			finally
			{
				_persisting = false;
			}
		}
	}

	internal static void ReloadFromFile()
	{
		lock (_lock)
		{
			try
			{
				string filePath = FilePath;
				if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
				{
					return;
				}
				string content;
				try
				{
					content = File.ReadAllText(filePath);
				}
				catch (Exception e)
				{
					LogErrorOnce("Reload ReadAllText", e);
					return;
				}
				List<string> values = ParseFile(content);
				ConfigEntry<string> autoKickChatWordList = CheatConfig.AutoKickChatWordList;
				if (autoKickChatWordList == null)
				{
					return;
				}
				_persisting = true;
				try
				{
					autoKickChatWordList.Value = string.Join(",", values);
				}
				catch (Exception e2)
				{
					LogErrorOnce("Reload cfg.Value set", e2);
				}
				finally
				{
					_persisting = false;
				}
			}
			catch (Exception e3)
			{
				LogErrorOnce("ReloadFromFile", e3);
			}
		}
	}

	private static bool IsValidWord(string w)
	{
		if (string.IsNullOrWhiteSpace(w))
		{
			return false;
		}
		if (w.Length > 32)
		{
			return false;
		}
		foreach (char c in w)
		{
			if (c == '\0' || c == '\r' || c == '\n')
			{
				return false;
			}
		}
		return true;
	}

	private static List<string> ParseFile(string content)
	{
		List<string> list = new List<string>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (string.IsNullOrEmpty(content))
		{
			return list;
		}
		if (content.Length > 1000000)
		{
			content = content.Substring(0, 1000000);
		}
		string[] array = content.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			if (list.Count >= 500)
			{
				break;
			}
			string text = array[i].Trim();
			if (string.IsNullOrEmpty(text) || text.StartsWith("#") || text.StartsWith("//"))
			{
				continue;
			}
			string[] array2 = text.Split(',');
			foreach (string text2 in array2)
			{
				if (list.Count >= 500)
				{
					break;
				}
				string text3 = text2.Trim();
				if (IsValidWord(text3) && hashSet.Add(text3))
				{
					list.Add(text3);
				}
			}
		}
		return list;
	}

	private static List<string> ParseCsv(string csv)
	{
		List<string> list = new List<string>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (string.IsNullOrEmpty(csv))
		{
			return list;
		}
		string[] array = csv.Split(new char[3] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			if (list.Count >= 500)
			{
				break;
			}
			string text2 = text.Trim();
			if (IsValidWord(text2) && hashSet.Add(text2))
			{
				list.Add(text2);
			}
		}
		return list;
	}

	private static void WriteFileSafe(string path, List<string> words)
	{
		StringBuilder stringBuilder = new StringBuilder(Math.Max(256, words.Count * 16));
		stringBuilder.AppendLine("# ModMenuCrew — Chat Word Blacklist");
		stringBuilder.AppendLine("# Auto-kicks players whose chat contains a listed word.");
		stringBuilder.AppendLine("# Format: one word per line OR comma-separated. Prefix '*' enables substring match.");
		stringBuilder.AppendLine("# Lines starting with # or // are comments. Max 500 entries, 32 chars each.");
		stringBuilder.AppendLine("# Entries with spaces (phrases) are matched as substring automatically.");
		stringBuilder.AppendLine("# Synced with in-game UI. Edits here apply after next config change (or restart).");
		stringBuilder.AppendLine();
		int num = Math.Min(words?.Count ?? 0, 500);
		for (int i = 0; i < num; i++)
		{
			string text = words[i];
			if (IsValidWord(text))
			{
				stringBuilder.AppendLine(text);
			}
		}
		AtomicWrite(path, stringBuilder.ToString());
	}

	private static void AtomicWrite(string path, string content)
	{
		string text = path + ".tmp." + Guid.NewGuid().ToString("N").Substring(0, 8);
		try
		{
			File.WriteAllText(text, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			bool flag = false;
			if (File.Exists(path))
			{
				try
				{
					File.Replace(text, path, null);
					flag = true;
				}
				catch
				{
				}
			}
			if (flag)
			{
				return;
			}
			try
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch
			{
			}
			File.Move(text, path);
		}
		catch (Exception e)
		{
			LogErrorOnce("AtomicWrite", e);
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
