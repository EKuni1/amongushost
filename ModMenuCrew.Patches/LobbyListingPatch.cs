using System;
using System.Collections.Generic;
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace ModMenuCrew.Patches;

[HarmonyPatch]
public static class LobbyListingPatch
{
	public class CapturedLobby
	{
		public int GameId { get; set; }

		public string GameCode { get; set; }

		public string HostName { get; set; }

		public string TrueHostName { get; set; }

		public string IPAddress { get; set; }

		public ushort Port { get; set; }

		public byte PlayerCount { get; set; }

		public byte MaxPlayers { get; set; }

		public byte MapId { get; set; }

		public string MapName { get; set; }

		public Platforms Platform { get; set; }

		public string PlatformName { get; set; }

		public uint Language { get; set; }

		public int Age { get; set; }

		public QuickChatModes QuickChat { get; set; }

		public DateTime CapturedAt { get; set; }

		public int NumImpostors { get; set; }

		public float KillCooldown { get; set; }

		public float PlayerSpeed { get; set; }

		public GameModes GameMode { get; set; }

		public string PlayerCountText { get; private set; }

		public string ListRowText { get; private set; }

		public string DetailsLine { get; private set; }

		public bool IsFull { get; private set; }

		internal void RefreshDisplayStrings()
		{
			PlayerCountText = PlayerCount + "/" + MaxPlayers;
			IsFull = PlayerCount >= MaxPlayers;
			string text = HostName ?? "?";
			if (text.Length > 10)
			{
				text = text.Substring(0, 8) + "..";
			}
			string text2 = ((Age > 0) ? (" " + Age + "s") : "");
			ListRowText = (GameCode ?? "?") + "  " + text + text2;
			DetailsLine = (MapName ?? "?") + " | " + PlayerCountText;
		}

		public override string ToString()
		{
			string text = ((NumImpostors > 0) ? (" | " + NumImpostors + "imp") : "");
			return "[" + GameCode + "] " + HostName + " (" + PlayerCount + "/" + MaxPlayers + ") " + MapName + " | " + PlatformName + text;
		}
	}

	public enum LobbySortMode
	{
		Default,
		PlayerCountDesc,
		PlayerCountAsc,
		AvailableSlots,
		Newest,
		MapName
	}

	public static class Maps
	{
		public const byte Skeld = 0;

		public const byte MiraHQ = 1;

		public const byte Polus = 2;

		public const byte Dleks = 3;

		public const byte Airship = 4;

		public const byte Fungle = 5;

		public static readonly (byte Id, string Name)[] AllMaps = new(byte, string)[5]
		{
			(0, "Skeld"),
			(1, "Mira"),
			(2, "Polus"),
			(4, "Airship"),
			(5, "Fungle")
		};
	}

	private static readonly Dictionary<int, int> _lobbyIndex = new Dictionary<int, int>();

	private static readonly object _lock = new object();

	public const int MAX_DISPLAY_LOBBIES = 30;

	private static readonly List<CapturedLobby> _filteredResultBuffer = new List<CapturedLobby>(64);

	private static readonly Comparison<CapturedLobby> _cmpPlayerDesc = (CapturedLobby a, CapturedLobby b) => b.PlayerCount.CompareTo(a.PlayerCount);

	private static readonly Comparison<CapturedLobby> _cmpPlayerAsc = (CapturedLobby a, CapturedLobby b) => a.PlayerCount.CompareTo(b.PlayerCount);

	private static readonly Comparison<CapturedLobby> _cmpAvailableSlots = (CapturedLobby a, CapturedLobby b) => (b.MaxPlayers - b.PlayerCount).CompareTo(a.MaxPlayers - a.PlayerCount);

	private static readonly Comparison<CapturedLobby> _cmpNewest = (CapturedLobby a, CapturedLobby b) => a.Age.CompareTo(b.Age);

	private static readonly Comparison<CapturedLobby> _cmpMapName = (CapturedLobby a, CapturedLobby b) => string.Compare(a.MapName, b.MapName, StringComparison.Ordinal);

	private static bool _isJoining = false;

	private static float _lastJoinTime = 0f;

	private const float JOIN_COOLDOWN = 3f;

	public static List<CapturedLobby> CapturedLobbies { get; private set; } = new List<CapturedLobby>();


	public static DateTime LastUpdateTime { get; private set; } = DateTime.MinValue;


	public static int TotalGamesInMatchmaker { get; private set; } = 0;


	public static int MatchingGamesCount { get; private set; } = 0;


	public static bool CaptureEnabled { get; set; } = true;


	public static LobbySortMode CurrentSortMode { get; set; } = LobbySortMode.Default;


	public static byte? FilterMapId { get; set; } = null;


	public static Platforms? FilterPlatform { get; set; } = null;


	public static bool FilterFreeChatOnly { get; set; } = false;


	public static bool FilterHasSpace { get; set; } = false;


	public static int? FilterNumImpostors { get; set; } = null;


	public static event Action<List<CapturedLobby>> OnLobbiesCaptured;

	[HarmonyPatch(typeof(FindAGameManager), "HandleList")]
	[HarmonyPrefix]
	public static void HandleListPrefix(object totalGames, object response)
	{
		if (!CaptureEnabled)
		{
			return;
		}
		try
		{
			if (((response != null) ? response.Metadata : null) != null)
			{
				TotalGamesInMatchmaker = response.Metadata.AllGamesCount;
				MatchingGamesCount = response.Metadata.MatchingGamesCount;
			}
			LastUpdateTime = DateTime.Now;
			int num = 0;
			if (((response != null) ? response.Games : null) != null)
			{
				lock (_lock)
				{
					Enumerator<GameListing> enumerator = response.Games.GetEnumerator();
					while (enumerator.MoveNext())
					{
						GameListing current = enumerator.Current;
						try
						{
							if (_lobbyIndex.TryGetValue(current.GameId, out var value) && value < CapturedLobbies.Count)
							{
								CapturedLobby capturedLobby = ConvertToCapturedLobby(current);
								if (capturedLobby != null)
								{
									CapturedLobbies[value] = capturedLobby;
								}
								continue;
							}
							CapturedLobby capturedLobby2 = ConvertToCapturedLobby(current);
							if (capturedLobby2 != null)
							{
								_lobbyIndex[current.GameId] = CapturedLobbies.Count;
								CapturedLobbies.Add(capturedLobby2);
								num++;
							}
						}
						catch (Exception ex)
						{
							Debug.LogWarning(Object.op_Implicit("[LobbyListing] Error capturing lobby: " + ex.Message));
						}
					}
					if (CapturedLobbies.RemoveAll((CapturedLobby l) => (DateTime.Now - l.CapturedAt).TotalMinutes > 5.0) > 0)
					{
						RebuildLobbyIndex();
					}
				}
			}
			Debug.Log(Object.op_Implicit($"[LobbyListing] Added {num} new lobbies. Total captured: {CapturedLobbies.Count}. In matchmaker: {TotalGamesInMatchmaker}, Matching: {MatchingGamesCount}"));
			Action<List<CapturedLobby>> onLobbiesCaptured = LobbyListingPatch.OnLobbiesCaptured;
			if (onLobbiesCaptured != null)
			{
				List<CapturedLobby> obj;
				lock (_lock)
				{
					obj = new List<CapturedLobby>(CapturedLobbies);
				}
				onLobbiesCaptured(obj);
			}
		}
		catch (Exception value2)
		{
			Debug.LogError(Object.op_Implicit($"[LobbyListing] HandleListPrefix error: {value2}"));
		}
	}

	[HarmonyPatch(typeof(MatchMakerGameButton), "SetGame")]
	[HarmonyPostfix]
	public static void SetGamePostfix(MatchMakerGameButton __instance, GameListing gameListing)
	{
		if (!CaptureEnabled)
		{
			return;
		}
		try
		{
			lock (_lock)
			{
				if (!_lobbyIndex.ContainsKey(gameListing.GameId))
				{
					CapturedLobby capturedLobby = ConvertToCapturedLobby(gameListing);
					if (capturedLobby != null)
					{
						_lobbyIndex[gameListing.GameId] = CapturedLobbies.Count;
						CapturedLobbies.Add(capturedLobby);
						Debug.Log(Object.op_Implicit($"[LobbyListing] Additional lobby captured: {capturedLobby}"));
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(Object.op_Implicit("[LobbyListing] SetGamePostfix error: " + ex.Message));
		}
	}

	[HarmonyPatch(typeof(GameContainer), "SetGameListing")]
	[HarmonyPostfix]
	public static void SetGameListingPostfix(GameContainer __instance, GameListing gameL)
	{
		if (!CaptureEnabled)
		{
			return;
		}
		try
		{
			lock (_lock)
			{
				if (!_lobbyIndex.ContainsKey(gameL.GameId))
				{
					CapturedLobby capturedLobby = ConvertToCapturedLobby(gameL);
					if (capturedLobby != null)
					{
						_lobbyIndex[gameL.GameId] = CapturedLobbies.Count;
						CapturedLobbies.Add(capturedLobby);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(Object.op_Implicit("[LobbyListing] SetGameListingPostfix error: " + ex.Message));
		}
	}

	private static CapturedLobby ConvertToCapturedLobby(GameListing listing)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		if (listing.GameId == 0)
		{
			return null;
		}
		CapturedLobby capturedLobby = new CapturedLobby
		{
			GameId = listing.GameId,
			GameCode = GameCode.IntToGameName(listing.GameId),
			HostName = (listing.HostName ?? "Unknown"),
			TrueHostName = (listing.TrueHostName ?? listing.HostName ?? "Unknown"),
			IPAddress = (listing.IPString ?? "N/A"),
			Port = listing.Port,
			PlayerCount = listing.PlayerCount,
			MaxPlayers = (byte)listing.MaxPlayers,
			MapId = listing.MapId,
			MapName = GetMapName(listing.MapId),
			Platform = listing.Platform,
			PlatformName = FormatPlatform(listing.Platform),
			Language = listing.Language,
			Age = listing.Age,
			QuickChat = listing.QuickChat,
			CapturedAt = DateTime.Now
		};
		if (listing.Options != null)
		{
			try
			{
				capturedLobby.NumImpostors = listing.Options.NumImpostors;
				capturedLobby.GameMode = listing.Options.GameMode;
				float killCooldown = default(float);
				if (listing.Options.TryGetFloat((FloatOptionNames)1, ref killCooldown))
				{
					capturedLobby.KillCooldown = killCooldown;
				}
				float playerSpeed = default(float);
				if (listing.Options.TryGetFloat((FloatOptionNames)2, ref playerSpeed))
				{
					capturedLobby.PlayerSpeed = playerSpeed;
				}
			}
			catch
			{
			}
		}
		capturedLobby.RefreshDisplayStrings();
		return capturedLobby;
	}

	private static string GetMapName(byte mapId)
	{
		return mapId switch
		{
			0 => "The Skeld", 
			1 => "Mira HQ", 
			2 => "Polus", 
			3 => "Dleks", 
			4 => "Airship", 
			5 => "The Fungle", 
			_ => $"Map {mapId}", 
		};
	}

	private static string FormatPlatform(Platforms platform)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected I4, but got Unknown
		return (platform - 1) switch
		{
			1 => "Steam", 
			0 => "Epic", 
			3 => "MS Store", 
			5 => "iOS", 
			6 => "Android", 
			7 => "Switch", 
			8 => "Xbox", 
			9 => "PlayStation", 
			_ => "PC", 
		};
	}

	public static List<CapturedLobby> GetFilteredAndSortedLobbies()
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Invalid comparison between Unknown and I4
		lock (_lock)
		{
			DateTime now = DateTime.Now;
			if (CapturedLobbies.Count > 0)
			{
				bool flag = false;
				for (int num = CapturedLobbies.Count - 1; num >= 0; num--)
				{
					if ((now - CapturedLobbies[num].CapturedAt).TotalMinutes > 5.0)
					{
						CapturedLobbies.RemoveAt(num);
						flag = true;
					}
				}
				if (flag)
				{
					RebuildLobbyIndex();
				}
			}
			bool hasValue = FilterMapId.HasValue;
			byte b = (byte)(hasValue ? FilterMapId.Value : 0);
			bool hasValue2 = FilterPlatform.HasValue;
			Platforms val = (Platforms)(hasValue2 ? ((int)FilterPlatform.Value) : 0);
			bool filterFreeChatOnly = FilterFreeChatOnly;
			bool filterHasSpace = FilterHasSpace;
			bool hasValue3 = FilterNumImpostors.HasValue;
			int num2 = (hasValue3 ? FilterNumImpostors.Value : 0);
			_filteredResultBuffer.Clear();
			int count = CapturedLobbies.Count;
			for (int i = 0; i < count; i++)
			{
				CapturedLobby capturedLobby = CapturedLobbies[i];
				if ((!hasValue || capturedLobby.MapId == b) && (!hasValue2 || capturedLobby.Platform == val) && (!filterFreeChatOnly || (int)capturedLobby.QuickChat == 1) && (!filterHasSpace || capturedLobby.PlayerCount < capturedLobby.MaxPlayers) && (!hasValue3 || capturedLobby.NumImpostors == num2))
				{
					_filteredResultBuffer.Add(capturedLobby);
				}
			}
			switch (CurrentSortMode)
			{
			case LobbySortMode.PlayerCountDesc:
				_filteredResultBuffer.Sort(_cmpPlayerDesc);
				break;
			case LobbySortMode.PlayerCountAsc:
				_filteredResultBuffer.Sort(_cmpPlayerAsc);
				break;
			case LobbySortMode.AvailableSlots:
				_filteredResultBuffer.Sort(_cmpAvailableSlots);
				break;
			case LobbySortMode.Newest:
				_filteredResultBuffer.Sort(_cmpNewest);
				break;
			case LobbySortMode.MapName:
				_filteredResultBuffer.Sort(_cmpMapName);
				break;
			}
			if (_filteredResultBuffer.Count > 30)
			{
				_filteredResultBuffer.RemoveRange(30, _filteredResultBuffer.Count - 30);
			}
			return _filteredResultBuffer;
		}
	}

	public static void ClearFilters()
	{
		FilterMapId = null;
		FilterPlatform = null;
		FilterFreeChatOnly = false;
		FilterHasSpace = false;
		FilterNumImpostors = null;
		CurrentSortMode = LobbySortMode.Default;
	}

	public static void ClearCapturedLobbies()
	{
		lock (_lock)
		{
			CapturedLobbies.Clear();
			_lobbyIndex.Clear();
			LastUpdateTime = DateTime.MinValue;
			TotalGamesInMatchmaker = 0;
			MatchingGamesCount = 0;
		}
	}

	private static void RebuildLobbyIndex()
	{
		_lobbyIndex.Clear();
		for (int i = 0; i < CapturedLobbies.Count; i++)
		{
			_lobbyIndex[CapturedLobbies[i].GameId] = i;
		}
	}

	public static void RefreshLobbyList()
	{
		try
		{
			FindAGameManager val = Object.FindObjectOfType<FindAGameManager>();
			if ((Object)(object)val != (Object)null)
			{
				val.RefreshList();
				Debug.Log(Object.op_Implicit("[LobbyListing] Refresh requested."));
			}
			else
			{
				Debug.LogWarning(Object.op_Implicit("[LobbyListing] FindAGameManager not found. Are you in the Find Game menu?"));
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[LobbyListing] RefreshLobbyList error: {value}"));
		}
	}

	public static void JoinByCode(string gameCode)
	{
		if (string.IsNullOrEmpty(gameCode) || (Object)(object)AmongUsClient.Instance == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("[LobbyListing] Cannot join: Invalid code or client."));
			return;
		}
		if (((InnerNetClient)AmongUsClient.Instance).AmConnected || _isJoining || Time.time - _lastJoinTime < 3f)
		{
			Debug.LogWarning(Object.op_Implicit("[LobbyListing] Cannot join: Already connected or join in progress."));
			return;
		}
		try
		{
			gameCode = gameCode.Trim().ToUpper();
			if (gameCode.Length != 6 && gameCode.Length != 4)
			{
				Debug.LogWarning(Object.op_Implicit($"[LobbyListing] Invalid game code length: {gameCode.Length}. Expected 4 or 6."));
				return;
			}
			int num = GameCode.GameNameToInt(gameCode);
			if (num == -1)
			{
				Debug.LogWarning(Object.op_Implicit("[LobbyListing] Invalid game code: " + gameCode));
				return;
			}
			Debug.Log(Object.op_Implicit($"[LobbyListing] Joining by code: {gameCode} (ID: {num})"));
			_isJoining = true;
			_lastJoinTime = Time.time;
			((MonoBehaviour)AmongUsClient.Instance).StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(num, true));
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[LobbyListing] JoinByCode error: {value}"));
		}
	}

	public static void JoinCapturedLobby(CapturedLobby lobby)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		if (lobby == null || (Object)(object)AmongUsClient.Instance == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("[LobbyListing] Cannot join: Invalid lobby or client."));
			return;
		}
		if (((InnerNetClient)AmongUsClient.Instance).AmConnected || _isJoining || Time.time - _lastJoinTime < 3f)
		{
			Debug.LogWarning(Object.op_Implicit("[LobbyListing] Cannot join: Already connected or join in progress."));
			return;
		}
		try
		{
			if ((int)lobby.QuickChat == 1)
			{
				if (DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed())
				{
					DataManager.Settings.Multiplayer.ChatMode = (QuickChatModes)1;
				}
			}
			else
			{
				DataManager.Settings.Multiplayer.ChatMode = (QuickChatModes)2;
			}
			if (!string.IsNullOrEmpty(lobby.IPAddress) && lobby.IPAddress != "N/A" && lobby.Port > 0)
			{
				Debug.Log(Object.op_Implicit($"[LobbyListing] Direct join lobby: {lobby.GameCode} @ {lobby.IPAddress}:{lobby.Port}"));
				_isJoining = true;
				_lastJoinTime = Time.time;
				((MonoBehaviour)AmongUsClient.Instance).StartCoroutine(AmongUsClient.Instance.CoJoinOnlinePublicGame(lobby.GameId, lobby.IPAddress, lobby.Port, (MainMenuTarget)3));
			}
			else
			{
				Debug.Log(Object.op_Implicit("[LobbyListing] Code-based join lobby: " + lobby.GameCode + " (no IP available)"));
				_isJoining = true;
				_lastJoinTime = Time.time;
				((MonoBehaviour)AmongUsClient.Instance).StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(lobby.GameId, false));
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[LobbyListing] JoinCapturedLobby error: {value}"));
		}
	}

	public static void CopyCodeToClipboard(string gameCode)
	{
		try
		{
			GUIUtility.systemCopyBuffer = gameCode;
			Debug.Log(Object.op_Implicit("[LobbyListing] Copied to clipboard: " + gameCode));
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[LobbyListing] CopyCodeToClipboard error: {value}"));
		}
	}

	[HarmonyPatch(typeof(AmongUsClient), "OnGameJoined")]
	[HarmonyPostfix]
	public static void OnGameJoinedResetLock()
	{
		_isJoining = false;
	}

	[HarmonyPatch(typeof(AmongUsClient), "OnDisconnected")]
	[HarmonyPostfix]
	public static void OnDisconnectedResetLock()
	{
		_isJoining = false;
	}
}
