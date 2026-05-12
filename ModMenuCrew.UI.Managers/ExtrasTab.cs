using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using ModMenuCrew.Features;
using ModMenuCrew.UI.Menus;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew.UI.Managers;

public class ExtrasTab
{
	private ServerData.UISnapshot _safeSnapshot;

	private byte[] _snapshotBytecode;

	private byte[] _snapshotInverseMap;

	private long _snapshotToken;

	private static string _customKickNameBuffer = "";

	private static string _editingFieldId = null;

	private static readonly char[] _commaSeparator = new char[1] { ',' };

	private GUIStyle _textInputStyle;

	private GUIStyle _textInputFocusedStyle;

	private GUIStyle _akBtnStyle;

	private GUIStyle _akLabelStyle;

	private static readonly GUILayoutOption[] _emptyLayoutOptions = Array.Empty<GUILayoutOption>();

	private string _lastCursorDisplay;

	private string _lastCursorValue;

	private bool _lastCursorState;

	public static void RegisterActions(Dictionary<string, Action<long>> registry)
	{
		BanMenu.RegisterActions(registry);
	}

	internal static void ClearCustomNameBuffer()
	{
		_customKickNameBuffer = "";
		_editingFieldId = null;
	}

	public void Draw()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)Event.current.type == 8)
		{
			PlayerPickMenu.CheckRealtimeUpdate();
			_safeSnapshot = ServerData.CurrentSnapshot;
			byte[] array = _safeSnapshot?.ExtrasBytecode;
			if (array != null && array.Length >= 536 && array != _snapshotBytecode)
			{
				_snapshotBytecode = array;
				_snapshotInverseMap = new byte[256];
				Array.Copy(array, 268, _snapshotInverseMap, 0, 256);
				_snapshotToken = BitConverter.ToInt64(array, 260);
			}
		}
		if (_snapshotBytecode != null && _snapshotBytecode.Length != 0)
		{
			GhostUI.Execute(_snapshotBytecode, _snapshotToken, HostControlsTab.GetOrBuildActionRegistry(), _snapshotInverseMap);
			DrawCustomNameInput();
		}
		else
		{
			GUILayout.Label("<color=#949EAD>Loading extras...</color>", GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedHeight(50f) });
		}
	}

	private void EnsureStyles()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		if (_textInputStyle == null)
		{
			_textInputStyle = new GUIStyle(GuiStyles.TextFieldStyle);
			_textInputFocusedStyle = new GUIStyle(GuiStyles.TextFieldStyle);
			GUIStyle textFieldStyle = GuiStyles.TextFieldStyle;
			if ((Object)(object)textFieldStyle.focused.background != (Object)null)
			{
				_textInputFocusedStyle.normal.background = textFieldStyle.focused.background;
			}
			_textInputFocusedStyle.normal.textColor = textFieldStyle.focused.textColor;
			_akBtnStyle = new GUIStyle(GuiStyles.ButtonStyle)
			{
				fontSize = Mathf.RoundToInt(GuiStyles.Spacing.MenuSize(13f, 9f)),
				padding = GuiStyles.CreateRectOffset(6, 6, 3, 3)
			};
			_akLabelStyle = new GUIStyle(GuiStyles.LabelStyle)
			{
				fontSize = Mathf.RoundToInt(GuiStyles.Spacing.MenuSize(12f, 8f)),
				richText = true
			};
		}
	}

	private void DrawCustomNameInput()
	{
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		AmongUsClient instance = AmongUsClient.Instance;
		if (instance == null || !((InnerNetClient)instance).AmHost)
		{
			return;
		}
		EnsureStyles();
		LayoutBucket currentBucket = GhostUI.CurrentBucket;
		GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
		GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label("<color=#949EAD>Add Custom Name (Kick By Name)</color>", _akLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
		float h = GuiStyles.Spacing.MenuSize((currentBucket <= LayoutBucket.Tight) ? 28f : 32f, 20f);
		if (currentBucket <= LayoutBucket.Tight)
		{
			DrawTextInput("ak_custom", ref _customKickNameBuffer, 30);
			GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			Color color = GUI.color;
			GUI.color = new Color(0.3f, 0.85f, 0.5f);
			if (GUILayout.Button("+ Add", _akBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedHeight(h) }))
			{
				AddCustomName();
			}
			GUI.color = color;
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		else
		{
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			DrawTextInput("ak_custom", ref _customKickNameBuffer, 30);
			GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
			Color color2 = GUI.color;
			GUI.color = new Color(0.3f, 0.85f, 0.5f);
			if (GUILayout.Button("+ Add", _akBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(60f, 40f)),
				GuiStyles.CachedHeight(h)
			}))
			{
				AddCustomName();
			}
			GUI.color = color2;
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}

	private void AddCustomName()
	{
		string name = _customKickNameBuffer?.Trim();
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		try
		{
			List<string> list = new List<string>((CheatConfig.AutoKickNameList.Value ?? "").Split(_commaSeparator, StringSplitOptions.RemoveEmptyEntries));
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = list[i].Trim();
			}
			if (!list.Exists((string n) => string.Equals(n, name, StringComparison.OrdinalIgnoreCase)))
			{
				list.Add(name);
				CheatConfig.AutoKickNameList.Value = string.Join(",", list);
				CheatConfig.Save();
			}
			_customKickNameBuffer = "";
			PlayerPickMenu.TriggerRealtimeUpdate(force: true);
		}
		catch
		{
		}
	}

	private bool DrawTextInput(string fieldId, ref string value, int maxLength, params GUILayoutOption[] options)
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Invalid comparison between Unknown and I4
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Invalid comparison between Unknown and I4
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Invalid comparison between Unknown and I4
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Invalid comparison between Unknown and I4
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Invalid comparison between Unknown and I4
		if (value == null)
		{
			value = "";
		}
		if (options == null || options.Length == 0)
		{
			options = _emptyLayoutOptions;
		}
		bool result = false;
		bool flag = _editingFieldId == fieldId;
		Event current = Event.current;
		string text;
		if (flag)
		{
			bool flag2 = (int)(Time.realtimeSinceStartup * 2.5f) % 2 == 0;
			if ((object)value != _lastCursorValue || flag2 != _lastCursorState)
			{
				_lastCursorValue = value;
				_lastCursorState = flag2;
				_lastCursorDisplay = value + (flag2 ? "│" : " ");
			}
			text = _lastCursorDisplay;
		}
		else
		{
			text = (string.IsNullOrEmpty(value) ? "<color=#4A4E54>Type a name...</color>" : value);
		}
		GUIStyle val = (flag ? _textInputFocusedStyle : _textInputStyle);
		bool richText = val.richText;
		val.richText = !flag;
		GUILayout.Label(text, val, options);
		val.richText = richText;
		Rect lastRect = GUILayoutUtility.GetLastRect();
		if ((int)current.type == 0)
		{
			if ((lastRect).Contains(current.mousePosition))
			{
				_editingFieldId = fieldId;
				current.Use();
			}
			else if (flag)
			{
				_editingFieldId = null;
			}
		}
		if (flag && (int)current.type == 4)
		{
			if ((int)current.keyCode == 8)
			{
				if (value.Length > 0)
				{
					value = value.Substring(0, value.Length - 1);
					result = true;
				}
				current.Use();
			}
			else if ((int)current.keyCode == 13 || (int)current.keyCode == 271)
			{
				AddCustomName();
				_editingFieldId = null;
				current.Use();
			}
			else if ((int)current.keyCode == 27)
			{
				_editingFieldId = null;
				current.Use();
			}
			else if (current.character != 0 && !char.IsControl(current.character) && value.Length < maxLength)
			{
				value += current.character;
				result = true;
				current.Use();
			}
		}
		return result;
	}
}
