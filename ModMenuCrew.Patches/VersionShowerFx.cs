using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenuCrew.Patches;

public class VersionShowerFx : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CAudioDistortion_003Ed__135 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CAudioDistortion_003Ed__135(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				string input = sAudioGlitches[sRandom.Next(sAudioGlitches.Length)];
				((Graphic)versionShowerFx._text).color = Color.Lerp(GhostCyan, Color.white, (float)sRandom.NextDouble());
				versionShowerFx.SetText(versionShowerFx.CorruptText(input, 4), isGlitching: true);
				_003C_003E2__current = sWait04;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CBinaryHorror_003Ed__148 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string[] _003C_003E7__wrap1;

		private int _003C_003E7__wrap2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CBinaryHorror_003Ed__148(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E7__wrap1 = sBinaryMsgs;
				_003C_003E7__wrap2 = 0;
				goto IL_00ba;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E7__wrap2++;
				goto IL_00ba;
			case 3:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00ba:
				if (_003C_003E7__wrap2 < _003C_003E7__wrap1.Length)
				{
					string text = _003C_003E7__wrap1[_003C_003E7__wrap2];
					if ((Object)(object)versionShowerFx._text == (Object)null)
					{
						return false;
					}
					((Graphic)versionShowerFx._text).color = FnafGreen;
					_003C_003E2__current = versionShowerFx.TypewriterText(text, 0.02f);
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E7__wrap1 = null;
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("HELP. DEAD. RUN.", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CBloodDripEffect_003Ed__98 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string _003Ccurrent_003E5__2;

		private int _003Clen_003E5__3;

		private int _003Ci_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CBloodDripEffect_003Ed__98(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Ccurrent_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				_003Ccurrent_003E5__2 = versionShowerFx._modText;
				_003Clen_003E5__3 = _003Ccurrent_003E5__2.Length;
				_003Ci_003E5__4 = 0;
				goto IL_00e9;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__4++;
				goto IL_00e9;
			case 2:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00e9:
				if (_003Ci_003E5__4 < _003Clen_003E5__3)
				{
					versionShowerFx._textBuilder.Clear().Append(_003Ccurrent_003E5__2, 0, _003Ci_003E5__4).Append("<color=red>█</color>")
						.Append(_003Ccurrent_003E5__2, _003Ci_003E5__4 + 1, _003Clen_003E5__3 - _003Ci_003E5__4 - 1);
					versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
					_003C_003E2__current = sWait05;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E2__current = versionShowerFx.ImpostorFlash(DeadRed);
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CBurnInPulse_003Ed__126 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CBurnInPulse_003Ed__126(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				float num2 = Mathf.PingPong(Time.time * 2f, 1f);
				((TMP_Text)versionShowerFx._text).outlineWidth = Mathf.Lerp(versionShowerFx._baseOutlineWidth, 0.5f, num2);
				_003C_003E2__current = sWaitFrame;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCRTCrosstalk_003Ed__119 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCRTCrosstalk_003Ed__119(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				versionShowerFx._textBuilder.Clear().Append(versionShowerFx._modText).Append("\n<color=#FFFF00>")
					.Append(versionShowerFx._modText)
					.Append("</color>");
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003C_003E2__current = sWait04;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCameraDisabled_003Ed__137 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCameraDisabled_003Ed__137(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				string content = sCamRooms[sRandom.Next(sCamRooms.Length)];
				((Graphic)versionShowerFx._text).color = Color.cyan;
				versionShowerFx.SetText(content, isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_00bb;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_00bb;
			case 3:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00bb:
				if (_003Ci_003E5__2 < 5)
				{
					versionShowerFx.SetText(versionShowerFx.RandomNoise(15), isGlitching: true);
					_003C_003E2__current = sWait05;
					_003C_003E1__state = 2;
					return true;
				}
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("SIGNAL LOST", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCameraLabelFlash_003Ed__125 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		private int _003Ccam_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCameraLabelFlash_003Ed__125(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				_003Ccam_003E5__3 = sRandom.Next(1, 9);
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				versionShowerFx._textBuilder.Clear().Append("CAM ").Append(_003Ccam_003E5__3.ToString("00"));
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				((Graphic)versionShowerFx._text).color = Color.cyan;
				_003Ccam_003E5__3 = _003Ccam_003E5__3 % 8 + 1;
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCharacterSwapGlitch_003Ed__117 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		private char[] _003Cchars_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCharacterSwapGlitch_003Ed__117(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Cchars_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				_003Cchars_003E5__3 = versionShowerFx._modText.ToCharArray();
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				int num2 = sRandom.Next(_003Cchars_003E5__3.Length);
				int num3 = sRandom.Next(_003Cchars_003E5__3.Length);
				ref char reference = ref _003Cchars_003E5__3[num2];
				ref char reference2 = ref _003Cchars_003E5__3[num3];
				char c = _003Cchars_003E5__3[num3];
				char c2 = _003Cchars_003E5__3[num2];
				reference = c;
				reference2 = c2;
				versionShowerFx._textBuilder.Clear().Append(_003Cchars_003E5__3);
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003C_003E2__current = sWait04;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CChromaticAberration_003Ed__110 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CChromaticAberration_003Ed__110(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				versionShowerFx._textBuilder.Clear().Append(versionShowerFx._modText).Append("\n<color=#FF0000>")
					.Append(versionShowerFx._modText)
					.Append("</color>\n<color=#00FF00>")
					.Append(versionShowerFx._modText)
					.Append("</color>");
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003C_003E2__current = sWait04;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CColorDrain_003Ed__115 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CColorDrain_003Ed__115(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				float num2 = (_003CendTime_003E5__2 - Time.time) / duration;
				((Graphic)versionShowerFx._text).color = Color.Lerp(new Color(0.1f, 0.1f, 0.1f), versionShowerFx._baseColor, num2);
				_003C_003E2__current = sWaitFrame;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CColorFlash_003Ed__130 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		public Color color;

		public float duration;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CColorFlash_003Ed__130(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				if ((Object)(object)versionShowerFx._text == (Object)null)
				{
					return false;
				}
				((Graphic)versionShowerFx._text).color = color;
				((TMP_Text)versionShowerFx._text).outlineColor = Color32.op_Implicit(color);
				WaitForSeconds val = ((duration <= 0.06f) ? sWait05 : ((duration <= 0.12f) ? sWait1 : ((duration <= 0.2f) ? sWait15 : ((duration <= 0.3f) ? sWait2 : ((!(duration <= 0.45f)) ? sWait5 : sWait3)))));
				object obj = val;
				_003C_003E2__current = obj;
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCommonTierEvent_003Ed__91 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCommonTierEvent_003Ed__91(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				switch (sRandom.Next(0, 6))
				{
				case 0:
					_003C_003E2__current = versionShowerFx.Jitter(0.3f, 1f, 1.5f);
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E2__current = versionShowerFx.TextCorruption(0.4f, 6, versionShowerFx._modText);
					_003C_003E1__state = 2;
					return true;
				case 2:
					_003C_003E2__current = versionShowerFx.VHSStaticBurst(0.35f);
					_003C_003E1__state = 3;
					return true;
				case 3:
					_003C_003E2__current = versionShowerFx.GreenPulse(0.4f);
					_003C_003E1__state = 4;
					return true;
				case 4:
					_003C_003E2__current = versionShowerFx.NoiseScroll(0.4f);
					_003C_003E1__state = 5;
					return true;
				case 5:
					_003C_003E2__current = versionShowerFx.ColorFlash(DeadRed, 0.25f);
					_003C_003E1__state = 6;
					return true;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
				_003C_003E1__state = -1;
				break;
			case 3:
				_003C_003E1__state = -1;
				break;
			case 4:
				_003C_003E1__state = -1;
				break;
			case 5:
				_003C_003E1__state = -1;
				break;
			case 6:
				_003C_003E1__state = -1;
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCorpseReveal_003Ed__159 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCorpseReveal_003Ed__159(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DecayBrown;
				_003Ci_003E5__2 = 0;
				goto IL_00be;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_00be;
			case 2:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("BODY REPORTED", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 3;
				return true;
			case 3:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00be:
				if (_003Ci_003E5__2 < sCorpseArt.Length)
				{
					if ((Object)(object)versionShowerFx._text == (Object)null)
					{
						return false;
					}
					versionShowerFx.SetText(sCorpseArt[_003Ci_003E5__2], isGlitching: true);
					((Graphic)versionShowerFx._text).color = Color.Lerp(DecayBrown, BoneWhite, (float)_003Ci_003E5__2 / (float)sCorpseArt.Length);
					_003C_003E2__current = sWait4;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E2__current = versionShowerFx.ImpostorFlash(DeadRed);
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCountdownTerror_003Ed__145 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCountdownTerror_003Ed__145(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 5;
				goto IL_0120;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait07;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2--;
				goto IL_0120;
			case 3:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0120:
				if (_003Ci_003E5__2 >= 0)
				{
					float num2 = 1f - (float)_003Ci_003E5__2 / 5f;
					((Graphic)versionShowerFx._text).color = Color.Lerp(AlertOrange, DeadRed, num2);
					float num3 = versionShowerFx._baseScale * (1f + 0.1f * (float)(5 - _003Ci_003E5__2));
					((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(num3, num3, 1f);
					versionShowerFx.SetText((_003Ci_003E5__2 > 0) ? _003Ci_003E5__2.ToString() : "TIME'S UP", isGlitching: true);
					_003C_003E2__current = versionShowerFx.Jitter(0.1f, 1f + (float)(5 - _003Ci_003E5__2), 2f);
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E2__current = versionShowerFx.ImpostorFlash(DeadRed);
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCrewmateColorCycle_003Ed__105 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Citerations_003E5__2;

		private int _003Ci_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCrewmateColorCycle_003Ed__105(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Citerations_003E5__2 = _colorCycle.Length * 2;
				_003Ci_003E5__3 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__3++;
				break;
			}
			if (_003Ci_003E5__3 < _003Citerations_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				((Graphic)versionShowerFx._text).color = _colorCycle[_003Ci_003E5__3 % _colorCycle.Length];
				((TMP_Text)versionShowerFx._text).outlineColor = Color32.op_Implicit(_colorCycle[_003Ci_003E5__3 % _colorCycle.Length] * 0.7f);
				versionShowerFx.SetText(versionShowerFx.CorruptText(versionShowerFx._modText, 3), isGlitching: true);
				_003C_003E2__current = sWait025;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CDeepZalgo_003Ed__172 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Cintensity_003E5__2;

		private string _003CcolorPrefix_003E5__3;

		private float _003Celapsed_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CDeepZalgo_003Ed__172(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003CcolorPrefix_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cintensity_003E5__2 = (int)(2f + versionShowerFx._CorrLevel() * 3f);
				_003CcolorPrefix_003E5__3 = "<color=#" + ColorUtility.ToHtmlStringRGB(BloodDark) + ">";
				_003Celapsed_003E5__4 = 0f;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Celapsed_003E5__4 += 0.08f;
				break;
			}
			if (_003Celapsed_003E5__4 < 1.2f)
			{
				string text = versionShowerFx._Zg(versionShowerFx._modText, _003Cintensity_003E5__2);
				versionShowerFx.SetText(_003CcolorPrefix_003E5__3 + text + "</color>", isGlitching: true);
				_003C_003E2__current = sWait08;
				_003C_003E1__state = 1;
				return true;
			}
			versionShowerFx.SetText(versionShowerFx._modText);
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CEndoskeletonExposed_003Ed__164 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string[] _003Clayers_003E5__2;

		private Color[] _003ClayerColors_003E5__3;

		private int _003Ci_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CEndoskeletonExposed_003Ed__164(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Clayers_003E5__2 = null;
			_003ClayerColors_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Clayers_003E5__2 = new string[6]
				{
					versionShowerFx._modText,
					"[ " + versionShowerFx._modText + " ]",
					"[▓" + versionShowerFx._modText + "▓]",
					"[█▓ ENDO ▓█]",
					"[█▓ 01 ▓█]",
					"[●_●]"
				};
				_003ClayerColors_003E5__3 = (Color[])(object)new Color[6]
				{
					versionShowerFx._baseColor,
					StaticGray,
					OxidizedGreen,
					BoneWhite,
					DeadRed,
					Color.white
				};
				_003Ci_003E5__4 = 0;
				goto IL_0171;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__4++;
				goto IL_0171;
			case 3:
				{
					_003C_003E1__state = -1;
					_003Ci_003E5__4++;
					break;
				}
				IL_0171:
				if (_003Ci_003E5__4 < _003Clayers_003E5__2.Length && (Object)(object)versionShowerFx._text != (Object)null)
				{
					((Graphic)versionShowerFx._text).color = _003ClayerColors_003E5__3[_003Ci_003E5__4];
					versionShowerFx.SetText(_003Clayers_003E5__2[_003Ci_003E5__4], isGlitching: true);
					_003C_003E2__current = versionShowerFx.Jitter(0.15f, 1f, 3f);
					_003C_003E1__state = 1;
					return true;
				}
				_003Ci_003E5__4 = 0;
				break;
			}
			if (_003Ci_003E5__4 < 4)
			{
				((Graphic)versionShowerFx._text).color = ((_003Ci_003E5__4 % 2 == 0) ? Color.white : DeadRed);
				_003C_003E2__current = sWait08;
				_003C_003E1__state = 3;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CEyesInDarkness_003Ed__154 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string _003CrandomGrotesque_003E5__2;

		private int _003Cidx_003E5__3;

		private string _003Ceyes_003E5__4;

		private int _003Cf_003E5__5;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CEyesInDarkness_003Ed__154(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003CrandomGrotesque_003E5__2 = null;
			_003Ceyes_003E5__4 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				((Graphic)versionShowerFx._text).color = Color.black;
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003CrandomGrotesque_003E5__2 = sGrotesqueFaces[sRandom.Next(sGrotesqueFaces.Length)];
				_003Cidx_003E5__3 = 0;
				goto IL_0173;
			case 2:
				_003C_003E1__state = -1;
				_003Cf_003E5__5++;
				goto IL_012c;
			case 3:
				_003C_003E1__state = -1;
				_003Ceyes_003E5__4 = null;
				_003Cidx_003E5__3++;
				goto IL_0173;
			case 4:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0173:
				if (_003Cidx_003E5__3 <= sEyePatterns.Length)
				{
					if ((Object)(object)versionShowerFx._text == (Object)null)
					{
						return false;
					}
					_003Ceyes_003E5__4 = ((_003Cidx_003E5__3 < sEyePatterns.Length) ? sEyePatterns[_003Cidx_003E5__3] : _003CrandomGrotesque_003E5__2);
					_003Cf_003E5__5 = 0;
					goto IL_012c;
				}
				((Graphic)versionShowerFx._text).color = DeadRed;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 2f, versionShowerFx._baseScale * 2f, 1f);
				versionShowerFx.SetText("WE SEE YOU", isGlitching: true);
				_003C_003E2__current = versionShowerFx.Jitter(0.5f, 6f, 12f);
				_003C_003E1__state = 4;
				return true;
				IL_012c:
				if (_003Cf_003E5__5 < 3)
				{
					((Graphic)versionShowerFx._text).color = ((_003Cf_003E5__5 % 2 == 0) ? Color.white : StaticGray);
					versionShowerFx.SetText(_003Ceyes_003E5__4, isGlitching: true);
					_003C_003E2__current = sWait08;
					_003C_003E1__state = 2;
					return true;
				}
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait3;
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CFaceFlash_003Ed__134 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string _003Cface_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CFaceFlash_003Ed__134(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Cface_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cface_003E5__2 = sCreepyFaces[sRandom.Next(sCreepyFaces.Length)];
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 2f, versionShowerFx._baseScale * 2f, 1f);
				versionShowerFx.SetText(_003Cface_003E5__2, isGlitching: true);
				_003C_003E2__current = sWait08;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.Jitter(0.2f, 5f, 10f);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale, versionShowerFx._baseScale, 1f);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CFleshReveal_003Ed__153 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string _003Ctext_003E5__2;

		private int _003Clen_003E5__3;

		private int _003Clayer_003E5__4;

		private int _003Ci_003E5__5;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CFleshReveal_003Ed__153(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Ctext_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ctext_003E5__2 = versionShowerFx._modText;
				_003Clen_003E5__3 = _003Ctext_003E5__2.Length;
				_003Clayer_003E5__4 = 0;
				goto IL_01cf;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__5++;
				goto IL_0183;
			case 2:
				_003C_003E1__state = -1;
				_003Clayer_003E5__4++;
				goto IL_01cf;
			case 3:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_01cf:
				if (_003Clayer_003E5__4 < 4 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					Color color = (Color)(_003Clayer_003E5__4 switch
					{
						0 => versionShowerFx._baseColor, 
						1 => DecayBrown, 
						2 => FleshPink, 
						_ => VisceralRed, 
					});
					((Graphic)versionShowerFx._text).color = color;
					_003Ci_003E5__5 = 0;
					goto IL_0183;
				}
				((Graphic)versionShowerFx._text).color = VisceralRed;
				versionShowerFx.SetText("FLESH EXPOSED", isGlitching: true);
				_003C_003E2__current = versionShowerFx.Jitter(0.3f, 4f, 8f);
				_003C_003E1__state = 3;
				return true;
				IL_0183:
				if (_003Ci_003E5__5 < _003Clen_003E5__3 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					char value = _003Clayer_003E5__4 switch
					{
						1 => '▓', 
						2 => '░', 
						3 => '█', 
						_ => _003Ctext_003E5__2[_003Ci_003E5__5], 
					};
					versionShowerFx._textBuilder.Clear().Append(_003Ctext_003E5__2, 0, _003Ci_003E5__5).Append(value)
						.Append(_003Ctext_003E5__2, _003Ci_003E5__5 + 1, _003Clen_003E5__3 - _003Ci_003E5__5 - 1);
					versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
					_003C_003E2__current = sWait025;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGlitchScheduler_003Ed__89 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGlitchScheduler_003Ed__89(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected O, but got Unknown
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				versionShowerFx.SetText(versionShowerFx._modText);
				break;
			case 1:
				_003C_003E1__state = -1;
				if (!versionShowerFx._isGlitchActive && !((Object)(object)versionShowerFx._text == (Object)null))
				{
					versionShowerFx._isGlitchActive = true;
					bool flag = Time.time >= versionShowerFx._nextHeavyAllowedTime;
					double num2 = sRandom.NextDouble();
					if (num2 < (double)sProbPhantom && flag)
					{
						versionShowerFx._nextHeavyAllowedTime = Time.time + versionShowerFx._heavyCooldownSeconds;
						_003C_003E2__current = versionShowerFx.PhantomTierEvent();
						_003C_003E1__state = 2;
						return true;
					}
					if (num2 < (double)(sProbPhantom + sProbMythic) && flag)
					{
						versionShowerFx._nextHeavyAllowedTime = Time.time + versionShowerFx._heavyCooldownSeconds;
						_003C_003E2__current = versionShowerFx.MythicTierEvent();
						_003C_003E1__state = 3;
						return true;
					}
					if (num2 < (double)(sProbPhantom + sProbMythic + sProbRare))
					{
						_003C_003E2__current = versionShowerFx.RareTierEvent();
						_003C_003E1__state = 4;
						return true;
					}
					if (num2 < 0.8500000238418579)
					{
						_003C_003E2__current = versionShowerFx.UncommonTierEvent();
						_003C_003E1__state = 5;
						return true;
					}
					_003C_003E2__current = versionShowerFx.CommonTierEvent();
					_003C_003E1__state = 6;
					return true;
				}
				break;
			case 2:
				_003C_003E1__state = -1;
				goto IL_01d0;
			case 3:
				_003C_003E1__state = -1;
				goto IL_01d0;
			case 4:
				_003C_003E1__state = -1;
				goto IL_01d0;
			case 5:
				_003C_003E1__state = -1;
				goto IL_01d0;
			case 6:
				_003C_003E1__state = -1;
				goto IL_01d0;
			case 7:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_01d0:
				versionShowerFx._isGlitchActive = false;
				versionShowerFx.ResetVisualsToStable();
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 7;
				return true;
			}
			if (versionShowerFx._isEffectRunning && (Object)(object)versionShowerFx._text != (Object)null)
			{
				float num3 = ((float)sRandom.NextDouble() * 2f + 1.5f) * (1.5f - versionShowerFx._CorrLevel());
				_003C_003E2__current = (object)new WaitForSeconds(num3);
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGreenPulse_003Ed__120 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGreenPulse_003Ed__120(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				float num2 = Mathf.PingPong(Time.time * 2f, 1f);
				((Graphic)versionShowerFx._text).color = Color.Lerp(versionShowerFx._baseColor, FnafGreen, num2);
				_003C_003E2__current = sWaitFrame;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CHallucination_003Ed__157 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CHallucination_003Ed__157(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				float num2 = 2.5f;
				_003CendTime_003E5__2 = Time.time + num2;
				goto IL_0222;
			}
			case 1:
				_003C_003E1__state = -1;
				goto IL_0222;
			case 2:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("WAKE UP", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 3;
				return true;
			case 3:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0222:
				if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					((Graphic)versionShowerFx._text).color = new Color((float)sRandom.NextDouble(), (float)sRandom.NextDouble(), (float)sRandom.NextDouble());
					versionShowerFx.SetText(versionShowerFx.CorruptText(sRandom.Next(0, 5) switch
					{
						0 => sSystemMessages[sRandom.Next(sSystemMessages.Length)], 
						1 => sSpringtrapLore[sRandom.Next(sSpringtrapLore.Length)], 
						2 => sVictimMessages[sRandom.Next(sVictimMessages.Length)], 
						3 => sGrotesqueMessages[sRandom.Next(sGrotesqueMessages.Length)], 
						_ => sGrotesqueFaces[sRandom.Next(sGrotesqueFaces.Length)], 
					}, sRandom.Next(3, 8)), isGlitching: true);
					float num3 = ((float)sRandom.NextDouble() - 0.5f) * 8f;
					float num4 = ((float)sRandom.NextDouble() - 0.5f) * 8f;
					versionShowerFx._textRectTransform.anchoredPosition = new Vector2(versionShowerFx._baseAnchoredPosition.x + num3, versionShowerFx._baseAnchoredPosition.y + num4);
					((Transform)versionShowerFx._textRectTransform).localRotation = Quaternion.Euler(0f, 0f, ((float)sRandom.NextDouble() - 0.5f) * 20f);
					float num5 = versionShowerFx._baseScale * (0.8f + (float)sRandom.NextDouble() * 0.8f);
					((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(num5, num5, 1f);
					_003C_003E2__current = sWait025;
					_003C_003E1__state = 1;
					return true;
				}
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CHeartbeatHorror_003Ed__97 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CHeartbeatHorror_003Ed__97(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale, versionShowerFx._baseScale, 1f);
				versionShowerFx.SetText(versionShowerFx._modText, isGlitching: true);
				_003C_003E2__current = sWait08;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 1.2f, versionShowerFx._baseScale * 1.2f, 1f);
				((Graphic)versionShowerFx._text).color = BloodDark;
				_003C_003E2__current = sWait05;
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale, versionShowerFx._baseScale, 1f);
				_003C_003E2__current = sWait6;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				break;
			}
			if (_003Ci_003E5__2 < 4)
			{
				((Graphic)versionShowerFx._text).color = DeadRed;
				((TMP_Text)versionShowerFx._text).outlineColor = Color32.op_Implicit(BloodDark);
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 1.4f, versionShowerFx._baseScale * 1.4f, 1f);
				versionShowerFx.SetText("THUMP", isGlitching: true);
				_003C_003E2__current = sWait05;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CHorrorFaceFlash_003Ed__170 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string _003Ccolored_003E5__2;

		private int _003Ci_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CHorrorFaceFlash_003Ed__170(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Ccolored_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				string value = sHorrorFaces[sRandom.Next(sHorrorFaces.Length)];
				Color val = ((sRandom.Next(0, 2) == 0) ? VisceralRed : OxidizedGreen);
				_003Ccolored_003E5__2 = $"<color=#{ColorUtility.ToHtmlStringRGB(val)}><b>{value}</b></color>";
				_003Ci_003E5__3 = 0;
				goto IL_011a;
			}
			case 1:
				_003C_003E1__state = -1;
				versionShowerFx.SetText(" ", isGlitching: true);
				_003C_003E2__current = sWait05;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__3++;
				goto IL_011a;
			case 3:
				{
					_003C_003E1__state = -1;
					versionShowerFx.SetText(versionShowerFx._modText);
					return false;
				}
				IL_011a:
				if (_003Ci_003E5__3 < 3)
				{
					versionShowerFx.SetText(_003Ccolored_003E5__2, isGlitching: true);
					_003C_003E2__current = sWait15;
					_003C_003E1__state = 1;
					return true;
				}
				versionShowerFx.SetText(_003Ccolored_003E5__2, isGlitching: true);
				_003C_003E2__current = sWait4;
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CIdleBreathing_003Ed__90 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private float _003Cseed_003E5__2;

		private float _003C_nextIdleGlitchTime_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CIdleBreathing_003Ed__90(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cseed_003E5__2 = (float)sRandom.NextDouble() * 100f;
				_003C_nextIdleGlitchTime_003E5__3 = Time.time + 4f + (float)sRandom.NextDouble() * 6f;
				goto IL_0304;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWaitFrame;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				versionShowerFx.SetText(versionShowerFx._modText);
				goto IL_02e9;
			case 3:
				{
					_003C_003E1__state = -1;
					goto IL_0304;
				}
				IL_0304:
				if (versionShowerFx._isEffectRunning)
				{
					if ((Object)(object)versionShowerFx._text == (Object)null)
					{
						return false;
					}
					if (!versionShowerFx._isGlitchActive)
					{
						RectTransform textRectTransform = versionShowerFx._textRectTransform;
						float num2 = Time.time + _003Cseed_003E5__2;
						float num3 = Mathf.Sin(num2 * 1.2f);
						float num4 = Mathf.SmoothStep(-1f, 1f, (num3 + 1f) * 0.5f) * 2f - 1f;
						float num5 = Mathf.Sin(num2 * 5f) * 0.008f;
						float num6 = num4 * 0.04f + num5;
						float num7 = Mathf.Sin(num2 * 0.6f) * 0.8f + Mathf.Sin(num2 * 1.7f) * 0.3f;
						((Transform)textRectTransform).localRotation = Quaternion.Euler(0f, 0f, num7);
						((Transform)textRectTransform).localScale = new Vector3(versionShowerFx._baseScale + num6, versionShowerFx._baseScale + num6, 1f);
						textRectTransform.anchoredPosition = versionShowerFx._baseAnchoredPosition;
						float num8 = Mathf.PerlinNoise(num2 * 0.3f, _003Cseed_003E5__2) * 0.12f;
						float num9 = Mathf.PerlinNoise(num2 * 0.7f, _003Cseed_003E5__2 + 50f) * 0.05f;
						float num10 = num8 + num9;
						((Graphic)versionShowerFx._text).color = new Color(Mathf.Lerp(versionShowerFx._baseColor.r, versionShowerFx._baseColor.r - num10 * 0.4f, 0.5f), Mathf.Lerp(versionShowerFx._baseColor.g, versionShowerFx._baseColor.g + num10, 0.5f), Mathf.Lerp(versionShowerFx._baseColor.b, versionShowerFx._baseColor.b - num10 * 0.2f, 0.5f), 1f);
						float num11 = Mathf.Sin(num2 * 1f);
						float num12 = ((num11 > 0f) ? (num11 * num11) : (num11 * 0.3f)) * 0.04f;
						((TMP_Text)versionShowerFx._text).outlineWidth = versionShowerFx._baseOutlineWidth + num12;
						if (Time.time >= _003C_nextIdleGlitchTime_003E5__3)
						{
							_003C_nextIdleGlitchTime_003E5__3 = Time.time + 5f + (float)sRandom.NextDouble() * 10f;
							versionShowerFx.SetText(versionShowerFx.CorruptText(versionShowerFx._modText, 1), isGlitching: true);
							_003C_003E2__current = sWaitFrame;
							_003C_003E1__state = 1;
							return true;
						}
					}
					goto IL_02e9;
				}
				return false;
				IL_02e9:
				_003C_003E2__current = sWaitFrame;
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CImpostorFlash_003Ed__112 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		public Color color;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CImpostorFlash_003Ed__112(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if ((Object)(object)versionShowerFx._text == (Object)null)
				{
					return false;
				}
				((Graphic)versionShowerFx._text).color = color;
				((TMP_Text)versionShowerFx._text).outlineColor = Color32.op_Implicit(color * 0.7f);
				((TMP_Text)versionShowerFx._text).outlineWidth = 0.45f;
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CIntensiveGlitchSequence_003Ed__131 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		private RectTransform _003Crt_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CIntensiveGlitchSequence_003Ed__131(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Crt_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				_003Crt_003E5__3 = versionShowerFx._textRectTransform;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				float num2 = 1f - (_003CendTime_003E5__2 - Time.time) / duration;
				float num3 = Mathf.Lerp(1f, 2.5f, num2);
				versionShowerFx.SetText(versionShowerFx.CorruptText(versionShowerFx._modText, (int)(num3 * 5f)), isGlitching: true);
				float num4 = ((float)sRandom.NextDouble() - 0.5f) * num3 * 2f;
				float num5 = ((float)sRandom.NextDouble() - 0.5f) * num3 * 2f;
				_003Crt_003E5__3.anchoredPosition = new Vector2(versionShowerFx._baseAnchoredPosition.x + num4, versionShowerFx._baseAnchoredPosition.y + num5);
				((Graphic)versionShowerFx._text).color = _flashColors[(int)(num2 * 4f) % 4];
				_003C_003E2__current = sWait025;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CInvertedReality_003Ed__171 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private RectTransform _003Crt_003E5__2;

		private Quaternion _003CoriginalRot_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CInvertedReality_003Ed__171(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Crt_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				string value = versionShowerFx._Fl(versionShowerFx._modText);
				string content = $"<color=#{ColorUtility.ToHtmlStringRGB(CursedPurple)}>{value}</color>";
				versionShowerFx.SetText(content, isGlitching: true);
				_003Crt_003E5__2 = versionShowerFx._textRectTransform;
				_003CoriginalRot_003E5__3 = ((Transform)_003Crt_003E5__2).localRotation;
				((Transform)_003Crt_003E5__2).localRotation = Quaternion.Euler(0f, 0f, 180f);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				((Transform)_003Crt_003E5__2).localRotation = _003CoriginalRot_003E5__3;
				versionShowerFx.SetText(versionShowerFx._modText);
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CJitter_003Ed__109 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		public float posRange;

		public float rotRange;

		private float _003CendTime_003E5__2;

		private RectTransform _003Crt_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CJitter_003Ed__109(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Crt_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				_003Crt_003E5__3 = versionShowerFx._textRectTransform;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				float num2 = ((float)sRandom.NextDouble() - 0.5f) * posRange;
				float num3 = ((float)sRandom.NextDouble() - 0.5f) * posRange;
				float num4 = ((float)sRandom.NextDouble() - 0.5f) * 2f * rotRange;
				_003Crt_003E5__3.anchoredPosition = new Vector2(versionShowerFx._baseAnchoredPosition.x + num2, versionShowerFx._baseAnchoredPosition.y + num3);
				((Transform)_003Crt_003E5__3).localRotation = Quaternion.Euler(0f, 0f, num4);
				_003C_003E2__current = sWaitFrame;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CMotionPing_003Ed__128 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CMotionPing_003Ed__128(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				versionShowerFx.SetText(versionShowerFx._modText, isGlitching: true);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				string value = versionShowerFx.RandomRoom();
				((Graphic)versionShowerFx._text).color = Color.yellow;
				versionShowerFx._textBuilder.Clear().Append("MOTION - ").Append(value);
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CMythicTierEvent_003Ed__94 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CMythicTierEvent_003Ed__94(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				switch (sRandom.Next(0, 26))
				{
				case 0:
					_003C_003E2__current = versionShowerFx.Sequence_EmergencyMeeting();
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E2__current = versionShowerFx.Sequence_ImpostorReveal();
					_003C_003E1__state = 2;
					return true;
				case 2:
					_003C_003E2__current = versionShowerFx.Sequence_SystemReboot();
					_003C_003E1__state = 3;
					return true;
				case 3:
					_003C_003E2__current = versionShowerFx.Sequence_SabotageCritical();
					_003C_003E1__state = 4;
					return true;
				case 4:
					_003C_003E2__current = versionShowerFx.Sequence_CriticalBreach();
					_003C_003E1__state = 5;
					return true;
				case 5:
					_003C_003E2__current = versionShowerFx.Sequence_TheStare();
					_003C_003E1__state = 6;
					return true;
				case 6:
					_003C_003E2__current = versionShowerFx.Sequence_SpringtrapApproach();
					_003C_003E1__state = 7;
					return true;
				case 7:
					_003C_003E2__current = versionShowerFx.Sequence_PhantomJumpscare();
					_003C_003E1__state = 8;
					return true;
				case 8:
					_003C_003E2__current = versionShowerFx.Sequence_NightmareMode();
					_003C_003E1__state = 9;
					return true;
				case 9:
					_003C_003E2__current = versionShowerFx.Sequence_BadEnding();
					_003C_003E1__state = 10;
					return true;
				case 10:
					_003C_003E2__current = versionShowerFx.Sequence_6AM();
					_003C_003E1__state = 11;
					return true;
				case 11:
					_003C_003E2__current = versionShowerFx.Sequence_GoldenFreddy();
					_003C_003E1__state = 12;
					return true;
				case 12:
					_003C_003E2__current = versionShowerFx.PowerOutage();
					_003C_003E1__state = 13;
					return true;
				case 13:
					_003C_003E2__current = versionShowerFx.CountdownTerror();
					_003C_003E1__state = 14;
					return true;
				case 14:
					_003C_003E2__current = versionShowerFx.Sequence_SpringLockFailure();
					_003C_003E1__state = 15;
					return true;
				case 15:
					_003C_003E2__current = versionShowerFx.Sequence_AftonDecomposition();
					_003C_003E1__state = 16;
					return true;
				case 16:
					_003C_003E2__current = versionShowerFx.SoulCapture();
					_003C_003E1__state = 17;
					return true;
				case 17:
					_003C_003E2__current = versionShowerFx.Hallucination();
					_003C_003E1__state = 18;
					return true;
				case 18:
					_003C_003E2__current = versionShowerFx.Sequence_PhantomMangle();
					_003C_003E1__state = 19;
					return true;
				case 19:
					_003C_003E2__current = versionShowerFx.Sequence_NightmareFredbear();
					_003C_003E1__state = 20;
					return true;
				case 20:
					_003C_003E2__current = versionShowerFx.Sequence_Dismemberment();
					_003C_003E1__state = 21;
					return true;
				case 21:
					_003C_003E2__current = versionShowerFx.EyesInDarkness();
					_003C_003E1__state = 22;
					return true;
				case 22:
					_003C_003E2__current = versionShowerFx.Sequence_VentilationCascade();
					_003C_003E1__state = 23;
					return true;
				case 23:
					_003C_003E2__current = versionShowerFx.Sequence_SafeRoom();
					_003C_003E1__state = 24;
					return true;
				case 24:
					_003C_003E2__current = versionShowerFx.InvertedReality();
					_003C_003E1__state = 25;
					return true;
				case 25:
					_003C_003E2__current = versionShowerFx.DeepZalgo();
					_003C_003E1__state = 26;
					return true;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
				_003C_003E1__state = -1;
				break;
			case 3:
				_003C_003E1__state = -1;
				break;
			case 4:
				_003C_003E1__state = -1;
				break;
			case 5:
				_003C_003E1__state = -1;
				break;
			case 6:
				_003C_003E1__state = -1;
				break;
			case 7:
				_003C_003E1__state = -1;
				break;
			case 8:
				_003C_003E1__state = -1;
				break;
			case 9:
				_003C_003E1__state = -1;
				break;
			case 10:
				_003C_003E1__state = -1;
				break;
			case 11:
				_003C_003E1__state = -1;
				break;
			case 12:
				_003C_003E1__state = -1;
				break;
			case 13:
				_003C_003E1__state = -1;
				break;
			case 14:
				_003C_003E1__state = -1;
				break;
			case 15:
				_003C_003E1__state = -1;
				break;
			case 16:
				_003C_003E1__state = -1;
				break;
			case 17:
				_003C_003E1__state = -1;
				break;
			case 18:
				_003C_003E1__state = -1;
				break;
			case 19:
				_003C_003E1__state = -1;
				break;
			case 20:
				_003C_003E1__state = -1;
				break;
			case 21:
				_003C_003E1__state = -1;
				break;
			case 22:
				_003C_003E1__state = -1;
				break;
			case 23:
				_003C_003E1__state = -1;
				break;
			case 24:
				_003C_003E1__state = -1;
				break;
			case 25:
				_003C_003E1__state = -1;
				break;
			case 26:
				_003C_003E1__state = -1;
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CNoiseFrame_003Ed__124 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CNoiseFrame_003Ed__124(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				string value = versionShowerFx.RandomNoise(20);
				string value2 = versionShowerFx.RandomNoise(20);
				versionShowerFx._textBuilder.Clear().Append(value).Append("\n")
					.Append(versionShowerFx._modText)
					.Append("\n")
					.Append(value2);
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003C_003E2__current = sWait04;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CNoiseHalo_003Ed__123 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CNoiseHalo_003Ed__123(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				string value = versionShowerFx.RandomNoise(10);
				versionShowerFx._textBuilder.Clear().Append(versionShowerFx._modText).Append(" ")
					.Append(value);
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003C_003E2__current = sWait04;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CNoiseRain_003Ed__122 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CNoiseRain_003Ed__122(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				string value = versionShowerFx.RandomNoise(18);
				string value2 = versionShowerFx.RandomNoise(18);
				versionShowerFx._textBuilder.Clear().Append(versionShowerFx._modText).Append("\n")
					.Append(value)
					.Append("\n")
					.Append(value2);
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003C_003E2__current = sWait025;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CNoiseScroll_003Ed__121 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CNoiseScroll_003Ed__121(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				string value = versionShowerFx.RandomNoise(20);
				versionShowerFx._textBuilder.Clear().Append(versionShowerFx._modText).Append("\n")
					.Append(value);
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003C_003E2__current = sWait025;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CPhantomAppearance_003Ed__104 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		public Color color;

		public string name;

		public float duration;

		private string _003CphantomText_003E5__2;

		private float _003CendTime_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CPhantomAppearance_003Ed__104(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003CphantomText_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = color;
				((TMP_Text)versionShowerFx._text).outlineColor = Color32.op_Implicit(color * 0.6f);
				((TMP_Text)versionShowerFx._text).outlineWidth = 0.4f;
				_003CphantomText_003E5__2 = versionShowerFx.CorruptText(name, 5);
				_003CendTime_003E5__3 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait08;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__3 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				versionShowerFx.SetText(_003CphantomText_003E5__2, isGlitching: true);
				_003C_003E2__current = sWait08;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CPhantomSignal_003Ed__129 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CPhantomSignal_003Ed__129(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				string content = versionShowerFx.RandomNoise(3) + " SIGNAL " + versionShowerFx.RandomNoise(3);
				versionShowerFx.SetText(content, isGlitching: true);
				((Graphic)versionShowerFx._text).color = GhostCyan;
				_003C_003E2__current = sWait025;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CPhantomTierEvent_003Ed__95 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CPhantomTierEvent_003Ed__95(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_033d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0380: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03df: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (sRandom.NextDouble() < 0.01)
				{
					_003C_003E2__current = versionShowerFx.RareScreen_SpringBonnie();
					_003C_003E1__state = 1;
					return true;
				}
				switch (sRandom.Next(0, 18))
				{
				case 0:
					_003C_003E2__current = versionShowerFx.PhantomAppearance("I SEE YOU", DeadRed, 0.8f);
					_003C_003E1__state = 2;
					return true;
				case 1:
					_003C_003E2__current = versionShowerFx.PhantomAppearance("BEHIND YOU", Color.black, 0.7f);
					_003C_003E1__state = 3;
					return true;
				case 2:
					_003C_003E2__current = versionShowerFx.PhantomAppearance("IT'S HERE", DeadRed, 0.8f);
					_003C_003E1__state = 4;
					return true;
				case 3:
					_003C_003E2__current = versionShowerFx.CrewmateColorCycle();
					_003C_003E1__state = 5;
					return true;
				case 4:
					_003C_003E2__current = versionShowerFx.PhantomSignal(1f);
					_003C_003E1__state = 6;
					return true;
				case 5:
					_003C_003E2__current = versionShowerFx.TerrifyingSequence();
					_003C_003E1__state = 7;
					return true;
				case 6:
					_003C_003E2__current = versionShowerFx.PhantomAppearance("IT'S ME", DeadRed, 0.5f);
					_003C_003E1__state = 8;
					return true;
				case 7:
					_003C_003E2__current = versionShowerFx.PhantomAppearance("SAVE THEM", GhostCyan, 0.6f);
					_003C_003E1__state = 9;
					return true;
				case 8:
					_003C_003E2__current = versionShowerFx.Sequence_SpringtrapStare();
					_003C_003E1__state = 10;
					return true;
				case 9:
					_003C_003E2__current = versionShowerFx.Sequence_PurpleGuy();
					_003C_003E1__state = 11;
					return true;
				case 10:
					_003C_003E2__current = versionShowerFx.PhantomAppearance(sSpringtrapLore[sRandom.Next(sSpringtrapLore.Length)], RottenPurple, 0.7f);
					_003C_003E1__state = 12;
					return true;
				case 11:
					_003C_003E2__current = versionShowerFx.PhantomAppearance(sVictimMessages[sRandom.Next(sVictimMessages.Length)], GhostCyan, 0.6f);
					_003C_003E1__state = 13;
					return true;
				case 12:
					_003C_003E2__current = versionShowerFx.PhantomAppearance(sGrotesqueMessages[sRandom.Next(sGrotesqueMessages.Length)], VisceralRed, 0.8f);
					_003C_003E1__state = 14;
					return true;
				case 13:
					_003C_003E2__current = versionShowerFx.PhantomAppearance(sGrotesqueFaces[sRandom.Next(sGrotesqueFaces.Length)], OxidizedGreen, 0.5f);
					_003C_003E1__state = 15;
					return true;
				case 14:
					_003C_003E2__current = versionShowerFx.PhantomAppearance("SPRING LOCK", RottenPurple, 1f);
					_003C_003E1__state = 16;
					return true;
				case 15:
					_003C_003E2__current = versionShowerFx.PhantomAppearance("GIVE LIFE", BloodDark, 0.9f);
					_003C_003E1__state = 17;
					return true;
				case 16:
					_003C_003E2__current = versionShowerFx.Sequence_PhantomFreddyWalkBy();
					_003C_003E1__state = 18;
					return true;
				case 17:
					_003C_003E2__current = versionShowerFx.Sequence_FazbearsFright();
					_003C_003E1__state = 19;
					return true;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				return false;
			case 2:
				_003C_003E1__state = -1;
				break;
			case 3:
				_003C_003E1__state = -1;
				break;
			case 4:
				_003C_003E1__state = -1;
				break;
			case 5:
				_003C_003E1__state = -1;
				break;
			case 6:
				_003C_003E1__state = -1;
				break;
			case 7:
				_003C_003E1__state = -1;
				break;
			case 8:
				_003C_003E1__state = -1;
				break;
			case 9:
				_003C_003E1__state = -1;
				break;
			case 10:
				_003C_003E1__state = -1;
				break;
			case 11:
				_003C_003E1__state = -1;
				break;
			case 12:
				_003C_003E1__state = -1;
				break;
			case 13:
				_003C_003E1__state = -1;
				break;
			case 14:
				_003C_003E1__state = -1;
				break;
			case 15:
				_003C_003E1__state = -1;
				break;
			case 16:
				_003C_003E1__state = -1;
				break;
			case 17:
				_003C_003E1__state = -1;
				break;
			case 18:
				_003C_003E1__state = -1;
				break;
			case 19:
				_003C_003E1__state = -1;
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CPowerOutage_003Ed__147 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CPowerOutage_003Ed__147(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = AlertOrange;
				versionShowerFx.SetText("POWER: 1%", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_00f4;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_00f4;
			case 3:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = Color.white;
				versionShowerFx.SetText("◉     ◉", isGlitching: true);
				_003C_003E2__current = sWait20s;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("IT'S ME", isGlitching: true);
				_003C_003E2__current = sWait08;
				_003C_003E1__state = 5;
				return true;
			case 5:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00f4:
				if (_003Ci_003E5__2 < 6 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					((Graphic)versionShowerFx._text).color = ((_003Ci_003E5__2 % 2 == 0) ? Color.white : Color.black);
					versionShowerFx.SetText((_003Ci_003E5__2 % 2 == 0) ? versionShowerFx._modText : "", isGlitching: true);
					_003C_003E2__current = ((sRandom.Next(0, 2) == 0) ? sWait08 : sWait1);
					_003C_003E1__state = 2;
					return true;
				}
				((Graphic)versionShowerFx._text).color = Color.black;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15s;
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CRareScreen_SpringBonnie_003Ed__162 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string[] _003Creveal_003E5__2;

		private int _003Ci_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CRareScreen_SpringBonnie_003Ed__162(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Creveal_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Creveal_003E5__2 = sRevealFrames;
				((Graphic)versionShowerFx._text).color = GoldenYellow;
				_003Ci_003E5__3 = 0;
				goto IL_0118;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__3++;
				goto IL_0118;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.StaticWall(0.5f);
				_003C_003E1__state = 4;
				return true;
			case 4:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0118:
				if (_003Ci_003E5__3 < _003Creveal_003E5__2.Length)
				{
					if ((Object)(object)versionShowerFx._text == (Object)null)
					{
						return false;
					}
					versionShowerFx.SetText(_003Creveal_003E5__2[_003Ci_003E5__3], isGlitching: true);
					if (_003Ci_003E5__3 > _003Creveal_003E5__2.Length / 2)
					{
						((Graphic)versionShowerFx._text).color = Color.Lerp(GoldenYellow, VisceralRed, (float)(_003Ci_003E5__3 - _003Creveal_003E5__2.Length / 2) / (float)(_003Creveal_003E5__2.Length / 2));
					}
					_003C_003E2__current = sWait5;
					_003C_003E1__state = 2;
					return true;
				}
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 2f, versionShowerFx._baseScale * 2f, 1f);
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("IT'S ME", isGlitching: true);
				_003C_003E2__current = versionShowerFx.Jitter(0.5f, 8f, 15f);
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CRareTierEvent_003Ed__93 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CRareTierEvent_003Ed__93(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0534: Unknown result type (might be due to invalid IL or missing references)
			//IL_0565: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ea: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				switch (sRandom.Next(0, 36))
				{
				case 0:
					_003C_003E2__current = versionShowerFx.ImpostorFlash(DeadRed);
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E2__current = versionShowerFx.TextCorruption(1f, 15, sSystemMessages[sRandom.Next(sSystemMessages.Length)]);
					_003C_003E1__state = 2;
					return true;
				case 2:
					_003C_003E2__current = versionShowerFx.Jitter(0.8f, 4f, 5f);
					_003C_003E1__state = 3;
					return true;
				case 3:
					_003C_003E2__current = versionShowerFx.SystemWarning("O2 DEPLETED", 1f, AlertOrange);
					_003C_003E1__state = 4;
					return true;
				case 4:
					_003C_003E2__current = versionShowerFx.SystemWarning("REACTOR CRITICAL", 1.2f, DeadRed);
					_003C_003E1__state = 5;
					return true;
				case 5:
					_003C_003E2__current = versionShowerFx.TrackingNoise(0.9f);
					_003C_003E1__state = 6;
					return true;
				case 6:
					_003C_003E2__current = versionShowerFx.NoiseFrame(0.8f);
					_003C_003E1__state = 7;
					return true;
				case 7:
					_003C_003E2__current = versionShowerFx.ColorDrain(1f);
					_003C_003E1__state = 8;
					return true;
				case 8:
					_003C_003E2__current = versionShowerFx.TypewriterText(sSystemMessages[sRandom.Next(sSystemMessages.Length)], 0.05f);
					_003C_003E1__state = 9;
					return true;
				case 9:
					_003C_003E2__current = versionShowerFx.CharacterSwapGlitch(0.8f);
					_003C_003E1__state = 10;
					return true;
				case 10:
					_003C_003E2__current = versionShowerFx.BurnInPulse(0.9f);
					_003C_003E1__state = 11;
					return true;
				case 11:
					_003C_003E2__current = versionShowerFx.CRTCrosstalk(0.8f);
					_003C_003E1__state = 12;
					return true;
				case 12:
					_003C_003E2__current = versionShowerFx.HeartbeatHorror();
					_003C_003E1__state = 13;
					return true;
				case 13:
					_003C_003E2__current = versionShowerFx.BloodDripEffect();
					_003C_003E1__state = 14;
					return true;
				case 14:
					_003C_003E2__current = versionShowerFx.VentilationError();
					_003C_003E1__state = 15;
					return true;
				case 15:
					_003C_003E2__current = versionShowerFx.StaticWall(0.8f);
					_003C_003E1__state = 16;
					return true;
				case 16:
					_003C_003E2__current = versionShowerFx.FaceFlash();
					_003C_003E1__state = 17;
					return true;
				case 17:
					_003C_003E2__current = versionShowerFx.AudioDistortion(0.7f);
					_003C_003E1__state = 18;
					return true;
				case 18:
					_003C_003E2__current = versionShowerFx.SpringtrapWarning();
					_003C_003E1__state = 19;
					return true;
				case 19:
					_003C_003E2__current = versionShowerFx.CameraDisabled();
					_003C_003E1__state = 20;
					return true;
				case 20:
					_003C_003E2__current = versionShowerFx.ScanlineEffect(0.8f);
					_003C_003E1__state = 21;
					return true;
				case 21:
					_003C_003E2__current = versionShowerFx.SlowCorruption();
					_003C_003E1__state = 22;
					return true;
				case 22:
					_003C_003E2__current = versionShowerFx.BinaryHorror();
					_003C_003E1__state = 23;
					return true;
				case 23:
					_003C_003E2__current = versionShowerFx.ScrambledMessage(sSystemMessages[sRandom.Next(sSystemMessages.Length)]);
					_003C_003E1__state = 24;
					return true;
				case 24:
					_003C_003E2__current = versionShowerFx.ImpostorFlash(CursedPurple);
					_003C_003E1__state = 25;
					return true;
				case 25:
					_003C_003E2__current = versionShowerFx.SystemWarning("MEMORY LEAK", 0.9f, GhostCyan);
					_003C_003E1__state = 26;
					return true;
				case 26:
					_003C_003E2__current = versionShowerFx.FleshReveal();
					_003C_003E1__state = 27;
					return true;
				case 27:
					_003C_003E2__current = versionShowerFx.CorpseReveal();
					_003C_003E1__state = 28;
					return true;
				case 28:
					_003C_003E2__current = versionShowerFx.VictimsCrying();
					_003C_003E1__state = 29;
					return true;
				case 29:
					_003C_003E2__current = versionShowerFx.EndoskeletonExposed();
					_003C_003E1__state = 30;
					return true;
				case 30:
					_003C_003E2__current = versionShowerFx.TypewriterText(sSpringtrapLore[sRandom.Next(sSpringtrapLore.Length)], 0.06f);
					_003C_003E1__state = 31;
					return true;
				case 31:
					_003C_003E2__current = versionShowerFx.TypewriterText(sVictimMessages[sRandom.Next(sVictimMessages.Length)], 0.05f);
					_003C_003E1__state = 32;
					return true;
				case 32:
					_003C_003E2__current = versionShowerFx.TypewriterText(sGrotesqueMessages[sRandom.Next(sGrotesqueMessages.Length)], 0.04f);
					_003C_003E1__state = 33;
					return true;
				case 33:
					_003C_003E2__current = versionShowerFx.SystemWarning(sGrotesqueMessages[sRandom.Next(sGrotesqueMessages.Length)], 1f, VisceralRed);
					_003C_003E1__state = 34;
					return true;
				case 34:
					_003C_003E2__current = versionShowerFx.HorrorFaceFlash();
					_003C_003E1__state = 35;
					return true;
				case 35:
					_003C_003E2__current = versionShowerFx.TypewriterText(sFnaf3Authentic[sRandom.Next(sFnaf3Authentic.Length)], 0.06f);
					_003C_003E1__state = 36;
					return true;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
				_003C_003E1__state = -1;
				break;
			case 3:
				_003C_003E1__state = -1;
				break;
			case 4:
				_003C_003E1__state = -1;
				break;
			case 5:
				_003C_003E1__state = -1;
				break;
			case 6:
				_003C_003E1__state = -1;
				break;
			case 7:
				_003C_003E1__state = -1;
				break;
			case 8:
				_003C_003E1__state = -1;
				break;
			case 9:
				_003C_003E1__state = -1;
				break;
			case 10:
				_003C_003E1__state = -1;
				break;
			case 11:
				_003C_003E1__state = -1;
				break;
			case 12:
				_003C_003E1__state = -1;
				break;
			case 13:
				_003C_003E1__state = -1;
				break;
			case 14:
				_003C_003E1__state = -1;
				break;
			case 15:
				_003C_003E1__state = -1;
				break;
			case 16:
				_003C_003E1__state = -1;
				break;
			case 17:
				_003C_003E1__state = -1;
				break;
			case 18:
				_003C_003E1__state = -1;
				break;
			case 19:
				_003C_003E1__state = -1;
				break;
			case 20:
				_003C_003E1__state = -1;
				break;
			case 21:
				_003C_003E1__state = -1;
				break;
			case 22:
				_003C_003E1__state = -1;
				break;
			case 23:
				_003C_003E1__state = -1;
				break;
			case 24:
				_003C_003E1__state = -1;
				break;
			case 25:
				_003C_003E1__state = -1;
				break;
			case 26:
				_003C_003E1__state = -1;
				break;
			case 27:
				_003C_003E1__state = -1;
				break;
			case 28:
				_003C_003E1__state = -1;
				break;
			case 29:
				_003C_003E1__state = -1;
				break;
			case 30:
				_003C_003E1__state = -1;
				break;
			case 31:
				_003C_003E1__state = -1;
				break;
			case 32:
				_003C_003E1__state = -1;
				break;
			case 33:
				_003C_003E1__state = -1;
				break;
			case 34:
				_003C_003E1__state = -1;
				break;
			case 35:
				_003C_003E1__state = -1;
				break;
			case 36:
				_003C_003E1__state = -1;
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CScanlineEffect_003Ed__144 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		private int _003CscanPos_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CScanlineEffect_003Ed__144(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				_003CscanPos_003E5__3 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				versionShowerFx._textBuilder.Clear();
				for (int i = 0; i < 4; i++)
				{
					if (i == _003CscanPos_003E5__3 % 4)
					{
						versionShowerFx._textBuilder.Append("<color=#FFFFFF>█████████████████</color>\n");
					}
					else
					{
						versionShowerFx._textBuilder.Append("<color=#333333>░░░░░░░░░░░░░░░░░</color>\n");
					}
				}
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003CscanPos_003E5__3++;
				_003C_003E2__current = sWait025;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CScrambledMessage_003Ed__146 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string secret;

		public VersionShowerFx _003C_003E4__this;

		private int _003Clen_003E5__2;

		private char[] _003Cdisplay_003E5__3;

		private bool[] _003Crevealed_003E5__4;

		private int _003Cpass_003E5__5;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CScrambledMessage_003Ed__146(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Cdisplay_003E5__3 = null;
			_003Crevealed_003E5__4 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				if (string.IsNullOrEmpty(secret))
				{
					return false;
				}
				_003Clen_003E5__2 = secret.Length;
				_003Cdisplay_003E5__3 = new char[_003Clen_003E5__2];
				_003Crevealed_003E5__4 = new bool[_003Clen_003E5__2];
				for (int i = 0; i < _003Clen_003E5__2; i++)
				{
					_003Cdisplay_003E5__3[i] = sNoisePool[sRandom.Next(sNoisePool.Length)];
				}
				_003Cpass_003E5__5 = 0;
				goto IL_01c3;
			}
			case 1:
				_003C_003E1__state = -1;
				_003Cpass_003E5__5++;
				goto IL_01c3;
			case 2:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_01c3:
				if (_003Cpass_003E5__5 < _003Clen_003E5__2 + 5 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					if (_003Cpass_003E5__5 < _003Clen_003E5__2 && sRandom.NextDouble() > 0.3)
					{
						int num2 = sRandom.Next(_003Clen_003E5__2);
						if (!_003Crevealed_003E5__4[num2])
						{
							_003Crevealed_003E5__4[num2] = true;
							_003Cdisplay_003E5__3[num2] = secret[num2];
						}
					}
					for (int j = 0; j < _003Clen_003E5__2; j++)
					{
						if (!_003Crevealed_003E5__4[j])
						{
							_003Cdisplay_003E5__3[j] = sNoisePool[sRandom.Next(sNoisePool.Length)];
						}
					}
					((Graphic)versionShowerFx._text).color = Color.Lerp(GhostCyan, DeadRed, (float)_003Cpass_003E5__5 / (float)(_003Clen_003E5__2 + 5));
					versionShowerFx._textBuilder.Clear().Append(_003Cdisplay_003E5__3);
					versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
					_003C_003E2__current = sWait08;
					_003C_003E1__state = 1;
					return true;
				}
				versionShowerFx.SetText(secret, isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_6AM_003Ed__150 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		private float _003Ct_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_6AM_003Ed__150(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_00a3;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_00a3;
			case 3:
				_003C_003E1__state = -1;
				_003Ct_003E5__3 = 0f;
				goto IL_0178;
			case 4:
				_003C_003E1__state = -1;
				_003Ct_003E5__3 += 0.1f;
				goto IL_0178;
			case 5:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("THIS TIME", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 6;
				return true;
			case 6:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0178:
				if (_003Ct_003E5__3 < 1f && (Object)(object)versionShowerFx._text != (Object)null)
				{
					((Graphic)versionShowerFx._text).color = Color.Lerp(GoldenYellow, Color.white, _003Ct_003E5__3);
					_003C_003E2__current = sWait1;
					_003C_003E1__state = 4;
					return true;
				}
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale, versionShowerFx._baseScale, 1f);
				versionShowerFx.SetText("YOU SURVIVED...", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 5;
				return true;
				IL_00a3:
				if (_003Ci_003E5__2 < 4 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					versionShowerFx.SetText(versionShowerFx.RandomNoise(20), isGlitching: true);
					_003C_003E2__current = sWait05;
					_003C_003E1__state = 2;
					return true;
				}
				((Graphic)versionShowerFx._text).color = GoldenYellow;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 1.8f, versionShowerFx._baseScale * 1.8f, 1f);
				versionShowerFx.SetText("6 AM", isGlitching: true);
				_003C_003E2__current = sWait20s;
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_AftonDecomposition_003Ed__155 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_AftonDecomposition_003Ed__155(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_012b;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.Jitter(0.2f, 1f + (float)_003Ci_003E5__2, 3f);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_012b;
			case 4:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_012b:
				if (_003Ci_003E5__2 < sDecompositionStages.Length && (Object)(object)versionShowerFx._text != (Object)null)
				{
					TextMeshPro text = versionShowerFx._text;
					((Graphic)text).color = (Color)(_003Ci_003E5__2 switch
					{
						0 => FleshPink, 
						1 => DecayBrown, 
						2 => BoneWhite, 
						3 => StaticGray, 
						4 => OxidizedGreen, 
						_ => RottenPurple, 
					});
					_003C_003E2__current = versionShowerFx.TypewriterText(sDecompositionStages[_003Ci_003E5__2], 0.05f);
					_003C_003E1__state = 1;
					return true;
				}
				((Graphic)versionShowerFx._text).color = DeadRed;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 1.5f, versionShowerFx._baseScale * 1.5f, 1f);
				versionShowerFx.SetText("I AM STILL HERE", isGlitching: true);
				_003C_003E2__current = sWait20s;
				_003C_003E1__state = 4;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_BadEnding_003Ed__141 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private float _003Ct_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_BadEnding_003Ed__141(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.TypewriterText("GAME OVER", 0.15f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = BloodDark;
				_003C_003E2__current = versionShowerFx.TypewriterText("BAD ENDING", 0.1f);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003Ct_003E5__2 = 0f;
				goto IL_0125;
			case 5:
				_003C_003E1__state = -1;
				_003Ct_003E5__2 += 0.1f;
				goto IL_0125;
			case 6:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0125:
				if (_003Ct_003E5__2 < 1f)
				{
					((Graphic)versionShowerFx._text).color = Color.Lerp(BloodDark, DeadRed, _003Ct_003E5__2);
					_003C_003E2__current = sWait1;
					_003C_003E1__state = 5;
					return true;
				}
				_003C_003E2__current = versionShowerFx.StaticWall(0.5f);
				_003C_003E1__state = 6;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_CriticalBreach_003Ed__103 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_CriticalBreach_003Ed__103(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.IntensiveGlitchSequence(1.2f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.SystemWarning("BREACH", 1.2f, DeadRed);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.TypewriterText("INTRUDER", 0.08f);
				_003C_003E1__state = 5;
				return true;
			case 5:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_Dismemberment_003Ed__161 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string _003Ctext_003E5__2;

		private int _003Clen_003E5__3;

		private int _003Ci_003E5__4;

		private int _003Cdrop_003E5__5;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_Dismemberment_003Ed__161(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Ctext_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ctext_003E5__2 = versionShowerFx._modText;
				_003Clen_003E5__3 = _003Ctext_003E5__2.Length;
				((Graphic)versionShowerFx._text).color = versionShowerFx._baseColor;
				versionShowerFx.SetText(_003Ctext_003E5__2, isGlitching: true);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__4 = _003Clen_003E5__3 - 1;
				goto IL_0195;
			case 2:
				_003C_003E1__state = -1;
				_003Cdrop_003E5__5++;
				goto IL_0149;
			case 3:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0195:
				if (_003Ci_003E5__4 >= 0 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					_003Cdrop_003E5__5 = 0;
					goto IL_0149;
				}
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("▓▒░ DESTROYED ░▒▓", isGlitching: true);
				_003C_003E2__current = versionShowerFx.Jitter(0.4f, 4f, 8f);
				_003C_003E1__state = 3;
				return true;
				IL_0149:
				if (_003Cdrop_003E5__5 < 3)
				{
					versionShowerFx._textBuilder.Clear();
					versionShowerFx._textBuilder.Append(_003Ctext_003E5__2, 0, _003Ci_003E5__4);
					for (int i = 0; i < _003Cdrop_003E5__5; i++)
					{
						versionShowerFx._textBuilder.Append(" ");
					}
					versionShowerFx._textBuilder.Append(sNoisePool[sRandom.Next(sNoisePool.Length)]);
					versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
					_003C_003E2__current = sWait025;
					_003C_003E1__state = 2;
					return true;
				}
				((Graphic)versionShowerFx._text).color = Color.Lerp(versionShowerFx._baseColor, DeadRed, 1f - (float)_003Ci_003E5__4 / (float)_003Clen_003E5__3);
				_003Ci_003E5__4--;
				goto IL_0195;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_EmergencyMeeting_003Ed__99 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_EmergencyMeeting_003Ed__99(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.TypewriterText("WHO IS THE IMPOSTOR?", 0.06f);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.Jitter(0.5f, 5f, 5f);
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 5;
				return true;
			case 5:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_FazbearsFright_003Ed__167 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string _003Cheadline_003E5__2;

		private char[] _003Cburning_003E5__3;

		private int _003Cpass_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_FazbearsFright_003Ed__167(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Cheadline_003E5__2 = null;
			_003Cburning_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_030f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0347: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = BoneWhite;
				_003C_003E2__current = versionShowerFx.TypewriterText("LOCAL NEWS", 0.08f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait5;
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = Color.white;
				_003C_003E2__current = versionShowerFx.TypewriterText("FAZBEAR'S FRIGHT", 0.06f);
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 5;
				return true;
			case 5:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = AlertOrange;
				_003C_003E2__current = versionShowerFx.TypewriterText("BURNS DOWN", 0.1f);
				_003C_003E1__state = 6;
				return true;
			case 6:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 7;
				return true;
			case 7:
				_003C_003E1__state = -1;
				_003Cheadline_003E5__2 = "FAZBEAR'S FRIGHT";
				_003Cburning_003E5__3 = _003Cheadline_003E5__2.ToCharArray();
				_003Cpass_003E5__4 = 0;
				goto IL_02bd;
			case 8:
				_003C_003E1__state = -1;
				_003Cpass_003E5__4++;
				goto IL_02bd;
			case 9:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = StaticGray;
				versionShowerFx.SetText("ASHES", isGlitching: true);
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 10;
				return true;
			case 10:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = RottenPurple;
				versionShowerFx.SetText("BUT HE SURVIVED", isGlitching: true);
				_003C_003E2__current = sWait20s;
				_003C_003E1__state = 11;
				return true;
			case 11:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_02bd:
				if (_003Cpass_003E5__4 < _003Cheadline_003E5__2.Length && (Object)(object)versionShowerFx._text != (Object)null)
				{
					int num2 = sRandom.Next(_003Cburning_003E5__3.Length);
					_003Cburning_003E5__3[num2] = sFireChars[sRandom.Next(sFireChars.Length)];
					((Graphic)versionShowerFx._text).color = Color.Lerp(AlertOrange, DeadRed, (float)_003Cpass_003E5__4 / (float)_003Cheadline_003E5__2.Length);
					versionShowerFx._textBuilder.Clear().Append(_003Cburning_003E5__3);
					versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
					float num3 = ((float)sRandom.NextDouble() - 0.5f) * (1f + (float)_003Cpass_003E5__4 * 0.3f);
					float num4 = ((float)sRandom.NextDouble() - 0.5f) * (1f + (float)_003Cpass_003E5__4 * 0.3f);
					versionShowerFx._textRectTransform.anchoredPosition = new Vector2(versionShowerFx._baseAnchoredPosition.x + num3, versionShowerFx._baseAnchoredPosition.y + num4);
					_003C_003E2__current = sWait08;
					_003C_003E1__state = 8;
					return true;
				}
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 9;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_GoldenFreddy_003Ed__151 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_GoldenFreddy_003Ed__151(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15s;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = GoldenYellow;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 2.5f, versionShowerFx._baseScale * 2.5f, 1f);
				versionShowerFx.SetText("IT'S ME", isGlitching: true);
				_003C_003E2__current = sWait05;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_018c;
			case 3:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_018c;
			case 4:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.StaticWall(0.5f);
				_003C_003E1__state = 5;
				return true;
			case 5:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_018c:
				if (_003Ci_003E5__2 < 8 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					((Graphic)versionShowerFx._text).color = ((_003Ci_003E5__2 % 2 == 0) ? GoldenYellow : Color.black);
					versionShowerFx.SetText(versionShowerFx.CorruptText("IT'S ME", _003Ci_003E5__2), isGlitching: true);
					float num2 = ((float)sRandom.NextDouble() - 0.5f) * 10f;
					float num3 = ((float)sRandom.NextDouble() - 0.5f) * 10f;
					versionShowerFx._textRectTransform.anchoredPosition = new Vector2(versionShowerFx._baseAnchoredPosition.x + num2, versionShowerFx._baseAnchoredPosition.y + num3);
					_003C_003E2__current = sWait025;
					_003C_003E1__state = 3;
					return true;
				}
				((Graphic)versionShowerFx._text).color = Color.white;
				versionShowerFx.SetText("FATAL ERROR", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 4;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_ImpostorReveal_003Ed__100 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_ImpostorReveal_003Ed__100(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.TypewriterText("THE IMPOSTOR IS...", 0.08f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.Jitter(0.4f, 15f, 15f);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.ImpostorFlash(DeadRed);
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				versionShowerFx.SetText(versionShowerFx.CorruptText("IMPOSTOR", 10), isGlitching: true);
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 5;
				return true;
			case 5:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_NightmareFredbear_003Ed__160 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_NightmareFredbear_003Ed__160(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0274: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = BloodDark;
				_003Ci_003E5__2 = 0;
				goto IL_00cb;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_00cb;
			case 3:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = GoldenYellow;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 3f, versionShowerFx._baseScale * 3f, 1f);
				versionShowerFx.SetText("▀▀▀ TEETH ▀▀▀", isGlitching: true);
				_003C_003E2__current = sWait05;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_0247;
			case 5:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_0247;
			case 6:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale, versionShowerFx._baseScale, 1f);
				versionShowerFx.SetText("NIGHTMARE", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 7;
				return true;
			case 7:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0247:
				if (_003Ci_003E5__2 < 10 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					((Graphic)versionShowerFx._text).color = ((_003Ci_003E5__2 % 2 == 0) ? GoldenYellow : DeadRed);
					string input = sGrotesqueFaces[sRandom.Next(sGrotesqueFaces.Length)];
					versionShowerFx.SetText(versionShowerFx.CorruptText(input, _003Ci_003E5__2), isGlitching: true);
					float num2 = ((float)sRandom.NextDouble() - 0.5f) * 12f;
					float num3 = ((float)sRandom.NextDouble() - 0.5f) * 12f;
					versionShowerFx._textRectTransform.anchoredPosition = new Vector2(versionShowerFx._baseAnchoredPosition.x + num2, versionShowerFx._baseAnchoredPosition.y + num3);
					_003C_003E2__current = sWait025;
					_003C_003E1__state = 5;
					return true;
				}
				versionShowerFx.SetText("", isGlitching: true);
				((Graphic)versionShowerFx._text).color = Color.black;
				_003C_003E2__current = sWait20s;
				_003C_003E1__state = 6;
				return true;
				IL_00cb:
				if (_003Ci_003E5__2 < sWhispers.Length)
				{
					if ((Object)(object)versionShowerFx._text == (Object)null)
					{
						return false;
					}
					versionShowerFx.SetText(sWhispers[_003Ci_003E5__2], isGlitching: true);
					_003C_003E2__current = sWait6;
					_003C_003E1__state = 2;
					return true;
				}
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15s;
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_NightmareMode_003Ed__140 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_NightmareMode_003Ed__140(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				float num2 = 2f;
				_003CendTime_003E5__2 = Time.time + num2;
				goto IL_0113;
			}
			case 1:
				_003C_003E1__state = -1;
				goto IL_0113;
			case 2:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0113:
				if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					((Graphic)versionShowerFx._text).color = new Color((float)sRandom.NextDouble(), (float)sRandom.NextDouble(), (float)sRandom.NextDouble());
					string input = sSystemMessages[sRandom.Next(sSystemMessages.Length)];
					versionShowerFx.SetText(versionShowerFx.CorruptText(input, 10), isGlitching: true);
					float num3 = ((float)sRandom.NextDouble() - 0.5f) * 6f;
					float num4 = ((float)sRandom.NextDouble() - 0.5f) * 6f;
					versionShowerFx._textRectTransform.anchoredPosition = new Vector2(versionShowerFx._baseAnchoredPosition.x + num3, versionShowerFx._baseAnchoredPosition.y + num4);
					_003C_003E2__current = sWait025;
					_003C_003E1__state = 1;
					return true;
				}
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("NIGHTMARE", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_PhantomFreddyWalkBy_003Ed__166 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_PhantomFreddyWalkBy_003Ed__166(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait5;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = OxidizedGreen;
				((TMP_Text)versionShowerFx._text).outlineColor = Color32.op_Implicit(Color.black);
				((TMP_Text)versionShowerFx._text).outlineWidth = 0.3f;
				_003Ci_003E5__2 = 0;
				goto IL_0156;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_0156;
			case 3:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 1.8f, versionShowerFx._baseScale * 1.8f, 1f);
				versionShowerFx.SetText("◉_◉", isGlitching: true);
				_003C_003E2__current = sWait3;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.Jitter(0.3f, 6f, 12f);
				_003C_003E1__state = 5;
				return true;
			case 5:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0156:
				if (_003Ci_003E5__2 < sWalkFrames.Length && (Object)(object)versionShowerFx._text != (Object)null)
				{
					float num2 = Mathf.Clamp01((float)_003Ci_003E5__2 / ((float)sWalkFrames.Length * 0.6f));
					((Graphic)versionShowerFx._text).color = Color.Lerp(Color.black, OxidizedGreen, num2);
					versionShowerFx.SetText(sWalkFrames[_003Ci_003E5__2], isGlitching: true);
					float num3 = Mathf.Sin((float)_003Ci_003E5__2 * 1.2f) * 1.5f;
					versionShowerFx._textRectTransform.anchoredPosition = new Vector2(versionShowerFx._baseAnchoredPosition.x, versionShowerFx._baseAnchoredPosition.y + num3);
					_003C_003E2__current = sWait2;
					_003C_003E1__state = 2;
					return true;
				}
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_PhantomJumpscare_003Ed__139 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_PhantomJumpscare_003Ed__139(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			string content;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_00d2;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_00d2;
			case 3:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				((Graphic)versionShowerFx._text).color = Color.black;
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = AlertOrange;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale, versionShowerFx._baseScale, 1f);
				versionShowerFx.SetText("VENT: ERROR", isGlitching: true);
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 5;
				return true;
			case 5:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = FnafGreen;
				_003C_003E2__current = versionShowerFx.TypewriterText("REBOOTING...", 0.05f);
				_003C_003E1__state = 6;
				return true;
			case 6:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait5;
				_003C_003E1__state = 7;
				return true;
			case 7:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("VENT: ONLINE", isGlitching: true);
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 8;
				return true;
			case 8:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00d2:
				if (_003Ci_003E5__2 < 4 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					((Graphic)versionShowerFx._text).color = Color.Lerp(StaticGray, OxidizedGreen, (float)_003Ci_003E5__2 / 3f);
					versionShowerFx.SetText(versionShowerFx.RandomNoise(20), isGlitching: true);
					_003C_003E2__current = sWait08;
					_003C_003E1__state = 2;
					return true;
				}
				((Graphic)versionShowerFx._text).color = OxidizedGreen;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 2.5f, versionShowerFx._baseScale * 2.5f, 1f);
				content = sGrotesqueFaces[sRandom.Next(sGrotesqueFaces.Length)];
				versionShowerFx.SetText(content, isGlitching: true);
				_003C_003E2__current = versionShowerFx.Jitter(0.4f, 10f, 18f);
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_PhantomMangle_003Ed__158 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_PhantomMangle_003Ed__158(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = OxidizedGreen;
				_003Ci_003E5__2 = 0;
				goto IL_0093;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_0093;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_012b;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.StaticWall(0.4f);
				_003C_003E1__state = 4;
				return true;
			case 4:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0093:
				if (_003Ci_003E5__2 < 6 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					versionShowerFx.SetText(sAudioPatterns[sRandom.Next(sAudioPatterns.Length)], isGlitching: true);
					_003C_003E2__current = sWait15;
					_003C_003E1__state = 1;
					return true;
				}
				_003Ci_003E5__2 = 0;
				goto IL_012b;
				IL_012b:
				if (_003Ci_003E5__2 < sMangleFrames.Length)
				{
					if ((Object)(object)versionShowerFx._text == (Object)null)
					{
						return false;
					}
					((Graphic)versionShowerFx._text).color = Color.Lerp(OxidizedGreen, DeadRed, (float)_003Ci_003E5__2 / (float)sMangleFrames.Length);
					versionShowerFx.SetText(sMangleFrames[_003Ci_003E5__2], isGlitching: true);
					_003C_003E2__current = sWait08;
					_003C_003E1__state = 2;
					return true;
				}
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 2f, versionShowerFx._baseScale * 2f, 1f);
				versionShowerFx.SetText(sGrotesqueFaces[sRandom.Next(sGrotesqueFaces.Length)], isGlitching: true);
				_003C_003E2__current = versionShowerFx.Jitter(0.3f, 5f, 10f);
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_PurpleGuy_003Ed__143 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private Color _003CpurpleGuy_003E5__2;

		private int _003Ci_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_PurpleGuy_003Ed__143(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CpurpleGuy_003E5__2 = new Color(0.5f, 0f, 0.5f);
				((Graphic)versionShowerFx._text).color = _003CpurpleGuy_003E5__2;
				_003C_003E2__current = versionShowerFx.TypewriterText("I ALWAYS COME BACK", 0.08f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__3 = 0;
				goto IL_0111;
			case 3:
				_003C_003E1__state = -1;
				_003Ci_003E5__3++;
				goto IL_0111;
			case 4:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("...", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 5;
				return true;
			case 5:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0111:
				if (_003Ci_003E5__3 < 5)
				{
					((Graphic)versionShowerFx._text).color = ((_003Ci_003E5__3 % 2 == 0) ? _003CpurpleGuy_003E5__2 : Color.black);
					versionShowerFx.SetText(versionShowerFx.CorruptText("I ALWAYS COME BACK", _003Ci_003E5__3 * 2), isGlitching: true);
					_003C_003E2__current = sWait1;
					_003C_003E1__state = 3;
					return true;
				}
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 4;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_SabotageCritical_003Ed__102 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private float _003Cduration_003E5__2;

		private float _003CendTime_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_SabotageCritical_003Ed__102(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cduration_003E5__2 = 1.5f;
				_003CendTime_003E5__3 = Time.time + _003Cduration_003E5__2;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__3 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				float num2 = 1f - (_003CendTime_003E5__3 - Time.time) / _003Cduration_003E5__2;
				Color color = ((Time.frameCount % 8 < 4) ? DeadRed : Color.yellow);
				((Graphic)versionShowerFx._text).color = color;
				versionShowerFx.SetText((sRandom.Next(0, 2) == 1) ? "SABOTAGE" : versionShowerFx.CorruptText("SABOTAGE", (int)(num2 * 15f)), isGlitching: true);
				_003C_003E2__current = sWait04;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_SafeRoom_003Ed__168 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_SafeRoom_003Ed__168(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0314: Unknown result type (might be due to invalid IL or missing references)
			//IL_0367: Unknown result type (might be due to invalid IL or missing references)
			//IL_039f: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = RottenPurple;
				_003C_003E2__current = versionShowerFx.TypewriterText("FOLLOW ME", 0.12f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = GhostCyan;
				_003Ci_003E5__2 = 0;
				goto IL_0128;
			case 3:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_0128;
			case 5:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 6;
				return true;
			case 6:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = GoldenYellow;
				versionShowerFx.SetText("THE SUIT", isGlitching: true);
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 7;
				return true;
			case 7:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = RottenPurple;
				_003Ci_003E5__2 = 0;
				goto IL_0299;
			case 8:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait3;
				_003C_003E1__state = 9;
				return true;
			case 9:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_0299;
			case 10:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = BloodDark;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 0.8f, versionShowerFx._baseScale * 0.8f, 1f);
				_003C_003E2__current = versionShowerFx.TypewriterText("SEALED INSIDE", 0.15f);
				_003C_003E1__state = 11;
				return true;
			case 11:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 12;
				return true;
			case 12:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DecayBrown;
				versionShowerFx.SetText("30 YEARS...", isGlitching: true);
				_003C_003E2__current = sWait20s;
				_003C_003E1__state = 13;
				return true;
			case 13:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 1.5f, versionShowerFx._baseScale * 1.5f, 1f);
				versionShowerFx.SetText("I AM STILL HERE", isGlitching: true);
				_003C_003E2__current = versionShowerFx.Jitter(0.5f, 5f, 10f);
				_003C_003E1__state = 14;
				return true;
			case 14:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0299:
				if (_003Ci_003E5__2 < sSpringLockSounds.Length && (Object)(object)versionShowerFx._text != (Object)null)
				{
					versionShowerFx.SetText(sSpringLockSounds[_003Ci_003E5__2], isGlitching: true);
					((Graphic)versionShowerFx._text).color = Color.Lerp(RottenPurple, DeadRed, (float)_003Ci_003E5__2 / 3f);
					_003C_003E2__current = versionShowerFx.Jitter(0.1f, 1f + (float)_003Ci_003E5__2 * 2f, 3f + (float)_003Ci_003E5__2 * 3f);
					_003C_003E1__state = 8;
					return true;
				}
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15s;
				_003C_003E1__state = 10;
				return true;
				IL_0128:
				if (_003Ci_003E5__2 < sSafeRoomChildren.Length && (Object)(object)versionShowerFx._text != (Object)null)
				{
					versionShowerFx.SetText(sSafeRoomChildren[_003Ci_003E5__2], isGlitching: true);
					_003C_003E2__current = sWait5;
					_003C_003E1__state = 3;
					return true;
				}
				((Graphic)versionShowerFx._text).color = StaticGray;
				_003C_003E2__current = versionShowerFx.TypewriterText("SAFE ROOM", 0.1f);
				_003C_003E1__state = 5;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_SpringLockFailure_003Ed__152 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_SpringLockFailure_003Ed__152(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0287: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = RottenPurple;
				_003C_003E2__current = versionShowerFx.TypewriterText("SPRING LOCKS...", 0.08f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_0188;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait08;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_0188;
			case 5:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_0240;
			case 6:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = BloodDark;
				versionShowerFx.SetText("...", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 7;
				return true;
			case 7:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0188:
				if (_003Ci_003E5__2 < 5 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					string input = _003Ci_003E5__2 switch
					{
						0 => "* CRACK *", 
						1 => "* SNAP! *", 
						2 => "* CRUNCH *", 
						3 => "AAAHHH!!!", 
						_ => "* SQUELCH *", 
					};
					((Graphic)versionShowerFx._text).color = Color.Lerp(RottenPurple, VisceralRed, (float)_003Ci_003E5__2 / 4f);
					versionShowerFx.SetText(versionShowerFx.CorruptText(input, _003Ci_003E5__2), isGlitching: true);
					_003C_003E2__current = versionShowerFx.Jitter(0.15f, 2f + (float)_003Ci_003E5__2, 5f + (float)(_003Ci_003E5__2 * 2));
					_003C_003E1__state = 3;
					return true;
				}
				((Graphic)versionShowerFx._text).color = DeadRed;
				_003Ci_003E5__2 = 0;
				goto IL_0240;
				IL_0240:
				if (_003Ci_003E5__2 < 8 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					versionShowerFx._textBuilder.Clear();
					for (int i = 0; i < _003Ci_003E5__2; i++)
					{
						versionShowerFx._textBuilder.Append("█");
					}
					versionShowerFx._textBuilder.Append("▓▓▓ BLOOD ▓▓▓");
					versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
					_003C_003E2__current = sWait1;
					_003C_003E1__state = 5;
					return true;
				}
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15s;
				_003C_003E1__state = 6;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_SpringtrapApproach_003Ed__138 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		private int _003Cs_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_SpringtrapApproach_003Ed__138(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_018f;
			case 1:
				_003C_003E1__state = -1;
				_003Cs_003E5__3++;
				goto IL_0094;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.Jitter(0.1f + (float)_003Ci_003E5__2 * 0.15f, 1f + (float)_003Ci_003E5__2, 2f + (float)_003Ci_003E5__2);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_018f;
			case 4:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 1.8f, versionShowerFx._baseScale * 1.8f, 1f);
				versionShowerFx.SetText("HE'S HERE", isGlitching: true);
				_003C_003E2__current = versionShowerFx.Jitter(0.5f, 6f, 10f);
				_003C_003E1__state = 5;
				return true;
			case 5:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_018f:
				if (_003Ci_003E5__2 < sSpringtrapDistances.Length && (Object)(object)versionShowerFx._text != (Object)null)
				{
					_003Cs_003E5__3 = 0;
					goto IL_0094;
				}
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 4;
				return true;
				IL_0094:
				if (_003Cs_003E5__3 < 2)
				{
					((Graphic)versionShowerFx._text).color = StaticGray;
					versionShowerFx.SetText(versionShowerFx.RandomNoise(15), isGlitching: true);
					_003C_003E2__current = sWait025;
					_003C_003E1__state = 1;
					return true;
				}
				((Graphic)versionShowerFx._text).color = Color.Lerp(OxidizedGreen, DeadRed, (float)_003Ci_003E5__2 / (float)sSpringtrapDistances.Length);
				versionShowerFx.SetText(sSpringtrapDistances[_003Ci_003E5__2], isGlitching: true);
				_003C_003E2__current = _003Ci_003E5__2 switch
				{
					0 => sWait20s, 
					1 => sWait20s, 
					2 => sWait15s, 
					3 => sWait10, 
					_ => sWait10, 
				};
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_SpringtrapStare_003Ed__142 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private float _003Cscale_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_SpringtrapStare_003Ed__142(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = Color.white;
				versionShowerFx.SetText("◉   ◉", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003Cscale_003E5__2 = 1f;
				goto IL_00f8;
			case 3:
				_003C_003E1__state = -1;
				_003Cscale_003E5__2 += 0.1f;
				goto IL_00f8;
			case 4:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00f8:
				if (_003Cscale_003E5__2 < 2f)
				{
					((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * _003Cscale_003E5__2, versionShowerFx._baseScale * _003Cscale_003E5__2, 1f);
					_003C_003E2__current = sWait05;
					_003C_003E1__state = 3;
					return true;
				}
				((Graphic)versionShowerFx._text).color = DeadRed;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale, versionShowerFx._baseScale, 1f);
				versionShowerFx.SetText("I SEE YOU", isGlitching: true);
				_003C_003E2__current = versionShowerFx.Jitter(0.5f, 5f, 10f);
				_003C_003E1__state = 4;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_SystemReboot_003Ed__101 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_SystemReboot_003Ed__101(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.SystemWarning("SYSTEM FAILURE", 1.5f, DeadRed);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.TypewriterText("REBOOTING...", 0.1f);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				versionShowerFx.SetText(versionShowerFx.CorruptText("............", 12), isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 5;
				return true;
			case 5:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_TheStare_003Ed__96 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_TheStare_003Ed__96(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.SystemWarning("DON'T BLINK", 1f, Color.white);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("O_O", isGlitching: true);
				_003C_003E2__current = sWait5;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.Jitter(0.5f, 6f, 5f);
				_003C_003E1__state = 5;
				return true;
			case 5:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.ImpostorFlash(Color.black);
				_003C_003E1__state = 6;
				return true;
			case 6:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSequence_VentilationCascade_003Ed__165 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string _003CphantomFace_003E5__2;

		private int _003Ci_003E5__3;

		private int _003Cs_003E5__4;

		private string[] _003C_003E7__wrap4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSequence_VentilationCascade_003Ed__165(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003CphantomFace_003E5__2 = null;
			_003C_003E7__wrap4 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0388: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02db: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = AlertOrange;
				((TMP_Text)versionShowerFx._text).outlineColor = Color32.op_Implicit(AlertOrange * 0.5f);
				versionShowerFx.SetText("VENT: WARNING", isGlitching: true);
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__3 = 0;
				goto IL_0192;
			case 2:
				_003C_003E1__state = -1;
				_003Cs_003E5__4 = 0;
				goto IL_0179;
			case 3:
				_003C_003E1__state = -1;
				_003Cs_003E5__4++;
				goto IL_0179;
			case 4:
				_003C_003E1__state = -1;
				_003CphantomFace_003E5__2 = sGrotesqueFaces[sRandom.Next(sGrotesqueFaces.Length)];
				_003Ci_003E5__3 = 0;
				goto IL_02bb;
			case 5:
				_003C_003E1__state = -1;
				_003Ci_003E5__3++;
				goto IL_02bb;
			case 6:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 7;
				return true;
			case 7:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = FnafGreen;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale, versionShowerFx._baseScale, 1f);
				_003C_003E7__wrap4 = sRebootLines;
				_003Ci_003E5__3 = 0;
				break;
			case 8:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait3;
				_003C_003E1__state = 9;
				return true;
			case 9:
				{
					_003C_003E1__state = -1;
					_003Ci_003E5__3++;
					break;
				}
				IL_02bb:
				if (_003Ci_003E5__3 < 6 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					if (_003Ci_003E5__3 % 2 == 0)
					{
						((Graphic)versionShowerFx._text).color = OxidizedGreen;
						((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * (1.5f + (float)_003Ci_003E5__3 * 0.15f), versionShowerFx._baseScale * (1.5f + (float)_003Ci_003E5__3 * 0.15f), 1f);
						versionShowerFx.SetText(_003CphantomFace_003E5__2, isGlitching: true);
					}
					else
					{
						versionShowerFx.SetText("", isGlitching: true);
					}
					_003C_003E2__current = sWait08;
					_003C_003E1__state = 5;
					return true;
				}
				((Graphic)versionShowerFx._text).color = DeadRed;
				((Transform)versionShowerFx._textRectTransform).localScale = new Vector3(versionShowerFx._baseScale * 2.5f, versionShowerFx._baseScale * 2.5f, 1f);
				versionShowerFx.SetText(sGrotesqueFaces[sRandom.Next(sGrotesqueFaces.Length)], isGlitching: true);
				_003C_003E2__current = versionShowerFx.Jitter(0.4f, 8f, 15f);
				_003C_003E1__state = 6;
				return true;
				IL_0179:
				if (_003Cs_003E5__4 < 3)
				{
					versionShowerFx.SetText(versionShowerFx.RandomNoise(18), isGlitching: true);
					_003C_003E2__current = sWait025;
					_003C_003E1__state = 3;
					return true;
				}
				_003Ci_003E5__3++;
				goto IL_0192;
				IL_0192:
				if (_003Ci_003E5__3 < sCascadeSystems.Length && (Object)(object)versionShowerFx._text != (Object)null)
				{
					((Graphic)versionShowerFx._text).color = Color.Lerp(AlertOrange, DeadRed, (float)_003Ci_003E5__3 / (float)sCascadeSystems.Length);
					versionShowerFx._textBuilder.Clear().Append(sCascadeSystems[_003Ci_003E5__3]).Append(": ")
						.Append(sCascadeStatuses[_003Ci_003E5__3]);
					versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
					_003C_003E2__current = sWait10;
					_003C_003E1__state = 2;
					return true;
				}
				versionShowerFx.SetText("", isGlitching: true);
				((Graphic)versionShowerFx._text).color = Color.black;
				_003C_003E2__current = sWait10;
				_003C_003E1__state = 4;
				return true;
			}
			if (_003Ci_003E5__3 < _003C_003E7__wrap4.Length)
			{
				string text = _003C_003E7__wrap4[_003Ci_003E5__3];
				if ((Object)(object)versionShowerFx._text == (Object)null)
				{
					return false;
				}
				_003C_003E2__current = versionShowerFx.TypewriterText(text, 0.04f);
				_003C_003E1__state = 8;
				return true;
			}
			_003C_003E7__wrap4 = null;
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSlowCorruption_003Ed__149 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private string _003Ctext_003E5__2;

		private int _003Clen_003E5__3;

		private int _003Ccorruption_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSlowCorruption_003Ed__149(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Ctext_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ctext_003E5__2 = versionShowerFx._modText;
				_003Clen_003E5__3 = _003Ctext_003E5__2.Length;
				_003Ccorruption_003E5__4 = 0;
				goto IL_00c1;
			case 1:
				_003C_003E1__state = -1;
				_003Ccorruption_003E5__4++;
				goto IL_00c1;
			case 2:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("CORRUPTED", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 3;
				return true;
			case 3:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00c1:
				if (_003Ccorruption_003E5__4 <= _003Clen_003E5__3 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					((Graphic)versionShowerFx._text).color = Color.Lerp(versionShowerFx._baseColor, DeadRed, (float)_003Ccorruption_003E5__4 / (float)_003Clen_003E5__3);
					versionShowerFx.SetText(versionShowerFx.CorruptText(_003Ctext_003E5__2, _003Ccorruption_003E5__4), isGlitching: true);
					_003C_003E2__current = sWait1;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSoulCapture_003Ed__156 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSoulCapture_003Ed__156(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = GhostCyan;
				versionShowerFx.SetText("✧ SOUL ✧", isGlitching: true);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_00e9;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_00e9;
			case 3:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("GIVE LIFE", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 4;
				return true;
			case 4:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00e9:
				if (_003Ci_003E5__2 < sSpiralFrames.Length)
				{
					if ((Object)(object)versionShowerFx._text == (Object)null)
					{
						return false;
					}
					((Graphic)versionShowerFx._text).color = Color.Lerp(GhostCyan, RottenPurple, (float)_003Ci_003E5__2 / (float)sSpiralFrames.Length);
					versionShowerFx.SetText(sSpiralFrames[_003Ci_003E5__2], isGlitching: true);
					_003C_003E2__current = sWait08;
					_003C_003E1__state = 2;
					return true;
				}
				((Graphic)versionShowerFx._text).color = RottenPurple;
				versionShowerFx.SetText("CAPTURED", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSpringtrapWarning_003Ed__136 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSpringtrapWarning_003Ed__136(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.TypewriterText("HE'S HERE", 0.1f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_00cf;
			case 3:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_00cf;
			case 4:
				_003C_003E1__state = -1;
				versionShowerFx.SetText("", isGlitching: true);
				_003C_003E2__current = sWait1;
				_003C_003E1__state = 5;
				return true;
			case 5:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("TOO LATE", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 6;
				return true;
			case 6:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00cf:
				if (_003Ci_003E5__2 < 3)
				{
					((Graphic)versionShowerFx._text).color = ((_003Ci_003E5__2 % 2 == 0) ? DeadRed : Color.black);
					_003C_003E2__current = sWait08;
					_003C_003E1__state = 3;
					return true;
				}
				_003C_003E2__current = versionShowerFx.Jitter(0.5f, 4f, 8f);
				_003C_003E1__state = 4;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CStaticWall_003Ed__133 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CStaticWall_003Ed__133(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				versionShowerFx._textBuilder.Clear();
				for (int i = 0; i < 3; i++)
				{
					versionShowerFx._textBuilder.Append(versionShowerFx.RandomNoise(25)).Append("\n");
				}
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				((Graphic)versionShowerFx._text).color = Color.Lerp(Color.white, Color.gray, (float)sRandom.NextDouble());
				_003C_003E2__current = sWait025;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSystemWarning_003Ed__113 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		public Color color;

		public string message;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSystemWarning_003Ed__113(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				((Graphic)versionShowerFx._text).color = color;
				((TMP_Text)versionShowerFx._text).outlineColor = Color32.op_Implicit(color * 0.6f);
				versionShowerFx.SetText((sRandom.Next(0, 2) == 1) ? message : versionShowerFx.CorruptText(message, 3), isGlitching: true);
				_003C_003E2__current = sWait08;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CTerrifyingSequence_003Ed__106 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CTerrifyingSequence_003Ed__106(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				goto IL_0105;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.ChromaticAberration(0.35f);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				goto IL_0105;
			case 5:
				_003C_003E1__state = -1;
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 6;
				return true;
			case 6:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.Jitter(1f, 8f, 25f);
				_003C_003E1__state = 7;
				return true;
			case 7:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0105:
				if (_003Ci_003E5__2 < 3)
				{
					_003C_003E2__current = versionShowerFx.Jitter(0.4f + (float)_003Ci_003E5__2 * 0.1f, 3f + (float)_003Ci_003E5__2 * 1.5f, 6f + (float)_003Ci_003E5__2 * 3f);
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E2__current = versionShowerFx.PhantomAppearance("IT'S HERE", DeadRed, 0.6f);
				_003C_003E1__state = 5;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CTextCorruption_003Ed__108 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		public string baseText;

		public int intensity;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CTextCorruption_003Ed__108(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				versionShowerFx.SetText(versionShowerFx.CorruptText(baseText, intensity), isGlitching: true);
				_003C_003E2__current = sWait04;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CTrackingNoise_003Ed__114 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CTrackingNoise_003Ed__114(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				versionShowerFx._textBuilder.Clear().Append(versionShowerFx._modText).Append("\n")
					.Append(versionShowerFx.CorruptText("█ ░ █ ░ █", 6));
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003C_003E2__current = sWait025;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CTypewriterText_003Ed__116 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float delayPerChar;

		public VersionShowerFx _003C_003E4__this;

		public string text;

		private object _003Cwait_003E5__2;

		private int _003Ci_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CTypewriterText_003Ed__116(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Cwait_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				WaitForSeconds val = ((delayPerChar <= 0.02f) ? sWait025 : ((delayPerChar <= 0.04f) ? sWait04 : ((delayPerChar <= 0.06f) ? sWait05 : ((delayPerChar <= 0.09f) ? sWait08 : ((!(delayPerChar <= 0.12f)) ? sWait15 : sWait1)))));
				_003Cwait_003E5__2 = val;
				_003Ci_003E5__3 = 0;
				break;
			}
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__3++;
				break;
			}
			if (_003Ci_003E5__3 <= text.Length && (Object)(object)versionShowerFx._text != (Object)null)
			{
				versionShowerFx._textBuilder.Clear().Append(text, 0, _003Ci_003E5__3);
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003C_003E2__current = _003Cwait_003E5__2;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CUncannyText_003Ed__169 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		public float duration;

		private float _003Cratio_003E5__2;

		private string _003Ccolored_003E5__3;

		private float _003Celapsed_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CUncannyText_003Ed__169(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Ccolored_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cratio_003E5__2 = 0.1f + versionShowerFx._CorrLevel() * 0.6f;
				_003Ccolored_003E5__3 = $"<color=#{ColorUtility.ToHtmlStringRGB(FnafGreen)}>{versionShowerFx._modText}</color>";
				_003Celapsed_003E5__4 = 0f;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Celapsed_003E5__4 += 0.15f;
				break;
			}
			if (_003Celapsed_003E5__4 < duration)
			{
				versionShowerFx.SetText(versionShowerFx._CrCy(_003Ccolored_003E5__3, _003Cratio_003E5__2), isGlitching: true);
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 1;
				return true;
			}
			versionShowerFx.SetText(versionShowerFx._modText);
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CUncommonTierEvent_003Ed__92 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CUncommonTierEvent_003Ed__92(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				switch (sRandom.Next(0, 9))
				{
				case 0:
					_003C_003E2__current = versionShowerFx.ChromaticAberration(0.5f);
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E2__current = versionShowerFx.VerticalRoll(0.3f, 40f, 3f);
					_003C_003E1__state = 2;
					return true;
				case 2:
					_003C_003E2__current = versionShowerFx.NoiseRain(0.6f);
					_003C_003E1__state = 3;
					return true;
				case 3:
					_003C_003E2__current = versionShowerFx.SystemWarning("RUN AWAY", 0.7f, DeadRed);
					_003C_003E1__state = 4;
					return true;
				case 4:
					_003C_003E2__current = versionShowerFx.Wobble(0.5f, 15f, 3f);
					_003C_003E1__state = 5;
					return true;
				case 5:
					_003C_003E2__current = versionShowerFx.CameraLabelFlash(0.6f);
					_003C_003E1__state = 6;
					return true;
				case 6:
					_003C_003E2__current = versionShowerFx.MotionPing(0.7f);
					_003C_003E1__state = 7;
					return true;
				case 7:
					_003C_003E2__current = versionShowerFx.NoiseHalo(0.5f);
					_003C_003E1__state = 8;
					return true;
				case 8:
					_003C_003E2__current = versionShowerFx.UncannyText(0.8f);
					_003C_003E1__state = 9;
					return true;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
				_003C_003E1__state = -1;
				break;
			case 3:
				_003C_003E1__state = -1;
				break;
			case 4:
				_003C_003E1__state = -1;
				break;
			case 5:
				_003C_003E1__state = -1;
				break;
			case 6:
				_003C_003E1__state = -1;
				break;
			case 7:
				_003C_003E1__state = -1;
				break;
			case 8:
				_003C_003E1__state = -1;
				break;
			case 9:
				_003C_003E1__state = -1;
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CVHSStaticBurst_003Ed__118 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CVHSStaticBurst_003Ed__118(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				versionShowerFx.SetText(versionShowerFx._modText, isGlitching: true);
				_003C_003E2__current = sWait025;
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				versionShowerFx._textBuilder.Clear().Append(versionShowerFx._modText).Append("\n")
					.Append('█', sRandom.Next(8, 16));
				versionShowerFx.SetText(versionShowerFx._textBuilder.ToString(), isGlitching: true);
				_003C_003E2__current = sWait025;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CVentilationError_003Ed__132 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ccount_003E5__2;

		private int _003Ci_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CVentilationError_003Ed__132(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = AlertOrange;
				_003Ccount_003E5__2 = Math.Min(5, sVentilationMessages.Length);
				_003Ci_003E5__3 = 0;
				goto IL_00cd;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = versionShowerFx.Jitter(0.15f, 2f, 3f);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__3++;
				goto IL_00cd;
			case 3:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00cd:
				if (_003Ci_003E5__3 < _003Ccount_003E5__2)
				{
					versionShowerFx.SetText(sVentilationMessages[sRandom.Next(sVentilationMessages.Length)], isGlitching: true);
					_003C_003E2__current = sWait2;
					_003C_003E1__state = 1;
					return true;
				}
				((Graphic)versionShowerFx._text).color = DeadRed;
				versionShowerFx.SetText("VENT OPEN", isGlitching: true);
				_003C_003E2__current = sWait3;
				_003C_003E1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CVerticalRoll_003Ed__111 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		public float frequency;

		public float amplitude;

		private float _003CendTime_003E5__2;

		private RectTransform _003Crt_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CVerticalRoll_003Ed__111(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Crt_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				_003Crt_003E5__3 = versionShowerFx._textRectTransform;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				float num2 = Mathf.Sin(Time.time * frequency) * amplitude;
				_003Crt_003E5__3.anchoredPosition = new Vector2(versionShowerFx._baseAnchoredPosition.x, versionShowerFx._baseAnchoredPosition.y + num2);
				_003C_003E2__current = sWaitFrame;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CVictimsCrying_003Ed__163 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VersionShowerFx _003C_003E4__this;

		private int _003Ci_003E5__2;

		private string _003Cvictim_003E5__3;

		private int _003Cshake_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CVictimsCrying_003Ed__163(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Cvictim_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				((Graphic)versionShowerFx._text).color = GhostCyan;
				_003Ci_003E5__2 = 0;
				goto IL_0135;
			case 1:
				_003C_003E1__state = -1;
				_003Cshake_003E5__4++;
				goto IL_00f7;
			case 2:
				_003C_003E1__state = -1;
				_003Cvictim_003E5__3 = null;
				_003Ci_003E5__2++;
				goto IL_0135;
			case 3:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0135:
				if (_003Ci_003E5__2 < 5 && (Object)(object)versionShowerFx._text != (Object)null)
				{
					_003Cvictim_003E5__3 = sVictimMessages[sRandom.Next(sVictimMessages.Length)];
					_003Cshake_003E5__4 = 0;
					goto IL_00f7;
				}
				((Graphic)versionShowerFx._text).color = BloodDark;
				versionShowerFx.SetText("WE ARE STILL HERE", isGlitching: true);
				_003C_003E2__current = sWait2;
				_003C_003E1__state = 3;
				return true;
				IL_00f7:
				if (_003Cshake_003E5__4 < 4)
				{
					float num2 = Mathf.Sin((float)_003Cshake_003E5__4 * 2.5f) * 4f;
					versionShowerFx._textRectTransform.anchoredPosition = new Vector2(versionShowerFx._baseAnchoredPosition.x, versionShowerFx._baseAnchoredPosition.y + num2);
					versionShowerFx.SetText(versionShowerFx.CorruptText(_003Cvictim_003E5__3, _003Cshake_003E5__4), isGlitching: true);
					_003C_003E2__current = sWait08;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E2__current = sWait15;
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CWobble_003Ed__127 : IEnumerator<object>, IEnumerator, System.IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public VersionShowerFx _003C_003E4__this;

		public float frequency;

		public float amplitude;

		private float _003CendTime_003E5__2;

		private RectTransform _003Crt_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CWobble_003Ed__127(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void System.IDisposable.Dispose()
		{
			_003Crt_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			VersionShowerFx versionShowerFx = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				_003Crt_003E5__3 = versionShowerFx._textRectTransform;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2 && (Object)(object)versionShowerFx._text != (Object)null)
			{
				float num2 = Time.time * frequency;
				float num3 = Mathf.Sin(num2) * amplitude;
				float num4 = Mathf.Cos(num2 * 0.5f) * amplitude * 0.5f;
				_003Crt_003E5__3.anchoredPosition = new Vector2(versionShowerFx._baseAnchoredPosition.x + num3, versionShowerFx._baseAnchoredPosition.y + num4);
				_003C_003E2__current = sWaitFrame;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private TextMeshPro _text;

	private RectTransform _textRectTransform;

	private string _baseText;

	private string _modText;

	private readonly StringBuilder _textBuilder = new StringBuilder(256);

	private bool _isEffectRunning;

	private bool _isGlitchActive;

	private Coroutine _schedulerRoutine;

	private Coroutine _breathingRoutine;

	private float _sessionStartTime;

	private Vector2 _baseAnchoredPosition;

	private float _baseScale;

	private Color _baseColor;

	private Color _baseOutlineColor;

	private float _baseOutlineWidth;

	private string _baseColorHex;

	private static readonly WaitForSeconds sWaitFrame = new WaitForSeconds(0.016f);

	private static readonly WaitForSeconds sWait025 = new WaitForSeconds(0.025f);

	private static readonly WaitForSeconds sWait04 = new WaitForSeconds(0.04f);

	private static readonly WaitForSeconds sWait05 = new WaitForSeconds(0.05f);

	private static readonly WaitForSeconds sWait08 = new WaitForSeconds(0.08f);

	private static readonly WaitForSeconds sWait1 = new WaitForSeconds(0.1f);

	private static readonly WaitForSeconds sWait15 = new WaitForSeconds(0.15f);

	private static readonly WaitForSeconds sWait2 = new WaitForSeconds(0.2f);

	private static readonly WaitForSeconds sWait3 = new WaitForSeconds(0.3f);

	private static readonly WaitForSeconds sWait4 = new WaitForSeconds(0.4f);

	private static readonly WaitForSeconds sWait5 = new WaitForSeconds(0.5f);

	private static readonly WaitForSeconds sWait6 = new WaitForSeconds(0.6f);

	private static readonly WaitForSeconds sWait07 = new WaitForSeconds(0.7f);

	private static readonly WaitForSeconds sWait10 = new WaitForSeconds(1f);

	private static readonly WaitForSeconds sWait15s = new WaitForSeconds(1.5f);

	private static readonly WaitForSeconds sWait20s = new WaitForSeconds(2f);

	private static float sProbPhantom = 0.06f;

	private static float sProbMythic = 0.14f;

	private static float sProbRare = 0.32f;

	private float _heavyCooldownSeconds = 6f;

	private float _nextHeavyAllowedTime;

	private static readonly Color FnafGreen = new Color(0.6f, 1f, 0.3f);

	private static readonly Color AlertOrange = new Color(1f, 0.4f, 0f);

	private static readonly Color DeadRed = new Color(1f, 0f, 0f);

	private static readonly Color BloodDark = new Color(0.5f, 0f, 0f);

	private static readonly Color GhostCyan = new Color(0.4f, 1f, 1f, 0.8f);

	private static readonly Color CursedPurple = new Color(0.4f, 0f, 0.6f);

	private static readonly Color GoldenYellow = new Color(1f, 0.85f, 0f);

	private static readonly Color CorpseGreen = new Color(0.3f, 0.5f, 0.2f);

	private static readonly Color RottenPurple = new Color(0.35f, 0.1f, 0.4f);

	private static readonly Color FleshPink = new Color(0.9f, 0.5f, 0.5f);

	private static readonly Color BoneWhite = new Color(0.95f, 0.92f, 0.85f);

	private static readonly Color OxidizedGreen = new Color(0.2f, 0.4f, 0.3f);

	private static readonly Color VisceralRed = new Color(0.7f, 0.1f, 0.15f);

	private static readonly Color DecayBrown = new Color(0.4f, 0.25f, 0.1f);

	private static readonly Color StaticGray = new Color(0.6f, 0.6f, 0.6f);

	private static readonly Random sRandom = new Random();

	private static readonly string[] sSystemMessages = new string[56]
	{
		"SUS", "[REDACTED]", "ACCESS DENIED", "SECURITY ALERT", "UNKNOWN SIGNAL", "WHO IS IT?", "NOT THE IMPOSTOR", "VENT ERROR", "AUDIO ERROR", "VIDEO ERROR",
		"CAM SYS ERROR", "REBOOT ALL", "BODY FOUND", "INTRUDER ALERT", "LOCKDOWN", "RUN", "HIDE", "THEY ARE HERE", "DON'T LOOK BACK", "I SEE YOU",
		"YOU ARE NEXT", "IT'S TOO LATE", "NO ESCAPE", "KILL", "DEAD", "HELP ME", "BEHIND YOU", "ERROR 666", "WATCHING...", "DO NOT MOVE",
		"IMPOSTOR WIN", "GAME OVER", "BLOOD", "DARKNESS", "IT'S ME", "SAVE THEM", "SAVE HIM", "HELP THEM", "YOU CAN'T", "FOLLOW ME",
		"HE ALWAYS COMES BACK", "I WILL PUT YOU BACK TOGETHER", "I'M STILL HERE", "MY NAME IS SPRINGTRAP", "THE JOY OF CREATION", "WAS THAT THE BITE OF '87?", "FIVE NIGHTS", "NIGHTMARE", "4", "BAD ENDING",
		"YOU DIED", "GAME OVER", "VENT SEALED", "AUDIO LURE FAILED", "CAMERA DISABLED", "SYSTEM OVERLOAD"
	};

	private static readonly string[] sVentilationMessages = new string[12]
	{
		"VENT. ERROR", "SEAL VENT", "AUDIO ERROR", "CAM ERROR", "REBOOT", "VENT 1: SEALED", "VENT 2: ERROR", "VENT 3: OPEN", "VENT 4: ???", "AIR FLOW: CRITICAL",
		"O2 LEVELS: LOW", "PRESSURE: DROPPING"
	};

	private static readonly string[] sSpringtrapLore = new string[20]
	{
		"I FOUND YOU", "THERE IS NO ESCAPE", "I AM STILL HERE", "THE SUIT IS MINE", "JOIN US", "THEY LOCKED ME AWAY", "30 YEARS", "THE SAFE ROOM", "I PUT THEM BACK TOGETHER", "DID YOU MISS ME?",
		"FAZBEAR'S FRIGHT", "ONE MORE SOUL", "I ALWAYS COME BACK", "THE SPRING LOCKS", "MY NAME IS SPRINGTRAP", "PURPLE GUY", "WILLIAM AFTON", "THE MURDERER", "FATHER?", "I'M GOING TO COME FIND YOU"
	};

	private static readonly string[] sVictimMessages = new string[22]
	{
		"HELP US", "SET US FREE", "IT HURTS", "WHY WON'T YOU SAVE US?", "WE ARE STILL HERE", "GIVE GIFTS", "GIVE LIFE", "SAVE ME", "HE KILLED US", "WE CAN'T LEAVE",
		"THE PUPPET SAVED US", "FIVE CHILDREN", "MISSING CHILDREN INCIDENT", "WE REMEMBER", "GABRIEL", "JEREMY", "SUSIE", "FRITZ", "CASSIDY", "THE BITE VICTIM",
		"PUT US BACK TOGETHER", "WE WANT TO BE FREE"
	};

	private static readonly string[] sGrotesqueMessages = new string[19]
	{
		"FLESH", "BONES", "BLOOD", "DECAY", "ROT", "CORPSE", "THE SMELL", "TRAPPED INSIDE", "CRUSHED", "SPRING LOCK FAILURE",
		"HEAR THE SCREAMING", "FEEL THE METAL", "PIERCING FLESH", "ORGANS EXPOSED", "MUMMIFIED", "30 YEARS IN DARKNESS", "THE WIRES TANGLE", "ENDOSKELETON EXPOSED", "ROTTING SUIT"
	};

	private static readonly string[] sCreepyFaces = new string[30]
	{
		"O_O", "◉_◉", "⊙_⊙", "●_●", "○_○", "◎_◎", "X_X", "✖_✖", "☠", "\ud83d\udc41",
		"◉\u203f◉", "⊙\u203f⊙", "( \u0361° \u035cʖ \u0361°)", "ಠ_ಠ", "ʘ\u203fʘ", "◕\u203f◕", "⊙\ufe4f⊙", "⊙ω⊙", "ఠ_ఠ", "(╬ Ò\ufe4fÓ)",
		"ლ(ಠ益ಠლ)", "(ಠ\u203fಠ)", "◉◡◉", "⊙▃⊙", "●▂●", "☉_☉", "(⊙_⊙)", "٩(\u0361๏\u032f\u0361๏)۶", "(\u00b4°\u0325\u0325\u0325ω°\u0325\u0325\u0325`)", "q(❂\u203f❂)p"
	};

	private static readonly string[] sGrotesqueFaces = new string[31]
	{
		"▓▓▓☠▓▓▓", "◉ ▄▄ ◉", "● ▀▀ ●", "⊙ ▓▓ ⊙", "◉     ◉", "●  ●", "○   ○", "☉    ☉", "◉╭▓▓╮◉", "●╭██╮●",
		"⊙╭░░╮⊙", "◉\u0334_\u0334◉\u0334", "●\u0337_\u0337●\u0337", "⊙\u0338_\u0338⊙\u0338", "◉╭╮◉", "●Д●", "⊙Д⊙", "◎Д◎", "Ⓧ_Ⓧ", "✞_✞",
		"☢_☢", "☣_☣", "◉◉◉◉◉", "●●●●●", "⊙⊙⊙⊙⊙", "[◉_◉]", "[●_●]", "<◉_◉>", "○   ○", "      ",
		"..."
	};

	private static readonly string[] sAudioPatterns = new string[10] { "▓░▓░▓ KRRRshhh ▓░▓░▓", "~~~STATIC~~~NOISE~~~", "...bzzt...crackle...", "♪♫...............♫♪", ">>>AUDIO LURE<<<", "...PLAYING SOUND...", "◄◄ █▓▒░ ►►", "≋≋≋ SIGNAL ≋≋≋", "### ERROR ###", "▓▓▓ NO SIGNAL ▓▓▓" };

	private static readonly char[] sNoisePool = new char[50]
	{
		'░', '▒', '▓', '█', '▚', '▞', '▙', '▟', '_', '#',
		'/', '!', '?', 'Ø', '¤', '◊', '†', '‡', '☠', '☢',
		'☣', '⚡', '⚠', '⚔', '⚖', '§', '¶', '▄', '▀', '■',
		'□', '▪', '▫', '⛧', '⌬', '⌭', '⌖', '⌘', '⏚', '⏧',
		'⌽', '⛒', '⛓', '⛤', '␡', '␉', '⌻', '⌺', '⏥', '⌗'
	};

	private static readonly Dictionary<char, char> _crMap = new Dictionary<char, char>
	{
		{ 'A', 'А' },
		{ 'B', 'В' },
		{ 'E', 'Е' },
		{ 'K', 'К' },
		{ 'M', 'М' },
		{ 'H', 'Н' },
		{ 'O', 'О' },
		{ 'P', 'Р' },
		{ 'C', 'С' },
		{ 'T', 'Т' },
		{ 'X', 'Х' },
		{ 'Y', 'У' }
	};

	private static readonly string _flSrc = "abcdefghijklmnopqrstuvwxyz";

	private static readonly string _flDst = "ɐqɔpǝɟƃɥıɾʞןɯuodbɹsʇnʌʍxʎz";

	private static readonly string[] sHorrorFaces = new string[12]
	{
		"[◉\u20e0 \u20dd ◉\u20e0]", "\u02d9·٠•●ˏˋ●\u02d9", "[ \u20e0X\u20e0 _ \u20e0X\u20e0 ]", "▷━ ◉ \u0338 ◉ ━◁", "( • \u032a\u0300 ⌖ \u032a\u0301 • )", "[\u0333 \u0332◡\u0332\u0305 \u0332\u0305◡\u0332\u0305 \u0332\u0305]", "\u02d9 ʢ ⌭ ʡ \u02d9", "ʢ░◉░ʡ", "⌖ \u0332 ⌖ \u0332 ⌖", "◣◢ ⛒ ◣◢",
		"⏚_⏚_⏚", "( ⌬ \u203f ⌬ )"
	};

	private static readonly string[] sRevealFrames = new string[11]
	{
		"  ◉_◉  ", " SPRING ", " BONNIE ", "   ...  ", "  /◉_◉\\  ", "REMOVING", "  HEAD  ", "   ...  ", "  ☠ ☠   ", " AFTON  ",
		" INSIDE "
	};

	private static readonly string[] sBinaryMsgs = new string[3] { "01001000 01000101 01001100 01010000", "01000100 01000101 01000001 01000100", "01010010 01010101 01001110" };

	private static readonly string[] sAudioGlitches = new string[5] { "▓▓▓AUDIO▓▓▓", "█ERROR█", "###SND###", "~~~STATIC~~~", ">>>NOISE<<<" };

	private static readonly string[] sSpringtrapDistances = new string[5] { "CAM 10", "CAM 09", "CAM 08", "HALLWAY", "DOOR" };

	private static readonly string[] sEyePatterns = new string[4] { "◉          ◉", "◉ ◉      ◉ ◉", "◉ ◉ ◉  ◉ ◉ ◉", "◉◉◉◉◉◉◉◉◉◉◉" };

	private static readonly string[] sDecompositionStages = new string[6] { "YEAR 1: FRESH", "YEAR 5: DECAY", "YEAR 10: BONES", "YEAR 20: DUST", "YEAR 30: FUSED", "NOW: ETERNAL" };

	private static readonly string[] sSpiralFrames = new string[14]
	{
		"     ✧     ", "    ✧ ✧    ", "   ✧   ✧   ", "  ✧     ✧  ", " ✧       ✧ ", "✧         ✧", " ★       ★ ", "  ★     ★  ", "   ★   ★   ", "    ★ ★    ",
		"     ★     ", "     ▓     ", "     █     ", "           "
	};

	private static readonly string[] sMangleFrames = new string[7] { "░░░░ M ░░░░", "░░░ MA ░░░", "░░ MAN ░░", "░ MANG ░", "MANGLE", "   MANGLE   ", " ▀▀MANGLE▀▀ " };

	private static readonly string[] sCorpseArt = new string[6] { "     ☠     ", "   / ☠ \\   ", "  ─/ ☠ \\─  ", " ─┼/ ☠ \\┼─ ", "─┼┼/ ☠ \\┼┼─", "CORPSE FOUND" };

	private static readonly string[] sWhispers = new string[4] { "...", "...do you...", "...hear me...", "...I'm here..." };

	private static readonly string[] sCascadeSystems = new string[3] { "VENTILATION", "CAM SYSTEM", "AUDIO DEVICE" };

	private static readonly string[] sCascadeStatuses = new string[3] { "OFFLINE", "ERROR", "MALFUNCTION" };

	private static readonly string[] sRebootLines = new string[4] { "REBOOTING...", "VENT: ONLINE", "CAM: ONLINE", "AUDIO: ONLINE" };

	private static readonly string[] sWalkFrames = new string[12]
	{
		"                    ◉", "                 ◉ ◉", "              ◉_◉", "           ◉_◉", "        ◉_◉", "     ◉_◉", "  ◉_◉", "◉_◉", "◉_◉  ", " ◉_◉    ",
		"  ◉_◉      ", "   ◉_◉        "
	};

	private static readonly char[] sFireChars = new char[4] { '▓', '░', '█', '▒' };

	private static readonly string[] sSafeRoomChildren = new string[5] { "GABRIEL", "JEREMY", "SUSIE", "FRITZ", "CASSIDY" };

	private static readonly string[] sSpringLockSounds = new string[4] { "*click*", "*SNAP*", "*CRUNCH*", "..." };

	private static readonly string[] sFnaf3Authentic = new string[29]
	{
		"STAGE 01", "BALLOON BOY", "LEFT BEHIND", "HAPPIEST DAY", "PROPERTY OF AFTON ROBOTICS", "8-BIT NIGHTMARE", "CAM 01", "CAM 04", "CAM 07", "CAM 08",
		"CAM 15", "ROOM 1A", "ROOM 1B", "VENT 1", "VENT 2", "GIVE GIFTS", "GIVE LIFE", "TAKE CAKE TO THE CHILDREN", "THE END", "THE MARIONETTE",
		"THE PUPPET", "REBOOT VENTILATION", "REBOOT AUDIO", "REBOOT CAMERAS", "POWER OUT 0%", "ERROR 30:00:00", "FAZBEAR FRIGHTS: THE HORROR ATTRACTION", "ESTABLISHED 1983", "MISSING POSTERS"
	};

	private static readonly string[] sCamRooms = new string[28]
	{
		"CAM 01", "CAM 02", "CAM 03", "CAM 04", "CAM 05", "CAM 06", "CAM 07", "CAM 08", "CAM 09", "CAM 10",
		"VENT CAM", "ARCADE", "OFFICE", "HALLWAY", "PARTS/SERVICE", "Electrical", "MedBay", "Security", "Reactor", "O2",
		"Admin", "Navigation", "Cafeteria", "Storage", "THE VOID", "MORGUE", "SAFE ROOM", "????"
	};

	private static readonly Color[] _colorCycle = (Color[])(object)new Color[6]
	{
		DeadRed,
		BloodDark,
		Color.black,
		DeadRed,
		GhostCyan,
		Color.grey
	};

	private static readonly Color[] _flashColors = (Color[])(object)new Color[4]
	{
		DeadRed,
		GhostCyan,
		Color.white,
		BloodDark
	};

	public VersionShowerFx(IntPtr ptr)
		: base(ptr)
	{
	}

	[HideFromIl2Cpp]
	public void Initialize(TextMeshPro text)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_text != (Object)null)
		{
			return;
		}
		if ((Object)(object)text == (Object)null)
		{
			Debug.LogError(Object.op_Implicit("[VersionShowerFx] TextMeshPro is null!"));
			return;
		}
		_text = text;
		_textRectTransform = ((TMP_Text)text).rectTransform;
		_baseText = ((TMP_Text)text).text;
		_modText = "Mod Menu Crew 6.1.4b";
		_baseAnchoredPosition = _textRectTransform.anchoredPosition;
		_baseScale = ((Transform)_textRectTransform).localScale.x;
		_baseColor = ((Graphic)_text).color;
		_baseOutlineColor = Color32.op_Implicit(((TMP_Text)_text).outlineColor);
		_baseOutlineWidth = ((TMP_Text)_text).outlineWidth;
		_baseColorHex = ColorUtility.ToHtmlStringRGB(_baseColor);
		_PrewarmAtlas();
		if (!_isEffectRunning)
		{
			_isEffectRunning = true;
			_nextHeavyAllowedTime = Time.time + 3f;
			_sessionStartTime = Time.realtimeSinceStartup;
			if (_schedulerRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_schedulerRoutine);
			}
			if (_breathingRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_breathingRoutine);
			}
			_schedulerRoutine = ((MonoBehaviour)this).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(GlitchScheduler()));
			_breathingRoutine = ((MonoBehaviour)this).StartCoroutine(CollectionExtensions.WrapToIl2Cpp(IdleBreathing()));
			Debug.Log(Object.op_Implicit("[VersionShowerFx] Efeitos de terror inicializados!"));
		}
	}

	[IteratorStateMachine(typeof(_003CGlitchScheduler_003Ed__89))]
	[HideFromIl2Cpp]
	private IEnumerator GlitchScheduler()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGlitchScheduler_003Ed__89(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CIdleBreathing_003Ed__90))]
	[HideFromIl2Cpp]
	private IEnumerator IdleBreathing()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CIdleBreathing_003Ed__90(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CCommonTierEvent_003Ed__91))]
	[HideFromIl2Cpp]
	private IEnumerator CommonTierEvent()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCommonTierEvent_003Ed__91(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CUncommonTierEvent_003Ed__92))]
	[HideFromIl2Cpp]
	private IEnumerator UncommonTierEvent()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CUncommonTierEvent_003Ed__92(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CRareTierEvent_003Ed__93))]
	[HideFromIl2Cpp]
	private IEnumerator RareTierEvent()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRareTierEvent_003Ed__93(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CMythicTierEvent_003Ed__94))]
	[HideFromIl2Cpp]
	private IEnumerator MythicTierEvent()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CMythicTierEvent_003Ed__94(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CPhantomTierEvent_003Ed__95))]
	[HideFromIl2Cpp]
	private IEnumerator PhantomTierEvent()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CPhantomTierEvent_003Ed__95(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_TheStare_003Ed__96))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_TheStare()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_TheStare_003Ed__96(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CHeartbeatHorror_003Ed__97))]
	[HideFromIl2Cpp]
	private IEnumerator HeartbeatHorror()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CHeartbeatHorror_003Ed__97(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CBloodDripEffect_003Ed__98))]
	[HideFromIl2Cpp]
	private IEnumerator BloodDripEffect()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBloodDripEffect_003Ed__98(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_EmergencyMeeting_003Ed__99))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_EmergencyMeeting()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_EmergencyMeeting_003Ed__99(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_ImpostorReveal_003Ed__100))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_ImpostorReveal()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_ImpostorReveal_003Ed__100(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_SystemReboot_003Ed__101))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_SystemReboot()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_SystemReboot_003Ed__101(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_SabotageCritical_003Ed__102))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_SabotageCritical()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_SabotageCritical_003Ed__102(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_CriticalBreach_003Ed__103))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_CriticalBreach()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_CriticalBreach_003Ed__103(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CPhantomAppearance_003Ed__104))]
	[HideFromIl2Cpp]
	private IEnumerator PhantomAppearance(string name, Color color, float duration)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CPhantomAppearance_003Ed__104(0)
		{
			_003C_003E4__this = this,
			name = name,
			color = color,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CCrewmateColorCycle_003Ed__105))]
	[HideFromIl2Cpp]
	private IEnumerator CrewmateColorCycle()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCrewmateColorCycle_003Ed__105(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CTerrifyingSequence_003Ed__106))]
	[HideFromIl2Cpp]
	private IEnumerator TerrifyingSequence()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTerrifyingSequence_003Ed__106(0)
		{
			_003C_003E4__this = this
		};
	}

	[HideFromIl2Cpp]
	private void SetText(string content, bool isGlitching = false)
	{
		if (!((Object)(object)_text == (Object)null))
		{
			string value = (isGlitching ? content : _modText);
			_textBuilder.Clear().Append(_baseText).Append(" <color=#")
				.Append(_baseColorHex)
				.Append("><b><i>")
				.Append(value)
				.Append("</i></b></color>");
			((TMP_Text)_text).text = _textBuilder.ToString();
		}
	}

	[IteratorStateMachine(typeof(_003CTextCorruption_003Ed__108))]
	[HideFromIl2Cpp]
	private IEnumerator TextCorruption(float duration, int intensity, string baseText)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTextCorruption_003Ed__108(0)
		{
			_003C_003E4__this = this,
			duration = duration,
			intensity = intensity,
			baseText = baseText
		};
	}

	[IteratorStateMachine(typeof(_003CJitter_003Ed__109))]
	[HideFromIl2Cpp]
	private IEnumerator Jitter(float duration, float posRange, float rotRange)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CJitter_003Ed__109(0)
		{
			_003C_003E4__this = this,
			duration = duration,
			posRange = posRange,
			rotRange = rotRange
		};
	}

	[IteratorStateMachine(typeof(_003CChromaticAberration_003Ed__110))]
	[HideFromIl2Cpp]
	private IEnumerator ChromaticAberration(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CChromaticAberration_003Ed__110(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CVerticalRoll_003Ed__111))]
	[HideFromIl2Cpp]
	private IEnumerator VerticalRoll(float duration, float frequency, float amplitude)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CVerticalRoll_003Ed__111(0)
		{
			_003C_003E4__this = this,
			duration = duration,
			frequency = frequency,
			amplitude = amplitude
		};
	}

	[IteratorStateMachine(typeof(_003CImpostorFlash_003Ed__112))]
	[HideFromIl2Cpp]
	private IEnumerator ImpostorFlash(Color color)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CImpostorFlash_003Ed__112(0)
		{
			_003C_003E4__this = this,
			color = color
		};
	}

	[IteratorStateMachine(typeof(_003CSystemWarning_003Ed__113))]
	[HideFromIl2Cpp]
	private IEnumerator SystemWarning(string message, float duration, Color color)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSystemWarning_003Ed__113(0)
		{
			_003C_003E4__this = this,
			message = message,
			duration = duration,
			color = color
		};
	}

	[IteratorStateMachine(typeof(_003CTrackingNoise_003Ed__114))]
	[HideFromIl2Cpp]
	private IEnumerator TrackingNoise(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTrackingNoise_003Ed__114(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CColorDrain_003Ed__115))]
	[HideFromIl2Cpp]
	private IEnumerator ColorDrain(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CColorDrain_003Ed__115(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CTypewriterText_003Ed__116))]
	[HideFromIl2Cpp]
	private IEnumerator TypewriterText(string text, float delayPerChar)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTypewriterText_003Ed__116(0)
		{
			_003C_003E4__this = this,
			text = text,
			delayPerChar = delayPerChar
		};
	}

	[IteratorStateMachine(typeof(_003CCharacterSwapGlitch_003Ed__117))]
	[HideFromIl2Cpp]
	private IEnumerator CharacterSwapGlitch(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCharacterSwapGlitch_003Ed__117(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CVHSStaticBurst_003Ed__118))]
	[HideFromIl2Cpp]
	private IEnumerator VHSStaticBurst(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CVHSStaticBurst_003Ed__118(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CCRTCrosstalk_003Ed__119))]
	[HideFromIl2Cpp]
	private IEnumerator CRTCrosstalk(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCRTCrosstalk_003Ed__119(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CGreenPulse_003Ed__120))]
	[HideFromIl2Cpp]
	private IEnumerator GreenPulse(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGreenPulse_003Ed__120(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CNoiseScroll_003Ed__121))]
	[HideFromIl2Cpp]
	private IEnumerator NoiseScroll(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CNoiseScroll_003Ed__121(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CNoiseRain_003Ed__122))]
	[HideFromIl2Cpp]
	private IEnumerator NoiseRain(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CNoiseRain_003Ed__122(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CNoiseHalo_003Ed__123))]
	[HideFromIl2Cpp]
	private IEnumerator NoiseHalo(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CNoiseHalo_003Ed__123(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CNoiseFrame_003Ed__124))]
	[HideFromIl2Cpp]
	private IEnumerator NoiseFrame(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CNoiseFrame_003Ed__124(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CCameraLabelFlash_003Ed__125))]
	[HideFromIl2Cpp]
	private IEnumerator CameraLabelFlash(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCameraLabelFlash_003Ed__125(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CBurnInPulse_003Ed__126))]
	[HideFromIl2Cpp]
	private IEnumerator BurnInPulse(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBurnInPulse_003Ed__126(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CWobble_003Ed__127))]
	[HideFromIl2Cpp]
	private IEnumerator Wobble(float duration, float frequency, float amplitude)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWobble_003Ed__127(0)
		{
			_003C_003E4__this = this,
			duration = duration,
			frequency = frequency,
			amplitude = amplitude
		};
	}

	[IteratorStateMachine(typeof(_003CMotionPing_003Ed__128))]
	[HideFromIl2Cpp]
	private IEnumerator MotionPing(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CMotionPing_003Ed__128(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CPhantomSignal_003Ed__129))]
	[HideFromIl2Cpp]
	private IEnumerator PhantomSignal(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CPhantomSignal_003Ed__129(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CColorFlash_003Ed__130))]
	[HideFromIl2Cpp]
	private IEnumerator ColorFlash(Color color, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CColorFlash_003Ed__130(0)
		{
			_003C_003E4__this = this,
			color = color,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CIntensiveGlitchSequence_003Ed__131))]
	[HideFromIl2Cpp]
	private IEnumerator IntensiveGlitchSequence(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CIntensiveGlitchSequence_003Ed__131(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CVentilationError_003Ed__132))]
	[HideFromIl2Cpp]
	private IEnumerator VentilationError()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CVentilationError_003Ed__132(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CStaticWall_003Ed__133))]
	[HideFromIl2Cpp]
	private IEnumerator StaticWall(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStaticWall_003Ed__133(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CFaceFlash_003Ed__134))]
	[HideFromIl2Cpp]
	private IEnumerator FaceFlash()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFaceFlash_003Ed__134(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CAudioDistortion_003Ed__135))]
	[HideFromIl2Cpp]
	private IEnumerator AudioDistortion(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CAudioDistortion_003Ed__135(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CSpringtrapWarning_003Ed__136))]
	[HideFromIl2Cpp]
	private IEnumerator SpringtrapWarning()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSpringtrapWarning_003Ed__136(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CCameraDisabled_003Ed__137))]
	[HideFromIl2Cpp]
	private IEnumerator CameraDisabled()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCameraDisabled_003Ed__137(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_SpringtrapApproach_003Ed__138))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_SpringtrapApproach()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_SpringtrapApproach_003Ed__138(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_PhantomJumpscare_003Ed__139))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_PhantomJumpscare()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_PhantomJumpscare_003Ed__139(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_NightmareMode_003Ed__140))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_NightmareMode()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_NightmareMode_003Ed__140(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_BadEnding_003Ed__141))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_BadEnding()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_BadEnding_003Ed__141(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_SpringtrapStare_003Ed__142))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_SpringtrapStare()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_SpringtrapStare_003Ed__142(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_PurpleGuy_003Ed__143))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_PurpleGuy()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_PurpleGuy_003Ed__143(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CScanlineEffect_003Ed__144))]
	[HideFromIl2Cpp]
	private IEnumerator ScanlineEffect(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CScanlineEffect_003Ed__144(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CCountdownTerror_003Ed__145))]
	[HideFromIl2Cpp]
	private IEnumerator CountdownTerror()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCountdownTerror_003Ed__145(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CScrambledMessage_003Ed__146))]
	[HideFromIl2Cpp]
	private IEnumerator ScrambledMessage(string secret)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CScrambledMessage_003Ed__146(0)
		{
			_003C_003E4__this = this,
			secret = secret
		};
	}

	[IteratorStateMachine(typeof(_003CPowerOutage_003Ed__147))]
	[HideFromIl2Cpp]
	private IEnumerator PowerOutage()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CPowerOutage_003Ed__147(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CBinaryHorror_003Ed__148))]
	[HideFromIl2Cpp]
	private IEnumerator BinaryHorror()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBinaryHorror_003Ed__148(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSlowCorruption_003Ed__149))]
	[HideFromIl2Cpp]
	private IEnumerator SlowCorruption()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSlowCorruption_003Ed__149(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_6AM_003Ed__150))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_6AM()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_6AM_003Ed__150(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_GoldenFreddy_003Ed__151))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_GoldenFreddy()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_GoldenFreddy_003Ed__151(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_SpringLockFailure_003Ed__152))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_SpringLockFailure()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_SpringLockFailure_003Ed__152(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CFleshReveal_003Ed__153))]
	[HideFromIl2Cpp]
	private IEnumerator FleshReveal()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFleshReveal_003Ed__153(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CEyesInDarkness_003Ed__154))]
	[HideFromIl2Cpp]
	private IEnumerator EyesInDarkness()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEyesInDarkness_003Ed__154(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_AftonDecomposition_003Ed__155))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_AftonDecomposition()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_AftonDecomposition_003Ed__155(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSoulCapture_003Ed__156))]
	[HideFromIl2Cpp]
	private IEnumerator SoulCapture()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSoulCapture_003Ed__156(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CHallucination_003Ed__157))]
	[HideFromIl2Cpp]
	private IEnumerator Hallucination()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CHallucination_003Ed__157(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_PhantomMangle_003Ed__158))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_PhantomMangle()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_PhantomMangle_003Ed__158(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CCorpseReveal_003Ed__159))]
	[HideFromIl2Cpp]
	private IEnumerator CorpseReveal()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCorpseReveal_003Ed__159(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_NightmareFredbear_003Ed__160))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_NightmareFredbear()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_NightmareFredbear_003Ed__160(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_Dismemberment_003Ed__161))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_Dismemberment()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_Dismemberment_003Ed__161(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CRareScreen_SpringBonnie_003Ed__162))]
	[HideFromIl2Cpp]
	private IEnumerator RareScreen_SpringBonnie()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRareScreen_SpringBonnie_003Ed__162(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CVictimsCrying_003Ed__163))]
	[HideFromIl2Cpp]
	private IEnumerator VictimsCrying()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CVictimsCrying_003Ed__163(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CEndoskeletonExposed_003Ed__164))]
	[HideFromIl2Cpp]
	private IEnumerator EndoskeletonExposed()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEndoskeletonExposed_003Ed__164(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_VentilationCascade_003Ed__165))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_VentilationCascade()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_VentilationCascade_003Ed__165(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_PhantomFreddyWalkBy_003Ed__166))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_PhantomFreddyWalkBy()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_PhantomFreddyWalkBy_003Ed__166(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_FazbearsFright_003Ed__167))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_FazbearsFright()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_FazbearsFright_003Ed__167(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CSequence_SafeRoom_003Ed__168))]
	[HideFromIl2Cpp]
	private IEnumerator Sequence_SafeRoom()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSequence_SafeRoom_003Ed__168(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CUncannyText_003Ed__169))]
	[HideFromIl2Cpp]
	private IEnumerator UncannyText(float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CUncannyText_003Ed__169(0)
		{
			_003C_003E4__this = this,
			duration = duration
		};
	}

	[IteratorStateMachine(typeof(_003CHorrorFaceFlash_003Ed__170))]
	[HideFromIl2Cpp]
	private IEnumerator HorrorFaceFlash()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CHorrorFaceFlash_003Ed__170(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CInvertedReality_003Ed__171))]
	[HideFromIl2Cpp]
	private IEnumerator InvertedReality()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CInvertedReality_003Ed__171(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CDeepZalgo_003Ed__172))]
	[HideFromIl2Cpp]
	private IEnumerator DeepZalgo()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDeepZalgo_003Ed__172(0)
		{
			_003C_003E4__this = this
		};
	}

	[HideFromIl2Cpp]
	private string CorruptText(string input, int passes)
	{
		if (string.IsNullOrEmpty(input))
		{
			return "";
		}
		char[] array = input.ToCharArray();
		for (int i = 0; i < passes && i < array.Length; i++)
		{
			int num = sRandom.Next(0, array.Length);
			array[num] = sNoisePool[sRandom.Next(sNoisePool.Length)];
		}
		_textBuilder.Clear().Append(array);
		return _textBuilder.ToString();
	}

	[HideFromIl2Cpp]
	private string _CrCy(string s, float ratio)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		_textBuilder.Clear();
		bool flag = false;
		foreach (char c in s)
		{
			switch (c)
			{
			case '<':
				flag = true;
				_textBuilder.Append(c);
				continue;
			case '>':
				flag = false;
				_textBuilder.Append(c);
				continue;
			}
			if (!flag && (float)sRandom.NextDouble() < ratio && _crMap.TryGetValue(c, out var value))
			{
				_textBuilder.Append(value);
			}
			else
			{
				_textBuilder.Append(c);
			}
		}
		return _textBuilder.ToString();
	}

	[HideFromIl2Cpp]
	private string _Zg(string s, int intensity)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		if (intensity < 1)
		{
			intensity = 1;
		}
		if (intensity > 5)
		{
			intensity = 5;
		}
		_textBuilder.Clear();
		bool flag = false;
		foreach (char c in s)
		{
			_textBuilder.Append(c);
			switch (c)
			{
			case '<':
				flag = true;
				continue;
			case '>':
				flag = false;
				continue;
			}
			if (!flag)
			{
				for (int j = 0; j < intensity; j++)
				{
					_textBuilder.Append((char)(768 + sRandom.Next(0, 112)));
				}
			}
		}
		return _textBuilder.ToString();
	}

	[HideFromIl2Cpp]
	private string _Fl(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		_textBuilder.Clear();
		bool flag = false;
		foreach (char c in s)
		{
			switch (c)
			{
			case '<':
				flag = true;
				_textBuilder.Append(c);
				continue;
			case '>':
				flag = false;
				_textBuilder.Append(c);
				continue;
			}
			if (flag)
			{
				_textBuilder.Append(c);
				continue;
			}
			char value = char.ToLowerInvariant(c);
			int num = _flSrc.IndexOf(value);
			_textBuilder.Append((num >= 0) ? _flDst[num] : c);
		}
		return _textBuilder.ToString();
	}

	[HideFromIl2Cpp]
	private float _CorrLevel()
	{
		return Mathf.Clamp01((Time.realtimeSinceStartup - _sessionStartTime) / 60f / 30f);
	}

	[HideFromIl2Cpp]
	private void _PrewarmAtlas()
	{
		if ((Object)(object)_text == (Object)null)
		{
			return;
		}
		try
		{
			StringBuilder stringBuilder = new StringBuilder(512);
			foreach (char value3 in _crMap.Values)
			{
				stringBuilder.Append(value3);
			}
			stringBuilder.Append(_flDst);
			string[] array = sHorrorFaces;
			foreach (string value in array)
			{
				stringBuilder.Append(value);
			}
			char[] array2 = sNoisePool;
			foreach (char value2 in array2)
			{
				stringBuilder.Append(value2);
			}
			for (int j = 0; j < 112; j++)
			{
				stringBuilder.Append((char)(768 + j));
			}
			stringBuilder.Append("\u20e0 \u20dd \u20d0 \u20d1 \u20d2 \u20d3");
			string text = ((TMP_Text)_text).text;
			((TMP_Text)_text).text = stringBuilder.ToString();
			((TMP_Text)_text).ForceMeshUpdate(false, false);
			((TMP_Text)_text).text = text;
			((TMP_Text)_text).ForceMeshUpdate(false, false);
		}
		catch
		{
		}
	}

	[HideFromIl2Cpp]
	private string RandomNoise(int length)
	{
		if (length <= 0)
		{
			return "";
		}
		_textBuilder.Clear();
		for (int i = 0; i < length; i++)
		{
			_textBuilder.Append(sNoisePool[sRandom.Next(sNoisePool.Length)]);
		}
		return _textBuilder.ToString();
	}

	[HideFromIl2Cpp]
	private string RandomRoom()
	{
		return sCamRooms[sRandom.Next(sCamRooms.Length)];
	}

	private void ResetVisualsToStable()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_text == (Object)null))
		{
			RectTransform textRectTransform = _textRectTransform;
			textRectTransform.anchoredPosition = _baseAnchoredPosition;
			((Transform)textRectTransform).localRotation = Quaternion.identity;
			((Transform)textRectTransform).localScale = new Vector3(_baseScale, _baseScale, 1f);
			((Graphic)_text).color = _baseColor;
			((TMP_Text)_text).outlineColor = Color32.op_Implicit(_baseOutlineColor);
			((TMP_Text)_text).outlineWidth = _baseOutlineWidth;
			SetText(_modText);
		}
	}

	private void OnDisable()
	{
		_isEffectRunning = false;
		if (_schedulerRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_schedulerRoutine);
		}
		if (_breathingRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_breathingRoutine);
		}
		if ((Object)(object)_text != (Object)null)
		{
			ResetVisualsToStable();
		}
	}

	private void OnDestroy()
	{
		OnDisable();
		_text = null;
		_textRectTransform = null;
	}
}
