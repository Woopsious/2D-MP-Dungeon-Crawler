using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
	[Header("Debug settings")]
	public bool debugSetStartingItems;
	public bool debugUseSelectedTargetForAttackDirection;
	public bool debugSetPlayerLevelOnStart;
	public int debugPlayerLevel;

	[Header("Player Info")]
	private Camera playerCamera;
	public LayerMask includeMe;
	[HideInInspector] public EntityStats playerStats;
	[HideInInspector] public PlayerClassHandler playerClassHandler;
	[HideInInspector] public PlayerEquipmentHandler playerEquipmentHandler;
	[HideInInspector] public PlayerExperienceHandler playerExperienceHandler;
	[HideInInspector] public EntityDetection enemyDetection;
	private PlayerInput playerInput;
	private Rigidbody2D rb;
	private Animator animator;

	public float speed = 12;

	//main attack auto attack timer
	private readonly float mainAttackAutoAttackCooldown = 0.25f;
	private float mainAttackAutoAttackTimer;

	[Header("Player Enemy Targeting")] // +info
	public EntityStats selectedEnemyTarget;
	private int selectedEnemyTargetIndex;
	private List<EnemyDistance> EnemyTargetList = new List<EnemyDistance>();

	[Header("Player Friendly Targeting")] // +info
	public EntityStats selectedFriendlyTarget;

	//target selected event
	public static event Action<EntityStats> OnNewTargetSelected;

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
		playerInput = GetComponent<PlayerInput>();
		playerStats = GetComponent<EntityStats>();
		playerClassHandler = GetComponent<PlayerClassHandler>();
		playerEquipmentHandler = GetComponent<PlayerEquipmentHandler>();
		playerExperienceHandler = GetComponent<PlayerExperienceHandler>();
		enemyDetection = GetComponentInChildren<EntityDetection>();
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	}
	private void Start()
	{
		Initilize();
	}

	private void OnEnable()
	{
		SaveManager.ReloadSaveGameData += ReloadPlayerInfo;
		ObjectPoolingManager.OnEntityDeathEvent += OnSelectedTargetDeath;
		ObjectPoolingManager.AddPlayerToList(this);
	}
	private void OnDisable()
	{
		SaveManager.ReloadSaveGameData -= ReloadPlayerInfo;
		ObjectPoolingManager.OnEntityDeathEvent -= OnSelectedTargetDeath;
		ObjectPoolingManager.RemovePlayerFromList(this);
	}

	private void Update()
	{
		if (GameManager.Localplayer == this)
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
		HealPlayerInHubScene();
	}

	//set player data
	private void Initilize()
	{
		if (PlayerIsLocalPlayer())
		{
			UpdateLocalPlayerReferences();

			if (MultiplayerManager.IsMultiplayer())
				RequestPlayerInfoOfOtherClients();
		}

		if (debugSetPlayerLevelOnStart)
			playerStats.entityLevel = debugPlayerLevel;
		else
			playerStats.entityLevel = 1;

		PlayerEventManager.PlayerLevelUp(playerStats);
		playerStats.CalculateBaseStats();
	}
	private void UpdateLocalPlayerReferences()
	{
		GameManager.Instance.UpdateLocalPlayerInstanceAndReloadAllGameData(this);
		playerCamera = GameManager.LocalPlayerCamera;
		playerInput.actions = PlayerInputHandler.Instance.playerControls;
	}
	private void RequestPlayerInfoOfOtherClients()
	{
		foreach (PlayerController player in ObjectPoolingManager.Instance.playersPool)
		{
			if (player != this)
			{
				player.playerClassHandler.SyncInfoToNewlyJoinedClientRpc(ClientManager.Instance.clientNetworkedId);
				player.playerEquipmentHandler.SyncInfoToNewlyJoinedClientRpc(ClientManager.Instance.clientNetworkedId);
			}
		}
	}

	//event up update info
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
		Vector2 moveInput = new (PlayerInputHandler.Instance.MovementInput.x * speed, PlayerInputHandler.Instance.MovementInput.y * speed);

		if (!MultiplayerManager.IsMultiplayer())
		{
			//Debug.LogError("sp | move input: " + moveInput);
			Move(moveInput);
		}
		else if (IsHost && IsLocalPlayer)
		{
			//Debug.LogError("host | move input: " + moveInput);
			MoveServerRPC(moveInput);
		}
		else if (IsClient && IsLocalPlayer)
		{
			//Debug.LogError("client | move input: " + moveInput);
			MoveServerRPC(moveInput);
		}

		UpdateSpriteDirection();
		UpdateAnimationState();
	}
	[ServerRpc]
	private void MoveServerRPC(Vector2 moveInput)
	{
		Move(moveInput);
	}
	private void Move(Vector2 moveInput)
	{
		rb.velocity = moveInput;
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
	public void UpdateMovementSpeed(float speedModifier, bool resetSpeed)
	{
		if (resetSpeed)
			speed = 12;
		else
			speed *= speedModifier;
	}

	private void HealPlayerInHubScene()
	{
		if (GameManager.Instance.currentlyLoadedScene.name != GameManager.Instance.hubScene) return;
		if (playerStats.currentHealth < playerStats.maxHealth.finalValue)
			playerStats.RecieveHealing(1f, true, playerStats.healingPercentageModifier.finalPercentageValue);
	}

	//PLAYER TARGETING OPTIONS
	//mouse select targeting
	private void CheckForSelectableTarget()
	{
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, includeMe);
		if (hit.collider == null)
			return;
		if (hit.collider.GetComponent<EntityStats>() == null)
			return;

		EntityStats entityStats = hit.collider.GetComponent<EntityStats>();
		if (entityStats.IsPlayerEntity())
		{
			SetNewFriendlySelectedTarget(entityStats);
			return;
		}
		else
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
		EntityStats entityStats = obj.GetComponent<EntityStats>();
		if (entityStats.IsPlayerEntity() && entityStats == selectedFriendlyTarget)
			ClearSelectedTarget(true);
		else if (!entityStats.IsPlayerEntity() && entityStats == selectedEnemyTarget)
			ClearSelectedTarget(false);
    }
	public void ClearSelectedTarget(bool targetFriendly)
	{
        if (targetFriendly)
			selectedFriendlyTarget = null;
		else
		{
			selectedEnemyTarget = null;
			selectedEnemyTargetIndex = 0;
		}
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
	private void SetNewSelectedEnemyTarget(int entityIndex)
	{
		OnNewTargetSelected?.Invoke(EnemyTargetList[entityIndex].entity);
		selectedEnemyTarget = EnemyTargetList[entityIndex].entity;
		selectedEnemyTargetIndex = entityIndex;
	}

	//set friendly target
	private void SetNewFriendlySelectedTarget(EntityStats entity)
	{
		OnNewTargetSelected?.Invoke(entity);
		selectedFriendlyTarget = entity;
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

	//PLAYER MAIN WEAPON ATTACKS
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

		if (selectedEnemyTarget != null)

			if (weapon.weaponBaseRef.isRangedWeapon)    //ranged weapon logic
			{
				if (selectedEnemyTarget != null)
				{
					if (MultiplayerManager.IsMultiplayer())
						SyncMainWeaponAttackRpc(selectedEnemyTarget.transform.position);
					else
						MainWeaponAttack(selectedEnemyTarget.transform.position);
				}
				else    //if player selected target null, try find one within min and max attack range
				{
					foreach (EnemyDistance enemy in EnemyTargetList)
					{
						if (enemy.distance > weapon.weaponBaseRef.minAttackRange && enemy.distance < weapon.weaponBaseRef.maxAttackRange)
							entityToAttack = enemy.entity;
					}

					if (GrabDistanceToEntity(entityToAttack) <= weapon.weaponBaseRef.maxAttackRange)
					{
						if (MultiplayerManager.IsMultiplayer())
							SyncMainWeaponAttackRpc(selectedEnemyTarget.transform.position);
						else
							MainWeaponAttack(selectedEnemyTarget.transform.position);
					}
				}
			}
			else    //melee weapon logic
			{
				if (selectedEnemyTarget != null && GrabDistanceToEntity(selectedEnemyTarget) < weapon.weaponBaseRef.maxAttackRange)
				{
					if (MultiplayerManager.IsMultiplayer())
						SyncMainWeaponAttackRpc(selectedEnemyTarget.transform.position);
					else
						MainWeaponAttack(selectedEnemyTarget.transform.position);
				}
				//if player selected target null && out of range, attack closest enemy set at start of func
				else if (GrabDistanceToEntity(entityToAttack) <= weapon.weaponBaseRef.maxAttackRange)
				{
					if (MultiplayerManager.IsMultiplayer())
						SyncMainWeaponAttackRpc(selectedEnemyTarget.transform.position);
					else
						MainWeaponAttack(selectedEnemyTarget.transform.position);
				}
			}
	}

	//initiate player attacks
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SyncMainWeaponAttackRpc(Vector2 attackPos)
	{
		MainWeaponAttack(attackPos);
	}
	private void MainWeaponAttack(Vector2 attackPos)
	{
		Weapons weapon = playerEquipmentHandler.equippedWeapon;
		weapon.Attack(attackPos);
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
	private EntityStats TryGrabNewEntityOnEffectCasting(bool lookingForFriendly)	//add support/option to handle friendly targets
	{
		EntityStats newEntity;
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, includeMe);

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
	private void CancelAbility()
	{
		OnPlayerCancelAbility?.Invoke();
		queuedAbility = null;
		abilityCastingTimer = 0;
	}

	//casting timer + casting of ability
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
		if (ability.abilityBaseRef.isProjectile || ability.abilityBaseRef.isAOE)
		{
			if (MultiplayerManager.IsMultiplayer())
				SyncSetUpAndCastAbilitiesRpc(playerStats.NetworkObjectId, GetAbilityIndex(ability.abilityBaseRef), GetAbilityAttackPos(ability));
			else
				SetUpAndCastAbilities(playerStats, ability.abilityBaseRef, GetAbilityAttackPos(ability));
		}
		else if (ability.abilityBaseRef.requiresTarget)
			CastEffectAbilities(ability);
		else
		{
			CancelAbility();
			Debug.LogError("failed to find ability type and cast, shouldnt happen");
			return;
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

	//set up and cast projectile/aoe ability types
	[Rpc(SendTo.Server, RequireOwnership = false)]
	private void SyncSetUpAndCastAbilitiesRpc(ulong casterId, int abilityIndex, Vector2 attackPos)
	{
		EntityStats casterStats = NetworkManager.SpawnManager.SpawnedObjects[casterId].GetComponent<EntityStats>();
		SOAbilities ability = AssetDatabase.Database.abilities[abilityIndex];
		SetUpAndCastAbilities(casterStats, ability, attackPos);
	}
	private void SetUpAndCastAbilities(EntityStats casterStats, SOAbilities ability, Vector2 attackPos)
	{
		if (ability.isProjectile)
			SetUpAndCastProjectileAbility(casterStats, ability, attackPos);
		else if (ability.isAOE)
			SetUpAndCastAoeAbility(casterStats, ability, attackPos);
	}
	private void SetUpAndCastProjectileAbility(EntityStats casterStats, SOAbilities abilityRef, Vector2 attackPos)
	{
		Projectiles projectile = ObjectPoolingManager.GetInActiveProjectile();
		if (projectile == null)
		{
			GameObject go = Instantiate(projectilePrefab, transform, true);
			projectile = go.GetComponent<Projectiles>();
			ObjectPoolingManager.AddProjectileToObjectPooling(projectile);

			if (MultiplayerManager.IsMultiplayer())
				projectile.GetComponent<NetworkObject>().Spawn();
		}

		projectile.Initilize(casterStats, abilityRef,attackPos);
	}
	private void SetUpAndCastAoeAbility(EntityStats casterStats, SOAbilities abilityRef, Vector2 attackPos)
	{
		AbilityAOE abilityAOE = ObjectPoolingManager.GetInActiveAoeAbility();
		if (abilityAOE == null)
		{
			GameObject go = Instantiate(AbilityAoePrefab, transform, true);
			abilityAOE = go.GetComponent<AbilityAOE>();
			ObjectPoolingManager.AddAoeAbilityToObjectPooling(abilityAOE);

			if (MultiplayerManager.IsMultiplayer())
				abilityAOE.GetComponent<NetworkObject>().Spawn();
		}

		//will need additional code here to handle supportive and offensive aoe abilities
		abilityAOE.Initilize(casterStats, abilityRef, attackPos);
	}

	//set up and cast effect types
	private void CastEffectAbilities(Abilities ability)
	{
		EntityStats target;

		if (ability.abilityBaseRef.isOffensiveAbility)
			target = selectedEnemyTarget != null ? selectedEnemyTarget : TryGrabNewEntityOnEffectCasting(false);
		else
			target = playerStats; //update to include support for friendlies

		if (ability.abilityBaseRef.damageType == IDamagable.DamageType.isHealing)
			CastHealingEffect(ability, target);
		else if (ability.abilityBaseRef.damageValue != 0)
			CastDamageEffect(ability, target);

		if (ability.abilityBaseRef.hasStatusEffects)    //apply effects if any
			target.ApplyNewStatusEffects(ability.abilityBaseRef.statusEffects, playerStats);
	}
	private void CastHealingEffect(Abilities ability, EntityStats target)
	{
		if (target.currentHealth < target.maxHealth.finalValue) //cancel heal if player at full health
		{
			target.RecieveHealing(
				ability.abilityBaseRef.damageValuePercentage, true, target.healingPercentageModifier.finalPercentageValue);
		}
		else
		{
			CancelAbility();     //add support/option to heal other players for MP
			return;
		}
	}
	private void CastDamageEffect(Abilities ability, EntityStats target)
	{
		DamageSourceInfo damageSourceInfo = new(playerStats, IDamagable.HitBye.player, ability.abilityBaseRef.damageValue *
			playerStats.levelModifier, ability.abilityBaseRef.damageType, false);

		damageSourceInfo.SetDeathMessage(ability.abilityBaseRef);
		target.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);
	}

	//casting helper funcs
	private Vector2 GetAbilityAttackPos(Abilities ability)
	{
		if (ability.abilityBaseRef.isProjectile)
		{
			if (PlayerSettingsManager.Instance.autoCastDirectionalAbilitiesAtTarget && selectedEnemyTarget != null)
				return selectedEnemyTarget.transform.position;
			else
				return Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}
		else if (ability.abilityBaseRef.isAOE)
		{
			if (PlayerSettingsManager.Instance.autoCastAoeAbilitiesOnTarget && selectedEnemyTarget != null)
				return selectedEnemyTarget.transform.position;
			else
				return Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}
		else return new Vector2(0, 0);
	}
	private int GetAbilityIndex(SOAbilities ability)
	{
		for (int i = 0; i < AssetDatabase.Database.abilities.Count; i++)
		{
			if (ability == AssetDatabase.Database.abilities[i])
				return i;
		}

		Debug.LogError("failed to get class index");
		return 0;
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
	public bool PlayerIsLocalPlayer()
	{
		if (!MultiplayerManager.IsMultiplayer())
			return true;
		else if (IsLocalPlayer)
			return true;
		else return false;
	}
	private bool IsPlayerInteracting()
	{
		if (isInteractingWithInteractable)
			return true;
		else return false;
	}
	private bool IsCollidedObjectInteractable(Collider2D other)
	{
		if (GameManager.Localplayer != this) return false; //ignore if not local player

		if (other.GetComponent<BossRoomHandler>() != null || other.GetComponent<PortalHandler>() != null ||
			other.GetComponent<NpcHandler>() != null || other.GetComponent<ChestHandler>() != null ||
			other.GetComponent<EnchantmentHandler>() != null || other.GetComponent<TrapHandler>() != null)
		{
			return true;
		}
		else return false;
	}

	//INTERACTABLES COLLISSION TRIGGER EVENTS
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (IsCollidedObjectInteractable(other))
			HandleInteractWithCollidables(other);
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (IsCollidedObjectInteractable(other))
			HandleUnInteractWithCollidables(other);
	}
	private void HandleInteractWithCollidables(Collider2D other)
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
			if (other.GetComponent<ChestHandler>().GetChestState() == ChestHandler.ChestState.opened)
				PlayerEventManager.DetectNewInteractedObject(other.gameObject, false);
			else
				PlayerEventManager.DetectNewInteractedObject(other.gameObject, true);
		}
		else
			PlayerEventManager.DetectNewInteractedObject(other.gameObject, true);
	}
	private void HandleUnInteractWithCollidables(Collider2D other)
	{
		PlayerEventManager.DetectNewInteractedObject(other.gameObject, false);
		currentInteractedObject = null;
		isInteractingWithInteractable = false;
	}

	/// <summary>
	/// Below are all player actions
	/// </summary>

	//in game actions
	private void OnMainAttack()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		if (queuedAbility != null)
			CastAbility();
		else
		{
			if (playerEquipmentHandler.equippedWeapon == null || PlayerInventoryUi.Instance.PlayerInfoAndInventoryPanelUi.activeSelf 
				|| PlayerSettingsManager.Instance.mainAttackIsAutomatic) return;

			if (MultiplayerManager.IsMultiplayer())
			{
				if (!debugUseSelectedTargetForAttackDirection)
					SyncMainWeaponAttackRpc(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				else
					SyncMainWeaponAttackRpc(selectedEnemyTarget.transform.position);
			}
			else
			{
				if (!debugUseSelectedTargetForAttackDirection)
					MainWeaponAttack(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				else
					MainWeaponAttack(selectedEnemyTarget.transform.position);
			}
		}
	}
	private void OnRightClick()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		if (queuedAbility != null)
			CancelAbility();

		CheckForSelectableTarget();
	}
	private void OnCameraZoom()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		//limit min and max zoom size to x, stop camera from zooming in/out based on value grabbed from scroll wheel input
		float value = PlayerInputHandler.Instance.CameraZoomInput;
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
