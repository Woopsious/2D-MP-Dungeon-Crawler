using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityMovement : BTNode
{
	//entity movements
	//melee weapon logic
	protected void KeepPlayerInMeleeRange(EntityBehaviour entity, EntityEquipmentHandler equipment)
	{
		if (entity.entityStats.statsRef.isBossVersion)
		{
			if (CheckDistanceToPlayerIsBigger(entity, equipment.equippedWeapon.weaponBaseRef.maxAttackRange + 0.75f))
				entity.SetNewDestination(entity.playersLastKnownPosition);
		}
		else
		{
			if (CheckDistanceToPlayerIsBigger(entity, equipment.equippedWeapon.weaponBaseRef.maxAttackRange - 0.25f))
				entity.SetNewDestination(entity.playersLastKnownPosition);
		}
	}

	//ranged weapon logic
	protected void KeepDistanceFromPlayer(EntityBehaviour entity, EntityEquipmentHandler equipment)
	{
		//with ranged weapons idle within max range of weapon. (bow example: (10 - 2 = 8)	(2 + 2 = 4))

		//move closer
		if (CheckDistanceToPlayerIsBigger(entity, DistanceToKeepWithinFromPlayer(entity, equipment)))
			entity.SetNewDestination(MoveCloserToPlayer(entity.transform.position, entity.playersLastKnownPosition, 0.3f));

		//flee from player
		else if (!CheckDistanceToPlayerIsBigger(entity, DistanceToKeepFromPlayer(equipment)))
			entity.SetNewDestination(FleeFromPlayer(entity.transform.position, entity.playersLastKnownPosition));
	}
	protected int DistanceToKeepWithinFromPlayer(EntityBehaviour entity, EntityEquipmentHandler equipment)
	{
		int distance;
		if ((int)entity.behaviourRef.aggroRange < equipment.equippedWeapon.weaponBaseRef.maxAttackRange - 2)
			distance = (int)entity.behaviourRef.aggroRange;
		else
			distance = (int)equipment.equippedWeapon.weaponBaseRef.maxAttackRange - 2;

		return distance;
	}
	protected int DistanceToKeepFromPlayer(EntityEquipmentHandler equipment)
	{
		int distance = (int)(equipment.equippedWeapon.weaponBaseRef.minAttackRange + 2);
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

	//boss entity movements
	//move to center piece logic
	protected void MoveToCenterPiece(BossEntityBehaviour bossBehaviour)
	{
		BossEntityStats bossStats = (BossEntityStats)bossBehaviour.entityStats;
		bossBehaviour.SetNewDestination(bossStats.roomCenterPiece.transform.position);
	}

	protected bool BossReachedCenterPiece(BossEntityBehaviour bossBehaviour)
	{
		BossEntityStats bossStats = (BossEntityStats)bossBehaviour.entityStats;
		float distance = Vector2.Distance(bossBehaviour.transform.position, bossStats.roomCenterPiece.transform.position);

		if (distance < bossBehaviour.navMeshAgent.stoppingDistance)
			return true;
		else
			return false;
	}
}
