using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityStatusEffect : MonoBehaviour
{
	private SOStatusEffects statusEffect;
	public float abilityDurationTimer;
	private EntityStats entityEffectIsAppliedTo;

	public int damage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	private float damageOverTimeCooldown = 1f;
	private float timerTillNextDamage;

	private void Update()
	{
		AbilityDurationTimer();
		DamageOverTimeEffect();
	}

	//set data
	public void Initilize(SOStatusEffects statusEffect, EntityStats casterInfo, EntityStats entityToApplyEffectTo)
	{
		transform.localPosition = Vector3.zero;
		this.statusEffect = statusEffect;
		gameObject.name = statusEffect.Name + "Effect";
		entityEffectIsAppliedTo = entityToApplyEffectTo;

		//dot effects only get level modifier
		damage = (int)(statusEffect.effectValue * Utilities.GetLevelModifier(casterInfo.entityLevel));
		timerTillNextDamage = 0f;

		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}
	public void ClearEffect()
	{
		entityEffectIsAppliedTo.UnApplyStatusEffect(this);
		Destroy(gameObject);
	}

	//timer
	private void AbilityDurationTimer()
	{
		abilityDurationTimer += Time.deltaTime;

		if (abilityDurationTimer >= statusEffect.abilityDuration)
		{
			entityEffectIsAppliedTo.UnApplyStatusEffect(this);
			Destroy(gameObject);
		}
	}

	//dot effect if it has one
	private void DamageOverTimeEffect()
	{
		if (!statusEffect.isDOT) return;

		timerTillNextDamage -= Time.deltaTime;
		if (timerTillNextDamage < 0)
		{
			entityEffectIsAppliedTo.GetComponent<Damageable>().OnHitFromDamageSource(null, null, damage,
				(IDamagable.DamageType)damageType, 0, false, false, true);
			timerTillNextDamage = damageOverTimeCooldown;
		}
	}

	public void ResetTimer()
	{
		abilityDurationTimer = 0;
	}
	public float GetTimer()
	{
		return abilityDurationTimer;
	}
	public SOStatusEffects GrabAbilityBaseRef()
	{
		return statusEffect;
	}
}
