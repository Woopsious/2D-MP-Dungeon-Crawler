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
	readonly EntityStats entity;
	readonly NavMeshAgent agent;
	readonly Weapons equippedWeapon;

	readonly float distanceCheckDuration = 0.1f;
	float distanceCheckTimer;

	public bool CanPerform => entity.entityBehaviour.currentPlayerTargetInView;
	public bool Complete => !entity.entityBehaviour.currentPlayerTargetInView;

	public AttackStrategy(EntityStats entity)
	{
		this.entity = entity;
		this.agent = entity.entityBehaviour.navMeshAgent;
		this.equippedWeapon = entity.equipmentHandler.equippedWeapon;
	}

	public void Start()
	{

	}
	public void Update(float time)
	{
		entity.entityBehaviour.TryAttackWithMainWeapon();
		entity.entityBehaviour.TryCastOffensiveAbility();

		distanceCheckTimer -= time;

		if (distanceCheckTimer <= 0)
		{
			distanceCheckTimer = distanceCheckDuration;

			if (equippedWeapon.weaponBaseRef.isRangedWeapon)
				KeepDistanceFromPlayer(entity.entityBehaviour);
			else
				KeepPlayerInMeleeRange(entity.entityBehaviour);
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
	readonly EntityStats entity;
	readonly NavMeshAgent agent;
	readonly float stoppingDistance;

	public bool CanPerform => !entity.entityBehaviour.currentPlayerTargetInView;
	public bool Complete => agent.remainingDistance <= stoppingDistance && !agent.pathPending;

	public WanderStrategy(EntityStats entity)
	{
		this.entity = entity;
		this.agent = entity.entityBehaviour.navMeshAgent;
		this.stoppingDistance = entity.entityBehaviour.entityBehaviour.navMeshStoppingDistance;
	}

	public void Start() => Move();
	public void Move()
	{
		if (entity.entityBehaviour.playersLastKnownPosition == new Vector2(0, 0))
			FindNewIdlePosition(entity.entityBehaviour);
		else
			InvestigatePlayersLastKnownPos(entity.entityBehaviour);
	}
	private void InvestigatePlayersLastKnownPos(EntityBehaviour entity)
	{
		if (entity.playersLastKnownPosition == new Vector2(0, 0)) return;

		entity.SetNewDestination(entity.playersLastKnownPosition);

		if (entity.HasReachedDestination())
		{
			entity.playersLastKnownPosition = new Vector2(0, 0);
			entity.ChangeState(entity.idleState);
		}
	}
	private void FindNewIdlePosition(EntityBehaviour entity)
	{
		Vector2 randomMovePosition = Utilities.GetRandomPointInBounds(entity.idleBounds);
		NavMeshPath path = new NavMeshPath();

		if (entity.navMeshAgent.CalculatePath(randomMovePosition, path) && path.status == NavMeshPathStatus.PathComplete)
			entity.navMeshAgent.SetPath(path);
		else
			FindNewIdlePosition(entity);
	}
}

public class IdleStrategy : IActionStrategy
{
	EntityStats entity;
	float idleDuration;
	float idleTimer;

	public bool CanPerform => !entity.entityBehaviour.currentPlayerTargetInView; // Agent can always Idle
	public bool Complete => idleTimer <= 0;

	public IdleStrategy(EntityStats entity, float idleDuration)
	{
		this.entity = entity;
		this.idleDuration = idleDuration;
	}

	public void Start() => SetTimer();
	private void SetTimer()
	{
		idleTimer = idleDuration;
	}
	public void Update(float time)
	{
		idleTimer -= time;
	}
}
