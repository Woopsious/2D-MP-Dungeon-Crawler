using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
	[Header("Debug settings")]
	public bool debugSetStartingItems;
	public bool debugUseSelectedTargetForAttackDirection;
	public bool debugSetPlayerLevelOnStart;
	public int debugPlayerLevel;
	public bool debugNoDeath;

	[Header("Player Info")]
	private Camera playerCamera;
	private GameObject objectCameraTracks;
	public LayerMask includeMe;
	[HideInInspector] public EntityStats playerStats;
	[HideInInspector] public PlayerClassHandler playerClassHandler;
	[HideInInspector] public PlayerEquipmentHandler playerEquipmentHandler;
	[HideInInspector] public PlayerExperienceHandler playerExperienceHandler;
	[HideInInspector] public PlayerInventoryHandler playerInventoryHandler;
	[HideInInspector] public EntityDetection enemyDetection;
	private PlayerInput playerInput;
	private Rigidbody2D rb;
	private Animator animator;

	//movement/velocity
	private float moveSpeed = 12;
	public NetworkVariable<Vector2> playerVelocity = new NetworkVariable<Vector2>(default, 
		NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

	[Header("Prefabs")]
	public GameObject AbilityAoePrefab;
	public GameObject projectilePrefab;

	//main attack auto attack timer
	private readonly float mainAttackAutoAttackCooldown = 0.25f;
	private float mainAttackAutoAttackTimer;

	//player respawn info
	private readonly float respawnTimerCooldown = 20f;
	private float respawnTimer;

	//player revive info
	private PlayerController playerRevivingThis;
	private PlayerController playerBeingRevived;
	private bool beingRevived;
	private readonly float reviveTimerCooldown = 3f;
	private float reviveTimer;

	//targetlist update timer
	private readonly float updateTargetListCooldown = 0.5f;
	private float updateTargetListTimer;

	//ENTITY TARGETING
	public static event Action<EntityStats> OnNewTargetSelected;

	//enemy targeting
	private EntityStats selectedEnemyTarget;
	private int selectedEnemyTargetIndex;
	private List<EnemyDistance> EnemyTargetList = new List<EnemyDistance>();

	//friendly targeting
	private EntityStats selectedFriendlyTarget;

	//current player spectating index
	private int playerSpectatorIndex;

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
	public bool isInteractingWithInteractable;
	public Interactables currentInteractedObject;

	private void Awake()
	{
		playerInput = GetComponent<PlayerInput>();
		playerStats = GetComponent<EntityStats>();
		playerClassHandler = GetComponent<PlayerClassHandler>();
		playerEquipmentHandler = GetComponent<PlayerEquipmentHandler>();
		playerExperienceHandler = GetComponent<PlayerExperienceHandler>();
		playerInventoryHandler = GetComponent<PlayerInventoryHandler>();
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
		PlayerEventManager.OnRespawnAllPlayersEvent += ReviveAllDeadPlayer;
		PlayerEventManager.OnRespawnPlayerEvent += ReviveDeadPlayer;

		SaveManager.ReloadSaveGameData += ReloadPlayerInfo;
		ObjectPoolingManager.OnEntityDeathEvent += OnSelectedTargetDeath;
		ObjectPoolingManager.AddPlayerToList(this);
	}
	private void OnDisable()
	{
		PlayerEventManager.OnRespawnAllPlayersEvent -= ReviveAllDeadPlayer;
		PlayerEventManager.OnRespawnPlayerEvent -= ReviveDeadPlayer;

		SaveManager.ReloadSaveGameData -= ReloadPlayerInfo;
		ObjectPoolingManager.OnEntityDeathEvent -= OnSelectedTargetDeath;
		ObjectPoolingManager.RemovePlayerFromList(this);
	}

	private void Update()
	{
		if (playerStats.IsEntityDead())
		{
			if (PlayerIsLocalPlayer())
				RespawnTimer();
			else
				ReviveTimer();
		}
		else
		{
			if (IsPlayerInteracting()) return;

			UpdateTargetsInList();
			AutoAttackTimer();
			AbilityCastingTimer();
		}
	}
	private void FixedUpdate()
	{
		UpdatePlayerCameraPosition();
		if (playerStats.IsEntityDead() || IsPlayerInteracting()) return;

		PlayerMovementInput();
		UpdateSpriteDirection();
		UpdateAnimationState();

		if (!MultiplayerManager.IsClientHost()) return;

		HealPlayerInHubScene();
	}

	//set player data
	private void Initilize()
	{
		if (PlayerIsLocalPlayer())
		{
			reviveTimer = reviveTimerCooldown;
			respawnTimer = respawnTimerCooldown;
			playerSpectatorIndex = 0;
			UpdateLocalPlayerReferences();

			if (MultiplayerManager.IsMultiplayer())
				RequestPlayerInfoOfOtherClients();
		}

		if (debugSetPlayerLevelOnStart)
			playerStats.entityLevel = debugPlayerLevel;
		else
			playerStats.entityLevel = 1;

		PlayerEventManager.PlayerLevelChange(this);
		playerStats.CalculateBaseStats();
	}
	private void UpdateLocalPlayerReferences()
	{
		GameManager.Instance.UpdateLocalPlayerInstanceAndReloadAllGameData(this);
		playerCamera = GameManager.LocalPlayerCamera;
		objectCameraTracks = gameObject;
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
		PlayerEventManager.PlayerLevelChange(this);
	}

	//movement
	private void PlayerMovementInput()
	{
		if (GameManager.Localplayer != this) return;

		Vector2 moveInput = new (PlayerInputHandler.Instance.MovementInput.x, PlayerInputHandler.Instance.MovementInput.y);
		rb.velocity = moveInput * moveSpeed;

		if (MultiplayerManager.IsMultiplayer() && IsOwner)
			playerVelocity.Value = moveInput;
	}
	public void UpdateMovementSpeed(float speedModifier, bool resetSpeed)
	{
		if (resetSpeed)
			moveSpeed = 12;
		else
			moveSpeed *= speedModifier;
	}
	private void UpdateSpriteDirection()
	{
		if (MultiplayerManager.IsMultiplayer())
		{
			if (Mathf.Approximately(playerVelocity.Value.x, 0)) return;

			if (playerVelocity.Value.x > 0.01)
				transform.eulerAngles = new Vector3(0, 0, 0);
			else if (playerVelocity.Value.x < -0.01)
				transform.eulerAngles = new Vector3(0, 180, 0);
		}
		else
		{
			if (Mathf.Approximately(rb.velocity.x, 0)) return;

			if (rb.velocity.x > 0.01)
				transform.eulerAngles = new Vector3(0, 0, 0);
			else if (rb.velocity.x < -0.01)
				transform.eulerAngles = new Vector3(0, 180, 0);
		}
	}
	private void UpdateAnimationState()
	{
		if (MultiplayerManager.IsMultiplayer())
		{
			if (Mathf.Approximately(playerVelocity.Value.magnitude, 0))
				animator.SetBool("isIdle", true);
			else
				animator.SetBool("isIdle", false);
		}
		else
		{
			if (Mathf.Approximately(rb.velocity.magnitude, 0))
				animator.SetBool("isIdle", true);
			else
				animator.SetBool("isIdle", false);
		}
	}
	private void UpdatePlayerCameraPosition()
	{
		if (GameManager.Localplayer == this)
			playerCamera.transform.position = new Vector3(
				objectCameraTracks.transform.position.x, objectCameraTracks.transform.position.y, playerCamera.transform.position.z);
	}

	//force heal
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
			SetNewSelectedFriendlyTarget(entityStats);
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

	//get selected targets
	public EntityStats GetFriendlySelectedTarget()
	{
		return selectedFriendlyTarget;
	}
	public EntityStats GetEnemySelectedTarget()
	{
		return selectedEnemyTarget;
	}

	//cycle targeting
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

	//setting new target
	private void SetNewSelectedEnemyTarget(int entityIndex)
	{
		OnNewTargetSelected?.Invoke(EnemyTargetList[entityIndex].entity);
		selectedEnemyTarget = EnemyTargetList[entityIndex].entity;
		selectedEnemyTargetIndex = entityIndex;
	}
	private void SetNewSelectedFriendlyTarget(EntityStats entity)
	{
		OnNewTargetSelected?.Invoke(entity);
		selectedFriendlyTarget = entity;
	}

	//enemy target list updates
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

	//PLAYER RESPAWNING
	private void RespawnTimer()
	{
		if (BossRoomHandler.Instance != null)
			if (BossRoomHandler.Instance.GetBossRoomState() == BossRoomHandler.BossRoomState.bossActive) return; //disable respawning

		if (respawnTimer > 0)
		{
			respawnTimer -= Time.deltaTime;

			if (respawnTimer < 0)
			{
				Debug.LogError("respawn complete");
				respawnTimer = respawnTimerCooldown;
			}
		}
	}
	public float GetRespawnTime()
	{
		return respawnTimer;
	}

	//PLAYER REVIVNG
	public void StartReviveTimer(PlayerController playerRevivingThis)
	{
		if (beingRevived) return;

		PlayerEventManager.SyncStartRevivePlayerUiTimerEvent(reviveTimerCooldown);
		ClientRpcManager.instance.SyncStartRevivePlayerTimerUiRpc(reviveTimerCooldown, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
		SyncStartReviveRpc(playerRevivingThis.NetworkObjectId, NetworkObjectId);
	}
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SyncStartReviveRpc(ulong playerRevivingThisId, ulong playerBeingRevivedId)
	{
		reviveTimer = reviveTimerCooldown;
		playerRevivingThis = NetworkManager.SpawnManager.SpawnedObjects[playerRevivingThisId].GetComponent<PlayerController>();
		playerBeingRevived = NetworkManager.SpawnManager.SpawnedObjects[playerBeingRevivedId].GetComponent<PlayerController>();
		beingRevived = true;
	}
	public void CancelReviveTimer(PlayerController playerRevivingThis)
	{
		if (this.playerRevivingThis == null || beingRevived && this.playerRevivingThis != playerRevivingThis) return;

		if (beingRevived && this.playerRevivingThis == playerRevivingThis)
		{
			PlayerEventManager.SyncCancelRevivePlayerUiTimerEvent();
			ClientRpcManager.instance.SyncCancelRevivePlayerTimerUiRpc(RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
			SyncCancelReviveRpc();
		}
	}
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SyncCancelReviveRpc()
	{
		beingRevived = false;
		reviveTimer = reviveTimerCooldown;
		playerRevivingThis = null;
		playerBeingRevived = null;
	}
	private void ReviveTimer()
	{
		if (!beingRevived) return;

		if (reviveTimer > 0)
		{
			reviveTimer -= Time.deltaTime;

			if (reviveTimer < 0)
			{
				ClientRpcManager.instance.RespawnPlayerRpc(playerRevivingThis.NetworkObjectId, playerBeingRevived.NetworkObjectId);
				PlayerEventManager.SyncCancelRevivePlayerUiTimerEvent();
				ClientRpcManager.instance.SyncCancelRevivePlayerTimerUiRpc(RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
				SyncCancelReviveRpc();
			}
		}
	}

	//revive event listners
	private void ReviveAllDeadPlayer()
	{
		reviveTimer = reviveTimerCooldown;
		respawnTimer = respawnTimerCooldown;
		playerStats.ResetEntityStats();
		ResetPlayerSpectateMode();
	}
	private void ReviveDeadPlayer(PlayerController optionalReviverPlayer, PlayerController revivedPlayer)
	{
		if (revivedPlayer != this) return;

		reviveTimer = reviveTimerCooldown;
		respawnTimer = respawnTimerCooldown;
		playerStats.ResetEntityStats();
		ResetPlayerSpectateMode();
	}

	//PLAYER SPECTATING
	//spectate new players
	private void SpectateNextAlivePlayer()
	{
		if (playerSpectatorIndex + 1 <= ObjectPoolingManager.Instance.playersPool.Count - 1)
			UpdateSpectatedPlayer(playerSpectatorIndex + 1);
		else
			UpdateSpectatedPlayer(0);
	}
	private void SpecatePreviousAlivePlayer()
	{
		if (playerSpectatorIndex - 1 >= 0)
			UpdateSpectatedPlayer(playerSpectatorIndex - 1);
		else
			UpdateSpectatedPlayer(ObjectPoolingManager.Instance.playersPool.Count - 1);
	}

	//set new spectated target
	private void UpdateSpectatedPlayer(int playerIndex)
	{
		PlayerController player = ObjectPoolingManager.Instance.playersPool[playerIndex];

		objectCameraTracks = player.gameObject;
		playerSpectatorIndex = playerIndex;
		PlayerDeathUi.Instance.UpdateSpectatingPlayer(player.OwnerClientId);
	}
	private void ResetPlayerSpectateMode()
	{
		if (GameManager.Localplayer != this) return;

		playerSpectatorIndex = 0;
		objectCameraTracks = gameObject;
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
		{
			if (!MultiplayerManager.IsMultiplayer()) //apply to self in sp
				target = playerStats;
			else
				target = selectedFriendlyTarget != null ? selectedFriendlyTarget : TryGrabNewEntityOnEffectCasting(true);

		}

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
		Debug.LogError("player marked");

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
	private bool IsCollidedObjectInteractable(Collider2D other)
	{
		if (GameManager.Localplayer != this) return false; //ignore if not local player

		if (other.GetComponent<Interactables>() != null)
			return true;
		else
			return false;
	}
	private void HandleInteractWithCollidables(Collider2D other)
	{
		currentInteractedObject = other.GetComponent<Interactables>();

		if (currentInteractedObject.GetInteractableType() == Interactables.InteractType.trap)
		{
			TrapHandler trapHandler = other.GetComponent<TrapHandler>();

			if (trapHandler.GetTrapState() != TrapHandler.TrapStates.detected) return;
			PlayerEventManager.DetectNewInteractedObject(currentInteractedObject, true, "Interact");
		}
		else if (currentInteractedObject.GetInteractableType() == Interactables.InteractType.portal)
		{
			if (!MultiplayerManager.IsClientHost())
				PlayerEventManager.DetectNewInteractedObject(currentInteractedObject, true, "Not Host");
			else
				PlayerEventManager.DetectNewInteractedObject(currentInteractedObject, true, "Interact");
		}
		else if (currentInteractedObject.GetInteractableType() == Interactables.InteractType.chest)
		{
			if (other.GetComponent<ChestHandler>().GetChestState() == ChestHandler.ChestState.opened)
				PlayerEventManager.DetectNewInteractedObject(currentInteractedObject, false, "Interact");
			else
				PlayerEventManager.DetectNewInteractedObject(currentInteractedObject, true, "Interact");
		}
		else if (currentInteractedObject.GetInteractableType() == Interactables.InteractType.player)
		{
			PlayerController player = currentInteractedObject.GetPlayer();
			if (!player.playerStats.IsEntityDead()) return; //dont care about alive players

			if (player.beingRevived)
				PlayerEventManager.DetectNewInteractedObject(currentInteractedObject, false, "Being Revived");
			else
				PlayerEventManager.DetectNewInteractedObject(currentInteractedObject, true, "Revive");
		}
		else
			PlayerEventManager.DetectNewInteractedObject(currentInteractedObject, true, "Interact");
	}
	private void HandleUnInteractWithCollidables(Collider2D other)
	{
		if (currentInteractedObject == null) return;

		if (currentInteractedObject.GetInteractableType() == Interactables.InteractType.player)
		{
			PlayerController player = currentInteractedObject.GetPlayer();

			if (beingRevived && playerRevivingThis == player)
			{
				PlayerEventManager.SyncCancelRevivePlayerUiTimerEvent();
				ClientRpcManager.instance.SyncCancelRevivePlayerTimerUiRpc(RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
				SyncCancelReviveRpc();
			}
		}

		PlayerEventManager.DetectNewInteractedObject(currentInteractedObject, false, "Interact");
		currentInteractedObject = null;
		isInteractingWithInteractable = false;
	}

	/// <summary>
	/// Below are all player actions
	/// </summary>

	//player interacts
	public void InteractStarted()
	{
		if (playerStats.IsEntityDead() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		if (currentInteractedObject == null) return;

		currentInteractedObject.Interact(this);
	}
	public void InteractCanceled()
	{
		if (playerStats.IsEntityDead() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		if (currentInteractedObject == null) return;
		currentInteractedObject.CancelInteract(this);
	}

	//in game actions
	public void OnCameraZoom()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		//limit min and max zoom size to x, stop camera from zooming in/out based on value grabbed from scroll wheel input
		float value = PlayerInputHandler.Instance.CameraZoomInput;
		if (playerCamera.orthographicSize > 3 && value == 120 || playerCamera.orthographicSize < 12 && value == -120)
			playerCamera.orthographicSize -= value / 480;
	}
	public void OnSpectateNextPlayer()
	{
		if (!playerStats.IsEntityDead() || IsPlayerInteracting()) return;

		SpectateNextAlivePlayer();
	}
	public void OnSpectatePreviousPlayer()
	{
		if (!playerStats.IsEntityDead() || IsPlayerInteracting()) return;

		SpecatePreviousAlivePlayer();
	}
	public void OnMainAttack()
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
	public void OnRightClick()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		if (queuedAbility != null)
			CancelAbility();

		CheckForSelectableTarget();
	}
	public void OnTabTargetingForwards()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		if (selectedEnemyTarget == null || selectedEnemyTargetIndex == EnemyTargetList.Count - 1) //start at begining of list
			CycleTargetsForwards(0);
		else
			CycleTargetsForwards(selectedEnemyTargetIndex + 1);
	}
	public void OnTabTargetingBackwards()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;

		if (selectedEnemyTarget == null || selectedEnemyTargetIndex == 0) //start at end of list
			CycleTargetsBackwards(EnemyTargetList.Count - 1);
		else
			CycleTargetsBackwards(selectedEnemyTargetIndex - 1);
	}

	//hotbar actions
	public void OnConsumablesOne()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		if (PlayerHotbarUi.Instance.equippedConsumableOne == null) return;

		PlayerHotbarUi.Instance.equippedConsumableOne.ConsumeItem(playerStats);
	}
	public void OnConsumablesTwo()
	{
		if (playerStats.IsEntityDead() || IsPlayerInteracting() || MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		if (PlayerHotbarUi.Instance.equippedConsumableTwo == null) return;

		PlayerHotbarUi.Instance.equippedConsumableTwo.ConsumeItem(playerStats);
	}
	public void OnAbilityOne()
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
	public void OnAbilityTwo()
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
	public void OnAbilityThree()
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
	public void OnAbilityFour()
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
	public void OnAbilityFive()
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
	public void TryReacquireNewTarget()
	{
		if (selectedEnemyTarget == null && PlayerSettingsManager.Instance.autoSelectNewTarget)
			CycleTargetsForwards(0);
		else return;
	}

	//ui actions
	public void OnMainMenu()
	{
		if (MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		MainMenuManager.Instance.ShowHideMainMenuKeybind();
	}
	public void OnInventory()
	{
		if (MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		PlayerEventManager.ShowPlayerInventory();
	}
	public void OnJournal()
	{
		if (MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		PlayerEventManager.ShowPlayerJournal();
	}
	public void OnClassSelection()
	{
		if (MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		PlayerEventManager.ShowPlayerClassSelection();
	}
	public void OnClassSkillTree()
	{
		if (MultiplayerManager.CheckIfMultiplayerMenusOpen()) return;
		PlayerEventManager.ShowPlayerSkillTree();
	}
	public void OnLearntAbilities()
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
