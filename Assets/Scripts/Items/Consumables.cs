using System;
using System.Collections;
using UnityEngine;

public class Consumables : Items
{
	private void Start()
	{
		if (generateStatsOnStart)
			GenerateStatsOnStart();
	}

	//set consumable data
	public override void Initilize(Rarity setRarity, int setLevel, int setEnchantmentLevel)
	{
		base.Initilize(setRarity, setLevel, setEnchantmentLevel);
		isStackable = consumableBaseRef.isStackable;
	}
	public override void UpdateToolTip(EntityStats playerStats, bool itemInShopSlot)
	{
		string info = $"{itemName}\n{AdjustItemPriceDisplay(itemInShopSlot)} Price";
		string extraInfo;

		if (consumableBaseRef.consumableType == SOConsumables.ConsumableType.healthRestoration)
			extraInfo = $"Restores {Utilities.ConvertFloatToUiPercentage(consumableBaseRef.consumablePercentage)}% of max health";
		else
			extraInfo = $"Restores {Utilities.ConvertFloatToUiPercentage(consumableBaseRef.consumablePercentage)}% of max mana";

		toolTip.tipToShow = $"{info}\n{extraInfo}";
	}

	//consume item
	public void ConsumeItem(EntityStats entityStats)
	{
		if (consumableBaseRef.consumableType == SOConsumables.ConsumableType.healthRestoration && !EntityHealthFull(entityStats))
			entityStats.OnHeal(consumableBaseRef.consumablePercentage, true, entityStats.healingPercentageModifier.finalPercentageValue);
		else if (consumableBaseRef.consumableType == SOConsumables.ConsumableType.manaRestoration && !EntityManaFull(entityStats))
			entityStats.IncreaseMana(consumableBaseRef.consumablePercentage, true);
		else return;

		GetComponent<InventoryItemUi>().DecreaseStackCounter();
	}

	//bool checks
	public bool EntityHealthFull(EntityStats entityStats)
	{
		if (entityStats.currentHealth >= entityStats.maxHealth.finalValue)
			return true;
        else return false;
    }
	public bool EntityManaFull(EntityStats entityStats)
	{
		if (entityStats.currentMana >= entityStats.maxMana.finalValue)
			return true;
		else return false;
	}
}
