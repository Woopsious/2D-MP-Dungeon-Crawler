using NavMeshPlus.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityBehaviour : MonoBehaviour
{
	[Header("Refs")]
	private EntityStats entityStats;
	private Rigidbody2D rb;
	private AudioHandler audioHandler;
	private Animator animator;
	public LayerMask includeMe;
	public NavMeshAgent navMeshAgent;

	[Header("Behaviour")]
	public SOEntityBehaviour entityBehaviour;
	private EnemyBaseState currentState;
	private EnemyIdleState idleState = new EnemyIdleState();
	private EnemyAttackState attackState = new EnemyAttackState();

	[Header("bounds Settings")]
	[HideInInspector] public Bounds idleBounds;
	public Vector2 movePosition;
	public bool HasReachedDestination;
	public float idleTimer;

	[HideInInspector] public Bounds chaseBounds;
	public Vector2 playersLastKnownPosition;
	public PlayerController player;
	public CircleCollider2D viewRangeCollider;

	[Header("Prefabs")]
	public GameObject AbilityAoePrefab;
	public GameObject projectilePrefab;

	[Header("Idle Sound Settings")]
	private readonly float idleSoundCooldown = 5f;
	private float idleSoundTimer;
	private readonly int chanceOfIdleSound = 25;

	private void Awake()
	{
		entityStats = GetComponent<EntityStats>();
		rb = GetComponent<Rigidbody2D>();
		audioHandler = GetComponent<AudioHandler>();
		animator = GetComponent<Animator>();
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

		UpdateSpriteDirection();
		UpdateAnimationState();
		PlayIdleSound();
	}
	private void Initilize()
	{
		ResetBehaviour();

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
		ChangeStateIdle();

		idleBounds.min = new Vector3(transform.position.x - entityBehaviour.idleWanderRadius,
			transform.position.y - entityBehaviour.idleWanderRadius, transform.position.z);

		idleBounds.max = new Vector3(transform.position.x + entityBehaviour.idleWanderRadius,
			transform.position.y + entityBehaviour.idleWanderRadius, transform.position.z);

		chaseBounds.min = new Vector3(transform.position.x - entityBehaviour.maxChaseRange,
			transform.position.y - entityBehaviour.maxChaseRange, transform.position.z);

		chaseBounds.max = new Vector3(transform.position.x + entityBehaviour.maxChaseRange,
			transform.position.y + entityBehaviour.maxChaseRange, transform.position.z);
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
	private void PlayIdleSound()
	{
		idleSoundTimer -= Time.deltaTime;

		if (idleSoundTimer <= 0)
		{
			idleSoundTimer = idleSoundCooldown;
			if (chanceOfIdleSound < Utilities.GetRandomNumber(100))
				audioHandler.PlayAudio(entityStats.entityBaseStats.idleSfx);
		}
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
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(idleBounds.center, idleBounds.size);

		if (player != null)
			Gizmos.DrawLine(transform.position, player.transform.position);

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(chaseBounds.center, chaseBounds.size);
	}
}
