using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnchantmentHandler : MonoBehaviour, IInteractables
{
	private SpriteRenderer spriteRenderer;
	private AudioHandler audioHandler;



	[Header("Enchantment Sound Settings")]
	public AudioClip audioSfx;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		audioHandler = GetComponent<AudioHandler>();
	}

	//player interactions
	public void Interact(PlayerController player)
	{
		audioHandler.PlayAudio(audioSfx);
		PlayerEventManager.ShowPlayerInventory();
		PlayerInventoryUi.Instance.ShowEnchanterUi();
		player.isInteractingWithInteractable = true;
	}
	public void UnInteract(PlayerController player)
	{
		audioHandler.PlayAudio(audioSfx);
		PlayerEventManager.ShowPlayerInventory();
		PlayerInventoryUi.Instance.HideEnchanterUi();
		player.isInteractingWithInteractable = false;
	}
}
