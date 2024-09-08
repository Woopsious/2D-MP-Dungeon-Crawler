using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactables : MonoBehaviour
{
	PortalHandler portalHandler;
	ChestHandler chestHandler;
	NpcHandler npcHandler;
	EnchantmentHandler enchantmentHandler;

	private void Awake()
	{
		portalHandler = GetComponent<PortalHandler>();
		chestHandler = GetComponent<ChestHandler>();
		npcHandler = GetComponent<NpcHandler>();
		enchantmentHandler = GetComponent<EnchantmentHandler>();
	}

	public void Interact(PlayerController player)
	{
		if (portalHandler != null)
		{
			if (DungeonPortalUi.instance.portalPanelUi.activeInHierarchy)
				portalHandler.UnInteract(player);
            else
				portalHandler.Interact(player);
		}
		else if (chestHandler != null)
		{
			if (PlayerInventoryUi.Instance.interactedInventorySlotsUi.activeInHierarchy)
				chestHandler.UnInteract(player);
			else
				chestHandler.Interact(player);
		}
		else if (npcHandler != null)
		{
			if (PlayerJournalUi.Instance.npcJournalPanalUi.activeInHierarchy || PlayerInventoryUi.Instance.npcShopPanalUi.activeInHierarchy)
				npcHandler.UnInteract(player);
			else
				npcHandler.Interact(player);
		}
		else if (enchantmentHandler != null)
		{
			if (PlayerInventoryUi.Instance.EnchanterUi.activeInHierarchy)
				enchantmentHandler.UnInteract(player);
			else
				enchantmentHandler.Interact(player);
		}
	}
}

public interface IInteractables
{
	public void Interact(PlayerController player);
	public void UnInteract(PlayerController player);
}
