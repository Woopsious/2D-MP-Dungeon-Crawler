using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventoryUi : MonoBehaviour
{
	public static PlayerInventoryUi Instance;

	public GameObject ItemUiPrefab;
	public GameObject InventoryPanelUi;

	[Header("Inventory items")]
	public GameObject LearntAbilitiesTextObj;
	public GameObject LearntAbilitiesUi;
	public List<GameObject> LearntAbilitySlots = new List<GameObject>();

	[Header("Inventory items")]
	public GameObject InventoryUi;
	public List<GameObject> InventorySlots = new List<GameObject>();

	[Header("Equipment items")]
	public GameObject EquipmentUi;
	public List<GameObject> EquipmentSlots = new List<GameObject>();

	public GameObject weaponEquipmentSlot;
	public GameObject offHandEquipmentSlot;
	public GameObject helmetEquipmentSlot;
	public GameObject chestpieceEquipmentSlot;
	public GameObject legsEquipmentSlot;

	public GameObject artifactSlot;
	public GameObject necklassEquipmentSlot;
	public GameObject ringEquipmentSlotOne;
	public GameObject ringEquipmentSlotTwo;

	private void Awake()
	{
		Instance = this;
	}
	private void Start()
	{
		InventoryPanelUi.SetActive(false);
	}
	private void OnEnable()
	{
		ClassesUi.OnClassReset += OnClassReset;
		ClassesUi.OnNewAbilityUnlock += AddNewUnlockedAbility;
	}
	private void OnDisable()
	{
		ClassesUi.OnClassReset -= OnClassReset;
		ClassesUi.OnNewAbilityUnlock -= AddNewUnlockedAbility;
	}

	//CLASSES + ABILITIES
	//reset/clear any learnt abilities from learnt abilities ui
	private void OnClassReset(SOClasses currentClass)
	{
		foreach (GameObject abilitySlot in LearntAbilitySlots)
		{
			if (abilitySlot.transform.GetChild(0) != null)
				Destroy(abilitySlot.transform.GetChild(0).gameObject);
		}
	}
	//Adding new abilities to Ui
	public void AddNewUnlockedAbility(SOClassAbilities newAbility)
	{
		/*
		for (int i = 0; i < LearntAbilitySlots.Count; i++)
		{
			InventorySlotUi inventorySlot = LearntAbilitySlots[i].GetComponent<InventorySlotUi>();

			//instantiate here

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

	//ITEMS
	//Adding new items to Ui

	//UI CHANGES
	public void ShowHideInventoryKeybind()
	{
		if (InventoryPanelUi.activeInHierarchy)
			HideInventory();
		else
			ShowInventory();
	}
	public void ShowInventory()
	{
		InventoryPanelUi.SetActive(true);
	}
	public void HideInventory()
	{
		InventoryPanelUi.SetActive(false);
	}

	public void ShowHideLearntAbilitiesKeybind()
	{
		if (LearntAbilitiesUi.activeInHierarchy)
			ShowLearntAbilities();
		else
			HideLearntAbilities();
	}
	public void ShowLearntAbilities()
	{
		LearntAbilitiesTextObj.SetActive(true);
		LearntAbilitiesUi.SetActive(true);
	}
	public void HideLearntAbilities()
	{
		LearntAbilitiesTextObj.SetActive(false);
		LearntAbilitiesUi.SetActive(false);
	}
}
