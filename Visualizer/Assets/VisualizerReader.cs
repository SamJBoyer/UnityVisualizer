using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VisualizerReader : MonoBehaviour
{

    [SerializeField] private ArmController _armController;

    private Hardpoint _hardpoint;


    // Start is called before the first frame update
    void Start()
    {
        //declare hardpoint
        string[] channels = new string[] {  
            "test_stream"
        };
        _hardpoint = new Hardpoint(channels, "localhost:6379");
    }

    // Update is called once per frame
    void Update()
    {        
        //float F(Dictionary<string, string> dict) => float.Parse(dict.Values.First());
        //get the dictionary of channel data
        var data = Hardpoint.ChannelDataDict;  
        var reply = data["test_stream"];
        print(reply);

        /*var adjustments = new Dictionary<DOF, float>();
s
        adjustments.Add(DOF.ShoulderFlexion, F(data["shoulderFlexion"]));
        adjustments.Add(DOF.ShoulderAbduction, F(data["shoulderAbduction"]));
        adjustments.Add(DOF.ShoulderRotation, F(data["shoulderRotation"]));
        adjustments.Add(DOF.ElbowFlexion, F(data["elbowFlexion"]));
        adjustments.Add(DOF.WristFlexion, F(data["wristFlexion"]));
        _armController.AdjustAngles(adjustments);*/
    }
}
