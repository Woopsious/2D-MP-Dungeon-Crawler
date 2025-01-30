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

	public List<ClientDataInfo> connectedClientsList;

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
		ClearPlayers();
		GameManager.Instance.PauseGame(false);
		StartCoroutine(RelayConfigureTransportAsHostingPlayer());
		MultiplayerManager.Instance.SubToEvents();
		MultiplayerManager.Instance.isMultiplayer = true;
	}
	public void StopHost()
	{
		ClearPlayers();
		LobbyManager.Instance.DeleteLobby();
		LobbyManager.Instance.ResetLobbyReferences();
		MultiplayerManager.Instance.UnsubToEvents();
		MultiplayerManager.Instance.ShutDownNetworkManagerIfActive();
		MultiplayerManager.Instance.isMultiplayer = false;
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
	
	private void SpawnClientListObj(ClientDataInfo data)
	{
		var instance = Instantiate(clientListObj);
		var instanceNetworkObject = instance.GetComponent<NetworkObject>();
		instanceNetworkObject.Spawn();
	}

	//DISCONNECT OPTIONS
	//close lobby
	public void CloseLobby(string disconnectReason)
	{
		foreach (ClientDataInfo clientData in Instance.connectedClientsList)
			RemoveClientFromRelay(clientData.clientNetworkedId, disconnectReason);

		MultiplayerMenuUi.Instance.ShowMpMenuUi();
		StopHost();
	}
	//remove clients from relay
	public void RemoveClientFromRelay(ulong networkedId, string disconnectReason)
	{
		networkIdOfKickedPlayer = networkedId.ToString();

		//save data

		if (disconnectReason.IsNullOrEmpty())
			NetworkManager.Singleton.DisconnectClient(networkedId, "Network error"); //fall back reason
		else
			NetworkManager.Singleton.DisconnectClient(networkedId, disconnectReason);
	}
	[ServerRpc(RequireOwnership = false)]
	public void LeaveRelayServerRPC(ulong clientId) //non Host players leave lobby this way
	{
		RemoveClientFromRelay(clientId, "player left lobby");
	}

	//HANDLE CLIENT CONNECTS/DISCONNECTS EVENTS
	public void HandleClientConnectsAsHost(ulong id)
	{
		AddClientToConnectedClients(id);
		GameManager.Instance.SpawnNetworkedPlayerObject(id);
	}
	public void HandleClientDisconnectsAsHost(ulong id)
	{
		RemoveClientFromConnectedClients(id);
	}

	//remove clients from lobby after disconnects
	public async void RemoveClientFromLobby(string clientId)
	{
		try
		{
			await LobbyService.Instance.RemovePlayerAsync(LobbyManager.Instance._Lobby.Id, clientId);
			Debug.LogWarning($"player with Id: {clientId} kicked from lobby");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}

	//Sync connectedClientsList for all clients

	private void AddClientToConnectedClients(ulong id)
	{
		if (id == 0) //grab host data locally as lobby is not yet made
		{
			if (Instance.connectedClientsList == null)
				Instance.connectedClientsList = new List<ClientDataInfo>();

			ClientDataInfo clientData = new(ClientManager.Instance.clientUsername, ClientManager.Instance.clientId,
				ClientManager.Instance.clientNetworkedId);

			Instance.connectedClientsList.Add(clientData);
			SpawnClientListObj(clientData);
			ConnectedClients.Instance.AddClientToList(clientData);
		}
		else //grab other clients data through lobby
		{
			Player player = LobbyManager.Instance._Lobby.Players[Instance.connectedClientsList.Count];
			ClientDataInfo clientData = new(player.Data["PlayerName"].Value, player.Data["PlayerID"].Value, id);

			Instance.connectedClientsList.Add(clientData);
			ConnectedClients.Instance.AddClientToList(clientData);
		}
	}
	private void RemoveClientFromConnectedClients(ulong id)
	{
		foreach (ClientDataInfo clientData in Instance.connectedClientsList)
		{
			if (clientData.clientNetworkedId == id)
			{
				Instance.connectedClientsList.Remove(clientData);
				ConnectedClients.Instance.RemoveClientFromList(clientData);
				RemoveClientFromLobby(clientData.clientId.ToString());
			}
		}
	}

	//reset connectedClientsList
	public void ClearPlayers()
	{
		try
		{
			Instance.connectedClientsList.Clear();
		}
		catch
		{
			Debug.Log("failed to clear connectedClientsList: Not an issue so far");
		}
	}
}
