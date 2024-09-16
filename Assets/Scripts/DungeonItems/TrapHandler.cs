using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrapHandler : MonoBehaviour, IInteractables
{
	public SpriteRenderer spriteRenderer;
	private AudioHandler audioHandler;

	[Header("Trap Info")]
	public SOTraps trapBaseRef;
	public bool trapDetected;
	public bool trapDisabled;
	public bool trapActivated;

	public int trapLevel;
	public float levelModifier;

	public GameObject playerDetectionCollider;

	[Header("Trap Damage")]
	public int trapDamage;

	[Header("players")] //track players already trying to detect trap
	public List<PlayerController> playersCheckedTrap = new List<PlayerController>();

	[Header("Shared audio")]
	public AudioClip trapDetectedSfx;
	public AudioClip trapDeactivatedSfx;

	private void Awake()
	{
		Initilize();
	}

	private void OnEnable()
	{
		PlayerEventManager.OnPlayerLevelUpEvent += UpdateTrapLevel;
	}
	private void OnDisable()
	{
		PlayerEventManager.OnPlayerLevelUpEvent -= UpdateTrapLevel;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.GetComponent<PlayerController>() != null)
		{
			TryDetectTrap(collision.GetComponent<PlayerController>());
		}
	}

	private void Initilize()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		audioHandler = GetComponent<AudioHandler>();
		playerDetectionCollider.GetComponent<EntityDetection>().trapHandler = this;

		spriteRenderer.sprite = null;
		name = trapBaseRef.name;
		trapDetected = false;
		trapDisabled = false;
		trapActivated = false;
	}
	private void UpdateTrapLevel(EntityStats playerStats)
	{
		trapLevel = playerStats.entityLevel;
		levelModifier = Utilities.GetLevelModifier(trapLevel);

		trapDamage = (int)(trapBaseRef.baseDamage * levelModifier);
	}

	//trap actions
	public void ActivateTrap(PlayerController player, Collider2D coll)
	{
		if (trapDisabled || trapActivated) return;

		player.GetComponent<Damageable>().OnHitFromDamageSource(null, coll, trapDamage, (IDamagable.DamageType)trapBaseRef.baseDamageType, 
			0, false, false, true); //apply damage

		if (trapBaseRef.hasEffects) //apply effects
			player.playerStats.ApplyNewStatusEffects(trapBaseRef.statusEffects, player.playerStats);

		Debug.Log("trap activated");

		trapActivated = true;
		spriteRenderer.sprite = trapBaseRef.trapSpriteActivated;
		audioHandler.PlayAudio(trapBaseRef.trapActivatedSfx);
	}
	private void TryDetectTrap(PlayerController newPlayer)
	{
		if (trapDisabled || trapActivated) return;

		bool playerAlreadyTried = false; //check if player already rolled for detect
		foreach (PlayerController player in playersCheckedTrap)
		{
			if (newPlayer == player)
			{
				playerAlreadyTried = true;
				Debug.Log("same player detected breaking loop");
				break;
			}
		}
		if (playerAlreadyTried) return;

		if (!RollPlayerDetectChance(newPlayer)) //roll for detect
		{
			Debug.Log("trap not detected");
			return; //failed detect
		}

		//detect trap
		Debug.Log("trap detected");

		trapDetected = true;
		spriteRenderer.sprite = trapBaseRef.trapSpriteActivated;
		audioHandler.PlayAudio(trapDetectedSfx);
	}
	private bool RollPlayerDetectChance(PlayerController newPlayer)
	{
		playersCheckedTrap.Add(newPlayer);

		if (newPlayer.playerClassHandler.currentEntityClass.trapDetectionChance >= trapBaseRef.trapDetectionDifficulty)
			return true;
		else return false;
	}
	private void DeactivateTrap()
	{
		if (trapDisabled || trapActivated) return;

		Debug.Log("trap deactivated");

		trapDisabled = true;
		audioHandler.PlayAudio(trapDeactivatedSfx);
		StartCoroutine(DisableObject(trapDeactivatedSfx.length));
	}

	private IEnumerator DisableObject(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		gameObject.SetActive(false);
	}

	//interacts
	public void Interact(PlayerController player)
	{
		if (trapDetected && !trapActivated)
			DeactivateTrap();
	}
	public void UnInteract(PlayerController player)
	{

	}
}
