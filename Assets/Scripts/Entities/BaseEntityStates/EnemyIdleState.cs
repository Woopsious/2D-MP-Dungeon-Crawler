using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
			IdleAtPositionTimer(entity);
	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{

	}

	private void IdleAtPositionTimer(EntityBehaviour entity)
	{
		entity.idleTimer -= Time.deltaTime;

		if (entity.idleTimer < 0)
		{
			entity.idleTimer = entity.entityBehaviour.idleWaitTime;
			entity.ChangeState(entity.wanderState);
		}
	}
}
