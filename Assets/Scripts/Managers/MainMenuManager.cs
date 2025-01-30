using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

	[Header("Play MP Button")]
	public Button playMpButton;
	public TMP_Text playMpButtonText;

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
		if (Utilities.SceneIsActive(GameManager.Instance.menuScene)) //show ui if menu scene alreadly loaded
		{
			Instance.ShowMainMenu();
			PlayerHudUi.Instance.HotbarPanelUi.SetActive(false);
			PlayerSelectedTargetsUi.Instance.selectedTargetPanelUi.SetActive(false);

			quitGameButton.SetActive(true);
			startNewGameButton.SetActive(true);
		}
		else //hide it
		{
			Instance.HideMainMenu();
			PlayerHudUi.Instance.HotbarPanelUi.SetActive(true);
			PlayerSelectedTargetsUi.Instance.selectedTargetPanelUi.SetActive(true);

			quitGameButton.SetActive(false);
			startNewGameButton.SetActive(false);
		}

		EnableMainMenuButtons();
		SetActionForPlayMpButton();
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += UpdatePlayerUiOnSceneChange;
	}
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= UpdatePlayerUiOnSceneChange;
	}

	private void UpdatePlayerUiOnSceneChange(Scene loadedScene, LoadSceneMode mode)
	{
		if (GameManager.Instance == null)
		{
			//load the main scene
		}

		if (loadedScene.name == GameManager.Instance.mainScene || loadedScene.name == GameManager.Instance.uiScene) return;
		if (loadedScene.name == GameManager.Instance.menuScene) //show ui if new scene main menu
		{
			Instance.ShowMainMenu();
			PlayerHudUi.Instance.HotbarPanelUi.SetActive(false);
			PlayerSelectedTargetsUi.Instance.selectedTargetPanelUi.SetActive(false);

			quitGameButton.SetActive(true);
			startNewGameButton.SetActive(true);
		}
		else //hide it
		{
			Instance.HideMainMenu();
			PlayerHudUi.Instance.HotbarPanelUi.SetActive(true);
			PlayerSelectedTargetsUi.Instance.selectedTargetPanelUi.SetActive(true);

			quitGameButton.SetActive(false);
			startNewGameButton.SetActive(false);
		}
	}

	private void EnableMainMenuButtons()
	{
		if (!Utilities.SceneIsActive(GameManager.Instance.mainScene)) return;

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
		GameManager.Instance.LoadMainMenu();
	}

	public void PlayGameButton()
	{
		HideMainMenu();
		GameManager.Instance.PauseGame(false);
	}
	public void StartNewGameButton()
	{
		GameManager.Instance.LoadHubArea(true, GameManager.GameDataReloadMode.noReload);
	}


	//mp button actions + checks
	private void SetActionForPlayMpButton()
	{
		playMpButton.onClick.RemoveAllListeners();

		if (LobbyManager.Instance != null && LobbyManager.Instance._Lobby != null) //player in lobby show lobby ui
		{
			playMpButton.onClick.AddListener(delegate { ShowLobbyUiWhenPlayerInLobby(); });
			playMpButton.interactable = true;
			playMpButtonText.color = Color.black;
			playMpButtonText.text = "Open Lobby Ui";
		}
		else //player not in lobby show play mp process
		{
			if (Utilities.SceneIsActive(GameManager.Instance.hubScene))
			{
				playMpButton.onClick.AddListener(delegate { PlayMultiplayer(); });
				playMpButton.interactable = true;
				playMpButtonText.color = Color.black;
				playMpButtonText.text = "Play Multiplayer";
			}
			else
			{
				playMpButton.interactable = false;
				playMpButtonText.color = new Color(0.8f, 0, 0);
				playMpButtonText.text = "Mp available in Hub";
			}
		}
	}
	public void PlayMultiplayer()
	{
		mainMenuPanel.SetActive(false);

		SaveManager.Instance.AutoSaveData();
		MultiplayerMenuUi.Instance.ShowMpMenuUi();
	}

	//show MpLobbyUi when players in lobby
	public void ShowLobbyUiWhenPlayerInLobby()
	{
		if (LobbyManager.Instance._Lobby != null)
		{
			MainMenuManager.Instance.HideMainMenu();
			LobbyUi.Instance.ShowLobbyUi();
		}
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
		SetActionForPlayMpButton();
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
