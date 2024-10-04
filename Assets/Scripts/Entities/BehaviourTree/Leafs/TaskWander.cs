using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskWander : BTNode
{
	EntityStats entity;
	EntityBehaviour entityBehaviour;
	NavMeshAgent navMesh;

	public TaskWander(EntityStats entity)
	{
		this.entity = entity;
		entityBehaviour = entity.entityBehaviour;
		navMesh = entity.entityBehaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		Debug.Log(entity.name + " wander task");

		if (navMesh.remainingDistance < navMesh.stoppingDistance && entityBehaviour.idleTimer <= 0)
		{
			Debug.Log(entity.name + " get path");
			entityBehaviour.idleTimer = entityBehaviour.entityBehaviour.idleWaitTime;
			FindNewIdlePosition(entityBehaviour);
		}

		if (navMesh.remainingDistance > navMesh.stoppingDistance)
			return state = NodeState.RUNNING;
		else return state = NodeState.FAILURE;
	}

	//Wander behaviour
	private void WanderBehaviour(EntityBehaviour entity)
	{
		if (entity.CurrentPlayerTargetVisible())
		{
			if (entity.entityStats.entityBaseStats.isBossVersion)
			{
				BossEntityBehaviour bossEntity = (BossEntityBehaviour)entity;
				bossEntity.ChangeState(bossEntity.goblinAttackState);
			}
			else
				entity.ChangeState(entity.attackState);
		}
		else
		{
			InvestigatePlayersLastKnownPos(entity);

			if (entity.HasReachedDestination())
				entity.ChangeState(entity.idleState);
		}
	}
	private void InvestigatePlayersLastKnownPos(EntityBehaviour entity)
	{
		if (entity.playersLastKnownPosition == new Vector2(0, 0)) return;

		entity.SetNewDestination(entity.playersLastKnownPosition);

		if (entity.HasReachedDestination())
		{
			entity.playersLastKnownPosition = new Vector2(0, 0);
			entity.ChangeState(entity.idleState);
		}
	}
	private void FindNewIdlePosition(EntityBehaviour entity)
	{
		Vector2 randomMovePosition = Utilities.GetRandomPointInBounds(entity.idleBounds);
		NavMeshPath path = new NavMeshPath();

		if (entity.navMeshAgent.CalculatePath(randomMovePosition, path) && path.status == NavMeshPathStatus.PathComplete)
			entity.navMeshAgent.SetPath(path);
		else
			FindNewIdlePosition(entity);
	}
}
