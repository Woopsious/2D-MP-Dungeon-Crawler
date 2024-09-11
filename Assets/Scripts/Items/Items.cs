using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
	[Header("Debug settings")]
	public bool generateStatsOnStart;

	[Header("Item Base Ref")]
	public SOWeapons weaponBaseRef;
	public SOArmors armorBaseRef;
	public SOAccessories accessoryBaseRef;
	public SOConsumables consumableBaseRef;

	[Header("Item Info")]
	private AudioHandler audioHandler;
	protected ToolTipUi toolTip;
	public string itemName;
	public Sprite itemSprite;
	public int itemPrice;
	public int itemId;

	public ItemType itemType;
	public enum ItemType
	{
		isConsumable, isWeapon, isArmor, isAccessory, isAbility
	}

	public int itemLevel;
	public int itemEnchantmentLevel;
	public float levelModifier;

	public Rarity rarity;
	public enum Rarity
	{
		isCommon, isRare, isEpic, isLegendary
	}

	[Header("Inventory Dynamic Info")]
	public bool isStackable;
	public int currentStackCount;
	public int inventroySlot;

	//set item data
	public virtual void Initilize(Rarity setRarity, int setLevel, int setEnchantmentLevel)
	{
		audioHandler = GetComponent<AudioHandler>();
		rarity = setRarity;
		itemLevel = setLevel;
		itemEnchantmentLevel = setEnchantmentLevel;
		GetStatModifier(itemLevel, (IGetStatModifier.Rarity)rarity, setEnchantmentLevel);

		itemName = GetItemName();
		name = itemName;
		itemSprite = GetItemImage();
		itemPrice = GetItemPrice();
		itemType = GetItemType();

		if (GetComponent<InventoryItemUi>() != null) return; //is inventoryItem so doesnt need this ref
		GetComponent<SpriteRenderer>().sprite = itemSprite;
	}
	private void GetStatModifier(int level, IGetStatModifier.Rarity rarity, int enchantmentLevel)
	{
		if (level == 1)  //get level modifier, +10% per level
			levelModifier = 1;
		else
			levelModifier = 1 + (level / 10f);

		if (enchantmentLevel == 1) //get enchantment modifier, +5% per level
			levelModifier += 0.05f;
		if (enchantmentLevel == 2)
			levelModifier += 0.1f;
		if (enchantmentLevel == 3)
			levelModifier += 0.15f;

		if (rarity == IGetStatModifier.Rarity.isLegendary) { levelModifier += 0.8f; } //get rarity modifier, +20% x2 per level
		if (rarity == IGetStatModifier.Rarity.isEpic) { levelModifier += 0.4f; }
		if (rarity == IGetStatModifier.Rarity.isRare) { levelModifier += 0.2f; }
		else { levelModifier += 0; }
	}
	private string GetItemName()
	{
		if (weaponBaseRef != null) return weaponBaseRef.itemName;
		else if (armorBaseRef != null) return armorBaseRef.itemName;
		else if (accessoryBaseRef != null) return accessoryBaseRef.itemName;
		else return consumableBaseRef.itemName;
	}
	private Sprite GetItemImage()
	{
		if (weaponBaseRef != null) return weaponBaseRef.itemImage;
		else if (armorBaseRef != null) return armorBaseRef.itemImage;
		else if (accessoryBaseRef != null) return accessoryBaseRef.itemImage;
		else return consumableBaseRef.itemImage;
	}
	private int GetItemPrice()
	{
		if (weaponBaseRef != null) return (int)(weaponBaseRef.itemPrice * levelModifier);
		else if (armorBaseRef != null) return (int)(armorBaseRef.itemPrice * levelModifier);
		else if (accessoryBaseRef != null) return (int)(accessoryBaseRef.itemPrice * levelModifier);
		else return (int)(consumableBaseRef.itemPrice);
	}
	private ItemType GetItemType()
	{
		if (weaponBaseRef != null) return (ItemType)weaponBaseRef.itemType;
		else if (armorBaseRef != null) return (ItemType)armorBaseRef.itemType;
		else if (accessoryBaseRef != null) return (ItemType)accessoryBaseRef.itemType;
		else return (ItemType)consumableBaseRef.itemType;
	}
	public void SetCurrentStackCount(int count)
	{
		currentStackCount = count;
	}

	public virtual void OnMouseOverItem() //not implamented
	{
		//display what item it is eg: item price and name and rarity
		//in child classes also display weapon stats if weapon or armor stats if armor
	}

	//player item interactions (pick up dropped items from floor)
	public void Interact(PlayerController playerController)
	{
		PlayerInventoryHandler playerInventory = playerController.GetComponent<PlayerInventoryHandler>();
		if (playerInventory.CheckIfInventoryFull())
		{
			Debug.LogWarning("Inventory is full");
			return;
		}

		playerInventory.PickUpNewItem(this);
		Destroy(gameObject);
	}
	public void UnInteract(PlayerController playerController)
	{

	}
	protected virtual void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<PlayerController>() != null)
			Interact(other.GetComponent<PlayerController>());
	}

	//tool tip
	public virtual void SetToolTip(EntityStats playerStats)
	{

	}

	//Debug functions
	public void GenerateStatsOnStart()
	{
		Initilize(rarity, itemLevel, 0);
		BoxCollider2D collider2D = gameObject.AddComponent<BoxCollider2D>();
		collider2D.isTrigger = true;
	}
}
