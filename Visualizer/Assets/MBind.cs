using System.Collections;
using System.Collections.Generic;
using Mujoco;
using UnityEngine;

public class MBind : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform acutatorList;

    void Start()
    {
        foreach (Transform actChild in acutatorList)
        {
            var actuator = actChild.gameObject.AddComponent<Mujoco.MjActuator>();
            MjBaseJoint boundJoint = actuator.Joint;
            Transform parent = boundJoint.transform.parent;
            if (boundJoint is MjHingeJoint)
            {
                Vector3 axis = ((MjHingeJoint)boundJoint).RotationAxis;
            }
            else
            {
                Debug.Log("Not a hinge joint");
            }

            //Vector3 jointAxis = (MjHingeJoint)boundJoint.RotationAxis;



        }


    }

    // Update is called once per frame
    void Update()
    {

    }
}

public class ShadowHandController : MonoBehaviour
{

}