using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject StartCanvas;
    private void OnEnable() {
        EventManager.StartListening(Events.OnStartLevel, StartLevel);
    }

    private void OnDisable() {
        EventManager.StopListening(Events.OnStartLevel, StartLevel);
    }

    private void StartLevel(Dictionary<string, object> message)
    {
        StartCanvas.SetActive(false);
    }

}
