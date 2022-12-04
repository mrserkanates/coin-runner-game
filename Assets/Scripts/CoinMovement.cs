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

    public void DropCoin(){
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForceAtPosition(transform.forward * 2f, transform.position + Vector3.up * radius, ForceMode.Impulse);
        Destroy(GetComponent<CoinMovement>());
    }

    void FixedUpdate()
    {
        if (ConnectedCoin != null){
            Vector3 newPosition = ConnectedCoin.position + ConnectedCoin.right * radius * 2;
            rb.MovePosition(Vector3.Lerp(transform.position, newPosition, Time.fixedDeltaTime * followSpeed));
            rb.MoveRotation(Quaternion.identity * Quaternion.Euler(Vector3.Lerp(transform.eulerAngles, ConnectedCoin.eulerAngles, 240 * Time.fixedDeltaTime)));
            //rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, ConnectedCoin.rotation, 180 * Time.fixedDeltaTime));
            transform.localScale = Vector3.Lerp(transform.localScale, ConnectedCoin.localScale, Time.fixedDeltaTime * 60);
        }
    }
    
}
