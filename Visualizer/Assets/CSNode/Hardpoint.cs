using System.Collections.Generic;
using UnityEngine;
using System;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Author: Sam Boyer
/// Email: Sam.James.Boyer@gmail.com
/// 
/// This script extends the CSNode with additional features to make reading and writing to redis streams easier
/// 
/// </summary>

public class Hardpoint : UnityNode
{
    private Dictionary<string, Dictionary<string, string>> _instantDataDict; //dictionary of the most recent entry in the channel

    //a queue buffer of the data from a channel. holds the queue and a string of the most recent data.
    private Dictionary<string, (string, Queue<Dictionary<string, string>>)> _channelDataQueue; //queue of all the channel data

    private List<Task> _readingTasks;

    //adding an override string as an argument means the node will connect synchronously and should only be used when
    //debugging from the unity editor 
    public Hardpoint(string[] channels, bool runOpen = true) : base(runOpen)
    {
        _instantDataDict = new Dictionary<string, Dictionary<string, string>>();
        _channelDataQueue = new Dictionary<string, (string, Queue<Dictionary<string, string>>)>();
        _readingTasks = new List<Task>();
        foreach (string channelName in channels)
        {
            _instantDataDict.Add(channelName, new Dictionary<string, string>());
            _channelDataQueue.Add(channelName, (string.Empty, new Queue<Dictionary<string, string>>()));
            _readingTasks.Add(ReadFromStreamAsync(channelName));
        }
        Debug.Log("starting hardpoint");
        base.Run();
    }

    //if no channels are given as an argument, automatically read from input_streams 
    public Hardpoint(bool runOpen = true) : base(runOpen)
    {
        Debug.Log("starting hardpoint");
        _instantDataDict = new Dictionary<string, Dictionary<string, string>>();
        _channelDataQueue = new Dictionary<string, (string, Queue<Dictionary<string, string>>)>();
        StartListenersFromGraph();
        base.Run();
    }

    //add values from input_stream property of the parameters to the list of tasks to listen to 
    private void StartListenersFromGraph()
    {
        var tasks = new List<Task>();
        foreach (KeyValuePair<string, object> kvp in _parameters)
        {
            Debug.Log($"param key: {kvp.Key} | param value: {kvp.Value}");
        }

        if (_parameters.ContainsKey("input_streams"))
        {
            var inputStreamObj = _parameters["input_streams"];
            try
            {
                var inputStreamArray = JsonConvert.DeserializeObject<string[]>(inputStreamObj.ToString());
                foreach (string inputStream in inputStreamArray)
                {
                    Debug.Log($"adding {inputStream} to reading task list");
                    tasks.Add(ReadFromStreamAsync(inputStream));
                }
            }
            catch (Exception ex)
            {
                //Debug.LogWarning("could not load streams from graph. please declare input streams manually");
                Debug.LogWarning(ex);
            }
        }
        else
        {
            Debug.Log("no input streams declared in graph");
        }
        //return tasks;
        _readingTasks = tasks;
    }

    protected override async void Work()
    {
        try
        {
            await Task.WhenAll(_readingTasks);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    //returns a task of writing a string to a stream 
    public async Task WriteToStream(string key, string entryName, string entry)
    {
        NameValueEntry newEntry = new NameValueEntry(entryName, entry);
        await _database.StreamAddAsync(key, new NameValueEntry[] { newEntry });
    }

    //returns the task of reading an entry from a channel in the database
    private async Task ReadFromStreamAsync(string channelName)
    {
        //application playing should be removed because this is a unity-specific feature 
        while (Application.isPlaying && _currentState.Equals(Status.NODE_READY))
        { // Application is playing is basically a replacement for SIGINT
            await Task.Run(async () =>
            {
                var dataQueueCollection = _channelDataQueue[channelName];
                var result = await _database.StreamRangeAsync(channelName, "-", "+", 1, Order.Descending);
                if (result.Any())
                {
                    var entry = result.First();
                    string latestID = dataQueueCollection.Item1;
                    if (entry.Id != latestID)
                    {
                        var dataDict = ParseResult(entry);
                        var dataBuffer = dataQueueCollection.Item2;
                        dataBuffer.Enqueue(dataDict); // add data to the buffer
                        _channelDataQueue[channelName] = (entry.Id, dataBuffer);
                    }
                }
            });
        }
        Debug.LogWarning("hardpoint is stopping reading tasks");
    }

    Dictionary<string, string> ParseResult(StreamEntry entry) =>
entry.Values.ToDictionary(x => x.Name.ToString(), x =>
x.Value.ToString());

    public Dictionary<string, string> GetInstantData(string channelName)
    {
        return _instantDataDict[channelName];
    }

    public Queue<Dictionary<string, string>> GetChannelDataQueue(string channelName)
    {
        return _channelDataQueue[channelName].Item2;
    }

    public Dictionary<string, string> DequeueData(string channelName)
    {
        //Debug.Log($"data: {_channelDataQueue[channelName].Item2.Count}");
        try
        {
            var data = _channelDataQueue[channelName].Item2.Dequeue();
            return data;
        }
        catch
        {
            return null;
        }

    }

}
