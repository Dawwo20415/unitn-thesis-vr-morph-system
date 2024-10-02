using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalAlignmentComponent : MonoBehaviour
{
    private List<Transform> m_Bones;
    private List<BSANormal> m_Definitions;
    private BSAComponent m_BSAC;

    [SerializeField] private float m_distanceTreshold = 0.5f;
    [SerializeField] [Range(0.01f, 1.0f)] private float m_scale = 0.25f;

    public void Init(Animator animator, BodySurfaceApproximationDefinition BSAD, BSAComponent BSAC)
    {
        m_Bones = new List<Transform>(BSAD.normals.Count);
        m_Definitions = new List<BSANormal>(BSAD.normals.Count);

        m_BSAC = BSAC;

        foreach (BSANormal normal in BSAD.normals)
        {
            m_Bones.Add(animator.GetBoneTransform(normal.anchor));
            m_Definitions.Add(normal);
        }
    }

    public void RotateNormals()
    {
        for (int i = 0; i < m_Bones.Count; i++)
        {
            (Vector3 normal, float distance) = m_BSAC.CastForCloasest(m_Definitions[i].anchor);
            Vector3 bone_direction = m_Bones[i].rotation * m_Definitions[i].rot_offset * Vector3.forward;
            m_Bones[i].rotation = m_Bones[i].rotation * ComputeNormal(bone_direction, normal, distance);
        }
    }

    private Quaternion ComputeNormal(Vector3 direction, Vector3 normal, float distance)
    {
        Quaternion q = Quaternion.identity;
        if (distance <= m_distanceTreshold)
        {
            float angle = Vector3.Dot(direction, normal);
            Vector3 axis = Vector3.Cross(direction, normal);
            q = Quaternion.AngleAxis(Mathf.LerpAngle(0.0f, angle, distance / m_distanceTreshold), axis);
        }

        return q;
    }

    private void DrawGizmoArrow(Vector3 position, Vector3 direction, float angle = 25.0f, float length = 0.25f)
    {
        Gizmos.DrawRay(position, direction.normalized * m_scale);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + angle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - angle, 0) * Vector3.forward;

        Gizmos.DrawRay(position + direction.normalized * m_scale, right * length * m_scale);
        Gizmos.DrawRay(position + direction.normalized * m_scale, left * length * m_scale);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < m_Definitions.Count; i++)
        {
            Vector3 direction = m_Bones[i].rotation * m_Definitions[i].rot_offset * Vector3.forward;
            Vector3 position = m_Bones[i].position + (m_Bones[i].rotation * m_Definitions[i].pos_offset);
            DrawGizmoArrow(position, direction);
        }
    }
}
