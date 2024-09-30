using System.Collections.Generic;
using UnityEngine;
using System;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Author: Sam Boyer
/// Sam.James.Boyer@gmail.com 
/// 
/// This code is a c# implementation of a BRAND node described in 
/// https://github.com/brandbci/brand
/// 
/// Things to change:
///     - sdoes not catch SIGINT for a graceful shutdown. catching SIGINT seems very complicated and not worth
///     - does not allow access through socket 
/// </summary>

public class CSNode
{
    //indicates the status of the current node. This concept is not yet implemented anywhere in BRAND, 
    //to my knowledge, and is largely irrelevant 
    protected enum Status
    {
        NODE_STARTED, //state upon initialization
        NODE_READY, //state after successful connection
        NODE_SHUTDOWN, //never used due to no handling of sigint
        NODE_FATAL_ERROR, //used when connection failure occurs 
        NODE_WARNING, //unused and optional
        NODE_INFO //unused and optional
    }


    protected string _serverSocket; //the socket (path) from command line. currently unimplemented
    protected string _nickname; //nickname of this node recieved from command line 
    protected string _serverIP; //redis IP from command line
    protected string _serverPort; //redis port from command line
    protected Status _currentState;
    protected ConnectionMultiplexer _redis; //redis multiplexer. do not dispose, this object is expensive
    protected IDatabase _database; //data base for reading and writing to redis.
    protected Dictionary<string, object> _parameters; //this nodes parameters, as read from the supergraph
    protected string _stateKeyString; //string indicating this nodes key for itself in the graph_status. will soon have lowered access 

    public CSNode()
    {
        _currentState = Status.NODE_STARTED;
        _parameters = new Dictionary<string, object>();
        ConnectToRedis();
    }

    //overridden in the unity node. Syncronously connects to redis by calling async ConnectToBRANDAsync
    protected virtual void ConnectToRedis()
    {
        string[] args = Environment.GetCommandLineArgs();
        HandleArgs(args); //parses the args into the flags 
        //Debug.Log($"socket {_serverSocket} ip {_serverIP} port {_serverPort}");
        if (_serverSocket != null || (_serverIP != null && _serverPort != null))
        {
            ConnectToBRANDAsync(_serverIP, _serverPort).Wait(); //connect async. currently doesnt handle sockets v
            _stateKeyString = _nickname + "_state";
            _database.StreamAdd(_stateKeyString, new NameValueEntry[] { new NameValueEntry("status", _currentState.ToString()) });
        }
        else
        {
            _currentState = Status.NODE_FATAL_ERROR;
            Debug.LogWarning("insufficient arguments have been passed. node must shut down");
        }

        //async method is called by synchronous method using .Wait()
        async Task ConnectToBRANDAsync(string serverIP, string serverPort, string serverSocket = null)
        {
            string connectionString = serverSocket != null ? serverSocket : $"{serverIP}:{serverPort}"; //prioritize server socket 
            var options = new ConfigurationOptions //can add more custom options as the need arises 
            {
                EndPoints = { connectionString }
            };

            try
            {
                _redis = await ConnectionMultiplexer.ConnectAsync(options);
                _database = _redis.GetDatabase();
                _parameters = ParseGraphParameters();
                _currentState = Status.NODE_READY;
                Debug.Log($"successfully connected from BRAND to {connectionString}");

            }
            catch (Exception ex)
            {
                Debug.LogError($"A connection error occurred: {ex.Message}");
                _currentState = Status.NODE_FATAL_ERROR;
            }
        }

    }

    //interprets the incoming command line arguments by their flags
    private void HandleArgs(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-s":
                    _serverSocket = args[i + 1];
                    break;
                case "-n":
                    _nickname = args[i + 1];
                    break;
                case "-i":
                    _serverIP = args[i + 1];
                    break;
                case "-p":
                    _serverPort = args[i + 1];
                    break;
            }
        }
    }

    //gets a dictionary of parameters from the supergraph stream 
    private Dictionary<string, object> ParseGraphParameters()
    {
        _database = _redis.GetDatabase();
        var values = new Dictionary<string, object>();
        var key = "supergraph_stream"; //at risk due to hardcode 
        var result = _database.StreamRange(key, "-", "+", 1, Order.Descending);

        if (result.Any())
        {
            //painful extraction of the parameters from the SUPER jagged json string 
            string masterJsonString = result[0].Values[0].Value.ToString();
            JObject jobject = JObject.Parse(masterJsonString);
            Dictionary<string, object> dict = jobject.ToObject<Dictionary<string, object>>();
            var nodesString = dict["nodes"].ToString();
            var nodesDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(nodesString);
            if (nodesDict.ContainsKey(_nickname))
            {
                var graphDict = nodesDict[_nickname];
                values = JsonConvert.DeserializeObject<Dictionary<string, object>>(graphDict["parameters"].ToString());
            }
            else
            {
                Debug.LogError("could not find this nodes parameters in the super graph");
            }
        }
        return values;
    }

    //experimental method to grab some parameters from the supergraph stream
    public Dictionary<string, string> GetBuddyParameters(string buddyName)
    {
        var key = "supergraph_stream"; //at risk due to hardcode 
        var result = _database.StreamRange(key, "-", "+", 1, Order.Descending);

        if (result.Any())
        {
            //painful extraction of the parameters from the SUPER jagged json string 
            string masterJsonString = result[0].Values[0].Value.ToString();
            JObject jObject = JObject.Parse(masterJsonString);
            Dictionary<string, object> dict = jObject.ToObject<Dictionary<string, object>>();
            var buddyString = dict["buddies"].ToString();
            var buddyDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(buddyString);
            if (buddyDict.ContainsKey(buddyName))
            {
                return buddyDict[buddyName];
            }
            else
            {
                Debug.LogError("could not find this nodes parameters in the super graph");
            }
        }
        return null;
    }


    //what happend to while true? maybe this is blocked so it can't be in the generic class 
    protected void Run()
    {
        Work();
        UpdateParameters();
    }

    //empty method to be overridden by subclasses as needed
    protected virtual void UpdateParameters() { }
    //empty method to be overridden by subclasses as needed
    protected virtual void Work() { }

    public string GetState() => _currentState.ToString();
    public IDatabase GetDatabase() => _database;
}



