using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    private Rigidbody rb;
    private float currentRotX;
    private float currentRotZ = 0.0f;
    private float minRotX = 20.0f;
    private float maxRotX = 160.0f;
    private bool isSwingingRight = true;

    private void Awake(){
        rb = GetComponent<Rigidbody>();
    }

    private void Start(){
        currentRotX = minRotX;
    }

    private void FixedUpdate(){
        rb.MoveRotation(Quaternion.identity * Quaternion.Euler(currentRotX, 0f, currentRotZ));
        if (isSwingingRight){
            if (currentRotX < maxRotX){
                currentRotX += Time.fixedDeltaTime * rotationSpeed;
            }else{
                isSwingingRight = false;
                currentRotZ = 180f;
            }
        }else{
            if (currentRotX > minRotX){
                currentRotX -= Time.fixedDeltaTime * rotationSpeed;
            }else{
                isSwingingRight = true;
                currentRotZ = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other){
        if (other.gameObject.tag == "MainCoin"){
            EventManager.TriggerEvent(Events.OnFailLevel, new Dictionary<string, object>(){});
            EventManager.TriggerEvent(Events.OnHitMainCoin, new Dictionary<string, object>(){});
        }else if (other.gameObject.tag == "Collected"){
            EventManager.TriggerEvent(Events.OnHitCoin, new Dictionary<string, object>(){{"transform", other.transform}});
        }
    }
}
