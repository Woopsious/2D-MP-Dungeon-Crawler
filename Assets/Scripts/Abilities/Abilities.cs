using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Abilities : MonoBehaviour
{
	private bool isStatusEffectTimerForUi;

	[Header("Ability Info")]
	private ToolTipUi toolTip;
	public SOAbilities abilityBaseRef;
	public SOStatusEffects effectBaseRef;

	public string abilityName;
	public string abilityDescription;
	public Image abilityImage;
	public Sprite abilitySprite;

	[Header("Ability Dynamic Info")]
	public bool isEquippedAbility;
	public bool isOnCooldown;
	public bool isAbilityQueuedUp;
	public float abilityCooldownTimer;

	[Header("Spell Cost")]
	public int manaCost;

	private void Update()
	{
		if (isStatusEffectTimerForUi)
			StatusEffectUiCooldownTimer();
		else
			AbilityCooldownTimer();
	}

	private void OnEnable()
	{
		PlayerEventManager.OnPlayerStatChangeEvent += UpdateToolTip;
	}

	private void OnDisable()
	{
		PlayerEventManager.OnPlayerStatChangeEvent -= UpdateToolTip;
	}

	//set data types
	public void Initilize()
	{
		toolTip = GetComponent<ToolTipUi>();
		isStatusEffectTimerForUi = false;
		name = abilityBaseRef.Name;
		abilityName = abilityBaseRef.Name;
		abilityDescription = abilityBaseRef.Description;
		abilitySprite = abilityBaseRef.abilitySprite;

		isEquippedAbility = false;
		isOnCooldown = false;
		abilityCooldownTimer = 0;

		UpdateToolTip(SceneHandler.playerInstance.playerStats);
	}
	public void InitilizeStatusEffectUiTimer(SOStatusEffects effect, float currentTimer)
	{
		effectBaseRef = effect;
		isStatusEffectTimerForUi = true;
		name = effect.Name;
		abilityName = effect.Name;
		abilityDescription = effect.Name;

		abilitySprite = effect.effectSprite;
		abilityCooldownTimer = currentTimer;

		GetComponent<ToolTipUi>().tipToShow = $"{abilityDescription}";
	}

	//tool tips
	public void UpdateToolTip(EntityStats playerStats)
	{
		if (abilityBaseRef == null || abilityBaseRef.Description == null) return;
		string info = $"{abilityBaseRef.Description}";

		if (abilityBaseRef.canOnlyTargetSelf) //targeting info
			info += "\nCan only cast on self";
		else if (abilityBaseRef.requiresTarget && abilityBaseRef.isOffensiveAbility)
			info += "\nNeeds selected enemy target";
		else if (abilityBaseRef.requiresTarget && !abilityBaseRef.isOffensiveAbility)
			info += "\nNeeds selected friendly target";

		if (abilityBaseRef.damageType != IDamagable.DamageType.isHealing) //abilty info
		{
			int damage = (int)(abilityBaseRef.damageValue * Utilities.GetLevelModifier(playerStats.entityLevel));

			if (abilityBaseRef.damageType == IDamagable.DamageType.isPhysicalDamage)
				damage = (int)(damage * playerStats.physicalDamagePercentageModifier.finalPercentageValue);
			if (abilityBaseRef.damageType == IDamagable.DamageType.isPoisonDamage)
				damage = (int)(damage * playerStats.poisonDamagePercentageModifier.finalPercentageValue);
			if (abilityBaseRef.damageType == IDamagable.DamageType.isFireDamage)
				damage = (int)(damage * playerStats.fireDamagePercentageModifier.finalPercentageValue);
			if (abilityBaseRef.damageType == IDamagable.DamageType.isIceDamage)
				damage = (int)(damage * playerStats.iceDamagePercentageModifier.finalPercentageValue);

			if (damage != 0) //optional instant damage info
			{
				info += $"\nDeals {damage} damage to enemies ";
				if (abilityBaseRef.isAOE) //optional aoe info
					info += "inside AoE";
			}

			if (abilityBaseRef.hasAoeDuration) //optional aoe info
				info += $"\nAoe lasts for {abilityBaseRef.aoeDuration}s";

			if (abilityBaseRef.hasStatusEffects) //optional effect info
				info += SetStatusEffectToolTips(playerStats);
		}
		else if (abilityBaseRef.damageType == IDamagable.DamageType.isHealing) //healing info
		{
			float healing = Utilities.ConvertFloatToUiPercentage(abilityBaseRef.damageValuePercentage);

			if (abilityBaseRef.isAOE)
				info += $"\nHeals for {healing}% of health for friendlies inside AoE";
			else
				info += $"\nHeals for {healing}% of health for selected friendlies or self ";
		}

		info += $"\nHas a {abilityBaseRef.abilityCooldown}s cooldown"; //cooldown effect info

		if (abilityBaseRef.isSpell) //optional spell info
			info += $"\nCosts {(int)(abilityBaseRef.manaCost * Utilities.GetLevelModifier(playerStats.entityLevel))} mana";

		toolTip.tipToShow = $"{info}";
	}
	private string SetStatusEffectToolTips(EntityStats playerStats)
	{
		string info = "\n";

		foreach (SOStatusEffects effect in abilityBaseRef.statusEffects) //list all effect info
		{
			if (effect.isDOT) //dot info
				info += $"Deals {effect.effectValue * playerStats.levelModifier} damage every second";
			else //effect info
			{
				if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isDamageEffect) //effect type info
					info += $"Applies a {Utilities.ConvertFloatToUiPercentage(effect.effectValue)}% damage ";
				else if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isResistanceEffect)
					info += $"Applies a {Utilities.ConvertFloatToUiPercentage(effect.effectValue)}% damage res ";
				else if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isDamageRecievedEffect)
					info += $"Applies a {Utilities.ConvertFloatToUiPercentage(effect.effectValue)}% damage recieved modifier ";
				else if (effect.statusEffectType == SOStatusEffects.StatusEffectType.isMovementEffect)
					info += $"Applies a {Utilities.ConvertFloatToUiPercentage(effect.effectValue)}% movement speed ";

				if (abilityBaseRef.canOnlyTargetSelf) //target info
					info += "buff to yourself";
				else
				{
					if (abilityBaseRef.isOffensiveAbility && abilityBaseRef.isAOE)
						info += "debuff to enemies inside AoE";
					else if (!abilityBaseRef.isOffensiveAbility && abilityBaseRef.isAOE)
						info += "buff to friendlies/self inside AoE";

					if (abilityBaseRef.isOffensiveAbility && !abilityBaseRef.isAOE)
						info += "debuff to selected enemy";
					else if (!abilityBaseRef.isOffensiveAbility && !abilityBaseRef.isAOE)
						info += "buff to selected friendlies or self";
				}
				info += $"\nEffect lasts for {effect.abilityDuration}s";
			}
		}
		return info;
	}

	//timers
	private void AbilityCooldownTimer()
	{
		if (!isOnCooldown) return;

		abilityCooldownTimer += Time.deltaTime;
		abilityImage.fillAmount = abilityCooldownTimer / abilityBaseRef.abilityCooldown;

		if (abilityCooldownTimer >= abilityBaseRef.abilityCooldown)
		{
			isOnCooldown = false;
			abilityImage.fillAmount = 1;
			abilityCooldownTimer = 0;
		}
	}
	private void StatusEffectUiCooldownTimer()
	{
		abilityCooldownTimer += Time.deltaTime;
		abilityImage.fillAmount = abilityCooldownTimer / effectBaseRef.abilityDuration;

		if (abilityCooldownTimer >= effectBaseRef.abilityDuration)
		{
			isOnCooldown = false;
			abilityImage.fillAmount = 1;
			abilityCooldownTimer = 0;
			gameObject.SetActive(false);
		}
	}
	public void ResetEffectTimer()
	{
		abilityCooldownTimer = 0;
	}

	//bool checks
	public bool CanUseAbility(EntityStats entityStats)
	{
		if (isOnCooldown || !CanAffordManaCost(entityStats))
			return false;
		else return true;
	}
	public bool CanAffordManaCost(EntityStats entityStats)
	{
		int totalManaCost = (int)(abilityBaseRef.manaCost * Utilities.GetLevelModifier(entityStats.entityLevel));
		if (entityStats.currentMana < totalManaCost)
			return false;
		else return true;
	}
	public bool CanInstantCastAbility()
	{
		if (abilityBaseRef.canOnlyTargetSelf || abilityBaseRef.isProjectile)
			return true;
		if (abilityBaseRef.isAOE)
		{
			if (PlayerSettingsManager.Instance.autoCastAoeAbilitiesOnTarget && PlayerHasSelectedEnemy())
				return true;
			else return false;
		}

		if (abilityBaseRef.requiresTarget)
		{
			if (abilityBaseRef.isOffensiveAbility)
			{
				if (PlayerSettingsManager.Instance.autoCastEffectAbilitiesOnTarget && PlayerHasSelectedEnemy())
					return true;
				else return false;
			}
			else return true; //always possible to insta cast positive buffs on self
		}

		Debug.LogError("failed to figure out if ability can be insta casted");
		return false;
	}
	private bool PlayerHasSelectedEnemy()
	{
		if (SceneHandler.playerInstance.selectedEnemyTarget != null)
			return true;
		else return false;
	}
}
