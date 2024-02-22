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
		//if (!hasRecievedStartingItems)
			//SpawnStartingItems();
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
				weapon.SetCurrentStackCount(1);
			}

			if (startingItems[i].itemType == SOItems.ItemType.isArmor)
			{
				Armors armor = go.AddComponent<Armors>();
				armor.armorBaseRef = (SOArmors)startingItems[i];
				armor.SetCurrentStackCount(1);
			}

			if (startingItems[i].itemType == SOItems.ItemType.isAccessory)
			{
				Accessories accessories = go.AddComponent<Accessories>();
				accessories.accessoryBaseRef = (SOAccessories)startingItems[i];
				accessories.SetCurrentStackCount(1);
			}

			if (startingItems[i].itemType == SOItems.ItemType.isConsumable)
			{
				Consumables consumables = go.AddComponent<Consumables>();
				consumables.consumableBaseRef = (SOConsumables)startingItems[i];
				consumables.SetCurrentStackCount(3);
			}

			Items item = go.GetComponent<Items>();
			item.gameObject.name = startingItems[i].name;
			item.itemName = startingItems[i].name;
			item.itemImage = startingItems[i].itemImage;
			item.itemPrice = startingItems[i].itemPrice;

			//generic data here, may change if i make unique droppables like keys as they might not have a need for item level etc.
			//im just not sure of a better way to do it atm
			go.AddComponent<Interactables>(); //add interactables script. set randomized stats
			go.GetComponent<Items>().Initilize(Items.Rarity.isLegendary, 1);
			BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
			collider.isTrigger = true;
		}
	}
	private void ReloadPlayerInventory()
	{
		hasRecievedStartingItems = SaveManager.Instance.GameData.hasRecievedStartingItems;

		if (!hasRecievedStartingItems)
			SpawnStartingItems();
	}
	//on item pickup
	public void AddNewItemToPlayerInventory(Items item)
	{
		PlayerInventoryUi.Instance.AddItemToInventory(item);
	}
	//bool check before item pickup
	public bool CheckIfInventoryFull()
	{
		int numOfFilledSlots = 0;

		foreach (GameObject obj in PlayerInventoryUi.Instance.InventorySlots)
		{
			InventorySlotUi inventorySlot = obj.GetComponent<InventorySlotUi>();

			if (!inventorySlot.IsSlotEmpty())
				numOfFilledSlots++;
		}

		if (numOfFilledSlots == PlayerInventoryUi.Instance.InventorySlots.Count)
			return true;
		else return false;
	}
}
