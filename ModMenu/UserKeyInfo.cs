using System;
using System.Text.Json.Serialization;

namespace ModMenuCrew;

public sealed class UserKeyInfo
{
	[JsonPropertyName("key")]
	public string Key { get; set; } = "";


	[JsonPropertyName("keyType")]
	public string KeyType { get; set; } = "";


	[JsonPropertyName("isPremium")]
	public bool IsPremium { get; set; }

	[JsonPropertyName("expiresAt")]
	public long ExpiresAt { get; set; }

	[JsonPropertyName("createdAt")]
	public long CreatedAt { get; set; }

	[JsonPropertyName("isActive")]
	public bool IsActive { get; set; }

	[JsonPropertyName("isExpired")]
	public bool IsExpired { get; set; }

	[JsonPropertyName("isRevoked")]
	public bool IsRevoked { get; set; }

	public string Preview
	{
		get
		{
			if (string.IsNullOrEmpty(Key) || Key.Length < 8)
			{
				return "****-****-****-****";
			}
			if (Key.Length >= 19)
			{
				return Key.Substring(0, 4) + "-****-****-" + Key.Substring(15, Math.Min(4, Key.Length - 15));
			}
			return Key.Substring(0, 4) + "-****-****-****";
		}
	}
}
