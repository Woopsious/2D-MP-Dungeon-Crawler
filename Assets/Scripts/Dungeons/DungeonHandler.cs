using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonHandler : MonoBehaviour
{
	public List<GameObject> dungeonPortalsList = new List<GameObject>();

	private void OnEnable()
	{
		GameManager.OnSceneChangeFinish += SetPlayerSpawn;
	}

	private void OnDisable()
	{
		GameManager.OnSceneChangeFinish -= SetPlayerSpawn;
	}

	public void SetPlayerSpawn()
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
