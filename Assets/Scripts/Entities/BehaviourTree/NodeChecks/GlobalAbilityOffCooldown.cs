using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GlobalAbilityOffCooldown : BTNode
{
	EntityStats stats;
	EntityBehaviour behaviour;
	EntityEquipmentHandler equipmentHandler;
	NavMeshAgent navMesh;


	public GlobalAbilityOffCooldown(EntityStats entity)
	{
		stats = entity;
		behaviour = entity.entityBehaviour;
		equipmentHandler = entity.equipmentHandler;
		navMesh = entity.entityBehaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		Debug.Log(stats.name + " weapon attack task");
		return state = NodeState.RUNNING;
	}
}
