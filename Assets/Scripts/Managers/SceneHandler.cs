using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
	public static SceneHandler Instance;

	public Camera playerCamera;

	public GameObject PlayerPrefab;
	public static PlayerController playerInstance {  get; private set; }

	private void Awake()
	{
		Instance = this;
		playerCamera.transform.parent = null;

		SpawnPlayerObject();
	}

	private void SpawnPlayerObject()
	{
		if (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.isMultiplayer)
		{
			Debug.LogError("is multipler = " + MultiplayerManager.Instance.isMultiplayer);

			if (FindObjectOfType<PlayerController>() != null)
			{
				Debug.LogWarning("player prefab already exists, ignore if testing");
				return;
			}

			GameObject player = Instantiate(PlayerPrefab);
			player.transform.parent = null;
		}
		else
		{
			Debug.LogError("is multipler = " + MultiplayerManager.Instance.isMultiplayer);
		}
	}
	public void SpawnNetworkedPlayerObject(ulong clientNetworkIdOfOwner)
	{
		GameObject playerObj = Instantiate(PlayerPrefab);
		NetworkObject playerNetworkedObj = playerObj.GetComponent<NetworkObject>();
		playerNetworkedObj.SpawnAsPlayerObject(clientNetworkIdOfOwner);
	}

	public void UpdateLocalPlayerInstance(PlayerController newPlayerInstance)
	{
		if (playerInstance != null && playerInstance != newPlayerInstance)
			Destroy(playerInstance.gameObject);

		playerInstance = newPlayerInstance;
	}
}
