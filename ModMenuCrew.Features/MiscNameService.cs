using System.Text;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;

namespace ModMenuCrew.Features;

public static class MiscNameService
{
	private static readonly StringBuilder _sb = new StringBuilder(256);

	private static readonly StringBuilder _previewSb = new StringBuilder(256);

	private static string _cachedFormattedName = null;

	private static bool _cacheValid = false;

	private static bool _wasCustomNameEnabled = false;

	private static string _cachedPreview;

	private static int _cachedPreviewKey;

	private static float _cachedPreviewTime;

	private static int _cachedPreviewFrame = -1;

	private const float ANIMATED_PREVIEW_INTERVAL = 0.033f;

	private static bool _wasNameColorOverrideOn;

	private static bool _wasHideNameOn;

	private static bool _wasHidePetOn;

	private static bool _wasColorOverrideOn;

	private static readonly char[] HexDigits = "0123456789ABCDEF".ToCharArray();

	public static void InvalidateCache()
	{
		_cacheValid = false;
		_cachedFormattedName = null;
		_cachedPreview = null;
		_cachedPreviewKey = 0;
	}

	public static void UpdateLocalName()
	{
		try
		{
			if ((Object)(object)PlayerControl.LocalPlayer == (Object)null || (Object)(object)PlayerControl.LocalPlayer.cosmetics == (Object)null || (Object)(object)PlayerControl.LocalPlayer.cosmetics.nameText == (Object)null)
			{
				return;
			}
			TextMeshPro nameText = PlayerControl.LocalPlayer.cosmetics.nameText;
			bool flag = MiscConfig.CustomNameEnabled?.Value ?? false;
			if (GameCheats.IsRevealSusActive)
			{
				_wasCustomNameEnabled = flag;
				ApplyLocalOverrides();
				return;
			}
			if (!flag)
			{
				if (_wasCustomNameEnabled)
				{
					NetworkedPlayerInfo data = PlayerControl.LocalPlayer.Data;
					string text = ((data != null) ? data.PlayerName : null) ?? "???";
					((TMP_Text)nameText).text = text;
					_wasCustomNameEnabled = false;
				}
				ApplyLocalOverrides();
				return;
			}
			_wasCustomNameEnabled = true;
			string text2 = BuildFormattedName();
			if (text2 == null)
			{
				return;
			}
			string text3;
			if (MMCIdentification.Enabled)
			{
				ConfigEntry<bool> hideMMCStar = CheatConfig.HideMMCStar;
				if ((hideMMCStar == null || !hideMMCStar.Value) && MMCIdentification.IsMMCPlayer(PlayerControl.LocalPlayer.PlayerId))
				{
					text3 = "<color=#FFD700>★</color> " + text2;
					goto IL_0111;
				}
			}
			text3 = text2;
			goto IL_0111;
			IL_0111:
			string text4 = text3;
			if (Object.op_Implicit((Object)(object)LobbyBehaviour.Instance))
			{
				ConfigEntry<bool> showPlayerInfo = CheatConfig.ShowPlayerInfo;
				if (showPlayerInfo != null && showPlayerInfo.Value)
				{
					string text5 = PlayerInfoDisplay.BuildInfoSuffix(PlayerControl.LocalPlayer);
					if (text5 != null)
					{
						text4 += text5;
					}
				}
			}
			string text6 = GameCheats.BuildKillCooldownSuffix(PlayerControl.LocalPlayer);
			if (text6 != null)
			{
				text4 += text6;
			}
			if (((TMP_Text)nameText).text != text4)
			{
				((TMP_Text)nameText).text = text4;
			}
			ApplyLocalOverrides();
		}
		catch
		{
		}
	}

	private static void ApplyLocalOverrides()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			if ((Object)(object)localPlayer == (Object)null || (Object)(object)localPlayer.cosmetics == (Object)null)
			{
				return;
			}
			ConfigEntry<bool> nameColorOverrideEnabled = MiscConfig.NameColorOverrideEnabled;
			int num;
			if (nameColorOverrideEnabled == null)
			{
				num = 0;
			}
			else
			{
				num = (nameColorOverrideEnabled.Value ? 1 : 0);
				if (num != 0)
				{
					string text = MiscConfig.NameColorOverrideHex?.Value ?? "FFFFFF";
					Color nameColor = default(Color);
					if (ColorUtility.TryParseHtmlString("#" + text, ref nameColor))
					{
						localPlayer.cosmetics.SetNameColor(nameColor);
					}
					goto IL_008a;
				}
			}
			if (_wasNameColorOverrideOn)
			{
				localPlayer.cosmetics.SetNameColor(Color.white);
			}
			goto IL_008a;
			IL_00c4:
			int num2;
			_wasHideNameOn = (byte)num2 != 0;
			ConfigEntry<bool> hideMyPet = MiscConfig.HideMyPet;
			int num3;
			if (hideMyPet == null)
			{
				num3 = 0;
			}
			else
			{
				num3 = (hideMyPet.Value ? 1 : 0);
				if (num3 != 0)
				{
					localPlayer.cosmetics.SetPetVisible(false);
					goto IL_00fe;
				}
			}
			if (_wasHidePetOn)
			{
				localPlayer.cosmetics.SetPetVisible(true);
			}
			goto IL_00fe;
			IL_00fe:
			_wasHidePetOn = (byte)num3 != 0;
			bool flag = MiscConfig.LocalColorOverrideEnabled?.Value ?? false;
			if (flag)
			{
				int num4 = MiscConfig.LocalColorOverrideId?.Value ?? 0;
				if ((((Object)(object)localPlayer.Data != (Object)null) ? localPlayer.Data.DefaultOutfit.ColorId : (-1)) != num4 && !_wasColorOverrideOn)
				{
					try
					{
						localPlayer.CmdCheckColor((byte)num4);
					}
					catch
					{
					}
				}
				localPlayer.cosmetics.SetColor(num4);
			}
			else if (_wasColorOverrideOn)
			{
				int color = (((Object)(object)localPlayer.Data != (Object)null) ? localPlayer.Data.DefaultOutfit.ColorId : 0);
				localPlayer.cosmetics.SetColor(color);
			}
			_wasColorOverrideOn = flag;
			return;
			IL_008a:
			_wasNameColorOverrideOn = (byte)num != 0;
			ConfigEntry<bool> hideMyName = MiscConfig.HideMyName;
			if (hideMyName == null)
			{
				num2 = 0;
			}
			else
			{
				num2 = (hideMyName.Value ? 1 : 0);
				if (num2 != 0)
				{
					localPlayer.cosmetics.ToggleNameVisible(false);
					goto IL_00c4;
				}
			}
			if (_wasHideNameOn)
			{
				localPlayer.cosmetics.ToggleNameVisible(true);
			}
			goto IL_00c4;
		}
		catch
		{
		}
	}

	private static string BuildFormattedName()
	{
		string text = MiscConfig.CustomNameText?.Value;
		if (string.IsNullOrEmpty(text))
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			object obj;
			if (localPlayer == null)
			{
				obj = null;
			}
			else
			{
				NetworkedPlayerInfo data = localPlayer.Data;
				obj = ((data != null) ? data.PlayerName : null);
			}
			if (obj == null)
			{
				obj = "Player";
			}
			text = (string)obj;
		}
		int num = MiscConfig.NameColorMode?.Value ?? 0;
		bool flag = num == 3 || num == 4;
		if (!flag && _cacheValid && _cachedFormattedName != null)
		{
			return _cachedFormattedName;
		}
		string text2 = BuildRichText(text, num);
		if (!flag)
		{
			_cachedFormattedName = text2;
			_cacheValid = true;
		}
		return text2;
	}

	private static string BuildRichText(string rawName, int colorMode)
	{
		return BuildRichTextInto(_sb, rawName, colorMode);
	}

	private static string BuildRichTextInto(StringBuilder sb, string rawName, int colorMode, bool includeSizeTag = true)
	{
		sb.Clear();
		bool flag = MiscConfig.NameBold?.Value ?? false;
		bool flag2 = MiscConfig.NameItalic?.Value ?? false;
		bool flag3 = MiscConfig.NameUnderline?.Value ?? false;
		bool flag4 = MiscConfig.NameStrikethrough?.Value ?? false;
		int num = MiscConfig.NameSizePercent?.Value ?? 100;
		if (includeSizeTag && num != 100)
		{
			sb.Append("<size=").Append(num).Append("%>");
		}
		if (flag)
		{
			sb.Append("<b>");
		}
		if (flag2)
		{
			sb.Append("<i>");
		}
		if (flag3)
		{
			sb.Append("<u>");
		}
		if (flag4)
		{
			sb.Append("<s>");
		}
		switch (colorMode)
		{
		case 0:
			sb.Append(rawName);
			break;
		case 1:
		{
			string value = MiscConfig.NameColorHex?.Value ?? "FFFFFF";
			sb.Append("<color=#").Append(value).Append('>')
				.Append(rawName)
				.Append("</color>");
			break;
		}
		case 2:
			AppendGradient(sb, rawName);
			break;
		case 3:
			AppendRainbow(sb, rawName);
			break;
		case 4:
			AppendPulse(sb, rawName);
			break;
		default:
			sb.Append(rawName);
			break;
		}
		if (flag4)
		{
			sb.Append("</s>");
		}
		if (flag3)
		{
			sb.Append("</u>");
		}
		if (flag2)
		{
			sb.Append("</i>");
		}
		if (flag)
		{
			sb.Append("</b>");
		}
		if (includeSizeTag && num != 100)
		{
			sb.Append("</size>");
		}
		return sb.ToString();
	}

	private static void AppendGradient(StringBuilder sb, string text)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		string text2 = MiscConfig.NameGradientStartHex?.Value ?? "FF0000";
		string text3 = MiscConfig.NameGradientEndHex?.Value ?? "0000FF";
		Color red = default(Color);
		if (!ColorUtility.TryParseHtmlString("#" + text2, ref red))
		{
			red = Color.red;
		}
		Color blue = default(Color);
		if (!ColorUtility.TryParseHtmlString("#" + text3, ref blue))
		{
			blue = Color.blue;
		}
		int length = text.Length;
		for (int i = 0; i < length; i++)
		{
			float num = ((length > 1) ? ((float)i / (float)(length - 1)) : 0f);
			Color c = Color.Lerp(red, blue, num);
			AppendHexRGB(sb, c);
			sb.Append('>').Append(text[i]).Append("</color>");
		}
	}

	private static void AppendRainbow(StringBuilder sb, string text)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time * 0.5f;
		int length = text.Length;
		float num2 = 1f / (float)Mathf.Max(length, 1);
		for (int i = 0; i < length; i++)
		{
			Color c = Color.HSVToRGB((num + (float)i * num2) % 1f, 1f, 1f);
			AppendHexRGB(sb, c);
			sb.Append('>').Append(text[i]).Append("</color>");
		}
	}

	private static void AppendPulse(StringBuilder sb, string text)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		string text2 = MiscConfig.NamePulseColor1Hex?.Value ?? "FF0000";
		string text3 = MiscConfig.NamePulseColor2Hex?.Value ?? "00FF00";
		Color red = default(Color);
		if (!ColorUtility.TryParseHtmlString("#" + text2, ref red))
		{
			red = Color.red;
		}
		Color green = default(Color);
		if (!ColorUtility.TryParseHtmlString("#" + text3, ref green))
		{
			green = Color.green;
		}
		float num = (Mathf.Sin(Time.time * 2f) + 1f) * 0.5f;
		Color c = Color.Lerp(red, green, num);
		AppendHexRGB(sb, c);
		sb.Append('>').Append(text).Append("</color>");
	}

	private static void AppendHexRGB(StringBuilder sb, Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Clamp(Mathf.RoundToInt(c.r * 255f), 0, 255);
		int num2 = Mathf.Clamp(Mathf.RoundToInt(c.g * 255f), 0, 255);
		int num3 = Mathf.Clamp(Mathf.RoundToInt(c.b * 255f), 0, 255);
		sb.Append("<color=#");
		sb.Append(HexDigits[(num >> 4) & 0xF]).Append(HexDigits[num & 0xF]);
		sb.Append(HexDigits[(num2 >> 4) & 0xF]).Append(HexDigits[num2 & 0xF]);
		sb.Append(HexDigits[(num3 >> 4) & 0xF]).Append(HexDigits[num3 & 0xF]);
	}

	public static void CleanupAll()
	{
		try
		{
			_wasCustomNameEnabled = false;
			_cacheValid = false;
			_cachedFormattedName = null;
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			object obj;
			if (localPlayer == null)
			{
				obj = null;
			}
			else
			{
				CosmeticsLayer cosmetics = localPlayer.cosmetics;
				obj = ((cosmetics != null) ? cosmetics.nameText : null);
			}
			if ((Object)obj != (Object)null && (Object)(object)localPlayer.Data != (Object)null)
			{
				((TMP_Text)localPlayer.cosmetics.nameText).text = localPlayer.Data.PlayerName;
			}
		}
		catch
		{
		}
	}

	public static string GetPreviewString()
	{
		try
		{
			string text = MiscConfig.CustomNameText?.Value;
			if (string.IsNullOrEmpty(text))
			{
				PlayerControl localPlayer = PlayerControl.LocalPlayer;
				object obj;
				if (localPlayer == null)
				{
					obj = null;
				}
				else
				{
					NetworkedPlayerInfo data = localPlayer.Data;
					obj = ((data != null) ? data.PlayerName : null);
				}
				if (obj == null)
				{
					obj = "Player";
				}
				text = (string)obj;
			}
			int num = MiscConfig.NameColorMode?.Value ?? 0;
			bool flag = num == 3 || num == 4;
			int frameCount = Time.frameCount;
			if (_cachedPreview != null && _cachedPreviewFrame == frameCount)
			{
				return _cachedPreview;
			}
			int num2 = ComputePreviewKey(text, num);
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (_cachedPreview != null && _cachedPreviewKey == num2)
			{
				if (!flag)
				{
					_cachedPreviewFrame = frameCount;
					return _cachedPreview;
				}
				if (realtimeSinceStartup - _cachedPreviewTime < 0.033f)
				{
					_cachedPreviewFrame = frameCount;
					return _cachedPreview;
				}
			}
			_cachedPreview = BuildRichTextInto(_previewSb, text, num, includeSizeTag: false);
			_cachedPreviewKey = num2;
			_cachedPreviewTime = realtimeSinceStartup;
			_cachedPreviewFrame = frameCount;
			return _cachedPreview;
		}
		catch
		{
			return "Preview Error";
		}
	}

	private static int ComputePreviewKey(string rawName, int colorMode)
	{
		int num = rawName?.GetHashCode() ?? 0;
		num = num * 31 + colorMode;
		num = num * 31 + ((MiscConfig.NameBold?.Value ?? false) ? 1 : 0);
		num = num * 31 + ((MiscConfig.NameItalic?.Value ?? false) ? 1 : 0);
		num = num * 31 + ((MiscConfig.NameUnderline?.Value ?? false) ? 1 : 0);
		num = num * 31 + ((MiscConfig.NameStrikethrough?.Value ?? false) ? 1 : 0);
		num = num * 31 + (MiscConfig.NameSizePercent?.Value ?? 100);
		switch (colorMode)
		{
		case 1:
			num = num * 31 + (MiscConfig.NameColorHex?.Value?.GetHashCode()).GetValueOrDefault();
			break;
		case 2:
			num = num * 31 + (MiscConfig.NameGradientStartHex?.Value?.GetHashCode()).GetValueOrDefault();
			num = num * 31 + (MiscConfig.NameGradientEndHex?.Value?.GetHashCode()).GetValueOrDefault();
			break;
		case 4:
			num = num * 31 + (MiscConfig.NamePulseColor1Hex?.Value?.GetHashCode()).GetValueOrDefault();
			num = num * 31 + (MiscConfig.NamePulseColor2Hex?.Value?.GetHashCode()).GetValueOrDefault();
			break;
		}
		return num;
	}
}
