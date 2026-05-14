using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenuCrew.Patches;

public class ModTooltipHandler : MonoBehaviour
{
	private TextMeshPro _textComponent;

	private Camera _mainCamera;

	private GameObject _tooltipObject;

	private TextMeshProUGUI _tooltipText;

	private RectTransform _tooltipRect;

	private float _lastUpdateTime;

	private const float UPDATE_INTERVAL = 0.05f;

	private int _lastLinkIndex = -1;

	private static readonly Vector3 TOOLTIP_OFFSET = new Vector3(15f, 15f, 0f);

	public ModTooltipHandler(IntPtr ptr)
		: base(ptr)
	{
	}

	public void Setup(TextMeshPro textComponent)
	{
		_textComponent = textComponent;
		_mainCamera = null;
	}

	private void Update()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_textComponent == (Object)null || !((Component)this).gameObject.activeInHierarchy || Time.time - _lastUpdateTime < 0.05f)
		{
			return;
		}
		_lastUpdateTime = Time.time;
		if ((Object)(object)_mainCamera == (Object)null)
		{
			HudManager instance = DestroyableSingleton<HudManager>.Instance;
			_mainCamera = ((instance != null) ? instance.UICamera : null) ?? Camera.main;
		}
		if ((Object)(object)_mainCamera == (Object)null)
		{
			return;
		}
		int num = TMP_TextUtilities.FindIntersectingLink((TMP_Text)(object)_textComponent, Input.mousePosition, _mainCamera);
		if (num == _lastLinkIndex && num == -1)
		{
			return;
		}
		_lastLinkIndex = num;
		if (num != -1)
		{
			TMP_TextInfo textInfo = ((TMP_Text)_textComponent).textInfo;
			if (((textInfo != null) ? textInfo.linkInfo : null) != null && num < ((TMP_Text)_textComponent).textInfo.linkCount)
			{
				TMP_LinkInfo val = ((Il2CppArrayBase<TMP_LinkInfo>)(object)((TMP_Text)_textComponent).textInfo.linkInfo)[num];
				ShowTooltip(val.GetLinkID());
				return;
			}
		}
		HideTooltip();
	}

	private void ShowTooltip(string text)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(text))
		{
			if ((Object)(object)_tooltipObject == (Object)null)
			{
				CreateTooltipObject();
			}
			if (!((Object)(object)_tooltipObject == (Object)null))
			{
				_tooltipObject.SetActive(true);
				((TMP_Text)_tooltipText).text = text;
				((Transform)_tooltipRect).position = Input.mousePosition + TOOLTIP_OFFSET;
			}
		}
	}

	private void HideTooltip()
	{
		if ((Object)(object)_tooltipObject != (Object)null && _tooltipObject.activeSelf)
		{
			_tooltipObject.SetActive(false);
		}
	}

	private void CreateTooltipObject()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected O, but got Unknown
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		Canvas val = null;
		int num = int.MinValue;
		foreach (Canvas item in Object.FindObjectsOfType<Canvas>())
		{
			if (item.sortingOrder > num)
			{
				num = item.sortingOrder;
				val = item;
			}
		}
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogError(Object.op_Implicit("[ModTooltip] No Canvas found."));
			return;
		}
		_tooltipObject = new GameObject("ModTooltip");
		_tooltipObject.transform.SetParent(((Component)val).transform, false);
		_tooltipRect = _tooltipObject.AddComponent<RectTransform>();
		_tooltipRect.pivot = new Vector2(0f, 1f);
		_tooltipObject.AddComponent<LayoutElement>().minWidth = 100f;
		ContentSizeFitter obj = _tooltipObject.AddComponent<ContentSizeFitter>();
		obj.horizontalFit = (FitMode)2;
		obj.verticalFit = (FitMode)2;
		((Graphic)_tooltipObject.AddComponent<Image>()).color = new Color(0.06f, 0.07f, 0.09f, 0.96f);
		Outline obj2 = _tooltipObject.AddComponent<Outline>();
		((Shadow)obj2).effectColor = new Color(1f, 0.13f, 0.25f, 0.5f);
		((Shadow)obj2).effectDistance = new Vector2(1f, -1f);
		GameObject val2 = new GameObject("TooltipText");
		val2.transform.SetParent((Transform)(object)_tooltipRect, false);
		_tooltipText = val2.AddComponent<TextMeshProUGUI>();
		((TMP_Text)_tooltipText).fontSize = 15f;
		((Graphic)_tooltipText).color = new Color(0.86f, 0.93f, 0.89f, 1f);
		((TMP_Text)_tooltipText).alignment = (TextAlignmentOptions)513;
		((TMP_Text)_tooltipText).margin = new Vector4(10f, 6f, 10f, 6f);
		RectTransform component = ((Component)_tooltipText).GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.sizeDelta = Vector2.zero;
		_tooltipObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if ((Object)(object)_tooltipObject != (Object)null)
		{
			Object.Destroy((Object)(object)_tooltipObject);
			_tooltipObject = null;
		}
		_textComponent = null;
		_mainCamera = null;
	}
}
