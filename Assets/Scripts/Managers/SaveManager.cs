using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Windows;

public class SaveManager : MonoBehaviour
{
	public static SaveManager Instance;

	public static event Action OnGameLoad;

	[SerializeReference] public GameData GameData = new GameData();

	public GameObject saveSlotContainer;
	public GameObject saveSlotCardPrefab;

	private int maxSaveSlots = 20;
	private string playerDataPath;
	private string gameDataPath;
	private string directoryForCurrentGame;

	private void Awake()
	{
		Instance = this;
	}
	private void Start()
	{
		playerDataPath = Application.persistentDataPath + "/PlayerData";
		gameDataPath = Application.persistentDataPath + "/GameData/Save";
	}

	public void ReloadSaveSlots()
	{
		foreach (Transform child in saveSlotContainer.transform)
			Destroy(child.gameObject);

		for (int i = 0; i < maxSaveSlots; i++)
		{
			GameObject go = Instantiate(saveSlotCardPrefab, saveSlotContainer.transform);
			SaveSlotManager saveSlot = go.GetComponent<SaveSlotManager>();

			if (DoesDirectoryExist(gameDataPath + i))
			{
				LoadDataToJson(gameDataPath + i);
				saveSlot.Name = GameData.name;
				saveSlot.Level = GameData.level;
				saveSlot.Date = GameData.date;
				saveSlot.Initilize(gameDataPath + i, false);
			}
			else
			{
				saveSlot.Name = "Empty";
				saveSlot.Level = "Empty";
				saveSlot.Date = "Empty";
				saveSlot.Initilize(gameDataPath + i, true);
			}
		}
	}

	/// <summary>
	/// public methods are called from buttons on ui that pass in corrisponding directory path, run checks,
	/// then save/load Json data to disk, or delete Json data
	/// </summary>
	public void CreateNewGameSave()
	{
		GameData.name = Utilities.GetRandomNumber(1000).ToString();
		GameData.level = Utilities.GetRandomNumber(50).ToString();
		GameData.date = System.DateTime.Now.ToString();
	}
	public void SaveGameData(string directory)
	{
		if (DoesDirectoryExist(directory))
		{
			if (DoesFileExist(directory, "/GameData.json"))
				System.IO.File.Delete(directory + "/GameData.json");
		}
		else
			System.IO.Directory.CreateDirectory(directory);

		directoryForCurrentGame = directory;
		SaveDataToJson(directoryForCurrentGame);
	}
	public void LoadGameData(string directory)
	{
		if (!DoesDirectoryExist(directory)) return;
		if (!DoesFileExist(directory, "/GameData.json")) return;

		directoryForCurrentGame = directory;
		LoadDataToJson(directoryForCurrentGame);
	}
	public void DeleteGameData(string directory)
	{
		if (!DoesDirectoryExist(directory)) return;
		if (!DoesFileExist(directory, "/GameData.json")) return;

		DeleteJsonFile(directory);
	}

	private void SaveDataToJson(string directory)
	{
		//SavePlayerInfoData();
		//SavePlayerInventoryData();
		//SavePlayerEquipmentData();

		string filePath = directory + "/GameData.json";
		string inventoryData = JsonUtility.ToJson(GameData);
        System.IO.File.WriteAllText(filePath, inventoryData);

		ReloadSaveSlots();
	}
	private void LoadDataToJson(string directory)
	{
		string filePath = directory + "/GameData.json";
		string inventoryData = System.IO.File.ReadAllText(filePath);
		GameData = JsonUtility.FromJson<GameData>(inventoryData);

		OnGameLoad?.Invoke();
	}
	private void DeleteJsonFile(string directory)
	{
		System.IO.File.Delete(directory + "/GameData.json");
		System.IO.Directory.Delete(directory);

		ReloadSaveSlots();
	}

	//data to save to disk
	private void SavePlayerInfoData()
	{
		EntityStats playerStats = FindObjectOfType<PlayerController>().GetComponent<EntityStats>();

		GameData.playerLevel = playerStats.entityLevel;
		GameData.playerCurrentExp = playerStats.GetComponent<PlayerExperienceHandler>().currentExp;
		GameData.playerCurrenthealth = playerStats.currentHealth;
		GameData.playerCurrentMana = playerStats.currentMana;
	}
	private void SavePlayerInventoryData()
	{
		GameData.hasRecievedStartingItems = PlayerInventoryManager.Instance.hasRecievedStartingItems;

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

				GameData.inventoryItems.Add(itemData);
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

				GameData.equipmentItems.Add(itemData);
			}
		}
	}

	//bool checks
	private bool DoesDirectoryExist(string path)
	{
		if (System.IO.Directory.Exists(path))
			return true;
		else
			return false;
	}
	private bool DoesFileExist(string path, string file)
	{
		if (System.IO.File.Exists(path + file))
			return true;
		else
			return false;
	}
}
[System.Serializable]
public class PlayerData
{
	//keybinds settings
	//audio settings
}

[System.Serializable]
public class GameData
{
	public string name;
	public string level;
	public string date;

	public int playerLevel;
	public int playerCurrentExp;
	public int playerCurrenthealth;
	public int playerCurrentMana;

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