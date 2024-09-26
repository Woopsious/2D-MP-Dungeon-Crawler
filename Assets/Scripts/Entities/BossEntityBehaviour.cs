using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEntityBehaviour : EntityBehaviour
{
	[Header("Boss Abilities")]
	[Header("Ability One")]
	public SOClassAbilities abilityOne;
	public bool canCastAbilityOne;
	public float abilityTimerOneCounter;

	[Header("Ability Two")]
	public SOClassAbilities abilityTwo;
	public bool canCastAbilityTwo;
	public float abilityTimerTwoCounter;

	[Header("Ability Three")]
	public SOClassAbilities abilityThree;
	public bool canCastAbilityThree;
	public float abilityTimerThreeCounter;

	protected override void Update()
	{
		base.Update();

		AbilityTimerOne();
		AbilityTimerTwo();
		AbilityTimerThree();
	}

	protected override void Initilize()
	{
		base.Initilize();

		SOBossEntityBehaviour behaviour = (SOBossEntityBehaviour)entityBehaviour;
		abilityOne = behaviour.abilityOne;
		abilityTwo = behaviour.abilityTwo;
		abilityThree = behaviour.abilityThree;
	}

	//additional abilities
	public void CastBossAbilityOne()
	{
		if (!canCastAbilityOne || abilityOne == null) return;
		if (playerTarget == null) return;

		if (!HasEnoughManaToCast(abilityOne))
		{
			canCastAbilityOne = false;
			abilityTimerOneCounter = 2.5f;   //if low mana wait 2.5s then try again
			return;
		}

		if (abilityOne.hasStatusEffects)
			CastEffect(abilityOne);
		else
		{
			if (abilityOne.isProjectile)
				CastDirectionalAbility(abilityOne);
			else if (abilityOne.isAOE)
				CastAoeAbility(abilityOne);
		}

		canCastAbilityOne = false;
		abilityTimerOneCounter = abilityOne.abilityCooldown;
		return;
	}
	public void CastBossAbilityTwo()
	{
		if (!canCastAbilityTwo || abilityTwo == null) return;
		if (playerTarget == null) return;

		if (!HasEnoughManaToCast(abilityTwo))
		{
			canCastAbilityTwo = false;
			abilityTimerTwoCounter = 2.5f;   //if low mana wait 2.5s then try again
			return;
		}

		if (abilityTwo.hasStatusEffects)
			CastEffect(abilityTwo);
		else
		{
			if (abilityTwo.isProjectile)
				CastDirectionalAbility(abilityTwo);
			else if (abilityTwo.isAOE)
				CastAoeAbility(abilityTwo);
		}

		canCastAbilityTwo = false;
		abilityTimerTwoCounter = abilityTwo.abilityCooldown;
		return;
	}
	public void CastBossAbilityThree()
	{
		if (!canCastAbilityThree || abilityThree == null) return;
		if (playerTarget == null) return;

		if (!HasEnoughManaToCast(abilityThree))
		{
			canCastAbilityThree = false;
			abilityTimerThreeCounter = 2.5f;   //if low mana wait 2.5s then try again
			return;
		}

		if (abilityThree.hasStatusEffects)
			CastEffect(abilityThree);
		else
		{
			if (abilityThree.isProjectile)
				CastDirectionalAbility(abilityThree);
			else if (abilityThree.isAOE)
				CastAoeAbility(abilityThree);
		}

		canCastAbilityThree = false;
		abilityTimerThreeCounter = abilityThree.abilityCooldown;
		return;
	}

	//timers
	private void AbilityTimerOne()
	{
		if (canCastAbilityOne) return;

		abilityTimerOneCounter -= Time.deltaTime;

		if (abilityTimerOneCounter <= 0)
			canCastAbilityOne = true;
	}
	private void AbilityTimerTwo()
	{
		if (canCastAbilityTwo) return;

		abilityTimerTwoCounter -= Time.deltaTime;

		if (abilityTimerTwoCounter <= 0)
			canCastAbilityTwo = true;
	}
	private void AbilityTimerThree()
	{
		if (canCastAbilityThree) return;

		abilityTimerThreeCounter -= Time.deltaTime;

		if (abilityTimerThreeCounter <= 0)
			canCastAbilityThree = true;
	}
}