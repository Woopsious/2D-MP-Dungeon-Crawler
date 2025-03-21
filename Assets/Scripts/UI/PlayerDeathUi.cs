using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathUi : MonoBehaviour
{
	public static PlayerDeathUi Instance;

	[Header("Death Panel Ui")]
	public GameObject PlayerDeathPanelUi;

	[Header("Text")]
	public TMP_Text PlayerDeathText;

	[Header("Buttons")]
	public GameObject respawnInHubAreaButton;
	public GameObject respawnInDungeonButton;

	[Header("Spectate Panel Ui")]
	public GameObject PlayerSpectatePanelUi;
	public TMP_Text spectatingPlayerName;

	public void Awake()
	{
		Instance = this;

		PlayerDeathPanelUi.SetActive(false);
		respawnInDungeonButton.SetActive(false);
		respawnInHubAreaButton.SetActive(false);
		PlayerSpectatePanelUi.SetActive(false);
	}

	private void OnEnable()
	{
		PlayerEventManager.OnPlayerDeathEvent += OnPlayerDeath;
		PlayerEventManager.OnRevivePlayerEvent += HideDeathAndSpectatorPanelUi;
	}

	private void OnDisable()
	{
		PlayerEventManager.OnPlayerDeathEvent -= OnPlayerDeath;
		PlayerEventManager.OnRevivePlayerEvent -= HideDeathAndSpectatorPanelUi;

	}
	private void OnPlayerDeath(GameObject playerObj, PlayerEventManager.PlayerDeathType playerDeathType, string deathMessage)
	{
		if (PlayerPartyWiped()) //show respawn screen on player party wipe (all players dead in sp/mp)
			ShowPlayerRespawnUi();

		if (playerObj != GameManager.Localplayer.gameObject) return; //this player didnt die so ingnore ui
		ShowDeathAndSpectatorPanelUi(deathMessage);
	}

	public void CheckDeadPlayersOnClientDisconnect()
	{
		if (PlayerPartyWiped() && MultiplayerManager.IsClientHost())
		{
			ShowPlayerRespawnUi();
			ShowDeathAndSpectatorPanelUi("Last player alive left");
		}
	}

	//bool checks
	private bool PlayerPartyWiped()
	{
		if (MultiplayerManager.IsClientHost()) //show respawn screen only for host/sp to respawn
		{
			int playersDeadCount = 0;
			foreach (PlayerController player in ObjectPoolingManager.Instance.playersPool)
			{
				if (player.playerStats.IsEntityDead())
					playersDeadCount++;
			}

			if (playersDeadCount == ObjectPoolingManager.Instance.playersPool.Count)
				return true;
			else return false;
		}
		else return false;
	}

	//ui updates
	private void ShowPlayerRespawnUi()
	{
		respawnInDungeonButton.SetActive(false);
		respawnInHubAreaButton.SetActive(false);

		respawnInHubAreaButton.SetActive(true);

		if (BossRoomHandler.Instance != null)
		{
			respawnInDungeonButton.SetActive(true);
		}
	}
	private void ShowDeathAndSpectatorPanelUi(string deathMessage)
	{
		PlayerDeathText.text = deathMessage;
		PlayerDeathPanelUi.SetActive(true);

		if (!MultiplayerManager.IsMultiplayer()) return; //no one to spectate in sp
		PlayerSpectatePanelUi.SetActive(true);
	}
	private void HideDeathAndSpectatorPanelUi(GameObject playerObj)
	{
		if (playerObj != GameManager.Localplayer.gameObject) return;

		PlayerDeathPanelUi.SetActive(false);
		respawnInDungeonButton.SetActive(false);
		respawnInHubAreaButton.SetActive(false);
		PlayerSpectatePanelUi.SetActive(false);
	}

	//update spectate player text
	public void UpdateSpectatingPlayer(ulong idOfPlayer)
	{
		if (LobbyManager.Instance == null) return;
		spectatingPlayerName.text = "Spectating player " + LobbyManager.Instance.GetSpecificPlayerName(idOfPlayer);
	}

	//BUTTON ACTIONS
	//sp/mp host client actions
	public void RespawnPlayersInHubArea()
	{
		GameManager.Instance.LoadHubArea(false, GameManager.GameDataReloadMode.noReload);

		if (MultiplayerManager.IsMultiplayer())
			ClientRpcManager.instance.ReviveAllPlayersRpc();
		else
			CallReviveAllPlayersEvent();
	}
	public void RespawnPlayersInDungeon()
	{
		if (MultiplayerManager.IsMultiplayer())
			ClientRpcManager.instance.ReviveAllPlayersRpc();
		else
			CallReviveAllPlayersEvent();
	}

	//revive players events (own func so mp works)
	public void CallReviveAllPlayersEvent()
	{
		PlayerEventManager.ReviveAllPlayers();
		PlayerEventManager.RevivePlayer(GameManager.Localplayer.gameObject);
	}
}
