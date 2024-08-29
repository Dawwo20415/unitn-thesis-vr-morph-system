using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarBSAProjection : MonoBehaviour
{
    public BodySurfaceApproximationDefinition BSA_def;
    public Material mat;
    public Animator animator;
    public Mesh cyMesh;

    public AvatarBSAProjection()
    {
        //Store Body Surface Approximation
    }

    private void Awake()
    {
        GameObject collection = new GameObject("BSA Meshes");
        collection.transform.parent = transform;
        collection.transform.localPosition = Vector3.zero;
        collection.transform.localRotation = Quaternion.identity;

        foreach (BSACustomMesh bsa_mesh in BSA_def.meshes)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = bsa_mesh.vertices;
            mesh.triangles = bsa_mesh.triangles;

            GameObject mesh_obj = new GameObject(bsa_mesh.name);
            MeshFilter mesh_filter = mesh_obj.AddComponent<MeshFilter>();
            MeshRenderer mesh_renderer = mesh_obj.AddComponent<MeshRenderer>();

            mesh_filter.mesh = mesh;
            mesh_renderer.material = mat;

            mesh_obj.transform.parent = collection.transform;
            mesh_obj.transform.localPosition = Vector3.zero;
            mesh_obj.transform.localRotation = Quaternion.identity;
        }
    }

    private void JointToBSAProjection() { }

    private void BSAToJointCalculation(List<BSACoordinates> coordinates)
    {

    }

    // DEBUG

    private void OnDrawGizmos()
    {
        DrawCustomMeshes();
        DrawCylinders();
    }

    private void DrawCustomMeshes()
    {
        
    }

    private void DrawCylinders()
    {
        foreach (BSACylinder cylinder in BSA_def.cylinders)
        {
            Transform beginning = animator.GetBoneTransform(cylinder.start);
            Transform end = animator.GetBoneTransform(cylinder.end);
            Gizmos.color = Color.white;
            Gizmos.DrawRay(beginning.position, (end.position - beginning.position));
            Gizmos.color = Color.yellow;
        }
    }
}
