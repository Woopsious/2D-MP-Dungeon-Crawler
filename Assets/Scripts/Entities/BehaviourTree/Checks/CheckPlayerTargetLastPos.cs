using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayerTargetLastPos : BTNode
{
	EntityBehaviour behaviour;

	public CheckPlayerTargetLastPos(EntityStats entity)
	{
		behaviour = entity.entityBehaviour;
	}

	public override NodeState Evaluate()
	{
		if (behaviour.playersLastKnownPosition == new Vector2 (0, 0))
		{
			return state = NodeState.FAILURE;
		}
		else
		{
			return state = NodeState.SUCCESS;
		}
	}
}
