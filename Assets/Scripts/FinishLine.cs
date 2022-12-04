using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "MainCoin"){
            EventManager.TriggerEvent(Events.OnFinishLevel, new Dictionary<string, object>(){{"tfRoad", transform.parent.transform}});
        }
    }
}
