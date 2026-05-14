using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AmongUs.Data;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace ModMenuCrew.Features;

public static class SpoofingService
{
	private static bool _enableLevelSpoof;

	private static bool _enablePlatformSpoof;

	private static bool _enableFriendCodeSpoof;

	private static bool _enableShuffleName;

	private static bool _enableShuffleColor;

	private static bool _enableShuffleCosmetics;

	private static uint _spoofedLevel = 100u;

	private static Platforms _spoofedPlatform = (Platforms)2;

	private static string _spoofedFriendCode = "";

	private static uint _originalLevel = 0u;

	private static bool _levelCached = false;

	private static string _originalFriendCode = "";

	private static bool _friendCodeCached = false;

	private static string _cachedPlatformName = null;

	private static Platforms _cachedPlatformType = (Platforms)0;

	private static readonly Random _rng = new Random();

	private static string _originalName = "";

	private static byte _originalColor = 0;

	private static string _originalHat = "";

	private static string _originalSkin = "";

	private static string _originalPet = "";

	private static string _originalVisor = "";

	private static bool _identityCached = false;

	private static readonly string[] _shuffleAdj = new string[20]
	{
		"Shadow", "Cosmic", "Neon", "Void", "Glitch", "Phantom", "Frost", "Blaze", "Venom", "Spark",
		"Storm", "Lunar", "Toxic", "Stealth", "Rogue", "Nova", "Pixel", "Flux", "Drift", "Apex"
	};

	private static readonly string[] _shuffleNoun = new string[20]
	{
		"Bean", "Crew", "Ghost", "Star", "Wolf", "Byte", "Mask", "Core", "Edge", "Hex",
		"Imp", "Mist", "Orb", "Shard", "Zap", "Bolt", "Fox", "Raven", "Viper", "Hawk"
	};

	internal static bool EnableLevelSpoof
	{
		get
		{
			return _enableLevelSpoof;
		}
		set
		{
			_enableLevelSpoof = value;
			if (CheatConfig.SpoofLevel != null)
			{
				CheatConfig.SpoofLevel.Value = value;
			}
		}
	}

	internal static bool EnablePlatformSpoof
	{
		get
		{
			return _enablePlatformSpoof;
		}
		set
		{
			_enablePlatformSpoof = value;
			if (CheatConfig.SpoofPlatform != null)
			{
				CheatConfig.SpoofPlatform.Value = value;
			}
		}
	}

	internal static bool EnableFriendCodeSpoof
	{
		get
		{
			return _enableFriendCodeSpoof;
		}
		set
		{
			_enableFriendCodeSpoof = value;
			if (CheatConfig.SpoofFriendCode != null)
			{
				CheatConfig.SpoofFriendCode.Value = value;
			}
		}
	}

	internal static bool EnableShuffleName
	{
		get
		{
			return _enableShuffleName;
		}
		set
		{
			_enableShuffleName = value;
			if (CheatConfig.SpoofShuffleName != null)
			{
				CheatConfig.SpoofShuffleName.Value = value;
			}
		}
	}

	internal static bool EnableShuffleColor
	{
		get
		{
			return _enableShuffleColor;
		}
		set
		{
			_enableShuffleColor = value;
			if (CheatConfig.SpoofShuffleColor != null)
			{
				CheatConfig.SpoofShuffleColor.Value = value;
			}
		}
	}

	internal static bool EnableShuffleCosmetics
	{
		get
		{
			return _enableShuffleCosmetics;
		}
		set
		{
			_enableShuffleCosmetics = value;
			if (CheatConfig.SpoofShuffleCosmetics != null)
			{
				CheatConfig.SpoofShuffleCosmetics.Value = value;
			}
		}
	}

	internal static uint SpoofedLevel
	{
		get
		{
			return _spoofedLevel;
		}
		set
		{
			_spoofedLevel = value;
			if (CheatConfig.SpoofLevelValue != null)
			{
				CheatConfig.SpoofLevelValue.Value = (int)value;
			}
		}
	}

	internal static uint CustomLevel { get; set; } = 0u;


	internal static Platforms SpoofedPlatform
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return _spoofedPlatform;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			_spoofedPlatform = value;
			if (CheatConfig.SpoofPlatformValue != null)
			{
				CheatConfig.SpoofPlatformValue.Value = value;
			}
		}
	}

	internal static string SpoofedPlatformName { get; set; } = "";


	internal static string SpoofedFriendCode
	{
		get
		{
			return _spoofedFriendCode;
		}
		set
		{
			_spoofedFriendCode = value ?? "";
			if (CheatConfig.SpoofFriendCodeValue != null)
			{
				CheatConfig.SpoofFriendCodeValue.Value = _spoofedFriendCode;
			}
		}
	}

	public static uint GetEffectiveLevel()
	{
		if (CustomLevel == 0)
		{
			return SpoofedLevel;
		}
		return CustomLevel;
	}

	internal static void SetCustomLevel(uint level)
	{
		if (level != 0 && level <= 10000)
		{
			CustomLevel = level;
			ApplyLevelSpoof();
		}
	}

	internal static void SetLevel(uint level)
	{
		SpoofedLevel = level;
		CustomLevel = 0u;
		ApplyLevelSpoof();
	}

	internal static void ApplyLevelSpoof()
	{
		if (!EnableLevelSpoof)
		{
			return;
		}
		try
		{
			if (DataManager.Player == null || DataManager.Player.Stats == null)
			{
				Debug.LogWarning(Object.op_Implicit("[SpoofingService] DataManager not available"));
				return;
			}
			if (!_levelCached)
			{
				_originalLevel = DataManager.Player.Stats.Level;
				_levelCached = true;
				Debug.Log(Object.op_Implicit($"[SpoofingService] Level original: {_originalLevel}"));
			}
			uint effectiveLevel = GetEffectiveLevel();
			uint level = DataManager.Player.Stats.Level;
			if (effectiveLevel != 0 && effectiveLevel != level)
			{
				DataManager.Player.Stats.Level = effectiveLevel;
				((AbstractSaveData)DataManager.Player).Save();
				Debug.Log(Object.op_Implicit($"[SpoofingService] Level local: {level} -> {effectiveLevel}"));
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[SpoofingService] Level error: " + ex.Message));
		}
	}

	internal static void RestoreLevel()
	{
		if (!_levelCached)
		{
			return;
		}
		try
		{
			if (DataManager.Player != null && DataManager.Player.Stats != null)
			{
				DataManager.Player.Stats.Level = _originalLevel;
				((AbstractSaveData)DataManager.Player).Save();
				Debug.Log(Object.op_Implicit($"[SpoofingService] Level restaurado: {_originalLevel}"));
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[SpoofingService] Error restoring: " + ex.Message));
		}
	}

	internal static void ApplyPlatformSpoof(PlatformSpecificData data)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		if (!EnablePlatformSpoof || data == null)
		{
			return;
		}
		try
		{
			data.Platform = SpoofedPlatform;
			if (!string.IsNullOrEmpty(SpoofedPlatformName))
			{
				data.PlatformName = SpoofedPlatformName;
			}
			else
			{
				if (_cachedPlatformName == null || _cachedPlatformType != SpoofedPlatform)
				{
					_cachedPlatformName = GetDefaultPlatformName(SpoofedPlatform);
					_cachedPlatformType = SpoofedPlatform;
				}
				data.PlatformName = _cachedPlatformName;
			}
			Platforms spoofedPlatform = SpoofedPlatform;
			if ((int)spoofedPlatform != 4 && (int)spoofedPlatform != 9)
			{
				if ((int)spoofedPlatform == 10)
				{
					data.PsnPlatformId = GenerateFakePsnId();
				}
			}
			else
			{
				data.XboxPlatformId = GenerateFakeXboxId();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[SpoofingService] Platform error: " + ex.Message));
		}
	}

	private static string GetDefaultPlatformName(Platforms platform)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected I4, but got Unknown
		return (platform - 1) switch
		{
			1 => $"Player{_rng.Next(1000, 99999)}", 
			0 => $"Player{_rng.Next(1000, 99999)}", 
			3 => $"Player{_rng.Next(1000, 99999)}", 
			2 => $"Player{_rng.Next(1000, 99999)}", 
			4 => $"Player{_rng.Next(1000, 99999)}", 
			6 => "", 
			5 => "", 
			7 => "", 
			8 => $"Player{_rng.Next(1000, 99999)}", 
			9 => "", 
			_ => "", 
		};
	}

	private static ulong GenerateFakeXboxId()
	{
		byte[] array = new byte[8];
		_rng.NextBytes(array);
		return BitConverter.ToUInt64(array, 0) & 0xFFFFFFFFFFFFuL;
	}

	private static ulong GenerateFakePsnId()
	{
		byte[] array = new byte[8];
		_rng.NextBytes(array);
		return BitConverter.ToUInt64(array, 0) & 0xFFFFFFFFFFFFuL;
	}

	internal static void SetPlatform(Platforms platform)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		SpoofedPlatform = platform;
		_cachedPlatformName = null;
	}

	internal static void SetPlatformName(string name)
	{
		SpoofedPlatformName = name ?? "";
	}

	internal static void ApplyFriendCodeSpoof()
	{
		if (!EnableFriendCodeSpoof)
		{
			return;
		}
		try
		{
			if (!_friendCodeCached)
			{
				try
				{
					_originalFriendCode = DestroyableSingleton<EOSManager>.Instance.FriendCode ?? "";
				}
				catch
				{
				}
				_friendCodeCached = true;
			}
			string text = ((!string.IsNullOrEmpty(SpoofedFriendCode)) ? SpoofedFriendCode : GenerateRandomFriendCode());
			try
			{
				DestroyableSingleton<EOSManager>.Instance.FriendCode = text;
			}
			catch
			{
			}
			if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
			{
				PlayerControl.LocalPlayer.FriendCode = text;
			}
			Debug.Log(Object.op_Implicit("[SpoofingService] FriendCode spoofed to: " + text));
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[SpoofingService] FriendCode error: " + ex.Message));
		}
	}

	public static string GenerateRandomFriendCode()
	{
		string[] array = new string[10] { "cosmic", "stellar", "nebula", "astral", "lunar", "solar", "vortex", "plasma", "photon", "quasar" };
		string[] array2 = new string[10] { "flux", "wave", "beam", "core", "node", "link", "zone", "pulse", "spark", "glow" };
		return $"{array[_rng.Next(array.Length)]}{array2[_rng.Next(array2.Length)]}#{_rng.Next(1000, 9999)}";
	}

	internal static void RestoreFriendCode()
	{
		if (!_friendCodeCached)
		{
			return;
		}
		try
		{
			try
			{
				DestroyableSingleton<EOSManager>.Instance.FriendCode = _originalFriendCode;
			}
			catch
			{
			}
			if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
			{
				PlayerControl.LocalPlayer.FriendCode = _originalFriendCode;
			}
		}
		catch
		{
		}
	}

	private static void CacheOriginalIdentity()
	{
		if (_identityCached)
		{
			return;
		}
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if (!((Object)(object)localPlayer == (Object)null) && !((Object)(object)localPlayer.Data == (Object)null))
			{
				_originalName = localPlayer.Data.PlayerName ?? "";
				_originalColor = (byte)localPlayer.Data.DefaultOutfit.ColorId;
				_originalHat = localPlayer.Data.DefaultOutfit.HatId ?? "";
				_originalSkin = localPlayer.Data.DefaultOutfit.SkinId ?? "";
				_originalPet = localPlayer.Data.DefaultOutfit.PetId ?? "";
				_originalVisor = localPlayer.Data.DefaultOutfit.VisorId ?? "";
				_identityCached = true;
			}
		}
		catch
		{
		}
	}

	internal static void ApplyIdentityShuffle()
	{
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if (!((Object)(object)localPlayer == (Object)null) && !((Object)(object)localPlayer.Data == (Object)null))
			{
				CacheOriginalIdentity();
				if (EnableShuffleColor)
				{
					RandomizeColor(localPlayer);
				}
				if (EnableShuffleCosmetics)
				{
					RandomizeCosmetics(localPlayer);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(Object.op_Implicit("[SpoofingService] IdentityShuffle: " + ex.Message));
		}
	}

	private static void RandomizeName(PlayerControl p)
	{
		try
		{
			string text = _shuffleAdj[_rng.Next(_shuffleAdj.Length)] + _shuffleNoun[_rng.Next(_shuffleNoun.Length)] + _rng.Next(10, 99);
			p.CmdCheckName(text);
		}
		catch
		{
		}
	}

	private static void RandomizeColor(PlayerControl p)
	{
		try
		{
			List<byte> availableColors = GetAvailableColors(p);
			if (availableColors.Count != 0)
			{
				byte b = availableColors[_rng.Next(availableColors.Count)];
				p.CmdCheckColor(b);
			}
		}
		catch
		{
		}
	}

	private static List<byte> GetAvailableColors(PlayerControl local)
	{
		HashSet<byte> hashSet = new HashSet<byte>();
		try
		{
			if ((Object)(object)GameData.Instance != (Object)null)
			{
				Enumerator<NetworkedPlayerInfo> enumerator = GameData.Instance.AllPlayers.GetEnumerator();
				while (enumerator.MoveNext())
				{
					NetworkedPlayerInfo current = enumerator.Current;
					if (!((Object)(object)current == (Object)null) && !current.Disconnected && current.PlayerId != local.PlayerId)
					{
						hashSet.Add((byte)current.DefaultOutfit.ColorId);
					}
				}
			}
		}
		catch
		{
		}
		int num = 18;
		try
		{
			num = ((Il2CppArrayBase<Color32>)(object)Palette.PlayerColors).Length;
		}
		catch
		{
		}
		List<byte> list = new List<byte>();
		for (byte b = 0; b < num; b++)
		{
			if (!hashSet.Contains(b))
			{
				list.Add(b);
			}
		}
		return list;
	}

	private static void RandomizeCosmetics(PlayerControl p)
	{
		try
		{
			HatManager instance = DestroyableSingleton<HatManager>.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				return;
			}
			Il2CppReferenceArray<HatData> allHats = instance.allHats;
			if (allHats != null && ((Il2CppArrayBase<HatData>)(object)allHats).Length > 0)
			{
				HatData val = ((Il2CppArrayBase<HatData>)(object)allHats)[_rng.Next(((Il2CppArrayBase<HatData>)(object)allHats).Length)];
				if ((Object)(object)val != (Object)null && !string.IsNullOrEmpty(((CosmeticData)val).ProdId))
				{
					p.RpcSetHat(((CosmeticData)val).ProdId);
				}
			}
			Il2CppReferenceArray<SkinData> allSkins = instance.allSkins;
			if (allSkins != null && ((Il2CppArrayBase<SkinData>)(object)allSkins).Length > 0)
			{
				SkinData val2 = ((Il2CppArrayBase<SkinData>)(object)allSkins)[_rng.Next(((Il2CppArrayBase<SkinData>)(object)allSkins).Length)];
				if ((Object)(object)val2 != (Object)null && !string.IsNullOrEmpty(((CosmeticData)val2).ProdId))
				{
					p.RpcSetSkin(((CosmeticData)val2).ProdId);
				}
			}
			Il2CppReferenceArray<PetData> allPets = instance.allPets;
			if (allPets != null && ((Il2CppArrayBase<PetData>)(object)allPets).Length > 0)
			{
				PetData val3 = ((Il2CppArrayBase<PetData>)(object)allPets)[_rng.Next(((Il2CppArrayBase<PetData>)(object)allPets).Length)];
				if ((Object)(object)val3 != (Object)null && !string.IsNullOrEmpty(((CosmeticData)val3).ProdId))
				{
					p.RpcSetPet(((CosmeticData)val3).ProdId);
				}
			}
			Il2CppReferenceArray<VisorData> allVisors = instance.allVisors;
			if (allVisors != null && ((Il2CppArrayBase<VisorData>)(object)allVisors).Length > 0)
			{
				VisorData val4 = ((Il2CppArrayBase<VisorData>)(object)allVisors)[_rng.Next(((Il2CppArrayBase<VisorData>)(object)allVisors).Length)];
				if ((Object)(object)val4 != (Object)null && !string.IsNullOrEmpty(((CosmeticData)val4).ProdId))
				{
					p.RpcSetVisor(((CosmeticData)val4).ProdId);
				}
			}
		}
		catch
		{
		}
	}

	internal static void RestoreIdentity()
	{
		if (!_identityCached)
		{
			return;
		}
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if (!((Object)(object)localPlayer == (Object)null))
			{
				localPlayer.CmdCheckName(_originalName);
				localPlayer.CmdCheckColor(_originalColor);
				localPlayer.RpcSetHat(_originalHat);
				localPlayer.RpcSetSkin(_originalSkin);
				localPlayer.RpcSetPet(_originalPet);
				localPlayer.RpcSetVisor(_originalVisor);
			}
		}
		catch
		{
		}
	}

	internal static bool IsAnyShuffleEnabled()
	{
		if (!EnableShuffleName && !EnableShuffleColor)
		{
			return EnableShuffleCosmetics;
		}
		return true;
	}

	public static string PlatformToString(Platforms platform)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected I4, but got Unknown
		return (int)platform switch
		{
			0 => "Unknown", 
			2 => "Steam", 
			1 => "Epic", 
			4 => "Microsoft Store", 
			3 => "Mac", 
			5 => "Itch.io", 
			7 => "Android", 
			6 => "iPhone", 
			8 => "Switch", 
			9 => "Xbox", 
			10 => "PlayStation", 
			_ => ((object)(Platforms)platform).ToString(), 
		};
	}

	public static Platforms[] GetAllPlatforms()
	{
		Platforms[] array = new Platforms[10];
		RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		return (Platforms[])(object)array;
	}

	public static string GetStatus()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		if (EnableLevelSpoof)
		{
			text += $"Lv{GetEffectiveLevel()} ";
		}
		if (EnablePlatformSpoof)
		{
			text = text + PlatformToString(SpoofedPlatform) + " ";
		}
		if (EnableFriendCodeSpoof)
		{
			text += "FC ";
		}
		if (IsAnyShuffleEnabled())
		{
			text += "Shuffle ";
		}
		if (!string.IsNullOrEmpty(text))
		{
			return text.Trim();
		}
		return "OFF";
	}

	internal static void DisableAll()
	{
		EnableLevelSpoof = false;
		EnablePlatformSpoof = false;
		EnableFriendCodeSpoof = false;
		EnableShuffleName = false;
		EnableShuffleColor = false;
		EnableShuffleCosmetics = false;
		RestoreLevel();
		RestoreFriendCode();
		RestoreIdentity();
	}

	internal static void MarkForReapplication()
	{
		ApplyLevelSpoof();
		if (EnableFriendCodeSpoof)
		{
			ApplyFriendCodeSpoof();
		}
	}

	public static int GetActiveCount()
	{
		int num = 0;
		if (EnableLevelSpoof)
		{
			num++;
		}
		if (EnablePlatformSpoof)
		{
			num++;
		}
		if (EnableFriendCodeSpoof)
		{
			num++;
		}
		if (IsAnyShuffleEnabled())
		{
			num++;
		}
		return num;
	}
}
