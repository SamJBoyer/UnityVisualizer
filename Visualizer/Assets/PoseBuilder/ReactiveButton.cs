using UnityEngine;
using UnityEngine.UI;

public class ReactiveButton : MonoBehaviour
{
    private bool _selected;
    public void ToggleSelection()
    {
        if (_selected)
        {
            _selected = false;
            GetComponent<Image>().color = Color.white;
        }
        else
        {
            _selected = true;
            GetComponent<Image>().color = Color.green;
        }
    }
}
