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
		if (WithinWeaponAttackRange(equipmentHandler.equippedWeapon.weaponBaseRef))
			return NodeState.SUCCESS;
		else
			return NodeState.FAILURE;
	}

	private bool WithinWeaponAttackRange(SOWeapons weaponBaseRef)
	{
		float maxDistanceToCheck = weaponBaseRef.maxAttackRange;
		if (!weaponBaseRef.isRangedWeapon && behaviour.entityStats.statsRef.isBossVersion)
				maxDistanceToCheck = weaponBaseRef.maxAttackRange * 2;

		if (behaviour.distanceToPlayerTarget <= maxDistanceToCheck)
			return true;
		else return false;
	}
}
