using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;
using Scene = UnityEngine.SceneManagement.Scene;

public class SaveManager : MonoBehaviour
{
	public static SaveManager Instance;

	public static event Action RestoreData; //event for data that needs to be restored

	[SerializeReference] public GameData GameData = new GameData();
	[SerializeReference] public SlotData SlotData = new SlotData();

	public GameObject saveSlotCardPrefab;

	private int maxSaveSlots = 20;

	/// <summary>
	/// DIRECTORIES FOR DATA
	/// Application.persistentDataPath + "/PlayerData";
	/// Application.persistentDataPath + "/GameData/Save";
	/// Application.persistentDataPath + "/GameData/AutoSave";
	/// </summary>

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		GameManager.OnSceneChangeStart += AutoSaveData;
		GameManager.OnSceneChangeFinish += AutoLoadData;
	}
	private void OnDisable()
	{
		GameManager.OnSceneChangeStart -= AutoSaveData;
		GameManager.OnSceneChangeFinish -= AutoLoadData;
	}

	public void ReloadSaveSlots(GameObject saveSlotContainer)
	{
		foreach (Transform child in saveSlotContainer.transform)
			Destroy(child.gameObject);

		for (int i = 0; i < maxSaveSlots; i++)
		{
			GameObject go = Instantiate(saveSlotCardPrefab, saveSlotContainer.transform);
			SaveSlotManager saveSlot = go.GetComponent<SaveSlotManager>();

			if (DoesDirectoryExist(Application.persistentDataPath + "/GameData/Save" + i))
			{
				GrabJsonSlotData(Application.persistentDataPath + "/GameData/Save" + i);
				saveSlot.Name = SlotData.name;
				saveSlot.Level = SlotData.level;
				saveSlot.Date = SlotData.date;
				saveSlot.Initilize(Application.persistentDataPath + "/GameData/Save" + i, false, false);
			}
			else
			{
				saveSlot.Name = "Empty";
				saveSlot.Level = "Empty";
				saveSlot.Date = "Empty";
				saveSlot.Initilize(Application.persistentDataPath + "/GameData/Save" + i, false, true);
			}
		}
	}
	public void ReloadAutoSaveSlot(GameObject saveSlotContainer)
	{
		foreach (Transform child in saveSlotContainer.transform)
			Destroy(child.gameObject);

		GameObject go = Instantiate(saveSlotCardPrefab, saveSlotContainer.transform);
		SaveSlotManager saveSlot = go.GetComponent<SaveSlotManager>();

		if (DoesDirectoryExist(Application.persistentDataPath + "/GameData/AutoSave"))
		{
			GrabJsonSlotData(Application.persistentDataPath + "/GameData/AutoSave");
			saveSlot.Name = SlotData.name;
			saveSlot.Level = SlotData.level;
			saveSlot.Date = SlotData.date;
			saveSlot.Initilize(Application.persistentDataPath + "/GameData/AutoSave", true, false);
		}
		else
		{
			saveSlot.Name = "Empty";
			saveSlot.Level = "Empty";
			saveSlot.Date = "Empty";
			saveSlot.Initilize(Application.persistentDataPath + "/GameData/AutoSave", true, true);
		}
	}

	/// <summary>
	/// public funcs called from ui buttons etc, pass in directory path run bool checks, call private func that save/load 
	/// Json data to disk, or delete Json data and files
	/// auto save is normal save but only has 1 slot and cant be called by player (may add keybind later)
	/// </summary>
	public void CreateNewGameSave() //temporary test function
	{
		SlotData.name = Utilities.GetRandomNumber(1000).ToString();
		SlotData.level = Utilities.GetRandomNumber(50).ToString();
		SlotData.date = System.DateTime.Now.ToString();
	}
	public void AutoSaveData()
	{
		if (Utilities.GetCurrentlyActiveScene(GameManager.Instance.mainMenuName)) return;
		SaveGameData(Application.persistentDataPath + "/GameData/AutoSave");
	}
	public void AutoLoadData()
	{
		LoadGameData(Application.persistentDataPath + "/GameData/AutoSave");
	}
	//directory checks/creation
	public void SaveGameData(string directory)
	{
		if (DoesDirectoryExist(directory))
		{
			if (DoesFileExist(directory, "/GameData.json"))
				System.IO.File.Delete(directory + "/GameData.json");
		}
		else
			System.IO.Directory.CreateDirectory(directory);

		SaveDataToJson(directory);
	}
	public void LoadGameData(string directory)
	{
		if (GameManager.isNewGame) return;
		if (!DoesDirectoryExist(directory)) return;
		if (!DoesFileExist(directory, "/GameData.json")) return;

		LoadDataToJson(directory);
	}
	public void DeleteGameData(string directory)
	{
		if (!DoesDirectoryExist(directory)) return;
		if (!DoesFileExist(directory, "/GameData.json")) return;

		DeleteJsonFile(directory);
	}

	//saving/loading/deleting json file
	private void SaveDataToJson(string directory)
	{
		SaveSavedDungeonData();
		SavePlayerInfoData();
		SavePlayerClassData();
		SavePlayerInventoryData();

		string filePath = directory + "/GameData.json";
		string inventoryData = JsonUtility.ToJson(GameData);
        System.IO.File.WriteAllText(filePath, inventoryData);

		string slotPath = directory + "/SlotData.json";
		string slotData = JsonUtility.ToJson(SlotData);
		System.IO.File.WriteAllText(slotPath, slotData);

		MainMenuManager.Instance.ReloadSaveSlots();
	}
	private void LoadDataToJson(string directory)
	{
		string filePath = directory + "/GameData.json";
		string inventoryData = System.IO.File.ReadAllText(filePath);
		GameData = JsonUtility.FromJson<GameData>(inventoryData);

		RestoreData?.Invoke();
	}
	private void DeleteJsonFile(string directory)
	{
		System.IO.File.Delete(directory + "/GameData.json");
		System.IO.File.Delete(directory + "/SlotData.json");
		System.IO.Directory.Delete(directory);

		MainMenuManager.Instance.ReloadSaveSlots();
	}

	//grab slot data for Ui
	private void GrabJsonSlotData(string directory)
	{
		string slotPath = directory + "/SlotData.json";
		string slotData = System.IO.File.ReadAllText(slotPath);
		SlotData = JsonUtility.FromJson<SlotData>(slotData);
	}

	//data to save to disk, loading data handled in other scripts that sub to OnGameLoad event (may make a OnGameSave event instead)
	private void SaveSavedDungeonData()
	{
		GameData.savedDungeonsList.Clear();
		foreach (DungeonSlotUi dungeon in DungeonPortalUi.instance.savedDungeonLists)
		{
			DungeonData dungeonData = new DungeonData
			{
				hasExploredDungeon = dungeon.hasExploredDungeon,
				isDungeonSaved = dungeon.isDungeonSaved,
				dungeonIndex = dungeon.dungeonIndex,
				dungeonNumber = dungeon.dungeonNumber,
				dungeonStatModifiers = dungeon.dungeonStatModifiers,
				dungeonChestData = dungeon.dungeonChestData
			};
			GameData.savedDungeonsList.Add(dungeonData);
		}

		GameData.activeDungeonsList.Clear();
		foreach (DungeonSlotUi dungeon in DungeonPortalUi.instance.activeDungeonLists)
		{
			DungeonData dungeonData = new DungeonData
			{
				hasExploredDungeon = dungeon.hasExploredDungeon,
				isDungeonSaved = dungeon.isDungeonSaved,
				dungeonIndex = dungeon.dungeonIndex,
				dungeonNumber = dungeon.dungeonNumber,
				dungeonStatModifiers = dungeon.dungeonStatModifiers,
				dungeonChestData = dungeon.dungeonChestData
			};
			GameData.activeDungeonsList.Add(dungeonData);
		}
	}
	private void SavePlayerInfoData()
	{
		if (FindObjectOfType<PlayerController>() == null) return;
		//need reworking for mp
		EntityStats playerStats = FindObjectOfType<PlayerController>().GetComponent<EntityStats>();

		SlotData.name = Utilities.GetRandomNumber(1000).ToString();
		SlotData.level = playerStats.entityLevel.ToString();
		SlotData.date = DateTime.Now.ToString();

		GameData.playerLevel = playerStats.entityLevel;
		GameData.playerCurrentExp = playerStats.GetComponent<PlayerExperienceHandler>().currentExp;
		GameData.playerCurrenthealth = playerStats.currentHealth;
		GameData.playerCurrentMana = playerStats.currentMana;
		GameData.playerGoldAmount = playerStats.GetComponent<PlayerInventoryManager>().GetGoldAmount();
	}
	private void SavePlayerClassData()
	{
		//need reworking for mp
		GameData.currentPlayerClass = ClassesUi.Instance.currentPlayerClass;
		GameData.unlockedClassNodeIndexesList.Clear();

		foreach (ClassTreeNodeSlotUi node in ClassesUi.Instance.currentUnlockedClassNodes)
			GameData.unlockedClassNodeIndexesList.Add(node.nodeIndex);
	}
	private void SavePlayerInventoryData()
	{
		//need reworking for mp
		GameData.hasRecievedStartingItems = PlayerInventoryManager.Instance.hasRecievedStartingItems;

		GrabInventoryItemsFromUi(GameData.inventoryItems, PlayerInventoryUi.Instance.InventorySlots);
		GrabInventoryItemsFromUi(GameData.equipmentItems, PlayerInventoryUi.Instance.EquipmentSlots);
		GrabInventoryItemsFromUi(GameData.consumableItems, PlayerHotbarUi.Instance.ConsumableSlots);
		GrabInventoryItemsFromUi(GameData.abilityItems, PlayerHotbarUi.Instance.AbilitySlots);
		GrabQuestDataFromActiveOnes(GameData.activePlayerQuests, PlayerJournalUi.Instance.activeQuests);
	}
	private void GrabInventoryItemsFromUi(List<InventoryItemData> itemDataList, List<GameObject> gameObjects)
	{
		itemDataList.Clear();

		foreach (GameObject slot in gameObjects)
		{
			if (slot.GetComponent<InventorySlotUi>().IsSlotEmpty()) continue;
			else
			{
				InventoryItemUi inventoryItem = slot.GetComponent<InventorySlotUi>().itemInSlot;
				InventoryItemData itemData = new()
				{
					weaponBaseRef = inventoryItem.weaponBaseRef,
					armorBaseRef = inventoryItem.armorBaseRef,
					accessoryBaseRef = inventoryItem.accessoryBaseRef,
					consumableBaseRef = inventoryItem.consumableBaseRef,
					abilityBaseRef = inventoryItem.abilityBaseRef,

					itemLevel = inventoryItem.itemLevel,
					rarity = (InventoryItemData.Rarity)inventoryItem.rarity,

					inventorySlotIndex = inventoryItem.inventorySlotIndex,
					isStackable = inventoryItem.isStackable,
					maxStackCount = inventoryItem.maxStackCount,
					currentStackCount = inventoryItem.currentStackCount
				};
				itemDataList.Add(itemData);
			}
		}
	}
	private void GrabQuestDataFromActiveOnes(List<QuestItemData> questDataList, List<QuestSlotsUi> activeQuestList)
	{
		questDataList.Clear();

		foreach (QuestSlotsUi quest in activeQuestList)
		{
			QuestItemData questData = new()
			{
				questType = (QuestItemData.QuestType)quest.questType,
				amount = quest.amount,
				currentAmount = quest.currentAmount,
				entityToKill = quest.entityToKill,
				weaponToHandIn = quest.weaponToHandIn,
				armorToHandIn = quest.armorToHandIn,
				accessoryToHandIn = quest.accessoryToHandIn,
				consumableToHandIn = quest.consumableToHandIn,
				itemTypeToHandIn = (QuestItemData.ItemType)quest.questType,
				questRewardType = (QuestItemData.RewardType)quest.questRewardType,
				rewardToAdd = quest.rewardToAdd
			};
			questDataList.Add(questData);
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
public class SlotData
{
	public string name;
	public string level;
	public string date;
}

[System.Serializable]
public class GameData
{
	public int playerLevel;
	public int playerCurrentExp;
	public int playerCurrenthealth;
	public int playerCurrentMana;
	public int playerGoldAmount;

	public bool hasRecievedStartingItems;

	public SOClasses currentPlayerClass;
	public List<int> unlockedClassNodeIndexesList = new List<int>();

	public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
	public List<InventoryItemData> equipmentItems = new List<InventoryItemData>();
	public List<InventoryItemData> consumableItems = new List<InventoryItemData>();
	public List<InventoryItemData> abilityItems = new List<InventoryItemData>();

	public List<DungeonData> activeDungeonsList = new List<DungeonData>();
	public List<DungeonData> savedDungeonsList = new List<DungeonData>();
	public List<QuestItemData> activePlayerQuests = new List<QuestItemData>();
}

[System.Serializable]
public class InventoryItemData
{
	public SOClassAbilities abilityBaseRef;

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
[System.Serializable]
public class QuestItemData
{
	[Header("Quest Info")]
	public QuestType questType;
	public enum QuestType
	{
		isBossKillQuest, isKillQuest, isItemHandInQuest
	}
	public int amount;
	public int currentAmount;

	[Header("Kill Quest Info")]
	public SOEntityStats entityToKill;

	[Header("Item Quest Info")]
	public SOWeapons weaponToHandIn;
	public SOArmors armorToHandIn;
	public SOAccessories accessoryToHandIn;
	public SOConsumables consumableToHandIn;

	public ItemType itemTypeToHandIn;
	public enum ItemType
	{
		isConsumable, isWeapon, isArmor, isAccessory, isAbility
	}

	[Header("Quest Reward")]
	public RewardType questRewardType;
	public enum RewardType
	{
		isExpReward, isGoldReward
	}
	public int rewardToAdd;
}
[System.Serializable]
public class DungeonData
{
	public bool hasExploredDungeon;
	public bool isDungeonSaved;
	public int dungeonIndex;
	public int dungeonNumber;
	public DungeonStatModifier dungeonStatModifiers;
	public List<DungeonChestData> dungeonChestData;
}
[System.Serializable]
public class DungeonChestData
{
	public bool chestActive;
	public bool chestStateOpened;
}
