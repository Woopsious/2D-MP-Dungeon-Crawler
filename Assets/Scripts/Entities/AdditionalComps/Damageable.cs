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

		if (damageSourceInfo.applyKnockback)
			ApplyHitForce(damageSourceInfo.colliderPosition, damageSourceInfo.knockBack);

		OnHit?.Invoke(damageSourceInfo, isDestroyedInOneHit);
		//Debug.Log(gameObject.name + " was hit");
	}

	private bool DamageShouldBeApplied(DamageSourceInfo damageSourceInfo)
	{
		if (invincible || damageSourceInfo.hitBye == IDamagable.HitBye.entity && !CanOtherEntitiesDamageThis) return false;
		return true;
	}
	private void ApplyHitForce(Vector3 originPosition, float knockback)
	{
		if (GetComponent<Rigidbody2D>() == null) return;

		Vector2 direction = (transform.position - originPosition).normalized;
		GetComponent<Rigidbody2D>().AddForce(100 * knockback * direction, ForceMode2D.Impulse);
	}
}

public class DamageSourceMpInfo : INetworkSerializable
{
	//death message + refs
	public DeathMessageType deathMessageType;
	public enum DeathMessageType
	{
		entityWeapon, entityAbility, trap, trapProjectile, statusEffect, enviromental
	}

	//refs for death message
	public int trapIndex;
	public int weaponIndex;
	public int abilityIndex;
	public int statusEffectIndex;

	//damage source info
	public EntityStats entity;
	public HitBye hitBye;

	//damage type
	public float damage;
	public DamageType damageType;

	//optional knockback info (shouldnt be needed as host applying knockback will change position of obj for all clients)
	public bool applyKnockback;
	public Vector3 colliderPosition;
	public float knockBack;

	//percentage damage
	public bool isPercentage;
	void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
	{
		serializer.SerializeValue(ref deathMessageType);

		serializer.SerializeValue(ref trapIndex);
		serializer.SerializeValue(ref weaponIndex);
		serializer.SerializeValue(ref abilityIndex);
		serializer.SerializeValue(ref statusEffectIndex);

		//serializer.SerializeValue(ref entity);
		serializer.SerializeValue(ref hitBye);

		serializer.SerializeValue(ref damage);
		serializer.SerializeValue(ref damageType);

		serializer.SerializeValue(ref applyKnockback);
		serializer.SerializeValue(ref colliderPosition);
		serializer.SerializeValue(ref knockBack);
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
	}
}
