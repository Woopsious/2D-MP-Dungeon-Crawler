using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour
{
	public bool debugSetStartingItems;
	public bool debugUseSelectedTargetForAttackDirection;
	public bool debugSetPlayerLevelOnStart;
	public int debugPlayerLevel;
	public Camera playerCamera;
	public LayerMask includeMe;
	[HideInInspector] public EntityStats playerStats;
	[HideInInspector] public EntityClassHandler playerClassHandler;
	[HideInInspector] public PlayerEquipmentHandler playerEquipmentHandler;
	[HideInInspector] public PlayerExperienceHandler playerExperienceHandler;
	private PlayerInputHandler playerInputs;
	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;
	private Animator animator;

	private float speed = 12;

	//target selection
	public event Action<EntityStats> OnNewTargetSelected;
	public EntityStats selectedTarget;

	//abilities
	public event Action<Abilities, PlayerController> OnUseQueuedAbilities;
	public event Action<Abilities> OnCancelQueuedAbilities;
	public GameObject AbilityAoePrefab;
	public GameObject projectilePrefab;

	public Abilities queuedAbility;

	//interactions
	[HideInInspector] public bool isInteractingWithPortal;
	private PortalHandler currentInteractedPortal;
	[HideInInspector] public bool isInteractingWithNpc;
	private NpcHandler currentInteractedNpc;
	[HideInInspector] public bool isInteractingWithChest;
	private ChestHandler currentInteractedChest;

	private void Awake()
	{
		playerStats = GetComponent<EntityStats>();
		playerClassHandler = GetComponent<EntityClassHandler>();
		playerEquipmentHandler = GetComponent<PlayerEquipmentHandler>();
		playerExperienceHandler = GetComponent<PlayerExperienceHandler>();
		playerEquipmentHandler.player = this;
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		animator = GetComponent<Animator>();
	}
	private void Start()
	{
		Initilize();

		OnNewTargetSelected += PlayerHotbarUi.Instance.OnNewTargetSelected;

		PlayerHotbarUi.OnNewQueuedAbilities += OnNewQueuedAbility;
		OnUseQueuedAbilities += PlayerHotbarUi.Instance.OnUseQueuedAbility;
		OnUseQueuedAbilities += OnUseQueuedAbility;
		OnCancelQueuedAbilities += PlayerHotbarUi.Instance.OnCancelQueuedAbility;
		OnCancelQueuedAbilities += OnCancelQueuedAbility;
	}

	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadPlayerInfo;
		DungeonHandler.OnEntityDeathEvent += OnSelectedTargetDeath;
	}
	private void OnDisable()
	{
		OnNewTargetSelected -= PlayerHotbarUi.Instance.OnNewTargetSelected;

		PlayerHotbarUi.OnNewQueuedAbilities -= OnNewQueuedAbility;
		OnUseQueuedAbilities -= PlayerHotbarUi.Instance.OnUseQueuedAbility;
		OnUseQueuedAbilities -= OnUseQueuedAbility;
		OnCancelQueuedAbilities -= PlayerHotbarUi.Instance.OnCancelQueuedAbility;
		OnCancelQueuedAbilities -= OnCancelQueuedAbility;

		SaveManager.RestoreData -= ReloadPlayerInfo;
		DungeonHandler.OnEntityDeathEvent -= OnSelectedTargetDeath;
	}

	private void Update()
	{
		playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 10);
	}
	private void FixedUpdate()
	{
		if (IsPlayerInteracting()) return;
		PlayerMovement();
	}

	public void Initilize()
	{
		if (playerInputs == null)
			playerInputs = PlayerInputHandler.Instance;

		playerCamera.transform.parent = null;

		if (debugSetPlayerLevelOnStart)
			playerStats.entityLevel = debugPlayerLevel;
		else
			playerStats.entityLevel = 1;
		PlayerEventManager.PlayerLevelUp(playerStats);
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

	private void PlayerMovement()
	{
		rb.velocity = new Vector2(playerInputs.MovementInput.x * speed, playerInputs.MovementInput.y * speed);

		UpdateSpriteDirection();
		UpdateAnimationState();
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

	//player select targeting
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
			OnNewTargetSelected?.Invoke(entityStats);
			selectedTarget = entityStats;
		}
	}
	private void OnSelectedTargetDeath(GameObject obj)
	{
		if (selectedTarget == null) return;
		if (selectedTarget.gameObject != obj) return;
		selectedTarget = null;
	}

	//player ability casting
	//events
	private void OnNewQueuedAbility(Abilities ability, EntityStats playerStats)
	{
		queuedAbility = ability;
	}
	public void CastQueuedAbility(Abilities ability)
	{
		OnUseQueuedAbilities?.Invoke(ability, this);
	}
	private void OnUseQueuedAbility(Abilities ability, PlayerController player)
	{
		if (ability.abilityBaseRef.isAOE)
			CastAoeAbility(ability);
		else if (ability.abilityBaseRef.isProjectile)
			CastDirectionalAbility(ability);
		else if (ability.abilityBaseRef.requiresTarget && ability.abilityBaseRef.isOffensiveAbility)
		{
			EntityStats newEnemyEntity;
			if (selectedTarget == null)
			{
				newEnemyEntity = TryGrabNewEntityOnQueuedAbilityClick(false);
				if (newEnemyEntity == null)
				{
					CancelQueuedAbility(queuedAbility);
					return;
				}
				else
					CastEffect(ability, newEnemyEntity);
			}
			else
				CastEffect(ability, selectedTarget);
		}
		else if (ability.abilityBaseRef.requiresTarget && !ability.abilityBaseRef.isOffensiveAbility)	//for MP add support for friendlies
			CastEffect(ability, playerStats);
		else
		{
			CancelQueuedAbility(queuedAbility);
			Debug.LogError("failed to find ability type and cast, shouldnt happen");
			return;
		}
	}
	private EntityStats TryGrabNewEntityOnQueuedAbilityClick(bool lookingForFriendly)	//add support/option to handle friendly targets
	{
		EntityStats newEntity;
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 50, includeMe);
		//Debug.Log("hit position: " + hit.point);

		if (hit.transform == null)
		{
			//Debug.Log("no obj found at location");
			return null;
		}
		if (hit.transform.gameObject.GetComponent<EntityStats>() == null)
		{
			//Debug.Log("no entity found");
			return null;
		}

		newEntity = hit.transform.gameObject.GetComponent<EntityStats>();

		if (newEntity.IsPlayerEntity() && lookingForFriendly)
		{
			//Debug.Log("entity found is player");
			return newEntity;
		}
		else if (!newEntity.IsPlayerEntity() && !lookingForFriendly)
		{
			//Debug.Log("entity found is enemy");
			return newEntity;
		}
		else
		{
			//Debug.Log("entity found is incorrect type");
			return null;
		}
	}
	public void CancelQueuedAbility(Abilities ability)
	{
		OnCancelQueuedAbilities?.Invoke(ability);
	}
	private void OnCancelQueuedAbility(Abilities ability)
	{
		queuedAbility = null;
	}

	//casting
	private void CastEffect(Abilities ability, EntityStats enemyTarget)
	{
		if (ability.abilityBaseRef.damageType == SOClassAbilities.DamageType.isHealing)	//healing
		{
			if (playerStats.currentHealth < playerStats.maxHealth.finalValue) //cancel heal if player at full health in SP
				playerStats.OnHeal(ability.abilityBaseRef.damageValuePercentage, true, playerStats.healingPercentageModifier.finalPercentageValue);
			else
			{
				CancelQueuedAbility(queuedAbility);      //add support/option to heal other players for MP
				return;
			}
		}
		else if (ability.abilityBaseRef.statusEffectType == SOClassAbilities.StatusEffectType.noEffect)	//insta damage abilities
		{
			if (ability.abilityBaseRef.isOffensiveAbility)
				enemyTarget.GetComponent<Damageable>().OnHitFromDamageSource(this, GetComponent<Collider2D>(), ability.abilityBaseRef.damageValue
					* playerStats.levelModifier, (IDamagable.DamageType)ability.abilityBaseRef.damageType, 0, false, true);
		}

		else if (ability.abilityBaseRef.statusEffectType != SOClassAbilities.StatusEffectType.noEffect)	//buffing/debuffing status effects
		{
			if (ability.abilityBaseRef.canOnlyTargetSelf)
				playerStats.ApplyStatusEffect(ability.abilityBaseRef);
			else if (ability.abilityBaseRef.isOffensiveAbility && enemyTarget != null)
				enemyTarget.ApplyStatusEffect(ability.abilityBaseRef);
			else if (!ability.abilityBaseRef.isOffensiveAbility)		 //add support/option to buff other players for MP
				playerStats.ApplyStatusEffect(ability.abilityBaseRef);
			else
			{
				Debug.LogError("failed to cast status effect");
				CancelQueuedAbility(queuedAbility);
				return;
			}
		}
		else
		{
			Debug.LogError("failed to cast ability effect");
			CancelQueuedAbility(queuedAbility);
			return;
		}

		OnSuccessfulCast(ability);
	}
	private void CastDirectionalAbility(Abilities ability)
	{
		Projectiles projectile = DungeonHandler.GetProjectile();
		if (projectile == null)
		{
			GameObject go = Instantiate(projectilePrefab, transform, true);
			projectile = go.GetComponent<Projectiles>();
		}
		projectile.transform.SetParent(null);
		projectile.transform.position = (Vector2)transform.position;
		projectile.Initilize(this, ability.abilityBaseRef, playerStats);

		if (!debugUseSelectedTargetForAttackDirection)
			SetProjectileDirection(projectile, GetAttackRotation(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
		else
			SetProjectileDirection(projectile, GetAttackRotation(selectedTarget.transform.position));

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

		Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		abilityAOE.transform.SetParent(null);
		abilityAOE.transform.position = (Vector2)mousePosition;
		abilityAOE.Initilize(ability.abilityBaseRef, playerStats);
		abilityAOE.AddPlayerRef(this);

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
	}

	//directional ability attacks
	private float GetAttackRotation(Vector3 positionOfThingToAttack)
	{
		Vector3 rotation = positionOfThingToAttack - transform.position;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		return rotz;
	}
	private void SetProjectileDirection(Projectiles projectile, float rotz)
	{
		projectile.transform.rotation = Quaternion.Euler(0, 0, rotz - 90);
	}

	//bool checks
	private bool IsPlayerInteracting()
	{
		if (isInteractingWithNpc || isInteractingWithPortal || isInteractingWithChest)
			return true;
		else return false;
	}

	/// <summary>
	/// Below are functions to link to player so they can interact with them
	/// </summary>
	
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<PortalHandler>() != null)
		{
			PlayerEventManager.DetectNewInteractedObject(other.gameObject, true);
			currentInteractedPortal = other.GetComponent<PortalHandler>();
		}
		else if (other.GetComponent<NpcHandler>() != null)
		{
			PlayerEventManager.DetectNewInteractedObject(other.gameObject, true);
			currentInteractedNpc = other.GetComponent<NpcHandler>();
		}
		else if (other.GetComponent<ChestHandler>() != null)
		{
			PlayerEventManager.DetectNewInteractedObject(other.gameObject, true);
			currentInteractedChest = other.GetComponent<ChestHandler>();
			if (currentInteractedChest.chestStateOpened)
				PlayerEventManager.DetectNewInteractedObject(other.gameObject, false);
			else
				PlayerEventManager.DetectNewInteractedObject(other.gameObject, true);
		}
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.GetComponent<PortalHandler>() != null)
		{
			PlayerEventManager.DetectNewInteractedObject(other.gameObject, false);
			currentInteractedPortal = null;
		}
		else if (other.GetComponent<NpcHandler>() != null)
		{
			PlayerEventManager.DetectNewInteractedObject(other.gameObject, false);
			currentInteractedNpc = null;
		}
		else if (other.GetComponent<ChestHandler>() != null)
		{
			PlayerEventManager.DetectNewInteractedObject(other.gameObject, false);
			currentInteractedChest = null;
		}
	}

	/// <summary>
	/// Below are all player actions
	/// </summary>

	//in game actions
	private void OnMainAttack()
	{
		if (IsPlayerInteracting()) return;

		if (queuedAbility == null)
		{
			if (playerEquipmentHandler.equippedWeapon == null || PlayerInventoryUi.Instance.PlayerInfoAndInventoryPanelUi.activeSelf)
				return;
			Weapons weapon = playerEquipmentHandler.equippedWeapon;

			if (weapon.weaponBaseRef.isRangedWeapon)
			{
				if (!debugUseSelectedTargetForAttackDirection)
					weapon.RangedAttack(Camera.main.ScreenToWorldPoint(Input.mousePosition), projectilePrefab);
				else
					weapon.RangedAttack(selectedTarget.transform.position, projectilePrefab);
			}
			else
			{
				if (!debugUseSelectedTargetForAttackDirection)
					weapon.MeleeAttack(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				else
					weapon.MeleeAttack(selectedTarget.transform.position);
			}

		}
		else
			CastQueuedAbility(queuedAbility);
	}
	private void OnRightClick()
	{
		if (IsPlayerInteracting()) return;

		if (queuedAbility != null)
			CancelQueuedAbility(queuedAbility);

		CheckForSelectableTarget();
	}
	private void OnCameraZoom()
	{
		//limit min and max zoom size to x, stop camera from zooming in/out based on value grabbed from scroll wheel input
		float value = playerInputs.CameraZoomInput;
		if (playerCamera.orthographicSize > 3 && value == 120 || playerCamera.orthographicSize < 12 && value == -120)
			playerCamera.orthographicSize -= value / 480;
	}
	private void OnInteract()
	{
		if (currentInteractedPortal != null)
			currentInteractedPortal.Interact(this);
		else if (currentInteractedChest != null)
		{
			if (PlayerInventoryUi.Instance.interactedInventorySlotsUi.activeInHierarchy)
				currentInteractedChest.UnInteract(this);
			else
				currentInteractedChest.Interact(this);
		}
		else if (currentInteractedNpc != null)
		{
			if (currentInteractedNpc.npc.npcType == SONpcs.NPCType.isQuestNpc)
			{
				if (PlayerJournalUi.Instance.npcJournalPanalUi.activeInHierarchy)
					currentInteractedNpc.UnInteract(this);
				else
					currentInteractedNpc.Interact(this);
			}
			else if (currentInteractedNpc.npc.npcType == SONpcs.NPCType.isShopNpc)
			{
				if (PlayerInventoryUi.Instance.npcShopPanalUi.activeInHierarchy)
					currentInteractedNpc.UnInteract(this);
				else
					currentInteractedNpc.Interact(this);
			}
		}
	}

	//hotbar actions
	private void OnConsumablesOne()
	{
		if (IsPlayerInteracting()) return;
		if (PlayerHotbarUi.Instance.equippedConsumableOne == null) return;
		PlayerHotbarUi.Instance.equippedConsumableOne.ConsumeItem(playerStats);
	}
	private void OnConsumablesTwo()
	{
		if (IsPlayerInteracting()) return;
		if (PlayerHotbarUi.Instance.equippedConsumableTwo == null) return;
		PlayerHotbarUi.Instance.equippedConsumableTwo.ConsumeItem(playerStats);
	}
	private void OnAbilityOne()
	{
		if (IsPlayerInteracting() || queuedAbility != null) return;
		if (PlayerHotbarUi.Instance.equippedAbilityOne == null) return;

		Abilities newQueuedAbility = PlayerHotbarUi.Instance.equippedAbilityOne;
		if (!newQueuedAbility.CanUseAbility(playerStats)) return;
		PlayerHotbarUi.Instance.AddNewQueuedAbility(newQueuedAbility, this);

		if (newQueuedAbility.CanInstantCastAbility(selectedTarget))
			CastQueuedAbility(queuedAbility);
	}
	private void OnAbilityTwo()
	{
		if (IsPlayerInteracting() || queuedAbility != null) return;
		if (PlayerHotbarUi.Instance.equippedAbilityTwo == null) return;

		Abilities newQueuedAbility = PlayerHotbarUi.Instance.equippedAbilityTwo;
		if (!newQueuedAbility.CanUseAbility(playerStats)) return;
		PlayerHotbarUi.Instance.AddNewQueuedAbility(newQueuedAbility, this);

		if (newQueuedAbility.CanInstantCastAbility(selectedTarget))
			CastQueuedAbility(queuedAbility);
	}
	private void OnAbilityThree()
	{
		if (IsPlayerInteracting() || queuedAbility != null) return;
		if (PlayerHotbarUi.Instance.equippedAbilityThree == null) return;

		Abilities newQueuedAbility = PlayerHotbarUi.Instance.equippedAbilityThree;
		if (!newQueuedAbility.CanUseAbility(playerStats)) return;
		PlayerHotbarUi.Instance.AddNewQueuedAbility(newQueuedAbility, this);

		if (newQueuedAbility.CanInstantCastAbility(selectedTarget))
			CastQueuedAbility(queuedAbility);
	}
	private void OnAbilityFour()
	{
		if (IsPlayerInteracting() || queuedAbility != null) return;
		if (PlayerHotbarUi.Instance.equippedAbilityFour == null) return;

		Abilities newQueuedAbility = PlayerHotbarUi.Instance.equippedAbilityFour;
		if (!newQueuedAbility.CanUseAbility(playerStats)) return;
		PlayerHotbarUi.Instance.AddNewQueuedAbility(newQueuedAbility, this);

		if (newQueuedAbility.CanInstantCastAbility(selectedTarget))
			CastQueuedAbility(queuedAbility);
	}
	private void OnAbilityFive()
	{
		if (IsPlayerInteracting() || queuedAbility != null) return;
		if (PlayerHotbarUi.Instance.equippedAbilityFive == null) return;

		Abilities newQueuedAbility = PlayerHotbarUi.Instance.equippedAbilityFive;
		if (!newQueuedAbility.CanUseAbility(playerStats)) return;
		PlayerHotbarUi.Instance.AddNewQueuedAbility(newQueuedAbility, this);

		if (newQueuedAbility.CanInstantCastAbility(selectedTarget))
			CastQueuedAbility(queuedAbility);
	}

	//ui actions
	private void OnMainMenu()
	{
		MainMenuManager.Instance.ShowHideMainMenuKeybind();
	}
	private void OnInventory()
	{
		PlayerEventManager.ShowPlayerInventory();
	}
	private void OnJournal()
	{
		PlayerEventManager.ShowPlayerJournal();
	}
	private void OnClassSelection()
	{
		PlayerEventManager.ShowPlayerClassSelection();
	}
	private void OnSkillTree()
	{
		PlayerEventManager.ShowPlayerSkillTree();
	}
	private void OnLearntAbilities()
	{
		PlayerEventManager.ShowPlayerLearntAbilities();
	}
}
