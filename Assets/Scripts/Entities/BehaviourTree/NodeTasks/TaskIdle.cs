using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskIdle : BTNode
{
	EntityStats entity;
	EntityBehaviour entityBehaviour;
	NavMeshAgent navMesh;

	public TaskIdle(EntityStats entity)
	{
		this.entity = entity;
		entityBehaviour = entity.entityBehaviour;
		navMesh = entity.entityBehaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		Debug.Log(entity.name + " idle task");

		if (navMesh.remainingDistance < navMesh.stoppingDistance && entityBehaviour.idleTimer >= 0)
		{
			Debug.Log(entity.name + " timer ticking");
			entityBehaviour.idleTimer -= Time.deltaTime;
			return state = NodeState.RUNNING;
		}
		else return state = NodeState.FAILURE;
	}
}
