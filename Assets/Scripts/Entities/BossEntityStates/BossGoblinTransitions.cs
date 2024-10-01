using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGoblinTransitions : EnemyBaseState
{
	BossEntityStats entityStats;
	BossEntityBehaviour entityBehaviour;

	float slowHealTimer;

	public override void Enter(EntityBehaviour entity)
	{
		base.Enter(entity);
		entityStats = (BossEntityStats)entity.entityStats;
		entityBehaviour = (BossEntityBehaviour)entity;
	}
	public override void Exit(EntityBehaviour entity)
	{
		base.Exit(entity);
	}
	public override void UpdateLogic(EntityBehaviour entity)
	{
		if (entityStats.bossPhase == BossEntityStats.BossPhase.secondPhase)
			PhaseTwoTransition((BossEntityBehaviour)entity);
		else if (entityStats.bossPhase == BossEntityStats.BossPhase.thirdPhase)
			PhaseThreeTransition((BossEntityBehaviour)entity);
	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{
		base.UpdatePhysics(entity);
	}

	private void PhaseTwoTransition(BossEntityBehaviour entity)
	{
		SlowHealHealthTimer();

		//Debug.Log("Phase Two Transition");
	}
	private void PhaseThreeTransition(BossEntityBehaviour entity)
	{
		//Debug.Log("Phase Three Transition");
	}

	private void SlowHealHealthTimer()
	{
		if (entityStats.lastBossHealthPercentage >= 0.8)
			entityBehaviour.ChangeState(entityBehaviour.goblinAttackState);

		slowHealTimer -= Time.deltaTime;
		if (slowHealTimer <= 0)
		{
			slowHealTimer = 10;
			entityStats.OnHeal(0.05f, true, entityStats.healingPercentageModifier.finalPercentageValue);
		}
	}
}
