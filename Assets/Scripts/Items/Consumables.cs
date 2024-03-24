using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;

public class Consumables : Items
{
	[Header("Consumable Info")]
	[Header("Consumable Type")]
	public ConsumableType consumableType;
	public enum ConsumableType
	{
		healthRestoration, manaRestoration
	}

	[Header("Percentage Value")]
	public int consumablePercentage;

	private void Start()
	{
		if (generateStatsOnStart)
			GenerateStatsOnStart();
	}

	public override void Initilize(Rarity setRarity, int setLevel)
	{
		base.Initilize(setRarity, setLevel);

		consumableType = (ConsumableType)consumableBaseRef.consumableType;
		consumablePercentage = consumableBaseRef.consumablePercentage;
		isStackable = consumableBaseRef.isStackable;

		SetToolTip();
	}
	protected override void SetToolTip()
	{
		toolTip = GetComponent<ToolTipUi>();

		string info = $"{itemName} \n {itemPrice} Price";

		toolTip.tipToShow = info;
	}

	public void ConsumeItem(EntityStats entityStats)
	{
		if (consumableType == ConsumableType.healthRestoration)
			entityStats.OnHeal(consumablePercentage, true);
		else if (consumableType == ConsumableType.manaRestoration)
			entityStats.IncreaseMana(consumablePercentage, true);

		GetComponent<InventoryItemUi>().DecreaseStackCounter();
	}
}
