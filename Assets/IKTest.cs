using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKTest : MonoBehaviour
{
    public List<Transform> m_Bones;
    public List<Transform> m_Targets;

    public float m_SqrDistError;
    public int m_MaxIterationCount;

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
            PrintBoneStuff(m_Bones[i], m_Bones[i - 1], currentEE, m_Targets[i - 1].position, goal);
        }

        int iterations = 0;
        do
        {
            //Triangular Loop
            for (int i = 1; i < m_Bones.Count; i++)
            {
                for (int j = i; j >= 1; j--)
                {
                    if (j == 7)
                    {
                        RotateAdjustBone(m_Bones[j], m_Bones[j - 1], currentEE, m_Targets[j - 1].position, goal);
                    }
                    else
                    {
                        RotateBone(m_Bones[j], currentEE, goal);
                    }
                    currentEE = m_Bones[0].position;
                    distance = (currentEE - goal).magnitude;

                    if (distance <= m_SqrDistError)
                        return;
                }
            }
            iterations++;
        } while (distance > m_SqrDistError && iterations < m_MaxIterationCount);
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

    private void RotateBone(Transform bone, Vector3 effector, Vector3 eeGoal)
    {
        Vector3 bonePosition = bone.position;
        Quaternion boneRotation = bone.rotation;

        Vector3 boneToEffector = effector - bonePosition;
        Vector3 boneToEEGoal = eeGoal - bonePosition;

        Quaternion fromToRotation = Quaternion.FromToRotation(boneToEffector, boneToEEGoal);
        bone.rotation = fromToRotation * boneRotation;
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
