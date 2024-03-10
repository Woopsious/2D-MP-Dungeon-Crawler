using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityAOE : MonoBehaviour
{
	public SOClassAbilities abilityBaseRef;

	private float abilityDurationTimer;
	private CircleCollider2D circleCollider;
	private SpriteRenderer aoeSprite;
	private bool isPlayerAoe;
	private bool applyEffectToEveryone;
	public int aoeDamage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}
	private void Start()
	{
		FindObjectOfType<PlayerController>();
		Initilize(abilityBaseRef, FindObjectOfType<PlayerController>().GetComponent<EntityStats>());
	}

	public void Initilize(SOClassAbilities abilityBaseRef, EntityStats casterInfo)
	{
		transform.localPosition = Vector3.zero;
		this.abilityBaseRef = abilityBaseRef;
		gameObject.name = abilityBaseRef.Name + "Projectile";
		circleCollider = GetComponent<CircleCollider2D>();
		aoeSprite = GetComponent<SpriteRenderer>();
		aoeSprite.sprite = abilityBaseRef.abilitySprite;
		transform.localScale = new Vector3 (abilityBaseRef.aoeSize, abilityBaseRef.aoeSize, 1);
		circleCollider.radius = 0.1f;
		circleCollider.offset = new Vector2(0, 0);

		isPlayerAoe = false;
		damageType = (DamageType)abilityBaseRef.damageType;
		int newDamage = (int)(abilityBaseRef.damageValue * Utilities.GetLevelModifier(casterInfo.entityLevel));

		if (damageType == DamageType.isPhysicalDamageType)
			aoeDamage = (int)(newDamage * casterInfo.physicalDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isPoisonDamageType)
			aoeDamage = (int)(newDamage * casterInfo.poisonDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isFireDamageType)
			aoeDamage = (int)(newDamage * casterInfo.fireDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isIceDamageType)
			aoeDamage = (int)(newDamage * casterInfo.iceDamagePercentageModifier.finalPercentageValue);

		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	private void Update()
	{
		AbilityDurationTimer();
	}
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!abilityBaseRef.isDOT) return;

		if (other.gameObject.GetComponent<EntityStats>() == null) return;

		if (abilityBaseRef.statusEffectType != SOClassAbilities.StatusEffectType.noEffect)
		{
			//status effects only apply to friendlies
			if (other.gameObject.layer == LayerMask.NameToLayer("Player") && isPlayerAoe ||
				other.gameObject.layer == LayerMask.NameToLayer("Enemy") && !isPlayerAoe)
				return;

			EntityStats stats = other.gameObject.GetComponent<EntityStats>();
			stats.ApplyStatusEffect(abilityBaseRef);
		}
		else
		{
			other.GetComponent<Damageable>().OnHitFromDamageSource(aoeDamage, (IDamagable.DamageType)damageType,
				abilityBaseRef.isDamagePercentageBased, isPlayerAoe);
		}
	}

	private void AbilityDurationTimer()
	{
		abilityDurationTimer += Time.deltaTime;

		if (abilityDurationTimer >= abilityBaseRef.abilityDuration)
			Destroy(gameObject);
	}
}
