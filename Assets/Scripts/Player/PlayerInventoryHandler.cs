using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInventoryHandler : MonoBehaviour
{
	public static PlayerInventoryHandler Instance;

	[Header("Player Starting Items")]
	public GameObject droppedItemPrefab;
	public bool hasRecievedStartingItems;
	public bool debugSpawnStartingItems;
	public List<SOItems> debugStartingItems = new List<SOItems>();

	[Header("Player Gold")]
	public int playerGoldAmount;
	public int startingGold;

	public void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadPlayerInventory;
	}
	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadPlayerInventory;
	}

	public void TrySpawnStartingItems(SOClasses playerClass)
	{
		if (SaveManager.Instance != null) //skip when in testing scene
		{
			if (SaveManager.Instance.GameData.hasRecievedStartingItems) return;
			SaveManager.Instance.GameData.hasRecievedStartingItems = true;
		}

		hasRecievedStartingItems = true;

		PlayerInventoryUi.Instance.UpdateGoldAmount(startingGold);

		for (int i = 0; i < playerClass.startingItems.Count; i++)
		{
			GameObject go = Instantiate(droppedItemPrefab, gameObject.transform.position, Quaternion.identity);

			if (playerClass.startingItems[i].itemType == SOItems.ItemType.isWeapon)
			{
				Weapons weapon = go.AddComponent<Weapons>();
				weapon.weaponBaseRef = (SOWeapons)playerClass.startingItems[i];
				weapon.SetCurrentStackCount(1);
			}

			if (playerClass.startingItems[i].itemType == SOItems.ItemType.isArmor)
			{
				Armors armor = go.AddComponent<Armors>();
				armor.armorBaseRef = (SOArmors)playerClass.startingItems[i];
				armor.SetCurrentStackCount(1);
			}

			if (playerClass.startingItems[i].itemType == SOItems.ItemType.isAccessory)
			{
				Accessories accessories = go.AddComponent<Accessories>();
				accessories.accessoryBaseRef = (SOAccessories)playerClass.startingItems[i];
				accessories.SetCurrentStackCount(1);
			}

			if (playerClass.startingItems[i].itemType == SOItems.ItemType.isConsumable)
			{
				Consumables consumables = go.AddComponent<Consumables>();
				consumables.consumableBaseRef = (SOConsumables)playerClass.startingItems[i];
				consumables.SetCurrentStackCount(3);
			}

			Items item = go.GetComponent<Items>();
			item.gameObject.name = playerClass.startingItems[i].name;
			item.itemName = playerClass.startingItems[i].name;
			item.itemSprite = playerClass.startingItems[i].itemImage;
			item.itemPrice = playerClass.startingItems[i].itemPrice;

			go.GetComponent<Items>().Initilize(Items.Rarity.isCommon, GetComponent<EntityStats>().entityLevel);
			BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
			collider.isTrigger = true;
		}

		if (debugSpawnStartingItems)
			SpawnDebugStartingItems();
	}
	private void SpawnDebugStartingItems()
	{
		for (int i = 0; i < debugStartingItems.Count; i++)
		{
			GameObject go = Instantiate(droppedItemPrefab, gameObject.transform.position, Quaternion.identity);

			if (debugStartingItems[i].itemType == SOItems.ItemType.isWeapon)
			{
				Weapons weapon = go.AddComponent<Weapons>();
				weapon.weaponBaseRef = (SOWeapons)debugStartingItems[i];
				weapon.SetCurrentStackCount(1);
			}

			if (debugStartingItems[i].itemType == SOItems.ItemType.isArmor)
			{
				Armors armor = go.AddComponent<Armors>();
				armor.armorBaseRef = (SOArmors)debugStartingItems[i];
				armor.SetCurrentStackCount(1);
			}

			if (debugStartingItems[i].itemType == SOItems.ItemType.isAccessory)
			{
				Accessories accessories = go.AddComponent<Accessories>();
				accessories.accessoryBaseRef = (SOAccessories)debugStartingItems[i];
				accessories.SetCurrentStackCount(1);
			}

			if (debugStartingItems[i].itemType == SOItems.ItemType.isConsumable)
			{
				Consumables consumables = go.AddComponent<Consumables>();
				consumables.consumableBaseRef = (SOConsumables)debugStartingItems[i];
				consumables.SetCurrentStackCount(3);
			}

			Items item = go.GetComponent<Items>();
			item.gameObject.name = debugStartingItems[i].name;
			item.itemName = debugStartingItems[i].name;
			item.itemSprite = debugStartingItems[i].itemImage;
			item.itemPrice = debugStartingItems[i].itemPrice;

			go.GetComponent<Items>().Initilize(Items.Rarity.isLegendary, 1);
			BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
			collider.isTrigger = true;
		}
	}
	private void ReloadPlayerInventory()
	{
		hasRecievedStartingItems = SaveManager.Instance.GameData.hasRecievedStartingItems;
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
			InventorySlotDataUi inventorySlot = obj.GetComponent<InventorySlotDataUi>();

			if (!inventorySlot.IsSlotEmpty())
				numOfFilledSlots++;
		}

		if (numOfFilledSlots == PlayerInventoryUi.Instance.InventorySlots.Count)
			return true;
		else return false;
	}
}
