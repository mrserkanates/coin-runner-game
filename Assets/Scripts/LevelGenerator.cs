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
    [SerializeField] private int levelDifficulty;

    [Header("Prefabs")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private GameObject roadJunctionPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject axePrefab;
    [SerializeField] private GameObject propPrefab;


    [Header("Parameters")]
    [SerializeField] private int metersOfOneUnit;
    [SerializeField] private int platformSize;
    private int minCoinAtSingleRoad = 1;
    private int maxCoinAtSingleRoad = 4;

    private float roadLength;
    private float roadWidth;
    private float roadJunctionLength;

    void Start()
    {
        roadLength = 10f;
        roadWidth = 6f;
        roadJunctionLength = 6f;
        GenerateLevel();
    }

    private void GenerateLevel(){
        for (int childIndex = 0; childIndex < transform.childCount; childIndex++){
            Destroy(transform.GetChild(childIndex).gameObject);
        }
        Vector3 currentRoadSpawnPos = transform.position;
        Instantiate(roadPrefab, currentRoadSpawnPos, Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform); // first road is created
        RoadType currentRoadType = RoadType.R_Forward;
        
        for (int platformIndex = 0; platformIndex < platformSize; platformIndex++){
            GameObject newRoad = CreateRandomRoad(currentRoadType, currentRoadSpawnPos);
            if (platformIndex > 1){ // do not fill first platform
                FillPlatform(newRoad.transform);
            }
            currentRoadType = newRoad.GetComponent<Road>().RoadType;
            currentRoadSpawnPos = newRoad.transform.position;
        }
    }

    private void FillPlatform(Transform tfRoad){
        if (tfRoad.GetComponent<Road>().RoadType == RoadType.R_Forward){ // if it is vertical road

            // spawn coins and traps at random positions 

            ////////// COIN //////////
            int coinAmount = Random.Range(minCoinAtSingleRoad, maxCoinAtSingleRoad + 1);
            Vector3 randomCoinPos = GetRandomPosOnRoad(roadLength, roadWidth, 0.8f);
            for (int coinIndex = 0; coinIndex < coinAmount; coinIndex++){
                GameObject newCoin = Instantiate(coinPrefab, randomCoinPos, Quaternion.identity, tfRoad);
                newCoin.transform.localPosition = randomCoinPos;
                newCoin.transform.localRotation = Quaternion.Euler(0, (Random.Range(0, 13) - 7) * 15, 0);
                randomCoinPos = GetRandomPosOnRoad(roadLength, roadWidth, 0.8f);
            }
            ///////////////////////////

            /////////// AXE ///////////
            Vector3 randomAxePos = GetRandomPosOnRoad(roadLength, roadWidth, 4.5f);
            bool spawnAxe = Random.Range(0, 2) == 1 ? true : false;
            if (spawnAxe){
                GameObject newAxe = Instantiate(axePrefab, randomAxePos, Quaternion.identity, tfRoad);
                newAxe.transform.localPosition = randomAxePos;
            }
            ///////////////////////////

            /////////// PROP //////////
            if (!spawnAxe){ // it is better to have only one trap on a road piece
                Vector3 randomPropPos = GetRandomPosOnRoad(roadLength, roadWidth, 1f);
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
            }
            ///////////////////////////

            /////////// PROP //////////
            if (!spawnAxe){ // it is better to have only one trap on a road piece
                bool spawnProp = Random.Range(0, 2) == 1 ? true : false;
                if (spawnProp){
                    GameObject newProp = Instantiate(propPrefab, Vector3.zero, Quaternion.identity, tfRoad);
                }
            }
            ///////////////////////////

        }

    }

    private Vector3 GetRandomPosOnRoad(float roadSizeX, float roadSizeZ, float posY){
        float xAxisFactor = Random.Range(1, (int)roadLength) / roadLength;
        float zAxisFactor = Random.Range(1, (int)roadWidth) / roadWidth;
        return new Vector3(xAxisFactor * roadLength - roadLength / 2, posY, zAxisFactor * roadWidth - roadWidth / 2);
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
                        currentRoadSpawnPos + Vector3.right * (roadLength / 2 + roadJunctionLength / 2),
                        Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_ForwardToLeft), 0f), transform);
                    newRoad.GetComponent<Road>().RoadType = nextRoadType;
                }else if (rndRoad == 1){
                    nextRoadType = RoadType.RJ_ForwardToRight;
                    newRoad = Instantiate(roadJunctionPrefab,
                        currentRoadSpawnPos + Vector3.right * (roadLength / 2 + roadJunctionLength / 2),
                        Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_ForwardToRight), 0f), transform);
                    newRoad.GetComponent<Road>().RoadType = nextRoadType;
                }else{
                    nextRoadType = RoadType.R_Forward;
                    newRoad = Instantiate(roadPrefab,
                    currentRoadSpawnPos + Vector3.right * roadLength,
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
                    newRoad.GetComponent<Road>().RoadType = nextRoadType;
                }
            break;
            case RoadType.RJ_RightToForward:
                nextRoadType = RoadType.R_Forward;
                newRoad = Instantiate(roadPrefab,
                    currentRoadSpawnPos + Vector3.right * (roadLength / 2 + roadJunctionLength / 2),
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            case RoadType.RJ_LeftToForward:
                nextRoadType = RoadType.R_Forward;
                newRoad = Instantiate(roadPrefab,
                    currentRoadSpawnPos + Vector3.right * (roadLength / 2 + roadJunctionLength / 2),
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.R_Forward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            case RoadType.RJ_ForwardToRight:
                nextRoadType = RoadType.RJ_RightToForward;
                newRoad = Instantiate(roadJunctionPrefab,
                    currentRoadSpawnPos + Vector3.back * roadJunctionLength,
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_RightToForward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            case RoadType.RJ_ForwardToLeft:
                nextRoadType = RoadType.RJ_LeftToForward;
                newRoad = Instantiate(roadJunctionPrefab,
                    currentRoadSpawnPos + Vector3.forward * roadJunctionLength,
                    Quaternion.Euler(0f, GetRoadAngle(RoadType.RJ_LeftToForward), 0f), transform);
                newRoad.GetComponent<Road>().RoadType = nextRoadType;
            break;
            default:
                nextRoadType = RoadType.R_Forward;
                newRoad = Instantiate(roadPrefab,
                currentRoadSpawnPos + Vector3.right * roadLength,
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
