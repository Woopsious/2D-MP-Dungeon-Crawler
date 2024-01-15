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

	private bool canAttackAgain;
	private BoxCollider2D boxCollider;

	public void Start()
	{
		if (generateStatsOnStart)
			SetItemStats(rarity, itemLevel);

		boxCollider = gameObject.AddComponent<BoxCollider2D>();
		boxCollider.enabled = false;
		boxCollider.isTrigger = true;
		canAttackAgain = true;
	}

	public override void SetItemStats(Rarity setRarity, int setLevel)
	{
		base.SetItemStats(setRarity, setLevel);

		damage = (int)(weaponBaseRef.baseDamage * statModifier);
		bonusMana = (int)(weaponBaseRef.baseBonusMana * statModifier);
		isStackable = weaponBaseRef.isStackable;

		if (entityEquipmentHandler != null) //for non player
			entityEquipmentHandler.OnWeaponEquip(this, false, true);
		else if (playerEquipmentHandler != null)//for player
			playerEquipmentHandler.OnWeaponEquip(this, true, false);
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

		canAttackAgain = false;
		boxCollider.enabled = true;
		StartCoroutine(weaponCooldown());
	}
	IEnumerator weaponCooldown()
	{
		yield return new WaitForSeconds(0.1f);
		boxCollider.enabled = false;
		yield return new WaitForSeconds(weaponBaseRef.baseAttackSpeed - 0.1f);
		canAttackAgain = true;
	}
}
