using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCoin : MonoBehaviour
{
    public const float COIN_RADIUS = 0.3f;
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float scalingIncrease;
    [SerializeField] private float rotateSpeed;
    private Rigidbody rb;
    private bool isMoving = false;
    private List<Transform> collectedCoins;
    private bool isScaling = false;
    private float initialScale;

    private void Awake() {
        collectedCoins = new List<Transform>();
        rb = GetComponent<Rigidbody>();
        transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 180f, 0f); // to prevent coin rotation at the beginning
        initialScale = transform.localScale.x;
    }

    private void OnEnable() {
        EventManager.StartListening(Events.OnCreateNewLevel, Reset);
        EventManager.StartListening(Events.OnStartLevel, StartMoving);
        EventManager.StartListening(Events.OnHitCoin, DropCoin);
        EventManager.StartListening(Events.OnHitMainCoin, DropAllCoins);
        EventManager.StartListening(Events.OnFallMainCoin, StopMoving);
        EventManager.StartListening(Events.OnFinishLevel, TakeCoinsToStairs);
    }

    private void OnDisable() {
        EventManager.StopListening(Events.OnCreateNewLevel, Reset);
        EventManager.StopListening(Events.OnStartLevel, StartMoving);
        EventManager.StopListening(Events.OnHitCoin, DropCoin);
        EventManager.StopListening(Events.OnHitMainCoin, DropAllCoins);
        EventManager.StopListening(Events.OnFallMainCoin, StopMoving);
        EventManager.StopListening(Events.OnFinishLevel, TakeCoinsToStairs);
    }

    private void Update()
    {
        if (isMoving){
            float horizontalInput = Input.GetAxis("Horizontal");
            // x axis is for vertical movements, z axis is for horizontal movements
            Vector3 movementDirection = new Vector3(verticalSpeed * Time.deltaTime,
                0f,
                -horizontalInput * horizontalSpeed * Time.deltaTime);

            transform.Translate(movementDirection, Space.World);

            Quaternion toRotation = Quaternion.LookRotation(new Vector3(movementDirection.z,
                0f,
                -movementDirection.x), Vector3.up); 

            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                toRotation,
                720 * Time.deltaTime); // rotate towards movement direction

            if (transform.position.y < -8f){ // if coin falls from road
                EventManager.TriggerEvent(Events.OnFallMainCoin, new Dictionary<string, object>(){});
                EventManager.TriggerEvent(Events.OnFailLevel, new Dictionary<string, object>(){});
            }
        }
    }

#region PUBLIC_FUNCTIONS

#endregion

#region PRIVATE_FUNCTIONS

    private void StartMoving(Dictionary<string, object> message)
    {
        isMoving = true;
    }

    private void StopMoving(Dictionary<string, object> message)
    {
        isMoving = false;
    }

    private int GetSiblingIndexByTransform(Transform tf){
        // returns sibling index of given transform in collectedCoins list
        for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
            if (collectedCoins[coinIndex].transform == tf){
                return coinIndex;
            }
        }
        return -1;
    }

    private void DropCoin(Dictionary<string, object> message)
    {
        // disconnects coin from queue and make it fall
        var tf = message["transform"]; // transform of coin that is gonna drop
        var isRemoveFromList = message["isRemoveFromList"];
        if (tf != null && isRemoveFromList != null){
            Transform tfCoin = (Transform)tf;
            int coinIndex = GetSiblingIndexByTransform(tfCoin);
            if (coinIndex != -1){ //
                if (coinIndex + 1 < collectedCoins.Count){
                    // if there is a coin after this coin, then connect next coin to this coin's connected coin
                    Transform nextCoin = collectedCoins[coinIndex].GetComponent<CoinMovement>().ConnectedCoin;
                    collectedCoins[coinIndex + 1].GetComponent<CoinMovement>().ConnectedCoin = nextCoin;
                }
                collectedCoins[coinIndex].GetComponent<CoinMovement>().DropCoin();
                collectedCoins[coinIndex].tag = "Uncollected";
                if ((bool)isRemoveFromList)
                    collectedCoins.RemoveAt(coinIndex);
            }else{
                Debug.Log("coin not found in collected coin list");
            }
        }else{
            Debug.Log("transform of coin not found in scene");
        }
    }

    private void DropAllCoins(Dictionary<string, object> message)
    {
        // drops all coins including main coin
        isMoving = false; // stops main coin movement
        DropMainCoin();
        for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
            collectedCoins[coinIndex].GetComponent<CoinMovement>().DropCoin();
        }
        collectedCoins.Clear();
    }

    private void DropMainCoin(){
        rb.constraints = RigidbodyConstraints.None; // removes constraints to make rigidbody impulse work
        rb.AddForceAtPosition(transform.forward * 2f, transform.position + Vector3.down * COIN_RADIUS, ForceMode.Impulse); // applies a force to make main coin fall
    }

    private void TakeCoinsToStairs(Dictionary<string, object> message){
        // does level completed animation
        isMoving = false; // stop player to control it manually
        var tfRoad = message["tfRoad"];
        if (tfRoad != null){
            StartCoroutine(DoCompletionAnimation((Transform)tfRoad)); // start animation chain
        }
    }

    private IEnumerator DoCompletionAnimation(Transform tfRoad){
        float movSpeed = 2f; // the movement speed of main coin when it goes towards center of last road segment
        rb.MoveRotation(Quaternion.identity * Quaternion.Euler(0f, 180f, 0f)); // correct main coins rotation

        DisconnectCoins(); // disconnect coins to move them independent by main coin

        // set constraints to make main coin move on X axis without rotating
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;

        for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
            collectedCoins[coinIndex].rotation = Quaternion.identity * Quaternion.Euler(0f, 180f, 0f); // correct rotation of coins
            // set constraints to make coins move on X axis without rotating
            collectedCoins[coinIndex].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation
                | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        }

        Vector3 centerOfRoad = tfRoad.position;
        centerOfRoad.y = transform.position.y;
        while (Vector3.Distance(transform.position, centerOfRoad) > 0.6f){
        // if distance between main coin and center of the last road segment is not close enough
            transform.position = Vector3.Lerp(transform.position,
                centerOfRoad,
                movSpeed * Time.deltaTime); // move main coin to center of the last road segment
            
            for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
                collectedCoins[coinIndex].position = Vector3.Lerp(collectedCoins[coinIndex].position,
                    centerOfRoad + Vector3.left * (COIN_RADIUS * 2) * (coinIndex + 1),
                    movSpeed * Time.deltaTime);
            }

            yield return null;
        }

        rb.MoveRotation(Quaternion.identity * Quaternion.Euler(0f, 180f, 0f)); // correct main coins rotation

        StartCoroutine(ElevateCoins(tfRoad));
    }

    private IEnumerator ElevateCoins(Transform tfRoad){
        // put all coins on top of each other
        float elevateSpeed = 5f;
        if (collectedCoins.Count != 0){
            Vector3 lastCoinPos = transform.position + Vector3.up * (COIN_RADIUS * 2) * (collectedCoins.Count);
            while (Vector3.Distance(collectedCoins[collectedCoins.Count - 1].position, lastCoinPos) > 1f){
            // if topmost coin is not reached to its destination
                for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
                    collectedCoins[coinIndex].position = Vector3.Lerp(collectedCoins[coinIndex].position,
                        transform.position + Vector3.up * (COIN_RADIUS * 2) * (coinIndex + 1),
                        elevateSpeed * Time.deltaTime); // move coins to upwards
                }
                yield return null;
            }

            // if coins are not in exactly desired position, then locate coins to desired positions
            for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
                collectedCoins[coinIndex].position = transform.position + Vector3.up * (COIN_RADIUS * 2) * (coinIndex + 1);
            }
        }

        yield return new WaitForSeconds(0.6f); // wait before sending coins to stair steps
        StartCoroutine(SendCoinsToStairs(tfRoad));
    }

    private IEnumerator SendCoinsToStairs(Transform tfRoad){
        // makes coins go to stairs and fall
        float movSpeed = 5f; // the speed when coins go towards stairs
        List<Vector3> stairPosList = Stairs.StairPosList; // position list that includes every position of stair steps

        bool isMainCoinDropped = false;
        List<bool> isCoinsDropped = new List<bool>();
        for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++)
            isCoinsDropped.Add(false);

        if (collectedCoins.Count == 0){ // if no coin is collected
            while (Vector3.Distance(transform.position, stairPosList[0]) > 1f){
                transform.position = Vector3.MoveTowards(transform.position,
                    new Vector3(stairPosList[0].x, transform.position.y, stairPosList[0].z),
                    movSpeed * Time.deltaTime); // move main coin to first stair step
                yield return null;
            }
            isMainCoinDropped = true;
            DropMainCoin();
        }else{ // if there are collected coins
            while (!isCoinsDropped[collectedCoins.Count - 1]){

                for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
                    Vector3 coinDestination = new Vector3(stairPosList[coinIndex + 1].x, collectedCoins[coinIndex].position.y,
                            stairPosList[coinIndex + 1].z);
                    if (!isCoinsDropped[coinIndex]){
                        if (Vector3.Distance(collectedCoins[coinIndex].position, coinDestination) > 0.2f){
                            collectedCoins[coinIndex].position = Vector3.MoveTowards(collectedCoins[coinIndex].position,
                                coinDestination,
                                movSpeed * Time.deltaTime); // move collected coin to specified stair step
                        }else{
                            isCoinsDropped[coinIndex] = true;
                            DropCoin(new Dictionary<string, object>(){{"transform", collectedCoins[coinIndex].transform}, {"isRemoveFromList", false}});
                        }
                    }
                }

                Vector3 mainCoinDestination = new Vector3(stairPosList[0].x, transform.position.y, stairPosList[0].z);
                if (!isMainCoinDropped){
                    if (Vector3.Distance(transform.position, mainCoinDestination) > 0.2f){
                        transform.position = Vector3.MoveTowards(transform.position,
                            mainCoinDestination,
                            movSpeed * Time.deltaTime); // move main coin to first stair step
                    }else{
                        isMainCoinDropped = true;
                        DropMainCoin();
                    }
                }

                yield return null;
            }
        }
        
        yield return new WaitForSeconds(1.5f); // wait before show win screen
        EventManager.TriggerEvent(Events.OnWinLevel, new Dictionary<string, object>(){});
    }

    private void DisconnectCoins(){
        // disconnects all coins from each other
        for (int coinIndex = 0; coinIndex < collectedCoins.Count; coinIndex++){
            collectedCoins[coinIndex].GetComponent<CoinMovement>().ConnectedCoin = null;
        }
    }

    private void OnCollisionEnter(Collision col) {

        if (col.gameObject.tag == "Uncollected"){

            collectedCoins.Add(col.gameObject.transform); // add new coin to the collected coin list
            col.gameObject.AddComponent<CoinMovement>(); // add coin movement script to make it follow queue
            col.gameObject.tag = "Collected";

            if (collectedCoins.Count == 1) // if only one coin is collected
                col.gameObject.GetComponent<CoinMovement>().ConnectedCoin = transform;
            else{ // if more than one coin are collected
                col.gameObject.GetComponent<CoinMovement>().ConnectedCoin = collectedCoins[collectedCoins.Count - 2].transform;
                col.gameObject.transform.position = collectedCoins[collectedCoins.Count - 2].transform.position;
            }

            col.gameObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // to prevent coin rotation at the beginning

            ///// do bounce effect when new coin is collected /////
            if (isScaling){
                StopCoroutine(ScaleUpAndDown());
            }
            StartCoroutine(ScaleUpAndDown());
            ///////////////////////////////////////////////////////
        }
    }

    private IEnumerator ScaleUpAndDown(){
        // makes main coin scale up to desired scale
        // then scale down to initial scale
        isScaling = true;
        float newScale = transform.localScale.x;
        transform.localScale = new Vector3(newScale, newScale, newScale);
        while (newScale < initialScale + scalingIncrease){
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

    private void Reset(Dictionary<string, object> message){
        // resets main coin to prepare it next level
        collectedCoins.Clear();
        transform.position = Vector3.zero + Vector3.up * (COIN_RADIUS * 2f);
        transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 180f, 0f);
        transform.localScale = new Vector3(initialScale, initialScale, initialScale);
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

#endregion
}
