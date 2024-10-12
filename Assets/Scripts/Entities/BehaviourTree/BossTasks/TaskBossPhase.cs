using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskBossPhase : BTNode
{
	protected BossEntityBehaviour behaviour;
	protected BossEntityStats stats;
	protected EntityEquipmentHandler equipmentHandler;

	protected virtual void RunPhases()
	{
		Debug.Log(stats.name + " boss phases task");

		if (stats.bossPhase == BossEntityStats.BossPhase.firstPhase)
			PhaseOne();
		else if (stats.bossPhase == BossEntityStats.BossPhase.secondPhase)
			PhaseTwo();
		else if (stats.bossPhase == BossEntityStats.BossPhase.thirdPhase)
			PhaseThree();
	}

	protected virtual void PhaseOne()
	{

	}
	protected virtual void PhaseTwo()
	{

	}
	protected virtual void PhaseThree()
	{

	}

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
		if (CheckDistanceToPlayerIsBigger(entity, GetDistanceToKeepFromPlayer(entity, equipment)))//move closer
			entity.SetNewDestination(MoveCloserToPlayer(entity.transform.position, entity.playersLastKnownPosition, 0.3f));

		else if (!CheckDistanceToPlayerIsBigger(entity, equipment.equippedWeapon.weaponBaseRef.minAttackRange + 2))//flee from player
			entity.SetNewDestination(FleeFromPlayer(entity.transform.position, entity.playersLastKnownPosition));
	}
	protected int GetDistanceToKeepFromPlayer(EntityBehaviour entity, EntityEquipmentHandler equipment)
	{
		int distance;
		if ((int)entity.behaviourRef.aggroRange < equipment.equippedWeapon.weaponBaseRef.maxAttackRange - 2)
			distance = (int)entity.behaviourRef.aggroRange;
		else
			distance = (int)equipment.equippedWeapon.weaponBaseRef.maxAttackRange - 2;

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
