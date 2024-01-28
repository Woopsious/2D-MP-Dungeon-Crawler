using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryUi : MonoBehaviour
{
	public static InventoryUi Instance;

	public GameObject ItemUiPrefab;
	[Header("Inventory")]
	public GameObject InventoryPanelUi;
	public List<GameObject> InventorySlots = new List<GameObject>();

	[Header("Equipment")]
	public GameObject weaponEquipmentSlot;
	public GameObject offHandEquipmentSlot;

	public GameObject helmetEquipmentSlot;
	public GameObject chestpieceEquipmentSlot;
	public GameObject legsEquipmentSlot;

	public GameObject artifactSlot;
	public GameObject necklassEquipmentSlot;
	public GameObject ringEquipmentSlotOne;
	public GameObject ringEquipmentSlotTwo;

	[Header("Hotbar")]
	public GameObject HotbarPanelUi;
	public GameObject consumableSlotOne;
	public GameObject consumableSlotTwo;

	public GameObject abilitySlotOne;
	public GameObject abilitySlotTwo;
	public GameObject abilitySlotThree;
	public GameObject abilitySlotFour;
	public GameObject abilitySlotFive;

	public void Awake()
	{
		Instance = this;
	}

	public void Start()
	{
		gameObject.SetActive(false);
	}

	public void HideShowInventory()
	{
		if (gameObject.activeInHierarchy)
			gameObject.SetActive(false);
		else
			gameObject.SetActive(true);
	}
}
