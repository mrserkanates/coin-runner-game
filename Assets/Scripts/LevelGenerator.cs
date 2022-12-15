using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoadType{
    R_Forward, // vertical road
    RJ_RightToForward, // road junction that turns right to forward
    RJ_LeftToForward, // road junction that turns left to forward
    RJ_ForwardToRight, // road junction that turns forward to right
    RJ_ForwardToLeft // road junction that turns forward to left
}

public class LevelGenerator : MonoBehaviour
{
    public const float ROAD_LENGTH = 10f;
    public const float ROAD_WIDTH = 6f;
    public const float ROAD_JUNCTION_LENGTH = 6f;
    public const float ROAD_JUNCTION_WIDTH = 6f;

    [Header("Prefabs")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private GameObject roadJunctionPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject axePrefab;
    [SerializeField] private GameObject propPrefab;
    [SerializeField] private GameObject finishLinePrefab;
    [SerializeField] private GameObject stairsPrefab;

    [Header("Parameters")]
    [SerializeField] private int platformSize;
    [SerializeField] private int minCoinAtSingleRoad;
    [SerializeField] private int maxCoinAtSingleRoad;

    private float coinOffsetY = 0.8f; // needed Y offset while creating coins
    private float axeOffsetY = 4.45f; // needed Y offset while creating axes
    private float propOffsetY = 1.2f; // needed Y offset while creating props

    private void OnEnable() {
        EventManager.StartListening(Events.OnCreateNewLevel, GenerateLevel);
    }

    private void OnDisable() {
        EventManager.StopListening(Events.OnCreateNewLevel, GenerateLevel);
    }

#region PRIVATE_FUNCTIONS
    private void GenerateLevel(Dictionary<string, object> message){
        // destroy all items before generating level
        for (int childIndex = 0; childIndex < transform.childCount; childIndex++){
            Destroy(transform.GetChild(childIndex).gameObject);
        }

        // create a vertical road at the beginning
        Vector3 currentRoadSpawnPos = transform.position;
        Instantiate(roadPrefab, currentRoadSpawnPos, Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform); // first road is created
        RoadType currentRoadType = RoadType.R_Forward;
        
        // create remained roads according to platform size
        for (int roadIndex = 0; roadIndex < platformSize; roadIndex++){
            GameObject newRoad = CreateRandomRoad(currentRoadType, currentRoadSpawnPos);
            if (roadIndex > 1){ // do not fill first road
                FillPlatform(newRoad.transform);
            }
            currentRoadType = newRoad.GetComponent<Road>().RoadType;
            currentRoadSpawnPos = newRoad.transform.position;
        }

        // if there is a road junction at the end
        // then create another road junction
        // to be able to connect it with vertical road
        GameObject roadJunctionAtTheEnd;
        if (currentRoadType == RoadType.R_Forward){
            currentRoadSpawnPos += Vector3.right * ROAD_LENGTH;
        }else if (currentRoadType == RoadType.RJ_LeftToForward || currentRoadType == RoadType.RJ_RightToForward){
            currentRoadSpawnPos += Vector3.right * (ROAD_LENGTH / 2 + ROAD_JUNCTION_LENGTH / 2);
        }else if (currentRoadType == RoadType.RJ_ForwardToRight){
            roadJunctionAtTheEnd = Instantiate(roadJunctionPrefab,
                currentRoadSpawnPos + Vector3.back * ROAD_JUNCTION_LENGTH,
                Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_RightToForward), 0f), transform);
            roadJunctionAtTheEnd.GetComponent<Road>().RoadType = RoadType.RJ_RightToForward;
            currentRoadSpawnPos += Vector3.back * ROAD_JUNCTION_LENGTH + Vector3.right * (ROAD_JUNCTION_LENGTH / 2 + ROAD_LENGTH / 2);
        }else if (currentRoadType == RoadType.RJ_ForwardToLeft){
            roadJunctionAtTheEnd = Instantiate(roadJunctionPrefab,
                currentRoadSpawnPos + Vector3.forward * ROAD_JUNCTION_LENGTH,
                Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_LeftToForward), 0f), transform);
            roadJunctionAtTheEnd.GetComponent<Road>().RoadType = RoadType.RJ_LeftToForward;
            currentRoadSpawnPos += Vector3.forward * ROAD_JUNCTION_LENGTH + Vector3.right * (ROAD_JUNCTION_LENGTH / 2 + ROAD_LENGTH / 2);
        }

        // create the road before the finishing line
        GameObject roadBeforeFinishLine = Instantiate(roadPrefab,
            currentRoadSpawnPos,
            Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
        roadBeforeFinishLine.GetComponent<Road>().RoadType = RoadType.R_Forward;

        // create the finish line
        GameObject finishLine = Instantiate(finishLinePrefab,
            currentRoadSpawnPos + Vector3.up * 0.51f,
            Quaternion.Euler(0f, 90f, 0f), transform);
        
        // create the last road, after the finishing line
        currentRoadSpawnPos += Vector3.right * ROAD_LENGTH;
        GameObject roadAfterFinishLine = Instantiate(roadPrefab,
            currentRoadSpawnPos,
            Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
        roadAfterFinishLine.GetComponent<Road>().RoadType = RoadType.R_Forward;

        // set parent of finishing line to road
        // it is used in finishing line script
        finishLine.transform.SetParent(roadAfterFinishLine.transform);

        // create stairs at the end
        GameObject stairs = Instantiate(stairsPrefab,
            currentRoadSpawnPos + Vector3.right * (ROAD_LENGTH / 2) + Vector3.down * 0.1f,
            Quaternion.Euler(0f, 90f, 0f), transform);
    }

    private void FillPlatform(Transform tfRoad){
        // fills platforms randomly with coins and traps

        if (tfRoad.GetComponent<Road>().RoadType == RoadType.R_Forward){ // if it is vertical road

            // spawn coins and traps at random positions 

            ////////// COIN //////////
            List<Vector3> coinPositions = new List<Vector3>();
            int coinAmount = Random.Range(minCoinAtSingleRoad, maxCoinAtSingleRoad + 1);
            Vector3 randomCoinPos = GetRandomPosOnRoad(ROAD_LENGTH, ROAD_WIDTH, coinOffsetY);
            coinPositions.Add(randomCoinPos);
            for (int coinIndex = 0; coinIndex < coinAmount; coinIndex++){
                GameObject newCoin = Instantiate(coinPrefab, randomCoinPos, Quaternion.identity, tfRoad);
                newCoin.transform.localPosition = randomCoinPos;
                // give a random rotation to coin
                newCoin.transform.localRotation = Quaternion.Euler(0, (Random.Range(0, 13) - 7) * 15, 0);
                randomCoinPos = GetRandomPosOnRoad(ROAD_LENGTH, ROAD_WIDTH, coinOffsetY);
                while (coinPositions.Contains(randomCoinPos)){
                    // if a coin is already created in this position
                    // then change the random position
                    randomCoinPos = GetRandomPosOnRoad(ROAD_LENGTH, ROAD_WIDTH, coinOffsetY);
                }
                coinPositions.Add(randomCoinPos);
            }
            ///////////////////////////

            /////////// AXE ///////////
            Vector3 randomAxePos = GetRandomPosOnRoad(ROAD_LENGTH, ROAD_WIDTH, axeOffsetY);
            bool spawnAxe = Random.Range(0, 2) == 1 ? true : false;
            if (spawnAxe){
                GameObject newAxe = Instantiate(axePrefab, randomAxePos, Quaternion.identity, tfRoad);
                newAxe.transform.localPosition = randomAxePos;
            }
            ///////////////////////////

            /////////// PROP //////////
            if (!spawnAxe){ // it is better to have only one trap on a road piece
                Vector3 randomPropPos = GetRandomPosOnRoad(ROAD_LENGTH, ROAD_WIDTH, propOffsetY);
                bool spawnProp = Random.Range(0, 2) == 1 ? true : false;
                if (spawnProp){
                    GameObject newProp = Instantiate(propPrefab, randomPropPos, Quaternion.identity, tfRoad);
                    newProp.transform.localPosition = randomPropPos;
                }
            }
            ///////////////////////////

        }else{ // if it is road junction

            // spawn traps at the center

            /////////// AXE ///////////
            bool spawnAxe = Random.Range(0, 2) == 1 ? true : false;
            if (spawnAxe){
                GameObject newAxe = Instantiate(axePrefab, Vector3.zero, Quaternion.identity, tfRoad);
                newAxe.transform.localPosition = new Vector3(0f, axeOffsetY, 0f);
            }
            ///////////////////////////

            /////////// PROP //////////
            if (!spawnAxe){ // it is better to have only one trap on a road piece
                bool spawnProp = Random.Range(0, 2) == 1 ? true : false;
                if (spawnProp){
                    GameObject newProp = Instantiate(propPrefab, Vector3.zero, Quaternion.identity, tfRoad);
                    newProp.transform.localPosition = new Vector3(0f, propOffsetY, 0f);
                }
            }
            ///////////////////////////
        }
    }

    private GameObject CreateRandomRoad(RoadType currentRoadType, Vector3 currentRoadSpawnPos){
        // creates a random road based on current road
        GameObject newRoad;
        RoadType nextRoadType;
        int rndRoad; // random road index

        // select a random road type between possible road types
        // then create and set the road
        switch (currentRoadType){
            case RoadType.R_Forward:
            // three different ways: forward, forward to right, forward to left
                rndRoad = Random.Range(0, 3);
                if (rndRoad == 0){
                    nextRoadType = RoadType.RJ_ForwardToRight;
                    newRoad = Instantiate(roadJunctionPrefab,
                        currentRoadSpawnPos + Vector3.right * (ROAD_LENGTH / 2 + ROAD_JUNCTION_LENGTH / 2),
                        Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_ForwardToRight), 0f), transform);
                    newRoad.GetComponent<Road>().RoadType = nextRoadType;
  
                }else if (rndRoad == 1){
                    nextRoadType = RoadType.RJ_ForwardToLeft;
                    newRoad = Instantiate(roadJunctionPrefab,
                        currentRoadSpawnPos + Vector3.right * (ROAD_LENGTH / 2 + ROAD_JUNCTION_LENGTH / 2),
                        Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_ForwardToLeft), 0f), transform);
                    newRoad.GetComponent<Road>().RoadType = nextRoadType;
                }else{
                    nextRoadType = RoadType.R_Forward;
                    newRoad = Instantiate(roadPrefab,
                    currentRoadSpawnPos + Vector3.right * ROAD_LENGTH,
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
                    newRoad.GetComponent<Road>().RoadType = nextRoadType;
                }
            break;
            case RoadType.RJ_RightToForward:
                // only way is forward
                nextRoadType = RoadType.R_Forward;
                newRoad = Instantiate(roadPrefab,
                    currentRoadSpawnPos + Vector3.right * (ROAD_LENGTH / 2 + ROAD_JUNCTION_LENGTH / 2),
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            case RoadType.RJ_LeftToForward:
                // only way is forward
                nextRoadType = RoadType.R_Forward;
                newRoad = Instantiate(roadPrefab,
                    currentRoadSpawnPos + Vector3.right * (ROAD_LENGTH / 2 + ROAD_JUNCTION_LENGTH / 2),
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            case RoadType.RJ_ForwardToRight:
                // only way is right to forward
                nextRoadType = RoadType.RJ_RightToForward;
                newRoad = Instantiate(roadJunctionPrefab,
                    currentRoadSpawnPos + Vector3.back * ROAD_JUNCTION_LENGTH,
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_RightToForward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            case RoadType.RJ_ForwardToLeft:
                // only way is left to forward
                nextRoadType = RoadType.RJ_LeftToForward;
                newRoad = Instantiate(roadJunctionPrefab,
                    currentRoadSpawnPos + Vector3.forward * ROAD_JUNCTION_LENGTH,
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_LeftToForward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            default:
                // default case
                nextRoadType = RoadType.R_Forward;
                newRoad = Instantiate(roadPrefab,
                currentRoadSpawnPos + Vector3.right * ROAD_LENGTH,
                Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
        }
        return newRoad;
    }

    private Vector3 GetRandomPosOnRoad(float roadSizeX, float roadSizeZ, float posY){
        // produces a random position on the road
        float xAxisFactor = Random.Range(1, (int)ROAD_LENGTH) / ROAD_LENGTH;
        float zAxisFactor = Random.Range(1, (int)ROAD_WIDTH) / ROAD_WIDTH;
        return new Vector3(xAxisFactor * ROAD_LENGTH - ROAD_LENGTH / 2, posY, zAxisFactor * ROAD_WIDTH - ROAD_WIDTH / 2);
    }

    private float GetRoadAngle(RoadType roadType){
        // returns the Y rotations in degrees
        // in order to place the road correctly
        if (roadType == RoadType.R_Forward)
            return 0f;
        else if (roadType == RoadType.RJ_RightToForward)
            return 180f;
        else if (roadType == RoadType.RJ_LeftToForward)
            return 270f;
        else if (roadType == RoadType.RJ_ForwardToRight)
            return 0;
        else if (roadType == RoadType.RJ_ForwardToLeft)
            return 90;
        else
            return 0;
    }

#endregion

}
