using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.InputSystem.Samples.RebindUI;

public class MainMenuManager : MonoBehaviour
{
	public static MainMenuManager Instance;

	[HideInInspector] public Canvas canvasRef;

	[Header("MainMenu Panel")]
	public GameObject mainMenuPanel;
	public GameObject runtimeUiContainer;

	[Header("Unique Main Menu Scene Buttons")]
	public GameObject quitGameButton;
	public GameObject startNewGameButton;

	[Header("Save/load Game Panel")]
	public GameObject saveLoadGamePanel;
	public GameObject confirmActionPanel;
	public GameObject confirmActionButton;
	public GameObject cancelActionButton;
	public TMP_Text actionTypeText;

	public GameObject autoSaveContainer;
	public GameObject saveSlotContainer;

	[Header("Keybinds Panel")]
	public GameObject keybindsSettingsPanel;
	public GameObject KeyboardKeybindsPanel;

	[Header("Player Settings")]
	public GameObject playerSettingsPanel;

	[Header("Audio Panel")]
	public GameObject audioSettingsPanel;

	private void Awake()
	{
		Instance = this;
		canvasRef = GetComponent<Canvas>();
	}
	private void Start()
	{
		EnableMainMenuButtons();
	}


	private void EnableMainMenuButtons()
	{
		//enable buttons unique to main menu scene
		if (GameManager.Instance == null)
		{
			Debug.LogWarning("Game Manager Instance not found hiding title screen buttons, ignore if testing");
			return;
		}

		if (Utilities.GetCurrentlyActiveScene("TestingScene")) return;
		if (!Utilities.GetCurrentlyActiveScene(GameManager.Instance.mainMenuName)) return;

		quitGameButton.SetActive(true);
		startNewGameButton.SetActive(true);
	}

	//MAIN MENU PANEL ACTIONS
	public void ButtonFunctionNotSetUp()
	{
		Debug.LogWarning("Button function not yet set up");
	}

	public void QuitGameButton()
	{
		Application.Quit();
	}
	public void QuitToMainMenuButton()
	{
		GameManager.Instance.LoadMainMenu(false);
	}

	public void PlayGameButton()
	{
		HideMainMenu();
		GameManager.Instance.PauseGame(false);
	}
	public void StartNewGameButton()
	{
		GameManager.Instance.LoadHubArea(true);
	}

	//SHOW/HIDE PANELS
	//main menu panel
	public void ShowHideMainMenuKeybind()
	{
		if (confirmActionPanel.activeInHierarchy) return;

		if (!mainMenuPanel.activeInHierarchy)
			ShowMainMenu();
		else if (mainMenuPanel.activeInHierarchy)
			HideMainMenu();
	}
	public void ShowMainMenu()
	{
		HideSaveSlotsMenu();
		HideKeybindsMenu();
		HidePlayerSettingsMenu();
		HideAudioMenu();
		mainMenuPanel.SetActive(true);
		GameManager.Instance.PauseGame(true);
	}
	public void HideMainMenu()
	{
		mainMenuPanel.SetActive(false);
		GameManager.Instance.PauseGame(false);
	}

	//keybinds panel
	public void ShowKeybindsMenu()
	{
		mainMenuPanel.SetActive(false);
		keybindsSettingsPanel.SetActive(true);
		KeyboardKeybindsPanel.SetActive(true);

		SaveManager.Instance.LoadPlayerData();
	}
	public void HideKeybindsMenu()
	{
		mainMenuPanel.SetActive(true);
		keybindsSettingsPanel.SetActive(false);
		KeyboardKeybindsPanel.SetActive(false);

		SaveManager.Instance.SavePlayerData();
	}

	//player settings panel
	public void ShowPlayerSettingsMenu()
	{
		mainMenuPanel.SetActive(false);
		playerSettingsPanel.SetActive(true);

		SaveManager.Instance.LoadPlayerData();
	}
	public void HidePlayerSettingsMenu()
	{
		mainMenuPanel.SetActive(true);
		playerSettingsPanel.SetActive(false);

		SaveManager.Instance.SavePlayerData();
	}

	//audio panel
	public void ShowAudioMenu()
	{
		mainMenuPanel.SetActive(false);
		audioSettingsPanel.SetActive(true);

		AudioManager.Instance.SetVolumeSliderValues();
		SaveManager.Instance.LoadPlayerData();
	}
	public void HideAudioMenu()
	{
		mainMenuPanel.SetActive(true);
		audioSettingsPanel.SetActive(false);

		AudioManager.Instance.UpdateAudioVolume();
		SaveManager.Instance.SavePlayerData();
	}

	//SAVE LOAD GAME PANEL ACTIONS
	public void ReloadSaveSlots()
	{
		SaveManager.Instance.ReloadAutoSaveSlot(autoSaveContainer);
		SaveManager.Instance.ReloadSaveSlots(saveSlotContainer);
	}

	//show/hide panel for saves
	public void ShowSaveSlotsMenu()
	{
		ReloadSaveSlots();
		mainMenuPanel.SetActive(false);
		saveLoadGamePanel.SetActive(true);
	}
	public void HideSaveSlotsMenu()
	{
		mainMenuPanel.SetActive(true);
		saveLoadGamePanel.SetActive(false);
	}
	public void ShowConfirmActionPanel(SaveSlotDataUi saveData, int actionType)
	{
		if (actionType == 0)
			actionTypeText.text = "Overwrite save slot " + saveData.slotNumber;
		else if (actionType == 1)
			actionTypeText.text = "Load game slot " + saveData.slotNumber;
		else if (actionType == 2)
			actionTypeText.text = "Delete game slot " + saveData.slotNumber;

		confirmActionPanel.SetActive(true);
	}
	public void HideConfirmActionPanel()
	{
		confirmActionPanel.SetActive(false);
	}
}
