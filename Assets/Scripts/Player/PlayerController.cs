using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
	PlayerInputActions _playerInputs;
	Rigidbody2D _rb;

	Vector2 moveDirection = Vector2.zero;
	public float speed;

	public GameObject weapon;
	public Collider2D weaponCollider;

	private void Awake()
	{
		_playerInputs = new PlayerInputActions();
		_rb = GetComponent<Rigidbody2D>();
	}

	private void OnEnable()
	{
		if (_playerInputs == null)
			_playerInputs = new PlayerInputActions();
		_playerInputs.Enable();
	}

	private void OnDisable()
	{
		_playerInputs.Disable();
	}

	private void Update()
	{

	}

	private void FixedUpdate()
	{
		moveDirection = _playerInputs.Player.Movement.ReadValue<Vector2>();
		_rb.velocity = new Vector2(moveDirection.x * speed, moveDirection.y * speed);
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
		float value = _playerInputs.Player.CameraZoom.ReadValue<float>();
		if (Camera.main.orthographicSize > 3 && value == 120 || Camera.main.orthographicSize < 8 && value == -120)
			Camera.main.orthographicSize -= value / 480;
	}

	//ui action
	private void OnMainMenu()
	{
		MainMenuManager.Instance.ShowHideMainMenu();
	}
}
