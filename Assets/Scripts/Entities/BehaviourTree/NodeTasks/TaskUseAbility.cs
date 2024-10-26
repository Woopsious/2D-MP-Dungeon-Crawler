using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TaskUseAbility : BTNode
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
				behaviour.TryCastHealingAbility(stats.maxHealth.finalValue, stats.currentHealth);
			else if (CanUseOffensiveAbility())
				behaviour.TryCastOffensiveAbility();
			else return NodeState.FAILURE;

			//add ability animation length here if needed, include a bool if animation should block movement
			behaviour.globalAttackTimer = 1f;
			return NodeState.SUCCESS;
		}
	}

	public bool CanUseHealingAbility()
	{
		if (!behaviour.canCastHealingAbility || !behaviour.HasEnoughManaToCast(behaviour.healingAbility)) return false;

		int healthPercentage = (int)((float)stats.currentHealth / stats.maxHealth.finalValue * 100);
		if (healthPercentage > 50) return false;
		else return true;
	}
	public bool CanUseOffensiveAbility()
	{
		if (!behaviour.canCastOffensiveAbility || !behaviour.HasEnoughManaToCast(behaviour.offensiveAbility)) return false;
		else return true;
	}
}
