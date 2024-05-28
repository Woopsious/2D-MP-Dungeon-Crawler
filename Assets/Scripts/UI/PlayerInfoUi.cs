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
		EventManager.OnPlayerStatChangeEvent += UpdatePlayerStatInfo;
	}
	private void OnDisable()
	{
		EventManager.OnPlayerStatChangeEvent -= UpdatePlayerStatInfo;
	}

	public void UpdatePlayerStatInfo(EntityStats stats)
	{
		playerInstance = stats.GetComponent<PlayerController>();

		string mainInfo = $"(Player Name)\nHealth: {stats.currentHealth} / {stats.maxHealth.finalValue}" +
			$"\r\nMana: {stats.currentMana} / {stats.maxMana.finalValue}";

		string weaponInfo;
		if (PlayerInventoryUi.Instance.weaponEquipmentSlot.GetComponent<InventorySlotUi>().itemInSlot != null)
		{
			int dps = (int)(stats.equipmentHandler.equippedWeapon.damage /
				stats.equipmentHandler.equippedWeapon.weaponBaseRef.baseAttackSpeed);

			weaponInfo = $"\r\n\r\nWeapon DPS: {dps} \r\nWeapon Damage: {stats.equipmentHandler.equippedWeapon.damage} \r\n " +
				$"Weapon Knockback: {stats.equipmentHandler.equippedWeapon.weaponBaseRef.baseKnockback}";

			string rangeInfo;
			if (stats.equipmentHandler.equippedWeapon.weaponBaseRef.isRangedWeapon)
			{
				rangeInfo = $"\r\nMax Weapon Range: {stats.equipmentHandler.equippedWeapon.weaponBaseRef.maxAttackRange}";
				rangeInfo += $"\r\nMin Weapon Range: {stats.equipmentHandler.equippedWeapon.weaponBaseRef.minAttackRange}";
			}
			else
				rangeInfo = $"\r\nWeapon Reach: {stats.equipmentHandler.equippedWeapon.weaponBaseRef.maxAttackRange}";

			weaponInfo += $"\r\nWeapon Attack Speed: {stats.equipmentHandler.equippedWeapon.weaponBaseRef.baseAttackSpeed}{rangeInfo}";
		}
		else
			weaponInfo = "\r\n\r\nNo main weapon equipped";

		string resistanceInfo = 
			$"\r\n\r\nPhysical Resist: {stats.physicalResistance.finalValue}" +
			$"\r\nPosion Resist: {stats.poisonResistance.finalValue}" +
			$"\r\nFire Resist: {stats.fireResistance.finalValue}" +
			$"\r\nIce Resist: {stats.iceResistance.finalValue}";

		string damageBonusInfo = 
			$"\r\n\r\nBonus Main Weapon Damage: " +
			$"{Utilities.ConvertFloatToUiPercentage(stats.mainWeaponDamageModifier.finalPercentageValue - 1)}%" +
			$"\r\nBonus Dual Weapon Damage: " +
			$"{Utilities.ConvertFloatToUiPercentage(stats.dualWeaponDamageModifier.finalPercentageValue - 1)}%" +
			$"\r\nBonus Ranged Weapon Damage: " +
			$"{Utilities.ConvertFloatToUiPercentage(stats.rangedWeaponDamageModifier.finalPercentageValue - 1)}%" +
			$"\r\n\r\nBonus Physical Damage: " +
			$"{Utilities.ConvertFloatToUiPercentage(stats.physicalDamagePercentageModifier.finalPercentageValue - 1)}%" +
			$"\r\nBonus Posion Damage: " +
			$"{Utilities.ConvertFloatToUiPercentage(stats.poisonDamagePercentageModifier.finalPercentageValue - 1)}%" +
			$"\r\nBonus Fire Damage: " +
			$"{Utilities.ConvertFloatToUiPercentage(stats.fireDamagePercentageModifier.finalPercentageValue - 1)}%" +
			$"\r\nBonus Ice Damage: " +
			$"{Utilities.ConvertFloatToUiPercentage(stats.iceDamagePercentageModifier.finalPercentageValue - 1)}%";

		string resistanceBonusInfo = 
			$"\r\n\r\nPhysical Resist: {Utilities.ConvertFloatToUiPercentage(stats.physicalResistance.GetPercentageModifiers())}%" +
			$"\r\nPosion Resist: {Utilities.ConvertFloatToUiPercentage(stats.poisonResistance.GetPercentageModifiers())}%" +
			$"\r\nFire Resist: {Utilities.ConvertFloatToUiPercentage(stats.fireResistance.GetPercentageModifiers())}%" +
			$"\r\nIce Resist: {Utilities.ConvertFloatToUiPercentage(stats.iceResistance.GetPercentageModifiers())}%";

		playerInfo.text = mainInfo + weaponInfo + resistanceInfo + damageBonusInfo + resistanceBonusInfo;
	}
}
