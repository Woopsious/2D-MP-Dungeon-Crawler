using System;
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
	}
	private void OnDisable()
	{
		SaveManager.OnGameLoad -= ReloadPlayerClass;
		ClassesUi.OnClassChange -= OnClassChanges;
		ClassesUi.OnClassReset -= OnClassReset;
		ClassesUi.OnNewStatBonusUnlock -= UnlockStatBoost;
		ClassesUi.OnNewAbilityUnlock -= UnlockAbility;
	}
	protected override void UnlockStatBoost(SOClassStatBonuses statBoost)
	{
		base.UnlockStatBoost(statBoost);
		UpdateClassTreeUi();
	}
	protected override void UnlockAbility(SOClassAbilities ability)
	{
		base.UnlockAbility(ability);
		UpdateClassTreeUi();
	}

	private void UpdateClassTreeUi()
	{
		if (ClassesUi.Instance == null)
			Debug.LogError("ClassesUi component instance not set, ignore if intentional");
		else
			ClassesUi.Instance.UpdateNodesInClassTree(this);
	}

	private void ReloadPlayerClass()
	{
		currentEntityClass = SaveManager.Instance.GameData.currentPlayerClass;

		foreach (SOClassStatBonuses statBonus in SaveManager.Instance.GameData.currentUnlockedStatBonuses)
			UnlockStatBoost(statBonus);

		foreach (SOClassAbilities ability in SaveManager.Instance.GameData.currentUnlockedAbilities)
			UnlockAbility(ability);
	}
}
