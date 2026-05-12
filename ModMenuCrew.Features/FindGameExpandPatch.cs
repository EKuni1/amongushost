using System;
using System.Reflection;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace ModMenuCrew.Features;

[HarmonyPatch]
internal static class FindGameExpandPatch
{
	internal const int TARGET_COUNT = 10;

	private static int _expandedInstanceId;

	private static PropertyInfo _containersProp;

	private static FieldInfo _containersField;

	private static PropertyInfo _parentContainerProp;

	private static FieldInfo _parentContainerField;

	private static bool _probeDone;

	private static float _dyOriginal;

	private static float _dyUsed;

	private static int _originalCount = 5;

	private static float _scrollY;

	private static float _scrollMin;

	private static float _scrollMax;

	private static bool _scrollRangeReady;

	private static PropertyInfo _refreshBtnProp;

	private static FieldInfo _refreshBtnField;

	private static bool _refreshPinned;

	private static bool IsContainerArrayType(Type ft)
	{
		if (ft == null)
		{
			return false;
		}
		if (ft == typeof(GameContainer[]))
		{
			return true;
		}
		if (ft == typeof(Il2CppReferenceArray<GameContainer>))
		{
			return true;
		}
		if (ft.IsArray && ft.GetElementType() == typeof(GameContainer))
		{
			return true;
		}
		if (ft.IsGenericType && ft.GetGenericTypeDefinition() == typeof(Il2CppReferenceArray<>))
		{
			return ft.GetGenericArguments()[0] == typeof(GameContainer);
		}
		return false;
	}

	private static void ProbeOnce()
	{
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Expected O, but got Unknown
		if (_probeDone)
		{
			return;
		}
		_probeDone = true;
		Type typeFromHandle = typeof(FindAGameManager);
		PropertyInfo property = typeFromHandle.GetProperty("gameContainers", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null && IsContainerArrayType(property.PropertyType))
		{
			_containersProp = property;
		}
		else
		{
			PropertyInfo[] properties = typeFromHandle.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (IsContainerArrayType(propertyInfo.PropertyType))
				{
					_containersProp = propertyInfo;
					break;
				}
			}
		}
		if (_containersProp == null)
		{
			FieldInfo field = typeFromHandle.GetField("gameContainers", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null && IsContainerArrayType(field.FieldType))
			{
				_containersField = field;
			}
			else
			{
				FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					if (IsContainerArrayType(fieldInfo.FieldType))
					{
						_containersField = fieldInfo;
						break;
					}
				}
			}
		}
		PropertyInfo property2 = typeFromHandle.GetProperty("container", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (property2 != null && typeof(Transform).IsAssignableFrom(property2.PropertyType))
		{
			_parentContainerProp = property2;
		}
		else
		{
			PropertyInfo[] properties = typeFromHandle.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo propertyInfo2 in properties)
			{
				if (typeof(Transform).IsAssignableFrom(propertyInfo2.PropertyType) && propertyInfo2.Name.ToLower().Contains("container"))
				{
					_parentContainerProp = propertyInfo2;
					break;
				}
			}
		}
		if (_parentContainerProp == null)
		{
			FieldInfo field2 = typeFromHandle.GetField("container", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (field2 != null && typeof(Transform).IsAssignableFrom(field2.FieldType))
			{
				_parentContainerField = field2;
			}
			else
			{
				FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo2 in fields)
				{
					if (typeof(Transform).IsAssignableFrom(fieldInfo2.FieldType) && fieldInfo2.Name.ToLower().Contains("container"))
					{
						_parentContainerField = fieldInfo2;
						break;
					}
				}
			}
		}
		PropertyInfo property3 = typeFromHandle.GetProperty("refreshButton", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (property3 != null && typeof(Component).IsAssignableFrom(property3.PropertyType))
		{
			_refreshBtnProp = property3;
		}
		else
		{
			PropertyInfo[] properties = typeFromHandle.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo propertyInfo3 in properties)
			{
				if (typeof(Component).IsAssignableFrom(propertyInfo3.PropertyType) && propertyInfo3.Name.ToLower().Contains("refresh"))
				{
					_refreshBtnProp = propertyInfo3;
					break;
				}
			}
		}
		if (_refreshBtnProp == null)
		{
			FieldInfo field3 = typeFromHandle.GetField("refreshButton", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (field3 != null && typeof(Component).IsAssignableFrom(field3.FieldType))
			{
				_refreshBtnField = field3;
			}
			else
			{
				FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo3 in fields)
				{
					if (typeof(Component).IsAssignableFrom(fieldInfo3.FieldType) && fieldInfo3.Name.ToLower().Contains("refresh"))
					{
						_refreshBtnField = fieldInfo3;
						break;
					}
				}
			}
		}
		ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
		if (instance == null)
		{
			return;
		}
		ManualLogSource log = ((BasePlugin)instance).Log;
		if (log != null)
		{
			bool flag = default(bool);
			BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(52, 3, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[FindGameExpand] probe: containers=");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(_containersProp?.Name ?? _containersField?.Name ?? "NULL");
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" parent=");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(_parentContainerProp?.Name ?? _parentContainerField?.Name ?? "NULL");
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" refresh=");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(_refreshBtnProp?.Name ?? _refreshBtnField?.Name ?? "NULL");
			}
			log.LogInfo(val);
		}
	}

	private static Transform GetRefreshBtnTransform(FindAGameManager inst)
	{
		Component val = null;
		if (_refreshBtnProp != null)
		{
			object? value = _refreshBtnProp.GetValue(inst);
			val = (Component)((value is Component) ? value : null);
		}
		else if (_refreshBtnField != null)
		{
			object? value2 = _refreshBtnField.GetValue(inst);
			val = (Component)((value2 is Component) ? value2 : null);
		}
		if (!((Object)(object)val != (Object)null))
		{
			return null;
		}
		return val.transform;
	}

	private static Il2CppReferenceArray<GameContainer> GetContainers(FindAGameManager inst)
	{
		object obj = ((_containersProp != null) ? _containersProp.GetValue(inst) : _containersField?.GetValue(inst));
		if (obj is Il2CppReferenceArray<GameContainer> result)
		{
			return result;
		}
		if (obj is GameContainer[] array)
		{
			Il2CppReferenceArray<GameContainer> val = new Il2CppReferenceArray<GameContainer>((long)array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				((Il2CppArrayBase<GameContainer>)(object)val)[i] = array[i];
			}
			return val;
		}
		return null;
	}

	private static void SetContainers(FindAGameManager inst, Il2CppReferenceArray<GameContainer> v)
	{
		if (_containersProp != null)
		{
			_containersProp.SetValue(inst, v);
		}
		else
		{
			_containersField?.SetValue(inst, v);
		}
	}

	private static Transform GetParentContainer(FindAGameManager inst)
	{
		if (_parentContainerProp != null)
		{
			object? value = _parentContainerProp.GetValue(inst);
			return (Transform)((value is Transform) ? value : null);
		}
		object? obj = _parentContainerField?.GetValue(inst);
		return (Transform)((obj is Transform) ? obj : null);
	}

	[HarmonyPatch(typeof(FindAGameManager), "Start")]
	[HarmonyPostfix]
	private static void OnStart()
	{
		_expandedInstanceId = 0;
		_scrollY = 0f;
		_scrollRangeReady = false;
		_refreshPinned = false;
	}

	[HarmonyPatch(typeof(FindAGameManager), "HandleList")]
	[HarmonyPrefix]
	private static void PreHandleList(FindAGameManager __instance)
	{
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Expected O, but got Unknown
		if ((Object)(object)__instance == (Object)null)
		{
			return;
		}
		ProbeOnce();
		int instanceID = ((Object)__instance).GetInstanceID();
		if (_expandedInstanceId == instanceID)
		{
			return;
		}
		if (_containersProp == null && _containersField == null)
		{
			_expandedInstanceId = instanceID;
			return;
		}
		bool flag = default(bool);
		try
		{
			Il2CppReferenceArray<GameContainer> containers = GetContainers(__instance);
			if (containers == null || ((Il2CppArrayBase<GameContainer>)(object)containers).Length == 0)
			{
				_expandedInstanceId = instanceID;
				return;
			}
			if (((Il2CppArrayBase<GameContainer>)(object)containers).Length >= 10)
			{
				_expandedInstanceId = instanceID;
				return;
			}
			GameContainer val = ((Il2CppArrayBase<GameContainer>)(object)containers)[0];
			if ((Object)(object)val == (Object)null)
			{
				_expandedInstanceId = instanceID;
				return;
			}
			_originalCount = ((Il2CppArrayBase<GameContainer>)(object)containers).Length;
			GameObject gameObject = ((Component)val).gameObject;
			Transform transform = gameObject.transform;
			Transform parent = transform.parent;
			float num = 0f;
			if (((Il2CppArrayBase<GameContainer>)(object)containers).Length > 1 && (Object)(object)((Il2CppArrayBase<GameContainer>)(object)containers)[1] != (Object)null)
			{
				num = ((Component)((Il2CppArrayBase<GameContainer>)(object)containers)[1]).transform.localPosition.y - transform.localPosition.y;
			}
			if (Mathf.Approximately(num, 0f))
			{
				num = -1.1f;
			}
			_dyOriginal = num;
			_dyUsed = num;
			Vector3 localScale = transform.localScale;
			Il2CppReferenceArray<GameContainer> val2 = new Il2CppReferenceArray<GameContainer>(10L);
			for (int i = 0; i < ((Il2CppArrayBase<GameContainer>)(object)containers).Length; i++)
			{
				((Il2CppArrayBase<GameContainer>)(object)val2)[i] = ((Il2CppArrayBase<GameContainer>)(object)containers)[i];
			}
			for (int j = ((Il2CppArrayBase<GameContainer>)(object)containers).Length; j < 10; j++)
			{
				GameObject val3 = Object.Instantiate<GameObject>(gameObject, parent);
				((Object)val3).name = ((Object)gameObject).name + "_mmc_ext_" + j;
				Transform transform2 = val3.transform;
				transform2.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + (float)j * num, transform.localPosition.z);
				transform2.localScale = localScale;
				val3.SetActive(false);
				GameContainer component = val3.GetComponent<GameContainer>();
				if ((Object)(object)component == (Object)null)
				{
					Object.Destroy((Object)(object)val3);
					_expandedInstanceId = instanceID;
					return;
				}
				((Il2CppArrayBase<GameContainer>)(object)val2)[j] = component;
			}
			SetContainers(__instance, val2);
			_expandedInstanceId = instanceID;
			int num2 = 10 - _originalCount;
			_scrollMin = 0f;
			_scrollMax = Mathf.Abs(num) * (float)num2;
			_scrollRangeReady = true;
			_refreshPinned = true;
			ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
			if (instance == null)
			{
				return;
			}
			ManualLogSource log = ((BasePlugin)instance).Log;
			if (log != null)
			{
				BepInExInfoLogInterpolatedStringHandler val4 = new BepInExInfoLogInterpolatedStringHandler(63, 4, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val4).AppendLiteral("[FindGameExpand] expanded ");
					((BepInExLogInterpolatedStringHandler)val4).AppendFormatted<int>(_originalCount);
					((BepInExLogInterpolatedStringHandler)val4).AppendLiteral(" -> ");
					((BepInExLogInterpolatedStringHandler)val4).AppendFormatted<int>(10);
					((BepInExLogInterpolatedStringHandler)val4).AppendLiteral(", vanilla layout, scroll [0,");
					((BepInExLogInterpolatedStringHandler)val4).AppendFormatted<float>(_scrollMax, "F2");
					((BepInExLogInterpolatedStringHandler)val4).AppendLiteral("] dy=");
					((BepInExLogInterpolatedStringHandler)val4).AppendFormatted<float>(num, "F2");
				}
				log.LogInfo(val4);
			}
		}
		catch (Exception ex)
		{
			ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
			if (instance != null)
			{
				ManualLogSource log = ((BasePlugin)instance).Log;
				if (log != null)
				{
					BepInExWarningLogInterpolatedStringHandler val5 = new BepInExWarningLogInterpolatedStringHandler(32, 1, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val5).AppendLiteral("[FindGameExpand] expand failed: ");
						((BepInExLogInterpolatedStringHandler)val5).AppendFormatted<string>(ex.Message);
					}
					log.LogWarning(val5);
				}
			}
			_expandedInstanceId = instanceID;
		}
	}

	[HarmonyPatch(typeof(FindAGameManager), "Update")]
	[HarmonyPostfix]
	private static void PostUpdate(FindAGameManager __instance)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)__instance == (Object)null || !_scrollRangeReady)
		{
			return;
		}
		try
		{
			if (_parentContainerProp == null && _parentContainerField == null)
			{
				return;
			}
			float num = Input.mouseScrollDelta.y;
			if (Input.GetKey((KeyCode)281))
			{
				num -= 0.5f;
			}
			if (Input.GetKey((KeyCode)280))
			{
				num += 0.5f;
			}
			if (Input.GetKey((KeyCode)274))
			{
				num -= 0.3f;
			}
			if (Input.GetKey((KeyCode)273))
			{
				num += 0.3f;
			}
			if (!Mathf.Approximately(num, 0f))
			{
				_scrollY = Mathf.Clamp(_scrollY - num * 0.8f, _scrollMin, _scrollMax);
				Transform parentContainer = GetParentContainer(__instance);
				if (!((Object)(object)parentContainer == (Object)null))
				{
					Vector3 localPosition = parentContainer.localPosition;
					parentContainer.localPosition = new Vector3(localPosition.x, _scrollY, localPosition.z);
				}
			}
		}
		catch
		{
		}
	}
}
