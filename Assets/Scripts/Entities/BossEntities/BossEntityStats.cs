using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEntityStats : EntityStats
{

	[Header("Additional Boss Variables")]
	public Damageable damageable;
	public SpawnHandler spawner;
	public float lastBossHealthPercentage;

	public BossPhase bossPhase;
	public enum BossPhase
	{
		firstPhase, secondPhase, thirdPhase
	}

	protected override void Awake()
	{
		base.Awake();
		damageable = GetComponent<Damageable>();
	}
	protected override void Start()
	{
		base.Start();
	}
	protected override void OnEnable()
	{
		base.OnEnable();
		OnHealthChangeEvent += UpdateBossHealthPercentage;
	}
	protected override void OnDisable()
	{
		base.OnDisable();
		OnHealthChangeEvent -= UpdateBossHealthPercentage;
	}
	private void UpdateBossHealthPercentage(int maxHealth, int currentHealth)
	{
		float newPercentage = (float)currentHealth / maxHealth;

		if (lastBossHealthPercentage >= 0.66 && newPercentage < 0.66 || lastBossHealthPercentage >= 0.33 && newPercentage < 0.33)
		{
			BossEntityBehaviour bossBehaviour = (BossEntityBehaviour)entityBehaviour;
			bossPhase++;
			spawner.ForceSpawnEntitiesForBosses();

			if (statsRef.humanoidType == SOEntityStats.HumanoidTypes.isGoblin)
				bossBehaviour.ChangeState(bossBehaviour.goblinAttackState);
		}
		lastBossHealthPercentage = newPercentage;
	}
}
