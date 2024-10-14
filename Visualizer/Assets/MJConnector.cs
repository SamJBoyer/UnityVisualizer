using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class MJConnector : MonoBehaviour
{
    [SerializeField] private Transform _mocapStub;
    private Hardpoint _hardpoint;
    private List<Task> _writeTasks;

    private BRANDAccessor _mjAccessor;

    void Start()
    {
        _mjAccessor = new BRANDAccessor("test", 1);
        var adata = _mjAccessor.DequeueData();
        print(adata);

    }

    void Update()
    {
        Vector3 position = _mocapStub.position;
        Quaternion rotation = _mocapStub.rotation;
        float[] positionArray = new float[] { position.x, position.y, position.z };
        float[] rotationArray = new float[] { rotation.x, rotation.y, rotation.z, rotation.w };
        Dictionary<string, string> data = new Dictionary<string, string>() { 
            { "pos", JsonConvert.SerializeObject(positionArray) }, 
            { "rot", JsonConvert.SerializeObject(rotationArray) }
        };
        //print(posAsString);
        BRANDAccessor.WriteToRedis("mocap", data);

    }


}
