using System.Collections;
using System.Collections.Generic;
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
		Debug.Log("initial damage: " + damage);
		bonusMana = (int)(weaponBaseRef.baseBonusMana * levelModifier);
		isStackable = weaponBaseRef.isStackable;
	}
	private void WeaponInitilization()
	{
		if (GetComponent<InventoryItem>() != null) return;  //return as this is an item in inventory
		if (transform.parent == null) return;				//weapon is not equipped

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
	public void UpdateWeaponDamage(float phyMod, float poiMod, float fireMod, float iceMod, Weapons offHandWeapon)
	{
		Debug.Log("old damage: " + damage);

		if (offHandWeapon != null) //apply offhand weapon dmg to main weapon (atm only useful for dagger)
			damage += offHandWeapon.damage;

		Debug.Log("physical damage modifier" + phyMod);

		Debug.Log("damage + offhand weapon: " + damage);

		if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isPhysicalDamageType)
			damage = (int)(damage * phyMod);
		else if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isPoisonDamageType)
			damage = (int)(damage * poiMod);
		else if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isFireDamageType)
			damage = (int)(damage * fireMod);
		else if (weaponBaseRef.baseDamageType == SOWeapons.DamageType.isIceDamageType)
			damage = (int)(damage * iceMod);

		Debug.Log("new damage: " + damage);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.GetComponent<Damageable>() == null || isEquippedByPlayer == false && isEquippedByOther == false) return;

		///<summery>
		///check for damage modifiers (from accessories or class skill trees etc) here when attacking with main weapon
		///then add it to the damage done and pass it onto Damageable script
		///<summery>

		other.GetComponent<Damageable>().OnHitFromDamageSource(damage, (IDamagable.DamageType)weaponBaseRef.baseDamageType, isEquippedByPlayer);

		Vector2 difference = other.transform.position - gameObject.transform.position;
		difference = difference.normalized * weaponBaseRef.baseKnockback * 100;
		Debug.LogWarning(difference.normalized);
		other.GetComponent<Rigidbody2D>().AddForce(difference, ForceMode2D.Impulse);
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
