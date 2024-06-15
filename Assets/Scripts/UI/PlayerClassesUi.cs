using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerClassesUi : MonoBehaviour
{
	public static PlayerClassesUi Instance;

	[Header("Class Selection")]
	public GameObject playerClassSelectionPanel;
	public SOClasses currentPlayerClass;
	public List<ClassTreeNodeSlotUi> currentUnlockedClassNodes = new List<ClassTreeNodeSlotUi>();

	[Header("knight Class")]
	public SOClasses knightClass;
	public GameObject knightClassPanel;
	public List<GameObject> knightClassNodes;
	public Button playAsKnightButton;

	[Header("Warrior Class")]
	public SOClasses warriorClass;
	public GameObject warriorClassPanel;
	public List<GameObject> warriorClassNodes;
	public Button playAsWarriorButton;

	[Header("Rogue Class")]
	public SOClasses rogueClass;
	public GameObject rogueClassPanel;
	public List<GameObject> rogueClassNodes;
	public Button playAsRogueButton;

	[Header("Ranger Class")]
	public SOClasses rangerClass;
	public GameObject rangerClassPanel;
	public List<GameObject> rangerClassNodes;
	public Button playAsRangerButton;

	[Header("Mage Class")]
	public SOClasses mageClass;
	public GameObject MageClassPanel;
	public List<GameObject> mageClassNodes;
	public Button playAsMageButton;

	[Header("Shared Class ui elements")]
	public GameObject closeClassTreeButtonObj;
	public GameObject resetPlayerClassButtonObj;

	public static event Action<SOClasses> OnClassChange;
	public static event Action<SOClasses> OnClassReset;

	public static event Action<EntityStats> OnClassNodeUnlocks;

	public static event Action<SOClassStatBonuses> OnNewStatBonusUnlock;
	public static event Action<SOClassAbilities> OnNewAbilityUnlock;

	/// <summary> (NEW IDEA)
	/// player selects class on new game start. if reloading game, indexes of Unlocked ClassTreeSlotUi nodes are saved and loaded to disk,
	/// from a List in PlayerClassHandler, nodes are then auto unlocked based on those indexes when reloading a game.
	/// when player opens or levels up updating ui and check if nodes could be unlocked (check for exclusions, player level, pre requisites).
	/// once player unlocks new node, event sent to PlayerClassHandler that applies stat bonuses, PlayerClassHandler calls another event
	/// </summary>

	private void Awake()
	{
		Instance = this;
		Initilize();
	}
	private void OnEnable()
	{
		SaveManager.RestoreData += ReloadPlayerClass;

		EventManager.OnShowPlayerInventoryEvent += HidePlayerClassSelection;
		EventManager.OnShowPlayerClassSelectionEvent += ShowPlayerClassSelection;
		EventManager.OnShowPlayerSkillTreeEvent += HidePlayerClassSelection;
		EventManager.OnShowPlayerLearntAbilitiesEvent += HidePlayerClassSelection;
		EventManager.OnShowPlayerJournalEvent += HidePlayerClassSelection;

		EventManager.OnShowPlayerInventoryEvent += HideClassSkillTree;
		EventManager.OnShowPlayerClassSelectionEvent += HideClassSkillTree;
		EventManager.OnShowPlayerSkillTreeEvent += ShowClassSkillTree;
		EventManager.OnShowPlayerLearntAbilitiesEvent += HideClassSkillTree;
		EventManager.OnShowPlayerJournalEvent += HideClassSkillTree;
	}
	private void OnDisable()
	{
		SaveManager.RestoreData -= ReloadPlayerClass;

		EventManager.OnShowPlayerInventoryEvent -= HidePlayerClassSelection;
		EventManager.OnShowPlayerClassSelectionEvent -= ShowPlayerClassSelection;
		EventManager.OnShowPlayerSkillTreeEvent -= HidePlayerClassSelection;
		EventManager.OnShowPlayerLearntAbilitiesEvent -= HidePlayerClassSelection;
		EventManager.OnShowPlayerJournalEvent -= HidePlayerClassSelection;

		EventManager.OnShowPlayerInventoryEvent -= HideClassSkillTree;
		EventManager.OnShowPlayerClassSelectionEvent -= HideClassSkillTree;
		EventManager.OnShowPlayerSkillTreeEvent -= ShowClassSkillTree;
		EventManager.OnShowPlayerLearntAbilitiesEvent -= HideClassSkillTree;
		EventManager.OnShowPlayerJournalEvent -= HideClassSkillTree;
	}

	private void Initilize()
	{
		SetSkillTreeIndexes(knightClassPanel);
		SetSkillTreeIndexes(warriorClassPanel);
		SetSkillTreeIndexes(rogueClassPanel);
		SetSkillTreeIndexes(rangerClassPanel);
		SetSkillTreeIndexes(MageClassPanel);
	}
	private void SetSkillTreeIndexes(GameObject parentObj)
	{
		for (int i = 0; i < parentObj.transform.childCount - 1; i++)
			parentObj.transform.GetChild(i).gameObject.GetComponent<ClassTreeNodeSlotUi>().Initilize();
	}

	//skill tree node unlocks
	public void UnlockStatBonus(ClassTreeNodeSlotUi classTreeSlot, SOClassStatBonuses statBonus)
	{
		currentUnlockedClassNodes.Add(classTreeSlot);
		OnNewStatBonusUnlock?.Invoke(statBonus);
	}
	public void UnlockAbility(ClassTreeNodeSlotUi classTreeSlot, SOClassAbilities ability)
	{
		currentUnlockedClassNodes.Add(classTreeSlot);
		OnNewAbilityUnlock?.Invoke(ability);
	}

	public void UpdateNodesInClassTree(EntityStats playerStats)
	{
		OnClassNodeUnlocks?.Invoke(playerStats);
	}

	//reload player class
	public void ReloadPlayerClass()
	{
		if (SaveManager.Instance.GameData.currentPlayerClass == null) return;

		SetNewClass(SaveManager.Instance.GameData.currentPlayerClass);

		if (currentPlayerClass == knightClass)
			ReloadPlayerClassTreeNodes(knightClassPanel);
		if (currentPlayerClass == warriorClass)
			ReloadPlayerClassTreeNodes(warriorClassPanel);
		if (currentPlayerClass == rogueClass)
			ReloadPlayerClassTreeNodes(rogueClassPanel);
		if (currentPlayerClass == rangerClass)
			ReloadPlayerClassTreeNodes(rangerClassPanel);
		if (currentPlayerClass == mageClass)
			ReloadPlayerClassTreeNodes(MageClassPanel);

		HidePlayerClassSelection();
		/// <summery>
		/// based on indexes in	SaveManager.Instance.GameData.unlockedClassNodeIndexesList and current, loop through a list
		/// of said classes Nodes, and reunlock those bypassing checks
		/// <summery>
	}
	private void ReloadPlayerClassTreeNodes(GameObject currentClassPanelUi)
	{
		List<int> indexs = SaveManager.Instance.GameData.unlockedClassNodeIndexesList;
		foreach (int index in indexs)
			currentClassPanelUi.transform.GetChild(index).GetComponent<ClassTreeNodeSlotUi>().UnlockThisNodeButton();
	}

	//UI CHANGES
	//classes
	public void PlayAsKnightButton()
	{
		SetNewClass(knightClass);
	}
	public void PlayAsWarriorButton()
	{
		SetNewClass(warriorClass);
	}
	public void PlayAsRogueButton()
	{
		SetNewClass(rogueClass);
	}
	public void PlayAsRangerButton()
	{
		SetNewClass(rangerClass);
	}
	public void PlayAsMageButton()
	{
		SetNewClass(mageClass);
	}
	public void SetNewClass(SOClasses newClass)
	{
		if (currentPlayerClass != null)
			ResetCurrentClassButton();

		OnClassChange?.Invoke(newClass);
		currentPlayerClass = newClass;
		EventManager.ShowPlayerSkillTree();
	}
	public void ResetCurrentClassButton()
	{
		OnClassReset?.Invoke(currentPlayerClass);
		currentUnlockedClassNodes.Clear();
	}

	//class selection
	public void ShowHidePlayerClassSelection()
	{
		if (playerClassSelectionPanel.activeInHierarchy)
			HidePlayerClassSelection();
		else
			ShowPlayerClassSelection();
	}
	public void ShowPlayerClassSelection()
	{
		if (playerClassSelectionPanel.activeInHierarchy)
			HidePlayerClassSelection();
		else
			playerClassSelectionPanel.SetActive(true);
	}
	public void HidePlayerClassSelection()
	{
		playerClassSelectionPanel.SetActive(false);
	}

	//class skill trees
	public void ShowHideClassSkillTree()
	{
		if (closeClassTreeButtonObj.activeInHierarchy)
			HideClassSkillTree();
		else
			ShowClassSkillTree();
	}
	public void ShowClassSkillTree()
	{
		if (currentPlayerClass == null) return;

		if (closeClassTreeButtonObj.activeInHierarchy)
			HideClassSkillTree();
		else
		{
			if (currentPlayerClass == knightClass)
			{
				knightClassPanel.SetActive(true);
				UpdatePlayerToolTips(knightClassNodes);
			}
			if (currentPlayerClass == warriorClass)
			{
				warriorClassPanel.SetActive(true);
				UpdatePlayerToolTips(warriorClassNodes);
			}
			if (currentPlayerClass == rogueClass)
			{
				rogueClassPanel.SetActive(true);
				UpdatePlayerToolTips(rogueClassNodes);
			}
			if (currentPlayerClass == rangerClass)
			{
				rangerClassPanel.SetActive(true);
				UpdatePlayerToolTips(rangerClassNodes);
			}
			if (currentPlayerClass == mageClass)
			{
				MageClassPanel.SetActive(true);
				UpdatePlayerToolTips(mageClassNodes);
			}

			closeClassTreeButtonObj.SetActive(true);
			resetPlayerClassButtonObj.SetActive(true);
		}
	}
	public void HideClassSkillTree()
	{
		closeClassTreeButtonObj.SetActive(false);
		resetPlayerClassButtonObj.SetActive(false);
		knightClassPanel.SetActive(false);
		warriorClassPanel.SetActive(false);
		rogueClassPanel.SetActive(false);
		rangerClassPanel.SetActive(false);
		MageClassPanel.SetActive(false);
	}

	private void UpdatePlayerToolTips(List<GameObject> objList)
	{
		foreach (GameObject obj in objList)
		{
			if (obj.GetComponent<ToolTipUi>() == null) continue;
			ToolTipUi tip = obj.GetComponent<ToolTipUi>();
			tip.UpdatePlayerToolTip();
		}
	}
}
