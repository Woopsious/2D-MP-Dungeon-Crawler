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

	public void Awake()
	{
		Instance = this;
	}
	public void Start()
	{
		if (GameManager.isNewGame || debugSpawnStartingItems)
		{
			SpawnStartingItems();
			UpdateGoldAmount(startingGold);
		}
	}

	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadPlayerInventory;
		InventorySlotUi.OnItemSellEvent += OnItemSell;
		InventorySlotUi.OnItemTryBuyEvent += OnItemTryBuy;

		PlayerJournalUi.OnQuestComplete += OnQuestComplete;
	}
	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadPlayerInventory;
		InventorySlotUi.OnItemSellEvent -= OnItemSell;
		InventorySlotUi.OnItemTryBuyEvent -= OnItemTryBuy;

		PlayerJournalUi.OnQuestComplete -= OnQuestComplete;
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

		UpdateGoldAmount(SaveManager.Instance.GameData.playerGoldAmount);
	}

	//player gold
	public int GetGoldAmount()
	{
		return playerGoldAmount;
	}
	public void UpdateGoldAmount(int gold)
	{
		playerGoldAmount += gold;
		GetGoldAmount();
		EventManager.GoldAmountChange(playerGoldAmount);
	}
	public void OnItemTryBuy(InventoryItemUi item, InventorySlotUi newSlot, InventorySlotUi oldSlot)
	{
		if (item.itemPrice * item.currentStackCount > playerGoldAmount)
			oldSlot.ItemCancelBuy(item, oldSlot, "Cant Afford Item");
		else
		{
			newSlot.ItemConfirmBuy(item, newSlot);

			int gold = 0;
			gold -= item.itemPrice * item.currentStackCount;
			UpdateGoldAmount(gold);
		}
	}
	public void OnItemSell(InventoryItemUi item, InventorySlotUi slot)
	{
		int newgold = 0;
		newgold += item.itemPrice * item.currentStackCount;
		UpdateGoldAmount(newgold);
	}
	public void OnQuestComplete(QuestSlotsUi quest)
	{
		if (quest.questRewardType == QuestSlotsUi.RewardType.isGoldReward)
			UpdateGoldAmount(quest.rewardToAdd);
	}

	//on item pickup
	public void PickUpNewItem(Items item)
	{
		PlayerInventoryUi.Instance.AddItemToInventory(item, true);
	}
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
