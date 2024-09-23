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
		Debug.Log("boss scenes count: " + bossSceneNamesList.Count);
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
		StartCoroutine(LoadNewSceneAsync(bossDungeonLayoutOneName, false));
	}
	private void LoadBossDungoenTwo()
	{
		GameManager.isNewGame = false;
		StartCoroutine(LoadNewSceneAsync(bossDungeonLayoutTwoName, false));
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
