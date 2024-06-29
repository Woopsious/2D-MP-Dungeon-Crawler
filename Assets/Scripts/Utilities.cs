using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Utilities
{
	//return random number
	public static int GetRandomNumber(int num) //returns num between 0 and num -1
	{
		return Random.Range(0, num + 1);
	}
	public static int GetRandomNumberBetween(int numOne, int numTwo) //returns num between numOne and numTwo -1
	{
		return Random.Range(numOne, numTwo + 1);
	}
	public static Vector2 GetRandomPointInBounds(Bounds bounds)
	{
		return new Vector2(
			Random.Range(bounds.min.x, bounds.max.x),
			Random.Range(bounds.min.y, bounds.max.y));
	}

	//return random rarity
	public static Items.Rarity SetRarity()
	{
		float percentage = GetRandomNumber(100); //3% for legendary | 10% for epic | 35% for rare | 57% for common

		if (percentage >= 98)
			return Items.Rarity.isLegendary;
		else if (percentage >= 88 && percentage < 98)
			return Items.Rarity.isEpic;
		else if (percentage >= 57 && percentage < 88)
			return Items.Rarity.isRare;
		else
			return Items.Rarity.isCommon;
	}

	//return random item lvl in range of player lvl +/- a max of 4
	public static int SetItemLevel(int level)
	{
		int entityLvl = level;
		int itemLvl = GetRandomNumber(8);
		itemLvl += entityLvl;

		if (itemLvl <= 4) //stop items from dropping below lvl 0
			itemLvl = 5;
		else if (itemLvl > 20) //stop items from dropping above max lvl (20 atm)
			itemLvl = 24;
		return itemLvl - 4;
	}

	public static float GetLevelModifier(int level)
	{
		float levelModifier;
		if (level == 1)  //get level modifier
			levelModifier = 1;
		else
			levelModifier = 1 + (level / 10f);

		return levelModifier;
	}
	public static float GetStatModifier(int level, IGetStatModifier.Rarity rarity)
	{
		float levelModifier;
		if (level == 1)  //get level modifier
			levelModifier = 1;
		else
			levelModifier = 1 + (level / 10f);

		if (rarity == IGetStatModifier.Rarity.isLegendary) { return levelModifier += 0.8f; } //get rarity modifier
		if (rarity == IGetStatModifier.Rarity.isEpic) { return levelModifier += 0.4f; }
		if (rarity == IGetStatModifier.Rarity.isRare) { return levelModifier += 0.2f; }
		else { return levelModifier += 0; }
	}

	//convert % to ui % (0.08f = 8%)
	public static int ConvertFloatToUiPercentage(float value)
	{
		return Mathf.RoundToInt(value *= 100);
	}

	//check if current active scene == this scene name
	public static bool GetCurrentlyActiveScene(string sceneName)
	{
		Scene currentScene = SceneManager.GetActiveScene();
		if (currentScene.name == sceneName)
			return true;
		else
			return false;
	}
}
