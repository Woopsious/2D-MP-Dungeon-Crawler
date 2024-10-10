using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TaskGoblinBossTransitionPhase : TaskBossTransitionPhase
{
	BossEntityStats stats;
	BossEntityBehaviour behaviour;

	float slowHealTimer;

	readonly int selfHealLimit = 5;
	int currentSelfHealCounter;

	public TaskGoblinBossTransitionPhase(BossEntityStats entity)
	{
		this.stats = entity;
		behaviour = (BossEntityBehaviour)entity.entityBehaviour;
	}

	public override NodeState Evaluate()
	{
		if (stats.inPhaseTransition == false) return NodeState.FAILURE;

		RunPhaseTransition(stats);

		return NodeState.SUCCESS;
	}

	//run boss phases
	protected override void RunPhaseTransition(BossEntityStats stats)
	{
		base.RunPhaseTransition(stats);
	}

	//boss phases
	protected override void PhaseOneTransition(BossEntityStats stats)
	{
		base.PhaseOneTransition(stats);
	}
	protected override void PhaseTwoTransition(BossEntityStats stats)
	{
		if (stats.lastBossHealthPercentage >= 0.8 || currentSelfHealCounter >= selfHealLimit ||
			stats.spawner.CheckIfSpawnedEntitiesListEmpty())
		{
			behaviour.ChangeState(behaviour.goblinAttackState);
			stats.damageable.invincible = false;
			stats.inPhaseTransition = false;
		}

		slowHealTimer -= Time.deltaTime;
		if (slowHealTimer <= 0 && currentSelfHealCounter <= selfHealLimit)
		{
			currentSelfHealCounter++;
			slowHealTimer = 5;
			stats.OnHeal(0.025f, true, stats.healingPercentageModifier.finalPercentageValue);
		}
	}
	protected override void PhaseThreeTransition(BossEntityStats stats)
	{
		base.PhaseThreeTransition(stats);
	}
}
