using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class PlayerSettingsManager : MonoBehaviour
{
	//store all player settings here as an instance.
	//read said settings from here in PlayerController

	//save and load player settings to and from SaveManager script in player data.

	public static PlayerSettingsManager Instance;

	public bool mainAttackIsAutomatic;
	public TMP_Text mainAttackIsAutomaticText;

	public bool autoSelectNewTarget;
	public TMP_Text autoSelectNewTargetText;

	public bool autoCastDirectionalAbilitiesAtTarget;
	public TMP_Text autoCastDirectionalAbilitiesAtTargetText;

	public bool autoCastAoeAbilitiesOnTarget;
	public TMP_Text autoCastAoeAbilitiesOnTargetText;

	public bool autoCastEffectAbilitiesOnEnemyTarget;
	public TMP_Text autoCastEffectAbilitiesOnEnemyTargetText;

	public bool autoCastEffectAbilitiesOnFriendlyTarget;
	public TMP_Text autoCastEffectAbilitiesOnFriendlyTargetText;

	private void Awake()
	{
		Instance = this;
	}

	//restore data
	public void RestorePlayerSettingsData(bool mainAttackIsAutomatic, bool autoSelectNewTarget, bool autoCastDirectionalAbilitiesAtTarget, 
		bool autoCastAoeAbilitiesOnTarget, bool autoCastEffectsOnEnemyTarget, bool autoCastEffectsOnFriendlyTarget)
	{
		if (!mainAttackIsAutomatic)
			mainAttackIsAutomaticText.text = "Main Attack is Automatic: \nFalse";
		else
			mainAttackIsAutomaticText.text = "Main Attack is Automatic: \nTrue";

		this.mainAttackIsAutomatic = mainAttackIsAutomatic;


		if (!autoSelectNewTarget)
			autoSelectNewTargetText.text = "Auto select closest target when no target already selected: \nFalse";
		else
			autoSelectNewTargetText.text = "Auto select closest target when no target already selected: \nTrue";

		this.autoSelectNewTarget = autoSelectNewTarget;


		if (!autoCastDirectionalAbilitiesAtTarget)
			autoCastDirectionalAbilitiesAtTargetText.text = "Auto cast directional abilities at selected targets: \nFalse";
		else
			autoCastDirectionalAbilitiesAtTargetText.text = "Auto cast directional abilities at selected targets: \nTrue";

		this.autoCastDirectionalAbilitiesAtTarget = autoCastDirectionalAbilitiesAtTarget;


		if (!autoCastAoeAbilitiesOnTarget)
			autoCastAoeAbilitiesOnTargetText.text = "Auto cast AOE abilities on selected targets: \nFalse";
		else
			autoCastAoeAbilitiesOnTargetText.text = "Auto cast AOE abilities on selected targets: \nTrue";

		this.autoCastAoeAbilitiesOnTarget = autoCastAoeAbilitiesOnTarget;


		if (!autoCastEffectsOnEnemyTarget)
			autoCastEffectAbilitiesOnEnemyTargetText.text = "Auto cast effects on selected enemy targets: \nFalse";
		else
			autoCastEffectAbilitiesOnEnemyTargetText.text = "Auto cast effects on selected enemy targets: \nTrue";

		this.autoCastEffectAbilitiesOnEnemyTarget = autoCastEffectsOnEnemyTarget;

		if (!autoCastEffectsOnFriendlyTarget)
			autoCastEffectAbilitiesOnFriendlyTargetText.text = "Auto cast effects on selected friendly targets: \nFalse";
		else
			autoCastEffectAbilitiesOnFriendlyTargetText.text = "Auto cast effects on selected friendly targets: \nTrue";

		this.autoCastEffectAbilitiesOnFriendlyTarget = autoCastEffectsOnFriendlyTarget;
	}

	//button actions
	public void ToggleMainAttackIsAutomatic()
	{
		if (mainAttackIsAutomatic)
		{
			mainAttackIsAutomaticText.text = "Main attack is automatic: \nFalse";
			mainAttackIsAutomatic = false;
		}
		else
		{
			mainAttackIsAutomaticText.text = "Main attack is automatic: \nTrue";
			mainAttackIsAutomatic = true;
		}
	}
	public void ToggleAutoSelectNewTarget()
	{
		if (autoSelectNewTarget)
		{
			autoSelectNewTargetText.text = "Auto select closest target when no target already selected: \nFalse";
			autoSelectNewTarget = false;
		}
		else
		{
			autoSelectNewTargetText.text = "Auto select closest target when no target already selected: \nTrue";
			autoSelectNewTarget = true;
		}
	}
	public void ToggleAutoCastDirectionalAbilitiesAtTarget()
	{
		if (autoCastDirectionalAbilitiesAtTarget)
		{
			autoCastDirectionalAbilitiesAtTargetText.text = "Auto cast directional abilities at selected targets: \nFalse";
			autoCastDirectionalAbilitiesAtTarget = false;
		}
		else
		{
			autoCastDirectionalAbilitiesAtTargetText.text = "Auto cast directional abilities at selected targets: \nTrue";
			autoCastDirectionalAbilitiesAtTarget = true;
		}
	}
	public void ToggleAutoCastAoeAbilitiesOnTarget()
	{
		if (autoCastAoeAbilitiesOnTarget)
		{
			autoCastAoeAbilitiesOnTargetText.text = "Auto cast AOE abilities on selected targets: \nFalse";
			autoCastAoeAbilitiesOnTarget = false;
		}
		else
		{
			autoCastAoeAbilitiesOnTargetText.text = "Auto cast AOE abilities on selected targets: \nTrue";
			autoCastAoeAbilitiesOnTarget = true;
		}
	}
	public void ToggleAutoCastEffectAbilitiesOnEnemyTarget()
	{
		if (autoCastEffectAbilitiesOnEnemyTarget)
		{
			autoCastEffectAbilitiesOnEnemyTargetText.text = "Auto cast effects on selected enemy targets: \nFalse";
			autoCastEffectAbilitiesOnEnemyTarget = false;
		}
		else
		{
			autoCastEffectAbilitiesOnEnemyTargetText.text = "Auto cast effects on selected enemy targets: \nTrue";
			autoCastEffectAbilitiesOnEnemyTarget = true;
		}
	}
	public void ToggleAutoCastEffectAbilitiesOnFriendlyTarget()
	{
		if (autoCastEffectAbilitiesOnFriendlyTarget)
		{
			autoCastEffectAbilitiesOnFriendlyTargetText.text = "Auto cast effects on selected friendly targets: \nFalse";
			autoCastEffectAbilitiesOnFriendlyTarget = false;
		}
		else
		{
			autoCastEffectAbilitiesOnFriendlyTargetText.text = "Auto cast effects on selected friendly targets: \nTrue";
			autoCastEffectAbilitiesOnFriendlyTarget = true;
		}
	}
}
