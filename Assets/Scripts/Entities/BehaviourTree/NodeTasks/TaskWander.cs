using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class TaskWander : BTNode
{
	EntityBehaviour behaviour;
	NavMeshAgent navMesh;

	public TaskWander(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		navMesh = behaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		if (behaviour.playerTarget != null && behaviour.playersLastKnownPosition != new Vector2(0, 0))
			return NodeState.FAILURE;

		if (navMesh.remainingDistance < navMesh.stoppingDistance && behaviour.idleTimer < 0)
		{
			behaviour.idleTimer = behaviour.behaviourRef.idleWaitTime;
			FindNewIdlePosition();
		}

		if (navMesh.remainingDistance > navMesh.stoppingDistance)
		{
			Debug.Log(behaviour.name + " wander task");
			return NodeState.RUNNING;
		}
		else return NodeState.FAILURE;
	}

	//Wander behaviour
	private void FindNewIdlePosition()
	{
		Vector2 randomMovePosition = Utilities.GetRandomPointInBounds(behaviour.idleBounds);
		NavMeshPath path = new NavMeshPath();

		if (navMesh.CalculatePath(randomMovePosition, path) && path.status == NavMeshPathStatus.PathComplete)
			navMesh.SetPath(path);
		else
			FindNewIdlePosition();
	}
}
