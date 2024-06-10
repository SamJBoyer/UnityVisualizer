using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// you might say to yourself, why are we setting every angle individually? isn't it better to just set the x,y,z rotation at once?
/// nope. read about gimbal lock and quaternions. 
/// </summary>

public class ArmController : MonoBehaviour
{
    public Transform model;


    [SerializeField] private Transform _shoulderAbductor, _shoulderElevator, _shoulderRotator;
    [SerializeField] private Transform _elbowFlexer;
    [SerializeField] private Transform _wristUpdown, _wristSideside, _wristRotation;


    [SerializeField] private float _shoulderAbductionAngle, _shoulderElevationAngle, _shoulderRotationAngle;
    [SerializeField] private float _elbowFlexion;
    [SerializeField] private float _wristUpdownAngle, _wristSideAngle, _wristRotationAngle;

    private void Start()
    {
        _shoulderAbductor.transform.rotation = Quaternion.identity;
        _shoulderElevator.transform.rotation = Quaternion.identity;
        _shoulderRotator.transform.rotation = Quaternion.identity;

        _elbowFlexer.transform.rotation = Quaternion.identity;
        
        _wristUpdown.transform.rotation = Quaternion.identity;
        _wristSideside.transform.rotation = Quaternion.identity;
        _wristRotation.transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        //print(_shoulderAbductionAngle + " " + _shoulderElevationAngle + " " + _shoulderRotationAngle);

        _shoulderAbductor.localRotation = Quaternion.Euler(new Vector3(0, _shoulderAbductionAngle, 0));
        _shoulderElevator.localRotation = Quaternion.Euler(new Vector3(0, 0, _shoulderElevationAngle));
        _shoulderRotator.localRotation = Quaternion.Euler(new Vector3(_shoulderRotationAngle, 0, 0));

        //print(_elbowAbductionAngle + " " + _elbowElevationAngle + " " + _elbowRotation);

        _elbowFlexer.localRotation = Quaternion.Euler(new Vector3(_elbowFlexion, 0, 0));

        _wristUpdown.transform.localRotation = Quaternion.Euler(new Vector3(_wristSideAngle, 0, 0)); ;
        _wristSideside.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, _wristUpdownAngle)); ;
        _wristRotation.transform.localRotation = Quaternion.Euler(new Vector3(0, _wristRotationAngle, 0)); ;
    }
}
