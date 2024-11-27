using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEditor.Playables;
using UnityEditor;
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
	private float projectileSpeed;
	public int projectileDamage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}
	private IDamagable.HitBye ownedBye;
	private bool isPercentageDamage;

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
		projectileSpeed = trapBaseRef.projectileSpeed;
		projectileDamage = trapDamage;
		damageType = (DamageType)trapBaseRef.baseDamageType;
		ownedBye = IDamagable.HitBye.enviroment;
		isPercentageDamage = false;
		gameObject.SetActive(true);
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
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

		projectileSpeed = abilityBaseRef.projectileSpeed;
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
		damageType = (DamageType)abilityBaseRef.damageType;
		isPercentageDamage = abilityBaseRef.isDamagePercentageBased;
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

		projectileSpeed = weaponBaseRef.projectileSpeed;
		projectileDamage = weaponRef.damage;
		damageType = (DamageType)weaponBaseRef.baseDamageType;
		isPercentageDamage = false;
		gameObject.SetActive(true);
	}

	//optional, helps with applying damage only to enemies
	public void AddPlayerRef(PlayerController player)
	{
		this.player = player;

		if (player != null)
			ownedBye = IDamagable.HitBye.player;
		else
			ownedBye = IDamagable.HitBye.entity;
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

		if (ownedBye == IDamagable.HitBye.player && other.gameObject.layer == LayerMask.NameToLayer("Player") ||
			ownedBye == IDamagable.HitBye.entity && other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
			return;

		DamageSourceInfo damageSourceInfo = new(player, ownedBye, other, 
			projectileDamage, (IDamagable.DamageType)damageType, 0, isPercentageDamage);

		if (trapBaseRef != null)    //traps
		{
			other.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);

			if (trapBaseRef.hasEffects && other.gameObject.GetComponent<EntityStats>() != null)
				other.gameObject.GetComponent<EntityStats>().ApplyNewStatusEffects(abilityBaseRef.statusEffects, casterInfo);
		}
		else if (abilityBaseRef != null)//abilities
		{
			other.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);

			if (abilityBaseRef.hasStatusEffects && other.gameObject.GetComponent<EntityStats>() != null)
				other.gameObject.GetComponent<EntityStats>().ApplyNewStatusEffects(abilityBaseRef.statusEffects, casterInfo);
		}
		else     //weapon projectiles
		{
			//half ranged weapon damage
			if (distanceTraveled < weaponBaseRef.minAttackRange)
				projectileDamage /= 2;

			damageSourceInfo.collider = other;
			damageSourceInfo.knockBack = weaponBaseRef.baseKnockback;
			other.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);
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
