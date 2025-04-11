using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerEventManager
{
	/// <summary>
	/// GAME EVENTS
	/// </summary>
	public static Action<PlayerController> OnPlayerLevelChangeEvent;
	public static void PlayerLevelChange(PlayerController player)
	{
		OnPlayerLevelChangeEvent?.Invoke(player);
	}

	//player death event
	public static event Action<PlayerController, string> OnPlayerDeathEvent;
	public static void PlayerDeath(PlayerController player, string deathMessage)
	{
		OnPlayerDeathEvent?.Invoke(player, deathMessage);
		OnShowPlayerDeathUiEvent?.Invoke();
	}

	public static event Action OnShowPlayerDeathUiEvent; //invoked from PlayerDeath() hides all other ui elements

	//sync player revive ui timer events
	public static event Action<float> OnStartReviveTimerUiEvent; //also invoked from PlayerDeath()
	public static void SyncStartRevivePlayerUiTimerEvent(float respawnTimer)
	{
		OnStartReviveTimerUiEvent?.Invoke(respawnTimer);
	}
	public static event Action OnCancelReviveTimerUiEvent; //also invoked from PlayerDeath()
	public static void SyncCancelRevivePlayerUiTimerEvent()
	{
		OnCancelReviveTimerUiEvent?.Invoke();
	}

	//player Respawn events
	public static event Action OnRespawnAllPlayersEvent;
	public static void RespawnAllPlayers()
	{
		OnRespawnAllPlayersEvent?.Invoke();
		OnRespawnPlayerEvent?.Invoke(null, GameManager.Localplayer);
	}
	public static event Action<PlayerController, PlayerController> OnRespawnPlayerEvent;
	public static void RespawnPlayer(PlayerController optionalReviverPlayer, PlayerController revivedplayer)
	{
		OnRespawnPlayerEvent?.Invoke(optionalReviverPlayer, revivedplayer);
	}

	//player revive events
	public static event Action<GameObject> OnStartRevivePlayerEvent;
	public static void StartRevivePlayerEvent(GameObject playerObj)
	{
		OnStartRevivePlayerEvent?.Invoke(playerObj);
	}
	public static event Action<GameObject> OnCancelRevivePlayerEvent;
	public static void CancelRevivePlayerEvent(GameObject playerObj)
	{
		OnCancelRevivePlayerEvent?.Invoke(playerObj);
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
	public static event Action<Interactables, bool, string> OnDetectNewInteractedObject;
	public static void DetectNewInteractedObject(Interactables interactable, bool showText, string message)
	{
		OnDetectNewInteractedObject?.Invoke(interactable, showText, message);
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
		//allow skill changes in editor when ever
		if (Application.isEditor || Utilities.SceneIsActive(GameManager.Instance.hubScene))
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

	public static event Action OnShowPlayerCodexEvent;
	public static void ShowPlayerCodex()
	{
		OnShowPlayerCodexEvent?.Invoke();
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
