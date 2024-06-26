using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour, IInteractable
{
	[Header("Debug settings")]
	public bool generateStatsOnStart;

	[Header("Item Base Ref")]
	public SOWeapons weaponBaseRef;
	public SOArmors armorBaseRef;
	public SOAccessories accessoryBaseRef;
	public SOConsumables consumableBaseRef;

	[Header("Item Info")]
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
	public float levelModifier;

	public Rarity rarity;
	public enum Rarity
	{
		isCommon, isRare, isEpic, isLegendary
	}

	[Header("Inventroy Dynamic Info")]
	public bool isStackable;
	public int currentStackCount;
	public int inventroySlot;

	public virtual void Initilize(Rarity setRarity, int setLevel)
	{
		rarity = setRarity;
		itemLevel = setLevel;
		GetStatModifier(itemLevel, (IGetStatModifier.Rarity)rarity);

		itemName = GetItemName();
		name = itemName;
		itemSprite = GetItemImage();
		itemPrice = GetItemPrice();
		itemType = GetItemType();

		if (GetComponent<InventoryItemUi>() != null) return; //is inventoryItem so doesnt need this ref
		GetComponent<SpriteRenderer>().sprite = itemSprite;
	}
	private void GetStatModifier(int level, IGetStatModifier.Rarity rarity)
	{
		if (level == 1)  //get level modifier
			levelModifier = 1;
		else
			levelModifier = 1 + (level / 10f);

		if (rarity == IGetStatModifier.Rarity.isLegendary) { levelModifier += 0.8f; } //get rarity modifier
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

	public virtual void OnMouseOverItem()
	{
		//display what item it is eg: item price and name and rarity
		//in child classes also display weapon stats if weapon or armor stats if armor
	}

	//item interactions
	public void Interact(PlayerController playerController)
	{
		PlayerInventoryManager playerInventory = playerController.GetComponent<PlayerInventoryManager>();
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
		Initilize(rarity, itemLevel);
		BoxCollider2D collider2D = gameObject.AddComponent<BoxCollider2D>();
		collider2D.isTrigger = true;
	}
}
