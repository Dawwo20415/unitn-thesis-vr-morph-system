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
    public Transform target1;
    public Transform target2;
    public Mesh capsuleMesh;
    [Range(0.0f,0.3f)]
    public float capsule_thickness;


    [ContextMenu("Generate Calibration Objects")]
    void GenerateCalibrationObjects()
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
            Mesh mesh = new Mesh();
            mesh_filter.mesh = mesh;
            mesh.vertices = calibration.points.ToArray();
            mesh.triangles = calibration.triangles.getTrisArray();
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

        GameObject cal_capsule = new GameObject("g_CalCapsule_");
        cal_capsule.transform.parent = appendeges.transform;
        MeshFilter mesh_filter = cal_capsule.AddComponent<MeshFilter>();
        MeshRenderer mesh_renderer = cal_capsule.AddComponent<MeshRenderer>();
        mesh_renderer.material = meshMaterial;
        mesh_filter.mesh = capsuleMesh;
        Vector3 position = (target1.position + target2.position) / 2;
        float distance = (target2.position - target1.position).magnitude;
        Vector3 rotation = (target1.position - position).normalized;
        cal_capsule.transform.position = position;
        cal_capsule.transform.localScale = new Vector3(capsule_thickness, distance, capsule_thickness);
        cal_capsule.transform.rotation = Quaternion.LookRotation(target2.localPosition);
    }  
}
