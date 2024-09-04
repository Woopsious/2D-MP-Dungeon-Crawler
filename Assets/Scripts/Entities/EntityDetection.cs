using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityDetection : MonoBehaviour
{
	//refs set in respected scripts on Initilize()
	[HideInInspector] public EntityBehaviour entityBehaviourRef;
	[HideInInspector] public PlayerController player;

	//in Mp will need to be modified to handle multiple players

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (entityBehaviourRef != null) //detect players
		{
			if (other.gameObject.GetComponent<PlayerController>() != null)
				entityBehaviourRef.AddPlayerToAggroList(other.gameObject.GetComponent<PlayerController>(), 0);
		}
		else if (player != null) //detect others
		{
			if (other.gameObject.GetComponent<EntityStats>() != null && other.gameObject.GetComponent<PlayerController>() == null)
				player.AddNewEnemyTargetToList(other.gameObject.GetComponent<EntityStats>());
		}
		else
			Debug.LogError("entity detection not set up correctly");
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (entityBehaviourRef != null) //detect players
		{
			if (other.gameObject.GetComponent<PlayerController>() != null)
				entityBehaviourRef.RemovePlayerFromAggroList(other.gameObject.GetComponent<PlayerController>());

			if (entityBehaviourRef.markedForCleanUp)
				DungeonHandler.Instance.AddNewEntitiesToPool(entityBehaviourRef.entityStats);
		}
		else if (player != null) //detect others
		{
			if (other.gameObject.GetComponent<EntityStats>() != null && other.gameObject.GetComponent<PlayerController>() == null)
				player.RemoveEnemyTargetFromList(other.gameObject.GetComponent<EntityStats>());
		}
		else
			Debug.LogError("entity detection not set up correctly");
	}
}
