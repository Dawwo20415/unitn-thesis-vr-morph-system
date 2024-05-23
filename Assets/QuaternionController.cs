using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaternionController : MonoBehaviour
{

    public Transform armA_trn1;
    public Transform armA_trn2;
    public Transform armA_trn3;
    public Transform armB_trn1;
    public Transform armB_trn2;
    public Transform armB_trn3;

    private Quaternion armA_init1;
    private Quaternion armA_init2;
    private Quaternion armA_init3;
    private Quaternion armB_init1;
    private Quaternion armB_init2;
    private Quaternion armB_init3;

    private Quaternion fromAtoB_1;
    private Quaternion fromAtoB_2;
    private Quaternion fromAtoB_3;

    public Quaternion a1;
    public Quaternion a2;
    public Quaternion a3;

    // Start is called before the first frame update
    void Start()
    {
        armA_init1 = QFix(armA_trn1.localRotation);
        armA_init2 = QFix(armA_trn2.localRotation);
        armA_init3 = QFix(armA_trn3.localRotation);
        armB_init1 = QFix(armB_trn1.localRotation);
        armB_init2 = QFix(armB_trn2.localRotation);
        armB_init3 = QFix(armB_trn3.localRotation);

        fromAtoB_1 = QFromTo(armA_init1, armB_init1);
        fromAtoB_2 = QFromTo(armA_init2, armB_init2);
        fromAtoB_3 = QFromTo(armA_init3, armB_init3);
    }

    // Update is called once per frame
    void Update()
    {
        a1 = QFix(a1);
        a2 = QFix(a2);
        a3 = QFix(a3);

        Quaternion b1 = QChangeFrame(a1, fromAtoB_1);
        Quaternion b2 = QChangeFrame(a2, fromAtoB_2);
        Quaternion b3 = QChangeFrame(a3, fromAtoB_3);

        armA_trn1.localRotation = armA_init1 * a1;
        armA_trn2.localRotation = armA_init2 * a2;
        armA_trn3.localRotation = armA_init3 * a3;

        armB_trn1.localRotation = armB_init1 * b1;
        armB_trn2.localRotation = armB_init2 * b2;
        armB_trn3.localRotation = armB_init3 * b3;
    }

    private string QPrint(Quaternion q)
    {
        return "[" + q.x + "," + q.y + "," + q.z + "," + q.w + "]"; 
    }

    private Quaternion QChangeFrame(Quaternion q, Quaternion frame)
    {
        return Quaternion.Inverse(frame) * q * frame;
    }

    private Quaternion QFix(Quaternion q)
    {
        if (q.eulerAngles == Vector3.zero) { return Quaternion.identity; }
        else { return q; }
    }

    private Quaternion QFromTo(Quaternion from, Quaternion to)
    {
        return Quaternion.Inverse(from) * to;
    }

    private Quaternion QDifference(Quaternion from, Quaternion to)
    {
        return to * Quaternion.Inverse(from);
    }
}
