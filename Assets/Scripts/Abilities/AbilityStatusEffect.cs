using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AbilityStatusEffect : NetworkBehaviour
{
	public EntityStats casterInfo;
	public EntityStats entityEffectIsAppliedTo;
	public SOStatusEffects statusEffect;

	public float abilityDurationTimer;

	public int damage;
	private readonly float damageOverTimeCooldown = 1f;
	private float timerTillNextDamage;

	private void Update()
	{
		if (!MultiplayerManager.IsClientHost()) return;

		AbilityDurationTimer();
		DamageOverTimeEffect();
	}

	//set data
	public void Initilize(EntityStats casterInfo, EntityStats entityEffectIsAppliedTo, SOStatusEffects statusEffect)
	{
		if (!MultiplayerManager.IsMultiplayer()) //fix ref as Network spawn manager wont exist in sp
		{
			InitilizeSinglePlayer(casterInfo, entityEffectIsAppliedTo, statusEffect);
			return;
		}

		ulong casterId = casterInfo.GetComponent<NetworkObject>().NetworkObjectId;
		ulong entityIdEffectIsAppliedTo = entityEffectIsAppliedTo.GetComponent<NetworkObject>().NetworkObjectId;
		int statusEffectIndex = 0;

		foreach (SOStatusEffects effect in AssetDatabase.Database.statusEffects)
		{
			if (statusEffect != effect)
			{
				statusEffectIndex++;
				continue;
			}

			SyncStatusEffectRpc(casterId, entityIdEffectIsAppliedTo, statusEffectIndex);
			break;
		}
	}

	[Rpc(SendTo.Everyone)]
	public void SyncStatusEffectRpc(ulong casterId, ulong entityIdEffectIsAppliedTo, int statusEffectIndex)
	{
		SyncStatusEffect(casterId, entityIdEffectIsAppliedTo, statusEffectIndex);
	}
	public void SyncStatusEffect(ulong casterId, ulong entityIdEffectIsAppliedTo, int statusEffectIndex)
	{
		casterInfo = NetworkManager.SpawnManager.SpawnedObjects[casterId].GetComponent<EntityStats>();
		entityEffectIsAppliedTo = NetworkManager.SpawnManager.SpawnedObjects[entityIdEffectIsAppliedTo].GetComponent<EntityStats>();
		statusEffect = AssetDatabase.Database.statusEffects[statusEffectIndex];

		SetParentObjectRpc();

		gameObject.name = statusEffect.Name + "Effect";
		damage = (int)(statusEffect.effectValue * Utilities.GetLevelModifier(casterInfo.entityLevel));
		timerTillNextDamage = 0f;

		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}
	private void InitilizeSinglePlayer(EntityStats casterInfo, EntityStats entityEffectIsAppliedTo, SOStatusEffects statusEffect)
	{
		this.casterInfo = casterInfo;
		this.entityEffectIsAppliedTo = entityEffectIsAppliedTo;
		this.statusEffect = statusEffect;

		transform.SetParent(entityEffectIsAppliedTo.transform);
		transform.localPosition = Vector3.zero;

		gameObject.name = statusEffect.Name + "Effect";
		damage = (int)(statusEffect.effectValue * Utilities.GetLevelModifier(casterInfo.entityLevel));
		timerTillNextDamage = 0f;

		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SetParentObjectRpc()
	{
		transform.SetParent(entityEffectIsAppliedTo.transform);
		transform.localPosition = Vector3.zero;
	}

	//clear effect sync
	[Rpc(SendTo.Everyone)]
	public void ClearStatusEffectRpc()
	{
		ClearStatusEffect();
	}
	public void ClearStatusEffect()
	{
		entityEffectIsAppliedTo.UnApplyStatusEffect(this);
	}

	//timers
	private void AbilityDurationTimer()
	{
		abilityDurationTimer += Time.deltaTime;

		if (abilityDurationTimer >= statusEffect.abilityDuration)
			entityEffectIsAppliedTo.UnApplyStatusEffect(this);
	}
	//dot effect if it has one
	private void DamageOverTimeEffect()
	{
		if (!statusEffect.isDOT || entityEffectIsAppliedTo.IsEntityDead()) return;

		timerTillNextDamage -= Time.deltaTime;
		if (timerTillNextDamage < 0)
		{
			DamageSourceInfo damageSourceInfo = new(null, IDamagable.HitBye.enviroment, damage, statusEffect.damageType, false);

			damageSourceInfo.SetDeathMessage(statusEffect);
			entityEffectIsAppliedTo.GetComponent<Damageable>().OnHitFromDamageSource(damageSourceInfo);
			timerTillNextDamage = damageOverTimeCooldown;
		}
	}

	public void ResetAbilityTimer()
	{
		abilityDurationTimer = 0;
	}
	public float GetAbilityDuration()
	{
		return abilityDurationTimer;
	}
	public SOStatusEffects GrabAbilityBaseRef()
	{
		return statusEffect;
	}
}
