using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using StackExchange.Redis;
using System.Linq;

/// <summary>
/// author: Samuel James Boyer
/// gmail: sam.james.boyer@gmail.com
/// 
/// this class manages access to the brand redis database by accessing the UnityNodeManager
/// </summary>
public class BRANDAccessor
{
    public static UnityNodeManager UnityNodeManager = null;
    //the max number of stream entries that are stored in the buffer
    private int _bufferSize;
    //the name of the stream that the accessor is reading from
    private string _streamName;
    //the buffer that stores the stream entries
    private Queue<StreamEntry> _redisData;
    //the last read stream id 
    private string _streamIDHead = "0-0";


    //custom lambda can be entered via constructor argument. can be used to define a custom way of reading from the redis database
    public BRANDAccessor(string streamName, int bufferSize, Action<UnityNode> customLambda = null)
    {
        //get an reference to the unity node component
        if (UnityNodeManager == null)
        {
            UnityNodeManager = new GameObject("UnityNodeManager").AddComponent<UnityNodeManager>();
        }
        _bufferSize = bufferSize;
        _streamName = streamName;
        _redisData = new Queue<StreamEntry>(_bufferSize);
        //by default the accessor will read the latest data from the stream and add it to the buffer unless custom lambda is given
        UnityNodeManager.AddReadTask(customLambda == null ? (x) => AddLatestToBuffer(x) : (x) => customLambda(x));
    }

    //gets the latest data and adds it to data buffer. Does not read duplicates 
    private async void AddLatestToBuffer(UnityNode unityNode)
    {
        //StreamEntry? latestData = await unityNode.ReadLatestAsync(_streamName);

        StreamEntry[] result = await unityNode.GetDatabase().StreamRangeAsync(_streamName, "-", "+", 1, Order.Descending);
        if (result.Any())
        {
            StreamEntry entry = result.First();
            if (entry.Id != _streamIDHead)
            {
                _streamIDHead = entry.Id;
                this.EnqueueData(entry);
            }
        }
    }

    public void EnqueueData(StreamEntry newData)
    {
        if (_redisData.Count >= _bufferSize)
        {
            _redisData.Dequeue();
        }
        _redisData.Enqueue(newData);
    }

    public StreamEntry DequeueData()
    {
        _redisData.TryDequeue(out StreamEntry data);
        return data;
    }


    //wrapper for write to redis that passes this message request as a task
    public static void WriteToRedis<T>(string streamName, Dictionary<string, T> data)
    {
        UnityNodeManager.AddWriteTask(streamName, data);
    }
}

