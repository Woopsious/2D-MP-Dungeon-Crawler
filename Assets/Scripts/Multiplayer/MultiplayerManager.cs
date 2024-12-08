using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using System.Threading.Tasks;

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
		MarkLobbyUiAsDirty?.Invoke();

		if (IsPlayerHost())
		{
			if (id == 0) //grab host data locally as lobby is not yet made
			{
				Debug.LogWarning("joining client is host");

				ClientDataInfo data = new ClientDataInfo(ClientManager.Instance.clientUsername,
					ClientManager.Instance.clientId, ClientManager.Instance.clientNetworkedId);

				HostManager.Instance.connectedClientsList.Add(data);
			}
			else //grab other clients data through lobby
			{
				Debug.LogWarning("joining client is not host");

				int i = HostManager.Instance.connectedClientsList.Count;

				ClientDataInfo data = new ClientDataInfo(LobbyManager.Instance._Lobby.Players[i].Data["PlayerName"].Value,
					LobbyManager.Instance._Lobby.Players[i].Data["PlayerID"].Value, id);

				HostManager.Instance.connectedClientsList.Add(data);
			}
		}

		/*
		if (!MenuUIManager.Instance.MpLobbyPanel.activeInHierarchy) //enable lobby ui once connected to relay
			MenuUIManager.Instance.ShowLobbyUi();
		*/
	}
	public void PlayerDisconnectedCallback(ulong id)
	{
		MarkLobbyUiAsDirty?.Invoke();

		if (IsPlayerHost())
			HostManager.Instance.HandlePlayerDisconnectsAsHost(id);
		else
			ClientManager.Instance.HandlePlayerDisconnectsAsClient();
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendClientDataToHostServerRPC(string clientUserName, string clientId, ulong clientNetworkId)
	{
		HostManager.Instance.connectedClientsList.Add(new ClientDataInfo(clientUserName, clientId, clientNetworkId));
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
			if (NetworkManager.Singleton.IsClient)
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
