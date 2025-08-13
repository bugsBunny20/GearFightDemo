using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool GameStarted { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartGame()
    {
        GameStarted = true;
        Debug.Log("Game Started — filler now active!");
    }

    public void StopGame()
    {
        GameStarted = false;
    }
}