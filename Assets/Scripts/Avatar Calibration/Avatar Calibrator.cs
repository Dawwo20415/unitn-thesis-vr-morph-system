using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

public class AvatarCalibrator : MonoBehaviour
{
    public GameObject[] meshPoints;
    public List<AvatarCalibrationMesh> calibrations;
    public Material meshMaterial;
    public string EgocentricDescriptionName;
    public Transform HIPS;
    [SerializeField] private List<Vector3> mesh_points;
    [SerializeField] private Vector3 avatar_offset;

    [Header("To Generate Arms Cylinders")]
    [Tooltip("Target object to generate empty references for the arms and legs")]
    public List<Transform> targets;
    public Mesh capsuleMesh;
    [Range(0.0f,0.3f)]
    public float capsule_thickness;

    [Header("To Apply on Character Avatar")]
    public string parent_obj_name = "Colliders";
    public EgocentricMappingDescription egocentric_description;
    public GameObject avatar_root;

    [ContextMenu("GenPointList")]
    private void GenPointList()
    {
        mesh_points = new List<Vector3>();
        GameObject obj = meshPoints[0];
        foreach (Transform childTrn in obj.transform)
        {
            mesh_points.Add(childTrn.position);
        }

        Vector3 midpoint = Vector3.zero;
        foreach (Vector3 vec in mesh_points)
        { midpoint += vec; }
        midpoint = midpoint / mesh_points.Count;
        avatar_offset = midpoint - HIPS.position;

        for (int i = 0; i < mesh_points.Count; i++)
        {
            mesh_points[i] -= midpoint;
        }

        
    }

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

    List<Vector3> centerPivotToMidpoint(List<Vector3> list, Vector3 midpoint)
    {
        List<Vector3> tmp = list;

        for (int i = 0; i < list.Count; i++)
        {
            tmp[i] = list[i] - midpoint;
        }

        return tmp;
    }

    List<Vector3> getAsyncMeshPoints (Transform parent)
    {
        List<Vector3> tmp = new List<Vector3>();
        foreach (Transform childTrn in parent)
        {
            tmp.Add(childTrn.position);
        }

        return tmp;
    }

    Vector3 getMidpoint(List<Vector3> points)
    {
        Vector3 midpoint = Vector3.zero;

        foreach (Vector3 point in points)
        {
            midpoint += point;
        }

        midpoint /= points.Count;

        return midpoint;
    }

    [ContextMenu("Create Calibration Object")]
    void makeEgocentricMapDescription()
    {
        //So this is basically an outline of the calibration process, right?
        //Calibration for an humanoid so it doesn't need to be much flexible, just adapt to the avatar.   
        EgocentricMappingDescription calibration = ScriptableObject.CreateInstance<EgocentricMappingDescription>();
        string assetPath = "Assets/Scriptable Objects/EgocentricDescription_" + EgocentricDescriptionName + ".asset";
        AssetDatabase.CreateAsset(calibration, assetPath);
        AssetDatabase.SaveAssets();


        //Make Head-Torso Mesh
        foreach (GameObject parentObj in meshPoints)
        {
            CustomAvatarCalibrationMesh asset = ScriptableObject.CreateInstance<CustomAvatarCalibrationMesh>();
            CalibrationMeshDescriptor descriptor = parentObj.GetComponent<CalibrationMeshAsync>().descriptor;
            asset.name = "MeshDescription_" + parentObj.name;

            List<Vector3> vertices = getAsyncMeshPoints(parentObj.transform);
            Vector3 midpoint = getMidpoint(vertices);

            asset.mesh_name = parentObj.name;

            asset.points = centerPivotToMidpoint(vertices, midpoint);
            asset.triangles = descriptor;

            asset.position_offset = midpoint;
            asset.rotation_offset = Quaternion.Euler(0,0,0);
            //asset.avatar_reference_points = parentObj.GetComponent<CalibrationMeshAsync>().getBoneNames();
            
            AssetDatabase.SaveAssets();
            calibration.meshes.Add(asset);
            AssetDatabase.AddObjectToAsset(asset, calibration);
        }

        //Make Appendiges Capsules
        for (int i = 0; i < targets.Count; i += 2)
        {
            CapsuleAvatarCalibrationMesh asset = ScriptableObject.CreateInstance<CapsuleAvatarCalibrationMesh>();
            asset.name = "MeshDescription_" + targets[i].name;

            Vector3 position = (targets[i].position + targets[i + 1].position) / 2;
            float distance = (targets[i + 1].position - targets[i].position).magnitude;
            Quaternion rot = Quaternion.LookRotation((targets[i].position - position).normalized);
            Quaternion rotation = rot * Quaternion.Euler(new Vector3(90, 0, 0));

            List<string> name_list = new List<string>() { targets[i].name, targets[i+1].name };

            asset.capsule_mesh = capsuleMesh;
            asset.length = distance / 2;
            asset.radius = capsule_thickness;
            //asset.avatar_reference_points = name_list;
            asset.position_offset = position;
            asset.rotation_offset = rotation;

            AssetDatabase.SaveAssets();
            calibration.meshes.Add(asset);
            AssetDatabase.AddObjectToAsset(asset, calibration);
        }

        //Construct The calibration Object
        AssetDatabase.SaveAssets();
        egocentric_description = calibration;
    }

    [ContextMenu("Apply Egocentric Description to Avatar (Only in play mode)")]
    void ApplyOnAvatar ()
    {
        string prefabPath = "Assets/Prefabs/EgocentricAvatar_" + avatar_root.name + ".prefab";

        foreach (Transform trn in avatar_root.GetComponentsInChildren<Transform>())
        {
            if (trn.name == ("g_" + parent_obj_name))
            {
                DestroyImmediate(trn.gameObject);
                break;
            }
        }

        //Make a parent collection under the avatar to collect all colliders
        GameObject parent_obj = new GameObject("g_" + parent_obj_name);
        parent_obj.transform.parent = avatar_root.transform;

        foreach (AvatarCalibrationMesh calibration in egocentric_description.meshes)
        {
            GameObject obj = new GameObject("g_" + calibration.name);
            obj.transform.parent = parent_obj.transform;

            MeshFilter mesh_filter = obj.AddComponent<MeshFilter>();
            MeshRenderer mesh_renderer = obj.AddComponent<MeshRenderer>();
            ObjectBoneFollow follow = obj.AddComponent<ObjectBoneFollow>();

            mesh_filter.mesh = calibration.getMesh();
            mesh_renderer.material = meshMaterial;

            List<Transform> anchors = new List<Transform>();

            /*
            foreach (string name in calibration.avatar_reference_points)
            {
                foreach (Transform trn in avatar_root.GetComponentsInChildren<Transform>())
                {
                    if (trn.name == name)
                    {
                        anchors.Add(trn);
                    }
                }
            }
            */
            if (anchors.Count != 0)
                follow.calibrate(anchors, calibration.position_offset, calibration.rotation_offset, calibration.getScale());
            else
                Debug.LogError("Have been unable to find string names in parent, for object: " + obj.name, obj);
        }

        PrefabUtility.SaveAsPrefabAsset(avatar_root, prefabPath);
    }
}
