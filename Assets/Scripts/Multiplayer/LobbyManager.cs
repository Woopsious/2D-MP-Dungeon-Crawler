using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class LobbyManager : NetworkBehaviour
{
	public static LobbyManager Instance;

	public Lobby _Lobby;
	public string _LobbyId;
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
	private void Update()
	{
		if (_Lobby == null || _LobbyId.IsNullOrEmpty()) return;

		HandleLobbyPollForUpdates();
		if (MultiplayerManager.IsClientHost())
		{
			LobbyHeartBeat();
			//no longer valid with multiple joined players
			//KickPlayerFromLobbyIfFailedToConnectToRelay();
		}
	}

	private void OnEnable()
	{
		PlayerEventManager.OnPlayerLevelChangeEvent += UpdateClientPlayerLevel;
	}
	private void OnDisable()
	{
		PlayerEventManager.OnPlayerLevelChangeEvent -= UpdateClientPlayerLevel;
	}

	//LOBBY CREATION
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
				Player = SetClientPlayerData(),
				IsLocked = false,
				Data = new Dictionary<string, DataObject>
				{
					{"joinCode", new DataObject(visibility: DataObject.VisibilityOptions.Public, lobbyJoinCode)}
				}
			};

			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
				lobbyName, Instance.maxConnections, createLobbyOptions);

			Instance._Lobby = lobby;
			Instance._LobbyId = lobby.Id;
			Debug.LogWarning($"Created lobby with name: {lobby.Name} and Id: {lobby.Id}");
			Debug.LogWarning($"lobby code: {lobby.Data["joinCode"].Value}");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}

		SubToLobbyEvents();
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
				Player = SetClientPlayerData(),
				IsLocked = false,
				Password = lobbyPassword,
				Data = new Dictionary<string, DataObject>{}
			};

			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
				lobbyName, Instance.maxConnections, createLobbyOptions);

			Instance._Lobby = lobby;
			Instance._LobbyId = lobby.Id;
			Debug.LogWarning($"Created lobby with name: {lobby.Name} and Id: {lobby.Id}");
			Debug.LogWarning($"lobby code: {lobby.Data["joinCode"].Value}");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}

		SubToLobbyEvents();
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

	//DELETING LOBBIES
	public async void DeleteLobby() //host delete lobby
	{
		if (Instance._Lobby != null)
			await LobbyService.Instance.DeleteLobbyAsync(Instance._LobbyId);
	}
	public void ResetLobbyReferences() //host/client resets refs
	{
		UnSubToLobbyEvents();
		Instance._Lobby = null;
		Instance._LobbyId = null;
		Instance.lobbyJoinCode = null;
	}

	//CLIENTS JOINING LOBBY
	public async void JoinLobby(Lobby lobby)
	{
		try
		{
			JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
			{
				Player = SetClientPlayerData()
			};

			await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);
			Instance._Lobby = lobby;
			Instance._LobbyId = lobby.Id;
			lobbyName = lobby.Name;
			lobbyJoinCode = _Lobby.Data["joinCode"].Value;
		}
		catch (LobbyServiceException e)
		{
			ReturnToLobbyListWhenFailedToJoinLobby();
			Debug.LogWarning("Failed to join lobby: " + e.Message);
			return;
		}

		SubToLobbyEvents();
		StartCoroutine(ClientManager.Instance.RelayConfigureTransportAsConnectingPlayer());
	}
	public void ReturnToLobbyListWhenFailedToJoinLobby()
	{
		//MultiplayerManager.Instance.GetLobbiesList();
		//GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Failed to join Lobby", 3f);
	}

	//CLIENTS LEAVING LOBBY (getting removed by host on disconnect)
	public async void RemoveDisconnectedClientFromLobby(string clientId)
	{
		try
		{
			for (int i = 0; i < Instance._Lobby.Players.Count; i++)
			{
				Player player = Instance._Lobby.Players[i];

				if (player.Data["PlayerNetworkID"].Value == clientId)
					await LobbyService.Instance.RemovePlayerAsync(Instance._LobbyId, player.Data["PlayerID"].Value);
			}
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}

	//LOBBY EVENTS
	private async void SubToLobbyEvents()
	{
		try
		{
			var callbacks = new LobbyEventCallbacks();
			callbacks.PlayerJoined += PlayerJoinedLobby;
			callbacks.PlayerLeft += PlayerLeftLobby;
			_LobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(_LobbyId, callbacks);
		}
		catch (LobbyServiceException ex)
		{
			switch (ex.Reason)
			{
				case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning
					($"Already subscribed to lobby[{_Lobby.Id}]. didnt need to subscribe again. Exception Message: {ex.Message}"); break;
				case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError(
					$"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
				case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError(
					$"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
				default: throw;
			}
		}
	}
	private void UnSubToLobbyEvents()
	{
		_LobbyEvents.Callbacks.PlayerJoined -= PlayerJoinedLobby;
		_LobbyEvents.Callbacks.PlayerLeft -= PlayerLeftLobby;
		_LobbyEvents = null;
	}

	private void PlayerJoinedLobby(List<LobbyPlayerJoined> playersJoined)
	{
		foreach (LobbyPlayerJoined player in playersJoined)
		{
			string playerName = player.Player.Data["PlayerName"].Value;
			PlayerPartyUi.Instance.SendPlayerJoinedMessage(playerName);
		}
	}
	private void PlayerLeftLobby(List<int> playerLeft)
	{
		string playerName = _Lobby.Players[playerLeft.Count].Data["PlayerName"].Value;
		PlayerPartyUi.Instance.SendPlayerLeftMessage(playerName);
	}

	//UPDATING CLIENT PLAYER DATA IN LOBBY
	private Player SetClientPlayerData()
	{
		return new Player
		{
			Data = new Dictionary<string, PlayerDataObject>
			{
				{ "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,
					ClientManager.Instance.clientUsername.ToString())},
				{ "PlayerID", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,
					ClientManager.Instance.clientId.ToString())},
				{ "PlayerNetworkID", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,
					ClientManager.Instance.clientNetworkedId.ToString())},
				{ "PlayerClass", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,
					GameManager.Localplayer.playerClassHandler.currentEntityClass.className.ToString())},
				{ "PlayerLevel", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,
					GameManager.Localplayer.playerStats.entityLevel.ToString())},
			}
		};
	}

	//update specific client player data
	public async void UpdateJoiningClientsNetworkID()
	{
		try
		{
			UpdatePlayerOptions options = new UpdatePlayerOptions();

			options.Data = new Dictionary<string, PlayerDataObject>()
			{
				{ "PlayerNetworkID", new PlayerDataObject(
				visibility: PlayerDataObject.VisibilityOptions.Member,
				value: ClientManager.Instance.clientNetworkedId.ToString())}
			};

			string playerId = AuthenticationService.Instance.PlayerId;
			await LobbyService.Instance.UpdatePlayerAsync(_LobbyId, playerId, options);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}
	public async void UpdateClientPlayerClass(SOClasses newClass)
	{
		try
		{
			UpdatePlayerOptions options = new UpdatePlayerOptions();

			options.Data = new Dictionary<string, PlayerDataObject>()
			{
				{ "PlayerClass", new PlayerDataObject(
				visibility: PlayerDataObject.VisibilityOptions.Member,
				value: newClass.className)}
			};

			string playerId = AuthenticationService.Instance.PlayerId;
			await LobbyService.Instance.UpdatePlayerAsync(_LobbyId, playerId, options);
		}
		catch (LobbyServiceException e)
		{
			if (e.ErrorCode == 429) //rate limit error
				StartCoroutine(RetryUpdatingClientPlayerInfo(null, newClass));
			else
				Debug.Log(e);
		}
	}
	public async void UpdateClientPlayerLevel(PlayerController player)
	{
		if (_LobbyId.IsNullOrEmpty()) return; //occasionally happens when first hosting lobby

		try
		{
			UpdatePlayerOptions options = new UpdatePlayerOptions();

			options.Data = new Dictionary<string, PlayerDataObject>()
			{
				{ "PlayerLevel", new PlayerDataObject(
				visibility: PlayerDataObject.VisibilityOptions.Member,
				value: player.playerStats.entityLevel.ToString())}
			};

			string playerId = AuthenticationService.Instance.PlayerId;
			await LobbyService.Instance.UpdatePlayerAsync(_LobbyId, playerId, options);
		}
		catch (LobbyServiceException e)
		{
			if (e.ErrorCode == 429) //rate limit error
				StartCoroutine(RetryUpdatingClientPlayerInfo(player, null));
			else
				Debug.Log(e);
		}
	}

	//retry on rate limit hit
	private IEnumerator RetryUpdatingClientPlayerInfo(PlayerController player, SOClasses newClass)
	{
		yield return new WaitForSeconds(5.5f);

		if (player == null)
			UpdateClientPlayerClass(newClass);
		else
			UpdateClientPlayerLevel(player);
	}

	public void LogSpecificPlayerInfo(string networkIdOfPlayerToLog)
	{
		foreach (Player player in  _Lobby.Players)
		{
			if (player.Data["PlayerNetworkID"].Value != networkIdOfPlayerToLog) continue;

			for (int i = 0; i < player.Data.Count; i++)
			{
				if (i == 0)
					Debug.LogError("PlayerName: " + player.Data["PlayerName"].Value);
				else if (i == 1)
					Debug.LogError("PlayerID: " + player.Data["PlayerID"].Value);
				else if (i == 2)
					Debug.LogError("PlayerNetworkID: " + player.Data["PlayerNetworkID"].Value);
				else if (i == 3)
					Debug.LogError("PlayerLevel: " + player.Data["PlayerLevel"].Value);
				else if (i == 4)
					Debug.LogError("PlayerClass: " + player.Data["PlayerClass"].Value);
			}
		}
	}

	//FETCHING LOBBY PLAYER DATA
	//fetch player based join order
	public string GetPlayerName(int index)
	{
		if (!_Lobby.Players[index].Data.TryGetValue("PlayerName", out PlayerDataObject playerName))
			Debug.LogError("player Name Key not found");

		return playerName.Value.ToString();
	}
	public string GetPlayerLevel(int index)
	{
		if (!_Lobby.Players[index].Data.TryGetValue("PlayerLevel", out PlayerDataObject playerLevel))
			Debug.LogError("player level Key not found");

		return playerLevel.Value.ToString();
	}
	public string GetPlayerClass(int index)
	{
		if (!_Lobby.Players[index].Data.TryGetValue("PlayerClass", out PlayerDataObject playerClass))
			Debug.LogError("player Class Key not found");

		return playerClass.Value.ToString();
	}
	public ulong GetPlayerNetworkId(int index)
	{
		if (!_Lobby.Players[index].Data.TryGetValue("PlayerNetworkID", out PlayerDataObject playerNetworkId))
			Debug.LogError("player Name Key not found");

		return Convert.ToUInt64(playerNetworkId.Value);
	}

	//fetch specific player
	public string GetSpecificPlayerName(ulong clientId)
	{
		string playerName = "";

		foreach (Player player in _Lobby.Players)
		{
			if (!player.Data.TryGetValue("PlayerNetworkID", out PlayerDataObject playerNetworkId))
			{
				Debug.LogError("Player With Network ID Not Found");
				break;
			}

			if (clientId != Convert.ToUInt64(playerNetworkId.Value)) continue;
			else
				playerName = player.Data["PlayerName"].Value;
		}
		return playerName;
	}
	public string GetSpecificPlayerLevel(ulong clientId)
	{
		string playerLevel = "";

		foreach (Player player in _Lobby.Players)
		{
			if (!player.Data.TryGetValue("PlayerNetworkID", out PlayerDataObject playerNetworkId))
			{
				Debug.LogError("Player With Network ID Not Found");
				break;
			}

			if (clientId != Convert.ToUInt64(playerNetworkId.Value)) continue;
			else
				playerLevel = player.Data["PlayerLevel"].Value;
		}
		return playerLevel;
	}
	public string GetSpecificPlayerClass(ulong clientId)
	{
		string playerClass = "";

		foreach (Player player in _Lobby.Players)
		{
			if (!player.Data.TryGetValue("PlayerNetworkID", out PlayerDataObject playerNetworkId))
			{
				Debug.LogError("Player With Network ID Not Found");
				break;
			}

			if (clientId != Convert.ToUInt64(playerNetworkId.Value)) continue;
			else
				playerClass = player.Data["PlayerClass"].Value;
		}
		return playerClass;
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
				Instance._Lobby = await LobbyService.Instance.GetLobbyAsync(Instance._LobbyId);
				LobbyUi.Instance.SyncPlayerListforLobbyUi(Instance._Lobby);
				PlayerPartyUi.Instance.SyncPlayerListforPartyUi(Instance._Lobby);
			}
			catch (LobbyServiceException e)
			{
				Debug.LogError($"{e.Message}");
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
