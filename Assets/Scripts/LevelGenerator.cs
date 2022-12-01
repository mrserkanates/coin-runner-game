using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private int levelDifficulty;

    [Header("Prefabs")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject axePrefab;
    [SerializeField] private GameObject propPrefab;


    [Header("Parameters")]
    [SerializeField] private int metersOfOneUnit;
    [SerializeField] private int platformSize;
    private int minCoinAtSinglePlatform = 2;
    private int maxCoinAtSinglePlatform = 5;

    void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel(){
        Vector3 currentPlatformPoint = transform.position;
        for (int platformIndex = 0; platformIndex < platformSize; platformIndex++){
            GameObject newPlatform = Instantiate(platformPrefab, currentPlatformPoint, Quaternion.identity, transform);
            if (platformIndex > 1){ // do not fill first platform
                FillPlatform(newPlatform.transform);
            }
            currentPlatformPoint += Vector3.right * platformPrefab.transform.localScale.x * metersOfOneUnit;
        }
    }

    private void FillPlatform(Transform tfPlatform){
        float pSizeX = tfPlatform.localScale.x * metersOfOneUnit;
        float pSizeZ = tfPlatform.localScale.z * metersOfOneUnit;
        int coinAmount = Random.Range(minCoinAtSinglePlatform, maxCoinAtSinglePlatform + 1);

        Vector3 randomCoinPos = new Vector3(Random.Range(0, pSizeZ) - pSizeZ / 2, 0.5f, Random.Range(0, pSizeX) - pSizeX / 2);
        for (int coinIndex = 0; coinIndex < coinAmount; coinIndex++){
            GameObject newCoin = Instantiate(coinPrefab, randomCoinPos, Quaternion.identity, tfPlatform);
            newCoin.transform.localPosition = randomCoinPos;
            newCoin.transform.localRotation = Quaternion.Euler(0, (Random.Range(0, 13) - 7) * 15, 0);
            randomCoinPos = new Vector3(Random.Range(0, pSizeZ) - pSizeZ / 2, 0.5f, Random.Range(0, pSizeX) - pSizeX / 2);
        }
    }
}
