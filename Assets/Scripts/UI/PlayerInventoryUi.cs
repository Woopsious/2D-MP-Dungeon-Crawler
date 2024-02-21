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

	[Header("Hotbar items")]
	public GameObject ConsumablesUi;
	public List<GameObject> ConsumableSlots = new List<GameObject>();

	public GameObject AbilitiesUi;
	public List<GameObject> AbilitySlots = new List<GameObject>();

	private void Awake()
	{
		Instance = this;
	}
	private void OnEnable()
	{
		ClassesUi.OnClassReset += OnClassReset;
		ClassesUi.OnNewAbilityUnlock += OnAbilityUnlock;
	}
	private void OnDisable()
	{
		ClassesUi.OnClassReset -= OnClassReset;
		ClassesUi.OnNewAbilityUnlock -= OnAbilityUnlock;
	}
	private void OnClassReset(SOClasses currentClass)
	{
		foreach (GameObject abilitySlot in LearntAbilitySlots)
		{
			if (abilitySlot.transform.GetChild(0) != null)
				Destroy(abilitySlot.transform.GetChild(0).gameObject);
		}

		foreach (GameObject equippedAbility in AbilitySlots)
		{
			if (equippedAbility.transform.GetChild(0) != null)
				Destroy(equippedAbility.transform.GetChild(0).gameObject);
		}
	}
	private void OnAbilityUnlock(SOClassAbilities newClassAbility)
	{
		/*
		for (int i = 0; i < LearntAbilitySlots.Count; i++)
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


	private void Start()
	{
		InventoryPanelUi.SetActive(false);
	}

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
