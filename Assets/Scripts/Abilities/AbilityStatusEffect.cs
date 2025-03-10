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
	public void Initilize(EntityStats casterStats, EntityStats entityEffectIsAppliedTo, SOStatusEffects statusEffect)
	{
		if (MultiplayerManager.IsMultiplayer())
		{
			ulong casterId = casterStats.GetComponent<NetworkObject>().NetworkObjectId;
			ulong entityIdEffectIsAppliedTo = entityEffectIsAppliedTo.GetComponent<NetworkObject>().NetworkObjectId;

			for (int i = 0; i < AssetDatabase.Database.statusEffects.Count; i++)
			{
				if (statusEffect == AssetDatabase.Database.statusEffects[i])
					SyncSetUpStatusEffectRpc(casterId, entityIdEffectIsAppliedTo, i);
			}
		}
		else
			SetUpStatusEffect(casterStats, entityEffectIsAppliedTo, statusEffect);
	}

	[Rpc(SendTo.Everyone)]
	private void SyncSetUpStatusEffectRpc(ulong casterId, ulong entityIdEffectIsAppliedTo, int statusEffectIndex)
	{
		EntityStats casterStats = NetworkManager.SpawnManager.SpawnedObjects[casterId].GetComponent<EntityStats>();
		EntityStats entityEffectIsAppliedTo = NetworkManager.SpawnManager.SpawnedObjects[entityIdEffectIsAppliedTo].GetComponent<EntityStats>();
		SOStatusEffects statusEffect = AssetDatabase.Database.statusEffects[statusEffectIndex];

		SetUpStatusEffect(casterStats, entityEffectIsAppliedTo, statusEffect);
	}
	private void SetUpStatusEffect(EntityStats casterInfo, EntityStats entityEffectIsAppliedTo, SOStatusEffects statusEffect)
	{
		this.casterInfo = casterInfo;
		this.entityEffectIsAppliedTo = entityEffectIsAppliedTo;
		this.statusEffect = statusEffect;

		transform.SetParent(entityEffectIsAppliedTo.transform);
		transform.localPosition = Vector3.zero;

		gameObject.name = statusEffect.Name + "Effect";
		damage = (int)(statusEffect.effectValue * Utilities.GetLevelModifier(casterInfo.entityLevel));
		timerTillNextDamage = 0f;

		this.entityEffectIsAppliedTo.AddStatusEffectValues(this);

		//add setup of particle effects for each status effect when i have something for them (atm all simple white particles)
	}

	//clear effect sync
	public void ClearStatusEffect()
	{
		if (MultiplayerManager.IsMultiplayer())
			ClearStatusEffectRpc();
		else
			ClearStatusEffectAndDestroy();
	}
	[Rpc(SendTo.Everyone)]
	private void ClearStatusEffectRpc()
	{
		ClearStatusEffectAndDestroy();
	}
	private void ClearStatusEffectAndDestroy()
	{
		entityEffectIsAppliedTo.RemoveStatusEffectValues(this);
		if (MultiplayerManager.IsClientHost())
			Destroy(gameObject);
	}

	//timers
	private void AbilityDurationTimer()
	{
		abilityDurationTimer += Time.deltaTime;

		if (abilityDurationTimer >= statusEffect.abilityDuration)
			ClearStatusEffect();
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
