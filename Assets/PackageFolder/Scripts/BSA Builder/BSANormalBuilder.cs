using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BSANormalBuilder : MonoBehaviour
{
    [Range(0.01f, 1.0f)] public float scale = 1.0f;
    public Transform anchor;
    public BSANormal bsa_normal { get { return GetBSANormal(); } }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;

        DrawGizmoArrow(transform.position, transform.forward * scale);
    }

    private void DrawGizmoArrow(Vector3 position, Vector3 direction, float angle = 25.0f, float length = 0.25f)
    {
        Gizmos.DrawRay(position, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + angle, 0) * Vector3.forward * scale;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - angle, 0) * Vector3.forward * scale;

        Gizmos.DrawRay(position + direction, right * length);
        Gizmos.DrawRay(position + direction, left * length);
    }

    private BSANormal GetBSANormal()
    {
        BSANormal tmp = new BSANormal();

        tmp.name = this.name;
        tmp.pos_offset = transform.position - anchor.position;
        tmp.rot_offset = Quaternion.Inverse(anchor.rotation) * transform.rotation;

        return tmp;
    }
}
