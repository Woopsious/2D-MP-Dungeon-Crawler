using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

public class PlayerEquipmentHandler : EntityEquipmentHandler
{
	[Header("equipped Consumables")]
	public Consumables equippedConsumableOne;
	public Consumables equippedConsumableTwo;

	[Header("equipped Abilities")]
	public Consumables equippedAbilityOne;
	public Consumables equippedAbilityTwo;
	public Consumables equippedAbilityThree;
	public Consumables equippedAbilityFour;
	public Consumables equippedAbilityFive;

	private void Start()
	{
		Initilize();
	}
	private void OnEnable()
	{
		InventorySlotUi.OnItemEquip += EquipItem;
	}
	private void OnDisable()
	{
		InventorySlotUi.OnItemEquip -= EquipItem;
	}

	public override void Initilize()
	{
		entityStats = GetComponent<EntityStats>();
		isPlayerEquipment = true;
	}

	private void EquipItem(InventoryItem item, InventorySlotUi slot)
	{
		if (item == null) // when player unequips equipment without swapping/replacing it
			HandleEmptySlots(slot);

		else if (item.itemType == InventoryItem.ItemType.isWeapon) //when player first equips/swaps equipment
		{
			Weapons weapon = item.GetComponent<Weapons>();
			if (weapon.weaponBaseRef.weaponType == SOWeapons.WeaponType.isMainHand)
				EquipWeapon(weapon, equippedWeapon, weaponSlotContainer);
			else
				EquipWeapon(weapon, equippedOffhandWeapon, offhandWeaponSlotContainer);
		}
		else if (item.itemType == InventoryItem.ItemType.isArmor)
		{
			Armors armor = item.GetComponent<Armors>();
			if (armor.armorSlot == Armors.ArmorSlot.helmet)
				EquipArmor(armor, equippedHelmet, helmetSlotContainer);

			if (armor.armorSlot == Armors.ArmorSlot.chestpiece)
				EquipArmor(armor, equippedChestpiece, chestpieceSlotContainer);

			if (armor.armorSlot == Armors.ArmorSlot.legs)
				EquipArmor(armor, equippedLegs, legsSlotContainer);
		}
		else if (item.itemType == InventoryItem.ItemType.isAccessory)
		{
			Accessories accessories = item.GetComponent<Accessories>();
			if (accessories.accessorySlot == Accessories.AccessorySlot.necklace)
				EquipAccessory(accessories, equippedNecklace, necklaceSlotContainer);

			if (accessories.accessorySlot == Accessories.AccessorySlot.ring && slot.slotType == InventorySlotUi.SlotType.ringOne)
				EquipAccessory(accessories, equippedRingOne, ringOneSlotContainer);

			if (accessories.accessorySlot == Accessories.AccessorySlot.ring && slot.slotType == InventorySlotUi.SlotType.ringTwo)
				EquipAccessory(accessories, equippedRingTwo, ringTwoSlotContainer);
		}
		else if (item.itemType == InventoryItem.ItemType.isConsumable)
		{
			EquipConsumables(item.GetComponent<Consumables>(), slot);
		}
	}

	//physically spawned in to player
	private void EquipWeapon(Weapons weaponToEquip, Weapons equippedWeaponRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnWeaponUnequip(equippedWeaponRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedWeaponRef = go.AddComponent<Weapons>();

		equippedWeaponRef.weaponBaseRef = weaponToEquip.weaponBaseRef;
		equippedWeaponRef.Initilize(weaponToEquip.rarity, weaponToEquip.itemLevel);
		equippedWeaponRef.isEquippedByPlayer = true;

		equippedWeaponRef.GetComponent<SpriteRenderer>().enabled = false;
		OnWeaponEquip(equippedWeaponRef, slotToSpawnIn);
	}
	private void EquipArmor(Armors armorToEquip, Armors equippedArmorRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnArmorUnequip(equippedArmorRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedArmorRef = go.AddComponent<Armors>();

		equippedArmorRef.armorBaseRef = armorToEquip.armorBaseRef;
		equippedArmorRef.Initilize(armorToEquip.rarity, armorToEquip.itemLevel);

		equippedArmorRef.GetComponent<SpriteRenderer>().enabled = false;
		OnArmorEquip(equippedArmorRef, slotToSpawnIn);
	}
	private void EquipAccessory(Accessories accessoryToEquip, Accessories equippedAccessoryRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnAccessoryUnequip(equippedAccessoryRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedAccessoryRef = go.AddComponent<Accessories>();

		equippedAccessoryRef.accessoryBaseRef = accessoryToEquip.accessoryBaseRef;
		equippedAccessoryRef.Initilize(accessoryToEquip.rarity, accessoryToEquip.itemLevel);

		equippedAccessoryRef.GetComponent<SpriteRenderer>().enabled = false;
		OnAccessoryEquip(equippedAccessoryRef, slotToSpawnIn);
	}

	//not physically spawned in
	private void EquipConsumables(Consumables consumableToEquip, InventorySlotUi slotEquippedTo)
	{
		if (slotEquippedTo.slotIndex == 0)
		{
			equippedConsumableOne = consumableToEquip;
		}
		else if (slotEquippedTo.slotIndex == 1)
		{
			equippedConsumableTwo = consumableToEquip;
		}
	}

	private void HandleEmptySlots(InventorySlotUi slot)
	{
		if (slot.slotType == InventorySlotUi.SlotType.weaponMain)
		{
			OnWeaponUnequip(equippedWeapon);
			Destroy(equippedWeapon.gameObject);
		}
		if (slot.slotType == InventorySlotUi.SlotType.weaponOffhand)
		{
			OnWeaponUnequip(equippedOffhandWeapon);
			Destroy(equippedOffhandWeapon.gameObject);
		}
		if (slot.slotType == InventorySlotUi.SlotType.helmet)
		{
			OnArmorUnequip(equippedHelmet);
			Destroy(equippedHelmet.gameObject);
		}
		if (slot.slotType == InventorySlotUi.SlotType.chestpiece)
		{
			OnArmorUnequip(equippedChestpiece);
			Destroy(equippedChestpiece.gameObject);
		}
		if (slot.slotType == InventorySlotUi.SlotType.legs)
		{
			OnArmorUnequip(equippedLegs);
			Destroy(equippedLegs.gameObject);
		}
		if (slot.slotType == InventorySlotUi.SlotType.necklace)
		{
			OnAccessoryUnequip(equippedNecklace);
			Destroy(equippedNecklace.gameObject);
		}
		if (slot.slotType == InventorySlotUi.SlotType.ringOne)
		{
			OnAccessoryUnequip(equippedRingOne);
			Destroy(equippedRingOne.gameObject);
		}
		if (slot.slotType == InventorySlotUi.SlotType.ringTwo)
		{
			OnAccessoryUnequip(equippedRingTwo);
			Destroy(equippedRingTwo.gameObject);
		}
		if (slot.slotType == InventorySlotUi.SlotType.consumables)
		{
			if (slot.slotIndex == 0)
				equippedConsumableOne = null;
			if (slot.slotIndex == 1)
				equippedConsumableTwo = null;
		}
	}
}
