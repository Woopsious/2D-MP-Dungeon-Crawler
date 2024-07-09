using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
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
		GameManager.OnSceneChangeFinish += LoadPlayerData;
		GameManager.OnSceneChangeFinish += RestoreGameData;
	}
	private void OnDisable()
	{
		GameManager.OnSceneChangeFinish -= LoadPlayerData;
		GameManager.OnSceneChangeFinish -= RestoreGameData;
	}

	public void ReloadSaveSlots(GameObject saveSlotContainer)
	{
		foreach (Transform child in saveSlotContainer.transform)
			Destroy(child.gameObject);

		for (int i = 0; i < maxSaveSlots; i++)
		{
			GameObject go = Instantiate(saveSlotCardPrefab, saveSlotContainer.transform);
			SaveSlotDataUi saveSlot = go.GetComponent<SaveSlotDataUi>();

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
		SaveSlotDataUi saveSlot = go.GetComponent<SaveSlotDataUi>();

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

	//SAVING PLAYER DATA
	public void SavePlayerData()
	{
		DeletePlayerData();

		string directory = Application.persistentDataPath + "/PlayerData";
		string filePath = Application.persistentDataPath + "/PlayerData/Keybinds.json";

		if (DoesDirectoryExist(directory))
		{
			if (DoesFileExist(directory, "/Keybinds.json"))
				System.IO.File.Delete(directory + "/Keybinds.json");
		}
		else
			System.IO.Directory.CreateDirectory(directory);

		var rebinds = PlayerInputHandler.Instance.playerControls.SaveBindingOverridesAsJson();
		System.IO.File.WriteAllText(filePath, rebinds);
	}
	public void LoadPlayerData()
	{
		string directory = Application.persistentDataPath + "/PlayerData";
		string filePath = Application.persistentDataPath + "/PlayerData/Keybinds.json";

		if (!DoesDirectoryExist(directory)) return;
		if (!DoesFileExist(directory, "/Keybinds.json")) return;

		string rebinds = System.IO.File.ReadAllText(filePath);
		if (!string.IsNullOrEmpty(rebinds))
			PlayerInputHandler.Instance.playerControls.LoadBindingOverridesFromJson(rebinds);
	}
	public void DeletePlayerData()
	{
		string directory = Application.persistentDataPath + "/PlayerData";

		if (!DoesDirectoryExist(directory)) return;
		if (!DoesFileExist(directory, "/Keybinds.json")) return;

		System.IO.File.Delete(directory + "/Keybinds.json");
		System.IO.Directory.Delete(directory);

	}

	//SAVING GAME DATA
	//auto features
	public void AutoSaveData()
	{
		if (Utilities.GetCurrentlyActiveScene(GameManager.Instance.mainMenuName)) return;
		SaveGameData(Application.persistentDataPath + "/GameData/AutoSave");
	}
	public void AutoLoadData() //(redundent function, may reuse at some point)
	{
		if (Utilities.GetCurrentlyActiveScene(GameManager.Instance.mainMenuName)) return;
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

		SaveSavedDungeonData();
		SavePlayerInfoData();
		SavePlayerClassData();
		SavePlayerStorageChestData();
		SavePlayerInventoryData();

		SaveDataToJson(directory, "/GameData.json");
	}
	public void LoadGameData(string directory)
	{
		if (GameManager.isNewGame) return;
		if (!DoesDirectoryExist(directory)) return;
		if (!DoesFileExist(directory, "/GameData.json")) return;

		LoadDataToJson(directory, "/GameData.json");
	}
	public void DeleteGameData(string directory)
	{
		if (!DoesDirectoryExist(directory)) return;
		if (!DoesFileExist(directory, "/GameData.json")) return;

		DeleteJsonFile(directory, "/GameData.json");
	}
	public void RestoreGameData()
	{
		RestoreData?.Invoke();
	}

	//saving/loading/deleting json file
	private void SaveDataToJson(string directory, string fileName)
	{
		string filePath = directory + fileName;
		string inventoryData = JsonUtility.ToJson(GameData);
        System.IO.File.WriteAllText(filePath, inventoryData);

		string slotPath = directory + "/SlotData.json";
		string slotData = JsonUtility.ToJson(SlotData);
		System.IO.File.WriteAllText(slotPath, slotData);

		MainMenuManager.Instance.ReloadSaveSlots();
	}
	private void LoadDataToJson(string directory, string fileName)
	{
		string filePath = directory + fileName;
		string inventoryData = System.IO.File.ReadAllText(filePath);
		GameData = JsonUtility.FromJson<GameData>(inventoryData);
	}
	private void DeleteJsonFile(string directory, string fileName)
	{
		System.IO.File.Delete(directory + "/SlotData.json");
		System.IO.File.Delete(directory + fileName);
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
		foreach (DungeonDataSlotUi dungeon in DungeonPortalUi.instance.savedDungeonLists)
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
		foreach (DungeonDataSlotUi dungeon in DungeonPortalUi.instance.activeDungeonLists)
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
		GameData.playerGoldAmount = PlayerInventoryUi.Instance.GetGoldAmount();
		GameData.hasRecievedStartingItems = playerStats.GetComponent<PlayerInventoryHandler>().hasRecievedStartingItems;
	}
	private void SavePlayerClassData()
	{
		//need reworking for mp
		GameData.currentPlayerClass = PlayerClassesUi.Instance.currentPlayerClass;
		GameData.unlockedClassNodeIndexesList.Clear();

		foreach (ClassTreeNodeSlotUi node in PlayerClassesUi.Instance.currentUnlockedClassNodes)
			GameData.unlockedClassNodeIndexesList.Add(node.nodeIndex);
	}
	private void SavePlayerStorageChestData()
	{
		if (DungeonHandler.Instance.playerStorageChest == null) return;

		GameData.playerStorageChestItems.Clear();
		ChestHandler playerStorageChest = DungeonHandler.Instance.playerStorageChest;

		foreach (InventoryItemUi item in playerStorageChest.itemList)
		{
			InventoryItemData itemData = new()
			{
				weaponBaseRef = item.weaponBaseRef,
				armorBaseRef = item.armorBaseRef,
				accessoryBaseRef = item.accessoryBaseRef,
				consumableBaseRef = item.consumableBaseRef,
				abilityBaseRef = item.abilityBaseRef,

				itemLevel = item.itemLevel,
				rarity = (InventoryItemData.Rarity)item.rarity,

				inventorySlotIndex = item.inventorySlotIndex,
				isStackable = item.isStackable,
				maxStackCount = item.maxStackCount,
				currentStackCount = item.currentStackCount
			};
			GameData.playerStorageChestItems.Add(itemData);
		}
	}
	private void SavePlayerInventoryData()
	{
		//need reworking for mp
		GrabInventoryItemsFromUi(GameData.playerInventoryItems, PlayerInventoryUi.Instance.InventorySlots);
		GrabInventoryItemsFromUi(GameData.playerEquippedItems, PlayerInventoryUi.Instance.EquipmentSlots);
		GrabInventoryItemsFromUi(GameData.PlayerEquippedConsumables, PlayerHotbarUi.Instance.ConsumableSlots);
		GrabInventoryItemsFromUi(GameData.playerEquippedAbilities, PlayerHotbarUi.Instance.AbilitySlots);
		GrabQuestDataFromActiveOnes(GameData.activePlayerQuests, PlayerJournalUi.Instance.activeQuests);
	}
	private void GrabInventoryItemsFromUi(List<InventoryItemData> itemDataList, List<GameObject> gameObjects)
	{
		itemDataList.Clear();

		foreach (GameObject slot in gameObjects)
		{
			if (slot.GetComponent<InventorySlotDataUi>().IsSlotEmpty()) continue;
			else
			{
				InventoryItemUi inventoryItem = slot.GetComponent<InventorySlotDataUi>().itemInSlot;
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
	private void GrabQuestDataFromActiveOnes(List<QuestItemData> questDataList, List<QuestDataSlotUi> activeQuestList)
	{
		questDataList.Clear();

		foreach (QuestDataSlotUi quest in activeQuestList)
		{
			QuestItemData questData = new()
			{
				isCurrentlyActiveQuest = quest.isCurrentlyActiveQuest,
				questType = (QuestItemData.QuestType)quest.questType,
				amount = quest.amount,
				currentAmount = quest.currentAmount,
				entityToKill = quest.entityToKill,
				weaponToHandIn = quest.weaponToHandIn,
				armorToHandIn = quest.armorToHandIn,
				accessoryToHandIn = quest.accessoryToHandIn,
				consumableToHandIn = quest.consumableToHandIn,
				itemTypeToHandIn = (QuestItemData.ItemType)quest.itemTypeToHandIn,
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

	public DungeonData currentDungeon;
	public SOClasses currentPlayerClass;
	public List<int> unlockedClassNodeIndexesList = new List<int>();

	public List<InventoryItemData> playerStorageChestItems = new List<InventoryItemData>();
	public List<InventoryItemData> playerInventoryItems = new List<InventoryItemData>();
	public List<InventoryItemData> playerEquippedItems = new List<InventoryItemData>();
	public List<InventoryItemData> PlayerEquippedConsumables = new List<InventoryItemData>();
	public List<InventoryItemData> playerEquippedAbilities = new List<InventoryItemData>();

	public List<DungeonData> activeDungeonsList = new List<DungeonData>();
	public List<DungeonData> savedDungeonsList = new List<DungeonData>();
	public List<QuestItemData> activePlayerQuests = new List<QuestItemData>();
}

[System.Serializable]
public class InventoryItemData
{
	public int slotIndexRef;

	[Header("Item Base Ref")]
	public SOWeapons weaponBaseRef;
	public SOArmors armorBaseRef;
	public SOAccessories accessoryBaseRef;
	public SOConsumables consumableBaseRef;
	public SOClassAbilities abilityBaseRef;

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
	public bool isCurrentlyActiveQuest;

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
	public List<DungeonChestData> dungeonChestData = new List<DungeonChestData>();
}
[System.Serializable]
public class DungeonChestData
{
	public bool chestActive;
	public bool chestStateOpened;
}
