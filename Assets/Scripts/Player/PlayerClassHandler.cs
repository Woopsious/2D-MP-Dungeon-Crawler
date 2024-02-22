using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClassHandler : EntityClassHandler
{
	private void Start()
	{
		Initilize();
	}

	private void OnEnable()
	{
		ClassesUi.OnClassChange += OnClassChanges;
		ClassesUi.OnClassReset += OnClassReset;
		ClassesUi.OnNewStatBonusUnlock += UnlockStatBoost;
		ClassesUi.OnNewAbilityUnlock += UnlockAbility;
	}
	private void OnDisable()
	{
		ClassesUi.OnClassChange -= OnClassChanges;
		ClassesUi.OnClassReset -= OnClassReset;
		ClassesUi.OnNewStatBonusUnlock -= UnlockStatBoost;
		ClassesUi.OnNewAbilityUnlock -= UnlockAbility;
	}
	protected override void OnClassReset(SOClasses currentClass)
	{
		base.OnClassReset(currentClass);

		foreach (GameObject abilitySlot in PlayerInventoryUi.Instance.LearntAbilitySlots)
		{
			if (abilitySlot.transform.GetChild(0) != null)
				Destroy(abilitySlot.transform.GetChild(0).gameObject);
		}

		foreach (GameObject equippedAbility in PlayerHotbarUi.Instance.AbilitySlots)
		{
			if (equippedAbility.transform.GetChild(0) != null)
				Destroy(equippedAbility.transform.GetChild(0).gameObject);
		}
	}

	protected override void UnlockStatBoost(SOClassStatBonuses statBoost)
	{
		base.UnlockStatBoost(statBoost);
		UpdateClassTreeUi();
	}
	protected override void UnlockAbility(SOClassAbilities ability)
	{
		base.UnlockAbility(ability);
		AddNewAbilityToLearntOnes();
		UpdateClassTreeUi();
	}

	private void AddNewAbilityToLearntOnes()
	{
		/*
		for (int i = 0; i < PlayerInventoryUi.Instance.LearntAbilitySlots.Count; i++)
		{
			InventorySlotUi inventorySlot = LearntAbilitySlots[i].GetComponent<InventorySlotUi>();

			if (inventorySlot.IsSlotEmpty())
			{
				item.inventorySlotIndex = i;
				item.transform.SetParent(inventorySlot.transform);
				item.SetTextColour();
				inventorySlot.itemInSlot = item;
				inventorySlot.UpdateSlotSize();

				return;
			}
		}
		*/
	}

	private void UpdateClassTreeUi()
	{
		if (ClassesUi.Instance == null)
			Debug.LogError("ClassesUi component instance not set, ignore if intentional");
		else
			ClassesUi.Instance.UpdateNodesInClassTree(this);
	}
}
