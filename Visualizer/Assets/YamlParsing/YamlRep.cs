using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;
using System.IO;
using System;
using YamlDotNet.Core;
using TMPro;

public class YamlRep : MonoBehaviour
{
    [SerializeField] TMP_Text _labelText;
    private AnchorName _yamlAnchor;
    private string _content;

    public AnchorName GetAnchor()
    {
        return _yamlAnchor;
    }

    public virtual string GetContent()
    {
        return _content;
    }

    public virtual void Initialize(string label, AnchorName anchor, string[] options)
    {
        _labelText.text = label;
        _yamlAnchor = anchor;
    }
}


