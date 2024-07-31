using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode]
public class BSABuilderComponent : MonoBehaviour
{
    public Animator animator;

    public Transform normal;

    public Mesh mesh;

    [SerializeField] private List<Transform> meshes;
    [SerializeField] private List<Transform> normals;
    [SerializeField] private List<Transform> cylinders;

    private void Start()
    {

    }

    [ContextMenu("Instance/Surface Normal")]
    public void CreatePlaneNormalDefinitionObject()
    {
        GameObject obj = new GameObject("Plane Normal");
        obj.transform.position = transform.position;
        obj.transform.rotation *= Quaternion.Euler(90, 0, 0);
        obj.transform.parent = transform;

        obj.AddComponent<SceneVectorDisplay>();
        SceneVectorDisplay component = obj.GetComponent<SceneVectorDisplay>();
        component.scale = 0.1f;

        //if (normals.Empty())
        //    normals = new List<Transform>();

        normals.Add(obj.transform);
        Selection.activeGameObject = obj;
    }

    [ContextMenu("Instance/Mesh Descriptor")]
    public void CreateMeshControllerObject()
    {
        if (!mesh) { Debug.LogError("No Mesh is selected to create Object"); return; }

        Mesh actual = Object.Instantiate(mesh);
        MergeVertices(actual);
        
        GameObject obj = new GameObject(mesh.name);
        obj.transform.position = transform.position;
        obj.transform.parent = transform;

        obj.AddComponent<BSAMeshBuilder>();
        BSAMeshBuilder builder = obj.GetComponent<BSAMeshBuilder>();
        builder.mesh = actual;
        builder.vertices = new List<Transform>(actual.vertexCount);

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
            //normals.RemoveAt(indexes[i].Item1);

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
}
