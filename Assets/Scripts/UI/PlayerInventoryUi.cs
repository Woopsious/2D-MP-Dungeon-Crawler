using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
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
	public GameObject storageChestPanelUi;
	public Button closeStorageChestPanelButton;
	private ChestHandler interactedChest;

	[Header("Interacted Slots Ui")]
	public GameObject interactedInventorySlotsUi;
	public List<GameObject> interactedInventorySlots = new List<GameObject>();
	public TMP_Text interactedInventorySlotsText;

	[Header("Enchantment Ui")]
	public GameObject EnchanterUi;
	public TMP_Text enchanterSlotText;
	public InventorySlotDataUi enchanterSlot;
	public GameObject enchantItemButton;

	private void Awake()
	{
		Instance = this;
		Initilize();
	}
	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadPlayerInventory;
		PlayerClassesUi.OnNewAbilityUnlock += AddNewUnlockedAbility;
		PlayerClassesUi.OnRefundAbilityUnlock += OnAbilityRefund;
		PlayerJournalUi.OnQuestComplete += OnQuestComplete;
		InventorySlotDataUi.OnNewItemToEnchant += UpdateEnchantItemUiInfo;

		PlayerEventManager.OnShowPlayerInventoryEvent += ShowInventory;
		PlayerEventManager.OnShowPlayerClassSelectionEvent += HideInventory;
		PlayerEventManager.OnShowPlayerSkillTreeEvent += HideInventory;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent += HideInventory;
		PlayerEventManager.OnShowPlayerJournalEvent += HideInventory;

		PlayerEventManager.OnShowPlayerInventoryEvent += HideLearntAbilities;
		PlayerEventManager.OnShowPlayerClassSelectionEvent += HideLearntAbilities;
		PlayerEventManager.OnShowPlayerSkillTreeEvent += HideLearntAbilities;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent += ShowLearntAbilities;
		PlayerEventManager.OnShowPlayerJournalEvent += HideLearntAbilities;

		PlayerEventManager.OnShowNpcShopInventory += ShowNpcShop;
		PlayerEventManager.OnHideNpcShopInventory += HideNpcShop;
	}
	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadPlayerInventory;
		PlayerClassesUi.OnNewAbilityUnlock -= AddNewUnlockedAbility;
		PlayerClassesUi.OnRefundAbilityUnlock -= OnAbilityRefund;
		PlayerJournalUi.OnQuestComplete -= OnQuestComplete;
		InventorySlotDataUi.OnNewItemToEnchant -= UpdateEnchantItemUiInfo;

		PlayerEventManager.OnShowPlayerInventoryEvent -= ShowInventory;
		PlayerEventManager.OnShowPlayerClassSelectionEvent -= HideInventory;
		PlayerEventManager.OnShowPlayerSkillTreeEvent -= HideInventory;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent -= HideInventory;
		PlayerEventManager.OnShowPlayerJournalEvent -= HideInventory;

		PlayerEventManager.OnShowPlayerInventoryEvent -= HideLearntAbilities;
		PlayerEventManager.OnShowPlayerClassSelectionEvent -= HideLearntAbilities;
		PlayerEventManager.OnShowPlayerSkillTreeEvent -= HideLearntAbilities;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent -= ShowLearntAbilities;
		PlayerEventManager.OnShowPlayerJournalEvent -= HideLearntAbilities;

		PlayerEventManager.OnShowNpcShopInventory -= ShowNpcShop;
		PlayerEventManager.OnHideNpcShopInventory -= HideNpcShop;
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

	//restore player inventory data
	private void ReloadPlayerInventory()
	{
		UpdateGoldAmount(SaveManager.Instance.GameData.playerGoldAmount);

		RestoreInventoryItems(SaveManager.Instance.GameData.playerInventoryItems, InventorySlots);
		RestoreInventoryItems(SaveManager.Instance.GameData.playerEquippedItems, EquipmentSlots);
		RestoreInventoryItems(SaveManager.Instance.GameData.PlayerEquippedConsumables, PlayerHotbarUi.Instance.ConsumableSlots);
		RestoreInventoryItems(SaveManager.Instance.GameData.playerEquippedAbilities, PlayerHotbarUi.Instance.AbilitySlots);
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
	public void ReloadItemData(InventoryItemUi inventoryItem, InventoryItemData itemData)
	{
		inventoryItem.inventorySlotIndex = itemData.inventorySlotIndex;

		if (itemData.weaponBaseRef != null)
		{
			inventoryItem.weaponBaseRef = itemData.weaponBaseRef;
			Weapons weapon = inventoryItem.AddComponent<Weapons>();
			weapon.weaponBaseRef = itemData.weaponBaseRef;
			weapon.itemLevel = itemData.itemLevel;
			weapon.itemEnchantmentLevel = itemData.enchantmentLevel;
			weapon.rarity = (Items.Rarity)itemData.rarity;
			weapon.Initilize(weapon.rarity, weapon.itemLevel, weapon.itemEnchantmentLevel);
			weapon.SetCurrentStackCount(itemData.currentStackCount);
		}
		if (itemData.armorBaseRef != null)
		{
			inventoryItem.armorBaseRef = itemData.armorBaseRef;
			Armors armor = inventoryItem.AddComponent<Armors>();
			armor.armorBaseRef = itemData.armorBaseRef;
			armor.itemLevel = itemData.itemLevel;
			armor.itemEnchantmentLevel = itemData.enchantmentLevel;
			armor.rarity = (Items.Rarity)itemData.rarity;
			armor.Initilize(armor.rarity, armor.itemLevel, armor.itemEnchantmentLevel);
			armor.SetCurrentStackCount(itemData.currentStackCount);
		}
		if (itemData.accessoryBaseRef != null)
		{
			inventoryItem.accessoryBaseRef = itemData.accessoryBaseRef;
			Accessories accessory = inventoryItem.AddComponent<Accessories>();
			accessory.accessoryBaseRef = itemData.accessoryBaseRef;
			accessory.itemLevel = itemData.itemLevel;
			accessory.itemEnchantmentLevel = itemData.enchantmentLevel;
			accessory.rarity = (Items.Rarity)itemData.rarity;
			accessory.Initilize(accessory.rarity, accessory.itemLevel, accessory.itemEnchantmentLevel);
			accessory.SetCurrentStackCount(itemData.currentStackCount);
		}
		if (itemData.consumableBaseRef != null)
		{
			inventoryItem.consumableBaseRef = itemData.consumableBaseRef;
			Consumables consumable = inventoryItem.AddComponent<Consumables>();
			consumable.consumableBaseRef = itemData.consumableBaseRef;
			consumable.itemLevel = itemData.itemLevel;
			consumable.itemEnchantmentLevel = itemData.enchantmentLevel;
			consumable.rarity = (Items.Rarity)itemData.rarity;
			consumable.Initilize(consumable.rarity, consumable.itemLevel, consumable.itemEnchantmentLevel);
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

	//player gold updates
	public int GetGoldAmount()
	{
		return playerGoldAmount;
	}
	public void UpdateGoldAmount(int gold)
	{
		playerGoldAmount += gold;
		GetGoldAmount();
		PlayerEventManager.GoldAmountChange(playerGoldAmount);
	}
	public void OnQuestComplete(QuestDataUi quest)
	{
		if (quest.questRewardType == QuestDataUi.RewardType.isGoldReward)
			UpdateGoldAmount(quest.rewardToAdd);
	}

	//buying/selling items
	public void OnItemSell(InventoryItemUi item, InventorySlotDataUi slot)
	{
		int goldFromItemSelling = 0;
		goldFromItemSelling += item.itemPrice * item.currentStackCount;
		UpdateGoldAmount(goldFromItemSelling);

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

			int goldCostFromItemBuying = 0;
			goldCostFromItemBuying -= item.itemPrice * item.currentStackCount;
			UpdateGoldAmount(goldCostFromItemBuying);
		}
	}
	public void OnItemConfirmBuy(InventoryItemUi item, InventorySlotDataUi newSlot)
	{
		goldTransaction = -item.itemPrice * item.currentStackCount;
		transactionTrackerText.text = $"Gold: {goldTransaction}";
		transactionInfoText.text = "Item Brought";

		newSlot.AddItemToSlot(item);
	}
	public void OnItemCancelBuy(InventoryItemUi item, InventorySlotDataUi oldSlot, string reason)
	{
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
			weapon.itemEnchantmentLevel = item.itemEnchantmentLevel;
			weapon.rarity = item.rarity;
			weapon.Initilize(weapon.rarity, weapon.itemLevel, weapon.itemEnchantmentLevel);
			weapon.SetCurrentStackCount(item.currentStackCount);
		}
		if (item.armorBaseRef != null)
		{
			inventoryItem.armorBaseRef = item.armorBaseRef;
			Armors armor = inventoryItem.AddComponent<Armors>();
			armor.armorBaseRef = item.armorBaseRef;
			armor.itemLevel = item.itemLevel;
			armor.itemEnchantmentLevel = item.itemEnchantmentLevel;
			armor.rarity = item.rarity;
			armor.Initilize(armor.rarity, armor.itemLevel, armor.itemEnchantmentLevel);
			armor.SetCurrentStackCount(item.currentStackCount);
		}
		if (item.accessoryBaseRef != null)
		{
			inventoryItem.accessoryBaseRef = item.accessoryBaseRef;
			Accessories accessory = inventoryItem.AddComponent<Accessories>();
			accessory.accessoryBaseRef = item.accessoryBaseRef;
			accessory.itemLevel = item.itemLevel;
			accessory.itemEnchantmentLevel = item.itemEnchantmentLevel;
			accessory.rarity = item.rarity;
			accessory.Initilize(accessory.rarity, accessory.itemLevel, accessory.itemEnchantmentLevel);
			accessory.SetCurrentStackCount(item.currentStackCount);
		}
		if (item.consumableBaseRef != null)
		{
			inventoryItem.consumableBaseRef = item.consumableBaseRef;
			Consumables consumable = inventoryItem.AddComponent<Consumables>();
			consumable.consumableBaseRef = item.consumableBaseRef;
			consumable.itemLevel = item.itemLevel;
			consumable.itemEnchantmentLevel = item.itemEnchantmentLevel;
			consumable.rarity = item.rarity;
			consumable.Initilize(consumable.rarity, consumable.itemLevel, consumable.itemEnchantmentLevel);
			consumable.SetCurrentStackCount(item.currentStackCount);
		}
	}

	//item stacking
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

	//ABILITIES
	//Add/remove abilities to/from learnt abilities Ui
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
	private void OnAbilityRefund(SOClassAbilities ability)
	{
		foreach (GameObject abilitySlot in LearntAbilitySlots)
		{
			InventorySlotDataUi slotData = abilitySlot.GetComponent<InventorySlotDataUi>();
			if (slotData.itemInSlot == null)
				continue;

			if (slotData.itemInSlot.abilityBaseRef == ability)
			{
				Destroy(slotData.itemInSlot.gameObject);
				slotData.RemoveItemFromSlot();
			}
		}
	}


	//UI CHANGES
	//inventory
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

	//abilities
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
		if (npc.npc.shopType == SONpcs.ShopType.isWeaponSmith)
			interactedInventorySlotsText.text = "Weapon Smith";
		else if (npc.npc.shopType == SONpcs.ShopType.isArmorer)
			interactedInventorySlotsText.text = "Armourer";
		else if (npc.npc.shopType == SONpcs.ShopType.isGoldSmith)
			interactedInventorySlotsText.text = "Gold Smith";
		else if (npc.npc.shopType == SONpcs.ShopType.isGeneralStore)
			interactedInventorySlotsText.text = "General Store";

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

	//player storage chest
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
	public void ShowPlayerStorageChest(ChestHandler playerChest, int itemTypeToShow)
	{
		closeStorageChestPanelButton.onClick.AddListener(delegate { HidePlayerStorageChest(playerChest); }) ;

		foreach (GameObject obj in interactedInventorySlots) //change slotType
		{
			InventorySlotDataUi slot = obj.GetComponent<InventorySlotDataUi>();

			if (itemTypeToShow == 0)
			{
				interactedInventorySlotsText.text = "Weapon Storage";
				slot.slotType = InventorySlotDataUi.SlotType.weaponStorage;
			}
			else if (itemTypeToShow == 1)
			{
				interactedInventorySlotsText.text = "Armour Storage";
				slot.slotType = InventorySlotDataUi.SlotType.armourStorage;
			}
			else if (itemTypeToShow == 2)
			{
				interactedInventorySlotsText.text = "Accessory Storage";
				slot.slotType = InventorySlotDataUi.SlotType.accessoryStorage;
			}
			else if (itemTypeToShow == 3)
			{
				interactedInventorySlotsText.text = "Consumables Storage";
				slot.slotType = InventorySlotDataUi.SlotType.consumablesStorage;
			}
		}

		for (int i = 0; i < playerChest.itemList.Count; i++) //move items to ui slots
		{
			InventoryItemUi item = playerChest.itemList[i].GetComponent<InventoryItemUi>();
			InventorySlotDataUi slot = interactedInventorySlots[item.inventorySlotIndex].GetComponent<InventorySlotDataUi>();

			if (itemTypeToShow == 0 && item.weaponBaseRef != null)
			{
				slot.slotType = InventorySlotDataUi.SlotType.weaponStorage;
				slot.AddItemToSlot(item);
			}
			else if (itemTypeToShow == 1 && item.armorBaseRef != null)
			{
				slot.slotType = InventorySlotDataUi.SlotType.armourStorage;
				slot.AddItemToSlot(item);
			}
			else if (itemTypeToShow == 2 && item.accessoryBaseRef != null)
			{
				slot.slotType = InventorySlotDataUi.SlotType.accessoryStorage;
				slot.AddItemToSlot(item);
			}
			else if (itemTypeToShow == 3 && item.consumableBaseRef != null)
			{
				slot.slotType = InventorySlotDataUi.SlotType.consumablesStorage;
				slot.AddItemToSlot(item);
			}
		}

		interactedChest = playerChest;
		PlayerEventManager.ShowPlayerInventory();
		interactedInventorySlotsUi.SetActive(true);
		storageChestPanelUi.SetActive(true);
		UpdatePlayerInventoryItemsUi(interactedInventorySlots);
	}
	public void HidePlayerStorageChest(ChestHandler playerChest)
	{
		closeStorageChestPanelButton.onClick.RemoveAllListeners();
		playerChest.itemList.Clear();

		foreach (GameObject obj in interactedInventorySlots) //move to container, rest slot data
		{
			InventorySlotDataUi slot = obj.GetComponent<InventorySlotDataUi>();
			if (slot.IsSlotEmpty()) continue;

			slot.itemInSlot.transform.SetParent(playerChest.itemContainer.transform);
			slot.RemoveItemFromSlot();
		}

		for (int i = playerChest.itemContainer.transform.childCount - 1;  i >= 0; i--) //re-add all items + any new ones
			playerChest.itemList.Add(playerChest.itemContainer.transform.GetChild(i).GetComponent<InventoryItemUi>());

		PlayerEventManager.ShowPlayerInventory();
		interactedInventorySlotsUi.SetActive(false);
		storageChestPanelUi.SetActive(false);
		HideInventory();
	}

	//enchanter
	public void ShowEnchanterUi()
	{
		if (EnchanterUi.activeInHierarchy)
			HideEnchanterUi();
		else
			EnchanterUi.SetActive(true);
	}
	public void HideEnchanterUi()
	{
		EnchanterUi.SetActive(false);
		HideInventory();
	}
	public void UpdateEnchantItemUiInfo(InventoryItemUi item)
	{
		if (item == null)
		{
			enchantItemButton.SetActive(false);
			enchanterSlotText.text = "Drag enchantable item here to enchant";
			return;
		}
		else if (item.itemEnchantmentLevel >= 3)
		{
			enchantItemButton.SetActive(false);
			enchanterSlotText.text = $"Cant Enchant {item.itemName}, already max enchantment level";
		}
		else
		{
			int goldCostToEnchant = item.itemPrice * (item.itemEnchantmentLevel + 1);

			if (playerGoldAmount >= goldCostToEnchant)
			{
				enchantItemButton.SetActive(true);
				enchanterSlotText.text = $"Enchant {item.itemName} for {goldCostToEnchant} gold";
			}
			else
			{
				enchantItemButton.SetActive(false);
				enchanterSlotText.text = $"Cant Enchant {item.itemName}, you need {goldCostToEnchant} gold";
			}
		}
	}
	public void EnchantItemButton()
	{
		int goldCost = enchanterSlot.itemInSlot.itemPrice * (enchanterSlot.itemInSlot.itemEnchantmentLevel + 1);
		UpdateGoldAmount(-goldCost);
		enchanterSlot.EnchantItemInSlot();
		UpdateEnchantItemUiInfo(enchanterSlot.itemInSlot);
	}

	//update items in ui incase any changes were made
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
