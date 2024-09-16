using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityDetection : MonoBehaviour
{
	//refs set in respected scripts on Initilize()
	[HideInInspector] public TrapHandler trapHandler;
	[HideInInspector] public EntityBehaviour entityBehaviour;
	[HideInInspector] public PlayerController player;

	//in Mp will need to be modified to handle multiple players

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (trapHandler != null)
		{
			if (other.GetComponent<PlayerController>() != null)
				trapHandler.ActivateTrap(other.GetComponent<PlayerController>(), other);
		}
		else if (entityBehaviour != null) //detect players
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
		if (trapHandler != null)
			return;
		else if (entityBehaviour != null) //detect players
		{
			if (other.GetComponent<PlayerController>() != null)
				entityBehaviour.RemovePlayerFromAggroList(other.GetComponent<PlayerController>());

			if (entityBehaviour.markedForCleanUp)
				DungeonHandler.Instance.AddNewEntitiesToPool(entityBehaviour.entityStats);
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
