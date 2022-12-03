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
    private float initialScale = 200f;

    private void Awake() {
        collectedCoins = new List<Coin>();
    }

    private void OnEnable() {
        EventManager.StartListening(Events.OnStartLevel, StartMoving);
        EventManager.StartListening(Events.OnLoseCoin, DropCoin);
        EventManager.StartListening(Events.OnFailLevel, DropAllCoins);
    }

    private void OnDisable() {
        EventManager.StopListening(Events.OnStartLevel, StartMoving);
        EventManager.StopListening(Events.OnLoseCoin, DropCoin);
        EventManager.StopListening(Events.OnFailLevel, DropAllCoins);
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
            else{
                col.gameObject.GetComponent<CoinMovement>().ConnectedCoin = collectedCoins[collectedCoins.Count - 2].transform;
                col.gameObject.transform.position = collectedCoins[collectedCoins.Count - 2].transform.position;
            }

            col.gameObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // to prevent coin rotation at the beginning

            if (isScaling){
                StopCoroutine(ScaleUpAndDown());
                isScaling = false;
            }
            StartCoroutine(ScaleUpAndDown());
        }
    }

    private IEnumerator ScaleUpAndDown(){
        isScaling = true;
        float newScale = initialScale;
        transform.localScale = new Vector3(newScale, newScale, newScale);
        while (newScale < initialScale + scalingIncrease && isScaling){
            newScale += 1;
            transform.localScale = new Vector3(newScale, newScale, newScale);
            yield return new WaitForSeconds(0.001f);
        }
        while (newScale > initialScale && isScaling){
            newScale -= 1;
            transform.localScale = new Vector3(newScale, newScale, newScale);
            yield return new WaitForSeconds(0.001f);
        }
        isScaling = false;
    }

    private void DropCoin(Dictionary<string, object> message)
    {
        var tf = message["transform"];
        if (transform != null){
            Transform tfCoin = (Transform)tf;
            int coinIndex = tfCoin.GetSiblingIndex();
            if (transform.childCount > coinIndex + 1){
                Transform nextCoin = collectedCoins[coinIndex].GetComponent<CoinMovement>().ConnectedCoin;
                collectedCoins[coinIndex + 1].GetComponent<CoinMovement>().ConnectedCoin = nextCoin;
                collectedCoins[coinIndex].GetComponent<CoinMovement>().ConnectedCoin = null;
                collectedCoins[coinIndex].GetComponent<CoinMovement>().DropCoin();
            }else{
                collectedCoins[coinIndex].GetComponent<CoinMovement>().ConnectedCoin = null;
                collectedCoins[coinIndex].GetComponent<CoinMovement>().DropCoin();
            }
        }
    }

    private void DropAllCoins(Dictionary<string, object> message)
    {
        throw new NotImplementedException();
    }

}
