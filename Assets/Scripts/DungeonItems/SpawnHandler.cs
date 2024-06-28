using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
	//public List<GameObject> possibleEntitiesPrefabsToSpawn = new List<GameObject>();
	public GameObject enemyTemplatePrefab;
	public List<SOEntityStats> possibleEntityTypesToSpawn = new List<SOEntityStats>();

	public int debugSpawnerLevel;
	public bool debugSpawnEnemiesAtSetLevel;

	public int spawnerLevel;
	public int maxNumOfEnemiesToSpawn;
	private List<EntityStats> listOfSpawnedEnemies = new List<EntityStats>();

	//if player distance to spawner above 50 and enemies spawner tracks isnt engaging player despawn them
	//if player distance below 50 and above 25 spawn enemies
	private CircleCollider2D playerCollider; //radius set to 50 distance
	private readonly int minSpawningDistance = 25;
	private List<PlayerController> listOfPlayersInRange = new List<PlayerController>();

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
		if (other.GetComponent<PlayerController>() == null) return;

		if (listOfPlayersInRange.Contains(other.GetComponent<PlayerController>()))
			listOfPlayersInRange.Remove(other.GetComponent<PlayerController>());

		if (listOfPlayersInRange.Count != 0) return;

		ClearUpEntities();
	}

	private void OnPlayerLevelUpUpdateSpawnerLevel(EntityStats playerStats)
	{
		spawnerLevel = playerStats.entityLevel;
	}
	private void OnEntityDeath(GameObject obj)
	{
		if (listOfSpawnedEnemies.Contains(obj.GetComponent<EntityStats>()))
		{
			listOfSpawnedEnemies.Remove(obj.GetComponent<EntityStats>());
			TrySpawnEntity();
		}
	}

	private void ClearUpEntities()
	{
		if (listOfPlayersInRange.Count != 0) return;

		for (int i = listOfSpawnedEnemies.Count - 1; i >= 0; i--)
		{
			if (listOfSpawnedEnemies[i] == null) return;
			if (listOfSpawnedEnemies[i].GetComponent<EntityBehaviour>().player != null) continue; //leave entities in range of a player

			listOfSpawnedEnemies[i].gameObject.SetActive(false);
			listOfSpawnedEnemies[i].transform.position = Vector3.zero;
			DungeonHandler.Instance.inActiveEntityPool.Add(listOfSpawnedEnemies[i]);
			listOfSpawnedEnemies.Remove(listOfSpawnedEnemies[i]);
		}
	}

	private void TrySpawnEntity()
	{
		if (listOfSpawnedEnemies.Count >= maxNumOfEnemiesToSpawn) return;
		if (listOfPlayersInRange.Count == 0) return;
		if (IsPlayerInsideMinSpawningDistance()) return;

		int num = Utilities.GetRandomNumber(possibleEntityTypesToSpawn.Count - 1);
		bool entityTypeMatches = false;

		foreach (EntityStats entity in DungeonHandler.Instance.inActiveEntityPool)
		{
			if (entity.entityBaseStats == possibleEntityTypesToSpawn[num])
			{
				entityTypeMatches = true;
				DungeonHandler.Instance.inActiveEntityPool.Remove(entity);
				RespawnInActiveEntity(entity);
				break;
			}
		}
		if (!entityTypeMatches)
			SpawnNewEntity();
	}
	private void RespawnInActiveEntity(EntityStats entity)
	{
		entity.gameObject.transform.position = transform.position;
		entity.gameObject.SetActive(true);
		entity.ResetEntity();
		listOfSpawnedEnemies.Add(entity);

		if (debugSpawnEnemiesAtSetLevel)
			entity.entityLevel = debugSpawnerLevel;
		else
			entity.entityLevel = spawnerLevel;

		TrySpawnEntity();
	}
	private void SpawnNewEntity()
	{
		int num = Utilities.GetRandomNumber(possibleEntityTypesToSpawn.Count - 1);
		GameObject go = Instantiate(enemyTemplatePrefab, transform.position, transform.rotation);
		EntityStats entity = go.GetComponent<EntityStats>();
		entity.entityBaseStats = possibleEntityTypesToSpawn[num];
		entity.GetComponent<EntityBehaviour>().entityBehaviour = possibleEntityTypesToSpawn[num].entityBehaviour;
		listOfSpawnedEnemies.Add(entity);

		if (debugSpawnEnemiesAtSetLevel)
			entity.entityLevel = debugSpawnerLevel;
		else
			entity.entityLevel = spawnerLevel;

		TrySpawnEntity();
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
}
