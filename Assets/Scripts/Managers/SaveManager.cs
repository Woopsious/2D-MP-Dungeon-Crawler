using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
		if (Instance != null && Instance != this)
			Destroy(gameObject);
		else
		{
			Instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
	}

	private void OnEnable()
	{
		//GameManager.OnSceneChangeFinish += LoadPlayerData;
		//GameManager.OnSceneChangeFinish += RestoreGameData;
	}
	private void OnDisable()
	{
		//GameManager.OnSceneChangeFinish -= LoadPlayerData;
		//GameManager.OnSceneChangeFinish -= RestoreGameData;
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

	//SAVING PLAYER DATA
	public void SavePlayerData()
	{
		DeletePlayerData();

		string directory = Application.persistentDataPath + "/PlayerData";
		string filePath = Application.persistentDataPath + "/PlayerData/data.json";

		if (DoesDirectoryExist(directory))
		{
			if (DoesFileExist(directory, "/data.json"))
				System.IO.File.Delete(directory + "/data.json");
		}
		else
			System.IO.Directory.CreateDirectory(directory);

		PlayerData playerData = new PlayerData
		{
			mainAttackIsAutomatic = PlayerSettingsManager.Instance.mainAttackIsAutomatic,
			autoSelectNewTarget = PlayerSettingsManager.Instance.autoSelectNewTarget,
			autoCastDirectionalAbilitiesAtTarget = PlayerSettingsManager.Instance.autoCastDirectionalAbilitiesAtTarget,
			autoCastAoeAbilitiesOnTarget = PlayerSettingsManager.Instance.autoCastAoeAbilitiesOnTarget,
			autoCastEffectAbilitiesOnTarget = PlayerSettingsManager.Instance.autoCastEffectAbilitiesOnTarget,
			keybindsData = PlayerInputHandler.Instance.playerControls.SaveBindingOverridesAsJson(),
			musicVolume = AudioManager.Instance.musicVolume,
			menuSfxVolume = AudioManager.Instance.menuSfxVolume,
			ambienceVolume = AudioManager.Instance.ambienceVolume,
			sfxVolume = AudioManager.Instance.sfxVolume,
		};

		string data = JsonUtility.ToJson(playerData);
		System.IO.File.WriteAllText(filePath, data);
	}
	public void LoadPlayerData()
	{
		string directory = Application.persistentDataPath + "/PlayerData";
		string filePath = Application.persistentDataPath + "/PlayerData/data.json";

		if (!DoesDirectoryExist(directory)) return;
		if (!DoesFileExist(directory, "/data.json")) return;

		string data = System.IO.File.ReadAllText(filePath);
		PlayerData playerData = JsonUtility.FromJson<PlayerData>(data);

		if (!string.IsNullOrEmpty(playerData.keybindsData))
			PlayerInputHandler.Instance.playerControls.LoadBindingOverridesFromJson(playerData.keybindsData);

		PlayerSettingsManager.Instance.RestorePlayerSettingsData(playerData.mainAttackIsAutomatic, playerData.autoSelectNewTarget,
			playerData.autoCastDirectionalAbilitiesAtTarget, playerData.autoCastAoeAbilitiesOnTarget, 
			playerData.autoCastEffectAbilitiesOnTarget);

		AudioManager.Instance.RestoreAudioVolume(playerData.musicVolume,
			playerData.menuSfxVolume, playerData.ambienceVolume, playerData.sfxVolume);
	}
	private void DeletePlayerData()
	{
		string directory = Application.persistentDataPath + "/PlayerData";

		if (!DoesDirectoryExist(directory)) return;
		if (!DoesFileExist(directory, "/data.json")) return;

		System.IO.File.Delete(directory + "/data.json");
		System.IO.Directory.Delete(directory);
	}

	//SAVING GAME DATA
	//auto features
	public void AutoSaveData()
	{
		if (Utilities.SceneIsActive(GameManager.Instance.menuScene)) return;
		SaveGameData(Application.persistentDataPath + "/GameData/AutoSave");
	}
	public void AutoLoadData() //(redundent function, may reuse at some point)
	{
		if (Utilities.SceneIsActive(GameManager.Instance.menuScene)) return;
		LoadGameData(Application.persistentDataPath + "/GameData/AutoSave");
	}

	//called via ui buttons + auto features
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

	//restore data event called on scene change
	public void RestoreGameData()
	{
		Debug.LogError("restoring data at: " + DateTime.Now.ToString());
		RestoreData?.Invoke();
	}

	//saving/loading/deleting json file
	private void SaveDataToJson(string directory, string fileName)
	{
		string filePath = directory + fileName;
		string inventoryData = JsonUtility.ToJson(Instance.GameData);
        System.IO.File.WriteAllText(filePath, inventoryData);

		string slotPath = directory + "/SlotData.json";
		string slotData = JsonUtility.ToJson(Instance.SlotData);
		System.IO.File.WriteAllText(slotPath, slotData);

		MainMenuManager.Instance.ReloadSaveSlots();
	}
	private void LoadDataToJson(string directory, string fileName)
	{
		string filePath = directory + fileName;
		string inventoryData = System.IO.File.ReadAllText(filePath);
		Instance.GameData = JsonUtility.FromJson<GameData>(inventoryData);
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
		Instance.SlotData = JsonUtility.FromJson<SlotData>(slotData);
	}

	//data to save to disk, loading data handled in scripts that sub to OnGameLoad event
	private void SaveSavedDungeonData()
	{
		Instance.GameData.savedDungeonsList.Clear();
		foreach (DungeonDataUi dungeon in DungeonPortalUi.instance.savedDungeonLists)
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
			Instance.GameData.savedDungeonsList.Add(dungeonData);
		}

		Instance.GameData.activeDungeonsList.Clear();
		foreach (DungeonDataUi dungeon in DungeonPortalUi.instance.activeDungeonLists)
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
			Instance.GameData.activeDungeonsList.Add(dungeonData);
		}
	}
	private void SavePlayerInfoData()
	{
		//need reworking for mp
		EntityStats playerStats = GameManager.Localplayer.playerStats;

		Instance.SlotData.name = Utilities.GetRandomNumber(1000).ToString();
		Instance.SlotData.level = playerStats.entityLevel.ToString();
		Instance.SlotData.date = DateTime.Now.ToString();

		Instance.GameData.playerLevel = playerStats.entityLevel;
		Instance.GameData.playerCurrentExp = playerStats.GetComponent<PlayerExperienceHandler>().currentExp;
		Instance.GameData.playerCurrenthealth = playerStats.currentHealth;
		Instance.GameData.playerCurrentMana = playerStats.currentMana;
		Instance.GameData.playerGoldAmount = PlayerInventoryUi.Instance.GetGoldAmount();
		Instance.GameData.hasRecievedStartingItems = playerStats.GetComponent<PlayerInventoryHandler>().hasRecievedStartingItems;
		Instance.GameData.hasRecievedKnightItems = playerStats.GetComponent<PlayerInventoryHandler>().hasRecievedKnightItems;
		Instance.GameData.hasRecievedWarriorItems = playerStats.GetComponent<PlayerInventoryHandler>().hasRecievedWarriorItems;
		Instance.GameData.hasRecievedRogueItems = playerStats.GetComponent<PlayerInventoryHandler>().hasRecievedRogueItems;
		Instance.GameData.hasRecievedRangerItems = playerStats.GetComponent<PlayerInventoryHandler>().hasRecievedRangerItems;
		Instance.GameData.hasRecievedMageItems = playerStats.GetComponent<PlayerInventoryHandler>().hasRecievedMageItems;
	}
	private void SavePlayerClassData()
	{
		//need reworking for mp
		Instance.GameData.currentPlayerClass = PlayerClassesUi.Instance.currentPlayerClass;
		Instance.GameData.unlockedClassNodeIndexesList.Clear();

		bool isNodeStatBoost;
		foreach (ClassTreeNodeUi node in PlayerClassesUi.Instance.currentUnlockedClassNodes)
		{
			if (node.statUnlock != null)
				isNodeStatBoost = true;
			else
				isNodeStatBoost = false;

			ClassTreeNodeData nodeData = new ClassTreeNodeData()
			{
				isStatBoost = isNodeStatBoost,
				nodeVerticalParentIndex = node.nodeVerticalParentIndex,
				nodeHorizontalIndex = node.nodeHorizontalIndex,
			};
			Instance.GameData.unlockedClassNodeIndexesList.Add(nodeData);
		}
	}
	private void SavePlayerStorageChestData()
	{
		if (DungeonHandler.Instance.playerStorageChest == null)
		{
			Debug.LogError("storage chest null");
			return;
		}

		Instance.GameData.playerStorageChestItems.Clear();
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
			Instance.GameData.playerStorageChestItems.Add(itemData);
		}
	}
	private void SavePlayerInventoryData()
	{
		//need reworking for mp
		GrabInventoryItemsFromUi(Instance.GameData.playerInventoryItems, PlayerInventoryUi.Instance.InventorySlots);
		GrabInventoryItemsFromUi(Instance.GameData.playerEquippedItems, PlayerInventoryUi.Instance.EquipmentSlots);
		GrabInventoryItemsFromUi(Instance.GameData.PlayerEquippedConsumables, PlayerHotbarUi.Instance.ConsumableSlots);
		GrabInventoryItemsFromUi(Instance.GameData.playerEquippedAbilities, PlayerHotbarUi.Instance.AbilitySlots);
		GrabQuestDataFromActiveOnes(Instance.GameData.activePlayerQuests, PlayerJournalUi.Instance.activeQuests);
	}
	private void GrabInventoryItemsFromUi(List<InventoryItemData> itemDataList, List<GameObject> inventorySlots)
	{
		itemDataList.Clear();

		foreach (GameObject slot in inventorySlots)
		{
			InventorySlotDataUi slotData = slot.GetComponent<InventorySlotDataUi>();
			if (slotData.IsSlotEmpty()) continue;
			else
			{
				InventoryItemUi inventoryItem = slotData.itemInSlot;
				InventoryItemData itemData = new()
				{
					weaponBaseRef = inventoryItem.weaponBaseRef,
					armorBaseRef = inventoryItem.armorBaseRef,
					accessoryBaseRef = inventoryItem.accessoryBaseRef,
					consumableBaseRef = inventoryItem.consumableBaseRef,
					abilityBaseRef = inventoryItem.abilityBaseRef,

					itemLevel = inventoryItem.itemLevel,
					enchantmentLevel = inventoryItem.itemEnchantmentLevel,
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
	private void GrabQuestDataFromActiveOnes(List<QuestItemData> questDataList, List<QuestDataUi> activeQuestList)
	{
		questDataList.Clear();

		foreach (QuestDataUi quest in activeQuestList)
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

//PLAYER DATA
[System.Serializable]
public class PlayerData
{
	public bool mainAttackIsAutomatic;
	public bool autoSelectNewTarget;
	public bool autoCastDirectionalAbilitiesAtTarget;
	public bool autoCastAoeAbilitiesOnTarget;
	public bool autoCastEffectAbilitiesOnTarget;

	public string keybindsData;

	public float musicVolume;
	public float menuSfxVolume;
	public float ambienceVolume;
	public float sfxVolume;
}

//GAME DATA
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
	public bool hasRecievedKnightItems;
	public bool hasRecievedWarriorItems;
	public bool hasRecievedRogueItems;
	public bool hasRecievedRangerItems;
	public bool hasRecievedMageItems;

	public DungeonData currentDungeon;
	public SOClasses currentPlayerClass;
	public List<ClassTreeNodeData> unlockedClassNodeIndexesList = new List<ClassTreeNodeData>();

	public List<InventoryItemData> playerStorageChestItems = new List<InventoryItemData>();
	public List<InventoryItemData> playerInventoryItems = new List<InventoryItemData>();
	public List<InventoryItemData> playerEquippedItems = new List<InventoryItemData>();
	public List<InventoryItemData> PlayerEquippedConsumables = new List<InventoryItemData>();
	public List<InventoryItemData> playerEquippedAbilities = new List<InventoryItemData>();

	public List<DungeonData> activeDungeonsList = new List<DungeonData>();
	public List<DungeonData> savedDungeonsList = new List<DungeonData>();
	public List<QuestItemData> activePlayerQuests = new List<QuestItemData>();
}

//GAME DATA TYPES
[System.Serializable]
public class InventoryItemData
{
	public int slotIndexRef;

	[Header("Item Base Ref")]
	public SOWeapons weaponBaseRef;
	public SOArmors armorBaseRef;
	public SOAccessories accessoryBaseRef;
	public SOConsumables consumableBaseRef;
	public SOAbilities abilityBaseRef;

	[Header("Item Info")]
	public int itemLevel;
	public int enchantmentLevel;
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
public class ClassTreeNodeData
{
	public bool isStatBoost;
	public int nodeVerticalParentIndex;
	public int nodeHorizontalIndex;
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
	public SOEntityStats bossToSpawn;
	public DungeonStatModifier dungeonStatModifiers;
	public List<DungeonChestData> dungeonChestData = new List<DungeonChestData>();
}
[System.Serializable]
public class DungeonChestData
{
	public bool chestActive;
	public bool chestStateOpened;
}
