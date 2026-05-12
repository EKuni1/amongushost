using System;
using System.Runtime.CompilerServices;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace ModMenuCrew;

internal static class ServerGate
{
	private static string _renderKey = null;

	private static long _renderExpiresXor = 0L;

	private const long EXPIRES_SENTINEL = 6597278099616784425L;

	private const long RENDER_REL_SENTINEL = 4214978224969449510L;

	private static long _renderReceivedUnscaledXor = 4214978224969449510L;

	private static long _renderTtlMsXor = 4214978224969449510L;

	private const long RENDER_TTL_MAX_MS = 600000L;

	private static long _renderNonce = 0L;

	private static int _consecutiveFailures = 0;

	private static int _logThrottle = 0;

	private static long RenderExpires => _renderExpiresXor ^ 0x5B8E3D7A4C1F6029L;

	private static long RenderReceivedUnscaledMs => _renderReceivedUnscaledXor ^ 0x3A7E9C5D1B4F8026L;

	private static long RenderTtlMs => _renderTtlMsXor ^ 0x3A7E9C5D1B4F8026L;

	private static bool RenderHasRelativeTracking
	{
		get
		{
			if (_renderReceivedUnscaledXor != 4214978224969449510L)
			{
				return _renderTtlMsXor != 4214978224969449510L;
			}
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool CanRender()
	{
		if (string.IsNullOrEmpty(_renderKey) || _renderKey.Length != 64)
		{
			return false;
		}
		bool flag;
		if (RenderHasRelativeTracking)
		{
			long renderTtlMs = RenderTtlMs;
			if (renderTtlMs <= 0 || renderTtlMs > 600000)
			{
				flag = true;
			}
			else
			{
				long num = ModKeyValidator.MonotonicMs - RenderReceivedUnscaledMs;
				flag = num < 0 || num >= renderTtlMs;
			}
		}
		else
		{
			long num2 = ((ModKeyValidator.CachedFrameTimeMs > 0) ? ModKeyValidator.CachedFrameTimeMs : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) + ModKeyValidator.ServerTimeOffsetMs;
			long num3 = 300000L;
			flag = num2 > RenderExpires + num3;
		}
		if (flag)
		{
			if (_logThrottle++ % 300 == 0 && ModMenuCrewPlugin.Instance != null)
			{
				((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogWarning((object)"[ServerGate] x");
			}
			return false;
		}
		if (_renderNonce == 0L)
		{
			return false;
		}
		if (!ModKeyValidator.V())
		{
			return false;
		}
		if (_consecutiveFailures < 0)
		{
			Application.Quit();
			return false;
		}
		return true;
	}

	internal static void UpdateRenderPermission(string renderKey, long expires, long nonce, long ttlSeconds = 0L)
	{
		if (string.IsNullOrEmpty(renderKey) || renderKey.Length != 64 || expires <= 0 || nonce <= 0)
		{
			return;
		}
		_renderKey = renderKey;
		_renderExpiresXor = expires ^ 0x5B8E3D7A4C1F6029L;
		_renderNonce = nonce;
		_consecutiveFailures = 0;
		if (ttlSeconds > 0)
		{
			long num = ttlSeconds * 1000;
			if (num > 600000)
			{
				num = 600000L;
			}
			_renderReceivedUnscaledXor = ModKeyValidator.MonotonicMs ^ 0x3A7E9C5D1B4F8026L;
			_renderTtlMsXor = num ^ 0x3A7E9C5D1B4F8026L;
		}
		else
		{
			_renderReceivedUnscaledXor = 4214978224969449510L;
			_renderTtlMsXor = 4214978224969449510L;
		}
	}

	internal static void Revoke()
	{
		_renderKey = null;
		_renderExpiresXor = 0L;
		_renderNonce = 0L;
		_renderReceivedUnscaledXor = 4214978224969449510L;
		_renderTtlMsXor = 4214978224969449510L;
		ServerData.SetLoaded(loaded: false);
	}

	internal static bool IsNearExpiration()
	{
		if (string.IsNullOrEmpty(_renderKey))
		{
			return true;
		}
		if (RenderHasRelativeTracking)
		{
			long num = ModKeyValidator.MonotonicMs - RenderReceivedUnscaledMs;
			if (num >= 0)
			{
				return num > RenderTtlMs - 10000;
			}
			return true;
		}
		return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ModKeyValidator.ServerTimeOffsetMs > RenderExpires - 10000;
	}

	internal static void RecordFailure()
	{
		_consecutiveFailures++;
		if (_consecutiveFailures >= 3)
		{
			Revoke();
		}
	}

	internal static int GetRemainingSeconds()
	{
		if (string.IsNullOrEmpty(_renderKey))
		{
			return 0;
		}
		if (RenderHasRelativeTracking)
		{
			long num = ModKeyValidator.MonotonicMs - RenderReceivedUnscaledMs;
			if (num < 0)
			{
				return (int)(RenderTtlMs / 1000);
			}
			long num2 = RenderTtlMs - num;
			if (num2 <= 0)
			{
				return 0;
			}
			return (int)(num2 / 1000);
		}
		long num3 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ModKeyValidator.ServerTimeOffsetMs;
		long num4 = RenderExpires - num3;
		if (num4 <= 0)
		{
			return 0;
		}
		return (int)(num4 / 1000);
	}
}
