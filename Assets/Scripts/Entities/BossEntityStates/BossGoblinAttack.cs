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
			PhaseOneBossBehaviour((BossEntityBehaviour)entity);
		else if (entityStats.bossPhase == BossEntityStats.BossPhase.secondPhase)
			PhaseTwoBossBehaviour((BossEntityBehaviour)entity);
		else if (entityStats.bossPhase == BossEntityStats.BossPhase.thirdPhase)
			PhaseThreeBossBehaviour((BossEntityBehaviour)entity);
	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{
		base.UpdatePhysics(entity);
	}

	private void PhaseOneBossBehaviour(BossEntityBehaviour entity)
	{
		//Debug.Log("First Phase");

		AttackBehaviourLogic(entity); //attack behaviour same as regular entities
		TryCastFirstAbility(entity);
	}
	private void PhaseTwoBossBehaviour(BossEntityBehaviour entity)
	{
		//Debug.Log("Second Phase");

		AttackBehaviourLogic(entity); //attack behaviour same as regular entities
		TryCastFirstAbility(entity);
		TryCastSecondAbility(entity);
	}
	private void PhaseThreeBossBehaviour(BossEntityBehaviour entity)
	{
		//Debug.Log("Third Phase");
	}

	//unique boss abilities
	public void TryCastFirstAbility(BossEntityBehaviour entity)
	{
		if (entity.abilityOne == null || !entity.canCastAbilityOne) return;
		if (entity.playerTarget == null) return;

		if (!entity.HasEnoughManaToCast(entity.abilityOne))
		{
			entity.canCastAbilityOne = false;
			entity.abilityTimerOneCounter = 2.5f;   //if low mana wait 2.5s then try again
			return;
		}

		if (entity.offensiveAbility.hasStatusEffects)
			entity.CastEffect(entity.abilityOne);
		else
		{
			if (entity.abilityOne.isProjectile)
				entity.CastDirectionalAbility(entity.abilityOne);
			else if (entity.abilityOne.isAOE)
				entity.CastAoeAbility(entity.abilityOne);
		}

		entity.canCastAbilityOne = false;
		entity.abilityTimerOneCounter = entity.abilityOne.abilityCooldown;
		return;
	}
	public void TryCastSecondAbility(BossEntityBehaviour entity)
	{
		if (entity.abilityTwo == null || !entity.canCastAbilityTwo) return;
		if (entity.playerTarget == null) return;

		if (!entity.HasEnoughManaToCast(entity.abilityTwo))
		{
			entity.canCastAbilityTwo = false;
			entity.abilityTimerTwoCounter = 2.5f;   //if low mana wait 2.5s then try again
			return;
		}

		if (entity.offensiveAbility.hasStatusEffects)
			entity.CastEffect(entity.abilityTwo);
		else
		{
			if (entity.abilityTwo.isProjectile)
				entity.CastDirectionalAbility(entity.abilityTwo);
			else if (entity.abilityTwo.isAOE)
				entity.CastAoeAbility(entity.abilityTwo);
		}

		entity.canCastAbilityTwo = false;
		entity.abilityTimerTwoCounter = entity.abilityTwo.abilityCooldown;
		return;
	}
}
