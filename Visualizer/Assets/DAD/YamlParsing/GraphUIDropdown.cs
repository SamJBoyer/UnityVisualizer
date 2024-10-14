using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YamlDotNet.Core;

public class GraphUIDropdown : YamlRep
{
    [SerializeField] private TMP_Dropdown _dropdown;

    public override string GetContent()
    {
        int index = _dropdown.value;
        return _dropdown.options[index].text;
    }

    public override void Initialize(string label, AnchorName anchor, string[] options)
    {
        base.Initialize(label, anchor, options);
        _dropdown.ClearOptions();
        _dropdown.AddOptions(new List<string>(options));
    }
}
