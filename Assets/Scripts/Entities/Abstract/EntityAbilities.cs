using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class EntityAbilities : BTNode
{
	//entity abilities
	protected void CastHealingAbility(EntityBehaviour behaviour)
	{
		behaviour.canCastHealingAbility = false;
		behaviour.healingAbilityTimer = behaviour.healingAbility.abilityCooldown;
		behaviour.abilityBeingCasted = behaviour.healingAbility;
	}

	protected void CastOffensiveAbility(EntityBehaviour behaviour)
	{
		behaviour.canCastOffensiveAbility = false;
		behaviour.offensiveAbilityTimer = behaviour.offensiveAbility.abilityCooldown;
		behaviour.abilityBeingCasted = behaviour.offensiveAbility;
	}

	//boss entity abilities
	protected void CastAbilityOne(BossEntityBehaviour behaviour)
	{
		behaviour.canCastAbilityOne = false;
		behaviour.abilityTimerOneCounter = behaviour.abilityOne.abilityCooldown;
		behaviour.abilityBeingCasted = behaviour.abilityOne;
	}
	protected void CastAbilityTwo(BossEntityBehaviour behaviour)
	{
		behaviour.canCastAbilityTwo = false;
		behaviour.abilityTimerTwoCounter = behaviour.abilityTwo.abilityCooldown;
		behaviour.abilityBeingCasted = behaviour.abilityTwo;
	}
	protected void CastAbilityThree(BossEntityBehaviour behaviour)
	{
		behaviour.canCastAbilityThree = false;
		behaviour.abilityTimerThreeCounter = behaviour.abilityThree.abilityCooldown;
		behaviour.abilityBeingCasted = behaviour.abilityThree;
	}

	protected bool HasEnoughManaToCast(EntityStats stats, SOClassAbilities ability)
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
