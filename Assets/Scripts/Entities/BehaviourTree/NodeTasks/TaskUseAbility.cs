using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TaskUseAbility : BTNode
{
	EntityStats stats;
	EntityBehaviour behaviour;
	EntityEquipmentHandler equipmentHandler;
	NavMeshAgent navMesh;


	public TaskUseAbility(EntityStats entity)
	{
		stats = entity;
		behaviour = entity.entityBehaviour;
		equipmentHandler = entity.equipmentHandler;
		navMesh = entity.entityBehaviour.navMeshAgent;
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

			//Debug.LogError(stats.name + " using an ability");

			//add ability animation length here if needed, include a bool if animation should block movement
			behaviour.globalAttackTimer = 1f;
			return NodeState.SUCCESS;
		}
	}

	public bool CanUseHealingAbility()
	{
		if (!behaviour.canCastHealingAbility || !HasEnoughManaToCast(behaviour.healingAbility)) return false;

		int healthPercentage = (int)((float)stats.currentHealth / stats.maxHealth.finalValue * 100);
		if (healthPercentage > 50) return false;
		else return true;
	}
	public bool CanUseOffensiveAbility()
	{
		if (!behaviour.canCastOffensiveAbility || !HasEnoughManaToCast(behaviour.offensiveAbility)) return false;
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
