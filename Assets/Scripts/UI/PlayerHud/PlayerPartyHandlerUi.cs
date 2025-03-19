using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPartyHandlerUi : MonoBehaviour
{
	public ulong playerClientNetworkId;
	public PlayerController playerClient;

	public TMP_Text playerName;
	public TMP_Text playerInfo;

	[Header("Bar Fillers")]
	public Image healthBarFiller;
	public Image manaBarFiller;

	public void UpdatePlayersInParty(Lobby lobby, int index)
	{
		if (lobby.Players.Count - 1 < index) //blank info if no player exists
			ClearUiInfo();
		else
		{
			LinkToClientPlayerObject(index);

			if (playerClientNetworkId == NetworkManager.Singleton.LocalClientId)
				playerName.text = "(YOU)" + LobbyManager.Instance.GetPlayerName(index);
			else
				playerName.text = LobbyManager.Instance.GetPlayerName(index);

			playerInfo.text = $"Level {LobbyManager.Instance.GetPlayerLevel(index)} {LobbyManager.Instance.GetPlayerClass(index)}";
			gameObject.SetActive(true);
		}
	}
	private void LinkToClientPlayerObject(int index)
	{
		playerClientNetworkId = LobbyManager.Instance.GetPlayerNetworkId(index);

		foreach (PlayerController player in ObjectPoolingManager.Instance.playersPool)
		{
			ulong ownerId = player.GetComponent<NetworkObject>().OwnerClientId;

			if (ownerId != playerClientNetworkId) continue;

			if (playerClient != null)
			{
				playerClient.playerStats.OnHealthChangeEvent -= UpdatePlayerHealthBar;
				playerClient.playerStats.OnManaChangeEvent -= UpdatePlayerManaBar;
			}

			playerClient = player;
			player.playerStats.OnHealthChangeEvent += UpdatePlayerHealthBar;
			player.playerStats.OnManaChangeEvent += UpdatePlayerManaBar;

			UpdatePlayerHealthBar(player.playerStats.maxHealth.finalValue, player.playerStats.currentHealth);
			UpdatePlayerManaBar(player.playerStats.maxMana.finalValue, player.playerStats.currentMana);
		}
	}
	public void ClearUiInfo()
	{
		gameObject.SetActive(false);
		playerName.text = "";
		playerInfo.text = "";
		healthBarFiller.fillAmount = 100;
		manaBarFiller.fillAmount = 100;

		if (playerClient != null)
		{
			playerClient.playerStats.OnHealthChangeEvent -= UpdatePlayerHealthBar;
			playerClient.playerStats.OnManaChangeEvent -= UpdatePlayerManaBar;
			playerClient = null;
		}
	}

	//events
	private void UpdatePlayerHealthBar(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		healthBarFiller.fillAmount = percentage;
	}
	private void UpdatePlayerManaBar(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		manaBarFiller.fillAmount = percentage;
	}
}
