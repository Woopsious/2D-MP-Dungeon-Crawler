using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	public static SaveManager Instance;

	public static event Action OnGameLoad;

	[SerializeReference] public InventoryData InventoryData = new InventoryData();

	public void Start()
	{
		Instance = this;
	}

	public void SaveDataToJson()
	{
		InventoryData.hasRecievedStartingItems = PlayerInventoryManager.Instance.hasRecievedStartingItems;

		foreach (GameObject slot in PlayerInventoryUi.Instance.InventorySlots)
		{
			if (slot.GetComponent<InventorySlot>().IsSlotEmpty()) continue;
			else
			{
				InventoryItem inventoryItem = slot.GetComponent<InventorySlot>().itemInSlot;
				InventoryItemData itemData = new()
				{
					weaponBaseRef = inventoryItem.weaponBaseRef,
					armorBaseRef = inventoryItem.armorBaseRef,
					accessoryBaseRef = inventoryItem.accessoryBaseRef,
					consumableBaseRef = inventoryItem.consumableBaseRef,

					itemLevel = inventoryItem.itemLevel,
					rarity = (InventoryItemData.Rarity)inventoryItem.rarity,

					inventorySlotIndex = inventoryItem.inventorySlotIndex,
					isStackable = inventoryItem.isStackable,
					maxStackCount = inventoryItem.maxStackCount,
					currentStackCount = inventoryItem.currentStackCount
				};

				InventoryData.InventoryItems.Add(itemData);
			}
		}

		string inventoryData = JsonUtility.ToJson(InventoryData);
		string filePath = Application.persistentDataPath + "/InventoryData.json";
		System.IO.File.WriteAllText(filePath, inventoryData);

		Debug.Log(filePath);
	}
	public void LoadDataToJson()
	{
		string filePath = Application.persistentDataPath + "/InventoryData.json";
		string inventoryData = System.IO.File.ReadAllText(filePath);
		InventoryData = JsonUtility.FromJson<InventoryData>(inventoryData);

		OnGameLoad?.Invoke();
	}
}

[System.Serializable]
public class InventoryData
{
	public bool hasRecievedStartingItems;

	public List<InventoryItemData> InventoryItems = new List<InventoryItemData>();
}

[System.Serializable]
public class EquipmentData
{
	public List<InventoryItem> EquipmentItems = new List<InventoryItem>();
}

[System.Serializable]
public class InventoryItemData
{
	[Header("Item Base Ref")]
	public SOWeapons weaponBaseRef;
	public SOArmors armorBaseRef;
	public SOAccessories accessoryBaseRef;
	public SOConsumables consumableBaseRef;

	[Header("Item Info")]
	public int itemLevel;
	public Rarity rarity;
	public enum Rarity
	{
		isCommon, isRare, isEpic, isLegendary
	}

	[Header("Item Dynamic Info")]
	public int inventorySlotIndex;
	public bool isStackable;
	public int maxStackCount;
	public int currentStackCount;
}