using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityStatusEffect : MonoBehaviour
{
	private SOStatusEffects statusEffect;
	public float abilityDurationTimer;
	private EntityStats entityEffectIsAppliedTo;

	public int damage;

	private readonly float damageOverTimeCooldown = 1f;
	private float timerTillNextDamage;

	private void Update()
	{
		AbilityDurationTimer();
		DamageOverTimeEffect();
	}

	//set data
	public void Initilize(EntityStats casterInfo, SOStatusEffects statusEffect, EntityStats entityToApplyEffectTo)
	{
		transform.localPosition = Vector3.zero;
		this.statusEffect = statusEffect;
		gameObject.name = statusEffect.Name + "Effect";
		entityEffectIsAppliedTo = entityToApplyEffectTo;

		damage = (int)(statusEffect.effectValue * Utilities.GetLevelModifier(casterInfo.entityLevel));
		timerTillNextDamage = 0f;

		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}
	public void ClearEffect()
	{
		entityEffectIsAppliedTo.UnApplyStatusEffect(this);
		Destroy(gameObject);
	}

	//timers
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
		if (!statusEffect.isDOT || entityEffectIsAppliedTo.IsEntityDead()) return;

		timerTillNextDamage -= Time.deltaTime;
		if (timerTillNextDamage < 0)
		{
			DamageSourceInfo damageSourceInfo = new(null, IDamagable.HitBye.enviroment, damage, statusEffect.damageType, false);

			damageSourceInfo.SetDeathMessage(statusEffect);
			entityEffectIsAppliedTo.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);
			timerTillNextDamage = damageOverTimeCooldown;
		}
	}

	public void ResetAbilityTimer()
	{
		abilityDurationTimer = 0;
	}
	public float GetAbilityDuration()
	{
		return abilityDurationTimer;
	}
	public SOStatusEffects GrabAbilityBaseRef()
	{
		return statusEffect;
	}
}
