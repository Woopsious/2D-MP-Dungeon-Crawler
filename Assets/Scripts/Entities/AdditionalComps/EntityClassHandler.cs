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
	public List<SOAbilities> unlockedAbilitiesList = new List<SOAbilities>();

	public event Action<SOClassStatBonuses> OnStatUnlock;
	public event Action<SOAbilities> OnAbilityUnlock;
	public event Action<SOClassStatBonuses> OnStatRefund;
	public event Action<SOAbilities> OnAbilityRefund;

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
		entityStats.abilityHandler.offensiveAbility = null;
		entityStats.abilityHandler.healingAbility = null;
		SetUpEntityEquippedAbilities();
	}
	private void SetUpEntityEquippedAbilities()
	{
		ChooseEntityAbilities();
	}
	private void ChooseEntityAbilities()
	{
		SOAbilities pickedAbility = PickOffensiveAbility();
		if (pickedAbility != null && !IsAbilityAlreadyEquipped(pickedAbility))
			entityStats.abilityHandler.offensiveAbility = pickedAbility;

		pickedAbility = PickHealingAbility();
        if (pickedAbility != null && !IsAbilityAlreadyEquipped(pickedAbility))
			entityStats.abilityHandler.healingAbility = pickedAbility;
	}
	private SOAbilities PickOffensiveAbility()
	{
		List<SOAbilities> offensiveAbilities = new List<SOAbilities>();
		foreach (SOAbilities ability in unlockedAbilitiesList)
		{
			if (ability.damageType != IDamagable.DamageType.isHealing)
				offensiveAbilities.Add(ability);
		}

		if (offensiveAbilities.Count == 0)
			return null;
		else return offensiveAbilities[Utilities.GetRandomNumber(offensiveAbilities.Count - 1)];
	}
	private SOAbilities PickHealingAbility()
	{
		List<SOAbilities> healingAbilities = new List<SOAbilities>();
		foreach (SOAbilities ability in unlockedAbilitiesList)
		{
			if (ability.damageType == IDamagable.DamageType.isHealing)
				healingAbilities.Add(ability);
		}

		if (healingAbilities.Count == 0)
			return null;
		else return healingAbilities[Utilities.GetRandomNumber(healingAbilities.Count - 1)];
	}

	//duplicate ability check
	private bool IsAbilityAlreadyEquipped(SOAbilities abilityToCheck)
	{
		if (abilityToCheck == entityStats.abilityHandler.offensiveAbility) return true;
		if (abilityToCheck == entityStats.abilityHandler.healingAbility) return true;
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
	protected virtual void UnlockAbility(SOAbilities ability)
	{
		unlockedAbilitiesList.Add(ability);
		OnAbilityUnlock?.Invoke(ability);
	}
	protected virtual void RefundStatBoost(SOClassStatBonuses statBoost)
	{
		unlockedStatBoostList.Remove(statBoost);
		OnStatRefund?.Invoke(statBoost);
	}
	protected virtual void RefundAbility(SOAbilities ability)
	{
		unlockedAbilitiesList.Remove(ability);
		OnAbilityRefund?.Invoke(ability);
	}
}
