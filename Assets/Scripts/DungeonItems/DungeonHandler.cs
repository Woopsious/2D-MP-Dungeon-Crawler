using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonHandler : MonoBehaviour
{
	public static DungeonHandler Instance;

	public List<GameObject> dungeonPortalsList = new List<GameObject>();

	//keep track of all chests when player leaves dungeon, save to GameData in DungeonData function weather they have/havnt been opened
	//when player revisits dungeon OnSceneChangeFinish restore state of chests from GameData
	public ChestHandler playerStorageChest;
	public List<ChestHandler> dungeonLootChestsList = new List<ChestHandler>();
	private readonly int chanceForChestToActivate = 50;

	public List<EntityStats> inActiveEntityPool = new List<EntityStats>();

	public List<Projectiles> inActiveProjectilesPool = new List<Projectiles>();
	public List<AbilityAOE> inActiveAoeAbilitesPool = new List<AbilityAOE>();

	public static event Action<GameObject> OnEntityDeathEvent;
	public static event Action OnEntitySpawnEvent;

	private void Awake()
	{
		Instance = this;
		ActivateRandomChests();
	}
	private void OnEnable()
	{
		GameManager.OnSceneChangeFinish += SetPlayerSpawn;
		SaveManager.RestoreData += RestoreDungeonChestData;
	}
	private void OnDisable()
	{
		GameManager.OnSceneChangeFinish -= SetPlayerSpawn;
		SaveManager.RestoreData -= RestoreDungeonChestData;
	}

	//OBJECT POOLING
	//entity obj pooling + death event
	public void AddNewEntitiesToPool(EntityStats entity)
	{
		entity.gameObject.SetActive(false);
		entity.transform.position = Vector3.zero;
		inActiveEntityPool.Add(entity);
	}
	public static void EntityDeathEvent(GameObject gameObject)
	{
		OnEntityDeathEvent?.Invoke(gameObject);

		Instance.OnEntityDeath(gameObject);
	}
	private void OnEntityDeath(GameObject obj)
	{
		obj.SetActive(false);
		EntityStats entityStats = obj.GetComponent<EntityStats>();
		inActiveEntityPool.Add(entityStats);
	}

	//projectile obj pooling
	public static Projectiles GetProjectile()
	{
		if (Instance.inActiveProjectilesPool.Count != 0)
		{
			Projectiles projectile = Instance.inActiveProjectilesPool[0];
			Instance.inActiveProjectilesPool.RemoveAt(0);
			return projectile;
		}
		else return null;
	}
	public static void ProjectileCleanUp(Projectiles projectile)
	{
		projectile.gameObject.SetActive(false);
		projectile.transform.position = Vector3.zero;
		Instance.inActiveProjectilesPool.Add(projectile);
	}

	//aoe obj pooling
	public static AbilityAOE GetAoeAbility()
	{
		if (Instance.inActiveAoeAbilitesPool.Count != 0)
		{
			AbilityAOE abilityAOE = Instance.inActiveAoeAbilitesPool[0];
			Instance.inActiveAoeAbilitesPool.RemoveAt(0);
			return abilityAOE;
		}
		else return null;
	}
	public static void AoeAbilitiesCleanUp(AbilityAOE abilityAOE)
	{
		abilityAOE.gameObject.SetActive(false);
		abilityAOE.transform.position = Vector3.zero;
		Instance.inActiveAoeAbilitesPool.Add(abilityAOE);
	}

	//DUNGEON SETUP
	private void ActivateRandomChests()
	{
		foreach (ChestHandler chest in dungeonLootChestsList)
		{
			if (chest.isPlayerStorageChest) continue;

			int chance = Utilities.GetRandomNumberBetween(0, 100);

			if (chance > chanceForChestToActivate)
				chest.ActivateChest();
			else
				chest.DeactivateChest();
		}
	}

	//restore dungeon data
	private void RestoreDungeonChestData()
	{
		if (playerStorageChest == null) return;
		RestorePlayerStorageChestData();

		if (GameManager.Instance.currentDungeonData.dungeonChestData.Count <= 0 ||
			dungeonLootChestsList.Count <= 0) return; //return on first time enter + no loot chest (hub area)

		int i = 0;
		foreach (DungeonChestData chestData in GameManager.Instance.currentDungeonData.dungeonChestData)
		{
			if (chestData.chestActive)
			{
				dungeonLootChestsList[i].ActivateChest();
				if (chestData.chestStateOpened)
					dungeonLootChestsList[i].ChangeChestStateToOpen(false);
			}
			else
				dungeonLootChestsList[i].DeactivateChest();
			i++;
		}
	}
	private void RestorePlayerStorageChestData()
	{
		foreach (InventoryItemData itemData in SaveManager.Instance.GameData.playerStorageChestItems)
		{
			GameObject go = Instantiate(PlayerInventoryUi.Instance.ItemUiPrefab, playerStorageChest.itemContainer.transform);
			InventoryItemUi newInventoryItem = go.GetComponent<InventoryItemUi>();

			PlayerInventoryUi.Instance.ReloadItemData(newInventoryItem, itemData);
			newInventoryItem.Initilize();
			playerStorageChest.itemList.Add(newInventoryItem);
		}
	}
	private void SetPlayerSpawn()
	{
		if (dungeonPortalsList.Count <= 0) return;

		//need a better solution but it works for now, for mp will have to use player refs stored in a lobbyHandler or something similar
		//then probably call an event to equip the gear the player is actually supposed to be wearing as well as when ever they change
		//damage, spells, skill etc should just work if i call them through the rpc network manager events

		/*
		PlayerController player = PlayerInventoryManager.Instance.GetComponent<PlayerController>();
		GameObject portalSpawnPoint = dungeonPortalsList[Utilities.GetRandomNumber(dungeonPortalsList.Count - 1)];
		player.transform.position = portalSpawnPoint.transform.position;
		*/

		PlayerController[] players = FindObjectsOfType<PlayerController>();
		GameObject portalSpawnPoint = dungeonPortalsList[Utilities.GetRandomNumber(dungeonPortalsList.Count - 1)];

		foreach (PlayerController player in players)
			player.transform.position = portalSpawnPoint.transform.position;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, 20);
	}
}

[System.Serializable]
public class DungeonStatModifier
{
	public float difficultyModifier;

	public float healthModifier;
	public float manaModifier;
	public float physicalResistanceModifier;
	public float poisonResistanceModifier;
	public float fireResistanceModifier;
	public float iceResistanceModifier;

	public float physicalDamageModifier;
	public float poisonDamageModifier;
	public float fireDamageModifier;
	public float iceDamageModifier;
	public float mainWeaponDamageModifier;
	public float dualWeaponDamageModifier;
	public float rangedWeaponDamageModifier;
}
