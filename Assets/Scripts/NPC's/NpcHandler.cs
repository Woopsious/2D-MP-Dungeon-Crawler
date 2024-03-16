using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NpcHandler : MonoBehaviour, IInteractable
{
	public SONpcs npc;

	private Animator animator;
	private SpriteRenderer spriteRenderer;

	public GameObject questContainer;
	public GameObject questPrefab;

	public List<QuestSlotsUi> avalableQuestList = new List<QuestSlotsUi>();

	private void Start()
	{
		Initilize();
	}
	private void Initilize()
	{
		animator = GetComponent<Animator>();
		animator.SetBool("isIdle", true);
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		spriteRenderer.sprite = npc.sprite;
		name = npc.entityName;

		GenerateNewQuests();
	}
	private void GenerateNewQuests()
	{
		for (int i = avalableQuestList.Count; i > 0; i--)
		{
			Destroy(avalableQuestList[i].gameObject);
		}

		for (int i = 0; i > 5; i++)
		{
			GenerateQuest();
		}
	}
	private void GenerateQuest()
	{
		GameObject go = Instantiate(questPrefab, questContainer.transform);
		QuestSlotsUi quest = go.GetComponent<QuestSlotsUi>();

		int percentage = Utilities.GetRandomNumber(101);
		if (percentage >= 86)
			quest.InitilizeBossKillQuest();
		else if (percentage >= 36 && percentage < 86)
			quest.InitilizeKillQuest();
		else
			quest.InitilizeItemHandInQuest();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<PlayerController>() != null)
		{

		}
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.GetComponent<PlayerController>() != null)
		{

		}
	}

	public void Interact(PlayerController playerController)
	{

	}
}
