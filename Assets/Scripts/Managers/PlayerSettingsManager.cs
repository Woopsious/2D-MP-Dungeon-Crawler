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

	public bool autoCastEffectAbilitiesOnTarget;
	public TMP_Text autoCastEffectAbilitiesOnTargetText;

	private void Awake()
	{
		Instance = this;
	}

	//restore data
	public void RestorePlayerSettingsData(bool mainAttackIsAutomatic, bool autoSelectNewTarget,
		bool autoCastDirectionalAbilitiesAtTarget, bool autoCastAoeAbilitiesOnTarget, bool autoCastEffectsOnTarget)
	{
		if (mainAttackIsAutomatic)
			mainAttackIsAutomaticText.text = "Main Attack is Automatic: False";
		else
			mainAttackIsAutomaticText.text = "Main Attack is Automatic: True";

		this.mainAttackIsAutomatic = mainAttackIsAutomatic;


		if (autoSelectNewTarget)
			mainAttackIsAutomaticText.text = "Auto select closest target when no target already selected: False";
		else
			mainAttackIsAutomaticText.text = "Auto select closest target when no target already selected: True";

		this.autoSelectNewTarget = autoSelectNewTarget;


		if (autoCastDirectionalAbilitiesAtTarget)
			autoCastDirectionalAbilitiesAtTargetText.text = "Auto cast directional abilities at selected targets: False";
		else
			autoCastDirectionalAbilitiesAtTargetText.text = "Auto cast directional abilities at selected targets: True";

		this.autoCastDirectionalAbilitiesAtTarget = autoCastDirectionalAbilitiesAtTarget;


		if (autoCastAoeAbilitiesOnTarget)
			autoCastAoeAbilitiesOnTargetText.text = "Auto cast AOE abilities on selected targets: False";
		else
			autoCastAoeAbilitiesOnTargetText.text = "Auto cast AOE abilities on selected targets: True";

		this.autoCastAoeAbilitiesOnTarget = autoCastAoeAbilitiesOnTarget;


		if (autoCastEffectsOnTarget)
			autoCastEffectAbilitiesOnTargetText.text = "Auto cast effects on selected targets: False";
		else
			autoCastEffectAbilitiesOnTargetText.text = "Auto cast effects on selected targets: True";

		this.autoCastEffectAbilitiesOnTarget = autoCastEffectsOnTarget;
	}

	//button actions
	public void ToggleMainAttackIsAutomatic()
	{
		if (mainAttackIsAutomatic)
		{
			mainAttackIsAutomaticText.text = "Main attack is automatic: False";
			mainAttackIsAutomatic = false;
		}
		else
		{
			mainAttackIsAutomaticText.text = "Main attack is automatic: True";
			mainAttackIsAutomatic = true;
		}
	}
	public void ToggleAutoSelectNewTarget()
	{
		if (autoSelectNewTarget)
		{
			autoSelectNewTargetText.text = "Auto select closest target when no target already selected: False";
			autoSelectNewTarget = false;
		}
		else
		{
			autoSelectNewTargetText.text = "Auto select closest target when no target already selected: True";
			autoSelectNewTarget = true;
		}
	}
	public void ToggleAutoCastDirectionalAbilitiesAtTarget()
	{
		if (autoCastDirectionalAbilitiesAtTarget)
		{
			autoCastDirectionalAbilitiesAtTargetText.text = "Auto cast directional abilities at selected targets: False";
			autoCastDirectionalAbilitiesAtTarget = false;
		}
		else
		{
			autoCastDirectionalAbilitiesAtTargetText.text = "Auto cast directional abilities at selected targets: True";
			autoCastDirectionalAbilitiesAtTarget = true;
		}
	}
	public void ToggleAutoCastAoeAbilitiesOnTarget()
	{
		if (autoCastAoeAbilitiesOnTarget)
		{
			autoCastAoeAbilitiesOnTargetText.text = "Auto cast AOE abilities on selected targets: False";
			autoCastAoeAbilitiesOnTarget = false;
		}
		else
		{
			autoCastAoeAbilitiesOnTargetText.text = "Auto cast AOE abilities on selected targets: True";
			autoCastAoeAbilitiesOnTarget = true;
		}
	}
	public void ToggleAutoCastEffectAbilitiesOnTarget()
	{
		if (autoCastEffectAbilitiesOnTarget)
		{
			autoCastEffectAbilitiesOnTargetText.text = "Auto cast effects on selected targets: False";
			autoCastEffectAbilitiesOnTarget = false;
		}
		else
		{
			autoCastEffectAbilitiesOnTargetText.text = "Auto cast effects on selected targets: True";
			autoCastEffectAbilitiesOnTarget = true;
		}
	}
}
