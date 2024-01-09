using UnityEngine;

[CreateAssetMenu(fileName = "ConsumablesScriptableObject", menuName = "Items/Consumables")]
public class SOConsumables : SOItems
{
	[Header("Consumable Info")]
	public int healthRestoration;
	public int manaRestoration;
}
