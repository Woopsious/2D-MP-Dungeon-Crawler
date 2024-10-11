using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TaskGoblinBossTransitionPhase : TaskBossTransitionPhase
{
	/// <summary>
	/// Goblin boss behaviour for phase transitions 1-3: 
	/// boss will spawn in along with a center piece.
	/// TRANSITION PHASE 1: spawn with a center piece and enter phase 1.
	/// TRANSITION PHASE 2: become invulnerable, disable attacking, disable ability use. return to center piece, then heal till all 
	/// friendlies dead or healed 5x (each heal = 2.5%), become vulnerable enter phase 2.
	/// TRANSITION PHASE 3: enter "enrage" state, increase damage done by 10% + increase move speed/acceleration.
	/// </summary>

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
		slowHealTimer -= Time.deltaTime;

		if (stats.lastBossHealthPercentage >= 0.8 || currentSelfHealCounter >= selfHealLimit ||
			stats.spawner.CheckIfSpawnedEntitiesListEmpty()) //checks to exit
		{
			stats.damageable.invincible = false;
			stats.inPhaseTransition = false;
		}

		if (slowHealTimer <= 0 && currentSelfHealCounter <= selfHealLimit) //checks to heal/reset timer
		{
			currentSelfHealCounter++;
			slowHealTimer = 5;
			stats.OnHeal(0.025f, true, stats.healingPercentageModifier.finalPercentageValue);
		}
	}
	protected override void PhaseThreeTransition(BossEntityStats stats)
	{
		stats.damageDealtModifier.AddPercentageValue(0.1f);
		behaviour.navMeshAgent.speed += 2;
		behaviour.navMeshAgent.acceleration += 2;
		//play enrage animation etc...
		stats.inPhaseTransition = false;
	}
}
