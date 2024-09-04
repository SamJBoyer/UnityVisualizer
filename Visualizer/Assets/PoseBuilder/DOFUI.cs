using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// script on the DOF Ui box that manages inputting of the values through the text field or by dragging
//the box. Also locks the camera if bieng dragged 
public class DOFUI : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TextMeshProUGUI _DOFName;
    [SerializeField] private TMP_InputField _valueField;
    [SerializeField] private Image _image;

    public static bool DOFBeingDragged; //if the box is being dragged
    private float _textValue; //the float-verified value of the text input 

    private void Start()
    {
        _textValue = 0;
        _valueField.onValueChanged.AddListener(OnTextChanged);

    }

    void OnTextChanged(string text)
    {
        if (float.TryParse(text, out float value))
        {
            _textValue = value % 360;
            _valueField.text = _textValue.ToString();
        }
        else
        {
            _valueField.text = _textValue.ToString();
        }
    }

    //called by the PoseUiManager to give this box its DOF and color
    public void InitializeDOFBox(string name, Color color)
    {
        _DOFName.text = name;
        _image.color = color;
    }

    public float? GetValue()
    {
        float value;
        if (float.TryParse(_valueField.text, out value))
        {
            return value;
        }
        else
        {
            return null;
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        // Calculate value change based on horizontal movement
        var dragValue = eventData.delta.x * 1f; // Adjust the multiplier to control sensitivity
        _textValue += dragValue;
        _valueField.text = _textValue.ToString();
        //Debug.Log("Value: " + dragValue);
    }

    public void OnPointerDown(PointerEventData data)
    {
        DOFBeingDragged = true;
    }

    public void OnPointerUp(PointerEventData data)
    {
        DOFBeingDragged = false;
    }
}
