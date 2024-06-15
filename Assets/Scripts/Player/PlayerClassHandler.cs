using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClassHandler : EntityClassHandler
{
	private void OnEnable()
	{
		PlayerClassesUi.OnClassChange += OnClassChanges;
		PlayerClassesUi.OnClassReset += OnClassReset;
		PlayerClassesUi.OnNewStatBonusUnlock += UnlockStatBoost;
		PlayerClassesUi.OnNewAbilityUnlock += UnlockAbility;
	}
	private void OnDisable()
	{
		PlayerClassesUi.OnClassChange -= OnClassChanges;
		PlayerClassesUi.OnClassReset -= OnClassReset;
		PlayerClassesUi.OnNewStatBonusUnlock -= UnlockStatBoost;
		PlayerClassesUi.OnNewAbilityUnlock -= UnlockAbility;
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
		if (PlayerClassesUi.Instance == null)
			Debug.LogError("ClassesUi component instance not set, ignore if intentional");
		else
			PlayerClassesUi.Instance.UpdateNodesInClassTree(GetComponent<EntityStats>());
	}
}
