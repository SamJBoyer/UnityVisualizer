using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class VisualizerReader : MonoBehaviour
{
    private Hardpoint _hardpoint;

    public static float ShoulderX, ShoulderY, ShoulderZ;
    public static float ElbowDeg; 
    public static float GraspDeg;


    // Start is called before the first frame update
    void Start()
    {
        //set up the channels this hardpoint will be listening to
        RedisChannel[] channels = {
            new RedisChannel("ShoulderX", false),
            new RedisChannel("ShoulderY", false),
            new RedisChannel("ShoulderZ", false),
            new RedisChannel("ElbowDeg", false),
            new RedisChannel("GraspDeg", false),
        };
        //declare hardpoint
        _hardpoint = new Hardpoint(channels);
    }

    // Update is called once per frame
    void Update()
    {        
        float GetFloat(Dictionary<string, string> dict) => float.Parse(dict.Values.First());
        //get the dictionary of channel data
        var data = Hardpoint.InstChannelData;  

        //read data from REDIS
        ShoulderX = GetFloat(data["ShoulderX"]);
        ShoulderY = GetFloat(data["ShoulderY"]);
        ShoulderZ = GetFloat(data["ShoulderZ"]);
        ElbowDeg = GetFloat(data["ElbowDeg"]);
        GraspDeg = GetFloat(data["GraspDeg"]);
    }

}
