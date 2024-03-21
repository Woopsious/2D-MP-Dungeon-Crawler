using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcScriptableObject", menuName = "NPCs")]
public class SONpcs : ScriptableObject
{
	public string entityName;
	public Sprite sprite;

	public NPCType npcType;
	public enum NPCType
	{
		isQuestNpc, isShopNpc
	}

	public ShopType shopType;
	public enum ShopType
	{
		isntShop, isWeaponSmith, isArmorer, isGoldSmith, isGeneralStore
	}

	public List<SOItems> weaponSmithShopItems = new List<SOItems>();
	public List<SOItems> armorerShopItems = new List<SOItems>();
	public List<SOItems> goldSmithShopItems = new List<SOItems>();
	public List<SOItems>generalStoreItems = new List<SOItems>();
}
