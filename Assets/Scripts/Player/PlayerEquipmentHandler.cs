using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipmentHandler : EntityEquipmentHandler
{
	PlayerController playerController;

	public override void Start()
	{
		entityStats = gameObject.GetComponentInParent<EntityStats>();
		entityStats.playerEquipment = this;
		playerController = gameObject.GetComponentInParent<PlayerController>();
		playerController.playerEquipmentHandler = this;
		//when loading/saving game, once inventory is loaded then load/instantiate equipped items based on loaded inventory
	}
	private void OnEnable()
	{
		InventorySlot.onItemEquip += EquipItem;
	}

	private void OnDisable()
	{
		InventorySlot.onItemEquip -= EquipItem;
	}

	public virtual void EquipItem(InventoryItem item, InventorySlot slot)
	{
		if (item == null) // when player unequips equipment without swapping/replacing it
			HandleEmptySlots(slot);

		else if (item.itemType == InventoryItem.ItemType.isWeapon) //when player first equips/swaps equipment
		{
			EquipWeapon(item, equippedWeapon);
			equippedWeapon = weaponSlotContainer.GetComponentInChildren<Weapons>();// add this for armor pieces
		}
		else if (item.itemType == InventoryItem.ItemType.isArmor)
		{
			if (item.armorSlot == InventoryItem.ArmorSlot.helmet)
			{
				EquipArmor(item, equippedHelmet, helmetSlotContainer);
				equippedHelmet = helmetSlotContainer.GetComponentInChildren<Armors>();
			}

			if (item.armorSlot == InventoryItem.ArmorSlot.chestpiece)
			{
				EquipArmor(item, equippedChestpiece, chestpieceSlotContainer);
				equippedChestpiece = chestpieceSlotContainer.GetComponentInChildren<Armors>();
			}

			if (item.armorSlot == InventoryItem.ArmorSlot.legs)
			{
				EquipArmor(item, equippedLegs, legsSlotContainer);
				equippedLegs = legsSlotContainer.GetComponentInChildren<Armors>();
			}
		}
	}

	public void EquipWeapon(InventoryItem weaponToEquip, Weapons equippedWeaponRef)
	{
		GameObject go;
		OnWeaponUnequip(equippedWeaponRef);

		if (weaponSlotContainer.transform.childCount == 0)
		{
			go = Instantiate(itemPrefab, weaponSlotContainer.transform);
			go.AddComponent<Weapons>();
			equippedWeaponRef = go.GetComponent<Weapons>();
		}

		equippedWeaponRef.weaponBaseRef = weaponToEquip.weaponBaseRef;
		equippedWeaponRef.SetItemStats((Items.Rarity)weaponToEquip.rarity, weaponToEquip.itemLevel);
		equippedWeaponRef.isEquippedByPlayer = true;

		equippedWeapon = equippedWeaponRef;
		OnWeaponEquip(equippedWeaponRef);

		/*
		equippedWeaponRef.itemName = weaponToEquip.itemName;
		equippedWeaponRef.itemImage = weaponToEquip.itemImage.sprite;
		equippedWeaponRef.itemLevel = weaponToEquip.itemLevel;
		equippedWeaponRef.rarity = (Items.Rarity)weaponToEquip.rarity;

		equippedWeaponRef.weaponBaseRef = weaponToEquip.weaponBaseRef;
		equippedWeaponRef.damage = weaponToEquip.damage;
		equippedWeaponRef.bonusMana = weaponToEquip.bonusWeaponMana;
		*/
	}
	public void EquipArmor(InventoryItem armorToEquip, Armors equippedArmorRef, GameObject slot)
	{
		GameObject go;
		OnArmorUnequip(equippedArmorRef);

		if (slot.transform.childCount == 0)
		{
			go = Instantiate(itemPrefab, slot.transform);
			go.AddComponent<Armors>();
			equippedArmorRef = go.GetComponent<Armors>();
		}
		equippedArmorRef.armorBaseRef = armorToEquip.armorBaseRef;
		equippedArmorRef.SetItemStats((Items.Rarity)armorToEquip.rarity, armorToEquip.itemLevel);

		if (equippedArmorRef.armorBaseRef.armorSlot == SOArmors.ArmorSlot.helmet)
			equippedHelmet = equippedArmorRef;
		if (equippedArmorRef.armorBaseRef.armorSlot == SOArmors.ArmorSlot.chest)
			equippedChestpiece = equippedArmorRef;
		if (equippedArmorRef.armorBaseRef.armorSlot == SOArmors.ArmorSlot.legs)
			equippedLegs = equippedArmorRef;

		OnArmorEquip(equippedArmorRef);

		/*
		equippedArmorRef.itemName = armorToEquip.itemName;
		equippedArmorRef.itemImage = armorToEquip.itemImage.sprite;
		equippedArmorRef.itemLevel = armorToEquip.itemLevel;
		equippedArmorRef.rarity = (Items.Rarity)armorToEquip.rarity;
		equippedArmorRef.armorSlot = (Armors.ArmorSlot)armorToEquip.armorSlot;

		equippedArmorRef.armorBaseRef = armorToEquip.armorBaseRef;
		equippedArmorRef.bonusHealth = armorToEquip.bonusArmorHealth;
		equippedArmorRef.bonusMana = armorToEquip.bonusArmorMana;
		equippedArmorRef.bonusPhysicalResistance = armorToEquip.bonusPhysicalResistance;
		equippedArmorRef.bonusPoisonResistance = armorToEquip.bonusPoisonResistance;
		equippedArmorRef.bonusFireResistance = armorToEquip.bonusFireResistance;
		equippedArmorRef.bonusIceResistance = armorToEquip.bonusIceResistance;
		*/
	}

	public void HandleEmptySlots(InventorySlot slot)
	{
		if (slot.slotType == InventorySlot.SlotType.weaponMain)
		{
			OnWeaponUnequip(equippedWeapon);
			Destroy(equippedWeapon.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.helmet)
		{
			OnArmorUnequip(equippedHelmet);
			Destroy(equippedHelmet.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.chestpiece)
		{
			OnArmorUnequip(equippedChestpiece);
			Destroy(equippedChestpiece.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.legs)
		{
			OnArmorUnequip(equippedLegs);
			Destroy(equippedLegs.gameObject);
		}
	}
}
