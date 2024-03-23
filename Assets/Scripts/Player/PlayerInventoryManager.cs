using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEditor.Progress;
using Random = UnityEngine.Random;

public class PlayerInventoryManager : MonoBehaviour
{
	public static PlayerInventoryManager Instance;

	[Header("Player Starting Items")]
	public GameObject droppedItemPrefab;
	public bool debugSpawnStartingItems;
	public bool hasRecievedStartingItems;
	public List<SOItems> startingItems = new List<SOItems>();

	[Header("Player Gold")]
	public int playerGoldAmount;
	public int startingGold;

	[Header("Player Quests")]
	public GameObject questPrefab;
	public List<QuestSlotsUi> activeQuests = new List<QuestSlotsUi>();

	public void Awake()
	{
		Instance = this;
	}
	public void Start()
	{
		if (debugSpawnStartingItems)
		{
			SpawnStartingItems();
			UpdateGoldAmount(startingGold);
		}
	}

	private void OnEnable()
	{
		SaveManager.OnGameLoad += ReloadPlayerInventory;
		InventorySlotUi.OnItemBuyEvent += OnItemBuy;
		InventorySlotUi.OnItemSellEvent += OnItemSell;

		PlayerJournalUi.OnNewQuestAccepted += OnQuestAccept;
		PlayerJournalUi.OnQuestComplete += OnQuestComplete;
		PlayerJournalUi.OnQuestAbandon += OnQuestAbandon;

		//sub to item buy/sell events
	}
	private void OnDisable()
	{
		SaveManager.OnGameLoad -= ReloadPlayerInventory;
		InventorySlotUi.OnItemBuyEvent -= OnItemBuy;
		InventorySlotUi.OnItemSellEvent -= OnItemSell;

		PlayerJournalUi.OnNewQuestAccepted -= OnQuestAccept;
		PlayerJournalUi.OnQuestComplete -= OnQuestComplete;
		PlayerJournalUi.OnQuestAbandon -= OnQuestAbandon;

		//unsub to item buy/sell events
	}

	private void SpawnStartingItems()
	{
		hasRecievedStartingItems = true;

		for (int i = 0; i < startingItems.Count; i++) //spawn item from loot pool at death location
		{
			GameObject go = Instantiate(droppedItemPrefab, gameObject.transform.position, Quaternion.identity);

			if (startingItems[i].itemType == SOItems.ItemType.isWeapon)
			{
				Weapons weapon = go.AddComponent<Weapons>();
				weapon.weaponBaseRef = (SOWeapons)startingItems[i];
				weapon.SetCurrentStackCount(1);
			}

			if (startingItems[i].itemType == SOItems.ItemType.isArmor)
			{
				Armors armor = go.AddComponent<Armors>();
				armor.armorBaseRef = (SOArmors)startingItems[i];
				armor.SetCurrentStackCount(1);
			}

			if (startingItems[i].itemType == SOItems.ItemType.isAccessory)
			{
				Accessories accessories = go.AddComponent<Accessories>();
				accessories.accessoryBaseRef = (SOAccessories)startingItems[i];
				accessories.SetCurrentStackCount(1);
			}

			if (startingItems[i].itemType == SOItems.ItemType.isConsumable)
			{
				Consumables consumables = go.AddComponent<Consumables>();
				consumables.consumableBaseRef = (SOConsumables)startingItems[i];
				consumables.SetCurrentStackCount(3);
			}

			Items item = go.GetComponent<Items>();
			item.gameObject.name = startingItems[i].name;
			item.itemName = startingItems[i].name;
			item.itemSprite = startingItems[i].itemImage;
			item.itemPrice = startingItems[i].itemPrice;

			//generic data here, may change if i make unique droppables like keys as they might not have a need for item level etc.
			//im just not sure of a better way to do it atm
			go.GetComponent<Items>().Initilize(Items.Rarity.isLegendary, 1);
			BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
			collider.isTrigger = true;
		}
	}
	private void ReloadPlayerInventory()
	{
		hasRecievedStartingItems = SaveManager.Instance.GameData.hasRecievedStartingItems;

		if (!hasRecievedStartingItems)
		{
			UpdateGoldAmount(startingGold);
			SpawnStartingItems();
		}
		else
		{
			UpdateGoldAmount(SaveManager.Instance.GameData.playerGoldAmount);
			ReloadActiveQuests();
		}
	}

	//player gold
	public int GetGoldAmount()
	{
		return playerGoldAmount;
	}
	private void UpdateGoldAmount(int gold)
	{
		playerGoldAmount += gold;
		GetGoldAmount();
		EventManagerUi.GoldAmountChange(playerGoldAmount);
	}
	public void OnItemBuy(InventoryItem item)
	{
		int gold = 0;
		gold -= item.itemPrice * item.currentStackCount;
		UpdateGoldAmount(gold);
	}
	public void CheckIfCanAffordItem(InventoryItem item)
	{
		int gold = 0;
		gold -= item.itemPrice * item.currentStackCount;
		if (gold <= playerGoldAmount)
		{
			gold -= item.itemPrice * item.currentStackCount;
			UpdateGoldAmount(gold);
		}
		else
		{

		}
	}
	public void OnItemSell(InventoryItem item)
	{
		int newgold = 0;
		newgold += item.itemPrice * item.currentStackCount;
		UpdateGoldAmount(newgold);
	}

	//player quests
	public void ReloadActiveQuests()
	{
		List<QuestItemData> questData = SaveManager.Instance.GameData.activePlayerQuests;
		for (int i = 0; i < questData.Count; i++)
		{
			GameObject go = Instantiate(questPrefab, PlayerJournalUi.Instance.activeQuestContainer.transform);
			QuestSlotsUi quest = go.GetComponent<QuestSlotsUi>();

			quest.questType = (QuestSlotsUi.QuestType)questData[i].questType;
			quest.amount = questData[i].amount;
			quest.currentAmount = questData[i].currentAmount;
			quest.entityToKill = questData[i].entityToKill;
			quest.weaponToHandIn = questData[i].weaponToHandIn;
			quest.armorToHandIn = questData[i].armorToHandIn;
			quest.accessoryToHandIn = questData[i].accessoryToHandIn;
			quest.consumableToHandIn = questData[i].consumableToHandIn;
			quest.itemTypeToHandIn = (QuestSlotsUi.ItemType)questData[i].questType;
			quest.questRewardType = (QuestSlotsUi.RewardType)questData[i].questRewardType;
			quest.rewardToAdd = questData[i].rewardToAdd;

			quest.InitilizeText();
			quest.AcceptThisQuest();
		}
	}
	public void OnQuestAccept(QuestSlotsUi quest)
	{
		quest.isCurrentlyActiveQuest = true;
		activeQuests.Add(quest);
	}
	public void OnQuestComplete(QuestSlotsUi quest)
	{
		//do stuff to complete quest
		if (quest.questRewardType == QuestSlotsUi.RewardType.isExpReward)
			GetComponent<PlayerExperienceHandler>().AddExperience(quest.gameObject);
		else if (quest.questRewardType == QuestSlotsUi.RewardType.isGoldReward)
			UpdateGoldAmount(quest.rewardToAdd);

		activeQuests.Remove(quest);
	}
	public void OnQuestAbandon(QuestSlotsUi quest)
	{
		activeQuests.Remove(quest);
	}

	//on item pickup
	public void AddNewItemToPlayerInventory(Items item)
	{
		PlayerInventoryUi.Instance.AddItemToInventory(item);
	}
	//bool check before item pickup
	public bool CheckIfInventoryFull()
	{
		int numOfFilledSlots = 0;
		foreach (GameObject obj in PlayerInventoryUi.Instance.InventorySlots)
		{
			InventorySlotUi inventorySlot = obj.GetComponent<InventorySlotUi>();

			if (!inventorySlot.IsSlotEmpty())
				numOfFilledSlots++;
		}

		if (numOfFilledSlots == PlayerInventoryUi.Instance.InventorySlots.Count)
			return true;
		else return false;
	}
}
