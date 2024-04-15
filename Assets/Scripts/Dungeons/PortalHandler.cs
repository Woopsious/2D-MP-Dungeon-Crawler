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
		//may have a confirm teleport button pop up on interact instead of instantly teleporting

		//EventManager.AutoSaveGame();

		if (isDungeonEnterencePortal)
		{
			int i = Utilities.GetRandomNumber(SceneManager.sceneCountInBuildSettings - 2); //(not including hub and main menu scene)
			if (i == 0)
			{
				EventManager.ShowPortalUi(this);
				//GameManager.Instance.LoadDungeonOne();
				//StartCoroutine(EventManager.ChangeSceneAsync(EventManager.dungeonLayoutOneName, false));
			}
			else if (i == 1)
			{
				EventManager.ShowPortalUi(this);
				//GameManager.Instance.LoadDungeonTwo();
				//StartCoroutine(EventManager.ChangeSceneAsync(EventManager.dungeonLayoutTwoName, false));
			}
		}
		else
		{
			EventManager.ShowPortalUi(this);
			//GameManager.Instance.LoadHubArea(false);
			//StartCoroutine(EventManager.ChangeSceneAsync(EventManager.hubAreaName, false));
		}
	}

	public void UnInteract(PlayerController playerController)
	{

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
