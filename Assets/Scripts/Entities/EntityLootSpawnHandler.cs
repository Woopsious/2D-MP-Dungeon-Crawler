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
		EventManager.OnDeathEvent += OnDeathEvent;
	}
	private void OnDisable()
	{
		EventManager.OnDeathEvent -= OnDeathEvent;
	}
	private void OnDeathEvent(GameObject obj)
	{
		if (gameObject != obj) return;

		for (int i = 0; i < lootPool.minDroppedItemsAmount; i++) //spawn item from loot pool at death location
		{
			int index = Utilities.GetRandomNumber(lootPool.lootPoolList.Count);
			GameObject go = Instantiate(droppedItemPrefab, obj.transform.position, Quaternion.identity);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isWeapon)
				SetUpWeaponItem(go, index);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isArmor)
				SetUpArmorItem(go, index);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isAccessory)
				SetUpAccessory(go, index);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isConsumable)
				SetUpConsumableItem(go, index);

			SetUpItem(go, index);

			//generic data here, may change if i make unique droppables like keys as they might not have a need for item level etc.
			//im just not sure of a better way to do it atm
			go.GetComponent<Items>().Initilize(Utilities.SetRarity(), Utilities.SetItemLevel(GetComponent<EntityStats>().entityLevel));
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

	private bool WillDropExtraloot()
	{
		return false;
	}
}
