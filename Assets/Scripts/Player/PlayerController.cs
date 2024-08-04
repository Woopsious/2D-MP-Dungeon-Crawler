using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

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

	private Vector2 moveDirection = Vector2.zero;
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
		if (rb.velocity.x < 0.01 && rb.velocity.x != 0)
			transform.eulerAngles = new Vector3(0, 0, 0);
		else if (rb.velocity.x > -0.01 && rb.velocity.x != 0)
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
		if (gameObject != obj) return;
		selectedTarget = null;
	}

	//player ability casting
	//events
	private void OnNewQueuedAbility(Abilities ability, EntityStats playerStats)
	{
		queuedAbility = ability;
	}
	private void OnUseQueuedAbility(Abilities ability, PlayerController player)
	{
		Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		if (ability.abilityBaseRef.isAOE)
			CastAoeAbility(ability.abilityBaseRef);

		else if (ability.abilityBaseRef.isProjectile)
			CastDirectionalAbility(ability.abilityBaseRef);

		else if (ability.abilityBaseRef.damageType == SOClassAbilities.DamageType.isHealing ||
			ability.abilityBaseRef.damageType == SOClassAbilities.DamageType.isMana ||
			ability.abilityBaseRef.statusEffectType != SOClassAbilities.StatusEffectType.noEffect)
				CastEffect(ability.abilityBaseRef);

		queuedAbility = null;
	}
	private void OnCancelQueuedAbility(Abilities ability)
	{
		queuedAbility = null;
	}
	//casting
	public void CastQueuedAbility(Abilities ability)
	{
		OnUseQueuedAbilities?.Invoke(ability, this);
	}
	private void CastEffect(SOClassAbilities ability)
	{
		if (ability.canOnlyTargetSelf && ability.damageType == SOClassAbilities.DamageType.isHealing)
			playerStats.OnHeal(ability.damageValuePercentage, true, playerStats.HealingPercentageModifier.finalPercentageValue);
		if (ability.canOnlyTargetSelf && ability.damageType == SOClassAbilities.DamageType.isMana)
			playerStats.IncreaseMana(ability.damageValuePercentage, true);
		else
		{
			if (ability.canOnlyTargetSelf)
				playerStats.ApplyStatusEffect(ability);
		}
	}
	private void CastDirectionalAbility(SOClassAbilities ability)
	{
		GameObject go = Instantiate(projectilePrefab, transform, true);
		go.transform.SetParent(null);
		go.transform.position = (Vector2)transform.position;
		go.GetComponent<Projectiles>().Initilize(ability, playerStats);

		Vector3 rotation;
		if (!debugUseSelectedTargetForAttackDirection)
			rotation = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
		else
			rotation = selectedTarget.transform.position - transform.position;

		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		go.transform.rotation = Quaternion.Euler(0, 0, rotz - 90);
	}
	private void CastAoeAbility(SOClassAbilities ability)
	{
		Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		GameObject go = Instantiate(AbilityAoePrefab, transform, true);
		go.transform.SetParent(null);
		go.transform.position = (Vector2)mousePosition;
		go.GetComponent<AbilityAOE>().Initilize(ability, playerStats);
		go.GetComponent<AbilityAOE>().AddPlayerRef(this);
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
			OnCancelQueuedAbilities?.Invoke(queuedAbility);

		CheckForSelectableTarget();
	}
	private void OnCameraZoom()
	{
		//limit min and max zoom size to x, stop camera from zooming in/out based on value grabbed from scroll wheel input
		float value = playerInputs.CameraZoomInput;
		if (playerCamera.orthographicSize > 3 && value == 120 || playerCamera.orthographicSize < 8 && value == -120)
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
		if (IsPlayerInteracting()) return;
		if (PlayerHotbarUi.Instance.equippedAbilityOne == null) return;
		PlayerHotbarUi.Instance.equippedAbilityOne.PlayerUseAbility(playerStats);
		queuedAbility = PlayerHotbarUi.Instance.equippedAbilityOne;
	}
	private void OnAbilityTwo()
	{
		if (IsPlayerInteracting()) return;
		if (PlayerHotbarUi.Instance.equippedAbilityTwo == null) return;
		PlayerHotbarUi.Instance.equippedAbilityTwo.PlayerUseAbility(playerStats);
		queuedAbility = PlayerHotbarUi.Instance.equippedAbilityTwo;
	}
	private void OnAbilityThree()
	{
		if (IsPlayerInteracting()) return;
		if (PlayerHotbarUi.Instance.equippedAbilityThree == null) return;
		PlayerHotbarUi.Instance.equippedAbilityThree.PlayerUseAbility(playerStats);
		queuedAbility = PlayerHotbarUi.Instance.equippedAbilityThree;
	}
	private void OnAbilityFour()
	{
		if (IsPlayerInteracting()) return;
		if (PlayerHotbarUi.Instance.equippedAbilityFour == null) return;
		PlayerHotbarUi.Instance.equippedAbilityFour.PlayerUseAbility(playerStats);
		queuedAbility = PlayerHotbarUi.Instance.equippedAbilityFour;
	}
	private void OnAbilityFive()
	{
		if (IsPlayerInteracting()) return;
		if (PlayerHotbarUi.Instance.equippedAbilityFive == null) return;
		PlayerHotbarUi.Instance.equippedAbilityFive.PlayerUseAbility(playerStats);
		queuedAbility = PlayerHotbarUi.Instance.equippedAbilityFive;
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
