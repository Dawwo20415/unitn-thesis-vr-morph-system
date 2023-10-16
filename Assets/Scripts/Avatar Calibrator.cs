using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AvatarCalibrator : MonoBehaviour
{
    public GameObject[] meshPoints;

    void Start()
    {
        foreach (GameObject parentObj in meshPoints)
        {
            AvatarCalibrationMesh asset = ScriptableObject.CreateInstance<AvatarCalibrationMesh>();
            CalibrationMeshDescriptor descriptor = parentObj.GetComponent<CalibrationMeshAsync>().descriptor;
            string assetPath = "Assets/Scriptable Objects/Calibration_" + parentObj.name + ".asset";

            List<Vector3> tmp = new List<Vector3>();
            foreach (Transform childTrn in parentObj.transform)
            {
                tmp.Add(childTrn.position);
            }
            asset.points = tmp;
            asset.triangles = descriptor;


            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
        }
    }
}
