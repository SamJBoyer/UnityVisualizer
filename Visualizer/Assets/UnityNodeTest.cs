using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.PlayerLoop;

public class UnityNodeTest : MonoBehaviour
{
    public string streamName;
    private BRANDAccessor _mjAccessor;

    void Start()
    {
        _mjAccessor = new BRANDAccessor(streamName, 1);
        var adata = _mjAccessor.DequeueData();
        //print(adata.Values);

    }

    void Update()
    {
        try
        {
            print(_mjAccessor.DequeueData().Values[0]);
        }
        catch
        {
            ;
        }

    }


}
