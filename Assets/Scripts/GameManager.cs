using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera runnerCam;
    [SerializeField] private Transform mainCoin;
    [SerializeField] private GameObject StartCanvas;
    [SerializeField] private GameObject FailCanvas;
    [SerializeField] private GameObject WinCanvas;

    private void Start() {
        EventManager.TriggerEvent(Events.OnStartNewLevel, new Dictionary<string, object>(){});
    }

    private void OnEnable() {
        EventManager.StartListening(Events.OnStartNewLevel, CameraFollow);
        EventManager.StartListening(Events.OnStartLevel, StartLevel);
        EventManager.StartListening(Events.OnFailLevel, ShowFailCanvas);
        EventManager.StartListening(Events.OnFallMainCoin, CameraUnfollow);
        EventManager.StartListening(Events.OnWinLevel, ShowWinCanvas);
    }

    private void OnDisable() {
        EventManager.StopListening(Events.OnStartNewLevel, CameraFollow);
        EventManager.StopListening(Events.OnStartLevel, StartLevel);
        EventManager.StopListening(Events.OnFailLevel, StartLevel);
        EventManager.StopListening(Events.OnFallMainCoin, CameraUnfollow);
        EventManager.StopListening(Events.OnWinLevel, ShowWinCanvas);
    }

    public void OnPressNewLevelButton(){
        EventManager.TriggerEvent(Events.OnStartNewLevel, new Dictionary<string, object>(){});
    }

    private void StartLevel(Dictionary<string, object> message)
    {
        StartCanvas.SetActive(false);
    }

    private void ShowFailCanvas(Dictionary<string, object> message)
    {
        FailCanvas.SetActive(true);
    }

    private void ShowWinCanvas(Dictionary<string, object> message)
    {
        WinCanvas.SetActive(true);
    }

    private void CameraFollow(Dictionary<string, object> message){
        runnerCam.Follow = mainCoin;
    }

    private void CameraUnfollow(Dictionary<string, object> message){
        runnerCam.Follow = null;
    }

}
