using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static TrapHandler;
using static ChestHandler;
using UnityEngine.InputSystem.LowLevel;

public class ClientRpcManager : NetworkBehaviour
{
	public static ClientRpcManager instance;

	private void Awake()
	{
		instance = this;
	}

	//sync debug toggles
	[Rpc(SendTo.Everyone)]
	public void SyncLocalPlayerInvincibleRpc(ulong objId, bool newState)
	{
		Damageable player = NetworkManager.SpawnManager.SpawnedObjects[objId].GetComponent<Damageable>();
		player.invincible = newState;
	}
	[Rpc(SendTo.Everyone)]
	public void SyncLocalPlayerNoDeathRpc(ulong objId, bool newState)
	{
		EntityStats player = NetworkManager.SpawnManager.SpawnedObjects[objId].GetComponent<EntityStats>();
		player.playerRef.debugNoDeath = newState;
	}

	//dungeon stat modifiers
	[Rpc(SendTo.Everyone)]
	public void SyncDungeonStatModifiersRpc(float difficultyModifier, float[] modifiersList)
	{
		GameManager.Instance.SyncDungeonStatModifiers(difficultyModifier, modifiersList);
	}

	//dungeon trap types
	[Rpc(SendTo.Everyone)]
	public void SyncDungeonTrapTypesRpc(int[] trapTypeIndexes)
	{
		for (int i = 0; i < trapTypeIndexes.Length; i++)
			DungeonHandler.Instance.dungeonTrapsList[i].SetTrapType(trapTypeIndexes[i]);
	}

	//dungeon trap states
	[Rpc(SendTo.Everyone)]
	public void SyncDungeonTrapStateRpc(int trapIndex, TrapStates newState, float waitTime)
	{
		TrapHandler trap = DungeonHandler.Instance.dungeonTrapsList[trapIndex];

		if (newState == TrapStates.disabled)
			StartCoroutine(trap.DisableTrapState(waitTime));
		else if (newState == TrapStates.enabled)
			trap.EnableTrapState();
		else if (newState == TrapStates.detected)
			trap.DetectTrapState();
		else if (newState == TrapStates.activated)
			StartCoroutine(trap.ActivateTrapState());
	}

	//dungeon chest states
	[Rpc(SendTo.Everyone)]
	public void SyncAllInitialChestStatesRpc(ChestState[] chestStates)
	{
		int i = 0;
		foreach (ChestState chestState in chestStates)
		{
			if (chestState == ChestState.disabled)
				DungeonHandler.Instance.dungeonLootChestsList[i].DisableChestState();
			else if (chestState == ChestState.enabled)
				DungeonHandler.Instance.dungeonLootChestsList[i].EnableChestState();
			else if (chestState == ChestState.opened)
				DungeonHandler.Instance.dungeonLootChestsList[i].OpenChestState(false);
			i++;
		}
	}

	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	public void SyncChestStateRpc(int chestIndex, ChestState newState, bool isPlayerInteraction)
	{
		ChestHandler chest = DungeonHandler.Instance.dungeonLootChestsList[chestIndex];

		if (newState == ChestState.disabled)
			chest.DisableChestState();
		else if (newState == ChestState.enabled)
			chest.EnableChestState();
		else if (newState == ChestState.opened)
			chest.OpenChestState(isPlayerInteraction);
	}

	//sync start boss fights
	[Rpc(SendTo.Everyone)]
	public void SyncStartBossFightRpc()
	{
		BossRoomHandler.Instance.StartBossFight();
	}

	//player revive ui timers
	[Rpc(SendTo.SpecifiedInParams, RequireOwnership = false)]
	public void SyncStartRevivePlayerTimerUiRpc(float reviveTimer, RpcParams rpcParams)
	{
		PlayerEventManager.SyncStartRevivePlayerUiTimerEvent(reviveTimer);
	}
	[Rpc(SendTo.SpecifiedInParams, RequireOwnership = false)]
	public void SyncCancelRevivePlayerTimerUiRpc(RpcParams rpcParams)
	{
		PlayerEventManager.SyncCancelRevivePlayerUiTimerEvent();
	}

	//player respawning
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	public void RespawnAllPlayersRpc()
	{
		PlayerEventManager.RespawnAllPlayers();
	}
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	public void RespawnPlayerRpc(ulong objIdOfReviverPlayer, ulong objIOfRevivedPlayer)
	{
		PlayerController reviverPlayer = NetworkManager.SpawnManager.SpawnedObjects[objIdOfReviverPlayer].GetComponent<PlayerController>();
		PlayerController revivedPlayer = NetworkManager.SpawnManager.SpawnedObjects[objIOfRevivedPlayer].GetComponent<PlayerController>();

		if (reviverPlayer != revivedPlayer)
			PlayerEventManager.RespawnPlayer(reviverPlayer, revivedPlayer);
		else
			PlayerEventManager.RespawnPlayer(revivedPlayer, revivedPlayer);
	}
}
