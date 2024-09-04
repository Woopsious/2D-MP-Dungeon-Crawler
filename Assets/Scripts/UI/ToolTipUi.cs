using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ToolTipUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public string tipToShow;
	private readonly float timeToWait = 0.5f;

	//mouse events
	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button.ToString() != "Right") return;

		ToolTipManager.OnMouseRightClick(GetComponent<InventoryItemUi>(), eventData.position);
	}
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
	//delay tooltip
	public IEnumerator StartTimer(PointerEventData eventData)
	{
		yield return new WaitForSeconds(timeToWait);
		ShowMessage(eventData);
	}
	public void UpdatePlayerToolTip()
	{
		if (GetComponent<ClassTreeNodeUi>() != null)
			GetComponent<ClassTreeNodeUi>().SetToolTip(PlayerInfoUi.playerInstance.GetComponent<EntityStats>());
		if (GetComponent<Items>() != null)
			GetComponent<Items>().SetToolTip(PlayerInfoUi.playerInstance.GetComponent<EntityStats>());
		if (GetComponent<Abilities>() != null)
			GetComponent<Abilities>().SetToolTip(PlayerInfoUi.playerInstance.GetComponent<EntityStats>());
	}
}
