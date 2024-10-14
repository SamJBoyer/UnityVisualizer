using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Collections.Concurrent;

/// <summary>
/// author: Samuel James Boyer
/// gmail: sam.james.boyer@gmail.com
/// 
/// UnityNodeManager manages the instance of unityNode and is a component so accessors can find this resource via the gameobject.find method
/// Class mainly keeps a list of reading and writing tasks and manages how they are passed to the unity node for asyncronisity 
/// There only needs to be one instance of this class in the scene because Unity works on 1 thread. 
/// </summary>
public class UnityNodeManager : MonoBehaviour
{
    //whether the node will connect from the editor or launch as a standalone app
    public bool RunNodeOpen = true;
    private UnityNode _node;
    public static readonly List<BRANDAccessor> BRANDAccessors = new List<BRANDAccessor>();
    //read tasks are run on seperate threads because they need to run constantly
    private readonly List<Task> _readTasks = new List<Task>();
    private readonly List<Func<UnityNode, Task>> _readTaskFactories = new List<Func<UnityNode, Task>>();

    private void Start()
    {
        if (this.gameObject.name != "Unity Node")
        {
            Debug.LogError("Unity Node Manager must be named 'Unity Node.' Accessors will not be able to find this resource");
        }
        //StartCoroutine(_node.StartReaders(_readTaskFactories));
        _node = new UnityNode(RunNodeOpen, BRANDAccessors);
        Task.WhenAll(_readTasks);

    }

    //method to call write class from the main thread which is then offloaded as an non-blocking enumerator
    public void AddWriteTask<T>(string streamName, Dictionary<string, T> data)
    {
        StartCoroutine(ExecuteTask(async () => await _node.WriteToStream(streamName, data)));
        IEnumerator ExecuteTask(Func<Task> taskFunc)
        {
            var task = taskFunc();
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
            }
        }
    }



    public void AddReadTask(Func<UnityNode, Task> task)
    {
        _readTasks.Add(MakeRepeatingTask(task));
        async Task MakeRepeatingTask(Func<UnityNode, Task> task)
        {
            while (Application.isPlaying)
            {
                try
                {
                    await Task.Run(async () =>
                        await task(_node)
                    );
                }
                catch (NullReferenceException)
                {
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }

            }
        }
    }
}
//override the read top method because we are going to want multiple instances of the top key for each accessor


/// <summary>
/// author: Samuel James Boyer
/// gmail: sam.james.boyer@gmail.com
/// 
/// this class is a special instance of the CS node for Unity. Because unity works one 1 thread, we must pay special attention 
/// to our async methods to not cause blocking. Also, because unity is run through the nonasync methods Start and Update, we need to store our 
/// redis data in a data buffer so it can be accessed in a sync context. Unity Node, UnityNodeManager, RedisBuffer, and BRANDAccessor are 4 classes 
/// that manage this async -> sync buffer system 
/// </summary>
public class UnityNode : CSNode
{

    //if false is passed the node will connect to redis as if it was launched as a standalone app by the BRAND supervisor
    public UnityNode(bool runOpen, List<BRANDAccessor> accessors) : base()
    {
        if (runOpen)
        {
            //connect to unity
            try
            {
                ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect("localHost");
                _database = _redis.GetDatabase();
                _currentState = Status.NODE_READY;
                Debug.Log("successful UnityNode connection to local redis");
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

    //run all the read tasks in parallel
    private async Task RunReadTasksAsync(List<Task> cyclicalTasks)
    {
        Debug.Log($"running read tasks with {cyclicalTasks.Count} tasks");
        await Task.WhenAll(cyclicalTasks);
        Debug.Log("all read tasks completed");
    }

    //run read tasks in a loop as an IEnumerator for unity asyc abilities
    public IEnumerator StartReaders(List<Func<UnityNode, Task>> taskFactories)
    {
        //Debug.Log($"starting read tasks with {cyclicalTasks.Count} tasks");
        while (Application.isPlaying)
        {
            List<Task> cyclicalTasks = taskFactories.Select(x => Task.Run(() => x(this))).ToList();
            yield return RunReadTasksAsync(cyclicalTasks);
        }
    }

    //replace the connect to redis method with a dummy method to effectively override the constructor 
    protected override void ConnectToRedis() { }

}