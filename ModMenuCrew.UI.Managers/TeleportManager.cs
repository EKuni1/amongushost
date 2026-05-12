using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using UnityEngine;

namespace ModMenuCrew.UI.Managers;

public class TeleportManager
{
	private const int VERIFICATION_TOKEN_LENGTH = 32;

	private readonly Dictionary<byte, DateTime> lastPlayerTeleports;

	private readonly RandomNumberGenerator _secureRandom;

	private float lastTeleportTime;

	private byte lastTeleportId;

	private Dictionary<SystemTypes, Vector2> cachedLocations = new Dictionary<SystemTypes, Vector2>();

	private int lastCachedMapType = -1;

	private static byte TELEPORT_RPC
	{
		get
		{
			if (ServerData.Config.RpcTeleport <= 0)
			{
				return 230;
			}
			return ServerData.Config.RpcTeleport;
		}
	}

	private static byte TELEPORT_SYNC
	{
		get
		{
			if (ServerData.Config.RpcTeleportSync <= 0)
			{
				return 231;
			}
			return ServerData.Config.RpcTeleportSync;
		}
	}

	private static float MAX_TELEPORT_DISTANCE
	{
		get
		{
			if (!(ServerData.Config.TeleportMaxDistance > 0f))
			{
				return 50f;
			}
			return ServerData.Config.TeleportMaxDistance;
		}
	}

	private static float MIN_TELEPORT_INTERVAL
	{
		get
		{
			if (!(ServerData.Config.TeleportMinInterval > 0f))
			{
				return 0.5f;
			}
			return ServerData.Config.TeleportMinInterval;
		}
	}

	private static int MAX_ATTEMPTS
	{
		get
		{
			if (ServerData.Config.TeleportMaxAttempts <= 0)
			{
				return 3;
			}
			return ServerData.Config.TeleportMaxAttempts;
		}
	}

	public IReadOnlyDictionary<SystemTypes, Vector2> Locations => GetLocationsForCurrentMap();

	public TeleportManager()
	{
		_secureRandom = RandomNumberGenerator.Create();
		lastPlayerTeleports = new Dictionary<byte, DateTime>();
	}

	public int GetCurrentMapType()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected I4, but got Unknown
		try
		{
			if ((Object)(object)ShipStatus.Instance == (Object)null)
			{
				return -1;
			}
			return (int)ShipStatus.Instance.Type;
		}
		catch
		{
			return -1;
		}
	}

	public string GetCurrentMapName()
	{
		return GetCurrentMapType() switch
		{
			0 => "The Skeld", 
			1 => "MIRA HQ", 
			2 => "Polus", 
			3 => "Dleks", 
			4 => "The Airship", 
			5 => "The Fungle", 
			_ => "Unknown", 
		};
	}

	public string GetCurrentMapIcon()
	{
		return GetCurrentMapType() switch
		{
			0 => "\ud83d\ude80", 
			1 => "\ud83c\udfe2", 
			2 => "❄\ufe0f", 
			3 => "\ud83d\udd04", 
			4 => "✈\ufe0f", 
			5 => "\ud83c\udf3f", 
			_ => "❓", 
		};
	}

	public IReadOnlyDictionary<SystemTypes, Vector2> GetLocationsForCurrentMap()
	{
		try
		{
			if ((Object)(object)ShipStatus.Instance == (Object)null)
			{
				return new Dictionary<SystemTypes, Vector2>();
			}
			int currentMapType = GetCurrentMapType();
			if (currentMapType != lastCachedMapType || cachedLocations.Count == 0)
			{
				cachedLocations.Clear();
				lastCachedMapType = currentMapType;
				BuildLocationsForMap(currentMapType);
			}
			return cachedLocations;
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[TeleportManager] Failed to get locations: {value}"));
			return new Dictionary<SystemTypes, Vector2>();
		}
	}

	private void BuildLocationsForMap(int mapType)
	{
		TryBuildDynamicLocations();
		if (cachedLocations.Count == 0)
		{
			LoadStaticLocations(mapType);
		}
		else
		{
			SupplementWithStaticLocations(mapType);
		}
		Debug.Log(Object.op_Implicit($"[TeleportManager] Built {cachedLocations.Count} locations for {GetCurrentMapName()}"));
	}

	private void TryBuildDynamicLocations()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if ((Object)(object)ShipStatus.Instance == (Object)null)
			{
				return;
			}
			Dictionary<SystemTypes, PlainShipRoom> fastRooms = ShipStatus.Instance.FastRooms;
			if (fastRooms != null)
			{
				Enumerator<SystemTypes, PlainShipRoom> enumerator = fastRooms.GetEnumerator();
				while (enumerator.MoveNext())
				{
					KeyValuePair<SystemTypes, PlainShipRoom> current = enumerator.Current;
					try
					{
						SystemTypes key = current.Key;
						PlainShipRoom value = current.Value;
						if ((Object)(object)value != (Object)null && (Object)(object)value.roomArea != (Object)null)
						{
							Bounds bounds = value.roomArea.bounds;
							Vector2 value2 = Vector2.op_Implicit((bounds).center);
							cachedLocations[key] = value2;
						}
					}
					catch
					{
					}
				}
			}
			Il2CppReferenceArray<Console> allConsoles = ShipStatus.Instance.AllConsoles;
			if (allConsoles == null)
			{
				return;
			}
			foreach (Console item in (Il2CppArrayBase<Console>)(object)allConsoles)
			{
				try
				{
					if ((Object)(object)item != (Object)null && !cachedLocations.ContainsKey(item.Room))
					{
						Vector2 value3 = Vector2.op_Implicit(((Component)item).transform.position);
						cachedLocations[item.Room] = value3;
					}
				}
				catch
				{
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(Object.op_Implicit("[TeleportManager] Dynamic location building failed: " + ex.Message));
		}
	}

	private void LoadStaticLocations(int mapType)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<SystemTypes, Vector2> item in GetStaticLocationsForMap(mapType))
		{
			cachedLocations[item.Key] = item.Value;
		}
	}

	private void SupplementWithStaticLocations(int mapType)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<SystemTypes, Vector2> item in GetStaticLocationsForMap(mapType))
		{
			if (!cachedLocations.ContainsKey(item.Key))
			{
				cachedLocations[item.Key] = item.Value;
			}
		}
	}

	private Dictionary<SystemTypes, Vector2> GetStaticLocationsForMap(int mapType)
	{
		return mapType switch
		{
			0 => GetSkeldLocations(), 
			1 => GetMiraHQLocations(), 
			2 => GetPolusLocations(), 
			3 => GetDleksLocations(), 
			4 => GetAirshipLocations(), 
			5 => GetFungleLocations(), 
			_ => new Dictionary<SystemTypes, Vector2>(), 
		};
	}

	public void RefreshLocations()
	{
		lastCachedMapType = -1;
		cachedLocations.Clear();
	}

	private Dictionary<SystemTypes, Vector2> GetSkeldLocations()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		return new Dictionary<SystemTypes, Vector2>
		{
			{
				(SystemTypes)2,
				new Vector2(1.3f, 1.5f)
			},
			{
				(SystemTypes)12,
				new Vector2(9.5f, 1.5f)
			},
			{
				(SystemTypes)5,
				new Vector2(16.7f, -4.8f)
			},
			{
				(SystemTypes)8,
				new Vector2(6.5f, -3.5f)
			},
			{
				(SystemTypes)6,
				new Vector2(3f, -7.5f)
			},
			{
				(SystemTypes)7,
				new Vector2(-7.5f, -8.5f)
			},
			{
				(SystemTypes)13,
				new Vector2(-18.3f, -13.3f)
			},
			{
				(SystemTypes)4,
				new Vector2(-16.9f, 2.4f)
			},
			{
				(SystemTypes)11,
				new Vector2(-13.5f, -5.5f)
			},
			{
				(SystemTypes)3,
				new Vector2(-20.5f, -5.5f)
			},
			{
				(SystemTypes)10,
				new Vector2(-9f, -4f)
			},
			{
				(SystemTypes)1,
				new Vector2(0f, -14.3f)
			},
			{
				(SystemTypes)14,
				new Vector2(4f, -15.5f)
			},
			{
				(SystemTypes)9,
				new Vector2(9.5f, -12f)
			}
		};
	}

	private Dictionary<SystemTypes, Vector2> GetMiraHQLocations()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		return new Dictionary<SystemTypes, Vector2>
		{
			{
				(SystemTypes)2,
				new Vector2(25.5f, 2f)
			},
			{
				(SystemTypes)22,
				new Vector2(24f, -1.5f)
			},
			{
				(SystemTypes)1,
				new Vector2(19.5f, 4.5f)
			},
			{
				(SystemTypes)14,
				new Vector2(14f, 3.5f)
			},
			{
				(SystemTypes)10,
				new Vector2(15f, 0f)
			},
			{
				(SystemTypes)20,
				new Vector2(9f, 5f)
			},
			{
				(SystemTypes)18,
				new Vector2(6f, 6f)
			},
			{
				(SystemTypes)21,
				new Vector2(9.5f, 12f)
			},
			{
				(SystemTypes)3,
				new Vector2(2.5f, 10.5f)
			},
			{
				(SystemTypes)19,
				new Vector2(-4f, 2f)
			},
			{
				(SystemTypes)6,
				new Vector2(21f, 17.5f)
			},
			{
				(SystemTypes)23,
				new Vector2(15f, 19.5f)
			},
			{
				(SystemTypes)24,
				new Vector2(17.8f, 23.5f)
			}
		};
	}

	private Dictionary<SystemTypes, Vector2> GetPolusLocations()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		return new Dictionary<SystemTypes, Vector2>
		{
			{
				(SystemTypes)23,
				new Vector2(19.5f, -17.5f)
			},
			{
				(SystemTypes)6,
				new Vector2(24f, -21.5f)
			},
			{
				(SystemTypes)14,
				new Vector2(11.5f, -16f)
			},
			{
				(SystemTypes)12,
				new Vector2(12f, -23.5f)
			},
			{
				(SystemTypes)29,
				new Vector2(3f, -24f)
			},
			{
				(SystemTypes)8,
				new Vector2(3.5f, -17.5f)
			},
			{
				(SystemTypes)7,
				new Vector2(9.5f, -12.5f)
			},
			{
				(SystemTypes)11,
				new Vector2(3f, -12f)
			},
			{
				(SystemTypes)25,
				new Vector2(16.5f, -1.5f)
			},
			{
				(SystemTypes)1,
				new Vector2(20.5f, -11.5f)
			},
			{
				(SystemTypes)21,
				new Vector2(34.5f, -6f)
			},
			{
				(SystemTypes)28,
				new Vector2(36.5f, -21.5f)
			},
			{
				(SystemTypes)18,
				new Vector2(40f, -24.5f)
			},
			{
				(SystemTypes)27,
				new Vector2(7.5f, -7.5f)
			}
		};
	}

	private Dictionary<SystemTypes, Vector2> GetDleksLocations()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<SystemTypes, Vector2> skeldLocations = GetSkeldLocations();
		Dictionary<SystemTypes, Vector2> dictionary = new Dictionary<SystemTypes, Vector2>();
		foreach (KeyValuePair<SystemTypes, Vector2> item in skeldLocations)
		{
			dictionary[item.Key] = new Vector2(0f - item.Value.x, item.Value.y);
		}
		return dictionary;
	}

	private Dictionary<SystemTypes, Vector2> GetAirshipLocations()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		return new Dictionary<SystemTypes, Vector2>
		{
			{
				(SystemTypes)40,
				new Vector2(-0.5f, 8.5f)
			},
			{
				(SystemTypes)39,
				new Vector2(0f, -1f)
			},
			{
				(SystemTypes)33,
				new Vector2(-7f, -11.5f)
			},
			{
				(SystemTypes)45,
				new Vector2(11f, 0f)
			},
			{
				(SystemTypes)44,
				new Vector2(12f, 8.5f)
			},
			{
				(SystemTypes)42,
				new Vector2(20f, 10.5f)
			},
			{
				(SystemTypes)36,
				new Vector2(33.5f, -1.5f)
			},
			{
				(SystemTypes)43,
				new Vector2(30.5f, 7f)
			},
			{
				(SystemTypes)38,
				new Vector2(21f, -0.5f)
			},
			{
				(SystemTypes)41,
				new Vector2(17f, 15f)
			},
			{
				(SystemTypes)30,
				new Vector2(-9f, 12.5f)
			},
			{
				(SystemTypes)14,
				new Vector2(-12.5f, 1.5f)
			},
			{
				(SystemTypes)31,
				new Vector2(-23.5f, -1.5f)
			},
			{
				(SystemTypes)32,
				new Vector2(-10f, -5.5f)
			},
			{
				(SystemTypes)34,
				new Vector2(-14f, -12.5f)
			},
			{
				(SystemTypes)11,
				new Vector2(7.5f, -12f)
			},
			{
				(SystemTypes)7,
				new Vector2(16.5f, -8.5f)
			},
			{
				(SystemTypes)46,
				new Vector2(25.5f, -9.5f)
			}
		};
	}

	private Dictionary<SystemTypes, Vector2> GetFungleLocations()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		return new Dictionary<SystemTypes, Vector2>
		{
			{
				(SystemTypes)2,
				new Vector2(-16.5f, 0f)
			},
			{
				(SystemTypes)25,
				new Vector2(-7.5f, 10.5f)
			},
			{
				(SystemTypes)1,
				new Vector2(-0.5f, -1.5f)
			},
			{
				(SystemTypes)41,
				new Vector2(-2f, -9.5f)
			},
			{
				(SystemTypes)56,
				new Vector2(2.5f, -12.5f)
			},
			{
				(SystemTypes)21,
				new Vector2(3f, -5.5f)
			},
			{
				(SystemTypes)55,
				new Vector2(5.5f, 1f)
			},
			{
				(SystemTypes)24,
				new Vector2(9.5f, -12f)
			},
			{
				(SystemTypes)3,
				new Vector2(22.5f, -7f)
			},
			{
				(SystemTypes)52,
				new Vector2(8.5f, 14.5f)
			},
			{
				(SystemTypes)49,
				new Vector2(12.5f, 12f)
			},
			{
				(SystemTypes)4,
				new Vector2(22f, 3f)
			},
			{
				(SystemTypes)14,
				new Vector2(22f, 14f)
			},
			{
				(SystemTypes)54,
				new Vector2(-14.5f, 10.5f)
			},
			{
				(SystemTypes)33,
				new Vector2(-15.5f, -7.5f)
			},
			{
				(SystemTypes)53,
				new Vector2(-22f, 0f)
			},
			{
				(SystemTypes)50,
				new Vector2(-15.5f, -0.5f)
			},
			{
				(SystemTypes)51,
				new Vector2(0.5f, 2f)
			}
		};
	}

	public void TeleportToLocation(SystemTypes location)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (ValidateGameState() && ValidateTeleportCooldown())
			{
				if (GetLocationsForCurrentMap().TryGetValue(location, out var value))
				{
					ExecuteTeleportWithRetry(value);
					return;
				}
				Debug.LogWarning(Object.op_Implicit($"[TeleportManager] Location {location} not found for current map"));
			}
		}
		catch (Exception value2)
		{
			Debug.LogError(Object.op_Implicit($"[TeleportManager] Location teleport error: {value2}"));
		}
	}

	public void TeleportToPosition(Vector2 position)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (ValidateGameState() && ValidateTeleportCooldown())
			{
				ExecuteTeleportWithRetry(position);
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[TeleportManager] Position teleport error: {value}"));
		}
	}

	public void TeleportToPlayer(PlayerControl target)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (ValidateGameState() && ValidateTeleportCooldown() && ValidateTargetPlayer(target))
			{
				Vector2 truePosition = target.GetTruePosition();
				if (ValidateTeleportDistance(truePosition))
				{
					ExecuteTeleportWithRetry(truePosition);
				}
			}
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[TeleportManager] Player teleport error: {value}"));
		}
	}

	public PlayerControl GetClosestPlayer()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!ValidateGameState())
			{
				return null;
			}
			Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
			PlayerControl result = null;
			float num = float.MaxValue;
			Enumerator<PlayerControl> enumerator = PlayerControl.AllPlayerControls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerControl current = enumerator.Current;
				if (ValidateTargetPlayer(current))
				{
					float num2 = Vector2.Distance(current.GetTruePosition(), truePosition);
					if (num2 < num)
					{
						num = num2;
						result = current;
					}
				}
			}
			return result;
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[TeleportManager] GetClosestPlayer error: {value}"));
			return null;
		}
	}

	public List<PlayerControl> GetPlayersByDistance()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!ValidateGameState())
			{
				return new List<PlayerControl>();
			}
			Vector2 localPosition = PlayerControl.LocalPlayer.GetTruePosition();
			return (from p in (IEnumerable<PlayerControl>)PlayerControl.AllPlayerControls.ToArray()
				where ValidateTargetPlayer(p)
				orderby Vector2.Distance(p.GetTruePosition(), localPosition)
				select p).ToList();
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[TeleportManager] GetPlayersByDistance error: {value}"));
			return new List<PlayerControl>();
		}
	}

	private void ExecuteTeleportWithRetry(Vector2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			ExecuteTeleport(position);
			SyncTeleportWithServer(position);
			lastTeleportTime = Time.time;
			lastTeleportId = (byte)((lastTeleportId + 1) % 255);
		}
		catch (Exception value)
		{
			Debug.LogError(Object.op_Implicit($"[TeleportManager] Failed to execute teleport: {value}"));
		}
	}

	private void ExecuteTeleport(Vector2 position)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerControl.LocalPlayer.inVent)
		{
			PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
		}
		PlayerControl.LocalPlayer.NetTransform.SnapTo(position);
	}

	private void SyncTeleportWithServer(Vector2 position)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (((InnerNetClient)AmongUsClient.Instance).AmClient)
		{
			string token = GenerateSecureToken();
			SendTeleportRpc(PlayerControl.LocalPlayer.PlayerId, position, token);
			SendBackupTeleportSync(position, token);
		}
	}

	private bool ValidateGameState()
	{
		if ((Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)AmongUsClient.Instance == (Object)null)
		{
			Debug.LogWarning(Object.op_Implicit("[TeleportManager] Invalid game state: Player or client is null"));
			return false;
		}
		return ((InnerNetClient)AmongUsClient.Instance).AmConnected;
	}

	private bool ValidateTeleportCooldown()
	{
		return Time.time - lastTeleportTime >= MIN_TELEPORT_INTERVAL;
	}

	private bool IsPlayerTeleportOnCooldown(byte playerId)
	{
		if (lastPlayerTeleports.TryGetValue(playerId, out var value))
		{
			return (DateTime.UtcNow - value).TotalSeconds < (double)MIN_TELEPORT_INTERVAL;
		}
		return false;
	}

	private bool ValidateTargetPlayer(PlayerControl target)
	{
		if ((Object)(object)target != (Object)null && (Object)(object)target != (Object)(object)PlayerControl.LocalPlayer && !target.Data.IsDead)
		{
			return !target.Data.Disconnected;
		}
		return false;
	}

	private bool ValidateTeleportDistance(Vector2 targetPosition)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), targetPosition) <= MAX_TELEPORT_DISTANCE;
	}

	private string GenerateSecureToken()
	{
		byte[] array = new byte[32];
		_secureRandom.GetBytes(array);
		return Convert.ToBase64String(array);
	}

	private void SendTeleportRpc(byte playerId, Vector2 position, string token)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		MessageWriter val = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)PlayerControl.LocalPlayer).NetId, TELEPORT_RPC, (SendOption)1, -1);
		val.Write(playerId);
		val.Write(position.x);
		val.Write(position.y);
		val.Write((float)DateTime.UtcNow.Ticks);
		val.Write(token);
		val.Write(lastTeleportId);
		((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val);
	}

	private void SendBackupTeleportSync(Vector2 position, string token)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		MessageWriter val = ((InnerNetClient)AmongUsClient.Instance).StartRpcImmediately(((InnerNetObject)PlayerControl.LocalPlayer).NetId, TELEPORT_SYNC, (SendOption)1, -1);
		val.Write(PlayerControl.LocalPlayer.PlayerId);
		val.Write(position.x);
		val.Write(position.y);
		val.Write(token);
		((InnerNetClient)AmongUsClient.Instance).FinishRpcImmediately(val);
	}
}
