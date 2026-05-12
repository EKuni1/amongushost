using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModMenuCrew.Features;

public static class HostOutboundBudget
{
	public const int SUBMSG_PER_SEC_CAP = 28;

	public const float SAFE_UTIL_THRESHOLD = 0.8f;

	public const float HIGH_UTIL_THRESHOLD = 0.55f;

	private static readonly Queue<KeyValuePair<float, int>> _log = new Queue<KeyValuePair<float, int>>(256);

	public static void Record(int subMsgs)
	{
		if (subMsgs > 0)
		{
			float time = Time.time;
			_log.Enqueue(new KeyValuePair<float, int>(time, subMsgs));
			Trim(time);
		}
	}

	private static void Trim(float now)
	{
		while (_log.Count > 0 && now - _log.Peek().Key > 1f)
		{
			_log.Dequeue();
		}
	}

	public static int CurrentSubMsgsPerSec()
	{
		Trim(Time.time);
		int num = 0;
		foreach (KeyValuePair<float, int> item in _log)
		{
			num += item.Value;
		}
		return num;
	}

	public static int Remaining()
	{
		return Math.Max(0, 28 - CurrentSubMsgsPerSec());
	}

	public static bool CanSend(int expected)
	{
		return CurrentSubMsgsPerSec() + expected <= 28;
	}

	public static float Utilization()
	{
		return (float)CurrentSubMsgsPerSec() / 28f;
	}

	public static bool IsSaturated()
	{
		return Utilization() >= 0.8f;
	}

	public static bool IsBusy()
	{
		return Utilization() >= 0.55f;
	}

	public static void Reset()
	{
		_log.Clear();
	}
}
