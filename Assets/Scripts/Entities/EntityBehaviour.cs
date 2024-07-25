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
	[HideInInspector] public NavMeshAgent navMeshAgent;
	[HideInInspector] public bool markedForCleanUp;

	[Header("Bounds Settings")]
	[HideInInspector] public Bounds idleBounds;
	[HideInInspector] public Bounds chaseBounds;

	[Header("Movement Settings")]
	public float idleTimer;
	public bool HasReachedDestination;

	[Header("Player Targeting")]
	public LayerMask includeMe;
	public List<PlayerAggroRating> playerAggroList = new List<PlayerAggroRating>();
	public PlayerController playerTarget;
	public bool currentPlayerTargetInView;
	public Vector2 playersLastKnownPosition;

	[Header("Player Detection")]
	public CircleCollider2D viewRangeCollider;
	public float playerDetectionRange;

	[Header("Prefabs")]
	public GameObject AbilityAoePrefab;
	public GameObject projectilePrefab;

	[Header("Idle Sound Settings")]
	private readonly float playerAggroRatingCooldown = 0.5f;
	private float playerAggroRatingTimer;

	[Header("Idle Sound Settings")]
	private readonly float playerDetectionCooldown = 0.1f;
	private float playerDetectionTimer;

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
		UpdateAggroRatingTimer();
		CheckIfPlayerVisibleTimer();
	}
	private void Initilize()
	{
		UpdateBounds(transform.position);
		ResetBehaviour();

		markedForCleanUp = false;
		HasReachedDestination = true;

		viewRangeCollider.radius = playerDetectionRange;
		viewRangeCollider.gameObject.GetComponent<PlayerDetection>().entityBehaviourRef = this;

		navMeshAgent.speed = entityBehaviour.navMeshMoveSpeed;
		navMeshAgent.angularSpeed = entityBehaviour.navMeshTurnSpeed;
		navMeshAgent.acceleration = entityBehaviour.navMeshAcceleration;
		navMeshAgent.stoppingDistance = entityBehaviour.navMeshStoppingDistance;
	}
	public void ResetBehaviour()
	{
		markedForCleanUp = false;
		//idleTimer = entityBehaviour.idleWaitTime;
		HasReachedDestination = true;
		entityStats.equipmentHandler.equippedWeapon.canAttackAgain = true;
		playerAggroList.Clear();
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
	public void CheckIfPlayerVisibleTimer()
	{
		if (playerTarget == null)
		{
			currentPlayerTargetInView = false;
			return;
		}

		playerDetectionTimer -= Time.deltaTime;

		if (playerDetectionTimer <= 0)
		{
			playerDetectionTimer = playerDetectionCooldown;

			if (Vector2.Distance(transform.position, playerTarget.transform.position) > entityBehaviour.aggroRange)
			{
				currentPlayerTargetInView = false;
				return;
			}

			RaycastHit2D hit = Physics2D.Linecast(transform.position, playerTarget.transform.position, includeMe);
			if (hit.point != null && hit.collider.gameObject == playerTarget.gameObject)
				currentPlayerTargetInView = true;
			else
				currentPlayerTargetInView = false;
		}
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
	public void SetNewDestination(Vector2 destination)
	{
		HasReachedDestination = false;
		navMeshAgent.SetDestination(destination);
	}
	public int GetAggroDistanceFromPlayer(PlayerController player, float aggroModifier)
	{
		float distance = Vector2.Distance(transform.position, player.transform.position);
		float aggroRating = 200 / distance;
		return (int)aggroRating;
	}

	//aggro list
	private void UpdateAggroRatingTimer()
	{
		if (playerAggroList.Count <= 0) return;

		playerAggroRatingTimer -= Time.deltaTime;

		if (playerAggroRatingTimer <= 0)
		{
			playerAggroRatingTimer = playerAggroRatingCooldown;
			UpdateAggroList();
		}
	}
	private void AddPlayerToAggroList(PlayerController player, int damageRecieved)
	{
		foreach (PlayerAggroRating aggroRating in playerAggroList)
		{
			if (aggroRating.player == player) break;
		}

		float aggroModifier;
		if (player.playerClassHandler.currentEntityClass = PlayerClassesUi.Instance.knightClass)
			aggroModifier = 1.1f;
		else if (player.playerClassHandler.currentEntityClass = PlayerClassesUi.Instance.warriorClass)
			aggroModifier = 1.05f;
		else if (player.playerClassHandler.currentEntityClass = PlayerClassesUi.Instance.rogueClass)
			aggroModifier = 1f;
		else if (player.playerClassHandler.currentEntityClass = PlayerClassesUi.Instance.rangerClass)
			aggroModifier = 0.95f;
		else
			aggroModifier = 0.9f;

		PlayerAggroRating aggroStats = new PlayerAggroRating
		{
			player = player,
			aggroModifier = aggroModifier,
			aggroRatingDistance = GetAggroDistanceFromPlayer(player, aggroModifier),
			aggroRatingDamage = (int)(damageRecieved * aggroModifier),
		};

		playerAggroList.Add(aggroStats);
		UpdateAggroList();
	}
	public void AddToAggroRating(PlayerController player, int damageRecieved)
	{
		bool playerAlreadyInAggroList = false;

		foreach (PlayerAggroRating aggroStats in playerAggroList)
		{
			if (aggroStats.player == player)
			{
				aggroStats.aggroRatingDamage += damageRecieved;
				playerAlreadyInAggroList = true;
			}
		}

		if (playerAlreadyInAggroList) return;
		AddPlayerToAggroList(player, damageRecieved);
	}
	private void UpdateAggroList()
	{
		foreach (PlayerAggroRating aggroStats in playerAggroList)
		{
			aggroStats.aggroRatingDistance = GetAggroDistanceFromPlayer(aggroStats.player, aggroStats.aggroModifier);
			aggroStats.aggroRatingTotal = aggroStats.aggroRatingDistance + aggroStats.aggroRatingDamage;
		}

		playerAggroList.Sort((b, a) => a.aggroRatingTotal.CompareTo(b.aggroRatingTotal));
		playerTarget = playerAggroList[0].player;
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
		if (playerTarget != null)
			Gizmos.DrawLine(transform.position, playerTarget.transform.position);
	}
}

[System.Serializable]
public class PlayerAggroRating
{
	public PlayerController player;
	public float aggroModifier;
	public int aggroRatingDistance;
	public int aggroRatingDamage;
	public int aggroRatingTotal;
}
