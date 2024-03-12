using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
	public Camera playerCamera;
	public LayerMask includeMe;
	private EntityStats playerStats;
	private EntityClassHandler playerClassHandler;
	[HideInInspector] public PlayerEquipmentHandler playerEquipmentHandler;
	private PlayerInputActions playerInputs;
	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;
	private Animator animator;

	private Vector2 moveDirection = Vector2.zero;
	private float speed = 12;

	//target selection
	public event Action<EntityStats> OnNewTargetSelected;
	public EntityStats selectedTarget;

	public event Action<Abilities, PlayerController> OnUseQueuedAbilities;
	public event Action<Abilities> OnCancelQueuedAbilities;
	public bool debugUseMouseDirectionForProjectiles;
	public GameObject AbilityDirectionalPrefab;
	public GameObject AbilityAoePrefab;

	public Abilities queuedAbility;

	private void Awake()
	{
		Initilize();
	}
	public void Initilize()
	{
		playerInputs = new PlayerInputActions();
		playerStats = GetComponent<EntityStats>();
		playerClassHandler = GetComponent<EntityClassHandler>();
		playerEquipmentHandler = GetComponent<PlayerEquipmentHandler>();
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		animator = GetComponent<Animator>();
		playerCamera.transform.parent = null;
	}

	private void Start()
	{
		playerStats.OnHealthChangeEvent += PlayerHotbarUi.Instance.OnHealthChange; //would be in OnEnable but i get null ref
		playerStats.OnManaChangeEvent += PlayerHotbarUi.Instance.OnManaChange;
		OnNewTargetSelected += PlayerHotbarUi.Instance.OnNewTargetSelected;

		PlayerHotbarUi.OnNewQueuedAbilities += OnNewQueuedAbility;
		OnUseQueuedAbilities += PlayerHotbarUi.Instance.OnUseQueuedAbility;
		OnUseQueuedAbilities += OnUseQueuedAbility;
		OnCancelQueuedAbilities += PlayerHotbarUi.Instance.OnCancelQueuedAbility;
		OnCancelQueuedAbilities += OnCancelQueuedAbility;

		playerStats.entityLevel = 20;
		playerStats.CalculateBaseStats();
	}

	private void OnEnable()
	{
		if (playerInputs == null)
			playerInputs = new PlayerInputActions();
		playerInputs.Enable();

		SaveManager.OnGameLoad += ReloadPlayerLevel;
	}

	private void OnDisable()
	{
		playerInputs.Disable();

		playerStats.OnHealthChangeEvent -= PlayerHotbarUi.Instance.OnHealthChange;
		playerStats.OnManaChangeEvent -= PlayerHotbarUi.Instance.OnManaChange;
		OnNewTargetSelected -= PlayerHotbarUi.Instance.OnNewTargetSelected;

		PlayerHotbarUi.OnNewQueuedAbilities -= OnNewQueuedAbility;
		OnUseQueuedAbilities -= PlayerHotbarUi.Instance.OnUseQueuedAbility;
		OnUseQueuedAbilities -= OnUseQueuedAbility;
		OnCancelQueuedAbilities -= PlayerHotbarUi.Instance.OnCancelQueuedAbility;
		OnCancelQueuedAbilities -= OnCancelQueuedAbility;
		SaveManager.OnGameLoad -= ReloadPlayerLevel;
	}

	private void Update()
	{
		playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 10);
	}

	private void FixedUpdate()
	{
		moveDirection = playerInputs.Player.Movement.ReadValue<Vector2>();
		rb.velocity = new Vector2(moveDirection.x * speed, moveDirection.y * speed);

		UpdateSpriteDirection();
		UpdateAnimationState();
	}

	public void ReloadPlayerLevel()
	{
		playerStats.entityLevel = SaveManager.Instance.GameData.playerLevel;
		playerStats.CalculateBaseStats();
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
	public void CheckForSelectableTarget()
	{
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 1000, includeMe);
		if (hit.collider == null)
			return;
		if (hit.collider.GetComponent<EntityStats>() == null)
			return;

		EntityStats entityStats = hit.collider.GetComponent<EntityStats>();
		if (!entityStats.IsPlayerEntity())
		{
			Debug.Log("new target selected");
			OnNewTargetSelected?.Invoke(entityStats);
				
			if (selectedTarget != null)
				selectedTarget.OnDeathEvent -= OnSelectedTargetDeath;

			selectedTarget = entityStats;
			selectedTarget.OnDeathEvent += OnSelectedTargetDeath;
		}
	}
	public void OnSelectedTargetDeath(GameObject obj)
	{
		selectedTarget.OnDeathEvent -= OnSelectedTargetDeath;
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
			playerStats.OnHeal(ability.damageValuePercentage, true);
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
		GameObject go = Instantiate(AbilityDirectionalPrefab, transform, true);
		go.transform.SetParent(null);
		go.transform.position = (Vector2)transform.position;
		go.GetComponent<AbilityDirectional>().Initilize(ability, playerStats);

		Vector3 rotation;
		if (debugUseMouseDirectionForProjectiles)
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
	}

	/// <summary>
	/// Below are all player actions
	/// </summary>

	//player actions
	private void OnMainAttack()
	{
		if (queuedAbility == null)
		{
			if (playerEquipmentHandler.equippedWeapon == null || PlayerInventoryUi.Instance.PlayerInfoAndInventoryPanelUi.activeSelf)
				return;
			playerEquipmentHandler.equippedWeapon.Attack(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		}
		else
			CastQueuedAbility(queuedAbility);
	}
	private void OnRightClick()
	{
		if (queuedAbility != null)
			OnCancelQueuedAbilities?.Invoke(queuedAbility);

		CheckForSelectableTarget();
	}
	private void OnCameraZoom()
	{
		//limit min and max zoom size to x, stop camera from zooming in/out based on value grabbed from scroll wheel input
		float value = playerInputs.Player.CameraZoom.ReadValue<float>();
		if (playerCamera.orthographicSize > 3 && value == 120 || playerCamera.orthographicSize < 8 && value == -120)
			playerCamera.orthographicSize -= value / 480;
	}

	//hotbar actions
	private void OnConsumablesOne()
	{
		if (PlayerHotbarUi.Instance.equippedConsumableOne == null) return;
		PlayerHotbarUi.Instance.equippedConsumableOne.ConsumeItem(playerStats);
	}
	private void OnConsumablesTwo()
	{
		if (PlayerHotbarUi.Instance.equippedConsumableTwo == null) return;
		PlayerHotbarUi.Instance.equippedConsumableTwo.ConsumeItem(playerStats);
	}
	private void OnAbilityOne()
	{
		playerStats.DecreaseMana(0.33f , true);

		if (PlayerHotbarUi.Instance.equippedAbilityOne == null) return;
		PlayerHotbarUi.Instance.equippedAbilityOne.PlayerUseAbility(playerStats);
		queuedAbility = PlayerHotbarUi.Instance.equippedAbilityOne;
	}
	private void OnAbilityTwo()
	{
		if (PlayerHotbarUi.Instance.equippedAbilityTwo == null) return;
		PlayerHotbarUi.Instance.equippedAbilityTwo.PlayerUseAbility(playerStats);
		queuedAbility = PlayerHotbarUi.Instance.equippedAbilityTwo;
	}
	private void OnAbilityThree()
	{
		if (PlayerHotbarUi.Instance.equippedAbilityThree == null) return;
		PlayerHotbarUi.Instance.equippedAbilityThree.PlayerUseAbility(playerStats);
		queuedAbility = PlayerHotbarUi.Instance.equippedAbilityThree;
	}
	private void OnAbilityFour()
	{
		if (PlayerHotbarUi.Instance.equippedAbilityFour == null) return;
		PlayerHotbarUi.Instance.equippedAbilityFour.PlayerUseAbility(playerStats);
		queuedAbility = PlayerHotbarUi.Instance.equippedAbilityFour;
	}
	private void OnAbilityFive()
	{
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
		PlayerInventoryUi.Instance.ShowHideInventoryKeybind();
	}
	private void OnSkillTree()
	{
		ClassesUi.Instance.ShowHideClassSkillTree();
	}
	private void OnLearntAbilities()
	{
		PlayerInventoryUi.Instance.ShowHideLearntAbilitiesKeybind();
	}
}
