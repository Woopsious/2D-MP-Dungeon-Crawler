using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventoryUi : MonoBehaviour
{
	public static PlayerInventoryUi Instance;

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

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		InventoryPanelUi.gameObject.SetActive(false);
	}

	public void HideShowInventory()
	{
		if (InventoryPanelUi.gameObject.activeInHierarchy)
			InventoryPanelUi.gameObject.SetActive(false);
		else
			InventoryPanelUi.gameObject.SetActive(true);
	}
}
