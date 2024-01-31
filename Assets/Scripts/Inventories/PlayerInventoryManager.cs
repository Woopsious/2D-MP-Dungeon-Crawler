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

	[Header("Starting Items")]
	public GameObject droppedItemPrefab;
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

	public void SpawnStartingItems()
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

	//on item pickup
	private InventoryItem ConvertPickupsToInventoryItem(Items item)
	{
		GameObject go = Instantiate(PlayerInventoryUi.Instance.ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
		InventoryItem newItem = go.GetComponent<InventoryItem>();

		if (item.weaponBaseRef != null)
			SetWeaponData(newItem, item);

		if (item.armorBaseRef != null)
			SetArmorData(newItem, item);

		if (item.accessoryBaseRef != null)
			SetAccessoryData(newItem, item);

		if (item.consumableBaseRef != null)
			SetConsumableData(newItem, item);

		newItem.UpdateUi();
		return newItem;
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

	//set specific item data on pickup
	private void SetWeaponData(InventoryItem inventoryItem, Items item)
	{
		inventoryItem.itemName = item.itemName;
		inventoryItem.itemImage = item.itemImage;
		inventoryItem.itemType = (InventoryItem.ItemType)item.weaponBaseRef.itemType;

		inventoryItem.itemLevel = item.itemLevel;
		inventoryItem.rarity = (InventoryItem.Rarity)item.rarity;
		inventoryItem.classRestriction = (InventoryItem.ClassRestriction)item.weaponBaseRef.classRestriction;

		Weapons weaponData = inventoryItem.AddComponent<Weapons>();
		weaponData.weaponBaseRef = item.weaponBaseRef;
		weaponData.itemLevel = item.itemLevel;

		weaponData.weaponBaseRef.weaponType = item.weaponBaseRef.weaponType;
		weaponData.isShield = item.weaponBaseRef.isShield;
		weaponData.damage = (int)(item.weaponBaseRef.baseDamage * item.levelModifier);
		weaponData.bonusMana = (int)(item.weaponBaseRef.baseBonusMana * item.levelModifier);

		inventoryItem.isStackable = item.isStackable;
		inventoryItem.maxStackCount = item.weaponBaseRef.MaxStackCount;
		inventoryItem.currentStackCount = item.currentStackCount;
	}
	private void SetArmorData(InventoryItem inventoryItem, Items item)
	{
		inventoryItem.itemName = item.itemName;
		inventoryItem.itemImage = item.itemImage;
		inventoryItem.itemType = (InventoryItem.ItemType)item.armorBaseRef.itemType;

		inventoryItem.itemLevel = item.itemLevel;
		inventoryItem.rarity = (InventoryItem.Rarity)item.rarity;
		inventoryItem.classRestriction = (InventoryItem.ClassRestriction)item.armorBaseRef.classRestriction;

		Armors armorData = inventoryItem.AddComponent<Armors>();
		armorData.armorBaseRef = item.armorBaseRef;
		armorData.armorSlot = (Armors.ArmorSlot)item.armorBaseRef.armorSlot;
		armorData.itemLevel = item.itemLevel;

		armorData.bonusHealth = (int)(item.armorBaseRef.baseBonusHealth * item.levelModifier);
		armorData.bonusMana = (int)(item.armorBaseRef.baseBonusMana * item.levelModifier);
		armorData.bonusPhysicalResistance = (int)(item.armorBaseRef.bonusPhysicalResistance * item.levelModifier);
		armorData.bonusPoisonResistance = (int)(item.armorBaseRef.bonusPoisonResistance * item.levelModifier);
		armorData.bonusFireResistance = (int)(item.armorBaseRef.bonusFireResistance * item.levelModifier);
		armorData.bonusIceResistance = (int)(item.armorBaseRef.bonusIceResistance * item.levelModifier);

		inventoryItem.isStackable = item.isStackable;
		inventoryItem.maxStackCount = item.armorBaseRef.MaxStackCount;
		inventoryItem.currentStackCount = item.currentStackCount;
	}
	private void SetAccessoryData(InventoryItem inventoryItem, Items item)
	{
		inventoryItem.itemName = item.itemName;
		inventoryItem.itemImage = item.itemImage;
		inventoryItem.itemType = (InventoryItem.ItemType)item.accessoryBaseRef.itemType;

		inventoryItem.itemLevel = item.itemLevel;
		inventoryItem.rarity = (InventoryItem.Rarity)item.rarity;
		inventoryItem.classRestriction = InventoryItem.ClassRestriction.light;

		Accessories accessoryData = inventoryItem.AddComponent<Accessories>();
		accessoryData.accessoryBaseRef = item.accessoryBaseRef;
		accessoryData.accessorySlot = (Accessories.AccessorySlot)item.accessoryBaseRef.accessorySlot;
		accessoryData.accessoryType = (Accessories.AccessoryType)item.accessoryBaseRef.accessoryType;
		accessoryData.itemLevel = item.itemLevel;

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

		inventoryItem.isStackable = item.isStackable;
		inventoryItem.maxStackCount = item.accessoryBaseRef.MaxStackCount;
		inventoryItem.currentStackCount = item.currentStackCount;
	}
	private void SetConsumableData(InventoryItem inventoryItem, Items item)
	{
		inventoryItem.itemName = item.itemName;
		inventoryItem.itemImage = item.itemImage;

		Consumables consumablesData = inventoryItem.AddComponent<Consumables>();
		consumablesData.consumableBaseRef = item.consumableBaseRef;
		consumablesData.itemLevel = item.itemLevel;

		consumablesData.consumableType = (Consumables.ConsumableType)item.consumableBaseRef.consumableType;
		consumablesData.consumablePercentage = item.consumableBaseRef.consumablePercentage;

		inventoryItem.isStackable = item.isStackable;
		inventoryItem.maxStackCount = item.consumableBaseRef.MaxStackCount;
		inventoryItem.currentStackCount = item.currentStackCount;
	}

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
