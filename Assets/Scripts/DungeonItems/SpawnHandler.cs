using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

public class SpawnHandler : MonoBehaviour
{
	[Header("Spawner Info")]
	public int debugSpawnerLevel;
	public bool debugSpawnEnemiesAtSetLevel;

	public int spawnerLevel;
	public int maxNumOfEnemiesToSpawn;
	private List<EntityStats> listOfSpawnedEnemies = new List<EntityStats>();

	[Header("Spawner Range Settings")]
	public int maxSpawningDistance;
	public int minSpawningDistance;
	[HideInInspector] public Bounds spawnBounds;

	[Header("Spawner Entity Types")]
	public GameObject enemyTemplatePrefab;
	public List<SOEntityStats> possibleEntityTypesToSpawn = new List<SOEntityStats>();

	private CircleCollider2D playerCollider;
	private List<PlayerController> listOfPlayersInRange = new List<PlayerController>();
	private float closestPlayerDistance;
	private bool spawningDisabled;

	private float totalEnemySpawnChance;
	private List<float> enemySpawnChanceTable = new List<float>();

	private void Awake()
	{
		Initilize();
	}
	private void OnEnable()
	{
		DungeonHandler.OnEntityDeathEvent += OnEntityDeath;
		PlayerEventManager.OnPlayerLevelUpEvent += OnPlayerLevelUpUpdateSpawnerLevel;
		GameManager.OnSceneChangeFinish += TrySpawnEntity;

		TrySpawnEntity();
	}
	private void OnDisable()
	{
		DungeonHandler.OnEntityDeathEvent -= OnEntityDeath;
		PlayerEventManager.OnPlayerLevelUpEvent -= OnPlayerLevelUpUpdateSpawnerLevel;
		GameManager.OnSceneChangeFinish -= TrySpawnEntity;

		enemySpawnChanceTable.Clear();
		totalEnemySpawnChance = 0;
		StopAllCoroutines();
	}

	//track players
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (listOfPlayersInRange.Count <= 0)
			spawningDisabled = false;

		if (other.GetComponent<PlayerController>() != null)
			listOfPlayersInRange.Add(other.GetComponent<PlayerController>());

		TrySpawnEntity();
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.GetComponent<PlayerController>() == null) return;

		if (listOfPlayersInRange.Contains(other.GetComponent<PlayerController>()))
			listOfPlayersInRange.Remove(other.GetComponent<PlayerController>());

		closestPlayerDistance = maxSpawningDistance;
		if (listOfPlayersInRange.Count != 0) return;

		spawningDisabled = false;
		CleanUpEntities();
	}

	private void FixedUpdate()
	{
		TrackClosestPlayerToSpawner();
	}

	private void Initilize()
	{
		playerCollider = GetComponent<CircleCollider2D>();
		playerCollider.radius = maxSpawningDistance;
		closestPlayerDistance = maxSpawningDistance;
		spawningDisabled = false;

		spawnBounds.min = new Vector3(transform.position.x - (minSpawningDistance / 3f),
			transform.position.y - (minSpawningDistance / 3f), transform.position.z);

		spawnBounds.max = new Vector3(transform.position.x + (minSpawningDistance / 3f),
			transform.position.y + (minSpawningDistance / 3f), transform.position.z);

		CreateEnemySpawnTable();
	}
	private void CreateEnemySpawnTable()
	{
		enemySpawnChanceTable.Clear();
		totalEnemySpawnChance = 0;

		foreach (SOEntityStats enemy in possibleEntityTypesToSpawn)
			enemySpawnChanceTable.Add(enemy.enemySpawnChance);

		foreach (float num in enemySpawnChanceTable)
			totalEnemySpawnChance += num;
	}
	private int GetIndexOfEnemyToSpawn()
	{
		float rand = Random.Range(0, totalEnemySpawnChance);
		float cumChance = 0;

		for (int i = 0; i < enemySpawnChanceTable.Count; i++)
		{
			cumChance += enemySpawnChanceTable[i];

			if (rand <= cumChance)
				return i;
		}
		return -1;
	}

	//event listeners
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

	private void CleanUpEntities()
	{
		if (listOfPlayersInRange.Count != 0) return;

		for (int i = listOfSpawnedEnemies.Count - 1; i >= 0; i--)
		{
			if (listOfSpawnedEnemies[i] == null) return;

			if (listOfSpawnedEnemies[i].GetComponent<EntityBehaviour>().playerTarget == null)
				DungeonHandler.Instance.AddNewEntitiesToPool(listOfSpawnedEnemies[i]);
			else
				listOfSpawnedEnemies[i].GetComponent<EntityBehaviour>().markedForCleanUp = true;

			listOfSpawnedEnemies.Remove(listOfSpawnedEnemies[i]);
		}
	}

	//entity spawning
	private void TrySpawnEntity()
	{
		if (listOfSpawnedEnemies.Count >= maxNumOfEnemiesToSpawn) return;
		if (listOfPlayersInRange.Count == 0) return;
		if (spawningDisabled) return;

		int num = GetIndexOfEnemyToSpawn();
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
		entity.gameObject.transform.position = Utilities.GetRandomPointInBounds(spawnBounds);
		entity.gameObject.SetActive(true);
		entity.ResetEntityStats();
		entity.ResetEntityBehaviour(this);
		listOfSpawnedEnemies.Add(entity);

		if (debugSpawnEnemiesAtSetLevel)
			entity.entityLevel = debugSpawnerLevel;
		else
			entity.entityLevel = spawnerLevel;

		TrySpawnEntity();
	}
	private void SpawnNewEntity()
	{
		int num = GetIndexOfEnemyToSpawn();
		GameObject go = Instantiate(enemyTemplatePrefab, Utilities.GetRandomPointInBounds(spawnBounds), transform.rotation);
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

	private void TrackClosestPlayerToSpawner()
	{
		if (listOfPlayersInRange.Count <= 0) return;
		float newClosestPlayerDistance = maxSpawningDistance;

		foreach (PlayerController player in listOfPlayersInRange)
		{
			float currentPlayerDistance = Vector2.Distance(player.transform.position, transform.position);

			if (newClosestPlayerDistance > currentPlayerDistance)
				newClosestPlayerDistance = currentPlayerDistance;
		}
		closestPlayerDistance = newClosestPlayerDistance;

		if (closestPlayerDistance <= minSpawningDistance)
			spawningDisabled = true;
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		//Gizmos.DrawWireSphere(transform.position, maxSpawningDistance);

		Gizmos.color = Color.red;
		//Gizmos.DrawWireSphere(transform.position, minSpawningDistance);

		Gizmos.color = Color.magenta;
		//Gizmos.DrawWireCube(spawnBounds.center, spawnBounds.size);
	}
}
