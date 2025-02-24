using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreensManager : MonoBehaviour
{
	public static LoadingScreensManager instance;

	public GameObject GameCanvas;
	public GameObject LobbyCanvas;
	public TMP_Text loadingMessage;

	public enum LoadingScreenType
	{
		game, dungeon, bossDungeon, joiningLobby, creatingLobby,
	}

	private void Awake()
	{
		instance = this;
	}

	public void ShowGameLoadingScreen(LoadingScreenType loadingScreenType)
	{
		if (loadingScreenType == LoadingScreenType.game)
			loadingMessage.text = "Loading Game...";
		else if (loadingScreenType == LoadingScreenType.dungeon)
			loadingMessage.text = "Loading Dungeon...";
		else if (loadingScreenType == LoadingScreenType.bossDungeon)
			loadingMessage.text = "Loading Boss Dungeon...";

		loadingMessage.transform.SetParent(GameCanvas.transform);
		GameCanvas.SetActive(true);
	}
	public void HideGameLoadingScreen()
	{
		GameCanvas.SetActive(false);
	}

	public void ShowLobbyLoadingScreen(LoadingScreenType loadingScreenType)
	{
		if (loadingScreenType == LoadingScreenType.joiningLobby)
			loadingMessage.text = "Joining Game...";
		else if (loadingScreenType == LoadingScreenType.creatingLobby)
			loadingMessage.text = "Creating Lobby...";

		loadingMessage.text = "Creating Lobby...";
		LobbyCanvas.SetActive(true);
	}
	public void HideLobbyLoadingScreen()
	{
		LobbyCanvas.SetActive(false);
	}
}
