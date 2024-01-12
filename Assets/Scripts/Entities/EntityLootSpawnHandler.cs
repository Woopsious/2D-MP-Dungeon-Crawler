using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityLootSpawnHandler : MonoBehaviour
{
	public GameObject droppedItemPrefab;
	public SOLootPools lootPool;

	//invoked from event
	private void OnEnable()
	{
		GetComponent<EntityStats>().onDeathEvent += OnDeathEvent;
	}
	private void OnDisable()
	{
		GetComponent<EntityStats>().onDeathEvent -= OnDeathEvent;
	}
	public void OnDeathEvent(GameObject obj)
	{
		for (int i = 0; i < lootPool.minDroppedItemsAmount; i++) //spawn item from loot pool at death location
		{
			int index = Utilities.GetRandomNumber(lootPool.lootPoolList.Count);
			GameObject go = Instantiate(droppedItemPrefab, obj.transform.position, Quaternion.identity);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isWeapon)
				SetUpWeaponItem(go, index);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isArmor)
				SetUpArmorItem(go, index);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isConsumable)
				SetUpConsumableItem(go, index);

			SetUpItem(go, index);

			//generic data here, may change if i make unique droppables like keys as they might not have a need for item level etc.
			//im just not sure of a better way to do it atm
			go.AddComponent<Interactables>(); //add interactables script. set randomized stats
			go.GetComponent<Items>().SetItemStats(Utilities.SetRarity(), Utilities.SetItemLevel(GetComponent<EntityStats>().entityLevel));
		}
	}
	public void SetUpItem(GameObject go, int index)
	{
		Items item = go.GetComponent<Items>();
		item.gameObject.name = lootPool.lootPoolList[index].name;
		item.itemName = lootPool.lootPoolList[index].name;
		item.itemImage = lootPool.lootPoolList[index].itemImage;
		item.ItemPrice = lootPool.lootPoolList[index].ItemPrice;
	}
	public void SetUpWeaponItem(GameObject go, int index)
	{
		Weapons weapon = go.AddComponent<Weapons>();
		weapon.weaponBaseRef = (SOWeapons)lootPool.lootPoolList[index];
		weapon.currentStackCount = 1;
	}
	public void SetUpArmorItem(GameObject go, int index)
	{
		Armors armor = go.AddComponent<Armors>();
		armor.armorBaseRef = (SOArmors)lootPool.lootPoolList[index];
		armor.currentStackCount = 1;
	}
	public void SetUpConsumableItem(GameObject go, int index)
	{
		Consumables consumables = go.AddComponent<Consumables>();
		consumables.consumableBaseRef = (SOConsumables)lootPool.lootPoolList[index];
		consumables.currentStackCount = 3;
	}

	public bool WillDropExtraloot()
	{
		return false;
	}
}
