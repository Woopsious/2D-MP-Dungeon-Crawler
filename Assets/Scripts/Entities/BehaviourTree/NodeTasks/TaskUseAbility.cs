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
			if (CanUseHealingAbility() && HealingAbilityInRangeOfTarget())
				abilityHandler.CastHealingAbility();
			else if (CanUseOffensiveAbility() && OffensiveAbilityInRangeOfTarget())
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

	public bool HealingAbilityInRangeOfTarget()
	{
		return true; //atm always true as can only target self
	}
	public bool OffensiveAbilityInRangeOfTarget()
	{
		Vector2 targetPos;

		if (abilityHandler.overriddenPlayerTarget)
			targetPos = abilityHandler.overriddenPlayerTarget.transform.position;
		else
		{
			targetPos = behaviour.playerTarget.transform.position;
		}

		if (abilityHandler.offensiveAbility.isAOE)
		{
			float distance = Vector2.Distance(targetPos, behaviour.transform.position);

			if (abilityHandler.offensiveAbility.aoeType == SOAbilities.AoeType.isConeAoe)
			{
				Debug.LogError("distance to target: " + distance);

				if (distance < abilityHandler.offensiveAbility.coneAoeRadius)
					return true;
				else return false;
			}
			else if (abilityHandler.offensiveAbility.aoeType == SOAbilities.AoeType.isBoxAoe)
			{
				float convertOffset = (float)(abilityHandler.offensiveAbility.boxAoeSizeY / 6.66666666);

				Debug.LogError("distance to target: " + distance);
				Debug.LogError("converted World Size: " + convertOffset);

				if (distance < convertOffset)
					return true;
				else return false;
			}
			else
				return true;
		}
		else return true;
	}
}
