using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskEyeBossAbilities : BTNode, IBossAbilities
{
	/// <summary>
	///
	/// </summary>

	readonly BossEntityBehaviour behaviour;
	readonly BossEntityStats stats;

	public TaskEyeBossAbilities(BossEntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		stats = (BossEntityStats)behaviour.entityStats;
	}

	public override NodeState Evaluate()
	{
		//return failure to force switch back to attack with main weapon
		if (behaviour.globalAttackTimer > 0) return NodeState.FAILURE;
		else
		{
			if (CanUseBossAbilityOne())
			{
				behaviour.CastBossAbilityOne();
			}
			else if (CanUseBossAbilityTwo())
			{
				behaviour.CastBossAbilityTwo();
			}
			else if (CanUseBossAbilityThree())
			{
				behaviour.CastBossAbilityThree();
			}
			else return NodeState.FAILURE;

			//Debug.LogError(stats.name + " using an ability");

			//add ability animation length here if needed, include a bool if animation should block movement
			behaviour.globalAttackTimer = 1f;
			return NodeState.SUCCESS;
		}
	}

	public bool CanUseBossAbilityOne()
	{
		throw new System.NotImplementedException();
	}

	public bool CanUseBossAbilityThree()
	{
		throw new System.NotImplementedException();
	}

	public bool CanUseBossAbilityTwo()
	{
		throw new System.NotImplementedException();
	}
}
