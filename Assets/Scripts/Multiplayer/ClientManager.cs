using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;

public class ClientManager : NetworkBehaviour
{
	public static ClientManager Instance;

	public string clientUsername = "PlayerName";
	public string clientId;
	public ulong clientNetworkedId;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			HostManager.Instance.connectedClientsList = new NetworkList<ClientDataInfo>();
			DontDestroyOnLoad(Instance);
		}
		else
			Destroy(gameObject);
	}

	//START/STOP CLIENT
	public void StartClientAndJoinLobby(Lobby lobby)
	{
		GameManager.Instance.PauseGame(false);
		LobbyManager.Instance.JoinLobby(lobby);
		MultiplayerManager.Instance.SubToEvents();
		MultiplayerManager.Instance.isMultiplayer = true;
	}
	public void StopClient()
	{
		LobbyManager.Instance.ResetLobbyReferences();
		MultiplayerManager.Instance.UnsubToEvents();
		MultiplayerManager.Instance.ShutDownNetworkManagerIfActive();
		MultiplayerManager.Instance.isMultiplayer = false;
	}

	//JOINING HOST RELAY SERVER
	public IEnumerator RelayConfigureTransportAsConnectingPlayer()
	{
		// Populate RelayJoinCode beforehand through the UI
		var clientRelayUtilityTask = JoinRelayServerFromJoinCode(LobbyManager.Instance.lobbyJoinCode);

		while (!clientRelayUtilityTask.IsCompleted)
		{
			yield return null;
		}

		if (clientRelayUtilityTask.IsFaulted)
		{
			Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
			yield break;
		}

		var relayServerData = clientRelayUtilityTask.Result;

		NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
		NetworkManager.Singleton.StartClient();
		NetworkManager.Singleton.SceneManager.OnSceneEvent += MultiplayerManager.Instance.SceneManager_OnSceneEvent;
	}
	public static async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
	{
		JoinAllocation allocation;
		try
		{
			allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
		}
		catch
		{
			Debug.LogError("Relay create join code request failed");
			throw;
		}
		return new RelayServerData(allocation, "dtls");
	}

	//HANDLE CLIENT CONNECTS/DISCONNECTS EVENTS
	public void HandleClientConnectsAsClient(ulong id)
	{
		//noop
	}
	public void HandleClientDisconnectsAsClient(ulong id)
	{
		if (id != Instance.clientNetworkedId) return;
		///<summery>
		/// if disconnected player is this player stop client
		/// shut down all mp related stuff, probably send player back to hub world/reload hubworld
		/// possibly save player inventory or not??
		///<summery>

		MultiplayerMenuUi.Instance.SetDisconnectReason(NetworkManager.DisconnectReason);
		MultiplayerMenuUi.Instance.ShowDisconnectUiPanel();
		StopClient();
	}
}

//client data
[Serializable]
public struct ClientDataInfo : INetworkSerializable, IEquatable<ClientDataInfo>
{
	public FixedString64Bytes clientName;
	public FixedString64Bytes clientId;
	public ulong clientNetworkedId;

	public ClientDataInfo(string playerName = "not set", string clientId = "No Id Token", ulong clientNetworkedId = 0)
	{
		this.clientName = playerName;
		this.clientId = clientId;
		this.clientNetworkedId = clientNetworkedId;
	}
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref clientName);
		serializer.SerializeValue(ref clientId);
		serializer.SerializeValue(ref clientNetworkedId);
	}
	public bool Equals(ClientDataInfo other)
	{
		return clientName == other.clientName && clientId == other.clientId && clientNetworkedId == other.clientNetworkedId;
	}
}
