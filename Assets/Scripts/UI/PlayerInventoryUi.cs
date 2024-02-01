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

	[Header("Equipment")]
	public GameObject EquipmentUi;
	public GameObject weaponEquipmentSlot;
	public GameObject offHandEquipmentSlot;

	public GameObject helmetEquipmentSlot;
	public GameObject chestpieceEquipmentSlot;
	public GameObject legsEquipmentSlot;

	public GameObject artifactSlot;
	public GameObject necklassEquipmentSlot;
	public GameObject ringEquipmentSlotOne;
	public GameObject ringEquipmentSlotTwo;

	[Header("Inventory")]
	public GameObject InventoryUi;
	public List<GameObject> InventorySlots = new List<GameObject>();

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		InventoryPanelUi.gameObject.SetActive(false);
	}

	public void ShowHideInventoryKeybind()
	{
		if (InventoryPanelUi.gameObject.activeInHierarchy)
			InventoryPanelUi.gameObject.SetActive(false);
		else
			InventoryPanelUi.gameObject.SetActive(true);
	}
}