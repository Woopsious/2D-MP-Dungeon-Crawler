using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ChestHandler;

public class DungeonHandler : MonoBehaviour
{
	public static DungeonHandler Instance;

	public List<GameObject> dungeonPortalsList = new List<GameObject>();
	private GameObject dungeonEnterencePortal;

	public List<TrapHandler> dungeonTrapsList = new List<TrapHandler>();
	public List<ChestHandler> dungeonLootChestsList = new List<ChestHandler>();
	private readonly int chanceForChestToActivate = 50;
	public ChestHandler playerStorageChest;

	private void Awake()
	{
		Instance = this;
	}
	private void Start()
	{
		MovePlayersToEnterencePortal();
		SetUpChests();
		SetUpTraps();
	}
	private void OnEnable()
	{
		PlayerEventManager.OnRespawnAllPlayersEvent += RespawnPlayersAtClosestPortal;
		PlayerEventManager.OnRespawnPlayerEvent += RespawnPlayerAtClosestPortal;
		SaveManager.ReloadDungeonData += RestoreDungeonChestData;
	}
	private void OnDisable()
	{
		PlayerEventManager.OnRespawnAllPlayersEvent -= RespawnPlayersAtClosestPortal;
		PlayerEventManager.OnRespawnPlayerEvent -= RespawnPlayerAtClosestPortal;
		SaveManager.ReloadDungeonData -= RestoreDungeonChestData;
	}

	//player respawns
	public void RespawnPlayersAtClosestPortal()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		//respawn all players at hosts closest portal for simplicity + keeping players together
		Vector2 positionToRespawnAt = GetClosestPortalToPlayer(GameManager.Localplayer);

		foreach (PlayerController player in ObjectPoolingManager.Instance.playersPool)
			player.transform.position = positionToRespawnAt;
	}
	private void RespawnPlayerAtClosestPortal(PlayerController optionalPlayer, PlayerController player)
	{
		if (!MultiplayerManager.IsClientHost()) return;

		player.transform.position = GetClosestPortalToPlayer(player);
	}
	private Vector2 GetClosestPortalToPlayer(PlayerController player)
	{
		List<float> portalDistances = new();
		Vector2 positionToRespawnAt = Vector2.zero;
		float distance = 10000;

		foreach (GameObject portal in dungeonPortalsList)
		{
			float newDistance = Vector2.Distance(player.transform.position, portal.transform.position);
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

	//SET UP TRAPS
	private void SetUpTraps()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		foreach (TrapHandler trap in dungeonTrapsList)
			trap.SetUpTrap();
	}
	//sync trap types for mp
	public void SyncTrapsTypes()
	{
		int[] trapTypeIndexes = new int[dungeonTrapsList.Count];
		int i = 0;

		foreach (TrapHandler trap in dungeonTrapsList)
		{
			trapTypeIndexes[i] = trap.GetTrapTypeIndex();
			i++;
		}

		ClientRpcManager.instance.SyncDungeonTrapTypesRpc(trapTypeIndexes);
	}

	//SET UP LOOT CHESTS
	private void SetUpChests()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		foreach (ChestHandler chest in dungeonLootChestsList)
		{
			if (chest.isPlayerStorageChest) continue;
			int chance = Utilities.GetRandomNumberBetween(0, 100);

			if (chance > chanceForChestToActivate)
				chest.EnableChestState();
			else
				chest.DisableChestState();
		}
	}

	//sync loot chest states for mp
	public void SyncChestStates()
	{
		ChestState[] chestStates = new ChestState[dungeonLootChestsList.Count];
		int i = 0;

		foreach (ChestHandler chest in dungeonLootChestsList)
		{
			chestStates[i] = chest.GetChestState();
			i++;
		}

		ClientRpcManager.instance.SyncAllInitialChestStatesRpc(chestStates);
	}

	//restore chest data
	private void RestoreDungeonChestData()
	{
		if (!MultiplayerManager.IsClientHost()) return;
		if (GameManager.Instance.currentDungeonData.dungeonChestData.Count <= 0 ||
			dungeonLootChestsList.Count <= 0) return; //return on first time enter + no loot chest (hub area)

		ChestState[] chestStates = new ChestState[dungeonLootChestsList.Count];

		int i = 0;
		foreach (DungeonChestData chestData in GameManager.Instance.currentDungeonData.dungeonChestData)
		{
			chestStates[i] = chestData.chestState;
			i++;
		}

		ClientRpcManager.instance.SyncAllInitialChestStatesRpc(chestStates);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, 20);
	}
}

[Serializable]
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

	public DungeonStatModifier(float difficultyModifier, float[] statsModifiers)
	{
		this.difficultyModifier = difficultyModifier;

		for (int i = 0; i < statsModifiers.Length; i++)
		{
			if (i == 0)
				healthModifier = statsModifiers[i];
			else if (i == 1)
				manaModifier = statsModifiers[i];

			else if (i == 2)
				physicalResistanceModifier = statsModifiers[i];
			else if (i == 3)
				poisonResistanceModifier = statsModifiers[i];
			else if (i == 4)
				fireResistanceModifier = statsModifiers[i];
			else if (i == 5)
				iceResistanceModifier = statsModifiers[i];

			else if (i == 6)
				physicalDamageModifier = statsModifiers[i];
			else if (i == 7)
				poisonDamageModifier = statsModifiers[i];
			else if (i == 8)
				fireDamageModifier = statsModifiers[i];
			else if (i == 9)
				iceDamageModifier = statsModifiers[i];

			else if (i == 10)
				mainWeaponDamageModifier = statsModifiers[i];
			else if (i == 11)
				dualWeaponDamageModifier = statsModifiers[i];
			else if (i == 12)
				rangedWeaponDamageModifier = statsModifiers[i];
			else
				Debug.LogError("modifer type out of range");
		}

	}
}
