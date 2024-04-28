using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EnemyAttackState : EnemyBaseState
{
	Weapons equippedWeapon;
	float distanceToPlayer;

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

		///<summary>
		///	code to dictate when an enemy should decide to attack, for now only supports melee weapons
		///	and doesnt take into account the length of the weapon (dagger only has a reach of about 1, the rest have about 2)
		///<summary>

		if (distanceToPlayer <= equippedWeapon.weaponBaseRef.baseMaxAttackRange)
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

			if (entity.CheckDistanceToPlayer()) //player outside of max chase range
			{
				entity.player = null;
				entity.ChangeStateIdle();
			}

			if (entity.CheckDistanceToDestination())
				ChasePlayer(entity);
		}
	}

	//attack behaviour
	public void UpdatePlayerPosition(EntityBehaviour entity)
	{
		if (entity.player != null && entity.CheckIfPlayerVisible())
			entity.playersLastKnownPosition = entity.player.transform.position;
	}
	public void ChasePlayer(EntityBehaviour entity)
	{
		Vector2 movePosition = entity.SampleNewMovePosition(entity.playersLastKnownPosition);
		entity.CheckAndSetNewPath(movePosition);
	}
}
