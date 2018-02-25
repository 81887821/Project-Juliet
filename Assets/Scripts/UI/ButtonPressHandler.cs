using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPresseHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool Pressing
    {
        get;
        private set;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Pressing = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Pressing = false;
    }
}
