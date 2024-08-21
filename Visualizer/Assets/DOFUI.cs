using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class DOFUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _DOFName;
    [SerializeField] private TMP_InputField _valueField;
    [SerializeField] private Image _image;
    
    public void InitializeDOFBox(string name, Color color){
        _DOFName.text = name;
        _image.color = color;
    }

    public float? GetValue(){
        float value;
        if (float.TryParse(_valueField.text, out value)){
            return value;
        } else {
            return null;
        }
    }
}
