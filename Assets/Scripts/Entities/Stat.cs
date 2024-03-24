using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
	public bool isPercentageStat;
	public int baseValue;
	public int finalValue;
	public float basePercentageValue;
	public float finalPercentageValue;

	public int equipmentValue;
	public float equipmentPercentageValue;
	public List<float> percentageBonuses = new List<float>();

	public void SetBaseValue(float value)
	{
		if (isPercentageStat)
			basePercentageValue = value;
		else
			baseValue = (int)value;
		CalcFinalValue();
	}

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

	public void AddPercentageValue(float modifier)
	{
		if (modifier != 0)
			percentageBonuses.Add(modifier);

		CalcFinalValue();
	}
	public void RemovePercentageValue(float modifier)
	{
		if (modifier != 0)
			percentageBonuses.Remove(modifier);

		CalcFinalValue();
	}

	public void CalcFinalValue()
	{
		if (isPercentageStat)
		{
			float percentageValue = basePercentageValue;
			percentageBonuses.ForEach(x => percentageValue += x);
			finalPercentageValue = equipmentPercentageValue + percentageValue;
		}
		else
		{
			float percentageValue = equipmentPercentageValue + 1;
			percentageBonuses.ForEach(x => percentageValue += x);
			finalValue = (int)((baseValue + equipmentValue) * percentageValue);
		}
	}
}
