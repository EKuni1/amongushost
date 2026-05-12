using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Reflection;
using Innersloth.Assets;
using ModMenuCrew.Features;
using PowerTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenuCrew.ReplaySystem;

public class ReplayViewer : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoLoadCosmeticsFromAssets_003Ed__69 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ReplayPlayerInfo info;

		public HatManager hm;

		public GameObject puppet;

		public ReplayPuppet puppetComp;

		private AddressableAsset<HatViewData> _003ChatAsset_003E5__2;

		private HatViewData _003ChatView_003E5__3;

		private AddressableAsset<SkinViewData> _003CskinAsset_003E5__4;

		private SkinViewData _003CskinView_003E5__5;

		private AddressableAsset<VisorViewData> _003CvisorAsset_003E5__6;

		private VisorViewData _003CvisorView_003E5__7;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoLoadCosmeticsFromAssets_003Ed__69(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003ChatAsset_003E5__2 = null;
			_003ChatView_003E5__3 = null;
			_003CskinAsset_003E5__4 = null;
			_003CskinView_003E5__5 = null;
			_003CvisorAsset_003E5__6 = null;
			_003CvisorView_003E5__7 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0261: Unknown result type (might be due to invalid IL or missing references)
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_0288: Unknown result type (might be due to invalid IL or missing references)
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_05bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_05db: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_08d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_08dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_08f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_08f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_08ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0905: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e6: Expected O, but got Unknown
			//IL_0632: Unknown result type (might be due to invalid IL or missing references)
			//IL_063c: Expected O, but got Unknown
			//IL_0953: Unknown result type (might be due to invalid IL or missing references)
			//IL_095d: Expected O, but got Unknown
			//IL_043a: Unknown result type (might be due to invalid IL or missing references)
			//IL_043f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0441: Unknown result type (might be due to invalid IL or missing references)
			//IL_0446: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_083b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0840: Unknown result type (might be due to invalid IL or missing references)
			//IL_0849: Unknown result type (might be due to invalid IL or missing references)
			//IL_0855: Unknown result type (might be due to invalid IL or missing references)
			//IL_085a: Unknown result type (might be due to invalid IL or missing references)
			//IL_085f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0861: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_0876: Unknown result type (might be due to invalid IL or missing references)
			//IL_087d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_088a: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_089f: Unknown result type (might be due to invalid IL or missing references)
			//IL_08a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_08bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0511: Unknown result type (might be due to invalid IL or missing references)
			//IL_0518: Unknown result type (might be due to invalid IL or missing references)
			//IL_052a: Unknown result type (might be due to invalid IL or missing references)
			//IL_052f: Unknown result type (might be due to invalid IL or missing references)
			//IL_053d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0542: Unknown result type (might be due to invalid IL or missing references)
			//IL_0544: Unknown result type (might be due to invalid IL or missing references)
			//IL_0559: Unknown result type (might be due to invalid IL or missing references)
			//IL_0560: Unknown result type (might be due to invalid IL or missing references)
			//IL_056d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0582: Unknown result type (might be due to invalid IL or missing references)
			//IL_0589: Unknown result type (might be due to invalid IL or missing references)
			//IL_059b: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
			bool flag2;
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!string.IsNullOrEmpty(info.HatId) && info.HatId != "hat_NoHat")
				{
					_003ChatAsset_003E5__2 = null;
					_003ChatView_003E5__3 = null;
					bool flag = false;
					try
					{
						HatData hatById = hm.GetHatById(info.HatId);
						if ((Object)(object)hatById != (Object)null)
						{
							_003ChatAsset_003E5__2 = hatById.CreateAddressableAsset();
							_003ChatView_003E5__3 = _003ChatAsset_003E5__2?.GetAsset();
							flag = (Object)(object)_003ChatView_003E5__3 != (Object)null && (Object)(object)_003ChatView_003E5__3.MainImage != (Object)null;
						}
					}
					catch
					{
					}
					if (!flag && _003ChatAsset_003E5__2 != null)
					{
						_003C_003E2__current = ((AddressableAsset)_003ChatAsset_003E5__2).CoLoadAsync((Action)null);
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_0114;
				}
				goto IL_02fe;
			case 1:
				_003C_003E1__state = -1;
				try
				{
					_003ChatView_003E5__3 = _003ChatAsset_003E5__2.GetAsset();
				}
				catch
				{
				}
				goto IL_0114;
			case 2:
				_003C_003E1__state = -1;
				try
				{
					_003CskinView_003E5__5 = _003CskinAsset_003E5__4.GetAsset();
				}
				catch
				{
				}
				goto IL_03f2;
			case 3:
				{
					_003C_003E1__state = -1;
					try
					{
						_003CvisorView_003E5__7 = _003CvisorAsset_003E5__6.GetAsset();
					}
					catch
					{
					}
					goto IL_0789;
				}
				IL_03f2:
				if ((Object)(object)_003CskinView_003E5__5 != (Object)null && (Object)(object)_003CskinView_003E5__5.IdleFrame != (Object)null && (Object)(object)puppet != (Object)null && !((Il2CppObjectBase)puppet).WasCollected)
				{
					try
					{
						Vector3 localPosition = Vector3.zero;
						Vector3 localScale = Vector3.one;
						try
						{
							Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
							while (enumerator.MoveNext())
							{
								PlayerControl current = enumerator.Current;
								if (!((Object)(object)current == (Object)null))
								{
									CosmeticsLayer cosmetics = current.cosmetics;
									SkinLayer val = ((cosmetics != null) ? cosmetics.CurrentSkin : null);
									if ((Object)(object)((val != null) ? val.layer : null) != (Object)null)
									{
										Vector3 localScale2 = puppet.transform.localScale;
										Vector3 val2 = ((Component)val.layer).transform.position - ((Component)current).transform.position;
										localPosition = new Vector3((localScale2.x != 0f) ? (val2.x / Mathf.Abs(localScale2.x)) : 0f, (localScale2.y != 0f) ? (val2.y / Mathf.Abs(localScale2.y)) : 0f, -0.001f);
										Vector3 lossyScale = ((Component)val.layer).transform.lossyScale;
										localScale = new Vector3((localScale2.x != 0f) ? (lossyScale.x / Mathf.Abs(localScale2.x)) : 1f, (localScale2.y != 0f) ? (lossyScale.y / Mathf.Abs(localScale2.y)) : 1f, 1f);
										break;
									}
								}
							}
						}
						catch
						{
						}
						GameObject val3 = new GameObject("Skin_Loaded");
						val3.transform.SetParent(puppet.transform);
						val3.transform.localPosition = localPosition;
						val3.transform.localScale = localScale;
						SpriteRenderer val4 = val3.AddComponent<SpriteRenderer>();
						val4.sprite = _003CskinView_003E5__5.IdleFrame;
						((Renderer)val4).sortingOrder = 7;
						val4.flipX = false;
						if (_003CskinView_003E5__5.MatchPlayerColor)
						{
							try
							{
								PlayerMaterial.SetColors(info.ColorId, (Renderer)val4);
							}
							catch
							{
							}
						}
						if ((Object)(object)puppetComp != (Object)null && !((Il2CppObjectBase)puppetComp).WasCollected && (Object)(object)_003CskinView_003E5__5.RunAnim != (Object)null)
						{
							puppetComp.SetupSkinAnimation(val4, _003CskinView_003E5__5);
						}
					}
					catch
					{
					}
				}
				_003CskinAsset_003E5__4 = null;
				_003CskinView_003E5__5 = null;
				goto IL_0695;
				IL_0789:
				if ((Object)(object)_003CvisorView_003E5__7 != (Object)null && (Object)(object)_003CvisorView_003E5__7.IdleFrame != (Object)null && (Object)(object)puppet != (Object)null && !((Il2CppObjectBase)puppet).WasCollected)
				{
					try
					{
						Vector3 localPosition2 = default(Vector3);
						((Vector3)(ref localPosition2))._002Ector(0.36f, 0.2f, -0.003f);
						try
						{
							Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
							while (enumerator.MoveNext())
							{
								PlayerControl current2 = enumerator.Current;
								if (!((Object)(object)current2 == (Object)null))
								{
									CosmeticsLayer cosmetics2 = current2.cosmetics;
									VisorLayer val5 = ((cosmetics2 != null) ? cosmetics2.visor : null);
									if ((Object)(object)val5 != (Object)null)
									{
										Vector3 localScale3 = puppet.transform.localScale;
										Vector3 val6 = ((Component)val5).transform.position - ((Component)current2).transform.position;
										localPosition2 = new Vector3((localScale3.x != 0f) ? (val6.x / Mathf.Abs(localScale3.x)) : 0f, (localScale3.y != 0f) ? (val6.y / Mathf.Abs(localScale3.y)) : 0f, -0.003f);
										break;
									}
								}
							}
						}
						catch
						{
						}
						GameObject val7 = new GameObject("Visor_Loaded");
						val7.transform.SetParent(puppet.transform);
						val7.transform.localPosition = localPosition2;
						val7.transform.localScale = Vector3.one;
						SpriteRenderer val8 = val7.AddComponent<SpriteRenderer>();
						val8.sprite = _003CvisorView_003E5__7.IdleFrame;
						((Renderer)val8).sortingOrder = 11;
						val8.flipX = false;
						if (_003CvisorView_003E5__7.MatchPlayerColor)
						{
							try
							{
								PlayerMaterial.SetColors(info.ColorId, (Renderer)val8);
							}
							catch
							{
							}
						}
					}
					catch
					{
					}
				}
				_003CvisorAsset_003E5__6 = null;
				_003CvisorView_003E5__7 = null;
				break;
				IL_0695:
				if (string.IsNullOrEmpty(info.VisorId) || !(info.VisorId != "visor_EmptyVisor"))
				{
					break;
				}
				_003CvisorAsset_003E5__6 = null;
				_003CvisorView_003E5__7 = null;
				flag2 = false;
				try
				{
					VisorData visorById = hm.GetVisorById(info.VisorId);
					if ((Object)(object)visorById != (Object)null)
					{
						_003CvisorAsset_003E5__6 = visorById.CreateAddressableAsset();
						_003CvisorView_003E5__7 = _003CvisorAsset_003E5__6?.GetAsset();
						flag2 = (Object)(object)_003CvisorView_003E5__7 != (Object)null && (Object)(object)_003CvisorView_003E5__7.IdleFrame != (Object)null;
					}
				}
				catch
				{
				}
				if (!flag2 && _003CvisorAsset_003E5__6 != null)
				{
					_003C_003E2__current = ((AddressableAsset)_003CvisorAsset_003E5__6).CoLoadAsync((Action)null);
					_003C_003E1__state = 3;
					return true;
				}
				goto IL_0789;
				IL_02fe:
				if (!string.IsNullOrEmpty(info.SkinId) && info.SkinId != "skin_None")
				{
					_003CskinAsset_003E5__4 = null;
					_003CskinView_003E5__5 = null;
					bool flag3 = false;
					try
					{
						SkinData skinById = hm.GetSkinById(info.SkinId);
						if ((Object)(object)skinById != (Object)null)
						{
							_003CskinAsset_003E5__4 = skinById.CreateAddressableAsset();
							_003CskinView_003E5__5 = _003CskinAsset_003E5__4?.GetAsset();
							flag3 = (Object)(object)_003CskinView_003E5__5 != (Object)null && (Object)(object)_003CskinView_003E5__5.IdleFrame != (Object)null;
						}
					}
					catch
					{
					}
					if (!flag3 && _003CskinAsset_003E5__4 != null)
					{
						_003C_003E2__current = ((AddressableAsset)_003CskinAsset_003E5__4).CoLoadAsync((Action)null);
						_003C_003E1__state = 2;
						return true;
					}
					goto IL_03f2;
				}
				goto IL_0695;
				IL_0114:
				if ((Object)(object)_003ChatView_003E5__3 != (Object)null && (Object)(object)_003ChatView_003E5__3.MainImage != (Object)null && (Object)(object)puppet != (Object)null && !((Il2CppObjectBase)puppet).WasCollected)
				{
					try
					{
						Vector3 localPosition3 = default(Vector3);
						((Vector3)(ref localPosition3))._002Ector(0.01f, 0.55f, -0.002f);
						try
						{
							Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
							while (enumerator.MoveNext())
							{
								PlayerControl current3 = enumerator.Current;
								if (!((Object)(object)current3 == (Object)null))
								{
									CosmeticsLayer cosmetics3 = current3.cosmetics;
									HatParent val9 = ((cosmetics3 != null) ? cosmetics3.hat : null);
									if ((Object)(object)val9 != (Object)null)
									{
										Vector3 localScale4 = puppet.transform.localScale;
										Vector3 val10 = ((Component)val9).transform.position - ((Component)current3).transform.position;
										localPosition3 = new Vector3((localScale4.x != 0f) ? (val10.x / Mathf.Abs(localScale4.x)) : 0f, (localScale4.y != 0f) ? (val10.y / Mathf.Abs(localScale4.y)) : 0f, -0.002f);
										break;
									}
								}
							}
						}
						catch
						{
						}
						GameObject val11 = new GameObject("Hat_Loaded");
						val11.transform.SetParent(puppet.transform);
						val11.transform.localPosition = localPosition3;
						val11.transform.localScale = Vector3.one;
						SpriteRenderer val12 = val11.AddComponent<SpriteRenderer>();
						val12.sprite = _003ChatView_003E5__3.MainImage;
						((Renderer)val12).sortingOrder = 10;
						val12.flipX = false;
						if (_003ChatView_003E5__3.MatchPlayerColor)
						{
							try
							{
								PlayerMaterial.SetColors(info.ColorId, (Renderer)val12);
							}
							catch
							{
							}
						}
					}
					catch
					{
					}
				}
				_003ChatAsset_003E5__2 = null;
				_003ChatView_003E5__3 = null;
				goto IL_02fe;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public static ReplayViewer Instance;

	private ReplayData data;

	private bool isActive;

	private bool isPaused = true;

	private float currentTime;

	private float playbackSpeed = 1f;

	private Dictionary<byte, ReplayPuppet> puppets = new Dictionary<byte, ReplayPuppet>();

	private GameObject puppetRoot;

	private Camera replayCamera;

	private Vector3 cameraPosition;

	private float cameraZoom = 5f;

	private const float MIN_ZOOM = 2f;

	private const float MAX_ZOOM = 15f;

	private byte? followingPlayerId;

	private Dictionary<byte, LineRenderer> routeLines = new Dictionary<byte, LineRenderer>();

	private Dictionary<byte, List<Vector3>> routePoints = new Dictionary<byte, List<Vector3>>();

	private const int MAX_ROUTE_POINTS = 500;

	private Dictionary<Vector2Int, float> heatmapData = new Dictionary<Vector2Int, float>();

	private Dictionary<byte, GameObject> deadBodies = new Dictionary<byte, GameObject>();

	private Dictionary<byte, GameObject> puppetPets = new Dictionary<byte, GameObject>();

	private ReplayUI ui;

	private int lastFrameIndex = -1;

	private Dictionary<byte, Vector3[]> _routeArrayCache = new Dictionary<byte, Vector3[]>();

	private Dictionary<byte, PlayerState> _nextFrameLookup = new Dictionary<byte, PlayerState>();

	private Sprite cachedTemplateSprite;

	private Material cachedTemplateMaterial;

	private List<Material> _allocatedMaterials = new List<Material>();

	private List<Texture2D> _allocatedTextures = new List<Texture2D>();

	private Vector3 _defaultPlayerScale = new Vector3(0.5f, 0.5f, 1f);

	private MonoBehaviour vanillaCameraScript;

	[HideFromIl2Cpp]
	public ReplayData Data => data;

	public bool IsActive => isActive;

	public bool IsPaused => isPaused;

	public float CurrentTime => currentTime;

	public float TotalDuration
	{
		get
		{
			ReplayData replayData = data;
			if (replayData == null)
			{
				ReplayData replayData2 = data;
				if (replayData2 == null || !(replayData2.Frames?.Count > 0))
				{
					return 0f;
				}
				return data.Frames[data.Frames.Count - 1].Time;
			}
			return replayData.TotalDuration;
		}
	}

	public byte? FollowingPlayerId => followingPlayerId;

	private Material TrackMaterial(Material m)
	{
		if ((Object)(object)m != (Object)null)
		{
			_allocatedMaterials.Add(m);
		}
		return m;
	}

	private Texture2D TrackTexture(Texture2D t)
	{
		if ((Object)(object)t != (Object)null)
		{
			_allocatedTextures.Add(t);
		}
		return t;
	}

	public ReplayViewer(IntPtr ptr)
		: base(ptr)
	{
	}//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
	//IL_00a7: Unknown result type (might be due to invalid IL or missing references)


	private void Awake()
	{
		Instance = this;
	}

	public void StartViewer(string replayPath)
	{
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Expected O, but got Unknown
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Expected O, but got Unknown
		bool flag = default(bool);
		try
		{
			if (isActive)
			{
				Stop();
			}
			ReplayRecorder.ReplayInProgress = true;
			try
			{
				typeof(ReplayRecorder).GetField("_replayInProgressSince", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, Time.realtimeSinceStartup);
			}
			catch
			{
			}
			if (!((Object)(object)ShipStatus.Instance != (Object)null))
			{
				((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogWarning((object)"[ReplayViewer] Starting in Menu Mode - visuals may be limited");
			}
			data = ReplayData.Load(replayPath);
			if (data == null || data.Frames.Count == 0)
			{
				((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogError((object)"[ReplayViewer] Failed to load replay or empty");
				ReplayRecorder.ReplayInProgress = false;
				return;
			}
			routeLines.Clear();
			routePoints.Clear();
			puppets.Clear();
			InitializePuppetRoot();
			InitializeCamera();
			SpawnAllPuppets();
			InitializeRoutes();
			InitializeUI();
			currentTime = 0f;
			isPaused = false;
			isActive = true;
			ReplayRecorder.ReplayInProgress = true;
			HideRealPlayers();
			if ((Object)(object)DestroyableSingleton<HudManager>.Instance != (Object)null)
			{
				((Component)DestroyableSingleton<HudManager>.Instance).gameObject.SetActive(false);
			}
			ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(50, 3, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ReplayViewer] Started: ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>(data.Players.Count);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" players, ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>(data.Frames.Count);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" frames, ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>(data.Events?.Count ?? 0);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" events");
			}
			log.LogInfo(val);
		}
		catch (Exception ex)
		{
			ManualLogSource log2 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(35, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[ReplayViewer] StartViewer failed: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex);
			}
			log2.LogError(val2);
			Stop();
		}
	}

	public void Stop()
	{
		ReplayRecorder.ReplayInProgress = false;
		if ((Object)(object)DestroyableSingleton<HudManager>.Instance != (Object)null)
		{
			((Component)DestroyableSingleton<HudManager>.Instance).gameObject.SetActive(true);
			try
			{
				DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
			}
			catch
			{
			}
		}
		ShowRealPlayers();
		if (!isActive)
		{
			return;
		}
		isActive = false;
		try
		{
			((MonoBehaviour)this).StopAllCoroutines();
		}
		catch
		{
		}
		if (Object.op_Implicit((Object)(object)puppetRoot))
		{
			Object.Destroy((Object)(object)puppetRoot);
		}
		if (Object.op_Implicit((Object)(object)ui))
		{
			Object.Destroy((Object)(object)((Component)ui).gameObject);
		}
		foreach (LineRenderer value in routeLines.Values)
		{
			if (Object.op_Implicit((Object)(object)value))
			{
				Object.Destroy((Object)(object)((Component)value).gameObject);
			}
		}
		routeLines.Clear();
		routePoints.Clear();
		_routeArrayCache.Clear();
		_nextFrameLookup.Clear();
		puppets.Clear();
		foreach (GameObject value2 in deadBodies.Values)
		{
			if (Object.op_Implicit((Object)(object)value2))
			{
				Object.Destroy((Object)(object)value2);
			}
		}
		deadBodies.Clear();
		foreach (GameObject value3 in puppetPets.Values)
		{
			if (Object.op_Implicit((Object)(object)value3))
			{
				Object.Destroy((Object)(object)value3);
			}
		}
		puppetPets.Clear();
		foreach (Material allocatedMaterial in _allocatedMaterials)
		{
			try
			{
				if ((Object)(object)allocatedMaterial != (Object)null)
				{
					Object.Destroy((Object)(object)allocatedMaterial);
				}
			}
			catch
			{
			}
		}
		_allocatedMaterials.Clear();
		foreach (Texture2D allocatedTexture in _allocatedTextures)
		{
			try
			{
				if ((Object)(object)allocatedTexture != (Object)null)
				{
					Object.Destroy((Object)(object)allocatedTexture);
				}
			}
			catch
			{
			}
		}
		_allocatedTextures.Clear();
		cachedTemplateSprite = null;
		cachedTemplateMaterial = null;
		RestoreCamera();
		data = null;
		((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogInfo((object)"[ReplayViewer] Stopped and Cleaned Up");
	}

	public void TogglePause()
	{
		isPaused = !isPaused;
	}

	public void SetSpeed(float speed)
	{
		playbackSpeed = Mathf.Clamp(speed, 0.1f, 10f);
	}

	public void Seek(float time)
	{
		currentTime = Mathf.Clamp(time, 0f, TotalDuration);
		foreach (List<Vector3> value in routePoints.Values)
		{
			value.Clear();
		}
		_routeArrayCache.Clear();
		foreach (GameObject value2 in deadBodies.Values)
		{
			if (Object.op_Implicit((Object)(object)value2))
			{
				Object.Destroy((Object)(object)value2);
			}
		}
		deadBodies.Clear();
		lastFrameIndex = -1;
		ApplyFrameState(currentTime);
		UpdateRoutes();
	}

	public void StepFrame(int direction)
	{
		if (data != null && data.Frames.Count != 0)
		{
			int index = Mathf.Clamp(FindFrameIndex(currentTime) + direction, 0, data.Frames.Count - 1);
			currentTime = data.Frames[index].Time;
			ApplyFrameState(currentTime);
		}
	}

	public void AdjustZoom(float multiplier)
	{
		cameraZoom = Mathf.Clamp(cameraZoom * multiplier, 2f, 15f);
		if (Object.op_Implicit((Object)(object)replayCamera))
		{
			replayCamera.orthographicSize = cameraZoom;
		}
	}

	public void FollowPlayer(byte playerId)
	{
		if (puppets.ContainsKey(playerId))
		{
			followingPlayerId = playerId;
		}
	}

	public void StopFollowing()
	{
		followingPlayerId = null;
	}

	public void MoveCamera(Vector3 delta)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)replayCamera != (Object)null)
		{
			Transform transform = ((Component)replayCamera).transform;
			transform.position += delta;
		}
	}

	[HideFromIl2Cpp]
	public IEnumerable<ReplayPuppet> GetPuppets()
	{
		return puppets.Values;
	}

	private void InitializePuppetRoot()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		puppetRoot = new GameObject("ReplayViewer_Puppets");
		if ((Object)(object)puppetRoot != (Object)null)
		{
			Object.DontDestroyOnLoad((Object)(object)puppetRoot);
		}
	}

	private void InitializeCamera()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		if (!((Object)(object)main != (Object)null))
		{
			return;
		}
		cameraPosition = ((Component)main).transform.position;
		replayCamera = main;
		cameraZoom = main.orthographicSize;
		foreach (MonoBehaviour component in ((Component)main).GetComponents<MonoBehaviour>())
		{
			Type il2CppType = ((Object)component).GetIl2CppType();
			if (il2CppType != (Type)null && ((MemberInfo)il2CppType).Name == "FollowerCamera")
			{
				vanillaCameraScript = component;
				((Behaviour)component).enabled = false;
				((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogInfo((object)"[ReplayViewer] Disabled FollowerCamera logic");
				break;
			}
		}
	}

	private void RestoreCamera()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)replayCamera != (Object)null)
		{
			((Component)replayCamera).transform.position = cameraPosition;
			if ((Object)(object)vanillaCameraScript != (Object)null)
			{
				((Behaviour)vanillaCameraScript).enabled = true;
				vanillaCameraScript = null;
				((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogInfo((object)"[ReplayViewer] Restored FollowerCamera logic");
			}
		}
	}

	private void SpawnAllPuppets()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		if (data == null)
		{
			return;
		}
		cachedTemplateSprite = null;
		cachedTemplateMaterial = null;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		bool flag = default(bool);
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current == (Object)null)
			{
				continue;
			}
			try
			{
				CosmeticsLayer cosmetics = current.cosmetics;
				object obj;
				if (cosmetics == null)
				{
					obj = null;
				}
				else
				{
					PlayerBodySprite currentBodySprite = cosmetics.currentBodySprite;
					obj = ((currentBodySprite != null) ? currentBodySprite.BodySprite : null);
				}
				SpriteRenderer val = (SpriteRenderer)obj;
				if ((Object)(object)val != (Object)null && (Object)(object)val.sprite != (Object)null)
				{
					cachedTemplateSprite = val.sprite;
					cachedTemplateMaterial = ((Renderer)val).material;
					_defaultPlayerScale = ((Component)current).transform.localScale;
					ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
					BepInExInfoLogInterpolatedStringHandler val2 = new BepInExInfoLogInterpolatedStringHandler(72, 2, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[ReplayViewer] Cached body template from ");
						BepInExInfoLogInterpolatedStringHandler obj2 = val2;
						NetworkedPlayerInfo obj3 = current.Data;
						((BepInExLogInterpolatedStringHandler)obj2).AppendFormatted<string>(((obj3 != null) ? obj3.PlayerName : null) ?? "?");
						((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(" (cosmetics.BodySprite, scale=");
						((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Vector3>(((Component)current).transform.localScale);
						((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(")");
					}
					log.LogInfo(val2);
					break;
				}
				SpriteRenderer component = ((Component)current).GetComponent<SpriteRenderer>();
				if ((Object)(object)component != (Object)null && (Object)(object)component.sprite != (Object)null)
				{
					cachedTemplateSprite = component.sprite;
					cachedTemplateMaterial = ((Renderer)component).material;
					_defaultPlayerScale = ((Component)current).transform.localScale;
					ManualLogSource log2 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
					BepInExInfoLogInterpolatedStringHandler val2 = new BepInExInfoLogInterpolatedStringHandler(73, 2, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[ReplayViewer] Cached body template from ");
						BepInExInfoLogInterpolatedStringHandler obj4 = val2;
						NetworkedPlayerInfo obj5 = current.Data;
						((BepInExLogInterpolatedStringHandler)obj4).AppendFormatted<string>(((obj5 != null) ? obj5.PlayerName : null) ?? "?");
						((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(" (fallback GetComponent, scale=");
						((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Vector3>(((Component)current).transform.localScale);
						((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(")");
					}
					log2.LogInfo(val2);
					break;
				}
			}
			catch
			{
			}
		}
		if ((Object)(object)cachedTemplateSprite == (Object)null)
		{
			((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogWarning((object)"[ReplayViewer] No template sprite available - procedural fallback will be used");
		}
		foreach (ReplayPlayerInfo player in data.Players)
		{
			SpawnPuppet(player);
		}
	}

	[HideFromIl2Cpp]
	private PlayerControl FindSourcePlayer(ReplayPlayerInfo info)
	{
		PlayerControl val = null;
		PlayerControl val2 = null;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if (!((Object)(object)current == (Object)null) && !((Object)(object)current.Data == (Object)null))
			{
				if (current.PlayerId == info.PlayerId)
				{
					val = current;
					break;
				}
				if ((Object)(object)val2 == (Object)null && current.Data.PlayerName == info.Name)
				{
					val2 = current;
				}
			}
		}
		return val ?? val2;
	}

	[HideFromIl2Cpp]
	private SpriteRenderer ClonePlayerVisuals(PlayerControl source, GameObject target, int targetColorId, bool isExactMatch)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Expected O, but got Unknown
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Expected O, but got Unknown
		SpriteRenderer val = target.GetComponent<SpriteRenderer>() ?? target.AddComponent<SpriteRenderer>();
		SpriteRenderer val2 = null;
		try
		{
			CosmeticsLayer cosmetics = source.cosmetics;
			object obj;
			if (cosmetics == null)
			{
				obj = null;
			}
			else
			{
				PlayerBodySprite currentBodySprite = cosmetics.currentBodySprite;
				obj = ((currentBodySprite != null) ? currentBodySprite.BodySprite : null);
			}
			val2 = (SpriteRenderer)obj;
		}
		catch
		{
		}
		if ((Object)(object)val2 == (Object)null)
		{
			val2 = ((Component)source).GetComponent<SpriteRenderer>();
		}
		Vector3 one = Vector3.one;
		one = ((!((Object)(object)val2 != (Object)null)) ? _defaultPlayerScale : ((Component)val2).transform.lossyScale);
		target.transform.localScale = one;
		if ((Object)(object)val2 != (Object)null && (Object)(object)val2.sprite != (Object)null)
		{
			val.sprite = val2.sprite;
			((Renderer)val).material = TrackMaterial(new Material(((Renderer)val2).material));
			try
			{
				PlayerMaterial.SetColors(targetColorId, (Renderer)val);
			}
			catch
			{
			}
		}
		Vector3 localPosition = default(Vector3);
		Vector3 localScale = default(Vector3);
		foreach (SpriteRenderer componentsInChild in ((Component)source).GetComponentsInChildren<SpriteRenderer>(false))
		{
			if ((Object)(object)componentsInChild == (Object)null || (Object)(object)componentsInChild == (Object)(object)val2 || (Object)(object)componentsInChild.sprite == (Object)null)
			{
				continue;
			}
			try
			{
				Vector3 val3 = ((Component)componentsInChild).transform.position - ((Component)source).transform.position;
				((Vector3)(ref localPosition))._002Ector((one.x != 0f) ? (val3.x / one.x) : 0f, (one.y != 0f) ? (val3.y / one.y) : 0f, (one.z != 0f) ? (val3.z / one.z) : 0f);
				Vector3 lossyScale = ((Component)componentsInChild).transform.lossyScale;
				((Vector3)(ref localScale))._002Ector((one.x != 0f) ? (lossyScale.x / one.x) : 1f, (one.y != 0f) ? (lossyScale.y / one.y) : 1f, (one.z != 0f) ? (lossyScale.z / one.z) : 1f);
				GameObject val4 = new GameObject(((Object)((Component)componentsInChild).gameObject).name);
				val4.transform.SetParent(target.transform);
				val4.transform.localPosition = localPosition;
				val4.transform.localScale = localScale;
				SpriteRenderer val5 = val4.AddComponent<SpriteRenderer>();
				val5.sprite = componentsInChild.sprite;
				((Renderer)val5).material = TrackMaterial(new Material(((Renderer)componentsInChild).material));
				((Renderer)val5).sortingOrder = ((Renderer)componentsInChild).sortingOrder;
				val5.color = componentsInChild.color;
				val5.flipX = false;
				if (!isExactMatch && (Object)(object)((Renderer)componentsInChild).material != (Object)null && ((Renderer)componentsInChild).material.HasProperty("_BodyColor"))
				{
					try
					{
						PlayerMaterial.SetColors(targetColorId, (Renderer)val5);
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
		}
		return val;
	}

	[HideFromIl2Cpp]
	public void SpawnPuppet(ReplayPlayerInfo info)
	{
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Expected O, but got Unknown
		try
		{
			if ((Object)(object)puppetRoot == (Object)null || ((Il2CppObjectBase)puppetRoot).WasCollected)
			{
				return;
			}
			GameObject val = new GameObject("ReplayPuppet_" + info.Name);
			val.transform.SetParent(puppetRoot.transform);
			SpriteRenderer val2 = null;
			bool flag = false;
			PlayerControl val3 = null;
			PlayerControl val4 = FindSourcePlayer(info);
			if ((Object)(object)val4 != (Object)null)
			{
				val3 = val4;
				val2 = ClonePlayerVisuals(val4, val, info.ColorId, isExactMatch: true);
				flag = (Object)(object)val2 != (Object)null && (Object)(object)val2.sprite != (Object)null;
				if (flag)
				{
					ApplyStoredCosmetics(val4, val, info, isExact: true);
				}
			}
			if (!flag)
			{
				for (int num = val.transform.childCount - 1; num >= 0; num--)
				{
					Object.Destroy((Object)(object)((Component)val.transform.GetChild(num)).gameObject);
				}
				PlayerControl val5 = null;
				Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PlayerControl current = enumerator.Current;
					if ((Object)(object)current != (Object)null)
					{
						val5 = current;
						break;
					}
				}
				if ((Object)(object)val5 != (Object)null)
				{
					val3 = val5;
					val2 = ClonePlayerVisuals(val5, val, info.ColorId, isExactMatch: false);
					flag = (Object)(object)val2 != (Object)null && (Object)(object)val2.sprite != (Object)null;
					if (flag)
					{
						ApplyStoredCosmetics(val5, val, info, isExact: false);
					}
				}
			}
			if (!flag && (Object)(object)cachedTemplateSprite != (Object)null)
			{
				val.transform.localScale = _defaultPlayerScale;
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = val.AddComponent<SpriteRenderer>();
				}
				val2.sprite = cachedTemplateSprite;
				((Renderer)val2).material = TrackMaterial(new Material(cachedTemplateMaterial));
				try
				{
					PlayerMaterial.SetColors(info.ColorId, (Renderer)val2);
				}
				catch
				{
				}
				flag = true;
			}
			if (!flag)
			{
				val.transform.localScale = _defaultPlayerScale;
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = val.AddComponent<SpriteRenderer>();
				}
				val2.sprite = CreateCrewmateBodySprite();
				if (info.ColorId >= 0 && info.ColorId < ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length)
				{
					val2.color = Color32.op_Implicit(((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors)[info.ColorId]);
				}
				else
				{
					val2.color = Color32.op_Implicit(info.RealColor);
				}
				GameObject val6 = new GameObject("Visor");
				val6.transform.SetParent(val.transform);
				val6.transform.localPosition = new Vector3(0f, 0f, -0.001f);
				val6.transform.localScale = Vector3.one;
				SpriteRenderer obj2 = val6.AddComponent<SpriteRenderer>();
				obj2.sprite = CreateVisorSprite();
				((Renderer)obj2).sortingOrder = 6;
			}
			((Renderer)val2).sortingOrder = 5;
			SpawnPuppetPet(info, val3, val);
			GameObject val7 = new GameObject("NameTag");
			val7.transform.SetParent(val.transform);
			val7.transform.localPosition = new Vector3(0f, 0.8f, 0f);
			TextMeshPro obj3 = val7.AddComponent<TextMeshPro>();
			((TMP_Text)obj3).text = info.Name;
			((TMP_Text)obj3).fontSize = 1.5f;
			((TMP_Text)obj3).alignment = (TextAlignmentOptions)514;
			((Graphic)obj3).color = Color.white;
			((TMP_Text)obj3).outlineColor = Color32.op_Implicit(Color.black);
			((TMP_Text)obj3).outlineWidth = 0.2f;
			obj3.sortingOrder = 20;
			if (info.IsImpostor)
			{
				GameObject val8 = new GameObject("RoleIndicator");
				val8.transform.SetParent(val.transform);
				val8.transform.localPosition = new Vector3(0.3f, 0.3f, 0f);
				TextMeshPro obj4 = val8.AddComponent<TextMeshPro>();
				((TMP_Text)obj4).text = "<color=#FF4444>★</color>";
				((TMP_Text)obj4).fontSize = 1f;
				obj4.sortingOrder = 21;
			}
			ReplayPuppet replayPuppet = val.AddComponent<ReplayPuppet>();
			replayPuppet.Initialize(info, val2);
			SetupPuppetAnimation(replayPuppet, val, val3, info);
			if (flag)
			{
				LoadCosmeticsFromAssets(val, info, val3, replayPuppet);
			}
			puppets[info.PlayerId] = replayPuppet;
		}
		catch (Exception ex)
		{
			ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			bool flag2 = default(bool);
			BepInExErrorLogInterpolatedStringHandler val9 = new BepInExErrorLogInterpolatedStringHandler(35, 1, ref flag2);
			if (flag2)
			{
				((BepInExLogInterpolatedStringHandler)val9).AppendLiteral("[ReplayViewer] SpawnPuppet failed: ");
				((BepInExLogInterpolatedStringHandler)val9).AppendFormatted<string>(ex.Message);
			}
			log.LogError(val9);
		}
	}

	[HideFromIl2Cpp]
	private void ApplyStoredCosmetics(PlayerControl source, GameObject puppet, ReplayPlayerInfo info, bool isExact)
	{
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Expected O, but got Unknown
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Expected O, but got Unknown
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Expected O, but got Unknown
		try
		{
			if ((Object)(object)((source != null) ? source.cosmetics : null) == (Object)null)
			{
				return;
			}
			if (isExact)
			{
				NetworkedPlayerInfo obj = source.Data;
				PlayerOutfit val = ((obj != null) ? obj.DefaultOutfit : null);
				if (val != null && val.HatId == info.HatId && val.SkinId == info.SkinId && val.VisorId == info.VisorId)
				{
					return;
				}
			}
			_ = source.cosmetics;
			bool flag = !string.IsNullOrEmpty(info.HatId) && info.HatId != "hat_NoHat";
			bool flag2 = !string.IsNullOrEmpty(info.SkinId) && info.SkinId != "skin_None";
			bool flag3 = !string.IsNullOrEmpty(info.VisorId) && info.VisorId != "visor_EmptyVisor";
			if (!flag && !flag2 && !flag3)
			{
				return;
			}
			PlayerControl val2 = null;
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if ((Object)(object)current == (Object)null || (Object)(object)current.Data == (Object)null)
				{
					continue;
				}
				PlayerOutfit defaultOutfit = current.Data.DefaultOutfit;
				if (defaultOutfit != null)
				{
					bool num = !flag || defaultOutfit.HatId == info.HatId;
					bool flag4 = !flag2 || defaultOutfit.SkinId == info.SkinId;
					bool flag5 = !flag3 || defaultOutfit.VisorId == info.VisorId;
					if (num && flag4 && flag5)
					{
						val2 = current;
						break;
					}
				}
			}
			if ((Object)(object)val2 == (Object)null)
			{
				return;
			}
			Il2CppArrayBase<SpriteRenderer> componentsInChildren = ((Component)val2).GetComponentsInChildren<SpriteRenderer>(false);
			CosmeticsLayer cosmetics = val2.cosmetics;
			object obj2;
			if (cosmetics == null)
			{
				obj2 = null;
			}
			else
			{
				PlayerBodySprite currentBodySprite = cosmetics.currentBodySprite;
				obj2 = ((currentBodySprite != null) ? currentBodySprite.BodySprite : null);
			}
			SpriteRenderer val3 = (SpriteRenderer)obj2;
			Vector3 localScale = puppet.transform.localScale;
			Vector3 localPosition = default(Vector3);
			Vector3 localScale2 = default(Vector3);
			foreach (SpriteRenderer item in componentsInChildren)
			{
				if ((Object)(object)item == (Object)null || (Object)(object)item == (Object)(object)val3 || (Object)(object)item.sprite == (Object)null)
				{
					continue;
				}
				string text = ((Object)((Component)item).gameObject).name.ToLower();
				if (!text.Contains("hat") && !text.Contains("skin") && !text.Contains("visor") && !text.Contains("front") && !text.Contains("back"))
				{
					continue;
				}
				try
				{
					Vector3 val4 = ((Component)item).transform.position - ((Component)val2).transform.position;
					((Vector3)(ref localPosition))._002Ector((localScale.x != 0f) ? (val4.x / localScale.x) : 0f, (localScale.y != 0f) ? (val4.y / localScale.y) : 0f, (localScale.z != 0f) ? (val4.z / localScale.z) : 0f);
					Vector3 lossyScale = ((Component)item).transform.lossyScale;
					((Vector3)(ref localScale2))._002Ector((localScale.x != 0f) ? (lossyScale.x / localScale.x) : 1f, (localScale.y != 0f) ? (lossyScale.y / localScale.y) : 1f, (localScale.z != 0f) ? (lossyScale.z / localScale.z) : 1f);
					GameObject val5 = new GameObject("Cosmetic_" + ((Object)((Component)item).gameObject).name);
					val5.transform.SetParent(puppet.transform);
					val5.transform.localPosition = localPosition;
					val5.transform.localScale = localScale2;
					SpriteRenderer val6 = val5.AddComponent<SpriteRenderer>();
					val6.sprite = item.sprite;
					((Renderer)val6).material = TrackMaterial(new Material(((Renderer)item).material));
					((Renderer)val6).sortingOrder = ((Renderer)item).sortingOrder;
					val6.color = item.color;
					val6.flipX = false;
					if ((Object)(object)((Renderer)item).material != (Object)null && ((Renderer)item).material.HasProperty("_BodyColor"))
					{
						try
						{
							PlayerMaterial.SetColors(info.ColorId, (Renderer)val6);
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
			}
		}
		catch (Exception ex)
		{
			ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			bool flag6 = default(bool);
			BepInExWarningLogInterpolatedStringHandler val7 = new BepInExWarningLogInterpolatedStringHandler(37, 1, ref flag6);
			if (flag6)
			{
				((BepInExLogInterpolatedStringHandler)val7).AppendLiteral("[ReplayViewer] ApplyStoredCosmetics: ");
				((BepInExLogInterpolatedStringHandler)val7).AppendFormatted<string>(ex.Message);
			}
			log.LogWarning(val7);
		}
	}

	[HideFromIl2Cpp]
	private void SpawnPuppetPet(ReplayPlayerInfo info, PlayerControl source, GameObject puppetGo)
	{
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Expected O, but got Unknown
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Expected O, but got Unknown
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (string.IsNullOrEmpty(info.PetId) || info.PetId == "pet_EmptyPet")
			{
				return;
			}
			PlayerControl val = null;
			if ((Object)(object)source != (Object)null)
			{
				CosmeticsLayer cosmetics = source.cosmetics;
				if ((Object)(object)((cosmetics != null) ? cosmetics.CurrentPet : null) != (Object)null)
				{
					NetworkedPlayerInfo obj = source.Data;
					PlayerOutfit val2 = ((obj != null) ? obj.DefaultOutfit : null);
					if (val2 != null && val2.PetId == info.PetId)
					{
						val = source;
					}
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PlayerControl current = enumerator.Current;
					if ((Object)(object)current == (Object)null || (Object)(object)current.Data == (Object)null)
					{
						continue;
					}
					PlayerOutfit defaultOutfit = current.Data.DefaultOutfit;
					if (((defaultOutfit != null) ? defaultOutfit.PetId : null) == info.PetId)
					{
						CosmeticsLayer cosmetics2 = current.cosmetics;
						if ((Object)(object)((cosmetics2 != null) ? cosmetics2.CurrentPet : null) != (Object)null)
						{
							val = current;
							break;
						}
					}
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			CosmeticsLayer cosmetics3 = val.cosmetics;
			if ((Object)(object)((cosmetics3 != null) ? cosmetics3.CurrentPet : null) == (Object)null)
			{
				return;
			}
			PetBehaviour currentPet = val.cosmetics.CurrentPet;
			Il2CppArrayBase<SpriteRenderer> componentsInChildren = ((Component)currentPet).GetComponentsInChildren<SpriteRenderer>(false);
			if (componentsInChildren == null || componentsInChildren.Length == 0)
			{
				return;
			}
			GameObject val3 = new GameObject("Pet_" + info.Name);
			val3.transform.SetParent(puppetGo.transform);
			val3.transform.localPosition = new Vector3(-0.6f, -0.3f, 0.001f);
			Vector3 lossyScale = ((Component)currentPet).transform.lossyScale;
			Vector3 localScale = puppetGo.transform.localScale;
			val3.transform.localScale = new Vector3((localScale.x != 0f) ? (lossyScale.x / localScale.x) : 1f, (localScale.y != 0f) ? (lossyScale.y / localScale.y) : 1f, 1f);
			foreach (SpriteRenderer item in componentsInChildren)
			{
				if ((Object)(object)item == (Object)null || (Object)(object)item.sprite == (Object)null)
				{
					continue;
				}
				try
				{
					Vector3 localPosition = ((Component)item).transform.position - ((Component)currentPet).transform.position;
					GameObject val4 = new GameObject(((Object)((Component)item).gameObject).name);
					val4.transform.SetParent(val3.transform);
					val4.transform.localPosition = localPosition;
					val4.transform.localScale = Vector3.one;
					SpriteRenderer val5 = val4.AddComponent<SpriteRenderer>();
					val5.sprite = item.sprite;
					((Renderer)val5).material = TrackMaterial(new Material(((Renderer)item).material));
					((Renderer)val5).sortingOrder = ((Renderer)item).sortingOrder;
					val5.color = item.color;
					val5.flipX = false;
					if (((Renderer)item).material.HasProperty("_BodyColor"))
					{
						try
						{
							PlayerMaterial.SetColors(info.ColorId, (Renderer)val5);
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
			}
			puppetPets[info.PlayerId] = val3;
		}
		catch (Exception ex)
		{
			ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			bool flag = default(bool);
			BepInExWarningLogInterpolatedStringHandler val6 = new BepInExWarningLogInterpolatedStringHandler(31, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val6).AppendLiteral("[ReplayViewer] SpawnPuppetPet: ");
				((BepInExLogInterpolatedStringHandler)val6).AppendFormatted<string>(ex.Message);
			}
			log.LogWarning(val6);
		}
	}

	[HideFromIl2Cpp]
	private void SetupPuppetAnimation(ReplayPuppet puppet, GameObject puppetGo, PlayerControl source, ReplayPlayerInfo info)
	{
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Expected O, but got Unknown
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		if ((Object)(object)source == (Object)null || (Object)(object)puppet == (Object)null)
		{
			return;
		}
		bool flag = default(bool);
		try
		{
			PlayerPhysics myPhysics = source.MyPhysics;
			PlayerAnimations val = ((myPhysics != null) ? myPhysics.Animations : null);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Il2CppArrayBase<SpriteAnim> componentsInChildren = ((Component)source).GetComponentsInChildren<SpriteAnim>(true);
			if (componentsInChildren == null || componentsInChildren.Length == 0)
			{
				ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
				BepInExWarningLogInterpolatedStringHandler val2 = new BepInExWarningLogInterpolatedStringHandler(38, 1, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[ReplayViewer] No SpriteAnim found on ");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(info.Name);
				}
				log.LogWarning(val2);
				return;
			}
			try
			{
				val.PlayIdleAnimation();
			}
			catch
			{
			}
			AnimationClip[] array = (AnimationClip[])(object)new AnimationClip[componentsInChildren.Length];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				try
				{
					array[i] = componentsInChildren[i].GetCurrentAnimation();
				}
				catch
				{
				}
			}
			try
			{
				val.PlayRunAnimation();
			}
			catch
			{
			}
			SpriteAnim val3 = null;
			AnimationClip val4 = null;
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				try
				{
					AnimationClip currentAnimation = componentsInChildren[j].GetCurrentAnimation();
					if ((Object)(object)currentAnimation == (Object)null || (!((Object)(object)array[j] == (Object)null) && !(((Il2CppObjectBase)currentAnimation).Pointer != ((Il2CppObjectBase)array[j]).Pointer)))
					{
						continue;
					}
					val3 = componentsInChildren[j];
					val4 = currentAnimation;
					break;
				}
				catch
				{
				}
			}
			if ((Object)(object)val3 == (Object)null)
			{
				ManualLogSource log2 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
				BepInExWarningLogInterpolatedStringHandler val2 = new BepInExWarningLogInterpolatedStringHandler(69, 2, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[ReplayViewer] Could not identify body SpriteAnim for ");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(info.Name);
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(" (");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<int>(componentsInChildren.Length);
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(" anims found)");
				}
				log2.LogWarning(val2);
				try
				{
					val.PlayIdleAnimation();
					return;
				}
				catch
				{
					return;
				}
			}
			AnimationClip val5 = array[Array.IndexOf(Il2CppArrayBase<SpriteAnim>.op_Implicit(componentsInChildren), val3)];
			val.PlayGhostIdleAnimation();
			AnimationClip currentAnimation2 = val3.GetCurrentAnimation();
			val.PlayClimbAnimation(false);
			AnimationClip currentAnimation3 = val3.GetCurrentAnimation();
			val.PlayClimbAnimation(true);
			AnimationClip currentAnimation4 = val3.GetCurrentAnimation();
			try
			{
				val.PlayIdleAnimation();
			}
			catch
			{
			}
			puppet.SetupBodyAnimation(val4, val5, currentAnimation2, null, null, null, currentAnimation3, currentAnimation4);
			ManualLogSource log3 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			BepInExInfoLogInterpolatedStringHandler val6 = new BepInExInfoLogInterpolatedStringHandler(59, 4, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val6).AppendLiteral("[ReplayViewer] Body animation OK for ");
				((BepInExLogInterpolatedStringHandler)val6).AppendFormatted<string>(info.Name);
				((BepInExLogInterpolatedStringHandler)val6).AppendLiteral(" (Run=");
				((BepInExLogInterpolatedStringHandler)val6).AppendFormatted<bool>((Object)(object)val4 != (Object)null);
				((BepInExLogInterpolatedStringHandler)val6).AppendLiteral(", Idle=");
				((BepInExLogInterpolatedStringHandler)val6).AppendFormatted<bool>((Object)(object)val5 != (Object)null);
				((BepInExLogInterpolatedStringHandler)val6).AppendLiteral(", Ghost=");
				((BepInExLogInterpolatedStringHandler)val6).AppendFormatted<bool>((Object)(object)currentAnimation2 != (Object)null);
				((BepInExLogInterpolatedStringHandler)val6).AppendLiteral(")");
			}
			log3.LogInfo(val6);
			SetupSkinAnimation(puppet, puppetGo, source, info);
		}
		catch (Exception ex)
		{
			ManualLogSource log4 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			BepInExWarningLogInterpolatedStringHandler val2 = new BepInExWarningLogInterpolatedStringHandler(37, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[ReplayViewer] SetupPuppetAnimation: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(ex.Message);
			}
			log4.LogWarning(val2);
		}
	}

	[HideFromIl2Cpp]
	private void SetupSkinAnimation(ReplayPuppet puppet, GameObject puppetGo, PlayerControl source, ReplayPlayerInfo info)
	{
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Expected O, but got Unknown
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Expected O, but got Unknown
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Expected O, but got Unknown
		bool flag = default(bool);
		try
		{
			SkinViewData val = null;
			SpriteRenderer val2 = null;
			CosmeticsLayer cosmetics = source.cosmetics;
			SkinLayer val3 = ((cosmetics != null) ? cosmetics.CurrentSkin : null);
			if ((Object)(object)val3 != (Object)null && (Object)(object)val3.skin != (Object)null)
			{
				NetworkedPlayerInfo obj = source.Data;
				PlayerOutfit val4 = ((obj != null) ? obj.DefaultOutfit : null);
				if (val4 != null && val4.SkinId == info.SkinId)
				{
					val = val3.skin;
				}
			}
			if ((Object)(object)val == (Object)null && !string.IsNullOrEmpty(info.SkinId) && info.SkinId != "skin_None")
			{
				Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PlayerControl current = enumerator.Current;
					if ((Object)(object)current == (Object)null || (Object)(object)current.Data == (Object)null)
					{
						continue;
					}
					PlayerOutfit defaultOutfit = current.Data.DefaultOutfit;
					if (!(((defaultOutfit != null) ? defaultOutfit.SkinId : null) != info.SkinId))
					{
						CosmeticsLayer cosmetics2 = current.cosmetics;
						SkinLayer val5 = ((cosmetics2 != null) ? cosmetics2.CurrentSkin : null);
						if ((Object)(object)((val5 != null) ? val5.skin : null) != (Object)null)
						{
							val = val5.skin;
							break;
						}
					}
				}
			}
			if ((Object)(object)val == (Object)null && !string.IsNullOrEmpty(info.SkinId) && info.SkinId != "skin_None")
			{
				try
				{
					HatManager instance = DestroyableSingleton<HatManager>.Instance;
					if ((Object)(object)instance != (Object)null)
					{
						SkinData skinById = instance.GetSkinById(info.SkinId);
						if ((Object)(object)skinById != (Object)null)
						{
							AddressableAsset<SkinViewData> val6 = skinById.CreateAddressableAsset();
							if (val6 != null)
							{
								SkinViewData asset = val6.GetAsset();
								if ((Object)(object)asset != (Object)null)
								{
									val = asset;
								}
								else
								{
									ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
									BepInExInfoLogInterpolatedStringHandler val7 = new BepInExInfoLogInterpolatedStringHandler(84, 1, ref flag);
									if (flag)
									{
										((BepInExLogInterpolatedStringHandler)val7).AppendLiteral("[ReplayViewer] SkinViewData for '");
										((BepInExLogInterpolatedStringHandler)val7).AppendFormatted<string>(info.SkinId);
										((BepInExLogInterpolatedStringHandler)val7).AppendLiteral("' not loaded yet (skin animation will use fallback)");
									}
									log.LogInfo(val7);
								}
							}
						}
					}
				}
				catch
				{
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			int childCount = puppetGo.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = puppetGo.transform.GetChild(i);
				if ((Object)(object)child == (Object)null)
				{
					continue;
				}
				string text = ((Object)((Component)child).gameObject).name.ToLower();
				if (text.Contains("skin") && !text.Contains("cosmetic"))
				{
					val2 = ((Component)child).GetComponent<SpriteRenderer>();
					if ((Object)(object)val2 != (Object)null)
					{
						break;
					}
				}
			}
			if ((Object)(object)val2 == (Object)null)
			{
				for (int j = 0; j < childCount; j++)
				{
					Transform child2 = puppetGo.transform.GetChild(j);
					if (!((Object)(object)child2 == (Object)null) && ((Object)((Component)child2).gameObject).name.ToLower().Contains("skin"))
					{
						val2 = ((Component)child2).GetComponent<SpriteRenderer>();
						if ((Object)(object)val2 != (Object)null)
						{
							break;
						}
					}
				}
			}
			if ((Object)(object)val2 != (Object)null && (Object)(object)val != (Object)null)
			{
				puppet.SetupSkinAnimation(val2, val);
				ManualLogSource log2 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
				BepInExInfoLogInterpolatedStringHandler val7 = new BepInExInfoLogInterpolatedStringHandler(53, 2, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val7).AppendLiteral("[ReplayViewer] Skin animation setup OK for ");
					((BepInExLogInterpolatedStringHandler)val7).AppendFormatted<string>(info.Name);
					((BepInExLogInterpolatedStringHandler)val7).AppendLiteral(" (SkinId=");
					((BepInExLogInterpolatedStringHandler)val7).AppendFormatted<string>(info.SkinId);
					((BepInExLogInterpolatedStringHandler)val7).AppendLiteral(")");
				}
				log2.LogInfo(val7);
			}
		}
		catch (Exception ex)
		{
			ManualLogSource log3 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			BepInExWarningLogInterpolatedStringHandler val8 = new BepInExWarningLogInterpolatedStringHandler(35, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val8).AppendLiteral("[ReplayViewer] SetupSkinAnimation: ");
				((BepInExLogInterpolatedStringHandler)val8).AppendFormatted<string>(ex.Message);
			}
			log3.LogWarning(val8);
		}
	}

	[HideFromIl2Cpp]
	private void LoadCosmeticsFromAssets(GameObject puppet, ReplayPlayerInfo info, PlayerControl sourcePlayer, ReplayPuppet puppetComp)
	{
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		try
		{
			if ((Object)(object)sourcePlayer != (Object)null)
			{
				NetworkedPlayerInfo obj = sourcePlayer.Data;
				PlayerOutfit val = ((obj != null) ? obj.DefaultOutfit : null);
				if (val != null && val.HatId == info.HatId && val.SkinId == info.SkinId && val.VisorId == info.VisorId)
				{
					return;
				}
			}
			HatManager instance = DestroyableSingleton<HatManager>.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				return;
			}
			for (int num = puppet.transform.childCount - 1; num >= 0; num--)
			{
				string text = ((Object)((Component)puppet.transform.GetChild(num)).gameObject).name.ToLower();
				if (text.Contains("hat") || text.Contains("skin") || text.Contains("visor") || text.Contains("cosmetic") || text.Contains("front") || text.Contains("back"))
				{
					Object.Destroy((Object)(object)((Component)puppet.transform.GetChild(num)).gameObject);
				}
			}
			((MonoBehaviour)this).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(CoLoadCosmeticsFromAssets(puppet, info, instance, puppetComp)));
		}
		catch (Exception ex)
		{
			ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			bool flag = default(bool);
			BepInExWarningLogInterpolatedStringHandler val2 = new BepInExWarningLogInterpolatedStringHandler(40, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[ReplayViewer] LoadCosmeticsFromAssets: ");
				((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(ex.Message);
			}
			log.LogWarning(val2);
		}
	}

	[IteratorStateMachine(typeof(_003CCoLoadCosmeticsFromAssets_003Ed__69))]
	[HideFromIl2Cpp]
	private IEnumerator CoLoadCosmeticsFromAssets(GameObject puppet, ReplayPlayerInfo info, HatManager hm, ReplayPuppet puppetComp)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoLoadCosmeticsFromAssets_003Ed__69(0)
		{
			puppet = puppet,
			info = info,
			hm = hm,
			puppetComp = puppetComp
		};
	}

	private void InitializeRoutes()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		foreach (ReplayPlayerInfo player in data.Players)
		{
			GameObject val = new GameObject("Route_" + player.Name);
			val.transform.SetParent(puppetRoot.transform);
			LineRenderer val2 = val.AddComponent<LineRenderer>();
			val2.startWidth = 0.1f;
			val2.endWidth = 0.05f;
			((Renderer)val2).material = TrackMaterial(new Material(Shader.Find("Sprites/Default")));
			Color val3 = Color32.op_Implicit(player.RealColor);
			val3.a = 0.6f;
			val2.startColor = val3;
			val2.endColor = new Color(val3.r, val3.g, val3.b, 0.1f);
			val2.positionCount = 0;
			((Renderer)val2).sortingOrder = 3;
			routeLines[player.PlayerId] = val2;
			routePoints[player.PlayerId] = new List<Vector3>();
		}
	}

	private void InitializeUI()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		GameObject val = new GameObject("ReplayViewer_UI");
		Object.DontDestroyOnLoad((Object)(object)val);
		ui = val.AddComponent<ReplayUI>();
		ui.Initialize(this);
	}

	private Sprite CreateCrewmateBodySprite()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		int w = 64;
		int h = 80;
		Texture2D val = TrackTexture(new Texture2D(w, h, (TextureFormat)4, false));
		((Texture)val).filterMode = (FilterMode)0;
		Color val2 = default(Color);
		((Color)(ref val2))._002Ector(0f, 0f, 0f, 0f);
		Color white = Color.white;
		Color arg = default(Color);
		((Color)(ref arg))._002Ector(0.7f, 0.7f, 0.7f, 1f);
		Color[] colors = (Color[])(object)new Color[w * h];
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = val2;
		}
		Action<int, int, Color> action = delegate(int x, int y, Color c)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (x >= 0 && x < w && y >= 0 && y < h)
			{
				colors[y * w + x] = c;
			}
		};
		for (int j = 22; j < 52; j++)
		{
			float num = 37f;
			float num2 = 15f;
			float num3 = ((float)j - num) / num2;
			if (!(num3 * num3 > 1f))
			{
				float num4 = 10f * Mathf.Sqrt(1f - num3 * num3);
				for (int k = (int)(13f - num4); k < 13; k++)
				{
					action(k, j, arg);
				}
			}
		}
		int num5 = 34;
		int num6 = 42;
		for (int l = 18; l < h; l++)
		{
			float num7 = (float)(l - num6) / 30f;
			float num8 = ((num7 < 0f) ? (22f + num7 * 4f) : (22f - num7 * 6f));
			if (num8 < 4f)
			{
				continue;
			}
			for (int m = 0; m < w; m++)
			{
				float num9 = (float)(m - num5) / num8;
				if (num9 * num9 <= 1f)
				{
					action(m, l, white);
				}
			}
		}
		for (int n = 5; n < 20; n++)
		{
			for (int num10 = 22; num10 < 31; num10++)
			{
				action(num10, n, white);
			}
			for (int num11 = 37; num11 < 46; num11++)
			{
				action(num11, n, white);
			}
		}
		for (int num12 = 3; num12 < 5; num12++)
		{
			for (int num13 = 23; num13 < 30; num13++)
			{
				action(num13, num12, white);
			}
			for (int num14 = 38; num14 < 45; num14++)
			{
				action(num14, num12, white);
			}
		}
		val.SetPixels(Il2CppStructArray<Color>.op_Implicit(colors));
		val.Apply();
		return Sprite.Create(val, new Rect(0f, 0f, (float)w, (float)h), new Vector2(0.5f, 0.25f), 48f);
	}

	private Sprite CreateVisorSprite()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		int num = 64;
		int num2 = 80;
		Texture2D val = TrackTexture(new Texture2D(num, num2, (TextureFormat)4, false));
		((Texture)val).filterMode = (FilterMode)0;
		Color val2 = default(Color);
		((Color)(ref val2))._002Ector(0f, 0f, 0f, 0f);
		Color val3 = default(Color);
		((Color)(ref val3))._002Ector(0.2f, 0.8f, 0.95f, 1f);
		Color val4 = default(Color);
		((Color)(ref val4))._002Ector(0.5f, 0.92f, 1f, 1f);
		Color[] array = (Color[])(object)new Color[num * num2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = val2;
		}
		float num3 = 43f;
		float num4 = 53f;
		float num5 = 12f;
		float num6 = 8f;
		for (int j = 0; j < num2; j++)
		{
			for (int k = 0; k < num; k++)
			{
				float num7 = ((float)k - num3) / num5;
				float num8 = ((float)j - num4) / num6;
				if (num7 * num7 + num8 * num8 <= 1f)
				{
					bool flag = num7 > 0.2f && num8 > 0.1f && num7 * num7 + num8 * num8 > 0.4f;
					array[j * num + k] = (flag ? val4 : val3);
				}
			}
		}
		val.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
		val.Apply();
		return Sprite.Create(val, new Rect(0f, 0f, (float)num, (float)num2), new Vector2(0.5f, 0.25f), 48f);
	}

	private void ApplyPlayerColor(SpriteRenderer rend, Color realColor, int colorId)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rend == (Object)null || (Object)(object)((Renderer)rend).material == (Object)null)
		{
			return;
		}
		try
		{
			bool flag = ((Renderer)rend).material.HasProperty("_BodyColor");
			bool flag2 = ((Renderer)rend).material.HasProperty("_BackColor");
			if (colorId >= 0 && colorId < ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length && (flag || flag2))
			{
				if (flag)
				{
					((Renderer)rend).material.SetColor("_BodyColor", Color32.op_Implicit(((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors)[colorId]));
				}
				if (flag2)
				{
					((Renderer)rend).material.SetColor("_BackColor", Color32.op_Implicit(((Il2CppArrayBase<Color32>)(object)Palette.ShadowColors)[colorId]));
				}
				if (((Renderer)rend).material.HasProperty("_VisorColor"))
				{
					((Renderer)rend).material.SetColor("_VisorColor", Color32.op_Implicit(Palette.VisorColor));
				}
			}
			else if (colorId >= 0 && colorId < ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length)
			{
				rend.color = Color32.op_Implicit(((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors)[colorId]);
			}
			else
			{
				rend.color = realColor;
			}
		}
		catch
		{
			rend.color = realColor;
		}
	}

	private void Update()
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		if (!isActive || data == null)
		{
			return;
		}
		try
		{
			float num = Mathf.Min(Time.deltaTime, 0.1f);
			if (!isPaused)
			{
				currentTime += num * playbackSpeed;
				if (currentTime >= TotalDuration)
				{
					currentTime = TotalDuration;
					isPaused = true;
				}
			}
			ApplyFrameState(currentTime);
			UpdateCamera();
			if ((Object)(object)ui != (Object)null && ui.ShowRoutes)
			{
				UpdateRoutes();
			}
			else
			{
				HideRoutes();
			}
			if ((Object)(object)ui != (Object)null && ui.ShowHeatmap)
			{
				UpdateHeatmap();
			}
		}
		catch (Exception ex)
		{
			if (Time.frameCount % 120 == 0)
			{
				ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
				bool flag = default(bool);
				BepInExErrorLogInterpolatedStringHandler val = new BepInExErrorLogInterpolatedStringHandler(29, 1, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ReplayViewer] Update error: ");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(ex.Message);
				}
				log.LogError(val);
			}
		}
	}

	private void ApplyFrameState(float time)
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		if (data == null || data.Frames.Count == 0)
		{
			return;
		}
		int num = FindFrameIndex(time);
		if (num < 0 || num >= data.Frames.Count)
		{
			return;
		}
		ReplayFrame replayFrame = data.Frames[num];
		ReplayFrame replayFrame2 = ((num < data.Frames.Count - 1) ? data.Frames[num + 1] : null);
		float num2 = 0f;
		if (replayFrame2 != null)
		{
			float num3 = replayFrame2.Time - replayFrame.Time;
			if (num3 > 0.001f)
			{
				num2 = Mathf.Clamp01((time - replayFrame.Time) / num3);
			}
		}
		_nextFrameLookup.Clear();
		if (replayFrame2 != null)
		{
			foreach (PlayerState state in replayFrame2.States)
			{
				_nextFrameLookup[state.PlayerId] = state;
			}
		}
		foreach (PlayerState state2 in replayFrame.States)
		{
			if (!puppets.TryGetValue(state2.PlayerId, out var value) || !((Object)(object)value != (Object)null))
			{
				continue;
			}
			Vector2 pos = state2.Position;
			bool faceRight = state2.FaceRight;
			AnimState animState = state2.AnimState;
			if (replayFrame2 != null && _nextFrameLookup.TryGetValue(state2.PlayerId, out var value2))
			{
				pos = Vector2.Lerp(state2.Position, value2.Position, num2);
				faceRight = ((num2 > 0.5f) ? value2.FaceRight : state2.FaceRight);
				animState = state2.AnimState;
			}
			bool flag = state2.IsDead && (Object)(object)ui != (Object)null && !ui.ShowGhosts;
			bool activeSelf = ((Component)value).gameObject.activeSelf;
			if (flag && activeSelf)
			{
				((Component)value).gameObject.SetActive(false);
			}
			else if (!flag && !activeSelf)
			{
				((Component)value).gameObject.SetActive(true);
			}
			if (!flag)
			{
				value.UpdateState(pos, faceRight, state2.IsDead, animState);
			}
			if (!state2.IsDead || deadBodies.ContainsKey(state2.PlayerId))
			{
				continue;
			}
			Vector2 position = state2.Position;
			if (data.Events != null)
			{
				foreach (ReplayEvent @event in data.Events)
				{
					if (@event.Type == ReplayEventType.Kill && @event.TargetId == state2.PlayerId)
					{
						position = @event.Position;
						break;
					}
				}
			}
			SpawnDeadBody(state2.PlayerId, position);
		}
		UpdateDeadBodyVisibility(time);
	}

	private void SpawnDeadBody(byte playerId, Vector2 position)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		if (deadBodies.ContainsKey(playerId))
		{
			return;
		}
		ReplayPlayerInfo replayPlayerInfo = data.Players.Find((ReplayPlayerInfo p) => p.PlayerId == playerId);
		if (replayPlayerInfo != null)
		{
			GameObject val = new GameObject("DeadBody_" + replayPlayerInfo.Name);
			val.transform.SetParent(puppetRoot.transform);
			val.transform.position = new Vector3(position.x, position.y, 0f);
			SpriteRenderer val2 = val.AddComponent<SpriteRenderer>();
			val2.sprite = CreateDeadBodySprite();
			if (replayPlayerInfo.ColorId >= 0 && replayPlayerInfo.ColorId < ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length)
			{
				val2.color = Color32.op_Implicit(((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors)[replayPlayerInfo.ColorId]);
			}
			else
			{
				val2.color = Color32.op_Implicit(replayPlayerInfo.RealColor);
			}
			((Renderer)val2).sortingOrder = 4;
			GameObject val3 = new GameObject("Bone");
			val3.transform.SetParent(val.transform);
			val3.transform.localPosition = new Vector3(0.3f, 0f, -0.01f);
			SpriteRenderer obj = val3.AddComponent<SpriteRenderer>();
			obj.sprite = CreateBoneSprite();
			((Renderer)obj).sortingOrder = 5;
			deadBodies[playerId] = val;
			ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			bool flag = default(bool);
			BepInExInfoLogInterpolatedStringHandler val4 = new BepInExInfoLogInterpolatedStringHandler(41, 2, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val4).AppendLiteral("[ReplayViewer] Spawned dead body for ");
				((BepInExLogInterpolatedStringHandler)val4).AppendFormatted<string>(replayPlayerInfo.Name);
				((BepInExLogInterpolatedStringHandler)val4).AppendLiteral(" at ");
				((BepInExLogInterpolatedStringHandler)val4).AppendFormatted<Vector2>(position);
			}
			log.LogInfo(val4);
		}
	}

	private Sprite CreateDeadBodySprite()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		int num = 80;
		int num2 = 40;
		Texture2D val = TrackTexture(new Texture2D(num, num2, (TextureFormat)4, false));
		((Texture)val).filterMode = (FilterMode)0;
		Color val2 = default(Color);
		((Color)(ref val2))._002Ector(0f, 0f, 0f, 0f);
		Color white = Color.white;
		Color[] array = (Color[])(object)new Color[num * num2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = val2;
		}
		int num3 = 40;
		int num4 = 20;
		int num5 = 35;
		int num6 = 15;
		for (int j = 0; j < num2; j++)
		{
			for (int k = 0; k < num; k++)
			{
				float num7 = (float)(k - num3) / (float)num5;
				float num8 = (float)(j - num4) / (float)num6;
				if (num7 * num7 + num8 * num8 <= 1f)
				{
					array[j * num + k] = white;
				}
			}
		}
		val.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
		val.Apply();
		return Sprite.Create(val, new Rect(0f, 0f, (float)num, (float)num2), new Vector2(0.5f, 0.5f), 48f);
	}

	private Sprite CreateBoneSprite()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		int num = 16;
		int num2 = 16;
		Texture2D val = TrackTexture(new Texture2D(num, num2, (TextureFormat)4, false));
		((Texture)val).filterMode = (FilterMode)0;
		Color val2 = default(Color);
		((Color)(ref val2))._002Ector(0f, 0f, 0f, 0f);
		Color val3 = default(Color);
		((Color)(ref val3))._002Ector(0.9f, 0.85f, 0.8f, 1f);
		Color[] array = (Color[])(object)new Color[num * num2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = val2;
		}
		for (int j = 0; j < num; j++)
		{
			array[j * num + j] = val3;
			array[j * num + (num - 1 - j)] = val3;
		}
		val.SetPixels(Il2CppStructArray<Color>.op_Implicit(array));
		val.Apply();
		return Sprite.Create(val, new Rect(0f, 0f, (float)num, (float)num2), new Vector2(0.5f, 0.5f), 32f);
	}

	private void UpdateDeadBodyVisibility(float time)
	{
		if (data == null || data.Events == null)
		{
			return;
		}
		foreach (KeyValuePair<byte, GameObject> deadBody in deadBodies)
		{
			byte key = deadBody.Key;
			GameObject value = deadBody.Value;
			if ((Object)(object)value == (Object)null)
			{
				continue;
			}
			float num = 0f;
			foreach (ReplayEvent @event in data.Events)
			{
				if (@event.Type == ReplayEventType.Kill && @event.TargetId == key)
				{
					num = @event.Time;
					break;
				}
			}
			value.SetActive(time >= num);
		}
	}

	private int FindFrameIndex(float time)
	{
		List<ReplayFrame> frames = data.Frames;
		if (frames.Count == 0)
		{
			return -1;
		}
		if (lastFrameIndex >= 0 && lastFrameIndex < frames.Count - 1 && frames[lastFrameIndex].Time <= time && frames[lastFrameIndex + 1].Time > time)
		{
			return lastFrameIndex;
		}
		int num = 0;
		int num2 = frames.Count - 1;
		while (num < num2)
		{
			int num3 = (num + num2 + 1) / 2;
			if (frames[num3].Time <= time)
			{
				num = num3;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		lastFrameIndex = num;
		return num;
	}

	private void UpdateCamera()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)replayCamera == (Object)null)
		{
			return;
		}
		if (followingPlayerId.HasValue && puppets.TryGetValue(followingPlayerId.Value, out var value) && (Object)(object)value != (Object)null && (Object)(object)((Component)value).transform != (Object)null && ((Component)value).gameObject.activeSelf)
		{
			Vector3 position = ((Component)value).transform.position;
			if (Mathf.Abs(((Component)value).transform.localScale.x) > 0.1f)
			{
				position.z = -10f;
				((Component)replayCamera).transform.position = Vector3.Lerp(((Component)replayCamera).transform.position, position, Mathf.Min(Time.deltaTime, 0.1f) * 5f);
			}
		}
		replayCamera.orthographicSize = cameraZoom;
	}

	private void UpdateRoutes()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (data == null)
		{
			return;
		}
		int num = FindFrameIndex(currentTime);
		if (num < 0)
		{
			return;
		}
		Vector3 val = default(Vector3);
		foreach (PlayerState state in data.Frames[num].States)
		{
			if (!routePoints.ContainsKey(state.PlayerId) || !routeLines.ContainsKey(state.PlayerId))
			{
				continue;
			}
			List<Vector3> list = routePoints[state.PlayerId];
			LineRenderer obj = routeLines[state.PlayerId];
			((Vector3)(ref val))._002Ector(state.Position.x, state.Position.y, 0f);
			if (list.Count == 0 || Vector3.Distance(list[list.Count - 1], val) > 0.2f)
			{
				list.Add(val);
				if (list.Count > 500)
				{
					list.RemoveAt(0);
				}
			}
			if (!_routeArrayCache.TryGetValue(state.PlayerId, out var value) || value.Length < list.Count)
			{
				value = (Vector3[])(object)new Vector3[Mathf.Max(list.Count, 500)];
				_routeArrayCache[state.PlayerId] = value;
			}
			list.CopyTo(value);
			obj.positionCount = list.Count;
			obj.SetPositions(Il2CppStructArray<Vector3>.op_Implicit(value));
		}
	}

	private void HideRoutes()
	{
		foreach (LineRenderer value in routeLines.Values)
		{
			if (Object.op_Implicit((Object)(object)value))
			{
				value.positionCount = 0;
			}
		}
	}

	private void UpdateHeatmap()
	{
	}

	private void SetLocalPlayerVisibility(bool visible)
	{
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			return;
		}
		try
		{
			foreach (Renderer componentsInChild in ((Component)PlayerControl.LocalPlayer).GetComponentsInChildren<Renderer>(true))
			{
				componentsInChild.enabled = visible;
			}
		}
		catch
		{
		}
		try
		{
			foreach (TextMeshPro componentsInChild2 in ((Component)PlayerControl.LocalPlayer).GetComponentsInChildren<TextMeshPro>(true))
			{
				((Behaviour)componentsInChild2).enabled = visible;
			}
		}
		catch
		{
		}
		try
		{
			foreach (Canvas componentsInChild3 in ((Component)PlayerControl.LocalPlayer).GetComponentsInChildren<Canvas>(true))
			{
				((Behaviour)componentsInChild3).enabled = visible;
			}
		}
		catch
		{
		}
	}

	private void HideRealPlayers()
	{
		try
		{
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if (Object.op_Implicit((Object)(object)current) && Object.op_Implicit((Object)(object)((Component)current).gameObject))
				{
					((Component)current).gameObject.SetActive(false);
				}
			}
		}
		catch
		{
		}
	}

	private void ShowRealPlayers()
	{
		try
		{
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if (Object.op_Implicit((Object)(object)current) && Object.op_Implicit((Object)(object)((Component)current).gameObject))
				{
					((Component)current).gameObject.SetActive(true);
				}
			}
		}
		catch
		{
		}
		try
		{
			if (Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer) && Object.op_Implicit((Object)(object)DestroyableSingleton<HudManager>.Instance))
			{
				DestroyableSingleton<HudManager>.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, !PlayerControl.LocalPlayer.Data.IsDead);
			}
		}
		catch
		{
		}
	}
}
