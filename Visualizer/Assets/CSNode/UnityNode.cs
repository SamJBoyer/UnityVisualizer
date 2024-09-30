using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using StackExchange.Redis;
using System;



public class UnityNode : CSNode
{

    public UnityNode(bool runOpen) : base()
    {
        if (runOpen)
        {
            ConnectFromUnity();
        }
        else
        {
            base.ConnectToRedis();
        }
    }

    //replace teh connect to redis method with a dummy method to effectively override the constructor 
    protected override void ConnectToRedis() { }

    //used to connect to redis synchronously. used when running the node from editor and is called if node
    //is instantiated with an override string as an argument 
    protected void ConnectFromUnity()
    {

        //connect using connection string 
        try
        {
            ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect("localHost");
            _database = _redis.GetDatabase();
            _currentState = Status.NODE_READY;
            Debug.Log($"attempting open connection from unity to local host");
        }
        catch (Exception ex)
        {
            Debug.LogError($"A connection error occurred: {ex}");
            _currentState = Status.NODE_FATAL_ERROR;
        }
    }

}
