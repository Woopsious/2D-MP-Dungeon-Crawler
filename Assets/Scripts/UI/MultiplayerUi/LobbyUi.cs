using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class LobbyUi : MonoBehaviour
{
	public static LobbyUi Instance;

	[Header("Panels")] //this needs to hide all other ui like player inv 
	public GameObject LobbySettingsUiPanel;
	public GameObject LobbyUiPanel;

	[Header("Lobby Settings Ui")]
	public TMP_InputField lobbyNameInput;
	public TMP_Text lobbyNamePlaceholder;

	public TMP_Text lobbyPrivateButtonText;
	public bool lobbyPrivate;

	public TMP_Text lobbyPasswordButtonText;
	public bool lobbyHasPassword;

	public TMP_InputField lobbyPasswordInput;
	public TMP_Text lobbyPasswordPlaceholder;

	public Button createLobbyButton;
	public Button cancelLobbyCreationButton;

	[Header("Lobby Ui")]
	public List<PlayerCardInfoHandler> playerCardInfoList = new List<PlayerCardInfoHandler>();
	public Button LobbySettingsButton;

	[Header("Creating Lobby Panel")]
	public GameObject creatingLobbyPanel;

	[Header("Joining Lobby Panel")]
	public GameObject joiningLobbyPanel;

	private void Awake()
	{
		Instance = this;
	}

	/// <summary>
	///NEED EVENTS ON SCENE CHANGE TO UPDATE WHAT UI SETTINGS DISPALY SO THEY MATCH LOBBYMANAGER SETTINGS
	/// </summary>

	//LOBBY SETTINGS PANEL
	//lobby settings update button
	public void UpdateLobbyPrivateSetting()
	{
		if (lobbyPrivate)
		{
			lobbyPrivate = false;
			lobbyPrivateButtonText.text = "Lobby Private: False";
		}
		else
		{
			lobbyPrivate = true;
			lobbyPrivateButtonText.text = "Lobby Private: True";
		}
	}
	public void UpdateLobbyPasswordSetting()
	{
		if (lobbyHasPassword)
		{
			lobbyHasPassword = false;
			lobbyPasswordButtonText.text = "Lobby Has Password: False";
		}
		else
		{
			lobbyHasPassword = true;
			lobbyPasswordButtonText.text = "Lobby Has Password: True";
		}
	}

	//create/cancel lobby creation buttons
	public void CreateLobby()
	{
		if (LobbyNameValid() && LobbyPasswordValid())
		{
			HostManager.Instance.StartHost();

			if (!lobbyHasPassword)
				LobbyManager.Instance.CreateLobby(lobbyNameInput.text, lobbyPrivate);
			else
				LobbyManager.Instance.CreateLobbyWithPassword(lobbyNameInput.text, lobbyPrivate, lobbyPasswordInput.text);

			ShowCreatingLobbyUi();
		}
	}
	public void CancelLobbyCreation()
	{
		MultiplayerMenuUi.Instance.ShowMainMenu();
	}

	//lobby settings checks
	private bool LobbyNameValid()
	{
		if (lobbyNameInput.text == "")
		{
			lobbyNamePlaceholder.color = new Color(0.8f, 0, 0);
			lobbyNamePlaceholder.text = "REQUIRED";
			return false;
		}
		else
		{
			return true;
		}
	}
	private bool LobbyPasswordValid()
	{
		if (lobbyHasPassword && lobbyPasswordInput.text == "")
		{
			lobbyPasswordPlaceholder.color = new Color(0.8f, 0, 0);
			lobbyPasswordPlaceholder.text = "REQUIRED";
			return false;
		}
		else
		{
			return true;
		}
	}

	//LOBBY PANEL
	//Set up Player list
	public void SyncPlayerListforLobbyUi(Lobby lobby)
	{
		if (MultiplayerManager.IsClientHost())
			LobbySettingsButton.gameObject.SetActive(true);
		else
			LobbySettingsButton.gameObject.SetActive(false);

		int index = 0;
		foreach (PlayerCardInfoHandler playerCard in playerCardInfoList)
		{
			playerCard.UpdateUiInfo(lobby, index);
			index++;
		}
	}
	public void ClearPlayerListUi()
	{
		LobbySettingsButton.gameObject.SetActive(false);

		foreach (PlayerCardInfoHandler playerCard in playerCardInfoList)
			playerCard.ClearUiInfo();
	}

	//UI PANEL CHANGES
	//called when hosting game from MultiplayerManager
	public void ShowLobbySettingsUiWithCreateLobbyButton()
	{
		createLobbyButton.gameObject.SetActive(true);
		cancelLobbyCreationButton.gameObject.SetActive(true);

		LobbySettingsUiPanel.SetActive(true);
		MultiplayerMenuUi.Instance.HideMpMenuUi();
		LobbyListUi.Instance.HideLobbyListUi();
		HideLobbyUi();
	}
	public void ShowLobbySettingsUi()
	{
		createLobbyButton.gameObject.SetActive(false);
		cancelLobbyCreationButton.gameObject.SetActive(false);

		LobbySettingsUiPanel.SetActive(true);
		MultiplayerMenuUi.Instance.HideMpMenuUi();
		LobbyListUi.Instance.HideLobbyListUi();
		HideLobbyUi();
	}
	public void HideLobbySettingsUi()
	{
		LobbySettingsUiPanel.SetActive(false);
	}

	public void ShowLobbyUi()
	{
		if (creatingLobbyPanel.activeInHierarchy)
			HideCreatingLobbyUi();
		else if (joiningLobbyPanel.activeInHierarchy)
			HideJoiningLobbyUi();

		LobbyUiPanel.SetActive(true);
		MultiplayerMenuUi.Instance.HideMpMenuUi();
		LobbyListUi.Instance.HideLobbyListUi();
		HideLobbySettingsUi();
	}
	public void HideLobbyUi()
	{
		LobbyUiPanel.SetActive(false);
	}

	public void ShowCreatingLobbyUi()
	{
		creatingLobbyPanel.SetActive(true);
	}
	public void HideCreatingLobbyUi()
	{
		creatingLobbyPanel.SetActive(false);
	}

	public void ShowJoiningLobbyUi()
	{
		joiningLobbyPanel.SetActive(true);
	}
	public void HideJoiningLobbyUi()
	{
		joiningLobbyPanel.SetActive(false);
	}
}
