using System;
using UnityEngine;

namespace ModMenuCrew.Easing;

public static class Easing
{
	public static float SmoothStep(float t)
	{
		return t * t * (3f - 2f * t);
	}

	public static float EaseOutExpo(float t)
	{
		if (!(t >= 1f))
		{
			if (t != 0f)
			{
				return 1f - Mathf.Pow(2f, -10f * t);
			}
			return 0f;
		}
		return 1f;
	}

	public static float EaseOutBack(float t)
	{
		float num = 1.70158f;
		float num2 = num + 1f;
		return 1f + num2 * Mathf.Pow(t - 1f, 3f) + num * Mathf.Pow(t - 1f, 2f);
	}

	public static float EaseInOutCirc(float t)
	{
		if (!(t < 0.5f))
		{
			return (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;
		}
		return (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * t, 2f))) / 2f;
	}

	public static float Lerp(float a, float b, float t, Func<float, float> easing = null)
	{
		return a + (b - a) * (easing?.Invoke(t) ?? t);
	}

	public static float EaseInOutExpo(float t)
	{
		if (!(t < 0.5f))
		{
			return 1f - EaseOutExpo(2f - t * 2f) * 0.5f;
		}
		return EaseInExpo(t * 2f) * 0.5f;
	}

	public static float EaseInExpo(float t)
	{
		if (!(t <= 0f))
		{
			return Mathf.Pow(2f, 10f * (t - 1f));
		}
		return 0f;
	}

	public static float EaseOutQuad(float t)
	{
		return 1f - (1f - t) * (1f - t);
	}

	public static float EaseOutCubic(float t)
	{
		float num = t - 1f;
		return 1f + num * num * num;
	}

	public static float CrtFlicker(float time)
	{
		return 1f - (Mathf.Sin(time * 60f) * 0.008f + Mathf.Sin(time * 127f) * 0.004f);
	}

	public static float Damp(float current, float target, float speed, float dt)
	{
		return Mathf.Lerp(current, target, dt * speed);
	}

	public static float SinePulse(float time, float freq, float amplitude, float baseline)
	{
		return baseline + Mathf.Sin(time * freq) * amplitude;
	}

	public static float Bob(float time, float freq, float amplitude, float offset = 0f)
	{
		return Mathf.Sin(time * freq + offset) * amplitude;
	}

	public static float MoveTowards(float current, float target, float maxDelta)
	{
		return Mathf.MoveTowards(current, target, maxDelta);
	}
}
