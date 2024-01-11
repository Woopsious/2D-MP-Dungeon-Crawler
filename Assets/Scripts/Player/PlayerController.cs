using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
	public Camera playerCamera;
	private PlayerInputActions playerInputs;
	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;

	private Vector2 moveDirection = Vector2.zero;
	public float speed;

	public GameObject weapon;
	public Collider2D weaponCollider;

	private void Awake()
	{
		playerInputs = new PlayerInputActions();
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
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

		if (rb.velocity.x > 0.01 && rb.velocity.x != 0)
			transform.eulerAngles = new Vector3(0,0,0);
		else if (rb.velocity.x < -0.01 && rb.velocity.x != 0)
			transform.eulerAngles = new Vector3(0, 180, 0);
	}

	/// <summary>
	/// Below are all player actions
	/// </summary>

	//player action
	public bool canAttackAgain;
	private void OnMainAttack()
	{
		if (!canAttackAgain) return;
		Debug.Log("Attacking");

		canAttackAgain = false;
		weaponCollider.enabled = true;
		StartCoroutine(weaponCooldown());
	}
	IEnumerator weaponCooldown()
	{
		yield return new WaitForSeconds(0.1f);
		weaponCollider.enabled = false;
		yield return new WaitForSeconds(weapon.GetComponent<Weapons>().weaponBaseRef.baseAttackSpeed - 0.1f);
		canAttackAgain = true;
	}
	private void OnCameraZoom()
	{
		//limit min and max zoom size to x, stop camera from zooming in/out based on value grabbed from scroll wheel input
		float value = playerInputs.Player.CameraZoom.ReadValue<float>();
		if (playerCamera.orthographicSize > 3 && value == 120 || playerCamera.orthographicSize < 8 && value == -120)
			playerCamera.orthographicSize -= value / 480;
	}

	//ui action
	private void OnMainMenu()
	{
		MainMenuManager.Instance.ShowHideMainMenu();
	}
}
