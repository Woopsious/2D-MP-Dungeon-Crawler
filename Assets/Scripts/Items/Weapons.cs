using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
	[Header("Weapon Info")]
	public int damage;
	public int bonusMana;
	public bool isEquippedByPlayer;
	public bool isEquippedByNonPlayer;

	//move damageType to SoWeapons in future
	[Header("Damage Type")]
	public DamageType baseDamageType;
	public enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}
}
