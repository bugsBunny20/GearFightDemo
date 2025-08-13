using UnityEngine;
using GearSystem;

public class StartButton : MonoBehaviour
{
    public void OnStartButtonPressed()
    {
        GameManager.Instance.StartGame();
    }
}