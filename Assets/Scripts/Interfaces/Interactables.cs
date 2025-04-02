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
	private PlayerController playerController;

	private InteractType interactType;

	public enum InteractType
	{
		bossDungeonSpawner, trap, portal, chest, npc, enchantmentTable, player
	}

	private void Awake()
	{
		bossDungeonHandler = GetComponent<BossRoomHandler>();
		trapHandler = GetComponent<TrapHandler>();
		portalHandler = GetComponent<PortalHandler>();
		chestHandler = GetComponent<ChestHandler>();
		npcHandler = GetComponent<NpcHandler>();
		enchantmentHandler = GetComponent<EnchantmentHandler>();
		playerController = GetComponent<PlayerController>();

		if (bossDungeonHandler != null)
			interactType = InteractType.bossDungeonSpawner;
		else if (trapHandler != null)
			interactType = InteractType.trap;
		else if (portalHandler != null)
			interactType = InteractType.portal;
		else if (chestHandler != null)
			interactType = InteractType.chest;
		else if (npcHandler != null)
			interactType = InteractType.npc;
		else if (enchantmentHandler != null)
			interactType = InteractType.enchantmentTable;
		else if (playerController != null)
			interactType = InteractType.player;
	}

	public void Interact(PlayerController player)
	{
        if (interactType == InteractType.bossDungeonSpawner)
			bossDungeonHandler.Interact(player);
        else if (interactType == InteractType.trap)
		{
			if (trapHandler.GetTrapState() == TrapHandler.TrapStates.detected)
				trapHandler.Interact(player);
		}
        else if (interactType == InteractType.portal)
		{
			if (MultiplayerManager.IsMultiplayer() && !MultiplayerManager.IsClientHost()) return; //joined clients cant interact

			if (DungeonPortalUi.instance.portalPanelUi.activeInHierarchy)
				portalHandler.UnInteract(player);
            else
				portalHandler.Interact(player);
		}
		else if (interactType == InteractType.chest)
		{
			if (PlayerInventoryUi.Instance.interactedInventorySlotsUi.activeInHierarchy)
				chestHandler.UnInteract(player);
			else
				chestHandler.Interact(player);
		}
		else if (interactType == InteractType.npc)
		{
			if (PlayerJournalUi.Instance.npcJournalPanalUi.activeInHierarchy || PlayerInventoryUi.Instance.npcShopPanalUi.activeInHierarchy)
				npcHandler.UnInteract(player);
			else
				npcHandler.Interact(player);
		}
		else if (interactType == InteractType.enchantmentTable)
		{
			if (PlayerInventoryUi.Instance.EnchanterUi.activeInHierarchy)
				enchantmentHandler.UnInteract(player);
			else
				enchantmentHandler.Interact(player);
		}
		else if (interactType == InteractType.player)
		{
			if (playerController.playerStats.IsEntityDead())
				playerController.StartReviveTimer(player);
		}
	}
	public void CancelInteract(PlayerController player)
	{
		if (interactType != InteractType.player) return;

		playerController.CancelReviveTimer(player);
	}
	public InteractType GetInteractableType()
	{
		return interactType;
	}
	public PlayerController GetPlayer()
	{
		return playerController;
	}
}

public interface IInteractables
{
	void Interact(PlayerController player);
	void UnInteract(PlayerController player);
}
