using System.Collections.Generic;
using UnityEngine;
using System;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

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
    //the most recently read id for each stream. used to read only new data
    private Dictionary<string, string> _lastReadStreamIDs;

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

    /// <summary>
    /// recognized types: 
    /// special types:
    /// 
    /// </summary>
    /// 

    Dictionary<string, Type> oof = new Dictionary<string, Type>()
    {
        {"hello", typeof(byte)}
    };

    //make the first redis method of getting the dtypes of the data in this stream
    public void InitializeStream(string streamName, Dictionary<string, Type> dtypes)
    {
        if (_database.StreamLength(streamName) > 0)
        {
            Debug.LogWarning("stream already exists. dtypes will not be initialized");
            return;
        }

        List<NameValueEntry> dtypeList = new List<NameValueEntry>();
        foreach (var kvp in dtypes)
        {
            string entryName = kvp.Key;
            Type type = kvp.Value;
            if (type.IsPrimitive)
            {
                dtypeList.Add(new NameValueEntry(entryName, type.ToString()));
            }
        }
        _database.StreamAdd(streamName, dtypeList.ToArray());
    }

    public StreamEntry? GetStreamInit(string streamName)
    {
        var firstEntry = _database.StreamRange(streamName, "-", "+", 1, Order.Ascending);
        return firstEntry.Any() ? firstEntry[0] : null;
    }

    public Dictionary<string, Type> GetStreamInitDtype(string streamName)
    {
        if (GetStreamInit(streamName) is StreamEntry entry)
        {
            Dictionary<string, Type> dtypes = new Dictionary<string, Type>();
            foreach (var kvp in entry.Values)
            {
                string name = kvp.Name;
                string typeString = kvp.Value;
                Type type = Type.GetType(typeString);
                dtypes.Add(name, type);
            }
            return dtypes;
        }

        return null;
    }


    //when sending data that is a primitive or an array of primatives, use this method 
    public async Task WriteToStream<T>(string streamName, Dictionary<string, T> data)
    {
        List<NameValueEntry> packedDataList = new List<NameValueEntry>();
        foreach (var kvp in data)
        {
            string name = kvp.Key;
            T element = kvp.Value;
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string) || (typeof(T).IsArray && typeof(T).GetElementType().IsPrimitive))
            {
                byte[] byteArray = PrimitiveConverter.ToByteArray(element);
                string entry = Convert.ToBase64String(byteArray); // Convert byte array to a base64 string
                NameValueEntry newEntry = new NameValueEntry(name, entry);
                packedDataList.Add(newEntry);
            }
            else
            {
                Debug.LogError("The type T is not a primitive type, string, or array of primitives.");
            }
        }
        await _database.StreamAddAsync(streamName, packedDataList.ToArray());
    }

    public void DecodeRedisEntry(NameValueEntry[] entries, Dictionary<string, string> dtypes = null)
    {
        Dictionary<string, Type> dtypesDict = new Dictionary<string, Type>();
        if (dtypes == null)
        {

        }

        foreach (var entry in entries)
        {
            string name = entry.Name;

        }
    }

    //Returns the newest unread entry of a stream, or null
    public async Task<StreamEntry?> ReadLatestAsync(string channelName)
    {
        var result = await _database.StreamRangeAsync(channelName, "-", "+", 1, Order.Descending);
        if (result.Any())
        {
            StreamEntry entry = result.First();
            if (!_lastReadStreamIDs.TryGetValue(channelName, out string lastId) || entry.Id != lastId)
            {
                _lastReadStreamIDs[channelName] = entry.Id;
                return entry;
            }
        }
        return null;
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
    protected virtual void Run()
    {
        while (true)
        {
            Work();
            UpdateParameters();
        }
    }

    //empty method to be overridden by subclasses as needed
    protected virtual void UpdateParameters() { }
    //empty method to be overridden by subclasses as needed
    protected virtual void Work() { }

    public string GetState() => _currentState.ToString();
    public IDatabase GetDatabase() => _database;
}

public static class PrimitiveConverter
{
    public static byte[] ToByteArray<T>(T value)
    {
        if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
        {
            if (typeof(T) == typeof(int))
            {
                return BitConverter.GetBytes((int)(object)value);
            }
            else if (typeof(T) == typeof(uint))
            {
                return BitConverter.GetBytes((uint)(object)value);
            }
            else if (typeof(T) == typeof(short))
            {
                return BitConverter.GetBytes((short)(object)value);
            }
            else if (typeof(T) == typeof(ushort))
            {
                return BitConverter.GetBytes((ushort)(object)value);
            }
            else if (typeof(T) == typeof(long))
            {
                return BitConverter.GetBytes((long)(object)value);
            }
            else if (typeof(T) == typeof(ulong))
            {
                return BitConverter.GetBytes((ulong)(object)value);
            }
            else if (typeof(T) == typeof(float))
            {
                return BitConverter.GetBytes((float)(object)value);
            }
            else if (typeof(T) == typeof(double))
            {
                return BitConverter.GetBytes((double)(object)value);
            }
            else if (typeof(T) == typeof(bool))
            {
                return BitConverter.GetBytes((bool)(object)value);
            }
            else if (typeof(T) == typeof(char))
            {
                return BitConverter.GetBytes((char)(object)value);
            }
            else if (typeof(T) == typeof(byte))
            {
                return new byte[] { (byte)(object)value };
            }
            else if (typeof(T) == typeof(sbyte))
            {
                return new byte[] { (byte)(sbyte)(object)value };
            }
            else if (typeof(T) == typeof(string))
            {
                return Encoding.UTF8.GetBytes((string)(object)value);
            }
            else
            {
                throw new ArgumentException("Unsupported type");
            }
        }
        else if (typeof(T).IsArray && typeof(T).GetElementType().IsPrimitive)
        {
            Array array = (Array)(object)value;
            int totalLength = 0;
            foreach (var item in array)
            {
                totalLength += ToByteArray(item).Length;
            }

            byte[] result = new byte[totalLength];
            int offset = 0;
            foreach (var item in array)
            {
                byte[] bytes = ToByteArray(item);
                Buffer.BlockCopy(bytes, 0, result, offset, bytes.Length);
                offset += bytes.Length;
            }
            return result;
        }
        else
        {
            throw new ArgumentException("Unsupported type");
        }
    }
}

