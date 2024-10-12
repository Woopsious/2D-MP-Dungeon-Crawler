using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class TaskIdle : BTNode
{
	EntityBehaviour behaviour;
	NavMeshAgent navMesh;

	public TaskIdle(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		navMesh = behaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		if (navMesh.remainingDistance < navMesh.stoppingDistance && behaviour.idleTimer >= 0)
		{
			Debug.Log(behaviour.name + " idle task");
			behaviour.idleTimer -= Time.deltaTime;
			return NodeState.RUNNING;
		}
		else return NodeState.FAILURE;
	}
}
