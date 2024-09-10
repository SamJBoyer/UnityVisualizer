using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.IO;
using static ArmatureStructure;

/// <summary>
/// Author: Sam Boyer
/// Gmail: Sam.James.Boyer@gmail.com
/// </summary>

public enum Focus
{
    Shoulder,
    Elbow,
    Wrist
}


public class ArmController : MonoBehaviour
{

    public bool Immobalize = true; //if the arm is immobalized. this is used by the task tray to prevent the arm from
    //updating itself from this script 

    [SerializeField] private GameObject _shoulderFocus, _elbowFocus, _wristFocus;
    [SerializeField] private GameObject _armObj;
    [SerializeField] private Material _targetMaterial;
    [SerializeField] private Transform _bodyTransform;


    #region  //transform fields

    [SerializeField] private Transform _shoulderAbductor, _shoulderFlexor, _shoulderRotator;
    [SerializeField] private Transform _elbowFlexor;
    [SerializeField] private Transform _wristRotation, _wristAbductor, _wristFlexor;


    [SerializeField] private Transform _indexFinger1, _indexFinger2, _indexFinger3;
    [SerializeField] private Transform _middleFinger1, _middleFinger2, _middleFinger3;
    [SerializeField] private Transform _ringFinger1, _ringFinger2, _ringFinger3;
    [SerializeField] private Transform _pinkyFinger1, _pinkyFinger2, _pinkyFinger3;
    [SerializeField] private Transform _thumbFinger1, _thumbFinger2, _thumbFinger3;

    #endregion

    [SerializeField] private float _indexFinger1Rot, _indexFinger2Rot, _indexFinger3Rot;
    [SerializeField] private float _middleFinger1Rot, _middleFinger2Rot, _middleFinger3Rot;
    [SerializeField] private float _ringFinger1Rot, _ringFinger2Rot, _ringFinger3Rot;
    [SerializeField] private float _pinkyFinger1Rot, _pinkyFinger2Rot, _pinkyFinger3Rot;
    [SerializeField] private float _thumbFinger1Rot, _thumbFinger2Rot, _thumbFinger3Rot;

    [SerializeField] private float _shoulderAbductionAngle, _shoulderElevationAngle, _shoulderRotationAngle;
    [SerializeField] private float _elbowFlexionAngle;
    [SerializeField] private float _wristRotationAngle, _wristAbductionAngle, _wristFlexionAngle;

    private ArmatureStructure _myArmature;
    private ArmatureStructure _defaultArmature;

    private Dictionary<DOF, float[]> jointLimitDict;

    private Dictionary<Focus, GameObject> _focusObjectDict;

    public bool ImposeJointLimits;

    private void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Poses", "Default.json");
        _defaultArmature = ArmatureStructure.LoadArmatureFromFile(path);
        _myArmature = _defaultArmature;
        FindJointLimits();
        UpdateArm();
    }

    private void FindJointLimits()
    {
        string fileName = "jointlimits.json";
        //print($"loading {fileName}");
        var limitInterpretDict = new Dictionary<string, float[]>();
        try
        {
            jointLimitDict = new Dictionary<DOF, float[]>();
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            string jsonString = File.ReadAllText(filePath);
            limitInterpretDict = JsonConvert.DeserializeObject<Dictionary<string, float[]>>(jsonString);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"couldn't load joint limits {ex}");
        }


        if (limitInterpretDict.ContainsKey("ShoulderFlexion"))
        {
            jointLimitDict[DOF.SHOULDERFLEXION] = limitInterpretDict["ShoulderFlexion"];
        }

        if (limitInterpretDict.ContainsKey("ShoulderAbduction"))
        {
            jointLimitDict[DOF.SHOULDERFLEXION] = limitInterpretDict["ShoulderAbduction"];
        }

        if (limitInterpretDict.ContainsKey("ShoulderRotation"))
        {
            jointLimitDict[DOF.SHOULDERROTATION] = limitInterpretDict["ShoulderRotation"];
        }

        if (limitInterpretDict.ContainsKey("ElbowFlexion"))
        {
            jointLimitDict[DOF.ELBOWFLEXION] = limitInterpretDict["ElbowFlexion"];
        }

        if (limitInterpretDict.ContainsKey("WristRotation"))
        {
            jointLimitDict[DOF.WRISTSUPINATION] = limitInterpretDict["WristRotation"];
        }

        if (limitInterpretDict.ContainsKey("WristAbduction"))
        {
            jointLimitDict[DOF.WRISTABDUCTION] = limitInterpretDict["WristAbduction"];
        }

        if (limitInterpretDict.ContainsKey("WristFlexion"))
        {
            jointLimitDict[DOF.WRISTFLEXION] = limitInterpretDict["WristFlexion"];
        }

        if (limitInterpretDict.ContainsKey("nonThumbFingers"))
        {
            float[] limit = limitInterpretDict["nonThumbFingers"];
            jointLimitDict[DOF.INDEX1] = limit;
            jointLimitDict[DOF.INDEX2] = limit;
            jointLimitDict[DOF.INDEX3] = limit;
            jointLimitDict[DOF.MIDDLE1] = limit;
            jointLimitDict[DOF.MIDDLE2] = limit;
            jointLimitDict[DOF.MIDDLE3] = limit;
            jointLimitDict[DOF.RING1] = limit;
            jointLimitDict[DOF.RING2] = limit;
            jointLimitDict[DOF.RING3] = limit;
            jointLimitDict[DOF.PINKY1] = limit;
            jointLimitDict[DOF.PINKY2] = limit;
            jointLimitDict[DOF.PINKY3] = limit;
        }
    }


    private void LimitJoints()
    {
        var currentValues = _myArmature.GetValues();
        var tempDict = new Dictionary<DOF, float>();
        foreach (var masterKVP in currentValues)
        {
            if (jointLimitDict.ContainsKey(masterKVP.Key))
            {
                var limitKVP = jointLimitDict[masterKVP.Key];
                float lowerLimit = limitKVP[0];
                float upperLimit = limitKVP[1];
                var value = currentValues[masterKVP.Key];
                var clamped = Mathf.Clamp(value, lowerLimit, upperLimit);
                tempDict[masterKVP.Key] = clamped;
            }
        }
        _myArmature = new ArmatureStructure(tempDict);

    }

    //makes the green orbs visable for a task 
    public void SetFocuses(List<Focus> focuses)
    {
        _focusObjectDict = new Dictionary<Focus, GameObject> {
            {Focus.Shoulder, _shoulderFocus},
            {Focus.Elbow, _elbowFocus},
            {Focus.Wrist, _wristFocus}
        };
        foreach (GameObject gm in _focusObjectDict.Values)
        {
            gm.SetActive(false);
        }

        foreach (Focus focus in focuses)
        {
            print(focus.ToString());
            _focusObjectDict[focus].SetActive(true);
        }
    }

    //update the armature from the public fields
    public void SetArmatureFromFields()
    {
        var dofAngleDict = new Dictionary<DOF, float>
        {
            { DOF.INDEX1, _indexFinger1Rot },
            { DOF.INDEX2, _indexFinger2Rot },
            { DOF.INDEX3, _indexFinger3Rot },
            { DOF.MIDDLE1, _middleFinger1Rot },
            { DOF.MIDDLE2, _middleFinger2Rot },
            { DOF.MIDDLE3, _middleFinger3Rot },
            { DOF.RING1, _ringFinger1Rot },
            { DOF.RING2, _ringFinger2Rot },
            { DOF.RING3, _ringFinger3Rot },
            { DOF.PINKY1, _pinkyFinger1Rot },
            { DOF.PINKY2, _pinkyFinger2Rot },
            { DOF.PINKY3, _pinkyFinger3Rot },
            { DOF.THUMB1, _thumbFinger1Rot },
            { DOF.THUMB2, _thumbFinger2Rot },
            { DOF.THUMB3, _thumbFinger3Rot },
            { DOF.SHOULDERABDUCTION, _shoulderAbductionAngle },
            { DOF.SHOULDERFLEXION, _shoulderElevationAngle },
            { DOF.SHOULDERROTATION, _shoulderRotationAngle },
            { DOF.ELBOWFLEXION, _elbowFlexionAngle },
            { DOF.WRISTFLEXION, _wristFlexionAngle },
            { DOF.WRISTABDUCTION, _wristAbductionAngle },
            { DOF.WRISTSUPINATION, _wristRotationAngle }
        };
        _myArmature = new ArmatureStructure(dofAngleDict);
    }

    //sets the serialized fields from the armature
    public void SetFieldsFromArmature()
    {
        var dofAngleDict = new Dictionary<DOF, float>(_myArmature.GetValues());

        _indexFinger1Rot = dofAngleDict[DOF.INDEX1];
        _indexFinger2Rot = dofAngleDict[DOF.INDEX2];
        _indexFinger3Rot = dofAngleDict[DOF.INDEX3];

        _middleFinger1Rot = dofAngleDict[DOF.MIDDLE1];
        _middleFinger2Rot = dofAngleDict[DOF.MIDDLE2];
        _middleFinger3Rot = dofAngleDict[DOF.MIDDLE3];

        _ringFinger1Rot = dofAngleDict[DOF.RING1];
        _ringFinger2Rot = dofAngleDict[DOF.RING2];
        _ringFinger3Rot = dofAngleDict[DOF.RING3];

        _pinkyFinger1Rot = dofAngleDict[DOF.PINKY1];
        _pinkyFinger2Rot = dofAngleDict[DOF.PINKY2];
        _pinkyFinger3Rot = dofAngleDict[DOF.PINKY3];

        _thumbFinger1Rot = dofAngleDict[DOF.THUMB1];
        _thumbFinger2Rot = dofAngleDict[DOF.THUMB2];
        _thumbFinger3Rot = dofAngleDict[DOF.THUMB3];

        _shoulderAbductionAngle = dofAngleDict[DOF.SHOULDERABDUCTION];
        _shoulderElevationAngle = dofAngleDict[DOF.SHOULDERFLEXION];
        _shoulderRotationAngle = dofAngleDict[DOF.SHOULDERROTATION];

        _elbowFlexionAngle = dofAngleDict[DOF.ELBOWFLEXION];
        _wristFlexionAngle = dofAngleDict[DOF.WRISTFLEXION];
        _wristAbductionAngle = dofAngleDict[DOF.WRISTABDUCTION];
        _wristRotationAngle = dofAngleDict[DOF.WRISTSUPINATION];
    }


    //sets the joint angles to their nominal field position once a frame. 
    private void Update()
    {
        if (!Immobalize)
        {
            UpdateArm();
        }
    }

    public void UpdateArm()
    {
        SetArmatureFromFields();
        if (ImposeJointLimits && jointLimitDict.Count > 0)
        {
            //LimitJoints();
        }
        SetFieldsFromArmature();

        _shoulderAbductor.localRotation = Quaternion.Euler(new Vector3(0, _shoulderAbductionAngle, 0));
        _shoulderFlexor.localRotation = Quaternion.Euler(new Vector3(_shoulderElevationAngle, 0, 0));
        _shoulderRotator.localRotation = Quaternion.Euler(new Vector3(0, _shoulderRotationAngle, 0));

        _elbowFlexor.localRotation = Quaternion.Euler(new Vector3(0, 0, _elbowFlexionAngle));

        _wristRotation.transform.localRotation = Quaternion.Euler(new Vector3(0, _wristRotationAngle, 0));
        _wristAbductor.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, _wristAbductionAngle));
        _wristFlexor.transform.localRotation = Quaternion.Euler(new Vector3(_wristFlexionAngle, 0, 0));

        _indexFinger1.transform.localRotation = Quaternion.Euler(new Vector3(_indexFinger1Rot, 0, 0));
        _indexFinger2.transform.localRotation = Quaternion.Euler(new Vector3(_indexFinger2Rot, 0, 0));
        _indexFinger3.transform.localRotation = Quaternion.Euler(new Vector3(_indexFinger3Rot, 0, 0));

        _middleFinger1.transform.localRotation = Quaternion.Euler(new Vector3(_middleFinger1Rot, 0, 0));
        _middleFinger2.transform.localRotation = Quaternion.Euler(new Vector3(_middleFinger2Rot, 0, 0));
        _middleFinger3.transform.localRotation = Quaternion.Euler(new Vector3(_middleFinger3Rot, 0, 0));

        _ringFinger1.transform.localRotation = Quaternion.Euler(new Vector3(_ringFinger1Rot, 0, 0));
        _ringFinger2.transform.localRotation = Quaternion.Euler(new Vector3(_ringFinger2Rot, 0, 0));
        _ringFinger3.transform.localRotation = Quaternion.Euler(new Vector3(_ringFinger3Rot, 0, 0));

        _pinkyFinger1.transform.localRotation = Quaternion.Euler(new Vector3(_pinkyFinger1Rot, 0, 0));
        _pinkyFinger2.transform.localRotation = Quaternion.Euler(new Vector3(_pinkyFinger2Rot, 0, 0));
        _pinkyFinger3.transform.localRotation = Quaternion.Euler(new Vector3(_pinkyFinger3Rot, 0, 0));

        _thumbFinger1.transform.localRotation = Quaternion.Euler(new Vector3(_thumbFinger1Rot, 0, 0));
        _thumbFinger2.transform.localRotation = Quaternion.Euler(new Vector3(_thumbFinger2Rot, 0, 0));
        _thumbFinger3.transform.localRotation = Quaternion.Euler(new Vector3(_thumbFinger3Rot, 0, 0));

    }

    //inputs a dictionary of DOFS with their new values. this method then sets the DOF dict to reflect these changes.
    //this is the easiest way to make controlled changes to the visualized arm 
    public void AdjustAngles(Dictionary<DOF, float> adjustments)
    {
        SetArmatureFromFields();
        ArmatureStructure adjustmentArmature = new ArmatureStructure(adjustments);
        _myArmature = adjustmentArmature.Union(_myArmature);
        SetFieldsFromArmature();
    }

    public void SetArmature(ArmatureStructure armature)
    {
        _myArmature = armature;
        SetFieldsFromArmature();
    }


    //makes the arm transparent. used by the TaskTray to indicate which arm is the Master arm and which one is the Target arm 
    public void MakeTransparent()
    {
        _bodyTransform.GetComponent<SkinnedMeshRenderer>().material = _targetMaterial;
    }

    //update the armature to reflect the public fields and then return the armature
    public ArmatureStructure GetCurrentArmature()
    {
        SetArmatureFromFields();
        return _myArmature;
    }
}
