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
		if (!abilityAOE.IsCollidedObjEnemy(other)) return;

		abilityAOE.OnEntityEnter2D(other.GetComponent<EntityStats>());
	}
}
