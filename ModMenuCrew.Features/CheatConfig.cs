using System;
using AmongUs.GameOptions;
using BepInEx.Configuration;
using UnityEngine;

namespace ModMenuCrew.Features;

public static class CheatConfig
{
	public static bool GodMode;

	public static bool GodModeAll;

	public static ConfigFile Config { get; private set; }

	public static ConfigEntry<KeyCode> MenuToggleKey { get; private set; }

	public static ConfigEntry<float> MenuOpacity { get; private set; }

	public static ConfigEntry<float> MenuWidth { get; private set; }

	public static ConfigEntry<float> MenuHeight { get; private set; }

	public static ConfigEntry<bool> SeeGhosts { get; private set; }

	public static ConfigEntry<bool> SeeDeadChat { get; private set; }

	public static ConfigEntry<bool> FreeCamEnabled { get; private set; }

	public static ConfigEntry<bool> NoClipSmoothEnabled { get; private set; }

	public static ConfigEntry<bool> CustomSpeedEnabled { get; private set; }

	public static ConfigEntry<bool> InfiniteVision { get; private set; }

	public static float VisionMultiplier { get; internal set; } = 3f;


	public static ConfigEntry<bool> LockScrollZoom { get; private set; }

	public static ConfigEntry<bool> RadarEnabled { get; private set; }

	public static ConfigEntry<bool> TracersEnabled { get; private set; }

	public static ConfigEntry<float> RadarScale { get; private set; }

	public static ConfigEntry<bool> RadarShowMapImage { get; private set; }

	public static ConfigEntry<bool> WebRadarEnabled { get; private set; }

	public static ConfigEntry<float> RadarMapZoom { get; private set; }

	public static ConfigEntry<bool> AllowVenting { get; private set; }

	public static ConfigEntry<bool> WalkInVent { get; private set; }

	public static ConfigEntry<bool> TeleportWithCursor { get; private set; }

	public static ConfigEntry<bool> NoKillCooldown { get; private set; }

	public static ConfigEntry<bool> InstantWin { get; private set; }

	public static ConfigEntry<bool> DisableGameEnd { get; private set; }

	public static ConfigEntry<bool> ForceImpostor { get; private set; }

	public static ConfigEntry<string> SelectedRole { get; private set; }

	public static ConfigEntry<bool> RevealVotes { get; private set; }

	public static ConfigEntry<bool> RevealSus { get; private set; }

	public static ConfigEntry<bool> NoKillDistanceLimit { get; private set; }

	public static ConfigEntry<bool> SpoofLevel { get; private set; }

	public static ConfigEntry<bool> SpoofPlatform { get; private set; }

	public static ConfigEntry<bool> SpoofFriendCode { get; private set; }

	public static ConfigEntry<bool> SpoofShuffleName { get; private set; }

	public static ConfigEntry<bool> SpoofShuffleColor { get; private set; }

	public static ConfigEntry<bool> SpoofShuffleCosmetics { get; private set; }

	public static ConfigEntry<int> SpoofLevelValue { get; private set; }

	public static ConfigEntry<Platforms> SpoofPlatformValue { get; private set; }

	public static ConfigEntry<string> SpoofFriendCodeValue { get; private set; }

	public static ConfigEntry<bool> EndlessVentTime { get; private set; }

	public static ConfigEntry<bool> NoVentCooldown { get; private set; }

	public static ConfigEntry<bool> EndlessShapeshiftDuration { get; private set; }

	public static ConfigEntry<bool> NoShapeshiftCooldown { get; private set; }

	public static ConfigEntry<bool> EndlessBattery { get; private set; }

	public static ConfigEntry<bool> NoVitalsCooldown { get; private set; }

	public static ConfigEntry<bool> EndlessTracking { get; private set; }

	public static ConfigEntry<bool> NoTrackingCooldown { get; private set; }

	public static ConfigEntry<bool> ShowKillCooldowns { get; private set; }

	public static ConfigEntry<bool> NoSabotageCooldown { get; private set; }

	public static ConfigEntry<bool> CrewmateSabotage { get; private set; }

	public static ConfigEntry<bool> MultiSabotage { get; private set; }

	public static ConfigEntry<bool> GodMapEnabled { get; private set; }

	public static ConfigEntry<bool> EventLoggerEnabled { get; private set; }

	public static ConfigEntry<bool> KillAlertsEnabled { get; private set; }

	public static ConfigEntry<bool> NoShadows { get; private set; }

	public static ConfigEntry<bool> Nf4 { get; private set; }

	public static ConfigEntry<bool> Nf7 { get; private set; }

	public static ConfigEntry<bool> PhantomMode { get; private set; }

	public static ConfigEntry<bool> HideMMCStar { get; private set; }

	public static ConfigEntry<bool> ShowPlayerInfo { get; private set; }

	public static ConfigEntry<bool> InfiniteReportRange { get; private set; }

	public static ConfigEntry<bool> ChatAllowPaste { get; private set; }

	public static ConfigEntry<int> ThemeIndex { get; private set; }

	public static ConfigEntry<string> PinnedSettings { get; private set; }

	public static ConfigEntry<bool> UnlockHats { get; private set; }

	public static ConfigEntry<bool> UnlockSkins { get; private set; }

	public static ConfigEntry<bool> UnlockPets { get; private set; }

	public static ConfigEntry<bool> UnlockVisors { get; private set; }

	public static ConfigEntry<bool> UnlockNameplates { get; private set; }

	public static ConfigEntry<bool> UnlockBundles { get; private set; }

	public static ConfigEntry<bool> UnlockStarsBeans { get; private set; }

	public static ConfigEntry<bool> AutoKickByLevelEnabled { get; private set; }

	public static ConfigEntry<int> AutoKickMinLevel { get; private set; }

	public static ConfigEntry<bool> AutoKickByNameEnabled { get; private set; }

	public static ConfigEntry<string> AutoKickNameList { get; private set; }

	public static ConfigEntry<bool> AutoKickByChatEnabled { get; private set; }

	public static ConfigEntry<string> AutoKickChatWordList { get; private set; }

	public static ConfigEntry<int> ShadowClonesCount { get; private set; }

	public static ConfigEntry<bool> ShadowClonesOrbit { get; private set; }

	public static ConfigEntry<float> ShadowCloneOrbitRadius { get; private set; }

	public static ConfigEntry<float> ShadowCloneOrbitSpeed { get; private set; }

	public static ConfigEntry<KeyCode> KeybindRadar { get; private set; }

	public static ConfigEntry<KeyCode> KeybindFreeCam { get; private set; }

	public static ConfigEntry<KeyCode> KeybindNoClip { get; private set; }

	public static ConfigEntry<KeyCode> KeybindTracers { get; private set; }

	public static ConfigEntry<KeyCode> KeybindSeeGhosts { get; private set; }

	public static ConfigEntry<KeyCode> KeybindNoKillCooldown { get; private set; }

	public static void Initialize(ConfigFile config)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Expected O, but got Unknown
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		//IL_0666: Unknown result type (might be due to invalid IL or missing references)
		//IL_0670: Expected O, but got Unknown
		//IL_078a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0794: Expected O, but got Unknown
		//IL_082a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0834: Expected O, but got Unknown
		//IL_087d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0887: Expected O, but got Unknown
		//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bf: Expected O, but got Unknown
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab8: Unknown result type (might be due to invalid IL or missing references)
		Config = config;
		MenuToggleKey = config.Bind<KeyCode>("1. Menu", "Menu Toggle Key", (KeyCode)282, "Key to toggle the mod menu (default: F1). You can change to any key like F2, Insert, Delete, etc.");
		MenuOpacity = config.Bind<float>("1. Menu", "Menu Opacity", 0.95f, new ConfigDescription("Menu transparency (1.0 = opaque, 0.3 = very transparent)", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.3f, 1f), Array.Empty<object>()));
		MenuWidth = config.Bind<float>("1. Menu", "Menu Width", 770f, new ConfigDescription("Menu window width in pixels (use when resizer doesn't work in fullscreen)", (AcceptableValueBase)(object)new AcceptableValueRange<float>(400f, 1200f), Array.Empty<object>()));
		MenuHeight = config.Bind<float>("1. Menu", "Menu Height", 600f, new ConfigDescription("Menu window height in pixels (use when resizer doesn't work in fullscreen)", (AcceptableValueBase)(object)new AcceptableValueRange<float>(400f, 1000f), Array.Empty<object>()));
		SeeGhosts = config.Bind<bool>("8. Vision", "See Ghosts", false, "See ghosts while alive");
		SeeDeadChat = config.Bind<bool>("8. Vision", "See Dead Chat", false, "Read dead player chat while alive");
		FreeCamEnabled = config.Bind<bool>("8. Vision", "FreeCam", false, "Free camera mode (WASD + Q/E)");
		NoClipSmoothEnabled = config.Bind<bool>("8. Vision", "NoClip", false, "Walk through walls");
		CustomSpeedEnabled = config.Bind<bool>("8. Vision", "Custom Speed", false, "Custom player speed");
		InfiniteVision = config.Bind<bool>("8. Vision", "Infinite Vision", false, "See entire map");
		LockScrollZoom = config.Bind<bool>("8. Vision", "Lock Scroll Zoom", false, "Disable scroll wheel from changing vision range / satellite view zoom");
		RadarEnabled = config.Bind<bool>("9. Radar", "Radar Enabled", false, "Show player positions on radar");
		TracersEnabled = config.Bind<bool>("9. Radar", "Tracers", false, "Lines showing player positions");
		RadarScale = config.Bind<float>("9. Radar", "Radar Scale", 0.08f, new ConfigDescription("Radar zoom level", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.03f, 0.15f), Array.Empty<object>()));
		RadarShowMapImage = config.Bind<bool>("9. Radar", "Show Map Image", true, "Show actual map image as radar background (Skeld only)");
		WebRadarEnabled = config.Bind<bool>("9. Radar", "Web Radar", false, "Enable web-based radar on localhost:9222 (accessible from browser/phone)");
		RadarMapZoom = config.Bind<float>("9. Radar", "Map Zoom", 1f, new ConfigDescription("Zoom level for map image (scroll mouse on radar)", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.5f, 3f), Array.Empty<object>()));
		AllowVenting = config.Bind<bool>("10. Gameplay", "Allow Venting", false, "All roles can use vents");
		WalkInVent = config.Bind<bool>("10. Gameplay", "Walk In Vent", false, "Move freely while invisible inside vents");
		TeleportWithCursor = config.Bind<bool>("10. Gameplay", "Teleport With Cursor", false, "Click to teleport");
		NoKillCooldown = config.Bind<bool>("10. Gameplay", "No Kill Cooldown", false, "Kill without cooldown");
		InstantWin = config.Bind<bool>("10. Gameplay", "Instant Win", false, "Force instant win");
		DisableGameEnd = config.Bind<bool>("10. Gameplay", "Disable Game End", false, "Prevent game from ending");
		ForceImpostor = config.Bind<bool>("10. Gameplay", "Force Impostor", false, "Always be impostor (host)");
		SelectedRole = config.Bind<string>("10. Gameplay", "Selected Role", "Impostor", "Saved role for host override (Impostor, Shapeshifter, Phantom, Viper, etc.)");
		RevealVotes = config.Bind<bool>("10. Gameplay", "Reveal Votes", true, "See who voted for whom during meetings");
		RevealSus = config.Bind<bool>("10. Gameplay", "Reveal Sus", false, "Show impostor names and roles above players");
		NoKillDistanceLimit = config.Bind<bool>("10. Gameplay", "No Kill Distance Limit", false, "Kill from any distance");
		if (ForceImpostor.Value)
		{
			ImpostorForcer.SetRoleOverrideEnabled(enabled: true);
			try
			{
				ImpostorForcer.SetSelectedRoleForHost((RoleTypes)Enum.Parse(typeof(RoleTypes), SelectedRole.Value ?? "Impostor"));
			}
			catch
			{
				ImpostorForcer.SetSelectedRoleForHost((RoleTypes)1);
			}
		}
		EndlessVentTime = config.Bind<bool>("11. Role Engineer", "Endless Vent Time", false, "Unlimited time in vents");
		NoVentCooldown = config.Bind<bool>("11. Role Engineer", "No Vent Cooldown", false, "No vent cooldown");
		EndlessShapeshiftDuration = config.Bind<bool>("12. Role Shapeshifter", "Endless Duration", false, "Unlimited shapeshift duration");
		NoShapeshiftCooldown = config.Bind<bool>("12. Role Shapeshifter", "No Cooldown", false, "No shapeshift cooldown");
		EndlessBattery = config.Bind<bool>("13. Role Scientist", "Endless Battery", false, "Unlimited vitals battery");
		NoVitalsCooldown = config.Bind<bool>("13. Role Scientist", "No Vitals Cooldown", false, "No vitals cooldown");
		EndlessTracking = config.Bind<bool>("14. Role Tracker", "Endless Tracking", false, "Unlimited tracking duration");
		NoTrackingCooldown = config.Bind<bool>("14. Role Tracker", "No Tracking Cooldown", false, "No tracking cooldown");
		ShowKillCooldowns = config.Bind<bool>("15. New Features", "Show Kill Cooldowns", false, "Show cooldown timer above impostors");
		NoSabotageCooldown = config.Bind<bool>("15. New Features", "No Sabotage Cooldown", false, "Bypass sabotage cooldown");
		CrewmateSabotage = config.Bind<bool>("15. New Features", "Crewmate Sabotage", false, "Allow crewmates to open sabotage map");
		MultiSabotage = config.Bind<bool>("15. New Features", "Multi Sabotage", false, "Allow multiple sabotages at once");
		GodMapEnabled = config.Bind<bool>("15. New Features", "God Map", false, "See all players on map");
		EventLoggerEnabled = config.Bind<bool>("15. New Features", "Event Logger", false, "Log game events (kills, tasks, etc)");
		KillAlertsEnabled = config.Bind<bool>("15. New Features", "Kill Alerts", false, "Alert when impostor kills someone");
		NoShadows = config.Bind<bool>("15. New Features", "No Shadows", false, "Remove fog and shadows for better visibility");
		Nf4 = config.Bind<bool>("15. New Features", "Network Filter", true, "Advanced network packet validation");
		Nf7 = config.Bind<bool>("15. New Features", "Network Filter Extended", true, "Extended network packet validation");
		PhantomMode = config.Bind<bool>("15. New Features", "Phantom Mode", false, "Infinite vanish + kill while invisible (requires Phantom role)");
		HideMMCStar = config.Bind<bool>("15. New Features", "Hide MMC Star", false, "Hide the star (★) prefix on MMC user names");
		ShowPlayerInfo = config.Bind<bool>("7. Lobby", "Show Player Info", false, "Show platform, level and ID next to each player in the player list");
		InfiniteReportRange = config.Bind<bool>("15. New Features", "Infinite Report Range", false, "Report dead bodies from anywhere on the map");
		ChatAllowPaste = config.Bind<bool>("10. Gameplay", "Chat Allow Paste", false, "Enable Ctrl+V paste in the in-game chat input (still capped at 100 chars)");
		ThemeIndex = config.Bind<int>("1. Menu", "Theme", 0, new ConfigDescription("UI color theme (0=Red Void, 1=Phantom Blue, 2=Venom Green, 3=Eclipse Purple, 4=Amber, 5=Frost)", (AcceptableValueBase)(object)new AcceptableValueRange<int>(0, 5), Array.Empty<object>()));
		PinnedSettings = config.Bind<string>("1. Menu", "Pinned Settings", "", "Comma-separated keys shown in Quick Access");
		UnlockHats = config.Bind<bool>("19. Cosmetics", "Unlock Hats", true, "Unlock all hats");
		UnlockSkins = config.Bind<bool>("19. Cosmetics", "Unlock Skins", true, "Unlock all skins");
		UnlockPets = config.Bind<bool>("19. Cosmetics", "Unlock Pets", true, "Unlock all pets");
		UnlockVisors = config.Bind<bool>("19. Cosmetics", "Unlock Visors", true, "Unlock all visors");
		UnlockNameplates = config.Bind<bool>("19. Cosmetics", "Unlock Nameplates", true, "Unlock all nameplates");
		UnlockBundles = config.Bind<bool>("19. Cosmetics", "Unlock Bundles", true, "Unlock all bundles and featured items");
		UnlockStarsBeans = config.Bind<bool>("19. Cosmetics", "Unlock Stars & Beans", true, "Set Stars and Beans to 999999");
		AutoKickByLevelEnabled = config.Bind<bool>("18. Host - AutoKick", "AutoKick By Level", false, "Automatically kick players below a minimum level (HOST ONLY)");
		AutoKickMinLevel = config.Bind<int>("18. Host - AutoKick", "Min Level", 5, new ConfigDescription("Minimum level required to stay in lobby", (AcceptableValueBase)(object)new AcceptableValueRange<int>(1, 100), Array.Empty<object>()));
		AutoKickByNameEnabled = config.Bind<bool>("18. Host - AutoKick", "AutoKick By Name", false, "Automatically kick players whose name matches the blacklist (HOST ONLY)");
		AutoKickNameList = config.Bind<string>("18. Host - AutoKick", "Name Blacklist", "", "Comma-separated list of player names to auto-kick (case-insensitive)");
		AutoKickByChatEnabled = config.Bind<bool>("18. Host - AutoKick", "AutoKick By Chat", false, "Automatically kick players whose chat contains a blacklisted word (HOST ONLY)");
		AutoKickChatWordList = config.Bind<string>("18. Host - AutoKick", "Chat Word Blacklist", "", "Comma-separated list of words; add/remove in-game with /bw add|remove|clear (case-insensitive substring match)");
		ShadowClonesCount = config.Bind<int>("19. Host - LAB", "Shadow Clones Count", 1, new ConfigDescription("Number of shadow clones to spawn per non-host player (1-8)", (AcceptableValueBase)(object)new AcceptableValueRange<int>(1, 8), Array.Empty<object>()));
		ShadowClonesOrbit = config.Bind<bool>("19. Host - LAB", "Shadow Clones Orbit", false, "Clones orbit around each player instead of stacking on their position");
		ShadowCloneOrbitRadius = config.Bind<float>("19. Host - LAB", "Orbit Radius", 1.5f, new ConfigDescription("Distance from player to clones when orbit is enabled", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.3f, 4f), Array.Empty<object>()));
		ShadowCloneOrbitSpeed = config.Bind<float>("19. Host - LAB", "Orbit Speed", 3f, new ConfigDescription("Angular speed of orbiting clones (rad/s)", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0.5f, 10f), Array.Empty<object>()));
		KeybindRadar = config.Bind<KeyCode>("17. Keybinds", "Radar Keybind", (KeyCode)0, "Hotkey to toggle Radar (set to None to disable)");
		KeybindFreeCam = config.Bind<KeyCode>("17. Keybinds", "FreeCam Keybind", (KeyCode)0, "Hotkey to toggle FreeCam (set to None to disable)");
		KeybindNoClip = config.Bind<KeyCode>("17. Keybinds", "NoClip Keybind", (KeyCode)0, "Hotkey to toggle NoClip (set to None to disable)");
		KeybindTracers = config.Bind<KeyCode>("17. Keybinds", "Tracers Keybind", (KeyCode)0, "Hotkey to toggle Tracers (set to None to disable)");
		KeybindSeeGhosts = config.Bind<KeyCode>("17. Keybinds", "See Ghosts Keybind", (KeyCode)0, "Hotkey to toggle See Ghosts (set to None to disable)");
		KeybindNoKillCooldown = config.Bind<KeyCode>("17. Keybinds", "No Kill CD Keybind", (KeyCode)0, "Hotkey to toggle No Kill Cooldown (set to None to disable)");
		SpoofLevel = config.Bind<bool>("20. Spoofing", "Spoof Level", false, "Randomize your level");
		SpoofPlatform = config.Bind<bool>("20. Spoofing", "Spoof Platform", false, "Randomize your platform");
		SpoofFriendCode = config.Bind<bool>("20. Spoofing", "Spoof Friend Code", false, "Randomize your friend code");
		SpoofShuffleName = config.Bind<bool>("20. Spoofing", "Shuffle Name", false, "Randomize name each lobby");
		SpoofShuffleColor = config.Bind<bool>("20. Spoofing", "Shuffle Color", false, "Randomize color each lobby");
		SpoofShuffleCosmetics = config.Bind<bool>("20. Spoofing", "Shuffle Cosmetics", false, "Randomize cosmetics each lobby");
		SpoofLevelValue = config.Bind<int>("20. Spoofing", "Spoof Level Value", 100, "Saved spoofed level (1-10000)");
		SpoofPlatformValue = config.Bind<Platforms>("20. Spoofing", "Spoof Platform Value", (Platforms)2, "Saved spoofed platform");
		SpoofFriendCodeValue = config.Bind<string>("20. Spoofing", "Spoof Friend Code Value", "", "Saved spoofed friend code");
		GameCheats.NoKillDistanceLimitEnabled = NoKillDistanceLimit?.Value ?? false;
		GameCheats.IsRevealSusActive = RevealSus?.Value ?? false;
		SpoofingService.SpoofedLevel = (uint)Mathf.Clamp(SpoofLevelValue?.Value ?? 100, 1, 10000);
		ConfigEntry<Platforms> spoofPlatformValue = SpoofPlatformValue;
		SpoofingService.SpoofedPlatform = (Platforms)((spoofPlatformValue == null) ? 2 : ((int)spoofPlatformValue.Value));
		SpoofingService.SpoofedFriendCode = SpoofFriendCodeValue?.Value ?? "";
		SpoofingService.EnableLevelSpoof = SpoofLevel?.Value ?? false;
		SpoofingService.EnablePlatformSpoof = SpoofPlatform?.Value ?? false;
		SpoofingService.EnableFriendCodeSpoof = SpoofFriendCode?.Value ?? false;
		SpoofingService.EnableShuffleName = SpoofShuffleName?.Value ?? false;
		SpoofingService.EnableShuffleColor = SpoofShuffleColor?.Value ?? false;
		SpoofingService.EnableShuffleCosmetics = SpoofShuffleCosmetics?.Value ?? false;
	}

	public static void Save()
	{
		ConfigFile config = Config;
		if (config != null)
		{
			config.Save();
		}
	}

	public static void Reload()
	{
		ConfigFile config = Config;
		if (config != null)
		{
			config.Reload();
		}
	}

	public static void ResetToDefaults()
	{
		if (Config == null)
		{
			return;
		}
		foreach (ConfigDefinition key in Config.Keys)
		{
			Config[key].BoxedValue = Config[key].DefaultValue;
		}
		VisionMultiplier = 3f;
		Config.Save();
	}
}
