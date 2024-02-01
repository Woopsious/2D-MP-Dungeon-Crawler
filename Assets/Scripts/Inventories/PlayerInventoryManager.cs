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

	public GameObject ItemUiPrefab;
	public GameObject droppedItemPrefab;

	[Header("Starting Items")]
	public bool hasRecievedStartingItems;
	public List<SOItems> startingItems = new List<SOItems>();

	public void Awake()
	{
		Instance = this;
	}
	public void Start()
	{
		if(!hasRecievedStartingItems)
			SpawnStartingItems();
	}

	private void OnEnable()
	{
		SaveManager.OnGameLoad += ReloadPlayerInventory;
	}
	private void OnDisable()
	{
		SaveManager.OnGameLoad -= ReloadPlayerInventory;
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
				weapon.currentStackCount = 1;
			}

			if (startingItems[i].itemType == SOItems.ItemType.isArmor)
			{
				Armors armor = go.AddComponent<Armors>();
				armor.armorBaseRef = (SOArmors)startingItems[i];
				armor.currentStackCount = 1;
			}

			if (startingItems[i].itemType == SOItems.ItemType.isAccessory)
			{
				Accessories accessories = go.AddComponent<Accessories>();
				accessories.accessoryBaseRef = (SOAccessories)startingItems[i];
				accessories.currentStackCount = 1;
			}

			if (startingItems[i].itemType == SOItems.ItemType.isConsumable)
			{
				Consumables consumables = go.AddComponent<Consumables>();
				consumables.consumableBaseRef = (SOConsumables)startingItems[i];
				consumables.currentStackCount = 3;
			}

			Items item = go.GetComponent<Items>();
			item.gameObject.name = startingItems[i].name;
			item.itemName = startingItems[i].name;
			item.itemImage = startingItems[i].itemImage;
			item.ItemPrice = startingItems[i].ItemPrice;

			//generic data here, may change if i make unique droppables like keys as they might not have a need for item level etc.
			//im just not sure of a better way to do it atm
			go.AddComponent<Interactables>(); //add interactables script. set randomized stats
			go.GetComponent<Items>().Initilize(Items.Rarity.isCommon, 1, null);
			BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
			collider.isTrigger = true;
		}
	}
	private void ReloadPlayerInventory()
	{
		for (int i = 0; i < SaveManager.Instance.InventoryData.InventoryItems.Count; i++) //spawn item from loot pool at death location
		{
			GameObject go = Instantiate(PlayerInventoryUi.Instance.ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
			InventoryItem newInventoryItem = go.GetComponent<InventoryItem>();

			//foreach (InventoryItem inventoryitem in SaveManager.Instance.InventoryData.InventoryItems)
				//newInventoryItem = inventoryitem;

			SetItemData(newInventoryItem, newInventoryItem);
		}
	}

	//on item pickup
	private InventoryItem ConvertPickupsToInventoryItem(Items item)
	{
		GameObject go = Instantiate(PlayerInventoryUi.Instance.ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
		InventoryItem newItem = go.GetComponent<InventoryItem>();

		SetItemData(newItem, item);
		newItem.UpdateUi();
		return newItem;

		/*
		if (item.weaponBaseRef != null)
			SetWeaponData(newItem, item);

		if (item.armorBaseRef != null)
			SetArmorData(newItem, item);

		if (item.accessoryBaseRef != null)
			SetAccessoryData(newItem, item);

		if (item.consumableBaseRef != null)
			SetConsumableData(newItem, item);
		*/
	}
	public void AddItemToPlayerInventory(Items item)
	{
		if (item.isStackable)
			TryStackItem(ConvertPickupsToInventoryItem(item));
		else
			SpawnNewItemInInventory(ConvertPickupsToInventoryItem(item));
	}

	//item stacking
	private void TryStackItem(InventoryItem newItem)
	{
		for (int i = 0; i < PlayerInventoryUi.Instance.InventorySlots.Count; i++)
		{
			InventorySlot inventroySlot = PlayerInventoryUi.Instance.InventorySlots[i].GetComponent<InventorySlot>();

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
	public void AddToStackCount(InventorySlot inventroySlot, InventoryItem newItem)
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

	//adding new item
	private void SpawnNewItemInInventory(InventoryItem item)
	{
		for (int i = 0; i < PlayerInventoryUi.Instance.InventorySlots.Count; i++)
		{
			InventorySlot inventorySlot = PlayerInventoryUi.Instance.InventorySlots[i].GetComponent<InventorySlot>();

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
	private void SetItemData(InventoryItem inventoryItem, Items item)
	{
		inventoryItem.itemName = item.name;
		inventoryItem.itemImage = item.itemImage;
		inventoryItem.itemPrice = item.ItemPrice;
		inventoryItem.itemLevel = item.itemLevel;
		inventoryItem.rarity = (InventoryItem.Rarity)item.rarity;
		inventoryItem.isStackable = item.isStackable;
		inventoryItem.currentStackCount = item.currentStackCount;

		if (item.weaponBaseRef != null)
		{
			Weapons weapon = inventoryItem.AddComponent<Weapons>();
			weapon.weaponBaseRef = item.weaponBaseRef;
			inventoryItem.weaponBaseRef = item.weaponBaseRef;
			inventoryItem.itemType = (InventoryItem.ItemType)item.weaponBaseRef.itemType;
			inventoryItem.maxStackCount = item.weaponBaseRef.MaxStackCount;
			weapon.itemLevel = item.itemLevel;
			weapon.rarity = item.rarity;
			weapon.Initilize(weapon.rarity, weapon.itemLevel, null);
		}
		if (item.armorBaseRef != null)
		{
			Armors armor = inventoryItem.AddComponent<Armors>();
			armor.armorBaseRef = item.armorBaseRef;
			inventoryItem.armorBaseRef = item.armorBaseRef;
			inventoryItem.itemType = (InventoryItem.ItemType)item.armorBaseRef.itemType;
			inventoryItem.maxStackCount = item.armorBaseRef.MaxStackCount;
			armor.itemLevel = item.itemLevel;
			armor.rarity = item.rarity;
			armor.Initilize(armor.rarity, armor.itemLevel, null);
		}
		if (item.accessoryBaseRef != null)
		{
			Accessories accessory = inventoryItem.AddComponent<Accessories>();
			accessory.accessoryBaseRef = item.accessoryBaseRef;
			inventoryItem.accessoryBaseRef = item.accessoryBaseRef;
			inventoryItem.itemType = (InventoryItem.ItemType)item.accessoryBaseRef.itemType;
			inventoryItem.maxStackCount = item.accessoryBaseRef.MaxStackCount;
			accessory.itemLevel = item.itemLevel;
			accessory.rarity = item.rarity;
			accessory.Initilize(accessory.rarity, accessory.itemLevel, null);
		}
		if (item.consumableBaseRef != null)
		{
			Consumables consumable = inventoryItem.AddComponent<Consumables>();
			consumable.consumableBaseRef = item.consumableBaseRef;
			inventoryItem.consumableBaseRef = item.consumableBaseRef;
			inventoryItem.itemType = (InventoryItem.ItemType)item.consumableBaseRef.itemType;
			inventoryItem.maxStackCount = item.consumableBaseRef.MaxStackCount;
			consumable.itemLevel = item.itemLevel;
			consumable.rarity = item.rarity;
			consumable.Initilize(consumable.rarity, consumable.itemLevel, null);
		}
	}
	private void SetItemData(InventoryItem inventoryItem, InventoryItem item)
	{
		inventoryItem.itemName = item.name;
		inventoryItem.itemImage = item.itemImage;
		inventoryItem.itemPrice = item.itemPrice;
		inventoryItem.itemLevel = item.itemLevel;
		inventoryItem.rarity = (InventoryItem.Rarity)item.rarity;
		inventoryItem.isStackable = item.isStackable;
		inventoryItem.currentStackCount = item.currentStackCount;

		if (item.weaponBaseRef != null)
		{
			Weapons weapon = inventoryItem.AddComponent<Weapons>();
			weapon.weaponBaseRef = item.weaponBaseRef;
			inventoryItem.weaponBaseRef = item.weaponBaseRef;
			inventoryItem.itemType = (InventoryItem.ItemType)item.weaponBaseRef.itemType;
			inventoryItem.maxStackCount = item.weaponBaseRef.MaxStackCount;
			weapon.itemLevel = item.itemLevel;
			weapon.rarity = (Items.Rarity)item.rarity;
			weapon.Initilize(weapon.rarity, weapon.itemLevel, null);
		}
		if (item.armorBaseRef != null)
		{
			Armors armor = inventoryItem.AddComponent<Armors>();
			armor.armorBaseRef = item.armorBaseRef;
			inventoryItem.armorBaseRef = item.armorBaseRef;
			inventoryItem.itemType = (InventoryItem.ItemType)item.armorBaseRef.itemType;
			inventoryItem.maxStackCount = item.armorBaseRef.MaxStackCount;
			armor.itemLevel = item.itemLevel;
			armor.rarity = (Items.Rarity)item.rarity;
			armor.Initilize(armor.rarity, armor.itemLevel, null);
		}
		if (item.accessoryBaseRef != null)
		{
			Accessories accessory = inventoryItem.AddComponent<Accessories>();
			accessory.accessoryBaseRef = item.accessoryBaseRef;
			inventoryItem.accessoryBaseRef = item.accessoryBaseRef;
			inventoryItem.itemType = (InventoryItem.ItemType)item.accessoryBaseRef.itemType;
			inventoryItem.maxStackCount = item.accessoryBaseRef.MaxStackCount;
			accessory.itemLevel = item.itemLevel;
			accessory.rarity = (Items.Rarity)item.rarity;
			accessory.Initilize(accessory.rarity, accessory.itemLevel, null);
		}
		if (item.consumableBaseRef != null)
		{
			Consumables consumable = inventoryItem.AddComponent<Consumables>();
			consumable.consumableBaseRef = item.consumableBaseRef;
			inventoryItem.consumableBaseRef = item.consumableBaseRef;
			inventoryItem.itemType = (InventoryItem.ItemType)item.consumableBaseRef.itemType;
			inventoryItem.maxStackCount = item.consumableBaseRef.MaxStackCount;
			consumable.itemLevel = item.itemLevel;
			consumable.rarity = (Items.Rarity)item.rarity;
			consumable.Initilize(consumable.rarity, consumable.itemLevel, null);
		}
	}

	//set specific item data on pickup
	/*
	private void SetWeaponData(InventoryItem inventoryItem, Items item)
	{
		Weapons weaponData = inventoryItem.AddComponent<Weapons>();
		weaponData.itemName = item.name;
		weaponData.itemImage = item.itemImage;
		weaponData.ItemPrice = item.ItemPrice;
		weaponData.itemLevel = item.itemLevel;

		weaponData.weaponBaseRef = item.weaponBaseRef;
		inventoryItem.weaponBaseRef = item.weaponBaseRef;
		inventoryItem.itemType = (InventoryItem.ItemType)item.weaponBaseRef.itemType;
		inventoryItem.classRestriction = (InventoryItem.ClassRestriction)item.weaponBaseRef.classRestriction;

		weaponData.isStackable = item.isStackable;
		weaponData.currentStackCount = item.currentStackCount;
		weaponData.inventroySlot = item.inventroySlot;
		inventoryItem.maxStackCount = item.weaponBaseRef.MaxStackCount;

		weaponData.weaponBaseRef.weaponType = item.weaponBaseRef.weaponType;
		weaponData.isShield = item.weaponBaseRef.isShield;
		weaponData.damage = (int)(item.weaponBaseRef.baseDamage * item.levelModifier);
		weaponData.bonusMana = (int)(item.weaponBaseRef.baseBonusMana * item.levelModifier);
	}
	private void SetArmorData(InventoryItem inventoryItem, Items item)
	{
		Armors armorData = inventoryItem.AddComponent<Armors>();
		armorData.itemName = item.name;
		armorData.itemImage = item.itemImage;
		armorData.ItemPrice = item.ItemPrice;
		armorData.itemLevel = item.itemLevel;

		armorData.armorBaseRef = item.armorBaseRef;
		inventoryItem.armorBaseRef = item.armorBaseRef;
		inventoryItem.itemType = (InventoryItem.ItemType)item.armorBaseRef.itemType;
		inventoryItem.classRestriction = (InventoryItem.ClassRestriction)item.armorBaseRef.classRestriction;
		armorData.armorSlot = (Armors.ArmorSlot)item.armorBaseRef.armorSlot;

		armorData.isStackable = item.isStackable;
		armorData.currentStackCount = item.currentStackCount;
		armorData.inventroySlot = item.inventroySlot;
		inventoryItem.maxStackCount = item.armorBaseRef.MaxStackCount;

		armorData.bonusHealth = (int)(item.armorBaseRef.baseBonusHealth * item.levelModifier);
		armorData.bonusMana = (int)(item.armorBaseRef.baseBonusMana * item.levelModifier);
		armorData.bonusPhysicalResistance = (int)(item.armorBaseRef.bonusPhysicalResistance * item.levelModifier);
		armorData.bonusPoisonResistance = (int)(item.armorBaseRef.bonusPoisonResistance * item.levelModifier);
		armorData.bonusFireResistance = (int)(item.armorBaseRef.bonusFireResistance * item.levelModifier);
		armorData.bonusIceResistance = (int)(item.armorBaseRef.bonusIceResistance * item.levelModifier);
	}
	private void SetAccessoryData(InventoryItem inventoryItem, Items item)
	{
		Accessories accessoryData = inventoryItem.AddComponent<Accessories>();
		accessoryData.itemName = item.name;
		accessoryData.itemImage = item.itemImage;
		accessoryData.ItemPrice = item.ItemPrice;
		accessoryData.itemLevel = item.itemLevel;

		accessoryData.accessoryBaseRef = item.accessoryBaseRef;
		inventoryItem.accessoryBaseRef = item.accessoryBaseRef;
		inventoryItem.itemType = (InventoryItem.ItemType)item.accessoryBaseRef.itemType;
		accessoryData.accessorySlot = (Accessories.AccessorySlot)item.accessoryBaseRef.accessorySlot;
		accessoryData.accessoryType = (Accessories.AccessoryType)item.accessoryBaseRef.accessoryType;

		accessoryData.isStackable = item.isStackable;
		accessoryData.currentStackCount = item.currentStackCount;
		accessoryData.inventroySlot = item.inventroySlot;
		inventoryItem.maxStackCount = item.accessoryBaseRef.MaxStackCount;

		accessoryData.damageTypeToBoost = (Accessories.DamageTypeToBoost)item.accessoryBaseRef.damageTypeToBoost;
		accessoryData.bonusHealth = (int)(item.accessoryBaseRef.baseBonusHealth * item.levelModifier);
		accessoryData.bonusMana = (int)(item.accessoryBaseRef.baseBonusMana * item.levelModifier);

		if (accessoryData.rarity == Items.Rarity.isCommon)
			accessoryData.bonusPercentageValue = item.accessoryBaseRef.bonusPercentageValue[0];
		else if (accessoryData.rarity == Items.Rarity.isRare)
			accessoryData.bonusPercentageValue = item.accessoryBaseRef.bonusPercentageValue[1];
		else if (accessoryData.rarity == Items.Rarity.isEpic)
			accessoryData.bonusPercentageValue = item.accessoryBaseRef.bonusPercentageValue[2];
		else if (accessoryData.rarity == Items.Rarity.isLegendary)
			accessoryData.bonusPercentageValue = item.accessoryBaseRef.bonusPercentageValue[3];

		accessoryData.bonusPhysicalResistance = (int)(item.accessoryBaseRef.bonusPhysicalResistance * item.levelModifier);
		accessoryData.bonusPoisonResistance = (int)(item.accessoryBaseRef.bonusPoisonResistance * item.levelModifier);
		accessoryData.bonusFireResistance = (int)(item.accessoryBaseRef.bonusFireResistance * item.levelModifier);
		accessoryData.bonusIceResistance = (int)(item.accessoryBaseRef.bonusIceResistance * item.levelModifier);
	}
	private void SetConsumableData(InventoryItem inventoryItem, Items item)
	{
		Consumables consumablesData = inventoryItem.AddComponent<Consumables>();
		consumablesData.itemName = item.name;
		consumablesData.itemImage = item.itemImage;
		consumablesData.ItemPrice = item.ItemPrice;
		consumablesData.itemLevel = item.itemLevel;

		consumablesData.consumableBaseRef = item.consumableBaseRef;
		inventoryItem.consumableBaseRef = item.consumableBaseRef;
		inventoryItem.itemType = (InventoryItem.ItemType)item.consumableBaseRef.itemType;

		consumablesData.isStackable = item.isStackable;
		consumablesData.currentStackCount = item.currentStackCount;
		consumablesData.inventroySlot = item.inventroySlot;
		inventoryItem.maxStackCount = item.consumableBaseRef.MaxStackCount;

		consumablesData.consumableType = (Consumables.ConsumableType)item.consumableBaseRef.consumableType;
		consumablesData.consumablePercentage = item.consumableBaseRef.consumablePercentage;
	}
	*/

	//bool check before item pickup
	public bool CheckIfInventoryFull()
	{
		int numOfFilledSlots = 0;

		foreach (GameObject obj in PlayerInventoryUi.Instance.InventorySlots)
		{
			InventorySlot inventorySlot = obj.GetComponent<InventorySlot>();

			if (!inventorySlot.IsSlotEmpty())
				numOfFilledSlots++;
		}

		if (numOfFilledSlots == PlayerInventoryUi.Instance.InventorySlots.Count)
			return true;
		else return false;
	}
}
