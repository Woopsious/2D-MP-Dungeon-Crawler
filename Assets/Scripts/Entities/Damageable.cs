using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
	public bool isDestroyedInOneHit;

	public event Action<int, IDamagable.DamageType, bool> OnHit;

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponent<Weapons>() == null) return;

		Weapons weapon = other.gameObject.GetComponent<Weapons>();

		OnHit?.Invoke(weapon.damage, (IDamagable.DamageType)weapon.baseDamageType, isDestroyedInOneHit);
	}
}
