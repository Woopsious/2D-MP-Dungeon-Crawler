using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemsScriptableObject", menuName = "Entities/BossBehaviours")]
public class SOBossEntityBehaviour : SOEntityBehaviour
{
	[Header("Boss Abilities")]
	public SOBossAbilities abilityOne;
	public SOBossAbilities abilityTwo;
	public SOBossAbilities abilityThree;

	[Header("Boss Mark Player")]
	public SOAbilities markPlayerAbility;

	[Tooltip("optional for bosses with transition abilities")]
	public List<SOAbilities> specialBossAbilities = new List<SOAbilities>();
}
