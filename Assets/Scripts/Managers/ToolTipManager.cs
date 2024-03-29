using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToolTipManager : MonoBehaviour
{
	public RectTransform tipWindow;
	public TextMeshProUGUI tipText;

	public static Action<string, Vector2> OnMouseHover;
	public static Action OnMouseLoseFocus;

	private void OnEnable()
	{
		OnMouseHover += ShowTip;
		OnMouseLoseFocus += HideTip;

		EventManagerUi.OnShowPlayerInventoryEvent += HideTip;
		EventManagerUi.OnShowPlayerClassSelectionEvent += HideTip;
		EventManagerUi.OnShowPlayerSkillTreeEvent += HideTip;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent += HideTip;
		EventManagerUi.OnShowPlayerJournalEvent += HideTip;
	}

	private void OnDisable()
	{
		OnMouseHover -= ShowTip;
		OnMouseLoseFocus -= HideTip;

		EventManagerUi.OnShowPlayerInventoryEvent -= HideTip;
		EventManagerUi.OnShowPlayerClassSelectionEvent -= HideTip;
		EventManagerUi.OnShowPlayerSkillTreeEvent -= HideTip;
		EventManagerUi.OnShowPlayerLearntAbilitiesEvent -= HideTip;
		EventManagerUi.OnShowPlayerJournalEvent -= HideTip;
	}

	private void ShowTip(string tip, Vector2 mousePos)
	{
		tipText.text = tip;
		tipWindow.sizeDelta = new Vector2(tipText.preferredWidth > 300 ? 300 : 
			tipText.preferredWidth * 1.25f, tipText.preferredHeight * 1.25f);

		tipWindow.transform.position = new Vector2(mousePos.x + 25 + tipWindow.sizeDelta.x / 2, mousePos.y);
		tipWindow.gameObject.SetActive(true);
	}

	private void HideTip()
	{
		tipText.text = null;
		tipWindow.gameObject.SetActive(false);
	}
}
