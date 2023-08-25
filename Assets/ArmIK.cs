using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ArmIK : MonoBehaviour
{

    [SerializeField]
    private bool debug;

    public GameObject target;
    private Transform target_trn;
    private Transform obj_origin;
    private ArmDescriptor descriptor;

    void Awake () {
        target_trn = target.transform;
        obj_origin = gameObject.transform;
        descriptor = gameObject.GetComponent<ArmDescriptor>();

        if (descriptor == null) {
            Debug.LogError("GameObject doesn't have \"Arm Descriptor\" component.");
        }
    }

    Vector3 CalcDirection() {
        return (target_trn.position - obj_origin.position).normalized;
    }

    float CalcAngle() {
        float targetDistance = (target_trn.localPosition - obj_origin.position).magnitude;

        if ( targetDistance > descriptor.total_length) { return 0.0f; };
        
        return 180 - (Mathf.Acos(Mathf.Clamp((Mathf.Pow(descriptor.arm_length, 2f) + Mathf.Pow(descriptor.forearm_length, 2f) -
                                        Mathf.Pow(targetDistance, 2f)) / (2f * descriptor.arm_length * descriptor.forearm_length), -1f, 1f)) * Mathf.Rad2Deg);
    }

    void ApplyIK() {
        
        Quaternion elbow_angle = Quaternion.identity;
        Quaternion direction = Quaternion.identity;
        //Apply Direction
        direction = Quaternion.LookRotation(CalcDirection(), Vector3.up);

        //Apply Angles
        float angle = CalcAngle();
        angle = descriptor.direction == 0 ? angle : -angle;
        direction *= Quaternion.Euler(0, -angle / 2, 0);
        elbow_angle *= Quaternion.Euler(0, (angle) + 90, 0);
        

        descriptor.SetArmRotation(direction);
        descriptor.SetForearmRotation(elbow_angle);

    }

    // Update is called once per frame
    void Update() {
        if (debug) {
            Debug.DrawLine(obj_origin.position, obj_origin.position + (CalcDirection() * descriptor.total_length));
        }

        ApplyIK();
    }
}
