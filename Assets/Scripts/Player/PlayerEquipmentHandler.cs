using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

public class PlayerEquipmentHandler : EntityEquipmentHandler
{
	PlayerController playerController;

	[Header("equipped Consumables")]
	public Consumables equippedConsumableOne;
	public Consumables equippedConsumableTwo;

	[Header("equipped Abilities")]
	public Consumables equippedAbilityOne;
	public Consumables equippedAbilityTwo;
	public Consumables equippedAbilityThree;
	public Consumables equippedAbilityFour;
	public Consumables equippedAbilityFive;

	public override void Start()
	{
		entityStats = gameObject.transform.parent.GetComponentInParent<EntityStats>();
		entityStats.playerEquipment = this;
		playerController = gameObject.transform.parent.GetComponentInParent<PlayerController>();
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

	public void EquipItem(InventoryItem item, InventorySlot slot)
	{
		if (item == null) // when player unequips equipment without swapping/replacing it
			HandleEmptySlots(slot);

		else if (item.itemType == InventoryItem.ItemType.isWeapon) //when player first equips/swaps equipment
		{
			if (item.weaponType == InventoryItem.WeaponType.isMainHand)
				EquipWeapon(item, equippedWeapon, weaponSlotContainer);
			else
				EquipWeapon(item, equippedOffhandWeapon, offhandWeaponSlotContainer);
		}
		else if (item.itemType == InventoryItem.ItemType.isArmor)
		{
			if (item.armorSlot == InventoryItem.ArmorSlot.helmet)
				EquipArmor(item, equippedHelmet, helmetSlotContainer);

			if (item.armorSlot == InventoryItem.ArmorSlot.chestpiece)
				EquipArmor(item, equippedChestpiece, chestpieceSlotContainer);

			if (item.armorSlot == InventoryItem.ArmorSlot.legs)
				EquipArmor(item, equippedLegs, legsSlotContainer);
		}
		else if (item.itemType == InventoryItem.ItemType.isAccessory)
		{
			if (item.accessorySlot == InventoryItem.AccessorySlot.necklace)
				EquipAccessory(item, equippedNecklace, necklaceSlotContainer);

			if (item.accessorySlot == InventoryItem.AccessorySlot.ring && slot.slotType == InventorySlot.SlotType.ringOne)
				EquipAccessory(item, equippedRingOne, ringOneSlotContainer);

			if (item.accessorySlot == InventoryItem.AccessorySlot.ring && slot.slotType == InventorySlot.SlotType.ringTwo)
				EquipAccessory(item, equippedRingTwo, ringTwoSlotContainer);
		}
	}

	public void EquipWeapon(InventoryItem weaponToEquip, Weapons equippedWeaponRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnWeaponUnequip(equippedWeaponRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedWeaponRef = go.AddComponent<Weapons>();

		equippedWeaponRef.weaponBaseRef = weaponToEquip.weaponBaseRef;
		equippedWeaponRef.SetItemStats((Items.Rarity)weaponToEquip.rarity, weaponToEquip.itemLevel, this);
		equippedWeaponRef.isEquippedByPlayer = true;

		equippedWeaponRef.GetComponent<SpriteRenderer>().enabled = false;
	}
	public void EquipArmor(InventoryItem armorToEquip, Armors equippedArmorRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnArmorUnequip(equippedArmorRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedArmorRef = go.AddComponent<Armors>();

		equippedArmorRef.armorBaseRef = armorToEquip.armorBaseRef;
		equippedArmorRef.SetItemStats((Items.Rarity)armorToEquip.rarity, armorToEquip.itemLevel, this);

		equippedArmorRef.GetComponent<SpriteRenderer>().enabled = false;
	}
	public void EquipAccessory(InventoryItem accessoryToEquip, Accessories equippedAccessoryRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnAccessoryUnequip(equippedAccessoryRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedAccessoryRef = go.AddComponent<Accessories>();

		equippedAccessoryRef.accessoryBaseRef = accessoryToEquip.accessoryBaseRef;
		equippedAccessoryRef.SetItemStats((Items.Rarity)accessoryToEquip.rarity, accessoryToEquip.itemLevel, this);

		equippedAccessoryRef.GetComponent<SpriteRenderer>().enabled = false;
	}

	public void HandleEmptySlots(InventorySlot slot)
	{
		if (slot.slotType == InventorySlot.SlotType.weaponMain)
		{
			OnWeaponUnequip(equippedWeapon);
			Destroy(equippedWeapon.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.weaponOffhand)
		{
			OnWeaponUnequip(equippedOffhandWeapon);
			Destroy(equippedOffhandWeapon.gameObject);
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
		if (slot.slotType == InventorySlot.SlotType.necklace)
		{
			OnAccessoryUnequip(equippedNecklace);
			Destroy(equippedNecklace.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.ringOne)
		{
			OnAccessoryUnequip(equippedRingOne);
			Destroy(equippedRingOne.gameObject);
		}
		if (slot.slotType == InventorySlot.SlotType.ringTwo)
		{
			OnAccessoryUnequip(equippedRingTwo);
			Destroy(equippedRingTwo.gameObject);
		}
	}
}
