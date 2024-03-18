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
	public void GenerateNewQuests()
	{
		for (int i = avalableQuestList.Count; i > 0; i--)
		{
			if (!avalableQuestList[i - 1].isCurrentlyActiveQuest)
				Destroy(avalableQuestList[i - 1].gameObject);
		}
		avalableQuestList.Clear();

		for (int i = 0; i < 5; i++)
			GenerateQuest();
	}
	public void GenerateQuest()
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

		avalableQuestList.Add(quest);
	}

	public void RefreshThisNpcsQuests() //function delegated to button in ui
	{
		GenerateNewQuests();
		MoveQuestsToUi();
	}

	public void Interact(PlayerController player)
	{
		player.isInteractingWithSomething = true;
		PlayerJournalUi.Instance.ShowPlayerJournal();
		PlayerJournalUi.Instance.ShowNpcJournal();
		PlayerJournalUi.Instance.refreshNpcQuestsButton.onClick.AddListener(delegate { RefreshThisNpcsQuests(); } );
		PlayerJournalUi.Instance.closeAvalableQuestsButton.onClick.AddListener(delegate { UnInteract(player); } );
		MoveQuestsToUi();
	}
	public void UnInteract(PlayerController player)
	{
		player.isInteractingWithSomething = false;
		PlayerJournalUi.Instance.HidePlayerJournal();
		PlayerJournalUi.Instance.HideNpcJournal();
		PlayerJournalUi.Instance.refreshNpcQuestsButton.onClick.RemoveAllListeners();
		PlayerJournalUi.Instance.closeAvalableQuestsButton.onClick.RemoveAllListeners();
		MoveQuestsToContainer();
	}

	private void MoveQuestsToUi()
	{
		foreach (QuestSlotsUi quest in avalableQuestList)
			quest.transform.SetParent(PlayerJournalUi.Instance.avalableQuestContainer.transform);
	}
	private void MoveQuestsToContainer()
	{
		foreach (QuestSlotsUi quest in avalableQuestList)
			quest.transform.SetParent(questContainer.transform);
	}
}
