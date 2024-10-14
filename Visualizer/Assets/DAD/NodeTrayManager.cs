using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NodeTrayManager : MonoBehaviour
{
    [SerializeField] private Transform scrollContent;
    [SerializeField] private Transform nodeZoneTransform;
    [SerializeField] private GameObject nodeResourcePrefab;

    private void Start()
    {
        LoadResources();
    }

    private void LoadResources()
    {
        string nodeDirectory = Path.Combine(Application.dataPath, "DAD", "Nodes");
        print(nodeDirectory);
        string[] nodes = Directory.GetDirectories(nodeDirectory, "*", SearchOption.AllDirectories);
        foreach (string directory in nodes)
        {
            //get files to verify this node is complete
            string[] directoryFiles = Directory.GetFiles(directory);
            foreach (string file in directoryFiles)
            {
                if (Path.GetExtension(file) == ".yaml")
                {

                }
            }
            GameObject newNodeResource = Instantiate(nodeResourcePrefab, scrollContent);
            newNodeResource.GetComponent<NodeResource>().Initialize(directory, directoryFiles[0], nodeZoneTransform);
        }
    }
}
