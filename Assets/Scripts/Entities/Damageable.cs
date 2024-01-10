using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
	public bool isDestroyedInOneHit;
	public bool CanOtherEntitiesDamageThis;

	public event Action<int, IDamagable.DamageType, bool> OnHit;

	public void OnHitFromDamageSource(int damage, IDamagable.DamageType damageType, bool wasHitByPlayer)
	{
		if (!wasHitByPlayer && !CanOtherEntitiesDamageThis) return;

		OnHit?.Invoke(damage, damageType, isDestroyedInOneHit);
	}
}
