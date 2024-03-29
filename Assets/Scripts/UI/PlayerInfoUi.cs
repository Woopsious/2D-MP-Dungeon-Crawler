using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInfoUi : MonoBehaviour
{
	public static PlayerInfoUi Instance;

	public static PlayerController playerInstance;
	public TMP_Text playerInfo;

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		EventManagerUi.OnPlayerStatChangeEvent += UpdatePlayerStatInfo;
	}
	private void OnDisable()
	{
		EventManagerUi.OnPlayerStatChangeEvent -= UpdatePlayerStatInfo;
	}

	public void UpdatePlayerStatInfo(EntityStats stats)
	{
		playerInstance = stats.GetComponent<PlayerController>();

		string mainInfo = $"(Player Name)\nHealth: {stats.currentHealth} / {stats.maxHealth.finalValue}" +
			$"\r\nMana: {stats.currentMana} / {stats.maxMana.finalValue}";

		string weaponInfo;
		if (stats.equipmentHandler != null && stats.equipmentHandler.equippedWeapon != null)
		{
			int dps = (int)(stats.equipmentHandler.equippedWeapon.damage /
				stats.equipmentHandler.equippedWeapon.weaponBaseRef.baseAttackSpeed);

			weaponInfo = $"\r\n\r\nWeapon DPS: {dps} \r\nWeapon Damage: {stats.equipmentHandler.equippedWeapon.damage} \r\n " +
				$"Weapon Knockback: {stats.equipmentHandler.equippedWeapon.weaponBaseRef.baseKnockback}";
			string rangeInfo;

			if (stats.equipmentHandler.equippedWeapon.weaponBaseRef.isRangedWeapon)
				rangeInfo = $"\r\nWeapon Range: {stats.equipmentHandler.equippedWeapon.weaponBaseRef.baseMaxAttackRange}";
			else
				rangeInfo = $"\r\nWeapon Range: Melee";

			weaponInfo += $"\r\nWeapon Speed: {stats.equipmentHandler.equippedWeapon.weaponBaseRef.baseAttackSpeed}{rangeInfo}";
		}
		else
			weaponInfo = "\r\n\r\nNo main weapon equipped";

		string damageBonusInfo = $"\r\n\r\nBonus Main Weapon Damage: {GetPercentageValue(stats.mainWeaponDamageModifier)}%" +
			$"\r\nBonus Dual Weapon Damage: {GetPercentageValue(stats.dualWeaponDamageModifier)}%" +
			$"\r\nBonus Ranged Weapon Damage: {GetPercentageValue(stats.rangedWeaponDamageModifier)}%" +
			$"\r\n\r\nBonus Physical Damage: {GetPercentageValue(stats.physicalDamagePercentageModifier)}%" +
			$"\r\nBonus Posion Damage: {GetPercentageValue(stats.poisonDamagePercentageModifier)}%" +
			$"\r\nBonus Fire Damage: {GetPercentageValue(stats.fireDamagePercentageModifier)}%" +
			$"\r\nBonus Ice Damage: {GetPercentageValue(stats.iceDamagePercentageModifier)}%";

		string resistanceInfo = $"\r\n\r\nPhysical Resist: {stats.physicalResistance.finalValue}" +
			$"\r\nPosion Resist: {stats.poisonResistance.finalValue}" +
			$"\r\nFire Resist: {stats.fireResistance.finalValue}" +
			$"\r\nIce Resist: {stats.iceResistance.finalValue}";

		playerInfo.text = mainInfo + weaponInfo + damageBonusInfo + resistanceInfo;
	}
	public int GetPercentageValue(Stat stat)
	{
		float newValue;
		newValue = stat.finalPercentageValue - 1;
		newValue *= 100;

		return Mathf.RoundToInt(newValue);
	}
}
