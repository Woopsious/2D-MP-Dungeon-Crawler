using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskMoveInRangeForAbility : EntityMovement
{
	EntityBehaviour behaviour;
	EntityEquipmentHandler equipmentHandler;

	public TaskMoveInRangeForAbility(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		equipmentHandler = behaviour.equipmentHandler;
	}

	public override NodeState Evaluate()
	{
		//Debug.Log(behaviour.name + " move player in range for ability");

		if (equipmentHandler.equippedWeapon.weaponBaseRef.isRangedWeapon)
			KeepDistanceFromPlayer(behaviour, equipmentHandler);
		else
			KeepPlayerInMeleeRange(behaviour, equipmentHandler);

		return NodeState.RUNNING;
	}
}
