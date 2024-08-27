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

	public void RestorePlayerSettingsData(bool manualCastOffensiveAbilities)
	{
		this.manualCastOffensiveAbilities = manualCastOffensiveAbilities;
	}
}
