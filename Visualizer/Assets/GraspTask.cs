using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GraspTask
{

    public GraspTask(Dictionary<DOF, float> targetAngles, GameObject armObj, Dictionary<DOF, float> poseDict){

        List<DOF> dofs = targetAngles.Keys.ToList();
        List<Focus> focuses = new List<Focus>();

        if (dofs.Contains(DOF.ShoulderAbduction) || dofs.Contains(DOF.ShoulderFlexion) || dofs.Contains(DOF.ShoulderRotation)) {
            focuses.Add(Focus.Shoulder);
        } else if (dofs.Contains(DOF.ElbowFlexion)) {
            focuses.Add(Focus.Elbow);
        } else if (dofs.Contains(DOF.WristAbduction) || dofs.Contains(DOF.WristFlexion) || dofs.Contains(DOF.WristSupination)) {
            focuses.Add(Focus.Elbow);
        }

        GameObject newArm = GameObject.Instantiate(armObj);
        ArmController armController = newArm.GetComponent<ArmController>();
        armController.SetFieldsFromDict(poseDict);
        armController.AdjustAngles(targetAngles);
        armController.SetFocuses(focuses);
        armController.MakeTransparent();

    }

}
