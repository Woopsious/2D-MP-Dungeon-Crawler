using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Projectiles : NetworkBehaviour
{
	public SOTraps trapBaseRef;
	public SOWeapons weaponBaseRef;
	public SOAbilities abilityBaseRef;
	public EntityStats projectileOwner;	//only set for abilities

	private BoxCollider2D boxCollider;
	private SpriteRenderer projectileSprite;
	private float projectileSpeed;
	public int projectileDamage;
	private DamageType damageType;
	enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}
	private IDamagable.HitBye hitBye;
	private bool isPercentageDamage;

	Vector2 projectileOrigin;
	float distanceTraveled;

	//set trap projectile data
	public void Initilize(SOTraps trap, int trapDamage)
	{
		trapBaseRef = trap;
		weaponBaseRef = null;
		abilityBaseRef = null;
		gameObject.name = trapBaseRef.name + "Projectile";

		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = trapBaseRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		projectileSpeed = trapBaseRef.projectileSpeed;
		projectileDamage = trapDamage;
		damageType = (DamageType)trapBaseRef.baseDamageType;
		UpdateHitByeVariable(null);
		isPercentageDamage = false;

		if (MultiplayerManager.IsMultiplayer())
			EnableObjectRpc();
		else
			EnableObject();
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	//set ability projectile data
	public void Initilize(EntityStats projectileOwner, SOAbilities abilityBaseRef)
	{
		trapBaseRef = null;
		weaponBaseRef = null;
		this.abilityBaseRef = abilityBaseRef;
		this.projectileOwner = projectileOwner;
		gameObject.name = abilityBaseRef.Name + "Projectile";

		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = abilityBaseRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		projectileSpeed = abilityBaseRef.projectileSpeed;
		int newDamage = (int)(abilityBaseRef.damageValue * Utilities.GetLevelModifier(projectileOwner.entityLevel));

		if (damageType == DamageType.isPhysicalDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.physicalDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isPoisonDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.poisonDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isFireDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.fireDamagePercentageModifier.finalPercentageValue);
		if (damageType == DamageType.isIceDamageType)
			projectileDamage = (int)(newDamage * projectileOwner.iceDamagePercentageModifier.finalPercentageValue);

		projectileDamage *= (int)projectileOwner.damageDealtModifier.finalPercentageValue;
		damageType = (DamageType)abilityBaseRef.damageType;
		UpdateHitByeVariable(projectileOwner.playerRef);
		isPercentageDamage = abilityBaseRef.isDamagePercentageBased;

		if (MultiplayerManager.IsMultiplayer())
			EnableObjectRpc();
		else
			EnableObject();
		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	//set weapon projectile data
	public void Initilize(EntityStats projectileOwner, SOWeapons weaponBaseRef, int projectileDamage)
	{
		trapBaseRef = null;
		this.weaponBaseRef = weaponBaseRef;
		abilityBaseRef = null;
		this.projectileOwner = projectileOwner;
		gameObject.name = weaponBaseRef.itemName + "Projectile";

		boxCollider = GetComponent<BoxCollider2D>();
		projectileSprite = GetComponent<SpriteRenderer>();
		projectileSprite.sprite = weaponBaseRef.projectileSprite;
		boxCollider.size = projectileSprite.size;
		boxCollider.offset = new Vector2(0, 0);

		projectileSpeed = weaponBaseRef.projectileSpeed;
		this.projectileDamage = projectileDamage;
		damageType = (DamageType)weaponBaseRef.baseDamageType;
		UpdateHitByeVariable(projectileOwner.playerRef);
		isPercentageDamage = false;

		if (MultiplayerManager.IsMultiplayer())
			EnableObjectRpc();
		else
			EnableObject();
	}

	//helps with applying damage only to enemies
	private void UpdateHitByeVariable(PlayerController player)
	{
		if (player != null)
			hitBye = IDamagable.HitBye.player;
		else
			hitBye = IDamagable.HitBye.entity;

		if (trapBaseRef != null) //if ref not null overwrite hitbye
			hitBye = IDamagable.HitBye.enviroment;
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
		if (!MultiplayerManager.IsClientHost()) return;

		if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
		{
			if (MultiplayerManager.IsMultiplayer())
				DisableObjectRpc();
			else
				DisableObject();
		}

		if (other.gameObject.GetComponent<Damageable>() == null) return;

		if (hitBye == IDamagable.HitBye.player && other.gameObject.layer == LayerMask.NameToLayer("Player") ||
			hitBye == IDamagable.HitBye.entity && other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
			return;

		DamageSourceInfo damageSourceInfo = new(
			projectileOwner, hitBye, projectileDamage, (IDamagable.DamageType)damageType, isPercentageDamage);

		if (trapBaseRef != null)    //traps
		{
			damageSourceInfo.SetDeathMessage(trapBaseRef);
			other.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);

			if (trapBaseRef.hasEffects && other.gameObject.GetComponent<EntityStats>() != null)
				other.gameObject.GetComponent<EntityStats>().ApplyNewStatusEffects(abilityBaseRef.statusEffects, projectileOwner);
		}
		else if (abilityBaseRef != null)//abilities
		{
			damageSourceInfo.SetDeathMessage(abilityBaseRef);
			other.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);

			if (abilityBaseRef.hasStatusEffects && other.gameObject.GetComponent<EntityStats>() != null)
				other.gameObject.GetComponent<EntityStats>().ApplyNewStatusEffects(abilityBaseRef.statusEffects, projectileOwner);
		}
		else     //weapon projectiles
		{
			//half ranged weapon damage
			if (distanceTraveled < weaponBaseRef.minAttackRange)
				projectileDamage /= 2;

			damageSourceInfo.AddKnockbackEffect(boxCollider, weaponBaseRef.baseKnockback);
			damageSourceInfo.SetDeathMessage(weaponBaseRef);
			other.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);
		}

		if (MultiplayerManager.IsMultiplayer())
			DisableObjectRpc();
		else
			DisableObject();
	}
	private void FixedUpdate()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		transform.Translate(projectileSpeed * Time.deltaTime * Vector2.up);
		if (weaponBaseRef == null) return;

		distanceTraveled = Vector2.Distance(transform.position, projectileOrigin);
		if (distanceTraveled >= weaponBaseRef.maxAttackRange)
		{
			if (MultiplayerManager.IsMultiplayer())
				DisableObjectRpc();
			else
				DisableObject();
		}
	}

	[Rpc(SendTo.Everyone)]
	private void EnableObjectRpc()
	{
		EnableObject();
	}
	private void EnableObject()
	{
		gameObject.SetActive(true);
	}

	[Rpc(SendTo.Everyone)]
	private void DisableObjectRpc()
	{
		DisableObject();
	}
	private void DisableObject()
	{
		gameObject.SetActive(false);
		transform.position = Vector3.zero;
		ObjectPoolingManager.AddProjectileToInActivePool(this);
	}
}
