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
	public static PlayerController playerInstance;

	private void Awake()
	{
		Instance = this;
		playerCamera.transform.parent = null;

		SpawnPlayerObjForSp();
	}

	private void SpawnPlayerObjForSp()
	{
		if (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.isMultiplayer)
		{
			if (FindObjectOfType<PlayerController>() != null)
			{
				Debug.LogWarning("player prefab already exists, ignore if testing");
				return;
			}

			GameObject player = Instantiate(PlayerPrefab);
			player.transform.parent = null;
		}
	}
}
