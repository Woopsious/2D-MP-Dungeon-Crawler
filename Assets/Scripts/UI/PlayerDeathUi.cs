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
	public TMP_Text PlayerDeathText;

	[Header("Respawn Buttons Ui")]
	public GameObject respawnInHubAreaButton;
	public GameObject respawnInDungeonButton;

	[Header("Spectate Panel Ui")]
	public GameObject PlayerSpectatePanelUi;
	public TMP_Text spectatingPlayerName;

	[Header("RespawnTimer Ui")]
	public GameObject PlayerRespawnTimerPanelUi;
	public TMP_Text respawnTimerText;
	private float respawnTimer;

	[Header("ReviveTimer Ui")]
	public GameObject PlayerReviveTimerPanelUi;
	public TMP_Text reviveTimerText;
	private float reviveTimer;

	public void Awake()
	{
		Instance = this;

		PlayerDeathPanelUi.SetActive(false);
		respawnInDungeonButton.SetActive(false);
		respawnInHubAreaButton.SetActive(false);
		PlayerSpectatePanelUi.SetActive(false);
		PlayerRespawnTimerPanelUi.SetActive(false);
	}

	private void OnEnable()
	{
		PlayerEventManager.OnPlayerDeathEvent += OnPlayerDeath;

		PlayerEventManager.OnStartReviveTimerUiEvent += ShowReviveTimerUi;
		PlayerEventManager.OnCancelReviveTimerUiEvent += HideReviveTimerUi;

		PlayerEventManager.OnRespawnPlayerEvent += HideAllUiPanelsOnRespawn;
	}

	private void OnDisable()
	{
		PlayerEventManager.OnPlayerDeathEvent -= OnPlayerDeath;

		PlayerEventManager.OnStartReviveTimerUiEvent -= ShowReviveTimerUi;
		PlayerEventManager.OnCancelReviveTimerUiEvent -= HideReviveTimerUi;

		PlayerEventManager.OnRespawnPlayerEvent -= HideAllUiPanelsOnRespawn;

	}

	private void Update()
	{
		RespawnTimer();
		ReviveTimer();
	}

	private void OnPlayerDeath(PlayerController player, string deathMessage)
	{
		if (MultiplayerManager.IsMultiplayer() && PlayerPartyWiped()) //update to wipe respawns for mp
			ShowDeathAndSpectatorUiPanels("Party Wiped");

		if (player != GameManager.Localplayer) return; //this player didnt die so ingnore ui

		ShowDeathAndSpectatorUiPanels(deathMessage);
		ShowRespawnTimerUi(player.GetRespawnTime());
	}
	public void CheckDeadPlayersOnClientDisconnect()
	{
		if (!PlayerPartyWiped()) return;
		ShowDeathAndSpectatorUiPanels("Last player alive left");
	}

	//ui updates
	private void ShowDeathAndSpectatorUiPanels(string deathMessage)
	{
		PlayerDeathText.text = deathMessage;
		PlayerDeathPanelUi.SetActive(true);

		if (MultiplayerManager.IsMultiplayer())
		{
			PlayerSpectatePanelUi.SetActive(true);
			PlayerRespawnTimerPanelUi.SetActive(true);
		}
		if (PlayerPartyWiped()) //show respawn screen on player party wipe (all players dead in sp/mp)
			ShowPlayerRespawnUi();
	}
	private void HideAllUiPanelsOnRespawn(PlayerController optionalReviverPlayer, PlayerController revivedPlayer)
	{
		if (revivedPlayer != GameManager.Localplayer) return;

		PlayerDeathPanelUi.SetActive(false);
		respawnInDungeonButton.SetActive(false);
		respawnInHubAreaButton.SetActive(false);
		PlayerSpectatePanelUi.SetActive(false);
		PlayerRespawnTimerPanelUi.SetActive(false);
	}

	//update spectate player text
	public void UpdateSpectatingPlayer(ulong idOfPlayer)
	{
		if (LobbyManager.Instance == null) return;
		spectatingPlayerName.text = "Spectating player " + LobbyManager.Instance.GetSpecificPlayerName(idOfPlayer);
	}

	//REVIVE TIMER UI
	private void ShowReviveTimerUi(float reviveTimer)
	{
		this.reviveTimer = reviveTimer;
		reviveTimerText.text = "Respawn in " + (int)reviveTimer;
		PlayerReviveTimerPanelUi.SetActive(true);
	}
	private void HideReviveTimerUi()
	{
		PlayerReviveTimerPanelUi.SetActive(false);
	}
	private void ReviveTimer()
	{
		if (reviveTimer > 0)
		{
			reviveTimer -= Time.deltaTime;
			reviveTimerText.text = "Revived in " + (int)reviveTimer;

			if (reviveTimer < 0)
				reviveTimerText.text = "Revived";
		}
	}

	//RESPAWN TIMER UI
	private void ShowRespawnTimerUi(float respawnTimer)
	{
		if (PlayerPartyWiped()) //skip respawn timer
			respawnTimer = 0.1f;

		this.respawnTimer = respawnTimer;
		respawnTimerText.text = "Respawn in " + (int)respawnTimer;

		if (BossRoomHandler.Instance != null)
		{
			if (BossRoomHandler.Instance.GetBossRoomState() == BossRoomHandler.BossRoomState.bossActive)
				respawnTimerText.text = "Respawn Disabled";
		}

		PlayerRespawnTimerPanelUi.SetActive(true);
	}
	private void RespawnTimer()
	{
		if (BossRoomHandler.Instance != null)
			if (BossRoomHandler.Instance.GetBossRoomState() == BossRoomHandler.BossRoomState.bossActive) return;

		if (respawnTimer > 0)
		{
			respawnTimer -= Time.deltaTime;
			respawnTimerText.text = "Respawn in " + (int)respawnTimer;

			if (respawnTimer < 0)
			{
				ShowPlayerRespawnUi();
				respawnTimerText.text = "Respawn Available";
			}
		}
	}

	//RESPAWN UI
	private void ShowPlayerRespawnUi()
	{
		respawnInDungeonButton.SetActive(false);
		respawnInHubAreaButton.SetActive(false);

		if (PlayerPartyWiped())						//respawns in dungeons on party wipes
		{
			if (BossRoomHandler.Instance == null)	//force host to respawn everyone in hub (regualar dungeon failed)
			{
				if (MultiplayerManager.IsClientHost())
					respawnInHubAreaButton.SetActive(true);
			}
			else									//allow all respawns in boss dungeons (quick boss kill retry)
			{
				respawnInDungeonButton.SetActive(true);

				if (MultiplayerManager.IsClientHost())
					respawnInHubAreaButton.SetActive(true);
			}

			respawnInDungeonButton.SetActive(true);

			if (MultiplayerManager.IsClientHost())
				respawnInHubAreaButton.SetActive(true);
		}
		else                                        //respawns in dungeons on non party wipes
		{
			if (BossRoomHandler.Instance != null)	//allow respawns only if boss isnt active (alive)
			{
				if (BossRoomHandler.Instance.GetBossRoomState() != BossRoomHandler.BossRoomState.bossActive)
					respawnInDungeonButton.SetActive(true);
			}
			else                                    //allow respawns in regualar dungeons
				respawnInDungeonButton.SetActive(true);
		}
	}

	//bool checks
	public bool PlayerPartyWiped()
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

	//BUTTON ACTIONS
	public void RespawnPlayersInHubArea()
	{
		GameManager.Instance.LoadHubArea(false, GameManager.GameDataReloadMode.noReload);

		if (MultiplayerManager.IsMultiplayer())
			ClientRpcManager.instance.RespawnAllPlayersRpc();
		else
			PlayerEventManager.RespawnAllPlayers();
	}
	public void RespawnPlayerInDungeon()
	{
		if (MultiplayerManager.IsMultiplayer())
			ClientRpcManager.instance.RespawnPlayerRpc(GameManager.Localplayer.NetworkObjectId, GameManager.Localplayer.NetworkObjectId);
		else
			PlayerEventManager.RespawnPlayer(GameManager.Localplayer, GameManager.Localplayer);
	}
}
