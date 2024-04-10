using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonHandler : MonoBehaviour
{
	public static event Action<Vector2> OnSetPlayerSpawn;

	public List<GameObject> dungeonPortalsList = new List<GameObject>();

	private void OnEnable()
	{
		SaveManager.RestoreData += SetPlayerSpawn;
	}

	private void OnDisable()
	{
		SaveManager.RestoreData -= SetPlayerSpawn;
	}

	public void SetPlayerSpawn()
	{
		if (dungeonPortalsList.Count <= 0) return;
		Debug.Log("invoke set player spawn");
		GameObject portalSpawnPoint = dungeonPortalsList[Utilities.GetRandomNumber(dungeonPortalsList.Count - 1)];
		OnSetPlayerSpawn?.Invoke(portalSpawnPoint.transform.position);
	}
}
