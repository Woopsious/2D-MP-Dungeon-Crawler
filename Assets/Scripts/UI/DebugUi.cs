using System;
using TMPro;
using UnityEngine;

public class DebugUi : MonoBehaviour
{
	public GameObject DebugUiPanel;

	public TMP_InputField money;
	public TMP_InputField exp;

	public TMP_Text PlayerInvincibleText;
	public TMP_Text PlayerNoDeathText;

	//event to notify other ui comps (eg: debug kill selected enemy button, debug complete quest)
	public static event Action<bool> UpdateShowDebubUiEvent;

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

	public void KillLocalPlayer()
	{
		DamageSourceInfo damageSourceInfo = new(
			GameManager.Localplayer.playerStats, IDamagable.HitBye.enviroment, 10, IDamagable.DamageType.isPhysicalDamage, true);

		damageSourceInfo.SetDebugDeathMessage();
		GameManager.Localplayer.playerStats.RecieveDamage(damageSourceInfo, false);
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

	private void ShowDebugUi()
	{
		DebugUiPanel.SetActive(true);
		UpdateShowDebubUiEvent?.Invoke(true);
	}
	private void HideDebugUi()
	{
		DebugUiPanel.SetActive(false);
		UpdateShowDebubUiEvent?.Invoke(false);
	}
}
