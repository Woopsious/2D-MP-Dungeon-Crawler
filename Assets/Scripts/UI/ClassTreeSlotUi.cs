using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClassTreeSlotUi : MonoBehaviour
{
	public SOClassStatBonuses statBonus;
	public SOClassAbilities ability;

	public int nodeLevelRequirment;

	public TMP_Text slotInfoText;
	public GameObject slotUnlockButtonObj;

	private void Start()
	{
		Initilize();
	}
	private void Initilize()
	{
		//LockSkillTreeNode();

		if (statBonus != null)
		{
			nodeLevelRequirment = statBonus.playerLevelRequirement;
			slotInfoText.text = statBonus.Name;
		}
		else if (ability != null)
		{
			nodeLevelRequirment = ability.playerLevelRequirement;
			slotInfoText.text = ability.Name;
		}
		else
			Debug.LogError("Skill tree slot has no reference, this shouldnt happen");
	}

	public void UnlockThisClassSkillButton()
	{
		Debug.Log("new class skill unlocked");

		if (statBonus != null)
			ClassesUi.Instance.UnlockStatBonus(statBonus);
		else if (ability != null)
			ClassesUi.Instance.UnlockAbility(ability);
	}

	public void CheckIfNodeShouldBeLockedOrUnlocked(PlayerClassHandler playerClassHandler)
	{
		if (playerClassHandler.entityStats.entityLevel <= nodeLevelRequirment) return;
	}

	public void LockSkillTreeNode()
	{
		slotUnlockButtonObj.SetActive(false);
	}
	public void UnlockSkillTreeNode()
	{
		slotUnlockButtonObj.SetActive(true);
	}
}