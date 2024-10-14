using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GraphUIText : YamlRep
{
    [SerializeField] private TMP_InputField _inputField;

    public override string GetContent()
    {
        return _inputField.text;
    }
}
