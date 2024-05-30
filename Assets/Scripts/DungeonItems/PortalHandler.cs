using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalHandler : MonoBehaviour, IInteractable
{
	public GameObject portalSpriteObj;
	public TMP_Text interactWithText;
	public bool isDungeonEnterencePortal;

	private void Start()
	{
		Initilize();
	}
	private void Initilize()
	{
		interactWithText.transform.SetParent(FindObjectOfType<Canvas>().transform);
		interactWithText.transform.SetAsFirstSibling();
		interactWithText.text = $"F to interact";
	}

	private void Update()
	{
		UpdateInteractText();
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

	private void UpdateInteractText()
	{
		if (!interactWithText.gameObject.activeInHierarchy) return;
		interactWithText.transform.position =
			Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + 1f, 0));
	}

	//add a portal spin and wobble effect at some point via code or animator
	private void PortalSpinEffect()
	{

	}
	private void PortalWobbleEffect()
	{

	}
}
