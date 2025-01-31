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
}
