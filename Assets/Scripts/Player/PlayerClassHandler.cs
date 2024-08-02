using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClassHandler : EntityClassHandler
{
	public int abilitySlots;

	private void OnEnable()
	{
		PlayerClassesUi.OnClassChange += OnClassUpdates;
		PlayerClassesUi.OnClassReset += OnClassUpdates;
		PlayerClassesUi.OnNewStatBonusUnlock += UnlockStatBoost;
		PlayerClassesUi.OnNewAbilityUnlock += UnlockAbility;
		PlayerClassesUi.OnRefundStatBonusUnlock += RefundStatBoost;
		PlayerClassesUi.OnRefundAbilityUnlock += RefundAbility;

		EventManager.OnPlayerLevelUpEvent += UpdateAbilitySlotsOnLevelUp;
	}
	private void OnDisable()
	{
		PlayerClassesUi.OnClassChange -= OnClassUpdates;
		PlayerClassesUi.OnClassReset -= OnClassUpdates;
		PlayerClassesUi.OnNewStatBonusUnlock -= UnlockStatBoost;
		PlayerClassesUi.OnNewAbilityUnlock -= UnlockAbility;
		PlayerClassesUi.OnRefundStatBonusUnlock -= RefundStatBoost;
		PlayerClassesUi.OnRefundAbilityUnlock -= RefundAbility;

		EventManager.OnPlayerLevelUpEvent -= UpdateAbilitySlotsOnLevelUp;
	}

	protected override void OnClassUpdates(SOClasses newPlayerClass)
	{
		base.OnClassUpdates(newPlayerClass);
		UpdateMaxAbilitySlots();
		GetComponent<PlayerInventoryHandler>().TrySpawnStartingItems(newPlayerClass);
	}
	protected override void UnlockStatBoost(SOClassStatBonuses statBoost)
	{
		base.UnlockStatBoost(statBoost);
		UpdateClassTreeUi();
	}
	protected override void UnlockAbility(SOClassAbilities ability)
	{
		base.UnlockAbility(ability);
		UpdateClassTreeUi();
	}
	protected override void RefundStatBoost(SOClassStatBonuses statBoost)
	{
		base.RefundStatBoost(statBoost);
		UpdateClassTreeUi();
	}
	protected override void RefundAbility(SOClassAbilities ability)
	{
		base.RefundAbility(ability);
		UpdateClassTreeUi();
	}

	private void UpdateAbilitySlotsOnLevelUp(EntityStats playerStats)
	{
		UpdateMaxAbilitySlots();
	}
	private void UpdateMaxAbilitySlots()
	{
		if (currentEntityClass == null) return;

		abilitySlots = currentEntityClass.baseClassAbilitySlots;

		foreach (SpellSlots spellSlot in currentEntityClass.spellSlotsPerLevel)
		{
			if (entityStats.entityLevel >= spellSlot.LevelRequirement)
				abilitySlots += spellSlot.SpellSlotsPerLevel;
		}
	}
	private void UpdateClassTreeUi()
	{
		if (PlayerClassesUi.Instance == null)
			Debug.LogError("ClassesUi component instance not set, ignore if intentional");
		else
			PlayerClassesUi.Instance.UpdateNodesInClassTree(GetComponent<EntityStats>());
	}
}
