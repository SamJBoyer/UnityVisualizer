using System.Collections;
using System.Collections.Generic;
using Mujoco;
using UnityEngine;

public class thingthing : MonoBehaviour
{
    public MjHingeJoint myHinge;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        print($"config: {myHinge.Configuration} raw: {myHinge.RawConfiguration} axis: {myHinge.RotationAxis}");
    }
}
