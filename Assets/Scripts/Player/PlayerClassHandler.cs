using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerClassHandler : EntityClassHandler
{
	private PlayerController playerController;

	private void OnEnable()
	{
		playerController = GetComponent<PlayerController>();
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

	//sync pre-existing player obj info to newly joined clients
	[Rpc(SendTo.Server, RequireOwnership = false)]
	public void SyncInfoToNewlyJoinedClientRpc(ulong clientId)
	{
		SyncInfoToNewlyJoinedClientRpc(GetIndexOfClass(currentEntityClass), GetIndexesOfStatBoosts(), RpcTarget.Single(clientId, RpcTargetUse.Temp));
	}
	[Rpc(SendTo.SpecifiedInParams)]
	private void SyncInfoToNewlyJoinedClientRpc(int playerClassIndex, int[] playerStatBoostIndexs, RpcParams rpcParams)
	{
		base.UpdateClass(AssetDatabase.Database.classes[playerClassIndex]);

		foreach (int statBoostIndex in playerStatBoostIndexs)
			base.UnlockStatBoost(AssetDatabase.Database.classStatBoosts[statBoostIndex]);
	}

	//player class events
	protected void UpdateClass(PlayerController player, SOClasses newClass)
	{
		if (player != GameManager.Localplayer) return;

		if (MultiplayerManager.IsMultiplayer())
			SyncPlayerClassRpc(ClientManager.Instance.clientNetworkedId, GetIndexOfClass(newClass));
		else
			base.UpdateClass(newClass);
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
		if (GameManager.Localplayer != entityStats.playerRef) return;

		if (MultiplayerManager.IsMultiplayer())
			SyncUnlockStatBoostRpc(ClientManager.Instance.clientNetworkedId, GetIndexOfStatBoost(statBoost));
		else
			base.UnlockStatBoost(statBoost);

		UpdateClassTreeUi();
	}

	//sync unlocks
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
		if (GameManager.Localplayer != entityStats.playerRef) return;

		if (MultiplayerManager.IsMultiplayer())
			SyncRefundStatBoostRpc(ClientManager.Instance.clientNetworkedId, GetIndexOfStatBoost(statBoost));
		else
			base.RefundStatBoost(statBoost);

		UpdateClassTreeUi();
	}

	//sync refunds
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

		PlayerClassesUi.Instance.UpdateNodesInClassTree(playerController);
	}

	//helpers
	protected int GetIndexOfClass(SOClasses entityClass)
	{
		for (int i = 0; i < AssetDatabase.Database.classes.Count; i++)
		{
			if (entityClass == AssetDatabase.Database.classes[i])
				return i;
		}

		Debug.LogError("failed to get class index");
		return 0;
	}
	protected int[] GetIndexesOfStatBoosts()
	{
		int[] playerStatBoostIndexs = new int[unlockedStatBoostList.Count];

		for (int i = 0; i < unlockedStatBoostList.Count; i++)
			playerStatBoostIndexs[i] = GetIndexOfStatBoost(unlockedStatBoostList[i]);

		return playerStatBoostIndexs;
	}
	protected int GetIndexOfStatBoost(SOClassStatBonuses statBoost)
	{
		for (int i = 0; i < AssetDatabase.Database.classStatBoosts.Count; i++)
		{
			if (statBoost == AssetDatabase.Database.classStatBoosts[i])
				return i;
		}

		Debug.LogError("failed to get stat boost index");
		return 0;
	}
}
