using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEntityStats : EntityStats
{
	private BossEntityBehaviour bossBehaviour;

	[Header("Additional Boss Variables")]
	public Damageable damageable;
	public float lastBossHealthPercentage;
	public bool inPhaseTransition;

	public BossPhase bossPhase;
	public enum BossPhase
	{
		firstPhase, secondPhase, thirdPhase
	}

	public GameObject BossRoomCenterPiecePrefab;
	[HideInInspector] public GameObject roomCenterPiece;

	protected override void Awake()
	{
		base.Awake();
		bossBehaviour = GetComponent<BossEntityBehaviour>();
		damageable = GetComponent<Damageable>();
		SpawnBossRoomCenterPiece();
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

	private void SpawnBossRoomCenterPiece()
	{
		GameObject go = Instantiate(BossRoomCenterPiecePrefab);
		roomCenterPiece = go;
		roomCenterPiece.transform.position = transform.position;
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
	}
}
