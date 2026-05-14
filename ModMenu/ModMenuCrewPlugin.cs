using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Microsoft.Win32;
using ModMenuCrew.Easing;
using ModMenuCrew.Features;
using ModMenuCrew.Monitoring;
using ModMenuCrew.Networking;
using ModMenuCrew.Patches;
using ModMenuCrew.ReplaySystem;
using ModMenuCrew.UI;
using ModMenuCrew.UI.Managers;
using ModMenuCrew.UI.Menus;
using ModMenuCrew.UI.Styles;
using ModMenuCrew.Web;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ModMenuCrew;

[BepInPlugin("CrewCore.online", "Among Us Mod Menu Crew", "6.1.4")]
[BepInProcess("Among Us.exe")]
public class ModMenuCrewPlugin : BasePlugin
{
	public class ModInitWatcher : MonoBehaviour
	{
		private float _nextCheck;

		private float _elapsed;

		private const float CHECK_INTERVAL = 0.25f;

		private const float TIMEOUT = 30f;

		public ModInitWatcher(IntPtr ptr)
			: base(ptr)
		{
		}

		private void Update()
		{
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Expected O, but got Unknown
			_elapsed += Time.deltaTime;
			if (_elapsed >= 30f)
			{
				((BasePlugin)Instance).Log.LogWarning((object)"[ModInitWatcher] Timeout (30s). Forcing deferred init.");
				DoInit();
			}
			else
			{
				if (_elapsed < _nextCheck)
				{
					return;
				}
				_nextCheck = _elapsed + 0.25f;
				try
				{
					if (!DestroyableSingleton<TranslationController>.InstanceExists)
					{
						return;
					}
					TranslationController instance = DestroyableSingleton<TranslationController>.Instance;
					if (instance.Languages != null && instance.Languages.Count != 0)
					{
						ManualLogSource log = ((BasePlugin)Instance).Log;
						bool flag = default(bool);
						BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(60, 1, ref flag);
						if (flag)
						{
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ModInitWatcher] Game initialized after ");
							((BepInExLogInterpolatedStringHandler)val).AppendFormatted<float>(_elapsed, "F1");
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral("s. Activating mod...");
						}
						log.LogInfo(val);
						DoInit();
					}
				}
				catch
				{
				}
			}
		}

		private void DoInit()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			try
			{
				Instance.RunDeferredInit();
			}
			catch (Exception ex)
			{
				ManualLogSource log = ((BasePlugin)Instance).Log;
				bool flag = default(bool);
				BepInExErrorLogInterpolatedStringHandler val = new BepInExErrorLogInterpolatedStringHandler(39, 1, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ModInitWatcher] Deferred init failed: ");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<Exception>(ex);
				}
				log.LogError(val);
			}
			try
			{
				Object.Destroy((Object)(object)this);
			}
			catch
			{
			}
		}
	}

	public class DebuggerComponent : MonoBehaviour
	{
		private class Star
		{
			public RectTransform Rect;

			public float Speed;

			public Image Image;

			public bool IsBright;
		}

		private sealed class YourKeysRow
		{
			public GameObject Root;

			public TextMeshProUGUI PreviewTxt;

			public TextMeshProUGUI DateTxt;

			public TextMeshProUGUI BadgeTxt;

			public Button CopyBtn;
		}

		private class FloatingCrewmate
		{
			public GameObject Root;

			public RectTransform Rect;

			public Vector2 Velocity;

			public float RotSpeed;

			public float BobFrequency;

			public float BobAmplitude;

			public float TimeOffset;
		}

		private readonly struct PopupTheme
		{
			public readonly Color Accent;

			public readonly Color AccentSoft;

			public readonly Color BadgeText;

			public readonly Color PrimaryButtonTop;

			public readonly Color PrimaryButtonBottom;

			public readonly Color SecondaryButtonTop;

			public readonly Color SecondaryButtonBottom;

			public PopupTheme(Color accent, Color accentSoft, Color badgeText, Color primaryButtonTop, Color primaryButtonBottom, Color secondaryButtonTop, Color secondaryButtonBottom)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				Accent = accent;
				AccentSoft = accentSoft;
				BadgeText = badgeText;
				PrimaryButtonTop = primaryButtonTop;
				PrimaryButtonBottom = primaryButtonBottom;
				SecondaryButtonTop = secondaryButtonTop;
				SecondaryButtonBottom = secondaryButtonBottom;
			}
		}

		private enum SavedKeyLoadResult
		{
			NotFound,
			Loaded,
			Invalid,
			Error
		}

		private sealed class PendingValidationResult
		{
			public readonly bool Success;

			public readonly string Message;

			public readonly string Username;

			public PendingValidationResult(bool s, string m, string u)
			{
				Success = s;
				Message = m;
				Username = u;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass238_0
		{
			public byte[] imageData;

			public int downloadState;

			public string downloadError;

			public string capturedUrl;

			public string capturedCachePath;

			internal async Task? _003CLoadDiscordAvatarCoroutine_003Eb__0()
			{
				var (array, text) = await DownloadAvatarBytesAsync(capturedUrl, CancellationToken.None);
				if (array != null && array.Length > 100)
				{
					imageData = array;
					downloadState = 1;
					if (capturedCachePath != null)
					{
						try
						{
							File.WriteAllBytes(capturedCachePath, array);
						}
						catch
						{
						}
					}
				}
				else
				{
					downloadError = text;
					downloadState = -1;
				}
			}
		}

		[CompilerGenerated]
		private sealed class _003CAnimatePopupContent_003Ed__315 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public RectTransform content;

			public Transform popup;

			public CanvasGroup canvasGroup;

			private float _003Cduration_003E5__2;

			private float _003Celapsed_003E5__3;

			object System.Collections.Generic.IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			object System.Collections.IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			[DebuggerHidden]
			public _003CAnimatePopupContent_003Ed__315(int _003C_003E1__state)
			{
				this._003C_003E1__state = _003C_003E1__state;
			}

			[DebuggerHidden]
			void System.IDisposable.Dispose()
			{
				_003C_003E1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
				//IL_0114: Unknown result type (might be due to invalid IL or missing references)
				//IL_011a: Unknown result type (might be due to invalid IL or missing references)
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_014a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0161: Unknown result type (might be due to invalid IL or missing references)
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					if ((Object)(object)content == (Object)null || (Object)(object)popup == (Object)null)
					{
						return false;
					}
					try
					{
						((Transform)content).localScale = Vector3.zero;
					}
					catch
					{
						return false;
					}
					try
					{
						popup.localScale = Vector3.zero;
					}
					catch
					{
					}
					if ((Object)(object)canvasGroup != (Object)null)
					{
						try
						{
							canvasGroup.alpha = 0f;
						}
						catch
						{
						}
					}
					_003Cduration_003E5__2 = 0.5f;
					_003Celapsed_003E5__3 = 0f;
					break;
				case 1:
					_003C_003E1__state = -1;
					break;
				}
				if (_003Celapsed_003E5__3 < _003Cduration_003E5__2)
				{
					if ((Object)(object)content == (Object)null)
					{
						return false;
					}
					_003Celapsed_003E5__3 += Time.deltaTime;
					float num = _003Celapsed_003E5__3 / _003Cduration_003E5__2;
					float num2 = Mathf.Sin(-13f * (num + 1f) * (float)Math.PI * 0.5f) * Mathf.Pow(2f, -10f * num) + 1f;
					try
					{
						((Transform)content).localScale = Vector3.one * num2;
					}
					catch
					{
						return false;
					}
					if (num > 0.3f && (Object)(object)popup != (Object)null)
					{
						try
						{
							popup.localScale = Vector3.one * Mathf.Clamp01((num - 0.3f) * 2f);
						}
						catch
						{
						}
					}
					if ((Object)(object)canvasGroup != (Object)null)
					{
						try
						{
							canvasGroup.alpha = Mathf.Clamp01(num * 2.5f);
						}
						catch
						{
						}
					}
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				if ((Object)(object)content != (Object)null)
				{
					try
					{
						((Transform)content).localScale = Vector3.one;
					}
					catch
					{
					}
				}
				if ((Object)(object)popup != (Object)null)
				{
					try
					{
						popup.localScale = Vector3.one;
					}
					catch
					{
					}
				}
				if ((Object)(object)canvasGroup != (Object)null)
				{
					try
					{
						canvasGroup.alpha = 1f;
					}
					catch
					{
					}
				}
				return false;
			}

			bool System.Collections.IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void System.Collections.IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}
		}

		[CompilerGenerated]
		private sealed class _003CLoadDiscordAvatarCoroutine_003Ed__238 : System.Collections.Generic.IEnumerator<object>, System.Collections.IEnumerator, System.IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public DebuggerComponent _003C_003E4__this;

			private _003C_003Ec__DisplayClass238_0 _003C_003E8__1;

			private float _003Ctimeout_003E5__2;

			object System.Collections.Generic.IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			object System.Collections.IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			[DebuggerHidden]
			public _003CLoadDiscordAvatarCoroutine_003Ed__238(int _003C_003E1__state)
			{
				this._003C_003E1__state = _003C_003E1__state;
			}

			[DebuggerHidden]
			void System.IDisposable.Dispose()
			{
				_003C_003E8__1 = null;
				_003C_003E1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_0316: Unknown result type (might be due to invalid IL or missing references)
				//IL_030f: Unknown result type (might be due to invalid IL or missing references)
				int num = _003C_003E1__state;
				DebuggerComponent debuggerComponent = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					_003C_003E8__1 = new _003C_003Ec__DisplayClass238_0();
					if (DiscordAuthManager.IsAvatarLoaded)
					{
						Debug.Log(Object.op_Implicit("[ModMenuCrew] Avatar already loaded, skipping coroutine download"));
						return false;
					}
					string avatarUrl = DiscordAuthManager.GetAvatarUrl();
					if (string.IsNullOrEmpty(avatarUrl))
					{
						Debug.LogWarning(Object.op_Implicit("[ModMenuCrew] Avatar URL is null — id:" + (DiscordAuthManager.DiscordId ?? "null") + " avatar:" + (DiscordAuthManager.DiscordAvatar ?? "null")));
						debuggerComponent._avatarLoadRequested = false;
						return false;
					}
					Debug.Log(Object.op_Implicit("[ModMenuCrew] Avatar download starting: " + avatarUrl));
					_003C_003E8__1.imageData = null;
					_003C_003E8__1.downloadState = 0;
					_003C_003E8__1.downloadError = null;
					string avatarCachePath = GetAvatarCachePath();
					_003C_003E8__1.capturedUrl = avatarUrl;
					_003C_003E8__1.capturedCachePath = avatarCachePath;
					Task.Run(async delegate
					{
						var (array, downloadError) = await DownloadAvatarBytesAsync(_003C_003E8__1.capturedUrl, CancellationToken.None);
						if (array != null && array.Length > 100)
						{
							_003C_003E8__1.imageData = array;
							_003C_003E8__1.downloadState = 1;
							if (_003C_003E8__1.capturedCachePath != null)
							{
								try
								{
									File.WriteAllBytes(_003C_003E8__1.capturedCachePath, array);
								}
								catch
								{
								}
							}
						}
						else
						{
							_003C_003E8__1.downloadError = downloadError;
							_003C_003E8__1.downloadState = -1;
						}
					});
					_003Ctimeout_003E5__2 = 30f;
					break;
				}
				case 1:
					_003C_003E1__state = -1;
					_003Ctimeout_003E5__2 -= Time.unscaledDeltaTime;
					break;
				}
				if (_003C_003E8__1.downloadState == 0 && _003Ctimeout_003E5__2 > 0f)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				if (_003C_003E8__1.downloadState == 0)
				{
					Debug.LogWarning(Object.op_Implicit("[ModMenuCrew] Avatar download timed out (30s)"));
					debuggerComponent._avatarLoadRequested = false;
					return false;
				}
				if (_003C_003E8__1.downloadState != 1 || _003C_003E8__1.imageData == null || _003C_003E8__1.imageData.Length < 100)
				{
					Debug.LogWarning(Object.op_Implicit("[ModMenuCrew] Avatar download failed: " + (_003C_003E8__1.downloadError ?? "unknown error")));
					debuggerComponent._avatarLoadRequested = false;
					return false;
				}
				Debug.Log(Object.op_Implicit($"[ModMenuCrew] Avatar downloaded ({_003C_003E8__1.imageData.Length} bytes), decoding PNG..."));
				Texture2D val = null;
				try
				{
					val = CreateAvatarTexture();
					bool flag = false;
					try
					{
						TinyPngDecoder.Decode(_003C_003E8__1.imageData, val);
						flag = true;
					}
					catch
					{
						try
						{
							flag = ImageConversion.LoadImage(val, Il2CppStructArray<byte>.op_Implicit(_003C_003E8__1.imageData));
						}
						catch
						{
						}
					}
					_003C_003E8__1.imageData = null;
					if (flag && ((Texture)val).width > 1 && ((Texture)val).height > 1)
					{
						DiscordAuthManager.SetAvatarTexture(val);
						val = null;
						try
						{
							if ((Object)(object)debuggerComponent._discordAvatarImage != (Object)null && (Object)(object)((Component)debuggerComponent._discordAvatarImage).gameObject != (Object)null)
							{
								debuggerComponent._discordAvatarImage.texture = (Texture)(object)DiscordAuthManager.AvatarTexture;
								if ((Object)(object)debuggerComponent._activationAvatarContainer != (Object)null && (Object)(object)((Component)debuggerComponent._activationAvatarContainer).gameObject != (Object)null)
								{
									((Component)debuggerComponent._activationAvatarContainer).gameObject.SetActive(true);
									((Transform)debuggerComponent._activationAvatarContainer).localScale = (debuggerComponent._bootSequenceComplete ? Vector3.one : Vector3.zero);
									if (!debuggerComponent._bootSequenceComplete)
									{
										debuggerComponent._activationAvatarAnimStart = Time.time;
									}
								}
								if ((Object)(object)debuggerComponent._discordLogoGO != (Object)null)
								{
									debuggerComponent._discordLogoGO.SetActive(false);
								}
							}
							if ((Object)(object)debuggerComponent._successPopupAvatarImage != (Object)null && (Object)(object)((Component)debuggerComponent._successPopupAvatarImage).gameObject != (Object)null)
							{
								debuggerComponent._successPopupAvatarImage.texture = (Texture)(object)DiscordAuthManager.AvatarTexture;
							}
						}
						catch
						{
						}
					}
					else
					{
						Object.Destroy((Object)(object)val);
						val = null;
					}
				}
				catch
				{
					if ((Object)(object)val != (Object)null)
					{
						try
						{
							Object.Destroy((Object)(object)val);
						}
						catch
						{
						}
					}
				}
				return false;
			}

			bool System.Collections.IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void System.Collections.IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}
		}

		private static DebuggerComponent _instance;

		public uint NetId;

		private float _customSpeed = -1f;

		private float _originalBaseSpeed = -1f;

		private bool _isCustomSpeedActive;

		private float _killCooldown = 10f;

		private string _pendingSavedKey;

		private const int BanPointsPerClick = 10;

		private static byte[] _ghostUIBytecode = null;

		private static bool _useGhostUI = true;

		private static bool _ghostUILastContext = false;

		private TeleportManager teleportManager;

		private CheatManager cheatManager;

		private PlayerPickMenu playerPickMenu;

		private BanMenu banMenu;

		private HostControlsTab hostControlsTab;

		private ExtrasTab extrasTab;

		private AnimationsTab animationsTab;

		private SpoofingMenu spoofingMenu;

		private SettingsTab settingsTab;

		private MiscTab miscTab;

		private bool _isModGloballyActivated;

		private float _lastSecurityPassTime;

		private const float SECURITY_GRACE_PERIOD = 30f;

		private float _lastZombieResetTime;

		private const float ZOMBIE_RESET_COOLDOWN = 30f;

		private string currentActivationStatusMessage = "Loading...";

		private bool isValidatingNow;

		private Task pendingValidationTask;

		private float _validationStartTime = -1f;

		private bool _validationCancelOffered;

		private float _lastGetKeyClickTime = -10f;

		private Canvas activationCanvasTMP;

		private TMP_InputField apiKeyInputFieldTMP;

		private TextMeshProUGUI statusMessageTextTMP;

		private Button validateButtonTMP;

		private TextMeshProUGUI validateButtonTextTMP;

		private Button getKeyButtonTMP;

		private Button _discordLoginBtnTMP;

		private TextMeshProUGUI _discordLoginTextTMP;

		private Button _copyLinkBtnTMP;

		private TextMeshProUGUI _copyLinkTextTMP;

		private Button _bindKeyBtnTMP;

		private GameObject activationPanelGO;

		private GameObject eventSystemGO;

		private GameObject canvasGO;

		private int inGameRoleGridIndex;

		private bool isLocalFakeImpostor;

		private Vector2 _lobbyRoleDropdownScroll = Vector2.zero;

		private Vector2 _bodyTypeDropdownScroll = Vector2.zero;

		private bool _showGameEndDropdown;

		private bool _showAnimationCheats;

		private bool hasAttemptedInitialActivationUIShow;

		private bool _wasServerDataLoaded;

		private const string SavedKeyFileName = "crewcore_key.txt";

		private bool _isAutoValidatingSavedKey;

		private GameObject successPopupGO;

		private TMP_FontAsset _cachedFont;

		private Texture2D _headerGradientTexture;

		private Sprite _headerGradientSprite;

		private Sprite _cachedValidateBtnSprite;

		private Sprite _cachedGetKeyBtnSprite;

		private Sprite _cachedOkBtnSprite;

		private Sprite _cachedSaveKeyBtnSprite;

		private Sprite _cachedWhiteSprite;

		private Texture2D _validateButtonGradientTexture;

		private Texture2D _getKeyButtonGradientTexture;

		private Texture2D _okButtonGradientTexture;

		private Texture2D _discordIconTexture;

		private Texture2D _successCircleTexture;

		private Sprite _successCircleSprite;

		private string _lastStatusMessage;

		private bool _lastValidatingState;

		private string _lastInputText;

		private CanvasGroup _panelCanvasGroup;

		private Image _validateBtnImage;

		private Outline _validateBtnOutline;

		private float _targetAlpha;

		private float _currentAlpha;

		private bool _shouldAutoFocus;

		private bool _bootSkipAllowed;

		private TextMeshProUGUI _bootSkipHintText;

		private float _bootSkipHintAlpha;

		private float _panelScaleCurrent = 0.92f;

		private const float PANEL_SCALE_TARGET = 1f;

		private const float PANEL_SCALE_SPEED = 6f;

		private float _revealStartTime = -1f;

		private bool _revealComplete;

		private List<CanvasGroup> _staggerElements = new List<CanvasGroup>();

		private static readonly float[] _staggerDelays = new float[9] { 0f, 0.08f, 0.16f, 0.24f, 0.32f, 0.4f, 0.48f, 0.56f, 0.64f };

		private Image _inputFieldBgImage;

		private Outline _inputFieldOutline;

		private float _inputGlowPhase;

		private Image _accentTopLine;

		private Image _accentBottomLine;

		private float _loadingDotsTimer;

		private int _loadingDotsCount;

		private static readonly string[] _loadingDotsTexts = new string[4] { "VALIDATING   ", "VALIDATING.  ", "VALIDATING.. ", "VALIDATING..." };

		private float _breathePhase;

		private Image _stepLine1;

		private Image _stepLine2;

		private Image _stepCircle1;

		private Image _stepCircle2;

		private Image _stepCircle3;

		private TextMeshProUGUI _stepLabel1;

		private TextMeshProUGUI _stepLabel2;

		private TextMeshProUGUI _stepLabel3;

		private Image _statusPillBg;

		private int _currentStep;

		private float _lastGameStateUpdateTime;

		private RoleTypes[] _cachedRoles;

		private string[] _cachedRoleNames;

		private const int MAX_STARS = 100;

		private List<Star> _stars = new List<Star>(100);

		private bool _bootSequenceComplete;

		private float _bootStartTime;

		private int _bootLineIndex;

		private int _bootCharIndex;

		private float _lastBootCharTime;

		private float _bootCharDelay = 0.005f;

		private TextMeshProUGUI _bootConsoleText;

		private GameObject _bootConsoleGO;

		private static readonly string[] _bootLines = new string[24]
		{
			"<color=#33FFFF>> MODMENUCREW // BOOT SEQUENCE</color> <color=#666666>v6.1.4b</color>", "<color=#333333>----------------------------------------------</color>", " ", "<color=#33FFFF>[INIT]</color> Loading core framework...", "<color=#00FF55>[OK]</color> BepInEx Runtime <color=#555555>............</color> <color=#00FF55>LOADED</color>", "<color=#00FF55>[OK]</color> Harmony Patcher <color=#555555>............</color> <color=#00FF55>READY</color>", "<color=#00FF55>[OK]</color> Il2Cpp Interop <color=#555555>.............</color> <color=#00FF55>BOUND</color>", " ", "<color=#FFAA00>[SCAN]</color> Scanning host environment...", "<color=#00FF55>[OK]</color> Among Us <color=#555555>...................</color> <color=#00FF55>PID 0x1A4F</color>",
			"<color=#00FF55>[OK]</color> GameAssembly.dll <color=#555555>...........</color> <color=#00FF55>MAPPED</color>", " ", "<color=#FF6B6B>[HOOK]</color> Injecting runtime patches...", "<color=#00FF55>  |--</color> PlayerControl <color=#555555>.............</color> <color=#00FF55>PATCHED</color>", "<color=#00FF55>  |--</color> ShipStatus <color=#555555>................</color> <color=#00FF55>PATCHED</color>", "<color=#00FF55>  |--</color> MeetingHud <color=#555555>................</color> <color=#00FF55>PATCHED</color>", " ", "<color=#FF3333>[!!]</color> <color=#FFAA00>EasyAntiCheat DETECTED</color>", "<color=#FF6B6B>[EAC]</color> Evading signatures <color=#555555>........</color> <color=#FFAA00>EVADING</color>", "<color=#00FF55>[EAC]</color> Bypass status <color=#555555>.............</color> <color=#00FF55>CLEAN</color>",
			" ", "<color=#333333>----------------------------------------------</color>", "<color=#00FF55>[DONE]</color> All systems operational", "<color=#FF3333>[AUTH]</color> <color=#FFFFFF>Awaiting license key...</color>"
		};

		private static readonly int[] _bootPlainLineLengths = ComputeBootPlainLineLengths();

		private string _bootDisplayedText = "";

		private Image _bootProgressFill;

		private TextMeshProUGUI _bootPhaseLabel;

		private TextMeshProUGUI _bootPercentLabel;

		private Image _bootScanlineOverlay;

		private CanvasGroup _bootConsoleCanvasGroup;

		private float _bootCursorBlinkTimer;

		private int _bootSpinnerIdx;

		private float _bootLastSpinnerTime;

		private const string BOOT_CURSOR_ON = "<color=#00FFD0>_</color>";

		private const string BOOT_CURSOR_OFF = " ";

		private string _lastBootDisplayedText;

		private string _lastBootCursor;

		private static readonly KeyCode[] _allKeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));

		private static readonly char[] MATRIX_CHARS = "01".ToCharArray();

		private GameObject _bootOverlay;

		private bool _avatarLoadRequested;

		private float _discordAnimStartTime = -1f;

		private bool _discordAnimPlayed;

		private const float AVATAR_ANIM_DURATION = 0.5f;

		private const float NAME_ANIM_DELAY = 0.2f;

		private const float NAME_ANIM_DURATION = 0.3f;

		private RawImage _discordAvatarImage;

		private RawImage _successPopupAvatarImage;

		private RectTransform _activationAvatarContainer;

		private GameObject _discordLogoGO;

		private Image _activationAvatarGlow;

		private float _activationAvatarAnimStart = -1f;

		private GUIStyle _cachedAvatarStyle;

		private Texture2D _cachedAvatarTexture;

		private Rect _f2GlowRect;

		private Rect _f2AvatarRect;

		private Rect _f2SparkleRect;

		private Rect _f2BadgeRect;

		private RectTransform _statusPillRT;

		private TextMeshProUGUI _discordLoginSubTextTMP;

		private RectTransform _yourKeysCardRT;

		private TextMeshProUGUI _yourKeysCounterTxt;

		private TextMeshProUGUI _yourKeysEmptyTxt;

		private YourKeysRow[] _yourKeysRows;

		private float _lastUserKeysFetchAt = -10f;

		private readonly List<UserKeyInfo> _displayedKeys = new List<UserKeyInfo>(4);

		private static readonly Dictionary<string, Texture2D> _activationAssetCache = new Dictionary<string, Texture2D>();

		private const float HOLD_DURATION = 2f;

		private TextMeshProUGUI _titleTextTMP;

		private RectTransform _scanLineRT;

		private float _glitchTimer;

		private bool _isGlitching;

		private string _originalTitle = "MODMENUCREW <color=#A78BFA>// ACCESS TERMINAL</color>";

		private static StringBuilder _glitchSb;

		private static string _avatarDirCache;

		private bool _avatarLazyLoadAttempted;

		private string _cachedUsernameLabel;

		private string _cachedUsernameSource;

		private static string _dashCache_greeting;

		private static int _dashCache_lastFrame = -1;

		private static string _dashCache_roleLine;

		private static string _dashCache_lastRoleName;

		private static float _dashSessionStart = -1f;

		private Dictionary<string, Action> _lobbyTabRegistry;

		private Dictionary<string, Action<long>> _tabDrawRegistry;

		private Vector2 _replayScroll;

		private string[] _cachedReplayFiles;

		private float _lastReplayFileScanTime = -999f;

		private const float REPLAY_FILE_SCAN_INTERVAL = 3f;

		private static int _tmpTextCounter;

		private List<FloatingCrewmate> _floatingCrewmates = new List<FloatingCrewmate>();

		private const int MAX_FLOATING_CREWMATES = 12;

		private float _spawnTimer;

		private Stack<FloatingCrewmate> _floatingCrewmatePool = new Stack<FloatingCrewmate>(12);

		private Sprite _cachedCircleSprite;

		private List<Color> _availableCrewmateColors = new List<Color>();

		private float _pulseTimer;

		private List<Star> _popupStars = new List<Star>();

		private float _popupStarLoopTime;

		private PendingValidationResult _pendingValidationResult;

		private volatile bool _hasPendingValidationResult;

		private CancellationTokenSource _activationCts;

		private string _lastWindowTitle = "";

		private float _lastTitleUpdateTime;

		private int _selectedLobbyIndex = -1;

		private GUIStyle _lobbyEmptyIconStyle;

		private bool _lobbyEmptyIconStyleIsMicro;

		private static readonly string[] _sortOptions = new string[6] { "Default", "Players ↓", "Players ↑", "Slots", "New", "Map" };

		private static readonly string[] _mapFilterOptions = new string[6] { "All", "Skeld", "Mira", "Polus", "Air", "Fungle" };

		private static readonly Color _colorWhite = Color.white;

		private static readonly Color _colorCyan = GuiStyles.Theme.Visor;

		private static readonly Color _colorGreen = GuiStyles.Theme.Success;

		private static readonly Color _colorGray = GuiStyles.Theme.TextMuted;

		private static readonly Color _colorOrange = GuiStyles.Theme.Warning;

		private static readonly Color _colorRed = GuiStyles.Theme.ImpostorRed;

		private static readonly Color _colorDimGray = GuiStyles.Theme.TextInactive;

		private static readonly Color[] _mapColors = (Color[])(object)new Color[6]
		{
			new Color(0.4f, 0.7f, 1f, 1f),
			new Color(0.9f, 0.5f, 0.8f, 1f),
			new Color(0.4f, 0.9f, 0.6f, 1f),
			new Color(0.4f, 0.7f, 1f, 1f),
			new Color(1f, 0.7f, 0.3f, 1f),
			new Color(0.7f, 1f, 0.4f, 1f)
		};

		private static readonly Dictionary<SystemTypes, string> _tpRoomNames = new Dictionary<SystemTypes, string>
		{
			{
				(SystemTypes)2,
				"☕ Cafeteria"
			},
			{
				(SystemTypes)12,
				"\ud83d\udd2b Weapons"
			},
			{
				(SystemTypes)5,
				"\ud83e\udded Navigation"
			},
			{
				(SystemTypes)8,
				"\ud83d\udca8 O2"
			},
			{
				(SystemTypes)6,
				"\ud83d\udcca Admin"
			},
			{
				(SystemTypes)7,
				"⚡ Electrical"
			},
			{
				(SystemTypes)13,
				"⬇ Lower Engine"
			},
			{
				(SystemTypes)4,
				"⬆ Upper Engine"
			},
			{
				(SystemTypes)11,
				"\ud83d\udcf9 Security"
			},
			{
				(SystemTypes)3,
				"☢ Reactor"
			},
			{
				(SystemTypes)10,
				"\ud83d\udc8a MedBay"
			},
			{
				(SystemTypes)1,
				"\ud83d\udce6 Storage"
			},
			{
				(SystemTypes)14,
				"\ud83d\udce1 Comms"
			},
			{
				(SystemTypes)9,
				"\ud83d\udee1 Shields"
			},
			{
				(SystemTypes)22,
				"\ud83c\udf05 Balcony"
			},
			{
				(SystemTypes)20,
				"\ud83d\udebf Locker Room"
			},
			{
				(SystemTypes)18,
				"\ud83e\uddea Decontam"
			},
			{
				(SystemTypes)21,
				"\ud83d\udd2c Laboratory"
			},
			{
				(SystemTypes)19,
				"\ud83d\ude80 Launchpad"
			},
			{
				(SystemTypes)23,
				"\ud83c\udfe2 Office"
			},
			{
				(SystemTypes)24,
				"\ud83c\udf3f Greenhouse"
			},
			{
				(SystemTypes)25,
				"\ud83d\udef8 Dropship"
			},
			{
				(SystemTypes)29,
				"\ud83d\udd25 Boiler Room"
			},
			{
				(SystemTypes)28,
				"\ud83e\uddeb Specimens"
			},
			{
				(SystemTypes)27,
				"\ud83c\udf32 Outside"
			},
			{
				(SystemTypes)40,
				"⛓ Brig"
			},
			{
				(SystemTypes)39,
				"⚙ Engine"
			},
			{
				(SystemTypes)33,
				"\ud83c\udf73 Kitchen"
			},
			{
				(SystemTypes)45,
				"\ud83c\udfdb Main Hall"
			},
			{
				(SystemTypes)44,
				"\ud83c\udf09 Gap Room"
			},
			{
				(SystemTypes)42,
				"\ud83d\udcc1 Records"
			},
			{
				(SystemTypes)36,
				"\ud83d\udce6 Cargo Bay"
			},
			{
				(SystemTypes)43,
				"\ud83d\udecb Lounge"
			},
			{
				(SystemTypes)38,
				"\ud83d\udebf Showers"
			},
			{
				(SystemTypes)41,
				"\ud83e\ude91 Meeting Room"
			},
			{
				(SystemTypes)30,
				"\ud83d\udd10 Vault"
			},
			{
				(SystemTypes)31,
				"✈ Cockpit"
			},
			{
				(SystemTypes)32,
				"⚔ Armory"
			},
			{
				(SystemTypes)34,
				"\ud83d\udd2d Viewing Deck"
			},
			{
				(SystemTypes)46,
				"\ud83c\udfe5 Medical"
			},
			{
				(SystemTypes)56,
				"\ud83d\udecf Sleeping"
			},
			{
				(SystemTypes)55,
				"\ud83c\udf34 Jungle"
			},
			{
				(SystemTypes)52,
				"\ud83d\udc41 Lookout"
			},
			{
				(SystemTypes)49,
				"⛏ Mining Pit"
			},
			{
				(SystemTypes)54,
				"\ud83c\udfd4 Highlands"
			},
			{
				(SystemTypes)53,
				"\ud83c\udfd6 Beach"
			},
			{
				(SystemTypes)50,
				"\ud83c\udfa3 Fishing Dock"
			},
			{
				(SystemTypes)51,
				"\ud83c\udfae Rec Room"
			},
			{
				(SystemTypes)26,
				"\ud83e\uddea Decontam 2"
			}
		};

		private List<LobbyListingPatch.CapturedLobby> _cachedLobbies;

		private float _lastLobbyRefresh;

		private const float LOBBY_CACHE_INTERVAL = 0.5f;

		private const int MAX_VISIBLE_LOBBIES = 30;

		private Vector2 _movementScrollPosition = Vector2.zero;

		private readonly List<KeyValuePair<SystemTypes, Vector2>> _movementLocsList = new List<KeyValuePair<SystemTypes, Vector2>>(32);

		private string _movementLocsCachedMap;

		private int _movementLocsCachedCount = -1;

		private static readonly Dictionary<SystemTypes, string> _locationNameCache = new Dictionary<SystemTypes, string>(32);

		private byte[] _cachedSabBytecode;

		private byte[] _cachedSabInverseMap;

		private long _cachedSabToken;

		private bool _isBindingToggleKey;

		private float _securityCheckTimer;

		private TextMeshProUGUI _toggleKeyTextTMP;

		private float _lastRealtimeFrame;

		private ServerData.UISnapshot _safeSnapshot;

		private bool _safeIsLoaded;

		private bool _safeCanRender;

		private byte[] _cachedMainInverseMap;

		private byte[] _cachedMainBytecode;

		private int _cachedMatrixScreenH = -1;

		private Vector2 _cachedMatrixRenderOffset = new Vector2(float.NaN, float.NaN);

		private Matrix4x4 _cachedRenderMatrix = Matrix4x4.identity;

		public bool DisableGameEnd { get; set; }

		public bool ForceImpostor { get; set; }

		public bool IsNoclipping { get; set; }

		public static bool BypassSecurity { get; set; }

		public float PlayerSpeed
		{
			get
			{
				if (!ModKeyValidator.V() || !_isCustomSpeedActive || !(_customSpeed > 0f))
				{
					return LobbySpeedMod;
				}
				return _customSpeed;
			}
			set
			{
				if (!_isCustomSpeedActive)
				{
					EnableCustomSpeed();
				}
				_customSpeed = Math.Max(0.5f, Math.Min(6f, value));
			}
		}

		public float LobbySpeedMod
		{
			get
			{
				try
				{
					GameOptionsManager instance = GameOptionsManager.Instance;
					if (((instance != null) ? instance.CurrentGameOptions : null) != null)
					{
						return GameOptionsManager.Instance.CurrentGameOptions.GetFloat((FloatOptionNames)2);
					}
				}
				catch
				{
				}
				return 1f;
			}
		}

		public bool IsCustomSpeedActive
		{
			get
			{
				return _isCustomSpeedActive;
			}
			set
			{
				if (!value)
				{
					ResetPlayerSpeed();
				}
				else
				{
					EnableCustomSpeed();
				}
			}
		}

		public float KillCooldown
		{
			get
			{
				if (!ModKeyValidator.V())
				{
					return 10f;
				}
				return _killCooldown;
			}
			set
			{
				if (ModKeyValidator.V())
				{
					_killCooldown = Math.Max(0f, value);
				}
			}
		}

		public bool NoKillCooldown
		{
			get
			{
				if (ModKeyValidator.V())
				{
					return CheatConfig.NoKillCooldown?.Value ?? false;
				}
				return false;
			}
			set
			{
				if (CheatConfig.NoKillCooldown != null)
				{
					CheatConfig.NoKillCooldown.Value = value;
				}
			}
		}

		public bool InstantWin { get; set; }

		public static bool IsMenuOpen => GhostUI.IsMouseOverAnyWindow();

		private bool isModGloballyActivated
		{
			get
			{
				if (!_isModGloballyActivated)
				{
					return false;
				}
				bool flag = ModKeyValidator.V();
				if (!ServerData.IsLoaded)
				{
					return false;
				}
				if (ServerData.Tabs == null || ServerData.Tabs.Count == 0)
				{
					return false;
				}
				bool flag2 = ServerGate.CanRender();
				if (flag && flag2)
				{
					_lastSecurityPassTime = Time.realtimeSinceStartup;
					return true;
				}
				if (_lastSecurityPassTime > 0f && Time.realtimeSinceStartup - _lastSecurityPassTime < 30f)
				{
					return true;
				}
				return false;
			}
			set
			{
				_isModGloballyActivated = value;
			}
		}

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr ShellExecuteW(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

		private static bool IsImpostorRoleSafe(RoleTypes role)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Invalid comparison between Unknown and I4
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Invalid comparison between Unknown and I4
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			try
			{
				return RoleManager.IsImpostorRole(role);
			}
			catch
			{
				return (int)role == 1 || (int)role == 5 || (int)role == 9 || (int)role == 18;
			}
		}

		internal static bool OpenBrowser(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return false;
			}
			if (!Uri.TryCreate(url, UriKind.Absolute, out Uri result) || (result.Scheme != "https" && result.Scheme != "http"))
			{
				return false;
			}
			try
			{
				if (ShellExecuteW(IntPtr.Zero, "open", url, null, null, 1).ToInt64() > 32)
				{
					return true;
				}
			}
			catch
			{
			}
			bool flag = false;
			try
			{
				flag = ModKeyValidator.IsRunningUnderWine;
			}
			catch
			{
			}
			if (flag)
			{
				try
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = "winebrowser",
						Arguments = url,
						UseShellExecute = false,
						CreateNoWindow = true
					});
					return true;
				}
				catch
				{
				}
				try
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = "xdg-open",
						Arguments = url,
						UseShellExecute = false,
						CreateNoWindow = true
					});
					return true;
				}
				catch
				{
				}
				try
				{
					string fileName = (File.Exists("Z:\\usr\\bin\\open") ? "Z:\\usr\\bin\\open" : "open");
					Process.Start(new ProcessStartInfo
					{
						FileName = fileName,
						Arguments = url,
						UseShellExecute = false,
						CreateNoWindow = true
					});
					return true;
				}
				catch
				{
				}
			}
			else
			{
				try
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = url,
						UseShellExecute = true
					});
					return true;
				}
				catch
				{
				}
				try
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = "cmd.exe",
						Arguments = "/c start \"\" \"" + url.Replace("&", "^&") + "\"",
						UseShellExecute = false,
						CreateNoWindow = true
					});
					return true;
				}
				catch
				{
				}
			}
			try
			{
				Application.OpenURL(url);
				return true;
			}
			catch
			{
			}
			return false;
		}

		public void EnableCustomSpeed()
		{
			if (!ModKeyValidator.V() || _isCustomSpeedActive)
			{
				return;
			}
			try
			{
				PlayerControl localPlayer = PlayerControl.LocalPlayer;
				if ((Object)(object)((localPlayer != null) ? localPlayer.MyPhysics : null) != (Object)null)
				{
					_originalBaseSpeed = PlayerControl.LocalPlayer.MyPhysics.Speed;
				}
			}
			catch
			{
			}
			if (_originalBaseSpeed <= 0f)
			{
				_originalBaseSpeed = 2.5f;
			}
			_customSpeed = _originalBaseSpeed;
			_isCustomSpeedActive = true;
		}

		public void ResetPlayerSpeed()
		{
			if (!_isCustomSpeedActive)
			{
				return;
			}
			_isCustomSpeedActive = false;
			_customSpeed = -1f;
			try
			{
				PlayerControl localPlayer = PlayerControl.LocalPlayer;
				if ((Object)(object)((localPlayer != null) ? localPlayer.MyPhysics : null) != (Object)null && _originalBaseSpeed > 0f)
				{
					PlayerControl.LocalPlayer.MyPhysics.Speed = _originalBaseSpeed;
				}
			}
			catch
			{
			}
			_originalBaseSpeed = -1f;
		}

		private void FixedUpdate()
		{
			if (PlayerControl.AllPlayerControls == null)
			{
				return;
			}
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && !((Object)(object)current == (Object)(object)PlayerControl.LocalPlayer) && (Object)(object)current.Data != (Object)null && (Object)(object)current.Data.Role != (Object)null && current.Data.Role.IsImpostor && !current.Data.IsDead && current.killTimer > 0f)
				{
					current.killTimer -= Time.fixedDeltaTime;
					if (current.killTimer < 0f)
					{
						current.killTimer = 0f;
					}
				}
			}
		}

		private static int[] ComputeBootPlainLineLengths()
		{
			int[] array = new int[_bootLines.Length];
			for (int i = 0; i < _bootLines.Length; i++)
			{
				string text = _bootLines[i];
				bool flag = false;
				int num = 0;
				for (int j = 0; j < text.Length; j++)
				{
					switch (text[j])
					{
					case '<':
						flag = true;
						continue;
					case '>':
						flag = false;
						continue;
					}
					if (!flag)
					{
						num++;
					}
				}
				array[i] = num;
			}
			return array;
		}

		private void ResetDiscordAnimation()
		{
			_discordAnimStartTime = -1f;
			_discordAnimPlayed = false;
			_avatarLazyLoadAttempted = false;
			_cachedAvatarStyle = null;
			_cachedAvatarTexture = null;
			_cachedUsernameLabel = null;
			_cachedUsernameSource = null;
		}

		private void UpdateGlitchText()
		{
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_titleTextTMP == (Object)null)
			{
				return;
			}
			_glitchTimer += Time.deltaTime;
			if (!_isGlitching)
			{
				if (_glitchTimer > Random.Range(2f, 5f))
				{
					_isGlitching = true;
					_glitchTimer = 0f;
				}
			}
			else if (_glitchTimer < Random.Range(0.1f, 0.3f))
			{
				if (_glitchSb == null)
				{
					_glitchSb = new StringBuilder(64);
				}
				_glitchSb.Clear();
				StringBuilder glitchSb = _glitchSb;
				bool flag = false;
				for (int i = 0; i < _originalTitle.Length; i++)
				{
					char c = _originalTitle[i];
					switch (c)
					{
					case '<':
						flag = true;
						glitchSb.Append(c);
						continue;
					case '>':
						flag = false;
						glitchSb.Append(c);
						continue;
					}
					if (!flag && char.IsLetterOrDigit(c) && Random.value > 0.6f)
					{
						glitchSb.Append(MATRIX_CHARS[Random.Range(0, MATRIX_CHARS.Length)]);
					}
					else
					{
						glitchSb.Append(c);
					}
				}
				((TMP_Text)_titleTextTMP).text = glitchSb.ToString();
				if (Random.value > 0.5f)
				{
					((Graphic)_titleTextTMP).color = new Color(0.2f, 0.8f, 1f);
				}
				else
				{
					((Graphic)_titleTextTMP).color = new Color(0.2f, 1f, 1f);
				}
			}
			else
			{
				_isGlitching = false;
				_glitchTimer = 0f;
				((TMP_Text)_titleTextTMP).text = _originalTitle;
				((Graphic)_titleTextTMP).color = Color.white;
			}
		}

		private static Texture2D LoadActivationAsset(string fileName)
		{
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Expected O, but got Unknown
			if (_activationAssetCache.TryGetValue(fileName, out var value) && (Object)(object)value != (Object)null)
			{
				return value;
			}
			Texture2D val = null;
			try
			{
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				string name = "ModMenuCrew." + fileName;
				Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(name);
				if (manifestResourceStream == null)
				{
					name = "ModMenuCrew.Resources." + fileName;
					manifestResourceStream = executingAssembly.GetManifestResourceStream(name);
				}
				if (manifestResourceStream == null)
				{
					Debug.LogWarning(Object.op_Implicit("[ActivationUI] Embedded missing: " + fileName));
					return null;
				}
				using (manifestResourceStream)
				{
					using MemoryStream memoryStream = new MemoryStream();
					manifestResourceStream.CopyTo(memoryStream);
					byte[] array = memoryStream.ToArray();
					val = new Texture2D(2, 2, (TextureFormat)4, false)
					{
						filterMode = (FilterMode)1,
						wrapMode = (TextureWrapMode)1,
						hideFlags = (HideFlags)61
					};
					bool flag = false;
					try
					{
						flag = ImageConversion.LoadImage(val, Il2CppStructArray<byte>.op_Implicit(array), true);
					}
					catch
					{
					}
					if (!flag)
					{
						try
						{
							TinyPngDecoder.Decode(array, val);
							flag = true;
						}
						catch
						{
						}
					}
					if (!flag || ((Texture)val).width <= 1 || ((Texture)val).height <= 1)
					{
						if ((Object)(object)val != (Object)null)
						{
							Object.Destroy((Object)(object)val);
						}
						return null;
					}
					_activationAssetCache[fileName] = val;
					return val;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(Object.op_Implicit("[ActivationUI] Load " + fileName + ": " + ex.Message));
				if ((Object)(object)val != (Object)null)
				{
					try
					{
						Object.Destroy((Object)(object)val);
					}
					catch
					{
					}
				}
				return null;
			}
		}

		private void CreateBootConsole(RectTransform parent)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Expected O, but got Unknown
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Expected O, but got Unknown
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Expected O, but got Unknown
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0276: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d1: Expected O, but got Unknown
			//IL_0341: Unknown result type (might be due to invalid IL or missing references)
			//IL_036c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0381: Unknown result type (might be due to invalid IL or missing references)
			//IL_038c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0396: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ac: Expected O, but got Unknown
			//IL_041e: Unknown result type (might be due to invalid IL or missing references)
			//IL_044a: Unknown result type (might be due to invalid IL or missing references)
			//IL_045f: Unknown result type (might be due to invalid IL or missing references)
			//IL_046a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0474: Unknown result type (might be due to invalid IL or missing references)
			//IL_0483: Unknown result type (might be due to invalid IL or missing references)
			//IL_048a: Expected O, but got Unknown
			//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_04de: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_052e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0549: Unknown result type (might be due to invalid IL or missing references)
			//IL_0550: Expected O, but got Unknown
			//IL_05c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0603: Unknown result type (might be due to invalid IL or missing references)
			//IL_0618: Unknown result type (might be due to invalid IL or missing references)
			//IL_062d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0641: Unknown result type (might be due to invalid IL or missing references)
			_bootConsoleGO = new GameObject("BootConsole");
			_bootConsoleGO.transform.SetParent((Transform)(object)parent, false);
			RectTransform obj = _bootConsoleGO.AddComponent<RectTransform>();
			obj.anchorMin = Vector2.zero;
			obj.anchorMax = Vector2.one;
			obj.sizeDelta = new Vector2(-40f, -40f);
			obj.anchoredPosition = Vector2.zero;
			_bootConsoleCanvasGroup = _bootConsoleGO.AddComponent<CanvasGroup>();
			_bootConsoleCanvasGroup.alpha = 1f;
			Image obj2 = _bootConsoleGO.AddComponent<Image>();
			((Graphic)obj2).color = new Color(0.03f, 0.03f, 0.05f, 0.97f);
			((Graphic)obj2).raycastTarget = false;
			GameObject val = new GameObject("BootConsoleText");
			val.transform.SetParent(_bootConsoleGO.transform, false);
			RectTransform obj3 = val.AddComponent<RectTransform>();
			obj3.anchorMin = new Vector2(0f, 0.13f);
			obj3.anchorMax = Vector2.one;
			obj3.sizeDelta = new Vector2(-24f, -16f);
			obj3.anchoredPosition = Vector2.zero;
			_bootConsoleText = val.AddComponent<TextMeshProUGUI>();
			((TMP_Text)_bootConsoleText).font = LoadGameFont();
			((TMP_Text)_bootConsoleText).fontSize = 13f;
			((TMP_Text)_bootConsoleText).alignment = (TextAlignmentOptions)257;
			((Graphic)_bootConsoleText).color = new Color(0f, 1f, 0.3f);
			((TMP_Text)_bootConsoleText).richText = true;
			((TMP_Text)_bootConsoleText).overflowMode = (TextOverflowModes)3;
			((TMP_Text)_bootConsoleText).text = "";
			GameObject val2 = new GameObject("BootProgressBG");
			val2.transform.SetParent(_bootConsoleGO.transform, false);
			RectTransform obj4 = val2.AddComponent<RectTransform>();
			obj4.anchorMin = new Vector2(0.03f, 0.04f);
			obj4.anchorMax = new Vector2(0.75f, 0.075f);
			obj4.sizeDelta = Vector2.zero;
			obj4.anchoredPosition = Vector2.zero;
			Image obj5 = val2.AddComponent<Image>();
			((Graphic)obj5).color = new Color(0.08f, 0.09f, 0.11f, 1f);
			((Graphic)obj5).raycastTarget = false;
			GameObject val3 = new GameObject("BootProgressFill");
			val3.transform.SetParent(val2.transform, false);
			RectTransform obj6 = val3.AddComponent<RectTransform>();
			obj6.anchorMin = Vector2.zero;
			obj6.anchorMax = new Vector2(0f, 1f);
			obj6.sizeDelta = Vector2.zero;
			obj6.anchoredPosition = Vector2.zero;
			_bootProgressFill = val3.AddComponent<Image>();
			((Graphic)_bootProgressFill).color = new Color(0.2f, 1f, 0.85f, 0.9f);
			((Graphic)_bootProgressFill).raycastTarget = false;
			GameObject val4 = new GameObject("BootPhaseLabel");
			val4.transform.SetParent(_bootConsoleGO.transform, false);
			_bootPhaseLabel = val4.AddComponent<TextMeshProUGUI>();
			((TMP_Text)_bootPhaseLabel).font = LoadGameFont();
			((TMP_Text)_bootPhaseLabel).fontSize = 11f;
			((TMP_Text)_bootPhaseLabel).alignment = (TextAlignmentOptions)1025;
			((Graphic)_bootPhaseLabel).color = new Color(0.4f, 0.9f, 1f, 0.85f);
			((TMP_Text)_bootPhaseLabel).text = "<color=#33FFFF>[|]</color> INITIALIZING";
			RectTransform component = val4.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0.03f, 0.075f);
			component.anchorMax = new Vector2(0.5f, 0.12f);
			component.sizeDelta = Vector2.zero;
			component.anchoredPosition = Vector2.zero;
			GameObject val5 = new GameObject("BootPercentLabel");
			val5.transform.SetParent(_bootConsoleGO.transform, false);
			_bootPercentLabel = val5.AddComponent<TextMeshProUGUI>();
			((TMP_Text)_bootPercentLabel).font = LoadGameFont();
			((TMP_Text)_bootPercentLabel).fontSize = 12f;
			((TMP_Text)_bootPercentLabel).alignment = (TextAlignmentOptions)4100;
			((Graphic)_bootPercentLabel).color = new Color(0.2f, 1f, 0.85f, 0.9f);
			((TMP_Text)_bootPercentLabel).text = "0%";
			RectTransform component2 = val5.GetComponent<RectTransform>();
			component2.anchorMin = new Vector2(0.76f, 0.03f);
			component2.anchorMax = new Vector2(0.97f, 0.09f);
			component2.sizeDelta = Vector2.zero;
			component2.anchoredPosition = Vector2.zero;
			GameObject val6 = new GameObject("BootScanline");
			val6.transform.SetParent(_bootConsoleGO.transform, false);
			RectTransform obj7 = val6.AddComponent<RectTransform>();
			obj7.anchorMin = new Vector2(0f, 0f);
			obj7.anchorMax = new Vector2(1f, 0f);
			obj7.pivot = new Vector2(0.5f, 0f);
			obj7.sizeDelta = new Vector2(0f, 3f);
			obj7.anchoredPosition = Vector2.zero;
			_bootScanlineOverlay = val6.AddComponent<Image>();
			((Graphic)_bootScanlineOverlay).color = new Color(0.2f, 1f, 0.9f, 0.05f);
			((Graphic)_bootScanlineOverlay).raycastTarget = false;
			GameObject val7 = new GameObject("BootSkipHint");
			val7.transform.SetParent(_bootConsoleGO.transform, false);
			_bootSkipHintText = val7.AddComponent<TextMeshProUGUI>();
			((TMP_Text)_bootSkipHintText).font = LoadGameFont();
			((TMP_Text)_bootSkipHintText).fontSize = 10f;
			((TMP_Text)_bootSkipHintText).alignment = (TextAlignmentOptions)1028;
			((Graphic)_bootSkipHintText).color = new Color(0.4f, 0.4f, 0.5f, 0f);
			((TMP_Text)_bootSkipHintText).text = "[ CLICK or PRESS ANY KEY to skip ]";
			RectTransform component3 = val7.GetComponent<RectTransform>();
			component3.anchorMin = new Vector2(0f, 0f);
			component3.anchorMax = new Vector2(1f, 0f);
			component3.pivot = new Vector2(1f, 0f);
			component3.sizeDelta = new Vector2(-20f, 24f);
			component3.anchoredPosition = new Vector2(0f, 6f);
			_bootSequenceComplete = false;
			_bootStartTime = Time.realtimeSinceStartup;
			_bootLineIndex = 0;
			_bootCharIndex = 0;
			_bootSkipAllowed = false;
			_bootSkipHintAlpha = 0f;
			_bootCursorBlinkTimer = 0f;
			_bootSpinnerIdx = 0;
			_bootLastSpinnerTime = 0f;
			_bootDisplayedText = "";
			_lastBootDisplayedText = null;
			_lastBootCursor = null;
			int num = 0;
			string[] bootLines = _bootLines;
			foreach (string text in bootLines)
			{
				bool flag = false;
				for (int j = 0; j < text.Length; j++)
				{
					if (text[j] == '<')
					{
						flag = true;
					}
					else if (text[j] == '>')
					{
						flag = false;
					}
					else if (!flag)
					{
						num++;
					}
				}
			}
			_bootCharDelay = ((num > 0) ? (3.5f / (float)num) : 0.005f);
		}

		public DebuggerComponent(IntPtr ptr)
			: base(ptr)
		{
		}//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)


		private void DestroySpriteAndTexture(ref Sprite sprite)
		{
			if ((Object)(object)sprite == (Object)null)
			{
				return;
			}
			try
			{
				Texture2D texture = sprite.texture;
				Object.Destroy((Object)(object)sprite);
				sprite = null;
				if ((Object)(object)texture != (Object)null && (Object)(object)texture != (Object)(object)Texture2D.whiteTexture)
				{
					Object.Destroy((Object)(object)texture);
				}
			}
			catch
			{
				sprite = null;
			}
		}

		public void CleanupResources()
		{
			try
			{
				CleanupActivationUI();
				if ((Object)(object)successPopupGO != (Object)null)
				{
					Object.Destroy((Object)(object)successPopupGO);
					successPopupGO = null;
				}
				if ((Object)(object)_headerGradientSprite != (Object)null)
				{
					Object.Destroy((Object)(object)_headerGradientSprite);
					_headerGradientSprite = null;
				}
				if ((Object)(object)_headerGradientTexture != (Object)null)
				{
					Object.Destroy((Object)(object)_headerGradientTexture);
					_headerGradientTexture = null;
				}
				DestroySpriteAndTexture(ref _cachedValidateBtnSprite);
				DestroySpriteAndTexture(ref _cachedGetKeyBtnSprite);
				DestroySpriteAndTexture(ref _cachedOkBtnSprite);
				DestroySpriteAndTexture(ref _cachedSaveKeyBtnSprite);
				DestroySpriteAndTexture(ref _cachedWhiteSprite);
				DestroySpriteAndTexture(ref _cachedCircleSprite);
				if ((Object)(object)_validateButtonGradientTexture != (Object)null)
				{
					Object.Destroy((Object)(object)_validateButtonGradientTexture);
					_validateButtonGradientTexture = null;
				}
				if ((Object)(object)_getKeyButtonGradientTexture != (Object)null)
				{
					Object.Destroy((Object)(object)_getKeyButtonGradientTexture);
					_getKeyButtonGradientTexture = null;
				}
				if ((Object)(object)_okButtonGradientTexture != (Object)null)
				{
					Object.Destroy((Object)(object)_okButtonGradientTexture);
					_okButtonGradientTexture = null;
				}
				if ((Object)(object)_successCircleSprite != (Object)null)
				{
					Object.Destroy((Object)(object)_successCircleSprite);
					_successCircleSprite = null;
				}
				if ((Object)(object)_successCircleTexture != (Object)null)
				{
					Object.Destroy((Object)(object)_successCircleTexture);
					_successCircleTexture = null;
				}
				if ((Object)(object)_discordIconTexture != (Object)null)
				{
					Object.Destroy((Object)(object)_discordIconTexture);
					_discordIconTexture = null;
				}
				if ((Object)(object)eventSystemGO != (Object)null)
				{
					Object.Destroy((Object)(object)eventSystemGO);
					eventSystemGO = null;
				}
				teleportManager = null;
				cheatManager = null;
				playerPickMenu = null;
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error resource cleanup: {value}"));
			}
		}

		private void OnDestroy()
		{
			CleanupResources();
		}

		private void Awake()
		{
			//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ce: Expected O, but got Unknown
			//IL_0404: Unknown result type (might be due to invalid IL or missing references)
			//IL_040b: Expected O, but got Unknown
			//IL_0361: Unknown result type (might be due to invalid IL or missing references)
			//IL_0368: Expected O, but got Unknown
			//IL_0309: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Expected O, but got Unknown
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Expected O, but got Unknown
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			_instance = this;
			try
			{
				((BasePlugin)Instance).Log.LogInfo((object)"DebuggerComponent: Awake started.");
				isModGloballyActivated = false;
				currentActivationStatusMessage = "Enter your activation key.";
				InitializeFeatureManagers();
				InitializeMainWindowIMGUI();
				InitializeTabsForGameIMGUI();
				IntegrityGuard.Initialize();
				DiscordAuthManager.TryRestoreSession();
				DiscordAuthManager.OnLoginComplete += OnDiscordLoginComplete;
				bool flag = default(bool);
				BepInExInfoLogInterpolatedStringHandler val2;
				if (!isModGloballyActivated)
				{
					currentActivationStatusMessage = "Enter your activation key or get a new one.";
					SetupActivationUI_TMP();
					Debug.Log(Object.op_Implicit($"[ModMenuCrew] Avatar check: HasProfile={DiscordAuthManager.HasDiscordProfile}, LoadRequested={_avatarLoadRequested}, ID={DiscordAuthManager.DiscordId ?? "null"}"));
					if (DiscordAuthManager.HasDiscordProfile && !_avatarLoadRequested)
					{
						if (TryLoadAvatarFromCache())
						{
							_avatarLoadRequested = true;
							if ((Object)(object)_discordAvatarImage != (Object)null && (Object)(object)DiscordAuthManager.AvatarTexture != (Object)null)
							{
								_discordAvatarImage.texture = (Texture)(object)DiscordAuthManager.AvatarTexture;
								if ((Object)(object)_activationAvatarContainer != (Object)null)
								{
									((Component)_activationAvatarContainer).gameObject.SetActive(true);
									((Transform)_activationAvatarContainer).localScale = Vector3.one;
								}
								if ((Object)(object)_discordLogoGO != (Object)null)
								{
									_discordLogoGO.SetActive(false);
								}
							}
							if ((Object)(object)_successPopupAvatarImage != (Object)null && (Object)(object)DiscordAuthManager.AvatarTexture != (Object)null)
							{
								_successPopupAvatarImage.texture = (Texture)(object)DiscordAuthManager.AvatarTexture;
							}
						}
						else
						{
							_avatarLoadRequested = true;
							((MonoBehaviour)this).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(LoadDiscordAvatarCoroutine()));
						}
					}
					try
					{
						string key;
						string detail;
						SavedKeyLoadResult savedKeyLoadResult = TryLoadSavedKeyFromFile(out key, out detail);
						switch (savedKeyLoadResult)
						{
						case SavedKeyLoadResult.Loaded:
							if (DiscordAuthManager.IsLoggedIn)
							{
								_isAutoValidatingSavedKey = true;
								hasAttemptedInitialActivationUIShow = true;
								_pendingSavedKey = null;
								currentActivationStatusMessage = "Validating saved key, please wait...";
								isValidatingNow = true;
								_validationStartTime = Time.realtimeSinceStartup;
								_validationCancelOffered = false;
								pendingValidationTask = ValidateKeyAndSetState(key);
								if ((Object)(object)activationCanvasTMP != (Object)null && ((Component)activationCanvasTMP).gameObject.activeSelf)
								{
									((Component)activationCanvasTMP).gameObject.SetActive(false);
								}
								ManageActivationUIVisibility();
								ManualLogSource log2 = ((BasePlugin)Instance).Log;
								val2 = new BepInExInfoLogInterpolatedStringHandler(69, 1, ref flag);
								if (flag)
								{
									((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[ModMenuCrew] Discord session active. Auto-validating saved key from ");
									((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(GetSavedKeyFilePath());
								}
								log2.LogInfo(val2);
							}
							else
							{
								_pendingSavedKey = key;
								_isAutoValidatingSavedKey = false;
								hasAttemptedInitialActivationUIShow = false;
								currentActivationStatusMessage = "Saved key found. Please login to Discord to verify ownership.";
								if ((Object)(object)activationCanvasTMP != (Object)null && !((Component)activationCanvasTMP).gameObject.activeSelf)
								{
									((Component)activationCanvasTMP).gameObject.SetActive(true);
									_shouldAutoFocus = false;
								}
								ManageActivationUIVisibility();
								ManualLogSource log3 = ((BasePlugin)Instance).Log;
								val2 = new BepInExInfoLogInterpolatedStringHandler(94, 0, ref flag);
								if (flag)
								{
									((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[ModMenuCrew] Saved key found but Discord not logged in. Waiting for Discord authentication...");
								}
								log3.LogInfo(val2);
							}
							break;
						case SavedKeyLoadResult.Invalid:
						case SavedKeyLoadResult.Error:
							_isAutoValidatingSavedKey = false;
							_pendingSavedKey = null;
							currentActivationStatusMessage = "Your saved key is no longer valid. Visit crewcore.online to regenerate.";
							if (!string.IsNullOrWhiteSpace(detail))
							{
								ManualLogSource log = ((BasePlugin)Instance).Log;
								BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(39, 2, ref flag);
								if (flag)
								{
									((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ModMenuCrew] Saved key file issue (");
									((BepInExLogInterpolatedStringHandler)val).AppendFormatted<SavedKeyLoadResult>(savedKeyLoadResult);
									((BepInExLogInterpolatedStringHandler)val).AppendLiteral("): ");
									((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(detail);
								}
								log.LogWarning(val);
							}
							break;
						}
					}
					catch (Exception ex)
					{
						_isAutoValidatingSavedKey = false;
						_pendingSavedKey = null;
						currentActivationStatusMessage = "Your saved key is no longer valid. Visit crewcore.online to regenerate.";
						ManualLogSource log4 = ((BasePlugin)Instance).Log;
						BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(47, 1, ref flag);
						if (flag)
						{
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ModMenuCrew] Saved key auto-validation error: ");
							((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(ex.Message);
						}
						log4.LogWarning(val);
					}
				}
				ManualLogSource log5 = ((BasePlugin)Instance).Log;
				val2 = new BepInExInfoLogInterpolatedStringHandler(51, 1, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("DebuggerComponent: Awake completed. Mod initially ");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(isModGloballyActivated ? "activated" : "deactivated");
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(".");
				}
				log5.LogInfo(val2);
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Critical error DebuggerComponent.Awake: {value}"));
			}
		}

		private void Start()
		{
			if (Instance.HasForeignPlugins())
			{
				((BasePlugin)Instance).Log.LogError((object)"[SECURITY] CRITICAL: Foreign plugin detected in START CHECK! Bypass attempt?");
				((BasePlugin)Instance).Log.LogError((object)"[SECURITY] Shutting down to prevent bypass.");
				((BasePlugin)Instance).Unload();
				Application.Quit();
			}
		}

		private void OnDiscordLoginComplete(bool success, string message)
		{
			if ((Object)(object)_discordLoginTextTMP != (Object)null)
			{
				string text = DiscordAuthManager.DiscordUsernameSafe ?? string.Empty;
				if (text.Length > 12)
				{
					text = text.Substring(0, 11) + "...";
				}
				((TMP_Text)_discordLoginTextTMP).text = (success ? ("CONTINUE AS " + text) : "CONTINUE WITH DISCORD");
			}
			if ((Object)(object)_discordLoginSubTextTMP != (Object)null)
			{
				((TMP_Text)_discordLoginSubTextTMP).text = (success ? "Click to logout" : "Click to login with Discord");
			}
			if (success)
			{
				UpdateStepIndicator(2);
			}
			if ((Object)(object)statusMessageTextTMP != (Object)null)
			{
				currentActivationStatusMessage = message;
				((TMP_Text)statusMessageTextTMP).text = message;
			}
			if (success)
			{
				try
				{
					RefreshUserKeys();
				}
				catch
				{
				}
			}
			else
			{
				try
				{
					UserKeysService.Clear();
					RenderUserKeysList();
				}
				catch
				{
				}
			}
			if (success && !_avatarLoadRequested)
			{
				_avatarLoadRequested = true;
				((MonoBehaviour)this).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(LoadDiscordAvatarCoroutine()));
			}
			if (success && !string.IsNullOrEmpty(_pendingSavedKey))
			{
				((BasePlugin)Instance).Log.LogInfo((object)"[ModMenuCrew] Discord authenticated. Validating pending saved key...");
				string pendingSavedKey = _pendingSavedKey;
				_pendingSavedKey = null;
				currentActivationStatusMessage = "Verifying key ownership...";
				if ((Object)(object)statusMessageTextTMP != (Object)null)
				{
					((TMP_Text)statusMessageTextTMP).text = currentActivationStatusMessage;
				}
				_isAutoValidatingSavedKey = true;
				isValidatingNow = true;
				_validationStartTime = Time.realtimeSinceStartup;
				_validationCancelOffered = false;
				pendingValidationTask = ValidateKeyAndSetState(pendingSavedKey);
				if ((Object)(object)activationCanvasTMP != (Object)null && ((Component)activationCanvasTMP).gameObject.activeSelf)
				{
					((Component)activationCanvasTMP).gameObject.SetActive(false);
				}
				ManageActivationUIVisibility();
			}
		}

		private static Texture2D CreateAvatarTexture()
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			return new Texture2D(2, 2, (TextureFormat)4, false)
			{
				wrapMode = (TextureWrapMode)1,
				filterMode = (FilterMode)1,
				hideFlags = (HideFlags)61
			};
		}

		private static string GetAvatarCachePath()
		{
			try
			{
				string discordId = DiscordAuthManager.DiscordId;
				if (string.IsNullOrEmpty(discordId))
				{
					return null;
				}
				if (_avatarDirCache == null)
				{
					string gameRootPathSafe = GetGameRootPathSafe();
					if (string.IsNullOrEmpty(gameRootPathSafe))
					{
						return null;
					}
					_avatarDirCache = Path.Combine(gameRootPathSafe, "BepInEx", "plugins", "avatars");
					if (!Directory.Exists(_avatarDirCache))
					{
						Directory.CreateDirectory(_avatarDirCache);
					}
				}
				string discordAvatar = DiscordAuthManager.DiscordAvatar;
				string text = ((!string.IsNullOrEmpty(discordAvatar)) ? discordAvatar : "default");
				return Path.Combine(_avatarDirCache, discordId + "_" + text + ".png");
			}
			catch
			{
				return null;
			}
		}

		private bool TryLoadAvatarFromCache()
		{
			if (DiscordAuthManager.IsAvatarLoaded)
			{
				return true;
			}
			Texture2D val = null;
			try
			{
				string avatarCachePath = GetAvatarCachePath();
				if (avatarCachePath == null || !File.Exists(avatarCachePath))
				{
					return false;
				}
				byte[] array = File.ReadAllBytes(avatarCachePath);
				if (array == null || array.Length < 100)
				{
					return false;
				}
				val = CreateAvatarTexture();
				bool flag = false;
				try
				{
					TinyPngDecoder.Decode(array, val);
					flag = true;
				}
				catch
				{
					try
					{
						flag = ImageConversion.LoadImage(val, Il2CppStructArray<byte>.op_Implicit(array));
					}
					catch
					{
					}
				}
				if (flag && ((Texture)val).width > 1 && ((Texture)val).height > 1)
				{
					DiscordAuthManager.SetAvatarTexture(val);
					return true;
				}
				Object.Destroy((Object)(object)val);
				val = null;
			}
			catch
			{
				if ((Object)(object)val != (Object)null)
				{
					try
					{
						Object.Destroy((Object)(object)val);
					}
					catch
					{
					}
				}
			}
			return false;
		}

		private static async Task<(byte[] data, string error)> DownloadAvatarBytesAsync(string url, CancellationToken cancellationToken)
		{
			string item = null;
			try
			{
				HttpClient cdnClient = DiscordAuthManager.GetCdnClient();
				if (cdnClient != null)
				{
					using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
					cts.CancelAfter(TimeSpan.FromSeconds(12.0));
					try
					{
						using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
						using HttpResponseMessage httpResponseMessage = await cdnClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cts.Token);
						byte[] array = await httpResponseMessage.Content.ReadAsByteArrayAsync();
						if (array != null && array.Length > 100)
						{
							return (data: array, error: null);
						}
						item = $"HttpClient returned {((array != null) ? array.Length : 0)} bytes";
					}
					catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
					{
						return (data: null, error: "cancelled");
					}
					catch (OperationCanceledException)
					{
						item = "HttpClient timed out (12s)";
					}
				}
				else
				{
					item = "CDN HttpClient is null";
				}
			}
			catch (Exception ex3)
			{
				item = "HttpClient: " + ex3.GetBaseException().Message;
			}
			try
			{
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
				req.Method = "GET";
				req.Timeout = 12000;
				req.ReadWriteTimeout = 12000;
				req.UserAgent = "ModMenuCrew/2026 (BepInEx)";
				req.Accept = "image/png, image/*;q=0.9";
				req.AllowAutoRedirect = true;
				req.MaximumAutomaticRedirections = 5;
				using (cancellationToken.Register(delegate
				{
					try
					{
						req.Abort();
					}
					catch
					{
					}
				}))
				{
					using HttpWebResponse resp = (HttpWebResponse)(await req.GetResponseAsync());
					if (resp.StatusCode == HttpStatusCode.OK)
					{
						using Stream stream = resp.GetResponseStream();
						using MemoryStream ms = new MemoryStream();
						await stream.CopyToAsync(ms, 81920, cancellationToken);
						byte[] array2 = ms.ToArray();
						if (array2.Length > 100)
						{
							return (data: array2, error: null);
						}
						item = $"WebRequest returned {array2.Length} bytes";
					}
					else
					{
						item = $"WebRequest HTTP {resp.StatusCode}";
					}
				}
			}
			catch (OperationCanceledException)
			{
				return (data: null, error: "cancelled");
			}
			catch (Exception ex5)
			{
				item = "WebRequest: " + ex5.GetBaseException().Message;
			}
			return (data: null, error: item);
		}

		[IteratorStateMachine(typeof(_003CLoadDiscordAvatarCoroutine_003Ed__238))]
		[HideFromIl2Cpp]
		private IEnumerator LoadDiscordAvatarCoroutine()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003CLoadDiscordAvatarCoroutine_003Ed__238(0)
			{
				_003C_003E4__this = this
			};
		}

		private void InitializeFeatureManagers()
		{
			teleportManager = new TeleportManager();
			cheatManager = new CheatManager();
			playerPickMenu = new PlayerPickMenu();
			banMenu = new BanMenu();
			hostControlsTab = new HostControlsTab();
			extrasTab = new ExtrasTab();
			animationsTab = new AnimationsTab();
			spoofingMenu = new SpoofingMenu();
			settingsTab = new SettingsTab();
			miscTab = new MiscTab();
		}

		private void InitializeMainWindowIMGUI()
		{
		}

		private void RenderDiscordCardWithAnimations()
		{
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0472: Unknown result type (might be due to invalid IL or missing references)
			//IL_0478: Invalid comparison between Unknown and I4
			//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Expected O, but got Unknown
			//IL_05c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0503: Unknown result type (might be due to invalid IL or missing references)
			//IL_0509: Invalid comparison between Unknown and I4
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0242: Invalid comparison between Unknown and I4
			//IL_0565: Unknown result type (might be due to invalid IL or missing references)
			//IL_0570: Unknown result type (might be due to invalid IL or missing references)
			//IL_0589: Unknown result type (might be due to invalid IL or missing references)
			//IL_0356: Unknown result type (might be due to invalid IL or missing references)
			//IL_0361: Unknown result type (might be due to invalid IL or missing references)
			//IL_0376: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_030c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0667: Unknown result type (might be due to invalid IL or missing references)
			//IL_040c: Unknown result type (might be due to invalid IL or missing references)
			//IL_043c: Unknown result type (might be due to invalid IL or missing references)
			//IL_045e: Unknown result type (might be due to invalid IL or missing references)
			if (!DiscordAuthManager.IsAvatarLoaded && !_avatarLazyLoadAttempted)
			{
				_avatarLazyLoadAttempted = true;
				Debug.Log(Object.op_Implicit("[ModMenuCrew] Avatar lazy-load triggered from dashboard render"));
				if (TryLoadAvatarFromCache())
				{
					_avatarLoadRequested = true;
					Debug.Log(Object.op_Implicit("[ModMenuCrew] Avatar lazy-loaded from cache OK"));
				}
				else if (!_avatarLoadRequested && DiscordAuthManager.HasDiscordProfile)
				{
					_avatarLoadRequested = true;
					((MonoBehaviour)this).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(LoadDiscordAvatarCoroutine()));
					Debug.Log(Object.op_Implicit("[ModMenuCrew] Avatar lazy-load: started CDN download"));
				}
			}
			if (DiscordAuthManager.IsAvatarLoaded && !_discordAnimPlayed)
			{
				_discordAnimStartTime = Time.time;
				_discordAnimPlayed = true;
			}
			float num = ((_discordAnimStartTime > 0f) ? (Time.time - _discordAnimStartTime) : 99f);
			float num2 = Mathf.Clamp01(num / 0.5f);
			float num3 = num2 - 1f;
			float num4 = num3 * num3;
			float num5 = num4 * num3;
			float num6 = ((num2 >= 1f) ? 1f : Mathf.Clamp(1f + 2.70158f * num5 + 1.70158f * num4, 0f, 1.08f));
			float num7 = Mathf.Clamp01(num2 * 1.3f);
			float num8 = 1f - num7;
			float num9 = 1f - num8 * num8 * num8;
			float num10 = Mathf.Clamp01((num - 0.2f) / 0.3f);
			float num11 = 1f - num10;
			float num12 = num11 * num11;
			float num13 = 1f - num12 * num12;
			GUILayout.BeginVertical(GuiStyles.DashboardCardStyle, Array.Empty<GUILayoutOption>());
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(80f) });
			GUILayout.Space(4f);
			Rect rect = GUILayoutUtility.GetRect(72f, 72f, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.Width(72f),
				GUILayout.Height(72f)
			});
			float num14 = (rect).x + 36f;
			float num15 = (rect).y + 36f;
			if ((Object)(object)DiscordAuthManager.AvatarTexture != (Object)null)
			{
				Texture2D avatarTexture = DiscordAuthManager.AvatarTexture;
				if (_cachedAvatarStyle == null || (Object)(object)_cachedAvatarTexture != (Object)(object)avatarTexture)
				{
					GUIStyle val = new GUIStyle();
					val.normal.background = avatarTexture;
					_cachedAvatarStyle = val;
					_cachedAvatarTexture = avatarTexture;
				}
				if ((int)Event.current.type == 7)
				{
					if (num2 >= 0.2f)
					{
						float num16 = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.5f);
						float num17 = Mathf.Clamp01((num2 - 0.2f) * 2f) * (0.3f + num16 * 0.4f);
						float num18 = 76f + num16 * 6f;
						GUI.color = new Color(GuiStyles.Theme.Blurple.r, GuiStyles.Theme.Blurple.g, GuiStyles.Theme.Blurple.b, num17);
						(_f2GlowRect).Set(num14 - num18 * 0.5f, num15 - num18 * 0.5f, num18, num18);
						GUI.Box(_f2GlowRect, "", GUI.skin.box);
						GUI.color = Color.white;
					}
					float num19 = 64f * num6;
					(_f2AvatarRect).Set(num14 - num19 * 0.5f, num15 - num19 * 0.5f, num19, num19);
					GUI.color = new Color(1f, 1f, 1f, num9);
					GUI.Box(_f2AvatarRect, GUIContent.none, _cachedAvatarStyle);
					GUI.color = Color.white;
					if (num2 >= 1f)
					{
						float num20 = Time.time * 1.5f;
						for (int i = 0; i < 2; i++)
						{
							float num21 = num20 + (float)i * (float)Math.PI;
							float num22 = 42f;
							float num23 = num14 + Mathf.Cos(num21) * num22;
							float num24 = num15 + Mathf.Sin(num21) * num22;
							float num25 = 0.4f + 0.4f * Mathf.Sin(Time.time * 3f + (float)i * 2f);
							GUI.color = new Color(0.6f, 0.7f, 1f, num25);
							(_f2SparkleRect).Set(num23 - 4f, num24 - 4f, 12f, 12f);
							GUI.Label(_f2SparkleRect, "✦", GuiStyles.LabelStyle);
						}
						GUI.color = Color.white;
					}
				}
			}
			else if ((int)Event.current.type == 7)
			{
				float num26 = 0.6f + 0.4f * Mathf.Sin(Time.time * 4f);
				GUI.color = new Color(GuiStyles.Theme.Blurple.r, GuiStyles.Theme.Blurple.g, GuiStyles.Theme.Blurple.b, num26);
				GUI.Box(rect, "", GUI.skin.box);
				GUI.color = Color.white;
				GUI.Label(rect, "<size=20>⟳</size>", GuiStyles.LabelStyle);
			}
			if (num2 >= 1f && (int)Event.current.type == 7)
			{
				float num27 = 0.92f + 0.08f * Mathf.Sin(Time.time * 2f);
				float num28 = 12f * num27;
				(_f2BadgeRect).Set((rect).xMax - num28 - 4f, (rect).yMax - num28 - 4f, num28, num28);
				GUI.color = GuiStyles.Theme.OnlineGreen;
				GUI.Box(_f2BadgeRect, "", GUI.skin.box);
				GUI.color = Color.white;
			}
			GUILayout.EndVertical();
			GUILayout.Space(12f);
			GUILayout.BeginVertical((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Space(8f);
			GUI.color = new Color(1f, 1f, 1f, num13);
			GUILayout.Label("<color=#5865F2><b>DISCORD</b></color>", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			string discordUsernameSafe = DiscordAuthManager.DiscordUsernameSafe;
			if (_cachedUsernameLabel == null || !Object.ReferenceEquals(Object.op_Implicit(_cachedUsernameSource), Object.op_Implicit(discordUsernameSafe)))
			{
				_cachedUsernameSource = discordUsernameSafe;
				_cachedUsernameLabel = "<size=16><b>" + discordUsernameSafe + "</b></size>";
			}
			GUILayout.Label(_cachedUsernameLabel, GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			if (num10 >= 1f)
			{
				if (DiscordAuthManager.IsLoggedIn)
				{
					GUILayout.Label("<color=#57F287>● Online</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
				else
				{
					GUILayout.Label("<color=#FAA61A>● Re-login</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
			}
			GUI.color = Color.white;
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			if (num10 >= 1f)
			{
				GUILayout.BeginVertical((Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Space(16f);
				if (GUILayout.Button(DiscordAuthManager.IsLoggedIn ? "Logout" : "Re-login", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(78f) }))
				{
					if (DiscordAuthManager.IsLoggedIn)
					{
						DiscordAuthManager.Logout();
						DiscordAuthManager.SetAvatarTexture(null);
						_avatarLoadRequested = false;
						ResetDiscordAnimation();
						PlayerPickMenu.TriggerRealtimeUpdate(force: true);
					}
					else if (!DiscordAuthManager.IsLoggingIn)
					{
						DiscordAuthManager.StartLoginAsync();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}

		private static string GetExpiryColor(string timeDisplay)
		{
			if (string.IsNullOrEmpty(timeDisplay))
			{
				return "B0B5BA";
			}
			string text = timeDisplay.ToLower();
			if (text.Contains("expired") || text.Contains("permanent"))
			{
				return "B0B5BA";
			}
			if (text.Contains("d ") || text.Contains("day"))
			{
				int i;
				for (i = 0; i < text.Length && !char.IsDigit(text[i]); i++)
				{
				}
				int j;
				for (j = i; j < text.Length && char.IsDigit(text[j]); j++)
				{
				}
				if (j > i && int.TryParse(text.Substring(i, j - i), out var result))
				{
					if (result > 3)
					{
						return "B0B5BA";
					}
					return "FFB020";
				}
				return "B0B5BA";
			}
			if (text.Contains("h ") || text.Contains("hour"))
			{
				return "FF3344";
			}
			if (text.Contains("m ") || text.Contains("min") || text.Contains("sec"))
			{
				return "FF3344";
			}
			return "B0B5BA";
		}

		private static string GetTimeGreeting()
		{
			int hour = DateTime.Now.Hour;
			if (hour < 6)
			{
				return "Good Night";
			}
			if (hour < 12)
			{
				return "Good Morning";
			}
			if (hour < 18)
			{
				return "Good Afternoon";
			}
			return "Good Evening";
		}

		private static string GetMapName(int mapId)
		{
			return mapId switch
			{
				0 => "The Skeld", 
				1 => "Mira HQ", 
				2 => "Polus", 
				3 => "Dleks", 
				4 => "Airship", 
				5 => "The Fungle", 
				_ => "Unknown", 
			};
		}

		private void DrawDashboardTab()
		{
			//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_072f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0734: Unknown result type (might be due to invalid IL or missing references)
			//IL_060e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0613: Unknown result type (might be due to invalid IL or missing references)
			//IL_08c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_08cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_07aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_07af: Unknown result type (might be due to invalid IL or missing references)
			//IL_090f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0914: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Invalid comparison between Unknown and I4
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Invalid comparison between Unknown and I4
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Invalid comparison between Unknown and I4
			if (!ServerData.IsTabEnabled("dashboard"))
			{
				return;
			}
			if (_dashSessionStart < 0f)
			{
				_dashSessionStart = Time.realtimeSinceStartup;
			}
			bool num = Time.frameCount - _dashCache_lastFrame > 30;
			if (num)
			{
				_dashCache_lastFrame = Time.frameCount;
			}
			string value = (DiscordAuthManager.HasDiscordProfile ? DiscordAuthManager.DiscordUsernameSafe : ModKeyValidator.ValidatedUsername);
			if (num)
			{
				_dashCache_greeting = $"<size=20><b>{GetTimeGreeting()}, {value}</b></size>";
			}
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label(_dashCache_greeting, GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.EndVertical();
			GUILayout.Space(8f);
			if (DiscordAuthManager.HasDiscordProfile)
			{
				RenderDiscordCardWithAnimations();
				GUILayout.Space(8f);
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			int value2 = 0;
			int value3 = 0;
			string value4 = "";
			string text = "";
			string text2 = "";
			string text3 = "None";
			try
			{
				AmongUsClient instance = AmongUsClient.Instance;
				if ((Object)(object)instance != (Object)null)
				{
					flag = ((InnerNetClient)instance).IsGameStarted;
					flag2 = !flag && (int)((InnerNetClient)instance).GameState == 1;
					flag3 = ((InnerNetClient)instance).AmHost;
					if (flag2 || flag)
					{
						text2 = GameCode.IntToGameName(((InnerNetClient)instance).GameId);
						value2 = PlayerControl.AllPlayerControls?.Count ?? 0;
						GameOptionsManager instance2 = GameOptionsManager.Instance;
						int? obj;
						if (instance2 == null)
						{
							obj = null;
						}
						else
						{
							IGameOptions currentGameOptions = instance2.CurrentGameOptions;
							obj = ((currentGameOptions != null) ? new int?(currentGameOptions.MaxPlayers) : null);
						}
						int? num2 = obj;
						value3 = num2.GetValueOrDefault(15);
						GameOptionsManager instance3 = GameOptionsManager.Instance;
						byte? obj2;
						if (instance3 == null)
						{
							obj2 = null;
						}
						else
						{
							IGameOptions currentGameOptions2 = instance3.CurrentGameOptions;
							obj2 = ((currentGameOptions2 != null) ? new byte?(currentGameOptions2.MapId) : null);
						}
						byte? b = obj2;
						value4 = GetMapName(b.GetValueOrDefault());
						GameOptionsManager instance4 = GameOptionsManager.Instance;
						GameModes? obj3;
						if (instance4 == null)
						{
							obj3 = null;
						}
						else
						{
							IGameOptions currentGameOptions3 = instance4.CurrentGameOptions;
							obj3 = ((currentGameOptions3 != null) ? new GameModes?(currentGameOptions3.GameMode) : null);
						}
						GameModes? val = obj3;
						GameModes valueOrDefault = val.GetValueOrDefault((GameModes)1);
						text = (((int)valueOrDefault == 2 || (int)valueOrDefault == 4) ? "Hide & Seek" : "Classic");
					}
					PlayerControl localPlayer = PlayerControl.LocalPlayer;
					object obj4;
					if (localPlayer == null)
					{
						obj4 = null;
					}
					else
					{
						NetworkedPlayerInfo data = localPlayer.Data;
						obj4 = ((data != null) ? data.Role : null);
					}
					if ((Object)obj4 != (Object)null)
					{
						text3 = GetEnglishRoleName(PlayerControl.LocalPlayer.Data.Role.Role);
					}
				}
			}
			catch
			{
				try
				{
					PlayerControl localPlayer2 = PlayerControl.LocalPlayer;
					object obj5;
					if (localPlayer2 == null)
					{
						obj5 = null;
					}
					else
					{
						NetworkedPlayerInfo data2 = localPlayer2.Data;
						obj5 = ((data2 != null) ? data2.Role : null);
					}
					if ((Object)obj5 != (Object)null)
					{
						text3 = PlayerControl.LocalPlayer.Data.Role.NiceName;
					}
				}
				catch
				{
				}
			}
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.BeginVertical(GuiStyles.DashboardCardStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
			Color color = GUI.color;
			GUI.color = GuiStyles.Theme.Accent;
			GUILayout.Label("[*] LICENSE", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color;
			GuiStyles.DrawSeparator();
			string text4 = ((!string.IsNullOrEmpty(ModKeyValidator.KeyDisplay)) ? ModKeyValidator.KeyDisplay : (ModKeyValidator.IsPremium ? "★ PREMIUM" : "FREE"));
			GUILayout.Label("<color=#FFD700><b>" + text4 + "</b></color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			if (!string.IsNullOrEmpty(ModKeyValidator.TimeDisplay))
			{
				string timeDisplay = ModKeyValidator.TimeDisplay;
				string expiryColor = GetExpiryColor(timeDisplay);
				float num3 = ((expiryColor == "FF3344") ? (0.75f + 0.25f * Mathf.Abs(Mathf.Sin(Time.time * 2.2f))) : 1f);
				Color color2 = GUI.color;
				GUI.color = new Color(1f, 1f, 1f, num3);
				GUILayout.Label($"<color=#{expiryColor}>{timeDisplay}</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color2;
			}
			else if (!ModKeyValidator.IsPremium)
			{
				GUILayout.Label("<color=#FFAA00>Session only</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Label("<color=#6B7280><b>KEY</b></color> <color=#4A4E54>••••-••••-••••</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Copy", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.Width(48f),
				GUILayout.Height(22f)
			}))
			{
				try
				{
					string currentKey = ModKeyValidator.CurrentKey;
					if (!string.IsNullOrEmpty(currentKey))
					{
						GUIUtility.systemCopyBuffer = currentKey;
						ShowNotification("Key copied (keep it private)");
					}
					else
					{
						ShowNotification("No key to copy");
					}
				}
				catch
				{
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.Space(8f);
			GUILayout.BeginVertical(GuiStyles.DashboardCardStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
			if (flag)
			{
				Color color3 = GUI.color;
				GUI.color = GuiStyles.Theme.Accent;
				GUILayout.Label("[*] IN GAME", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color3;
				GuiStyles.DrawSeparator();
				GUILayout.Label("<color=#00E664><b>● Playing</b></color>  <color=#6B7280>(" + text + ")</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Label($"<color=#E6EAEF>{value4}</color>  <color=#6B7280>·</color>  <color=#00D9FF>{value2}/{value3}</color>  <color=#6B7280>·</color>  {(flag3 ? "<color=#FFD700>HOST</color>" : "<color=#949EAD>Client</color>")}", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				if (!Object.ReferenceEquals(Object.op_Implicit(_dashCache_lastRoleName), Object.op_Implicit(text3)) || _dashCache_roleLine == null)
				{
					_dashCache_roleLine = "Role: <color=#E6EAEF><b>" + text3 + "</b></color>";
					_dashCache_lastRoleName = text3;
				}
				GUILayout.Label(_dashCache_roleLine, GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			else if (flag2)
			{
				Color color4 = GUI.color;
				GUI.color = GuiStyles.Theme.Accent;
				GUILayout.Label("[*] IN LOBBY", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color4;
				GuiStyles.DrawSeparator();
				GUILayout.Label("<color=#FFB020><b>● Waiting</b></color>  <color=#6B7280>(" + text + ")</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Label($"<color=#E6EAEF>{value4}</color>  <color=#6B7280>·</color>  <color=#00D9FF>{value2}/{value3}</color>  <color=#6B7280>·</color>  {(flag3 ? "<color=#FFD700>HOST</color>" : "<color=#949EAD>Client</color>")}", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Label("Code: <color=#FFD700><b>" + text2 + "</b></color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				if (GUILayout.Button("Copy", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(50f) }))
				{
					GUIUtility.systemCopyBuffer = text2;
					ShowNotification("Code copied!");
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				Color color5 = GUI.color;
				GUI.color = GuiStyles.Theme.Accent;
				GUILayout.Label("[*] STATUS", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color5;
				GuiStyles.DrawSeparator();
				GUILayout.Label("<color=#949EAD><b>● In Menu</b></color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Label("<color=#6B7280>Join or host a game to see details</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.Space(8f);
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.BeginVertical(GuiStyles.DashboardCardStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
			Color color6 = GUI.color;
			GUI.color = GuiStyles.Theme.Accent;
			GUILayout.Label("[*] SESSION", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color6;
			GuiStyles.DrawSeparator();
			int num4 = (int)((Time.realtimeSinceStartup - _dashSessionStart) / 60f);
			int num5 = num4 / 60;
			string text5 = ((num5 > 0) ? $"{num5}h {num4 % 60}m" : $"{num4}m");
			GUILayout.Label("Uptime: <color=#E6EAEF><b>" + text5 + "</b></color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			bool flag4 = false;
			try
			{
				flag4 = RealtimeConnection.IsConnected;
			}
			catch
			{
			}
			GUILayout.Label(flag4 ? "<color=#00E664>● WebSocket Connected</color>" : "<color=#FF3344>○ WebSocket Offline</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.EndVertical();
			GUILayout.Space(8f);
			GUILayout.BeginVertical(GuiStyles.DashboardCardStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
			Color color7 = GUI.color;
			GUI.color = GuiStyles.Theme.Accent;
			GUILayout.Label("[*] ABOUT", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color7;
			GuiStyles.DrawSeparator();
			GUILayout.Label("<color=#E6EAEF><b>v6.1.4b</b></color>  <color=#6B7280>·</color>  <color=#949EAD>Among Us Mod Menu • 2026</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			ConfigEntry<KeyCode> menuToggleKey = CheatConfig.MenuToggleKey;
			KeyCode val2 = (KeyCode)((menuToggleKey == null) ? 282 : ((int)menuToggleKey.Value));
			string text6 = ((object)(KeyCode)val2).ToString();
			GUILayout.Label("<color=#949EAD>crewcore.online  ·  Press <color=#FFD700><b>" + text6 + "</b></color> to toggle</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private string GetEnglishRoleName(RoleTypes role)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected I4, but got Unknown
			return (int)role switch
			{
				1 => "Impostor", 
				5 => "Shapeshifter", 
				9 => "Phantom", 
				18 => "Viper", 
				3 => "Engineer", 
				2 => "Scientist", 
				4 => "Guardian Angel", 
				8 => "Noisemaker", 
				10 => "Tracker", 
				0 => "Crewmate", 
				_ => role.ToString(), 
			};
		}

		private string GetTooltipForTab(string id)
		{
			return id switch
			{
				"dashboard" => "Home & Status", 
				"ban_menu" => "Ban management and lobby settings", 
				"lobbies" => "Find and join game lobbies", 
				"spoofing" => "Level, Platform, Friend Code spoofing", 
				"settings" => "Configuration and preferences", 
				_ => "", 
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool ValidateRenderToken(long token)
		{
			return GhostUI.CheckToken(token);
		}

		private void InitializeActionRegistry()
		{
			_tabDrawRegistry = new Dictionary<string, Action<long>>
			{
				{
					"dashboard",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							DrawDashboardTab();
						}
					}
				},
				{
					"ban_menu",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							banMenu?.Draw();
						}
					}
				},
				{
					"host_controls",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							hostControlsTab?.Draw();
						}
					}
				},
				{
					"extras",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							extrasTab?.Draw();
						}
					}
				},
				{
					"animations",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							animationsTab?.Draw();
						}
					}
				},
				{
					"spoofing",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							spoofingMenu?.Draw();
						}
					}
				},
				{
					"game",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							DrawGameTabIMGUI();
						}
					}
				},
				{
					"movement",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							DrawMovementTabIMGUI();
						}
					}
				},
				{
					"sabotage",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							DrawSabotageTabIMGUI();
						}
					}
				},
				{
					"impostor",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							DrawImpostorTabIMGUI();
						}
					}
				},
				{
					"lobbies",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							DrawLobbyListingTabIMGUI();
						}
					}
				},
				{
					"settings",
					delegate(long t)
					{
						if (ValidateRenderToken(t))
						{
							settingsTab?.DrawSettingsTab();
						}
					}
				}
			};
			if (cheatManager != null)
			{
				_tabDrawRegistry["cheats"] = delegate(long t)
				{
					if (ValidateRenderToken(t))
					{
						cheatManager.DrawCheatsTab();
					}
				};
			}
			if (playerPickMenu != null)
			{
				_tabDrawRegistry["playerpick"] = delegate(long t)
				{
					if (ValidateRenderToken(t))
					{
						playerPickMenu.Draw();
					}
				};
			}
			_tabDrawRegistry["replay"] = delegate(long t)
			{
				if (ValidateRenderToken(t))
				{
					ReplayUI.DrawTabContent();
				}
			};
			if (miscTab == null)
			{
				return;
			}
			_tabDrawRegistry["misc"] = delegate(long t)
			{
				if (ValidateRenderToken(t))
				{
					miscTab.Draw();
				}
			};
		}

		private void InitializeTabsForGameIMGUI()
		{
			IconLoader.PreloadAll();
			if (_tabDrawRegistry == null)
			{
				InitializeActionRegistry();
			}
		}

		private void DrawTeleportTabFromServer()
		{
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("TELEPORT", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GuiStyles.DrawSeparator();
			TabDefinition tabDefinition = ServerData.Tabs?.FirstOrDefault((TabDefinition t) => t.Id == "teleport");
			if (tabDefinition != null)
			{
				foreach (TeleportLocation location in tabDefinition.Locations)
				{
					if (GUILayout.Button(location.Name, GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
					{
						PlayerControl localPlayer = PlayerControl.LocalPlayer;
						if ((Object)(object)localPlayer != (Object)null)
						{
							localPlayer.NetTransform.RpcSnapTo(new Vector2(location.X, location.Y));
						}
					}
				}
			}
			GUILayout.EndVertical();
		}

		private void DrawSettingsTabFromServer()
		{
			if (!ServerData.IsTabEnabled("settings"))
			{
				return;
			}
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("SETTINGS", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GuiStyles.DrawSeparator();
			TabDefinition tabDefinition = ServerData.Tabs?.FirstOrDefault((TabDefinition t) => t.Id == "settings");
			if (tabDefinition != null)
			{
				foreach (SectionDefinition section in tabDefinition.Sections)
				{
					foreach (SliderDefinition slider in section.Sliders)
					{
						float sliderValue = ServerData.GetSliderValue(slider.Id);
						GUILayout.Label($"{slider.Label}: {sliderValue:F1}", (Il2CppReferenceArray<GUILayoutOption>)null);
						sliderValue = GUILayout.HorizontalSlider(sliderValue, slider.Min, slider.Max, Array.Empty<GUILayoutOption>());
						ServerData.SetSliderValue(slider.Id, sliderValue);
					}
				}
			}
			GUILayout.EndVertical();
		}

		private void DrawServerDefinedTab(string tabId)
		{
			if (!ServerData.IsTabEnabled(tabId))
			{
				return;
			}
			TabDefinition tabDefinition = ServerData.Tabs?.FirstOrDefault((TabDefinition t) => t.Id == tabId);
			if (tabDefinition == null)
			{
				return;
			}
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label(tabDefinition.Name.ToUpper(), GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GuiStyles.DrawSeparator();
			foreach (SectionDefinition section in tabDefinition.Sections)
			{
				if (!string.IsNullOrEmpty(section.VisibleWhen) && section.VisibleWhen == "is_host" && ((Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmHost))
				{
					continue;
				}
				if (!string.IsNullOrEmpty(section.Name))
				{
					GUILayout.Label(section.Name, GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
				int num = 0;
				foreach (ButtonDefinition button in section.Buttons)
				{
					if (num % 2 == 0)
					{
						GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					}
					if (button.Type == "toggle")
					{
						bool toggleState = ServerData.GetToggleState(button.Id);
						bool flag = GUILayout.Toggle(toggleState, button.Label, GuiStyles.ToggleStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						if (flag != toggleState && button.Enabled)
						{
							ServerData.SetToggleState(button.Id, flag);
						}
					}
					else if (GUILayout.Button(button.Label, GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()) && button.Enabled)
					{
						ExecuteServerAction(button.Id);
					}
					num++;
					if (num % 2 == 0)
					{
						GUILayout.EndHorizontal();
					}
				}
				if (num % 2 != 0)
				{
					GUILayout.EndHorizontal();
				}
				foreach (SliderDefinition slider in section.Sliders)
				{
					float sliderValue = ServerData.GetSliderValue(slider.Id);
					GUILayout.Label($"{slider.Label}: {sliderValue:F1}", (Il2CppReferenceArray<GUILayoutOption>)null);
					sliderValue = GUILayout.HorizontalSlider(sliderValue, slider.Min, slider.Max, Array.Empty<GUILayoutOption>());
					ServerData.SetSliderValue(slider.Id, sliderValue);
				}
			}
			GUILayout.EndVertical();
		}

		private void ExecuteServerAction(string actionId)
		{
			switch (actionId)
			{
			case "auto_tasks":
				if ((Object)(object)DestroyableSingleton<HudManager>.Instance != (Object)null)
				{
					((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(GameCheats.CompleteAllTasksWithDelay(0.2f)));
				}
				else
				{
					GameCheats.CompleteAllTasks();
				}
				ShowNotification("Tasks completing...");
				break;
			case "skip_meeting":
				GameCheats.CloseMeeting();
				ShowNotification("Meeting closed!");
				break;
			case "reveal_sus":
				GameCheats.RevealImpostors();
				ShowNotification("Impostors revealed!");
				break;
			case "bypass_scanner":
			{
				HudManager instance = DestroyableSingleton<HudManager>.Instance;
				if (instance != null)
				{
					((MonoBehaviour)instance).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(GameCheats.BypassScannerWithTimeout(12f)));
				}
				ShowNotification("Scanner bypassed!");
				break;
			}
			case "godmode":
				CheatConfig.GodMode = !CheatConfig.GodMode;
				break;
			case "kill_all":
				GameCheats.KillAll();
				break;
			case "kill_crew":
				GameCheats.KillAll(crewOnly: true);
				break;
			case "kill_imps":
				GameCheats.KillAll(crewOnly: false, impostorsOnly: true);
				break;
			case "crew_vent":
				if (CheatConfig.AllowVenting != null)
				{
					CheatConfig.AllowVenting.Value = !CheatConfig.AllowVenting.Value;
				}
				break;
			case "click_tp":
				if (CheatConfig.TeleportWithCursor != null)
				{
					CheatConfig.TeleportWithCursor.Value = !CheatConfig.TeleportWithCursor.Value;
				}
				break;
			case "drone_cam":
				if (CheatConfig.FreeCamEnabled != null)
				{
					CheatConfig.FreeCamEnabled.Value = !CheatConfig.FreeCamEnabled.Value;
				}
				break;
			case "ghost_walk":
				if (CheatConfig.NoClipSmoothEnabled != null)
				{
					CheatConfig.NoClipSmoothEnabled.Value = !CheatConfig.NoClipSmoothEnabled.Value;
				}
				break;
			case "radar":
				if (CheatConfig.RadarEnabled != null)
				{
					CheatConfig.RadarEnabled.Value = !CheatConfig.RadarEnabled.Value;
				}
				break;
			case "tracers":
				if (CheatConfig.TracersEnabled != null)
				{
					CheatConfig.TracersEnabled.Value = !CheatConfig.TracersEnabled.Value;
				}
				break;
			case "see_ghosts":
				if (CheatConfig.SeeGhosts != null)
				{
					CheatConfig.SeeGhosts.Value = !CheatConfig.SeeGhosts.Value;
				}
				break;
			case "dead_chat":
				if (CheatConfig.SeeDeadChat != null)
				{
					CheatConfig.SeeDeadChat.Value = !CheatConfig.SeeDeadChat.Value;
				}
				break;
			case "kill_timer":
				if (CheatConfig.ShowKillCooldowns != null)
				{
					CheatConfig.ShowKillCooldowns.Value = !CheatConfig.ShowKillCooldowns.Value;
				}
				break;
			case "no_shadows":
				if (CheatConfig.NoShadows != null)
				{
					CheatConfig.NoShadows.Value = !CheatConfig.NoShadows.Value;
				}
				break;
			default:
				ShowNotification("Action: " + actionId);
				break;
			}
		}

		private void DrawReplayTabIMGUI()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			Color color = GUI.color;
			GUI.color = GuiStyles.Theme.Accent;
			GUILayout.Label("[*] REPLAY SYSTEM", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color;
			GuiStyles.DrawSeparator();
			ReplayRecorder instance = ReplayRecorder.Instance;
			ReplayPlayer instance2 = ReplayPlayer.Instance;
			if ((Object)(object)instance2 != (Object)null && instance2.IsPlaying)
			{
				Color color2 = GUI.color;
				GUI.color = GuiStyles.Theme.Success;
				GUILayout.Label("▶ PLAYING REPLAY", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color2;
				GUILayout.Label($"Time: {instance2.CurrentTime:F1}s / {instance2.TotalDuration:F1}s", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				float num = GUILayout.HorizontalSlider(instance2.CurrentTime, 0f, instance2.TotalDuration, Array.Empty<GUILayoutOption>());
				if (Math.Abs(num - instance2.CurrentTime) > 1f)
				{
					instance2.Seek(num);
				}
				GUILayout.Space(4f);
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				if (GUILayout.Button("0.5x", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
				{
					instance2.SetSpeed(0.5f);
				}
				if (GUILayout.Button("1x", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
				{
					instance2.SetSpeed(1f);
				}
				if (GUILayout.Button("2x", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
				{
					instance2.SetSpeed(2f);
				}
				if (GUILayout.Button("4x", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
				{
					instance2.SetSpeed(4f);
				}
				if (GUILayout.Button("10x", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
				{
					instance2.SetSpeed(10f);
				}
				GUILayout.EndHorizontal();
				if (GUILayout.Button("STOP PLAYBACK", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
				{
					instance2.Cleanup();
				}
			}
			else if ((Object)(object)instance != (Object)null)
			{
				if (instance.IsRecording)
				{
					GUILayout.Label("<color=#FF3344>● RECORDING...</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUILayout.Label($"Captured: {instance.FrameCount} frames", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					if (GUILayout.Button("STOP RECORDING", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
					{
						instance.StopRecording();
					}
				}
				else
				{
					if (Object.op_Implicit((Object)(object)AmongUsClient.Instance) && ((InnerNetClient)AmongUsClient.Instance).IsGameStarted)
					{
						GUILayout.Label("RECORDER READY", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						if (GUILayout.Button("START RECORDING", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
						{
							instance.StartRecording();
						}
					}
					else
					{
						GUILayout.Label("Recording available in-game only.", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					}
					GUILayout.Space(8f);
					GuiStyles.DrawSeparator();
					GUILayout.Label("SAVED REPLAYS:", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					string text = Path.Combine(Directory.GetCurrentDirectory(), "Replays");
					if (!Directory.Exists(text))
					{
						Directory.CreateDirectory(text);
					}
					if (GUILayout.Button("OPEN REPLAY FOLDER", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
					{
						Application.OpenURL(text);
					}
					if (GUILayout.Button("REFRESH LIST", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
					{
						_lastReplayFileScanTime = -999f;
					}
					if (_cachedReplayFiles == null || Time.realtimeSinceStartup - _lastReplayFileScanTime > 3f)
					{
						_lastReplayFileScanTime = Time.realtimeSinceStartup;
						try
						{
							_cachedReplayFiles = Directory.GetFiles(text, "*.mmc");
						}
						catch
						{
							_cachedReplayFiles = Array.Empty<string>();
						}
					}
					_replayScroll = GUILayout.BeginScrollView(_replayScroll, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(200f) });
					string[] obj2 = _cachedReplayFiles ?? Array.Empty<string>();
					if (obj2.Length == 0)
					{
						GUILayout.Label("No replays found. Play a match to record!", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					}
					string[] array = obj2;
					foreach (string path in array)
					{
						string? fileName = Path.GetFileName(path);
						GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.Label(fileName, GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						if (GUILayout.Button("▶ LOAD", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(60f) }))
						{
							instance2.LoadAndPlay(path);
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndScrollView();
				}
			}
			else
			{
				GUILayout.Label("Replay System Unavailable.", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			GUILayout.Space(8f);
			GUILayout.EndVertical();
		}

		private Texture2D CreateDiscordIcon()
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)_discordIconTexture != (Object)null)
				{
					return _discordIconTexture;
				}
				_discordIconTexture = new Texture2D(16, 16);
				Color blurple = GuiStyles.Theme.Blurple;
				Color white = Color.white;
				for (int i = 0; i < 16; i++)
				{
					for (int j = 0; j < 16; j++)
					{
						_discordIconTexture.SetPixel(j, i, blurple);
					}
				}
				for (int k = 5; k <= 10; k++)
				{
					for (int l = 2; l <= 6; l++)
					{
						_discordIconTexture.SetPixel(l, k, white);
					}
				}
				for (int m = 5; m <= 10; m++)
				{
					for (int n = 9; n <= 13; n++)
					{
						_discordIconTexture.SetPixel(n, m, white);
					}
				}
				_discordIconTexture.Apply();
				return _discordIconTexture;
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error creating Discord icon: {value}"));
				return null;
			}
		}

		private TMP_FontAsset LoadGameFont(string primaryName = null, string fallbackName = null)
		{
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Expected O, but got Unknown
			if ((Object)(object)_cachedFont != (Object)null)
			{
				return _cachedFont;
			}
			try
			{
				Il2CppReferenceArray<Object> val = Resources.FindObjectsOfTypeAll(Il2CppType.Of<TMP_FontAsset>());
				if (val != null && ((Il2CppArrayBase<Object>)(object)val).Length > 0)
				{
					TMP_FontAsset val2 = null;
					if (!string.IsNullOrWhiteSpace(primaryName))
					{
						foreach (Object item in (Il2CppArrayBase<Object>)(object)val)
						{
							TMP_FontAsset val3 = ((item != (Object)null) ? ((Il2CppObjectBase)item).TryCast<TMP_FontAsset>() : null);
							if ((Object)(object)val3 != (Object)null && ((Object)val3).name.IndexOf(primaryName, StringComparison.OrdinalIgnoreCase) >= 0)
							{
								val2 = val3;
								break;
							}
						}
						if ((Object)(object)val2 != (Object)null)
						{
							return _cachedFont = val2;
						}
					}
					if (!string.IsNullOrWhiteSpace(fallbackName))
					{
						foreach (Object item2 in (Il2CppArrayBase<Object>)(object)val)
						{
							TMP_FontAsset val4 = ((item2 != (Object)null) ? ((Il2CppObjectBase)item2).TryCast<TMP_FontAsset>() : null);
							if ((Object)(object)val4 != (Object)null && ((Object)val4).name.IndexOf(fallbackName, StringComparison.OrdinalIgnoreCase) >= 0)
							{
								val2 = val4;
								break;
							}
						}
						if ((Object)(object)val2 != (Object)null)
						{
							return _cachedFont = val2;
						}
					}
					foreach (Object item3 in (Il2CppArrayBase<Object>)(object)val)
					{
						TMP_FontAsset val5 = ((item3 != (Object)null) ? ((Il2CppObjectBase)item3).TryCast<TMP_FontAsset>() : null);
						if ((Object)(object)val5 != (Object)null)
						{
							return _cachedFont = val5;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ModMenuCrewPlugin instance = Instance;
				if (instance != null)
				{
					ManualLogSource log = ((BasePlugin)instance).Log;
					if (log != null)
					{
						bool flag = default(bool);
						BepInExWarningLogInterpolatedStringHandler val6 = new BepInExWarningLogInterpolatedStringHandler(30, 1, ref flag);
						if (flag)
						{
							((BepInExLogInterpolatedStringHandler)val6).AppendLiteral("[UI] Could not load TMP font: ");
							((BepInExLogInterpolatedStringHandler)val6).AppendFormatted<string>(ex.Message);
						}
						log.LogWarning(val6);
					}
				}
			}
			return null;
		}

		private void SetupActivationUI_TMP(bool forceRebuild = false)
		{
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Expected O, but got Unknown
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Expected O, but got Unknown
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_0270: Expected O, but got Unknown
			//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0304: Unknown result type (might be due to invalid IL or missing references)
			//IL_031a: Unknown result type (might be due to invalid IL or missing references)
			//IL_032e: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0364: Unknown result type (might be due to invalid IL or missing references)
			//IL_0379: Unknown result type (might be due to invalid IL or missing references)
			//IL_038e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0399: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Expected O, but got Unknown
			//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0402: Unknown result type (might be due to invalid IL or missing references)
			//IL_0425: Unknown result type (might be due to invalid IL or missing references)
			//IL_0445: Unknown result type (might be due to invalid IL or missing references)
			//IL_045a: Unknown result type (might be due to invalid IL or missing references)
			//IL_046f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0484: Unknown result type (might be due to invalid IL or missing references)
			//IL_0498: Unknown result type (might be due to invalid IL or missing references)
			//IL_0558: Unknown result type (might be due to invalid IL or missing references)
			//IL_055f: Expected O, but got Unknown
			//IL_0580: Unknown result type (might be due to invalid IL or missing references)
			//IL_0599: Unknown result type (might be due to invalid IL or missing references)
			//IL_059e: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_05db: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0605: Unknown result type (might be due to invalid IL or missing references)
			//IL_0619: Unknown result type (might be due to invalid IL or missing references)
			//IL_0628: Unknown result type (might be due to invalid IL or missing references)
			//IL_062f: Expected O, but got Unknown
			//IL_0650: Unknown result type (might be due to invalid IL or missing references)
			//IL_0669: Unknown result type (might be due to invalid IL or missing references)
			//IL_066e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0696: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_06e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0703: Unknown result type (might be due to invalid IL or missing references)
			//IL_070a: Expected O, but got Unknown
			//IL_072d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0743: Unknown result type (might be due to invalid IL or missing references)
			//IL_0759: Unknown result type (might be due to invalid IL or missing references)
			//IL_076f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0785: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_050b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0520: Unknown result type (might be due to invalid IL or missing references)
			//IL_0535: Unknown result type (might be due to invalid IL or missing references)
			//IL_0549: Unknown result type (might be due to invalid IL or missing references)
			//IL_084b: Unknown result type (might be due to invalid IL or missing references)
			//IL_085a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0869: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_07c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_07e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_07f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_080e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0823: Unknown result type (might be due to invalid IL or missing references)
			//IL_0837: Unknown result type (might be due to invalid IL or missing references)
			//IL_08af: Unknown result type (might be due to invalid IL or missing references)
			//IL_08be: Unknown result type (might be due to invalid IL or missing references)
			//IL_08cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0929: Unknown result type (might be due to invalid IL or missing references)
			//IL_0938: Unknown result type (might be due to invalid IL or missing references)
			//IL_0947: Unknown result type (might be due to invalid IL or missing references)
			//IL_097a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0989: Unknown result type (might be due to invalid IL or missing references)
			//IL_09e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_09fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_09bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a2b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a30: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a43: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a5e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a7e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a93: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aa8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0abd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ad1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b11: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b20: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b7f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b93: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b54: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bd6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bdb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c09: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c29: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c3e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c53: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c68: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cad: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cb2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cb7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cc2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cc9: Expected O, but got Unknown
			//IL_0cf4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d0e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d28: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d42: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d5c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0deb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0df2: Expected O, but got Unknown
			//IL_0e2c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e54: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e68: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e80: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e8f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e9e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0eca: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ed1: Expected O, but got Unknown
			//IL_0f0b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f33: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f47: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f5f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f6e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f7d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c8c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fb1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fb6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fc3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fde: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ffe: Unknown result type (might be due to invalid IL or missing references)
			//IL_1013: Unknown result type (might be due to invalid IL or missing references)
			//IL_1028: Unknown result type (might be due to invalid IL or missing references)
			//IL_103d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1051: Unknown result type (might be due to invalid IL or missing references)
			//IL_1060: Unknown result type (might be due to invalid IL or missing references)
			//IL_1067: Expected O, but got Unknown
			//IL_1091: Unknown result type (might be due to invalid IL or missing references)
			//IL_10cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_10e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_110a: Unknown result type (might be due to invalid IL or missing references)
			//IL_1124: Unknown result type (might be due to invalid IL or missing references)
			//IL_113e: Unknown result type (might be due to invalid IL or missing references)
			//IL_1158: Unknown result type (might be due to invalid IL or missing references)
			//IL_1172: Unknown result type (might be due to invalid IL or missing references)
			//IL_118a: Unknown result type (might be due to invalid IL or missing references)
			//IL_1199: Unknown result type (might be due to invalid IL or missing references)
			//IL_11a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_11e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_11f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_1203: Unknown result type (might be due to invalid IL or missing references)
			//IL_1297: Unknown result type (might be due to invalid IL or missing references)
			//IL_12b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_12ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_1301: Unknown result type (might be due to invalid IL or missing references)
			//IL_1310: Unknown result type (might be due to invalid IL or missing references)
			//IL_135b: Unknown result type (might be due to invalid IL or missing references)
			//IL_1393: Unknown result type (might be due to invalid IL or missing references)
			//IL_1398: Unknown result type (might be due to invalid IL or missing references)
			//IL_13aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_13c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_13dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_13fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_1410: Unknown result type (might be due to invalid IL or missing references)
			//IL_141f: Unknown result type (might be due to invalid IL or missing references)
			//IL_1424: Unknown result type (might be due to invalid IL or missing references)
			//IL_1432: Unknown result type (might be due to invalid IL or missing references)
			//IL_144d: Unknown result type (might be due to invalid IL or missing references)
			//IL_146d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1482: Unknown result type (might be due to invalid IL or missing references)
			//IL_1497: Unknown result type (might be due to invalid IL or missing references)
			//IL_14ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_14d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_14de: Expected O, but got Unknown
			//IL_1505: Unknown result type (might be due to invalid IL or missing references)
			//IL_152b: Unknown result type (might be due to invalid IL or missing references)
			//IL_1540: Unknown result type (might be due to invalid IL or missing references)
			//IL_1555: Unknown result type (might be due to invalid IL or missing references)
			//IL_156a: Unknown result type (might be due to invalid IL or missing references)
			//IL_157e: Unknown result type (might be due to invalid IL or missing references)
			//IL_15d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_15e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_15f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_1662: Unknown result type (might be due to invalid IL or missing references)
			//IL_1671: Unknown result type (might be due to invalid IL or missing references)
			//IL_1680: Unknown result type (might be due to invalid IL or missing references)
			//IL_1699: Unknown result type (might be due to invalid IL or missing references)
			//IL_16a0: Expected O, but got Unknown
			//IL_16cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_16f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_1705: Unknown result type (might be due to invalid IL or missing references)
			//IL_17e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_17f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_1807: Unknown result type (might be due to invalid IL or missing references)
			//IL_172d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1732: Unknown result type (might be due to invalid IL or missing references)
			//IL_1745: Unknown result type (might be due to invalid IL or missing references)
			//IL_1768: Unknown result type (might be due to invalid IL or missing references)
			//IL_1788: Unknown result type (might be due to invalid IL or missing references)
			//IL_179d: Unknown result type (might be due to invalid IL or missing references)
			//IL_17b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_17c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_17d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_18da: Unknown result type (might be due to invalid IL or missing references)
			//IL_18e1: Expected O, but got Unknown
			//IL_1905: Unknown result type (might be due to invalid IL or missing references)
			//IL_191b: Unknown result type (might be due to invalid IL or missing references)
			//IL_1931: Unknown result type (might be due to invalid IL or missing references)
			//IL_1947: Unknown result type (might be due to invalid IL or missing references)
			//IL_195d: Unknown result type (might be due to invalid IL or missing references)
			//IL_196c: Unknown result type (might be due to invalid IL or missing references)
			//IL_1971: Unknown result type (might be due to invalid IL or missing references)
			//IL_1984: Unknown result type (might be due to invalid IL or missing references)
			//IL_19b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_19d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_19e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_19fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_1a08: Unknown result type (might be due to invalid IL or missing references)
			//IL_1a1c: Unknown result type (might be due to invalid IL or missing references)
			//IL_1a33: Unknown result type (might be due to invalid IL or missing references)
			//IL_1a3a: Expected O, but got Unknown
			//IL_1a61: Unknown result type (might be due to invalid IL or missing references)
			//IL_1a89: Unknown result type (might be due to invalid IL or missing references)
			//IL_1a9e: Unknown result type (might be due to invalid IL or missing references)
			//IL_1ab3: Unknown result type (might be due to invalid IL or missing references)
			//IL_1abe: Unknown result type (might be due to invalid IL or missing references)
			//IL_1ad2: Unknown result type (might be due to invalid IL or missing references)
			//IL_1b07: Unknown result type (might be due to invalid IL or missing references)
			//IL_1b1b: Unknown result type (might be due to invalid IL or missing references)
			//IL_1b5d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1b6c: Unknown result type (might be due to invalid IL or missing references)
			//IL_1835: Unknown result type (might be due to invalid IL or missing references)
			//IL_183a: Unknown result type (might be due to invalid IL or missing references)
			//IL_184c: Unknown result type (might be due to invalid IL or missing references)
			//IL_1870: Unknown result type (might be due to invalid IL or missing references)
			//IL_1885: Unknown result type (might be due to invalid IL or missing references)
			//IL_189a: Unknown result type (might be due to invalid IL or missing references)
			//IL_18af: Unknown result type (might be due to invalid IL or missing references)
			//IL_18c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_1bf6: Unknown result type (might be due to invalid IL or missing references)
			//IL_1bfb: Unknown result type (might be due to invalid IL or missing references)
			//IL_1c08: Unknown result type (might be due to invalid IL or missing references)
			//IL_1c23: Unknown result type (might be due to invalid IL or missing references)
			//IL_1c43: Unknown result type (might be due to invalid IL or missing references)
			//IL_1c58: Unknown result type (might be due to invalid IL or missing references)
			//IL_1c6d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1c82: Unknown result type (might be due to invalid IL or missing references)
			//IL_1c96: Unknown result type (might be due to invalid IL or missing references)
			//IL_1ca5: Unknown result type (might be due to invalid IL or missing references)
			//IL_1cac: Expected O, but got Unknown
			//IL_1cd6: Unknown result type (might be due to invalid IL or missing references)
			//IL_1d14: Unknown result type (might be due to invalid IL or missing references)
			//IL_1d28: Unknown result type (might be due to invalid IL or missing references)
			//IL_1d47: Unknown result type (might be due to invalid IL or missing references)
			//IL_1d5d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1d73: Unknown result type (might be due to invalid IL or missing references)
			//IL_1d89: Unknown result type (might be due to invalid IL or missing references)
			//IL_1d9f: Unknown result type (might be due to invalid IL or missing references)
			//IL_1bd6: Unknown result type (might be due to invalid IL or missing references)
			//IL_1e38: Unknown result type (might be due to invalid IL or missing references)
			//IL_1e47: Unknown result type (might be due to invalid IL or missing references)
			//IL_1e56: Unknown result type (might be due to invalid IL or missing references)
			//IL_1dcc: Unknown result type (might be due to invalid IL or missing references)
			//IL_1dd1: Unknown result type (might be due to invalid IL or missing references)
			//IL_1ddf: Unknown result type (might be due to invalid IL or missing references)
			//IL_1def: Unknown result type (might be due to invalid IL or missing references)
			//IL_1e0f: Unknown result type (might be due to invalid IL or missing references)
			//IL_1e23: Unknown result type (might be due to invalid IL or missing references)
			//IL_1e8a: Unknown result type (might be due to invalid IL or missing references)
			//IL_1e8f: Unknown result type (might be due to invalid IL or missing references)
			//IL_1e9d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1eca: Unknown result type (might be due to invalid IL or missing references)
			//IL_1eda: Unknown result type (might be due to invalid IL or missing references)
			//IL_1f07: Unknown result type (might be due to invalid IL or missing references)
			//IL_1f1b: Unknown result type (might be due to invalid IL or missing references)
			//IL_1f38: Unknown result type (might be due to invalid IL or missing references)
			//IL_1f4e: Unknown result type (might be due to invalid IL or missing references)
			//IL_1f64: Unknown result type (might be due to invalid IL or missing references)
			//IL_1f69: Unknown result type (might be due to invalid IL or missing references)
			//IL_1f78: Unknown result type (might be due to invalid IL or missing references)
			//IL_1ff7: Unknown result type (might be due to invalid IL or missing references)
			//IL_1ffe: Expected O, but got Unknown
			//IL_202c: Unknown result type (might be due to invalid IL or missing references)
			//IL_20a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_20b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_20d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_20ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_2100: Unknown result type (might be due to invalid IL or missing references)
			//IL_211a: Unknown result type (might be due to invalid IL or missing references)
			//IL_2130: Unknown result type (might be due to invalid IL or missing references)
			//IL_249e: Unknown result type (might be due to invalid IL or missing references)
			//IL_24ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_24bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_21c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_21d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_21e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_2155: Unknown result type (might be due to invalid IL or missing references)
			//IL_215a: Unknown result type (might be due to invalid IL or missing references)
			//IL_2168: Unknown result type (might be due to invalid IL or missing references)
			//IL_217e: Unknown result type (might be due to invalid IL or missing references)
			//IL_219e: Unknown result type (might be due to invalid IL or missing references)
			//IL_21b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_2506: Unknown result type (might be due to invalid IL or missing references)
			//IL_2515: Unknown result type (might be due to invalid IL or missing references)
			//IL_2211: Unknown result type (might be due to invalid IL or missing references)
			//IL_2220: Unknown result type (might be due to invalid IL or missing references)
			//IL_222f: Unknown result type (might be due to invalid IL or missing references)
			//IL_225e: Unknown result type (might be due to invalid IL or missing references)
			//IL_226d: Unknown result type (might be due to invalid IL or missing references)
			//IL_227c: Unknown result type (might be due to invalid IL or missing references)
			//IL_2292: Unknown result type (might be due to invalid IL or missing references)
			//IL_2299: Expected O, but got Unknown
			//IL_22c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_2311: Unknown result type (might be due to invalid IL or missing references)
			//IL_2325: Unknown result type (might be due to invalid IL or missing references)
			//IL_2371: Unknown result type (might be due to invalid IL or missing references)
			//IL_2385: Unknown result type (might be due to invalid IL or missing references)
			//IL_2582: Unknown result type (might be due to invalid IL or missing references)
			//IL_2596: Unknown result type (might be due to invalid IL or missing references)
			//IL_2546: Unknown result type (might be due to invalid IL or missing references)
			//IL_23ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_23b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_23c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_23d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_23f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_2409: Unknown result type (might be due to invalid IL or missing references)
			//IL_241e: Unknown result type (might be due to invalid IL or missing references)
			//IL_2433: Unknown result type (might be due to invalid IL or missing references)
			//IL_243d: Unknown result type (might be due to invalid IL or missing references)
			//IL_25df: Unknown result type (might be due to invalid IL or missing references)
			//IL_25e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_25f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_260c: Unknown result type (might be due to invalid IL or missing references)
			//IL_262c: Unknown result type (might be due to invalid IL or missing references)
			//IL_2641: Unknown result type (might be due to invalid IL or missing references)
			//IL_2656: Unknown result type (might be due to invalid IL or missing references)
			//IL_266b: Unknown result type (might be due to invalid IL or missing references)
			//IL_267f: Unknown result type (might be due to invalid IL or missing references)
			//IL_268e: Unknown result type (might be due to invalid IL or missing references)
			//IL_2695: Expected O, but got Unknown
			//IL_26bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_26fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_2711: Unknown result type (might be due to invalid IL or missing references)
			//IL_2730: Unknown result type (might be due to invalid IL or missing references)
			//IL_2746: Unknown result type (might be due to invalid IL or missing references)
			//IL_275c: Unknown result type (might be due to invalid IL or missing references)
			//IL_2772: Unknown result type (might be due to invalid IL or missing references)
			//IL_2788: Unknown result type (might be due to invalid IL or missing references)
			//IL_25c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_2819: Unknown result type (might be due to invalid IL or missing references)
			//IL_2828: Unknown result type (might be due to invalid IL or missing references)
			//IL_2837: Unknown result type (might be due to invalid IL or missing references)
			//IL_27ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_27b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_27c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_27d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_27f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_2804: Unknown result type (might be due to invalid IL or missing references)
			//IL_287e: Unknown result type (might be due to invalid IL or missing references)
			//IL_288d: Unknown result type (might be due to invalid IL or missing references)
			//IL_2921: Unknown result type (might be due to invalid IL or missing references)
			//IL_293b: Unknown result type (might be due to invalid IL or missing references)
			//IL_28d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_2979: Unknown result type (might be due to invalid IL or missing references)
			//IL_297e: Unknown result type (might be due to invalid IL or missing references)
			//IL_298c: Unknown result type (might be due to invalid IL or missing references)
			//IL_29aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_29b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_29e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_29f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_29ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_2a3b: Unknown result type (might be due to invalid IL or missing references)
			//IL_2a51: Unknown result type (might be due to invalid IL or missing references)
			//IL_2b32: Unknown result type (might be due to invalid IL or missing references)
			//IL_2b41: Unknown result type (might be due to invalid IL or missing references)
			//IL_2a79: Unknown result type (might be due to invalid IL or missing references)
			//IL_2a7e: Unknown result type (might be due to invalid IL or missing references)
			//IL_2a8c: Unknown result type (might be due to invalid IL or missing references)
			//IL_2a9b: Unknown result type (might be due to invalid IL or missing references)
			//IL_2abb: Unknown result type (might be due to invalid IL or missing references)
			//IL_2ad0: Unknown result type (might be due to invalid IL or missing references)
			//IL_2ae5: Unknown result type (might be due to invalid IL or missing references)
			//IL_2afa: Unknown result type (might be due to invalid IL or missing references)
			//IL_2b04: Unknown result type (might be due to invalid IL or missing references)
			//IL_2bad: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c3d: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c4c: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c5b: Unknown result type (might be due to invalid IL or missing references)
			//IL_2bd2: Unknown result type (might be due to invalid IL or missing references)
			//IL_2bd7: Unknown result type (might be due to invalid IL or missing references)
			//IL_2be5: Unknown result type (might be due to invalid IL or missing references)
			//IL_2bf5: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c15: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c29: Unknown result type (might be due to invalid IL or missing references)
			//IL_2c94: Unknown result type (might be due to invalid IL or missing references)
			//IL_2ca3: Unknown result type (might be due to invalid IL or missing references)
			//IL_2cb2: Unknown result type (might be due to invalid IL or missing references)
			//IL_2cd8: Unknown result type (might be due to invalid IL or missing references)
			//IL_2cdd: Unknown result type (might be due to invalid IL or missing references)
			//IL_2ceb: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d18: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d38: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d4c: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d7a: Unknown result type (might be due to invalid IL or missing references)
			//IL_2d89: Unknown result type (might be due to invalid IL or missing references)
			//IL_2dee: Unknown result type (might be due to invalid IL or missing references)
			//IL_2e37: Unknown result type (might be due to invalid IL or missing references)
			//IL_2e51: Unknown result type (might be due to invalid IL or missing references)
			//IL_2e60: Unknown result type (might be due to invalid IL or missing references)
			//IL_2e65: Unknown result type (might be due to invalid IL or missing references)
			//IL_2e73: Unknown result type (might be due to invalid IL or missing references)
			//IL_2e8e: Unknown result type (might be due to invalid IL or missing references)
			//IL_2eae: Unknown result type (might be due to invalid IL or missing references)
			//IL_2ec3: Unknown result type (might be due to invalid IL or missing references)
			//IL_2ed8: Unknown result type (might be due to invalid IL or missing references)
			//IL_2eec: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f7c: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f8b: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f9a: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f11: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f16: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f24: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f33: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f53: Unknown result type (might be due to invalid IL or missing references)
			//IL_2f67: Unknown result type (might be due to invalid IL or missing references)
			//IL_2ff6: Unknown result type (might be due to invalid IL or missing references)
			//IL_3005: Unknown result type (might be due to invalid IL or missing references)
			//IL_3014: Unknown result type (might be due to invalid IL or missing references)
			//IL_3042: Unknown result type (might be due to invalid IL or missing references)
			//IL_3049: Expected O, but got Unknown
			//IL_3074: Unknown result type (might be due to invalid IL or missing references)
			//IL_308e: Unknown result type (might be due to invalid IL or missing references)
			//IL_30a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_30c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_30dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_316e: Unknown result type (might be due to invalid IL or missing references)
			//IL_318a: Unknown result type (might be due to invalid IL or missing references)
			//IL_31a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_31c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_31da: Unknown result type (might be due to invalid IL or missing references)
			//IL_31df: Unknown result type (might be due to invalid IL or missing references)
			//IL_31ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_3215: Unknown result type (might be due to invalid IL or missing references)
			//IL_322b: Unknown result type (might be due to invalid IL or missing references)
			//IL_3241: Unknown result type (might be due to invalid IL or missing references)
			//IL_3257: Unknown result type (might be due to invalid IL or missing references)
			//IL_3266: Unknown result type (might be due to invalid IL or missing references)
			//IL_326b: Unknown result type (might be due to invalid IL or missing references)
			//IL_3279: Unknown result type (might be due to invalid IL or missing references)
			//IL_3294: Unknown result type (might be due to invalid IL or missing references)
			//IL_32b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_32c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_32d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_32dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_32ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_3305: Unknown result type (might be due to invalid IL or missing references)
			//IL_3325: Unknown result type (might be due to invalid IL or missing references)
			//IL_3339: Unknown result type (might be due to invalid IL or missing references)
			//IL_334d: Unknown result type (might be due to invalid IL or missing references)
			//IL_335c: Unknown result type (might be due to invalid IL or missing references)
			//IL_336b: Unknown result type (might be due to invalid IL or missing references)
			//IL_33a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_33b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_33c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_33f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_3404: Unknown result type (might be due to invalid IL or missing references)
			//IL_3478: Unknown result type (might be due to invalid IL or missing references)
			//IL_3491: Unknown result type (might be due to invalid IL or missing references)
			//IL_3496: Unknown result type (might be due to invalid IL or missing references)
			//IL_34aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_3460: Unknown result type (might be due to invalid IL or missing references)
			//IL_34cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_34d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_34fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_34ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_350e: Unknown result type (might be due to invalid IL or missing references)
			//IL_356a: Unknown result type (might be due to invalid IL or missing references)
			//IL_3579: Unknown result type (might be due to invalid IL or missing references)
			//IL_3588: Unknown result type (might be due to invalid IL or missing references)
			//IL_35b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_35c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_35d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_3607: Unknown result type (might be due to invalid IL or missing references)
			//IL_360c: Unknown result type (might be due to invalid IL or missing references)
			//IL_361a: Unknown result type (might be due to invalid IL or missing references)
			//IL_3647: Unknown result type (might be due to invalid IL or missing references)
			//IL_3667: Unknown result type (might be due to invalid IL or missing references)
			//IL_367b: Unknown result type (might be due to invalid IL or missing references)
			//IL_368a: Unknown result type (might be due to invalid IL or missing references)
			//IL_3691: Expected O, but got Unknown
			//IL_36b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_36d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_36ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_36ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_3703: Unknown result type (might be due to invalid IL or missing references)
			//IL_3722: Unknown result type (might be due to invalid IL or missing references)
			//IL_3738: Unknown result type (might be due to invalid IL or missing references)
			//IL_3756: Unknown result type (might be due to invalid IL or missing references)
			//IL_375b: Unknown result type (might be due to invalid IL or missing references)
			//IL_376a: Unknown result type (might be due to invalid IL or missing references)
			//IL_37f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_3812: Unknown result type (might be due to invalid IL or missing references)
			//IL_3865: Unknown result type (might be due to invalid IL or missing references)
			//IL_386a: Unknown result type (might be due to invalid IL or missing references)
			//IL_37a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_37af: Unknown result type (might be due to invalid IL or missing references)
			//IL_37c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_37ce: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)activationCanvasTMP != (Object)null)
				{
					if (!forceRebuild)
					{
						((Component)activationCanvasTMP).gameObject.SetActive(false);
						return;
					}
					if ((Object)(object)canvasGO != (Object)null)
					{
						Object.Destroy((Object)(object)canvasGO);
					}
					activationCanvasTMP = null;
					activationPanelGO = null;
					statusMessageTextTMP = null;
					validateButtonTMP = null;
					getKeyButtonTMP = null;
					validateButtonTextTMP = null;
					apiKeyInputFieldTMP = null;
					_copyLinkBtnTMP = null;
					_copyLinkTextTMP = null;
					_stepCircle1 = null;
					_stepCircle2 = null;
					_stepCircle3 = null;
					_stepLine1 = null;
					_stepLine2 = null;
					_stepLabel1 = null;
					_stepLabel2 = null;
					_stepLabel3 = null;
					_statusPillBg = null;
				}
				if ((Object)(object)Object.FindObjectOfType<EventSystem>() == (Object)null)
				{
					if ((Object)(object)eventSystemGO == (Object)null)
					{
						eventSystemGO = new GameObject("ModMenuCrew_EventSystem");
						eventSystemGO.AddComponent<EventSystem>();
						eventSystemGO.AddComponent<StandaloneInputModule>();
					}
				}
				else if ((Object)(object)eventSystemGO != (Object)null)
				{
					Object.Destroy((Object)(object)eventSystemGO);
					eventSystemGO = null;
				}
				canvasGO = new GameObject("ModMenuCrew_ActivationCanvas");
				Object.DontDestroyOnLoad((Object)(object)canvasGO);
				activationCanvasTMP = canvasGO.AddComponent<Canvas>();
				activationCanvasTMP.renderMode = (RenderMode)0;
				activationCanvasTMP.sortingOrder = 32767;
				activationCanvasTMP.pixelPerfect = true;
				CanvasScaler obj = canvasGO.AddComponent<CanvasScaler>();
				obj.uiScaleMode = (ScaleMode)1;
				obj.referenceResolution = new Vector2(1920f, 1080f);
				obj.matchWidthOrHeight = 0.5f;
				obj.referencePixelsPerUnit = 100f;
				canvasGO.AddComponent<GraphicRaycaster>();
				GameObject val = new GameObject("DarkOverlay");
				val.transform.SetParent(((Component)activationCanvasTMP).transform, false);
				Image obj2 = val.AddComponent<Image>();
				((Graphic)obj2).color = new Color(0.04f, 0.04f, 0.07f, 0.97f);
				((Graphic)obj2).raycastTarget = true;
				RectTransform component = val.GetComponent<RectTransform>();
				component.anchorMin = Vector2.zero;
				component.anchorMax = Vector2.one;
				component.sizeDelta = Vector2.zero;
				_bootOverlay = val;
				CreateStarfield((Transform)(object)component);
				CreateBootConsole(component);
				CreateScanLine(component);
				activationPanelGO = new GameObject("ActivationPanel");
				activationPanelGO.transform.SetParent(((Component)activationCanvasTMP).transform, false);
				activationPanelGO.SetActive(false);
				_panelCanvasGroup = activationPanelGO.AddComponent<CanvasGroup>();
				_panelCanvasGroup.alpha = 0f;
				_currentAlpha = 0f;
				_targetAlpha = 1f;
				_panelScaleCurrent = 0.92f;
				activationPanelGO.transform.localScale = Vector3.one * 0.92f;
				((Graphic)activationPanelGO.AddComponent<Image>()).color = GuiStyles.Theme.PanelBg;
				Outline obj3 = activationPanelGO.AddComponent<Outline>();
				((Shadow)obj3).effectColor = GuiStyles.Theme.PanelOutline;
				((Shadow)obj3).effectDistance = new Vector2(1f, -1f);
				RectTransform component2 = activationPanelGO.GetComponent<RectTransform>();
				component2.anchorMin = new Vector2(0.5f, 0.5f);
				component2.anchorMax = new Vector2(0.5f, 0.5f);
				component2.pivot = new Vector2(0.5f, 0.5f);
				component2.sizeDelta = new Vector2(1200f, 1000f);
				component2.anchoredPosition = Vector2.zero;
				CreateTechCorners(component2, new Color(GuiStyles.Theme.Accent.r, GuiStyles.Theme.Accent.g, GuiStyles.Theme.Accent.b, 0.6f));
				Texture2D val2 = LoadActivationAsset("planet_bg.png");
				if ((Object)(object)val2 != (Object)null)
				{
					GameObject val3 = new GameObject("PlanetDecoration");
					val3.transform.SetParent((Transform)(object)component2, false);
					RawImage obj4 = val3.AddComponent<RawImage>();
					obj4.texture = (Texture)(object)val2;
					((Graphic)obj4).color = new Color(1f, 1f, 1f, 0.85f);
					((Graphic)obj4).raycastTarget = false;
					RectTransform component3 = val3.GetComponent<RectTransform>();
					component3.anchorMin = new Vector2(1f, 1f);
					component3.anchorMax = new Vector2(1f, 1f);
					component3.pivot = new Vector2(1f, 1f);
					component3.sizeDelta = new Vector2(420f, 420f);
					component3.anchoredPosition = new Vector2(40f, 60f);
				}
				Texture2D val4 = LoadActivationAsset("crewmate_green.png");
				if ((Object)(object)val4 != (Object)null)
				{
					GameObject val5 = new GameObject("GreenCrewmateDeco");
					val5.transform.SetParent((Transform)(object)component2, false);
					RawImage obj5 = val5.AddComponent<RawImage>();
					obj5.texture = (Texture)(object)val4;
					((Graphic)obj5).raycastTarget = false;
					RectTransform component4 = val5.GetComponent<RectTransform>();
					component4.anchorMin = new Vector2(1f, 0f);
					component4.anchorMax = new Vector2(1f, 0f);
					component4.pivot = new Vector2(1f, 0f);
					component4.sizeDelta = new Vector2(150f, 200f);
					component4.anchoredPosition = new Vector2(20f, -10f);
				}
				GameObject val6 = new GameObject("AccentTop");
				val6.transform.SetParent((Transform)(object)component2, false);
				_accentTopLine = val6.AddComponent<Image>();
				((Graphic)_accentTopLine).color = GuiStyles.Theme.Accent * new Color(1f, 1f, 1f, 0.7f);
				((Graphic)_accentTopLine).raycastTarget = false;
				RectTransform component5 = val6.GetComponent<RectTransform>();
				component5.anchorMin = new Vector2(0f, 1f);
				component5.anchorMax = new Vector2(1f, 1f);
				component5.pivot = new Vector2(0.5f, 1f);
				component5.sizeDelta = new Vector2(-10f, 2f);
				component5.anchoredPosition = new Vector2(0f, -1f);
				GameObject val7 = new GameObject("AccentBottom");
				val7.transform.SetParent((Transform)(object)component2, false);
				_accentBottomLine = val7.AddComponent<Image>();
				((Graphic)_accentBottomLine).color = GuiStyles.Theme.Accent * new Color(1f, 1f, 1f, 0.7f);
				((Graphic)_accentBottomLine).raycastTarget = false;
				RectTransform component6 = val7.GetComponent<RectTransform>();
				component6.anchorMin = new Vector2(0f, 0f);
				component6.anchorMax = new Vector2(1f, 0f);
				component6.pivot = new Vector2(0.5f, 0f);
				component6.sizeDelta = new Vector2(-10f, 2f);
				component6.anchoredPosition = new Vector2(0f, 1f);
				_staggerElements.Clear();
				GameObject val8 = new GameObject("HeaderSection");
				val8.transform.SetParent((Transform)(object)component2, false);
				RectTransform val9 = val8.AddComponent<RectTransform>();
				val9.anchorMin = new Vector2(0.5f, 0.5f);
				val9.anchorMax = new Vector2(0.5f, 0.5f);
				val9.pivot = new Vector2(0.5f, 0.5f);
				val9.anchoredPosition = new Vector2(0f, 440f);
				val9.sizeDelta = new Vector2(1140f, 110f);
				Texture2D val10 = LoadActivationAsset("logo_crewmate.png");
				if ((Object)(object)val10 != (Object)null)
				{
					GameObject val11 = new GameObject("LogoCrewmate");
					val11.transform.SetParent((Transform)(object)val9, false);
					RawImage obj6 = val11.AddComponent<RawImage>();
					obj6.texture = (Texture)(object)val10;
					((Graphic)obj6).raycastTarget = false;
					RectTransform component7 = val11.GetComponent<RectTransform>();
					component7.anchorMin = new Vector2(0f, 0.5f);
					component7.anchorMax = new Vector2(0f, 0.5f);
					component7.pivot = new Vector2(0f, 0.5f);
					component7.sizeDelta = new Vector2(150f, 100f);
					component7.anchoredPosition = new Vector2(8f, 0f);
				}
				TextMeshProUGUI val12 = CreateTMPText(val9, "MOD\nMENU\nCREW", 14, Color.white, new Vector2(-415f, 0f), new Vector2(95f, 75f), (TextAlignmentOptions)513);
				if ((Object)(object)val12 != (Object)null)
				{
					((TMP_Text)val12).fontStyle = (FontStyles)1;
					((TMP_Text)val12).lineSpacing = -10f;
					((TMP_Text)val12).characterSpacing = 2f;
				}
				_titleTextTMP = CreateTMPText(val9, "MODMENUCREW <color=#A78BFA>// ACCESS TERMINAL</color>", 36, Color.white, new Vector2(20f, 18f), new Vector2(900f, 50f), (TextAlignmentOptions)514);
				if ((Object)(object)_titleTextTMP != (Object)null)
				{
					((TMP_Text)_titleTextTMP).fontStyle = (FontStyles)1;
					((TMP_Text)_titleTextTMP).characterSpacing = 3f;
				}
				CreateTMPText(val9, "Welcome to ModMenuCrew - Access Terminal.", 14, new Color(0.85f, 0.85f, 0.92f, 1f), new Vector2(20f, -30f), new Vector2(800f, 22f), (TextAlignmentOptions)514);
				Button val13 = CreateTMPButton(val9, "", UnityAction.op_Implicit((Action)delegate
				{
					if ((Object)(object)_panelCanvasGroup != (Object)null)
					{
						_panelCanvasGroup.alpha = 0f;
						_panelCanvasGroup.interactable = false;
						_panelCanvasGroup.blocksRaycasts = false;
					}
				}), new Vector2(485f, 0f), new Vector2(50f, 50f));
				Image component8 = ((Component)val13).GetComponent<Image>();
				if (Object.op_Implicit((Object)(object)component8))
				{
					((Graphic)component8).color = new Color(0.1f, 0.1f, 0.14f, 0.85f);
				}
				Outline obj7 = ((Component)val13).gameObject.AddComponent<Outline>();
				((Shadow)obj7).effectColor = new Color(1f, 1f, 1f, 0.1f);
				((Shadow)obj7).effectDistance = new Vector2(1f, -1f);
				TextMeshProUGUI componentInChildren = ((Component)val13).GetComponentInChildren<TextMeshProUGUI>();
				if ((Object)(object)componentInChildren != (Object)null)
				{
					((Component)componentInChildren).gameObject.SetActive(false);
				}
				GameObject val14 = new GameObject("MinBar");
				val14.transform.SetParent(((Component)val13).transform, false);
				Image obj8 = val14.AddComponent<Image>();
				((Graphic)obj8).color = new Color(1f, 1f, 1f, 0.85f);
				((Graphic)obj8).raycastTarget = false;
				RectTransform component9 = val14.GetComponent<RectTransform>();
				component9.anchorMin = new Vector2(0.5f, 0.5f);
				component9.anchorMax = new Vector2(0.5f, 0.5f);
				component9.pivot = new Vector2(0.5f, 0.5f);
				component9.sizeDelta = new Vector2(16f, 2f);
				component9.anchoredPosition = new Vector2(0f, -8f);
				Button val15 = CreateTMPButton(val9, "", UnityAction.op_Implicit((Action)delegate
				{
					try
					{
						Application.Quit();
					}
					catch
					{
					}
				}), new Vector2(540f, 0f), new Vector2(50f, 50f));
				Image component10 = ((Component)val15).GetComponent<Image>();
				if (Object.op_Implicit((Object)(object)component10))
				{
					((Graphic)component10).color = new Color(0.1f, 0.1f, 0.14f, 0.85f);
				}
				Outline obj9 = ((Component)val15).gameObject.AddComponent<Outline>();
				((Shadow)obj9).effectColor = new Color(1f, 1f, 1f, 0.1f);
				((Shadow)obj9).effectDistance = new Vector2(1f, -1f);
				TextMeshProUGUI componentInChildren2 = ((Component)val15).GetComponentInChildren<TextMeshProUGUI>();
				if ((Object)(object)componentInChildren2 != (Object)null)
				{
					((Component)componentInChildren2).gameObject.SetActive(false);
				}
				for (int i = 0; i < 2; i++)
				{
					GameObject val16 = new GameObject("CloseBar" + i);
					val16.transform.SetParent(((Component)val15).transform, false);
					Image obj10 = val16.AddComponent<Image>();
					((Graphic)obj10).color = new Color(1f, 1f, 1f, 0.85f);
					((Graphic)obj10).raycastTarget = false;
					RectTransform component11 = val16.GetComponent<RectTransform>();
					component11.anchorMin = new Vector2(0.5f, 0.5f);
					component11.anchorMax = new Vector2(0.5f, 0.5f);
					component11.pivot = new Vector2(0.5f, 0.5f);
					component11.sizeDelta = new Vector2(20f, 2f);
					((Transform)component11).localRotation = Quaternion.Euler(0f, 0f, (i == 0) ? 45f : (-45f));
				}
				WrapInStaggerGroup(val8);
				Color stepActiveColor = GuiStyles.Theme.Accent;
				_ = GuiStyles.Theme.StepInactive;
				GameObject val17 = new GameObject("StepIndicatorRow");
				val17.transform.SetParent((Transform)(object)component2, false);
				RectTransform stepRT = val17.AddComponent<RectTransform>();
				stepRT.anchorMin = new Vector2(0.5f, 0.5f);
				stepRT.anchorMax = new Vector2(0.5f, 0.5f);
				stepRT.pivot = new Vector2(0.5f, 0.5f);
				stepRT.anchoredPosition = new Vector2(0f, 320f);
				stepRT.sizeDelta = new Vector2(1080f, 95f);
				Action<string, string, string, float, bool, Action<Image, TextMeshProUGUI>> obj11 = delegate(string number, string label, string sublabel, float posX, bool isActive, Action<Image, TextMeshProUGUI> refSetter)
				{
					//IL_000b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0010: Unknown result type (might be due to invalid IL or missing references)
					//IL_0022: Unknown result type (might be due to invalid IL or missing references)
					//IL_004a: Unknown result type (might be due to invalid IL or missing references)
					//IL_0042: Unknown result type (might be due to invalid IL or missing references)
					//IL_005b: Unknown result type (might be due to invalid IL or missing references)
					//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
					//IL_007a: Unknown result type (might be due to invalid IL or missing references)
					//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
					//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
					//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
					//IL_0100: Unknown result type (might be due to invalid IL or missing references)
					//IL_0112: Unknown result type (might be due to invalid IL or missing references)
					//IL_0127: Unknown result type (might be due to invalid IL or missing references)
					//IL_013b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0140: Unknown result type (might be due to invalid IL or missing references)
					//IL_014f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0189: Unknown result type (might be due to invalid IL or missing references)
					//IL_0182: Unknown result type (might be due to invalid IL or missing references)
					//IL_019b: Unknown result type (might be due to invalid IL or missing references)
					//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
					//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
					//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
					//IL_0205: Unknown result type (might be due to invalid IL or missing references)
					GameObject val93 = new GameObject("StepCard_" + label);
					val93.transform.SetParent((Transform)(object)stepRT, false);
					Image val94 = val93.AddComponent<Image>();
					((Graphic)val94).color = (Color)(isActive ? stepActiveColor : new Color(0.12f, 0.12f, 0.18f, 0.6f));
					((Graphic)val94).raycastTarget = false;
					Outline obj65 = val93.AddComponent<Outline>();
					((Shadow)obj65).effectColor = (isActive ? new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 1f) : new Color(1f, 1f, 1f, 0.06f));
					((Shadow)obj65).effectDistance = new Vector2(1f, -1f);
					RectTransform component59 = val93.GetComponent<RectTransform>();
					component59.anchorMin = new Vector2(0.5f, 0.5f);
					component59.anchorMax = new Vector2(0.5f, 0.5f);
					component59.pivot = new Vector2(0.5f, 0.5f);
					component59.anchoredPosition = new Vector2(posX, 0f);
					component59.sizeDelta = new Vector2(60f, 60f);
					TextMeshProUGUI val95 = CreateTMPText(component59, number, 28, Color.white, Vector2.zero, new Vector2(60f, 60f), (TextAlignmentOptions)514);
					if ((Object)(object)val95 != (Object)null)
					{
						((TMP_Text)val95).fontStyle = (FontStyles)1;
					}
					TextMeshProUGUI val96 = CreateTMPText(stepRT, label, 13, isActive ? Color.white : GuiStyles.Theme.TextMuted, new Vector2(posX + 145f, 10f), new Vector2(200f, 18f), (TextAlignmentOptions)513);
					if ((Object)(object)val96 != (Object)null)
					{
						((TMP_Text)val96).fontStyle = (FontStyles)1;
						((TMP_Text)val96).characterSpacing = 2f;
					}
					CreateTMPText(stepRT, sublabel, 10, GuiStyles.Theme.TextMuted, new Vector2(posX + 145f, -10f), new Vector2(200f, 16f), (TextAlignmentOptions)513);
					refSetter?.Invoke(val94, val96);
				};
				obj11("1", "DISCORD", "Login", -370f, arg5: true, delegate(Image img, TextMeshProUGUI lbl)
				{
					_stepCircle1 = img;
					_stepLabel1 = lbl;
				});
				obj11("2", "GET KEY", "Activate mod", -40f, arg5: false, delegate(Image img, TextMeshProUGUI lbl)
				{
					_stepCircle2 = img;
					_stepLabel2 = lbl;
				});
				obj11("3", "ACTIVATE", "Activate in game", 290f, arg5: false, delegate(Image img, TextMeshProUGUI lbl)
				{
					_stepCircle3 = img;
					_stepLabel3 = lbl;
				});
				GameObject val18 = new GameObject("StepLine1");
				val18.transform.SetParent((Transform)(object)stepRT, false);
				_stepLine1 = val18.AddComponent<Image>();
				((Graphic)_stepLine1).color = new Color(0f, 0f, 0f, 0f);
				((Graphic)_stepLine1).raycastTarget = false;
				RectTransform component12 = val18.GetComponent<RectTransform>();
				component12.anchoredPosition = new Vector2(-200f, 0f);
				component12.sizeDelta = new Vector2(140f, 2f);
				TextMeshProUGUI val19 = CreateTMPText(stepRT, "- - - - - - - -", 14, GuiStyles.Theme.StepLineOff, new Vector2(-200f, 0f), new Vector2(140f, 16f), (TextAlignmentOptions)514);
				if ((Object)(object)val19 != (Object)null)
				{
					((TMP_Text)val19).characterSpacing = -2f;
				}
				GameObject val20 = new GameObject("StepLine2");
				val20.transform.SetParent((Transform)(object)stepRT, false);
				_stepLine2 = val20.AddComponent<Image>();
				((Graphic)_stepLine2).color = new Color(0f, 0f, 0f, 0f);
				((Graphic)_stepLine2).raycastTarget = false;
				RectTransform component13 = val20.GetComponent<RectTransform>();
				component13.anchoredPosition = new Vector2(130f, 0f);
				component13.sizeDelta = new Vector2(140f, 2f);
				TextMeshProUGUI val21 = CreateTMPText(stepRT, "- - - - - - - -", 14, GuiStyles.Theme.StepLineOff, new Vector2(130f, 0f), new Vector2(140f, 16f), (TextAlignmentOptions)514);
				if ((Object)(object)val21 != (Object)null)
				{
					((TMP_Text)val21).characterSpacing = -2f;
				}
				WrapInStaggerGroup(val17);
				GameObject val22 = new GameObject("LoginShadow");
				val22.transform.SetParent((Transform)(object)component2, false);
				Image obj12 = val22.AddComponent<Image>();
				((Graphic)obj12).color = new Color(0f, 0f, 0f, 0.45f);
				((Graphic)obj12).raycastTarget = false;
				RectTransform component14 = val22.GetComponent<RectTransform>();
				component14.anchorMin = new Vector2(0.5f, 0.5f);
				component14.anchorMax = new Vector2(0.5f, 0.5f);
				component14.pivot = new Vector2(0.5f, 0.5f);
				component14.anchoredPosition = new Vector2(0f, 103f);
				component14.sizeDelta = new Vector2(1108f, 296f);
				GameObject val23 = new GameObject("LoginSectionCard");
				val23.transform.SetParent((Transform)(object)component2, false);
				Image obj13 = val23.AddComponent<Image>();
				((Graphic)obj13).color = new Color(0.085f, 0.085f, 0.135f, 0.92f);
				((Graphic)obj13).raycastTarget = false;
				Outline obj14 = val23.AddComponent<Outline>();
				((Shadow)obj14).effectColor = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.1f);
				((Shadow)obj14).effectDistance = new Vector2(1f, -1f);
				RectTransform loginCardRT = val23.GetComponent<RectTransform>();
				loginCardRT.anchorMin = new Vector2(0.5f, 0.5f);
				loginCardRT.anchorMax = new Vector2(0.5f, 0.5f);
				loginCardRT.pivot = new Vector2(0.5f, 0.5f);
				loginCardRT.anchoredPosition = new Vector2(0f, 110f);
				loginCardRT.sizeDelta = new Vector2(1100f, 290f);
				TextMeshProUGUI val24 = CreateTMPText(loginCardRT, "LOGIN WITH <color=#A78BFA>DISCORD</color>", 28, Color.white, new Vector2(-205f, 108f), new Vector2(540f, 36f), (TextAlignmentOptions)513);
				if ((Object)(object)val24 != (Object)null)
				{
					((TMP_Text)val24).fontStyle = (FontStyles)1;
					((TMP_Text)val24).characterSpacing = 1f;
				}
				TextMeshProUGUI val25 = CreateTMPText(loginCardRT, "Login with your Discord account to access\nthe mod and all features.", 13, GuiStyles.Theme.TextMuted, new Vector2(-220f, 60f), new Vector2(540f, 40f), (TextAlignmentOptions)513);
				if ((Object)(object)val25 != (Object)null)
				{
					((TMP_Text)val25).lineSpacing = 4f;
				}
				Action<string, string, string, Color, float> obj15 = delegate(string iconName, string title, string desc, Color iconColor, float posY)
				{
					//IL_000b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0011: Expected O, but got Unknown
					//IL_002a: Unknown result type (might be due to invalid IL or missing references)
					//IL_0031: Unknown result type (might be due to invalid IL or missing references)
					//IL_0038: Unknown result type (might be due to invalid IL or missing references)
					//IL_0044: Unknown result type (might be due to invalid IL or missing references)
					//IL_0062: Unknown result type (might be due to invalid IL or missing references)
					//IL_0076: Unknown result type (might be due to invalid IL or missing references)
					//IL_0152: Unknown result type (might be due to invalid IL or missing references)
					//IL_0164: Unknown result type (might be due to invalid IL or missing references)
					//IL_0173: Unknown result type (might be due to invalid IL or missing references)
					//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
					//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
					//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
					//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
					//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
					//IL_0104: Unknown result type (might be due to invalid IL or missing references)
					//IL_0119: Unknown result type (might be due to invalid IL or missing references)
					//IL_012e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0138: Unknown result type (might be due to invalid IL or missing references)
					//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
					//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
					//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
					GameObject val89 = new GameObject("BulletBg_" + iconName);
					val89.transform.SetParent((Transform)(object)loginCardRT, false);
					Image obj63 = val89.AddComponent<Image>();
					((Graphic)obj63).color = new Color(iconColor.r, iconColor.g, iconColor.b, 0.18f);
					((Graphic)obj63).raycastTarget = false;
					RectTransform component57 = val89.GetComponent<RectTransform>();
					component57.anchoredPosition = new Vector2(-475f, posY);
					component57.sizeDelta = new Vector2(36f, 36f);
					Texture2D val90 = LoadActivationAsset("Icons." + iconName + ".png");
					if ((Object)(object)val90 != (Object)null)
					{
						GameObject val91 = new GameObject("BulletIcon_" + iconName);
						val91.transform.SetParent(val89.transform, false);
						RawImage obj64 = val91.AddComponent<RawImage>();
						obj64.texture = (Texture)(object)val90;
						((Graphic)obj64).color = iconColor;
						((Graphic)obj64).raycastTarget = false;
						RectTransform component58 = val91.GetComponent<RectTransform>();
						component58.anchorMin = new Vector2(0.5f, 0.5f);
						component58.anchorMax = new Vector2(0.5f, 0.5f);
						component58.pivot = new Vector2(0.5f, 0.5f);
						component58.sizeDelta = new Vector2(20f, 20f);
						component58.anchoredPosition = Vector2.zero;
					}
					TextMeshProUGUI val92 = CreateTMPText(loginCardRT, title, 14, stepActiveColor, new Vector2(-310f, posY + 9f), new Vector2(280f, 20f), (TextAlignmentOptions)513);
					if ((Object)(object)val92 != (Object)null)
					{
						((TMP_Text)val92).fontStyle = (FontStyles)1;
					}
					CreateTMPText(loginCardRT, desc, 11, GuiStyles.Theme.TextMuted, new Vector2(-285f, posY - 9f), new Vector2(330f, 18f), (TextAlignmentOptions)513);
				};
				Color arg = default(Color);
				(arg)._002Ector(0.13f, 0.82f, 0.48f, 1f);
				Color arg2 = default(Color);
				(arg2)._002Ector(0.65f, 0.55f, 0.98f, 1f);
				Color arg3 = default(Color);
				(arg3)._002Ector(0.96f, 0.78f, 0.18f, 1f);
				obj15("shield_check", "Secure & Safe", "100% secure login with Discord OAuth2", arg, 20f);
				obj15("gift", "Free Access", "Get your key completely for free", arg2, -32f);
				obj15("zap", "Fast & Easy", "Just 3 simple steps to start", arg3, -84f);
				Button val26 = (_discordLoginBtnTMP = CreateTMPButton(loginCardRT, "", UnityAction.op_Implicit((Action)delegate
				{
					if (!isValidatingNow)
					{
						if (DiscordAuthManager.IsLoggedIn)
						{
							DiscordAuthManager.Logout();
							_avatarLoadRequested = false;
							if ((Object)(object)_discordAvatarImage != (Object)null)
							{
								((Component)_discordAvatarImage).gameObject.SetActive(false);
							}
							if ((Object)(object)_activationAvatarContainer != (Object)null)
							{
								((Component)_activationAvatarContainer).gameObject.SetActive(false);
							}
							if ((Object)(object)_discordLogoGO != (Object)null)
							{
								_discordLogoGO.SetActive(true);
							}
							currentActivationStatusMessage = "Logged out from Discord.";
							ManageActivationUIVisibility();
							if (Object.op_Implicit((Object)(object)_discordLoginTextTMP))
							{
								((TMP_Text)_discordLoginTextTMP).text = "CONTINUE WITH DISCORD";
							}
							if (Object.op_Implicit((Object)(object)_discordLoginSubTextTMP))
							{
								((TMP_Text)_discordLoginSubTextTMP).text = "Click to login with Discord";
							}
							try
							{
								RenderUserKeysList();
								return;
							}
							catch
							{
								return;
							}
						}
						if (!DiscordAuthManager.IsLoggingIn)
						{
							currentActivationStatusMessage = "Opening browser for Discord login...";
							ManageActivationUIVisibility();
							DiscordAuthManager.StartLoginAsync();
						}
					}
				}), new Vector2(180f, 30f), new Vector2(420f, 90f)));
				Image component15 = ((Component)val26).GetComponent<Image>();
				if (Object.op_Implicit((Object)(object)component15))
				{
					((Graphic)component15).color = new Color(GuiStyles.Theme.Blurple.r, GuiStyles.Theme.Blurple.g, GuiStyles.Theme.Blurple.b, 0.9f);
				}
				TextMeshProUGUI componentInChildren3 = ((Component)val26).GetComponentInChildren<TextMeshProUGUI>();
				if ((Object)(object)componentInChildren3 != (Object)null)
				{
					((Component)componentInChildren3).gameObject.SetActive(false);
				}
				RectTransform component16 = ((Component)val26).GetComponent<RectTransform>();
				GameObject val27 = new GameObject("DiscordShadow");
				val27.transform.SetParent((Transform)(object)loginCardRT, false);
				val27.transform.SetSiblingIndex(((Component)val26).transform.GetSiblingIndex());
				Image obj16 = val27.AddComponent<Image>();
				((Graphic)obj16).color = new Color(0.36f, 0.4f, 0.95f, 0.3f);
				((Graphic)obj16).raycastTarget = false;
				RectTransform component17 = val27.GetComponent<RectTransform>();
				component17.anchoredPosition = new Vector2(180f, 22f);
				component17.sizeDelta = new Vector2(440f, 100f);
				GameObject val28 = new GameObject("DiscordHi");
				val28.transform.SetParent((Transform)(object)component16, false);
				Image obj17 = val28.AddComponent<Image>();
				((Graphic)obj17).color = new Color(1f, 1f, 1f, 0.1f);
				((Graphic)obj17).raycastTarget = false;
				RectTransform component18 = val28.GetComponent<RectTransform>();
				component18.anchorMin = new Vector2(0f, 0.5f);
				component18.anchorMax = new Vector2(1f, 1f);
				component18.offsetMin = new Vector2(2f, 0f);
				component18.offsetMax = new Vector2(-2f, -2f);
				Texture2D val29 = LoadActivationAsset("Icons.discord.png");
				if ((Object)(object)val29 != (Object)null)
				{
					_discordLogoGO = new GameObject("DiscordLogo");
					_discordLogoGO.transform.SetParent((Transform)(object)component16, false);
					RawImage obj18 = _discordLogoGO.AddComponent<RawImage>();
					obj18.texture = (Texture)(object)val29;
					((Graphic)obj18).color = Color.white;
					((Graphic)obj18).raycastTarget = false;
					RectTransform component19 = _discordLogoGO.GetComponent<RectTransform>();
					component19.anchorMin = new Vector2(0f, 0.5f);
					component19.anchorMax = new Vector2(0f, 0.5f);
					component19.pivot = new Vector2(0f, 0.5f);
					component19.sizeDelta = new Vector2(54f, 54f);
					component19.anchoredPosition = new Vector2(22f, 0f);
				}
				string text = DiscordAuthManager.DiscordUsernameSafe ?? string.Empty;
				if (text.Length > 12)
				{
					text = text.Substring(0, 11) + "...";
				}
				_discordLoginTextTMP = CreateTMPText(component16, DiscordAuthManager.IsLoggedIn ? ("CONTINUE AS " + text) : "CONTINUE WITH DISCORD", 18, Color.white, new Vector2(50f, 12f), new Vector2(360f, 26f), (TextAlignmentOptions)513);
				if ((Object)(object)_discordLoginTextTMP != (Object)null)
				{
					((TMP_Text)_discordLoginTextTMP).fontStyle = (FontStyles)1;
					((TMP_Text)_discordLoginTextTMP).characterSpacing = 1f;
				}
				_discordLoginSubTextTMP = CreateTMPText(component16, DiscordAuthManager.IsLoggedIn ? "Click to logout" : "Click to login with Discord", 12, new Color(1f, 1f, 1f, 0.78f), new Vector2(50f, -14f), new Vector2(360f, 18f), (TextAlignmentOptions)513);
				GameObject val30 = new GameObject("TrustCheckBg");
				val30.transform.SetParent((Transform)(object)loginCardRT, false);
				Image obj19 = val30.AddComponent<Image>();
				((Graphic)obj19).color = new Color(0.13f, 0.82f, 0.48f, 0.2f);
				((Graphic)obj19).raycastTarget = false;
				RectTransform component20 = val30.GetComponent<RectTransform>();
				component20.anchoredPosition = new Vector2(-5f, -50f);
				component20.sizeDelta = new Vector2(20f, 20f);
				Texture2D val31 = LoadActivationAsset("Icons.check.png");
				if ((Object)(object)val31 != (Object)null)
				{
					GameObject val32 = new GameObject("TrustCheck");
					val32.transform.SetParent(val30.transform, false);
					RawImage obj20 = val32.AddComponent<RawImage>();
					obj20.texture = (Texture)(object)val31;
					((Graphic)obj20).color = new Color(0.13f, 0.82f, 0.48f, 1f);
					((Graphic)obj20).raycastTarget = false;
					RectTransform component21 = val32.GetComponent<RectTransform>();
					component21.anchorMin = new Vector2(0.5f, 0.5f);
					component21.anchorMax = new Vector2(0.5f, 0.5f);
					component21.pivot = new Vector2(0.5f, 0.5f);
					component21.sizeDelta = new Vector2(14f, 14f);
					component21.anchoredPosition = Vector2.zero;
				}
				CreateTMPText(loginCardRT, "We never store your password   <color=#666666>-</color>   Secure by Discord", 11, GuiStyles.Theme.TextMuted, new Vector2(220f, -50f), new Vector2(420f, 18f), (TextAlignmentOptions)513);
				Texture2D val33 = LoadActivationAsset("crewmate_red.png");
				if ((Object)(object)val33 != (Object)null)
				{
					GameObject val34 = new GameObject("CrewmateRed");
					val34.transform.SetParent((Transform)(object)loginCardRT, false);
					RawImage obj21 = val34.AddComponent<RawImage>();
					obj21.texture = (Texture)(object)val33;
					((Graphic)obj21).raycastTarget = false;
					RectTransform component22 = val34.GetComponent<RectTransform>();
					component22.anchorMin = new Vector2(1f, 0.5f);
					component22.anchorMax = new Vector2(1f, 0.5f);
					component22.pivot = new Vector2(1f, 0.5f);
					component22.sizeDelta = new Vector2(200f, 240f);
					component22.anchoredPosition = new Vector2(50f, 0f);
				}
				WrapInStaggerGroup(val23);
				GameObject val35 = new GameObject("DiscordAvatarContainer");
				val35.transform.SetParent((Transform)(object)component16, false);
				RectTransform val36 = val35.AddComponent<RectTransform>();
				val36.anchorMin = new Vector2(0f, 0.5f);
				val36.anchorMax = new Vector2(0f, 0.5f);
				val36.pivot = new Vector2(0.5f, 0.5f);
				val36.anchoredPosition = new Vector2(49f, 0f);
				val36.sizeDelta = new Vector2(54f, 54f);
				GameObject val37 = new GameObject("AvatarGlow");
				val37.transform.SetParent(val35.transform, false);
				Image val38 = val37.AddComponent<Image>();
				((Graphic)val38).color = new Color(GuiStyles.Theme.Blurple.r, GuiStyles.Theme.Blurple.g, GuiStyles.Theme.Blurple.b, 0.5f);
				((Graphic)val38).raycastTarget = false;
				RectTransform component23 = val37.GetComponent<RectTransform>();
				component23.anchorMin = new Vector2(0.5f, 0.5f);
				component23.anchorMax = new Vector2(0.5f, 0.5f);
				component23.pivot = new Vector2(0.5f, 0.5f);
				component23.anchoredPosition = Vector2.zero;
				component23.sizeDelta = new Vector2(56f, 56f);
				_activationAvatarGlow = val38;
				GameObject val39 = new GameObject("DiscordAvatar");
				val39.transform.SetParent(val35.transform, false);
				_discordAvatarImage = val39.AddComponent<RawImage>();
				((Graphic)_discordAvatarImage).color = Color.white;
				((Graphic)_discordAvatarImage).raycastTarget = false;
				RectTransform component24 = val39.GetComponent<RectTransform>();
				component24.anchorMin = new Vector2(0.5f, 0.5f);
				component24.anchorMax = new Vector2(0.5f, 0.5f);
				component24.pivot = new Vector2(0.5f, 0.5f);
				component24.anchoredPosition = Vector2.zero;
				component24.sizeDelta = new Vector2(46f, 46f);
				Outline obj22 = val39.AddComponent<Outline>();
				((Shadow)obj22).effectColor = new Color(GuiStyles.Theme.Blurple.r, GuiStyles.Theme.Blurple.g, GuiStyles.Theme.Blurple.b, 0.9f);
				((Shadow)obj22).effectDistance = new Vector2(1f, -1f);
				val35.SetActive(false);
				_activationAvatarContainer = val36;
				_copyLinkBtnTMP = CreateTMPButton(loginCardRT, "Copy Login Link", UnityAction.op_Implicit((Action)delegate
				{
					string lastAuthUrl = DiscordAuthManager.LastAuthUrl;
					if (!string.IsNullOrEmpty(lastAuthUrl))
					{
						try
						{
							GUIUtility.systemCopyBuffer = lastAuthUrl;
						}
						catch
						{
						}
						currentActivationStatusMessage = "Link copied! Paste it in your browser.";
						ManageActivationUIVisibility();
						if ((Object)(object)_copyLinkTextTMP != (Object)null)
						{
							((TMP_Text)_copyLinkTextTMP).text = "Copied!";
						}
					}
				}), new Vector2(225f, -85f), new Vector2(220f, 28f));
				_copyLinkTextTMP = ((Component)_copyLinkBtnTMP).GetComponentInChildren<TextMeshProUGUI>();
				if ((Object)(object)_copyLinkTextTMP != (Object)null)
				{
					((TMP_Text)_copyLinkTextTMP).fontSize = 11f;
				}
				Image component25 = ((Component)_copyLinkBtnTMP).GetComponent<Image>();
				if (Object.op_Implicit((Object)(object)component25))
				{
					((Graphic)component25).color = new Color(0.25f, 0.25f, 0.3f, 0.9f);
				}
				((Component)_copyLinkBtnTMP).gameObject.SetActive(false);
				GameObject val40 = new GameObject("YkShadow");
				val40.transform.SetParent((Transform)(object)component2, false);
				Image obj23 = val40.AddComponent<Image>();
				((Graphic)obj23).color = new Color(0f, 0f, 0f, 0.45f);
				((Graphic)obj23).raycastTarget = false;
				RectTransform component26 = val40.GetComponent<RectTransform>();
				component26.anchorMin = new Vector2(0.5f, 0.5f);
				component26.anchorMax = new Vector2(0.5f, 0.5f);
				component26.pivot = new Vector2(0.5f, 0.5f);
				component26.anchoredPosition = new Vector2(-285f, -207f);
				component26.sizeDelta = new Vector2(538f, 296f);
				GameObject val41 = new GameObject("YourKeysCard");
				val41.transform.SetParent((Transform)(object)component2, false);
				Image obj24 = val41.AddComponent<Image>();
				((Graphic)obj24).color = new Color(0.085f, 0.085f, 0.135f, 0.92f);
				((Graphic)obj24).raycastTarget = false;
				Outline obj25 = val41.AddComponent<Outline>();
				((Shadow)obj25).effectColor = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.1f);
				((Shadow)obj25).effectDistance = new Vector2(1f, -1f);
				RectTransform component27 = val41.GetComponent<RectTransform>();
				component27.anchorMin = new Vector2(0.5f, 0.5f);
				component27.anchorMax = new Vector2(0.5f, 0.5f);
				component27.pivot = new Vector2(0.5f, 0.5f);
				component27.anchoredPosition = new Vector2(-285f, -200f);
				component27.sizeDelta = new Vector2(530f, 290f);
				_yourKeysCardRT = component27;
				Texture2D val42 = LoadActivationAsset("Icons.key_round.png");
				if ((Object)(object)val42 != (Object)null)
				{
					GameObject val43 = new GameObject("YkKeyIcon");
					val43.transform.SetParent((Transform)(object)component27, false);
					RawImage obj26 = val43.AddComponent<RawImage>();
					obj26.texture = (Texture)(object)val42;
					((Graphic)obj26).color = stepActiveColor;
					((Graphic)obj26).raycastTarget = false;
					RectTransform component28 = val43.GetComponent<RectTransform>();
					component28.anchoredPosition = new Vector2(-230f, 122f);
					component28.sizeDelta = new Vector2(20f, 20f);
				}
				TextMeshProUGUI val44 = CreateTMPText(component27, "YOUR KEYS", 13, stepActiveColor, new Vector2(-95f, 122f), new Vector2(200f, 20f), (TextAlignmentOptions)513);
				if ((Object)(object)val44 != (Object)null)
				{
					((TMP_Text)val44).fontStyle = (FontStyles)1;
					((TMP_Text)val44).characterSpacing = 4f;
				}
				GameObject val45 = new GameObject("YkCounterPill");
				val45.transform.SetParent((Transform)(object)component27, false);
				Image obj27 = val45.AddComponent<Image>();
				((Graphic)obj27).color = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.18f);
				((Graphic)obj27).raycastTarget = false;
				Outline obj28 = val45.AddComponent<Outline>();
				((Shadow)obj28).effectColor = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.45f);
				((Shadow)obj28).effectDistance = new Vector2(1f, -1f);
				RectTransform component29 = val45.GetComponent<RectTransform>();
				component29.anchoredPosition = new Vector2(155f, 122f);
				component29.sizeDelta = new Vector2(170f, 26f);
				_yourKeysCounterTxt = CreateTMPText(component29, "Login with Discord", 11, stepActiveColor, Vector2.zero, new Vector2(160f, 24f), (TextAlignmentOptions)514);
				if ((Object)(object)_yourKeysCounterTxt != (Object)null)
				{
					((TMP_Text)_yourKeysCounterTxt).fontStyle = (FontStyles)1;
				}
				_yourKeysRows = new YourKeysRow[5];
				for (int j = 0; j < 5; j++)
				{
					int capturedIdx = j;
					GameObject val46 = new GameObject($"YkRow{j}");
					val46.transform.SetParent((Transform)(object)component27, false);
					Image val47 = val46.AddComponent<Image>();
					((Graphic)val47).color = new Color(0.04f, 0.04f, 0.08f, 0.95f);
					Button obj29 = val46.AddComponent<Button>();
					((Selectable)obj29).targetGraphic = (Graphic)(object)val47;
					((UnityEvent)obj29.onClick).AddListener(UnityAction.op_Implicit((Action)delegate
					{
						OnYourKeyRowClicked(capturedIdx);
					}));
					Outline obj30 = val46.AddComponent<Outline>();
					((Shadow)obj30).effectColor = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.2f);
					((Shadow)obj30).effectDistance = new Vector2(1f, -1f);
					RectTransform component30 = val46.GetComponent<RectTransform>();
					component30.anchorMin = new Vector2(0.5f, 0.5f);
					component30.anchorMax = new Vector2(0.5f, 0.5f);
					component30.pivot = new Vector2(0.5f, 0.5f);
					component30.anchoredPosition = new Vector2(0f, (float)(86 - j * 40));
					component30.sizeDelta = new Vector2(490f, 36f);
					Texture2D val48 = LoadActivationAsset("Icons.key_round.png");
					if ((Object)(object)val48 != (Object)null)
					{
						GameObject val49 = new GameObject("RowKeyIcon");
						val49.transform.SetParent((Transform)(object)component30, false);
						RawImage obj31 = val49.AddComponent<RawImage>();
						obj31.texture = (Texture)(object)val48;
						((Graphic)obj31).color = stepActiveColor;
						((Graphic)obj31).raycastTarget = false;
						RectTransform component31 = val49.GetComponent<RectTransform>();
						component31.anchoredPosition = new Vector2(-225f, 0f);
						component31.sizeDelta = new Vector2(20f, 20f);
					}
					TextMeshProUGUI val50 = CreateTMPText(component30, "", 13, Color.white, new Vector2(-85f, 7f), new Vector2(280f, 18f), (TextAlignmentOptions)513);
					if ((Object)(object)val50 != (Object)null)
					{
						((TMP_Text)val50).fontStyle = (FontStyles)1;
					}
					TextMeshProUGUI dateTxt = CreateTMPText(component30, "", 10, GuiStyles.Theme.TextMuted, new Vector2(-85f, -8f), new Vector2(280f, 16f), (TextAlignmentOptions)513);
					TextMeshProUGUI badgeTxt = CreateTMPText(component30, "", 11, new Color(0.13f, 0.82f, 0.48f, 1f), new Vector2(150f, 0f), new Vector2(80f, 18f), (TextAlignmentOptions)516);
					GameObject val51 = new GameObject("CopyBtn");
					val51.transform.SetParent((Transform)(object)component30, false);
					Image val52 = val51.AddComponent<Image>();
					((Graphic)val52).color = new Color(0.1f, 0.1f, 0.14f, 0.95f);
					Outline obj32 = val51.AddComponent<Outline>();
					((Shadow)obj32).effectColor = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.4f);
					((Shadow)obj32).effectDistance = new Vector2(1f, -1f);
					Button val53 = val51.AddComponent<Button>();
					((Selectable)val53).targetGraphic = (Graphic)(object)val52;
					((UnityEvent)val53.onClick).AddListener(UnityAction.op_Implicit((Action)delegate
					{
						OnYourKeyCopyClicked(capturedIdx);
					}));
					RectTransform component32 = val51.GetComponent<RectTransform>();
					component32.anchoredPosition = new Vector2(225f, 0f);
					component32.sizeDelta = new Vector2(32f, 28f);
					Texture2D val54 = LoadActivationAsset("Icons.copy.png");
					if ((Object)(object)val54 != (Object)null)
					{
						GameObject val55 = new GameObject("CopyIco");
						val55.transform.SetParent(val51.transform, false);
						RawImage obj33 = val55.AddComponent<RawImage>();
						obj33.texture = (Texture)(object)val54;
						((Graphic)obj33).color = GuiStyles.Theme.TextMuted;
						((Graphic)obj33).raycastTarget = false;
						RectTransform component33 = val55.GetComponent<RectTransform>();
						component33.anchorMin = new Vector2(0.5f, 0.5f);
						component33.anchorMax = new Vector2(0.5f, 0.5f);
						component33.pivot = new Vector2(0.5f, 0.5f);
						component33.sizeDelta = new Vector2(14f, 14f);
						component33.anchoredPosition = Vector2.zero;
					}
					val46.SetActive(false);
					_yourKeysRows[j] = new YourKeysRow
					{
						Root = val46,
						PreviewTxt = val50,
						DateTxt = dateTxt,
						BadgeTxt = badgeTxt,
						CopyBtn = val53
					};
				}
				_yourKeysEmptyTxt = CreateTMPText(component27, "Login with Discord to see your keys.", 12, GuiStyles.Theme.TextMuted, new Vector2(0f, 0f), new Vector2(480f, 30f), (TextAlignmentOptions)514);
				Button obj34 = CreateTMPButton(component27, "View key history  >", UnityAction.op_Implicit((Action)delegate
				{
					try
					{
						OpenBrowser("https://crewcore.online/#keys");
					}
					catch
					{
					}
				}), new Vector2(0f, -125f), new Vector2(490f, 32f));
				Image component34 = ((Component)obj34).GetComponent<Image>();
				if (Object.op_Implicit((Object)(object)component34))
				{
					((Graphic)component34).color = new Color(0.1f, 0.1f, 0.14f, 0.85f);
				}
				Outline obj35 = ((Component)obj34).gameObject.AddComponent<Outline>();
				((Shadow)obj35).effectColor = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.2f);
				((Shadow)obj35).effectDistance = new Vector2(1f, -1f);
				TextMeshProUGUI componentInChildren4 = ((Component)obj34).GetComponentInChildren<TextMeshProUGUI>();
				if ((Object)(object)componentInChildren4 != (Object)null)
				{
					((TMP_Text)componentInChildren4).fontSize = 12f;
					((Graphic)componentInChildren4).color = stepActiveColor;
					((TMP_Text)componentInChildren4).fontStyle = (FontStyles)1;
				}
				WrapInStaggerGroup(val41);
				GameObject val56 = new GameObject("LkShadow");
				val56.transform.SetParent((Transform)(object)component2, false);
				Image obj36 = val56.AddComponent<Image>();
				((Graphic)obj36).color = new Color(0f, 0f, 0f, 0.45f);
				((Graphic)obj36).raycastTarget = false;
				RectTransform component35 = val56.GetComponent<RectTransform>();
				component35.anchorMin = new Vector2(0.5f, 0.5f);
				component35.anchorMax = new Vector2(0.5f, 0.5f);
				component35.pivot = new Vector2(0.5f, 0.5f);
				component35.anchoredPosition = new Vector2(285f, -207f);
				component35.sizeDelta = new Vector2(538f, 296f);
				GameObject val57 = new GameObject("LicenseKeyCard");
				val57.transform.SetParent((Transform)(object)component2, false);
				Image obj37 = val57.AddComponent<Image>();
				((Graphic)obj37).color = new Color(0.085f, 0.085f, 0.135f, 0.92f);
				((Graphic)obj37).raycastTarget = false;
				Outline obj38 = val57.AddComponent<Outline>();
				((Shadow)obj38).effectColor = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.1f);
				((Shadow)obj38).effectDistance = new Vector2(1f, -1f);
				RectTransform component36 = val57.GetComponent<RectTransform>();
				component36.anchorMin = new Vector2(0.5f, 0.5f);
				component36.anchorMax = new Vector2(0.5f, 0.5f);
				component36.pivot = new Vector2(0.5f, 0.5f);
				component36.anchoredPosition = new Vector2(285f, -200f);
				component36.sizeDelta = new Vector2(530f, 290f);
				Texture2D val58 = LoadActivationAsset("Icons.key_round.png");
				if ((Object)(object)val58 != (Object)null)
				{
					GameObject val59 = new GameObject("LkKeyIcon");
					val59.transform.SetParent((Transform)(object)component36, false);
					RawImage obj39 = val59.AddComponent<RawImage>();
					obj39.texture = (Texture)(object)val58;
					((Graphic)obj39).color = stepActiveColor;
					((Graphic)obj39).raycastTarget = false;
					RectTransform component37 = val59.GetComponent<RectTransform>();
					component37.anchoredPosition = new Vector2(-230f, 122f);
					component37.sizeDelta = new Vector2(20f, 20f);
				}
				TextMeshProUGUI val60 = CreateTMPText(component36, "LICENSE KEY", 13, stepActiveColor, new Vector2(-95f, 122f), new Vector2(220f, 20f), (TextAlignmentOptions)513);
				if ((Object)(object)val60 != (Object)null)
				{
					((TMP_Text)val60).fontStyle = (FontStyles)1;
					((TMP_Text)val60).characterSpacing = 4f;
				}
				apiKeyInputFieldTMP = CreateTMPInputField(component36, "", "Paste your license key here...", new Vector2(-20f, 30f), new Vector2(440f, 50f));
				_inputFieldBgImage = ((Component)apiKeyInputFieldTMP).GetComponent<Image>();
				if ((Object)(object)_inputFieldBgImage != (Object)null)
				{
					((Graphic)_inputFieldBgImage).color = new Color(0.04f, 0.04f, 0.08f, 0.95f);
				}
				_inputFieldOutline = ((Component)apiKeyInputFieldTMP).gameObject.AddComponent<Outline>();
				((Shadow)_inputFieldOutline).effectColor = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.45f);
				((Shadow)_inputFieldOutline).effectDistance = new Vector2(2f, -2f);
				if ((Object)(object)apiKeyInputFieldTMP != (Object)null)
				{
					((UnityEvent<string>)(object)apiKeyInputFieldTMP.onSubmit).AddListener(UnityAction<string>.op_Implicit((Action<string>)delegate(string submitted)
					{
						TrySubmitActivation(submitted);
					}));
				}
				GameObject val61 = new GameObject("PasteBtn");
				val61.transform.SetParent((Transform)(object)component36, false);
				Image val62 = val61.AddComponent<Image>();
				((Graphic)val62).color = new Color(0.1f, 0.1f, 0.14f, 0.95f);
				Outline obj40 = val61.AddComponent<Outline>();
				((Shadow)obj40).effectColor = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.45f);
				((Shadow)obj40).effectDistance = new Vector2(1f, -1f);
				Button obj41 = val61.AddComponent<Button>();
				((Selectable)obj41).targetGraphic = (Graphic)(object)val62;
				((UnityEvent)obj41.onClick).AddListener(UnityAction.op_Implicit((Action)delegate
				{
					try
					{
						string text3 = GUIUtility.systemCopyBuffer ?? string.Empty;
						if ((Object)(object)apiKeyInputFieldTMP != (Object)null)
						{
							apiKeyInputFieldTMP.text = text3.Trim();
						}
						ManageActivationUIVisibility();
					}
					catch
					{
					}
				}));
				RectTransform component38 = val61.GetComponent<RectTransform>();
				component38.anchoredPosition = new Vector2(225f, 30f);
				component38.sizeDelta = new Vector2(40f, 40f);
				Texture2D val63 = LoadActivationAsset("Icons.copy.png");
				if ((Object)(object)val63 != (Object)null)
				{
					GameObject val64 = new GameObject("PasteIco");
					val64.transform.SetParent((Transform)(object)component38, false);
					RawImage obj42 = val64.AddComponent<RawImage>();
					obj42.texture = (Texture)(object)val63;
					((Graphic)obj42).color = GuiStyles.Theme.TextMuted;
					((Graphic)obj42).raycastTarget = false;
					RectTransform component39 = val64.GetComponent<RectTransform>();
					component39.anchorMin = new Vector2(0.5f, 0.5f);
					component39.anchorMax = new Vector2(0.5f, 0.5f);
					component39.pivot = new Vector2(0.5f, 0.5f);
					component39.sizeDelta = new Vector2(20f, 20f);
					component39.anchoredPosition = Vector2.zero;
				}
				getKeyButtonTMP = CreateTMPButton(component36, "", UnityAction.op_Implicit((Action)delegate
				{
					float realtimeSinceStartup = Time.realtimeSinceStartup;
					if (!(realtimeSinceStartup - _lastGetKeyClickTime < 2f))
					{
						_lastGetKeyClickTime = realtimeSinceStartup;
						if (!OpenBrowser("https://crewcore.online/"))
						{
							currentActivationStatusMessage = "Could not open browser. Visit crewcore.online manually.";
							ManageActivationUIVisibility();
						}
					}
				}), new Vector2(-130f, -75f), new Vector2(232f, 80f));
				TextMeshProUGUI componentInChildren5 = ((Component)getKeyButtonTMP).GetComponentInChildren<TextMeshProUGUI>();
				if ((Object)(object)componentInChildren5 != (Object)null)
				{
					((Component)componentInChildren5).gameObject.SetActive(false);
				}
				RectTransform component40 = ((Component)getKeyButtonTMP).GetComponent<RectTransform>();
				Image component41 = ((Component)getKeyButtonTMP).GetComponent<Image>();
				if (Object.op_Implicit((Object)(object)component41))
				{
					((Graphic)component41).color = new Color(0.18f, 0.18f, 0.22f, 0.95f);
				}
				Texture2D val65 = LoadActivationAsset("Icons.key_round.png");
				if ((Object)(object)val65 != (Object)null)
				{
					GameObject val66 = new GameObject("GkIcon");
					val66.transform.SetParent((Transform)(object)component40, false);
					RawImage obj43 = val66.AddComponent<RawImage>();
					obj43.texture = (Texture)(object)val65;
					((Graphic)obj43).color = stepActiveColor;
					((Graphic)obj43).raycastTarget = false;
					RectTransform component42 = val66.GetComponent<RectTransform>();
					component42.anchoredPosition = new Vector2(-88f, 0f);
					component42.sizeDelta = new Vector2(36f, 36f);
				}
				TextMeshProUGUI val67 = CreateTMPText(component40, "GET KEY", 16, Color.white, new Vector2(40f, 11f), new Vector2(180f, 22f), (TextAlignmentOptions)513);
				if ((Object)(object)val67 != (Object)null)
				{
					((TMP_Text)val67).fontStyle = (FontStyles)1;
					((TMP_Text)val67).characterSpacing = 1f;
				}
				CreateTMPText(component40, "Generate a new license key", 11, GuiStyles.Theme.TextMuted, new Vector2(40f, -12f), new Vector2(190f, 16f), (TextAlignmentOptions)513);
				WrapInStaggerGroup(((Component)getKeyButtonTMP).gameObject);
				GameObject val68 = new GameObject("ActShadow");
				val68.transform.SetParent((Transform)(object)component36, false);
				Image obj44 = val68.AddComponent<Image>();
				((Graphic)obj44).color = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.35f);
				((Graphic)obj44).raycastTarget = false;
				RectTransform component43 = val68.GetComponent<RectTransform>();
				component43.anchoredPosition = new Vector2(130f, -82f);
				component43.sizeDelta = new Vector2(252f, 86f);
				validateButtonTMP = CreateTMPButton(component36, "", UnityAction.op_Implicit((Action)delegate
				{
					TrySubmitActivation(((Object)(object)apiKeyInputFieldTMP != (Object)null) ? apiKeyInputFieldTMP.text : null);
				}), new Vector2(130f, -75f), new Vector2(232f, 80f));
				TextMeshProUGUI componentInChildren6 = ((Component)validateButtonTMP).GetComponentInChildren<TextMeshProUGUI>();
				if ((Object)(object)componentInChildren6 != (Object)null)
				{
					((Component)componentInChildren6).gameObject.SetActive(false);
				}
				RectTransform component44 = ((Component)validateButtonTMP).GetComponent<RectTransform>();
				_validateBtnImage = ((Component)validateButtonTMP).GetComponent<Image>();
				if (Object.op_Implicit((Object)(object)_validateBtnImage))
				{
					((Graphic)_validateBtnImage).color = stepActiveColor;
				}
				_validateBtnOutline = ((Component)validateButtonTMP).GetComponent<Outline>() ?? ((Component)validateButtonTMP).gameObject.AddComponent<Outline>();
				((Shadow)_validateBtnOutline).effectColor = new Color(1f, 1f, 1f, 0.18f);
				((Shadow)_validateBtnOutline).effectDistance = new Vector2(1f, -1f);
				GameObject val69 = new GameObject("ActHi");
				val69.transform.SetParent((Transform)(object)component44, false);
				Image obj45 = val69.AddComponent<Image>();
				((Graphic)obj45).color = new Color(1f, 1f, 1f, 0.12f);
				((Graphic)obj45).raycastTarget = false;
				RectTransform component45 = val69.GetComponent<RectTransform>();
				component45.anchorMin = new Vector2(0f, 0.5f);
				component45.anchorMax = new Vector2(1f, 1f);
				component45.offsetMin = new Vector2(2f, 0f);
				component45.offsetMax = new Vector2(-2f, -2f);
				Texture2D val70 = LoadActivationAsset("Icons.lock_keyhole_open.png");
				if ((Object)(object)val70 != (Object)null)
				{
					GameObject val71 = new GameObject("ActIcon");
					val71.transform.SetParent((Transform)(object)component44, false);
					RawImage obj46 = val71.AddComponent<RawImage>();
					obj46.texture = (Texture)(object)val70;
					((Graphic)obj46).color = Color.white;
					((Graphic)obj46).raycastTarget = false;
					RectTransform component46 = val71.GetComponent<RectTransform>();
					component46.anchoredPosition = new Vector2(-88f, 0f);
					component46.sizeDelta = new Vector2(36f, 36f);
				}
				validateButtonTextTMP = CreateTMPText(component44, "[ ACTIVATE ]", 16, Color.white, new Vector2(40f, 11f), new Vector2(180f, 22f), (TextAlignmentOptions)513);
				if ((Object)(object)validateButtonTextTMP != (Object)null)
				{
					((TMP_Text)validateButtonTextTMP).fontStyle = (FontStyles)1;
					((TMP_Text)validateButtonTextTMP).characterSpacing = 1f;
				}
				CreateTMPText(component44, "Activate your key", 11, new Color(1f, 1f, 1f, 0.85f), new Vector2(40f, -12f), new Vector2(180f, 16f), (TextAlignmentOptions)513);
				WrapInStaggerGroup(((Component)validateButtonTMP).gameObject);
				WrapInStaggerGroup(val57);
				GameObject val72 = new GameObject("FeaturesBar");
				val72.transform.SetParent((Transform)(object)component2, false);
				RectTransform featRowRT = val72.AddComponent<RectTransform>();
				featRowRT.anchorMin = new Vector2(0.5f, 0.5f);
				featRowRT.anchorMax = new Vector2(0.5f, 0.5f);
				featRowRT.pivot = new Vector2(0.5f, 0.5f);
				featRowRT.anchoredPosition = new Vector2(0f, -410f);
				featRowRT.sizeDelta = new Vector2(1140f, 90f);
				Action<string, string, string, Color, float> obj47 = delegate(string iconName, string title, string desc, Color iconColor, float posX)
				{
					//IL_001a: Unknown result type (might be due to invalid IL or missing references)
					//IL_001f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0031: Unknown result type (might be due to invalid IL or missing references)
					//IL_004c: Unknown result type (might be due to invalid IL or missing references)
					//IL_005c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0077: Unknown result type (might be due to invalid IL or missing references)
					//IL_008b: Unknown result type (might be due to invalid IL or missing references)
					//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
					//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
					//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
					//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
					//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
					//IL_0106: Unknown result type (might be due to invalid IL or missing references)
					//IL_010c: Expected O, but got Unknown
					//IL_0120: Unknown result type (might be due to invalid IL or missing references)
					//IL_0127: Unknown result type (might be due to invalid IL or missing references)
					//IL_012e: Unknown result type (might be due to invalid IL or missing references)
					//IL_013a: Unknown result type (might be due to invalid IL or missing references)
					//IL_015b: Unknown result type (might be due to invalid IL or missing references)
					//IL_016f: Unknown result type (might be due to invalid IL or missing references)
					//IL_023f: Unknown result type (might be due to invalid IL or missing references)
					//IL_024b: Unknown result type (might be due to invalid IL or missing references)
					//IL_025a: Unknown result type (might be due to invalid IL or missing references)
					//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
					//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
					//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
					//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
					//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
					//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
					//IL_020c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0221: Unknown result type (might be due to invalid IL or missing references)
					//IL_022b: Unknown result type (might be due to invalid IL or missing references)
					//IL_028f: Unknown result type (might be due to invalid IL or missing references)
					//IL_029e: Unknown result type (might be due to invalid IL or missing references)
					//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
					GameObject val84 = new GameObject("Feat_" + title.Replace(" ", ""));
					val84.transform.SetParent((Transform)(object)featRowRT, false);
					Image obj55 = val84.AddComponent<Image>();
					((Graphic)obj55).color = new Color(0.08f, 0.08f, 0.12f, 0.6f);
					((Graphic)obj55).raycastTarget = false;
					Outline obj56 = val84.AddComponent<Outline>();
					((Shadow)obj56).effectColor = new Color(1f, 1f, 1f, 0.04f);
					((Shadow)obj56).effectDistance = new Vector2(1f, -1f);
					RectTransform component54 = val84.GetComponent<RectTransform>();
					component54.anchorMin = new Vector2(0.5f, 0.5f);
					component54.anchorMax = new Vector2(0.5f, 0.5f);
					component54.pivot = new Vector2(0.5f, 0.5f);
					component54.anchoredPosition = new Vector2(posX, 0f);
					component54.sizeDelta = new Vector2(265f, 80f);
					GameObject val85 = new GameObject("FeatIconBg");
					val85.transform.SetParent((Transform)(object)component54, false);
					Image obj57 = val85.AddComponent<Image>();
					((Graphic)obj57).color = new Color(iconColor.r, iconColor.g, iconColor.b, 0.18f);
					((Graphic)obj57).raycastTarget = false;
					RectTransform component55 = val85.GetComponent<RectTransform>();
					component55.anchoredPosition = new Vector2(-100f, 0f);
					component55.sizeDelta = new Vector2(44f, 44f);
					Texture2D val86 = LoadActivationAsset("Icons." + iconName + ".png");
					if ((Object)(object)val86 != (Object)null)
					{
						GameObject val87 = new GameObject("FeatIcon");
						val87.transform.SetParent(val85.transform, false);
						RawImage obj58 = val87.AddComponent<RawImage>();
						obj58.texture = (Texture)(object)val86;
						((Graphic)obj58).color = iconColor;
						((Graphic)obj58).raycastTarget = false;
						RectTransform component56 = val87.GetComponent<RectTransform>();
						component56.anchorMin = new Vector2(0.5f, 0.5f);
						component56.anchorMax = new Vector2(0.5f, 0.5f);
						component56.pivot = new Vector2(0.5f, 0.5f);
						component56.sizeDelta = new Vector2(26f, 26f);
						component56.anchoredPosition = Vector2.zero;
					}
					TextMeshProUGUI val88 = CreateTMPText(component54, title, 12, iconColor, new Vector2(15f, 14f), new Vector2(170f, 18f), (TextAlignmentOptions)513);
					if ((Object)(object)val88 != (Object)null)
					{
						((TMP_Text)val88).fontStyle = (FontStyles)1;
						((TMP_Text)val88).characterSpacing = 1f;
					}
					CreateTMPText(component54, desc, 10, GuiStyles.Theme.TextMuted, new Vector2(15f, -10f), new Vector2(170f, 32f), (TextAlignmentOptions)513);
				};
				Color arg4 = default(Color);
				(arg4)._002Ector(0.13f, 0.82f, 0.48f, 1f);
				Color arg5 = default(Color);
				(arg5)._002Ector(0.78f, 0.55f, 0.98f, 1f);
				Color arg6 = default(Color);
				(arg6)._002Ector(0.4f, 0.65f, 0.98f, 1f);
				Color arg7 = default(Color);
				(arg7)._002Ector(0.98f, 0.62f, 0.3f, 1f);
				obj47("shield_alert", "ANTI-BAN PROTECTION", "Stay safe while using\nadvanced protection", arg4, -420f);
				obj47("crown", "PREMIUM FEATURES", "Unlock all mod menu\nfeatures", arg5, -140f);
				obj47("zap", "REGULAR UPDATES", "Always the latest version\nand improvements", arg6, 140f);
				obj47("headphones", "DISCORD SUPPORT", "Get help and support\nfrom our team", arg7, 420f);
				WrapInStaggerGroup(val72);
				GameObject val73 = new GameObject("FooterBar");
				val73.transform.SetParent((Transform)(object)component2, false);
				RectTransform val74 = val73.AddComponent<RectTransform>();
				val74.anchorMin = new Vector2(0.5f, 0.5f);
				val74.anchorMax = new Vector2(0.5f, 0.5f);
				val74.pivot = new Vector2(0.5f, 0.5f);
				val74.anchoredPosition = new Vector2(0f, -480f);
				val74.sizeDelta = new Vector2(1140f, 30f);
				GameObject val75 = new GameObject("SysDotGlow");
				val75.transform.SetParent((Transform)(object)val74, false);
				Image obj48 = val75.AddComponent<Image>();
				((Graphic)obj48).color = new Color(0.13f, 0.82f, 0.48f, 0.3f);
				((Graphic)obj48).raycastTarget = false;
				RectTransform component47 = val75.GetComponent<RectTransform>();
				component47.anchoredPosition = new Vector2(-540f, 0f);
				component47.sizeDelta = new Vector2(20f, 20f);
				GameObject val76 = new GameObject("SysDot");
				val76.transform.SetParent((Transform)(object)val74, false);
				Image obj49 = val76.AddComponent<Image>();
				((Graphic)obj49).color = new Color(0.13f, 0.82f, 0.48f, 1f);
				((Graphic)obj49).raycastTarget = false;
				RectTransform component48 = val76.GetComponent<RectTransform>();
				component48.anchoredPosition = new Vector2(-540f, 0f);
				component48.sizeDelta = new Vector2(10f, 10f);
				TextMeshProUGUI val77 = CreateTMPText(val74, "SYSTEM STATUS", 10, Color.white, new Vector2(-415f, 6f), new Vector2(200f, 16f), (TextAlignmentOptions)513);
				if ((Object)(object)val77 != (Object)null)
				{
					((TMP_Text)val77).fontStyle = (FontStyles)1;
					((TMP_Text)val77).characterSpacing = 2f;
				}
				CreateTMPText(val74, "All systems operational", 10, GuiStyles.Theme.TextMuted, new Vector2(-405f, -8f), new Vector2(220f, 14f), (TextAlignmentOptions)513);
				Button val78 = (_bindKeyBtnTMP = CreateTMPButton(val74, "", UnityAction.op_Implicit((Action)delegate
				{
					if (!isValidatingNow)
					{
						_isBindingToggleKey = true;
						if ((Object)(object)_toggleKeyTextTMP != (Object)null)
						{
							((TMP_Text)_toggleKeyTextTMP).text = "Press Any Key...";
						}
					}
				}), new Vector2(0f, 0f), new Vector2(240f, 36f)));
				TextMeshProUGUI componentInChildren7 = ((Component)val78).GetComponentInChildren<TextMeshProUGUI>();
				if ((Object)(object)componentInChildren7 != (Object)null)
				{
					((Component)componentInChildren7).gameObject.SetActive(false);
				}
				Image component49 = ((Component)val78).GetComponent<Image>();
				if (Object.op_Implicit((Object)(object)component49))
				{
					((Graphic)component49).color = new Color(0.05f, 0.05f, 0.08f, 0.9f);
				}
				Outline obj50 = ((Component)val78).gameObject.AddComponent<Outline>();
				((Shadow)obj50).effectColor = stepActiveColor * new Color(1f, 1f, 1f, 0.7f);
				((Shadow)obj50).effectDistance = new Vector2(1f, -1f);
				RectTransform component50 = ((Component)val78).GetComponent<RectTransform>();
				ConfigEntry<KeyCode> menuToggleKey = CheatConfig.MenuToggleKey;
				KeyCode val79 = (KeyCode)((menuToggleKey == null) ? 282 : ((int)menuToggleKey.Value));
				string text2 = ((object)(KeyCode)val79).ToString();
				_toggleKeyTextTMP = CreateTMPText(component50, "Menu Key: <color=#A78BFA>[ " + text2 + " ]</color>", 12, Color.white, Vector2.zero, new Vector2(220f, 30f), (TextAlignmentOptions)514);
				if ((Object)(object)_toggleKeyTextTMP != (Object)null)
				{
					((TMP_Text)_toggleKeyTextTMP).fontStyle = (FontStyles)1;
					((TMP_Text)_toggleKeyTextTMP).characterSpacing = 2f;
				}
				TextMeshProUGUI val80 = CreateTMPText(val74, "MODMENUCREW v6.1.4b", 10, new Color(0.85f, 0.85f, 0.92f, 0.9f), new Vector2(455f, 6f), new Vector2(220f, 16f), (TextAlignmentOptions)516);
				if ((Object)(object)val80 != (Object)null)
				{
					((TMP_Text)val80).fontStyle = (FontStyles)1;
				}
				TextMeshProUGUI val81 = CreateTMPText(val74, "<color=#A78BFA>CREWCORE.ONLINE</color>", 11, Color.white, new Vector2(455f, -8f), new Vector2(220f, 14f), (TextAlignmentOptions)516);
				if ((Object)(object)val81 != (Object)null)
				{
					((TMP_Text)val81).fontStyle = (FontStyles)1;
					((TMP_Text)val81).characterSpacing = 2f;
				}
				GameObject val82 = new GameObject("CrewcoreUl");
				val82.transform.SetParent((Transform)(object)val74, false);
				Image obj51 = val82.AddComponent<Image>();
				((Graphic)obj51).color = new Color(stepActiveColor.r, stepActiveColor.g, stepActiveColor.b, 0.85f);
				((Graphic)obj51).raycastTarget = false;
				RectTransform component51 = val82.GetComponent<RectTransform>();
				component51.anchoredPosition = new Vector2(510f, -16f);
				component51.sizeDelta = new Vector2(110f, 1f);
				GameObject val83 = new GameObject("StatusPill");
				val83.transform.SetParent((Transform)(object)component2, false);
				_statusPillBg = val83.AddComponent<Image>();
				((Graphic)_statusPillBg).color = GuiStyles.Theme.StatusPillBg;
				((Graphic)_statusPillBg).raycastTarget = false;
				Outline obj52 = val83.AddComponent<Outline>();
				((Shadow)obj52).effectColor = stepActiveColor * new Color(1f, 1f, 1f, 0.18f);
				((Shadow)obj52).effectDistance = new Vector2(1f, -1f);
				RectTransform component52 = val83.GetComponent<RectTransform>();
				component52.anchoredPosition = new Vector2(0f, 395f);
				component52.sizeDelta = new Vector2(700f, 26f);
				_statusPillRT = component52;
				statusMessageTextTMP = CreateTMPText(component52, currentActivationStatusMessage, 13, GuiStyles.Theme.TextPrimary, Vector2.zero, new Vector2(680f, 28f), (TextAlignmentOptions)514);
				if ((Object)(object)statusMessageTextTMP != (Object)null)
				{
					((TMP_Text)statusMessageTextTMP).enableWordWrapping = false;
					RectTransform component53 = ((Component)statusMessageTextTMP).GetComponent<RectTransform>();
					component53.anchorMin = Vector2.zero;
					component53.anchorMax = Vector2.one;
					component53.sizeDelta = new Vector2(-20f, 0f);
					component53.anchoredPosition = Vector2.zero;
				}
				StyleButton(getKeyButtonTMP, ref _cachedGetKeyBtnSprite, new Color(0.13f, 0.13f, 0.2f, 1f), new Color(0.08f, 0.08f, 0.13f, 1f));
				Color bottomColor = default(Color);
				(bottomColor)._002Ector(GuiStyles.Theme.Accent.r * 0.78f, GuiStyles.Theme.Accent.g * 0.78f, GuiStyles.Theme.Accent.b * 0.92f, 1f);
				StyleButton(validateButtonTMP, ref _cachedValidateBtnSprite, GuiStyles.Theme.Accent, bottomColor);
				ConfigureActivationNavigation();
				try
				{
					RenderUserKeysList();
				}
				catch
				{
				}
				if (DiscordAuthManager.IsLoggedIn)
				{
					try
					{
						RefreshUserKeys();
					}
					catch
					{
					}
				}
				((Component)activationCanvasTMP).gameObject.SetActive(false);
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error SetupActivationUI_TMP: {value}"));
			}
		}

		private void UpdateToggleKeyText()
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)_toggleKeyTextTMP != (Object)null)
				{
					TextMeshProUGUI toggleKeyTextTMP = _toggleKeyTextTMP;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Menu Key: [");
					ConfigEntry<KeyCode> menuToggleKey = CheatConfig.MenuToggleKey;
					defaultInterpolatedStringHandler.AppendFormatted<KeyCode>((KeyCode)((menuToggleKey == null) ? 282 : ((int)menuToggleKey.Value)));
					defaultInterpolatedStringHandler.AppendLiteral("]");
					((TMP_Text)toggleKeyTextTMP).text = defaultInterpolatedStringHandler.ToStringAndClear();
				}
			}
			catch
			{
			}
		}

		private static void SetNav(Selectable s, Selectable up, Selectable down, Selectable left = null, Selectable right = null)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			if (!((Object)(object)s == (Object)null))
			{
				Navigation val = new Navigation
				{
					mode = (Mode)4
				};
				val.selectOnUp = up;
				val.selectOnDown = down;
				val.selectOnLeft = left;
				val.selectOnRight = right;
				s.navigation = val;
			}
		}

		private void ConfigureActivationNavigation()
		{
			try
			{
				Selectable discordLoginBtnTMP = (Selectable)(object)_discordLoginBtnTMP;
				Selectable val = (Selectable)(object)apiKeyInputFieldTMP;
				Selectable val2 = (Selectable)(object)validateButtonTMP;
				Selectable val3 = (Selectable)(object)getKeyButtonTMP;
				Selectable copyLinkBtnTMP = (Selectable)(object)_copyLinkBtnTMP;
				Selectable bindKeyBtnTMP = (Selectable)(object)_bindKeyBtnTMP;
				SetNav(discordLoginBtnTMP, bindKeyBtnTMP, val, copyLinkBtnTMP, copyLinkBtnTMP);
				SetNav(val, discordLoginBtnTMP, val2);
				SetNav(val3, val, bindKeyBtnTMP, null, val2);
				SetNav(val2, val, bindKeyBtnTMP, val3);
				SetNav(copyLinkBtnTMP, discordLoginBtnTMP, val);
				SetNav(bindKeyBtnTMP, val2, discordLoginBtnTMP);
			}
			catch
			{
			}
		}

		private void UpdateStepIndicator(int step)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			_currentStep = step;
			Color accent = GuiStyles.Theme.Accent;
			Color val = default(Color);
			(val)._002Ector(0.12f, 0.12f, 0.18f, 0.6f);
			Color val2 = GuiStyles.Theme.Accent * new Color(1f, 1f, 1f, 0.6f);
			Color stepLineOff = GuiStyles.Theme.StepLineOff;
			if ((Object)(object)_stepCircle1 != (Object)null)
			{
				((Graphic)_stepCircle1).color = ((step >= 1) ? accent : accent);
			}
			if ((Object)(object)_stepCircle2 != (Object)null)
			{
				((Graphic)_stepCircle2).color = ((step >= 2) ? accent : val);
			}
			if ((Object)(object)_stepCircle3 != (Object)null)
			{
				((Graphic)_stepCircle3).color = ((step >= 3) ? accent : val);
			}
			if ((Object)(object)_stepLine1 != (Object)null)
			{
				((Graphic)_stepLine1).color = ((step >= 2) ? val2 : stepLineOff);
			}
			if ((Object)(object)_stepLine2 != (Object)null)
			{
				((Graphic)_stepLine2).color = ((step >= 3) ? val2 : stepLineOff);
			}
			if ((Object)(object)_stepLabel1 != (Object)null)
			{
				((Graphic)_stepLabel1).color = Color.white;
			}
			if ((Object)(object)_stepLabel2 != (Object)null)
			{
				((Graphic)_stepLabel2).color = ((step >= 2) ? Color.white : GuiStyles.Theme.TextMuted);
			}
			if ((Object)(object)_stepLabel3 != (Object)null)
			{
				((Graphic)_stepLabel3).color = ((step >= 3) ? Color.white : GuiStyles.Theme.TextMuted);
			}
		}

		private Sprite GetWhiteSprite()
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_cachedWhiteSprite == (Object)null)
			{
				_cachedWhiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 4f, 4f), Vector2.one * 0.5f, 100f, 0u, (SpriteMeshType)0);
			}
			return _cachedWhiteSprite;
		}

		private TextMeshProUGUI CreateTMPText(RectTransform parent, string text, int fontSize, Color color, Vector2 anchoredPosition, Vector2 sizeDelta, TextAlignmentOptions alignment = (TextAlignmentOptions)513)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject($"TMPText_{_tmpTextCounter++}");
			val.transform.SetParent((Transform)(object)parent, false);
			TextMeshProUGUI val2 = val.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val2).text = text;
			((TMP_Text)val2).fontSize = fontSize;
			((Graphic)val2).color = color;
			((TMP_Text)val2).alignment = alignment;
			((TMP_Text)val2).font = LoadGameFont();
			((Graphic)val2).raycastTarget = false;
			((TMP_Text)val2).extraPadding = true;
			((TMP_Text)val2).geometrySortingOrder = (VertexSortingOrder)0;
			((TMP_Text)val2).isRightToLeftText = false;
			((TMP_Text)val2).isOverlay = false;
			if ((Object)(object)((TMP_Text)val2).fontMaterial != (Object)null)
			{
				try
				{
					((TMP_Text)val2).fontMaterial.mainTexture.filterMode = (FilterMode)1;
				}
				catch
				{
				}
			}
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchoredPosition = anchoredPosition;
			component.sizeDelta = sizeDelta;
			return val2;
		}

		private TMP_InputField CreateTMPInputField(RectTransform parent, string initialText, string placeholderText, Vector2 anchoredPosition, Vector2 sizeDelta)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("TMP_InputField_Activation");
			val.transform.SetParent((Transform)(object)parent, false);
			Image obj = val.AddComponent<Image>();
			((Graphic)obj).color = GuiStyles.Theme.InputFieldBg;
			obj.sprite = GetWhiteSprite();
			TMP_InputField val2 = val.AddComponent<TMP_InputField>();
			val2.contentType = (ContentType)0;
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchoredPosition = anchoredPosition;
			component.sizeDelta = sizeDelta;
			GameObject val3 = new GameObject("Text Area");
			val3.transform.SetParent((Transform)(object)component, false);
			RectTransform val4 = val3.AddComponent<RectTransform>();
			val4.anchorMin = Vector2.zero;
			val4.anchorMax = Vector2.one;
			val4.offsetMin = new Vector2(10f, 5f);
			val4.offsetMax = new Vector2(-10f, -5f);
			val3.AddComponent<RectMask2D>();
			TMP_FontAsset font = LoadGameFont();
			GameObject val5 = new GameObject("Placeholder");
			val5.transform.SetParent((Transform)(object)val4, false);
			TextMeshProUGUI val6 = val5.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val6).text = placeholderText;
			((TMP_Text)val6).fontSize = 16f;
			((TMP_Text)val6).fontStyle = (FontStyles)0;
			((Graphic)val6).color = GuiStyles.Theme.PlaceholderText;
			((TMP_Text)val6).alignment = (TextAlignmentOptions)513;
			((TMP_Text)val6).font = font;
			((Graphic)val6).raycastTarget = false;
			((TMP_Text)val6).extraPadding = true;
			RectTransform component2 = val5.GetComponent<RectTransform>();
			component2.anchorMin = Vector2.zero;
			component2.anchorMax = Vector2.one;
			component2.sizeDelta = Vector2.zero;
			GameObject val7 = new GameObject("Text");
			val7.transform.SetParent((Transform)(object)val4, false);
			TextMeshProUGUI val8 = val7.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val8).text = initialText;
			((TMP_Text)val8).fontSize = 18f;
			((Graphic)val8).color = Color.white;
			((TMP_Text)val8).alignment = (TextAlignmentOptions)513;
			((TMP_Text)val8).font = font;
			((Graphic)val8).raycastTarget = false;
			((TMP_Text)val8).extraPadding = true;
			RectTransform component3 = val7.GetComponent<RectTransform>();
			component3.anchorMin = Vector2.zero;
			component3.anchorMax = Vector2.one;
			component3.sizeDelta = Vector2.zero;
			val2.textViewport = val4;
			val2.textComponent = (TMP_Text)(object)val8;
			val2.placeholder = (Graphic)(object)val6;
			return val2;
		}

		private Button CreateTMPButton(RectTransform parent, string buttonText, UnityAction onClickAction, Vector2 anchoredPosition, Vector2 sizeDelta)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("TMPButton_" + buttonText.Replace(" ", ""));
			val.transform.SetParent((Transform)(object)parent, false);
			Image val2 = val.AddComponent<Image>();
			val2.sprite = GetWhiteSprite();
			Button val3 = val.AddComponent<Button>();
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchoredPosition = anchoredPosition;
			component.sizeDelta = sizeDelta;
			GameObject val4 = new GameObject("Text (TMP)");
			val4.transform.SetParent((Transform)(object)component, false);
			TextMeshProUGUI obj = val4.AddComponent<TextMeshProUGUI>();
			((TMP_Text)obj).text = buttonText;
			((TMP_Text)obj).fontSize = 18f;
			((Graphic)obj).color = Color.white;
			((TMP_Text)obj).alignment = (TextAlignmentOptions)514;
			((TMP_Text)obj).font = LoadGameFont();
			((Graphic)obj).raycastTarget = false;
			((TMP_Text)obj).extraPadding = true;
			RectTransform component2 = val4.GetComponent<RectTransform>();
			component2.anchorMin = Vector2.zero;
			component2.anchorMax = Vector2.one;
			component2.sizeDelta = Vector2.zero;
			((Selectable)val3).targetGraphic = (Graphic)(object)val2;
			((UnityEvent)val3.onClick).AddListener(onClickAction);
			return val3;
		}

		private void WrapInStaggerGroup(GameObject go)
		{
			if (!((Object)(object)go == (Object)null))
			{
				CanvasGroup val = go.GetComponent<CanvasGroup>();
				if ((Object)(object)val == (Object)null)
				{
					val = go.AddComponent<CanvasGroup>();
				}
				val.alpha = 0f;
				_staggerElements.Add(val);
			}
		}

		private void CreateScanLine(RectTransform parent)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("ScanLine");
			val.transform.SetParent((Transform)(object)parent, false);
			_scanLineRT = val.AddComponent<RectTransform>();
			_scanLineRT.anchorMin = new Vector2(0f, 1f);
			_scanLineRT.anchorMax = new Vector2(1f, 1f);
			_scanLineRT.sizeDelta = new Vector2(0f, 2f);
			_scanLineRT.anchoredPosition = new Vector2(0f, 0f);
			Image obj = val.AddComponent<Image>();
			((Graphic)obj).color = new Color(1f, 1f, 1f, 0.15f);
			((Graphic)obj).raycastTarget = false;
		}

		private void UpdateScanningLine()
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)_scanLineRT == (Object)null))
			{
				float num = 200f;
				float num2 = ((Screen.height > 0) ? ((float)Screen.height) : 600f);
				if ((Object)(object)activationCanvasTMP != (Object)null)
				{
					Rect rect = ((Component)activationCanvasTMP).GetComponent<RectTransform>().rect;
					num2 = (rect).height;
				}
				Vector2 anchoredPosition = _scanLineRT.anchoredPosition;
				anchoredPosition.y -= num * Time.deltaTime;
				if (anchoredPosition.y < 0f - num2)
				{
					anchoredPosition.y = 0f;
				}
				_scanLineRT.anchoredPosition = anchoredPosition;
			}
		}

		private void FinishBootSequence()
		{
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			_bootSequenceComplete = true;
			if ((Object)(object)_bootConsoleGO != (Object)null)
			{
				_bootConsoleGO.SetActive(false);
			}
			if ((Object)(object)activationPanelGO != (Object)null)
			{
				activationPanelGO.SetActive(true);
			}
			if ((Object)(object)_discordAvatarImage != (Object)null && (Object)(object)_discordAvatarImage.texture != (Object)null && (Object)(object)_activationAvatarContainer != (Object)null)
			{
				((Component)_activationAvatarContainer).gameObject.SetActive(true);
				((Transform)_activationAvatarContainer).localScale = Vector3.one;
				_activationAvatarAnimStart = -1f;
			}
			_revealStartTime = Time.realtimeSinceStartup;
			_revealComplete = false;
			_panelScaleCurrent = 0.92f;
			_currentAlpha = 0f;
			if (Object.op_Implicit((Object)(object)_panelCanvasGroup))
			{
				_panelCanvasGroup.alpha = 0f;
			}
		}

		private void UpdateBootSequence()
		{
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0330: Unknown result type (might be due to invalid IL or missing references)
			//IL_03da: Unknown result type (might be due to invalid IL or missing references)
			if (_bootSequenceComplete || (Object)(object)_bootConsoleText == (Object)null)
			{
				return;
			}
			float num = Time.realtimeSinceStartup - _bootStartTime;
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if ((Object)(object)_bootConsoleCanvasGroup != (Object)null)
			{
				_bootConsoleCanvasGroup.alpha = ModMenuCrew.Easing.Easing.CrtFlicker(realtimeSinceStartup);
			}
			if ((Object)(object)_bootScanlineOverlay != (Object)null)
			{
				RectTransform component = ((Component)_bootScanlineOverlay).GetComponent<RectTransform>();
				Transform parent = ((Component)component).transform.parent;
				RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
				if ((Object)(object)val != (Object)null)
				{
					Rect rect = val.rect;
					float height = (rect).height;
					if (height > 0f)
					{
						float num2 = realtimeSinceStartup * 90f % height;
						component.anchoredPosition = new Vector2(0f, num2);
					}
				}
			}
			_bootCursorBlinkTimer += Time.deltaTime;
			string text = ((_bootCursorBlinkTimer % 0.6f < 0.35f) ? "<color=#00FFD0>_</color>" : " ");
			float num3 = ((_bootLines.Length != 0) ? ((float)_bootLineIndex / (float)_bootLines.Length) : 0f);
			if ((Object)(object)_bootProgressFill != (Object)null)
			{
				RectTransform component2 = ((Component)_bootProgressFill).GetComponent<RectTransform>();
				float x = component2.anchorMax.x;
				float target = Mathf.Clamp01(num3);
				float num4 = ModMenuCrew.Easing.Easing.Damp(x, target, 8f, Time.deltaTime);
				component2.anchorMax = new Vector2(num4, 1f);
				float num5 = ModMenuCrew.Easing.Easing.SinePulse(realtimeSinceStartup, 4f, 0.15f, 0.85f);
				((Graphic)_bootProgressFill).color = new Color(0.2f * num5, 1f * num5, 0.85f * num5, 0.9f);
			}
			if ((Object)(object)_bootPercentLabel != (Object)null)
			{
				((TMP_Text)_bootPercentLabel).text = Mathf.RoundToInt(num3 * 100f) + "%";
			}
			if ((Object)(object)_bootPhaseLabel != (Object)null)
			{
				string text2 = ((_bootLineIndex < 7) ? "INITIALIZING" : ((_bootLineIndex < 11) ? "SCANNING" : ((_bootLineIndex < 16) ? "HOOKING" : ((_bootLineIndex >= 20) ? "FINALIZING" : "BYPASSING EAC"))));
				string[] array = new string[4] { "|", "/", "-", "\\" };
				if (realtimeSinceStartup - _bootLastSpinnerTime > 0.1f)
				{
					_bootSpinnerIdx = (_bootSpinnerIdx + 1) % array.Length;
					_bootLastSpinnerTime = realtimeSinceStartup;
				}
				if (num3 < 1f)
				{
					((TMP_Text)_bootPhaseLabel).text = "<color=#33FFFF>[" + array[_bootSpinnerIdx] + "]</color> " + text2;
				}
				else
				{
					((TMP_Text)_bootPhaseLabel).text = "<color=#00FF55>[*]</color> COMPLETE";
				}
			}
			if (num > 0.8f && (Object)(object)_bootSkipHintText != (Object)null)
			{
				_bootSkipAllowed = true;
				_bootSkipHintAlpha = ModMenuCrew.Easing.Easing.MoveTowards(_bootSkipHintAlpha, 1f, Time.deltaTime * 3f);
				((Graphic)_bootSkipHintText).color = new Color(0.4f, 0.4f, 0.5f, _bootSkipHintAlpha * 0.6f);
			}
			if (_bootSkipAllowed && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
			{
				while (_bootLineIndex < _bootLines.Length)
				{
					_bootDisplayedText = _bootDisplayedText + _bootLines[_bootLineIndex] + "\n";
					_bootLineIndex++;
				}
				if ((Object)(object)_bootConsoleText != (Object)null)
				{
					((TMP_Text)_bootConsoleText).text = _bootDisplayedText;
				}
				if ((Object)(object)_bootProgressFill != (Object)null)
				{
					((Component)_bootProgressFill).GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
				}
				if ((Object)(object)_bootPercentLabel != (Object)null)
				{
					((TMP_Text)_bootPercentLabel).text = "100%";
				}
				if ((Object)(object)_bootPhaseLabel != (Object)null)
				{
					((TMP_Text)_bootPhaseLabel).text = "<color=#00FF55>[*]</color> COMPLETE";
				}
				if ((Object)(object)_bootConsoleCanvasGroup != (Object)null)
				{
					_bootConsoleCanvasGroup.alpha = 1f;
				}
				FinishBootSequence();
				return;
			}
			while (realtimeSinceStartup - _lastBootCharTime >= _bootCharDelay)
			{
				_lastBootCharTime += _bootCharDelay;
				if (_bootLineIndex < _bootLines.Length)
				{
					int num6 = _bootPlainLineLengths[_bootLineIndex];
					if (_bootCharIndex < num6)
					{
						_bootCharIndex++;
						if (_bootCharIndex >= num6)
						{
							_bootDisplayedText = _bootDisplayedText + _bootLines[_bootLineIndex] + "\n";
							((TMP_Text)_bootConsoleText).text = _bootDisplayedText + text;
							_bootLineIndex++;
							_bootCharIndex = 0;
						}
					}
					continue;
				}
				FinishBootSequence();
				break;
			}
			if (!_bootSequenceComplete && (Object)(object)_bootConsoleText != (Object)null && _bootLineIndex < _bootLines.Length && (!Object.ReferenceEquals(Object.op_Implicit(_lastBootCursor), Object.op_Implicit(text)) || !Object.ReferenceEquals(Object.op_Implicit(_lastBootDisplayedText), Object.op_Implicit(_bootDisplayedText))))
			{
				((TMP_Text)_bootConsoleText).text = _bootDisplayedText + text;
				_lastBootCursor = text;
				_lastBootDisplayedText = _bootDisplayedText;
			}
		}

		private void UpdateFloatingCrewmates()
		{
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_bootOverlay == (Object)null)
			{
				return;
			}
			if (_floatingCrewmates.Count == 0)
			{
				for (int i = 0; i < 6; i++)
				{
					SpawnFloatingCrewmate(_bootOverlay.transform, spawnOnScreen: true);
				}
				_spawnTimer = 1f;
				return;
			}
			_spawnTimer -= Time.deltaTime;
			if (_floatingCrewmates.Count < 12 && _spawnTimer <= 0f)
			{
				SpawnFloatingCrewmate(_bootOverlay.transform);
				_spawnTimer = Random.Range(1f, 2.5f);
			}
			for (int num = _floatingCrewmates.Count - 1; num >= 0; num--)
			{
				FloatingCrewmate floatingCrewmate = _floatingCrewmates[num];
				if ((Object)(object)floatingCrewmate.Root == (Object)null)
				{
					_floatingCrewmates.RemoveAt(num);
				}
				else
				{
					float deltaTime = Time.deltaTime;
					Vector3 val = Vector2.op_Implicit(floatingCrewmate.Rect.anchoredPosition);
					val += Vector2.op_Implicit(floatingCrewmate.Velocity * deltaTime);
					float num2 = ModMenuCrew.Easing.Easing.Bob(Time.time, floatingCrewmate.BobFrequency, floatingCrewmate.BobAmplitude, floatingCrewmate.TimeOffset) * deltaTime;
					val.y += num2;
					floatingCrewmate.Rect.anchoredPosition = Vector2.op_Implicit(val);
					((Transform)floatingCrewmate.Rect).Rotate(0f, 0f, floatingCrewmate.RotSpeed * deltaTime);
					if (val.x > 1300f || val.x < -1300f || val.y > 800f || val.y < -800f)
					{
						try
						{
							floatingCrewmate.Root.SetActive(false);
						}
						catch
						{
						}
						_floatingCrewmatePool.Push(floatingCrewmate);
						_floatingCrewmates.RemoveAt(num);
					}
				}
			}
		}

		private void SpawnFloatingCrewmate(Transform parent, bool spawnOnScreen = false)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02be: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_032f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0331: Unknown result type (might be due to invalid IL or missing references)
			//IL_0333: Unknown result type (might be due to invalid IL or missing references)
			//IL_033a: Unknown result type (might be due to invalid IL or missing references)
			//IL_033f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0344: Unknown result type (might be due to invalid IL or missing references)
			//IL_035d: Unknown result type (might be due to invalid IL or missing references)
			//IL_035f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0361: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0413: Unknown result type (might be due to invalid IL or missing references)
			//IL_0418: Unknown result type (might be due to invalid IL or missing references)
			//IL_041a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0421: Unknown result type (might be due to invalid IL or missing references)
			//IL_0426: Unknown result type (might be due to invalid IL or missing references)
			//IL_042b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0454: Unknown result type (might be due to invalid IL or missing references)
			//IL_0459: Unknown result type (might be due to invalid IL or missing references)
			//IL_045b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0474: Unknown result type (might be due to invalid IL or missing references)
			//IL_0476: Unknown result type (might be due to invalid IL or missing references)
			//IL_0478: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_050c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0519: Unknown result type (might be due to invalid IL or missing references)
			//IL_0526: Unknown result type (might be due to invalid IL or missing references)
			//IL_052b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0538: Unknown result type (might be due to invalid IL or missing references)
			//IL_0545: Unknown result type (might be due to invalid IL or missing references)
			//IL_054a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0585: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			while (_floatingCrewmatePool.Count > 0)
			{
				FloatingCrewmate floatingCrewmate = _floatingCrewmatePool.Pop();
				if (floatingCrewmate != null && !((Object)(object)floatingCrewmate.Root == (Object)null))
				{
					ReuseFloatingCrewmate(floatingCrewmate, spawnOnScreen);
					return;
				}
			}
			GameObject val = new GameObject("FloatingCrewmate");
			val.transform.SetParent(parent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			float num;
			float num2;
			Vector2 val4;
			if (spawnOnScreen)
			{
				num = Random.Range(-800f, 800f);
				num2 = Random.Range(-400f, 400f);
				Vector2 val3 = new Vector2(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f));
				val4 = (val3).normalized;
			}
			else
			{
				bool num3 = Random.value > 0.5f;
				num = (num3 ? (-1000) : 1000);
				num2 = Random.Range(-450, 450);
				val4 = (num3 ? Vector2.right : Vector2.left);
				val4 += new Vector2(0f, Random.Range(-0.3f, 0.3f));
				(val4).Normalize();
			}
			val2.anchoredPosition = new Vector2(num, num2);
			float num4 = Random.Range(0.6f, 1.3f);
			((Transform)val2).localScale = Vector3.one * num4;
			float num5 = Random.Range(50f, 120f) * (num4 * 0.7f + 0.3f);
			FloatingCrewmate item = new FloatingCrewmate
			{
				Root = val,
				Rect = val2,
				Velocity = val4 * num5,
				RotSpeed = Random.Range(-25f, 25f),
				BobFrequency = Random.Range(0.8f, 2.5f),
				BobAmplitude = Random.Range(15f, 40f),
				TimeOffset = Random.Range(0f, 10f)
			};
			Color randomCrewmateColor = GetRandomCrewmateColor();
			new Color(randomCrewmateColor.r * 0.7f, randomCrewmateColor.g * 0.7f, randomCrewmateColor.b * 0.7f);
			Color col2 = default(Color);
			(col2)._002Ector(0f, 0f, 0f, 1f);
			Color col3 = default(Color);
			(col3)._002Ector(0.55f, 0.82f, 0.96f);
			Color col4 = default(Color);
			(col4)._002Ector(1f, 1f, 1f, 0.9f);
			new Color(0.25f, 0.45f, 0.55f);
			float num6 = 65f;
			float num7 = 60f;
			float num8 = 6f;
			Vector2 val5 = default(Vector2);
			(val5)._002Ector(num6 * 0.35f, num7 * 0.7f);
			Vector2 pos2 = default(Vector2);
			(pos2)._002Ector((0f - num6) * 0.45f, (0f - num7) * 0.05f);
			CreatePart("BackpackOut", pos2, val5 + Vector2.one * num8, col2, val.transform, 1.5f);
			CreatePart("Backpack", pos2, val5, randomCrewmateColor, val.transform, 1.5f);
			Vector2 val6 = default(Vector2);
			(val6)._002Ector(num6 * 0.85f, num7 * 0.95f);
			Vector2 pos3 = default(Vector2);
			(pos3)._002Ector(0f, 0f);
			CreatePart("BodyOut", pos3, val6 + Vector2.one * num8, col2, val.transform, 0.5f);
			CreatePart("Body", pos3, val6, randomCrewmateColor, val.transform, 0.5f);
			Vector2 val7 = default(Vector2);
			(val7)._002Ector(num6 * 0.3f, num7 * 0.4f);
			float num9 = (0f - num7) * 0.5f;
			CreatePart("LegLOut", new Vector2((0f - num6) * 0.25f, num9), val7 + Vector2.one * num8, col2, val.transform, 0.8f);
			CreatePart("LegL", new Vector2((0f - num6) * 0.25f, num9 + 2f), val7, randomCrewmateColor, val.transform, 0.8f);
			CreatePart("LegROut", new Vector2(num6 * 0.25f, num9), val7 + Vector2.one * num8, col2, val.transform, 0.8f);
			CreatePart("LegR", new Vector2(num6 * 0.25f, num9 + 2f), val7, randomCrewmateColor, val.transform, 0.8f);
			GameObject val8 = CreatePart("BodyCover", pos3, val6, randomCrewmateColor, val.transform, 0.5f);
			Vector2 val9 = default(Vector2);
			(val9)._002Ector(num6 * 0.65f, num7 * 0.45f);
			Vector2 pos4 = default(Vector2);
			(pos4)._002Ector(num6 * 0.35f, num7 * 0.1f);
			CreatePart("VisorOut", pos4, val9 + Vector2.one * num8, col2, val.transform, 0.6f);
			GameObject val10 = CreatePart("Visor", pos4, val9, col3, val.transform, 0.6f);
			CreatePart("VisorShine", new Vector2(val9.x * 0.25f, val9.y * 0.2f), new Vector2(val9.x * 0.4f, val9.y * 0.2f), col4, val10.transform);
			_floatingCrewmates.Add(item);
			if (Random.value > 0.3f)
			{
				AddRandomHat(val.transform, val8.transform, randomCrewmateColor);
			}
			GameObject CreatePart(string name, Vector2 pos, Vector2 size, Color col, Transform p, float pps = 1f)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_0050: Expected O, but got Unknown
				GameObject val11 = new GameObject(name);
				val11.transform.SetParent(p, false);
				Image obj = val11.AddComponent<Image>();
				obj.sprite = GetCircleSprite();
				obj.type = (Type)1;
				obj.pixelsPerUnitMultiplier = pps;
				((Graphic)obj).color = col;
				RectTransform component = val11.GetComponent<RectTransform>();
				component.anchoredPosition = pos;
				component.sizeDelta = size;
				return val11;
			}
		}

		private void ReuseFloatingCrewmate(FloatingCrewmate crew, bool spawnOnScreen)
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			float num;
			float num2;
			Vector2 val2;
			if (spawnOnScreen)
			{
				num = Random.Range(-800f, 800f);
				num2 = Random.Range(-400f, 400f);
				Vector2 val = new Vector2(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f));
				val2 = (val).normalized;
			}
			else
			{
				bool num3 = Random.value > 0.5f;
				num = (num3 ? (-1000) : 1000);
				num2 = Random.Range(-450, 450);
				val2 = (num3 ? Vector2.right : Vector2.left);
				val2 += new Vector2(0f, Random.Range(-0.3f, 0.3f));
				(val2).Normalize();
			}
			crew.Rect.anchoredPosition = new Vector2(num, num2);
			float num4 = Random.Range(0.6f, 1.3f);
			((Transform)crew.Rect).localScale = Vector3.one * num4;
			float num5 = Random.Range(50f, 120f) * (num4 * 0.7f + 0.3f);
			crew.Velocity = val2 * num5;
			crew.RotSpeed = Random.Range(-25f, 25f);
			crew.BobFrequency = Random.Range(0.8f, 2.5f);
			crew.BobAmplitude = Random.Range(15f, 40f);
			crew.TimeOffset = Random.Range(0f, 10f);
			crew.Root.SetActive(true);
			_floatingCrewmates.Add(crew);
		}

		private Sprite GetCircleSprite()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_cachedCircleSprite != (Object)null)
			{
				return _cachedCircleSprite;
			}
			int num = 64;
			Texture2D val = new Texture2D(num, num, (TextureFormat)4, false)
			{
				wrapMode = (TextureWrapMode)1,
				filterMode = (FilterMode)1,
				hideFlags = (HideFlags)61
			};
			Color[] array = (Color[])(object)new Color[num * num];
			float num2 = (float)num / 2f;
			Vector2 val2 = default(Vector2);
			(val2)._002Ector(num2, num2);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num; j++)
				{
					float num3 = Vector2.Distance(new Vector2((float)j, (float)i), val2) - num2;
					float num4 = 1f - Mathf.Clamp01(num3 + 1f);
					array[i * num + j] = new Color(1f, 1f, 1f, num4);
				}
			}
			val.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
			val.Apply();
			float num5 = (float)num * 0.45f;
			Vector4 val3 = default(Vector4);
			val3..ctor(num5, num5, num5, num5);
			_cachedCircleSprite = Sprite.Create(val, new Rect(0f, 0f, (float)num, (float)num), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0, val3);
			return _cachedCircleSprite;
		}

		private void AddRandomHat(Transform root, Transform body, Color bodyColor)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_0314: Unknown result type (might be due to invalid IL or missing references)
			//IL_0319: Unknown result type (might be due to invalid IL or missing references)
			//IL_0348: Unknown result type (might be due to invalid IL or missing references)
			//IL_0357: Unknown result type (might be due to invalid IL or missing references)
			//IL_035c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0375: Unknown result type (might be due to invalid IL or missing references)
			//IL_0384: Unknown result type (might be due to invalid IL or missing references)
			//IL_0389: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
			switch (Random.Range(0, 6))
			{
			case 0:
				CreateImage(root, "Balloon", new Vector2(15f, 70f), new Vector2(24f, 28f), new Color(0.9f, 0.2f, 0.2f));
				CreateImage(root, "String", new Vector2(15f, 45f), new Vector2(2f, 25f), new Color(1f, 1f, 1f, 0.5f));
				break;
			case 1:
			{
				Color color4 = default(Color);
				(color4)._002Ector(1f, 0.85f, 0.1f);
				CreateImage(root, "Halo", new Vector2(12f, 50f), new Vector2(30f, 4f), color4);
				CreateImage(root, "HaloL", new Vector2(-2f, 50f), new Vector2(4f, 4f), color4);
				CreateImage(root, "HaloR", new Vector2(28f, 50f), new Vector2(4f, 4f), color4);
				break;
			}
			case 2:
			{
				Color color3 = default(Color);
				(color3)._002Ector(1f, 0.95f, 0.4f);
				CreateImage(body, "Dum", new Vector2(10f, 25f), new Vector2(20f, 15f), color3);
				break;
			}
			case 3:
				CreateImage(root, "CherryR", new Vector2(20f, 40f), new Vector2(12f, 12f), new Color(0.9f, 0.1f, 0.1f));
				CreateImage(root, "CherryL", new Vector2(8f, 38f), new Vector2(12f, 12f), new Color(0.8f, 0.1f, 0.1f));
				CreateImage(root, "Stem", new Vector2(15f, 48f), new Vector2(4f, 10f), new Color(0.1f, 0.7f, 0.1f));
				break;
			case 4:
			{
				Color color2 = default(Color);
				(color2)._002Ector(1f, 0.8f, 0f);
				GameObject val = CreateImage(root, "CrownBase", new Vector2(10f, 40f), new Vector2(30f, 8f), color2);
				CreateImage(val.transform, "Spike1", new Vector2(-10f, 8f), new Vector2(8f, 8f), color2);
				CreateImage(val.transform, "Spike2", new Vector2(0f, 10f), new Vector2(8f, 8f), color2);
				CreateImage(val.transform, "Spike3", new Vector2(10f, 8f), new Vector2(8f, 8f), color2);
				break;
			}
			case 5:
			{
				Color color = default(Color);
				(color)._002Ector(0.2f, 0.8f, 0.2f);
				CreateImage(root, "Stem", new Vector2(12f, 40f), new Vector2(4f, 12f), color);
				CreateImage(root, "LeafL", new Vector2(4f, 50f), new Vector2(10f, 6f), color);
				CreateImage(root, "LeafR", new Vector2(18f, 52f), new Vector2(10f, 6f), color);
				break;
			}
			}
		}

		private Color GetRandomCrewmateColor()
		{
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			if (_availableCrewmateColors.Count == 0)
			{
				Color[] collection = (Color[])(object)new Color[12]
				{
					new Color(0.78f, 0.08f, 0.08f),
					new Color(0.07f, 0.18f, 0.85f),
					new Color(0.08f, 0.51f, 0.15f),
					new Color(0.93f, 0.35f, 0.73f),
					new Color(0.91f, 0.53f, 0.08f),
					new Color(0.97f, 0.96f, 0.33f),
					new Color(0.3f, 0.3f, 0.3f),
					new Color(0.92f, 0.92f, 0.92f),
					new Color(0.5f, 0.2f, 0.8f),
					new Color(0.55f, 0.35f, 0.15f),
					new Color(0.22f, 0.95f, 0.88f),
					new Color(0.35f, 0.95f, 0.3f)
				};
				_availableCrewmateColors.AddRange(collection);
				for (int num = _availableCrewmateColors.Count - 1; num > 0; num--)
				{
					int index = Random.Range(0, num + 1);
					Color value = _availableCrewmateColors[num];
					_availableCrewmateColors[num] = _availableCrewmateColors[index];
					_availableCrewmateColors[index] = value;
				}
			}
			Color result = _availableCrewmateColors[_availableCrewmateColors.Count - 1];
			_availableCrewmateColors.RemoveAt(_availableCrewmateColors.Count - 1);
			return result;
		}

		private GameObject CreateImage(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Expected O, but got Unknown
			GameObject val = new GameObject(name);
			val.transform.SetParent(parent, false);
			Image obj = val.AddComponent<Image>();
			obj.sprite = GetCircleSprite();
			((Graphic)obj).color = color;
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchoredPosition = pos;
			component.sizeDelta = size;
			return val;
		}

		private void ManageActivationUIVisibility()
		{
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02da: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0306: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0295: Unknown result type (might be due to invalid IL or missing references)
			//IL_029a: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)activationCanvasTMP == (Object)null)
				{
					return;
				}
				if ((Object)(object)_panelCanvasGroup != (Object)null && !isModGloballyActivated && ((Component)activationCanvasTMP).gameObject.activeSelf && _panelCanvasGroup.alpha < 0.01f)
				{
					_panelCanvasGroup.alpha = 1f;
					_panelCanvasGroup.interactable = true;
					_panelCanvasGroup.blocksRaycasts = true;
					_currentAlpha = 1f;
				}
				TickValidationTimeoutHint();
				if ((Object)(object)statusMessageTextTMP != (Object)null)
				{
					if ((Object)(object)_statusPillRT != (Object)null)
					{
						string text = (currentActivationStatusMessage ?? string.Empty).ToLowerInvariant();
						bool flag = !string.IsNullOrEmpty(currentActivationStatusMessage) && (text.Contains("error") || text.Contains("invalid") || text.Contains("failed") || text.Contains("validating") || text.Contains("wait") || text.Contains("expired") || text.Contains("premium only") || text.Contains("login with") || text.Contains("login pending") || text.Contains("please") || text.Contains("opening") || text.Contains("browser") || text.Contains("timeout") || text.Contains("cancel") || text.Contains("server slow"));
						if (((Component)_statusPillRT).gameObject.activeSelf != flag)
						{
							((Component)_statusPillRT).gameObject.SetActive(flag);
						}
					}
					if (!string.Equals(_lastStatusMessage, currentActivationStatusMessage, StringComparison.Ordinal))
					{
						string text2 = "";
						Color val = Color.white;
						string text3 = currentActivationStatusMessage.ToLowerInvariant();
						if (text3.Contains("error") || text3.Contains("invalid") || text3.Contains("failed") || text3.Contains("timeout"))
						{
							val = GuiStyles.Theme.ErrorSoft;
							text2 = "[!] ";
						}
						else if (text3.Contains("success") || text3.Contains("validated") || text3.Contains("activated"))
						{
							val = GuiStyles.Theme.SuccessSoft;
							text2 = "[✓] ";
						}
						else if (text3.Contains("wait") || text3.Contains("validating"))
						{
							val = GuiStyles.Theme.StatusGold;
							text2 = "[...] ";
						}
						((TMP_Text)statusMessageTextTMP).text = text2 + currentActivationStatusMessage;
						((Graphic)statusMessageTextTMP).color = val;
						if ((Object)(object)_statusPillBg != (Object)null)
						{
							((Graphic)_statusPillBg).color = new Color(val.r * 0.1f, val.g * 0.1f, val.b * 0.1f, 0.8f);
						}
						_lastStatusMessage = currentActivationStatusMessage;
					}
				}
				if ((Object)(object)validateButtonTMP != (Object)null)
				{
					string text4 = (((Object)(object)apiKeyInputFieldTMP != (Object)null) ? apiKeyInputFieldTMP.text : null);
					bool flag2 = !isValidatingNow && !string.IsNullOrWhiteSpace(text4) && DiscordAuthManager.IsLoggedIn;
					if (((Selectable)validateButtonTMP).interactable != flag2)
					{
						((Selectable)validateButtonTMP).interactable = flag2;
					}
					if ((Object)(object)validateButtonTextTMP != (Object)null && !isValidatingNow)
					{
						string text5 = "[ ACTIVATE ]";
						if (!string.Equals(((TMP_Text)validateButtonTextTMP).text, text5, StringComparison.Ordinal))
						{
							((TMP_Text)validateButtonTextTMP).text = text5;
							_loadingDotsTimer = 0f;
							_loadingDotsCount = 0;
						}
					}
					_lastValidatingState = isValidatingNow;
					_lastInputText = text4;
				}
				int num = 1;
				if (DiscordAuthManager.IsLoggedIn || DiscordAuthManager.HasDiscordProfile)
				{
					num = 2;
				}
				if (num >= 2 && (Object)(object)apiKeyInputFieldTMP != (Object)null && !string.IsNullOrWhiteSpace(apiKeyInputFieldTMP.text))
				{
					num = 3;
				}
				if (_currentStep != num)
				{
					UpdateStepIndicator(num);
				}
				if (_shouldAutoFocus && (Object)(object)apiKeyInputFieldTMP != (Object)null && ((Component)activationCanvasTMP).gameObject.activeInHierarchy && (Object)(object)EventSystem.current != (Object)null)
				{
					EventSystem.current.SetSelectedGameObject(((Component)apiKeyInputFieldTMP).gameObject);
					apiKeyInputFieldTMP.ActivateInputField();
					_shouldAutoFocus = false;
				}
			}
			catch
			{
			}
		}

		private void UpdateActivationUIAnimations()
		{
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_045b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0460: Unknown result type (might be due to invalid IL or missing references)
			//IL_0468: Unknown result type (might be due to invalid IL or missing references)
			//IL_046f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0476: Unknown result type (might be due to invalid IL or missing references)
			//IL_047f: Unknown result type (might be due to invalid IL or missing references)
			//IL_040a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0403: Unknown result type (might be due to invalid IL or missing references)
			//IL_040f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0417: Unknown result type (might be due to invalid IL or missing references)
			//IL_041e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0425: Unknown result type (might be due to invalid IL or missing references)
			//IL_0301: Unknown result type (might be due to invalid IL or missing references)
			//IL_031a: Unknown result type (might be due to invalid IL or missing references)
			//IL_031f: Unknown result type (might be due to invalid IL or missing references)
			//IL_032f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02df: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_051f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0535: Unknown result type (might be due to invalid IL or missing references)
			//IL_053a: Unknown result type (might be due to invalid IL or missing references)
			//IL_053f: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_04de: Unknown result type (might be due to invalid IL or missing references)
			//IL_0555: Unknown result type (might be due to invalid IL or missing references)
			//IL_043d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0570: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0639: Unknown result type (might be due to invalid IL or missing references)
			//IL_063e: Unknown result type (might be due to invalid IL or missing references)
			//IL_065a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0856: Unknown result type (might be due to invalid IL or missing references)
			//IL_07fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_067b: Unknown result type (might be due to invalid IL or missing references)
			//IL_069b: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_0750: Unknown result type (might be due to invalid IL or missing references)
			//IL_0757: Unknown result type (might be due to invalid IL or missing references)
			//IL_075e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0776: Unknown result type (might be due to invalid IL or missing references)
			//IL_0717: Unknown result type (might be due to invalid IL or missing references)
			//IL_071e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0725: Unknown result type (might be due to invalid IL or missing references)
			//IL_073d: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)activationCanvasTMP == (Object)null || !((Component)activationCanvasTMP).gameObject.activeSelf)
				{
					return;
				}
				float deltaTime = Time.deltaTime;
				UpdateBootSequence();
				UpdateFloatingCrewmates();
				UpdateGlitchText();
				UpdateScanningLine();
				if ((Object)(object)_panelCanvasGroup != (Object)null && Mathf.Abs(_currentAlpha - _targetAlpha) > 0.01f)
				{
					_currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, deltaTime * 8f);
					_panelCanvasGroup.alpha = _currentAlpha;
				}
				if ((Object)(object)activationPanelGO != (Object)null && _panelScaleCurrent < 0.999f)
				{
					_panelScaleCurrent = Mathf.Lerp(_panelScaleCurrent, 1f, deltaTime * 6f);
					activationPanelGO.transform.localScale = Vector3.one * _panelScaleCurrent;
				}
				else if ((Object)(object)activationPanelGO != (Object)null && _panelScaleCurrent < 1f)
				{
					_panelScaleCurrent = 1f;
					activationPanelGO.transform.localScale = Vector3.one;
				}
				if (_revealStartTime > 0f && !_revealComplete && _staggerElements.Count > 0)
				{
					float num = Time.realtimeSinceStartup - _revealStartTime;
					bool flag = true;
					for (int i = 0; i < _staggerElements.Count; i++)
					{
						CanvasGroup val = _staggerElements[i];
						if ((Object)(object)val == (Object)null)
						{
							continue;
						}
						float num2 = ((i < _staggerDelays.Length) ? _staggerDelays[i] : ((float)i * 0.08f));
						float num3 = num - num2;
						if (num3 < 0f)
						{
							flag = false;
							continue;
						}
						float num4 = Mathf.Clamp01(num3 / 0.25f);
						float alpha = 1f - (1f - num4) * (1f - num4);
						val.alpha = alpha;
						if (num4 < 1f)
						{
							flag = false;
						}
					}
					if (flag)
					{
						_revealComplete = true;
					}
				}
				if ((Object)(object)apiKeyInputFieldTMP != (Object)null && (Object)(object)_inputFieldOutline != (Object)null && (Object)(object)_inputFieldBgImage != (Object)null)
				{
					float num5 = (apiKeyInputFieldTMP.isFocused ? 1f : 0f);
					_inputGlowPhase = Mathf.MoveTowards(_inputGlowPhase, num5, deltaTime * 5f);
					if (_inputGlowPhase > 0.01f)
					{
						float num6 = ModMenuCrew.Easing.Easing.SinePulse(Time.time, 2.5f, 0.3f, 0.5f);
						float num7 = _inputGlowPhase * num6;
						((Shadow)_inputFieldOutline).effectColor = GuiStyles.Theme.Accent * new Color(1f, 1f, 1f, num7);
						((Graphic)_inputFieldBgImage).color = Color.Lerp(GuiStyles.Theme.InputFieldBg, GuiStyles.Theme.InputFieldBgFocused, _inputGlowPhase);
					}
					else
					{
						((Shadow)_inputFieldOutline).effectColor = GuiStyles.Theme.Accent * new Color(1f, 1f, 1f, 0f);
						((Graphic)_inputFieldBgImage).color = GuiStyles.Theme.InputFieldBg;
					}
				}
				if (isValidatingNow && (Object)(object)validateButtonTextTMP != (Object)null)
				{
					_loadingDotsTimer += deltaTime;
					if (_loadingDotsTimer > 0.4f)
					{
						_loadingDotsTimer = 0f;
						_loadingDotsCount = (_loadingDotsCount + 1) % 4;
						((TMP_Text)validateButtonTextTMP).text = _loadingDotsTexts[_loadingDotsCount];
					}
				}
				if (!(_currentAlpha > 0.9f))
				{
					return;
				}
				_pulseTimer += deltaTime * 3f;
				float num8 = 0.7f + Mathf.Sin(_pulseTimer) * 0.3f;
				if ((Object)(object)_validateBtnImage != (Object)null)
				{
					Color val2 = (Color)(isValidatingNow ? Color.gray : new Color(1f, 1f, 1f, 1f));
					((Graphic)_validateBtnImage).color = new Color(val2.r, val2.g, val2.b, isValidatingNow ? 1f : num8);
				}
				if ((Object)(object)_validateBtnOutline != (Object)null)
				{
					Color effectColor = ((Shadow)_validateBtnOutline).effectColor;
					((Shadow)_validateBtnOutline).effectColor = new Color(effectColor.r, effectColor.g, effectColor.b, num8);
				}
				if (_revealComplete && (Object)(object)activationPanelGO != (Object)null)
				{
					_breathePhase += deltaTime * 1.2f;
					float num9 = 1f + Mathf.Sin(_breathePhase) * 0.003f;
					activationPanelGO.transform.localScale = Vector3.one * num9;
				}
				if ((Object)(object)_accentTopLine != (Object)null || (Object)(object)_accentBottomLine != (Object)null)
				{
					float num10 = ModMenuCrew.Easing.Easing.SinePulse(Time.time, 2f, 0.3f, 0.5f);
					Color color = GuiStyles.Theme.Accent * new Color(1f, 1f, 1f, num10);
					if ((Object)(object)_accentTopLine != (Object)null)
					{
						((Graphic)_accentTopLine).color = color;
					}
					if ((Object)(object)_accentBottomLine != (Object)null)
					{
						((Graphic)_accentBottomLine).color = color;
					}
				}
				Image val3 = ((_currentStep == 1) ? _stepCircle1 : ((_currentStep == 2) ? _stepCircle2 : ((_currentStep == 3) ? _stepCircle3 : null)));
				if ((Object)(object)val3 != (Object)null)
				{
					float num11 = ModMenuCrew.Easing.Easing.SinePulse(Time.time, 2.5f, 0.4f, 0.6f);
					Color color2 = ((Graphic)val3).color;
					((Graphic)val3).color = new Color(color2.r, color2.g, color2.b, num11);
				}
				if (_stars != null)
				{
					for (int j = 0; j < _stars.Count; j++)
					{
						Star star = _stars[j];
						if (!((Object)(object)star.Rect != (Object)null))
						{
							continue;
						}
						Vector2 anchoredPosition = star.Rect.anchoredPosition;
						anchoredPosition.x -= star.Speed * deltaTime * 60f;
						if (anchoredPosition.x < -1000f)
						{
							anchoredPosition.x = 1000f;
						}
						star.Rect.anchoredPosition = anchoredPosition;
						if ((Object)(object)star.Image != (Object)null)
						{
							Color color3 = ((Graphic)star.Image).color;
							float num12 = (star.IsBright ? (0.1f + Mathf.Sin(Time.time * star.Speed * 2f + (float)j) * 0.08f) : (color3.a + Mathf.Sin(Time.time * star.Speed * 3f + (float)j * 0.5f) * 0.1f));
							if (star.IsBright)
							{
								((Graphic)star.Image).color = new Color(color3.r, color3.g, color3.b, Mathf.Clamp(num12, 0.05f, 0.25f));
							}
							else
							{
								((Graphic)star.Image).color = new Color(color3.r, color3.g, color3.b, Mathf.Clamp(num12, 0.15f, 0.85f));
							}
						}
					}
				}
				_glitchTimer += deltaTime;
				if (!_isGlitching && Random.Range(0, 500) == 0 && _glitchTimer > 2f)
				{
					_isGlitching = true;
					_glitchTimer = 0f;
					if ((Object)(object)_titleTextTMP != (Object)null)
					{
						((TMP_Text)_titleTextTMP).text = "IMPOSTOR DETECTED";
						((Graphic)_titleTextTMP).color = Color.red;
					}
				}
				else if (_isGlitching && _glitchTimer > 0.15f)
				{
					_isGlitching = false;
					_glitchTimer = 0f;
					if ((Object)(object)_titleTextTMP != (Object)null)
					{
						((TMP_Text)_titleTextTMP).text = _originalTitle;
						((Graphic)_titleTextTMP).color = Color.white;
					}
				}
			}
			catch
			{
			}
		}

		private void CreateStarfield(Transform parent)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Expected O, but got Unknown
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_0252: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_0289: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Unknown result type (might be due to invalid IL or missing references)
			//IL_0325: Unknown result type (might be due to invalid IL or missing references)
			//IL_032f: Unknown result type (might be due to invalid IL or missing references)
			_stars.Clear();
			Color[] array = (Color[])(object)new Color[6]
			{
				new Color(1f, 1f, 1f),
				new Color(1f, 0.95f, 0.8f),
				new Color(0.8f, 0.9f, 1f),
				new Color(1f, 0.8f, 0.8f),
				new Color(0.9f, 1f, 0.95f),
				new Color(1f, 0.6f, 0.4f)
			};
			for (int i = 0; i < 50; i++)
			{
				GameObject val = new GameObject("Star");
				val.transform.SetParent(parent, false);
				Image val2 = val.AddComponent<Image>();
				Color val3 = array[Random.Range(0, array.Length)];
				float num = Random.Range(0.2f, 0.7f);
				((Graphic)val2).color = new Color(val3.r, val3.g, val3.b, num);
				RectTransform component = val.GetComponent<RectTransform>();
				component.anchorMin = new Vector2(0.5f, 0.5f);
				component.anchorMax = new Vector2(0.5f, 0.5f);
				float num2 = Random.Range(1f, 4f);
				component.sizeDelta = new Vector2(num2, num2);
				component.anchoredPosition = new Vector2((float)Random.Range(-960, 960), (float)Random.Range(-540, 540));
				Star item = new Star
				{
					Rect = component,
					Speed = Random.Range(0.3f, 1.5f),
					Image = val2
				};
				_stars.Add(item);
			}
			for (int j = 0; j < 5; j++)
			{
				GameObject val4 = new GameObject("BrightStar");
				val4.transform.SetParent(parent, false);
				Image val5 = val4.AddComponent<Image>();
				Color val6 = array[Random.Range(0, 3)];
				((Graphic)val5).color = new Color(val6.r, val6.g, val6.b, 0.15f);
				RectTransform component2 = val4.GetComponent<RectTransform>();
				component2.anchorMin = new Vector2(0.5f, 0.5f);
				component2.anchorMax = new Vector2(0.5f, 0.5f);
				float num3 = Random.Range(15f, 30f);
				component2.sizeDelta = new Vector2(num3, num3);
				component2.anchoredPosition = new Vector2((float)Random.Range(-900, 900), (float)Random.Range(-500, 500));
				GameObject val7 = new GameObject("Core");
				val7.transform.SetParent(val4.transform, false);
				((Graphic)val7.AddComponent<Image>()).color = new Color(1f, 1f, 1f, 0.9f);
				RectTransform component3 = val7.GetComponent<RectTransform>();
				component3.anchorMin = new Vector2(0.3f, 0.3f);
				component3.anchorMax = new Vector2(0.7f, 0.7f);
				component3.sizeDelta = Vector2.zero;
				Star item2 = new Star
				{
					Rect = component2,
					Speed = Random.Range(1f, 3f),
					Image = val5,
					IsBright = true
				};
				_stars.Add(item2);
			}
		}

		private void CreateTechCorners(RectTransform parent)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			CreateTechCorners(parent, new Color(1f, 0.2f, 0.3f, 0.8f));
		}

		private void CreateTechCorners(RectTransform parent, Color color)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			float size = 30f;
			float thickness = 2f;
			CreateCorner(parent, new Vector2(0f, 1f), new Vector2(5f, -5f), size, thickness, color, 0f);
			CreateCorner(parent, new Vector2(1f, 1f), new Vector2(-5f, -5f), size, thickness, color, 90f);
			CreateCorner(parent, new Vector2(1f, 0f), new Vector2(-5f, 5f), size, thickness, color, 180f);
			CreateCorner(parent, new Vector2(0f, 0f), new Vector2(5f, 5f), size, thickness, color, 270f);
		}

		private void CreateCorner(RectTransform parent, Vector2 anchor, Vector2 offset, float size, float thickness, Color color, float rotation)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("Corner");
			val.transform.SetParent((Transform)(object)parent, false);
			RectTransform obj = val.AddComponent<RectTransform>();
			obj.anchorMin = anchor;
			obj.anchorMax = anchor;
			obj.pivot = anchor;
			obj.anchoredPosition = offset;
			obj.sizeDelta = new Vector2(size, size);
			((Transform)obj).localRotation = Quaternion.Euler(0f, 0f, 0f - rotation);
			GameObject val2 = new GameObject("V");
			val2.transform.SetParent(val.transform, false);
			((Graphic)val2.AddComponent<Image>()).color = color;
			RectTransform component = val2.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.zero;
			component.pivot = Vector2.zero;
			component.anchoredPosition = Vector2.zero;
			component.sizeDelta = new Vector2(thickness, size);
			GameObject val3 = new GameObject("H");
			val3.transform.SetParent(val.transform, false);
			((Graphic)val3.AddComponent<Image>()).color = color;
			RectTransform component2 = val3.GetComponent<RectTransform>();
			component2.anchorMin = Vector2.zero;
			component2.anchorMax = Vector2.zero;
			component2.pivot = Vector2.zero;
			component2.anchoredPosition = Vector2.zero;
			component2.sizeDelta = new Vector2(size, thickness);
		}

		private void StyleButton(Button btn, ref Sprite cachedSprite, Color topColor, Color bottomColor)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)btn == (Object)null)
			{
				return;
			}
			Image component = ((Component)btn).GetComponent<Image>();
			if (!((Object)(object)component != (Object)null))
			{
				return;
			}
			if ((Object)(object)cachedSprite == (Object)null)
			{
				Texture2D val = new Texture2D(1, 32, (TextureFormat)4, false)
				{
					wrapMode = (TextureWrapMode)1,
					filterMode = (FilterMode)1,
					hideFlags = (HideFlags)61
				};
				Color32[] array = (Color32[])(object)new Color32[32];
				for (int i = 0; i < 32; i++)
				{
					float num = (float)i / 31f;
					array[i] = Color32.op_Implicit(Color.Lerp(topColor, bottomColor, num));
				}
				val.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
				val.Apply();
				cachedSprite = Sprite.Create(val, new Rect(0f, 0f, 1f, 32f), Vector2.one * 0.5f);
			}
			component.sprite = cachedSprite;
			Outline val2 = ((Component)btn).gameObject.GetComponent<Outline>();
			if ((Object)(object)val2 == (Object)null)
			{
				val2 = ((Component)btn).gameObject.AddComponent<Outline>();
			}
			((Shadow)val2).effectColor = new Color(topColor.r * 1.5f, topColor.g * 1.5f, topColor.b * 1.5f, 0.8f);
			((Shadow)val2).effectDistance = new Vector2(1f, -1f);
		}

		private static PopupTheme GetPopupTheme(bool isPremium)
		{
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			if (isPremium)
			{
				Color gold = GuiStyles.Theme.Gold;
				Color accentSoft = default(Color);
				(accentSoft)._002Ector(1f, 0.92f, 0.4f, 1f);
				return new PopupTheme(gold, accentSoft, new Color(0.12f, 0.09f, 0.02f, 1f), new Color(1f, 0.84f, 0.2f, 1f), new Color(0.75f, 0.52f, 0.05f, 1f), new Color(0.18f, 0.18f, 0.24f, 1f), new Color(0.1f, 0.1f, 0.14f, 1f));
			}
			return new PopupTheme(GuiStyles.Theme.Visor, new Color(0.2f, 1f, 1f, 1f), new Color(0.02f, 0.06f, 0.08f, 1f), new Color(0f, 0.6f, 0.6f, 1f), new Color(0f, 0.3f, 0.3f, 1f), new Color(0.18f, 0.18f, 0.24f, 1f), new Color(0.1f, 0.1f, 0.14f, 1f));
		}

		private void EnsureSuccessCircleSprite()
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_successCircleSprite != (Object)null)
			{
				return;
			}
			if ((Object)(object)_successCircleTexture != (Object)null)
			{
				try
				{
					Object.Destroy((Object)(object)_successCircleTexture);
				}
				catch
				{
				}
				_successCircleTexture = null;
			}
			_successCircleTexture = new Texture2D(64, 64, (TextureFormat)4, false)
			{
				wrapMode = (TextureWrapMode)1,
				filterMode = (FilterMode)1,
				hideFlags = (HideFlags)61
			};
			Color32[] array = (Color32[])(object)new Color32[4096];
			for (int i = 0; i < 64; i++)
			{
				for (int j = 0; j < 64; j++)
				{
					float num = (float)j - 31.5f;
					float num2 = (float)i - 31.5f;
					float num3 = Mathf.Sqrt(num * num + num2 * num2);
					byte b = (byte)(Mathf.Clamp01(1f - (num3 - 10f) / 20f) * 255f);
					array[i * 64 + j] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, b);
				}
			}
			_successCircleTexture.SetPixels32(Il2CppStructArray<Color32>.op_Implicit(array));
			_successCircleTexture.Apply();
			_successCircleSprite = Sprite.Create(_successCircleTexture, new Rect(0f, 0f, 64f, 64f), Vector2.one * 0.5f, 100f, 0u, (SpriteMeshType)0);
		}

		private void ShowActivationSuccessPopup(string username, bool isPremium, string keyDisplay, string timeDisplay)
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Expected O, but got Unknown
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Expected O, but got Unknown
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Expected O, but got Unknown
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0300: Unknown result type (might be due to invalid IL or missing references)
			//IL_030b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0316: Unknown result type (might be due to invalid IL or missing references)
			//IL_0325: Unknown result type (might be due to invalid IL or missing references)
			//IL_033f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0354: Unknown result type (might be due to invalid IL or missing references)
			//IL_0369: Unknown result type (might be due to invalid IL or missing references)
			//IL_037e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0392: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0411: Unknown result type (might be due to invalid IL or missing references)
			//IL_0426: Unknown result type (might be due to invalid IL or missing references)
			//IL_043b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0450: Unknown result type (might be due to invalid IL or missing references)
			//IL_0464: Unknown result type (might be due to invalid IL or missing references)
			//IL_0473: Unknown result type (might be due to invalid IL or missing references)
			//IL_0478: Unknown result type (might be due to invalid IL or missing references)
			//IL_048b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0492: Unknown result type (might be due to invalid IL or missing references)
			//IL_049d: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0510: Unknown result type (might be due to invalid IL or missing references)
			//IL_0524: Unknown result type (might be due to invalid IL or missing references)
			//IL_0533: Unknown result type (might be due to invalid IL or missing references)
			//IL_053a: Expected O, but got Unknown
			//IL_0562: Unknown result type (might be due to invalid IL or missing references)
			//IL_056d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0578: Unknown result type (might be due to invalid IL or missing references)
			//IL_0591: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_05af: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_060e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0623: Unknown result type (might be due to invalid IL or missing references)
			//IL_0638: Unknown result type (might be due to invalid IL or missing references)
			//IL_064c: Unknown result type (might be due to invalid IL or missing references)
			//IL_065b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0660: Unknown result type (might be due to invalid IL or missing references)
			//IL_0673: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_06db: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f7: Expected O, but got Unknown
			//IL_071f: Unknown result type (might be due to invalid IL or missing references)
			//IL_072a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0735: Unknown result type (might be due to invalid IL or missing references)
			//IL_0744: Unknown result type (might be due to invalid IL or missing references)
			//IL_0760: Unknown result type (might be due to invalid IL or missing references)
			//IL_0775: Unknown result type (might be due to invalid IL or missing references)
			//IL_078a: Unknown result type (might be due to invalid IL or missing references)
			//IL_079f: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_07c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_07c9: Expected O, but got Unknown
			//IL_07e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_07e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_07fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0801: Unknown result type (might be due to invalid IL or missing references)
			//IL_081b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0830: Unknown result type (might be due to invalid IL or missing references)
			//IL_0849: Unknown result type (might be due to invalid IL or missing references)
			//IL_0858: Unknown result type (might be due to invalid IL or missing references)
			//IL_085d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0870: Unknown result type (might be due to invalid IL or missing references)
			//IL_0877: Unknown result type (might be due to invalid IL or missing references)
			//IL_0891: Unknown result type (might be due to invalid IL or missing references)
			//IL_08a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_08bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_08ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_08d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_08e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_092d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0925: Unknown result type (might be due to invalid IL or missing references)
			//IL_0951: Unknown result type (might be due to invalid IL or missing references)
			//IL_0966: Unknown result type (might be due to invalid IL or missing references)
			//IL_097b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0990: Unknown result type (might be due to invalid IL or missing references)
			//IL_09a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_09b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_09ba: Expected O, but got Unknown
			//IL_09e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a0f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a25: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a3b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a52: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a57: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a6a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a71: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a7c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a87: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a96: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aa6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ab1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0abb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aca: Unknown result type (might be due to invalid IL or missing references)
			//IL_0acf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ae2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b06: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b1b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b25: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b6b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b70: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b83: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bb9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0be4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bf9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c0e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c23: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c37: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d6b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d70: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d83: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d8a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d95: Unknown result type (might be due to invalid IL or missing references)
			//IL_0da0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0daf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dc9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dde: Unknown result type (might be due to invalid IL or missing references)
			//IL_0df3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e08: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e1c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c51: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c56: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c69: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c89: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cde: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d09: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d1e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d33: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d48: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d5c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e3c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e41: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e54: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e7c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0eb9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ece: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ee3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ef8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f0c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f1b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f20: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f33: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f3a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f45: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f50: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f5f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f79: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f8e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fa3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fb8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fcc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fdb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fe0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ff3: Unknown result type (might be due to invalid IL or missing references)
			//IL_1009: Unknown result type (might be due to invalid IL or missing references)
			//IL_100e: Unknown result type (might be due to invalid IL or missing references)
			//IL_1050: Unknown result type (might be due to invalid IL or missing references)
			//IL_1082: Unknown result type (might be due to invalid IL or missing references)
			//IL_1097: Unknown result type (might be due to invalid IL or missing references)
			//IL_10ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_10c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_10d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_10e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_10e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_10fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_1103: Unknown result type (might be due to invalid IL or missing references)
			//IL_110e: Unknown result type (might be due to invalid IL or missing references)
			//IL_1119: Unknown result type (might be due to invalid IL or missing references)
			//IL_1128: Unknown result type (might be due to invalid IL or missing references)
			//IL_1142: Unknown result type (might be due to invalid IL or missing references)
			//IL_1157: Unknown result type (might be due to invalid IL or missing references)
			//IL_116c: Unknown result type (might be due to invalid IL or missing references)
			//IL_1181: Unknown result type (might be due to invalid IL or missing references)
			//IL_1195: Unknown result type (might be due to invalid IL or missing references)
			//IL_136e: Unknown result type (might be due to invalid IL or missing references)
			//IL_137d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1393: Unknown result type (might be due to invalid IL or missing references)
			//IL_1399: Unknown result type (might be due to invalid IL or missing references)
			//IL_11db: Unknown result type (might be due to invalid IL or missing references)
			//IL_11ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_11fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_1264: Unknown result type (might be due to invalid IL or missing references)
			//IL_1273: Unknown result type (might be due to invalid IL or missing references)
			//IL_1291: Unknown result type (might be due to invalid IL or missing references)
			//IL_1297: Unknown result type (might be due to invalid IL or missing references)
			//IL_12ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_12fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_1312: Unknown result type (might be due to invalid IL or missing references)
			//IL_1318: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)successPopupGO != (Object)null)
				{
					Object.Destroy((Object)(object)successPopupGO);
				}
				PopupTheme popupTheme = GetPopupTheme(isPremium);
				string text = (string.IsNullOrWhiteSpace(username) ? "User" : username.Trim());
				string text2 = ((!string.IsNullOrEmpty(keyDisplay)) ? keyDisplay : (isPremium ? "★ PREMIUM" : "◇ FREE"));
				string value = timeDisplay ?? "";
				successPopupGO = new GameObject("ModMenuCrew_SuccessPopupCanvas");
				Object.DontDestroyOnLoad((Object)(object)successPopupGO);
				Canvas val = successPopupGO.AddComponent<Canvas>();
				val.renderMode = (RenderMode)0;
				val.sortingOrder = 32767;
				CanvasScaler obj = successPopupGO.AddComponent<CanvasScaler>();
				obj.uiScaleMode = (ScaleMode)1;
				obj.referenceResolution = new Vector2(1920f, 1080f);
				obj.matchWidthOrHeight = 0.5f;
				successPopupGO.AddComponent<GraphicRaycaster>();
				GameObject val2 = new GameObject("Overlay");
				val2.transform.SetParent(((Component)val).transform, false);
				((Graphic)val2.AddComponent<Image>()).color = new Color(0.03f, 0.03f, 0.05f, 0.96f);
				RectTransform component = val2.GetComponent<RectTransform>();
				component.anchorMin = Vector2.zero;
				component.anchorMax = Vector2.one;
				component.sizeDelta = Vector2.zero;
				GameObject val3 = new GameObject("PopupStarfield");
				val3.transform.SetParent(val2.transform, false);
				RectTransform obj2 = val3.AddComponent<RectTransform>();
				obj2.anchorMin = Vector2.zero;
				obj2.anchorMax = Vector2.one;
				obj2.sizeDelta = Vector2.zero;
				CreatePopupStarfield(val3.transform);
				GameObject val4 = new GameObject("MainCard");
				val4.transform.SetParent(((Component)val).transform, false);
				CanvasGroup val5 = val4.AddComponent<CanvasGroup>();
				val5.alpha = 0f;
				((Graphic)val4.AddComponent<Image>()).color = new Color(0.06f, 0.07f, 0.09f, 0.99f);
				RectTransform component2 = val4.GetComponent<RectTransform>();
				component2.anchorMin = new Vector2(0.5f, 0.5f);
				component2.anchorMax = new Vector2(0.5f, 0.5f);
				component2.pivot = new Vector2(0.5f, 0.5f);
				component2.anchoredPosition = Vector2.zero;
				component2.sizeDelta = new Vector2(650f, 480f);
				Outline obj3 = val4.AddComponent<Outline>();
				((Shadow)obj3).effectColor = new Color(popupTheme.Accent.r, popupTheme.Accent.g, popupTheme.Accent.b, 0.5f);
				((Shadow)obj3).effectDistance = new Vector2(1f, -1f);
				CreateTechCorners(component2, new Color(popupTheme.Accent.r, popupTheme.Accent.g, popupTheme.Accent.b, 0.85f));
				GameObject val6 = new GameObject("PopupAccentTop");
				val6.transform.SetParent(val4.transform, false);
				((Graphic)val6.AddComponent<Image>()).color = new Color(popupTheme.Accent.r, popupTheme.Accent.g, popupTheme.Accent.b, 0.8f);
				RectTransform component3 = val6.GetComponent<RectTransform>();
				component3.anchorMin = new Vector2(0f, 1f);
				component3.anchorMax = new Vector2(1f, 1f);
				component3.pivot = new Vector2(0.5f, 1f);
				component3.sizeDelta = new Vector2(-10f, 2f);
				component3.anchoredPosition = new Vector2(0f, -1f);
				GameObject val7 = new GameObject("HeaderBar");
				val7.transform.SetParent(val4.transform, false);
				((Graphic)val7.AddComponent<Image>()).color = new Color(popupTheme.Accent.r * 0.15f, popupTheme.Accent.g * 0.15f, popupTheme.Accent.b * 0.15f, 0.6f);
				RectTransform component4 = val7.GetComponent<RectTransform>();
				component4.anchorMin = new Vector2(0f, 1f);
				component4.anchorMax = new Vector2(1f, 1f);
				component4.pivot = new Vector2(0.5f, 1f);
				component4.sizeDelta = new Vector2(0f, 4f);
				component4.anchoredPosition = new Vector2(0f, -3f);
				GameObject val8 = new GameObject("HeaderGlow");
				val8.transform.SetParent(val4.transform, false);
				((Graphic)val8.AddComponent<Image>()).color = new Color(popupTheme.Accent.r, popupTheme.Accent.g, popupTheme.Accent.b, 0.2f);
				RectTransform component5 = val8.GetComponent<RectTransform>();
				component5.anchorMin = new Vector2(0.05f, 1f);
				component5.anchorMax = new Vector2(0.95f, 1f);
				component5.pivot = new Vector2(0.5f, 1f);
				component5.sizeDelta = new Vector2(0f, 1f);
				component5.anchoredPosition = new Vector2(0f, -7f);
				GameObject val9 = new GameObject("AccountBadge");
				val9.transform.SetParent(val4.transform, false);
				Image obj4 = val9.AddComponent<Image>();
				obj4.sprite = GetWhiteSprite();
				((Graphic)obj4).color = new Color(popupTheme.Accent.r, popupTheme.Accent.g, popupTheme.Accent.b, isPremium ? 0.95f : 0.35f);
				Outline obj5 = val9.AddComponent<Outline>();
				((Shadow)obj5).effectColor = new Color(popupTheme.AccentSoft.r, popupTheme.AccentSoft.g, popupTheme.AccentSoft.b, 0.8f);
				((Shadow)obj5).effectDistance = new Vector2(1f, -1f);
				RectTransform component6 = val9.GetComponent<RectTransform>();
				component6.anchorMin = new Vector2(1f, 1f);
				component6.anchorMax = new Vector2(1f, 1f);
				component6.pivot = new Vector2(1f, 1f);
				component6.anchoredPosition = new Vector2(-18f, -16f);
				component6.sizeDelta = new Vector2(180f, 28f);
				GameObject val10 = new GameObject("BadgeText");
				val10.transform.SetParent(val9.transform, false);
				TextMeshProUGUI obj6 = val10.AddComponent<TextMeshProUGUI>();
				((TMP_Text)obj6).text = text2;
				((TMP_Text)obj6).font = LoadGameFont();
				((TMP_Text)obj6).fontSize = 14f;
				((TMP_Text)obj6).fontStyle = (FontStyles)1;
				((TMP_Text)obj6).alignment = (TextAlignmentOptions)514;
				((Graphic)obj6).color = (isPremium ? popupTheme.BadgeText : Color.white);
				RectTransform component7 = val10.GetComponent<RectTransform>();
				component7.anchorMin = Vector2.zero;
				component7.anchorMax = Vector2.one;
				component7.sizeDelta = Vector2.zero;
				EnsureSuccessCircleSprite();
				GameObject val11 = new GameObject("IconBG");
				val11.transform.SetParent(val4.transform, false);
				Image obj7 = val11.AddComponent<Image>();
				obj7.sprite = _successCircleSprite;
				((Graphic)obj7).color = new Color(popupTheme.Accent.r, popupTheme.Accent.g, popupTheme.Accent.b, 0.45f);
				RectTransform component8 = val11.GetComponent<RectTransform>();
				component8.anchorMin = new Vector2(0.5f, 0.5f);
				component8.anchorMax = new Vector2(0.5f, 0.5f);
				component8.pivot = new Vector2(0.5f, 0.5f);
				component8.anchoredPosition = new Vector2(0f, 135f);
				component8.sizeDelta = new Vector2(100f, 100f);
				GameObject val12 = new GameObject("Checkmark");
				val12.transform.SetParent(val11.transform, false);
				GameObject val13 = new GameObject("Line1");
				val13.transform.SetParent(val12.transform, false);
				((Graphic)val13.AddComponent<Image>()).color = popupTheme.AccentSoft;
				RectTransform component9 = val13.GetComponent<RectTransform>();
				component9.sizeDelta = new Vector2(10f, 30f);
				component9.anchoredPosition = new Vector2(-10f, -5f);
				((Transform)component9).localRotation = Quaternion.Euler(0f, 0f, 45f);
				GameObject val14 = new GameObject("Line2");
				val14.transform.SetParent(val12.transform, false);
				((Graphic)val14.AddComponent<Image>()).color = popupTheme.AccentSoft;
				RectTransform component10 = val14.GetComponent<RectTransform>();
				component10.sizeDelta = new Vector2(10f, 55f);
				component10.anchoredPosition = new Vector2(12f, 10f);
				((Transform)component10).localRotation = Quaternion.Euler(0f, 0f, -45f);
				GameObject val15 = new GameObject("TitleText");
				val15.transform.SetParent(val4.transform, false);
				TextMeshProUGUI obj8 = val15.AddComponent<TextMeshProUGUI>();
				((TMP_Text)obj8).text = (isPremium ? "PREMIUM ACCESS GRANTED" : "ACCESS GRANTED");
				((TMP_Text)obj8).font = LoadGameFont();
				((TMP_Text)obj8).fontSize = 26f;
				((TMP_Text)obj8).fontStyle = (FontStyles)1;
				((Graphic)obj8).color = (isPremium ? popupTheme.AccentSoft : Color.white);
				((TMP_Text)obj8).alignment = (TextAlignmentOptions)514;
				RectTransform component11 = val15.GetComponent<RectTransform>();
				component11.anchorMin = new Vector2(0.5f, 0.5f);
				component11.anchorMax = new Vector2(0.5f, 0.5f);
				component11.pivot = new Vector2(0.5f, 0.5f);
				component11.anchoredPosition = new Vector2(0f, 85f);
				component11.sizeDelta = new Vector2(560f, 36f);
				GameObject val16 = new GameObject("AvatarContainer");
				val16.transform.SetParent(val4.transform, false);
				RectTransform val17 = val16.AddComponent<RectTransform>();
				val17.anchorMin = new Vector2(0.5f, 0.5f);
				val17.anchorMax = new Vector2(0.5f, 0.5f);
				val17.pivot = new Vector2(0.5f, 0.5f);
				val17.anchoredPosition = new Vector2(-220f, 55f);
				val17.sizeDelta = new Vector2(64f, 64f);
				_activationAvatarContainer = val17;
				GameObject val18 = new GameObject("AvatarBG");
				val18.transform.SetParent(val16.transform, false);
				((Graphic)val18.AddComponent<Image>()).color = new Color(popupTheme.Accent.r, popupTheme.Accent.g, popupTheme.Accent.b, 0.3f);
				RectTransform component12 = val18.GetComponent<RectTransform>();
				component12.anchorMin = Vector2.zero;
				component12.anchorMax = Vector2.one;
				component12.sizeDelta = Vector2.zero;
				GameObject val19 = new GameObject("AvatarImage");
				val19.transform.SetParent(val16.transform, false);
				RawImage val20 = val19.AddComponent<RawImage>();
				((Graphic)val20).color = Color.white;
				RectTransform component13 = val19.GetComponent<RectTransform>();
				component13.anchorMin = new Vector2(0.1f, 0.1f);
				component13.anchorMax = new Vector2(0.9f, 0.9f);
				component13.sizeDelta = Vector2.zero;
				_successPopupAvatarImage = val20;
				if ((Object)(object)DiscordAuthManager.AvatarTexture != (Object)null)
				{
					_successPopupAvatarImage.texture = (Texture)(object)DiscordAuthManager.AvatarTexture;
					val16.SetActive(true);
				}
				else
				{
					val16.SetActive(false);
				}
				GameObject val21 = new GameObject("WelcomeText");
				val21.transform.SetParent(val4.transform, false);
				TextMeshProUGUI obj9 = val21.AddComponent<TextMeshProUGUI>();
				((TMP_Text)obj9).text = "Welcome, <b>" + text + "</b>";
				((TMP_Text)obj9).font = LoadGameFont();
				((TMP_Text)obj9).fontSize = 18f;
				((Graphic)obj9).color = Color.white;
				((TMP_Text)obj9).alignment = (TextAlignmentOptions)514;
				((TMP_Text)obj9).richText = true;
				RectTransform component14 = val21.GetComponent<RectTransform>();
				component14.anchorMin = new Vector2(0.5f, 0.5f);
				component14.anchorMax = new Vector2(0.5f, 0.5f);
				component14.pivot = new Vector2(0.5f, 0.5f);
				component14.anchoredPosition = new Vector2(0f, 55f);
				component14.sizeDelta = new Vector2(560f, 28f);
				if (!string.IsNullOrEmpty(value))
				{
					GameObject val22 = new GameObject("TimeRemainingText");
					val22.transform.SetParent(val4.transform, false);
					TextMeshProUGUI obj10 = val22.AddComponent<TextMeshProUGUI>();
					((TMP_Text)obj10).text = $"<color=#{ColorUtility.ToHtmlStringRGB(popupTheme.Accent)}>{value}</color>";
					((TMP_Text)obj10).font = LoadGameFont();
					((TMP_Text)obj10).fontSize = 14f;
					((Graphic)obj10).color = Color.white;
					((TMP_Text)obj10).alignment = (TextAlignmentOptions)514;
					((TMP_Text)obj10).richText = true;
					RectTransform component15 = val22.GetComponent<RectTransform>();
					component15.anchorMin = new Vector2(0.5f, 0.5f);
					component15.anchorMax = new Vector2(0.5f, 0.5f);
					component15.pivot = new Vector2(0.5f, 0.5f);
					component15.anchoredPosition = new Vector2(0f, 28f);
					component15.sizeDelta = new Vector2(560f, 22f);
				}
				GameObject val23 = new GameObject("Divider1");
				val23.transform.SetParent(val4.transform, false);
				((Graphic)val23.AddComponent<Image>()).color = new Color(popupTheme.Accent.r, popupTheme.Accent.g, popupTheme.Accent.b, 0.15f);
				RectTransform component16 = val23.GetComponent<RectTransform>();
				component16.anchorMin = new Vector2(0.5f, 0.5f);
				component16.anchorMax = new Vector2(0.5f, 0.5f);
				component16.pivot = new Vector2(0.5f, 0.5f);
				component16.anchoredPosition = new Vector2(0f, 8f);
				component16.sizeDelta = new Vector2(480f, 1f);
				string text3 = (isPremium ? "<size=16><color=#00E664>✓</color> No ads / shortener links\n<color=#00E664>✓</color> Unlimited premium access\n<color=#00E664>✓</color> Priority support</size>" : "<size=16><color=#00E664>✓</color> All features unlocked\n<color=#9A9EA3>•</color> Single-use key</size>");
				GameObject val24 = new GameObject("BenefitsText");
				val24.transform.SetParent(val4.transform, false);
				TextMeshProUGUI obj11 = val24.AddComponent<TextMeshProUGUI>();
				((TMP_Text)obj11).text = text3;
				((TMP_Text)obj11).font = LoadGameFont();
				((TMP_Text)obj11).fontSize = 16f;
				((Graphic)obj11).color = Color.white;
				((TMP_Text)obj11).alignment = (TextAlignmentOptions)514;
				((TMP_Text)obj11).enableWordWrapping = true;
				((TMP_Text)obj11).richText = true;
				((TMP_Text)obj11).lineSpacing = 6f;
				RectTransform component17 = val24.GetComponent<RectTransform>();
				component17.anchorMin = new Vector2(0.5f, 0.5f);
				component17.anchorMax = new Vector2(0.5f, 0.5f);
				component17.pivot = new Vector2(0.5f, 0.5f);
				component17.anchoredPosition = new Vector2(0f, -40f);
				component17.sizeDelta = new Vector2(520f, 140f);
				GameObject val25 = new GameObject("Divider2");
				val25.transform.SetParent(val4.transform, false);
				((Graphic)val25.AddComponent<Image>()).color = new Color(popupTheme.Accent.r, popupTheme.Accent.g, popupTheme.Accent.b, 0.12f);
				RectTransform component18 = val25.GetComponent<RectTransform>();
				component18.anchorMin = new Vector2(0.5f, 0.5f);
				component18.anchorMax = new Vector2(0.5f, 0.5f);
				component18.pivot = new Vector2(0.5f, 0.5f);
				component18.anchoredPosition = new Vector2(0f, -105f);
				component18.sizeDelta = new Vector2(480f, 1f);
				GameObject val26 = new GameObject("FooterText");
				val26.transform.SetParent(val4.transform, false);
				TextMeshProUGUI obj12 = val26.AddComponent<TextMeshProUGUI>();
				ConfigEntry<KeyCode> menuToggleKey = CheatConfig.MenuToggleKey;
				KeyCode val27 = (KeyCode)((menuToggleKey == null) ? 282 : ((int)menuToggleKey.Value));
				string text4 = ((object)(KeyCode)val27).ToString();
				((TMP_Text)obj12).text = "<size=14><color=#B0B5BA>Press  <b>[ " + text4 + " ]</b>  to open/close menu</color></size>\n<size=13><color=#00CCE6><u><link=\"https://crewcore.online\">crewcore.online</link></u></color></size>";
				((TMP_Text)obj12).font = LoadGameFont();
				((TMP_Text)obj12).fontSize = 14f;
				((Graphic)obj12).color = Color.white;
				((TMP_Text)obj12).alignment = (TextAlignmentOptions)514;
				((TMP_Text)obj12).enableWordWrapping = true;
				((TMP_Text)obj12).richText = true;
				RectTransform component19 = val26.GetComponent<RectTransform>();
				component19.anchorMin = new Vector2(0.5f, 0.5f);
				component19.anchorMax = new Vector2(0.5f, 0.5f);
				component19.pivot = new Vector2(0.5f, 0.5f);
				component19.anchoredPosition = new Vector2(0f, -135f);
				component19.sizeDelta = new Vector2(560f, 48f);
				GameObject val28 = new GameObject("PopupAccentBottom");
				val28.transform.SetParent(val4.transform, false);
				((Graphic)val28.AddComponent<Image>()).color = new Color(popupTheme.Accent.r, popupTheme.Accent.g, popupTheme.Accent.b, 0.8f);
				RectTransform component20 = val28.GetComponent<RectTransform>();
				component20.anchorMin = new Vector2(0f, 0f);
				component20.anchorMax = new Vector2(1f, 0f);
				component20.pivot = new Vector2(0.5f, 0f);
				component20.sizeDelta = new Vector2(-10f, 2f);
				component20.anchoredPosition = new Vector2(0f, 1f);
				float num = -195f;
				if (isPremium)
				{
					TextMeshProUGUI saveStatusTMP = CreateTMPText(component2, "", 12, new Color(0.75f, 0.85f, 0.95f, 1f), new Vector2(0f, num - 23f), new Vector2(560f, 24f), (TextAlignmentOptions)514);
					if ((Object)(object)saveStatusTMP != (Object)null)
					{
						((TMP_Text)saveStatusTMP).richText = true;
						((TMP_Text)saveStatusTMP).enableWordWrapping = true;
					}
					Button saveBtn = null;
					saveBtn = CreateTMPButton(component2, "SAVE KEY", UnityAction.op_Implicit((Action)delegate
					{
						try
						{
							if (TrySaveKeyToFile(ModKeyValidator.CurrentKey, out var savedPath, out var error))
							{
								if ((Object)(object)saveStatusTMP != (Object)null)
								{
									((TMP_Text)saveStatusTMP).text = "<color=#00E664>Saved!</color> " + savedPath;
								}
								if ((Object)(object)saveBtn != (Object)null)
								{
									((Selectable)saveBtn).interactable = false;
								}
								TextMeshProUGUI val31 = (((Object)(object)saveBtn != (Object)null) ? ((Component)saveBtn).GetComponentInChildren<TextMeshProUGUI>() : null);
								if ((Object)(object)val31 != (Object)null)
								{
									((TMP_Text)val31).text = "SAVED";
								}
							}
							else if ((Object)(object)saveStatusTMP != (Object)null)
							{
								((TMP_Text)saveStatusTMP).text = "<color=#FF4455>Save failed:</color> " + error;
							}
						}
						catch (Exception ex)
						{
							if ((Object)(object)saveStatusTMP != (Object)null)
							{
								((TMP_Text)saveStatusTMP).text = "<color=#FF4455>Save failed:</color> " + ex.Message;
							}
						}
					}), new Vector2(-110f, num), new Vector2(200f, 50f));
					StyleButton(saveBtn, ref _cachedSaveKeyBtnSprite, popupTheme.SecondaryButtonTop, popupTheme.SecondaryButtonBottom);
					TextMeshProUGUI componentInChildren = ((Component)saveBtn).GetComponentInChildren<TextMeshProUGUI>();
					if ((Object)(object)componentInChildren != (Object)null)
					{
						((TMP_Text)componentInChildren).fontStyle = (FontStyles)1;
						((TMP_Text)componentInChildren).fontSize = 18f;
					}
					Button val29 = CreateTMPButton(component2, "CONTINUE", UnityAction.op_Implicit((Action)delegate
					{
						Object.Destroy((Object)(object)successPopupGO);
					}), new Vector2(110f, num), new Vector2(200f, 50f));
					StyleButton(val29, ref _cachedOkBtnSprite, popupTheme.PrimaryButtonTop, popupTheme.PrimaryButtonBottom);
					TextMeshProUGUI componentInChildren2 = ((Component)val29).GetComponentInChildren<TextMeshProUGUI>();
					if ((Object)(object)componentInChildren2 != (Object)null)
					{
						((TMP_Text)componentInChildren2).fontStyle = (FontStyles)1;
						((TMP_Text)componentInChildren2).fontSize = 18f;
					}
				}
				else
				{
					Button val30 = CreateTMPButton(component2, "CONTINUE", UnityAction.op_Implicit((Action)delegate
					{
						Object.Destroy((Object)(object)successPopupGO);
					}), new Vector2(0f, num), new Vector2(220f, 50f));
					StyleButton(val30, ref _cachedOkBtnSprite, popupTheme.PrimaryButtonTop, popupTheme.PrimaryButtonBottom);
					TextMeshProUGUI componentInChildren3 = ((Component)val30).GetComponentInChildren<TextMeshProUGUI>();
					if ((Object)(object)componentInChildren3 != (Object)null)
					{
						((TMP_Text)componentInChildren3).fontStyle = (FontStyles)1;
						((TMP_Text)componentInChildren3).fontSize = 20f;
					}
				}
				((MonoBehaviour)this).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(AnimatePopupContent(component2, val11.transform, val5)));
			}
			catch (Exception value2)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error showing popup: {value2}"));
			}
		}

		private void CreatePopupStarfield(Transform parent)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			_popupStars.Clear();
			for (int i = 0; i < 25; i++)
			{
				GameObject val = new GameObject("PopupStar");
				val.transform.SetParent(parent, false);
				Image val2 = val.AddComponent<Image>();
				((Graphic)val2).color = new Color(1f, 1f, 1f, Random.Range(0.2f, 0.6f));
				RectTransform component = val.GetComponent<RectTransform>();
				component.anchorMin = new Vector2(0.5f, 0.5f);
				component.anchorMax = new Vector2(0.5f, 0.5f);
				float num = Random.Range(2f, 4f);
				component.sizeDelta = new Vector2(num, num);
				component.anchoredPosition = new Vector2((float)Random.Range(-960, 960), (float)Random.Range(-540, 540));
				_popupStars.Add(new Star
				{
					Rect = component,
					Speed = Random.Range(0.2f, 1f),
					Image = val2
				});
			}
		}

		[IteratorStateMachine(typeof(_003CAnimatePopupContent_003Ed__315))]
		[HideFromIl2Cpp]
		private IEnumerator AnimatePopupContent(RectTransform content, Transform popup, CanvasGroup canvasGroup = null)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003CAnimatePopupContent_003Ed__315(0)
			{
				content = content,
				popup = popup,
				canvasGroup = canvasGroup
			};
		}

		private void UpdatePopupStarAnimation()
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)successPopupGO == (Object)null || _popupStars == null)
			{
				return;
			}
			_popupStarLoopTime += Time.deltaTime;
			for (int i = 0; i < _popupStars.Count; i++)
			{
				Star star = _popupStars[i];
				if ((Object)(object)star.Rect != (Object)null)
				{
					Vector2 anchoredPosition = star.Rect.anchoredPosition;
					anchoredPosition.x -= star.Speed * Time.deltaTime * 30f;
					if (anchoredPosition.x < -1000f)
					{
						anchoredPosition.x = 1000f;
					}
					star.Rect.anchoredPosition = anchoredPosition;
					if ((Object)(object)star.Image != (Object)null)
					{
						float num = 0.3f + Mathf.Sin(_popupStarLoopTime * star.Speed * 3f + (float)i * 1.7f) * 0.25f;
						Color color = ((Graphic)star.Image).color;
						((Graphic)star.Image).color = new Color(color.r, color.g, color.b, Mathf.Clamp(num, 0.08f, 0.7f));
					}
				}
			}
		}

		private static string GetGameRootPathSafe()
		{
			try
			{
				string gameRootPath = Paths.GameRootPath;
				if (!string.IsNullOrWhiteSpace(gameRootPath))
				{
					return gameRootPath;
				}
			}
			catch
			{
			}
			try
			{
				string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
				if (!string.IsNullOrWhiteSpace(baseDirectory))
				{
					return baseDirectory;
				}
			}
			catch
			{
			}
			try
			{
				string currentDirectory = Environment.CurrentDirectory;
				if (!string.IsNullOrWhiteSpace(currentDirectory))
				{
					return currentDirectory;
				}
			}
			catch
			{
			}
			return ".";
		}

		private string GetSavedKeyFilePath()
		{
			return Path.Combine(GetGameRootPathSafe(), "crewcore_key.txt");
		}

		private static bool IsKeyFormatValid(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return false;
			}
			string input = key.Trim().ToUpperInvariant();
			return ModKeyValidator.KeyFormatRegex.IsMatch(input);
		}

		private SavedKeyLoadResult TryLoadSavedKeyFromFile(out string key, out string detail)
		{
			key = null;
			detail = null;
			string savedKeyFilePath = GetSavedKeyFilePath();
			try
			{
				if (!File.Exists(savedKeyFilePath))
				{
					return SavedKeyLoadResult.NotFound;
				}
				string text = File.ReadAllText(savedKeyFilePath);
				if (string.IsNullOrWhiteSpace(text))
				{
					detail = "Saved key file is empty.";
					return SavedKeyLoadResult.Invalid;
				}
				string text2 = text.Trim();
				if (text2.StartsWith("ENC2:"))
				{
					try
					{
						byte[] bytes = DecryptLocalKey(Convert.FromBase64String(text2.Substring(5).Trim()));
						text2 = Encoding.UTF8.GetString(bytes);
					}
					catch
					{
						detail = "Key file was saved on a different machine.";
						return SavedKeyLoadResult.Invalid;
					}
				}
				else if (text2.StartsWith("ENC:"))
				{
					try
					{
						byte[] bytes2 = DecryptLocalKeyLegacy(Convert.FromBase64String(text2.Substring(4).Trim()));
						text2 = Encoding.UTF8.GetString(bytes2);
					}
					catch
					{
						detail = "Key file was saved on a different machine.";
						return SavedKeyLoadResult.Invalid;
					}
				}
				else
				{
					string[] array = text.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					for (int i = 0; i < array.Length; i++)
					{
						string text3 = array[i].Trim();
						if (text3.Length != 0)
						{
							text2 = text3;
							break;
						}
					}
				}
				if (string.IsNullOrWhiteSpace(text2))
				{
					detail = "Saved key file is empty.";
					return SavedKeyLoadResult.Invalid;
				}
				if (text2.StartsWith("KEY=", StringComparison.OrdinalIgnoreCase))
				{
					text2 = text2.Substring(4).Trim();
				}
				text2 = text2.Trim().ToUpperInvariant();
				if (!IsKeyFormatValid(text2))
				{
					detail = "Saved key has invalid format.";
					return SavedKeyLoadResult.Invalid;
				}
				key = text2;
				return SavedKeyLoadResult.Loaded;
			}
			catch (Exception ex)
			{
				detail = ex.Message;
				return SavedKeyLoadResult.Error;
			}
		}

		private bool TrySaveKeyToFile(string key, out string savedPath, out string error)
		{
			savedPath = GetSavedKeyFilePath();
			error = null;
			try
			{
				if (string.IsNullOrWhiteSpace(key))
				{
					error = "No key to save.";
					return false;
				}
				string text = key.Trim().ToUpperInvariant();
				if (!IsKeyFormatValid(text))
				{
					error = "Key has invalid format.";
					return false;
				}
				byte[] inArray = EncryptLocalKey(Encoding.UTF8.GetBytes(text));
				string text2 = "ENC2:" + Convert.ToBase64String(inArray);
				File.WriteAllText(savedPath, text2 + Environment.NewLine);
				return true;
			}
			catch (Exception ex)
			{
				error = ex.Message;
				return false;
			}
		}

		private static byte[] EncryptLocalKey(byte[] data)
		{
			byte[] array = new byte[16];
			using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
			{
				randomNumberGenerator.GetBytes(array);
			}
			byte[] key = DeriveKeyPBKDF2(GetMachinePassword(), array);
			using Aes aes = Aes.Create();
			aes.Key = key;
			aes.GenerateIV();
			using ICryptoTransform cryptoTransform = aes.CreateEncryptor();
			byte[] array2 = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
			byte[] array3 = new byte[16 + aes.IV.Length + array2.Length];
			Buffer.BlockCopy(array, 0, array3, 0, 16);
			Buffer.BlockCopy(aes.IV, 0, array3, 16, aes.IV.Length);
			Buffer.BlockCopy(array2, 0, array3, 32, array2.Length);
			return array3;
		}

		private static byte[] DecryptLocalKey(byte[] encryptedData)
		{
			byte[] array = new byte[16];
			Buffer.BlockCopy(encryptedData, 0, array, 0, 16);
			byte[] key = DeriveKeyPBKDF2(GetMachinePassword(), array);
			using Aes aes = Aes.Create();
			aes.Key = key;
			byte[] array2 = new byte[16];
			Buffer.BlockCopy(encryptedData, 16, array2, 0, 16);
			aes.IV = array2;
			byte[] array3 = new byte[encryptedData.Length - 32];
			Buffer.BlockCopy(encryptedData, 32, array3, 0, array3.Length);
			using ICryptoTransform cryptoTransform = aes.CreateDecryptor();
			return cryptoTransform.TransformFinalBlock(array3, 0, array3.Length);
		}

		private static byte[] DecryptLocalKeyLegacy(byte[] encryptedData)
		{
			using Aes aes = Aes.Create();
			aes.Key = GetLegacyEncryptionKey();
			byte[] array = new byte[16];
			Buffer.BlockCopy(encryptedData, 0, array, 0, 16);
			aes.IV = array;
			byte[] array2 = new byte[encryptedData.Length - 16];
			Buffer.BlockCopy(encryptedData, 16, array2, 0, array2.Length);
			using ICryptoTransform cryptoTransform = aes.CreateDecryptor();
			return cryptoTransform.TransformFinalBlock(array2, 0, array2.Length);
		}

		private static byte[] DeriveKeyPBKDF2(byte[] password, byte[] salt)
		{
			using Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
			return rfc2898DeriveBytes.GetBytes(32);
		}

		private static byte[] GetMachinePassword()
		{
			string text = "";
			try
			{
				using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Cryptography", writable: false);
				if (registryKey != null)
				{
					text = registryKey.GetValue("MachineGuid")?.ToString() ?? "";
				}
			}
			catch
			{
			}
			string s = string.Join("|", text, Environment.MachineName, Environment.UserName, SystemInfo.processorType, SystemInfo.processorCount.ToString(), Environment.OSVersion.ToString());
			return Encoding.UTF8.GetBytes(s);
		}

		private static byte[] GetLegacyEncryptionKey()
		{
			string s = Environment.MachineName + Environment.UserName + Environment.OSVersion.ToString();
			using SHA256 sHA = SHA256.Create();
			return sHA.ComputeHash(Encoding.UTF8.GetBytes(s));
		}

		private void ProcessApiKeyValidation(string keyToValidate)
		{
			if (!isValidatingNow)
			{
				if (string.IsNullOrWhiteSpace(keyToValidate))
				{
					currentActivationStatusMessage = "Please enter a key.";
					ManageActivationUIVisibility();
					return;
				}
				keyToValidate = keyToValidate.Trim();
				isValidatingNow = true;
				_validationStartTime = Time.realtimeSinceStartup;
				_validationCancelOffered = false;
				currentActivationStatusMessage = "Validating your key, please wait...";
				ManageActivationUIVisibility();
				pendingValidationTask = ValidateKeyAndSetState(keyToValidate);
			}
		}

		private void TickValidationTimeoutHint()
		{
			if (!isValidatingNow || _validationStartTime < 0f)
			{
				return;
			}
			float num = Time.realtimeSinceStartup - _validationStartTime;
			if (num >= 15f && !_validationCancelOffered)
			{
				_validationCancelOffered = true;
				string b = "Server slow — still validating, please wait...";
				if (!string.Equals(currentActivationStatusMessage, b, StringComparison.Ordinal))
				{
					currentActivationStatusMessage = b;
					if (Object.op_Implicit((Object)(object)statusMessageTextTMP))
					{
						((TMP_Text)statusMessageTextTMP).text = currentActivationStatusMessage;
					}
				}
			}
			if (num >= 30f)
			{
				try
				{
					_activationCts?.Cancel();
				}
				catch
				{
				}
				_validationStartTime = -1f;
			}
		}

		private void OnYourKeyCopyClicked(int rowIdx)
		{
			try
			{
				if (rowIdx < 0 || rowIdx >= _displayedKeys.Count)
				{
					return;
				}
				UserKeyInfo userKeyInfo = _displayedKeys[rowIdx];
				if (userKeyInfo != null && !string.IsNullOrEmpty(userKeyInfo.Key) && !userKeyInfo.IsExpired && !userKeyInfo.IsRevoked)
				{
					try
					{
						GUIUtility.systemCopyBuffer = userKeyInfo.Key;
					}
					catch
					{
					}
					currentActivationStatusMessage = "Key copied to clipboard.";
					ManageActivationUIVisibility();
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Object.op_Implicit("[YourKeys] OnCopyClicked error: " + ex.Message));
			}
		}

		private void OnYourKeyRowClicked(int rowIdx)
		{
			try
			{
				if (rowIdx < 0 || rowIdx >= _displayedKeys.Count)
				{
					return;
				}
				UserKeyInfo userKeyInfo = _displayedKeys[rowIdx];
				if (userKeyInfo == null || string.IsNullOrEmpty(userKeyInfo.Key) || userKeyInfo.IsExpired || userKeyInfo.IsRevoked)
				{
					return;
				}
				if ((Object)(object)apiKeyInputFieldTMP != (Object)null)
				{
					apiKeyInputFieldTMP.text = userKeyInfo.Key;
					if ((Object)(object)EventSystem.current != (Object)null)
					{
						EventSystem.current.SetSelectedGameObject(((Component)apiKeyInputFieldTMP).gameObject);
						apiKeyInputFieldTMP.ActivateInputField();
					}
				}
				currentActivationStatusMessage = "Key loaded — click ACTIVATE to validate.";
				ManageActivationUIVisibility();
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Object.op_Implicit("[YourKeys] OnRowClicked error: " + ex.Message));
			}
		}

		internal void RefreshUserKeys()
		{
			if (!DiscordAuthManager.IsLoggedIn)
			{
				RenderUserKeysList();
				return;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (!(realtimeSinceStartup - _lastUserKeysFetchAt < 2f))
			{
				_lastUserKeysFetchAt = realtimeSinceStartup;
				if ((Object)(object)_yourKeysCounterTxt != (Object)null)
				{
					((TMP_Text)_yourKeysCounterTxt).text = "Loading...";
				}
				FetchUserKeysAsyncWrapper();
			}
		}

		[HideFromIl2Cpp]
		private async Task FetchUserKeysAsyncWrapper()
		{
			try
			{
				var (ok, msg, _) = await UserKeysService.FetchAsync();
				ActionPermitSystem.EnqueueMainThread(delegate
				{
					try
					{
						RenderUserKeysList(ok ? null : msg);
					}
					catch
					{
					}
				});
			}
			catch (Exception ex)
			{
				string err = ex.Message;
				ActionPermitSystem.EnqueueMainThread(delegate
				{
					try
					{
						RenderUserKeysList("Error: " + err);
					}
					catch
					{
					}
				});
			}
		}

		private void RenderUserKeysList(string errorMessage = null)
		{
			//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0443: Unknown result type (might be due to invalid IL or missing references)
			if (_yourKeysRows == null)
			{
				return;
			}
			IReadOnlyList<UserKeyInfo> cachedKeys = UserKeysService.CachedKeys;
			bool isLoggedIn = DiscordAuthManager.IsLoggedIn;
			int num = cachedKeys?.Count ?? 0;
			_displayedKeys.Clear();
			if (cachedKeys != null && cachedKeys.Count > 0)
			{
				List<UserKeyInfo> list = new List<UserKeyInfo>(cachedKeys.Count);
				for (int i = 0; i < cachedKeys.Count; i++)
				{
					UserKeyInfo userKeyInfo = cachedKeys[i];
					if (userKeyInfo != null && !string.IsNullOrEmpty(userKeyInfo.Key) && !userKeyInfo.IsExpired && !userKeyInfo.IsRevoked)
					{
						list.Add(userKeyInfo);
					}
				}
				list.Sort((UserKeyInfo a, UserKeyInfo b) => b.CreatedAt.CompareTo(a.CreatedAt));
				int num2 = _yourKeysRows.Length;
				UserKeyInfo userKeyInfo2 = null;
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].IsPremium)
					{
						userKeyInfo2 = list[j];
						break;
					}
				}
				if (userKeyInfo2 != null)
				{
					_displayedKeys.Add(userKeyInfo2);
					for (int k = 0; k < list.Count; k++)
					{
						if (_displayedKeys.Count >= num2)
						{
							break;
						}
						if (list[k] != userKeyInfo2)
						{
							_displayedKeys.Add(list[k]);
						}
					}
				}
				else
				{
					int num3 = Math.Min(list.Count, num2);
					for (int l = 0; l < num3; l++)
					{
						_displayedKeys.Add(list[l]);
					}
				}
			}
			int count = _displayedKeys.Count;
			if ((Object)(object)_yourKeysCounterTxt != (Object)null)
			{
				if (!isLoggedIn)
				{
					((TMP_Text)_yourKeysCounterTxt).text = "Login with Discord";
				}
				else if (!string.IsNullOrEmpty(errorMessage))
				{
					((TMP_Text)_yourKeysCounterTxt).text = errorMessage;
				}
				else if (num == 0)
				{
					((TMP_Text)_yourKeysCounterTxt).text = "No keys yet";
				}
				else
				{
					((TMP_Text)_yourKeysCounterTxt).text = ((num == 1) ? "1 Key Generated" : $"{num} Keys Generated");
				}
			}
			if ((Object)(object)_yourKeysEmptyTxt != (Object)null)
			{
				bool active = !isLoggedIn || count == 0;
				((Component)_yourKeysEmptyTxt).gameObject.SetActive(active);
				if (!isLoggedIn)
				{
					((TMP_Text)_yourKeysEmptyTxt).text = "Login with Discord to see your keys.";
				}
				else if (num == 0)
				{
					((TMP_Text)_yourKeysEmptyTxt).text = "No keys yet. Click GET KEY below.";
				}
				else if (count == 0)
				{
					((TMP_Text)_yourKeysEmptyTxt).text = "All your keys are expired or revoked.";
				}
			}
			for (int m = 0; m < _yourKeysRows.Length; m++)
			{
				YourKeysRow yourKeysRow = _yourKeysRows[m];
				if (yourKeysRow == null || (Object)(object)yourKeysRow.Root == (Object)null)
				{
					continue;
				}
				if (m >= count)
				{
					yourKeysRow.Root.SetActive(false);
					continue;
				}
				UserKeyInfo userKeyInfo3 = _displayedKeys[m];
				yourKeysRow.Root.SetActive(true);
				if ((Object)(object)yourKeysRow.PreviewTxt != (Object)null)
				{
					((TMP_Text)yourKeysRow.PreviewTxt).text = userKeyInfo3.Preview;
				}
				string text;
				if (userKeyInfo3.CreatedAt > 0)
				{
					try
					{
						DateTime localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(userKeyInfo3.CreatedAt).LocalDateTime;
						text = $"Generated: {localDateTime:dd/MM/yyyy HH:mm}";
					}
					catch
					{
						text = "Generated: ?";
					}
				}
				else
				{
					text = "Legacy key";
				}
				if ((Object)(object)yourKeysRow.DateTxt != (Object)null)
				{
					((TMP_Text)yourKeysRow.DateTxt).text = text;
				}
				if ((Object)(object)yourKeysRow.BadgeTxt != (Object)null)
				{
					if (userKeyInfo3.IsExpired)
					{
						((TMP_Text)yourKeysRow.BadgeTxt).text = "Expired";
						((Graphic)yourKeysRow.BadgeTxt).color = GuiStyles.Theme.ErrorSoft;
					}
					else if (userKeyInfo3.IsRevoked)
					{
						((TMP_Text)yourKeysRow.BadgeTxt).text = "Revoked";
						((Graphic)yourKeysRow.BadgeTxt).color = GuiStyles.Theme.ErrorSoft;
					}
					else
					{
						string text2 = (userKeyInfo3.IsPremium ? "[*] Premium" : "[ ] Free");
						((TMP_Text)yourKeysRow.BadgeTxt).text = text2 + "  - Active";
						((Graphic)yourKeysRow.BadgeTxt).color = new Color(0.13f, 0.82f, 0.48f, 1f);
					}
				}
			}
		}

		private void TrySubmitActivation(string keyText)
		{
			if (isValidatingNow || (Object)(object)validateButtonTMP == (Object)null || !((Selectable)validateButtonTMP).interactable)
			{
				return;
			}
			if (DiscordAuthManager.IsLoggingIn)
			{
				currentActivationStatusMessage = "Discord login pending — finish in your browser.";
				ManageActivationUIVisibility();
				return;
			}
			if (!DiscordAuthManager.IsLoggedIn)
			{
				currentActivationStatusMessage = "Login with Discord first!";
				ManageActivationUIVisibility();
				return;
			}
			string text = (keyText ?? string.Empty).Trim();
			if (string.IsNullOrEmpty(text))
			{
				currentActivationStatusMessage = "Please enter your license key.";
				ManageActivationUIVisibility();
				return;
			}
			string text2 = text.ToUpperInvariant();
			if (!ModKeyValidator.KeyFormatRegex.IsMatch(text2))
			{
				currentActivationStatusMessage = "Invalid key format.";
				ManageActivationUIVisibility();
			}
			else
			{
				ProcessApiKeyValidation(text2);
			}
		}

		[HideFromIl2Cpp]
		private async Task ValidateKeyAndSetState(string key)
		{
			CancellationToken cancellationToken;
			try
			{
				_activationCts?.Dispose();
				_activationCts = new CancellationTokenSource();
				cancellationToken = _activationCts.Token;
			}
			catch
			{
				cancellationToken = CancellationToken.None;
			}
			(bool, string, string) tuple = await ModKeyValidator.ValidateKeyAsync(key, cancellationToken);
			Volatile.Write(ref _pendingValidationResult, new PendingValidationResult(tuple.Item1, tuple.Item2, tuple.Item3));
			_hasPendingValidationResult = true;
		}

		private void CleanupActivationUI()
		{
			try
			{
				try
				{
					_activationCts?.Cancel();
				}
				catch
				{
				}
				if ((Object)(object)canvasGO != (Object)null)
				{
					if ((Object)(object)activationCanvasTMP != (Object)null && ((Component)activationCanvasTMP).gameObject.activeInHierarchy)
					{
						((Component)activationCanvasTMP).gameObject.SetActive(false);
					}
					activationPanelGO = null;
					apiKeyInputFieldTMP = null;
					statusMessageTextTMP = null;
					validateButtonTMP = null;
					getKeyButtonTMP = null;
					validateButtonTextTMP = null;
					_copyLinkBtnTMP = null;
					_copyLinkTextTMP = null;
					activationCanvasTMP = null;
					_discordAvatarImage = null;
					_successPopupAvatarImage = null;
					_activationAvatarContainer = null;
					_activationAvatarGlow = null;
					_discordLoginTextTMP = null;
					_stepCircle1 = null;
					_stepCircle2 = null;
					_stepCircle3 = null;
					_stepLine1 = null;
					_stepLine2 = null;
					_stepLabel1 = null;
					_stepLabel2 = null;
					_stepLabel3 = null;
					_statusPillBg = null;
					if (Application.isPlaying)
					{
						Object.Destroy((Object)(object)canvasGO);
					}
					canvasGO = null;
					if ((Object)(object)eventSystemGO != (Object)null)
					{
						Object.Destroy((Object)(object)eventSystemGO);
						eventSystemGO = null;
					}
					((BasePlugin)Instance).Log.LogInfo((object)"Activation UI cleaned up.");
				}
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error CleanupActivationUI: {value}"));
			}
		}

		private void HandleValidationComplete()
		{
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Expected O, but got Unknown
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			try
			{
				currentActivationStatusMessage = ModKeyValidator.LastValidationMessage;
				string text = ((DiscordAuthManager.IsLoggedIn && !string.IsNullOrEmpty(DiscordAuthManager.DiscordUsername)) ? DiscordAuthManager.DiscordUsernameSafe : (ModKeyValidator.ValidatedUsername ?? "User"));
				bool flag = Volatile.Read(ref _pendingValidationResult)?.Success ?? false;
				_isModGloballyActivated = flag;
				bool flag2 = default(bool);
				if (flag)
				{
					ManualLogSource log = ((BasePlugin)Instance).Log;
					BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(40, 1, ref flag2);
					if (flag2)
					{
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ModMenuCrew] Validation Success. User: ");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(text);
					}
					log.LogInfo(val);
				}
				else
				{
					ManualLogSource log2 = ((BasePlugin)Instance).Log;
					BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(38, 1, ref flag2);
					if (flag2)
					{
						((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[ModMenuCrew] Validation Failed. Msg: ");
						((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(currentActivationStatusMessage);
					}
					log2.LogError(val2);
				}
				if (flag)
				{
					_isAutoValidatingSavedKey = false;
					CleanupActivationUI();
					if (ServerData.IsLoaded)
					{
						bool flag3 = false;
						try
						{
							ServerData.UISnapshot currentSnapshot = ServerData.CurrentSnapshot;
							if (currentSnapshot != null)
							{
								byte[] array = currentSnapshot.LobbyBytecode ?? currentSnapshot.GameBytecode;
								if (array != null && array.Length >= 536 && array[0] == 80 && array[1] == 79 && array[2] == 76 && array[3] == 53)
								{
									long num = BitConverter.ToInt64(array, 528);
									long num2 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ModKeyValidator.ServerTimeOffsetMs;
									flag3 = Math.Abs(num - num2) < 300000;
								}
							}
						}
						catch
						{
						}
						if (flag3)
						{
							_useGhostUI = true;
							if (Instance != null)
							{
								((BasePlugin)Instance).Log.LogInfo((object)"[ModMenuCrew] Ghost UI Enabled.");
							}
						}
						else
						{
							_useGhostUI = true;
							if (Instance != null)
							{
								((BasePlugin)Instance).Log.LogWarning((object)"[ModMenuCrew] Ghost UI Enabled (bytecodes may be stale, requesting refresh).");
							}
							try
							{
								ModKeyValidator.ForceHeartbeatWakeup();
							}
							catch
							{
							}
						}
					}
					else if (Instance != null)
					{
						((BasePlugin)Instance).Log.LogWarning((object)"[ModMenuCrew] Validation success but ServerData NOT loaded. Ghost UI delayed.");
					}
					if (ModKeyValidator.IsPremium && !string.IsNullOrEmpty(ModKeyValidator.CurrentKey))
					{
						try
						{
							TrySaveKeyToFile(ModKeyValidator.CurrentKey, out var _, out var _);
						}
						catch
						{
						}
					}
					if (!ModKeyValidator._isHeartbeatRunning)
					{
						try
						{
							ModKeyValidator.StartHeartbeat();
						}
						catch
						{
						}
					}
					_lastWindowTitle = "";
					try
					{
						ShowActivationSuccessPopup(text, ModKeyValidator.IsPremium, ModKeyValidator.KeyDisplay, ModKeyValidator.TimeDisplay);
						return;
					}
					catch (Exception value)
					{
						Debug.LogError(Object.op_Implicit($"[ModMenuCrew] CRASH in ShowActivationSuccessPopup: {value}"));
						return;
					}
				}
				string text2 = "Error: " + currentActivationStatusMessage;
				if (currentActivationStatusMessage.Contains("expired", StringComparison.OrdinalIgnoreCase))
				{
					text2 += "\nYour key has expired. Please generate a new one.";
				}
				else if (currentActivationStatusMessage.Contains("used", StringComparison.OrdinalIgnoreCase))
				{
					text2 += "\nThis key is already in use. Please generate a new one.";
				}
				ShowNotification(text2);
				if (_isAutoValidatingSavedKey)
				{
					_isAutoValidatingSavedKey = false;
					if (currentActivationStatusMessage.Contains("já foi utilizada", StringComparison.OrdinalIgnoreCase) || currentActivationStatusMessage.Contains("already in use", StringComparison.OrdinalIgnoreCase) || currentActivationStatusMessage.Contains("invalid", StringComparison.OrdinalIgnoreCase) || currentActivationStatusMessage.Contains("belongs to another", StringComparison.OrdinalIgnoreCase) || currentActivationStatusMessage.Contains("Formato de chave", StringComparison.OrdinalIgnoreCase))
					{
						try
						{
							string savedKeyFilePath = GetSavedKeyFilePath();
							if (File.Exists(savedKeyFilePath))
							{
								File.Delete(savedKeyFilePath);
							}
							ModMenuCrewPlugin instance = Instance;
							if (instance != null)
							{
								ManualLogSource log3 = ((BasePlugin)instance).Log;
								if (log3 != null)
								{
									log3.LogInfo((object)"[AutoValidate] Deleted saved key file: irrecoverable validation error");
								}
							}
						}
						catch
						{
						}
					}
					currentActivationStatusMessage = "Your saved key is no longer valid. Visit crewcore.online to regenerate.";
					if ((Object)(object)activationCanvasTMP == (Object)null)
					{
						SetupActivationUI_TMP();
					}
					if ((Object)(object)activationCanvasTMP != (Object)null)
					{
						((Component)activationCanvasTMP).gameObject.SetActive(true);
						_shouldAutoFocus = true;
						_currentAlpha = 0f;
						_panelScaleCurrent = 0.92f;
						_revealStartTime = Time.realtimeSinceStartup;
						_revealComplete = false;
						if (Object.op_Implicit((Object)(object)_panelCanvasGroup))
						{
							_panelCanvasGroup.alpha = 0f;
						}
						if ((Object)(object)apiKeyInputFieldTMP != (Object)null)
						{
							apiKeyInputFieldTMP.text = "";
						}
						ManageActivationUIVisibility();
					}
					hasAttemptedInitialActivationUIShow = true;
				}
				if ((Object)(object)activationCanvasTMP != (Object)null && ((Component)activationCanvasTMP).gameObject.activeSelf)
				{
					ManageActivationUIVisibility();
				}
				if ((Object)(object)apiKeyInputFieldTMP != (Object)null && (string.IsNullOrEmpty(apiKeyInputFieldTMP.text) || currentActivationStatusMessage.Contains("empty")))
				{
					apiKeyInputFieldTMP.text = "";
				}
			}
			catch (Exception value2)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error HandleValidationComplete: {value2}"));
			}
			finally
			{
				isValidatingNow = false;
				_validationStartTime = -1f;
				_validationCancelOffered = false;
				if (!isModGloballyActivated && (Object)(object)activationCanvasTMP != (Object)null && ((Component)activationCanvasTMP).gameObject.activeSelf)
				{
					ManageActivationUIVisibility();
				}
			}
		}

		private void DrawMainModWindowIMGUI()
		{
			try
			{
				if (!isModGloballyActivated)
				{
					if (GuiStyles.ErrorStyle != null)
					{
						GUILayout.Label("Mod not activated. Press F1.", GuiStyles.ErrorStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					}
					else
					{
						GUILayout.Label("Mod not activated. Press F1.", (Il2CppReferenceArray<GUILayoutOption>)null);
					}
					return;
				}
				if (GuiStyles.SectionStyle == null)
				{
					GUILayout.Label("Initializing UI...", (Il2CppReferenceArray<GUILayoutOption>)null);
					return;
				}
				UpdateWindowTitle();
				if (!((Object)(object)ShipStatus.Instance != (Object)null))
				{
					DrawLobbyUI_IMGUI();
				}
				else if (!ServerData.IsLoaded)
				{
					GUILayout.Label("Waiting for server authorization...", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
				else
				{
					GUILayout.Label("Ghost UI Active", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
			}
			catch (Exception ex)
			{
				GUILayout.Label("Menu Error: " + ex.Message, (Il2CppReferenceArray<GUILayoutOption>)null);
				if (Time.frameCount % 300 == 0)
				{
					Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Draw Error: {ex}"));
				}
			}
		}

		private void UpdateWindowTitle()
		{
			if (!(Time.unscaledTime - _lastTitleUpdateTime < 1f))
			{
				_lastTitleUpdateTime = Time.unscaledTime;
				string text = ((!string.IsNullOrEmpty(ModKeyValidator.KeyDisplay)) ? ModKeyValidator.KeyDisplay : (ModKeyValidator.IsPremium ? "★ PREMIUM" : "◇ FREE"));
				string text2 = ((!string.IsNullOrEmpty(ModKeyValidator.TimeDisplay)) ? (" • " + ModKeyValidator.TimeDisplay) : "");
				string text3 = ((!ModKeyValidator.IsPremium) ? ("◇ FREE • @" + ModKeyValidator.ValidatedUsername) : ((string.IsNullOrEmpty(ModKeyValidator.ValidatedUsername) || !(ModKeyValidator.ValidatedUsername != "⭐ Premium User") || !(ModKeyValidator.ValidatedUsername != "Premium User") || !(ModKeyValidator.ValidatedUsername != "User")) ? (text + text2) : (text + text2 + " • " + ModKeyValidator.ValidatedUsername)));
				string text4 = text3;
				if (_lastWindowTitle != text4)
				{
					_lastWindowTitle = text4;
				}
			}
		}

		private void DrawLobbyUI_IMGUI()
		{
			if (!ServerData.IsLoaded)
			{
				GUILayout.Label("Waiting for server authorization...", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				return;
			}
			if (ServerData.Tabs == null || ServerData.Tabs.Count == 0)
			{
				GUILayout.Label("No UI data received from server.", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				return;
			}
			if (_lobbyTabRegistry == null)
			{
				_lobbyTabRegistry = new Dictionary<string, Action>
				{
					{ "dashboard", DrawDashboardTab },
					{
						"ban_menu",
						delegate
						{
							banMenu?.Draw();
						}
					},
					{ "lobbies", DrawLobbyListingTabIMGUI },
					{
						"spoofing",
						delegate
						{
							spoofingMenu?.Draw();
						}
					},
					{ "settings", settingsTab.DrawSettingsTab }
				};
			}
			GUILayout.Label("Use Ghost UI mode for full functionality.", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}

		private void DrawGameEndButton(GameOverReason reason, string label, Color color)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			Color color2 = GUI.color;
			GUI.color = color;
			if (GUILayout.Button(label, GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
			{
				GameEndManager.ForceGameEnd(reason);
				string text = (GameEndManager.DidImpostorsWin(reason) ? "\ud83d\udc7f" : "\ud83d\udc65");
				ShowNotification(text + " " + label + "!");
				_showGameEndDropdown = false;
			}
			GUI.color = color2;
		}

		private void CallGameEnd(GameOverReason reason, string label)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			GameEndManager.ForceGameEnd(reason);
			string text = (GameEndManager.DidImpostorsWin(reason) ? "\ud83d\udc7f" : "\ud83d\udc65");
			ShowNotification(text + " " + label + "!");
			_showGameEndDropdown = false;
		}

		private static string GetTpRoomName(SystemTypes room)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			if (!_tpRoomNames.TryGetValue(room, out var value))
			{
				return room.ToString();
			}
			return value;
		}

		private void DrawTeleportTabIMGUI()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			if (!ServerData.IsTabEnabled("movement"))
			{
				return;
			}
			Color color = GUI.color;
			try
			{
				GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Label("\ud83d\ude80 TELEPORT", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.FlexibleSpace();
				if (teleportManager != null)
				{
					string currentMapIcon = teleportManager.GetCurrentMapIcon();
					string currentMapName = teleportManager.GetCurrentMapName();
					GUI.color = GuiStyles.Theme.Visor;
					GUILayout.Label(currentMapIcon + " " + currentMapName, GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = color;
					GUILayout.Space(8f);
					if (GUILayout.Button("↻", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(28f) }))
					{
						teleportManager.RefreshLocations();
					}
				}
				GUILayout.EndHorizontal();
				GuiStyles.DrawSeparator();
				if (teleportManager == null)
				{
					GUILayout.Label("<color=#949EAD>Teleport not available</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUILayout.EndVertical();
					return;
				}
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = GuiStyles.Theme.Accent;
				if (GUILayout.Button("\ud83c\udfaf Nearest Player", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth }))
				{
					PlayerControl closestPlayer = teleportManager.GetClosestPlayer();
					if ((Object)(object)closestPlayer != (Object)null)
					{
						teleportManager.TeleportToPlayer(closestPlayer);
						NetworkedPlayerInfo data = closestPlayer.Data;
						ShowNotification("TP → " + (((data != null) ? data.PlayerName : null) ?? "Player"));
					}
					else
					{
						ShowNotification("No players nearby");
					}
				}
				GUI.color = color;
				GUILayout.EndHorizontal();
				GUILayout.Space(4f);
				IReadOnlyDictionary<SystemTypes, Vector2> locations = teleportManager.Locations;
				if (locations.Count > 0)
				{
					int i = 0;
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					foreach (KeyValuePair<SystemTypes, Vector2> item in locations)
					{
						if (i >= 3)
						{
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
							i = 0;
						}
						if (GUILayout.Button(GetTpRoomName(item.Key), GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth }))
						{
							teleportManager.TeleportToPosition(item.Value);
							ShowNotification("TP → " + GetTpRoomName(item.Key));
						}
						i++;
					}
					for (; i < 3 && i > 0; i++)
					{
						GUILayout.FlexibleSpace();
					}
					GUILayout.EndHorizontal();
				}
				else
				{
					GUILayout.Label("<color=#949EAD>No locations — join a game first</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
				GUILayout.Space(4f);
				GUILayout.Label($"<color=#6B7280>{locations.Count} locations available</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.EndVertical();
			}
			catch (Exception ex)
			{
				GUILayout.Label("<color=#FF3344>Error: " + ex.Message + "</color>", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				try
				{
					GUILayout.EndHorizontal();
				}
				catch
				{
				}
				try
				{
					GUILayout.EndVertical();
				}
				catch
				{
				}
			}
		}

		private void DrawLobbyListingTabIMGUI()
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0341: Unknown result type (might be due to invalid IL or missing references)
			//IL_033a: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0444: Unknown result type (might be due to invalid IL or missing references)
			//IL_0437: Unknown result type (might be due to invalid IL or missing references)
			//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_0532: Unknown result type (might be due to invalid IL or missing references)
			//IL_0525: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_059c: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0701: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_07de: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0842: Unknown result type (might be due to invalid IL or missing references)
			//IL_09fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0805: Unknown result type (might be due to invalid IL or missing references)
			//IL_07fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0965: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a4b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a71: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a76: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aa9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a85: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a91: Expected O, but got Unknown
			//IL_0ad1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b2e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e5e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0eaf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c5d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c81: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c8c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c91: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c93: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c96: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cc4: Expected I4, but got Unknown
			//IL_0d8a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d90: Invalid comparison between Unknown and I4
			//IL_0dcc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dc5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d92: Unknown result type (might be due to invalid IL or missing references)
			//IL_0db6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d46: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d3f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f5c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f7d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0df2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d7f: Unknown result type (might be due to invalid IL or missing references)
			//IL_10fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0faa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0faf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fb1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fb4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fe2: Expected I4, but got Unknown
			//IL_115d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1010: Unknown result type (might be due to invalid IL or missing references)
			//IL_1076: Unknown result type (might be due to invalid IL or missing references)
			//IL_107c: Invalid comparison between Unknown and I4
			//IL_1194: Unknown result type (might be due to invalid IL or missing references)
			//IL_108e: Unknown result type (might be due to invalid IL or missing references)
			//IL_1094: Invalid comparison between Unknown and I4
			//IL_124b: Unknown result type (might be due to invalid IL or missing references)
			//IL_10eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_12fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_12f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_1380: Unknown result type (might be due to invalid IL or missing references)
			if (!ServerData.IsTabEnabled("lobbies"))
			{
				return;
			}
			bool flag = GhostUI.CurrentBucket <= LayoutBucket.Micro;
			bool flag2 = GhostUI.CurrentBucket <= LayoutBucket.Tight;
			bool flag3 = GhostUI.CurrentBucket <= LayoutBucket.Compact;
			Color color = GUI.color;
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			try
			{
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Label(flag ? "\ud83c\udf10 LOBBIES" : "\ud83c\udf10 LOBBY BROWSER", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.FlexibleSpace();
				bool captureEnabled = LobbyListingPatch.CaptureEnabled;
				GUI.color = (captureEnabled ? GuiStyles.Theme.Success : GuiStyles.Theme.TextMuted);
				GUILayout.Label((!flag) ? (captureEnabled ? "● LIVE" : "○ OFF") : (captureEnabled ? "●" : "○"), GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color;
				GUILayout.EndHorizontal();
				GUILayout.Space((float)(flag ? 4 : 8));
				GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
				float w = GuiStyles.Spacing.MenuSize(flag ? 40f : (flag2 ? 60f : 75f), 36f);
				float w2 = GuiStyles.Spacing.MenuSize(flag ? 32f : (flag2 ? 52f : 65f), 28f);
				float w3 = GuiStyles.Spacing.MenuSize(flag ? 32f : (flag2 ? 52f : 65f), 28f);
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				if (GUILayout.Button(flag ? "\ud83d\udd04" : "\ud83d\udd04 Refresh", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w) }))
				{
					LobbyListingPatch.RefreshLobbyList();
					_cachedLobbies = null;
					ShowNotification("Refreshing...");
				}
				if (GUILayout.Button(flag ? "\ud83d\uddd1" : "\ud83d\uddd1 Clear", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w2) }))
				{
					LobbyListingPatch.ClearCapturedLobbies();
					_selectedLobbyIndex = -1;
					_cachedLobbies = null;
				}
				if (GUILayout.Button(flag ? "\ud83d\udccb" : "\ud83d\udccb Paste", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w3) }))
				{
					try
					{
						string text = GUIUtility.systemCopyBuffer?.Trim().ToUpper();
						if (!string.IsNullOrEmpty(text) && text.Length >= 4 && text.Length <= 6)
						{
							LobbyListingPatch.JoinByCode(text);
							ShowNotification("Joining...");
						}
						else
						{
							ShowNotification("Copy a code first!");
						}
					}
					catch
					{
						ShowNotification("Invalid code!");
					}
				}
				if (!flag)
				{
					GUILayout.FlexibleSpace();
					LobbyListingPatch.CaptureEnabled = GUILayout.Toggle(LobbyListingPatch.CaptureEnabled, flag2 ? "  Auto" : "  Auto-Capture", GuiStyles.ToggleStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
				else
				{
					GUILayout.FlexibleSpace();
					LobbyListingPatch.CaptureEnabled = GUILayout.Toggle(LobbyListingPatch.CaptureEnabled, "", GuiStyles.ToggleStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(18f) });
				}
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
				GUILayout.Space((float)(flag ? 2 : 4));
				GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
				byte? filterMapId = LobbyListingPatch.FilterMapId;
				int currentSortMode = (int)LobbyListingPatch.CurrentSortMode;
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				float num = GuiStyles.Spacing.MenuSize(flag ? 26f : 35f, 22f);
				float num2 = GuiStyles.Spacing.MenuSize(flag ? 20f : 24f, 18f);
				GUI.color = ((!filterMapId.HasValue) ? GuiStyles.Theme.Accent : _colorWhite);
				if (GUILayout.Button("All", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num) }))
				{
					LobbyListingPatch.FilterMapId = null;
					_cachedLobbies = null;
				}
				GUI.color = ((filterMapId == 0) ? _mapColors[0] : _colorWhite);
				if (GUILayout.Button("\ud83d\ude80", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num2) }))
				{
					LobbyListingPatch.FilterMapId = 0;
					_cachedLobbies = null;
				}
				GUI.color = ((((int?)filterMapId).GetValueOrDefault() == 1) ? _mapColors[1] : _colorWhite);
				if (GUILayout.Button("\ud83c\udfe2", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num2) }))
				{
					LobbyListingPatch.FilterMapId = (byte)1;
					_cachedLobbies = null;
				}
				GUI.color = ((((int?)filterMapId).GetValueOrDefault() == 2) ? _mapColors[2] : _colorWhite);
				if (GUILayout.Button("❄", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num2) }))
				{
					LobbyListingPatch.FilterMapId = (byte)2;
					_cachedLobbies = null;
				}
				GUI.color = ((((int?)filterMapId).GetValueOrDefault() == 4) ? _mapColors[4] : _colorWhite);
				if (GUILayout.Button("✈", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num2) }))
				{
					LobbyListingPatch.FilterMapId = (byte)4;
					_cachedLobbies = null;
				}
				GUI.color = ((((int?)filterMapId).GetValueOrDefault() == 5) ? _mapColors[5] : _colorWhite);
				if (GUILayout.Button("\ud83c\udf34", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num2) }))
				{
					LobbyListingPatch.FilterMapId = (byte)5;
					_cachedLobbies = null;
				}
				GUI.color = color;
				if (!flag)
				{
					GUILayout.Space((float)(flag2 ? 4 : 8));
					bool filterHasSpace = LobbyListingPatch.FilterHasSpace;
					LobbyListingPatch.FilterHasSpace = GUILayout.Toggle(LobbyListingPatch.FilterHasSpace, flag2 ? "Sp" : "Space", GuiStyles.ToggleStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(flag2 ? 38f : 60f) });
					if (filterHasSpace != LobbyListingPatch.FilterHasSpace)
					{
						_cachedLobbies = null;
					}
					bool filterFreeChatOnly = LobbyListingPatch.FilterFreeChatOnly;
					LobbyListingPatch.FilterFreeChatOnly = GUILayout.Toggle(LobbyListingPatch.FilterFreeChatOnly, flag2 ? "Ch" : "Chat", GuiStyles.ToggleStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(flag2 ? 38f : 50f) });
					if (filterFreeChatOnly != LobbyListingPatch.FilterFreeChatOnly)
					{
						_cachedLobbies = null;
					}
				}
				GUILayout.FlexibleSpace();
				if (filterMapId.HasValue || currentSortMode != 0 || LobbyListingPatch.FilterHasSpace || LobbyListingPatch.FilterFreeChatOnly)
				{
					GUI.color = GuiStyles.Theme.Error;
					if (GUILayout.Button("✕", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(22f) }))
					{
						LobbyListingPatch.ClearFilters();
						_cachedLobbies = null;
					}
					GUI.color = color;
				}
				GUILayout.EndHorizontal();
				if (flag)
				{
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					bool filterHasSpace2 = LobbyListingPatch.FilterHasSpace;
					LobbyListingPatch.FilterHasSpace = GUILayout.Toggle(LobbyListingPatch.FilterHasSpace, "Sp", GuiStyles.ToggleStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(34f) });
					if (filterHasSpace2 != LobbyListingPatch.FilterHasSpace)
					{
						_cachedLobbies = null;
					}
					bool filterFreeChatOnly2 = LobbyListingPatch.FilterFreeChatOnly;
					LobbyListingPatch.FilterFreeChatOnly = GUILayout.Toggle(LobbyListingPatch.FilterFreeChatOnly, "Ch", GuiStyles.ToggleStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(34f) });
					if (filterFreeChatOnly2 != LobbyListingPatch.FilterFreeChatOnly)
					{
						_cachedLobbies = null;
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				if (!flag)
				{
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					if (!flag2)
					{
						GUI.color = GuiStyles.Theme.TextMuted;
						GUILayout.Label("Sort:", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(30f) });
						GUI.color = color;
					}
					int num3 = (flag2 ? 4 : _sortOptions.Length);
					for (int i = 0; i < num3; i++)
					{
						GUI.color = ((currentSortMode == i) ? GuiStyles.Theme.Accent : _colorWhite);
						if (GUILayout.Button(_sortOptions[i], GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
						{
							LobbyListingPatch.CurrentSortMode = (LobbyListingPatch.LobbySortMode)i;
							_cachedLobbies = null;
						}
					}
					GUI.color = color;
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				GUILayout.Space((float)(flag ? 2 : 4));
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				if (_cachedLobbies == null || realtimeSinceStartup - _lastLobbyRefresh > 0.5f)
				{
					_cachedLobbies = LobbyListingPatch.GetFilteredAndSortedLobbies();
					_lastLobbyRefresh = realtimeSinceStartup;
				}
				int count = LobbyListingPatch.CapturedLobbies.Count;
				int totalGamesInMatchmaker = LobbyListingPatch.TotalGamesInMatchmaker;
				DateTime lastUpdateTime = LobbyListingPatch.LastUpdateTime;
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = GuiStyles.Theme.TextMuted;
				if (flag)
				{
					GUILayout.Label(_cachedLobbies.Count + "/" + count, GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
				else
				{
					GUILayout.Label("\ud83d\udcca " + _cachedLobbies.Count + "/" + count + " captured", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUILayout.FlexibleSpace();
					if ((Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmConnected)
					{
						GUI.color = _colorOrange;
						GUILayout.Label("⚠ In Game", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					}
					else
					{
						GUILayout.Label(totalGamesInMatchmaker + " online", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					}
					if (!flag2)
					{
						GUILayout.FlexibleSpace();
						int num4 = ((lastUpdateTime == DateTime.MinValue) ? (-1) : ((int)(DateTime.Now - lastUpdateTime).TotalSeconds));
						GUILayout.Label((num4 < 0) ? "\ud83d\udd50 never" : ("\ud83d\udd50 " + num4 + "s ago"), GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					}
				}
				GUI.color = color;
				GUILayout.EndHorizontal();
				GUILayout.Space((float)(flag ? 2 : 4));
				if (_cachedLobbies.Count == 0)
				{
					GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
					GUILayout.Space((float)(flag ? 8 : 16));
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					GUILayout.FlexibleSpace();
					GUI.color = GuiStyles.Theme.TextMuted;
					if (_lobbyEmptyIconStyle == null || _lobbyEmptyIconStyleIsMicro != flag)
					{
						_lobbyEmptyIconStyle = new GUIStyle(GUI.skin.label)
						{
							fontSize = (flag ? 20 : 32),
							alignment = (TextAnchor)4
						};
						_lobbyEmptyIconStyleIsMicro = flag;
					}
					GUILayout.Label("\ud83d\udd0d", _lobbyEmptyIconStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = color;
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					if (!flag)
					{
						GUILayout.Space(8f);
						GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.FlexibleSpace();
						GUI.color = GuiStyles.Theme.TextMuted;
						GUILayout.Label("No lobbies captured yet", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						GUILayout.Space(4f);
						GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.FlexibleSpace();
						GUILayout.Label(flag2 ? "Open Find Game" : "Go to  Online → Find Game  to capture lobbies", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						GUI.color = color;
					}
					GUILayout.Space((float)(flag ? 8 : 16));
					GUILayout.EndVertical();
					return;
				}
				int num5 = Math.Min(_cachedLobbies.Count, 30);
				float num6 = (flag ? 22f : (flag2 ? 24f : 28f));
				float num7 = GuiStyles.Spacing.MenuSize(14f, 10f);
				float num8 = GuiStyles.Spacing.MenuSize(18f, 14f);
				float num9 = GuiStyles.Spacing.MenuSize(18f, 14f);
				float num10 = GuiStyles.Spacing.MenuSize(28f, 20f);
				float num11 = GuiStyles.Spacing.MenuSize(flag ? 30f : 35f, 24f);
				float num12 = GuiStyles.Spacing.MenuSize(flag ? 22f : 26f, 18f);
				for (int j = 0; j < num5; j++)
				{
					LobbyListingPatch.CapturedLobby capturedLobby = _cachedLobbies[j];
					if (capturedLobby == null)
					{
						continue;
					}
					GUILayout.BeginHorizontal((j == _selectedLobbyIndex) ? GuiStyles.SelectedItemStyle : GuiStyles.ItemStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(num6) });
					byte b = (byte)((capturedLobby.MapId < 6) ? capturedLobby.MapId : 0);
					GUI.color = _mapColors[b];
					GUILayout.Label("●", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num7) });
					GUI.color = color;
					if (!flag)
					{
						Platforms platform = capturedLobby.Platform;
						string text2;
						switch (platform - 1)
						{
						case 0:
						case 1:
						case 3:
							text2 = "\ud83d\udcbb";
							break;
						case 7:
						case 8:
						case 9:
							text2 = "\ud83c\udfae";
							break;
						case 5:
						case 6:
							text2 = "\ud83d\udcf1";
							break;
						default:
							text2 = "\ud83d\udda5";
							break;
						}
						GUILayout.Label(text2, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num8) });
					}
					if (GUILayout.Button(capturedLobby.ListRowText, GuiStyles.ListButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth }))
					{
						_selectedLobbyIndex = j;
					}
					if (!flag && capturedLobby.NumImpostors > 0)
					{
						GUI.color = ((capturedLobby.NumImpostors >= 3) ? _colorRed : _colorOrange);
						GUILayout.Label(capturedLobby.NumImpostors + "i", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num9) });
						GUI.color = color;
					}
					if (!flag3 && (int)capturedLobby.GameMode == 2)
					{
						GUI.color = _colorCyan;
						GUILayout.Label("H&S", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num10) });
						GUI.color = color;
					}
					GUI.color = (capturedLobby.IsFull ? GuiStyles.Theme.Error : GuiStyles.Theme.Success);
					GUILayout.Label(capturedLobby.PlayerCountText, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num11) });
					GUI.color = color;
					if (GUILayout.Button("▶", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num12) }))
					{
						LobbyListingPatch.JoinCapturedLobby(capturedLobby);
						ShowNotification("Joining " + (capturedLobby.GameCode ?? "?"));
					}
					GUILayout.EndHorizontal();
				}
				if (_cachedLobbies.Count > 30)
				{
					GUI.color = GuiStyles.Theme.TextMuted;
					int num13 = _cachedLobbies.Count - 30;
					GUILayout.Label(flag ? ("+" + num13) : ("+ " + num13 + " more lobbies..."), GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = color;
				}
				if (_selectedLobbyIndex >= _cachedLobbies.Count)
				{
					_selectedLobbyIndex = -1;
				}
				if (_selectedLobbyIndex < 0 || _selectedLobbyIndex >= _cachedLobbies.Count)
				{
					return;
				}
				LobbyListingPatch.CapturedLobby capturedLobby2 = _cachedLobbies[_selectedLobbyIndex];
				if (capturedLobby2 == null)
				{
					return;
				}
				GUILayout.Space((float)(flag ? 4 : 8));
				GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
				byte b2 = (byte)((capturedLobby2.MapId < 6) ? capturedLobby2.MapId : 0);
				string text3 = capturedLobby2.GameCode ?? "?";
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = _mapColors[b2];
				GUILayout.Label("● " + text3, GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color;
				GUILayout.FlexibleSpace();
				if (!flag)
				{
					GUILayout.Label(capturedLobby2.DetailsLine, GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
				GUILayout.EndHorizontal();
				if (!flag2)
				{
					Platforms platform = capturedLobby2.Platform;
					string text2;
					switch (platform - 1)
					{
					case 0:
					case 1:
					case 3:
						text2 = "\ud83d\udcbb";
						break;
					case 7:
					case 8:
					case 9:
						text2 = "\ud83c\udfae";
						break;
					case 5:
					case 6:
						text2 = "\ud83d\udcf1";
						break;
					default:
						text2 = "\ud83d\udda5";
						break;
					}
					string text4 = text2;
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = GuiStyles.Theme.TextMuted;
					string text5 = capturedLobby2.TrueHostName ?? capturedLobby2.HostName ?? "Unknown";
					if (text5.Length > 15)
					{
						text5 = text5.Substring(0, 13) + "..";
					}
					GUILayout.Label("Host: " + text5, GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUILayout.FlexibleSpace();
					string text6 = (((int)capturedLobby2.QuickChat == 1) ? "\ud83d\udcac Free" : "⚡ Quick");
					string text7 = (((int)capturedLobby2.GameMode == 2) ? "\ud83c\udfc3 H&S" : "\ud83c\udfad Classic");
					GUILayout.Label(text4 + " " + capturedLobby2.PlatformName + " | " + text6 + " | " + text7, GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = color;
					GUILayout.EndHorizontal();
				}
				else
				{
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = GuiStyles.Theme.TextMuted;
					string text8 = capturedLobby2.TrueHostName ?? capturedLobby2.HostName ?? "Unknown";
					if (text8.Length > 18)
					{
						text8 = text8.Substring(0, 16) + "..";
					}
					GUILayout.Label("Host: " + text8, GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = color;
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				if (!flag && (capturedLobby2.NumImpostors > 0 || capturedLobby2.KillCooldown > 0f))
				{
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = _colorOrange;
					if (capturedLobby2.NumImpostors > 0)
					{
						GUILayout.Label("\ud83d\udc7f " + capturedLobby2.NumImpostors + " Imp", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					}
					if (capturedLobby2.KillCooldown > 0f)
					{
						GUILayout.Label("⏱ " + (int)capturedLobby2.KillCooldown + "s CD", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					}
					if (capturedLobby2.PlayerSpeed > 0f && !flag2)
					{
						GUILayout.Label("\ud83c\udfc3 " + capturedLobby2.PlayerSpeed.ToString("F1") + "x", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					}
					GUI.color = color;
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				GUILayout.Space((float)(flag ? 2 : 4));
				float w4 = GuiStyles.Spacing.MenuSize(flag ? 40f : (flag2 ? 56f : 70f), 36f);
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				if (GUILayout.Button(flag ? "\ud83d\udccb" : "\ud83d\udccb Copy", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedWidth(w4) }))
				{
					LobbyListingPatch.CopyCodeToClipboard(text3);
					ShowNotification("Code Copied!");
				}
				bool flag4 = (Object)(object)AmongUsClient.Instance == (Object)null || !((InnerNetClient)AmongUsClient.Instance).AmConnected;
				GUI.color = (flag4 ? GuiStyles.Theme.Success : GuiStyles.Theme.TextMuted);
				if (GUILayout.Button((!flag4) ? (flag ? "⚠" : (flag2 ? "⚠ Busy" : "⚠ Already Connected")) : (flag ? "\ud83d\ude80 JOIN" : (flag2 ? "\ud83d\ude80 JOIN" : "\ud83d\ude80 JOIN LOBBY")), GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth }))
				{
					if (flag4)
					{
						LobbyListingPatch.JoinCapturedLobby(capturedLobby2);
						ShowNotification("Joining " + text3);
					}
					else
					{
						ShowNotification("Leave current game first!");
					}
				}
				GUI.color = color;
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
			}
			finally
			{
				GUILayout.EndVertical();
			}
		}

		private void DrawGameTabIMGUI()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_048e: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0275: Unknown result type (might be due to invalid IL or missing references)
			//IL_054a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0550: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_05dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0600: Unknown result type (might be due to invalid IL or missing references)
			//IL_061a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0358: Unknown result type (might be due to invalid IL or missing references)
			//IL_036f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0382: Unknown result type (might be due to invalid IL or missing references)
			//IL_0390: Unknown result type (might be due to invalid IL or missing references)
			//IL_039e: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_03de: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0302: Unknown result type (might be due to invalid IL or missing references)
			//IL_0324: Unknown result type (might be due to invalid IL or missing references)
			//IL_0344: Unknown result type (might be due to invalid IL or missing references)
			//IL_0402: Unknown result type (might be due to invalid IL or missing references)
			//IL_0419: Unknown result type (might be due to invalid IL or missing references)
			//IL_042c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0449: Unknown result type (might be due to invalid IL or missing references)
			//IL_0695: Unknown result type (might be due to invalid IL or missing references)
			//IL_06af: Unknown result type (might be due to invalid IL or missing references)
			//IL_06fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0716: Unknown result type (might be due to invalid IL or missing references)
			//IL_072b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0745: Unknown result type (might be due to invalid IL or missing references)
			//IL_06be: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_081a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0834: Unknown result type (might be due to invalid IL or missing references)
			//IL_086b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0885: Unknown result type (might be due to invalid IL or missing references)
			if (!ServerData.IsTabEnabled("cheats"))
			{
				return;
			}
			Color color = GUI.color;
			Color backgroundColor = GUI.backgroundColor;
			bool flag = (Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmHost;
			bool flag2 = GameEndManager.CanEndGame();
			Color val = default(Color);
			(val)._002Ector(0.3f, 0.9f, 0.55f, 1f);
			Color val2 = default(Color);
			(val2)._002Ector(1f, 0.55f, 0.3f, 1f);
			Color color2 = default(Color);
			(color2)._002Ector(1f, 0.4f, 0.45f, 1f);
			Color color3 = default(Color);
			(color3)._002Ector(0.45f, 0.75f, 1f, 1f);
			Color color4 = default(Color);
			(color4)._002Ector(0.35f, 0.95f, 0.55f, 1f);
			Color color5 = default(Color);
			(color5)._002Ector(0.55f, 0.85f, 1f, 1f);
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			GUI.color = GuiStyles.Theme.Accent;
			GUILayout.Label("FORCE GAME END", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color;
			GuiStyles.DrawSeparator();
			GUILayout.Space(6f);
			if (!flag2)
			{
				GUI.color = GuiStyles.Theme.TextMuted;
				GUILayout.Label("Unavailable: " + GameEndManager.GetCannotEndReason(), GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color;
			}
			else
			{
				GUI.color = GuiStyles.Theme.TextMuted;
				GUILayout.Label("Quick end — pick a winner:", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color;
				GUILayout.Space(6f);
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.backgroundColor = new Color(val.r, val.g, val.b, 0.6f);
				GUI.color = Color.white;
				if (GUILayout.Button("MY TEAM WINS", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
				{
					GUILayout.Height(54f),
					GUILayout.ExpandWidth(true)
				}))
				{
					GameEndManager.ForceMyTeamWin();
					ShowNotification("Your team wins!");
					_showGameEndDropdown = false;
				}
				GUI.backgroundColor = new Color(val2.r, val2.g, val2.b, 0.6f);
				if (GUILayout.Button("ENEMY WINS", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
				{
					GUILayout.Height(54f),
					GUILayout.ExpandWidth(true)
				}))
				{
					GameEndManager.ForceEnemyTeamWin();
					ShowNotification("Enemy team wins!");
					_showGameEndDropdown = false;
				}
				GUI.backgroundColor = backgroundColor;
				GUI.color = color;
				GUILayout.EndHorizontal();
				GUILayout.Space(8f);
				if (GUILayout.Button(_showGameEndDropdown ? "ADVANCED WIN CONDITIONS  ▼" : "ADVANCED WIN CONDITIONS  ▶", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(28f) }))
				{
					_showGameEndDropdown = !_showGameEndDropdown;
				}
				if (_showGameEndDropdown)
				{
					GUILayout.Space(8f);
					if (GameEndManager.IsHideAndSeekMode())
					{
						GUI.color = GuiStyles.Theme.Warning;
						GUILayout.Label("HIDE AND SEEK", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUI.color = color;
						GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
						DrawGameEndButton((GameOverReason)8, "Seeker Wins", new Color(1f, 0.4f, 0.4f));
						DrawGameEndButton((GameOverReason)7, "Hiders Win", new Color(0.4f, 0.9f, 0.6f));
						GUILayout.EndHorizontal();
					}
					else
					{
						GUI.color = color2;
						GUILayout.Label("IMPOSTOR VICTORY BY...", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUI.color = color;
						GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
						DrawGameEndButton((GameOverReason)3, "KILL", color2);
						DrawGameEndButton((GameOverReason)4, "SABOTAGE", color2);
						DrawGameEndButton((GameOverReason)2, "VOTE", color2);
						GUILayout.EndHorizontal();
						GUILayout.Space(8f);
						GUI.color = color3;
						GUILayout.Label("CREWMATE VICTORY BY...", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUI.color = color;
						GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
						DrawGameEndButton((GameOverReason)0, "VOTE", color3);
						DrawGameEndButton((GameOverReason)1, "TASKS", color4);
						GUILayout.EndHorizontal();
					}
					GUILayout.Space(8f);
					GUI.color = color2;
					GUILayout.Label("FAKE DISCONNECT", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = color;
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					DrawGameEndButton((GameOverReason)5, "IMP DC -> CREW", color5);
					DrawGameEndButton((GameOverReason)6, "CREW DC -> IMP", new Color(0.95f, 0.5f, 0.5f));
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndVertical();
			GUILayout.Space(8f);
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			GUI.color = GuiStyles.Theme.Accent;
			GUILayout.Label("MEETING CONTROLS", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color;
			GuiStyles.DrawSeparator();
			GUILayout.Space(6f);
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.backgroundColor = new Color(GuiStyles.Theme.Accent.r, GuiStyles.Theme.Accent.g, GuiStyles.Theme.Accent.b, 0.85f);
			GUI.color = Color.white;
			if ((Object)(object)PlayerControl.LocalPlayer != (Object)null && GUILayout.Button("CALL EMERGENCY", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.Height(40f),
				GUILayout.ExpandWidth(true)
			}))
			{
				PlayerControl.LocalPlayer.CmdReportDeadBody((NetworkedPlayerInfo)null);
				ShowNotification("Emergency meeting called!");
			}
			GUI.backgroundColor = backgroundColor;
			GUI.color = color;
			GUI.enabled = flag;
			if (GUILayout.Button("CLOSE MEETING", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.Height(40f),
				GUILayout.ExpandWidth(true)
			}))
			{
				GameCheats.CloseMeeting();
				ShowNotification("Meeting Closed!");
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			GUILayout.Space(4f);
			GUI.color = GuiStyles.Theme.TextMuted;
			GUILayout.Label("Control meetings and discussions" + (flag ? "" : " — Close requires host."), GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color;
			GUILayout.EndVertical();
			GUILayout.Space(8f);
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			GUI.color = GuiStyles.Theme.Accent;
			GUILayout.Label("HOST PROTECTIONS", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color;
			GuiStyles.DrawSeparator();
			GUILayout.Space(6f);
			bool flag3 = CheatConfig.DisableGameEnd?.Value ?? DisableGameEndFallback;
			bool flag4 = GuiStyles.DrawBetterToggle(flag3, "DISABLE GAME END", "Prevent players from ending the game");
			if (flag4 != flag3)
			{
				DisableGameEndFallback = flag4;
				if (CheatConfig.DisableGameEnd != null)
				{
					CheatConfig.DisableGameEnd.Value = flag4;
				}
				if (flag4)
				{
					ShowNotification("Game End Blocked! (Host Only)");
				}
				else
				{
					ShowNotification("Game End Allowed.");
				}
			}
			GUI.color = GuiStyles.Theme.TextMuted;
			GUILayout.Label("Blocks all methods to end the game prematurely", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color;
			if (!flag && flag3)
			{
				GUI.color = GuiStyles.Theme.Warning;
				GUILayout.Label("You are not host — effect won't apply.", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color;
			}
			GUILayout.EndVertical();
			GUILayout.Space(8f);
			GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
			GUI.color = GuiStyles.Theme.Accent;
			GUILayout.Label("FAKE ANIMATIONS", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color;
			GuiStyles.DrawSeparator();
			GUILayout.Space(6f);
			GUI.color = GuiStyles.Theme.TextMuted;
			GUILayout.Label("Play once:", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color;
			GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
			if (GUILayout.Button("SHIELDS", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.Height(34f),
				GUILayout.ExpandWidth(true)
			}))
			{
				GameCheats.AnimShields = true;
				ShowNotification("Shields animation triggered");
			}
			if (GUILayout.Button("WEAPONS", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.Height(34f),
				GUILayout.ExpandWidth(true)
			}))
			{
				GameCheats.AnimAsteroids = true;
				ShowNotification("Weapons animation triggered");
			}
			if (GUILayout.Button("TRASH", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.Height(34f),
				GUILayout.ExpandWidth(true)
			}))
			{
				GameCheats.AnimEmptyGarbage = true;
				ShowNotification("Trash animation triggered");
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(6f);
			GUI.color = GuiStyles.Theme.TextMuted;
			GUILayout.Label("Persistent toggle:", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color;
			bool animCamsInUse = GameCheats.AnimCamsInUse;
			bool flag5 = GuiStyles.DrawBetterToggle(animCamsInUse, "CAMS ACTIVE", "Fake security cameras as permanently in-use");
			if (flag5 != animCamsInUse)
			{
				GameCheats.AnimCamsInUse = flag5;
			}
			GUILayout.Space(8f);
			GUI.color = GuiStyles.Theme.TextMuted;
			GUILayout.Label("(i) Animations respect current map limits.", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUI.color = color;
			GUILayout.EndVertical();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private void DrawMovementTabIMGUI()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0477: Unknown result type (might be due to invalid IL or missing references)
			//IL_0491: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_052e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0539: Unknown result type (might be due to invalid IL or missing references)
			//IL_0579: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_05db: Unknown result type (might be due to invalid IL or missing references)
			//IL_064d: Unknown result type (might be due to invalid IL or missing references)
			//IL_029d: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_080d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0827: Unknown result type (might be due to invalid IL or missing references)
			//IL_08d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_08ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_08f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_08a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_08be: Unknown result type (might be due to invalid IL or missing references)
			//IL_09af: Unknown result type (might be due to invalid IL or missing references)
			//IL_09d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0727: Unknown result type (might be due to invalid IL or missing references)
			//IL_0720: Unknown result type (might be due to invalid IL or missing references)
			//IL_0779: Unknown result type (might be due to invalid IL or missing references)
			if (!ServerData.IsTabEnabled("movement"))
			{
				return;
			}
			Color color = GUI.color;
			if ((Object)(object)ShipStatus.Instance == (Object)null)
			{
				GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
				GUI.color = GuiStyles.Theme.Accent;
				GUILayout.Label("<b>[*] MOVEMENT</b>", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color;
				GuiStyles.DrawSeparator();
				GUILayout.Space(8f);
				GUI.color = GuiStyles.Theme.TextMuted;
				GUILayout.Label("Join or start a match to use movement features.", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color;
				GUILayout.Space(8f);
				GUILayout.EndVertical();
				return;
			}
			try
			{
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
				GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
				GUI.color = GuiStyles.Theme.Accent;
				GUILayout.Label("<b>[*] MOBILITY</b>", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color;
				GuiStyles.DrawSeparator();
				GUILayout.Space(4f);
				if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
				{
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					IsNoclipping = GuiStyles.DrawBetterToggle(IsNoclipping, "Noclip", "Walk through walls");
					GUILayout.FlexibleSpace();
					GuiStyles.DrawStatusIndicator(IsNoclipping);
					GUILayout.EndHorizontal();
					if ((Object)(object)PlayerControl.LocalPlayer.Collider != (Object)null)
					{
						((Behaviour)PlayerControl.LocalPlayer.Collider).enabled = !IsNoclipping;
					}
				}
				GUILayout.Space(2f);
				GUILayout.EndVertical();
				GUILayout.Space(6f);
				GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = GuiStyles.Theme.Accent;
				GUILayout.Label("<b>[*] SPEED</b>", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color;
				GUILayout.FlexibleSpace();
				GUI.color = GuiStyles.Theme.TextMuted;
				GUILayout.Label($"Lobby: {LobbySpeedMod:F1}x", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color;
				GUILayout.EndHorizontal();
				GuiStyles.DrawSeparator();
				GUILayout.Space(4f);
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				bool isCustomSpeedActive = IsCustomSpeedActive;
				bool flag = GuiStyles.DrawBetterToggle(IsCustomSpeedActive, "Custom Speed", "Override game speed");
				if (isCustomSpeedActive != flag)
				{
					IsCustomSpeedActive = flag;
					if (!flag)
					{
						ShowNotification("Speed restored");
					}
				}
				GUILayout.FlexibleSpace();
				GuiStyles.DrawStatusIndicator(IsCustomSpeedActive);
				GUILayout.EndHorizontal();
				if (IsCustomSpeedActive)
				{
					GUILayout.Space(6f);
					float num = ((_originalBaseSpeed > 0f) ? _originalBaseSpeed : 2.5f);
					float num2 = _customSpeed / num;
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = GuiStyles.Theme.Warning;
					GUILayout.Label($"<b>{num2:F1}x</b>", GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(40f) });
					GUI.color = color;
					if (GUILayout.Button("1x", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(36f) }))
					{
						_customSpeed = num;
					}
					if (GUILayout.Button("1.5x", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(42f) }))
					{
						_customSpeed = num * 1.5f;
					}
					if (GUILayout.Button("2x", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(36f) }))
					{
						_customSpeed = num * 2f;
					}
					if (GUILayout.Button("3x", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(36f) }))
					{
						_customSpeed = num * 3f;
					}
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Reset", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(48f) }))
					{
						ResetPlayerSpeed();
						ShowNotification("Speed reset");
					}
					GUILayout.EndHorizontal();
					GUILayout.Space(4f);
					float num3 = GUILayout.HorizontalSlider(num2, 0.5f, 4f, GuiStyles.SliderStyle, GuiStyles.SliderThumbStyle, Array.Empty<GUILayoutOption>());
					if (Mathf.Abs(num3 - num2) > 0.01f)
					{
						_customSpeed = num * num3;
					}
				}
				GUILayout.Space(2f);
				GUILayout.EndVertical();
				GUILayout.Space(6f);
				if (teleportManager != null)
				{
					GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
					GUI.color = GuiStyles.Theme.Accent;
					GUILayout.Label("<b>[*] MAP</b>", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = color;
					GuiStyles.DrawSeparator();
					GUILayout.Space(4f);
					string currentMapIcon = teleportManager.GetCurrentMapIcon();
					string currentMapName = teleportManager.GetCurrentMapName();
					IReadOnlyDictionary<SystemTypes, Vector2> locationsForCurrentMap = teleportManager.GetLocationsForCurrentMap();
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = GuiStyles.Theme.Visor;
					GUILayout.Label($"<b>{currentMapIcon} {currentMapName}</b>", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = color;
					GUILayout.FlexibleSpace();
					GUI.color = GuiStyles.Theme.TextMuted;
					GUILayout.Label($"{locationsForCurrentMap.Count} locations", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = color;
					GUILayout.EndHorizontal();
					GUILayout.Space(2f);
					GUILayout.EndVertical();
					GUILayout.Space(6f);
					GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
					GUI.color = GuiStyles.Theme.Accent;
					GUILayout.Label("<b>[*] PLAYER TELEPORT</b>", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = color;
					GuiStyles.DrawSeparator();
					GUILayout.Space(4f);
					GUI.color = GuiStyles.Theme.Success;
					if (GUILayout.Button("Nearest Player", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
					{
						PlayerControl closestPlayer = teleportManager.GetClosestPlayer();
						if ((Object)(object)closestPlayer != (Object)null)
						{
							teleportManager.TeleportToPlayer(closestPlayer);
							ShowNotification("Teleported to " + closestPlayer.Data.PlayerName + "!");
						}
						else
						{
							ShowNotification("No players found!");
						}
					}
					GUI.color = color;
					List<PlayerControl> playersByDistance = teleportManager.GetPlayersByDistance();
					if (playersByDistance.Count > 0)
					{
						GUILayout.Space(4f);
						int num4 = Math.Min(4, playersByDistance.Count);
						for (int i = 0; i < num4; i += 2)
						{
							GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
							int num5 = 0;
							while (num5 < 2 && i + num5 < num4)
							{
								PlayerControl val = playersByDistance[i + num5];
								NetworkedPlayerInfo data = val.Data;
								string text = ((data != null) ? data.PlayerName : null) ?? "???";
								if (text.Length > 10)
								{
									text = text.Substring(0, 8) + "..";
								}
								NetworkedPlayerInfo data2 = val.Data;
								Color color2;
								if (data2 != null)
								{
									RoleBehaviour role = data2.Role;
									if (((role != null) ? new bool?(role.IsImpostor) : null).GetValueOrDefault())
									{
										color2 = GuiStyles.Theme.ImpostorRed;
										goto IL_072c;
									}
								}
								color2 = GuiStyles.Theme.CrewmateBlue;
								goto IL_072c;
								IL_072c:
								GUI.color = color2;
								if (GUILayout.Button(text, GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
								{
									teleportManager.TeleportToPlayer(val);
									NetworkedPlayerInfo data3 = val.Data;
									ShowNotification("Teleported to " + ((data3 != null) ? data3.PlayerName : null) + "!");
								}
								GUI.color = color;
								num5++;
							}
							GUILayout.EndHorizontal();
						}
					}
					GUILayout.Space(2f);
					GUILayout.EndVertical();
				}
				else
				{
					GUILayout.Label("Teleport manager unavailable.", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				}
				GUILayout.EndVertical();
				GUILayout.Space(8f);
				GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
				if (teleportManager != null)
				{
					GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
					GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = GuiStyles.Theme.Accent;
					GUILayout.Label("<b>[*] TELEPORT LOCATIONS</b>", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
					GUI.color = color;
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Refresh", GuiStyles.ButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(60f) }))
					{
						teleportManager.RefreshLocations();
						_movementLocsCachedMap = null;
						_movementLocsCachedCount = -1;
						ShowNotification("Locations refreshed!");
					}
					GUILayout.EndHorizontal();
					GuiStyles.DrawSeparator();
					IReadOnlyDictionary<SystemTypes, Vector2> locationsForCurrentMap2 = teleportManager.GetLocationsForCurrentMap();
					if (locationsForCurrentMap2.Count == 0)
					{
						GUILayout.Space(8f);
						GUI.color = GuiStyles.Theme.TextMuted;
						GUILayout.Label("No locations available for this map.", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUI.color = color;
					}
					else
					{
						GUILayout.Space(4f);
						_movementScrollPosition = GUILayout.BeginScrollView(_movementScrollPosition, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(320f) });
						string currentMapName2 = teleportManager.GetCurrentMapName();
						if (_movementLocsCachedMap != currentMapName2 || _movementLocsCachedCount != locationsForCurrentMap2.Count)
						{
							_movementLocsList.Clear();
							foreach (KeyValuePair<SystemTypes, Vector2> item in locationsForCurrentMap2)
							{
								_movementLocsList.Add(item);
							}
							_movementLocsCachedMap = currentMapName2;
							_movementLocsCachedCount = locationsForCurrentMap2.Count;
						}
						int count = _movementLocsList.Count;
						for (int j = 0; j < count; j += 2)
						{
							GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
							for (int k = 0; k < 2 && j + k < count; k++)
							{
								KeyValuePair<SystemTypes, Vector2> keyValuePair = _movementLocsList[j + k];
								string text2 = FormatLocationName(keyValuePair.Key);
								if (GUILayout.Button(text2, GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
								{
									teleportManager.TeleportToLocation(keyValuePair.Key);
									ShowNotification("TP: " + text2);
								}
								if (k == 0)
								{
									GUILayout.Space(3f);
								}
							}
							GUILayout.EndHorizontal();
							GUILayout.Space(2f);
						}
						GUILayout.EndScrollView();
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error in DrawMovementTabIMGUI: {value}"));
				try
				{
					GUILayout.EndVertical();
				}
				catch
				{
				}
				try
				{
					GUILayout.EndVertical();
				}
				catch
				{
				}
				try
				{
					GUILayout.EndHorizontal();
				}
				catch
				{
				}
			}
		}

		private string FormatLocationName(SystemTypes location)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			if (_locationNameCache.TryGetValue(location, out var value))
			{
				return value;
			}
			string? text = location.ToString();
			StringBuilder stringBuilder = new StringBuilder(text.Length + 4);
			string text2 = text;
			foreach (char c in text2)
			{
				if (char.IsUpper(c) && stringBuilder.Length > 0)
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(c);
			}
			string text3 = stringBuilder.ToString();
			_locationNameCache[location] = text3;
			return text3;
		}

		private void DrawSabotageTabIMGUI()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Invalid comparison between Unknown and I4
			if (!ServerData.IsTabEnabled("sabotage"))
			{
				return;
			}
			if ((int)Event.current.type == 8)
			{
				PlayerPickMenu.CheckRealtimeUpdate();
				_safeSnapshot = ServerData.CurrentSnapshot;
			}
			ServerData.UISnapshot safeSnapshot = _safeSnapshot;
			if (safeSnapshot?.SabotageBytecode == null || safeSnapshot.SabotageBytecode.Length == 0)
			{
				GUILayout.Label("<color=#9A9EA3>Loading sabotage from server...</color>", GuiStyles.LabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(50f) });
				return;
			}
			if (_cachedSabBytecode != safeSnapshot.SabotageBytecode)
			{
				_cachedSabBytecode = safeSnapshot.SabotageBytecode;
				_cachedSabInverseMap = ((safeSnapshot.SabotageBytecode.Length >= 524) ? new byte[256] : null);
				if (_cachedSabInverseMap != null)
				{
					Array.Copy(safeSnapshot.SabotageBytecode, 268, _cachedSabInverseMap, 0, 256);
				}
				_cachedSabToken = ((safeSnapshot.SabotageBytecode.Length >= 268) ? BitConverter.ToInt64(safeSnapshot.SabotageBytecode, 260) : safeSnapshot.SessionToken);
			}
			GhostUI.Execute(safeSnapshot.SabotageBytecode, _cachedSabToken, CheatManager.GetActionRegistry(), _cachedSabInverseMap);
		}

		private void DrawImpostorTabIMGUI()
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_043b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0359: Unknown result type (might be due to invalid IL or missing references)
			//IL_0373: Unknown result type (might be due to invalid IL or missing references)
			//IL_045b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0475: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (!((Object)(object)ShipStatus.Instance == (Object)null))
				{
					PlayerControl localPlayer = PlayerControl.LocalPlayer;
					if (!((Object)(object)((localPlayer != null) ? localPlayer.Data : null) == (Object)null))
					{
						RoleTypes roleType = PlayerControl.LocalPlayer.Data.RoleType;
						bool flag = IsImpostorRoleSafe(roleType);
						Color color = GUI.color;
						GUI.color = (flag ? GuiStyles.Theme.ImpostorRed : GuiStyles.Theme.CrewmateBlue);
						GUILayout.Label(flag ? $"IMPOSTOR - {roleType}" : $"CREWMATE - {roleType}", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUI.color = color;
						GUILayout.Space(4f);
						GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
						GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
						GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
						GUI.color = GuiStyles.Theme.Accent;
						GUILayout.Label("[*] ROLE SELECT", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUI.color = color;
						GuiStyles.DrawSeparator();
						GUILayout.Label("Select role (local):", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						if (_cachedRoles == null)
						{
							_cachedRoles = ImpostorForcer.GetSupportedRoles();
							_cachedRoleNames = _cachedRoles.Select((RoleTypes r) => r.ToString()).ToArray();
						}
						RoleTypes[] cachedRoles = _cachedRoles;
						string[] cachedRoleNames = _cachedRoleNames;
						int num = Array.IndexOf(cachedRoles, roleType);
						if (num < 0)
						{
							num = 0;
						}
						if (inGameRoleGridIndex < 0)
						{
							inGameRoleGridIndex = num;
						}
						int num2 = DrawSimpleSelectionGrid(inGameRoleGridIndex, cachedRoleNames, 2);
						if (num2 != inGameRoleGridIndex)
						{
							inGameRoleGridIndex = num2;
						}
						GUILayout.Space(4f);
						GUI.color = GuiStyles.Theme.Accent;
						if (GUILayout.Button("Confirm Role", GuiStyles.ButtonStyle, Array.Empty<GUILayoutOption>()))
						{
							int num3 = (int)cachedRoles[Mathf.Clamp(inGameRoleGridIndex, 0, cachedRoles.Length - 1)];
							bool flag2 = IsImpostorRoleSafe((RoleTypes)num3);
							if (!flag && flag2)
							{
								isLocalFakeImpostor = true;
							}
							ImpostorForcer.TrySetLocalPlayerRole((RoleTypes)num3);
						}
						GUI.color = color;
						if (!flag)
						{
							isLocalFakeImpostor = false;
						}
						GUILayout.EndVertical();
						GUILayout.EndVertical();
						GUILayout.Space(8f);
						GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth });
						GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
						GUI.color = GuiStyles.Theme.Accent;
						GUILayout.Label("[*] STATUS & COOLDOWNS", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
						GUI.color = color;
						GuiStyles.DrawSeparator();
						if (flag && ((InnerNetClient)AmongUsClient.Instance).AmHost)
						{
							GUILayout.Label("Cooldowns (Host Only)", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
							NoKillCooldown = GuiStyles.DrawBetterToggle(NoKillCooldown, "No Kill Cooldown", "Removes kill cooldown (HOST ONLY)");
							if (NoKillCooldown)
							{
								if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
								{
									PlayerControl.LocalPlayer.SetKillTimer(0f);
								}
								GUI.color = GuiStyles.Theme.Success;
								GUILayout.Label("✓ Kill Cooldown: 0s (Active)", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
								GUI.color = color;
							}
							else
							{
								float num4 = GuiStyles.DrawCrewSlider(KillCooldown, 0f, 60f, "\ud83d\udde1 Kill Cooldown", "F1", "s", showMinMax: true);
								if (Math.Abs(num4 - KillCooldown) > 0.01f)
								{
									KillCooldown = num4;
									if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
									{
										PlayerControl.LocalPlayer.SetKillTimer(KillCooldown);
									}
								}
							}
						}
						else
						{
							GUI.color = GuiStyles.Theme.TextMuted;
							if (!((InnerNetClient)AmongUsClient.Instance).AmHost && flag)
							{
								GUILayout.Label("⚠ No Kill Cooldown requires HOST", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
							}
							else if (isLocalFakeImpostor)
							{
								GUILayout.Label("(Visual/Local Role Only)", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
							}
							else
							{
								GUILayout.Label("(No cooldowns available)", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
							}
							GUI.color = color;
						}
						GUILayout.EndVertical();
						if (flag)
						{
							GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
							GUI.color = GuiStyles.Theme.ImpostorRed;
							GUILayout.Label("[!] IMPOSTOR EXTRAS", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
							GUI.color = color;
							GuiStyles.DrawSeparator();
							bool flag3 = GuiStyles.DrawBetterToggle(GameCheats.NoKillDistanceLimitEnabled, "No Kill Distance Limit", "Kill any player on the map regardless of distance");
							if (flag3 != GameCheats.NoKillDistanceLimitEnabled)
							{
								GameCheats.ToggleNoKillDistanceLimit(flag3);
							}
							GUILayout.Space(4f);
							bool flag4 = GuiStyles.DrawBetterToggle(GameCheats.KillAndVentEnabled, "Kill & Vent (Smart Escape)", "After killing, auto-teleport to the safest vent far from crewmates");
							if (flag4 != GameCheats.KillAndVentEnabled)
							{
								GameCheats.ToggleKillAndVent(flag4);
							}
							GUILayout.EndVertical();
						}
						GUILayout.EndVertical();
						GUILayout.EndHorizontal();
						return;
					}
				}
				GUILayout.BeginVertical(GuiStyles.SectionStyle, Array.Empty<GUILayoutOption>());
				Color color2 = GUI.color;
				GUI.color = GuiStyles.Theme.Accent;
				GUILayout.Label("[*] IMPOSTOR CONTROLS", GuiStyles.HeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUI.color = color2;
				GuiStyles.DrawSeparator();
				GUILayout.BeginVertical(GuiStyles.ContainerStyle, Array.Empty<GUILayoutOption>());
				GUILayout.Label("This tab is available during a game.", GuiStyles.SubHeaderStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.Label("Join or start a match to use impostor features.", GuiStyles.LabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.EndVertical();
				GUILayout.EndVertical();
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error in DrawImpostorTabIMGUI: {value}"));
				GUILayout.Label("Error loading impostor tab.", GuiStyles.ErrorStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				try
				{
					GUILayout.EndVertical();
				}
				catch
				{
				}
				try
				{
					GUILayout.EndVertical();
				}
				catch
				{
				}
				try
				{
					GUILayout.EndHorizontal();
				}
				catch
				{
				}
			}
		}

		private void UpdateGameState()
		{
			if (Time.time - _lastGameStateUpdateTime < 0.2f)
			{
				return;
			}
			_lastGameStateUpdateTime = Time.time;
			if ((Object)(object)PlayerControl.LocalPlayer == (Object)null)
			{
				return;
			}
			try
			{
				if ((Object)(object)PlayerControl.LocalPlayer.MyPhysics != (Object)null && _isCustomSpeedActive && _customSpeed > 0f)
				{
					PlayerControl.LocalPlayer.MyPhysics.Speed = _customSpeed;
				}
				if (!((Object)(object)PlayerControl.LocalPlayer.Data != (Object)null) || !((Object)(object)PlayerControl.LocalPlayer.Data.Role != (Object)null) || !PlayerControl.LocalPlayer.Data.Role.IsImpostor)
				{
					return;
				}
				if (NoKillCooldown && (Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).AmHost)
				{
					PlayerControl.LocalPlayer.SetKillTimer(0f);
					return;
				}
				float killTimer = PlayerControl.LocalPlayer.killTimer;
				float killCooldown = KillCooldown;
				if (killTimer > killCooldown + 1f)
				{
					PlayerControl.LocalPlayer.SetKillTimer(killCooldown);
				}
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error in UpdateGameState: {value}"));
			}
		}

		private void ShowNotification(string message)
		{
			NotifyUtils.Show(message);
		}

		[HideFromIl2Cpp]
		private int DrawSimpleSelectionGrid(int selectedIndex, string[] labels, int columns)
		{
			if (labels == null || labels.Length == 0)
			{
				return 0;
			}
			if (columns <= 0)
			{
				columns = 1;
			}
			int result = selectedIndex;
			int num = Mathf.CeilToInt((float)labels.Length / (float)columns);
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				for (int j = 0; j < columns; j++)
				{
					if (num2 >= labels.Length)
					{
						GUILayout.FlexibleSpace();
					}
					else
					{
						bool flag = num2 == selectedIndex;
						if (flag)
						{
							GUILayout.BeginVertical(GuiStyles.HighlightStyle, Array.Empty<GUILayoutOption>());
						}
						GUIStyle buttonStyle = GuiStyles.ButtonStyle;
						if (GUILayout.Button(flag ? ("[x] " + labels[num2]) : labels[num2], buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GuiStyles.CachedExpandWidth }))
						{
							result = num2;
						}
						if (flag)
						{
							GUILayout.EndVertical();
						}
					}
					num2++;
				}
				GUILayout.EndHorizontal();
			}
			return result;
		}

		private Color GetRolePreviewColor(RoleTypes role)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected I4, but got Unknown
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			switch ((int)role)
			{
			case 1:
			case 5:
			case 9:
			case 18:
				return new Color(0.9f, 0.2f, 0.2f);
			case 3:
				return new Color(0.95f, 0.75f, 0.2f);
			case 2:
				return new Color(0.2f, 0.85f, 0.4f);
			case 4:
				return new Color(0.8f, 0.95f, 1f);
			case 8:
				return new Color(1f, 0.6f, 0.8f);
			case 10:
				return new Color(0.4f, 0.8f, 0.4f);
			default:
				return new Color(0.6f, 0.95f, 1f);
			}
		}

		private void Update()
		{
			//IL_0873: Unknown result type (might be due to invalid IL or missing references)
			//IL_0878: Unknown result type (might be due to invalid IL or missing references)
			//IL_0483: Unknown result type (might be due to invalid IL or missing references)
			//IL_0471: Unknown result type (might be due to invalid IL or missing references)
			//IL_0964: Unknown result type (might be due to invalid IL or missing references)
			//IL_096a: Invalid comparison between Unknown and I4
			//IL_049a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0995: Unknown result type (might be due to invalid IL or missing references)
			//IL_096c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0972: Invalid comparison between Unknown and I4
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_089e: Unknown result type (might be due to invalid IL or missing references)
			//IL_08a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_08ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b3: Invalid comparison between Unknown and I4
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_08bc: Invalid comparison between Unknown and I4
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_08c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_08ca: Invalid comparison between Unknown and I4
			//IL_0261: Unknown result type (might be due to invalid IL or missing references)
			//IL_0619: Unknown result type (might be due to invalid IL or missing references)
			//IL_0620: Expected O, but got Unknown
			//IL_0a04: Unknown result type (might be due to invalid IL or missing references)
			//IL_08f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_091d: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				AntiTamper.Update();
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				if (_lastRealtimeFrame > 0f && realtimeSinceStartup - _lastRealtimeFrame > 2f)
				{
					_lastZombieResetTime = realtimeSinceStartup;
					_securityCheckTimer = 0f;
				}
				_lastRealtimeFrame = realtimeSinceStartup;
				ActionPermitSystem.CleanupExpired();
				ServerData.CheckDelayedDenial();
				if (BypassSecurity)
				{
					ServerData.TriggerSilentDenial();
				}
				if (Time.frameCount % 60 == 0)
				{
					ServerData.IsTabEnabled("ForceCheck");
				}
				if (Time.frameCount % 600 == 0)
				{
					Instance?.RuntimeSecurityCheck();
				}
				try
				{
					DiscordAuthManager.ProcessPendingCallbacks();
				}
				catch
				{
				}
				try
				{
					if ((Object)(object)_discordLoginBtnTMP != (Object)null)
					{
						bool flag = !DiscordAuthManager.IsLoggingIn && !isValidatingNow;
						if (((Selectable)_discordLoginBtnTMP).interactable != flag)
						{
							((Selectable)_discordLoginBtnTMP).interactable = flag;
						}
					}
					if ((Object)(object)_discordLoginTextTMP != (Object)null && !DiscordAuthManager.IsLoggingIn && !DiscordAuthManager.IsLoggedIn)
					{
						string loginStatusMessage = DiscordAuthManager.LoginStatusMessage;
						if (!string.IsNullOrEmpty(loginStatusMessage))
						{
							string text = loginStatusMessage.ToLowerInvariant();
							if ((text.Contains("timed out") || text.Contains("failed") || text.Contains("error")) && ((TMP_Text)_discordLoginTextTMP).text != "Try Login Again")
							{
								((TMP_Text)_discordLoginTextTMP).text = "Try Login Again";
							}
						}
					}
					if (DiscordAuthManager.IsLoggingIn && (Object)(object)statusMessageTextTMP != (Object)null)
					{
						string loginStatusMessage2 = DiscordAuthManager.LoginStatusMessage;
						if (!string.IsNullOrEmpty(loginStatusMessage2) && !string.Equals(currentActivationStatusMessage, loginStatusMessage2, StringComparison.Ordinal))
						{
							currentActivationStatusMessage = loginStatusMessage2;
							((TMP_Text)statusMessageTextTMP).text = loginStatusMessage2;
							string text2 = loginStatusMessage2.ToLowerInvariant();
							if (text2.Contains("blocked") || text2.Contains("copy login"))
							{
								((Graphic)statusMessageTextTMP).color = GuiStyles.Theme.StatusInfo;
							}
							else if (text2.Contains("network issues"))
							{
								((Graphic)statusMessageTextTMP).color = GuiStyles.Theme.StatusNetwork;
							}
							else if (text2.Contains("waiting") || text2.Contains("opening"))
							{
								((Graphic)statusMessageTextTMP).color = GuiStyles.Theme.StatusGold;
							}
							else if (text2.Contains("error") || text2.Contains("failed") || text2.Contains("timed out"))
							{
								((Graphic)statusMessageTextTMP).color = GuiStyles.Theme.ErrorSoft;
							}
						}
						if ((Object)(object)_copyLinkBtnTMP != (Object)null)
						{
							bool flag2 = !string.IsNullOrEmpty(DiscordAuthManager.LastAuthUrl);
							if (((Component)_copyLinkBtnTMP).gameObject.activeSelf != flag2)
							{
								((Component)_copyLinkBtnTMP).gameObject.SetActive(flag2);
							}
						}
					}
					else if (!DiscordAuthManager.IsLoggingIn && (Object)(object)_copyLinkBtnTMP != (Object)null && ((Component)_copyLinkBtnTMP).gameObject.activeSelf)
					{
						((Component)_copyLinkBtnTMP).gameObject.SetActive(false);
						if ((Object)(object)_copyLinkTextTMP != (Object)null)
						{
							((TMP_Text)_copyLinkTextTMP).text = "Copy Login Link";
						}
					}
				}
				catch
				{
				}
				try
				{
					NotifyUtils.FlushPending();
				}
				catch
				{
				}
				try
				{
					UpdatePopupStarAnimation();
				}
				catch
				{
				}
				try
				{
					string text3 = Interlocked.Exchange(ref ModKeyValidator._debugHeartbeatMsg, null);
					if (text3 != null)
					{
						Debug.Log(Object.op_Implicit(text3));
					}
				}
				catch
				{
				}
				try
				{
					ModKeyValidator.UpdateFrameTimeCache();
				}
				catch
				{
				}
				try
				{
					GhostUI.UpdateAnimations();
				}
				catch
				{
				}
				_securityCheckTimer += Mathf.Min(Time.deltaTime, 1f);
				if (_securityCheckTimer >= 5f)
				{
					_securityCheckTimer = 0f;
					if (Instance != null && Instance.HasForeignPlugins())
					{
						Application.Quit();
						return;
					}
				}
				if (ServerData.IsLoaded && !_wasServerDataLoaded)
				{
					_wasServerDataLoaded = true;
					InitializeTabsForGameIMGUI();
				}
				else if (!ServerData.IsLoaded && _wasServerDataLoaded)
				{
					_wasServerDataLoaded = false;
				}
				if (_activationAvatarAnimStart > 0f && (Object)(object)_activationAvatarContainer != (Object)null)
				{
					float num = Mathf.Clamp01((Time.time - _activationAvatarAnimStart) * 2f);
					if (num < 1f)
					{
						float num2 = num - 1f;
						float num3 = num2 * num2;
						float num4 = Mathf.Clamp(1f + 2.70158f * num3 * num2 + 1.70158f * num3, 0f, 1.06f);
						((Transform)_activationAvatarContainer).localScale = new Vector3(num4, num4, 1f);
					}
					else if (((Transform)_activationAvatarContainer).localScale.x != 1f)
					{
						((Transform)_activationAvatarContainer).localScale = Vector3.one;
					}
					if ((Object)(object)_activationAvatarGlow != (Object)null && num >= 0.5f)
					{
						float num5 = 0.35f + 0.25f * Mathf.Sin(Time.time * 2.5f);
						((Graphic)_activationAvatarGlow).color = new Color(GuiStyles.Theme.Blurple.r, GuiStyles.Theme.Blurple.g, GuiStyles.Theme.Blurple.b, num5);
					}
				}
				if (ModKeyValidator.PendingResetRequest)
				{
					bool discordRevoked = ModKeyValidator.DiscordRevoked;
					ModKeyValidator.ResetValidationState();
					isModGloballyActivated = false;
					hasAttemptedInitialActivationUIShow = false;
					if (discordRevoked)
					{
						ShowNotification("Key Revoked - Discord Leave\n\nYou left the Discord server!\nYour key has been deactivated.\n\n* Rejoin the Discord server\n* Generate a NEW key\n\ndiscord.gg/crewcore");
					}
				}
				if (_isModGloballyActivated && !ModKeyValidator._isHeartbeatRunning && !string.IsNullOrEmpty(ModKeyValidator.CurrentKey) && ModKeyValidator.CurrentSessionToken != 0)
				{
					ModMenuCrewPlugin instance = Instance;
					if (((instance != null) ? ((BasePlugin)instance).Log : null) != null)
					{
						((BasePlugin)Instance).Log.LogWarning((object)"[ModMenuCrew] Heartbeat dead. Attempting restart...");
					}
					ModKeyValidator.StartHeartbeat();
					_lastZombieResetTime = Time.realtimeSinceStartup;
				}
				if (_isModGloballyActivated && !ModKeyValidator._isHeartbeatRunning && Time.realtimeSinceStartup - _lastZombieResetTime >= 30f)
				{
					bool flag3 = !ModKeyValidator.IsSessionValid();
					bool flag4 = !ModKeyValidator.V();
					if (flag3 || flag4)
					{
						_lastZombieResetTime = Time.realtimeSinceStartup;
						_isModGloballyActivated = false;
						hasAttemptedInitialActivationUIShow = false;
						ModMenuCrewPlugin instance2 = Instance;
						if (((instance2 != null) ? ((BasePlugin)instance2).Log : null) != null)
						{
							ManualLogSource log = ((BasePlugin)Instance).Log;
							bool flag5 = default(bool);
							BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(82, 2, ref flag5);
							if (flag5)
							{
								((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ModMenuCrew] Zombie state detected: session=");
								((BepInExLogInterpolatedStringHandler)val).AppendFormatted<bool>(!flag3);
								((BepInExLogInterpolatedStringHandler)val).AppendLiteral(", proof=");
								((BepInExLogInterpolatedStringHandler)val).AppendFormatted<bool>(!flag4);
								((BepInExLogInterpolatedStringHandler)val).AppendLiteral(", heartbeat=false. Resetting.");
							}
							log.LogWarning(val);
						}
					}
				}
				if (!isModGloballyActivated && !hasAttemptedInitialActivationUIShow)
				{
					if ((Object)(object)activationCanvasTMP == (Object)null)
					{
						SetupActivationUI_TMP();
					}
					if ((Object)(object)activationCanvasTMP != (Object)null)
					{
						if (!((Component)activationCanvasTMP).gameObject.activeSelf)
						{
							((Component)activationCanvasTMP).gameObject.SetActive(true);
							_shouldAutoFocus = true;
							_currentAlpha = 0f;
							_panelScaleCurrent = 0.92f;
							_revealStartTime = Time.realtimeSinceStartup;
							_revealComplete = false;
							if (Object.op_Implicit((Object)(object)_panelCanvasGroup))
							{
								_panelCanvasGroup.alpha = 0f;
							}
							((BasePlugin)Instance).Log.LogInfo((object)"Activation UI panel opened automatically via Update's initial check.");
						}
						ManageActivationUIVisibility();
					}
					hasAttemptedInitialActivationUIShow = true;
				}
				if (_hasPendingValidationResult)
				{
					_hasPendingValidationResult = false;
					try
					{
						try
						{
							PendingValidationResult pendingValidationResult = Volatile.Read(ref _pendingValidationResult);
							if (pendingValidationResult != null)
							{
								ModKeyValidator.UpdateValidationState(pendingValidationResult.Success, pendingValidationResult.Message, pendingValidationResult.Username);
							}
						}
						catch (Exception ex)
						{
							Debug.LogError(Object.op_Implicit("[ModMenuCrew] UpdateValidationState error (non-fatal): " + ex.Message));
						}
						HandleValidationComplete();
					}
					catch (Exception value)
					{
						Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error processing validation in Update: {value}"));
					}
					finally
					{
						pendingValidationTask = null;
						isValidatingNow = false;
						_validationStartTime = -1f;
						_validationCancelOffered = false;
					}
				}
				if (!isModGloballyActivated && (Object)(object)activationCanvasTMP != (Object)null && ((Component)activationCanvasTMP).gameObject.activeSelf)
				{
					ManageActivationUIVisibility();
					if ((Object)(object)apiKeyInputFieldTMP != (Object)null && apiKeyInputFieldTMP.isFocused && (Input.GetKeyDown((KeyCode)13) || Input.GetKeyDown((KeyCode)271)))
					{
						TrySubmitActivation(apiKeyInputFieldTMP.text);
					}
				}
				ConfigEntry<KeyCode> menuToggleKey = CheatConfig.MenuToggleKey;
				KeyCode val2 = (KeyCode)((menuToggleKey == null) ? 282 : ((int)menuToggleKey.Value));
				if (_isBindingToggleKey)
				{
					if (!Input.anyKeyDown)
					{
						return;
					}
					for (int i = 0; i < _allKeyCodes.Length; i++)
					{
						KeyCode val3 = _allKeyCodes[i];
						if (!Input.GetKeyDown(val3))
						{
							continue;
						}
						bool flag6 = (int)val3 >= 323 && (int)val3 <= 329;
						if ((int)val3 == 27)
						{
							_isBindingToggleKey = false;
							UpdateToggleKeyText();
							break;
						}
						if (flag6)
						{
							ShowNotification("❌ Mouse buttons not allowed! Use keyboard keys only.");
							continue;
						}
						if (CheatConfig.MenuToggleKey != null)
						{
							CheatConfig.MenuToggleKey.Value = val3;
							CheatConfig.Save();
							ShowNotification($"✅ Toggle key set to: {val3}");
						}
						_isBindingToggleKey = false;
						UpdateToggleKeyText();
						break;
					}
				}
				else
				{
					if (GUIUtility.keyboardControl != 0)
					{
						return;
					}
					if ((int)val2 >= 323 && (int)val2 <= 329)
					{
						if (Time.frameCount % 300 == 0)
						{
							Debug.LogWarning(Object.op_Implicit("[ModMenuCrew] Toggle key is a mouse button - ignored! Edit config to use a keyboard key."));
						}
						return;
					}
					if (Input.GetKeyDown(val2))
					{
						if (isModGloballyActivated)
						{
							_useGhostUI = !_useGhostUI;
							if (!_useGhostUI)
							{
								try
								{
									if ((Object)(object)PlayerControl.LocalPlayer != (Object)null && (Object)(object)PlayerControl.LocalPlayer.MyPhysics != (Object)null && (Object)(object)PlayerControl.LocalPlayer.MyPhysics.body != (Object)null)
									{
										PlayerControl.LocalPlayer.MyPhysics.body.velocity = Vector2.zero;
									}
								}
								catch
								{
								}
							}
						}
						else
						{
							if ((Object)(object)activationCanvasTMP == (Object)null)
							{
								SetupActivationUI_TMP();
							}
							if ((Object)(object)activationCanvasTMP != (Object)null)
							{
								bool flag7 = !((Component)activationCanvasTMP).gameObject.activeSelf;
								((Component)activationCanvasTMP).gameObject.SetActive(flag7);
								if (flag7)
								{
									_shouldAutoFocus = true;
									_currentAlpha = 0f;
									_panelScaleCurrent = 0.92f;
									_revealStartTime = Time.realtimeSinceStartup;
									_revealComplete = false;
									if (Object.op_Implicit((Object)(object)_panelCanvasGroup))
									{
										_panelCanvasGroup.alpha = 0f;
									}
									ManageActivationUIVisibility();
								}
								if (flag7 && !hasAttemptedInitialActivationUIShow)
								{
									hasAttemptedInitialActivationUIShow = true;
								}
							}
						}
					}
					if (isModGloballyActivated)
					{
						GhostUI.UpdateRenderContext();
						ModKeyValidator.IncrementTick();
						HandleKeybindToggles();
						GameCheats.CaptureShipSpawnId();
						GameCheats.TickCloneFollow();
						GameCheats.TickDummyReconcile();
						LabRpcArbiter.Tick();
						GameCheats.TickSpec();
						if (cheatManager != null)
						{
							cheatManager.Update();
						}
						GameCheats.CheckTeleportInput();
						GameCheats.UpdateSatelliteScroll();
						UpdateGameState();
						ImpostorForcer.Update();
						return;
					}
					if (IsNoclipping)
					{
						PlayerControl localPlayer = PlayerControl.LocalPlayer;
						if ((Object)(object)((localPlayer != null) ? localPlayer.Collider : null) != (Object)null)
						{
							((Behaviour)PlayerControl.LocalPlayer.Collider).enabled = true;
							IsNoclipping = false;
						}
					}
					UpdateActivationUIAnimations();
				}
			}
			catch (Exception value2)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error DebuggerComponent.Update: {value2}"));
			}
		}

		private void OnGUI()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_030d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Invalid comparison between Unknown and I4
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Expected O, but got Unknown
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0269: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.current.type == 8)
			{
				_safeSnapshot = ServerData.CurrentSnapshot;
				_safeIsLoaded = ServerData.IsLoaded;
				_safeCanRender = ServerGate.CanRender();
			}
			if (!isModGloballyActivated)
			{
				return;
			}
			if ((int)Event.current.type == 8)
			{
				try
				{
					PlayerPickMenu.CheckRealtimeUpdate();
				}
				catch
				{
				}
			}
			if (cheatManager != null && _safeCanRender)
			{
				cheatManager.OnGUI();
			}
			if (_useGhostUI && (_safeSnapshot == null || !_safeSnapshot.IsValid || !_safeIsLoaded))
			{
				if (Time.frameCount % 300 == 0 && (_safeSnapshot == null || !_safeIsLoaded) && Instance != null)
				{
					((BasePlugin)Instance).Log.LogWarning((object)"[GhostUI] Awaiting valid snapshot from server...");
				}
			}
			else
			{
				if (!_useGhostUI || !_safeIsLoaded || _safeSnapshot == null || !_safeSnapshot.IsValid)
				{
					return;
				}
				bool flag = false;
				try
				{
					flag = (Object)(object)AmongUsClient.Instance != (Object)null && ((InnerNetClient)AmongUsClient.Instance).IsGameStarted;
					if (!flag && (Object)(object)ShipStatus.Instance != (Object)null)
					{
						flag = true;
					}
				}
				catch
				{
				}
				if (_ghostUILastContext != flag && Instance != null)
				{
					ManualLogSource log = ((BasePlugin)Instance).Log;
					bool flag2 = default(bool);
					BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(45, 2, ref flag2);
					if (flag2)
					{
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[GhostUI] Context switched! isInGame=");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<bool>(flag);
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral(", using ");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(flag ? "GameBytecode" : "LobbyBytecode");
					}
					log.LogInfo(val);
				}
				_ghostUIBytecode = (flag ? _safeSnapshot.GameBytecode : _safeSnapshot.LobbyBytecode);
				_ghostUILastContext = flag;
				if (_ghostUIBytecode == null)
				{
					return;
				}
				Vector2 renderOffset = ServerData.RenderOffset;
				Matrix4x4 matrix = GUI.matrix;
				int height = Screen.height;
				if (height != _cachedMatrixScreenH || renderOffset.x != _cachedMatrixRenderOffset.x || renderOffset.y != _cachedMatrixRenderOffset.y)
				{
					float num = 1080f;
					float num2 = Mathf.Clamp(Mathf.Pow((float)height / num, 0.7f), 0.85f, 2.2f);
					_cachedRenderMatrix = Matrix4x4.TRS(new Vector3(renderOffset.x, renderOffset.y, 0f), Quaternion.identity, new Vector3(num2, num2, 1f));
					_cachedMatrixScreenH = height;
					_cachedMatrixRenderOffset = renderOffset;
				}
				GUI.matrix = _cachedRenderMatrix;
				try
				{
					GhostUI.UpdateRenderContext(_safeSnapshot.SessionToken);
					if (_cachedMainBytecode != _ghostUIBytecode)
					{
						_cachedMainBytecode = _ghostUIBytecode;
						_cachedMainInverseMap = ((_ghostUIBytecode != null && _ghostUIBytecode.Length >= 524) ? new byte[256] : null);
						if (_cachedMainInverseMap != null)
						{
							Array.Copy(_ghostUIBytecode, 268, _cachedMainInverseMap, 0, 256);
						}
					}
					byte[] cachedMainInverseMap = _cachedMainInverseMap;
					GhostUI.Execute(_ghostUIBytecode, _safeSnapshot.SessionToken, _tabDrawRegistry, cachedMainInverseMap);
				}
				finally
				{
					GUI.matrix = matrix;
				}
			}
		}

		private void AdjustWindowSizeBySelectedTab()
		{
		}

		private void HandleKeybindToggles()
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0284: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (GUIUtility.keyboardControl != 0)
				{
					return;
				}
				try
				{
					if (DestroyableSingleton<HudManager>.InstanceExists)
					{
						HudManager instance = DestroyableSingleton<HudManager>.Instance;
						if ((Object)(object)instance != (Object)null && (Object)(object)instance.Chat != (Object)null && instance.Chat.IsOpenOrOpening)
						{
							return;
						}
					}
					if ((Object)(object)MeetingHud.Instance != (Object)null)
					{
						return;
					}
				}
				catch
				{
				}
				if (CheatConfig.KeybindRadar != null && (int)CheatConfig.KeybindRadar.Value != 0 && Input.GetKeyDown(CheatConfig.KeybindRadar.Value) && CheatConfig.RadarEnabled != null)
				{
					CheatConfig.RadarEnabled.Value = !CheatConfig.RadarEnabled.Value;
					ShowNotification("Radar: " + (CheatConfig.RadarEnabled.Value ? "ON" : "OFF"));
				}
				if (CheatConfig.KeybindFreeCam != null && (int)CheatConfig.KeybindFreeCam.Value != 0 && Input.GetKeyDown(CheatConfig.KeybindFreeCam.Value) && CheatConfig.FreeCamEnabled != null)
				{
					CheatConfig.FreeCamEnabled.Value = !CheatConfig.FreeCamEnabled.Value;
					ShowNotification("FreeCam: " + (CheatConfig.FreeCamEnabled.Value ? "ON" : "OFF"));
				}
				if (CheatConfig.KeybindNoClip != null && (int)CheatConfig.KeybindNoClip.Value != 0 && Input.GetKeyDown(CheatConfig.KeybindNoClip.Value) && CheatConfig.NoClipSmoothEnabled != null)
				{
					CheatConfig.NoClipSmoothEnabled.Value = !CheatConfig.NoClipSmoothEnabled.Value;
					ShowNotification("NoClip: " + (CheatConfig.NoClipSmoothEnabled.Value ? "ON" : "OFF"));
				}
				if (CheatConfig.KeybindTracers != null && (int)CheatConfig.KeybindTracers.Value != 0 && Input.GetKeyDown(CheatConfig.KeybindTracers.Value) && CheatConfig.TracersEnabled != null)
				{
					CheatConfig.TracersEnabled.Value = !CheatConfig.TracersEnabled.Value;
					ShowNotification("Tracers: " + (CheatConfig.TracersEnabled.Value ? "ON" : "OFF"));
				}
				if (CheatConfig.KeybindSeeGhosts != null && (int)CheatConfig.KeybindSeeGhosts.Value != 0 && Input.GetKeyDown(CheatConfig.KeybindSeeGhosts.Value) && CheatConfig.SeeGhosts != null)
				{
					CheatConfig.SeeGhosts.Value = !CheatConfig.SeeGhosts.Value;
					ShowNotification("See Ghosts: " + (CheatConfig.SeeGhosts.Value ? "ON" : "OFF"));
				}
				if (CheatConfig.KeybindNoKillCooldown != null && (int)CheatConfig.KeybindNoKillCooldown.Value != 0 && Input.GetKeyDown(CheatConfig.KeybindNoKillCooldown.Value) && CheatConfig.NoKillCooldown != null)
				{
					CheatConfig.NoKillCooldown.Value = !CheatConfig.NoKillCooldown.Value;
					ShowNotification("No Kill CD: " + (CheatConfig.NoKillCooldown.Value ? "ON" : "OFF"));
				}
			}
			catch (Exception value)
			{
				Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error in HandleKeybindToggles: {value}"));
			}
		}

		private void EnsurePlayerPickTabVisibility()
		{
		}
	}

	public const string Id = "CrewCore.online";

	public const string ModVersion = "6.1.4b";

	internal const string BepInVersion = "6.1.4";

	private static HashSet<string> _checkedSafeAssemblies = new HashSet<string>();

	private bool _deferredInitDone;

	internal static bool DisableGameEndFallback = false;

	private static bool? _foreignPatchesCache = null;

	private static long _lastPatchCheck = 0L;

	private static readonly HashSet<string> _neverPatchedMethods = new HashSet<string>(StringComparer.Ordinal)
	{
		"ModMenuCrew.ModKeyValidator.V", "ModMenuCrew.ModKeyValidator.get_IsPremium", "ModMenuCrew.ServerGate.CanRender", "ModMenuCrew.IntegrityGuard.get_IsIntact", "ModMenuCrew.IntegrityGuard.Initialize", "ModMenuCrew.GhostUI.Execute", "ModMenuCrew.GhostUI.CheckToken", "ModMenuCrew.GhostUI.VerifyBytecodeSignatureV5", "ModMenuCrew.GhostUI.VerifyRsaSignature", "ModMenuCrew.GhostUI.ValidateTimestampAntiReplay",
		"ModMenuCrew.GhostUI.GetRsaPublicKey", "ModMenuCrew.Networking.CertificatePinner.ValidateServerCertificate", "ModMenuCrew.ActionPermitSystem.RequestExecution", "ModMenuCrew.ActionPermitSystem.OnServerApproval", "ModMenuCrew.ServerData.TriggerSilentDenial", "ModMenuCrew.ServerData.ParseFromEncryptedPayload", "ModMenuCrew.ServerData.DecryptPayload", "ModMenuCrew.ServerData.CalculateIntegrity", "ModMenuCrew.ServerData.GetStoredIntegrityHash", "ModMenuCrew.Monitoring.AntiTamper.Update",
		"ModMenuCrew.ModMenuCrewPlugin.CheckForSelfPatches", "ModMenuCrew.ModMenuCrewPlugin.RuntimeSecurityCheck", "ModMenuCrew.ModMenuCrewPlugin.HasForeignPatches"
	};

	private static readonly string[] _criticalMethods = new string[6] { "System.Net.Http.HttpClient.SendAsync", "System.Net.Http.HttpMessageInvoker.SendAsync", "System.Net.HttpWebRequest.GetResponse", "System.Net.WebRequest.Create", "System.Net.ServicePointManager.set_ServerCertificateValidationCallback", "System.Net.Security.SslStream.AuthenticateAsClient" };

	public DebuggerComponent Component { get; private set; }

	public static ModMenuCrewPlugin Instance { get; private set; }

	public Harmony Harmony { get; } = new Harmony("CrewCore.online");


	public override void Load()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Expected O, but got Unknown
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Expected O, but got Unknown
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Expected O, but got Unknown
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Expected O, but got Unknown
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Expected O, but got Unknown
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Expected O, but got Unknown
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		Instance = this;
		try
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
		}
		catch
		{
		}
		ManualLogSource log = ((BasePlugin)Instance).Log;
		bool flag = default(bool);
		BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(28, 2, ref flag);
		if (flag)
		{
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Plugin ");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>("CrewCore.online");
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" version ");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>("6.1.4b");
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" is loading.");
		}
		log.LogInfo(val);
		try
		{
			ManualLogSource log2 = ((BasePlugin)Instance).Log;
			val = new BepInExInfoLogInterpolatedStringHandler(33, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[Platform] Wine/Proton detected: ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<bool>(ModKeyValidator.IsRunningUnderWine);
			}
			log2.LogInfo(val);
		}
		catch
		{
		}
		try
		{
			ClassInjector.RegisterTypeInIl2Cpp<ModInitWatcher>();
		}
		catch (Exception ex)
		{
			ManualLogSource log3 = ((BasePlugin)this).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(35, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Failed to register ModInitWatcher: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex);
			}
			log3.LogError(val2);
		}
		try
		{
			ClassInjector.RegisterTypeInIl2Cpp<DebuggerComponent>();
		}
		catch (Exception ex2)
		{
			ManualLogSource log4 = ((BasePlugin)this).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(38, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Failed to register DebuggerComponent: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex2);
			}
			log4.LogError(val2);
		}
		try
		{
			ClassInjector.RegisterTypeInIl2Cpp<ReplayRecorder>();
		}
		catch (Exception ex3)
		{
			ManualLogSource log5 = ((BasePlugin)this).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(35, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Failed to register ReplayRecorder: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex3);
			}
			log5.LogError(val2);
		}
		try
		{
			ClassInjector.RegisterTypeInIl2Cpp<ReplayPlayer>();
		}
		catch (Exception ex4)
		{
			ManualLogSource log6 = ((BasePlugin)this).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(33, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Failed to register ReplayPlayer: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex4);
			}
			log6.LogError(val2);
		}
		try
		{
			ClassInjector.RegisterTypeInIl2Cpp<ReplayPuppet>();
		}
		catch (Exception ex5)
		{
			ManualLogSource log7 = ((BasePlugin)this).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(33, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Failed to register ReplayPuppet: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex5);
			}
			log7.LogError(val2);
		}
		try
		{
			ClassInjector.RegisterTypeInIl2Cpp<ReplayViewer>();
		}
		catch (Exception ex6)
		{
			ManualLogSource log8 = ((BasePlugin)this).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(33, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Failed to register ReplayViewer: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex6);
			}
			log8.LogError(val2);
		}
		try
		{
			ClassInjector.RegisterTypeInIl2Cpp<ReplayUI>();
		}
		catch (Exception ex7)
		{
			ManualLogSource log9 = ((BasePlugin)this).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(29, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Failed to register ReplayUI: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex7);
			}
			log9.LogError(val2);
		}
		try
		{
			ClassInjector.RegisterTypeInIl2Cpp<WebRadarPlugin>();
		}
		catch (Exception ex8)
		{
			ManualLogSource log10 = ((BasePlugin)this).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(35, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Failed to register WebRadarPlugin: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex8);
			}
			log10.LogError(val2);
		}
		try
		{
			ClassInjector.RegisterTypeInIl2Cpp<ModTooltipHandler>();
		}
		catch (Exception ex9)
		{
			ManualLogSource log11 = ((BasePlugin)this).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(38, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Failed to register ModTooltipHandler: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex9);
			}
			log11.LogError(val2);
		}
		try
		{
			ClassInjector.RegisterTypeInIl2Cpp<VersionShowerFx>();
		}
		catch (Exception ex10)
		{
			ManualLogSource log12 = ((BasePlugin)this).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(36, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Failed to register VersionShowerFx: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex10);
			}
			log12.LogError(val2);
		}
		try
		{
			ClassInjector.RegisterTypeInIl2Cpp<MMCRendererComponent>();
		}
		catch (Exception ex11)
		{
			ManualLogSource log13 = ((BasePlugin)this).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(41, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("Failed to register MMCRendererComponent: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex11);
			}
			log13.LogError(val2);
		}
		((BasePlugin)this).AddComponent<ModInitWatcher>();
		ManualLogSource log14 = ((BasePlugin)Instance).Log;
		val = new BepInExInfoLogInterpolatedStringHandler(54, 1, ref flag);
		if (flag)
		{
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Plugin ");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>("CrewCore.online");
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" registered. Waiting for game initialization...");
		}
		log14.LogInfo(val);
	}

	internal void RunDeferredInit()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		if (_deferredInitDone)
		{
			return;
		}
		_deferredInitDone = true;
		ManualLogSource log = ((BasePlugin)Instance).Log;
		bool flag = default(bool);
		BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(63, 0, ref flag);
		if (flag)
		{
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ModInitWatcher] Game ready. Running deferred initialization...");
		}
		log.LogInfo(val);
		if (HasForeignPlugins())
		{
			((BasePlugin)this).Log.LogError((object)"[SECURITY] ModMenuCrew cannot run with other plugins. Aborting load.");
			return;
		}
		if (((BasePlugin)this).Config != null)
		{
			CheatConfig.Initialize(((BasePlugin)this).Config);
		}
		if (((BasePlugin)this).Config != null)
		{
			MiscConfig.Initialize(((BasePlugin)this).Config);
		}
		try
		{
			ChatBlacklistFileService.EnsureLoaded();
		}
		catch
		{
		}
		ConfigEntry<int> themeIndex = CheatConfig.ThemeIndex;
		if (themeIndex != null && themeIndex.Value > 0)
		{
			ThemePresets.Apply(CheatConfig.ThemeIndex.Value);
		}
		Harmony.PatchAll();
		if (((BasePlugin)this).Config != null)
		{
			LobbyHarmonyPatches.InitializeConfig(((BasePlugin)this).Config);
		}
		Component = ((BasePlugin)this).AddComponent<DebuggerComponent>();
		((BasePlugin)this).AddComponent<ReplayRecorder>();
		((BasePlugin)this).AddComponent<ReplayPlayer>();
		((BasePlugin)this).AddComponent<WebRadarPlugin>();
		ConfigEntry<bool> webRadarEnabled = CheatConfig.WebRadarEnabled;
		if (webRadarEnabled != null && webRadarEnabled.Value)
		{
			try
			{
				WebRadarService.Start();
			}
			catch
			{
			}
		}
		ManualLogSource log2 = ((BasePlugin)Instance).Log;
		val = new BepInExInfoLogInterpolatedStringHandler(28, 1, ref flag);
		if (flag)
		{
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Plugin ");
			((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>("CrewCore.online");
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" loaded successfully.");
		}
		log2.LogInfo(val);
	}

	public override bool Unload()
	{
		try
		{
			WebRadarService.Stop();
			if ((Object)(object)Component != (Object)null)
			{
				Component.CleanupResources();
			}
			Harmony harmony = Harmony;
			if (harmony != null)
			{
				harmony.UnpatchSelf();
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error during plugin unload: {value}"));
		}
		return ((BasePlugin)this).Unload();
	}

	private void CleanupPlayerPrefsAndResetValidation()
	{
		try
		{
			string text = "ModMenuCrew_Activated_" + "6.1.4b";
			PlayerPrefs.DeleteKey(text);
			PlayerPrefs.DeleteKey(text + "_Message");
			PlayerPrefs.DeleteKey("ModMenuCrew_Activated_" + "4.0.0");
			PlayerPrefs.DeleteKey("ModMenuCrew_Activated_" + "4.0.0_Message");
			PlayerPrefs.DeleteKey("ModMenuCrew_Activated_" + "5.0.0");
			PlayerPrefs.DeleteKey("ModMenuCrew_Activated_" + "5.0.0_Message");
			PlayerPrefs.Save();
			ModKeyValidator.ResetValidationState();
			((BasePlugin)Instance).Log.LogInfo((object)"PlayerPrefs (including current version) cleared. Activation state reset.");
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[ModMenuCrew] Error clearing PlayerPrefs: {value}"));
		}
	}

	public bool HasForeignPatches()
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Expected O, but got Unknown
		try
		{
			long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			if (_foreignPatchesCache.HasValue && num - _lastPatchCheck < 500)
			{
				return _foreignPatchesCache.Value;
			}
			_lastPatchCheck = num;
			string name = typeof(ModMenuCrewPlugin).Assembly.GetName().Name;
			string text = "CrewCore.online";
			bool flag = default(bool);
			foreach (MethodBase allPatchedMethod in Harmony.GetAllPatchedMethods())
			{
				if (allPatchedMethod == null || allPatchedMethod.DeclaringType == null)
				{
					continue;
				}
				string text2 = allPatchedMethod.DeclaringType.FullName + "." + allPatchedMethod.Name;
				string[] criticalMethods = _criticalMethods;
				foreach (string text3 in criticalMethods)
				{
					if (!text2.Contains(text3.Split('.').Last()))
					{
						continue;
					}
					string? fullName = allPatchedMethod.DeclaringType.FullName;
					if (fullName != null && fullName.Contains(text3.Substring(0, text3.LastIndexOf('.'))))
					{
						ManualLogSource log = ((BasePlugin)this).Log;
						BepInExErrorLogInterpolatedStringHandler val = new BepInExErrorLogInterpolatedStringHandler(49, 1, ref flag);
						if (flag)
						{
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[SECURITY] MITM ATTACK: Critical method patched: ");
							((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(text2);
						}
						log.LogError(val);
						_foreignPatchesCache = true;
						return true;
					}
				}
				if (allPatchedMethod.DeclaringType.Assembly.GetName().Name != name)
				{
					continue;
				}
				Patches patchInfo = Harmony.GetPatchInfo(allPatchedMethod);
				if (patchInfo == null)
				{
					continue;
				}
				string item = allPatchedMethod.DeclaringType?.FullName + "." + allPatchedMethod.Name;
				if (_neverPatchedMethods.Contains(item))
				{
					int num2 = 0;
					if (patchInfo.Prefixes != null)
					{
						num2 += patchInfo.Prefixes.Count;
					}
					if (patchInfo.Postfixes != null)
					{
						num2 += patchInfo.Postfixes.Count;
					}
					if (patchInfo.Transpilers != null)
					{
						num2 += patchInfo.Transpilers.Count;
					}
					if (patchInfo.Finalizers != null)
					{
						num2 += patchInfo.Finalizers.Count;
					}
					if (num2 > 0)
					{
						_foreignPatchesCache = true;
						return true;
					}
				}
				List<string> list = new List<string>();
				if (patchInfo.Prefixes != null)
				{
					list.AddRange(patchInfo.Prefixes.Select((Patch p) => p.owner));
				}
				if (patchInfo.Postfixes != null)
				{
					list.AddRange(patchInfo.Postfixes.Select((Patch p) => p.owner));
				}
				if (patchInfo.Transpilers != null)
				{
					list.AddRange(patchInfo.Transpilers.Select((Patch p) => p.owner));
				}
				if (patchInfo.Finalizers != null)
				{
					list.AddRange(patchInfo.Finalizers.Select((Patch p) => p.owner));
				}
				foreach (string item2 in list)
				{
					if (item2 != text && !string.IsNullOrEmpty(item2))
					{
						ManualLogSource log2 = ((BasePlugin)this).Log;
						BepInExErrorLogInterpolatedStringHandler val = new BepInExErrorLogInterpolatedStringHandler(56, 3, ref flag);
						if (flag)
						{
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[SECURITY] Foreign Harmony patch on our code: ");
							((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(item2);
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" patched ");
							((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(allPatchedMethod.DeclaringType?.Name);
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral(".");
							((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(allPatchedMethod.Name);
						}
						log2.LogError(val);
						_foreignPatchesCache = true;
						return true;
					}
				}
			}
			_foreignPatchesCache = false;
			return false;
		}
		catch (Exception)
		{
			((BasePlugin)this).Log.LogError((object)"[SECURITY] Harmony introspection failed.");
			return true;
		}
	}

	public bool HasForeignPlugins()
	{
		return HasForeignPatches();
	}

	public void RuntimeSecurityCheck()
	{
		_foreignPatchesCache = null;
		if (CheckForSelfPatches())
		{
			ServerData.TriggerSilentDenial();
			return;
		}
		if (HasForeignPlugins())
		{
			ServerData.TriggerSilentDenial();
			ModKeyValidator.ResetValidationState();
			return;
		}
		try
		{
			byte[] rsaModulusHash = GhostUI.GetRsaModulusHash();
			if (rsaModulusHash != null)
			{
				byte[] array = SHA256.HashData(Convert.FromBase64String(ModKeyValidator._rsaModulusPart1 + ModKeyValidator._rsaModulusPart2 + ModKeyValidator._rsaModulusPart3));
				if (!CryptographicOperations.FixedTimeEquals(rsaModulusHash, array))
				{
					ServerData.TriggerSilentDenial();
					return;
				}
			}
		}
		catch (Exception)
		{
			ServerData.TriggerSilentDenial();
		}
		try
		{
			string derivedApiUrl = ModKeyValidator.GetDerivedApiUrl();
			if (string.IsNullOrEmpty(derivedApiUrl) || !derivedApiUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || !derivedApiUrl.Contains("crewcore.online"))
			{
				ServerData.TriggerSilentDenial();
				return;
			}
		}
		catch (Exception)
		{
			ServerData.TriggerSilentDenial();
		}
		try
		{
			if (!ModKeyValidator.VerifyAllowedHostnames())
			{
				ServerData.TriggerSilentDenial();
				return;
			}
		}
		catch (Exception)
		{
			ServerData.TriggerSilentDenial();
		}
		if (Time.frameCount % 1800 == 150)
		{
			VerifyDnsResolutionAsync();
		}
	}

	private static async Task VerifyDnsResolutionAsync()
	{
		try
		{
			string derivedApiUrl = ModKeyValidator.GetDerivedApiUrl();
			if (string.IsNullOrEmpty(derivedApiUrl))
			{
				return;
			}
			IPAddress[] array = await Dns.GetHostAddressesAsync(new Uri(derivedApiUrl).Host);
			bool flag2 = default(bool);
			foreach (IPAddress iPAddress in array)
			{
				byte[] addressBytes = iPAddress.GetAddressBytes();
				bool flag = false;
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork && addressBytes.Length == 4)
				{
					flag = addressBytes[0] == 127 || addressBytes[0] == 10 || (addressBytes[0] == 172 && addressBytes[1] >= 16 && addressBytes[1] <= 31) || (addressBytes[0] == 192 && addressBytes[1] == 168) || (addressBytes[0] == 0 && addressBytes[1] == 0 && addressBytes[2] == 0 && addressBytes[3] == 0);
				}
				else if (iPAddress.AddressFamily == AddressFamily.InterNetworkV6)
				{
					byte[] addressBytes2 = iPAddress.GetAddressBytes();
					flag = IPAddress.IsLoopback(iPAddress) || iPAddress.IsIPv6LinkLocal || iPAddress.IsIPv6SiteLocal || addressBytes2[0] == 252 || addressBytes2[0] == 253 || iPAddress.Equals(IPAddress.IPv6None) || (iPAddress.IsIPv4MappedToIPv6 && IsPrivateIPv4Mapped(iPAddress));
				}
				if (!flag)
				{
					continue;
				}
				try
				{
					ModMenuCrewPlugin instance = Instance;
					if (((instance != null) ? ((BasePlugin)instance).Log : null) != null)
					{
						ManualLogSource log = ((BasePlugin)Instance).Log;
						BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(75, 1, ref flag2);
						if (flag2)
						{
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[SECURITY] DNS resolved to private IP: ");
							((BepInExLogInterpolatedStringHandler)val).AppendFormatted<IPAddress>(iPAddress);
							((BepInExLogInterpolatedStringHandler)val).AppendLiteral(". VPN/proxy detected — not blocking.");
						}
						log.LogWarning(val);
					}
					break;
				}
				catch
				{
					break;
				}
			}
		}
		catch
		{
		}
	}

	private static bool IsPrivateIPv4Mapped(IPAddress addr)
	{
		try
		{
			byte[] addressBytes = addr.MapToIPv4().GetAddressBytes();
			return addressBytes[0] == 127 || addressBytes[0] == 10 || (addressBytes[0] == 172 && addressBytes[1] >= 16 && addressBytes[1] <= 31) || (addressBytes[0] == 192 && addressBytes[1] == 168) || (addressBytes[0] == 0 && addressBytes[1] == 0 && addressBytes[2] == 0 && addressBytes[3] == 0);
		}
		catch
		{
			return true;
		}
	}

	private bool CheckForSelfPatches()
	{
		try
		{
			MethodInfo[] array = new MethodInfo[19]
			{
				AccessTools.Method(typeof(ModMenuCrewPlugin), "HasForeignPlugins", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(ModMenuCrewPlugin), "RuntimeSecurityCheck", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(GhostUI), "Execute", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(ServerData), "ParseFromEncryptedPayload", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(ServerData), "DecryptPayload", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(ModKeyValidator), "ValidateKeyAsync", new Type[2]
				{
					typeof(string),
					typeof(CancellationToken)
				}, (Type[])null),
				AccessTools.Method(typeof(GhostUI), "VerifyBytecodeSignatureV5", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(GhostUI), "ValidateTimestampAntiReplay", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(CertificatePinner), "ValidateServerCertificate", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(RealtimeConnection), "Connect", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(ActionPermitSystem), "OnServerApproval", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(ActionPermitSystem), "RequestExecution", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(ServerData), "TriggerSilentDenial", (Type[])null, (Type[])null),
				typeof(ModKeyValidator).GetProperty("IsPremium", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetGetMethod(nonPublic: true),
				AccessTools.Method(typeof(ModKeyValidator), "V", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(ServerGate), "CanRender", (Type[])null, (Type[])null),
				typeof(IntegrityGuard).GetProperty("IsIntact", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetGetMethod(nonPublic: true),
				AccessTools.Method(typeof(ServerData), "CalculateIntegrity", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(ServerData), "GetStoredIntegrityHash", (Type[])null, (Type[])null)
			};
			foreach (MethodInfo methodInfo in array)
			{
				if (!(methodInfo == null))
				{
					Patches patchInfo = Harmony.GetPatchInfo((MethodBase)methodInfo);
					if (patchInfo != null && (patchInfo.Prefixes.Count > 0 || patchInfo.Postfixes.Count > 0 || patchInfo.Transpilers.Count > 0 || patchInfo.Finalizers.Count > 0))
					{
						return true;
					}
				}
			}
			return false;
		}
		catch
		{
			return true;
		}
	}
}
