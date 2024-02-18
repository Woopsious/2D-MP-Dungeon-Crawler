using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClassHandler : EntityClassHandler
{
	private void Start()
	{
		Initilize();
	}

	private void OnEnable()
	{
		SaveManager.OnGameLoad += ReloadPlayerClass;
		ClassesUi.OnClassChange += OnClassChanges;
		ClassesUi.OnClassReset += OnClassReset;
		ClassesUi.OnNewStatBonusUnlock += UnlockStatBoost;
		ClassesUi.OnNewAbilityUnlock += UnlockAbility;
		Debug.Log("player class handler subbing to events");
	}
	private void OnDisable()
	{
		SaveManager.OnGameLoad -= ReloadPlayerClass;
		ClassesUi.OnClassChange -= OnClassChanges;
		ClassesUi.OnClassReset -= OnClassReset;
		ClassesUi.OnNewStatBonusUnlock -= UnlockStatBoost;
		ClassesUi.OnNewAbilityUnlock -= UnlockAbility;
	}

	private void ReloadPlayerClass()
	{
		currentEntityClass = SaveManager.Instance.GameData.currentPlayerClass;

		foreach (SOClassStatBonuses statBonus in SaveManager.Instance.GameData.currentUnlockedStatBonuses)
			UnlockStatBoost(statBonus);

		foreach (SOClassAbilities ability in SaveManager.Instance.GameData.currentUnlockedAbilities)
			UnlockAbility(ability);
	}

	public void UpdatePlayerClassUi()
	{

	}
}
