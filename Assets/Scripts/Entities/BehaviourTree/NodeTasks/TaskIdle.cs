using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class TaskIdle : BTNode
{
	EntityStats stats;
	EntityBehaviour behaviour;
	NavMeshAgent navMesh;

	public TaskIdle(EntityStats entity)
	{
		stats = entity;
		behaviour = entity.entityBehaviour;
		navMesh = entity.entityBehaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		if (navMesh.remainingDistance < navMesh.stoppingDistance && behaviour.idleTimer >= 0)
		{
			Debug.Log(stats.name + " idle task");
			behaviour.idleTimer -= Time.deltaTime;
			return NodeState.RUNNING;
		}
		else return NodeState.FAILURE;
	}
}
