using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class EntityEquipmentHandler : MonoBehaviour
{
	public GameObject itemPrefab;

	[HideInInspector] public EntityStats entityStats;
	[HideInInspector] public EntityClassHandler entityClassHandler;
	[HideInInspector] public bool isPlayerEquipment;

	[Header("Weapon")]
	public GameObject weaponSlotContainer;
	public Weapons equippedWeapon;

	[Header("Offhand Weapon")]
	public GameObject offhandWeaponSlotContainer;
	public Weapons equippedOffhandWeapon;

	[Header("Armor")]
	public GameObject helmetSlotContainer;
	public GameObject chestpieceSlotContainer;
	public GameObject legsSlotContainer;

	public Armors equippedHelmet;
	public Armors equippedChestpiece;
	public Armors equippedLegs;

	[Header("Accessories")]
	public GameObject necklaceSlotContainer;
	public GameObject ringOneSlotContainer;
	public GameObject ringTwoSlotContainer;

	public Accessories equippedNecklace;
	public Accessories equippedRingOne;
	public Accessories equippedRingTwo;

	[Header("Bonuses Provided By Equipment")]
	public int equipmentHealth;
	public int equipmentMana;

	public int equipmentPhysicalResistance;
	public int equipmentPoisonResistance;
	public int equipmentFireResistance;
	public int equipmentIceResistance;

	public float physicalDamagePercentage;
	public float poisonDamagePercentage;
	public float fireDamagePercentage;
	public float iceDamagePercentage;

	public event Action<EntityEquipmentHandler> OnEquipmentChanges;

	private void Awake()
	{
		entityStats = GetComponentInParent<EntityStats>();
		entityClassHandler = GetComponentInParent<EntityClassHandler>();
		isPlayerEquipment = false;
	}

	//for entitys other then player to randomly equip items when they spawn in
	public void SpawnEntityEquipment()
	{
		if (entityStats.entityBaseStats.canUseEquipment)
		{
			EquipWeapon(entityClassHandler.currentEntityClass.startingWeapon, equippedWeapon, weaponSlotContainer);		//main weapon
			//EquipRandomWeapon( NO LIST FOR OFFHAND WEAPONS ATM, equippedOffhandWeapon, offhandWeaponSlotContainer);	//offhand weapon

			foreach (SOArmors armor in entityClassHandler.currentEntityClass.startingArmor)                             //armor
			{
				if (armor.armorSlot == SOArmors.ArmorSlot.helmet)
					EquipArmor(armor, equippedHelmet, helmetSlotContainer);
				if (armor.armorSlot == SOArmors.ArmorSlot.chest)
					EquipArmor(armor, equippedChestpiece, chestpieceSlotContainer);
				if (armor.armorSlot == SOArmors.ArmorSlot.legs)
					EquipArmor(armor, equippedLegs, legsSlotContainer);
			}
			//Accessory functions here if/when i decide to add it
		}
		else
			EquipWeapon(entityStats.entityBaseStats.UniqueAttackWeapon, equippedWeapon, weaponSlotContainer);
	}
	private void EquipWeapon(SOWeapons weaponToEquip, Weapons equippedWeaponRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnWeaponUnequip(equippedWeaponRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedWeaponRef = go.AddComponent<Weapons>();

		equippedWeaponRef.weaponBaseRef = weaponToEquip;
		equippedWeaponRef.Initilize(Utilities.SetRarity(), entityStats.entityLevel);
		equippedWeaponRef.AddPlayerRef(null);
		equippedWeaponRef.isEquippedByOther = true;

		equippedWeaponRef.GetComponent<SpriteRenderer>().enabled = false;
		OnWeaponEquip(equippedWeaponRef, slotToSpawnIn);
	}
	private void EquipArmor(SOArmors armorToEquip, Armors equippedArmorRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnArmorUnequip(equippedArmorRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedArmorRef = go.AddComponent<Armors>();

		equippedArmorRef.armorBaseRef = armorToEquip;
		equippedArmorRef.Initilize(Utilities.SetRarity(), entityStats.entityLevel);

		equippedArmorRef.GetComponent<SpriteRenderer>().enabled = false;
		OnArmorEquip(equippedArmorRef, slotToSpawnIn);
	}

	//weapons
	protected void OnWeaponUnequip(Weapons weapon)
	{
		if (weapon == null) return;

		if (weapon.isShield)	//shield is a unique use damage value to store bonus health and resists it adds
		{
			equipmentHealth -= weapon.damage;
			equipmentPhysicalResistance -= weapon.damage;
			equipmentPoisonResistance -= weapon.damage;
			equipmentFireResistance -= weapon.damage;
			equipmentIceResistance -= weapon.damage;
		}
		else
			equipmentMana -= weapon.bonusMana;

		entityStats.idleWeaponSprite.sprite = null;
		Destroy(weapon);
		OnEquipmentChanges?.Invoke(this);
	}
	protected void OnWeaponEquip(Weapons weapon, GameObject slotItemIsIn)
	{
		if (weapon.isShield)	//shield is a unique so i use damage value to store bonus health and resists it adds
		{
			equipmentHealth += weapon.damage;
			equipmentPhysicalResistance += weapon.damage;
			equipmentPoisonResistance += weapon.damage;
			equipmentFireResistance += weapon.damage;
			equipmentIceResistance += weapon.damage;
		}
		else
			equipmentMana += weapon.bonusMana;

		weapon.WeaponInitilization(entityStats.idleWeaponSprite);
		AssignItemRefOnEquip(weapon, slotItemIsIn);
		OnEquipmentChanges?.Invoke(this);
	}

	//armors
	protected void OnArmorUnequip(Armors armor)
	{
		if (armor == null) return;

		equipmentHealth -= armor.bonusHealth;
		equipmentMana -= armor.bonusMana;
		equipmentPhysicalResistance -= armor.bonusPhysicalResistance;
		equipmentPoisonResistance -= armor.bonusPoisonResistance;
		equipmentFireResistance -= armor.bonusFireResistance;
		equipmentIceResistance -= armor.bonusIceResistance;

		Destroy(armor);
		OnEquipmentChanges?.Invoke(this);
	}
	protected void OnArmorEquip(Armors armor, GameObject slotItemIsIn)
	{
		equipmentHealth += armor.bonusHealth;
		equipmentMana += armor.bonusMana;
		equipmentPhysicalResistance += armor.bonusPhysicalResistance;
		equipmentPoisonResistance += armor.bonusPoisonResistance;
		equipmentFireResistance += armor.bonusFireResistance;
		equipmentIceResistance += armor.bonusIceResistance;

		AssignItemRefOnEquip(armor, slotItemIsIn);
		OnEquipmentChanges?.Invoke(this);
	}

	//accessories
	protected void OnAccessoryUnequip(Accessories accessory)
	{
		if (accessory == null) return;

		equipmentHealth -= accessory.bonusHealth;
		equipmentMana -= accessory.bonusMana;
		equipmentPhysicalResistance -= accessory.bonusPhysicalResistance;
		equipmentPoisonResistance -= accessory.bonusPoisonResistance;
		equipmentFireResistance -= accessory.bonusFireResistance;
		equipmentIceResistance -= accessory.bonusIceResistance;

		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isPhysicalDamageType)
			physicalDamagePercentage -= accessory.bonusPercentageValue;
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isPoisonDamageType)
			poisonDamagePercentage -= accessory.bonusPercentageValue;
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isFireDamageType)
			fireDamagePercentage -= accessory.bonusPercentageValue;
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isIceDamageType)
			iceDamagePercentage -= accessory.bonusPercentageValue;

		Destroy(accessory);
		OnEquipmentChanges?.Invoke(this);
	}
	protected void OnAccessoryEquip(Accessories accessory, GameObject slotItemIsIn)
	{
		equipmentHealth += accessory.bonusHealth;
		equipmentMana += accessory.bonusMana;
		equipmentPhysicalResistance += accessory.bonusPhysicalResistance;
		equipmentPoisonResistance += accessory.bonusPoisonResistance;
		equipmentFireResistance += accessory.bonusFireResistance;
		equipmentIceResistance += accessory.bonusIceResistance;

		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isPhysicalDamageType)
			physicalDamagePercentage += accessory.bonusPercentageValue;
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isPoisonDamageType)
			poisonDamagePercentage += accessory.bonusPercentageValue;
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isFireDamageType)
			fireDamagePercentage += accessory.bonusPercentageValue;
		if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isIceDamageType)
			iceDamagePercentage += accessory.bonusPercentageValue;

		AssignItemRefOnEquip(accessory, slotItemIsIn);
		OnEquipmentChanges?.Invoke(this);
	}

	protected GameObject SpawnItemPrefab(GameObject slotToSpawnIn)
	{
		GameObject go;
		if (slotToSpawnIn.transform.childCount == 0)
		{
			go = Instantiate(itemPrefab, slotToSpawnIn.transform);
			return go;
		}
		else return slotToSpawnIn.transform.GetChild(0).gameObject;
	}
	private Items AssignItemRefOnEquip(Items itemToAssign, GameObject SlotItemIsIn)
	{
		if (SlotItemIsIn == weaponSlotContainer)
			equippedWeapon = (Weapons)itemToAssign;
		else if (SlotItemIsIn == offhandWeaponSlotContainer)
			equippedOffhandWeapon = (Weapons)itemToAssign;
		else if (SlotItemIsIn == helmetSlotContainer)
			equippedHelmet = (Armors)itemToAssign;
		else if (SlotItemIsIn == chestpieceSlotContainer)
			equippedChestpiece = (Armors)itemToAssign;
		else if (SlotItemIsIn == legsSlotContainer)
			equippedLegs = (Armors)itemToAssign;
		else if (SlotItemIsIn == necklaceSlotContainer)
			equippedNecklace = (Accessories)itemToAssign;
		else if (SlotItemIsIn == ringOneSlotContainer)
			equippedRingOne = (Accessories)itemToAssign;
		else if (SlotItemIsIn == ringTwoSlotContainer)
			equippedRingTwo = (Accessories)itemToAssign;
		else
			Debug.LogError("item doesnt match any equipment slot"); return null;
	}
}
