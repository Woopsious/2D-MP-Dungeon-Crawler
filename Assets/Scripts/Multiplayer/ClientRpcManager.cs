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

	[Rpc(SendTo.Everyone)]
	public void SyncDungeonChestStatsRpc(ChestHandler.ChestState[] chestStates)
	{
		DungeonHandler.Instance.SyncChestStates(chestStates);
	}

	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	public void SyncChestStateRpc(int chestIndex, ChestHandler.ChestState newState)
	{
		DungeonHandler.Instance.SyncChestState(chestIndex, newState);
	}

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
}
