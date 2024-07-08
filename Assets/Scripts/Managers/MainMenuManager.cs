using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	public static MainMenuManager Instance;

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

	private void Awake()
	{
		Instance = this;
	}
	private void Start()
	{
		EnableMainMenuButtons();
	}
	private void EnableMainMenuButtons()
	{
		//enable buttons unique to main menu scene
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
		GameManager.Instance.UnPauseGame();
	}
	public void StartNewGameButton()
	{
		GameManager.Instance.LoadHubArea(true);
	}

	//show/hide panel
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
		saveLoadGamePanel.SetActive(false);
		mainMenuPanel.SetActive(true);
		GameManager.Instance.PauseGame();
	}
	public void HideMainMenu()
	{
		mainMenuPanel.SetActive(false);
		GameManager.Instance.UnPauseGame();
	}

	public void ShowKeybindsMenu()
	{
		mainMenuPanel.SetActive(false);
		keybindsSettingsPanel.SetActive(true);
		KeyboardKeybindsPanel.SetActive(true);
	}
	public void HideKeybindsMenu()
	{
		mainMenuPanel.SetActive(true);
		keybindsSettingsPanel.SetActive(false);
		KeyboardKeybindsPanel.SetActive(false);
	}
	//SAVE LOAD GAME PANEL ACTIONS
	public void ReloadSaveSlots()
	{
		SaveManager.Instance.ReloadAutoSaveSlot(autoSaveContainer);
		SaveManager.Instance.ReloadSaveSlots(saveSlotContainer);
	}

	//show/hide panel
	public void ShowSaveSlotsMenu()
	{
		ReloadSaveSlots();
		saveLoadGamePanel.SetActive(true);
	}
	public void HideSaveSlotsMenu()
	{
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
