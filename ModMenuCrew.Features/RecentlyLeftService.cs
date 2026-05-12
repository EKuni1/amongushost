using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModMenuCrew.Features;

public static class RecentlyLeftService
{
	public sealed class Entry
	{
		public string FriendCode;

		public string PlayerName;

		public float LeftAt;
	}

	private static readonly object _lock = new object();

	private static readonly List<Entry> _entries = new List<Entry>(32);

	private const int MAX_ENTRIES = 20;

	private static int _stateVersion;

	public static int Count
	{
		get
		{
			lock (_lock)
			{
				return _entries.Count;
			}
		}
	}

	public static void Track(string friendCode, string playerName)
	{
		if (string.IsNullOrWhiteSpace(friendCode))
		{
			return;
		}
		string text = friendCode.Trim();
		lock (_lock)
		{
			for (int num = _entries.Count - 1; num >= 0; num--)
			{
				if (string.Equals(_entries[num].FriendCode, text, StringComparison.OrdinalIgnoreCase))
				{
					_entries.RemoveAt(num);
				}
			}
			_entries.Add(new Entry
			{
				FriendCode = text,
				PlayerName = (playerName ?? "").Trim(),
				LeftAt = Time.realtimeSinceStartup
			});
			while (_entries.Count > 20)
			{
				_entries.RemoveAt(0);
			}
			_stateVersion++;
		}
	}

	public static bool RemoveAt(int index)
	{
		lock (_lock)
		{
			if (index < 0 || index >= _entries.Count)
			{
				return false;
			}
			_entries.RemoveAt(index);
			_stateVersion++;
			return true;
		}
	}

	public static bool RemoveByFriendCode(string friendCode)
	{
		if (string.IsNullOrWhiteSpace(friendCode))
		{
			return false;
		}
		string b = friendCode.Trim();
		lock (_lock)
		{
			for (int i = 0; i < _entries.Count; i++)
			{
				if (string.Equals(_entries[i].FriendCode, b, StringComparison.OrdinalIgnoreCase))
				{
					_entries.RemoveAt(i);
					_stateVersion++;
					return true;
				}
			}
			return false;
		}
	}

	public static void Clear()
	{
		lock (_lock)
		{
			if (_entries.Count != 0)
			{
				_entries.Clear();
				_stateVersion++;
			}
		}
	}

	public static List<Entry> ListDetailed()
	{
		lock (_lock)
		{
			List<Entry> list = new List<Entry>(_entries.Count);
			for (int num = _entries.Count - 1; num >= 0; num--)
			{
				list.Add(_entries[num]);
			}
			return list;
		}
	}

	public static int GetStateHash()
	{
		lock (_lock)
		{
			return _stateVersion;
		}
	}

	public static Entry GetAt(int index)
	{
		lock (_lock)
		{
			int num = _entries.Count - 1 - index;
			if (num < 0 || num >= _entries.Count)
			{
				return null;
			}
			Entry entry = _entries[num];
			return new Entry
			{
				FriendCode = entry.FriendCode,
				PlayerName = entry.PlayerName,
				LeftAt = entry.LeftAt
			};
		}
	}
}
