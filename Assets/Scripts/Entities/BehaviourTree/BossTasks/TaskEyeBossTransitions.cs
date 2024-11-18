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

	readonly SOBossEntityBehaviour bossBehaviourRef;
	readonly BossEntityBehaviour behaviour;
	readonly BossEntityStats stats;

	private float actionDelayTimer;
	private int phaseTwoStep;

	public TaskEyeBossTransitions(BossEntityBehaviour behaviour)
	{
		bossBehaviourRef = (SOBossEntityBehaviour)behaviour.behaviourRef;
		this.behaviour = behaviour;
		stats = (BossEntityStats)behaviour.entityStats;
		phaseTwoStep = 0;
	}

	public override NodeState Evaluate()
	{
		if (stats.inPhaseTransition == false) return NodeState.FAILURE;

		Debug.Log(stats.name + " boss phase Transition task");

		ActionDelayTimer();

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
		if (!CanStartNextPhaseTwoStep()) return;

		switch (phaseTwoStep)
		{
			case 0:
			stats.damageable.invincible = true;
			SetActionDelayTimer(5);
			NextPhaseTwoStep();
			break;

			case 1:
			behaviour.ForceCastBossAbilityAtLocation(bossBehaviourRef.specialBossAbilities[0], Vector3.left * 10, true);
			SetActionDelayTimer(5);
			NextPhaseTwoStep();
			break;

			case 2:
			behaviour.ForceCastBossAbilityAtLocation(bossBehaviourRef.specialBossAbilities[0], Vector3.right * 10, true);
			SetActionDelayTimer(5);
			NextPhaseTwoStep();
			break;

			case 3:
			stats.damageable.invincible = false;
			stats.inPhaseTransition = false;
			break;

			default:
			Debug.LogError("no step found");
			break;
		}
	}

	public void PhaseThreeTransition()
	{
		stats.inPhaseTransition = false;
	}

	//update phase 2 steps
	private void NextPhaseTwoStep()
	{
		phaseTwoStep++;
	}

	//delay phase 2 steps
	private void ActionDelayTimer()
	{
        if (actionDelayTimer >= 0)
			actionDelayTimer -= Time.deltaTime;
	}
	private void SetActionDelayTimer(float delay)
	{
		actionDelayTimer = delay;
	}
	private bool CanStartNextPhaseTwoStep()
	{
		if (actionDelayTimer <= 0)
			return true;
		else return false;
	}
}
