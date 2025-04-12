using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInventoryUi : MonoBehaviour
{
	public static PlayerInventoryUi Instance;

	public GameObject ItemUiPrefab;
	public GameObject PlayerInfoAndInventoryPanelUi;
	public GameObject LearntAbilitiesPanelUi;

	[Header("Player Gold")]
	public int playerGoldAmount;

	[Header("Player Stored Items")]
	public GameObject playerStoredItemsContainer;
	private List<InventoryItemUi> playerStoredItems = new List<InventoryItemUi>();

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

	[Header("Storage Chest Ui")]
	public GameObject storageChestPanelUi;
	public Button closeStorageChestPanelButton;

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
		SaveManager.ReloadSaveGameData += ReloadPlayerInventory;

		PlayerClassesUi.OnNewAbilityUnlock += AddNewUnlockedAbility;
		PlayerClassesUi.OnRefundAbilityUnlock += OnAbilityRefund;
		PlayerJournalUi.OnQuestComplete += OnQuestComplete;
		InventorySlotDataUi.OnNewItemToEnchant += UpdateEnchantItemUiInfo;

		PlayerEventManager.OnShowPlayerInventoryEvent += ShowInventory;
		PlayerEventManager.OnShowPlayerClassSelectionEvent += HideInventory;
		PlayerEventManager.OnShowPlayerSkillTreeEvent += HideInventory;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent += HideInventory;
		PlayerEventManager.OnShowPlayerJournalEvent += HideInventory;
		PlayerEventManager.OnShowPlayerCodexEvent += HideInventory;
		PlayerEventManager.OnShowPlayerDeathUiEvent += HideInventory;

		PlayerEventManager.OnShowPlayerInventoryEvent += HideLearntAbilities;
		PlayerEventManager.OnShowPlayerClassSelectionEvent += HideLearntAbilities;
		PlayerEventManager.OnShowPlayerSkillTreeEvent += HideLearntAbilities;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent += ShowLearntAbilities;
		PlayerEventManager.OnShowPlayerJournalEvent += HideLearntAbilities;
		PlayerEventManager.OnShowPlayerCodexEvent += HideLearntAbilities;
		PlayerEventManager.OnShowPlayerDeathUiEvent += HideLearntAbilities;

		PlayerEventManager.OnShowNpcShopInventory += ShowNpcShop;
		PlayerEventManager.OnHideNpcShopInventory += HideNpcShop;
	}
	private void OnDisable()
	{
		SaveManager.ReloadSaveGameData -= ReloadPlayerInventory;

		PlayerClassesUi.OnNewAbilityUnlock -= AddNewUnlockedAbility;
		PlayerClassesUi.OnRefundAbilityUnlock -= OnAbilityRefund;
		PlayerJournalUi.OnQuestComplete -= OnQuestComplete;
		InventorySlotDataUi.OnNewItemToEnchant -= UpdateEnchantItemUiInfo;

		PlayerEventManager.OnShowPlayerInventoryEvent -= ShowInventory;
		PlayerEventManager.OnShowPlayerClassSelectionEvent -= HideInventory;
		PlayerEventManager.OnShowPlayerSkillTreeEvent -= HideInventory;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent -= HideInventory;
		PlayerEventManager.OnShowPlayerJournalEvent -= HideInventory;
		PlayerEventManager.OnShowPlayerCodexEvent -= HideInventory;
		PlayerEventManager.OnShowPlayerDeathUiEvent -= HideInventory;

		PlayerEventManager.OnShowPlayerInventoryEvent -= HideLearntAbilities;
		PlayerEventManager.OnShowPlayerClassSelectionEvent -= HideLearntAbilities;
		PlayerEventManager.OnShowPlayerSkillTreeEvent -= HideLearntAbilities;
		PlayerEventManager.OnShowPlayerLearntAbilitiesEvent -= ShowLearntAbilities;
		PlayerEventManager.OnShowPlayerJournalEvent -= HideLearntAbilities;
		PlayerEventManager.OnShowPlayerCodexEvent -= HideLearntAbilities;
		PlayerEventManager.OnShowPlayerDeathUiEvent -= HideLearntAbilities;

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

	public List<InventoryItemUi> GetPlayerStoredItemsList()
	{
		return playerStoredItems;
	}

	//restore player inventory data
	private void ReloadPlayerInventory()
	{
		UpdateGoldAmount(SaveManager.Instance.GameData.playerGoldAmount);

		RestorePlayerStorageChestItems(SaveManager.Instance.GameData.playerStorageChestItems, playerStoredItemsContainer);
		RestoreInventoryItems(SaveManager.Instance.GameData.playerInventoryItems, InventorySlots);
		RestoreInventoryItems(SaveManager.Instance.GameData.playerEquippedItems, EquipmentSlots);
		RestoreInventoryItems(SaveManager.Instance.GameData.PlayerEquippedConsumables, PlayerHotbarUi.Instance.ConsumableSlots);
		RestoreInventoryItems(SaveManager.Instance.GameData.playerEquippedAbilities, PlayerHotbarUi.Instance.AbilitySlots);
	}
	private void RestorePlayerStorageChestItems(List<InventoryItemData> itemDataList, GameObject container)
	{
		if (DungeonHandler.Instance == null || DungeonHandler.Instance.playerStorageChest == null) return;

		foreach (InventoryItemData itemData in itemDataList)
		{
			GameObject go = Instantiate(Instance.ItemUiPrefab, container.transform);
			InventoryItemUi newInventoryItem = go.GetComponent<InventoryItemUi>();

			Instance.ReloadItemData(newInventoryItem, itemData);
			newInventoryItem.Initilize();
			playerStoredItems.Add(newInventoryItem);
		}
	}
	private void RestoreInventoryItems(List<InventoryItemData> itemDataList, List<GameObject> slotListObjs)
	{
		foreach (InventoryItemData itemData in itemDataList)
		{
			GameObject go = Instantiate(ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
			InventoryItemUi newInventoryItem = go.GetComponent<InventoryItemUi>();

			ReloadItemData(newInventoryItem, itemData);
			InventorySlotDataUi inventorySlot = slotListObjs[itemData.inventorySlotIndex].GetComponent<InventorySlotDataUi>();

			newInventoryItem.Initilize();
			newInventoryItem.transform.SetParent(inventorySlot.transform);
			newInventoryItem.parentAfterDrag = InventorySlots[0].transform;
			inventorySlot.AddItemToSlot(newInventoryItem);
		}
	}
	private void ReloadItemData(InventoryItemUi inventoryItem, InventoryItemData itemData)
	{
		inventoryItem.inventorySlotIndex = itemData.inventorySlotIndex;

		if (itemData.weaponBaseRef != null)
		{
			inventoryItem.weaponBaseRef = itemData.weaponBaseRef;
			Weapons weapon = inventoryItem.AddComponent<Weapons>();
			weapon.weaponBaseRef = itemData.weaponBaseRef;
			weapon.level = itemData.level;
			weapon.enchantmentLevel = itemData.enchantmentLevel;
			weapon.rarity = itemData.rarity;
			weapon.Initilize(weapon.rarity, weapon.level, weapon.enchantmentLevel);
			weapon.SetCurrentStackCount(itemData.currentStackCount);
		}
		else if (itemData.armorBaseRef != null)
		{
			inventoryItem.armorBaseRef = itemData.armorBaseRef;
			Armors armor = inventoryItem.AddComponent<Armors>();
			armor.armorBaseRef = itemData.armorBaseRef;
			armor.level = itemData.level;
			armor.enchantmentLevel = itemData.enchantmentLevel;
			armor.rarity = itemData.rarity;
			armor.Initilize(armor.rarity, armor.level, armor.enchantmentLevel);
			armor.SetCurrentStackCount(itemData.currentStackCount);
		}
		else if (itemData.accessoryBaseRef != null)
		{
			inventoryItem.accessoryBaseRef = itemData.accessoryBaseRef;
			Accessories accessory = inventoryItem.AddComponent<Accessories>();
			accessory.accessoryBaseRef = itemData.accessoryBaseRef;
			accessory.level = itemData.level;
			accessory.enchantmentLevel = itemData.enchantmentLevel;
			accessory.rarity = itemData.rarity;
			accessory.Initilize(accessory.rarity, accessory.level, accessory.enchantmentLevel);
			accessory.SetCurrentStackCount(itemData.currentStackCount);
		}
		else if (itemData.consumableBaseRef != null)
		{
			inventoryItem.consumableBaseRef = itemData.consumableBaseRef;
			Consumables consumable = inventoryItem.AddComponent<Consumables>();
			consumable.consumableBaseRef = itemData.consumableBaseRef;
			consumable.level = itemData.level;
			consumable.enchantmentLevel = itemData.enchantmentLevel;
			consumable.rarity = itemData.rarity;
			consumable.Initilize(consumable.rarity, consumable.level, consumable.enchantmentLevel);
			consumable.SetCurrentStackCount(itemData.currentStackCount);
		}
		else if (itemData.abilityBaseRef != null)
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
		else
			Debug.LogError("itemData.Ref null this shouldnt happen");
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

	//quest completion event listener
	public void OnQuestComplete(QuestDataUi quest)
	{
		if (quest.questRewardType == QuestDataUi.RewardType.isGoldReward)
			UpdateGoldAmount(quest.rewardToAdd);
	}

	//buying/selling item event listeners
	public void OnItemSell(InventoryItemUi item, InventorySlotDataUi slot)
	{
		float goldFromItemSelling = item.price * item.currentStackCount;
		Math.Round(goldFromItemSelling *= 0.9f, 0);
		UpdateGoldAmount((int)goldFromItemSelling);

		transactionTrackerText.text = $"Gold: {goldFromItemSelling}";
		transactionInfoText.text = "Item Sold";
		slot.AddItemToSlot(item);
	}
	public void OnItemTryBuy(InventoryItemUi item, InventorySlotDataUi newSlot, InventorySlotDataUi oldSlot)
	{
		if (item.price * item.currentStackCount > playerGoldAmount)
			OnItemCancelBuy(item, oldSlot, "Cant Afford Item");
		else
		{
			OnItemConfirmBuy(item, newSlot);

			int goldFromItemBuying = -item.price * item.currentStackCount;
			UpdateGoldAmount(goldFromItemBuying);
		}
	}
	public void OnItemConfirmBuy(InventoryItemUi item, InventorySlotDataUi newSlot)
	{
		int goldFromItemBuying = -item.price * item.currentStackCount;
		transactionTrackerText.text = $"Gold: {goldFromItemBuying}";
		transactionInfoText.text = "Item Brought";

		newSlot.AddItemToSlot(item);
	}
	public void OnItemCancelBuy(InventoryItemUi item, InventorySlotDataUi oldSlot, string reason)
	{
		transactionTrackerText.text = $"Gold: {0}";
		transactionInfoText.text = reason;

		oldSlot.AddItemToSlot(item);
	}

	//ITEMS
	//adding items to inventory
	public void AddNewItemToInventory(Items item, bool tryStack)
	{
		if (item.isStackable && tryStack)
			TryStackInventoryItem(ConvertPickupsToInventoryItem(item));
		else
			AddInventoryItemToInventory(ConvertPickupsToInventoryItem(item));
	}
	public void AddInventoryItemToInventory(InventoryItemUi item, bool tryStack)
	{
		if (item.isStackable && tryStack)
			TryStackInventoryItem(item);
		else
			AddInventoryItemToInventory(item);
	}

	//convert Items data to InventoryItemUi format
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
			weapon.level = item.level;
			weapon.enchantmentLevel = item.enchantmentLevel;
			weapon.rarity = item.rarity;
			weapon.Initilize(weapon.rarity, weapon.level, weapon.enchantmentLevel);
			weapon.SetCurrentStackCount(item.currentStackCount);
		}
		else if (item.armorBaseRef != null)
		{
			inventoryItem.armorBaseRef = item.armorBaseRef;
			Armors armor = inventoryItem.AddComponent<Armors>();
			armor.armorBaseRef = item.armorBaseRef;
			armor.level = item.level;
			armor.enchantmentLevel = item.enchantmentLevel;
			armor.rarity = item.rarity;
			armor.Initilize(armor.rarity, armor.level, armor.enchantmentLevel);
			armor.SetCurrentStackCount(item.currentStackCount);
		}
		else if (item.accessoryBaseRef != null)
		{
			inventoryItem.accessoryBaseRef = item.accessoryBaseRef;
			Accessories accessory = inventoryItem.AddComponent<Accessories>();
			accessory.accessoryBaseRef = item.accessoryBaseRef;
			accessory.level = item.level;
			accessory.enchantmentLevel = item.enchantmentLevel;
			accessory.rarity = item.rarity;
			accessory.Initilize(accessory.rarity, accessory.level, accessory.enchantmentLevel);
			accessory.SetCurrentStackCount(item.currentStackCount);
		}
		else if (item.consumableBaseRef != null)
		{
			inventoryItem.consumableBaseRef = item.consumableBaseRef;
			Consumables consumable = inventoryItem.AddComponent<Consumables>();
			consumable.consumableBaseRef = item.consumableBaseRef;
			consumable.level = item.level;
			consumable.enchantmentLevel = item.enchantmentLevel;
			consumable.rarity = item.rarity;
			consumable.Initilize(consumable.rarity, consumable.level, consumable.enchantmentLevel);
			consumable.SetCurrentStackCount(item.currentStackCount);
		}
		else
			Debug.LogError("item.Ref null this shouldnt happen");
	}

	//inventory item stacking
	private void TryStackInventoryItem(InventoryItemUi item)
	{
		for (int i = 0; i < InventorySlots.Count; i++)
		{
			InventorySlotDataUi inventroySlot = InventorySlots[i].GetComponent<InventorySlotDataUi>();

			if (!inventroySlot.IsSlotEmpty())
				AddToStackCount(inventroySlot, item);

			else if (item.currentStackCount > 0)
			{
				AddInventoryItemToInventory(item);
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

	//adding inventory items to free slots
	private void AddInventoryItemToInventory(InventoryItemUi item)
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

	//ABILITIES
	//Add/remove abilities to/from learnt abilities Ui
	private void AddNewUnlockedAbility(SOAbilities newAbility)
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
	public void SetAbilityData(InventoryItemUi inventoryItem, SOAbilities newAbility)
	{
		inventoryItem.abilityBaseRef = newAbility;
		Abilities ability = inventoryItem.AddComponent<Abilities>();
		ability.abilityBaseRef = newAbility;
		ability.Initilize();
	}
	private void OnAbilityRefund(SOAbilities ability)
	{
		foreach (GameObject abilitySlot in LearntAbilitySlots)
		{
			InventorySlotDataUi slotData = abilitySlot.GetComponent<InventorySlotDataUi>();
			if (slotData.itemInSlot == null)
				continue;

			if (slotData.itemInSlot.abilityBaseRef == ability)
			{
				Destroy(slotData.itemInSlot.gameObject);
				//slotData.RemoveItemFromSlot();
				slotData.DestroyItemInSlot();
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
		closeShopButton.onClick.AddListener(delegate { HideNpcShop(npc); });

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
			//slot.RemoveItemFromSlot();
		}

		GameManager.Localplayer.isInteractingWithInteractable = false;
		interactedInventorySlotsUi.SetActive(false);
		npcShopPanalUi.SetActive(false);
		HideInventory();
	}

	//player storage chest
	public void ShowPlayerStoredWeaponsButton()
	{
		HidePlayerStorageChest();
		ShowPlayerStorageChest(0);
	}
	public void ShowPlayerStoredArmourButton()
	{
		HidePlayerStorageChest();
		ShowPlayerStorageChest(1);
	}
	public void ShowPlayerStoredAccessoriesButton()
	{
		HidePlayerStorageChest();
		ShowPlayerStorageChest(2);
	}
	public void ShowPlayerStoredConsumablesButton()
	{
		HidePlayerStorageChest();
		ShowPlayerStorageChest(3);
	}
	public void ShowPlayerStorageChest(int itemTypeToShow)
	{
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

		for (int i = 0; i < playerStoredItems.Count; i++) //move items to ui slots
		{
			InventoryItemUi item = playerStoredItems[i].GetComponent<InventoryItemUi>();
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

		PlayerEventManager.ShowPlayerInventory();
		interactedInventorySlotsUi.SetActive(true);
		storageChestPanelUi.SetActive(true);
		UpdatePlayerInventoryItemsUi(interactedInventorySlots);
	}
	public void HidePlayerStorageChest()
	{
		playerStoredItems.Clear();

		foreach (GameObject obj in interactedInventorySlots) //move to container, clear slots
		{
			InventorySlotDataUi slot = obj.GetComponent<InventorySlotDataUi>();
			if (slot.IsSlotEmpty()) continue;

			slot.itemInSlot.transform.SetParent(playerStoredItemsContainer.transform);
			slot.RemoveItemFromSlot(); //call directly as not being added to new slot
		}

		for (int i = playerStoredItemsContainer.transform.childCount - 1;  i >= 0; i--) //re-add all items + any new ones
			playerStoredItems.Add(playerStoredItemsContainer.transform.GetChild(i).GetComponent<InventoryItemUi>());

		GameManager.Localplayer.isInteractingWithInteractable = false;
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

		UpdateEnchantItemUiInfo(null);
	}
	public void HideEnchanterUi()
	{
		ReturnItemToInventory();
		EnchanterUi.SetActive(false);
		GameManager.Localplayer.isInteractingWithInteractable = false;
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
		else if (item.enchantmentLevel >= 3)
		{
			enchantItemButton.SetActive(false);
			enchanterSlotText.text = $"Cant Enchant {item.itemName}, already max enchantment level";
		}
		else
		{
			int goldCostToEnchant = item.price * (item.enchantmentLevel + 1);

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
		int goldCost = enchanterSlot.itemInSlot.price * (enchanterSlot.itemInSlot.enchantmentLevel + 1);
		UpdateGoldAmount(-goldCost);
		enchanterSlot.EnchantItemInSlot();
		UpdateEnchantItemUiInfo(enchanterSlot.itemInSlot);
	}
	private void ReturnItemToInventory()
	{
		if (enchanterSlot.itemInSlot == null) return;
		Instance.AddInventoryItemToInventory(enchanterSlot.itemInSlot);
	}

	//update items in ui incase any changes were made
	private void UpdatePlayerInventoryItemsUi(List<GameObject> slotListObjs)
	{
		foreach (GameObject obj in slotListObjs)
		{
			InventorySlotDataUi slot = obj.GetComponent<InventorySlotDataUi>();
			if (slot.IsSlotEmpty()) continue;

			InventoryItemUi itemUi = slot.itemInSlot;
			itemUi.CheckIfCanEquipItem();

			if (itemUi.abilityBaseRef != null)
				itemUi.GetComponent<Abilities>().UpdateToolTip(GameManager.Localplayer.playerStats);
			else
				itemUi.GetComponent<Items>().UpdateToolTip(GameManager.Localplayer.playerStats, slot.IsShopSlot());
		}
	}
}
