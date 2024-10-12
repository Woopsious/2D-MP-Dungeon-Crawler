using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class TaskInvestigate : BTNode
{
	EntityBehaviour behaviour;
	NavMeshAgent navMesh;

	public TaskInvestigate(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		navMesh = behaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		if (behaviour.playerTarget == null && behaviour.playersLastKnownPosition != new Vector2(0, 0))
		{
			if (!navMesh.hasPath && navMesh.remainingDistance > navMesh.stoppingDistance)
			{
				Debug.LogError(behaviour.name + " investigating player pos");
				behaviour.SetNewDestination(behaviour.playersLastKnownPosition); //move to last know player target pos
			}

			if (behaviour.playerTarget == null && navMesh.remainingDistance > navMesh.stoppingDistance)
			{
				Debug.Log(behaviour.name + " investigate task");
				return NodeState.RUNNING;
			}
			else
			{
				behaviour.playersLastKnownPosition = new Vector2(0, 0);
				return NodeState.FAILURE;
			}
		}
		else return NodeState.FAILURE;
	}
}
