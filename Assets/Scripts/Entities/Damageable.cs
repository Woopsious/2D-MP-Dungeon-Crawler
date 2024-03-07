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

	public void OnHitFromDamageSource(float damage, IDamagable.DamageType damageType, bool isPercentageValue, bool wasHitByPlayer)
	{
		if (DebugInvincible) return;
		if (!wasHitByPlayer && !CanOtherEntitiesDamageThis) return;

		OnHit?.Invoke(damage, damageType, isPercentageValue, isDestroyedInOneHit);
	}
}
