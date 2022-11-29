using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject axePrefab;
    [SerializeField] private GameObject propPrefab;

    [Header("Parameters")]
    [SerializeField] private int platformSize;

    void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel(){
        Vector3 currentPlatformPoint = transform.position;
        for (int platformIndex = 0; platformIndex < platformSize; platformIndex++){
            GameObject newPlatform = Instantiate(platformPrefab, currentPlatformPoint, Quaternion.identity, transform);
            FillPlatform(newPlatform.transform);
            currentPlatformPoint += Vector3.forward * platformPrefab.transform.localScale.z * 10;
        }
    }

    private void FillPlatform(Transform tfPlatform){

    }
}
