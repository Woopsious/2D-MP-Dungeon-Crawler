using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	public static SaveManager Instance;

	public static event Action OnGameLoad;

	[SerializeReference] public InventoryData InventoryData = new InventoryData();

	private string gameDataPath;
	private string playerDataPath;

	private void Awake()
	{
		Instance = this;
	}
	private void Start()
	{
		gameDataPath = Application.persistentDataPath + "/GameData";
		playerDataPath = Application.persistentDataPath + "/PlayerData";
	}

	public void SaveDataToJson()
	{
		SavePlayerInventoryData();
		SavePlayerEquipmentData();

		if (DoesDirectoryExist(gameDataPath))
		{
			if (DoesFileExist(gameDataPath, "/InventoryData.json"))
				System.IO.File.Delete(gameDataPath + "/InventoryData.json");
		}
		else
			System.IO.Directory.CreateDirectory(gameDataPath);

		string inventoryData = JsonUtility.ToJson(InventoryData);
		string filePath = gameDataPath + "/InventoryData.json";
		System.IO.File.WriteAllText(filePath, inventoryData);

		Debug.Log(filePath);
	}
	public void LoadDataToJson()
	{
		if (!DoesDirectoryExist(gameDataPath)) return;
		if (!DoesFileExist(gameDataPath, "/InventoryData.json")) return;

		string filePath = gameDataPath + "/InventoryData.json";
		string inventoryData = System.IO.File.ReadAllText(filePath);
		InventoryData = JsonUtility.FromJson<InventoryData>(inventoryData);

		OnGameLoad?.Invoke();
	}

	//data to save to disk
	private void SavePlayerInventoryData()
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

				InventoryData.inventoryItems.Add(itemData);
			}
		}
	}
	private void SavePlayerEquipmentData()
	{
		foreach (GameObject slot in PlayerInventoryUi.Instance.EquipmentSlots)
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

				InventoryData.equipmentItems.Add(itemData);
			}
		}
	}

	//bool checks
	private bool DoesDirectoryExist(string path)
	{
		if (System.IO.Directory.Exists(path))
			return true;
		else
		{
			Debug.LogError("Directory Doesnt Exist");
			return false;
		}
	}
	private bool DoesFileExist(string path, string file)
	{
		if (System.IO.File.Exists(path + file))
			return true;
		else
		{
			Debug.LogError("File Doesnt Exist");
			return false;
		}
	}
}

[System.Serializable]
public class InventoryData
{
	public bool hasRecievedStartingItems;

	public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
	public List<InventoryItemData> equipmentItems = new List<InventoryItemData>();
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