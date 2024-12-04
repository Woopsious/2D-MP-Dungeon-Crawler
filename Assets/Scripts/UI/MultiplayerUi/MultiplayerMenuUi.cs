using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class MultiplayerMenuUi : MonoBehaviour
{
	public static MultiplayerMenuUi Instance;

	[Header("Mp Menu")]
	public GameObject MpMenuUiPanel;

	[Header("Lobby List")]
	public GameObject LobbyListUiPanel;

	[Header("Lobby")] //this needs to hide all other ui like player inv 
	public GameObject LobbyUiPanel;
	public GameObject LobbySettingsUiPanel;

	private void Awake()
	{
		Instance = this;
	}

	//Set up lobby list and Player list
	public void SetUpLobbyListUi(QueryResponse queryResponse)
	{
		ClearLobbiesList();

		foreach (Lobby lobby in queryResponse.Results)
		{
			/*
			GameObject obj = Instantiate(LobbyItemPrefab, LobbyListParentTransform);
			obj.GetComponent<LobbyItemManager>().Initialize(lobby);
			*/
		}
	}
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
	public void ClearLobbiesList()
	{
		/*
		foreach (Transform child in LobbyListParentTransform)
			Destroy(child.gameObject);
		*/
	}
	public void ClearPlayersList()
	{
		/*
		foreach (Transform child in LobbyScreenParentTransform)
			Destroy(child.gameObject);
		*/
	}

	//joining lobby
	public void JoinLobby()
	{
		ShowLobbyListUi();
	}

	//Hosting lobby
	public void HostLobby()
	{
		ShowLobbySettingsUi();
	}

	//back button
	public void BackToMpMainMenu()
	{
		ShowMpMenuUi();
		//need to un auth + disconnect from any Mp features 
	}

	//UI CHANGES
	public void ShowMpMenuUi()
	{
		MpMenuUiPanel.SetActive(true);
		HideLobbyListUi();
		HideLobbyUi();
		HideLobbySettingsUi();
	}
	public void HideMpMenuUi()
	{
		MpMenuUiPanel.SetActive(false);
	}

	public void ShowLobbyListUi()
	{
		LobbyListUiPanel.SetActive(true);
		HideMpMenuUi();
		HideLobbyUi();
		HideLobbySettingsUi();
	}
	public void HideLobbyListUi()
	{
		LobbyListUiPanel.SetActive(false);
	}

	public void ShowLobbyUi()
	{
		LobbyUiPanel.SetActive(true);
		HideMpMenuUi();
		HideLobbyListUi();
		HideLobbySettingsUi();
	}
	public void HideLobbyUi()
	{
		LobbyUiPanel.SetActive(false);
	}

	public void ShowLobbySettingsUi()
	{
		LobbySettingsUiPanel.SetActive(true);
		HideMpMenuUi();
		HideLobbyListUi();
		HideLobbyUi();
	}
	public void HideLobbySettingsUi()
	{
		LobbySettingsUiPanel.SetActive(false);
	}
}
