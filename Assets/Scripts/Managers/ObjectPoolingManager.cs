using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectPoolingManager : MonoBehaviour
{
	public static ObjectPoolingManager Instance;

	public List<EntityStats> entityPool = new List<EntityStats>();
	public List<EntityStats> inActiveEntityPool = new List<EntityStats>();

	public List<Projectiles> projectilesPool = new List<Projectiles>();
	public List<Projectiles> inActiveProjectilesPool = new List<Projectiles>();

	public List<AbilityAOE> abilityAoePool = new List<AbilityAOE>();
	public List<AbilityAOE> inActiveAoeAbilitesPool = new List<AbilityAOE>();

	public List<Items> droppedItemsPool = new List<Items>();

	public static event Action<GameObject> OnEntityDeathEvent;
	//public static event Action OnEntitySpawnEvent;

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += CleanUpScenesOnLoadScene;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= CleanUpScenesOnLoadScene;
	}

	private void CleanUpScenesOnLoadScene(Scene newLoadedScene, LoadSceneMode mode)
	{
		if (newLoadedScene.name != GameManager.Instance.hubScene) return;

		//deactivate entities, projectiles and ability aoes, destroy instead if more then 10

		for (int i = entityPool.Count - 1; i >= 0; i--)
		{
			if (entityPool[i].gameObject.activeInHierarchy)
				AddEntityToInActivePool(entityPool[i]);
		}

		for (int i = projectilesPool.Count - 1; i >= 0; i--)
		{
			if (projectilesPool[i].gameObject.activeInHierarchy)
				AddProjectileToInActivePool(projectilesPool[i]);
		}

		for (int i = abilityAoePool.Count - 1; i >= 0; i--)
		{
			if (abilityAoePool[i].gameObject.activeInHierarchy)
				AddAoeAbilityToInActivePool(abilityAoePool[i]);
		}

		//destroy any items not yet picked up
		for (int i = droppedItemsPool.Count - 1;  i >= 0; i--)
		{
			if (droppedItemsPool[i] == null)
				continue;
			else
				Destroy(droppedItemsPool[i].gameObject);
		}
		droppedItemsPool.Clear();
	}

	//entity death event
	public static void EntityDeathEvent(GameObject gameObject)
	{
		OnEntityDeathEvent?.Invoke(gameObject);
		Instance.OnEntityDeath(gameObject);
	}

	//OBJECT POOLING
	//entity obj pooling
	public static void AddEntityToObjectPooling(EntityStats entity)
	{
		if (Instance.entityPool.Contains(entity)) return;
		Instance.entityPool.Add(entity);
	}
	public static void AddEntityToInActivePool(EntityStats entity)
	{
		if (Instance.inActiveEntityPool.Contains(entity)) return;
		entity.gameObject.SetActive(false);
		entity.transform.position = Vector3.zero;
		Instance.inActiveEntityPool.Add(entity);
	}
	private void OnEntityDeath(GameObject obj)
	{
		obj.SetActive(false);
		EntityStats entityStats = obj.GetComponent<EntityStats>();
		inActiveEntityPool.Add(entityStats);
	}

	public static EntityStats GetInActiveEntity(SOEntityStats stats)
	{
		foreach (EntityStats entity in Instance.inActiveEntityPool)
		{
			if (entity.statsRef == stats)
			{
				Instance.inActiveEntityPool.Remove(entity);
				return entity;
			}
		}
		return null;
	}

	//projectile obj pooling
	public static void AddProjectileToObjectPooling(Projectiles projectile)
	{
		if (Instance.projectilesPool.Contains(projectile)) return;
		Instance.projectilesPool.Add(projectile);
	}
	public static Projectiles GetInActiveProjectile()
	{
		if (Instance.inActiveProjectilesPool.Count != 0)
		{
			Projectiles projectile = Instance.inActiveProjectilesPool[0];
			Instance.inActiveProjectilesPool.RemoveAt(0);
			return projectile;
		}
		else return null;
	}
	public static void AddProjectileToInActivePool(Projectiles projectile)
	{
		if (Instance.inActiveProjectilesPool.Contains(projectile)) return;
		projectile.gameObject.SetActive(false);
		projectile.transform.position = Vector3.zero;
		Instance.inActiveProjectilesPool.Add(projectile);
	}

	//aoe obj pooling
	public static void AddAoeAbilityToObjectPooling(AbilityAOE abilityAOE)
	{
		if (Instance.abilityAoePool.Contains(abilityAOE)) return;
		Instance.abilityAoePool.Add(abilityAOE);
	}
	public static AbilityAOE GetInActiveAoeAbility()
	{
		if (Instance.inActiveAoeAbilitesPool.Count != 0)
		{
			AbilityAOE abilityAOE = Instance.inActiveAoeAbilitesPool[0];
			Instance.inActiveAoeAbilitesPool.RemoveAt(0);
			return abilityAOE;
		}
		else return null;
	}
	public static void AddAoeAbilityToInActivePool(AbilityAOE abilityAOE)
	{
		if (Instance.inActiveAoeAbilitesPool.Contains(abilityAOE)) return;
		abilityAOE.gameObject.SetActive(false);
		abilityAOE.transform.position = Vector3.zero;
		Instance.inActiveAoeAbilitesPool.Add(abilityAOE);
	}

	//item obj pooling
	public static void AddItemsToObjectPooling(Items item)
	{
		if (Instance.droppedItemsPool.Contains(item)) return;
		Instance.droppedItemsPool.Add(item);
	}
}
