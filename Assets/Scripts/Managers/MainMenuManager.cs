using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	public GameObject mainMenuObj;
	public GameObject saveSlotsMenuObj;
	public static MainMenuManager Instance;

	public GameObject saveSlotContainer;

	public void Start()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		GameManager.OnSceneChange += ReloadSaveSlots;
	}
	private void OnDisable()
	{
		GameManager.OnSceneChange -= ReloadSaveSlots;
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
		GameManager.Instance.LoadMainMenu();
	}

	public void PlayGameButton()
	{
		HideMainMenu();
		GameManager.Instance.UnPauseGame();
	}
	public void StartNewGameButton()
	{
		GameManager.Instance.LoadHubArea();
	}

	//show/hide save slots menu
	public void ShowSaveSlotsMenu()
	{
		saveSlotsMenuObj.SetActive(true);
		GameManager.Instance.PauseGame();
	}
	public void HideSaveSlotsMenu()
	{
		saveSlotsMenuObj.SetActive(false);
		GameManager.Instance.UnPauseGame();
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
		SaveManager.Instance.ReloadSaveSlots(saveSlotContainer);
	}
}
