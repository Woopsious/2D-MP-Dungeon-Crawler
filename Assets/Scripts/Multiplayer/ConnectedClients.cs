using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConnectedClients : NetworkBehaviour
{
	public static ConnectedClients Instance;

	[SerializeField]
	public NetworkList<ClientDataInfo> clientsList;

	private void Awake()
	{
		if (Instance != null && Instance != this)
			Destroy(gameObject);
		else
		{
			Instance = this;
			clientsList = new NetworkList<ClientDataInfo>();
			DontDestroyOnLoad(this.gameObject);
		}
	}

	public void AddClientToList(ClientDataInfo clientData)
	{
		if (Instance.clientsList == null)
			Instance.clientsList = new NetworkList<ClientDataInfo>();

		clientsList.Add(clientData);
	}

	public void RemoveClientFromList(ClientDataInfo clientData)
	{
		clientsList.Remove(clientData);
	}

	[Rpc(SendTo.Server, RequireOwnership = false)]
	private void RequestClientsSyncRPC()
	{
		SyncConnectedClientsListRPC();
	}

	[Rpc(SendTo.Everyone)]
	public void SyncConnectedClientsListRPC()
	{
		Debug.LogError("SYNC CONNECTED CLIENTS RPC");
		ClientDataInfo[] clientDataArray = HostManager.Instance.connectedClientsList.ToArray();
		SyncConnectedClients(clientDataArray);
	}
	private void SyncConnectedClients(ClientDataInfo[] clientDataArray)
	{
		Debug.LogError("SYNC CONNECTED CLIENTS");
		Instance.clientsList = new NetworkList<ClientDataInfo>();

		foreach (ClientDataInfo clientData in clientDataArray)
			Instance.clientsList.Add(clientData);
	}
}
