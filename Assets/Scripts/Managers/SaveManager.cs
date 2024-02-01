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
				//InventoryData.InventoryItems.Add(inventoryItem);
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

public class InventoryItemData
{
	public List<InventoryItem> InventoryItems = new List<InventoryItem>();

	[Header("Item Info")]
	public string itemName;
	public Sprite itemImage;
	public int itemPrice;
	public int itemLevel;
	public ItemType itemType;
	public enum ItemType
	{
		isConsumable, isWeapon, isArmor, isAccessory, isAbility
	}
	public Rarity rarity;
	public enum Rarity
	{
		isCommon, isRare, isEpic, isLegendary
	}

	[Header("Item Base Ref")]
	public SOWeapons weaponBaseRef;
	public SOArmors armorBaseRef;
	public SOAccessories accessoryBaseRef;
	public SOConsumables consumableBaseRef;

	[Header("Item Dynamic Info")]
	public int inventorySlotIndex;
	public bool isStackable;
	public int maxStackCount;
	public int currentStackCount;

	[Header("Class Restriction")]
	public ClassRestriction classRestriction;
	public enum ClassRestriction
	{
		light, medium, heavy
	}
}