using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CheckGlobalAttackCooldown : BTNode
{
	EntityBehaviour behaviour;

	public CheckGlobalAttackCooldown(EntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
	}

	public override NodeState Evaluate()
	{
		if (behaviour.globalAttackTimer >= 0)
			return NodeState.RUNNING;
		else return NodeState.SUCCESS;
	}
}
