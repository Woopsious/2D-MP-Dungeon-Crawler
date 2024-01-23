using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.VisualScripting;
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
			go.GetComponent<Items>().SetItemStats(Items.Rarity.isCommon, 1, null);
			BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
			collider.isTrigger = true;
		}
	}

	//on item pickup
	public InventoryItem ConvertPickupsToInventoryItem(Items item)
	{
		GameObject go = Instantiate(InventoryUi.Instance.ItemUiPrefab, gameObject.transform.position, Quaternion.identity);
		InventoryItem newItem = go.GetComponent<InventoryItem>();

		if (item.weaponBaseRef != null)
			SetWeaponData(newItem, item);

		if (item.armorBaseRef != null)
			SetArmorData(newItem, item);

		if (item.accessoryBaseRef != null)
			SetAccessoryData(newItem, item);

		if (item.consumableBaseRef != null)
			SetConsumableData(newItem, item);

		newItem.UpdateName();
		newItem.UpdateImage();
		newItem.UpdateItemLevel();
		newItem.UpdateStackCounter();

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
	public void TryStackItem(InventoryItem newItem)
	{
		for (int i = 0; i < InventoryUi.Instance.InventorySlots.Count; i++)
		{
			InventorySlot inventroySlot = InventoryUi.Instance.InventorySlots[i].GetComponent<InventorySlot>();

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
				if (newItem.currentStackCount <= 0) return; //stop adding

				newItem.currentStackCount--;
				newItem.UpdateStackCounter();
				itemInSlot.currentStackCount++;
				itemInSlot.UpdateStackCounter();
			}
			if (newItem.currentStackCount != 0) //if stackcount still has more find next stackable item
				return;
		}
		else
			return;
	}

	//adding new item
	public void SpawnNewItemInInventory(InventoryItem item)
	{
		for (int i = 0; i < InventoryUi.Instance.InventorySlots.Count; i++)
		{
			InventorySlot inventorySlot = InventoryUi.Instance.InventorySlots[i].GetComponent<InventorySlot>();

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
	public void SetWeaponData(InventoryItem inventoryItem, Items item)
	{
		inventoryItem.itemName = item.itemName;
		inventoryItem.itemImage = item.itemImage;
		inventoryItem.itemType = (InventoryItem.ItemType)item.weaponBaseRef.itemType;

		inventoryItem.itemLevel = item.itemLevel;
		inventoryItem.rarity = (InventoryItem.Rarity)item.rarity;
		inventoryItem.classRestriction = (InventoryItem.ClassRestriction)item.weaponBaseRef.classRestriction;
		inventoryItem.weaponType = (InventoryItem.WeaponType)item.weaponBaseRef.weaponType;

		inventoryItem.weaponBaseRef = item.weaponBaseRef;
		inventoryItem.isShield = item.weaponBaseRef.isShield;
		inventoryItem.damage = (int)(item.weaponBaseRef.baseDamage * item.statModifier);
		inventoryItem.bonusMana = (int)(item.weaponBaseRef.baseBonusMana * item.statModifier);

		inventoryItem.maxStackCount = item.weaponBaseRef.MaxStackCount;
		inventoryItem.currentStackCount = item.currentStackCount;
	}
	public void SetArmorData(InventoryItem inventoryItem, Items item)
	{
		inventoryItem.itemName = item.itemName;
		inventoryItem.itemImage = item.itemImage;
		inventoryItem.itemType = (InventoryItem.ItemType)item.armorBaseRef.itemType;

		inventoryItem.itemLevel = item.itemLevel;
		inventoryItem.rarity = (InventoryItem.Rarity)item.rarity;
		inventoryItem.classRestriction = (InventoryItem.ClassRestriction)item.armorBaseRef.classRestriction;
		inventoryItem.armorSlot = (InventoryItem.ArmorSlot)item.armorBaseRef.armorSlot;

		inventoryItem.armorBaseRef = item.armorBaseRef;
		inventoryItem.bonusHealth = (int)(item.armorBaseRef.baseBonusHealth * item.statModifier);
		inventoryItem.bonusMana = (int)(item.armorBaseRef.baseBonusMana * item.statModifier);

		inventoryItem.bonusPhysicalResistance = (int)(item.armorBaseRef.bonusPhysicalResistance * item.statModifier);
		inventoryItem.bonusPoisonResistance = (int)(item.armorBaseRef.bonusPoisonResistance * item.statModifier);
		inventoryItem.bonusFireResistance = (int)(item.armorBaseRef.bonusFireResistance * item.statModifier);
		inventoryItem.bonusIceResistance = (int)(item.armorBaseRef.bonusIceResistance * item.statModifier);

		inventoryItem.isStackable = item.isStackable;
		inventoryItem.maxStackCount = item.armorBaseRef.MaxStackCount;
		inventoryItem.currentStackCount = item.currentStackCount;
	}
	public void SetAccessoryData(InventoryItem inventoryItem, Items item)
	{
		inventoryItem.itemName = item.itemName;
		inventoryItem.itemImage = item.itemImage;
		inventoryItem.itemType = (InventoryItem.ItemType)item.accessoryBaseRef.itemType;

		inventoryItem.itemLevel = item.itemLevel;
		inventoryItem.rarity = (InventoryItem.Rarity)item.rarity;
		inventoryItem.classRestriction = InventoryItem.ClassRestriction.light;
		inventoryItem.accessorySlot = (InventoryItem.AccessorySlot)item.accessoryBaseRef.accessorySlot;
		inventoryItem.accessoryType = (InventoryItem.AccessoryType)item.accessoryBaseRef.accessoryType;
		inventoryItem.damageTypeToBoost = (InventoryItem.DamageTypeToBoost)item.accessoryBaseRef.damageTypeToBoost;

		inventoryItem.accessoryBaseRef = item.accessoryBaseRef;
		inventoryItem.bonusHealth = (int)(item.accessoryBaseRef.baseBonusHealth * item.statModifier);
		inventoryItem.bonusMana = (int)(item.accessoryBaseRef.baseBonusMana * item.statModifier);

		if (inventoryItem.rarity == InventoryItem.Rarity.isCommon)
			inventoryItem.bonusDamagePercentageValue = item.accessoryBaseRef.bonusPercentageValue[0];
		else if (inventoryItem.rarity == InventoryItem.Rarity.isRare)
			inventoryItem.bonusDamagePercentageValue = item.accessoryBaseRef.bonusPercentageValue[1];
		else if (inventoryItem.rarity == InventoryItem.Rarity.isEpic)
			inventoryItem.bonusDamagePercentageValue = item.accessoryBaseRef.bonusPercentageValue[2];
		else if (inventoryItem.rarity == InventoryItem.Rarity.isLegendary)
			inventoryItem.bonusDamagePercentageValue = item.accessoryBaseRef.bonusPercentageValue[3];

		inventoryItem.bonusPhysicalResistance = (int)(item.accessoryBaseRef.bonusPhysicalResistance * item.statModifier);
		inventoryItem.bonusPoisonResistance = (int)(item.accessoryBaseRef.bonusPoisonResistance * item.statModifier);
		inventoryItem.bonusFireResistance = (int)(item.accessoryBaseRef.bonusFireResistance * item.statModifier);
		inventoryItem.bonusIceResistance = (int)(item.accessoryBaseRef.bonusIceResistance * item.statModifier);

		inventoryItem.isStackable = item.isStackable;
		inventoryItem.maxStackCount = item.accessoryBaseRef.MaxStackCount;
		inventoryItem.currentStackCount = item.currentStackCount;
	}
	public void SetConsumableData(InventoryItem inventoryItem, Items item)
	{
		inventoryItem.itemName = item.itemName;
		inventoryItem.itemImage = item.itemImage;
		inventoryItem.consumableBaseRef = item.consumableBaseRef;

		inventoryItem.healthRestoration = item.consumableBaseRef.healthRestoration;
		inventoryItem.manaRestoration = item.consumableBaseRef.manaRestoration;

		inventoryItem.isStackable = item.isStackable;
		inventoryItem.maxStackCount = item.consumableBaseRef.MaxStackCount;
		inventoryItem.currentStackCount = item.currentStackCount;
	}

	//bool check before item pickup
	public bool CheckIfInventoryFull()
	{
		int numOfFilledSlots = 0;

		foreach (GameObject obj in InventoryUi.Instance.InventorySlots)
		{
			InventorySlot inventorySlot = obj.GetComponent<InventorySlot>();

			if (!inventorySlot.IsSlotEmpty())
				numOfFilledSlots++;
		}

		if (numOfFilledSlots == InventoryUi.Instance.InventorySlots.Count)
			return true;
		else return false;
	}
}
