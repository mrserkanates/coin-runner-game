using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinMovement : MonoBehaviour
{
    private Transform connectedCoin;

    public Transform ConnectedCoin { get => connectedCoin; set => connectedCoin = value; }

    void Update()
    {
        float zDelta;
        //zDelta = Mathf.Lerp(transform.position.z, ConnectedCoin.position.z, Time.deltaTime * 20);
        float zDiff = Mathf.Abs(ConnectedCoin.position.z - transform.position.z);
        if (ConnectedCoin.position.z > transform.position.z){
            zDelta = Mathf.Lerp(transform.position.z, transform.position.z + zDiff * 10, Time.deltaTime * 20);
        }else{
            zDelta = Mathf.Lerp(transform.position.z, transform.position.z - zDiff * 10, Time.deltaTime * 20);
        }
        Debug.Log("ZDIFF: " + zDiff);
        transform.position = new Vector3(
            connectedCoin.position.x - 1f,
            ConnectedCoin.position.y,
            zDelta);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, ConnectedCoin.rotation, 720 * Time.deltaTime);
    }
}
