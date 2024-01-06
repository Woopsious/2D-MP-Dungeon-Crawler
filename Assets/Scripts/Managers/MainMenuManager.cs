using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	public GameObject mainMenuObj;

	public void Start()
	{
		
	}
	public void Update()
	{
		ShowHideMainMenuKeybind();
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
	public void ShowHideMainMenuKeybind()
	{
		/*
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (!mainMenuObj.activeInHierarchy)
				ShowMainMenu();
			else if (mainMenuObj.activeInHierarchy)
				HideMainMenu();
		}
		*/
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
