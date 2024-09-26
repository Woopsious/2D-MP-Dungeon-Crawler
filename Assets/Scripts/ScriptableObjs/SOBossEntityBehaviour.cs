using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemsScriptableObject", menuName = "Entities/BossBehaviours")]
public class SOBossEntityBehaviour : SOEntityBehaviour
{
	[Header("Boss Abilities")]
	public SOClassAbilities abilityOne;
	public SOClassAbilities abilityTwo;
	public SOClassAbilities abilityThree;
}
