using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TaskUseAbility : EntityAbilities
{
	EntityBehaviour behaviour;
	EntityStats stats;

	public TaskUseAbility(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		stats = behaviour.entityStats;
	}

	public override NodeState Evaluate()
	{
		//return failure to force switch back to attack with main weapon
		if (behaviour.globalAttackTimer > 0) return NodeState.FAILURE;
		else
		{
			if (CanUseHealingAbility())
				CastHealingAbility(behaviour);
			else if (CanUseOffensiveAbility())
				CastOffensiveAbility(behaviour);
			else return NodeState.FAILURE;

			//add ability animation length here if needed, include a bool if animation should block movement
			behaviour.globalAttackTimer = 1f;
			return NodeState.SUCCESS;
		}
	}

	public bool CanUseHealingAbility()
	{
		if (behaviour.abilityBeingCasted != null || stats.statsRef.isBossVersion || behaviour.healingAbility == null) return false;

		int healthPercentage = (int)((float)stats.currentHealth / stats.maxHealth.finalValue * 100);
		if (healthPercentage > 50) return false; //unique ability checks

		if (!behaviour.canCastHealingAbility  || stats.maxHealth.finalValue == 0) return false;

		if (!HasEnoughManaToCast(stats, behaviour.healingAbility)) //mana check
		{
			behaviour.healingAbilityTimer = 2.5f;   //if low mana wait 2.5s then try again
			return false;
		}

		else return true;
	}
	public bool CanUseOffensiveAbility()
	{
		if (behaviour.abilityBeingCasted != null || behaviour.offensiveAbility == null || !behaviour.canCastOffensiveAbility) return false;

		if (behaviour.playerTarget == null) return false; //unique ability checks

		if (!HasEnoughManaToCast(stats, behaviour.offensiveAbility)) //mana check
		{
			behaviour.offensiveAbilityTimer = 2.5f;   //if low mana wait 2.5s then try again
			return false;
		}
		else return true;
	}
}
