using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardInfoHandler : MonoBehaviour
{
	public bool uiDirty;

	public TMP_Text hostText;
	public TMP_Text playerNameText;
	public TMP_Text PlayerInfoText;
	public Button button;

	/// <summary>
	/// will also need to mark ui as dirty when a player changes there class or level (left out for now)
	/// </summary>

	//update ui if not marked as dirty
	public void UpdateInfo(Lobby lobby, int index)
	{
		if (!uiDirty) return;
		UpdateUi(lobby, index);
		UpdateKickPlayerButton();
		uiDirty = false;
	}

	//update ui text fields
	private void UpdateUi(Lobby lobby, int index)
	{
		if (lobby.Players.Count - 1 < index) //blank info if no player exists
		{
			hostText.gameObject.SetActive(false);
			playerNameText.text = "Empty";
			PlayerInfoText.text = "";
			return;
		}

		if (MultiplayerManager.Instance.IsPlayerHost() && index == 0)
			hostText.gameObject.SetActive(true);
		else
			hostText.gameObject.SetActive(false);

		playerNameText.text = GetPlayerName(lobby, index);
		PlayerInfoText.text = $"Level {GetPlayerLevel(lobby, index)} {GetPlayerClass(lobby, index)}";
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
	private void UpdateKickPlayerButton()
	{
		if (MultiplayerManager.Instance.IsPlayerHost())
		{
			button.gameObject.SetActive(true);
			button.onClick.AddListener(delegate { KickPlayer(); }) ;
		}
		else
		{
			button.gameObject.SetActive(false);
			button.onClick.RemoveAllListeners();
		}
	}

	private void KickPlayer()
	{
		//kick player shit
	}
}
