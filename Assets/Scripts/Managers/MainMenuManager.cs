using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	public GameObject mainMenuObj;
	public static MainMenuManager Instance;

	public void Start()
	{
		Instance = this;
	}
	public void Update()
	{
		//ShowHideMainMenuKeybind();
	}

	//button actions

	public void QuiteGameButton()
	{
		Application.Quit();
	}

	public void PlayGameButton()
	{
		HideMainMenu();
		GameManager.Instance.UnPauseGame();
	}

	//show/hide main menu
	public void ShowHideMainMenu()
	{
		if (!mainMenuObj.activeInHierarchy)
			ShowMainMenu();
		else if (mainMenuObj.activeInHierarchy)
			HideMainMenu();
	}
	public void ShowMainMenu()
	{
		mainMenuObj.SetActive(true);
		GameManager.Instance.PauseGame();
	}
	public void HideMainMenu()
	{
		mainMenuObj.SetActive(false);
		GameManager.Instance.UnPauseGame();
	}
}
