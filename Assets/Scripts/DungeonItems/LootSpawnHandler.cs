using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class LootSpawnHandler : MonoBehaviour
{
	public GameObject droppedItemPrefab;
	public SOLootPools lootPool;
	public int maxGold;
	public int minGold;
	private int lootSpawnerLevel;
	private float levelModifier;
	private float itemRarityChanceModifier;

	private float totalItemSpawnChance;
	private List<float> itemSpawnChanceTable = new List<float>();

	private void OnEnable()
	{
		DungeonHandler.OnEntityDeathEvent += OnEntityDeathEvent;
		PlayerEventManager.OnPlayerLevelUpEvent += UpdateLootSpawnerLevel;
		PlayerClassesUi.OnClassChanges += UpdateLootSpawnTable;
	}
	private void OnDisable()
	{
		DungeonHandler.OnEntityDeathEvent -= OnEntityDeathEvent;
		PlayerEventManager.OnPlayerLevelUpEvent -= UpdateLootSpawnerLevel;
		PlayerClassesUi.OnClassChanges -= UpdateLootSpawnTable;

		itemSpawnChanceTable.Clear();
		totalItemSpawnChance = 0;
	}

	//set data
	public void Initilize(int maxGold, int minGold, SOLootPools newLootPool, float itemRarityChanceModifier)
	{
		this.maxGold = maxGold;
		this.minGold = minGold;
		lootPool = newLootPool;
		this.itemRarityChanceModifier = itemRarityChanceModifier;

		UpdateLootSpawnTable(PlayerClassesUi.Instance.currentPlayerClass);
	}
	private void UpdateLootSpawnTable(SOClasses playerClass)
	{
		if (lootPool == null || playerClass == null) return; //lootPool == null when called via OnClassChanges event when scene switching

		itemSpawnChanceTable.Clear();
		totalItemSpawnChance = 0;

		foreach (SOItems item in lootPool.lootPoolList)
		{
			if (item.itemType == SOItems.ItemType.isWeapon)
				itemSpawnChanceTable.Add(AdjustSpawnChanceForWeaponsBasedOnClass((SOWeapons)item, playerClass));
			else if (item.itemType == SOItems.ItemType.isArmor)
				itemSpawnChanceTable.Add(AdjustSpawnChanceForArmorsBasedOnClass((SOArmors)item, playerClass));
			else
				itemSpawnChanceTable.Add(item.itemSpawnChance);
		}

		foreach (float num in itemSpawnChanceTable)
			totalItemSpawnChance += num;
	}
	private float AdjustSpawnChanceForWeaponsBasedOnClass(SOWeapons weapon, SOClasses playerClass)
	{
		if (playerClass.classType == SOClasses.ClassType.isKnight)
		{
			if (weapon.weaponType == SOWeapons.WeaponType.isAxe || weapon.weaponType == SOWeapons.WeaponType.isMace ||
				weapon.weaponType == SOWeapons.WeaponType.isShield || weapon.weaponType == SOWeapons.WeaponType.isSword)
				return weapon.itemSpawnChance * 1.5f;
			else
				return weapon.itemSpawnChance;
		}
		else if (playerClass.classType == SOClasses.ClassType.isWarrior)
		{
			if (weapon.weaponType == SOWeapons.WeaponType.isShield || weapon.weaponType == SOWeapons.WeaponType.isSword)
				return weapon.itemSpawnChance * 1.5f;
			else
				return weapon.itemSpawnChance;
		}
		else if (playerClass.classType == SOClasses.ClassType.isRanger)
		{
			if (weapon.weaponType == SOWeapons.WeaponType.isShield || weapon.weaponType == SOWeapons.WeaponType.isBow)
				return weapon.itemSpawnChance * 1.5f;
			else
				return weapon.itemSpawnChance;
		}
		else if (playerClass.classType == SOClasses.ClassType.isRogue)
		{
			if (weapon.weaponType == SOWeapons.WeaponType.isShield || weapon.weaponType == SOWeapons.WeaponType.isDagger)
				return weapon.itemSpawnChance * 1.5f;
			else
				return weapon.itemSpawnChance;
		}
		else if (playerClass.classType == SOClasses.ClassType.isMage)
		{
			if (weapon.weaponType == SOWeapons.WeaponType.isStaff)
				return weapon.itemSpawnChance * 1.5f;
			else
				return weapon.itemSpawnChance;
		}
		else return weapon.itemSpawnChance;
	}
	private float AdjustSpawnChanceForArmorsBasedOnClass(SOArmors armor, SOClasses playerClass)
	{
		if (armor.classRestriction == (SOArmors.ClassRestriction)playerClass.classRestriction)
		{
			if (playerClass.classType == SOClasses.ClassType.isMage && armor.baseBonusMana == 0)
				return armor.itemSpawnChance * 1.5f;
			else	//for mage class also increase chance for robes to spawn
				return armor.itemSpawnChance * 1.5f;
		}
		else
			return armor.itemSpawnChance;
	}
	private int GetIndexOfItemToDrop()
	{
		float rand = Random.Range(0, totalItemSpawnChance);
		float cumChance = 0;

		for (int i = 0; i < itemSpawnChanceTable.Count; i++)
		{
			cumChance += itemSpawnChanceTable[i];

			if (rand <= cumChance)
				return i;
		}
		return 0; //incase of fail return first item
	}

	//event listners
	private void OnEntityDeathEvent(GameObject obj)
	{
		if (obj != gameObject) return;
		SpawnLoot();
		AddGold();
	}
	private void UpdateLootSpawnerLevel(EntityStats playerStats)
	{
		lootSpawnerLevel = playerStats.entityLevel;
		levelModifier = playerStats.levelModifier;
	}

	//spawn loot
	public void AddGold()
	{
		float goldModifier = levelModifier;

		if (GameManager.Instance != null)   //add difficulty modifier to gold drops
			goldModifier += GameManager.Instance.currentDungeonData.dungeonStatModifiers.difficultyModifier;
		else
			Debug.LogWarning("Game Manager not found whilst updating spawner level, ignore if scene testing");

		int goldToAdd = Utilities.GetRandomNumberBetween((int)(minGold * goldModifier), (int)(maxGold * goldModifier));
		PlayerInventoryUi.Instance.UpdateGoldAmount(goldToAdd);
	}
	public void SpawnLoot()
	{
		int numOfItemsToSpawn = Utilities.GetRandomNumberBetween(lootPool.minDroppedItemsAmount, lootPool.maxDroppedItemsAmount);
		for (int i = 0; i < numOfItemsToSpawn; i++)
		{
			int index = GetIndexOfItemToDrop();
			GameObject go = Instantiate(droppedItemPrefab, transform.position, Quaternion.identity);

			//throwing index out of range?? leave check in for now
			if (lootPool.lootPoolList[index] == null)
			{
				Debug.LogError("item index: " + index);
				Debug.LogError("loot pool count: " + lootPool.lootPoolList.Count);
				return;
			}

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isWeapon)
				SetUpWeaponItem(go, index);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isArmor)
				SetUpArmorItem(go, index);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isAccessory)
				SetUpAccessory(go, index);

			if (lootPool.lootPoolList[index].itemType == SOItems.ItemType.isConsumable)
				SetUpConsumableItem(go, index);

			SetUpItem(go, index);

			go.GetComponent<Items>().Initilize(Utilities.SetRarity(itemRarityChanceModifier), Utilities.SetItemLevel(lootSpawnerLevel), 0);
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
