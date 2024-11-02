using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskEyeBossPhases : EntityMovement, IBossPhases
{
	/// <summary> Eye boss behaviour for phase 1-3: 
	/// PHASE 1: 
	/// stay on center platfrom using ranged attacks + ability 1
	/// 
	/// PHASE 2: 
	/// stay on center platfrom using ranged attacks + ability 1 and ability 2 via Marked Eye special ability
	/// 
	/// PHASE 3: 
	/// stay on center platfrom using ranged attacks + ability 1 and ability 2, 3 via Marked Eye special ability
	/// </summary>

	readonly BossEntityBehaviour behaviour;
	readonly BossEntityStats stats;
	readonly EntityEquipmentHandler equipmentHandler;

	public TaskEyeBossPhases(BossEntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		stats = (BossEntityStats)behaviour.entityStats;
		equipmentHandler = behaviour.equipmentHandler;
	}

	public override NodeState Evaluate()
	{
		if (stats.inPhaseTransition != false) return NodeState.RUNNING;

		Debug.Log(stats.name + " boss phase task");

		if (stats.bossPhase == BossEntityStats.BossPhase.firstPhase)
			PhaseOne();
		else if (stats.bossPhase == BossEntityStats.BossPhase.secondPhase)
			PhaseTwo();
		else if (stats.bossPhase == BossEntityStats.BossPhase.thirdPhase)
			PhaseThree();

		return NodeState.RUNNING;
	}

	public void PhaseOne()
	{
		MoveToCenterPiece(behaviour);
	}

	public void PhaseThree()
	{
		MoveToCenterPiece(behaviour);
	}

	public void PhaseTwo()
	{
		MoveToCenterPiece(behaviour);
	}
}
