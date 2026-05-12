using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using ModMenuCrew.Easing;
using ModMenuCrew.Features;
using ModMenuCrew.Networking;
using ModMenuCrew.UI;
using ModMenuCrew.UI.Menus;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew;

public static class GhostUI
{
	private class CachedPlayerData
	{
		public PlayerControl Player;

		public string Name;

		public bool IsImpostor;

		public bool IsDead;

		public Color DisplayColor;

		public string StatusText;

		public int SortPriority;

		public bool Disconnected;

		public bool Active;
	}

	private sealed class SecureStreamReader : IDisposable
	{
		private readonly Stream _stream;

		private readonly byte[] _inverseMap;

		private volatile byte _lastDecodedByte;

		private readonly char[] _stringBuffer = new char[64];

		private readonly byte[] _stringDecodeBuffer = new byte[4096];

		public long Position
		{
			get
			{
				return _stream.Position;
			}
			set
			{
				_stream.Position = value;
			}
		}

		public long Length => _stream.Length;

		public bool EndOfStream => _stream.Position >= _stream.Length;

		public SecureStreamReader(Stream scrambledStream, byte[] inverseMap)
		{
			_stream = scrambledStream ?? throw new InvalidOperationException();
			_inverseMap = inverseMap ?? throw new InvalidOperationException();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public byte ReadByte()
		{
			int num = _stream.ReadByte();
			if (num < 0)
			{
				return byte.MaxValue;
			}
			_lastDecodedByte = _inverseMap[num];
			byte lastDecodedByte = _lastDecodedByte;
			_lastDecodedByte = 0;
			return lastDecodedByte;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public unsafe float ReadSingle()
		{
			byte num = ReadByte();
			byte b = ReadByte();
			byte b2 = ReadByte();
			byte b3 = ReadByte();
			int num2 = num | (b << 8) | (b2 << 16) | (b3 << 24);
			return *(float*)(&num2);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public float ReadSingleSafe()
		{
			byte[] array = new byte[4]
			{
				ReadByte(),
				ReadByte(),
				ReadByte(),
				ReadByte()
			};
			float result = BitConverter.ToSingle(array, 0);
			array[0] = (array[1] = (array[2] = (array[3] = 0)));
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public long ReadInt64()
		{
			long num = 0L;
			for (int i = 0; i < 8; i++)
			{
				num |= (long)((ulong)ReadByte() << i * 8);
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public ushort ReadUInt16()
		{
			byte num = ReadByte();
			byte b = ReadByte();
			return (ushort)(num | (b << 8));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public string ReadString()
		{
			int num = 0;
			int num2 = 0;
			byte b;
			do
			{
				b = ReadByte();
				num |= (b & 0x7F) << num2;
				num2 += 7;
				if (num2 > 35)
				{
					return string.Empty;
				}
			}
			while ((b & 0x80u) != 0);
			if (num == 0)
			{
				return string.Empty;
			}
			if (num > 4096)
			{
				return string.Empty;
			}
			byte[] array = ((num <= _stringDecodeBuffer.Length) ? _stringDecodeBuffer : new byte[num]);
			for (int i = 0; i < num; i++)
			{
				array[i] = ReadByte();
			}
			try
			{
				return Encoding.UTF8.GetString(array, 0, num);
			}
			catch
			{
				return string.Empty;
			}
		}

		public void Skip(long count)
		{
			_stream.Position += count;
		}

		public void Dispose()
		{
			for (int i = 0; i < _stringBuffer.Length; i++)
			{
				_stringBuffer[i] = '\0';
			}
			_lastDecodedByte = 0;
			_stream?.Dispose();
		}
	}

	private static uint _pp_LastRefreshFrame = 0u;

	private static float _pp_LastCleanupTime = 0f;

	private static readonly List<CachedPlayerData> _pp_DataPool = new List<CachedPlayerData>(15);

	private static readonly List<CachedPlayerData> _pp_ActiveList = new List<CachedPlayerData>(15);

	private static readonly HashSet<byte> _pp_TriedFix = new HashSet<byte>();

	private static readonly Dictionary<byte, Color> _pp_DeadColorCache = new Dictionary<byte, Color>();

	private static readonly HashSet<byte> _pp_CurrentIdsCache = new HashSet<byte>();

	private static readonly Dictionary<byte, Color> _pp_NameColorCache = new Dictionary<byte, Color>();

	private static readonly Dictionary<byte, Color> _pp_StatusColorCache = new Dictionary<byte, Color>();

	private static readonly List<byte> _pp_KeysToRemove = new List<byte>();

	private static readonly Comparison<CachedPlayerData> _pp_SortComparison = (CachedPlayerData a, CachedPlayerData b) => (a.SortPriority != b.SortPriority) ? a.SortPriority.CompareTo(b.SortPriority) : string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);

	private static readonly RectOffset _pp_Margin4 = new RectOffset
	{
		left = 4,
		right = 4,
		top = 4,
		bottom = 4
	};

	private static readonly RectOffset _pp_Margin0 = new RectOffset
	{
		left = 0,
		right = 0,
		top = 0,
		bottom = 0
	};

	private static GUIStyle _pp_ColorBoxStyle;

	private static GUIStyle _pp_PlayerNameStyle;

	private static GUIStyle _pp_ImpostorNameStyle;

	private static GUIStyle _pp_StatusStyle;

	private static GUIStyle _pp_RoleButtonStyle;

	private static GUIStyle _pp_PreAssignButtonStyle;

	private static GUIStyle _pp_PreAssignLabelStyle;

	private const float FADE_DURATION = 0.25f;

	private const float GLOW_PULSE_SPEED = 2f;

	private const float SNAP_DISTANCE = 15f;

	private const float SIDEBAR_WIDTH = 200f;

	private const float BUTTON_HEIGHT = 42f;

	private const float ICON_SIZE = 32f;

	private const float BUCKET_HYSTERESIS = 20f;

	private static GUIStyle _resizeDotStyle;

	private static GUIStyle _resizeGlowStyle;

	private static GUIStyle _resizeDimStyle;

	private static GUIStyle _resizeDimBgStyle;

	private static int _lastBucketFrame = -1;

	private static CachedPlayerData _currentContext = null;

	private const int TOKEN_MISMATCH_TOLERANCE = 60;

	private static int _tokenMismatchFrameCount = 0;

	private const int INTEGRITY_MISMATCH_TOLERANCE = 30;

	private static int _integrityMismatchFrameCount = 0;

	private static float _integrityFirstMismatchTime = -1f;

	private static float _lastSelfHealTime = -999f;

	private const float SELF_HEAL_COOLDOWN_SEC = 10f;

	private const int V5_OFFSET_MAGIC = 0;

	private const int V5_OFFSET_SIGNATURE = 4;

	private const int V5_SIZE_SIGNATURE = 256;

	private const int V5_OFFSET_SESSION_TOKEN = 260;

	private const int V5_OFFSET_INVERSE_MAP = 268;

	private const int V5_OFFSET_SEED = 524;

	private const int V5_OFFSET_TIMESTAMP = 528;

	private const int V5_OFFSET_BYTECODE = 536;

	private const int V5_MIN_BYTECODE_LENGTH = 537;

	private const long TIMESTAMP_TOLERANCE_MS = 300000L;

	private static readonly string _c1 = "10nomJIIOLVleBhf8OiVGn/PpaOnlN1Zvl0MfCN+Qymp3KEGIclegEujxXU28osU";

	private static readonly string _c2 = "jF2ND/FsnC6vwu+x9WbBaURjBaFY6rgYB8EpVYls5SoHikaq4+407SPDo/1wHa+J";

	private static readonly string _c3 = "3tU3a+e5D7mIFAWo13N11b2G9Veg+QHR7mtq3qB3Q6ltX9KKAEtJPSdhPRdizp8z";

	private static readonly string _c4 = "cXvnZJ6PLFdOcCRgRCAChGGYUnm7rMdJwcFwjxE2WADHscpjcqiPuQ5pTU/KIrYg";

	private static readonly string _c5 = "NzZvcpFF20/10ejFGluvHSSzWwh68fJeJ21lGOZleLDfRU3vZQ4LaRwfqAQaLT0v";

	private static readonly string _c6 = "pTndZsLbtuhyAU0/MwVN3Q==";

	private static Dictionary<string, VMWindowState> _windowStates = new Dictionary<string, VMWindowState>();

	private static VMWindowState _currentWindow;

	private static Stack<Color> _colorStack = new Stack<Color>();

	private static Stack<string> _scrollStack = new Stack<string>();

	private static string _textInputFocusedId = null;

	private static readonly Dictionary<string, string> _textInputBuffers = new Dictionary<string, string>();

	private static readonly Dictionary<string, int> _textInputLastValue = new Dictionary<string, int>();

	private static readonly Dictionary<string, string> _textInputLabelCache = new Dictionary<string, string>();

	private static GUIStyle _textInputFocusedStyle = null;

	private static readonly Dictionary<string, string> _stagedStrings = new Dictionary<string, string>();

	private static readonly Stack<List<string>> _tabIdsPool = new Stack<List<string>>(4);

	private static readonly Stack<Stack<Color>> _colorStackPool = new Stack<Stack<Color>>(4);

	private static readonly Stack<Stack<string>> _scrollStackPool = new Stack<Stack<string>>(4);

	private static int _hoveredTabLastFrame = -1;

	private static int _hoveredTabThisFrame = -1;

	private static long _renderContextIdXor = 0L;

	private const long CONTEXT_SENTINEL = 4214628778660270185L;

	private static int _lastContextFrame = -1;

	private static long _lastContextToken = 0L;

	private static readonly long _contextSalt = 8952763866055401101L;

	private static bool _isWindowMinimized = false;

	private static readonly Dictionary<string, float> _pendingSliderCommit = new Dictionary<string, float>();

	private const float SLIDER_DEBOUNCE_SEC = 0.15f;

	private const float SLIDER_DEBOUNCE_TTL_SEC = 5f;

	private static Color _savedGuiColor = Color.white;

	private static Texture2D _cachedProgressBgTex;

	private static GUIStyle _cachedProgressBgStyle;

	private static Texture2D _cachedProgressFillTex;

	private static GUIStyle _cachedProgressFillStyle;

	private static Color _cachedProgressFillColor;

	private static Texture2D _cachedAlertInfoTex;

	private static Texture2D _cachedAlertWarnTex;

	private static Texture2D _cachedAlertErrTex;

	private static Texture2D _cachedAlertOkTex;

	private static GUIStyle[] _cachedAlertStyles = (GUIStyle[])(object)new GUIStyle[4];

	private static string _pendingTooltipText = null;

	private static Rect _pendingTooltipRect;

	private static byte _lastTabBadgeCount = 0;

	private static GUIStyle _tooltipStyle = null;

	private static float _tooltipHoverStartTime = 0f;

	private static Rect _tooltipHoverRect;

	private const float TOOLTIP_HOVER_DELAY = 0.3f;

	private static Dictionary<string, Action<long>> _actionRegistry;

	private static List<string> _tabIds = new List<string>();

	private static byte[] _lastIdentityCheckedMap;

	private static bool _lastIdentityCheckResult;

	private static long _cachedFrameNowMs;

	private static int _cachedFrameNowMsFrame = -1;

	private static Dictionary<string, int> _tabIdLookup = new Dictionary<string, int>();

	private static int _tabIndex = 0;

	private static byte _gridColumns = 2;

	private static float _gridCellWidth = 200f;

	private static float _gridCellHeight = 80f;

	private static float _gridSpacing = 6f;

	private static GUIStyle _v5CardStyle;

	private static GUIStyle _v5CardNameStyle;

	private static GUIStyle _v5CardStatusStyle;

	private static GUIStyle _v5ColorBoxStyle;

	private static GUIStyle _v5ActionBtnStyle;

	private static GUIStyle _cachedTabLabelNormal;

	private static GUIStyle _cachedTabLabelSelected;

	private static byte[] _lastVerifiedBytecode1;

	private static byte[] _lastVerifiedBytecode2;

	private static byte[] _lastVerifiedBytecode3;

	private static byte[] _lastVerifiedBytecode4;

	private static bool _lastVerifyResult1;

	private static bool _lastVerifyResult2;

	private static bool _lastVerifyResult3;

	private static bool _lastVerifyResult4;

	private static long _currentExecutionToken = 0L;

	private static byte[] _hmacKeyBuffer = null;

	internal static LayoutBucket CurrentBucket { get; private set; } = LayoutBucket.Standard;


	internal static LayoutBucketV CurrentBucketV { get; private set; } = LayoutBucketV.Tall;


	internal static float CurrentSidebarWidth { get; private set; } = 200f;


	internal static float CurrentWindowWidth
	{
		get
		{
			VMWindowState currentWindow = _currentWindow;
			if (currentWindow == null)
			{
				return 500f;
			}
			return ((Rect)(ref currentWindow.WindowRect)).width;
		}
	}

	internal static float CurrentWindowHeight
	{
		get
		{
			VMWindowState currentWindow = _currentWindow;
			if (currentWindow == null)
			{
				return 600f;
			}
			return ((Rect)(ref currentWindow.WindowRect)).height;
		}
	}

	internal static bool IsActivelyResizing => _currentWindow?.IsResizing ?? false;

	internal static float CurrentContentWidth
	{
		get
		{
			if (_currentWindow == null)
			{
				return 300f;
			}
			return ((Rect)(ref _currentWindow.WindowRect)).width - CurrentSidebarWidth - 1f;
		}
	}

	internal static float CurrentContentHeight
	{
		get
		{
			if (_currentWindow == null)
			{
				return 574f;
			}
			return ((Rect)(ref _currentWindow.WindowRect)).height - 26f;
		}
	}

	internal static float GetButtonHeight()
	{
		return GuiStyles.Spacing.MenuSize(CurrentBucket switch
		{
			LayoutBucket.Micro => 28f, 
			LayoutBucket.Tight => 32f, 
			_ => 42f, 
		}, 20f);
	}

	internal static float GetIconSize()
	{
		return GuiStyles.Spacing.MenuSize(CurrentBucket switch
		{
			LayoutBucket.Micro => 20f, 
			LayoutBucket.Tight => 24f, 
			_ => 32f, 
		}, 16f);
	}

	private static void EnsureResizeStyles()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00d4: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (_resizeDotStyle == null)
		{
			Texture2D background = GuiStyles.MakeTexture(1, 1, Color.white);
			GUIStyle val = new GUIStyle();
			val.normal.background = background;
			_resizeDotStyle = val;
			GUIStyle val2 = new GUIStyle();
			val2.normal.background = background;
			_resizeGlowStyle = val2;
			GUIStyle val3 = new GUIStyle();
			val3.normal.background = background;
			_resizeDimBgStyle = val3;
			_resizeDimStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = GuiStyles.Spacing.MenuFont(10),
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4,
				padding = new RectOffset
				{
					left = 0,
					right = 0,
					top = 0,
					bottom = 0
				},
				margin = new RectOffset
				{
					left = 0,
					right = 0,
					top = 0,
					bottom = 0
				}
			};
			_resizeDimStyle.normal.textColor = Color.white;
		}
	}

	private static void DrawResizeGrip(VMWindowState win)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type == 7)
		{
			EnsureResizeStyles();
			Rect cachedResizeHandle = win.CachedResizeHandle;
			float resizeHoverT = win.ResizeHoverT;
			float resizeActiveT = win.ResizeActiveT;
			Color accent = GuiStyles.Theme.Accent;
			Color color = GUI.color;
			if (resizeHoverT > 0.01f)
			{
				float num = ModMenuCrew.Easing.Easing.EaseOutCubic(resizeHoverT) * 0.15f + resizeActiveT * 0.1f;
				GUI.color = new Color(accent.r, accent.g, accent.b, num * win.FadeAlpha);
				GUI.Box(new Rect(((Rect)(ref cachedResizeHandle)).x + 2f, ((Rect)(ref cachedResizeHandle)).y + 2f, ((Rect)(ref cachedResizeHandle)).width - 2f, ((Rect)(ref cachedResizeHandle)).height - 2f), GUIContent.none, _resizeGlowStyle);
			}
			float num2 = 2f;
			float num3 = 6f;
			float num4 = ((Rect)(ref cachedResizeHandle)).x + 6f;
			float num5 = ((Rect)(ref cachedResizeHandle)).y + 6f;
			float num6 = Mathf.Lerp(0.25f, 0.7f, ModMenuCrew.Easing.Easing.EaseOutCubic(resizeHoverT));
			num6 = Mathf.Lerp(num6, 0.95f, resizeActiveT);
			if (resizeActiveT > 0.01f)
			{
				num6 += Mathf.Sin(Time.realtimeSinceStartup * 6f) * 0.05f * resizeActiveT;
			}
			Color val = new Color(0.6f, 0.6f, 0.65f, num6 * win.FadeAlpha);
			Color val2 = default(Color);
			((Color)(ref val2))._002Ector(accent.r, accent.g, accent.b, num6 * win.FadeAlpha);
			GUI.color = Color.Lerp(val, val2, ModMenuCrew.Easing.Easing.SmoothStep(resizeHoverT));
			GUI.Box(new Rect(num4 + num3 * 2f, num5, num2, num2), GUIContent.none, _resizeDotStyle);
			GUI.Box(new Rect(num4 + num3, num5 + num3, num2, num2), GUIContent.none, _resizeDotStyle);
			GUI.Box(new Rect(num4 + num3 * 2f, num5 + num3, num2, num2), GUIContent.none, _resizeDotStyle);
			GUI.Box(new Rect(num4, num5 + num3 * 2f, num2, num2), GUIContent.none, _resizeDotStyle);
			GUI.Box(new Rect(num4 + num3, num5 + num3 * 2f, num2, num2), GUIContent.none, _resizeDotStyle);
			GUI.Box(new Rect(num4 + num3 * 2f, num5 + num3 * 2f, num2, num2), GUIContent.none, _resizeDotStyle);
			if (resizeActiveT > 0.3f)
			{
				string text = $"{Mathf.RoundToInt(((Rect)(ref win.WindowRect)).width)}x{Mathf.RoundToInt(((Rect)(ref win.WindowRect)).height)}";
				float num7 = 64f;
				float num8 = 18f;
				Rect val3 = new Rect(((Rect)(ref cachedResizeHandle)).x - num7 - 4f, ((Rect)(ref cachedResizeHandle)).yMax - num8, num7, num8);
				float num9 = ModMenuCrew.Easing.Easing.EaseOutCubic(Mathf.Clamp01((resizeActiveT - 0.3f) / 0.7f));
				GUI.color = new Color(0.08f, 0.09f, 0.11f, 0.9f * num9 * win.FadeAlpha);
				GUI.Box(val3, GUIContent.none, _resizeDimBgStyle);
				GUI.color = new Color(1f, 1f, 1f, num9 * win.FadeAlpha);
				GUI.Label(val3, text, _resizeDimStyle);
			}
			GUI.color = color;
		}
	}

	private static void CalculateLayoutBuckets()
	{
		int frameCount = Time.frameCount;
		if (frameCount != _lastBucketFrame)
		{
			_lastBucketFrame = frameCount;
			float menuScale = GuiStyles.Spacing.MenuScale;
			VMWindowState currentWindow = _currentWindow;
			float menuW = ((currentWindow != null) ? ((Rect)(ref currentWindow.WindowRect)).width : 500f);
			VMWindowState currentWindow2 = _currentWindow;
			GuiStyles.Spacing.UpdateMenuScale(menuW, (currentWindow2 != null) ? ((Rect)(ref currentWindow2.WindowRect)).height : 600f);
			bool flag = Mathf.Abs(GuiStyles.Spacing.MenuScale - menuScale) >= 0.03f;
			GuiStyles.InvalidateIfMenuScaleChanged();
			VMWindowState currentWindow3 = _currentWindow;
			float num = ((currentWindow3 != null) ? ((Rect)(ref currentWindow3.WindowRect)).width : 500f);
			float num2 = ((num < 250f) ? 48f : ((num < 400f) ? 80f : ((num < 550f) ? 140f : 200f)));
			float num3 = num - num2 - 1f;
			float currentContentHeight = CurrentContentHeight;
			LayoutBucket layoutBucket = ((!(num3 < 150f - ((CurrentBucket == LayoutBucket.Micro) ? 0f : 20f))) ? ((num3 < 250f - ((CurrentBucket <= LayoutBucket.Tight) ? 0f : 20f)) ? LayoutBucket.Tight : ((!(num3 < 350f - ((CurrentBucket <= LayoutBucket.Compact) ? 0f : 20f))) ? LayoutBucket.Standard : LayoutBucket.Compact)) : LayoutBucket.Micro);
			if (layoutBucket != CurrentBucket || flag)
			{
				_cachedTabLabelNormal = null;
				_cachedTabLabelSelected = null;
				_resizeDotStyle = null;
				_resizeGlowStyle = null;
				_resizeDimStyle = null;
				_resizeDimBgStyle = null;
				_tooltipStyle = null;
				_pp_ColorBoxStyle = null;
				_pp_PlayerNameStyle = null;
				_pp_ImpostorNameStyle = null;
				_pp_StatusStyle = null;
				_pp_RoleButtonStyle = null;
				_pp_PreAssignButtonStyle = null;
				_pp_PreAssignLabelStyle = null;
				_v5CardStyle = null;
				_v5CardNameStyle = null;
				_v5CardStatusStyle = null;
				_v5ColorBoxStyle = null;
				_v5ActionBtnStyle = null;
			}
			CurrentBucket = layoutBucket;
			if (currentContentHeight < 250f)
			{
				CurrentBucketV = LayoutBucketV.Short;
			}
			else if (currentContentHeight < 450f)
			{
				CurrentBucketV = LayoutBucketV.Medium;
			}
			else
			{
				CurrentBucketV = LayoutBucketV.Tall;
			}
		}
	}

	private static string ApplyContext(string input)
	{
		if (_currentContext == null || input == null)
		{
			return input;
		}
		return input.Replace("%NAME%", _currentContext.Name).Replace("%STATUS%", _currentContext.StatusText);
	}

	private static string ApplyContextAction(string actionId)
	{
		if (_currentContext == null || actionId == null)
		{
			return actionId;
		}
		return actionId + "_" + _currentContext.Player.PlayerId;
	}

	private static RSA GetRsaPublicKey()
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_c1);
			stringBuilder.Append(_c2);
			stringBuilder.Append(_c3);
			stringBuilder.Append(_c4);
			stringBuilder.Append(_c5);
			stringBuilder.Append(_c6);
			byte[] modulus = Convert.FromBase64String(stringBuilder.ToString());
			byte[] exponent = Convert.FromBase64String("AQAB");
			RSAParameters rSAParameters = default(RSAParameters);
			rSAParameters.Modulus = modulus;
			rSAParameters.Exponent = exponent;
			RSAParameters parameters = rSAParameters;
			RSA rSA = RSA.Create();
			rSA.ImportParameters(parameters);
			return rSA;
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit(ex.Message));
			return null;
		}
	}

	internal static byte[] GetRsaModulusHash()
	{
		try
		{
			return SHA256.HashData(Convert.FromBase64String(_c1 + _c2 + _c3 + _c4 + _c5 + _c6));
		}
		catch
		{
			return null;
		}
	}

	internal static byte[] RsaEncrypt(byte[] data)
	{
		try
		{
			return GetRsaPublicKey()?.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
		}
		catch
		{
			return null;
		}
	}

	internal static bool VerifyRsaSignature(byte[] data, byte[] signature)
	{
		try
		{
			return GetRsaPublicKey()?.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss) ?? false;
		}
		catch
		{
			return false;
		}
	}

	private static bool VerifyBytecodeSignatureV5(byte[] bytecode)
	{
		try
		{
			RSA rsaPublicKey = GetRsaPublicKey();
			if (rsaPublicKey == null)
			{
				ServerData.TriggerSilentDenial();
				return false;
			}
			byte[] array = new byte[256];
			Array.Copy(bytecode, 4, array, 0, 256);
			int num = bytecode.Length - 260;
			byte[] array2 = new byte[num];
			Array.Copy(bytecode, 260, array2, 0, num);
			return rsaPublicKey.VerifyData(array2, array, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
		}
		catch
		{
			return false;
		}
	}

	private static bool ValidateTimestampAntiReplay(long bytecodeTimestamp)
	{
		long num = GetCachedNowMs() + ModKeyValidator.ServerTimeOffsetMs;
		long num2 = Math.Abs(bytecodeTimestamp - num);
		if (Time.frameCount % 1000 == 0)
		{
			Debug.Log(Object.op_Implicit($"tc:{num2}"));
		}
		if (num2 > 300000)
		{
			if (Time.frameCount % 300 == 0)
			{
				Debug.LogWarning(Object.op_Implicit($"e4:{num2}"));
			}
			return false;
		}
		return true;
	}

	public static bool IsMouseOverAnyWindow()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (_windowStates == null || _windowStates.Count == 0)
		{
			return false;
		}
		Vector3 mousePosition = Input.mousePosition;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(mousePosition.x, (float)Screen.height - mousePosition.y);
		foreach (KeyValuePair<string, VMWindowState> windowState in _windowStates)
		{
			VMWindowState value = windowState.Value;
			if (value != null && !(value.FadeAlpha < 0.1f) && !(((Rect)(ref value.WindowRect)).width < 10f) && !(((Rect)(ref value.WindowRect)).height < 10f) && ((Rect)(ref value.WindowRect)).Contains(val))
			{
				return true;
			}
		}
		return false;
	}

	internal static string ConsumeStagedString(string realId)
	{
		if (string.IsNullOrEmpty(realId))
		{
			return "";
		}
		if (_stagedStrings.TryGetValue(realId, out var value))
		{
			_stagedStrings.Remove(realId);
			return value ?? "";
		}
		return "";
	}

	internal static void ResetTextInputState()
	{
		try
		{
			_textInputBuffers.Clear();
			_textInputLastValue.Clear();
			_textInputLabelCache.Clear();
			_stagedStrings.Clear();
			_textInputFocusedId = null;
		}
		catch
		{
		}
	}

	private static List<string> RentTabIds()
	{
		List<string> obj = ((_tabIdsPool.Count > 0) ? _tabIdsPool.Pop() : new List<string>());
		obj.Clear();
		_tabIdLookup.Clear();
		return obj;
	}

	private static void ReturnTabIds(List<string> list)
	{
		if (list != null)
		{
			list.Clear();
			_tabIdLookup.Clear();
			_tabIdsPool.Push(list);
		}
	}

	private static Stack<Color> RentColorStack()
	{
		Stack<Color> obj = ((_colorStackPool.Count > 0) ? _colorStackPool.Pop() : new Stack<Color>());
		obj.Clear();
		return obj;
	}

	private static void ReturnColorStack(Stack<Color> s)
	{
		if (s != null)
		{
			s.Clear();
			_colorStackPool.Push(s);
		}
	}

	private static Stack<string> RentScrollStack()
	{
		Stack<string> obj = ((_scrollStackPool.Count > 0) ? _scrollStackPool.Pop() : new Stack<string>());
		obj.Clear();
		return obj;
	}

	private static void ReturnScrollStack(Stack<string> s)
	{
		if (s != null)
		{
			s.Clear();
			_scrollStackPool.Push(s);
		}
	}

	private static GUIStyle GetCachedAlertStyle(byte type, Texture2D tex)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		int num = ((type < 4) ? type : 0);
		if (_cachedAlertStyles[num] == null || (Object)(object)_cachedAlertStyles[num].normal.background != (Object)(object)tex)
		{
			_cachedAlertStyles[num] = new GUIStyle();
			_cachedAlertStyles[num].normal.background = tex;
		}
		return _cachedAlertStyles[num];
	}

	internal static void ClearCachedTextures()
	{
		_cachedProgressBgTex = null;
		_cachedProgressBgStyle = null;
		_cachedProgressFillTex = null;
		_cachedProgressFillStyle = null;
		_cachedAlertInfoTex = null;
		_cachedAlertWarnTex = null;
		_cachedAlertErrTex = null;
		_cachedAlertOkTex = null;
		for (int i = 0; i < _cachedAlertStyles.Length; i++)
		{
			_cachedAlertStyles[i] = null;
		}
		_resizeDotStyle = null;
	}

	internal static void InvalidateThemeStyles()
	{
		_cachedTabLabelNormal = null;
		_cachedTabLabelSelected = null;
		_resizeDotStyle = null;
		_resizeGlowStyle = null;
		_resizeDimStyle = null;
		_resizeDimBgStyle = null;
		_tooltipStyle = null;
		_pp_ColorBoxStyle = null;
		_pp_PlayerNameStyle = null;
		_pp_ImpostorNameStyle = null;
		_pp_StatusStyle = null;
		_pp_RoleButtonStyle = null;
		_pp_PreAssignButtonStyle = null;
		_pp_PreAssignLabelStyle = null;
		_v5CardStyle = null;
		_v5CardNameStyle = null;
		_v5CardStatusStyle = null;
		_v5ColorBoxStyle = null;
		_v5ActionBtnStyle = null;
		_cachedProgressBgTex = null;
		_cachedProgressBgStyle = null;
		_cachedProgressFillTex = null;
		_cachedProgressFillStyle = null;
	}

	internal static long GetCachedNowMs()
	{
		int frameCount = Time.frameCount;
		if (frameCount != _cachedFrameNowMsFrame)
		{
			_cachedFrameNowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			_cachedFrameNowMsFrame = frameCount;
		}
		return _cachedFrameNowMs;
	}

	internal static void UpdateAnimations()
	{
		try
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (_windowStates == null)
			{
				return;
			}
			foreach (VMWindowState value in _windowStates.Values)
			{
				if (value == null)
				{
					continue;
				}
				if (value.FadeStartTime < 0f)
				{
					value.FadeStartTime = realtimeSinceStartup;
				}
				if (!value.FadeComplete)
				{
					float num = realtimeSinceStartup - value.FadeStartTime;
					value.FadeAlpha = Mathf.Clamp01(num / 0.25f);
					if (value.FadeAlpha >= 1f)
					{
						value.FadeComplete = true;
					}
				}
				value.CachedPulse = 0.3f + Mathf.Sin(realtimeSinceStartup * 2f) * 0.08f;
				if (!value.ResizeHintShown && value.ResizeHintStart < 0f)
				{
					value.ResizeHintStart = realtimeSinceStartup;
				}
				if (value.ResizeHintStart > 0f && realtimeSinceStartup - value.ResizeHintStart > 5f)
				{
					value.ResizeHintShown = true;
				}
				float num2 = ((value.LastAnimTime > 0f) ? (realtimeSinceStartup - value.LastAnimTime) : 0.016f);
				value.LastAnimTime = realtimeSinceStartup;
				float num3 = 10f * num2;
				float num4 = ((value.IsResizing || value.ResizeHovered) ? 1f : 0f);
				float num5 = (value.IsResizing ? 1f : 0f);
				value.ResizeHoverT = Mathf.MoveTowards(value.ResizeHoverT, num4, num3);
				value.ResizeActiveT = Mathf.MoveTowards(value.ResizeActiveT, num5, num3);
			}
		}
		catch
		{
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void UpdateRenderContext()
	{
		UpdateRenderContext(ModKeyValidator.CurrentSessionToken);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void UpdateRenderContext(long sessionToken)
	{
		int frameCount = Time.frameCount;
		if (frameCount != _lastContextFrame || sessionToken != _lastContextToken)
		{
			_lastContextFrame = frameCount;
			_lastContextToken = sessionToken;
			long num = (frameCount ^ sessionToken ^ _contextSalt) * 1099511628211L;
			long num2 = num ^ (num >> 32);
			_renderContextIdXor = num2 ^ (num2 >> 16) ^ 0x3A7D5E8B4C2F1069L;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool CheckToken(long token)
	{
		if (token != (_renderContextIdXor ^ 0x3A7D5E8B4C2F1069L))
		{
			Debug.LogError(Object.op_Implicit("x"));
			ServerData.TriggerSilentDenial();
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static long GetRenderContext()
	{
		return _renderContextIdXor ^ 0x3A7D5E8B4C2F1069L;
	}

	internal static void SetWindowSize(float width, float height)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (_windowStates.TryGetValue("main_menu", out var value))
		{
			Rect windowRect = value.WindowRect;
			float num = Mathf.Clamp(width, value.MinWidth, value.MaxWidth);
			float num2 = Mathf.Clamp(height, value.MinHeight, value.MaxHeight);
			value.WindowRect = new Rect(((Rect)(ref windowRect)).x, ((Rect)(ref windowRect)).y, num, num2);
		}
		foreach (KeyValuePair<string, VMWindowState> windowState in _windowStates)
		{
			Rect windowRect2 = windowState.Value.WindowRect;
			float num3 = Mathf.Clamp(width, windowState.Value.MinWidth, windowState.Value.MaxWidth);
			float num4 = Mathf.Clamp(height, windowState.Value.MinHeight, windowState.Value.MaxHeight);
			windowState.Value.WindowRect = new Rect(((Rect)(ref windowRect2)).x, ((Rect)(ref windowRect2)).y, num3, num4);
		}
	}

	internal static void CenterWindow()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		float scale = GuiStyles.Spacing.Scale;
		float num = (float)Screen.width / scale;
		float num2 = (float)Screen.height / scale;
		foreach (KeyValuePair<string, VMWindowState> windowState in _windowStates)
		{
			Rect windowRect = windowState.Value.WindowRect;
			float num3 = Mathf.Max(0f, (num - ((Rect)(ref windowRect)).width) / 2f);
			float num4 = Mathf.Max(0f, (num2 - ((Rect)(ref windowRect)).height) / 2f);
			windowState.Value.WindowRect = new Rect(num3, num4, ((Rect)(ref windowRect)).width, ((Rect)(ref windowRect)).height);
		}
	}

	internal static void Execute(byte[] bytecode, long expectedToken, Dictionary<string, Action<long>> actions, byte[] inverseMap)
	{
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Invalid comparison between Unknown and I4
		if (!IntegrityGuard.IsIntact)
		{
			ServerData.TriggerSilentDenial();
			return;
		}
		if (actions != null)
		{
			ActionPermitSystem.SetActionRegistry(actions);
		}
		if (_pendingSliderCommit.Count > 0)
		{
			float unscaledTime = Time.unscaledTime;
			bool flag = GUIUtility.hotControl == 0;
			List<string> list = null;
			List<string> list2 = null;
			foreach (KeyValuePair<string, float> item in _pendingSliderCommit)
			{
				float num = unscaledTime - item.Value;
				if (num >= 5f)
				{
					(list2 ?? (list2 = new List<string>())).Add(item.Key);
				}
				else if (flag || num >= 0.15f)
				{
					(list ?? (list = new List<string>())).Add(item.Key);
				}
			}
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					string text = list[i];
					_pendingSliderCommit.Remove(text);
					ActionPermitSystem.RequestExecution(text, "", GetRenderContext());
				}
			}
			if (list2 != null)
			{
				for (int j = 0; j < list2.Count; j++)
				{
					_pendingSliderCommit.Remove(list2[j]);
				}
			}
		}
		if (bytecode == null || bytecode.Length < 537)
		{
			ServerData.TriggerSilentDenial();
			return;
		}
		if (bytecode[0] != 80 || bytecode[1] != 79 || bytecode[2] != 76 || bytecode[3] != 53)
		{
			ServerData.TriggerSilentDenial();
			return;
		}
		bool flag2;
		if (bytecode == _lastVerifiedBytecode1)
		{
			flag2 = _lastVerifyResult1;
		}
		else if (bytecode == _lastVerifiedBytecode2)
		{
			flag2 = _lastVerifyResult2;
		}
		else if (bytecode == _lastVerifiedBytecode3)
		{
			flag2 = _lastVerifyResult3;
		}
		else if (bytecode == _lastVerifiedBytecode4)
		{
			flag2 = _lastVerifyResult4;
		}
		else
		{
			flag2 = VerifyBytecodeSignatureV5(bytecode);
			_lastVerifiedBytecode4 = _lastVerifiedBytecode3;
			_lastVerifyResult4 = _lastVerifyResult3;
			_lastVerifiedBytecode3 = _lastVerifiedBytecode2;
			_lastVerifyResult3 = _lastVerifyResult2;
			_lastVerifiedBytecode2 = _lastVerifiedBytecode1;
			_lastVerifyResult2 = _lastVerifyResult1;
			_lastVerifiedBytecode1 = bytecode;
			_lastVerifyResult1 = flag2;
		}
		if (!flag2)
		{
			ServerData.TriggerSilentDenial();
			return;
		}
		long num2 = BitConverter.ToInt64(bytecode, 528);
		if (!ValidateTimestampAntiReplay(num2))
		{
			if (Time.frameCount % 300 == 0)
			{
				DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				_ = ModKeyValidator.ServerTimeOffsetMs;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (!(realtimeSinceStartup - _lastSelfHealTime > 10f))
			{
				return;
			}
			_lastSelfHealTime = realtimeSinceStartup;
			try
			{
				ModKeyValidator.ForceHeartbeatWakeup();
				ActionPermitSystem.EnqueueMainThread(delegate
				{
					try
					{
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					}
					catch
					{
					}
				});
				if (Math.Abs(num2 - (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ModKeyValidator.ServerTimeOffsetMs)) > 120000 && RealtimeConnection.IsConnected)
				{
					RealtimeConnection.ForceReconnect();
				}
				return;
			}
			catch
			{
				return;
			}
		}
		if (BitConverter.ToInt64(bytecode, 260) != expectedToken)
		{
			_tokenMismatchFrameCount++;
			if (_tokenMismatchFrameCount >= 60)
			{
				ServerData.TriggerSilentDenial();
			}
			return;
		}
		_tokenMismatchFrameCount = 0;
		if (inverseMap == null || inverseMap.Length != 256)
		{
			ServerData.TriggerSilentDenial();
			return;
		}
		bool flag3;
		if (inverseMap == _lastIdentityCheckedMap)
		{
			flag3 = _lastIdentityCheckResult;
		}
		else
		{
			int num3 = 0;
			for (int k = 0; k < 256; k++)
			{
				if (inverseMap[k] == (byte)k)
				{
					num3++;
				}
			}
			flag3 = num3 <= 32;
			_lastIdentityCheckedMap = inverseMap;
			_lastIdentityCheckResult = flag3;
		}
		if (!flag3)
		{
			ServerData.TriggerSilentDenial();
			return;
		}
		Dictionary<string, Action<long>> actionRegistry = _actionRegistry;
		List<string> tabIds = _tabIds;
		int tabIndex = _tabIndex;
		long currentExecutionToken = _currentExecutionToken;
		Stack<Color> colorStack = _colorStack;
		Stack<string> scrollStack = _scrollStack;
		VMWindowState currentWindow = _currentWindow;
		CachedPlayerData currentContext = _currentContext;
		byte gridColumns = _gridColumns;
		float gridCellWidth = _gridCellWidth;
		float gridCellHeight = _gridCellHeight;
		float gridSpacing = _gridSpacing;
		bool isWindowMinimized = _isWindowMinimized;
		try
		{
			_currentExecutionToken = expectedToken;
			_actionRegistry = actions;
			_tabIds = RentTabIds();
			_tabIndex = 0;
			_colorStack = RentColorStack();
			_scrollStack = RentScrollStack();
			if (Event.current != null && (int)Event.current.type == 7)
			{
				_hoveredTabLastFrame = _hoveredTabThisFrame;
				_hoveredTabThisFrame = -1;
			}
			GuiStyles.EnsureInitialized();
			int num4 = 536;
			int count = bytecode.Length - num4;
			using MemoryStream scrambledStream = new MemoryStream(bytecode, num4, count, writable: false);
			using SecureStreamReader reader = new SecureStreamReader(scrambledStream, inverseMap);
			try
			{
				ExecuteStream(reader, -1L);
			}
			catch (Exception ex)
			{
				if (ex.GetType().Name.Contains("ExitGUI"))
				{
					throw;
				}
				Debug.LogWarning(Object.op_Implicit(ex.Message));
			}
		}
		finally
		{
			ReturnTabIds(_tabIds);
			ReturnColorStack(_colorStack);
			ReturnScrollStack(_scrollStack);
			_actionRegistry = actionRegistry;
			_tabIds = tabIds;
			_tabIndex = tabIndex;
			_currentExecutionToken = currentExecutionToken;
			_colorStack = colorStack;
			_scrollStack = scrollStack;
			_currentWindow = currentWindow;
			_currentContext = currentContext;
			_gridColumns = gridColumns;
			_gridCellWidth = gridCellWidth;
			_gridCellHeight = gridCellHeight;
			_gridSpacing = gridSpacing;
			_isWindowMinimized = isWindowMinimized;
		}
	}

	private static void ExecuteWinBegin(SecureStreamReader reader)
	{
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Invalid comparison between Unknown and I4
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Invalid comparison between Unknown and I4
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Invalid comparison between Unknown and I4
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b6: Unknown result type (might be due to invalid IL or missing references)
		string text = reader.ReadString();
		float num = reader.ReadSingle();
		float num2 = reader.ReadSingle();
		long currentExecutionToken = _currentExecutionToken;
		int num3 = (int)(currentExecutionToken & 0xFFFFFFFFu);
		int num4 = (int)((currentExecutionToken >> 32) & 0xFFFFFFFFu);
		float num5 = num - (float)(num3 % 1000);
		float num6 = num2 - (float)(num4 % 1000);
		float num7 = reader.ReadSingle();
		float num8 = reader.ReadSingle();
		string text2 = reader.ReadString();
		bool num9 = num5 >= -5000f && num5 <= 5000f && num6 >= -5000f && num6 <= 5000f;
		if (!_windowStates.ContainsKey(text))
		{
			_windowStates[text] = new VMWindowState(text);
		}
		_currentWindow = _windowStates[text];
		if (!num9)
		{
			_currentWindow.WindowRect = new Rect(99999f, 99999f, 1f, 1f);
			_currentWindow.FadeAlpha = 0f;
			_currentWindow.FadeComplete = false;
			_currentWindow.FadeStartTime = -1f;
			return;
		}
		if (((Rect)(ref _currentWindow.WindowRect)).width == 0f || ((Rect)(ref _currentWindow.WindowRect)).height == 0f)
		{
			_currentWindow.WindowRect = new Rect(num5, num6, num7, num8);
		}
		Color color = GUI.color;
		float num10 = 1f;
		try
		{
			num10 = CheatConfig.MenuOpacity?.Value ?? 1f;
		}
		catch
		{
		}
		GUI.color = new Color(color.r, color.g, color.b, _currentWindow.FadeAlpha * num10);
		float num11 = 26f;
		Rect val = (Rect)(_currentWindow.IsMinimized ? new Rect(((Rect)(ref _currentWindow.WindowRect)).x, ((Rect)(ref _currentWindow.WindowRect)).y, ((Rect)(ref _currentWindow.WindowRect)).width, num11) : _currentWindow.WindowRect);
		if ((int)Event.current.type == 7)
		{
			Color accent = GuiStyles.Theme.Accent;
			Color color2 = GUI.color;
			GUI.color = new Color(accent.r, accent.g, accent.b, _currentWindow.CachedPulse * _currentWindow.FadeAlpha * 0.6f);
			float num12 = 2f;
			GUI.Box(new Rect(((Rect)(ref val)).x - num12, ((Rect)(ref val)).y - num12, ((Rect)(ref val)).width + num12 * 2f, ((Rect)(ref val)).height + num12 * 2f), GUIContent.none, GuiStyles.ShadowStyle);
			GUI.color = color2;
			GUI.Box(val, GUIContent.none, GuiStyles.WindowStyle);
			if (!_currentWindow.IsMinimized && (int)Event.current.type == 7)
			{
				GuiStyles.DrawAnimatedBackground(val);
				GuiStyles.DrawAnimatedScanlines(val);
			}
		}
		_currentWindow.CachedHeaderRect = new Rect(((Rect)(ref _currentWindow.WindowRect)).x, ((Rect)(ref _currentWindow.WindowRect)).y, ((Rect)(ref _currentWindow.WindowRect)).width, num11);
		_currentWindow.CachedContentRect = new Rect(((Rect)(ref _currentWindow.WindowRect)).x, ((Rect)(ref _currentWindow.WindowRect)).y + num11, ((Rect)(ref _currentWindow.WindowRect)).width, ((Rect)(ref _currentWindow.WindowRect)).height - num11);
		_currentWindow.CachedResizeHandle = new Rect(((Rect)(ref _currentWindow.WindowRect)).xMax - 24f, ((Rect)(ref _currentWindow.WindowRect)).yMax - 24f, 24f, 24f);
		CalculateLayoutBuckets();
		if ((int)Event.current.type == 7)
		{
			GUI.Box(_currentWindow.CachedHeaderRect, GUIContent.none, GuiStyles.HeaderBackgroundStyle);
		}
		float num13 = Mathf.Max(60f, ((Rect)(ref _currentWindow.WindowRect)).width - 60f);
		GUI.Label(new Rect(((Rect)(ref _currentWindow.CachedHeaderRect)).x + 10f, ((Rect)(ref _currentWindow.CachedHeaderRect)).y, num13, num11), text2, GuiStyles.TitleLabelStyle);
		float num14 = 20f;
		float num15 = 18f;
		float num16 = ((Rect)(ref _currentWindow.CachedHeaderRect)).y + 4f;
		Rect val2 = default(Rect);
		((Rect)(ref val2))._002Ector(((Rect)(ref _currentWindow.WindowRect)).xMax - num14 - 8f, num16, num14, num15);
		if (GUI.Button(new Rect(((Rect)(ref val2)).x - num14 - 4f, num16, num14, num15), _currentWindow.IsMinimized ? "▭" : "—", GuiStyles.TitleBarButtonStyle))
		{
			_currentWindow.IsMinimized = !_currentWindow.IsMinimized;
		}
		if (GUI.Button(val2, "✕", GuiStyles.TitleBarButtonStyle))
		{
			_currentWindow.FadeComplete = false;
			_currentWindow.FadeStartTime = -1f;
			_currentWindow.FadeAlpha = 0f;
		}
		_isWindowMinimized = _currentWindow.IsMinimized;
		if (!_currentWindow.IsMinimized)
		{
			_savedGuiColor = GUI.color;
			float slideOffset = GuiStyles.TabTransition.GetSlideOffset();
			float alpha = GuiStyles.TabTransition.GetAlpha();
			Rect cachedContentRect = _currentWindow.CachedContentRect;
			if (Mathf.Abs(slideOffset) > 0.1f)
			{
				((Rect)(ref cachedContentRect))._002Ector(((Rect)(ref cachedContentRect)).x + slideOffset, ((Rect)(ref cachedContentRect)).y, ((Rect)(ref cachedContentRect)).width, ((Rect)(ref cachedContentRect)).height);
			}
			GUILayout.BeginArea(cachedContentRect);
			if (alpha < 1f)
			{
				GUI.color = new Color(_savedGuiColor.r, _savedGuiColor.g, _savedGuiColor.b, _savedGuiColor.a * alpha);
			}
		}
	}

	private static void ExecuteWinEnd()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Invalid comparison between Unknown and I4
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Invalid comparison between Unknown and I4
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Invalid comparison between Unknown and I4
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Invalid comparison between Unknown and I4
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Invalid comparison between Unknown and I4
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		if (_currentWindow == null)
		{
			return;
		}
		if (!_currentWindow.IsMinimized)
		{
			GUILayout.EndArea();
			GUI.color = _savedGuiColor;
		}
		_isWindowMinimized = false;
		if (!_currentWindow.IsMinimized)
		{
			DrawResizeGrip(_currentWindow);
		}
		Event current = Event.current;
		if (!_currentWindow.IsResizing)
		{
			if ((int)current.type == 0 && ((Rect)(ref _currentWindow.CachedHeaderRect)).Contains(current.mousePosition))
			{
				_currentWindow.IsDragging = true;
				_currentWindow.DragOffset = current.mousePosition - ((Rect)(ref _currentWindow.WindowRect)).position;
				current.Use();
			}
			else if ((int)current.type == 3 && _currentWindow.IsDragging)
			{
				Vector2 val = current.mousePosition - _currentWindow.DragOffset;
				((Rect)(ref _currentWindow.WindowRect)).x = val.x;
				((Rect)(ref _currentWindow.WindowRect)).y = val.y;
				float num = (float)Screen.width / GuiStyles.Spacing.Scale;
				float num2 = (float)Screen.height / GuiStyles.Spacing.Scale;
				if (((Rect)(ref _currentWindow.WindowRect)).x < 15f)
				{
					((Rect)(ref _currentWindow.WindowRect)).x = 0f;
				}
				if (((Rect)(ref _currentWindow.WindowRect)).y < 15f)
				{
					((Rect)(ref _currentWindow.WindowRect)).y = 0f;
				}
				if (num - ((Rect)(ref _currentWindow.WindowRect)).xMax < 15f)
				{
					((Rect)(ref _currentWindow.WindowRect)).x = num - ((Rect)(ref _currentWindow.WindowRect)).width;
				}
				if (num2 - ((Rect)(ref _currentWindow.WindowRect)).yMax < 15f)
				{
					((Rect)(ref _currentWindow.WindowRect)).y = num2 - ((Rect)(ref _currentWindow.WindowRect)).height;
				}
				current.Use();
			}
			else if ((int)current.type == 1 && _currentWindow.IsDragging)
			{
				_currentWindow.IsDragging = false;
				_currentWindow.SavePosition();
			}
		}
		if (!_currentWindow.IsMinimized)
		{
			Rect val2 = default(Rect);
			((Rect)(ref val2))._002Ector(((Rect)(ref _currentWindow.CachedResizeHandle)).x - 4f, ((Rect)(ref _currentWindow.CachedResizeHandle)).y - 4f, 32f, 32f);
			_currentWindow.ResizeHovered = ((Rect)(ref val2)).Contains(current.mousePosition);
			if ((int)current.type == 0 && _currentWindow.ResizeHovered)
			{
				_currentWindow.IsResizing = true;
				_currentWindow.ResizeStartMouse = current.mousePosition;
				_currentWindow.ResizeStartRect = _currentWindow.WindowRect;
				_currentWindow.ResizeHintShown = true;
				current.Use();
			}
			else if ((int)current.type == 3 && _currentWindow.IsResizing)
			{
				Vector2 val3 = current.mousePosition - _currentWindow.ResizeStartMouse;
				((Rect)(ref _currentWindow.WindowRect)).width = Mathf.Clamp(((Rect)(ref _currentWindow.ResizeStartRect)).width + val3.x, _currentWindow.MinWidth, _currentWindow.MaxWidth);
				((Rect)(ref _currentWindow.WindowRect)).height = Mathf.Clamp(((Rect)(ref _currentWindow.ResizeStartRect)).height + val3.y, _currentWindow.MinHeight, _currentWindow.MaxHeight);
				current.Use();
			}
			else if ((int)current.type == 1 && _currentWindow.IsResizing)
			{
				_currentWindow.IsResizing = false;
				_currentWindow.SavePosition();
				try
				{
					PlayerPickMenu.TriggerRealtimeUpdate(force: true);
				}
				catch
				{
				}
			}
		}
		float num3 = (float)Screen.width / GuiStyles.Spacing.Scale;
		float num4 = (float)Screen.height / GuiStyles.Spacing.Scale;
		float num5 = Mathf.Max(80f, ((Rect)(ref _currentWindow.WindowRect)).width * 0.5f);
		float num6 = 40f;
		((Rect)(ref _currentWindow.WindowRect)).x = Mathf.Clamp(((Rect)(ref _currentWindow.WindowRect)).x, 0f - (((Rect)(ref _currentWindow.WindowRect)).width - num5), Mathf.Max(0f, num3 - num5));
		((Rect)(ref _currentWindow.WindowRect)).y = Mathf.Clamp(((Rect)(ref _currentWindow.WindowRect)).y, 0f, Mathf.Max(0f, num4 - num6));
		if (!_currentWindow.IsMinimized && (int)Event.current.type == 7)
		{
			GuiStyles.VisualEffects.UpdateAndDraw(_currentWindow.WindowRect);
		}
		GuiStyles.DrawPendingEliteTooltip();
		_currentWindow = null;
	}

	private static void ExecuteSectionBegin(SecureStreamReader reader)
	{
		string id = reader.ReadString();
		string text = reader.ReadString();
		GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(GuiStyles.HeaderBackgroundStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedHeight(GuiStyles.Spacing.MenuSize(26f, 18f)) });
		GUILayout.Label(text, GuiStyles.TitleLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
		bool flag = _currentWindow?.IsSectionExpanded(id) ?? true;
		if (GUILayout.Button(flag ? "[-]" : "[+]", GuiStyles.TitleBarButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(30f, 20f)),
			GuiStyles.CachedHeight(GuiStyles.Spacing.MenuSize(20f, 14f))
		}))
		{
			_currentWindow?.ToggleSection(id);
		}
		GUILayout.EndHorizontal();
		if (flag)
		{
			GUILayout.BeginVertical(GuiStyles.HighlightStyle, Array.Empty<GUILayoutOption>());
		}
		if (_currentWindow != null)
		{
			_scrollStack.Push(flag ? "1" : "0");
		}
	}

	private static void ExecuteSectionEnd()
	{
		bool flag = true;
		if (_scrollStack.Count > 0)
		{
			flag = _scrollStack.Pop() == "1";
		}
		if (flag)
		{
			GUILayout.EndVertical();
		}
		GUILayout.EndVertical();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(8f, 4f));
	}

	private static void ExecuteTabButton(SecureStreamReader reader)
	{
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Invalid comparison between Unknown and I4
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Invalid comparison between Unknown and I4
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Expected O, but got Unknown
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Expected O, but got Unknown
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		string text = reader.ReadString();
		string text2 = reader.ReadString();
		reader.ReadString();
		int count = _tabIds.Count;
		_tabIds.Add(text);
		_tabIdLookup[text] = count;
		bool flag = _currentWindow != null && _currentWindow.SelectedTab == count;
		GUIStyle obj = (flag ? GuiStyles.SidebarButtonActiveStyle : GuiStyles.SidebarButtonStyle);
		float buttonHeight = GetButtonHeight();
		float iconSize = GetIconSize();
		bool flag2 = CurrentBucket == LayoutBucket.Micro;
		float num = GuiStyles.Spacing.MenuSize(flag2 ? 4f : 12f, 2f);
		float num2 = (flag2 ? 0f : GuiStyles.Spacing.MenuSize(8f, 4f));
		GUILayout.BeginHorizontal(obj, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedHeight(buttonHeight),
			GuiStyles.CachedExpandWidth
		});
		Texture2D val = null;
		try
		{
			val = IconLoader.GetIcon(text) ?? IconLoader.GetIconForTab(text2);
		}
		catch
		{
		}
		if ((Object)(object)val != (Object)null)
		{
			GUILayout.Space(num);
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GuiStyles.CachedWidth(iconSize),
				GuiStyles.CachedHeight(buttonHeight)
			});
			GUILayout.FlexibleSpace();
			Rect rect = GUILayoutUtility.GetRect(iconSize, iconSize, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GuiStyles.CachedWidth(iconSize),
				GuiStyles.CachedHeight(iconSize)
			});
			if ((int)Event.current.type == 7)
			{
				bool hovered = _hoveredTabLastFrame == count;
				DrawTabIconNeverlose(rect, val, flag, hovered);
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			if (!flag2)
			{
				GUILayout.Space(num2);
			}
		}
		else
		{
			GUILayout.Space(num);
		}
		if (!flag2)
		{
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GuiStyles.CachedExpandWidth,
				GuiStyles.CachedHeight(buttonHeight)
			});
			GUILayout.FlexibleSpace();
			if (_cachedTabLabelNormal == null)
			{
				_cachedTabLabelNormal = new GUIStyle(GUI.skin.label)
				{
					fontSize = GuiStyles.Spacing.MenuFont((CurrentBucket == LayoutBucket.Tight) ? 11 : 13),
					fontStyle = (FontStyle)0,
					alignment = (TextAnchor)3,
					wordWrap = false,
					clipping = (TextClipping)1
				};
				_cachedTabLabelNormal.normal.textColor = GuiStyles.Theme.TextMuted;
			}
			if (_cachedTabLabelSelected == null)
			{
				_cachedTabLabelSelected = new GUIStyle(GUI.skin.label)
				{
					fontSize = GuiStyles.Spacing.MenuFont((CurrentBucket == LayoutBucket.Tight) ? 11 : 13),
					fontStyle = (FontStyle)1,
					alignment = (TextAnchor)3,
					wordWrap = false,
					clipping = (TextClipping)1
				};
				_cachedTabLabelSelected.normal.textColor = GuiStyles.Theme.TextPrimary;
			}
			GUILayout.Label(text2, flag ? _cachedTabLabelSelected : _cachedTabLabelNormal, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
		Rect lastRect = GUILayoutUtility.GetLastRect();
		((Rect)(ref lastRect)).x = 0f;
		((Rect)(ref lastRect)).width = CurrentSidebarWidth;
		if ((int)Event.current.type == 7)
		{
			if (flag)
			{
				GuiStyles.SidebarIndicator.SetTarget(((Rect)(ref lastRect)).y, ((Rect)(ref lastRect)).height);
			}
			else if (((Rect)(ref lastRect)).Contains(Event.current.mousePosition))
			{
				GuiStyles.DrawMultiLayerBloom(lastRect, 0.25f);
				_hoveredTabThisFrame = count;
			}
		}
		if (GUI.Button(lastRect, GUIContent.none, GUIStyle.none) && _currentWindow != null && _currentWindow.SelectedTab != count)
		{
			GuiStyles.TabTransition.NotifyTabSwitch(_currentWindow.SelectedTab, count);
			_currentWindow.SelectedTab = count;
		}
	}

	private static void DrawTabIconNeverlose(Rect iconRect, Texture2D icon, bool selected, bool hovered)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)icon == (Object)null))
		{
			Color color = GUI.color;
			if (selected)
			{
				Color accent = GuiStyles.Theme.Accent;
				Rect val = new Rect(((Rect)(ref iconRect)).x - 3f, ((Rect)(ref iconRect)).y - 3f, ((Rect)(ref iconRect)).width + 6f, ((Rect)(ref iconRect)).height + 6f);
				GUI.color = new Color(accent.r, accent.g, accent.b, 0.15f);
				GUI.Label(val, (Texture)(object)icon, GUIStyle.none);
				Rect val2 = new Rect(((Rect)(ref iconRect)).x - 1f, ((Rect)(ref iconRect)).y - 1f, ((Rect)(ref iconRect)).width + 2f, ((Rect)(ref iconRect)).height + 2f);
				GUI.color = new Color(accent.r, accent.g, accent.b, 0.35f);
				GUI.Label(val2, (Texture)(object)icon, GUIStyle.none);
				GUI.color = accent;
				GUI.Label(iconRect, (Texture)(object)icon, GUIStyle.none);
			}
			else if (hovered)
			{
				GUI.color = new Color(0.92f, 0.94f, 0.98f, 1f);
				GUI.Label(iconRect, (Texture)(object)icon, GUIStyle.none);
			}
			else
			{
				GUI.color = new Color(0.55f, 0.57f, 0.62f, 1f);
				GUI.Label(iconRect, (Texture)(object)icon, GUIStyle.none);
			}
			GUI.color = color;
		}
	}

	private static void ExecuteLicenseBadge()
	{
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.FlexibleSpace();
		bool flag = ModKeyValidator.IsPremium && ModKeyValidator.V();
		bool flag2 = CurrentBucket == LayoutBucket.Micro;
		string obj = ((!flag2) ? (flag ? "PREMIUM" : "FREE USER") : (flag ? "PRO" : "FREE"));
		float w = (flag2 ? 40f : 100f);
		GUILayout.Label(obj, GuiStyles.StatusPillStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w) });
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	private static void ExecuteHeaderDraw()
	{
	}

	private static void ExecuteResizeHandle()
	{
	}

	private static void ExecutePlayerList()
	{
		if (!ServerData.IsLoaded)
		{
			return;
		}
		if ((Object)(object)AmongUsClient.Instance == (Object)null || PlayerControl.AllPlayerControls == null)
		{
			GUILayout.Label("Waiting for game...", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			return;
		}
		if (_pp_ColorBoxStyle == null)
		{
			InitPlayerListStyles();
		}
		if (Time.frameCount - _pp_LastRefreshFrame >= 30)
		{
			RefreshPlayerCache();
		}
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.Label($"Players ({_pp_ActiveList.Count})", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(8f, 4f));
		bool flag = false;
		bool amHost = ((InnerNetClient)AmongUsClient.Instance).AmHost;
		bool isLobby = (Object)(object)ShipStatus.Instance == (Object)null;
		for (int i = 0; i < _pp_ActiveList.Count; i++)
		{
			CachedPlayerData cachedPlayerData = _pp_ActiveList[i];
			if (cachedPlayerData != null && !((Object)(object)cachedPlayerData.Player == (Object)null) && !cachedPlayerData.Disconnected)
			{
				DrawPlayerEntryNative(cachedPlayerData, amHost, isLobby);
				flag = true;
			}
		}
		if (!flag)
		{
			GUILayout.Label("No players to display", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
	}

	private static void InitPlayerListStyles()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Expected O, but got Unknown
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Expected O, but got Unknown
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Expected O, but got Unknown
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		_pp_ColorBoxStyle = new GUIStyle(GUI.skin.box)
		{
			fixedWidth = GuiStyles.Spacing.MenuSize(20f, 14f),
			fixedHeight = GuiStyles.Spacing.MenuSize(20f, 14f),
			margin = CreateRectOffset(GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4)),
			padding = _pp_Margin0
		};
		_pp_ColorBoxStyle.normal.background = Texture2D.whiteTexture;
		_pp_PlayerNameStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = GuiStyles.Spacing.MenuFont(13),
			alignment = (TextAnchor)3,
			padding = CreateRectOffset(GuiStyles.Spacing.MenuPad(4), 0, GuiStyles.Spacing.MenuPad(4), 0),
			wordWrap = false,
			clipping = (TextClipping)1
		};
		_pp_PlayerNameStyle.normal.textColor = Color.white;
		_pp_ImpostorNameStyle = new GUIStyle(_pp_PlayerNameStyle)
		{
			fontStyle = (FontStyle)1,
			padding = CreateRectOffset(0, 0, GuiStyles.Spacing.MenuPad(4), 0)
		};
		_pp_StatusStyle = new GUIStyle(_pp_PlayerNameStyle)
		{
			fontSize = GuiStyles.Spacing.MenuFont(10),
			padding = CreateRectOffset(0, GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4))
		};
		_pp_StatusStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
		_pp_RoleButtonStyle = new GUIStyle(GuiStyles.ButtonStyle)
		{
			fontSize = GuiStyles.Spacing.MenuFont(14),
			padding = CreateRectOffset(GuiStyles.Spacing.MenuPad(8), GuiStyles.Spacing.MenuPad(8), GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4)),
			margin = CreateRectOffset(GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4)),
			alignment = (TextAnchor)4
		};
		_pp_PreAssignButtonStyle = new GUIStyle(GuiStyles.ButtonStyle)
		{
			fontSize = GuiStyles.Spacing.MenuFont(12)
		};
		_pp_PreAssignLabelStyle = new GUIStyle(GUI.skin.label)
		{
			alignment = (TextAnchor)3,
			fontStyle = (FontStyle)1,
			fontSize = GuiStyles.Spacing.MenuFont(12),
			wordWrap = false,
			clipping = (TextClipping)1
		};
		_pp_PreAssignLabelStyle.normal.textColor = GuiStyles.Theme.TextPrimary;
	}

	private static void RefreshPlayerCache()
	{
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		_pp_LastRefreshFrame = (uint)Time.frameCount;
		float unscaledTime = Time.unscaledTime;
		for (int i = 0; i < _pp_DataPool.Count; i++)
		{
			_pp_DataPool[i].Active = false;
		}
		_pp_ActiveList.Clear();
		_pp_CurrentIdsCache.Clear();
		int num = 0;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current == (Object)null || (Object)(object)current.Data == (Object)null || current.Data.Disconnected)
			{
				continue;
			}
			_pp_CurrentIdsCache.Add(current.PlayerId);
			CachedPlayerData cachedPlayerData;
			if (num < _pp_DataPool.Count)
			{
				cachedPlayerData = _pp_DataPool[num];
			}
			else
			{
				cachedPlayerData = new CachedPlayerData();
				_pp_DataPool.Add(cachedPlayerData);
			}
			num++;
			cachedPlayerData.Active = true;
			cachedPlayerData.Player = current;
			cachedPlayerData.Disconnected = false;
			cachedPlayerData.IsDead = current.Data.IsDead;
			CachedPlayerData cachedPlayerData2 = cachedPlayerData;
			RoleBehaviour role = current.Data.Role;
			cachedPlayerData2.IsImpostor = role != null && role.IsImpostor;
			cachedPlayerData.Name = current.Data.PlayerName;
			Color val = Palette.CrewmateBlue;
			PlayerOutfit defaultOutfit = current.Data.DefaultOutfit;
			int num2 = ((defaultOutfit != null) ? defaultOutfit.ColorId : (-1));
			if (num2 >= 0 && num2 < ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length)
			{
				val = Color32.op_Implicit(((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors)[num2]);
			}
			if (cachedPlayerData.IsDead)
			{
				if (_pp_DeadColorCache.TryGetValue(current.PlayerId, out var value))
				{
					val = value;
				}
				else
				{
					val = Color.Lerp(val, Color.black, 0.4f);
					_pp_DeadColorCache[current.PlayerId] = val;
				}
			}
			cachedPlayerData.DisplayColor = val;
			if (cachedPlayerData.IsDead)
			{
				cachedPlayerData.SortPriority = (cachedPlayerData.IsImpostor ? 3 : 4);
			}
			else
			{
				cachedPlayerData.SortPriority = (cachedPlayerData.IsImpostor ? 1 : 2);
			}
			_pp_ActiveList.Add(cachedPlayerData);
		}
		_pp_ActiveList.Sort(_pp_SortComparison);
		if (unscaledTime - _pp_LastCleanupTime >= 2f)
		{
			_pp_TriedFix.Clear();
			_pp_LastCleanupTime = unscaledTime;
		}
	}

	private static void DrawPlayerEntryNative(CachedPlayerData data, bool amHost, bool isLobby)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		PlayerControl player = data.Player;
		if (amHost && !_pp_TriedFix.Contains(player.PlayerId) && (Object)(object)player.Data.Role == (Object)null)
		{
			_pp_TriedFix.Add(player.PlayerId);
			ImpostorForcer.UpdateRoleLocally(player, player.Data.RoleType);
		}
		Color backgroundColor = GUI.backgroundColor;
		GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		Color color = GUI.color;
		if (data.IsImpostor)
		{
			GUI.color = new Color(1f, 0f, 0f, 0.3f);
		}
		else
		{
			GUI.color = data.DisplayColor;
		}
		float num = GuiStyles.Spacing.MenuSize(22f, 14f);
		GUILayout.Box(GUIContent.none, _pp_ColorBoxStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedWidth(num),
			GuiStyles.CachedHeight(num)
		});
		GUI.color = color;
		GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
		GUILayout.Label(data.Name, data.IsImpostor ? _pp_ImpostorNameStyle : _pp_PlayerNameStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
		if (data.IsDead)
		{
			GUILayout.Label("[DEAD]", _pp_StatusStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
		}
		GUILayout.EndVertical();
		GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 1f));
		float w = GuiStyles.Spacing.MenuSize(CurrentBucket switch
		{
			LayoutBucket.Micro => 30f, 
			LayoutBucket.Tight => 36f, 
			_ => 45f, 
		}, 24f);
		float w2 = GuiStyles.Spacing.MenuSize(CurrentBucket switch
		{
			LayoutBucket.Micro => 34f, 
			LayoutBucket.Tight => 40f, 
			_ => 50f, 
		}, 28f);
		float h = GuiStyles.Spacing.MenuSize(CurrentBucket switch
		{
			LayoutBucket.Micro => 20f, 
			LayoutBucket.Tight => 22f, 
			_ => 26f, 
		}, 18f);
		if (!data.IsDead && !data.Disconnected)
		{
			GUI.backgroundColor = GuiStyles.Theme.Visor;
			if (GUILayout.Button("TP", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GuiStyles.CachedWidth(w),
				GuiStyles.CachedHeight(h)
			}))
			{
				PlayerControl.LocalPlayer.NetTransform.SnapTo(Vector2.op_Implicit(((Component)player).transform.position));
			}
		}
		if (amHost && (Object)(object)ShipStatus.Instance != (Object)null && !data.IsDead)
		{
			GUI.backgroundColor = GuiStyles.Theme.Error;
			if (GUILayout.Button("Kill", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GuiStyles.CachedWidth(w2),
				GuiStyles.CachedHeight(h)
			}))
			{
				GameCheats.HostForceKillPlayer(player);
			}
		}
		if (amHost && (Object)(object)player != (Object)(object)PlayerControl.LocalPlayer)
		{
			GUI.backgroundColor = GuiStyles.Theme.Error;
			if (GUILayout.Button("Kick", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GuiStyles.CachedWidth(w2),
				GuiStyles.CachedHeight(h)
			}))
			{
				GameCheats.KickPlayer(player);
			}
			if (isLobby)
			{
				GUI.backgroundColor = GuiStyles.Theme.Accent;
				if (GUILayout.Button("Ban", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
				{
					GuiStyles.CachedWidth(w),
					GuiStyles.CachedHeight(h)
				}))
				{
					GameCheats.KickPlayer(player, ban: true);
				}
			}
		}
		GUI.backgroundColor = backgroundColor;
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	private static void ExecuteGlowBorder()
	{
	}

	private static void InitV5Styles()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected O, but got Unknown
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Expected O, but got Unknown
		if (_v5CardStyle == null)
		{
			_v5CardStyle = new GUIStyle(GuiStyles.ContainerStyle);
			_v5CardStyle.margin = CreateRectOffset(GuiStyles.Spacing.MenuPad(2), GuiStyles.Spacing.MenuPad(2), GuiStyles.Spacing.MenuPad(2), GuiStyles.Spacing.MenuPad(2));
			_v5CardStyle.padding = CreateRectOffset(GuiStyles.Spacing.MenuPad(6), GuiStyles.Spacing.MenuPad(6), GuiStyles.Spacing.MenuPad(6), GuiStyles.Spacing.MenuPad(6));
			_v5ColorBoxStyle = new GUIStyle(GUI.skin.box);
			_v5ColorBoxStyle.fixedWidth = GuiStyles.Spacing.MenuSize(24f, 16f);
			_v5ColorBoxStyle.fixedHeight = GuiStyles.Spacing.MenuSize(24f, 16f);
			_v5ColorBoxStyle.margin = CreateRectOffset(0, GuiStyles.Spacing.MenuPad(8), 0, 0);
			_v5ColorBoxStyle.normal.background = Texture2D.whiteTexture;
			_v5CardNameStyle = new GUIStyle(GUI.skin.label);
			_v5CardNameStyle.fontStyle = (FontStyle)1;
			_v5CardNameStyle.fontSize = GuiStyles.Spacing.MenuFont(14);
			_v5CardNameStyle.alignment = (TextAnchor)3;
			_v5CardNameStyle.wordWrap = false;
			_v5CardNameStyle.clipping = (TextClipping)1;
			_v5CardNameStyle.normal.textColor = Color.white;
			_v5CardStatusStyle = new GUIStyle(GUI.skin.label);
			_v5CardStatusStyle.fontSize = GuiStyles.Spacing.MenuFont(11);
			_v5CardStatusStyle.alignment = (TextAnchor)3;
			_v5CardStatusStyle.wordWrap = false;
			_v5CardStatusStyle.clipping = (TextClipping)1;
			_v5CardStatusStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
			_v5ActionBtnStyle = new GUIStyle(GuiStyles.ButtonStyle);
			_v5ActionBtnStyle.fontSize = GuiStyles.Spacing.MenuFont(12);
			_v5ActionBtnStyle.padding = CreateRectOffset(GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(2), GuiStyles.Spacing.MenuPad(2));
			_v5ActionBtnStyle.margin = CreateRectOffset(GuiStyles.Spacing.MenuPad(2), GuiStyles.Spacing.MenuPad(2), 0, 0);
		}
	}

	private static RectOffset CreateRectOffset(int left, int right, int top, int bottom)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		return new RectOffset
		{
			left = left,
			right = right,
			top = top,
			bottom = bottom
		};
	}

	private static void ExecutePlayerCard(SecureStreamReader reader)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		reader.ReadByte();
		string text = reader.ReadString();
		float num = reader.ReadSingle();
		float num2 = reader.ReadSingle();
		float num3 = reader.ReadSingle();
		byte b = reader.ReadByte();
		byte b2 = reader.ReadByte();
		string text2 = reader.ReadString();
		InitV5Styles();
		Color val = default(Color);
		((Color)(ref val))._002Ector(num, num2, num3, 1f);
		Color backgroundColor = ((b == 1) ? new Color(1f, 0.1f, 0.1f, 0.15f) : new Color(0.2f, 0.2f, 0.25f, 0.8f));
		Color backgroundColor2 = GUI.backgroundColor;
		GUI.backgroundColor = backgroundColor;
		GUILayout.BeginVertical(_v5CardStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedWidth(_gridCellWidth),
			GuiStyles.CachedHeight(_gridCellHeight)
		});
		GUI.backgroundColor = backgroundColor2;
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		Color color = GUI.color;
		GUI.color = val;
		GUILayout.Box(GUIContent.none, _v5ColorBoxStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUI.color = color;
		GUILayout.BeginVertical((Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		Color contentColor = GUI.contentColor;
		GUI.contentColor = (Color)((b2 == 1) ? new Color(0.5f, 0.5f, 0.5f) : Color.Lerp(Color.white, val, 0.7f));
		GUILayout.Label(text, _v5CardNameStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		if (b == 1)
		{
			GUI.contentColor = GuiStyles.Theme.Error;
			GUILayout.Label(" (Impostor)", _v5CardNameStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		GUI.contentColor = contentColor;
		GUILayout.EndHorizontal();
		GUI.contentColor = (Color)((b2 == 1) ? new Color(0.6f, 0.6f, 0.6f) : ((b == 1) ? Color.white : Color.Lerp(Color.white, val, 0.3f)));
		GUILayout.Label(text2, _v5CardStatusStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUI.contentColor = contentColor;
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.Space(_gridSpacing);
	}

	private static void ExecutePlayerCardMini(SecureStreamReader reader)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		reader.ReadByte();
		string text = reader.ReadString();
		float num = reader.ReadSingle();
		float num2 = reader.ReadSingle();
		float num3 = reader.ReadSingle();
		InitV5Styles();
		Color color = default(Color);
		((Color)(ref color))._002Ector(num, num2, num3, 1f);
		GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedHeight(GuiStyles.Spacing.MenuSize(28f, 20f)) });
		Color color2 = GUI.color;
		GUI.color = color;
		float num4 = GuiStyles.Spacing.MenuSize(20f, 14f);
		GUILayout.Box(GUIContent.none, _v5ColorBoxStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedWidth(num4),
			GuiStyles.CachedHeight(num4)
		});
		GUI.color = color2;
		GUILayout.Label(text, _v5CardNameStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	private static void ExecuteActionButton(SecureStreamReader reader)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		string text = reader.ReadString();
		string key = reader.ReadString();
		float num = reader.ReadSingle();
		float num2 = reader.ReadSingle();
		float num3 = reader.ReadSingle();
		float w = reader.ReadSingle();
		float h = reader.ReadSingle();
		InitV5Styles();
		Color backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = new Color(num, num2, num3, 1f);
		if (GUILayout.Button(text, _v5ActionBtnStyle ?? GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedWidth(w),
			GuiStyles.CachedHeight(h)
		}) && _actionRegistry != null && _actionRegistry.ContainsKey(key))
		{
			_actionRegistry[key]?.Invoke(GetRenderContext());
		}
		GUI.backgroundColor = backgroundColor;
	}

	private static byte[] HexToBytes(string hex)
	{
		if (string.IsNullOrEmpty(hex))
		{
			return null;
		}
		int num = hex.Length / 2;
		byte[] array = new byte[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
		}
		return array;
	}

	private static bool ValidateBytecodeHMAC(byte[] bytecode, int payloadStart, byte[] expectedHmac)
	{
		if (expectedHmac == null || expectedHmac.Length != 32)
		{
			return false;
		}
		if (string.IsNullOrEmpty(ServerData.SessionDecryptKey))
		{
			return false;
		}
		try
		{
			if (_hmacKeyBuffer == null || _hmacKeyBuffer.Length != 32)
			{
				_hmacKeyBuffer = HexToBytes(ServerData.SessionDecryptKey);
			}
			if (_hmacKeyBuffer == null)
			{
				return false;
			}
			using HMACSHA256 hMACSHA = new HMACSHA256(_hmacKeyBuffer);
			byte[] array = hMACSHA.ComputeHash(bytecode, payloadStart, bytecode.Length - payloadStart);
			if (array.Length != expectedHmac.Length)
			{
				return false;
			}
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				num |= array[i] ^ expectedHmac[i];
			}
			return num == 0;
		}
		catch
		{
			return false;
		}
	}

	internal static void InvalidateHMACCache()
	{
		_hmacKeyBuffer = null;
	}

	private static void ExecuteStream(SecureStreamReader reader, long limitPos = -1L)
	{
		//IL_2410: Unknown result type (might be due to invalid IL or missing references)
		//IL_241a: Expected O, but got Unknown
		//IL_2415: Unknown result type (might be due to invalid IL or missing references)
		//IL_241a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2421: Unknown result type (might be due to invalid IL or missing references)
		//IL_2460: Unknown result type (might be due to invalid IL or missing references)
		//IL_2474: Expected O, but got Unknown
		//IL_239a: Unknown result type (might be due to invalid IL or missing references)
		//IL_23a4: Expected O, but got Unknown
		//IL_23ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_24e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1869: Unknown result type (might be due to invalid IL or missing references)
		//IL_1897: Unknown result type (might be due to invalid IL or missing references)
		//IL_176c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1771: Unknown result type (might be due to invalid IL or missing references)
		//IL_177e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b77: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b7d: Invalid comparison between Unknown and I4
		//IL_1cee: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cf3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d00: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dce: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dd3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Invalid comparison between Unknown and I4
		//IL_0562: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_06da: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e0: Invalid comparison between Unknown and I4
		//IL_123a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1240: Invalid comparison between Unknown and I4
		//IL_18bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b82: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b87: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b90: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_17c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b9f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ba4: Unknown result type (might be due to invalid IL or missing references)
		//IL_2149: Unknown result type (might be due to invalid IL or missing references)
		//IL_2187: Unknown result type (might be due to invalid IL or missing references)
		//IL_21c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Unknown result type (might be due to invalid IL or missing references)
		//IL_0707: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bad: Unknown result type (might be due to invalid IL or missing references)
		//IL_1baf: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e14: Unknown result type (might be due to invalid IL or missing references)
		//IL_2235: Unknown result type (might be due to invalid IL or missing references)
		//IL_223a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2247: Unknown result type (might be due to invalid IL or missing references)
		//IL_21fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bdf: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bf4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c0e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c13: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e48: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e78: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e34: Expected O, but got Unknown
		//IL_1eac: Unknown result type (might be due to invalid IL or missing references)
		//IL_1eb8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ec2: Expected O, but got Unknown
		//IL_1ed6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ed8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ea1: Unknown result type (might be due to invalid IL or missing references)
		//IL_2292: Unknown result type (might be due to invalid IL or missing references)
		//IL_1eec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f92: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f9c: Expected O, but got Unknown
		//IL_0b6b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b75: Expected O, but got Unknown
		//IL_0fe3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bbc: Unknown result type (might be due to invalid IL or missing references)
		//IL_103f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1044: Unknown result type (might be due to invalid IL or missing references)
		//IL_104d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c18: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c26: Unknown result type (might be due to invalid IL or missing references)
		//IL_1058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c34: Unknown result type (might be due to invalid IL or missing references)
		//IL_1087: Unknown result type (might be due to invalid IL or missing references)
		//IL_108d: Invalid comparison between Unknown and I4
		//IL_0ccd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd3: Invalid comparison between Unknown and I4
		//IL_1094: Unknown result type (might be due to invalid IL or missing references)
		//IL_109a: Invalid comparison between Unknown and I4
		//IL_0cda: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce0: Invalid comparison between Unknown and I4
		//IL_10d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d8: Invalid comparison between Unknown and I4
		//IL_0d17: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d1e: Invalid comparison between Unknown and I4
		//IL_10ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_1106: Invalid comparison between Unknown and I4
		//IL_0d47: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d4e: Invalid comparison between Unknown and I4
		//IL_110a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1114: Invalid comparison between Unknown and I4
		//IL_0d52: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d5c: Invalid comparison between Unknown and I4
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		try
		{
			int num5 = 0;
			int num6 = 10000;
			Color val2 = default(Color);
			while (!reader.EndOfStream && (limitPos == -1 || reader.Position < limitPos) && ++num5 <= num6)
			{
				switch (reader.ReadByte())
				{
				case 1:
					ExecuteWinBegin(reader);
					break;
				case 2:
					ExecuteWinEnd();
					break;
				case 3:
					reader.ReadString();
					break;
				case 16:
				{
					byte b4 = reader.ReadByte();
					if (!_isWindowMinimized)
					{
						if (b4 == 0)
						{
							GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
						}
						else
						{
							GUILayout.BeginHorizontal(GhostStyleMap.Get(b4), Array.Empty<GUILayoutOption>());
						}
						num++;
					}
					break;
				}
				case 17:
					if (!_isWindowMinimized)
					{
						GUILayout.EndHorizontal();
						num--;
					}
					break;
				case 18:
				{
					byte b2 = reader.ReadByte();
					if (!_isWindowMinimized)
					{
						if (b2 == 0)
						{
							GUILayout.BeginVertical((Il2CppReferenceArray<GUILayoutOption>)null);
						}
						else
						{
							GUILayout.BeginVertical(GhostStyleMap.Get(b2), Array.Empty<GUILayoutOption>());
						}
						num2++;
					}
					break;
				}
				case 19:
					if (!_isWindowMinimized)
					{
						GUILayout.EndVertical();
						num2--;
					}
					break;
				case 20:
				{
					string text29 = reader.ReadString();
					bool flag8 = reader.ReadByte() == 1;
					bool flag9 = reader.ReadByte() == 1;
					if (!_isWindowMinimized)
					{
						Vector2 pos = GUILayout.BeginScrollView(_currentWindow?.GetScroll(text29) ?? Vector2.zero, flag8, flag9, (GUILayoutOption[])(object)new GUILayoutOption[2]
						{
							GuiStyles.CachedExpandWidth,
							GuiStyles.CachedExpandHeight
						});
						_currentWindow?.SetScroll(text29, pos);
						_scrollStack.Push(text29);
						num3++;
					}
					break;
				}
				case 21:
					if (!_isWindowMinimized)
					{
						GUILayout.EndScrollView();
						if (_scrollStack.Count > 0)
						{
							_scrollStack.Pop();
						}
						num3--;
					}
					break;
				case 22:
				{
					float num54 = reader.ReadSingle();
					float num55 = reader.ReadSingle();
					float num56 = reader.ReadSingle();
					float num57 = reader.ReadSingle();
					if (!_isWindowMinimized)
					{
						GUILayout.BeginArea(new Rect(num54, num55, num56, num57));
						num4++;
					}
					break;
				}
				case 23:
					if (!_isWindowMinimized)
					{
						GUILayout.EndArea();
						num4--;
					}
					break;
				case 24:
					if (!_isWindowMinimized)
					{
						ExecuteSectionBegin(reader);
						break;
					}
					reader.ReadString();
					reader.ReadString();
					break;
				case 25:
					if (!_isWindowMinimized)
					{
						ExecuteSectionEnd();
					}
					break;
				case 32:
				{
					string text6 = ApplyContext(reader.ReadString());
					byte id = reader.ReadByte();
					if (!_isWindowMinimized)
					{
						GUILayout.Label(text6, GhostStyleMap.Get(id), (Il2CppReferenceArray<GUILayoutOption>)null);
					}
					break;
				}
				case 33:
				{
					string text25 = ApplyContext(reader.ReadString());
					string text26 = ApplyContextAction(reader.ReadString());
					if (_isWindowMinimized)
					{
						break;
					}
					bool num44 = GUILayout.Button(text25, GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedHeight(GuiStyles.Layout.ResponsiveButtonHeight) });
					if ((int)Event.current.type == 7)
					{
						Rect lastRect4 = GUILayoutUtility.GetLastRect();
						GuiStyles.DrawRippleEffect(text26, lastRect4);
						if (((Rect)(ref lastRect4)).Contains(Event.current.mousePosition))
						{
							GuiStyles.DrawMultiLayerBloom(lastRect4, 0.5f);
						}
					}
					if (num44)
					{
						GuiStyles.TriggerRipple(text26);
						ActionPermitSystem.RequestExecution(text26, "", GetRenderContext());
					}
					break;
				}
				case 34:
				{
					byte id4 = reader.ReadByte();
					if (_isWindowMinimized)
					{
						break;
					}
					Color color2 = GUI.color;
					if (_currentContext != null)
					{
						if (_currentContext.IsImpostor)
						{
							GUI.color = new Color(1f, 0f, 0f, 0.3f);
						}
						else
						{
							GUI.color = _currentContext.DisplayColor;
						}
					}
					float num45 = GuiStyles.Spacing.MenuSize(22f, 14f);
					GUILayout.Box(GUIContent.none, GhostStyleMap.Get(id4), (GUILayoutOption[])(object)new GUILayoutOption[2]
					{
						GuiStyles.CachedWidth(num45),
						GuiStyles.CachedHeight(num45)
					});
					GUI.color = color2;
					break;
				}
				case 35:
				{
					float num26 = reader.ReadSingle();
					if (!_isWindowMinimized)
					{
						GUILayout.Space(GuiStyles.Spacing.MenuSize(CurrentBucket switch
						{
							LayoutBucket.Micro => num26 * 0.25f, 
							LayoutBucket.Tight => num26 * 0.5f, 
							LayoutBucket.Compact => num26 * 0.75f, 
							_ => num26, 
						}, 1f));
					}
					break;
				}
				case 36:
					if (!_isWindowMinimized)
					{
						GUILayout.FlexibleSpace();
					}
					break;
				case 37:
					if (!_isWindowMinimized)
					{
						GuiStyles.DrawPulseSeparator();
					}
					break;
				case 38:
				{
					string text27 = reader.ReadString();
					string text28 = ApplyContext(reader.ReadString());
					if (_isWindowMinimized)
					{
						break;
					}
					bool flag7 = text28.Contains("✓");
					bool num48 = GUILayout.Toggle(flag7, text28, GuiStyles.ToggleStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedHeight(GuiStyles.Layout.ResponsiveToggleHeight) });
					if ((int)Event.current.type == 7)
					{
						Rect lastRect5 = GUILayoutUtility.GetLastRect();
						GuiStyles.DrawToggleIndicator(text27, flag7, lastRect5);
						if (((Rect)(ref lastRect5)).Contains(Event.current.mousePosition))
						{
							GuiStyles.DrawMultiLayerBloom(lastRect5, 0.3f);
						}
					}
					if (num48 != flag7)
					{
						ActionPermitSystem.RequestExecution(text27, "", GetRenderContext());
					}
					break;
				}
				case 39:
				{
					string text23 = reader.ReadString();
					string text24 = reader.ReadString();
					float num40 = reader.ReadSingle();
					float num41 = reader.ReadSingle();
					if (!_isWindowMinimized)
					{
						ServerData.RegisterControlId(text23);
						string id3 = ServerData.DeobfuscateActionId(text23);
						float num42 = ServerData.GetSliderValue(id3);
						if (num42 < num40 || num42 > num41)
						{
							num42 = (num40 + num41) / 2f;
							ServerData.SetSliderValueInternal(id3, num42);
						}
						if (text24.Length > 0)
						{
							GUILayout.Label($"{text24}: {num42:F1}", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						}
						float num43 = GUILayout.HorizontalSlider(num42, num40, num41, GuiStyles.SliderStyle, GuiStyles.SliderThumbStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
						{
							GuiStyles.CachedExpandWidth,
							GuiStyles.CachedHeight(GuiStyles.Spacing.MenuSize(18f))
						});
						if (Mathf.Abs(num43 - num42) > 0.001f)
						{
							ServerData.SetSliderValueInternal(id3, num43);
							_pendingSliderCommit[text23] = Time.unscaledTime;
						}
					}
					break;
				}
				case 187:
				{
					string text8 = reader.ReadString();
					string text9 = reader.ReadString();
					float num9 = reader.ReadSingle();
					float num10 = reader.ReadSingle();
					if (!_isWindowMinimized)
					{
						ServerData.RegisterControlId(text8);
						string id2 = ServerData.DeobfuscateActionId(text8);
						float num11 = ServerData.GetSliderValue(id2);
						if (num11 < num9 || num11 > num10)
						{
							num11 = Mathf.Round((num9 + num10) / 2f);
							ServerData.SetSliderValueInternal(id2, num11);
						}
						int num12 = Mathf.RoundToInt(num11);
						if (text9.Length > 0)
						{
							GUILayout.Label($"{text9}: {num12}", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						}
						int num13 = Mathf.RoundToInt(GUILayout.HorizontalSlider(num11, num9, num10, GuiStyles.SliderStyle, GuiStyles.SliderThumbStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
						{
							GuiStyles.CachedExpandWidth,
							GuiStyles.CachedHeight(GuiStyles.Spacing.MenuSize(18f))
						}));
						if (num13 != num12)
						{
							ServerData.SetSliderValueInternal(id2, num13);
							_pendingSliderCommit[text8] = Time.unscaledTime;
						}
					}
					break;
				}
				case 185:
				{
					string text18 = reader.ReadString();
					string text19 = reader.ReadString();
					float num34 = reader.ReadSingle();
					float num35 = reader.ReadSingle();
					if (_isWindowMinimized)
					{
						break;
					}
					ServerData.RegisterControlId(text18);
					string text20 = ServerData.DeobfuscateActionId(text18);
					float num36 = ServerData.GetSliderValue(text20);
					int num37 = (int)num34;
					int num38 = (int)num35;
					if (num36 < num34 || num36 > num35)
					{
						num36 = num34;
					}
					int num39 = (int)num36;
					string text21 = text20;
					bool flag3 = _textInputFocusedId == text21;
					if (!_textInputBuffers.TryGetValue(text21, out var value9))
					{
						value9 = num39.ToString();
						_textInputBuffers[text21] = value9;
						_textInputLastValue[text21] = num39;
					}
					else if (!flag3)
					{
						_textInputLastValue.TryGetValue(text21, out var value10);
						if (value10 != num39)
						{
							value9 = num39.ToString();
							_textInputBuffers[text21] = value9;
							_textInputLastValue[text21] = num39;
						}
					}
					Event current2 = Event.current;
					string text22;
					if (!flag3)
					{
						text22 = ((!string.IsNullOrEmpty(value9)) ? value9 : $"<color=#555>Enter {num37}–{num38}...</color>");
					}
					else
					{
						bool flag4 = (int)(Time.realtimeSinceStartup * 2.5f) % 2 == 0;
						text22 = value9 + (flag4 ? "│" : " ");
					}
					if (!_textInputLabelCache.TryGetValue(text21, out var value11))
					{
						value11 = text19 + ":";
						_textInputLabelCache[text21] = value11;
					}
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					GUILayout.Label(value11, GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(80f, 50f)) });
					GUIStyle val4;
					if (flag3)
					{
						if (_textInputFocusedStyle == null || (Object)(object)_textInputFocusedStyle.normal.background == (Object)null)
						{
							_textInputFocusedStyle = new GUIStyle(GuiStyles.TextFieldStyle);
							GUIStyle textFieldStyle2 = GuiStyles.TextFieldStyle;
							if ((Object)(object)textFieldStyle2.focused.background != (Object)null)
							{
								_textInputFocusedStyle.normal.background = textFieldStyle2.focused.background;
							}
							_textInputFocusedStyle.normal.textColor = textFieldStyle2.focused.textColor;
						}
						val4 = _textInputFocusedStyle;
					}
					else
					{
						val4 = GuiStyles.TextFieldStyle;
					}
					bool richText3 = !flag3 && string.IsNullOrEmpty(value9);
					bool richText4 = val4.richText;
					val4.richText = richText3;
					GUILayout.Label(text22, val4, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
					val4.richText = richText4;
					Rect lastRect2 = GUILayoutUtility.GetLastRect();
					GUILayout.EndHorizontal();
					if ((int)current2.type == 0)
					{
						if (((Rect)(ref lastRect2)).Contains(current2.mousePosition))
						{
							_textInputFocusedId = text21;
							current2.Use();
						}
						else if (flag3)
						{
							_textInputFocusedId = null;
							int result = 0;
							if (value9.Length > 0)
							{
								int.TryParse(value9, out result);
							}
							result = Mathf.Clamp(result, num37, num38);
							value9 = result.ToString();
							_textInputBuffers[text21] = value9;
							_textInputLastValue[text21] = result;
							if (result != num39)
							{
								ServerData.SetSliderValueInternal(text20, result);
								ActionPermitSystem.RequestExecution(text18, "", GetRenderContext());
							}
						}
					}
					if (!flag3 || (int)current2.type != 4)
					{
						break;
					}
					if ((int)current2.keyCode == 8)
					{
						if (value9.Length > 0)
						{
							_textInputBuffers[text21] = value9.Substring(0, value9.Length - 1);
						}
						current2.Use();
					}
					else if ((int)current2.keyCode == 27)
					{
						_textInputBuffers[text21] = num39.ToString();
						_textInputFocusedId = null;
						current2.Use();
					}
					else if ((int)current2.keyCode == 13 || (int)current2.keyCode == 271)
					{
						_textInputFocusedId = null;
						int result2 = 0;
						if (value9.Length > 0)
						{
							int.TryParse(value9, out result2);
						}
						result2 = Mathf.Clamp(result2, num37, num38);
						value9 = result2.ToString();
						_textInputBuffers[text21] = value9;
						_textInputLastValue[text21] = result2;
						if (result2 != num39)
						{
							ServerData.SetSliderValueInternal(text20, result2);
							ActionPermitSystem.RequestExecution(text18, "", GetRenderContext());
						}
						current2.Use();
					}
					else if (current2.character >= '0' && current2.character <= '9' && value9.Length < 5)
					{
						_textInputBuffers[text21] = value9 + current2.character;
						current2.Use();
					}
					else if (current2.character != 0)
					{
						current2.Use();
					}
					break;
				}
				case 186:
				{
					string text10 = reader.ReadString();
					string text11 = reader.ReadString();
					string text12 = reader.ReadString();
					int num30 = reader.ReadByte();
					if (num30 <= 0 || num30 > 64)
					{
						num30 = 32;
					}
					if (_isWindowMinimized)
					{
						break;
					}
					ServerData.RegisterControlId(text10);
					string text13 = ServerData.DeobfuscateActionId(text10);
					string text14 = "s:" + text13;
					bool flag = _textInputFocusedId == text14;
					if (!_textInputBuffers.TryGetValue(text14, out var value7))
					{
						value7 = "";
						_textInputBuffers[text14] = value7;
					}
					Event current = Event.current;
					string text15;
					if (!flag)
					{
						text15 = ((!string.IsNullOrEmpty(value7)) ? value7 : ("<color=#555>" + text12 + "</color>"));
					}
					else
					{
						bool flag2 = (int)(Time.realtimeSinceStartup * 2.5f) % 2 == 0;
						text15 = value7 + (flag2 ? "│" : " ");
					}
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					if (!string.IsNullOrEmpty(text11))
					{
						GUILayout.Label(text11 + ":", GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(GuiStyles.Spacing.MenuSize(80f, 50f)) });
					}
					GUIStyle val3;
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
						val3 = _textInputFocusedStyle;
					}
					else
					{
						val3 = GuiStyles.TextFieldStyle;
					}
					bool richText = !flag && string.IsNullOrEmpty(value7);
					bool richText2 = val3.richText;
					val3.richText = richText;
					GUILayout.Label(text15, val3, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
					val3.richText = richText2;
					Rect lastRect = GUILayoutUtility.GetLastRect();
					GUILayout.EndHorizontal();
					if ((int)current.type == 0)
					{
						if (((Rect)(ref lastRect)).Contains(current.mousePosition))
						{
							_textInputFocusedId = text14;
							current.Use();
						}
						else if (flag)
						{
							_textInputFocusedId = null;
						}
					}
					if (!flag || (int)current.type != 4)
					{
						break;
					}
					if ((int)current.keyCode == 8)
					{
						if (value7.Length > 0)
						{
							_textInputBuffers[text14] = value7.Substring(0, value7.Length - 1);
						}
						current.Use();
					}
					else if ((int)current.keyCode == 27)
					{
						_textInputBuffers[text14] = "";
						_textInputFocusedId = null;
						current.Use();
					}
					else if ((int)current.keyCode == 13 || (int)current.keyCode == 271)
					{
						string text16 = value7 ?? "";
						_textInputBuffers[text14] = "";
						_textInputFocusedId = null;
						if (text16.Length > 0)
						{
							_stagedStrings[text13] = text16;
							ActionPermitSystem.RequestExecution(text10, "", GetRenderContext());
						}
						current.Use();
					}
					else if (current.character != 0 && current.character != '\n' && current.character != '\r' && current.character != '\b' && value7.Length < num30)
					{
						_textInputBuffers[text14] = value7 + current.character;
						current.Use();
					}
					break;
				}
				case 48:
				{
					reader.ReadSingle();
					float w2 = (CurrentSidebarWidth = GuiStyles.Layout.ResponsiveSidebarWidth);
					if (!_isWindowMinimized)
					{
						GUILayout.BeginVertical(GuiStyles.SidebarStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
						{
							GuiStyles.CachedWidth(w2),
							GuiStyles.CachedExpandHeight
						});
						num2++;
					}
					break;
				}
				case 49:
					if (!_isWindowMinimized)
					{
						if ((int)Event.current.type == 7 && _currentWindow != null)
						{
							GuiStyles.SidebarIndicator.Draw(0f);
						}
						GUILayout.EndVertical();
						num2--;
						GUILayout.Box(GUIContent.none, (GUILayoutOption[])(object)new GUILayoutOption[2]
						{
							GuiStyles.CachedWidth(1f),
							GuiStyles.CachedExpandHeight
						});
					}
					break;
				case 50:
					if (!_isWindowMinimized)
					{
						ExecuteTabButton(reader);
						break;
					}
					reader.ReadString();
					reader.ReadString();
					reader.ReadString();
					break;
				case 51:
					if (!_isWindowMinimized)
					{
						GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[2]
						{
							GuiStyles.CachedExpandWidth,
							GuiStyles.CachedExpandHeight
						});
						num2++;
					}
					break;
				case 52:
					if (!_isWindowMinimized)
					{
						GUILayout.EndVertical();
						num2--;
					}
					break;
				case 53:
					if (!_isWindowMinimized)
					{
						ExecuteLicenseBadge();
					}
					break;
				case 54:
					if (!_isWindowMinimized)
					{
						int value8 = GuiStyles.Spacing.MenuFont((CurrentBucket == LayoutBucket.Micro) ? 14 : ((CurrentBucket == LayoutBucket.Tight) ? 16 : 20));
						GUILayout.Label($"<size={value8}><b>MODMENU</b><color=#FF2140>CREW</color></size>", GuiStyles.SidebarHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					}
					break;
				case 55:
					if (!_isWindowMinimized)
					{
						float num8 = GuiStyles.Spacing.MenuSize((CurrentBucket == LayoutBucket.Micro) ? 4f : ((CurrentBucket == LayoutBucket.Tight) ? 8f : 16f), 2f);
						GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.Space(num8);
						GUILayout.Label($"<size={GuiStyles.Spacing.MenuFont(10)}><color=#9A9EA3>NAVIGATION</color></size>", GuiStyles.SidebarFooterStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.EndHorizontal();
						GUILayout.Box(GUIContent.none, GuiStyles.SeparatorStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.Space(GuiStyles.Spacing.MenuSize((CurrentBucket == LayoutBucket.Micro) ? 1f : 4f, 1f));
					}
					break;
				case 56:
					if (!_isWindowMinimized)
					{
						string value = "Offline";
						try
						{
							value = (((Object)(object)AmongUsClient.Instance != (Object)null) ? "\ud83d\udfe2 Online" : "\ud83d\udd34 Offline");
						}
						catch
						{
						}
						if (CurrentBucket == LayoutBucket.Micro)
						{
							GUILayout.Label($"<size={GuiStyles.Spacing.MenuFont(9)}>v{"6.1.4b"}</size>", GuiStyles.SidebarFooterStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						}
						else
						{
							GUILayout.Label($"<size={GuiStyles.Spacing.MenuFont(10)}>v{"6.1.4b"} | {value}</size>", GuiStyles.SidebarFooterStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						}
					}
					break;
				case 57:
					if (!_isWindowMinimized)
					{
						ExecutePlayerList();
					}
					break;
				case 160:
				{
					byte gridColumns = reader.ReadByte();
					float gridCellWidth = reader.ReadSingle();
					float gridCellHeight = reader.ReadSingle();
					float num53 = reader.ReadSingle();
					_gridColumns = gridColumns;
					_gridCellWidth = gridCellWidth;
					_gridCellHeight = gridCellHeight;
					_gridSpacing = Mathf.Max(2f, num53);
					if (!_isWindowMinimized)
					{
						GUILayout.BeginVertical((Il2CppReferenceArray<GUILayoutOption>)null);
						num2++;
					}
					break;
				}
				case 161:
					if (!_isWindowMinimized)
					{
						GUILayout.EndVertical();
						num2--;
					}
					break;
				case 167:
					if (!_isWindowMinimized)
					{
						GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
						num++;
					}
					break;
				case 168:
					if (!_isWindowMinimized)
					{
						GUILayout.EndHorizontal();
						num--;
						GUILayout.Space(_gridSpacing);
					}
					break;
				case 162:
					if (!_isWindowMinimized)
					{
						ExecutePlayerCard(reader);
						break;
					}
					reader.ReadByte();
					reader.ReadString();
					reader.ReadSingle();
					reader.ReadSingle();
					reader.ReadSingle();
					reader.ReadByte();
					reader.ReadByte();
					reader.ReadString();
					break;
				case 166:
					if (!_isWindowMinimized)
					{
						ExecutePlayerCardMini(reader);
						break;
					}
					reader.ReadByte();
					reader.ReadString();
					reader.ReadSingle();
					reader.ReadSingle();
					reader.ReadSingle();
					break;
				case 163:
				{
					float w5 = reader.ReadSingle();
					if (!_isWindowMinimized)
					{
						GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w5) });
						num++;
					}
					break;
				}
				case 164:
					if (!_isWindowMinimized)
					{
						GUILayout.EndHorizontal();
						num--;
					}
					break;
				case 165:
					if (!_isWindowMinimized)
					{
						ExecuteActionButton(reader);
						break;
					}
					reader.ReadString();
					reader.ReadString();
					reader.ReadSingle();
					reader.ReadSingle();
					reader.ReadSingle();
					reader.ReadSingle();
					reader.ReadSingle();
					break;
				case 169:
				{
					string text17 = reader.ReadString();
					string actionId = reader.ReadString();
					string permitToken = reader.ReadString();
					float num31 = reader.ReadSingle();
					float num32 = reader.ReadSingle();
					float num33 = reader.ReadSingle();
					float w4 = reader.ReadSingle();
					float h = reader.ReadSingle();
					if (!_isWindowMinimized)
					{
						InitV5Styles();
						Color backgroundColor = GUI.backgroundColor;
						GUI.backgroundColor = new Color(num31, num32, num33, 1f);
						if (GUILayout.Button(text17, _v5ActionBtnStyle ?? GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
						{
							GuiStyles.CachedWidth(w4),
							GuiStyles.CachedHeight(h)
						}))
						{
							ActionPermitSystem.RequestExecution(actionId, permitToken, GetRenderContext());
						}
						GUI.backgroundColor = backgroundColor;
					}
					break;
				}
				case 58:
				{
					ushort num20 = reader.ReadUInt16();
					long position = reader.Position;
					long num21 = position + num20;
					if (Time.frameCount - _pp_LastRefreshFrame >= 30)
					{
						RefreshPlayerCache();
					}
					CachedPlayerData currentContext = _currentContext;
					for (int i = 0; i < _pp_ActiveList.Count; i++)
					{
						CachedPlayerData cachedPlayerData = _pp_ActiveList[i];
						if (cachedPlayerData.Active)
						{
							_currentContext = cachedPlayerData;
							reader.Position = position;
							ExecuteStream(reader, num21);
						}
					}
					_currentContext = currentContext;
					reader.Position = num21;
					break;
				}
				case 64:
				{
					_colorStack.Push(GUI.color);
					float num22 = reader.ReadSingle();
					float num23 = reader.ReadSingle();
					float num24 = reader.ReadSingle();
					float num25 = reader.ReadSingle();
					GUI.color = new Color(num22, num23, num24, num25);
					break;
				}
				case 65:
					if (_colorStack.Count > 0)
					{
						GUI.color = _colorStack.Pop();
					}
					break;
				case 80:
					ExecuteHeaderDraw();
					break;
				case 81:
					ExecuteResizeHandle();
					break;
				case 82:
					ExecuteGlowBorder();
					break;
				case 96:
				{
					string obfuscatedId2 = ApplyContextAction(reader.ReadString());
					if (_isWindowMinimized)
					{
						break;
					}
					string key3 = ServerData.DeobfuscateActionId(obfuscatedId2);
					if (ActionPermitSystem.ActionRegistry != null && ActionPermitSystem.ActionRegistry.TryGetValue(key3, out var value4))
					{
						try
						{
							value4(GetRenderContext());
						}
						catch
						{
						}
					}
					break;
				}
				case 97:
				{
					byte b3 = reader.ReadByte();
					string obfuscatedId = ApplyContextAction(reader.ReadString());
					if (_isWindowMinimized || _currentWindow == null || _currentWindow.SelectedTab != b3)
					{
						break;
					}
					string key = ServerData.DeobfuscateActionId(obfuscatedId);
					if (ActionPermitSystem.ActionRegistry != null && ActionPermitSystem.ActionRegistry.TryGetValue(key, out var value2))
					{
						try
						{
							value2(GetRenderContext());
						}
						catch
						{
						}
					}
					break;
				}
				case 112:
				{
					ushort num7 = reader.ReadUInt16();
					if (!ModKeyValidator.IsPremium || !ModKeyValidator.V())
					{
						reader.Skip(num7);
					}
					break;
				}
				case 113:
				{
					string key4 = reader.ReadString();
					ushort num51 = reader.ReadUInt16();
					int value12;
					int num52 = (_tabIdLookup.TryGetValue(key4, out value12) ? value12 : (-1));
					if (_currentWindow == null || _currentWindow.SelectedTab != num52)
					{
						reader.Skip(num51);
					}
					break;
				}
				case 114:
				{
					ushort num50 = reader.ReadUInt16();
					bool flag10 = false;
					try
					{
						flag10 = (Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).IsGameStarted;
					}
					catch
					{
					}
					if (!flag10)
					{
						reader.Skip(num50);
					}
					break;
				}
				case 115:
				{
					string id5 = reader.ReadString();
					ushort num49 = reader.ReadUInt16();
					if (_currentWindow == null || !_currentWindow.IsSectionExpanded(id5))
					{
						reader.Skip(num49);
					}
					break;
				}
				case 240:
				{
					long num46 = reader.ReadInt64();
					bool flag5 = false;
					if (ServerData.PremiumFeatures != null && ServerData.PremiumFeatures.Count > 0)
					{
						if (ServerData.CalculateIntegrity() != num46)
						{
							flag5 = true;
						}
					}
					else if (num46 != 3735928559u && ServerData.CalculateIntegrity() != num46)
					{
						flag5 = true;
					}
					if (flag5)
					{
						_integrityMismatchFrameCount++;
						if (_integrityFirstMismatchTime < 0f)
						{
							_integrityFirstMismatchTime = Time.realtimeSinceStartup;
						}
						bool num47 = _integrityMismatchFrameCount >= 30;
						bool flag6 = Time.realtimeSinceStartup - _integrityFirstMismatchTime > 2f;
						if (num47 || flag6)
						{
							Debug.LogError(Object.op_Implicit("x"));
							ServerData.TriggerSilentDenial();
							return;
						}
					}
					else
					{
						_integrityMismatchFrameCount = 0;
						_integrityFirstMismatchTime = -1f;
					}
					break;
				}
				case 176:
				{
					string pendingTooltipText = reader.ReadString();
					if (_isWindowMinimized || (int)Event.current.type != 7)
					{
						break;
					}
					Rect lastRect3 = GUILayoutUtility.GetLastRect();
					if (((Rect)(ref lastRect3)).Contains(Event.current.mousePosition))
					{
						if (_tooltipHoverRect != lastRect3)
						{
							_tooltipHoverRect = lastRect3;
							_tooltipHoverStartTime = Time.realtimeSinceStartup;
						}
						if (Time.realtimeSinceStartup - _tooltipHoverStartTime >= 0.3f)
						{
							_pendingTooltipText = pendingTooltipText;
							_pendingTooltipRect = new Rect(Event.current.mousePosition.x + 16f, Event.current.mousePosition.y + 16f, 250f, 30f);
						}
					}
					break;
				}
				case 177:
				{
					string value6 = reader.ReadString();
					if (!_isWindowMinimized && CurrentBucket != 0)
					{
						float w3 = GuiStyles.Spacing.MenuSize((CurrentBucket == LayoutBucket.Tight) ? 30f : 40f, 24f);
						GUILayout.Label($"<color=#9A9EA3><size={GuiStyles.Spacing.MenuFont(10)}>[{value6}]</size></color>", GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w3) });
					}
					break;
				}
				case 178:
				{
					string value5 = reader.ReadString();
					float num27 = reader.ReadSingle();
					float num28 = reader.ReadSingle();
					float num29 = reader.ReadSingle();
					if (!_isWindowMinimized)
					{
						Color color = GUI.color;
						GUI.color = new Color(num27, num28, num29, 1f);
						GUILayout.Label($"<size={GuiStyles.Spacing.MenuFont(9)}><b> {value5} </b></size>", GuiStyles.StatusPillStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidthFalse });
						GUI.color = color;
					}
					break;
				}
				case 179:
				{
					float num14 = reader.ReadSingle();
					float num15 = reader.ReadSingle();
					float num16 = reader.ReadSingle();
					float num17 = reader.ReadSingle();
					float num18 = reader.ReadSingle();
					if (!_isWindowMinimized)
					{
						Rect rect2 = GUILayoutUtility.GetRect(0f, GuiStyles.Spacing.MenuSize(16f, 10f), (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
						float num19 = ((num15 > 0f) ? Mathf.Clamp01(num14 / num15) : 0f);
						if ((Object)(object)_cachedProgressBgTex == (Object)null)
						{
							_cachedProgressBgTex = GuiStyles.MakeTexture(1, 1, new Color(0.15f, 0.15f, 0.15f, 0.8f));
						}
						if (_cachedProgressBgStyle == null)
						{
							_cachedProgressBgStyle = new GUIStyle();
							_cachedProgressBgStyle.normal.background = _cachedProgressBgTex;
						}
						GUI.Box(rect2, GUIContent.none, _cachedProgressBgStyle);
						Rect val = new Rect(((Rect)(ref rect2)).x, ((Rect)(ref rect2)).y, ((Rect)(ref rect2)).width * num19, ((Rect)(ref rect2)).height);
						((Color)(ref val2))._002Ector(num16, num17, num18, 0.9f);
						if ((Object)(object)_cachedProgressFillTex == (Object)null || _cachedProgressFillColor != val2)
						{
							_cachedProgressFillTex = GuiStyles.MakeTexture(1, 1, val2);
							_cachedProgressFillStyle = new GUIStyle();
							_cachedProgressFillStyle.normal.background = _cachedProgressFillTex;
							_cachedProgressFillColor = val2;
						}
						GUI.Box(val, GUIContent.none, _cachedProgressFillStyle);
						GUI.Label(rect2, $"<color=#FFFFFF><size={GuiStyles.Spacing.MenuFont(10)}><b>{Mathf.RoundToInt(num14)}/{Mathf.RoundToInt(num15)}</b></size></color>", GuiStyles.LabelStyle);
					}
					break;
				}
				case 180:
				{
					string text7 = reader.ReadString();
					if (!_isWindowMinimized)
					{
						GUILayout.Space(GuiStyles.Spacing.MenuSize(4f, 1f));
						GUILayout.Label("<color=#8A8E93>── " + text7 + " ──</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.Space(GuiStyles.Spacing.MenuSize(2f, 1f));
					}
					break;
				}
				case 181:
				{
					string text4 = reader.ReadString();
					string text5 = ApplyContext(reader.ReadString());
					string key2 = ApplyContextAction(reader.ReadString());
					if (!_isWindowMinimized && GUILayout.Button(text4 + " " + text5, GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()) && _actionRegistry != null && _actionRegistry.TryGetValue(key2, out var value3))
					{
						value3(GetRenderContext());
					}
					break;
				}
				case 182:
				{
					string text2 = reader.ReadString();
					string text3 = reader.ReadString();
					if (!_isWindowMinimized)
					{
						float w = CurrentBucket switch
						{
							LayoutBucket.Micro => 60f, 
							LayoutBucket.Tight => 80f, 
							LayoutBucket.Compact => 120f, 
							_ => 150f, 
						};
						GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.Label("<color=#B0B5BA>" + text2 + "</color>", GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w) });
						GUILayout.Label("<color=#FFFFFF><b>" + text3 + "</b></color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.EndHorizontal();
					}
					break;
				}
				case 183:
				{
					string text = reader.ReadString();
					byte b = reader.ReadByte();
					if (_isWindowMinimized)
					{
						break;
					}
					Texture2D tex;
					switch (b)
					{
					case 1:
						if ((Object)(object)_cachedAlertWarnTex == (Object)null)
						{
							_cachedAlertWarnTex = GuiStyles.MakeTexture(1, 1, new Color(1f, 0.7f, 0f, 0.2f));
						}
						tex = _cachedAlertWarnTex;
						break;
					case 2:
						if ((Object)(object)_cachedAlertErrTex == (Object)null)
						{
							_cachedAlertErrTex = GuiStyles.MakeTexture(1, 1, new Color(1f, 0.2f, 0.2f, 0.2f));
						}
						tex = _cachedAlertErrTex;
						break;
					case 3:
						if ((Object)(object)_cachedAlertOkTex == (Object)null)
						{
							_cachedAlertOkTex = GuiStyles.MakeTexture(1, 1, new Color(0f, 0.9f, 0.4f, 0.2f));
						}
						tex = _cachedAlertOkTex;
						break;
					default:
						if ((Object)(object)_cachedAlertInfoTex == (Object)null)
						{
							_cachedAlertInfoTex = GuiStyles.MakeTexture(1, 1, new Color(0f, 0.6f, 1f, 0.2f));
						}
						tex = _cachedAlertInfoTex;
						break;
					}
					Rect rect = GUILayoutUtility.GetRect(0f, GuiStyles.Spacing.MenuSize(28f, 18f), (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
					GUIStyle cachedAlertStyle = GetCachedAlertStyle(b, tex);
					GUI.Box(rect, GUIContent.none, cachedAlertStyle);
					GUI.Label(rect, "  " + b switch
					{
						1 => "⚠\ufe0f", 
						2 => "❌", 
						3 => "✅", 
						_ => "ℹ\ufe0f", 
					} + " " + text, GuiStyles.LabelStyle);
					break;
				}
				case 184:
					_lastTabBadgeCount = reader.ReadByte();
					break;
				case 241:
					reader.ReadInt64();
					break;
				case 254:
					Debug.LogError(Object.op_Implicit("x"));
					ServerData.TriggerSilentDenial();
					return;
				case byte.MaxValue:
					return;
				default:
					Debug.LogError(Object.op_Implicit("x"));
					ServerData.TriggerSilentDenial();
					return;
				}
			}
		}
		catch (Exception ex)
		{
			if (ex.GetType().Name.Contains("ExitGUI"))
			{
				throw;
			}
			Debug.LogWarning(Object.op_Implicit(ex.Message));
		}
		finally
		{
			try
			{
				while (num3 > 0)
				{
					GUILayout.EndScrollView();
					num3--;
				}
				while (num2 > 0)
				{
					GUILayout.EndVertical();
					num2--;
				}
				while (num > 0)
				{
					GUILayout.EndHorizontal();
					num--;
				}
				while (num4 > 0)
				{
					GUILayout.EndArea();
					num4--;
				}
			}
			catch
			{
			}
			if (!string.IsNullOrEmpty(_pendingTooltipText))
			{
				try
				{
					if (_tooltipStyle == null)
					{
						_tooltipStyle = new GUIStyle(GUI.skin.box);
						_tooltipStyle.fontSize = GuiStyles.Spacing.MenuFont(11);
						_tooltipStyle.wordWrap = true;
						_tooltipStyle.normal.textColor = Color.white;
						_tooltipStyle.padding = CreateRectOffset(GuiStyles.Spacing.MenuPad(8), GuiStyles.Spacing.MenuPad(8), GuiStyles.Spacing.MenuPad(4), GuiStyles.Spacing.MenuPad(4));
						_tooltipStyle.richText = true;
					}
					Vector2 val5 = _tooltipStyle.CalcSize(new GUIContent(_pendingTooltipText));
					((Rect)(ref _pendingTooltipRect)).width = Mathf.Min(val5.x + GuiStyles.Spacing.MenuSize(16f), GuiStyles.Spacing.MenuSize(300f, 180f));
					((Rect)(ref _pendingTooltipRect)).height = _tooltipStyle.CalcHeight(new GUIContent(_pendingTooltipText), ((Rect)(ref _pendingTooltipRect)).width) + GuiStyles.Spacing.MenuSize(8f, 6f);
					if (((Rect)(ref _pendingTooltipRect)).xMax > (float)Screen.width)
					{
						((Rect)(ref _pendingTooltipRect)).x = (float)Screen.width - ((Rect)(ref _pendingTooltipRect)).width;
					}
					if (((Rect)(ref _pendingTooltipRect)).yMax > (float)Screen.height)
					{
						((Rect)(ref _pendingTooltipRect)).y = (float)Screen.height - ((Rect)(ref _pendingTooltipRect)).height;
					}
					GUI.Box(_pendingTooltipRect, _pendingTooltipText, _tooltipStyle);
				}
				catch
				{
				}
				_pendingTooltipText = null;
			}
		}
	}
}
