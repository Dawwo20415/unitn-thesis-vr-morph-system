using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AvatarRetargetingComponents
{
    public Quaternion localA;
    public Quaternion localB;
    public Quaternion fromAtoB;

    public AvatarRetargetingComponents(Quaternion q1, Quaternion q2, Quaternion q3)
    {
        localA = q1;
        localB = q2;
        fromAtoB = q3;
    }

    public AvatarRetargetingComponents(Quaternion q)
    {
        localA = q;
        localB = q;
        fromAtoB = q;
    }

    public static AvatarRetargetingComponents identity { get => new AvatarRetargetingComponents(Quaternion.identity); }
}

public struct VExtension
{
    public static Vector3 FrameChildToParent(Vector3 pPosition, Quaternion pRotation, Vector3 cPosition)
    {
        return Quaternion.Inverse(pRotation) * (cPosition - pPosition);
    }

    public static string Print(Vector3 vec)
    {
        return "[" + vec.x + "," + vec.y + "," + vec.z + "]";
    }
}

public struct QExtension
{
    public static Quaternion ChangeFrame(Quaternion q, Quaternion frame)
    {
        return Quaternion.Inverse(frame) * q * frame;
    }

    public static Quaternion Fix(Quaternion q)
    {
        if (q.eulerAngles == Vector3.zero) { return Quaternion.identity; }
        else { return q; }
    }

    public static Quaternion FromTo(Quaternion from, Quaternion to)
    {
        return Quaternion.Inverse(from) * to;
    }

    public static Quaternion Difference(Quaternion from, Quaternion to)
    {
        return to * Quaternion.Inverse(from);
    }

    public static string Print(Quaternion q)
    {
        return "(q)[" + q.x + "," + q.y + "," + q.z + "," + q.w + "]";
    }

    public static string PrintEuler(Quaternion q)
    {
        return "(q)[" + q.eulerAngles.x + "," + q.eulerAngles.y + "," + q.eulerAngles.z + "]";
    }

    public static Quaternion StackToParent(Transform obj, Transform root, bool include_root)
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
