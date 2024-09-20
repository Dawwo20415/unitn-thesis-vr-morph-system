//#define DEBUG_HERE
#define DEBUG_DRAW_GIZMO
#define NEW_FOLLOW

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BSAComponent : MonoBehaviour
{
    public BodySurfaceApproximationDefinition BSAD;

    private BodySurfaceApproximationRuntime m_BSAR;
    private List<BSACoordinates> m_Coordinates;
    private Animator m_animator;

    private List<Vector3> m_DebugList;
    private List<Vector3> m_DebugListMesh;

    private void Start()
    {
        m_DebugList = new List<Vector3>();
        m_DebugListMesh = new List<Vector3>();

        m_animator = GetComponent<Animator>();
        m_Coordinates = new List<BSACoordinates>(BSAD.coordinateSpan);
        m_BSAR = new BodySurfaceApproximationRuntime(BSAD);
        BuildBSA();
    }

#if DEBUG_HERE
    private void Update()
    {
        Vector3 initial_position = m_animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
        List<BSACoordinates> coord = Project(HumanBodyBones.LeftHand);
        Vector3 result = ReverseProject(HumanBodyBones.LeftHand, coord);

        Debug.Log("Initial Position: " + initial_position + " | Result: " + result);
    }
#endif

    private void BuildBSA()
    {
        BuildCustomMeshes();
    }

    private List<Transform> HBBToTrn(List<HumanBodyBones> anchors)
    {
        List<Transform> result = new List<Transform>(anchors.Count);

        foreach (HumanBodyBones hbb in anchors)
        {
            result.Add(m_animator.GetBoneTransform(hbb));
        }

        return result;
    }

    private void BuildCustomMeshes()
    {
        GameObject collection = new GameObject("BSA Meshes");
        collection.transform.parent = transform;
        collection.transform.localPosition = Vector3.zero;
        collection.transform.localRotation = Quaternion.identity;

        foreach (BSACustomMesh bsad_mesh in BSAD.meshes)
        {
            GameObject mesh_obj = new GameObject(bsad_mesh.name);

#if DEBUG_DRAW_GIZMO
            DebugDrawMesh comp = mesh_obj.AddComponent<DebugDrawMesh>();

            List<Transform> anchors = new List<Transform>(bsad_mesh.anchors.Count);
            foreach (HumanBodyBones hbb in bsad_mesh.anchors)
            {
                anchors.Add(m_animator.GetBoneTransform(hbb));
            }

            comp.Setup(bsad_mesh, anchors);
#endif

#if NEW_FOLLOW
            LazyMeshBoneWeights component = mesh_obj.AddComponent<LazyMeshBoneWeights>();
            component.Calibrate(HBBToTrn(bsad_mesh.anchors), bsad_mesh.offset, bsad_mesh.rot_offset);
#else
            ObjectBoneFollow component = mesh_obj.AddComponent<ObjectBoneFollow>();
            if (HBBToTrn(bsad_mesh.anchors) == null) { Debug.LogError("HBBTOTrn failing"); }
            component.calibrate(HBBToTrn(bsad_mesh.anchors), bsad_mesh.offset, bsad_mesh.rot_offset, Vector3.one);
#endif

            mesh_obj.transform.parent = collection.transform;

            m_BSAR.meshes.Add(mesh_obj.transform);
        }
    }

    public List<BSACoordinates> Project(HumanBodyBones hbb)
    {
        m_Coordinates.Clear();
        int counter = 0;

        Vector3 position = m_animator.GetBoneTransform(hbb).position;
        float proportional_weight = BSAD.body_proportion_weights[(int)hbb];
        float total_weight_sum = 0.0f;

        //PROJECT ON EACH MESH FACE
        int previous_id = 0;
        Transform trn = m_BSAR.meshes[0].transform;
        foreach (Triangle tris in BSAD.meshTris())
        {
            if (tris.id != previous_id)
            {
                trn = m_BSAR.meshes[tris.id].transform;
                previous_id = tris.id;
            }

            {
                m_DebugList.Add(position);
            }

            Vector3 p1 = trn.TransformPoint(tris.a);
            Vector3 p2 = trn.TransformPoint(tris.b);
            Vector3 p3 = trn.TransformPoint(tris.c);

            Vector3 debug_tmp;
            BSACoordinates bsac = BSAProjectionOperators.MeshRaycast(p1, p2, p3, position, proportional_weight, out debug_tmp, ref m_DebugListMesh);
            m_Coordinates.Add(bsac);
            total_weight_sum += bsac.weight;

            {
                m_DebugList.Add(debug_tmp);
            }

            counter++;
        }

        //PROJECT ON EACH CYLINDER
        Vector3 anchor_position = m_animator.GetBoneTransform(HumanBodyBones.Hips).transform.position;
        foreach (BSACylinder cyl in BSAD.cylindersDef())
        {
            Vector3 a = m_animator.GetBoneTransform(cyl.start).position;
            Vector3 b = m_animator.GetBoneTransform(cyl.end).position;

            BSACoordinates bsac = BSAProjectionOperators.CylinderRaycast(a, b, cyl.radius, position, proportional_weight, anchor_position);
            m_Coordinates.Add(bsac);
            total_weight_sum += bsac.weight;
            counter++;
        }

        //NORMALIZE WEIGHTS
        for (int i = 0; i < m_Coordinates.Count; i++)
        {
            BSACoordinates tmp = m_Coordinates[i];
            tmp.weight = tmp.weight / total_weight_sum;
            m_Coordinates[i] = tmp;
        }

        Debug.Log("Direct|[Counter/ListCount](" + counter + "/" + m_Coordinates.Count + ")");

        return m_Coordinates;
    }

    public List<BSACoordinates> Project(HumanBodyBones hbb, ref List<Vector3> debug_proj, ref List<Vector3> debug_onMesh)
    {
        m_DebugList.Clear();
        m_DebugListMesh.Clear();

        List<BSACoordinates> tmp = Project(hbb);
        debug_proj = m_DebugList;
        debug_onMesh = m_DebugListMesh;
        return tmp;
    }

    public Vector3 ReverseProject(HumanBodyBones hbb, List<BSACoordinates> coord)
    {
        Vector3 weighted_sum = Vector3.zero;
        float proportional_weight = BSAD.body_proportion_weights[(int)hbb];
        float weight = 0.0f;
        int t = 0;

        //MESH REVERSAL
        int previous_id = 0;
        Transform trn = m_BSAR.meshes[0].transform;
        foreach (Triangle tris in BSAD.meshTris())
        {
            Vector3 pos; float w;
            
            if (tris.id != previous_id)
            {
                trn = m_BSAR.meshes[tris.id].transform;
                previous_id = tris.id;
            }

            Vector3 p1 = trn.TransformPoint(tris.a);
            Vector3 p2 = trn.TransformPoint(tris.b);
            Vector3 p3 = trn.TransformPoint(tris.c);

            (pos, w) = BSAProjectionOperators.MeshReversal(p1, p2, p3, coord[t], proportional_weight);
            weighted_sum += pos;
            weight += w;
            t++;
        }

        //CYLINDERS REVERSAL
        Vector3 anchor_position = m_animator.GetBoneTransform(HumanBodyBones.Hips).transform.position;
        foreach (BSACylinder cyl in BSAD.cylindersDef())
        {
            Vector3 pos; float w;

            Vector3 a = m_animator.GetBoneTransform(cyl.start).position;
            Vector3 b = m_animator.GetBoneTransform(cyl.end).position;

            (pos, w) = BSAProjectionOperators.CylinderReversal(a, b, cyl.radius, coord[t], proportional_weight, anchor_position);

            weighted_sum += pos;
            weight += w;
            t++;
        }

        Debug.Log("Reverse|[Counter/ListCount](" + t + "/" + coord.Count + ")");

        return weighted_sum / t;
    }

}
