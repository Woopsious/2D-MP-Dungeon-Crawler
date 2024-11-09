using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.EventSystems;

public class Projectiles : MonoBehaviour
{
	private PlayerController player;

	public SOTraps trapBaseRef;
	public SOWeapons weaponBaseRef;
	public SOAbilities abilityBaseRef;
	public EntityStats casterInfo;	//only set for abilities

	private BoxCollider2D boxCollider;
	private SpriteRenderer projectileSprite;
	private bool isEnviromentalProjectile;
	private bool isPlayerProjectile;
	private float projectileSpeed;
	public int projectileDamage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	Vector2 projectileOrigin;
	float distanceTraveled;

	//set trap projectile data
	public void Initilize(SOTraps trap, int trapDamage)
	{
		this.player = null;
		this.trapBaseRef = trap;
		this.weaponBaseRef = null;
		this.abilityBaseRef = null;
		this.casterInfo = null;
		gameObject.name = trapBaseRef.name + "Projectile";

		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = trapBaseRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		this.player = null;
		isEnviromentalProjectile = true;
		isPlayerProjectile = false;
		projectileSpeed = trapBaseRef.projectileSpeed;
		damageType = (DamageType)trapBaseRef.baseDamageType;
		projectileDamage = trapDamage;
		gameObject.SetActive(true);
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	//set weapon projectile data
	public void Initilize(PlayerController player, Weapons weaponRef)
	{
		this.player = player;
		this.trapBaseRef = null;
		this.weaponBaseRef = weaponRef.weaponBaseRef;
		this.abilityBaseRef = null;
		gameObject.name = weaponBaseRef.itemName + "Projectile";

		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = weaponBaseRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		isEnviromentalProjectile = false;
		isPlayerProjectile = weaponRef.isEquippedByPlayer;
		projectileSpeed = weaponBaseRef.projectileSpeed;
		damageType = (DamageType)weaponBaseRef.baseDamageType;
		projectileDamage = weaponRef.damage;
		gameObject.SetActive(true);
	}

	//set ability projectile data
	public void Initilize(PlayerController player, SOAbilities abilityBaseRef, EntityStats casterInfo)
	{
		this.player = player;
		this.trapBaseRef = null;
		this.weaponBaseRef = null;
		this.abilityBaseRef = abilityBaseRef;
		this.casterInfo = casterInfo;
		gameObject.name = abilityBaseRef.Name + "Projectile";

		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = abilityBaseRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		isEnviromentalProjectile = false;
		isPlayerProjectile = casterInfo.IsPlayerEntity();
		projectileSpeed = abilityBaseRef.projectileSpeed;
		damageType = (DamageType)abilityBaseRef.damageType;
		int newDamage = (int)(abilityBaseRef.damageValue * Utilities.GetLevelModifier(casterInfo.entityLevel));

		if (damageType == DamageType.isPhysicalDamageType)
			projectileDamage = (int)(newDamage * casterInfo.physicalDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isPoisonDamageType)
			projectileDamage = (int)(newDamage * casterInfo.poisonDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isFireDamageType)
			projectileDamage = (int)(newDamage * casterInfo.fireDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isIceDamageType)
			projectileDamage = (int)(newDamage * casterInfo.iceDamagePercentageModifier.finalPercentageValue);

		projectileDamage *= (int)casterInfo.damageDealtModifier.finalPercentageValue;
		gameObject.SetActive(true);
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	//set projectile position, rotation and target position
	public void SetPositionAndAttackDirection(Vector3 OriginPosition, Vector3 positionOfThingToAttack)
	{
		projectileOrigin = OriginPosition;
		Vector3 rotation = positionOfThingToAttack - OriginPosition;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		transform.SetPositionAndRotation(OriginPosition, Quaternion.Euler(0, 0, rotz - 90));
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
			DungeonHandler.ProjectileCleanUp(this);

		if (other.gameObject.GetComponent<Damageable>() == null) return;

		if (!isEnviromentalProjectile && other.gameObject.layer == LayerMask.NameToLayer("Player") && isPlayerProjectile ||
			!isEnviromentalProjectile && other.gameObject.layer == LayerMask.NameToLayer("Enemies") && !isPlayerProjectile)
			return;

		if (trapBaseRef != null)	//traps
		{
			other.GetComponent<Damageable>().OnHitFromDamageSource(player, other, projectileDamage, (IDamagable.DamageType)damageType, 0,
				false, isPlayerProjectile, isEnviromentalProjectile);

			if (trapBaseRef.hasEffects && other.gameObject.GetComponent<EntityStats>() != null)
				other.gameObject.GetComponent<EntityStats>().ApplyNewStatusEffects(abilityBaseRef.statusEffects, casterInfo);
		}
		else if (abilityBaseRef != null)		//abilities
		{
			other.GetComponent<Damageable>().OnHitFromDamageSource(player, other, projectileDamage, (IDamagable.DamageType)damageType, 0,
				abilityBaseRef.isDamagePercentageBased, isPlayerProjectile, isEnviromentalProjectile);

			if (abilityBaseRef.hasStatusEffects && other.gameObject.GetComponent<EntityStats>() != null)
				other.gameObject.GetComponent<EntityStats>().ApplyNewStatusEffects(abilityBaseRef.statusEffects, casterInfo);
		}
		else     //weapon projectiles
		{
			//half ranged weapon damage
			if (distanceTraveled < weaponBaseRef.minAttackRange)
				projectileDamage /= 2;

			other.GetComponent<Damageable>().OnHitFromDamageSource(player, other, projectileDamage, (IDamagable.DamageType)damageType, 0,
				false, isPlayerProjectile, isEnviromentalProjectile);
		}
		DungeonHandler.ProjectileCleanUp(this);
	}
	private void FixedUpdate()
	{
		transform.Translate(projectileSpeed * Time.deltaTime * Vector2.up);
		if (weaponBaseRef == null) return;

		distanceTraveled = Vector2.Distance(transform.position, projectileOrigin);
		if (distanceTraveled >= weaponBaseRef.maxAttackRange)
			DungeonHandler.ProjectileCleanUp(this);
	}
}
