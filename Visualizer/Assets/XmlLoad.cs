using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Xml.Linq;
using System;
using System.IO;

[ExecuteAlways]
public class XmlLoad : MonoBehaviour
{
    // Path to the XML file
    public string xmlFilePath = "example.xml";


    public void LoadXml()
    {
        // Load the XML file
        XDocument xmlDoc = XDocument.Load(xmlFilePath);

        // Find the "worldbody" element
        XElement worldbody = xmlDoc.Root.Element("worldbody");
        GameObject origin = new GameObject("Origin");
        CreateNewBody(origin, worldbody);


    }

    private void CreateNewBody(GameObject parent, XElement element)
    {
        //create a new game object
        GameObject newBody = new GameObject("New Body");
        newBody.transform.parent = parent.transform;

        foreach (XElement childElement in element.Elements())
        {
            if (childElement.Name == "geom")
            {
                CreateGeom(newBody.transform, childElement);
            }
            else if (childElement.Name == "body")
            {
                CreateNewBody(newBody, childElement);
            }
        }
        return;
    }

    private void CreateGeom(Transform parent, XElement element)
    {
        Vector3 position = new Vector3();
        Vector3 size = new Vector3();
        Quaternion rotation = new Quaternion();
        PrimitiveType primType = PrimitiveType.Cube;

        foreach (XAttribute attribute in element.Attributes())
        {
            XName attributeName = attribute.Name;
            if (attributeName == "mesh")
            {
                print("mesh found");
            }
            else if (attributeName == "pos")
            {
                string[] values = attribute.Value.Split(" ");
                position = new Vector3(float.Parse(values[0]), float.Parse(values[2]), float.Parse(values[1]));
            }
            else if (attributeName == "quat")
            {
                string[] values = attribute.Value.Split(" ");
                rotation = new Quaternion(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]), float.Parse(values[0]));
            }
            else if (attributeName == "size")
            {
                string[] values = attribute.Value.Split(" ");
                if (values.Length == 1)
                {
                    size = new Vector3(float.Parse(values[0]), float.Parse(values[0]), float.Parse(values[0]));
                }
                else if (values.Length == 2)
                {
                    size = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[0]));
                }
                else if (values.Length == 3)
                {
                    size = new Vector3(float.Parse(values[0]), float.Parse(values[2]), float.Parse(values[1]));
                }
            }
            else if (attributeName == "type")
            {
                switch (attribute.Value)
                {
                    case "box":
                        primType = PrimitiveType.Cube;
                        break;
                    case "cylinder":
                        primType = PrimitiveType.Cylinder;
                        break;
                    case "sphere":
                        primType = PrimitiveType.Sphere;
                        break;
                    case "capsule":
                        primType = PrimitiveType.Capsule;
                        break;
                }
            }
        }

        GameObject newGeom = GameObject.CreatePrimitive(primType);
        newGeom.transform.parent = parent;
        newGeom.transform.position = position;
        newGeom.transform.rotation = rotation;
        newGeom.transform.localScale = size;
    }



}