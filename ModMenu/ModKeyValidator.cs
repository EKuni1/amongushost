using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppSystem;
using Microsoft.Win32;
using ModMenuCrew.Networking;
using ModMenuCrew.UI.Managers;
using ModMenuCrew.UI.Menus;
using UnityEngine;

namespace ModMenuCrew;

public static class ModKeyValidator
{
	private sealed class RateLimitedException : Exception
	{
		public TimeSpan RetryAfter { get; }

		public RateLimitedException(TimeSpan retryAfter)
			: base("Rate limited")
		{
			RetryAfter = retryAfter;
		}
	}

	private static readonly JsonSerializerOptions _jsonOpts;

	internal static readonly Regex KeyFormatRegex;

	internal static readonly string[] TrustedRootCAThumbprints;

	internal static readonly string[] AllowedHostnames;

	internal static readonly bool IsRunningUnderWine;

	internal static volatile string _sslDiagLog;

	private static readonly HttpClientHandler httpHandler;

	private static readonly HttpClientHandler httpHandlerPinned;

	private static readonly HttpClient httpClient;

	internal static readonly string _rsaModulusPart1;

	internal static readonly string _rsaModulusPart2;

	internal static readonly string _rsaModulusPart3;

	private static readonly string _rsaExponent;

	private static readonly (PropertyInfo prop, string jsonName)[] _rsaCachedProps;

	private static readonly JsonSerializerOptions _rsaJsonOpts;

	private static uint _textureMemoryPool;

	private static readonly string ApiBaseUrl;

	private const string DisplayUrl = "https://crewcore.online/";

	private const string PlayerPrefsKeyActivatedPrefix = "ModMenuCrew_Activated_";

	private static string _cachedHwid;

	private static bool __svc_internal;

	private static int __svc_checksum;

	private static readonly object _svcLock;

	private static int _svcMismatchCount;

	private const int SVC_MISMATCH_TOLERANCE = 10;

	private static string _srvProof;

	private static long _proofSeedXor;

	private static long _proofExpiresXor;

	private const long PROOF_SENTINEL = 9173317376565600345L;

	private const long PROOF_REL_SENTINEL = 3215389256933277750L;

	private static long _proofReceivedUnscaledXor;

	private static long _proofTtlMsXor;

	private const long PROOF_TTL_MAX_MS = 600000L;

	private static volatile string _serverHeartbeatNonce;

	private static int _integrityCounter;

	private static long _serverTimeOffsetXor;

	private const long OFFSET_SENTINEL = 5588521007158401630L;

	private static readonly Stopwatch _monoStopwatch;

	private static long _lastWallCheckMs;

	private static long _lastMonoCheckMs;

	private const long CLOCK_JUMP_THRESHOLD_MS = 30000L;

	private static long _lastJumpWakeMs;

	private const long JUMP_WAKE_COOLDOWN_MS = 5000L;

	internal static long _clockDriftClampCount;

	private static long _rtx;

	private static int _vcs;

	private static byte _xf;

	private static uint _hv;

	private static long _lastServerCheck;

	private static readonly object _validationLock;

	private static bool _debugSessionDeathLogged;

	internal static volatile string _debugHeartbeatMsg;

	private static long _localTick;

	private static long _lastTickUpdateMs;

	private static long _serverExpectedTick;

	private static long _serverTickTolerance;

	private static readonly Regex _queryKeyRegex;

	private static readonly Regex _keyLikeRegex;

	private static readonly Regex _jsonSecretRegex;

	private static int _isHeartbeatRunningInt;

	private static CancellationTokenSource _heartbeatCancellation;

	private static CancellationTokenSource _sleepTokenSource;

	private static bool _enableHeartbeat;

	internal static string CachedHwid => _cachedHwid ?? "";

	public static long SessionToken { get; private set; }

	public static bool _svcCtx
	{
		get
		{
			lock (_svcLock)
			{
				int num = (__svc_internal ? 23130 : 42405) ^ (int)(SessionToken & 0xFFFF) ^ (_integrityCounter * 4919);
				if (__svc_checksum != num)
				{
					_svcMismatchCount++;
					if (_svcMismatchCount >= 10)
					{
						__svc_internal = false;
						Interlocked.Exchange(ref _svcMismatchCount, 0);
						return false;
					}
					return __svc_internal;
				}
				Interlocked.Exchange(ref _svcMismatchCount, 0);
				return __svc_internal;
			}
		}
		private set
		{
			lock (_svcLock)
			{
				__svc_internal = value;
				__svc_checksum = (value ? 23130 : 42405) ^ (int)(SessionToken & 0xFFFF) ^ (_integrityCounter * 4919);
				Interlocked.Exchange(ref _svcMismatchCount, 0);
			}
		}
	}

	public static string LastValidationMessage { get; private set; }

	public static string ValidatedUsername { get; private set; }

	public static string CurrentKey { get; private set; }

	public static bool PendingResetRequest { get; private set; }

	private static long ProofSeed => _proofSeedXor ^ 0x7F4E2B8D3A1C6059L;

	private static long ProofExpires => _proofExpiresXor ^ 0x7F4E2B8D3A1C6059L;

	private static long ProofReceivedUnscaledMs => _proofReceivedUnscaledXor ^ 0x2C9F5B7E4A1D8036L;

	private static long ProofTtlMs => _proofTtlMsXor ^ 0x2C9F5B7E4A1D8036L;

	private static bool ProofHasRelativeTracking
	{
		get
		{
			if (_proofReceivedUnscaledXor != 3215389256933277750L)
			{
				return _proofTtlMsXor != 3215389256933277750L;
			}
			return false;
		}
	}

	internal static long ServerTimeOffsetMs => _serverTimeOffsetXor ^ 0x4D8E6A3F2B7C1A5EL;

	internal static long CachedFrameTimeMs { get; private set; }

	internal static long MonotonicMs => _monoStopwatch.ElapsedMilliseconds;

	internal static long ClockDriftClampCount => _clockDriftClampCount;

	public static string FormattedDisplayString { get; private set; }

	public static bool IsPremium
	{
		get
		{
			if (SessionToken == 0L)
			{
				return false;
			}
			if (!_svcCtx)
			{
				return false;
			}
			if (!V())
			{
				return false;
			}
			if (ServerData.PremiumFeatures == null || ServerData.PremiumFeatures.Count == 0)
			{
				return false;
			}
			long num = ServerData.CalculateIntegrity();
			long storedIntegrityHash = ServerData.GetStoredIntegrityHash();
			if (num != storedIntegrityHash)
			{
				ServerData.TriggerSilentDenial();
				return false;
			}
			return true;
		}
	}

	public static string KeyDisplay { get; private set; }

	public static string TimeDisplay { get; private set; }

	public static long CurrentSessionToken { get; private set; }

	public static long LocalTick => _localTick;

	internal static bool _isHeartbeatRunning => _isHeartbeatRunningInt != 0;

	public static bool DiscordRevoked { get; internal set; }

	public static string RevokeReason { get; internal set; }

	private static bool DetectWine()
	{
		try
		{
			if (GetProcAddress(GetModuleHandle("ntdll.dll"), "wine_get_version") != IntPtr.Zero)
			{
				return true;
			}
			string? environmentVariable = Environment.GetEnvironmentVariable("WINEPREFIX");
			string environmentVariable2 = Environment.GetEnvironmentVariable("CX_BOTTLE");
			if (!string.IsNullOrEmpty(environmentVariable) || !string.IsNullOrEmpty(environmentVariable2))
			{
				return true;
			}
			if (Directory.Exists("Z:\\") && (Directory.Exists("Z:\\usr") || Directory.Exists("Z:\\Applications")))
			{
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

	internal static bool UnifiedSslCallback(HttpRequestMessage request, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors, bool runSpkiPin)
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(31, 3, stringBuilder2);
				handler.AppendLiteral("[SSL-DIAG] Wine=");
				handler.AppendFormatted(IsRunningUnderWine);
				handler.AppendLiteral(" errors=");
				handler.AppendFormatted(errors);
				handler.AppendLiteral(" (raw=");
				handler.AppendFormatted((int)errors);
				handler.AppendLiteral(")");
				stringBuilder3.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder2);
				handler.AppendLiteral("  URI: ");
				handler.AppendFormatted(request?.RequestUri);
				stringBuilder4.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder5 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(17, 2, stringBuilder2);
				handler.AppendLiteral("  Cert: ");
				handler.AppendFormatted(cert?.Subject ?? "NULL");
				handler.AppendLiteral(" Issuer: ");
				handler.AppendFormatted(cert?.Issuer ?? "NULL");
				stringBuilder5.AppendLine(ref handler);
				if (chain?.ChainElements != null)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder6 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(18, 1, stringBuilder2);
					handler.AppendLiteral("  Chain: ");
					handler.AppendFormatted(chain.ChainElements.Count);
					handler.AppendLiteral(" elements");
					stringBuilder6.AppendLine(ref handler);
					try
					{
						for (int i = 0; i < chain.ChainElements.Count; i++)
						{
							X509ChainElement x509ChainElement = chain.ChainElements[i];
							stringBuilder2 = stringBuilder;
							StringBuilder stringBuilder7 = stringBuilder2;
							handler = new StringBuilder.AppendInterpolatedStringHandler(8, 3, stringBuilder2);
							handler.AppendLiteral("    [");
							handler.AppendFormatted(i);
							handler.AppendLiteral("] ");
							handler.AppendFormatted(x509ChainElement.Certificate?.Thumbprint?.ToUpperInvariant());
							handler.AppendLiteral(" ");
							handler.AppendFormatted(x509ChainElement.Certificate?.Subject);
							stringBuilder7.AppendLine(ref handler);
						}
					}
					catch
					{
						stringBuilder.AppendLine("    [chain iteration error]");
					}
				}
				try
				{
					if (chain?.ChainStatus != null)
					{
						X509ChainStatus[] chainStatus = chain.ChainStatus;
						for (int j = 0; j < chainStatus.Length; j++)
						{
							X509ChainStatus x509ChainStatus = chainStatus[j];
							stringBuilder2 = stringBuilder;
							StringBuilder stringBuilder8 = stringBuilder2;
							handler = new StringBuilder.AppendInterpolatedStringHandler(13, 2, stringBuilder2);
							handler.AppendLiteral("  Status: ");
							handler.AppendFormatted(x509ChainStatus.Status);
							handler.AppendLiteral(" - ");
							handler.AppendFormatted(x509ChainStatus.StatusInformation);
							stringBuilder8.AppendLine(ref handler);
						}
					}
				}
				catch
				{
					stringBuilder.AppendLine("  [ChainStatus access error]");
				}
			}
			catch
			{
			}
			bool flag = (errors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0;
			bool flag2 = (errors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0;
			if (flag || flag2)
			{
				try
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder9 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(39, 2, stringBuilder2);
					handler.AppendLiteral("  REJECTED: NameMismatch=");
					handler.AppendFormatted(flag);
					handler.AppendLiteral(" NotAvailable=");
					handler.AppendFormatted(flag2);
					stringBuilder9.AppendLine(ref handler);
					_sslDiagLog = stringBuilder.ToString();
				}
				catch
				{
				}
				return false;
			}
			string text = request?.RequestUri?.Host?.ToLowerInvariant();
			bool flag3 = false;
			string[] allowedHostnames = AllowedHostnames;
			foreach (string text2 in allowedHostnames)
			{
				if (text == text2.ToLowerInvariant())
				{
					flag3 = true;
					break;
				}
			}
			if (!flag3)
			{
				try
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder10 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(35, 1, stringBuilder2);
					handler.AppendLiteral("  REJECTED: hostname '");
					handler.AppendFormatted(text);
					handler.AppendLiteral("' not allowed");
					stringBuilder10.AppendLine(ref handler);
					_sslDiagLog = stringBuilder.ToString();
				}
				catch
				{
				}
				return false;
			}
			if (IsRunningUnderWine)
			{
				try
				{
					string text3 = cert?.Issuer ?? "";
					StringBuilder stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler;
					if (!text3.Contains("Let's Encrypt") && !text3.Contains("Google Trust") && !text3.Contains("ISRG") && !text3.Contains("Cloudflare") && !text3.Contains("GlobalSign") && !text3.Contains("GTS"))
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder11 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(41, 1, stringBuilder2);
						handler.AppendLiteral("  REJECTED (Wine mode): unknown issuer '");
						handler.AppendFormatted(text3);
						handler.AppendLiteral("'");
						stringBuilder11.AppendLine(ref handler);
						_sslDiagLog = stringBuilder.ToString();
						return false;
					}
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder12 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(52, 1, stringBuilder2);
					handler.AppendLiteral("  ACCEPTED (Wine mode): hostname valid, issuer '");
					handler.AppendFormatted(text3);
					handler.AppendLiteral("' OK");
					stringBuilder12.AppendLine(ref handler);
					_sslDiagLog = stringBuilder.ToString();
					return true;
				}
				catch (Exception ex)
				{
					try
					{
						_sslDiagLog = "REJECTED (Wine): cert exception " + ex.GetType().Name;
					}
					catch
					{
					}
					return false;
				}
			}
			bool flag4 = errors == SslPolicyErrors.RemoteCertificateChainErrors;
			if (chain == null || chain.ChainElements == null || chain.ChainElements.Count == 0)
			{
				if (!flag4)
				{
					try
					{
						stringBuilder.AppendLine("  REJECTED: chain null/empty, errors != ChainErrorsOnly");
						_sslDiagLog = stringBuilder.ToString();
					}
					catch
					{
					}
					return false;
				}
			}
			else
			{
				bool flag5 = false;
				try
				{
					foreach (X509ChainElement chainElement in chain.ChainElements)
					{
						if (chainElement.Certificate != null)
						{
							string text4 = chainElement.Certificate.Thumbprint?.ToUpperInvariant();
							if (!string.IsNullOrEmpty(text4))
							{
								allowedHostnames = TrustedRootCAThumbprints;
								foreach (string text5 in allowedHostnames)
								{
									if (text4 == text5)
									{
										flag5 = true;
										break;
									}
								}
							}
						}
						if (flag5)
						{
							break;
						}
					}
				}
				catch
				{
				}
				if (!flag5 && !flag4)
				{
					try
					{
						stringBuilder.AppendLine("  REJECTED: no CA match, errors not ChainErrorsOnly");
						_sslDiagLog = stringBuilder.ToString();
					}
					catch
					{
					}
					return false;
				}
			}
			if (runSpkiPin)
			{
				try
				{
					bool flag6 = CertificatePinner.ValidateServerCertificate(request, cert, chain, errors);
					string value = CertificatePinner._lastDiag ?? "no diag";
					try
					{
						StringBuilder stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder13 = stringBuilder2;
						StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(9, 2, stringBuilder2);
						handler.AppendLiteral("  SPKI: ");
						handler.AppendFormatted(flag6 ? "ACCEPTED" : "REJECTED");
						handler.AppendLiteral(" ");
						handler.AppendFormatted(value);
						stringBuilder13.AppendLine(ref handler);
						_sslDiagLog = stringBuilder.ToString();
					}
					catch
					{
					}
					return flag6;
				}
				catch (Exception ex2)
				{
					try
					{
						StringBuilder stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder14 = stringBuilder2;
						StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(16, 2, stringBuilder2);
						handler.AppendLiteral("  SPKI CRASH: ");
						handler.AppendFormatted(ex2.GetType().Name);
						handler.AppendLiteral(": ");
						handler.AppendFormatted(ex2.Message);
						stringBuilder14.AppendLine(ref handler);
						_sslDiagLog = stringBuilder.ToString();
					}
					catch
					{
					}
					return false;
				}
			}
			try
			{
				stringBuilder.AppendLine("  ACCEPTED (base validation passed)");
				_sslDiagLog = stringBuilder.ToString();
			}
			catch
			{
			}
			return true;
		}
		catch
		{
			_sslDiagLog = "[CRASH] UnifiedSslCallback threw unhandled exception";
			return false;
		}
	}

	internal static HttpClient GetSharedHttpClient()
	{
		return httpClient;
	}

	static ModKeyValidator()
	{
		_jsonOpts = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
		KeyFormatRegex = new Regex("^[A-Z0-9P-]{19,23}$", RegexOptions.Compiled);
		TrustedRootCAThumbprints = new string[8] { "CABD2A79A1076A31F21D253635CB039D4329A5E8", "933C6DDEE95C9C41A40F9F50493D82BE03AD87BF", "E89B46892C805016E9367851B0444A61C0E67189", "D3779E396F4C39C80D4677765691079D85489F66", "F9AC55798481358D88DF1C3F44321C210D1D643D", "E72DF1D45493B827E6D0A760630B0B2E88381E28", "932BED339AA69212C89375B79304B475490B89A0", "B1BC968BD4F49D622AA89A81F2150152A41D829C" };
		AllowedHostnames = new string[1] { "api.crewcore.online" };
		_sslDiagLog = null;
		httpHandler = new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = (HttpRequestMessage request, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors errors) => UnifiedSslCallback(request, cert, chain, errors, runSpkiPin: false),
			AllowAutoRedirect = false
		};
		httpHandlerPinned = new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = (HttpRequestMessage request, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors errors) => UnifiedSslCallback(request, cert, chain, errors, runSpkiPin: true),
			AllowAutoRedirect = false
		};
		_rsaModulusPart1 = "10nomJIIOLVleBhf8OiVGn/PpaOnlN1Zvl0MfCN+Qymp3KEGIclegEujxXU28osUjF2ND/FsnC6vwu+x9WbBaURjBaFY6rgYB8EpVYls";
		_rsaModulusPart2 = "5SoHikaq4+407SPDo/1wHa+J3tU3a+e5D7mIFAWo13N11b2G9Veg+QHR7mtq3qB3Q6ltX9KKAEtJPSdhPRdizp8zcXvnZJ6PLFdOcCRgRCAChGGYUnm7rMdJwcFwjxE2WADHscpjcqiPuQ5pTU/KIrYgNzZvcpFF20/10ejFGluvHSSz";
		_rsaModulusPart3 = "Wwh68fJeJ21lGOZleLDfRU3vZQ4LaRwfqAQaLT0vpTndZsLbtuhyAU0/MwVN3Q==";
		_rsaExponent = "AQAB";
		_rsaCachedProps = BuildRsaPropertyCache();
		_rsaJsonOpts = new JsonSerializerOptions
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
		_textureMemoryPool = 0u;
		ApiBaseUrl = "https://api.crewcore.online";
		_cachedHwid = null;
		SessionToken = 0L;
		__svc_internal = false;
		__svc_checksum = 0;
		_svcLock = new object();
		_svcMismatchCount = 0;
		LastValidationMessage = "Aguardando validação...";
		ValidatedUsername = "";
		CurrentKey = "";
		PendingResetRequest = false;
		_srvProof = null;
		_proofSeedXor = 0L;
		_proofExpiresXor = 0L;
		_proofReceivedUnscaledXor = 3215389256933277750L;
		_proofTtlMsXor = 3215389256933277750L;
		_serverHeartbeatNonce = "";
		_integrityCounter = 0;
		_serverTimeOffsetXor = 5588521007158401630L;
		CachedFrameTimeMs = 0L;
		_monoStopwatch = Stopwatch.StartNew();
		_lastWallCheckMs = -1L;
		_lastMonoCheckMs = -1L;
		_lastJumpWakeMs = 0L;
		_clockDriftClampCount = 0L;
		FormattedDisplayString = "https://crewcore.online/";
		KeyDisplay = "";
		TimeDisplay = "";
		_rtx = 0L;
		_vcs = 0;
		_xf = 0;
		_hv = 0u;
		_lastServerCheck = 0L;
		_validationLock = new object();
		_debugSessionDeathLogged = false;
		_debugHeartbeatMsg = null;
		CurrentSessionToken = 0L;
		_localTick = 0L;
		_lastTickUpdateMs = 0L;
		_serverExpectedTick = 0L;
		_serverTickTolerance = 100L;
		_queryKeyRegex = new Regex("(?i)([?&](?:key|hwid|token|auth|session|password|pwd|secret|credential|bearer)=)[^&\"'\\s]+", RegexOptions.Compiled);
		_keyLikeRegex = new Regex("[A-Z0-9]{4}(?:-[A-Z0-9]{4}){2,5}", RegexOptions.Compiled);
		_jsonSecretRegex = new Regex("(?i)(\"(?:key|hwid|token|session_token|identity_credential|bearer|secret)\"\\s*:\\s*\")[^\"]*(?=\")", RegexOptions.Compiled);
		_isHeartbeatRunningInt = 0;
		_enableHeartbeat = true;
		DiscordRevoked = false;
		RevokeReason = "";
		IsRunningUnderWine = DetectWine();
		try
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
		}
		catch
		{
		}
		httpClient = new HttpClient(httpHandlerPinned);
		httpClient.Timeout = TimeSpan.FromSeconds(30.0);
		httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CrewClient/1.0 (Unity; Windows; Elite)");
		httpClient.DefaultRequestHeaders.ConnectionClose = false;
	}

	private static (PropertyInfo, string)[] BuildRsaPropertyCache()
	{
		List<(PropertyInfo, string)> list = new List<(PropertyInfo, string)>();
		PropertyInfo[] properties = typeof(ApiValidationResponse).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!propertyInfo.Name.StartsWith("Signature", StringComparison.OrdinalIgnoreCase) && propertyInfo.GetCustomAttributes(typeof(JsonExtensionDataAttribute), inherit: false).Length == 0)
			{
				object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(JsonPropertyNameAttribute), inherit: false);
				string text = ((customAttributes.Length != 0) ? ((JsonPropertyNameAttribute)customAttributes[0]).Name : ToSnakeCase(char.ToLowerInvariant(propertyInfo.Name[0]) + propertyInfo.Name.Substring(1)));
				if (!text.StartsWith("signature"))
				{
					list.Add((propertyInfo, text));
				}
			}
		}
		return list.ToArray();
	}

	private static bool VerifyRSASignature(ApiValidationResponse response)
	{
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Expected O, but got Unknown
		bool flag = default(bool);
		if (response == null || string.IsNullOrEmpty(response.SignatureRsa))
		{
			ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
			if (((instance != null) ? ((BasePlugin)instance).Log : null) != null)
			{
				ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
				BepInExErrorLogInterpolatedStringHandler val = new BepInExErrorLogInterpolatedStringHandler(40, 2, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[RSA-VERIFY] REJECTED: response=");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<bool>(response != null);
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" sigRsa=");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>((response?.SignatureRsa?.Length).GetValueOrDefault(-1));
				}
				log.LogError(val);
			}
			return false;
		}
		try
		{
			byte[] modulus = Convert.FromBase64String(_rsaModulusPart1 + _rsaModulusPart2 + _rsaModulusPart3);
			byte[] exponent = Convert.FromBase64String(_rsaExponent);
			RSAParameters rSAParameters = default(RSAParameters);
			rSAParameters.Modulus = modulus;
			rSAParameters.Exponent = exponent;
			RSAParameters parameters = rSAParameters;
			List<(string, string)> list = new List<(string, string)>();
			(PropertyInfo, string)[] rsaCachedProps = _rsaCachedProps;
			for (int i = 0; i < rsaCachedProps.Length; i++)
			{
				(PropertyInfo, string) tuple = rsaCachedProps[i];
				PropertyInfo item = tuple.Item1;
				string item2 = tuple.Item2;
				object value = item.GetValue(response);
				if (value != null)
				{
					object obj = value;
					Type type = value.GetType();
					if (!(Nullable.GetUnderlyingType(type) != null) || value != null)
					{
						string item3 = ((!(obj is string text)) ? ((!(obj is bool flag2)) ? ((!(obj is JsonElement jsonElement)) ? ((obj is IEnumerable && !(obj is string)) ? (item2 + "=" + JsonSerializer.Serialize(obj, _rsaJsonOpts)) : ((!type.IsClass || !(type != typeof(string))) ? $"{item2}={obj}" : (item2 + "=" + JsonSerializer.Serialize(obj, _rsaJsonOpts)))) : (item2 + "=" + jsonElement.GetRawText())) : (item2 + "=" + flag2.ToString().ToLowerInvariant())) : (item2 + "=\"" + text + "\""));
						list.Add((item2, item3));
					}
				}
			}
			if (response.ExtensionData != null)
			{
				foreach (KeyValuePair<string, JsonElement> extensionDatum in response.ExtensionData)
				{
					if (!extensionDatum.Key.StartsWith("signature") && extensionDatum.Value.ValueKind != JsonValueKind.Null)
					{
						list.Add((extensionDatum.Key, extensionDatum.Key + "=" + extensionDatum.Value.GetRawText()));
					}
				}
			}
			list.Sort(((string key, string serialized) a, (string key, string serialized) b) => string.Compare(a.key, b.key, StringComparison.Ordinal));
			string text2 = string.Join("&", list.Select<(string, string), string>(((string key, string serialized) kv) => kv.serialized));
			ModMenuCrewPlugin instance2 = ModMenuCrewPlugin.Instance;
			if (((instance2 != null) ? ((BasePlugin)instance2).Log : null) != null)
			{
				byte[] array = SHA256.HashData(Encoding.UTF8.GetBytes(text2));
				ManualLogSource log2 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
				BepInExWarningLogInterpolatedStringHandler val2 = new BepInExWarningLogInterpolatedStringHandler(41, 3, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[RSA-VERIFY] canonical_len=");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<int>(text2.Length);
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(" hash=");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(BitConverter.ToString(array, 0, 8).Replace("-", ""));
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(" keys=[");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(string.Join(",", list.Select<(string, string), string>(((string key, string serialized) kv) => kv.key)));
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("]");
				}
				log2.LogWarning(val2);
			}
			using RSA rSA = RSA.Create();
			rSA.ImportParameters(parameters);
			byte[] signature = Convert.FromBase64String(response.SignatureRsa);
			byte[] bytes = Encoding.UTF8.GetBytes(text2);
			bool num = rSA.VerifyData(bytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			if (!num)
			{
				ModMenuCrewPlugin instance3 = ModMenuCrewPlugin.Instance;
				if (((instance3 != null) ? ((BasePlugin)instance3).Log : null) != null)
				{
					((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogError((object)"[RSA-VERIFY] MISMATCH: signature format valid but canonical data doesn't match.");
				}
			}
			return num;
		}
		catch (Exception ex)
		{
			ModMenuCrewPlugin instance4 = ModMenuCrewPlugin.Instance;
			if (((instance4 != null) ? ((BasePlugin)instance4).Log : null) != null)
			{
				ManualLogSource log3 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
				bool flag3 = default(bool);
				BepInExErrorLogInterpolatedStringHandler val = new BepInExErrorLogInterpolatedStringHandler(26, 2, ref flag3);
				if (flag3)
				{
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[RSA-VERIFY] EXCEPTION: ");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(ex.GetType().Name);
					((BepInExLogInterpolatedStringHandler)val).AppendLiteral(": ");
					((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(ex.Message);
				}
				log3.LogError(val);
			}
			return false;
		}
	}

	private static string ToSnakeCase(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (char.IsUpper(c) && i > 0)
			{
				stringBuilder.Append('_');
				stringBuilder.Append(char.ToLowerInvariant(c));
			}
			else
			{
				stringBuilder.Append(char.ToLowerInvariant(c));
			}
		}
		return stringBuilder.ToString();
	}

	private static string GetPlayerPrefsKeyActivated()
	{
		return "ModMenuCrew_Activated_6.1.4b";
	}

	private static string GetHardwareId()
	{
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Expected O, but got Unknown
		if (!string.IsNullOrEmpty(_cachedHwid))
		{
			return _cachedHwid;
		}
		try
		{
			string text = "";
			try
			{
				using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Cryptography", writable: false);
				if (registryKey != null)
				{
					text = registryKey.GetValue("MachineGuid")?.ToString() ?? "";
				}
			}
			catch
			{
			}
			string s = string.Join("|", text, Environment.MachineName, Environment.UserName, SystemInfo.processorType, SystemInfo.processorCount.ToString());
			using SHA256 sHA = SHA256.Create();
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			byte[] array = sHA.ComputeHash(bytes);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < 16; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			_cachedHwid = stringBuilder.ToString().ToUpperInvariant();
		}
		catch (Exception ex)
		{
			ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
			if (instance != null)
			{
				ManualLogSource log = ((BasePlugin)instance).Log;
				if (log != null)
				{
					bool flag = default(bool);
					BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(59, 1, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ModKeyValidator] HWID generation failed (");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(ex.GetType().Name);
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral("), using fallback");
					}
					log.LogWarning(val);
				}
			}
			string s2 = Environment.MachineName + "|" + Environment.UserName + "|FALLBACK";
			using SHA256 sHA2 = SHA256.Create();
			byte[] bytes2 = Encoding.UTF8.GetBytes(s2);
			byte[] array2 = sHA2.ComputeHash(bytes2);
			StringBuilder stringBuilder2 = new StringBuilder();
			for (int j = 0; j < 16; j++)
			{
				stringBuilder2.Append(array2[j].ToString("x2"));
			}
			_cachedHwid = stringBuilder2.ToString().ToUpperInvariant();
		}
		return _cachedHwid;
	}

	internal static void SetServiceContext(bool value, long token)
	{
		lock (_svcLock)
		{
			if (!(token != SessionToken && value))
			{
				__svc_internal = value;
				__svc_checksum = (value ? 23130 : 42405) ^ (int)(token & 0xFFFF) ^ (_integrityCounter * 4919);
				Interlocked.Exchange(ref _svcMismatchCount, 0);
			}
		}
	}

	internal static void RequestResetFromRealtime(string reason = "")
	{
		if (!string.IsNullOrEmpty(reason))
		{
			if ("discord_left".Equals(reason, StringComparison.OrdinalIgnoreCase))
			{
				DiscordRevoked = true;
				RevokeReason = "discord_left";
			}
			else if ("expired".Equals(reason, StringComparison.OrdinalIgnoreCase))
			{
				RevokeReason = "expired";
			}
			else if ("hwid_mismatch".Equals(reason, StringComparison.OrdinalIgnoreCase))
			{
				RevokeReason = "hwid_mismatch";
			}
			else
			{
				RevokeReason = reason;
			}
		}
		PendingResetRequest = true;
	}

	internal static void UpdateFrameTimeCache()
	{
		CachedFrameTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		CheckClockJump();
	}

	private static void CheckClockJump()
	{
		try
		{
			long cachedFrameTimeMs = CachedFrameTimeMs;
			long monotonicMs = MonotonicMs;
			if (_lastWallCheckMs < 0 || _lastMonoCheckMs < 0)
			{
				_lastWallCheckMs = cachedFrameTimeMs;
				_lastMonoCheckMs = monotonicMs;
				return;
			}
			long num = cachedFrameTimeMs - _lastWallCheckMs;
			long num2 = monotonicMs - _lastMonoCheckMs;
			_lastWallCheckMs = cachedFrameTimeMs;
			_lastMonoCheckMs = monotonicMs;
			bool flag = num > num2 + 30000;
			bool flag2 = num < 0 || num2 < 0;
			bool flag3 = Math.Abs(num - num2) > 30000;
			if (!(flag || flag2 || flag3) || cachedFrameTimeMs - _lastJumpWakeMs <= 5000)
			{
				return;
			}
			_lastJumpWakeMs = cachedFrameTimeMs;
			try
			{
				ForceHeartbeatWakeup();
			}
			catch
			{
			}
		}
		catch
		{
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool V()
	{
		if (_srvProof == null || ProofSeed <= 0)
		{
			return false;
		}
		if (ProofHasRelativeTracking)
		{
			long proofTtlMs = ProofTtlMs;
			if (proofTtlMs <= 0 || proofTtlMs > 600000)
			{
				return false;
			}
			long num = MonotonicMs - ProofReceivedUnscaledMs;
			if (num < 0)
			{
				return false;
			}
			return num < proofTtlMs;
		}
		long num2 = ((CachedFrameTimeMs > 0) ? CachedFrameTimeMs : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) + ServerTimeOffsetMs;
		return ProofExpires > num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float GetSeedValue(float min, float max)
	{
		if (!V())
		{
			return min;
		}
		float num = max - min;
		float num2 = (float)(ProofSeed % 1000) / 1000f;
		return min + num * num2;
	}

	internal static void UpdateProof(string proof, long seed, long expires, long ttlSeconds = 0L)
	{
		if (string.IsNullOrEmpty(proof) || seed <= 0 || expires <= 0)
		{
			return;
		}
		_srvProof = proof;
		_proofSeedXor = seed ^ 0x7F4E2B8D3A1C6059L;
		_proofExpiresXor = expires ^ 0x7F4E2B8D3A1C6059L;
		if (ttlSeconds > 0)
		{
			long num = ttlSeconds * 1000;
			if (num > 600000)
			{
				num = 600000L;
			}
			_proofReceivedUnscaledXor = MonotonicMs ^ 0x2C9F5B7E4A1D8036L;
			_proofTtlMsXor = num ^ 0x2C9F5B7E4A1D8036L;
		}
		else
		{
			_proofReceivedUnscaledXor = 3215389256933277750L;
			_proofTtlMsXor = 3215389256933277750L;
		}
		lock (_svcLock)
		{
			_integrityCounter++;
			if (__svc_internal)
			{
				__svc_checksum = 0x5A5A ^ (int)(SessionToken & 0xFFFF) ^ (_integrityCounter * 4919);
			}
		}
		long num2 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		long num3 = expires - 300000 - num2;
		if (Math.Abs(num3) > 1800000)
		{
			_serverTimeOffsetXor = ((num3 > 0) ? 1800000 : (-1800000)) ^ 0x4D8E6A3F2B7C1A5EL;
			_clockDriftClampCount++;
		}
		else
		{
			_serverTimeOffsetXor = num3 ^ 0x4D8E6A3F2B7C1A5EL;
		}
	}

	internal static void ClearProof()
	{
		_srvProof = null;
		_proofSeedXor = 0L;
		_proofExpiresXor = 0L;
		_proofReceivedUnscaledXor = 3215389256933277750L;
		_proofTtlMsXor = 3215389256933277750L;
	}

	private static string DecryptUIPayload(EncryptedPayload payload, string sessionKeyHex)
	{
		if (payload == null || string.IsNullOrEmpty(payload.Ciphertext) || string.IsNullOrEmpty(payload.Iv) || string.IsNullOrEmpty(payload.Tag) || string.IsNullOrEmpty(sessionKeyHex))
		{
			return null;
		}
		try
		{
			byte[] key = HexToBytes(sessionKeyHex);
			byte[] nonce = HexToBytes(payload.Iv);
			byte[] tag = HexToBytes(payload.Tag);
			byte[] array = Convert.FromBase64String(payload.Ciphertext);
			byte[] array2 = new byte[array.Length];
			using (AesGcm aesGcm = new AesGcm(key))
			{
				aesGcm.Decrypt(nonce, array, tag, array2);
			}
			return Encoding.UTF8.GetString(array2);
		}
		catch (Exception)
		{
			return null;
		}
	}

	private static byte[] HexToBytes(string hex)
	{
		int length = hex.Length;
		byte[] array = new byte[length / 2];
		for (int i = 0; i < length; i += 2)
		{
			array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static string GetDerivedApiUrl()
	{
		string apiBaseUrl = ApiBaseUrl;
		if (apiBaseUrl == null)
		{
			return string.Empty;
		}
		if (apiBaseUrl.Length == 0)
		{
			return string.Empty;
		}
		if (!apiBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			return string.Empty;
		}
		if (!apiBaseUrl.Contains("crewcore.online"))
		{
			return string.Empty;
		}
		return apiBaseUrl;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static bool VerifyAllowedHostnames()
	{
		if (AllowedHostnames == null || AllowedHostnames.Length != 1)
		{
			return false;
		}
		return string.Equals(AllowedHostnames[0], "api.crewcore.online", StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsDebuggerPresent()
	{
		if (Debugger.IsAttached)
		{
			return true;
		}
		try
		{
			foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
			{
				string text = module.ModuleName.ToLowerInvariant();
				if (text.Contains("dnspy") || text.Contains("ilspy") || text.Contains("de4dot") || text.Contains("harmony") || text.Contains("cheatengine") || text.Contains("x64dbg") || text.Contains("ollydbg") || text.Contains("ida"))
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	private static uint CalculateMethodChecksum()
	{
		uint num = 305419896u;
		try
		{
			MethodInfo method = typeof(ModKeyValidator).GetMethod("IsSessionValid", BindingFlags.Static | BindingFlags.Public);
			if (method != null)
			{
				MethodBody methodBody = method.GetMethodBody();
				if (methodBody != null)
				{
					byte[] iLAsByteArray = methodBody.GetILAsByteArray();
					if (iLAsByteArray != null)
					{
						byte[] array = iLAsByteArray;
						foreach (byte b in array)
						{
							num = num * 31 + b;
						}
					}
				}
			}
		}
		catch
		{
			num = 0u;
		}
		return num;
	}

	internal static bool IsSessionValid()
	{
		bool flag = string.IsNullOrEmpty(CurrentKey);
		bool flag2 = CurrentSessionToken == 0;
		bool flag3 = SessionToken == 0;
		bool flag4 = !_svcCtx;
		if (flag || flag2 || flag3 || flag4)
		{
			if (!_debugSessionDeathLogged && !flag)
			{
				_debugSessionDeathLogged = true;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool QuickValidate()
	{
		if (!__svc_internal)
		{
			return false;
		}
		if (SessionToken == 0L || CurrentSessionToken == 0L)
		{
			return false;
		}
		if (string.IsNullOrEmpty(CurrentKey))
		{
			return false;
		}
		if (_textureMemoryPool == 0)
		{
			_textureMemoryPool = (uint)(SessionToken ^ CurrentSessionToken ^ (uint)CurrentKey.GetHashCode());
			return true;
		}
		uint num = (uint)(SessionToken ^ CurrentSessionToken ^ (uint)CurrentKey.GetHashCode());
		return _textureMemoryPool == num;
	}

	internal static void SyncValidationState()
	{
		lock (_svcLock)
		{
			if (__svc_internal && SessionToken != 0L && CurrentSessionToken != 0L && !string.IsNullOrEmpty(CurrentKey))
			{
				_textureMemoryPool = (uint)(SessionToken ^ CurrentSessionToken ^ (uint)CurrentKey.GetHashCode());
				__svc_checksum = (__svc_internal ? 23130 : 42405) ^ (int)(SessionToken & 0xFFFF) ^ (_integrityCounter * 4919);
			}
		}
	}

	internal static void UpdateValidationState(bool success, long serverTime)
	{
		if (success && !string.IsNullOrEmpty(CurrentKey))
		{
			_rtx = SessionToken;
			_vcs = CurrentKey.GetHashCode() ^ (int)(CurrentSessionToken >> 16);
			_xf = (byte)((uint)(CurrentKey[0] ^ CurrentKey[CurrentKey.Length - 1]) & 0xFFu);
			_hv = (uint)(SessionToken ^ CurrentSessionToken ^ 0xDEADBEEFu);
			_lastServerCheck = ((serverTime > 0) ? serverTime : DateTimeOffset.UtcNow.ToUnixTimeSeconds());
		}
		else
		{
			_rtx = 0L;
			_vcs = 0;
			_xf = 0;
			_hv = 0u;
			_lastServerCheck = 0L;
		}
	}

	internal static void SetSessionTokenInternal(long token)
	{
		if (token > 0)
		{
			CurrentSessionToken = token;
			SyncValidationState();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void IncrementTick()
	{
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		if (num - _lastTickUpdateMs >= 14)
		{
			_lastTickUpdateMs = num;
			_localTick++;
		}
	}

	internal static bool ValidateServerTick(long serverExpected, long tolerance)
	{
		_serverExpectedTick = serverExpected;
		_serverTickTolerance = tolerance;
		if (Math.Abs(_localTick - serverExpected) > tolerance)
		{
			ServerData.TriggerSilentDenial();
			return false;
		}
		return true;
	}

	internal static void ResetTicks()
	{
		_localTick = 0L;
		_lastTickUpdateMs = 0L;
		_serverExpectedTick = 0L;
	}

	private static long CalculateSessionToken(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return 0L;
		}
		long num = 0L;
		foreach (char c in key)
		{
			num = num * 31 + c;
		}
		return num;
	}

	internal static Task<(bool success, string message, string username)> ValidateKeyAsync(string keyFromInput)
	{
		return ValidateKeyAsync(keyFromInput, CancellationToken.None);
	}

	internal static async Task<(bool success, string message, string username)> ValidateKeyAsync(string keyFromInput, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(keyFromInput))
		{
			return (success: false, message: "No key provided.", username: null);
		}
		if (!KeyFormatRegex.IsMatch(keyFromInput))
		{
			return (success: false, message: "Invalid key format.", username: null);
		}
		Exception ex = null;
		TimeSpan nextDelay = TimeSpan.FromMilliseconds(1500.0);
		bool flag = default(bool);
		for (int attempt = 0; attempt < 2; attempt++)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return (success: false, message: "Validation cancelled.", username: null);
			}
			if (attempt > 0)
			{
				try
				{
					await Task.Delay(nextDelay, cancellationToken);
				}
				catch (OperationCanceledException)
				{
					return (success: false, message: "Validation cancelled.", username: null);
				}
			}
			try
			{
				return await ValidateKeyAttemptAsync(keyFromInput, cancellationToken);
			}
			catch (RateLimitedException ex3)
			{
				ex = ex3;
				nextDelay = ((ex3.RetryAfter > TimeSpan.Zero) ? ex3.RetryAfter : TimeSpan.FromMilliseconds(1500.0));
				if (ModMenuCrewPlugin.Instance != null)
				{
					ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
					BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(37, 1, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[Validation] Rate limited, retry in ");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<double>(nextDelay.TotalSeconds, "0.#");
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral("s");
					}
					log.LogWarning(val);
				}
			}
			catch (HttpRequestException ex4)
			{
				ex = ex4;
				string fullExceptionChain = GetFullExceptionChain(ex4);
				string sslDiagLog = _sslDiagLog;
				if (ModMenuCrewPlugin.Instance == null)
				{
					continue;
				}
				ManualLogSource log2 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
				BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(44, 3, ref flag);
				if (flag)
				{
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[Validation] Network attempt ");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<int>(attempt + 1);
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("/");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<int>(2);
					((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(" FULL DETAIL:\n");
					((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(fullExceptionChain);
				}
				log2.LogError(val2);
				if (!string.IsNullOrEmpty(sslDiagLog))
				{
					ManualLogSource log3 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
					val2 = new BepInExErrorLogInterpolatedStringHandler(32, 1, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[Validation] SSL CALLBACK DIAG:\n");
						((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(MaskSensitiveText(sslDiagLog));
					}
					log3.LogError(val2);
				}
				else
				{
					ManualLogSource log4 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
					val2 = new BepInExErrorLogInterpolatedStringHandler(78, 0, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[Validation] SSL callback was NEVER invoked (handshake failed before callback)");
					}
					log4.LogError(val2);
				}
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				return (success: false, message: "Validation cancelled.", username: null);
			}
			catch (TaskCanceledException ex6)
			{
				ex = ex6;
				if (ModMenuCrewPlugin.Instance != null)
				{
					ManualLogSource log5 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
					BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(30, 2, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[Validation] Timeout attempt ");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>(attempt + 1);
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral("/");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>(2);
					}
					log5.LogWarning(val);
				}
			}
			catch (Exception ex7)
			{
				string fullExceptionChain2 = GetFullExceptionChain(ex7);
				if (ModMenuCrewPlugin.Instance != null)
				{
					ManualLogSource log6 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
					BepInExErrorLogInterpolatedStringHandler val2 = new BepInExErrorLogInterpolatedStringHandler(43, 1, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[Validation] Unexpected error FULL DETAIL:\n");
						((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(fullExceptionChain2);
					}
					log6.LogError(val2);
				}
				return (success: false, message: "Unexpected error: " + ex7.GetType().Name + ": " + MaskSensitiveText(ex7.Message), username: null);
			}
		}
		if (ex is TaskCanceledException)
		{
			return (success: false, message: "Validation timeout. Check your connection.", username: null);
		}
		if (ex is RateLimitedException)
		{
			return (success: false, message: "Server busy, please try again shortly.", username: null);
		}
		string fullExceptionChain3 = GetFullExceptionChain(ex);
		if (fullExceptionChain3.Contains("SSL") || fullExceptionChain3.Contains("TLS") || fullExceptionChain3.Contains("certificate") || fullExceptionChain3.Contains("AuthenticationException") || fullExceptionChain3.Contains("SecureChannel"))
		{
			try
			{
				ModMenuCrewPlugin.DebuggerComponent.OpenBrowser("https://crewcore.online/");
			}
			catch
			{
			}
			return (success: false, message: "Connection error — your version may be outdated.\nPlease download the latest version at crewcore.online", username: null);
		}
		string fullExceptionChainShort = GetFullExceptionChainShort(ex);
		return (success: false, message: "Login error: " + fullExceptionChainShort, username: null);
	}

	internal static string MaskSensitiveText(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		try
		{
			string input2 = _queryKeyRegex.Replace(input, "$1***");
			input2 = _jsonSecretRegex.Replace(input2, "$1***");
			return _keyLikeRegex.Replace(input2, "***-****-****");
		}
		catch
		{
			return "[mask-error]";
		}
	}

	private static string GetFullExceptionChain(Exception ex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		Exception ex2 = ex;
		while (ex2 != null)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(11, 3, stringBuilder2);
			handler.AppendLiteral("[Level ");
			handler.AppendFormatted(num);
			handler.AppendLiteral("] ");
			handler.AppendFormatted(ex2.GetType().FullName);
			handler.AppendLiteral(": ");
			handler.AppendFormatted(MaskSensitiveText(ex2.Message));
			stringBuilder3.AppendLine(ref handler);
			if (ex2.StackTrace != null)
			{
				string[] array = ex2.StackTrace.Split('\n');
				for (int i = 0; i < Math.Min(3, array.Length); i++)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder4 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
					handler.AppendLiteral("  ");
					handler.AppendFormatted(array[i].Trim());
					stringBuilder4.AppendLine(ref handler);
				}
			}
			ex2 = ex2.InnerException;
			num++;
		}
		return stringBuilder.ToString();
	}

	private static string GetFullExceptionChainShort(Exception ex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Exception ex2 = ex;
		int num = 0;
		while (ex2 != null)
		{
			if (num > 0)
			{
				stringBuilder.Append(" → ");
			}
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
			handler.AppendFormatted(ex2.GetType().Name);
			handler.AppendLiteral(": ");
			handler.AppendFormatted(MaskSensitiveText(ex2.Message));
			stringBuilder2.Append(ref handler);
			ex2 = ex2.InnerException;
			num++;
		}
		return stringBuilder.ToString();
	}

	private static async Task<(bool success, string message, string username)> ValidateKeyAttemptAsync(string keyFromInput)
	{
		return await ValidateKeyAttemptAsync(keyFromInput, CancellationToken.None);
	}

	private static async Task<(bool success, string message, string username)> ValidateKeyAttemptAsync(string keyFromInput, CancellationToken cancellationToken)
	{
		string hardwareId = GetHardwareId();
		string requestUri = ApiBaseUrl + "/validate";
		string content = JsonSerializer.Serialize(new
		{
			key = keyFromInput,
			hwid = hardwareId,
			version = "6.1.4b",
			tick = _localTick
		});
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
		using StringContent requestContent = new StringContent(content, Encoding.UTF8, "application/json");
		request.Content = requestContent;
		string identityCredential = DiscordAuthManager.IdentityCredential;
		if (!string.IsNullOrEmpty(identityCredential))
		{
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", identityCredential);
		}
		using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
		string text = await response.Content.ReadAsStringAsync();
		bool flag = default(bool);
		if (!response.IsSuccessStatusCode)
		{
			try
			{
				string text2 = (response.Headers.Contains("CF-RAY") ? response.Headers.GetValues("CF-RAY").FirstOrDefault() : "none");
				string text3 = (response.Headers.Contains("Server") ? response.Headers.GetValues("Server").FirstOrDefault() : "none");
				string text4 = MaskSensitiveText((text != null && text.Length > 300) ? text.Substring(0, 300) : (text ?? "empty"));
				string text5 = ApiBaseUrl + "/validate";
				ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
				if (((instance != null) ? ((BasePlugin)instance).Log : null) != null)
				{
					ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
					BepInExErrorLogInterpolatedStringHandler val = new BepInExErrorLogInterpolatedStringHandler(58, 6, ref flag);
					if (flag)
					{
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[VALIDATE-DIAG] Endpoint=");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(text5);
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" Status=");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<HttpStatusCode>(response.StatusCode);
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" (");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<int>((int)response.StatusCode);
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral(") CF-RAY=");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(text2);
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" Server=");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(text3);
						((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" Body=");
						((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(text4);
					}
					log.LogError(val);
				}
			}
			catch
			{
			}
			if (response.StatusCode == HttpStatusCode.TooManyRequests)
			{
				TimeSpan retryAfter = TimeSpan.FromMilliseconds(1500.0);
				try
				{
					if (response.Headers.RetryAfter != null)
					{
						if (response.Headers.RetryAfter.Delta.HasValue)
						{
							retryAfter = response.Headers.RetryAfter.Delta.Value;
						}
						else if (response.Headers.RetryAfter.Date.HasValue)
						{
							TimeSpan timeSpan = response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
							if (timeSpan > TimeSpan.Zero)
							{
								retryAfter = timeSpan;
							}
						}
					}
				}
				catch
				{
				}
				throw new RateLimitedException(retryAfter);
			}
		}
		if (response.IsSuccessStatusCode)
		{
			ApiValidationResponse apiValidationResponse = null;
			try
			{
				apiValidationResponse = JsonSerializer.Deserialize<ApiValidationResponse>(text, _jsonOpts);
			}
			catch
			{
			}
			if (apiValidationResponse != null && "success".Equals(apiValidationResponse.Status, StringComparison.OrdinalIgnoreCase))
			{
				if (!VerifyRSASignature(apiValidationResponse))
				{
					ServerData.TriggerSilentDenial();
					return (success: false, message: "Server verification failed.", username: null);
				}
				if (apiValidationResponse.ExpectedTick.HasValue && apiValidationResponse.ExpectedTick.Value > 0)
				{
					long valueOrDefault = apiValidationResponse.TickTolerance.GetValueOrDefault(100L);
					if (!ValidateServerTick(apiValidationResponse.ExpectedTick.Value, valueOrDefault))
					{
						return (success: false, message: "Session sync failed.", username: null);
					}
				}
				DiscordRevoked = false;
				RevokeReason = "";
				CurrentKey = keyFromInput;
				SessionToken = CalculateSessionToken(CurrentKey);
				long num = ((apiValidationResponse.SessionToken.GetValueOrDefault() > 1000) ? apiValidationResponse.SessionToken.GetValueOrDefault() : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
				KeyDisplay = apiValidationResponse.KeyDisplay ?? "";
				TimeDisplay = apiValidationResponse.TimeDisplay ?? "";
				ValidatedUsername = apiValidationResponse.Username ?? "User";
				SetServiceContext(value: true, SessionToken);
				UpdateProof(apiValidationResponse.SessionProof, apiValidationResponse.ProofSeed.GetValueOrDefault(), apiValidationResponse.ProofExpires.GetValueOrDefault(), apiValidationResponse.ProofTtlSeconds.GetValueOrDefault());
				ServerGate.UpdateRenderPermission(apiValidationResponse.RenderKey, apiValidationResponse.RenderExpires.GetValueOrDefault(), apiValidationResponse.RenderNonce.GetValueOrDefault(), apiValidationResponse.RenderTtlSeconds.GetValueOrDefault());
				EncryptedPayload encryptedPayload = apiValidationResponse.UiPayload;
				if (encryptedPayload == null && apiValidationResponse.ExtensionData != null && apiValidationResponse.ExtensionData.TryGetValue("ui_payload", out var value) && value.ValueKind == JsonValueKind.Object)
				{
					try
					{
						encryptedPayload = JsonSerializer.Deserialize<EncryptedPayload>(value.GetRawText(), _jsonOpts);
					}
					catch
					{
					}
				}
				if (encryptedPayload != null && !string.IsNullOrEmpty(apiValidationResponse.SessionKey))
				{
					try
					{
						ServerData.ParseFromEncryptedPayload(new ServerData.EncryptedPayload
						{
							Ciphertext = encryptedPayload.Ciphertext,
							Iv = encryptedPayload.Iv,
							Tag = encryptedPayload.Tag
						}, apiValidationResponse.SessionKey, num);
					}
					catch (Exception ex)
					{
						if (ModMenuCrewPlugin.Instance != null)
						{
							ManualLogSource log2 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
							BepInExWarningLogInterpolatedStringHandler val2 = new BepInExWarningLogInterpolatedStringHandler(30, 1, ref flag);
							if (flag)
							{
								((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[Validation] UI Parse failed: ");
								((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(ex.Message);
							}
							log2.LogWarning(val2);
						}
						ServerData.SetLoaded(loaded: false);
					}
				}
				else if (num > 0)
				{
					CurrentSessionToken = num;
				}
				StartHeartbeat();
				return (success: true, message: apiValidationResponse.Message ?? "Key validated!", username: apiValidationResponse.Username);
			}
			return (success: false, message: apiValidationResponse?.Message ?? "Invalid key/API error.", username: null);
		}
		string item = $"Server error ({response.StatusCode}).";
		try
		{
			if (!string.IsNullOrWhiteSpace(text) && text.TrimStart().StartsWith("{"))
			{
				ApiValidationResponse apiValidationResponse2 = JsonSerializer.Deserialize<ApiValidationResponse>(text, _jsonOpts);
				if (apiValidationResponse2 != null && apiValidationResponse2.ForceUpdate.GetValueOrDefault())
				{
					string url = apiValidationResponse2.DownloadUrl ?? "https://crewcore.online/";
					string text6 = apiValidationResponse2.MinVersion ?? "6.1.4b";
					if (ModMenuCrewPlugin.Instance != null)
					{
						ManualLogSource log3 = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
						BepInExWarningLogInterpolatedStringHandler val2 = new BepInExWarningLogInterpolatedStringHandler(63, 2, ref flag);
						if (flag)
						{
							((BepInExLogInterpolatedStringHandler)val2).AppendLiteral("[IRON CURTAIN] Mandatory update required. Current: ");
							((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>("6.1.4b");
							((BepInExLogInterpolatedStringHandler)val2).AppendLiteral(", Required: ");
							((BepInExLogInterpolatedStringHandler)val2).AppendFormatted<string>(text6);
						}
						log3.LogWarning(val2);
					}
					ModMenuCrewPlugin.DebuggerComponent.OpenBrowser(url);
					return (success: false, message: "⚠\ufe0f SECURITY UPDATE REQUIRED ⚠\ufe0f\nPlease download v" + text6 + " or newer.\nOpening download page...", username: null);
				}
				if (apiValidationResponse2 != null && "discord_left".Equals(apiValidationResponse2.Reason, StringComparison.OrdinalIgnoreCase))
				{
					DiscordRevoked = true;
					RevokeReason = "discord_left";
				}
				if (!string.IsNullOrWhiteSpace(apiValidationResponse2?.Message))
				{
					item = ((!string.IsNullOrWhiteSpace(apiValidationResponse2.Hint)) ? (apiValidationResponse2.Message + "\n" + apiValidationResponse2.Hint) : apiValidationResponse2.Message);
				}
			}
		}
		catch (JsonException)
		{
		}
		return (success: false, message: item, username: null);
	}

	internal static void ForceHeartbeatWakeup()
	{
		try
		{
			CancellationTokenSource cancellationTokenSource = Interlocked.CompareExchange(ref _sleepTokenSource, null, null);
			if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
			{
				cancellationTokenSource.Cancel();
			}
		}
		catch
		{
		}
	}

	private static bool SafeIsInMeeting()
	{
		try
		{
			return (Object)(object)MeetingHud.Instance != (Object)null;
		}
		catch
		{
			return false;
		}
	}

	private static string SafeGetCurrentRole()
	{
		try
		{
			return RealtimeConnection.GetCurrentRoleForHeartbeat();
		}
		catch
		{
			return "";
		}
	}

	internal static async void StartHeartbeat()
	{
		if (!_enableHeartbeat || Interlocked.CompareExchange(ref _isHeartbeatRunningInt, 1, 0) != 0)
		{
			return;
		}
		_debugSessionDeathLogged = false;
		CancellationTokenSource localCts = new CancellationTokenSource();
		_heartbeatCancellation = localCts;
		int consecutiveFailures = 0;
		bool isFirstIteration = true;
		try
		{
			RealtimeConnection.Connect(ApiBaseUrl, CurrentKey, CurrentSessionToken.ToString());
		}
		catch
		{
		}
		try
		{
			await Task.Run(async delegate
			{
				_debugHeartbeatMsg = "hb_start";
				while (IsSessionValid() && !localCts.Token.IsCancellationRequested)
				{
					try
					{
						int millisecondsDelay;
						if (PlayerPickMenu.PendingImmediateHeartbeat)
						{
							millisecondsDelay = 100;
							PlayerPickMenu.PendingImmediateHeartbeat = false;
						}
						else if (isFirstIteration)
						{
							millisecondsDelay = 3000;
							isFirstIteration = false;
						}
						else
						{
							millisecondsDelay = ((consecutiveFailures > 0) ? Math.Min(5000 + consecutiveFailures * 5000, 60000) : 45000);
						}
						CancellationTokenSource newSleepCts = new CancellationTokenSource();
						CancellationTokenSource cancellationTokenSource = Interlocked.Exchange(ref _sleepTokenSource, newSleepCts);
						try
						{
							cancellationTokenSource?.Dispose();
						}
						catch
						{
						}
						using (CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(localCts.Token, newSleepCts.Token))
						{
							try
							{
								await Task.Delay(millisecondsDelay, linkedCts.Token);
							}
							catch (TaskCanceledException)
							{
							}
						}
						try
						{
							newSleepCts.Dispose();
						}
						catch
						{
						}
						Interlocked.CompareExchange(ref _sleepTokenSource, null, newSleepCts);
						if (!IsSessionValid() || localCts.Token.IsCancellationRequested)
						{
							_debugHeartbeatMsg = "hb_break";
							break;
						}
						string hwid = GetHardwareId();
						object playerData = null;
						bool isHost = false;
						bool isInGame = false;
						byte localPlayerId = byte.MaxValue;
						object uiState = null;
						object banMenuState = null;
						object banMenuUiState = null;
						object cheatsState = null;
						object cheatsUiState = null;
						object alivePlayers = null;
						object spoofingState = null;
						object settingsState = null;
						object sabotageState = null;
						bool isInMeeting = false;
						string currentRole = "";
						string httpAttProof = "";
						TaskCompletionSource<bool> dataTcs = new TaskCompletionSource<bool>();
						ActionPermitSystem.EnqueueMainThread(delegate
						{
							try
							{
								playerData = PlayerPickMenu.CollectPlayerDataForServer();
							}
							catch
							{
							}
							try
							{
								(isHost, isInGame, localPlayerId) = PlayerPickMenu.GetGameContext();
							}
							catch
							{
							}
							try
							{
								uiState = PlayerPickMenu.GetUIState();
							}
							catch
							{
							}
							try
							{
								banMenuState = BanMenu.GetBanMenuState();
							}
							catch
							{
							}
							try
							{
								banMenuUiState = BanMenu.GetUIState();
							}
							catch
							{
							}
							try
							{
								cheatsState = CheatManager.GetCheatsState();
							}
							catch
							{
							}
							try
							{
								cheatsUiState = CheatManager.GetCheatsUiState();
							}
							catch
							{
							}
							try
							{
								alivePlayers = CheatManager.GetAlivePlayersForServer();
							}
							catch
							{
							}
							try
							{
								spoofingState = SpoofingMenu.GetSpoofingState();
							}
							catch
							{
							}
							try
							{
								settingsState = SettingsTab.GetSettingsState();
							}
							catch
							{
							}
							try
							{
								sabotageState = CheatManager.GetSabotageState();
							}
							catch
							{
							}
							try
							{
								isInMeeting = (Object)(object)MeetingHud.Instance != (Object)null;
							}
							catch
							{
							}
							try
							{
								currentRole = RealtimeConnection.GetCurrentRoleForHeartbeat();
							}
							catch
							{
							}
							try
							{
								httpAttProof = ActionPermitSystem.ComputeAttestationProof();
							}
							catch
							{
							}
							dataTcs.TrySetResult(result: true);
						});
						try
						{
							await Task.WhenAny(dataTcs.Task, Task.Delay(5000, localCts.Token));
						}
						catch (OperationCanceledException)
						{
							break;
						}
						if (localCts.Token.IsCancellationRequested)
						{
							break;
						}
						string content2 = JsonSerializer.Serialize(new
						{
							key = CurrentKey,
							token = CurrentSessionToken,
							hwid = hwid,
							clientSessionId = RealtimeConnection.ClientSessionId,
							heartbeat_nonce = _serverHeartbeatNonce,
							attestation_proof = httpAttProof,
							players = playerData,
							isHost = isHost,
							isInGame = isInGame,
							localPlayerId = (int)localPlayerId,
							uiState = uiState,
							banMenuState = banMenuState,
							banMenuUiState = banMenuUiState,
							cheatsState = cheatsState,
							cheatsUiState = cheatsUiState,
							alivePlayers = alivePlayers,
							spoofingState = spoofingState,
							settingsState = settingsState,
							sabotageState = sabotageState,
							menuWidth = GhostUI.CurrentWindowWidth,
							menuHeight = GhostUI.CurrentWindowHeight,
							isInMeeting = isInMeeting,
							currentRole = currentRole
						});
						using StringContent content = new StringContent(content2, Encoding.UTF8, "application/json");
						using (HttpResponseMessage response = await httpClient.PostAsync(ApiBaseUrl + "/heartbeat", content, localCts.Token))
						{
							if (response.IsSuccessStatusCode)
							{
								consecutiveFailures = 0;
								try
								{
									RealtimeConnection.ResetReconnectAttempts();
								}
								catch
								{
								}
								lock (_validationLock)
								{
									_lastServerCheck = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
								}
								ApiValidationResponse apiValidationResponse = JsonSerializer.Deserialize<ApiValidationResponse>(await response.Content.ReadAsStringAsync(), _jsonOpts);
								long valueOrDefault = (apiValidationResponse?.NewToken).GetValueOrDefault();
								EncryptedPayload encryptedPayload = apiValidationResponse?.UiPayload;
								if (encryptedPayload == null && apiValidationResponse?.ExtensionData != null && apiValidationResponse.ExtensionData.TryGetValue("ui_payload", out var value) && value.ValueKind == JsonValueKind.Object)
								{
									try
									{
										encryptedPayload = JsonSerializer.Deserialize<EncryptedPayload>(value.GetRawText(), _jsonOpts);
									}
									catch
									{
									}
								}
								if (encryptedPayload != null && !string.IsNullOrEmpty(apiValidationResponse.SessionKey))
								{
									try
									{
										ServerData.ParseFromEncryptedPayload(new ServerData.EncryptedPayload
										{
											Ciphertext = encryptedPayload.Ciphertext,
											Iv = encryptedPayload.Iv,
											Tag = encryptedPayload.Tag
										}, apiValidationResponse.SessionKey, valueOrDefault, isHeartbeat: true);
									}
									catch (Exception ex3)
									{
										try
										{
											ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
											if (((instance != null) ? ((BasePlugin)instance).Log : null) != null)
											{
												ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
												bool flag;
												BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(35, 1, ref flag);
												if (flag)
												{
													((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ModKeyValidator] UI Parse failed: ");
													((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(ex3.Message);
												}
												log.LogWarning(val);
											}
										}
										catch
										{
										}
										ServerData.SetLoaded(loaded: false);
									}
								}
								else
								{
									_ = 0;
								}
								if (apiValidationResponse != null && !string.IsNullOrEmpty(apiValidationResponse.SessionProof))
								{
									UpdateProof(apiValidationResponse.SessionProof, apiValidationResponse.ProofSeed.GetValueOrDefault(), apiValidationResponse.ProofExpires.GetValueOrDefault(), apiValidationResponse.ProofTtlSeconds.GetValueOrDefault());
								}
								if (apiValidationResponse != null && !string.IsNullOrEmpty(apiValidationResponse.RenderKey))
								{
									ServerGate.UpdateRenderPermission(apiValidationResponse.RenderKey, apiValidationResponse.RenderExpires.GetValueOrDefault(), apiValidationResponse.RenderNonce.GetValueOrDefault(), apiValidationResponse.RenderTtlSeconds.GetValueOrDefault());
								}
								if (apiValidationResponse != null)
								{
									if (!string.IsNullOrEmpty(apiValidationResponse.KeyDisplay))
									{
										KeyDisplay = apiValidationResponse.KeyDisplay;
									}
									if (apiValidationResponse.TimeDisplay != null)
									{
										TimeDisplay = apiValidationResponse.TimeDisplay;
									}
								}
								if (apiValidationResponse != null && !string.IsNullOrEmpty(apiValidationResponse.HeartbeatNonce))
								{
									_serverHeartbeatNonce = apiValidationResponse.HeartbeatNonce;
								}
								if (apiValidationResponse != null && !string.IsNullOrEmpty(apiValidationResponse.AttestationSeed) && apiValidationResponse.AttestationMethods != null && apiValidationResponse.AttestationMethods.Length != 0)
								{
									try
									{
										ActionPermitSystem.UpdateAttestation(apiValidationResponse.AttestationSeed, apiValidationResponse.AttestationMethods);
									}
									catch
									{
									}
									if (RealtimeConnection.IsConnected)
									{
										try
										{
											RealtimeConnection.ForceReconnect();
										}
										catch
										{
										}
									}
								}
								goto end_IL_0529;
							}
							switch ((int)response.StatusCode)
							{
							case 403:
								try
								{
									ApiValidationResponse apiValidationResponse2 = JsonSerializer.Deserialize<ApiValidationResponse>(await response.Content.ReadAsStringAsync(), _jsonOpts);
									if (apiValidationResponse2 != null && "discord_left".Equals(apiValidationResponse2.Reason, StringComparison.OrdinalIgnoreCase))
									{
										DiscordRevoked = true;
										RevokeReason = "discord_left";
									}
									else if (apiValidationResponse2 != null && "expired".Equals(apiValidationResponse2.Reason, StringComparison.OrdinalIgnoreCase))
									{
										RevokeReason = "expired";
										LastValidationMessage = "Your Premium key has expired.";
									}
									else if (apiValidationResponse2 != null && "hwid_mismatch".Equals(apiValidationResponse2.Reason, StringComparison.OrdinalIgnoreCase))
									{
										RevokeReason = "hwid_mismatch";
									}
								}
								catch
								{
								}
								PendingResetRequest = true;
								return;
							case 426:
								try
								{
									LastValidationMessage = JsonSerializer.Deserialize<ApiValidationResponse>(await response.Content.ReadAsStringAsync(), _jsonOpts)?.Message ?? "Security update required.";
									RevokeReason = "force_update";
								}
								catch
								{
									LastValidationMessage = "Security update required.";
								}
								PendingResetRequest = true;
								return;
							default:
								consecutiveFailures++;
								break;
							}
							goto end_IL_0503;
							end_IL_0529:;
						}
						end_IL_0503:;
					}
					catch (TaskCanceledException)
					{
						if (localCts.IsCancellationRequested)
						{
							break;
						}
						consecutiveFailures++;
					}
					catch (OperationCanceledException)
					{
						if (localCts.IsCancellationRequested)
						{
							break;
						}
						consecutiveFailures++;
					}
					catch (Exception)
					{
						consecutiveFailures++;
					}
				}
			}, localCts.Token);
		}
		catch
		{
		}
		finally
		{
			_debugHeartbeatMsg = "hb_stop";
			Interlocked.CompareExchange(ref _heartbeatCancellation, null, localCts);
			Interlocked.Exchange(ref _isHeartbeatRunningInt, 0);
			try
			{
				localCts.Dispose();
			}
			catch
			{
			}
		}
	}

	internal static void StopHeartbeat()
	{
		try
		{
			_heartbeatCancellation?.Cancel();
		}
		catch
		{
		}
	}

	internal static void UpdateValidationState(bool isValidated, string message, string username)
	{
		if (isValidated)
		{
			if (SessionToken != 0L && CurrentSessionToken != 0L && !string.IsNullOrEmpty(CurrentKey))
			{
				UpdateValidationState(success: true, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				SyncValidationState();
			}
		}
		else
		{
			SessionToken = 0L;
			CurrentSessionToken = 0L;
			SetServiceContext(value: false, 0L);
			_textureMemoryPool = 0u;
			UpdateValidationState(success: false, 0L);
		}
		LastValidationMessage = message;
		if (!string.IsNullOrEmpty(username))
		{
			ValidatedUsername = username;
		}
		UpdateFormattedString();
		try
		{
			SaveValidationState(isValidated, message, ValidatedUsername);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(Object.op_Implicit("[ModKeyValidator] SaveValidationState failed (non-fatal): " + ex.Message));
		}
	}

	private static void UpdateFormattedString()
	{
		FormattedDisplayString = ValidatedUsername + " -- https://crewcore.online/";
	}

	public static string TruncateMessage(string message, int maxLength = 50)
	{
		if (!string.IsNullOrEmpty(message))
		{
			if (message.Length > maxLength)
			{
				return message.Substring(0, maxLength) + "...";
			}
			return message;
		}
		return "";
	}

	internal static void LoadValidationState()
	{
		LastValidationMessage = "Enter your activation key.";
		string playerPrefsKeyActivated = GetPlayerPrefsKeyActivated();
		if (PlayerPrefs.HasKey(playerPrefsKeyActivated + "_KeyDisplay"))
		{
			KeyDisplay = PlayerPrefs.GetString(playerPrefsKeyActivated + "_KeyDisplay", "");
		}
		if (PlayerPrefs.HasKey(playerPrefsKeyActivated + "_TimeDisplay"))
		{
			TimeDisplay = PlayerPrefs.GetString(playerPrefsKeyActivated + "_TimeDisplay", "");
		}
	}

	internal static void ResetValidationState()
	{
		if (ModMenuCrewPlugin.Instance != null)
		{
			((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogInfo((object)"[ModKeyValidator] Validation state reset.");
		}
		StopHeartbeat();
		ClearProof();
		ServerGate.Revoke();
		ResetTicks();
		PendingResetRequest = false;
		SessionToken = 0L;
		CurrentSessionToken = 0L;
		__svc_internal = false;
		__svc_checksum = 0;
		_textureMemoryPool = 0u;
		_rtx = 0L;
		_vcs = 0;
		_xf = 0;
		_hv = 0u;
		_lastServerCheck = 0L;
		LastValidationMessage = "Enter your activation key.";
		ValidatedUsername = "";
		CurrentKey = "";
		KeyDisplay = "";
		TimeDisplay = "";
		UpdateFormattedString();
		string playerPrefsKeyActivated = GetPlayerPrefsKeyActivated();
		PlayerPrefs.DeleteKey(playerPrefsKeyActivated);
		PlayerPrefs.DeleteKey(playerPrefsKeyActivated + "_Message");
		PlayerPrefs.DeleteKey(playerPrefsKeyActivated + "_Username");
		PlayerPrefs.DeleteKey(playerPrefsKeyActivated + "_IsPremium");
		PlayerPrefs.DeleteKey(playerPrefsKeyActivated + "_KeyDisplay");
		PlayerPrefs.DeleteKey(playerPrefsKeyActivated + "_TimeDisplay");
		PlayerPrefs.DeleteKey(playerPrefsKeyActivated + "_KeyType");
		PlayerPrefs.DeleteKey(playerPrefsKeyActivated + "_ExpiresAt");
		PlayerPrefs.Save();
		ModMenuCrewPlugin instance = ModMenuCrewPlugin.Instance;
		if (((instance != null) ? ((BasePlugin)instance).Log : null) != null)
		{
			((BasePlugin)ModMenuCrewPlugin.Instance).Log.LogInfo((object)"[ModKeyValidator] Validation state reset.");
		}
	}

	private static void SaveValidationState(bool isValidated, string message, string username)
	{
		PlayerPrefs.SetInt(GetPlayerPrefsKeyActivated(), isValidated ? 1 : 0);
		PlayerPrefs.SetString(GetPlayerPrefsKeyActivated() + "_Message", message);
		PlayerPrefs.SetString(GetPlayerPrefsKeyActivated() + "_Username", username);
		PlayerPrefs.SetString(GetPlayerPrefsKeyActivated() + "_KeyDisplay", KeyDisplay ?? "");
		PlayerPrefs.SetString(GetPlayerPrefsKeyActivated() + "_TimeDisplay", TimeDisplay ?? "");
		PlayerPrefs.Save();
		if (isValidated)
		{
			if (SessionToken == 0L && !string.IsNullOrEmpty(CurrentKey))
			{
				SessionToken = CalculateSessionToken(CurrentKey);
			}
		}
		else
		{
			SessionToken = 0L;
			CurrentSessionToken = 0L;
		}
		LastValidationMessage = message;
		ValidatedUsername = username;
		UpdateFormattedString();
	}
}
