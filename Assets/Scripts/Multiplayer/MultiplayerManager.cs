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
using Unity.Services.Qos.V2.Models;

public class MultiplayerManager : NetworkBehaviour
{
	public static MultiplayerManager Instance;

	public GameObject PlayerPrefab;

	public PlayerController localPlayer;

	public List<PlayerController> ListOfplayers = new List<PlayerController>();

	public GameObject HostClientManagerObj;
	public bool isMultiplayer;

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
		ClientManager.Instance.clientNetworkedId = NetworkManager.Singleton.LocalClientId;

		if (IsPlayerHost())
			HostManager.Instance.HandleClientConnectsAsHost(id);
		else
			ClientManager.Instance.HandleClientConnectsAsClient(id);

		if (NetworkManager.Singleton.LocalClientId == id)
			LobbyUi.Instance.ShowLobbyUi();
	}
	public void PlayerDisconnectedCallback(ulong id)
	{
		if (IsPlayerHost())
			HostManager.Instance.HandleClientDisconnectsAsHost(id);
		else
			ClientManager.Instance.HandleClientDisconnectsAsClient(id);
	}

	//Spawning/Shutdown NetworkManager
	public void SpawnHostClientManager()
	{
		GameObject go = Instantiate(HostClientManagerObj);
		go.transform.SetParent(null);
	}
	public void ShutDownNetworkManagerIfActive()
	{
		if (NetworkManager.Singleton.isActiveAndEnabled)
			NetworkManager.Singleton.Shutdown();
	}
	public static bool CheckIfMultiplayerMenusOpen()
	{
		if (MultiplayerMenuUi.Instance.MpMenuUiPanel.activeInHierarchy || LobbyListUi.Instance.LobbyListUiPanel.activeInHierarchy ||
			LobbyUi.Instance.LobbySettingsUiPanel.activeInHierarchy || LobbyUi.Instance.LobbyUiPanel.activeInHierarchy)
			return true;
		else return false;
	}

	//mp scene change complete event
	public void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
	{
		//disabled in sp as GameManager handles restoring data in sp
		if (!Instance.isMultiplayer) return;

		// Both client and server receive these notifications
		switch (sceneEvent.SceneEventType)
		{
			// Handle Server to Client Load Complete (all clients finished loading notification)
			case SceneEventType.LoadEventCompleted:
			{
				// This will let you know when all clients have finished loading a scene
				// Received on both server and clients


				foreach (var clientId in sceneEvent.ClientsThatCompleted)
				{
					// Example of parsing through the clients that completed list
					if (IsServer)
					{
						// Handle any server-side tasks here
						Debug.LogError("loadEventCompleted for server, client ID: " + clientId + " | at: " + DateTime.Now.ToString());
						SceneHandler.Instance.SpawnNetworkedPlayerObject(clientId);
					}
					else
					{
						// Handle any client-side tasks here
						Debug.LogError("loadEventCompleted for client, client ID: " + clientId + " | at: " + DateTime.Now.ToString());
					}
				}

				SaveManager.Instance.RestoreGameData();

				break;
			}
		}
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
