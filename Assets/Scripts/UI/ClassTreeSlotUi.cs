using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClassTreeSlotUi : MonoBehaviour
{
	public SOClassStatBonuses statBonus;
	public SOClassAbilities ability;

	public TMP_Text slotInfoText;
	public GameObject slotUnlockButtonObj;

	public int nodeLevelRequirment;
	public bool isAlreadyUnlocked;
	public int nodeIndex;

	[Tooltip("previous skill tree items needed to unlock this one")]
	public List<ClassTreeSlotUi> preRequisites = new List<ClassTreeSlotUi>();
	[Tooltip("skill tree items that will lock this one")]
	public List<ClassTreeSlotUi> exclusions = new List<ClassTreeSlotUi>();

	/// <summary>
	/// for saving data, 
	/// </summary>

	private void Start()
	{
		Initilize();
	}
	public void OnEnable()
	{
		ClassesUi.OnClassNodeUnlocks += CheckIfNodeShouldBeLockedOrUnlocked;
	}
	private void OnDisable()
	{
		ClassesUi.OnClassNodeUnlocks -= CheckIfNodeShouldBeLockedOrUnlocked;
	}

	private void Initilize()
	{
		if (statBonus != null)
			slotInfoText.text = statBonus.Name;
		else if (ability != null)
			slotInfoText.text = ability.Name;
		else
			Debug.LogError("Skill tree slot has no reference, this shouldnt happen");

		LockNode();
		isAlreadyUnlocked = false;
		nodeIndex = transform.GetSiblingIndex();
		if (nodeLevelRequirment == 1)
			UnlockNode();
	}

	public void UnlockThisClassSkillButton()
	{
		isAlreadyUnlocked = true;

		if (statBonus != null)
			ClassesUi.Instance.UnlockStatBonus(this, statBonus);
		else if (ability != null)
			ClassesUi.Instance.UnlockAbility(this, ability);
	}

	public void CheckIfNodeShouldBeLockedOrUnlocked(PlayerClassHandler playerClassHandler)
	{
		if (isAlreadyUnlocked)
		{
			LockNode();
			return;
		}

		if (playerClassHandler.entityStats.entityLevel <= nodeLevelRequirment)
		{
			LockNode();
			return;
		}

		if (!CheckIfAllPreRequisiteNodesUnlocked())
		{
			LockNode();
			return;
		}

		UnlockNode();
	}

	public bool CheckIfAllPreRequisiteNodesUnlocked()
	{
		int numOfPreRequisiteNodesUnlocked = 0;
		foreach (ClassTreeSlotUi node in preRequisites)
		{
			if (ClassesUi.Instance.unlockedKnightClassNodes.Contains(node))
				numOfPreRequisiteNodesUnlocked++;
		}

		if (numOfPreRequisiteNodesUnlocked == preRequisites.Count)
			return true;
		else
			return false;
	}

	public void LockNode()
	{
		slotUnlockButtonObj.SetActive(false);
	}
	public void UnlockNode()
	{
		slotUnlockButtonObj.SetActive(true);
	}
}