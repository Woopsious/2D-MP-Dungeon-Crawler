using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
	[Header("Debug settings")]
	public bool generateStatsOnStart;

	[Header("Item Info")]
	public string itemName;
	public Sprite itemImage;
	public int ItemPrice;
	public int ItemId;

	public int itemLevel;
	public Rarity rarity;
	public enum Rarity
	{
		isCommon, isRare, isEpic, isLegendary
	}

	[Header("Item Base Ref")]
	public SOWeapons weaponBaseRef;
	public SOArmors armorBaseRef;
	public SOAccessories accessoryBaseRef;
	public SOConsumables consumableBaseRef;

	[Header("Inventroy Dynamic Info")]
	public bool isStackable;
	public int currentStackCount;
	public float levelModifier;
	public int inventroySlot;

	public virtual void Initilize(Rarity setRarity, int setLevel, EntityEquipmentHandler equipmentHandler)
	{
		rarity = setRarity;
		itemLevel = setLevel;
		GetStatModifier(itemLevel, (IGetStatModifier.Rarity)rarity);

		itemName = GetItemName();
		name = itemName;
		itemImage = GetItemImage();
		ItemPrice = GetItemPrice();

		GetComponent<SpriteRenderer>().sprite = itemImage;
	}
	public void GetStatModifier(int level, IGetStatModifier.Rarity rarity)
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
	public string GetItemName()
	{
		if (weaponBaseRef != null) return weaponBaseRef.itemName;
		else if (armorBaseRef != null) return armorBaseRef.itemName;
		else if (accessoryBaseRef != null) return accessoryBaseRef.itemName;
		else return consumableBaseRef.itemName;
	}
	public Sprite GetItemImage()
	{
		if (weaponBaseRef != null) return weaponBaseRef.itemImage;
		else if (armorBaseRef != null) return armorBaseRef.itemImage;
		else if (accessoryBaseRef != null) return accessoryBaseRef.itemImage;
		else return consumableBaseRef.itemImage;
	}
	public int GetItemPrice()
	{
		if (weaponBaseRef != null) return (int)(weaponBaseRef.ItemPrice * levelModifier);
		else if (armorBaseRef != null) return (int)(armorBaseRef.ItemPrice * levelModifier);
		else if (accessoryBaseRef != null) return (int)(accessoryBaseRef.ItemPrice * levelModifier);
		else return (int)(consumableBaseRef.ItemPrice * levelModifier);
	}

	public virtual void OnMouseOverItem()
	{
		//display what item it is eg: item price and name and rarity
		//in child classes also display weapon stats if weapon or armor stats if armor
	}
	public virtual void PickUpItem()
	{
		if (PlayerInventoryManager.Instance.CheckIfInventoryFull())
		{
			Debug.LogWarning("Inventory is full");
			return;
		}

		PlayerInventoryManager.Instance.AddItemToPlayerInventory(this);
		Destroy(gameObject);
	}

	public void GenerateStatsOnStart()
	{
		Initilize(rarity, itemLevel, null);
		gameObject.AddComponent<Interactables>();
		BoxCollider2D collider2D = gameObject.AddComponent<BoxCollider2D>();
		collider2D.isTrigger = true;
	}
}
