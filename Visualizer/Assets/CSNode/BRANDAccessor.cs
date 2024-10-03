using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using StackExchange.Redis;

/// <summary>
/// class that manages access to a the UnityNode
/// </summary>
public class BRANDAccessor
{
    public static UnityNodeManager UnityNodeManager = null;

    public BRANDAccessor(RedisBuffer[] buffers)
    {
        //get an reference to the unity node component
        if (UnityNodeManager == null)
        {
            UnityNodeManager = new GameObject("UnityNodeManager").AddComponent<UnityNodeManager>();
        }

        foreach (RedisBuffer buffer in buffers)
        {
            UnityNodeManager.AddReadTask(buffer.GetBufferMethod());
        }
    }

    //single buffer polymorph
    public BRANDAccessor(RedisBuffer buffer)
    {
        //get an reference to the unity node component
        if (UnityNodeManager == null)
        {
            UnityNodeManager = new GameObject("UnityNodeManager").AddComponent<UnityNodeManager>();
        }
        UnityNodeManager.AddReadTask(buffer.GetBufferMethod());
    }

    //wrapper for write to redis that passes this message request as a task
    public void WriteToRedis<T>(string streamName, Dictionary<string, T> data)
    {
        UnityNodeManager.AddWriteTask(streamName, data);
    }
}

//class that manages the size and type of buffer from redis to unity. assume all data is read via getlatest
public class RedisBuffer
{
    private int _bufferSize;
    private string _streamName;
    private Queue<StreamEntry> _redisData;
    private Action<UnityNode> _customLambda;


    public RedisBuffer(int size, string streamName, Action<UnityNode> customLambda = null)
    {
        _bufferSize = size;
        _streamName = streamName;
        _redisData = new Queue<StreamEntry>(_bufferSize);
        _customLambda = customLambda;
    }
    //allows passing a custom Action as a buffer method if you don't want to just read the latest data into the buffer
    public Action<UnityNode> GetBufferMethod()
    {
        if (_customLambda != null)
        {
            return (node) => _customLambda(node);
        }
        return (x) => AddLatestToBuffer(this, x);
    }

    //gets the latest data and adds it to data buffer. Does not read duplicates 
    private async void AddLatestToBuffer(RedisBuffer buffer, UnityNode unityNode)
    {
        StreamEntry? latestData = await unityNode.ReadLatestAsync(_streamName);
        if (latestData != null)
        {
            EnqueueData(latestData.Value);
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
}