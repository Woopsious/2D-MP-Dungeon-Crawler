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
	public void ReviveAllPlayersRpc()
	{
		PlayerDeathUi.Instance.CallReviveAllPlayersEvent();
	}

	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	public void RevivePlayerRpc(ulong playerObjId)
	{
		PlayerEventManager.RevivePlayer(NetworkManager.SpawnManager.SpawnedObjects[playerObjId].gameObject);
	}
}
