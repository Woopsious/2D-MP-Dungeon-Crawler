using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
		PlayerEventManager.OnShowPlayerDeathUiEvent += ShowPlayerDeathUi;
		PlayerEventManager.GetPlayerDeathMessaage += SetPlayerDeathMessage;
	}

	private void OnDisable()
	{
		PlayerEventManager.OnShowPlayerDeathUiEvent -= ShowPlayerDeathUi;
		PlayerEventManager.GetPlayerDeathMessaage -= SetPlayerDeathMessage;
	}

	private void ShowPlayerDeathUi()
	{
		//possiby get a reason for there death at some point. eg:
		//died while fighting x enemy / died from x trap / burned/poisoned to death in lava/posion pit etc...

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
		GameManager.Instance.LoadHubArea(false);

		HidePlayerDeathUi();
	}
	public void RespawnPlayerInDungeon()
	{
		//revive/reset player stats, send to closest portal
		if (BossRoomHandler.Instance != null)
		{
			BossRoomHandler.Instance.RespawnPlayerAtPortal(SceneHandler.playerInstance.gameObject);
			BossRoomHandler.Instance.ResetRoom();
		}
		else
		{
			DungeonHandler.Instance.RespawnPlayerAtClosestPortal(SceneHandler.playerInstance.gameObject);
		}

		SceneHandler.playerInstance.playerStats.ResetEntityStats();

		HidePlayerDeathUi();
	}

	//MP funcs for respawning player
}
