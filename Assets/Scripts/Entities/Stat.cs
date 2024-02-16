using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
	public bool isPercentageStat;
	public int baseValue;
	public int finalValue;
	public float finalPercentageValue;

	public int equipmentValue;
	public float equipmentPercentageValue;
	public List<float> percentageBonuses = new List<float>();

	public void SetBaseValue(int value)
	{
		baseValue = value;
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
			float percentageValue = equipmentPercentageValue + baseValue;
			percentageBonuses.ForEach(x => percentageValue += x);
			Debug.LogWarning("value: " + percentageValue);
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
