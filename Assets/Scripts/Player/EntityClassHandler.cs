using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EntityClassHandler : MonoBehaviour
{
	[HideInInspector] public EntityStats entityStats;

	public List<SOClassAbilities> unlockedAbilitiesList = new List<SOClassAbilities>();
	public List<SOClassStatBonuses> unlockedStatBoostList = new List<SOClassStatBonuses>();

	public SOClasses currentEntityClass;
	public GameObject itemPrefab;

	[Header("Bonuses Provided By Class")]
	public int classHealth;
	public int classMana;

	public int classPhysicalResistance;
	public int classPoisonResistance;
	public int classFireResistance;
	public int classIceResistance;

	public int physicalDamagePercentage;
	public int poisonDamagePercentage;
	public int fireDamagePercentage;
	public int iceDamagePercentage;

	public event Action<EntityClassHandler> OnClassChange;
	public event Action<SOClassStatBonuses> OnStatUnlock;
	public event Action<SOClassAbilities> OnAbilityUnlock;

	private void Start()
	{
		Initilize();
	}

	private void OnEnable()
	{
		PlayerClassesUi.OnClassChange += OnClassChanges;
	}
	private void OnDisable()
	{
		PlayerClassesUi.OnClassChange -= OnClassChanges;
	}

	private void Initilize()
	{
		entityStats = GetComponent<EntityStats>();
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

	public void AddStatBonuses(SOClassStatBonuses statBoost)
	{

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
}
