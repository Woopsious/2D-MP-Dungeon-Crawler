using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EnemyAttackState : EnemyBaseState
{
	public override void Enter(EntityBehaviour entity)
	{

	}
	public override void Exit(EntityBehaviour entity)
	{

	}
	public override void UpdateLogic(EntityBehaviour entity)
	{
		UpdatePlayerPosition(entity);

		if (!entity.CheckIfPlayerVisible())
			entity.ChangeStateIdle();
		else if (entity.CheckIfPlayerVisible())
		{
			if (entity.CheckDistanceToPlayer()) //player outside of max chase range
			{
				entity.player = null;
				entity.ChangeStateIdle();
			}

			if (entity.CheckDistanceToDestination())
				ChasePlayer(entity);
		}
	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{

	}

	//attack behaviour
	public void UpdatePlayerPosition(EntityBehaviour entity)
	{
		if (entity.player != null && entity.CheckIfPlayerVisible())
			entity.playersLastKnownPosition = entity.player.transform.position;
	}
	public void ChasePlayer(EntityBehaviour entity)
	{
		Vector2 movePosition = entity.SampleNewMovePosition(entity.playersLastKnownPosition);
		entity.CheckAndSetNewPath(movePosition);
	}
}
