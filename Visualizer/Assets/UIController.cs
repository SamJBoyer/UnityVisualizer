using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class UIController : MonoBehaviour
{
    [SerializeField] private ArmController _armController;

    [SerializeField] private GameObject _menuObj;
    [SerializeField] private TextMeshProUGUI _menuText;
    [SerializeField] private TMP_Dropdown _poseDropdown;
    [SerializeField] private TMP_InputField _poseNameInput;

    public string FolderName;

    // Start is called before the first frame update
    void Start()
    {
        _menuObj.SetActive(false);
        PopulateDropdown();
    }

    public void ToggleMenu(){
        bool menuActive = !_menuObj.activeSelf;
        _menuObj.SetActive(menuActive);
        _menuText.text = menuActive == true ? "close menu" : "open menu";
    }


    public void SavePose(){
        string fileName = _poseNameInput.text;
        _armController.GetComponent<ArmController>().SavePose(fileName);
    }

    void PopulateDropdown()
    {
        // Ensure the dropdown options are clear
        _poseDropdown.ClearOptions();

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
    
}


