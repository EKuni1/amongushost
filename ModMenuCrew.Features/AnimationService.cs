using System;
using System.Collections;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.Audio;

namespace ModMenuCrew.Features;

internal static class AnimationService
{
	private static AnimationClip _cachedBlurAnim;

	private static MushroomMixupScreenTint _cachedTint;

	private static AudioClip _cachedDeadSound;

	private static AudioClip _cachedTextSound;

	private static PlayerControl L => PlayerControl.LocalPlayer;

	private static PlayerAnimations A
	{
		get
		{
			PlayerControl l = L;
			if (!((Object)(object)l != (Object)null) || !((Object)(object)l.MyPhysics != (Object)null))
			{
				return null;
			}
			return l.MyPhysics.Animations;
		}
	}

	private static PetBehaviour Pet
	{
		get
		{
			try
			{
				PlayerControl l = L;
				object result;
				if (l == null)
				{
					result = null;
				}
				else
				{
					CosmeticsLayer cosmetics = l.cosmetics;
					result = ((cosmetics != null) ? cosmetics.currentPet : null);
				}
				return (PetBehaviour)result;
			}
			catch
			{
				return null;
			}
		}
	}

	private static SkinLayer Skin
	{
		get
		{
			try
			{
				PlayerControl l = L;
				object result;
				if (l == null)
				{
					result = null;
				}
				else
				{
					CosmeticsLayer cosmetics = l.cosmetics;
					result = ((cosmetics != null) ? cosmetics.skin : null);
				}
				return (SkinLayer)result;
			}
			catch
			{
				return null;
			}
		}
	}

	private static void StartCo(IEnumerator co)
	{
		try
		{
			PlayerControl l = L;
			if (!((Object)(object)l == (Object)null) && co != null)
			{
				((MonoBehaviour)l.MyPhysics).StartCoroutine(co);
			}
		}
		catch
		{
		}
	}

	private static void PlayRole(Func<RoleManager, RoleEffectAnimation> pick, float duration = 0f)
	{
		try
		{
			PlayerControl l = L;
			if ((Object)(object)l == (Object)null)
			{
				return;
			}
			RoleManager instance = DestroyableSingleton<RoleManager>.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				return;
			}
			RoleEffectAnimation val = pick(instance);
			if (!((Object)(object)val == (Object)null))
			{
				RoleEffectAnimation val2 = Object.Instantiate<RoleEffectAnimation>(val, ((Component)l).transform);
				bool flag = (Object)(object)l.cosmetics != (Object)null && l.cosmetics.FlipX;
				if (duration > 0f)
				{
					val2.Play(l, (Action)null, flag, (SoundType)1, duration, true, -0.05f);
				}
				else
				{
					val2.Play(l, (Action)null, flag, (SoundType)1, 0f, true, 0f);
				}
			}
		}
		catch
		{
		}
	}

	internal static void PlayIdle()
	{
		try
		{
			PlayerAnimations a = A;
			if (a != null)
			{
				a.PlayIdleAnimation();
			}
		}
		catch
		{
		}
	}

	internal static void PlayRun()
	{
		try
		{
			PlayerAnimations a = A;
			if (a != null)
			{
				a.PlayRunAnimation();
			}
		}
		catch
		{
		}
	}

	internal static void PlayClimbUp()
	{
		try
		{
			PlayerAnimations a = A;
			if (a != null)
			{
				a.PlayClimbAnimation(false);
			}
		}
		catch
		{
		}
	}

	internal static void PlayClimbDown()
	{
		try
		{
			PlayerAnimations a = A;
			if (a != null)
			{
				a.PlayClimbAnimation(true);
			}
		}
		catch
		{
		}
	}

	internal static void PlayEnterVent()
	{
		try
		{
			PlayerAnimations a = A;
			if ((Object)(object)a != (Object)null)
			{
				StartCo(a.CoPlayEnterVentAnimation(0));
			}
		}
		catch
		{
		}
	}

	internal static void PlayExitVent()
	{
		try
		{
			PlayerAnimations a = A;
			if ((Object)(object)a != (Object)null)
			{
				StartCo(a.CoPlayExitVentAnimation());
			}
		}
		catch
		{
		}
	}

	internal static void PlayJump()
	{
		try
		{
			PlayerAnimations a = A;
			if ((Object)(object)a != (Object)null)
			{
				StartCo(a.CoPlayJumpAnimation());
			}
		}
		catch
		{
		}
	}

	internal static void PlaySpawn()
	{
		try
		{
			PlayerControl l = L;
			if (!((Object)(object)l == (Object)null))
			{
				PlayerAnimations a = A;
				if ((Object)(object)a != (Object)null)
				{
					StartCo(a.CoPlaySpawnAnimation((Object)(object)l.cosmetics != (Object)null && l.cosmetics.FlipX));
				}
			}
		}
		catch
		{
		}
	}

	internal static void PlayScannerOn()
	{
		try
		{
			PlayerControl l = L;
			if (!((Object)(object)l == (Object)null))
			{
				PlayerAnimations a = A;
				if (a != null)
				{
					a.PlayScanner(true, false, (Object)(object)l.cosmetics != (Object)null && l.cosmetics.FlipX);
				}
			}
		}
		catch
		{
		}
	}

	internal static void PlayScannerOff()
	{
		try
		{
			PlayerAnimations a = A;
			if (a != null)
			{
				a.PlayScanner(false, false, false);
			}
		}
		catch
		{
		}
	}

	internal static void PlayGhostIdle()
	{
		try
		{
			PlayerAnimations a = A;
			if (a != null)
			{
				a.PlayGhostIdleAnimation();
			}
		}
		catch
		{
		}
	}

	internal static void PlayGuardianAngelIdle()
	{
		try
		{
			PlayerAnimations a = A;
			if (a != null)
			{
				a.PlayGuardianAngelIdleAnimation();
			}
		}
		catch
		{
		}
	}

	internal static void PlayShapeshift()
	{
		PlayRole((RoleManager rm) => rm.shapeshiftAnim);
	}

	internal static void PlayVanishCharge()
	{
		PlayRole((RoleManager rm) => rm.vanish_ChargeAnim, 0.5f);
	}

	internal static void PlayVanishPoof()
	{
		PlayRole((RoleManager rm) => rm.vanish_PoofAnim);
	}

	internal static void PlayAppearPoof()
	{
		PlayRole((RoleManager rm) => rm.appear_PoofAnim);
	}

	internal static void PlayProtectFlash()
	{
		PlayRole((RoleManager rm) => rm.protectAnim);
	}

	internal static void PlayProtectLoop()
	{
		PlayRole((RoleManager rm) => rm.protectLoopAnim, 5f);
	}

	internal static void PlayPetSequence()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			PlayerControl l = L;
			if (!((Object)(object)l == (Object)null) && !((Object)(object)l.MyPhysics == (Object)null))
			{
				Vector2 val = Vector2.op_Implicit(((Component)l).transform.position);
				Vector2 val2 = val + new Vector2(0.5f, 0f);
				l.MyPhysics.PetPet(val, val2);
			}
		}
		catch
		{
		}
	}

	internal static void PlayPetIdle()
	{
		try
		{
			PetBehaviour pet = Pet;
			if ((Object)(object)pet != (Object)null)
			{
				pet.SetIdle();
			}
		}
		catch
		{
		}
	}

	internal static void PlayPetWalk()
	{
		try
		{
			PetBehaviour pet = Pet;
			if ((Object)(object)pet != (Object)null)
			{
				pet.StartWalkAnim();
			}
		}
		catch
		{
		}
	}

	internal static void PlayPetScared()
	{
		try
		{
			PetBehaviour pet = Pet;
			if ((Object)(object)pet != (Object)null)
			{
				pet.SetScared();
			}
		}
		catch
		{
		}
	}

	internal static void PlayPetMourn()
	{
		try
		{
			PetBehaviour pet = Pet;
			if ((Object)(object)pet != (Object)null)
			{
				pet.SetMourning();
			}
		}
		catch
		{
		}
	}

	internal static void PlaySkinIdle()
	{
		try
		{
			PlayerControl l = L;
			SkinLayer skin = Skin;
			if ((Object)(object)skin != (Object)null && (Object)(object)l != (Object)null && (Object)(object)l.cosmetics != (Object)null)
			{
				skin.SetIdle(l.cosmetics.FlipX);
			}
		}
		catch
		{
		}
	}

	internal static void PlaySkinJump()
	{
		try
		{
			PlayerControl l = L;
			SkinLayer skin = Skin;
			if ((Object)(object)skin != (Object)null && (Object)(object)l != (Object)null && (Object)(object)l.cosmetics != (Object)null)
			{
				skin.SetJump(l.cosmetics.FlipX);
			}
		}
		catch
		{
		}
	}

	internal static void PlaySkinClimbUp()
	{
		try
		{
			SkinLayer skin = Skin;
			if (skin != null)
			{
				skin.SetClimb(false);
			}
		}
		catch
		{
		}
	}

	internal static void PlaySkinClimbDown()
	{
		try
		{
			SkinLayer skin = Skin;
			if (skin != null)
			{
				skin.SetClimb(true);
			}
		}
		catch
		{
		}
	}

	internal static void PlaySkinSpawn()
	{
		try
		{
			PlayerControl l = L;
			SkinLayer skin = Skin;
			if ((Object)(object)skin != (Object)null && (Object)(object)l != (Object)null && (Object)(object)l.cosmetics != (Object)null)
			{
				skin.SetSpawn(l.cosmetics.FlipX, 0f);
			}
		}
		catch
		{
		}
	}

	internal static void PlaySkinGhost()
	{
		try
		{
			SkinLayer skin = Skin;
			if (skin != null)
			{
				skin.SetGhost();
			}
		}
		catch
		{
		}
	}

	internal static void PlayHatClimb()
	{
		try
		{
			PlayerControl l = L;
			if (l == null)
			{
				return;
			}
			CosmeticsLayer cosmetics = l.cosmetics;
			if (cosmetics != null)
			{
				HatParent hat = cosmetics.hat;
				if (hat != null)
				{
					hat.SetClimbAnim();
				}
			}
		}
		catch
		{
		}
	}

	internal static void PlayHatFloor()
	{
		try
		{
			PlayerControl l = L;
			if (l == null)
			{
				return;
			}
			CosmeticsLayer cosmetics = l.cosmetics;
			if (cosmetics != null)
			{
				HatParent hat = cosmetics.hat;
				if (hat != null)
				{
					hat.SetFloorAnim();
				}
			}
		}
		catch
		{
		}
	}

	internal static void PlayAlertFlash()
	{
		try
		{
			HudManager instance = DestroyableSingleton<HudManager>.Instance;
			if (!((Object)(object)instance == (Object)null) && !((Object)(object)instance.AlertFlash == (Object)null))
			{
				Animator animator = instance.AlertFlash.animator;
				if (!((Object)(object)animator == (Object)null))
				{
					animator.SetTrigger("OnFlash");
				}
			}
		}
		catch
		{
		}
	}

	private static AnimationClip FindBlurAnim()
	{
		if ((Object)(object)_cachedBlurAnim != (Object)null)
		{
			return _cachedBlurAnim;
		}
		try
		{
			PlayerControl l = L;
			if ((Object)(object)l != (Object)null && l.KillAnimations != null)
			{
				for (int i = 0; i < ((Il2CppArrayBase<KillAnimation>)(object)l.KillAnimations).Count; i++)
				{
					KillAnimation val = ((Il2CppArrayBase<KillAnimation>)(object)l.KillAnimations)[i];
					if ((Object)(object)val != (Object)null && (Object)(object)val.BlurAnim != (Object)null)
					{
						_cachedBlurAnim = val.BlurAnim;
						return _cachedBlurAnim;
					}
				}
			}
			Il2CppReferenceArray<Object> val2 = Resources.FindObjectsOfTypeAll(Il2CppType.Of<KillAnimation>());
			if (val2 != null)
			{
				for (int j = 0; j < ((Il2CppArrayBase<Object>)(object)val2).Length; j++)
				{
					Object obj = ((Il2CppArrayBase<Object>)(object)val2)[j];
					KillAnimation val3 = ((obj != null) ? ((Il2CppObjectBase)obj).TryCast<KillAnimation>() : null);
					if ((Object)(object)val3 != (Object)null && (Object)(object)val3.BlurAnim != (Object)null)
					{
						_cachedBlurAnim = val3.BlurAnim;
						return _cachedBlurAnim;
					}
				}
			}
		}
		catch
		{
		}
		return null;
	}

	internal static void PlayKillBlur()
	{
		try
		{
			PlayerControl l = L;
			if ((Object)(object)l == (Object)null || (Object)(object)l.MyPhysics == (Object)null)
			{
				return;
			}
			PlayerAnimations animations = l.MyPhysics.Animations;
			if (!((Object)(object)animations == (Object)null))
			{
				AnimationClip val = FindBlurAnim();
				if (!((Object)(object)val == (Object)null))
				{
					StartCo(animations.CoPlayCustomAnimation(val));
				}
			}
		}
		catch
		{
		}
	}

	private static MushroomMixupScreenTint FindMushroomTint()
	{
		if ((Object)(object)_cachedTint != (Object)null)
		{
			return _cachedTint;
		}
		try
		{
			HudManager instance = DestroyableSingleton<HudManager>.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				MushroomMixupScreenTint componentInChildren = ((Component)instance).GetComponentInChildren<MushroomMixupScreenTint>(true);
				if ((Object)(object)componentInChildren != (Object)null)
				{
					_cachedTint = componentInChildren;
					return componentInChildren;
				}
			}
			ShipStatus instance2 = ShipStatus.Instance;
			if ((Object)(object)instance2 != (Object)null)
			{
				MushroomMixupScreenTint componentInChildren2 = ((Component)instance2).GetComponentInChildren<MushroomMixupScreenTint>(true);
				if ((Object)(object)componentInChildren2 != (Object)null)
				{
					_cachedTint = componentInChildren2;
					return componentInChildren2;
				}
			}
			Il2CppReferenceArray<Object> val = Resources.FindObjectsOfTypeAll(Il2CppType.Of<MushroomMixupScreenTint>());
			if (val != null && ((Il2CppArrayBase<Object>)(object)val).Length > 0)
			{
				for (int i = 0; i < ((Il2CppArrayBase<Object>)(object)val).Length; i++)
				{
					Object obj = ((Il2CppArrayBase<Object>)(object)val)[i];
					MushroomMixupScreenTint val2 = ((obj != null) ? ((Il2CppObjectBase)obj).TryCast<MushroomMixupScreenTint>() : null);
					if ((Object)(object)val2 != (Object)null)
					{
						_cachedTint = val2;
						return _cachedTint;
					}
				}
			}
		}
		catch
		{
		}
		return null;
	}

	internal static void PlayMushroomTintIn()
	{
		try
		{
			MushroomMixupScreenTint obj = FindMushroomTint();
			if (obj != null)
			{
				obj.Activate();
			}
		}
		catch
		{
		}
	}

	internal static void PlayMushroomTintOut()
	{
		try
		{
			MushroomMixupScreenTint obj = FindMushroomTint();
			if (obj != null)
			{
				obj.Deactivate();
			}
		}
		catch
		{
		}
	}

	internal static void PlayParticleBurst()
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			PlayerControl l = L;
			if ((Object)(object)l == (Object)null)
			{
				return;
			}
			PlayerParticles val = Object.FindObjectOfType<PlayerParticles>();
			if ((Object)(object)val == (Object)null)
			{
				Il2CppReferenceArray<Object> val2 = Resources.FindObjectsOfTypeAll(Il2CppType.Of<PlayerParticles>());
				if (val2 != null)
				{
					for (int i = 0; i < ((Il2CppArrayBase<Object>)(object)val2).Length; i++)
					{
						Object obj = ((Il2CppArrayBase<Object>)(object)val2)[i];
						PlayerParticles val3 = ((obj != null) ? ((Il2CppObjectBase)obj).TryCast<PlayerParticles>() : null);
						if ((Object)(object)val3 != (Object)null)
						{
							val = val3;
							break;
						}
					}
				}
			}
			if ((Object)(object)val == (Object)null || (Object)(object)val.pool == (Object)null)
			{
				return;
			}
			for (int j = 0; j < 8; j++)
			{
				PoolableBehavior val4 = val.pool.Get<PoolableBehavior>();
				if (!((Object)(object)val4 == (Object)null))
				{
					((Component)val4).transform.position = ((Component)l).transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0f);
					((Component)val4).gameObject.SetActive(true);
					continue;
				}
				break;
			}
		}
		catch
		{
		}
	}

	private static AudioClip FindDeadSound()
	{
		if ((Object)(object)_cachedDeadSound != (Object)null)
		{
			return _cachedDeadSound;
		}
		try
		{
			Il2CppReferenceArray<Object> val = Resources.FindObjectsOfTypeAll(Il2CppType.Of<MeetingIntroAnimation>());
			if (val != null)
			{
				for (int i = 0; i < ((Il2CppArrayBase<Object>)(object)val).Length; i++)
				{
					Object obj = ((Il2CppArrayBase<Object>)(object)val)[i];
					MeetingIntroAnimation val2 = ((obj != null) ? ((Il2CppObjectBase)obj).TryCast<MeetingIntroAnimation>() : null);
					if ((Object)(object)val2 != (Object)null && (Object)(object)val2.PlayerDeadSound != (Object)null)
					{
						_cachedDeadSound = val2.PlayerDeadSound;
						return _cachedDeadSound;
					}
				}
			}
		}
		catch
		{
		}
		return null;
	}

	private static AudioClip FindTextSound()
	{
		if ((Object)(object)_cachedTextSound != (Object)null)
		{
			return _cachedTextSound;
		}
		try
		{
			Il2CppReferenceArray<Object> val = Resources.FindObjectsOfTypeAll(Il2CppType.Of<ExileController>());
			if (val != null)
			{
				for (int i = 0; i < ((Il2CppArrayBase<Object>)(object)val).Length; i++)
				{
					Object obj = ((Il2CppArrayBase<Object>)(object)val)[i];
					ExileController val2 = ((obj != null) ? ((Il2CppObjectBase)obj).TryCast<ExileController>() : null);
					if ((Object)(object)val2 != (Object)null && (Object)(object)val2.TextSound != (Object)null)
					{
						_cachedTextSound = val2.TextSound;
						return _cachedTextSound;
					}
				}
			}
		}
		catch
		{
		}
		return null;
	}

	internal static void PlayMeetingSlam()
	{
		try
		{
			AudioClip val = FindDeadSound();
			if (!((Object)(object)val == (Object)null))
			{
				SoundManager instance = SoundManager.Instance;
				if (instance != null)
				{
					instance.PlaySound(val, false, 0.7f, (AudioMixerGroup)null);
				}
			}
		}
		catch
		{
		}
	}

	internal static void PlayEjectTextSfx()
	{
		try
		{
			AudioClip val = FindTextSound();
			if (!((Object)(object)val == (Object)null))
			{
				SoundManager instance = SoundManager.Instance;
				if (instance != null)
				{
					instance.PlaySoundImmediate(val, false, 0.8f, 1f, (AudioMixerGroup)null);
				}
			}
		}
		catch
		{
		}
	}
}
