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

	public bool manualCastOffensiveAbilities;
	public TMP_Text manualCastOffensiveAbilitiesText;

	public bool mainAttackIsAutomatic;
	public TMP_Text mainAttackIsAutomaticText;

	private void Awake()
	{
		Instance = this;
	}

	//button actions
	public void ToggleManualCastOffensiveAbilities()
	{
		if (manualCastOffensiveAbilities)
		{
			manualCastOffensiveAbilitiesText.text = "Manual Cast Offensive Abilities: False";
			manualCastOffensiveAbilities = false;
		}
		else
		{
			manualCastOffensiveAbilitiesText.text = "Manual Cast Offensive Abilities: True";
			manualCastOffensiveAbilities = true;
		}
	}
	public void ToggleMainAttackIsAutomatic()
	{
		if (mainAttackIsAutomatic)
		{
			mainAttackIsAutomaticText.text = "Main Attack is Automatic: False";
			mainAttackIsAutomatic = false;
		}
		else
		{
			mainAttackIsAutomaticText.text = "Main Attack is Automatic: True";
			mainAttackIsAutomatic = true;
		}
	}

	public void RestorePlayerSettingsData(bool manualCastOffensiveAbilities, bool mainAttackIsAutomatic)
	{
		if (manualCastOffensiveAbilities)
			manualCastOffensiveAbilitiesText.text = "Manual Cast Offensive Abilities: False";
		else
			manualCastOffensiveAbilitiesText.text = "Manual Cast Offensive Abilities: True";

		this.manualCastOffensiveAbilities = manualCastOffensiveAbilities;

		if (mainAttackIsAutomatic)
			mainAttackIsAutomaticText.text = "Main Attack is Automatic: False";
		else
			mainAttackIsAutomaticText.text = "Main Attack is Automatic: True";

		this.mainAttackIsAutomatic = mainAttackIsAutomatic;
	}
}
