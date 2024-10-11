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

	BossEntityStats stats;
	BossEntityBehaviour behaviour;
	EntityEquipmentHandler equipmentHandler;

	public TaskGoblinBossPhase(BossEntityStats entity)
	{
		stats = entity;
		behaviour = (BossEntityBehaviour)entity.entityBehaviour;
		equipmentHandler = entity.equipmentHandler;
	}

	public override NodeState Evaluate()
	{
		if (stats.inPhaseTransition == false)
			RunPhases(stats);

		return NodeState.RUNNING;
	}

	protected override void RunPhases(BossEntityStats stats)
	{
		base.RunPhases(stats);
	}

	protected override void PhaseOne(BossEntityStats stats)
	{
		KeepPlayerInMeleeRange(behaviour, equipmentHandler);
	}
	protected override void PhaseTwo(BossEntityStats stats)
	{
		KeepPlayerInMeleeRange(behaviour, equipmentHandler);
	}
	protected override void PhaseThree(BossEntityStats stats)
	{
		KeepPlayerInMeleeRange(behaviour, equipmentHandler);
	}
}
