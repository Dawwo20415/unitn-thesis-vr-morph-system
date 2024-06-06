using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Progress: It works well enough but there needs to be a better handling of the up direction to match more
 * the normal of the plane created by the 3 points origin | target | effector target
*/

public class IKTest : MonoBehaviour
{
    public List<Transform> m_Bones;
    public List<Transform> m_Targets;
    private List<Quaternion> m_Pose;

    public float m_SqrDistError;
    public int m_MaxIterationCount;

    public enum Method
    {
        PrePosition,
        SingleAdjust
    }

    public Method method;

    private void Start()
    {
        m_Pose = new List<Quaternion>();

        foreach (Transform trn in m_Bones)
        {
            m_Pose.Add(trn.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 goal = m_target.GetTarget();
        Vector3 goal = m_Targets[0].position;
        //EE = EndEffector
        Vector3 currentEE = m_Bones[0].position;
        float distance = (currentEE - goal).magnitude;

        for (int i = 1; i < m_Bones.Count; i++)
        {
            //PrintBoneStuff(m_Bones[i], m_Bones[i - 1], currentEE, m_Targets[i - 1].position, goal);
        }

        if (method == Method.PrePosition)
        {
            //If you don't input a specific pose each frame like in an animation this algorithm starts to rotate
            //the bones on themselves because of the circular motion in going from one vector to the other
            SetPose();

            for (int i = m_Bones.Count - 1; i > 0; i--)
            {
                RotateBone(m_Bones[i], m_Bones[i - 1].position, m_Targets[i - 1].position);
            }

            int iterations = 0;
            do
            {
                //Triangular Loop
                for (int i = 1; i < m_Bones.Count; i++)
                {
                    for (int j = i; j >= 1; j--)
                    {
                        RotateBone(m_Bones[j], currentEE, goal);
                        currentEE = m_Bones[0].position;
                        distance = (currentEE - goal).magnitude;

                        if (distance <= m_SqrDistError)
                            return;
                    }
                }
                iterations++;
            } while (distance > m_SqrDistError && iterations < m_MaxIterationCount);
            Debug.Log("Arrived at 10 iterations", this);
        }
    }

    private void PrintBoneStuff(Transform bone, Transform nextBone, Vector3 effector, Vector3 boneGoal, Vector3 eeGoal)
    {
        //Effector
        Debug.DrawLine(boneGoal, boneGoal + new Vector3(0.0f,0.2f,0.0f), Color.yellow, Time.deltaTime, false);

        //Bone to Target
        Vector3 boneToTarget = boneGoal - bone.position;
        Debug.DrawLine(bone.position, bone.position + boneToTarget, Color.white, Time.deltaTime, false);

        //Bone to EE
        Vector3 boneToEE = eeGoal - bone.position;
        Debug.DrawLine(bone.position, bone.position + boneToEE, Color.blue, Time.deltaTime, false);
    }

    private void RotateSingleBone(Transform bone, Vector3 current, Vector3 goal)
    {
        Vector3 bonePosition = bone.position;
        Quaternion boneRotation = bone.rotation;

        Vector3 boneToEffector = current - bonePosition;
        Vector3 boneToEEGoal = goal - bonePosition;

        Quaternion fromToRotation = Quaternion.FromToRotation(boneToEffector, boneToEEGoal);
        bone.rotation = fromToRotation * boneRotation;
    }

    private void RotateBone(Transform bone, Vector3 effector, Vector3 eeGoal)
    {
        Vector3 bonePosition = bone.position;
        Quaternion boneRotation = bone.rotation;

        Vector3 boneToEffector = effector - bonePosition;
        Vector3 boneToEEGoal = eeGoal - bonePosition;

        Quaternion fromToRotation = Quaternion.FromToRotation(boneToEffector, boneToEEGoal);
        bone.rotation = fromToRotation * boneRotation;
    }

    private void SetPose()
    {
        for (int i = 0; i < m_Bones.Count; i++)
        {
            m_Bones[i].rotation = m_Pose[i];
        }
    }

    private void RotateAdjustBone(Transform bone, Transform nextBone, Vector3 effector, Vector3 boneGoal, Vector3 eeGoal)
    {
        Vector3 bonePosition = bone.position;
        Quaternion boneRotation = bone.rotation;

        Vector3 boneToEffector = effector - bonePosition;
        Vector3 boneToEEGoal = eeGoal - bonePosition;

        Quaternion fromToRotation = Quaternion.FromToRotation(boneToEffector, boneToEEGoal);

        //Adjustment
        Vector3 boneToGoal = boneGoal - bonePosition;
        Vector3 boneToNext = nextBone.position - bonePosition;

        //Debug.DrawLine(bonePosition, bonePosition + boneToGoal, Color.blue, Time.deltaTime, false);
        //Debug.DrawLine(bonePosition, bonePosition + boneToNext, Color.red, Time.deltaTime, false);

        Vector3 nGoal = Vector3.Cross(boneToGoal, bonePosition);
        Vector3 nCurrent = Vector3.Cross(boneToNext, bonePosition);

        //Debug.DrawLine(boneGoal, boneGoal + (nGoal.normalized / 3), Color.blue, Time.deltaTime, false);
        //Debug.DrawLine(boneGoal, boneGoal + (nCurrent.normalized / 3), Color.red, Time.deltaTime, false);

        Quaternion boneTargetAdjust = Quaternion.AngleAxis(-Vector3.Angle(boneToNext, boneToGoal), boneToEEGoal);

        bone.rotation = fromToRotation * boneRotation;
    }
}
