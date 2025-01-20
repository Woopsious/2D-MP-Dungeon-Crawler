using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonHandler : MonoBehaviour
{
	public static DungeonHandler Instance;

	public List<GameObject> dungeonPortalsList = new List<GameObject>();
	private GameObject dungeonEnterencePortal;

	public ChestHandler playerStorageChest;
	public List<ChestHandler> dungeonLootChestsList = new List<ChestHandler>();
	private readonly int chanceForChestToActivate = 50;

	private void Awake()
	{
		Instance = this;	
		ActivateRandomChests();
		MovePlayersToEnterencePortal();
	}
	private void OnEnable()
	{
		//SaveManager.RestoreData += RestoreDungeonChestData;
		SaveManager.ReloadDungeonData += RestoreDungeonChestData;
	}
	private void OnDisable()
	{
		//SaveManager.RestoreData -= RestoreDungeonChestData;
		SaveManager.ReloadDungeonData -= RestoreDungeonChestData;
	}

	//player respawns
	public void RespawnPlayerAtClosestPortal(GameObject playerObj)
	{
		List<float> portalDistances = new();
		Vector2 positionToRespawnAt = Vector2.zero;
		float distance = 10000;

		foreach (GameObject portal in dungeonPortalsList)
		{
			float newDistance = Vector2.Distance(playerObj.transform.position, portal.transform.position);
			portalDistances.Add(distance);

			if (newDistance < distance)
			{
				positionToRespawnAt = portal.transform.position;
				distance = newDistance;
			}
		}
		playerObj.transform.position = positionToRespawnAt;
	}

	//DUNGEON SETUP
	private void MovePlayersToEnterencePortal()
	{
		GameObject portalSpawnPoint = dungeonPortalsList[Utilities.GetRandomNumber(dungeonPortalsList.Count - 1)];
		dungeonEnterencePortal = portalSpawnPoint;

		GameManager.Localplayer.transform.position = dungeonEnterencePortal.transform.position;
	}
	public Vector2 GetDungeonEnterencePortal(GameObject player)
	{
		if (dungeonEnterencePortal == null)
			return player.transform.position;
		else return dungeonEnterencePortal.transform.position;
	}
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
