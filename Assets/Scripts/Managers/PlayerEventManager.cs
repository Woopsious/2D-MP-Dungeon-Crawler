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
	public enum PlayerDeathType
	{
		dungeonSpDeath, bossDungeonSpDeath, dungeonMpDeath, bossDungeonMpDeath
	}

	public static event Action<GameObject, PlayerDeathType, string> OnPlayerDeathEvent;
	public static void PlayerDeath(GameObject playerObj, PlayerDeathType playerDeathType, string deathMessage)
	{
		OnPlayerDeathEvent?.Invoke(playerObj, playerDeathType, deathMessage);
		OnShowPlayerDeathUiEvent?.Invoke();
	}
	public static event Action OnShowPlayerDeathUiEvent; //invoked from PlayerDeath() used to hide all other ui elements

	public static event Action<GameObject> OnRevivePlayerEvent;
	public static void RevivePlayer(GameObject playerObj)
	{
		OnRevivePlayerEvent?.Invoke(playerObj);
	}

	public static event Action OnReviveAllPlayersEvent;
	public static void ReviveAllPlayers()
	{
		OnReviveAllPlayersEvent?.Invoke();
	}

	/// <summary>
	/// UI EVENTS
	/// </summary>
	public static Action<int> OnGoldAmountChange;
	public static void GoldAmountChange(int gold)
	{
		OnGoldAmountChange?.Invoke(gold);
	}

	public static event Action<int, int> OnPlayerExpChangeEvent;
	public static void PlayerExpChange(int max, int current)
	{
		OnPlayerExpChangeEvent?.Invoke(max, current);
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

	public static event Action<EntityStats> OnPlayerStatChangeEvent;
	public static void PlayerStatChange(EntityStats playerStats)
	{
		OnPlayerStatChangeEvent?.Invoke(playerStats);
	}
	public static event Action<AbilityStatusEffect> OnPlayerStatusEffectChange;
	public static void PlayerStatusEffectChange(AbilityStatusEffect statusEffect)
	{
		OnPlayerStatusEffectChange?.Invoke(statusEffect);
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
		//allow class changes in editor when ever
		if (Application.isEditor || Utilities.SceneIsActive(GameManager.Instance.hubScene))
			OnShowPlayerClassSelectionEvent?.Invoke();
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
