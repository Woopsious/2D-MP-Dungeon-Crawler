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

	[Header("Disconnect Menu")]
	public GameObject disconnectUiPanel;
	public TMP_Text disconnectReasonText;

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

	//hosting/joining lobby
	public void HostLobby()
	{
		if (!PlayerNameFilledIn()) return;

		AuthPlayer();
		LobbyUi.Instance.ShowLobbySettingsUiWithCreateLobbyButton();
	}
	public void JoinLobby()
	{
		if (!PlayerNameFilledIn()) return;

		AuthPlayer();
		LobbyListUi.Instance.ShowLobbyListUi();
	}
	private async void AuthPlayer()
	{
		await MultiplayerManager.Instance.AuthenticatePlayer();
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

	public void ConfirmDisconnectReason()
	{
		ShowMpMenuUi();
	}

	//UI PANEL CHANGES
	public void ShowMpMenuUi()
	{
		MpMenuUiPanel.SetActive(true);
		HideDisconnectUiPanel();
		LobbyListUi.Instance.HideLobbyListUi();
		LobbyUi.Instance.HideLobbyUi();
		LobbyUi.Instance.HideLobbySettingsUi();
	}
	public void HideMpMenuUi()
	{
		MpMenuUiPanel.SetActive(false);
	}

	public void SetDisconnectReason(string reason)
	{
		disconnectReasonText.text = "DISCONNECTED\n" + reason;
	}
	public void ShowDisconnectUiPanel()
	{
		disconnectUiPanel.SetActive(true);
		HideMpMenuUi();
		LobbyListUi.Instance.HideLobbyListUi();
		LobbyUi.Instance.HideLobbyUi();
		LobbyUi.Instance.HideLobbySettingsUi();
	}
	public void HideDisconnectUiPanel()
	{
		disconnectUiPanel.SetActive(false);
	}
}
