using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;

public abstract class EntityAbilities : BTNode
{
	//entity abilities
	protected void CastHealingAbility(EntityBehaviour behaviour)
	{
		behaviour.canCastHealingAbility = false;
		behaviour.healingAbilityTimer = behaviour.healingAbility.abilityCooldown + behaviour.healingAbility.abilityCastingTimer;
		behaviour.abilityCastingTimer = behaviour.healingAbility.abilityCastingTimer;
		behaviour.abilityBeingCasted = behaviour.healingAbility;
	}

	protected void CastOffensiveAbility(EntityBehaviour behaviour)
	{
		behaviour.canCastOffensiveAbility = false;
		behaviour.offensiveAbilityTimer = behaviour.offensiveAbility.abilityCooldown + behaviour.offensiveAbility.abilityCastingTimer;
		behaviour.abilityCastingTimer = behaviour.offensiveAbility.abilityCastingTimer;
		behaviour.abilityBeingCasted = behaviour.offensiveAbility;
	}

	//boss entity abilities
	protected void CastAbilityOne(BossEntityBehaviour behaviour)
	{
		TryMarkPlayer(behaviour, behaviour.abilityOne);

		behaviour.canCastAbilityOne = false;
		behaviour.abilityTimerOneCounter = behaviour.abilityOne.abilityCooldown + behaviour.abilityOne.abilityCastingTimer;
		behaviour.abilityCastingTimer = behaviour.abilityOne.abilityCastingTimer;
		behaviour.abilityBeingCasted = behaviour.abilityOne;

		behaviour.abilityIndicators.ShowAoeIndicators(behaviour.abilityOne, behaviour);
		behaviour.EventBossAbilityBeginCasting(behaviour.abilityOne);
	}
	protected void CastAbilityTwo(BossEntityBehaviour behaviour)
	{
		TryMarkPlayer(behaviour, behaviour.abilityTwo);

		behaviour.canCastAbilityTwo = false;
		behaviour.abilityTimerTwoCounter = behaviour.abilityTwo.abilityCooldown + behaviour.abilityTwo.abilityCastingTimer;
		behaviour.abilityCastingTimer = behaviour.abilityTwo.abilityCastingTimer;
		behaviour.abilityBeingCasted = behaviour.abilityTwo;

		behaviour.abilityIndicators.ShowAoeIndicators(behaviour.abilityTwo, behaviour);
		behaviour.EventBossAbilityBeginCasting(behaviour.abilityTwo);
	}
	protected void CastAbilityThree(BossEntityBehaviour behaviour)
	{
		TryMarkPlayer(behaviour, behaviour.abilityThree);

		behaviour.canCastAbilityThree = false;
		behaviour.abilityTimerThreeCounter = behaviour.abilityThree.abilityCooldown + behaviour.abilityThree.abilityCastingTimer;
		behaviour.abilityCastingTimer = behaviour.abilityThree.abilityCastingTimer;
		behaviour.abilityBeingCasted = behaviour.abilityThree;

		behaviour.abilityIndicators.ShowAoeIndicators(behaviour.abilityThree, behaviour);
		behaviour.EventBossAbilityBeginCasting(behaviour.abilityThree);
	}

	protected void TryMarkPlayer(BossEntityBehaviour behaviour, SOBossAbilities ability)
	{
		if (!ability.marksPlayer) return;

		PlayerController newPlayerTarget = behaviour.playerTarget;
		int chance = (int)(ability.chanceMarkedPlayerIsAggroTarget * 100);

		//mark random player that isnt current aggro player as long as aggro list count bigger then 1
		if (behaviour.playerAggroList.Count > 1 && Utilities.GetRandomNumber(100) >= chance)
			newPlayerTarget = behaviour.playerAggroList[Utilities.GetRandomNumberBetween(1, behaviour.playerAggroList.Count)].player;

		behaviour.OverrideCurrentPlayerTarget(newPlayerTarget); //override for marked by boss effect
		behaviour.ForceCastMarkPlayerBossAbility();
		behaviour.OverrideCurrentPlayerTarget(newPlayerTarget); //override for ability
	}

	protected bool HasEnoughManaToCast(EntityStats stats, SOAbilities ability)
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
