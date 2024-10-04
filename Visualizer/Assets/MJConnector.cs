using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

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


}
