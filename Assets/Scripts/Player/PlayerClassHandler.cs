using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerClassHandler : EntityClassHandler
{
	private void OnEnable()
	{
		PlayerClassesUi.OnClassChanges += UpdateClass;
		PlayerClassesUi.OnNewStatBonusUnlock += UnlockStatBoost;
		PlayerClassesUi.OnNewAbilityUnlock += UnlockAbility;
		PlayerClassesUi.OnRefundStatBonusUnlock += RefundStatBoost;
		PlayerClassesUi.OnRefundAbilityUnlock += RefundAbility;
	}
	private void OnDisable()
	{
		PlayerClassesUi.OnClassChanges -= UpdateClass;
		PlayerClassesUi.OnNewStatBonusUnlock -= UnlockStatBoost;
		PlayerClassesUi.OnNewAbilityUnlock -= UnlockAbility;
		PlayerClassesUi.OnRefundStatBonusUnlock -= RefundStatBoost;
		PlayerClassesUi.OnRefundAbilityUnlock -= RefundAbility;
	}

	//player class events
	protected override void UpdateClass(SOClasses newPlayerClass)
	{
		if (entityStats.playerRef != GameManager.Localplayer) return;

		if (MultiplayerManager.IsMultiplayer())
		{
			for (int i = 0; i < AssetDatabase.Database.classes.Count; i++)
			{
				if (newPlayerClass == AssetDatabase.Database.classes[i])
					SyncPlayerClassRpc(ClientManager.Instance.clientNetworkedId, i);
			}
		}
		else
		{
			base.UpdateClass(newPlayerClass);
			GetComponent<PlayerInventoryHandler>().TrySpawnStartingItems(newPlayerClass);
		}
	}
	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SyncPlayerClassRpc(ulong OwnerClientIdToMatch, int newPlayerClassIndex)
	{
		SyncPlayerClass(OwnerClientIdToMatch, newPlayerClassIndex);
	}
	private void SyncPlayerClass(ulong OwnerClientIdToMatch, int newPlayerClassIndex)
	{
		if (OwnerClientIdToMatch != OwnerClientId) return;
		base.UpdateClass(AssetDatabase.Database.classes[newPlayerClassIndex]);
	}

	//player stat unlock/refund events
	protected override void UnlockStatBoost(SOClassStatBonuses statBoost)
	{
		if (entityStats.playerRef != GameManager.Localplayer) return;

		if (MultiplayerManager.IsMultiplayer())
		{
			for (int i = 0; i < AssetDatabase.Database.classStatBoosts.Count; i++)
			{
				if (statBoost == AssetDatabase.Database.classStatBoosts[i])
					SyncUnlockStatBoostRpc(ClientManager.Instance.clientNetworkedId, i);
			}
		}
		else
			base.UnlockStatBoost(statBoost);

		UpdateClassTreeUi();
	}

	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SyncUnlockStatBoostRpc(ulong OwnerClientIdToMatch, int newStatBoostIndex)
	{
		SyncUnlockStatBoost(OwnerClientIdToMatch, newStatBoostIndex);
	}
	private void SyncUnlockStatBoost(ulong OwnerClientIdToMatch, int newStatBoostIndex)
	{
		if (OwnerClientIdToMatch != OwnerClientId) return;
		base.UnlockStatBoost(AssetDatabase.Database.classStatBoosts[newStatBoostIndex]);
	}

	protected override void RefundStatBoost(SOClassStatBonuses statBoost)
	{
		if (entityStats.playerRef != GameManager.Localplayer) return;

		if (MultiplayerManager.IsMultiplayer())
		{
			for (int i = 0; i < AssetDatabase.Database.classStatBoosts.Count; i++)
			{
				if (statBoost == AssetDatabase.Database.classStatBoosts[i])
					SyncRefundStatBoostRpc(ClientManager.Instance.clientNetworkedId, i);
			}
		}
		else
			base.RefundStatBoost(statBoost);

		UpdateClassTreeUi();
	}

	[Rpc(SendTo.Everyone, RequireOwnership = false)]
	private void SyncRefundStatBoostRpc(ulong OwnerClientIdToMatch, int refundStatBoostIndex)
	{
		SyncRefundStatBoost(OwnerClientIdToMatch, refundStatBoostIndex);
	}
	private void SyncRefundStatBoost(ulong OwnerClientIdToMatch, int refundStatBoostIndex)
	{
		if (OwnerClientIdToMatch != OwnerClientId) return;
		base.RefundStatBoost(AssetDatabase.Database.classStatBoosts[refundStatBoostIndex]);
	}

	//player ability unlock/refund events
	protected override void UnlockAbility(SOAbilities ability)
	{
		base.UnlockAbility(ability);
		UpdateClassTreeUi();
	}
	protected override void RefundAbility(SOAbilities ability)
	{
		base.RefundAbility(ability);
		UpdateClassTreeUi();
	}

	private void UpdateClassTreeUi()
	{
		if (MultiplayerManager.IsMultiplayer() && OwnerClientId != ClientManager.Instance.clientNetworkedId) return;

		PlayerClassesUi.Instance.UpdateNodesInClassTree(entityStats);
	}
}
