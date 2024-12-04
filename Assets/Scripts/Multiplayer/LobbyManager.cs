using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
	public static LobbyManager Instance;

	public Lobby _Lobby;
	public string lobbyJoinCode;

	public string lobbyName = "LobbyName";
	private int maxConnections = 2;

	private readonly float lobbyHeartbeatWaitTime = 25f;
	public float lobbyHeartbeatTimer;
	private readonly float lobbyPollWaitTimer = 1.5f;
	public float lobbyPollTimer;

	public float kickPlayerFromLobbyOnFailedToConnectTimer = 10f;

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
	public void Update()
	{
		HandleLobbyPollForUpdates();
		if (MultiplayerManager.Instance.IsPlayerHost())
		{
			LobbyHeartBeat();
			KickPlayerFromLobbyIfFailedToConnectToRelay();
		}
	}

	//create lobby
	public async void CreateLobby()
	{
		try
		{
			CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
			{
				IsPrivate = false,
				Player = GetPlayer(),
				IsLocked = false,
				Data = new Dictionary<string, DataObject>
				{
					{"joinCode", new DataObject(visibility: DataObject.VisibilityOptions.Public, lobbyJoinCode)}
				}
			};

			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
				Instance.lobbyName, Instance.maxConnections, createLobbyOptions);

			Instance._Lobby = lobby;
			Debug.LogWarning($"Created lobby with name: {lobby.Name} and Id: {lobby.Id}");
			Debug.LogWarning($"lobby code: {lobby.Data["joinCode"].Value}");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}

	//join lobby
	public async void JoinLobby(Lobby lobby)
	{
		try
		{
			JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
			{
				Player = GetPlayer()
			};
			await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);

			_Lobby = lobby;
			lobbyJoinCode = _Lobby.Data["joinCode"].Value;
		}
		catch (LobbyServiceException e)
		{
			ReturnToLobbyListWhenFailedToJoinLobby();
			Debug.LogWarning("Failed to join lobby: " + e.Message);
			return;
		}

		StartCoroutine(ClientManager.Instance.RelayConfigureTransportAsConnectingPlayer());
	}
	public void ReturnToLobbyListWhenFailedToJoinLobby()
	{
		//MultiplayerManager.Instance.GetLobbiesList();
		//GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Failed to join Lobby", 3f);
	}

	//delete lobby
	public async void DeleteLobby()
	{
		if (_Lobby != null)
		{
			await LobbyService.Instance.DeleteLobbyAsync(_Lobby.Id);
			_Lobby = null;
		}
	}

	//set up player data when joining/creating lobby
	public Player GetPlayer()
	{
		return new Player
		{
			Data = new Dictionary<string, PlayerDataObject>
				{
					{ "PlayerName", new PlayerDataObject(
						PlayerDataObject.VisibilityOptions.Member, ClientManager.Instance.clientUsername.ToString())},
					{ "PlayerID", new PlayerDataObject(
						PlayerDataObject.VisibilityOptions.Member, ClientManager.Instance.clientId.ToString())},
					{ "PlayerNetworkID", new PlayerDataObject(
						PlayerDataObject.VisibilityOptions.Member, ClientManager.Instance.clientNetworkedId.ToString())},
				}
		};
	}

	//lobby hearbeat and update poll
	private async void LobbyHeartBeat()
	{
		if (_Lobby != null && _Lobby.HostId == ClientManager.Instance.clientId)
		{
			lobbyHeartbeatTimer -= Time.deltaTime;
			if (lobbyHeartbeatTimer < 0)
			{
				lobbyHeartbeatTimer = lobbyHeartbeatWaitTime;
				await LobbyService.Instance.SendHeartbeatPingAsync(_Lobby.Id);
			}
		}
	}
	private async void HandleLobbyPollForUpdates()
	{
		if (_Lobby != null)
		{
			lobbyPollTimer -= Time.deltaTime;
			if (lobbyPollTimer < 0)
			{
				lobbyPollTimer = lobbyPollWaitTimer;
				try
				{
					Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_Lobby.Id);
					_Lobby = lobby;

					if (SceneManager.GetActiveScene().buildIndex == 0)
						MultiplayerMenuUi.Instance.SyncPlayerListforLobbyUi(_Lobby);

					/*
					if (MultiplayerManager.Instance.CheckIfHost())
						Debug.LogWarning($"connected Networked clients: {NetworkManager.Singleton.ConnectedClientsList.Count}");

					Debug.LogWarning($"connected clients count: {HostManager.Instance.connectedClientsList.Count}");
					Debug.LogWarning($"clients in lobby: {_Lobby.Players.Count}");
					Debug.LogWarning($"Local Networked ID: {ClientManager.Instance.clientNetworkedId}");
					Debug.LogWarning($"Lobby Join Code: {_Lobby.Data["joinCode"].Value}");
					*/
				}
				catch
				{
					Debug.Log($"Lobby with id: {_Lobby.Id} no longer exists");
					_Lobby = null;
				}
			}
		}
	}

	//if player fails to join relay after 10s unity timesout, this function will then auto kick player from lobby after 11s
	public void KickPlayerFromLobbyIfFailedToConnectToRelay()
	{
		if (_Lobby != null)
		{
			kickPlayerFromLobbyOnFailedToConnectTimer -= Time.deltaTime;
			if (kickPlayerFromLobbyOnFailedToConnectTimer < 0)
			{
				if (_Lobby.Players.Count != HostManager.Instance.connectedClientsList.Count)
					HostManager.Instance.RemoveClientFromLobby(_Lobby.Players[1].Id);

				kickPlayerFromLobbyOnFailedToConnectTimer = 11f;
			}
		}
	}
}
