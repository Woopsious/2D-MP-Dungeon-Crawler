using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CheckPlayerInAttackRange : BTNode
{
	EntityBehaviour behaviour;
	EntityEquipmentHandler equipmentHandler;

	public CheckPlayerInAttackRange(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		equipmentHandler = behaviour.equipmentHandler;
	}

	public override NodeState Evaluate()
	{
		if (PlayerWithinMaxAttackRange(equipmentHandler.equippedWeapon.weaponBaseRef))
			return NodeState.SUCCESS;
		else
			return NodeState.FAILURE;
	}

	private bool PlayerWithinMaxAttackRange(SOWeapons weaponBaseRef)
	{
		float distanceToCheck = weaponBaseRef.maxAttackRange;
		if (behaviour.entityStats.statsRef.isBossVersion)
			distanceToCheck *= 2;

		if (behaviour.distanceToPlayerTarget < distanceToCheck)
			return true;
		else
			return false;
	}
}
