using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskInvestigate : BTNode
{
	EntityStats entity;
	EntityBehaviour entityBehaviour;
	NavMeshAgent navMesh;

	public TaskInvestigate(EntityStats entity)
	{
		this.entity = entity;
		entityBehaviour = entity.entityBehaviour;
		navMesh = entity.entityBehaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		Debug.Log(entity.name + " investigate task");

		if (navMesh.remainingDistance < navMesh.stoppingDistance && entityBehaviour.playersLastKnownPosition != new Vector2(0,0))
		{
			Debug.Log(entity.name + " investigate player last pos");
			entityBehaviour.SetNewDestination(entityBehaviour.playersLastKnownPosition); //move to last know player target pos
		}

		if (navMesh.remainingDistance > navMesh.stoppingDistance)
			return state = NodeState.RUNNING;
		else
		{
			entityBehaviour.playersLastKnownPosition = new Vector2(0, 0);
			return state = NodeState.FAILURE;
		}
	}
}
