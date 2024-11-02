using UnityEngine;

public class TaskGoblinBossPhases : EntityMovement, IBossPhases
{
	/// <summary> Goblin boss behaviour for phase 1-3: 
	/// PHASE 1: 
	/// basic chase and attack player + ability 1
	/// 
	/// PHASE 2: 
	/// basic chase and attack player + ability 1 & 2
	/// 
	/// PHASE 3: 
	/// basic chase and attack player + ability 1, 2 & 3
	/// </summary>

	readonly BossEntityBehaviour behaviour;
	readonly BossEntityStats stats;
	readonly EntityEquipmentHandler equipmentHandler;

	public TaskGoblinBossPhases(BossEntityBehaviour behaviour)
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
		KeepPlayerInMeleeRange(behaviour, equipmentHandler);
	}

	public void PhaseTwo()
	{
		KeepPlayerInMeleeRange(behaviour, equipmentHandler);
	}

	public void PhaseThree()
	{
		KeepPlayerInMeleeRange(behaviour, equipmentHandler);
	}
}
