using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EnemyAttackState : EnemyBaseState
{
	Weapons equippedWeapon;
	float distanceToPlayer;
	bool idleInWeaponRange;

	private LayerMask playerLayerMask;
	private LayerMask entityLayerMask;

	private int chanceToUseAbility = 50;
	private float useAbilityCooldown = 2.5f;
	private float useAbilityTimer;

	public override void Enter(EntityBehaviour entity)
	{
		equippedWeapon = entity.entityStats.equipmentHandler.equippedWeapon;
		distanceToPlayer = entity.entityBehaviour.aggroRange;
		playerLayerMask = 1 << 8;
		entityLayerMask = 1 << 9;
	}
	public override void Exit(EntityBehaviour entity)
	{

	}
	public override void UpdateLogic(EntityBehaviour entity)
	{
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
		entity.CastOffensiveAbility();

		if (equippedWeapon == null && !equippedWeapon.canAttackAgain) return;

		if (distanceToPlayer <= equippedWeapon.weaponBaseRef.maxAttackRange)
		{
			if (equippedWeapon.weaponBaseRef.isRangedWeapon)
				equippedWeapon.RangedAttack(entity.playerTarget.transform.position, entity.projectilePrefab);
			else
				equippedWeapon.MeleeAttack(entity.playerTarget.transform.position);
		}
	}

	//abilites logic

	//attack behaviour physics
	private void AttackBehaviourPhysics(EntityBehaviour entity)
	{
		if (!entity.currentPlayerTargetInView)
			entity.ChangeStateIdle();
		else
		{
			distanceToPlayer = Vector3.Distance(entity.transform.position, entity.playerTarget.transform.position);

			if (CheckDistanceToPlayerIsBigger(entity, entity.entityBehaviour.maxChaseRange)) //player outside of max chase range
				entity.ChangeStateIdle();

			if (equippedWeapon == null) return;
			if (equippedWeapon.weaponBaseRef.isRangedWeapon)
				KeepDistanceFromPlayer(entity);
			else
			{
				//if melee weapon charge at player
				if (entity.CheckDistanceToDestination())
					entity.SetNewDestination(entity.playersLastKnownPosition);
			}
		}
	}
	private void KeepDistanceFromPlayer(EntityBehaviour entity)
	{
		//idle when within ranges of max range		eg: 10 / 1.25 = 8		eg: 10 / 1.5 = 6.25
		//chase player when out of max attack range till inside max range / 1.25f
		//flee when player inside min attack range * 2	eg: 2 * 2 = 4

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
