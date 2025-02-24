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

		base.UpdateClass(newPlayerClass);
		GetComponent<PlayerInventoryHandler>().TrySpawnStartingItems(newPlayerClass);

		if (!MultiplayerManager.IsMultiplayer()) return;

		for (int i = 0; i < AssetDatabase.Database.classes.Count; i++)
		{
			if (newPlayerClass == AssetDatabase.Database.classes[i])
				SyncPlayerClassRpc(ClientManager.Instance.clientNetworkedId, i);
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

	//player stat/ability unlock events
	protected override void UnlockStatBoost(SOClassStatBonuses statBoost)
	{
		base.UnlockStatBoost(statBoost);
		UpdateClassTreeUi();
	}
	protected override void UnlockAbility(SOAbilities ability)
	{
		base.UnlockAbility(ability);
		UpdateClassTreeUi();
	}
	protected override void RefundStatBoost(SOClassStatBonuses statBoost)
	{
		base.RefundStatBoost(statBoost);
		UpdateClassTreeUi();
	}
	protected override void RefundAbility(SOAbilities ability)
	{
		base.RefundAbility(ability);
		UpdateClassTreeUi();
	}

	private void UpdateClassTreeUi()
	{
		if (PlayerClassesUi.Instance == null)
			Debug.LogError("ClassesUi component instance not set, ignore if intentional");
		else
			PlayerClassesUi.Instance.UpdateNodesInClassTree(GetComponent<EntityStats>());
	}
}
