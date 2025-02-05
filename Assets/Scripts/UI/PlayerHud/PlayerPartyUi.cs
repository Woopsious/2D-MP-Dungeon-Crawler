using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerPartyUi : MonoBehaviour
{
	public static PlayerPartyUi Instance;

	public GameObject playerPartyPanelUi;

	public List<PlayerPartyHandlerUi> playerPartyHandlerUiList = new List<PlayerPartyHandlerUi>();

	private void Awake()
	{
		Instance = this;
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
}
