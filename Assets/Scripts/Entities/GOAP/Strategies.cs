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
	public bool CanPerform => true; // Agent can always attack
	public bool Complete { get; private set; }

	public AttackStrategy(EntityStats entity)
	{

	}

	public void Start()
	{

	}

	public void Update()
	{

	}
}

public class MoveStrategy : IActionStrategy
{
	readonly NavMeshAgent agent;
	readonly Func<Vector3> destination;

	public bool CanPerform => !Complete;
	public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

	public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination)
	{
		this.agent = agent;
		this.destination = destination;
	}

	public void Start() => agent.SetDestination(destination());
	public void Stop() => agent.ResetPath();
}

public class WanderStrategy : IActionStrategy
{
	readonly EntityStats entity;
	readonly NavMeshAgent agent;
	readonly float stoppingDistance;

	public bool CanPerform => !Complete;
	public bool Complete => agent.remainingDistance <= stoppingDistance && !agent.pathPending;

	public WanderStrategy(EntityStats entity, Bounds wanderBounds, float stoppingDistance)
	{
		this.entity = entity;
		this.agent = entity.entityBehaviour.navMeshAgent;
		this.stoppingDistance = stoppingDistance;
	}

	public void Start() => FindNewIdlePosition(entity.entityBehaviour);
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
	float idleDuration;
	float idleTimer;

	public bool CanPerform => true; // Agent can always Idle
	public bool Complete => idleTimer <= 0;

	public IdleStrategy(float idleDuration)
	{
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
