using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ModMenuCrew.Features;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

public class ReplayUI : MonoBehaviour
{
	public static ReplayUI Instance;

	private ReplayViewer viewer;

	private bool showUI = true;

	private bool showPlayerList = true;

	private bool showEventList;

	private bool showMinimap = true;

	private bool showRoutes = true;

	private bool showHeatmap;

	private bool showGhosts = true;

	private bool showDiagnostics;

	private bool showChat = true;

	private GUIStyle boxStyle;

	private GUIStyle labelStyle;

	private GUIStyle buttonStyle;

	private GUIStyle timelineStyle;

	private GUIStyle eventStyle;

	private GUIStyle _centerLabelStyle;

	private GUIStyle _accentLineStyle;

	private GUIStyle _shortcutsLabelStyle;

	private GUIStyle _smallLabelStyle;

	private GUIStyle _rightLabelStyle;

	private Texture2D bgTex;

	private Texture2D accentTex;

	private Texture2D darkTex;

	private bool stylesInit;

	private static GUIStyle _tabCenterLabelStyle;

	private static GUIStyle _tabCenterPlainStyle;

	private static GUIStyle _tabWordWrapStyle;

	private Vector2 _eventScrollPos = Vector2.zero;

	private Vector2 _playerScrollPos = Vector2.zero;

	private string _followingName = "";

	private float _followingFeedbackTimer;

	private HashSet<byte> _deadNowCache = new HashSet<byte>();

	private int _deadNowCacheFrame = -1;

	private Vector2 _chatScrollPos = Vector2.zero;

	private Texture2D _chatBubbleTex;

	private Texture2D _chatBubbleDeadTex;

	private GUIStyle _chatBubbleStyle;

	private GUIStyle _chatBubbleDeadStyle;

	private GUIStyle _chatNameStyle;

	private GUIStyle _chatMsgStyle;

	private Dictionary<byte, Color> _playerColorsCache;

	private Dictionary<byte, string> _playerNamesCache;

	private int _lastChatCount;

	private static readonly Color BG_COLOR = new Color(0.05f, 0.05f, 0.05f, 0.94f);

	private static readonly Color BG_DARK = new Color(0.07f, 0.07f, 0.07f, 0.94f);

	private static readonly Color ACCENT = new Color(0.95f, 0.1f, 0.1f, 1f);

	private static readonly Color ACCENT_DARK = new Color(0.8f, 0f, 0f, 1f);

	private static readonly Color ACCENT_GLOW = new Color(0.95f, 0.1f, 0.1f, 0.25f);

	private static readonly Color TEXT_DIM = new Color(0.85f, 0.85f, 0.85f, 1f);

	private static readonly Color TEXT_MUTED = new Color(0.5f, 0.5f, 0.55f, 1f);

	private static readonly float[] SPEEDS = new float[6] { 0.25f, 0.5f, 1f, 2f, 4f, 8f };

	private int currentSpeedIndex = 2;

	public bool ShowRoutes => showRoutes;

	public bool ShowHeatmap => showHeatmap;

	public bool ShowMinimap => showMinimap;

	public bool ShowGhosts => showGhosts;

	private static float GetPanelWidth(float w)
	{
		return Mathf.Min(280f, w * 0.35f);
	}

	private static float GetTimelineHeight(float w)
	{
		return Mathf.Min(55f, Mathf.Max(40f, w * 0.06f));
	}

	private static float GetControlsHeight()
	{
		return 48f;
	}

	private static float GetEventPanelWidth(float w)
	{
		return Mathf.Min(240f, w * 0.3f);
	}

	public ReplayUI(IntPtr ptr)
		: base(ptr)
	{
	}//IL_002b: Unknown result type (might be due to invalid IL or missing references)
	//IL_0030: Unknown result type (might be due to invalid IL or missing references)
	//IL_0036: Unknown result type (might be due to invalid IL or missing references)
	//IL_003b: Unknown result type (might be due to invalid IL or missing references)
	//IL_005e: Unknown result type (might be due to invalid IL or missing references)
	//IL_0063: Unknown result type (might be due to invalid IL or missing references)


	private void Awake()
	{
		Instance = this;
	}

	public void Initialize(ReplayViewer v)
	{
		viewer = v;
		showUI = true;
		_playerColorsCache = null;
		_playerNamesCache = null;
	}

	private void InitStyles()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Expected O, but got Unknown
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Expected O, but got Unknown
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Expected O, but got Unknown
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Expected O, but got Unknown
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Expected O, but got Unknown
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Expected O, but got Unknown
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Expected O, but got Unknown
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Expected O, but got Unknown
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Expected O, but got Unknown
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Expected O, but got Unknown
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0518: Expected O, but got Unknown
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0553: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Expected O, but got Unknown
		//IL_057e: Unknown result type (might be due to invalid IL or missing references)
		if (stylesInit && (Object)(object)bgTex != (Object)null && (Object)(object)accentTex != (Object)null && (Object)(object)darkTex != (Object)null)
		{
			return;
		}
		if ((Object)(object)bgTex != (Object)null)
		{
			try
			{
				Object.Destroy((Object)(object)bgTex);
			}
			catch
			{
			}
		}
		if ((Object)(object)accentTex != (Object)null)
		{
			try
			{
				Object.Destroy((Object)(object)accentTex);
			}
			catch
			{
			}
		}
		if ((Object)(object)darkTex != (Object)null)
		{
			try
			{
				Object.Destroy((Object)(object)darkTex);
			}
			catch
			{
			}
		}
		stylesInit = false;
		bgTex = MakeTex(2, 2, BG_COLOR);
		((Object)bgTex).hideFlags = (HideFlags)61;
		accentTex = MakeTex(2, 2, ACCENT);
		((Object)accentTex).hideFlags = (HideFlags)61;
		darkTex = MakeTex(2, 2, BG_DARK);
		((Object)darkTex).hideFlags = (HideFlags)61;
		boxStyle = new GUIStyle(GUI.skin.box);
		boxStyle.normal.background = bgTex;
		boxStyle.padding = CRO(8, 8, 8, 8);
		labelStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 13,
			alignment = (TextAnchor)3,
			richText = true
		};
		labelStyle.normal.textColor = Color.white;
		buttonStyle = new GUIStyle(GUI.skin.button)
		{
			fontSize = 12,
			fontStyle = (FontStyle)1,
			alignment = (TextAnchor)4,
			fixedHeight = 28f
		};
		buttonStyle.padding = CRO(4, 4, 3, 3);
		Texture2D val = MakeFrameTex(32, 28, new Color(0.12f, 0.12f, 0.14f, 1f), new Color(0.4f, 0.1f, 0.1f, 1f), 1);
		((Object)val).hideFlags = (HideFlags)61;
		buttonStyle.normal.background = val;
		buttonStyle.normal.textColor = TEXT_DIM;
		Texture2D val2 = MakeFrameTex(32, 28, new Color(0.18f, 0.08f, 0.08f, 1f), ACCENT, 2);
		((Object)val2).hideFlags = (HideFlags)61;
		buttonStyle.hover.background = val2;
		buttonStyle.hover.textColor = Color.white;
		Texture2D val3 = MakeTex(2, 2, ACCENT_DARK);
		((Object)val3).hideFlags = (HideFlags)61;
		buttonStyle.active.background = val3;
		buttonStyle.active.textColor = Color.white;
		timelineStyle = new GUIStyle(GUI.skin.box);
		timelineStyle.normal.background = darkTex;
		eventStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 11,
			richText = true
		};
		eventStyle.normal.textColor = TEXT_DIM;
		eventStyle.hover.textColor = ACCENT;
		_centerLabelStyle = new GUIStyle(labelStyle)
		{
			alignment = (TextAnchor)4
		};
		GUIStyle val4 = new GUIStyle();
		val4.normal.background = accentTex;
		_accentLineStyle = val4;
		_shortcutsLabelStyle = new GUIStyle(labelStyle)
		{
			alignment = (TextAnchor)4,
			fontSize = 10
		};
		_smallLabelStyle = new GUIStyle(labelStyle)
		{
			fontSize = 10
		};
		_rightLabelStyle = new GUIStyle(labelStyle)
		{
			alignment = (TextAnchor)5,
			richText = true
		};
		if ((Object)(object)_chatBubbleTex != (Object)null)
		{
			try
			{
				Object.Destroy((Object)(object)_chatBubbleTex);
			}
			catch
			{
			}
		}
		if ((Object)(object)_chatBubbleDeadTex != (Object)null)
		{
			try
			{
				Object.Destroy((Object)(object)_chatBubbleDeadTex);
			}
			catch
			{
			}
		}
		_chatBubbleTex = MakeTex(2, 2, new Color(0.92f, 0.92f, 0.92f, 0.95f));
		((Object)_chatBubbleTex).hideFlags = (HideFlags)61;
		_chatBubbleDeadTex = MakeTex(2, 2, new Color(0.7f, 0.7f, 0.7f, 0.6f));
		((Object)_chatBubbleDeadTex).hideFlags = (HideFlags)61;
		_chatBubbleStyle = new GUIStyle(GUI.skin.box);
		_chatBubbleStyle.normal.background = _chatBubbleTex;
		_chatBubbleStyle.padding = CRO(10, 10, 5, 5);
		_chatBubbleStyle.margin = CRO(2, 2, 2, 2);
		_chatBubbleDeadStyle = new GUIStyle(_chatBubbleStyle);
		_chatBubbleDeadStyle.normal.background = _chatBubbleDeadTex;
		_chatNameStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 11,
			fontStyle = (FontStyle)1,
			richText = true,
			alignment = (TextAnchor)0
		};
		_chatNameStyle.normal.textColor = Color.white;
		_chatMsgStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 12,
			wordWrap = true,
			richText = false,
			alignment = (TextAnchor)0
		};
		_chatMsgStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
		stylesInit = true;
	}

	private Texture2D MakeFrameTex(int w, int h, Color bg, Color border, int t)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(w, h, (TextureFormat)4, false);
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				val.SetPixel(j, i, (j < t || j >= w - t || i < t || i >= h - t) ? border : bg);
			}
		}
		val.Apply();
		return val;
	}

	private RectOffset CRO(int l, int r, int t, int b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		return new RectOffset
		{
			left = l,
			right = r,
			top = t,
			bottom = b
		};
	}

	private Texture2D MakeTex(int w, int h, Color c)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return NotifyUtils.MakeTex(w, h, c);
	}

	internal static void DrawTabContent()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		if (_tabCenterLabelStyle == null)
		{
			_tabCenterLabelStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = (TextAnchor)4,
				richText = true
			};
			_tabCenterPlainStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = (TextAnchor)4
			};
			_tabWordWrapStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = (TextAnchor)4,
				richText = true,
				wordWrap = true,
				fontSize = 12
			};
		}
		GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GuiStyles.CachedExpandWidth,
			GuiStyles.CachedExpandHeight
		});
		string text = ColorUtility.ToHtmlStringRGB(GuiStyles.Theme.Accent);
		GUILayout.Label("<size=22><color=#" + text + "><b>[*] REPLAY SYSTEM</b></color></size>", _tabCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.Space(16f);
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(400f) });
		GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
		GUILayout.Label("<size=15><color=#" + text + "><b>RECORDER STATUS</b></color></size>", _tabCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.Space(8f);
		GuiStyles.DrawSeparator();
		GUILayout.Space(8f);
		if ((Object)(object)ReplayRecorder.Instance != (Object)null && ReplayRecorder.Instance.IsRecording)
		{
			GUILayout.Label($"<color=#FF4444><size=18>REC</size></color> - {ReplayRecorder.Instance.FrameCount} frames", _tabCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Space(8f);
			if (GUILayout.Button("STOP RECORDING", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(40f) }))
			{
				ReplayRecorder.Instance.StopRecording();
			}
		}
		else
		{
			GUILayout.Label("<color=#88FF88><size=18>READY</size></color> - Auto: " + (ReplayRecorder.AutoRecordEnabled ? "ON" : "OFF"), _tabCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Space(8f);
			if (GUILayout.Button("START RECORDING", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(40f) }))
			{
				ReplayRecorder.Instance?.StartRecording();
			}
		}
		GUILayout.Space(4f);
		if (GUILayout.Button(ReplayRecorder.AutoRecordEnabled ? "DISABLE AUTO-RECORD" : "ENABLE AUTO-RECORD", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
		{
			ReplayRecorder.AutoRecordEnabled = !ReplayRecorder.AutoRecordEnabled;
		}
		GUILayout.EndVertical();
		GUILayout.Space(16f);
		GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
		GUILayout.Label("<size=15><color=#" + text + "><b>ACTIONS</b></color></size>", _tabCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.Space(8f);
		GuiStyles.DrawSeparator();
		GUILayout.Space(8f);
		if (GUILayout.Button("OPEN REPLAYS FOLDER", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
		{
			Application.OpenURL(Path.Combine(Paths.GameRootPath, "Replays"));
		}
		GUILayout.Space(8f);
		GUILayout.Label("<i>Use <b>F9</b> in-game for the replay overlay.</i>", _tabWordWrapStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.EndVertical();
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
	}

	private void Update()
	{
		HandleInput();
		if (_followingFeedbackTimer > 0f)
		{
			_followingFeedbackTimer -= Time.deltaTime;
		}
	}

	private void HandleInput()
	{
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)viewer == (Object)null)
		{
			return;
		}
		if (Input.GetKeyDown((KeyCode)9))
		{
			showUI = !showUI;
		}
		if (Input.GetKeyDown((KeyCode)32))
		{
			viewer.TogglePause();
		}
		if (Input.GetKeyDown((KeyCode)276))
		{
			viewer.StepFrame(-1);
		}
		if (Input.GetKeyDown((KeyCode)275))
		{
			viewer.StepFrame(1);
		}
		if (Input.GetKeyDown((KeyCode)273) || Input.GetKeyDown((KeyCode)61) || Input.GetKeyDown((KeyCode)270))
		{
			ChangeSpeed(1);
		}
		if (Input.GetKeyDown((KeyCode)274) || Input.GetKeyDown((KeyCode)45) || Input.GetKeyDown((KeyCode)269))
		{
			ChangeSpeed(-1);
		}
		if (Input.GetKey((KeyCode)113))
		{
			viewer.AdjustZoom(0.98f);
		}
		if (Input.GetKey((KeyCode)101))
		{
			viewer.AdjustZoom(1.02f);
		}
		if (Input.GetKeyDown((KeyCode)114))
		{
			showRoutes = !showRoutes;
		}
		if (Input.GetKeyDown((KeyCode)104))
		{
			showHeatmap = !showHeatmap;
		}
		if (Input.GetKeyDown((KeyCode)112))
		{
			showPlayerList = !showPlayerList;
		}
		if (Input.GetKeyDown((KeyCode)108))
		{
			showEventList = !showEventList;
		}
		if (Input.GetKeyDown((KeyCode)103))
		{
			showGhosts = !showGhosts;
		}
		if (Input.GetKeyDown((KeyCode)284))
		{
			showDiagnostics = !showDiagnostics;
		}
		if (Input.GetKeyDown((KeyCode)99))
		{
			showChat = !showChat;
		}
		if (Input.GetKeyDown((KeyCode)46))
		{
			SeekToEvent(1);
		}
		if (Input.GetKeyDown((KeyCode)44))
		{
			SeekToEvent(-1);
		}
		if (Input.GetKeyDown((KeyCode)107))
		{
			SeekToEventOfType(ReplayEventType.Kill, 1);
		}
		for (int i = 0; i < 9; i++)
		{
			if (!Input.GetKeyDown((KeyCode)(49 + i)))
			{
				continue;
			}
			viewer.FollowPlayer((byte)i);
			try
			{
				ReplayData data = viewer.Data;
				if (data == null)
				{
					continue;
				}
				foreach (ReplayPlayerInfo player in data.Players)
				{
					if (player.PlayerId == i)
					{
						_followingName = player.Name;
						_followingFeedbackTimer = 2f;
						break;
					}
				}
			}
			catch
			{
			}
		}
		if (Input.GetKeyDown((KeyCode)48))
		{
			viewer.StopFollowing();
			_followingName = "";
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (Mathf.Abs(axis) > 0.01f)
		{
			viewer.AdjustZoom((axis > 0f) ? 0.9f : 1.1f);
		}
		Vector3 zero = Vector3.zero;
		if (Input.GetKey((KeyCode)119))
		{
			zero.y += 1f;
		}
		if (Input.GetKey((KeyCode)115))
		{
			zero.y -= 1f;
		}
		if (Input.GetKey((KeyCode)97))
		{
			zero.x -= 1f;
		}
		if (Input.GetKey((KeyCode)100))
		{
			zero.x += 1f;
		}
		if (zero != Vector3.zero)
		{
			viewer.StopFollowing();
			viewer.MoveCamera(zero * Time.deltaTime * 5f);
		}
		if (Input.GetKeyDown((KeyCode)27))
		{
			viewer.Stop();
		}
	}

	private void ChangeSpeed(int d)
	{
		currentSpeedIndex = Mathf.Clamp(currentSpeedIndex + d, 0, SPEEDS.Length - 1);
		viewer.SetSpeed(SPEEDS[currentSpeedIndex]);
	}

	[HideFromIl2Cpp]
	private void SeekToEvent(int dir)
	{
		if (viewer?.Data?.Events == null)
		{
			return;
		}
		List<ReplayEvent> events = viewer.Data.Events;
		float currentTime = viewer.CurrentTime;
		if (dir > 0)
		{
			for (int i = 0; i < events.Count; i++)
			{
				if (events[i].Time > currentTime + 0.1f)
				{
					viewer.Seek(events[i].Time);
					_eventScrollPos.y = Mathf.Max(0f, (float)i * 24f - 60f);
					break;
				}
			}
			return;
		}
		for (int num = events.Count - 1; num >= 0; num--)
		{
			if (events[num].Time < currentTime - 0.1f)
			{
				viewer.Seek(events[num].Time);
				_eventScrollPos.y = Mathf.Max(0f, (float)num * 24f - 60f);
				break;
			}
		}
	}

	[HideFromIl2Cpp]
	private void SeekToEventOfType(ReplayEventType type, int dir)
	{
		if (viewer?.Data?.Events == null)
		{
			return;
		}
		List<ReplayEvent> events = viewer.Data.Events;
		float currentTime = viewer.CurrentTime;
		if (dir > 0)
		{
			for (int i = 0; i < events.Count; i++)
			{
				if (events[i].Type == type && events[i].Time > currentTime + 0.1f)
				{
					viewer.Seek(events[i].Time);
					_eventScrollPos.y = Mathf.Max(0f, (float)i * 24f - 60f);
					break;
				}
			}
			return;
		}
		for (int num = events.Count - 1; num >= 0; num--)
		{
			if (events[num].Type == type && events[num].Time < currentTime - 0.1f)
			{
				viewer.Seek(events[num].Time);
				_eventScrollPos.y = Mathf.Max(0f, (float)num * 24f - 60f);
				break;
			}
		}
	}

	private void OnGUI()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)viewer == (Object)null) && viewer.IsActive && showUI && ServerData.IsLoaded)
		{
			InitStyles();
			float scale = GuiStyles.Spacing.Scale;
			Matrix4x4 matrix = GUI.matrix;
			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
			float w = (float)Screen.width / scale;
			float h = (float)Screen.height / scale;
			DrawTopBar(w);
			DrawTimeline(w, h);
			DrawControlsBar(w, h);
			float nextPanelY = 50f;
			if (showEventList)
			{
				DrawEventPanel(w, h, ref nextPanelY);
			}
			if (showPlayerList)
			{
				DrawPlayerPanel(w, h);
			}
			if (showChat)
			{
				DrawChatPanel(w, h);
			}
			DrawOverlays(w, h);
			GUI.matrix = matrix;
		}
	}

	private void DrawTopBar(float w)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		GUI.Box(new Rect(0f, 0f, w, 40f), "", boxStyle);
		GUI.color = ACCENT;
		GUI.Box(new Rect(0f, 38f, w, 2f), "", _accentLineStyle);
		GUI.color = Color.white;
		string text = viewer.Data?.MapName ?? "";
		if (string.IsNullOrEmpty(text) || text == "Unknown")
		{
			text = (viewer.Data?.MapId ?? (-1)) switch
			{
				0 => "The Skeld", 
				1 => "MIRA HQ", 
				2 => "Polus", 
				3 => "Dleks", 
				4 => "Airship", 
				5 => "The Fungle", 
				_ => "", 
			};
		}
		GUI.Label(new Rect(15f, 8f, 300f, 24f), "<size=15><b><color=#FF4444>REPLAY</color></b></size>  <color=#888><size=12>" + text + "</size></color>", labelStyle);
		string text2 = FormatTime(viewer.CurrentTime) + " / " + FormatTime(viewer.TotalDuration);
		GUI.Label(new Rect(w / 2f - 80f, 8f, 160f, 24f), "<size=18><b>" + text2 + "</b></size>", _centerLabelStyle);
		string text3 = $"<size=13><color=#FF5555>{SPEEDS[currentSpeedIndex]}x</color></size>";
		if (viewer.FollowingPlayerId.HasValue)
		{
			string text4 = ((!string.IsNullOrEmpty(_followingName)) ? _followingName : $"P{viewer.FollowingPlayerId.Value}");
			text3 = "<size=11><color=#FF8888>" + text4 + "</color></size>  " + text3;
		}
		GUI.Label(new Rect(w - 200f, 8f, 190f, 24f), text3, _rightLabelStyle);
	}

	private void DrawTimeline(float w, float h)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		float timelineHeight = GetTimelineHeight(w);
		float controlsHeight = GetControlsHeight();
		float num = h - timelineHeight - controlsHeight;
		float num2 = Mathf.Min(80f, w * 0.1f);
		Rect val = default(Rect);
		((Rect)(ref val))._002Ector(num2, num, w - num2 * 2f, timelineHeight);
		GUI.Box(val, "", timelineStyle);
		float totalDuration = viewer.TotalDuration;
		if (totalDuration <= 0f)
		{
			return;
		}
		float num3 = viewer.CurrentTime / totalDuration;
		float num4 = ((Rect)(ref val)).y + 18f;
		float num5 = 14f;
		float num6 = ((Rect)(ref val)).width - 10f;
		Rect val2 = default(Rect);
		((Rect)(ref val2))._002Ector(((Rect)(ref val)).x + 5f, num4, num6, num5);
		GUI.color = new Color(0.15f, 0.15f, 0.15f, 1f);
		GUI.Box(val2, "", _accentLineStyle);
		GUI.color = ACCENT;
		GUI.Box(new Rect(((Rect)(ref val2)).x, ((Rect)(ref val2)).y, num6 * num3, num5), "", _accentLineStyle);
		GUI.Label(new Rect(((Rect)(ref val2)).x + num6 * num3 - 5f, num4 - 12f, 12f, 14f), "<size=12><b>▼</b></size>", _centerLabelStyle);
		GUI.color = Color.white;
		if (viewer.Data?.Events != null)
		{
			foreach (ReplayEvent @event in viewer.Data.Events)
			{
				float num7 = Mathf.Clamp01(@event.Time / totalDuration);
				float num8 = ((Rect)(ref val2)).x + num6 * num7;
				GUI.color = @event.GetColor();
				GUI.Box(new Rect(num8 - 1f, num4, 2f, num5), "", _accentLineStyle);
			}
			GUI.color = Color.white;
		}
		for (int i = 0; i <= 4; i++)
		{
			GUI.Label(new Rect(((Rect)(ref val2)).x + num6 * ((float)i / 4f) - 18f, num4 + num5 + 2f, 40f, 14f), "<size=9>" + FormatTime(totalDuration * (float)i / 4f) + "</size>", _centerLabelStyle);
		}
		if (Input.GetMouseButtonDown(0))
		{
			float scale = GuiStyles.Spacing.Scale;
			Vector2 val3 = default(Vector2);
			((Vector2)(ref val3))._002Ector(Input.mousePosition.x / scale, ((float)Screen.height - Input.mousePosition.y) / scale);
			if (((Rect)(ref val2)).Contains(val3))
			{
				viewer.Seek((val3.x - ((Rect)(ref val2)).x) / num6 * totalDuration);
			}
		}
	}

	private void DrawControlsBar(float w, float h)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0471: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		float controlsHeight = GetControlsHeight();
		float num = h - controlsHeight;
		GUI.Box(new Rect(0f, num, w, controlsHeight), "", boxStyle);
		GUI.color = ACCENT;
		GUI.Box(new Rect(0f, num, w, 2f), "", _accentLineStyle);
		GUI.color = Color.white;
		float num2 = 30f;
		float num3 = num + 9f;
		float num4 = 10f;
		if (GUI.Button(new Rect(num4, num3, 36f, num2), "|<<", buttonStyle))
		{
			viewer.Seek(0f);
		}
		num4 += 40f;
		if (GUI.Button(new Rect(num4, num3, 36f, num2), "<<", buttonStyle))
		{
			viewer.StepFrame(-1);
		}
		num4 += 40f;
		string text = (viewer.IsPaused ? "PLAY" : "PAUSE");
		GUI.backgroundColor = (viewer.IsPaused ? new Color(0.1f, 0.6f, 0.1f) : new Color(0.6f, 0.5f, 0.1f));
		if (GUI.Button(new Rect(num4, num3, 54f, num2), text, buttonStyle))
		{
			viewer.TogglePause();
		}
		GUI.backgroundColor = Color.white;
		num4 += 58f;
		if (GUI.Button(new Rect(num4, num3, 36f, num2), ">>", buttonStyle))
		{
			viewer.StepFrame(1);
		}
		num4 += 40f;
		if (GUI.Button(new Rect(num4, num3, 36f, num2), ">>|", buttonStyle))
		{
			viewer.Seek(viewer.TotalDuration - 0.1f);
		}
		num4 += 42f;
		GUI.color = new Color(0.3f, 0.3f, 0.3f);
		GUI.Box(new Rect(num4, num3 + 4f, 1f, num2 - 8f), "", _accentLineStyle);
		GUI.color = Color.white;
		num4 += 6f;
		GUI.Label(new Rect(num4, num3, 42f, num2), "<size=10>Speed:</size>", _centerLabelStyle);
		num4 += 40f;
		if (GUI.Button(new Rect(num4, num3, 24f, num2), "-", buttonStyle))
		{
			ChangeSpeed(-1);
		}
		num4 += 27f;
		GUI.Label(new Rect(num4, num3, 36f, num2), $"<b>{SPEEDS[currentSpeedIndex]}x</b>", _centerLabelStyle);
		num4 += 38f;
		if (GUI.Button(new Rect(num4, num3, 24f, num2), "+", buttonStyle))
		{
			ChangeSpeed(1);
		}
		num4 += 28f;
		GUI.color = new Color(0.3f, 0.3f, 0.3f);
		GUI.Box(new Rect(num4, num3 + 4f, 1f, num2 - 8f), "", _accentLineStyle);
		GUI.color = Color.white;
		num4 += 6f;
		GUI.Label(new Rect(num4, num3, 42f, num2), "<size=10>Event:</size>", _centerLabelStyle);
		num4 += 40f;
		if (GUI.Button(new Rect(num4, num3, 36f, num2), "Prev", buttonStyle))
		{
			SeekToEvent(-1);
		}
		num4 += 39f;
		if (GUI.Button(new Rect(num4, num3, 36f, num2), "Next", buttonStyle))
		{
			SeekToEvent(1);
		}
		num4 += 40f;
		GUI.backgroundColor = new Color(0.7f, 0.1f, 0.1f);
		if (GUI.Button(new Rect(w - 65f, num3, 55f, num2), "EXIT", buttonStyle))
		{
			viewer.Stop();
		}
		GUI.backgroundColor = Color.white;
		if (GUI.Button(new Rect(w - 140f, num3, 65f, num2), "FREE CAM", buttonStyle))
		{
			viewer.StopFollowing();
			_followingName = "";
		}
		float num5 = w - 150f;
		float num6 = num4 + 8f;
		float num7 = num5 - num6;
		bool[] array = new bool[6] { showRoutes, showHeatmap, showEventList, showGhosts, showPlayerList, showChat };
		int num8 = array.Length;
		string[] array2 = ((num7 > (float)(num8 * 62)) ? new string[6] { "Routes", "Heat", "Events", "Ghosts", "Players", "Chat" } : ((!(num7 > (float)(num8 * 38))) ? new string[6] { "R", "H", "E", "G", "P", "C" } : new string[6] { "Route", "Heat", "Evts", "Ghost", "List", "Chat" }));
		if (!(num7 > (float)(num8 * 24)))
		{
			return;
		}
		float num9 = Mathf.Min(62f, (num7 - (float)((num8 - 1) * 3)) / (float)num8);
		for (int i = 0; i < num8; i++)
		{
			GUI.color = (array[i] ? ACCENT : TEXT_MUTED);
			if (GUI.Button(new Rect(num6 + (float)i * (num9 + 3f), num3, num9, num2), array2[i], buttonStyle))
			{
				switch (i)
				{
				case 0:
					showRoutes = !showRoutes;
					break;
				case 1:
					showHeatmap = !showHeatmap;
					break;
				case 2:
					showEventList = !showEventList;
					break;
				case 3:
					showGhosts = !showGhosts;
					break;
				case 4:
					showPlayerList = !showPlayerList;
					break;
				case 5:
					showChat = !showChat;
					break;
				}
			}
		}
		GUI.color = Color.white;
	}

	private void DrawPlayerPanel(float w, float h)
	{
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c8: Unknown result type (might be due to invalid IL or missing references)
		float panelWidth = GetPanelWidth(w);
		float num = 10f;
		float num2 = 50f;
		float num3 = h - 220f;
		int valueOrDefault = (viewer.Data?.Players?.Count).GetValueOrDefault();
		float num4 = Mathf.Min((float)(32 + valueOrDefault * 26), num3);
		GUI.Box(new Rect(num, num2, panelWidth, num4), "", boxStyle);
		GUI.Label(new Rect(num + 8f, num2 + 4f, panelWidth - 16f, 18f), "<b><color=#FF4444>PLAYERS</color></b>", labelStyle);
		GUI.color = ACCENT;
		GUI.Box(new Rect(num + 4f, num2 + 24f, panelWidth - 8f, 1f), "", _accentLineStyle);
		GUI.color = Color.white;
		if (viewer.Data == null)
		{
			return;
		}
		int frameCount = Time.frameCount;
		if (_deadNowCacheFrame != frameCount)
		{
			_deadNowCache.Clear();
			_deadNowCacheFrame = frameCount;
			try
			{
				List<ReplayFrame> frames = viewer.Data.Frames;
				if (frames.Count > 0 && viewer.TotalDuration > 0f)
				{
					int index = Mathf.Clamp((int)(viewer.CurrentTime / viewer.TotalDuration * (float)(frames.Count - 1)), 0, frames.Count - 1);
					foreach (PlayerState state in frames[index].States)
					{
						if (state.IsDead)
						{
							_deadNowCache.Add(state.PlayerId);
						}
					}
				}
			}
			catch
			{
			}
		}
		HashSet<byte> deadNowCache = _deadNowCache;
		GUILayout.BeginArea(new Rect(num + 2f, num2 + 27f, panelWidth - 4f, num4 - 30f));
		_playerScrollPos = GUILayout.BeginScrollView(_playerScrollPos, false, false, Array.Empty<GUILayoutOption>());
		foreach (ReplayPlayerInfo player in viewer.Data.Players)
		{
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(24f) });
			Color val = ((((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length > player.ColorId) ? Color32.op_Implicit(((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors)[player.ColorId]) : Color.white);
			bool flag = deadNowCache.Contains(player.PlayerId);
			GUI.color = (Color)(flag ? new Color(val.r, val.g, val.b, 0.4f) : val);
			GUILayout.Label("●", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(16f) });
			GUI.color = Color.white;
			_ = player.IsImpostorTeam;
			string text = player.RoleName ?? "";
			if (text.EndsWith("Ghost"))
			{
				text = text.Replace("Ghost", "").Trim();
			}
			bool flag2 = text == "Crewmate" || text == "Impostor" || string.IsNullOrEmpty(text);
			string text2 = ((!player.IsImpostorTeam) ? (flag2 ? "" : ("<color=#5AF><size=10>[" + text + "]</size></color> ")) : (flag2 ? "<color=#F44><size=10>[IMP]</size></color> " : ("<color=#F44><size=10>[" + text + "]</size></color> ")));
			string text3 = (flag ? " <color=#555>DEAD</color>" : "");
			GUILayout.Label(text2 + player.Name + text3, labelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.FlexibleSpace();
			bool flag3 = viewer.FollowingPlayerId == player.PlayerId;
			GUI.color = (flag3 ? ACCENT : TEXT_DIM);
			if (GUILayout.Button(flag3 ? "[ON]" : "CAM", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(42f) }))
			{
				if (flag3)
				{
					viewer.StopFollowing();
					_followingName = "";
				}
				else
				{
					viewer.FollowPlayer(player.PlayerId);
					_followingName = player.Name;
					_followingFeedbackTimer = 2f;
				}
			}
			GUI.color = Color.white;
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void DrawEventPanel(float w, float h, ref float nextPanelY)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		float eventPanelWidth = GetEventPanelWidth(w);
		if (w < 500f)
		{
			return;
		}
		float num = w - eventPanelWidth - 10f;
		float num2 = 50f;
		float num3 = h - 180f;
		nextPanelY = num2 + num3 + 10f;
		GUI.Box(new Rect(num, num2, eventPanelWidth, num3), "", boxStyle);
		GUI.Label(new Rect(num + 8f, num2 + 4f, eventPanelWidth - 16f, 18f), "<b><color=#FF4444>EVENTS</color></b>", labelStyle);
		GUI.color = ACCENT;
		GUI.Box(new Rect(num + 4f, num2 + 24f, eventPanelWidth - 8f, 1f), "", _accentLineStyle);
		GUI.color = Color.white;
		if (viewer.Data?.Events == null)
		{
			return;
		}
		if (!viewer.IsPaused)
		{
			float currentTime = viewer.CurrentTime;
			for (int num4 = viewer.Data.Events.Count - 1; num4 >= 0; num4--)
			{
				if (viewer.Data.Events[num4].Time <= currentTime)
				{
					float num5 = (float)num4 * 24f - num3 * 0.4f;
					_eventScrollPos.y = Mathf.Lerp(_eventScrollPos.y, Mathf.Max(0f, num5), Time.deltaTime * 3f);
					break;
				}
			}
		}
		GUILayout.BeginArea(new Rect(num + 2f, num2 + 27f, eventPanelWidth - 4f, num3 - 30f));
		_eventScrollPos = GUILayout.BeginScrollView(_eventScrollPos, false, true, Array.Empty<GUILayoutOption>());
		foreach (ReplayEvent @event in viewer.Data.Events)
		{
			if (@event.Type != ReplayEventType.Chat)
			{
				bool flag = Mathf.Abs(viewer.CurrentTime - @event.Time) < 0.5f;
				if (flag)
				{
					GUI.color = ACCENT_GLOW;
					GUILayout.Box("", _accentLineStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
					{
						GUILayout.Height(2f),
						GUILayout.ExpandWidth(true)
					});
					GUI.color = Color.white;
				}
				GUI.color = (flag ? Color.white : @event.GetColor());
				string value = FormatTime(@event.Time);
				string value2 = (flag ? ">> " : "");
				if (GUILayout.Button($"{value2}{@event.GetIcon()} [{value}] {@event.Description}", eventStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(22f) }))
				{
					viewer.Seek(@event.Time);
				}
				GUI.color = Color.white;
			}
		}
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private static void GetMapBounds(int id, out float x1, out float x2, out float y1, out float y2)
	{
		switch (id)
		{
		case 0:
			x1 = -22f;
			x2 = 15f;
			y1 = -17f;
			y2 = 5f;
			break;
		case 1:
			x1 = -7f;
			x2 = 27f;
			y1 = -3f;
			y2 = 17f;
			break;
		case 2:
			x1 = 3f;
			x2 = 42f;
			y1 = -27f;
			y2 = -3f;
			break;
		case 3:
			x1 = -15f;
			x2 = 22f;
			y1 = -17f;
			y2 = 5f;
			break;
		case 4:
			x1 = -26f;
			x2 = 26f;
			y1 = -14f;
			y2 = 5f;
			break;
		case 5:
			x1 = -24f;
			x2 = 24f;
			y1 = -18f;
			y2 = 2f;
			break;
		default:
			x1 = -25f;
			x2 = 25f;
			y1 = -15f;
			y2 = 15f;
			break;
		}
	}

	private void DrawMinimap(float w, float startY)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		float num = 160f;
		float num2 = w - num - 15f;
		Rect val = default(Rect);
		((Rect)(ref val))._002Ector(num2, startY, num, num);
		GUI.Box(val, "", boxStyle);
		if (viewer.Data == null)
		{
			return;
		}
		GetMapBounds(viewer.Data.MapId, out var x, out var x2, out var y, out var y2);
		float num3 = Mathf.Max(1f, x2 - x);
		float num4 = Mathf.Max(1f, y2 - y);
		GUI.Label(new Rect(num2 + 4f, startY + 2f, num - 8f, 14f), "<size=9><color=#666>" + (viewer.Data.MapName ?? "Map") + "</color></size>", labelStyle);
		foreach (ReplayPuppet puppet in viewer.GetPuppets())
		{
			if (!((Object)(object)puppet == (Object)null) && ((Component)puppet).gameObject.activeSelf)
			{
				Vector2 val2 = Vector2.op_Implicit(((Component)puppet).transform.position);
				float num5 = Mathf.Clamp01((val2.x - x) / num3);
				float num6 = 1f - Mathf.Clamp01((val2.y - y) / num4);
				float num7 = ((Rect)(ref val)).x + num5 * ((Rect)(ref val)).width;
				float num8 = ((Rect)(ref val)).y + 14f + num6 * (((Rect)(ref val)).height - 14f);
				GUI.color = ((((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length > puppet.Info.ColorId) ? Color32.op_Implicit(((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors)[puppet.Info.ColorId]) : Color.white);
				if (viewer.FollowingPlayerId == puppet.Info.PlayerId)
				{
					GUI.Label(new Rect(num7 - 6f, num8 - 6f, 14f, 14f), "<size=12><b>X</b></size>", _centerLabelStyle);
				}
				else
				{
					GUI.Label(new Rect(num7 - 4f, num8 - 4f, 10f, 10f), "<size=10><b>o</b></size>", _centerLabelStyle);
				}
			}
		}
		GUI.color = Color.white;
	}

	[HideFromIl2Cpp]
	private void EnsurePlayerCaches()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if ((_playerColorsCache != null && _playerNamesCache != null) || viewer?.Data?.Players == null)
		{
			return;
		}
		_playerColorsCache = new Dictionary<byte, Color>();
		_playerNamesCache = new Dictionary<byte, string>();
		foreach (ReplayPlayerInfo player in viewer.Data.Players)
		{
			Color value = ((player.ColorId >= 0 && player.ColorId < ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length) ? Color32.op_Implicit(((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors)[player.ColorId]) : Color.white);
			_playerColorsCache[player.PlayerId] = value;
			_playerNamesCache[player.PlayerId] = player.Name ?? "?";
		}
	}

	[HideFromIl2Cpp]
	private void DrawChatPanel(float w, float h)
	{
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		if (viewer?.Data?.Events == null)
		{
			return;
		}
		EnsurePlayerCaches();
		if (_playerColorsCache == null)
		{
			return;
		}
		float num = Mathf.Min(360f, w * 0.35f);
		float num2 = Mathf.Min(280f, h * 0.35f);
		float controlsHeight = GetControlsHeight();
		float timelineHeight = GetTimelineHeight(w);
		float num3 = 10f;
		float num4 = h - controlsHeight - timelineHeight - num2 - 8f;
		if (showPlayerList)
		{
			int valueOrDefault = (viewer.Data?.Players?.Count).GetValueOrDefault();
			float num5 = 50f + Mathf.Min((float)(32 + valueOrDefault * 26), h - 220f);
			if (num4 < num5 + 8f)
			{
				num4 = num5 + 8f;
			}
			num2 = h - controlsHeight - timelineHeight - num4 - 8f;
			if (num2 < 80f)
			{
				return;
			}
		}
		GUI.Box(new Rect(num3, num4, num, num2), "", boxStyle);
		GUI.Label(new Rect(num3 + 8f, num4 + 4f, num - 16f, 16f), "<b><color=#FF4444>CHAT</color></b>  <size=9><color=#666>[C]</color></size>", labelStyle);
		GUI.color = ACCENT;
		GUI.Box(new Rect(num3 + 4f, num4 + 22f, num - 8f, 1f), "", _accentLineStyle);
		GUI.color = Color.white;
		float currentTime = viewer.CurrentTime;
		GUILayout.BeginArea(new Rect(num3 + 2f, num4 + 25f, num - 4f, num2 - 28f));
		_chatScrollPos = GUILayout.BeginScrollView(_chatScrollPos, false, false, Array.Empty<GUILayoutOption>());
		int num6 = 0;
		foreach (ReplayEvent @event in viewer.Data.Events)
		{
			if (@event.Type == ReplayEventType.Chat)
			{
				if (@event.Time > currentTime)
				{
					break;
				}
				num6++;
				string text = @event.Description ?? "";
				string value = "?";
				string text2 = text;
				int num7 = text.IndexOf(": ");
				if (num7 > 0)
				{
					value = text.Substring(0, num7);
					text2 = text.Substring(num7 + 2);
				}
				bool flag = _deadNowCache.Contains(@event.PlayerId);
				Color val = (_playerColorsCache.ContainsKey(@event.PlayerId) ? _playerColorsCache[@event.PlayerId] : Color.white);
				GUILayout.BeginVertical(flag ? _chatBubbleDeadStyle : _chatBubbleStyle, Array.Empty<GUILayoutOption>());
				string value2 = ColorUtility.ToHtmlStringRGB(val);
				GUILayout.Label($"<color=#{value2}>{value}</color>", _chatNameStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(14f) });
				GUILayout.Label(text2, _chatMsgStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.EndVertical();
				GUILayout.Space(2f);
			}
		}
		if (!viewer.IsPaused && num6 > _lastChatCount)
		{
			_chatScrollPos.y = float.MaxValue;
		}
		_lastChatCount = num6;
		if (num6 == 0)
		{
			GUI.color = TEXT_MUTED;
			GUILayout.Label("<i>No messages</i>", _centerLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = Color.white;
		}
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void DrawOverlays(float w, float h)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_065f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Unknown result type (might be due to invalid IL or missing references)
		//IL_068a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0633: Unknown result type (might be due to invalid IL or missing references)
		if (viewer.IsPaused)
		{
			float num = 0.5f + Mathf.Sin(Time.time * 3f) * 0.15f;
			GUI.color = new Color(0f, 0f, 0f, 0.35f);
			GUI.Box(new Rect(w / 2f - 70f, h / 2f - 65f, 140f, 50f), "", boxStyle);
			GUI.color = new Color(1f, 1f, 1f, num);
			GUI.Label(new Rect(w / 2f - 60f, h / 2f - 65f, 120f, 30f), "<size=20><b>PAUSED</b></size>", _centerLabelStyle);
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			GUI.Label(new Rect(w / 2f - 60f, h / 2f - 42f, 120f, 18f), "<size=10>[Space] to play</size>", _centerLabelStyle);
			GUI.color = Color.white;
		}
		if (_followingFeedbackTimer > 0f && !string.IsNullOrEmpty(_followingName))
		{
			float num2 = Mathf.Clamp01(_followingFeedbackTimer);
			GUI.color = new Color(0f, 0f, 0f, num2 * 0.5f);
			GUI.Box(new Rect(w / 2f - 90f, h * 0.3f - 4f, 180f, 28f), "", boxStyle);
			GUI.color = new Color(1f, 0.6f, 0.6f, num2);
			GUI.Label(new Rect(w / 2f - 80f, h * 0.3f, 160f, 20f), "<size=14><b>CAM: " + _followingName + "</b></size>", _centerLabelStyle);
			GUI.color = Color.white;
		}
		if (showDiagnostics && viewer.Data != null)
		{
			Rect val = default(Rect);
			((Rect)(ref val))._002Ector(w / 2f - 160f, 50f, 320f, 120f);
			GUI.Box(val, "", boxStyle);
			int num3 = 0;
			try
			{
				foreach (ReplayPuppet puppet in viewer.GetPuppets())
				{
					_ = puppet;
					num3++;
				}
			}
			catch
			{
			}
			ReplayGameSettings settings = viewer.Data.Settings;
			string text = $"<size=10><b>DIAGNOSTICS [F3]</b>\nFrames: {viewer.Data.Frames.Count} | Events: {viewer.Data.Events?.Count ?? 0} | Puppets: {num3}\nMap: {viewer.Data.MapName} (ID {viewer.Data.MapId}) | Version: v{5}\nSpeed: {settings?.PlayerSpeed:F1}x | Vision: C{settings?.CrewmateVision:F1} I{settings?.ImpostorVision:F1} | Kill CD: {settings?.KillCooldown:F0}s\nTasks: {settings?.NumCommonTasks}C {settings?.NumLongTasks}L {settings?.NumShortTasks}S | Mode: {settings?.GameMode ?? "?"}\nConfirm Ejects: {settings?.ConfirmEjects} | Anon Votes: {settings?.AnonymousVotes} | Visual Tasks: {settings?.VisualTasks}</size>";
			GUI.Label(new Rect(((Rect)(ref val)).x + 8f, ((Rect)(ref val)).y + 4f, ((Rect)(ref val)).width - 16f, ((Rect)(ref val)).height - 8f), text, labelStyle);
		}
		float num4 = 43f;
		GUI.color = new Color(1f, 1f, 1f, 0.6f);
		GUI.Label(new Rect(0f, num4, w, 16f), "<size=10><color=#777>Space:Play  < >:Events  K:Kill  1-9:Follow  G:Ghosts  C:Chat  R:Routes  F3:Info  Tab:UI  Esc:Exit</color></size>", _shortcutsLabelStyle);
		GUI.color = Color.white;
	}

	private string FormatTime(float sec)
	{
		if (sec >= 3600f)
		{
			int value = (int)(sec / 3600f);
			int value2 = (int)(sec % 3600f / 60f);
			int value3 = (int)(sec % 60f);
			return $"{value}:{value2:D2}:{value3:D2}";
		}
		int value4 = (int)(sec / 60f);
		int value5 = (int)(sec % 60f);
		return $"{value4}:{value5:D2}";
	}
}
