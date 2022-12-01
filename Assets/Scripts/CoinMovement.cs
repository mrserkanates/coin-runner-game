using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinMovement : MonoBehaviour
{
    private Transform connectedCoin;
    private float radius = 0.3f;
    private float followSpeed = 30f;

    public Transform ConnectedCoin { get => connectedCoin; set => connectedCoin = value; }

    void Start(){
        //radius = GetComponent<MeshCollider>().bounds.size.x / 2;
    }

    void Update()
    {
        float zDelta;
        float targetZ;

        if (ConnectedCoin.position.z > transform.position.z){ // go to left
            targetZ = ConnectedCoin.position.z - 2 * radius * Mathf.Sin(Mathf.Deg2Rad * (180 - ConnectedCoin.eulerAngles.y));
        }else if(ConnectedCoin.position.z < transform.position.z){ // go to right
            targetZ = ConnectedCoin.position.z + 2 * radius * Mathf.Sin(Mathf.Deg2Rad * (ConnectedCoin.eulerAngles.y - 180));
        }else{
            targetZ = ConnectedCoin.position.z;
        }

        if (targetZ - transform.position.z > 0.02f){
            zDelta = Mathf.Lerp(transform.position.z,
            targetZ,
            Time.deltaTime * followSpeed);
        }else{
            zDelta = targetZ;
        }
        Debug.Log("diff: " + (targetZ - transform.position.z));

        //Debug.Log("con coin rot: " + ConnectedCoin.eulerAngles + "rad: " + radius + " | name: " + gameObject.name);
        transform.position = new Vector3(connectedCoin.position.x - radius * 2f, ConnectedCoin.position.y, zDelta);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, ConnectedCoin.rotation, 720 * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, ConnectedCoin.localScale, Time.deltaTime * 60);
    }
    
}
