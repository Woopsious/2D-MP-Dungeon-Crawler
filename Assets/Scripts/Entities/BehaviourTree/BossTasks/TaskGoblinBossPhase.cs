using UnityEngine;

public class TaskGoblinBossPhase : EntityMovement, IBossPhases
{
	/// <summary>
	/// Goblin boss behaviour for phase 1-3: 
	/// PHASE 1-3: basic chase and attack player
	/// </summary>

	readonly BossEntityBehaviour behaviour;
	readonly BossEntityStats stats;
	readonly EntityEquipmentHandler equipmentHandler;

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

	public void RunPhases()
	{
		Debug.Log(stats.name + " boss phase task");

		if (stats.bossPhase == BossEntityStats.BossPhase.firstPhase)
			PhaseOne();
		else if (stats.bossPhase == BossEntityStats.BossPhase.secondPhase)
			PhaseTwo();
		else if (stats.bossPhase == BossEntityStats.BossPhase.thirdPhase)
			PhaseThree();
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
