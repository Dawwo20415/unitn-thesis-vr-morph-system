using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingParentChildPoint : MonoBehaviour
{
    public Transform parent;
    public Transform child;

    public Transform parent_cube;
    public Transform child_cube;

    public Quaternion q;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        parent_cube.position = parent.position;
        parent_cube.rotation = parent.rotation;
        child_cube.localPosition = FrameChildToParent(parent.position, parent.rotation, child.position);
        child_cube.localRotation = child.localRotation;

        Debug.Log("base: " + VExtension.Print(child.localPosition) + " rotated " + VExtension.Print(FrameChildToParent(parent.position, parent.rotation, child.position)));
    }

    private Vector3 Scale(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    private Vector3 FrameChildToParent(Vector3 pPosition, Quaternion pRotation, Vector3 cPosition)
    {
        return Quaternion.Inverse(pRotation) * (cPosition - pPosition);
    }
}
