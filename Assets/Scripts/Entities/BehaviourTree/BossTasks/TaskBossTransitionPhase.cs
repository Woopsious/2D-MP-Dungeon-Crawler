using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;

public class TaskBossTransitionPhase : BTNode
{
	protected BossEntityBehaviour behaviour;
	protected BossEntityStats stats;
	protected EntityEquipmentHandler equipmentHandler;

	protected virtual void RunPhaseTransition()
	{
		Debug.Log(stats.name + " boss phase Transition task");

		if (stats.bossPhase == BossEntityStats.BossPhase.firstPhase)
			PhaseOneTransition();
		else if (stats.bossPhase == BossEntityStats.BossPhase.secondPhase)
			PhaseTwoTransition();
		else if (stats.bossPhase == BossEntityStats.BossPhase.thirdPhase)
			PhaseThreeTransition();
	}

	protected virtual void PhaseOneTransition()
	{
		stats.inPhaseTransition = false;
	}
	protected virtual void PhaseTwoTransition()
	{
		stats.inPhaseTransition = false;
	}
	protected virtual void PhaseThreeTransition()
	{
		stats.inPhaseTransition = false;
	}
}
