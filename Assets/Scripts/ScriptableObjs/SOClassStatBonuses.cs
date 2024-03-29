using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClassUnlocksScriptableObject", menuName = "ClassUnlocks/StatBoosts")]
public class SOClassStatBonuses : SOClassUnlocks
{
	[Header("Stat Info")]
	public float healthBoostValue;
	public float manaBoostValue;
	public float manaRegenBoostValue;

	[Header("Resistance Boosts")]
	public float physicalResistanceBoostValue;
	public float poisonResistanceBoostValue;
	public float fireResistanceBoostValue;
	public float iceResistanceBoostValue;

	[Header("Damage boosts")]
	public float physicalDamageBoostValue;
	public float poisionDamageBoostValue;
	public float fireDamageBoostValue;
	public float iceDamageBoostValue;

	public float mainWeaponDamageBoostValue;
	public float duelWeaponDamageBoostValue;
	public float rangedWeaponDamageBoostValue;
}
