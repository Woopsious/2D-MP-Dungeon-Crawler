using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassTreeNodeUi : MonoBehaviour
{
	public ClassStatUnlocks statUnlock;
	public ClassAbilityUnlocks abilityUnlock;

	private ToolTipUi toolTip;
	public TMP_Text nodeInfoText;
	private Image image;
	public GameObject nodeUnlockButtonObj;
	public GameObject nodeRefundButtonObj;

	public bool isAlreadyUnlocked;

	public int nodeVerticalParentIndex;
	public int nodeHorizontalIndex;
	public int nodeIndex;

	private void OnEnable()
	{
		PlayerClassesUi.OnClassNodeUnlocks += CheckIfNodeShouldBeLockedOrUnlocked;
		PlayerClassesUi.OnClassChanges += ResetNode;
	}
	private void OnDisable()
	{
		PlayerClassesUi.OnClassNodeUnlocks -= CheckIfNodeShouldBeLockedOrUnlocked;
		PlayerClassesUi.OnClassChanges -= ResetNode;
	}
	public void Initilize(int newVerticalIndex)
	{
		nodeVerticalParentIndex = newVerticalIndex;
		nodeHorizontalIndex = transform.GetSiblingIndex();

		if (statUnlock != null)
			nodeInfoText.text = statUnlock.unlock.Name;
		else if (abilityUnlock != null)
			nodeInfoText.text = abilityUnlock.unlock.Name;
		else
			Debug.LogError("Skill tree slot has no reference, this shouldnt happen");

		image = GetComponent<Image>();
		nodeUnlockButtonObj.GetComponent<Button>().onClick.AddListener(UnlockThisNode);
		nodeRefundButtonObj.GetComponent<Button>().onClick.AddListener(RefundThisNode);
		ResetNode(null);
	}

	//tool tip
	public void SetToolTip(EntityStats playerStats)
	{
		toolTip = GetComponent<ToolTipUi>();

		if (statUnlock != null)
			SetStatBonusToolTip();
		if (abilityUnlock != null)
			SetAbilityToolTip(playerStats);
	}
	private void SetStatBonusToolTip()
	{
		string info = $"{statUnlock.unlock.Description} \n";

		if (statUnlock.unlock.healthBoostValue != 0)
			info += $"\nBoosts max health by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.healthBoostValue)}%";
		if (statUnlock.unlock.manaBoostValue != 0)
			info += $"\nBoosts max mana by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.manaBoostValue)}%";
		if (statUnlock.unlock.manaRegenBoostValue != 0)
			info += $"\nBoosts mana regen by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.manaRegenBoostValue)}%";

		if (statUnlock.unlock.physicalResistanceBoostValue != 0)
			info += $"\nBoosts physical res by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.physicalResistanceBoostValue)}%";
		if (statUnlock.unlock.poisonResistanceBoostValue != 0)
			info += $"\nBoosts poison res by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.poisonResistanceBoostValue)}%";
		if (statUnlock.unlock.fireResistanceBoostValue != 0)
			info += $"\nBoosts fire res by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.fireResistanceBoostValue)}%";
		if (statUnlock.unlock.iceResistanceBoostValue != 0)
			info += $"\nBoosts ice res by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.iceResistanceBoostValue)}%";

		if (statUnlock.unlock.physicalDamageBoostValue != 0)
			info += $"\nBoosts physical damage by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.physicalDamageBoostValue)}%";
		if (statUnlock.unlock.poisionDamageBoostValue != 0)
			info += $"\nBoosts poison damage by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.poisionDamageBoostValue)}%";
		if (statUnlock.unlock.fireDamageBoostValue != 0)
			info += $"\nBoosts fire damage by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.fireDamageBoostValue)}%";
		if (statUnlock.unlock.iceDamageBoostValue != 0)
			info += $"\nBoosts ice damage by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.iceDamageBoostValue)}%";

		if (statUnlock.unlock.mainWeaponDamageBoostValue != 0)
			info += $"\nBoosts main weapon damage by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.mainWeaponDamageBoostValue)}%";
		if (statUnlock.unlock.duelWeaponDamageBoostValue != 0)
			info += $"\nBoosts dual weapon damage by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.duelWeaponDamageBoostValue)}%";
		if (statUnlock.unlock.rangedWeaponDamageBoostValue != 0)
			info += $"\nBoosts ranged weapon damage by {Utilities.ConvertFloatToUiPercentage(statUnlock.unlock.rangedWeaponDamageBoostValue)}%";

		toolTip.tipToShow = $"{info}";
	}
	private void SetAbilityToolTip(EntityStats playerStats)
	{
		string info = $"{abilityUnlock.unlock.Description}";

		if (abilityUnlock.unlock.canOnlyTargetSelf) //targeting info
			info += "\nCan only cast on self";
		else if (abilityUnlock.unlock.requiresTarget && abilityUnlock.unlock.isOffensiveAbility)
			info += "\nNeeds selected enemy target";
		else if (abilityUnlock.unlock.requiresTarget && !abilityUnlock.unlock.isOffensiveAbility)
			info += "\nNeeds selected friendly target";

		if (abilityUnlock.unlock.damageType != IDamagable.DamageType.isHealing) //abilty info
		{
			int damage = (int)(abilityUnlock.unlock.damageValue * Utilities.GetLevelModifier(playerStats.entityLevel));

			if (abilityUnlock.unlock.damageType == IDamagable.DamageType.isPhysicalDamage)
				damage = (int)(damage * playerStats.physicalDamagePercentageModifier.finalPercentageValue);
			if (abilityUnlock.unlock.damageType == IDamagable.DamageType.isPoisonDamage)
				damage = (int)(damage * playerStats.poisonDamagePercentageModifier.finalPercentageValue);
			if (abilityUnlock.unlock.damageType == IDamagable.DamageType.isFireDamage)
				damage = (int)(damage * playerStats.fireDamagePercentageModifier.finalPercentageValue);
			if (abilityUnlock.unlock.damageType == IDamagable.DamageType.isIceDamage)
				damage = (int)(damage * playerStats.iceDamagePercentageModifier.finalPercentageValue);

			if (damage != 0) //optional instant damage info
			{
				info += $"\nDeals {damage} damage to enemies ";
				if (abilityUnlock.unlock.isAOE) //optional aoe info
					info += "inside AoE";
			}

			if (abilityUnlock.unlock.hasAoeDuration) //optional aoe info
				info += $"\nAoe lasts for {abilityUnlock.unlock.aoeDuration}s";

			if (abilityUnlock.unlock.hasStatusEffects) //optional effect info
				info += SetStatusEffectToolTips(playerStats);
		}
		else if (abilityUnlock.unlock.damageType == IDamagable.DamageType.isHealing) //healing info
		{
			float healing = Utilities.ConvertFloatToUiPercentage(abilityUnlock.unlock.damageValuePercentage);

			if (abilityUnlock.unlock.isAOE)
				info += $"\nHeals for {healing}% of health for friendlies inside AoE";
			else
				info += $"\nHeals for {healing}% of health for selected friendlies or self ";
		}

		info += $"\nHas a {abilityUnlock.unlock.abilityCooldown}s cooldown"; //cooldown effect info

		if (abilityUnlock.unlock.isSpell) //optional spell info
			info += $"\nCosts {(int)(abilityUnlock.unlock.manaCost * Utilities.GetLevelModifier(playerStats.entityLevel))} mana";

		toolTip.tipToShow = $"{info}";
	}
	private string SetStatusEffectToolTips(EntityStats playerStats)
	{
		string info = "\n";

		foreach (SOStatusEffects effect in abilityUnlock.unlock.statusEffects) //list all effect info
		{
			if (effect.isDOT) //dot info
				info += $"Deals {effect.effectValue * playerStats.levelModifier} damage every second";
			else //effect info
			{
				if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isDamageEffect) //effect type info
					info += $"Applies a {Utilities.ConvertFloatToUiPercentage(effect.effectValue)}% damage ";
				else if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isResistanceEffect)
					info += $"Applies a {Utilities.ConvertFloatToUiPercentage(effect.effectValue)}% damage res ";
				else if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isDamageRecievedEffect)
					info += $"Applies a {Utilities.ConvertFloatToUiPercentage(effect.effectValue)}% damage recieved modifier ";
				else if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isMovementEffect)
					info += $"Applies a {Utilities.ConvertFloatToUiPercentage(effect.effectValue)}% movement speed ";

				if (abilityUnlock.unlock.canOnlyTargetSelf) //target info
					info += "buff to yourself";
				else
				{
					if (abilityUnlock.unlock.isOffensiveAbility && abilityUnlock.unlock.isAOE)
						info += "debuff to enemies inside AoE";
					else if (!abilityUnlock.unlock.isOffensiveAbility && abilityUnlock.unlock.isAOE)
						info += "buff to friendlies/self inside AoE";

					if (abilityUnlock.unlock.isOffensiveAbility && !abilityUnlock.unlock.isAOE)
						info += "debuff to selected enemy";
					else if (!abilityUnlock.unlock.isOffensiveAbility && !abilityUnlock.unlock.isAOE)
						info += "buff to selected friendlies or self";
				}
				info += $"\nEffect lasts for {effect.abilityDuration}s";
			}
		}
		return info;
	}

	//node updates
	public void UnlockThisNode()
	{
		if (statUnlock != null)
			PlayerClassesUi.Instance.UnlockStatBonus(this, statUnlock.unlock);
		else if (abilityUnlock.unlock != null)
			PlayerClassesUi.Instance.UnlockAbility(this, abilityUnlock.unlock);
	}
	public void RefundThisNode()
	{
		if (statUnlock != null)
			PlayerClassesUi.Instance.RefundStatBonus(this, statUnlock.unlock);
		else if (abilityUnlock.unlock != null)
			PlayerClassesUi.Instance.RefundAbility(this, abilityUnlock.unlock);
	}
	private void ResetNode(SOClasses currentClass)
	{
		isAlreadyUnlocked = false;
	}

	//node checks 
	public void CheckIfNodeShouldBeLockedOrUnlocked(EntityStats playerStats)
	{
		if (isAlreadyUnlocked)
		{
			LockNode();
			ActivateNode();
			return;
		}
		if (statUnlock != null)
		{
			if (playerStats.entityLevel < statUnlock.LevelRequirement)
			{
				LockNode();
				return;
			}
			if (AreStatBonusesForThisLevelAlreadyUnlocked())
			{
				LockNode();
				return;
			}
		}
		if (abilityUnlock != null)
		{
			if (playerStats.entityLevel < abilityUnlock.LevelRequirement)
			{
				LockNode();
				return;
			}
			if (!PlayerClassesUi.Instance.DoesPlayerHaveFreeAbilitySlot())
			{
				LockNode();
				return;
			}
		}
		UnlockNode();
	}
	private bool AreStatBonusesForThisLevelAlreadyUnlocked()
	{
		Transform parentTransform = transform.parent;

		if (parentTransform.GetChild(0).GetComponent<ClassTreeNodeUi>().isAlreadyUnlocked == true)
			return true;
		else if (parentTransform.childCount == 2)
		{
			if (parentTransform.GetChild(1).GetComponent<ClassTreeNodeUi>().isAlreadyUnlocked == true)
				return true;
			else return false;
		}
		else return false;
	}

	//node states
	private void LockNode()
	{
		image.color = new Color(1, 0.4f, 0.4f, 1);
		nodeUnlockButtonObj.SetActive(false);
		nodeRefundButtonObj.SetActive(false);
	}
	private void UnlockNode()
	{
		image.color = new Color(1, 1, 1, 1);
		nodeUnlockButtonObj.SetActive(true);
		nodeRefundButtonObj.SetActive(false);
	}
	private void ActivateNode()
	{
		image.color = new Color(0.4f, 1, 0.4f, 1);
		nodeRefundButtonObj.SetActive(true);
	}
}