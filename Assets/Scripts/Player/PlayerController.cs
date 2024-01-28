using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[HideInInspector] public PlayerEquipmentHandler playerEquipmentHandler;
	public Camera playerCamera;
	private EntityStats playerStats;
	private PlayerInputActions playerInputs;
	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;
	private Animator animator;

	private Vector2 moveDirection = Vector2.zero;
	public float speed;

	private void Awake()
	{
		playerInputs = new PlayerInputActions();
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		animator = GetComponent<Animator>();
		playerCamera.transform.parent = null;
	}

	private void OnEnable()
	{
		if (playerInputs == null)
			playerInputs = new PlayerInputActions();
		playerInputs.Enable();
	}

	private void OnDisable()
	{
		playerInputs.Disable();
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
	public void UpdateSpriteDirection()
	{
		if (rb.velocity.x < 0.01 && rb.velocity.x != 0)
			transform.eulerAngles = new Vector3(0, 0, 0);
		else if (rb.velocity.x > -0.01 && rb.velocity.x != 0)
			transform.eulerAngles = new Vector3(0, 180, 0);
	}
	public void UpdateAnimationState()
	{
		if (rb.velocity == new Vector2(0,0))
			animator.SetBool("isIdle", true);
		else
			animator.SetBool("isIdle", false);
	}

	/// <summary>
	/// Below are all player actions
	/// </summary>

	//player actions
	private void OnMainAttack()
	{
		if (playerEquipmentHandler.equippedWeapon == null || InventoryUi.Instance.isActiveAndEnabled) return;
		playerEquipmentHandler.equippedWeapon.Attack(Camera.main.ScreenToWorldPoint(Input.mousePosition));
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
		if (playerEquipmentHandler.equippedConsumableOne == null)
		{
			Debug.Log("consumable slot one empty"); return;
		}
	}
	private void OnConsumablesTwo()
	{
		if (playerEquipmentHandler.equippedConsumableTwo == null)
		{
			Debug.Log("consumable slot two empty"); return;
		}
	}
	private void OnAbilityOne()
	{

	}
	private void OnAbilityTwo()
	{

	}
	private void OnAbilityThree()
	{

	}
	private void OnAbilityFour()
	{

	}
	private void OnAbilityFive()
	{

	}

	//ui actions
	private void OnMainMenu()
	{
		MainMenuManager.Instance.ShowHideMainMenu();
	}
	private void OnInventory()
	{
		InventoryUi.Instance.HideShowInventory();
	}
}
