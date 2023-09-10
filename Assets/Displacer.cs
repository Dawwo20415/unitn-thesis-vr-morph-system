using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Displacer : MonoBehaviour
{
    public GameObject reference_object;
    private Transform ref_transform;
    private Transform own_transform;

    public GameObject[] displacement_references;
    public PointDisplacement[] operations;

    void Awake() {
        ref_transform = reference_object.transform;
        own_transform = gameObject.transform;

        for (int i = 0; i < operations.Length; i++) {
            if (displacement_references[i])
                operations[i].setup(displacement_references[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 displacement = Vector3.zero;
        foreach(PointDisplacement op in operations) {
            displacement += op.getDisplacement(own_transform.position);
        }

        own_transform.position = ref_transform.position + displacement;
    }
}
