using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
	public static PlayerInputHandler Instance;

	[Header("Input Action Asset")]
	[SerializeField] private InputActionAsset playerControls;

	[Header("Action Map Name Ref")]
	[SerializeField] private string actionMapName = "Player";

	[Header("Input Action Name Refs")]
	[SerializeField] private string move = "Movement";
	[SerializeField] private string attack = "MainAttack";

	public PlayerInput _PlayerInput;

	public InputAction _MoveAction;
	public InputAction _MainAttackAction;

	public Vector2 MoveInput { get; private set; }
	public bool AttackInput { get; private set; }

	private void Awake()
	{
		if (Instance == null)
			Instance = this;

		SetUpInputActions();
	}
	private void Update()
	{
		UpdateInputs();
	}

	private void SetUpInputActions()
	{
		_MoveAction = playerControls.FindActionMap(actionMapName).FindAction(move);
		_MainAttackAction = playerControls.FindActionMap(actionMapName).FindAction(attack);
	}
	private void UpdateInputs()
	{
		MoveInput = _MoveAction.ReadValue<Vector2>();
		AttackInput = _MainAttackAction.WasPerformedThisFrame();
	}
}
