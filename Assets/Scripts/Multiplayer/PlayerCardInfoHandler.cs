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
	public bool uiDirty;

	public TMP_Text hostText;
	public TMP_Text playerNameText;
	public TMP_Text PlayerInfoText;

	public ulong clientNetworkId;
	public Button button;

	//update ui text fields
	public void UpdateUiInfo(Lobby lobby, int index)
	{
		if (lobby.Players.Count - 1 < index || HostManager.Instance.connectedClientsList.Count - 1 < index) //blank info if no player exists
			ClearUiInfo();
		else
		{
			clientNetworkId = HostManager.Instance.connectedClientsList[index].clientNetworkedId;
			SetHostText(index);
			playerNameText.text = GetPlayerName(lobby, index);
			PlayerInfoText.text = $"Level {GetPlayerLevel(lobby, index)} {GetPlayerClass(lobby, index)}";
			UpdatePlayerActionButton(index);
		}
	}
	public void ClearUiInfo()
	{
		hostText.text = "";
		playerNameText.text = "Empty";
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
	private string GetPlayerName(Lobby lobby, int index)
	{
		if (!lobby.Players[index].Data.TryGetValue("PlayerName", out PlayerDataObject playerName))
			Debug.LogError("player Name Key not found");

		return playerName.Value.ToString();
	}
	private string GetPlayerLevel(Lobby lobby, int index)
	{
		if (!lobby.Players[index].Data.TryGetValue("PlayerLevel", out PlayerDataObject playerLevel))
			Debug.LogError("player level Key not found");

		return playerLevel.Value.ToString();
	}
	private string GetPlayerClass(Lobby lobby, int index)
	{
		if (!lobby.Players[index].Data.TryGetValue("PlayerClass", out PlayerDataObject playerClass))
			Debug.LogError("player Class Key not found");

		return playerClass.Value.ToString();
	}

	/// <summary>
	/// will need additional checks to stop host from kicking players in certian situations (left out for now)
	/// </summary>

	//show kick player button for host
	private void UpdatePlayerActionButton(int index)
	{
		button.gameObject.SetActive(true);
		button.onClick.RemoveAllListeners();

		if (MultiplayerManager.Instance.IsPlayerHost())
		{
			if (index == 0)
			{
				button.GetComponentInChildren<TMP_Text>().text = "Close Lobby";
				button.onClick.AddListener(delegate { CloseLobbyButton(); });
			}
			else
			{
				button.GetComponentInChildren<TMP_Text>().text = "Kick Player";
				button.onClick.AddListener(delegate { KickPlayerButton(); });
			}
		}
		else
		{
			if (clientNetworkId == NetworkManager.Singleton.LocalClientId)
			{
				button.GetComponentInChildren<TMP_Text>().text = "Leave Lobby";
				button.onClick.AddListener(delegate { LeaveRelayAndLobbyButton(); });
			}
			else
			{
				button.gameObject.SetActive(false);
			}
		}
	}
	private void CloseLobbyButton()
	{
		if (!MultiplayerManager.Instance.IsPlayerHost()) return; //double check
		HostManager.Instance.CloseLobby("Host Closed Lobby");
	}
	private void KickPlayerButton()
	{
		if (!MultiplayerManager.Instance.IsPlayerHost()) return; //double check
		HostManager.Instance.RemoveClientFromRelay(clientNetworkId, "Kicked from lobby by host");
	}
	private void LeaveRelayAndLobbyButton()
	{
		HostManager.Instance.LeaveRelayServerRPC(clientNetworkId);
	}
}
