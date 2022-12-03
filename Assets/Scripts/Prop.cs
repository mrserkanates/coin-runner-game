using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    private Rigidbody rb;
    private float currentRotZ = 0.0f;

    private void Awake(){
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate(){
        rb.MoveRotation(Quaternion.identity * Quaternion.Euler(90f, 0f, currentRotZ));
        if (currentRotZ < 360){
            currentRotZ += Time.fixedDeltaTime * rotationSpeed;
        }else{
            currentRotZ = 0.0f;
        }
    }

    private void OnCollisionEnter(Collision other){
        Debug.Log("calisiyor");
        if (other.gameObject.tag == "MainCoin"){
            EventManager.TriggerEvent(Events.OnFailLevel, new Dictionary<string, object>());
        }else if (other.gameObject.tag == "Coin"){
            EventManager.TriggerEvent(Events.OnLoseCoin, new Dictionary<string, object>(){{"transform", other.transform}});
            Debug.Log("YEAPPU");
        }
    }
}
