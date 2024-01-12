using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LootPoolScriptableObject", menuName = "LootPools")]
public class SOLootPools : ScriptableObject
{
	public int droppedGoldAmount;

	public List<SOItems> lootPoolList = new List<SOItems>();

	public int minDroppedItemsAmount;
	public int maxDroppedItemsAmount;
}
