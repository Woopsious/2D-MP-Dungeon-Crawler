using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumables : Items
{
	[Header("Consumable Info")]
	public int healthRestoration;
	public int manaRestoration;

	public void Start()
	{
		if (generateStatsOnStart)
			SetItemStats(rarity, itemLevel);
	}

	public override void SetItemStats(Rarity setRarity, int setLevel)
	{
		base.SetItemStats(setRarity, setLevel);

		healthRestoration = consumableBaseRef.healthRestoration;
		manaRestoration = consumableBaseRef.manaRestoration;
		isStackable = consumableBaseRef.isStackable;
	}
}
