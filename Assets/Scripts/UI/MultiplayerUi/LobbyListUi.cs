using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyListUi : MonoBehaviour
{
	public static LobbyListUi Instance;

	[Header("Lobby List")]
	public GameObject LobbyListUiPanel;

	private void Awake()
	{
		Instance = this;
	}

	//Set up lobby list
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
	public void ClearLobbiesList()
	{
		/*
		foreach (Transform child in LobbyListParentTransform)
			Destroy(child.gameObject);
		*/
	}

	//UI PANEL CHANGES
	public void ShowLobbyListUi()
	{
		LobbyListUiPanel.SetActive(true);
		MultiplayerMenuUi.Instance.HideMpMenuUi();
		LobbyUi.Instance.HideLobbyUi();
		LobbyUi.Instance.HideLobbySettingsUi();
	}
	public void HideLobbyListUi()
	{
		LobbyListUiPanel.SetActive(false);
	}
}
