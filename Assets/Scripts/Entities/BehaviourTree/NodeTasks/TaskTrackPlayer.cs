using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class TaskTrackPlayer : EntityMovement
{
	EntityBehaviour behaviour;
	EntityEquipmentHandler equipmentHandler;

	public TaskTrackPlayer(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		equipmentHandler = behaviour.equipmentHandler;
	}

	public override NodeState Evaluate()
	{
		Debug.Log(behaviour.name + " track player task");

		if (equipmentHandler.equippedWeapon.weaponBaseRef.isRangedWeapon)
			KeepDistanceFromPlayer(behaviour, equipmentHandler);
		else
			KeepPlayerInMeleeRange(behaviour, equipmentHandler);

		return NodeState.RUNNING;
	}
}
