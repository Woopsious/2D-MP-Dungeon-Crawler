using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

	public static event Action<bool> OnSceneChange;

	public readonly string mainMenuName = "MainMenu";
	public readonly string hubAreaName = "HubArea";

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

	public void Update()
	{
		//Debug.Log(Time.timeScale);
	}

	//saving and loading scenes
	public void LoadMainMenu(bool isNewGame)
	{
		StartCoroutine(LoadNewSceneAsync(mainMenuName, isNewGame));
	}
	public void LoadHubArea(bool isNewGame)
	{
		StartCoroutine(LoadNewSceneAsync(hubAreaName, isNewGame));
	}
	private IEnumerator LoadNewSceneAsync(string sceneToLoad, bool isNewGame)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

		while (!asyncLoad.isDone)
			yield return null;
		OnSceneChange?.Invoke(isNewGame);
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
