using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskAttack : BTNode
{
	EntityStats stats;
	EntityBehaviour behaviour;
	EntityEquipmentHandler equipmentHandler;
	NavMeshAgent navMesh;
	

	public TaskAttack(EntityStats entity)
	{
		stats = entity;
		behaviour = entity.entityBehaviour;
		equipmentHandler = entity.equipmentHandler;
		navMesh = entity.entityBehaviour.navMeshAgent;
	}

	public override NodeState Evaluate()
	{
		Debug.Log(stats.name + " attack task");
		AttackBehaviourLogic();
		return state = NodeState.RUNNING;
	}

	//attack behaviour logic
	protected void AttackBehaviourLogic()
	{
		if (equipmentHandler.equippedWeapon.weaponBaseRef.isRangedWeapon)
			KeepDistanceFromPlayer(behaviour);
		else
			KeepPlayerInMeleeRange(behaviour);

		behaviour.TryAttackWithMainWeapon();
		behaviour.TryCastOffensiveAbility();
	}

	//melee weapon logic
	protected void KeepPlayerInMeleeRange(EntityBehaviour entity)
	{
		if (entity.entityStats.entityBaseStats.isBossVersion)
		{
			if (CheckDistanceToPlayerIsBigger(entity, equipmentHandler.equippedWeapon.weaponBaseRef.maxAttackRange + 0.5f))
				entity.SetNewDestination(entity.playersLastKnownPosition);
		}
		else
		{
			if (CheckDistanceToPlayerIsBigger(entity, equipmentHandler.equippedWeapon.weaponBaseRef.maxAttackRange - 0.5f))
				entity.SetNewDestination(entity.playersLastKnownPosition);
		}
	}

	//ranged weapon logic
	protected void KeepDistanceFromPlayer(EntityBehaviour entity)
	{
		//if (CheckDistanceToPlayerIsBigger(entity, GetDistanceToKeepFromPlayer(entity)) &&
		//!CheckDistanceToPlayerIsBigger(entity, equippedWeapon.weaponBaseRef.minAttackRange + 2)) //stop within ranges
		//return;

		//with ranged weapons idle within max range of weapon. (bow example: (10 - 2 = 8)	(2 + 2 = 4))
		if (CheckDistanceToPlayerIsBigger(entity, GetDistanceToKeepFromPlayer(entity)))//move closer
			entity.SetNewDestination(MoveCloserToPlayer(entity.transform.position, entity.playersLastKnownPosition, 0.3f));

		else if (!CheckDistanceToPlayerIsBigger(entity, equipmentHandler.equippedWeapon.weaponBaseRef.minAttackRange + 2))//flee from player
			entity.SetNewDestination(FleeFromPlayer(entity.transform.position, entity.playersLastKnownPosition));
	}
	protected int GetDistanceToKeepFromPlayer(EntityBehaviour entity)
	{
		int distance;
		if ((int)entity.entityBehaviour.aggroRange < equipmentHandler.equippedWeapon.weaponBaseRef.maxAttackRange - 2)
			distance = (int)entity.entityBehaviour.aggroRange;
		else
			distance = (int)equipmentHandler.equippedWeapon.weaponBaseRef.maxAttackRange - 2;

		return distance;
	}
	protected Vector2 FleeFromPlayer(Vector2 start, Vector2 end)
	{
		Vector2 fleePos = start - (end - start);
		return fleePos;
	}
	protected Vector2 MoveCloserToPlayer(Vector2 start, Vector2 end, float percent)
	{
		Vector2 closerPos = start + percent * (end - start);
		return closerPos;
	}

	//distance checks
	protected bool CheckDistanceToPlayerIsBigger(EntityBehaviour entity, float distanceToCheck)
	{
		float distance = Vector2.Distance(entity.transform.position, entity.playersLastKnownPosition);

		if (distance > distanceToCheck)
			return true;
		else
			return false;
	}
}
