using UnityEngine;

[CreateAssetMenu(fileName = "EntityStatsScriptableObject", menuName = "Entities/Stats")]
public class SOEntityStats : ScriptableObject
{
	public string entityName;

	[Header("health")]
	public int maxHealth;

	[Header("Resistances")]
	public int physicalDamageResistance;
	public int poisonDamageResistance;
	public int fireDamageResistance;
	public int iceDamageResistance;

	[Header("Speed")]
	public float navMeshMoveSpeed;
	public int navMeshTurnSpeed;

	[Header("Mana")]
	public int maxMana;
}
