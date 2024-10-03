using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class MJStrip : MonoBehaviour
{

    public void RemoveMJComponents()
    {
        Transform targetTransform = this.transform;
        HuntComponents(targetTransform);
    }

    private void HuntComponents(Transform parent)
    {
        string[] mjComponents = new string[]{
            "MjSite",
            "MjBody",
            "MjHingeJoint",
            "MjMeshFilter",
            "MjGeom"
        };


        // Print components of the current transform
        Component[] components = parent.GetComponents<Component>();
        foreach (Component component in components)
        {
            string componentName = component.GetType().Name;
            if (mjComponents.Contains(componentName))
            {
                print($"destorying {componentName}");
                DestroyImmediate(component);
            }
        }
        // Recursively print components of all children
        foreach (Transform child in parent)
        {
            HuntComponents(child);
        }
    }
}