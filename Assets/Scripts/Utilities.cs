using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Utilities
{
	//generate player death message
	public static string GetPlayerDeathMessage(DamageSourceInfo damageSourceInfo)
	{
		string deathMessage = string.Empty;

		if (damageSourceInfo.deathMessageType == DamageSourceInfo.DeathMessageType.entityWeapon)
		{
			if (damageSourceInfo.weapon.isRangedWeapon)
				deathMessage = $"Died by arrow from {damageSourceInfo.entity.statsRef.entityName}'s {damageSourceInfo.weapon.itemName}";
			else
				deathMessage = $"Died from swing of {damageSourceInfo.entity.statsRef.entityName}'s {damageSourceInfo.weapon.itemName}";
		}
		else if (damageSourceInfo.deathMessageType == DamageSourceInfo.DeathMessageType.entityAbility)
		{
			deathMessage = $"Died from {damageSourceInfo.entity.statsRef.entityName}'s {damageSourceInfo.ability.Name} ability";
		}
		else if (damageSourceInfo.deathMessageType == DamageSourceInfo.DeathMessageType.trap)
		{
			deathMessage = $"Died from {damageSourceInfo.trap.trapName}";
		}
		else if (damageSourceInfo.deathMessageType == DamageSourceInfo.DeathMessageType.statusEffect)
		{
			if (damageSourceInfo.statusEffect.damageType == IDamagable.DamageType.isPhysicalDamage)
				deathMessage = $"Died from {damageSourceInfo.statusEffect.Name} out";
			else if (damageSourceInfo.statusEffect.damageType == IDamagable.DamageType.isPoisonDamage)
				deathMessage = $"Died from being {damageSourceInfo.statusEffect.Name}";
			else if (damageSourceInfo.statusEffect.damageType == IDamagable.DamageType.isFireDamage)
				deathMessage = $"Died from {damageSourceInfo.statusEffect.Name} to death";
			else if (damageSourceInfo.statusEffect.damageType == IDamagable.DamageType.isIceDamage)
				deathMessage = $"Died from Freezing internally";
		}

		return deathMessage;
	}

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
	public static Items.Rarity SetRarity(float rarityChanceModifier)
	{
		float percentage = GetRandomNumber(100); //0.5% for legendary | 5% for epic | 10% for rare | 84.5% for common (Normal Difficulty)

		if (GameManager.Instance != null && GameManager.Instance.currentDungeonData != null)
		{
			if (GameManager.Instance.currentDungeonData.dungeonStatModifiers.difficultyModifier == 0.25f)
				return HellDifficultyRarity(percentage, rarityChanceModifier);
			else if (GameManager.Instance.currentDungeonData.dungeonStatModifiers.difficultyModifier == 0.1f)
				return HardDifficultyRarity(percentage, rarityChanceModifier);
			else
				return NormalDifficultyRarity(percentage, rarityChanceModifier);
		}
		else
		{
			Debug.LogWarning("GameManager Instance not found for setting item rarity, " +
				"defaulting to NormalDifficultyRarity, if testing ignore");
			return NormalDifficultyRarity(percentage, rarityChanceModifier);
		}
	}
	//modify chances based on dungeon difficulty
	private static Items.Rarity HellDifficultyRarity(float percentage, float rarityChanceModifier)
	{
		if (percentage >= 97.5 - rarityChanceModifier) //2.5%
			return Items.Rarity.isLegendary;
		else if (percentage >= 87.5 - rarityChanceModifier && percentage < 97.5 - rarityChanceModifier) //10%
			return Items.Rarity.isEpic;
		else if (percentage >= 67.5 - rarityChanceModifier && percentage < 87.5 - rarityChanceModifier) //20%
			return Items.Rarity.isRare;
		else
			return Items.Rarity.isCommon;
	}
	private static Items.Rarity HardDifficultyRarity(float percentage, float rarityChanceModifier)
	{
		if (percentage >= 99 - rarityChanceModifier) //1%
			return Items.Rarity.isLegendary;
		else if (percentage >= 91.5 - rarityChanceModifier && percentage < 99 - rarityChanceModifier) //7.5%
			return Items.Rarity.isEpic;
		else if (percentage >= 76.5 - rarityChanceModifier && percentage < 91.5 - rarityChanceModifier) //15%
			return Items.Rarity.isRare;
		else
			return Items.Rarity.isCommon;
	}
	private static Items.Rarity NormalDifficultyRarity(float percentage, float rarityChanceModifier)
	{
		if (percentage >= 99.5 - rarityChanceModifier) //0.5%
			return Items.Rarity.isLegendary;
		else if (percentage >= 94.5 - rarityChanceModifier && percentage < 99.5 - rarityChanceModifier) //5%
			return Items.Rarity.isEpic;
		else if (percentage >= 84.5 - rarityChanceModifier && percentage < 94.5 - rarityChanceModifier) //10%
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
