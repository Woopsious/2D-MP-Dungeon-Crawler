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
			DontDestroyOnLoad(Instance);
		}
		else
			Destroy(gameObject);
	}

	//start/stop host
	public void StartHost()
	{
		GameManager.Instance.PauseGame(false);
		StartCoroutine(RelayConfigureTransportAsHostingPlayer());
		ClearPlayers();
		MultiplayerManager.Instance.SubToEvents();
		MultiplayerManager.Instance.isMultiplayer = true;

		Instance.connectedClientsList = new NetworkList<ClientDataInfo>();
	}
	public void StopHost()
	{
		ClearPlayers();
		LobbyManager.Instance.DeleteLobby();
		MultiplayerManager.Instance.UnsubToEvents();
		MultiplayerManager.Instance.ShutDownNetworkManagerIfActive();
		MultiplayerManager.Instance.isMultiplayer = false;
	}

	//create relay server
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
	public void LeaveLobbyServerRPC(ulong clientId, string disconnectReason)
	{
		RemoveClientFromRelay(clientId, disconnectReason);
	}

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

	//handle client disconnects
	public void HandlePlayerDisconnectsAsHost(ulong id)
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
