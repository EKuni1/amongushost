using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ModMenuCrew.Easing;
using ModMenuCrew.Features;
using UnityEngine;

namespace ModMenuCrew.UI.Styles;

public static class GuiStyles
{
	public static class Spacing
	{
		public const float XS = 2f;

		public const float SM = 4f;

		public const float MD = 8f;

		public const float LG = 12f;

		public const float XL = 16f;

		public const float XXL = 24f;

		private static float _cachedScale = 1f;

		private static int _cachedScaleHeight = -1;

		private static float _cachedMenuScale = 1f;

		private static float _lastMenuW = 500f;

		private static float _lastMenuH = 600f;

		private static int _menuScaleFrame = -1;

		public static float Scale
		{
			get
			{
				int height = Screen.height;
				if (height != _cachedScaleHeight)
				{
					_cachedScaleHeight = height;
					_cachedScale = Mathf.Clamp(Mathf.Pow((float)height / 1080f, 0.7f), 0.85f, 2.2f);
				}
				return _cachedScale;
			}
		}

		public static float MenuScale => _cachedMenuScale;

		public static int Scaled(float value)
		{
			return Mathf.RoundToInt(value * Scale);
		}

		public static int ScaledFont(int baseSize)
		{
			return Mathf.Max(10, Mathf.RoundToInt((float)baseSize * Scale));
		}

		public static void UpdateMenuScale(float menuW, float menuH)
		{
			int frameCount = Time.frameCount;
			if (frameCount == _menuScaleFrame)
			{
				return;
			}
			_menuScaleFrame = frameCount;
			if (!(Mathf.Abs(menuW - _lastMenuW) < 2f) || !(Mathf.Abs(menuH - _lastMenuH) < 2f))
			{
				_lastMenuW = menuW;
				_lastMenuH = menuH;
				float num = Mathf.Clamp(Mathf.Min(menuW / 500f, menuH / 600f), 0.55f, 1f);
				if (GhostUI.IsActivelyResizing)
				{
					_cachedMenuScale = num;
					return;
				}
				float num2 = 12f * Time.deltaTime;
				_cachedMenuScale = ((Mathf.Abs(num - _cachedMenuScale) < 0.01f) ? num : Mathf.Lerp(_cachedMenuScale, num, Mathf.Clamp01(num2)));
			}
		}

		public static int MenuFont(int baseSize)
		{
			return Mathf.Max(9, Mathf.RoundToInt((float)baseSize * _cachedMenuScale));
		}

		public static float MenuSize(float baseSize, float floor = 12f)
		{
			return Mathf.Max(floor, baseSize * _cachedMenuScale);
		}

		public static int MenuPad(int basePad)
		{
			return Mathf.Max(1, Mathf.RoundToInt((float)basePad * _cachedMenuScale));
		}
	}

	public static class Layout
	{
		public const float LABEL_WIDTH = 120f;

		public const float SLIDER_HEIGHT = 24f;

		public const float BUTTON_HEIGHT = 42f;

		public const float TOGGLE_HEIGHT = 28f;

		public const float SECTION_PADDING = 12f;

		public const float SIDEBAR_WIDTH = 200f;

		public static float ScaledLabelWidth => 120f * Spacing.Scale;

		public static float ScaledButtonHeight => 42f * Spacing.Scale;

		public static float ScaledSidebarWidth => 200f * Spacing.Scale;

		public static float ResponsiveLabelWidth => GhostUI.CurrentBucket switch
		{
			LayoutBucket.Micro => 60f, 
			LayoutBucket.Tight => 80f, 
			_ => 120f, 
		};

		public static float ResponsiveSidebarWidth
		{
			get
			{
				float currentWindowWidth = GhostUI.CurrentWindowWidth;
				if (currentWindowWidth < 250f)
				{
					return 48f;
				}
				if (currentWindowWidth < 400f)
				{
					return 80f;
				}
				if (currentWindowWidth < 550f)
				{
					return 140f;
				}
				return 200f;
			}
		}

		public static float ResponsiveButtonHeight => Spacing.MenuSize(GhostUI.CurrentBucket switch
		{
			LayoutBucket.Micro => 24f, 
			LayoutBucket.Tight => 28f, 
			_ => 42f, 
		}, 18f);

		public static float ResponsiveToggleHeight => Spacing.MenuSize(GhostUI.CurrentBucket switch
		{
			LayoutBucket.Micro => 22f, 
			LayoutBucket.Tight => 24f, 
			_ => 28f, 
		}, 16f);
	}

	public static class Animation
	{
		public const float PulseSpeed = 2f;

		private static float _lastHoverTime;

		private static string _lastHoveredId;

		public static float GetHoverScale(string id, Rect rect)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			if ((rect).Contains(Event.current.mousePosition))
			{
				if (_lastHoveredId != id)
				{
					_lastHoveredId = id;
					_lastHoverTime = Time.realtimeSinceStartup;
				}
				float num = Time.realtimeSinceStartup - _lastHoverTime;
				return 1f + Mathf.Min(0.05f, num * 0.25f);
			}
			return 1f;
		}

		public static float Pulse(float speed = 2f, float min = 0.85f, float max = 1f)
		{
			return Mathf.Lerp(min, max, (Mathf.Sin(Time.realtimeSinceStartup * speed) + 1f) * 0.5f);
		}

		public static float PingPong(float speed = 1f)
		{
			return Mathf.PingPong(Time.realtimeSinceStartup * speed, 1f);
		}

		public static Color PulseColor(Color baseColor, float intensity = 0.15f, float speed = 2f)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			float num = Pulse(speed, 1f - intensity);
			return new Color(baseColor.r * num, baseColor.g * num, baseColor.b * num, baseColor.a);
		}
	}

	public static class Theme
	{
		public static Color BgDarkA = new Color(0.05f, 0.05f, 0.07f, 0.97f);

		public static Color BgDarkB = new Color(0.07f, 0.08f, 0.1f, 0.97f);

		public static Color BgSection = new Color(0.1f, 0.11f, 0.14f, 0.94f);

		public static Color BgCard = new Color(0.14f, 0.15f, 0.19f, 0.9f);

		public static Color Primary = new Color(1f, 0.13f, 0.25f, 1f);

		public static Color Secondary = new Color(0.07f, 0.08f, 0.1f, 1f);

		public static Color Accent = new Color(1f, 0.13f, 0.25f, 1f);

		public static Color AccentSoft = new Color(1f, 0.13f, 0.25f, 0.2f);

		public static Color AccentDim = new Color(1f, 0.13f, 0.25f, 0.08f);

		public static Color AccentHover = new Color(1f, 0.2f, 0.32f, 0.9f);

		public static Color AccentActive = new Color(1f, 0.08f, 0.2f, 1f);

		public static readonly Color Visor = new Color(0f, 0.85f, 1f, 1f);

		public static readonly Color Gold = new Color(1f, 0.85f, 0f, 1f);

		public static readonly Color Success = new Color(0f, 0.9f, 0.4f, 1f);

		public static readonly Color SuccessSoft = new Color(0.3f, 1f, 0.3f, 1f);

		public static readonly Color Error = new Color(1f, 0.2f, 0.2f, 1f);

		public static readonly Color ErrorSoft = new Color(1f, 0.3f, 0.3f, 1f);

		public static readonly Color Warning = new Color(1f, 0.7f, 0f, 1f);

		public static readonly Color StatusInfo = new Color(0.2f, 1f, 1f, 1f);

		public static readonly Color StatusNetwork = new Color(1f, 0.6f, 0.2f, 1f);

		public static readonly Color StatusGold = new Color(1f, 0.8f, 0.2f, 1f);

		public static readonly Color DividerSoft = new Color(0.2f, 0.2f, 0.25f, 0.35f);

		public static readonly Color PanelBg = new Color(0.04f, 0.04f, 0.06f, 0.98f);

		public static readonly Color PanelOutline = new Color(0.15f, 0.15f, 0.2f, 0.5f);

		public static readonly Color HeaderBarBg = new Color(0.06f, 0.06f, 0.09f, 1f);

		public static readonly Color StepInactive = new Color(0.15f, 0.15f, 0.2f, 0.8f);

		public static readonly Color StepLineOff = new Color(0.2f, 0.2f, 0.28f, 0.6f);

		public static readonly Color StepLabelInactive = new Color(0.5f, 0.5f, 0.6f, 1f);

		public static readonly Color BindBtnBg = new Color(0.12f, 0.12f, 0.18f, 0.9f);

		public static readonly Color VersionFooter = new Color(0.62f, 0.64f, 0.72f, 1f);

		public static readonly Color PlaceholderText = new Color(0.75f, 0.75f, 0.78f, 0.7f);

		public static readonly Color StatusPillBg = new Color(0.08f, 0.08f, 0.12f, 0.8f);

		public static readonly Color OverlayDim = new Color(0.02f, 0.02f, 0.04f, 0.3f);

		public static readonly Color InputFieldBg = new Color(0.1f, 0.1f, 0.1f, 0.8f);

		public static readonly Color InputFieldBgFocused = new Color(0.14f, 0.14f, 0.16f, 0.85f);

		public static readonly Color StepOutlineInactive = new Color(0.2f, 0.2f, 0.3f, 0.3f);

		public static readonly Color DividerFaint = new Color(0.18f, 0.18f, 0.22f, 0.25f);

		public static readonly Color TextPrimary = new Color(0.9f, 0.92f, 0.94f, 1f);

		public static readonly Color TextMuted = new Color(0.58f, 0.62f, 0.68f, 1f);

		public static readonly Color TextInactive = new Color(0.4f, 0.43f, 0.48f, 1f);

		public static readonly Color TextDisabled = new Color(0.28f, 0.3f, 0.34f, 0.8f);

		public static Color TextAccent = new Color(1f, 0.45f, 0.5f, 1f);

		public static Color Glow = new Color(1f, 0.13f, 0.25f, 0.18f);

		public static Color GlowStrong = new Color(1f, 0.13f, 0.25f, 0.35f);

		public static Color GlowCyan = new Color(0f, 0.85f, 1f, 0.18f);

		public static readonly Color Blurple = new Color(0.345f, 0.396f, 0.949f, 1f);

		public static readonly Color OnlineGreen = new Color(0.34f, 0.97f, 0.53f, 1f);

		public static readonly Color ImpostorRed = new Color(1f, 0.3f, 0.3f, 1f);

		public static readonly Color CrewmateBlue = new Color(0.4f, 0.8f, 1f, 1f);

		public static Color HeaderTop = new Color(0.07f, 0.08f, 0.1f, 0.97f);

		public static Color HeaderBottom = new Color(0.07f, 0.08f, 0.1f, 0.97f);

		public static readonly Color FreeBorder = new Color(0.18f, 0.19f, 0.22f, 1f);
	}

	public enum GlowState
	{
		Disabled,
		Normal,
		Hover,
		Active,
		Pressed
	}

	public static class GlowSystem
	{
		private static Texture2D _glowTextureNormal;

		private static Texture2D _glowTextureHover;

		private static Texture2D _glowTextureActive;

		private static GUIStyle _glowBoxStyleHover;

		private static GUIStyle _glowBoxStyleActive;

		private static GUIStyle _glowBoxStyleNormal;

		public static Color GetGlowColor(GlowState state, float intensity = 0.15f)
		{
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			float num = state switch
			{
				GlowState.Disabled => 0f, 
				GlowState.Normal => intensity * 0.5f, 
				GlowState.Hover => intensity * 1f, 
				GlowState.Active => intensity * (1f + Mathf.Sin(Time.realtimeSinceStartup * 2f) * 0.15f), 
				GlowState.Pressed => intensity * 1.5f, 
				_ => intensity, 
			};
			return new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, num);
		}

		public static float GetActivePulse()
		{
			return 1f + Mathf.Sin(Time.realtimeSinceStartup * 2f) * 0.15f;
		}

		public static void DrawGlowBackground(Rect elementRect, GlowState state, float padding = 4f)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.current.type == 7)
			{
				float num;
				switch (state)
				{
				case GlowState.Disabled:
					return;
				case GlowState.Normal:
					num = 0.6f;
					break;
				case GlowState.Hover:
					num = 1.4f;
					break;
				case GlowState.Active:
					num = GetActivePulse() * 1.2f;
					break;
				case GlowState.Pressed:
					num = 1.8f;
					break;
				default:
					num = 0.4f;
					break;
				}
				float num2 = num;
				Color glowColor = GetGlowColor(state, 0.2f * num2);
				Rect val = default(Rect);
				(val)._002Ector((elementRect).x - padding, (elementRect).y - padding, (elementRect).width + padding * 2f, (elementRect).height + padding * 2f);
				Texture2D glowTexture = GetGlowTexture(state);
				if ((Object)(object)glowTexture != (Object)null)
				{
					GUIStyle glowBoxStyle = GetGlowBoxStyle(state, glowTexture);
					Color color = GUI.color;
					GUI.color = glowColor;
					GUI.Box(val, GUIContent.none, glowBoxStyle);
					GUI.color = color;
				}
			}
		}

		private static Texture2D GetGlowTexture(GlowState state)
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			return (Texture2D)(state switch
			{
				GlowState.Hover => _glowTextureHover ?? (_glowTextureHover = MakeGlowBorderTexture(8, 8, 2, new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, 0.3f))), 
				GlowState.Active => _glowTextureActive ?? (_glowTextureActive = MakeGlowBorderTexture(8, 8, 2, new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, 0.4f))), 
				_ => _glowTextureNormal ?? (_glowTextureNormal = MakeGlowBorderTexture(8, 8, 2, new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, 0.2f))), 
			});
		}

		private static GUIStyle GetGlowBoxStyle(GlowState state, Texture2D tex)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected O, but got Unknown
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Expected O, but got Unknown
			switch (state)
			{
			case GlowState.Hover:
				if (_glowBoxStyleHover == null)
				{
					_glowBoxStyleHover = new GUIStyle();
					_glowBoxStyleHover.normal.background = tex;
				}
				return _glowBoxStyleHover;
			case GlowState.Active:
				if (_glowBoxStyleActive == null)
				{
					_glowBoxStyleActive = new GUIStyle();
					_glowBoxStyleActive.normal.background = tex;
				}
				return _glowBoxStyleActive;
			default:
				if (_glowBoxStyleNormal == null)
				{
					_glowBoxStyleNormal = new GUIStyle();
					_glowBoxStyleNormal.normal.background = tex;
				}
				return _glowBoxStyleNormal;
			}
		}

		internal static void ClearGlowCache()
		{
			if (Object.op_Implicit((Object)(object)_glowTextureNormal))
			{
				Object.Destroy((Object)(object)_glowTextureNormal);
				_glowTextureNormal = null;
			}
			if (Object.op_Implicit((Object)(object)_glowTextureHover))
			{
				Object.Destroy((Object)(object)_glowTextureHover);
				_glowTextureHover = null;
			}
			if (Object.op_Implicit((Object)(object)_glowTextureActive))
			{
				Object.Destroy((Object)(object)_glowTextureActive);
				_glowTextureActive = null;
			}
			_glowBoxStyleHover = null;
			_glowBoxStyleActive = null;
			_glowBoxStyleNormal = null;
		}

		private static Texture2D MakeGlowBorderTexture(int width, int height, int borderWidth, Color glowColor)
		{
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			string key = $"glowborder_{width}_{height}_{borderWidth}_{((object)(Color)glowColor).GetHashCode()}";
			if (_textureCache.TryGetValue(key, out var value) && (Object)(object)value != (Object)null)
			{
				return value;
			}
			Texture2D val = new Texture2D(width, height, (TextureFormat)4, false);
			((Texture)val).wrapMode = (TextureWrapMode)1;
			((Texture)val).filterMode = (FilterMode)1;
			((Object)val).hideFlags = (HideFlags)61;
			Color32[] array = (Color32[])(object)new Color32[width * height];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					int val2 = j;
					int val3 = width - 1 - j;
					int val4 = i;
					int val5 = height - 1 - i;
					int num = Math.Min(Math.Min(val2, val3), Math.Min(val4, val5));
					if (num < borderWidth)
					{
						float num2 = (float)num / (float)borderWidth;
						Color val6 = glowColor;
						val6.a *= num2;
						array[i * width + j] = Color32.op_Implicit(val6);
					}
					else
					{
						array[i * width + j] = new Color32((byte)0, (byte)0, (byte)0, (byte)0);
					}
				}
			}
			val.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
			val.Apply();
			_textureCache[key] = val;
			return val;
		}
	}

	public static class VisualEffects
	{
		private struct Particle
		{
			public Vector2 position;

			public Vector2 velocity;

			public float size;

			public float alpha;

			public float life;

			public float maxLife;

			public Color color;
		}

		private static List<Particle> _particles = new List<Particle>();

		private static float _lastUpdate;

		private const int MAX_PARTICLES = 30;

		private static uint _lcgState = 1575931494u;

		private static Texture2D _particleTex;

		internal static void ClearEffectsCache()
		{
			if (Object.op_Implicit((Object)(object)_particleTex))
			{
				Object.Destroy((Object)(object)_particleTex);
				_particleTex = null;
			}
			_particleBoxStyle = null;
		}

		private static float LcgNext()
		{
			_lcgState = _lcgState * 1664525 + 1013904223;
			return (float)(_lcgState >> 8) / 16777215f;
		}

		private static float LcgRange(float min, float max)
		{
			return min + LcgNext() * (max - min);
		}

		public static void UpdateAndDraw(Rect windowRect)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_0250: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.current.type != 7)
			{
				return;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float num = realtimeSinceStartup - _lastUpdate;
			_lastUpdate = realtimeSinceStartup;
			if (num > 0.1f)
			{
				num = 0.016f;
			}
			if (_particles.Count < 30 && LcgNext() < 0.1f)
			{
				float num2 = LcgRange((windowRect).x, (windowRect).xMax);
				_particles.Add(new Particle
				{
					position = new Vector2(num2, (windowRect).yMax),
					velocity = new Vector2(LcgRange(-5f, 5f), LcgRange(-20f, -60f)),
					size = LcgRange(2f, 5f),
					alpha = 0f,
					life = 0f,
					maxLife = LcgRange(3f, 6f),
					color = ((LcgNext() > 0.5f) ? Theme.Accent : Theme.Secondary)
				});
			}
			DrawParticle(Vector2.zero, 0f, Color.clear);
			for (int num3 = _particles.Count - 1; num3 >= 0; num3--)
			{
				Particle value = _particles[num3];
				value.life += num;
				ref Vector2 position = ref value.position;
				position += value.velocity * num;
				float num4 = 0.5f;
				if (value.life < num4)
				{
					value.alpha = value.life / num4;
				}
				else
				{
					value.alpha = 1f - (value.life - num4) / (value.maxLife - num4);
				}
				if (value.alpha > 0f)
				{
					Color color = value.color;
					color.a = value.alpha * 0.4f;
					if (value.position.x > (windowRect).x && value.position.x < (windowRect).xMax && value.position.y > (windowRect).y && value.position.y < (windowRect).yMax)
					{
						DrawParticle(value.position, value.size, color);
					}
				}
				_particles[num3] = value;
				if (value.life >= value.maxLife)
				{
					_particles.RemoveAt(num3);
				}
			}
		}

		private static void DrawParticle(Vector2 pos, float size, Color color)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			if ((Object)(object)_particleTex == (Object)null)
			{
				_particleTex = MakeCircleTexture(16, Color.white, new Color(1f, 1f, 1f, 0f));
			}
			if (!(color.a <= 0f))
			{
				Color color2 = GUI.color;
				GUI.color = color;
				if (_particleBoxStyle == null)
				{
					_particleBoxStyle = new GUIStyle();
					_particleBoxStyle.normal.background = _particleTex;
				}
				GUI.Box(new Rect(pos.x - size / 2f, pos.y - size / 2f, size, size), GUIContent.none, _particleBoxStyle);
				GUI.color = color2;
			}
		}
	}

	public static class SidebarIndicator
	{
		private static float _currentY;

		private static float _targetY;

		private static float _currentH;

		private static float _targetH;

		private static bool _initialized;

		public static void SetTarget(float y, float h)
		{
			_targetY = y;
			_targetH = h;
			if (!_initialized)
			{
				_currentY = y;
				_currentH = h;
				_initialized = true;
			}
		}

		public static void Draw(float sidebarX)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.current.type == 7 && _initialized)
			{
				_currentY = Mathf.Lerp(_currentY, _targetY, Mathf.Clamp01(Time.deltaTime * 12f));
				_currentH = Mathf.Lerp(_currentH, _targetH, Mathf.Clamp01(Time.deltaTime * 12f));
				float num = 3f;
				Color color = GUI.color;
				GUI.color = Theme.Accent;
				GUI.Box(new Rect(sidebarX, _currentY, num, _currentH), GUIContent.none, WhitePixelBoxStyle);
				GUI.color = new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, 0.2f);
				GUI.Box(new Rect(sidebarX, _currentY - 2f, num + 8f, _currentH + 4f), GUIContent.none, RadialGlowBoxStyle);
				GUI.color = color;
			}
		}
	}

	public static class TabTransition
	{
		private static int _previousTab = -1;

		private static int _currentTab = 0;

		private static float _transitionStart = -1f;

		private const float DURATION = 0.15f;

		public static void NotifyTabSwitch(int oldTab, int newTab)
		{
			if (oldTab != newTab)
			{
				_previousTab = oldTab;
				_currentTab = newTab;
				_transitionStart = Time.realtimeSinceStartup;
			}
		}

		public static float GetAlpha()
		{
			if (_transitionStart < 0f)
			{
				return 1f;
			}
			float num = Mathf.Clamp01((Time.realtimeSinceStartup - _transitionStart) / 0.15f);
			if (num >= 1f)
			{
				return 1f;
			}
			return ModMenuCrew.Easing.Easing.EaseOutQuad(num);
		}

		public static float GetSlideOffset()
		{
			if (_transitionStart < 0f)
			{
				return 0f;
			}
			float num = Mathf.Clamp01((Time.realtimeSinceStartup - _transitionStart) / 0.15f);
			if (num >= 1f)
			{
				_transitionStart = -1f;
				return 0f;
			}
			float num2 = ModMenuCrew.Easing.Easing.EaseOutExpo(num);
			float num3 = ((_currentTab > _previousTab) ? 1f : (-1f));
			return (1f - num2) * 12f * num3;
		}
	}

	private static readonly Dictionary<float, GUILayoutOption> _widthCache = new Dictionary<float, GUILayoutOption>(32);

	private static readonly Dictionary<float, GUILayoutOption> _heightCache = new Dictionary<float, GUILayoutOption>(32);

	private static GUILayoutOption _cachedExpandWidth;

	private static GUILayoutOption _cachedExpandWidthFalse;

	private static GUILayoutOption _cachedExpandHeight;

	private static readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

	private static Texture2D _cachedPixelTexture;

	private static Texture2D _cachedPixelDarkTexture;

	private static Texture2D _cachedPixelAccentTexture;

	private static Texture2D _cachedPixelErrorTexture;

	private static Texture2D _cachedSuccessTexture;

	private static Texture2D _cachedErrorTexture;

	private static GUIStyle _cachedSuccessIndicatorStyle;

	private static GUIStyle _cachedErrorIndicatorStyle;

	private static GUIStyle _cachedAnimatedHeaderStyle;

	private static int _cyberNoiseSeed = 42;

	private static Texture2D _cachedHoverGlowTexture;

	private static Texture2D _gradientTexture;

	private static GUIStyle _headerStyle;

	private static GUIStyle _subHeaderStyle;

	private static GUIStyle _buttonStyle;

	private static GUIStyle _toggleStyle;

	private static GUIStyle _sliderStyle;

	private static GUIStyle _labelStyle;

	private static GUIStyle _tabStyle;

	private static GUIStyle _selectedTabStyle;

	private static GUIStyle _containerStyle;

	private static LayoutBucket _lastContainerBucket = LayoutBucket.Standard;

	private static GUIStyle _sectionStyle;

	private static LayoutBucket _lastSectionBucket = LayoutBucket.Standard;

	private static GUIStyle _errorStyle;

	private static GUIStyle _iconStyle;

	private static GUIStyle _tooltipStyle;

	private static GUIStyle _statusIndicatorStyle;

	private static GUIStyle _glowStyle;

	private static GUIStyle _shadowStyle;

	private static GUIStyle _highlightStyle;

	private static GUIStyle _separatorStyle;

	private static GUIStyle _betterToggleStyle;

	private static GUIStyle _windowStyle;

	private static GUIStyle _headerBackgroundStyle;

	private static GUIStyle _titleLabelStyle;

	private static GUIStyle _titleBarButtonStyle;

	private static GUIStyle _textFieldStyle;

	private static GUIStyle _crewToggleStyle;

	private static GUIStyle _premiumBadgeStyle;

	private static GUIStyle _freeBadgeStyle;

	private static GUIStyle _timeRemainingStyle;

	private static GUIStyle _visorAccentStyle;

	private static GUIStyle _primaryButtonStyle;

	private static GUIStyle _hostButtonStyle;

	private static GUIStyle _sidebarStyle;

	private static GUIStyle _sidebarButtonStyle;

	private static GUIStyle _sidebarButtonActiveStyle;

	private static GUIStyle _sidebarHeaderStyle;

	private static GUIStyle _sidebarFooterStyle;

	private static GUIStyle _dashboardCardStyle;

	private static GUIStyle _itemStyle;

	private static GUIStyle _selectedItemStyle;

	private static GUIStyle _listButtonStyle;

	private static GUIStyle _scrollbarTrackStyle;

	private static GUIStyle _scrollbarThumbStyle;

	private static GUIStyle _scrollbarThumbHoverStyle;

	private static float _lastStyleMenuScale = 1f;

	private static Texture2D _noiseTexture;

	private static GUIStyle _statusPillStyle;

	private static GUIStyle _sliderThumbStyle;

	private static Texture2D _sliderThumbTexture;

	private static Texture2D _sliderThumbHoverTexture;

	private static Texture2D _separatorTexture;

	private static GUIStyle _gameLetterBig;

	private static GUIStyle _gameLetterMed;

	private static GUIStyle _gameLetterSmall;

	private static GUIStyle _gameTitleStyle;

	private static GUIStyle _gameSubtitleStyle;

	private static GUIStyle _gamePillTitleStyle;

	private static GUIStyle _gamePillSubStyle;

	private static GUIStyle _gameAnimLabelStyle;

	private static float _gameStylesScale = -1f;

	private const float CHAR_WIDTH_RATIO = 0.85f;

	private const float TEXT_PADDING = 8f;

	private static GUIStyle _crewLogoStyleRed;

	private static GUIStyle _crewLogoStyleCyan;

	private static GUIStyle _sliderLabelStyle;

	private static GUIStyle _sliderValueStyle;

	private static GUIStyle _sliderMinMaxStyle;

	private static GUIStyle _eliteStatusActiveStyle;

	private static GUIStyle _eliteStatusInactiveStyle;

	private static Texture2D _radialGlowTex;

	private static Texture2D _pillTex;

	private static Texture2D _knobTex;

	private static GUIStyle _whitePixelBoxStyle;

	private static GUIStyle _radialGlowBoxStyle;

	private static GUIStyle _pillBoxStyle;

	private static GUIStyle _knobBoxStyle;

	private static GUIStyle _particleBoxStyle;

	private static GUIStyle _hoverGlowBoxStyle;

	private static GUIStyle _tooltipBgBoxStyle;

	private static GUIStyle _dropShadowBoxStyle;

	private static GUIStyle _animBgBoxStyleA;

	private static GUIStyle _animBgBoxStyleB;

	private static GUIStyle _scanlineBoxStyle;

	private static int _scanlineCachedHeight;

	private static GUIStyle _eliteDotBoxStyle;

	private static Texture2D _dropShadowTex;

	private static Texture2D _animBgA;

	private static Texture2D _animBgB;

	private static Texture2D _scanlineTileTex;

	private static Dictionary<string, float> _rippleTime = new Dictionary<string, float>();

	private static string _eliteTooltipText;

	private static Vector2 _eliteTooltipPos;

	private static float _eliteTooltipHoverStart = -1f;

	private static float _eliteTooltipShowTime = -1f;

	private static Rect _eliteTooltipTriggerRect;

	private static Texture2D _eliteTooltipBgTex;

	private static Dictionary<string, float> _toggleAnim = new Dictionary<string, float>();

	private static GUIStyle _toggleLabelStyle;

	private static Dictionary<string, float> _scrollSmooth = new Dictionary<string, float>();

	public static GUILayoutOption CachedExpandWidth
	{
		get
		{
			if (_cachedExpandWidth != null)
			{
				try
				{
					_ = ((Il2CppObjectBase)_cachedExpandWidth).Pointer;
					return _cachedExpandWidth;
				}
				catch (ObjectCollectedException)
				{
				}
			}
			_cachedExpandWidth = GUILayout.ExpandWidth(true);
			return _cachedExpandWidth;
		}
	}

	public static GUILayoutOption CachedExpandWidthFalse
	{
		get
		{
			if (_cachedExpandWidthFalse != null)
			{
				try
				{
					_ = ((Il2CppObjectBase)_cachedExpandWidthFalse).Pointer;
					return _cachedExpandWidthFalse;
				}
				catch (ObjectCollectedException)
				{
				}
			}
			_cachedExpandWidthFalse = GUILayout.ExpandWidth(false);
			return _cachedExpandWidthFalse;
		}
	}

	public static GUILayoutOption CachedExpandHeight
	{
		get
		{
			if (_cachedExpandHeight != null)
			{
				try
				{
					_ = ((Il2CppObjectBase)_cachedExpandHeight).Pointer;
					return _cachedExpandHeight;
				}
				catch (ObjectCollectedException)
				{
				}
			}
			_cachedExpandHeight = GUILayout.ExpandHeight(true);
			return _cachedExpandHeight;
		}
	}

	public static GUIStyle SidebarStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			if (_sidebarStyle == null)
			{
				_sidebarStyle = new GUIStyle(GUI.skin.box)
				{
					padding = CreateRectOffset(0, 0, 0, 0),
					margin = CreateRectOffset(0, 0, 0, 0)
				};
				_sidebarStyle.normal.background = MakeVerticalGradientTexture(256, 256, Theme.BgDarkB, Theme.BgDarkA);
			}
			else
			{
				GUIStyleState normal = _sidebarStyle.normal;
				if ((Object)(object)((normal != null) ? normal.background : null) == (Object)null)
				{
					_sidebarStyle.normal.background = MakeVerticalGradientTexture(256, 256, Theme.BgDarkB, Theme.BgDarkA);
				}
			}
			return _sidebarStyle;
		}
	}

	public static GUIStyle SeparatorStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			if (_separatorStyle == null)
			{
				_separatorStyle = new GUIStyle(GUI.skin.box)
				{
					fixedHeight = 2f,
					margin = CreateRectOffset(Spacing.MenuPad(10), Spacing.MenuPad(10), Spacing.MenuPad(5), Spacing.MenuPad(5)),
					stretchWidth = true
				};
				_separatorStyle.normal.background = MakeLaserTexture(128, 2, Theme.Primary);
			}
			return _separatorStyle;
		}
	}

	public static GUIStyle SidebarButtonStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			if (_sidebarButtonStyle == null)
			{
				_sidebarButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = Spacing.MenuFont(13),
					alignment = (TextAnchor)3,
					fixedHeight = Spacing.MenuSize(38f, 24f),
					padding = CreateRectOffset(0, Spacing.MenuPad(8), Spacing.MenuPad(6), Spacing.MenuPad(6)),
					margin = CreateRectOffset(0, 0, 1, 1),
					border = CreateRectOffset(0, 0, 0, 0),
					wordWrap = false,
					clipping = (TextClipping)1
				};
				_sidebarButtonStyle.normal.background = MakeTexture(2, 2, Color.clear);
				_sidebarButtonStyle.normal.textColor = Theme.TextMuted;
				_sidebarButtonStyle.hover.background = MakeTexture(2, 2, new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.15f));
				_sidebarButtonStyle.hover.textColor = Theme.TextPrimary;
				_sidebarButtonStyle.active.background = MakeTexture(2, 2, new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.25f));
				_sidebarButtonStyle.active.textColor = Color.white;
				_sidebarButtonStyle.richText = true;
			}
			return _sidebarButtonStyle;
		}
	}

	public static GUIStyle SidebarButtonActiveStyle
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			if (_sidebarButtonActiveStyle == null)
			{
				GUIStyle val = new GUIStyle(SidebarButtonStyle);
				val.normal.textColor = new Color(1f, 0.2f, 0.2f, 1f);
				_sidebarButtonActiveStyle = val;
				int num = 128;
				int num2 = 42;
				Texture2D val2 = new Texture2D(num, num2, (TextureFormat)4, false);
				Color32 val3 = Color32.op_Implicit(new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.15f));
				Color32 val4 = Color32.op_Implicit(new Color(1f, 0.1f, 0.1f, 1f));
				Color32[] array = (Color32[])(object)new Color32[num * num2];
				for (int i = 0; i < num2; i++)
				{
					for (int j = 0; j < num; j++)
					{
						if (j < 3)
						{
							array[i * num + j] = val4;
							continue;
						}
						float num3 = 1f - (float)j / ((float)num * 0.6f);
						num3 = Mathf.Clamp01(num3);
						array[i * num + j] = new Color32(val3.r, val3.g, val3.b, (byte)((float)(int)val3.a * num3));
					}
				}
				val2.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
				val2.Apply();
				((Object)val2).hideFlags = (HideFlags)61;
				_sidebarButtonActiveStyle.normal.background = val2;
				_sidebarButtonActiveStyle.hover.background = val2;
				_sidebarButtonActiveStyle.active.background = val2;
			}
			return _sidebarButtonActiveStyle;
		}
	}

	public static GUIStyle StatusPillStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Expected O, but got Unknown
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			if (_statusPillStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.label)
				{
					alignment = (TextAnchor)4,
					fontSize = Spacing.MenuFont(10),
					fontStyle = (FontStyle)1
				};
				val.normal.textColor = Theme.TextPrimary;
				val.padding = CreateRectOffset(Spacing.MenuPad(8), Spacing.MenuPad(8), Spacing.MenuPad(2), Spacing.MenuPad(2));
				val.margin = CreateRectOffset(0, 0, 0, 0);
				val.fixedHeight = Spacing.MenuSize(20f, 14f);
				_statusPillStyle = val;
				_statusPillStyle.normal.background = MakeGlowFrameTexture(32, 20, new Color(0.1f, 0.1f, 0.1f, 0.8f), Theme.Success, 2);
			}
			return _statusPillStyle;
		}
	}

	public static GUIStyle SidebarHeaderStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected O, but got Unknown
			if (_sidebarHeaderStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.label)
				{
					fontSize = Spacing.MenuFont(16),
					fontStyle = (FontStyle)1,
					alignment = (TextAnchor)4,
					padding = CreateRectOffset(0, 0, Spacing.MenuPad(15), Spacing.MenuPad(5))
				};
				val.normal.textColor = Theme.TextPrimary;
				_sidebarHeaderStyle = val;
				_sidebarHeaderStyle.richText = true;
			}
			return _sidebarHeaderStyle;
		}
	}

	public static GUIStyle SidebarFooterStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Expected O, but got Unknown
			if (_sidebarFooterStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.label)
				{
					fontSize = Spacing.MenuFont(10),
					alignment = (TextAnchor)7,
					padding = CreateRectOffset(Spacing.MenuPad(10), Spacing.MenuPad(10), Spacing.MenuPad(4), Spacing.MenuPad(10))
				};
				val.normal.textColor = Theme.TextMuted;
				_sidebarFooterStyle = val;
			}
			return _sidebarFooterStyle;
		}
	}

	public static GUIStyle DashboardCardStyle
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			if (_dashboardCardStyle == null)
			{
				_dashboardCardStyle = new GUIStyle(SectionStyle);
				_dashboardCardStyle.normal.background = MakeGlowFrameTexture(32, 32, Theme.BgSection, Theme.AccentDim, 1);
			}
			else
			{
				GUIStyleState normal = _dashboardCardStyle.normal;
				if ((Object)(object)((normal != null) ? normal.background : null) == (Object)null)
				{
					_dashboardCardStyle.normal.background = MakeGlowFrameTexture(32, 32, Theme.BgSection, Theme.AccentDim, 1);
				}
			}
			return _dashboardCardStyle;
		}
	}

	public static GUIStyle HeaderStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Expected O, but got Unknown
			if (_headerStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.label)
				{
					fontSize = Spacing.MenuFont(16),
					fontStyle = (FontStyle)1,
					alignment = (TextAnchor)3
				};
				val.normal.textColor = Theme.Accent;
				val.padding = CreateRectOffset(Spacing.MenuPad(8), Spacing.MenuPad(8), Spacing.MenuPad(6), Spacing.MenuPad(6));
				val.margin = CreateRectOffset(Spacing.MenuPad(4), Spacing.MenuPad(4), Spacing.MenuPad(2), Spacing.MenuPad(4));
				val.wordWrap = false;
				val.clipping = (TextClipping)1;
				_headerStyle = val;
				_headerStyle.richText = true;
			}
			return _headerStyle;
		}
	}

	public static GUIStyle SubHeaderStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Expected O, but got Unknown
			if (_subHeaderStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.label)
				{
					fontSize = Spacing.MenuFont(13),
					fontStyle = (FontStyle)1,
					alignment = (TextAnchor)3
				};
				val.normal.textColor = new Color(0.9f, 0.7f, 0.7f, 1f);
				val.padding = CreateRectOffset(Spacing.MenuPad(6), Spacing.MenuPad(6), Spacing.MenuPad(4), Spacing.MenuPad(4));
				val.margin = CreateRectOffset(Spacing.MenuPad(4), Spacing.MenuPad(4), Spacing.MenuPad(2), Spacing.MenuPad(2));
				val.wordWrap = false;
				val.clipping = (TextClipping)1;
				_subHeaderStyle = val;
				_subHeaderStyle.richText = true;
			}
			return _subHeaderStyle;
		}
	}

	public static GUIStyle ButtonStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Expected O, but got Unknown
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			//IL_0326: Unknown result type (might be due to invalid IL or missing references)
			//IL_0358: Unknown result type (might be due to invalid IL or missing references)
			if (_buttonStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.button)
				{
					fontSize = Spacing.MenuFont(13),
					alignment = (TextAnchor)4
				};
				val.normal.textColor = Theme.TextPrimary;
				val.padding = CreateRectOffset(Spacing.MenuPad(8), Spacing.MenuPad(8), Spacing.MenuPad(4), Spacing.MenuPad(4));
				val.margin = CreateRectOffset(Spacing.MenuPad(2), Spacing.MenuPad(2), 1, 1);
				val.fixedHeight = 0f;
				val.wordWrap = false;
				val.clipping = (TextClipping)1;
				_buttonStyle = val;
				Color innerTop = default(Color);
				(innerTop)._002Ector(0.14f, 0.14f, 0.16f, 1f);
				Color border = default(Color);
				(border)._002Ector(0.45f, 0.12f, 0.12f, 1f);
				_buttonStyle.normal.background = MakeFrameTexture(16, 64, innerTop, new Color(0.11f, 0.11f, 0.13f, 1f), border, 1);
				_buttonStyle.normal.textColor = new Color(0.92f, 0.92f, 0.92f, 1f);
				Color innerTop2 = default(Color);
				(innerTop2)._002Ector(0.22f, 0.08f, 0.08f, 1f);
				_buttonStyle.hover.background = MakeFrameTexture(16, 64, innerTop2, new Color(0.18f, 0.06f, 0.06f, 1f), Theme.Accent, 2);
				_buttonStyle.hover.textColor = Color.white;
				_buttonStyle.active.background = MakeFrameTexture(16, 64, new Color(0.5f, 0.05f, 0.05f, 1f), new Color(0.4f, 0.04f, 0.04f, 1f), new Color(0.7f, 0.1f, 0.1f, 1f), 1);
				_buttonStyle.active.textColor = Color.white;
				_buttonStyle.focused.background = _buttonStyle.hover.background;
				_buttonStyle.richText = true;
			}
			else
			{
				GUIStyleState normal = _buttonStyle.normal;
				if (!((Object)(object)((normal != null) ? normal.background : null) == (Object)null))
				{
					GUIStyleState hover = _buttonStyle.hover;
					if (!((Object)(object)((hover != null) ? hover.background : null) == (Object)null))
					{
						GUIStyleState active = _buttonStyle.active;
						if (!((Object)(object)((active != null) ? active.background : null) == (Object)null))
						{
							goto IL_0385;
						}
					}
				}
				Color val2 = default(Color);
				(val2)._002Ector(0.22f, 0.22f, 0.24f, 1f);
				Color border2 = default(Color);
				(border2)._002Ector(0.5f, 0.2f, 0.2f, 1f);
				_buttonStyle.normal.background = MakeFrameTexture(16, 64, val2, val2, border2, 1);
				_buttonStyle.hover.background = MakeFrameTexture(16, 64, new Color(0.28f, 0.18f, 0.18f, 1f), new Color(0.28f, 0.18f, 0.18f, 1f), Theme.Accent, 2);
				_buttonStyle.active.background = MakeTexture(16, 64, new Color(0.5f, 0.1f, 0.1f, 1f));
				_buttonStyle.focused.background = _buttonStyle.hover.background;
			}
			goto IL_0385;
			IL_0385:
			return _buttonStyle;
		}
	}

	public static GUIStyle ToggleStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Expected O, but got Unknown
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_025d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0277: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0303: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0335: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
			if (_toggleStyle == null)
			{
				_toggleStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = Spacing.MenuFont(12),
					fontStyle = (FontStyle)1,
					alignment = (TextAnchor)4,
					padding = CreateRectOffset(Spacing.MenuPad(8), Spacing.MenuPad(6), Spacing.MenuPad(3), Spacing.MenuPad(3)),
					margin = CreateRectOffset(Spacing.MenuPad(2), Spacing.MenuPad(2), 1, 1),
					fixedHeight = 0f,
					stretchWidth = true,
					wordWrap = false,
					clipping = (TextClipping)1
				};
				Color val = default(Color);
				(val)._002Ector(0.08f, 0.08f, 0.1f, 1f);
				Color border = default(Color);
				(border)._002Ector(0.22f, 0.22f, 0.26f, 1f);
				Color textColor = default(Color);
				(textColor)._002Ector(0.55f, 0.58f, 0.64f, 1f);
				Color val2 = default(Color);
				(val2)._002Ector(0.28f, 0.05f, 0.09f, 1f);
				Color border2 = default(Color);
				(border2)._002Ector(1f, 0.13f, 0.25f, 1f);
				Color textColor2 = default(Color);
				(textColor2)._002Ector(1f, 0.78f, 0.82f, 1f);
				_toggleStyle.normal.background = MakeFrameTexture(16, 64, val, val, border, 1);
				_toggleStyle.normal.textColor = textColor;
				_toggleStyle.onNormal.background = MakeFrameTexture(16, 64, val2, val2, border2, 1);
				_toggleStyle.onNormal.textColor = textColor2;
				_toggleStyle.hover.background = MakeFrameTexture(16, 64, new Color(0.15f, 0.15f, 0.17f, 1f), new Color(0.15f, 0.15f, 0.17f, 1f), new Color(0.4f, 0.4f, 0.45f, 1f), 1);
				_toggleStyle.hover.textColor = Theme.TextPrimary;
				_toggleStyle.onHover.background = MakeFrameTexture(16, 64, new Color(0.3f, 0.1f, 0.13f, 1f), new Color(0.3f, 0.1f, 0.13f, 1f), new Color(1f, 0.25f, 0.35f, 1f), 2);
				_toggleStyle.onHover.textColor = Color.white;
				_toggleStyle.active.background = MakeFrameTexture(16, 64, new Color(0.08f, 0.08f, 0.1f, 1f), new Color(0.08f, 0.08f, 0.1f, 1f), border, 1);
				_toggleStyle.active.textColor = Theme.TextPrimary;
				_toggleStyle.onActive.background = MakeFrameTexture(16, 64, new Color(0.35f, 0.12f, 0.15f, 1f), new Color(0.35f, 0.12f, 0.15f, 1f), new Color(1f, 0.3f, 0.4f, 1f), 2);
				_toggleStyle.onActive.textColor = Color.white;
				_toggleStyle.richText = true;
			}
			else
			{
				GUIStyleState normal = _toggleStyle.normal;
				if ((Object)(object)((normal != null) ? normal.background : null) == (Object)null)
				{
					_toggleStyle.normal.background = MakeTexture(2, 2, new Color(0.12f, 0.12f, 0.14f, 1f));
				}
			}
			return _toggleStyle;
		}
	}

	public static GUIStyle SliderStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			if (_sliderStyle == null)
			{
				_sliderStyle = new GUIStyle(GUI.skin.horizontalSlider)
				{
					margin = CreateRectOffset(Spacing.MenuPad(4), Spacing.MenuPad(4), Spacing.MenuPad(4), Spacing.MenuPad(4)),
					padding = CreateRectOffset(0, 0, 0, 0),
					fixedHeight = Spacing.MenuSize(6f, 4f),
					stretchWidth = true
				};
				_sliderStyle.normal.background = MakeRoundedTexture(100, 6, Theme.BgDarkA, Theme.AccentDim, 1);
			}
			return _sliderStyle;
		}
	}

	public static GUIStyle SliderThumbStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Expected O, but got Unknown
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			if (_sliderThumbStyle == null)
			{
				_sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
				{
					fixedWidth = Spacing.MenuSize(14f, 10f),
					fixedHeight = Spacing.MenuSize(14f, 10f),
					margin = CreateRectOffset(0, 0, 0, 0),
					padding = CreateRectOffset(0, 0, 0, 0)
				};
				if ((Object)(object)_sliderThumbTexture == (Object)null)
				{
					_sliderThumbTexture = MakeCircleTexture(14, Theme.Accent, Theme.Primary);
				}
				if ((Object)(object)_sliderThumbHoverTexture == (Object)null)
				{
					_sliderThumbHoverTexture = MakeCircleTexture(14, Theme.AccentHover, Theme.Gold);
				}
				_sliderThumbStyle.normal.background = _sliderThumbTexture;
				_sliderThumbStyle.hover.background = _sliderThumbHoverTexture;
				_sliderThumbStyle.active.background = _sliderThumbHoverTexture;
				_sliderThumbStyle.focused.background = _sliderThumbHoverTexture;
			}
			return _sliderThumbStyle;
		}
	}

	public static GUIStyle LabelStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			if (_labelStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.label)
				{
					fontSize = Spacing.MenuFont(14)
				};
				val.normal.textColor = Theme.TextPrimary;
				val.padding = CreateRectOffset(Spacing.MenuPad(10), Spacing.MenuPad(10), Spacing.MenuPad(6), Spacing.MenuPad(6));
				val.margin = CreateRectOffset(Spacing.MenuPad(6), Spacing.MenuPad(6), Spacing.MenuPad(3), Spacing.MenuPad(5));
				val.wordWrap = false;
				val.clipping = (TextClipping)1;
				_labelStyle = val;
				_labelStyle.richText = true;
			}
			return _labelStyle;
		}
	}

	public static GUIStyle TabStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Expected O, but got Unknown
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_0267: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			if (_tabStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.button)
				{
					fontSize = Spacing.MenuFont(13),
					padding = CreateRectOffset(Spacing.MenuPad(10), Spacing.MenuPad(10), Spacing.MenuPad(5), Spacing.MenuPad(5)),
					margin = CreateRectOffset(Spacing.MenuPad(3), Spacing.MenuPad(3), Spacing.MenuPad(2), Spacing.MenuPad(2)),
					fixedHeight = Spacing.MenuSize(28f, 20f)
				};
				val.normal.textColor = Theme.TextMuted;
				_tabStyle = val;
				_tabStyle.normal.background = MakeFrameTexture(16, 48, new Color(0.09f, 0.09f, 0.11f, 0.95f), new Color(0.07f, 0.07f, 0.09f, 0.95f), Theme.AccentDim, 1);
				_tabStyle.hover.background = MakeFrameTexture(16, 48, new Color(0.11f, 0.11f, 0.14f, 0.95f), new Color(0.09f, 0.09f, 0.12f, 0.95f), Theme.AccentHover, 1);
				_tabStyle.active.background = MakeFrameTexture(16, 48, new Color(0.12f, 0.03f, 0.06f, 0.95f), new Color(0.1f, 0.02f, 0.05f, 0.95f), Theme.AccentActive, 1);
				_tabStyle.richText = true;
			}
			else
			{
				GUIStyleState normal = _tabStyle.normal;
				if (!((Object)(object)((normal != null) ? normal.background : null) == (Object)null))
				{
					GUIStyleState hover = _tabStyle.hover;
					if (!((Object)(object)((hover != null) ? hover.background : null) == (Object)null))
					{
						GUIStyleState active = _tabStyle.active;
						if (!((Object)(object)((active != null) ? active.background : null) == (Object)null))
						{
							goto IL_02e5;
						}
					}
				}
				_tabStyle.normal.background = MakeFrameTexture(16, 48, new Color(0.09f, 0.09f, 0.11f, 0.95f), new Color(0.07f, 0.07f, 0.09f, 0.95f), Theme.AccentDim, 1);
				_tabStyle.hover.background = MakeFrameTexture(16, 48, new Color(0.11f, 0.11f, 0.14f, 0.95f), new Color(0.09f, 0.09f, 0.12f, 0.95f), Theme.AccentHover, 1);
				_tabStyle.active.background = MakeFrameTexture(16, 48, new Color(0.12f, 0.03f, 0.06f, 0.95f), new Color(0.1f, 0.02f, 0.05f, 0.95f), Theme.AccentActive, 1);
			}
			goto IL_02e5;
			IL_02e5:
			return _tabStyle;
		}
	}

	public static GUIStyle SelectedTabStyle
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			if (_selectedTabStyle == null)
			{
				GUIStyle val = new GUIStyle(TabStyle);
				val.normal.textColor = Theme.Accent;
				_selectedTabStyle = val;
				_selectedTabStyle.normal.background = MakeFrameTexture(16, 48, new Color(0.13f, 0.04f, 0.08f, 0.95f), new Color(0.11f, 0.03f, 0.07f, 0.95f), Theme.Accent, 1);
			}
			return _selectedTabStyle;
		}
	}

	public static GUIStyle ErrorStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Expected O, but got Unknown
			if (_errorStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.label)
				{
					fontSize = Spacing.MenuFont(14)
				};
				val.normal.textColor = Theme.Error;
				val.padding = CreateRectOffset(Spacing.MenuPad(12), Spacing.MenuPad(12), Spacing.MenuPad(10), Spacing.MenuPad(10));
				val.wordWrap = true;
				_errorStyle = val;
				_errorStyle.richText = true;
			}
			return _errorStyle;
		}
	}

	public static GUIStyle ContainerStyle
	{
		get
		{
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected O, but got Unknown
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			if (_containerStyle != null && _lastContainerBucket != GhostUI.CurrentBucket)
			{
				_containerStyle = null;
				_lastContainerBucket = GhostUI.CurrentBucket;
			}
			if (_containerStyle == null)
			{
				int num = ((GhostUI.CurrentBucket == LayoutBucket.Micro) ? 3 : ((GhostUI.CurrentBucket == LayoutBucket.Tight) ? 5 : 10));
				int num2 = ((num > 5) ? 8 : 4);
				_containerStyle = new GUIStyle(GUI.skin.box)
				{
					padding = CreateRectOffset(num, num, num2, num2),
					margin = CreateRectOffset(2, 2, 3, 3)
				};
				_containerStyle.normal.background = MakeGlowFrameTexture(32, 32, new Color(0.06f, 0.06f, 0.08f, 0.85f), new Color(0.3f, 0.1f, 0.1f, 0.15f), 1);
			}
			else
			{
				GUIStyleState normal = _containerStyle.normal;
				if ((Object)(object)((normal != null) ? normal.background : null) == (Object)null)
				{
					_containerStyle.normal.background = MakeTexture(2, 2, new Color(0.07f, 0.07f, 0.09f, 0.8f));
				}
			}
			return _containerStyle;
		}
	}

	public static GUIStyle SectionStyle
	{
		get
		{
			//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected O, but got Unknown
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			if (_sectionStyle != null && _lastSectionBucket != GhostUI.CurrentBucket)
			{
				_sectionStyle = null;
				_lastSectionBucket = GhostUI.CurrentBucket;
			}
			if (_sectionStyle == null)
			{
				int num = ((GhostUI.CurrentBucket == LayoutBucket.Micro) ? 4 : ((GhostUI.CurrentBucket == LayoutBucket.Tight) ? 6 : 12));
				int num2 = ((num > 6) ? 8 : 4);
				_sectionStyle = new GUIStyle(GUI.skin.box)
				{
					padding = CreateRectOffset(num, num, num2, num2),
					margin = CreateRectOffset(4, 4, 4, 4)
				};
				int num3 = 64;
				int num4 = 64;
				Texture2D val = new Texture2D(num3, num4, (TextureFormat)4, false);
				((Texture)val).wrapMode = (TextureWrapMode)1;
				((Texture)val).filterMode = (FilterMode)1;
				((Object)val).hideFlags = (HideFlags)61;
				Color32 val2 = Color32.op_Implicit(Theme.BgDarkB);
				Color32 val3 = Color32.op_Implicit(new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.7f));
				Color32 val4 = Color32.op_Implicit(new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.08f));
				Color32[] array = (Color32[])(object)new Color32[num3 * num4];
				for (int i = 0; i < num4; i++)
				{
					for (int j = 0; j < num3; j++)
					{
						if (j < 3)
						{
							array[i * num3 + j] = val3;
						}
						else if (j == num3 - 1 || i == 0 || i == num4 - 1)
						{
							array[i * num3 + j] = val4;
						}
						else
						{
							array[i * num3 + j] = val2;
						}
					}
				}
				val.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
				val.Apply();
				_sectionStyle.normal.background = val;
			}
			else
			{
				GUIStyleState normal = _sectionStyle.normal;
				if ((Object)(object)((normal != null) ? normal.background : null) == (Object)null)
				{
					_sectionStyle.normal.background = MakeGlowFrameTexture(32, 32, Theme.BgDarkB, Theme.AccentDim, 1);
				}
			}
			return _sectionStyle;
		}
	}

	public static GUIStyle IconStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			if (_iconStyle == null)
			{
				_iconStyle = new GUIStyle(GUI.skin.box)
				{
					padding = CreateRectOffset(3, 3, 3, 3),
					margin = CreateRectOffset(3, 3, 3, 3),
					fixedWidth = 28f,
					fixedHeight = 28f
				};
				_iconStyle.normal.background = MakeFrameTexture(8, 8, new Color(0.11f, 0.11f, 0.13f, 0.95f), new Color(0.09f, 0.09f, 0.11f, 0.95f), Theme.AccentDim, 1);
			}
			else
			{
				GUIStyleState normal = _iconStyle.normal;
				if ((Object)(object)((normal != null) ? normal.background : null) == (Object)null)
				{
					_iconStyle.normal.background = MakeFrameTexture(8, 8, new Color(0.11f, 0.11f, 0.13f, 0.95f), new Color(0.09f, 0.09f, 0.11f, 0.95f), Theme.AccentDim, 1);
				}
			}
			return _iconStyle;
		}
	}

	public static GUIStyle TooltipStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Expected O, but got Unknown
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			if (_tooltipStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.box)
				{
					fontSize = 12,
					richText = true,
					wordWrap = false
				};
				val.normal.textColor = Theme.TextPrimary;
				val.normal.background = MakeRoundedTexture(200, 40, new Color(0.05f, 0.05f, 0.07f, 0.95f), Theme.AccentDim, 1);
				val.padding = CreateRectOffset(8, 8, 6, 6);
				val.margin = CreateRectOffset(4, 4, 4, 4);
				val.border = CreateRectOffset(4, 4, 4, 4);
				_tooltipStyle = val;
			}
			else
			{
				GUIStyleState normal = _tooltipStyle.normal;
				if ((Object)(object)((normal != null) ? normal.background : null) == (Object)null)
				{
					_tooltipStyle.normal.background = MakeRoundedTexture(200, 40, new Color(0.05f, 0.05f, 0.07f, 0.95f), Theme.AccentDim, 1);
				}
			}
			return _tooltipStyle;
		}
	}

	public static GUIStyle StatusIndicatorStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			if (_statusIndicatorStyle == null)
			{
				_statusIndicatorStyle = new GUIStyle(GUI.skin.box)
				{
					fixedWidth = 12f,
					fixedHeight = 12f,
					margin = CreateRectOffset(6, 6, 4, 4)
				};
			}
			return _statusIndicatorStyle;
		}
	}

	public static GUIStyle GlowStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			if (_glowStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.box);
				val.normal.background = MakeTexture(2, 2, new Color(1f, 0f, 0.2f, 0.15f));
				val.margin = CreateRectOffset(3, 3, 3, 3);
				_glowStyle = val;
			}
			return _glowStyle;
		}
	}

	public static GUIStyle ShadowStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			if (_shadowStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.box);
				val.normal.background = MakeTexture(2, 2, new Color(0f, 0f, 0f, 0.6f));
				val.margin = CreateRectOffset(3, 3, 3, 3);
				_shadowStyle = val;
			}
			return _shadowStyle;
		}
	}

	public static GUIStyle HighlightStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			if (_highlightStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.box);
				val.normal.background = MakeTexture(2, 2, new Color(1f, 1f, 1f, 0.12f));
				val.margin = CreateRectOffset(3, 3, 3, 3);
				_highlightStyle = val;
			}
			return _highlightStyle;
		}
	}

	public static GUIStyle BetterToggleStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0249: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			//IL_0334: Unknown result type (might be due to invalid IL or missing references)
			//IL_034d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0366: Unknown result type (might be due to invalid IL or missing references)
			//IL_0394: Unknown result type (might be due to invalid IL or missing references)
			if (_betterToggleStyle == null)
			{
				_betterToggleStyle = new GUIStyle(GUI.skin.toggle)
				{
					fontSize = 15,
					fontStyle = (FontStyle)0,
					alignment = (TextAnchor)3,
					padding = CreateRectOffset(32, 18, 11, 11),
					margin = CreateRectOffset(10, 10, 6, 6),
					fixedHeight = 38f,
					stretchWidth = true
				};
				Color innerTop = default(Color);
				(innerTop)._002Ector(0.12f, 0.12f, 0.15f, 0.95f);
				Color innerBottom = default(Color);
				(innerBottom)._002Ector(0.08f, 0.08f, 0.1f, 0.95f);
				Color border = default(Color);
				(border)._002Ector(0.35f, 0.35f, 0.4f, 1f);
				Color innerTop2 = default(Color);
				(innerTop2)._002Ector(0.08f, 0.4f, 0.15f, 0.95f);
				Color innerBottom2 = default(Color);
				(innerBottom2)._002Ector(0.04f, 0.28f, 0.1f, 0.95f);
				Color border2 = default(Color);
				(border2)._002Ector(0.25f, 0.95f, 0.4f, 1f);
				_betterToggleStyle.normal.background = MakeFrameTexture(16, 64, innerTop, innerBottom, border, 2);
				_betterToggleStyle.normal.textColor = new Color(0.55f, 0.55f, 0.6f, 1f);
				_betterToggleStyle.onNormal.background = MakeFrameTexture(16, 64, innerTop2, innerBottom2, border2, 2);
				_betterToggleStyle.onNormal.textColor = new Color(0.9f, 1f, 0.9f, 1f);
				_betterToggleStyle.hover.background = MakeFrameTexture(16, 64, new Color(0.15f, 0.15f, 0.18f, 0.95f), new Color(0.11f, 0.11f, 0.14f, 0.95f), new Color(0.5f, 0.5f, 0.55f, 1f), 2);
				_betterToggleStyle.hover.textColor = Theme.TextPrimary;
				_betterToggleStyle.onHover.background = MakeFrameTexture(16, 64, new Color(0.1f, 0.5f, 0.18f, 0.95f), new Color(0.06f, 0.38f, 0.12f, 0.95f), new Color(0.35f, 1f, 0.5f, 1f), 2);
				_betterToggleStyle.onHover.textColor = new Color(0.95f, 1f, 0.95f, 1f);
				_betterToggleStyle.active.background = MakeFrameTexture(16, 64, new Color(0.1f, 0.1f, 0.12f, 0.95f), new Color(0.07f, 0.07f, 0.09f, 0.95f), new Color(0.4f, 0.4f, 0.45f, 1f), 2);
				_betterToggleStyle.active.textColor = Theme.TextMuted;
				_betterToggleStyle.onActive.background = MakeFrameTexture(16, 64, new Color(0.06f, 0.35f, 0.12f, 0.95f), new Color(0.03f, 0.22f, 0.08f, 0.95f), new Color(0.2f, 0.8f, 0.35f, 1f), 2);
				_betterToggleStyle.onActive.textColor = new Color(0.85f, 1f, 0.85f, 1f);
			}
			return _betterToggleStyle;
		}
	}

	public static GUIStyle WindowStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			if (_windowStyle == null)
			{
				_windowStyle = new GUIStyle(GUI.skin.box)
				{
					padding = CreateRectOffset(16, 16, 16, 16),
					margin = CreateRectOffset(0, 0, 0, 0)
				};
				_windowStyle.normal.background = MakeGlowFrameTexture(64, 64, Theme.BgDarkA, Theme.Glow, 1);
			}
			else
			{
				GUIStyleState normal = _windowStyle.normal;
				if ((Object)(object)((normal != null) ? normal.background : null) == (Object)null)
				{
					_windowStyle.normal.background = MakeGlowFrameTexture(64, 64, Theme.BgDarkA, Theme.Glow, 1);
				}
			}
			return _windowStyle;
		}
	}

	public static GUIStyle HeaderBackgroundStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			if (_headerBackgroundStyle == null)
			{
				_headerBackgroundStyle = new GUIStyle(GUI.skin.box)
				{
					padding = CreateRectOffset(0, 0, 0, 0),
					margin = CreateRectOffset(0, 0, 0, 0)
				};
				Color headerTop = Theme.HeaderTop;
				Color headerBottom = Theme.HeaderBottom;
				_headerBackgroundStyle.normal.background = MakeVerticalGradientTexture(2, 32, headerTop, headerBottom);
			}
			else
			{
				GUIStyleState normal = _headerBackgroundStyle.normal;
				if ((Object)(object)((normal != null) ? normal.background : null) == (Object)null)
				{
					Color headerTop2 = Theme.HeaderTop;
					Color headerBottom2 = Theme.HeaderBottom;
					_headerBackgroundStyle.normal.background = MakeVerticalGradientTexture(2, 32, headerTop2, headerBottom2);
				}
			}
			return _headerBackgroundStyle;
		}
	}

	public static GUIStyle TitleLabelStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Expected O, but got Unknown
			if (_titleLabelStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.label)
				{
					fontSize = 15,
					fontStyle = (FontStyle)1,
					alignment = (TextAnchor)4
				};
				val.normal.textColor = Theme.TextPrimary;
				_titleLabelStyle = val;
			}
			return _titleLabelStyle;
		}
	}

	public static GUIStyle TitleBarButtonStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0275: Unknown result type (might be due to invalid IL or missing references)
			//IL_027a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
			if (_titleBarButtonStyle == null)
			{
				_titleBarButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = 14,
					alignment = (TextAnchor)4,
					padding = CreateRectOffset(2, 2, 2, 2),
					margin = CreateRectOffset(4, 4, 4, 4),
					fixedWidth = 24f,
					fixedHeight = 20f
				};
				_titleBarButtonStyle.normal.textColor = Theme.TextMuted;
				_titleBarButtonStyle.normal.background = MakeFrameTexture(8, 32, new Color(0.13f, 0.13f, 0.15f, 0.95f), new Color(0.11f, 0.11f, 0.13f, 0.95f), Theme.AccentDim, 1);
				_titleBarButtonStyle.hover.textColor = Theme.Accent;
				_titleBarButtonStyle.hover.background = MakeFrameTexture(8, 32, new Color(0.15f, 0.05f, 0.1f, 0.95f), new Color(0.13f, 0.04f, 0.08f, 0.95f), Theme.AccentHover, 1);
				_titleBarButtonStyle.active.textColor = Color.white;
				_titleBarButtonStyle.active.background = MakeFrameTexture(8, 32, new Color(0.17f, 0.06f, 0.12f, 0.95f), new Color(0.15f, 0.05f, 0.1f, 0.95f), Theme.AccentActive, 1);
			}
			else
			{
				GUIStyleState normal = _titleBarButtonStyle.normal;
				if (!((Object)(object)((normal != null) ? normal.background : null) == (Object)null))
				{
					GUIStyleState hover = _titleBarButtonStyle.hover;
					if (!((Object)(object)((hover != null) ? hover.background : null) == (Object)null))
					{
						GUIStyleState active = _titleBarButtonStyle.active;
						if (!((Object)(object)((active != null) ? active.background : null) == (Object)null))
						{
							goto IL_02d9;
						}
					}
				}
				_titleBarButtonStyle.normal.background = MakeFrameTexture(8, 32, new Color(0.13f, 0.13f, 0.15f, 0.95f), new Color(0.11f, 0.11f, 0.13f, 0.95f), Theme.AccentDim, 1);
				_titleBarButtonStyle.hover.background = MakeFrameTexture(8, 32, new Color(0.15f, 0.05f, 0.1f, 0.95f), new Color(0.13f, 0.04f, 0.08f, 0.95f), Theme.AccentHover, 1);
				_titleBarButtonStyle.active.background = MakeFrameTexture(8, 32, new Color(0.17f, 0.06f, 0.12f, 0.95f), new Color(0.15f, 0.05f, 0.1f, 0.95f), Theme.AccentActive, 1);
			}
			goto IL_02d9;
			IL_02d9:
			return _titleBarButtonStyle;
		}
	}

	public static GUIStyle TextFieldStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected O, but got Unknown
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_025d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0294: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
			if (_textFieldStyle == null)
			{
				_textFieldStyle = new GUIStyle(GUI.skin.textField)
				{
					fontSize = 14,
					alignment = (TextAnchor)3,
					padding = CreateRectOffset(10, 10, 8, 8),
					margin = CreateRectOffset(6, 6, 6, 8)
				};
				_textFieldStyle.normal.textColor = Theme.TextPrimary;
				_textFieldStyle.normal.background = MakeFrameTexture(16, 48, new Color(0.08f, 0.08f, 0.1f, 0.95f), new Color(0.06f, 0.06f, 0.08f, 0.95f), Theme.AccentDim, 1);
				_textFieldStyle.hover.background = MakeFrameTexture(16, 48, new Color(0.1f, 0.1f, 0.12f, 0.95f), new Color(0.08f, 0.08f, 0.1f, 0.95f), Theme.AccentHover, 1);
				_textFieldStyle.focused.background = MakeFrameTexture(16, 48, new Color(0.11f, 0.02f, 0.05f, 0.95f), new Color(0.09f, 0.01f, 0.04f, 0.95f), Theme.Accent, 1);
				_textFieldStyle.focused.textColor = Theme.TextPrimary;
				_textFieldStyle.richText = true;
			}
			else
			{
				GUIStyleState normal = _textFieldStyle.normal;
				if (!((Object)(object)((normal != null) ? normal.background : null) == (Object)null))
				{
					GUIStyleState hover = _textFieldStyle.hover;
					if (!((Object)(object)((hover != null) ? hover.background : null) == (Object)null))
					{
						GUIStyleState focused = _textFieldStyle.focused;
						if (!((Object)(object)((focused != null) ? focused.background : null) == (Object)null))
						{
							goto IL_02c2;
						}
					}
				}
				_textFieldStyle.normal.background = MakeFrameTexture(16, 48, new Color(0.08f, 0.08f, 0.1f, 0.95f), new Color(0.06f, 0.06f, 0.08f, 0.95f), Theme.AccentDim, 1);
				_textFieldStyle.hover.background = MakeFrameTexture(16, 48, new Color(0.1f, 0.1f, 0.12f, 0.95f), new Color(0.08f, 0.08f, 0.1f, 0.95f), Theme.AccentHover, 1);
				_textFieldStyle.focused.background = MakeFrameTexture(16, 48, new Color(0.11f, 0.02f, 0.05f, 0.95f), new Color(0.09f, 0.01f, 0.04f, 0.95f), Theme.Accent, 1);
			}
			goto IL_02c2;
			IL_02c2:
			return _textFieldStyle;
		}
	}

	public static GUIStyle CrewToggleStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Expected O, but got Unknown
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			if (_crewToggleStyle == null)
			{
				_crewToggleStyle = new GUIStyle(GUI.skin.toggle)
				{
					fontSize = 14,
					fontStyle = (FontStyle)0,
					alignment = (TextAnchor)3,
					padding = CreateRectOffset(30, 14, 8, 8),
					margin = CreateRectOffset(8, 8, 4, 4),
					fixedHeight = 34f,
					stretchWidth = true
				};
				Color val = default(Color);
				(val)._002Ector(0.08f, 0.08f, 0.12f, 0.95f);
				Color border = default(Color);
				(border)._002Ector(0.3f, 0.3f, 0.38f, 1f);
				_crewToggleStyle.normal.background = MakeFrameTexture(16, 34, val, val, border, 2);
				_crewToggleStyle.normal.textColor = Theme.TextMuted;
				Color innerTop = default(Color);
				(innerTop)._002Ector(0.15f, 0.03f, 0.06f, 0.95f);
				Color innerBottom = default(Color);
				(innerBottom)._002Ector(0.1f, 0.02f, 0.04f, 0.95f);
				_crewToggleStyle.onNormal.background = MakeFrameTexture(16, 34, innerTop, innerBottom, Theme.Visor, 2);
				_crewToggleStyle.onNormal.textColor = Theme.TextPrimary;
				_crewToggleStyle.hover.background = MakeFrameTexture(16, 34, new Color(0.12f, 0.12f, 0.16f, 0.95f), new Color(0.1f, 0.1f, 0.14f, 0.95f), new Color(0.5f, 0.5f, 0.6f, 1f), 2);
				_crewToggleStyle.hover.textColor = Theme.TextPrimary;
				_crewToggleStyle.onHover.background = MakeFrameTexture(16, 34, new Color(0.18f, 0.04f, 0.08f, 0.95f), new Color(0.13f, 0.03f, 0.06f, 0.95f), Theme.Visor, 2);
				_crewToggleStyle.onHover.textColor = Theme.TextPrimary;
				_crewToggleStyle.richText = true;
			}
			return _crewToggleStyle;
		}
	}

	public static GUIStyle PremiumBadgeStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			if (_premiumBadgeStyle == null)
			{
				_premiumBadgeStyle = new GUIStyle(GUI.skin.box)
				{
					fontSize = 10,
					fontStyle = (FontStyle)1,
					alignment = (TextAnchor)4,
					padding = CreateRectOffset(6, 6, 2, 2),
					margin = CreateRectOffset(4, 4, 2, 2),
					fixedHeight = 18f
				};
				_premiumBadgeStyle.normal.textColor = new Color(0.1f, 0.08f, 0f, 1f);
				_premiumBadgeStyle.normal.background = MakeFrameTexture(12, 18, Theme.Gold, new Color(0.85f, 0.65f, 0f, 1f), new Color(1f, 0.9f, 0.4f, 1f), 1);
			}
			return _premiumBadgeStyle;
		}
	}

	public static GUIStyle FreeBadgeStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			if (_freeBadgeStyle == null)
			{
				_freeBadgeStyle = new GUIStyle(GUI.skin.box)
				{
					fontSize = 10,
					fontStyle = (FontStyle)0,
					alignment = (TextAnchor)4,
					padding = CreateRectOffset(6, 6, 2, 2),
					margin = CreateRectOffset(4, 4, 2, 2),
					fixedHeight = 18f
				};
				_freeBadgeStyle.normal.textColor = Theme.TextMuted;
				_freeBadgeStyle.normal.background = MakeFrameTexture(12, 18, new Color(0.15f, 0.15f, 0.18f, 0.95f), new Color(0.12f, 0.12f, 0.15f, 0.95f), Theme.FreeBorder, 1);
			}
			return _freeBadgeStyle;
		}
	}

	public static GUIStyle TimeRemainingStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Expected O, but got Unknown
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			if (_timeRemainingStyle == null)
			{
				_timeRemainingStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 11,
					fontStyle = (FontStyle)0,
					alignment = (TextAnchor)5,
					padding = CreateRectOffset(4, 8, 2, 2),
					margin = CreateRectOffset(2, 2, 0, 0)
				};
				_timeRemainingStyle.normal.textColor = Theme.Gold;
				_timeRemainingStyle.richText = true;
			}
			return _timeRemainingStyle;
		}
	}

	public static GUIStyle VisorAccentStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			if (_visorAccentStyle == null)
			{
				_visorAccentStyle = new GUIStyle(GUI.skin.label)
				{
					fontSize = 14,
					fontStyle = (FontStyle)1,
					alignment = (TextAnchor)3,
					padding = CreateRectOffset(8, 8, 4, 4)
				};
				_visorAccentStyle.normal.textColor = Theme.Visor;
				_visorAccentStyle.richText = true;
			}
			return _visorAccentStyle;
		}
	}

	public static GUIStyle PrimaryButtonStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Expected O, but got Unknown
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			if (_primaryButtonStyle == null)
			{
				_primaryButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = 16,
					fontStyle = (FontStyle)1,
					alignment = (TextAnchor)4,
					padding = CreateRectOffset(14, 14, 8, 8),
					margin = CreateRectOffset(6, 6, 6, 6),
					fixedHeight = 38f
				};
				_primaryButtonStyle.normal.background = MakeFrameTexture(16, 64, new Color(0.85f, 0.08f, 0.22f, 0.95f), new Color(0.65f, 0.06f, 0.18f, 0.95f), new Color(1f, 0.2f, 0.35f, 1f), 2);
				_primaryButtonStyle.normal.textColor = Color.white;
				_primaryButtonStyle.hover.background = MakeFrameTexture(16, 64, new Color(1f, 0.12f, 0.28f, 0.98f), new Color(0.78f, 0.08f, 0.22f, 0.98f), new Color(1f, 0.4f, 0.5f, 1f), 2);
				_primaryButtonStyle.hover.textColor = Color.white;
				_primaryButtonStyle.active.background = MakeFrameTexture(16, 64, new Color(0.55f, 0.04f, 0.14f, 0.98f), new Color(0.45f, 0.03f, 0.12f, 0.98f), new Color(0.8f, 0.15f, 0.25f, 1f), 2);
				_primaryButtonStyle.active.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
				_primaryButtonStyle.focused.background = _primaryButtonStyle.hover.background;
				_primaryButtonStyle.richText = true;
			}
			return _primaryButtonStyle;
		}
	}

	public static GUIStyle HostButtonStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Expected O, but got Unknown
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			if (_hostButtonStyle == null)
			{
				_hostButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = 14,
					fontStyle = (FontStyle)1,
					alignment = (TextAnchor)4,
					padding = CreateRectOffset(10, 10, 5, 5),
					margin = CreateRectOffset(4, 4, 3, 3),
					fixedHeight = 30f
				};
				_hostButtonStyle.normal.background = MakeFrameTexture(16, 64, new Color(0f, 0.7f, 0.8f, 0.25f), new Color(0f, 0.5f, 0.6f, 0.25f), Theme.Visor, 1);
				_hostButtonStyle.normal.textColor = Theme.Visor;
				_hostButtonStyle.hover.background = MakeFrameTexture(16, 64, new Color(0f, 0.8f, 0.9f, 0.35f), new Color(0f, 0.6f, 0.7f, 0.35f), new Color(0.2f, 1f, 1f, 1f), 2);
				_hostButtonStyle.hover.textColor = Color.white;
				_hostButtonStyle.active.background = MakeFrameTexture(16, 64, new Color(0f, 0.4f, 0.5f, 0.4f), new Color(0f, 0.3f, 0.4f, 0.4f), Theme.Visor, 1);
				_hostButtonStyle.active.textColor = Theme.Visor;
				_hostButtonStyle.richText = true;
			}
			return _hostButtonStyle;
		}
	}

	public static GUIStyle ItemStyle
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			if (_itemStyle == null)
			{
				_itemStyle = new GUIStyle(GUI.skin.box)
				{
					padding = CreateRectOffset(4, 4, 2, 2),
					margin = CreateRectOffset(2, 2, 1, 1)
				};
				_itemStyle.normal.background = MakeTexture(2, 2, new Color(0.08f, 0.08f, 0.1f, 0.8f));
			}
			return _itemStyle;
		}
	}

	public static GUIStyle SelectedItemStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			if (_selectedItemStyle == null)
			{
				_selectedItemStyle = new GUIStyle(GUI.skin.box)
				{
					padding = CreateRectOffset(4, 4, 2, 2),
					margin = CreateRectOffset(2, 2, 1, 1)
				};
				_selectedItemStyle.normal.background = MakeFrameTexture(16, 28, new Color(0.12f, 0.03f, 0.06f, 0.95f), new Color(0.1f, 0.02f, 0.05f, 0.95f), Theme.Accent, 2);
			}
			return _selectedItemStyle;
		}
	}

	public static GUIStyle ListButtonStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			if (_listButtonStyle == null)
			{
				_listButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontSize = 12,
					alignment = (TextAnchor)3,
					padding = CreateRectOffset(6, 6, 3, 3),
					margin = CreateRectOffset(1, 1, 0, 0),
					fixedHeight = 22f
				};
				_listButtonStyle.normal.textColor = Color.white;
				_listButtonStyle.fontStyle = (FontStyle)1;
				_listButtonStyle.normal.background = MakeTexture(new Color(0.1f, 0.1f, 0.12f, 0.6f));
				_listButtonStyle.hover.background = MakeFrameTexture(8, 22, new Color(0.14f, 0.14f, 0.17f, 0.8f), new Color(0.12f, 0.12f, 0.15f, 0.8f), Theme.AccentSoft, 1);
				_listButtonStyle.hover.textColor = Color.white;
				_listButtonStyle.active.background = MakeFrameTexture(8, 22, new Color(0.16f, 0.04f, 0.08f, 0.9f), new Color(0.12f, 0.03f, 0.06f, 0.9f), Theme.Accent, 1);
				_listButtonStyle.active.textColor = Color.white;
				_listButtonStyle.richText = true;
			}
			return _listButtonStyle;
		}
	}

	public static GUIStyle ScrollbarTrackStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Expected O, but got Unknown
			if (_scrollbarTrackStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.horizontalScrollbar)
				{
					fixedHeight = 6f,
					margin = CreateRectOffset(0, 0, 2, 2),
					padding = CreateRectOffset(0, 0, 0, 0)
				};
				val.normal.background = MakeTexture(1, 1, new Color(0.1f, 0.1f, 0.12f, 0.5f));
				val.hover.background = MakeTexture(1, 1, new Color(0.1f, 0.1f, 0.12f, 0.5f));
				val.active.background = MakeTexture(1, 1, new Color(0.1f, 0.1f, 0.12f, 0.5f));
				_scrollbarTrackStyle = val;
			}
			return _scrollbarTrackStyle;
		}
	}

	public static GUIStyle ScrollbarThumbStyle
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Expected O, but got Unknown
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			if (_scrollbarThumbStyle == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.horizontalScrollbarThumb)
				{
					fixedHeight = 6f,
					margin = CreateRectOffset(0, 0, 0, 0),
					padding = CreateRectOffset(2, 2, 0, 0)
				};
				val.normal.background = MakeTexture(1, 1, Theme.Accent);
				val.hover.background = MakeTexture(1, 1, Theme.AccentHover);
				val.active.background = MakeTexture(1, 1, Theme.AccentActive);
				_scrollbarThumbStyle = val;
				RectOffset border = CreateRectOffset(3, 3, 3, 3);
				_scrollbarThumbStyle.border = border;
			}
			else
			{
				GUIStyleState normal = _scrollbarThumbStyle.normal;
				if ((Object)(object)((normal != null) ? normal.background : null) == (Object)null)
				{
					_scrollbarThumbStyle.normal.background = MakeTexture(1, 1, Theme.Accent);
					_scrollbarThumbStyle.hover.background = MakeTexture(1, 1, Theme.AccentHover);
				}
			}
			return _scrollbarThumbStyle;
		}
	}

	public static GUIStyle ScrollbarThumbHoverStyle
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			if (_scrollbarThumbHoverStyle == null)
			{
				GUIStyle val = new GUIStyle(ScrollbarThumbStyle);
				val.normal.background = MakeTexture(1, 1, Theme.AccentHover);
				val.hover.background = MakeTexture(1, 1, new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, 0.9f));
				_scrollbarThumbHoverStyle = val;
				RectOffset border = CreateRectOffset(3, 3, 3, 3);
				_scrollbarThumbHoverStyle.border = border;
			}
			return _scrollbarThumbHoverStyle;
		}
	}

	private static GUIStyle WhitePixelBoxStyle
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (_whitePixelBoxStyle == null)
			{
				_whitePixelBoxStyle = new GUIStyle();
				_whitePixelBoxStyle.normal.background = MakeTexture(1, 1, Color.white);
			}
			return _whitePixelBoxStyle;
		}
	}

	private static GUIStyle RadialGlowBoxStyle
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			if (_radialGlowBoxStyle == null)
			{
				_radialGlowBoxStyle = new GUIStyle();
				_radialGlowBoxStyle.normal.background = GetRadialGlow();
			}
			return _radialGlowBoxStyle;
		}
	}

	private static GUIStyle PillBoxStyle
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			if (_pillBoxStyle == null)
			{
				_pillBoxStyle = new GUIStyle();
				_pillBoxStyle.normal.background = GetPillTexture();
			}
			return _pillBoxStyle;
		}
	}

	private static GUIStyle KnobBoxStyle
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			if (_knobBoxStyle == null)
			{
				_knobBoxStyle = new GUIStyle();
				_knobBoxStyle.normal.background = GetKnobTexture();
			}
			return _knobBoxStyle;
		}
	}

	public static GUIStyle ToggleLabelStyle
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if (_toggleLabelStyle == null)
			{
				_toggleLabelStyle = new GUIStyle(LabelStyle)
				{
					alignment = (TextAnchor)3
				};
				_toggleLabelStyle.normal.textColor = Theme.TextPrimary;
			}
			return _toggleLabelStyle;
		}
	}

	public static GUILayoutOption CachedWidth(float w)
	{
		if (_widthCache.TryGetValue(w, out var value))
		{
			try
			{
				_ = ((Il2CppObjectBase)value).Pointer;
				return value;
			}
			catch (ObjectCollectedException)
			{
			}
		}
		value = GUILayout.Width(w);
		_widthCache[w] = value;
		return value;
	}

	public static GUILayoutOption CachedHeight(float h)
	{
		if (_heightCache.TryGetValue(h, out var value))
		{
			try
			{
				_ = ((Il2CppObjectBase)value).Pointer;
				return value;
			}
			catch (ObjectCollectedException)
			{
			}
		}
		value = GUILayout.Height(h);
		_heightCache[h] = value;
		return value;
	}

	public static void ClearCache()
	{
		foreach (Texture2D value in _textureCache.Values)
		{
			if ((Object)(object)value != (Object)null)
			{
				Object.Destroy((Object)(object)value);
			}
		}
		_textureCache.Clear();
		if (Object.op_Implicit((Object)(object)_cachedPixelTexture))
		{
			Object.Destroy((Object)(object)_cachedPixelTexture);
		}
		if (Object.op_Implicit((Object)(object)_cachedPixelDarkTexture))
		{
			Object.Destroy((Object)(object)_cachedPixelDarkTexture);
		}
		if (Object.op_Implicit((Object)(object)_cachedPixelAccentTexture))
		{
			Object.Destroy((Object)(object)_cachedPixelAccentTexture);
		}
		if (Object.op_Implicit((Object)(object)_cachedPixelErrorTexture))
		{
			Object.Destroy((Object)(object)_cachedPixelErrorTexture);
		}
		if (Object.op_Implicit((Object)(object)_cachedSuccessTexture))
		{
			Object.Destroy((Object)(object)_cachedSuccessTexture);
		}
		if (Object.op_Implicit((Object)(object)_cachedErrorTexture))
		{
			Object.Destroy((Object)(object)_cachedErrorTexture);
		}
		_cachedPixelTexture = null;
		_cachedPixelDarkTexture = null;
		_cachedPixelAccentTexture = null;
		_cachedPixelErrorTexture = null;
		_cachedSuccessTexture = null;
		_cachedErrorTexture = null;
		_cachedSuccessIndicatorStyle = null;
		_cachedErrorIndicatorStyle = null;
		_cachedAnimatedHeaderStyle = null;
		if (Object.op_Implicit((Object)(object)_noiseTexture))
		{
			Object.Destroy((Object)(object)_noiseTexture);
		}
		_noiseTexture = null;
	}

	private static Texture2D MakeVerticalGradientTexture(int width, int height, Color top, Color bottom)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		string key = $"grad_{width}_{height}_{((object)(Color)top).GetHashCode()}_{((object)(Color)bottom).GetHashCode()}";
		if (_textureCache.ContainsKey(key) && (Object)(object)_textureCache[key] != (Object)null)
		{
			return _textureCache[key];
		}
		if (width < 1)
		{
			width = 1;
		}
		if (height < 2)
		{
			height = 2;
		}
		Texture2D val = new Texture2D(width, height, (TextureFormat)4, false);
		((Texture)val).wrapMode = (TextureWrapMode)1;
		((Texture)val).filterMode = (FilterMode)1;
		((Object)val).hideFlags = (HideFlags)61;
		Color[] array = (Color[])(object)new Color[width * height];
		for (int i = 0; i < height; i++)
		{
			float num = (float)i / (float)(height - 1);
			Color val2 = Color.Lerp(top, bottom, num);
			for (int j = 0; j < width; j++)
			{
				array[i * width + j] = val2;
			}
		}
		val.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
		val.Apply();
		_textureCache[key] = val;
		return val;
	}

	internal static Texture2D MakeFrameTexture(int width, int height, Color innerTop, Color innerBottom, Color border, int borderThickness)
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		string key = $"frame_{width}_{height}_{((object)(Color)innerTop).GetHashCode()}_{((object)(Color)innerBottom).GetHashCode()}_{((object)(Color)border).GetHashCode()}_{borderThickness}";
		if (_textureCache.ContainsKey(key) && (Object)(object)_textureCache[key] != (Object)null)
		{
			return _textureCache[key];
		}
		if (width < borderThickness * 2 + 1)
		{
			width = borderThickness * 2 + 1;
		}
		if (height < borderThickness * 2 + 2)
		{
			height = borderThickness * 2 + 2;
		}
		Texture2D val = new Texture2D(width, height, (TextureFormat)4, false);
		((Texture)val).wrapMode = (TextureWrapMode)1;
		((Texture)val).filterMode = (FilterMode)1;
		((Object)val).hideFlags = (HideFlags)61;
		float num = Mathf.Min(3f, (float)Mathf.Min(width, height) * 0.15f);
		Color32[] array = (Color32[])(object)new Color32[width * height];
		for (int i = 0; i < height; i++)
		{
			float num2 = (float)i / (float)(height - 1);
			Color val2 = Color.Lerp(innerTop, innerBottom, num2);
			for (int j = 0; j < width; j++)
			{
				float num3 = Mathf.Max(new float[3]
				{
					num - (float)j,
					(float)j - ((float)(width - 1) - num),
					0f
				});
				float num4 = Mathf.Max(new float[3]
				{
					num - (float)i,
					(float)i - ((float)(height - 1) - num),
					0f
				});
				float num5 = Mathf.Sqrt(num3 * num3 + num4 * num4) - num;
				if (num5 > 0.5f)
				{
					array[i * width + j] = new Color32((byte)0, (byte)0, (byte)0, (byte)0);
					continue;
				}
				float num6 = Mathf.Clamp01(0.5f - num5);
				Color val3 = ((j < borderThickness || j >= width - borderThickness || i < borderThickness || i >= height - borderThickness) ? border : val2);
				val3.a *= num6;
				array[i * width + j] = Color32.op_Implicit(val3);
			}
		}
		val.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
		val.Apply();
		_textureCache[key] = val;
		return val;
	}

	public static Texture2D MakeTexture(int width, int height, Color color)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		string key = $"solid_{width}_{height}_{((object)(Color)color).GetHashCode()}";
		if (_textureCache.ContainsKey(key) && (Object)(object)_textureCache[key] != (Object)null)
		{
			return _textureCache[key];
		}
		Texture2D val = new Texture2D(width, height, (TextureFormat)4, false);
		((Texture)val).wrapMode = (TextureWrapMode)1;
		((Texture)val).filterMode = (FilterMode)1;
		((Object)val).hideFlags = (HideFlags)61;
		Color32 val2 = Color32.op_Implicit(color);
		Color32[] array = (Color32[])(object)new Color32[width * height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = val2;
		}
		val.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
		val.Apply();
		_textureCache[key] = val;
		return val;
	}

	private static Texture2D MakeTexture(Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (color == Theme.BgDarkA)
		{
			if ((Object)(object)_cachedPixelTexture == (Object)null)
			{
				_cachedPixelTexture = CreatePixelTexture(color);
			}
			return _cachedPixelTexture;
		}
		if (color == Theme.BgDarkB)
		{
			if ((Object)(object)_cachedPixelDarkTexture == (Object)null)
			{
				_cachedPixelDarkTexture = CreatePixelTexture(color);
			}
			return _cachedPixelDarkTexture;
		}
		if (color == Theme.Accent)
		{
			if ((Object)(object)_cachedPixelAccentTexture == (Object)null)
			{
				_cachedPixelAccentTexture = CreatePixelTexture(color);
			}
			return _cachedPixelAccentTexture;
		}
		if (color == Theme.Error)
		{
			if ((Object)(object)_cachedPixelErrorTexture == (Object)null)
			{
				_cachedPixelErrorTexture = CreatePixelTexture(color);
			}
			return _cachedPixelErrorTexture;
		}
		string key = $"pixel_{((object)(Color)color).GetHashCode()}";
		if (_textureCache.ContainsKey(key) && (Object)(object)_textureCache[key] != (Object)null)
		{
			return _textureCache[key];
		}
		Texture2D val = CreatePixelTexture(color);
		_textureCache[key] = val;
		return val;
	}

	private static Texture2D CreatePixelTexture(Color color)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(1, 1, (TextureFormat)4, false);
		val.SetPixels32(Il2CppStructArray<Color32>.op_Implicit((Color32[])(object)new Color32[1] { Color32.op_Implicit(color) }));
		((Texture)val).filterMode = (FilterMode)1;
		((Object)val).hideFlags = (HideFlags)61;
		val.Apply();
		return val;
	}

	public static Texture2D MakeCyberTexture(int w, int h, Color baseCol)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		string key = $"cyber_{w}_{h}_{((object)(Color)baseCol).GetHashCode()}";
		if (_textureCache.ContainsKey(key) && (Object)(object)_textureCache[key] != (Object)null)
		{
			return _textureCache[key];
		}
		if (w > 64)
		{
			w = 64;
		}
		if (h > 64)
		{
			h = 64;
		}
		if (w < 4)
		{
			w = 4;
		}
		if (h < 4)
		{
			h = 4;
		}
		Texture2D val = new Texture2D(w, h, (TextureFormat)4, false);
		((Texture)val).wrapMode = (TextureWrapMode)1;
		((Texture)val).filterMode = (FilterMode)1;
		((Object)val).hideFlags = (HideFlags)61;
		Color[] array = (Color[])(object)new Color[w * h];
		float num = (float)w / 2f;
		float num2 = (float)h / 2f;
		int num3 = _cyberNoiseSeed;
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				Color val2 = baseCol;
				if (i % 3 == 0)
				{
					val2.a *= 0.85f;
				}
				float num4 = ((float)j - num) / num;
				float num5 = ((float)i - num2) / num2;
				float num6 = Mathf.Sqrt(num4 * num4 + num5 * num5);
				float num7 = 1f - num6 * 0.21f;
				num7 = Mathf.Clamp01(num7);
				val2.r *= num7;
				val2.g *= num7;
				val2.b *= num7;
				num3 = (num3 * 1103515245 + 12345) & 0x7FFFFFFF;
				float num8 = ((float)(num3 % 1000) / 1000f - 0.5f) * 0.04f;
				val2.r = Mathf.Clamp01(val2.r + num8);
				val2.g = Mathf.Clamp01(val2.g + num8);
				val2.b = Mathf.Clamp01(val2.b + num8);
				if (j == 0 || i == h - 1)
				{
					val2.r = Mathf.Min(1f, val2.r * 1.2f);
					val2.g = Mathf.Min(1f, val2.g * 1.2f);
					val2.b = Mathf.Min(1f, val2.b * 1.2f);
				}
				else if (j == w - 1 || i == 0)
				{
					val2.r *= 0.5f;
					val2.g *= 0.5f;
					val2.b *= 0.5f;
				}
				array[i * w + j] = val2;
			}
		}
		val.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
		val.Apply();
		_textureCache[key] = val;
		return val;
	}

	public static void DrawOutlinedLabel(Rect r, string content, GUIStyle s)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		if (s != null && !string.IsNullOrEmpty(content))
		{
			Color textColor = s.normal.textColor;
			Color textColor2 = default(Color);
			(textColor2)._002Ector(0f, 0f, 0f, 0.9f);
			s.normal.textColor = textColor2;
			GUI.Label(new Rect((r).x - 1f, (r).y - 1f, (r).width, (r).height), content, s);
			GUI.Label(new Rect((r).x - 1f, (r).y + 1f, (r).width, (r).height), content, s);
			GUI.Label(new Rect((r).x + 1f, (r).y - 1f, (r).width, (r).height), content, s);
			GUI.Label(new Rect((r).x + 1f, (r).y + 1f, (r).width, (r).height), content, s);
			s.normal.textColor = textColor;
			GUI.Label(r, content, s);
		}
	}

	public static bool DrawCyberButton(Rect r, string label, GUIStyle s)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Expected O, but got Unknown
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		if (s == null)
		{
			return false;
		}
		bool num = (r).Contains(Event.current.mousePosition);
		bool flag = num && Input.GetMouseButton(0);
		Rect val = r;
		if (flag)
		{
			(val).x = (val).x + 1f;
			(val).y = (val).y + 2f;
		}
		if (num && !flag)
		{
			if ((Object)(object)_cachedHoverGlowTexture == (Object)null)
			{
				_cachedHoverGlowTexture = MakeTexture(4, 4, new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, 0.15f));
			}
			Color color = GUI.color;
			Color accent = Theme.Accent;
			for (int num2 = 3; num2 >= 1; num2--)
			{
				float num3 = (float)num2 * 3f;
				GUI.color = new Color(accent.r, accent.g, accent.b, 0.05f * (float)num2);
				GUI.Box(new Rect((r).x - num3, (r).y - num3, (r).width + num3 * 2f, (r).height + num3 * 2f), GUIContent.none, WhitePixelBoxStyle);
			}
			GUI.color = color;
			if (_hoverGlowBoxStyle == null || (Object)(object)_hoverGlowBoxStyle.normal.background != (Object)(object)_cachedHoverGlowTexture)
			{
				_hoverGlowBoxStyle = new GUIStyle();
				_hoverGlowBoxStyle.normal.background = _cachedHoverGlowTexture;
			}
			GUI.Box(new Rect((r).x - 2f, (r).y - 2f, (r).width + 4f, (r).height + 4f), GUIContent.none, _hoverGlowBoxStyle);
		}
		return GUI.Button(val, label, s);
	}

	private static Texture2D MakeGlowFrameTexture(int width, int height, Color bgColor, Color glowColor, int thickness)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		string key = $"glowframe_{width}_{height}_{((object)(Color)bgColor).GetHashCode()}_{((object)(Color)glowColor).GetHashCode()}_{thickness}";
		if (_textureCache.ContainsKey(key) && (Object)(object)_textureCache[key] != (Object)null)
		{
			return _textureCache[key];
		}
		Texture2D val = new Texture2D(width, height, (TextureFormat)4, false);
		((Texture)val).wrapMode = (TextureWrapMode)1;
		Color32 val2 = Color32.op_Implicit(bgColor);
		Color32.op_Implicit(glowColor);
		Color32[] array = (Color32[])(object)new Color32[width * height];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				int val3 = j;
				int val4 = width - 1 - j;
				int val5 = i;
				int val6 = height - 1 - i;
				int num = Math.Min(Math.Min(val3, val4), Math.Min(val5, val6));
				if (num < thickness)
				{
					float num2 = (float)num / (float)thickness;
					array[i * width + j] = Color32.op_Implicit(Color.Lerp(glowColor, bgColor, num2));
				}
				else
				{
					array[i * width + j] = val2;
				}
			}
		}
		val.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
		val.Apply();
		_textureCache[key] = val;
		return val;
	}

	public static Texture2D MakeGlassTexture(int width, int height, Color baseColor)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		string key = $"glass_cyber_{width}_{height}_{((object)(Color)baseColor).GetHashCode()}";
		if (_textureCache.ContainsKey(key) && (Object)(object)_textureCache[key] != (Object)null)
		{
			return _textureCache[key];
		}
		Texture2D val = new Texture2D(width, height, (TextureFormat)4, false);
		((Texture)val).wrapMode = (TextureWrapMode)1;
		((Texture)val).filterMode = (FilterMode)1;
		Color[] array = (Color[])(object)new Color[width * height];
		for (int i = 0; i < height; i++)
		{
			float num = (float)i / (float)height;
			float num2 = 0.9f + num * 0.1f;
			for (int j = 0; j < width; j++)
			{
				Color val2 = baseColor;
				if ((j + i) % 4 == 0)
				{
					val2.r *= 0.95f;
					val2.g *= 0.95f;
					val2.b *= 0.95f;
				}
				if (i >= height - 2)
				{
					val2 += new Color(0.1f, 0.1f, 0.1f, 0f);
				}
				val2.a *= num2;
				array[i * width + j] = val2;
			}
		}
		val.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
		val.Apply();
		_textureCache[key] = val;
		return val;
	}

	public static Texture2D GetGradientTexture()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_gradientTexture != (Object)null)
		{
			return _gradientTexture;
		}
		int num = 128;
		int num2 = 4;
		_gradientTexture = new Texture2D(num, num2, (TextureFormat)4, false);
		((Texture)_gradientTexture).wrapMode = (TextureWrapMode)1;
		((Texture)_gradientTexture).filterMode = (FilterMode)1;
		Color primary = Theme.Primary;
		Color32[] array = (Color32[])(object)new Color32[num * num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float num3 = (float)j / (float)num;
				byte b = (byte)(((num3 < 0.5f) ? (num3 * 2f) : ((1f - num3) * 2f)) * 0.8f * 255f);
				array[i * num + j] = new Color32((byte)(primary.r * 255f), (byte)(primary.g * 255f), (byte)(primary.b * 255f), b);
			}
		}
		_gradientTexture.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
		_gradientTexture.Apply();
		((Object)_gradientTexture).hideFlags = (HideFlags)61;
		return _gradientTexture;
	}

	internal static RectOffset CreateRectOffset(int left, int right, int top, int bottom)
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

	public static void InvalidateIfMenuScaleChanged()
	{
		float menuScale = Spacing.MenuScale;
		if (!(Mathf.Abs(menuScale - _lastStyleMenuScale) < 0.03f))
		{
			_lastStyleMenuScale = menuScale;
			_headerStyle = null;
			_subHeaderStyle = null;
			_buttonStyle = null;
			_toggleStyle = null;
			_labelStyle = null;
			_tabStyle = null;
			_selectedTabStyle = null;
			_containerStyle = null;
			_sectionStyle = null;
			_errorStyle = null;
			_separatorStyle = null;
			_statusPillStyle = null;
			_sidebarButtonStyle = null;
			_sidebarButtonActiveStyle = null;
			_sidebarHeaderStyle = null;
			_sidebarFooterStyle = null;
			_titleLabelStyle = null;
			_titleBarButtonStyle = null;
			_sliderStyle = null;
			_sliderThumbStyle = null;
			_sliderLabelStyle = null;
			_sliderMinMaxStyle = null;
			_listButtonStyle = null;
			_itemStyle = null;
			_selectedItemStyle = null;
		}
	}

	public static void InvalidateAllStyles()
	{
		ClearCache();
		_headerStyle = null;
		_subHeaderStyle = null;
		_buttonStyle = null;
		_toggleStyle = null;
		_sliderStyle = null;
		_labelStyle = null;
		_tabStyle = null;
		_selectedTabStyle = null;
		_containerStyle = null;
		_sectionStyle = null;
		_errorStyle = null;
		_iconStyle = null;
		_tooltipStyle = null;
		_statusIndicatorStyle = null;
		_glowStyle = null;
		_shadowStyle = null;
		_highlightStyle = null;
		_separatorStyle = null;
		_betterToggleStyle = null;
		_windowStyle = null;
		_headerBackgroundStyle = null;
		_titleLabelStyle = null;
		_titleBarButtonStyle = null;
		_textFieldStyle = null;
		_crewToggleStyle = null;
		_premiumBadgeStyle = null;
		_freeBadgeStyle = null;
		_timeRemainingStyle = null;
		_visorAccentStyle = null;
		_primaryButtonStyle = null;
		_hostButtonStyle = null;
		_sidebarStyle = null;
		_sidebarButtonStyle = null;
		_sidebarButtonActiveStyle = null;
		_sidebarHeaderStyle = null;
		_sidebarFooterStyle = null;
		_dashboardCardStyle = null;
		_itemStyle = null;
		_selectedItemStyle = null;
		_listButtonStyle = null;
		_scrollbarTrackStyle = null;
		_scrollbarThumbStyle = null;
		_scrollbarThumbHoverStyle = null;
		_statusPillStyle = null;
		_sliderThumbStyle = null;
		_sliderLabelStyle = null;
		_sliderValueStyle = null;
		_sliderMinMaxStyle = null;
		_crewLogoStyleRed = null;
		_crewLogoStyleCyan = null;
		_eliteStatusActiveStyle = null;
		_eliteStatusInactiveStyle = null;
		_whitePixelBoxStyle = null;
		_radialGlowBoxStyle = null;
		_pillBoxStyle = null;
		_knobBoxStyle = null;
		_particleBoxStyle = null;
		_hoverGlowBoxStyle = null;
		_tooltipBgBoxStyle = null;
		_dropShadowBoxStyle = null;
		_animBgBoxStyleA = null;
		_animBgBoxStyleB = null;
		_scanlineBoxStyle = null;
		_eliteDotBoxStyle = null;
		_toggleLabelStyle = null;
		if (Object.op_Implicit((Object)(object)_cachedHoverGlowTexture))
		{
			Object.Destroy((Object)(object)_cachedHoverGlowTexture);
			_cachedHoverGlowTexture = null;
		}
		if (Object.op_Implicit((Object)(object)_gradientTexture))
		{
			Object.Destroy((Object)(object)_gradientTexture);
			_gradientTexture = null;
		}
		if (Object.op_Implicit((Object)(object)_sliderThumbTexture))
		{
			Object.Destroy((Object)(object)_sliderThumbTexture);
			_sliderThumbTexture = null;
		}
		if (Object.op_Implicit((Object)(object)_sliderThumbHoverTexture))
		{
			Object.Destroy((Object)(object)_sliderThumbHoverTexture);
			_sliderThumbHoverTexture = null;
		}
		if (Object.op_Implicit((Object)(object)_separatorTexture))
		{
			Object.Destroy((Object)(object)_separatorTexture);
			_separatorTexture = null;
		}
		if (Object.op_Implicit((Object)(object)_radialGlowTex))
		{
			Object.Destroy((Object)(object)_radialGlowTex);
			_radialGlowTex = null;
		}
		if (Object.op_Implicit((Object)(object)_pillTex))
		{
			Object.Destroy((Object)(object)_pillTex);
			_pillTex = null;
		}
		if (Object.op_Implicit((Object)(object)_knobTex))
		{
			Object.Destroy((Object)(object)_knobTex);
			_knobTex = null;
		}
		if (Object.op_Implicit((Object)(object)_dropShadowTex))
		{
			Object.Destroy((Object)(object)_dropShadowTex);
			_dropShadowTex = null;
		}
		if (Object.op_Implicit((Object)(object)_animBgA))
		{
			Object.Destroy((Object)(object)_animBgA);
			_animBgA = null;
		}
		if (Object.op_Implicit((Object)(object)_animBgB))
		{
			Object.Destroy((Object)(object)_animBgB);
			_animBgB = null;
		}
		if (Object.op_Implicit((Object)(object)_scanlineTileTex))
		{
			Object.Destroy((Object)(object)_scanlineTileTex);
			_scanlineTileTex = null;
		}
		if (Object.op_Implicit((Object)(object)_eliteTooltipBgTex))
		{
			Object.Destroy((Object)(object)_eliteTooltipBgTex);
			_eliteTooltipBgTex = null;
		}
		GlowSystem.ClearGlowCache();
		VisualEffects.ClearEffectsCache();
		try
		{
			GhostUI.InvalidateThemeStyles();
		}
		catch
		{
		}
		try
		{
			MiscTab.ThemeInvalidated = true;
		}
		catch
		{
		}
	}

	private static Texture2D MakeLaserTexture(int width, int height, Color color)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		string key = $"laser_{width}_{height}_{((object)(Color)color).GetHashCode()}";
		if (_textureCache.ContainsKey(key) && (Object)(object)_textureCache[key] != (Object)null)
		{
			return _textureCache[key];
		}
		Texture2D val = new Texture2D(width, height, (TextureFormat)4, false);
		((Texture)val).wrapMode = (TextureWrapMode)1;
		Color[] array = (Color[])(object)new Color[width * height];
		for (int i = 0; i < width; i++)
		{
			float num = Mathf.Sin((float)i / (float)(width - 1) * (float)Math.PI);
			for (int j = 0; j < height; j++)
			{
				Color val2 = color;
				val2.a *= num;
				array[j * width + i] = val2;
			}
		}
		val.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
		val.Apply();
		_textureCache[key] = val;
		return val;
	}

	public static Texture2D MakeNoiseTexture(int w, int h, float intensity = 0.03f)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		string key = $"noise_{w}_{h}_{intensity}";
		if (_textureCache.TryGetValue(key, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		if (w > 64)
		{
			w = 64;
		}
		if (h > 64)
		{
			h = 64;
		}
		if (w < 4)
		{
			w = 4;
		}
		if (h < 4)
		{
			h = 4;
		}
		Texture2D val = new Texture2D(w, h, (TextureFormat)4, false);
		((Texture)val).wrapMode = (TextureWrapMode)0;
		((Texture)val).filterMode = (FilterMode)1;
		((Object)val).hideFlags = (HideFlags)61;
		int num = 42;
		Color32[] array = (Color32[])(object)new Color32[w * h];
		for (int i = 0; i < array.Length; i++)
		{
			num = (num * 1103515245 + 12345) & 0x7FFFFFFF;
			float num2 = ((float)(num % 1000) / 1000f - 0.5f) * 2f * intensity;
			byte b = (byte)Mathf.Clamp(128f + num2 * 128f, 0f, 255f);
			array[i] = new Color32(b, b, b, (byte)1);
		}
		val.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
		val.Apply();
		_textureCache[key] = val;
		return val;
	}

	public static Texture2D MakeRadialGradientTexture(int w, int h, Color center, Color edge)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		string key = $"radial_{w}_{h}_{((object)(Color)center).GetHashCode()}_{((object)(Color)edge).GetHashCode()}";
		if (_textureCache.TryGetValue(key, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		if (w > 64)
		{
			w = 64;
		}
		if (h > 64)
		{
			h = 64;
		}
		if (w < 4)
		{
			w = 4;
		}
		if (h < 4)
		{
			h = 4;
		}
		Texture2D val = new Texture2D(w, h, (TextureFormat)4, false);
		((Texture)val).wrapMode = (TextureWrapMode)1;
		((Texture)val).filterMode = (FilterMode)1;
		((Object)val).hideFlags = (HideFlags)61;
		float num = (float)w / 2f;
		float num2 = (float)h / 2f;
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		Color32[] array = (Color32[])(object)new Color32[w * h];
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				float num4 = ((float)j - num) / num;
				float num5 = ((float)i - num2) / num2;
				float num6 = Mathf.Sqrt(num4 * num4 + num5 * num5) / num3;
				num6 = Mathf.Clamp01(num6);
				Color val2 = Color.Lerp(center, edge, num6);
				array[i * w + j] = Color32.op_Implicit(val2);
			}
		}
		val.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
		val.Apply();
		_textureCache[key] = val;
		return val;
	}

	public static void DrawAccentSeparator(float height = 1f, float alpha = 0.8f)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type == 7)
		{
			GUILayout.Space(4f);
			float activePulse = GlowSystem.GetActivePulse();
			Color color = default(Color);
			(color)._002Ector(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, alpha * activePulse);
			Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.ExpandWidth(true),
				GUILayout.Height(height)
			});
			Color color2 = GUI.color;
			GUI.color = color;
			GUI.Box(rect, GUIContent.none, WhitePixelBoxStyle);
			GUI.color = color2;
			GUILayout.Space(4f);
		}
	}

	public static void DrawLaserLine(float height = 2f, float alpha = 0.8f, bool dashed = false)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type != 7)
		{
			return;
		}
		float activePulse = GlowSystem.GetActivePulse();
		Color color = default(Color);
		(color)._002Ector(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, alpha * activePulse);
		Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.ExpandWidth(true),
			GUILayout.Height(height)
		});
		if (dashed)
		{
			float num = 8f;
			float num2 = 4f;
			float num3 = (rect).x;
			Color color2 = GUI.color;
			GUI.color = color;
			for (; num3 < (rect).xMax; num3 += num + num2)
			{
				float num4 = Mathf.Min(num3 + num, (rect).xMax);
				GUI.Box(new Rect(num3, (rect).y, num4 - num3, height), GUIContent.none, WhitePixelBoxStyle);
			}
			GUI.color = color2;
		}
		else
		{
			Color color3 = GUI.color;
			GUI.color = color;
			GUI.Box(rect, GUIContent.none, WhitePixelBoxStyle);
			GUI.color = color3;
		}
	}

	public static Texture2D GetNoiseOverlay()
	{
		if ((Object)(object)_noiseTexture == (Object)null)
		{
			_noiseTexture = MakeNoiseTexture(64, 64);
		}
		return _noiseTexture;
	}

	internal static Texture2D MakeCircleTexture(int size, Color centerColor, Color edgeColor)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(size, size, (TextureFormat)4, false);
		((Texture)val).filterMode = (FilterMode)1;
		((Texture)val).wrapMode = (TextureWrapMode)1;
		((Object)val).hideFlags = (HideFlags)61;
		float num = (float)size / 2f;
		Color[] array = (Color[])(object)new Color[size * size];
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float num2 = (float)j - num + 0.5f;
				float num3 = (float)i - num + 0.5f;
				float num4 = Mathf.Sqrt(num2 * num2 + num3 * num3);
				if (num4 <= num)
				{
					float num5 = num4 / num;
					Color val2 = Color.Lerp(centerColor, edgeColor, num5 * 0.6f);
					if (num4 > num - 1f)
					{
						val2.a *= num - num4;
					}
					array[i * size + j] = val2;
				}
				else
				{
					array[i * size + j] = Color.clear;
				}
			}
		}
		val.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
		val.Apply();
		return val;
	}

	private static Texture2D MakeRoundedTexture(int width, int height, Color fillColor, Color borderColor, int borderWidth)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(width, height, (TextureFormat)4, false);
		((Texture)val).filterMode = (FilterMode)1;
		((Texture)val).wrapMode = (TextureWrapMode)1;
		((Object)val).hideFlags = (HideFlags)61;
		float num = (float)height / 2f;
		Color[] array = (Color[])(object)new Color[width * height];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				bool flag = false;
				float num2 = 0f;
				if ((float)j < num)
				{
					float num3 = (float)j - num;
					float num4 = (float)i - num;
					float num5 = Mathf.Sqrt(num3 * num3 + num4 * num4);
					flag = num5 <= num;
					num2 = num - num5;
				}
				else if ((float)j >= (float)width - num)
				{
					float num6 = (float)j - ((float)width - num);
					float num7 = (float)i - num;
					float num8 = Mathf.Sqrt(num6 * num6 + num7 * num7);
					flag = num8 <= num;
					num2 = num - num8;
				}
				else
				{
					flag = true;
					num2 = Mathf.Min(i, height - 1 - i);
				}
				if (flag)
				{
					Color val2 = ((num2 < (float)borderWidth) ? borderColor : fillColor);
					array[i * width + j] = val2;
				}
				else
				{
					array[i * width + j] = Color.clear;
				}
			}
		}
		val.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
		val.Apply();
		return val;
	}

	public static void EnsureInitialized()
	{
		_ = WindowStyle;
		_ = HeaderBackgroundStyle;
		_ = TitleLabelStyle;
		_ = TitleBarButtonStyle;
		_ = HeaderStyle;
		_ = SubHeaderStyle;
		_ = ButtonStyle;
		_ = ToggleStyle;
		_ = SliderStyle;
		_ = LabelStyle;
		_ = TabStyle;
		_ = SelectedTabStyle;
		_ = ContainerStyle;
		_ = SectionStyle;
		_ = ErrorStyle;
		_ = IconStyle;
		_ = TooltipStyle;
		_ = StatusIndicatorStyle;
		_ = GlowStyle;
		_ = ShadowStyle;
		_ = HighlightStyle;
		_ = SeparatorStyle;
		_ = BetterToggleStyle;
		_ = TextFieldStyle;
		_ = CrewToggleStyle;
		_ = PrimaryButtonStyle;
		_ = HostButtonStyle;
		_ = PremiumBadgeStyle;
		_ = FreeBadgeStyle;
		_ = TimeRemainingStyle;
		_ = VisorAccentStyle;
	}

	public static void DrawTooltip(string tooltip, Rect rect)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(tooltip) && (rect).Contains(Event.current.mousePosition))
		{
			float num = Mathf.Min(200f, (float)tooltip.Length * 7f + 20f);
			float num2 = 28f;
			GUI.Label(new Rect(Event.current.mousePosition.x + 15f, Event.current.mousePosition.y, num, num2), tooltip, TooltipStyle);
		}
	}

	public static void DrawStatusIndicator(bool isActive)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (isActive)
		{
			if (_cachedSuccessIndicatorStyle == null)
			{
				if ((Object)(object)_cachedSuccessTexture == (Object)null)
				{
					_cachedSuccessTexture = MakeTexture(1, 1, Theme.Success);
				}
				_cachedSuccessIndicatorStyle = new GUIStyle(StatusIndicatorStyle);
				_cachedSuccessIndicatorStyle.normal.background = _cachedSuccessTexture;
			}
			GUILayout.Box(GUIContent.none, _cachedSuccessIndicatorStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			return;
		}
		if (_cachedErrorIndicatorStyle == null)
		{
			if ((Object)(object)_cachedErrorTexture == (Object)null)
			{
				_cachedErrorTexture = MakeTexture(1, 1, Theme.Error);
			}
			_cachedErrorIndicatorStyle = new GUIStyle(StatusIndicatorStyle);
			_cachedErrorIndicatorStyle.normal.background = _cachedErrorTexture;
		}
		GUILayout.Box(GUIContent.none, _cachedErrorIndicatorStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
	}

	public static void DrawSeparator()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (_separatorStyle == null || (Object)(object)_separatorTexture == (Object)null)
		{
			int num = 128;
			int num2 = 2;
			_separatorTexture = new Texture2D(num, num2, (TextureFormat)4, false);
			((Texture)_separatorTexture).filterMode = (FilterMode)1;
			((Texture)_separatorTexture).wrapMode = (TextureWrapMode)1;
			((Object)_separatorTexture).hideFlags = (HideFlags)61;
			Color[] array = (Color[])(object)new Color[num * num2];
			Color val = default(Color);
			for (int i = 0; i < num; i++)
			{
				float num3 = (float)i / (float)(num - 1);
				float num4 = 1f - Mathf.Abs(num3 * 2f - 1f);
				num4 *= num4;
				(val)._002Ector(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, num4 * 0.5f);
				for (int j = 0; j < num2; j++)
				{
					array[j * num + i] = val;
				}
			}
			_separatorTexture.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
			_separatorTexture.Apply();
			_separatorStyle = new GUIStyle();
			_separatorStyle.normal.background = _separatorTexture;
			_separatorStyle.stretchWidth = true;
			_separatorStyle.fixedHeight = 2f;
		}
		GUILayout.Space(4f);
		GUILayout.Box(GUIContent.none, _separatorStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.ExpandWidth(true),
			GUILayout.Height(2f)
		});
		GUILayout.Space(4f);
	}

	public static bool DrawTab(string label, bool selected)
	{
		return GUILayout.Toggle(selected, label, selected ? SelectedTabStyle : TabStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
	}

	public static bool DrawBetterToggle(bool value, string label, string tooltip = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Color color = GUI.color;
		if (value)
		{
			GUI.color = new Color(0.6f, 1f, 0.6f, 1f);
		}
		bool result = GUILayout.Toggle(value, label, BetterToggleStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUI.color = color;
		Rect lastRect = GUILayoutUtility.GetLastRect();
		if (!string.IsNullOrEmpty(tooltip))
		{
			DrawTooltip(tooltip, lastRect);
		}
		return result;
	}

	private static float EstimateTextWidth(string text, int fontSize)
	{
		if (string.IsNullOrEmpty(text))
		{
			return 0f;
		}
		return (float)(text.Length * fontSize) * 0.85f + 8f;
	}

	private static string TruncateToWidth(string text, int fontSize, float availableWidth)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (EstimateTextWidth(text, fontSize) <= availableWidth)
		{
			return text;
		}
		float num = (float)fontSize * 0.85f;
		int num2 = Mathf.Max(2, Mathf.FloorToInt((availableWidth - 8f - num * 1.5f) / num));
		if (num2 >= text.Length)
		{
			return text;
		}
		return text.Substring(0, num2) + "…";
	}

	private static void EnsureGameStyles()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Expected O, but got Unknown
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Expected O, but got Unknown
		if (_gameLetterBig == null || Mathf.Abs(_gameStylesScale - Spacing.MenuScale) > 0.01f)
		{
			_gameStylesScale = Spacing.MenuScale;
			_gameLetterBig = new GUIStyle(LabelStyle)
			{
				fontSize = Spacing.MenuFont(22),
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4,
				clipping = (TextClipping)1,
				wordWrap = false
			};
			_gameLetterMed = new GUIStyle(LabelStyle)
			{
				fontSize = Spacing.MenuFont(14),
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4,
				clipping = (TextClipping)1,
				wordWrap = false
			};
			_gameLetterSmall = new GUIStyle(LabelStyle)
			{
				fontSize = Spacing.MenuFont(10),
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4,
				clipping = (TextClipping)1,
				wordWrap = false
			};
			_gameTitleStyle = new GUIStyle(LabelStyle)
			{
				fontSize = Spacing.MenuFont(12),
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4,
				clipping = (TextClipping)1,
				wordWrap = false
			};
			_gameSubtitleStyle = new GUIStyle(LabelStyle)
			{
				fontSize = Spacing.MenuFont(9),
				alignment = (TextAnchor)4,
				clipping = (TextClipping)1,
				wordWrap = false
			};
			_gamePillTitleStyle = new GUIStyle(LabelStyle)
			{
				fontSize = Spacing.MenuFont(12),
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)3,
				clipping = (TextClipping)1,
				wordWrap = false
			};
			_gamePillSubStyle = new GUIStyle(LabelStyle)
			{
				fontSize = Spacing.MenuFont(9),
				alignment = (TextAnchor)3,
				clipping = (TextClipping)1,
				wordWrap = false
			};
			_gameAnimLabelStyle = new GUIStyle(LabelStyle)
			{
				fontSize = Spacing.MenuFont(10),
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4,
				clipping = (TextClipping)1,
				wordWrap = false
			};
		}
	}

	private static void DrawRectBorder(Rect r, Color c, float thickness = 1f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		Color color = GUI.color;
		GUI.color = c;
		GUI.Box(new Rect((r).x, (r).y, (r).width, thickness), GUIContent.none, WhitePixelBoxStyle);
		GUI.Box(new Rect((r).x, (r).y + (r).height - thickness, (r).width, thickness), GUIContent.none, WhitePixelBoxStyle);
		GUI.Box(new Rect((r).x, (r).y, thickness, (r).height), GUIContent.none, WhitePixelBoxStyle);
		GUI.Box(new Rect((r).x + (r).width - thickness, (r).y, thickness, (r).height), GUIContent.none, WhitePixelBoxStyle);
		GUI.color = color;
	}

	private static void DrawSolidRect(Rect r, Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Color color = GUI.color;
		GUI.color = c;
		GUI.Box(r, GUIContent.none, WhitePixelBoxStyle);
		GUI.color = color;
	}

	public static bool DrawWinnerCard(string letter, string title, string subtitle, Color tint, float height = 110f)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		EnsureGameStyles();
		Rect rect = GUILayoutUtility.GetRect(0f, Spacing.MenuSize(height, 70f), (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
		bool result = false;
		bool flag = (rect).Contains(Event.current.mousePosition);
		if ((int)Event.current.type == 0 && flag)
		{
			result = true;
			Event.current.Use();
		}
		if ((int)Event.current.type == 7)
		{
			Color c = default(Color);
			(c)._002Ector(tint.r * 0.2f, tint.g * 0.2f, tint.b * 0.2f, 0.95f);
			DrawSolidRect(rect, c);
			DrawRectBorder(rect, new Color(tint.r, tint.g, tint.b, flag ? 1f : 0.65f), 2f);
			if (flag)
			{
				DrawSolidRect(rect, new Color(tint.r, tint.g, tint.b, 0.1f));
			}
			float num = (rect).height * 0.35f;
			float num2 = (rect).height * 0.3f;
			float num3 = (rect).height * 0.3f;
			float num4 = Spacing.MenuSize(6f, 3f);
			Color color = GUI.color;
			GUI.color = tint;
			GUI.Label(new Rect((rect).x, (rect).y + (rect).height * 0.08f, (rect).width, num), letter, _gameLetterBig);
			string text = TruncateToWidth(title, _gameTitleStyle.fontSize, (rect).width - num4 * 2f);
			GUI.Label(new Rect((rect).x + num4, (rect).y + (rect).height * 0.45f, (rect).width - num4 * 2f, num2), text, _gameTitleStyle);
			GUI.color = Theme.TextMuted;
			string text2 = TruncateToWidth(subtitle, _gameSubtitleStyle.fontSize, (rect).width - num4 * 2f);
			GUI.Label(new Rect((rect).x + num4, (rect).y + (rect).height * 0.72f, (rect).width - num4 * 2f, num3), text2, _gameSubtitleStyle);
			GUI.color = color;
		}
		return result;
	}

	public static bool DrawHeroButton(string letter, string label, Color accentColor, bool useAccentBg, float height = 56f)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		EnsureGameStyles();
		Rect rect = GUILayoutUtility.GetRect(0f, Spacing.MenuSize(height, 32f), (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
		bool result = false;
		bool flag = (rect).Contains(Event.current.mousePosition);
		if ((int)Event.current.type == 0 && flag)
		{
			result = true;
			Event.current.Use();
		}
		if ((int)Event.current.type == 7)
		{
			Color c = (useAccentBg ? new Color(accentColor.r, accentColor.g, accentColor.b, 0.85f) : new Color(0.1f, 0.1f, 0.14f, 1f));
			DrawSolidRect(rect, c);
			DrawRectBorder(rect, useAccentBg ? new Color(accentColor.r, accentColor.g, accentColor.b, 1f) : new Color(accentColor.r, accentColor.g, accentColor.b, 0.45f));
			if (flag)
			{
				DrawSolidRect(rect, new Color(1f, 1f, 1f, 0.08f));
			}
			float num = Spacing.MenuSize(24f, 16f);
			float num2 = Spacing.MenuSize(8f, 4f);
			float num3 = Spacing.MenuSize(8f, 4f);
			float num4 = (rect).x + num2 + num + num3;
			float num5 = (rect).width - num2 - num - num3 - num2;
			Rect val = new Rect((rect).x + num2, (rect).y + ((rect).height - num) * 0.5f, num, num);
			Color c2 = (useAccentBg ? new Color(1f, 1f, 1f, 0.2f) : new Color(accentColor.r, accentColor.g, accentColor.b, 0.3f));
			DrawSolidRect(val, c2);
			Color color = GUI.color;
			GUI.color = (useAccentBg ? Color.white : accentColor);
			GUI.Label(val, letter, _gameLetterSmall);
			GUI.color = (Color)(useAccentBg ? Color.white : new Color(0.92f, 0.92f, 0.95f, 1f));
			GUIStyle val2 = _gameTitleStyle;
			if (EstimateTextWidth(label, val2.fontSize) > num5)
			{
				val2 = _gameLetterSmall;
			}
			string text = TruncateToWidth(label, val2.fontSize, num5);
			GUI.Label(new Rect(num4, (rect).y, num5, (rect).height), text, val2);
			GUI.color = color;
		}
		return result;
	}

	public static bool DrawTogglePill(string letter, string title, string subtitle, bool value, Color iconColor, float height = 70f)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Invalid comparison between Unknown and I4
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Invalid comparison between Unknown and I4
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		EnsureGameStyles();
		Rect rect = GUILayoutUtility.GetRect(0f, Spacing.MenuSize(height, 44f), (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
		bool flag = false;
		bool flag2 = (rect).Contains(Event.current.mousePosition);
		if ((int)Event.current.type == 0 && flag2)
		{
			flag = true;
			Event.current.Use();
		}
		(letter + title).GetHashCode();
		string key = "toggle_pill_" + letter + title;
		if (!_toggleAnim.TryGetValue(key, out var value2))
		{
			value2 = (value ? 1f : 0f);
		}
		float num = (value ? 1f : 0f);
		if ((int)Event.current.type == 7)
		{
			value2 = Mathf.MoveTowards(value2, num, Time.deltaTime * 8f);
			_toggleAnim[key] = value2;
		}
		float num2 = value2 * value2 * (3f - 2f * value2);
		if ((int)Event.current.type == 7)
		{
			Color val = default(Color);
			(val)._002Ector(0.07f, 0.07f, 0.1f, 1f);
			Color val2 = default(Color);
			(val2)._002Ector(iconColor.r * 0.22f, iconColor.g * 0.22f, iconColor.b * 0.22f, 1f);
			DrawSolidRect(rect, Color.Lerp(val, val2, num2));
			Color c = Color.Lerp(new Color(iconColor.r, iconColor.g, iconColor.b, 0.2f), new Color(iconColor.r, iconColor.g, iconColor.b, 0.85f), num2);
			DrawRectBorder(rect, c);
			if (flag2)
			{
				DrawSolidRect(rect, new Color(1f, 1f, 1f, 0.06f));
			}
			float num3 = Spacing.MenuSize(8f, 4f);
			float num4 = Spacing.MenuSize(30f, 20f);
			float num5 = Spacing.MenuSize(44f, 28f);
			float num6 = Spacing.MenuSize(22f, 14f);
			float num7 = Spacing.MenuSize(8f, 4f);
			float num8 = (rect).x + num3 + num4 + num7;
			float num9 = (rect).width - num3 - num4 - num7 - num5 - num3 - num7;
			Rect val3 = new Rect((rect).x + num3, (rect).y + ((rect).height - num4) * 0.5f, num4, num4);
			DrawSolidRect(val3, new Color(iconColor.r, iconColor.g, iconColor.b, 0.22f));
			DrawRectBorder(val3, new Color(iconColor.r, iconColor.g, iconColor.b, 0.55f));
			Color color = GUI.color;
			GUI.color = iconColor;
			GUI.Label(val3, letter, _gameLetterSmall);
			GUI.color = Color.white;
			string text = TruncateToWidth(title, _gamePillTitleStyle.fontSize, num9);
			GUI.Label(new Rect(num8, (rect).y + (rect).height * 0.18f, num9, (rect).height * 0.36f), text, _gamePillTitleStyle);
			GUI.color = Theme.TextMuted;
			string text2 = TruncateToWidth(subtitle, _gamePillSubStyle.fontSize, num9);
			GUI.Label(new Rect(num8, (rect).y + (rect).height * 0.52f, num9, (rect).height * 0.34f), text2, _gamePillSubStyle);
			Rect r = default(Rect);
			(r)._002Ector((rect).x + (rect).width - num5 - num3, (rect).y + ((rect).height - num6) * 0.5f, num5, num6);
			Color val4 = default(Color);
			(val4)._002Ector(0.18f, 0.18f, 0.22f, 1f);
			Color val5 = default(Color);
			(val5)._002Ector(iconColor.r, iconColor.g, iconColor.b, 1f);
			DrawSolidRect(r, Color.Lerp(val4, val5, num2));
			DrawRectBorder(r, new Color(0f, 0f, 0f, 0.4f));
			float num10 = num6 - Mathf.Max(4f, num6 * 0.2f);
			float num11 = (num6 - num10) * 0.5f;
			float num12 = Mathf.Lerp((r).x + num11, (r).x + num5 - num10 - num11, num2);
			DrawSolidRect(new Rect(num12, (r).y + num11, num10, num10), Color.white);
			GUI.color = color;
		}
		if (!flag)
		{
			return value;
		}
		return !value;
	}

	public static bool DrawSimpleColoredButton(string label, Color color, float height = 50f)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Invalid comparison between Unknown and I4
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		EnsureGameStyles();
		Rect rect = GUILayoutUtility.GetRect(0f, Spacing.MenuSize(height, 26f), (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
		bool result = false;
		bool flag = (rect).Contains(Event.current.mousePosition);
		if ((int)Event.current.type == 0 && flag)
		{
			result = true;
			Event.current.Use();
		}
		if ((int)Event.current.type == 7)
		{
			DrawSolidRect(rect, new Color(color.r * 0.18f, color.g * 0.18f, color.b * 0.18f, 1f));
			DrawRectBorder(rect, new Color(color.r, color.g, color.b, flag ? 0.95f : 0.55f));
			if (flag)
			{
				DrawSolidRect(rect, new Color(color.r, color.g, color.b, 0.1f));
			}
			Color color2 = GUI.color;
			GUI.color = color;
			GUIStyle val = _gameAnimLabelStyle;
			if (EstimateTextWidth(label, val.fontSize) > (rect).width - 6f)
			{
				val = _gameLetterSmall;
			}
			string text = TruncateToWidth(label, val.fontSize, (rect).width - 4f);
			GUI.Label(rect, text, val);
			GUI.color = color2;
		}
		return result;
	}

	public static bool DrawAnimationCardBtn(string letter, string label, Color iconColor, float height = 50f)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Invalid comparison between Unknown and I4
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		EnsureGameStyles();
		Rect rect = GUILayoutUtility.GetRect(0f, Spacing.MenuSize(height, 28f), (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
		bool result = false;
		bool flag = (rect).Contains(Event.current.mousePosition);
		if ((int)Event.current.type == 0 && flag)
		{
			result = true;
			Event.current.Use();
		}
		if ((int)Event.current.type == 7)
		{
			DrawSolidRect(rect, new Color(0.07f, 0.07f, 0.11f, 1f));
			DrawRectBorder(rect, new Color(iconColor.r, iconColor.g, iconColor.b, flag ? 0.7f : 0.3f));
			if (flag)
			{
				DrawSolidRect(rect, new Color(iconColor.r, iconColor.g, iconColor.b, 0.06f));
			}
			float num = Spacing.MenuSize(20f, 14f);
			float num2 = Spacing.MenuSize(16f, 10f);
			float num3 = Spacing.MenuSize(6f, 3f);
			float num4 = Spacing.MenuSize(6f, 3f);
			float num5 = Spacing.MenuSize(4f, 2f);
			float num6 = (rect).x + num3 + num + num5;
			float num7 = (rect).width - num3 - num - num5 - num2 - num4 - num5;
			Rect val = new Rect((rect).x + num3, (rect).y + ((rect).height - num) * 0.5f, num, num);
			Color color = GUI.color;
			GUI.color = iconColor;
			GUI.Label(val, letter, _gameLetterSmall);
			GUI.color = Color.white;
			GUIStyle val2 = _gameAnimLabelStyle;
			if (EstimateTextWidth(label, val2.fontSize) > num7)
			{
				val2 = _gameLetterSmall;
			}
			string text = TruncateToWidth(label, val2.fontSize, num7);
			GUI.Label(new Rect(num6, (rect).y, num7, (rect).height), text, val2);
			GUI.color = iconColor;
			GUI.Label(new Rect((rect).x + (rect).width - num4 - num2, (rect).y, num2, (rect).height), "▶", _gameLetterSmall);
			GUI.color = color;
		}
		return result;
	}

	public static void DrawSectionBadge(string letter, string title, Color badgeColor)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		EnsureGameStyles();
		float num = Spacing.MenuSize(26f, 18f);
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		Rect rect = GUILayoutUtility.GetRect(num, num, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Width(num),
			GUILayout.Height(num)
		});
		if ((int)Event.current.type == 7)
		{
			DrawSolidRect(rect, new Color(badgeColor.r, badgeColor.g, badgeColor.b, 0.3f));
			DrawRectBorder(rect, new Color(badgeColor.r, badgeColor.g, badgeColor.b, 0.85f));
			Color color = GUI.color;
			GUI.color = badgeColor;
			GUI.Label(rect, letter, _gameLetterSmall);
			GUI.color = color;
		}
		GUILayout.Space((float)Spacing.MenuPad(8));
		Color color2 = GUI.color;
		GUI.color = badgeColor;
		GUILayout.Label(title, HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUI.color = color2;
		GUILayout.EndHorizontal();
	}

	public static string GetHeaderText(string text)
	{
		return $"{text} - {DateTime.Now:HH:mm:ss}";
	}

	public static GUIStyle GetAnimatedHeaderStyle()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (_cachedAnimatedHeaderStyle == null)
		{
			_cachedAnimatedHeaderStyle = new GUIStyle(HeaderStyle);
		}
		_cachedAnimatedHeaderStyle.normal.textColor = Color.Lerp(Theme.Accent, Theme.AccentSoft, Mathf.PingPong(Time.time * 2f, 1f));
		return _cachedAnimatedHeaderStyle;
	}

	public static string GetCurrentTime()
	{
		return DateTime.Now.ToString("HH:mm:ss");
	}

	public static void DrawKeyBadge(bool isPremium, string keyDisplay = null)
	{
		if (isPremium)
		{
			GUILayout.Box(string.IsNullOrEmpty(keyDisplay) ? "★ PREMIUM" : keyDisplay, PremiumBadgeStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
		}
		else
		{
			GUILayout.Box("FREE", FreeBadgeStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
		}
	}

	public static void DrawTimeRemaining(string timeDisplay)
	{
		if (!string.IsNullOrEmpty(timeDisplay))
		{
			GUILayout.Label("⏱ " + timeDisplay, TimeRemainingStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
		}
	}

	public static void DrawCrewSeparator()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Color color = GUI.color;
		GUI.color = Theme.Accent;
		GUILayout.Box("", (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Height(2f),
			GUILayout.ExpandWidth(true)
		});
		GUI.color = color;
	}

	public static void DrawCrewCoreLogo()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.FlexibleSpace();
		if (_crewLogoStyleRed == null)
		{
			_crewLogoStyleRed = new GUIStyle(HeaderStyle)
			{
				fontSize = 20
			};
			_crewLogoStyleRed.normal.textColor = Theme.Accent;
		}
		GUILayout.Label("CREW", _crewLogoStyleRed, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
		if (_crewLogoStyleCyan == null)
		{
			_crewLogoStyleCyan = new GUIStyle(HeaderStyle)
			{
				fontSize = 20
			};
			_crewLogoStyleCyan.normal.textColor = Theme.Visor;
		}
		GUILayout.Label("CORE", _crewLogoStyleCyan, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	public static bool DrawCrewToggle(bool value, string label, string tooltip = null)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		bool result = GUILayout.Toggle(value, label, CrewToggleStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		Rect lastRect = GUILayoutUtility.GetLastRect();
		if (!string.IsNullOrEmpty(tooltip))
		{
			DrawTooltip(tooltip, lastRect);
		}
		return result;
	}

	public static float DrawCrewSlider(float value, float min, float max, string label, string format = "F1", string suffix = "", bool showMinMax = false)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		if (_sliderLabelStyle == null)
		{
			_sliderLabelStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = Spacing.MenuFont(13),
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)3,
				padding = CreateRectOffset(0, 0, 0, 0),
				margin = CreateRectOffset(0, 0, 0, 0)
			};
			_sliderLabelStyle.normal.textColor = Theme.TextPrimary;
		}
		if (_sliderValueStyle == null)
		{
			_sliderValueStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 14,
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)5,
				padding = CreateRectOffset(0, 0, 0, 0),
				margin = CreateRectOffset(0, 0, 0, 0)
			};
			_sliderValueStyle.normal.textColor = Theme.Visor;
		}
		GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(48f) });
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.Label(label, _sliderLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
		GUILayout.Label(value.ToString(format) + suffix, _sliderValueStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(60f) });
		GUILayout.EndHorizontal();
		GUILayout.Space(2f);
		float result = GUILayout.HorizontalSlider(value, min, max, SliderStyle, SliderThumbStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Height(18f),
			GUILayout.ExpandWidth(true)
		});
		if (showMinMax)
		{
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			if (_sliderMinMaxStyle == null)
			{
				_sliderMinMaxStyle = new GUIStyle(_sliderLabelStyle)
				{
					fontSize = Spacing.MenuFont(10)
				};
				_sliderMinMaxStyle.normal.textColor = Theme.TextMuted;
			}
			GUILayout.Label(min.ToString(format), _sliderMinMaxStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.FlexibleSpace();
			GUILayout.Label(max.ToString(format), _sliderMinMaxStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		return result;
	}

	private static void DrawSliderFill(Rect sliderRect, float value, float min, float max)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01((value - min) / (max - min));
		Rect val = default(Rect);
		(val)._002Ector((sliderRect).x + 9f, (sliderRect).y + 5f, (sliderRect).width - 18f, 8f);
		GUI.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);
		GUI.Box(val, GUIContent.none, GUIStyle.none);
		if (num > 0.01f)
		{
			Rect val2 = default(Rect);
			(val2)._002Ector((val).x, (val).y, (val).width * num, (val).height);
			Color val3 = Color.Lerp(Theme.AccentDim, Theme.Accent, num);
			float num2 = 0.9f + Mathf.Sin(Time.realtimeSinceStartup * 2f) * 0.1f;
			GUI.color = val3 * num2;
			GUI.Box(val2, GUIContent.none, GUIStyle.none);
			if (num > 0.05f)
			{
				Color color = default(Color);
				(color)._002Ector(Theme.Visor.r, Theme.Visor.g, Theme.Visor.b, 0.4f * num);
				Rect val4 = new Rect((val2).xMax - 4f, (val).y - 1f, 8f, (val).height + 2f);
				GUI.color = color;
				GUI.Box(val4, GUIContent.none, GUIStyle.none);
			}
		}
		GUI.color = Color.white;
	}

	public static float DrawMiniSlider(float value, float min, float max, float width = 100f)
	{
		return GUILayout.HorizontalSlider(value, min, max, SliderStyle, SliderThumbStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Width(width),
			GUILayout.Height(16f)
		});
	}

	public static bool DrawEliteButton(string label, float height = 32f)
	{
		return GUILayout.Button(label, ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Height(height),
			GUILayout.ExpandWidth(true)
		});
	}

	public static void DrawEliteSeparator(float alpha = 0.8f)
	{
		DrawSeparator();
	}

	public static Vector2 BeginEliteScrollView(Vector2 scrollPos, params GUILayoutOption[] options)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		return GUILayout.BeginScrollView(scrollPos, false, false, options);
	}

	public static void EndEliteScrollView()
	{
		GUILayout.EndScrollView();
	}

	public static void BeginEliteCard(string title = null)
	{
		GUILayout.BeginVertical(SectionStyle, Array.Empty<GUILayoutOption>());
		if (!string.IsNullOrEmpty(title))
		{
			GUILayout.Label(title, HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Space(4f);
		}
	}

	public static void EndEliteCard()
	{
		GUILayout.EndVertical();
	}

	public static void DrawEliteStatus(bool isActive, string label)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		Rect rect = GUILayoutUtility.GetRect(12f, 12f, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Width(12f),
			GUILayout.Height(12f)
		});
		if ((int)Event.current.type == 7)
		{
			Color val = (isActive ? Theme.Success : Theme.TextDisabled);
			if (isActive)
			{
				float num = Animation.Pulse(2f, 0.7f);
				(val)._002Ector(val.r * num, val.g * num, val.b * num, val.a);
			}
			if (_eliteDotBoxStyle == null)
			{
				_eliteDotBoxStyle = new GUIStyle();
				_eliteDotBoxStyle.normal.background = MakeTexture(12, 12, Color.white);
			}
			GUI.color = val;
			GUI.Box(rect, GUIContent.none, _eliteDotBoxStyle);
			GUI.color = Color.white;
		}
		GUILayout.Space(4f);
		if (_eliteStatusActiveStyle == null)
		{
			_eliteStatusActiveStyle = new GUIStyle(LabelStyle)
			{
				fontSize = 12
			};
			_eliteStatusActiveStyle.normal.textColor = Theme.TextPrimary;
			_eliteStatusInactiveStyle = new GUIStyle(LabelStyle)
			{
				fontSize = 12
			};
			_eliteStatusInactiveStyle.normal.textColor = Theme.TextInactive;
		}
		GUILayout.Label(label, isActive ? _eliteStatusActiveStyle : _eliteStatusInactiveStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		GUILayout.EndHorizontal();
	}

	public static void DrawEliteProgressBar(float progress, string label = null, float height = 20f)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Invalid comparison between Unknown and I4
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		progress = Mathf.Clamp01(progress);
		GUILayout.BeginVertical((Il2CppReferenceArray<GUILayoutOption>)null);
		if (!string.IsNullOrEmpty(label))
		{
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Label(label, LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.FlexibleSpace();
			GUILayout.Label($"{progress * 100f:F0}%", LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.EndHorizontal();
		}
		Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Height(height),
			GUILayout.ExpandWidth(true)
		});
		if ((int)Event.current.type == 7)
		{
			GUI.color = new Color(0.1f, 0.1f, 0.12f, 0.9f);
			GUI.Box(rect, GUIContent.none, WhitePixelBoxStyle);
			if (progress > 0.01f)
			{
				Rect val = default(Rect);
				(val)._002Ector((rect).x, (rect).y, (rect).width * progress, (rect).height);
				float num = Animation.Pulse();
				Color val2 = Color.Lerp(Theme.AccentActive, Theme.Accent, progress);
				(val2)._002Ector(val2.r * num, val2.g * num, val2.b * num, 1f);
				GUI.color = val2;
				GUI.Box(val, GUIContent.none, WhitePixelBoxStyle);
				if (progress < 0.99f)
				{
					Rect val3 = new Rect((val).xMax - 4f, (rect).y, 8f, (rect).height);
					GUI.color = new Color(1f, 1f, 1f, 0.3f * num);
					GUI.Box(val3, GUIContent.none, WhitePixelBoxStyle);
				}
			}
			GUI.color = Color.white;
		}
		GUILayout.EndVertical();
	}

	private static Texture2D GetRadialGlow()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_radialGlowTex != (Object)null)
		{
			return _radialGlowTex;
		}
		int num = 32;
		_radialGlowTex = new Texture2D(num, num, (TextureFormat)4, false);
		((Texture)_radialGlowTex).filterMode = (FilterMode)1;
		((Texture)_radialGlowTex).wrapMode = (TextureWrapMode)1;
		((Object)_radialGlowTex).hideFlags = (HideFlags)61;
		Color32[] array = (Color32[])(object)new Color32[num * num];
		float num2 = (float)num * 0.5f;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float num3 = ((float)j - num2 + 0.5f) / num2;
				float num4 = ((float)i - num2 + 0.5f) / num2;
				float num5 = Mathf.Sqrt(num3 * num3 + num4 * num4);
				float num6 = Mathf.Clamp01(1f - num5);
				num6 = num6 * num6 * num6;
				array[i * num + j] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(num6 * 255f));
			}
		}
		_radialGlowTex.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
		_radialGlowTex.Apply();
		return _radialGlowTex;
	}

	private static Texture2D GetPillTexture()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_pillTex != (Object)null)
		{
			return _pillTex;
		}
		_pillTex = MakeRoundedTexture(32, 16, new Color(0.2f, 0.2f, 0.25f, 1f), new Color(0.3f, 0.3f, 0.35f, 1f), 1);
		return _pillTex;
	}

	private static Texture2D GetKnobTexture()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_knobTex != (Object)null)
		{
			return _knobTex;
		}
		_knobTex = MakeCircleTexture(16, Color.white, new Color(0.9f, 0.9f, 0.9f, 1f));
		return _knobTex;
	}

	public static void DrawDropShadow(Rect elementRect, float spread = 10f, float alpha = 0.45f)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type != 7)
		{
			return;
		}
		if ((Object)(object)_dropShadowTex == (Object)null)
		{
			int num = 32;
			_dropShadowTex = new Texture2D(num, num, (TextureFormat)4, false);
			((Texture)_dropShadowTex).filterMode = (FilterMode)1;
			((Texture)_dropShadowTex).wrapMode = (TextureWrapMode)1;
			((Object)_dropShadowTex).hideFlags = (HideFlags)61;
			Color32[] array = (Color32[])(object)new Color32[num * num];
			float num2 = (float)num * 0.5f;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num; j++)
				{
					float num3 = ((float)j - num2 + 0.5f) / num2;
					float num4 = ((float)i - num2 + 0.5f) / num2;
					float num5 = Mathf.Sqrt(num3 * num3 + num4 * num4);
					float num6 = Mathf.Clamp01(1f - num5);
					num6 = num6 * num6 * num6;
					array[i * num + j] = new Color32((byte)0, (byte)0, (byte)0, (byte)(num6 * 220f));
				}
			}
			_dropShadowTex.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
			_dropShadowTex.Apply();
		}
		if (_dropShadowBoxStyle == null)
		{
			_dropShadowBoxStyle = new GUIStyle();
			_dropShadowBoxStyle.normal.background = _dropShadowTex;
		}
		Color color = GUI.color;
		GUI.color = new Color(0f, 0f, 0f, alpha);
		GUI.Box(new Rect((elementRect).x - spread * 0.5f + 2f, (elementRect).y - spread * 0.5f + 4f, (elementRect).width + spread, (elementRect).height + spread), GUIContent.none, _dropShadowBoxStyle);
		GUI.color = color;
	}

	public static void DrawMultiLayerBloom(Rect r, float intensity = 1f)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type == 7)
		{
			Color color = GUI.color;
			Color accent = Theme.Accent;
			GetRadialGlow();
			for (int num = 3; num >= 1; num--)
			{
				float num2 = (float)num * 5f;
				float num3 = 0.12f * (float)num * intensity;
				GUI.color = new Color(accent.r, accent.g, accent.b, num3);
				GUI.Box(new Rect((r).x - num2, (r).y - num2, (r).width + num2 * 2f, (r).height + num2 * 2f), GUIContent.none, RadialGlowBoxStyle);
			}
			GUI.color = color;
		}
	}

	public static void DrawAnimatedBackground(Rect area)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		if ((int)Event.current.type == 7)
		{
			if ((Object)(object)_animBgA == (Object)null)
			{
				_animBgA = MakeVerticalGradientTexture(4, 64, new Color(0.04f, 0.04f, 0.08f, 0.6f), new Color(0.12f, 0.03f, 0.05f, 0.6f));
				_animBgB = MakeVerticalGradientTexture(4, 64, new Color(0.1f, 0.02f, 0.04f, 0.6f), new Color(0.03f, 0.04f, 0.1f, 0.6f));
			}
			float num = (Mathf.Sin(Time.realtimeSinceStartup * 0.8f) + 1f) * 0.5f;
			Color color = GUI.color;
			if (_animBgBoxStyleA == null)
			{
				_animBgBoxStyleA = new GUIStyle();
				_animBgBoxStyleA.normal.background = _animBgA;
				_animBgBoxStyleB = new GUIStyle();
				_animBgBoxStyleB.normal.background = _animBgB;
			}
			GUI.color = new Color(1f, 1f, 1f, 1f - num);
			GUI.Box(area, GUIContent.none, _animBgBoxStyleA);
			GUI.color = new Color(1f, 1f, 1f, num);
			GUI.Box(area, GUIContent.none, _animBgBoxStyleB);
			GUI.color = color;
		}
	}

	public static void DrawAnimatedScanlines(Rect area, float opacity = 0.07f)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type != 7)
		{
			return;
		}
		if ((Object)(object)_scanlineTileTex == (Object)null)
		{
			_scanlineTileTex = new Texture2D(1, 6, (TextureFormat)4, false);
			_scanlineTileTex.SetPixels32(Il2CppStructArray<Color32>.op_Implicit((Color32[])(object)new Color32[6]
			{
				new Color32((byte)0, (byte)0, (byte)0, (byte)0),
				new Color32((byte)0, (byte)0, (byte)0, (byte)40),
				new Color32((byte)0, (byte)0, (byte)0, (byte)25),
				new Color32((byte)0, (byte)0, (byte)0, (byte)0),
				new Color32((byte)0, (byte)0, (byte)0, (byte)0),
				new Color32((byte)0, (byte)0, (byte)0, (byte)0)
			}));
			((Texture)_scanlineTileTex).filterMode = (FilterMode)0;
			((Texture)_scanlineTileTex).wrapMode = (TextureWrapMode)0;
			_scanlineTileTex.Apply();
			((Object)_scanlineTileTex).hideFlags = (HideFlags)61;
		}
		int num = Mathf.CeilToInt((area).height) + 6;
		if (_scanlineBoxStyle == null || Mathf.Abs(_scanlineCachedHeight - num) > 20)
		{
			Texture2D val = new Texture2D(1, num, (TextureFormat)4, false);
			((Texture)val).filterMode = (FilterMode)0;
			((Texture)val).wrapMode = (TextureWrapMode)1;
			((Object)val).hideFlags = (HideFlags)61;
			Color32[] array = (Color32[])(object)new Color32[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = new Color32((byte)0, (byte)0, (byte)0, (i % 6) switch
				{
					1 => 40, 
					2 => 25, 
					_ => 0, 
				});
			}
			val.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
			val.Apply();
			_scanlineBoxStyle = new GUIStyle();
			_scanlineBoxStyle.normal.background = val;
			_scanlineCachedHeight = num;
		}
		Color color = GUI.color;
		GUI.color = new Color(1f, 1f, 1f, opacity);
		GUI.Box(area, GUIContent.none, _scanlineBoxStyle);
		GUI.color = color;
	}

	public static void DrawAccentHeaderLine(Rect windowRect)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type == 7)
		{
			float num = 3f;
			Rect val = default(Rect);
			(val)._002Ector((windowRect).x, (windowRect).y, (windowRect).width, num);
			Color color = GUI.color;
			GUI.color = Theme.Accent;
			GUI.Box(val, GUIContent.none, WhitePixelBoxStyle);
			float num2 = Time.realtimeSinceStartup * 0.3f % 1f;
			float num3 = (windowRect).width * 0.25f;
			float num4 = (val).x + num2 * ((val).width + num3) - num3;
			float num5 = num3 * 0.3f;
			float num6 = num3 * 0.4f;
			float num7 = Mathf.Max(num4 + num5, (val).x);
			float num8 = Mathf.Max(0f, Mathf.Min(num6, (val).xMax - num7));
			float num9 = Mathf.Max(num4, (val).x);
			float num10 = Mathf.Max(0f, Mathf.Min(num5, (val).xMax - num9));
			float num11 = Mathf.Max(num4 + num5 + num6, (val).x);
			float num12 = Mathf.Max(0f, Mathf.Min(num5, (val).xMax - num11));
			if (num8 > 0f)
			{
				GUI.color = new Color(1f, 1f, 1f, 0.45f);
				GUI.Box(new Rect(num7, (val).y, num8, num), GUIContent.none, WhitePixelBoxStyle);
			}
			if (num10 > 0f)
			{
				GUI.color = new Color(1f, 1f, 1f, 0.18f);
				GUI.Box(new Rect(num9, (val).y, num10, num), GUIContent.none, WhitePixelBoxStyle);
			}
			if (num12 > 0f)
			{
				GUI.color = new Color(1f, 1f, 1f, 0.18f);
				GUI.Box(new Rect(num11, (val).y, num12, num), GUIContent.none, WhitePixelBoxStyle);
			}
			GUI.color = color;
		}
	}

	public static void TriggerRipple(string id)
	{
		_rippleTime[id] = Time.realtimeSinceStartup;
	}

	public static void DrawRippleEffect(string id, Rect r)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type == 7 && _rippleTime.TryGetValue(id, out var value))
		{
			float num = Time.realtimeSinceStartup - value;
			if (num > 0.5f)
			{
				_rippleTime.Remove(id);
				return;
			}
			float num2 = num / 0.5f;
			float num3 = ModMenuCrew.Easing.Easing.EaseOutExpo(num2);
			float num4 = (1f - num2) * 0.35f;
			float num5 = num3 * 16f;
			Color color = GUI.color;
			GUI.color = new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, num4);
			GUI.Box(new Rect((r).x - num5, (r).y - num5, (r).width + num5 * 2f, (r).height + num5 * 2f), GUIContent.none, RadialGlowBoxStyle);
			GUI.color = color;
		}
	}

	public static void DrawPulseSeparator()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.Space(3f);
		Rect rect = GUILayoutUtility.GetRect(0f, 2f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
		if ((int)Event.current.type == 7)
		{
			Color color = GUI.color;
			GUI.color = new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, 0.25f);
			GUI.Box(rect, GUIContent.none, WhitePixelBoxStyle);
			float num = Time.realtimeSinceStartup * 0.4f % 1f;
			float num2 = (rect).x + num * (rect).width;
			float num3 = (rect).width * 0.28f;
			Rect val = new Rect(Mathf.Max(num2 - num3 * 0.5f, (rect).x), (rect).y - 1f, Mathf.Min(num3, (rect).xMax - Mathf.Max(num2 - num3 * 0.5f, (rect).x)), (rect).height + 2f);
			float num4 = 0.9f * Mathf.Sin(num * (float)Math.PI);
			GUI.color = new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, num4);
			GUI.Box(val, GUIContent.none, RadialGlowBoxStyle);
			GUI.color = color;
		}
		GUILayout.Space(3f);
	}

	public static void QueueEliteTooltip(string text, Rect triggerRect, float delay = 0.3f)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (!(triggerRect).Contains(Event.current.mousePosition))
		{
			if (_eliteTooltipTriggerRect == triggerRect)
			{
				_eliteTooltipHoverStart = -1f;
				_eliteTooltipShowTime = -1f;
			}
			return;
		}
		if (_eliteTooltipTriggerRect != triggerRect)
		{
			_eliteTooltipTriggerRect = triggerRect;
			_eliteTooltipHoverStart = Time.realtimeSinceStartup;
			_eliteTooltipShowTime = -1f;
		}
		if (!(Time.realtimeSinceStartup - _eliteTooltipHoverStart < delay))
		{
			if (_eliteTooltipShowTime < 0f)
			{
				_eliteTooltipShowTime = Time.realtimeSinceStartup;
			}
			_eliteTooltipText = text;
			_eliteTooltipPos = Event.current.mousePosition;
		}
	}

	public static void DrawPendingEliteTooltip()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Expected O, but got Unknown
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(_eliteTooltipText))
		{
			return;
		}
		if ((int)Event.current.type != 7)
		{
			_eliteTooltipText = null;
			return;
		}
		if ((Object)(object)_eliteTooltipBgTex == (Object)null)
		{
			_eliteTooltipBgTex = MakeFrameTexture(16, 16, new Color(0.08f, 0.08f, 0.12f, 0.97f), new Color(0.06f, 0.06f, 0.1f, 0.97f), Theme.Accent, 1);
		}
		float num = ((_eliteTooltipShowTime > 0f) ? Mathf.Clamp01((Time.realtimeSinceStartup - _eliteTooltipShowTime) / 0.15f) : 1f);
		float num2 = 0.92f + 0.08f * ModMenuCrew.Easing.Easing.EaseOutQuad(num);
		float num3 = 6.2f * Spacing.MenuScale;
		float num4 = 15f * Spacing.MenuScale;
		string[] array = _eliteTooltipText.Split('\n');
		float num5 = 0f;
		string[] array2 = array;
		foreach (string text in array2)
		{
			num5 = Mathf.Max(num5, (float)text.Length * num3);
		}
		float num6 = Mathf.Clamp(num5 + 24f, 70f, 320f);
		float num7 = (float)array.Length * num4 + 14f;
		float num8 = (float)Screen.width / Spacing.Scale;
		float num9 = (float)Screen.height / Spacing.Scale;
		float num10 = Mathf.Min(_eliteTooltipPos.x + 14f, num8 - num6 - 4f);
		float num11 = Mathf.Min(_eliteTooltipPos.y + 18f, num9 - num7 - 4f);
		Rect val = default(Rect);
		(val)._002Ector(num10, num11, num6 * num2, num7 * num2);
		Color color = GUI.color;
		if (_tooltipBgBoxStyle == null || (Object)(object)_tooltipBgBoxStyle.normal.background != (Object)(object)_eliteTooltipBgTex)
		{
			_tooltipBgBoxStyle = new GUIStyle();
			_tooltipBgBoxStyle.normal.background = _eliteTooltipBgTex;
		}
		GUI.color = new Color(0f, 0f, 0f, 0.5f * num);
		GUI.Box(new Rect((val).x + 3f, (val).y + 3f, (val).width, (val).height), GUIContent.none, RadialGlowBoxStyle);
		GUI.color = new Color(1f, 1f, 1f, num);
		GUI.Box(val, GUIContent.none, _tooltipBgBoxStyle);
		GUI.color = new Color(Theme.TextPrimary.r, Theme.TextPrimary.g, Theme.TextPrimary.b, num);
		GUI.Label(new Rect(num10 + 10f, num11 + 5f, num6 - 20f, num7 - 10f), _eliteTooltipText, TooltipStyle);
		GUI.color = color;
		_eliteTooltipText = null;
	}

	public static void DrawToggleIndicator(string id, bool value, Rect toggleRect)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Event.current.type == 7)
		{
			if (!_toggleAnim.TryGetValue(id, out var value2))
			{
				value2 = (value ? 1f : 0f);
			}
			float num = (value ? 1f : 0f);
			value2 = Mathf.MoveTowards(value2, num, Time.deltaTime * 10f);
			_toggleAnim[id] = value2;
			float num2 = value2 * value2 * (3f - 2f * value2);
			float num3 = 3f;
			float num4 = (toggleRect).height * 0.6f;
			float num5 = (toggleRect).y + ((toggleRect).height - num4) * 0.5f;
			Color color = GUI.color;
			GUI.color = new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, num2);
			GUI.Box(new Rect((toggleRect).x, num5, num3, num4), GUIContent.none, WhitePixelBoxStyle);
			if (num2 > 0.1f)
			{
				GUI.color = new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, num2 * 0.12f);
				GUI.Box(new Rect((toggleRect).x - 2f, num5 - 2f, num3 + 10f, num4 + 4f), GUIContent.none, RadialGlowBoxStyle);
			}
			GUI.color = color;
		}
	}

	public static bool DrawAnimatedToggle(string id, bool value, string label)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Invalid comparison between Unknown and I4
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		_toggleAnim.TryGetValue(id, out var value2);
		float num = (value ? 1f : 0f);
		if ((int)Event.current.type == 7)
		{
			value2 = Mathf.MoveTowards(value2, num, Time.deltaTime * 10f);
			_toggleAnim[id] = value2;
		}
		float num2 = value2 * value2 * (3f - 2f * value2);
		float menuScale = Spacing.MenuScale;
		float num3 = Mathf.Max(36f * menuScale, 28f);
		float num4 = Mathf.Max(18f * menuScale, 14f);
		float num5 = Mathf.Max(num4, 20f * menuScale);
		GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
		Rect rect = GUILayoutUtility.GetRect(num3, num5, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num3) });
		float num6 = (rect).y + (num5 - num4) * 0.5f;
		Rect val = default(Rect);
		(val)._002Ector((rect).x, num6, num3, num4);
		bool flag = false;
		if ((int)Event.current.type == 0 && (val).Contains(Event.current.mousePosition))
		{
			flag = true;
			Event.current.Use();
		}
		if ((int)Event.current.type == 7)
		{
			Color val2 = new Color(0.18f, 0.18f, 0.22f, 1f);
			Color val3 = default(Color);
			(val3)._002Ector(Theme.Accent.r * 0.6f, Theme.Accent.g * 0.6f, Theme.Accent.b * 0.6f, 1f);
			Color color = Color.Lerp(val2, val3, num2);
			Color color2 = GUI.color;
			GUI.color = color;
			GUI.Box(val, GUIContent.none, PillBoxStyle);
			float num7 = num4 - 4f;
			float num8 = Mathf.Lerp((val).x + 2f, (val).x + num3 - num7 - 2f, num2);
			float num9 = num6 + 2f;
			GUI.color = Color.Lerp(new Color(0.65f, 0.65f, 0.68f, 1f), Color.white, num2);
			GUI.Box(new Rect(num8, num9, num7, num7), GUIContent.none, KnobBoxStyle);
			if (num2 > 0.1f)
			{
				GUI.color = new Color(Theme.Accent.r, Theme.Accent.g, Theme.Accent.b, num2 * 0.15f);
				GUI.Box(new Rect((val).x - 4f, (val).y - 4f, num3 + 8f, num4 + 8f), GUIContent.none, RadialGlowBoxStyle);
			}
			GUI.color = color2;
		}
		GUILayout.Space(6f * menuScale);
		Color color3 = (value ? Theme.TextPrimary : Theme.TextMuted);
		Color color4 = GUI.color;
		GUI.color = color3;
		GUILayout.Label(label, ToggleLabelStyle ?? LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { CachedHeight(num5) });
		Rect lastRect = GUILayoutUtility.GetLastRect();
		GUI.color = color4;
		if ((int)Event.current.type == 0 && (lastRect).Contains(Event.current.mousePosition))
		{
			flag = true;
			Event.current.Use();
		}
		GUILayout.EndHorizontal();
		if (!flag)
		{
			return value;
		}
		return !value;
	}

	public static Vector2 SmoothScroll(string id, Vector2 rawScroll)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		_scrollSmooth.TryGetValue(id, out var value);
		value = Mathf.Lerp(value, rawScroll.y, Mathf.Clamp01(Time.deltaTime * 14f));
		if (Mathf.Abs(value - rawScroll.y) < 0.5f)
		{
			value = rawScroll.y;
		}
		_scrollSmooth[id] = value;
		return new Vector2(rawScroll.x, value);
	}
}
