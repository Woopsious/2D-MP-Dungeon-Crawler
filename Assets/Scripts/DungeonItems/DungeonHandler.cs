using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static ChestHandler;
using static UnityEngine.Mesh;

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
	}
	private void Start()
	{
		MovePlayersToEnterencePortal();
		ActivateRandomChests();
	}
	private void OnEnable()
	{
		PlayerEventManager.OnRespawnAllPlayersEvent += RespawnPlayersAtClosestPortal;
		SaveManager.ReloadDungeonData += RestoreDungeonChestData;
	}
	private void OnDisable()
	{
		PlayerEventManager.OnRespawnAllPlayersEvent -= RespawnPlayersAtClosestPortal;
		SaveManager.ReloadDungeonData -= RestoreDungeonChestData;
	}

	//player respawns
	public void RespawnPlayersAtClosestPortal()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		//respawn all players at hosts closest portal for simplicity + keeping players together
		Vector2 positionToRespawnAt = GetClosestPortalToPlayer(GameManager.Localplayer.gameObject);

		foreach (PlayerController player in ObjectPoolingManager.Instance.playersPool)
			player.transform.position = positionToRespawnAt;
	}
	private void RespawnPlayerAtClosestPortal(GameObject playerObj)
	{
		if (!MultiplayerManager.IsClientHost()) return;

		playerObj.transform.position = GetClosestPortalToPlayer(playerObj);
	}

	private Vector2 GetClosestPortalToPlayer(GameObject playerObj)
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
		return positionToRespawnAt;
	}

	//DUNGEON SETUP
	//setting enterence portals + moving players to them
	public Vector2 GetDungeonEnterencePortal(GameObject player)
	{
		if (dungeonEnterencePortal == null)
			return player.transform.position;
		else return dungeonEnterencePortal.transform.position;
	}
	private void MovePlayersToEnterencePortal()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		GameObject portalSpawnPoint = dungeonPortalsList[Utilities.GetRandomNumber(dungeonPortalsList.Count - 1)];
		dungeonEnterencePortal = portalSpawnPoint;

		foreach (PlayerController player in ObjectPoolingManager.Instance.playersPool)
			player.transform.position = dungeonEnterencePortal.transform.position;
	}

	//SET UP LOOT CHESTS
	private void ActivateRandomChests()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		foreach (ChestHandler chest in dungeonLootChestsList)
		{
			if (chest.isPlayerStorageChest) continue;
			int chance = Utilities.GetRandomNumberBetween(0, 100);

			if (chance > chanceForChestToActivate)
				UpdateChestState(chest, ChestState.enabled, false);
			else
				UpdateChestState(chest, ChestState.disabled, false);
		}
	}

	//restore chest data
	private void RestoreDungeonChestData()
	{
		if (!MultiplayerManager.IsClientHost()) return;
		if (GameManager.Instance.currentDungeonData.dungeonChestData.Count <= 0 ||
			dungeonLootChestsList.Count <= 0) return; //return on first time enter + no loot chest (hub area)

		int i = 0;
		foreach (DungeonChestData chestData in GameManager.Instance.currentDungeonData.dungeonChestData)
		{
			UpdateChestState(dungeonLootChestsList[i], chestData.chestState, false);
			i++;
		}
	}

	//sync chest states between clients on Start
	public void TrySyncChestStates()
	{
		if (!MultiplayerManager.IsMultiplayer()) return;

		ChestState[] chestStates = new ChestState[dungeonLootChestsList.Count];
		int i = 0;

		foreach (ChestHandler chest in dungeonLootChestsList)
		{
			chestStates[i] = chest.GetChestState();
			i++;
		}

		ClientRpcManager.instance.SyncDungeonChestStatsRpc(chestStates);
	}
	public void SyncChestStates(ChestState[] chestStates)
	{
		int i = 0;
		foreach (ChestState chestState in chestStates)
		{
			UpdateChestState(dungeonLootChestsList[i], chestState, false);
			i++;
		}
	}

	//update chest states between clients during runtime
	public void TrySyncChestState(ChestHandler chest)
	{
		for (int i = 0; i <  dungeonLootChestsList.Count; i++)
		{
			if (chest == dungeonLootChestsList[i])
				ClientRpcManager.instance.SyncChestStateRpc(i, chest.GetChestState());
		}
	}
	public void SyncChestState(int chestIndex, ChestState newState)
	{
		UpdateChestState(dungeonLootChestsList[chestIndex], newState, true);
	}

	//update chest state
	private void UpdateChestState(ChestHandler chest, ChestState newState, bool OpenChestAsPlayer)
	{
		if (newState == ChestState.disabled)
			chest.DisableChest();
		else if (newState == ChestState.enabled)
			chest.EnableChest();
		else if (newState == ChestState.opened)
			chest.OpenChest(OpenChestAsPlayer);
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
