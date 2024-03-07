using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDirectional : MonoBehaviour
{
	private SOClassAbilities abilityBaseRef;

	private bool isPlayerProjectile;
	private float projectileSpeed;
	private int projectileDamage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	public void Initilize(SOClassAbilities abilityBaseRef, EntityStats casterInfo)
	{
		transform.localPosition = Vector3.zero;
		this.abilityBaseRef = abilityBaseRef;
		gameObject.name = abilityBaseRef.Name + "Projectile";

		isPlayerProjectile = casterInfo.IsPlayerEntity();
		projectileSpeed = abilityBaseRef.projectileSpeed;
		damageType = (DamageType)abilityBaseRef.damageType;

		if (damageType == DamageType.isPhysicalDamageType)
			projectileDamage = (int)(abilityBaseRef.damageValue * casterInfo.physicalDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isPoisonDamageType)
			projectileDamage = (int)(abilityBaseRef.damageValue * casterInfo.poisonDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isFireDamageType)
			projectileDamage = (int)(abilityBaseRef.damageValue * casterInfo.fireDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isIceDamageType)
			projectileDamage = (int)(abilityBaseRef.damageValue * casterInfo.iceDamagePercentageModifier.finalPercentageValue);

		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.GetComponent<Damageable>() == null || isPlayerProjectile == false) return;

		other.GetComponent<Damageable>().OnHitFromDamageSource(projectileDamage, (IDamagable.DamageType)damageType, 
			abilityBaseRef.isDamagePercentageBased ,isPlayerProjectile);

		Destroy(gameObject);
	}

	private void Update()
	{
		//AbilityDurationTimer();
	}

	/*
	private void AbilityDurationTimer()
	{
		abilityDurationTimer += Time.deltaTime;

		if (abilityDurationTimer >= abilityBaseRef.abilityDuration)
		{
			entityEffectIsAppliedTo.UnApplyStatusEffect(this, abilityBaseRef);
			Destroy(gameObject);
		}
	}
	*/
}
