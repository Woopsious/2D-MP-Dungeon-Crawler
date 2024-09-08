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
	public bool debugSpawnStartingItems;

	public bool hasRecievedStartingItems;
	public bool hasRecievedKnightItems;
	public bool hasRecievedWarriorItems;
	public bool hasRecievedRogueItems;
	public bool hasRecievedRangerItems;
	public bool hasRecievedMageItems;

	[Header("Player Starting Gold")]
	public int startingGold;

	public void Awake()
	{
		Instance = this;
		hasRecievedStartingItems = false;
		hasRecievedKnightItems = false;
		hasRecievedWarriorItems = false;
		hasRecievedRogueItems = false;
		hasRecievedRangerItems = false;
		hasRecievedMageItems = false;
	}

	//spawn starting items based on starting class + if already receieved them
	public void TrySpawnStartingItems(SOClasses playerClass)
	{
		if (debugSpawnStartingItems)
		{
			DebugSpawnStartingItems(playerClass);
			return;
		}
		ReloadPlayerInventory();

		if (SaveManager.Instance != null) //skip when in testing scene
		{
			if (playerClass == PlayerClassesUi.Instance.knightClass && !hasRecievedKnightItems)
			{
				SpawnClassStartingItems(playerClass);
				hasRecievedKnightItems = true;
			}
			else if (playerClass == PlayerClassesUi.Instance.warriorClass && !hasRecievedWarriorItems)
			{
				SpawnClassStartingItems(playerClass);
				hasRecievedWarriorItems = true;
			}
			else if (playerClass == PlayerClassesUi.Instance.rogueClass && !hasRecievedRogueItems)
			{
				SpawnClassStartingItems(playerClass);
				hasRecievedRogueItems = true;
			}
			else if (playerClass == PlayerClassesUi.Instance.rangerClass && !hasRecievedRangerItems)
			{
				SpawnClassStartingItems(playerClass);
				hasRecievedRangerItems = true;
			}
			else if (playerClass == PlayerClassesUi.Instance.mageClass && !hasRecievedMageItems)
			{
				SpawnClassStartingItems(playerClass);
				hasRecievedMageItems = true;
			}

			if (!SaveManager.Instance.GameData.hasRecievedStartingItems)
			{
				GameManager.isNewGame = false;
				SaveManager.Instance.GameData.hasRecievedStartingItems = true;
				hasRecievedStartingItems = true;
				SpawnSharedStartingItems(playerClass);
			}
		}
	}
	private void SpawnClassStartingItems(SOClasses playerClass)
	{
		SpawnStartingItem(playerClass.startingWeapon[Utilities.GetRandomNumber(playerClass.startingWeapon.Count - 1)]);

		foreach (SOArmors SOarmor in playerClass.startingArmor)
			SpawnStartingItem(SOarmor);

		foreach (SOAccessories SOaccessory in playerClass.startingAccessories)
			SpawnStartingItem(SOaccessory);
	}
	private void SpawnSharedStartingItems(SOClasses playerClass)
	{
		PlayerInventoryUi.Instance.UpdateGoldAmount(startingGold);

		foreach (SOConsumables SOconsumable in playerClass.startingConsumables)
			SpawnStartingItem(SOconsumable);
	}
	private void DebugSpawnStartingItems(SOClasses playerClass)
	{
		PlayerInventoryUi.Instance.UpdateGoldAmount(startingGold);
		SpawnStartingItem(playerClass.startingWeapon[Utilities.GetRandomNumber(playerClass.startingWeapon.Count - 1)]);

		foreach (SOArmors SOarmor in playerClass.startingArmor)
			SpawnStartingItem(SOarmor);

		foreach (SOAccessories SOaccessory in playerClass.startingAccessories)
			SpawnStartingItem(SOaccessory);

		foreach (SOConsumables SOconsumable in playerClass.startingConsumables)
			SpawnStartingItem(SOconsumable);
	}
	private void SpawnStartingItem(SOItems SOitem)
	{
		GameObject go = Instantiate(droppedItemPrefab, gameObject.transform.position, Quaternion.identity);

		if (SOitem.itemType == SOItems.ItemType.isWeapon)
		{
			Weapons weapon = go.AddComponent<Weapons>();
			weapon.weaponBaseRef = (SOWeapons)SOitem;
			weapon.SetCurrentStackCount(1);
		}

		if (SOitem.itemType == SOItems.ItemType.isArmor)
		{
			Armors armor = go.AddComponent<Armors>();
			armor.armorBaseRef = (SOArmors)SOitem;
			armor.SetCurrentStackCount(1);
		}

		if (SOitem.itemType == SOItems.ItemType.isAccessory)
		{
			Accessories accessories = go.AddComponent<Accessories>();
			accessories.accessoryBaseRef = (SOAccessories)SOitem;
			accessories.SetCurrentStackCount(1);
		}

		if (SOitem.itemType == SOItems.ItemType.isConsumable)
		{
			Consumables consumables = go.AddComponent<Consumables>();
			consumables.consumableBaseRef = (SOConsumables)SOitem;
			consumables.SetCurrentStackCount(3);
		}

		Items item = go.GetComponent<Items>();
		item.Initilize(Items.Rarity.isRare, GetComponent<EntityStats>().entityLevel, 3);
		BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
		collider.isTrigger = true;
	}
	private void ReloadPlayerInventory()
	{
		if (SaveManager.Instance == null) return;
		hasRecievedStartingItems = SaveManager.Instance.GameData.hasRecievedStartingItems;
		hasRecievedKnightItems = SaveManager.Instance.GameData.hasRecievedKnightItems;
		hasRecievedWarriorItems = SaveManager.Instance.GameData.hasRecievedWarriorItems;
		hasRecievedRogueItems = SaveManager.Instance.GameData.hasRecievedRogueItems;
		hasRecievedRangerItems = SaveManager.Instance.GameData.hasRecievedRangerItems;
		hasRecievedMageItems = SaveManager.Instance.GameData.hasRecievedMageItems;
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
