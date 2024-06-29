using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalHandler : MonoBehaviour, IInteractable
{
	public GameObject portalSpriteObj;
	public bool isDungeonEnterencePortal;

	private void Update()
	{
		PortalSpinEffect();
		PortalWobbleEffect();
	}

	public void Interact(PlayerController playerController)
	{
		EventManager.ShowPortalUi(this);
	}
	public void UnInteract(PlayerController playerController)
	{
		EventManager.HidePortalUi();
	}

	//add a portal spin and wobble effect at some point via code or animator
	private void PortalSpinEffect()
	{

	}
	private void PortalWobbleEffect()
	{

	}
}
