using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public string tipToShow;
	private float timeToWait = 0.5f;

	public void OnPointerEnter(PointerEventData eventData)
	{
		StopAllCoroutines();
		StartCoroutine(StartTimer(eventData));
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		StopAllCoroutines();
		ToolTipManager.OnMouseLoseFocus();
	}

	private void ShowMessage(PointerEventData eventData)
	{
		ToolTipManager.OnMouseHover(tipToShow, eventData.position);
	}
	public IEnumerator StartTimer(PointerEventData eventData)
	{
		yield return new WaitForSeconds(timeToWait);
		ShowMessage(eventData);
	}
	public void UpdatePlayerToolTip()
	{
		if (GetComponent<Items>() != null)
			GetComponent<Items>().SetToolTip(PlayerInfoUi.playerInstance.GetComponent<EntityStats>());
	}
}
