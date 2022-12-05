using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    public static List<Vector3> StairPosList;
    [SerializeField] private float stairCount;
    [SerializeField] private float stairHeigth;
    [SerializeField] private float stairLength;

    private void Start() {
        StairPosList = new List<Vector3>();
        SetStairPosList();
    }

#region PRIVATE_FUNCTIONS
    private void SetStairPosList(){
        StairPosList.Clear();
        for (int stairIndex = 0; stairIndex < stairCount; stairIndex++){
            StairPosList.Add(transform.position + new Vector3(stairLength * (stairIndex + 1) - stairLength / 2, stairHeigth * (stairIndex + 1), 0f));
        }
    }

#endregion

}
