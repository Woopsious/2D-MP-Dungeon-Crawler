using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapActivationCollider : MonoBehaviour
{
	//refs set in respected scripts on Initilize()
	[HideInInspector] public TrapHandler trapHandler;

	//in Mp will need to be modified to handle multiple players

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<PlayerController>() != null)
			StartCoroutine(trapHandler.ActivateTrapDelay(other));
	}
}
