using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateHandOffset : MonoBehaviour
{
    public HumanBodyBones hbb;

    public ExtremitiesPlaneData data;

    public Transform reference;

    [ContextMenu("Calculate")]
    public void Calc()
    {
        data.bone = hbb;
        data.position_offset = transform.position - reference.position;
        data.rotation_offset = transform.rotation * Quaternion.Inverse(reference.rotation);
        data.scale = transform.localScale;
    }
}
