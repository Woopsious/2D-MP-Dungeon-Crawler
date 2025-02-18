using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityBehaviour : Tree
{
	[Header("Behaviour Info")]
	public SOEntityBehaviour behaviourRef;
	[HideInInspector] public EntityStats entityStats;
	[HideInInspector] public EntityAbilityHandler abilityHandler;
	[HideInInspector] public EntityEquipmentHandler equipmentHandler;
	private Animator animator;
	[HideInInspector] public NavMeshAgent navMeshAgent;
	public bool markedForCleanUp;

	[Header("Bounds Settings")]
	[HideInInspector] public Bounds idleBounds;
	[HideInInspector] public Bounds aggroBounds;
	[HideInInspector] public Bounds chaseBounds;

	[Header("Movement Settings")]
	public float idleTimer;

	[Header("Player Targeting")]
	public LayerMask includeMe;
	public List<PlayerAggroRating> playerAggroList = new List<PlayerAggroRating>();
	public PlayerController playerTarget;
	[HideInInspector] public float distanceToPlayerTarget;
	[HideInInspector] public bool currentPlayerTargetInView;
	[HideInInspector] public Vector2 playersLastKnownPosition;

	[Header("Player Detection")]
	public CircleCollider2D viewRangeCollider;
	private float playerDetectionRange = 30;
	private readonly float playerDetectionCooldown = 0.1f;
	private float playerDetectionTimer;

	[Header("Global Attack Cooldown")]
	public float globalAttackTimer;

	[Header("Prefabs")]
	public GameObject AbilityAoePrefab;
	public GameObject projectilePrefab;

	[Header("Idle Sound Settings")]
	private readonly float playerAggroRatingCooldown = 0.5f;
	private float playerAggroRatingTimer;

	protected virtual void Awake()
	{
		entityStats = GetComponent<EntityStats>();
		abilityHandler = GetComponent<EntityAbilityHandler>();
		equipmentHandler = GetComponent<EntityEquipmentHandler>();
		animator = GetComponent<Animator>();
		navMeshAgent = GetComponent<NavMeshAgent>();
	}
	protected override void Start()
	{
		Initilize();
		base.Start();
	}

	protected override void Update()
	{
		if (entityStats.IsEntityDead()) return;

		UpdateAggroRatingTimer();
		TrackCurrentPlayerTarget();

		abilityHandler.CastAbilityTimer();
		abilityHandler.HealingAbilityCooldownTimer();
		abilityHandler.OffensiveAbilityCooldownTimer();

		GlobalAttackTimer();
		IsPlayerTargetVisibleTimer();
		base.Update();
	}
	protected virtual void FixedUpdate()
	{
		if (entityStats.IsEntityDead()) return;

		aggroBounds.min = new Vector3(transform.position.x - behaviourRef.aggroRange,
			transform.position.y - behaviourRef.aggroRange, transform.position.z);

		aggroBounds.max = new Vector3(transform.position.x + behaviourRef.aggroRange,
		transform.position.y + behaviourRef.aggroRange, transform.position.z);

		chaseBounds.min = new Vector3(transform.position.x - behaviourRef.maxChaseRange,
		transform.position.y - behaviourRef.maxChaseRange, transform.position.z);

		chaseBounds.max = new Vector3(transform.position.x + behaviourRef.maxChaseRange,
		transform.position.y + behaviourRef.maxChaseRange, transform.position.z);

		UpdateSpriteDirection();
		UpdateAnimationState();
	}

	//build Behaviour Tree
	protected override BTNode SetupTree()
	{
		BTNode root = new Selector(new List<BTNode> //entity Behaviour Tree
		{
			new Sequence(new List<BTNode> //attack behaviour
			{
				new CheckPlayerInFOV(this), 
				new TaskTrackPlayer(this),

				new Sequence(new List<BTNode> //attack actions
				{
					new CheckGlobalAttackCooldown(this),
					new Selector(new List<BTNode> //attack actions
					{
						new Sequence(new List<BTNode> //use ability
						{
							new TaskUseAbility(this),
						}),
						new Sequence(new List<BTNode> //weapon attack
						{
							new CheckPlayerInAttackRange(this),
							new TaskWeaponAttack(this),
						}),
					}),
				}),
			}),

			new Sequence(new List<BTNode> //investigate behaviour
			{
				new CheckPlayersLastKnownPos(this),
				new TaskInvestigate(this),
			}),

			new Selector(new List<BTNode> //wander behaviour
			{
				new TaskIdle(this),
				new TaskWander(this),
			}),
		});

		return root;
	}

	//set behaviour data
	protected virtual void Initilize()
	{
		behaviourRef = entityStats.statsRef.entityBehaviour;
		UpdateBounds(transform.position);

		viewRangeCollider.radius = playerDetectionRange;
		viewRangeCollider.gameObject.GetComponent<EntityDetection>().entityBehaviour = this;

		navMeshAgent.speed = behaviourRef.navMeshMoveSpeed;
		navMeshAgent.angularSpeed = behaviourRef.navMeshTurnSpeed;
		navMeshAgent.acceleration = behaviourRef.navMeshAcceleration;
		navMeshAgent.stoppingDistance = behaviourRef.navMeshStoppingDistance;
	}
	public void ResetBehaviour(SpawnHandler spawner)
	{
		UpdateBounds(spawner.transform.position);
		markedForCleanUp = false;
		navMeshAgent.isStopped = false;
		entityStats.equipmentHandler.equippedWeapon.canAttackAgain = true;
		playerAggroList.Clear();
	}
	private void UpdateBounds(Vector3 position)
	{
		idleBounds.min = new Vector3(position.x - behaviourRef.idleWanderRadius,
			position.y - behaviourRef.idleWanderRadius, position.z);

		idleBounds.max = new Vector3(position.x + behaviourRef.idleWanderRadius,
			position.y + behaviourRef.idleWanderRadius, position.z);

		aggroBounds.min = new Vector3(position.x - behaviourRef.aggroRange,
			position.y - behaviourRef.aggroRange, position.z);

		aggroBounds.max = new Vector3(position.x + behaviourRef.aggroRange,
			position.y + behaviourRef.aggroRange, position.z);

		chaseBounds.min = new Vector3(position.x - behaviourRef.maxChaseRange,
			position.y - behaviourRef.maxChaseRange, position.z);

		chaseBounds.max = new Vector3(position.x + behaviourRef.maxChaseRange,
			position.y + behaviourRef.maxChaseRange, position.z);
	}

	private void UpdateSpriteDirection()
	{
        if (playerTarget == null)
        {
			if (navMeshAgent.velocity.x > 0)
				transform.eulerAngles = new Vector3(0, 180, 0);
			else
				transform.eulerAngles = new Vector3(0, 0, 0);
		}
		else
		{
			if (playerTarget.transform.position.x > transform.position.x)
				transform.eulerAngles = new Vector3(0, 180, 0);
			else if (playerTarget.transform.position.x < transform.position.x)
				transform.eulerAngles = new Vector3(0, 0, 0);
		}
	}
	private void UpdateAnimationState()
	{
		if (navMeshAgent.velocity == new Vector3(0,0,0))
			animator.SetBool("isIdle", true);
		else
			animator.SetBool("isIdle", false);
	}

	public void UpdateMovementSpeed(float speedModifier, bool resetSpeed)
	{
		if (resetSpeed)
			navMeshAgent.speed = behaviourRef.navMeshMoveSpeed;
		else 
			navMeshAgent.speed *= speedModifier;
	}

	//player visible Checks + timer
	private void IsPlayerTargetVisibleTimer() //0.1s timer
	{
		playerDetectionTimer -= Time.deltaTime;

		if (playerDetectionTimer <= 0)
		{
			playerDetectionTimer = playerDetectionCooldown;

			//check if player behind obstacles. check if player within aggro/chase ranges based on currentState
			if (PlayerTargetVisible(playerTarget))
			{
				currentPlayerTargetInView = true;
				return;
			}
			else
			{
				currentPlayerTargetInView = false;
				return;
			}
		}
	}
	private bool PlayerTargetVisible(PlayerController player)
	{
		if (player == null) return false; //check null
		RaycastHit2D hit = Physics2D.Linecast(transform.position, player.transform.position, includeMe);

		if (hit.point == null || hit.collider.GetComponent<PlayerController>() == null) //check LoS
			return false;

		if (Vector2.Distance(transform.position, player.transform.position) > behaviourRef.aggroRange) //check aggro range
			return false;
		
		return true;
	}

	//playerTarget checks and pos tracking
	private void TrackCurrentPlayerTarget()
	{
		if (!CurrentPlayerTargetVisible()) return;

		playersLastKnownPosition = playerTarget.transform.position;
		distanceToPlayerTarget = Vector3.Distance(transform.position, playerTarget.transform.position);
	}
	private bool CurrentPlayerTargetVisible()
	{
		if (playerTarget == null) return false;

		if (currentPlayerTargetInView)
			return true;
		else return false;
	}

	//set destination
	public void SetNewDestination(Vector2 destination)
	{
		navMeshAgent.SetDestination(destination);
	}

	//global attack timer
	private void GlobalAttackTimer()
	{
		if (globalAttackTimer >= 0)
			globalAttackTimer -= Time.deltaTime;
	}

	//AGGRO LIST
	private void UpdateAggroRatingTimer()
	{
		if (playerAggroList.Count <= 0)
		{
			playerTarget = null;
			return;
		}

		playerAggroRatingTimer -= Time.deltaTime;

		if (playerAggroRatingTimer <= 0)
		{
			playerAggroRatingTimer = playerAggroRatingCooldown;
			UpdateAggroList();
		}
	}
	private void UpdateAggroList()
	{
		foreach (PlayerAggroRating aggroStats in playerAggroList)
		{
			aggroStats.aggroRatingDistance = GetAggroDistanceFromPlayer(aggroStats.player, aggroStats.aggroModifier);
			aggroStats.aggroRatingTotal = aggroStats.aggroRatingDistance + aggroStats.aggroRatingDamage;
		}

		playerAggroList.Sort((b, a) => a.aggroRatingTotal.CompareTo(b.aggroRatingTotal));
		SetCurrentPlayerTarget();
	}
	private void SetCurrentPlayerTarget()
	{
		for (int i = 0; i < playerAggroList.Count; i++)
		{
			if (PlayerTargetVisible(playerAggroList[i].player))
			{
				playerTarget = playerAggroList[i].player;
				return;
			}
		}
		playerTarget = null;
	}

	//update values
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
	private int GetAggroDistanceFromPlayer(PlayerController player, float aggroModifier)
	{
		float distance = Vector2.Distance(transform.position, player.transform.position);
		float aggroRating = (entityStats.maxHealth.finalValue * aggroModifier) / distance;
		return (int)aggroRating;
	}

	//adding/removing players
	public void AddPlayerToAggroList(PlayerController player, int damageRecieved)
	{
		foreach (PlayerAggroRating aggroRating in playerAggroList)
			if (aggroRating.player == player)
				return;

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
	public void RemovePlayerFromAggroList(PlayerController player)
	{
		for (int i = playerAggroList.Count - 1; i >= 0; i--)
		{
			if (playerAggroList[i].player == player)
				playerAggroList.RemoveAt(i);
		}
	}

	//utility
	public void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(idleBounds.center, idleBounds.extents.x);

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(aggroBounds.center, aggroBounds.extents.x);
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
