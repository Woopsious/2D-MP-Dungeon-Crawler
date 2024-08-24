using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LootPoolScriptableObject", menuName = "LootPools")]
public class SOLootPools : ScriptableObject
{
	public List<SOItems> lootPoolList = new List<SOItems>();

	public int maxDroppedItemsAmount;
	public int minDroppedItemsAmount;
}
