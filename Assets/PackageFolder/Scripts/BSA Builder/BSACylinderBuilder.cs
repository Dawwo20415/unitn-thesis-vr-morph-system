using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BSACylinderBuilder : MonoBehaviour
{
    [HideInInspector]
    public Transform beginning;
    [HideInInspector]
    public Transform end;
    public BSACylinder cylinder;
    [HideInInspector]
    public Mesh cyMesh;

    private Vector3 position;
    private Quaternion rotation;
    private float length;

    // Start is called before the first frame update
    void Start()
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        length = (beginning.position - end.position).magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        length = (beginning.position - end.position).magnitude;
        position = beginning.position + ((end.position - beginning.position) / 2);
        rotation = Quaternion.LookRotation(end.position - beginning.position) * Quaternion.Euler(90, 0, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawRay(beginning.position, (end.position - beginning.position));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireMesh(cyMesh, position, QExtension.Fix(rotation), new Vector3(cylinder.radius, length / 2, cylinder.radius));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.92f, 0.016f, 0.4f);
        Gizmos.DrawMesh(cyMesh, position, QExtension.Fix(rotation), new Vector3(cylinder.radius, length / 2, cylinder.radius));
    }
}
