using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCoin : MonoBehaviour
{
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private Material platformMaterial;
    private bool isMoving = false;
    private List<Coin> collectedCoins;

    private void Awake() {
        collectedCoins = new List<Coin>();
    }

    private void OnEnable() {
        EventManager.StartListening(Events.OnStartLevel, StartMoving);
    }

    private void OnDisable() {
        EventManager.StopListening(Events.OnStartLevel, StartMoving);
    }

    private void Update()
    {
        platformMaterial.SetVector("_Pos", transform.position);
        if (isMoving){
            float horizontalInput = Input.GetAxis("Horizontal");
            Vector3 movementDirection = new Vector3(verticalSpeed * Time.deltaTime, 0f, -horizontalInput * horizontalSpeed * Time.deltaTime);
            transform.Translate(movementDirection, Space.World);
            if (movementDirection != Vector3.zero){
                movementDirection = new Vector3(movementDirection.z, 0f, -movementDirection.x);
                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.down);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
            }
        }
    }

    private void StartMoving(Dictionary<string, object> message)
    {
        isMoving = true;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Coin"){
            collectedCoins.Add(other.gameObject.GetComponent<Coin>());
            other.gameObject.AddComponent<CoinMovement>();
            if (collectedCoins.Count == 1)
                other.gameObject.GetComponent<CoinMovement>().ConnectedCoin = transform;
            else
                other.gameObject.GetComponent<CoinMovement>().ConnectedCoin = collectedCoins[collectedCoins.Count - 2].transform;
        }
    }
}
