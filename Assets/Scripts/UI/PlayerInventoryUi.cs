using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerInventoryUi : MonoBehaviour
{
	public static PlayerInventoryUi Instance;

	public GameObject ItemUiPrefab;
	public GameObject PlayerInfoAndInventoryPanelUi;
	public GameObject LearntAbilitiesPanelUi;

	[Header("Player Gold")]
	public int playerGoldAmount;

	[Header("Inventory Slots")]
	public List<GameObject> InventorySlots = new List<GameObject>();

	[Header("Equipment Slots")]
	public List<GameObject> EquipmentSlots = new List<GameObject>();

	public GameObject weaponEquipmentSlot;
	public GameObject offHandEquipmentSlot;
	public GameObject helmetEquipmentSlot;
	public GameObject chestpieceEquipmentSlot;
	public GameObject legsEquipmentSlot;

	public GameObject artifactSlot;
	public GameObject necklassEquipmentSlot;
	public GameObject ringEquipmentSlotOne;
	public GameObject ringEquipmentSlotTwo;

	[Header("Learnt Ability Slots")]
	public List<GameObject> LearntAbilitySlots = new List<GameObject>();

	[Header("Shop Npc Ui")]
	public GameObject npcShopPanalUi;
	public TMP_Text transactionInfoText;
	public TMP_Text transactionTrackerText;
	public Button closeShopButton;

	public int goldTransaction;

	[Header("Storage Chest Ui")]
	private ChestHandler interactedChest;
	public GameObject storageChestPanelUi;
	public int itemTypeToShow;

	[Header("Interacted Slots Ui")]
	public GameObject interactedInventorySlotsUi;
	public List<GameObject> interactedInventorySlots = new List<GameObject>();

	private void Awake()
	{
		Instance = this;
		Initilize();
	}
	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadPlayerInventory;
		PlayerClassesUi.OnClassReset += OnClassReset;
		PlayerClassesUi.OnNewAbilityUnlock += AddNewUnlockedAbility;
		PlayerJournalUi.OnQuestComplete += OnQuestComplete;

		EventManager.OnShowPlayerInventoryEvent += ShowInventory;
		EventManager.OnShowPlayerClassSelectionEvent += HideInventory;
		EventManager.OnShowPlayerSkillTreeEvent += HideInventory;
		EventManager.OnShowPlayerLearntAbilitiesEvent += HideInventory;
		EventManager.OnShowPlayerJournalEvent += HideInventory;

		EventManager.OnShowPlayerInventoryEvent += HideLearntAbilities;
		EventManager.OnShowPlayerClassSelectionEvent += HideLearntAbilities;
		EventManager.OnShowPlayerSkillTreeEvent += HideLearntAbilities;
		EventManager.OnShowPlayerLearntAbilitiesEvent += ShowLearntAbilities;
		EventManager.OnShowPlayerJournalEvent += HideLearntAbilities;

		EventManager.OnShowNpcShopInventory += ShowNpcShop;
		EventManager.OnHideNpcShopInventory += HideNpcShop;
	}
	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadPlayerInventory;
		PlayerClassesUi.OnClassReset -= OnClassReset;
		PlayerClassesUi.OnNewAbilityUnlock -= AddNewUnlockedAbility;
		PlayerJournalUi.OnQuestComplete -= OnQuestComplete;

		EventManager.OnShowPlayerInventoryEvent -= ShowInventory;
		EventManager.OnShowPlayerClassSelectionEvent -= HideInventory;
		EventManager.OnShowPlayerSkillTreeEvent -= HideInventory;
		EventManager.OnShowPlayerLearntAbilitiesEvent -= HideInventory;
		EventManager.OnShowPlayerJournalEvent -= HideInventory;

		EventManager.OnShowPlayerInventoryEvent -= HideLearntAbilities;
		EventManager.OnShowPlayerClassSelectionEvent -= HideLearntAbilities;
		EventManager.OnShowPlayerSkillTreeEvent -= HideLearntAbilities;
		EventManager.OnShowPlayerLearntAbilitiesEvent -= ShowLearntAbilities;
		EventManager.OnShowPlayerJournalEvent -= HideLearntAbilities;

		EventManager.OnShowNpcShopInventory -= ShowNpcShop;
		EventManager.OnHideNpcShopInventory -= HideNpcShop;
	}
	private void Initilize()
	{
		PlayerInfoAndInventoryPanelUi.SetActive(false);

		foreach (GameObject slot in LearntAbilitySlots)
			slot.GetComponent<InventorySlotDataUi>().SetSlotIndex();

		foreach (GameObject slot in InventorySlots)
			slot.GetComponent<InventorySlotDataUi>().SetSlotIndex();

		foreach (GameObject slot in EquipmentSlots)
			slot.GetComponent<InventorySlotDataUi>().SetSlotIndex();

		foreach (GameObject slot in interactedInventorySlots)
			slot.GetComponent<InventorySlotDataUi>().SetSlotIndex();
	}

	//reload player inventory
	private void ReloadPlayerInventory()
	{
		UpdateGoldAmount(SaveManager.Instance.GameData.playerGoldAmount);

		RestoreInventoryItems(SaveManager.Instance.GameData.inventoryItems, InventorySlots);
		RestoreInventoryItems(SaveManager.Instance.GameData.equipmentItems, EquipmentSlots);
		RestoreInventoryItems(SaveManager.Instance.GameData.consumableItems, PlayerHotbarUi.Instance.ConsumableSlots);
		RestoreInventoryItems(SaveManager.Instance.GameData.abilityItems, PlayerHotbarUi.Instance.AbilitySlots);
	}
	private void RestoreInventoryItems(List<InventoryItemData> itemDataList, List<GameObject> gameObjects)
	{
		foreach (InventoryItemData itemData in itemDataList)
		{
			GameObject go = Instantiate(ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
			InventoryItemUi newInventoryItem = go.GetComponent<InventoryItemUi>();

			ReloadItemData(newInventoryItem, itemData);
			InventorySlotDataUi inventorySlot = gameObjects[itemData.inventorySlotIndex].GetComponent<InventorySlotDataUi>();

			newInventoryItem.Initilize();
			newInventoryItem.transform.SetParent(inventorySlot.transform);
			newInventoryItem.parentAfterDrag = InventorySlots[0].transform;
			inventorySlot.AddItemToSlot(newInventoryItem);
		}
	}
	private void ReloadItemData(InventoryItemUi inventoryItem, InventoryItemData itemData)
	{
		if (itemData.weaponBaseRef != null)
		{
			inventoryItem.weaponBaseRef = itemData.weaponBaseRef;
			Weapons weapon = inventoryItem.AddComponent<Weapons>();
			weapon.weaponBaseRef = itemData.weaponBaseRef;
			weapon.itemLevel = itemData.itemLevel;
			weapon.rarity = (Items.Rarity)itemData.rarity;
			weapon.Initilize(weapon.rarity, weapon.itemLevel);
			weapon.SetCurrentStackCount(itemData.currentStackCount);
		}
		if (itemData.armorBaseRef != null)
		{
			inventoryItem.armorBaseRef = itemData.armorBaseRef;
			Armors armor = inventoryItem.AddComponent<Armors>();
			armor.armorBaseRef = itemData.armorBaseRef;
			armor.itemLevel = itemData.itemLevel;
			armor.rarity = (Items.Rarity)itemData.rarity;
			armor.Initilize(armor.rarity, armor.itemLevel);
			armor.SetCurrentStackCount(itemData.currentStackCount);
		}
		if (itemData.accessoryBaseRef != null)
		{
			inventoryItem.accessoryBaseRef = itemData.accessoryBaseRef;
			Accessories accessory = inventoryItem.AddComponent<Accessories>();
			accessory.accessoryBaseRef = itemData.accessoryBaseRef;
			accessory.itemLevel = itemData.itemLevel;
			accessory.rarity = (Items.Rarity)itemData.rarity;
			accessory.Initilize(accessory.rarity, accessory.itemLevel);
			accessory.SetCurrentStackCount(itemData.currentStackCount);
		}
		if (itemData.consumableBaseRef != null)
		{
			inventoryItem.consumableBaseRef = itemData.consumableBaseRef;
			Consumables consumable = inventoryItem.AddComponent<Consumables>();
			consumable.consumableBaseRef = itemData.consumableBaseRef;
			consumable.itemLevel = itemData.itemLevel;
			consumable.rarity = (Items.Rarity)itemData.rarity;
			consumable.Initilize(consumable.rarity, consumable.itemLevel);
			consumable.SetCurrentStackCount(itemData.currentStackCount);
		}
		if (itemData.abilityBaseRef != null)
		{
			inventoryItem.abilityBaseRef = itemData.abilityBaseRef;
			Abilities ability = inventoryItem.AddComponent<Abilities>();
			ability.abilityBaseRef = itemData.abilityBaseRef;
			ability.abilityName = itemData.abilityBaseRef.Name;
			ability.abilityDescription = itemData.abilityBaseRef.Description;
			ability.abilitySprite = itemData.abilityBaseRef.abilitySprite;
			ability.isEquippedAbility = false;
			ability.isOnCooldown = false;
		}
	}

	//PLAYER GOLD
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
	public void OnQuestComplete(QuestDataSlotUi quest)
	{
		if (quest.questRewardType == QuestDataSlotUi.RewardType.isGoldReward)
			UpdateGoldAmount(quest.rewardToAdd);
	}
	//buying/selling items
	public void OnItemSell(InventoryItemUi item, InventorySlotDataUi slot)
	{
		int newgold = 0;
		newgold += item.itemPrice * item.currentStackCount;
		UpdateGoldAmount(newgold);

		goldTransaction = item.itemPrice * item.currentStackCount;
		transactionTrackerText.text = $"Gold: {goldTransaction}";
		transactionInfoText.text = "Item Sold";
		slot.AddItemToSlot(item);
	}
	public void OnItemTryBuy(InventoryItemUi item, InventorySlotDataUi newSlot, InventorySlotDataUi oldSlot)
	{
		if (item.itemPrice * item.currentStackCount > playerGoldAmount)
			OnItemCancelBuy(item, oldSlot, "Cant Afford Item");
		else
		{
			OnItemConfirmBuy(item, newSlot);

			int gold = 0;
			gold -= item.itemPrice * item.currentStackCount;
			UpdateGoldAmount(gold);
		}
	}
	public void OnItemConfirmBuy(InventoryItemUi item, InventorySlotDataUi newSlot)
	{
		Debug.Log("confirm buy");

		goldTransaction = -item.itemPrice * item.currentStackCount;
		transactionTrackerText.text = $"Gold: {goldTransaction}";
		transactionInfoText.text = "Item Brought";

		newSlot.AddItemToSlot(item);
	}
	public void OnItemCancelBuy(InventoryItemUi item, InventorySlotDataUi oldSlot, string reason)
	{
		Debug.Log("cancel buy");

		goldTransaction = 0;
		transactionTrackerText.text = $"Gold: {goldTransaction}";
		transactionInfoText.text = reason;

		oldSlot.AddItemToSlot(item);
	}

	//ITEMS
	//Adding new items to Ui
	public void AddItemToInventory(Items item, bool tryStack)
	{
		if (item.isStackable && tryStack)
			TryStackItem(ConvertPickupsToInventoryItem(item));
		else
			SpawnNewItemInInventory(ConvertPickupsToInventoryItem(item));
	}
	private void SpawnNewItemInInventory(InventoryItemUi item)
	{
		for (int i = 0; i < InventorySlots.Count; i++)
		{
			InventorySlotDataUi inventorySlot = InventorySlots[i].GetComponent<InventorySlotDataUi>();

			if (inventorySlot.IsSlotEmpty())
			{
				inventorySlot.AddItemToSlot(item);
				return;
			}
		}
	}
	private InventoryItemUi ConvertPickupsToInventoryItem(Items item)
	{
		GameObject go = Instantiate(ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
		InventoryItemUi newItem = go.GetComponent<InventoryItemUi>();

		SetItemData(newItem, item);
		newItem.Initilize();
		return newItem;
	}
	private void SetItemData(InventoryItemUi inventoryItem, Items item)
	{
		if (item.weaponBaseRef != null)
		{
			inventoryItem.weaponBaseRef = item.weaponBaseRef;
			Weapons weapon = inventoryItem.AddComponent<Weapons>();
			weapon.weaponBaseRef = item.weaponBaseRef;
			weapon.itemLevel = item.itemLevel;
			weapon.rarity = item.rarity;
			weapon.Initilize(weapon.rarity, weapon.itemLevel);
			weapon.SetCurrentStackCount(item.currentStackCount);
		}
		if (item.armorBaseRef != null)
		{
			inventoryItem.armorBaseRef = item.armorBaseRef;
			Armors armor = inventoryItem.AddComponent<Armors>();
			armor.armorBaseRef = item.armorBaseRef;
			armor.itemLevel = item.itemLevel;
			armor.rarity = item.rarity;
			armor.Initilize(armor.rarity, armor.itemLevel);
			armor.SetCurrentStackCount(item.currentStackCount);
		}
		if (item.accessoryBaseRef != null)
		{
			inventoryItem.accessoryBaseRef = item.accessoryBaseRef;
			Accessories accessory = inventoryItem.AddComponent<Accessories>();
			accessory.accessoryBaseRef = item.accessoryBaseRef;
			accessory.itemLevel = item.itemLevel;
			accessory.rarity = item.rarity;
			accessory.Initilize(accessory.rarity, accessory.itemLevel);
			accessory.SetCurrentStackCount(item.currentStackCount);
		}
		if (item.consumableBaseRef != null)
		{
			inventoryItem.consumableBaseRef = item.consumableBaseRef;
			Consumables consumable = inventoryItem.AddComponent<Consumables>();
			consumable.consumableBaseRef = item.consumableBaseRef;
			consumable.itemLevel = item.itemLevel;
			consumable.rarity = item.rarity;
			consumable.Initilize(consumable.rarity, consumable.itemLevel);
			consumable.SetCurrentStackCount(item.currentStackCount);
		}
	}

	//stack item to existing ui items
	private void TryStackItem(InventoryItemUi newItem)
	{
		for (int i = 0; i < InventorySlots.Count; i++)
		{
			InventorySlotDataUi inventroySlot = InventorySlots[i].GetComponent<InventorySlotDataUi>();

			if (!inventroySlot.IsSlotEmpty())
				AddToStackCount(inventroySlot, newItem);

			else if (newItem.currentStackCount > 0)
			{
				SpawnNewItemInInventory(newItem);
				return;
			}
		}
	}
	public void AddToStackCount(InventorySlotDataUi inventroySlot, InventoryItemUi newItem)
	{
		InventoryItemUi itemInSlot = inventroySlot.GetComponentInChildren<InventoryItemUi>();
		if (inventroySlot.IsItemInSlotSameAs(newItem) && itemInSlot.currentStackCount < itemInSlot.maxStackCount)
		{
			//add to itemInSlot.CurrentStackCount till maxStackCountReached
			for (int itemCount = itemInSlot.currentStackCount; itemCount < itemInSlot.maxStackCount; itemCount++)
			{
				if (newItem.currentStackCount <= 0) return;
				newItem.DecreaseStackCounter();
				itemInSlot.IncreaseStackCounter();
			}
			if (newItem.currentStackCount != 0) //if stackcount still has more find next stackable item
				return;
		}
		else
			return;
	}

	//CLASSES + ABILITIES
	//reset/clear any learnt abilities from learnt abilities ui
	private void OnClassReset(SOClasses currentClass)
	{
		foreach (GameObject abilitySlot in LearntAbilitySlots)
		{
			if (abilitySlot.transform.childCount == 0)
				continue;

			Destroy(abilitySlot.transform.GetChild(0).gameObject);
		}
	}
	//Adding new abilities to Ui
	private void AddNewUnlockedAbility(SOClassAbilities newAbility)
	{
		GameObject go = Instantiate(ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
		InventoryItemUi item = go.GetComponent<InventoryItemUi>();
		SetAbilityData(item, newAbility);

		for (int i = 0; i < LearntAbilitySlots.Count; i++)
		{
			InventorySlotDataUi inventorySlot = LearntAbilitySlots[i].GetComponent<InventorySlotDataUi>();

			if (inventorySlot.IsSlotEmpty())
			{
				item.inventorySlotIndex = i;
				item.transform.SetParent(inventorySlot.transform);
				inventorySlot.itemInSlot = item;
				inventorySlot.UpdateSlotSize();
				item.Initilize();

				return;
			}
		}
	}
	public void SetAbilityData(InventoryItemUi inventoryItem, SOClassAbilities newAbility)
	{
		inventoryItem.abilityBaseRef = newAbility;
		Abilities ability = inventoryItem.AddComponent<Abilities>();
		ability.abilityBaseRef = newAbility;
		ability.Initilize();
	}

	//UI CHANGES
	public void ShowInventory()
	{
		if (PlayerInfoAndInventoryPanelUi.activeInHierarchy)
			HideInventory();
		else
			PlayerInfoAndInventoryPanelUi.SetActive(true);

		UpdatePlayerInventoryItemsUi(InventorySlots);
		UpdatePlayerInventoryItemsUi(EquipmentSlots);
	}
	public void HideInventory()
	{
		PlayerInfoAndInventoryPanelUi.SetActive(false);
	}

	public void ShowLearntAbilities()
	{
		if (LearntAbilitiesPanelUi.activeInHierarchy)
			HideLearntAbilities();
		else
			LearntAbilitiesPanelUi.SetActive(true);

		UpdatePlayerInventoryItemsUi(LearntAbilitySlots);
	}
	public void HideLearntAbilities()
	{
		LearntAbilitiesPanelUi.SetActive(false);
	}

	//npc shop
	public void ShowNpcShop(NpcHandler npc)
	{
		foreach (GameObject obj in interactedInventorySlots) //change slotType
		{
			InventorySlotDataUi slot = obj.GetComponent<InventorySlotDataUi>();
			slot.slotType = InventorySlotDataUi.SlotType.shopSlot;
		}

		for (int i = 0; i < npc.avalableShopItemsList.Count; i++) //move items to ui slots
		{
			InventorySlotDataUi slot = interactedInventorySlots[i].GetComponent<InventorySlotDataUi>();
			slot.AddItemToSlot(npc.avalableShopItemsList[i]);
		}

		transactionTrackerText.text = "Gold: 0";
		transactionInfoText.text = "No Item Sold/Brought";
		interactedInventorySlotsUi.SetActive(true);
		npcShopPanalUi.SetActive(true);
		UpdatePlayerInventoryItemsUi(interactedInventorySlots);
	}
	public void HideNpcShop(NpcHandler npc)
	{
		npc.avalableShopItemsList.Clear();

		foreach (GameObject obj in interactedInventorySlots) //move to container, rest slot data
		{
			InventorySlotDataUi slot = obj.GetComponent<InventorySlotDataUi>();
			if (slot.IsSlotEmpty()) continue;

			npc.avalableShopItemsList.Add(slot.itemInSlot); //add new items
			slot.itemInSlot.transform.SetParent(npc.npcContainer.transform);
			slot.RemoveItemFromSlot();
		}

		interactedInventorySlotsUi.SetActive(false);
		npcShopPanalUi.SetActive(false);
		HideInventory();
	}

	//playerStorageChest
	public void ShowPlayerStoredWeaponsButton()
	{
		HidePlayerStorageChest(interactedChest);
		ShowPlayerStorageChest(interactedChest, 0);
	}
	public void ShowPlayerStoredArmourButton()
	{
		HidePlayerStorageChest(interactedChest);
		ShowPlayerStorageChest(interactedChest, 1);
	}
	public void ShowPlayerStoredAccessoriesButton()
	{
		HidePlayerStorageChest(interactedChest);
		ShowPlayerStorageChest(interactedChest, 2);
	}
	public void ShowPlayerStoredConsumablesButton()
	{
		HidePlayerStorageChest(interactedChest);
		ShowPlayerStorageChest(interactedChest, 3);
	}
	public void ShowPlayerStorageChest(ChestHandler chest, int itemTypeToShow)
	{
		foreach (GameObject obj in interactedInventorySlots) //change slotType
		{
			InventorySlotDataUi slot = obj.GetComponent<InventorySlotDataUi>();

			if (itemTypeToShow == 0)
				slot.slotType = InventorySlotDataUi.SlotType.weapons;
			else if (itemTypeToShow == 1)
				slot.slotType = InventorySlotDataUi.SlotType.armour;
			else if (itemTypeToShow == 2)
				slot.slotType = InventorySlotDataUi.SlotType.accessories;
			else if (itemTypeToShow == 3)
				slot.slotType = InventorySlotDataUi.SlotType.consumables;
		}

		for (int i = 0; i < chest.itemList.Count; i++) //move items to ui slots
		{
			InventorySlotDataUi slot = interactedInventorySlots[i].GetComponent<InventorySlotDataUi>();

			if (itemTypeToShow == 0 && chest.itemList[i].weaponBaseRef != null)
			{
				slot.slotType = InventorySlotDataUi.SlotType.weapons;
				slot.AddItemToSlot(chest.itemList[i]);
			}
			else if (itemTypeToShow == 1 && chest.itemList[i].armorBaseRef != null)
			{
				slot.slotType = InventorySlotDataUi.SlotType.armour;
				slot.AddItemToSlot(chest.itemList[i]);
			}
			else if (itemTypeToShow == 2 && chest.itemList[i].accessoryBaseRef != null)
			{
				slot.slotType = InventorySlotDataUi.SlotType.accessories;
				slot.AddItemToSlot(chest.itemList[i]);
			}
			else if (itemTypeToShow == 3 && chest.itemList[i].consumableBaseRef != null)
			{
				slot.slotType = InventorySlotDataUi.SlotType.consumables;
				slot.AddItemToSlot(chest.itemList[i]);
			}
		}

		interactedChest = chest;
		this.itemTypeToShow = itemTypeToShow;
		EventManager.ShowPlayerInventory();
		interactedInventorySlotsUi.SetActive(true);
		storageChestPanelUi.SetActive(true);
		UpdatePlayerInventoryItemsUi(interactedInventorySlots);
	}
	public void HidePlayerStorageChest(ChestHandler chest)
	{
		chest.itemList.Clear();

		foreach (GameObject obj in interactedInventorySlots) //move to container, rest slot data
		{
			InventorySlotDataUi slot = obj.GetComponent<InventorySlotDataUi>();
			if (slot.IsSlotEmpty()) continue;

			slot.itemInSlot.transform.SetParent(chest.itemContainer.transform);
			slot.RemoveItemFromSlot();
		}

		for (int i = chest.itemContainer.transform.childCount - 1;  i >= 0; i--)
			chest.itemList.Add(chest.itemContainer.transform.GetChild(i).GetComponent<InventoryItemUi>()); //re-add all/new items

		EventManager.ShowPlayerInventory();
		interactedInventorySlotsUi.SetActive(false);
		storageChestPanelUi.SetActive(false);
		HideInventory();
	}

	private void UpdatePlayerInventoryItemsUi(List<GameObject> objList)
	{
		foreach (GameObject obj in objList)
		{
			if (obj.GetComponent<InventorySlotDataUi>().itemInSlot == null) continue;
			UpdateCanEquipItem(obj);
			UpdateToolTip(obj);
		}
	}
	private void UpdateToolTip(GameObject obj)
	{
		ToolTipUi tip = obj.GetComponent<InventorySlotDataUi>().itemInSlot.GetComponent<ToolTipUi>();
		tip.UpdatePlayerToolTip();
	}
	private void UpdateCanEquipItem(GameObject obj)
	{
		InventoryItemUi itemUi = obj.GetComponent<InventorySlotDataUi>().itemInSlot;
		itemUi.CheckIfCanEquipItem();
	}
}
