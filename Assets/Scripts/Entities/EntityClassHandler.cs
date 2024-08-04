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
	///	
	///	<summery>
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
