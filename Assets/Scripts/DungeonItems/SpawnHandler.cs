using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
	public List<GameObject> possibleEntitiesPrefabsToSpawn = new List<GameObject>();

	public int debugSpawnerLevel;
	public bool debugSpawnEnemiesAtSetLevel;

	public int spawnerLevel;
	public int maxNumOfEnemiesToSpawn;
	private List<GameObject> listOfSpawnedEnemies = new List<GameObject>();

	private CircleCollider2D playerCollider;
	private List<PlayerController> listOfPlayersInRange = new List<PlayerController>();

	private void Start()
	{
		TrySpawnEntity();
	}
	private void OnEnable()
	{
		EventManager.OnDeathEvent += OnEntityDeath;
		EventManager.OnPlayerLevelUpEvent += OnPlayerLevelUpUpdateSpawnerLevel;
		GameManager.OnSceneChangeFinish += TrySpawnEntity;
	}

	private void OnDisable()
	{
		EventManager.OnDeathEvent -= OnEntityDeath;
		EventManager.OnPlayerLevelUpEvent -= OnPlayerLevelUpUpdateSpawnerLevel;
		GameManager.OnSceneChangeFinish -= TrySpawnEntity;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<PlayerController>() != null)
			listOfPlayersInRange.Add(other.GetComponent<PlayerController>());
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.GetComponent<PlayerController>() != null)
		{
			if (listOfPlayersInRange.Contains(other.GetComponent<PlayerController>()))
			{
				listOfPlayersInRange.Remove(other.GetComponent<PlayerController>());
				TrySpawnEntity();
			}
		}
	}

	private void OnPlayerLevelUpUpdateSpawnerLevel(EntityStats playerStats)
	{
		spawnerLevel = playerStats.entityLevel;
	}
	private void OnEntityDeath(GameObject obj)
	{
		if (listOfSpawnedEnemies.Contains(obj))
		{
			listOfSpawnedEnemies.Remove(obj);
			TrySpawnEntity();
		}
	}

	private void TrySpawnEntity()
	{
		if (listOfSpawnedEnemies.Count >= maxNumOfEnemiesToSpawn) return;
		if (listOfPlayersInRange.Count != 0) return;

		SpawnRandomEntity();
	}
	private void SpawnRandomEntity()
	{
		int num = Utilities.GetRandomNumber(possibleEntitiesPrefabsToSpawn.Count - 1);
		GameObject go = Instantiate(possibleEntitiesPrefabsToSpawn[num], transform.position, transform.rotation);
		listOfSpawnedEnemies.Add(go);

		if (debugSpawnEnemiesAtSetLevel)
			go.GetComponent<EntityStats>().entityLevel = debugSpawnerLevel;
		else
			go.GetComponent<EntityStats>().entityLevel = spawnerLevel;

		TrySpawnEntity();
	}
}
