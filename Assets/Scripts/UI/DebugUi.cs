using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugUi : MonoBehaviour
{
	public GameObject DebugUiPanel;

	public TMP_InputField money;
	public TMP_InputField exp;

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

	public void KillLocalPlayer()
	{
		DamageSourceInfo damageSourceInfo = new DamageSourceInfo(
			GameManager.Localplayer.playerStats, IDamagable.HitBye.enviroment, 1000000, IDamagable.DamageType.isPhysicalDamage, false);

		damageSourceInfo.SetDebugDeathMessage();

		GameManager.Localplayer.playerStats.RecieveDamage(damageSourceInfo, false);
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

		if (expToAdd < 0 || expToAdd > 1000)
			Debug.LogError("only numbers between 0 and 1000 valid");
		else
			GameManager.Localplayer.playerExperienceHandler.DebugAddExp(expToAdd);
	}
	private void ShowDebugUi()
	{
		DebugUiPanel.SetActive(true);
	}
	private void HideDebugUi()
	{
		DebugUiPanel.SetActive(false);
	}
}
