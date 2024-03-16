using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
	public bool DebugInvincible;
	public bool isDestroyedInOneHit;
	private bool CanOtherEntitiesDamageThis;

	public event Action<float, IDamagable.DamageType, bool, bool> OnHit;

	private void Start()
	{
		if (GetComponent<PlayerController>() != null)
			CanOtherEntitiesDamageThis = true;
		else
			CanOtherEntitiesDamageThis = false;
	}

	public void OnHitFromDamageSource(Collider2D other, float damage, IDamagable.DamageType damageType, float knockBack,
		bool isPercentageValue, bool wasHitByPlayer)
	{
		if (DebugInvincible) return;
		if (!wasHitByPlayer && !CanOtherEntitiesDamageThis) return;

		ApplyHitForce(other, knockBack);
		OnHit?.Invoke(damage, damageType, isPercentageValue, isDestroyedInOneHit);
	}
	public void ApplyHitForce(Collider2D other, float knockback)
	{
		if (GetComponent<Rigidbody2D>() == null || other == null) return;

		Vector2 difference = other.transform.position + gameObject.transform.position;
		difference = difference.normalized * knockback * 100;
		GetComponent<Rigidbody2D>().AddForce(difference, ForceMode2D.Impulse);
	}
}
