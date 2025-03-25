using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using TMPro;

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

		PlayerDeathUi.Instance.CheckDeadPlayersOnClientDisconnect();
	}

	//UPDATE MP MODE
	public static void UpdateIsMultiplayer(bool isMultiplayer)
	{
		if (isMultiplayer)
		{
			PlayerPartyUi.Instance.playerPartyPanelUi.SetActive(true);
			PlayerPartyUi.Instance.partyMessagesPanelUi.SetActive(true);
		}
		else
		{
			PlayerPartyUi.Instance.playerPartyPanelUi.SetActive(false);
			PlayerPartyUi.Instance.partyMessagesPanelUi.SetActive(false);
		}

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
		ulong clientId = sceneEvent.ClientId;

		switch (sceneEvent.SceneEventType)
		{
			// Handle server to client Load Notifications
			case SceneEventType.Load:
			{
				Debug.LogError("load for server ID: " + clientId + " | at: " + DateTime.Now.ToString());

				if (IsHost) return;
				if (IsClient)
					GameManager.Instance.UnloadSceneForConnectedClients();

				break;
			}
			// Handle client to server LoadComplete notifications
			case SceneEventType.LoadComplete:
			{
				// Server Side: receives thisn'tification for both itself and all clients
				if (IsServer)
				{
					Debug.LogError("loadCompleted for server ID: " + clientId + " | at: " + DateTime.Now.ToString());
				}
				else // Clients generate thisn'tification locally
				{
					Debug.LogError("loadCompleted for client ID: " + clientId + " | at: " + DateTime.Now.ToString());

					//if (sceneEvent.SceneName == GameManager.Instance.uiScene) //restore data for joining clients after clearing dup scenes
						//SaveManager.Instance.ReloadSaveGameDataEvent();
				}
				break;
			}
			// Handle Server to Client Load Complete (all clients finished loading notification)
			case SceneEventType.LoadEventCompleted:
			{
				foreach (var clientIdLoadComplete in sceneEvent.ClientsThatCompleted)
				{
					// Example of parsing through the clients that completed list
					if (IsServer)
					{
						Debug.LogError("loadEventCompleted for server ID: " + clientIdLoadComplete + " | at: " + DateTime.Now.ToString());

						DungeonHandler.Instance.TrySyncChestStates();
					}
					else
					{
						Debug.LogError("loadEventCompleted for client ID: " + clientIdLoadComplete + " | at: " + DateTime.Now.ToString());
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
		else
		{
			//Debug.LogError("CLIENT IS HOST/SP GAME");
			return true;
		}
	}
}
