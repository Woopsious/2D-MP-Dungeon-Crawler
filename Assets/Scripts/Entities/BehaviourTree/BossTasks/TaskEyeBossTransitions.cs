using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
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
	/// after 5 seconds do x degree cone attacks in left and right directions with transition ability 2,
	/// if player is hit by this, freeze movement for x seconds and apply x% damage taken debuff.
	/// 
	/// TRANSITION PHASE 3: 
	/// enter phase 3.
	/// </summary>

	readonly SOBossEntityBehaviour bossBehaviourRef;
	readonly BossEntityBehaviour behaviour;
	readonly BossEntityStats stats;
	readonly EntityAbilityHandler abilityHandler;

	private float actionDelayTimer;

	public TaskEyeBossTransitions(BossEntityBehaviour behaviour)
	{
		bossBehaviourRef = (SOBossEntityBehaviour)behaviour.behaviourRef;
		this.behaviour = behaviour;
		stats = (BossEntityStats)behaviour.entityStats;
		abilityHandler = behaviour.abilityHandler;
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

		switch (behaviour.stepInPhaseTransition)
		{
			case 0:
			stats.damageable.invincible = true;
			abilityHandler.AllowCastingOfTransitionAbility();
			SetActionDelayTimer(bossBehaviourRef.transitionAbilityTwo.abilityCastingTimer + 3);
			behaviour.NextStepInPhase();
			break;

			case 1:
			abilityHandler.AllowCastingOfTransitionAbility();
			SetActionDelayTimer(bossBehaviourRef.transitionAbilityTwo.abilityCastingTimer + 3);
			behaviour.NextStepInPhase();
			break;

			case 2:
			SetActionDelayTimer(5);
			behaviour.NextStepInPhase();
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

	//delay phase steps
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
