using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class PoseBuilderUI : MonoBehaviour
{
    [SerializeField] private ArmController _armController;
    [SerializeField] private GameObject _menuObj;
    [SerializeField] private TextMeshProUGUI _menuButtonText;
    [SerializeField] private TextMeshProUGUI _DOFChooseButtonText;
    [SerializeField] private TextMeshProUGUI _toggleCameraText;
    [SerializeField] private TMP_Dropdown _poseDropdown;
    [SerializeField] private TMP_InputField _poseNameInput;
    [SerializeField] private Transform _DOFContent;
    [SerializeField] private GameObject _DOFBoxPrefab;
    //[SerializeField] private Scroll _scroll;


    public static bool InGameView;
    public string FolderName;
    public static bool DOFChoosingMode; 
    public static bool FocusElsewhere; //if a text box has been clicked on 
    private Dictionary<DOF, DOFUI> DOFBoxes;

    // Start is called before the first frame update
    private void Start()
    {
        ToggleMenu(); //set menu to open
        PopulateDropdown();
        DOFBoxes = new Dictionary<DOF, DOFUI>();
        DOFChoosingMode = false;
        InGameView = false; 
        _DOFChooseButtonText.text = "enter DOF choose mode";
    }

    private void Update(){
        FocusElsewhere = _poseNameInput.isFocused || UIHover.IsMouseHover;
        var adjustments = new Dictionary<DOF, float>();
        foreach (var kvp in DOFBoxes) {
            DOFUI dofUI = kvp.Value;
            float? value = dofUI.GetValue();
            if (value != null){
                adjustments.Add(kvp.Key, (float)value);
            }
        }
        _armController.AdjustAngles(adjustments);
        _armController.UpdateArm();
    }

    public void ToggleDOFChooseMode(){
        if (DOFChoosingMode){
            PopulateDOFs();
            _DOFChooseButtonText.text = "enter DOF selection mode";
            DOFChoosingMode = false;
            var unselectedDOFs = DOFManager.AllTargets.Except(DOFManager.SelectedTargets);
            foreach (ReactiveTarget target in unselectedDOFs) {
                target.gameObject.SetActive(false);
            }

        } else {
            ClearDOFs();
            _armController.LoadPose("Default.json");
            Camera.main.GetComponent<CameraController>().SetCameraToDOF();
            _DOFChooseButtonText.text = "exit DOF selection mode";
            DOFChoosingMode = true;
        }
    }

    private void PopulateDOFs(){
        foreach (ReactiveTarget target in DOFManager.SelectedTargets) {
            GameObject newDofBox = Instantiate(_DOFBoxPrefab, _DOFContent);
            DOFUI dofUI = newDofBox.GetComponent<DOFUI>();
            Color randomColor = new Color(Random.value, Random.value, Random.value);
            dofUI.InitializeDOFBox(target.DOF.ToString(), randomColor);
            target.SetColor(randomColor);
            DOFBoxes.Add(target.DOF, dofUI);
        }
    }

    private void ClearDOFs(){
        foreach (var dofUI in DOFBoxes.Values) {
            Destroy(dofUI.gameObject);
        }
        DOFBoxes.Clear();
    }

    public void ToggleMenu(){
        bool menuActive = !_menuObj.activeSelf;
        _menuObj.SetActive(menuActive);
        _menuButtonText.text = menuActive == true ? "close menu" : "open menu";
    }


    public void SavePose(){
        string fileName = _poseNameInput.text;
        _armController.GetComponent<ArmController>().SavePose(fileName);
    }

    void PopulateDropdown()
    {
        // Ensure the dropdown options are clear
        _poseDropdown.ClearOptions();
        FolderName = "Poses";
        // List to hold the file names
        List<string> fileNames = new List<string>();
        string folderPath = Path.Combine(Application.streamingAssetsPath, FolderName);
        // Get all files in the specified folder
        string[] files = Directory.GetFiles(folderPath);

        foreach (string filePath in files)
        {
            // Get the file name without the path
            string fileName = Path.GetFileName(filePath);
            string fileExtension = Path.GetExtension(filePath);
            if (fileExtension.Equals(".json")) {
                fileNames.Add(fileName);
            }
            // Add the file name to the list
        }

        // Add the file names as options to the dropdown
        _poseDropdown.AddOptions(fileNames);
    }

    public void SelectPose(){
        string fileName = _poseDropdown.options[_poseDropdown.value].text;
        _armController.LoadPose(fileName);
    }
    
    public void ToggleGameView(){
        if (InGameView){
            Camera.main.GetComponent<CameraController>().ReturnCameraToPrevious();
            InGameView = false; 
            _toggleCameraText.text = "fixed camera mode";
        } else {
            Camera.main.GetComponent<CameraController>().SetCameraToGameView();
            _toggleCameraText.text = "free camera mode";
            InGameView = true;
        }
    }
}


