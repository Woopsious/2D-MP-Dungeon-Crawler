using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

	public static bool isNewGame;   //dictates if data is restored + resets recievedStartingItems bool + set false on getting items

	private GameDataReloadMode gameDataReloadMode;
	public enum GameDataReloadMode
	{
		reloadAllScenesAndData, reloadGameData, reloadDungeonData, noReload
	}

	public static string currentGameDataDirectory;

	public readonly string mainScene = "MainScene";
	public readonly string menuScene = "MenuScene";
	public readonly string uiScene = "UiScene";

	public readonly string hubScene = "HubScene";
	public readonly string dungeonOneScene = "DungeonOneScene";
	public readonly string bossDungeonOneScene = "BossDungeonOneScene";

	public Scene currentlyLoadedUiScene;
	public Scene currentlyLoadedScene;

	public GameObject PlayerPrefab;
	public static PlayerController Localplayer { get; private set; }

	public GameObject CameraPrefab;
	public static Camera LocalPlayerCamera;

	public List<string> dungeonSceneNamesList = new List<string>();
	public List<string> bossSceneNamesList = new List<string>();

	public DungeonData currentDungeonData;

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
			LoadUiScene();

		if (SceneHandler.Instance == null) //if != null starting scene = different scene
			LoadMainMenu(); //starting scene = MainScene
	}

	private void Start()
	{
		SaveManager.Instance.LoadPlayerData();
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
	public void LoadUiScene()
	{
		StartCoroutine(LoadSceneAsync(uiScene, false));
	}
	public void LoadMainMenu()
	{
		SaveManager.Instance.GameData = new GameData();

		DestroyCurrentLocalPlayer();
		StartCoroutine(LoadSceneAsync(menuScene, false));
	} 
	public void LoadHubArea(bool isNewGame, GameDataReloadMode gameDataRestoreMode)
	{
		LoadingScreensManager.Instance.ShowGameLoadingScreen(LoadingScreensManager.LoadingScreenType.game);
		GameManager.isNewGame = isNewGame;
		Instance.gameDataReloadMode = gameDataRestoreMode;

		if (gameDataRestoreMode == GameDataReloadMode.reloadAllScenesAndData)
		{
			ReloadAllScenes();
			return;
		}

		if (MultiplayerManager.IsMultiplayer())
			LoadNewMultiplayerScene(hubScene, isNewGame);
		else
			StartCoroutine(LoadSceneAsync(hubScene, isNewGame));
	}
	public void LoadDungeonOne()
	{
		LoadingScreensManager.Instance.ShowGameLoadingScreen(LoadingScreensManager.LoadingScreenType.dungeon);

		if (MultiplayerManager.IsMultiplayer())
			LoadNewMultiplayerScene(dungeonOneScene, false);
		else
			StartCoroutine(LoadSceneAsync(dungeonOneScene, false));
	}
	public void LoadDungeonTwo()
	{
		LoadingScreensManager.Instance.ShowGameLoadingScreen(LoadingScreensManager.LoadingScreenType.dungeon);

		if (MultiplayerManager.IsMultiplayer())
			LoadNewMultiplayerScene(dungeonOneScene, false);
		else
			StartCoroutine(LoadSceneAsync(dungeonOneScene, false));
	}
	public void LoadRandomBossDungeon()
	{
		LoadingScreensManager.Instance.ShowGameLoadingScreen(LoadingScreensManager.LoadingScreenType.bossDungeon);
		int bossDungeonIndex = Utilities.GetRandomNumber(bossSceneNamesList.Count - 1);

		if (bossDungeonIndex == 0)
			LoadBossDungoenOne();
		else if (bossDungeonIndex == 1)
			LoadBossDungoenTwo();
	}
	private void LoadBossDungoenOne()
	{
		if (MultiplayerManager.IsMultiplayer())
			LoadNewMultiplayerScene(bossDungeonOneScene, false);
		else
			StartCoroutine(LoadSceneAsync(bossDungeonOneScene, false));
	}
	private void LoadBossDungoenTwo()
	{
		if (MultiplayerManager.IsMultiplayer())
			LoadNewMultiplayerScene(bossDungeonOneScene, false);
		else
			StartCoroutine(LoadSceneAsync(bossDungeonOneScene, false));
	}

	//SCENE LOADING
	public void ClearDuplicateScenesForMultiplayer()
	{
		StartCoroutine(TryUnLoadSceneAsync(uiScene));
		StartCoroutine(TryUnLoadSceneAsync(currentlyLoadedScene.name));
	}
	private void ReloadAllScenes() //when loading a save file whilst already in a game scene
	{
		StartCoroutine(TryUnLoadSceneAsync(currentlyLoadedUiScene.name));
		StartCoroutine(TryUnLoadSceneAsync(currentlyLoadedScene.name));

		if (Localplayer != null)
			Destroy(Localplayer.gameObject);

		StartCoroutine(LoadSceneAsync(uiScene, false));
		StartCoroutine(LoadSceneAsync(hubScene, false));
	}
	private IEnumerator LoadSceneAsync(string sceneToLoad, bool isNewGame)
	{
		Debug.LogError("loading scene name: " + sceneToLoad + " at: " + DateTime.Now.ToString());
		StartCoroutine(TryUnLoadSceneAsync(currentlyLoadedScene.name));

		GameManager.isNewGame = isNewGame;
		Instance.PauseGame(false);
		AsyncOperation asyncLoadScene = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

		while (!asyncLoadScene.isDone)
			yield return null;
	}
	private void LoadNewMultiplayerScene(string sceneToLoad, bool isNewGame)
	{
		StartCoroutine(TryUnLoadSceneAsync(currentlyLoadedScene.name));

		GameManager.isNewGame = isNewGame;
		NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
	}

	//SCENE LOAD EVENT
	private void OnLoadSceneFinish(Scene newLoadedScene, LoadSceneMode mode)
	{
		Debug.LogError("loaded scene name: " + newLoadedScene.name + " at: " + DateTime.Now.ToString());

		UpdateActiveSceneToMainScene(newLoadedScene);
		UpdateCurrentlyLoadedScene(newLoadedScene);

		if (LocalPlayerCamera == null)
			SpawnPlayerCameraPrefab();

		if (!MultiplayerManager.IsMultiplayer())
		{
			if (SceneIsHubOrDungeonScene(newLoadedScene.name) && Localplayer == null)
				SpawnPlayerPrefab();
		}

		if (SceneIsHubOrDungeonScene(newLoadedScene.name))
		{
			if (gameDataReloadMode == GameDataReloadMode.reloadAllScenesAndData)
				SaveManager.Instance.ReloadDungeonDataEvent();
			else if (gameDataReloadMode == GameDataReloadMode.reloadDungeonData)
				SaveManager.Instance.ReloadDungeonDataEvent();

			Instance.gameDataReloadMode = GameDataReloadMode.noReload;
		}
		LoadingScreensManager.Instance.HideGameLoadingScreen();
	}

	//SCENE UNLOADING
	public void UnloadSceneForConnectedClients()
	{
		StartCoroutine(TryUnLoadSceneAsync(currentlyLoadedScene.name));
	}
	private IEnumerator TryUnLoadSceneAsync(string sceneToUnLoad)
	{
		if (sceneToUnLoad.IsNullOrEmpty()) yield return null;

		if (SceneIsHubOrDungeonScene(sceneToUnLoad))
			SaveManager.Instance.AutoSaveData();

		if (!sceneToUnLoad.IsNullOrEmpty()) //without this check again, throws scene to unload is invalid exception
		{
			AsyncOperation asyncUnLoadScene = SceneManager.UnloadSceneAsync(sceneToUnLoad);

			while (!asyncUnLoadScene.isDone)
				yield return null;
		}
	}

	//SCENE UNLOAD EVENT
	private void OnUnloadSceneFinish(Scene unLoadedScene)
	{
		Debug.LogError("unloaded scene: " + unLoadedScene.name + " at: " + DateTime.Now.ToString());
	}

	//SCENE CHECKS/UPDATES
	private void UpdateActiveSceneToMainScene(Scene newLoadedScene)
	{
		Scene currentActiveScene = SceneManager.GetActiveScene();
		if (currentActiveScene.name == mainScene) return;
		SceneManager.SetActiveScene(newLoadedScene);
	}
	private void UpdateCurrentlyLoadedScene(Scene newLoadedScene)
	{
		if (newLoadedScene.name == mainScene) return;    //scene always stays loaded
		else if (newLoadedScene.name == uiScene)
			currentlyLoadedUiScene = newLoadedScene;
		else
			currentlyLoadedScene = newLoadedScene;
	}
	public bool SceneIsHubOrDungeonScene(string newLoadedSceneName)
	{
		if (newLoadedSceneName.IsNullOrEmpty()) return false;
		if (newLoadedSceneName.Contains("Dungeon") || newLoadedSceneName.Contains("Hub"))
			return true;
		else return false;
	}

	//PLAYER CAMERA SPAWNING
	private void SpawnPlayerCameraPrefab()
	{
		GameObject cameraObj = Instantiate(CameraPrefab);
		LocalPlayerCamera = cameraObj.GetComponent<Camera>();
	}

	//PLAYER OBJECT SPAWNING
	private void SpawnPlayerPrefab()
	{
		if (FindObjectOfType<PlayerController>() != null)
		{
			Debug.LogWarning("player prefab already exists in scene, ignore if testing");
			return;
		}

		GameObject playerObj = Instantiate(PlayerPrefab);
		playerObj.transform.position = DungeonHandler.Instance.GetDungeonEnterencePortal(playerObj);
	}
	public void SpawnPlayerPrefab(ulong clientNetworkIdOfOwner)
	{
		GameObject playerObj = Instantiate(PlayerPrefab);
		playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientNetworkIdOfOwner, true);
		playerObj.transform.position = DungeonHandler.Instance.GetDungeonEnterencePortal(playerObj);
	}
	public void UpdateLocalPlayerInstanceAndReloadAllGameData(PlayerController newLocalPlayer)
	{
		if (!NewPlayerObjOwnedByClient(newLocalPlayer))
		{
			Debug.LogError("not owner of new local player");
			return;
		}

		DestroyOldLocalPlayer(newLocalPlayer);
		Localplayer = newLocalPlayer;
		SaveManager.Instance.ReloadSaveGameDataEvent(); //all game data reloaded once player instance set up in sp + for all clients in mp
		LoadingScreensManager.Instance.HideGameLoadingScreen();
	}

	private bool NewPlayerObjOwnedByClient(PlayerController newLocalPlayer)
	{
		if (!MultiplayerManager.IsMultiplayer())
			return true;
		else
		{
			if (newLocalPlayer.IsOwner)
				return true;
			else return false;
		}
	}
	private void DestroyOldLocalPlayer(PlayerController newLocalPlayer)
	{
		if (Localplayer != null && Localplayer != newLocalPlayer)
			Destroy(Localplayer.gameObject);
	}
	private void DestroyCurrentLocalPlayer()
	{
		if (Localplayer != null)
			Destroy(Localplayer.gameObject);
	}

	//pause game
	public void PauseGame(bool pauseGame)
    {
		if (MultiplayerManager.Instance != null)
		{
			if (pauseGame && !MultiplayerManager.IsMultiplayer())
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
