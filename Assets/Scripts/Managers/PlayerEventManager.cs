using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerEventManager
{
	/// <summary>
	/// GAME EVENTS
	/// </summary>
	public static Action<EntityStats> OnPlayerLevelUpEvent;
	public static void PlayerLevelUp(EntityStats playerStats)
	{
		OnPlayerLevelUpEvent?.Invoke(playerStats);
	}

	//player death events
	public static event Action<GameObject> OnPlayerDeathEvent;
	public static void PlayerDeath(GameObject obj, DamageSourceInfo damageSourceInfo)
	{
		OnPlayerDeathEvent?.Invoke(obj);
		GetPlayerDeathMessaage?.Invoke(Utilities.GetPlayerDeathMessage(damageSourceInfo));
		OnShowPlayerDeathUiEvent?.Invoke();
	}
	public static event Action<string> GetPlayerDeathMessaage;
	public static event Action OnShowPlayerDeathUiEvent; //invoked from PlayerDeath()

	/// <summary>
	/// UI EVENTS
	/// </summary>
	public static Action<int> OnGoldAmountChange;
	public static void GoldAmountChange(int gold)
	{
		OnGoldAmountChange?.Invoke(gold);
	}

	public static event Action<int, int> OnPlayerHealthChangeEvent;
	public static void PlayerHealthChange(int max, int current)
	{
		OnPlayerHealthChangeEvent?.Invoke(max, current);
	}

	public static event Action<int, int> OnPlayerManaChangeEvent;
	public static void PlayerManaChange(int max, int current)
	{
		OnPlayerManaChangeEvent?.Invoke(max, current);
	}

	public static event Action<int, int> OnPlayerExpChangeEvent;
	public static void PlayerExpChange(int max, int current)
	{
		OnPlayerExpChangeEvent?.Invoke(max, current);
	}

	public static event Action<EntityStats> OnPlayerStatChangeEvent;
	public static void PlayerStatChange(EntityStats playerStats)
	{
		OnPlayerStatChangeEvent?.Invoke(playerStats);
	}

	//player ui
	public static event Action<GameObject, bool> OnDetectNewInteractedObject;
	public static void DetectNewInteractedObject(GameObject obj, bool showText)
	{
		OnDetectNewInteractedObject?.Invoke(obj, showText);
	}

	public static event Action OnShowPlayerInventoryEvent;
	public static void ShowPlayerInventory()
	{
		OnShowPlayerInventoryEvent?.Invoke();
	}
	public static event Action OnShowPlayerClassSelectionEvent;
	public static void ShowPlayerClassSelection()
	{
		if (Application.isEditor) //allow class swapping in all scenes when open in editor
		{
			OnShowPlayerClassSelectionEvent?.Invoke();

			if (GameManager.Instance == null)
				Debug.LogWarning("Game Manager instance not found, ignore if testing scene");
		}
		else //allow class swapping only in hub area
		{
			if (GameManager.Instance != null && Utilities.GetCurrentlyActiveScene(GameManager.Instance.hubAreaName))
				OnShowPlayerClassSelectionEvent?.Invoke();
			else
				Debug.LogWarning("Game Manager instance not found, ignore if testing scene");
		}
	}
	public static event Action OnShowPlayerSkillTreeEvent;
	public static void ShowPlayerSkillTree()
	{
		OnShowPlayerSkillTreeEvent?.Invoke();
	}
	public static event Action OnShowPlayerLearntAbilitiesEvent;
	public static void ShowPlayerLearntAbilities()
	{
		OnShowPlayerLearntAbilitiesEvent?.Invoke();
	}
	public static event Action OnShowPlayerJournalEvent;
	public static void ShowPlayerJournal()
	{
		OnShowPlayerJournalEvent?.Invoke();
	}

	//portal ui
	public static event Action<PortalHandler> OnShowPortalUi;
	public static void ShowPortalUi(PortalHandler portal)
	{
		OnShowPortalUi?.Invoke(portal);
	}
	public static event Action OnHidePortalUi;
	public static void HidePortalUi()
	{
		OnHidePortalUi?.Invoke();
	}

	//npcs ui
	public static event Action<NpcHandler> OnShowNpcJournal;
	public static void ShowNpcJournal(NpcHandler npc)
	{
		OnShowNpcJournal?.Invoke(npc);
	}
	public static event Action<NpcHandler> OnHideNpcJournal;
	public static void HideNpcJournal(NpcHandler npc)
	{
		OnHideNpcJournal?.Invoke(npc);
	}
	public static event Action<NpcHandler> OnShowNpcShopInventory;
	public static void ShowNpcShopInventory(NpcHandler npc)
	{
		OnShowNpcShopInventory?.Invoke(npc);
	}
	public static event Action<NpcHandler> OnHideNpcShopInventory;
	public static void HideNpcShopInventory(NpcHandler npc)
	{
		OnHideNpcShopInventory?.Invoke(npc);
	}
}
