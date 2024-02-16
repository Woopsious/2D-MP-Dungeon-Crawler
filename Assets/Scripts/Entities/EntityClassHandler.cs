using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EntityClassHandler : MonoBehaviour
{
	[HideInInspector] public EntityStats entityStats;

	[Header("Current Class")]
	public List<SOClasses> possibleClassesList = new List<SOClasses>();

	[Header("Current Class")]
	public SOClasses currentEntityClass;
	public GameObject itemPrefab;

	public List<SOClassAbilities> unlockedAbilitiesList = new List<SOClassAbilities>();
	public List<SOClassStatBonuses> unlockedStatBoostList = new List<SOClassStatBonuses>();

	public event Action<EntityClassHandler> OnClassChange;
	public event Action<SOClassStatBonuses> OnStatUnlock;
	public event Action<SOClassAbilities> OnAbilityUnlock;

	private void Start()
	{
		Initilize();
	}

	protected void Initilize()
	{
		entityStats = GetComponent<EntityStats>();

		if (GetComponent<PlayerController>() == null)
			SetRandomClass();
	}
	public void SetRandomClass()
	{
		int num = Utilities.GetRandomNumber(possibleClassesList.Count);
		currentEntityClass = possibleClassesList[num];

		foreach (SOClassStatBonuses statBonuses in currentEntityClass.statBonusLists)
		{
			if (entityStats.entityLevel < statBonuses.playerLevelRequirement) continue;

			unlockedStatBoostList.Add(statBonuses);
			AddStatBonuses(statBonuses);
		}
		foreach (SOClassAbilities ability in currentEntityClass.abilityLists)
		{
			if (entityStats.entityLevel < ability.playerLevelRequirement) continue;

			unlockedAbilitiesList.Add(ability);
		}
	}

	public void AddStatBonuses(SOClassStatBonuses statBoost)
	{
		OnStatUnlock?.Invoke(statBoost);
	}
	public void RemoveStatBonuses(SOClassStatBonuses statBoost)
	{

	}
	public void AddAbilities()
	{

	}
	public void RemoveAbilities()
	{

	}

	///	<summery>
	///	remove all stat boosts currently applied to player, also unequip any equipped abilities player has
	///	then clear unlockedLists
	///	
	/// re add all stat boost player currently has the valid level for, leaving abilities to player to reunlock
	///	<summery>
	public void OnClassChanges(SOClasses newPlayerClass)
	{
		OnClassChange?.Invoke(this);

		unlockedAbilitiesList.Clear();
		unlockedStatBoostList.Clear();

		currentEntityClass = newPlayerClass;
	}
	///	<summery>
	///	remove all stat boosts currently applied to player, also unequip any equipped abilities player has
	///	then clear unlockedLists
	///	<summery>
	public void OnClassReset()
	{
		OnClassChanges(currentEntityClass);
	}

	public void UnlockStatBoost(SOClassStatBonuses classStatBoost)
	{
		unlockedStatBoostList.Add(classStatBoost);
		OnStatUnlock?.Invoke(classStatBoost);
	}
	public void UnlockAbility(SOClassAbilities classAbility)
	{
		unlockedAbilitiesList.Add(classAbility);
		OnAbilityUnlock?.Invoke(classAbility);
	}
}
