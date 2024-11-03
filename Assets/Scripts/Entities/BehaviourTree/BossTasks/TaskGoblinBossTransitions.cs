using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TaskGoblinBossTransitions : EntityMovement, IBossTransitionPhases
{
	/// <summary> Goblin boss behaviour for phase transitions 1-3:
	/// spawn with a center piece
	/// TRANSITION PHASE 1: 
	/// enter phase 1.
	/// 
	/// TRANSITION PHASE 2:
	/// become invulnerable, disable attacking, disable ability use. return to center piece, then heal till all 
	/// friendlies dead or healed 5x (each heal = 2.5%), become vulnerable enter phase 2.
	/// 
	/// TRANSITION PHASE 3: 
	/// enter "enrage" state, increase damage done by 10% + increase move speed/acceleration.
	/// </summary>

	BossEntityBehaviour behaviour;
	BossEntityStats stats;

	float slowHealTimer;

	readonly int selfHealLimit = 5;
	int currentSelfHealCounter;

	public TaskGoblinBossTransitions(BossEntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		stats = (BossEntityStats)behaviour.entityStats;
	}

	public override NodeState Evaluate()
	{
		if (stats.inPhaseTransition == false) return NodeState.FAILURE;

		Debug.Log(stats.name + " boss phase Transition task");

		if (stats.bossPhase == BossEntityStats.BossPhase.firstPhase)
			PhaseOneTransition();
		else if (stats.bossPhase == BossEntityStats.BossPhase.secondPhase)
			PhaseTwoTransition();
		else if (stats.bossPhase == BossEntityStats.BossPhase.thirdPhase)
			PhaseThreeTransition();

		return NodeState.SUCCESS;
	}


	public void PhaseOneTransition()
	{
		stats.inPhaseTransition = false;
	}
	public void PhaseTwoTransition()
	{
		slowHealTimer -= Time.deltaTime;

		MoveToCenterPiece(behaviour);

		if (!BossReachedCenterPiece(behaviour)) return;

		if (stats.lastBossHealthPercentage >= 0.8 || currentSelfHealCounter >= selfHealLimit) //checks to exit
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
	public void PhaseThreeTransition()
	{
		stats.damageDealtModifier.AddPercentageValue(0.1f);
		behaviour.navMeshAgent.speed += 2;
		behaviour.navMeshAgent.acceleration += 2;
		//play enrage animation etc...
		stats.inPhaseTransition = false;
	}
}
