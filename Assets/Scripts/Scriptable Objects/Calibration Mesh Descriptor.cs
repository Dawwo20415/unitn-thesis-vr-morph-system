using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Calibration Mesh Descriptor", menuName = "ScriptableObjects/Avatar Calibration/Mesh Descriptor")]
public class CalibrationMeshDescriptor : ScriptableObject
{
    public List<Vector3> triangles;

    public int[] getTrisArray()
    {
        int[] array = new int[triangles.Count * 3];

        int i = 0;
        foreach (Vector3 vec in triangles)
        {
            array[i] = (int)vec.x;
            array[i + 1] = (int)vec.y;
            array[i + 2] = (int)vec.z;

            i += 3;
        }

        return array;
    }
}
