using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonHandler : MonoBehaviour
{
	//public static event Action<Vector2> OnSetPlayerSpawn;

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

		//need a better solution but it works for now, for mp will also need to run this again after spawning in copies of other players.
		//then probably call an event to equip the gear the player is actually supposed to be wearing as well as when ever they change
		//damage, spells, skill etc should just work if i call them through the rpc network manager events

		PlayerController[] players = FindObjectsOfType<PlayerController>();
		GameObject portalSpawnPoint = dungeonPortalsList[Utilities.GetRandomNumber(dungeonPortalsList.Count - 1)];

		foreach (PlayerController player in players)
			player.transform.position = portalSpawnPoint.transform.position;

		//Debug.Log("invoke set player spawn");
		//GameObject portalSpawnPoint = dungeonPortalsList[Utilities.GetRandomNumber(dungeonPortalsList.Count - 1)];
		//OnSetPlayerSpawn?.Invoke(portalSpawnPoint.transform.position);
	}
}

[System.Serializable]
public class DungeonStatModifier
{
	public float percentageValue;

	public ModifierType modifierType;
	public enum ModifierType
	{
		healthMod, manaMod, physicalResistanceMod, poisonResistanceMod, fireResistanceMod, iceResistanceMod, 
		physicalDamageMod, poisonDamageMod, fireDamageMod, iceDamageMod, mainWeaponDamageMod, dualWeaponDamageMod, rangedWeaponDamageMod
	}
}
