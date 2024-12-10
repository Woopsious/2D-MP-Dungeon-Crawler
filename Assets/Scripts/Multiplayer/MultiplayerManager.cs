using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class MultiplayerManager : NetworkBehaviour
{
	public static MultiplayerManager Instance;

	public bool isMultiplayer;

	public static Action MarkLobbyUiAsDirty;

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

	public async Task AuthenticatePlayer()
	{
		await UnityServices.InitializeAsync();

		if (!AuthenticationService.Instance.IsAuthorized)
		{
			AuthenticationService.Instance.SignedIn += () => { Debug.Log($"Player Id: {AuthenticationService.Instance.PlayerId}"); };
			AuthenticationService.Instance.SignInFailed += (err) => {Debug.LogError("Player Sign In Failed");};
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
		}
		ClientManager.Instance.clientId = AuthenticationService.Instance.PlayerId;
	}
	public void UnAuthenticatePlayer()
	{
		AuthenticationService.Instance.SignOut();
	}

	//CLIENT CONNECT/DISCONNECT EVENTS
	public void SubToEvents()
	{
		NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback += PlayerDisconnectedCallback;
	}
	public void UnsubToEvents()
	{
		NetworkManager.Singleton.OnClientConnectedCallback -= PlayerConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerDisconnectedCallback;
	}
	public void PlayerConnectedCallback(ulong id)
	{
		Debug.LogError("player connected");

		if (id == NetworkManager.Singleton.LocalClientId)
		{
			int i = HostManager.Instance.connectedClientsList.Count;
			ClientManager.Instance.clientNetworkedId = NetworkManager.Singleton.LocalClientId;

			if (!Instance.IsPlayerHost())
			{
				Player player = LobbyManager.Instance._Lobby.Players[i];
				LobbyManager.Instance.UpdatePlayer(player.Data["PlayerName"].Value, player.Data["PlayerID"].Value, id.ToString(),
					player.Data["PlayerLevel"].Value, player.Data["PlayerClass"].Value);
			}

			Debug.LogError("client networkid: " + NetworkManager.Singleton.LocalClientId);
		}
		else
		{
			Debug.LogError("client networkid doesnt match | id: " + id +" | local id: " + NetworkManager.Singleton.LocalClientId);
		}

		if (IsPlayerHost())
		{
			int i = HostManager.Instance.connectedClientsList.Count;
			Debug.LogError("connected clients count: " + HostManager.Instance.connectedClientsList.Count);

			if (id == 0) //grab host data locally as lobby is not yet made
			{
				Debug.LogWarning("joining client is host");

				ClientDataInfo data = new(ClientManager.Instance.clientUsername,
					ClientManager.Instance.clientId, ClientManager.Instance.clientNetworkedId);

				HostManager.Instance.connectedClientsList.Add(data);
			}
			else //grab other clients data through lobby
			{
				Debug.LogWarning("joining client is not host");

				Player player = LobbyManager.Instance._Lobby.Players[i];

				ClientDataInfo data = new(player.Data["PlayerName"].Value, player.Data["PlayerID"].Value, id);

				if (!LobbyManager.Instance._Lobby.Players[i].Data.TryGetValue("PlayerNetworkID", out PlayerDataObject playerNetworkId))
					Debug.LogError("player NetworkedId Key not found");
                else
                {
					Debug.LogError("joining player NetworkedId in lobby: " + playerNetworkId.Value);
				}

                HostManager.Instance.connectedClientsList.Add(data);
			}
		}

		MarkLobbyUiAsDirty?.Invoke();

		/*
		if (!MenuUIManager.Instance.MpLobbyPanel.activeInHierarchy) //enable lobby ui once connected to relay
			MenuUIManager.Instance.ShowLobbyUi();
		*/
	}
	public void PlayerDisconnectedCallback(ulong id)
	{
		Debug.LogError("player disconnected, id:" + id);

		MarkLobbyUiAsDirty?.Invoke();

		if (IsPlayerHost())
			HostManager.Instance.HandlePlayerDisconnectsAsHost(id);
		else
			ClientManager.Instance.HandlePlayerDisconnectsAsClient(id);
	}

	//Shutdown NetworkManager
	public void ShutDownNetworkManagerIfActive()
	{
		if (NetworkManager.Singleton.isActiveAndEnabled)
			NetworkManager.Singleton.Shutdown();
	}

	public static bool CheckIfMultiplayerMenusOpen()
	{
		if (MultiplayerMenuUi.Instance.MpMenuUiPanel.activeInHierarchy ||
			LobbyListUi.Instance.LobbyListUiPanel.activeInHierarchy ||
			LobbyUi.Instance.LobbySettingsUiPanel.activeInHierarchy ||
			LobbyUi.Instance.LobbyUiPanel.activeInHierarchy)
			return true;
		else return false;
	}

	//bool checks
	public bool IsMultiplayer()
	{
		if (NetworkManager.Singleton != null)
		{       
			//Debug.LogError("is Multiplayer");
			return true;
		}
		//Debug.LogError("is Singleplayer");
		return false;
	}
	public bool IsPlayerHost()
	{
		if (NetworkManager.Singleton != null)
		{
			if (NetworkManager.Singleton.IsHost)
			{
				//Debug.LogError("CLIENT IS HOST");
				return true;
			}
			else
			{
				//Debug.LogError("CLIENT IS NOT HOST");
				return false;
			}
		}
		//Debug.LogError("NetworkManager doesnt exist");
		return false;
	}
}
