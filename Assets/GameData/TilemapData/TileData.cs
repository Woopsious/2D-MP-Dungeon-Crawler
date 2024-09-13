using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileData : ScriptableObject
{
	public TileBase[] tiles;

	public bool doesInstantDamage;
	public int baseDamageDealt;
	public DamageType baseDamageType;
	public enum DamageType
	{
		isPhysicalDamageType, isPoisonDamageType, isFireDamageType, isIceDamageType
	}

	public bool appliesEffect;
	public List<SOStatusEffects> effectsToApply;
}
