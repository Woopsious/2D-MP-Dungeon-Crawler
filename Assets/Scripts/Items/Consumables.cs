using System.Collections;
using System.Collections.Generic;
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
	}

	public void ConsumeItem(PlayerController player)
	{
		if (consumableType == ConsumableType.healthRestoration)
		{
			player.GetComponent<EntityStats>().OnHeal(consumablePercentage, true);
		}
		else if (consumableType == ConsumableType.manaRestoration)
		{
			player.GetComponent<EntityStats>().IncreaseMana(consumablePercentage, true);
		}

		GetComponent<InventoryItem>().DecreaseStackCounter();
	}
}
