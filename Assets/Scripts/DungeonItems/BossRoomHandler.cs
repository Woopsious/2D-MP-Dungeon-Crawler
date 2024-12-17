using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomHandler : MonoBehaviour, IInteractables
{
	public static BossRoomHandler Instance;

	private bool respawnPortalUnlocked;
	private bool bossFightStarted;
	private bool bossFightCompleted;

	public GameObject roomCenterPiece;
	private CircleCollider2D centerPieceCollider;
	public GameObject roomBarrier;

	public SpawnHandler roomSpawnHandler;
	public PortalHandler roomRespawnPortal;
	public PortalHandler roomExitPortal;

	public static event Action<GameObject> OnStartBossFight;

	private void Awake()
	{
		Instance = this;
		respawnPortalUnlocked = false;
		bossFightStarted = false;
		bossFightCompleted = false;
		centerPieceCollider = GetComponent<CircleCollider2D>();
		roomBarrier.SetActive(false);
		roomRespawnPortal.gameObject.SetActive(false);
		roomExitPortal.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		BossEntityStats.OnBossDeath += OnBossDeath;
	}
	private void OnDisable()
	{
		BossEntityStats.OnBossDeath -= OnBossDeath;
	}

	//respawning players
	public void RespawnPlayerAtPortal(GameObject playerObj)
	{
		if (respawnPortalUnlocked)
			playerObj.transform.position = roomRespawnPortal.transform.position;
		else
			DungeonHandler.Instance.RespawnPlayerAtClosestPortal(playerObj);
	}

	//boss room states
	private void StartBossFight()
	{
		centerPieceCollider.enabled = false;
		respawnPortalUnlocked = true;
		bossFightStarted = true;
		roomBarrier.SetActive(true);
		roomRespawnPortal.gameObject.SetActive(true);
	}
	private void OnBossDeath()
	{       
		//unlock room, enable exit portal
		bossFightCompleted = true;
		roomBarrier.SetActive(false);
		roomExitPortal.gameObject.SetActive(true);
	}
	public void ResetRoom()
	{
		bossFightStarted = false;
		bossFightCompleted = false;
		centerPieceCollider.enabled = true;
		roomBarrier.SetActive(false);

		roomSpawnHandler.ForceClearAllEntities();

		//when all players dead, revive them at respawn portal, reset boss/adds + anything else that comes up
	}

	//player interactions
	public void Interact(PlayerController player)
	{
		if (bossFightCompleted || bossFightStarted) return;

		OnStartBossFight?.Invoke(gameObject);
		StartBossFight();
	}
	public void UnInteract(PlayerController player)
	{
		return; //noop
	}
}
