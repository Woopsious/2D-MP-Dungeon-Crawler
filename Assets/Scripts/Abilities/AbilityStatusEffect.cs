using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityStatusEffect : MonoBehaviour
{
	private SOClassAbilities abilityBaseRef;
	private float abilityDurationTimer;
	private EntityStats entityEffectIsAppliedTo;

	private bool isPlayerProjectile;
	private int projectileDamage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	public void Initilize(SOClassAbilities abilityBaseRef, EntityStats entityToApplyEffectTo)
	{
		transform.localPosition = Vector3.zero;
		this.abilityBaseRef = abilityBaseRef;
		gameObject.name = abilityBaseRef.Name + "Effect";
		entityEffectIsAppliedTo = entityToApplyEffectTo;

		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	private void Update()
	{
		AbilityDurationTimer();
	}

	private void AbilityDurationTimer()
	{
		abilityDurationTimer += Time.deltaTime;

		if (abilityDurationTimer >= abilityBaseRef.abilityDuration)
		{
			entityEffectIsAppliedTo.UnApplyStatusEffect(this, abilityBaseRef);
			Destroy(gameObject);
		}
	}
}
