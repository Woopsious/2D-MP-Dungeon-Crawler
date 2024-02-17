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
		PlayerClassesUi.OnClassChange += OnClassChanges;
		PlayerClassesUi.OnClassReset += OnClassReset;
	}
	private void OnDisable()
	{
		SaveManager.OnGameLoad -= ReloadPlayerClass;
		PlayerClassesUi.OnClassChange -= OnClassChanges;
		PlayerClassesUi.OnClassReset -= OnClassReset;
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
