using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaternionTest2 : MonoBehaviour
{
    public Quaternion a1;
    public Quaternion a2;

    [Space]
    public Transform rootA;
    public Transform trnA1;
    public Transform trnA2;

    [Space]
    public Transform rootB;
    public Transform trnB1;
    public Transform trnB2;

    private Quaternion fromRootA1;
    private Quaternion fromRootB1;
    private Quaternion fromRootA2;
    private Quaternion fromRootB2;

    private Quaternion fromA1toB1;
    private Quaternion fromA2toB2;

    private Quaternion localA1;
    private Quaternion localA2;
    private Quaternion localB1;
    private Quaternion localB2;

    // Start is called before the first frame update
    void Start()
    {
        localA1 = QFix(trnA1.localRotation);
        localA2 = QFix(trnA2.localRotation);
        localB1 = QFix(trnB1.localRotation);
        localB2 = QFix(trnB2.localRotation);

        fromRootA1 = QStackToParent(trnA1, rootA, false);
        fromRootA2 = QStackToParent(trnA2, rootA, false);

        fromRootB1 = QStackToParent(trnB1, rootB, false);
        fromRootB2 = QStackToParent(trnB2, rootB, false);

        fromA1toB1 = QFromTo(fromRootA1, fromRootB1);
        fromA2toB2 = QFromTo(fromRootA2, fromRootB2);

        a1 = localA1;
        a2 = localA2;
    }

    // Update is called once per frame
    void Update()
    {
        a1 = QFix(a1);
        a2 = QFix(a2);

        Quaternion b1 = QChangeFrame(Quaternion.Inverse(localA1) * a1, fromA1toB1);
        Quaternion b2 = QChangeFrame(Quaternion.Inverse(localA2) * a2, fromA2toB2);

        trnA1.localRotation = a1;
        trnB1.localRotation = localB1 * b1;

        trnA2.localRotation = a2;
        trnB2.localRotation = localB2 * b2;

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

    private string QPrint(Quaternion q)
    {
        return "(q)[" + q.x + "," + q.y + "," + q.z + "," + q.w + "]";
    }

    private string QPrintEuler(Quaternion q)
    {
        return "(q)[" + q.eulerAngles.x + "," + q.eulerAngles.y + "," + q.eulerAngles.z + "]";
    }

    private Quaternion QStackToParent(Transform obj, Transform root, bool include_root)
    {
        Transform destination = include_root ? root.parent : root;
        Quaternion diff = Quaternion.identity;

        do
        {
            diff = obj.localRotation * diff;
            obj = obj.parent;
        } while (obj != destination);

        return diff;
    }
}
