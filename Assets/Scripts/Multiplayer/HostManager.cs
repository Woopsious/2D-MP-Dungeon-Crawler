using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using WebSocketSharp;

public class HostManager : NetworkBehaviour
{
	public static HostManager Instance;

	public GameObject clientListObj;

	public int connectedPlayers;
	public string idOfKickedPlayer;
	public string networkIdOfKickedPlayer;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(Instance);
		}
		else
			Destroy(gameObject);
	}

	//START/STOP HOST
	public void StartHost()
	{
		GameManager.Instance.PauseGame(false);
		StartCoroutine(RelayConfigureTransportAsHostingPlayer());
		MultiplayerManager.Instance.SubToEvents();
		MultiplayerManager.Instance.isMultiplayer = true;
	}
	public void StopHost()
	{
		if (GameManager.Instance.currentlyLoadedScene.name == GameManager.Instance.hubScene)
			SaveManager.Instance.AutoSaveData();

		LobbyManager.Instance.DeleteLobby();
		LobbyManager.Instance.ResetLobbyReferences();
		MultiplayerManager.Instance.UnsubToEvents();
		MultiplayerManager.Instance.isMultiplayer = false;
		NetworkManager.Singleton.Shutdown();
	}

	//CREATE RELAY SERVER
	IEnumerator RelayConfigureTransportAsHostingPlayer()
	{
		var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(4);
		while (!serverRelayUtilityTask.IsCompleted)
		{
			yield return null;
		}
		if (serverRelayUtilityTask.IsFaulted)
		{
			Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
			yield break;
		}

		var relayServerData = serverRelayUtilityTask.Result;
		NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
		yield return null;

		NetworkManager.Singleton.StartHost();
		NetworkManager.Singleton.SceneManager.OnSceneEvent += MultiplayerManager.Instance.SceneManager_OnSceneEvent;
	}
	public static async Task<RelayServerData> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
	{
		Allocation allocation;
		try
		{
			allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
		}
		catch (Exception e)
		{
			Debug.LogError($"Relay create allocation request failed {e.Message}");
			throw;
		}

		try
		{
			LobbyManager.Instance.lobbyJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
		}
		catch
		{
			Debug.LogError("Relay create join code request failed");
			throw;
		}
		return new RelayServerData(allocation, "dtls");
	}
	private void SpawnClientListObj()
	{
		var instance = Instantiate(clientListObj);
		var instanceNetworkObject = instance.GetComponent<NetworkObject>();
		instanceNetworkObject.Spawn();
	}

	//DISCONNECT OPTIONS
	//close lobby
	public void CloseLobby(string disconnectReason)
	{
		for (int i = 0; i < LobbyManager.Instance._Lobby.Players.Count; i++)
		{
			Player player = LobbyManager.Instance._Lobby.Players[i];
			RemoveClientFromRelay(player.Data["PlayerNetworkID"].Value, disconnectReason);
		}

		StopHost();
		MultiplayerMenuUi.Instance.SetDisconnectReason(disconnectReason);
		MultiplayerMenuUi.Instance.ShowDisconnectUiPanel();
	}
	//kick client
	public void KickClientFromRelay(string networkedStringId, string disconnectReason)
	{
		RemoveClientFromRelay(networkedStringId, disconnectReason);
	}
	//remove clients from relay
	private void RemoveClientFromRelay(string networkedStringId, string disconnectReason)
	{
		ulong networkedId = Convert.ToUInt64(networkedStringId);
		if (networkedId == 0) return; //skip host disconnecting

		if (disconnectReason.IsNullOrEmpty())
			NetworkManager.Singleton.DisconnectClient(networkedId, "Network error"); //fall back reason
		else
			NetworkManager.Singleton.DisconnectClient(networkedId, disconnectReason);
	}

	//HANDLE CLIENT CONNECTS/DISCONNECTS EVENTS
	public void HandleClientConnectsAsHost(ulong id)
	{
		if (id == 0)
			SpawnClientListObj();

		GameManager.Instance.SpawnNetworkedPlayerObject(id);
	}
	public void HandleClientDisconnectsAsHost(ulong id)
	{
		RemoveDisconnectedClientsFromLobby(id);
	}

	//auto remove disconnected clients from lobby for what ever reason
	private void RemoveDisconnectedClientsFromLobby(ulong id)
	{
		if (id == 0) return; //host disconnected, possibly check to ensure lobby host was in is also shut down

		LobbyManager.Instance.RemoveDisconnectedClientFromLobby(id.ToString());
	}
}
