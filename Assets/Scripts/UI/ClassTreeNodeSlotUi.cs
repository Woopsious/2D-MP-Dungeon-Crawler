using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ClassTreeNodeSlotUi : MonoBehaviour
{
	public SOClassStatBonuses statBonus;
	public SOClassAbilities ability;

	public TMP_Text nodeInfoText;
	private Image image;
	public GameObject nodeUnlockButtonObj;

	public int nodeLevelRequirment;
	public bool isAlreadyUnlocked;
	public int nodeIndex;

	[Tooltip("Prev skill nodes needed to unlock this one (only needs one of these)")]
	public List<ClassTreeNodeSlotUi> preRequisites = new List<ClassTreeNodeSlotUi>();
	[Tooltip("Nodes that are exclusive to this one")]
	public List<ClassTreeNodeSlotUi> exclusions = new List<ClassTreeNodeSlotUi>();

	private void Start()
	{
		Initilize();
	}
	private void OnEnable()
	{
		ClassesUi.OnClassNodeUnlocks += CheckIfNodeShouldBeLockedOrUnlocked;
		ClassesUi.OnClassReset += ResetNode;
	}
	private void OnDisable()
	{
		ClassesUi.OnClassNodeUnlocks -= CheckIfNodeShouldBeLockedOrUnlocked;
		ClassesUi.OnClassReset -= ResetNode;
	}
	private void Initilize()
	{
		if (statBonus != null)
			nodeInfoText.text = statBonus.Name;
		else if (ability != null)
			nodeInfoText.text = ability.Name;
		else
			Debug.LogError("Skill tree slot has no reference, this shouldnt happen");

		image = GetComponent<Image>();
		nodeIndex = transform.parent.GetSiblingIndex();
		DelegateButtonAction();
		ResetNode(null);
	}

	private void DelegateButtonAction()
	{
		nodeUnlockButtonObj.GetComponent<Button>().onClick.AddListener(UnlockThisNodeButton);
	}

	public void UnlockThisNodeButton()
	{
		isAlreadyUnlocked = true;

		if (statBonus != null)
			ClassesUi.Instance.UnlockStatBonus(this, statBonus);
		else if (ability != null)
			ClassesUi.Instance.UnlockAbility(this, ability);
	}

	//node update checks
	public void CheckIfNodeShouldBeLockedOrUnlocked(PlayerClassHandler playerClassHandler)
	{
		if (isAlreadyUnlocked)
		{
			LockNode();
			ActivateNode();
			return;
		}

		if (CheckForNodeExclusions())
		{
			LockNode();
			return;
		}

		if (playerClassHandler.entityStats.entityLevel < nodeLevelRequirment)
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
	public bool CheckForNodeExclusions()
	{
		bool containesExclusiveNode = false;
		foreach (ClassTreeNodeSlotUi node in exclusions)
		{
			if (ClassesUi.Instance.currentUnlockedClassNodes.Contains(node))
			{
				containesExclusiveNode = true;
				break;
			}
		}

		if (containesExclusiveNode)
			return true;
		else
			return false;
	}
	public bool CheckIfAllPreRequisiteNodesUnlocked()
	{
		if (preRequisites.Count == 1)
		{
			foreach (ClassTreeNodeSlotUi node in preRequisites)
			{
				if (ClassesUi.Instance.currentUnlockedClassNodes.Contains(node))
					return true;
			}
			return false;
		}
		else
		{
			foreach (ClassTreeNodeSlotUi node in preRequisites)
			{
				if (ClassesUi.Instance.currentUnlockedClassNodes.Contains(node))
					return true;
			}
			return false;
		}
	}

	//node updates
	private void LockNode()
	{
		image.color = new Color(1, 0.4f, 0.4f, 1);
		nodeUnlockButtonObj.SetActive(false);
	}
	private void UnlockNode()
	{
		image.color = new Color(1, 1, 1, 1);
		nodeUnlockButtonObj.SetActive(true);
	}
	private void ActivateNode()
	{
		image.color = new Color(0.4f, 1, 0.4f, 1);
	}
	private void ResetNode(SOClasses currentClass)
	{
		isAlreadyUnlocked = false;
		LockNode();
		if (nodeLevelRequirment == 1)
			UnlockNode();
	}
}