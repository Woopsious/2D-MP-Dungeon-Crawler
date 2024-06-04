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

	public static bool isNewGame;

	public static DungeonStatModifier dungeonStatModifiers;
	public static List<DungeonChestData> dungeonChestData;

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

		dungeonStatModifiers = new DungeonStatModifier();
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += SceneChangeFinished;
	}
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= SceneChangeFinished;
	}

	private void SceneChangeFinished(Scene scene, LoadSceneMode mode)
	{
		OnSceneChangeFinish?.Invoke();
	}

	//saving and loading scenes
	public void LoadMainMenu(bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;
		StartCoroutine(LoadNewSceneAsync(mainMenuName, isNewGame));
	}
	public void LoadHubArea(bool isNewGame)
	{
		dungeonStatModifiers = new DungeonStatModifier();
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
		StartCoroutine(LoadNewSceneAsync(dungeonLayoutTwoName, false));
	}
	private IEnumerator LoadNewSceneAsync(string sceneToLoad, bool isNewGame)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
		OnSceneChangeStart?.Invoke();

		while (!asyncLoad.isDone)
			yield return null;
	}

	public void PauseGame()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient) return;
        Time.timeScale = 0f;
    }
	public void UnPauseGame()
	{
		if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient) return;
		Time.timeScale = 1.0f;
	}
}
