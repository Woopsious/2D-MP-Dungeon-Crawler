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
		ClientManager.Instance.clientNetworkedId = NetworkManager.Singleton.LocalClientId;
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
	public void RemoveClientFromRelay(string networkedId)
	{
		networkIdOfKickedPlayer = networkedId;
		ulong networkedIdulong = Convert.ToUInt64(networkedId);

		NetworkManager.Singleton.DisconnectClient(networkedIdulong);
	}
	public async void RemoveClientFromLobby(string playerId)
	{
		try
		{
			await LobbyService.Instance.RemovePlayerAsync(LobbyManager.Instance._Lobby.Id, playerId);
			Debug.Log($"player with Id: {playerId} kicked from lobby");
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

		/*
		if (SceneManager.GetActiveScene().buildIndex == 1)
		{
			if (GameManager.Instance.hasGameEnded.Value == false)
				GameManager.Instance.gameUIManager.ShowPlayerDisconnectedPanel();
			else if (GameManager.Instance.hasGameEnded.Value == true)
			{
				GameManager.Instance.gameUIManager.playAgainButtonObj.SetActive(false);
				GameManager.Instance.gameUIManager.playAgainUiText.text = "Other Player Left";
			}
			StopHost();
		}
		*/
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
