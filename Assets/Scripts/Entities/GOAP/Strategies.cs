using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics.Internal;
using UnityEngine;
using UnityEngine.AI;

public interface IActionStrategy
{
	bool CanPerform { get; }
	bool Complete { get; }

	void Start()
	{
		// noop
	}

	void Update(float deltaTime)
	{
		// noop
	}

	void Stop()
	{
		// noop
	}
}

public class AttackStrategy : IActionStrategy
{
	readonly EntityBehaviour entity;
	readonly NavMeshAgent agent;
	readonly Weapons equippedWeapon;

	readonly float distanceCheckDuration = 0.1f;
	float distanceCheckTimer;

	public bool CanPerform => entity.currentPlayerTargetInView;
	public bool Complete => !entity.currentPlayerTargetInView;

	public AttackStrategy(EntityBehaviour entity)
	{
		this.entity = entity;
		this.agent = entity.navMeshAgent;
		this.equippedWeapon = entity.equipmentHandler.equippedWeapon;
	}

	public void Start()
	{

	}
	public void Update(float time)
	{
		entity.TryAttackWithMainWeapon();
		entity.TryCastOffensiveAbility();

		distanceCheckTimer -= time;

		if (distanceCheckTimer <= 0)
		{
			distanceCheckTimer = distanceCheckDuration;

			if (equippedWeapon.weaponBaseRef.isRangedWeapon)
				KeepDistanceFromPlayer(entity);
			else
				KeepPlayerInMeleeRange(entity);
		}
	}
	public void Stop() => agent.ResetPath();

	//melee weapon logic
	protected void KeepPlayerInMeleeRange(EntityBehaviour entity)
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
	protected void KeepDistanceFromPlayer(EntityBehaviour entity)
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
	protected int GetDistanceToKeepFromPlayer(EntityBehaviour entity)
	{
		int distance;
		if ((int)entity.entityBehaviour.aggroRange < equippedWeapon.weaponBaseRef.maxAttackRange - 2)
			distance = (int)entity.entityBehaviour.aggroRange;
		else
			distance = (int)equippedWeapon.weaponBaseRef.maxAttackRange - 2;

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

public class WanderStrategy : IActionStrategy
{
	EntityBehaviour entity;
	NavMeshAgent navMesh;

	public bool CanPerform => !entity.currentPlayerTargetInView && !Complete && entity.idleTimer > 0;
	public bool Complete => navMesh.remainingDistance <= navMesh.stoppingDistance && !navMesh.pathPending;

	public WanderStrategy(EntityBehaviour entity)
	{
		this.entity = entity;
		navMesh = entity.navMeshAgent;
	}

	public void Start()
	{
		FindNewIdlePosition(entity);
	}
	private void FindNewIdlePosition(EntityBehaviour entity)
	{
		Vector2 randomMovePosition = Utilities.GetRandomPointInBounds(entity.idleBounds);
		NavMeshPath path = new NavMeshPath();

		if (entity.navMeshAgent.CalculatePath(randomMovePosition, path) && path.status == NavMeshPathStatus.PathComplete)
		{
			entity.idleTimer = entity.entityBehaviour.idleWaitTime;
			entity.navMeshAgent.SetPath(path);
		}
		else
			FindNewIdlePosition(entity);
	}
}

public class IdleStrategy : IActionStrategy
{
	EntityBehaviour entity;

	public bool CanPerform => !entity.currentPlayerTargetInView;
	public bool Complete => entity.idleTimer <= 0;

	public IdleStrategy(EntityBehaviour entity)
	{
		this.entity = entity;
	}

	public void Update(float deltaTime)
	{
		entity.idleTimer -= Time.deltaTime;
	}
}
