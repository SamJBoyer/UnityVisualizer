using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;


public class ArmatureStructure
{
    //keep as a upper because it makes it a lot easier to compare directly because you can use caps(thing) 
    public enum DOF
    {
        SHOULDERABDUCTION,
        SHOULDERFLEXION,
        SHOULDERROTATION,
        ELBOWFLEXION,
        WRISTABDUCTION,
        WRISTFLEXION,
        WRISTSUPINATION,
        INDEX1, INDEX2, INDEX3,
        MIDDLE1, MIDDLE2, MIDDLE3,
        RING1, RING2, RING3,
        PINKY1, PINKY2, PINKY3,
        THUMB1, THUMB2, THUMB3
    }

    private Dictionary<DOF, float> _dofPositions;

    public ArmatureStructure(Dictionary<DOF, float> dofPositions)
    {
        _dofPositions = dofPositions;
    }

    public ArmatureStructure(string fileName)
    {
        LoadArmatureFromFile(fileName);
    }

    public ArmatureStructure(List<string> dofs)
    {
        _dofPositions = new Dictionary<DOF, float>();
        foreach (var dof in dofs)
        {
            _dofPositions.Add(StringToDof(dof), 0);
        }
    }

    public ArmatureStructure(Dictionary<string, float> dofPositions)
    {
        _dofPositions = new Dictionary<DOF, float>();
        foreach (var kvp in dofPositions)
        {
            _dofPositions.Add(StringToDof(kvp.Key), kvp.Value);
        }
    }

    private DOF StringToDof(string dofString)
    {
        DOF dof;
        Enum.TryParse(dofString.ToUpper(), out dof);
        Debug.Log(dof);
        return dof;
    }


    //loads a pose from a JSON file of a DOF dict 
    public static ArmatureStructure LoadArmatureFromFile(string filePath)
    {
        try
        {
            string jsonString = File.ReadAllText(filePath);
            var dofPositions = JsonConvert.DeserializeObject<Dictionary<DOF, float>>(jsonString);
            Debug.Log($"loading armature structure from filename: {filePath}");
            return new ArmatureStructure(dofPositions);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading pose: {ex.Message}");
            return null;
        }
    }


    //creates a new json file with the DOF dict saved 
    public void SavePose(string fileName)
    {
        try
        {
            // Serialize the dictionary to a JSON string
            string jsonString = JsonConvert.SerializeObject(_dofPositions);
            string filePath = Path.Combine(Application.streamingAssetsPath, "Poses", $"{fileName}.json");
            // Write the JSON string to a file
            File.WriteAllText(filePath, jsonString);
            Debug.Log($"armature position saved to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"could not save armature position. error: {ex.Message}");
        }
    }

    public static ArmatureStructure Lerp(ArmatureStructure a, ArmatureStructure b, float t)
    {
        return a + (b - a) * t;
    }

    public Dictionary<DOF, float> GetValues()
    {
        return _dofPositions;
    }

    // Overload the + operator
    public static ArmatureStructure operator +(ArmatureStructure a, ArmatureStructure b)
    {
        // Create a new dictionary to store the result of the addition
        var result = new Dictionary<DOF, float>(a._dofPositions);
        var aData = a._dofPositions;
        var bData = b._dofPositions;

        foreach (var dof in aData.Keys)
        {
            if (bData.ContainsKey(dof))
            {
                result[dof] += aData[dof] + bData[dof];
            }
        }
        // Return a new ArmaturePosition instance with the combined positions
        return new ArmatureStructure(result);
    }

    // Override the - operator
    public static ArmatureStructure operator -(ArmatureStructure a, ArmatureStructure b)
    {
        // Create a new dictionary to store the result of the addition
        var result = new Dictionary<DOF, float>();
        var aData = a._dofPositions;
        var bData = b._dofPositions;

        foreach (var key in aData.Keys)
        {
            if (bData.ContainsKey(key))
            {
                result[key] = aData[key] - bData[key];
            }
        }
        // Return a new ArmaturePosition instance with the combined positions
        return new ArmatureStructure(result);
    }

    // Override the * operator to scale the positions by a sclar multiplier
    public static ArmatureStructure operator *(ArmatureStructure a, float multiplier)
    {
        // Create a new dictionary to store the result of the multiplication
        var result = new Dictionary<DOF, float>(a._dofPositions);

        foreach (var kvp in result)
        {
            result[kvp.Key] *= multiplier;
        }
        // Return a new ArmaturePosition instance with the combined positions
        return new ArmatureStructure(result);
    }

    // Override the * operator to scale the positions by a armaturestruct multiplier
    public static ArmatureStructure operator *(ArmatureStructure a, ArmatureStructure b)
    {
        // Create a new dictionary to store the result of the multiplication
        var result = new Dictionary<DOF, float>();
        var aData = a._dofPositions;
        var bData = b._dofPositions;

        foreach (var key in aData.Keys)
        {
            if (bData.ContainsKey(key))
            {
                result[key] = aData[key] * bData[key];
            }
        }
        // Return a new ArmaturePosition instance with the combined positions
        return new ArmatureStructure(result);
    }

    public static ArmatureStructure operator /(ArmatureStructure a, float divisor)
    {
        // Create a new dictionary to store the result of the division
        var result = new Dictionary<DOF, float>(a._dofPositions);
        if (divisor != 0)
        {
            foreach (var key in result.Keys)
            {
                result[key] /= divisor;
            }
        }
        else
        {
            Debug.LogError("Division by zero");
        }

        // Return a new ArmaturePosition instance with the combined positions
        return new ArmatureStructure(result);
    }

    // Override the * operator to scale the positions by a armaturestruct multiplier
    public static ArmatureStructure operator /(ArmatureStructure a, ArmatureStructure b)
    {
        // Create a new dictionary to store the result of the multiplication
        var result = new Dictionary<DOF, float>();
        var aData = a._dofPositions;
        var bData = b._dofPositions;

        foreach (var key in aData.Keys)
        {
            if (bData.ContainsKey(key))
            {
                var value = bData[key];
                if (value != 0)
                {
                    result[key] /= bData[key];
                }
                else
                {
                    Debug.LogError("Division by zero");
                }
            }
        }
        // Return a new ArmaturePosition instance with the combined positions
        return new ArmatureStructure(result);
    }
}