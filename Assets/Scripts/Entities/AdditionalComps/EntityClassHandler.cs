using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EntityClassHandler : NetworkBehaviour
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

	//set class
	public void AssignEntityRandomClass()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		int classIndex = Utilities.GetRandomNumber(entityStats.statsRef.possibleClassesList.Count - 1);

		if (MultiplayerManager.IsMultiplayer())
			SyncEntityClassForClientsRPC(classIndex);
		else
			SetEntityClass(classIndex);
	}

	[Rpc(SendTo.Everyone)]
	private void SyncEntityClassForClientsRPC(int classIndex)
	{
		SetEntityClass(classIndex);
	}
	private void SetEntityClass(int classIndex)
	{
		currentEntityClass = entityStats.statsRef.possibleClassesList[classIndex];

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
