using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardInfoHandler : MonoBehaviour
{
	public TMP_Text hostText;
	public TMP_Text playerNameText;
	public TMP_Text PlayerInfoText;

	public ulong clientNetworkId;
	public Button button;

	//update ui text fields
	public void UpdateUiInfo(Lobby lobby, int index)
	{
		if (lobby.Players.Count - 1 < index) //blank info if no player exists
			ClearUiInfo();
		else
		{
			SetHostText(index);
			playerNameText.text = LobbyManager.Instance.GetPlayerName(index);
			PlayerInfoText.text = $"Level {LobbyManager.Instance.GetPlayerLevel(index)} {LobbyManager.Instance.GetPlayerClass(index)}";
			clientNetworkId = LobbyManager.Instance.GetPlayerNetworkId(index);
			UpdatePlayerActionButton(index);
		}
	}
	public void ClearUiInfo()
	{
		hostText.text = "";
		playerNameText.text = "No Player";
		PlayerInfoText.text = "";
		button.gameObject.SetActive(false);
	}
	private void SetHostText(int index)
	{
		if (index == 0)
			hostText.text = "HOST\nPlayer 1";
		else if (index == 1)
			hostText.text = "Player 2";
		else if (index == 2)
			hostText.text = "Player 3";
		else if (index == 3)
			hostText.text = "Player 4";
	}

	public void LogPlayerInfo()
	{
		LobbyManager.Instance.LogSpecificPlayerInfo(clientNetworkId.ToString());
	}

	/// <summary>
	/// will need additional checks to stop host from kicking players in certian situations (left out for now)
	/// </summary>

	//show kick player button for host
	private void UpdatePlayerActionButton(int index)
	{
		button.gameObject.SetActive(true);
		button.onClick.RemoveAllListeners();

		if (MultiplayerManager.IsClientHost())
		{
			if (index == 0)
			{
				button.GetComponentInChildren<TMP_Text>().text = "Close Lobby";
				button.onClick.AddListener(delegate { HostCloseLobbyButton(); });
			}
			else
			{
				button.GetComponentInChildren<TMP_Text>().text = "Kick Player";
				button.onClick.AddListener(delegate { HostKickPlayerButton(); });
			}
		}
		else
		{
			if (clientNetworkId == NetworkManager.Singleton.LocalClientId)
			{
				button.GetComponentInChildren<TMP_Text>().text = "Leave Lobby";
				button.onClick.AddListener(delegate { ClientLeaveRelayAndLobbyButton(); });
			}
			else
			{
				button.gameObject.SetActive(false);
			}
		}
	}
	private void ClientLeaveRelayAndLobbyButton()
	{
		if (MultiplayerManager.IsClientHost()) return; //double check

		ClientManager.Instance.ClientLeaveRelayAndLobby("Lobby Left", false);
	}
	private void HostCloseLobbyButton()
	{
		if (!MultiplayerManager.IsClientHost()) return; //double check

		HostManager.Instance.CloseLobbyAndStopHost("Host Closed Lobby", false);
	}
	private void HostKickPlayerButton()
	{
		if (!MultiplayerManager.IsClientHost()) return; //double check

		HostManager.Instance.KickClientFromRelay(clientNetworkId.ToString(), "Kicked from lobby by host");
	}
}
