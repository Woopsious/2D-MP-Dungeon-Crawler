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

	private CircleCollider2D playerCollider;
	private List<EntityStats> listOfSpawnedEntities = new List<EntityStats>();
	private List<PlayerController> listOfPlayersInRange = new List<PlayerController>();
	private float closestPlayerDistance;
	private bool spawningDisabled;

	[Header("Spawner Range Settings")]
	public int maxSpawningDistance;
	public int minSpawningDistance;
	[HideInInspector] public Bounds spawnBounds;

	[Header("Spawner Settings")]
	public GameObject entityTemplatePrefab;
	public List<SOEntityStats> possibleEntityTypesToSpawn = new List<SOEntityStats>();

	[Header("Boss Spawner Settings")]
	public bool isBossRoomSpawner;
	public bool isBossSpawner;
	public GameObject bossEntityTemplatePrefab;
	public SOEntityStats bossEntityToSpawn;
	private BossEntityStats bossEntity;

	[Header("Boss Room Obstacles")]
	public GameObject obstaclePrefab;
	public List<GameObject> obstaclesList = new List<GameObject>();

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
		GameManager.OnSceneChangeFinish += TrySpawnEntities;

		BossEntityBehaviour.OnSpawnBossAdds += ForceSpawnEntitiesForBosses;
		BossEntityBehaviour.OnBossAbilityBeginCasting += SpawnBossDungeonObstacles;
	}
	private void OnDisable()
	{
		DungeonHandler.OnEntityDeathEvent -= OnEntityDeath;
		PlayerEventManager.OnPlayerLevelUpEvent -= UpdateSpawnerLevel;
		GameManager.OnSceneChangeFinish -= TrySpawnEntities;

		BossEntityBehaviour.OnSpawnBossAdds -= ForceSpawnEntitiesForBosses;
		BossEntityBehaviour.OnBossAbilityBeginCasting -= SpawnBossDungeonObstacles;

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

		TrySpawnEntities();
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

		CreateEnemySpawnTable();
		TrySpawnEntities();
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
		if (obj.GetComponent<EntityStats>().statsRef.isBossVersion) //boss entity deaths, unsub to event and set to null
		{
			if (bossEntity == null) return; //occasionally called multiple times so ignore if null
			bossEntity = null;
			return;
		}

		if (listOfSpawnedEntities.Contains(obj.GetComponent<EntityStats>())) //entity deaths
		{
			listOfSpawnedEntities.Remove(obj.GetComponent<EntityStats>());
			TrySpawnEntities();

			if (isBossRoomSpawner) //decrease spawns on entity death for boss room spawners
				maxNumOfEntitiesToSpawn--;
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
	public void ForceSpawnEntitiesForBosses(int numToSpawn) //called via health change event in bosses
	{
		if (!isBossSpawner) return;

		for (int i = 0; i < numToSpawn; i++)
			SpawnEntity();
	}
	private void TrySpawnEntities()
	{
		if (isBossSpawner)
		{
			SpawnBossEntity();
			return;
		}

		if (listOfSpawnedEntities.Count >= maxNumOfEntitiesToSpawn) return;
		if (listOfPlayersInRange.Count == 0) return;
		if (spawningDisabled) return;

		SpawnEntity();
	}
	//boss entity spawning
	private void SpawnBossEntity()
	{
		if (bossEntity != null) return; //disable spawning multiple

		SOEntityStats bossToSpawn = null;

		if (GameManager.Instance != null) //get ref from dungeonData in regular build
			bossToSpawn = GameManager.Instance.currentDungeonData.bossToSpawn;
		else if (bossEntityToSpawn != null) //check if ref was set in Unity Inspector
		{
			bossToSpawn = bossEntityToSpawn;
			Debug.LogWarning("bossEntityToSpawn reference found for spawner, ignore if testing");
		}
		else
			Debug.LogError("bossEntityToSpawn reference null, add reference if testing");


		if (bossToSpawn == null) return;
		InstantiateNewBossEntity(bossToSpawn);
	}
	private void InstantiateNewBossEntity(SOEntityStats bossToSpawn)
	{
		GameObject go = Instantiate(bossEntityTemplatePrefab, Utilities.GetRandomPointInBounds(spawnBounds), transform.rotation);
		BossEntityStats bossEntity = go.GetComponent<BossEntityStats>();
		bossEntity.statsRef = bossToSpawn;
		this.bossEntity = bossEntity;

		if (debugSpawnEnemiesAtSetLevel)
			bossEntity.entityLevel = debugSpawnerLevel;
		else
			bossEntity.entityLevel = spawnerLevel;
	}
	//entity spawning
	private void SpawnEntity()
	{
		int num = GetIndexOfEnemyToSpawn();
		bool entityTypeMatches = false;

		foreach (EntityStats entity in DungeonHandler.Instance.inActiveEntityPool)
		{
			if (entity.statsRef == possibleEntityTypesToSpawn[num])
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

		TrySpawnEntities();
	}
	private void InstantiateNewEntity()
	{
		int num = GetIndexOfEnemyToSpawn();
		GameObject go = Instantiate(entityTemplatePrefab, Utilities.GetRandomPointInBounds(spawnBounds), transform.rotation);
		EntityStats entity = go.GetComponent<EntityStats>();
		entity.statsRef = possibleEntityTypesToSpawn[num];
		entity.GetComponent<EntityBehaviour>().behaviourRef = possibleEntityTypesToSpawn[num].entityBehaviour;
		listOfSpawnedEntities.Add(entity);

		if (debugSpawnEnemiesAtSetLevel)
			entity.entityLevel = debugSpawnerLevel;
		else
			entity.entityLevel = spawnerLevel;

		TrySpawnEntities();
	}

	//checks
	private bool CheckIfSpawnerShouldRespawnEnemies()
	{
		if (!isBossRoomSpawner) return true;
		if (maxNumOfEntitiesToSpawn > 0)
			return true; 
		else return false;
	}

	//SPAWNING OF BOSS ROOM OBSTACLES
	private void SpawnBossDungeonObstacles(SOClassAbilities ability, List<Vector2> positionList, Vector2 adjustPosition, float radius)
	{
		if (!isBossSpawner) return;
		for (int i = 0; i < positionList.Count; i++)
		{
			Vector2 spawnPosition = (positionList[i] * radius) + adjustPosition;
			GameObject go = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
			Obstacles bossRoomObstacle = go.GetComponent<Obstacles>();
			bossRoomObstacle.InitilizeBossRoomObstacle(ability.abilityCastingTimer);
		}
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
