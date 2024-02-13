using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	public GameObject mainMenuObj;
	public GameObject saveSlotsMenuObj;
	public static MainMenuManager Instance;

	public GameObject autoSaveContainer;
	public GameObject saveSlotContainer;

	private void Start()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		GameManager.OnSceneChange += ShowClassSelectionOnNewGameStart;
	}
	private void OnDisable()
	{
		GameManager.OnSceneChange -= ShowClassSelectionOnNewGameStart;
	}

	private void ShowClassSelectionOnNewGameStart(bool isNewGame)
	{
		//PlayerClassesUi.Instance.gameObject
	}

	//button actions
	public void ButtonFunctionNotSetUp()
	{
		Debug.LogWarning("Button function not yet set up");
	}

	public void QuiteGameButton()
	{
		Application.Quit();
	}
	public void QuiteToMainMenuButton()
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

	//show/hide save slots menu
	public void ShowSaveSlotsMenu()
	{
		ReloadSaveSlots();
		saveSlotsMenuObj.SetActive(true);
		GameManager.Instance.PauseGame();
	}
	public void HideSaveSlotsMenu()
	{
		saveSlotsMenuObj.SetActive(false);
		GameManager.Instance.PauseGame();
	}

	//show/hide main menu
	public void ShowHideMainMenuKeybind()
	{
		saveSlotsMenuObj.SetActive(false);

		if (!mainMenuObj.activeInHierarchy)
			ShowMainMenu();
		else if (mainMenuObj.activeInHierarchy)
			HideMainMenu();
	}
	public void ShowMainMenu()
	{
		saveSlotsMenuObj.SetActive(false);
		mainMenuObj.SetActive(true);
		GameManager.Instance.PauseGame();
	}
	public void HideMainMenu()
	{
		mainMenuObj.SetActive(false);
		GameManager.Instance.UnPauseGame();
	}

	public void ReloadSaveSlots()
	{
		SaveManager.Instance.ReloadAutoSaveSlot(autoSaveContainer);
		SaveManager.Instance.ReloadSaveSlots(saveSlotContainer);
	}
}
