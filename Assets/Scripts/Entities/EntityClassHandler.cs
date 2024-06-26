using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
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


	private void Awake()
	{
		entityStats = GetComponent<EntityStats>();
		Initilize();
	}
	protected virtual void Initilize()
	{
		if (GetComponent<PlayerController>() == null)
			SetRandomClass();
	}
	private void SetRandomClass()
	{
		int num = Utilities.GetRandomNumber(possibleClassesList.Count);
		currentEntityClass = possibleClassesList[num];

		foreach (SOClassStatBonuses statBonuses in currentEntityClass.statBonusLists)
		{
			if (entityStats.entityLevel < statBonuses.nonPlayerLevelRequirement) continue;
			UnlockStatBoost(statBonuses);
		}
		foreach (SOClassAbilities ability in currentEntityClass.abilityLists)
		{
			if (entityStats.entityLevel < ability.nonPlayerLevelRequirement) continue;
			UnlockAbility(ability);
		}
	}

	///	<summery>
	///	remove all stat boosts currently applied to player, also unequip any equipped abilities player has
	///	then clear unlockedLists
	///	
	/// re add all stat boost player currently has the valid level for, leaving abilities to player to reunlock
	///	<summery>
	protected void OnClassChanges(SOClasses newPlayerClass)
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
	protected virtual void OnClassReset(SOClasses currentClass)
	{
		OnClassChanges(currentEntityClass);
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
}
