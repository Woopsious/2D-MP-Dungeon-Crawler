using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalHandler : MonoBehaviour, IInteractables
{
	[Header("Portal Info")]
	private AudioHandler audioHandler;
	public GameObject portalSpriteObj;

	[Header("Portal Type")]
	public PortalType portalType;
	public enum PortalType
	{
		isDungeonEnterencePortal, isDungeonExitPortal, isBossDungeonExitPortal
	}

	[Header("Portal Sound Settings")]
	public AudioClip portalSfx;
	private readonly float portalSoundCooldown = 5f;
	private float portalSoundTimer;
	private readonly int chanceOfPortalSound = 15;

	private void Awake()
	{
		audioHandler = GetComponent<AudioHandler>();
	}

	private void Start()
	{
		if (portalType != PortalType.isBossDungeonExitPortal)
		{
			if (!DungeonHandler.Instance.dungeonPortalsList.Contains(gameObject))
				Debug.LogError("Portal not added to dungeon portal list, ensure all of this type are added");
		}
		else
		{
			if (DungeonHandler.Instance.dungeonPortalsList.Contains(gameObject))
				Debug.LogError("Portal added to dungeon portal list, ensure non of this type is added");
		}
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
