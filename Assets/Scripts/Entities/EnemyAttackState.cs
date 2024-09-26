using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EnemyAttackState : EnemyBaseState
{
	Weapons equippedWeapon;
	bool idleInWeaponRange;

	public override void Enter(EntityBehaviour entity)
	{
		equippedWeapon = entity.entityStats.equipmentHandler.equippedWeapon;
	}
	public override void Exit(EntityBehaviour entity)
	{

	}
	public override void UpdateLogic(EntityBehaviour entity)
	{
		if (entity.entityStats.entityBaseStats.isBossVersion)
			BossAttackBehaviourLogic((BossEntityBehaviour)entity);
		else
			AttackBehaviourLogic(entity);
	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{
		UpdatePlayerPosition(entity);
		AttackBehaviourPhysics(entity);
	}

	private void UpdatePlayerPosition(EntityBehaviour entity)
	{
		if (entity.playerTarget != null && entity.currentPlayerTargetInView)
			entity.playersLastKnownPosition = entity.playerTarget.transform.position;
	}
	
	//attack behaviour logic
	private void AttackBehaviourLogic(EntityBehaviour entity)
	{
		entity.AttackWithMainWeapon();

		entity.CastOffensiveAbility();
	}

	private void BossAttackBehaviourLogic(BossEntityBehaviour bossEntity)
	{
		bossEntity.AttackWithMainWeapon();

		bossEntity.CastOffensiveAbility();
		bossEntity.CastBossAbilityOne();
		bossEntity.CastBossAbilityTwo();
		bossEntity.CastBossAbilityThree();
	}

	//attack behaviour physics
	private void AttackBehaviourPhysics(EntityBehaviour entity)
	{
		if (!entity.currentPlayerTargetInView)
			entity.ChangeStateIdle();
		else
		{
			if (CheckDistanceToPlayerIsBigger(entity, entity.entityBehaviour.maxChaseRange)) //de aggro
				entity.ChangeStateIdle();

			if (equippedWeapon == null) return;
			if (equippedWeapon.weaponBaseRef.isRangedWeapon)
				KeepDistanceFromPlayer(entity);
			else
			{
				if (entity.CheckDistanceToDestination())
					entity.SetNewDestination(entity.playersLastKnownPosition);
			}
		}
	}
	private void KeepDistanceFromPlayer(EntityBehaviour entity)
	{
		//with ranged weapons idle within min/max range of weapon. (	bow example: (10 / 1.25 = 8)	(10 / 1.5 = 6.25)	)

		if (!CheckDistanceToPlayerIsBigger(entity, equippedWeapon.weaponBaseRef.maxAttackRange / 1.25f) &&
			CheckDistanceToPlayerIsBigger(entity, equippedWeapon.weaponBaseRef.maxAttackRange / 1.75f) && idleInWeaponRange == false)
		{
			entity.HasReachedDestination = true;
			idleInWeaponRange = true;
		}
		else if (CheckDistanceToPlayerIsBigger(entity, equippedWeapon.weaponBaseRef.maxAttackRange))
		{
			entity.SetNewDestination(entity.playersLastKnownPosition);
			idleInWeaponRange = false;
		}
		else if (!CheckDistanceToPlayerIsBigger(entity, equippedWeapon.weaponBaseRef.minAttackRange * 2))
		{
			Vector3 dirToPlayer =  entity.playersLastKnownPosition - new Vector2(entity.transform.position.x, entity.transform.position.y);
			Vector3 fleeDir = entity.transform.position - dirToPlayer;

			entity.SetNewDestination(fleeDir);
			idleInWeaponRange = false;
		}
	}
	private bool CheckDistanceToPlayerIsBigger(EntityBehaviour entity, float distanceToCheck)
	{
		if (entity.playerTarget == null) return false;
		float distance = Vector2.Distance(entity.transform.position, entity.playerTarget.transform.position);

		if (distance > distanceToCheck)
			return true;
		else
			return false;
	}
}
