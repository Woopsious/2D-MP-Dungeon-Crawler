using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEntityStats : EntityStats
{

	[Header("Additional Boss Variables")]
	public SpawnHandler spawner;
	public float lastBossHealthPercentage;

	public BossPhase bossPhase;
	public enum BossPhase
	{
		firstPhase, secondPhase, thirdPhase
	}

	private void OnEnable()
	{
		OnHealthChangeEvent += UpdateBossHealthPercentage;
	}
	private void OnDisable()
	{
		OnHealthChangeEvent -= UpdateBossHealthPercentage;
	}
	private void UpdateBossHealthPercentage(int maxHealth, int currentHealth)
	{
		float newPercentage = (float)currentHealth / maxHealth;

		if (lastBossHealthPercentage >= 0.66 && newPercentage < 0.66 || lastBossHealthPercentage >= 0.33 && newPercentage < 0.33)
		{
			//call transition state here
			spawner.ForceSpawnEntitiesForBosses();
		}
		lastBossHealthPercentage = newPercentage;
	}
}
