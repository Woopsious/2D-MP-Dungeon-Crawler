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
			gameObject.SetActive(true);
			LinkToClientPlayerObject(lobby, index);
			playerName.text = GetPlayerName(lobby, index);
			playerInfo.text = $"Level {GetPlayerLevel(lobby, index)} {GetPlayerClass(lobby, index)}";
		}
	}
	private void LinkToClientPlayerObject(Lobby lobby, int index)
	{
		playerClientNetworkId = GetPlayerNetworkId(lobby, index);

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
	private ulong GetPlayerNetworkId(Lobby lobby, int index)
	{
		if (!lobby.Players[index].Data.TryGetValue("PlayerNetworkID", out PlayerDataObject playerNetworkId))
			Debug.LogError("player Name Key not found");

		return Convert.ToUInt64(playerNetworkId.Value);
	}
}
