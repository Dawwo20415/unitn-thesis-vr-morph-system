using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationMeshAsync : MonoBehaviour
{
    public CalibrationMeshDescriptor descriptor;
    public List<GameObject> associated_bones;

    public List<string> getBoneNames()
    {
        List<string> tmp = new List<string>();

        foreach (GameObject obj in associated_bones)
        {
            tmp.Add(obj.name);
        }

        return tmp;
    }
}
