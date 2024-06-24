using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;

public class ControlPanel : MonoBehaviour
{

#region 
    public Slider ShoulderXSlider; 
    public Slider ShoulderYSlider; 
    public Slider ShoulderZSlider; 
    public Slider ElbowSlider; 
    public Slider GraspSlider; 

    public TextMeshProUGUI ShoulderXText;
    public TextMeshProUGUI ShoulderYText;
    public TextMeshProUGUI ShoulderZText;
    public TextMeshProUGUI ElbowText;
    public TextMeshProUGUI GraspText;
#endregion


    private List<Task> _writeTasks;

    private Hardpoint _hardpoint; 

    void Start()
    {
        _hardpoint = new Hardpoint(new RedisChannel[]{});
        _writeTasks = new List<Task>();
        Debug.Log("started hard point");
    }

    // Update is called once per frame
    async void Update()
    {
        Debug.Log("in cycle");
        float shoulderX = ShoulderXSlider.value;
        float shoulderY = ShoulderYSlider.value;
        float shoulderZ = ShoulderZSlider.value;

        float elbowDeg = ElbowSlider.value;

        float graspDeg = GraspSlider.value;

        _writeTasks.Add(_hardpoint.WriteToStream("ShoulderX", "value", shoulderX.ToString()));
        _writeTasks.Add(_hardpoint.WriteToStream("ShoulderY", "value", shoulderY.ToString()));
        _writeTasks.Add(_hardpoint.WriteToStream("ShoulderZ", "value", shoulderZ.ToString()));

        _writeTasks.Add(_hardpoint.WriteToStream("ElbowDeg", "value", elbowDeg.ToString()));

        _writeTasks.Add(_hardpoint.WriteToStream("GraspDeg", "value", graspDeg.ToString()));

        await Task.WhenAll(_writeTasks);

        ShoulderXText.text = shoulderX.ToString();
        ShoulderYText.text = shoulderY.ToString();
        ShoulderZText.text = shoulderZ.ToString();

        ElbowText.text = elbowDeg.ToString();

        GraspText.text = graspDeg.ToString();
    }
}
