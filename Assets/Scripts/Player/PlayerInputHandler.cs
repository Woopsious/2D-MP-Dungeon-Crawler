using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
	public static PlayerInputHandler Instance;

	[Header("Input Action Asset")]
	public InputActionAsset playerControls;

	[Header("Action Map Name Ref")]
	[SerializeField] private string actionMapName = "Player";

	[Header("Input Action Name Refs")]
	//game actions
	[SerializeField] private string CameraZoom = "CameraZoom";
	[SerializeField] private string Movement = "Movement";
	[SerializeField] private string MainAttack = "MainAttack";
	[SerializeField] private string RightClick = "RightClick";
	[SerializeField] private string Interact = "Interact";
	[SerializeField] private string TabTargetingForwards = "TabTargetingForwards";
	[SerializeField] private string TabTargetingBackwards = "TabTargetingBackwards";

	//hotbar actions
	[SerializeField] private string ConsOne = "ConsumablesOne";
	[SerializeField] private string ConsTwo = "ConsumablesTwo";
	[SerializeField] private string AbilityOne = "AbilityOne";
	[SerializeField] private string AbilityTwo = "AbilityTwo";
	[SerializeField] private string AbilityThree = "AbilityThree";
	[SerializeField] private string AbilityFour = "AbilityFour";
	[SerializeField] private string AbilityFive = "AbilityFive";

	//ui actions
	[SerializeField] private string MainMenu = "MainMenu";
	[SerializeField] private string Inventory = "Inventory";
	[SerializeField] private string Journal = "Journal";
	[SerializeField] private string ClassSelection = "ClassSelection";
	[SerializeField] private string ClassSkillTree = "ClassSkillTree";
	[SerializeField] private string LearntAbilities = "LearntAbilities";

	[Header("Inputs Actions")]
	public PlayerInput _PlayerInput;

	//game actions
	public InputAction _CameraZoomAction;
	public InputAction _MovementAction;
	public InputAction _MainAttackAction;
	public InputAction _RightClickAction;
	public InputAction _InteractAction;
	public InputAction _TabTargetingForwards;
	public InputAction _TabTargetingBackwards;

	//hotbar actions
	public InputAction _ConsOneAction;
	public InputAction _ConsTwoAction;
	public InputAction _AbilityOneAction;
	public InputAction _AbilityTwoAction;
	public InputAction _AbilityThreeAction;
	public InputAction _AbilityFourAction;
	public InputAction _AbilityFiveAction;

	//ui actions
	public InputAction _MainMenuAction;
	public InputAction _InventoryAction;
	public InputAction _JournalAction;
	public InputAction _ClassSelectionAction;
	public InputAction _ClassSkillTreeAction;
	public InputAction _LearntAbilitiesAction;

	//game inputs
	public float CameraZoomInput {  get; private set; }
	public Vector2 MovementInput { get; private set; }
	public bool MainAttackInput { get; private set; }
	public bool RightClickInput { get; private set; }
	public bool InteractInput { get; private set; }
	public bool TabTargetingForwardsInput {  get; private set; }
	public bool TabTargetingBackwardsInput {  get; private set; }

	//hotbar actions
	public bool ConsOneInput { get; private set; }
	public bool ConsTwoInput { get; private set; }
	public bool AbilityOneInput { get; private set; }
	public bool AbilityTwoInput { get; private set; }
	public bool AbilityThreeInput { get; private set; }
	public bool AbilityFourInput { get; private set; }
	public bool AbilityFiveInput { get; private set; }

	//ui inputs
	public bool MainMenuInput { get; private set; }
	public bool InventoryInput { get; private set; }
	public bool JournalInput { get; private set; }
	public bool ClassSelectionInput { get; private set; }
	public bool ClassSkillTreeInput { get; private set; }
	public bool LearntAbilitiesInput { get; private set; }

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
		//game actions
		_CameraZoomAction = playerControls.FindActionMap(actionMapName).FindAction(CameraZoom);
		_MovementAction = playerControls.FindActionMap(actionMapName).FindAction(Movement);
		_MainAttackAction = playerControls.FindActionMap(actionMapName).FindAction(MainAttack);
		_RightClickAction = playerControls.FindActionMap(actionMapName).FindAction(RightClick);
		_InteractAction = playerControls.FindActionMap(actionMapName).FindAction(Interact);
		_TabTargetingForwards = playerControls.FindActionMap(actionMapName).FindAction(TabTargetingForwards);
		_TabTargetingBackwards = playerControls.FindActionMap(actionMapName).FindAction(TabTargetingBackwards);
		
		//hotbar actions
		_ConsOneAction = playerControls.FindActionMap(actionMapName).FindAction(ConsOne);
		_ConsTwoAction = playerControls.FindActionMap(actionMapName).FindAction(ConsTwo);
		_AbilityOneAction = playerControls.FindActionMap(actionMapName).FindAction(AbilityOne);
		_AbilityTwoAction = playerControls.FindActionMap(actionMapName).FindAction(AbilityTwo);
		_AbilityThreeAction = playerControls.FindActionMap(actionMapName).FindAction(AbilityThree);
		_AbilityFourAction = playerControls.FindActionMap(actionMapName).FindAction(AbilityFour);
		_AbilityFiveAction = playerControls.FindActionMap(actionMapName).FindAction(AbilityFive);

		//ui actions
		_MainMenuAction = playerControls.FindActionMap(actionMapName).FindAction(MainMenu);
		_InventoryAction = playerControls.FindActionMap(actionMapName).FindAction(Inventory);
		_JournalAction = playerControls.FindActionMap(actionMapName).FindAction(Journal);
		_ClassSelectionAction = playerControls.FindActionMap(actionMapName).FindAction(ClassSelection);
		_ClassSkillTreeAction = playerControls.FindActionMap(actionMapName).FindAction(ClassSkillTree);
		_LearntAbilitiesAction = playerControls.FindActionMap(actionMapName).FindAction(LearntAbilities);
	}
	private void UpdateInputs()
	{
		//game inputs
		CameraZoomInput = _CameraZoomAction.ReadValue<float>();
		MovementInput = _MovementAction.ReadValue<Vector2>();
		MainAttackInput = _MainAttackAction.WasPressedThisFrame();
		RightClickInput = _RightClickAction.WasPressedThisFrame();
		InteractInput = _InteractAction.WasPressedThisFrame();
		TabTargetingForwardsInput = _TabTargetingForwards.WasPressedThisFrame();
		TabTargetingBackwardsInput = _TabTargetingBackwards.WasPressedThisFrame();
		
		//hotbar actions
		ConsOneInput = _ConsOneAction.WasPressedThisFrame();
		ConsTwoInput = _ConsTwoAction.WasPressedThisFrame();
		AbilityOneInput = _AbilityOneAction.WasPressedThisFrame();
		AbilityTwoInput = _AbilityTwoAction.WasPressedThisFrame();
		AbilityThreeInput = _AbilityThreeAction.WasPressedThisFrame();
		AbilityFourInput = _AbilityFourAction.WasPressedThisFrame();
		AbilityFiveInput = _AbilityFiveAction.WasPressedThisFrame();

		//ui inputs
		MainMenuInput = _MainMenuAction.WasPressedThisFrame();
		InventoryInput = _InventoryAction.WasPressedThisFrame();
		JournalInput = _JournalAction.WasPressedThisFrame();
		ClassSelectionInput = _ClassSelectionAction.WasPressedThisFrame();
		ClassSkillTreeInput = _ClassSkillTreeAction.WasPressedThisFrame();
		LearntAbilitiesInput = _LearntAbilitiesAction.WasPressedThisFrame();

		//incase of keybinds not working, check InputManager SceneComponent names
	}
}
