using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
		if (!entity.CheckIfPlayerVisible())
		{
			if (entity.CheckDistanceToDestination())
				IdleAtPositionTimer(entity);

			if (entity.CheckDistanceToPlayer()) //player outside of max chase range
				entity.player = null;
		}
		else if (entity.CheckIfPlayerVisible())
			entity.ChangeStateAttack();
	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{

	}

	//idle behaviour
	public void IdleAtPositionTimer(EntityBehaviour entity)
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
	public void FindNewIdlePosition(EntityBehaviour entity)
	{
		Vector2 randomMovePosition = Utilities.GetRandomPointInBounds(entity.idleBounds);
		entity.movePosition = entity.SampleNewMovePosition(randomMovePosition);

		Debug.Log("random move position: " + entity.movePosition);
		Debug.Log("move position: " + entity.movePosition);

		if (Double.IsInfinity(entity.movePosition.x)) return;

		if (entity.CheckAndSetNewPath(entity.movePosition)) //occasionally throws invalid target position (infinity, infinity, 0.0000)
			return;
		else
			FindNewIdlePosition(entity);
	}
}
