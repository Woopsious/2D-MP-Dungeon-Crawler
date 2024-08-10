using NavMeshPlus.Extensions;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
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
	private LayerMask playerLayerMask;
	private LayerMask entityLayerMask;
	public List<PlayerAggroRating> playerAggroList = new List<PlayerAggroRating>();
	public PlayerController playerTarget;
	public bool currentPlayerTargetInView;
	public Vector2 playersLastKnownPosition;

	[Header("Player Detection")]
	public CircleCollider2D viewRangeCollider;
	public float playerDetectionRange;
	private readonly float playerDetectionCooldown = 0.1f;
	private float playerDetectionTimer;

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
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		navMeshAgent = GetComponent<NavMeshAgent>();
		playerLayerMask = 1 << 8;
		entityLayerMask = 1 << 9;
	}
	private void Start()
	{
		Initilize();
	}

	private void OnEnable()
	{
		entityStats.OnHealthChangeEvent += CastHealingAbility;
	}
	private void OnDisable()
	{
		entityStats.OnHealthChangeEvent -= CastHealingAbility;
	}

	private void Update()
	{
		if (entityStats.IsEntityDead()) return;
		currentState.UpdateLogic(this);

		UpdateAggroRatingTimer();
		CheckIfPlayerVisibleTimer();

		HealingAbilityTimer();
		OffensiveAbilityTimer();
	}
	private void FixedUpdate()
	{
		if (entityStats.IsEntityDead()) return;
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

	//player visible Check
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

	//shared idle/attack behaviour 
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

	//ABILITIES
	//casting of each ability
	public void CastHealingAbility(int maxHealth, int currentHealth)
	{
		if (maxHealth == 0) return;
		if (!canCastHealingAbility || healingAbility == null) return;

		int healthPercentage = (int)((float)currentHealth / maxHealth * 100);
		if (healthPercentage > 50) return;

		canCastHealingAbility = false;
		healingAbilityTimer = healingAbility.abilityCooldown;
		CastEffect(healingAbility);
	}
	public void CastOffensiveAbility()
	{
		if (!canCastOffensiveAbility || offensiveAbility == null) return;
		if (playerTarget == null) return;

		if (offensiveAbility.statusEffectType != SOClassAbilities.StatusEffectType.noEffect)
			CastEffect(offensiveAbility);
		else
		{
			if (offensiveAbility.isProjectile)
				CastDirectionalAbility(offensiveAbility);
			else if (offensiveAbility.isAOE)
				CastAoeAbility(offensiveAbility);
		}

		canCastOffensiveAbility = false;
		offensiveAbilityTimer = offensiveAbility.abilityCooldown; // /2 to simulate having 5 equipped to hotbar
		return;
	}

	//casting of ability types
	public void CastEffect(SOClassAbilities ability)
	{
		if (ability.damageType == SOClassAbilities.DamageType.isHealing)
		{
			//eventually add support to heal friendlies
			entityStats.OnHeal(ability.damageValuePercentage, true, entityStats.healingPercentageModifier.finalPercentageValue);
		}
		else
		{
			if (ability.canOnlyTargetSelf || !ability.isOffensiveAbility) //add check to cast on friendly
				entityStats.ApplyStatusEffect(ability);
			else if (ability.isOffensiveAbility && playerTarget != null)
				playerTarget.playerStats.ApplyStatusEffect(ability);
		}
	}
	public void CastDirectionalAbility(SOClassAbilities ability)
	{
		GameObject go = Instantiate(projectilePrefab, transform, true);
		go.transform.SetParent(null);
		go.transform.position = (Vector2)transform.position;
		go.GetComponent<Projectiles>().Initilize(ability, entityStats);

		Vector3 rotation = playerTarget.transform.position - transform.position;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		go.transform.rotation = Quaternion.Euler(0, 0, rotz - 90);
	}
	public void CastAoeAbility(SOClassAbilities ability)
	{
		GameObject go = Instantiate(AbilityAoePrefab, transform, true);
		go.transform.SetParent(null);
		go.transform.position = playerTarget.transform.position;
		go.GetComponent<AbilityAOE>().Initilize(ability, entityStats);
		go.GetComponent<AbilityAOE>().AddPlayerRef(null);
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
