using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class BodyStructureReference : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float neck_height;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float shoulder_width;

    public GameObject hmd;
    public GameObject shoulder;
    public GameObject right_arm;
    public GameObject left_arm;
    private Transform hmd_origin;
    private Transform shoulder_origin;
    private Transform right_arm_origin;
    private Transform left_arm_origin;

    private Transform cameraTransform;

    private void Awake() {
        hmd_origin = hmd.transform;
        shoulder_origin = shoulder.transform;
        left_arm_origin = left_arm.transform;
        right_arm_origin = right_arm.transform;

        cameraTransform = Camera.main.transform;
    }

    void Start()
    {
        //Position Arms based on the parameters
        Vector3 height = hmd_origin.localPosition - new Vector3(0, neck_height, 0);
        Vector3 shoulder_vec = new Vector3(shoulder_width / 2, 0, 0);
        shoulder_origin.localPosition = height;
        shoulder_origin.localRotation = cameraTransform.localRotation;
        left_arm_origin.localPosition = - shoulder_vec;
        left_arm_origin.localRotation = hmd_origin.localRotation;
        right_arm_origin.localPosition = shoulder_vec;
        right_arm_origin.localRotation = hmd_origin.localRotation;

    }

    // Update is called once per frame
    void Update()
    {
        //shoulder_origin.localRotation = cameraTransform.localRotation;
    }
}
