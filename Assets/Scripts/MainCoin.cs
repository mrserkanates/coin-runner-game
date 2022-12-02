using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCoin : MonoBehaviour
{
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float scalingIncrease;
    [SerializeField] private Material platformMaterial;
    private bool isMoving = false;
    private List<Coin> collectedCoins;
    private bool isScaling = false;

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
            Vector3 movementDirection = movementDirection = new Vector3(verticalSpeed * Time.deltaTime, 0f, -horizontalInput * horizontalSpeed * Time.deltaTime);
            transform.Translate(movementDirection, Space.World);
            if (movementDirection != Vector3.zero){
                movementDirection = new Vector3(movementDirection.z, 0f, -movementDirection.x);
                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
            }else{
                movementDirection = new Vector3(0f, 0f, -movementDirection.x);
                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
            }
        }
    }

    private void StartMoving(Dictionary<string, object> message)
    {
        isMoving = true;
    }
    
    private void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Coin"){
            collectedCoins.Add(col.gameObject.GetComponent<Coin>());
            col.gameObject.AddComponent<CoinMovement>();
            if (collectedCoins.Count == 1)
                col.gameObject.GetComponent<CoinMovement>().ConnectedCoin = transform;
            else
                col.gameObject.GetComponent<CoinMovement>().ConnectedCoin = collectedCoins[collectedCoins.Count - 2].transform;
            if (isScaling){
                StopCoroutine(ScaleUpAndDown());
                isScaling = false;
            }
            StartCoroutine(ScaleUpAndDown());
        }
    }

    private IEnumerator ScaleUpAndDown(){
        isScaling = true;
        float newScale = 200;
        transform.localScale = new Vector3(newScale, newScale, newScale);
        while (newScale < 200 + scalingIncrease && isScaling){
            newScale += 1;
            transform.localScale = new Vector3(newScale, newScale, newScale);
            yield return new WaitForSeconds(0.001f);
        }

        while (newScale > 200 && isScaling){
            newScale -= 1;
            transform.localScale = new Vector3(newScale, newScale, newScale);
            yield return new WaitForSeconds(0.001f);
        }
        isScaling = false;
    }
}
