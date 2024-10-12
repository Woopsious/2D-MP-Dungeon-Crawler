using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskGoblinBossUseAbilities : TaskBossUseAbilities
{
	public TaskGoblinBossUseAbilities(BossEntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		stats = (BossEntityStats)behaviour.entityStats;
		equipmentHandler = behaviour.equipmentHandler;
	}

	public override NodeState Evaluate()
	{
		//return failure to force switch back to attack with main weapon
		if (behaviour.globalAttackTimer > 0) return NodeState.FAILURE;
		else
		{
			if (CanUseBossAbilityOne())
			{
				behaviour.CastBossAbilityOne();
			}
			else if (CanUseBossAbilityTwo())
			{
				behaviour.CastBossAbilityTwo();
			}
			else if (CanUseBossAbilityThree())
			{
				behaviour.CastBossAbilityThree();
			}
			else return NodeState.FAILURE;

			//Debug.LogError(stats.name + " using an ability");

			//add ability animation length here if needed, include a bool if animation should block movement
			behaviour.globalAttackTimer = 1f;
			return NodeState.SUCCESS;
		}
	}

	protected override bool CanUseBossAbilityOne()
	{
		if (stats.inPhaseTransition ||
			!behaviour.canCastAbilityOne || !HasEnoughManaToCast(behaviour.abilityOne)) return false;
		else return true;
	}
	protected override bool CanUseBossAbilityTwo()
	{
		if (stats.inPhaseTransition || stats.bossPhase < BossEntityStats.BossPhase.secondPhase ||
			!behaviour.canCastAbilityTwo || !HasEnoughManaToCast(behaviour.abilityTwo)) return false;
		else return true;
	}
	protected override bool CanUseBossAbilityThree()
	{
		if (stats.inPhaseTransition || stats.bossPhase < BossEntityStats.BossPhase.thirdPhase ||
			!behaviour.canCastAbilityThree || !HasEnoughManaToCast(behaviour.abilityThree)) return false;
		else return true;
	}

	private bool HasEnoughManaToCast(SOClassAbilities ability)
	{
		if (ability.isSpell)
		{
			int totalManaCost = (int)(ability.manaCost * stats.levelModifier);
			if (stats.currentMana <= totalManaCost)
				return false;
			else return true;
		}
		else
			return true;
	}
}
