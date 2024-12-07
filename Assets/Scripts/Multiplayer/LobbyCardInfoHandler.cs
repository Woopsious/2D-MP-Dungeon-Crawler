using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCardInfoHandler : MonoBehaviour
{
	public TMP_Text lobbyNameText;
	public TMP_Text lobbyHostNameText;
	public TMP_Text lobbyPlayerCountText;
	public Button button;

	//update ui if not marked as dirty
	public void UpdateInfo(Lobby lobby)
	{
		UpdateUi(lobby);
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(delegate {JoinLobbyButton(lobby);} );
	}

	//update ui text fields
	private void UpdateUi(Lobby lobby)
	{
		if (lobby == null)
		{
			gameObject.SetActive(false);
			return;
		}

		gameObject.SetActive(true);
		lobbyNameText.text = GetLobbyName(lobby);
		lobbyHostNameText.text = GetLobbyHostName(lobby);
		lobbyPlayerCountText.text = GetLobbyPlayerCount(lobby);
	}
	private string GetLobbyName(Lobby lobby)
	{
		return lobby.Name;
	}
	private string GetLobbyHostName(Lobby lobby)
	{
		if (lobby.Players[0].Data.TryGetValue("PlayerName", out PlayerDataObject playerName))
		{
			Debug.LogWarning("player Name Key found: " + playerName.Value);
		}
		else
		{
			Debug.LogError("player Name Key not found");
		}

		return playerName.Value.ToString();
	}
	private string GetLobbyPlayerCount(Lobby lobby)
	{
		return $"Players: {lobby.Players.Count - 1}/4";
	}
	private void JoinLobbyButton(Lobby lobby)
	{
		ClientManager.Instance.StartClientAndJoinLobby(lobby);
	}
}
