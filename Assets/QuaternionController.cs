using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaternionController : MonoBehaviour
{
    public Transform arm1_trn1;
    public Transform arm1_trn2;
    public Transform arm1_trn3;
    public Transform arm2_trn1;
    public Transform arm2_trn2;
    public Transform arm2_trn3;

    private Quaternion arm1_init1;
    private Quaternion arm1_init2;
    private Quaternion arm1_init3;
    private Quaternion arm2_init1;
    private Quaternion arm2_init2;
    private Quaternion arm2_init3;

    private Quaternion diff1;
    private Quaternion diff2;
    private Quaternion diff3;

    private Vector3 forward1;
    private Vector3 forward2;

    public Quaternion modify;

    // Start is called before the first frame update
    void Start()
    {
        arm1_init1 = arm1_trn1.localRotation;
        arm1_init2 = arm1_trn2.localRotation;
        arm1_init3 = arm1_trn3.localRotation;
        arm2_init1 = arm2_trn1.localRotation;
        arm2_init2 = arm2_trn2.localRotation;
        arm2_init3 = arm2_trn3.localRotation;

        forward1 = arm1_trn2.forward;
        forward2 = arm2_trn2.forward;

        diff1 = QDifference(arm1_init1, arm2_init1);
        diff2 = QDifference(arm1_init2, arm2_init2);
        diff3 = QDifference(arm1_init3, arm2_init3);
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion m = modify;
        arm1_trn1.localRotation = m * arm1_init1;

        //m = QChangeFrame(arm1_init1, arm2_init1, m);

        arm2_trn1.localRotation = m * diff1;
        arm2_trn2.localRotation = diff2;
        arm2_trn3.localRotation = diff3;
    }

    private Quaternion QDifference(Quaternion q1, Quaternion q2)
    {
        return q2 * Quaternion.Inverse(q1);
        //return Quaternion.FromToRotation(forward1, forward2);
    }

    private Quaternion QChangeFrame(Quaternion f1, Quaternion f2, Quaternion rot)
    {
        Quaternion trn = QDifference(f2, f1);
        return trn * rot;
    }
}
