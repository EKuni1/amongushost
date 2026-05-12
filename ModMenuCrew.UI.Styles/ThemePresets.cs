using UnityEngine;

namespace ModMenuCrew.UI.Styles;

public static class ThemePresets
{
	public struct ThemeColors
	{
		public Color Accent;

		public Color AccentSoft;

		public Color AccentDim;

		public Color AccentHover;

		public Color AccentActive;

		public Color BgDarkA;

		public Color BgDarkB;

		public Color BgSection;

		public Color BgCard;

		public Color Glow;

		public Color GlowStrong;
	}

	public const int COUNT = 6;

	public static readonly string[] Names = new string[6] { "Red Void", "Phantom Blue", "Venom Green", "Eclipse Purple", "Amber", "Frost" };

	private static readonly ThemeColors[] Presets = new ThemeColors[6]
	{
		new ThemeColors
		{
			Accent = new Color(1f, 0.13f, 0.25f, 1f),
			AccentSoft = new Color(1f, 0.13f, 0.25f, 0.2f),
			AccentDim = new Color(1f, 0.13f, 0.25f, 0.08f),
			AccentHover = new Color(1f, 0.2f, 0.32f, 0.9f),
			AccentActive = new Color(1f, 0.08f, 0.2f, 1f),
			BgDarkA = new Color(0.05f, 0.05f, 0.07f, 0.97f),
			BgDarkB = new Color(0.07f, 0.08f, 0.1f, 0.97f),
			BgSection = new Color(0.1f, 0.11f, 0.14f, 0.94f),
			BgCard = new Color(0.14f, 0.15f, 0.19f, 0.9f),
			Glow = new Color(1f, 0.13f, 0.25f, 0.18f),
			GlowStrong = new Color(1f, 0.13f, 0.25f, 0.35f)
		},
		new ThemeColors
		{
			Accent = new Color(0.27f, 0.67f, 0.95f, 1f),
			AccentSoft = new Color(0.27f, 0.67f, 0.95f, 0.2f),
			AccentDim = new Color(0.27f, 0.67f, 0.95f, 0.08f),
			AccentHover = new Color(0.35f, 0.73f, 1f, 0.9f),
			AccentActive = new Color(0.2f, 0.55f, 0.85f, 1f),
			BgDarkA = new Color(0.04f, 0.05f, 0.08f, 0.97f),
			BgDarkB = new Color(0.06f, 0.07f, 0.11f, 0.97f),
			BgSection = new Color(0.09f, 0.1f, 0.15f, 0.94f),
			BgCard = new Color(0.12f, 0.14f, 0.2f, 0.9f),
			Glow = new Color(0.27f, 0.67f, 0.95f, 0.18f),
			GlowStrong = new Color(0.27f, 0.67f, 0.95f, 0.35f)
		},
		new ThemeColors
		{
			Accent = new Color(0.18f, 0.8f, 0.44f, 1f),
			AccentSoft = new Color(0.18f, 0.8f, 0.44f, 0.2f),
			AccentDim = new Color(0.18f, 0.8f, 0.44f, 0.08f),
			AccentHover = new Color(0.25f, 0.88f, 0.52f, 0.9f),
			AccentActive = new Color(0.13f, 0.68f, 0.36f, 1f),
			BgDarkA = new Color(0.04f, 0.06f, 0.04f, 0.97f),
			BgDarkB = new Color(0.06f, 0.08f, 0.06f, 0.97f),
			BgSection = new Color(0.09f, 0.12f, 0.09f, 0.94f),
			BgCard = new Color(0.12f, 0.16f, 0.12f, 0.9f),
			Glow = new Color(0.18f, 0.8f, 0.44f, 0.18f),
			GlowStrong = new Color(0.18f, 0.8f, 0.44f, 0.35f)
		},
		new ThemeColors
		{
			Accent = new Color(0.66f, 0.33f, 0.97f, 1f),
			AccentSoft = new Color(0.66f, 0.33f, 0.97f, 0.2f),
			AccentDim = new Color(0.66f, 0.33f, 0.97f, 0.08f),
			AccentHover = new Color(0.73f, 0.42f, 1f, 0.9f),
			AccentActive = new Color(0.55f, 0.25f, 0.85f, 1f),
			BgDarkA = new Color(0.05f, 0.04f, 0.08f, 0.97f),
			BgDarkB = new Color(0.07f, 0.06f, 0.12f, 0.97f),
			BgSection = new Color(0.1f, 0.09f, 0.16f, 0.94f),
			BgCard = new Color(0.14f, 0.12f, 0.21f, 0.9f),
			Glow = new Color(0.66f, 0.33f, 0.97f, 0.18f),
			GlowStrong = new Color(0.66f, 0.33f, 0.97f, 0.35f)
		},
		new ThemeColors
		{
			Accent = new Color(0.96f, 0.62f, 0.04f, 1f),
			AccentSoft = new Color(0.96f, 0.62f, 0.04f, 0.2f),
			AccentDim = new Color(0.96f, 0.62f, 0.04f, 0.08f),
			AccentHover = new Color(1f, 0.72f, 0.15f, 0.9f),
			AccentActive = new Color(0.85f, 0.52f, 0f, 1f),
			BgDarkA = new Color(0.06f, 0.05f, 0.04f, 0.97f),
			BgDarkB = new Color(0.08f, 0.07f, 0.05f, 0.97f),
			BgSection = new Color(0.12f, 0.1f, 0.07f, 0.94f),
			BgCard = new Color(0.16f, 0.14f, 0.1f, 0.9f),
			Glow = new Color(0.96f, 0.62f, 0.04f, 0.18f),
			GlowStrong = new Color(0.96f, 0.62f, 0.04f, 0.35f)
		},
		new ThemeColors
		{
			Accent = new Color(0.02f, 0.71f, 0.83f, 1f),
			AccentSoft = new Color(0.02f, 0.71f, 0.83f, 0.2f),
			AccentDim = new Color(0.02f, 0.71f, 0.83f, 0.08f),
			AccentHover = new Color(0.1f, 0.8f, 0.92f, 0.9f),
			AccentActive = new Color(0f, 0.6f, 0.72f, 1f),
			BgDarkA = new Color(0.04f, 0.05f, 0.07f, 0.97f),
			BgDarkB = new Color(0.05f, 0.07f, 0.09f, 0.97f),
			BgSection = new Color(0.08f, 0.1f, 0.13f, 0.94f),
			BgCard = new Color(0.11f, 0.14f, 0.17f, 0.9f),
			Glow = new Color(0.02f, 0.71f, 0.83f, 0.18f),
			GlowStrong = new Color(0.02f, 0.71f, 0.83f, 0.35f)
		}
	};

	public static int ActiveIndex { get; private set; } = 0;


	public static void Apply(int index)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		if (index < 0 || index >= 6)
		{
			index = 0;
		}
		ActiveIndex = index;
		ThemeColors themeColors = Presets[index];
		GuiStyles.Theme.Primary = themeColors.Accent;
		GuiStyles.Theme.Accent = themeColors.Accent;
		GuiStyles.Theme.AccentSoft = themeColors.AccentSoft;
		GuiStyles.Theme.AccentDim = themeColors.AccentDim;
		GuiStyles.Theme.AccentHover = themeColors.AccentHover;
		GuiStyles.Theme.AccentActive = themeColors.AccentActive;
		GuiStyles.Theme.Secondary = themeColors.BgDarkB;
		GuiStyles.Theme.BgDarkA = themeColors.BgDarkA;
		GuiStyles.Theme.BgDarkB = themeColors.BgDarkB;
		GuiStyles.Theme.BgSection = themeColors.BgSection;
		GuiStyles.Theme.BgCard = themeColors.BgCard;
		GuiStyles.Theme.Glow = themeColors.Glow;
		GuiStyles.Theme.GlowStrong = themeColors.GlowStrong;
		GuiStyles.Theme.HeaderTop = themeColors.BgDarkB;
		GuiStyles.Theme.HeaderBottom = themeColors.BgDarkB;
		GuiStyles.Theme.TextAccent = new Color(themeColors.Accent.r, Mathf.Min(1f, themeColors.Accent.g + 0.32f), Mathf.Min(1f, themeColors.Accent.b + 0.25f), 1f);
		GuiStyles.Theme.GlowCyan = new Color(themeColors.Accent.r * 0.3f, themeColors.Accent.g * 0.7f + 0.3f, themeColors.Accent.b * 0.5f + 0.5f, 0.18f);
		GuiStyles.InvalidateAllStyles();
	}
}
