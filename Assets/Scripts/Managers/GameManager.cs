using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Start()
    {
        Instance = this;
    }
	public void Update()
	{
		Debug.Log(Time.timeScale);
	}

	public void PauseGame()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient) return;
        Time.timeScale = 0f;
    }
	public void UnPauseGame()
	{
		if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient) return;
		Time.timeScale = 1.0f;
	}
}
