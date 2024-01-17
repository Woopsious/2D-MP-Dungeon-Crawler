using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
	public bool isDestroyedInOneHit;
	private bool CanOtherEntitiesDamageThis;

	public event Action<int, IDamagable.DamageType, bool> OnHit;

	private void Start()
	{
		if (GetComponent<PlayerController>() != null)
			CanOtherEntitiesDamageThis = true;
		else
			CanOtherEntitiesDamageThis = false;
	}

	public void OnHitFromDamageSource(int damage, IDamagable.DamageType damageType, bool wasHitByPlayer)
	{
		if (!wasHitByPlayer && !CanOtherEntitiesDamageThis) return;

		OnHit?.Invoke(damage, damageType, isDestroyedInOneHit);
	}
}
