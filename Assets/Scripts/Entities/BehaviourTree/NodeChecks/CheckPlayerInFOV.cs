using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayerInFOV : BTNode
{
	EntityBehaviour behaviour;
	float playerDetectionTimer;
	float playerDetectionCooldown;

	public CheckPlayerInFOV(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
	}

	public override NodeState Evaluate()
	{
		playerDetectionTimer -= Time.deltaTime;

		if (playerDetectionTimer <= 0)
		{
			playerDetectionTimer = playerDetectionCooldown;

			//check if player behind obstacles. check if player within aggro/chase ranges based on currentState
			if (PlayerTargetVisible(behaviour.playerTarget))
			{
				behaviour.playersLastKnownPosition = behaviour.playerTarget.transform.position;
				return NodeState.SUCCESS;
			}
			else
			{
				return NodeState.FAILURE;
			}
		}
		return NodeState.RUNNING;
	}

	private bool PlayerTargetVisible(PlayerController player)
	{
		if (player == null) return false; //check null
		RaycastHit2D hit = Physics2D.Linecast(behaviour.transform.position, player.transform.position, behaviour.includeMe);

		if (hit.point == null || hit.collider.GetComponent<PlayerController>() == null) //check LoS
			return false;

		//check aggro range
		if (Vector2.Distance(behaviour.transform.position, player.transform.position) > behaviour.behaviourRef.aggroRange)
			return false;

		return true;
	}
}
