using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Il2CppSystem;
using ModMenuCrew.Features;
using ModMenuCrew.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModMenuCrew.Monitoring;

public static class AntiTamper
{
	private static float _nextScanTime = 0f;

	private static float _lastSceneChangeTime = 0f;

	private const float SCENE_CHANGE_GRACE = 8f;

	private static string _lastSceneName = null;

	private static bool _baselineNeedsRefresh = false;

	private static float _initTime = -1f;

	private const float SNAPSHOT_DELAY = 8f;

	private static bool _snapshotReady = false;

	private static int _tamperDetectedXor = 1569341298;

	private const int TAMPER_SENTINEL = 1569341298;

	private static int _scanCount = 0;

	private static bool _harmonyResolved = false;

	private static int _harmonyResolveAttempts = 0;

	private static MethodInfo _getPatchInfoMethod = null;

	private static PropertyInfo _prefixesProp = null;

	private static PropertyInfo _postfixesProp = null;

	private static PropertyInfo _transpilersProp = null;

	private static PropertyInfo _finalizersProp = null;

	private static readonly KeyValuePair<Type, string[]>[] CriticalMethods = new KeyValuePair<Type, string[]>[10]
	{
		new KeyValuePair<Type, string[]>(typeof(ModKeyValidator), new string[6] { "V", "IsSessionValid", "ResetValidationState", "GetDerivedApiUrl", "VerifyAllowedHostnames", "get_IsPremium" }),
		new KeyValuePair<Type, string[]>(typeof(ServerData), new string[5] { "IsTabEnabled", "get_IsLoaded", "TriggerSilentDenial", "ParseFromEncryptedPayload", "DecryptPayload" }),
		new KeyValuePair<Type, string[]>(typeof(ModMenuCrewPlugin), new string[3] { "RuntimeSecurityCheck", "CheckForSelfPatches", "VerifyDnsResolutionAsync" }),
		new KeyValuePair<Type, string[]>(typeof(GhostUI), new string[5] { "Execute", "CheckToken", "VerifyRsaSignature", "ValidateTimestampAntiReplay", "VerifyBytecodeSignatureV5" }),
		new KeyValuePair<Type, string[]>(typeof(ActionPermitSystem), new string[5] { "RequestExecution", "OnServerApproval", "CleanupExpired", "SetActionRegistry", "ClearRegistry" }),
		new KeyValuePair<Type, string[]>(typeof(CertificatePinner), new string[1] { "ValidateServerCertificate" }),
		new KeyValuePair<Type, string[]>(typeof(GameCheats), new string[4] { "ToggleCH", "ToggleCL", "ToggleSyncA", "ToggleSyncB" }),
		new KeyValuePair<Type, string[]>(typeof(IntegrityGuard), new string[2] { "get_IsIntact", "Initialize" }),
		new KeyValuePair<Type, string[]>(typeof(MethodBody), new string[1] { "GetILAsByteArray" }),
		new KeyValuePair<Type, string[]>(typeof(RealtimeConnection), new string[1] { "Connect" })
	};

	private static HashSet<string> _baselineAssemblies = null;

	private static readonly Random _jitter = new Random();

	private static bool _lastILScanFailed = false;

	private static (MethodBase method, string name, int minILSize)[] _cachedILMethods = null;

	private static MethodBase[] _cachedNativeMethods = null;

	private static int _updateCallCount = 0;

	private static float _lastCallCountCheck = 0f;

	private static int _lastCallCountValue = 0;

	private static int _baselineAssemblyCount = 0;

	private const int ASSEMBLY_GROWTH_TOLERANCE = 15;

	private static IntPtr[] _baselinePointers = null;

	private static byte[][] _baselineBytes = null;

	private static int[] _rebaselineCount = null;

	private const int MAX_REBASELINES = 5;

	private static int _corruptionLevel = 0;

	private static int _consecutiveCleanScans = 0;

	public static bool IsTamperDetected => (_tamperDetectedXor ^ 0x5D8A3F72) != 0;

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, out bool isDebuggerPresent);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetCurrentProcess();

	[DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
	private static extern IntPtr FindWindowA(string lpClassName, string lpWindowName);

	public static void Update()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		_updateCallCount++;
		if (((uint)_tamperDetectedXor ^ 0x5D8A3F72u) != 0 && _corruptionLevel >= 2)
		{
			return;
		}
		if (_initTime < 0f)
		{
			_initTime = Time.realtimeSinceStartup;
		}
		if ((!_snapshotReady && Time.realtimeSinceStartup - _initTime < 8f) || Time.realtimeSinceStartup < _nextScanTime)
		{
			return;
		}
		try
		{
			Scene activeScene = SceneManager.GetActiveScene();
			string name = activeScene.name;
			if (_lastSceneName != null && name != _lastSceneName)
			{
				_lastSceneChangeTime = Time.realtimeSinceStartup;
				_baselineNeedsRefresh = true;
			}
			_lastSceneName = name;
		}
		catch
		{
		}
		float num;
		if (_scanCount == 0)
		{
			num = 3f;
		}
		else if (_scanCount < 5)
		{
			num = 5f + (float)(_jitter.NextDouble() * 5.0);
		}
		else
		{
			num = 7f + (float)(_jitter.NextDouble() * 13.0);
			if (_jitter.NextDouble() < 0.1)
			{
				num = 1f;
			}
		}
		_nextScanTime = Time.realtimeSinceStartup + num;
		if (_scanCount < int.MaxValue)
		{
			_scanCount++;
		}
		RunScan();
	}

	private static void RunScan()
	{
		try
		{
			if (_lastCallCountCheck > 0f && Time.realtimeSinceStartup - _lastCallCountCheck > 30f)
			{
				if (_updateCallCount == _lastCallCountValue)
				{
					ServerData.TriggerSilentDenial();
					return;
				}
				_lastCallCountValue = _updateCallCount;
				_lastCallCountCheck = Time.realtimeSinceStartup;
			}
			else if (_lastCallCountCheck == 0f)
			{
				_lastCallCountCheck = Time.realtimeSinceStartup;
				_lastCallCountValue = _updateCallCount;
			}
			try
			{
				MethodInfo method = typeof(AntiTamper).GetMethod("TriggerDetect", BindingFlags.Static | BindingFlags.NonPublic);
				if (method != null)
				{
					MethodBody methodBody = method.GetMethodBody();
					if (methodBody == null)
					{
						goto IL_00a7;
					}
					byte[]? iLAsByteArray = methodBody.GetILAsByteArray();
					if (((iLAsByteArray != null) ? iLAsByteArray.Length : 0) < 8)
					{
						goto IL_00a7;
					}
				}
				goto end_IL_006b;
				IL_00a7:
				ServerData.TriggerSilentDenial();
				return;
				end_IL_006b:;
			}
			catch
			{
			}
			try
			{
				if (Debugger.IsAttached)
				{
					TriggerDetect("DebuggerAttached");
					return;
				}
			}
			catch
			{
			}
			try
			{
				if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DNSPY_EXTENSION_SEARCH_PATHS")))
				{
					TriggerDetect("DnSpyEnvironment");
					return;
				}
			}
			catch
			{
			}
			try
			{
				if (CheckRemoteDebuggerPresent(GetCurrentProcess(), out var isDebuggerPresent) && isDebuggerPresent)
				{
					TriggerDetect("RemoteDebugger");
					return;
				}
			}
			catch
			{
			}
			try
			{
				long timestamp = Stopwatch.GetTimestamp();
				int num = 0;
				for (int i = 0; i < 1000; i++)
				{
					num = num * 31 + i;
				}
				if ((double)(Stopwatch.GetTimestamp() - timestamp) * 1000.0 / (double)Stopwatch.Frequency > 50.0 && _scanCount > 3)
				{
					TriggerDetect("TimingAnomaly");
					return;
				}
			}
			catch
			{
			}
			try
			{
				if (FindWindowA(null, "dnSpy") != IntPtr.Zero || FindWindowA(null, "x64dbg") != IntPtr.Zero || FindWindowA(null, "x32dbg") != IntPtr.Zero || FindWindowA(null, "ILSpy") != IntPtr.Zero || FindWindowA(null, "dotPeek") != IntPtr.Zero || FindWindowA(null, "Cheat Engine") != IntPtr.Zero)
				{
					TriggerDetect("DebugToolWindow");
					return;
				}
			}
			catch
			{
			}
			if (!_harmonyResolved)
			{
				ResolveHarmony();
				if (_getPatchInfoMethod != null || ++_harmonyResolveAttempts > 10)
				{
					_harmonyResolved = true;
				}
			}
			if (_getPatchInfoMethod != null)
			{
				CheckCriticalMethods();
			}
			CheckSuspiciousAssemblies(Time.realtimeSinceStartup - _lastSceneChangeTime < 8f);
			if (Time.realtimeSinceStartup - _lastSceneChangeTime < 8f)
			{
				return;
			}
			CheckILGutting();
			try
			{
				EnsureMethodCachesResolved();
				if (_cachedILMethods != null && _cachedILMethods.Length >= 8)
				{
					MethodBase item = _cachedILMethods[9].method;
					MethodBase item2 = _cachedILMethods[6].method;
					if (item != null && item2 != null)
					{
						byte[] array = item.GetMethodBody()?.GetILAsByteArray();
						byte[] array2 = item2.GetMethodBody()?.GetILAsByteArray();
						if (array != null && array2 != null && array.Length == array2.Length && array.Length != 0)
						{
							bool flag = true;
							for (int j = 0; j < array.Length; j++)
							{
								if (array[j] != array2[j])
								{
									flag = false;
									break;
								}
							}
							if (flag)
							{
								TriggerDetect("GetILAsByteArray:Hooked");
								return;
							}
						}
					}
				}
			}
			catch
			{
			}
			if (_baselineNeedsRefresh)
			{
				_baselineNeedsRefresh = false;
				_baselinePointers = null;
				_baselineBytes = null;
				_rebaselineCount = null;
				_baselineAssemblyCount = AppDomain.CurrentDomain.GetAssemblies().Length;
				_baselineAssemblies = null;
			}
			CheckNativeDetours();
			if (_corruptionLevel != 1 || (_tamperDetectedXor ^ 0x5D8A3F72) == 0)
			{
				return;
			}
			_consecutiveCleanScans++;
			if (_consecutiveCleanScans < 3)
			{
				return;
			}
			_corruptionLevel = 0;
			_tamperDetectedXor = 1569341298;
			ServerData.CancelDelayedDenial();
			try
			{
				Debug.LogWarning(Object.op_Implicit("[ANTITAMPER] False positive cancelled after 3 clean scans"));
			}
			catch
			{
			}
		}
		catch
		{
		}
	}

	private static void ResolveHarmony()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			string name = assembly.GetName().Name;
			if (name == null || (!string.Equals(name, "0Harmony", StringComparison.OrdinalIgnoreCase) && !string.Equals(name, "HarmonyLib", StringComparison.OrdinalIgnoreCase) && name.IndexOf("Harmony", StringComparison.OrdinalIgnoreCase) < 0))
			{
				continue;
			}
			try
			{
				Type type = assembly.GetType("HarmonyLib.Harmony");
				if (!(type == null))
				{
					_getPatchInfoMethod = type.GetMethod("GetPatchInfo", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(MethodBase) }, null);
					if (!(_getPatchInfoMethod == null))
					{
						Type returnType = _getPatchInfoMethod.ReturnType;
						_prefixesProp = returnType.GetProperty("Prefixes");
						_postfixesProp = returnType.GetProperty("Postfixes");
						_transpilersProp = returnType.GetProperty("Transpilers");
						_finalizersProp = returnType.GetProperty("Finalizers");
						break;
					}
				}
			}
			catch
			{
			}
		}
	}

	private static void CheckCriticalMethods()
	{
		for (int i = 0; i < CriticalMethods.Length; i++)
		{
			Type key = CriticalMethods[i].Key;
			string[] value = CriticalMethods[i].Value;
			for (int j = 0; j < value.Length; j++)
			{
				MethodInfo method = key.GetMethod(value[j], BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (method == null)
				{
					continue;
				}
				try
				{
					object obj = _getPatchInfoMethod.Invoke(null, new object[1] { method });
					if (obj == null || !HasActivePatches(obj))
					{
						continue;
					}
					TriggerDetect("HarmonyPatch:" + key.Name + "." + value[j]);
					return;
				}
				catch
				{
				}
			}
		}
	}

	private static bool HasActivePatches(object patchInfo)
	{
		try
		{
			if (_prefixesProp != null && _prefixesProp.GetValue(patchInfo) is ICollection { Count: >0 })
			{
				return true;
			}
			if (_postfixesProp != null && _postfixesProp.GetValue(patchInfo) is ICollection { Count: >0 })
			{
				return true;
			}
			if (_transpilersProp != null && _transpilersProp.GetValue(patchInfo) is ICollection { Count: >0 })
			{
				return true;
			}
			if (_finalizersProp != null && _finalizersProp.GetValue(patchInfo) is ICollection { Count: >0 })
			{
				return true;
			}
		}
		catch
		{
			return true;
		}
		return false;
	}

	private static void CheckSuspiciousAssemblies(bool skipCountCheck = false)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		if (_baselineAssemblies == null)
		{
			_baselineAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < assemblies.Length; i++)
			{
				string text = null;
				try
				{
					text = assemblies[i].GetName()?.Name;
				}
				catch
				{
				}
				if (!string.IsNullOrEmpty(text))
				{
					_baselineAssemblies.Add(text);
				}
			}
			_baselineAssemblyCount = assemblies.Length;
			return;
		}
		if (!skipCountCheck && assemblies.Length > _baselineAssemblyCount + 15)
		{
			TriggerDetect($"AssemblyCountSpike:{assemblies.Length}vs{_baselineAssemblyCount}");
			return;
		}
		for (int j = 0; j < assemblies.Length; j++)
		{
			string text2 = null;
			try
			{
				text2 = assemblies[j].GetName()?.Name;
			}
			catch
			{
			}
			if (string.IsNullOrEmpty(text2))
			{
				if (!_baselineAssemblies.Contains("__unnamed__"))
				{
					TriggerDetect("SuspiciousAssembly:UnnamedAssembly");
					break;
				}
			}
			else
			{
				if (_baselineAssemblies.Contains(text2))
				{
					continue;
				}
				if (text2.IndexOf("dnlib", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("Mono.Cecil", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("UnityExplorer", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("MelonLoader", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("ScriptEngine", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("Cheat", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("Hack", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("Inject", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("Trainer", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					TriggerDetect("SuspiciousAssembly:Name:" + text2);
					break;
				}
				try
				{
					Type[] types = assemblies[j].GetTypes();
					for (int k = 0; k < types.Length; k++)
					{
						string fullName = types[k].FullName;
						if (fullName != null && (fullName.IndexOf("dnlib.DotNet", StringComparison.Ordinal) >= 0 || fullName.IndexOf("ICSharpCode.Decompiler", StringComparison.Ordinal) >= 0 || fullName.IndexOf("Mono.Cecil.Cil", StringComparison.Ordinal) >= 0 || fullName.IndexOf("UniverseLib", StringComparison.Ordinal) >= 0 || fullName.IndexOf("RuntimeInspector", StringComparison.Ordinal) >= 0 || fullName.IndexOf("MonoMod.RuntimeDetour", StringComparison.Ordinal) >= 0 || fullName.IndexOf("NativeDetour", StringComparison.Ordinal) >= 0))
						{
							TriggerDetect("SuspiciousAssembly:Type:" + fullName);
							return;
						}
					}
				}
				catch
				{
				}
			}
		}
	}

	private static void EnsureMethodCachesResolved()
	{
		if (_cachedILMethods != null)
		{
			return;
		}
		_cachedILMethods = new(MethodBase, string, int)[19]
		{
			(typeof(CertificatePinner).GetMethod("ValidateServerCertificate", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "ValidateServerCertificate", 50),
			(typeof(IntegrityGuard).GetProperty("IsIntact", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetGetMethod(nonPublic: true), "IsIntact", 10),
			(typeof(ActionPermitSystem).GetMethod("RequestExecution", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "RequestExecution", 50),
			(typeof(ActionPermitSystem).GetMethod("OnServerApproval", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "OnServerApproval", 50),
			(typeof(GhostUI).GetMethod("GetRsaPublicKey", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "GetRsaPublicKey", 50),
			(typeof(GhostUI).GetMethod("VerifyBytecodeSignatureV5", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "VerifyBytecodeSignatureV5", 50),
			(typeof(GhostUI).GetMethod("Execute", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "Execute", 200),
			(typeof(GhostUI).GetMethod("CheckToken", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "CheckToken", 10),
			(typeof(GhostUI).GetMethod("ValidateTimestampAntiReplay", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "ValidateTimestampAntiReplay", 20),
			(typeof(ModKeyValidator).GetMethod("V", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "V", 20),
			(typeof(ModKeyValidator).GetProperty("IsPremium", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetGetMethod(nonPublic: true), "get_IsPremium", 40),
			(typeof(ModKeyValidator).GetMethod("GetDerivedApiUrl", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "GetDerivedApiUrl", 10),
			(typeof(ModKeyValidator).GetMethod("VerifyAllowedHostnames", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "VerifyAllowedHostnames", 10),
			(typeof(ModMenuCrewPlugin).GetMethod("CheckForSelfPatches", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "CheckForSelfPatches", 50),
			(typeof(ModMenuCrewPlugin).GetMethod("VerifyDnsResolutionAsync", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "VerifyDnsResolutionAsync", 20),
			(typeof(ServerGate).GetMethod("CanRender", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "CanRender", 20),
			(typeof(ServerData).GetMethod("CalculateIntegrity", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "CalculateIntegrity", 20),
			(typeof(ServerData).GetMethod("GetStoredIntegrityHash", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "GetStoredIntegrityHash", 10),
			(typeof(ServerData).GetMethod("TriggerSilentDenial", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), "TriggerSilentDenial", 20)
		};
		List<MethodBase> list = new List<MethodBase>();
		for (int i = 0; i < _cachedILMethods.Length; i++)
		{
			if (_cachedILMethods[i].method != null)
			{
				list.Add(_cachedILMethods[i].method);
			}
		}
		MethodInfo method = typeof(RealtimeConnection).GetMethod("Connect", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (method != null)
		{
			list.Add(method);
		}
		_cachedNativeMethods = list.ToArray();
	}

	private static void CheckILGutting()
	{
		try
		{
			EnsureMethodCachesResolved();
			if (_cachedILMethods == null)
			{
				return;
			}
			bool flag = true;
			string text = null;
			for (int i = 0; i < _cachedILMethods.Length; i++)
			{
				MethodBase item = _cachedILMethods[i].method;
				string item2 = _cachedILMethods[i].name;
				int item3 = _cachedILMethods[i].minILSize;
				if (item == null)
				{
					continue;
				}
				bool flag2 = false;
				string text2 = null;
				try
				{
					MethodBody methodBody = item.GetMethodBody();
					if (methodBody == null)
					{
						flag2 = true;
						text2 = "ILGutting:NullBody:" + item2;
					}
					else
					{
						byte[] iLAsByteArray = methodBody.GetILAsByteArray();
						if (iLAsByteArray == null || iLAsByteArray.Length < item3)
						{
							flag2 = true;
							text2 = $"ILGutting:{item2}:ilLen={((iLAsByteArray != null) ? iLAsByteArray.Length : (-1))}<{item3}";
						}
					}
				}
				catch
				{
					flag2 = true;
					text2 = "ILGutting:Exception:" + item2;
				}
				if (flag2)
				{
					flag = false;
					if (text == null)
					{
						text = text2;
					}
				}
			}
			if (!flag)
			{
				if (!_lastILScanFailed)
				{
					_lastILScanFailed = true;
					try
					{
						Debug.LogWarning(Object.op_Implicit("[ANTITAMPER] IL soft-fail: " + text));
						return;
					}
					catch
					{
						return;
					}
				}
				TriggerDetect(text);
			}
			else
			{
				_lastILScanFailed = false;
			}
		}
		catch
		{
		}
	}

	private static void CheckNativeDetours()
	{
		try
		{
			EnsureMethodCachesResolved();
			if (_cachedNativeMethods == null)
			{
				return;
			}
			if (_baselinePointers == null)
			{
				_snapshotReady = true;
				_baselinePointers = new IntPtr[_cachedNativeMethods.Length];
				_baselineBytes = new byte[_cachedNativeMethods.Length][];
				_rebaselineCount = new int[_cachedNativeMethods.Length];
				for (int i = 0; i < _cachedNativeMethods.Length; i++)
				{
					if (_cachedNativeMethods[i] == null)
					{
						_baselinePointers[i] = IntPtr.Zero;
						_baselineBytes[i] = null;
						continue;
					}
					RuntimeHelpers.PrepareMethod(_cachedNativeMethods[i].MethodHandle);
					_baselinePointers[i] = _cachedNativeMethods[i].MethodHandle.GetFunctionPointer();
					try
					{
						byte[] array = new byte[12];
						for (int j = 0; j < 12; j++)
						{
							array[j] = Marshal.ReadByte(_baselinePointers[i], j);
						}
						_baselineBytes[i] = array;
					}
					catch
					{
						_baselineBytes[i] = null;
					}
				}
				return;
			}
			for (int k = 0; k < _cachedNativeMethods.Length; k++)
			{
				MethodBase methodBase = _cachedNativeMethods[k];
				if (methodBase == null)
				{
					continue;
				}
				IntPtr functionPointer;
				try
				{
					functionPointer = methodBase.MethodHandle.GetFunctionPointer();
				}
				catch
				{
					TriggerDetect($"NativeDetour:PtrException:i={k}");
					break;
				}
				if (functionPointer == IntPtr.Zero)
				{
					continue;
				}
				if (_baselinePointers[k] != IntPtr.Zero && functionPointer != _baselinePointers[k])
				{
					if (IntegrityGuard.IsIntact)
					{
						_baselinePointers[k] = functionPointer;
						try
						{
							if (_baselineBytes[k] != null)
							{
								for (int l = 0; l < 12; l++)
								{
									_baselineBytes[k][l] = Marshal.ReadByte(functionPointer, l);
								}
							}
						}
						catch
						{
						}
					}
					else
					{
						if (_rebaselineCount[k] >= 5)
						{
							TriggerDetect($"NativeDetour:PtrChanged:i={k}:rebase={_rebaselineCount[k]}");
							break;
						}
						_rebaselineCount[k]++;
						_baselinePointers[k] = functionPointer;
						try
						{
							if (_baselineBytes[k] != null)
							{
								for (int m = 0; m < 12; m++)
								{
									_baselineBytes[k][m] = Marshal.ReadByte(functionPointer, m);
								}
							}
						}
						catch
						{
						}
					}
				}
				if (_baselineBytes[k] == null)
				{
					continue;
				}
				try
				{
					bool flag = false;
					for (int n = 0; n < 12; n++)
					{
						if (Marshal.ReadByte(functionPointer, n) != _baselineBytes[k][n])
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
					if (IntegrityGuard.IsIntact)
					{
						for (int num = 0; num < 12; num++)
						{
							_baselineBytes[k][num] = Marshal.ReadByte(functionPointer, num);
						}
						_baselinePointers[k] = functionPointer;
						continue;
					}
					if (_rebaselineCount[k] < 5)
					{
						_rebaselineCount[k]++;
						for (int num2 = 0; num2 < 12; num2++)
						{
							_baselineBytes[k][num2] = Marshal.ReadByte(functionPointer, num2);
						}
						_baselinePointers[k] = functionPointer;
						continue;
					}
					TriggerDetect($"NativeDetour:BytesChanged:i={k}:rebase={_rebaselineCount[k]}");
					break;
				}
				catch
				{
					TriggerDetect($"NativeDetour:AccessViolation:i={k}");
					break;
				}
			}
		}
		catch
		{
		}
	}

	private static void TriggerDetect(string source = "unknown")
	{
		_consecutiveCleanScans = 0;
		_corruptionLevel++;
		_tamperDetectedXor = 1569341299;
		try
		{
			Debug.LogError(Object.op_Implicit($"[ANTITAMPER] TriggerDetect source={source} corruptionLevel={_corruptionLevel}"));
		}
		catch
		{
		}
		if (_corruptionLevel <= 1)
		{
			ServerData.ScheduleDelayedDenial(60f + Random.Range(0f, 120f));
		}
		else if (_corruptionLevel == 2)
		{
			if (Random.Range(0f, 1f) < 0.3f)
			{
				ServerData.TriggerSilentDenial();
			}
			else
			{
				ServerData.ScheduleDelayedDenial(30f + Random.Range(0f, 90f));
			}
		}
		else if (Random.Range(0f, 1f) < 0.6f)
		{
			ServerData.TriggerSilentDenial();
		}
		else
		{
			ServerData.ScheduleDelayedDenial(10f + Random.Range(0f, 30f));
		}
	}
}
