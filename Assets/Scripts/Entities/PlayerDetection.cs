using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
	//set in entitybehaviour script on start
	[HideInInspector] public EntityBehaviour entityBehaviourRef;

	//in Mp will need to be modified to handle multiple players
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.GetComponent<PlayerController>() == null) return;

		if (other.gameObject.GetComponent<PlayerController>())
			entityBehaviourRef.player = other.gameObject.GetComponent<PlayerController>();
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.GetComponent<PlayerController>() == null) return;

		if (other.gameObject.GetComponent<PlayerController>())
			entityBehaviourRef.player = null;

		if (entityBehaviourRef.markedForCleanUp)
			DungeonHandler.Instance.AddNewEntitiesToPool(entityBehaviourRef.entityStats);
	}
}
