using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityEquipmentHandler : MonoBehaviour
{
	public GameObject itemPrefab;

	[HideInInspector] public EntityStats entityStats;
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

	public int physicalDamagePercentage;
	public int poisonDamagePercentage;
	public int fireDamagePercentage;
	public int iceDamagePercentage;

	private void Start()
	{
		Initilize();
	}

	public virtual void Initilize()
	{
		entityStats = GetComponentInParent<EntityStats>();
		entityStats.entityEquipment = this;
		isPlayerEquipment = false;
	}
	public IEnumerator SpawnEntityEquipment(int numOfTries)
	{
		if (numOfTries >= 5 || isPlayerEquipment)
			Debug.LogWarning("Unable to Spawn Entity Equipment");

		else if (entityStats == null)
		{
			numOfTries += 1;
			yield return new WaitForSeconds(0.1f);
			StartCoroutine(SpawnEntityEquipment(numOfTries));
		}
		else
		{
			EquipRandomItems();
		}
	}

	//for entitys other then player to randomly equip items when they spawn in
	public void EquipRandomItems()
	{
		EquipRandomWeapon(entityStats.entityBaseStats.possibleWeaponsList, equippedWeapon, weaponSlotContainer);			//main weapon
		//EquipRandomWeapon( NO LIST FOR OFFHAND WEAPONS ATM, equippedOffhandWeapon, offhandWeaponSlotContainer);			//offhand weapon

		EquipRandomArmor(entityStats.entityBaseStats.possibleHelmetsList, equippedHelmet, helmetSlotContainer);				//helmet
		EquipRandomArmor(entityStats.entityBaseStats.possibleChestpiecesList, equippedChestpiece, chestpieceSlotContainer); //chestpiece
		EquipRandomArmor(entityStats.entityBaseStats.possibleLegsList, equippedLegs, legsSlotContainer);                    //legs

		//Accessory functions here if/when i decide to add it
	}
	public void EquipRandomWeapon(List<SOWeapons> listOfPossibleWeapons, Weapons equippedWeaponRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		int index;
		index = Utilities.GetRandomNumber(listOfPossibleWeapons.Count);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedWeaponRef = go.AddComponent<Weapons>();

		equippedWeaponRef.weaponBaseRef = listOfPossibleWeapons[index];
		equippedWeaponRef.Initilize(Items.Rarity.isCommon, entityStats.entityLevel, this);
		equippedWeaponRef.isEquippedByOther = true;

		equippedWeaponRef.GetComponent<SpriteRenderer>().enabled = false;
	}
	public void EquipRandomArmor(List<SOArmors> listOfPossibleArmors, Armors equippedArmorRef, GameObject slotToSpawnIn)
	{
		GameObject go;
		int index;
		index = Utilities.GetRandomNumber(listOfPossibleArmors.Count);

		go = SpawnItemPrefab(slotToSpawnIn);
		equippedArmorRef = go.AddComponent<Armors>();

		equippedArmorRef.armorBaseRef = listOfPossibleArmors[index];
		equippedArmorRef.Initilize(Items.Rarity.isCommon, entityStats.entityLevel, this);

		equippedArmorRef.GetComponent<SpriteRenderer>().enabled = false;
	}

	//weapons
	public void OnWeaponUnequip(Weapons weapon)
	{
		if (weapon == null) return;

		if (weapon.isShield)	//shield is a unique so i use damage value to store bonus health and resists it adds
		{
			equipmentHealth -= weapon.damage;
			equipmentPhysicalResistance -= weapon.damage;
			equipmentPoisonResistance -= weapon.damage;
			equipmentFireResistance -= weapon.damage;
			equipmentIceResistance -= weapon.damage;
		}
		else
			equipmentMana -= weapon.bonusMana;
	}
	public void OnWeaponEquip(Weapons weapon, GameObject slotItemIsIn)
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

		AssignItemRefOnEquip(weapon, slotItemIsIn);
	}

	//armors
	public void OnArmorUnequip(Armors armor)
	{
		if (armor == null) return;

		equipmentHealth -= armor.bonusPhysicalResistance;
		equipmentMana -= armor.bonusMana;
		equipmentPhysicalResistance -= armor.bonusPhysicalResistance;
		equipmentPoisonResistance -= armor.bonusPoisonResistance;
		equipmentFireResistance -= armor.bonusFireResistance;
		equipmentIceResistance -= armor.bonusIceResistance;
	}
	public void OnArmorEquip(Armors armor, GameObject slotItemIsIn)
	{
		equipmentHealth += armor.bonusPhysicalResistance;
		equipmentMana += armor.bonusMana;
		equipmentPhysicalResistance += armor.bonusPhysicalResistance;
		equipmentPoisonResistance += armor.bonusPoisonResistance;
		equipmentFireResistance += armor.bonusFireResistance;
		equipmentIceResistance += armor.bonusIceResistance;

		AssignItemRefOnEquip(armor, slotItemIsIn);
	}

	//accessories
	public void OnAccessoryUnequip(Accessories accessory)
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
	}
	public void OnAccessoryEquip(Accessories accessory, GameObject slotItemIsIn)
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
	}
	public void ReapplyEquipmentStats(Items equippedItem)
	{
		if (equippedItem == null) return;
		equippedItem.Initilize(equippedItem.rarity, equippedItem.itemLevel, this);
	}

	public GameObject SpawnItemPrefab(GameObject slotToSpawnIn)
	{
		GameObject go;
		if (slotToSpawnIn.transform.childCount == 0)
		{
			go = Instantiate(itemPrefab, slotToSpawnIn.transform);
			return go;
		}
		else return slotToSpawnIn.transform.GetChild(0).gameObject;
	}
	public Items AssignItemRefOnEquip(Items itemToAssign, GameObject SlotItemIsIn)
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
