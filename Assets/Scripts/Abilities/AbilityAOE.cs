using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityAOE : MonoBehaviour
{
	private PlayerController player;
	public SOClassAbilities abilityRef;
	private EntityStats casterInfo;

	public GameObject aoeIndicator;
	public GameObject aoeCollider;
	public LayerMask includeLayer;
	public LayerMask excludeLayer;

	public float abilityDurationTimer;
	private CircleCollider2D circleCollider;
	private BoxCollider2D boxCollider;
	private bool isPlayerAoe;
	public int aoeDamage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	/// <summary>
	///solution for ability:
	///use this parent object to set the position ontop of player position
	///adjust this parent objects rotation based on player position and mouse position relative to world position.
	///use child object aoeIndicator and aoeCollider of this parent object to offset child objects based on boxAoeSizeY.
	/// </summary>

	private void Update()
	{
		AbilityDurationTimer();
	}

	//set data
	public void Initilize(SOClassAbilities abilityRef, EntityStats casterInfo)
	{
		this.abilityRef = abilityRef;
		gameObject.name = abilityRef.Name + "Aoe";
		aoeIndicator.GetComponent<SpriteRenderer>().sprite = abilityRef.abilitySprite;

		SetupCollider();

		this.casterInfo = casterInfo;
		abilityDurationTimer = abilityRef.aoeDuration;
		if (abilityRef.aoeDuration == 0)
			abilityDurationTimer = 0.1f;

		isPlayerAoe = casterInfo.IsPlayerEntity();
		damageType = (DamageType)abilityRef.damageType;
		int newDamage = (int)(abilityRef.damageValue * Utilities.GetLevelModifier(casterInfo.entityLevel));

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

	///<summery>
	/// boxAoes will need position of thing casting it (can grab via casterInfo) + direction to cast in or target its being casted at
	/// then its position set equal distance between caster position + target its being casted at or direction its pointing
	/// getting position = Lerp(casterPos, targetPos) or Lerp(casterPos, new Vector2(casterPos + directionPos))
	/// 
	/// handle adjustable box shapes
	/// localScale x always set via abilityBaseRef.aoeSizeX, localScale y always set via abilityBaseRef.aoeSizeY
	/// unless marked as Reach target localScale y will reach target + 3 and position will need to be updated.
	///<summery>

	//set up circle/box collider
	private void SetupCollider()
	{
		if (abilityRef.isCircleAOE)
		{
			aoeIndicator.transform.localScale = new Vector2(abilityRef.circleAoeSize, abilityRef.circleAoeSize);
			aoeCollider.transform.localScale = new Vector2(abilityRef.circleAoeSize, abilityRef.circleAoeSize);

			circleCollider = aoeCollider.AddComponent(typeof(CircleCollider2D)) as CircleCollider2D;
			circleCollider.isTrigger = true;
			circleCollider.includeLayers = includeLayer;
			circleCollider.excludeLayers = excludeLayer;
			circleCollider.radius = 0.1f;
			circleCollider.offset = new Vector2(0, 0);
		}
		else
		{
			aoeIndicator.transform.localScale = new Vector2(abilityRef.boxAoeSizeX, abilityRef.boxAoeSizeY);
			aoeCollider.transform.localScale = new Vector2(abilityRef.boxAoeSizeX, abilityRef.boxAoeSizeY);

			boxCollider = aoeCollider.AddComponent(typeof(BoxCollider2D)) as BoxCollider2D;
			boxCollider.isTrigger = true;
			boxCollider.includeLayers = includeLayer;
			boxCollider.excludeLayers = excludeLayer;
			boxCollider.size = new Vector2(0.15f, 0.15f);
			boxCollider.offset = new Vector2(0, 0);
		}
	}

	//optional, helps with applying damage only to enemies
	public void AddPlayerRef(PlayerController player)
	{
		this.player = player;
	}

	//apply damage/effects to all entities inside aoe, called from AoeCollisions script OnTriggerEnter2D
	public void ApplyDamageToEntitiesInAoe(Collider2D other)
	{
		Debug.LogWarning("collision with: " + other.name);

		//status effects only apply to friendlies (add checks later to apply off effects only to enemies etc...)
		if (other.gameObject.layer == LayerMask.NameToLayer("Player") && isPlayerAoe ||
			other.gameObject.layer == LayerMask.NameToLayer("Enemies") && !isPlayerAoe)
			return;

		other.GetComponent<Damageable>().OnHitFromDamageSource(player, other, aoeDamage, (IDamagable.DamageType)damageType, 0,
			abilityRef.isDamagePercentageBased, isPlayerAoe, false);

		if (abilityRef.hasStatusEffects && other.gameObject.GetComponent<EntityStats>() != null)
			other.gameObject.GetComponent<EntityStats>().ApplyNewStatusEffects(abilityRef.statusEffects, casterInfo);
	}

	//timer
	private void AbilityDurationTimer()
	{
		abilityDurationTimer -= Time.deltaTime;

		//if (abilityDurationTimer <= 0)
			//DungeonHandler.AoeAbilitiesCleanUp(this);
	}
}
