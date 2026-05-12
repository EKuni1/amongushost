using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ModMenuCrew.Monitoring;
using ModMenuCrew.Networking;

namespace ModMenuCrew;

public static class IntegrityGuard
{
	private static byte[] _derivedKey;

	private static int _isIntactXor = 0;

	private const int INTACT_SENTINEL = 2100251278;

	private static long _verifyTagXor = 0L;

	private const long VERIFY_TAG_SENTINEL = -8197839618022862761L;

	private static bool _initialized = false;

	private static readonly byte[] _expectedHash = new byte[32]
	{
		154, 8, 120, 113, 117, 210, 172, 238, 250, 225,
		204, 201, 151, 17, 115, 42, 28, 170, 249, 155,
		122, 196, 24, 9, 34, 45, 109, 140, 249, 131,
		58, 178
	};

	public static bool IsIntact
	{
		get
		{
			if ((_isIntactXor ^ 0x7D2F4A8E) != 1)
			{
				return false;
			}
			if (_derivedKey == null || _derivedKey.Length < 8)
			{
				return false;
			}
			long num = BitConverter.ToInt64(_derivedKey, 0) ^ BitConverter.ToInt64(_derivedKey, _derivedKey.Length - 8);
			return (_verifyTagXor ^ -8197839618022862761L) == num;
		}
	}

	public static void Initialize()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		try
		{
			using IncrementalHash incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
			MethodBase[] array = new MethodBase[15]
			{
				typeof(ActionPermitSystem).GetMethod("OnServerApproval", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(ActionPermitSystem).GetMethod("RequestExecution", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(CertificatePinner).GetMethod("ValidateServerCertificate", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(GhostUI).GetMethod("Execute", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(GhostUI).GetMethod("ValidateTimestampAntiReplay", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(ModMenuCrewPlugin).GetMethod("CheckForSelfPatches", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(AntiTamper).GetMethod("Update", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(ModKeyValidator).GetProperty("IsPremium", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetGetMethod(nonPublic: true),
				typeof(ServerData).GetMethod("CalculateIntegrity", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(ServerData).GetMethod("GetStoredIntegrityHash", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(ModKeyValidator).GetMethod("V", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(GhostUI).GetMethod("CheckToken", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(IntegrityGuard).GetProperty("IsIntact", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetGetMethod(nonPublic: true),
				typeof(ServerData).GetMethod("TriggerSilentDenial", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				typeof(ServerGate).GetMethod("CanRender", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			};
			foreach (MethodBase methodBase in array)
			{
				if (methodBase == null)
				{
					continue;
				}
				MethodBody methodBody = methodBase.GetMethodBody();
				if (methodBody != null)
				{
					byte[] iLAsByteArray = methodBody.GetILAsByteArray();
					if (iLAsByteArray != null)
					{
						incrementalHash.AppendData(iLAsByteArray);
					}
				}
			}
			byte[] hashAndReset = incrementalHash.GetHashAndReset();
			_derivedKey = HKDF.DeriveKey(HashAlgorithmName.SHA256, hashAndReset, 32, Encoding.UTF8.GetBytes("MMC_Guard_7F3E"), Encoding.UTF8.GetBytes("AK_v3_8D2C"));
			byte[] array2 = SHA256.HashData(_derivedKey);
			bool flag = true;
			for (int j = 0; j < _expectedHash.Length; j++)
			{
				if (_expectedHash[j] != 0)
				{
					flag = false;
					break;
				}
			}
			bool num = !flag && CryptographicOperations.FixedTimeEquals(array2, _expectedHash);
			_isIntactXor = (num ? 1 : 0) ^ 0x7D2F4A8E;
			if (num && _derivedKey != null && _derivedKey.Length >= 8)
			{
				_verifyTagXor = BitConverter.ToInt64(_derivedKey, 0) ^ BitConverter.ToInt64(_derivedKey, _derivedKey.Length - 8) ^ -8197839618022862761L;
			}
		}
		catch
		{
			_isIntactXor = 0;
			_verifyTagXor = 0L;
		}
	}
}
