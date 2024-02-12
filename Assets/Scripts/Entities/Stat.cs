using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
	public int baseValue;
	public int finalValue;

	public int equipmentValue;
	public float equipmentPercentageValue;
	public List<float> percentageBonuses = new List<float>();

	public void UpdateEquipmentValue(int modifier)
	{
		equipmentValue = modifier;
		CalcFinalValue();
	}
	public void UpdateEquipmentPercentageValue(float modifier)
	{
		equipmentPercentageValue = modifier;
		CalcFinalValue();
	}

	public void CalcFinalValue()
	{
		float percentageValue = equipmentPercentageValue + 1;
		percentageBonuses.ForEach(x => percentageValue += x);

		finalValue = (int)(baseValue + equipmentValue * percentageValue);
	}
}
