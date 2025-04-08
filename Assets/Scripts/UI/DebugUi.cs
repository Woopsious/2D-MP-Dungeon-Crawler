using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DebugUi : MonoBehaviour
{
	public GameObject DebugUiPanel;

	public TMP_InputField money;
	public TMP_InputField exp;

	public TMP_InputField bossPercentageDamage;

	public TMP_Text PlayerInvincibleText;
	public TMP_Text PlayerNoDeathText;

	public GameObject KillSelectedEnemyTargetButton;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote))
		{
			if (DebugUiPanel.activeInHierarchy)
				HideDebugUi();
			else
				ShowDebugUi();
		}
	}

	public void AddMoney()
	{
		int moneyToAdd = 0;
		try
		{
			moneyToAdd = int.Parse(money.text);
		}
		catch
		{
			Debug.LogError("only numbers allowed");
		}
		PlayerInventoryUi.Instance.UpdateGoldAmount(moneyToAdd);
	}
	public void AddExp()
	{
		int expToAdd = 0;
		try
		{
			expToAdd = int.Parse(exp.text);
		}
		catch
		{
			Debug.LogError("only numbers allowed");
		}

		if (expToAdd < 0 || expToAdd > 40)
			Debug.LogError("only numbers between 0 and 1000 valid");
		else
			GameManager.Localplayer.playerExperienceHandler.DebugAddExp(expToAdd);
	}
	public void DamageDungeonBoss()
	{
		if (BossRoomHandler.Instance == null)
		{
			Debug.LogError("not dungeon boss room");
			return;
		}
		else if (BossRoomHandler.Instance.GetBossRoomState() != BossRoomHandler.BossRoomState.bossActive)
		{
			Debug.LogError("dungeon boss not alive");
			return;
		}

		float percentageDamage = 0;
		try
		{
			percentageDamage = float.Parse(bossPercentageDamage.text);
		}
		catch
		{
			Debug.LogError("only numbers allowed");
		}

		if (percentageDamage < 0 || percentageDamage > 40)
			Debug.LogError("only numbers between 0 and 40 valid");
		else
		{
			percentageDamage /= 100;

			DamageSourceInfo damageSourceInfo = new DamageSourceInfo(
				GameManager.Localplayer.playerStats, IDamagable.HitBye.enviroment, percentageDamage, IDamagable.DamageType.isPhysicalDamage, true);

			BossEntityStats bossEntity = BossRoomHandler.Instance.roomSpawnHandler.GetBossEntity();

			if (bossEntity != null)
				bossEntity.RecieveDamage(damageSourceInfo, false);
			else
				Debug.LogError("dungeon boss ref null");
		}
	}
	public void KillSelectedTarget()
	{
		EntityStats selectedEntityTarget = PlayerSelectedTargetsUi.Instance.GetSelectedEnemyTarget();

		if (selectedEntityTarget == null)
		{
			Debug.LogError("enemy target not selected");
			return;
		}

		DamageSourceInfo damageSourceInfo;

		//if boss only damage to move to next phase/40%
		if (selectedEntityTarget.statsRef.isBossVersion)
			damageSourceInfo = new(selectedEntityTarget, IDamagable.HitBye.enviroment, 0.4f, IDamagable.DamageType.isPhysicalDamage, true);
		else
			damageSourceInfo = new(selectedEntityTarget, IDamagable.HitBye.enviroment, 10, IDamagable.DamageType.isPhysicalDamage, true);

		selectedEntityTarget.RecieveDamage(damageSourceInfo, false);
	}

	public void ToggleLocalPlayerInvincible()
	{
		Damageable player = GameManager.Localplayer.GetComponent<Damageable>();
		EntityStats playerStats = GameManager.Localplayer.GetComponent<EntityStats>();

		if (player.invincible)
		{
			player.invincible = false;
			PlayerInvincibleText.text = "Toggle Local Player\nInvincible : False";
		}
		else
		{
			player.invincible = true;
			PlayerInvincibleText.text = "Toggle Local Player\nInvincible : True";
		}

		if (MultiplayerManager.IsMultiplayer())
			ClientRpcManager.instance.SyncLocalPlayerInvincibleRpc(playerStats.NetworkObjectId, player.invincible);
	}
	public void ToggleLocalPlayerNoDeath()
	{
		EntityStats player = GameManager.Localplayer.GetComponent<EntityStats>();

		if (player.playerRef.debugNoDeath)
		{
			player.playerRef.debugNoDeath = false;
			PlayerNoDeathText.text = "Toggle Local Player\nNo Death :False";
		}
		else
		{
			player.playerRef.debugNoDeath = true;
			PlayerNoDeathText.text = "Toggle Local Player\nNo Death :True";
		}

		if (MultiplayerManager.IsMultiplayer())
			ClientRpcManager.instance.SyncLocalPlayerNoDeathRpc(player.NetworkObjectId, player.playerRef.debugNoDeath);
	}
	public void KillLocalPlayer()
	{
		DamageSourceInfo damageSourceInfo = new(
			GameManager.Localplayer.playerStats, IDamagable.HitBye.enviroment, 10, IDamagable.DamageType.isPhysicalDamage, true);

		damageSourceInfo.SetDebugDeathMessage();
		GameManager.Localplayer.playerStats.RecieveDamage(damageSourceInfo, false);
	}


	private void ShowDebugUi()
	{
		DebugUiPanel.SetActive(true);
		KillSelectedEnemyTargetButton.SetActive(true);
	}
	private void HideDebugUi()
	{
		DebugUiPanel.SetActive(false);
		KillSelectedEnemyTargetButton.SetActive(false);
	}
}
