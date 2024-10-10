using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;

public class TaskBossTransitionPhase : BTNode
{
	protected virtual void RunPhaseTransition(BossEntityStats stats)
	{
		Debug.LogError(stats.name + " boss phase Transition task");

		if (stats.bossPhase == BossEntityStats.BossPhase.firstPhase)
			PhaseOneTransition(stats);
		else if (stats.bossPhase == BossEntityStats.BossPhase.secondPhase)
			PhaseTwoTransition(stats);
		else if (stats.bossPhase == BossEntityStats.BossPhase.thirdPhase)
			PhaseThreeTransition(stats);
	}

	protected virtual void PhaseOneTransition(BossEntityStats stats)
	{
		stats.inPhaseTransition = false;
	}
	protected virtual void PhaseTwoTransition(BossEntityStats stats)
	{
		stats.inPhaseTransition = false;
	}
	protected virtual void PhaseThreeTransition(BossEntityStats stats)
	{
		stats.inPhaseTransition = false;
	}
}
