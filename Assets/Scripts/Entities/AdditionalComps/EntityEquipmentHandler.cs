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

	//equip items to non players based on class
	public void SpawnEntityEquipment()
	{
		if (!entityStats.statsRef.canUseEquipment) //equip unique weapon
		{
			EquipWeapon(entityStats.statsRef.UniqueAttackWeapon, equippedWeapon, weaponSlotContainer);
			return;
		}

		EquipWeapon(entityClassHandler.currentEntityClass.startingWeapon[Utilities.GetRandomNumber
			(entityClassHandler.currentEntityClass.startingWeapon.Count - 1)], equippedWeapon, weaponSlotContainer);
		//EquipRandomWeapon( NO LIST FOR OFFHAND WEAPONS ATM, equippedOffhandWeapon, offhandWeaponSlotContainer);

		foreach (SOArmors armor in entityClassHandler.currentEntityClass.startingArmor)
		{
			if (armor.armorSlot == SOArmors.ArmorSlot.helmet)
				EquipArmor(armor, equippedHelmet, helmetSlotContainer);
			else if(armor.armorSlot == SOArmors.ArmorSlot.chest)
				EquipArmor(armor, equippedChestpiece, chestpieceSlotContainer);
			else if(armor.armorSlot == SOArmors.ArmorSlot.legs)
				EquipArmor(armor, equippedLegs, legsSlotContainer);
		}
		//Accessory functions here if/when i decide to add it
	}

	//equip items visibly to player sprite (armor currently not supported visualy)
	private void EquipWeapon(SOWeapons weaponToEquip, Weapons equippedWeaponRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		OnWeaponUnequip(equippedWeaponRef);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedWeaponRef = go.AddComponent<Weapons>();

		equippedWeaponRef.weaponBaseRef = weaponToEquip;
		equippedWeaponRef.Initilize(Utilities.SetRarity(0), entityStats.entityLevel, 0);

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
		equippedArmorRef.Initilize(Utilities.SetRarity(0), entityStats.entityLevel, 0);

		equippedArmorRef.GetComponent<SpriteRenderer>().enabled = false;
		OnArmorEquip(equippedArmorRef, slotToSpawnIn);
	}

	//equipment changes events
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

		entityStats.IdleWeaponSprite.sprite = null;
		Destroy(weapon.gameObject);
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

		weapon.WeaponInitilization(entityStats);
		AssignItemRefOnEquip(weapon, slotItemIsIn);
		OnEquipmentChanges?.Invoke(this);
	}

	protected void OnArmorUnequip(Armors armor)
	{
		if (armor == null) return;

		equipmentHealth -= armor.bonusHealth;
		equipmentMana -= armor.bonusMana;
		equipmentPhysicalResistance -= armor.bonusPhysicalResistance;
		equipmentPoisonResistance -= armor.bonusPoisonResistance;
		equipmentFireResistance -= armor.bonusFireResistance;
		equipmentIceResistance -= armor.bonusIceResistance;

		Destroy(armor.gameObject);
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
		else if(accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isPoisonDamageType)
			poisonDamagePercentage -= accessory.bonusPercentageValue;
		else if(accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isFireDamageType)
			fireDamagePercentage -= accessory.bonusPercentageValue;
		else if(accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isIceDamageType)
			iceDamagePercentage -= accessory.bonusPercentageValue;

		Destroy(accessory.gameObject);
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
		else if (accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isPoisonDamageType)
			poisonDamagePercentage += accessory.bonusPercentageValue;
		else if(accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isFireDamageType)
			fireDamagePercentage += accessory.bonusPercentageValue;
		else if(accessory.damageTypeToBoost == Accessories.DamageTypeToBoost.isIceDamageType)
			iceDamagePercentage += accessory.bonusPercentageValue;

		AssignItemRefOnEquip(accessory, slotItemIsIn);
		OnEquipmentChanges?.Invoke(this);
	}

	//physically spawned on entites
	protected GameObject SpawnItemPrefab(GameObject slotToSpawnIn)
	{
		GameObject go = Instantiate(itemPrefab, slotToSpawnIn.transform);
		return go;
	}
	private void AssignItemRefOnEquip(Items itemToAssign, GameObject SlotItemIsIn)
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
			Debug.LogError("item doesnt match any equipment slot");
	}
}
