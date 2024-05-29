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
		distanceToPlayer = entity.entityBehaviour.aggroRange;
	}
	public override void Exit(EntityBehaviour entity)
	{

	}
	public override void UpdateLogic(EntityBehaviour entity)
	{
		UpdatePlayerPosition(entity);

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
		if (!entity.CheckIfPlayerVisible())
			entity.ChangeStateIdle();
		else if (entity.CheckIfPlayerVisible())
		{
			distanceToPlayer = Vector3.Distance(entity.transform.position, entity.player.transform.position);

			if (entity.CheckDistanceToPlayerIsBigger(entity.entityBehaviour.maxChaseRange)) //player outside of max chase range
			{
				entity.player = null;
				entity.ChangeStateIdle();
			}

			if (equippedWeapon.weaponBaseRef.isRangedWeapon)
				KeepDistanceFromPlayer(entity);
			else
			{
				//if melee weapon charge at player
				if (entity.CheckDistanceToDestination())
					ChasePlayer(entity);
			}
		}
	}

	//attack behaviour
	private void UpdatePlayerPosition(EntityBehaviour entity)
	{
		if (entity.player != null && entity.CheckIfPlayerVisible())
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
			entity.CheckAndSetNewPath(entity.transform.position);
			idleInWeaponRange = true;
		}
		else if (entity.CheckDistanceToPlayerIsBigger(equippedWeapon.weaponBaseRef.maxAttackRange))
		{
			ChasePlayer(entity);
			idleInWeaponRange = false;
		}
		else if (!entity.CheckDistanceToPlayerIsBigger(equippedWeapon.weaponBaseRef.minAttackRange * 2))
		{
			Vector3 fleeDir = new Vector2(entity.transform.position.x, entity.transform.position.y) - entity.playersLastKnownPosition;
			Vector2 fleePos = entity.SampleNewMovePosition(entity.transform.position + fleeDir);
			entity.CheckAndSetNewPath(fleePos);
		}
	}
	private void ChasePlayer(EntityBehaviour entity)
	{
		Vector2 movePosition = entity.SampleNewMovePosition(entity.playersLastKnownPosition);
		entity.CheckAndSetNewPath(movePosition);
	}
}
