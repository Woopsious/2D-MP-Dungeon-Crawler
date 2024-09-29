using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGoblinAttack : EnemyAttackState
{
	/// <summary>
	/// boss will spawn in along with a center piece
	/// PHASE 1: simple chase player and melee attack, limited to casting 1st ability
	/// PHASE 1-2 transition: become invulnerable. return to center piece, slowly heal (limited to current health(max 66%) to 80% of health)
	/// only become vulnerable once player(s) kill the 2 enemies that spawn in (boss stays idle till enemies killed)
	/// PHASE 2: same as phase one but has access to 2nd ability
	/// PHASE 2-3 transition: enter "enrage" state, healing 15% oh health and increasing move speed.
	/// PHASE 3: stay in "enrage" state and gain access to 3rd ability
	/// </summary>

	BossEntityStats entityStats;
	BossEntityBehaviour entityBehaviour;

	public override void Enter(EntityBehaviour entity)
	{
		base.Enter(entity);
		entityStats = (BossEntityStats)entity.entityStats;
		entityBehaviour = (BossEntityBehaviour)entity;
	}
	public override void Exit(EntityBehaviour entity)
	{
		base.Exit(entity);
	}
	public override void UpdateLogic(EntityBehaviour entity)
	{
		if (entityStats.bossPhase == BossEntityStats.BossPhase.firstPhase)
			PhaseOneBossBehaviour(entity);
		else if (entityStats.bossPhase == BossEntityStats.BossPhase.secondPhase)
			PhaseTwoBossBehaviour(entity);
		else if (entityStats.bossPhase == BossEntityStats.BossPhase.thirdPhase)
			PhaseThreeBossBehaviour(entity);
	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{
		base.UpdatePhysics(entity);
	}

	private void PhaseOneBossBehaviour(EntityBehaviour entity)
	{
		Debug.Log("First Phase");
	}
	private void PhaseTwoBossBehaviour(EntityBehaviour entity)
	{
		Debug.Log("Second Phase");
	}
	private void PhaseThreeBossBehaviour(EntityBehaviour entity)
	{
		Debug.Log("Third Phase");
	}
}
