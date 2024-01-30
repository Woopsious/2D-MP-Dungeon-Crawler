using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityBehaviour : MonoBehaviour
{
	[HideInInspector] public EntityStats entityStats;

	public LayerMask includeMe;
	public NavMeshAgent navMeshAgent;
	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;
	private Animator animator;

	public SOEntityBehaviour entityBehaviour;
	private EnemyBaseState currentState;
	private EnemyIdleState idleState = new EnemyIdleState();
	private EnemyAttackState attackState = new EnemyAttackState();

	[HideInInspector] public Bounds idleBounds;
	public Vector2 movePosition;
	public bool HasReachedDestination;
	public float idleTimer;

	[HideInInspector] public Bounds chaseBounds;
	public Vector2 playersLastKnownPosition;
	public PlayerController player;
	public CircleCollider2D viewRangeCollider;

	public void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		entityStats = GetComponent<EntityStats>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		spriteRenderer.sprite = entityStats.entityBaseStats.sprite;
		animator = GetComponent<Animator>();

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
		navMeshAgent.stoppingDistance = entityBehaviour.navMeshStoppingDistance;

		ChangeStateIdle();
	}
	public void Update()
	{
		/// <summary>
		/// LOGIC FOR ENTITY BEHAVIOUR
		/// 1. idle and randomly move around the map within bounds of where they spawned
		/// 2. when play enters agro range, chase player endless till they escape max chase range
		/// 2A. if they escape max chase range move to last know position
		/// 2B. if player moves out of visible range move to last know position
		/// 3. once there, if player not found go back to step 1.
		/// 4. once there, if player is found return to step 2.
		/// </summary>
		/// 

		currentState.UpdateLogic(this);
	}
	public void FixedUpdate()
	{
		currentState.UpdatePhysics(this);

		UpdateSpriteDirection();
		UpdateAnimationState();
	}
	public void UpdateSpriteDirection()
	{
		if (navMeshAgent.velocity.x < 0)
			transform.eulerAngles = new Vector3(0, 0, 0);
		else
			transform.eulerAngles = new Vector3(0, 180, 0);
	}
	public void UpdateAnimationState()
	{
		if (navMeshAgent.velocity == new Vector3(0,0,0))
			animator.SetBool("isIdle", true);
		else
			animator.SetBool("isIdle", false);
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
		else
			return false;
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
	public bool CheckDistanceToDestination()
	{
		if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance && HasReachedDestination == false)
		{
			HasReachedDestination = true;
			return false;
		}
		else
			return true;
	}
	public bool CheckDistanceToPlayer()
	{
		if (player == null) return false;
		float distance = Vector2.Distance(transform.position, player.transform.position);

		if (distance < entityBehaviour.maxChaseRange)
			return false;
		else
			return true;
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
