using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
	public void RespawnPlayerRpc(ulong playerObjId)
	{
		PlayerEventManager.RespawnPlayer(NetworkManager.SpawnManager.SpawnedObjects[playerObjId].gameObject);
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
