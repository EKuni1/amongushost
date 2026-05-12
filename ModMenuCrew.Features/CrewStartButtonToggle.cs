using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using InnerNet;
using TMPro;
using UnityEngine;

namespace ModMenuCrew.Features;

[HarmonyPatch(typeof(GameStartManager))]
public static class CrewStartButtonToggle
{
	private static bool _inCountdown;

	private static string _lastLabel;

	private static int _lastN = -1;

	private static readonly List<SpriteRenderer> _hiddenSprites = new List<SpriteRenderer>();

	public static bool InCountdown => _inCountdown;

	private static void HideButtonSprites(GameStartManager gsm)
	{
		if ((Object)(object)((gsm != null) ? gsm.StartButton : null) == (Object)null)
		{
			return;
		}
		RestoreButtonSprites();
		try
		{
			foreach (SpriteRenderer componentsInChild in ((Component)gsm.StartButton).gameObject.GetComponentsInChildren<SpriteRenderer>(true))
			{
				if (!((Object)(object)componentsInChild == (Object)null) && ((Renderer)componentsInChild).enabled)
				{
					_hiddenSprites.Add(componentsInChild);
					((Renderer)componentsInChild).enabled = false;
				}
			}
		}
		catch
		{
		}
	}

	private static void RestoreButtonSprites()
	{
		for (int i = 0; i < _hiddenSprites.Count; i++)
		{
			SpriteRenderer val = _hiddenSprites[i];
			try
			{
				if ((Object)(object)val != (Object)null)
				{
					((Renderer)val).enabled = true;
				}
			}
			catch
			{
			}
		}
		_hiddenSprites.Clear();
	}

	private static string GetCancelText()
	{
		try
		{
			if (DestroyableSingleton<TranslationController>.InstanceExists)
			{
				return DestroyableSingleton<TranslationController>.Instance.GetString((StringNames)149, (Il2CppReferenceArray<Object>)null);
			}
		}
		catch
		{
		}
		return "Cancel";
	}

	private static string GetStartText()
	{
		try
		{
			if (DestroyableSingleton<TranslationController>.InstanceExists)
			{
				return DestroyableSingleton<TranslationController>.Instance.GetString((StringNames)2171, (Il2CppReferenceArray<Object>)null);
			}
		}
		catch
		{
		}
		return "Start";
	}

	private static int ExtractNumberFrom(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return 0;
		}
		int result = 0;
		int num = 0;
		bool flag = false;
		foreach (char c in s)
		{
			if (c >= '0' && c <= '9')
			{
				num = num * 10 + (c - 48);
				flag = true;
			}
			else if (flag)
			{
				result = num;
				num = 0;
				flag = false;
			}
		}
		if (flag)
		{
			result = num;
		}
		return result;
	}

	private static void ExitCountdown()
	{
		_inCountdown = false;
		_lastLabel = null;
		_lastN = -1;
		RestoreButtonSprites();
	}

	[HarmonyPatch("BeginGame")]
	[HarmonyPrefix]
	public static bool Prefix_BeginGame(GameStartManager __instance)
	{
		try
		{
			if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				return true;
			}
			if (!_inCountdown)
			{
				return true;
			}
			__instance.ResetStartState();
			ExitCountdown();
			if ((Object)(object)__instance.StartButton != (Object)null)
			{
				((Component)__instance.StartButton).gameObject.SetActive(true);
				__instance.StartButton.ChangeButtonText(GetStartText());
			}
			try
			{
				NotifyUtils.Warning("Start cancelled");
			}
			catch
			{
			}
			return false;
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[CrewMod] BeginGame prefix error: {value}"));
			return true;
		}
	}

	[HarmonyPatch("ReallyBegin")]
	[HarmonyPostfix]
	public static void Postfix_ReallyBegin(GameStartManager __instance)
	{
		try
		{
			if (!((Object)(object)AmongUsClient.Instance == (Object)null) && ((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				_inCountdown = true;
				_lastLabel = null;
				_lastN = -1;
				if ((Object)(object)__instance.StartButton != (Object)null)
				{
					((Component)__instance.StartButton).gameObject.SetActive(true);
					__instance.StartButton.SetButtonEnableState(true);
					__instance.StartButton.ChangeButtonText(GetCancelText());
					HideButtonSprites(__instance);
				}
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[CrewMod] ReallyBegin postfix error: {value}"));
		}
	}

	[HarmonyPatch("FinallyBegin")]
	[HarmonyPostfix]
	public static void Postfix_FinallyBegin()
	{
		ExitCountdown();
	}

	[HarmonyPatch("ResetStartState")]
	[HarmonyPostfix]
	public static void Postfix_ResetStartState(GameStartManager __instance)
	{
		if (!_inCountdown)
		{
			return;
		}
		ExitCountdown();
		try
		{
			if ((Object)(object)__instance != (Object)null && (Object)(object)__instance.StartButton != (Object)null)
			{
				__instance.StartButton.ChangeButtonText(GetStartText());
			}
		}
		catch
		{
		}
	}

	[HarmonyPatch("Update")]
	[HarmonyPrefix]
	public static void Prefix_Update(GameStartManager __instance)
	{
		try
		{
			if (!((Object)(object)__instance == (Object)null) && (Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				__instance.MinPlayers = 1;
			}
		}
		catch
		{
		}
	}

	[HarmonyPatch("Update")]
	[HarmonyPostfix]
	public static void Postfix_Update(GameStartManager __instance)
	{
		if (!_inCountdown)
		{
			return;
		}
		try
		{
			if ((Object)(object)__instance == (Object)null)
			{
				return;
			}
			if ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost)
			{
				ExitCountdown();
			}
			else
			{
				if ((Object)(object)__instance.StartButton == (Object)null)
				{
					return;
				}
				GameObject gameObject = ((Component)__instance.StartButton).gameObject;
				if (!gameObject.activeSelf)
				{
					gameObject.SetActive(true);
				}
				__instance.StartButton.SetButtonEnableState(true);
				int num = 0;
				if ((Object)(object)__instance.GameStartText != (Object)null)
				{
					num = ExtractNumberFrom(((TMP_Text)__instance.GameStartText).text);
				}
				if (num != _lastN)
				{
					_lastN = num;
					_lastLabel = ((num > 0) ? $"{GetCancelText()} {num}" : GetCancelText());
					__instance.StartButton.ChangeButtonText(_lastLabel);
				}
				if ((Object)(object)__instance.GameStartText != (Object)null && _lastLabel != null)
				{
					TMP_Text gameStartText = (TMP_Text)(object)__instance.GameStartText;
					if ((object)gameStartText.text != _lastLabel && gameStartText.text != _lastLabel)
					{
						gameStartText.text = _lastLabel;
					}
				}
			}
		}
		catch
		{
		}
	}
}
