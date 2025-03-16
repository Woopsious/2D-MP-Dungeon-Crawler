using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInventoryHandler : MonoBehaviour
{
	private PlayerController player;

	[Header("Player Starting Items")]
	public GameObject droppedItemPrefab;

	[Header("Player Starting Gold")]
	private readonly int startingGold = 500;

	public void Awake()
	{
		player = GetComponent<PlayerController>();
	}

	//spawn starting items based on starting class + if already receieved them
	public void TrySpawnStartingItems(SOClasses playerClass)
	{
		if (Application.isEditor)
		{
			//DebugSpawnStartingItems(playerClass);
			//return;
		}

		if (GameManager.Localplayer != player) return;

		if (playerClass == PlayerClassesUi.Instance.knightClass && !PlayerClassesUi.Instance.hasRecievedKnightItems)
			SpawnClassStartingItems(playerClass);
		else if (playerClass == PlayerClassesUi.Instance.warriorClass && !PlayerClassesUi.Instance.hasRecievedWarriorItems)
			SpawnClassStartingItems(playerClass);
		else if (playerClass == PlayerClassesUi.Instance.rogueClass && !PlayerClassesUi.Instance.hasRecievedRogueItems)
			SpawnClassStartingItems(playerClass);
		else if (playerClass == PlayerClassesUi.Instance.rangerClass && !PlayerClassesUi.Instance.hasRecievedRangerItems)
			SpawnClassStartingItems(playerClass);
		else if (playerClass == PlayerClassesUi.Instance.mageClass && !PlayerClassesUi.Instance.hasRecievedMageItems)
			SpawnClassStartingItems(playerClass);

		if (!PlayerClassesUi.Instance.hasRecievedStartingItems)
		{
			GameManager.isNewGame = false;
			SpawnSharedStartingItems(playerClass);
		}

		PlayerClassesUi.Instance.hasRecievedKnightItems = true;
		PlayerClassesUi.Instance.hasRecievedWarriorItems = true;
		PlayerClassesUi.Instance.hasRecievedRogueItems = true;
		PlayerClassesUi.Instance.hasRecievedRangerItems = true;
		PlayerClassesUi.Instance.hasRecievedMageItems = true;
		PlayerClassesUi.Instance.hasRecievedStartingItems = true;
	}

	//spawning starting items
	private void SpawnClassStartingItems(SOClasses playerClass)
	{
		if (playerClass.startingWeapon.Count != 0)
			SpawnStartingItem(playerClass.startingWeapon[Utilities.GetRandomNumber(playerClass.startingWeapon.Count - 1)]);
		if (playerClass.startingOffhandWeapon.Count != 0)
			SpawnStartingItem(playerClass.startingOffhandWeapon[Utilities.GetRandomNumber(playerClass.startingWeapon.Count - 1)]);

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
		PlayerInventoryUi.Instance.UpdateGoldAmount(1000000);
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
		if (SOitem == null)
		{
			Debug.LogError("No item to spawn in");
			return;
		}

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
		item.Initilize(SOItems.Rarity.isCommon, GetComponent<EntityStats>().entityLevel, 0);
		BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
		collider.isTrigger = true;
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
