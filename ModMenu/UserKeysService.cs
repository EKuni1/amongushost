using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Il2CppInterop.Runtime.Attributes;

namespace ModMenuCrew;

public static class UserKeysService
{
	private sealed class UserKeysResponse
	{
		[JsonPropertyName("status")]
		public string Status { get; set; } = "";


		[JsonPropertyName("count")]
		public int Count { get; set; }

		[JsonPropertyName("keys")]
		public List<UserKeyInfo> Keys { get; set; } = new List<UserKeyInfo>();


		[JsonPropertyName("message")]
		public string Message { get; set; } = "";

	}

	private static readonly JsonSerializerOptions _opts = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	private static List<UserKeyInfo> _cachedKeys = new List<UserKeyInfo>();

	private static long _lastFetchUnixMs = 0L;

	private const long CACHE_TTL_MS = 30000L;

	public static IReadOnlyList<UserKeyInfo> CachedKeys => _cachedKeys;

	public static long LastFetchAtMs => _lastFetchUnixMs;

	public static bool HasFreshCache
	{
		get
		{
			if (_cachedKeys.Count > 0)
			{
				return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastFetchUnixMs < 30000;
			}
			return false;
		}
	}

	public static void Clear()
	{
		_cachedKeys = new List<UserKeyInfo>();
		_lastFetchUnixMs = 0L;
	}

	[HideFromIl2Cpp]
	public static async Task<(bool success, string message, List<UserKeyInfo> keys)> FetchAsync(CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			if (!DiscordAuthManager.IsLoggedIn)
			{
				return (success: false, message: "Not logged in to Discord.", keys: new List<UserKeyInfo>());
			}
			string identityCredential = DiscordAuthManager.IdentityCredential;
			if (string.IsNullOrEmpty(identityCredential))
			{
				return (success: false, message: "Missing identity token.", keys: new List<UserKeyInfo>());
			}
			string requestUri = "https://api.crewcore.online/api/user_keys";
			using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, requestUri);
			req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", identityCredential);
			req.Content = new StringContent("{}", Encoding.UTF8, "application/json");
			using HttpResponseMessage resp = await GetHttpClient().SendAsync(req, ct);
			string json = await resp.Content.ReadAsStringAsync();
			if (!resp.IsSuccessStatusCode)
			{
				int statusCode = (int)resp.StatusCode;
				return statusCode switch
				{
					401 => (success: false, message: "Session expired. Please re-login with Discord.", keys: new List<UserKeyInfo>()), 
					429 => (success: false, message: "Too many requests. Try again in a minute.", keys: new List<UserKeyInfo>()), 
					_ => (success: false, message: $"Server error ({statusCode}).", keys: new List<UserKeyInfo>()), 
				};
			}
			UserKeysResponse userKeysResponse = JsonSerializer.Deserialize<UserKeysResponse>(json, _opts);
			if (userKeysResponse == null || !"success".Equals(userKeysResponse.Status, StringComparison.OrdinalIgnoreCase))
			{
				return (success: false, message: userKeysResponse?.Message ?? "Invalid response.", keys: new List<UserKeyInfo>());
			}
			_cachedKeys = userKeysResponse.Keys ?? new List<UserKeyInfo>();
			_lastFetchUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			return (success: true, message: "", keys: _cachedKeys);
		}
		catch (OperationCanceledException)
		{
			return (success: false, message: "Request cancelled.", keys: new List<UserKeyInfo>());
		}
		catch (Exception ex2)
		{
			return (success: false, message: "Network error: " + ex2.Message, keys: new List<UserKeyInfo>());
		}
	}

	private static HttpClient GetHttpClient()
	{
		return ModKeyValidator.GetSharedHttpClient();
	}
}
