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

		other.GetComponent<Damageable>().OnHitFromDamageSource(damage, (IDamagable.DamageType)weaponBaseRef.baseDamageType, isEquippedByPlayer);

		Vector2 difference = other.transform.position - gameObject.transform.position;
		difference = difference.normalized * weaponBaseRef.baseKnockback * 100;
		Debug.LogWarning(difference.normalized);
		other.GetComponent<Rigidbody2D>().AddForce(difference, ForceMode2D.Impulse);
	}
	public void Attack()
	{
		if (!canAttackAgain) return;

		idleWeaponSprite.enabled = false;
		attackWeaponSprite.enabled = true;
		MeleeDirectionToAttack();
		animator.SetBool("isMeleeAttack", true);
		canAttackAgain = false;
		boxCollider.enabled = true;
		StartCoroutine(weaponCooldown());
	}
	IEnumerator weaponCooldown()
	{
		yield return new WaitForSeconds(0.1f);
		boxCollider.enabled = false;
		animator.SetBool("isMeleeAttack", false);
		idleWeaponSprite.enabled = true;
		attackWeaponSprite.enabled = false;
		parentObj.transform.parent.eulerAngles = new Vector3(0, 0, 0);
		yield return new WaitForSeconds(weaponBaseRef.baseAttackSpeed - 0.1f);
		canAttackAgain = true;
	}
	public void MeleeDirectionToAttack()
	{
		/// <summary>
		/// change rotation of weaponSlot (parent of this obj) based on direction of mouse from player depending on what vector is greater
		/// 0.71 is the lowest ive ever managed to get when attacking diagonally from player pos, so for now vector needs to be greater then 0.7
		/// </summary>

		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mousePos.z = parentObj.transform.parent.position.z;

		Vector3 towardsMouseFromPlayer = mousePos - parentObj.transform.parent.position;
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
