using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
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
        _hardpoint = new Hardpoint(null);
        _writeTasks = new List<Task>();
    }

    // Update is called once per frame
    async void Update()
    {
        float shoulderX = ShoulderXSlider.value;
        float shoulderY = ShoulderYSlider.value;
        float shoulderZ = ShoulderZSlider.value;
        float elbowDeg = ElbowSlider.value;
        float graspDeg = GraspSlider.value;

        _writeTasks.Add(_hardpoint.WriteToStream("shoulderFlexion", "value", shoulderX.ToString()));
        _writeTasks.Add(_hardpoint.WriteToStream("shoulderAbduction", "value", shoulderY.ToString()));
        _writeTasks.Add(_hardpoint.WriteToStream("shoulderRotation", "value", shoulderZ.ToString()));
        _writeTasks.Add(_hardpoint.WriteToStream("elbowFlexion", "value", elbowDeg.ToString()));
        _writeTasks.Add(_hardpoint.WriteToStream("wristFlexion", "value", graspDeg.ToString()));
        await Task.WhenAll(_writeTasks);

        ShoulderXText.text = shoulderX.ToString();
        ShoulderYText.text = shoulderY.ToString();
        ShoulderZText.text = shoulderZ.ToString();
        ElbowText.text = elbowDeg.ToString();
        GraspText.text = graspDeg.ToString();
    }
}
