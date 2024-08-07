using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode]
public class BSABuilderComponent : MonoBehaviour
{
    [Header("Base Functionality")]
    public Animator animator;
    public string asset_name;
    public string asset_path;
    public int test;

    [Space] [Header("Normals")]
    public string normal_name;
    public HumanBodyBones normal_anchor;
    [SerializeField] private List<Transform> normals;

    [Space] [Header("Custom Meshes")]
    public string mesh_name;
    public Mesh mesh;
    public List<HumanBodyBones> mesh_anchors;

    [SerializeField] private List<Transform> meshes;

    [Space] [Header("Appendiges Cylinders")]
    public Mesh cylinder_mesh;
    public BSACylinder placeholder_cylinder;
    [SerializeField] private List<Transform> cylinders;

    private void Start()
    {
        if (meshes.Count < 1)
            meshes = new List<Transform>();

        if (normals.Count < 1)
            normals = new List<Transform>();

        if (cylinders.Count < 1)
            cylinders = new List<Transform>();
    }

    [ContextMenu("Instance/Cylinder")]
    public void CreateArmCylinderDescriptor()
    {
        if (!cylinder_mesh) { Debug.LogError("No Cylinder mesh is selected to create Object, prese select it from Unity's Primitives"); return; }

        GameObject parent = findAggregatorObject("Cylinders");

        GameObject obj = new GameObject("Cylinder_" + placeholder_cylinder.name);
        obj.transform.position = transform.position;
        obj.transform.rotation *= Quaternion.Euler(90, 0, 0);
        obj.transform.parent = parent.transform;

        obj.AddComponent<BSACylinderBuilder>();
        BSACylinderBuilder component = obj.GetComponent<BSACylinderBuilder>();
        component.beginning = animator.GetBoneTransform(placeholder_cylinder.start);
        component.end = animator.GetBoneTransform(placeholder_cylinder.end);
        component.cylinder = placeholder_cylinder;
        component.cyMesh = cylinder_mesh;

        cylinders.Add(obj.transform);
        Selection.activeGameObject = obj;
    }

    [ContextMenu("Instance/Surface Normal")]
    public void CreatePlaneNormalDefinitionObject()
    {
        GameObject parent = findAggregatorObject("Normals");

        GameObject obj = new GameObject(normal_name);
        obj.transform.position = transform.position;
        obj.transform.rotation *= Quaternion.Euler(90, 0, 0);
        obj.transform.parent = parent.transform;

        obj.AddComponent<BSANormalBuilder>();
        BSANormalBuilder component = obj.GetComponent<BSANormalBuilder>();
        component.scale = 0.1f;
        component.anchor = animator.GetBoneTransform(normal_anchor);

        normals.Add(obj.transform);
        Selection.activeGameObject = obj;
    }

    [ContextMenu("Instance/Mesh Descriptor")]
    public void CreateMeshControllerObject()
    {
        if (!mesh) { Debug.LogError("No Mesh is selected to create Object"); return; }

        GameObject parent = findAggregatorObject("Meshes");

        Mesh actual = Object.Instantiate(mesh);
        MergeVertices(actual);
        
        GameObject obj = new GameObject(mesh_name);
        obj.transform.position = transform.position;
        obj.transform.parent = parent.transform;

        obj.AddComponent<BSAMeshBuilder>();
        BSAMeshBuilder builder = obj.GetComponent<BSAMeshBuilder>();
        builder.mesh_name = mesh_name;
        builder.mesh = actual;
        builder.vertices = new List<Transform>(actual.vertexCount);
        builder.SetAnchors(animator, mesh_anchors);

        int i = 0;
        foreach (Vector3 vec in actual.vertices)
        {
            GameObject vert = new GameObject("Vertex #" + i);
            vert.transform.position = obj.transform.position + vec;
            vert.transform.parent = obj.transform;

            builder.vertices.Add(vert.transform);
            i++;
        }

        meshes.Add(obj.transform);
        Selection.activeGameObject = obj;
    }

    [ContextMenu("Create BSA ScriptableObject")]
    public void CompileBSAScriptableObject()
    {
        bool create_asset_from_scratch = false;
        string path = asset_path + "/" + asset_name + ".asset";
        BodySurfaceApproximationDefinition BSAD = AssetDatabase.LoadAssetAtPath(path, typeof(BodySurfaceApproximationDefinition)) as BodySurfaceApproximationDefinition;

        if (BSAD == null)
        {
            create_asset_from_scratch = true;
            BSAD = (BodySurfaceApproximationDefinition)ScriptableObject.CreateInstance(typeof(BodySurfaceApproximationDefinition));
        }

        //ASSIGNING DATA TO THE SCRIPTABLE OBJECT
        BSAD.name = asset_name;

        //Upload Cylinders
        BSAD.cylinders = new List<BSACylinder>(cylinders.Count);
        foreach (Transform trn in cylinders)
        {
            BSACylinderBuilder component = trn.gameObject.GetComponent<BSACylinderBuilder>();
            if (component == null)
                Debug.Log("Could not find component");
            BSAD.cylinders.Add(component.cylinder);
        }

        //Upload Meshes
        BSAD.meshes = new List<BSACustomMesh>(meshes.Count);
        foreach (Transform trn in meshes)
        {
            BSAMeshBuilder component = trn.gameObject.GetComponent<BSAMeshBuilder>();
            if (component == null)
                Debug.Log("Could not find component");
            BSAD.meshes.Add(component.bsa_mesh);
        }

        //Upload Normals
        BSAD.normals = new List<BSANormal>(normals.Count);
        foreach (Transform trn in normals)
        {
            BSANormalBuilder component = trn.gameObject.GetComponent<BSANormalBuilder>();
            if (component == null)
                Debug.Log("Could not find component");

            BSAD.normals.Add(component.bsa_normal);
        }

        //Upload Proportional Weights
        BSAD.body_proportion_weights = new List<float>((int)HumanBodyBones.LastBone);
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            BSAD.body_proportion_weights.Add(CalculatePath(animator, (HumanBodyBones)i));
        }

        //---------------------------------------

        if (create_asset_from_scratch)
        {
            AssetDatabase.CreateAsset(BSAD, path);
            return;
        }

        EditorUtility.SetDirty(BSAD);
        AssetDatabase.SaveAssets();
    }

    [ContextMenu("TPose Avatar")]
    public void TPose()
    {
        Avatar avatar = animator.avatar;
        HumanDescription hd = avatar.humanDescription;
        Dictionary<int,int> dic = MecanimHumanoidExtension.HumanBodyBones2AvatarSkeleton(animator);

        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            Transform bone = animator.GetBoneTransform((HumanBodyBones)i);
            int index = dic[i];
            Quaternion rot = hd.skeleton[index].rotation;
            bone.localRotation = rot;
        }
    }

    private void MergeVertices(Mesh mesh)
    {
        List<(int, int)> indexes = new List<(int, int)>();
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            Vector3 vertex = mesh.vertices[i];

            for (int j = i + 1; j < mesh.vertexCount; j++)
            {
                if (vertex == mesh.vertices[j])
                {
                    indexes.Add((j, i));
                }
            }
        }

        indexes.Sort();
        indexes = indexes.Distinct().ToList();

        List<Vector3> vertices = new List<Vector3>(mesh.vertices);
        List<int> triangles = new List<int>(mesh.triangles);
        List<Vector3> normals = new List<Vector3>(mesh.normals);

        for (int i = indexes.Count - 1; i >= 0; i--)
        {
            vertices.RemoveAt(indexes[i].Item1 - i);

            for (int j = 0; j < triangles.Count; j++)
            {
                if (triangles[j] == indexes[i].Item1)
                {
                    triangles[j] = indexes[i].Item2;
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
    }

    private GameObject findAggregatorObject(string name)
    {
        GameObject p = transform.Find(name).gameObject;

        if (p == null)
        {
            p = new GameObject(name);
            p.transform.localPosition = Vector3.zero;
            p.transform.parent = transform;
        }

        return p;
    }

    private float CalculatePath(Animator animator, HumanBodyBones start)
    {
        float length = 0.0f;

        Transform trn = animator.GetBoneTransform(start);
        HumanBodyBones target = HumanBodyBonesWeightPath.GetDestination(start);
        Transform dest = animator.GetBoneTransform(target);

        if (trn == dest || trn == null)
            return 1.0f;

        while (trn != dest)
        {
            if (trn.parent == null)
                throw new UnityException("HumanBodyBones path recursion encountered an object without a parent before reaching destination bone!");

            length += GetDistance(trn, trn.parent);
            trn = trn.parent;
        }

        return length;
    }

    private float GetDistance(Transform a, Transform b)
    {
        return Mathf.Abs((a.position - b.position).magnitude);
    }
}
