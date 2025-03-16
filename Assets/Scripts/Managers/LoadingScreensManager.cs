using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreensManager : MonoBehaviour
{
	public static LoadingScreensManager Instance;

	[Header("Game Loading Menu")]
	public GameObject GameLoadingScreenCanvas;
	public TMP_Text gameLoadingMessage;

	[Header("Lobby Loading Menu")]
	public GameObject LobbyLoadingScreenCanvas;
	public TMP_Text lobbyLoadingMessage;

	[Header("Disconnect Menu")]
	public GameObject DisconnectScreenCanvas;
	public TMP_Text disconnectMessage;
	public string disconnectReason;

	public enum LoadingScreenType
	{
		game, dungeon, bossDungeon, joiningLobby, creatingLobby,
	}

	private void Awake()
	{
		Instance = this;
	}

	public void ShowGameLoadingScreen(LoadingScreenType loadingScreenType)
	{
		if (loadingScreenType == LoadingScreenType.game)
			gameLoadingMessage.text = "Loading Game...";
		else if (loadingScreenType == LoadingScreenType.dungeon)
			gameLoadingMessage.text = "Loading Dungeon...";
		else if (loadingScreenType == LoadingScreenType.bossDungeon)
			gameLoadingMessage.text = "Loading Boss Dungeon...";

		GameLoadingScreenCanvas.SetActive(true);
	}
	public void HideGameLoadingScreen()
	{
		GameLoadingScreenCanvas.SetActive(false);
	}

	public void ShowLobbyLoadingScreen(LoadingScreenType loadingScreenType)
	{
		if (loadingScreenType == LoadingScreenType.joiningLobby)
			lobbyLoadingMessage.text = "Joining Game...";
		else if (loadingScreenType == LoadingScreenType.creatingLobby)
			lobbyLoadingMessage.text = "Creating Lobby...";

		LobbyLoadingScreenCanvas.SetActive(true);
	}
	public void HideLobbyLoadingScreen()
	{
		LobbyLoadingScreenCanvas.SetActive(false);
	}

	//DISCONNECT PANEL + Actions
	public void SetDisconnectReason(string reason)
	{
		disconnectReason = "DISCONNECTED\n" + reason;
	}
	public void ShowDisconnectScreen()
	{
		disconnectMessage.text = disconnectReason;
		DisconnectScreenCanvas.SetActive(true);
	}
	public void ConfirmDisconnectReason()
	{
		HideDisconnectScreen();
	}
	private void HideDisconnectScreen()
	{
		DisconnectScreenCanvas.SetActive(false);
		GameManager.Instance.LoadHubArea(true, GameManager.GameDataReloadMode.reloadAllScenesAndData);
	}
}
