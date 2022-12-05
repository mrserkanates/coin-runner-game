using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera runnerCam;
    [SerializeField] private Transform mainCoin;
    [SerializeField] private GameObject MovementCollider;
    [SerializeField] private GameObject StartCanvas;
    [SerializeField] private GameObject FailCanvas;
    [SerializeField] private GameObject WinCanvas;

    private void Start() {
        EventManager.TriggerEvent(Events.OnCreateNewLevel, new Dictionary<string, object>(){});
    }

    private void OnEnable() {
        EventManager.StartListening(Events.OnCreateNewLevel, CameraFollow);
        EventManager.StartListening(Events.OnCreateNewLevel, ShowStartCanvas);
        EventManager.StartListening(Events.OnStartLevel, StartLevel);
        EventManager.StartListening(Events.OnFailLevel, ShowFailCanvas);
        EventManager.StartListening(Events.OnFallMainCoin, CameraUnfollow);
        EventManager.StartListening(Events.OnWinLevel, ShowWinCanvas);
    }

    private void OnDisable() {
        EventManager.StopListening(Events.OnCreateNewLevel, CameraFollow);
        EventManager.StopListening(Events.OnCreateNewLevel, ShowStartCanvas);
        EventManager.StopListening(Events.OnStartLevel, StartLevel);
        EventManager.StopListening(Events.OnFailLevel, StartLevel);
        EventManager.StopListening(Events.OnFallMainCoin, CameraUnfollow);
        EventManager.StopListening(Events.OnWinLevel, ShowWinCanvas);
    }

#region PUBLIC_FUNCTIONS
    public void OnPressNewLevelButton(){
        EventManager.TriggerEvent(Events.OnCreateNewLevel, new Dictionary<string, object>(){});
    }

#endregion 

#region PRIVATE_FUNCTIONS
    private void StartLevel(Dictionary<string, object> message)
    {
        StartCanvas.SetActive(false);
        MovementCollider.SetActive(false);
    }

    private void ShowStartCanvas(Dictionary<string, object> message)
    {
        StartCanvas.SetActive(true);
        MovementCollider.SetActive(true);
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

#endregion

}
