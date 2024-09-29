using NavMeshPlus.Extensions;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;

public class EntityBehaviour : MonoBehaviour
{
	[Header("Behaviour Info")]
	public SOEntityBehaviour entityBehaviour;
	[HideInInspector] public EntityStats entityStats;
	private SpriteRenderer spriteRenderer;

	[Header("Entity States")]
	protected EnemyBaseState currentState;
	[HideInInspector] public EnemyIdleState idleState = new EnemyIdleState();
	[HideInInspector] public EnemyWanderState wanderState = new EnemyWanderState();
	[HideInInspector] public EnemyAttackState attackState = new EnemyAttackState();

	private Rigidbody2D rb;
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
	private float distanceToPlayerTarget;
	private bool currentPlayerTargetInView;
	public Vector2 playersLastKnownPosition;

	[Header("Player Detection")]
	public CircleCollider2D viewRangeCollider;
	private float playerDetectionRange;
	private readonly float playerDetectionCooldown = 0.1f;
	private float playerDetectionTimer;

	[Header("Healing Ability Cooldown")]
	public SOClassAbilities healingAbility;
	private bool canCastHealingAbility;
	private float healingAbilityTimer;

	[Header("Offensive Ability Cooldown")]
	public SOClassAbilities offensiveAbility;
	private bool canCastOffensiveAbility;
	private float offensiveAbilityTimer;

	[Header("Prefabs")]
	public GameObject AbilityAoePrefab;
	public GameObject projectilePrefab;

	[Header("Idle Sound Settings")]
	private readonly float playerAggroRatingCooldown = 0.5f;
	private float playerAggroRatingTimer;

	private void Awake()
	{
		entityStats = GetComponent<EntityStats>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		navMeshAgent = GetComponent<NavMeshAgent>();
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

	protected virtual void Update()
	{
		if (entityStats.IsEntityDead()) return;
		currentState.UpdateLogic(this);

		UpdateAggroRatingTimer();
		TrackCurrentPlayerTarget();
		IsPlayerTargetVisibleTimer();

		HealingAbilityTimer();
		OffensiveAbilityTimer();
	}
	protected virtual void FixedUpdate()
	{
		if (entityStats.IsEntityDead()) return;
		currentState.UpdatePhysics(this);

		aggroBounds.min = new Vector3(transform.position.x - entityBehaviour.aggroRange,
			transform.position.y - entityBehaviour.aggroRange, transform.position.z);

		aggroBounds.max = new Vector3(transform.position.x + entityBehaviour.aggroRange,
		transform.position.y + entityBehaviour.aggroRange, transform.position.z);

		chaseBounds.min = new Vector3(transform.position.x - entityBehaviour.maxChaseRange,
		transform.position.y - entityBehaviour.maxChaseRange, transform.position.z);

		chaseBounds.max = new Vector3(transform.position.x + entityBehaviour.maxChaseRange,
		transform.position.y + entityBehaviour.maxChaseRange, transform.position.z);

		UpdateSpriteDirection();
		UpdateAnimationState();
	}

	//set behaviour data
	protected virtual void Initilize()
	{
		UpdateBounds(transform.position);
		ResetBehaviour();

		markedForCleanUp = false;

		viewRangeCollider.radius = playerDetectionRange;
		viewRangeCollider.gameObject.GetComponent<EntityDetection>().entityBehaviour = this;

		navMeshAgent.speed = entityBehaviour.navMeshMoveSpeed;
		navMeshAgent.angularSpeed = entityBehaviour.navMeshTurnSpeed;
		navMeshAgent.acceleration = entityBehaviour.navMeshAcceleration;
		navMeshAgent.stoppingDistance = entityBehaviour.navMeshStoppingDistance;
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
		idleBounds.min = new Vector3(position.x - entityBehaviour.idleWanderRadius,
			position.y - entityBehaviour.idleWanderRadius, position.z);

		idleBounds.max = new Vector3(position.x + entityBehaviour.idleWanderRadius,
			position.y + entityBehaviour.idleWanderRadius, position.z);

		aggroBounds.min = new Vector3(position.x - entityBehaviour.aggroRange,
			position.y - entityBehaviour.aggroRange, position.z);

		aggroBounds.max = new Vector3(position.x + entityBehaviour.aggroRange,
			position.y + entityBehaviour.aggroRange, position.z);

		chaseBounds.min = new Vector3(position.x - entityBehaviour.maxChaseRange,
			position.y - entityBehaviour.maxChaseRange, position.z);

		chaseBounds.max = new Vector3(position.x + entityBehaviour.maxChaseRange,
			position.y + entityBehaviour.maxChaseRange, position.z);
	}

	private void UpdateSpriteDirection()
	{
		if (navMeshAgent.velocity.x > 0)
			transform.eulerAngles = new Vector3(0, 180, 0);
		else
			transform.eulerAngles = new Vector3(0, 0, 0);
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
			navMeshAgent.speed = entityBehaviour.navMeshMoveSpeed;
		else 
			navMeshAgent.speed *= speedModifier;
	}

	//player visible Checks + timer
	private void IsPlayerTargetVisibleTimer() //0.1s timer
	{
		if (playerTarget == null)
			return;

		playerDetectionTimer -= Time.deltaTime;

		if (playerDetectionTimer <= 0)
		{
			playerDetectionTimer = playerDetectionCooldown;

			//check if player behind obstacles. check if player within aggro/chase ranges based on currentState
			if (PlayerVisible(playerTarget) && PlayerWithinRange(playerTarget))
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
	private bool PlayerVisible(PlayerController player)
	{
		RaycastHit2D hit = Physics2D.Linecast(transform.position, player.transform.position, includeMe);

		if (hit.point != null && hit.collider.GetComponent<PlayerController>() != null)
			return true;
		else return false;
	}
	private bool PlayerWithinRange(PlayerController player)
	{
		if (currentState == idleState || currentState == wanderState)
		{
			if (Vector2.Distance(transform.position, player.transform.position) < entityBehaviour.aggroRange)
				return true;
			else return false;
		}
		else
		{
			if (Vector2.Distance(transform.position, player.transform.position) < entityBehaviour.maxChaseRange)
				return true;
			else return false;
		}
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
	public void AttackWithMainWeapon()
	{
		Weapons weapon = entityStats.equipmentHandler.equippedWeapon;
		if (weapon == null || !weapon.canAttackAgain) return;

		float distanceToCheck = weapon.weaponBaseRef.maxAttackRange;
		if (entityStats.entityBaseStats.isBossVersion)
			distanceToCheck = weapon.weaponBaseRef.maxAttackRange * 2;

		if (distanceToPlayerTarget > distanceToCheck) return;

		if (weapon.weaponBaseRef.isRangedWeapon)
			weapon.RangedAttack(playerTarget.transform.position, projectilePrefab);
		else
			weapon.MeleeAttack(playerTarget.transform.position);
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
			if (PlayerVisible(playerAggroList[i].player) && PlayerWithinRange(playerAggroList[i].player))
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
	public void CastHealingAbility(int maxHealth, int currentHealth)
	{
		if (entityStats.entityBaseStats.isBossVersion) return;

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
	protected bool HasEnoughManaToCast(SOClassAbilities ability)
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
	private void OnSuccessfulCast(SOClassAbilities ability)
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
	public void ChangeState(EnemyBaseState newState)
	{
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
