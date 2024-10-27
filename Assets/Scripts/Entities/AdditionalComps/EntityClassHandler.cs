using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityClassHandler : MonoBehaviour
{
	[HideInInspector] public EntityStats entityStats;
	[Header("Current Class")]
	public SOClasses currentEntityClass;

	public List<SOClassStatBonuses> unlockedStatBoostList = new List<SOClassStatBonuses>();
	public List<SOClassAbilities> unlockedAbilitiesList = new List<SOClassAbilities>();

	public event Action<SOClassStatBonuses> OnStatUnlock;
	public event Action<SOClassAbilities> OnAbilityUnlock;
	public event Action<SOClassStatBonuses> OnStatRefund;
	public event Action<SOClassAbilities> OnAbilityRefund;

	private void Awake()
	{
		entityStats = GetComponent<EntityStats>();
	}

	//set classes + abilities of non players
	public void SetEntityClass()
	{
		int num = Utilities.GetRandomNumber(entityStats.statsRef.possibleClassesList.Count - 1);
		currentEntityClass = entityStats.statsRef.possibleClassesList[num];

		foreach (ClassStatUnlocks statBonuses in currentEntityClass.classStatBonusList)
		{
			if (entityStats.entityLevel < statBonuses.LevelRequirement) continue;
			UnlockStatBoost(statBonuses.unlock);
		}
		foreach (ClassAbilityUnlocks ability in currentEntityClass.classAbilitiesOffensiveList)
		{
			if (entityStats.entityLevel < ability.LevelRequirement) continue;
			UnlockAbility(ability.unlock);
		}
		foreach (ClassAbilityUnlocks ability in currentEntityClass.classAbilitiesEffectsList)
		{
			if (entityStats.entityLevel < ability.LevelRequirement) continue;
			UnlockAbility(ability.unlock);
		}
		foreach (ClassAbilityUnlocks ability in currentEntityClass.classAbilitiesHealingList)
		{
			if (entityStats.entityLevel < ability.LevelRequirement) continue;
			UnlockAbility(ability.unlock);
		}

		SetUpEntityEquippedAbilities();
	}
	public void RerollEquippedAbilities()
	{
		entityStats.entityBehaviour.offensiveAbility = null;
		entityStats.entityBehaviour.healingAbility = null;
		SetUpEntityEquippedAbilities();
	}
	private void SetUpEntityEquippedAbilities()
	{
		ChooseEntityAbilities();
	}
	private void ChooseEntityAbilities()
	{
		SOClassAbilities pickedAbility = PickOffensiveAbility();
		if (pickedAbility != null && !IsAbilityAlreadyEquipped(pickedAbility))
			entityStats.entityBehaviour.offensiveAbility = pickedAbility;

		pickedAbility = PickHealingAbility();
        if (pickedAbility != null && !IsAbilityAlreadyEquipped(pickedAbility))
			entityStats.entityBehaviour.healingAbility = pickedAbility;
	}
	private SOClassAbilities PickOffensiveAbility()
	{
		List<SOClassAbilities> offensiveAbilities = new List<SOClassAbilities>();
		foreach (SOClassAbilities ability in unlockedAbilitiesList)
		{
			if (ability.damageType != SOClassAbilities.DamageType.isHealing)
				offensiveAbilities.Add(ability);
		}

		if (offensiveAbilities.Count == 0)
			return null;
		else return offensiveAbilities[Utilities.GetRandomNumber(offensiveAbilities.Count - 1)];
	}
	private SOClassAbilities PickHealingAbility()
	{
		List<SOClassAbilities> healingAbilities = new List<SOClassAbilities>();
		foreach (SOClassAbilities ability in unlockedAbilitiesList)
		{
			if (ability.damageType == SOClassAbilities.DamageType.isHealing)
				healingAbilities.Add(ability);
		}

		if (healingAbilities.Count == 0)
			return null;
		else return healingAbilities[Utilities.GetRandomNumber(healingAbilities.Count - 1)];
	}

	//duplicate ability check
	private bool IsAbilityAlreadyEquipped(SOClassAbilities abilityToCheck)
	{
		if (abilityToCheck == entityStats.entityBehaviour.offensiveAbility) return true;
		if (abilityToCheck == entityStats.entityBehaviour.healingAbility) return true;
		return false;
	}

	//entity class updates
	protected virtual void UpdateClass(SOClasses newClass)
	{
		for (int i = unlockedStatBoostList.Count - 1; i >= 0; i--)
			RefundStatBoost(unlockedStatBoostList[i]);
		for (int i = unlockedAbilitiesList.Count - 1; i >= 0; i--)
			RefundAbility(unlockedAbilitiesList[i]);

		unlockedStatBoostList.Clear();
		unlockedAbilitiesList.Clear();

		currentEntityClass = newClass;
	}

	//entity stat bonuses + ability event updates
	protected virtual void UnlockStatBoost(SOClassStatBonuses statBoost)
	{
		unlockedStatBoostList.Add(statBoost);
		OnStatUnlock?.Invoke(statBoost);
	}
	protected virtual void UnlockAbility(SOClassAbilities ability)
	{
		unlockedAbilitiesList.Add(ability);
		OnAbilityUnlock?.Invoke(ability);
	}
	protected virtual void RefundStatBoost(SOClassStatBonuses statBoost)
	{
		unlockedStatBoostList.Remove(statBoost);
		OnStatRefund?.Invoke(statBoost);
	}
	protected virtual void RefundAbility(SOClassAbilities ability)
	{
		unlockedAbilitiesList.Remove(ability);
		OnAbilityRefund?.Invoke(ability);
	}
}
