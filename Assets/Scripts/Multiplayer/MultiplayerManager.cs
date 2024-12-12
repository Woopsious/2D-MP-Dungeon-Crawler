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
using Unity.VisualScripting;

public class MultiplayerManager : MonoBehaviour
{
	public static MultiplayerManager Instance;

	public GameObject HostClientManagerObj;
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
		if (IsPlayerHost())
			HostManager.Instance.HandleClientConnectsAsHost(id);
		else
			ClientManager.Instance.HandleClientConnectsAsClient(id);

		if (NetworkManager.Singleton.LocalClientId == id)
			LobbyUi.Instance.ShowLobbyUi();

		MarkLobbyUiAsDirty?.Invoke();
	}
	public void PlayerDisconnectedCallback(ulong id)
	{
		if (IsPlayerHost())
			HostManager.Instance.HandleClientDisconnectsAsHost(id);
		else
			ClientManager.Instance.HandleClientDisconnectsAsClient(id);

		MarkLobbyUiAsDirty?.Invoke();
	}

	//Shutdown NetworkManager
	public void ShutDownNetworkManagerIfActive()
	{
		if (NetworkManager.Singleton.isActiveAndEnabled)
			NetworkManager.Singleton.Shutdown();
	}
	public void SpawnHostClientManager()
	{
		GameObject go = Instantiate(HostClientManagerObj);
		go.transform.SetParent(null);
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
