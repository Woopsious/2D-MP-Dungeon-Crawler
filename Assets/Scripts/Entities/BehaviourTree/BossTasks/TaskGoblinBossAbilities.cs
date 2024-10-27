using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskGoblinBossAbilities : EntityAbilities, IBossAbilities
{
	/// <summary>
	/// Goblin boss abilities can be used as long as ability number >= boss phase (ability 2 usable in phase 2 & 3, not in 1)
	/// </summary>

	readonly BossEntityBehaviour behaviour;
	readonly BossEntityStats stats;

	public TaskGoblinBossAbilities(BossEntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		stats = (BossEntityStats)behaviour.entityStats;
	}

	public override NodeState Evaluate()
	{
		//return failure to force switch back to attack with main weapon
		if (behaviour.globalAttackTimer > 0) return NodeState.FAILURE;
		else
		{
			if (CanUseBossAbilityOne())
			{
				CastAbilityOne(behaviour);
			}
			else if (CanUseBossAbilityTwo())
			{
				CastAbilityTwo(behaviour);
			}
			else if (CanUseBossAbilityThree())
			{
				CastAbilityThree(behaviour);
			}
			else return NodeState.FAILURE;

			//add ability animation length here if needed, include a bool if animation should block movement
			behaviour.globalAttackTimer = 1f;
			return NodeState.SUCCESS;
		}
	}

	public bool CanUseBossAbilityOne()
	{
		if (behaviour.abilityBeingCasted || stats.inPhaseTransition ||
			!behaviour.canCastAbilityOne || !HasEnoughManaToCast(stats, behaviour.abilityOne)) return false;
		else return true;
	}
	public bool CanUseBossAbilityTwo()
	{
		if (behaviour.abilityBeingCasted || stats.inPhaseTransition || stats.bossPhase < BossEntityStats.BossPhase.secondPhase ||
			!behaviour.canCastAbilityTwo || !HasEnoughManaToCast(stats, behaviour.abilityTwo)) return false;
		else return true;
	}
	public bool CanUseBossAbilityThree()
	{
		if (behaviour.abilityBeingCasted || stats.inPhaseTransition || stats.bossPhase < BossEntityStats.BossPhase.thirdPhase ||
			!behaviour.canCastAbilityThree || !HasEnoughManaToCast(stats, behaviour.abilityThree)) return false;
		else return true;
	}
}
