using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
	[HideInInspector] public Transform parentAfterDrag;

	public TMP_Text uiItemName;
	public Image uiItemImage;
	public TMP_Text uiItemLevel;
	public TMP_Text uiItemStackCount;
	private float timeToWait = 0.5f;

	public int inventorySlotIndex;

	[Header("Item Info")]
	public string itemName;
	public Sprite itemImage;
	public int itemPrice;
	public int itemLevel;
	public ItemType itemType;
	public enum ItemType
	{
		isConsumable, isWeapon, isArmor, isAccessory
	}
	public Rarity rarity;
	public enum Rarity
	{
		isCommon, isRare, isEpic, isLegendary
	}

	[Header("Item Dynamic Info")]
	public bool isStackable;
	public int maxStackCount;
	public int currentStackCount;
	public int inventroySlot;

	[Header("Class Restriction")]
	public ClassRestriction classRestriction;
	public enum ClassRestriction
	{
		light, medium, heavy
	}

	[Header("Weapon Info")]
	public SOWeapons weaponBaseRef;
	public bool isShield;
	public int damage;

	[Header("Weapon Type")]
	public WeaponType weaponType;
	public enum WeaponType
	{
		isMainHand, isOffhand, isBoth
	}

	[Header("Armor Info")]
	public SOArmors armorBaseRef;
	[Header("Armor Slot")]
	public ArmorSlot armorSlot;
	public enum ArmorSlot
	{
		helmet, chestpiece, legs
	}

	[Header("Accessory Info")]
	public SOAccessories accessoryBaseRef;
	[Header("Accessory Slot")]
	public AccessorySlot accessorySlot;
	public enum AccessorySlot
	{
		necklace, ring
	}

	[Header("Accessory Type")]
	public AccessoryType accessoryType;
	public enum AccessoryType
	{
		isWarding, isDamaging, isHealing
	}

	[Header("Accessory Damaging/healing")]
	public int bonusDamagePercentageValue;

	public DamageTypeToBoost damageTypeToBoost;
	public enum DamageTypeToBoost
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	[Header("Shared Bonus Stats")]
	public int bonusHealth;
	public int bonusMana;

	public int bonusPhysicalResistance;
	public int bonusPoisonResistance;
	public int bonusFireResistance;
	public int bonusIceResistance;

	[Header("Consumable Info")]
	public SOConsumables consumableBaseRef;
	[Header("Consumable Type")]
	public ConsumableType consumableType;
	public enum ConsumableType
	{
		healthRestoration, manaRestoration
	}

	[Header("Percentage Value")]
	public int consumablePercentage;

	//functions to display item info on mouse hover
	public void OnPointerEnter(PointerEventData eventData)
	{
		StopAllCoroutines();
		StartCoroutine(StartTimer());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		StopAllCoroutines();
		//HoverTipManager.OnMouseLoseFocus();
	}

	private void ShowMessage()
	{
		string info = itemName + "\n Sell Price: " + itemPrice / 25;

		if (itemType == ItemType.isWeapon)
		{
			info += "\n Level: " + itemLevel + "\n Rarity: " + rarity + "\n \n Damage: " + damage + "\n Range: ";
			if (weaponBaseRef.isRangedWeapon)
				info += weaponBaseRef.baseMaxAttackRange;
			else
				info += "Melee";
			info += "\n Attack Speed: " + weaponBaseRef.baseAttackSpeed + "\n Bonus Mana: " + bonusMana;
		}
		if (itemType == ItemType.isArmor)
		{
			info += "\n Level: " + itemLevel + "\n Rarity: " + rarity + "\n \n Bonus Health: " + bonusHealth + "\n Bonus Mana: " +
				bonusMana + "\n \n Physical Resistance: " + bonusPhysicalResistance + "\n Poison Resistance: " +
				bonusPoisonResistance + "\n Fire Resistance: " + bonusFireResistance + "\n Ice Resistance: " + bonusIceResistance;
		}
		if (itemType == ItemType.isConsumable)
		{
			if (consumableType == ConsumableType.healthRestoration)
				info += "\n \n Restores: " + consumablePercentage + "% Health";
            else if (consumableType == ConsumableType.manaRestoration)
				info += "\n \n Restores: " + consumablePercentage + "% Mana";
		}

		//HoverTipManager.OnMouseHover(info, Input.mousePosition);
	}
	public IEnumerator StartTimer()
	{
		yield return new WaitForSeconds(timeToWait);
		ShowMessage();
	}

	//functions for dragging
	public void OnBeginDrag(PointerEventData eventData)
	{
		parentAfterDrag = transform.parent;
		transform.SetParent(transform.root);
		transform.SetAsLastSibling();
		uiItemImage.raycastTarget = false;
	}
	public void OnDrag(PointerEventData eventData)
	{
		transform.position = Input.mousePosition;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		transform.SetParent(parentAfterDrag);
		uiItemImage.raycastTarget = true;
	}

	//update data
	public void UpdateName()
	{
		gameObject.name = itemName;
		uiItemName.text = itemName;
	}
	public void UpdateImage()
	{
		uiItemImage.sprite = itemImage;
	}
	public void UpdateItemLevel()
	{
		uiItemLevel.text = "LVL: " + itemLevel;
	}
	public void UpdateStackCounter()
	{
		uiItemStackCount.text = currentStackCount.ToString();
		if (currentStackCount <= 0) Destroy(gameObject);
	}

	//update Ui color based on rarity
	public void SetTextColour()
	{
		if (rarity == InventoryItem.Rarity.isRare)
			SetColour(Color.cyan);
		else if (rarity == InventoryItem.Rarity.isEpic)
			SetColour(Color.magenta);
		else if (rarity == InventoryItem.Rarity.isLegendary)
			SetColour(Color.red);
		else
			SetColour(Color.white);
	}
	public void SetColour(Color colour)
	{
		uiItemName.color = colour;
		uiItemLevel.color = colour;
		uiItemStackCount.color = colour;
	}
}
