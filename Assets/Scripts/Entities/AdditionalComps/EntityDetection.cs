using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityDetection : MonoBehaviour
{
	//refs set in respected scripts on Initilize()#
	private PlayerController player;
	private EntityBehaviour entityBehaviour;

	//in Mp will need to be modified to handle multiple players

	private void Awake()
	{
		player = GetComponent<PlayerController>();
		entityBehaviour = GetComponent<EntityBehaviour>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (entityBehaviour != null) //detect players
		{
			if (other.GetComponent<PlayerController>() != null)
				entityBehaviour.AddPlayerToAggroList(other.GetComponent<PlayerController>(), 0);
		}
		else if (player != null) //detect others
		{
			if (other.GetComponent<EntityStats>() != null && other.GetComponent<PlayerController>() == null)
				player.AddNewEnemyTargetToList(other.GetComponent<EntityStats>());
		}
		else
			Debug.LogError("entity detection not set up correctly");
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (entityBehaviour != null) //detect players
		{
			if (other.GetComponent<PlayerController>() != null)
				entityBehaviour.RemovePlayerFromAggroList(other.GetComponent<PlayerController>());

			if (entityBehaviour.markedForCleanUp)
				ObjectPoolingManager.AddEntityToInActivePool(entityBehaviour.entityStats);
		}
		else if (player != null) //detect others
		{
			if (other.GetComponent<EntityStats>() != null && other.GetComponent<PlayerController>() == null)
				player.RemoveEnemyTargetFromList(other.GetComponent<EntityStats>());
		}
		else
			Debug.LogError("entity detection not set up correctly");
	}
}
