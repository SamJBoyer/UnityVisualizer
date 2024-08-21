using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactiveTarget : MonoBehaviour
{
    private bool isSelected;
    private MeshRenderer meshRenderer;
    [SerializeField] private DOF dof; 

    public void SetColor(Color color){
        meshRenderer.material.color = color;
    }

    private void Start(){
        isSelected = false; 
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = Color.red; 
        DOFManager.AllTargets.Add(this);
    }

    public void Select(){
        isSelected = true;
        meshRenderer.material.color = Color.green;
        DOFManager.SelectedTargets.Add(this);
    }

    public void ToggleSelection(){
        if (isSelected){
            isSelected = false;
            meshRenderer.material.color = Color.red;
            DOFManager.SelectedTargets.Remove(this);
        } else {
            Select();
        }
    }

    public DOF DOF => dof;
}
