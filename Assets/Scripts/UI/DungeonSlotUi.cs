using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonSlotUi : MonoBehaviour
{
	public TMP_Text dungeonDifficultyText;
	public TMP_Text dungeonModifiersText;

	public bool isDungeonSaved;
	public int dungeonNumber;

	public int maxDungeonModifiers;
	public float dungeonDifficultyModifier;
	public List<float> dungeonModifiers;



	public void Initilize()
	{
		dungeonNumber = Utilities.GetRandomNumber(SceneManager.sceneCountInBuildSettings - 2); //(not including hub and main menu scene)
		int modifier = Utilities.GetRandomNumber(3);

		if (modifier == 0)
		{
			maxDungeonModifiers = 2;
			dungeonDifficultyModifier = 0;
			dungeonDifficultyText.text = "Difficulty: Normal \n (No Bonuses)";
		}
		else if (modifier == 1)
		{
			maxDungeonModifiers = 4;
			dungeonDifficultyModifier = 0.1f;
			dungeonDifficultyText.text = "Difficulty: hard \n (10% bonus to all enemy stats)";
		}
		else
		{
			maxDungeonModifiers = 6;
			dungeonDifficultyModifier = 0.25f;
			dungeonDifficultyText.text = "Difficulty: hard \n (25% bonus to all enemy stats)";
		}

		//50/50 chance of getting modifier, if get modifier, then pick random modifier from list of all possible modifiers
		//add new modifier to list and update modifer text with += (new modifier)+(type of modifier)
	}

	public void EnterDungeon()
	{
		if (dungeonNumber == 0)
			GameManager.Instance.LoadDungeonOne();
		else if (dungeonNumber == 1)
			GameManager.Instance.LoadDungeonTwo();
	}
	public void SaveDungeon()
	{
		isDungeonSaved = true;

	}
	public void DeleteDungeon()
	{
		Destroy(gameObject);
	}
}
