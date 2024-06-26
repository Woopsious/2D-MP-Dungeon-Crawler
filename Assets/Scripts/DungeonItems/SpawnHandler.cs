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

	//if player distance to spawner above 50 and enemies spawner tracks isnt engaging player despawn them
	//if player distance below 50 and above 25 spawn enemies
	private CircleCollider2D playerCollider; //radius set to 50 distance
	private readonly int minSpawningDistance = 25;
	public List<PlayerController> listOfPlayersInRange = new List<PlayerController>();

	private void OnEnable()
	{
		EventManager.OnDeathEvent += OnEntityDeath;
		EventManager.OnPlayerLevelUpEvent += OnPlayerLevelUpUpdateSpawnerLevel;
		GameManager.OnSceneChangeFinish += TrySpawnEntity;

		TrySpawnEntity();
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

		TrySpawnEntity();
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.GetComponent<PlayerController>() != null)
		{
			if (listOfPlayersInRange.Contains(other.GetComponent<PlayerController>()))
				listOfPlayersInRange.Remove(other.GetComponent<PlayerController>());
		}
	}

	private bool IsPlayerInsideMinSpawningDistance()
	{
		foreach (PlayerController player in listOfPlayersInRange)
		{
			if (Vector2.Distance(player.transform.position, transform.position) <= minSpawningDistance)
				return true;
		}
		return false;
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
		if (listOfPlayersInRange.Count == 0) return;
		if (IsPlayerInsideMinSpawningDistance()) return;

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
