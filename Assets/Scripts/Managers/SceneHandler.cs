using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.SearchService;
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

		//SpawnSinglePlayerObject();

		if (GameManager.Instance == null)
			StartCoroutine(LoadMainScene());
	}

	private IEnumerator LoadMainScene()
	{
		AsyncOperation asyncLoadScene = SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);

		while (!asyncLoadScene.isDone)
			yield return null;
	}

	private void SpawnSinglePlayerObject()
	{
		if (FindObjectOfType<PlayerController>() != null)
		{
			Debug.LogWarning("player prefab already exists in scene, ignore if testing");
			return;
		}

		if (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.isMultiplayer)
			Instantiate(PlayerPrefab);
	}
	public void SpawnNetworkedPlayerObject(ulong clientNetworkIdOfOwner)
	{
		Debug.LogError("spawned player obj");

		GameObject playerObj = Instantiate(PlayerPrefab);
		playerObj.transform.position = DungeonHandler.Instance.GetDungeonEnterencePortal(playerObj);
		NetworkObject playerNetworkedObj = playerObj.GetComponent<NetworkObject>();
		playerNetworkedObj.SpawnAsPlayerObject(clientNetworkIdOfOwner, true);
	}

	public void UpdateLocalPlayerInstance(PlayerController newPlayerInstance)
	{
		if (playerInstance != null && playerInstance != newPlayerInstance)
			Destroy(playerInstance.gameObject);

		playerInstance = newPlayerInstance;
	}
}
