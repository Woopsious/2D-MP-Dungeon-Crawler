using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyUi : MonoBehaviour
{
	public static LobbyUi Instance;

	[Header("Lobby")] //this needs to hide all other ui like player inv 
	public GameObject LobbySettingsUiPanel;
	public GameObject LobbyUiPanel;

	[Header("Lobby Settings")]
	public TMP_InputField lobbyNameInput;
	public TMP_Text lobbyNamePlaceholder;

	public TMP_Text lobbyPrivateButtonText;
	public bool lobbyPrivate;

	public TMP_Text lobbyPasswordButtonText;
	public bool lobbyHasPassword;

	public TMP_InputField lobbyPasswordInput;
	public TMP_Text lobbyPasswordPlaceholder;

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

	//create lobby button
	public void CreateLobby()
	{
		if (LobbyNameValid() && LobbyPasswordValid())
		{
			if (!lobbyHasPassword)
				LobbyManager.Instance.CreateLobby(lobbyNameInput.text, lobbyPrivate);
			else
				LobbyManager.Instance.CreateLobbyWithPassword(lobbyNameInput.text, lobbyPrivate, lobbyPasswordInput.text);
		}
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

	//back to lobby ui from lobby setting sui
	public void BackToLobbyUi()
	{
		
	}

	//LOBBY PANEL
	//Set up Player list
	public void SyncPlayerListforLobbyUi(Lobby lobby)
	{
		/*
		if (HostManager.Instance.connectedClientsList.Count < LobbyScreenParentTransform.childCount)
		{
			Transform childTransform = LobbyScreenParentTransform.GetChild(LobbyScreenParentTransform.childCount - 1);
			Destroy(childTransform.gameObject);
		}
		else if (HostManager.Instance.connectedClientsList.Count > LobbyScreenParentTransform.childCount)
		{
			Instantiate(PlayerItemPrefab, LobbyScreenParentTransform);
			UpdatePlayerList(lobby);
		}
		else
			UpdatePlayerList(lobby);
		*/
	}
	public void UpdatePlayerList(Lobby lobby)
	{
		/*
		int index = 0;
		foreach (Transform child in LobbyScreenParentTransform.transform)
		{
			PlayerItemManager playerItem = child.GetComponent<PlayerItemManager>();
			playerItem.Initialize(
				HostManager.Instance.connectedClientsList[index].clientName.ToString(),
				HostManager.Instance.connectedClientsList[index].clientId.ToString(),
				HostManager.Instance.connectedClientsList[index].clientNetworkedId.ToString()
				);

			if (!GameManager.Instance.isPlayerOne && playerItem.kickPlayerButton.activeInHierarchy)
				playerItem.kickPlayerButton.SetActive(false);

			else if (GameManager.Instance.isPlayerOne && !playerItem.kickPlayerButton.activeInHierarchy)
			{
				if (playerItem.localPlayerNetworkedId == "0")
					playerItem.kickPlayerButton.SetActive(false);

				else
					playerItem.kickPlayerButton.SetActive(true);
			}
			index++;
		}
		*/
	}
	public void ClearPlayersList()
	{
		/*
		foreach (Transform child in LobbyScreenParentTransform)
			Destroy(child.gameObject);
		*/
	}

	//UI PANEL CHANGES
	public void ShowLobbySettingsUi()
	{
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
		LobbyUiPanel.SetActive(true);
		MultiplayerMenuUi.Instance.HideMpMenuUi();
		LobbyListUi.Instance.HideLobbyListUi();
		HideLobbySettingsUi();
	}
	public void HideLobbyUi()
	{
		LobbyUiPanel.SetActive(false);
	}
}
