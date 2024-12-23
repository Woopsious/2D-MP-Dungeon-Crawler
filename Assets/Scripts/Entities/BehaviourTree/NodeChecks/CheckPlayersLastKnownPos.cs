using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayersLastKnownPos : BTNode
{
	EntityBehaviour behaviour;

	public CheckPlayersLastKnownPos(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
	}

	public override NodeState Evaluate()
	{
		if (behaviour.playerTarget != null && behaviour.playersLastKnownPosition == new Vector2 (0, 0))
			return NodeState.FAILURE;
		else
			return NodeState.SUCCESS;
	}
}
