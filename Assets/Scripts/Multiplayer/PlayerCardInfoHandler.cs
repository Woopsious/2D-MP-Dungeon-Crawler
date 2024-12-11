using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
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

	/// <summary>
	/// will also need to mark ui as dirty when a player changes there class or level (left out for now)
	/// </summary>

	//update ui if not marked as dirty
	public void UpdateInfo(Lobby lobby, int index)
	{
		if (uiDirty)
		{
			Debug.LogError("ui dirty");
			this.clientNetworkId = HostManager.Instance.connectedClientsList[index].clientNetworkedId;
			UpdateUi(lobby, index);
			uiDirty = false;
		}
	}

	//update ui text fields
	private void UpdateUi(Lobby lobby, int index)
	{
		Debug.LogError("lobby player count: " + lobby.Players.Count);

		if (lobby.Players.Count - 1 < index) //blank info if no player exists
		{
			hostText.text = "";
			playerNameText.text = "Empty";
			PlayerInfoText.text = "";
			button.gameObject.SetActive(false);
		}
		else
		{
			SetHostText(index);
			playerNameText.text = GetPlayerName(lobby, index);
			PlayerInfoText.text = $"Level {GetPlayerLevel(lobby, index)} {GetPlayerClass(lobby, index)}";
			UpdatePlayerActionButton(lobby, index);
		}
	}
	private void SetHostText(int index)
	{
		if (MultiplayerManager.Instance.IsPlayerHost() && index == 0)
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
	private void UpdatePlayerActionButton(Lobby lobby, int index)
	{
		button.gameObject.SetActive(true);
		button.onClick.RemoveAllListeners();

		if (MultiplayerManager.Instance.IsPlayerHost())
		{
			button.GetComponentInChildren<TMP_Text>().text = "Kick Player";
			button.onClick.AddListener(delegate { KickPlayer(); }) ;
		}
		else
		{
			if (clientNetworkId == NetworkManager.Singleton.LocalClientId)
			{
				button.GetComponentInChildren<TMP_Text>().text = "Leave Lobby";
				button.onClick.AddListener(delegate { LeaveLobby(); });
			}
			else
			{
				button.gameObject.SetActive(false);
			}
		}
	}
	private void LeaveLobby()
	{
		HostManager.Instance.LeaveLobbyServerRPC(clientNetworkId, "Player Left");
	}

	private void KickPlayer()
	{
		if (!MultiplayerManager.Instance.IsPlayerHost()) return; //double check
		HostManager.Instance.RemoveClientFromRelay(clientNetworkId, "Kicked from lobby by host");
		//kick player shit
	}
}
