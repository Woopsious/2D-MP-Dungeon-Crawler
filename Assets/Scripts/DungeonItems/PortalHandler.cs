using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalHandler : MonoBehaviour, IInteractables
{
	private AudioHandler audioHandler;
	public GameObject portalSpriteObj;
	public bool isDungeonEnterencePortal;

	[Header("Portal Sound Settings")]
	public AudioClip portalSfx;
	private readonly float portalSoundCooldown = 5f;
	private float portalSoundTimer;
	private readonly int chanceOfPortalSound = 25;

	private void Awake()
	{
		audioHandler = GetComponent<AudioHandler>();
	}

	private void Update()
	{
		PortalSpinEffect();
		PortalWobbleEffect();
		PlayPortalSound();
	}

	//player interactions
	public void Interact(PlayerController player)
	{
		PlayerEventManager.ShowPortalUi(this);
		player.isInteractingWithInteractable = true;
	}
	public void UnInteract(PlayerController player)
	{
		PlayerEventManager.HidePortalUi();
		player.isInteractingWithInteractable = false;
	}

	//add a portal spin and wobble effect at some point via code or animator
	private void PortalSpinEffect()
	{

	}
	private void PortalWobbleEffect()
	{

	}
	private void PlayPortalSound()
	{
		portalSoundTimer -= Time.deltaTime;

		if (portalSoundTimer <= 0)
		{
			portalSoundTimer = portalSoundCooldown;
			if (chanceOfPortalSound > Utilities.GetRandomNumber(100))
				audioHandler.PlayAudio(portalSfx);
		}
	}
}
