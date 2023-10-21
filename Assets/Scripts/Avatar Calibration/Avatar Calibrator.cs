using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AvatarCalibrator : MonoBehaviour
{
    public GameObject[] meshPoints;
    public List<AvatarCalibrationMesh> calibrations;
    public Material meshMaterial;

    [Header("To Generate Arms Cylinders")]
    [Tooltip("Target object to generate empty references for the arms and legs")]
    public List<Transform> targets;
    public Mesh capsuleMesh;
    [Range(0.0f,0.3f)]
    public float capsule_thickness;


    [ContextMenu("Generate Calibration Objects")]
    void GenerateCalibrationObjects()
    {
        foreach (GameObject parentObj in meshPoints)
        {
            CustomAvatarCalibrationMesh asset = ScriptableObject.CreateInstance<CustomAvatarCalibrationMesh>();
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

            calibrations.Add(asset);
        }

        List<AvatarCalibrationMesh> toDelete = new List<AvatarCalibrationMesh>();
        foreach (AvatarCalibrationMesh cal in calibrations)
        {
            if (!cal) { toDelete.Add(cal); }
        }
        foreach (AvatarCalibrationMesh cal in toDelete)
        {
            calibrations.Remove(cal);
        }
    }

    [ContextMenu("Generate the mesh")]
    private void GenerateMesh()
    {
        //If present delete previous
        List<Transform> toDelete = new List<Transform>();
        foreach (Transform child in transform)
        {
            toDelete.Add(child);
        }
        foreach (Transform child in toDelete)
        {
            DestroyImmediate(child.gameObject);
        }

        foreach (AvatarCalibrationMesh calibration in calibrations)
        {
            GameObject cal_mesh = new GameObject("g_CalMesh_" + calibration.name);
            cal_mesh.transform.parent = transform;
            MeshFilter mesh_filter = cal_mesh.AddComponent<MeshFilter>();
            MeshRenderer mesh_renderer = cal_mesh.AddComponent<MeshRenderer>();
            Mesh mesh = calibration.getMesh();
            mesh_filter.mesh = mesh;
            mesh_renderer.material = meshMaterial;
        } 
    }

    [ContextMenu("Generate Appendiges Meshes")]
    private void GenerateArmsMeshes()
    {
        string objName = "appendige_points";

        //If present delete previous
        foreach (Transform child in transform)
        {
            if (child.name == "g_" + objName)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        GameObject appendeges = new GameObject("g_" + objName);
        appendeges.transform.parent = transform;

        for (int i = 0; i < targets.Count; i+=2)
        {
            GameObject cal_capsule = new GameObject("g_CalCapsule_" + targets[i].name);
            cal_capsule.transform.parent = appendeges.transform;

            MeshFilter mesh_filter = cal_capsule.AddComponent<MeshFilter>();
            MeshRenderer mesh_renderer = cal_capsule.AddComponent<MeshRenderer>();
            ObjectBoneFollow follow = cal_capsule.AddComponent<ObjectBoneFollow>();
            mesh_renderer.material = meshMaterial;
            mesh_filter.mesh = capsuleMesh;

            Vector3 position = (targets[i].position + targets[i+1].position) / 2;
            float distance = (targets[i+1].position - targets[i].position).magnitude;
            Vector3 pointer = (targets[i].position - position).normalized;
            Quaternion rotation = Quaternion.LookRotation(pointer) * Quaternion.Euler(new Vector3(90, 0, 0));
            List<Transform> point_list = new List<Transform>() { targets[i], targets[i + 1] };
            Vector3 scale = new Vector3(capsule_thickness, distance / 2, capsule_thickness);

            follow.calibrate(point_list, position, Quaternion.Euler(new Vector3(90, 0, 0)), scale);
        }    
    }  
}
