using UnityEngine;

[CreateAssetMenu(fileName = "ItemsScriptableObject", menuName = "Items")]
public class SOItems : ScriptableObject
{
	[Header("Item Info")]
	public string itemName;
	public Sprite itemImage;
	public int ItemPrice;
	public int ItemId;

	[Header("Item Type")]
	public ItemType itemType;
	public enum ItemType
	{
		isConsumable, isWeapon, isArmor, isAccessory
	}
	public bool isEquipable;

	[Header("Is Inventory Stackable")]
	public bool isStackable;
	public int MaxStackCount;
}
