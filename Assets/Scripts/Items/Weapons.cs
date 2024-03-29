using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Weapons : Items
{
	[Header("Weapon Info")]
	public bool isShield;
	public int damage;
	public int bonusMana;
	public bool isEquippedByPlayer;
	public bool isEquippedByOther;

	public bool canAttackAgain;
	private GameObject parentObj;
	private SpriteRenderer attackWeaponSprite;
	private SpriteRenderer idleWeaponSprite;
	private Animator animator;
	private BoxCollider2D boxCollider;

	public void Start()
	{
		if (generateStatsOnStart)
			GenerateStatsOnStart();

		WeaponInitilization();
	}

	public override void Initilize(Rarity setRarity, int setLevel)
	{
		base.Initilize(setRarity, setLevel);

		isShield = weaponBaseRef.isShield;
		damage = (int)(weaponBaseRef.baseDamage * levelModifier);
		bonusMana = (int)(weaponBaseRef.baseBonusMana * levelModifier);
		isStackable = weaponBaseRef.isStackable;
	}
	public override void SetToolTip(EntityStats playerStats)
	{
		base.SetToolTip(playerStats);
		toolTip = GetComponent<ToolTipUi>();

		damage = (int)(weaponBaseRef.baseDamage * levelModifier);

		//apply offhand weapon dmg to main weapon (atm only useful for dagger)
		if (playerStats.equipmentHandler != null && playerStats.equipmentHandler.equippedOffhandWeapon != null)
			damage += playerStats.equipmentHandler.equippedOffhandWeapon.damage;

		damage = (int)(damage * GetWeaponDamageModifier(playerStats));

		string rarity;
		if (this.rarity == Rarity.isLegendary)
			rarity = "Legendary";
		else if (this.rarity == Rarity.isEpic)
			rarity = "Epic";
		else if (this.rarity == Rarity.isRare)
			rarity = "Rare";
		else
			rarity = "Common";
		string info = $"{rarity} Level {itemLevel} {itemName} \n {itemPrice} Price";

		if (weaponBaseRef.weaponType == SOWeapons.WeaponType.isMainHand)
			info += "\n Main hand ";
		else if (weaponBaseRef.weaponType == SOWeapons.WeaponType.isOffhand)
			info += "\n Offhand ";
		else if (weaponBaseRef.weaponType == SOWeapons.WeaponType.isBoth)
			info += "\n Dual hand ";

		if (weaponBaseRef.isRangedWeapon)
			info += "ranged weapon";
		else
			info += "melee weapon";

		string dps;
		if (isShield)
			dps = $"Blocks {damage}";
		else
		{
			int newDps = (int)(damage / weaponBaseRef.baseAttackSpeed);
			dps = $"{newDps} Dps";
		}
		string damageInfo = $"{dps}\n{damage} Damage\n" +
			$"{weaponBaseRef.baseAttackSpeed}s Attack speed\n{weaponBaseRef.baseKnockback} Knockback\n{bonusMana} Mana bonus";

		toolTip.tipToShow = $"{info}\n{damageInfo}";
	}
	public void UpdateWeaponDamage(EntityStats playerStats, Weapons offHandWeapon)
	{
		damage = (int)(weaponBaseRef.baseDamage * levelModifier);
		if (playerStats.equipmentHandler != null && offHandWeapon != null)
			damage += offHandWeapon.damage;

		if (offHandWeapon != null) //apply offhand weapon dmg to main weapon (atm only useful for dagger)
			damage += offHandWeapon.damage;

		damage = (int)(damage * GetWeaponDamageModifier(playerStats));
	}
	public float GetWeaponDamageModifier(EntityStats playerStats)
	{
		float percentageMod = 0;

		if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isPhysicalDamageType) //apply damage type mod
			percentageMod = playerStats.physicalDamagePercentageModifier.finalPercentageValue;
		else if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isPoisonDamageType)
			percentageMod = playerStats.poisonDamagePercentageModifier.finalPercentageValue;
		else if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isFireDamageType)
			percentageMod = playerStats.fireDamagePercentageModifier.finalPercentageValue;
		else if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isIceDamageType)
			percentageMod = playerStats.iceDamagePercentageModifier.finalPercentageValue;

		if (weaponBaseRef.weaponType == SOWeapons.WeaponType.isMainHand) //apply weapon type mod
			percentageMod += playerStats.mainWeaponDamageModifier.finalPercentageValue - 1;
		else if (weaponBaseRef.weaponType == SOWeapons.WeaponType.isBoth)
			percentageMod += playerStats.dualWeaponDamageModifier.finalPercentageValue - 1;

		if (weaponBaseRef.isRangedWeapon) //apply ranged weapon mod if it is one
			percentageMod += playerStats.rangedWeaponDamageModifier.finalPercentageValue - 1;

		return percentageMod;
	}

	private void WeaponInitilization()
	{
		if (GetComponent<InventoryItemUi>() != null) return;  //return as this is an item in inventory
		if (transform.parent == null) return;               //weapon is not equipped

		parentObj = transform.parent.gameObject;
		attackWeaponSprite = GetComponent<SpriteRenderer>();
		idleWeaponSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
		idleWeaponSprite.sprite = attackWeaponSprite.sprite;
		animator = GetComponent<Animator>();
		boxCollider = gameObject.AddComponent<BoxCollider2D>();
		boxCollider.enabled = false;
		boxCollider.isTrigger = true;
		canAttackAgain = true;
		animator.SetBool("isMeleeAttack", false);

		if (weaponBaseRef.weaponType == SOWeapons.WeaponType.isMainHand)
			idleWeaponSprite.enabled = true;

	}

	protected override void OnTriggerEnter2D(Collider2D other)
	{
		if (!isEquippedByPlayer && !isEquippedByOther)
			base.OnTriggerEnter2D(other);

		if (!isEquippedByPlayer && !isEquippedByOther) return;
		if (other.gameObject.GetComponent<Damageable>() == null) return; //|| !isEquippedByPlayer == false && !isEquippedByOther == false)

		other.GetComponent<Damageable>().OnHitFromDamageSource(boxCollider, damage, 
			(IDamagable.DamageType)weaponBaseRef.baseDamageType, weaponBaseRef.baseKnockback, false, isEquippedByPlayer);
	}
	public void Attack(Vector3 positionOfThingToAttack)
	{
		if (!canAttackAgain) return;

		MeleeDirectionToAttack(positionOfThingToAttack);
		OnWeaponAttack();
		StartCoroutine(WeaponCooldown());
	}
	private IEnumerator WeaponCooldown()
	{
		yield return new WaitForSeconds(0.1f);
		OnWeaponCooldown();
		yield return new WaitForSeconds(weaponBaseRef.baseAttackSpeed - 0.1f);
		canAttackAgain = true;
	}

	private void OnWeaponAttack()
	{
		animator.SetBool("isMeleeAttack", true);
		boxCollider.enabled = true;
		idleWeaponSprite.enabled = false;
		attackWeaponSprite.enabled = true;
		canAttackAgain = false;
	}
	private void OnWeaponCooldown()
	{
		parentObj.transform.parent.eulerAngles = new Vector3(0, 0, 0); //reset attack direction
		animator.SetBool("isMeleeAttack", false);
		boxCollider.enabled = false;
		idleWeaponSprite.enabled = true;
		attackWeaponSprite.enabled = false;
	}
	private void MeleeDirectionToAttack(Vector3 positionOfThingToAttack)
	{
		/// <summary>
		/// change rotation of weaponSlot (parent of this obj) based on direction of mouse from player depending on what vector is greater
		/// 0.71 is the lowest ive ever managed to get when attacking diagonally from player pos, so for now vector needs to be greater then 0.7
		/// </summary>

		positionOfThingToAttack.z = parentObj.transform.parent.position.z;

		Vector3 towardsMouseFromPlayer = positionOfThingToAttack - parentObj.transform.parent.position;
		Vector3 vectorAttack = towardsMouseFromPlayer.normalized;

		if (vectorAttack.y >= 0.7)
			parentObj.transform.parent.eulerAngles = new Vector3(0, 0, -90);
		else if (vectorAttack.y <= -0.7)
			parentObj.transform.parent.eulerAngles = new Vector3(0, 0, 90);
		else if (vectorAttack.x >= 0.7)
			parentObj.transform.parent.eulerAngles = new Vector3(0, 0, 180);
		else if (vectorAttack.x <= -0.7)
			parentObj.transform.parent.eulerAngles = new Vector3(0, 0, 0);
	}
}
