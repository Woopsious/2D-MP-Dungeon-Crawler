using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class ClientRpcManager : NetworkBehaviour
{
	public static ClientRpcManager instance;

	private void Awake()
	{
		instance = this;
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
	public void SyncDungeonTrapStateRpc(int trapIndex, TrapHandler.TrapStates newState, float waitTime)
	{
		if (newState == TrapHandler.TrapStates.disabled)
			StartCoroutine(DungeonHandler.Instance.dungeonTrapsList[trapIndex].DisableTrapState(waitTime));
		else if (newState == TrapHandler.TrapStates.enabled)
			DungeonHandler.Instance.dungeonTrapsList[trapIndex].EnableTrapState();
		else if (newState == TrapHandler.TrapStates.detected)
			DungeonHandler.Instance.dungeonTrapsList[trapIndex].DetectTrapState();
		else if (newState == TrapHandler.TrapStates.activated)
			StartCoroutine(DungeonHandler.Instance.dungeonTrapsList[trapIndex].ActivateTrapState());
	}

	//dungeon chest states
	[Rpc(SendTo.Everyone)]
	public void SyncDungeonChestStatesRpc(ChestHandler.ChestState[] chestStates)
	{
		DungeonHandler.Instance.SyncChestStates(chestStates);
	}

	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	public void SyncChestStateRpc(int chestIndex, ChestHandler.ChestState newState)
	{
		DungeonHandler.Instance.SyncChestState(chestIndex, newState);
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
