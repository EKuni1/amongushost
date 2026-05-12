using System;
using Il2CppSystem;
using ModMenuCrew.UI.Managers;
using UnityEngine;

namespace ModMenuCrew.Features;

public class MMCRendererComponent : MonoBehaviour
{
	private static MMCRendererComponent _instance;

	private static GameObject _gameObject;

	private float _lastUpdateTime;

	private const float UPDATE_INTERVAL = 0.1f;

	public MMCRendererComponent(IntPtr ptr)
		: base(ptr)
	{
	}

	public static void EnsureExists()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if ((Object)(object)_instance != (Object)null)
		{
			return;
		}
		try
		{
			_gameObject = new GameObject("MMCRenderer");
			((Object)_gameObject).hideFlags = (HideFlags)61;
			Object.DontDestroyOnLoad((Object)(object)_gameObject);
			_instance = _gameObject.AddComponent<MMCRendererComponent>();
			Debug.Log(Object.op_Implicit("[MMCIdentification] MMCRendererComponent inicializado automaticamente."));
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[MMCIdentification] Erro ao criar MMCRendererComponent: " + ex.Message));
		}
	}

	private void Update()
	{
		if (Time.time - _lastUpdateTime < 0.1f)
		{
			return;
		}
		_lastUpdateTime = Time.time;
		try
		{
			MMCIdentification.Update();
			if (!GameCheats.IsRevealSusActive)
			{
				MMCIdentification.UpdateMMCNameTags();
			}
		}
		catch
		{
		}
		try
		{
			PlayerInfoDisplay.UpdatePlayerInfoTags();
		}
		catch
		{
		}
		try
		{
			MiscNameService.UpdateLocalName();
		}
		catch
		{
		}
		try
		{
			SettingsTab.UpdateAutoSave();
		}
		catch
		{
		}
	}

	private void OnDestroy()
	{
		_instance = null;
		_gameObject = null;
	}
}
