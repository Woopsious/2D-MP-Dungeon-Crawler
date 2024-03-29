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

	private ToolTipUi toolTip;
	public TMP_Text nodeInfoText;
	private Image image;
	public GameObject nodeUnlockButtonObj;

	public int nodeLevelRequirment;
	public bool isAlreadyUnlocked;
	public int nodeIndex;

	[Tooltip("Prev skill nodes needed to unlock this one (only needs 1 owned to pass check, can have multiple")]
	public List<ClassTreeNodeSlotUi> preRequisites = new List<ClassTreeNodeSlotUi>();
	[Tooltip("Nodes that are exclusive to this one (must have all exclusions to be excluded)")]
	public List<ClassTreeNodeSlotUi> exclusions = new List<ClassTreeNodeSlotUi>();
	[Tooltip("Nodes that are exclusive to this one, mainly used for duplicate skills in class tree")]
	public List<ClassTreeNodeSlotUi> hardExclusions = new List<ClassTreeNodeSlotUi>();

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
	public void Initilize()
	{
		if (statBonus != null)
			nodeInfoText.text = statBonus.Name;
		else if (ability != null)
			nodeInfoText.text = ability.Name;
		else
			Debug.LogError("Skill tree slot has no reference, this shouldnt happen");

		image = GetComponent<Image>();
		nodeIndex = transform.GetSiblingIndex();
		nodeUnlockButtonObj.GetComponent<Button>().onClick.AddListener(UnlockThisNodeButton);
		ResetNode(null);
	}
	public void SetToolTip(EntityStats playerStats)
	{
		toolTip = GetComponent<ToolTipUi>();

		if (statBonus != null)
			SetStatBonusToolTip();
		if (ability != null)
			SetAbilityTypeToolTip(playerStats);
	}
	//tool tip
	private void SetStatBonusToolTip()
	{
		string info = $"{statBonus.Description} \n";

		if (statBonus.healthBoostValue != 0)
			info += $"\nBoosts health by {Utilities.ConvertFloatToUiPercentage(statBonus.healthBoostValue)}%";
		if (statBonus.manaBoostValue != 0)
			info += $"\nBoosts mana by {Utilities.ConvertFloatToUiPercentage(statBonus.manaBoostValue)}%";
		if (statBonus.manaRegenBoostValue != 0)
			info += $"\nBoosts mana regen by {Utilities.ConvertFloatToUiPercentage(statBonus.manaRegenBoostValue)}%";

		if (statBonus.physicalResistanceBoostValue != 0)
			info += $"\nBoosts physical res by {Utilities.ConvertFloatToUiPercentage(statBonus.physicalResistanceBoostValue)}%";
		if (statBonus.poisonResistanceBoostValue != 0)
			info += $"\nBoosts poison res by {Utilities.ConvertFloatToUiPercentage(statBonus.poisonResistanceBoostValue)}%";
		if (statBonus.fireResistanceBoostValue != 0)
			info += $"\nBoosts fire res by {Utilities.ConvertFloatToUiPercentage(statBonus.fireResistanceBoostValue)}%";
		if (statBonus.iceResistanceBoostValue != 0)
			info += $"\nBoosts ice res by {Utilities.ConvertFloatToUiPercentage(statBonus.iceResistanceBoostValue)}%";

		if (statBonus.physicalDamageBoostValue != 0)
			info += $"\nBoosts physical damage by {Utilities.ConvertFloatToUiPercentage(statBonus.physicalDamageBoostValue)}%";
		if (statBonus.poisionDamageBoostValue != 0)
			info += $"\nBoosts poison damage by {Utilities.ConvertFloatToUiPercentage(statBonus.poisionDamageBoostValue)}%";
		if (statBonus.fireDamageBoostValue != 0)
			info += $"\nBoosts fire damage by {Utilities.ConvertFloatToUiPercentage(statBonus.fireDamageBoostValue)}%";
		if (statBonus.iceDamageBoostValue != 0)
			info += $"\nBoosts ice damage by {Utilities.ConvertFloatToUiPercentage(statBonus.iceDamageBoostValue)}%";

		if (statBonus.mainWeaponDamageBoostValue != 0)
			info += $"\nBoosts main weapon damage by {Utilities.ConvertFloatToUiPercentage(statBonus.mainWeaponDamageBoostValue)}%";
		if (statBonus.duelWeaponDamageBoostValue != 0)
			info += $"\nBoosts dual weapon damage by {Utilities.ConvertFloatToUiPercentage(statBonus.duelWeaponDamageBoostValue)}%";
		if (statBonus.rangedWeaponDamageBoostValue != 0)
			info += $"\nBoosts ranged weapon damage by {Utilities.ConvertFloatToUiPercentage(statBonus.rangedWeaponDamageBoostValue)}%";

		toolTip.tipToShow = $"{info}";
	}
	private void SetAbilityTypeToolTip(EntityStats playerStats)
	{
		string info = $"{ability.Description} \n";

		if (ability.canOnlyTargetSelf)
			info += "\n Can only cast on self";
		else if (ability.requiresTarget && ability.isOffensiveAbility)
			info += "\nNeeds selected enemy target";
		else if (ability.requiresTarget && !ability.isOffensiveAbility)
			info += "\nNeeds selected friendly target";

		if (ability.statusEffectType != SOClassAbilities.StatusEffectType.noEffect)
			info = SetStatusEffectToolTip(info);
		else if (ability.statusEffectType == SOClassAbilities.StatusEffectType.noEffect)
			info = SetAbilityToolTip(info, playerStats);
		else
			Debug.LogError("Setting up ability tool tip failed");

		if (ability.isSpell) //optional
			info += $"\nCosts {(int)(ability.manaCost * Utilities.GetLevelModifier(playerStats.entityLevel))} mana";

		toolTip.tipToShow = $"{info}";
	}
	private string SetStatusEffectToolTip(string info)
	{
		if (ability.statusEffectType == SOClassAbilities.StatusEffectType.isDamageEffect)
			info += $"\nApplies a {Utilities.ConvertFloatToUiPercentage(ability.damageValuePercentage)}% damage ";
		else if (ability.statusEffectType == SOClassAbilities.StatusEffectType.isResistanceEffect)
			info += $"\nApplies a {Utilities.ConvertFloatToUiPercentage(ability.damageValuePercentage)}% damage res ";

		if (ability.canOnlyTargetSelf)
			info += "buff to yourself";
		else
		{
			if (ability.isOffensiveAbility && ability.isAOE)
				info += "debuff to enemies inside AoE";
			else if (!ability.isOffensiveAbility && ability.isAOE)
				info += "buff to friendlies/self inside AoE";

			if (ability.isOffensiveAbility && !ability.isAOE)
				info += "debuff to selected enemy";
			else if (!ability.isOffensiveAbility && !ability.isAOE)
				info += "buff to selected friendlies or self";
		}
		return info += $"\nEffect lasts for {ability.abilityDuration}s";
	}
	private string SetAbilityToolTip(string info, EntityStats playerStats)
	{
		if (ability.damageType != SOClassAbilities.DamageType.isHealing && ability.isOffensiveAbility)
		{
			int damage = (int)(ability.damageValue * Utilities.GetLevelModifier(playerStats.entityLevel));

			if (ability.damageType == SOClassAbilities.DamageType.isPhysicalDamageType)
				damage = (int)(damage * playerStats.physicalDamagePercentageModifier.finalPercentageValue);
			if (ability.damageType == SOClassAbilities.DamageType.isPoisonDamageType)
				damage = (int)(damage * playerStats.poisonDamagePercentageModifier.finalPercentageValue);
			if (ability.damageType == SOClassAbilities.DamageType.isFireDamageType)
				damage = (int)(damage * playerStats.fireDamagePercentageModifier.finalPercentageValue);
			if (ability.damageType == SOClassAbilities.DamageType.isIceDamageType)
				damage = (int)(damage * playerStats.iceDamagePercentageModifier.finalPercentageValue);

			info += $"\nDeals {damage} damage to enemies ";

			if (ability.isAOE) //optional
				info += "inside AoE";
		}
		else if (ability.damageType == SOClassAbilities.DamageType.isHealing && !ability.isOffensiveAbility)
		{
			float healing = Utilities.ConvertFloatToUiPercentage(ability.damageValuePercentage);

			if (ability.isAOE)
				info += $"\nHeals for {healing}% of health for friendlies inside AoE";
			else
				info += $"\nHeals for {healing}% of health for selected friendlies or self ";
		}
		else
			Debug.LogError("Setting up non effect ability tool tip failed");

		if (ability.isDOT)
			info += $"\nlasts for {ability.abilityDuration}s"; //optional
		return info;
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
		if (playerClassHandler.entityStats.entityLevel < nodeLevelRequirment)
		{
			LockNode();
			return;
		}
		if (CheckForHardExclusions())
		{
			LockNode();
			return;
		}
		if (CheckForNodeExclusions())
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
	public bool CheckForHardExclusions()
	{
		if (hardExclusions.Count == 0)
			return false;

		foreach (ClassTreeNodeSlotUi node in hardExclusions)
		{
			if (ClassesUi.Instance.currentUnlockedClassNodes.Contains(node))
				return true;
		}
		return false;
	}
	public bool CheckForNodeExclusions()
	{
		if (exclusions.Count == 0)
			return false;

		int numOfMatches = 0;
		foreach (ClassTreeNodeSlotUi node in exclusions)
		{
			if (ClassesUi.Instance.currentUnlockedClassNodes.Contains(node))
				numOfMatches++;
		}
		if (numOfMatches == exclusions.Count)
			return true;
        else
            return false;
    }
	public bool CheckIfAllPreRequisiteNodesUnlocked()
	{
		if (preRequisites.Count != 0)
		{
			foreach (ClassTreeNodeSlotUi node in preRequisites)
			{
				if (ClassesUi.Instance.currentUnlockedClassNodes.Contains(node))
					return true;
			}
			return false;
		}
		else return true;
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