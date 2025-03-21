using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static IDamagable;

public class Damageable : MonoBehaviour
{

	public bool invincible;
	public bool isDestroyedInOneHit;
	private bool isPlayer;
	private bool CanOtherEntitiesDamageThis;

	//public event Action<PlayerController, float, IDamagable.DamageType, bool, bool> OnHit;
	public event Action<DamageSourceInfo, bool> OnHit;

	private void Awake()
	{
		if (GetComponent<PlayerController>() != null)
			CanOtherEntitiesDamageThis = true;
		else
			CanOtherEntitiesDamageThis = false;

		isPlayer = false;
		if (GetComponent<PlayerController>() != null)
			isPlayer = true;
	}

	public void OnHitFromDamageSource(DamageSourceInfo damageSourceInfo)
	{
		if (!DamageShouldBeApplied(damageSourceInfo)) return;

		if (damageSourceInfo.applyKnockback)
			ApplyHitForce(damageSourceInfo.colliderPosition, damageSourceInfo.knockBack);

		OnHit?.Invoke(damageSourceInfo, isDestroyedInOneHit);
		//Debug.Log(gameObject.name + " was hit");
	}

	private bool DamageShouldBeApplied(DamageSourceInfo damageSourceInfo)
	{
		if (invincible || isPlayer && damageSourceInfo.hitBye == HitBye.player) return false;
		if (damageSourceInfo.hitBye == HitBye.entity && !CanOtherEntitiesDamageThis) return false;
		return true;
	}
	private void ApplyHitForce(Vector3 originPosition, float knockback)
	{
		if (GetComponent<Rigidbody2D>() == null) return;

		Vector2 direction = (transform.position - originPosition).normalized;
		GetComponent<Rigidbody2D>().AddForce(100 * knockback * direction, ForceMode2D.Impulse);
	}
}

public class DamageSourceInfo
{
	//death message + refs
	public string deathMessage;
	public DeathMessageType deathMessageType;
	public enum DeathMessageType
	{
		entityWeapon, entityAbility, trap, trapProjectile, statusEffect, enviromental, debug
	}

	//refs for death message
	public SOTraps trap;
	public SOWeapons weapon;
	public SOAbilities ability;
	public SOStatusEffects statusEffect;

	//damage source info
	public EntityStats entity;
	public HitBye hitBye;

	//damage type
	public float damage;
	public DamageType damageType;

	//optional knockback info
	public bool applyKnockback;
	public Vector3 colliderPosition;
	public float knockBack;

	//percentage damage
	public bool isPercentage;

	//basic constructors
	public DamageSourceInfo(EntityStats entity, HitBye hitBye, float damage, DamageType damageType, bool isPercentage)
	{
		this.entity = entity;
		this.hitBye = hitBye;

		this.damage = damage;
		this.damageType = damageType;
		this.isPercentage = isPercentage;
		this.applyKnockback = false;
	}

	public void AddKnockbackEffect(Vector3 colliderPosition, float knockBack)
	{
		this.applyKnockback = true;
		this.colliderPosition = colliderPosition;
		this.knockBack = knockBack;
	}

	//set and generate player death message
	public void SetDeathMessage<T>(T thingDealingDamage)
	{
		if (entity != null)
		{
			if (typeof(T).Equals(typeof(SOWeapons)))
			{
				weapon = thingDealingDamage as SOWeapons;
				deathMessageType = DeathMessageType.entityWeapon;
			}
			else if (typeof(T).Equals(typeof(SOAbilities)))
			{
				ability = thingDealingDamage as SOAbilities;
				deathMessageType = DeathMessageType.entityAbility;
			}
		}
		else
		{
			if (typeof(T).Equals(typeof(SOTraps)))
			{
				trap = thingDealingDamage as SOTraps;
				deathMessageType = DeathMessageType.trap;
			}
			else if (typeof(T).Equals(typeof(SOStatusEffects)))
			{
				statusEffect = thingDealingDamage as SOStatusEffects;
				deathMessageType = DeathMessageType.statusEffect;
			}
		}

		deathMessage = GetPlayerDeathMessage();
	}
	public void SetDebugDeathMessage()
	{
		deathMessageType = DeathMessageType.debug;
		deathMessage = $"Debug kill local player";
	}
	private string GetPlayerDeathMessage()
	{
		string deathMessage = string.Empty;

		if (deathMessageType == DeathMessageType.entityWeapon)
		{
			if (weapon.isRangedWeapon)
				deathMessage = $"Died by arrow from {entity.statsRef.entityName}'s {weapon.itemName}";
			else
				deathMessage = $"Died from swing of {entity.statsRef.entityName}'s {weapon.itemName}";
		}
		else if (deathMessageType == DeathMessageType.entityAbility)
		{
			deathMessage = $"Died from {entity.statsRef.entityName}'s {ability.Name} ability";
		}
		else if (deathMessageType == DeathMessageType.trap)
		{
			deathMessage = $"Died from {trap.trapName}";
		}
		else if (deathMessageType == DeathMessageType.statusEffect)
		{
			if (statusEffect.damageType == DamageType.isPhysicalDamage)
				deathMessage = $"Died from bleeding out";
			else if (statusEffect.damageType == DamageType.isPoisonDamage)
				deathMessage = $"Died from being poisoned ";
			else if (statusEffect.damageType == DamageType.isFireDamage)
				deathMessage = $"Died from burning to death";
			else if (statusEffect.damageType == DamageType.isIceDamage)
				deathMessage = $"Died from Freezing internally";
		}

		return deathMessage;
	}
}
