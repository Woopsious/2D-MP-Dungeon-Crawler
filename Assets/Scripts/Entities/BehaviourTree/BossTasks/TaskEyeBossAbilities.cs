using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskEyeBossAbilities : BTNode, IBossAbilities
{
	/// <summary>
	/// ABILITY 1:
	/// simple directional ability
	/// 
	/// ABILITY 2:
	/// directional aoe line attack from boss directed at player marked by eye, spawns in obstacles around room center piece
	/// marked player hides behind to avoid damage (takes 10s to cast)
	/// 
	/// ABILITY 3:
	/// circle aoe ontop of marked player, damage split between all players hit by aoe, damage adjust further based on player lobby count
	/// (players need to group up and share the damage, takes 10s to cast)
	/// </summary>

	readonly SOBossEntityBehaviour behaviourRef;
	readonly BossEntityBehaviour behaviour;
	readonly BossEntityStats stats;
	readonly EntityAbilityHandler abilityHandler;

	public TaskEyeBossAbilities(BossEntityBehaviour behaviour)
	{
		behaviourRef = (SOBossEntityBehaviour)behaviour.behaviourRef;
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
		if (behaviour.playerTarget == null || abilityHandler.abilityBeingCasted ||
			!abilityHandler.canCastAbilityOne || !abilityHandler.HasEnoughManaToCast(abilityHandler.abilityOne)) return false;
		else return true;
	}
	public bool CanUseBossAbilityTwo()
	{
		if (behaviour.playerTarget == null || 
			abilityHandler.abilityBeingCasted || stats.bossPhase < BossEntityStats.BossPhase.secondPhase ||
			!abilityHandler.canCastAbilityTwo || !abilityHandler.HasEnoughManaToCast(abilityHandler.abilityTwo)) return false;
		else return true;
	}
	public bool CanUseBossAbilityThree()
	{
		if (behaviour.playerTarget == null || 
			abilityHandler.abilityBeingCasted || stats.bossPhase < BossEntityStats.BossPhase.thirdPhase ||
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
		switch (behaviour.stepInPhaseTransition)
		{
			case 0:
			//noop
			break;

			case 1:
			abilityHandler.CastTransitionAbility(behaviourRef.transitionAbilityTwo, Vector2.left * 10, true);
			break;

			case 2:
			abilityHandler.CastTransitionAbility(behaviourRef.transitionAbilityTwo, Vector2.right * 10, true);
			break;

			case 3:
			//noop
			break;

			default:
			Debug.LogError("no step found");
			break;
		}
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
		if (behaviour.playerTarget == null || 
			!abilityHandler.canCastTransitionAbility || stats.bossPhase != BossEntityStats.BossPhase.secondPhase) return false;
		else return true;
	}
	public bool CanUseTransitionAbilityThree()
	{
		//noop
		return false;
	}
}
