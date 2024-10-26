using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityBehaviour : Tree
{
	[Header("Behaviour Info")]
	public bool useStateMachineBehaviour;
	public SOEntityBehaviour behaviourRef;
	[HideInInspector] public EntityStats entityStats;
	[HideInInspector] public EntityEquipmentHandler equipmentHandler;

	[Header("Entity States")]
	public EnemyBaseState currentState;
	[HideInInspector] public EnemyIdleState idleState = new EnemyIdleState();
	[HideInInspector] public EnemyWanderState wanderState = new EnemyWanderState();
	[HideInInspector] public EnemyAttackState attackState = new EnemyAttackState();

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
	public float distanceToPlayerTarget;
	public bool currentPlayerTargetInView;
	public Vector2 playersLastKnownPosition;

	[Header("Player Detection")]
	public CircleCollider2D viewRangeCollider;
	private float playerDetectionRange = 30;
	private readonly float playerDetectionCooldown = 0.1f;
	private float playerDetectionTimer;

	[Header("Global Attack Cooldown")]
	public float globalAttackTimer;

	[Header("Healing Ability Cooldown")]
	public SOClassAbilities healingAbility;
	public bool canCastHealingAbility;
	public float healingAbilityTimer;

	[Header("Offensive Ability Cooldown")]
	public SOClassAbilities offensiveAbility;
	public bool canCastOffensiveAbility;
	public float offensiveAbilityTimer;

	[Header("Prefabs")]
	public GameObject AbilityAoePrefab;
	public GameObject projectilePrefab;

	[Header("Idle Sound Settings")]
	private readonly float playerAggroRatingCooldown = 0.5f;
	private float playerAggroRatingTimer;

	private void Awake()
	{
		entityStats = GetComponent<EntityStats>();
		equipmentHandler = GetComponent<EntityEquipmentHandler>();
		animator = GetComponent<Animator>();
		navMeshAgent = GetComponent<NavMeshAgent>();
	}
	protected override void Start()
	{
		useStateMachineBehaviour = false;
		Initilize();

		if (useStateMachineBehaviour)
			return;
		else if (!useStateMachineBehaviour)
			base.Start();
	}

	private void OnEnable()
	{
		if (useStateMachineBehaviour)
			entityStats.OnHealthChangeEvent += TryCastHealingAbility;
	}
	private void OnDisable()
	{
		if (useStateMachineBehaviour)
			entityStats.OnHealthChangeEvent -= TryCastHealingAbility;
	}

	protected override void Update()
	{
		if (entityStats.IsEntityDead()) return;

		UpdateAggroRatingTimer();
		TrackCurrentPlayerTarget();

		HealingAbilityTimer();
		OffensiveAbilityTimer();

		if (useStateMachineBehaviour)
			currentState.UpdateLogic(this);
		else
		{
			GlobalAttackTimer();
			IsPlayerTargetVisibleTimer();
			base.Update();
		}
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

		if (useStateMachineBehaviour)
			currentState.UpdateLogic(this);
		else
			return;
	}

	//BehaviourTree related functions
	//build Basic Behaviour Tree
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
		UpdateBounds(transform.position);
		ResetBehaviour();

		markedForCleanUp = false;

		viewRangeCollider.radius = playerDetectionRange;
		viewRangeCollider.gameObject.GetComponent<EntityDetection>().entityBehaviour = this;

		navMeshAgent.speed = behaviourRef.navMeshMoveSpeed;
		navMeshAgent.angularSpeed = behaviourRef.navMeshTurnSpeed;
		navMeshAgent.acceleration = behaviourRef.navMeshAcceleration;
		navMeshAgent.stoppingDistance = behaviourRef.navMeshStoppingDistance;
	}
	public void ResetBehaviour()
	{
		markedForCleanUp = false;
		navMeshAgent.isStopped = false;
		entityStats.equipmentHandler.equippedWeapon.canAttackAgain = true;
		playerAggroList.Clear();
		ChangeState(wanderState);
	}
	public void UpdateBounds(Vector3 position)
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
	public bool CurrentPlayerTargetVisible()
	{
		if (playerTarget == null) return false;

		if (currentPlayerTargetInView)
			return true;
		else return false;
	}
	private void TrackCurrentPlayerTarget()
	{
		if (!CurrentPlayerTargetVisible()) return;

		playersLastKnownPosition = playerTarget.transform.position;
		distanceToPlayerTarget = Vector3.Distance(transform.position, playerTarget.transform.position);
	}

	//MOVEMENT
	public void SetNewDestination(Vector2 destination)
	{
		navMeshAgent.SetDestination(destination);
	}
	public bool HasReachedDestination()
	{
		if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
			return true;
		else
			return false;
	}

	//ATTACKING
	private void GlobalAttackTimer()
	{
		if (globalAttackTimer >= 0)
			globalAttackTimer -= Time.deltaTime;
	}
	public void TryAttackWithMainWeapon()
	{
		if (equipmentHandler.equippedWeapon == null) return;

		float maxDistanceToCheck = equipmentHandler.equippedWeapon.weaponBaseRef.maxAttackRange;
		if (!equipmentHandler.equippedWeapon.weaponBaseRef.isRangedWeapon && entityStats.statsRef.isBossVersion)
			maxDistanceToCheck = equipmentHandler.equippedWeapon.weaponBaseRef.maxAttackRange * 2;

		if (distanceToPlayerTarget > maxDistanceToCheck) return;

		if (equipmentHandler.equippedWeapon.weaponBaseRef.isRangedWeapon)
			equipmentHandler.equippedWeapon.RangedAttack(playerTarget.transform.position, projectilePrefab);
		else
			equipmentHandler.equippedWeapon.MeleeAttack(playerTarget.transform.position);
	}

	//ENEMY AGGRO LIST
	//sort + update list then set playerTarget
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

	//updating damage/distance values
	private int GetAggroDistanceFromPlayer(PlayerController player, float aggroModifier)
	{
		float distance = Vector2.Distance(transform.position, player.transform.position);
		float aggroRating = (entityStats.maxHealth.finalValue * aggroModifier) / distance;
		return (int)aggroRating;
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

	//ABILITIES
	//casting of each ability
	public void TryCastHealingAbility(int maxHealth, int currentHealth)
	{
		if (entityStats.statsRef.isBossVersion) return;
		if (healingAbility == null || !canCastHealingAbility || maxHealth == 0) return;

		int healthPercentage = (int)((float)currentHealth / maxHealth * 100);
		if (healthPercentage > 50) return;

		canCastHealingAbility = false;
		healingAbilityTimer = healingAbility.abilityCooldown;
		CastEffect(healingAbility);
	}
	public void TryCastOffensiveAbility()
	{
		if (offensiveAbility == null || !canCastOffensiveAbility) return;
		if (playerTarget == null) return;

		if (!HasEnoughManaToCast(offensiveAbility))
		{
			canCastOffensiveAbility = false;
			offensiveAbilityTimer = 2.5f;	//if low mana wait 2.5s then try again
			return;
		}

		if (offensiveAbility.hasStatusEffects)
			CastEffect(offensiveAbility);
		else
		{
			if (offensiveAbility.isProjectile)
				CastDirectionalAbility(offensiveAbility);
			else if (offensiveAbility.isAOE)
				CastAoeAbility(offensiveAbility);
		}

		canCastOffensiveAbility = false;
		offensiveAbilityTimer = offensiveAbility.abilityCooldown;
		return;
	}

	//casting of ability types
	public bool HasEnoughManaToCast(SOClassAbilities ability)
	{
		if (ability.isSpell)
		{
			int totalManaCost = (int)(ability.manaCost * entityStats.levelModifier);
			if (entityStats.currentMana <= totalManaCost)
				return false;
            else return true;
        }
		else
			return true;
	}
	protected void CastEffect(SOClassAbilities ability)
	{
		if (ability.damageType == SOClassAbilities.DamageType.isHealing)
		{
			//eventually add support to heal friendlies
			entityStats.OnHeal(ability.damageValuePercentage, true, entityStats.healingPercentageModifier.finalPercentageValue);
		}

		if (ability.damageValue != 0)    //apply damage for insta damage abilities
		{
			playerTarget.GetComponent<Damageable>().OnHitFromDamageSource(null, GetComponent<Collider2D>(), ability.damageValue
				* entityStats.levelModifier, (IDamagable.DamageType)ability.damageType, 0, false, true, false);
		}

		if (ability.hasStatusEffects)    //apply effects (if has any) based on what type it is.
		{
			//apply effects based on what type it is.
			if (ability.canOnlyTargetSelf)
				entityStats.ApplyNewStatusEffects(ability.statusEffects, entityStats);
			else if (ability.isOffensiveAbility && playerTarget != null)
				playerTarget.playerStats.ApplyNewStatusEffects(ability.statusEffects, entityStats);
			else if (!ability.isOffensiveAbility)         //add support/option to buff other friendlies
				entityStats.ApplyNewStatusEffects(ability.statusEffects, entityStats);
		}

		OnSuccessfulCast(ability);
	}
	protected void CastDirectionalAbility(SOClassAbilities ability)
	{
		Projectiles projectile = DungeonHandler.GetProjectile();
		if (projectile == null)
		{
			GameObject go = Instantiate(projectilePrefab, transform, true);
			projectile = go.GetComponent<Projectiles>();
		}

		projectile.transform.SetParent(null);
		projectile.SetPositionAndAttackDirection(transform.position, playerTarget.transform.position);
		projectile.Initilize(null, ability, entityStats);
		OnSuccessfulCast(ability);
	}
	protected void CastAoeAbility(SOClassAbilities ability)
	{
		AbilityAOE abilityAOE = DungeonHandler.GetAoeAbility();
		if (abilityAOE == null)
		{
			GameObject go = Instantiate(AbilityAoePrefab, transform, true);
			abilityAOE = go.GetComponent<AbilityAOE>();
		}

		abilityAOE.transform.SetParent(null);
		abilityAOE.transform.position = playerTarget.transform.position;
		abilityAOE.Initilize(ability, entityStats);
		abilityAOE.AddPlayerRef(null);

		OnSuccessfulCast(ability);
	}
	protected void OnSuccessfulCast(SOClassAbilities ability)
	{
		if (ability.isSpell)
		{
			int totalManaCost = (int)(ability.manaCost * entityStats.levelModifier);
			entityStats.DecreaseMana(totalManaCost, false);
		}
	}

	//ability type cooldowns
	private void HealingAbilityTimer()
	{
		if (canCastHealingAbility) return;

		healingAbilityTimer -= Time.deltaTime;

		if (healingAbilityTimer <= 0)
			canCastHealingAbility = true;
	}
	private void OffensiveAbilityTimer()
	{
		if (canCastOffensiveAbility) return;

		offensiveAbilityTimer -= Time.deltaTime;

		if (offensiveAbilityTimer <= 0)
			canCastOffensiveAbility = true;
	}

	//STATE CHANGES
	public virtual void ChangeState(EnemyBaseState newState)
	{
		currentState?.Exit(this);
		currentState = newState;
		currentState.Enter(this);
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
