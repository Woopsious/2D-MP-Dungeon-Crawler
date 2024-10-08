using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskUseAbility : BTNode
{
	EntityStats stats;
	EntityBehaviour behaviour;
	EntityEquipmentHandler equipmentHandler;
	NavMeshAgent navMesh;


	public TaskUseAbility(EntityStats entity)
	{
		stats = entity;
		behaviour = entity.entityBehaviour;
		equipmentHandler = entity.equipmentHandler;
		navMesh = entity.entityBehaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		Debug.Log(stats.name + " use ability task");

		//return failure to force switch back to attack with main weapon
		if (!behaviour.canCastOffensiveAbility || behaviour.globalAttackTimer > 0) return NodeState.FAILURE;

		Debug.LogError(stats.name + " using ability");
		behaviour.TryCastOffensiveAbility();

		//add ability animation length here if needed, include a bool if animation should block movement
		behaviour.globalAttackTimer = 1f;
		return NodeState.SUCCESS;
	}
}
