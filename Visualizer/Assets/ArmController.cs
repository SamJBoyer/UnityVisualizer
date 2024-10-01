using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
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
    public bool MakeTarget;
    //updating itself from this script 

    [SerializeField] private GameObject _shoulderFocus, _elbowFocus, _wristFocus;
    [SerializeField] private GameObject _armObj;
    [SerializeField] private Material _targetMaterial;
    [SerializeField] private Transform _bodyTransform;


    #region  //transform fields
    [SerializeField] private Transform _shoulderAbductor, _shoulderFlexor, _shoulderRotator;
    [SerializeField] private Transform _elbowFlexor;
    [SerializeField] private Transform _wristSupinator, _wristAbductor, _wristFlexor;
    [SerializeField] private Transform _indexFingerAbductor, _indexFinger1, _indexFinger2, _indexFinger3;
    [SerializeField] private Transform _middleFingerAbductor, _middleFinger1, _middleFinger2, _middleFinger3;
    [SerializeField] private Transform _ringFingerAbductor, _ringFinger1, _ringFinger2, _ringFinger3;
    [SerializeField] private Transform _palmRoller, _pinkyFingerAbductor, _pinkyFinger1, _pinkyFinger2, _pinkyFinger3;
    [SerializeField] private Transform _thumbRoller, _thumbAbductor, _thumbFinger1, _thumbFinger2;
    #endregion

    #region //angle fields
    [SerializeField] private float _shoulderAbductionAngle, _shoulderFlexionAngle, _shoulderRotationAngle;
    [SerializeField] private float _elbowFlexionAngle;
    [SerializeField] private float _wristSupinationAngle, _wristAbductionAngle, _wristFlexionAngle;
    [SerializeField] private float _indexAbductionAngle, _index1Angle, _index2Angle, _index3Angle;
    [SerializeField] private float _middleAbductionAngle, _middle1Angle, _middle2Angle, _middle3Angle;
    [SerializeField] private float _ringAbductionAngle, _ring1Angle, _ring2Angle, _ring3Angle;
    [SerializeField] private float _palmRollAngle, _pinkyAbductionAngle, _pinky1Angle, _pinky2Angle, _pinky3Angle;
    [SerializeField] private float _thumbRollAngle, _thumbAbductionAngle, _thumb1Angle, _thumb2Angle;
    #endregion

    //the armature that this arm controller will visually reflect 
    private ArmatureStructure _myArmature;
    private ArmatureStructure _defaultArmature;

    private Dictionary<DOF, float[]> jointLimitDict;

    private Dictionary<Focus, GameObject> _focusObjectDict;

    public bool ImposeJointLimits;

    //dictionary relating the object transforms to the DOF they represent
    private Dictionary<DOF, Transform> _transformsByDOF;

    //dictionary relating the DOF to the axis of rotation
    private Dictionary<DOF, Vector3> _jointAxisByDOFs;

    private void Start()
    {
        //string path = Path.Combine(Application.streamingAssetsPath, "Poses", "Default.json");
        //_defaultArmature = ArmatureStructure.LoadArmatureFromFile(path);
        //_myArmature = _defaultArmature;
        //FindJointLimits();
        //UpdateArm();
        if (MakeTarget)
        {
            MakeTransparent();
        }

        //load the armature axis
        string axisPath = Path.Combine(Application.streamingAssetsPath, "axis.json");
        if (File.Exists(axisPath))
        {
            string json = File.ReadAllText(axisPath);
            var jointAxisDict = JsonConvert.DeserializeObject<Dictionary<DOF, float[]>>(json);
            _jointAxisByDOFs = new Dictionary<DOF, Vector3>();
            foreach (var kvp in jointAxisDict)
            {
                DOF dof = kvp.Key;
                float[] values = kvp.Value;
                Vector3 axisVec = new Vector3(values[0], values[1], values[2]);
                _jointAxisByDOFs[dof] = axisVec;
            }
        }
        else
        {
            Debug.LogError("couldn't find axis file");
        }

        _transformsByDOF = new Dictionary<DOF, Transform>()
        {
            {DOF.INDEXABDUCTION, _indexFingerAbductor},
            { DOF.INDEX1, _indexFinger1 },
            { DOF.INDEX2, _indexFinger2 },
            { DOF.INDEX3, _indexFinger3 },
            { DOF.MIDDLEABDUCTION, _middleFingerAbductor },
            { DOF.MIDDLE1, _middleFinger1 },
            { DOF.MIDDLE2, _middleFinger2 },
            { DOF.MIDDLE3, _middleFinger3 },
            { DOF.RINGABDUCTION, _ringFingerAbductor },
            { DOF.RING1, _ringFinger1 },
            { DOF.RING2, _ringFinger2 },
            { DOF.RING3, _ringFinger3 },
            { DOF.PINKYABDUCTION, _pinkyFingerAbductor },
            {DOF.PALMROLL, _palmRoller},
            { DOF.PINKY1, _pinkyFinger1 },
            { DOF.PINKY2, _pinkyFinger2 },
            { DOF.PINKY3, _pinkyFinger3 },
            { DOF.THUMBABDUCTION, _thumbAbductor },
            {DOF.THUMBROLL, _thumbRoller},
            { DOF.THUMB1, _thumbFinger1 },
            { DOF.THUMB2, _thumbFinger2 },
            { DOF.SHOULDERABDUCTION, _shoulderAbductor },
            { DOF.SHOULDERFLEXION, _shoulderFlexor },
            { DOF.SHOULDERROTATION, _shoulderRotator },
            { DOF.ELBOWFLEXION, _elbowFlexor },
            { DOF.WRISTFLEXION, _wristFlexor },
            { DOF.WRISTABDUCTION, _wristAbductor },
            { DOF.WRISTSUPINATION, _wristSupinator }
        };


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
        var angles = new Dictionary<DOF, float>
        {
            { DOF.INDEXABDUCTION, _indexAbductionAngle },
            { DOF.INDEX1, _index1Angle },
            { DOF.INDEX2, _index2Angle },
            { DOF.INDEX3, _index3Angle },
            { DOF.MIDDLEABDUCTION, _middleAbductionAngle },
            { DOF.MIDDLE1, _middle1Angle },
            { DOF.MIDDLE2, _middle2Angle },
            { DOF.MIDDLE3, _middle3Angle },
            { DOF.RINGABDUCTION, _ringAbductionAngle },
            { DOF.RING1, _ring1Angle },
            { DOF.RING2, _ring2Angle },
            { DOF.RING3, _ring3Angle },
            { DOF.PINKYABDUCTION, _pinkyAbductionAngle },
            { DOF.PALMROLL, _palmRollAngle },
            { DOF.PINKY1, _pinky1Angle },
            { DOF.PINKY2, _pinky2Angle },
            { DOF.PINKY3, _pinky3Angle },
            { DOF.THUMBABDUCTION, _thumbAbductionAngle },
            { DOF.THUMBROLL, _thumbRollAngle },
            { DOF.THUMB1, _thumb1Angle },
            { DOF.THUMB2, _thumb2Angle },
            { DOF.SHOULDERABDUCTION, _shoulderAbductionAngle },
            { DOF.SHOULDERFLEXION, _shoulderFlexionAngle },
            { DOF.SHOULDERROTATION, _shoulderRotationAngle },
            { DOF.ELBOWFLEXION, _elbowFlexionAngle },
            { DOF.WRISTFLEXION, _wristFlexionAngle },
            { DOF.WRISTABDUCTION, _wristAbductionAngle },
            { DOF.WRISTSUPINATION, _wristSupinationAngle }
        };
        _myArmature = new ArmatureStructure(angles);
    }

    //sets the serialized fields from the armature
    public void SetFieldsFromArmature()
    {
        var dofAngleDict = new Dictionary<DOF, float>(_myArmature.GetValues());

        _index1Angle = dofAngleDict[DOF.INDEX1];
        _index2Angle = dofAngleDict[DOF.INDEX2];
        _index3Angle = dofAngleDict[DOF.INDEX3];

        _middle1Angle = dofAngleDict[DOF.MIDDLE1];
        _middle2Angle = dofAngleDict[DOF.MIDDLE2];
        _middle3Angle = dofAngleDict[DOF.MIDDLE3];

        _ring1Angle = dofAngleDict[DOF.RING1];
        _ring2Angle = dofAngleDict[DOF.RING2];
        _ring3Angle = dofAngleDict[DOF.RING3];

        _pinky1Angle = dofAngleDict[DOF.PINKY1];
        _pinky2Angle = dofAngleDict[DOF.PINKY2];
        _pinky3Angle = dofAngleDict[DOF.PINKY3];

        _thumb1Angle = dofAngleDict[DOF.THUMB1];
        _thumb2Angle = dofAngleDict[DOF.THUMB2];

        _shoulderAbductionAngle = dofAngleDict[DOF.SHOULDERABDUCTION];
        _shoulderFlexionAngle = dofAngleDict[DOF.SHOULDERFLEXION];
        _shoulderRotationAngle = dofAngleDict[DOF.SHOULDERROTATION];

        _elbowFlexionAngle = dofAngleDict[DOF.ELBOWFLEXION];
        _wristFlexionAngle = dofAngleDict[DOF.WRISTFLEXION];
        _wristAbductionAngle = dofAngleDict[DOF.WRISTABDUCTION];
        _wristSupinationAngle = dofAngleDict[DOF.WRISTSUPINATION];
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
        //SetFieldsFromArmature();

        var currentValues = _myArmature.GetValues();
        foreach (var kvp in currentValues)
        {
            DOF dof = kvp.Key;
            float angle = kvp.Value;
            if (_transformsByDOF.ContainsKey(dof) && _jointAxisByDOFs.ContainsKey(dof) && _transformsByDOF[dof] != null)
            {
                Transform jointTransform = _transformsByDOF[dof];
                Vector3 axis = _jointAxisByDOFs[dof];
                jointTransform.localRotation = Quaternion.AngleAxis(angle, axis);
                print($"setting {dof} to {angle}");

            }
        }
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

