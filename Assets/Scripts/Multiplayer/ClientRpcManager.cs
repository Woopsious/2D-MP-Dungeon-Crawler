using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientRpcManager : NetworkBehaviour
{
	public static ClientRpcManager instance;

	private void Awake()
	{
		instance = this;
	}
}
