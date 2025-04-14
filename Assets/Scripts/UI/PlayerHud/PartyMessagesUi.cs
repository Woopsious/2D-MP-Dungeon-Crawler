using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyMessagesUi : MonoBehaviour
{
	public TMP_Text message;
	private float timer;

	public void SetMessage(string message)
	{
		this.message.text = message;
	}
	public void SetMessage(string message, float timeTillMessageRemoved)
	{
		this.message.text = message;
		timer = timeTillMessageRemoved;

		StartCoroutine(RemoveMessageTimer());
	}

	private IEnumerator RemoveMessageTimer()
	{
		yield return new WaitForSeconds(timer);
		Destroy(gameObject);
	}
}
