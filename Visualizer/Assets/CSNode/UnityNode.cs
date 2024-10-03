using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using StackExchange.Redis;
using System;


/// <summary>
/// CS Node with special properties because unity is 1 entire thread so there only ever needs to be one connection
/// </summary>
/// 

//is the unity wrapper for unity node that allows it to be a scene component. in charge of managing a task list which is then passed to the unity node for action
public class UnityNodeManager : MonoBehaviour
{
    private UnityNode _node;
    public static List<BRANDAccessor> _accessors;

    //these are tasks where the program is CONSTANTLY running their threads. These threads should never terminate. Mostly used for reading in information 
    //from redis database 
    private List<Task> _readTasks = new List<Task>();

    private void Start()
    {
        _node = new UnityNode(true);
        StartCoroutine(_node.StartReaders(_readTasks));
    }

    public void AddWriteTask<T>(string streamName, Dictionary<string, T> data)
    {
        StartCoroutine(ExecuteTask(async () => await _node.WriteToStream(streamName, data)));
        IEnumerator ExecuteTask(Action action)
        {
            yield return action;
        }
    }


    public void AddReadTask(Action<UnityNode> task)
    {
        _readTasks.Add(Task.Run(() => task(_node)));
    }

    //override the read top method because we are going to want multiple instances of the top key for each accessor
}


public class UnityNode : CSNode
{
    private readonly List<Task> _allTasks = new List<Task>();

    public UnityNode(bool runOpen) : base()
    {
        if (runOpen)
        {
            //connect to unity
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
        else
        {
            base.ConnectToRedis();
        }
    }

    public bool IsRunning()
    {
        return _currentState.Equals(Status.NODE_READY);
    }

    private async Task RunReadTasksAsync(List<Task> cyclicalTasks)
    {
        await Task.WhenAll(cyclicalTasks);
    }

    public IEnumerator StartReaders(List<Task> cyclicalTasks)
    {
        while (Application.isPlaying && _currentState.Equals(Status.NODE_READY))
        {
            yield return RunReadTasksAsync(cyclicalTasks);
        }
    }

    //replace the connect to redis method with a dummy method to effectively override the constructor 
    protected override void ConnectToRedis() { }

}
