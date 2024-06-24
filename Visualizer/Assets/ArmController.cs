using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UI;
using Unity.VisualScripting;
using Newtonsoft.Json;
using System.IO;

/// <summary>
/// you might say to yourself, why are we setting every angle individually? isn't it better to just set the x,y,z rotation at once?
/// nope. read about gimbal lock and quaternions. 
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




    public bool hideFields = true; //if the transform fields are hidden in the editor. on by default.

    [SerializeField] private GameObject _shoulderFocus, _elbowFocus, _wristFocus;
    [SerializeField] private GameObject _armObj;
    [SerializeField] private Material _targetMaterial;
    [SerializeField] private Transform _bodyTransform;


    #region  //transform fields

    [SerializeField] private Transform _shoulderAbductor, _shoulderElevator, _shoulderRotator;
    [SerializeField] private Transform _elbowFlexer;
    [SerializeField] private Transform _wristUpdown, _wristSideside, _wristRotation;


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
    [SerializeField] private float _elbowFlexion;
    [SerializeField] private float _wristUpdownAngle, _wristSideAngle, _wristRotationAngle;

    private Dictionary<DOF, float> jsonAngleDict;
    private Dictionary<Focus, GameObject> _focusObjectDict;
    public bool ControlByBRAND;


    private void Start()
    {
        ResetRotation();
    }



    public void SetFocuses(List<Focus> focuses)
    {
        _focusObjectDict = new Dictionary<Focus, GameObject> {
            {Focus.Shoulder, _shoulderFocus},
            {Focus.Elbow, _elbowFocus},
            {Focus.Wrist, _wristFocus}
        };
        foreach (GameObject gm in _focusObjectDict.Values){
            gm.SetActive(false);
        }

        foreach (Focus focus in focuses)
        {
            print(focus.ToString());
            _focusObjectDict[focus].SetActive(true);
        }
    }

    public void SetDictFromFields()
    {
        jsonAngleDict = new Dictionary<DOF, float>
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
            { DOF.ElbowFlexion, _elbowFlexion },
            { DOF.WristFlexion, _wristUpdownAngle },
            { DOF.WristAbduction, _wristSideAngle },
            { DOF.WristSupination, _wristRotationAngle }
        };
    }

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

        _elbowFlexion = dict[DOF.ElbowFlexion];
        _wristUpdownAngle = dict[DOF.WristFlexion];
        _wristSideAngle = dict[DOF.WristAbduction];
        _wristRotationAngle = dict[DOF.WristSupination];
    }

    public void SavePose(string fileName)
    {
        SetDictFromFields();
        try
        {
            string filePath = Path.Combine(Application.dataPath, "Poses", $"{fileName}.json");
            print(filePath);
            // Serialize the dictionary to a JSON string
            string jsonString = JsonConvert.SerializeObject(jsonAngleDict);
            // Write the JSON string to a file
            File.WriteAllText(filePath, jsonString);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public void LoadPose(string fileName)
    {
        print($"loading {fileName}");
        try
        {
            string filePath = Path.Combine(Application.dataPath, "Poses", fileName);
            string jsonString = File.ReadAllText(filePath);
            jsonAngleDict = JsonConvert.DeserializeObject<Dictionary<DOF, float>>(jsonString);
            SetFieldsFromDict(jsonAngleDict);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    void Update()
    {
        if (ControlByBRAND)
        {
            //print(_shoulderAbductionAngle + " " + _shoulderElevationAngle + " " + _shoulderRotationAngle);

            _shoulderAbductor.localRotation = Quaternion.Euler(new Vector3(0, VisualizerReader.ShoulderX, 0));
            _shoulderElevator.localRotation = Quaternion.Euler(new Vector3(0, 0, VisualizerReader.ShoulderY));
            _shoulderRotator.localRotation = Quaternion.Euler(new Vector3(VisualizerReader.ShoulderZ, 0, 0));

            //print(_elbowAbductionAngle + " " + _elbowElevationAngle + " " + _elbowRotation);

            _elbowFlexer.localRotation = Quaternion.Euler(new Vector3(VisualizerReader.ElbowDeg, 0, 0));

            _wristUpdown.transform.localRotation = Quaternion.Euler(new Vector3(_wristUpdownAngle, 0, 0));
            _wristSideside.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, _wristSideAngle));
            _wristRotation.transform.localRotation = Quaternion.Euler(new Vector3(0, _wristRotationAngle, 0));

        }
        else
        {
            //print(_shoulderAbductionAngle + " " + _shoulderElevationAngle + " " + _shoulderRotationAngle);

            _shoulderAbductor.localRotation = Quaternion.Euler(new Vector3(0, _shoulderAbductionAngle, 0));
            _shoulderElevator.localRotation = Quaternion.Euler(new Vector3(0, 0, _shoulderElevationAngle));
            _shoulderRotator.localRotation = Quaternion.Euler(new Vector3(_shoulderRotationAngle, 0, 0));

            //print(_elbowAbductionAngle + " " + _elbowElevationAngle + " " + _elbowRotation);

            _elbowFlexer.localRotation = Quaternion.Euler(new Vector3(_elbowFlexion, 0, 0));

            _wristUpdown.transform.localRotation = Quaternion.Euler(new Vector3(_wristUpdownAngle, 0, 0)); ;
            _wristSideside.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, _wristSideAngle)); ;
            _wristRotation.transform.localRotation = Quaternion.Euler(new Vector3(0, _wristRotationAngle, 0)); ;

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

    }

    public void AdjustAngles(Dictionary<DOF, float> adjustments)
    {
        SetDictFromFields();
        foreach (KeyValuePair<DOF, float> kvp in adjustments)
        {
            DOF key = kvp.Key;
            if (jsonAngleDict.ContainsKey(key))
            {
                jsonAngleDict[key] = kvp.Value;
            }
        }
        SetFieldsFromDict(jsonAngleDict);
    }

    private void SetJointRotations(Dictionary<string, float> dict)
    {
        var a = new List<float>(dict.Values);

        _shoulderAbductor.localRotation = Quaternion.Euler(new Vector3(0, a[0], 0));
        _shoulderElevator.localRotation = Quaternion.Euler(new Vector3(0, 0, a[1]));
        _shoulderRotator.localRotation = Quaternion.Euler(new Vector3(a[2], 0, 0));

        _elbowFlexer.localRotation = Quaternion.Euler(new Vector3(a[3], 0, 0));

        _wristUpdown.transform.localRotation = Quaternion.Euler(new Vector3(a[4], 0, 0)); ;
        _wristSideside.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, a[5])); ;
        _wristRotation.transform.localRotation = Quaternion.Euler(new Vector3(0, a[6], 0)); ;

        _indexFinger1.transform.localRotation = Quaternion.Euler(new Vector3(a[7], 0, 0));
        _indexFinger2.transform.localRotation = Quaternion.Euler(new Vector3(a[8], 0, 0));
        _indexFinger3.transform.localRotation = Quaternion.Euler(new Vector3(a[9], 0, 0));

        _middleFinger1.transform.localRotation = Quaternion.Euler(new Vector3(a[10], 0, 0));
        _middleFinger2.transform.localRotation = Quaternion.Euler(new Vector3(a[11], 0, 0));
        _middleFinger3.transform.localRotation = Quaternion.Euler(new Vector3(a[12], 0, 0));

        _ringFinger1.transform.localRotation = Quaternion.Euler(new Vector3(a[13], 0, 0));
        _ringFinger2.transform.localRotation = Quaternion.Euler(new Vector3(a[14], 0, 0));
        _ringFinger3.transform.localRotation = Quaternion.Euler(new Vector3(a[15], 0, 0));

        _pinkyFinger1.transform.localRotation = Quaternion.Euler(new Vector3(a[16], 0, 0));
        _pinkyFinger2.transform.localRotation = Quaternion.Euler(new Vector3(a[17], 0, 0));
        _pinkyFinger3.transform.localRotation = Quaternion.Euler(new Vector3(a[18], 0, 0));

        _thumbFinger1.transform.localRotation = Quaternion.Euler(new Vector3(a[19], 0, 0));
        _thumbFinger2.transform.localRotation = Quaternion.Euler(new Vector3(a[20], 0, 0));
        _thumbFinger3.transform.localRotation = Quaternion.Euler(new Vector3(a[21], 0, 0));
    }

    public void SetJointRotations(float a, float b, float c, float d, float e, float f, float g, float h, float i,
        float j, float k, float l, float m, float n, float o, float p, float q, float r, float s, float t, float u, float v)
    {
        _shoulderAbductor.localRotation = Quaternion.Euler(new Vector3(0, a, 0));
        _shoulderElevator.localRotation = Quaternion.Euler(new Vector3(0, 0, b));
        _shoulderRotator.localRotation = Quaternion.Euler(new Vector3(c, 0, 0));

        _elbowFlexer.localRotation = Quaternion.Euler(new Vector3(d, 0, 0));

        _wristUpdown.transform.localRotation = Quaternion.Euler(new Vector3(e, 0, 0)); ;
        _wristSideside.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, f)); ;
        _wristRotation.transform.localRotation = Quaternion.Euler(new Vector3(0, g, 0)); ;

        _indexFinger1.transform.localRotation = Quaternion.Euler(new Vector3(h, 0, 0));
        _indexFinger2.transform.localRotation = Quaternion.Euler(new Vector3(i, 0, 0));
        _indexFinger3.transform.localRotation = Quaternion.Euler(new Vector3(j, 0, 0));

        _middleFinger1.transform.localRotation = Quaternion.Euler(new Vector3(k, 0, 0));
        _middleFinger2.transform.localRotation = Quaternion.Euler(new Vector3(l, 0, 0));
        _middleFinger3.transform.localRotation = Quaternion.Euler(new Vector3(m, 0, 0));

        _ringFinger1.transform.localRotation = Quaternion.Euler(new Vector3(n, 0, 0));
        _ringFinger2.transform.localRotation = Quaternion.Euler(new Vector3(o, 0, 0));
        _ringFinger3.transform.localRotation = Quaternion.Euler(new Vector3(p, 0, 0));

        _pinkyFinger1.transform.localRotation = Quaternion.Euler(new Vector3(q, 0, 0));
        _pinkyFinger2.transform.localRotation = Quaternion.Euler(new Vector3(r, 0, 0));
        _pinkyFinger3.transform.localRotation = Quaternion.Euler(new Vector3(s, 0, 0));

        _thumbFinger1.transform.localRotation = Quaternion.Euler(new Vector3(t, 0, 0));
        _thumbFinger2.transform.localRotation = Quaternion.Euler(new Vector3(u, 0, 0));
        _thumbFinger3.transform.localRotation = Quaternion.Euler(new Vector3(v, 0, 0));
    }


    public void MakeTransparent()
    {
        _bodyTransform.GetComponent<SkinnedMeshRenderer>().material = _targetMaterial;
    }

    private void ResetRotation()
    {
        _shoulderAbductor.transform.rotation = Quaternion.identity;
        _shoulderElevator.transform.rotation = Quaternion.identity;
        _shoulderRotator.transform.rotation = Quaternion.identity;

        _elbowFlexer.transform.rotation = Quaternion.identity;

        _wristUpdown.transform.rotation = Quaternion.identity;
        _wristSideside.transform.rotation = Quaternion.identity;
        _wristRotation.transform.rotation = Quaternion.identity;
    }

    public Dictionary<DOF, float> GetJointAngles(){
        SetDictFromFields();
        return jsonAngleDict;
    }
}
