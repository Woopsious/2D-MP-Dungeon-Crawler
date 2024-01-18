using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Utilities
{
	//return random number
	public static int GetRandomNumber(int num)
	{
		return Random.Range(0, num);
	}
	public static Vector3 GetRandomPointInBounds(Bounds bounds)
	{
		return new Vector3(
			Random.Range(bounds.min.x, bounds.max.x),
			Random.Range(bounds.min.y, bounds.max.y),
			Random.Range(bounds.min.z, bounds.max.z));
	}

	//return random rarity
	public static Items.Rarity SetRarity()
	{
		float percentage = GetRandomNumber(101); //3% for legendary | 10% for epic | 35% for rare | 57% for common

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
		int itemLvl = GetRandomNumber(9);
		itemLvl += entityLvl;

		if (itemLvl <= 4) //stop items from dropping below lvl 0
			itemLvl = 5;
		return itemLvl - 4;
	}

	//for item spawning
}
