using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using Unity.Services.Lobbies.Models;

public class HostManager : NetworkBehaviour
{
	public static HostManager Instance;

	public NetworkList<ClientDataInfo> connectedClientsList;

	public int connectedPlayers;
	public string idOfKickedPlayer;
	public string networkIdOfKickedPlayer;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Instance.connectedClientsList = new NetworkList<ClientDataInfo>();
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
		if (id == 0) //grab host data locally as lobby is not yet made
		{
			if (Instance.connectedClientsList == null)
				connectedClientsList = new NetworkList<ClientDataInfo>();

			ClientDataInfo data = new(ClientManager.Instance.clientUsername, ClientManager.Instance.clientId,
				ClientManager.Instance.clientNetworkedId);

			Instance.connectedClientsList.Add(data);
		}
		else //grab other clients data through lobby
		{
			Player player = LobbyManager.Instance._Lobby.Players[Instance.connectedClientsList.Count];
			ClientDataInfo data = new(player.Data["PlayerName"].Value, player.Data["PlayerID"].Value, id);

			Instance.connectedClientsList.Add(data);
		}
	}
	public void HandleClientDisconnectsAsHost(ulong id)
	{
		foreach (ClientDataInfo clientData in connectedClientsList)
		{
			if (clientData.clientNetworkedId == id)
			{
				connectedClientsList.Remove(clientData);
				RemoveClientFromLobby(clientData.clientId.ToString());
			}
		}
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
