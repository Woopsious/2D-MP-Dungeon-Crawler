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

	public void ConsumeItem(EntityStats entityStats)
	{
		if (consumableBaseRef.consumableType == SOConsumables.ConsumableType.healthRestoration)
			entityStats.OnHeal(consumableBaseRef.consumablePercentage, true);
		else if (consumableBaseRef.consumableType == SOConsumables.ConsumableType.manaRestoration)
			entityStats.IncreaseMana(consumableBaseRef.consumablePercentage, true);

		GetComponent<InventoryItemUi>().DecreaseStackCounter();
	}
}
