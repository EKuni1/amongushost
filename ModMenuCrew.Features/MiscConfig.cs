using System;
using BepInEx.Configuration;

namespace ModMenuCrew.Features;

public static class MiscConfig
{
	private const int MAX_OUTFIT_SLOTS = 10;

	public static ConfigEntry<bool> CustomNameEnabled { get; private set; }

	public static ConfigEntry<string> CustomNameText { get; private set; }

	public static ConfigEntry<bool> NameBold { get; private set; }

	public static ConfigEntry<bool> NameItalic { get; private set; }

	public static ConfigEntry<bool> NameUnderline { get; private set; }

	public static ConfigEntry<bool> NameStrikethrough { get; private set; }

	public static ConfigEntry<int> NameSizePercent { get; private set; }

	public static ConfigEntry<int> NameColorMode { get; private set; }

	public static ConfigEntry<string> NameColorHex { get; private set; }

	public static ConfigEntry<string> NameGradientStartHex { get; private set; }

	public static ConfigEntry<string> NameGradientEndHex { get; private set; }

	public static ConfigEntry<string> NamePulseColor1Hex { get; private set; }

	public static ConfigEntry<string> NamePulseColor2Hex { get; private set; }

	public static ConfigEntry<bool> NetworkNameSyncEnabled { get; private set; }

	public static ConfigEntry<string> NamePreset0 { get; private set; }

	public static ConfigEntry<string> NamePreset1 { get; private set; }

	public static ConfigEntry<string> NamePreset2 { get; private set; }

	public static ConfigEntry<string> NamePreset3 { get; private set; }

	public static ConfigEntry<string> NamePreset4 { get; private set; }

	public static ConfigEntry<string>[] OutfitSlots { get; private set; }

	public static ConfigEntry<string>[] OutfitSlotNames { get; private set; }

	public static ConfigEntry<bool> LocalColorOverrideEnabled { get; private set; }

	public static ConfigEntry<int> LocalColorOverrideId { get; private set; }

	public static ConfigEntry<bool> NameColorOverrideEnabled { get; private set; }

	public static ConfigEntry<string> NameColorOverrideHex { get; private set; }

	public static ConfigEntry<bool> HideMyName { get; private set; }

	public static ConfigEntry<bool> HideMyPet { get; private set; }

	public static ConfigEntry<bool> ChatColorEnabled { get; private set; }

	public static ConfigEntry<string> ChatColorHex { get; private set; }

	public static ConfigEntry<bool> FoldoutNameEditor { get; private set; }

	public static ConfigEntry<bool> FoldoutOutfitManager { get; private set; }

	public static ConfigEntry<bool> FoldoutChatColor { get; private set; }

	public static ConfigEntry<bool> FoldoutUtilities { get; private set; }

	public static void Initialize(ConfigFile config)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Expected O, but got Unknown
		CustomNameEnabled = config.Bind<bool>("20. Misc - Name", "Custom Name Enabled", false, "Enable local-only custom name display");
		CustomNameText = config.Bind<string>("20. Misc - Name", "Custom Name Text", "", "The custom name text to display locally");
		NameBold = config.Bind<bool>("20. Misc - Name", "Bold", false, "Apply bold formatting to custom name");
		NameItalic = config.Bind<bool>("20. Misc - Name", "Italic", false, "Apply italic formatting to custom name");
		NameUnderline = config.Bind<bool>("20. Misc - Name", "Underline", false, "Apply underline formatting to custom name");
		NameStrikethrough = config.Bind<bool>("20. Misc - Name", "Strikethrough", false, "Apply strikethrough formatting to custom name");
		NameSizePercent = config.Bind<int>("20. Misc - Name", "Size Percent", 100, new ConfigDescription("Font size as percentage (50-200)", (AcceptableValueBase)(object)new AcceptableValueRange<int>(50, 200), Array.Empty<object>()));
		NameColorMode = config.Bind<int>("20. Misc - Name", "Color Mode", 0, new ConfigDescription("0=None, 1=Single, 2=Gradient, 3=Rainbow, 4=Pulse", (AcceptableValueBase)(object)new AcceptableValueRange<int>(0, 4), Array.Empty<object>()));
		NameColorHex = config.Bind<string>("20. Misc - Name", "Color Hex", "FFFFFF", "Single color HEX (without #)");
		NameGradientStartHex = config.Bind<string>("20. Misc - Name", "Gradient Start", "FF0000", "Gradient start color HEX");
		NameGradientEndHex = config.Bind<string>("20. Misc - Name", "Gradient End", "0000FF", "Gradient end color HEX");
		NamePulseColor1Hex = config.Bind<string>("20. Misc - Name", "Pulse Color 1", "FF0000", "Pulse mode color 1 HEX");
		NamePulseColor2Hex = config.Bind<string>("20. Misc - Name", "Pulse Color 2", "00FF00", "Pulse mode color 2 HEX");
		NetworkNameSyncEnabled = config.Bind<bool>("20. Misc - Name", "Sync Name to All", false, "Send custom name to all players via network (plain text only, goes through game censor)");
		NamePreset0 = config.Bind<string>("21. Misc - Name Presets", "Preset 0", "", "Serialized name preset slot 0");
		NamePreset1 = config.Bind<string>("21. Misc - Name Presets", "Preset 1", "", "Serialized name preset slot 1");
		NamePreset2 = config.Bind<string>("21. Misc - Name Presets", "Preset 2", "", "Serialized name preset slot 2");
		NamePreset3 = config.Bind<string>("21. Misc - Name Presets", "Preset 3", "", "Serialized name preset slot 3");
		NamePreset4 = config.Bind<string>("21. Misc - Name Presets", "Preset 4", "", "Serialized name preset slot 4");
		OutfitSlots = new ConfigEntry<string>[10];
		OutfitSlotNames = new ConfigEntry<string>[10];
		for (int i = 0; i < 10; i++)
		{
			OutfitSlots[i] = config.Bind<string>("22. Misc - Outfits", $"Outfit Slot {i}", "", $"Serialized outfit data for slot {i} (ColorId|HatId|SkinId|VisorId|PetId|NamePlateId)");
			OutfitSlotNames[i] = config.Bind<string>("22. Misc - Outfits", $"Outfit Slot {i} Name", "", $"Display name for outfit slot {i}");
		}
		LocalColorOverrideEnabled = config.Bind<bool>("23. Misc - Overrides", "Local Color Override", false, "Override local player body color visually");
		LocalColorOverrideId = config.Bind<int>("23. Misc - Overrides", "Local Color ID", 0, new ConfigDescription("Color ID to use for local override", (AcceptableValueBase)(object)new AcceptableValueRange<int>(0, 17), Array.Empty<object>()));
		NameColorOverrideEnabled = config.Bind<bool>("23. Misc - Overrides", "Name Color Override", false, "Override name text color above your head");
		NameColorOverrideHex = config.Bind<string>("23. Misc - Overrides", "Name Color Hex", "FFFFFF", "HEX color for name text override");
		HideMyName = config.Bind<bool>("23. Misc - Overrides", "Hide My Name", false, "Hide your name above your character locally");
		HideMyPet = config.Bind<bool>("23. Misc - Overrides", "Hide My Pet", false, "Hide your pet locally");
		ChatColorEnabled = config.Bind<bool>("25. Misc - Chat Color", "Enabled", false, "Recolor your own chat messages locally (other players still see the default color)");
		ChatColorHex = config.Bind<string>("25. Misc - Chat Color", "Color Hex", "FFD700", "HEX color (without #) for your own messages in your own chat view");
		FoldoutNameEditor = config.Bind<bool>("24. Misc - UI", "Foldout Name Editor", true, "Name Editor section expanded");
		FoldoutOutfitManager = config.Bind<bool>("24. Misc - UI", "Foldout Outfit Manager", true, "Outfit Manager section expanded");
		FoldoutChatColor = config.Bind<bool>("24. Misc - UI", "Foldout Chat Color", true, "Chat Color section expanded");
		FoldoutUtilities = config.Bind<bool>("24. Misc - UI", "Foldout Utilities", true, "Utilities section expanded");
	}

	public static string SerializeOutfit(int colorId, string hatId, string skinId, string visorId, string petId, string namePlateId)
	{
		return $"{colorId}|{hatId}|{skinId}|{visorId}|{petId}|{namePlateId}";
	}

	public static bool TryDeserializeOutfit(string data, out int colorId, out string hatId, out string skinId, out string visorId, out string petId, out string namePlateId)
	{
		colorId = 0;
		hatId = "";
		skinId = "";
		visorId = "";
		petId = "";
		namePlateId = "";
		if (string.IsNullOrEmpty(data))
		{
			return false;
		}
		string[] array = data.Split('|');
		if (array.Length < 6)
		{
			return false;
		}
		if (!int.TryParse(array[0], out colorId))
		{
			return false;
		}
		hatId = array[1];
		skinId = array[2];
		visorId = array[3];
		petId = array[4];
		namePlateId = array[5];
		return true;
	}

	public static string SerializeNamePreset(string name, bool bold, bool italic, bool underline, bool strikethrough, int sizePercent, int colorMode, string colorHex, string gradStart, string gradEnd, string pulse1, string pulse2)
	{
		return $"{name}§{bold}§{italic}§{underline}§{strikethrough}§{sizePercent}§{colorMode}§{colorHex}§{gradStart}§{gradEnd}§{pulse1}§{pulse2}";
	}

	public static bool TryDeserializeNamePreset(string data, out string name, out bool bold, out bool italic, out bool underline, out bool strikethrough, out int sizePercent, out int colorMode, out string colorHex, out string gradStart, out string gradEnd, out string pulse1, out string pulse2)
	{
		name = "";
		bold = false;
		italic = false;
		underline = false;
		strikethrough = false;
		sizePercent = 100;
		colorMode = 0;
		colorHex = "FFFFFF";
		gradStart = "FF0000";
		gradEnd = "0000FF";
		pulse1 = "FF0000";
		pulse2 = "00FF00";
		if (string.IsNullOrEmpty(data))
		{
			return false;
		}
		string[] array = data.Split('§');
		if (array.Length < 12)
		{
			return false;
		}
		name = array[0];
		bold = array[1] == "1";
		italic = array[2] == "1";
		underline = array[3] == "1";
		strikethrough = array[4] == "1";
		if (!int.TryParse(array[5], out sizePercent))
		{
			sizePercent = 100;
		}
		if (!int.TryParse(array[6], out colorMode))
		{
			colorMode = 0;
		}
		colorHex = array[7];
		gradStart = array[8];
		gradEnd = array[9];
		pulse1 = array[10];
		pulse2 = array[11];
		return true;
	}

	public static ConfigEntry<string> GetNamePresetSlot(int index)
	{
		return (ConfigEntry<string>)(index switch
		{
			0 => NamePreset0, 
			1 => NamePreset1, 
			2 => NamePreset2, 
			3 => NamePreset3, 
			4 => NamePreset4, 
			_ => null, 
		});
	}
}
