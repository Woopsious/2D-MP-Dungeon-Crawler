using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyIdleState : EnemyBaseState
{
	public override void Enter(EntityBehaviour entity)
	{

	}
	public override void Exit(EntityBehaviour entity)
	{

	}
	public override void UpdateLogic(EntityBehaviour entity)
	{

	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{
		IdleBehaviour(entity);
	}

	//idle behaviour
	//movement
	private void IdleBehaviour(EntityBehaviour entity)
	{
		if (!entity.currentPlayerTargetInView)
		{
			InvestigatePlayersLastKnownPos(entity);

			if (entity.CheckDistanceToDestination())
				IdleAtPositionTimer(entity);
		}
		else
			entity.ChangeState(entity.attackState);
	}
	private void InvestigatePlayersLastKnownPos(EntityBehaviour entity)
	{
		if (entity.playersLastKnownPosition != new Vector2(0, 0))
		{
			entity.SetNewDestination(entity.playersLastKnownPosition);

			if (entity.CheckDistanceToDestination())
			{
				entity.playersLastKnownPosition = new Vector2(0, 0);
				IdleAtPositionTimer(entity);
			}
		}
	}

	private void IdleAtPositionTimer(EntityBehaviour entity)
	{
		if (entity.HasReachedDestination == true)
		{
			entity.idleTimer -= Time.deltaTime;

			if (entity.idleTimer < 0)
			{
				entity.idleTimer = entity.entityBehaviour.idleWaitTime;
				FindNewIdlePosition(entity);
			}
		}
	}
	private void FindNewIdlePosition(EntityBehaviour entity)
	{
		entity.HasReachedDestination = false;

		Vector2 randomMovePosition = Utilities.GetRandomPointInBounds(entity.idleBounds);
		NavMeshPath path = new NavMeshPath();

		if (entity.navMeshAgent.CalculatePath(randomMovePosition, path) && path.status == NavMeshPathStatus.PathComplete)
			entity.navMeshAgent.SetPath(path);
		else
			FindNewIdlePosition(entity);
	}
}
