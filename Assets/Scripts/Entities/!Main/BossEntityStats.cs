using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEntityStats : EntityStats
{
	private BossEntityBehaviour bossBehaviour;

	[Header("Additional Boss Variables")]
	[HideInInspector] public Damageable damageable;
	public float lastBossHealthPercentage;
	public bool inPhaseTransition;

	public BossPhase bossPhase;
	public enum BossPhase
	{
		firstPhase, secondPhase, thirdPhase
	}

	[HideInInspector] public GameObject roomCenterPiece;

	public static event Action OnBossDeath;

	protected override void Awake()
	{
		base.Awake();
		bossBehaviour = GetComponent<BossEntityBehaviour>();
		damageable = GetComponent<Damageable>();
	}
	protected override void Start()
	{
		base.Start();
		inPhaseTransition = true;
		bossBehaviour.EventSpawnBossAdds(2); //spawn adds at the start
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

	public void SetCenterPieceRef(GameObject roomCenterPiece)
	{
		this.roomCenterPiece = roomCenterPiece;
	}
	private void UpdateBossHealthPercentage(int maxHealth, int currentHealth)
	{
		float newPercentage = (float)currentHealth / maxHealth;

		if (lastBossHealthPercentage >= 0.66 && newPercentage < 0.66 || lastBossHealthPercentage >= 0.33 && newPercentage < 0.33)
		{
			bossPhase++;
			bossBehaviour.EventSpawnBossAdds(2); //spawn adds for evey new phase reached
			inPhaseTransition = true;
		}
		lastBossHealthPercentage = newPercentage;

		if (newPercentage <= 0)
			OnBossDeath?.Invoke();
	}
}
