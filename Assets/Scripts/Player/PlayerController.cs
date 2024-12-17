using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("Debug settings")]
	public bool debugSetStartingItems;
	public bool debugUseSelectedTargetForAttackDirection;
	public bool debugSetPlayerLevelOnStart;
	public int debugPlayerLevel;

	[Header("Player Info")]
	public Camera playerCamera;
	public LayerMask includeMe;
	[HideInInspector] public EntityStats playerStats;
	[HideInInspector] public EntityClassHandler playerClassHandler;
	[HideInInspector] public PlayerEquipmentHandler playerEquipmentHandler;
	[HideInInspector] public PlayerExperienceHandler playerExperienceHandler;
	[HideInInspector] public EntityDetection enemyDetection;
	private PlayerInputHandler playerInputs;
	private Rigidbody2D rb;
	private Animator animator;

	private float speed = 12;

	//main attack auto attack timer
	private readonly float mainAttackAutoAttackCooldown = 0.25f;
	private float mainAttackAutoAttackTimer;

	[Header("Player Targeting")] // +info
	public EntityStats selectedEnemyTarget;
	private int selectedEnemyTargetIndex;
	private List<EnemyDistance> EnemyTargetList = new List<EnemyDistance>();

	//target selected event
	public event Action<EntityStats> OnNewTargetSelected;

	//targetlist update timer
	private readonly float updateTargetListCooldown = 0.5f;
	private float updateTargetListTimer;

	[Header("Ability Prefabs")] // +info
	public GameObject AbilityAoePrefab;
	public GameObject projectilePrefab;

	//ability events
	public static event Action<Abilities> OnPlayerUseAbility;
	public static event Action OnPlayerCastAbility;
	public static event Action OnPlayerCancelAbility;

	//ability 
	private Abilities queuedAbility;
	private Abilities abilityBeingCasted;
	private float abilityCastingTimer;

	[Header("Marked By Boss")]
	public GameObject PlayerBossMarker;

	//interactions
	[HideInInspector] public bool isInteractingWithInteractable;
	[HideInInspector] public Interactables currentInteractedObject;

	private void Awake()
	{
		playerStats = GetComponent<EntityStats>();
		playerClassHandler = GetComponent<EntityClassHandler>();
		playerEquipmentHandler = GetComponent<PlayerEquipmentHandler>();
		playerExperienceHandler = GetComponent<PlayerExperienceHandler>();
		playerEquipmentHandler.player = this;
		enemyDetection = GetComponentInChildren<EntityDetection>();
		enemyDetection.player = this;
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	}
	private void Start()
	{
		Initilize();

		OnNewTargetSelected += PlayerHotbarUi.Instance.OnNewTargetSelected;

		playerStats.OnNewStatusEffect += PlayerHotbarUi.Instance.OnNewStatusEffectsForPlayer;
		playerStats.OnResetStatusEffectTimer += PlayerHotbarUi.Instance.OnResetStatusEffectTimerForPlayer;
	}

	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadPlayerInfo;
		DungeonHandler.OnEntityDeathEvent += OnSelectedTargetDeath;
	}
	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadPlayerInfo;
		DungeonHandler.OnEntityDeathEvent -= OnSelectedTargetDeath;

		OnNewTargetSelected -= PlayerHotbarUi.Instance.OnNewTargetSelected;

		playerStats.OnNewStatusEffect -= PlayerHotbarUi.Instance.OnNewStatusEffectsForPlayer;
		playerStats.OnResetStatusEffectTimer -= PlayerHotbarUi.Instance.OnResetStatusEffectTimerForPlayer;
	}

	private void Update()
	{
		playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, playerCamera.transform.position.z);

		if (playerStats.IsEntityDead() || IsPlayerInteracting()) return;

		UpdateTargetsInList();
		AutoAttackTimer();
		AbilityCastingTimer();
	}
	private void FixedUpdate()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting()) return;

		PlayerMovement();
	}

	//set player data
	public void Initilize()
	{
		if (playerInputs == null)
			playerInputs = PlayerInputHandler.Instance;

		playerCamera.transform.parent = null;
		PlayerBossMarker.SetActive(false);

		if (debugSetPlayerLevelOnStart)
			playerStats.entityLevel = debugPlayerLevel;
		else
			playerStats.entityLevel = 1;
		PlayerEventManager.PlayerLevelUp(playerStats);
		playerStats.CalculateBaseStats();
	}

	public void ResetPlayerAfterDeath()
	{
		playerStats.CalculateBaseStats();
	}
	private void ReloadPlayerInfo()
	{
		playerStats.entityLevel = SaveManager.Instance.GameData.playerLevel;
		if (playerStats.entityLevel == 0)
			playerStats.entityLevel += 1;
		playerStats.CalculateBaseStats();
		PlayerEventManager.PlayerLevelUp(playerStats);
	}

	//movement
	private void PlayerMovement()
	{
		rb.velocity = new Vector2(playerInputs.MovementInput.x * speed, playerInputs.MovementInput.y * speed);

		UpdateSpriteDirection();
		UpdateAnimationState();
	}
	public void UpdateMovementSpeed(float speedModifier, bool resetSpeed)
	{
		if (resetSpeed)
			speed = 12;
		else
			speed *= speedModifier;
	}
	private void UpdateSpriteDirection()
	{
		if (rb.velocity.x > 0.01 && rb.velocity.x != 0)
			transform.eulerAngles = new Vector3(0, 0, 0);
		else if (rb.velocity.x < -0.01 && rb.velocity.x != 0)
			transform.eulerAngles = new Vector3(0, 180, 0);
	}
	private void UpdateAnimationState()
	{
		if (rb.velocity == new Vector2(0, 0))
			animator.SetBool("isIdle", true);
		else
			animator.SetBool("isIdle", false);
	}

	//player auto attack
	private void AutoAttackTimer()
	{
		if (!PlayerSettingsManager.Instance.mainAttackIsAutomatic) return;
		if (EnemyTargetList.Count == 0) return;
		if (playerEquipmentHandler.equippedWeapon == null) return;

		mainAttackAutoAttackTimer -= Time.deltaTime;
		if (mainAttackAutoAttackTimer < 0)
		{
			//reset cooldown timer + extra 0.25s delay, making manual attack better
			AutoAttackWithMainWeapon();
		}
	}
	private void AutoAttackWithMainWeapon()
	{
		//auto attack with main weapon, aiming for players selected target, if too close or out of range, attack closest target instead
		//if no selected target aim for closest enemy (ranged weapon aim for closest enemy outside of min attack range if possible)

		Weapons weapon = playerEquipmentHandler.equippedWeapon;
		EntityStats entityToAttack = EnemyTargetList[0].entity; //grab closest enemy as default
		mainAttackAutoAttackTimer = weapon.weaponBaseRef.baseAttackSpeed + mainAttackAutoAttackCooldown;

		if (weapon.weaponBaseRef.isRangedWeapon)	//ranged weapon logic
		{
			if (selectedEnemyTarget != null)
			{
				weapon.RangedAttack(selectedEnemyTarget.transform.position, projectilePrefab);
			}
			else	//if player selected target null, try find one within min and max attack range
			{
				foreach (EnemyDistance enemy in EnemyTargetList)
				{
					if (enemy.distance > weapon.weaponBaseRef.minAttackRange && enemy.distance < weapon.weaponBaseRef.maxAttackRange)
						entityToAttack = enemy.entity;
				}

				if (GrabDistanceToEntity(entityToAttack) <= weapon.weaponBaseRef.maxAttackRange)
					weapon.RangedAttack(entityToAttack.transform.position, projectilePrefab);
			}
		}
		else	//melee weapon logic
		{
			if (selectedEnemyTarget != null && GrabDistanceToEntity(selectedEnemyTarget) < weapon.weaponBaseRef.maxAttackRange)
			{
				weapon.MeleeAttack(selectedEnemyTarget.transform.position);
			}
			else    //if player selected target null && out of range, attack closest enemy set at start of func
			{
				if (GrabDistanceToEntity(entityToAttack) <= weapon.weaponBaseRef.maxAttackRange)
					weapon.MeleeAttack(entityToAttack.transform.position);
			}
		}
	}

	//PLAYER TARGETING OPTIONS
	//mouse select targeting
	private void CheckForSelectableTarget()
	{
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 1000, includeMe);
		if (hit.collider == null)
			return;
		if (hit.collider.GetComponent<EntityStats>() == null)
			return;

		EntityStats entityStats = hit.collider.GetComponent<EntityStats>();
		if (!entityStats.IsPlayerEntity())
		{
			for (int i = 0; i < EnemyTargetList.Count; i++)
			{
				if (entityStats == EnemyTargetList[i].entity)
				{
					SetNewSelectedEnemyTarget(i);
					return;
				}
			}
		}

		//find corrisponding target in target list, set index to index of target in list
	}
	private void OnSelectedTargetDeath(GameObject obj)
	{
		if (selectedEnemyTarget == null) return;
		if (selectedEnemyTarget.gameObject != obj) return;

		ClearSelectedTarget();
	}
	public void ClearSelectedTarget()
	{
		selectedEnemyTarget = null;
		selectedEnemyTargetIndex = 0;
	}

	//tab targeting
	private void CycleTargetsForwards(int startingIndex)
	{
		//for next target in target list, if can see that target (with raycast) select that enemy as new target, if not ++
		if (EnemyTargetList.Count == 0) return;

		for (int i = startingIndex;  i <= EnemyTargetList.Count - 1; i++)
		{
			if (!CheckIfTargetVisibleOnCycleTargets(EnemyTargetList[i].entity))
				continue;

			SetNewSelectedEnemyTarget(i);
			break;
		}
	}
	private void CycleTargetsBackwards(int startingIndex)
	{
		//for previous target in target list, if can see that target (with raycast) select that enemy as new target if not --
		if (EnemyTargetList.Count == 0) return;

		for (int i = startingIndex; i <= EnemyTargetList.Count - 1; i--)
		{
			if (!CheckIfTargetVisibleOnCycleTargets(EnemyTargetList[i].entity))
				continue;

			SetNewSelectedEnemyTarget(i);
			break;
		}
	}
	private void SetNewSelectedEnemyTarget(int index)
	{
		OnNewTargetSelected?.Invoke(EnemyTargetList[index].entity);
		selectedEnemyTarget = EnemyTargetList[index].entity;
		selectedEnemyTargetIndex = index;
	}

	//targeting updates
	public void AddNewEnemyTargetToList(EntityStats entity)
	{
		//add new enemy to list, then update targets
		EnemyDistance enemy = new(entity.statsRef.name, 
			entity.classHandler.currentEntityClass.name, entity, GrabDistanceToEntity(entity));
		EnemyTargetList.Add(enemy);
		UpdateSelectedTargetIndexOnListChanges();
	}
	public void RemoveEnemyTargetFromList(EntityStats entity)
	{
		//remove enemy from list, then update targets

		for (int i = EnemyTargetList.Count - 1; i >= 0; i--)
		{
			if (EnemyTargetList[i].entity == entity)
				EnemyTargetList.RemoveAt(i);
		}
		UpdateSelectedTargetIndexOnListChanges();
	}
	private void UpdateTargetsInList()
	{
		//every x amount of seconds reorder list based on distance to player, updating current index with new
		//foreach enemy in target list if enemy = enemy in target list, index = enemy index in list
		if (EnemyTargetList.Count == 0) return;
		updateTargetListTimer -= Time.deltaTime;
		if (updateTargetListTimer > 0)
			return;

		foreach (EnemyDistance enemy in EnemyTargetList)
			enemy.distance = GrabDistanceToEntity(enemy.entity);

		EnemyTargetList.Sort((a, b) => a.distance.CompareTo(b.distance));
		UpdateSelectedTargetIndexOnListChanges();
		updateTargetListTimer = updateTargetListCooldown;
	}
	private void UpdateSelectedTargetIndexOnListChanges()
	{
		for (int i = 0; i < EnemyTargetList.Count - 1; i++)
		{
			if (selectedEnemyTarget == EnemyTargetList[i].entity)
			{
				selectedEnemyTargetIndex = i;
				return;
			}
		}
	}
	private float GrabDistanceToEntity(EntityStats entity)
	{
		float distance = Vector2.Distance(transform.position, entity.transform.position);
		return distance;
	}
	private bool CheckIfTargetVisibleOnCycleTargets(EntityStats entity)
	{
		//raycat to enemy, if hit return true, else false
		RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, entity.transform.position, includeMe);

		foreach (RaycastHit2D hit in hits)
		{
			if (hit.point != null && hit.collider.gameObject == entity.gameObject)
				return true;
		}
		return false;
	}

	//PLAYER ABILITY CASTING
	//casting events
	private void UseAbility(Abilities ability)
	{
		OnPlayerUseAbility?.Invoke(ability);
		queuedAbility = ability;
	}
	private void CastAbility()
	{
		abilityBeingCasted = queuedAbility;
		abilityCastingTimer = queuedAbility.abilityBaseRef.abilityCastingTimer;
		OnPlayerCastAbility?.Invoke();
	}
	private EntityStats TryGrabNewEntityOnQueuedAbilityClick(bool lookingForFriendly)	//add support/option to handle friendly targets
	{
		EntityStats newEntity;
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 50, includeMe);

		if (hit.transform == null || hit.transform.gameObject.GetComponent<EntityStats>() == null)
		{
			//Debug.Log("no obj or entity found at location");
			return null;
		}

		newEntity = hit.transform.gameObject.GetComponent<EntityStats>();

		if (newEntity.IsPlayerEntity() && lookingForFriendly)
			return newEntity;
		else if (!newEntity.IsPlayerEntity() && !lookingForFriendly)
			return newEntity;
		else
		{
			//Debug.Log("entity found but is incorrect type");
			return null;
		}
	}
	public void CancelAbility(Abilities ability)
	{
		OnPlayerCancelAbility?.Invoke();
		queuedAbility = null;
		abilityCastingTimer = 0;
	}

	//casting timer
	private void AbilityCastingTimer()
	{
		if (abilityBeingCasted != null)
		{
			abilityCastingTimer -= Time.deltaTime;

			if (abilityCastingTimer <= 0)
				CastAbility(abilityBeingCasted);
		}
	}
	private void CastAbility(Abilities ability)
	{
		EntityStats enemyTarget = selectedEnemyTarget != null ? selectedEnemyTarget : TryGrabNewEntityOnQueuedAbilityClick(false);

		if (ability.abilityBaseRef.isProjectile)
			CastDirectionalAbility(ability);
		else if (ability.abilityBaseRef.isAOE)
			CastAoeAbility(ability);
		else if (ability.abilityBaseRef.requiresTarget && ability.abilityBaseRef.isOffensiveAbility)
			CastEffect(ability, enemyTarget);
		else if (ability.abilityBaseRef.requiresTarget && !ability.abilityBaseRef.isOffensiveAbility)   //for MP add support for friendlies
			CastEffect(ability, playerStats);
		else
		{
			CancelAbility(queuedAbility);
			Debug.LogError("failed to find ability type and cast, shouldnt happen");
			return;
		}
	}

	//types of casting
	private void CastDirectionalAbility(Abilities ability)
	{
		Projectiles projectile = DungeonHandler.GetProjectile();
		if (projectile == null)
		{
			GameObject go = Instantiate(projectilePrefab, transform, true);
			projectile = go.GetComponent<Projectiles>();
		}

		projectile.transform.SetParent(null);
		if (PlayerSettingsManager.Instance.autoCastDirectionalAbilitiesAtTarget && selectedEnemyTarget != null)
			projectile.SetPositionAndAttackDirection(transform.position, selectedEnemyTarget.transform.position);
		else
			projectile.SetPositionAndAttackDirection(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
		projectile.Initilize(playerStats, ability.abilityBaseRef);

		OnSuccessfulCast(ability);
	}
	private void CastAoeAbility(Abilities ability)
	{
		AbilityAOE abilityAOE = DungeonHandler.GetAoeAbility();
		if (abilityAOE == null)
		{
			GameObject go = Instantiate(AbilityAoePrefab, transform, true);
			abilityAOE = go.GetComponent<AbilityAOE>();
		}

		//will need additional code here to handle supportive and offensive aoe abilities

		Vector2 movePosition;
		if (PlayerSettingsManager.Instance.autoCastAoeAbilitiesOnTarget && selectedEnemyTarget != null)
			movePosition = selectedEnemyTarget.transform.position;
		else
			movePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		abilityAOE.transform.SetParent(null);
		abilityAOE.Initilize(playerStats, ability.abilityBaseRef, movePosition);

		OnSuccessfulCast(ability);
	}
	private void CastEffect(Abilities ability, EntityStats enemyTarget)
	{
		if (ability.abilityBaseRef.damageType == IDamagable.DamageType.isHealing) //healing
		{
			if (playerStats.currentHealth < playerStats.maxHealth.finalValue) //cancel heal if player at full health in SP
			{
				playerStats.OnHeal(
					ability.abilityBaseRef.damageValuePercentage, true, playerStats.healingPercentageModifier.finalPercentageValue);
			}
			else
			{
				CancelAbility(queuedAbility);     //add support/option to heal other players for MP
				return;
			}
		}

		if (ability.abilityBaseRef.damageValue != 0)    //apply damage for insta damage abilities
		{
			DamageSourceInfo damageSourceInfo = new(playerStats, IDamagable.HitBye.player, ability.abilityBaseRef.damageValue * 
				playerStats.levelModifier, (IDamagable.DamageType)ability.abilityBaseRef.damageType, false);

			damageSourceInfo.SetDeathMessage(ability.abilityBaseRef);
			enemyTarget.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);
		}

		if (ability.abilityBaseRef.hasStatusEffects)    //apply effects (if has any) based on what type it is.
		{
			if (ability.abilityBaseRef.canOnlyTargetSelf)
				playerStats.ApplyNewStatusEffects(ability.abilityBaseRef.statusEffects, playerStats);
			else if (ability.abilityBaseRef.isOffensiveAbility && enemyTarget != null)
				enemyTarget.ApplyNewStatusEffects(ability.abilityBaseRef.statusEffects, playerStats);
			else if (!ability.abilityBaseRef.isOffensiveAbility)         //add support/option to buff other players for MP
				playerStats.ApplyNewStatusEffects(ability.abilityBaseRef.statusEffects, playerStats);
			else
			{
				Debug.LogError("failed to cast status effect");
				CancelAbility(queuedAbility);
				return;
			}
		}

		OnSuccessfulCast(ability);
	}
	private void OnSuccessfulCast(Abilities ability)
	{
		if (ability.abilityBaseRef.isSpell)
		{
			int totalManaCost = (int)(ability.abilityBaseRef.manaCost * playerStats.levelModifier);
			playerStats.DecreaseMana(totalManaCost, false);
		}
		ability.isOnCooldown = true;
		queuedAbility = null;
		abilityBeingCasted = null;
	}

	//PLAYER MARKING FOR BOSS ABILITIES
	public void MarkPlayer()
	{
		PlayerBossMarker.SetActive(true);
	}
	public void UnMarkPlayer()
	{
		PlayerBossMarker.SetActive(false);
	}

	//bool checks
	private bool IsPlayerInteracting()
	{
		if (isInteractingWithInteractable)
			return true;
		else return false;
	}
	
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<BossRoomHandler>() != null|| other.GetComponent<PortalHandler>() != null || 
			other.GetComponent<NpcHandler>() != null || other.GetComponent<ChestHandler>() != null || 
			other.GetComponent<EnchantmentHandler>() != null || other.GetComponent<TrapHandler>() != null)
		{
			currentInteractedObject = other.GetComponent<Interactables>();

			if (other.GetComponent<TrapHandler>() != null)
			{
				TrapHandler trapHandler = other.GetComponent<TrapHandler>();
				currentInteractedObject = other.GetComponent<Interactables>();

				if (!trapHandler.trapDetected) return;
				PlayerEventManager.DetectNewInteractedObject(other.gameObject, true);
			}
			if (other.GetComponent<ChestHandler>() != null)
			{
				if (other.GetComponent<ChestHandler>().chestStateOpened)
					PlayerEventManager.DetectNewInteractedObject(other.gameObject, false);
				else
					PlayerEventManager.DetectNewInteractedObject(other.gameObject, true);
			}
			else
				PlayerEventManager.DetectNewInteractedObject(other.gameObject, true);
		}
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.GetComponent<BossRoomHandler>() != null || other.GetComponent<PortalHandler>() != null ||
			other.GetComponent<NpcHandler>() != null || other.GetComponent<ChestHandler>() != null ||
			other.GetComponent<EnchantmentHandler>() != null || other.GetComponent<TrapHandler>() != null)
		{
			PlayerEventManager.DetectNewInteractedObject(other.gameObject, false);
			currentInteractedObject = null;
			isInteractingWithInteractable = false;
		}
	}

	/// <summary>
	/// Below are all player actions
	/// </summary>

	//in game actions
	private void OnMainAttack()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		if (queuedAbility == null)
		{
			if (PlayerSettingsManager.Instance.mainAttackIsAutomatic) return;
			if (playerEquipmentHandler.equippedWeapon == null || PlayerInventoryUi.Instance.PlayerInfoAndInventoryPanelUi.activeSelf)
				return;
			Weapons weapon = playerEquipmentHandler.equippedWeapon;

			if (weapon.weaponBaseRef.isRangedWeapon)
			{
				if (!debugUseSelectedTargetForAttackDirection)
					weapon.RangedAttack(Camera.main.ScreenToWorldPoint(Input.mousePosition), projectilePrefab);
				else
					weapon.RangedAttack(selectedEnemyTarget.transform.position, projectilePrefab);
			}
			else
			{
				if (!debugUseSelectedTargetForAttackDirection)
					weapon.MeleeAttack(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				else
					weapon.MeleeAttack(selectedEnemyTarget.transform.position);
			}
		}
		else
			CastAbility();
	}
	private void OnRightClick()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		if (queuedAbility != null)
			CancelAbility(queuedAbility);

		CheckForSelectableTarget();
	}
	private void OnCameraZoom()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		//limit min and max zoom size to x, stop camera from zooming in/out based on value grabbed from scroll wheel input
		float value = playerInputs.CameraZoomInput;
		if (playerCamera.orthographicSize > 3 && value == 120 || playerCamera.orthographicSize < 12 && value == -120)
			playerCamera.orthographicSize -= value / 480;
	}
	private void OnInteract()
	{
		if (playerStats.IsEntityDead() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		if (currentInteractedObject == null) return;
		currentInteractedObject.Interact(this);
	}
	private void OnTabTargetingForwards()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		if (selectedEnemyTarget == null || selectedEnemyTargetIndex == EnemyTargetList.Count - 1) //start at begining of list
			CycleTargetsForwards(0);
		else
			CycleTargetsForwards(selectedEnemyTargetIndex + 1);
	}
	private void OnTabTargetingBackwards()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		if (selectedEnemyTarget == null || selectedEnemyTargetIndex == 0) //start at end of list
			CycleTargetsBackwards(EnemyTargetList.Count - 1);
		else
			CycleTargetsBackwards(selectedEnemyTargetIndex - 1);
	}

	//hotbar actions
	private void OnConsumablesOne()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		if (PlayerHotbarUi.Instance.equippedConsumableOne == null) return;
		PlayerHotbarUi.Instance.equippedConsumableOne.ConsumeItem(playerStats);
	}
	private void OnConsumablesTwo()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		if (PlayerHotbarUi.Instance.equippedConsumableTwo == null) return;
		PlayerHotbarUi.Instance.equippedConsumableTwo.ConsumeItem(playerStats);
	}
	private void OnAbilityOne()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		if (queuedAbility != null || PlayerHotbarUi.Instance.equippedAbilityOne == null) return;

		Abilities newQueuedAbility = PlayerHotbarUi.Instance.equippedAbilityOne;
		if (!newQueuedAbility.CanUseAbility(playerStats)) return;

		TryReacquireNewTarget();
		UseAbility(newQueuedAbility);

		if (newQueuedAbility.CanInstantCastAbility())
			CastAbility();
	}
	private void OnAbilityTwo()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		if (queuedAbility != null || PlayerHotbarUi.Instance.equippedAbilityTwo == null) return;

		Abilities newQueuedAbility = PlayerHotbarUi.Instance.equippedAbilityTwo;
		if (!newQueuedAbility.CanUseAbility(playerStats)) return;

		TryReacquireNewTarget();
		UseAbility(newQueuedAbility);

		if (newQueuedAbility.CanInstantCastAbility())
			CastAbility();
	}
	private void OnAbilityThree()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		if (queuedAbility != null || PlayerHotbarUi.Instance.equippedAbilityThree == null) return;

		Abilities newQueuedAbility = PlayerHotbarUi.Instance.equippedAbilityThree;
		if (!newQueuedAbility.CanUseAbility(playerStats)) return;

		TryReacquireNewTarget();
		UseAbility(newQueuedAbility);

		if (newQueuedAbility.CanInstantCastAbility())
			CastAbility();
	}
	private void OnAbilityFour()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		if (queuedAbility != null || PlayerHotbarUi.Instance.equippedAbilityFour == null) return;

		Abilities newQueuedAbility = PlayerHotbarUi.Instance.equippedAbilityFour;
		if (!newQueuedAbility.CanUseAbility(playerStats)) return;

		TryReacquireNewTarget();
		UseAbility(newQueuedAbility);

		if (newQueuedAbility.CanInstantCastAbility())
			CastAbility();
	}
	private void OnAbilityFive()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		if (queuedAbility != null || PlayerHotbarUi.Instance.equippedAbilityFive == null) return;

		Abilities newQueuedAbility = PlayerHotbarUi.Instance.equippedAbilityFive;
		if (!newQueuedAbility.CanUseAbility(playerStats)) return;

		TryReacquireNewTarget();
		UseAbility(newQueuedAbility);

		if (newQueuedAbility.CanInstantCastAbility())
			CastAbility();
	}
	private void TryReacquireNewTarget()
	{
		if (PlayerSettingsManager.Instance.autoSelectNewTarget)
			CycleTargetsForwards(0);
		else return;
	}

	//ui actions
	private void OnMainMenu()
	{
		if (MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		MainMenuManager.Instance.ShowHideMainMenuKeybind();
	}
	private void OnInventory()
	{
		if (MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		PlayerEventManager.ShowPlayerInventory();
	}
	private void OnJournal()
	{
		if (MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		PlayerEventManager.ShowPlayerJournal();
	}
	private void OnClassSelection()
	{
		if (MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		PlayerEventManager.ShowPlayerClassSelection();
	}
	private void OnClassSkillTree()
	{
		if (MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		PlayerEventManager.ShowPlayerSkillTree();
	}
	private void OnLearntAbilities()
	{
		if (MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		PlayerEventManager.ShowPlayerLearntAbilities();
	}

	[System.Serializable]
	public class EnemyDistance
	{
		public string entityClass;
		public string entityName;
		public EntityStats entity;
		public float distance;

		public EnemyDistance(string name, string className, EntityStats entity, float distance)
		{
			entityClass = className;
			entityName = name;
			this.entity = entity;
			this.distance = distance;
		}
	}
}
