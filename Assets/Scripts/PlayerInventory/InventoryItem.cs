using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditorInternal.Profiling.Memory.Experimental;
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
		isConsumable, isWeapon, isArmor, isAccessory, isAbility
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
			Weapons weapon = GetComponent<Weapons>();
			info += "\n Level: " + itemLevel + "\n Rarity: " + rarity + "\n \n Damage: " + weapon.damage + "\n Range: ";
			if (weapon.weaponBaseRef.isRangedWeapon)
				info += weapon.weaponBaseRef.baseMaxAttackRange;
			else
				info += "Melee";
			info += "\n Attack Speed: " + weapon.weaponBaseRef.baseAttackSpeed + "\n Bonus Mana: " + weapon.bonusMana;
		}
		if (itemType == ItemType.isArmor)
		{
			Armors armor = GetComponent<Armors>();
			info += "\n Level: " + itemLevel + "\n Rarity: " + rarity + "\n \n Bonus Health: " + armor.bonusHealth + "\n Bonus Mana: " +
				armor.bonusMana + "\n \n Physical Resistance: " + armor.bonusPhysicalResistance + "\n Poison Resistance: " +
				armor.bonusPoisonResistance + "\n Fire Resistance: " + armor.bonusFireResistance + "\n Ice Resistance: " + 
				armor.bonusIceResistance;
		}
		if (itemType == ItemType.isConsumable)
		{
			Consumables consumable = GetComponent<Consumables>();
			if (consumable.consumableType == Consumables.ConsumableType.healthRestoration)
				info += "\n \n Restores: " + consumable.consumablePercentage + "% Health";
            else if (consumable.consumableType == Consumables.ConsumableType.manaRestoration)
				info += "\n \n Restores: " + consumable.consumablePercentage + "% Mana";
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
	public void UpdateUi()
	{
		gameObject.name = itemName;
		uiItemName.text = itemName;
		uiItemImage.sprite = itemImage;
		uiItemLevel.text = "LVL: " + itemLevel;
		UpdateStackCount();
	}
	public void UpdateStackCount()
	{
		uiItemStackCount.text = currentStackCount.ToString();
	}
	public void IncreaseStackCounter()
	{
		currentStackCount++;
		uiItemStackCount.text = currentStackCount.ToString();
	}
	public void DecreaseStackCounter()
	{
		currentStackCount--;
		uiItemStackCount.text = currentStackCount.ToString();

		if (currentStackCount <= 0)
			Destroy(gameObject);
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
