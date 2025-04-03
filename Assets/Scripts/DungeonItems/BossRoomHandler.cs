using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomHandler : MonoBehaviour, IInteractables
{
	public static BossRoomHandler Instance;

	//private bool bossFightStarted;
	//private bool bossFightCompleted;

	public GameObject roomCenterPiece;
	private CircleCollider2D centerPieceCollider;
	public GameObject roomBarrier;

	public SpawnHandler roomSpawnHandler;
	public PortalHandler roomRespawnPortal;
	public PortalHandler roomExitPortal;

	private BossRoomState bossRoomState;
	public enum BossRoomState
	{
		bossNotReached, bossInactive, bossActive, bossDead
	}

	public static event Action<GameObject> OnStartBossFight;

	private void Awake()
	{
		Instance = this;
		bossRoomState = BossRoomState.bossNotReached;
		centerPieceCollider = GetComponent<CircleCollider2D>();
		roomBarrier.SetActive(false);
		roomRespawnPortal.gameObject.SetActive(false);
		roomExitPortal.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		PlayerEventManager.OnPlayerDeathEvent += ResetRoom;
		BossEntityStats.OnBossDeath += OnBossDeath;
	}
	private void OnDisable()
	{
		PlayerEventManager.OnPlayerDeathEvent -= ResetRoom;
		BossEntityStats.OnBossDeath -= OnBossDeath;
	}

	//respawning players
	public Vector3 GetPositionOfPortalToRespawnAt(PlayerController playerToRevive)
	{
		if (bossRoomState == BossRoomState.bossDead)
			return roomExitPortal.transform.position;
		else if (bossRoomState == BossRoomState.bossInactive)
			return roomRespawnPortal.transform.position;
		else
			return DungeonHandler.Instance.GetStartingPortalForBossRoom(); //enterence portal only portal in list
	}

	//boss room state updates
	private void StartBossFight()
	{
		centerPieceCollider.enabled = false;
		bossRoomState = BossRoomState.bossActive;
		roomBarrier.SetActive(true);
		roomRespawnPortal.gameObject.SetActive(true);
	}
	private void OnBossDeath()
	{
		//unlock room, enable exit portal
		bossRoomState = BossRoomState.bossDead;
		roomBarrier.SetActive(false);
		roomExitPortal.gameObject.SetActive(true);
	}

	public void CheckResetRoomOnClientDisconnect()
	{
		if (!PlayerDeathUi.Instance.PlayerPartyWiped()) return;
		ResetRoom(null, null); //func doesnt need args
	}
	private void ResetRoom(PlayerController player, string deathMessage)
	{
		if (!PlayerDeathUi.Instance.PlayerPartyWiped()) return;

		bossRoomState = BossRoomState.bossInactive;
		centerPieceCollider.enabled = true;
		roomBarrier.SetActive(false);

		roomSpawnHandler.ForceCleanUpBossRoomEntities();
	}

	public BossRoomState GetBossRoomState()
	{
		return bossRoomState;
	}

	//player interactions
	public void Interact(PlayerController player)
	{
		if (bossRoomState == BossRoomState.bossDead || bossRoomState == BossRoomState.bossActive) return;

		OnStartBossFight?.Invoke(gameObject);
		StartBossFight();
	}
	public void UnInteract(PlayerController player)
	{
		return; //noop
	}
}
