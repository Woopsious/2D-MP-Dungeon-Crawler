using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
	public bool DebugInvincible;
	public bool isDestroyedInOneHit;
	private bool CanOtherEntitiesDamageThis;

	public event Action<PlayerController, float, IDamagable.DamageType, bool, bool> OnHit;

	private void Start()
	{
		if (GetComponent<PlayerController>() != null)
			CanOtherEntitiesDamageThis = true;
		else
			CanOtherEntitiesDamageThis = false;
	}

	public void OnHitFromDamageSource(PlayerController player, Collider2D other, float damage, IDamagable.DamageType damageType, float knockBack,
		bool isPercentageValue, bool wasHitByPlayer, bool wasEnviroment)
	{
		if (DebugInvincible) return;
		if (!wasHitByPlayer && !CanOtherEntitiesDamageThis && !wasEnviroment) return;

		ApplyHitForce(other, knockBack);
		OnHit?.Invoke(player, damage, damageType, isPercentageValue, isDestroyedInOneHit);
		//Debug.Log(gameObject.name + " was hit");
	}
	public void ApplyHitForce(Collider2D other, float knockback)
	{
		if (GetComponent<Rigidbody2D>() == null || other == null) return;

		Vector2 direction = (transform.position - other.transform.position).normalized;
		GetComponent<Rigidbody2D>().AddForce(100 * knockback * direction, ForceMode2D.Impulse);
	}
}
