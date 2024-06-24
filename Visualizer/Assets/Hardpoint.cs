using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Channels;

/// <summary>
/// Author: Sam Boyer
/// Email: Sam.James.Boyer@gmail.com
/// 
/// </summary>

public class Hardpoint : CSNode
{

    public static Dictionary<string, Dictionary<string, string>> InstChannelData;
    public static Dictionary<string, Queue<Dictionary<string, string>>> BufferedChannelData; // i have yet to find a situation where this is needed 
    private readonly List<Task> _tasks;

    public Hardpoint(RedisChannel[] channels, string overrideString = null) : base(overrideString)
    {
        InstChannelData = new Dictionary<string, Dictionary<string, string>>();
        BufferedChannelData = new Dictionary<string, Queue<Dictionary<string, string>>>();
        _tasks = new List<Task>();
        foreach (RedisChannel channel in channels)
        {
            if (channel.IsBuffer)
            {
                var channelQueue = new Queue<Dictionary<string, string>>();
                BufferedChannelData.Add(channel.Name, channelQueue);
                _tasks.Add(ReadStreamToBuffer(channel.Name));
            }
            else
            {
                var channelDict = new Dictionary<string, string>();
                InstChannelData.Add(channel.Name, channelDict);
                _tasks.Add(ReadStreamToValue(channel.Name));
            }
        }
        Debug.Log($"Hardpoint for {base._nickname} connected with connection string {base._connectionString}. Connection returned with {base.State}");
        base.Run();
    }



    protected override async void Work()
    {
        try {
            await Task.WhenAll(_tasks);
        } catch (Exception ex){
            Debug.LogWarning(ex);
        }
    }

    public async Task WriteToStream(string key, string entryName, string entry){
        NameValueEntry newEntry = new NameValueEntry(entryName, entry);
        await _database.StreamAddAsync(key, new NameValueEntry[]{newEntry});
    }

    private async Task ReadStreamToBuffer(string key)
    {
        while (Application.isPlaying){ //should also check if redis is connected to anything 
            await Task.Run(async () =>
            {
                var result = await _database.StreamRangeAsync(key, "-", "+", 1,
                Order.Descending);

                if (result.Any()) //this will always be 1, unless 
                {
                    foreach (var entry in result)
                    {
                        BufferedChannelData[key].Enqueue(ParseResult(entry));
                    }
                }
            });
        }
    }

    private async Task ReadStreamToValue(string key)
    {
        while (Application.isPlaying){ //might not have access to Application in the future. should parse command line args 
            await Task.Run(async () =>
            {
                var result = await _database.StreamRangeAsync(key, "-", "+", 1,
                Order.Descending);

                if (result.Any()) //this will always be 1, unless 
                {
                    foreach (var entry in result)
                    {
                        InstChannelData[key] = ParseResult(entry);
                    }
                }
            });
        }
    }

    Dictionary<string, string> ParseResult(StreamEntry entry) =>
entry.Values.ToDictionary(x => x.Name.ToString(), x =>
x.Value.ToString());

    #region //wrappers for debugging features

    public string GetNodeState(){
        return base.State.ToString();
    }

    public string GetConnectionString(){
        return base.ConnectionString.ToString();
    }

    public string GetDatabaseString(){
        return base.Database.ToString();
    }

    public string[] GetNodeArguments(){
        return base.Args;
    }


    #endregion

}
