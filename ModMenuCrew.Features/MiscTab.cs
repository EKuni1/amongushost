using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew.Features;

public class MiscTab
{
	private static readonly string[] PaletteHexColors = new string[30]
	{
		"C61111", "132ED2", "117F2D", "ED54BA", "EF7D0E", "F5F557", "3F474E", "D6E0F0", "6B2FBB", "71491E",
		"38FEDC", "50EF39", "921C31", "B8B9CE", "E27D6D", "F6C7B4", "CDA622", "D7988A", "FF0000", "00FF00",
		"0000FF", "FF00FF", "00FFFF", "FFFF00", "FF6B6B", "6BCB77", "4D96FF", "FFD93D", "C84B31", "A149FA"
	};

	private static readonly string[] AmongUsColorNames = new string[18]
	{
		"Red", "Blue", "Green", "Pink", "Orange", "Yellow", "Black", "White", "Purple", "Brown",
		"Cyan", "Lime", "Maroon", "Rose", "Banana", "Gray", "Tan", "Coral"
	};

	private static readonly string[] ColorModeLabels = new string[5] { "OFF", "SOLID", "GRADIENT", "RAINBOW", "PULSE" };

	private static readonly string[] ColorModeIcons = new string[5] { "OFF", "SOLID", "GRAD", "RAIN", "PULSE" };

	private static readonly string[] ColorModeButtonLabels = new string[5] { "OFF", "SOLID", "GRADIENT", "RAINBOW", "PULSE" };

	private static readonly string[] SlotIndexLabels = new string[10] { "<color=#4A4E54><b>1</b></color>", "<color=#4A4E54><b>2</b></color>", "<color=#4A4E54><b>3</b></color>", "<color=#4A4E54><b>4</b></color>", "<color=#4A4E54><b>5</b></color>", "<color=#4A4E54><b>6</b></color>", "<color=#4A4E54><b>7</b></color>", "<color=#4A4E54><b>8</b></color>", "<color=#4A4E54><b>9</b></color>", "<color=#4A4E54><b>10</b></color>" };

	private static readonly Color[] PaletteColors = InitPaletteColors();

	private static readonly Color ColGrayBtn = new Color(0.7f, 0.7f, 0.75f);

	private static readonly Color ColGreen = new Color(0.3f, 0.85f, 0.5f);

	private static readonly Color ColRed = new Color(0.7f, 0.3f, 0.3f);

	private static readonly Color ColBlue = new Color(0.5f, 0.6f, 0.8f);

	private static readonly Color ColClipBlue = new Color(0.5f, 0.7f, 1f);

	private static readonly Color ColClipGreen = new Color(0.5f, 0.9f, 0.7f);

	private static readonly Color ColClipOrange = new Color(0.9f, 0.7f, 0.5f);

	private Action<string> _solidSwatchAction;

	private Action<string> _gradEndSwatchAction;

	private Action<string> _pulseSwatchAction;

	private Action<string> _nameColSwatchAction;

	private Action<string> _chatColorSwatchAction;

	private static readonly string RainbowLabel = "  <color=#FF0000>R</color><color=#FF8800>A</color><color=#FFFF00>I</color><color=#00FF00>N</color><color=#0088FF>B</color><color=#8800FF>O</color><color=#FF00FF>W</color>  <color=#6B7280>-  Animated, cycles automatically every frame</color>";

	private string _customNameBuffer;

	private string _editingFieldId;

	private GUIStyle _textInputFocusedStyle;

	private string _hexInput = "FFFFFF";

	private string _gradStartInput = "FF0000";

	private string _gradEndInput = "0000FF";

	private string _pulse1Input = "FF0000";

	private string _pulse2Input = "00FF00";

	private string _nameColorInput = "FFFFFF";

	private string _newOutfitName = "";

	private string _chatColorHexInput = "FFD700";

	private bool _initialized;

	private static readonly StringBuilder _clipboardSb = new StringBuilder(512);

	private static readonly StringBuilder _hexFilterSb = new StringBuilder(8);

	private float _pickerHue;

	private float _pickerSat = 1f;

	private float _pickerVal = 1f;

	private bool _draggingSV;

	private bool _draggingHue;

	private Texture2D _svTexture;

	private Texture2D _hueBarTexture;

	private Texture2D _checkerDot;

	private float _lastPickerHue = -1f;

	private static int _svSize = 120;

	private static LayoutBucket _lastSVBucket = LayoutBucket.Standard;

	private static float _lastSVMenuScale = 1f;

	private static bool _svTexNeedsRebuild = false;

	private const int HUE_W = 200;

	private const int HUE_H = 16;

	private string _pickerTargetField;

	private GUIStyle _previewStyle;

	private GUIStyle _foldoutStyle;

	private GUIStyle _foldoutOpenStyle;

	private GUIStyle _toolbarBtnStyle;

	private GUIStyle _toolbarBtnActiveStyle;

	private GUIStyle _swatchStyle;

	private GUIStyle _swatchSelectedStyle;

	private GUIStyle _tagLabelStyle;

	private GUIStyle _presetBtnStyle;

	private GUIStyle _cardInfoStyle;

	private GUIStyle _modeBtnStyle;

	private GUIStyle _modeBtnActiveStyle;

	private GUIStyle _pickerBoxStyle;

	private GUIStyle _pickerInfoStyle;

	private GUIStyle _texDrawStyle;

	private readonly List<Texture2D> _styleTextures = new List<Texture2D>(16);

	private readonly HashSet<int> _takenColorsCache = new HashSet<int>();

	private float _takenColorsCacheTime = -1f;

	private int _takenColorsLastVersion = -1;

	private const float TAKEN_COLORS_REFRESH_SEC = 0.5f;

	private static readonly StringBuilder _stripSb = new StringBuilder(64);

	private LayoutBucket _lastStyleBucket = LayoutBucket.Standard;

	private float _lastStyleMenuScale = 1f;

	internal static bool ThemeInvalidated;

	private static string _cachedAccentHex;

	private static int _cachedAccentHexFrame = -1;

	private static Color _cachedAccentHexColor;

	private GUIContent _livePreviewContent = new GUIContent();

	private int _previewMeasureFrame = -1;

	private float _previewMeasuredH;

	private int _previewMeasuredFont;

	private Texture2D _lastDrawTexBg;

	private Color32[] _svPixelBuffer32;

	private static Color[] InitPaletteColors()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Color[] array = (Color[])(object)new Color[PaletteHexColors.Length];
		Color val = default(Color);
		for (int i = 0; i < PaletteHexColors.Length; i++)
		{
			if (ColorUtility.TryParseHtmlString("#" + PaletteHexColors[i], ref val))
			{
				array[i] = val;
			}
			else
			{
				array[i] = Color.magenta;
			}
		}
		return array;
	}

	private static int GetSVSize()
	{
		LayoutBucket currentBucket = GhostUI.CurrentBucket;
		float menuScale = GuiStyles.Spacing.MenuScale;
		if (currentBucket != _lastSVBucket || Mathf.Abs(menuScale - _lastSVMenuScale) >= 0.03f)
		{
			_lastSVBucket = currentBucket;
			_lastSVMenuScale = menuScale;
			int num = Mathf.RoundToInt(GuiStyles.Spacing.MenuSize(currentBucket switch
			{
				LayoutBucket.Micro => 60f, 
				LayoutBucket.Tight => 80f, 
				_ => 120f, 
			}, 48f));
			if (num != _svSize)
			{
				_svSize = num;
				_svTexNeedsRebuild = true;
			}
		}
		return _svSize;
	}

	private void DisposeStyleTextures()
	{
		for (int i = 0; i < _styleTextures.Count; i++)
		{
			Texture2D val = _styleTextures[i];
			if ((Object)(object)val != (Object)null)
			{
				Object.Destroy((Object)(object)val);
			}
		}
		_styleTextures.Clear();
	}

	private Texture2D TrackTex(Texture2D t)
	{
		if ((Object)(object)t != (Object)null)
		{
			_styleTextures.Add(t);
		}
		return t;
	}

	public void Draw()
	{
		EnsureInit();
		LazyInitStyles();
		try
		{
			DrawLocalOnlyBanner();
		}
		catch (Exception ex)
		{
			GUILayout.Label("<color=#FF4444>[Banner Error: " + ex.Message + "]</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		bool flag = GhostUI.CurrentBucket <= LayoutBucket.Micro;
		float num = GuiStyles.Spacing.MenuSize(flag ? 4f : 8f, flag ? 2f : 4f);
		GUILayout.Space(num);
		if (DrawFoldoutHeader(flag ? "NAME" : "NAME EDITOR", MiscConfig.FoldoutNameEditor))
		{
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			try
			{
				DrawNameEditorSection();
			}
			catch (Exception ex2)
			{
				Debug.LogError(Object.op_Implicit($"[MiscTab] NameEditor error: {ex2}"));
				GUILayout.Label("<color=#FF4444>[Name Editor Error: " + ex2.Message + "]</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			GUILayout.EndVertical();
			GUILayout.Space(num);
		}
		if (DrawFoldoutHeader(flag ? "OUTFIT" : "OUTFIT MANAGER", MiscConfig.FoldoutOutfitManager))
		{
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			try
			{
				DrawOutfitManagerSection();
			}
			catch (Exception ex3)
			{
				Debug.LogError(Object.op_Implicit($"[MiscTab] OutfitManager error: {ex3}"));
				GUILayout.Label("<color=#FF4444>[Outfit Error: " + ex3.Message + "]</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			GUILayout.EndVertical();
			GUILayout.Space(num);
		}
		if (DrawFoldoutHeader(flag ? "CHAT" : "CHAT COLOR", MiscConfig.FoldoutChatColor))
		{
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			try
			{
				DrawChatColorSection();
			}
			catch (Exception ex4)
			{
				Debug.LogError(Object.op_Implicit($"[MiscTab] ChatColor error: {ex4}"));
				GUILayout.Label("<color=#FF4444>[Chat Color Error: " + ex4.Message + "]</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			GUILayout.EndVertical();
			GUILayout.Space(num);
		}
		if (DrawFoldoutHeader(flag ? "TOOLS" : "UTILITIES", MiscConfig.FoldoutUtilities))
		{
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			try
			{
				DrawUtilitiesSection();
			}
			catch (Exception ex5)
			{
				Debug.LogError(Object.op_Implicit($"[MiscTab] Utilities error: {ex5}"));
				GUILayout.Label("<color=#FF4444>[Utilities Error: " + ex5.Message + "]</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			GUILayout.EndVertical();
			GUILayout.Space(num);
		}
	}

	public void Cleanup()
	{
		if ((Object)(object)_svTexture != (Object)null)
		{
			Object.Destroy((Object)(object)_svTexture);
			_svTexture = null;
		}
		if ((Object)(object)_hueBarTexture != (Object)null)
		{
			Object.Destroy((Object)(object)_hueBarTexture);
			_hueBarTexture = null;
		}
		if ((Object)(object)_checkerDot != (Object)null)
		{
			Object.Destroy((Object)(object)_checkerDot);
			_checkerDot = null;
		}
		_svPixelBuffer32 = null;
		_lastPickerHue = -1f;
		DisposeStyleTextures();
		_previewStyle = null;
	}

	private void EnsureInit()
	{
		if (_initialized)
		{
			return;
		}
		_customNameBuffer = MiscConfig.CustomNameText?.Value ?? "";
		_hexInput = MiscConfig.NameColorHex?.Value ?? "FFFFFF";
		_gradStartInput = MiscConfig.NameGradientStartHex?.Value ?? "FF0000";
		_gradEndInput = MiscConfig.NameGradientEndHex?.Value ?? "0000FF";
		_pulse1Input = MiscConfig.NamePulseColor1Hex?.Value ?? "FF0000";
		_pulse2Input = MiscConfig.NamePulseColor2Hex?.Value ?? "00FF00";
		_solidSwatchAction = delegate(string hex)
		{
			_hexInput = hex;
			if (MiscConfig.NameColorHex != null)
			{
				MiscConfig.NameColorHex.Value = hex;
			}
			SetPickerFromHex(hex);
			MiscNameService.InvalidateCache();
		};
		_gradEndSwatchAction = delegate(string hex)
		{
			_gradEndInput = hex;
			if (MiscConfig.NameGradientEndHex != null)
			{
				MiscConfig.NameGradientEndHex.Value = hex;
			}
			MiscNameService.InvalidateCache();
		};
		_pulseSwatchAction = delegate(string hex)
		{
			_pulse2Input = hex;
			if (MiscConfig.NamePulseColor2Hex != null)
			{
				MiscConfig.NamePulseColor2Hex.Value = hex;
			}
			MiscNameService.InvalidateCache();
		};
		_nameColSwatchAction = delegate(string hex)
		{
			_nameColorInput = hex;
			if (MiscConfig.NameColorOverrideHex != null)
			{
				MiscConfig.NameColorOverrideHex.Value = hex;
			}
			SetPickerFromHex(hex);
			MiscNameService.InvalidateCache();
		};
		_nameColorInput = MiscConfig.NameColorOverrideHex?.Value ?? "FFFFFF";
		_chatColorHexInput = MiscConfig.ChatColorHex?.Value ?? "FFD700";
		_chatColorSwatchAction = delegate(string hex)
		{
			_chatColorHexInput = hex;
			if (MiscConfig.ChatColorHex != null)
			{
				MiscConfig.ChatColorHex.Value = hex;
			}
		};
		_initialized = true;
	}

	private void LazyInitStyles()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected O, but got Unknown
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Expected O, but got Unknown
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Expected O, but got Unknown
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Expected O, but got Unknown
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0529: Unknown result type (might be due to invalid IL or missing references)
		//IL_0536: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Expected O, but got Unknown
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_0591: Expected O, but got Unknown
		//IL_059c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05be: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0613: Unknown result type (might be due to invalid IL or missing references)
		//IL_0622: Expected O, but got Unknown
		//IL_0650: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_0662: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Unknown result type (might be due to invalid IL or missing references)
		//IL_068d: Expected O, but got Unknown
		//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fd: Expected O, but got Unknown
		//IL_0710: Unknown result type (might be due to invalid IL or missing references)
		//IL_0715: Unknown result type (might be due to invalid IL or missing references)
		//IL_0720: Unknown result type (might be due to invalid IL or missing references)
		//IL_073e: Expected O, but got Unknown
		//IL_0745: Unknown result type (might be due to invalid IL or missing references)
		//IL_074a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0755: Unknown result type (might be due to invalid IL or missing references)
		//IL_0773: Expected O, but got Unknown
		//IL_077e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0783: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d0: Expected O, but got Unknown
		//IL_07f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0811: Unknown result type (might be due to invalid IL or missing references)
		//IL_0816: Unknown result type (might be due to invalid IL or missing references)
		//IL_0823: Unknown result type (might be due to invalid IL or missing references)
		//IL_082a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0845: Unknown result type (might be due to invalid IL or missing references)
		//IL_0856: Unknown result type (might be due to invalid IL or missing references)
		//IL_086b: Unknown result type (might be due to invalid IL or missing references)
		//IL_087a: Expected O, but got Unknown
		LayoutBucket currentBucket = GhostUI.CurrentBucket;
		float menuScale = GuiStyles.Spacing.MenuScale;
		bool flag = currentBucket != _lastStyleBucket;
		bool flag2 = Mathf.Abs(menuScale - _lastStyleMenuScale) >= 0.08f;
		if (ThemeInvalidated)
		{
			_previewStyle = null;
			ThemeInvalidated = false;
		}
		if (_previewStyle == null || flag || flag2)
		{
			DisposeStyleTextures();
			_previewStyle = null;
			_lastStyleBucket = currentBucket;
			_lastStyleMenuScale = menuScale;
			int num = GuiStyles.Spacing.MenuPad(8);
			int num2 = GuiStyles.Spacing.MenuPad(4);
			GUIStyle val = new GUIStyle(GUI.skin.box)
			{
				fontSize = GuiStyles.Spacing.MenuFont(13),
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4,
				padding = GuiStyles.CreateRectOffset(num, num, num2, num2),
				margin = GuiStyles.CreateRectOffset(GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(2), GuiStyles.Spacing.MenuPad(4))
			};
			val.normal.textColor = Color.white;
			_previewStyle = val;
			_previewStyle.richText = true;
			_previewStyle.wordWrap = true;
			_previewStyle.normal.background = TrackTex(GuiStyles.MakeTexture(2, 2, new Color(0.04f, 0.04f, 0.06f, 0.95f)));
			int num3 = GuiStyles.Spacing.MenuPad(14);
			int num4 = GuiStyles.Spacing.MenuPad(8);
			_foldoutStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = GuiStyles.Spacing.MenuFont(14),
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)3,
				padding = GuiStyles.CreateRectOffset(num3, num3, num4, num4),
				margin = GuiStyles.CreateRectOffset(0, 0, 0, 0),
				fixedHeight = GuiStyles.Spacing.MenuSize(currentBucket switch
				{
					LayoutBucket.Tight => 32f, 
					LayoutBucket.Micro => 28f, 
					_ => 36f, 
				}, 22f)
			};
			_foldoutStyle.normal.textColor = GuiStyles.Theme.TextMuted;
			_foldoutStyle.hover.textColor = GuiStyles.Theme.TextPrimary;
			_foldoutStyle.normal.background = TrackTex(GuiStyles.MakeTexture(2, 2, new Color(0.08f, 0.08f, 0.1f, 0.95f)));
			_foldoutStyle.hover.background = TrackTex(GuiStyles.MakeTexture(2, 2, new Color(0.12f, 0.05f, 0.06f, 0.95f)));
			_foldoutStyle.active.background = TrackTex(GuiStyles.MakeTexture(2, 2, new Color(0.15f, 0.04f, 0.06f, 0.95f)));
			_foldoutStyle.richText = true;
			_foldoutOpenStyle = new GUIStyle(_foldoutStyle);
			_foldoutOpenStyle.normal.textColor = GuiStyles.Theme.Accent;
			_foldoutOpenStyle.normal.background = TrackTex(GuiStyles.MakeTexture(2, 2, new Color(0.12f, 0.04f, 0.06f, 0.95f)));
			int num5 = GuiStyles.Spacing.MenuPad(2);
			_toolbarBtnStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = GuiStyles.Spacing.MenuFont(12),
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4,
				padding = GuiStyles.CreateRectOffset(0, 0, 0, 0),
				margin = GuiStyles.CreateRectOffset(num5, num5, num5, num5),
				fixedWidth = GuiStyles.Spacing.MenuSize(currentBucket switch
				{
					LayoutBucket.Tight => 26f, 
					LayoutBucket.Micro => 22f, 
					_ => 32f, 
				}, 18f),
				fixedHeight = GuiStyles.Spacing.MenuSize(currentBucket switch
				{
					LayoutBucket.Tight => 22f, 
					LayoutBucket.Micro => 20f, 
					_ => 26f, 
				}, 16f)
			};
			_toolbarBtnStyle.normal.textColor = GuiStyles.Theme.TextMuted;
			_toolbarBtnStyle.normal.background = TrackTex(GuiStyles.MakeTexture(2, 2, new Color(0.1f, 0.1f, 0.12f, 0.95f)));
			_toolbarBtnStyle.hover.background = TrackTex(GuiStyles.MakeTexture(2, 2, new Color(0.18f, 0.08f, 0.08f, 0.95f)));
			_toolbarBtnStyle.richText = true;
			_toolbarBtnActiveStyle = new GUIStyle(_toolbarBtnStyle);
			_toolbarBtnActiveStyle.normal.textColor = Color.white;
			_toolbarBtnActiveStyle.normal.background = TrackTex(GuiStyles.MakeTexture(2, 2, new Color(0.55f, 0.08f, 0.12f, 0.95f)));
			int num6 = GuiStyles.Spacing.MenuPad(1);
			_swatchStyle = new GUIStyle(GUI.skin.box)
			{
				padding = GuiStyles.CreateRectOffset(0, 0, 0, 0),
				margin = GuiStyles.CreateRectOffset(num6, num6, num6, num6),
				fixedWidth = GuiStyles.Spacing.MenuSize(currentBucket switch
				{
					LayoutBucket.Tight => 20f, 
					LayoutBucket.Micro => 16f, 
					_ => 24f, 
				}),
				fixedHeight = GuiStyles.Spacing.MenuSize(currentBucket switch
				{
					LayoutBucket.Tight => 20f, 
					LayoutBucket.Micro => 16f, 
					_ => 24f, 
				}),
				fontSize = GuiStyles.Spacing.MenuFont(9),
				alignment = (TextAnchor)4,
				fontStyle = (FontStyle)1
			};
			_swatchStyle.normal.textColor = Color.clear;
			_swatchStyle.normal.background = TrackTex(GuiStyles.MakeTexture(2, 2, Color.white));
			_swatchSelectedStyle = new GUIStyle(_swatchStyle);
			_swatchSelectedStyle.normal.textColor = Color.white;
			int num7 = GuiStyles.Spacing.MenuPad(4);
			GUIStyle val2 = new GUIStyle(GUI.skin.label)
			{
				fontSize = GuiStyles.Spacing.MenuFont(10),
				alignment = (TextAnchor)3,
				padding = GuiStyles.CreateRectOffset(num7, num7, 0, 0),
				margin = GuiStyles.CreateRectOffset(num7, num7, 0, GuiStyles.Spacing.MenuPad(2))
			};
			val2.normal.textColor = new Color(0.45f, 0.48f, 0.52f, 1f);
			_tagLabelStyle = val2;
			_tagLabelStyle.richText = true;
			_tagLabelStyle.wordWrap = true;
			int num8 = GuiStyles.Spacing.MenuPad(8);
			int num9 = GuiStyles.Spacing.MenuPad(4);
			_presetBtnStyle = new GUIStyle(GuiStyles.ButtonStyle)
			{
				fontSize = GuiStyles.Spacing.MenuFont(11),
				padding = GuiStyles.CreateRectOffset(num8, num8, num9, num9),
				margin = GuiStyles.CreateRectOffset(num5, num5, num5, num5)
			};
			int num10 = GuiStyles.Spacing.MenuPad(6);
			int num11 = GuiStyles.Spacing.MenuPad(2);
			GUIStyle val3 = new GUIStyle(GUI.skin.label)
			{
				fontSize = GuiStyles.Spacing.MenuFont(12),
				padding = GuiStyles.CreateRectOffset(num10, num10, num11, num11),
				margin = GuiStyles.CreateRectOffset(num7, num7, GuiStyles.Spacing.MenuPad(1), GuiStyles.Spacing.MenuPad(1))
			};
			val3.normal.textColor = GuiStyles.Theme.TextPrimary;
			_cardInfoStyle = val3;
			_cardInfoStyle.richText = true;
			_modeBtnStyle = new GUIStyle(_toolbarBtnStyle)
			{
				fixedWidth = 0f,
				padding = GuiStyles.CreateRectOffset(GuiStyles.Spacing.MenuPad(6), GuiStyles.Spacing.MenuPad(6), 0, 0)
			};
			_modeBtnActiveStyle = new GUIStyle(_toolbarBtnActiveStyle)
			{
				fixedWidth = 0f,
				padding = GuiStyles.CreateRectOffset(GuiStyles.Spacing.MenuPad(6), GuiStyles.Spacing.MenuPad(6), 0, 0)
			};
			_pickerBoxStyle = new GUIStyle(GUI.skin.box)
			{
				padding = GuiStyles.CreateRectOffset(GuiStyles.Spacing.MenuPad(10), GuiStyles.Spacing.MenuPad(10), GuiStyles.Spacing.MenuPad(8), GuiStyles.Spacing.MenuPad(8)),
				margin = GuiStyles.CreateRectOffset(GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4))
			};
			_pickerBoxStyle.normal.background = TrackTex(GuiStyles.MakeTexture(2, 2, new Color(0.03f, 0.03f, 0.05f, 0.97f)));
			GUIStyle val4 = new GUIStyle(GUI.skin.label)
			{
				fontSize = GuiStyles.Spacing.MenuFont(11),
				alignment = (TextAnchor)3,
				padding = GuiStyles.CreateRectOffset(num7, num7, GuiStyles.Spacing.MenuPad(1), GuiStyles.Spacing.MenuPad(1)),
				margin = GuiStyles.CreateRectOffset(num5, num5, 0, 0)
			};
			val4.normal.textColor = new Color(0.7f, 0.72f, 0.75f);
			_pickerInfoStyle = val4;
			_pickerInfoStyle.richText = true;
		}
	}

	private void DrawLocalOnlyBanner()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginVertical(GuiStyles.DashboardCardStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		float num = 0.6f + 0.4f * Mathf.Sin(Time.time * 3f);
		GUI.color = new Color(1f, 0.7f, 0f, num);
		GUILayout.Label("<b>[!]</b>", GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(24f, 16f)) });
		GUI.color = Color.white;
		if (GhostUI.CurrentBucket <= LayoutBucket.Micro)
		{
			GUILayout.Label("<color=#FFB300><b>LOCAL ONLY</b></color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		else if (GhostUI.CurrentBucket <= LayoutBucket.Tight)
		{
			GUILayout.Label("<color=#FFB300><b>LOCAL ONLY</b></color>  <color=#6B7280>-</color>  <color=#949EAD>Only visible to you</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		else
		{
			GUILayout.Label("<color=#FFB300><b>LOCAL ONLY</b></color>  <color=#6B7280>-</color>  <color=#949EAD>Changes on this page are <b>only visible to you</b>. Other players see your real name and outfit.</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	private static string GetAccentHex()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		int frameCount = Time.frameCount;
		Color accent = GuiStyles.Theme.Accent;
		if (_cachedAccentHexFrame != frameCount || accent != _cachedAccentHexColor)
		{
			_cachedAccentHex = ColorUtility.ToHtmlStringRGB(accent);
			_cachedAccentHexColor = accent;
			_cachedAccentHexFrame = frameCount;
		}
		return _cachedAccentHex;
	}

	private void SectionTitle(string title, string subtitle = null)
	{
		LayoutBucket currentBucket = GhostUI.CurrentBucket;
		string accentHex = GetAccentHex();
		if (currentBucket <= LayoutBucket.Micro || string.IsNullOrEmpty(subtitle))
		{
			GUILayout.Label($"<color=#{accentHex}><b>[*]</b></color>  <color=#E6EAEF><b>{title}</b></color>", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		else
		{
			GUILayout.Label($"<color=#{accentHex}><b>[*]</b></color>  <color=#E6EAEF><b>{title}</b></color>  <color=#4A4E54>-</color>  <color=#949EAD>{subtitle}</color>", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		if (currentBucket > LayoutBucket.Micro)
		{
			GuiStyles.DrawSeparator();
		}
	}

	private bool DrawFoldoutHeader(string title, ConfigEntry<bool> foldout)
	{
		bool flag = foldout?.Value ?? true;
		string text = (flag ? "[-]" : "[+]");
		GUIStyle val = (flag ? _foldoutOpenStyle : _foldoutStyle);
		if (GUILayout.Button((GhostUI.CurrentBucket <= LayoutBucket.Tight) ? (text + "  " + title) : ("  " + text + "    " + title), val ?? GUI.skin.button, Array.Empty<GUILayoutOption>()))
		{
			if (foldout != null)
			{
				foldout.Value = !flag;
			}
			flag = !flag;
		}
		return flag;
	}

	private void DrawNameEditorSection()
	{
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		SectionTitle((GhostUI.CurrentBucket <= LayoutBucket.Micro) ? "NAME" : "CUSTOM NAME", (GhostUI.CurrentBucket > LayoutBucket.Tight) ? "Local name with rich-text formatting" : null);
		bool flag = MiscConfig.CustomNameEnabled?.Value ?? false;
		string text = ((GhostUI.CurrentBucket > LayoutBucket.Micro) ? (flag ? "  [ON]  Custom Name Active" : "  [OFF] Custom Name Disabled") : (flag ? "[ON]" : "[OFF]"));
		bool flag2 = GUILayout.Toggle(flag, text, GuiStyles.ToggleStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		if (flag2 != flag && MiscConfig.CustomNameEnabled != null)
		{
			MiscConfig.CustomNameEnabled.Value = flag2;
			MiscNameService.InvalidateCache();
			if (!flag2)
			{
				MiscNameService.CleanupAll();
			}
		}
		GUILayout.Space(GuiStyles.Spacing.MenuSize(6f, 3f));
		if (_customNameBuffer == null)
		{
			_customNameBuffer = MiscConfig.CustomNameText?.Value ?? "";
		}
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		if (GhostUI.CurrentBucket > LayoutBucket.Micro)
		{
			GUILayout.Label("<color=#949EAD>Display Name</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		float w = GuiStyles.Spacing.MenuSize((GhostUI.CurrentBucket == LayoutBucket.Micro) ? 110f : ((GhostUI.CurrentBucket == LayoutBucket.Tight) ? 170f : ((GhostUI.CurrentBucket == LayoutBucket.Compact) ? 230f : 300f)), 90f);
		if (DrawTextInput("customName", ref _customNameBuffer, 50, GuiStyles.CachedWidth(w)) && MiscConfig.CustomNameText != null)
		{
			MiscConfig.CustomNameText.Value = _customNameBuffer;
			MiscNameService.InvalidateCache();
		}
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		GUI.color = ColGrayBtn;
		float w2 = GuiStyles.Spacing.MenuSize((GhostUI.CurrentBucket == LayoutBucket.Micro) ? 60f : ((GhostUI.CurrentBucket == LayoutBucket.Tight) ? 100f : 150f), 50f);
		if (GUILayout.Button((GhostUI.CurrentBucket <= LayoutBucket.Micro) ? "[~] Reset" : "[~]  Reset to Original", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w2) }))
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			object obj;
			if (localPlayer == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo data = localPlayer.Data;
				obj = ((data != null) ? data.PlayerName : null);
			}
			if (obj == null)
			{
				obj = "";
			}
			string value = (_customNameBuffer = (string)obj);
			if (MiscConfig.CustomNameText != null)
			{
				MiscConfig.CustomNameText.Value = value;
			}
			MiscNameService.InvalidateCache();
			NotifyUtils.Success("Name reset to original");
		}
		GUI.color = Color.white;
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(6f, 3f));
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		if (GhostUI.CurrentBucket > LayoutBucket.Micro)
		{
			GUILayout.Label("<color=#949EAD>Text Style</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
		}
		bool flag3 = GhostUI.CurrentBucket <= LayoutBucket.Micro;
		if (flag3)
		{
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			DrawFormatPill("B", MiscConfig.NameBold);
			DrawFormatPill("I", MiscConfig.NameItalic);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			DrawFormatPill("U", MiscConfig.NameUnderline);
			DrawFormatPill("S", MiscConfig.NameStrikethrough);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		else
		{
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			DrawFormatPill("B", MiscConfig.NameBold);
			DrawFormatPill("I", MiscConfig.NameItalic);
			DrawFormatPill("U", MiscConfig.NameUnderline);
			DrawFormatPill("S", MiscConfig.NameStrikethrough);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		int num = MiscConfig.NameSizePercent?.Value ?? 100;
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		if (!flag3)
		{
			GUILayout.Label("<color=#6B7280>Size</color>", _tagLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(30f, 18f)) });
		}
		try
		{
			float w3 = GuiStyles.Spacing.MenuSize(flag3 ? 40f : ((GhostUI.CurrentBucket == LayoutBucket.Tight) ? 60f : 80f), 40f);
			int num2 = Mathf.RoundToInt(GUILayout.HorizontalSlider((float)num, 50f, 200f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w3) }));
			if (num2 != num && MiscConfig.NameSizePercent != null)
			{
				MiscConfig.NameSizePercent.Value = num2;
				MiscNameService.InvalidateCache();
			}
		}
		catch
		{
			if (GUILayout.Button("-", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(24f, 18f)) }))
			{
				int value2 = Mathf.Max(50, num - 10);
				if (MiscConfig.NameSizePercent != null)
				{
					MiscConfig.NameSizePercent.Value = value2;
					MiscNameService.InvalidateCache();
				}
			}
			if (GUILayout.Button("+", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(24f, 18f)) }))
			{
				int value3 = Mathf.Min(200, num + 10);
				if (MiscConfig.NameSizePercent != null)
				{
					MiscConfig.NameSizePercent.Value = value3;
					MiscNameService.InvalidateCache();
				}
			}
		}
		GUILayout.Label($"<color=#E6EAEF><b>{num}%</b></color>", _cardInfoStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(flag3 ? 32f : 40f, 24f)) });
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(6f, 3f));
		DrawColorModeCard();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(6f, 3f));
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		if (GhostUI.CurrentBucket > LayoutBucket.Micro)
		{
			GUILayout.Label("<color=#949EAD>Live Preview</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		string previewString = MiscNameService.GetPreviewString();
		DrawLivePreview(previewString);
		GUILayout.EndVertical();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(6f, 3f));
		DrawNamePresetsCard();
	}

	private void DrawLivePreview(string preview)
	{
		if (_previewStyle == null)
		{
			GUILayout.Label(preview ?? "", (Il2CppReferenceArray<GUILayoutOption>)null);
			return;
		}
		int num = MiscConfig.NameSizePercent?.Value ?? 100;
		int num2 = Mathf.Clamp(Mathf.RoundToInt((float)(GuiStyles.Spacing.MenuFont(GhostUI.CurrentBucket switch
		{
			LayoutBucket.Micro => 11, 
			LayoutBucket.Tight => 12, 
			LayoutBucket.Compact => 13, 
			_ => 14, 
		}) * num) / 100f), 9, 40);
		float num3 = Mathf.Max(64f, GhostUI.CurrentContentWidth - GuiStyles.Spacing.MenuSize(32f, 16f));
		int frameCount = Time.frameCount;
		if (_previewMeasureFrame != frameCount)
		{
			int fontSize = _previewStyle.fontSize;
			_previewStyle.fontSize = num2;
			_livePreviewContent.text = preview ?? "";
			float num4 = GhostUI.CurrentBucket switch
			{
				LayoutBucket.Micro => 22f, 
				LayoutBucket.Tight => 26f, 
				LayoutBucket.Compact => 30f, 
				_ => 34f, 
			};
			float num5 = 140f;
			float num6;
			try
			{
				num6 = _previewStyle.CalcHeight(_livePreviewContent, num3);
			}
			catch
			{
				num6 = num4;
			}
			_previewMeasuredH = Mathf.Clamp(num6 + GuiStyles.Spacing.MenuSize(8f, 4f), GuiStyles.Spacing.MenuSize(num4, num4 * 0.7f), num5);
			_previewMeasuredFont = num2;
			_previewMeasureFrame = frameCount;
			_previewStyle.fontSize = fontSize;
		}
		int fontSize2 = _previewStyle.fontSize;
		_previewStyle.fontSize = _previewMeasuredFont;
		GUILayout.Label(preview ?? "", _previewStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedWidth(num3),
			GuiStyles.CachedHeight(_previewMeasuredH)
		});
		_previewStyle.fontSize = fontSize2;
	}

	private void DrawFormatPill(string label, ConfigEntry<bool> config)
	{
		if (config != null)
		{
			bool value = config.Value;
			if (GUILayout.Button(label, value ? _toolbarBtnActiveStyle : _toolbarBtnStyle, Array.Empty<GUILayoutOption>()))
			{
				config.Value = !value;
				MiscNameService.InvalidateCache();
			}
		}
	}

	private void DrawColorModeCard()
	{
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		if (GhostUI.CurrentBucket > LayoutBucket.Micro)
		{
			GUILayout.Label("<color=#949EAD>Color Mode</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
		}
		int num = MiscConfig.NameColorMode?.Value ?? 0;
		int num2 = ((GhostUI.CurrentBucket <= LayoutBucket.Micro) ? 2 : ((GhostUI.CurrentBucket <= LayoutBucket.Compact) ? 3 : 5));
		float num3 = GhostUI.CurrentContentWidth - GuiStyles.Spacing.MenuSize(32f, 20f);
		GUILayoutOption val = GuiStyles.CachedWidth(Mathf.Max(GuiStyles.Spacing.MenuSize(48f, 32f), num3 / (float)num2 - GuiStyles.Spacing.MenuSize(4f, 2f)));
		GUILayoutOption val2 = GuiStyles.CachedHeight(GuiStyles.Spacing.MenuSize(26f, 18f));
		for (int i = 0; i < ColorModeLabels.Length; i++)
		{
			if (i % num2 == 0)
			{
				if (i > 0)
				{
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			}
			GUIStyle val3 = ((i == num) ? _modeBtnActiveStyle : _modeBtnStyle);
			if (GUILayout.Button((GhostUI.CurrentBucket <= LayoutBucket.Tight) ? ColorModeIcons[i] : ColorModeButtonLabels[i], val3, (GUILayoutOption[])(object)new GUILayoutOption[2] { val, val2 }))
			{
				if (MiscConfig.NameColorMode != null)
				{
					MiscConfig.NameColorMode.Value = i;
				}
				MiscNameService.InvalidateCache();
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
		bool flag = GhostUI.CurrentBucket <= LayoutBucket.Compact;
		switch (num)
		{
		case 1:
			if (flag)
			{
				DrawHexInputRow("HEX", "hex_solid_c", ref _hexInput, MiscConfig.NameColorHex);
			}
			else
			{
				DrawHSVColorPicker(ref _hexInput, MiscConfig.NameColorHex, "solid");
			}
			GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
			DrawSwatchPalette(_solidSwatchAction, _hexInput);
			break;
		case 2:
			if (flag)
			{
				DrawHexInputRow("Start", "hex_grad_start_c", ref _gradStartInput, MiscConfig.NameGradientStartHex);
			}
			else
			{
				GUILayout.Label("<color=#949EAD>Start Color</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				DrawHSVColorPicker(ref _gradStartInput, MiscConfig.NameGradientStartHex, "grad_start");
			}
			GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
			DrawHexInputRow(flag ? "End" : "End Color", "hex_end", ref _gradEndInput, MiscConfig.NameGradientEndHex);
			if (!flag)
			{
				GUILayout.Label("<color=#4A4E54>Picker controls Start color  ·  Type End color above</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
			DrawSwatchPalette(_gradEndSwatchAction, _gradEndInput);
			break;
		case 3:
			if (flag)
			{
				GUILayout.Label("<color=#FF0000>R</color><color=#FF8800>A</color><color=#FFFF00>I</color><color=#00FF00>N</color><color=#0088FF>B</color><color=#8800FF>O</color><color=#FF00FF>W</color>", _cardInfoStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			else
			{
				GUILayout.Label(RainbowLabel, _cardInfoStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			break;
		case 4:
			if (flag)
			{
				DrawHexInputRow("A", "hex_pulse_a_c", ref _pulse1Input, MiscConfig.NamePulseColor1Hex);
			}
			else
			{
				GUILayout.Label("<color=#949EAD>Color A</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				DrawHSVColorPicker(ref _pulse1Input, MiscConfig.NamePulseColor1Hex, "pulse_a");
			}
			GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
			DrawHexInputRow(flag ? "B" : "Color B", "hex_colorb", ref _pulse2Input, MiscConfig.NamePulseColor2Hex);
			GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
			DrawSwatchPalette(_pulseSwatchAction, _pulse2Input);
			break;
		}
		GUILayout.EndVertical();
	}

	private void DrawTex(Rect rect, Texture2D tex)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)tex == (Object)null))
		{
			if (_texDrawStyle == null)
			{
				_texDrawStyle = new GUIStyle();
				_texDrawStyle.border = GuiStyles.CreateRectOffset(0, 0, 0, 0);
				_texDrawStyle.padding = GuiStyles.CreateRectOffset(0, 0, 0, 0);
				_texDrawStyle.margin = GuiStyles.CreateRectOffset(0, 0, 0, 0);
				_lastDrawTexBg = null;
			}
			if ((Object)(object)_lastDrawTexBg != (Object)(object)tex)
			{
				_texDrawStyle.normal.background = tex;
				_lastDrawTexBg = tex;
			}
			GUI.Box(rect, GUIContent.none, _texDrawStyle);
		}
	}

	private bool DrawTextInput(string fieldId, ref string value, int maxLength, params GUILayoutOption[] options)
	{
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Invalid comparison between Unknown and I4
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Invalid comparison between Unknown and I4
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Invalid comparison between Unknown and I4
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Invalid comparison between Unknown and I4
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Invalid comparison between Unknown and I4
		if (value == null)
		{
			value = "";
		}
		bool result = false;
		bool flag = _editingFieldId == fieldId;
		Event current = Event.current;
		string text;
		if (flag)
		{
			bool flag2 = (int)(Time.realtimeSinceStartup * 2.5f) % 2 == 0;
			text = value + (flag2 ? "│" : " ");
		}
		else
		{
			text = (string.IsNullOrEmpty(value) ? " " : value);
		}
		GUIStyle val;
		if (flag)
		{
			if (_textInputFocusedStyle == null || (Object)(object)_textInputFocusedStyle.normal.background == (Object)null)
			{
				_textInputFocusedStyle = new GUIStyle(GuiStyles.TextFieldStyle);
				GUIStyle textFieldStyle = GuiStyles.TextFieldStyle;
				if ((Object)(object)textFieldStyle.focused.background != (Object)null)
				{
					_textInputFocusedStyle.normal.background = textFieldStyle.focused.background;
				}
				_textInputFocusedStyle.normal.textColor = textFieldStyle.focused.textColor;
			}
			val = _textInputFocusedStyle;
		}
		else
		{
			val = GuiStyles.TextFieldStyle;
		}
		bool richText = val.richText;
		val.richText = false;
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
			else if ((int)current.keyCode == 27 || (int)current.keyCode == 13 || (int)current.keyCode == 271)
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

	private void DrawHexInputRow(string label, string fieldId, ref string localValue, ConfigEntry<string> config)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		float w = GuiStyles.Spacing.MenuSize((GhostUI.CurrentBucket == LayoutBucket.Micro) ? 30f : 50f, 24f);
		GUILayout.Label("<color=#6B7280>" + label + "</color>", _tagLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w) });
		float num = GuiStyles.Spacing.MenuSize(24f);
		Color color = GUI.color;
		Color color2 = default(Color);
		if (ColorUtility.TryParseHtmlString("#" + localValue, ref color2))
		{
			GUI.color = color2;
		}
		GUILayout.Box("", _swatchStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedWidth(num),
			GuiStyles.CachedHeight(num)
		});
		GUI.color = color;
		GUILayout.Label("<color=#4A4E54>#</color>", _tagLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(12f, 8f)) });
		float w2 = GuiStyles.Spacing.MenuSize((GhostUI.CurrentBucket <= LayoutBucket.Tight) ? 50f : 80f, 36f);
		if (DrawTextInput(fieldId, ref localValue, 6, GuiStyles.CachedWidth(w2)))
		{
			localValue = localValue.ToUpper();
			_hexFilterSb.Clear();
			for (int i = 0; i < localValue.Length; i++)
			{
				char c = localValue[i];
				if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'))
				{
					_hexFilterSb.Append(c);
				}
			}
			localValue = _hexFilterSb.ToString();
			if (config != null && localValue.Length == 6)
			{
				config.Value = localValue;
				MiscNameService.InvalidateCache();
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	private void DrawSwatchPalette(Action<string> onClicked, string currentHex)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
		float num = GuiStyles.Spacing.MenuSize(24f);
		GUILayoutOption val = GuiStyles.CachedWidth(num);
		GUILayoutOption val2 = GuiStyles.CachedHeight(num);
		float num2 = num + GuiStyles.Spacing.MenuSize(4f, 2f);
		int num3 = Mathf.Clamp(Mathf.FloorToInt((GhostUI.CurrentContentWidth - GuiStyles.Spacing.MenuSize(48f, 28f)) / num2), 3, 10);
		LayoutBucket currentBucket = GhostUI.CurrentBucket;
		int num4 = Mathf.Min((currentBucket <= LayoutBucket.Micro) ? num3 : ((currentBucket <= LayoutBucket.Tight) ? (num3 * 2) : PaletteHexColors.Length), PaletteHexColors.Length);
		for (int i = 0; i < num4; i += num3)
		{
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
			for (int j = 0; j < num3 && i + j < num4; j++)
			{
				int num5 = i + j;
				string text = PaletteHexColors[num5];
				bool num6 = string.Equals(text, currentHex, StringComparison.OrdinalIgnoreCase);
				Color color = GUI.color;
				GUI.color = PaletteColors[num5];
				GUIStyle val3 = (num6 ? _swatchSelectedStyle : _swatchStyle);
				if (GUILayout.Button(num6 ? "*" : "", val3, (GUILayoutOption[])(object)new GUILayoutOption[2] { val, val2 }))
				{
					onClicked?.Invoke(text);
				}
				GUI.color = color;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
	}

	private void EnsureHueBarTexture()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_hueBarTexture != (Object)null))
		{
			_hueBarTexture = new Texture2D(200, 1, (TextureFormat)3, false);
			((Texture)_hueBarTexture).wrapMode = (TextureWrapMode)1;
			((Texture)_hueBarTexture).filterMode = (FilterMode)1;
			Color[] array = (Color[])(object)new Color[200];
			for (int i = 0; i < 200; i++)
			{
				float num = (float)i / 199f;
				array[i] = Color.HSVToRGB(num, 1f, 1f);
			}
			_hueBarTexture.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
			_hueBarTexture.Apply();
		}
	}

	private void EnsureSVTexture(float hue)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		float num = (_draggingHue ? 0.04f : 0.015f);
		if ((Object)(object)_svTexture != (Object)null && Mathf.Abs(hue - _lastPickerHue) < num)
		{
			return;
		}
		_lastPickerHue = hue;
		int sVSize = GetSVSize();
		int num2 = Mathf.Max(24, sVSize / 2);
		if ((Object)(object)_svTexture != (Object)null && (((Texture)_svTexture).width != num2 || ((Texture)_svTexture).height != num2))
		{
			Object.Destroy((Object)(object)_svTexture);
			_svTexture = null;
		}
		if ((Object)(object)_svTexture == (Object)null)
		{
			_svTexture = new Texture2D(num2, num2, (TextureFormat)4, false);
			((Texture)_svTexture).wrapMode = (TextureWrapMode)1;
			((Texture)_svTexture).filterMode = (FilterMode)1;
		}
		int num3 = num2 * num2;
		if (_svPixelBuffer32 == null || _svPixelBuffer32.Length != num3)
		{
			_svPixelBuffer32 = (Color32[])(object)new Color32[num3];
		}
		float num4 = 1f / (float)(num2 - 1);
		float num5 = hue * 6f;
		int num6 = (int)num5;
		float num7 = num5 - (float)num6;
		for (int i = 0; i < num2; i++)
		{
			float num8 = (float)i * num4;
			int num9 = i * num2;
			for (int j = 0; j < num2; j++)
			{
				float num10 = (float)j * num4;
				float num11 = num8 * (1f - num10);
				float num12 = num8 * (1f - num10 * num7);
				float num13 = num8 * (1f - num10 * (1f - num7));
				float num14;
				float num15;
				float num16;
				switch (num6 % 6)
				{
				case 0:
					num14 = num8;
					num15 = num13;
					num16 = num11;
					break;
				case 1:
					num14 = num12;
					num15 = num8;
					num16 = num11;
					break;
				case 2:
					num14 = num11;
					num15 = num8;
					num16 = num13;
					break;
				case 3:
					num14 = num11;
					num15 = num12;
					num16 = num8;
					break;
				case 4:
					num14 = num13;
					num15 = num11;
					num16 = num8;
					break;
				default:
					num14 = num8;
					num15 = num11;
					num16 = num12;
					break;
				}
				_svPixelBuffer32[num9 + j] = new Color32((byte)(num14 * 255f), (byte)(num15 * 255f), (byte)(num16 * 255f), byte.MaxValue);
			}
		}
		_svTexture.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(_svPixelBuffer32));
		_svTexture.Apply(false, false);
	}

	private void EnsureCheckerDot()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_checkerDot != (Object)null)
		{
			return;
		}
		int num = 12;
		_checkerDot = new Texture2D(num, num, (TextureFormat)4, false);
		Color[] array = (Color[])(object)new Color[num * num];
		float num2 = (float)(num - 1) * 0.5f;
		float num3 = num2;
		float num4 = num2 - 1.5f;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float num5 = (float)j - num2;
				float num6 = (float)i - num2;
				float num7 = Mathf.Sqrt(num5 * num5 + num6 * num6);
				if (num7 <= num3 && num7 >= num4)
				{
					array[i * num + j] = Color.white;
				}
				else
				{
					array[i * num + j] = Color.clear;
				}
			}
		}
		_checkerDot.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
		_checkerDot.Apply();
		((Texture)_checkerDot).filterMode = (FilterMode)1;
	}

	private static string ColorToHex(Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		int value = Mathf.Clamp(Mathf.RoundToInt(c.r * 255f), 0, 255);
		int value2 = Mathf.Clamp(Mathf.RoundToInt(c.g * 255f), 0, 255);
		int value3 = Mathf.Clamp(Mathf.RoundToInt(c.b * 255f), 0, 255);
		return $"{value:X2}{value2:X2}{value3:X2}";
	}

	private void SetPickerFromHex(string hex)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Color val = default(Color);
		if (!string.IsNullOrEmpty(hex) && hex.Length >= 6 && ColorUtility.TryParseHtmlString("#" + hex, ref val))
		{
			Color.RGBToHSV(val, ref _pickerHue, ref _pickerSat, ref _pickerVal);
			_lastPickerHue = -1f;
		}
	}

	private string DrawHSVColorPicker(ref string hexField, ConfigEntry<string> config, string fieldId)
	{
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Invalid comparison between Unknown and I4
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Invalid comparison between Unknown and I4
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Invalid comparison between Unknown and I4
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Invalid comparison between Unknown and I4
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		if (GhostUI.CurrentBucket <= LayoutBucket.Tight)
		{
			DrawHexInputRow("HEX", "hex_" + fieldId, ref hexField, config);
			return hexField;
		}
		if (_svTexNeedsRebuild)
		{
			_svTexNeedsRebuild = false;
			if ((Object)(object)_svTexture != (Object)null)
			{
				Object.Destroy((Object)(object)_svTexture);
				_svTexture = null;
			}
			if ((Object)(object)_hueBarTexture != (Object)null)
			{
				Object.Destroy((Object)(object)_hueBarTexture);
				_hueBarTexture = null;
			}
			_svPixelBuffer32 = null;
			_lastPickerHue = -1f;
		}
		EnsureHueBarTexture();
		EnsureSVTexture(_pickerHue);
		EnsureCheckerDot();
		if (_pickerTargetField != fieldId)
		{
			_pickerTargetField = fieldId;
			SetPickerFromHex(hexField);
		}
		GUILayout.BeginVertical(_pickerBoxStyle, Array.Empty<GUILayoutOption>());
		float num = GuiStyles.Spacing.MenuSize(6f, 3f);
		float num2 = GuiStyles.Spacing.MenuSize(16f, 10f);
		float num3 = GuiStyles.Spacing.MenuSize(12f, 8f);
		float num4 = GuiStyles.Spacing.MenuSize(32f, 18f);
		float num5 = GuiStyles.Spacing.MenuSize(28f, 18f);
		Rect rect = GUILayoutUtility.GetRect((float)GetSVSize(), (float)GetSVSize(), (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedWidth(GetSVSize()),
			GuiStyles.CachedHeight(GetSVSize())
		});
		DrawTex(rect, _svTexture);
		Event current = Event.current;
		Vector2 mousePosition = current.mousePosition;
		if ((int)current.type == 0 && (rect).Contains(mousePosition))
		{
			_draggingSV = true;
		}
		if ((int)current.type == 1)
		{
			_draggingSV = false;
		}
		if (_draggingSV && ((int)current.type == 3 || (int)current.type == 0))
		{
			_pickerSat = Mathf.Clamp01((mousePosition.x - (rect).x) / (rect).width);
			_pickerVal = Mathf.Clamp01(1f - (mousePosition.y - (rect).y) / (rect).height);
			ApplyPickerColor(ref hexField, config);
			current.Use();
		}
		float num6 = num3 * 0.5f;
		float num7 = (rect).x + _pickerSat * (rect).width - num6;
		float num8 = (rect).y + (1f - _pickerVal) * (rect).height - num6;
		GUI.color = ((_pickerVal > 0.5f && _pickerSat < 0.5f) ? Color.black : Color.white);
		DrawTex(new Rect(num7, num8, num3, num3), _checkerDot);
		GUI.color = Color.white;
		GUILayout.Space(num);
		Rect rect2 = GUILayoutUtility.GetRect((float)GetSVSize(), num2, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedWidth(GetSVSize()),
			GuiStyles.CachedHeight(num2)
		});
		DrawTex(rect2, _hueBarTexture);
		float num9 = (rect2).x + _pickerHue * (rect2).width;
		GUI.color = Color.white;
		DrawTex(new Rect(num9 - 1f, (rect2).y - 2f, 3f, (rect2).height + 4f), Texture2D.whiteTexture);
		GUI.color = Color.white;
		if ((int)current.type == 0 && (rect2).Contains(mousePosition))
		{
			_draggingHue = true;
		}
		if ((int)current.type == 1)
		{
			_draggingHue = false;
		}
		if (_draggingHue && ((int)current.type == 3 || (int)current.type == 0))
		{
			_pickerHue = Mathf.Clamp01((mousePosition.x - (rect2).x) / (rect2).width);
			ApplyPickerColor(ref hexField, config);
			current.Use();
		}
		GUILayout.Space(num);
		Color val = Color.HSVToRGB(_pickerHue, _pickerSat, _pickerVal);
		string text = ColorToHex(val);
		int value = Mathf.RoundToInt(val.r * 255f);
		int value2 = Mathf.RoundToInt(val.g * 255f);
		int value3 = Mathf.RoundToInt(val.b * 255f);
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		Color color = GUI.color;
		GUI.color = val;
		GUILayout.Box("", _swatchStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedWidth(num4),
			GuiStyles.CachedHeight(num4)
		});
		GUI.color = color;
		GUILayout.Space(GuiStyles.Spacing.MenuSize(8f, 4f));
		GUILayout.BeginVertical((Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.Label("<color=#E6EAEF><b>HEX</b></color>  <color=#FFD700>#" + text + "</color>", _pickerInfoStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.Label($"<color=#E6EAEF><b>RGB</b></color>  <color=#FF6B6B>{value}</color>, <color=#6BCB77>{value2}</color>, <color=#4D96FF>{value3}</color>", _pickerInfoStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Copy", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedWidth(num5),
			GuiStyles.CachedHeight(num5)
		}))
		{
			GUIUtility.systemCopyBuffer = "#" + text;
			NotifyUtils.Success("Color copied!");
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		return text;
	}

	private void ApplyPickerColor(ref string hexField, ConfigEntry<string> config)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Color c = Color.HSVToRGB(_pickerHue, _pickerSat, _pickerVal);
		hexField = ColorToHex(c);
		if (config != null)
		{
			config.Value = hexField;
		}
		MiscNameService.InvalidateCache();
	}

	private void DrawNamePresetsCard()
	{
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		if (GhostUI.CurrentBucket > LayoutBucket.Micro)
		{
			GUILayout.Label("<color=#949EAD>Name Presets</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
		}
		bool flag = GhostUI.CurrentBucket <= LayoutBucket.Micro;
		GUILayoutOption val = GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(16f));
		GUILayoutOption val2 = GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(flag ? 36f : 50f, flag ? 28f : 36f));
		GUILayoutOption val3 = GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(flag ? 22f : 26f, flag ? 16f : 20f));
		bool flag2 = GhostUI.CurrentBucket <= LayoutBucket.Tight;
		for (int i = 0; i < 5; i++)
		{
			ConfigEntry<string> namePresetSlot = MiscConfig.GetNamePresetSlot(i);
			if (namePresetSlot == null)
			{
				continue;
			}
			bool flag3 = !string.IsNullOrEmpty(namePresetSlot.Value);
			bool bold;
			bool italic;
			bool underline;
			bool strikethrough;
			int sizePercent;
			int colorMode;
			string colorHex;
			string gradStart;
			string gradEnd;
			string pulse;
			string pulse2;
			if (flag2)
			{
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Label(SlotIndexLabels[i], _tagLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val });
				if (flag3)
				{
					MiscConfig.TryDeserializeNamePreset(namePresetSlot.Value, out var name, out bold, out italic, out underline, out strikethrough, out sizePercent, out colorMode, out colorHex, out gradStart, out gradEnd, out pulse, out pulse2);
					string text = (string.IsNullOrEmpty(name) ? "Unnamed" : name);
					GUILayout.Label("<color=#E6EAEF><b>" + text + "</b></color>", _cardInfoStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
				else
				{
					GUILayout.Label("<color=#3A3E44>Empty</color>", _cardInfoStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Space(GuiStyles.Spacing.MenuSize(flag ? 4f : 20f, flag ? 2f : 14f));
				if (flag3)
				{
					GUI.color = ColGreen;
					if (GUILayout.Button(flag ? "[>]" : "Load", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val2 }))
					{
						MiscConfig.TryDeserializeNamePreset(namePresetSlot.Value, out var name2, out strikethrough, out underline, out italic, out bold, out colorMode, out sizePercent, out pulse2, out pulse, out gradEnd, out gradStart, out colorHex);
						LoadNamePreset(namePresetSlot.Value);
						NotifyUtils.Success("Loaded: " + name2);
					}
					GUI.color = ColRed;
					if (GUILayout.Button("X", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val3 }))
					{
						namePresetSlot.Value = "";
						NotifyUtils.Success("Preset deleted");
					}
				}
				GUI.color = ColBlue;
				if (GUILayout.Button(flag ? "[+]" : "Save", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val2 }))
				{
					SaveCurrentNamePreset(namePresetSlot);
					NotifyUtils.Success($"Saved to slot {i + 1}");
				}
				GUI.color = Color.white;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
				continue;
			}
			GUILayoutOption val4 = GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(100f, 60f));
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Label(SlotIndexLabels[i], _tagLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val });
			if (flag3)
			{
				MiscConfig.TryDeserializeNamePreset(namePresetSlot.Value, out var name3, out bold, out italic, out underline, out strikethrough, out sizePercent, out colorMode, out colorHex, out gradStart, out gradEnd, out pulse, out pulse2);
				string text2 = (string.IsNullOrEmpty(name3) ? "Unnamed" : name3);
				GUILayout.Label("<color=#E6EAEF><b>" + text2 + "</b></color>", _cardInfoStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val4 });
				GUI.color = ColGreen;
				if (GUILayout.Button("Load", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val2 }))
				{
					LoadNamePreset(namePresetSlot.Value);
					NotifyUtils.Success("Loaded preset: " + text2);
				}
				GUI.color = ColRed;
				if (GUILayout.Button("X", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val3 }))
				{
					namePresetSlot.Value = "";
					NotifyUtils.Success("Preset deleted");
				}
				GUI.color = Color.white;
			}
			else
			{
				GUILayout.Label("<color=#3A3E44>Empty</color>", _cardInfoStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val4 });
			}
			GUI.color = ColBlue;
			if (GUILayout.Button("Save", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val2 }))
			{
				SaveCurrentNamePreset(namePresetSlot);
				NotifyUtils.Success($"Saved to slot {i + 1}");
			}
			GUI.color = Color.white;
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}

	private void SaveCurrentNamePreset(ConfigEntry<string> slot)
	{
		if (slot != null)
		{
			slot.Value = MiscConfig.SerializeNamePreset(MiscConfig.CustomNameText?.Value ?? "", MiscConfig.NameBold?.Value ?? false, MiscConfig.NameItalic?.Value ?? false, MiscConfig.NameUnderline?.Value ?? false, MiscConfig.NameStrikethrough?.Value ?? false, MiscConfig.NameSizePercent?.Value ?? 100, MiscConfig.NameColorMode?.Value ?? 0, MiscConfig.NameColorHex?.Value ?? "FFFFFF", MiscConfig.NameGradientStartHex?.Value ?? "FF0000", MiscConfig.NameGradientEndHex?.Value ?? "0000FF", MiscConfig.NamePulseColor1Hex?.Value ?? "FF0000", MiscConfig.NamePulseColor2Hex?.Value ?? "00FF00");
		}
	}

	private void LoadNamePreset(string data)
	{
		if (MiscConfig.TryDeserializeNamePreset(data, out var name, out var bold, out var italic, out var underline, out var strikethrough, out var sizePercent, out var colorMode, out var colorHex, out var gradStart, out var gradEnd, out var pulse, out var pulse2))
		{
			if (MiscConfig.CustomNameText != null)
			{
				MiscConfig.CustomNameText.Value = name;
			}
			if (MiscConfig.NameBold != null)
			{
				MiscConfig.NameBold.Value = bold;
			}
			if (MiscConfig.NameItalic != null)
			{
				MiscConfig.NameItalic.Value = italic;
			}
			if (MiscConfig.NameUnderline != null)
			{
				MiscConfig.NameUnderline.Value = underline;
			}
			if (MiscConfig.NameStrikethrough != null)
			{
				MiscConfig.NameStrikethrough.Value = strikethrough;
			}
			if (MiscConfig.NameSizePercent != null)
			{
				MiscConfig.NameSizePercent.Value = sizePercent;
			}
			if (MiscConfig.NameColorMode != null)
			{
				MiscConfig.NameColorMode.Value = colorMode;
			}
			if (MiscConfig.NameColorHex != null)
			{
				MiscConfig.NameColorHex.Value = colorHex;
			}
			if (MiscConfig.NameGradientStartHex != null)
			{
				MiscConfig.NameGradientStartHex.Value = gradStart;
			}
			if (MiscConfig.NameGradientEndHex != null)
			{
				MiscConfig.NameGradientEndHex.Value = gradEnd;
			}
			if (MiscConfig.NamePulseColor1Hex != null)
			{
				MiscConfig.NamePulseColor1Hex.Value = pulse;
			}
			if (MiscConfig.NamePulseColor2Hex != null)
			{
				MiscConfig.NamePulseColor2Hex.Value = pulse2;
			}
			_customNameBuffer = name;
			_hexInput = colorHex;
			_gradStartInput = gradStart;
			_gradEndInput = gradEnd;
			_pulse1Input = pulse;
			_pulse2Input = pulse2;
			MiscNameService.InvalidateCache();
		}
	}

	private void DrawOutfitManagerSection()
	{
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0648: Unknown result type (might be due to invalid IL or missing references)
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0690: Unknown result type (might be due to invalid IL or missing references)
		//IL_0666: Unknown result type (might be due to invalid IL or missing references)
		//IL_084a: Unknown result type (might be due to invalid IL or missing references)
		//IL_084f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0892: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0905: Unknown result type (might be due to invalid IL or missing references)
		//IL_0868: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0952: Unknown result type (might be due to invalid IL or missing references)
		//IL_073e: Unknown result type (might be due to invalid IL or missing references)
		//IL_078b: Unknown result type (might be due to invalid IL or missing references)
		SectionTitle((GhostUI.CurrentBucket <= LayoutBucket.Micro) ? "OUTFIT" : "CURRENT OUTFIT", (GhostUI.CurrentBucket > LayoutBucket.Tight) ? "Snapshot of your cosmetics" : null);
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			object obj;
			if (localPlayer == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo data = localPlayer.Data;
				obj = ((data != null) ? data.DefaultOutfit : null);
			}
			if (obj != null)
			{
				PlayerOutfit defaultOutfit = localPlayer.Data.DefaultOutfit;
				int colorId = defaultOutfit.ColorId;
				string text = ((colorId >= 0 && colorId < AmongUsColorNames.Length) ? AmongUsColorNames[colorId] : "Unknown");
				if (GhostUI.CurrentBucket <= LayoutBucket.Micro)
				{
					DrawInfoRow("Color", "<color=#FFD700>" + text + "</color>");
					DrawInfoRow("Hat", defaultOutfit.HatId);
					DrawInfoRow("Skin", defaultOutfit.SkinId);
				}
				else if (GhostUI.CurrentBucket <= LayoutBucket.Tight)
				{
					DrawInfoRow("Color", $"<color=#FFD700>{text}</color> <color=#4A4E54>(ID: {colorId})</color>");
					DrawInfoRow("Hat", defaultOutfit.HatId);
					DrawInfoRow("Skin", defaultOutfit.SkinId);
					DrawInfoRow("Visor", defaultOutfit.VisorId);
					DrawInfoRow("Pet", defaultOutfit.PetId);
					DrawInfoRow("Plate", defaultOutfit.NamePlateId);
				}
				else
				{
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
					DrawInfoRow("Color", $"<color=#FFD700>{text}</color> <color=#4A4E54>(ID: {colorId})</color>");
					DrawInfoRow("Hat", defaultOutfit.HatId);
					DrawInfoRow("Skin", defaultOutfit.SkinId);
					GUILayout.EndVertical();
					GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
					DrawInfoRow("Visor", defaultOutfit.VisorId);
					DrawInfoRow("Pet", defaultOutfit.PetId);
					DrawInfoRow("Plate", defaultOutfit.NamePlateId);
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("<color=#4A4E54>  Not in game — join a lobby to see outfit data</color>", _cardInfoStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
		}
		catch
		{
			GUILayout.Label("<color=#4A4E54>  Unable to read outfit</color>", _cardInfoStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		GUILayout.EndVertical();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(6f, 3f));
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label((GhostUI.CurrentBucket <= LayoutBucket.Micro) ? "<color=#949EAD>Save Outfit</color>" : "<color=#949EAD>Save Current Outfit</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
		float w = GuiStyles.Spacing.MenuSize((GhostUI.CurrentBucket == LayoutBucket.Micro) ? 90f : ((GhostUI.CurrentBucket == LayoutBucket.Tight) ? 140f : ((GhostUI.CurrentBucket == LayoutBucket.Compact) ? 180f : 220f)), 70f);
		float w2 = GuiStyles.Spacing.MenuSize((GhostUI.CurrentBucket <= LayoutBucket.Micro) ? 60f : ((GhostUI.CurrentBucket <= LayoutBucket.Tight) ? 70f : 80f), 50f);
		if (GhostUI.CurrentBucket <= LayoutBucket.Tight)
		{
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Label("<color=#6B7280>Name</color>", _tagLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(38f, 24f)) });
			DrawTextInput("outfitName", ref _newOutfitName, 30, GuiStyles.CachedWidth(w));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUI.color = ColGreen;
			if (GUILayout.Button("[>]  Save", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w2) }))
			{
				SaveCurrentOutfitToFirstEmptySlot();
				NotifyUtils.Success("Outfit saved!");
			}
			GUI.color = Color.white;
		}
		else
		{
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Label("<color=#6B7280>Name</color>", _tagLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(38f, 24f)) });
			DrawTextInput("outfitName", ref _newOutfitName, 30, GuiStyles.CachedWidth(w));
			GUI.color = ColGreen;
			if (GUILayout.Button("[>]  Save", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w2) }))
			{
				SaveCurrentOutfitToFirstEmptySlot();
				NotifyUtils.Success("Outfit saved!");
			}
			GUI.color = Color.white;
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(6f, 3f));
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label("<color=#949EAD>Saved Outfits</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
		if (MiscConfig.OutfitSlots == null)
		{
			GUILayout.EndVertical();
			return;
		}
		bool flag = GhostUI.CurrentBucket <= LayoutBucket.Micro;
		GUILayoutOption val = GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(16f));
		GUILayoutOption val2 = GuiStyles.CachedHeight(GuiStyles.Spacing.MenuSize(16f));
		GUILayoutOption val3 = GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(flag ? 36f : 50f, flag ? 28f : 36f));
		GUILayoutOption val4 = GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(flag ? 22f : 26f, flag ? 16f : 20f));
		for (int i = 0; i < MiscConfig.OutfitSlots.Length; i++)
		{
			ConfigEntry<string> val5 = MiscConfig.OutfitSlots[i];
			ConfigEntry<string> val6 = MiscConfig.OutfitSlotNames[i];
			if (val5 == null)
			{
				continue;
			}
			bool flag2 = !string.IsNullOrEmpty(val5.Value);
			string namePlateId;
			string petId;
			string visorId;
			string skinId;
			string hatId;
			if (flag && flag2)
			{
				string text2 = val6?.Value ?? "";
				if (string.IsNullOrEmpty(text2))
				{
					text2 = "Outfit";
				}
				MiscConfig.TryDeserializeOutfit(val5.Value, out var colorId2, out namePlateId, out petId, out visorId, out skinId, out hatId);
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Label(SlotIndexLabels[i], _tagLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val });
				Color color = GUI.color;
				if (colorId2 >= 0 && colorId2 < PaletteColors.Length)
				{
					GUI.color = PaletteColors[colorId2];
				}
				GUILayout.Box("", _swatchStyle, (GUILayoutOption[])(object)new GUILayoutOption[2] { val, val2 });
				GUI.color = color;
				GUILayout.Label("<color=#E6EAEF><b>" + text2 + "</b></color>", _cardInfoStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Space(GuiStyles.Spacing.MenuSize(flag ? 4f : 20f, flag ? 2f : 14f));
				GUI.color = ColGreen;
				if (GUILayout.Button(flag ? "[>]" : "Load", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val3 }))
				{
					ApplyOutfitLocally(val5.Value);
					NotifyUtils.Success("Applied: " + text2);
				}
				GUI.color = ColRed;
				if (GUILayout.Button("X", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val4 }))
				{
					val5.Value = "";
					if (val6 != null)
					{
						val6.Value = "";
					}
					NotifyUtils.Success("Outfit deleted");
				}
				GUI.color = Color.white;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
				continue;
			}
			GUILayoutOption val7 = GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize((GhostUI.CurrentBucket == LayoutBucket.Tight) ? 64f : 80f, 50f));
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Label(SlotIndexLabels[i], _tagLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val });
			if (flag2)
			{
				string text3 = val6?.Value ?? "";
				if (string.IsNullOrEmpty(text3))
				{
					text3 = "Outfit";
				}
				MiscConfig.TryDeserializeOutfit(val5.Value, out var colorId3, out hatId, out skinId, out visorId, out petId, out namePlateId);
				Color color2 = GUI.color;
				if (colorId3 >= 0 && colorId3 < PaletteColors.Length)
				{
					GUI.color = PaletteColors[colorId3];
				}
				GUILayout.Box("", _swatchStyle, (GUILayoutOption[])(object)new GUILayoutOption[2] { val, val2 });
				GUI.color = color2;
				GUILayout.Label("<color=#E6EAEF><b>" + text3 + "</b></color>", _cardInfoStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val7 });
				GUI.color = ColGreen;
				if (GUILayout.Button("Load", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val3 }))
				{
					ApplyOutfitLocally(val5.Value);
					NotifyUtils.Success("Applied: " + text3);
				}
				GUI.color = ColRed;
				if (GUILayout.Button("X", _presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val4 }))
				{
					val5.Value = "";
					if (val6 != null)
					{
						val6.Value = "";
					}
					NotifyUtils.Success("Outfit deleted");
				}
				GUI.color = Color.white;
			}
			else
			{
				GUILayout.Label("<color=#3A3E44>-  Empty</color>", _cardInfoStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}

	private void DrawInfoRow(string label, string value)
	{
		GUILayout.Label($"<color=#6B7280>{label}:</color>  <color=#E6EAEF>{value}</color>", _cardInfoStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
	}

	private void SaveCurrentOutfitToFirstEmptySlot()
	{
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			object obj;
			if (localPlayer == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo data = localPlayer.Data;
				obj = ((data != null) ? data.DefaultOutfit : null);
			}
			if (obj == null)
			{
				return;
			}
			PlayerOutfit defaultOutfit = localPlayer.Data.DefaultOutfit;
			string value = MiscConfig.SerializeOutfit(defaultOutfit.ColorId, defaultOutfit.HatId, defaultOutfit.SkinId, defaultOutfit.VisorId, defaultOutfit.PetId, defaultOutfit.NamePlateId);
			int num = MiscConfig.OutfitSlots.Length - 1;
			for (int i = 0; i < MiscConfig.OutfitSlots.Length; i++)
			{
				if (string.IsNullOrEmpty(MiscConfig.OutfitSlots[i]?.Value))
				{
					num = i;
					break;
				}
			}
			MiscConfig.OutfitSlots[num].Value = value;
			if (MiscConfig.OutfitSlotNames[num] != null)
			{
				MiscConfig.OutfitSlotNames[num].Value = (string.IsNullOrEmpty(_newOutfitName) ? $"Outfit {num + 1}" : _newOutfitName);
			}
			_newOutfitName = "";
		}
		catch
		{
		}
	}

	private void ApplyOutfitLocally(string data)
	{
		try
		{
			if (!MiscConfig.TryDeserializeOutfit(data, out var colorId, out var hatId, out var skinId, out var visorId, out var petId, out var namePlateId))
			{
				return;
			}
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if ((Object)(object)localPlayer == (Object)null)
			{
				return;
			}
			AmongUsClient instance = AmongUsClient.Instance;
			if (!((Object)(object)instance != (Object)null) || !((InnerNetClient)instance).AmConnected)
			{
				if ((Object)(object)localPlayer.cosmetics != (Object)null)
				{
					localPlayer.cosmetics.SetColor(colorId);
					localPlayer.cosmetics.SetHat(hatId, colorId);
					localPlayer.cosmetics.SetSkin(skinId, colorId, (Action)null);
					localPlayer.cosmetics.SetVisor(visorId, colorId);
					localPlayer.cosmetics.SetPetIdle(petId, colorId, (Action)null);
				}
				localPlayer.SetNamePlate(namePlateId);
				return;
			}
			if (colorId >= 0)
			{
				byte b = PickFreeColor((byte)colorId, localPlayer.PlayerId);
				localPlayer.CmdCheckColor(b);
			}
			if (!string.IsNullOrEmpty(hatId))
			{
				localPlayer.RpcSetHat(hatId);
			}
			if (!string.IsNullOrEmpty(skinId))
			{
				localPlayer.RpcSetSkin(skinId);
			}
			if (!string.IsNullOrEmpty(visorId))
			{
				localPlayer.RpcSetVisor(visorId);
			}
			if (!string.IsNullOrEmpty(petId))
			{
				localPlayer.RpcSetPet(petId);
			}
			if (!string.IsNullOrEmpty(namePlateId))
			{
				localPlayer.RpcSetNamePlate(namePlateId);
			}
		}
		catch
		{
		}
	}

	private static byte PickFreeColor(byte desired, byte selfPid)
	{
		int num;
		try
		{
			num = ((Palette.PlayerColors != null) ? ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length : 18);
		}
		catch
		{
			num = 18;
		}
		if (num <= 0)
		{
			num = 18;
		}
		HashSet<byte> hashSet = new HashSet<byte>();
		try
		{
			GameData instance = GameData.Instance;
			List<NetworkedPlayerInfo> val = ((instance != null) ? instance.AllPlayers : null);
			if (val != null)
			{
				for (int i = 0; i < val.Count; i++)
				{
					NetworkedPlayerInfo val2 = val[i];
					if (!((Object)(object)val2 == (Object)null) && !val2.Disconnected && val2.PlayerId != selfPid && val2.DefaultOutfit != null)
					{
						hashSet.Add((byte)val2.DefaultOutfit.ColorId);
					}
				}
			}
		}
		catch
		{
		}
		byte b = desired;
		if (b >= num)
		{
			b = 0;
		}
		int num2 = 0;
		while (num2++ < 100 && hashSet.Contains(b))
		{
			b = (byte)((b + 1) % num);
		}
		return b;
	}

	private void DrawChatColorSection()
	{
		bool flag = GhostUI.CurrentBucket <= LayoutBucket.Tight;
		bool flag2 = GhostUI.CurrentBucket <= LayoutBucket.Micro;
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.FlexibleSpace();
		GUILayout.Label(flag ? "<color=#FFD700><b>[LOCAL VIEW]</b></color>" : "<color=#FFD700><b>[LOCAL VIEW]</b></color>  <color=#6B7280>- only you see the color, others see default</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(6f, 3f));
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		bool flag3 = MiscConfig.ChatColorEnabled?.Value ?? false;
		string text = ((!flag2) ? (flag3 ? "  [ON]  Chat Color Active" : "  [OFF] Enable Chat Color") : (flag3 ? "[ON]  Chat Color" : "[OFF] Chat Color"));
		bool flag4 = GUILayout.Toggle(flag3, text, GuiStyles.ToggleStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		if (flag4 != flag3 && MiscConfig.ChatColorEnabled != null)
		{
			MiscConfig.ChatColorEnabled.Value = flag4;
		}
		GUILayout.EndVertical();
		if (flag4)
		{
			GUILayout.Space(GuiStyles.Spacing.MenuSize(6f, 3f));
			GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
			if (!flag)
			{
				GUILayout.Label("<color=#949EAD>Pick your chat color</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			DrawSwatchPalette(_chatColorSwatchAction, _chatColorHexInput);
			GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
			DrawHexInputRow("HEX", "chat_color_hex", ref _chatColorHexInput, MiscConfig.ChatColorHex);
			GUILayout.EndVertical();
			GUILayout.Space(GuiStyles.Spacing.MenuSize(6f, 3f));
			GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
			if (!flag)
			{
				GUILayout.Label("<color=#949EAD>Preview</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			string value = (IsValidPreviewHex(_chatColorHexInput) ? _chatColorHexInput : "FFFFFF");
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			object obj;
			if (localPlayer == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo data = localPlayer.Data;
				obj = ((data != null) ? data.PlayerName : null);
			}
			if (obj == null)
			{
				obj = "You";
			}
			string value2 = StripAngleBrackets((string)obj);
			string value3 = (flag ? "hi!" : "Hello from your client!");
			GUILayout.Label($"  <b><color=#FFFFFF>{value2}</color></b>  <color=#{value}>{value3}</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			if (!flag)
			{
				GUILayout.Label("  <color=#6B7280><i>Others see default color.</i></color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			GUILayout.EndVertical();
		}
	}

	private static bool IsValidPreviewHex(string hex)
	{
		if (string.IsNullOrEmpty(hex) || hex.Length != 6)
		{
			return false;
		}
		for (int i = 0; i < 6; i++)
		{
			char c = hex[i];
			if ((c < '0' || c > '9') && (c < 'a' || c > 'f') && (c < 'A' || c > 'F'))
			{
				return false;
			}
		}
		return true;
	}

	private static string StripAngleBrackets(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		bool flag = false;
		for (int i = 0; i < input.Length; i++)
		{
			if (input[i] == '<' || input[i] == '>')
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return input;
		}
		_stripSb.Clear();
		foreach (char c in input)
		{
			if (c != '<' && c != '>')
			{
				_stripSb.Append(c);
			}
		}
		return _stripSb.ToString();
	}

	private void DrawUtilitiesSection()
	{
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		SectionTitle((GhostUI.CurrentBucket <= LayoutBucket.Micro) ? "COPY" : "CLIPBOARD", (GhostUI.CurrentBucket > LayoutBucket.Tight) ? "One-click copy to system clipboard" : null);
		GUILayoutOption val = GuiStyles.CachedHeight(GuiStyles.Spacing.MenuSize((GhostUI.CurrentBucket <= LayoutBucket.Micro) ? 24f : 30f, 20f));
		bool flag = GhostUI.CurrentBucket <= LayoutBucket.Micro;
		if (GhostUI.CurrentBucket <= LayoutBucket.Tight)
		{
			GUI.color = ColClipBlue;
			if (GUILayout.Button(flag ? "Code" : "Copy Game Code", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val }))
			{
				CopyGameCode();
				NotifyUtils.Success("Game code copied!");
			}
			GUI.color = ColClipGreen;
			if (GUILayout.Button(flag ? "Players" : "Copy Players", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val }))
			{
				CopyPlayerList();
				NotifyUtils.Success("Player list copied!");
			}
			GUI.color = ColClipOrange;
			if (GUILayout.Button(flag ? "Outfit" : "Copy Outfit", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val }))
			{
				CopyMyOutfit();
				NotifyUtils.Success("Outfit data copied!");
			}
			GUI.color = Color.white;
		}
		else
		{
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = ColClipBlue;
			if (GUILayout.Button("Copy Game Code", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val }))
			{
				CopyGameCode();
				NotifyUtils.Success("Game code copied!");
			}
			GUI.color = ColClipGreen;
			if (GUILayout.Button("Copy Players", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val }))
			{
				CopyPlayerList();
				NotifyUtils.Success("Player list copied!");
			}
			GUI.color = ColClipOrange;
			if (GUILayout.Button("Copy Outfit", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { val }))
			{
				CopyMyOutfit();
				NotifyUtils.Success("Outfit data copied!");
			}
			GUI.color = Color.white;
			GUILayout.EndHorizontal();
		}
		GUILayout.Space(GuiStyles.Spacing.MenuSize(flag ? 4f : 8f, flag ? 2f : 4f));
		SectionTitle(flag ? "COLOR" : "BODY COLOR", (GhostUI.CurrentBucket > LayoutBucket.Tight) ? "Visually override your crewmate color" : null);
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		bool flag2 = MiscConfig.LocalColorOverrideEnabled?.Value ?? false;
		string text = ((GhostUI.CurrentBucket > LayoutBucket.Micro) ? (flag2 ? "  [ON]  Body Color Active" : "  [OFF] Body Color Override") : (flag2 ? "[ON]  Color" : "[OFF] Body Color"));
		bool flag3 = GUILayout.Toggle(flag2, text, GuiStyles.ToggleStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		if (flag3 != flag2 && MiscConfig.LocalColorOverrideEnabled != null)
		{
			MiscConfig.LocalColorOverrideEnabled.Value = flag3;
		}
		if (flag3)
		{
			GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
			int num = MiscConfig.LocalColorOverrideId?.Value ?? 0;
			float num2 = GuiStyles.Spacing.MenuSize(24f);
			GUILayoutOption val2 = GuiStyles.CachedWidth(num2);
			GUILayoutOption val3 = GuiStyles.CachedHeight(num2);
			float num3 = num2 + GuiStyles.Spacing.MenuSize(4f, 2f);
			int num4 = Mathf.Clamp(Mathf.FloorToInt((GhostUI.CurrentContentWidth - GuiStyles.Spacing.MenuSize(48f, 28f)) / num3), 3, 9);
			HashSet<int> hashSet = null;
			try
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				if ((Object)(object)GameData.Instance != (Object)null && (realtimeSinceStartup - _takenColorsCacheTime > 0.5f || _takenColorsLastVersion != GameData.Instance.AllPlayers.Count))
				{
					_takenColorsCache.Clear();
					_takenColorsLastVersion = GameData.Instance.AllPlayers.Count;
					PlayerControl localPlayer = PlayerControl.LocalPlayer;
					byte b = ((localPlayer != null) ? localPlayer.PlayerId : byte.MaxValue);
					Enumerator<NetworkedPlayerInfo> enumerator = GameData.Instance.AllPlayers.GetEnumerator();
					while (enumerator.MoveNext())
					{
						NetworkedPlayerInfo current = enumerator.Current;
						if ((Object)(object)current != (Object)null && !current.Disconnected && current.PlayerId != b)
						{
							_takenColorsCache.Add(current.DefaultOutfit.ColorId);
						}
					}
					_takenColorsCacheTime = realtimeSinceStartup;
				}
				if ((Object)(object)GameData.Instance != (Object)null)
				{
					hashSet = _takenColorsCache;
				}
			}
			catch
			{
			}
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
			for (int i = 0; i < 18; i++)
			{
				bool num5 = i == num;
				bool flag4 = hashSet?.Contains(i) ?? false;
				Color color = GUI.color;
				GUI.color = (flag4 ? (PaletteColors[i] * 0.35f) : PaletteColors[i]);
				GUIStyle val4;
				string text2;
				if (num5)
				{
					val4 = _swatchSelectedStyle;
					text2 = "*";
				}
				else if (flag4)
				{
					val4 = _swatchSelectedStyle;
					text2 = "X";
				}
				else
				{
					val4 = _swatchStyle;
					text2 = "";
				}
				if (GUILayout.Button(text2, val4, (GUILayoutOption[])(object)new GUILayoutOption[2] { val2, val3 }))
				{
					if (MiscConfig.LocalColorOverrideId != null)
					{
						MiscConfig.LocalColorOverrideId.Value = i;
					}
					try
					{
						PlayerControl localPlayer2 = PlayerControl.LocalPlayer;
						if ((Object)(object)localPlayer2 != (Object)null)
						{
							localPlayer2.CmdCheckColor((byte)i);
							localPlayer2.cosmetics.SetColor(i);
						}
					}
					catch
					{
					}
				}
				GUI.color = color;
				if ((i + 1) % num4 == 0 && i < 17)
				{
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			if (!flag)
			{
				GUILayout.Label("<color=#6B7280>Colors marked X are taken. Host may assign a nearby color.</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
		}
		GUILayout.EndVertical();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		if (GhostUI.CurrentBucket > LayoutBucket.Micro)
		{
			GUILayout.Label("<color=#949EAD>Name Text Color</color>", _tagLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		bool flag5 = MiscConfig.NameColorOverrideEnabled?.Value ?? false;
		string text3 = ((GhostUI.CurrentBucket > LayoutBucket.Micro) ? (flag5 ? "  [ON]  Name Color Override Active" : "  [OFF] Name Color Override") : (flag5 ? "[ON]  Name" : "[OFF] Name Color"));
		bool flag6 = GUILayout.Toggle(flag5, text3, GuiStyles.ToggleStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		if (flag6 != flag5 && MiscConfig.NameColorOverrideEnabled != null)
		{
			MiscConfig.NameColorOverrideEnabled.Value = flag6;
		}
		if (flag6)
		{
			GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 2f));
			DrawHSVColorPicker(ref _nameColorInput, MiscConfig.NameColorOverrideHex, "name_col_override");
			GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
			if (GhostUI.CurrentBucket > LayoutBucket.Micro)
			{
				DrawSwatchPalette(_nameColSwatchAction, _nameColorInput);
			}
		}
		GUILayout.EndVertical();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(flag ? 4f : 8f, flag ? 2f : 4f));
		SectionTitle(flag ? "VISIBLE" : "VISIBILITY", (GhostUI.CurrentBucket > LayoutBucket.Tight) ? "Hide your own name or pet locally" : null);
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		bool flag7 = MiscConfig.HideMyName?.Value ?? false;
		string text4 = ((GhostUI.CurrentBucket > LayoutBucket.Micro) ? (flag7 ? "  [ON]  Name Hidden (local)" : "  [OFF] Hide My Name") : (flag7 ? "[ON]  Hidden" : "[OFF] Hide Name"));
		bool flag8 = GUILayout.Toggle(flag7, text4, GuiStyles.ToggleStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		if (flag8 != flag7 && MiscConfig.HideMyName != null)
		{
			MiscConfig.HideMyName.Value = flag8;
		}
		GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
		bool flag9 = MiscConfig.HideMyPet?.Value ?? false;
		string text5 = ((GhostUI.CurrentBucket > LayoutBucket.Micro) ? (flag9 ? "  [ON]  Pet Hidden (local)" : "  [OFF] Hide My Pet") : (flag9 ? "[ON]  Hidden" : "[OFF] Hide Pet"));
		bool flag10 = GUILayout.Toggle(flag9, text5, GuiStyles.ToggleStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		if (flag10 != flag9 && MiscConfig.HideMyPet != null)
		{
			MiscConfig.HideMyPet.Value = flag10;
		}
		GUILayout.EndVertical();
	}

	private void CopyGameCode()
	{
		try
		{
			AmongUsClient instance = AmongUsClient.Instance;
			if ((Object)(object)instance != (Object)null && ((InnerNetClient)instance).GameId != 0)
			{
				GUIUtility.systemCopyBuffer = GameCode.IntToGameName(((InnerNetClient)instance).GameId);
			}
		}
		catch
		{
		}
	}

	private void CopyPlayerList()
	{
		try
		{
			_clipboardSb.Clear();
			List<PlayerControl> allPlayerControls = PlayerControl.AllPlayerControls;
			if (allPlayerControls == null || allPlayerControls.Count == 0)
			{
				return;
			}
			_clipboardSb.AppendLine("=== Player List ===");
			for (int i = 0; i < allPlayerControls.Count; i++)
			{
				PlayerControl val = allPlayerControls[i];
				if (!((Object)(object)((val != null) ? val.Data : null) == (Object)null))
				{
					PlayerOutfit defaultOutfit = val.Data.DefaultOutfit;
					int num = ((defaultOutfit != null) ? defaultOutfit.ColorId : (-1));
					string value = ((num >= 0 && num < AmongUsColorNames.Length) ? AmongUsColorNames[num] : "?");
					string value2 = (val.Data.IsDead ? " [DEAD]" : "");
					int ownerId = ((InnerNetObject)val).OwnerId;
					AmongUsClient instance = AmongUsClient.Instance;
					string value3 = ((ownerId == ((instance != null) ? new int?(((InnerNetClient)instance).HostId) : null)) ? " [HOST]" : "");
					StringBuilder clipboardSb = _clipboardSb;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 4, clipboardSb);
					handler.AppendFormatted(val.Data.PlayerName);
					handler.AppendLiteral(" (");
					handler.AppendFormatted(value);
					handler.AppendLiteral(")");
					handler.AppendFormatted(value3);
					handler.AppendFormatted(value2);
					clipboardSb.AppendLine(ref handler);
				}
			}
			GUIUtility.systemCopyBuffer = _clipboardSb.ToString();
		}
		catch
		{
		}
	}

	private void CopyMyOutfit()
	{
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			object obj;
			if (localPlayer == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo data = localPlayer.Data;
				obj = ((data != null) ? data.DefaultOutfit : null);
			}
			if (obj != null)
			{
				PlayerOutfit defaultOutfit = localPlayer.Data.DefaultOutfit;
				GUIUtility.systemCopyBuffer = MiscConfig.SerializeOutfit(defaultOutfit.ColorId, defaultOutfit.HatId, defaultOutfit.SkinId, defaultOutfit.VisorId, defaultOutfit.PetId, defaultOutfit.NamePlateId);
			}
		}
		catch
		{
		}
	}
}
