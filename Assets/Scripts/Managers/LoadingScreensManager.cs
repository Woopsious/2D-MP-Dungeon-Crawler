using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreensManager : MonoBehaviour
{
	public static LoadingScreensManager instance;

	public GameObject Canvas;
	public TMP_Text loadingMessage;

	public enum LoadingScreenType
	{
		game, dungeon, bossDungeon, joiningGame, creatingLobby, 
	}
	private void Awake()
	{
		instance = this;
	}

	public void ShowLoadingScreen(LoadingScreenType loadingScreenType)
	{
		if (loadingScreenType == LoadingScreenType.game)
			loadingMessage.text = "Loading Game...";
		else if (loadingScreenType == LoadingScreenType.dungeon)
			loadingMessage.text = "Loading Dungeon...";
		else if (loadingScreenType == LoadingScreenType.bossDungeon)
			loadingMessage.text = "Loading Boss Dungeon...";
		else if (loadingScreenType == LoadingScreenType.joiningGame)
			loadingMessage.text = "Joining Game...";
		else if (loadingScreenType == LoadingScreenType.creatingLobby)
			loadingMessage.text = "Creating Lobby...";

		Canvas.SetActive(true);
	}
	public void HideLoadingScreen()
	{
		Canvas.SetActive(false);
	}
}
