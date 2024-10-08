using UnityEngine;

[CreateAssetMenu(fileName = "ItemsScriptableObject", menuName = "Items")]
public class SOItems : ScriptableObject
{
	[Header("Item Info")]
	public string itemName;
	public Sprite itemImage;
	public int itemPrice;
	public int itemId;

	[Range(0f, 1f)]
	public float itemSpawnChance;

	[Header("Item Type")]
	public ItemType itemType;
	public enum ItemType
	{
		isConsumable, isWeapon, isArmor, isAccessory, isAbility
	}
	public bool isEquipable;

	[Header("Is Inventory Stackable")]
	public bool isStackable;
	public int MaxStackCount;

	[Header("Audio")]
	public AudioClip equipItemSfx;
}
