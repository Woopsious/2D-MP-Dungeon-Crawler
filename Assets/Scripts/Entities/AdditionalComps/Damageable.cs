using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		if (invincible || damageSourceInfo.hitBye == DamageSourceInfo.HitBye.entity && !CanOtherEntitiesDamageThis) return false;
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
	public PlayerController player;
	public Collider2D collider;

	public float damage;
	public IDamagable.DamageType damageType;

	public float knockBack;

	public bool isPercentage;
	public bool wasHitByPlayer;
	public bool wasEnviroment;

	public HitBye hitBye;
	public enum HitBye
	{
		player, entity, enviroment
	}

	public DamageSourceInfo(PlayerController player, Collider2D collider, float damage, IDamagable.DamageType damageType, 
		float knockBack, bool isPercentage, bool wasHitByPlayer, bool wasEnviroment)
	{
		this.player = player;
		this.collider = collider;

		this.damage = damage;
		this.damageType = damageType;

		this.knockBack = knockBack;

		this.isPercentage = isPercentage;
		this.wasHitByPlayer = wasHitByPlayer;
		this.wasEnviroment = wasEnviroment;
	}

	//applies all
	public DamageSourceInfo(HitBye hitBye, Collider2D collider, float damage, IDamagable.DamageType damageType,
	float knockBack, bool isPercentage)
	{
		this.hitBye = hitBye;
		this.collider = collider;

		this.damage = damage;
		this.damageType = damageType;
		this.knockBack = knockBack;
		this.isPercentage = isPercentage;
	}

	//no knockback
	public DamageSourceInfo(HitBye hitBye, float damage, IDamagable.DamageType damageType, bool isPercentage)
	{
		this.hitBye = hitBye;

		this.damage = damage;
		this.damageType = damageType;
		this.isPercentage = isPercentage;
	}
}
