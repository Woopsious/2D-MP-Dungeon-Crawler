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
	public int maxNumOfEntitiesToSpawn;
	public int maxNumOfBossesToSpawn;
	private List<EntityStats> listOfSpawnedEntities = new List<EntityStats>();

	[Header("Spawner Range Settings")]
	public int maxSpawningDistance;
	public int minSpawningDistance;
	[HideInInspector] public Bounds spawnBounds;

	[Header("Boss Spawner Settings")]
	public bool isBossRoomSpawner;
	public bool isBossSpawner;
	private int numOfSpawnedBosses;
	public GameObject bossEntityTemplatePrefab;
	public SOEntityStats bossEntityToSpawn;

	[Header("Spawner Entity Types")]
	public GameObject entityTemplatePrefab;
	public List<SOEntityStats> possibleEntityTypesToSpawn = new List<SOEntityStats>();

	private CircleCollider2D playerCollider;
	private List<PlayerController> listOfPlayersInRange = new List<PlayerController>();
	private float closestPlayerDistance;
	private bool spawningDisabled;

	[Header("Spawn Table")]
	private float totalEnemySpawnChance;
	private List<float> enemySpawnChanceTable = new List<float>();

	private void Awake()
	{
		Initilize();
	}
	private void OnEnable()
	{
		DungeonHandler.OnEntityDeathEvent += OnEntityDeath;
		PlayerEventManager.OnPlayerLevelUpEvent += UpdateSpawnerLevel;
		GameManager.OnSceneChangeFinish += TrySpawnEntity;

		TrySpawnEntity();
	}
	private void OnDisable()
	{
		DungeonHandler.OnEntityDeathEvent -= OnEntityDeath;
		PlayerEventManager.OnPlayerLevelUpEvent -= UpdateSpawnerLevel;
		GameManager.OnSceneChangeFinish -= TrySpawnEntity;

		enemySpawnChanceTable.Clear();
		totalEnemySpawnChance = 0;
		StopAllCoroutines();
	}

	//track players
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (listOfPlayersInRange.Count <= 0 && CheckIfSpawnerShouldRespawnEnemies())
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

		if (!isBossRoomSpawner)
			CleanUpEntities();

		if (CheckIfSpawnerShouldRespawnEnemies())
			spawningDisabled = false;
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

		if (isBossSpawner)
			maxNumOfEntitiesToSpawn = 2;

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
	private void UpdateSpawnerLevel(EntityStats playerStats)
	{
		spawnerLevel = playerStats.entityLevel;
	}
	private void OnEntityDeath(GameObject obj)
	{
		if (listOfSpawnedEntities.Contains(obj.GetComponent<EntityStats>()))
		{
			listOfSpawnedEntities.Remove(obj.GetComponent<EntityStats>());
			TrySpawnEntity();
			if (isBossRoomSpawner) //decrease spawns on enemy death
			{
				if (obj.GetComponent<EntityStats>().entityBaseStats.isBossVersion)
					maxNumOfBossesToSpawn--;
				else
					maxNumOfEntitiesToSpawn--;
			}
		}
	}

	private void CleanUpEntities()
	{
		if (listOfPlayersInRange.Count != 0) return;

		for (int i = listOfSpawnedEntities.Count - 1; i >= 0; i--)
		{
			if (listOfSpawnedEntities[i] == null) return;

			if (listOfSpawnedEntities[i].GetComponent<EntityBehaviour>().playerTarget == null)
				DungeonHandler.Instance.AddNewEntitiesToPool(listOfSpawnedEntities[i]);
			else
				listOfSpawnedEntities[i].GetComponent<EntityBehaviour>().markedForCleanUp = true;

			listOfSpawnedEntities.Remove(listOfSpawnedEntities[i]);
		}
	}

	//SPAWNING OF ENTITIES AND BOSSES
	private void TrySpawnEntity()
	{
		if (listOfSpawnedEntities.Count >= maxNumOfEntitiesToSpawn) return;
		if (listOfPlayersInRange.Count == 0) return;
		if (spawningDisabled) return;

		SpawnEntity();
		if (isBossSpawner)
			SpawnBossEntity();
	}
	//entity spawning
	private void SpawnEntity()
	{
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
			InstantiateNewEntity();
	}
	private void RespawnInActiveEntity(EntityStats entity)
	{
		entity.gameObject.transform.position = Utilities.GetRandomPointInBounds(spawnBounds);
		entity.gameObject.SetActive(true);
		entity.ResetEntityStats();
		entity.ResetEntityBehaviour(this);
		listOfSpawnedEntities.Add(entity);

		if (debugSpawnEnemiesAtSetLevel)
			entity.entityLevel = debugSpawnerLevel;
		else
			entity.entityLevel = spawnerLevel;

		TrySpawnEntity();
	}
	private void InstantiateNewEntity()
	{
		int num = GetIndexOfEnemyToSpawn();
		GameObject go = Instantiate(entityTemplatePrefab, Utilities.GetRandomPointInBounds(spawnBounds), transform.rotation);
		EntityStats entity = go.GetComponent<EntityStats>();
		entity.entityBaseStats = possibleEntityTypesToSpawn[num];
		entity.GetComponent<EntityBehaviour>().entityBehaviour = possibleEntityTypesToSpawn[num].entityBehaviour;
		listOfSpawnedEntities.Add(entity);

		if (debugSpawnEnemiesAtSetLevel)
			entity.entityLevel = debugSpawnerLevel;
		else
			entity.entityLevel = spawnerLevel;

		TrySpawnEntity();
	}
	//boss entity spawning
	private void SpawnBossEntity()
	{
		//spawn the boss entity just like regular entities after grabbing its SO from GameManager.GameData.dungeonData etc..
		//grab its entityBossStats script etc and sub to its health events to force spawn eneimes when health gets to 75-50-25 %

		SOEntityStats bossToSpawn = null;

		if (GameManager.Instance != null) //get ref from dungeonData in regular build
		{
			bossToSpawn = GameManager.Instance.currentDungeonData.bossToSpawn;
			Debug.Log("getting ref of boss from GM:" + GameManager.Instance.currentDungeonData.bossToSpawn);
		}
		else if (bossEntityToSpawn != null) //check if ref was set in Unity Inspector
		{
			bossToSpawn = bossEntityToSpawn;
			Debug.LogWarning("bossEntityToSpawn reference found for spawner, ignore if testing");
		}
		else
			Debug.LogError("bossEntityToSpawn reference null, add reference if testing");

		Debug.Log("boss to spawn ref:" + bossToSpawn);

		if (numOfSpawnedBosses >= maxNumOfBossesToSpawn || bossToSpawn == null) return;
		InstantiateNewBossEntity(bossToSpawn);
	}
	private void InstantiateNewBossEntity(SOEntityStats bossToSpawn)
	{
		Debug.Log("Spawning boss entity");

		GameObject go = Instantiate(bossEntityTemplatePrefab, Utilities.GetRandomPointInBounds(spawnBounds), transform.rotation);
		BossEntityStats bossEntity = go.GetComponent<BossEntityStats>();
		bossEntity.entityBaseStats = bossToSpawn;
		bossEntity.GetComponent<BossEntityBehaviour>().entityBehaviour = bossEntityToSpawn.entityBehaviour;
		numOfSpawnedBosses++;

		if (debugSpawnEnemiesAtSetLevel)
			bossEntity.entityLevel = debugSpawnerLevel;
		else
			bossEntity.entityLevel = spawnerLevel;
	}

	private bool CheckIfSpawnerShouldRespawnEnemies()
	{
		if (!isBossRoomSpawner) return true;
		if (maxNumOfEntitiesToSpawn > 0)
			return true; 
		else return false;
	}

	//track closest player
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
