using NavMeshPlus.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EntityBehaviour : MonoBehaviour
{
	[Header("Behaviour Info")]
	public SOEntityBehaviour entityBehaviour;
	[HideInInspector] public EntityStats entityStats;
	private EnemyBaseState currentState;
	private EnemyIdleState idleState = new EnemyIdleState();
	private EnemyAttackState attackState = new EnemyAttackState();

	private Rigidbody2D rb;
	private Animator animator;
	public LayerMask includeMe;
	private NavMeshAgent navMeshAgent;
	[HideInInspector] public bool markedForCleanUp;

	[Header("Bounds Settings")]
	[HideInInspector] public Bounds idleBounds;
	[HideInInspector] public Vector2 movePosition;
	[HideInInspector] public bool HasReachedDestination;
	[HideInInspector] public float idleTimer;

	[HideInInspector] public Bounds chaseBounds;
	public Vector2 playersLastKnownPosition;
	public PlayerController player;
	public CircleCollider2D viewRangeCollider;

	[Header("Prefabs")]
	public GameObject AbilityAoePrefab;
	public GameObject projectilePrefab;

	private void Awake()
	{
		entityStats = GetComponent<EntityStats>();
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		navMeshAgent = GetComponent<NavMeshAgent>();
	}
	private void Start()
	{
		Initilize();
	}
	private void Update()
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
	private void FixedUpdate()
	{
		currentState.UpdatePhysics(this);

		chaseBounds.min = new Vector3(transform.position.x - entityBehaviour.maxChaseRange,
		transform.position.y - entityBehaviour.maxChaseRange, transform.position.z);

		chaseBounds.max = new Vector3(transform.position.x + entityBehaviour.maxChaseRange,
		transform.position.y + entityBehaviour.maxChaseRange, transform.position.z);

		UpdateSpriteDirection();
		UpdateAnimationState();
	}
	private void Initilize()
	{
		UpdateBounds(transform.position);
		ResetBehaviour();

		markedForCleanUp = false;
		HasReachedDestination = true;

		viewRangeCollider.radius = entityBehaviour.aggroRange;
		viewRangeCollider.gameObject.GetComponent<PlayerDetection>().entityBehaviourRef = this;

		navMeshAgent.speed = entityBehaviour.navMeshMoveSpeed;
		navMeshAgent.angularSpeed = entityBehaviour.navMeshTurnSpeed;
		navMeshAgent.acceleration = entityBehaviour.navMeshAcceleration;
		navMeshAgent.stoppingDistance = entityBehaviour.navMeshStoppingDistance;
	}
	public void ResetBehaviour()
	{
		markedForCleanUp = false;
		HasReachedDestination = true;
		ChangeStateIdle();
	}
	public void UpdateBounds(Vector3 position)
	{
		idleBounds.min = new Vector3(position.x - entityBehaviour.idleWanderRadius,
			position.y - entityBehaviour.idleWanderRadius, position.z);

		idleBounds.max = new Vector3(position.x + entityBehaviour.idleWanderRadius,
			position.y + entityBehaviour.idleWanderRadius, position.z);

		chaseBounds.min = new Vector3(position.x - entityBehaviour.maxChaseRange,
			position.y - entityBehaviour.maxChaseRange, position.z);

		chaseBounds.max = new Vector3(position.x + entityBehaviour.maxChaseRange,
			position.y + entityBehaviour.maxChaseRange, position.z);
	}

	private void UpdateSpriteDirection()
	{
		if (navMeshAgent.velocity.x < 0)
			transform.eulerAngles = new Vector3(0, 0, 0);
		else
			transform.eulerAngles = new Vector3(0, 180, 0);
	}
	private void UpdateAnimationState()
	{
		if (navMeshAgent.velocity == new Vector3(0,0,0))
			animator.SetBool("isIdle", true);
		else
			animator.SetBool("isIdle", false);
	}

	//idle + attack behaviour
	public Vector2 SampleNewMovePosition(Vector2 position)
	{
		NavMesh.SamplePosition(position, out NavMeshHit navMeshHit, 0.1f, navMeshAgent.areaMask);
		return navMeshHit.position;
	}
	public bool CheckAndSetNewPath(Vector2 movePosition)
	{
		if (double.IsInfinity(movePosition.x)) return false;

		NavMeshPath path = new NavMeshPath();
		if (!navMeshAgent.CalculatePath(movePosition, path)) return false;

		navMeshAgent.SetPath(path);
		HasReachedDestination = false;
		return true;
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
	public bool CheckDistanceToPlayerIsBigger(float distanceToCheck)
	{
		if (player == null) return false;
		float distance = Vector2.Distance(transform.position, player.transform.position);

		if (distance > distanceToCheck)
			return true;
		else
			return false;
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
		Gizmos.color = Color.yellow;
		//Gizmos.DrawWireSphere(idleBounds.center, idleBounds.extents.x);

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(chaseBounds.center, chaseBounds.extents.x);

		Gizmos.color = Color.red;
		if (player != null)
			Gizmos.DrawLine(transform.position, player.transform.position);
	}
}
