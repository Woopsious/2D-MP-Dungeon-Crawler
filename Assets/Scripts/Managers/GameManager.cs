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

	public static bool isNewGame;   //dictates if data is restored + resets recievedStartingItems bool + set false on getting items

	public static event Action sceneFinishedLoading;

	public static string currentGameDataDirectory;

	/// <summary>
	/// if i ever want to make it so u can go from a standard dungeon to a boss dungeon. to save a lot of headache and it might possibly 
	/// be funner, is once u kill the boss in the boss dungeon it returns u to the hub instead of the standard dungeon u entered from.
	/// </summary>
	public DungeonData currentDungeonData;

	public List<string> dungeonSceneNamesList = new List<string>();
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

	public GameObject CameraPrefab;
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

		GetAllDungeonSceneNames();
		GetAllBossSceneNames();

		if (MainMenuManager.Instance == null)
			StartCoroutine(LoadUiScene());

		if (SceneHandler.Instance == null) //if != null starting scene = different scene
			LoadMainMenu(); //starting scene = MainScene
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
	private void GetAllDungeonSceneNames()
	{
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
		{
			string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
			string[] splitScenePath = scenePath.Split('/');
			string sceneFile = splitScenePath[splitScenePath.Length - 1];
			string sceneName = sceneFile.Split('.')[0];
			if (sceneName.Contains("Boss")) continue;
			if (sceneName.Contains("Dungeon"))
				dungeonSceneNamesList.Add(sceneName);
		}
	}

	//loading different scenes
	public void LoadMainMenu()
	{
		SaveManager.Instance.GameData = new GameData();

		StartCoroutine(LoadSceneAsync(menuScene, false));
	} 
	public void LoadHubArea(bool isNewGame)
	{
		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(hubScene, isNewGame);
		else
			StartCoroutine(LoadSceneAsync(hubScene, isNewGame));
	}
	public void LoadDungeonOne()
	{
		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(dungeonOneScene, false);
		else
			StartCoroutine(LoadSceneAsync(dungeonOneScene, false));
	}
	public void LoadDungeonTwo()
	{
		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(dungeonOneScene, false);
		else
			StartCoroutine(LoadSceneAsync(dungeonOneScene, false));
	}
	public void LoadRandomBossDungeon()
	{
		int bossDungeonIndex = Utilities.GetRandomNumber(bossSceneNamesList.Count - 1);

		if (bossDungeonIndex == 0)
			LoadBossDungoenOne();
		else if (bossDungeonIndex == 1)
			LoadBossDungoenTwo();
	}
	private void LoadBossDungoenOne()
	{
		if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isMultiplayer)
			LoadNewMultiplayerScene(bossDungeonOneScene, false);
		else
			StartCoroutine(LoadSceneAsync(bossDungeonOneScene, false));
	}
	private void LoadBossDungoenTwo()
	{
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
		Debug.LogError("load scene async");

		GameManager.isNewGame = isNewGame;
		Instance.PauseGame(false);
		AsyncOperation asyncLoadScene = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

		while (!asyncLoadScene.isDone)
			yield return null;
	}
	private void LoadNewMultiplayerScene(string sceneToLoad, bool isNewGame)
	{
		GameManager.isNewGame = isNewGame;

		NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
	}

	//SCENE LOAD EVENT
	private void OnLoadSceneFinish(Scene newLoadedScene, LoadSceneMode mode)
	{
		Debug.LogError("loaded scene name: " + newLoadedScene.name + " at: " + DateTime.Now.ToString());

		UpdateActiveSceneToMainScene(newLoadedScene);

		TryUnloadPreviousScene(previouslyLoadedScene);
		previouslyLoadedScene = newLoadedScene;

		if (LocalPlayerCamera == null)
			SpawnPlayerCamera();

		if (SceneIsHubOrDungeonScene(newLoadedScene) && Localplayer == null)
			SpawnSinglePlayerObject();

		if (newLoadedScene.name == Instance.hubScene && !isNewGame)
			SaveManager.Instance.RestoreSaveGameData();
		else
			SaveManager.Instance.RestoreDungeonData();
	}

	//SCENE UNLOADING
	private void TryUnloadPreviousScene(Scene newLoadedScene)
	{
		if (newLoadedScene.name.IsNullOrEmpty() || previouslyLoadedScene.name.IsNullOrEmpty()) return; //no scene to unload
		if (NewLoadedSceneNeverUnloaded(newLoadedScene)) return;

		StartCoroutine(UnloadSceneAsync(previouslyLoadedScene.name));
		previouslyLoadedScene = newLoadedScene;
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
	}

	//SCENE CHECKS
	private bool NewLoadedSceneNeverUnloaded(Scene newLoadedScene)
	{
		if (newLoadedScene.name == mainScene || newLoadedScene.name == uiScene)
			return true;
		else return false;
	}
	public bool SceneIsHubOrDungeonScene(Scene newLoadedScene)
	{
		if (newLoadedScene.name.Contains("Dungeon") || newLoadedScene.name.Contains("Hub"))
		{
			//Debug.LogError("Scene is dungeon or hub");
			return true;
		}
		else
		{
			//Debug.LogError("Scene isnt dungeon or hub");
			return false;
		}
	}

	//SET ACTIVE SCENE
	private void UpdateActiveSceneToMainScene(Scene newLoadedScene)
	{
		Scene currentActiveScene = SceneManager.GetActiveScene();
		if (currentActiveScene.name == mainScene) return;

		if (SceneIsHubOrDungeonScene(currentActiveScene))
			SpawnSinglePlayerObject();

		SceneManager.SetActiveScene(newLoadedScene);
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
		{
			GameObject obj = Instantiate(PlayerPrefab);
			PlayerController player = obj.GetComponent<PlayerController>();
			Instance.UpdateLocalPlayerInstance(player);
		}
	}
	public void SpawnNetworkedPlayerObject(ulong clientNetworkIdOfOwner)
	{
		GameObject playerObj = Instantiate(PlayerPrefab);
		playerObj.transform.position = DungeonHandler.Instance.GetDungeonEnterencePortal(playerObj);
		NetworkObject playerNetworkedObj = playerObj.GetComponent<NetworkObject>();
		playerNetworkedObj.SpawnAsPlayerObject(clientNetworkIdOfOwner, true);
	}

	private void UpdateLocalPlayerInstance(PlayerController newLocalPlayer)
	{
		if (Localplayer != null && Localplayer != newLocalPlayer)
			Destroy(Localplayer.gameObject);

		Localplayer = newLocalPlayer;
	}

	//PLAYER CAMERA SPAWNING
	private void SpawnPlayerCamera()
	{
		GameObject cameraObj = Instantiate(CameraPrefab);
		LocalPlayerCamera = cameraObj.GetComponent<Camera>();
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
