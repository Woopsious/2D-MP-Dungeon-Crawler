using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGoblinTransitions : EnemyBaseState
{
	BossEntityStats entityStats;
	BossEntityBehaviour entityBehaviour;

	float slowHealTimer;

	private readonly int selfHealLimit = 5;
	private int currentSelfHealCounter;

	[Header("Entity Spawns")]
	public List<EntityStats> friendlySpawnedEntities = new List<EntityStats>();

	public override void Enter(EntityBehaviour entity)
	{
		base.Enter(entity);
		entityStats = (BossEntityStats)entity.entityStats;
		entityBehaviour = (BossEntityBehaviour)entity;

		if (entityStats.bossPhase == BossEntityStats.BossPhase.secondPhase)
		{
			//code to run to center piece and heal
			entityStats.damageable.invincible = true;
			currentSelfHealCounter = 0;
		}
		else if (entityStats.bossPhase == BossEntityStats.BossPhase.thirdPhase)
		{
			//noop
		}
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

	//enter

	//updates
	private void SlowHealHealthTimer()
	{
		if (entityStats.lastBossHealthPercentage >= 0.8 || currentSelfHealCounter >= selfHealLimit || 
			entityStats.spawner.CheckIfSpawnedEntitiesListEmpty())
		{
			entityBehaviour.ChangeState(entityBehaviour.goblinAttackState);
			entityStats.damageable.invincible = false;
		}

		slowHealTimer -= Time.deltaTime;
		if (slowHealTimer <= 0 && currentSelfHealCounter <= selfHealLimit)
		{
			currentSelfHealCounter++;
			slowHealTimer = 10;
			entityStats.OnHeal(0.025f, true, entityStats.healingPercentageModifier.finalPercentageValue);
		}
	}
}
