using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaternionTest : MonoBehaviour
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

    private Quaternion frame_a1;
    private Quaternion frame_b1; 
    private Quaternion frame_a2;
    private Quaternion frame_b2;

    private Quaternion fromRootA1;
    private Quaternion fromRootB1;
    private Quaternion fromRootA2;
    private Quaternion fromRootB2;

    private Quaternion fromA1toB1;
    private Quaternion fromA2toB2;

    private Quaternion startingA;
    private Quaternion startingB;

    // Start is called before the first frame update
    void Start()
    {
        frame_a1 = QFix(trnA1.rotation);
        frame_a2 = QFix(trnA2.rotation);

        frame_b1 = QFix(trnB1.rotation);
        frame_b2 = QFix(trnB2.rotation);

        startingA = QFix(trnA2.localRotation);
        startingB = QFix(trnB2.localRotation);

        fromRootA1 = QFromTo(QFix(rootA.rotation), frame_a1);
        fromRootA2 = QFromTo(QFix(rootA.rotation), frame_a2);
        Debug.Log("Root->A1 " + QPrintEuler(fromRootA1) + " | Root->A2 " + QPrintEuler(fromRootA2));

        fromRootB1 = QFromTo(QFix(rootB.rotation), frame_b1);
        fromRootB2 = QFromTo(QFix(rootB.rotation), frame_b2);
        Debug.Log("Root->B1 " + QPrintEuler(fromRootB1) + " | Root->B2 " + QPrintEuler(fromRootB2));

        fromA1toB1 = QFromTo(fromRootA1, fromRootB1);
        fromA2toB2 = QFromTo(fromRootA2, fromRootB2);
    }

    // Update is called once per frame
    void Update()
    {
        a1 = QFix(a1);
        a2 = QFix(a2);

        Quaternion b1 = QChangeFrame(a1, fromA1toB1);
        Quaternion b2 = QChangeFrame(a2, fromA2toB2);

        
        trnA1.localRotation = fromRootA1 * a1;
        trnB1.localRotation = fromRootB1 * b1;

        trnA2.localRotation = startingA * a2;
        trnB2.localRotation = startingB * b2;
        
        
        /*
        trnA1.localRotation = QChangeFrame(a1, fromRootA1);
        trnB1.localRotation = QChangeFrame(b1, fromRootB1);

        trnA2.localRotation = QChangeFrame(a2, startingA);
        trnB2.localRotation = QChangeFrame(b2, startingB);
        */
    }

    private Quaternion QChangeFrame (Quaternion q, Quaternion frame)
    {
        return Quaternion.Inverse(frame) * q * frame;
    }

    private Quaternion QFix (Quaternion q)
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
        return "[" + q.x + "," + q.y + "," + q.z + "," + q.w + "]";
    }

    private string QPrintEuler(Quaternion q)
    {
        return "[" + q.eulerAngles.x + "," + q.eulerAngles.y + "," + q.eulerAngles.z + "]";
    }
}


