using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EnemyAttackState : EnemyBaseState
{
	Weapons equippedWeapon;
	float distanceToPlayer;
	bool idleInWeaponRange;

	public override void Enter(EntityBehaviour entity)
	{
		equippedWeapon = entity.GetComponentInChildren<EntityEquipmentHandler>().equippedWeapon;
		equippedWeapon.canAttackAgain = true;
		distanceToPlayer = entity.entityBehaviour.aggroRange;
	}
	public override void Exit(EntityBehaviour entity)
	{

	}
	public override void UpdateLogic(EntityBehaviour entity)
	{
		if (equippedWeapon == null && !equippedWeapon.canAttackAgain) return;

		if (distanceToPlayer <= equippedWeapon.weaponBaseRef.maxAttackRange)
		{
			if (equippedWeapon.weaponBaseRef.isRangedWeapon)
				equippedWeapon.RangedAttack(entity.player.transform.position, entity.projectilePrefab);
			else
				equippedWeapon.MeleeAttack(entity.player.transform.position);
		}
	}
	public override void UpdatePhysics(EntityBehaviour entity)
	{
		UpdatePlayerPosition(entity);

		if (!entity.currentPlayerTargetInView)
			entity.ChangeStateIdle();
		else
		{
			distanceToPlayer = Vector3.Distance(entity.transform.position, entity.player.transform.position);

			if (entity.CheckDistanceToPlayerIsBigger(entity.entityBehaviour.maxChaseRange)) //player outside of max chase range
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

	//attack behaviour
	private void UpdatePlayerPosition(EntityBehaviour entity)
	{
		if (entity.player != null && entity.currentPlayerTargetInView)
			entity.playersLastKnownPosition = entity.player.transform.position;
	}
	private void KeepDistanceFromPlayer(EntityBehaviour entity)
	{
		//idle when within ranges of max range		eg: 10 / 1.25 = 8		eg: 10 / 1.5 = 6.25
		//chase player when out of max attack range till inside max range / 1.25f
		//flee when player inside min attack range * 2	eg: 2 * 2 = 4

		if (!entity.CheckDistanceToPlayerIsBigger(equippedWeapon.weaponBaseRef.maxAttackRange / 1.25f) &&
			entity.CheckDistanceToPlayerIsBigger(equippedWeapon.weaponBaseRef.maxAttackRange / 1.5f) && idleInWeaponRange == false)
		{
			entity.HasReachedDestination = true;
			idleInWeaponRange = true;
		}
		else if (entity.CheckDistanceToPlayerIsBigger(equippedWeapon.weaponBaseRef.maxAttackRange))
		{
			entity.SetNewDestination(entity.playersLastKnownPosition);
			idleInWeaponRange = false;
		}
		else if (!entity.CheckDistanceToPlayerIsBigger(equippedWeapon.weaponBaseRef.minAttackRange * 2))
		{
			Vector3 fleeDir = new Vector2(entity.transform.position.x, entity.transform.position.y) - entity.playersLastKnownPosition;
			idleInWeaponRange = false;
			entity.SetNewDestination(fleeDir);
		}
	}
}
