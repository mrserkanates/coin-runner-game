using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoadType{
    R_Forward,
    RJ_RightToForward,
    RJ_LeftToForward,
    RJ_ForwardToRight,
    RJ_ForwardToLeft
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
    [SerializeField] private int metersOfOneUnit;
    [SerializeField] private int platformSize;
    private int minCoinAtSingleRoad = 1;
    private int maxCoinAtSingleRoad = 5;

    private void OnEnable() {
        EventManager.StartListening(Events.OnStartNewLevel, GenerateLevel);
    }

    private void OnDisable() {
        EventManager.StopListening(Events.OnStartNewLevel, GenerateLevel);
    }

    private void GenerateLevel(Dictionary<string, object> message){
        for (int childIndex = 0; childIndex < transform.childCount; childIndex++){
            Destroy(transform.GetChild(childIndex).gameObject);
        }
        Vector3 currentRoadSpawnPos = transform.position;
        Instantiate(roadPrefab, currentRoadSpawnPos, Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform); // first road is created
        RoadType currentRoadType = RoadType.R_Forward;
        
        for (int roadIndex = 0; roadIndex < platformSize; roadIndex++){
            GameObject newRoad = CreateRandomRoad(currentRoadType, currentRoadSpawnPos);
            if (roadIndex > 1){ // do not fill first road
                FillPlatform(newRoad.transform);
            }
            currentRoadType = newRoad.GetComponent<Road>().RoadType;
            currentRoadSpawnPos = newRoad.transform.position;
        }

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

        GameObject roadBeforeFinishLine = Instantiate(roadPrefab,
            currentRoadSpawnPos,
            Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
        roadBeforeFinishLine.GetComponent<Road>().RoadType = RoadType.R_Forward;

        GameObject finishLine = Instantiate(finishLinePrefab,
            currentRoadSpawnPos + Vector3.up * 0.51f,
            Quaternion.Euler(0f, 90f, 0f), transform);
        
        currentRoadSpawnPos += Vector3.right * ROAD_LENGTH;
        GameObject roadAfterFinishLine = Instantiate(roadPrefab,
            currentRoadSpawnPos,
            Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
        roadAfterFinishLine.GetComponent<Road>().RoadType = RoadType.R_Forward;

        finishLine.transform.SetParent(roadAfterFinishLine.transform);

        GameObject stairs = Instantiate(stairsPrefab,
            currentRoadSpawnPos + Vector3.right * (ROAD_LENGTH / 2),
            Quaternion.Euler(0f, 90f, 0f), transform);
    }

    private void FillPlatform(Transform tfRoad){
        if (tfRoad.GetComponent<Road>().RoadType == RoadType.R_Forward){ // if it is vertical road

            // spawn coins and traps at random positions 

            ////////// COIN //////////
            int coinAmount = Random.Range(minCoinAtSingleRoad, maxCoinAtSingleRoad + 1);
            Vector3 randomCoinPos = GetRandomPosOnRoad(ROAD_LENGTH, ROAD_WIDTH, 0.8f);
            for (int coinIndex = 0; coinIndex < coinAmount; coinIndex++){
                GameObject newCoin = Instantiate(coinPrefab, randomCoinPos, Quaternion.identity, tfRoad);
                newCoin.transform.localPosition = randomCoinPos;
                newCoin.transform.localRotation = Quaternion.Euler(0, (Random.Range(0, 13) - 7) * 15, 0);
                randomCoinPos = GetRandomPosOnRoad(ROAD_LENGTH, ROAD_WIDTH, 0.8f);
            }
            ///////////////////////////

            /////////// AXE ///////////
            Vector3 randomAxePos = GetRandomPosOnRoad(ROAD_LENGTH, ROAD_WIDTH, 4.5f);
            bool spawnAxe = Random.Range(0, 2) == 1 ? true : false;
            if (spawnAxe){
                GameObject newAxe = Instantiate(axePrefab, randomAxePos, Quaternion.identity, tfRoad);
                newAxe.transform.localPosition = randomAxePos;
            }
            ///////////////////////////

            /////////// PROP //////////
            if (!spawnAxe){ // it is better to have only one trap on a road piece
                Vector3 randomPropPos = GetRandomPosOnRoad(ROAD_LENGTH, ROAD_WIDTH, 1f);
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
                newAxe.transform.localPosition = new Vector3(0f, 4.5f, 0f);
            }
            ///////////////////////////

            /////////// PROP //////////
            if (!spawnAxe){ // it is better to have only one trap on a road piece
                bool spawnProp = Random.Range(0, 2) == 1 ? true : false;
                if (spawnProp){
                    GameObject newProp = Instantiate(propPrefab, Vector3.zero, Quaternion.identity, tfRoad);
                    newProp.transform.localPosition = new Vector3(0f, 1f, 0f);
                }
            }
            ///////////////////////////

        }

    }

    private Vector3 GetRandomPosOnRoad(float roadSizeX, float roadSizeZ, float posY){
        float xAxisFactor = Random.Range(1, (int)ROAD_LENGTH) / ROAD_LENGTH;
        float zAxisFactor = Random.Range(1, (int)ROAD_WIDTH) / ROAD_WIDTH;
        return new Vector3(xAxisFactor * ROAD_LENGTH - ROAD_LENGTH / 2, posY, zAxisFactor * ROAD_WIDTH - ROAD_WIDTH / 2);
    }

    private GameObject CreateRandomRoad(RoadType currentRoadType, Vector3 currentRoadSpawnPos){

        GameObject newRoad;
        RoadType nextRoadType;
        int rndRoad;

        switch (currentRoadType){
            case RoadType.R_Forward:
                rndRoad = Random.Range(0, 3);
                if (rndRoad == 0){
                    nextRoadType = RoadType.RJ_ForwardToLeft;
                    newRoad = Instantiate(roadJunctionPrefab,
                        currentRoadSpawnPos + Vector3.right * (ROAD_LENGTH / 2 + ROAD_JUNCTION_LENGTH / 2),
                        Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_ForwardToLeft), 0f), transform);
                    newRoad.GetComponent<Road>().RoadType = nextRoadType;
                }else if (rndRoad == 1){
                    nextRoadType = RoadType.RJ_ForwardToRight;
                    newRoad = Instantiate(roadJunctionPrefab,
                        currentRoadSpawnPos + Vector3.right * (ROAD_LENGTH / 2 + ROAD_JUNCTION_LENGTH / 2),
                        Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_ForwardToRight), 0f), transform);
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
                nextRoadType = RoadType.R_Forward;
                newRoad = Instantiate(roadPrefab,
                    currentRoadSpawnPos + Vector3.right * (ROAD_LENGTH / 2 + ROAD_JUNCTION_LENGTH / 2),
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            case RoadType.RJ_LeftToForward:
                nextRoadType = RoadType.R_Forward;
                newRoad = Instantiate(roadPrefab,
                    currentRoadSpawnPos + Vector3.right * (ROAD_LENGTH / 2 + ROAD_JUNCTION_LENGTH / 2),
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            case RoadType.RJ_ForwardToRight:
                nextRoadType = RoadType.RJ_RightToForward;
                newRoad = Instantiate(roadJunctionPrefab,
                    currentRoadSpawnPos + Vector3.back * ROAD_JUNCTION_LENGTH,
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_RightToForward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            case RoadType.RJ_ForwardToLeft:
                nextRoadType = RoadType.RJ_LeftToForward;
                newRoad = Instantiate(roadJunctionPrefab,
                    currentRoadSpawnPos + Vector3.forward * ROAD_JUNCTION_LENGTH,
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_LeftToForward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            default:
                nextRoadType = RoadType.R_Forward;
                newRoad = Instantiate(roadPrefab,
                currentRoadSpawnPos + Vector3.right * ROAD_LENGTH,
                Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
        }
        return newRoad;
    }

    private float GetRoadAngle(RoadType roadType){
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

}
