using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityStatusEffect : MonoBehaviour
{
	private SOClassAbilities abilityBaseRef;
	private float abilityDurationTimer;
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
	public void Initilize(SOClassAbilities abilityBaseRef, EntityStats casterInfo, EntityStats entityToApplyEffectTo)
	{
		transform.localPosition = Vector3.zero;
		this.abilityBaseRef = abilityBaseRef;
		gameObject.name = abilityBaseRef.Name + "Effect";
		entityEffectIsAppliedTo = entityToApplyEffectTo;

		int newDamage = (int)(abilityBaseRef.damageValue * Utilities.GetLevelModifier(casterInfo.entityLevel));

		if (damageType == DamageType.isPhysicalDamageType)
			damage = (int)(newDamage * casterInfo.physicalDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isPoisonDamageType)
			damage = (int)(newDamage * casterInfo.poisonDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isFireDamageType)
			damage = (int)(newDamage * casterInfo.fireDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isIceDamageType)
			damage = (int)(newDamage * casterInfo.iceDamagePercentageModifier.finalPercentageValue);

		//damage *= (int)casterInfo.damageDealtModifier.finalPercentageValue; DoT effects always do full damage (base damage is low already)
		timerTillNextDamage = 0f;

		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	//timer
	private void AbilityDurationTimer()
	{
		abilityDurationTimer += Time.deltaTime;

		if (abilityDurationTimer >= abilityBaseRef.abilityDuration)
		{
			entityEffectIsAppliedTo.UnApplyStatusEffect(this, abilityBaseRef);
			Destroy(gameObject);
		}
	}

	//dot effect if it has one
	private void DamageOverTimeEffect()
	{
		if (!abilityBaseRef.isDOT) return;

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
		abilityDurationTimer = abilityBaseRef.abilityDuration;
	}
	public float GetTimer()
	{
		return abilityDurationTimer;
	}
	public SOClassAbilities GrabAbilityBaseRef()
	{
		return abilityBaseRef;
	}
}
