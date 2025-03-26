using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerPartyUi : MonoBehaviour
{
	public static PlayerPartyUi Instance;

	[Header("Player Party Ui")]
	public GameObject playerPartyPanelUi;
	public List<PlayerPartyHandlerUi> playerPartyHandlerUiList = new List<PlayerPartyHandlerUi>();

	[Header("Player Party Messages Ui")]
	public GameObject partyMessagesPanelUi;
	public GameObject partyMessagesContent;
	public List<PartyMessagesUi> partyMessagesList = new List<PartyMessagesUi>();
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

	private void OnEnable()
	{
		PlayerEventManager.OnPlayerDeathEvent += SendPlayerDiedMessage;
		PlayerEventManager.OnRespawnPlayerEvent += SendPlayerRespawnedMessage;
	}
	private void OnDisable()
	{
		PlayerEventManager.OnPlayerDeathEvent -= SendPlayerDiedMessage;
		PlayerEventManager.OnRespawnPlayerEvent -= SendPlayerRespawnedMessage;
	}

	//refresh players in party
	public void SyncPlayerListforPartyUi(Lobby lobby)
	{
		int index = 0;
		foreach (PlayerPartyHandlerUi player in playerPartyHandlerUiList)
		{
			player.UpdatePlayersInParty(lobby, index);
			index++;
		}
	}

	/// <summary>
	/// types of messages to send to party text box window
	/// </summary>

	//player deaths/respawns
	private void SendPlayerDiedMessage(PlayerController deadPlayer, string deathMessage)
	{
		if (LobbyManager.Instance == null || LobbyManager.Instance._Lobby == null) return;

		string newMessage = GetPlayerName(deadPlayer.OwnerClientId) + " Died";
		CreatePartyMessageUi().SetMessage(newMessage);
	}
	public void SendPlayerRespawnedMessage(PlayerController optionalReviverPlayer, PlayerController revivedplayer)
	{
		if (LobbyManager.Instance == null || LobbyManager.Instance._Lobby == null) return;

		string newMessage;
		if (optionalReviverPlayer != revivedplayer)
			newMessage = GetPlayerName(optionalReviverPlayer.OwnerClientId) + " revived " + GetPlayerName(revivedplayer.OwnerClientId);
		else
			newMessage = GetPlayerName(revivedplayer.OwnerClientId) + " respawned";

		CreatePartyMessageUi().SetMessage(newMessage);
	}

	//join/leave
	public void SendPlayerJoinedMessage(string playerName)
	{
		if (LobbyManager.Instance._Lobby == null) return;

		string newMessage = "Player" + playerName + " joined the party";
		CreatePartyMessageUi().SetMessage(newMessage, 10);
	}
	public void SendPlayerLeftMessage(string playerName)
	{
		if (LobbyManager.Instance._Lobby == null) return;

		string newMessage = "Player" + playerName + " left the party";
		CreatePartyMessageUi().SetMessage(newMessage, 10);
	}

	//instantiate ui message
	private PartyMessagesUi CreatePartyMessageUi()
	{
		GameObject go = Instantiate(partyMessagePrefab, partyMessagesContent.transform);
		PartyMessagesUi partyMessageUi = go.GetComponent<PartyMessagesUi>();
		partyMessagesList.Add(partyMessageUi);
		DeleteOldMessages();
		UpdatePartyMessagesScrollViewPort();
		return partyMessageUi;
	}
	private string GetPlayerName(ulong playerId)
	{
		return LobbyManager.Instance.GetSpecificPlayerName(playerId);
	}

	private void DeleteOldMessages()
	{
		if (partyMessagesList.Count < 21) return;
		Destroy(partyMessagesList[0].gameObject);
		partyMessagesList.RemoveAt(0);
	}
	private void UpdatePartyMessagesScrollViewPort()
	{
		if (partyMessagesList.Count < 6) return; //new messages not reach bottom, no need to auto scroll
		if (partyMessagesList.Count > 20) return; //max message limit reached, no need to auto scroll

		Vector3 localPos = new(270, partyMessagesContent.transform.position.y + 35, 0);
		partyMessagesContent.GetComponent<RectTransform>().SetLocalPositionAndRotation(localPos, Quaternion.identity);
	}
}
