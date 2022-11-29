using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCoin : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Material platformMaterial;
    private bool isMoving = false;

    private void OnEnable() {
        EventManager.StartListening(Events.OnStartLevel, StartMoving);
    }

    private void OnDisable() {
        EventManager.StopListening(Events.OnStartLevel, StartMoving);
    }

    void Update()
    {
        platformMaterial.SetVector("_Pos", transform.position);
        if (isMoving){
            transform.Translate(Vector3.left * speed * Time.deltaTime);
        }
    }

    private void StartMoving(Dictionary<string, object> message)
    {
        isMoving = true;
    }
}
