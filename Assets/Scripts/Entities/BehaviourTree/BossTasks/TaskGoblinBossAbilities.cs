using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskGoblinBossAbilities : BTNode, IBossAbilities
{
	/// <summary>
	/// ABILITY 1: 
	/// simple aoe ground stomp around boss
	/// 
	/// ABILITY 2: 
	/// offensive stance self buff
	/// 
	/// ABILITY 3: 
	/// last stand self buff
	/// </summary>

	readonly BossEntityBehaviour behaviour;
	readonly BossEntityStats stats;
	readonly EntityAbilityHandler abilityHandler;

	public TaskGoblinBossAbilities(BossEntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		stats = (BossEntityStats)behaviour.entityStats;
		abilityHandler = behaviour.abilityHandler;
	}

	public override NodeState Evaluate()
	{
		//return failure to force switch back to attack with main weapon
		if (behaviour.globalAttackTimer > 0) return NodeState.FAILURE;

		if (!stats.inPhaseTransition)
		{
			if (CanUseBossAbilityOne())
			{
				abilityHandler.CastBossAbilityOne();
			}
			else if (CanUseBossAbilityTwo())
			{
				abilityHandler.CastBossAbilityTwo();
			}
			else if (CanUseBossAbilityThree())
			{
				abilityHandler.CastBossAbilityThree();
			}
			else return NodeState.FAILURE;

			//add ability animation length here if needed, include a bool if animation should block movement
		}
		else
		{
			if (CanUseTransitionAbilityOne())
			{
				PhaseTransitionOneSteps();
			}
			else if (CanUseTransitionAbilityTwo())
			{
				PhaseTransitionTwoSteps();
			}
			else if (CanUseTransitionAbilityThree())
			{
				PhaseTransitionThreeSteps();
			}
			else return NodeState.FAILURE;
		}

		behaviour.globalAttackTimer = 1f;
		return NodeState.SUCCESS;
	}

	//ability checks
	public bool CanUseBossAbilityOne()
	{
		if (behaviour.playerTarget == null || abilityHandler.abilityBeingCasted || stats.inPhaseTransition ||
			!abilityHandler.canCastAbilityOne || !abilityHandler.HasEnoughManaToCast(abilityHandler.abilityOne)) return false;
		else return true;
	}
	public bool CanUseBossAbilityTwo()
	{
		if (behaviour.playerTarget == null || 
			abilityHandler.abilityBeingCasted || stats.inPhaseTransition || stats.bossPhase < BossEntityStats.BossPhase.secondPhase ||
			!abilityHandler.canCastAbilityTwo || !abilityHandler.HasEnoughManaToCast(abilityHandler.abilityTwo)) return false;
		else return true;
	}
	public bool CanUseBossAbilityThree()
	{
		if (behaviour.playerTarget == null || 
			abilityHandler.abilityBeingCasted || stats.inPhaseTransition || stats.bossPhase < BossEntityStats.BossPhase.thirdPhase ||
			!abilityHandler.canCastAbilityThree || !abilityHandler.HasEnoughManaToCast(abilityHandler.abilityThree)) return false;
		else return true;
	}

	//transition ability steps
	public void PhaseTransitionOneSteps()
	{
		//noop
	}
	public void PhaseTransitionTwoSteps()
	{
		//noop
	}
	public void PhaseTransitionThreeSteps()
	{
		//noop
	}

	//transition ability checks
	public bool CanUseTransitionAbilityOne()
	{
		//noop
		return false;
	}
	public bool CanUseTransitionAbilityTwo()
	{
		//noop
		return false;
	}
	public bool CanUseTransitionAbilityThree()
	{
		//noop
		return false;
	}
}
