using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;

public class EntityClassHandler : MonoBehaviour
{
	[HideInInspector] public EntityStats entityStats;
	[Header("Current Class")]
	public SOClasses currentEntityClass;

	public List<SOClassStatBonuses> newUnlockedStatBoostList = new List<SOClassStatBonuses>();
	public List<SOClassAbilities> newUnlockedAbilitiesList = new List<SOClassAbilities>();

	public event Action<EntityClassHandler> OnClassChange;
	public event Action<SOClassStatBonuses> OnStatUnlock;
	public event Action<SOClassAbilities> OnAbilityUnlock;
	public event Action<SOClassStatBonuses> OnStatRefund;
	public event Action<SOClassAbilities> OnAbilityRefund;


	private void Awake()
	{
		entityStats = GetComponent<EntityStats>();
	}
	public void SetEntityClass()
	{
		int num = Utilities.GetRandomNumber(entityStats.entityBaseStats.possibleClassesList.Count - 1);
		currentEntityClass = entityStats.entityBaseStats.possibleClassesList[num];

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
	}

	///	<summery>
	///	remove all stat boosts currently applied to player, also unequip any equipped abilities player has
	///	then clear unlockedLists
	///	
	/// re add all stat boost player currently has the valid level for, leaving abilities to player to reunlock
	///	<summery>
	protected virtual void OnClassChanges(SOClasses newClass)
	{
		OnClassChange?.Invoke(this);

		newUnlockedStatBoostList.Clear();
		newUnlockedAbilitiesList.Clear();

		currentEntityClass = newClass;
	}

	protected virtual void UnlockStatBoost(SOClassStatBonuses statBoost)
	{
		newUnlockedStatBoostList.Add(statBoost);
		OnStatUnlock?.Invoke(statBoost);
	}
	protected virtual void UnlockAbility(SOClassAbilities ability)
	{
		newUnlockedAbilitiesList.Add(ability);
		OnAbilityUnlock?.Invoke(ability);
	}
	protected virtual void RefundStatBoost(SOClassStatBonuses statBoost)
	{
		newUnlockedStatBoostList.Remove(statBoost);
		OnStatRefund?.Invoke(statBoost);
	}
	protected virtual void RefundAbility(SOClassAbilities ability)
	{
		newUnlockedAbilitiesList.Remove(ability);
		OnAbilityRefund?.Invoke(ability);
	}
}
