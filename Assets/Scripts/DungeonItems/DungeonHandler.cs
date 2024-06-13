using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonHandler : MonoBehaviour
{
	public static DungeonHandler Instance;

	public List<GameObject> dungeonPortalsList = new List<GameObject>();

	//keep track of all chests when player leaves dungeon, save to GameData in DungeonData function weather they have/havnt been opened
	//when player revisits dungeon OnSceneChangeFinish restore state of chests from GameData
	public List<ChestHandler> dungeonChestLists = new List<ChestHandler>();

	private void Awake()
	{
		Instance = this;
	}
	private void OnEnable()
	{
		GameManager.OnSceneChangeFinish += SetPlayerSpawn;
		SaveManager.RestoreData += RestoreDungeonChestData;
	}
	private void OnDisable()
	{
		GameManager.OnSceneChangeFinish -= SetPlayerSpawn;
		SaveManager.RestoreData -= RestoreDungeonChestData;
	}

	private void RestoreDungeonChestData()
	{
		Debug.Log("restoring chest data");
		if (GameManager.Instance.currentDungeonData.dungeonChestData.Count <= 0) return; //return when dungeon first entered

		int i = 0;
		foreach (DungeonChestData chestData in GameManager.Instance.currentDungeonData.dungeonChestData)
		{
			if (chestData.chestStateOpened)
				dungeonChestLists[i].ChangeChestStateToOpen(false);
			i++;
		}
	}
	private void SetPlayerSpawn()
	{
		if (dungeonPortalsList.Count <= 0) return;

		//need a better solution but it works for now, for mp will have to use player refs stored in a lobbyHandler or something similar
		//then probably call an event to equip the gear the player is actually supposed to be wearing as well as when ever they change
		//damage, spells, skill etc should just work if i call them through the rpc network manager events

		/*
		PlayerController player = PlayerInventoryManager.Instance.GetComponent<PlayerController>();
		GameObject portalSpawnPoint = dungeonPortalsList[Utilities.GetRandomNumber(dungeonPortalsList.Count - 1)];
		player.transform.position = portalSpawnPoint.transform.position;
		*/

		PlayerController[] players = FindObjectsOfType<PlayerController>();
		GameObject portalSpawnPoint = dungeonPortalsList[Utilities.GetRandomNumber(dungeonPortalsList.Count - 1)];

		foreach (PlayerController player in players)
			player.transform.position = portalSpawnPoint.transform.position;
	}
}

[System.Serializable]
public class DungeonStatModifier
{
	public float difficultyModifier;

	public float healthModifier;
	public float manaModifier;
	public float physicalResistanceModifier;
	public float poisonResistanceModifier;
	public float fireResistanceModifier;
	public float iceResistanceModifier;

	public float physicalDamageModifier;
	public float poisonDamageModifier;
	public float fireDamageModifier;
	public float iceDamageModifier;
	public float mainWeaponDamageModifier;
	public float dualWeaponDamageModifier;
	public float rangedWeaponDamageModifier;
}
