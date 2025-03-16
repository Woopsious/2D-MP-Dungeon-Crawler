using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerPartyUi : MonoBehaviour
{
	public static PlayerPartyUi Instance;

	public GameObject playerPartyPanelUi;
	public List<PlayerPartyHandlerUi> playerPartyHandlerUiList = new List<PlayerPartyHandlerUi>();

	public GameObject partyMessagesPanelUi;
	public GameObject partyMessagesContent;
	public GameObject partyMessagePrefab;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		if (MultiplayerManager.IsMultiplayer())
		{
			Instance.playerPartyPanelUi.SetActive(true);
			Instance.partyMessagesPanelUi.SetActive(true);
		}
		else
		{
			Instance.playerPartyPanelUi.SetActive(false);
			Instance.partyMessagesPanelUi.SetActive(false);
		}
	}

	public void SyncPlayerListforPartyUi(Lobby lobby)
	{
		int index = 0;
		foreach (PlayerPartyHandlerUi player in playerPartyHandlerUiList)
		{
			player.UpdatePlayersInParty(lobby, index);
			index++;
		}
	}

	public void SendPlayerJoinedMessage(string playerName)
	{
		if (LobbyManager.Instance._Lobby == null) return;

		GameObject go = Instantiate(partyMessagePrefab, partyMessagesContent.transform);
		PartyMessagesUi partyMessage = go.GetComponent<PartyMessagesUi>();

		string newMessage = "Player" + playerName + " joined the party";
		partyMessage.SetMessage(newMessage, 10);
	}
	public void SendPlayerLeftMessage(string playerName)
	{
		if (LobbyManager.Instance._Lobby == null) return;

		GameObject go = Instantiate(partyMessagePrefab, partyMessagesContent.transform);
		PartyMessagesUi partyMessage = go.GetComponent<PartyMessagesUi>();

		string newMessage = "Player" + playerName + " left the party";
		partyMessage.SetMessage(newMessage, 10);
	}
}
