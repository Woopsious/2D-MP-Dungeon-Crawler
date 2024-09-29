using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EnemyAttackState : EnemyBaseState
{
	Weapons equippedWeapon;

	public override void Enter(EntityBehaviour entity)
	{
		equippedWeapon = entity.entityStats.equipmentHandler.equippedWeapon;
	}
	public override void Exit(EntityBehaviour entity)
	{

	}
	public override void UpdateLogic(EntityBehaviour entity)
	{
		if (!entity.CurrentPlayerTargetVisible())
			entity.ChangeState(entity.wanderState);

		if (CheckDistanceToPlayerIsBigger(entity, entity.entityBehaviour.maxChaseRange)) //de aggro
			entity.ChangeState(entity.wanderState);

		if (entity.entityStats.entityBaseStats.isBossVersion)
			BossAttackBehaviourLogic((BossEntityBehaviour)entity);
		else
			AttackBehaviourLogic(entity);
	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{

	}
	
	//attack behaviour logic
	private void AttackBehaviourLogic(EntityBehaviour entity)
	{
		if (equippedWeapon == null) return;
		if (equippedWeapon.weaponBaseRef.isRangedWeapon)
			KeepDistanceFromPlayer(entity);
		else
			KeepPlayerInMeleeRange(entity);

		entity.AttackWithMainWeapon();

		entity.CastOffensiveAbility();
	}

	private void BossAttackBehaviourLogic(BossEntityBehaviour bossEntity)
	{
		if (equippedWeapon == null) return;
		if (equippedWeapon.weaponBaseRef.isRangedWeapon)
			KeepDistanceFromPlayer(bossEntity);
		else
			KeepPlayerInMeleeRange(bossEntity);

		bossEntity.AttackWithMainWeapon();

		bossEntity.CastOffensiveAbility();
		bossEntity.CastBossAbilityOne();
		bossEntity.CastBossAbilityTwo();
		bossEntity.CastBossAbilityThree();
	}

	//melee weapon logic
	private void KeepPlayerInMeleeRange(EntityBehaviour entity)
	{
		if (entity.entityStats.entityBaseStats.isBossVersion)
		{
			if (CheckDistanceToPlayerIsBigger(entity, equippedWeapon.weaponBaseRef.maxAttackRange + 0.5f))
				entity.SetNewDestination(entity.playersLastKnownPosition);
		}
		else
		{
			if (CheckDistanceToPlayerIsBigger(entity, equippedWeapon.weaponBaseRef.maxAttackRange - 0.5f))
				entity.SetNewDestination(entity.playersLastKnownPosition);
		}
	}

	//ranged weapon logic
	private void KeepDistanceFromPlayer(EntityBehaviour entity)
	{
		//if (CheckDistanceToPlayerIsBigger(entity, GetDistanceToKeepFromPlayer(entity)) &&
			//!CheckDistanceToPlayerIsBigger(entity, equippedWeapon.weaponBaseRef.minAttackRange + 2)) //stop within ranges
				//return;

		//with ranged weapons idle within max range of weapon. (bow example: (10 - 2 = 8)	(2 + 2 = 4))
		if (CheckDistanceToPlayerIsBigger(entity, GetDistanceToKeepFromPlayer(entity)))//move closer
			entity.SetNewDestination(MoveCloserToPlayer(entity.transform.position, entity.playersLastKnownPosition, 0.3f));

		else if (!CheckDistanceToPlayerIsBigger(entity, equippedWeapon.weaponBaseRef.minAttackRange + 2))//flee from player
			entity.SetNewDestination(FleeFromPlayer(entity.transform.position, entity.playersLastKnownPosition));
	}
	private int GetDistanceToKeepFromPlayer(EntityBehaviour entity)
	{
		int distance;
		if ((int)entity.entityBehaviour.aggroRange < equippedWeapon.weaponBaseRef.maxAttackRange - 2)
			distance = (int)entity.entityBehaviour.aggroRange;
		else
			distance = (int)equippedWeapon.weaponBaseRef.maxAttackRange - 2;

		return distance;
	}
	private Vector2 FleeFromPlayer(Vector2 start, Vector2 end)
	{
		Vector2 fleePos = start - (end - start);
		return fleePos;
	}
	private Vector2 MoveCloserToPlayer(Vector2 start, Vector2 end, float percent)
	{
		Vector2 closerPos = start + percent * (end - start);
		return closerPos;
	}

	//distance checks
	private bool CheckDistanceToPlayerIsBigger(EntityBehaviour entity, float distanceToCheck)
	{
		float distance = Vector2.Distance(entity.transform.position, entity.playersLastKnownPosition);

		if (distance > distanceToCheck)
			return true;
		else
			return false;
	}
}
