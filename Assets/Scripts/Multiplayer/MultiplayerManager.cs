using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MultiplayerManager : NetworkBehaviour
{
	public static MultiplayerManager Instance;

	public GameObject PlayerPrefab;

	public PlayerController localPlayer;

	//public List<PlayerController> ListOfplayers = new List<PlayerController>();

	public GameObject HostClientManagerObj;
	private bool isMultiplayer;

	[Header("Disconnect Menu")]
	public GameObject disconnectUiPanel;
	public TMP_Text disconnectReasonText;

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

		if (IsClientHost())
			HostManager.Instance.HandleClientConnectsAsHost(id);
		else
			ClientManager.Instance.HandleClientConnectsAsClient(id);

		if (NetworkManager.Singleton.LocalClientId == id)
			LobbyUi.Instance.ShowLobbyUi();
	}
	public void PlayerDisconnectedCallback(ulong id)
	{
		if (IsClientHost())
			HostManager.Instance.HandleClientDisconnectsAsHost(id);
		else
			ClientManager.Instance.HandleClientDisconnectsAsClient(id);
	}

	//UPDATE MP MODE
	public static void UpdateIsMultiplayer(bool isMultiplayer)
	{
		Instance.isMultiplayer = isMultiplayer;
	}

	//Spawning/Shutdown NetworkManager
	public void SpawnHostClientManager()
	{
		GameObject go = Instantiate(HostClientManagerObj);
		go.transform.SetParent(null);
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
		// Both client and server receive these notifications
		switch (sceneEvent.SceneEventType)
		{
			// Handle server to client Load Notifications
			case SceneEventType.Load:
			{
				// This event provides you with the associated AsyncOperation
				// AsyncOperation.progress can be used to determine scene loading progression

				var asyncLoadScene = sceneEvent.AsyncOperation;
				ulong clientId = sceneEvent.ClientId;

				if (IsClient)
					GameManager.Instance.UnloadSceneForConnectedClients();

				// Since the server "initiates" the event we can simply just check if we are the server here
				if (IsServer)
				{
					Debug.LogError("load for server, | ID: " + clientId + " | at: " + DateTime.Now.ToString());
					// Handle server side load event related tasks here
				}
				else
				{
					// Handle client side load event related tasks here
					Debug.LogError("load for client, | ID: " + clientId + " | at: " + DateTime.Now.ToString());
				}
				break;
			}
			// Handle server to client unload notifications
			case SceneEventType.Unload:
			{
				// You can use the same pattern above under SceneEventType.Load here
				break;
			}
			// Handle client to server LoadComplete notifications
			case SceneEventType.LoadComplete:
			{
				// This will let you know when a load is completed
				// Server Side: receives thisn'tification for both itself and all clients

				ulong clientId = sceneEvent.ClientId;
				if (IsServer)
				{
					Debug.LogError("loadCompleted for server, | ID: " + clientId + " | at: " + DateTime.Now.ToString());

					if (sceneEvent.ClientId == NetworkManager.LocalClientId)
					{
						// Handle server side LoadComplete related tasks here
					}
					else
					{
						// Handle client LoadComplete **server-side** notifications here
					}
				}
				else // Clients generate thisn'tification locally
				{
					// Handle client side LoadComplete related tasks here
					Debug.LogError("loadCompleted for client, | ID: " + clientId + " | at: " + DateTime.Now.ToString());

					if (sceneEvent.SceneName == GameManager.Instance.uiScene) //restore data for joining clients after clearing dup scenes
						SaveManager.Instance.ReloadSaveGameDataEvent();
				}

				// So you can use sceneEvent.ClientId to also track when clients are finished loading a scene
				break;
			}
			// Handle Client to Server Unload Complete Notification(s)
			case SceneEventType.UnloadComplete:
			{
				// This will let you know when an unload is completed
				// You can follow the same pattern above as SceneEventType.LoadComplete here

				// Server Side: receives thisn'tification for both itself and all clients
				// Client Side: receives thisn'tification for itself

				// So you can use sceneEvent.ClientId to also track when clients are finished unloading a scene
				break;
			}
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
						Debug.LogError("loadEventCompleted for server, | ID: " + clientId + " | at: " + DateTime.Now.ToString());
					}
					else
					{
						// Handle any client-side tasks here
						Debug.LogError("loadEventCompleted for client, | ID: " + clientId + " | at: " + DateTime.Now.ToString());
					}
				}
				break;
			}
			// Handle Server to Client unload Complete (all clients finished unloading notification)
			case SceneEventType.UnloadEventCompleted:
			{
				// This will let you know when all clients have finished unloading a scene
				// Received on both server and clients
				foreach (var clientId in sceneEvent.ClientsThatCompleted)
				{
					// Example of parsing through the clients that completed list
					if (IsServer)
					{
						// Handle any server-side tasks here
					}
					else
					{
						// Handle any client-side tasks here
					}
				}
				break;
			}
		}
	}

	//bool checks
	public static bool IsMultiplayer()
	{
		if (Instance.isMultiplayer)
		{       
			//Debug.LogError("is Multiplayer");
			return true;
		}
		//Debug.LogError("is Singleplayer");
		return false;
	}
	public static bool IsClientHost()
	{
		if (IsMultiplayer())
		{
			if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
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

		//Debug.LogError("CLIENT IS HOST/SP GAME");
		return true;
	}
}
