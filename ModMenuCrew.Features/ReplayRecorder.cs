using System;
using System.Collections.Generic;
using System.IO;
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using ModMenuCrew.ReplaySystem;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew.Features;

public class ReplayRecorder : MonoBehaviour
{
	public static ReplayRecorder Instance;

	[NonSerialized]
	private ReplayData currentReplay;

	private bool isRecording;

	private float recordingStartTime;

	private string replayDirectory;

	private HashSet<byte> deadPlayers = new HashSet<byte>();

	private bool wasInMeeting;

	internal static bool ReplayInProgress;

	private static float _replayInProgressSince;

	private const float REPLAY_FLAG_TIMEOUT = 1800f;

	private int currentGameId = -1;

	private bool hasRecordedThisGame;

	private GameStates lastGameState;

	internal static bool AutoRecordEnabled;

	[NonSerialized]
	private Dictionary<byte, byte> playerAnimStates = new Dictionary<byte, byte>();

	private bool wasLightsSabotaged;

	private bool wasReactorActive;

	private bool wasO2Active;

	private bool wasCommsActive;

	private SwitchSystem _cachedSwitchSys;

	private ReactorSystemType _cachedReactorSys;

	private LifeSuppSystemType _cachedO2Sys;

	private HudOverrideSystemType _cachedCommsSys;

	private bool _systemsCached;

	private float _lastRecordTime;

	private const float RECORD_INTERVAL = 0.05f;

	private bool showReplayMenu;

	private Vector2 replayListScroll = Vector2.zero;

	private string[] cachedReplayFiles = new string[0];

	private float lastFileRefresh;

	private static GUIStyle _replayCenterLabelStyle;

	private static GUIStyle _replayBoxStyle;

	private static GUIStyle _replayBtnStyle;

	private static Texture2D _replayBoxTex;

	private static Texture2D _replayBtnTex;

	private static Texture2D _replayBtnHoverTex;

	private static Texture2D _replayBtnActiveTex;

	public bool IsRecording => isRecording;

	public int FrameCount => (currentReplay?.Frames?.Count).GetValueOrDefault();

	public int EventCount => (currentReplay?.Events?.Count).GetValueOrDefault();

	public ReplayRecorder(IntPtr ptr)
		: base(ptr)
	{
	}//IL_001e: Unknown result type (might be due to invalid IL or missing references)
	//IL_0023: Unknown result type (might be due to invalid IL or missing references)


	private void Awake()
	{
		Instance = this;
		replayDirectory = Path.Combine(Paths.GameRootPath, "Replays");
		if (!Directory.Exists(replayDirectory))
		{
			Directory.CreateDirectory(replayDirectory);
		}
	}

	public void StartRecording()
	{
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Invalid comparison between Unknown and I4
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected I4, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected I4, but got Unknown
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		if (isRecording || !Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			return;
		}
		if (ReplayInProgress)
		{
			((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogWarning((object)"[ReplayRecorder] Cannot start recording: Replay in progress");
			return;
		}
		currentReplay = new ReplayData();
		currentReplay.GameVersion = "2026.2.17";
		currentReplay.RecordedAt = DateTime.Now;
		if (Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			currentReplay.MapId = (int)ShipStatus.Instance.Type;
			currentReplay.MapName = GetMapName((int)ShipStatus.Instance.Type);
		}
		try
		{
			GameManager instance = GameManager.Instance;
			LogicOptions val = ((instance != null) ? instance.LogicOptions : null);
			GameOptionsManager instance2 = GameOptionsManager.Instance;
			IGameOptions val2 = ((instance2 != null) ? instance2.CurrentGameOptions : null);
			if (val != null)
			{
				currentReplay.Settings = new ReplayGameSettings
				{
					PlayerSpeed = val.GetPlayerSpeedMod(PlayerControl.LocalPlayer),
					KillCooldown = val.GetKillCooldown(),
					KillDistance = val.GetKillDistance(),
					EmergencyButtonCooldown = val.GetEmergencyCooldown(),
					ConfirmEjects = val.GetConfirmImpostor(),
					AnonymousVotes = val.GetAnonymousVotes(),
					VisualTasks = val.GetVisualTasks()
				};
				if (val2 != null)
				{
					try
					{
						currentReplay.Settings.CrewmateVision = val2.GetFloat((FloatOptionNames)4);
					}
					catch
					{
					}
					try
					{
						currentReplay.Settings.ImpostorVision = val2.GetFloat((FloatOptionNames)3);
					}
					catch
					{
					}
					try
					{
						currentReplay.Settings.NumCommonTasks = val2.GetInt((Int32OptionNames)10);
					}
					catch
					{
					}
					try
					{
						currentReplay.Settings.NumLongTasks = val2.GetInt((Int32OptionNames)12);
					}
					catch
					{
					}
					try
					{
						currentReplay.Settings.NumShortTasks = val2.GetInt((Int32OptionNames)11);
					}
					catch
					{
					}
					try
					{
						ReplayGameSettings settings = currentReplay.Settings;
						GameModes gameMode = val2.GameMode;
						settings.GameMode = ((object)(GameModes)(ref gameMode)).ToString();
					}
					catch
					{
					}
					try
					{
						currentReplay.Settings.MaxEmergencyCalls = val2.GetInt((Int32OptionNames)3);
					}
					catch
					{
					}
				}
			}
		}
		catch
		{
		}
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current == (Object)null || (Object)(object)current.Data == (Object)null)
			{
				continue;
			}
			PlayerOutfit defaultOutfit = current.Data.DefaultOutfit;
			string roleName = "Crewmate";
			bool isImpostorTeam = false;
			try
			{
				RoleBehaviour role = current.Data.Role;
				if ((Object)(object)role != (Object)null)
				{
					RoleTypes role2 = role.Role;
					roleName = ((object)(RoleTypes)role2.ToString();
					isImpostorTeam = role.IsImpostor;
				}
			}
			catch
			{
			}
			ReplayPlayerInfo obj10 = new ReplayPlayerInfo
			{
				PlayerId = current.PlayerId,
				Name = current.Data.PlayerName,
				ColorId = defaultOutfit.ColorId,
				HatId = (defaultOutfit.HatId ?? ""),
				SkinId = (defaultOutfit.SkinId ?? ""),
				PetId = (defaultOutfit.PetId ?? ""),
				VisorId = (defaultOutfit.VisorId ?? ""),
				NamePlateId = (defaultOutfit.NamePlateId ?? "")
			};
			RoleBehaviour role3 = current.Data.Role;
			obj10.IsImpostor = role3 != null && role3.IsImpostor;
			obj10.RealColor = GetRealColor(current);
			obj10.RoleName = roleName;
			obj10.IsImpostorTeam = isImpostorTeam;
			ReplayPlayerInfo item = obj10;
			currentReplay.Players.Add(item);
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		AddEvent(ReplayEventType.GameStart, (byte)((localPlayer != null) ? localPlayer.PlayerId : 0), 0, Vector2.zero, "Game Started");
		deadPlayers.Clear();
		playerAnimStates.Clear();
		wasInMeeting = false;
		wasLightsSabotaged = false;
		wasReactorActive = false;
		wasO2Active = false;
		wasCommsActive = false;
		_systemsCached = false;
		_cachedSwitchSys = null;
		_cachedReactorSys = null;
		_cachedO2Sys = null;
		_cachedCommsSys = null;
		try
		{
			ShipStatus instance3 = ShipStatus.Instance;
			if (((instance3 != null) ? instance3.Systems : null) != null)
			{
				Dictionary<SystemTypes, ISystemType> systems = ShipStatus.Instance.Systems;
				try
				{
					if (systems.ContainsKey((SystemTypes)7))
					{
						SwitchSystem obj11 = ((Il2CppObjectBase)systems[(SystemTypes)7]).TryCast<SwitchSystem>();
						wasLightsSabotaged = obj11 != null && obj11.IsActive;
					}
				}
				catch
				{
				}
				try
				{
					SystemTypes val3 = (SystemTypes)(((int)ShipStatus.Instance.Type == 2) ? 21 : 3);
					if (systems.ContainsKey(val3))
					{
						ReactorSystemType obj13 = ((Il2CppObjectBase)systems[val3]).TryCast<ReactorSystemType>();
						wasReactorActive = obj13 != null && obj13.IsActive;
					}
				}
				catch
				{
				}
				try
				{
					if (systems.ContainsKey((SystemTypes)8))
					{
						LifeSuppSystemType obj15 = ((Il2CppObjectBase)systems[(SystemTypes)8]).TryCast<LifeSuppSystemType>();
						wasO2Active = obj15 != null && obj15.IsActive;
					}
				}
				catch
				{
				}
				try
				{
					if (systems.ContainsKey((SystemTypes)14))
					{
						HudOverrideSystemType obj17 = ((Il2CppObjectBase)systems[(SystemTypes)14]).TryCast<HudOverrideSystemType>();
						wasCommsActive = obj17 != null && obj17.IsActive;
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
		isRecording = true;
		recordingStartTime = Time.time;
		_lastRecordTime = -0.05f;
		((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogInfo((object)"[ReplayRecorder] Recording Started");
	}

	private void FixedUpdate()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Invalid comparison between Unknown and I4
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Invalid comparison between Unknown and I4
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Invalid comparison between Unknown and I4
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Invalid comparison between Unknown and I4
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)AmongUsClient.Instance == (Object)null)
		{
			return;
		}
		if (ReplayInProgress)
		{
			if (!(_replayInProgressSince > 0f) || !(Time.realtimeSinceStartup - _replayInProgressSince > 1800f))
			{
				return;
			}
			((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogWarning((object)"[ReplayRecorder] ReplayInProgress flag stuck for 30min — auto-resetting");
			ReplayInProgress = false;
			_replayInProgressSince = 0f;
		}
		GameStates gameState = ((InnerNetClient)AmongUsClient.Instance).GameState;
		if (gameState != lastGameState)
		{
			if ((int)lastGameState == 2 && (int)gameState != 2)
			{
				if (isRecording)
				{
					StopRecording();
				}
				hasRecordedThisGame = false;
			}
			if ((int)gameState == 2)
			{
				int gameId = ((InnerNetClient)AmongUsClient.Instance).GameId;
				if (gameId != currentGameId)
				{
					currentGameId = gameId;
					hasRecordedThisGame = false;
					ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
					bool flag = default(bool);
					BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(39, 1, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ReplayRecorder] New game detected: ID ");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>(currentGameId);
					}
					log.LogInfo(val);
				}
			}
			lastGameState = gameState;
		}
		if (!isRecording)
		{
			if ((int)gameState == 2 && !ReplayInProgress && !hasRecordedThisGame && AutoRecordEnabled)
			{
				StartRecording();
				hasRecordedThisGame = true;
			}
			return;
		}
		if ((int)gameState != 2)
		{
			StopRecording();
			return;
		}
		float num = Time.time - recordingStartTime;
		if (num - _lastRecordTime < 0.05f)
		{
			return;
		}
		_lastRecordTime = num;
		ReplayFrame replayFrame = new ReplayFrame
		{
			Time = num
		};
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current == (Object)null || (Object)(object)current.Data == (Object)null)
			{
				continue;
			}
			AnimState animState = AnimState.Idle;
			if (playerAnimStates.TryGetValue(current.PlayerId, out var value))
			{
				animState = (AnimState)value;
				if (animState == AnimState.VentEnter || animState == AnimState.VentExit || animState == AnimState.Climb)
				{
					playerAnimStates[current.PlayerId] = 0;
				}
			}
			else
			{
				try
				{
					if ((Object)(object)current.MyPhysics != (Object)null)
					{
						if ((Object)(object)current.MyPhysics.Animations != (Object)null && current.MyPhysics.Animations.IsPlayingClimbAnimation())
						{
							animState = AnimState.Climb;
						}
						else if ((Object)(object)current.MyPhysics.Animations != (Object)null && current.MyPhysics.Animations.IsPlayingRunAnimation())
						{
							animState = AnimState.Run;
						}
						else if ((Object)(object)current.MyPhysics.body != (Object)null)
						{
							Vector2 velocity = current.MyPhysics.body.velocity;
							if ((velocity).magnitude > 0.1f)
							{
								animState = AnimState.Run;
							}
						}
					}
				}
				catch
				{
				}
			}
			if (current.Data.IsDead)
			{
				animState = AnimState.Ghost;
			}
			if (current.inVent && animState == AnimState.Idle)
			{
				animState = AnimState.VentEnter;
			}
			PlayerState playerState = default(PlayerState);
			playerState.PlayerId = current.PlayerId;
			playerState.Position = Vector2.op_Implicit(((Component)current).transform.position);
			playerState.FaceRight = true;
			playerState.IsDead = current.Data.IsDead;
			playerState.IsInVent = current.inVent;
			playerState.AnimState = animState;
			PlayerState item = playerState;
			try
			{
				CosmeticsLayer cosmetics = current.cosmetics;
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
				SpriteRenderer val2 = (SpriteRenderer)obj2;
				if ((Object)(object)val2 != (Object)null)
				{
					item.FaceRight = !val2.flipX;
				}
				else
				{
					SpriteRenderer component = ((Component)current).GetComponent<SpriteRenderer>();
					if (Object.op_Implicit((Object)(object)component))
					{
						item.FaceRight = !component.flipX;
					}
				}
			}
			catch
			{
			}
			replayFrame.States.Add(item);
			if (current.Data.IsDead && !deadPlayers.Contains(current.PlayerId))
			{
				deadPlayers.Add(current.PlayerId);
				byte b = FindLikelyKiller(current);
				string text = ((b != byte.MaxValue) ? GetPlayerName(b) : "?");
				AddEvent(ReplayEventType.Kill, b, current.PlayerId, Vector2.op_Implicit(((Component)current).transform.position), text + " killed " + current.Data.PlayerName);
			}
		}
		currentReplay.Frames.Add(replayFrame);
		bool flag2 = (Object)(object)MeetingHud.Instance != (Object)null;
		wasInMeeting = flag2;
		DetectSabotageEvents();
	}

	private byte FindLikelyKiller(PlayerControl victim)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		byte result = byte.MaxValue;
		float num = float.MaxValue;
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current == (Object)null || (Object)(object)current.Data == (Object)null)
			{
				continue;
			}
			RoleBehaviour role = current.Data.Role;
			if (role != null && role.IsImpostor && !current.Data.IsDead)
			{
				float num2 = Vector2.Distance(Vector2.op_Implicit(((Component)current).transform.position), Vector2.op_Implicit(((Component)victim).transform.position));
				if (num2 < num)
				{
					num = num2;
					result = current.PlayerId;
				}
			}
		}
		return result;
	}

	private string GetPlayerName(byte playerId)
	{
		Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PlayerControl current = enumerator.Current;
			if ((Object)(object)current != (Object)null && current.PlayerId == playerId && (Object)(object)current.Data != (Object)null)
			{
				return current.Data.PlayerName;
			}
		}
		return "Unknown";
	}

	private void AddEvent(ReplayEventType type, byte playerId, byte targetId, Vector2 position, string description)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (currentReplay != null)
		{
			float time = Time.time - recordingStartTime;
			currentReplay.Events.Add(new ReplayEvent
			{
				Time = time,
				Type = type,
				PlayerId = playerId,
				TargetId = targetId,
				Position = position,
				Description = description
			});
		}
	}

	private string GetMapName(int mapId)
	{
		return mapId switch
		{
			0 => "The Skeld", 
			1 => "MIRA HQ", 
			2 => "Polus", 
			3 => "Dleks", 
			4 => "Airship", 
			5 => "The Fungle", 
			_ => "Unknown", 
		};
	}

	private Color32 GetRealColor(PlayerControl p)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		int colorId = p.Data.DefaultOutfit.ColorId;
		if (colorId >= 0 && colorId < ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length)
		{
			return ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors)[colorId];
		}
		SpriteRenderer component = ((Component)p).GetComponent<SpriteRenderer>();
		if ((Object)(object)component != (Object)null)
		{
			return Color32.op_Implicit(component.color);
		}
		return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
	}

	private void CacheSystems()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (_systemsCached)
		{
			return;
		}
		_systemsCached = true;
		try
		{
			ShipStatus instance = ShipStatus.Instance;
			Dictionary<SystemTypes, ISystemType> val = ((instance != null) ? instance.Systems : null);
			if (val == null)
			{
				return;
			}
			try
			{
				if (val.ContainsKey((SystemTypes)7))
				{
					_cachedSwitchSys = ((Il2CppObjectBase)val[(SystemTypes)7]).TryCast<SwitchSystem>();
				}
			}
			catch
			{
			}
			try
			{
				SystemTypes val2 = (SystemTypes)(((int)ShipStatus.Instance.Type == 2) ? 21 : 3);
				if (val.ContainsKey(val2))
				{
					_cachedReactorSys = ((Il2CppObjectBase)val[val2]).TryCast<ReactorSystemType>();
				}
			}
			catch
			{
			}
			try
			{
				if (val.ContainsKey((SystemTypes)8))
				{
					_cachedO2Sys = ((Il2CppObjectBase)val[(SystemTypes)8]).TryCast<LifeSuppSystemType>();
				}
			}
			catch
			{
			}
			try
			{
				if (val.ContainsKey((SystemTypes)14))
				{
					_cachedCommsSys = ((Il2CppObjectBase)val[(SystemTypes)14]).TryCast<HudOverrideSystemType>();
				}
			}
			catch
			{
			}
		}
		catch
		{
		}
	}

	private void DetectSabotageEvents()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			return;
		}
		CacheSystems();
		try
		{
			if (_cachedSwitchSys != null)
			{
				bool isActive = _cachedSwitchSys.IsActive;
				if (isActive && !wasLightsSabotaged)
				{
					AddEvent(ReplayEventType.LightsSabotage, byte.MaxValue, 0, Vector2.zero, "Lights sabotaged");
				}
				else if (!isActive && wasLightsSabotaged)
				{
					AddEvent(ReplayEventType.SabotageFixed, byte.MaxValue, 0, Vector2.zero, "Lights fixed");
				}
				wasLightsSabotaged = isActive;
			}
			if (_cachedReactorSys != null)
			{
				bool isActive2 = _cachedReactorSys.IsActive;
				if (isActive2 && !wasReactorActive)
				{
					AddEvent(ReplayEventType.ReactorSabotage, byte.MaxValue, 0, Vector2.zero, "Reactor sabotaged");
				}
				else if (!isActive2 && wasReactorActive)
				{
					AddEvent(ReplayEventType.SabotageFixed, byte.MaxValue, 0, Vector2.zero, "Reactor fixed");
				}
				wasReactorActive = isActive2;
			}
			if (_cachedO2Sys != null)
			{
				bool isActive3 = _cachedO2Sys.IsActive;
				if (isActive3 && !wasO2Active)
				{
					AddEvent(ReplayEventType.O2Sabotage, byte.MaxValue, 0, Vector2.zero, "O2 depleted");
				}
				else if (!isActive3 && wasO2Active)
				{
					AddEvent(ReplayEventType.SabotageFixed, byte.MaxValue, 0, Vector2.zero, "O2 fixed");
				}
				wasO2Active = isActive3;
			}
			if (_cachedCommsSys != null)
			{
				bool isActive4 = _cachedCommsSys.IsActive;
				if (isActive4 && !wasCommsActive)
				{
					AddEvent(ReplayEventType.CommsSabotage, byte.MaxValue, 0, Vector2.zero, "Comms sabotaged");
				}
				else if (!isActive4 && wasCommsActive)
				{
					AddEvent(ReplayEventType.SabotageFixed, byte.MaxValue, 0, Vector2.zero, "Comms fixed");
				}
				wasCommsActive = isActive4;
			}
		}
		catch
		{
		}
	}

	public void StopRecording()
	{
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Expected O, but got Unknown
		if (!isRecording)
		{
			return;
		}
		isRecording = false;
		ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
		bool flag = default(bool);
		BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(38, 0, ref flag);
		if (flag)
		{
			((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ReplayRecorder] Stopping recording...");
		}
		log.LogInfo(val);
		if (currentReplay != null && currentReplay.Frames.Count > 0)
		{
			currentReplay.TotalDuration = currentReplay.Frames[currentReplay.Frames.Count - 1].Time;
			currentReplay.Events.Add(new ReplayEvent
			{
				Time = currentReplay.TotalDuration,
				Type = ReplayEventType.GameEnd,
				PlayerId = 0,
				TargetId = 0,
				Position = Vector2.zero,
				Description = "Game Ended"
			});
			string path = $"Replay_{DateTime.Now:yyyyMMdd_HHmmss}_{currentReplay.Players.Count}p_{currentReplay.Frames.Count}f.mmc";
			string text = Path.Combine(replayDirectory, path);
			try
			{
				currentReplay.Save(text);
				ManualLogSource log2 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
				val = new BepInExInfoLogInterpolatedStringHandler(57, 4, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ReplayRecorder] ✅ Saved: ");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(text);
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" (");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>(currentReplay.Frames.Count);
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" frames, ");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>(currentReplay.Events.Count);
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" events, ");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<float>(currentReplay.TotalDuration, "F1");
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral("s duration)");
				}
				log2.LogInfo(val);
			}
			catch (Exception ex)
			{
				ManualLogSource log3 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
				BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(33, 1, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[ReplayRecorder] Failed to save: ");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<Exception>(ex);
				}
				log3.LogError(val2);
			}
		}
		else
		{
			((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogWarning((object)"[ReplayRecorder] No frames recorded, skipping save");
		}
		currentReplay = null;
		deadPlayers.Clear();
	}

	public void ToggleRecording()
	{
		if (isRecording)
		{
			StopRecording();
		}
		else
		{
			StartRecording();
		}
	}

	[HideFromIl2Cpp]
	public void LogEvent(ReplayEventType type, byte playerId, byte targetId, Vector2 position, string description)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!isRecording)
		{
			return;
		}
		switch (type)
		{
		case ReplayEventType.Kill:
			if (!deadPlayers.Contains(targetId))
			{
				deadPlayers.Add(targetId);
			}
			break;
		case ReplayEventType.Meeting:
			wasInMeeting = true;
			break;
		}
		AddEvent(type, playerId, targetId, position, description);
	}

	[HideFromIl2Cpp]
	public void SetAnimState(byte playerId, AnimState state)
	{
		playerAnimStates[playerId] = (byte)state;
	}

	private void EnsureReplayGuiStyles()
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Expected O, but got Unknown
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Expected O, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Expected O, but got Unknown
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		if (_replayCenterLabelStyle != null && (Object)(object)_replayBoxTex != (Object)null && (Object)(object)_replayBtnTex != (Object)null && (Object)(object)_replayBtnHoverTex != (Object)null && (Object)(object)_replayBtnActiveTex != (Object)null)
		{
			return;
		}
		if ((Object)(object)_replayBoxTex != (Object)null)
		{
			try
			{
				Object.Destroy((Object)(object)_replayBoxTex);
			}
			catch
			{
			}
		}
		if ((Object)(object)_replayBtnTex != (Object)null)
		{
			try
			{
				Object.Destroy((Object)(object)_replayBtnTex);
			}
			catch
			{
			}
		}
		if ((Object)(object)_replayBtnHoverTex != (Object)null)
		{
			try
			{
				Object.Destroy((Object)(object)_replayBtnHoverTex);
			}
			catch
			{
			}
		}
		if ((Object)(object)_replayBtnActiveTex != (Object)null)
		{
			try
			{
				Object.Destroy((Object)(object)_replayBtnActiveTex);
			}
			catch
			{
			}
		}
		_replayBoxTex = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.15f, 0.95f));
		_replayBtnTex = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.3f, 1f));
		_replayBtnHoverTex = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.5f, 1f));
		_replayBtnActiveTex = MakeTex(2, 2, new Color(0.4f, 0.4f, 0.7f, 1f));
		((Object)_replayBoxTex).hideFlags = (HideFlags)61;
		((Object)_replayBtnTex).hideFlags = (HideFlags)61;
		((Object)_replayBtnHoverTex).hideFlags = (HideFlags)61;
		((Object)_replayBtnActiveTex).hideFlags = (HideFlags)61;
		_replayCenterLabelStyle = new GUIStyle(GUI.skin.label)
		{
			alignment = (TextAnchor)4,
			richText = true
		};
		_replayCenterLabelStyle.normal.textColor = Color.white;
		_replayBoxStyle = new GUIStyle(GUI.skin.box);
		_replayBoxStyle.normal.background = _replayBoxTex;
		_replayBtnStyle = new GUIStyle(GUI.skin.button);
		_replayBtnStyle.normal.background = _replayBtnTex;
		_replayBtnStyle.normal.textColor = Color.white;
		_replayBtnStyle.hover.background = _replayBtnHoverTex;
		_replayBtnStyle.hover.textColor = Color.white;
		_replayBtnStyle.active.background = _replayBtnActiveTex;
		_replayBtnStyle.active.textColor = Color.white;
	}

	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)290))
		{
			showReplayMenu = !showReplayMenu;
			if (showReplayMenu)
			{
				RefreshReplayList();
			}
		}
	}

	private void RefreshReplayList()
	{
		try
		{
			if (Directory.Exists(replayDirectory))
			{
				cachedReplayFiles = Directory.GetFiles(replayDirectory, "*.mmc");
				Array.Sort(cachedReplayFiles);
				Array.Reverse(cachedReplayFiles);
			}
		}
		catch
		{
			cachedReplayFiles = new string[0];
		}
		lastFileRefresh = Time.time;
	}

	private void OnGUI()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		if (!showReplayMenu || !ServerData.IsLoaded)
		{
			return;
		}
		EnsureReplayGuiStyles();
		float scale = GuiStyles.Spacing.Scale;
		Matrix4x4 matrix = GUI.matrix;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
		float num = 320f;
		float num2 = 400f;
		Rect val = new Rect((float)Screen.width / scale - num - 20f, 100f, num, num2);
		GUI.Box(val, "", _replayBoxStyle);
		GUILayout.BeginArea(val);
		try
		{
			GUILayout.Space(8f);
			GUILayout.Label("<size=18><b>\ud83c\udfac REPLAY SYSTEM</b></size>", _replayCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			GUILayout.Space(4f);
			GUILayout.Label("<color=#00FFFF>━━━ RECORDER ━━━</color>", _replayCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			if (isRecording)
			{
				GUILayout.Label($"<color=#FF4444>● RECORDING</color> - {FrameCount} frames, {EventCount} events", _replayCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				if (GUILayout.Button("⏹\ufe0f STOP RECORDING", _replayBtnStyle, Array.Empty<GUILayoutOption>()))
				{
					StopRecording();
				}
			}
			else
			{
				GUILayout.Label("<color=#88FF88>● READY</color> - Auto-Record: " + (AutoRecordEnabled ? "ON" : "OFF"), _replayCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				GUILayout.BeginHorizontal((Il2CppReferenceArray<GUILayoutOption>)null);
				if (GUILayout.Button("▶\ufe0f START", _replayBtnStyle, Array.Empty<GUILayoutOption>()))
				{
					StartRecording();
				}
				if (GUILayout.Button(AutoRecordEnabled ? "\ud83d\udd34 AUTO:ON" : "⚪ AUTO:OFF", _replayBtnStyle, Array.Empty<GUILayoutOption>()))
				{
					AutoRecordEnabled = !AutoRecordEnabled;
				}
				GUILayout.EndHorizontal();
			}
			if (GUILayout.Button("\ud83d\udcc2 OPEN REPLAYS FOLDER", _replayBtnStyle, Array.Empty<GUILayoutOption>()))
			{
				Application.OpenURL(replayDirectory);
			}
			GUILayout.Space(8f);
			GUILayout.Label("<color=#FFFF00>━━━ SAVED REPLAYS ━━━</color>", _replayCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			if (Time.time - lastFileRefresh > 5f)
			{
				RefreshReplayList();
			}
			replayListScroll = GUILayout.BeginScrollView(replayListScroll, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(180f) });
			if (cachedReplayFiles.Length == 0)
			{
				GUILayout.Label("<i>No replays found</i>", _replayCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
			}
			else
			{
				string[] array = cachedReplayFiles;
				foreach (string path in array)
				{
					string text = Path.GetFileNameWithoutExtension(path);
					if (text.Length > 30)
					{
						text = text.Substring(0, 27) + "...";
					}
					if (GUILayout.Button("▶ " + text, _replayBtnStyle, Array.Empty<GUILayoutOption>()))
					{
						LoadAndPlayReplay(path);
					}
				}
			}
			GUILayout.EndScrollView();
			if ((Object)(object)ReplayViewer.Instance != (Object)null && ReplayViewer.Instance.IsActive)
			{
				GUILayout.Space(4f);
				GUILayout.Label("<color=#FF00FF>━━━ NOW PLAYING ━━━</color>", _replayCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
				if (GUILayout.Button("⏹\ufe0f STOP PLAYBACK", _replayBtnStyle, Array.Empty<GUILayoutOption>()))
				{
					ReplayViewer.Instance.Stop();
				}
			}
			GUILayout.Space(8f);
			GUILayout.Label("<size=10><color=#949EAD>Press F9 to close</color></size>", _replayCenterLabelStyle, (Il2CppReferenceArray<GUILayoutOption>)null);
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[ReplayRecorder] OnGUI error: {value}"));
			try
			{
				GUILayout.EndScrollView();
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
		finally
		{
			GUILayout.EndArea();
		}
		GUI.matrix = matrix;
	}

	private void LoadAndPlayReplay(string path)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		bool flag = default(bool);
		try
		{
			ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(44, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ReplayRecorder] Requesting to load replay: ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(path);
			}
			log.LogInfo(val);
			if ((Object)(object)ReplayViewer.Instance == (Object)null)
			{
				((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogInfo((object)"[ReplayRecorder] ReplayViewer instance is null. Creating new GameObject.");
				GameObject val2 = new GameObject("ReplayViewer");
				Object.DontDestroyOnLoad((Object)val2);
				Component val3 = val2.AddComponent(Il2CppType.Of<ReplayViewer>());
				if ((Object)(object)val3 == (Object)null)
				{
					((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogError((object)"[ReplayRecorder] FATAL: AddComponent<ReplayViewer> returned null!");
					return;
				}
				ManualLogSource log2 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
				val = new BepInExInfoLogInterpolatedStringHandler(34, 1, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ReplayRecorder] Component added: ");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(((Object)val3).ToString());
				}
				log2.LogInfo(val);
			}
			if ((Object)(object)ReplayViewer.Instance == (Object)null)
			{
				ReplayViewer.Instance = Object.FindObjectOfType<ReplayViewer>();
			}
			if ((Object)(object)ReplayViewer.Instance != (Object)null)
			{
				((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogInfo((object)"[ReplayRecorder] Starting viewer...");
				ReplayViewer.Instance.StartViewer(path);
				showReplayMenu = false;
			}
			else
			{
				((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogError((object)"[ReplayRecorder] FATAL: ReplayViewer.Instance is STILL NULL after creation attempt.");
			}
		}
		catch (Exception ex)
		{
			ManualLogSource log3 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			BepInExErrorLogInterpolatedStringHandler val4 = new BepInExErrorLogInterpolatedStringHandler(49, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val4).AppendLiteral("[ReplayRecorder] Exception in LoadAndPlayReplay: ");
				((BepInExLogInterpolatedStringHandler)val4).AppendFormatted<Exception>(ex);
			}
			log3.LogError(val4);
		}
	}

	private Texture2D MakeTex(int width, int height, Color col)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return NotifyUtils.MakeTex(width, height, col);
	}
}
