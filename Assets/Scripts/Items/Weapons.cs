using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : Items
{
	[Header("Weapon Info")]
	public int damage;
	public int bonusMana;
	public bool isEquippedByPlayer;
	public bool isEquippedByNonPlayer;

	public void Start()
	{
		if (generateStatsOnStart)
			SetItemStats(rarity, itemLevel);
	}

	public override void SetItemStats(Rarity setRarity, int setLevel)
	{
		base.SetItemStats(setRarity, setLevel);

		damage = (int)(weaponBaseRef.baseDamage * statModifier);
		bonusMana = (int)(weaponBaseRef.baseBonusMana * statModifier);
		isStackable = weaponBaseRef.isStackable;

		//if (entityEquipmentHandler != null) //for non player
			//entityEquipmentHandler.OnWeaponEquip(this, false, true);
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.GetComponent<Damageable>() == null) return;

		other.GetComponent<Damageable>().OnHitFromDamageSource(damage, (IDamagable.DamageType)weaponBaseRef.baseDamageType);
	}
}
