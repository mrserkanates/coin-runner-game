using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovementCollider : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public MainCoin mainCoin;
    private Vector3 startPos;
    private float delta;

    public void OnBeginDrag(PointerEventData eventData)
    {
        //mainCoin.StartDrag();
        //startPos = eventData.position;
        EventManager.TriggerEvent(Events.OnStartLevel, new Dictionary<string, object>(){});
    }

    public void OnDrag(PointerEventData eventData)
    {
        //mainCoin.Drag(0f);

        /*Debug.Log("DRAG");
        Vector3 endPos = eventData.position;
        Vector3 posDelta = endPos - startPos;
        Debug.Log(endPos + " || " + startPos);
        //mainCoin.transform.rotation = Quaternion.Euler(0f, mainCoin.transform.rotation.y + eventData.delta.x * 5, 0f);
        startPos = endPos;*/
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //mainCoin.StopDrag();
    }

}
