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

	public bool manualCastOffensiveAbilities;
	public TMP_Text manualCastOffensiveAbilitiesText;

	public bool autoCastOffensiveAoeAbilitiesOnSelectedTarget;
	public TMP_Text autoCastOffensiveAoeAbilitiesOnSelectedTargetText;

	public bool autoCastOffensiveDirectionalAbilitiesAtSelectedTarget;
	public TMP_Text autoCastOffensiveDirectionalAbilitiesAtSelectedTargetText;

	private void Awake()
	{
		Instance = this;
	}

	//restore data
	public void RestorePlayerSettingsData(bool mainAttackIsAutomatic, bool manualCastOffensiveAbilities,
		bool autoCastOffensiveAoeAbilitiesOnSelectedTarget, bool autoCastOffensiveDirectionalAbilitiesAtSelectedTarget)
	{
		if (mainAttackIsAutomatic)
			mainAttackIsAutomaticText.text = "Main Attack is Automatic: False";
		else
			mainAttackIsAutomaticText.text = "Main Attack is Automatic: True";

		this.mainAttackIsAutomatic = mainAttackIsAutomatic;


		if (manualCastOffensiveAbilities)
			manualCastOffensiveAbilitiesText.text = "Manual Cast Offensive Abilities: False";
		else
			manualCastOffensiveAbilitiesText.text = "Manual Cast Offensive Abilities: True";

		this.manualCastOffensiveAbilities = manualCastOffensiveAbilities;


		if (autoCastOffensiveAoeAbilitiesOnSelectedTarget)
			autoCastOffensiveAoeAbilitiesOnSelectedTargetText.text = "Auto cast offensive aoe abilities on selected targets: False";
		else
			autoCastOffensiveAoeAbilitiesOnSelectedTargetText.text = "Auto cast offensive aoe abilities on selected targets: True";

		this.mainAttackIsAutomatic = autoCastOffensiveAoeAbilitiesOnSelectedTarget;


		if (autoCastOffensiveDirectionalAbilitiesAtSelectedTarget)
			autoCastOffensiveDirectionalAbilitiesAtSelectedTargetText.text = "Auto cast offensive Directional Abilities at selected targets: False";
		else
			autoCastOffensiveDirectionalAbilitiesAtSelectedTargetText.text = "Auto cast offensive Directional Abilities at selected targets: True";

		this.mainAttackIsAutomatic = autoCastOffensiveDirectionalAbilitiesAtSelectedTarget;
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
	public void ToggleManualCastOffensiveAbilities()
	{
		if (manualCastOffensiveAbilities)
		{
			manualCastOffensiveAbilitiesText.text = "Manual cast offensive abilities: False";
			manualCastOffensiveAbilities = false;
		}
		else
		{
			manualCastOffensiveAbilitiesText.text = "Manual cast offensive abilities: True";
			manualCastOffensiveAbilities = true;
		}
	}
	public void ToggleAutoCastOffensiveAoeAbilitiesOnSelectedTarget()
	{
		if (autoCastOffensiveAoeAbilitiesOnSelectedTarget)
		{
			autoCastOffensiveAoeAbilitiesOnSelectedTargetText.text = "Auto cast offensive aoe abilities on selected targets: False";
			autoCastOffensiveAoeAbilitiesOnSelectedTarget = false;
		}
		else
		{
			autoCastOffensiveAoeAbilitiesOnSelectedTargetText.text = "Auto cast offensive aoe abilities on selected targets: True";
			autoCastOffensiveAoeAbilitiesOnSelectedTarget = true;
		}
	}
	public void ToggleAutoCastOffensiveDirectionalAbilitiesAtSelectedTarget()
	{
		if (autoCastOffensiveDirectionalAbilitiesAtSelectedTarget)
		{
			autoCastOffensiveDirectionalAbilitiesAtSelectedTargetText.text = "Auto cast offensive Directional Abilities at selected targets: False";
			autoCastOffensiveDirectionalAbilitiesAtSelectedTarget = false;
		}
		else
		{
			autoCastOffensiveDirectionalAbilitiesAtSelectedTargetText.text = "Auto cast offensive Directional Abilities at selected targets: True";
			autoCastOffensiveDirectionalAbilitiesAtSelectedTarget = true;
		}
	}
}
