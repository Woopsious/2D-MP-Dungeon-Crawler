using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;

public class Consumables : Items
{
	private void Start()
	{
		if (generateStatsOnStart)
			GenerateStatsOnStart();
	}

	//set consumable data
	public override void Initilize(Rarity setRarity, int setLevel)
	{
		base.Initilize(setRarity, setLevel);
		isStackable = consumableBaseRef.isStackable;
	}
	public override void SetToolTip(EntityStats playerStats)
	{
		base.SetToolTip(playerStats);
		toolTip = GetComponent<ToolTipUi>();

		string info = $"{itemName}\n{itemPrice} Price";

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
	private bool EntityHealthFull(EntityStats entityStats)
	{
		if (entityStats.currentHealth >= entityStats.maxHealth.finalValue)
			return true;
        else return false;
    }
	private bool EntityManaFull(EntityStats entityStats)
	{
		if (entityStats.currentMana >= entityStats.maxMana.finalValue)
			return true;
		else return false;
	}
}
