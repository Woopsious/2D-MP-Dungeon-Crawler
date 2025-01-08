using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
	public static LobbyManager Instance;

	public Lobby _Lobby;
	public ILobbyEvents _LobbyEvents;

	public string lobbyName;
	public bool lobbyPrivate;
	public bool lobbyHasPassword;
	public string lobbyPassword;

	public string lobbyJoinCode;

	private int maxConnections = 4;
	private readonly float lobbyHeartbeatWaitTime = 25f;
	public float lobbyHeartbeatTimer;
	private readonly float lobbyPollWaitTimer = 1.5f;
	public float lobbyPollTimer;

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
		if (_Lobby == null) return;

		HandleLobbyPollForUpdates();
		if (MultiplayerManager.Instance.IsPlayerHost())
		{
			LobbyHeartBeat();
			//no longer valid with multiple joined players
			//KickPlayerFromLobbyIfFailedToConnectToRelay();
		}
	}

	//LOBBY CREATION/UPDATES
	//creating lobbies
	public async void CreateLobby(string lobbyName, bool lobbyPrivate)
	{
		this.lobbyName = lobbyName;
		this.lobbyPrivate = lobbyPrivate;
		lobbyHasPassword = false;
		lobbyPassword = "";

		while (string.IsNullOrWhiteSpace(Instance.lobbyJoinCode)) await Task.Delay(1);

		try
		{
			CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
			{
				IsPrivate = lobbyPrivate,
				Player = GetPlayerData(),
				IsLocked = false,
				Data = new Dictionary<string, DataObject>
				{
					{"joinCode", new DataObject(visibility: DataObject.VisibilityOptions.Public, lobbyJoinCode)}
				}
			};

			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
				lobbyName, Instance.maxConnections, createLobbyOptions);

			Instance._Lobby = lobby;
			Debug.LogWarning($"Created lobby with name: {lobby.Name} and Id: {lobby.Id}");
			Debug.LogWarning($"lobby code: {lobby.Data["joinCode"].Value}");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}
	public async void CreateLobbyWithPassword(string lobbyName, bool lobbyPrivate, string lobbyPassword)
	{
		this.lobbyName = lobbyName;
		this.lobbyPrivate = lobbyPrivate;
		lobbyHasPassword = true;
		this.lobbyPassword = lobbyPassword;

		while (string.IsNullOrWhiteSpace(Instance.lobbyJoinCode)) await Task.Delay(1);

		try
		{
			CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
			{
				IsPrivate = lobbyPrivate,
				Player = GetPlayerData(),
				IsLocked = false,
				Password = lobbyPassword,
				Data = new Dictionary<string, DataObject>{}
			};

			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
				lobbyName, Instance.maxConnections, createLobbyOptions);

			Instance._Lobby = lobby;
			Debug.LogWarning($"Created lobby with name: {lobby.Name} and Id: {lobby.Id}");
			Debug.LogWarning($"lobby code: {lobby.Data["joinCode"].Value}");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}

	//updating lobby settings
	public void UpdateLobbySettings(string lobbyName, bool lobbyPrivate, bool hasPassword, string lobbyPassword)
	{
		if (!hasPassword)
		{
			//update lobby settings including password
		}
		else
		{
			//update lobby settings excluding password
		}
	}

	//JOINING LOBBIES
	public async void JoinLobby(Lobby lobby)
	{
		try
		{
			JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
			{
				Player = GetPlayerData()
			};

			await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);
			Instance._Lobby = lobby;
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

	//DELETING LOBBIES
	public async void DeleteLobby() //host delete lobby
	{
		if (Instance._Lobby != null)
			await LobbyService.Instance.DeleteLobbyAsync(Instance._Lobby.Id);
	}
	public void ResetLobbyReferences() //host/client resets refs
	{
		Instance._Lobby = null;
		Instance.lobbyJoinCode = null;
	}

	//set up player data when joining/creating lobby
	public Player GetPlayerData()
	{
		return new Player
		{
			Data = new Dictionary<string, PlayerDataObject>
			{
				{ "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, 
					ClientManager.Instance.clientUsername.ToString())},
				{ "PlayerID", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, 
					ClientManager.Instance.clientId.ToString())},
				{ "PlayerLevel", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,
					SceneHandler.playerInstance.playerStats.entityLevel.ToString())},
				{ "PlayerClass", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,
					SceneHandler.playerInstance.playerClassHandler.currentEntityClass.className.ToString())},
			}
		};
	}

	//lobby hearbeat
	private async void LobbyHeartBeat()
	{
		if (Instance._Lobby.HostId == ClientManager.Instance.clientId)
		{
			lobbyHeartbeatTimer -= Time.deltaTime;
			if (lobbyHeartbeatTimer < 0)
			{
				lobbyHeartbeatTimer = lobbyHeartbeatWaitTime;
				await LobbyService.Instance.SendHeartbeatPingAsync(Instance._Lobby.Id);
			}
		}
	}

	//poll lobby for updates
	private async void HandleLobbyPollForUpdates()
	{
		lobbyPollTimer -= Time.deltaTime;
		if (lobbyPollTimer < 0)
		{
			lobbyPollTimer = lobbyPollWaitTimer;
			try
			{
				Instance._Lobby = await LobbyService.Instance.GetLobbyAsync(Instance._Lobby.Id);
				LobbyUi.Instance.SyncPlayerListforLobbyUi(Instance._Lobby);

				//debug logs for logging
				/*
				if (MultiplayerManager.Instance.IsPlayerHost())
					Debug.LogWarning($"connected Networked clients: {NetworkManager.Singleton.ConnectedClientsList.Count}");

				Debug.LogWarning($"connected clients count: {HostManager.Instance.connectedClientsList.Count}");
				Debug.LogWarning($"clients in lobby: {_Lobby.Players.Count}");
				Debug.LogWarning($"Local Networked ID: {ClientManager.Instance.clientNetworkedId}");
				Debug.LogWarning($"Lobby Join Code: {_Lobby.Data["joinCode"].Value}");
				*/
			}
			catch (LobbyServiceException e)
			{
				Debug.LogError($"{e.Message}");
				Instance._Lobby = null;
				_LobbyEvents = null;
			}
		}
	}

	//if player fails to join relay after 10s unity timesout, this function will then auto kick player from lobby after 11s
	//no longer valid with multiple joined players
	/*
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
	*/
}
