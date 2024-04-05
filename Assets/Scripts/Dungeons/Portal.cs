using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour, IInteractable
{
	public GameObject portalSpriteObj;
	public bool isDungeonEnterencePortal;

	private void Start()
	{
		Initilize();
	}
	private void Initilize()
	{
		portalSpriteObj = transform.GetChild(0).gameObject;
	}

	private void Update()
	{
		PortalSpinEffect();
		PortalWobbleEffect();
	}

	public void Interact(PlayerController playerController)
	{

	}

	public void UnInteract(PlayerController playerController)
	{

	}

	//add a portal spin and wobble effect at some point via code or animator
	private void PortalSpinEffect()
	{

	}
	private void PortalWobbleEffect()
	{

	}
}
