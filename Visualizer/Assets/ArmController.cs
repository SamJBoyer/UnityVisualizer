using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.IO;

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

public enum DOF
{
    ShoulderAbduction,
    ShoulderFlexion,
    ShoulderRotation,
    ElbowFlexion,
    WristAbduction,
    WristFlexion,
    WristSupination,
    Index1, Index2, Index3,
    Middle1, Middle2, Middle3,
    Ring1, Ring2, Ring3,
    Pinky1, Pinky2, Pinky3,
    Thumb1, Thumb2, Thumb3
}


public class ArmController : MonoBehaviour
{


    public bool hideFields = true; //if the transform fields are hidden in the editor. the script that actually performs 
    //this feature has been removed and is being retooled because it was causing massive performance problems 

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

    private Dictionary<DOF, float> masterAngleDict; //dictionary of DOFS and their angles. this dict stores the values of the armature, but the 
    //armature is controlled by the fields directly. 

    private Dictionary<DOF, float[]> jointLimitDict; 

    private Dictionary<Focus, GameObject> _focusObjectDict;

    public bool ImposeJointLimits; 

    private void Start(){
        FindJointLimits();
    }

    private void FindJointLimits()
    {
        string fileName = "jointlimits.json";
        print($"loading {fileName}");
        var limitInterpretDict = new Dictionary<string, float[]>();
        try {
            jointLimitDict = new Dictionary<DOF, float[]>();
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            string jsonString = File.ReadAllText(filePath);
            limitInterpretDict = JsonConvert.DeserializeObject<Dictionary<string, float[]>>(jsonString);
        } catch (Exception ex){
            Debug.LogWarning($"couldn't load joint limits {ex}");
        }
        

        if (limitInterpretDict.ContainsKey("ShoulderFlexion")){
            jointLimitDict[DOF.ShoulderFlexion] = limitInterpretDict["ShoulderFlexion"];
        }

        if (limitInterpretDict.ContainsKey("ShoulderAbduction")){
            jointLimitDict[DOF.ShoulderFlexion] = limitInterpretDict["ShoulderAbduction"];
        }

        if (limitInterpretDict.ContainsKey("ShoulderRotation")){
            jointLimitDict[DOF.ShoulderRotation] = limitInterpretDict["ShoulderRotation"];
        }

        if (limitInterpretDict.ContainsKey("ElbowFlexion")){
            jointLimitDict[DOF.ElbowFlexion] = limitInterpretDict["ElbowFlexion"];
        }

        if (limitInterpretDict.ContainsKey("WristRotation")){
            jointLimitDict[DOF.WristSupination] = limitInterpretDict["WristRotation"];
        }

        if (limitInterpretDict.ContainsKey("WristAbduction")){
            jointLimitDict[DOF.WristAbduction] = limitInterpretDict["WristAbduction"];
        }

        if (limitInterpretDict.ContainsKey("WristFlexion")){
            jointLimitDict[DOF.WristFlexion] = limitInterpretDict["WristFlexion"];
        }
        
        if (limitInterpretDict.ContainsKey("nonThumbFingers")){
            float[] limit = limitInterpretDict["nonThumbFingers"];
            jointLimitDict[DOF.Index1] = limit;
            jointLimitDict[DOF.Index2] = limit;
            jointLimitDict[DOF.Index3] = limit;
            jointLimitDict[DOF.Middle1] = limit;
            jointLimitDict[DOF.Middle2] = limit;
            jointLimitDict[DOF.Middle3] = limit;
            jointLimitDict[DOF.Ring1] = limit;
            jointLimitDict[DOF.Ring2] = limit;
            jointLimitDict[DOF.Ring3] = limit;
            jointLimitDict[DOF.Pinky1] = limit;
            jointLimitDict[DOF.Pinky2] = limit;
            jointLimitDict[DOF.Pinky3] = limit;
        }
    }


    private void LimitJoints(){
        var tempDict = new Dictionary<DOF, float>(masterAngleDict);
        foreach(var masterKVP in tempDict){
            if (jointLimitDict.ContainsKey(masterKVP.Key)){
                var limitKVP = jointLimitDict[masterKVP.Key];
                float lowerLimit = limitKVP[0];
                float upperLimit = limitKVP[1];
                masterAngleDict[masterKVP.Key] = Mathf.Clamp(masterAngleDict[masterKVP.Key], lowerLimit, upperLimit); //clamp between the two limits
            }
        }
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

    //update the master dictionary of DOFS to reflect the values from the serialized fields 
    public void SetDictFromFields()
    {
        masterAngleDict = new Dictionary<DOF, float>
        {
            { DOF.Index1, _indexFinger1Rot },
            { DOF.Index2, _indexFinger2Rot },
            { DOF.Index3, _indexFinger3Rot },
            { DOF.Middle1, _middleFinger1Rot },
            { DOF.Middle2, _middleFinger2Rot },
            { DOF.Middle3, _middleFinger3Rot },
            { DOF.Ring1, _ringFinger1Rot },
            { DOF.Ring2, _ringFinger2Rot },
            { DOF.Ring3, _ringFinger3Rot },
            { DOF.Pinky1, _pinkyFinger1Rot },
            { DOF.Pinky2, _pinkyFinger2Rot },
            { DOF.Pinky3, _pinkyFinger3Rot },
            { DOF.Thumb1, _thumbFinger1Rot },
            { DOF.Thumb2, _thumbFinger2Rot },
            { DOF.Thumb3, _thumbFinger3Rot },
            { DOF.ShoulderAbduction, _shoulderAbductionAngle },
            { DOF.ShoulderFlexion, _shoulderElevationAngle },
            { DOF.ShoulderRotation, _shoulderRotationAngle },
            { DOF.ElbowFlexion, _elbowFlexionAngle },
            { DOF.WristFlexion, _wristFlexionAngle },
            { DOF.WristAbduction, _wristAbductionAngle },
            { DOF.WristSupination, _wristRotationAngle }
        };
    }

    //sets the serialized fields from the DOF dict
    public void SetFieldsFromDict(Dictionary<DOF, float> dict)
    {
        _indexFinger1Rot = dict[DOF.Index1];
        _indexFinger2Rot = dict[DOF.Index2];
        _indexFinger3Rot = dict[DOF.Index3];

        _middleFinger1Rot = dict[DOF.Middle1];
        _middleFinger2Rot = dict[DOF.Middle2];
        _middleFinger3Rot = dict[DOF.Middle3];

        _ringFinger1Rot = dict[DOF.Ring1];
        _ringFinger2Rot = dict[DOF.Ring2];
        _ringFinger3Rot = dict[DOF.Ring3];

        _pinkyFinger1Rot = dict[DOF.Pinky1];
        _pinkyFinger2Rot = dict[DOF.Pinky2];
        _pinkyFinger3Rot = dict[DOF.Pinky3];

        _thumbFinger1Rot = dict[DOF.Thumb1];
        _thumbFinger2Rot = dict[DOF.Thumb2];
        _thumbFinger3Rot = dict[DOF.Thumb3];

        _shoulderAbductionAngle = dict[DOF.ShoulderAbduction];
        _shoulderElevationAngle = dict[DOF.ShoulderFlexion];
        _shoulderRotationAngle = dict[DOF.ShoulderRotation];

        _elbowFlexionAngle = dict[DOF.ElbowFlexion];
        _wristFlexionAngle = dict[DOF.WristFlexion];
        _wristAbductionAngle = dict[DOF.WristAbduction];
        _wristRotationAngle = dict[DOF.WristSupination];
    }

    //creates a new json file with the DOF dict saved 
    public void SavePose(string fileName)
    {
        SetDictFromFields();
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "Poses", $"{fileName}.json");
            print(filePath);
            // Serialize the dictionary to a JSON string
            string jsonString = JsonConvert.SerializeObject(masterAngleDict);
            // Write the JSON string to a file
            File.WriteAllText(filePath, jsonString);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    //loads a pose from a JSON file of a DOF dict 
    public void LoadPose(string fileName)
    {
        print($"loading {fileName}");
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "Poses", $"{fileName}");
            string jsonString = File.ReadAllText(filePath);
            masterAngleDict = JsonConvert.DeserializeObject<Dictionary<DOF, float>>(jsonString);
            SetFieldsFromDict(masterAngleDict);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    //sets the joint angles to their nominal field position once a frame. 
    void Update()
    {
        SetDictFromFields();
        if (ImposeJointLimits && jointLimitDict.Count > 0){
            LimitJoints();
        }
        SetFieldsFromDict(masterAngleDict);

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
        SetDictFromFields();
        foreach (KeyValuePair<DOF, float> kvp in adjustments)
        {
            DOF key = kvp.Key;
            if (masterAngleDict.ContainsKey(key))
            {
                masterAngleDict[key] = kvp.Value;
            }
        }
        SetFieldsFromDict(masterAngleDict);
    }




    //makes the arm transparent. used by the TaskTray to indicate which arm is the Master arm and which one is the Target arm 
    public void MakeTransparent()
    {
        _bodyTransform.GetComponent<SkinnedMeshRenderer>().material = _targetMaterial;
    }


    public Dictionary<DOF, float> GetJointAngles()
    {
        SetDictFromFields();
        return masterAngleDict;
    }
}
