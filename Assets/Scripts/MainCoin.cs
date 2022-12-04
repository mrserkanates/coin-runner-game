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
    private Rigidbody rb;
    private bool isMoving = false;
    private List<Transform> collectedCoins;
    private bool isScaling = false;
    private float initialScale = 200f;

    private void Awake() {
        collectedCoins = new List<Transform>();
        rb = GetComponent<Rigidbody>();
        transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 180f, 0f);
    }

    private void OnEnable() {
        EventManager.StartListening(Events.OnStartNewLevel, Reset);
        EventManager.StartListening(Events.OnStartNewLevel, StartMoving);
        EventManager.StartListening(Events.OnStartLevel, StartMoving);
        EventManager.StartListening(Events.OnHitCoin, DropCoin);
        EventManager.StartListening(Events.OnHitMainCoin, DropAllCoins);
        EventManager.StartListening(Events.OnFinishLevel, TakeCoinsToStairs);
    }

    private void OnDisable() {
        EventManager.StopListening(Events.OnStartNewLevel, Reset);
        EventManager.StopListening(Events.OnStartNewLevel, StartMoving);
        EventManager.StopListening(Events.OnStartLevel, StartMoving);
        EventManager.StopListening(Events.OnHitCoin, DropCoin);
        EventManager.StopListening(Events.OnHitMainCoin, DropAllCoins);
        EventManager.StopListening(Events.OnFinishLevel, TakeCoinsToStairs);
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

            if (transform.position.y < -8f){ // if coin falls from road
                EventManager.TriggerEvent(Events.OnFallMainCoin, new Dictionary<string, object>(){});
                EventManager.TriggerEvent(Events.OnFailLevel, new Dictionary<string, object>(){});
            }
        }
    }

    private void StartMoving(Dictionary<string, object> message)
    {
        isMoving = true;
    }
    
    private void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Uncollected"){
            collectedCoins.Add(col.gameObject.transform);
            col.gameObject.AddComponent<CoinMovement>();
            col.gameObject.tag = "Collected";

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
            int coinIndex = GetSiblingIndexByTransform(tfCoin);
            if (coinIndex != -1){
                if (coinIndex + 1 < collectedCoins.Count){
                    Transform nextCoin = collectedCoins[coinIndex].GetComponent<CoinMovement>().ConnectedCoin;
                    collectedCoins[coinIndex + 1].GetComponent<CoinMovement>().ConnectedCoin = nextCoin;
                }
                collectedCoins[coinIndex].GetComponent<CoinMovement>().DropCoin();
                collectedCoins[coinIndex].tag = "Uncollected";
                collectedCoins.RemoveAt(coinIndex);
            }else{
                Debug.Log("Coin not found in collected coin list");
            }
        }
    }

    private int GetSiblingIndexByTransform(Transform tf){
        for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
            if (collectedCoins[coinIndex].transform == tf){
                return coinIndex;
            }
        }
        return -1;
    }

    private void DropAllCoins(Dictionary<string, object> message)
    {
        isMoving = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForceAtPosition(transform.forward * 2f, transform.position + Vector3.down * 0.3f, ForceMode.Impulse);
        for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
            collectedCoins[coinIndex].GetComponent<CoinMovement>().DropCoin();
        }
    }

    private void TakeCoinsToStairs(Dictionary<string, object> message){
        isMoving = false;
        var tfRoad = message["tfRoad"];
        if (tfRoad != null){
            StartCoroutine(DoCompletionAnimation((Transform)tfRoad));
        }
    }

    private IEnumerator DoCompletionAnimation(Transform tfRoad){
        rb.MoveRotation(Quaternion.identity * Quaternion.Euler(0f, 180f, 0f));
        while (Vector3.Distance(transform.position, tfRoad.position) > 1f){
            rb.MovePosition(Vector3.Lerp(transform.position, tfRoad.position + Vector3.up * 0.8f, 0.8f));
            yield return null;
        }
        Debug.Log("ORTAYA GELDI");
        RemoveMovementComponents();
        StartCoroutine(ElevateCoins());
    }

    private IEnumerator ElevateCoins(){
        float elevateTime = 2f;
        float currentTime = 0.0f;
        for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
            collectedCoins[coinIndex].rotation = Quaternion.identity * Quaternion.Euler(0f, 180f, 0f);
            collectedCoins[coinIndex].GetComponent<Rigidbody>().freezeRotation = true;
            collectedCoins[coinIndex].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezePosition;
        }
        while (currentTime < elevateTime){
            for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
                collectedCoins[coinIndex].position = Vector3.Lerp(collectedCoins[coinIndex].position,
                    transform.position + Vector3.up * 0.6f * (coinIndex + 1), elevateTime * Time.deltaTime);
            }
            currentTime += Time.deltaTime;
            yield return null;
        }
        for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
            collectedCoins[coinIndex].position = transform.position + Vector3.up * 0.6f * (coinIndex + 1);
        }
        Debug.Log("YUKSELDI");
    }

    private void RemoveMovementComponents(){
        for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
            collectedCoins[coinIndex].tag = "Uncollected";
            Destroy(collectedCoins[coinIndex].GetComponent<CoinMovement>());
        }
    }

    private void Reset(Dictionary<string, object> message){
        transform.position = Vector3.zero + Vector3.up * 2f;
        transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 180f, 0f);
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

}
