using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskGoblinBossPhase : TaskBossPhase
{
	/// <summary>
	/// Goblin boss behaviour for phase 1-3: 
	/// PHASE 1-3: basic chase and attack player
	/// </summary>

	public TaskGoblinBossPhase(BossEntityBehaviour behaviour)
	{
		this.behaviour = behaviour;
		stats = (BossEntityStats)behaviour.entityStats;
		equipmentHandler = behaviour.equipmentHandler;
	}

	public override NodeState Evaluate()
	{
		if (stats.inPhaseTransition == false)
			RunPhases();

		return NodeState.RUNNING;
	}

	protected override void RunPhases()
	{
		base.RunPhases();
	}

	protected override void PhaseOne()
	{
		KeepPlayerInMeleeRange(behaviour, equipmentHandler);
	}
	protected override void PhaseTwo()
	{
		KeepPlayerInMeleeRange(behaviour, equipmentHandler);
	}
	protected override void PhaseThree()
	{
		KeepPlayerInMeleeRange(behaviour, equipmentHandler);
	}
}
