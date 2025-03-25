using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
	public static SceneHandler Instance;

	private void Awake()
	{
		Instance = this;

		if (GameManager.Instance == null)
			StartCoroutine(LoadMainScene());
	}

	private IEnumerator LoadMainScene()
	{
		AsyncOperation asyncLoadScene = SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);

		while (!asyncLoadScene.isDone)
			yield return null;
	}
}
