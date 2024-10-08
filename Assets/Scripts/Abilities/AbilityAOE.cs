using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityAOE : MonoBehaviour
{
	private PlayerController player;
	public SOClassAbilities abilityBaseRef;
	private EntityStats casterInfo;

	public float abilityDurationTimer;
	private CircleCollider2D circleCollider;
	private SpriteRenderer aoeSprite;
	private bool isPlayerAoe;
	public int aoeDamage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	private void Update()
	{
		AbilityDurationTimer();
	}
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.GetComponent<EntityStats>() == null) return;

		//status effects only apply to friendlies (add checks later to apply off effects only to enemies etc...)
		if (other.gameObject.layer == LayerMask.NameToLayer("Player") && isPlayerAoe ||
			other.gameObject.layer == LayerMask.NameToLayer("Enemies") && !isPlayerAoe)
			return;

		other.GetComponent<Damageable>().OnHitFromDamageSource(player, other, aoeDamage, (IDamagable.DamageType)damageType, 0,
			abilityBaseRef.isDamagePercentageBased, isPlayerAoe, false);

		if (abilityBaseRef.hasStatusEffects && other.gameObject.GetComponent<EntityStats>() != null)
			other.gameObject.GetComponent<EntityStats>().ApplyNewStatusEffects(abilityBaseRef.statusEffects, casterInfo);
	}

	//set data
	public void Initilize(SOClassAbilities abilityBaseRef, EntityStats casterInfo)
	{
		this.abilityBaseRef = abilityBaseRef;
		gameObject.name = abilityBaseRef.Name + "Aoe";
		circleCollider = GetComponent<CircleCollider2D>();
		aoeSprite = GetComponent<SpriteRenderer>();
		aoeSprite.sprite = abilityBaseRef.abilitySprite;
		transform.localScale = new Vector3 (abilityBaseRef.aoeSize, abilityBaseRef.aoeSize, 1);
		circleCollider.radius = 0.1f;
		circleCollider.offset = new Vector2(0, 0);

		this.casterInfo = casterInfo;
		abilityDurationTimer = abilityBaseRef.aoeDuration;
		if (abilityBaseRef.aoeDuration == 0)
			abilityDurationTimer = 0.1f;

		isPlayerAoe = casterInfo.IsPlayerEntity();
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

		aoeDamage *= (int)casterInfo.damageDealtModifier.finalPercentageValue;
		gameObject.SetActive(true);
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}
	public void AddPlayerRef(PlayerController player)
	{
		this.player = player;
	}

	//timer
	private void AbilityDurationTimer()
	{
		abilityDurationTimer -= Time.deltaTime;

		if (abilityDurationTimer <= 0)
			DungeonHandler.AoeAbilitiesCleanUp(this);
	}
}
