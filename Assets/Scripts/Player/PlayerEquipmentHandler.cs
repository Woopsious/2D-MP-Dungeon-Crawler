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

	private void Start()
	{
		//when loading/saving game, once inventory is loaded then load/instantiate equipped items based on loaded inventory
		Initilize();
	}
	private void OnEnable()
	{
		InventorySlot.OnItemEquip += EquipItem;
	}
	private void OnDisable()
	{
		InventorySlot.OnItemEquip -= EquipItem;
	}

	public override void Initilize()
	{
		entityStats = gameObject.transform.parent.GetComponentInParent<EntityStats>();
		entityStats.entityEquipment = this;
		playerController = gameObject.transform.parent.GetComponentInParent<PlayerController>();
		playerController.playerEquipmentHandler = this;
		isPlayerEquipment = true;
	}

	private void EquipItem(InventoryItem item, InventorySlot slot)
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

			if (accessories.accessorySlot == Accessories.AccessorySlot.ring && slot.slotType == InventorySlot.SlotType.ringOne)
				EquipAccessory(accessories, equippedRingOne, ringOneSlotContainer);

			if (accessories.accessorySlot == Accessories.AccessorySlot.ring && slot.slotType == InventorySlot.SlotType.ringTwo)
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
		equippedWeaponRef.Initilize(weaponToEquip.rarity, weaponToEquip.itemLevel, this);
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
		equippedArmorRef.Initilize(armorToEquip.rarity, armorToEquip.itemLevel, this);

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
		equippedAccessoryRef.Initilize(accessoryToEquip.rarity, accessoryToEquip.itemLevel, this);

		equippedAccessoryRef.GetComponent<SpriteRenderer>().enabled = false;
		OnAccessoryEquip(equippedAccessoryRef, slotToSpawnIn);
	}

	//not physically spawned in
	private void EquipConsumables(Consumables consumableToEquip, InventorySlot slotEquippedTo)
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

	private void HandleEmptySlots(InventorySlot slot)
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
		if (slot.slotType == InventorySlot.SlotType.consumables)
		{
			if (slot.slotIndex == 0)
				equippedConsumableOne = null;
			if (slot.slotIndex == 1)
				equippedConsumableTwo = null;
		}
		entityStats.CalculateStats();
	}
}
