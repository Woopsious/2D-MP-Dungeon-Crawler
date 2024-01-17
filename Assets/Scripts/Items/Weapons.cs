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
	private GameObject parentObj;
	private Animator animator;
	private BoxCollider2D boxCollider;

	public void Start()
	{
		if (generateStatsOnStart)
			SetItemStats(rarity, itemLevel);

		parentObj = transform.parent.gameObject;
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

		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		parentObj.transform.LookAt(mousePos);

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
		yield return new WaitForSeconds(weaponBaseRef.baseAttackSpeed - 0.1f);
		parentObj.transform.localEulerAngles = new Vector3(0, 0, 15);
		canAttackAgain = true;
	}
}
