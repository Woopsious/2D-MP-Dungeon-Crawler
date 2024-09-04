using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

	public static event Action OnSceneChangeStart;
	public static event Action OnSceneChangeFinish;

	public static bool isNewGame;	//dictates if data is restored + resets recievedStartingItems bool + set false on getting items
	public static string currentGameDataDirectory;

	public DungeonData currentDungeonData;

	public readonly string mainMenuName = "MainMenu";
	public readonly string hubAreaName = "HubArea";
	public readonly string dungeonLayoutOneName = "DungeonOne";
	public readonly string dungeonLayoutTwoName = "DungeonTwo";

	private void Awake()
	{
		if (Instance != null && Instance != this)
			Destroy(gameObject);
		else
		{
			Instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += SceneChangeFinished;
	}
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= SceneChangeFinished;
	}

	//scene change finish event
	private void SceneChangeFinished(Scene scene, LoadSceneMode mode)
	{
		OnSceneChangeFinish?.Invoke();
	}

	//loading different scenes
	public void LoadMainMenu(bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;
		StartCoroutine(LoadNewSceneAsync(mainMenuName, isNewGame));
	}
	public void LoadHubArea(bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;
		StartCoroutine(LoadNewSceneAsync(hubAreaName, isNewGame));
	}
	public void LoadDungeonOne()
	{
		GameManager.isNewGame = false;
		StartCoroutine(LoadNewSceneAsync(dungeonLayoutOneName, false));
	}
	public void LoadDungeonTwo()
	{
		GameManager.isNewGame = false;
		StartCoroutine(LoadNewSceneAsync(dungeonLayoutOneName, false));
	}

	//scene loading
	private IEnumerator LoadNewSceneAsync(string sceneToLoad, bool isNewGame)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
		OnSceneChangeStart?.Invoke();

		while (!asyncLoad.isDone)
			yield return null;
	}

	//pause game
	public void PauseGame(bool pauseGame)
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient) return;

        if (pauseGame)
			Time.timeScale = 0f;
		else
		 Time.timeScale = 1.0f;
    }
}
