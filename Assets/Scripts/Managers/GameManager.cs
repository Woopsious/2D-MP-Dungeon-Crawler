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

	/// <summary>
	/// if i ever want to make it so u can go from a standard dungeon to a boss dungeon. to save a lot of headache and it might possibly 
	/// be funner, is once u kill the boss in the boss dungeon it returns u to the hub instead of the standard dungeon u entered from.
	/// </summary>
	public DungeonData currentDungeonData;

	public readonly string mainMenuName = "MainMenu";
	public readonly string hubAreaName = "HubArea";

	public readonly string dungeonLayoutOneName = "DungeonOne";
	public readonly string dungeonLayoutTwoName = "DungeonTwo";

	public readonly string bossDungeonLayoutOneName = "BossDungeonOne";
	public readonly string bossDungeonLayoutTwoName = "BossDungeonTwo";

	private List<string> bossSceneNamesList = new List<string>();

	private void Awake()
	{
		//if vSyncCount == 0, .targetFrameRate is enabled. (default setting)
		//if vSyncCount == 1, .targetFrameRate is ignored and frame rate will try match monitor refresh rate.
		//QualitySettings.vSyncCount = 1;
		//Application.targetFrameRate = 144;

		if (Instance != null && Instance != this)
			Destroy(gameObject);
		else
		{
			Instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		GetAllBossSceneNames();
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += SceneChangeFinished;
	}
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= SceneChangeFinished;
	}
	private void Update()
	{
		//Debug.LogError("vsync: " + QualitySettings.vSyncCount);
		//Debug.LogError("framerate: " + Application.targetFrameRate);
	}

	private void GetAllBossSceneNames()
	{
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
		{
			string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
			string[] splitScenePath = scenePath.Split('/');
			string sceneFile = splitScenePath[splitScenePath.Length - 1];
			string sceneName = sceneFile.Split('.')[0];
			if (sceneName.Contains("Boss"))
				bossSceneNamesList.Add(sceneName);
		}
	}

	//scene change finish event
	private void SceneChangeFinished(Scene scene, LoadSceneMode mode)
	{
		OnSceneChangeFinish?.Invoke();

		if (scene.name == mainMenuName) return;
		MainMenuManager.Instance.HideMainMenu();
		Instance.PauseGame(false);
	}

	//loading different scenes
	public void LoadMainMenu(bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;
		SaveManager.Instance.GameData = new GameData();

		StartCoroutine(LoadNewSceneAsync(mainMenuName, isNewGame));
	}
	public void LoadHubArea(bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;

		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(hubAreaName, isNewGame);
		else
			StartCoroutine(LoadNewSceneAsync(hubAreaName, isNewGame));
	}
	public void LoadDungeonOne()
	{
		GameManager.isNewGame = false;

		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(bossDungeonLayoutOneName, false);
		else
			StartCoroutine(LoadNewSceneAsync(dungeonLayoutOneName, false));
	}
	public void LoadDungeonTwo()
	{
		GameManager.isNewGame = false;

		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(dungeonLayoutOneName, false);
		else
			StartCoroutine(LoadNewSceneAsync(dungeonLayoutOneName, false));
	}
	public void LoadRandomBossDungeon()
	{
		GameManager.isNewGame = false;
		int bossDungeonIndex = Utilities.GetRandomNumber(bossSceneNamesList.Count - 1);

		if (bossDungeonIndex == 0)
			LoadBossDungoenOne();
		else if (bossDungeonIndex == 1)
			LoadBossDungoenTwo();
	}
	private void LoadBossDungoenOne()
	{
		GameManager.isNewGame = false;

		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(bossDungeonLayoutOneName, false);
		else
			StartCoroutine(LoadNewSceneAsync(bossDungeonLayoutOneName, false));
	}
	private void LoadBossDungoenTwo()
	{
		GameManager.isNewGame = false;

		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(bossDungeonLayoutTwoName, false);
		else
			StartCoroutine(LoadNewSceneAsync(bossDungeonLayoutTwoName, false));
	}

	//scene loading
	private IEnumerator LoadNewSceneAsync(string sceneToLoad, bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;

		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
		OnSceneChangeStart?.Invoke();

		while (!asyncLoad.isDone)
			yield return null;
	}
	private void LoadNewMultiplayerScene(string sceneToLoad, bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;

		NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
		OnSceneChangeStart?.Invoke();
	}

	//pause game
	public void PauseGame(bool pauseGame)
    {
        if (pauseGame && !MultiplayerManager.Instance.isMultiplayer)
			Time.timeScale = 0f;
		else
		 Time.timeScale = 1.0f;
    }
}
