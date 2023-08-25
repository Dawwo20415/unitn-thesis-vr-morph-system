using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmDescriptor : MonoBehaviour
{
    public enum Direction {Left = 0, Right = 1};

    public Direction direction;

    public GameObject arm;
    private Transform arm_trn;
    private Quaternion arm_base;
    public GameObject forearm;
    private Transform forearm_trn;
    private Quaternion forearm_base;
    public GameObject hand;
    private Transform hand_trn;
    private Quaternion hand_base;

    public float arm_length;
    public float forearm_length;
    [HideInInspector]
    public float total_length;

    void Awake()
    {
        arm_trn = arm.transform;
        arm_base = arm.transform.rotation;
        forearm_trn = forearm.transform;
        forearm_base = forearm.transform.rotation;
        hand_trn = hand.transform;
        hand_base = hand.transform.rotation;

        total_length = arm_length + forearm_length;
    }

    public void SetArmRotation(Quaternion new_rotation) {
        arm_trn.rotation = new_rotation;
    }

    public void SetForearmRotation(Quaternion new_rotation) {
        forearm_trn.localRotation = forearm_base * new_rotation;
    }
}
