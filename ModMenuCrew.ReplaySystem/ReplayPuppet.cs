using System;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes;
using PowerTools;
using UnityEngine;

namespace ModMenuCrew.ReplaySystem;

public class ReplayPuppet : MonoBehaviour
{
	private SpriteRenderer rend;

	[NonSerialized]
	public ReplayPlayerInfo Info;

	private Vector3 originalScale = Vector3.one;

	private float walkBobPhase;

	private byte lastAnim;

	private float lastAlpha = 1f;

	private bool isInVent;

	private SpriteAnim bodyAnim;

	private AnimationClip bodyRunClip;

	private AnimationClip bodyIdleClip;

	private AnimationClip bodyGhostIdleClip;

	private AnimationClip bodyEnterVentClip;

	private AnimationClip bodyExitVentClip;

	private AnimationClip bodySpawnClip;

	private AnimationClip bodyClimbUpClip;

	private AnimationClip bodyClimbDownClip;

	private bool hasBodyAnim;

	private SpriteRenderer skinRend;

	private SpriteAnim skinAnim;

	private SkinViewData skinViewData;

	private bool hasSkinAnim;

	private bool lastFaceRight = true;

	public ReplayPuppet(IntPtr ptr)
		: base(ptr)
	{
	}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
	//IL_0006: Unknown result type (might be due to invalid IL or missing references)


	[HideFromIl2Cpp]
	public void Initialize(ReplayPlayerInfo info, SpriteRenderer mainSprite)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Info = info;
		rend = mainSprite;
		originalScale = ((Component)this).transform.localScale;
	}

	[HideFromIl2Cpp]
	public void SetupBodyAnimation(AnimationClip runClip, AnimationClip idleClip, AnimationClip ghostIdleClip = null, AnimationClip enterVentClip = null, AnimationClip exitVentClip = null, AnimationClip spawnClip = null, AnimationClip climbUpClip = null, AnimationClip climbDownClip = null)
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		bodyRunClip = runClip;
		bodyIdleClip = idleClip;
		bodyGhostIdleClip = ghostIdleClip;
		bodyEnterVentClip = enterVentClip;
		bodyExitVentClip = exitVentClip;
		bodySpawnClip = spawnClip;
		bodyClimbUpClip = climbUpClip;
		bodyClimbDownClip = climbDownClip;
		if ((Object)(object)runClip == (Object)null && (Object)(object)idleClip == (Object)null)
		{
			return;
		}
		try
		{
			bodyAnim = ((Component)this).gameObject.GetComponent<SpriteAnim>();
			if ((Object)(object)bodyAnim == (Object)null)
			{
				bodyAnim = ((Component)this).gameObject.AddComponent<SpriteAnim>();
			}
			if ((Object)(object)bodyAnim != (Object)null)
			{
				try
				{
					bodyAnim.Initialize();
				}
				catch
				{
				}
				hasBodyAnim = true;
				if ((Object)(object)bodyIdleClip != (Object)null)
				{
					bodyAnim.Play(bodyIdleClip, 1f);
				}
			}
		}
		catch (Exception ex)
		{
			ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			bool flag = default(bool);
			BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(40, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ReplayPuppet] SpriteAnim setup failed: ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(ex.Message);
			}
			log.LogWarning(val);
			hasBodyAnim = false;
		}
	}

	[HideFromIl2Cpp]
	public void SetupSkinAnimation(SpriteRenderer skinRenderer, SkinViewData viewData)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		if ((Object)(object)skinRenderer == (Object)null || (Object)(object)viewData == (Object)null)
		{
			return;
		}
		skinRend = skinRenderer;
		skinViewData = viewData;
		try
		{
			skinAnim = ((Component)skinRenderer).gameObject.GetComponent<SpriteAnim>();
			if ((Object)(object)skinAnim == (Object)null)
			{
				skinAnim = ((Component)skinRenderer).gameObject.AddComponent<SpriteAnim>();
			}
			if ((Object)(object)skinAnim != (Object)null)
			{
				try
				{
					skinAnim.Initialize();
				}
				catch
				{
				}
				hasSkinAnim = true;
				if ((Object)(object)viewData.IdleAnim != (Object)null)
				{
					skinAnim.Play(viewData.IdleAnim, 1f);
				}
				else if ((Object)(object)viewData.IdleFrame != (Object)null)
				{
					skinRenderer.sprite = viewData.IdleFrame;
				}
			}
		}
		catch (Exception ex)
		{
			ManualLogSource log = ((BasePlugin)ModMenuCrewPlugin.Instance).Log;
			bool flag = default(bool);
			BepInExWarningLogInterpolatedStringHandler val = new BepInExWarningLogInterpolatedStringHandler(45, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[ReplayPuppet] Skin SpriteAnim setup failed: ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(ex.Message);
			}
			log.LogWarning(val);
			hasSkinAnim = false;
		}
	}

	[HideFromIl2Cpp]
	public void UpdateState(Vector2 pos, bool faceRight, bool isDead, AnimState anim)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			float num = Mathf.Min(Time.deltaTime, 0.1f);
			((Component)this).transform.position = new Vector3(pos.x, pos.y, pos.y / 1000f);
			float num2 = (faceRight ? 1f : (-1f));
			Vector3 localScale = ((Component)this).transform.localScale;
			float num3 = Mathf.Abs(localScale.x);
			((Component)this).transform.localScale = new Vector3(num2 * num3, localScale.y, localScale.z);
			int childCount = ((Component)this).transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = ((Component)this).transform.GetChild(i);
				if (!((Object)(object)child == (Object)null) && (((Object)child).name == "NameTag" || ((Object)child).name == "RoleIndicator"))
				{
					Vector3 localScale2 = child.localScale;
					child.localScale = new Vector3(num2 * Mathf.Abs(localScale2.x), localScale2.y, localScale2.z);
				}
			}
			HandleAnimState(anim, isDead, num);
			HandleAnimClips(anim, faceRight);
			HandleWalkCycle(anim, num);
			float num4 = ((isDead || anim == AnimState.Ghost) ? 0.45f : 1f);
			if (isInVent)
			{
				num4 = 0f;
			}
			if (Mathf.Abs(num4 - lastAlpha) > 0.01f)
			{
				lastAlpha = Mathf.Lerp(lastAlpha, num4, num * 8f);
				SetAlpha(lastAlpha);
			}
			lastFaceRight = faceRight;
			lastAnim = (byte)anim;
		}
		catch
		{
		}
	}

	private void HandleAnimClips(AnimState anim, bool faceRight)
	{
		if (!hasBodyAnim || (Object)(object)bodyAnim == (Object)null || ((uint)anim == lastAnim && faceRight == lastFaceRight))
		{
			return;
		}
		try
		{
			bool flag = !faceRight;
			AnimationClip val = null;
			AnimationClip val2 = null;
			switch (anim)
			{
			case AnimState.Run:
				val = bodyRunClip;
				if (hasSkinAnim && (Object)(object)skinViewData != (Object)null)
				{
					val2 = ((flag && (Object)(object)skinViewData.RunLeftAnim != (Object)null) ? skinViewData.RunLeftAnim : skinViewData.RunAnim);
				}
				break;
			case AnimState.Idle:
				val = bodyIdleClip;
				if (hasSkinAnim && (Object)(object)skinViewData != (Object)null)
				{
					val2 = ((flag && (Object)(object)skinViewData.IdleLeftAnim != (Object)null) ? skinViewData.IdleLeftAnim : skinViewData.IdleAnim);
				}
				break;
			case AnimState.Ghost:
				val = bodyGhostIdleClip ?? bodyIdleClip;
				break;
			case AnimState.VentEnter:
				val = bodyEnterVentClip;
				if (hasSkinAnim && (Object)(object)skinViewData != (Object)null)
				{
					val2 = ((flag && (Object)(object)skinViewData.EnterLeftVentAnim != (Object)null) ? skinViewData.EnterLeftVentAnim : skinViewData.EnterVentAnim);
				}
				break;
			case AnimState.VentExit:
				val = bodyExitVentClip;
				if (hasSkinAnim && (Object)(object)skinViewData != (Object)null)
				{
					val2 = ((flag && (Object)(object)skinViewData.ExitLeftVentAnim != (Object)null) ? skinViewData.ExitLeftVentAnim : skinViewData.ExitVentAnim);
				}
				break;
			case AnimState.Spawn:
				val = bodySpawnClip ?? bodyIdleClip;
				if (hasSkinAnim && (Object)(object)skinViewData != (Object)null)
				{
					val2 = ((flag && (Object)(object)skinViewData.SpawnLeftAnim != (Object)null) ? skinViewData.SpawnLeftAnim : skinViewData.SpawnAnim);
				}
				break;
			case AnimState.Climb:
				val = bodyClimbUpClip ?? bodyIdleClip;
				if (hasSkinAnim && (Object)(object)skinViewData != (Object)null)
				{
					val2 = skinViewData.ClimbAnim;
				}
				break;
			case AnimState.ClimbDown:
				val = bodyClimbDownClip ?? bodyClimbUpClip ?? bodyIdleClip;
				if (hasSkinAnim && (Object)(object)skinViewData != (Object)null)
				{
					val2 = skinViewData.ClimbDownAnim ?? skinViewData.ClimbAnim;
				}
				break;
			}
			if ((Object)(object)val != (Object)null)
			{
				AnimationClip currentAnimation = bodyAnim.GetCurrentAnimation();
				if ((Object)(object)currentAnimation == (Object)null || ((Il2CppObjectBase)currentAnimation).Pointer != ((Il2CppObjectBase)val).Pointer)
				{
					bodyAnim.Play(val, 1f);
				}
			}
			if (!hasSkinAnim || !((Object)(object)skinAnim != (Object)null) || !((Object)(object)val2 != (Object)null))
			{
				return;
			}
			AnimationClip currentAnimation2 = skinAnim.GetCurrentAnimation();
			if ((Object)(object)currentAnimation2 == (Object)null || ((Il2CppObjectBase)currentAnimation2).Pointer != ((Il2CppObjectBase)val2).Pointer)
			{
				skinAnim.Play(val2, 1f);
				bool flag2 = false;
				if (flag && (Object)(object)skinViewData != (Object)null)
				{
					flag2 = (Object)(object)val2 == (Object)(object)skinViewData.RunLeftAnim || (Object)(object)val2 == (Object)(object)skinViewData.IdleLeftAnim || (Object)(object)val2 == (Object)(object)skinViewData.EnterLeftVentAnim || (Object)(object)val2 == (Object)(object)skinViewData.ExitLeftVentAnim || (Object)(object)val2 == (Object)(object)skinViewData.SpawnLeftAnim;
				}
				if ((Object)(object)skinRend != (Object)null)
				{
					skinRend.flipX = flag && !flag2;
				}
			}
		}
		catch
		{
		}
	}

	private void HandleAnimState(AnimState anim, bool isDead, float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Sign(((Component)this).transform.localScale.x);
		if (num == 0f)
		{
			num = 1f;
		}
		Vector3 val = default(Vector3);
		float num2;
		switch (anim)
		{
		case AnimState.VentEnter:
			((Vector3)(ref val))._002Ector(0.01f, 0.01f, 1f);
			num2 = 8f;
			isInVent = true;
			break;
		case AnimState.VentExit:
			val = originalScale;
			num2 = 8f;
			isInVent = false;
			break;
		case AnimState.Spawn:
			val = originalScale;
			num2 = 6f;
			isInVent = false;
			break;
		case AnimState.Ghost:
		{
			float num3 = 1f + Mathf.Sin(Time.time * 2f) * 0.02f;
			((Vector3)(ref val))._002Ector(originalScale.x, originalScale.y * num3, originalScale.z);
			num2 = 100f;
			isInVent = false;
			break;
		}
		default:
			val = originalScale;
			num2 = 15f;
			if (anim != AnimState.VentEnter)
			{
				isInVent = false;
			}
			break;
		}
		Vector3 localScale = ((Component)this).transform.localScale;
		Vector3 val2 = Vector3.Lerp(new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z), val, dt * num2);
		((Component)this).transform.localScale = new Vector3(num * Mathf.Abs(val2.x), val2.y, val2.z);
	}

	private void HandleWalkCycle(AnimState anim, float dt)
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		switch (anim)
		{
		case AnimState.Run:
		{
			walkBobPhase += dt * 14f;
			float num2 = Mathf.Sin(walkBobPhase) * 0.04f;
			if (!hasBodyAnim)
			{
				float num3 = 1f + Mathf.Abs(Mathf.Sin(walkBobPhase)) * 0.015f;
				float num4 = Mathf.Sign(((Component)this).transform.localScale.x);
				if (num4 == 0f)
				{
					num4 = 1f;
				}
				Vector3 localScale = ((Component)this).transform.localScale;
				float num5 = Mathf.Abs(originalScale.x);
				((Component)this).transform.localScale = new Vector3(num4 * num5 * num3, originalScale.y * (1f + Mathf.Cos(walkBobPhase) * 0.02f), localScale.z);
			}
			Vector3 position2 = ((Component)this).transform.position;
			((Component)this).transform.position = new Vector3(position2.x, position2.y + num2, position2.z);
			return;
		}
		case AnimState.Idle:
			if (lastAnim == 0)
			{
				float num = Mathf.Sin(Time.time * 1.5f) * 0.008f;
				Vector3 position = ((Component)this).transform.position;
				((Component)this).transform.position = new Vector3(position.x, position.y + num, position.z);
				return;
			}
			break;
		}
		walkBobPhase = 0f;
	}

	private void SetAlpha(float a)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)rend))
		{
			Color color = rend.color;
			rend.color = new Color(color.r, color.g, color.b, a);
		}
		SetChildAlphaRecursive(((Component)this).transform, a);
	}

	private void SetChildAlphaRecursive(Transform parent, float a)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (!((Object)(object)child == (Object)null))
			{
				SpriteRenderer component = ((Component)child).GetComponent<SpriteRenderer>();
				if (Object.op_Implicit((Object)(object)component))
				{
					Color color = component.color;
					component.color = new Color(color.r, color.g, color.b, a);
				}
				if (child.childCount > 0)
				{
					SetChildAlphaRecursive(child, a);
				}
			}
		}
	}
}
