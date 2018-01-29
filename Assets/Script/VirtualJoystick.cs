using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


//Scrollbar VirtualJoystick
[RequireComponent(typeof(Scrollbar))]
public class VirtualJoystick : MonoBehaviour, IPointerUpHandler {

    private Scrollbar sc;

    private void Awake()
    {
        sc = GetComponent<Scrollbar>();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        sc.value = 0.5f;
    }
}
