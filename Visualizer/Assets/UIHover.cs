using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsMouseHover = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsMouseHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsMouseHover = false;
    }

}