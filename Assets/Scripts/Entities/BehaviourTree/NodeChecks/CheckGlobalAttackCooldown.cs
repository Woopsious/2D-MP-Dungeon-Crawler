using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CheckGlobalAttackCooldown : BTNode
{
	EntityStats stats;
	EntityBehaviour behaviour;
	EntityEquipmentHandler equipmentHandler;

	public CheckGlobalAttackCooldown(EntityStats entity)
	{
		stats = entity;
		behaviour = entity.entityBehaviour;
		equipmentHandler = entity.equipmentHandler;
	}

	public override NodeState Evaluate()
	{
		if (behaviour.globalAttackTimer >= 0)
			return NodeState.RUNNING;
		else return NodeState.SUCCESS;
	}
}
