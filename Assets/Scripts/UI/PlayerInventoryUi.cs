using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

	[Header("Inventory items")]
	public GameObject LearntAbilitiesUi;
	public List<GameObject> LearntAbilitySlots = new List<GameObject>();

	[Header("Inventory items")]
	public GameObject InventoryUi;
	public List<GameObject> InventorySlots = new List<GameObject>();

	[Header("Equipment items")]
	public GameObject EquipmentUi;
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

	[Header("Interacted Npc Ui")]
	public TMP_Text transactionTrackerText;
	public GameObject npcShopPanalUi;
	public List<GameObject> shopSlots = new List<GameObject>();
	public Button refreshShopButton;
	public Button closeShopButton;

	public int goldTransaction;

	private void Awake()
	{
		Instance = this;
	}
	private void Start()
	{
		Initilize();
	}
	private void OnEnable()
	{
		SaveManager.OnGameLoad += ReloadPlayerInventory;
		ClassesUi.OnClassReset += OnClassReset;
		ClassesUi.OnNewAbilityUnlock += AddNewUnlockedAbility;
		InventorySlotUi.OnItemBuyEvent += OnItemBuy;
		InventorySlotUi.OnItemSellEvent += OnItemSell;

		EventManagerUi.OnShowPlayerInventoryEvent += ShowInventory;
		EventManagerUi.OnShowPlayerClassSelectionEvent += HideInventory;
		EventManagerUi.OnShowPlayerSkillTreeEvent += HideInventory;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent += HideInventory;
		EventManagerUi.OnShowPlayerJournalEvent += HideInventory;

		EventManagerUi.OnShowPlayerInventoryEvent += HideLearntAbilities;
		EventManagerUi.OnShowPlayerClassSelectionEvent += HideLearntAbilities;
		EventManagerUi.OnShowPlayerSkillTreeEvent += HideLearntAbilities;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent += ShowLearntAbilities;
		EventManagerUi.OnShowPlayerJournalEvent += HideLearntAbilities;
	}
	private void OnDisable()
	{
		SaveManager.OnGameLoad -= ReloadPlayerInventory;
		ClassesUi.OnClassReset -= OnClassReset;
		ClassesUi.OnNewAbilityUnlock -= AddNewUnlockedAbility;
		InventorySlotUi.OnItemBuyEvent -= OnItemBuy;
		InventorySlotUi.OnItemSellEvent -= OnItemSell;

		EventManagerUi.OnShowPlayerInventoryEvent -= ShowInventory;
		EventManagerUi.OnShowPlayerClassSelectionEvent -= HideInventory;
		EventManagerUi.OnShowPlayerSkillTreeEvent -= HideInventory;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent -= HideInventory;
		EventManagerUi.OnShowPlayerJournalEvent -= HideInventory;

		EventManagerUi.OnShowPlayerInventoryEvent -= HideLearntAbilities;
		EventManagerUi.OnShowPlayerClassSelectionEvent -= HideLearntAbilities;
		EventManagerUi.OnShowPlayerSkillTreeEvent -= HideLearntAbilities;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent -= ShowLearntAbilities;
		EventManagerUi.OnShowPlayerJournalEvent -= HideLearntAbilities;
	}
	private void Initilize()
	{
		PlayerInfoAndInventoryPanelUi.SetActive(false);

		foreach (GameObject slot in LearntAbilitySlots)
			slot.GetComponent<InventorySlotUi>().SetSlotIndex();

		foreach (GameObject slot in InventorySlots)
			slot.GetComponent<InventorySlotUi>().SetSlotIndex();

		foreach (GameObject slot in EquipmentSlots)
			slot.GetComponent<InventorySlotUi>().SetSlotIndex();
	}

	//reload player inventory
	private void ReloadPlayerInventory()
	{
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
			InventoryItem newInventoryItem = go.GetComponent<InventoryItem>();

			ReloadItemData(newInventoryItem, itemData);
			InventorySlotUi inventorySlot = gameObjects[itemData.inventorySlotIndex].GetComponent<InventorySlotUi>();

			newInventoryItem.Initilize();
			newInventoryItem.transform.SetParent(inventorySlot.transform);
			newInventoryItem.parentAfterDrag = InventorySlots[0].transform;
			inventorySlot.AddItemToSlot(newInventoryItem);
		}
	}
	private void ReloadItemData(InventoryItem inventoryItem, InventoryItemData itemData)
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
		InventoryItem item = go.GetComponent<InventoryItem>();
		SetAbilityData(item, newAbility);

		for (int i = 0; i < LearntAbilitySlots.Count; i++)
		{
			InventorySlotUi inventorySlot = LearntAbilitySlots[i].GetComponent<InventorySlotUi>();

			if (inventorySlot.IsSlotEmpty())
			{
				item.inventorySlotIndex = i;
				item.transform.SetParent(inventorySlot.transform);
				item.SetTextColour();
				inventorySlot.itemInSlot = item;
				inventorySlot.UpdateSlotSize();
				item.Initilize();

				return;
			}
		}
	}
	public void SetAbilityData(InventoryItem inventoryItem, SOClassAbilities newAbility)
	{
		inventoryItem.abilityBaseRef = newAbility;
		Abilities ability = inventoryItem.AddComponent<Abilities>();
		ability.abilityBaseRef = newAbility;
		ability.abilityName = newAbility.Name;
		ability.abilityDescription = newAbility.Description;
		ability.abilitySprite = newAbility.abilitySprite;
		ability.isEquippedAbility = false;
		ability.isOnCooldown = false;
	}

	//ITEMS
	//Adding new items to Ui
	public void AddItemToInventory(Items item)
	{
		if (item.isStackable)
			TryStackItem(ConvertPickupsToInventoryItem(item));
		else
			SpawnNewItemInInventory(ConvertPickupsToInventoryItem(item));
	}
	private InventoryItem ConvertPickupsToInventoryItem(Items item)
	{
		GameObject go = Instantiate(ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
		InventoryItem newItem = go.GetComponent<InventoryItem>();

		SetItemData(newItem, item);
		newItem.Initilize();
		return newItem;
	}
	private void SetItemData(InventoryItem inventoryItem, Items item)
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

	//buying/selling items
	public void OnItemSell(InventoryItem item)
	{
		goldTransaction = item.itemPrice * item.currentStackCount;
		transactionTrackerText.text = $"Gold: {goldTransaction}";
	}
	public void OnItemBuy(InventoryItem item)
	{
		goldTransaction = -item.itemPrice * item.currentStackCount;
		transactionTrackerText.text = $"Gold: {goldTransaction}";
	}

	//adding new item to ui
	private void SpawnNewItemInInventory(InventoryItem item)
	{
		for (int i = 0; i < InventorySlots.Count; i++)
		{
			InventorySlotUi inventorySlot = InventorySlots[i].GetComponent<InventorySlotUi>();

			if (inventorySlot.IsSlotEmpty())
			{
				item.inventorySlotIndex = i;
				item.transform.SetParent(inventorySlot.transform);
				item.SetTextColour();
				inventorySlot.itemInSlot = item;
				inventorySlot.UpdateSlotSize();

				return;
			}
		}
	}

	//stack item to existing ui items
	private void TryStackItem(InventoryItem newItem)
	{
		for (int i = 0; i < InventorySlots.Count; i++)
		{
			InventorySlotUi inventroySlot = InventorySlots[i].GetComponent<InventorySlotUi>();

			if (!inventroySlot.IsSlotEmpty())
			{
				AddToStackCount(inventroySlot, newItem);
			}
			else if (newItem.currentStackCount > 0)
			{
				SpawnNewItemInInventory(newItem);
				return;
			}
		}
	}
	public void AddToStackCount(InventorySlotUi inventroySlot, InventoryItem newItem)
	{
		InventoryItem itemInSlot = inventroySlot.GetComponentInChildren<InventoryItem>();
		if (inventroySlot.IsItemInSlotSameAs(newItem) && itemInSlot.currentStackCount < itemInSlot.maxStackCount)
		{
			//add to itemInSlot.CurrentStackCount till maxStackCountReached
			for (int itemCount = itemInSlot.currentStackCount; itemCount < itemInSlot.maxStackCount; itemCount++)
			{
				newItem.DecreaseStackCounter();
				itemInSlot.IncreaseStackCounter();
			}
			if (newItem.currentStackCount != 0) //if stackcount still has more find next stackable item
				return;
		}
		else
			return;
	}

	//UI CHANGES
	public void ShowHideInventoryKeybind()
	{
		if (PlayerInfoAndInventoryPanelUi.activeInHierarchy)
			HideInventory();
		else
			HideInventory();
	}
	public void ShowInventory()
	{
		if (PlayerInfoAndInventoryPanelUi.activeInHierarchy)
			HideInventory();
		else
			PlayerInfoAndInventoryPanelUi.SetActive(true);
	}
	public void HideInventory()
	{
		PlayerInfoAndInventoryPanelUi.SetActive(false);
	}

	public void ShowHideLearntAbilitiesKeybind()
	{
		if (LearntAbilitiesUi.activeInHierarchy)
			HideLearntAbilities();
		else
			ShowLearntAbilities();
	}
	public void ShowLearntAbilities()
	{
		if (LearntAbilitiesUi.activeInHierarchy)
			HideLearntAbilities();
		else
			LearntAbilitiesPanelUi.SetActive(true);
	}
	public void HideLearntAbilities()
	{
		LearntAbilitiesPanelUi.SetActive(false);
	}

	//npc shop
	public void ShowNpcShop()
	{
		npcShopPanalUi.SetActive(true);
	}
	public void HideNpcShop()
	{
		npcShopPanalUi.SetActive(false);
	}
}
