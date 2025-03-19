using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PlayerEventManager;

public class PlayerDeathUi : MonoBehaviour
{
	public static PlayerDeathUi Instance;

	[Header("Panel Ui")]
	public GameObject PlayerDeathPanelUi;

	[Header("Text")]
	public TMP_Text PlayerDeathText;

	[Header("Buttons")]
	public GameObject respawnInHubAreaButton;
	public GameObject respawnInDungeonButton;

	public void Awake()
	{
		Instance = this;
		Initilize();
	}
	private void Initilize()
	{
		HidePlayerDeathUi();
	}

	private void OnEnable()
	{
		PlayerEventManager.OnPlayerDeathEvent += ShowPlayerDeathUi;
	}

	private void OnDisable()
	{
		PlayerEventManager.OnPlayerDeathEvent -= ShowPlayerDeathUi;
	}

	private void ShowPlayerDeathUi(GameObject playerObj, PlayerDeathType playerDeathType, string deathMessage)
	{
		if (playerDeathType == PlayerDeathType.dungeonMpDeath || playerDeathType == PlayerDeathType.bossDungeonMpDeath)
			if (playerObj != GameManager.Localplayer.gameObject) return; //this player didnt die so ingnore ui

		PlayerDeathText.text = deathMessage;
		PlayerDeathPanelUi.SetActive(true);

		if (Application.isEditor) //allow all respawning types whilst in editor
		{
			if (GameManager.Instance == null)
				Debug.LogWarning("Game Manager instance not found, some respawn types hidden, ignore if testing scene");
			else
				respawnInHubAreaButton.SetActive(true);

			respawnInDungeonButton.SetActive(true);
			return;
		}

		if (GameManager.Instance == null)
		{
			Debug.LogError("Game Manager instance not found");
			return;
		}

		respawnInHubAreaButton.SetActive(true);

		if (BossRoomHandler.Instance != null)
			respawnInDungeonButton.SetActive(true);
	}
	private void SetPlayerDeathMessage(string deathMessage)
	{
		PlayerDeathText.text = deathMessage;
	}

	private void HidePlayerDeathUi()
	{
		PlayerDeathPanelUi.SetActive(false);
		respawnInHubAreaButton.SetActive(false);
		respawnInDungeonButton.SetActive(false);
	}

	//button actions
	public void RespawnPlayerInHubArea()
	{
		GameManager.Instance.LoadHubArea(false, GameManager.GameDataReloadMode.noReload);

		HidePlayerDeathUi();
	}
	public void RespawnPlayerInDungeon()
	{
		//revive/reset player stats, send to closest portal
		if (BossRoomHandler.Instance != null)
		{
			BossRoomHandler.Instance.RespawnPlayerAtPortal(GameManager.Localplayer.gameObject);
			BossRoomHandler.Instance.ResetRoom();
		}
		else
		{
			DungeonHandler.Instance.RespawnPlayerAtClosestPortal(GameManager.Localplayer.gameObject);
		}

		GameManager.Localplayer.playerStats.ResetEntityStats();

		HidePlayerDeathUi();
	}

	//MP funcs for respawning player
}
