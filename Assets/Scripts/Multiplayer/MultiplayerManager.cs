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

	//spawning/removing player object prefabs
	public void SpawnNetworkedPlayerObject(ulong clientNetworkIdOfOwner)
	{
		GameObject playerObj = Instantiate(PlayerPrefab);
		NetworkObject playerNetworkedObj = playerObj.GetComponent<NetworkObject>();
		playerNetworkedObj.SpawnAsPlayerObject(clientNetworkIdOfOwner);
	}
	public void UpdatePlayerReferences()
	{       
		///<summery>
		/// instead of restoring all data split single event into multiple
		/// first clearing old singleplayer player instance
		///<summery>

		PlayerController[] players = FindObjectsOfType<PlayerController>();

		foreach (PlayerController player in players)
		{
            if (player.IsLocalPlayer && player != SceneHandler.playerInstance)
			{
				player.transform.position = SceneHandler.playerInstance.transform.position;
				Destroy(SceneHandler.playerInstance.gameObject);
				SceneHandler.playerInstance = player;

				SaveManager.Instance.RestoreGameData();
				return;
			}
        }
	}

	public static bool CheckIfMultiplayerMenusOpen()
	{
		if (MultiplayerMenuUi.Instance.MpMenuUiPanel.activeInHierarchy || LobbyListUi.Instance.LobbyListUiPanel.activeInHierarchy ||
			LobbyUi.Instance.LobbySettingsUiPanel.activeInHierarchy || LobbyUi.Instance.LobbyUiPanel.activeInHierarchy)
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
