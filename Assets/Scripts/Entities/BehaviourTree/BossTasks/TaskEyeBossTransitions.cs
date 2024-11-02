using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskEyeBossTransitions : EntityMovement, IBossTransitionPhases
{
	/// <summary> Eye boss behaviour for phase transitions 1-3:
	/// spawn with a center piece
	/// TRANSITION PHASE 1: 
	/// enter phase 1
	/// 
	/// TRANSITION PHASE 2: 
	/// become invulnerable, spawn x amount of adds. 
	/// after x seconds do x degree cone attacks in certian directions with unique ability,
	/// (represent cone visually before attack) till all directions hit with x delay between each use. enter phase 2 after another x seconds
	/// if player is hit by this, do x% damage to health, freeze movement for x seconds and apply x% damage taken debuff.
	/// 
	/// TRANSITION PHASE 3: 
	/// enter phase 3.
	/// </summary>

	BossEntityBehaviour behaviour;
	BossEntityStats stats;

	public TaskEyeBossTransitions(BossEntityBehaviour behaviour)
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

	public void PhaseThreeTransition()
	{
		stats.inPhaseTransition = false;
	}

	public void PhaseTwoTransition()
	{
		stats.inPhaseTransition = false;
	}
}
