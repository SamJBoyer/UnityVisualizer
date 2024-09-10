using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using static DOFManager;
using static ArmatureStructure;

public class PoseBuilderUI : MonoBehaviour
{
    [SerializeField] private ArmController _armController;
    [SerializeField] private TextMeshProUGUI _DOFChooseButtonText;
    [SerializeField] private TextMeshProUGUI _cameraStatusText;
    [SerializeField] private TextMeshProUGUI _poseSaveNotification;
    [SerializeField] private TMP_Dropdown _poseDropdown;
    [SerializeField] private TMP_InputField _poseNameInput;
    [SerializeField] private GameObject _DOFBoxPrefab;
    [SerializeField] private Transform _DOFContent;
    [SerializeField] private Transform _optionsTransform;
    [SerializeField] private Transform _menuTransform;

    //THINGS TO ADD
    /// <summary>
    /// create a little help button for camera controls 
    /// create a menu for changing the sensitivity ~ maybe 
    /// make the game view text dynamic 
    /// enforce joint limits
    /// fixing the stupid BEE issue s
    /// make it so the poses are loaded everytime a new one is created, not just once at the start
    /// </summary>

    public string FolderName;

    //these variables are used to determine if the user is in a mode where the camera is locked 
    public static bool InGameView; //if in game view
    public static bool DOFChoosingMode; //if in DOF selection mode
    public static bool FocusedOnUI; //if any of the UI are being interacted with in a control-blocking way


    private Dictionary<DOF, DOFUI> DOFBoxes;
    private bool _menuOpen;

    // Start is called before the first frame update
    private void Start()
    {
        PopulatePoseDropdown();
        DOFBoxes = new Dictionary<DOF, DOFUI>();
        DOFChoosingMode = false;
        InGameView = false;
        _DOFChooseButtonText.text = "enter DOF choose mode";
        _menuOpen = false;
        _menuTransform.gameObject.SetActive(false);
        _optionsTransform.gameObject.SetActive(true);
        ToggleDOFSelectionMode(); //set the dof selection mode as default 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_menuOpen)
            {
                _menuTransform.gameObject.SetActive(false);
                _optionsTransform.gameObject.SetActive(true);
                _menuOpen = false;
            }
            else
            {
                _menuTransform.gameObject.SetActive(true);
                _optionsTransform.gameObject.SetActive(false);
                _menuOpen = true;
            }
        }

        FocusedOnUI = DOFUI.DOFBeingDragged || UIHover.WithinDOFZone || _poseNameInput.isFocused;

        var adjustments = new Dictionary<DOF, float>();
        foreach (var kvp in DOFBoxes)
        {
            DOFUI dofUI = kvp.Value;
            float? value = dofUI.GetValue();
            if (value != null)
            {
                adjustments.Add(kvp.Key, (float)value);
            }
        }
        _armController.AdjustAngles(adjustments);
        //_armController.UpdateArm();
    }

    //called from the toggle selection mode button
    public void ToggleDOFSelectionMode()
    {
        if (DOFChoosingMode)
        {
            PopulateDOFs();
            _DOFChooseButtonText.text = "Select DOFs";
            _cameraStatusText.text = "free camera mode";
            var unselectedDOFs = AllTargets.Except(SelectedTargets);
            foreach (ReactiveTarget target in unselectedDOFs)
            {
                target.gameObject.SetActive(false);
            }
            DOFChoosingMode = false;
        }
        else
        {
            ClearDOFs();
            string path = Path.Combine(Application.streamingAssetsPath, "Poses", "Default.json");
            _armController.SetArmature(ArmatureStructure.LoadArmatureFromFile(path));
            Camera.main.GetComponent<CameraController>().SetCameraToDOF();
            foreach (ReactiveTarget target in AllTargets)
            {
                target.gameObject.SetActive(true);
                target.Reset();
                SelectedTargets.Clear();
            }
            _DOFChooseButtonText.text = "Build Pose";
            _cameraStatusText.text = "fixed camera mode";
            DOFChoosingMode = true;
        }
    }

    private void PopulateDOFs()
    {
        foreach (ReactiveTarget target in DOFManager.SelectedTargets)
        {
            GameObject newDofBox = Instantiate(_DOFBoxPrefab, _DOFContent);
            DOFUI dofUI = newDofBox.GetComponent<DOFUI>();
            Color randomColor = new Color(Random.value, Random.value, Random.value);
            dofUI.InitializeDOFBox(target.DOF.ToString(), randomColor);
            target.SetColor(randomColor);
            DOFBoxes.Add(target.DOF, dofUI);
        }
    }

    private void ClearDOFs()
    {
        foreach (var dofUI in DOFBoxes.Values)
        {
            Destroy(dofUI.gameObject);
        }
        DOFBoxes.Clear();
    }


    //exit pose builder and return to main menu 
    public void ReturnToMenu()
    {

    }

    //called from the save pose button UI object 
    public void SavePose()
    {
        string fileName = _poseNameInput.text;
        _armController.GetComponent<ArmController>().GetCurrentArmature().SavePose(fileName);
        StartCoroutine(SaveNotification());

        IEnumerator SaveNotification()
        {
            _poseSaveNotification.text = $"Pose Saved as {fileName}";
            yield return new WaitForSeconds(2);
            _poseSaveNotification.text = "";
        }
    }

    void PopulatePoseDropdown()
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
            if (fileExtension.Equals(".json"))
            {
                fileNames.Add(fileName);
            }
            // Add the file name to the list
        }

        // Add the file names as options to the dropdown
        _poseDropdown.AddOptions(fileNames);
    }

    //called from the pose dropdown UI object 
    public void SelectPose()
    {
        string fileName = _poseDropdown.options[_poseDropdown.value].text;
        string filePath = Path.Combine(Application.streamingAssetsPath, "Poses", fileName);
        _armController.SetArmature(ArmatureStructure.LoadArmatureFromFile(filePath));
    }

    public void ToggleGameView()
    {
        if (InGameView)
        {
            Camera.main.GetComponent<CameraController>().ReturnCameraToPrevious();
            InGameView = false;
            _cameraStatusText.text = "free camera mode";
        }
        else
        {
            Camera.main.GetComponent<CameraController>().SetCameraToGameView();
            _cameraStatusText.text = "fixed camera mode";
            InGameView = true;
        }
    }
}


