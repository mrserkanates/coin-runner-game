using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinMovement : MonoBehaviour
{
    private Transform connectedCoin;
    private float radius = 0.3f;
    private float followSpeed = 8f;
    private Rigidbody rb;

    public Transform ConnectedCoin { get => connectedCoin; set => connectedCoin = value; }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    void Start(){
        //radius = GetComponent<MeshCollider>().bounds.size.x / 2;
        Destroy(GetComponent<Collider>());
    }

    void FixedUpdate()
    {
        Vector3 newPosition = ConnectedCoin.position + ConnectedCoin.right * radius * 2;
        rb.MovePosition(Vector3.Lerp(transform.position, newPosition, Time.fixedDeltaTime * followSpeed));
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, ConnectedCoin.rotation, 360 * Time.fixedDeltaTime));
        transform.localScale = Vector3.Lerp(transform.localScale, ConnectedCoin.localScale, Time.fixedDeltaTime * 60);
    }
    
}
