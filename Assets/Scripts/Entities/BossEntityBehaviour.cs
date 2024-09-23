using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEntityBehaviour : EntityBehaviour
{
	[Header("Boss Abilities")]
	[Header("Offensive Ability OTwo")]
	public SOClassAbilities offensiveAbilityTwo;
	private bool canCastOffensiveAbilityTwo;
	private float offensiveAbilityTimerTwo;

	public void CastOffensiveAbilityTwo()
	{
		if (!canCastOffensiveAbilityTwo || offensiveAbilityTwo == null) return;
		if (playerTarget == null) return;

		if (!HasEnoughManaToCast(offensiveAbilityTwo))
		{
			canCastOffensiveAbilityTwo = false;
			offensiveAbilityTimerTwo = 2.5f;   //if low mana wait 2.5s then try again
			return;
		}

		if (offensiveAbilityTwo.hasStatusEffects)
			CastEffect(offensiveAbilityTwo);
		else
		{
			if (offensiveAbilityTwo.isProjectile)
				CastDirectionalAbility(offensiveAbilityTwo);
			else if (offensiveAbilityTwo.isAOE)
				CastAoeAbility(offensiveAbilityTwo);
		}

		canCastOffensiveAbilityTwo = false;
		offensiveAbilityTimerTwo = offensiveAbilityTwo.abilityCooldown;
		return;
	}
}
