using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityBehaviour : MonoBehaviour
{
	public LayerMask includeMe;
	public NavMeshAgent navMeshAgent;
	public Rigidbody rb;

	public SOEntityBehaviours entityBehaviour;
	public SphereCollider viewRangeCollider;
	public GameObject centerPoint;

	private Bounds idleBounds;
	private Vector3 movePosition;
	private bool HasReachedDestination;

	public float idleTimer;

	public PlayerController player;
	public Vector3 playersLastKnownPosition;
	private Bounds chaseBounds;

	public void Start()
	{
		idleBounds.min = new Vector3(transform.position.x - entityBehaviour.idleWanderRadius,
			transform.position.y - 3, transform.position.z - entityBehaviour.idleWanderRadius);

		idleBounds.max = new Vector3(transform.position.x + entityBehaviour.idleWanderRadius,
			transform.position.y + 3, transform.position.z + entityBehaviour.idleWanderRadius);

		chaseBounds.min = new Vector3(transform.position.x - entityBehaviour.maxChaseRange,
			transform.position.y - 3, transform.position.z - entityBehaviour.maxChaseRange);

		chaseBounds.max = new Vector3(transform.position.x + entityBehaviour.maxChaseRange,
			transform.position.y + 3, transform.position.z + entityBehaviour.maxChaseRange);

		HasReachedDestination = true;

		viewRangeCollider.radius = entityBehaviour.aggroRange;
		viewRangeCollider.gameObject.GetComponent<PlayerDetection>().SetBehaviourRef(this);

		navMeshAgent.speed = entityBehaviour.navMeshMoveSpeed;
		navMeshAgent.angularSpeed = entityBehaviour.navMeshTurnSpeed;
		navMeshAgent.acceleration = entityBehaviour.navMeshAcceleration;
	}
	public void Update()
	{
		UpdatePlayerPosition();

		if (playersLastKnownPosition == Vector3.zero)
		{
			// 1. idle and randomly move around the map within bounds of where they spawned
			IdleAtPositionTimer();
			CheckDistance();
		}
		else if (playersLastKnownPosition != Vector3.zero)
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
		Vector3 randomMovePosition = Utilities.GetRandomPointInBounds(idleBounds);
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
		Vector3 movePosition = SampleNewMovePosition(playersLastKnownPosition);
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
		try
		{
			Physics.Linecast(transform.position, player.transform.position, out RaycastHit hit, includeMe);

			if (hit.point != null && hit.collider.gameObject == player.gameObject)
				return true;
			else
				return false;
		}
		catch
		{
			return false;
		}
	}
	public bool CheckChaseDistance()
	{
		float distance = Vector3.Distance(gameObject.transform.position, playersLastKnownPosition);

		if (distance < entityBehaviour.maxChaseRange)
			return false;
		else return true;
	}

	//idle + attack behaviour
	public Vector3 SampleNewMovePosition(Vector3 position)
	{
		NavMesh.SamplePosition(position, out NavMeshHit navMeshHit, 10, navMeshAgent.areaMask);
		return navMeshHit.position;
	}
	public bool CheckAndSetNewPath(Vector3 movePosition)
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
			playersLastKnownPosition = Vector3.zero;
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
}
