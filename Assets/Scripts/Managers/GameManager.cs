using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

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

	public List<string> bossSceneNamesList = new List<string>();

	public List<Scene> activeScenesList = new List<Scene>();
	public List<string> activeScenesNameList = new List<string>();

	public readonly string mainScene = "MainScene";
	public readonly string menuScene = "MenuScene";
	public readonly string uiScene = "UiScene";

	public readonly string hubScene = "HubScene";
	public readonly string dungeonOneScene = "DungeonOneScene";
	public readonly string bossDungeonOneScene = "BossDungeonOneScene";

	public Scene previouslyLoadedScene;

	public GameObject PlayerPrefab;
	public static PlayerController Localplayer { get; private set; }
	public static Camera LocalPlayerCamera;

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

		if (MainMenuManager.Instance == null)
			StartCoroutine(LoadUiScene());

		if (SceneHandler.Instance == null) //if != null starting scene = different scene
			LoadMainMenu(true); //starting scene = MainScene
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnLoadSceneFinish;
		SceneManager.sceneUnloaded += OnUnloadSceneFinish;
	}
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLoadSceneFinish;
		SceneManager.sceneUnloaded -= OnUnloadSceneFinish;
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

	//loading different scenes
	public void LoadMainMenu(bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;
		SaveManager.Instance.GameData = new GameData();

		StartCoroutine(LoadSceneAsync(menuScene, isNewGame));
	}
	public void LoadHubArea(bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;

		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(hubScene, isNewGame);
		else
			StartCoroutine(LoadSceneAsync(hubScene, isNewGame));
	}
	public void LoadDungeonOne()
	{
		GameManager.isNewGame = false;

		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(dungeonOneScene, false);
		else
			StartCoroutine(LoadSceneAsync(dungeonOneScene, false));
	}
	public void LoadDungeonTwo()
	{
		GameManager.isNewGame = false;

		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(dungeonOneScene, false);
		else
			StartCoroutine(LoadSceneAsync(dungeonOneScene, false));
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
			LoadNewMultiplayerScene(bossDungeonOneScene, false);
		else
			StartCoroutine(LoadSceneAsync(bossDungeonOneScene, false));
	}
	private void LoadBossDungoenTwo()
	{
		GameManager.isNewGame = false;

		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(bossDungeonOneScene, false);
		else
			StartCoroutine(LoadSceneAsync(bossDungeonOneScene, false));
	}

	//SCENE LOADING
	private IEnumerator LoadUiScene()
	{
		AsyncOperation asyncLoadScene = SceneManager.LoadSceneAsync(uiScene, LoadSceneMode.Additive);

		while (!asyncLoadScene.isDone)
			yield return null;
	}
	private IEnumerator LoadSceneAsync(string sceneToLoad, bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;
		AsyncOperation asyncLoadScene = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

		OnSceneChangeStart?.Invoke();

		while (!asyncLoadScene.isDone)
			yield return null;
	}
	private void LoadNewMultiplayerScene(string sceneToLoad, bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;

		NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
		OnSceneChangeStart?.Invoke();
	}

	//SCENE LOAD EVENT
	private void OnLoadSceneFinish(Scene newLoadedScene, LoadSceneMode mode)
	{
		Debug.LogError("loaded scene name: " + newLoadedScene.name);

		//add loadedscene to active scene list

		//SceneManager.SetActiveScene(newlyLoadedScene);

		if (newLoadedScene.name == mainScene || newLoadedScene.name == uiScene) return; //scenes never unload

		UnloadPreviousScene(previouslyLoadedScene.name);
		previouslyLoadedScene = newLoadedScene;

		Debug.LogError("current active scene: " + SceneManager.GetActiveScene().name);
	}

	//SCENE UNLOADING
	private void UnloadPreviousScene(string previousSceneName)
	{
		if (previousSceneName.IsNullOrEmpty()) return; //no scene to unload
		StartCoroutine(UnloadSceneAsync(previousSceneName));
	}
	private IEnumerator UnloadSceneAsync(string sceneToUnLoad)
	{
		AsyncOperation asyncLoadScene = SceneManager.UnloadSceneAsync(sceneToUnLoad);

		while (!asyncLoadScene.isDone)
			yield return null;
	}

	//SCENE UNLOAD EVENT
	private void OnUnloadSceneFinish(Scene unLoadedScene)
	{
		Debug.LogError("unloaded scene name: " + unLoadedScene.name);

		//remove unloadedscene from active scene list
	}

	//PLAYER OBJECT SPAWNING
	private void SpawnSinglePlayerObject()
	{
		if (FindObjectOfType<PlayerController>() != null)
		{
			Debug.LogWarning("player prefab already exists in scene, ignore if testing");
			return;
		}

		if (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.isMultiplayer)
			Instantiate(PlayerPrefab);
	}
	public void SpawnNetworkedPlayerObject(ulong clientNetworkIdOfOwner)
	{
		Debug.LogError("spawned player obj");

		GameObject playerObj = Instantiate(PlayerPrefab);
		playerObj.transform.position = DungeonHandler.Instance.GetDungeonEnterencePortal(playerObj);
		NetworkObject playerNetworkedObj = playerObj.GetComponent<NetworkObject>();
		playerNetworkedObj.SpawnAsPlayerObject(clientNetworkIdOfOwner, true);
	}

	public void UpdateLocalPlayerInstance(PlayerController newLocalPlayer)
	{
		if (Localplayer != null && Localplayer != newLocalPlayer)
			Destroy(Localplayer.gameObject);

		Localplayer = newLocalPlayer;
	}

	//pause game
	public void PauseGame(bool pauseGame)
    {
		if (MultiplayerManager.Instance != null)
		{
			if (pauseGame && !MultiplayerManager.Instance.isMultiplayer)
				Time.timeScale = 0f;
			else
				Time.timeScale = 1.0f;
		}
		else
		{
			if (pauseGame)
				Time.timeScale = 0f;
			else
				Time.timeScale = 1.0f;
		}
    }
}
