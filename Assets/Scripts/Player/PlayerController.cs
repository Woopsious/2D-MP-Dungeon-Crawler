using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	PlayerInputActions _playerInputs;
	Rigidbody2D _rb;

	Vector2 moveDirection = Vector2.zero;
	public float speed;


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
		moveDirection = _playerInputs.Player.Movement.ReadValue<Vector2>();
	}

	private void FixedUpdate()
	{
		_rb.velocity = new Vector2(moveDirection.x * speed, moveDirection.y * speed);
	}
}
