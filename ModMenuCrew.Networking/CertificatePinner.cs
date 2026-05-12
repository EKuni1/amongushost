using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ModMenuCrew.Networking;

public static class CertificatePinner
{
	private static readonly HashSet<string> _pins = new HashSet<string>(StringComparer.Ordinal) { "T6VlexaJ/mwSs9GKinH9ZLOf/VkRtYD8iSPX+oi0g5A=", "SrDrc1Ix/NbOyBtTGt+iTnZUtR+rvG9ZFyWNfo+o3Gw=" };

	private static readonly bool _pinningEnabled = true;

	private static int _failureCountXor = -1942134994;

	private const int FAILURE_SENTINEL = -1942134994;

	private const int MAX_FAILURES_BEFORE_BAN = 3;

	private const int MAX_PINS = 5;

	private const string ORIGINAL_PIN = "T6VlexaJ/mwSs9GKinH9ZLOf/VkRtYD8iSPX+oi0g5A=";

	internal static volatile string _lastDiag = null;

	private static int FailureCount => _failureCountXor ^ -1942134994;

	private static void SetFailureCount(int value)
	{
		_failureCountXor = value ^ -1942134994;
	}

	public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		if (!_pinningEnabled)
		{
			return true;
		}
		if (certificate == null)
		{
			return false;
		}
		try
		{
			if (ModKeyValidator.IsRunningUnderWine)
			{
				_lastDiag = "[CertPinner] SKIPPED: Running under Wine/Proton (crypto APIs unsafe)";
				return true;
			}
		}
		catch
		{
		}
		bool num = (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0;
		bool flag = (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0;
		if (num || flag)
		{
			return false;
		}
		bool flag2 = sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors;
		try
		{
			using X509Certificate2 x509Certificate = new X509Certificate2(certificate);
			List<X509Certificate2> list = new List<X509Certificate2>();
			if (chain != null && chain.ChainElements != null && chain.ChainElements.Count > 0)
			{
				foreach (X509ChainElement chainElement in chain.ChainElements)
				{
					try
					{
						list.Add(chainElement.Certificate);
					}
					catch
					{
					}
				}
			}
			if (list.Count == 0)
			{
				list.Add(x509Certificate);
			}
			bool flag3 = false;
			bool flag4 = false;
			List<string> list2 = new List<string>(list.Count);
			foreach (X509Certificate2 item in list)
			{
				byte[] array = null;
				string value = "unknown";
				try
				{
					using RSA rSA = item.GetRSAPublicKey();
					if (rSA != null)
					{
						value = "RSA";
						array = rSA.ExportSubjectPublicKeyInfo();
					}
					else
					{
						using ECDsa eCDsa = item.GetECDsaPublicKey();
						if (eCDsa != null)
						{
							value = "ECDSA";
							array = eCDsa.ExportSubjectPublicKeyInfo();
						}
					}
				}
				catch
				{
					continue;
				}
				if (array != null)
				{
					string text;
					using (SHA256 sHA = SHA256.Create())
					{
						text = Convert.ToBase64String(sHA.ComputeHash(array));
					}
					list2.Add($"{value}:{text}:{item.Subject}");
					if (_pins.Contains(text))
					{
						flag3 = true;
					}
					if (item.Subject.IndexOf("O=Google Trust Services", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						flag4 = true;
					}
				}
			}
			_lastDiag = $"[CertPinner] chainLen={list.Count} leafSubj={x509Certificate.Subject} spkis=[{string.Join(" | ", list2)}]";
			if (!_pins.Contains("T6VlexaJ/mwSs9GKinH9ZLOf/VkRtYD8iSPX+oi0g5A="))
			{
				_lastDiag += " | REJECTED: ORIGINAL_PIN missing (reflection attack?)";
				ServerData.TriggerSilentDenial();
				return false;
			}
			if (flag3)
			{
				_lastDiag += " | ACCEPTED: SPKI pin match in chain";
				SetFailureCount(0);
				return true;
			}
			if (flag4 && sslPolicyErrors == SslPolicyErrors.None)
			{
				_lastDiag += " | ACCEPTED: trusted issuer (Google Trust Services) in WebPKI-validated chain";
				SetFailureCount(0);
				return true;
			}
			_lastDiag += " | REJECTED: no SPKI match, no trusted issuer (or chain errors block issuer fallback)";
			if (!flag2)
			{
				SetFailureCount(FailureCount + 1);
				if (FailureCount >= 3)
				{
					ServerData.TriggerSilentDenial();
				}
			}
			return false;
		}
		catch (Exception ex)
		{
			_lastDiag = "[CertPinner] EXCEPTION: " + ex.GetType().Name + ": " + ex.Message;
			return false;
		}
	}

	internal static void UpdatePinsFromServer(string[] serverPins)
	{
		if (serverPins == null || serverPins.Length == 0 || _pins.Count == 0 || _pins.Count >= 5)
		{
			return;
		}
		foreach (string text in serverPins)
		{
			if (_pins.Count < 5)
			{
				if (!string.IsNullOrEmpty(text) && text.Length == 44)
				{
					_pins.Add(text);
				}
				continue;
			}
			break;
		}
	}
}
