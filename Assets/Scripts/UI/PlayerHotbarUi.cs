using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHotbarUi : MonoBehaviour
{
	[SerializeField]
	public static PlayerHotbarUi Instance;

	[Header("Hotbar")]
	public GameObject HotbarPanelUi;
	public List<GameObject> ConsumableSlots = new List<GameObject>();
	public GameObject consumableSlotOne;
	public GameObject consumableSlotTwo;

	public List<GameObject> AbilitySlots = new List<GameObject>();
	public GameObject abilitySlotOne;
	public GameObject abilitySlotTwo;
	public GameObject abilitySlotThree;
	public GameObject abilitySlotFour;
	public GameObject abilitySlotFive;

	[Header("ExpBar")]
	public Image expBarFiller;

	[Header("HealthBar")]
	public Image healthBarFiller;

	[Header("ManaBar")]
	public Image manaBarFiller;

	public void Awake()
	{
		Instance = this;
	}

	public void OpenInventoryButton()
	{
		PlayerInventoryUi.Instance.ShowInventory();
	}
	public void OpenClassSelectionButton()
	{
		ClassesUi.Instance.ShowPlayerClassSelection();
	}
	public void OpenClassSkillTreeButton()
	{
		ClassesUi.Instance.ShowClassSkillTree(ClassesUi.Instance.currentPlayerClass);
	}

	public void OnExperienceChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		expBarFiller.fillAmount = percentage;
	}

	public void OnHealthChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		healthBarFiller.fillAmount = percentage;
	}

	public void OnManaChange(int MaxValue, int currentValue)
	{
		float percentage = (float)currentValue / MaxValue;
		manaBarFiller.fillAmount = percentage;
	}
}
