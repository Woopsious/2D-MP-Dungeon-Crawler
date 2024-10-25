using System;
using System.Collections;
using UnityEngine;

public class Weapons : Items
{
	[Header("Weapon Info")]
	public PlayerController player;
	public bool isShield;
	public int damage;
	public int bonusMana;
	public bool isEquippedByPlayer;
	public bool isEquippedByOther;

	public bool canAttackAgain;
	public GameObject parentObj;
	private SpriteRenderer attackWeaponSprite;
	private SpriteRenderer idleWeaponSprite;
	private AudioHandler audioHandler;
	private Animator animator;
	private BoxCollider2D boxCollider;

	public void Start()
	{
		if (generateStatsOnStart)
			GenerateStatsOnStart();
	}

	//set weapon data
	public override void Initilize(Rarity setRarity, int setLevel, int setEnchantmentLevel)
	{
		base.Initilize(setRarity, setLevel, setEnchantmentLevel);

		isShield = weaponBaseRef.isShield;
		damage = (int)(weaponBaseRef.baseDamage * levelModifier);
		bonusMana = (int)(weaponBaseRef.baseBonusMana * levelModifier);
		isStackable = weaponBaseRef.isStackable;
	}
	public override void SetToolTip(EntityStats playerStats, bool itemInShopSlot)
	{
		toolTip = GetComponent<ToolTipUi>();

		damage = (int)(weaponBaseRef.baseDamage * levelModifier);

		//apply offhand weapon dmg to main weapon (atm only useful for dagger)
		if (playerStats.equipmentHandler != null && playerStats.equipmentHandler.equippedOffhandWeapon != null)
			damage += playerStats.equipmentHandler.equippedOffhandWeapon.damage;

		damage = (int)(damage * GetWeaponDamageModifier(playerStats));

		string rarity;
		if (this.rarity == Rarity.isLegendary)
			rarity = "<color=orange>Legendary</color>";
		else if (this.rarity == Rarity.isEpic)
			rarity = "<color=purple>Epic</color>";
		else if (this.rarity == Rarity.isRare)
			rarity = "<color=blue>Rare</color>";
		else
			rarity = "Common";

		string weightClass;
		if (weaponBaseRef.classRestriction == SOWeapons.ClassRestriction.heavy)
			weightClass = "Heavy Weight Restriction";
		else if (weaponBaseRef.classRestriction == SOWeapons.ClassRestriction.medium)
			weightClass = "Medium Weight Restriction";
		else
			weightClass = "Light Weight Restriction";

		string info;
		if (itemEnchantmentLevel == 0)
			info = $"{rarity} Level {itemLevel} {itemName}\n{AdjustItemPriceDisplay(itemInShopSlot)} Price \n{weightClass}";
		else
			info = $"{rarity} Level {itemLevel} Enchanted {itemName} +{itemEnchantmentLevel}\n{itemPrice} Price \n{weightClass}";

		if (weaponBaseRef.weaponGripType == SOWeapons.WeaponGripType.isMainHand)
			info += "\n Main hand ";
		else if (weaponBaseRef.weaponGripType == SOWeapons.WeaponGripType.isOffhand)
			info += "\n Offhand ";
		else if (weaponBaseRef.weaponGripType == SOWeapons.WeaponGripType.isBoth)
			info += "\n Dual hand ";

		if (isShield)
			info += "";
		else if (weaponBaseRef.isRangedWeapon)
		{
			info += "Ranged Weapon\nHalf Damage At Min Range\n" +
				$"{weaponBaseRef.maxAttackRange} Max Range\n{weaponBaseRef.minAttackRange} Min Range";
		}
		else
			info += $"Melee Weapon\n{weaponBaseRef.maxAttackRange} Reach";

		string damageInfo;
		if (isShield)
			damageInfo = $"{damage} Extra Health\n{damage} To All Res";
		else
		{
			int dps = (int)(damage / weaponBaseRef.baseAttackSpeed);
			damageInfo = $"\n{dps} Dps\n{damage} Damage\n" +
				$"{weaponBaseRef.baseAttackSpeed}s Attack speed\n{weaponBaseRef.baseKnockback} Knockback\n{bonusMana} Mana bonus";
		}

		string equipInfo;
		if (playerStats.entityLevel < itemLevel)
			equipInfo = "<color=red>Cant Equip Weapon \n Level Too High</color>";
		else if ((int)PlayerClassesUi.Instance.currentPlayerClass.classRestriction < (int)weaponBaseRef.classRestriction)
			equipInfo = "<color=red>Cant Equip Weapon \n Weight too heavy</color>";
		else
			equipInfo = "<color=green>Can Equip Weapon</color>";

		toolTip.tipToShow = $"{info}\n{damageInfo}\n{equipInfo}";
	}

	//set equipped weapon data
	public void WeaponInitilization(SpriteRenderer idleWeaponSprite)
	{
		if (GetComponent<InventoryItemUi>() != null) return;    //return as this is an item in inventory
		if (transform.parent == null) return;                   //weapon is not equipped

		parentObj = transform.parent.gameObject;
		parentObj.transform.rotation = Quaternion.Euler(Vector3.zero);
		attackWeaponSprite = GetComponent<SpriteRenderer>();
		this.idleWeaponSprite = idleWeaponSprite;
		this.idleWeaponSprite.sprite = attackWeaponSprite.sprite;
		audioHandler = GetComponent<AudioHandler>();
		animator = GetComponent<Animator>();
		boxCollider = gameObject.AddComponent<BoxCollider2D>();
		boxCollider.enabled = false;
		boxCollider.isTrigger = true;
		canAttackAgain = true;
		animator.SetBool("isMeleeAttack", false);
		animator.SetBool("isRangedAttack", false);

		if (weaponBaseRef.isRangedWeapon)
			idleWeaponSprite.transform.rotation = Quaternion.Euler(0, 180, 180);
		else
			idleWeaponSprite.transform.rotation = Quaternion.Euler(0, 0, 180);

		if (weaponBaseRef.weaponGripType == SOWeapons.WeaponGripType.isMainHand)
			idleWeaponSprite.enabled = true;
	}
	public void AddPlayerRef(PlayerController player)
	{
		this.player = player;
	}
	public void UpdateWeaponDamage(SpriteRenderer idleWeaponSprite, EntityStats stats, Weapons offHandWeapon)
	{
		damage = (int)(weaponBaseRef.baseDamage * levelModifier * stats.damageDealtModifier.finalPercentageValue);
		damage = (int)(damage * GetWeaponDamageModifier(stats));

		/*
		if (stats.equipmentHandler != null && offHandWeapon != null)
			damage += offHandWeapon.damage;
		if (offHandWeapon != null) //apply offhand weapon dmg to main weapon (atm only useful for dagger)
		{
			if (offHandWeapon.weaponBaseRef.weaponType == SOWeapons.WeaponType.isBoth)
				damage += offHandWeapon.damage;
		}
		*/
	}
	public float GetWeaponDamageModifier(EntityStats stats)
	{
		float percentageMod = 0;

		if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isPhysicalDamageType) //apply damage type mod
			percentageMod = stats.physicalDamagePercentageModifier.finalPercentageValue;
		else if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isPoisonDamageType)
			percentageMod = stats.poisonDamagePercentageModifier.finalPercentageValue;
		else if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isFireDamageType)
			percentageMod = stats.fireDamagePercentageModifier.finalPercentageValue;
		else if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isIceDamageType)
			percentageMod = stats.iceDamagePercentageModifier.finalPercentageValue;

		if (weaponBaseRef.weaponGripType == SOWeapons.WeaponGripType.isMainHand) //apply weapon type mod
			percentageMod += stats.mainWeaponDamageModifier.finalPercentageValue - 1;
		else if (weaponBaseRef.weaponGripType == SOWeapons.WeaponGripType.isBoth)
			percentageMod += stats.dualWeaponDamageModifier.finalPercentageValue - 1;

		if (weaponBaseRef.isRangedWeapon) //apply ranged weapon mod if it is one
			percentageMod += stats.rangedWeaponDamageModifier.finalPercentageValue - 1;

		return percentageMod;
	}

	//weapon attack
	protected override void OnTriggerEnter2D(Collider2D other)
	{
		if (!isEquippedByPlayer && !isEquippedByOther) //disable picking up weapons that are equipped
			base.OnTriggerEnter2D(other);

		if (!isEquippedByPlayer && !isEquippedByOther) return;
		if (other.gameObject.GetComponent<Damageable>() == null) return;

		other.GetComponent<Damageable>().OnHitFromDamageSource(player, boxCollider, damage, 
			(IDamagable.DamageType)weaponBaseRef.baseDamageType, weaponBaseRef.baseKnockback, false, isEquippedByPlayer, false);
	}
	public void MeleeAttack(Vector3 positionOfThingToAttack)
	{
		if (!canAttackAgain) return;

		AttackInDirection(positionOfThingToAttack);
		OnWeaponAttack();
		StartCoroutine(WeaponCooldown());
	}
	public void RangedAttack(Vector3 positionOfThingToAttack, GameObject projectilePrefab)
	{
		if (!canAttackAgain) return;

		Projectiles projectile = DungeonHandler.GetProjectile();
		if (projectile == null)
		{
			GameObject go = Instantiate(projectilePrefab, transform, true);
			projectile = go.GetComponent<Projectiles>();
		}

		projectile.transform.SetParent(null);
		projectile.SetPositionAndAttackDirection(transform.position, positionOfThingToAttack);
		projectile.Initilize(player, this);

		AttackInDirection(positionOfThingToAttack);
		OnWeaponAttack();
		StartCoroutine(WeaponCooldown());
	}
	private IEnumerator WeaponCooldown()
	{
		float secondsForAttackCooldown;

		if (weaponBaseRef.isRangedWeapon)
			secondsForAttackCooldown = 0.5f;
		else
			secondsForAttackCooldown = 0.1f;

		yield return new WaitForSeconds(secondsForAttackCooldown);

		OnWeaponCooldown();
		if (isEquippedByPlayer)
			yield return new WaitForSeconds(weaponBaseRef.baseAttackSpeed - secondsForAttackCooldown);
		else
			yield return new WaitForSeconds(weaponBaseRef.baseAttackSpeed + 0.25f - secondsForAttackCooldown);
		canAttackAgain = true;
	}

	//sound + animation
	private void OnWeaponAttack()
	{
		if (weaponBaseRef.isRangedWeapon)
			animator.SetBool("isRangedAttack", true);
		else
		{
			animator.SetBool("isMeleeAttack", true);
			boxCollider.enabled = true;
		}

		idleWeaponSprite.enabled = false;
		attackWeaponSprite.enabled = true;
		audioHandler.PlayAudio(weaponBaseRef.attackSfx);
		canAttackAgain = false;
	}
	private void OnWeaponCooldown()
	{
		parentObj.transform.parent.eulerAngles = new Vector3(0, 0, 0); //reset attack direction
		animator.SetBool("isMeleeAttack", false);
		animator.SetBool("isRangedAttack", false);
		boxCollider.enabled = false;
		idleWeaponSprite.enabled = true;
		attackWeaponSprite.enabled = false;
	}

	//set direction of melee swings + direction ranged weapons point
	private void AttackInDirection(Vector3 positionOfThingToAttack)
	{
		Vector3 rotation = positionOfThingToAttack - transform.position;
		float rotz = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
		parentObj.transform.rotation = Quaternion.Euler(0, 0, rotz - 180);
	}
}
