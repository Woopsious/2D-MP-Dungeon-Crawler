using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class AbilityAoeCollider : MonoBehaviour
{
	AbilityAOE abilityAOE;

	private void Start()
	{
		abilityAOE = GetComponentInParent<AbilityAOE>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.GetComponent<EntityStats>() == null) return;
		abilityAOE.ApplyDamageToEntitiesInAoe(other);
	}
}
