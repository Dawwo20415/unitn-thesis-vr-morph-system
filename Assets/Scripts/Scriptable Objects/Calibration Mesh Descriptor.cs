using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Calibration Mesh Descriptor", menuName = "ScriptableObjects/Avatar Calibration/Mesh Descriptor")]
public class CalibrationMeshDescriptor : ScriptableObject
{
    public int[] triangles;
}
