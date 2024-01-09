using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SOLootPools : MonoBehaviour
{
	public int droppedGoldAmount;

	public List<SOItems> lootPoolList = new List<SOItems>();

	public int minDroppedItemsAmount;
	public int maxDroppedItemsAmount;
}
