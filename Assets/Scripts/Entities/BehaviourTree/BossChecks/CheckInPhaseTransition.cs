using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CheckInPhaseTransition : BTNode
{
	BossEntityStats stats;
	BossEntityBehaviour behaviour;
	NavMeshAgent navMesh;

	public CheckInPhaseTransition(BossEntityStats entity)
	{
		this.stats = entity;
		behaviour = (BossEntityBehaviour)entity.entityBehaviour;
		navMesh = entity.entityBehaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		return NodeState.FAILURE;
	}
}
