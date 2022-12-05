using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinMovement : MonoBehaviour
{
    private Transform connectedCoin;
    private float followSpeed = 8f;
    private float scaleSpeed = 60f;
    private float rotateSpeed = 240f;
    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (ConnectedCoin != null){
            rb.MovePosition(Vector3.Lerp(transform.position,
                ConnectedCoin.position + ConnectedCoin.right * MainCoin.COIN_RADIUS * 2,
                Time.fixedDeltaTime * followSpeed));
            rb.MoveRotation(Quaternion.identity * Quaternion.Euler(Vector3.Lerp(transform.eulerAngles, ConnectedCoin.eulerAngles,
                rotateSpeed * Time.fixedDeltaTime)));
            transform.localScale = Vector3.Lerp(transform.localScale, ConnectedCoin.localScale, Time.fixedDeltaTime * scaleSpeed);
        }
    }

#region PUBLIC_FUNCTIONS
    public void DropCoin(){
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForceAtPosition(transform.forward * 1f, transform.position + Vector3.up * MainCoin.COIN_RADIUS, ForceMode.Impulse);
        Destroy(GetComponent<CoinMovement>());
    }

#endregion

#region GETTER_AND_SETTERS
    public Transform ConnectedCoin { get => connectedCoin; set => connectedCoin = value; }

#endregion
    
}
