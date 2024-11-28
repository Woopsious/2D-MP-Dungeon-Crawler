using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IDamagable;

public class Damageable : MonoBehaviour
{
	public bool invincible;
	public bool isDestroyedInOneHit;
	private bool CanOtherEntitiesDamageThis;

	//public event Action<PlayerController, float, IDamagable.DamageType, bool, bool> OnHit;
	public event Action<DamageSourceInfo, bool> OnHit;

	private void Awake()
	{
		if (GetComponent<PlayerController>() != null)
			CanOtherEntitiesDamageThis = true;
		else
			CanOtherEntitiesDamageThis = false;
	}

	public void OnHitFromDamageSource(DamageSourceInfo damageSourceInfo)
	{
		if (!DamageShouldBeApplied(damageSourceInfo)) return;

		if (damageSourceInfo.collider != null)
			ApplyHitForce(damageSourceInfo.collider, damageSourceInfo.knockBack);

		OnHit?.Invoke(damageSourceInfo, isDestroyedInOneHit);
		//Debug.Log(gameObject.name + " was hit");
	}

	private bool DamageShouldBeApplied(DamageSourceInfo damageSourceInfo)
	{
		if (invincible || damageSourceInfo.hitBye == IDamagable.HitBye.entity && !CanOtherEntitiesDamageThis) return false;
		return true;
	}
	private void ApplyHitForce(Collider2D other, float knockback)
	{
		if (GetComponent<Rigidbody2D>() == null || other == null) return;

		Vector2 direction = (transform.position - other.transform.position).normalized;
		GetComponent<Rigidbody2D>().AddForce(100 * knockback * direction, ForceMode2D.Impulse);
	}
}

public class DamageSourceInfo
{
	//death message + refs
	public DeathMessageType deathMessageType;
	public enum DeathMessageType
	{
		entityWeapon, entityAbility, trap, trapProjectile, statusEffect, enviromental
	}

	//damage source info
	public EntityStats entity;
	public HitBye hitBye;

	//refs for death message
	public SOTraps trap;
	public SOWeapons weapon;
	public SOAbilities ability;
	public SOStatusEffects statusEffect;

	//damage type
	public float damage;
	public DamageType damageType;

	//optional knockback info
	public Collider2D collider;
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
	}

	public void AddKnockbackEffect(Collider2D collider, float knockBack)
	{
		this.collider = collider;
		this.knockBack = knockBack;
	}

	public void SetDeathMessage<A>(A thingDealingDamage)
	{
		if (entity != null)
		{
			if (typeof(A).Equals(typeof(SOWeapons)))
			{
				weapon = thingDealingDamage as SOWeapons;
				deathMessageType = DeathMessageType.entityWeapon;
			}
			else if (typeof(A).Equals(typeof(SOAbilities)))
			{
				ability = thingDealingDamage as SOAbilities;
				deathMessageType = DeathMessageType.entityAbility;
			}
		}
		else
		{
			if (typeof(A).Equals(typeof(SOTraps)))
			{
				trap = thingDealingDamage as SOTraps;
				deathMessageType = DeathMessageType.trap;
			}
			else if (typeof(A).Equals(typeof(SOStatusEffects)))
			{
				statusEffect = thingDealingDamage as SOStatusEffects;
				deathMessageType = DeathMessageType.statusEffect;
			}
		}
	}
}
