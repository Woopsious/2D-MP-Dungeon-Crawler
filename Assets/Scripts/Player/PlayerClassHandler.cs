using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClassHandler : EntityClassHandler
{
	private int abilitySlots;

	private void OnEnable()
	{
		PlayerClassesUi.OnClassChanges += UpdateClass;
		PlayerClassesUi.OnNewStatBonusUnlock += UnlockStatBoost;
		PlayerClassesUi.OnNewAbilityUnlock += UnlockAbility;
		PlayerClassesUi.OnRefundStatBonusUnlock += RefundStatBoost;
		PlayerClassesUi.OnRefundAbilityUnlock += RefundAbility;

		//PlayerEventManager.OnPlayerLevelUpEvent += UpdateAbilitySlotsOnLevelUp;
	}
	private void OnDisable()
	{
		PlayerClassesUi.OnClassChanges -= UpdateClass;
		PlayerClassesUi.OnNewStatBonusUnlock -= UnlockStatBoost;
		PlayerClassesUi.OnNewAbilityUnlock -= UnlockAbility;
		PlayerClassesUi.OnRefundStatBonusUnlock -= RefundStatBoost;
		PlayerClassesUi.OnRefundAbilityUnlock -= RefundAbility;

		//PlayerEventManager.OnPlayerLevelUpEvent -= UpdateAbilitySlotsOnLevelUp;
	}

	//player class events
	protected override void UpdateClass(SOClasses newPlayerClass)
	{
		base.UpdateClass(newPlayerClass);
		//UpdateMaxAbilitySlots();
		GetComponent<PlayerInventoryHandler>().TrySpawnStartingItems(newPlayerClass);
	}
	protected override void UnlockStatBoost(SOClassStatBonuses statBoost)
	{
		base.UnlockStatBoost(statBoost);
		UpdateClassTreeUi();
	}
	protected override void UnlockAbility(SOAbilities ability)
	{
		base.UnlockAbility(ability);
		UpdateClassTreeUi();
	}
	protected override void RefundStatBoost(SOClassStatBonuses statBoost)
	{
		base.RefundStatBoost(statBoost);
		UpdateClassTreeUi();
	}
	protected override void RefundAbility(SOAbilities ability)
	{
		base.RefundAbility(ability);
		UpdateClassTreeUi();
	}

	/*
	private void UpdateAbilitySlotsOnLevelUp(EntityStats playerStats)
	{
		UpdateMaxAbilitySlots();
	}
	protected void UpdateMaxAbilitySlots()
	{
		if (currentEntityClass == null) return;
		abilitySlots = currentEntityClass.baseClassAbilitySlots;

		foreach (AbilitySlots abilitySlot in currentEntityClass.spellSlotsPerLevel)
		{
			if (entityStats.entityLevel >= abilitySlot.LevelRequirement)
				abilitySlots += abilitySlot.AbilitySlotsPerLevel;
		}
	}
	*/

	private void UpdateClassTreeUi()
	{
		if (PlayerClassesUi.Instance == null)
			Debug.LogError("ClassesUi component instance not set, ignore if intentional");
		else
			PlayerClassesUi.Instance.UpdateNodesInClassTree(GetComponent<EntityStats>());
	}
}
