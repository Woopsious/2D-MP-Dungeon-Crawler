using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TaskUseAbility : BTNode
{
	readonly EntityBehaviour behaviour;
	readonly EntityStats stats;
	readonly EntityAbilityHandler abilityHandler;

	public TaskUseAbility(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		stats = behaviour.entityStats;
		abilityHandler = behaviour.abilityHandler;
	}

	public override NodeState Evaluate()
	{
		//return failure to force switch back to attack with main weapon
		if (behaviour.globalAttackTimer > 0) return NodeState.FAILURE;
		else
		{
			if (CanUseHealingAbility())
				abilityHandler.CastHealingAbility();
			else if (CanUseOffensiveAbility())
				abilityHandler.CastOffensiveAbility();
			else return NodeState.FAILURE;

			//add ability animation length here if needed, include a bool if animation should block movement
			behaviour.globalAttackTimer = 1f;
			return NodeState.SUCCESS;
		}
	}

	public bool CanUseHealingAbility()
	{
		if (abilityHandler.abilityBeingCasted != null || stats.statsRef.isBossVersion || 
			abilityHandler.healingAbility == null) return false;

		int healthPercentage = (int)((float)stats.currentHealth / stats.maxHealth.finalValue * 100);
		if (healthPercentage > 50) return false; //unique ability checks

		if (!abilityHandler.canCastHealingAbility  || stats.maxHealth.finalValue == 0) return false;

		if (!abilityHandler.HasEnoughManaToCast(abilityHandler.healingAbility)) //mana check
		{
			abilityHandler.healingAbilityTimer = 2.5f;   //if low mana wait 2.5s then try again
			return false;
		}

		else return true;
	}
	public bool CanUseOffensiveAbility()
	{
		if (abilityHandler.abilityBeingCasted != null || abilityHandler.offensiveAbility == null || 
			!abilityHandler.canCastOffensiveAbility) return false;

		if (behaviour.playerTarget == null) return false; //unique ability checks

		if (!abilityHandler.HasEnoughManaToCast(abilityHandler.offensiveAbility)) //mana check
		{
			abilityHandler.offensiveAbilityTimer = 2.5f;   //if low mana wait 2.5s then try again
			return false;
		}
		else return true;
	}
}
