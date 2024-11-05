using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactables : MonoBehaviour
{
	private BossRoomHandler bossDungeonHandler;
	private TrapHandler trapHandler;
	private PortalHandler portalHandler;
	private ChestHandler chestHandler;
	private NpcHandler npcHandler;
	private EnchantmentHandler enchantmentHandler;

	private void Awake()
	{
		bossDungeonHandler = GetComponent<BossRoomHandler>();
		trapHandler = GetComponent<TrapHandler>();
		portalHandler = GetComponent<PortalHandler>();
		chestHandler = GetComponent<ChestHandler>();
		npcHandler = GetComponent<NpcHandler>();
		enchantmentHandler = GetComponent<EnchantmentHandler>();
	}

	public void Interact(PlayerController player)
	{
        if (bossDungeonHandler != null)
			bossDungeonHandler.Interact(player);
        else if (trapHandler != null)
			trapHandler.Interact(player);
        else if (portalHandler != null)
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
	void Interact(PlayerController player);
	void UnInteract(PlayerController player);
}
