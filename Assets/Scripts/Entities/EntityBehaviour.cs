using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityBehaviour : MonoBehaviour
{
	public LayerMask includeMe;
	public NavMeshAgent navMeshAgent;
	public Rigidbody2D rb;

	public SOEntityBehaviour entityBehaviour;
	public EnemyBaseState currentState;
	public EnemyIdleState idleState = new EnemyIdleState();
	public EnemyAttackState attackState = new EnemyAttackState();

	public Bounds idleBounds;
	public Vector2 movePosition;
	public bool HasReachedDestination;
	public float idleTimer;

	public Bounds chaseBounds;
	public Vector2 playersLastKnownPosition;
	public PlayerController player;
	public CircleCollider2D viewRangeCollider;

	public void Start()
	{
		idleBounds.min = new Vector3(transform.position.x - entityBehaviour.idleWanderRadius,
			transform.position.y - entityBehaviour.idleWanderRadius, transform.position.z - 3);

		idleBounds.max = new Vector3(transform.position.x + entityBehaviour.idleWanderRadius,
			transform.position.y + entityBehaviour.idleWanderRadius, transform.position.z + 3);

		chaseBounds.min = new Vector3(transform.position.x - entityBehaviour.maxChaseRange,
			transform.position.y - entityBehaviour.maxChaseRange, transform.position.z - 3);

		chaseBounds.max = new Vector3(transform.position.x + entityBehaviour.maxChaseRange,
			transform.position.y + entityBehaviour.maxChaseRange, transform.position.z + 3);

		HasReachedDestination = true;

		viewRangeCollider.radius = entityBehaviour.aggroRange;
		viewRangeCollider.gameObject.GetComponent<PlayerDetection>().SetBehaviourRef(this);

		navMeshAgent.speed = entityBehaviour.navMeshMoveSpeed;
		navMeshAgent.angularSpeed = entityBehaviour.navMeshTurnSpeed;
		navMeshAgent.acceleration = entityBehaviour.navMeshAcceleration;
	}
	public void Update()
	{
		currentState.UpdateLogic(this);
		UpdatePlayerPosition();

		if (playersLastKnownPosition == Vector2.zero)
		{
			// 1. idle and randomly move around the map within bounds of where they spawned
			IdleAtPositionTimer();
			CheckDistance();
		}
		else if (playersLastKnownPosition != Vector2.zero)
		{
			// 2. when play enters agro range, chase player endless till they escape max chase range
			// 2A. if they escape max chase range move to last know position
			// 2B. if player moves out of visible range move to last know position
			// 3. once there, if player not found go back to step 1.
			// 4. once there, if player is found return to step 2.

			if (CheckIfPlayerVisible())
				ChasePlayer();
			else
				CheckDistance();
		}
	}
	public void FixedUpdate()
	{
		currentState.UpdatePhysics(this);
	}

	/// <summary>
	/// using a state machine in actual project will be better long term and simpler to maintain
	/// </summary>
	/// 
	//idle behaviour
	public void IdleAtPositionTimer()
	{
		if (HasReachedDestination == true)
		{
			idleTimer -= Time.deltaTime;

			if (idleTimer < 0)
			{
				idleTimer = entityBehaviour.idleWaitTime;
				FindNewIdlePosition();
			}
		}
	}
	public void FindNewIdlePosition()
	{
		Vector2 randomMovePosition = Utilities.GetRandomPointInBounds(idleBounds);
		movePosition = SampleNewMovePosition(randomMovePosition);

		if (CheckAndSetNewPath(movePosition))
			return;
		else
			FindNewIdlePosition();
	}

	//attack behaviour
	public void ChasePlayer()
	{
		HasReachedDestination = false;
		Vector2 movePosition = SampleNewMovePosition(playersLastKnownPosition);
		CheckAndSetNewPath(movePosition);

		if (CheckChaseDistance())
			player = null;
	}
	public void UpdatePlayerPosition()
	{
		if (player != null && CheckIfPlayerVisible())
			playersLastKnownPosition = player.transform.position;
	}
	public bool CheckIfPlayerVisible()
	{
		if (player == null) return false;

		RaycastHit2D hit = Physics2D.Linecast(transform.position, player.transform.position, includeMe);
		if (hit.point != null && hit.collider.gameObject == player.gameObject)
			return true;
		else
			return false;
	}
	public bool CheckChaseDistance()
	{
		float distance = Vector2.Distance(gameObject.transform.position, playersLastKnownPosition);

		if (distance < entityBehaviour.maxChaseRange)
			return false;
		else return true;
	}

	//idle + attack behaviour
	public Vector2 SampleNewMovePosition(Vector2 position)
	{
		NavMesh.SamplePosition(position, out NavMeshHit navMeshHit, 10, navMeshAgent.areaMask);
		return navMeshHit.position;
	}
	public bool CheckAndSetNewPath(Vector2 movePosition)
	{
		NavMeshPath path = new NavMeshPath();
		if (navMeshAgent.CalculatePath(movePosition, path))
		{
			navMeshAgent.SetPath(path);
			HasReachedDestination = false;
			return true;
		}
		else return false;
	}
	public void CheckDistance()
	{
		if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance && HasReachedDestination == false)
		{
			HasReachedDestination = true;
			playersLastKnownPosition = Vector2.zero;
		}
	}

	//utility
	public void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(idleBounds.center, idleBounds.size);

		if (player != null)
			Gizmos.DrawLine(transform.position, player.transform.position);

		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(chaseBounds.center, chaseBounds.size);
	}
	//STATE CHANGES
	public void ChangeStateIdle()
	{
		currentState = idleState;
		currentState.Enter(this);
	}
	public void ChangeStateAttack()
	{
		currentState = attackState;
		currentState.Enter(this);
	}
}
