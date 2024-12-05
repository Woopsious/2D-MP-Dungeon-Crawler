using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using TMPro;

public class MultiplayerMenuUi : MonoBehaviour
{
	public static MultiplayerMenuUi Instance;

	[Header("Mp Menu")]
	public GameObject MpMenuUiPanel;
	public TMP_InputField playerNameInput;
	public TMP_Text playerNamePlaceholder;

	private void Awake()
	{
		Instance = this;
	}

	//check if player name set
	private bool PlayerNameFilledIn()
	{
		if (playerNameInput.text == "")
		{
			playerNamePlaceholder.color = new Color(0.8f, 0, 0);
			playerNamePlaceholder.text = "NAME REQUIRED";
			return false;
		}
		else
		{
			ClientManager.Instance.clientUsername = playerNameInput.text;
			return true;
		}
    }

	//joining lobby
	public void JoinLobby()
	{
		if (!PlayerNameFilledIn()) return;

		LobbyListUi.Instance.ShowLobbyListUi();
	}

	//Hosting lobby
	public void HostLobby()
	{
		if (!PlayerNameFilledIn()) return;

		LobbyUi.Instance.ShowLobbySettingsUi();
	}

	//back button
	public void BackToMainMenu()
	{
		HideMpMenuUi();
		MainMenuManager.Instance.ShowMainMenu();
	}

	public void ShowMainMenu()
	{
		HideMpMenuUi();
		LobbyListUi.Instance.HideLobbyListUi();
		LobbyUi.Instance.HideLobbyUi();
		LobbyUi.Instance.HideLobbySettingsUi();
	}

	//UI PANEL CHANGES
	public void ShowMpMenuUi()
	{
		MpMenuUiPanel.SetActive(true);
		LobbyListUi.Instance.HideLobbyListUi();
		LobbyUi.Instance.HideLobbyUi();
		LobbyUi.Instance.HideLobbySettingsUi();
	}
	public void HideMpMenuUi()
	{
		MpMenuUiPanel.SetActive(false);
	}
}
