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
