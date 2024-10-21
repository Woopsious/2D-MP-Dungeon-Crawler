using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class EnemyWanderState : EnemyBaseState
{
	public override void Enter(EntityBehaviour entity)
	{
		FindNewIdlePosition(entity);
	}
	public override void Exit(EntityBehaviour entity)
	{

	}
	public override void UpdateLogic(EntityBehaviour entity)
	{
		WanderBehaviour(entity);
	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{

	}

	//Wander behaviour
	private void WanderBehaviour(EntityBehaviour entity)
	{
		if (entity.CurrentPlayerTargetVisible())
		{
			if (entity.entityStats.statsRef.isBossVersion)
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
