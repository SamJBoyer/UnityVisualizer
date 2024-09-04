using UnityEngine;
using UnityEngine.EventSystems;

//event pointer handler that sets helps lock the camera if the mouse is inside the dof selection zone 
public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool WithinDOFZone; //disable the camera controls if within this zone 

    public void OnPointerEnter(PointerEventData eventData)
    {
        WithinDOFZone = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        WithinDOFZone = false;
    }

}