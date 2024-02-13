using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClassUnlocksScriptableObject", menuName = "ClassUnlocks/StatBoosts")]
public class SOClassStatBonuses : SOClassUnlocks
{
	[Header("Stat Info")]
	public bool isHealthBoost;
	public bool isManaBoost;
	public bool isManaRegenBoost;

	public bool isPhysicalResistanceBoost;
	public bool isPoisonResistanceBoost;
	public bool isFireResistanceBoost;
	public bool isIceResistanceBoost;

	public bool isPhysicalDamageBoost;
	public bool isPosionDamageBoost;
	public bool isFireDamageBoost;
	public bool isIceDamageBoost;

	public float percentageValue;
}
