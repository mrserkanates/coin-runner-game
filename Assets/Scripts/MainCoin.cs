using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCoin : MonoBehaviour
{
    [SerializeField] private Material platformMaterial;
    void Start()
    {
        
    }

    void Update()
    {
        platformMaterial.SetVector("_Pos", transform.position);
    }
}
