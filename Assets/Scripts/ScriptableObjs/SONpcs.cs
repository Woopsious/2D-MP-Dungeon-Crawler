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
		isQuestNpc
	}
}
