using UnityEngine;

[CreateAssetMenu(fileName = "ConsumablesScriptableObject", menuName = "Items/Consumables")]
public class SOConsumables : SOItems
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
}
