using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LootSpawnHandler : MonoBehaviour
{
	public GameObject droppedItemPrefab;
	public SOLootPools lootPool;
	public int lootSpawnerLevel;

	//invoked from event
	private void OnEnable()
	{
		DungeonHandler.OnEntityDeathEvent += OnEntityDeathEvent;
		PlayerEventManager.OnPlayerLevelUpEvent += UpdateLootSpawnerLevel;
	}
	private void OnDisable()
	{
		DungeonHandler.OnEntityDeathEvent -= OnEntityDeathEvent;
		PlayerEventManager.OnPlayerLevelUpEvent -= UpdateLootSpawnerLevel;
	}

	private void OnEntityDeathEvent(GameObject obj)
	{
		if (obj != gameObject) return;

		SpawnLoot();
	}
	private void UpdateLootSpawnerLevel(EntityStats playerStats)
	{
		lootSpawnerLevel = playerStats.entityLevel;
	}

	public void SpawnLoot()
	{
		int numOfItemsToSpawn = Utilities.GetRandomNumberBetween(lootPool.minDroppedItemsAmount, lootPool.maxDroppedItemsAmount);
		for (int i = 0; i < numOfItemsToSpawn; i++)
		{
			int index = Utilities.GetRandomNumber(lootPool.lootPoolList.Count - 1);
			GameObject go = Instantiate(droppedItemPrefab, transform.position, Quaternion.identity);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isWeapon)
				SetUpWeaponItem(go, index);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isArmor)
				SetUpArmorItem(go, index);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isAccessory)
				SetUpAccessory(go, index);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isConsumable)
				SetUpConsumableItem(go, index);

			SetUpItem(go, index);

			go.GetComponent<Items>().Initilize(Utilities.SetRarity(), Utilities.SetItemLevel(lootSpawnerLevel));
			BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
			collider.isTrigger = true;
		}
	}
	private void SetUpItem(GameObject go, int index)
	{
		Items item = go.GetComponent<Items>();
		item.gameObject.name = lootPool.lootPoolList[index].name;
		item.itemName = lootPool.lootPoolList[index].name;
		item.itemSprite = lootPool.lootPoolList[index].itemImage;
		item.itemPrice = lootPool.lootPoolList[index].itemPrice;
	}
	private void SetUpWeaponItem(GameObject go, int index)
	{
		Weapons weapon = go.AddComponent<Weapons>();
		weapon.weaponBaseRef = (SOWeapons)lootPool.lootPoolList[index];
		weapon.SetCurrentStackCount(1);
	}
	private void SetUpArmorItem(GameObject go, int index)
	{
		Armors armor = go.AddComponent<Armors>();
		armor.armorBaseRef = (SOArmors)lootPool.lootPoolList[index];
		armor.SetCurrentStackCount(1);
	}
	private void SetUpAccessory(GameObject go, int index)
	{
		Accessories accessory = go.AddComponent<Accessories>();
		accessory.accessoryBaseRef = (SOAccessories)lootPool.lootPoolList[index];
		accessory.SetRandomDamageTypeOnDrop();
		accessory.SetCurrentStackCount(1);
	}
	private void SetUpConsumableItem(GameObject go, int index)
	{
		Consumables consumables = go.AddComponent<Consumables>();
		consumables.consumableBaseRef = (SOConsumables)lootPool.lootPoolList[index];
		consumables.SetCurrentStackCount(3);
	}
}
