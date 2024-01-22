using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : Items
{
	[Header("Weapon Info")]
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
			SetItemStats(rarity, itemLevel);

		parentObj = transform.parent.gameObject;
		attackWeaponSprite = GetComponent<SpriteRenderer>();
		attackWeaponSprite.enabled = false;
		idleWeaponSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
		idleWeaponSprite.enabled = true;
		idleWeaponSprite.sprite = attackWeaponSprite.sprite;
		animator = GetComponent<Animator>();
		boxCollider = gameObject.AddComponent<BoxCollider2D>();
		boxCollider.enabled = false;
		boxCollider.isTrigger = true;
		canAttackAgain = true;
		animator.SetBool("isMeleeAttack", false);
	}

	public override void SetItemStats(Rarity setRarity, int setLevel)
	{
		base.SetItemStats(setRarity, setLevel);

		damage = (int)(weaponBaseRef.baseDamage * statModifier);
		bonusMana = (int)(weaponBaseRef.baseBonusMana * statModifier);
		isStackable = weaponBaseRef.isStackable;
	}

	public void OnTriggerEnter2D(Collider2D other)
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
		StartCoroutine(weaponCooldown());
	}
	IEnumerator weaponCooldown()
	{
		yield return new WaitForSeconds(0.1f);
		OnWeaponCooldown();
		yield return new WaitForSeconds(weaponBaseRef.baseAttackSpeed - 0.1f);
		canAttackAgain = true;
	}

	public void OnWeaponAttack()
	{
		animator.SetBool("isMeleeAttack", true);
		boxCollider.enabled = true;
		idleWeaponSprite.enabled = false;
		attackWeaponSprite.enabled = true;
		canAttackAgain = false;
	}
	public void OnWeaponCooldown()
	{
		parentObj.transform.parent.eulerAngles = new Vector3(0, 0, 0); //reset attack direction
		animator.SetBool("isMeleeAttack", false);
		boxCollider.enabled = false;
		idleWeaponSprite.enabled = true;
		attackWeaponSprite.enabled = false;
	}
	public void MeleeDirectionToAttack(Vector3 positionOfThingToAttack)
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
