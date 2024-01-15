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
		isConsumable, isWeapon, isArmor
	}
	public Rarity rarity;
	public enum Rarity
	{
		isCommon, isRare, isLegendary
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
	public int damage;
	public int bonusWeaponMana;

	[Header("Weapon Type")]
	public WeaponType weaponType;
	public enum WeaponType
	{
		isMainHand, isOffhand, isBoth
	}

	[Header("Armor Info")]
	public int bonusArmorHealth;
	public int bonusArmorMana;

	[Header("Armor Slot")]
	public SOArmors armorBaseRef;
	public ArmorSlot armorSlot;
	public enum ArmorSlot
	{
		helmet, chestpiece, legs
	}

	[Header("Armor Resistances")]
	public int bonusPhysicalResistance;
	public int bonusPoisonResistance;
	public int bonusFireResistance;
	public int bonusIceResistance;

	[Header("Consumable Base Ref")]
	public SOConsumables consumableBaseRef;
	public int healthRestoration;
	public int manaRestoration;

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
			info += "\n Attack Speed: " + weaponBaseRef.baseAttackSpeed + "\n Bonus Mana: " + bonusWeaponMana;
		}
		if (itemType == ItemType.isArmor)
		{
			info += "\n Level: " + itemLevel + "\n Rarity: " + rarity + "\n \n Bonus Health: " + bonusArmorHealth + "\n Bonus Mana: " +
				bonusArmorMana + "\n \n Physical Resistance: " + bonusPhysicalResistance + "\n Poison Resistance: " +
				bonusPoisonResistance + "\n Fire Resistance: " + bonusFireResistance + "\n Ice Resistance: " + bonusIceResistance;
		}
		if (itemType == ItemType.isConsumable)
		{
			info += "\n \n Restores: " + healthRestoration + "% Health \n Restores: " + manaRestoration + "% Mana";
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
	public void UpdateStackCounter()
	{
		uiItemStackCount.text = currentStackCount.ToString();
		if (currentStackCount <= 0) Destroy(gameObject);
	}
}
